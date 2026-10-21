using System;
using System.Collections.Generic;
using System.Text;

namespace BossTodo.Core
{
    /// <summary>
    /// 优先级 / 四象限 / 到期文案的纯函数集合。不依赖界面，便于自测。
    /// </summary>
    public static class PriorityEngine
    {
        /// <summary>是否紧迫：已逾期，或距离截止时间不超过 urgentDays 天。</summary>
        public static bool IsUrgent(TaskItem t, DateTime now, int urgentDays)
        {
            if (t == null || !t.HasDue) return false;
            if (urgentDays < 1) urgentDays = 1;
            return t.DueAt <= now.AddDays(urgentDays);
        }

        public static bool IsImportant(TaskItem t)
        {
            return t != null && t.Importance != Importance.Low;
        }

        public static Quadrant GetQuadrant(TaskItem t, DateTime now, int urgentDays)
        {
            bool urgent = IsUrgent(t, now, urgentDays);
            bool important = IsImportant(t);
            if (urgent && important) return Quadrant.UrgentImportant;
            if (urgent && !important) return Quadrant.UrgentUnimportant;
            if (!urgent && important) return Quadrant.NotUrgentImportant;
            return Quadrant.NotUrgentUnimportant;
        }

        /// <summary>1 天内必须完成（含已逾期）→ 用红色强调。</summary>
        public static bool IsDueSoon(TaskItem t, DateTime now)
        {
            return t != null && t.HasDue && t.DueAt <= now.AddDays(1);
        }

        public static bool IsOverdue(TaskItem t, DateTime now)
        {
            return t != null && !t.IsDone && t.HasDue && t.DueAt < now;
        }

        /// <summary>形如：已逾期 2 天 / 今天 18:00 / 明天 09:30 / 3 天后 / 10月28日。</summary>
        public static string DueText(TaskItem t, DateTime now)
        {
            if (t == null || !t.HasDue) return "未设截止";
            DateTime due = t.DueAt;
            int dayDiff = (due.Date - now.Date).Days;

            if (due < now)
            {
                TimeSpan past = now - due;
                if (dayDiff == 0)
                {
                    if (past.TotalHours < 1) return "已逾期 " + Math.Max(1, (int)Math.Ceiling(past.TotalMinutes)) + " 分钟";
                    return "已逾期 " + Math.Max(1, (int)Math.Floor(past.TotalHours)) + " 小时";
                }
                return "已逾期 " + (-dayDiff) + " 天";
            }

            if (dayDiff == 0) return "今天 " + due.ToString("HH:mm");
            if (dayDiff == 1) return "明天 " + due.ToString("HH:mm");
            if (dayDiff < 7) return dayDiff + " 天后";
            if (due.Year == now.Year) return due.ToString("M月d日");
            return due.ToString("yyyy年M月d日");
        }

        /// <summary>24 小时内的剩余时间短文案，用于红色强调；否则返回 null。</summary>
        public static string RemainingText(TaskItem t, DateTime now)
        {
            if (t == null || !t.HasDue || t.IsDone) return null;
            if (t.DueAt <= now) return null;
            TimeSpan left = t.DueAt - now;
            if (left.TotalHours >= 24) return null;
            if (left.TotalHours < 1) return "剩 " + Math.Max(1, (int)Math.Ceiling(left.TotalMinutes)) + " 分钟";
            return "剩 " + Math.Max(1, (int)Math.Floor(left.TotalHours)) + " 小时";
        }

        public static string QuadrantName(Quadrant q)
        {
            switch (q)
            {
                case Quadrant.UrgentImportant: return "紧迫重要";
                case Quadrant.UrgentUnimportant: return "紧迫不重要";
                case Quadrant.NotUrgentImportant: return "不紧迫重要";
                default: return "不紧迫不重要";
            }
        }

