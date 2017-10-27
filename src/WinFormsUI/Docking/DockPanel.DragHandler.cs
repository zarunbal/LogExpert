using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        /// <summary>
        /// DragHandlerBase is the base class for drag handlers. The derived class should:
        ///   1. Define its public method BeginDrag. From within this public BeginDrag method,
        ///      DragHandlerBase.BeginDrag should be called to initialize the mouse capture
        ///      and message filtering.
        ///   2. Override the OnDragging and OnEndDrag methods.
        /// </summary>
        private abstract class DragHandlerBase : NativeWindow, IMessageFilter
        {
            #region Fields

            #endregion

            #region cTor

            protected DragHandlerBase()
            {
            }

            #endregion

            #region Properties

            protected abstract Control DragControl { get; }

            protected Point StartMousePosition { get; private set; } = Point.Empty;

            #endregion

            #region Overrides

            protected sealed override void WndProc(ref Message m)
            {
                if (m.Msg == (int) Win32.Msgs.WM_CANCELMODE || m.Msg == (int) Win32.Msgs.WM_CAPTURECHANGED)
                {
                    EndDrag(true);
                }

                base.WndProc(ref m);
            }

            #endregion

            #region Private Methods

            private void EndDrag(bool abort)
            {
                ReleaseHandle();
                Application.RemoveMessageFilter(this);
                DragControl.FindForm().Capture = false;

                OnEndDrag(abort);
            }

            bool IMessageFilter.PreFilterMessage(ref Message m)
            {
                if (m.Msg == (int) Win32.Msgs.WM_MOUSEMOVE)
                {
                    OnDragging();
                }
                else if (m.Msg == (int) Win32.Msgs.WM_LBUTTONUP)
                {
                    EndDrag(false);
                }
                else if (m.Msg == (int) Win32.Msgs.WM_CAPTURECHANGED)
                {
                    EndDrag(true);
                }
                else if (m.Msg == (int) Win32.Msgs.WM_KEYDOWN && (int) m.WParam == (int) Keys.Escape)
                {
                    EndDrag(true);
                }

                return OnPreFilterMessage(ref m);
            }

            #endregion

            protected bool BeginDrag()
            {
                // Avoid re-entrance;
                lock (this)
                {
                    if (DragControl == null)
                    {
                        return false;
                    }

                    StartMousePosition = Control.MousePosition;

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

            protected abstract void OnDragging();

            protected abstract void OnEndDrag(bool abort);

            protected virtual bool OnPreFilterMessage(ref Message m)
            {
                return false;
            }
        }

        private abstract class DragHandler : DragHandlerBase
        {
            #region Fields

            #endregion

            #region cTor

            protected DragHandler(DockPanel dockPanel)
            {
                DockPanel = dockPanel;
            }

            #endregion

            #region Properties

            public DockPanel DockPanel { get; }

            protected IDragSource DragSource { get; set; }

            protected sealed override Control DragControl
            {
                get { return DragSource == null ? null : DragSource.DragControl; }
            }

            #endregion

            #region Overrides

            protected sealed override bool OnPreFilterMessage(ref Message m)
            {
                if ((m.Msg == (int) Win32.Msgs.WM_KEYDOWN || m.Msg == (int) Win32.Msgs.WM_KEYUP) &&
                    ((int) m.WParam == (int) Keys.ControlKey || (int) m.WParam == (int) Keys.ShiftKey))
                {
                    OnDragging();
                }

                return base.OnPreFilterMessage(ref m);
            }

            #endregion
        }
    }
}