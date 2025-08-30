using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace ScreenForge.App.Services;

public static class OverlayService
{
  private static Window? _overlay;
  private static DispatcherTimer? _timer;

  public static void Show(Window owner, PixelRect rect, int border = 3, int autoHideMs = 0)
  {
    if (_overlay is null)
    {
      _overlay = new Window
      {
        SystemDecorations = SystemDecorations.None,
        ShowInTaskbar = false,
        CanResize = false,
        Topmost = true,
        Background = Brushes.Transparent,
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
      };

      _overlay.Content = new Border
      {
        BorderBrush = Brushes.Red,
        BorderThickness = new Thickness(border),
        Background = Brushes.Transparent,
        IsHitTestVisible = false
      };
    }

    _overlay.Width = rect.Width;
    _overlay.Height = rect.Height;
    _overlay.Position = new PixelPoint(rect.X, rect.Y);

    if (!_overlay.IsVisible)
      _overlay.Show(owner);

    if (autoHideMs > 0)
    {
      _timer?.Stop();
      _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(autoHideMs) };
      _timer.Tick += (_, __) => { _timer!.Stop(); Hide(); };
      _timer.Start();
    }
  }

  public static void Hide()
  {
    if (_overlay?.IsVisible == true)
      _overlay.Hide();
  }

  public static PixelRect RectFor(Window w, Core.CaptureRegion r)
  {
    if (r.IsVirtualDesktop)
    {
      var screens = w.Screens?.All;
      if (screens != null && screens.Count > 0)
      {
        var u = screens[0].Bounds;
        for (int i = 1; i < screens.Count; i++)
          u = u.Union(screens[i].Bounds);
        return u;
      }
    }
    return new PixelRect(r.X, r.Y, r.Width, r.Height);
  }

  public static void Close()
  {
    try { _timer?.Stop(); } catch { }
    _timer = null;

    if (_overlay is not null)
    {
      try { _overlay.Close(); } catch { }
      _overlay = null;
    }
  }

}
