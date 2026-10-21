using System;
using System.Drawing;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>细长圆角滚动条。</summary>
    public class ThinScrollbar : SoftControl
    {
        private int _maximum;
        private int _viewport;
        private int _value;
        private bool _hover;
        private bool _dragging;
        private int _dragOffset;

        public event EventHandler ValueChanged;

        public int Maximum
        {
            get { return _maximum; }
            set { _maximum = Math.Max(0, value); ClampValue(); Invalidate(); }
        }

        public int Viewport
        {
            get { return _viewport; }
            set { _viewport = Math.Max(0, value); ClampValue(); Invalidate(); }
        }

        public int Value
        {
            get { return _value; }
            set
            {
                int v = Math.Max(0, Math.Min(MaxValue, value));
                if (v == _value) return;
                _value = v;
                Invalidate();
                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
            }
        }

        private int MaxValue
        {
            get { return Math.Max(0, _maximum - _viewport); }
        }

        public ThinScrollbar()
        {
            Cursor = Cursors.Hand;
            TabStop = false;
            Width = Ui.S(8);
        }

        private void ClampValue()
        {
            if (_value > MaxValue) _value = MaxValue;
            if (_value < 0) _value = 0;
        }

        private Rectangle ThumbRect()
        {
            if (_maximum <= _viewport || _viewport <= 0) return Rectangle.Empty;
            int trackH = Height - Ui.S(4);
            int thumbH = Math.Max(Ui.S(30), (int)Math.Round((double)trackH * _viewport / _maximum));
            if (thumbH > trackH) thumbH = trackH;
            int range = Math.Max(1, MaxValue);
            int y = Ui.S(2) + (int)Math.Round((double)(trackH - thumbH) * _value / range);
            int w = Ui.S(5);
            return new Rectangle((Width - w) / 2, y, w, thumbH);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Rectangle thumb = ThumbRect();
            if (thumb == Rectangle.Empty) return;
            if (e.Y >= thumb.Top && e.Y <= thumb.Bottom)
            {
                _dragging = true;
                _dragOffset = e.Y - thumb.Top;
            }
            else
            {
                _dragging = true;
                _dragOffset = thumb.Height / 2;
                UpdateFromMouse(e.Y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging) UpdateFromMouse(e.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
            Invalidate();
        }

        private void UpdateFromMouse(int y)
        {
            Rectangle thumb = ThumbRect();
            if (thumb == Rectangle.Empty) return;
            int trackH = Height - Ui.S(4);
            int movable = Math.Max(1, trackH - thumb.Height);
            int pos = y - Ui.S(2) - _dragOffset;
            if (pos < 0) pos = 0;
            if (pos > movable) pos = movable;
            Value = (int)Math.Round((double)pos * MaxValue / movable);
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

            Rectangle track = new Rectangle((Width - Ui.S(2)) / 2, Ui.S(2), Ui.S(2), Height - Ui.S(4));
            Ui.FillRound(g, Ui.Alpha(Theme.BorderStrong, 70), track, Ui.S(1));

            Rectangle thumb = ThumbRect();
            if (thumb == Rectangle.Empty) return;
            Color color = _dragging ? Theme.Accent : (_hover ? Theme.TextSub : Theme.TextFaint);
            Ui.FillRound(g, color, thumb, thumb.Width / 2);
        }
    }
}
