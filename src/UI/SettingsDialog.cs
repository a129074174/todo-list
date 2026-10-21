using System;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    /// <summary>设置窗口：开机播报、语速、紧迫阈值、数据备份等。</summary>
    public partial class SettingsDialog : DialogBase
    {
        private readonly TaskStore _store;
        private readonly SpeechService _speech;

        private Panel _content;
        private Label _statusLabel;
        private ToggleSwitch _autoStartToggle;
        private ToggleSwitch _speakStartupToggle;
        private ToggleSwitch _speakEveryToggle;
        private ToggleSwitch _topMostToggle;
        private ToggleSwitch _closeToTrayToggle;
        private SegmentedControl _urgentSegment;
        private Label _rateValue;
        private ThinScrollbar _scroll;
        private bool _syncingScroll;
        private int _rate;
        private int _settingsScroll;

        public SettingsDialog(TaskStore store, SpeechService speech)
        {
            _store = store;
            _speech = speech;
            _rate = store.Settings.SpeechRate;

            Text = "设置";
            ClientSize = new Size(Ui.S(380), Ui.S(620));
            BackColor = Theme.Bg;

            BuildShell();
            BuildGeneralSection();
            BuildWindowSection();
            BuildDataSection();
            BuildScrollbar();
            CreateCloseButton();
            EnableSmoothScrolling();
        }

        protected override string DialogTitleText
        {
            get { return "设置"; }
        }

        protected override string DialogSubtitleText
        {
            get { return "所有数据仅保存在本机，不会上传"; }
        }

        private void BuildShell()
        {
            _content = new Panel();
            _content.Location = new Point(0, Ui.S(66));
            _content.Size = new Size(ClientSize.Width - Ui.S(20), ClientSize.Height - Ui.S(66) - Ui.S(62));
            _content.BackColor = Theme.Bg;
            _content.AutoScroll = false;
            Controls.Add(_content);

            Panel line = new Panel();
            line.BackColor = Theme.Border;
            line.Bounds = new Rectangle(Ui.S(24), ClientSize.Height - Ui.S(62), ClientSize.Width - Ui.S(48), 1);
            Controls.Add(line);

            _statusLabel = new Label();
            _statusLabel.Text = "Boss 待办 v1.0";
            _statusLabel.Font = Theme.Tiny;
            _statusLabel.ForeColor = Theme.TextFaint;
            _statusLabel.BackColor = Theme.Bg;
            _statusLabel.AutoEllipsis = true;
            _statusLabel.Bounds = new Rectangle(Ui.S(24), ClientSize.Height - Ui.S(48),
                ClientSize.Width - Ui.S(48) - Ui.S(104), Ui.S(30));
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(_statusLabel);

            PillButton done = new PillButton();
            done.Text = "完成";
            done.Bold = true;
            done.FontSize = 9.2f;
            done.Filled = true;
            done.FilledColor = Theme.Accent;
            done.BackColor = Theme.Bg;
            done.Bounds = new Rectangle(ClientSize.Width - Ui.S(24) - Ui.S(92), ClientSize.Height - Ui.S(50), Ui.S(92), Ui.S(34));
            done.Click += delegate { Close(); };
            Controls.Add(done);
        }

        private void BuildScrollbar()
        {
            _scroll = new ThinScrollbar();
            _scroll.BackColor = Theme.Bg;
            _scroll.Bounds = new Rectangle(ClientSize.Width - Ui.S(18), _content.Top + Ui.S(4),
                Ui.S(8), _content.Height - Ui.S(8));
            _scroll.ValueChanged += delegate
            {
                if (_syncingScroll) return;
                SetSettingsScroll(_scroll.Value);
            };
            Controls.Add(_scroll);
            _scroll.BringToFront();

            _content.Scroll += delegate
            {
                if (_syncingScroll) return;
                SyncSettingsScrollbar();
            };
            Shown += delegate { SyncSettingsScrollbar(); };
        }

        private void SyncSettingsScrollbar()
        {
            if (_scroll == null) return;
            int contentHeight = _content.Tag is int ? (int)_content.Tag : 0;
            int viewport = _content.ClientSize.Height;
            int max = Math.Max(0, contentHeight - viewport);
            if (_settingsScroll > max) _settingsScroll = max;
            _syncingScroll = true;
            _scroll.Maximum = Math.Max(contentHeight, viewport);
            _scroll.Viewport = viewport;
            _scroll.Value = _settingsScroll;
            _syncingScroll = false;
            _scroll.Visible = contentHeight > viewport + Ui.S(2);
        }

        private void SetSettingsScroll(int value)
        {
            int contentHeight = _content.Tag is int ? (int)_content.Tag : 0;
            int viewport = _content.ClientSize.Height;
            int max = Math.Max(0, contentHeight - viewport);
            _settingsScroll = Math.Max(0, Math.Min(max, value));
            foreach (Control control in _content.Controls)
            {
                if (control.Tag is int) control.Top = (int)control.Tag - _settingsScroll;
            }
            if (_scroll != null)
            {
                _syncingScroll = true;
                _scroll.Value = _settingsScroll;
                _syncingScroll = false;
            }
        }

        private int AddRowWide(RoundedPanel card, int y, string title, string desc, Control right)
        {
            int rowH = Ui.S(62);
            int rightWidth = right == null ? 0 : right.Width + Ui.S(12);

            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = Theme.F(9.4f, FontStyle.Bold);
            titleLabel.ForeColor = Theme.TextMain;
            titleLabel.BackColor = Color.White;
            titleLabel.AutoEllipsis = true;
            titleLabel.Bounds = new Rectangle(Ui.S(16), y + Ui.S(9),
                Math.Max(Ui.S(40), card.Width - Ui.S(32) - rightWidth), Ui.S(20));
            card.Controls.Add(titleLabel);

            Label descLabel = new Label();
            descLabel.Text = desc;
            descLabel.Font = Theme.Tiny;
            descLabel.ForeColor = Theme.TextFaint;
            descLabel.BackColor = Color.White;
            descLabel.AutoEllipsis = true;
            descLabel.Bounds = new Rectangle(Ui.S(16), y + Ui.S(40), card.Width - Ui.S(32), Ui.S(18));
            card.Controls.Add(descLabel);

            if (right != null)
            {
                right.Location = new Point(card.Width - Ui.S(16) - right.Width, y + Ui.S(8));
                card.Controls.Add(right);
            }

            Panel divider = new Panel();
            divider.BackColor = Theme.Border;
            divider.Bounds = new Rectangle(Ui.S(16), y + rowH - 1, card.Width - Ui.S(32), 1);
            card.Controls.Add(divider);
            return y + rowH;
        }

        private RoundedPanel NewCard(int y, out int cursor)
        {
            RoundedPanel card = new RoundedPanel();
            card.Radius = 12;
            card.BackColor = Color.White;
            card.Location = new Point(Ui.S(24), y);
            card.Tag = y;
            card.Size = new Size(ClientSize.Width - Ui.S(48) - Ui.S(18), Ui.S(10));
            _content.Controls.Add(card);
            cursor = Ui.S(8);
            return card;
        }

        private int AddRow(RoundedPanel card, int y, string title, string desc, Control right)
        {
            int rowH = Ui.S(56);
            int rightWidth = right == null ? 0 : right.Width + Ui.S(12);

            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = Theme.F(9.4f, FontStyle.Bold);
            titleLabel.ForeColor = Theme.TextMain;
            titleLabel.BackColor = Color.White;
            titleLabel.AutoEllipsis = true;
            titleLabel.Bounds = new Rectangle(Ui.S(16), y + Ui.S(9),
                Math.Max(Ui.S(40), card.Width - Ui.S(32) - rightWidth), Ui.S(20));
            card.Controls.Add(titleLabel);

            Label descLabel = new Label();
            descLabel.Text = desc;
            descLabel.Font = Theme.Tiny;
            descLabel.ForeColor = Theme.TextFaint;
            descLabel.BackColor = Color.White;
            descLabel.AutoEllipsis = true;
            descLabel.Bounds = new Rectangle(Ui.S(16), y + Ui.S(31),
                Math.Max(Ui.S(40), card.Width - Ui.S(32) - rightWidth), Ui.S(18));
            card.Controls.Add(descLabel);

            if (right != null)
            {
                right.Location = new Point(card.Width - Ui.S(16) - right.Width, y + (rowH - right.Height) / 2);
                card.Controls.Add(right);
            }

            Panel divider = new Panel();
            divider.BackColor = Theme.Border;
            divider.Bounds = new Rectangle(Ui.S(16), y + rowH - 1, card.Width - Ui.S(32), 1);
            card.Controls.Add(divider);
            return y + rowH;
        }
    }
}
