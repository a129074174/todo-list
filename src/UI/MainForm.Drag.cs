using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class MainForm
    {
        private void HandleCardDrag(TaskCard card)
        {
            if (!_dragActive || _dragCard != card) return;
            int mouseY = Cursor.Position.Y;
            if (Math.Abs(mouseY - _dragStartScreenY) < Ui.S(2)) return;

            Quadrant quadrant = PriorityEngine.GetQuadrant(card.Task, _now, Settings.UrgentDays);
            List<TaskItem> ordered = new List<TaskItem>();
            foreach (TaskItem t in PriorityEngine.SortPending(_store.Tasks, _now, Settings.UrgentDays))
            {
                if (PriorityEngine.GetQuadrant(t, _now, Settings.UrgentDays) == quadrant) ordered.Add(t);
            }
            int current = ordered.IndexOf(card.Task);
            if (current < 0) return;

            List<TaskCard> cards = new List<TaskCard>();
            foreach (TaskItem t in ordered)
            {
                TaskCard c;
                if (_cards.TryGetValue(t.Id, out c)) cards.Add(c);
            }
            int target = 0;
            foreach (TaskCard c in cards)
            {
                int center = c.PointToScreen(new Point(0, c.Height / 2)).Y;
                if (mouseY > center) target++;
            }

            int insert = target;
            ordered.RemoveAt(current);
            if (insert > current) insert--;
            if (insert < 0) insert = 0;
            if (insert > ordered.Count) insert = ordered.Count;
            if (insert == current) return;

            ordered.Insert(insert, card.Task);
            _store.RenumberManualOrder(ordered);
            _store.SaveTasks();
            LayoutList(_scroll);
            card.IsDragging = true;
            UpdateAutoSortVisibility();
        }

        private void FinishCardDrag(TaskCard card)
        {
            bool reordered = _dragActive;
            _dragActive = false;
            _dragCard = null;
            if (card != null) card.IsDragging = false;
            if (reordered)
            {
                _store.SaveTasks();
                RebuildList(true);
                ShowToast("顺序已保存 · 可点击「恢复自动排序」");
            }
        }
    }

    /// <summary>把滚轮事件转发给列表（无论焦点在哪个子控件）。</summary>
    public class MouseWheelRouter : IMessageFilter
    {
        private readonly MainForm _form;

        public MouseWheelRouter(MainForm form)
        {
            _form = form;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != 0x020A) return false;
            if (_form == null || _form.IsDisposed || !_form.Visible) return false;
            Point screen = Cursor.Position;
            Point client = _form.ListPointToClient(screen);
            if (!_form.ListClientRect.Contains(client)) return false;
            int delta = (short)((long)m.WParam >> 16);
            _form.ScrollBy(-delta / 120 * Ui.S(52));
            return true;
        }
    }
}
