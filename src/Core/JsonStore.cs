using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace BossTodo.Core
{
    /// <summary>轻量 JSON 读写：UTF-8、原子写入、自动备份、损坏文件保护。</summary>
    public static class JsonStore
    {
        private static JavaScriptSerializer CreateSerializer()
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            s.MaxJsonLength = 32 * 1024 * 1024;
            s.RecursionLimit = 100;
            return s;
        }

        public static T Load<T>(string path, T fallback) where T : class
        {
            try
            {
                if (!File.Exists(path)) return fallback;
                string json = File.ReadAllText(path, Encoding.UTF8);
                if (string.IsNullOrEmpty(json) || json.Trim().Length == 0) return fallback;
                T value = CreateSerializer().Deserialize<T>(json);
                return value ?? fallback;
            }
            catch (Exception ex)
            {
                AppPaths.LogError("读取失败 " + path + " : " + ex.Message);
                TryQuarantine(path);
                return fallback;
            }
        }

        public static bool Save<T>(string path, T value)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                string json = CreateSerializer().Serialize(value);
                string tmp = path + ".tmp";
                File.WriteAllText(tmp, json, new UTF8Encoding(false));

                if (File.Exists(path))
                {
                    string bak = path + ".bak";
                    try { File.Replace(tmp, path, bak); }
                    catch
                    {
                        File.Copy(tmp, path, true);
                        TryDelete(tmp);
                    }
                }
                else
                {
                    File.Move(tmp, path);
                }
                return true;
            }
            catch (Exception ex)
            {
                AppPaths.LogError("保存失败 " + path + " : " + ex.Message);
                return false;
            }
        }

        private static void TryQuarantine(string path)
        {
            try
            {
                if (!File.Exists(path)) return;
                string bad = path + ".corrupt-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                File.Move(path, bad);
            }
            catch { }
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { }
        }
    }
}
