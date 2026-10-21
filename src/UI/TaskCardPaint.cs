using System;
using System.Drawing;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class TaskCard
    {
        private void PaintCheck(Graphics g, bool done)
        {
            int d = _checkRect.Width;
            if (done)
            {
                using (SolidBrush brush = new SolidBrush(Theme.Accent))
                {
                    g.FillEllipse(brush, _checkRect);
                }
                Ui.Check(g, _checkRect, Color.White, Ui.SF(1.8));
                return;
            }

            if (_checkHover) Ui.FillRound(g, Theme.AccentSoft, _checkRect, d / 2);
            using (Pen pen = new Pen(_checkHover ? Theme.Accent : Theme.BorderStrong, Ui.SF(1.35)))
            {
                g.DrawEllipse(pen, _checkRect);
            }
            if (_checkHover) Ui.Check(g, _checkRect, Theme.Accent, Ui.SF(1.7));
        }

        private void PaintTitle(Graphics g, Rectangle titleRect, bool done)
        {
            Font titleFont = done ? Theme.F(10.6f, FontStyle.Regular) : Theme.CardTitle;
            Color titleColor = done ? Theme.TextFaint : Theme.TextMain;
            Ui.TextLeft(g, Task.Title, titleFont, titleColor, titleRect);
            if (done)
            {
                SizeF ts = Ui.Measure(g, Task.Title, titleFont);
                int lineW = Math.Min((int)Math.Ceiling(ts.Width), titleRect.Width);
                using (Pen pen = new Pen(Theme.TextFaint, 1.1f))
                {
                    g.DrawLine(pen, titleRect.X + 1, titleRect.Y + titleRect.Height / 2,
                        titleRect.X + lineW, titleRect.Y + titleRect.Height / 2);
                }
            }
        }

        private void PaintMeta(Graphics g, int metaX, int metaY, bool done, Quadrant q, Color accent, bool soon)
        {
            int metaH = Ui.S(19);
            Font pillFont = Theme.F(7.9f, FontStyle.Bold);
            string qName = done ? "已完成" : PriorityEngine.QuadrantName(q);
            Color pillColor = done ? Theme.Success : accent;
            Color pillSoft = done ? Theme.SuccessSoft : Theme.QuadrantSoft(q);
            int qW = (int)Math.Ceiling(Ui.Measure(g, qName, pillFont).Width) + Ui.S(14);
            Rectangle qPill = new Rectangle(metaX, metaY, qW, metaH);
            Ui.FillRound(g, pillSoft, qPill, metaH / 2);
            Ui.TextCenter(g, qName, pillFont, pillColor, qPill);
            metaX = qPill.Right + Ui.S(6);

            if (!done && Task.HasDue)
            {
                string dueText = PriorityEngine.DueText(Task, Now);
                bool overdue = PriorityEngine.IsOverdue(Task, Now);
                Font dueFont = soon ? Theme.F(8.1f, FontStyle.Bold) : Theme.F(8.1f, FontStyle.Regular);
                Color dueColor = soon ? Theme.Danger : Theme.TextSub;
                Color dueSoft = soon ? Theme.DangerSoft : Theme.Q4Soft;
                int textW = (int)Math.Ceiling(Ui.Measure(g, dueText, dueFont).Width);
                int iconW = (soon || overdue) ? Ui.S(12) : 0;
                int dueW = textW + iconW + Ui.S(16);
                Rectangle duePill = new Rectangle(metaX, metaY, dueW, metaH);
                Ui.FillRound(g, dueSoft, duePill, metaH / 2);
                if (iconW > 0)
                {
                    Ui.Glyph(g, "\uE7BA", 7f, dueColor,
                        new Rectangle(duePill.X + Ui.S(5), metaY, Ui.S(12), metaH), StringAlignment.Center);
                }
                Ui.TextLeft(g, dueText, dueFont, dueColor,
                    new Rectangle(duePill.X + Ui.S(6) + iconW, metaY, textW + Ui.S(6), metaH));
                metaX = duePill.Right + Ui.S(6);

                string remain = PriorityEngine.RemainingText(Task, Now);
                if (remain != null)
                {
                    int rw = Math.Min(_inner.Right - Ui.S(8) - metaX,
                        (int)Math.Ceiling(Ui.Measure(g, remain, Theme.F(7.9f, FontStyle.Bold)).Width) + Ui.S(4));
                    if (rw > 0)
                    {
                        Ui.TextLeft(g, remain, Theme.F(7.9f, FontStyle.Bold), Theme.Danger,
                            new Rectangle(metaX, metaY, rw, metaH));
                    }
                }
            }
            else if (done && Task.CompletedAt != default(DateTime))
            {
                string doneText = "完成于 " + Task.CompletedAt.ToString("M月d日 HH:mm");
                Ui.TextLeft(g, doneText, Theme.Tiny, Theme.TextFaint,
                    new Rectangle(metaX, metaY, _inner.Right - Ui.S(8) - metaX, metaH));
            }
        }

        private void PaintActions(Graphics g)
        {
            bool showAll = _hover || IsDragging;
            if (!Task.IsDone && (Task.IsPinned || showAll))
            {
                DrawAction(g, _pinRect, "\uE718", Task.IsPinned ? Theme.Accent : Theme.TextFaint,
                    _hoverAction == 1, Task.IsPinned);
            }
            if (showAll)
            {
                DrawAction(g, _editRect, "\uE70F", Theme.TextSub, _hoverAction == 2, false);
                DrawAction(g, _deleteRect, "\uE74D", Theme.TextSub, _hoverAction == 3, false);
            }
        }

        private void DrawAction(Graphics g, Rectangle rect, string glyph, Color color, bool hover, bool active)
        {
            if (active)
            {
                using (SolidBrush brush = new SolidBrush(Ui.Alpha(Theme.Accent, 24)))
                {
                    g.FillEllipse(brush, rect);
                }
            }
            else if (hover)
            {
                using (SolidBrush brush = new SolidBrush(Theme.BgDeep))
                {
                    g.FillEllipse(brush, rect);
                }
            }
            Ui.Glyph(g, glyph, 8.2f, hover ? Theme.Accent : color, rect, StringAlignment.Center);
        }
    }
}
