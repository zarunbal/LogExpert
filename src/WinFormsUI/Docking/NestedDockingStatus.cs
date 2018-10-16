using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking
{
    public sealed class NestedDockingStatus
    {
        #region Ctor

        internal NestedDockingStatus(DockPane pane)
        {
            DockPane = pane;
        }

        #endregion

        #region Properties / Indexers

        public DockAlignment Alignment { get; private set; } = DockAlignment.Left;

        public DockAlignment DisplayingAlignment { get; private set; } = DockAlignment.Left;

        public DockPane DisplayingPreviousPane { get; private set; }

        public double DisplayingProportion { get; private set; } = 0.5;

        public DockPane DockPane { get; }

        public bool IsDisplaying { get; private set; }

        public Rectangle LogicalBounds { get; private set; } = Rectangle.Empty;

        public NestedPaneCollection NestedPanes { get; private set; }

        public Rectangle PaneBounds { get; private set; } = Rectangle.Empty;

        public DockPane PreviousPane { get; private set; }

        public double Proportion { get; private set; } = 0.5;

        public Rectangle SplitterBounds { get; private set; } = Rectangle.Empty;

        #endregion

        internal void SetStatus(NestedPaneCollection nestedPanes, DockPane previousPane, DockAlignment alignment,
                                double proportion)
        {
            NestedPanes = nestedPanes;
            PreviousPane = previousPane;
            Alignment = alignment;
            Proportion = proportion;
        }

        internal void SetDisplayingStatus(bool isDisplaying, DockPane displayingPreviousPane,
                                          DockAlignment displayingAlignment, double displayingProportion)
        {
            IsDisplaying = isDisplaying;
            DisplayingPreviousPane = displayingPreviousPane;
            DisplayingAlignment = displayingAlignment;
            DisplayingProportion = displayingProportion;
        }

        internal void SetDisplayingBounds(Rectangle logicalBounds, Rectangle paneBounds, Rectangle splitterBounds)
        {
            LogicalBounds = logicalBounds;
            PaneBounds = paneBounds;
            SplitterBounds = splitterBounds;
        }
    }
}
