using System.Drawing;
using SkiaSharp;

namespace CoreView.App.Icons;

/// <summary>
/// Generates high-quality system tray icons displaying temperature values as text
/// </summary>
public static class TemperatureIconGenerator
{
    // Internal size for high-quality rendering before downscaling
    private const int InternalSize = 256;
    
    // Text positioning adjustment to compensate for Windows tray padding
    private const float VerticalOffset = InternalSize * -0.04f;
    
    // High-quality sampling configuration for text and image scaling
    private static readonly SKSamplingOptions SamplingOptions = new(
        SKFilterMode.Linear,
        SKMipmapMode.Linear);
    
    /// <summary>
    /// Creates a high-quality tray icon with the given temperature text centered in it
    /// </summary>
    /// <param name="temperatureText">The temperature text to display (e.g., "54°")</param>
    /// <param name="size">Target size of the icon in pixels (default 40x40)</param>
    /// <returns>A system icon suitable for use in the Windows system tray</returns>
    public static Icon CreateTrayIcon(string temperatureText, int size = 40)
    {
        // Create high-resolution bitmap for quality downscaling
        using var bitmap = new SKBitmap(InternalSize, InternalSize);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        // Set up font with modern Windows typeface in regular weight
        using var typeface = SKTypeface.FromFamilyName("Segoe UI Variable", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                            ?? SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        
        // Calculate initial font size to fill entire icon
        var fontSize = InternalSize; // Start with 90% of icon size
        using var font = new SKFont(typeface, fontSize)
        {
            Subpixel = true,
            Edging = SKFontEdging.SubpixelAntialias
        };

        // Measure and adjust font size to fit width and height
        using var paint = new SKPaint 
        { 
            IsAntialias = true,
            Color = SKColors.Black
        };

        var metrics = font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;
        var textWidth = font.MeasureText(temperatureText, paint);
        
        // Scale font to fit within icon bounds while maintaining aspect ratio
        var heightScale = InternalSize / textHeight;
        var widthScale = InternalSize / textWidth;
        var scale = Math.Min(heightScale, widthScale) * 0.98f; // 98% to ensure no edge clipping
        font.Size *= scale;

        // Recalculate metrics with final font size
        metrics = font.Metrics;
        textHeight = metrics.Descent - metrics.Ascent;
        
        // Calculate final text position for perfect centering with vertical offset
        var finalWidth = font.MeasureText(temperatureText, paint);
        var xPos = (InternalSize - finalWidth) / 2;
        var yPos = (InternalSize + textHeight) / 2 - metrics.Descent + VerticalOffset;

        // Draw text in solid black
        canvas.DrawText(temperatureText, xPos, yPos, font, paint);

        // Scale down to target size with high-quality interpolation
        using var scaledBitmap = bitmap.Resize(new SKSizeI(size, size), SamplingOptions);
        using var pixmap = new SKPixmap(new SKImageInfo(size, size), scaledBitmap.GetPixels());
        using var gdiImage = new Bitmap(size, size, size * 4, 
            System.Drawing.Imaging.PixelFormat.Format32bppArgb, 
            pixmap.GetPixels());

        // Create and return the system tray icon
        var iconHandle = gdiImage.GetHicon();
        return Icon.FromHandle(iconHandle);
    }
}
