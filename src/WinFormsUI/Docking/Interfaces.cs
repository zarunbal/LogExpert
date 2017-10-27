using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public interface IDockContent
    {
        #region Properties

        DockContentHandler DockHandler { get; }

        #endregion

        #region Public methods

        void OnActivated(EventArgs e);
        void OnDeactivate(EventArgs e);

        #endregion
    }

    public interface INestedPanesContainer
    {
        #region Properties

        DockState DockState { get; }
        Rectangle DisplayingRectangle { get; }
        NestedPaneCollection NestedPanes { get; }
        VisibleNestedPaneCollection VisibleNestedPanes { get; }
        bool IsFloat { get; }

        #endregion
    }

    internal interface IDragSource
    {
        #region Properties

        Control DragControl { get; }

        #endregion
    }

    internal interface IDockDragSource : IDragSource
    {
        #region Public methods

        Rectangle BeginDrag(Point ptMouse);
        bool IsDockStateValid(DockState dockState);
        bool CanDockTo(DockPane pane);
        void FloatAt(Rectangle floatWindowBounds);
        void DockTo(DockPane pane, DockStyle dockStyle, int contentIndex);
        void DockTo(DockPanel panel, DockStyle dockStyle);

        #endregion
    }

    internal interface ISplitterDragSource : IDragSource
    {
        #region Properties

        bool IsVertical { get; }
        Rectangle DragLimitBounds { get; }

        #endregion

        #region Public methods

        void BeginDrag(Rectangle rectSplitter);
        void EndDrag();
        void MoveSplitter(int offset);

        #endregion
    }
}