using System;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class EditTaskDialog
    {
        private void LoadValues()
        {
            _titleBox.Text = _isNew ? string.Empty : _source.Title;
            _noteBox.Text = _isNew ? string.Empty : _source.Note;

            if (_isNew)
            {
                _datePicker.Value = DateTime.Today;
                _timePicker.Value = new DateTime(2000, 1, 1, 18, 0, 0);
                _importance.SetSelectedSilently(1);
                _dueToggle.SetCheckedSilently(false);
            }
            else
            {
                _importance.SetSelectedSilently(IndexFromImportance(_source.Importance));
                _dueToggle.SetCheckedSilently(_source.HasDue);
                if (_source.HasDue)
                {
                    _datePicker.Value = _source.DueAt.Date;
                    _timePicker.Value = _source.DueAt;
                }
                else
                {
                    _datePicker.Value = DateTime.Today;
                    _timePicker.Value = new DateTime(2000, 1, 1, 18, 0, 0);
                }
            }
            ApplyDueEnabled();
            UpdateChipState();

            Shown += delegate
            {
                SetCue(_titleBox, "例如：周五前提交季度报告");
                SetCue(_noteBox, "补充说明（可选）");
                _titleBox.Focus();
                _titleBox.SelectionStart = _titleBox.Text.Length;
            };
        }

        private static int IndexFromImportance(Importance imp)
        {
            if (imp == Importance.High) return 0;
            if (imp == Importance.Low) return 2;
            return 1;
        }

        public void PrefillTitle(string title)
        {
            _titleBox.Text = title ?? string.Empty;
            _titleBox.SelectionStart = _titleBox.Text.Length;
        }

        private void ApplyDueEnabled()
        {
            bool on = _dueToggle.Checked;
            if (_datePicker != null) _datePicker.Enabled = on;
            if (_timePicker != null) _timePicker.Enabled = on;
            UpdateChipState();
        }

        private void ApplyPreset(string text)
        {
            DateTime now = DateTime.Now;
            DateTime target;
            if (text == "今天")
            {
                if (now.Hour >= 17)
                {
                    DateTime next = now.AddHours(1);
                    target = new DateTime(next.Year, next.Month, next.Day, next.Hour, 0, 0);
                }
                else
                {
                    target = new DateTime(now.Year, now.Month, now.Day, 18, 0, 0);
                }
            }
            else if (text == "明天")
            {
                target = DateTime.Today.AddDays(1).AddHours(18);
            }
            else if (text == "后天")
            {
                target = DateTime.Today.AddDays(2).AddHours(18);
            }
            else
            {
                DateTime d = DateTime.Today;
                int diff = ((int)DayOfWeek.Monday - (int)d.DayOfWeek + 7) % 7;
                if (diff == 0) diff = 7;
                target = d.AddDays(diff).AddHours(9);
            }

            _datePicker.Value = target.Date;
            _timePicker.Value = target;
            _dueToggle.SetCheckedSilently(true);
            ApplyDueEnabled();
        }

        private void UpdateChipState()
        {
            if (_todayChip == null) return;
            DateTime day = _datePicker.Value.Date;
            _todayChip.Selected = _dueToggle.Checked && day == DateTime.Today;
            _tomorrowChip.Selected = _dueToggle.Checked && day == DateTime.Today.AddDays(1);
            _afterChip.Selected = _dueToggle.Checked && day == DateTime.Today.AddDays(2);
            DateTime today = DateTime.Today;
            int diff = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (diff == 0) diff = 7;
            _nextWeekChip.Selected = _dueToggle.Checked && day == today.AddDays(diff);
            _todayChip.Invalidate();
            _tomorrowChip.Invalidate();
            _afterChip.Invalidate();
            _nextWeekChip.Invalidate();
        }

        private void SaveAndClose()
        {
            string title = _titleBox.Text.Trim();
            if (title.Length == 0)
            {
                _error.Visible = true;
                _titleBox.Focus();
                return;
            }

            TaskItem result = _source == null ? TaskItem.Create(title) : _source.Clone();
            result.Title = title;
            result.Note = _noteBox.Text.Trim();
            result.HasDue = _dueToggle.Checked;
            if (result.HasDue)
            {
                DateTime date = _datePicker.Value.Date;
                DateTime time = _timePicker.Value;
                result.DueAt = date.AddHours(time.Hour).AddMinutes(time.Minute);
            }
            int index = _importance.SelectedIndex;
            if (index < 0 || index > 2) index = 1;
            result.Importance = ImportanceMap[index];

            ResultTask = result;
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                SaveAndClose();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        public static void SetCueBanner(IntPtr handle, string text)
        {
            // EM_SETCUEBANNER = 0x1501
            SendMessage(handle, 0x1501, (IntPtr)1, text);
        }
    }
}
