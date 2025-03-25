using System.Windows;
using System.Windows.Forms;
using CoreView.App.Views;
using Application = System.Windows.Application;

namespace CoreView.App.Services;

/// <summary>
/// Base service class for managing system tray icons and their associated windows
/// </summary>
public abstract class BaseTrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly string _serviceName;
    private bool _disposed;
    private BaseMetricWindow? _metricWindow;

    protected BaseTrayIconService(string serviceName, Icon defaultIcon)
    {
        _serviceName = serviceName;

        // Initialize NotifyIcon with a default icon
        _notifyIcon = new NotifyIcon
        {
            Icon = defaultIcon,
            Text = $"CoreView {serviceName}",
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip()
        };

        // Set up context menu
        _notifyIcon.ContextMenuStrip.Items.Add($"Show {serviceName} Monitor", null, ShowWindow);
        _notifyIcon.ContextMenuStrip.Items.Add("-"); // Separator
        _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, Exit);

        // Handle clicks
        _notifyIcon.Click += OnNotifyIconClick;
    }

    /// <summary>
    /// Creates the metric window instance. Override to provide specific window type.
    /// </summary>
    protected abstract BaseMetricWindow CreateMetricWindow();

    /// <summary>
    /// Updates the tray icon. Override to provide specific icon update logic.
    /// </summary>
    protected virtual void UpdateTrayIcon(Icon icon, string tooltipText)
    {
        _notifyIcon.Icon = icon;
        _notifyIcon.Text = tooltipText;
    }

    private void OnNotifyIconClick(object? sender, EventArgs e)
    {
        // Only respond to left click
        if (e is MouseEventArgs mouseEvent && mouseEvent.Button != MouseButtons.Left)
            return;

        ToggleWindowVisibility();
    }

    private void ShowWindow(object? sender, EventArgs e)
    {
        if (_metricWindow?.IsVisible != true)
            ToggleWindowVisibility();
    }

    private void ToggleWindowVisibility()
    {
        if (_disposed) return;

        if (_metricWindow?.IsVisible == true)
        {
            _metricWindow.Hide();
        }
        else
        {
            if (_metricWindow == null)
            {
                _metricWindow = CreateMetricWindow();
                _metricWindow.SourceInitialized += (s, e) => _metricWindow.SetToBottomRight();
            }

            _metricWindow.Show();
            _metricWindow.Activate();
        }
    }

    private void Exit(object? sender, EventArgs e)
    {
        Application.Current.Shutdown();
    }

    public virtual void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _notifyIcon.Click -= OnNotifyIconClick;
        _notifyIcon.Dispose();

        GC.SuppressFinalize(this);
    }

    ~BaseTrayIconService()
    {
        Dispose();
    }
}