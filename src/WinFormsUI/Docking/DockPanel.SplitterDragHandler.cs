using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        #region Private Fields

        private SplitterDragHandler m_splitterDragHandler;

        #endregion

        #region Private Methods

        private SplitterDragHandler GetSplitterDragHandler()
        {
            if (m_splitterDragHandler == null)
            {
                m_splitterDragHandler = new SplitterDragHandler(this);
            }

            return m_splitterDragHandler;
        }

        #endregion

        #region Nested type: SplitterDragHandler

        private sealed class SplitterDragHandler : DragHandler
        {
            #region Ctor

            public SplitterDragHandler(DockPanel dockPanel)
                : base(dockPanel)
            {
            }

            #endregion

            #region Properties / Indexers

            public new ISplitterDragSource DragSource
            {
                get => base.DragSource as ISplitterDragSource;
                private set => base.DragSource = value;
            }

            private SplitterOutline Outline { get; set; }

            private Rectangle RectSplitter { get; set; }

            #endregion

            #region Public Methods

            public void BeginDrag(ISplitterDragSource dragSource, Rectangle rectSplitter)
            {
                DragSource = dragSource;
                RectSplitter = rectSplitter;

                if (!BeginDrag())
                {
                    DragSource = null;
                    return;
                }

                Outline = new SplitterOutline();
                Outline.Show(rectSplitter);
                DragSource.BeginDrag(rectSplitter);
            }

            #endregion

            #region Overrides

            protected override void OnDragging()
            {
                Outline.Show(GetSplitterOutlineBounds(MousePosition));
            }

            protected override void OnEndDrag(bool abort)
            {
                DockPanel.SuspendLayout(true);

                Outline.Close();

                if (!abort)
                {
                    DragSource.MoveSplitter(GetMovingOffset(MousePosition));
                }

                DragSource.EndDrag();
                DockPanel.ResumeLayout(true, true);
            }

            #endregion

            #region Private Methods

            private int GetMovingOffset(Point ptMouse)
            {
                Rectangle rect = GetSplitterOutlineBounds(ptMouse);
                if (DragSource.IsVertical)
                {
                    return rect.X - RectSplitter.X;
                }

                return rect.Y - RectSplitter.Y;
            }

            private Rectangle GetSplitterOutlineBounds(Point ptMouse)
            {
                Rectangle rectLimit = DragSource.DragLimitBounds;

                Rectangle rect = RectSplitter;
                if (rectLimit.Width <= 0 || rectLimit.Height <= 0)
                {
                    return rect;
                }

                if (DragSource.IsVertical)
                {
                    rect.X += ptMouse.X - StartMousePosition.X;
                    rect.Height = rectLimit.Height;
                }
                else
                {
                    rect.Y += ptMouse.Y - StartMousePosition.Y;
                    rect.Width = rectLimit.Width;
                }

                if (rect.Left < rectLimit.Left)
                {
                    rect.X = rectLimit.X;
                }

                if (rect.Top < rectLimit.Top)
                {
                    rect.Y = rectLimit.Y;
                }

                if (rect.Right > rectLimit.Right)
                {
                    rect.X -= rect.Right - rectLimit.Right;
                }

                if (rect.Bottom > rectLimit.Bottom)
                {
                    rect.Y -= rect.Bottom - rectLimit.Bottom;
                }

                return rect;
            }

            #endregion

            #region Nested type: SplitterOutline

            private class SplitterOutline
            {
                #region Ctor

                public SplitterOutline()
                {
                    DragForm = new DragForm();
                    SetDragForm(Rectangle.Empty);
                    DragForm.BackColor = Color.Black;
                    DragForm.Opacity = 0.7;
                    DragForm.Show(false);
                }

                #endregion

                #region Properties / Indexers

                private DragForm DragForm { get; }

                #endregion

                #region Public Methods

                public void Close()
                {
                    DragForm.Close();
                }

                public void Show(Rectangle rect)
                {
                    SetDragForm(rect);
                }

                #endregion

                #region Private Methods

                private void SetDragForm(Rectangle rect)
                {
                    DragForm.Bounds = rect;
                    if (rect == Rectangle.Empty)
                    {
                        DragForm.Region = new Region(Rectangle.Empty);
                    }
                    else if (DragForm.Region != null)
                    {
                        DragForm.Region = null;
                    }
                }

                #endregion
            }

            #endregion
        }

        #endregion

        internal void BeginDrag(ISplitterDragSource dragSource, Rectangle rectSplitter)
        {
            GetSplitterDragHandler().BeginDrag(dragSource, rectSplitter);
        }
    }
}
