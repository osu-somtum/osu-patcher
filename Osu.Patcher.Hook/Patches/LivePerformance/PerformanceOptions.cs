using System.Collections.Generic;
using JetBrains.Annotations;
using Osu.Stubs.GameModes.Options;
using Osu.Stubs.Wrappers;

namespace Osu.Patcher.Hook.Patches.LivePerformance;

[UsedImplicitly]
internal class PerformanceOptions : PatchOptions
{
    public override IEnumerable<object> CreateOptions() =>
    [
        CreateInGameDisplayOption(),
    ];

    public override void Load(Settings config)
    {
        ShowPerformanceInGame.Value = config.ShowPerformanceInGame;
    }

    public override void Save(Settings config)
    {
        config.ShowPerformanceInGame = ShowPerformanceInGame.Value;
    }

    #region Options Creation

    private static object CreateInGameDisplayOption() => OptionCheckbox.Constructor.Invoke([
        /* title: */ "Show PP during gameplay",
        /* tooltip: */ "A small PP counter display will be visible below the accuracy display.",
        /* binding: */ ShowPerformanceInGame.Bindable,
        /* onChange: */ null,
    ]);

    #endregion

    #region Bindables

    public static readonly BindableWrapper<bool> ShowPerformanceInGame =
        new(BindableType.Bool, false, Settings.Default.ShowPerformanceInGame);

    #endregion
}