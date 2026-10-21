using System;
using System.IO;
using System.Windows.Forms;

namespace BossTodo.Core
{
    /// <summary>数据目录：默认 %APPDATA%\BossTodo；绿色版（exe 同目录有 BossTodo.portable）使用 .\Data。</summary>
    public static class AppPaths
    {
        public const string PortableFlagName = "BossTodo.portable";

        public static string ExeDir
        {
            get { return Path.GetDirectoryName(Application.ExecutablePath); }
        }

        public static bool IsPortable
        {
            get { return File.Exists(Path.Combine(ExeDir, PortableFlagName)); }
        }

        public static string DataDir
        {
            get
            {
                string overrideDir = Environment.GetEnvironmentVariable("BOSSTODO_DATA");
                if (!string.IsNullOrEmpty(overrideDir)) return overrideDir;
                if (IsPortable) return Path.Combine(ExeDir, "Data");
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BossTodo");
            }
        }

        public static string TasksFile
        {
            get { return Path.Combine(DataDir, "tasks.json"); }
        }

        public static string SettingsFile
        {
            get { return Path.Combine(DataDir, "settings.json"); }
        }

        public static string LogFile
        {
            get { return Path.Combine(DataDir, "error.log"); }
        }

        public static void EnsureDataDir()
        {
            try { Directory.CreateDirectory(DataDir); }
            catch { }
        }

        public static void LogError(string message)
        {
            try
            {
                EnsureDataDir();
                File.AppendAllText(LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + message + Environment.NewLine);
            }
            catch { }
        }
    }
}
