using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace BossTodo.Core
{
    /// <summary>通过 HKCU Run 实现“开机自动启动并语音播报”。</summary>
    public static class AutoStart
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "BossTodo";

        public static string Command
        {
            get { return "\"" + Application.ExecutablePath + "\" -autostart"; }
        }

        public static bool IsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                {
                    if (key == null) return false;
                    object value = key.GetValue(ValueName);
                    return value != null && value.ToString().Length > 0;
                }
            }
            catch { return false; }
        }

        /// <summary>注册项存在但路径变化（移动/重装 exe）时自动修正。</summary>
        public static void RepairIfEnabled()
        {
            try
            {
                if (!IsEnabled()) return;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                {
                    object value = key == null ? null : key.GetValue(ValueName);
                    string current = value == null ? string.Empty : value.ToString();
                    if (!string.Equals(current, Command, StringComparison.OrdinalIgnoreCase))
                    {
                        SetEnabled(true);
                    }
                }
            }
            catch { }
        }

        public static bool SetEnabled(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
                {
                    if (key == null) return false;
                    if (enabled) key.SetValue(ValueName, Command, RegistryValueKind.String);
                    else
                    {
                        if (key.GetValue(ValueName) != null) key.DeleteValue(ValueName, false);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                AppPaths.LogError("设置开机启动失败：" + ex.Message);
                return false;
            }
        }

        public static void Remove()
        {
            SetEnabled(false);
        }
    }
}
