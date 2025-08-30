using ScreenForge.Core;

namespace ScreenForge.App.Models;

public class MonitorItem
{
  public string Id { get; init; } = "";
  public string Label { get; init; } = "";
  public CaptureRegion Region { get; init; } = new(true, 0, 0, 0, 0, null);
  public override string ToString() => Label;
}
