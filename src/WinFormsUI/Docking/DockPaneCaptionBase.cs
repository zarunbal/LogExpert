using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public abstract class DockPaneCaptionBase : Control
    {
        #region Fields

        #endregion

        #region cTor

        protected internal DockPaneCaptionBase(DockPane pane)
        {
            DockPane = pane;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Selectable, false);
        }

        #endregion

        #region Properties

        protected DockPane DockPane { get; }

        protected DockPane.AppearanceStyle Appearance
        {
            get { return DockPane.Appearance; }
        }

        protected bool HasTabPageContextMenu
        {
            get { return DockPane.HasTabPageContextMenu; }
        }

        #endregion

        #region Internals

        internal void RefreshChanges()
        {
            if (IsDisposed)
            {
                return;
            }

            OnRefreshChanges();
        }

        #endregion

        #region Overrides

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right)
            {
                ShowTabPageContextMenu(new Point(e.X, e.Y));
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left &&
                DockPane.DockPanel.AllowEndUserDocking &&
                DockPane.AllowDockDragAndDrop &&
                !DockHelper.IsDockStateAutoHide(DockPane.DockState) &&
                DockPane.ActiveContent != null)
            {
                DockPane.DockPanel.BeginDrag(DockPane);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int) Win32.Msgs.WM_LBUTTONDBLCLK)
            {
                if (DockHelper.IsDockStateAutoHide(DockPane.DockState))
                {
                    DockPane.DockPanel.ActiveAutoHideContent = null;
                    return;
                }

                if (DockPane.IsFloat)
                {
                    DockPane.RestoreToPanel();
                }
                else
                {
                    DockPane.Float();
                }
            }
            base.WndProc(ref m);
        }

        #endregion

        protected void ShowTabPageContextMenu(Point position)
        {
            DockPane.ShowTabPageContextMenu(this, position);
        }

        protected virtual void OnRightToLeftLayoutChanged()
        {
        }

        protected virtual void OnRefreshChanges()
        {
        }

        protected internal abstract int MeasureHeight();
    }
}