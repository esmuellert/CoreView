using SkiaSharp;

namespace CoreView.App.Icons;

/// <summary>
/// Generates high-quality system tray icons displaying temperature values as text
/// </summary>
public static class TemperatureIconGenerator
{
    // Internal rendering size for high quality (will be scaled down for the tray)
    private const int InternalSize = 64;
    
    // Background circle properties
    private static readonly SKColor BackgroundColor = new(0, 0, 0, 40); // Semi-transparent black
    private const float CircleRadius = InternalSize * 0.45f; // Slightly smaller than icon
    
    // High-quality sampling options for text and image scaling
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
        // Create a high-resolution bitmap for better quality when scaled down
        using var bitmap = new SKBitmap(InternalSize, InternalSize);
        using var canvas = new SKCanvas(bitmap);

        // Clear with transparency
        canvas.Clear(SKColors.Transparent);

        // Draw translucent background circle
        using (var backgroundPaint = new SKPaint
        {
            Color = BackgroundColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawCircle(InternalSize / 2f, InternalSize / 2f, CircleRadius, backgroundPaint);
        }

        // Create font and text effects
        using var typeface = SKTypeface.FromFamilyName("Segoe UI Variable Display", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                            ?? SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        
        // Use a large font size for internal rendering
        using var font = new SKFont(typeface, InternalSize * 0.4f)
        {
            Subpixel = true,
            Edging = SKFontEdging.SubpixelAntialias
        };

        // Configure text paint with effects for better visibility
        using var textPaint = new SKPaint
        {
            IsAntialias = true
        };

        // Calculate text position for perfect centering
        var textBounds = font.MeasureText(temperatureText, textPaint);
        var metrics = font.Metrics;
        var xPos = (InternalSize - textBounds) / 2;
        var yPos = (InternalSize - (metrics.Descent - metrics.Ascent)) / 2 - metrics.Ascent;

        // Draw text shadow/outline first
        textPaint.Style = SKPaintStyle.Stroke;
        textPaint.Color = new SKColor(0, 0, 0, 160);
        textPaint.StrokeWidth = InternalSize * 0.04f;
        canvas.DrawText(temperatureText, xPos, yPos, font, textPaint);

        // Draw main text
        textPaint.Style = SKPaintStyle.Fill;
        textPaint.Color = SKColors.White;
        canvas.DrawText(temperatureText, xPos, yPos, font, textPaint);

        // Scale down the high-resolution bitmap to the target size
        using var scaledBitmap = bitmap.Resize(new SKSizeI(size, size), SamplingOptions);
        using var pixmap = new SKPixmap(new SKImageInfo(size, size), scaledBitmap.GetPixels());
        using var gdiImage = new Bitmap(size, size, size * 4, 
            System.Drawing.Imaging.PixelFormat.Format32bppArgb, 
            pixmap.GetPixels());

        // Convert to icon
        var iconHandle = gdiImage.GetHicon();
        return Icon.FromHandle(iconHandle);
    }
}