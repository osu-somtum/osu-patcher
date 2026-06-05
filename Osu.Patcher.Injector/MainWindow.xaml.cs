using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace Osu.Patcher.Injector;

[SupportedOSPlatform("windows")]
public partial class MainWindow : Window
{
    private static readonly Brush ErrorBrush = new SolidColorBrush(Color.FromRgb(0xF2, 0x64, 0x5A));
    private static readonly Brush OkBrush = new SolidColorBrush(Color.FromRgb(0x7E, 0xD3, 0x21));
    private static readonly Brush NeutralBrush = new SolidColorBrush(Color.FromRgb(0xB0, 0xB0, 0xB0));

    private readonly Config _config = Config.Load();
    private bool _busy;

    public MainWindow()
    {
        InitializeComponent();
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        if (_busy) return;

        var osu = InjectorCore.FindRunningOsu();
        LocateLink.Visibility = Visibility.Collapsed;

        switch (osu.State)
        {
            case InjectorCore.OsuState.RunningValid:
                SetStatus("osu! is running — ready to inject", OkBrush);
                break;
            case InjectorCore.OsuState.RunningInvalid:
                SetStatus($"osu! is running but not on -devserver {InjectorCore.DevServer}.\nClose it first.", ErrorBrush);
                break;
            case InjectorCore.OsuState.NotRunning when KnownOsuPath() is not null:
                SetStatus("osu! is not running — Inject will launch it", NeutralBrush);
                break;
            default:
                SetStatus("osu! could not be found\nStart the game or locate it", ErrorBrush);
                LocateLink.Visibility = Visibility.Visible;
                break;
        }
    }

    private string? KnownOsuPath() =>
        !string.IsNullOrEmpty(_config.OsuPath) && File.Exists(_config.OsuPath) ? _config.OsuPath : null;

    private async void OnInject(object sender, RoutedEventArgs e)
    {
        if (_busy) return;

        try
        {
            SetBusy(true);
            await Task.Run(() => RunInjectFlow(CancellationToken.None));

            SetStatus("Injected successfully! Closing…", OkBrush);
            SetBusy(false);
            await Task.Delay(TimeSpan.FromSeconds(3));
            Close();
        }
        catch (OperationCanceledException)
        {
            SetBusy(false);
            RefreshStatus();
        }
        catch (Exception ex)
        {
            SetBusy(false);
            SetStatus(ex.Message, ErrorBrush);
        }
    }

    /// <summary>
    ///     The full inject decision tree:
    ///     1. osu! already running (valid)   -> remember its path, inject.
    ///     2. osu! not running, path known   -> launch it, wait, inject.
    ///     3. osu! not running, path unknown -> ask for the path, save, launch, inject.
    /// </summary>
    private void RunInjectFlow(CancellationToken token)
    {
        var osu = InjectorCore.FindRunningOsu();

        switch (osu.State)
        {
            case InjectorCore.OsuState.RunningValid:
                if (!string.IsNullOrEmpty(osu.ExecutablePath))
                {
                    _config.OsuPath = osu.ExecutablePath;
                    _config.Save();
                }

                WaitThenInject(osu.Pid, token);
                return;

            case InjectorCore.OsuState.RunningInvalid:
                throw new Exception($"osu! is running but not on -devserver {InjectorCore.DevServer}. Close it first.");
        }

        // Not running: need a path to launch from.
        var path = KnownOsuPath() ?? AskForOsuPathOnUiThread();
        if (path is null) throw new OperationCanceledException();

        _config.OsuPath = path;
        _config.Save();

        Report("Launching osu!…");
        var proc = InjectorCore.LaunchOsu(path);

        Report("Waiting for osu! to start…");
        var pid = InjectorCore.WaitForInjectable(proc, token, TimeSpan.FromSeconds(60));

        WaitThenInject(pid, token);
    }

    /// <summary>
    ///     Wait a few seconds (with a visible countdown) to let osu! fully settle before
    ///     injecting — this noticeably improves the injection success rate.
    /// </summary>
    private void WaitThenInject(uint pid, CancellationToken token)
    {
        for (var seconds = 5; seconds > 0; seconds--)
        {
            token.ThrowIfCancellationRequested();
            Report($"Injecting in {seconds}…");
            Thread.Sleep(1000);
        }

        Report("Injecting…");
        InjectorCore.Inject(pid);
    }

    private string? AskForOsuPathOnUiThread()
    {
        return Dispatcher.Invoke(() =>
        {
            var dialog = new OpenFileDialog
            {
                Title = "Locate osu!.exe",
                Filter = "osu! executable (osu!.exe)|osu!.exe|Executables (*.exe)|*.exe",
                FileName = "osu!.exe",
            };
            return dialog.ShowDialog(this) == true ? dialog.FileName : null;
        });
    }

    private void OnLocate(object sender, RoutedEventArgs e)
    {
        var path = AskForOsuPathOnUiThread();
        if (path is null) return;

        _config.OsuPath = path;
        _config.Save();
        RefreshStatus();
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        InjectButton.IsEnabled = !busy;
    }

    private void Report(string message) => Dispatcher.Invoke(() => SetStatus(message, NeutralBrush));

    private void SetStatus(string message, Brush brush)
    {
        StatusText.Text = message;
        StatusText.Foreground = brush;
    }

    private void OnWindowDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
