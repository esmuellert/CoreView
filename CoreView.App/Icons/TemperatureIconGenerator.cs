using System.Drawing;
using SkiaSharp;

namespace CoreView.App.Icons;

/// <summary>
/// Generates high-quality system tray icons displaying temperature values as text
/// </summary>
public static class TemperatureIconGenerator
{
    // Internal size for high-quality rendering before downscaling
    private const int InternalSize = 64;
    
    // Modern Windows blue color for background
    private static readonly SKColor BackgroundColor = new(0, 120, 215);
    
    // Background rectangle dimensions
    private const float RectPadding = InternalSize * 0.1f; // 10% padding from edges
    private const float CornerRadius = InternalSize * 0.15f; // ~10px at 64px size
    
    // High-quality sampling configuration for text and image scaling
    private static readonly SKSamplingOptions SamplingOptions = new(
        SKFilterMode.Linear,
        SKMipmapMode.Linear);
    
    /// <summary>
    /// Creates a high-quality tray icon with the given temperature text centered in it
    /// </summary>
    /// <param name="temperatureText">The temperature text to display (e.g., "54°")</param>
    /// <param name="size">Target size of the icon in pixels (default 32x32)</param>
    /// <returns>A system icon suitable for use in the Windows system tray</returns>
    public static Icon CreateTrayIcon(string temperatureText, int size = 32)
    {
        // Create high-resolution bitmap for quality downscaling
        using var bitmap = new SKBitmap(InternalSize, InternalSize);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        // Draw rounded rectangle background
        var rect = new SKRect(
            RectPadding,
            RectPadding,
            InternalSize - RectPadding,
            InternalSize - RectPadding
        );

        using (var backgroundPaint = new SKPaint
        {
            Color = BackgroundColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, backgroundPaint);
        }

        // Set up font with modern Windows typeface
        using var typeface = SKTypeface.FromFamilyName("Segoe UI Variable Display", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                            ?? SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        
        // Calculate initial font size based on rectangle height
        var maxHeight = rect.Height * 0.9f; // 90% of rectangle height
        var fontSize = maxHeight;
        using var font = new SKFont(typeface, fontSize)
        {
            Subpixel = true,
            Edging = SKFontEdging.SubpixelAntialias
        };

        // Measure and adjust font size to fit both width and height
        using var tempPaint = new SKPaint { IsAntialias = true };
        var metrics = font.Metrics;
        var textHeight = metrics.Descent - metrics.Ascent;
        var textWidth = font.MeasureText(temperatureText, tempPaint);
        
        // Scale font to fit within rectangle while maintaining aspect ratio
        var heightScale = maxHeight / textHeight;
        var widthScale = (rect.Width * 0.9f) / textWidth;
        var scale = Math.Min(heightScale, widthScale);
        font.Size *= scale;

        // Recalculate metrics with final font size
        metrics = font.Metrics;
        textHeight = metrics.Descent - metrics.Ascent;
        
        // Set up paint for text rendering
        using var textPaint = new SKPaint { IsAntialias = true };
        
        // Calculate final text position for perfect centering
        var finalWidth = font.MeasureText(temperatureText, textPaint);
        var xPos = (InternalSize - finalWidth) / 2;
        var yPos = (InternalSize + textHeight) / 2 - metrics.Descent;

        // Draw text shadow/outline for contrast
        textPaint.Style = SKPaintStyle.Stroke;
        textPaint.Color = new SKColor(0, 0, 0, 160);
        textPaint.StrokeWidth = InternalSize * 0.04f;
        canvas.DrawText(temperatureText, xPos, yPos, font, textPaint);

        // Draw main text in white
        textPaint.Style = SKPaintStyle.Fill;
        textPaint.Color = SKColors.White;
        canvas.DrawText(temperatureText, xPos, yPos, font, textPaint);

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
