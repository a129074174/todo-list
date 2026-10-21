using System;
using System.Drawing;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>胶囊文字按钮 / 筛选标签。</summary>
    public class PillButton : SoftControl
    {
        private bool _hover;
        private bool _down;

        public new string Text { get; set; }
        public bool Selected { get; set; }
        public bool Filled { get; set; }
        public Color Tint { get; set; }
        public Color SelectedTint { get; set; }
        public Color TextColor { get; set; }
        public Color SelectedTextColor { get; set; }
        public Color FilledColor { get; set; }
        public bool Bold { get; set; }
        public float FontSize { get; set; }

        public PillButton()
        {
            Text = string.Empty;
            Tint = Theme.BgDeep;
            SelectedTint = Theme.AccentSoft;
            TextColor = Theme.TextSub;
            SelectedTextColor = Theme.Accent;
            FilledColor = Theme.Accent;
            FontSize = 8.8f;
            Cursor = Cursors.Hand;
            TabStop = false;
            Size = new Size(Ui.S(58), Ui.S(28));
        }

        public int PreferredWidth(Graphics g)
        {
            Font font = Bold ? Theme.F(FontSize, FontStyle.Bold) : Theme.F(FontSize, FontStyle.Regular);
            SizeF size = Ui.Measure(g, Text ?? string.Empty, font);
            return (int)Math.Ceiling(size.Width) + Ui.S(24);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Ui.Quality(g);
            g.Clear(BackColor);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color back;
            Color fore;
            if (Filled)
            {
                back = _down ? Ui.Mix(FilledColor, Color.Black, 0.10f) : (_hover ? Ui.Mix(FilledColor, Color.White, 0.07f) : FilledColor);
                fore = Color.White;
            }
            else if (Selected)
            {
                back = SelectedTint;
                fore = SelectedTextColor;
            }
            else
            {
                back = _hover ? Ui.Mix(Tint, Theme.Border, 0.55f) : Tint;
                fore = TextColor;
            }

            Ui.FillRound(g, back, rect, Height / 2);
            Font font = Bold ? Theme.F(FontSize, FontStyle.Bold) : Theme.F(FontSize, FontStyle.Regular);
            Ui.TextCenter(g, Text, font, fore, new Rectangle(Ui.S(8), 0, Width - Ui.S(16), Height));
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
