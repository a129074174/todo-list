using System;
using System.Drawing;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>统一风格的确认弹窗。</summary>
    public class ConfirmDialog : DialogBase
    {
        private string _title;
        private string _message;
        private string _confirmText;
        private bool _danger;

        private ConfirmDialog(string title, string message, string confirmText, bool danger)
        {
            _title = title;
            _message = message;
            _confirmText = confirmText;
            _danger = danger;

            Text = title;
            ClientSize = new Size(Ui.S(352), Ui.S(198));
            BackColor = Theme.Bg;

            Label messageLabel = new Label();
            messageLabel.Text = message;
            messageLabel.Font = Theme.F(9.4f, FontStyle.Regular);
            messageLabel.ForeColor = Theme.TextSub;
            messageLabel.BackColor = Theme.Bg;
            messageLabel.Bounds = new Rectangle(Ui.S(24), Ui.S(64), ClientSize.Width - Ui.S(48), Ui.S(56));
            messageLabel.AutoEllipsis = true;
            Controls.Add(messageLabel);

            PillButton cancel = new PillButton();
            cancel.Text = "取消";
            cancel.Bold = true;
            cancel.FontSize = 9.2f;
            cancel.Tint = Theme.BgDeep;
            cancel.BackColor = Theme.Bg;
            cancel.Bounds = new Rectangle(ClientSize.Width - Ui.S(24) - Ui.S(168), Ui.S(142), Ui.S(76), Ui.S(34));
            cancel.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(cancel);

            PillButton confirm = new PillButton();
            confirm.Text = confirmText;
            confirm.Bold = true;
            confirm.FontSize = 9.2f;
            confirm.Filled = true;
            confirm.FilledColor = danger ? Theme.Danger : Theme.Accent;
            confirm.BackColor = Theme.Bg;
            confirm.Bounds = new Rectangle(ClientSize.Width - Ui.S(24) - Ui.S(84), Ui.S(142), Ui.S(84), Ui.S(34));
            confirm.Click += delegate
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(confirm);

            CreateCloseButton();
        }

        protected override string DialogTitleText
        {
            get { return _title; }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        public static bool Show(IWin32Window owner, string title, string message, string confirmText, bool danger)
        {
            using (ConfirmDialog dialog = new ConfirmDialog(title, message, confirmText, danger))
            {
                return dialog.ShowDialog(owner) == DialogResult.OK;
            }
        }
    }
}
