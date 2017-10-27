using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        #region Fields

        private SplitterDragHandler m_splitterDragHandler = null;

        #endregion

        #region Internals

        internal void BeginDrag(ISplitterDragSource dragSource, Rectangle rectSplitter)
        {
            GetSplitterDragHandler().BeginDrag(dragSource, rectSplitter);
        }

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

        private sealed class SplitterDragHandler : DragHandler
        {
            #region Fields

            #endregion

            #region cTor

            public SplitterDragHandler(DockPanel dockPanel)
                : base(dockPanel)
            {
            }

            #endregion

            #region Properties

            public new ISplitterDragSource DragSource
            {
                get { return base.DragSource as ISplitterDragSource; }
                private set { base.DragSource = value; }
            }

            private SplitterOutline Outline { get; set; }

            private Rectangle RectSplitter { get; set; }

            #endregion

            #region Public methods

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
                Outline.Show(GetSplitterOutlineBounds(Control.MousePosition));
            }

            protected override void OnEndDrag(bool abort)
            {
                DockPanel.SuspendLayout(true);

                Outline.Close();

                if (!abort)
                {
                    DragSource.MoveSplitter(GetMovingOffset(Control.MousePosition));
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
                else
                {
                    return rect.Y - RectSplitter.Y;
                }
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

            private class SplitterOutline
            {
                #region Fields

                #endregion

                #region cTor

                public SplitterOutline()
                {
                    DragForm = new DragForm();
                    SetDragForm(Rectangle.Empty);
                    DragForm.BackColor = Color.Black;
                    DragForm.Opacity = 0.7;
                    DragForm.Show(false);
                }

                #endregion

                #region Properties

                private DragForm DragForm { get; }

                #endregion

                #region Public methods

                public void Show(Rectangle rect)
                {
                    SetDragForm(rect);
                }

                public void Close()
                {
                    DragForm.Close();
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
        }
    }
}