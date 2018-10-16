using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking.Win32;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        #region Nested type: DragHandler

        private abstract class DragHandler : DragHandlerBase
        {
            #region Ctor

            protected DragHandler(DockPanel dockPanel)
            {
                DockPanel = dockPanel;
            }

            #endregion

            #region Properties / Indexers

            public DockPanel DockPanel { get; }

            protected sealed override Control DragControl => DragSource == null ? null : DragSource.DragControl;

            protected IDragSource DragSource { get; set; }

            #endregion

            #region Overrides

            protected sealed override bool OnPreFilterMessage(ref Message m)
            {
                if ((m.Msg == (int)Msgs.WM_KEYDOWN || m.Msg == (int)Msgs.WM_KEYUP) &&
                    ((int)m.WParam == (int)Keys.ControlKey || (int)m.WParam == (int)Keys.ShiftKey))
                {
                    OnDragging();
                }

                return base.OnPreFilterMessage(ref m);
            }

            #endregion
        }

        #endregion

        #region Nested type: DragHandlerBase

        /// <summary>
        ///     DragHandlerBase is the base class for drag handlers. The derived class should:
        ///     1. Define its public method BeginDrag. From within this public BeginDrag method,
        ///     DragHandlerBase.BeginDrag should be called to initialize the mouse capture
        ///     and message filtering.
        ///     2. Override the OnDragging and OnEndDrag methods.
        /// </summary>
        private abstract class DragHandlerBase : NativeWindow, IMessageFilter
        {
            #region Interface IMessageFilter

            bool IMessageFilter.PreFilterMessage(ref Message m)
            {
                if (m.Msg == (int)Msgs.WM_MOUSEMOVE)
                {
                    OnDragging();
                }
                else if (m.Msg == (int)Msgs.WM_LBUTTONUP)
                {
                    EndDrag(false);
                }
                else if (m.Msg == (int)Msgs.WM_CAPTURECHANGED)
                {
                    EndDrag(true);
                }
                else if (m.Msg == (int)Msgs.WM_KEYDOWN && (int)m.WParam == (int)Keys.Escape)
                {
                    EndDrag(true);
                }

                return OnPreFilterMessage(ref m);
            }

            #endregion

            #region Properties / Indexers

            protected abstract Control DragControl { get; }

            protected Point StartMousePosition { get; private set; } = Point.Empty;

            #endregion

            #region Overrides

            protected sealed override void WndProc(ref Message m)
            {
                if (m.Msg == (int)Msgs.WM_CANCELMODE || m.Msg == (int)Msgs.WM_CAPTURECHANGED)
                {
                    EndDrag(true);
                }

                base.WndProc(ref m);
            }

            #endregion

            #region Event handling Methods

            protected abstract void OnDragging();

            protected abstract void OnEndDrag(bool abort);

            protected virtual bool OnPreFilterMessage(ref Message m)
            {
                return false;
            }

            #endregion

            #region Private Methods

            protected bool BeginDrag()
            {
                // Avoid re-entrance;
                lock (this)
                {
                    if (DragControl == null)
                    {
                        return false;
                    }

                    StartMousePosition = MousePosition;

                    if (!NativeMethods.DragDetect(DragControl.Handle, StartMousePosition))
                    {
                        return false;
                    }

                    DragControl.FindForm().Capture = true;
                    AssignHandle(DragControl.FindForm().Handle);
                    Application.AddMessageFilter(this);
                    return true;
                }
            }

            private void EndDrag(bool abort)
            {
                ReleaseHandle();
                Application.RemoveMessageFilter(this);
                DragControl.FindForm().Capture = false;

                OnEndDrag(abort);
            }

            #endregion
        }

        #endregion
    }
}
