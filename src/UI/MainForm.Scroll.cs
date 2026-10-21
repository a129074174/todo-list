using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class MainForm
    {
        public Point ListPointToClient(Point screen)
        {
            return _list == null ? Point.Empty : _list.PointToClient(screen);
        }

        public Rectangle ListClientRect
        {
            get { return _list == null ? Rectangle.Empty : _list.ClientRectangle; }
        }

        private void LayoutList(int keepScroll)
        {
            if (_list == null) return;
            int width = _list.ClientSize.Width;
            int y = 0;
            List<TaskItem> pending = GetFilteredPending();

            if (_emptyState != null)
            {
                _emptyState.Bounds = new Rectangle(0, 0, width, Math.Max(Ui.S(180), _list.ClientSize.Height));
                _emptyState.Tag = null;
                y = _emptyState.Height;
            }
            else
            {
                foreach (Quadrant quadrant in new Quadrant[] {
                    Quadrant.UrgentImportant, Quadrant.UrgentUnimportant,
                    Quadrant.NotUrgentImportant, Quadrant.NotUrgentUnimportant })
                {
                    GroupHeader header;
                    if (!_headers.TryGetValue(quadrant, out header)) continue;
                    Place(header, y, width, Ui.S(28));
                    y += Ui.S(30);

                    foreach (TaskItem task in pending)
                    {
                        if (PriorityEngine.GetQuadrant(task, _now, Settings.UrgentDays) != quadrant) continue;
                        TaskCard card;
                        if (!_cards.TryGetValue(task.Id, out card)) continue;
                        int height = TaskCard.HeightFor(task);
                        Place(card, y, width, height);
                        y += height + Ui.S(8);
                    }
                    y += Ui.S(4);
                }
            }

            if (_doneToggle != null)
            {
                Place(_doneToggle, y, width, Ui.S(32));
                y += Ui.S(34);
                if (_showDone)
                {
                    List<TaskItem> done = new List<TaskItem>();
                    foreach (TaskItem t in _store.Tasks) { if (t.IsDone) done.Add(t); }
                    done.Sort(delegate(TaskItem a, TaskItem b) { return b.CompletedAt.CompareTo(a.CompletedAt); });
                    foreach (TaskItem task in done)
                    {
                        TaskCard card;
                        if (!_cards.TryGetValue(task.Id, out card)) continue;
                        int height = TaskCard.HeightFor(task);
                        Place(card, y, width, height);
                        y += height + Ui.S(8);
                    }
                }
            }

            y += Ui.S(8);
            int viewport = Math.Max(1, _list.ClientSize.Height);
            _list.ContentHeight = y;
            int maxScroll = Math.Max(0, y - viewport);
            if (keepScroll < 0) keepScroll = 0;
            if (keepScroll > maxScroll) keepScroll = maxScroll;
            _scroll = keepScroll;

            _bar.Maximum = Math.Max(y, viewport);
            _bar.Viewport = viewport;
            _bar.Value = _scroll;

            ApplyScroll();
        }

        private void Place(Control control, int baseY, int width, int height)
        {
            control.Tag = baseY;
            control.SetBounds(0, baseY - _scroll, width, height);
        }

        private void ApplyScroll()
        {
            if (_list == null) return;
            foreach (Control control in _list.Controls)
            {
                if (control == _emptyState) continue;
                object tag = control.Tag;
                if (tag is int) control.Top = (int)tag - _scroll;
            }
        }

        public void ScrollBy(int delta)
        {
            if (_list == null) return;
            int maxScroll = Math.Max(0, _list.ContentHeight - _list.ClientSize.Height);
            if (maxScroll <= 0) return;
            _scroll += delta;
            if (_scroll < 0) _scroll = 0;
            if (_scroll > maxScroll) _scroll = maxScroll;
            _bar.Value = _scroll;
            ApplyScroll();
        }
    }
}
