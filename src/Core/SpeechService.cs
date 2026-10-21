using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Speech.Synthesis;

namespace BossTodo.Core
{
    /// <summary>System.Speech 语音播报（Windows 自带 SAPI）。</summary>
    public class SpeechService : IDisposable
    {
        private readonly object _gate = new object();
        private SpeechSynthesizer _current;
        private bool _disposed;

        public static string ChineseVoiceName { get; private set; }
        public static bool HasChineseVoice { get; private set; }
        public static List<string> InstalledVoices { get; private set; }

        static SpeechService()
        {
            ChineseVoiceName = string.Empty;
            InstalledVoices = new List<string>();
            try
            {
                using (SpeechSynthesizer probe = new SpeechSynthesizer())
                {
                    foreach (InstalledVoice voice in probe.GetInstalledVoices())
                    {
                        if (!voice.Enabled) continue;
                        InstalledVoices.Add(voice.VoiceInfo.Name + " (" + voice.VoiceInfo.Culture.Name + ")");
                        string cul = voice.VoiceInfo.Culture.Name;
                        if (ChineseVoiceName.Length == 0 &&
                            cul.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                        {
                            ChineseVoiceName = voice.VoiceInfo.Name;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppPaths.LogError("语音引擎检测失败：" + ex.Message);
            }
            HasChineseVoice = ChineseVoiceName.Length > 0;
        }

        public bool IsSpeaking
        {
            get
            {
                SpeechSynthesizer s = _current;
                try { return s != null && s.State == SynthesizerState.Speaking; }
                catch { return false; }
            }
        }

        public void Speak(string text, int rate, int volume, string preferredVoice)
        {
            if (string.IsNullOrEmpty(text)) return;

            lock (_gate)
            {
                if (_disposed) return;
                Stop();

                SpeechSynthesizer synth = new SpeechSynthesizer();
                _current = synth;
                try
                {
                    SelectVoice(synth, preferredVoice);
                    synth.Rate = Math.Max(-10, Math.Min(10, rate));
                    synth.Volume = Math.Max(0, Math.Min(100, volume));
                    synth.SpeakCompleted += delegate(object sender, SpeakCompletedEventArgs e)
                    {
                        SpeechSynthesizer finished = sender as SpeechSynthesizer;
                        if (finished == null) return;
                        lock (_gate)
                        {
                            if (_current == finished) _current = null;
                        }
                        try { finished.Dispose(); }
                        catch { }
                    };
                    synth.SpeakAsync(text);
                }
                catch (Exception ex)
                {
                    AppPaths.LogError("语音播报失败：" + ex.Message);
                    if (_current == synth) _current = null;
                    try { synth.Dispose(); }
                    catch { }
                }
            }
        }

        private static void SelectVoice(SpeechSynthesizer synth, string preferredVoice)
        {
            if (!string.IsNullOrEmpty(preferredVoice))
            {
                try
                {
                    synth.SelectVoice(preferredVoice);
                    return;
                }
                catch { }
            }
            if (HasChineseVoice)
            {
                try { synth.SelectVoice(ChineseVoiceName); }
                catch { }
            }
        }

        public void Stop()
        {
            SpeechSynthesizer s = _current;
            if (s == null) return;
            try { s.SpeakAsyncCancelAll(); }
            catch { }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                _disposed = true;
                Stop();
                _current = null;
            }
        }

        public static string VoiceStatusText()
        {
            if (HasChineseVoice) return "中文语音：" + ChineseVoiceName;
            if (InstalledVoices.Count > 0) return "未找到中文语音，将使用系统默认语音";
            return "系统未安装语音引擎，请先在 Windows 设置中添加语音";
        }

        public static string DateKey(DateTime time)
        {
            return time.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>把示例语音渲染为 WAV，用于离线验证语音引擎（不播放声音）。</summary>
        public static bool RenderToWaveFile(string text, string path)
        {
            try
            {
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    SelectVoice(synth, string.Empty);
                    synth.Rate = 0;
                    synth.Volume = 100;
                    synth.SetOutputToWaveFile(path);
                    synth.Speak(text ?? string.Empty);
                    synth.SetOutputToNull();
                }
                return File.Exists(path) && new FileInfo(path).Length > 1000;
            }
            catch (Exception ex)
            {
                AppPaths.LogError("语音离线渲染失败：" + ex.Message);
                return false;
            }
        }
    }
}
