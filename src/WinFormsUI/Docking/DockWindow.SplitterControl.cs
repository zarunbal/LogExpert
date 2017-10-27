using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public partial class DockWindow
    {
        private class SplitterControl : SplitterBase
        {
            #region Properties

            protected override int SplitterSize
            {
                get { return Measures.SplitterSize; }
            }

            #endregion

            #region Overrides

            protected override void StartDrag()
            {
                DockWindow window = Parent as DockWindow;
                if (window == null)
                {
                    return;
                }

                window.DockPanel.BeginDrag(window, window.RectangleToScreen(Bounds));
            }

            #endregion
        }
    }
}