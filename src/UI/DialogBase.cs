using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>无边框圆角对话框基类：可拖动标题区、Esc 关闭、统一标题。</summary>
    public class DialogBase : Form
    {
        private bool _dragging;
        private Point _dragOffset;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public DialogBase()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Bg;
            ShowInTaskbar = false;
            KeyPreview = true;
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected virtual string DialogTitleText
        {
            get { return string.Empty; }
        }

        protected virtual string DialogSubtitleText
        {
            get { return string.Empty; }
        }

        protected int HeaderHeight
        {
            get { return Ui.S(58); }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Width > 0 && Height > 0)
            {
                Region = new Region(Ui.Round(new Rectangle(0, 0, Width, Height), Ui.S(16)));
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 主窗口“窗口置顶”时，普通模态弹窗会被它盖住（主窗口此时已禁用，看起来像卡死）。
            // 让弹窗跟随主窗口的置顶状态，确保它始终显示在主窗口之上。
            Form owner = Owner;
            if (owner != null && owner.TopMost && !TopMost)
            {
                TopMost = true;
                Activate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Ui.Quality(g);
            g.Clear(Theme.Bg);
            Ui.StrokeRound(g, Theme.Border, new Rectangle(0, 0, Width - 1, Height - 1), Ui.S(16), 1f);

            Ui.TextLeft(g, DialogTitleText, Theme.DialogTitle, Theme.TextMain,
                new Rectangle(Ui.S(24), Ui.S(16), Width - Ui.S(88), Ui.S(28)));
            string sub = DialogSubtitleText;
            if (!string.IsNullOrEmpty(sub))
            {
                Ui.TextLeft(g, sub, Theme.Tiny, Theme.TextFaint,
                    new Rectangle(Ui.S(24), Ui.S(40), Width - Ui.S(88), Ui.S(18)));
            }
        }

        protected IconButton CreateCloseButton()
        {
            IconButton close = new IconButton();
            close.Glyph = "\uE8BB";
            close.GlyphSize = 9f;
            close.BackColor = Theme.Bg;
            close.Size = new Size(Ui.S(28), Ui.S(28));
            close.Location = new Point(Width - Ui.S(40), Ui.S(14));
            close.HoverBack = Theme.BgDeep;
            close.HoverColor = Theme.TextMain;
            close.Click += delegate { Close(); };
            Controls.Add(close);
            return close;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.Y < HeaderHeight)
            {
                Point screen = PointToScreen(e.Location);
                _dragOffset = new Point(screen.X - Location.X, screen.Y - Location.Y);
                _dragging = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_dragging) return;
            Point screen = PointToScreen(e.Location);
            Location = new Point(screen.X - _dragOffset.X, screen.Y - _dragOffset.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
        }

        protected void EnableWindowDrag(Control header)
        {
            header.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;
                ReleaseCapture();
                SendMessage(Handle, 0xA1, (IntPtr)2, IntPtr.Zero);
            };
        }
    }

    /// <summary>圆角白色面板（可带边框）。</summary>
    public class RoundedPanel : Panel
    {
        public int Radius { get; set; }
        public bool DrawBorder { get; set; }
        public Color BorderColor { get; set; }

        public RoundedPanel()
        {
            Radius = 10;
            DrawBorder = true;
            BorderColor = Theme.Border;
            BackColor = Color.White;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Width > 0 && Height > 0)
            {
                Region = new Region(Ui.Round(new Rectangle(0, 0, Width, Height), Ui.S(Radius)));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            if (DrawBorder)
            {
                Ui.StrokeRound(e.Graphics, BorderColor, new Rectangle(0, 0, Width - 1, Height - 1), Ui.S(Radius), 1f);
            }
        }
    }
}