        public static string QuadrantHint(Quadrant q)
        {
            switch (q)
            {
                case Quadrant.UrgentImportant: return "马上做";
                case Quadrant.UrgentUnimportant: return "尽快处理";
                case Quadrant.NotUrgentImportant: return "安排时间";
                default: return "有空再做";
            }
        }

        private static int ManualKey(TaskItem t)
        {
            return t.ManualOrder > 0 ? t.ManualOrder : int.MaxValue;
        }

        public static int ComparePending(TaskItem a, TaskItem b, DateTime now, int urgentDays)
        {
            int qa = (int)GetQuadrant(a, now, urgentDays);
            int qb = (int)GetQuadrant(b, now, urgentDays);
            if (qa != qb) return qa.CompareTo(qb);

            if (a.IsPinned != b.IsPinned) return a.IsPinned ? -1 : 1;

            int ma = ManualKey(a);
            int mb = ManualKey(b);
            if (ma != mb) return ma.CompareTo(mb);

            if (a.HasDue != b.HasDue) return a.HasDue ? -1 : 1;
            if (a.HasDue && a.DueAt != b.DueAt) return a.DueAt.CompareTo(b.DueAt);
            return a.CreatedAt.CompareTo(b.CreatedAt);
        }

        /// <summary>返回已排序的新列表：未完成在前（按四象限优先级），已完成在后（按完成时间倒序）。</summary>
        public static List<TaskItem> SortAll(IEnumerable<TaskItem> tasks, DateTime now, int urgentDays)
        {
            List<TaskItem> pending = new List<TaskItem>();
            List<TaskItem> done = new List<TaskItem>();
            foreach (TaskItem t in tasks)
            {
                if (t == null) continue;
                if (t.IsDone) done.Add(t); else pending.Add(t);
            }
            pending.Sort(delegate(TaskItem a, TaskItem b) { return ComparePending(a, b, now, urgentDays); });
            done.Sort(delegate(TaskItem a, TaskItem b)
            {
                int c = b.CompletedAt.CompareTo(a.CompletedAt);
                if (c != 0) return c;
                return b.CreatedAt.CompareTo(a.CreatedAt);
            });
            List<TaskItem> result = new List<TaskItem>(pending.Count + done.Count);
            result.AddRange(pending);
            result.AddRange(done);
            return result;
        }

        public static List<TaskItem> SortPending(IEnumerable<TaskItem> tasks, DateTime now, int urgentDays)
        {
            List<TaskItem> pending = new List<TaskItem>();
            foreach (TaskItem t in tasks)
            {
                if (t != null && !t.IsDone) pending.Add(t);
            }
            pending.Sort(delegate(TaskItem a, TaskItem b) { return ComparePending(a, b, now, urgentDays); });
            return pending;
        }

        public static string BuildAnnouncement(IEnumerable<TaskItem> tasks, DateTime now, int urgentDays)
        {
            List<TaskItem> pending = SortPending(tasks, now, urgentDays);
            if (pending.Count == 0)
            {
                return "Boss，您还有 0 项任务待完成，今天可以放松一下。";
            }

            int overdue = 0;
            int today = 0;
            foreach (TaskItem t in pending)
            {
                if (!t.HasDue) continue;
                if (t.DueAt < now) overdue++;
                else if (t.DueAt.Date == now.Date) today++;
            }

            string top = pending[0].Title;
            if (string.IsNullOrEmpty(top)) top = "未命名任务";
            if (top.Length > 24) top = top.Substring(0, 23) + "…";

            StringBuilder sb = new StringBuilder();
            sb.Append("Boss，您还有 ");
            sb.Append(pending.Count);
            sb.Append(" 项任务待完成");
            if (overdue > 0)
            {
                sb.Append("，其中 ");
                sb.Append(overdue);
                sb.Append(" 项已逾期");
            }
            if (today > 0)
            {
                sb.Append("，");
                sb.Append(today);
                sb.Append(" 项今天到期");
            }
            sb.Append("。建议先完成：");
            sb.Append(top);
            sb.Append("。");
            return sb.ToString();
        }
    }
}
