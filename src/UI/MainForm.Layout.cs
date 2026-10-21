using System;
using System.Drawing;
using System.Windows.Forms;

namespace BossTodo.UI
{
    public partial class MainForm
    {
        private void AddChip(string text, string key)
        {
            PillButton chip = new PillButton();
            chip.Text = text;
            chip.FontSize = 8.8f;
            chip.BackColor = Theme.Bg;
            chip.Tint = Theme.BgDeep;
            chip.SelectedTint = Theme.AccentSoft;
            chip.SelectedTextColor = Theme.Accent;
            chip.Tag = key;
            using (Graphics g = CreateGraphics())
            {
                chip.Width = chip.PreferredWidth(g);
            }
            chip.Height = Ui.S(28);
            chip.Click += delegate { SetFilter(key); };
            _chips.Add(chip);
            Controls.Add(chip);
        }

        private void DoLayout()
        {
            if (!_layoutReady) return;
            int w = ClientSize.Width;
            int h = ClientSize.Height;
            int margin = Ui.S(16);

            _closeButton.Location = new Point(w - margin - _closeButton.Width + Ui.S(2), Ui.S(15));
            _minButton.Location = new Point(_closeButton.Left - Ui.S(4) - _minButton.Width, Ui.S(15));
            _pinButton.Location = new Point(_minButton.Left - Ui.S(4) - _pinButton.Width, Ui.S(15));

            _quickPanel.Bounds = new Rectangle(margin, Ui.S(66), w - margin * 2, Ui.S(44));
            _input.Bounds = new Rectangle(Ui.S(14), Ui.S(11), _quickPanel.Width - Ui.S(64), Ui.S(22));
            _addButton.Bounds = new Rectangle(_quickPanel.Width - Ui.S(39), Ui.S(7), Ui.S(30), Ui.S(30));

            int x = margin;
            int chipY = Ui.S(122);
            foreach (PillButton chip in _chips)
            {
                chip.Location = new Point(x, chipY);
                x += chip.Width + Ui.S(6);
            }

            _autoSortButton.Location = new Point(w - margin - _autoSortButton.Width, Ui.S(124));

            int listTop = Ui.S(160);
            int footerTop = h - Ui.S(56);
            _list.Bounds = new Rectangle(margin, listTop, w - margin * 2 - Ui.S(10), Math.Max(Ui.S(10), footerTop - listTop));
            _bar.Bounds = new Rectangle(w - margin + Ui.S(3), listTop + Ui.S(4), Ui.S(7), Math.Max(Ui.S(10), _list.Height - Ui.S(8)));
            _bar.BringToFront();

            int toastWidth = Math.Min(w - Ui.S(48), Ui.S(312));
            _toast.Bounds = new Rectangle((w - toastWidth) / 2, footerTop - Ui.S(48), toastWidth, Ui.S(38));

            _speakButton.Location = new Point(margin, h - Ui.S(41));
            _speakButton.Width = Ui.S(100);
            _settingsButton.Location = new Point(w - margin - _settingsButton.Width, h - Ui.S(42));
            _toast.BringToFront();
        }
    }
}
