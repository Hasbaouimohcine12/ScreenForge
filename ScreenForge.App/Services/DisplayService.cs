using System.Collections.Generic;
using Avalonia.Controls;
using ScreenForge.App.Models;
using ScreenForge.Core;

namespace ScreenForge.App.Services;

public static class DisplayService
{
  public static List<MonitorItem> GetMonitors(Window top)
  {
    var list = new List<MonitorItem>();
    var screens = top.Screens;

    // Opção "todos"
    list.Add(new MonitorItem
    {
      Id = "all",
      Label = "Todos os monitores (desktop virtual)",
      Region = new CaptureRegion(true, 0, 0, 0, 0, null)
    });

    int idx = 0;
    if (screens?.All is { Count: > 0 })
    {
      foreach (var s in screens.All)
      {
        var b = s.Bounds;
        list.Add(new MonitorItem
        {
          Id = $"m{idx}",
          Label = $"Monitor {idx + 1} — {b.Width}x{b.Height} @ ({b.X},{b.Y})" + (s.IsPrimary ? " [Primário]" : ""),
          Region = new CaptureRegion(false, b.X, b.Y, b.Width, b.Height, idx)
        });
        idx++;
      }
    }

    return list;
  }
}
