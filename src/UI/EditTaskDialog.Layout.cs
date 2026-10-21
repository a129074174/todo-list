using System;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class EditTaskDialog
    {
        private void BuildSchedule()
        {
            Controls.Add(MakeLabel("截止时间", Ui.S(24), Ui.S(250), Ui.S(160)));
            _dueToggle = new ToggleSwitch();
            _dueToggle.BackColor = Theme.Bg;
            _dueToggle.Location = new Point(ClientSize.Width - Ui.S(24) - _dueToggle.Width, Ui.S(247));
            _dueToggle.CheckedChanged += delegate { ApplyDueEnabled(); };
            Controls.Add(_dueToggle);

            _todayChip = CreateChip("今天", Ui.S(24), Ui.S(276));
            _tomorrowChip = CreateChip("明天", Ui.S(24) + Ui.S(62), Ui.S(276));
            _afterChip = CreateChip("后天", Ui.S(24) + Ui.S(124), Ui.S(276));
            _nextWeekChip = CreateChip("下周一", Ui.S(24) + Ui.S(186), Ui.S(276));

            RoundedPanel datePanel = new RoundedPanel();
            datePanel.Bounds = new Rectangle(Ui.S(24), Ui.S(318), Ui.S(312), Ui.S(38));
            datePanel.Radius = 10;
            _datePicker = new DateTimePicker();
            _datePicker.Format = DateTimePickerFormat.Custom;
            _datePicker.CustomFormat = "yyyy年M月d日";
            _datePicker.Font = Theme.F(9.2f, FontStyle.Regular);
            _datePicker.CalendarMonthBackground = Color.White;
            _datePicker.CalendarTitleBackColor = Theme.Accent;
            _datePicker.CalendarTitleForeColor = Color.White;
            _datePicker.CalendarTrailingForeColor = Theme.TextFaint;
            _datePicker.Bounds = new Rectangle(Ui.S(12), Ui.S(7), Ui.S(178), Ui.S(24));
            _datePicker.ValueChanged += delegate { UpdateChipState(); };
            datePanel.Controls.Add(_datePicker);

            _timePicker = new DateTimePicker();
            _timePicker.Format = DateTimePickerFormat.Custom;
            _timePicker.CustomFormat = "HH:mm";
            _timePicker.ShowUpDown = true;
            _timePicker.Font = Theme.F(9.2f, FontStyle.Regular);
            _timePicker.Bounds = new Rectangle(Ui.S(200), Ui.S(7), Ui.S(96), Ui.S(24));
            datePanel.Controls.Add(_timePicker);
            Controls.Add(datePanel);

            Controls.Add(MakeLabel("重要程度", Ui.S(24), Ui.S(368), Ui.S(160)));
            _importance = new SegmentedControl();
            _importance.Items = new string[] { "高", "中", "低" };
            _importance.ItemColors = new Color[] { Theme.Q1, Theme.Q3, Theme.Q4 };
            _importance.Bounds = new Rectangle(Ui.S(24), Ui.S(388), Ui.S(312), Ui.S(34));
            _importance.BackColor = Theme.Bg;
            Controls.Add(_importance);
        }

        private PillButton CreateChip(string text, int x, int y)
        {
            PillButton chip = new PillButton();
            chip.Text = text;
            chip.FontSize = 8.8f;
            chip.BackColor = Theme.Bg;
            chip.Tint = Theme.BgDeep;
            using (Graphics g = CreateGraphics())
            {
                chip.Width = chip.PreferredWidth(g);
            }
            chip.Height = Ui.S(28);
            chip.Location = new Point(x, y);
            chip.Click += delegate { ApplyPreset(text); };
            Controls.Add(chip);
            return chip;
        }

        private void BuildFooter()
        {
            Panel line = new Panel();
            line.BackColor = Theme.Border;
            line.Bounds = new Rectangle(Ui.S(24), Ui.S(438), ClientSize.Width - Ui.S(48), 1);
            Controls.Add(line);

            PillButton cancel = new PillButton();
            cancel.Text = "取消";
            cancel.Bold = true;
            cancel.FontSize = 9.2f;
            cancel.Tint = Theme.BgDeep;
            cancel.BackColor = Theme.Bg;
            cancel.Bounds = new Rectangle(ClientSize.Width - Ui.S(24) - Ui.S(164), Ui.S(454), Ui.S(76), Ui.S(34));
            cancel.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(cancel);

            PillButton save = new PillButton();
            save.Text = "保存";
            save.Bold = true;
            save.FontSize = 9.2f;
            save.Filled = true;
            save.FilledColor = Theme.Accent;
            save.BackColor = Theme.Bg;
            save.Bounds = new Rectangle(ClientSize.Width - Ui.S(24) - Ui.S(84), Ui.S(454), Ui.S(84), Ui.S(34));
            save.Click += delegate { SaveAndClose(); };
            Controls.Add(save);

            if (!_isNew)
            {
                PillButton delete = new PillButton();
                delete.Text = "删除";
                delete.Bold = true;
                delete.FontSize = 9.2f;
                delete.Tint = Theme.DangerSoft;
                delete.TextColor = Theme.Danger;
                delete.BackColor = Theme.Bg;
                delete.Bounds = new Rectangle(Ui.S(24), Ui.S(454), Ui.S(76), Ui.S(34));
                delete.Click += delegate
                {
                    DeleteRequested = true;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                Controls.Add(delete);
            }
        }
    }
}
