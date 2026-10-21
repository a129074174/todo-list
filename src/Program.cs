using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using BossTodo.Core;
using BossTodo.UI;

namespace BossTodo
{
    internal static partial class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            string previewTarget = GetOption(args, "--render-preview");
            bool demo = previewTarget != null;
            bool autostart = HasFlag(args, "-autostart");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    Ui.Scale = Math.Max(1f, g.DpiX / 96f);
                }
            }
            catch { Ui.Scale = 1f; }

            string iconTarget = GetOption(args, "--make-icon");
            if (iconTarget != null)
            {
                if (iconTarget.Length == 0)
                    iconTarget = System.IO.Path.Combine(Environment.CurrentDirectory, "assets", "BossTodo.ico");
                bool ok = IconFactory.SaveIco(iconTarget, new int[] { 16, 24, 32, 48, 64, 128, 256 });
                Console.WriteLine((ok ? "[PASS]" : "[FAIL]") + " icon -> " + iconTarget);
                Environment.ExitCode = ok ? 0 : 1;
                return;
            }

            if (HasFlag(args, "--selftest"))
            {
                Environment.ExitCode = RunSelfTest() ? 0 : 1;
                return;
            }

            string speakTarget = GetOption(args, "--speak-test");
            if (speakTarget != null)
            {
                if (speakTarget.Length == 0)
                    speakTarget = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BossTodo-speech-test.wav");
                bool ok = SpeechService.RenderToWaveFile(
                    "Boss，您还有 6 项任务待完成，其中 1 项已逾期。建议先完成：提交季度项目报告。", speakTarget);
                Console.WriteLine((ok ? "[PASS]" : "[FAIL]") + " speech wave -> " + speakTarget);
                Environment.ExitCode = ok ? 0 : 1;
                return;
            }

            if (HasFlag(args, "--smoke"))
            {
                Environment.ExitCode = RunSmokeTest() ? 0 : 1;
                return;
            }

            if (demo)
            {
                string previewDir = ResolvePreviewDir(previewTarget);
                string dataDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BossTodoPreviewData");
                PrepareDemoData(dataDir);
                RenderPreviews(previewDir, dataDir);
                return;
            }

            bool created;
            using (Mutex mutex = new Mutex(true, "Local\\BossTodoSingleInstance", out created))
            {
                if (!created)
                {
                    SignalExistingInstance();
                    return;
                }

                AppPaths.EnsureDataDir();
                AutoStart.RepairIfEnabled();

                EventWaitHandle showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "Local\\BossTodoShow");
                MainForm form = new MainForm(AppPaths.DataDir, autostart, false);
                ThreadPool.RegisterWaitForSingleObject(showEvent,
                    delegate
                    {
                        try
                        {
                            if (!form.IsDisposed && form.IsHandleCreated)
                                form.BeginInvoke(new MethodInvoker(form.RestoreFromTray));
                        }
                        catch { }
                    }, null, Timeout.Infinite, false);

                form.ScheduleStartupSpeech();
                Application.Run(form);
                showEvent.Close();
            }
        }

        private static void SignalExistingInstance()
        {
            try
            {
                EventWaitHandle showEvent;
                if (EventWaitHandle.TryOpenExisting("Local\\BossTodoShow", out showEvent))
                {
                    showEvent.Set();
                    showEvent.Close();
                }
            }
            catch { }
        }

        private static bool HasFlag(string[] args, string flag)
        {
            foreach (string a in args)
            {
                if (string.Equals(a, flag, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static string GetOption(string[] args, string name)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-", StringComparison.Ordinal))
                        return args[i + 1];
                    return string.Empty;
                }
            }
            return null;
        }

        private static string ResolvePreviewDir(string target)
        {
            string dir;
            if (string.IsNullOrEmpty(target)) dir = System.IO.Path.Combine(Environment.CurrentDirectory, "preview");
            else if (target.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                dir = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(target));
            else dir = System.IO.Path.GetFullPath(target);
            System.IO.Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
