using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Osu.Stubs.GameplayElements.Scoring;
using Osu.Utils.Extensions;
using Osu.Utils.IL;
using OsuMods = Osu.Stubs.Root.Mods;

namespace Osu.Patcher.Hook.Patches.Relax;

/// <summary>
///     Changes the following code in <c>osu.GameModes.Play.Player:CheckFailed()</c>
///     to enable failing while playing Relax*.
///     <br /><br />
///     From:
///     <code><![CDATA[
///         if ((mods & Mods.NoFail) <= Mods.None && !Player.Relaxing && !Player.Relaxing2 && ...)
///     ]]></code>
///     To:
///     <code><![CDATA[
///         if ((mods & Mods.NoFail) <= Mods.None && ...)
///     ]]></code>
///     <br /><br />
///     A prefix is also applied to skip <c>CheckFailed</c> entirely when the option is disabled
///     and the player is using Relax/Autopilot, restoring the original no-fail-on-relax behavior.
/// </summary>
[OsuPatch]
[HarmonyPatch]
[UsedImplicitly]
internal static class AllowRelaxFailing
{
    // #=zeXZ7VnmadWamDozl0oXkDPqWT5QR:#=zwMd5KYaUmGit
    private static readonly OpCode[] Signature =
    [
        OpCodes.And,
        OpCodes.Ldc_I4_0,
        OpCodes.Cgt,
        OpCodes.Brtrue_S,
        OpCodes.Ldsfld, // ----------
        OpCodes.Brtrue_S, // No-oped (4 inst)
        OpCodes.Ldsfld,
        OpCodes.Brtrue_S, // ---------
    ];

    [UsedImplicitly]
    [HarmonyTargetMethod]
    private static MethodBase Target() => OpCodeMatcher.FindMethodBySignature(null, Signature)!;

    [UsedImplicitly]
    [HarmonyPrefix]
    private static bool Before()
    {
        if (RelaxOptions.AllowFailing.Value)
            return true;

        var activeMods = (int)ModManager.ModStatus.Get(null)!;
        var isRelax = (activeMods & (OsuMods.Relax | OsuMods.Relax2)) > 0;

        return !isRelax;
    }

    [UsedImplicitly]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        instructions = instructions.NoopAfterSignature(
            Signature.Take(Signature.Length - 4).ToArray(),
            4
        );

        return instructions;
    }
}