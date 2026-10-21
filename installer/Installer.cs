using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BossTodo.Setup
{
    internal static class Installer
    {
        private const string UninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\BossTodo";

        public static int Run(string targetDir, bool silent, bool launch, bool shortcuts, bool desktopShortcut)
        {
            try
            {
                string sourceDir = AppDomain.CurrentDomain.BaseDirectory;
                string sourceExe = Path.Combine(sourceDir, "BossTodo.exe");
                if (!File.Exists(sourceExe))
                {
                    Program.Log("找不到 BossTodo.exe：" + sourceExe);
                    return 3;
                }
                if (Program.AppRunning())
                {
                    Program.Log("安装中止：Boss 待办正在运行");
                    return 2;
                }

                targetDir = Path.GetFullPath(targetDir);
                Directory.CreateDirectory(targetDir);

                string targetExe = Path.Combine(targetDir, "BossTodo.exe");
                CopyIfNeeded(sourceExe, targetExe);
                CopyIfNeeded(Path.Combine(sourceDir, "BossTodo.ico"), Path.Combine(targetDir, "BossTodo.ico"));
                CopyIfNeeded(Path.Combine(sourceDir, "README.txt"), Path.Combine(targetDir, "README.txt"));

                string self = Application.ExecutablePath;
                string uninstaller = Path.Combine(targetDir, "Uninstall.exe");
                CopyIfNeeded(self, uninstaller);

                if (shortcuts) CreateShortcuts(targetDir, desktopShortcut);
                WriteUninstallEntry(targetDir);

                if (launch)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(targetExe)
                        {
                            WorkingDirectory = targetDir,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        Program.Log("安装完成但启动失败：" + ex.Message);
                    }
                }

                Program.Log("安装成功：" + targetDir);
                return 0;
            }
            catch (Exception ex)
            {
                Program.Log("安装失败：" + ex);
                return 1;
            }
        }

        private static void CopyIfNeeded(string source, string target)
        {
            if (!File.Exists(source)) return;
            if (string.Equals(Path.GetFullPath(source), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                return;
            File.Copy(source, target, true);
        }

        /// <summary>写入“设置 → 应用 → 已安装的应用”中的卸载入口。</summary>
        private static void WriteUninstallEntry(string targetDir)
        {
            try
            {
                string exe = Path.Combine(targetDir, "BossTodo.exe");
                string icon = Path.Combine(targetDir, "BossTodo.ico");
                string uninstaller = Path.Combine(targetDir, "Uninstall.exe");
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(UninstallKeyPath))
                {
                    if (key == null) return;
                    key.SetValue("DisplayName", "Boss 待办", RegistryValueKind.String);
                    key.SetValue("DisplayVersion", "1.0.0", RegistryValueKind.String);
                    key.SetValue("Publisher", "BossTodo", RegistryValueKind.String);
                    key.SetValue("Comments", "四象限待办便签", RegistryValueKind.String);
                    key.SetValue("InstallLocation", targetDir, RegistryValueKind.String);
                    key.SetValue("DisplayIcon", File.Exists(icon) ? icon : exe, RegistryValueKind.String);
                    key.SetValue("UninstallString", "\"" + uninstaller + "\" --uninstall", RegistryValueKind.String);
                    key.SetValue("QuietUninstallString", "\"" + uninstaller + "\" /S --uninstall", RegistryValueKind.String);
                    key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                    key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"), RegistryValueKind.String);
                    try
                    {
                        key.SetValue("EstimatedSize", (int)(new FileInfo(exe).Length / 1024), RegistryValueKind.DWord);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Program.Log("写入卸载注册表项失败：" + ex.Message);
            }
        }

        public static bool CreateShortcuts(string targetDir, bool desktopShortcut)
        {
            try
            {
                string exe = Path.Combine(targetDir, "BossTodo.exe");
                string icon = Path.Combine(targetDir, "BossTodo.ico");
                if (!File.Exists(icon)) icon = exe;

                if (desktopShortcut)
                {
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    CreateShortcut(Path.Combine(desktop, "Boss 待办.lnk"), exe, string.Empty, icon,
                        "Boss 待办 - 四象限便签", targetDir);
                }

                string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string group = Path.Combine(programs, "Boss 待办");
                Directory.CreateDirectory(group);
                CreateShortcut(Path.Combine(group, "Boss 待办.lnk"), exe, string.Empty, icon,
                    "Boss 待办 - 四象限便签", targetDir);
                CreateShortcut(Path.Combine(group, "卸载 Boss 待办.lnk"),
                    Path.Combine(targetDir, "Uninstall.exe"), "--uninstall", icon,
                    "卸载 Boss 待办", targetDir);
                return true;
            }
            catch (Exception ex)
            {
                Program.Log("创建快捷方式失败：" + ex.Message);
                return false;
            }
        }

        private static bool CreateShortcut(string shortcutPath, string target, string arguments,
            string icon, string description, string workingDir)
        {
            object shell = null;
            object shortcut = null;
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return false;
                shell = Activator.CreateInstance(shellType);
                shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod,
                    null, shell, new object[] { shortcutPath });
                Type shortcutType = shortcut.GetType();
                shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { target });
                shortcutType.InvokeMember("Arguments", BindingFlags.SetProperty, null, shortcut, new object[] { arguments });
                shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { workingDir });
                shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { icon + ",0" });
                shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, new object[] { description });
                shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
                return File.Exists(shortcutPath);
            }
            catch (Exception ex)
            {
                Program.Log("创建快捷方式失败：" + shortcutPath + " : " + ex.Message);
                return false;
            }
            finally
            {
                if (shortcut != null && Marshal.IsComObject(shortcut)) { try { Marshal.ReleaseComObject(shortcut); } catch { } }
                if (shell != null && Marshal.IsComObject(shell)) { try { Marshal.ReleaseComObject(shell); } catch { } }
            }
        }
    }
}
