using System;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class SettingsDialog
    {
        private void BuildWindowSection()
        {
            int y = (int)(_content.Tag ?? 0);
            int cursor;
            RoundedPanel card = NewCard(y, out cursor);

            _urgentSegment = new SegmentedControl();
            _urgentSegment.Items = new string[] { "1 天", "2 天", "3 天", "5 天" };
            _urgentSegment.Size = new Size(Ui.S(172), Ui.S(30));
            _urgentSegment.BackColor = Color.White;
            _urgentSegment.SetSelectedSilently(IndexForDays(_store.Settings.UrgentDays));
            _urgentSegment.SelectionChanged += delegate
            {
                int index = _urgentSegment.SelectedIndex;
                if (index < 0 || index >= UrgentDaysOptions.Length) index = 1;
                _store.Settings.UrgentDays = UrgentDaysOptions[index];
                _store.SaveSettings();
                SetStatus("紧迫判定：截止时间在 " + _store.Settings.UrgentDays + " 天内", true);
            };
            cursor = AddRowWide(card, cursor, "紧迫判定",
                "截止时间在多少天内视为紧迫（红色强调）", _urgentSegment);

            _topMostToggle = NewToggle(_store.Settings.AlwaysOnTop);
            _topMostToggle.CheckedChanged += delegate
            {
                _store.Settings.AlwaysOnTop = _topMostToggle.Checked;
                _store.SaveSettings();
            };
            cursor = AddRow(card, cursor, "窗口置顶", "便签始终显示在其他窗口之上", _topMostToggle);

            _closeToTrayToggle = NewToggle(_store.Settings.CloseToTray);
            _closeToTrayToggle.CheckedChanged += delegate
            {
                _store.Settings.CloseToTray = _closeToTrayToggle.Checked;
                _store.SaveSettings();
            };
            cursor = AddRow(card, cursor, "关闭时最小化到托盘",
                "点关闭按钮时隐藏窗口，后台继续提醒", _closeToTrayToggle);

            card.Height = cursor + Ui.S(2);
            _content.Tag = card.Bottom + Ui.S(12);
        }

        private void BuildDataSection()
        {
            int y = (int)(_content.Tag ?? 0);
            int cursor;
            RoundedPanel card = NewCard(y, out cursor);

            PillButton test = NewTextButton("试听", 52, Theme.AccentSoft, Theme.Accent);
            test.Click += delegate { TestVoice(); };
            cursor = AddRow(card, cursor, "语音引擎", SpeechService.VoiceStatusText(), test);

            PillButton open = NewTextButton("打开", 52, Theme.BgDeep, Theme.TextSub);
            open.Click += delegate { OpenDataFolder(); };
            cursor = AddRowWide(card, cursor, "数据目录", _store.DataDir, open);

            Panel backupPanel = new Panel();
            backupPanel.BackColor = Color.White;
            backupPanel.Size = new Size(Ui.S(122), Ui.S(30));

            PillButton export = NewTextButton("导出", 58, Theme.BgDeep, Theme.TextSub);
            export.Location = new Point(0, Ui.S(1));
            export.Click += delegate { ExportBackup(); };
            backupPanel.Controls.Add(export);

            PillButton import = NewTextButton("导入", 58, Theme.BgDeep, Theme.TextSub);
            import.Location = new Point(Ui.S(64), Ui.S(1));
            import.Click += delegate { ImportBackup(); };
            backupPanel.Controls.Add(import);

            cursor = AddRowWide(card, cursor, "备份与恢复", "导出 / 导入 JSON，换电脑也不丢任务", backupPanel);

            cursor = AddRow(card, cursor, "关于",
                "Boss 待办 v1.0 · 无网络请求 · 数据仅保存在本机", null);

            card.Height = cursor + Ui.S(2);
            _content.Tag = card.Bottom + Ui.S(14);
        }

        private static int IndexForDays(int days)
        {
            int best = 1;
            int bestDiff = int.MaxValue;
            for (int i = 0; i < UrgentDaysOptions.Length; i++)
            {
                int diff = Math.Abs(UrgentDaysOptions[i] - days);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    best = i;
                }
            }
            return best;
        }
    }
}
