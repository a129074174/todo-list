using System;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class MainForm
    {
        private void ToggleAlwaysOnTop()
        {
            Settings.AlwaysOnTop = !Settings.AlwaysOnTop;
            TopMost = Settings.AlwaysOnTop;
            _pinButton.Active = Settings.AlwaysOnTop;
            _pinButton.Invalidate();
            _store.SaveSettings();
            ShowToast(Settings.AlwaysOnTop ? "窗口已置顶" : "已取消窗口置顶");
        }

        private void ResetManualOrder()
        {
            _store.ClearManualOrder();
            _store.SaveTasks();
            RebuildList(false);
            ShowToast("已恢复四象限自动排序");
        }

        private void SpeakSummary(bool automatic)
        {
            if (_demo) return;
            string text = PriorityEngine.BuildAnnouncement(_store.Tasks, DateTime.Now, Settings.UrgentDays);
            _speech.Speak(text, Settings.SpeechRate, Settings.SpeechVolume, Settings.VoiceName);
            if (!automatic)
            {
                ShowToast(SpeechService.HasChineseVoice ? "正在朗读今日总结" : "未检测到中文语音，使用系统默认语音");
            }
            else
            {
                Settings.LastSpokenDate = SpeechService.DateKey(DateTime.Today);
                _store.SaveSettings();
            }
        }

        private void OpenSettings()
        {
            using (SettingsDialog dialog = new SettingsDialog(_store, _speech))
            {
                dialog.ShowDialog(this);
            }
            ApplySettingsAfterDialog();
        }

        private void ApplySettingsAfterDialog()
        {
            TopMost = Settings.AlwaysOnTop;
            _pinButton.Active = Settings.AlwaysOnTop;
            _pinButton.Invalidate();
            _now = DateTime.Now;
            RebuildList(true);
            RefreshTrayMenuState();
            ShowToast("设置已保存");
        }

        private void SetupTray()
        {
            if (_demo) return;
            _tray = new NotifyIcon();
            try { _tray.Icon = IconFactory.Create(16); }
            catch { }
            _tray.Text = "Boss 待办";
            _tray.Visible = true;
            _tray.DoubleClick += delegate { RestoreFromTray(); };

            _trayMenu = MenuFactory.Create();
            _trayMenu.Opening += delegate { RefreshTrayMenuState(); };

            ToolStripMenuItem show = new ToolStripMenuItem("打开便签");
            show.Font = Theme.F(9.4f, FontStyle.Bold);
            show.Click += delegate { RestoreFromTray(); };
            _trayMenu.Items.Add(show);

            ToolStripMenuItem add = new ToolStripMenuItem("新建任务");
            add.Click += delegate
            {
                RestoreFromTray();
                OpenNewTaskDialog(string.Empty);
            };
            _trayMenu.Items.Add(add);

            ToolStripMenuItem speak = new ToolStripMenuItem("朗读今日总结");
            speak.Click += delegate { SpeakSummary(false); };
            _trayMenu.Items.Add(speak);
            _trayMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem startup = new ToolStripMenuItem("开机启动并语音播报");
            startup.Name = "startup";
            startup.Click += delegate
            {
                bool next = !AutoStart.IsEnabled();
                AutoStart.SetEnabled(next);
                Settings.AutoStart = next;
                _store.SaveSettings();
                ShowToast(next ? "已开启开机播报" : "已关闭开机播报");
            };
            _trayMenu.Items.Add(startup);

            ToolStripMenuItem settings = new ToolStripMenuItem("设置…");
            settings.Click += delegate
            {
                RestoreFromTray();
                OpenSettings();
            };
            _trayMenu.Items.Add(settings);
            _trayMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exit = new ToolStripMenuItem("退出 Boss 待办");
            exit.Click += delegate { CloseApplication(); };
            _trayMenu.Items.Add(exit);

            _tray.ContextMenuStrip = _trayMenu;
        }

        private void RefreshTrayMenuState()
        {
            if (_trayMenu == null) return;
            foreach (ToolStripItem item in _trayMenu.Items)
            {
                if (item.Name == "startup")
                {
                    ToolStripMenuItem mi = item as ToolStripMenuItem;
                    if (mi != null) mi.Checked = AutoStart.IsEnabled();
                }
            }
        }
    }
}
