using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BossTodo.Setup
{
    internal static class Uninstaller
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunValueName = "BossTodo";
        private const string UninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\BossTodo";

        public static int Run(string installDir, bool silent, bool deleteData)
        {
            try
            {
                if (Program.AppRunning())
                {
                    Program.Log("卸载中止：Boss 待办正在运行");
                    return 2;
                }

                string self = Application.ExecutablePath;
                string selfDir = Path.GetDirectoryName(Path.GetFullPath(self));
                if (string.IsNullOrEmpty(installDir))
                    installDir = selfDir;
                installDir = Path.GetFullPath(installDir);

                RemoveShortcuts();
                RemoveRunValue();

                bool hasApp = File.Exists(Path.Combine(installDir, "BossTodo.exe"));
                bool looksInstalled = hasApp || File.Exists(Path.Combine(installDir, "Uninstall.exe"));
                bool selfInside = string.Equals(selfDir, installDir, StringComparison.OrdinalIgnoreCase);
                bool keepDataDir = Directory.Exists(Path.Combine(installDir, "Data"));

                if (deleteData) DeleteDataFolder();

                if (looksInstalled || selfInside)
                {
                    TryDelete(Path.Combine(installDir, "BossTodo.exe"));
                    TryDelete(Path.Combine(installDir, "BossTodo.ico"));
                    TryDelete(Path.Combine(installDir, "README.txt"));
                    TryDelete(Path.Combine(installDir, "BossTodo.portable"));
                    TryDelete(Path.Combine(installDir, "install.log"));
                }

                RemoveUninstallEntry(installDir);

                // 只有确认是安装目录且没有需要保留的 Data\ 时，才在退出后递归删除整个目录。
                if (selfInside) ScheduleSelfDelete(self, installDir, hasApp && !keepDataDir);

                Program.Log("卸载完成：" + installDir);
                return 0;
            }
            catch (Exception ex)
            {
                Program.Log("卸载失败：" + ex);
                return 1;
            }
        }

        private static void RemoveShortcuts()
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                TryDelete(Path.Combine(desktop, "Boss 待办.lnk"));

                string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string group = Path.Combine(programs, "Boss 待办");
                if (Directory.Exists(group)) Directory.Delete(group, true);
            }
            catch (Exception ex)
            {
                Program.Log("删除快捷方式失败：" + ex.Message);
            }
        }

        private static void RemoveRunValue()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
                {
                    if (key != null && key.GetValue(RunValueName) != null)
                        key.DeleteValue(RunValueName, false);
                }
            }
            catch (Exception ex)
            {
                Program.Log("删除开机启动项失败：" + ex.Message);
            }
        }

        private static void RemoveUninstallEntry(string installDir)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(UninstallKeyPath))
                {
                    if (key == null) return;
                    string location = key.GetValue("InstallLocation") as string;
                    if (!string.IsNullOrEmpty(location) &&
                        !string.Equals(Path.GetFullPath(location).TrimEnd('\\'),
                                       installDir.TrimEnd('\\'),
                                       StringComparison.OrdinalIgnoreCase))
                    {
                        // 注册表项指向另一个安装目录，不要删错。
                        return;
                    }
                }
                Registry.CurrentUser.DeleteSubKeyTree(UninstallKeyPath, false);
            }
            catch (Exception ex)
            {
                Program.Log("删除卸载注册表项失败：" + ex.Message);
            }
        }

        private static void DeleteDataFolder()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dir = Path.Combine(appData, "BossTodo");
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
            catch (Exception ex)
            {
                Program.Log("删除任务数据失败：" + ex.Message);
            }
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception ex) { Program.Log("删除失败 " + path + " : " + ex.Message); }
        }

        private static void ScheduleSelfDelete(string self, string installDir, bool removeInstallDir)
        {
            try
            {
                string command = "/c ping 127.0.0.1 -n 3 >nul & del /f /q \"" + self + "\"";
                if (removeInstallDir && CanRemoveDirectory(installDir))
                    command += " & rmdir /s /q \"" + installDir + "\"";
                Process.Start(new ProcessStartInfo("cmd.exe", command)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetTempPath()
                });
            }
            catch (Exception ex)
            {
                Program.Log("安排卸载程序自删除失败：" + ex.Message);
            }
        }

        private static bool CanRemoveDirectory(string dir)
        {
            try
            {
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return false;
                string full = Path.GetFullPath(dir).TrimEnd('\\');
                string root = Path.GetPathRoot(full);
                if (string.IsNullOrEmpty(root)) return false;
                // 防止把盘符根目录（如 C:\）当成安装目录递归删除。
                return !string.Equals(full, root.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
