using System;
using System.Drawing;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>开关（带滑动动画）。</summary>
    public class ToggleSwitch : SoftControl
    {
        private bool _checked;
        private bool _hover;
        private float _anim;
        private readonly Timer _timer;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value) return;
                _checked = value;
                StartAnim();
            }
        }

        public event EventHandler CheckedChanged;

        public ToggleSwitch()
        {
            Cursor = Cursors.Hand;
            TabStop = false;
            Size = new Size(Ui.S(42), Ui.S(24));
            _timer = new Timer();
            _timer.Interval = 15;
            _timer.Tick += delegate
            {
                float target = _checked ? 1f : 0f;
                _anim += (target - _anim) * 0.35f;
                if (Math.Abs(target - _anim) < 0.02f)
                {
                    _anim = target;
                    _timer.Stop();
                }
                Invalidate();
            };
        }

        public void SetCheckedSilently(bool value)
        {
            _checked = value;
            _anim = value ? 1f : 0f;
            Invalidate();
        }

        private void StartAnim()
        {
            _timer.Start();
            Invalidate();
            if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Checked = !Checked;
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

            int h = Height - 1;
            int w = Width - 1;
            Rectangle track = new Rectangle(0, 0, w, h);
            Color trackColor = Ui.Mix(Theme.Track, _hover ? Ui.Mix(Theme.Accent, Color.White, 0.2f) : Theme.Accent, _anim);
            Ui.FillRound(g, trackColor, track, h / 2);

            int knob = h - Ui.S(6);
            int left = Ui.S(3);
            int right = w - knob - Ui.S(3);
            int x = (int)Math.Round(left + (right - left) * _anim);
            Rectangle knobRect = new Rectangle(x, Ui.S(3), knob, knob);
            using (SolidBrush shadow = new SolidBrush(Ui.Alpha(Color.Black, 26)))
            {
                g.FillEllipse(shadow, new Rectangle(knobRect.X, knobRect.Y + 1, knobRect.Width, knobRect.Height));
            }
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, knobRect);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
