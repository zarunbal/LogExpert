using System;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class DummyControl : Control
    {
        #region cTor

        public DummyControl()
        {
            SetStyle(ControlStyles.Selectable, false);
        }

        #endregion
    }
}