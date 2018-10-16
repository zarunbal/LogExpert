using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking.Win32;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class SplitterBase : Control
    {
        #region Ctor

        public SplitterBase()
        {
            SetStyle(ControlStyles.Selectable, false);
        }

        #endregion

        #region Properties / Indexers

        public override DockStyle Dock
        {
            get => base.Dock;
            set
            {
                SuspendLayout();
                base.Dock = value;

                if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                {
                    Width = SplitterSize;
                }
                else if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
                {
                    Height = SplitterSize;
                }
                else
                {
                    Bounds = Rectangle.Empty;
                }

                if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                {
                    Cursor = Cursors.VSplit;
                }
                else if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
                {
                    Cursor = Cursors.HSplit;
                }
                else
                {
                    Cursor = Cursors.Default;
                }

                ResumeLayout();
            }
        }

        protected virtual int SplitterSize => 0;

        #endregion

        #region Overrides

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            StartDrag();
        }

        protected override void WndProc(ref Message m)
        {
            // eat the WM_MOUSEACTIVATE message
            if (m.Msg == (int)Msgs.WM_MOUSEACTIVATE)
            {
                return;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Private Methods

        protected virtual void StartDrag()
        {
        }

        #endregion
    }
}
