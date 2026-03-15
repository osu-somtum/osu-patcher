<p align="center">
  <img align="center" width="400" alt="osu! logo" src=".github/assets/logo.png">
</p>

# osu! patcher (Sunrise Community Fork)

> This is a fork of [rushiiMachine/osu-patcher](https://github.com/rushiiMachine/osu-patcher), maintained by the [Sunrise Community](https://github.com/SunriseCommunity).
> All credit for the original project goes to [rushiiMachine](https://github.com/rushiiMachine).

Apply several fixes to osu! make playing Relax more enjoyable.

This is for use in offline play or private servers that allow modifications **only**.
Use at your own risk! Modifications are disallowed on most servers, even though this project this
_does not provide an unfair advantage_.

Using this on official Bancho servers WILL get you banned.

## Features

### Relax

- Show misses on hit objects while playing Relax
- Re-enable combobreak sounds with Relax enabled
- Save all Relax scores to local leaderboards automatically
- Allow failing with Relax enabled (toggleable)
- Re-enable the low hp glow (with shaders on)

<sup>Note: Relax refers to Relax _or_ Autopilot</sup>

### PP

- Show a live pp counter during gameplay and replays
- Switch between Bancho and Sunrise pp calculators

### Mods

- Always faintly show active mods during gameplay
- Auto restart when failed due to Sudden Death mod

### UI

- Increase the thumbnail opacity in song select
- Other miscellaneous fixes

<!-- ### Other -->
<!-- - Download from beatmap mirrors when offline -->

## Usage

Go to the [latest actions build](https://github.com/SunriseCommunity/osu-patcher/actions?query=branch%3Amaster)
for the `master` branch and download the attached artifact to extract. No automatic updater is included.

Only the `Stable` release stream is officially supported! `Cutting Edge` and `Beta` release streams
may have changes that cause errors or crashes. No support will be provided.

Latest tested `Stable` version: [`20260101.12`](https://osu.ppy.sh/home/changelog/stable40/20260101.1).

Your antivirus may detect it as malware, however this is completely expected as it contains code to inject
into processes. If you aren't convinced it isn't a false positive, feel free to build from source code.

## Compiling

1. Install the .NET SDK 8, the .NET Framework 4.5.2 developer pack, and Rust (rustup/cargo).
2. Run `dotnet build Osu.Patcher.Injector -c Release`
3. Output will be located in `./Osu.Patcher.Injector/bin/Release/net8.0/`

## How

This uses [ManagedInjector](https://github.com/holly-hacker/ManagedInjector) to inject a .NET DLL into an osu! process, which uses [Harmony](https://github.com/pardeike/Harmony) to hook methods/
rewrite IL instructions. To find obfuscated methods, "signatures" based on a portion of the IL instructions from the
target method are used to locate it, and then patch it. Since this method doesn't rely on neither the Eazfuscator
obfuscation key nor the obfuscated names, it should work on any version with matching IL even if the method names change.

## Is this okay?

This was initially made after the Akatsuki private server's patcher broke for multiple months and no alternative
existed to fix the issues listed above. This project does not and never intends to bypass the
anti-cheat built into osu! (to allow modifications), and for that reason this project is only usable when osu! is
launched with a custom `-devserver` (for offline play, something like `-devserver example.com`).

This is not a cheat and never will be.

## License

This project contains code originally released under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html) by [rushiiMachine](https://github.com/rushiiMachine). See [LICENSE](LICENSE) for details.
