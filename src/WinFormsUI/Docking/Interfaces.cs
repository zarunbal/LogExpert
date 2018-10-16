using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public interface IDockContent
    {
        #region Properties / Indexers

        DockContentHandler DockHandler { get; }

        #endregion

        #region Public Methods

        void OnActivated(EventArgs e);
        void OnDeactivate(EventArgs e);

        #endregion
    }

    public interface INestedPanesContainer
    {
        #region Properties / Indexers

        Rectangle DisplayingRectangle { get; }

        DockState DockState { get; }
        bool IsFloat { get; }
        NestedPaneCollection NestedPanes { get; }
        VisibleNestedPaneCollection VisibleNestedPanes { get; }

        #endregion
    }

    internal interface IDragSource
    {
        #region Properties / Indexers

        Control DragControl { get; }

        #endregion
    }

    internal interface IDockDragSource : IDragSource
    {
        #region Public Methods

        Rectangle BeginDrag(Point ptMouse);
        bool CanDockTo(DockPane pane);
        void DockTo(DockPane pane, DockStyle dockStyle, int contentIndex);
        void DockTo(DockPanel panel, DockStyle dockStyle);
        void FloatAt(Rectangle floatWindowBounds);
        bool IsDockStateValid(DockState dockState);

        #endregion
    }

    internal interface ISplitterDragSource : IDragSource
    {
        #region Properties / Indexers

        Rectangle DragLimitBounds { get; }

        bool IsVertical { get; }

        #endregion

        #region Public Methods

        void BeginDrag(Rectangle rectSplitter);
        void EndDrag();
        void MoveSplitter(int offset);

        #endregion
    }
}
