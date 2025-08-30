using Avalonia.Controls;
using Avalonia.Markup.Xaml;          // ✅ necessário para AvaloniaXamlLoader
using Avalonia.Threading;
using ScreenForge.App.ViewModels;

namespace ScreenForge.App.Views;

public partial class MainWindow : Window
{
    private bool _isShuttingDown;

    public MainWindow()
    {
        InitializeComponent();

        this.Opened += (_, __) =>
        {
            if (DataContext is MainWindowViewModel vm)
                vm.Attach(this);
        };

        this.Closing += (_, e) =>
        {
            if (_isShuttingDown) return;
            e.Cancel = true;
            _isShuttingDown = true;

            _ = Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try
                {
                    if (DataContext is MainWindowViewModel vm)
                        await vm.SafeShutdownAsync();
                }
                finally
                {
                    Close();
                }
            });
        };
    }


    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
