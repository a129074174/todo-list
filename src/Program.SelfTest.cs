using System;
using System.Collections.Generic;
using System.IO;
using BossTodo.Core;

namespace BossTodo
{
    internal static partial class Program
    {
        /// <summary>--selftest：不启动界面，验证核心逻辑与持久化。</summary>
        private static bool RunSelfTest()
        {
            int passed = 0;
            int failed = 0;
            Action<bool, string> check = delegate(bool ok, string name)
            {
                if (ok) { passed++; Console.WriteLine("[PASS] " + name); }
                else { failed++; Console.WriteLine("[FAIL] " + name); }
            };

            DateTime now = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Local);

            TaskItem q1 = TaskItem.Create("urgent important");
            q1.Importance = Importance.High;
            q1.HasDue = true;
            q1.DueAt = now.AddHours(2);
            q1.CreatedAt = now;

            TaskItem q2 = TaskItem.Create("urgent unimportant");
            q2.Importance = Importance.Low;
            q2.HasDue = true;
            q2.DueAt = now.AddDays(-1);
            q2.CreatedAt = now;

            TaskItem q3 = TaskItem.Create("not urgent important");
            q3.Importance = Importance.Medium;
            q3.HasDue = false;
            q3.CreatedAt = now;

            TaskItem q4 = TaskItem.Create("not urgent unimportant");
            q4.Importance = Importance.Low;
            q4.HasDue = true;
            q4.DueAt = now.AddDays(10);
            q4.CreatedAt = now;

            check(PriorityEngine.GetQuadrant(q1, now, 2) == Quadrant.UrgentImportant, "quadrant q1");
            check(PriorityEngine.GetQuadrant(q2, now, 2) == Quadrant.UrgentUnimportant, "quadrant q2");
            check(PriorityEngine.GetQuadrant(q3, now, 2) == Quadrant.NotUrgentImportant, "quadrant q3");
            check(PriorityEngine.GetQuadrant(q4, now, 2) == Quadrant.NotUrgentUnimportant, "quadrant q4");

            TaskItem edge = TaskItem.Create("edge");
            edge.HasDue = true;
            edge.DueAt = now.AddDays(2);
            check(PriorityEngine.IsUrgent(edge, now, 2), "urgent boundary inclusive");
            edge.DueAt = now.AddDays(2).AddSeconds(1);
            check(!PriorityEngine.IsUrgent(edge, now, 2), "not urgent past boundary");

            TaskItem due = TaskItem.Create("due text");
            due.HasDue = true;
            due.DueAt = now.AddHours(-3);
            check(PriorityEngine.DueText(due, now) == "已逾期 3 小时", "overdue hours text");
            due.DueAt = now.AddDays(-2);
            check(PriorityEngine.DueText(due, now) == "已逾期 2 天", "overdue days text");
            due.DueAt = now.AddHours(2);
            check(PriorityEngine.DueText(due, now) == "今天 12:00", "today text");
            due.DueAt = now.AddDays(1);
            check(PriorityEngine.DueText(due, now) == "明天 10:00", "tomorrow text");
            due.DueAt = now.AddDays(3);
            check(PriorityEngine.DueText(due, now) == "3 天后", "near days text");
            due.DueAt = new DateTime(2026, 10, 31, 9, 0, 0);
            check(PriorityEngine.DueText(due, now) == "10月31日", "same year date text");
            due.DueAt = new DateTime(2027, 1, 5, 9, 0, 0);
            check(PriorityEngine.DueText(due, now) == "2027年1月5日", "next year date text");
            due.DueAt = now.AddMinutes(25);
            check(PriorityEngine.RemainingText(due, now) == "剩 25 分钟", "remaining minutes");
            due.DueAt = now.AddHours(5);
            check(PriorityEngine.RemainingText(due, now) == "剩 5 小时", "remaining hours");
            due.DueAt = now.AddHours(26);
            check(PriorityEngine.RemainingText(due, now) == null, "no remaining over a day");

            TaskItem first = TaskItem.Create("first");
            first.Importance = Importance.High;
            first.HasDue = true;
            first.DueAt = now.AddHours(10);
            first.CreatedAt = now;
            TaskItem second = TaskItem.Create("second");
            second.Importance = Importance.High;
            second.HasDue = true;
            second.DueAt = now.AddHours(1);
            second.CreatedAt = now;
            List<TaskItem> sorted = PriorityEngine.SortPending(new TaskItem[] { first, second }, now, 2);
            check(sorted[0] == second, "sort by due time");
            first.IsPinned = true;
            sorted = PriorityEngine.SortPending(new TaskItem[] { second, first }, now, 2);
            check(sorted[0] == first, "pinned sorts first");
            first.IsPinned = false;
            first.ManualOrder = 2;
            second.ManualOrder = 1;
            sorted = PriorityEngine.SortPending(new TaskItem[] { first, second }, now, 2);
            check(sorted[0] == second, "manual order respected");

