using System;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class SettingsDialog
    {
        private static readonly int[] UrgentDaysOptions = new int[] { 1, 2, 3, 5 };

        private ToggleSwitch NewToggle(bool value)
        {
            ToggleSwitch toggle = new ToggleSwitch();
            toggle.BackColor = Color.White;
            toggle.SetCheckedSilently(value);
            return toggle;
        }

        private PillButton NewTextButton(string text, int width, Color tint, Color fore)
        {
            PillButton button = new PillButton();
            button.Text = text;
            button.FontSize = 8.6f;
            button.Bold = true;
            button.Tint = tint;
            button.TextColor = fore;
            button.BackColor = Color.White;
            button.Size = new Size(Ui.S(width), Ui.S(28));
            return button;
        }

        private void BuildGeneralSection()
        {
            int y = Ui.S(4);
            int cursor;
            RoundedPanel card = NewCard(y, out cursor);

            _autoStartToggle = NewToggle(_store.Settings.AutoStart);
            _autoStartToggle.CheckedChanged += delegate
            {
                _store.Settings.AutoStart = _autoStartToggle.Checked;
                bool ok = AutoStart.SetEnabled(_store.Settings.AutoStart);
                _store.SaveSettings();
                if (!ok) SetStatus("无法写入开机启动项，请检查权限", false);
                else SetStatus(_store.Settings.AutoStart ? "已开启开机自动启动" : "已关闭开机自动启动", true);
            };
            cursor = AddRow(card, cursor, "开机自动启动",
                "登录 Windows 后自动打开便签并语音播报", _autoStartToggle);

            _speakStartupToggle = NewToggle(_store.Settings.SpeakOnStartup);
            _speakStartupToggle.CheckedChanged += delegate
            {
                _store.Settings.SpeakOnStartup = _speakStartupToggle.Checked;
                _store.SaveSettings();
                SetStatus(_store.Settings.SpeakOnStartup ? "开机播报已开启" : "开机播报已关闭", true);
            };
            cursor = AddRow(card, cursor, "启动时语音播报",
                "开机启动后朗读今日任务概览（每天一次）", _speakStartupToggle);

            _speakEveryToggle = NewToggle(_store.Settings.SpeakOnEveryLaunch);
            _speakEveryToggle.CheckedChanged += delegate
            {
                _store.Settings.SpeakOnEveryLaunch = _speakEveryToggle.Checked;
                _store.SaveSettings();
            };
            cursor = AddRow(card, cursor, "手动启动也播报",
                "双击图标打开便签时也朗读一次总结", _speakEveryToggle);

            Panel ratePanel = new Panel();
            ratePanel.BackColor = Color.White;
            ratePanel.Size = new Size(Ui.S(94), Ui.S(30));

            PillButton minus = NewTextButton("−", 30, Theme.BgDeep, Theme.TextSub);
            minus.FontSize = 11f;
            minus.Location = new Point(0, Ui.S(1));
            minus.Click += delegate { ChangeRate(-1); };
            ratePanel.Controls.Add(minus);

            _rateValue = new Label();
            _rateValue.Text = FormatRate(_rate);
            _rateValue.Font = Theme.F(9f, FontStyle.Bold);
            _rateValue.ForeColor = Theme.TextMain;
            _rateValue.BackColor = Color.White;
            _rateValue.TextAlign = ContentAlignment.MiddleCenter;
            _rateValue.Bounds = new Rectangle(Ui.S(31), Ui.S(1), Ui.S(32), Ui.S(28));
            ratePanel.Controls.Add(_rateValue);

            PillButton plus = NewTextButton("+", 30, Theme.BgDeep, Theme.TextSub);
            plus.FontSize = 11f;
            plus.Location = new Point(Ui.S(64), Ui.S(1));
            plus.Click += delegate { ChangeRate(1); };
            ratePanel.Controls.Add(plus);

            cursor = AddRow(card, cursor, "语速", "数值越大朗读越快（-5 ~ +5）", ratePanel);

            card.Height = cursor + Ui.S(2);
            _content.Tag = card.Bottom + Ui.S(12);
        }
    }
}
