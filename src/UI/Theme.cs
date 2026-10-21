using System;
using System.Collections.Generic;
using System.Drawing;
using BossTodo.Core;

namespace BossTodo.UI
{
    /// <summary>配色、字体与象限色。</summary>
    public static class Theme
    {
        public static Color Hex(string hex) { return ColorTranslator.FromHtml(hex); }

        public static readonly Color Bg = Hex("#F4F6FB");
        public static readonly Color BgDeep = Hex("#EAEFF8");
        public static readonly Color Card = Color.White;
        public static readonly Color CardHover = Hex("#FCFDFF");
        public static readonly Color Border = Hex("#E5EAF3");
        public static readonly Color BorderStrong = Hex("#D3DCEA");
        public static readonly Color TextMain = Hex("#202634");
        public static readonly Color TextSub = Hex("#707C90");
        public static readonly Color TextFaint = Hex("#9BA6B7");
        public static readonly Color Accent = Hex("#3B7BF6");
        public static readonly Color AccentDark = Hex("#2C63D6");
        public static readonly Color AccentSoft = Hex("#EAF1FF");
        public static readonly Color Danger = Hex("#E5484D");
        public static readonly Color DangerSoft = Hex("#FDECEC");
        public static readonly Color Success = Hex("#2FA36B");
        public static readonly Color SuccessSoft = Hex("#E9F7EF");
        public static readonly Color Track = Hex("#D9E1EE");
        public static readonly Color Shadow = Hex("#DCE3F0");

        public static readonly Color Q1 = Hex("#E5484D");
        public static readonly Color Q1Soft = Hex("#FDECEC");
        public static readonly Color Q2 = Hex("#F08C1A");
        public static readonly Color Q2Soft = Hex("#FFF3E3");
        public static readonly Color Q3 = Hex("#3B7BF6");
        public static readonly Color Q3Soft = Hex("#EAF1FF");
        public static readonly Color Q4 = Hex("#8E9AAB");
        public static readonly Color Q4Soft = Hex("#F0F3F8");

        public static Color QuadrantColor(Quadrant q)
        {
            switch (q)
            {
                case Quadrant.UrgentImportant: return Q1;
                case Quadrant.UrgentUnimportant: return Q2;
                case Quadrant.NotUrgentImportant: return Q3;
                default: return Q4;
            }
        }

        public static Color QuadrantSoft(Quadrant q)
        {
            switch (q)
            {
                case Quadrant.UrgentImportant: return Q1Soft;
                case Quadrant.UrgentUnimportant: return Q2Soft;
                case Quadrant.NotUrgentImportant: return Q3Soft;
                default: return Q4Soft;
            }
        }

        public static Color ImportanceColor(Importance imp)
        {
            switch (imp)
            {
                case Importance.High: return Q1;
                case Importance.Medium: return Q3;
                default: return Q4;
            }
        }

        public static Color ImportanceSoft(Importance imp)
        {
            switch (imp)
            {
                case Importance.High: return Q1Soft;
                case Importance.Medium: return Q3Soft;
                default: return Q4Soft;
            }
        }

        public static string ImportanceName(Importance imp)
        {
            switch (imp)
            {
                case Importance.High: return "高";
                case Importance.Medium: return "中";
                default: return "低";
            }
        }

        private static readonly Dictionary<string, Font> FontCache = new Dictionary<string, Font>();
        private static readonly object FontGate = new object();

        public static string UiFontFamily = "Microsoft YaHei UI";
        public static string IconFontFamily = "Segoe MDL2 Assets";

        // 字号统一用像素单位：界面坐标已经通过 Ui.S() 按 DPI 缩放，
        // 若这里再用“点”并按 Ui.Scale 放大，会在高 DPI 下被系统再缩放一次，
        // 导致文字比行高多出一截并压到相邻文字上。
        private static float FontPixels(float point)
        {
            return (float)(point * 96.0 / 72.0 * Ui.Scale);
        }

        public static Font F(float point, FontStyle style)
        {
            float pixels = FontPixels(point);
            string key = pixels.ToString("0.##") + "|" + (int)style;
            lock (FontGate)
            {
                Font cached;
                if (FontCache.TryGetValue(key, out cached)) return cached;
                Font f;
                try { f = new Font(UiFontFamily, pixels, style, GraphicsUnit.Pixel); }
                catch
                {
                    try { f = new Font("Microsoft YaHei", pixels, style, GraphicsUnit.Pixel); }
                    catch { f = new Font("Segoe UI", pixels, style, GraphicsUnit.Pixel); }
                }
                FontCache[key] = f;
                return f;
            }
        }

        public static Font Icon(float point)
        {
            float pixels = FontPixels(point);
            string key = "icon|" + pixels.ToString("0.##");
            lock (FontGate)
            {
                Font cached;
                if (FontCache.TryGetValue(key, out cached)) return cached;
                Font f;
                try { f = new Font(IconFontFamily, pixels, FontStyle.Regular, GraphicsUnit.Pixel); }
                catch { f = new Font("Segoe UI Symbol", pixels, FontStyle.Regular, GraphicsUnit.Pixel); }
                FontCache[key] = f;
                return f;
            }
        }

        public static Font AppTitle { get { return F(13.2f, FontStyle.Bold); } }
        public static Font DialogTitle { get { return F(12.2f, FontStyle.Bold); } }
        public static Font CardTitle { get { return F(10.6f, FontStyle.Bold); } }
        public static Font Body { get { return F(9.6f, FontStyle.Regular); } }
        public static Font BodyStrong { get { return F(9.6f, FontStyle.Bold); } }
        public static Font Small { get { return F(8.8f, FontStyle.Regular); } }
        public static Font SmallStrong { get { return F(8.8f, FontStyle.Bold); } }
        public static Font Tiny { get { return F(8.1f, FontStyle.Regular); } }
    }
}
