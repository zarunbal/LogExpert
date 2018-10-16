using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking.Win32;

namespace WeifenLuo.WinFormsUI.Docking
{
    public abstract class DockPaneCaptionBase : Control
    {
        #region Ctor

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

        #region Properties / Indexers

        protected DockPane.AppearanceStyle Appearance => DockPane.Appearance;

        protected DockPane DockPane { get; }

        protected bool HasTabPageContextMenu => DockPane.HasTabPageContextMenu;

        #endregion

        #region Overrides

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

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right)
            {
                ShowTabPageContextMenu(new Point(e.X, e.Y));
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)Msgs.WM_LBUTTONDBLCLK)
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

        #region Event handling Methods

        protected virtual void OnRefreshChanges()
        {
        }

        protected virtual void OnRightToLeftLayoutChanged()
        {
        }

        #endregion

        #region Private Methods

        protected void ShowTabPageContextMenu(Point position)
        {
            DockPane.ShowTabPageContextMenu(this, position);
        }

        #endregion

        internal void RefreshChanges()
        {
            if (IsDisposed)
            {
                return;
            }

            OnRefreshChanges();
        }

        protected internal abstract int MeasureHeight();
    }
}
