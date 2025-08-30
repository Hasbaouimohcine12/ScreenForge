using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ScreenForge.App.Models;
using ScreenForge.App.Services;
using ScreenForge.Core;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace ScreenForge.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ScreenRecorder _rec = new();

    [ObservableProperty] private bool _isRecording;
    [ObservableProperty] private string _status = "Pronto";
    [ObservableProperty] private string _saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    public ObservableCollection<MonitorItem> Monitors { get; } = new();
    [ObservableProperty] private MonitorItem? _selectedMonitor;

    private Window? _window;

    public void Attach(Window window)
    {
        _window = window;
        LoadSettings();
        RefreshMonitors();
        ShowOverlayPreview(1000);
    }

    private void LoadSettings()
    {
        var s = SettingsService.Load();
        if (!string.IsNullOrWhiteSpace(s.SaveFolder) && Directory.Exists(s.SaveFolder))
            SaveFolder = s.SaveFolder!;
    }

    private void PersistSettings() => SettingsService.Save(new AppSettings { SaveFolder = SaveFolder });

    [RelayCommand]
    private void RefreshMonitors()
    {
        if (_window is null) return;

        Monitors.Clear();
        foreach (var m in DisplayService.GetMonitors(_window))
            Monitors.Add(m);

        SelectedMonitor ??= Monitors.Count > 0 ? Monitors[0] : null;
    }

    [RelayCommand]
    private async Task ChooseFolder()
    {
        if (_window is null) return;

        var picked = await _window.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { AllowMultiple = false });

        if (picked is { Count: > 0 })
        {
            var path = picked[0].Path.LocalPath;
            if (!string.IsNullOrWhiteSpace(path))
            {
                SaveFolder = path;
                PersistSettings();
            }
        }
    }

    [RelayCommand]
    private async Task ToggleRecord()
    {
        if (IsRecording)
        {
            Status = "Finalizando...";
            await _rec.StopAsync();
            IsRecording = false;
            OverlayService.Hide();

            if (_window is not null)
            {
                _window.WindowState = WindowState.Normal;
                _window.Activate();
            }

            Status = "Parado";
            return;
        }

        try
        {
            var target = Path.Combine(SaveFolder, $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
            var region = SelectedMonitor?.Region ?? new CaptureRegion(true, 0, 0, 0, 0, null);

            IsRecording = true;
            Status = $"Gravando em: {target}";

            if (_window is not null)
                _window.WindowState = WindowState.Minimized;

            await _rec.StartAsync(region, target, audioDevice: null);
        }
        catch (Exception ex)
        {
            IsRecording = false;
            OverlayService.Hide();

            if (_window is not null)
            {
                _window.WindowState = WindowState.Normal;
                _window.Activate();
            }

            Status = "Erro: " + ex.Message;
        }
    }

    partial void OnSelectedMonitorChanged(MonitorItem? value)
        => ShowOverlayPreview(1200);

    private void ShowOverlayPreview(int autoHideMs)
    {
        if (_window is null || SelectedMonitor is null) return;
        var rect = OverlayService.RectFor(_window, SelectedMonitor.Region);
        OverlayService.Show(_window, rect, border: 3, autoHideMs);
    }

    public async Task SafeShutdownAsync()
    {
        try
        {
            if (IsRecording)
                await _rec.StopAsync();
        }
        catch
        {
            _rec.ForceKill();
        }
        finally
        {
            OverlayService.Close();
        }
    }
}
