using System;

namespace BossTodo.Core
{
    /// <summary>用户设置。首次运行会写入默认值。</summary>
    public class AppSettings
    {
        public bool AutoStart { get; set; }
        public bool SpeakOnStartup { get; set; }
        public bool SpeakOnEveryLaunch { get; set; }
        /// <summary>-10 ~ 10，0 为正常语速。</summary>
        public int SpeechRate { get; set; }
        /// <summary>0 ~ 100。</summary>
        public int SpeechVolume { get; set; }
        /// <summary>多少天内视为“紧迫”，默认 2 天。</summary>
        public int UrgentDays { get; set; }
        public bool AlwaysOnTop { get; set; }
        public bool CloseToTray { get; set; }
        public int WindowX { get; set; }
        public int WindowY { get; set; }
        public bool HasPosition { get; set; }
        /// <summary>上次播报日期，格式 yyyy-MM-dd，用于同一天不重复播报。</summary>
        public string LastSpokenDate { get; set; }
        /// <summary>指定语音名称，空表示自动选择中文语音。</summary>
        public string VoiceName { get; set; }

        public AppSettings()
        {
            AutoStart = true;
            SpeakOnStartup = true;
            SpeakOnEveryLaunch = false;
            SpeechRate = 0;
            SpeechVolume = 100;
            UrgentDays = 2;
            AlwaysOnTop = true;
            CloseToTray = true;
            HasPosition = false;
            LastSpokenDate = string.Empty;
            VoiceName = string.Empty;
        }

        public void Normalize()
        {
            if (UrgentDays < 1 || UrgentDays > 30) UrgentDays = 2;
            if (SpeechRate < -10) SpeechRate = -10;
            if (SpeechRate > 10) SpeechRate = 10;
            if (SpeechVolume < 0) SpeechVolume = 0;
            if (SpeechVolume > 100) SpeechVolume = 100;
            if (LastSpokenDate == null) LastSpokenDate = string.Empty;
            if (VoiceName == null) VoiceName = string.Empty;
        }

        /// <summary>用于备份导入时保留原对象引用。</summary>
        public void CopyFrom(AppSettings other)
        {
            if (other == null) return;
            AutoStart = other.AutoStart;
            SpeakOnStartup = other.SpeakOnStartup;
            SpeakOnEveryLaunch = other.SpeakOnEveryLaunch;
            SpeechRate = other.SpeechRate;
            SpeechVolume = other.SpeechVolume;
            UrgentDays = other.UrgentDays;
            AlwaysOnTop = other.AlwaysOnTop;
            CloseToTray = other.CloseToTray;
            WindowX = other.WindowX;
            WindowY = other.WindowY;
            HasPosition = other.HasPosition;
            LastSpokenDate = other.LastSpokenDate;
            VoiceName = other.VoiceName;
            Normalize();
        }
    }
}
