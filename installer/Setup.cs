using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BossTodo.Setup
{
    internal static class Program
    {
        private const string AppName = "Boss 待办";
        private const string RunValueName = "BossTodo";

        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool silent = HasArg(args, "/S") || HasArg(args, "--silent");
            // Uninstall.exe 是安装程序自身的副本；直接双击时没有参数，必须按文件名识别为卸载模式，
            // 否则会被当成安装程序，出现“点卸载反而在安装”的问题。
            bool uninstall = HasArg(args, "--uninstall") || HasArg(args, "/uninstall") || IsUninstallerExecutable();
            string installDir = GetDirArg(args);
            if (string.IsNullOrEmpty(installDir))
            {
                installDir = uninstall
                    ? Path.GetDirectoryName(Application.ExecutablePath)
                    : DefaultInstallDir;
            }

            if (uninstall)
            {
                if (silent)
                {
                    return Uninstaller.Run(installDir, true, false);
                }
                using (SetupForm form = new SetupForm(installDir, true))
                {
                    Application.Run(form);
                    return form.ExitCode;
                }
            }

            if (silent)
            {
                return Installer.Run(installDir, true, false, true, true);
            }

            using (SetupForm form = new SetupForm(installDir, false))
            {
                Application.Run(form);
                return form.ExitCode;
            }
        }

        public static string DefaultInstallDir
        {
            get
            {
                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(Path.Combine(local, "Programs"), "BossTodo");
            }
        }

        private static bool HasArg(string[] args, string value)
        {
            foreach (string arg in args)
            {
                if (string.Equals(arg, value, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static bool IsUninstallerExecutable()
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
                return string.Equals(name, "Uninstall", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string GetDirArg(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("/DIR=", StringComparison.OrdinalIgnoreCase))
                    return arg.Substring(5).Trim('"');
                if (arg.StartsWith("--dir=", StringComparison.OrdinalIgnoreCase))
                    return arg.Substring(6).Trim('"');
                if ((string.Equals(arg, "/DIR", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(arg, "--dir", StringComparison.OrdinalIgnoreCase)) &&
                    i + 1 < args.Length)
                    return args[i + 1].Trim('"');
            }
            return null;
        }

        public static void Log(string message)
        {
            try
            {
                string path = Path.Combine(Path.GetTempPath(), "BossTodoSetup.log");
                File.AppendAllText(path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + message + Environment.NewLine,
                    Encoding.UTF8);
            }
            catch { }
        }

        public static bool AppRunning()
        {
            try { return Process.GetProcessesByName("BossTodo").Length > 0; }
            catch { return false; }
        }

        /// <summary>关闭正在运行的 BossTodo（关闭按钮只会最小化到托盘，因此超时后强制结束）。</summary>
        public static bool CloseRunningApp()
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName("BossTodo"))
                {
                    try
                    {
                        process.CloseMainWindow();
                        if (!process.WaitForExit(1500)) process.Kill();
                        process.WaitForExit(3000);
                    }
                    catch { }
                    finally { process.Dispose(); }
                }
                return !AppRunning();
            }
            catch
            {
                return false;
            }
        }
    }
}
