using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class DummyControl : Control
    {
        #region Ctor

        public DummyControl()
        {
            SetStyle(ControlStyles.Selectable, false);
        }

        #endregion
    }
}
