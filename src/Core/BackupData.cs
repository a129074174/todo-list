using System;
using System.Collections.Generic;

namespace BossTodo.Core
{
    /// <summary>导出 / 导入用的备份容器。</summary>
    public class BackupData
    {
        public string App { get; set; }
        public DateTime ExportedAt { get; set; }
        public List<TaskItem> Tasks { get; set; }
        public AppSettings Settings { get; set; }

        public BackupData()
        {
            App = "BossTodo";
            ExportedAt = DateTime.Now;
            Tasks = new List<TaskItem>();
            Settings = new AppSettings();
        }
    }
}
