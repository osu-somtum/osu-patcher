using System.Collections.Generic;
using JetBrains.Annotations;
using Osu.Stubs.GameModes.Options;
using Osu.Stubs.Wrappers;

namespace Osu.Patcher.Hook.Patches.Relax;

[UsedImplicitly]
internal class RelaxOptions : PatchOptions
{
    public static readonly BindableWrapper<bool> AllowFailing =
        new(BindableType.Bool, false, Settings.Default.AllowRelaxFailing);

    public override IEnumerable<object> CreateOptions() =>
    [
        OptionCheckbox.Constructor.Invoke([
            /* title: */ "Allow failing with Relax enabled",
            /* tooltip: */ "Allows you to fail during gameplay while using Relax or Autopilot mods.",
            /* binding: */ AllowFailing.Bindable,
            /* onChange: */ null,
        ]),
    ];

    public override void Load(Settings config) => AllowFailing.Value = config.AllowRelaxFailing;
    public override void Save(Settings config) => config.AllowRelaxFailing = AllowFailing.Value;
}
