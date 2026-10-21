using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class SettingsDialog
    {
        private SettingsWheelFilter _wheelFilter;

        private void EnableSmoothScrolling()
        {
            Shown += delegate
            {
                if (_wheelFilter == null)
                {
                    _wheelFilter = new SettingsWheelFilter(this);
                    Application.AddMessageFilter(_wheelFilter);
                }
            };
            FormClosed += delegate
            {
                if (_wheelFilter != null)
                {
                    Application.RemoveMessageFilter(_wheelFilter);
                    _wheelFilter = null;
                }
            };
        }

        internal void ScrollSettings(int delta)
        {
            if (_content == null) return;
            SetSettingsScroll(_settingsScroll + delta);
        }

        private static string FormatRate(int rate)
        {
            if (rate > 0) return "+" + rate;
            return rate.ToString();
        }

        private void ChangeRate(int delta)
        {
            _rate = Math.Max(-5, Math.Min(5, _rate + delta));
            _store.Settings.SpeechRate = _rate;
            if (_rateValue != null) _rateValue.Text = FormatRate(_rate);
            _store.SaveSettings();
            SetStatus("语速已调整为 " + FormatRate(_rate), true);
        }

        private void TestVoice()
        {
            string text = PriorityEngine.BuildAnnouncement(_store.Tasks, DateTime.Now, _store.Settings.UrgentDays);
            if (_speech != null)
            {
                _speech.Speak(text, _rate, _store.Settings.SpeechVolume, _store.Settings.VoiceName);
            }
            SetStatus(SpeechService.HasChineseVoice
                ? "正在试听：" + SpeechService.ChineseVoiceName
                : "未检测到中文语音，将尝试系统默认语音", true);
        }

        private void OpenDataFolder()
        {
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", "\"" + _store.DataDir + "\""));
            }
            catch (Exception ex)
            {
                SetStatus("无法打开目录：" + ex.Message, false);
            }
        }

        private void SetStatus(string message, bool success)
        {
            if (_statusLabel == null) return;
            _statusLabel.Text = message;
            _statusLabel.ForeColor = success ? Theme.Success : Theme.Danger;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape) Close();
        }
    }
}
