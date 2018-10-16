namespace WeifenLuo.WinFormsUI.Docking
{
    public partial class DockWindow
    {
        #region Nested type: SplitterControl

        private class SplitterControl : SplitterBase
        {
            #region Properties / Indexers

            protected override int SplitterSize => Measures.SplitterSize;

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

        #endregion
    }
}
