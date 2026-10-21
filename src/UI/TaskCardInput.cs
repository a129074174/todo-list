using System;
using System.Drawing;
using System.Windows.Forms;

namespace BossTodo.UI
{
    public partial class TaskCard
    {
        private int HitAction(Point p)
        {
            if (!Task.IsDone && _pinRect.Contains(p)) return 1;
            if (_editRect.Contains(p)) return 2;
            if (_deleteRect.Contains(p)) return 3;
            return 0;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_leftDown && !_dragStarted && !_checkRect.Contains(e.Location) && HitAction(e.Location) == 0)
            {
                if (Math.Abs(e.X - _downPoint.X) > Ui.S(5) || Math.Abs(e.Y - _downPoint.Y) > Ui.S(5))
                {
                    _dragStarted = true;
                    Cursor = Cursors.SizeNS;
                    if (DragBegin != null) DragBegin(this, EventArgs.Empty);
                }
            }

            if (_dragStarted)
            {
                if (DragMove != null) DragMove(this, EventArgs.Empty);
                return;
            }

            bool wasCheck = _checkHover;
            _checkHover = _checkRect.Contains(e.Location);
            int action = HitAction(e.Location);
            if (action != _hoverAction || wasCheck != _checkHover)
            {
                _hoverAction = action;
                Invalidate();
            }
            Cursor = (_checkHover || _hoverAction > 0) ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            _leftDown = true;
            _dragStarted = false;
            _downPoint = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;

            bool wasDrag = _dragStarted;
            _leftDown = false;
            _dragStarted = false;
            Cursor = Cursors.Default;

            if (wasDrag)
            {
                if (DragEnd != null) DragEnd(this, EventArgs.Empty);
                return;
            }

            if (_checkRect.Contains(e.Location))
            {
                if (ToggleDone != null) ToggleDone(this, EventArgs.Empty);
                return;
            }

            int action = HitAction(e.Location);
            if (action == 1)
            {
                if (PinRequested != null) PinRequested(this, EventArgs.Empty);
            }
            else if (action == 2)
            {
                if (EditRequested != null) EditRequested(this, EventArgs.Empty);
            }
            else if (action == 3)
            {
                if (DeleteRequested != null) DeleteRequested(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button == MouseButtons.Left && !_checkRect.Contains(e.Location) && HitAction(e.Location) == 0)
            {
                if (EditRequested != null) EditRequested(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _hoverAction = 0;
            _checkHover = false;
            Invalidate();
        }
    }
}
