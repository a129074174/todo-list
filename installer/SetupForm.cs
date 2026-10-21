using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BossTodo.Setup
{
    internal class SetupForm : Form
    {
        private static readonly Color Accent = Color.FromArgb(59, 123, 246);
        private static readonly Color Bg = Color.FromArgb(244, 246, 251);
        private static readonly Color TextMain = Color.FromArgb(32, 38, 52);
        private static readonly Color TextSub = Color.FromArgb(112, 124, 144);
        private static readonly Color Danger = Color.FromArgb(229, 72, 77);
        private static readonly Color Success = Color.FromArgb(47, 163, 107);

        private readonly bool _uninstallMode;
        private TextBox _dirBox;
        private CheckBox _desktopCheck;
        private CheckBox _launchCheck;
        private CheckBox _dataCheck;
        private Label _status;
        private Button _action;
        private Button _cancel;
        private Button _browse;

        public int ExitCode { get; private set; }

        public SetupForm(string installDir, bool uninstallMode)
        {
            _uninstallMode = uninstallMode;
            Text = uninstallMode ? "卸载 Boss 待办" : "Boss 待办 安装向导";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(480, 386);
            BackColor = Bg;
            Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            try
            {
                string icon = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BossTodo.ico");
                if (File.Exists(icon)) Icon = new Icon(icon);
            }
            catch { }

            BuildHeader();
            if (uninstallMode) BuildUninstallBody(installDir);
            else BuildInstallBody(installDir);
            BuildFooter();
        }

        private void BuildHeader()
        {
            Panel header = new Panel();
            header.Bounds = new Rectangle(0, 0, ClientSize.Width, 96);
            header.BackColor = Accent;
            Controls.Add(header);

            Label title = new Label();
            title.Text = _uninstallMode ? "卸载 Boss 待办" : "Boss 待办";
            title.Font = new Font("Microsoft YaHei UI", 17f, FontStyle.Bold, GraphicsUnit.Point);
            title.ForeColor = Color.White;
            title.BackColor = Accent;
            title.Bounds = new Rectangle(24, 18, 400, 34);
            header.Controls.Add(title);

            Label sub = new Label();
            sub.Text = _uninstallMode
                ? "将移除程序与快捷方式，任务数据默认保留"
                : "四象限待办便签 · 开机语音播报 · 数据仅保存在本机";
            sub.Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            sub.ForeColor = Color.FromArgb(230, 240, 255);
            sub.BackColor = Accent;
            sub.Bounds = new Rectangle(26, 56, 430, 22);
            header.Controls.Add(sub);
        }

        private Label MakeLabel(string text, int x, int y, int width, Color color)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = color;
            label.BackColor = Bg;
            label.Bounds = new Rectangle(x, y, width, 20);
            Controls.Add(label);
            return label;
        }

        private void BuildInstallBody(string installDir)
        {
            MakeLabel("安装位置", 24, 112, 200, TextMain);

            _dirBox = new TextBox();
            _dirBox.Text = installDir;
            _dirBox.Bounds = new Rectangle(24, 136, 344, 26);
            Controls.Add(_dirBox);

            _browse = MakeButton("浏览…", 376, 135, 80, 28, Color.White, TextMain);
            _browse.Click += delegate { BrowseFolder(); };

            _desktopCheck = new CheckBox();
            _desktopCheck.Text = "创建桌面快捷方式";
            _desktopCheck.Checked = true;
            _desktopCheck.ForeColor = TextMain;
            _desktopCheck.BackColor = Bg;
            _desktopCheck.Bounds = new Rectangle(24, 184, 260, 24);
            Controls.Add(_desktopCheck);

            _launchCheck = new CheckBox();
            _launchCheck.Text = "安装完成后立即启动";
            _launchCheck.Checked = true;
            _launchCheck.ForeColor = TextMain;
            _launchCheck.BackColor = Bg;
            _launchCheck.Bounds = new Rectangle(24, 212, 260, 24);
            Controls.Add(_launchCheck);

            MakeLabel("安装到当前用户目录，无需管理员权限。任务数据保存在", 24, 254, 430, TextSub);
            MakeLabel("%APPDATA%\\BossTodo，卸载时默认保留。", 24, 274, 430, TextSub);
        }

        private void BuildUninstallBody(string installDir)
        {
            MakeLabel("安装位置", 24, 112, 200, TextMain);
            _dirBox = new TextBox();
            _dirBox.Text = installDir;
            _dirBox.ReadOnly = true;
            _dirBox.Bounds = new Rectangle(24, 136, 432, 26);
            Controls.Add(_dirBox);

            _dataCheck = new CheckBox();
            _dataCheck.Text = "同时删除任务数据（%APPDATA%\\BossTodo，不可恢复）";
            _dataCheck.Checked = false;
            _dataCheck.ForeColor = Danger;
            _dataCheck.BackColor = Bg;
            _dataCheck.Bounds = new Rectangle(24, 186, 430, 24);
            Controls.Add(_dataCheck);

            MakeLabel("卸载前请先退出 Boss 待办；将移除快捷方式与开机启动项。", 24, 228, 430, TextSub);
        }

        private void BuildFooter()
        {
            Panel line = new Panel();
            line.BackColor = Color.FromArgb(229, 234, 243);
            line.Bounds = new Rectangle(24, 310, ClientSize.Width - 48, 1);
            Controls.Add(line);

            _status = new Label();
            _status.Text = string.Empty;
            _status.ForeColor = TextSub;
            _status.BackColor = Bg;
            _status.Bounds = new Rectangle(24, 324, 250, 42);
            Controls.Add(_status);

            _cancel = MakeButton("取消", 268, 328, 86, 34, Color.White, TextSub);
            _cancel.Click += delegate { ExitCode = 0; Close(); };

            _action = MakeButton(_uninstallMode ? "卸载" : "安装", 362, 328, 94, 34,
                _uninstallMode ? Danger : Accent, Color.White);
            _action.FlatAppearance.BorderSize = 0;
            _action.Font = new Font("Microsoft YaHei UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            _action.Click += delegate { StartAction(); };
        }

        private void BrowseFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择 Boss 待办的安装目录";
                dialog.ShowNewFolderButton = true;
                if (Directory.Exists(_dirBox.Text)) dialog.SelectedPath = _dirBox.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK) _dirBox.Text = dialog.SelectedPath;
            }
        }

        private void StartAction()
        {
            if (_uninstallMode) DoUninstall();
            else DoInstall();
        }

        private bool EnsureAppClosed(string action)
        {
            if (!Program.AppRunning()) return true;

            DialogResult result = MessageBox.Show(this,
                "Boss 待办 正在运行，需要先退出才能继续" + action + "。\r\n\r\n是否立即关闭它并继续？",
                "Boss 待办 正在运行", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                SetStatus("请先从托盘退出 Boss 待办，再继续" + action, Danger);
                return false;
            }
            if (!Program.CloseRunningApp())
            {
                SetStatus("无法关闭 Boss 待办，请手动退出后重试", Danger);
                return false;
            }
            return true;
        }

        private void DoInstall()
        {
            string input = _dirBox.Text.Trim();
            if (input.Length == 0)
            {
                SetStatus("请选择安装位置", Danger);
                return;
            }
            string dir;
            try { dir = Path.GetFullPath(input); }
            catch
            {
                SetStatus("安装路径无效", Danger);
                return;
            }
            if (!EnsureAppClosed("安装")) return;

            SetBusy(true, "正在安装…");
            Application.DoEvents();
            int code = Installer.Run(dir, false, _launchCheck.Checked, true, _desktopCheck.Checked);
            if (code == 0)
            {
                SetStatus("安装完成", Success);
                ExitCode = 0;
                MessageBox.Show(this, "Boss 待办已安装到：\r\n" + dir, "安装完成",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            else if (code == 2)
            {
                SetBusy(false, string.Empty);
                SetStatus("请先退出 Boss 待办，再开始安装", Danger);
            }
            else
            {
                SetBusy(false, string.Empty);
                SetStatus("安装失败，请查看 %TEMP%\\BossTodoSetup.log", Danger);
            }
        }

        private void DoUninstall()
        {
            DialogResult confirm = MessageBox.Show(this,
                "确定要卸载 Boss 待办吗？\r\n\r\n" +
                (_dataCheck.Checked ? "勾选的任务数据也会被删除。" : "任务数据将保留在 %APPDATA%\\BossTodo。"),
                "卸载确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            if (!EnsureAppClosed("卸载")) return;

            SetBusy(true, "正在卸载…");
            Application.DoEvents();
            int code = Uninstaller.Run(_dirBox.Text.Trim(), false, _dataCheck.Checked);
            if (code == 0)
            {
                ExitCode = 0;
                MessageBox.Show(this, "Boss 待办已卸载。", "卸载完成",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            else
            {
                SetBusy(false, string.Empty);
                SetStatus(code == 2 ? "请先退出 Boss 待办" : "卸载失败，请查看日志", Danger);
            }
        }

        private void SetBusy(bool busy, string message)
        {
            _action.Enabled = !busy;
            _cancel.Enabled = !busy;
            if (_browse != null) _browse.Enabled = !busy;
            if (_dirBox != null) _dirBox.Enabled = !busy;
            if (_desktopCheck != null) _desktopCheck.Enabled = !busy;
            if (_launchCheck != null) _launchCheck.Enabled = !busy;
            if (_dataCheck != null) _dataCheck.Enabled = !busy;
            if (message.Length > 0) SetStatus(message, TextSub);
        }

        private void SetStatus(string text, Color color)
        {
            _status.Text = text;
            _status.ForeColor = color;
        }

        private Button MakeButton(string text, int x, int y, int width, int height, Color back, Color fore)
        {
            Button button = new Button();
            button.Text = text;
            button.Bounds = new Rectangle(x, y, width, height);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(220, 226, 238);
            button.BackColor = back;
            button.ForeColor = fore;
            button.Cursor = Cursors.Hand;
            Controls.Add(button);
            return button;
        }
    }
}
