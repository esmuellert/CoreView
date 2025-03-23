using SkiaSharp;
using System.Drawing.Imaging;

namespace CoreView.App.Icons
{
    public static class TemperatureIconGenerator
    {
        // 提高内部渲染尺寸，保证缩放后依然清晰
        private const int InternalSize = 128;
        private const float CircleRadius = InternalSize * 0.48f;

        private static readonly SKSamplingOptions SamplingOptions = new(
            SKFilterMode.Linear,
            SKMipmapMode.Linear);

        /// <summary>
        /// 生成高清托盘图标
        /// </summary>
        /// <param name="temperatureText">例如 "54°"</param>
        /// <param name="finalSize">图标最终尺寸，托盘通常16~24，可用32或40确保更清晰</param>
        /// <returns>可直接赋值给 NotifyIcon.Icon 的 Icon 对象</returns>
        public static Icon CreateTrayIcon(string temperatureText, int finalSize = 32)
        {
            // 1. 创建高分辨率位图
            using var bitmap = new SKBitmap(InternalSize, InternalSize);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);

            // 2. 可选：绘制半透明背景圆
            using (var backgroundPaint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 50),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            })
            {
                canvas.DrawCircle(InternalSize / 2f, InternalSize / 2f, CircleRadius, backgroundPaint);
            }

            // 3. 构造 SKFontStyle
            var boldStyle = new SKFontStyle(
                SKFontStyleWeight.Bold,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright
            );

            // 4. 尝试用 "Segoe UI Variable Display"；若失败则回退到 "Segoe UI"
            using var typeface = SKTypeface.FromFamilyName("Segoe UI Variable Display", boldStyle)
                ?? SKTypeface.FromFamilyName("Segoe UI", boldStyle);

            // 字体尽量大，让文字几乎填满图标
            using var font = new SKFont(typeface, InternalSize * 0.55f)
            {
                Subpixel = true,
                Edging = SKFontEdging.SubpixelAntialias
            };

            using var textPaint = new SKPaint
            {
                IsAntialias = true
            };

            // 5. 计算文字的宽高，用于居中
            var textWidth = font.MeasureText(temperatureText, textPaint);
            var metrics = font.Metrics;
            var textHeight = metrics.Descent - metrics.Ascent;
            var xPos = (InternalSize - textWidth) / 2f;
            var yPos = (InternalSize - textHeight) / 2f - metrics.Ascent;

            // 6. 先画描边(阴影)
            textPaint.Style = SKPaintStyle.Stroke;
            textPaint.Color = new SKColor(0, 0, 0, 200);
            textPaint.StrokeWidth = InternalSize * 0.05f;
            canvas.DrawText(temperatureText, xPos, yPos, font, textPaint);

            // 7. 再画主文字
            textPaint.Style = SKPaintStyle.Fill;
            textPaint.Color = SKColors.White;
            canvas.DrawText(temperatureText, xPos, yPos, font, textPaint);

            // 8. 缩放到目标尺寸
            using var scaledBitmap = bitmap.Resize(new SKSizeI(finalSize, finalSize), SamplingOptions);
            using var pixmap = scaledBitmap.PeekPixels();

            // 9. 转为 GDI+ Bitmap
            using var gdiImage = new Bitmap(finalSize, finalSize, PixelFormat.Format32bppArgb);
            var data = gdiImage.LockBits(
                new Rectangle(0, 0, finalSize, finalSize),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb
            );

            // 需要 (dstInfo, dstPixels, dstRowBytes, srcX, srcY)
            pixmap.ReadPixels(
                new SKImageInfo(scaledBitmap.Width, scaledBitmap.Height),
                data.Scan0,
                scaledBitmap.Width * 4,
                0,
                0
            );

            gdiImage.UnlockBits(data);

            // 10. 转为 Icon
            var iconHandle = gdiImage.GetHicon();
            return Icon.FromHandle(iconHandle);
        }
    }
}
