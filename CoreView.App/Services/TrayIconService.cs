using System.Drawing;
using System.Windows;
using System.Windows.Forms;
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
    private readonly Icon[] _temperatureIcons;
    private Window? _popupWindow;
    private bool _isPopupVisible;
    private bool _disposed;

    public TrayIconService(ITemperatureService temperatureService)
    {
        _temperatureService = temperatureService;
        
        // Create temperature icon variations (we'll reuse a simple icon for this example)
        _temperatureIcons = new[]
        {
            SystemIcons.Information, // Cold
            SystemIcons.Information, // Normal
            SystemIcons.Warning      // Hot
        };

        // Initialize NotifyIcon
        _notifyIcon = new NotifyIcon
        {
            Icon = _temperatureIcons[1],
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
            var iconIndex = temp < 50 ? 0 : (temp < 75 ? 1 : 2);
            _notifyIcon.Icon = _temperatureIcons[iconIndex];
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
                _popupWindow.Deactivated += (_, _) =>
                {
                    _popupWindow.Hide();
                    _isPopupVisible = false;
                };
            }

            // Position window near the tray icon
            var cursorPos = System.Windows.Forms.Cursor.Position;
            _popupWindow.Left = cursorPos.X - _popupWindow.Width / 2;
            _popupWindow.Top = cursorPos.Y - _popupWindow.Height;

            _popupWindow.Show();
            _popupWindow.Activate();
            _isPopupVisible = true;
        }
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
        
        // As a fallback, look for a window with the name "PopupWindow"
        return Application.Current.Windows.OfType<Window>()
            .FirstOrDefault(w => w.Name == "PopupWindow") ?? 
            new Window 
            { 
                Width = 400, 
                Height = 300, 
                WindowStyle = WindowStyle.None,
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