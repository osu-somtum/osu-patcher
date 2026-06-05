using System;
using System.IO;
using System.Text.Json;

namespace Osu.Patcher.Injector;

/// <summary>
///     Small persisted settings stored under <c>%AppData%\osu-patcher\config.json</c>.
/// </summary>
internal sealed class Config
{
    public static readonly string Directory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu-patcher");

    private static readonly string FilePath = Path.Combine(Directory, "config.json");

    /// <summary>Last known full path to <c>osu!.exe</c>.</summary>
    public string? OsuPath { get; set; }

    public static Config Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<Config>(File.ReadAllText(FilePath)) ?? new Config();
        }
        catch
        {
            // Corrupt or unreadable config: start fresh rather than crashing the launcher.
        }

        return new Config();
    }

    public void Save()
    {
        System.IO.Directory.CreateDirectory(Directory);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
