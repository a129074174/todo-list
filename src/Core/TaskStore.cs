using System;
using System.Collections.Generic;
using System.IO;

namespace BossTodo.Core
{
    /// <summary>任务集合与持久化。</summary>
    public class TaskStore
    {
        private readonly string _tasksPath;
        private readonly string _settingsPath;

        public List<TaskItem> Tasks { get; private set; }
        public AppSettings Settings { get; private set; }
        public string DataDir { get; private set; }
        /// <summary>本次是否为首次运行（没有 settings.json）。</summary>
        public bool FirstRun { get; private set; }

        public TaskStore(string dataDir)
        {
            DataDir = dataDir;
            _tasksPath = Path.Combine(dataDir, "tasks.json");
            _settingsPath = Path.Combine(dataDir, "settings.json");
            Load();
        }

        public string TasksPath { get { return _tasksPath; } }
        public string SettingsPath { get { return _settingsPath; } }

        public void Load()
        {
            try { Directory.CreateDirectory(DataDir); } catch { }

            List<TaskItem> list = JsonStore.Load<List<TaskItem>>(_tasksPath, null);
            Tasks = list ?? new List<TaskItem>();
            bool repaired = false;
            for (int i = Tasks.Count - 1; i >= 0; i--)
            {
                if (Tasks[i] == null) { Tasks.RemoveAt(i); repaired = true; continue; }
                if (string.IsNullOrEmpty(Tasks[i].Id))
                {
                    Tasks[i].Id = Guid.NewGuid().ToString("N");
                    repaired = true;
                }
                if (Tasks[i].Title == null) { Tasks[i].Title = string.Empty; repaired = true; }
                if (Tasks[i].Note == null) { Tasks[i].Note = string.Empty; repaired = true; }
                if (Tasks[i].CreatedAt == default(DateTime))
                {
                    Tasks[i].CreatedAt = DateTime.Now;
                    repaired = true;
                }
            }
            if (repaired) SaveTasks();

            Settings = JsonStore.Load<AppSettings>(_settingsPath, null);
            if (Settings == null)
            {
                FirstRun = true;
                Settings = new AppSettings();
                SaveSettings();
            }
            Settings.Normalize();
        }

        public bool SaveTasks()
        {
            return JsonStore.Save(_tasksPath, Tasks);
        }

        public bool SaveSettings()
        {
            return JsonStore.Save(_settingsPath, Settings);
        }

        public TaskItem Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (TaskItem t in Tasks)
            {
                if (t.Id == id) return t;
            }
            return null;
        }

        public void Add(TaskItem task)
        {
            if (task == null) return;
            Tasks.Add(task);
        }

        public void Remove(string id)
        {
            for (int i = Tasks.Count - 1; i >= 0; i--)
            {
                if (Tasks[i].Id == id) Tasks.RemoveAt(i);
            }
        }

        public int PendingCount
        {
            get
            {
                int n = 0;
                foreach (TaskItem t in Tasks) { if (!t.IsDone) n++; }
                return n;
            }
        }

        public int DoneCount
        {
            get
            {
                int n = 0;
                foreach (TaskItem t in Tasks) { if (t.IsDone) n++; }
                return n;
            }
        }

        /// <summary>整组（某象限内所有未完成任务）重新编号，用于拖拽后的手动优先级。</summary>
        public void RenumberManualOrder(IEnumerable<TaskItem> ordered)
        {
            int i = 1;
            foreach (TaskItem t in ordered)
            {
                if (t == null) continue;
                t.ManualOrder = i++;
            }
        }

        public void ClearManualOrder()
        {
            foreach (TaskItem t in Tasks) t.ManualOrder = 0;
        }

        public bool HasManualOrder
        {
            get
            {
                foreach (TaskItem t in Tasks)
                {
                    if (t.ManualOrder > 0) return true;
                }
                return false;
            }
        }
    }
}
