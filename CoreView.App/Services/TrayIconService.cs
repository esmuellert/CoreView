using System.Windows;
using CoreView.App.Icons;
using CoreView.Core.Interfaces;
using Application = System.Windows.Application;

namespace CoreView.App.Services;

/// <summary>
/// Service to manage the system tray icon functionality
/// </summary>
public class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ITemperatureService _temperatureService;
    private bool _disposed;
    private Window? _popupWindow;
    private bool _isPopupVisible;

    public TrayIconService(ITemperatureService temperatureService)
    {
        _temperatureService = temperatureService;

        // Initialize NotifyIcon with a default icon
        _notifyIcon = new NotifyIcon
        {
            Icon = TemperatureIconGenerator.CreateTrayIcon("--°"),
            Text = "CoreView CPU Monitor",
            Visible = true
        };

        // Set up the context menu
        _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
        _notifyIcon.ContextMenuStrip.Items.Add("Show Monitor", null, ShowPopup);
        _notifyIcon.ContextMenuStrip.Items.Add("-"); // Separator
        _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, Exit);

        // Handle temperature changes and clicks
        _notifyIcon.Click += OnNotifyIconClick;
        _temperatureService.TemperatureChanged += OnTemperatureChanged;
    }

    /// <summary>
    /// Event handler for temperature changes - updates icon and tooltip
    /// </summary>
    private void OnTemperatureChanged(object? sender, TemperatureChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var temp = e.Temperature;
            var iconText = $"{temp:F0}";
            _notifyIcon.Icon = TemperatureIconGenerator.CreateTrayIcon(iconText);
            _notifyIcon.Text = $"CPU Temperature: {temp:F1}°C";
        });
    }

    /// <summary>
    /// Shows or hides the popup window when the tray icon is clicked
    /// </summary>
    private void OnNotifyIconClick(object? sender, EventArgs e)
    {
        // Only respond to left click
        if (e is MouseEventArgs mouseEvent && mouseEvent.Button != MouseButtons.Left)
            return;

        TogglePopupVisibility();
    }

    /// <summary>
    /// Shows the temperature monitor popup
    /// </summary>
    private void ShowPopup(object? sender, EventArgs e)
    {
        if (!_isPopupVisible)
            TogglePopupVisibility();
    }

    /// <summary>
    /// Toggles visibility of the popup window
    /// </summary>
    private void TogglePopupVisibility()
    {
        if (_disposed) return;

        if (_isPopupVisible)
        {
            _popupWindow?.Hide();
            _isPopupVisible = false;
        }
        else
        {
            if (_popupWindow == null)
            {
                _popupWindow = GetPopupWindow();
                _popupWindow.SourceInitialized += (s, e) => SetPopupWindowToBottomRight();
            }

            _popupWindow.Show();
            _popupWindow.Activate();
            _isPopupVisible = true;
        }
    }

    private void SetPopupWindowToBottomRight()
    {
        if (_popupWindow == null) return;

        // Calculate taskbar height
        var taskbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight - SystemParameters.WindowCaptionHeight;

        // Position window in the bottom right corner, right above the taskbar
        _popupWindow.Left = SystemParameters.PrimaryScreenWidth - _popupWindow.ActualWidth;
        _popupWindow.Top = SystemParameters.PrimaryScreenHeight - _popupWindow.ActualHeight - taskbarHeight;
    }

    /// <summary>
    /// Gets the popup window instance from the application resources
    /// </summary>
    protected virtual Window GetPopupWindow()
    {
        // Look for the PopupWindow in application resources
        if (Application.Current.Resources.Contains("MainWindow") &&
            Application.Current.Resources["MainWindow"] is Window window)
        {
            return window;
        }

        // As a fallback, create a new window
        return new Window
        {
            Width = 400,
            Height = 300,
            WindowStyle = WindowStyle.ToolWindow,  // Changed to ToolWindow to show close button
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            Topmost = true,
            BorderThickness = new Thickness(1),
            BorderBrush = System.Windows.Media.Brushes.Gray
        };
    }

    /// <summary>
    /// Exits the application
    /// </summary>
    private void Exit(object? sender, EventArgs e)
    {
        Application.Current.Shutdown();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _temperatureService.TemperatureChanged -= OnTemperatureChanged;
        _notifyIcon.Click -= OnNotifyIconClick;
        _notifyIcon.Dispose();

        GC.SuppressFinalize(this);
    }

    ~TrayIconService()
    {
        Dispose();
    }
}