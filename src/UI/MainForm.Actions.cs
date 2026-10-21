using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class MainForm
    {
        private void WireEvents()
        {
            _wheelFilter = new MouseWheelRouter(this);
            Application.AddMessageFilter(_wheelFilter);
        }

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            string text = _input.Text.Trim();
            if (e.Control || e.Shift)
            {
                OpenNewTaskDialog(text);
                return;
            }
            AddQuickTask();
        }

        private void AddQuickTask()
        {
            string title = _input.Text.Trim();
            if (title.Length == 0)
            {
                _input.Focus();
                ShowToast("先写下要做的事吧");
                return;
            }
            TaskItem task = TaskItem.Create(title);
            task.Importance = Importance.Medium;
            task.CreatedAt = DateTime.Now;
            _store.Add(task);
            _store.SaveTasks();
            _input.Clear();
            _now = DateTime.Now;
            RebuildList(false);
            ShowToast("已添加 · 双击卡片可设置截止时间");
        }

        private void OpenNewTaskDialog(string prefill)
        {
            using (EditTaskDialog dialog = new EditTaskDialog(null))
            {
                if (!string.IsNullOrEmpty(prefill)) dialog.PrefillTitle(prefill);
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                TaskItem task = dialog.ResultTask;
                task.CreatedAt = DateTime.Now;
                _store.Add(task);
                _store.SaveTasks();
                _input.Clear();
                _now = DateTime.Now;
                RebuildList(false);
                ShowToast("任务已创建");
            }
        }

        private void OpenEditDialog(TaskItem task)
        {
            if (task == null) return;
            using (EditTaskDialog dialog = new EditTaskDialog(task))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                if (dialog.DeleteRequested)
                {
                    DeleteTask(task);
                    return;
                }
                TaskItem updated = dialog.ResultTask;
                int index = _store.Tasks.FindIndex(delegate(TaskItem t) { return t.Id == task.Id; });
                if (index >= 0) _store.Tasks[index] = updated;
                else _store.Add(updated);
                _store.SaveTasks();
                _now = DateTime.Now;
                RebuildList(true);
                ShowToast("已保存修改");
            }
        }

        private void DeleteTask(TaskItem task)
        {
            if (task == null) return;
            bool confirmed = ConfirmDialog.Show(this, "删除任务",
                "确定要删除「" + task.Title + "」吗？此操作不可恢复。", "删除", true);
            if (!confirmed) return;
            _store.Remove(task.Id);
            _store.SaveTasks();
            RebuildList(true);
            ShowToast("任务已删除");
        }

        private void ToggleTaskDone(TaskItem task)
        {
            if (task == null) return;
            task.IsDone = !task.IsDone;
            task.CompletedAt = task.IsDone ? DateTime.Now : default(DateTime);
            _store.SaveTasks();
            _now = DateTime.Now;
            RebuildList(true);
            ShowToast(task.IsDone ? "完成一项，真棒" : "已恢复为未完成");
        }

        private void TogglePin(TaskItem task)
        {
            if (task == null) return;
            task.IsPinned = !task.IsPinned;
            _store.SaveTasks();
            RebuildList(true);
            ShowToast(task.IsPinned ? "已置顶" : "已取消置顶");
        }
    }
}
