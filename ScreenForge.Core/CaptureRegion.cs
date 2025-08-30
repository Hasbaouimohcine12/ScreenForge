namespace ScreenForge.Core;

public record CaptureRegion(
    bool IsVirtualDesktop,
    int X, int Y,
    int Width, int Height,
    int? MacScreenIndex = null
);
