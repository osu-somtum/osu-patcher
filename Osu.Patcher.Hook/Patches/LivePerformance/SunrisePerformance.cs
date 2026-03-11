using System;
using Osu.Performance;

namespace Osu.Patcher.Hook.Patches.LivePerformance;

/// <summary>
///     Applies Sunrise-specific PP recalculations for Relax and Autopilot mods
///     on top of the standard Bancho performance calculation from rosu-pp.
/// </summary>
internal static class SunrisePerformance
{
    private const int ModRelax = 1 << 7;
    private const int ModAutopilot = 1 << 13;
    private const int ModHardRock = 1 << 4;

    /// <summary>
    ///     Applies Sunrise recalculation to the gradual result if RX or AP mods are active.
    ///     Returns the final PP value.
    /// </summary>
    public static double Recalculate(OsuGradualResult result, uint mods)
    {
        if ((mods & ModRelax) != 0)
            return RecalculateRelaxStd(result, result.CalculateAccuracy(), mods);

        if ((mods & ModAutopilot) != 0)
            return RecalculateAutopilotStd(result);

        return result.Pp;
    }

    private static double RecalculateRelaxStd(OsuGradualResult result, double accuracy, uint mods)
    {
        var multi = CalculateStdPpMultiplier(result);
        var streamsNerf = CalculateStreamsNerf(result);

        double accDepression = 1;
        var ppAim = result.PpAim;

        if (streamsNerf < 1.09)
        {
            var accFactor = (100 - accuracy) / 100;
            accDepression = Math.Max(0.86 - accFactor, 0.5);

            if (accDepression > 0.0)
                ppAim *= accDepression;
        }

        if ((mods & ModHardRock) != 0)
            multi *= Math.Min(2, Math.Max(1, 1 * (CalculateMissPenalty(result) / 1.85)));

        var relaxPp = Math.Pow(
            Math.Pow(ppAim, 1.15) +
            Math.Pow(result.PpSpeed, 0.65 * accDepression) +
            Math.Pow(result.PpAccuracy, 1.1) +
            Math.Pow(result.PpFlashlight, 1.13),
            1.0 / 1.1
        ) * multi;

        return double.IsNaN(relaxPp) ? 0.0 : relaxPp;
    }

    private static double RecalculateAutopilotStd(OsuGradualResult result)
    {
        var multi = CalculateStdPpMultiplier(result);

        var autopilotPp = Math.Pow(
            Math.Pow(result.PpAim, 0.6) +
            Math.Pow(result.PpSpeed, 1.3) +
            Math.Pow(result.PpAccuracy, 1.05) +
            Math.Pow(result.PpFlashlight, 1.13),
            1.0 / 1.1
        ) * multi;

        return double.IsNaN(autopilotPp) ? 0.0 : autopilotPp;
    }

    private static double CalculateMissPenalty(OsuGradualResult result)
    {
        var missCount = result.Misses;
        var diffStrainCount = result.DiffAim;

        if (diffStrainCount <= 0)
            return 0;

        var logValue = Math.Log(diffStrainCount);
        var denominatorPart = 4.0 * Math.Pow(logValue, 0.94);

        if (double.IsNaN(denominatorPart) || double.IsInfinity(denominatorPart))
            return 0;

        return 2.0 / (missCount / denominatorPart + 1.0);
    }

    private static double CalculateStreamsNerf(OsuGradualResult result)
    {
        if (result.SpeedDifficultStrainCount <= 0)
            return double.MaxValue;

        return Math.Round(result.AimDifficultStrainCount / result.SpeedDifficultStrainCount * 100) / 100;
    }

    /// <summary>
    ///     Extracts the multiplier that rosu-pp applied on top of the raw component sum,
    ///     so we can reuse it when recombining components with different weights.
    /// </summary>
    private static double CalculateStdPpMultiplier(OsuGradualResult result)
    {
        var sum = Math.Pow(
            Math.Pow(result.PpAim, 1.1) +
            Math.Pow(result.PpSpeed, 1.1) +
            Math.Pow(result.PpAccuracy, 1.1) +
            Math.Pow(result.PpFlashlight, 1.1),
            1.0 / 1.1
        );

        if (sum <= 0) return 0;
        return result.Pp / sum;
    }
}
