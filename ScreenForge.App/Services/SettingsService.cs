using System;
using System.IO;
using System.Text.Json;

namespace ScreenForge.App.Services;

public class AppSettings
{
  public string? SaveFolder { get; set; }
}

public static class SettingsService
{
  static string Dir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScreenForge");
  static string File => Path.Combine(Dir, "settings.json");

  public static AppSettings Load()
  {
    try
    {
      if (System.IO.File.Exists(File))
        return JsonSerializer.Deserialize<AppSettings>(System.IO.File.ReadAllText(File)) ?? new();
    }
    catch { /* ignore */ }
    return new AppSettings();
  }

  public static void Save(AppSettings s)
  {
    Directory.CreateDirectory(Dir);
    var json = JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true });
    System.IO.File.WriteAllText(File, json);
  }
}
