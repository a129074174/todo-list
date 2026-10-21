using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BossTodo.Core
{
    /// <summary>使用 IShellLink COM 创建 .lnk 快捷方式（安装器与自启动辅助都用得上）。</summary>
    public static class ShortcutHelper
    {
        [ComImport, Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink
        {
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, IntPtr pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010b-0000-0000-C000-000000000046")]
        private interface IPersistFile
        {
            void GetClassID(out Guid pClassID);
            [PreserveSig]
            int IsDirty();
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }

        public static bool Create(string shortcutPath, string targetPath, string arguments, string description, string iconPath)
        {
            object linkObject = null;
            try
            {
                string dir = Path.GetDirectoryName(shortcutPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                linkObject = new ShellLink();
                IShellLinkW link = (IShellLinkW)linkObject;
                link.SetPath(targetPath);
                if (!string.IsNullOrEmpty(arguments)) link.SetArguments(arguments);
                if (!string.IsNullOrEmpty(description)) link.SetDescription(description);
                string work = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(work)) link.SetWorkingDirectory(work);
                if (!string.IsNullOrEmpty(iconPath)) link.SetIconLocation(iconPath, 0);

                ((IPersistFile)linkObject).Save(shortcutPath, true);
                return File.Exists(shortcutPath);
            }
            catch (Exception ex)
            {
                AppPaths.LogError("创建快捷方式失败：" + ex.Message);
                return false;
            }
            finally
            {
                if (linkObject != null && Marshal.IsComObject(linkObject))
                {
                    try { Marshal.ReleaseComObject(linkObject); }
                    catch { }
                }
            }
        }

        public static string DesktopShortcutPath(string name)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            return Path.Combine(desktop, name + ".lnk");
        }

        public static string StartMenuShortcutPath(string folder, string name)
        {
            string start = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            return Path.Combine(start, folder, name + ".lnk");
        }

        public static void DeleteShortcut(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { }
        }
    }
}
