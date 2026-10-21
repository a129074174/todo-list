using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BossTodo.Core;
using BossTodo.UI;

namespace BossTodo
{
    internal static partial class Program
    {
        private static void PrepareDemoData(string dataDir)
        {
            try { if (Directory.Exists(dataDir)) Directory.Delete(dataDir, true); }
            catch { }
            Directory.CreateDirectory(dataDir);

            TaskStore store = new TaskStore(dataDir);
            DateTime now = DateTime.Now;

            TaskItem report = TaskItem.Create("提交季度项目报告");
            report.Note = "附上数据看板截图与风险清单";
            report.Importance = Importance.High;
            report.HasDue = true;
            report.DueAt = now.AddHours(3);
            report.IsPinned = true;
            report.CreatedAt = now.AddDays(-3);
            store.Tasks.Add(report);

            TaskItem mail = TaskItem.Create("回复客户的报价邮件");
            mail.Importance = Importance.Low;
            mail.HasDue = true;
            mail.DueAt = now.AddHours(20);
            mail.CreatedAt = now.AddHours(-2);
            store.Tasks.Add(mail);

            TaskItem meeting = TaskItem.Create("整理下周产品评审会议议程");
            meeting.Importance = Importance.Medium;
            meeting.HasDue = true;
            meeting.DueAt = now.Date.AddDays(4).AddHours(15);
            meeting.CreatedAt = now.AddDays(-1);
            store.Tasks.Add(meeting);

            TaskItem read = TaskItem.Create("阅读 AI 行业趋势报告");
            read.Importance = Importance.Low;
            read.CreatedAt = now.AddDays(-4);
            store.Tasks.Add(read);

            TaskItem resume = TaskItem.Create("更新个人简历");
            resume.Importance = Importance.Low;
            resume.HasDue = true;
            resume.DueAt = now.AddDays(-2).Date.AddHours(18);
            resume.CreatedAt = now.AddDays(-6);
            store.Tasks.Add(resume);

            TaskItem checkup = TaskItem.Create("预约年度体检");
            checkup.Importance = Importance.High;
            checkup.Note = "记得空腹，带上医保卡";
            checkup.CreatedAt = now.AddDays(-5);
            store.Tasks.Add(checkup);

            TaskItem done = TaskItem.Create("整理桌面便签");
            done.Importance = Importance.Medium;
            done.IsDone = true;
            done.CompletedAt = now.AddHours(-4);
            done.CreatedAt = now.AddDays(-2);
            store.Tasks.Add(done);

            store.SaveTasks();
        }

        private static void RenderPreviews(string previewDir, string dataDir)
        {
            MainForm form = new MainForm(dataDir, false, true);
            form.Show();
            Application.DoEvents();
            ApplyIdle();
            SaveForm(form, Path.Combine(previewDir, "main.png"));

            TaskStore store = new TaskStore(dataDir);
            SpeechService speech = new SpeechService();
            using (SettingsDialog settings = new SettingsDialog(store, speech))
            {
                settings.Show(form);
                Application.DoEvents();
                ApplyIdle();
                SaveForm(settings, Path.Combine(previewDir, "settings.png"));
                settings.ScrollSettings(400);
                Application.DoEvents();
                ApplyIdle();
                SaveForm(settings, Path.Combine(previewDir, "settings-bottom.png"));
                settings.Close();
            }

            using (EditTaskDialog editor = new EditTaskDialog(store.Tasks.Count > 0 ? store.Tasks[0] : null))
            {
                editor.Show(form);
                Application.DoEvents();
                ApplyIdle();
                SaveForm(editor, Path.Combine(previewDir, "editor.png"));
                editor.Close();
            }

            form.Close();
            speech.Dispose();
        }

        private static void ApplyIdle()
        {
            for (int i = 0; i < 4; i++)
            {
                Application.DoEvents();
                Thread.Sleep(120);
            }
        }

        private static void SaveForm(Form form, string path)
        {
            form.Region = null;
            form.Refresh();
            Application.DoEvents();

            using (Bitmap bmp = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(bmp, new Rectangle(0, 0, form.Width, form.Height));
                bmp.Save(path, ImageFormat.Png);
            }
        }
    }
}
