using System.Collections.Generic;
using JetBrains.Annotations;
using Osu.Stubs.GameModes.Options;
using Osu.Stubs.Graphics;
using Osu.Stubs.Wrappers;
using Osu.Utils.Extensions;
using static Osu.Patcher.Hook.Patches.CustomStrings.CustomStrings;

namespace Osu.Patcher.Hook.Patches.LivePerformance;

[UsedImplicitly]
internal class PerformanceOptions : PatchOptions
{
    private static readonly OptionDropdown OptionDropdownStub = new(typeof(PerformanceCalculatorType));

    public override IEnumerable<object> CreateOptions() =>
    [
        CreateInGameDisplayOption(),
        CreatePerformanceTypeOption(),
    ];

    public override void Load(Settings config)
    {
        ShowPerformanceInGame.Value = config.ShowPerformanceInGame;
        PerformanceType.Value = config.PerformanceCalculator;
    }

    public override void Save(Settings config)
    {
        config.ShowPerformanceInGame = ShowPerformanceInGame.Value;
        config.PerformanceCalculator = PerformanceType.Value;
    }

    #region Options Creation

    private static object CreateInGameDisplayOption() => OptionCheckbox.Constructor.Invoke([
        /* title: */ "Show PP during gameplay",
        /* tooltip: */ "A small PP counter display will be visible below the accuracy display.",
        /* binding: */ ShowPerformanceInGame.Bindable,
        /* onChange: */ null,
    ]);

    private static object CreatePerformanceTypeOption()
    {
        var dropdownOptions = new[]
        {
            pDropdownItem.Constructor.Invoke(["Bancho", PerformanceCalculatorType.Bancho]),
            pDropdownItem.Constructor.Invoke(["Sunrise-based", PerformanceCalculatorType.Sunrise]),
            pDropdownItem.Constructor.Invoke([
                "Sunrise-based if RX/AP else Bancho",
                PerformanceCalculatorType.SunriseLimited,
            ]),
        }.ToType(pDropdownItem.Class.Reference);

        return OptionDropdownStub.Constructor.Invoke([
            /* title: */ AddOsuString("PatcherPerformance", "PP calculator"),
            /* items: */ dropdownOptions,
            /* bindable: */ PerformanceType.Bindable,
            /* onChange: */ null,
        ]);
    }

    #endregion

    #region Bindables

    public static readonly BindableWrapper<bool> ShowPerformanceInGame =
        new(BindableType.Bool, false, Settings.Default.ShowPerformanceInGame);

    public static readonly BindableWrapper<PerformanceCalculatorType> PerformanceType = new(
        BindableType.Object,
        Settings.Default.PerformanceCalculator,
        Settings.Default.PerformanceCalculator
    );

    #endregion
}