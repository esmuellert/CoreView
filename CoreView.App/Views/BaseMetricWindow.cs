using System.Windows;
using System.Windows.Input;

namespace CoreView.App.Views;

/// <summary>
/// Base window class for all metric monitoring windows
/// </summary>
public class BaseMetricWindow : Window
{
    public BaseMetricWindow()
    {
        // Set common window properties
        Width = 400;
        Height = 450;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;
        Background = System.Windows.Media.Brushes.WhiteSmoke;
        BorderBrush = System.Windows.Media.Brushes.DarkGray;
        BorderThickness = new Thickness(1);
    }

    /// <summary>
    /// Event handler for close button click
    /// </summary>
    protected void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// Event handler for mouse down on window - enables dragging
    /// </summary>
    protected void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    /// <summary>
    /// Centers the window in the screen
    /// </summary>
    public void SetToBottomRight()
    {
        // Calculate taskbar height
        var taskbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight - SystemParameters.WindowCaptionHeight;

        // Position window in the bottom right corner, right above the taskbar
        Left = SystemParameters.PrimaryScreenWidth - ActualWidth;
        Top = SystemParameters.PrimaryScreenHeight - ActualHeight - taskbarHeight;
    }
}