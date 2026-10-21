using System;
using System.Drawing;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>分段选择器（重要程度 / 紧迫阈值等），选中项白底高亮。</summary>
    public class SegmentedControl : SoftControl
    {
        private int _hoverIndex;

        public string[] Items { get; set; }
        public Color[] ItemColors { get; set; }
        public int SelectedIndex { get; set; }
        public event EventHandler SelectionChanged;

        public SegmentedControl()
        {
            Items = new string[0];
            _hoverIndex = -1;
            Cursor = Cursors.Hand;
            TabStop = false;
            Size = new Size(Ui.S(210), Ui.S(32));
        }

        public void SetSelectedSilently(int index)
        {
            SelectedIndex = index;
            Invalidate();
        }

        private int HitTest(int x)
        {
            if (Items == null || Items.Length == 0) return -1;
            int seg = Width / Items.Length;
            int index = x / Math.Max(1, seg);
            if (index < 0) index = 0;
            if (index >= Items.Length) index = Items.Length - 1;
            return index;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int h = HitTest(e.X);
            if (h != _hoverIndex)
            {
                _hoverIndex = h;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int index = HitTest(e.X);
            if (index < 0 || index == SelectedIndex) return;
            SelectedIndex = index;
            Invalidate();
            if (SelectionChanged != null) SelectionChanged(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Ui.Quality(g);
            g.Clear(BackColor);

            Rectangle all = new Rectangle(0, 0, Width, Height);
            Ui.FillRound(g, Theme.BgDeep, all, Ui.S(9));

            if (Items == null || Items.Length == 0) return;
            int seg = Width / Items.Length;
            for (int i = 0; i < Items.Length; i++)
            {
                Rectangle cell = new Rectangle(i * seg, 0, seg, Height);
                bool selected = i == SelectedIndex;
                if (selected)
                {
                    Rectangle inner = new Rectangle(cell.X + Ui.S(2), cell.Y + Ui.S(2), cell.Width - Ui.S(4), cell.Height - Ui.S(4));
                    Ui.FillRound(g, Color.White, inner, Ui.S(7));
                    Ui.StrokeRound(g, Theme.Border, inner, Ui.S(7), 1f);
                }
                else if (i == _hoverIndex)
                {
                    Rectangle inner = new Rectangle(cell.X + Ui.S(2), cell.Y + Ui.S(2), cell.Width - Ui.S(4), cell.Height - Ui.S(4));
                    Ui.FillRound(g, Ui.Alpha(Color.White, 110), inner, Ui.S(7));
                }

                Color color = Theme.TextSub;
                if (selected && ItemColors != null && i < ItemColors.Length) color = ItemColors[i];
                else if (selected) color = Theme.Accent;
                Font font = selected ? Theme.F(9f, FontStyle.Bold) : Theme.F(9f, FontStyle.Regular);
                Ui.TextCenter(g, Items[i], font, color, cell);
            }
        }
    }
}
