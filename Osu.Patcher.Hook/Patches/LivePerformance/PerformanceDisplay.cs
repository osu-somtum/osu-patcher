using System;
using Osu.Performance;
using Osu.Stubs.Graphics.Sprites;

namespace Osu.Patcher.Hook.Patches.LivePerformance;

internal static class PerformanceDisplay
{
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
    ///     Receives the full gradual result and updates the performance counter sprite.
    /// </summary>
    public static void UpdatePerformanceCounter(OsuGradualResult result)
    {
        try
        {
            if (!PerformanceCounter.TryGetTarget(out var sprite) || sprite == null)
                return;

            // Technically this should be run with "GameBase.Scheduler.AddOnce(() => ...)" but it works anyways, so...
            pText.SetText.Invoke(sprite, [$"{result.Pp:00.0}pp"]);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to set performance counter sprite text: {e}");
        }
    }
}
