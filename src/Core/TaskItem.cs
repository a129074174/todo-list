using System;
using System.Web.Script.Serialization;

namespace BossTodo.Core
{
    /// <summary>重要程度。中及以上视为“重要”。</summary>
    public enum Importance
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    /// <summary>艾森豪威尔四象限，枚举顺序即显示顺序。</summary>
    public enum Quadrant
    {
        UrgentImportant = 0,
        UrgentUnimportant = 1,
        NotUrgentImportant = 2,
        NotUrgentUnimportant = 3
    }

    /// <summary>一条待办任务。字段保持简单，便于 JSON 持久化。</summary>
    public class TaskItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public bool HasDue { get; set; }
        public Importance Importance { get; set; }
        public bool IsDone { get; set; }
        public bool IsPinned { get; set; }
        /// <summary>0 表示未手动排序；大于 0 表示手动优先级（越小越靠前）。</summary>
        public int ManualOrder { get; set; }

        /// <summary>时间以 Ticks 持久化，避免 JavaScriptSerializer 的时区偏移。</summary>
        [ScriptIgnore]
        public DateTime DueAt { get; set; }

        [ScriptIgnore]
        public DateTime CreatedAt { get; set; }

        [ScriptIgnore]
        public DateTime CompletedAt { get; set; }

        public long DueTicks
        {
            get { return HasDue ? DueAt.Ticks : 0; }
            set { DueAt = value > 0 ? new DateTime(value) : default(DateTime); }
        }

        public long CreatedTicks
        {
            get { return CreatedAt.Ticks; }
            set { CreatedAt = value > 0 ? new DateTime(value) : default(DateTime); }
        }

        public long CompletedTicks
        {
            get { return CompletedAt.Ticks; }
            set { CompletedAt = value > 0 ? new DateTime(value) : default(DateTime); }
        }

        public TaskItem()
        {
            Id = Guid.NewGuid().ToString("N");
            Title = string.Empty;
            Note = string.Empty;
            CreatedAt = DateTime.Now;
            Importance = Importance.Medium;
        }

        public static TaskItem Create(string title)
        {
            TaskItem t = new TaskItem();
            t.Title = (title ?? string.Empty).Trim();
            return t;
        }

        public TaskItem Clone()
        {
            TaskItem c = new TaskItem();
            c.Id = Id;
            c.Title = Title;
            c.Note = Note;
            c.HasDue = HasDue;
            c.DueAt = DueAt;
            c.Importance = Importance;
            c.IsDone = IsDone;
            c.IsPinned = IsPinned;
            c.ManualOrder = ManualOrder;
            c.CreatedAt = CreatedAt;
            c.CompletedAt = CompletedAt;
            return c;
        }
    }
}
