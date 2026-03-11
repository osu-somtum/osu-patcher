using System;
using Osu.Performance;
using Osu.Stubs.Graphics.Sprites;

namespace Osu.Patcher.Hook.Patches.LivePerformance;

internal static class PerformanceDisplay
{
    private const int ModRelax = 1 << 7;
    private const int ModAutopilot = 1 << 13;

    /// <summary>
    ///     The last known patched instance of our <c>pSpriteText</c> performance counter sprite.
    /// </summary>
    private static readonly WeakReference<object?> PerformanceCounter = new(null);

    /// <summary>
    ///     Set a new active performance counter to update.
    /// </summary>
    /// <param name="sprite">The <c>pSpriteText</c> performance counter sprite.</param>
    public static void SetPerformanceCounter(object sprite) =>
        PerformanceCounter.SetTarget(sprite);

    /// <summary>
    ///     Receives the full gradual result, applies Sunrise recalculation if needed,
    ///     and updates the performance counter sprite.
    /// </summary>
    public static void UpdatePerformanceCounter(OsuGradualResult result)
    {
        try
        {
            if (!PerformanceCounter.TryGetTarget(out var sprite) || sprite == null)
                return;

            var pp = ResolvePp(result);

            // Technically this should be run with "GameBase.Scheduler.AddOnce(() => ...)" but it works anyways, so...
            pText.SetText.Invoke(sprite, [$"{pp:00.0}pp"]);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to set performance counter sprite text: {e}");
        }
    }

    private static double ResolvePp(OsuGradualResult result)
    {
        var calcType = PerformanceOptions.PerformanceType.Value;
        var mods = PerformanceCalculator.OriginalMods;

        return calcType switch
        {
            PerformanceCalculatorType.Sunrise => SunrisePerformance.Recalculate(result, mods),
            PerformanceCalculatorType.SunriseLimited when (mods & (ModRelax | ModAutopilot)) != 0 =>
                SunrisePerformance.Recalculate(result, mods),
            _ => result.Pp,
        };
    }
}
