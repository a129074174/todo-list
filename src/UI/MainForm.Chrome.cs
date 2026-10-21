using System;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class MainForm
    {
        private IconButton MakeHeaderButton(string glyph, string tip)
        {
            IconButton button = new IconButton();
            button.Glyph = glyph;
            button.GlyphSize = 9.2f;
            button.Size = new Size(Ui.S(28), Ui.S(28));
            button.BackColor = Theme.Bg;
            button.HoverBack = Theme.BgDeep;
            button.HoverColor = Theme.TextMain;
            ToolTip tipHolder = _toolTip;
            if (tipHolder == null)
            {
                tipHolder = new ToolTip();
                _toolTip = tipHolder;
            }
            tipHolder.SetToolTip(button, tip);
            return button;
        }

        private void BuildChrome()
        {
            _pinButton = MakeHeaderButton("\uE718", "窗口置顶");
            _pinButton.Active = Settings.AlwaysOnTop;
            _pinButton.ActiveBack = Theme.AccentSoft;
            _pinButton.ActiveColor = Theme.Accent;
            _pinButton.Click += delegate { ToggleAlwaysOnTop(); };
            Controls.Add(_pinButton);

            _minButton = MakeHeaderButton("\uE921", "最小化");
            _minButton.Click += delegate { WindowState = FormWindowState.Minimized; };
            Controls.Add(_minButton);

            _closeButton = MakeHeaderButton("\uE8BB", "关闭到托盘");
            _closeButton.HoverColor = Theme.Danger;
            _closeButton.HoverBack = Theme.DangerSoft;
            _closeButton.Click += delegate
            {
                if (Settings.CloseToTray && !_demo) HideToTray(true);
                else CloseApplication();
            };
            Controls.Add(_closeButton);

            _quickPanel = new RoundedPanel();
            _quickPanel.Radius = 14;
            _quickPanel.DrawBorder = true;
            _quickPanel.BorderColor = Theme.Border;
            _quickPanel.BackColor = Color.White;
            Controls.Add(_quickPanel);

            _input = new TextBox();
            _input.BorderStyle = BorderStyle.None;
            _input.Font = Theme.F(10f, FontStyle.Regular);
            _input.ForeColor = Theme.TextMain;
            _input.BackColor = Color.White;
            _input.KeyDown += InputKeyDown;
            _input.Enter += delegate
            {
                _quickPanel.BorderColor = Ui.Alpha(Theme.Accent, 150);
                _quickPanel.Invalidate();
            };
            _input.Leave += delegate
            {
                _quickPanel.BorderColor = Theme.Border;
                _quickPanel.Invalidate();
            };
            _quickPanel.Controls.Add(_input);

            _addButton = new IconButton();
            _addButton.Glyph = "\uE710";
            _addButton.GlyphSize = 11f;
            _addButton.Filled = true;
            _addButton.FillColor = Theme.Accent;
            _addButton.BackColor = Color.White;
            _addButton.Size = new Size(Ui.S(30), Ui.S(30));
            _addButton.Click += delegate { AddQuickTask(); };
            _quickPanel.Controls.Add(_addButton);

            AddChip("全部", "all");
            AddChip("今天", "today");
            AddChip("逾期", "overdue");
            AddChip("重要", "important");

            _autoSortButton = new PillButton();
            _autoSortButton.Text = "恢复自动排序";
            _autoSortButton.FontSize = 8.2f;
            _autoSortButton.Tint = Theme.Bg;
            _autoSortButton.TextColor = Theme.TextFaint;
            _autoSortButton.BackColor = Theme.Bg;
            _autoSortButton.Height = Ui.S(24);
            _autoSortButton.Click += delegate { ResetManualOrder(); };
            Controls.Add(_autoSortButton);

            _list = new ListHost();
            Controls.Add(_list);

            _bar = new ThinScrollbar();
            _bar.BackColor = Theme.Bg;
            _bar.ValueChanged += delegate
            {
                _scroll = _bar.Value;
                ApplyScroll();
            };
            Controls.Add(_bar);

            _speakButton = new PillButton();
            _speakButton.Text = "朗读总结";
            _speakButton.FontSize = 8.4f;
            _speakButton.Bold = true;
            _speakButton.Tint = Theme.AccentSoft;
            _speakButton.TextColor = Theme.Accent;
            _speakButton.SelectedTint = Theme.AccentSoft;
            _speakButton.SelectedTextColor = Theme.Accent;
            _speakButton.BackColor = Theme.Bg;
            _speakButton.Click += delegate { SpeakSummary(false); };
            Controls.Add(_speakButton);

            _settingsButton = MakeHeaderButton("\uE713", "设置");
            _settingsButton.Click += delegate { OpenSettings(); };
            Controls.Add(_settingsButton);

            _toast = new ToastView();
            Controls.Add(_toast);
            _toast.BringToFront();

            Shown += delegate
            {
                try { NativeMethods.SetCueBanner(_input.Handle, "添加任务，回车即可…"); }
                catch { }
            };

            _layoutReady = true;
            DoLayout();
        }
    }
}
