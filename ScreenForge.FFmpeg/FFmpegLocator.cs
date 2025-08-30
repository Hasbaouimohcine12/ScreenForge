using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ScreenForge.FFmpeg;

public static class FFmpegLocator
{
  public static string Locate()
  {
    if (IsOnPath("ffmpeg")) return "ffmpeg";

    var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";
    var rid = GetRid();
    var local = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", exe);
    if (File.Exists(local)) return local;

    throw new FileNotFoundException("FFmpeg não encontrado no PATH nem no bundle local.");
  }

  static bool IsOnPath(string cmd)
  {
    try
    {
      var p = Process.Start(new ProcessStartInfo
      {
        FileName = cmd,
        Arguments = "-version",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      });
      p?.WaitForExit(1500);
      return p?.ExitCode == 0;
    }
    catch { return false; }
  }

  static string GetRid()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "win-x64";
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
    return "linux-x64";
  }
}
