using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Osu.Performance;

[StructLayout(LayoutKind.Sequential)]
public struct OsuDifficultyAttributes
{
    public double Stars;
    public uint MaxCombo;
    public double SpeedNoteCount;

    public double ApproachRate;
    public double HealthRate;

    public double AimSkill;
    public double SpeedSkill;
    public double FlashlightSkill;
    public double SliderSkill;

    public double AimDifficultStrainCount;
    public double SpeedDifficultStrainCount;

    public uint CircleCount;
    public uint SliderCount;
    public uint SpinnerCount;
}

[StructLayout(LayoutKind.Sequential)]
public struct OsuScoreState
{
    public uint ScoreMaxCombo;
    public uint Score300s;
    public uint Score100s;
    public uint Score50s;
    public uint ScoreMisses;
}

[StructLayout(LayoutKind.Sequential)]
public struct OsuPerformanceInfo
{
    public double TotalPP;
    public double AimPP;
    public double SpeedPP;
    public double AccuracyPP;
    public double FlashlightPP;
    public double EffectiveMissCount;
}

[StructLayout(LayoutKind.Sequential)]
public struct OsuGradualResult
{
    public double Pp;
    public double PpAim;
    public double PpSpeed;
    public double PpAccuracy;
    public double PpFlashlight;
    public double EffectiveMissCount;
    public double AimDifficultStrainCount;
    public double SpeedDifficultStrainCount;
    public double DiffAim;
    public uint Misses;
    public uint N300;
    public uint N100;
    public uint N50;

    public double CalculateAccuracy()
    {
        var totalHits = N300 + N100 + N50 + Misses;
        if (totalHits == 0) return 100.0;
        return (double)(N300 * 300 + N100 * 100 + N50 * 50) / (totalHits * 300) * 100.0;
    }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum OsuJudgement : byte
{
    None = 0,
    Result300 = 1,
    Result100 = 2,
    Result50 = 3,
    ResultMiss = 4,
}
