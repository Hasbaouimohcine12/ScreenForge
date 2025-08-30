using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ScreenForge.FFmpeg;

namespace ScreenForge.Core;

public class ScreenRecorder
{
  private Process? _proc;
  public bool IsRecording => _proc is { HasExited: false };

  public async Task StartAsync(CaptureRegion region, string outputPath, string? audioDevice = null)
  {
    if (IsRecording) throw new InvalidOperationException("Já está gravando.");

    var ffmpeg = FFmpegLocator.Locate();
    var args = BuildArgs(ffmpeg, region, outputPath, audioDevice); // ✅ versão síncrona

    var psi = new ProcessStartInfo
    {
      FileName = ffmpeg,
      Arguments = args,
      RedirectStandardError = true,
      RedirectStandardInput = true,
      UseShellExecute = false,
      CreateNoWindow = true,
      StandardErrorEncoding = Encoding.UTF8
    };

    _proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
    _proc.ErrorDataReceived += (_, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data))
        Console.WriteLine(e.Data);
    };

    if (!_proc.Start())
      throw new Exception("Não foi possível iniciar o FFmpeg.");

    _proc.BeginErrorReadLine();
  }

  public async Task StopAsync()
  {
    var p = _proc;
    _proc = null;
    if (p is null) return;

    try
    {
      if (!p.HasExited)
      {

        try
        {
          if (!p.StandardInput.BaseStream.CanWrite) { /* ignora */ }
          else { p.StandardInput.WriteLine("q"); p.StandardInput.Flush(); }
        }
        catch { /* pipe pode já ter fechado */ }


        try { p.CancelErrorRead(); } catch { /* se não iniciou, ignora */ }
        try { p.StandardInput.Close(); } catch { }


        var exited = await Task.Run(() => p.WaitForExit(2000));
        if (!exited)
        {
          try { p.Kill(entireProcessTree: true); } catch { /* já pode ter saído */ }
          p.WaitForExit(1500);
        }
      }
    }
    finally
    {
      try { p.Dispose(); } catch { }
    }
  }


  public void ForceKill()
  {
    try { _proc?.Kill(entireProcessTree: true); } catch { }
    try { _proc?.Dispose(); } catch { }
    _proc = null;
  }

  // ----------------- helpers -----------------

  private static string BuildArgs(string ffmpeg, CaptureRegion r, string outPath, string? audioDevice)
  {
    var audioPart = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? (!string.IsNullOrWhiteSpace(audioDevice) ? $"-f dshow -i audio=\"{audioDevice}\" " : "")
        : (!string.IsNullOrWhiteSpace(audioDevice) ? $"-f pulse -i \"{audioDevice}\" " : "");

    string vEncoder =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? PickWinEncoder(ffmpeg) :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "h264_videotoolbox -b:v 6000k" :
                                                              "libx264 -preset veryfast -crf 23";

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {

      var regionPart = r.IsVirtualDesktop
          ? "-f gdigrab -framerate 30 -i desktop"
          : $"-f gdigrab -framerate 30 -offset_x {r.X} -offset_y {r.Y} -video_size {r.Width}x{r.Height} -i desktop";

      return $"-y {regionPart} {audioPart}-c:v {vEncoder} -c:a aac -b:a 160k \"{outPath}\"";
    }

    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      var idx = r.MacScreenIndex ?? 0;
      return $"-y -f avfoundation -framerate 30 -i {idx}:0 {audioPart}-c:v {vEncoder} -c:a aac -b:a 160k \"{outPath}\"";
    }

    var linuxInput = r.IsVirtualDesktop
        ? "-f x11grab -framerate 30 -i :0.0"
        : $"-f x11grab -framerate 30 -video_size {r.Width}x{r.Height} -i :0.0+{r.X},{r.Y}";

    return $"-y {linuxInput} {audioPart}-c:v {vEncoder} -c:a aac -b:a 160k \"{outPath}\"";
  }

  private static string PickWinEncoder(string ffmpeg)
  {
    bool Has(string name)
    {
      try
      {
        using var p = new Process
        {
          StartInfo = new ProcessStartInfo
          {
            FileName = ffmpeg,
            Arguments = "-hide_banner -encoders",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
          }
        };
        p.Start();
        var text = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return text.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0;
      }
      catch { return false; }
    }

    if (Has("libx264")) return "libx264 -preset veryfast -crf 23";
    if (Has("h264_mf")) return "h264_mf -b:v 6000k";
    return "mpeg4 -q:v 5";
  }
}
