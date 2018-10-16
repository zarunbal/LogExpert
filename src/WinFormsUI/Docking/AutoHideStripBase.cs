using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public abstract class AutoHideStripBase : Control
    {
        #region Private Fields

        private GraphicsPath m_displayingArea;

        #endregion

        #region Ctor

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

        #region Properties / Indexers

        protected DockPanel DockPanel { get; }

        protected PaneCollection PanesBottom { get; }

        protected PaneCollection PanesLeft { get; }

        protected PaneCollection PanesRight { get; }

        protected PaneCollection PanesTop { get; }

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

        #region Overrides

        protected override void OnLayout(LayoutEventArgs levent)
        {
            RefreshChanges();
            base.OnLayout(levent);
        }

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

        #endregion

        #region Event handling Methods

        protected virtual void OnRefreshChanges()
        {
        }

        #endregion

        #region Private Methods

        protected virtual Pane CreatePane(DockPane dockPane)
        {
            return new Pane(dockPane);
        }

        protected virtual Tab CreateTab(IDockContent content)
        {
            return new Tab(content);
        }

        protected PaneCollection GetPanes(DockState dockState)
        {
            if (dockState == DockState.DockTopAutoHide)
            {
                return PanesTop;
            }

            if (dockState == DockState.DockBottomAutoHide)
            {
                return PanesBottom;
            }

            if (dockState == DockState.DockLeftAutoHide)
            {
                return PanesLeft;
            }

            if (dockState == DockState.DockRightAutoHide)
            {
                return PanesRight;
            }

            throw new ArgumentOutOfRangeException("dockState");
        }

        private IDockContent HitTest()
        {
            Point ptMouse = PointToClient(MousePosition);
            return HitTest(ptMouse);
        }

        protected abstract IDockContent HitTest(Point point);

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

        #endregion

        #region Nested type: Pane

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected class Pane : IDisposable
        {
            #region Ctor

            protected internal Pane(DockPane dockPane)
            {
                DockPane = dockPane;
            }

            #endregion

            #region Interface IDisposable

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Properties / Indexers

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

            public DockPane DockPane { get; }

            #endregion

            #region Private Methods

            protected virtual void Dispose(bool disposing)
            {
            }

            #endregion

            ~Pane()
            {
                Dispose(false);
            }
        }

        #endregion

        #region Nested type: PaneCollection

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected sealed class PaneCollection : IEnumerable<Pane>
        {
            #region Ctor

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

            #region Interface IEnumerable<Pane>

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

            #region Properties / Indexers

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

            public DockPanel DockPanel { get; }

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

            private AutoHideStateCollection States { get; }

            #endregion

            #region Public Methods

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

            #region Nested type: AutoHideState

            private class AutoHideState
            {
                #region Private Fields

                public readonly DockState m_dockState;
                public bool m_selected;

                #endregion

                #region Ctor

                public AutoHideState(DockState dockState)
                {
                    m_dockState = dockState;
                }

                #endregion

                #region Properties / Indexers

                public DockState DockState => m_dockState;

                public bool Selected
                {
                    get => m_selected;
                    set => m_selected = value;
                }

                #endregion
            }

            #endregion

            #region Nested type: AutoHideStateCollection

            private class AutoHideStateCollection
            {
                #region Private Fields

                private readonly AutoHideState[] m_states;

                #endregion

                #region Ctor

                public AutoHideStateCollection()
                {
                    m_states = new[]
                    {
                        new AutoHideState(DockState.DockTopAutoHide),
                        new AutoHideState(DockState.DockBottomAutoHide),
                        new AutoHideState(DockState.DockLeftAutoHide),
                        new AutoHideState(DockState.DockRightAutoHide)
                    };
                }

                #endregion

                #region Properties / Indexers

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

                #region Public Methods

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

            #endregion
        }

        #endregion

        #region Nested type: Tab

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected class Tab : IDisposable
        {
            #region Ctor

            protected internal Tab(IDockContent content)
            {
                Content = content;
            }

            #endregion

            #region Interface IDisposable

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Properties / Indexers

            public IDockContent Content { get; }

            #endregion

            #region Private Methods

            protected virtual void Dispose(bool disposing)
            {
            }

            #endregion

            ~Tab()
            {
                Dispose(false);
            }
        }

        #endregion

        #region Nested type: TabCollection

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected sealed class TabCollection : IEnumerable<Tab>
        {
            #region Ctor

            internal TabCollection(DockPane pane)
            {
                DockPane = pane;
            }

            #endregion

            #region Interface IEnumerable<Tab>

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

            #region Properties / Indexers

            public int Count => DockPane.DisplayingContents.Count;

            public DockPane DockPane { get; }

            public DockPanel DockPanel => DockPane.DockPanel;

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

            #region Public Methods

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
        }

        #endregion

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

        protected internal Rectangle GetTabStripRectangle(DockState dockState)
        {
            int height = MeasureHeight();
            if (dockState == DockState.DockTopAutoHide && PanesTop.Count > 0)
            {
                return new Rectangle(RectangleTopLeft.Width, 0,
                    Width - RectangleTopLeft.Width - RectangleTopRight.Width, height);
            }

            if (dockState == DockState.DockBottomAutoHide && PanesBottom.Count > 0)
            {
                return new Rectangle(RectangleBottomLeft.Width, Height - height,
                    Width - RectangleBottomLeft.Width - RectangleBottomRight.Width, height);
            }

            if (dockState == DockState.DockLeftAutoHide && PanesLeft.Count > 0)
            {
                return new Rectangle(0, RectangleTopLeft.Width, height,
                    Height - RectangleTopLeft.Height - RectangleBottomLeft.Height);
            }

            if (dockState == DockState.DockRightAutoHide && PanesRight.Count > 0)
            {
                return new Rectangle(Width - height, RectangleTopRight.Width, height,
                    Height - RectangleTopRight.Height - RectangleBottomRight.Height);
            }

            return Rectangle.Empty;
        }

        protected internal abstract int MeasureHeight();
    }
}
