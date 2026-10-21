using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using BossTodo.Core;
using BossTodo.UI;

namespace BossTodo
{
    internal static partial class Program
    {
        private static object ReadField(object instance, string name)
        {
            FieldInfo field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(instance);
        }

        private static object InvokePrivate(object instance, string name, params object[] args)
        {
            int count = args == null ? 0 : args.Length;
            foreach (MethodInfo method in instance.GetType().GetMethods(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (method.Name == name && method.GetParameters().Length == count)
                    return method.Invoke(instance, args);
            }
            throw new InvalidOperationException("找不到测试方法：" + name);
        }

        /// <summary>--smoke：真实窗口的启动/添加/完成/筛选/编辑/设置/关闭冒烟测试。</summary>
        private static bool RunSmokeTest()
        {
            int passed = 0;
            int failed = 0;
            Action<bool, string> check = delegate(bool ok, string name)
            {
                if (ok) { passed++; Console.WriteLine("[PASS] " + name); }
                else { failed++; Console.WriteLine("[FAIL] " + name); }
            };

            // 字体必须使用像素单位：界面坐标已按 DPI 缩放，字体若再按点数自动乘一次 DPI，
            // 高缩放下文字会比行高多出一截并压住相邻说明文字。
            Font titleFont = Theme.F(9.4f, FontStyle.Bold);
            check(titleFont.Unit == GraphicsUnit.Pixel, "fonts use pixel units (single DPI scaling)");
            check(titleFont.Height <= Ui.S(20), "settings title font fits row height");
            check(Theme.Tiny.Height <= Ui.S(18), "settings description font fits row height");

            string temp = Path.Combine(Path.GetTempPath(), "BossTodoSmoke_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);
            MainForm form = null;
            try
            {
                TaskStore setup = new TaskStore(temp);
                setup.Settings.AutoStart = false;
                setup.Settings.SpeakOnStartup = false;
                setup.Settings.SpeakOnEveryLaunch = false;
                setup.Settings.AlwaysOnTop = false;
                setup.Settings.CloseToTray = false;
                setup.Settings.HasPosition = false;
                setup.SaveSettings();

                form = new MainForm(temp, false, false);
                form.Show();
                Application.DoEvents();
                check(form.Visible && form.Width > 0 && form.Height > 0, "main window shown");
                check(ReadField(form, "_tray") != null, "tray icon created");

                TextBox input = ReadField(form, "_input") as TextBox;
                check(input != null, "quick input created");
                input.Text = "smoke quick add";
                InvokePrivate(form, "AddQuickTask");
                Application.DoEvents();

                TaskStore persisted = new TaskStore(temp);
                check(persisted.Tasks.Count == 1 && persisted.Tasks[0].Title == "smoke quick add",
                    "quick add persisted");

                TaskStore live = ReadField(form, "_store") as TaskStore;
                check(live != null && live.Tasks.Count == 1, "live store updated");
                TaskItem task = live.Tasks[0];
                InvokePrivate(form, "ToggleTaskDone", task);
                Application.DoEvents();
                check(task.IsDone && task.CompletedAt != default(DateTime), "toggle done");
                TaskStore afterDone = new TaskStore(temp);
                check(afterDone.Tasks[0].IsDone, "done state persisted");

                InvokePrivate(form, "TogglePin", task);
                Application.DoEvents();
                InvokePrivate(form, "SetFilter", "today");
                Application.DoEvents();
                InvokePrivate(form, "SetFilter", "all");
                Application.DoEvents();
                check(true, "filter switch does not throw");

                using (EditTaskDialog editor = new EditTaskDialog(task))
                {
                    Timer auto = new Timer();
                    auto.Interval = 300;
                    auto.Tick += delegate
                    {
                        auto.Stop();
                        try
                        {
                            TextBox title = ReadField(editor, "_titleBox") as TextBox;
                            title.Text = "smoke edited";
                            InvokePrivate(editor, "SaveAndClose");
                        }
                        catch
                        {
                            editor.Close();
                        }
                    };
                    auto.Start();
                    DialogResult result = editor.ShowDialog(form);
                    auto.Dispose();
                    check(result == DialogResult.OK, "editor modal returns OK");
                    check(editor.ResultTask != null && editor.ResultTask.Title == "smoke edited",
                        "editor returns edited title");
                }

                // 主窗口置顶时，编辑弹窗必须同步置顶，否则会被主窗口盖住（主窗口被模态禁用，看起来像卡死）。
                form.TopMost = true;
                using (EditTaskDialog topMostEditor = new EditTaskDialog(task))
                {
                    bool topMostSynced = false;
                    Timer auto = new Timer();
                    auto.Interval = 250;
                    auto.Tick += delegate
                    {
                        auto.Stop();
                        topMostSynced = topMostEditor.TopMost;
                        topMostEditor.Close();
                    };
                    auto.Start();
                    topMostEditor.ShowDialog(form);
                    auto.Dispose();
                    check(topMostSynced, "editor above always-on-top main window");
                }
                form.TopMost = false;

                using (SettingsDialog settings = new SettingsDialog(live, new SpeechService()))
                {
                    settings.Show(form);
                    Application.DoEvents();
                    InvokePrivate(settings, "ScrollSettings", new object[] { 240 });
                    Application.DoEvents();
                    int settingsOffset = (int)ReadField(settings, "_settingsScroll");
                    ThinScrollbar settingsBar = ReadField(settings, "_scroll") as ThinScrollbar;
                    check(settings.Visible && settingsOffset > 0, "settings shown and scrolled");
                    check(settingsBar != null && settingsBar.Visible && settingsBar.Value == settingsOffset,
                        "settings scrollbar synced");
                    settings.Close();
                }

                live.Settings.CloseToTray = true;
                form.Close();
                Application.DoEvents();
                check(!form.Visible && !form.IsDisposed, "close hides to tray");
                InvokePrivate(form, "RestoreFromTray");
                Application.DoEvents();
                check(form.Visible, "restore from tray");

                FieldInfo exitFlag = form.GetType().GetField("_reallyExit",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (exitFlag != null) exitFlag.SetValue(form, true);
                form.Close();
                Application.DoEvents();
                check(form.IsDisposed, "real exit disposes main window");
            }
            catch (Exception ex)
            {
                failed++;
                Console.WriteLine("[FAIL] smoke exception: " + ex);
            }
            finally
            {
                if (form != null && !form.IsDisposed)
                {
                    FieldInfo exitFlag = form.GetType().GetField("_reallyExit",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    if (exitFlag != null) exitFlag.SetValue(form, true);
                    form.Close();
                }
                Application.DoEvents();
                try { Directory.Delete(temp, true); }
                catch { }
            }

            Console.WriteLine("SmokeTest: " + passed + " passed, " + failed + " failed");
            return failed == 0;
        }
    }
}
