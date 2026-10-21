using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class MainForm
    {
        private void SetImportance(TaskItem task, Importance importance)
        {
            if (task == null) return;
            task.Importance = importance;
            _store.SaveTasks();
            RebuildList(true);
            ShowToast("重要程度：" + Theme.ImportanceName(importance));
        }

        private ContextMenuStrip BuildCardMenu(TaskItem task)
        {
            ContextMenuStrip menu = MenuFactory.Create();

            menu.Items.Add("编辑任务", null, delegate { OpenEditDialog(task); });
            menu.Items.Add(task.IsPinned ? "取消置顶" : "置顶显示", null, delegate { TogglePin(task); });
            menu.Items.Add("上移一位", null, delegate { MoveTask(task, -1); });
            menu.Items.Add("下移一位", null, delegate { MoveTask(task, 1); });
            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem importance = new ToolStripMenuItem("重要程度");
            foreach (Importance value in new Importance[] { Importance.High, Importance.Medium, Importance.Low })
            {
                Importance captured = value;
                ToolStripMenuItem item = new ToolStripMenuItem(
                    Theme.ImportanceName(value) + (task.Importance == value ? "   ✓" : string.Empty));
                item.Click += delegate { SetImportance(task, captured); };
                importance.DropDownItems.Add(item);
            }
            menu.Items.Add(importance);
            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem delete = new ToolStripMenuItem("删除任务");
            delete.ForeColor = Theme.Danger;
            delete.Click += delegate { DeleteTask(task); };
            menu.Items.Add(delete);
            return menu;
        }

        private void MoveTask(TaskItem task, int delta)
        {
            if (task == null) return;
            Quadrant quadrant = PriorityEngine.GetQuadrant(task, _now, Settings.UrgentDays);
            List<TaskItem> ordered = new List<TaskItem>();
            foreach (TaskItem t in PriorityEngine.SortPending(_store.Tasks, _now, Settings.UrgentDays))
            {
                if (PriorityEngine.GetQuadrant(t, _now, Settings.UrgentDays) == quadrant) ordered.Add(t);
            }
            int index = ordered.IndexOf(task);
            int target = index + delta;
            if (index < 0 || target < 0 || target >= ordered.Count) return;

            ordered.RemoveAt(index);
            ordered.Insert(target, task);
            _store.RenumberManualOrder(ordered);
            _store.SaveTasks();
            RebuildList(true);
            ShowToast("已调整顺序");
        }
    }
}
