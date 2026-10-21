using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class MainForm
    {
        private void CountSummary(out int overdue, out int today, out int pending)
        {
            overdue = 0;
            today = 0;
            pending = 0;
            DateTime now = DateTime.Now;
            foreach (TaskItem t in _store.Tasks)
            {
                if (t.IsDone) continue;
                pending++;
                if (!t.HasDue) continue;
                if (t.DueAt < now) overdue++;
                else if (t.DueAt.Date == now.Date) today++;
            }
        }

        private void PaintHeader(Graphics g)
        {
            Rectangle mark = new Rectangle(Ui.S(18), Ui.S(18), Ui.S(30), Ui.S(30));
            using (GraphicsPath path = Ui.Round(mark, Ui.S(9)))
            using (LinearGradientBrush brush = new LinearGradientBrush(mark, Theme.Accent, Theme.AccentDark, 45f))
            {
                g.FillPath(brush, path);
            }
            Ui.Check(g, new RectangleF(mark.X, mark.Y + Ui.S(1), mark.Width, mark.Height), Color.White, Ui.SF(2));

            Ui.TextLeft(g, "我的待办", Theme.AppTitle, Theme.TextMain,
                new Rectangle(Ui.S(58), Ui.S(11), Width - Ui.S(58) - Ui.S(104), Ui.S(28)));

            int overdue;
            int today;
            int pending;
            CountSummary(out overdue, out today, out pending);

            int x = Ui.S(58);
            int y = Ui.S(38);
            int h = Ui.S(19);
            bool first = true;
            if (overdue > 0)
            {
                x = DrawSummarySegment(g, x, y, h, "逾期 " + overdue, Theme.Danger, true, first);
                first = false;
            }
            if (today > 0)
            {
                x = DrawSummarySegment(g, x, y, h, "今天 " + today, Theme.Accent, true, first);
                first = false;
            }
            DrawSummarySegment(g, x, y, h, "待完成 " + pending, Theme.TextSub, false, first);
        }

        private int DrawSummarySegment(Graphics g, int x, int y, int h, string text, Color color, bool bold, bool first)
        {
            Font font = bold ? Theme.F(8.5f, FontStyle.Bold) : Theme.F(8.5f, FontStyle.Regular);
            if (!first)
            {
                SizeF dot = Ui.Measure(g, "·", font);
                Ui.TextLeft(g, "·", font, Theme.TextFaint, new Rectangle(x, y, (int)Math.Ceiling(dot.Width) + Ui.S(6), h));
                x += (int)Math.Ceiling(dot.Width) + Ui.S(6);
            }
            SizeF size = Ui.Measure(g, text, font);
            int width = (int)Math.Ceiling(size.Width) + Ui.S(4);
            Ui.TextLeft(g, text, font, color, new Rectangle(x, y, width, h));
            return x + width + Ui.S(2);
        }

        private void PaintFooter(Graphics g)
        {
            int footerTop = Height - Ui.S(56);
            using (Pen pen = new Pen(Theme.Border, 1f))
            {
                g.DrawLine(pen, Ui.S(16), footerTop, Width - Ui.S(16), footerTop);
            }
            int settingsWidth = _settingsButton == null ? Ui.S(28) : _settingsButton.Width;
            Rectangle versionRect = new Rectangle(Ui.S(120), footerTop + Ui.S(20),
                Math.Max(Ui.S(20), Width - Ui.S(120) - settingsWidth - Ui.S(20)), Ui.S(18));
            Ui.TextRight(g, "Boss 待办 v1.0", Theme.Tiny, Theme.TextFaint, versionRect);
        }

        private void ShowToast(string message)
        {
            if (_toast == null) return;
            _toast.ShowMessage(message);
            _toast.BringToFront();
        }

        private void ShowToastLater(string message, int delay)
        {
            Timer timer = new Timer();
            timer.Interval = Math.Max(100, delay);
            timer.Tick += delegate
            {
                timer.Stop();
                timer.Dispose();
                ShowToast(message);
            };
            timer.Start();
        }

        private void UpdateAutoSortVisibility()
        {
            if (_autoSortButton == null) return;
            bool show = _store.HasManualOrder;
            _autoSortButton.Visible = show;
            if (show)
            {
                using (Graphics g = CreateGraphics())
                {
                    _autoSortButton.Width = _autoSortButton.PreferredWidth(g) + Ui.S(4);
                }
                _autoSortButton.Location = new Point(ClientSize.Width - Ui.S(16) - _autoSortButton.Width, Ui.S(124));
                _autoSortButton.BringToFront();
            }
        }

        private void UpdateChipSelection()
        {
            foreach (PillButton chip in _chips)
            {
                chip.Selected = chip.Tag != null && (string)chip.Tag == _filter;
                chip.Invalidate();
            }
        }

        private void SetFilter(string filter)
        {
            _filter = filter;
            _scroll = 0;
            if (_bar != null) _bar.Value = 0;
            UpdateChipSelection();
            RebuildList(false);
        }
    }
}
