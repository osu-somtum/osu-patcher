using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using HoLLy.ManagedInjector;

namespace Osu.Patcher.Injector;

/// <summary>
///     The <c>-devserver</c> osu! must be launched with for injection to be allowed.
///     Injecting into a Bancho (<c>ppy.sh</c>) connection is intentionally impossible.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class InjectorCore
{
    public const string DevServer = "freedomdive.dev";

    private const string HookResourceName = "osu!.hook.dll";
    private const string HookTypeName = "Osu.Patcher.Hook.Hook";
    private const string HookMethodName = "Initialize";

    public enum OsuState
    {
        /// <summary>No osu!.exe process is running.</summary>
        NotRunning,

        /// <summary>osu! is running and was launched with the required dev server.</summary>
        RunningValid,

        /// <summary>osu! is running but connected to Bancho / wrong dev server. Refuse to inject.</summary>
        RunningInvalid,
    }

    public readonly record struct OsuProcess(OsuState State, uint Pid, string? ExecutablePath);

    /// <summary>
    ///     Look for a running <c>osu!.exe</c> and classify whether it is safe to inject into.
    /// </summary>
    public static OsuProcess FindRunningOsu()
    {
        using var mgmt = new ManagementClass("Win32_Process");
        using var processes = mgmt.GetInstances();

        foreach (var process in processes)
        {
            using (process)
            {
                if ((string)process["Name"] != "osu!.exe") continue;

                var pid = (uint)process["ProcessId"];
                var path = process["ExecutablePath"] as string;
                var cli = (process["CommandLine"] as string) ?? "";

                var args = cli.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var valid = args is [_, "-devserver", { Length: > 3 }] && args is not [_, "-devserver", "ppy.sh"];

                return new OsuProcess(valid ? OsuState.RunningValid : OsuState.RunningInvalid, pid, path);
            }
        }

        return new OsuProcess(OsuState.NotRunning, 0, null);
    }

    /// <summary>
    ///     Launch <c>osu!.exe</c> from the given path with the required <c>-devserver</c> argument.
    /// </summary>
    /// <returns>The started process.</returns>
    public static Process LaunchOsu(string osuExePath)
    {
        var dir = Path.GetDirectoryName(osuExePath)!;
        var psi = new ProcessStartInfo
        {
            FileName = osuExePath,
            WorkingDirectory = dir,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("-devserver");
        psi.ArgumentList.Add(DevServer);

        return Process.Start(psi) ?? throw new Exception("Failed to start osu!.");
    }

    /// <summary>
    ///     Wait until a freshly launched osu! process is ready to receive an injection
    ///     (its main window has been created), or until the timeout elapses.
    /// </summary>
    public static uint WaitForInjectable(Process started, CancellationToken token, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            token.ThrowIfCancellationRequested();

            // osu! is single-instance: a second launch hands off to the existing process and exits.
            // In that case, fall back to discovering the real running process.
            if (started.HasExited)
            {
                var running = FindRunningOsu();
                if (running.State == OsuState.RunningValid) return running.Pid;
                if (running.State == OsuState.RunningInvalid)
                    throw new Exception($"osu! is already running but not on -devserver {DevServer}.");
                throw new Exception("osu! exited before it could be injected.");
            }

            started.Refresh();
            if (started.MainWindowHandle != IntPtr.Zero)
            {
                // Give the CLR a moment to finish spinning up before injecting.
                Thread.Sleep(1500);
                return (uint)started.Id;
            }

            Thread.Sleep(200);
        }

        throw new Exception("Timed out waiting for osu! to start.");
    }

    /// <summary>
    ///     Inject the patcher hook into the given process id.
    /// </summary>
    public static void Inject(uint pid)
    {
        var hookPath = ExtractHook();
        using var proc = new InjectableProcess(pid);
        proc.Inject(hookPath, HookTypeName, HookMethodName);
    }

    /// <summary>
    ///     Extract the embedded hook and ALL of its runtime dependencies (managed DLLs +
    ///     the native rosu.ffi.dll) side-by-side into <c>%AppData%\osu-patcher\</c>, so that
    ///     dependency resolution works exactly like the original co-located folder layout.
    /// </summary>
    /// <returns>The full path to the extracted hook DLL.</returns>
    private static string ExtractHook()
    {
        var dir = Config.Directory;
        Directory.CreateDirectory(dir);

        var asm = Assembly.GetExecutingAssembly();
        foreach (var name in asm.GetManifestResourceNames())
        {
            // Our payload resources are the embedded *.dll files; skip WPF's .g.resources etc.
            if (!name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) continue;

            var dest = Path.Combine(dir, name);
            using var stream = asm.GetManifestResourceStream(name)!;
            try
            {
                using var file = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.Read);
                stream.CopyTo(file);
            }
            catch (IOException)
            {
                // File is locked because a previously injected osu! still has it loaded.
                // The on-disk copy is already correct, so leave it as-is.
            }
        }

        var hookPath = Path.Combine(dir, HookResourceName);
        if (!File.Exists(hookPath))
            throw new Exception($"Hook payload '{HookResourceName}' was not embedded in the build.");

        return hookPath;
    }
}
