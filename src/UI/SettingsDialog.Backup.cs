using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class SettingsDialog
    {
        private void ExportBackup()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "导出待办备份";
                dialog.Filter = "Boss 待办备份 (*.json)|*.json|所有文件 (*.*)|*.*";
                dialog.FileName = "BossTodo-backup-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".json";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                BackupData data = new BackupData();
                data.Tasks = _store.Tasks;
                data.Settings = _store.Settings;
                if (JsonStore.Save(dialog.FileName, data))
                {
                    SetStatus("已导出：" + Path.GetFileName(dialog.FileName), true);
                }
                else
                {
                    SetStatus("导出失败，请检查目标文件夹权限", false);
                }
            }
        }

        private void ImportBackup()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "导入待办备份";
                dialog.Filter = "Boss 待办备份 (*.json)|*.json|所有文件 (*.*)|*.*";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                BackupData data = JsonStore.Load<BackupData>(dialog.FileName, null);
                if (data == null || data.Tasks == null)
                {
                    SetStatus("文件格式不正确，无法导入", false);
                    return;
                }
                bool confirmed = ConfirmDialog.Show(this, "导入备份",
                    "将用备份覆盖当前 " + _store.Tasks.Count + " 条任务，是否继续？", "覆盖导入", true);
                if (!confirmed) return;

                _store.Tasks.Clear();
                foreach (TaskItem task in data.Tasks)
                {
                    if (task == null) continue;
                    if (string.IsNullOrEmpty(task.Id)) task.Id = Guid.NewGuid().ToString("N");
                    if (task.CreatedAt == default(DateTime)) task.CreatedAt = DateTime.Now;
                    _store.Tasks.Add(task);
                }
                _store.SaveTasks();

                if (data.Settings != null)
                {
                    _store.Settings.CopyFrom(data.Settings);
                    _store.SaveSettings();
                    AutoStart.SetEnabled(_store.Settings.AutoStart);
                }

                RefreshControlsFromSettings();
                SetStatus("导入成功，共 " + _store.Tasks.Count + " 条任务", true);
            }
        }

        private void RefreshControlsFromSettings()
        {
            _rate = _store.Settings.SpeechRate;
            if (_rateValue != null) _rateValue.Text = FormatRate(_rate);
            if (_autoStartToggle != null) _autoStartToggle.SetCheckedSilently(_store.Settings.AutoStart);
            if (_speakStartupToggle != null) _speakStartupToggle.SetCheckedSilently(_store.Settings.SpeakOnStartup);
            if (_speakEveryToggle != null) _speakEveryToggle.SetCheckedSilently(_store.Settings.SpeakOnEveryLaunch);
            if (_topMostToggle != null) _topMostToggle.SetCheckedSilently(_store.Settings.AlwaysOnTop);
            if (_closeToTrayToggle != null) _closeToTrayToggle.SetCheckedSilently(_store.Settings.CloseToTray);
            if (_urgentSegment != null) _urgentSegment.SetSelectedSilently(IndexForDays(_store.Settings.UrgentDays));
        }

        private class SettingsWheelFilter : IMessageFilter
        {
            private readonly SettingsDialog _dialog;

            public SettingsWheelFilter(SettingsDialog dialog)
            {
                _dialog = dialog;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != 0x020A) return false;
                if (_dialog == null || _dialog.IsDisposed || !_dialog.Visible) return false;
                if (_dialog._content == null) return false;
                Point client = _dialog._content.PointToClient(Cursor.Position);
                if (!_dialog._content.ClientRectangle.Contains(client)) return false;
                int delta = (short)((long)m.WParam >> 16);
                _dialog.ScrollSettings(-delta / 120 * Ui.S(56));
                return true;
            }
        }
    }
}
