using System;
using System.Drawing;
using System.Windows.Forms;
using BossTodo.Core;

namespace BossTodo.UI
{
    /// <summary>新建 / 编辑任务弹窗。</summary>
    public partial class EditTaskDialog : DialogBase
    {
        private readonly TaskItem _source;
        private readonly bool _isNew;

        private TextBox _titleBox;
        private TextBox _noteBox;
        private ToggleSwitch _dueToggle;
        private DateTimePicker _datePicker;
        private DateTimePicker _timePicker;
        private SegmentedControl _importance;
        private Label _error;
        private PillButton _todayChip;
        private PillButton _tomorrowChip;
        private PillButton _afterChip;
        private PillButton _nextWeekChip;

        public TaskItem ResultTask { get; private set; }
        public bool DeleteRequested { get; private set; }

        private static readonly Importance[] ImportanceMap =
        {
            Importance.High, Importance.Medium, Importance.Low
        };

        public EditTaskDialog(TaskItem task)
        {
            _source = task;
            _isNew = task == null;
            ResultTask = _isNew ? TaskItem.Create(string.Empty) : task.Clone();

            Text = _isNew ? "新建任务" : "编辑任务";
            ClientSize = new Size(Ui.S(360), Ui.S(506));
            BackColor = Theme.Bg;

            BuildTop();
            BuildSchedule();
            BuildFooter();
            LoadValues();
            CreateCloseButton();
        }

        protected override string DialogTitleText
        {
            get { return _isNew ? "新建任务" : "编辑任务"; }
        }

        protected override string DialogSubtitleText
        {
            get { return "截止时间与重要程度会自动决定它所在的象限"; }
        }

        private Label MakeLabel(string text, int x, int y, int width)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = false;
            label.Bounds = new Rectangle(x, y, width, Ui.S(18));
            label.Font = Theme.Small;
            label.ForeColor = Theme.TextSub;
            label.BackColor = Theme.Bg;
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private static void SetCue(TextBox box, string text)
        {
            try { NativeMethods.SetCueBanner(box.Handle, text); }
            catch { }
        }

        private void BuildTop()
        {
            Controls.Add(MakeLabel("任务内容", Ui.S(24), Ui.S(66), Ui.S(200)));

            RoundedPanel titlePanel = new RoundedPanel();
            titlePanel.Bounds = new Rectangle(Ui.S(24), Ui.S(86), Ui.S(312), Ui.S(46));
            titlePanel.Radius = 10;
            _titleBox = new TextBox();
            _titleBox.BorderStyle = BorderStyle.None;
            _titleBox.Font = Theme.F(10f, FontStyle.Regular);
            _titleBox.ForeColor = Theme.TextMain;
            _titleBox.BackColor = Color.White;
            _titleBox.Bounds = new Rectangle(Ui.S(14), Ui.S(12), Ui.S(284), Ui.S(22));
            _titleBox.TextChanged += delegate
            {
                if (_error.Visible && _titleBox.Text.Trim().Length > 0) _error.Visible = false;
            };
            titlePanel.Controls.Add(_titleBox);
            Controls.Add(titlePanel);

            _error = MakeLabel("请输入任务内容", Ui.S(24), Ui.S(134), Ui.S(220));
            _error.ForeColor = Theme.Danger;
            _error.Font = Theme.Tiny;
            _error.Visible = false;
            Controls.Add(_error);

            Controls.Add(MakeLabel("备注", Ui.S(24), Ui.S(154), Ui.S(200)));
            RoundedPanel notePanel = new RoundedPanel();
            notePanel.Bounds = new Rectangle(Ui.S(24), Ui.S(174), Ui.S(312), Ui.S(64));
            notePanel.Radius = 10;
            _noteBox = new TextBox();
            _noteBox.BorderStyle = BorderStyle.None;
            _noteBox.Multiline = true;
            _noteBox.AcceptsReturn = true;
            _noteBox.WordWrap = true;
            _noteBox.ScrollBars = ScrollBars.None;
            _noteBox.Font = Theme.F(9.2f, FontStyle.Regular);
            _noteBox.ForeColor = Theme.TextMain;
            _noteBox.BackColor = Color.White;
            _noteBox.Bounds = new Rectangle(Ui.S(14), Ui.S(10), Ui.S(282), Ui.S(44));
            notePanel.Controls.Add(_noteBox);
            Controls.Add(notePanel);
        }
    }
}
