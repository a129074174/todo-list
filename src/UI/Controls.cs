using System;
using System.Drawing;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>自绘控件基类：双缓冲 + 纯色背景。</summary>
    public abstract class SoftControl : Control
    {
        protected SoftControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Bg;
            ForeColor = Theme.TextMain;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
        }
    }

    /// <summary>图标按钮（Segoe MDL2 Assets），支持悬停底色、激活态、实心圆。</summary>
    public class IconButton : SoftControl
    {
        private bool _hover;
        private bool _down;

        public string Glyph { get; set; }
        public float GlyphSize { get; set; }
        public Color GlyphColor { get; set; }
        public Color HoverColor { get; set; }
        public Color HoverBack { get; set; }
        public Color ActiveBack { get; set; }
        public Color ActiveColor { get; set; }
        public bool Active { get; set; }
        public bool Filled { get; set; }
        public Color FillColor { get; set; }
        public bool ShowHoverBack { get; set; }

        public IconButton()
        {
            Glyph = string.Empty;
            GlyphSize = 10f;
            GlyphColor = Theme.TextSub;
            HoverColor = Theme.Accent;
            HoverBack = Theme.BgDeep;
            ActiveBack = Theme.AccentSoft;
            ActiveColor = Theme.Accent;
            FillColor = Theme.Accent;
            ShowHoverBack = true;
            Cursor = Cursors.Hand;
            TabStop = false;
            Size = new Size(Ui.S(30), Ui.S(30));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Ui.Quality(g);
            g.Clear(BackColor);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (Filled)
            {
                Color fill = _hover ? Ui.Mix(FillColor, Color.White, 0.06f) : FillColor;
                if (_down) fill = Ui.Mix(FillColor, Color.Black, 0.08f);
                Ui.FillRound(g, fill, rect, Math.Min(rect.Width, rect.Height) / 2);
                Ui.Glyph(g, Glyph, GlyphSize, Color.White, rect, StringAlignment.Center);
                return;
            }

            Color back = Color.Empty;
            if (Active) back = ActiveBack;
            else if (_hover && ShowHoverBack) back = HoverBack;
            if (_down && !Active) back = Theme.BgDeep;
            if (back != Color.Empty) Ui.FillRound(g, back, rect, Ui.S(9));

            Color color = Active ? ActiveColor : (_hover ? HoverColor : GlyphColor);
            Ui.Glyph(g, Glyph, GlyphSize, color, rect, StringAlignment.Center);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _down = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _down = false;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _down = false;
            Invalidate();
        }
    }
}