            string announce = PriorityEngine.BuildAnnouncement(new TaskItem[] { q1, q2 }, now, 2);
            check(announce.Contains("2 项任务待完成"), "announcement pending count");
            check(announce.Contains("1 项已逾期"), "announcement overdue count");
            check(announce.Contains("urgent important"), "announcement top task");

            AppSettings settings = new AppSettings();
            settings.UrgentDays = 99;
            settings.SpeechRate = -99;
            settings.SpeechVolume = 200;
            settings.Normalize();
            check(settings.UrgentDays == 2 && settings.SpeechRate == -10 && settings.SpeechVolume == 100,
                "settings normalize");

            string temp = Path.Combine(Path.GetTempPath(), "BossTodoSelfTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);
            string previousDataDir = Environment.GetEnvironmentVariable("BOSSTODO_DATA");
            Environment.SetEnvironmentVariable("BOSSTODO_DATA", temp);
            try
            {
                TaskStore store = new TaskStore(temp);
                check(store.FirstRun, "first run detected");
                check(store.Tasks.Count == 0, "empty task list");

                TaskItem persisted = TaskItem.Create("persisted");
                persisted.Note = "note";
                persisted.Importance = Importance.High;
                persisted.HasDue = true;
                persisted.DueAt = new DateTime(2026, 10, 22, 2, 10, 0, DateTimeKind.Local);
                persisted.CreatedAt = new DateTime(2026, 10, 21, 23, 10, 0, DateTimeKind.Local);
                persisted.ManualOrder = 3;
                store.Add(persisted);
                check(store.SaveTasks(), "save tasks");

                string json = File.ReadAllText(store.TasksPath);
                check(json.Contains("DueTicks") && json.Contains("CreatedTicks"), "ticks persisted");
                check(json.IndexOf("\"DueAt\"", StringComparison.Ordinal) < 0, "no local-time date strings");

                TaskStore loaded = new TaskStore(temp);
                check(loaded.Tasks.Count == 1, "reload task count");
                check(!loaded.FirstRun, "settings exist so not first run");
                check(loaded.Tasks[0].DueAt == persisted.DueAt, "due time round trip exact");
                check(loaded.Tasks[0].CreatedAt == persisted.CreatedAt, "created time round trip exact");
                check(loaded.Tasks[0].Note == "note" && loaded.Tasks[0].Importance == Importance.High,
                    "fields round trip");
                check(loaded.Tasks[0].ManualOrder == 3, "manual order round trip");

                TaskItem a = TaskItem.Create("A");
                TaskItem b = TaskItem.Create("B");
                TaskItem c = TaskItem.Create("C");
                loaded.Tasks.Clear();
                loaded.Tasks.Add(a);
                loaded.Tasks.Add(b);
                loaded.Tasks.Add(c);
                loaded.RenumberManualOrder(new TaskItem[] { c, a });
                check(c.ManualOrder == 1 && a.ManualOrder == 2 && b.ManualOrder == 0,
                    "renumber manual order");
                check(loaded.HasManualOrder, "has manual order");
                loaded.ClearManualOrder();
                check(!loaded.HasManualOrder, "clear manual order");

                BackupData backup = new BackupData();
                backup.Tasks.Add(persisted);
                backup.Settings.UrgentDays = 5;
                backup.Settings.AutoStart = false;
                string backupPath = Path.Combine(temp, "backup.json");
                check(JsonStore.Save(backupPath, backup), "save backup");
                BackupData restored = JsonStore.Load<BackupData>(backupPath, null);
                check(restored != null && restored.Tasks.Count == 1, "load backup");
                check(restored.Tasks[0].DueAt == persisted.DueAt, "backup due time exact");
                check(restored.Settings.UrgentDays == 5 && !restored.Settings.AutoStart, "backup settings");

                string corrupt = Path.Combine(temp, "corrupt.json");
                File.WriteAllText(corrupt, "{ not valid json");
                List<TaskItem> fallback = JsonStore.Load<List<TaskItem>>(corrupt, null);
                check(fallback == null, "corrupt json returns fallback");
                check(Directory.GetFiles(temp, "corrupt.json.corrupt-*").Length == 1,
                    "corrupt file quarantined");
            }
            finally
            {
                try { Directory.Delete(temp, true); }
                catch { }
                Environment.SetEnvironmentVariable("BOSSTODO_DATA", previousDataDir);
            }

            Console.WriteLine("SelfTest: " + passed + " passed, " + failed + " failed");
            return failed == 0;
        }
    }
}
