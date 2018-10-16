using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal abstract class DockOutlineBase
    {
        #region Ctor

        public DockOutlineBase()
        {
            Init();
        }

        #endregion

        #region Properties / Indexers

        public int ContentIndex { get; private set; }

        public DockStyle Dock { get; private set; }

        public Control DockTo { get; private set; }

        public bool FlagFullEdge => ContentIndex != 0;

        public bool FlagTestDrop { get; set; }

        public Rectangle FloatWindowBounds { get; private set; }

        protected int OldContentIndex { get; private set; }

        protected DockStyle OldDock { get; private set; }

        protected Control OldDockTo { get; private set; }

        protected Rectangle OldFloatWindowBounds { get; private set; }

        protected bool SameAsOldValue => FloatWindowBounds == OldFloatWindowBounds &&
                                         DockTo == OldDockTo &&
                                         Dock == OldDock &&
                                         ContentIndex == OldContentIndex;

        #endregion

        #region Public Methods

        public void Close()
        {
            OnClose();
        }

        public void Show()
        {
            SaveOldValues();
            SetValues(Rectangle.Empty, null, DockStyle.None, -1);
            TestChange();
        }

        public void Show(DockPane pane, DockStyle dock)
        {
            SaveOldValues();
            SetValues(Rectangle.Empty, pane, dock, -1);
            TestChange();
        }

        public void Show(DockPane pane, int contentIndex)
        {
            SaveOldValues();
            SetValues(Rectangle.Empty, pane, DockStyle.Fill, contentIndex);
            TestChange();
        }

        public void Show(DockPanel dockPanel, DockStyle dock, bool fullPanelEdge)
        {
            SaveOldValues();
            SetValues(Rectangle.Empty, dockPanel, dock, fullPanelEdge ? -1 : 0);
            TestChange();
        }

        public void Show(Rectangle floatWindowBounds)
        {
            SaveOldValues();
            SetValues(floatWindowBounds, null, DockStyle.None, -1);
            TestChange();
        }

        #endregion

        #region Event handling Methods

        protected abstract void OnClose();

        protected abstract void OnShow();

        #endregion

        #region Private Methods

        private void Init()
        {
            SetValues(Rectangle.Empty, null, DockStyle.None, -1);
            SaveOldValues();
        }

        private void SaveOldValues()
        {
            OldDockTo = DockTo;
            OldDock = Dock;
            OldContentIndex = ContentIndex;
            OldFloatWindowBounds = FloatWindowBounds;
        }

        private void SetValues(Rectangle floatWindowBounds, Control dockTo, DockStyle dock, int contentIndex)
        {
            FloatWindowBounds = floatWindowBounds;
            DockTo = dockTo;
            Dock = dock;
            ContentIndex = contentIndex;
            FlagTestDrop = true;
        }

        private void TestChange()
        {
            if (FloatWindowBounds != OldFloatWindowBounds ||
                DockTo != OldDockTo ||
                Dock != OldDock ||
                ContentIndex != OldContentIndex)
            {
                OnShow();
            }
        }

        #endregion
    }
}
