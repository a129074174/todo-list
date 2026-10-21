using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>GDI+ 绘制工具（圆角、文字、图标、对勾），所有像素尺寸都走 S() 缩放。</summary>
    public static class Ui
    {
        public static float Scale = 1f;

        public static int S(double value)
        {
            int v = (int)Math.Round(value * Scale);
            if (value > 0 && v < 1) v = 1;
            return v;
        }

        public static float SF(double value)
        {
            return (float)(value * Scale);
        }

        public static SizeF Measure(Graphics g, string text, Font font)
        {
            if (string.IsNullOrEmpty(text)) return SizeF.Empty;
            Size size = TextRenderer.MeasureText(g, text, font, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            return new SizeF(size.Width, size.Height);
        }

        public static void Quality(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        public static GraphicsPath Round(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = Math.Max(1, Math.Min(radius * 2, Math.Min(rect.Width, rect.Height)));
            if (d <= 2)
            {
                path.AddRectangle(rect);
                return path;
            }
            Rectangle arc = new Rectangle(rect.X, rect.Y, d, d);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d;
            path.AddArc(arc, 0, 90);
            arc.X = rect.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void FillRound(Graphics g, Color color, Rectangle rect, int radius)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            using (GraphicsPath path = Round(rect, radius))
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillPath(brush, path);
            }
        }

        public static void StrokeRound(Graphics g, Color color, Rectangle rect, int radius, float width)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            using (GraphicsPath path = Round(rect, radius))
            using (Pen pen = new Pen(color, width))
            {
                pen.Alignment = PenAlignment.Inset;
                g.DrawPath(pen, path);
            }
        }

        public static void Text(Graphics g, string text, Font font, Color color, Rectangle rect,
            StringAlignment hAlign, StringAlignment vAlign, bool ellipsis)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            if (ellipsis) flags |= TextFormatFlags.EndEllipsis;
            if (hAlign == StringAlignment.Center) flags |= TextFormatFlags.HorizontalCenter;
            else if (hAlign == StringAlignment.Far) flags |= TextFormatFlags.Right;
            else flags |= TextFormatFlags.Left;
            if (vAlign == StringAlignment.Center) flags |= TextFormatFlags.VerticalCenter;
            else if (vAlign == StringAlignment.Far) flags |= TextFormatFlags.Bottom;
            else flags |= TextFormatFlags.Top;

            TextRenderer.DrawText(g, text ?? string.Empty, font, rect, color, flags);
        }

        public static void TextLeft(Graphics g, string text, Font font, Color color, Rectangle rect)
        {
            Text(g, text, font, color, rect, StringAlignment.Near, StringAlignment.Center, true);
        }

        public static void TextRight(Graphics g, string text, Font font, Color color, Rectangle rect)
        {
            Text(g, text, font, color, rect, StringAlignment.Far, StringAlignment.Center, true);
        }

        public static void TextCenter(Graphics g, string text, Font font, Color color, Rectangle rect)
        {
            Text(g, text, font, color, rect, StringAlignment.Center, StringAlignment.Center, true);
        }

        public static void Glyph(Graphics g, string glyph, float point, Color color, Rectangle rect, StringAlignment align)
        {
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix |
                TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
            if (align == StringAlignment.Center) flags |= TextFormatFlags.HorizontalCenter;
            else if (align == StringAlignment.Far) flags |= TextFormatFlags.Right;
            else flags |= TextFormatFlags.Left;
            TextRenderer.DrawText(g, glyph, Theme.Icon(point), rect, color, flags);
        }

        public static void Check(Graphics g, RectangleF rect, Color color, float width)
        {
            using (Pen pen = new Pen(color, width))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;
                PointF a = new PointF(rect.X + rect.Width * 0.20f, rect.Y + rect.Height * 0.54f);
                PointF b = new PointF(rect.X + rect.Width * 0.43f, rect.Y + rect.Height * 0.76f);
                PointF c = new PointF(rect.X + rect.Width * 0.81f, rect.Y + rect.Height * 0.27f);
                g.DrawLines(pen, new PointF[] { a, b, c });
            }
        }

        public static Color Alpha(Color color, int alpha)
        {
            if (alpha < 0) alpha = 0;
            if (alpha > 255) alpha = 255;
            return Color.FromArgb(alpha, color);
        }

        public static Color Mix(Color a, Color b, float t)
        {
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            return Color.FromArgb(
                (int)Math.Round(a.A + (b.A - a.A) * t),
                (int)Math.Round(a.R + (b.R - a.R) * t),
                (int)Math.Round(a.G + (b.G - a.G) * t),
                (int)Math.Round(a.B + (b.B - a.B) * t));
        }

        public static void DrawSoftShadow(Graphics g, Rectangle rect, int radius, int spread, int alpha)
        {
            for (int i = spread; i >= 1; i--)
            {
                Rectangle r = new Rectangle(rect.X - i, rect.Y - i + 1, rect.Width + i * 2, rect.Height + i * 2);
                int a = Math.Max(1, alpha / (i + 1));
                StrokeRound(g, Color.FromArgb(a, Theme.Shadow), r, radius + i, 1f);
            }
        }
    }
}

