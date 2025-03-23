using System.Drawing;
using SkiaSharp;

namespace CoreView.App.Icons;

/// <summary>
/// Generates system tray icons displaying temperature values as text
/// </summary>
public static class TemperatureIconGenerator
{
    /// <summary>
    /// Creates a tray icon with the given temperature text centered in it
    /// </summary>
    /// <param name="temperatureText">The temperature text to display (e.g., "54°")</param>
    /// <param name="size">Size of the icon in pixels (default 32x32)</param>
    /// <returns>A system icon suitable for use in the Windows system tray</returns>
    public static Icon CreateTrayIcon(string temperatureText, int size = 32)
    {
        // Create a new SkiaSharp bitmap with transparency
        using var bitmap = new SKBitmap(size, size);
        using var canvas = new SKCanvas(bitmap);

        // Clear with transparency
        canvas.Clear(SKColors.Transparent);

        // Create font and paint objects
        using var typeface = SKTypeface.FromFamilyName("Segoe UI Variable", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                            ?? SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        using var font = new SKFont(typeface, size * 0.6f) { Subpixel = true };
        using var paint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true
        };

        // Calculate vertical center position for text
        var textWidth = font.MeasureText(temperatureText, paint);
        var textHeight = font.Metrics.CapHeight;
        var yPos = (size - textHeight) / 2 + textHeight;

        // Draw the text centered in the bitmap
        canvas.DrawText(temperatureText, (size - textWidth) / 2, yPos, font, paint);

        // Convert SKBitmap to System.Drawing.Bitmap
        using var pixmap = new SKPixmap(new SKImageInfo(size, size), bitmap.GetPixels());
        using var gdiImage = new Bitmap(size, size, size * 4, 
            System.Drawing.Imaging.PixelFormat.Format32bppArgb, 
            pixmap.GetPixels());

        // Convert to icon
        var iconHandle = gdiImage.GetHicon();
        return Icon.FromHandle(iconHandle);
    }
}