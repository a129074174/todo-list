using System;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    /// <summary>自绘任务卡片：象限色条、勾选、到期红字、悬停操作、拖拽。</summary>
    public partial class TaskCard : SoftControl
    {
        private bool _hover;
        private int _hoverAction;
        private bool _checkHover;
        private bool _dragStarted;
        private bool _leftDown;
        private Point _downPoint;

        public TaskItem Task { get; private set; }
        public DateTime Now { get; set; }
        public int UrgentDays { get; set; }
        public bool IsDragging { get; set; }

        public event EventHandler ToggleDone;
        public event EventHandler EditRequested;
        public event EventHandler DeleteRequested;
        public event EventHandler PinRequested;
        public event EventHandler DragBegin;
        public event EventHandler DragMove;
        public event EventHandler DragEnd;

        private Rectangle _checkRect;
        private Rectangle _pinRect;
        private Rectangle _editRect;
        private Rectangle _deleteRect;
        private Rectangle _inner;

        public TaskCard(TaskItem task, DateTime now, int urgentDays)
        {
            Task = task;
            Now = now;
            UrgentDays = urgentDays;
            BackColor = Theme.Bg;
            TabStop = false;
            Height = HeightFor(task);
            Cursor = Cursors.Default;
        }

        public static int HeightFor(TaskItem task)
        {
            bool hasNote = task != null && !string.IsNullOrEmpty(task.Note);
            return Ui.S(hasNote ? 94 : 76);
        }

        public void UpdateTime(DateTime now, int urgentDays)
        {
            Now = now;
            UrgentDays = urgentDays;
            Invalidate();
        }

        private bool HasNote
        {
            get { return Task != null && !string.IsNullOrEmpty(Task.Note); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Ui.Quality(g);
            g.Clear(BackColor);
            PaintCard(g);
        }

        private void PaintCard(Graphics g)
        {
            bool done = Task.IsDone;
            Quadrant q = PriorityEngine.GetQuadrant(Task, Now, UrgentDays);
            bool soon = !done && PriorityEngine.IsDueSoon(Task, Now);
            Color accent = done ? Theme.TextFaint : Theme.QuadrantColor(q);

            _inner = new Rectangle(Ui.S(2), Ui.S(1), Width - Ui.S(4), Height - Ui.S(4));
            if (_hover || IsDragging) Ui.DrawSoftShadow(g, _inner, Ui.S(12), Ui.S(3), 46);

            Color cardBack = IsDragging ? Theme.AccentSoft : (_hover ? Theme.CardHover : Theme.Card);
            Ui.FillRound(g, cardBack, _inner, Ui.S(12));
            Color border = soon ? Ui.Alpha(Theme.Danger, 90) : (_hover ? Theme.BorderStrong : Theme.Border);
            Ui.StrokeRound(g, border, _inner, Ui.S(12), 1f);

            // 左侧象限色条（1 天内到期/逾期用红色并加粗）
            int barW = soon ? Ui.S(5) : Ui.S(4);
            Rectangle bar = new Rectangle(_inner.X + Ui.S(1), _inner.Y + Ui.S(12), barW, _inner.Height - Ui.S(24));
            Ui.FillRound(g, done ? Theme.BorderStrong : (soon ? Theme.Danger : accent), bar, barW / 2);

            int d = Ui.S(20);
            _checkRect = new Rectangle(_inner.X + Ui.S(14), _inner.Y + (_inner.Height - d) / 2, d, d);
            PaintCheck(g, done);

            int btn = Ui.S(24);
            int gap = Ui.S(2);
            int right = _inner.Right - Ui.S(8);
            _deleteRect = new Rectangle(right - btn, _inner.Y + (_inner.Height - btn) / 2, btn, btn);
            _editRect = new Rectangle(_deleteRect.X - gap - btn, _deleteRect.Y, btn, btn);
            _pinRect = new Rectangle(_editRect.X - gap - btn, _deleteRect.Y, btn, btn);

            int titleX = _checkRect.Right + Ui.S(12);
            int titleW = Math.Max(Ui.S(40), _pinRect.X - Ui.S(8) - titleX);
            int titleTop = _inner.Y + Ui.S(HasNote ? 10 : 12);
            Rectangle titleRect = new Rectangle(titleX, titleTop, titleW, Ui.S(21));
            PaintTitle(g, titleRect, done);

            int metaY = _inner.Y + Ui.S(HasNote ? 34 : 38);
            PaintMeta(g, titleX, metaY, done, q, accent, soon);

            if (HasNote)
            {
                Rectangle noteRect = new Rectangle(titleX, _inner.Y + Ui.S(57), _inner.Right - Ui.S(14) - titleX, Ui.S(20));
                Ui.TextLeft(g, Task.Note.Replace("\r", " ").Replace("\n", " "), Theme.Tiny, Theme.TextFaint, noteRect);
            }

            PaintActions(g);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && ContextMenuStrip != null)
            {
                ContextMenuStrip.Dispose();
                ContextMenuStrip = null;
            }
            base.Dispose(disposing);
        }
    }
}
