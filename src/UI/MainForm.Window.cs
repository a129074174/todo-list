using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    public partial class MainForm
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 2;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Ui.Quality(g);
            g.Clear(Theme.Bg);
            PaintHeader(g);
            PaintFooter(g);
            Ui.StrokeRound(g, Theme.Border, new Rectangle(0, 0, Width - 1, Height - 1), Ui.S(16), 1f);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Width > 0 && Height > 0)
            {
                Region = new Region(Ui.Round(new Rectangle(0, 0, Width, Height), Ui.S(16)));
                DoLayout();
                if (_layoutReady && _list != null) RebuildList(true);
            }
        }

        private void ApplyWindowPosition()
        {
            Rectangle work = Screen.PrimaryScreen.WorkingArea;
            int x;
            int y;
            if (Settings.HasPosition)
            {
                x = Settings.WindowX;
                y = Settings.WindowY;
            }
            else
            {
                x = work.Right - Width - Ui.S(28);
                y = work.Top + Ui.S(56);
            }
            int minX = work.Left + Ui.S(6);
            int minY = work.Top + Ui.S(6);
            int maxX = work.Right - Width - Ui.S(6);
            int maxY = work.Bottom - Height - Ui.S(6);
            if (maxX < minX) maxX = minX;
            if (maxY < minY) maxY = minY;
            x = Math.Max(minX, Math.Min(maxX, x));
            y = Math.Max(minY, Math.Min(maxY, y));
            Location = new Point(x, y);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.Y < Ui.S(64) && e.X < Width - Ui.S(120))
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Control && e.KeyCode == Keys.N)
            {
                _input.Focus();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                OpenNewTaskDialog(string.Empty);
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.R)
            {
                SpeakSummary(false);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && Settings.CloseToTray && !_demo)
            {
                HideToTray(false);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_reallyExit && e.CloseReason == CloseReason.UserClosing && Settings.CloseToTray && !_demo)
            {
                e.Cancel = true;
                HideToTray(true);
                return;
            }
            SaveWindowState();
            _store.SaveSettings();
            _refreshTimer.Stop();
            if (_wheelFilter != null)
            {
                Application.RemoveMessageFilter(_wheelFilter);
                _wheelFilter = null;
            }
            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
                _tray = null;
            }
            if (_trayMenu != null)
            {
                _trayMenu.Dispose();
                _trayMenu = null;
            }
            if (_toolTip != null)
            {
                _toolTip.Dispose();
                _toolTip = null;
            }
            _speech.Dispose();
            base.OnFormClosing(e);
        }

        public void ScheduleStartupSpeech()
        {
            if (_demo) return;
            bool should = _isAutostart ? Settings.SpeakOnStartup : Settings.SpeakOnEveryLaunch;
            if (!should) return;
            if (_isAutostart && Settings.LastSpokenDate == SpeechService.DateKey(DateTime.Today)) return;

            Timer timer = new Timer();
            timer.Interval = _isAutostart ? 3000 : 1500;
            timer.Tick += delegate
            {
                timer.Stop();
                timer.Dispose();
                SpeakSummary(true);
            };
            timer.Start();
        }

        public void RestoreFromTray()
        {
            Show();
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
        }

        private void HideToTray(bool showHint)
        {
            SaveWindowState();
            _store.SaveSettings();
            Hide();
            if (_tray != null && showHint && !_trayHintShown)
            {
                _trayHintShown = true;
                try
                {
                    _tray.ShowBalloonTip(2500, "Boss 待办仍在运行",
                        "双击托盘图标即可重新打开便签。", ToolTipIcon.Info);
                }
                catch { }
            }
        }

        private void SaveWindowState()
        {
            if (_demo) return;
            if (WindowState == FormWindowState.Normal)
            {
                Settings.WindowX = Location.X;
                Settings.WindowY = Location.Y;
                Settings.HasPosition = true;
            }
        }

        private void CloseApplication()
        {
            _reallyExit = true;
            Close();
            Application.Exit();
        }
    }
}
