using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WeifenLuo.WinFormsUI.Docking
{
    public abstract partial class AutoHideStripBase : Control
    {
        #region Fields

        private GraphicsPath m_displayingArea = null;

        #endregion

        #region cTor

        protected AutoHideStripBase(DockPanel panel)
        {
            DockPanel = panel;
            PanesTop = new PaneCollection(panel, DockState.DockTopAutoHide);
            PanesBottom = new PaneCollection(panel, DockState.DockBottomAutoHide);
            PanesLeft = new PaneCollection(panel, DockState.DockLeftAutoHide);
            PanesRight = new PaneCollection(panel, DockState.DockRightAutoHide);

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, false);
        }

        #endregion

        #region Properties

        protected DockPanel DockPanel { get; }

        protected PaneCollection PanesTop { get; }

        protected PaneCollection PanesBottom { get; }

        protected PaneCollection PanesLeft { get; }

        protected PaneCollection PanesRight { get; }

        protected Rectangle RectangleTopLeft
        {
            get
            {
                int height = MeasureHeight();
                return PanesTop.Count > 0 && PanesLeft.Count > 0
                    ? new Rectangle(0, 0, height, height)
                    : Rectangle.Empty;
            }
        }

        protected Rectangle RectangleTopRight
        {
            get
            {
                int height = MeasureHeight();
                return PanesTop.Count > 0 && PanesRight.Count > 0
                    ? new Rectangle(Width - height, 0, height, height)
                    : Rectangle.Empty;
            }
        }

        protected Rectangle RectangleBottomLeft
        {
            get
            {
                int height = MeasureHeight();
                return PanesBottom.Count > 0 && PanesLeft.Count > 0
                    ? new Rectangle(0, Height - height, height, height)
                    : Rectangle.Empty;
            }
        }

        protected Rectangle RectangleBottomRight
        {
            get
            {
                int height = MeasureHeight();
                return PanesBottom.Count > 0 && PanesRight.Count > 0
                    ? new Rectangle(Width - height, Height - height, height, height)
                    : Rectangle.Empty;
            }
        }

        private GraphicsPath DisplayingArea
        {
            get
            {
                if (m_displayingArea == null)
                {
                    m_displayingArea = new GraphicsPath();
                }

                return m_displayingArea;
            }
        }

        #endregion

        #region Internals

        internal int GetNumberOfPanes(DockState dockState)
        {
            return GetPanes(dockState).Count;
        }

        internal void RefreshChanges()
        {
            if (IsDisposed)
            {
                return;
            }

            SetRegion();
            OnRefreshChanges();
        }

        #endregion

        #region Overrides

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            IDockContent content = HitTest();
            if (content == null)
            {
                return;
            }

            content.DockHandler.Activate();
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);

            IDockContent content = HitTest();
            if (content != null && DockPanel.ActiveAutoHideContent != content)
            {
                DockPanel.ActiveAutoHideContent = content;
            }

            // requires further tracking of mouse hover behavior,
            ResetMouseEventArgs();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            RefreshChanges();
            base.OnLayout(levent);
        }

        #endregion

        #region Private Methods

        private void SetRegion()
        {
            DisplayingArea.Reset();
            DisplayingArea.AddRectangle(RectangleTopLeft);
            DisplayingArea.AddRectangle(RectangleTopRight);
            DisplayingArea.AddRectangle(RectangleBottomLeft);
            DisplayingArea.AddRectangle(RectangleBottomRight);
            DisplayingArea.AddRectangle(GetTabStripRectangle(DockState.DockTopAutoHide));
            DisplayingArea.AddRectangle(GetTabStripRectangle(DockState.DockBottomAutoHide));
            DisplayingArea.AddRectangle(GetTabStripRectangle(DockState.DockLeftAutoHide));
            DisplayingArea.AddRectangle(GetTabStripRectangle(DockState.DockRightAutoHide));
            Region = new Region(DisplayingArea);
        }

        private IDockContent HitTest()
        {
            Point ptMouse = PointToClient(Control.MousePosition);
            return HitTest(ptMouse);
        }

        #endregion

        protected PaneCollection GetPanes(DockState dockState)
        {
            if (dockState == DockState.DockTopAutoHide)
            {
                return PanesTop;
            }
            else if (dockState == DockState.DockBottomAutoHide)
            {
                return PanesBottom;
            }
            else if (dockState == DockState.DockLeftAutoHide)
            {
                return PanesLeft;
            }
            else if (dockState == DockState.DockRightAutoHide)
            {
                return PanesRight;
            }
            else
            {
                throw new ArgumentOutOfRangeException("dockState");
            }
        }

        protected internal Rectangle GetTabStripRectangle(DockState dockState)
        {
            int height = MeasureHeight();
            if (dockState == DockState.DockTopAutoHide && PanesTop.Count > 0)
            {
                return new Rectangle(RectangleTopLeft.Width, 0,
                    Width - RectangleTopLeft.Width - RectangleTopRight.Width, height);
            }
            else if (dockState == DockState.DockBottomAutoHide && PanesBottom.Count > 0)
            {
                return new Rectangle(RectangleBottomLeft.Width, Height - height,
                    Width - RectangleBottomLeft.Width - RectangleBottomRight.Width, height);
            }
            else if (dockState == DockState.DockLeftAutoHide && PanesLeft.Count > 0)
            {
                return new Rectangle(0, RectangleTopLeft.Width, height,
                    Height - RectangleTopLeft.Height - RectangleBottomLeft.Height);
            }
            else if (dockState == DockState.DockRightAutoHide && PanesRight.Count > 0)
            {
                return new Rectangle(Width - height, RectangleTopRight.Width, height,
                    Height - RectangleTopRight.Height - RectangleBottomRight.Height);
            }
            else
            {
                return Rectangle.Empty;
            }
        }

        protected virtual void OnRefreshChanges()
        {
        }

        protected internal abstract int MeasureHeight();

        protected virtual Tab CreateTab(IDockContent content)
        {
            return new Tab(content);
        }

        protected virtual Pane CreatePane(DockPane dockPane)
        {
            return new Pane(dockPane);
        }

        protected abstract IDockContent HitTest(Point point);

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected class Tab : IDisposable
        {
            #region Fields

            #endregion

            #region cTor

            protected internal Tab(IDockContent content)
            {
                Content = content;
            }

            #endregion

            #region Properties

            public IDockContent Content { get; }

            #endregion

            #region Public methods

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            ~Tab()
            {
                Dispose(false);
            }

            protected virtual void Dispose(bool disposing)
            {
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected sealed class TabCollection : IEnumerable<Tab>
        {
            #region Fields

            #endregion

            #region cTor

            internal TabCollection(DockPane pane)
            {
                DockPane = pane;
            }

            #endregion

            #region Properties

            public DockPane DockPane { get; } = null;

            public DockPanel DockPanel
            {
                get { return DockPane.DockPanel; }
            }

            public int Count
            {
                get { return DockPane.DisplayingContents.Count; }
            }

            public Tab this[int index]
            {
                get
                {
                    IDockContent content = DockPane.DisplayingContents[index];
                    if (content == null)
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    if (content.DockHandler.AutoHideTab == null)
                    {
                        content.DockHandler.AutoHideTab = DockPanel.AutoHideStripControl.CreateTab(content);
                    }
                    return content.DockHandler.AutoHideTab as Tab;
                }
            }

            #endregion

            #region Public methods

            public bool Contains(Tab tab)
            {
                return IndexOf(tab) != -1;
            }

            public bool Contains(IDockContent content)
            {
                return IndexOf(content) != -1;
            }

            public int IndexOf(Tab tab)
            {
                if (tab == null)
                {
                    return -1;
                }

                return IndexOf(tab.Content);
            }

            public int IndexOf(IDockContent content)
            {
                return DockPane.DisplayingContents.IndexOf(content);
            }

            #endregion

            #region IEnumerable Members

            IEnumerator<Tab> IEnumerable<Tab>.GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }

            #endregion
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected class Pane : IDisposable
        {
            #region Fields

            #endregion

            #region cTor

            protected internal Pane(DockPane dockPane)
            {
                DockPane = dockPane;
            }

            #endregion

            #region Properties

            public DockPane DockPane { get; }

            public TabCollection AutoHideTabs
            {
                get
                {
                    if (DockPane.AutoHideTabs == null)
                    {
                        DockPane.AutoHideTabs = new TabCollection(DockPane);
                    }
                    return DockPane.AutoHideTabs as TabCollection;
                }
            }

            #endregion

            #region Public methods

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            ~Pane()
            {
                Dispose(false);
            }

            protected virtual void Dispose(bool disposing)
            {
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected sealed class PaneCollection : IEnumerable<Pane>
        {
            #region Fields

            #endregion

            #region cTor

            internal PaneCollection(DockPanel panel, DockState dockState)
            {
                DockPanel = panel;
                States = new AutoHideStateCollection();
                States[DockState.DockTopAutoHide].Selected = dockState == DockState.DockTopAutoHide;
                States[DockState.DockBottomAutoHide].Selected = dockState == DockState.DockBottomAutoHide;
                States[DockState.DockLeftAutoHide].Selected = dockState == DockState.DockLeftAutoHide;
                States[DockState.DockRightAutoHide].Selected = dockState == DockState.DockRightAutoHide;
            }

            #endregion

            #region Properties

            public DockPanel DockPanel { get; }

            private AutoHideStateCollection States { get; }

            public int Count
            {
                get
                {
                    int count = 0;
                    foreach (DockPane pane in DockPanel.Panes)
                    {
                        if (States.ContainsPane(pane))
                        {
                            count++;
                        }
                    }

                    return count;
                }
            }

            public Pane this[int index]
            {
                get
                {
                    int count = 0;
                    foreach (DockPane pane in DockPanel.Panes)
                    {
                        if (!States.ContainsPane(pane))
                        {
                            continue;
                        }

                        if (count == index)
                        {
                            if (pane.AutoHidePane == null)
                            {
                                pane.AutoHidePane = DockPanel.AutoHideStripControl.CreatePane(pane);
                            }
                            return pane.AutoHidePane as Pane;
                        }

                        count++;
                    }
                    throw new ArgumentOutOfRangeException("index");
                }
            }

            #endregion

            #region Public methods

            public bool Contains(Pane pane)
            {
                return IndexOf(pane) != -1;
            }

            public int IndexOf(Pane pane)
            {
                if (pane == null)
                {
                    return -1;
                }

                int index = 0;
                foreach (DockPane dockPane in DockPanel.Panes)
                {
                    if (!States.ContainsPane(pane.DockPane))
                    {
                        continue;
                    }

                    if (pane == dockPane.AutoHidePane)
                    {
                        return index;
                    }

                    index++;
                }
                return -1;
            }

            #endregion

            private class AutoHideState
            {
                #region Fields

                public readonly DockState m_dockState;
                public bool m_selected = false;

                #endregion

                #region cTor

                public AutoHideState(DockState dockState)
                {
                    m_dockState = dockState;
                }

                #endregion

                #region Properties

                public DockState DockState
                {
                    get { return m_dockState; }
                }

                public bool Selected
                {
                    get { return m_selected; }
                    set { m_selected = value; }
                }

                #endregion
            }

            private class AutoHideStateCollection
            {
                #region Fields

                private readonly AutoHideState[] m_states;

                #endregion

                #region cTor

                public AutoHideStateCollection()
                {
                    m_states = new AutoHideState[]
                    {
                        new AutoHideState(DockState.DockTopAutoHide),
                        new AutoHideState(DockState.DockBottomAutoHide),
                        new AutoHideState(DockState.DockLeftAutoHide),
                        new AutoHideState(DockState.DockRightAutoHide)
                    };
                }

                #endregion

                #region Properties

                public AutoHideState this[DockState dockState]
                {
                    get
                    {
                        for (int i = 0; i < m_states.Length; i++)
                        {
                            if (m_states[i].DockState == dockState)
                            {
                                return m_states[i];
                            }
                        }
                        throw new ArgumentOutOfRangeException("dockState");
                    }
                }

                #endregion

                #region Public methods

                public bool ContainsPane(DockPane pane)
                {
                    if (pane.IsHidden)
                    {
                        return false;
                    }

                    for (int i = 0; i < m_states.Length; i++)
                    {
                        if (m_states[i].DockState == pane.DockState && m_states[i].Selected)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                #endregion
            }

            #region IEnumerable Members

            IEnumerator<Pane> IEnumerable<Pane>.GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }

            #endregion
        }
    }
}