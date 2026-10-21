using System;
using System.Drawing;
using System.Windows.Forms;

namespace BossTodo.UI
{
    /// <summary>底部浮层提示条。</summary>
    public class ToastView : SoftControl
    {
        private readonly Timer _timer;
        private string _message;

        public ToastView()
        {
            _message = string.Empty;
            BackColor = Theme.Bg;
            Visible = false;
            Height = Ui.S(38);
            _timer = new Timer();
            _timer.Interval = 2600;
            _timer.Tick += delegate
            {
                _timer.Stop();
                Visible = false;
            };
        }

        public void ShowMessage(string message)
        {
            _message = message ?? string.Empty;
            Visible = true;
            BringToFront();
            _timer.Stop();
            _timer.Start();
            Invalidate();
        }

        public void HideNow()
        {
            _timer.Stop();
            Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Ui.Quality(g);
            g.Clear(BackColor);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Ui.FillRound(g, Theme.TextMain, rect, Height / 2);
            using (SolidBrush brush = new SolidBrush(Theme.Accent))
            {
                g.FillEllipse(brush, Ui.S(14), Height / 2 - Ui.S(2), Ui.S(4), Ui.S(4));
            }
            Ui.TextLeft(g, _message, Theme.Small, Color.White,
                new Rectangle(Ui.S(24), 0, Width - Ui.S(34), Height));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>右键菜单配色。</summary>
    public class SoftColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected { get { return Theme.AccentSoft; } }
        public override Color MenuItemSelectedGradientBegin { get { return Theme.AccentSoft; } }
        public override Color MenuItemSelectedGradientEnd { get { return Theme.AccentSoft; } }
        public override Color MenuItemBorder { get { return Theme.AccentSoft; } }
        public override Color MenuBorder { get { return Theme.Border; } }
        public override Color ToolStripDropDownBackground { get { return Color.White; } }
        public override Color ImageMarginGradientBegin { get { return Color.White; } }
        public override Color ImageMarginGradientMiddle { get { return Color.White; } }
        public override Color ImageMarginGradientEnd { get { return Color.White; } }
        public override Color SeparatorDark { get { return Theme.Border; } }
        public override Color SeparatorLight { get { return Color.White; } }
    }

    public class SoftMenuRenderer : ToolStripProfessionalRenderer
    {
        public SoftMenuRenderer() : base(new SoftColorTable())
        {
            RoundedEdges = true;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? Theme.TextMain : Theme.TextFaint;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Theme.TextSub;
            base.OnRenderArrow(e);
        }
    }

    public static class MenuFactory
    {
        public static ContextMenuStrip Create()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Renderer = new SoftMenuRenderer();
            menu.BackColor = Color.White;
            menu.ForeColor = Theme.TextMain;
            menu.Font = Theme.F(9.4f, FontStyle.Regular);
            menu.ShowImageMargin = false;
            menu.Padding = new Padding(Ui.S(4), Ui.S(6), Ui.S(4), Ui.S(6));
            return menu;
        }
    }
}
