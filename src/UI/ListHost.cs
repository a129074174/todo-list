using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>列表容器：双缓冲、手动滚动（配合 ThinScrollbar）。</summary>
    public class ListHost : Panel
    {
        public int ContentHeight { get; set; }

        public ListHost()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Bg;
            AutoScroll = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
        }
    }

    /// <summary>运行时绘制托盘 / 窗口图标（不依赖外部资源）。</summary>
    public static class IconFactory
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static Icon Create(int size)
        {
            using (Bitmap bmp = DrawBitmap(size))
            {
                IntPtr handle = bmp.GetHicon();
                try
                {
                    using (Icon temp = Icon.FromHandle(handle))
                    {
                        return (Icon)temp.Clone();
                    }
                }
                finally
                {
                    DestroyIcon(handle);
                }
            }
        }

        private static Bitmap DrawBitmap(int size)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                int pad = Math.Max(1, size / 16);
                Rectangle rect = new Rectangle(pad, pad, size - pad * 2, size - pad * 2);
                using (GraphicsPath path = Ui.Round(rect, Math.Max(2, size / 5)))
                using (LinearGradientBrush brush = new LinearGradientBrush(rect, Theme.Accent, Theme.AccentDark, 45f))
                {
                    g.FillPath(brush, path);
                }
                float w = Math.Max(1.6f, size / 12f);
                Ui.Check(g, new RectangleF(rect.X + rect.Width * 0.16f, rect.Y + rect.Height * 0.12f,
                    rect.Width * 0.7f, rect.Height * 0.72f), Color.White, w);
            }
            return bmp;
        }

        /// <summary>生成多尺寸 ICO（PNG 压缩条目），供 exe / 安装包使用。</summary>
        public static bool SaveIco(string path, int[] sizes)
        {
            try
            {
                if (sizes == null || sizes.Length == 0) return false;
                List<byte[]> images = new List<byte[]>();
                foreach (int size in sizes)
                {
                    using (Bitmap bmp = DrawBitmap(size))
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        images.Add(ms.ToArray());
                    }
                }

                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    writer.Write((ushort)0);
                    writer.Write((ushort)1);
                    writer.Write((ushort)images.Count);
                    int offset = 6 + 16 * images.Count;
                    for (int i = 0; i < images.Count; i++)
                    {
                        int size = sizes[i];
                        writer.Write((byte)(size >= 256 ? 0 : size));
                        writer.Write((byte)(size >= 256 ? 0 : size));
                        writer.Write((byte)0);
                        writer.Write((byte)0);
                        writer.Write((ushort)1);
                        writer.Write((ushort)32);
                        writer.Write(images[i].Length);
                        writer.Write(offset);
                        offset += images[i].Length;
                    }
                    foreach (byte[] image in images) writer.Write(image);
                }
                return File.Exists(path) && new FileInfo(path).Length > 1000;
            }
            catch
            {
                return false;
            }
        }
    }
}
