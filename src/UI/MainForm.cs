using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    /// <summary>主便签窗口（无边框、圆角、四象限卡片列表）。</summary>
    public partial class MainForm : Form
    {
        private readonly TaskStore _store;
        private readonly SpeechService _speech;
        private readonly bool _isAutostart;
        private readonly bool _demo;
        private readonly Dictionary<string, TaskCard> _cards = new Dictionary<string, TaskCard>();
        private readonly Dictionary<Quadrant, GroupHeader> _headers = new Dictionary<Quadrant, GroupHeader>();
        private readonly List<PillButton> _chips = new List<PillButton>();

        private DateTime _now;
        private string _filter = "all";
        private bool _showDone;
        private int _scroll;
        private bool _reallyExit;
        private bool _trayHintShown;
        private bool _layoutReady;

        private TextBox _input;
        private RoundedPanel _quickPanel;
        private IconButton _pinButton;
        private IconButton _minButton;
        private IconButton _closeButton;
        private IconButton _addButton;
        private IconButton _settingsButton;
        private PillButton _speakButton;
        private PillButton _autoSortButton;
        private ListHost _list;
        private ThinScrollbar _bar;
        private ToastView _toast;
        private SectionToggle _doneToggle;
        private Control _emptyState;
        private Timer _refreshTimer;
        private NotifyIcon _tray;
        private ContextMenuStrip _trayMenu;
        private ToolTip _toolTip;

        private TaskCard _dragCard;
        private bool _dragActive;
        private int _dragStartScreenY;
        private MouseWheelRouter _wheelFilter;

        private AppSettings Settings
        {
            get { return _store.Settings; }
        }

        public MainForm(string dataDir, bool isAutostart, bool demo)
        {
            _isAutostart = isAutostart;
            _demo = demo;
            _store = new TaskStore(dataDir);
            _speech = new SpeechService();
            _now = DateTime.Now;

            Text = "Boss 待办";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ClientSize = new Size(Ui.S(380), Ui.S(650));
            BackColor = Theme.Bg;
            DoubleBuffered = true;
            KeyPreview = true;
            AutoScaleMode = AutoScaleMode.None;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            TopMost = Settings.AlwaysOnTop;
            try { Icon = IconFactory.Create(32); }
            catch { }

            BuildChrome();
            ApplyWindowPosition();
            RebuildList(false);
            UpdateAutoSortVisibility();
            WireEvents();
            SetupTray();

            if (!_demo && Settings.AutoStart) AutoStart.SetEnabled(true);

            _refreshTimer = new Timer();
            _refreshTimer.Interval = 30000;
            _refreshTimer.Tick += delegate
            {
                _now = DateTime.Now;
                if (_dragActive)
                {
                    foreach (TaskCard card in _cards.Values) card.UpdateTime(_now, Settings.UrgentDays);
                }
                else
                {
                    RebuildList(true);
                }
                Invalidate();
            };
            _refreshTimer.Start();

            Shown += delegate
            {
                if (_demo) return;
                if (_store.FirstRun) ShowToastLater("已开启开机语音播报，可在设置中调整", 900);
            };
        }
    }
}
