using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class MainForm
    {
        private List<TaskItem> GetFilteredPending()
        {
            List<TaskItem> list = new List<TaskItem>();
            foreach (TaskItem t in _store.Tasks)
            {
                if (t.IsDone) continue;
                if (MatchesFilter(t)) list.Add(t);
            }
            return PriorityEngine.SortPending(list, _now, Settings.UrgentDays);
        }

        private bool MatchesFilter(TaskItem t)
        {
            if (_filter == "all") return true;
            if (_filter == "today") return t.HasDue && t.DueAt.Date == DateTime.Today;
            if (_filter == "overdue") return t.HasDue && t.DueAt < _now;
            if (_filter == "important") return t.Importance != Importance.Low;
            return true;
        }

        private void RebuildList(bool preserveScroll)
        {
            if (_list == null) return;
            int keepScroll = preserveScroll ? _scroll : 0;
            _dragActive = false;
            _dragCard = null;

            _list.SuspendLayout();
            while (_list.Controls.Count > 0)
            {
                Control child = _list.Controls[0];
                _list.Controls.RemoveAt(0);
                child.Dispose();
            }
            _cards.Clear();
            _headers.Clear();
            _emptyState = null;
            _doneToggle = null;

            List<TaskItem> pending = GetFilteredPending();
            if (pending.Count == 0)
            {
                EmptyState empty = new EmptyState();
                empty.BackColor = Theme.Bg;
                empty.TitleText = _store.Tasks.Count == 0 ? "还没有待办" : "这个分类下没有任务";
                empty.SubText = _store.Tasks.Count == 0
                    ? "在上方输入框写下第一件事，回车即可添加"
                    : "换个筛选条件看看，或在上方添加一条新任务";
                empty.Bounds = new Rectangle(0, 0, _list.ClientSize.Width, Math.Max(Ui.S(180), _list.ClientSize.Height));
                _emptyState = empty;
                _list.Controls.Add(empty);
            }
            else
            {
                foreach (Quadrant quadrant in new Quadrant[] {
                    Quadrant.UrgentImportant, Quadrant.UrgentUnimportant,
                    Quadrant.NotUrgentImportant, Quadrant.NotUrgentUnimportant })
                {
                    List<TaskItem> group = new List<TaskItem>();
                    foreach (TaskItem t in pending)
                    {
                        if (PriorityEngine.GetQuadrant(t, _now, Settings.UrgentDays) == quadrant) group.Add(t);
                    }
                    if (group.Count == 0) continue;

                    GroupHeader header = new GroupHeader();
                    header.Title = PriorityEngine.QuadrantName(quadrant);
                    header.Count = group.Count;
                    header.Accent = Theme.QuadrantColor(quadrant);
                    header.BackColor = Theme.Bg;
                    _headers[quadrant] = header;
                    _list.Controls.Add(header);

                    foreach (TaskItem task in group)
                    {
                        TaskCard card = CreateCard(task);
                        _cards[task.Id] = card;
                        _list.Controls.Add(card);
                    }
                }
            }

            List<TaskItem> done = new List<TaskItem>();
            foreach (TaskItem t in _store.Tasks)
            {
                if (t.IsDone) done.Add(t);
            }
            done.Sort(delegate(TaskItem a, TaskItem b) { return b.CompletedAt.CompareTo(a.CompletedAt); });

            if (done.Count > 0)
            {
                SectionToggle toggle = new SectionToggle();
                toggle.Text = "已完成";
                toggle.Count = done.Count;
                toggle.Expanded = _showDone;
                toggle.BackColor = Theme.Bg;
                toggle.Click += delegate
                {
                    _showDone = !_showDone;
                    RebuildList(true);
                };
                _doneToggle = toggle;
                _list.Controls.Add(toggle);

                if (_showDone)
                {
                    foreach (TaskItem task in done)
                    {
                        TaskCard card = CreateCard(task);
                        _cards[task.Id] = card;
                        _list.Controls.Add(card);
                    }
                }
            }

            LayoutList(keepScroll);
            _list.ResumeLayout();
            UpdateChipSelection();
            UpdateAutoSortVisibility();
        }

        private TaskCard CreateCard(TaskItem task)
        {
            TaskCard card = new TaskCard(task, _now, Settings.UrgentDays);
            card.Width = _list.ClientSize.Width;
            card.ToggleDone += delegate { ToggleTaskDone(task); };
            card.EditRequested += delegate { OpenEditDialog(task); };
            card.DeleteRequested += delegate { DeleteTask(task); };
            card.PinRequested += delegate { TogglePin(task); };
            card.ContextMenuStrip = BuildCardMenu(task);
            card.DragBegin += delegate
            {
                if (task.IsDone) return;
                if (_filter != "all")
                {
                    ShowToast("切换到「全部」后即可拖拽排序");
                    return;
                }
                _dragCard = card;
                _dragActive = true;
                card.IsDragging = true;
                _dragStartScreenY = Cursor.Position.Y;
            };
            card.DragMove += delegate { HandleCardDrag(card); };
            card.DragEnd += delegate { FinishCardDrag(card); };
            return card;
        }
    }
}
