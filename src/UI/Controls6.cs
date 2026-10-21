using System;
using System.Drawing;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>四象限分组标题：彩色圆点 + 名称 + 数量。</summary>
    public class GroupHeader : SoftControl
    {
        public string Title { get; set; }
        public int Count { get; set; }
        public Color Accent { get; set; }

        public GroupHeader()
        {
            Title = string.Empty;
            Accent = Theme.Accent;
            BackColor = Theme.Bg;
            Height = Ui.S(28);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Ui.Quality(g);
            g.Clear(BackColor);

            int dot = Ui.S(6);
            int cy = Height / 2;
            using (SolidBrush brush = new SolidBrush(Accent))
            {
                g.FillEllipse(brush, Ui.S(3), cy - dot / 2, dot, dot);
            }

            Font font = Theme.F(8.7f, FontStyle.Bold);
            SizeF size = Ui.Measure(g, Title, font);
            int tx = Ui.S(15);
            Ui.TextLeft(g, Title, font, Theme.TextSub, new Rectangle(tx, 0, Width - tx, Height));

            Font countFont = Theme.F(7.8f, FontStyle.Bold);
            string countText = Count.ToString();
            SizeF countSize = Ui.Measure(g, countText, countFont);
            int pillW = Math.Max(Ui.S(22), (int)Math.Ceiling(countSize.Width) + Ui.S(10));
            int countX = tx + (int)Math.Ceiling(size.Width) + Ui.S(4);
            Rectangle pill = new Rectangle(countX, cy - Ui.S(8), pillW, Ui.S(16));
            Ui.FillRound(g, Ui.Alpha(Accent, 26), pill, Ui.S(8));
            Ui.TextCenter(g, countText, countFont, Accent, pill);

            int lineX = pill.Right + Ui.S(8);
            if (lineX < Width)
            {
                using (Pen pen = new Pen(Theme.Border, 1f))
                {
                    g.DrawLine(pen, lineX, cy, Width - Ui.S(2), cy);
                }
            }
        }
    }

    /// <summary>“已完成”折叠标题。</summary>
    public class SectionToggle : SoftControl
    {
        private bool _hover;

        public new string Text { get; set; }
        public int Count { get; set; }
        public bool Expanded { get; set; }

        public SectionToggle()
        {
            Text = "已完成";
            BackColor = Theme.Bg;
            Cursor = Cursors.Hand;
            TabStop = false;
            Height = Ui.S(32);
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
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Ui.Quality(g);
            g.Clear(BackColor);

            int cy = Height / 2;
            Rectangle glyphRect = new Rectangle(Ui.S(2), cy - Ui.S(9), Ui.S(18), Ui.S(18));
            Ui.Glyph(g, Expanded ? "\uE70D" : "\uE76C", 8f, _hover ? Theme.TextSub : Theme.TextFaint, glyphRect, StringAlignment.Center);

            Font font = Theme.F(8.7f, FontStyle.Bold);
            string label = Text + " (" + Count + ")";
            Ui.TextLeft(g, label, font, _hover ? Theme.TextSub : Theme.TextFaint,
                new Rectangle(Ui.S(22), 0, Width - Ui.S(24), Height));

            SizeF size = Ui.Measure(g, label, font);
            int lineX = Ui.S(26) + (int)Math.Ceiling(size.Width);
            if (lineX < Width)
            {
                using (Pen pen = new Pen(Theme.Border, 1f))
                {
                    g.DrawLine(pen, lineX, cy, Width - Ui.S(2), cy);
                }
            }
        }
    }

    /// <summary>空状态插画（纯绘制，无图片资源）。</summary>
    public class EmptyState : SoftControl
    {
        public string TitleText { get; set; }
        public string SubText { get; set; }

        public EmptyState()
        {
            TitleText = "暂无待办";
            SubText = "在上方输入框写下第一件事，回车即可添加";
            BackColor = Theme.Bg;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Ui.Quality(g);
            g.Clear(BackColor);

            int size = Ui.S(64);
            int cx = Width / 2;
            int top = Math.Max(Ui.S(18), Height / 4);
            Rectangle circle = new Rectangle(cx - size / 2, top, size, size);
            Ui.FillRound(g, Theme.AccentSoft, circle, size / 2);
            Ui.StrokeRound(g, Ui.Alpha(Theme.Accent, 40), circle, size / 2, 1f);
            Ui.Glyph(g, "\uE73E", 24f, Theme.Accent, circle, StringAlignment.Center);

            Rectangle titleRect = new Rectangle(0, circle.Bottom + Ui.S(14), Width, Ui.S(24));
            Ui.TextCenter(g, TitleText, Theme.F(10.2f, FontStyle.Bold), Theme.TextSub, titleRect);
            Rectangle subRect = new Rectangle(Ui.S(24), titleRect.Bottom + Ui.S(2), Width - Ui.S(48), Ui.S(38));
            Ui.Text(g, SubText, Theme.Small, Theme.TextFaint, subRect, StringAlignment.Center, StringAlignment.Near, false);
        }
    }
}
