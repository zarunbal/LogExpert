using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking.Win32;

namespace WeifenLuo.WinFormsUI.Docking
{
    public abstract class DockPaneStripBase : Control
    {
        #region Private Fields

        private TabCollection m_tabs;

        #endregion

        #region Ctor

        protected DockPaneStripBase(DockPane pane)
        {
            DockPane = pane;

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, false);
            AllowDrop = true;
        }

        #endregion

        #region Properties / Indexers

        protected DockPane.AppearanceStyle Appearance => DockPane.Appearance;

        protected DockPane DockPane { get; }

        protected bool HasTabPageContextMenu => DockPane.HasTabPageContextMenu;

        protected TabCollection Tabs
        {
            get
            {
                if (m_tabs == null)
                {
                    m_tabs = new TabCollection(DockPane);
                }

                return m_tabs;
            }
        }

        #endregion

        #region Overrides

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            int index = HitTest();
            if (index != -1)
            {
                IDockContent content = Tabs[index].Content;
                if (DockPane.ActiveContent != content)
                {
                    DockPane.ActiveContent = content;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            int index = HitTest();
            if (index != -1)
            {
                IDockContent content = Tabs[index].Content;
                if (DockPane.ActiveContent != content)
                {
                    DockPane.ActiveContent = content;
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                if (DockPane.DockPanel.AllowEndUserDocking && DockPane.AllowDockDragAndDrop &&
                    DockPane.ActiveContent.DockHandler.AllowEndUserDocking)
                {
                    DockPane.DockPanel.BeginDrag(DockPane.ActiveContent.DockHandler);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right)
            {
                ShowTabPageContextMenu(new Point(e.X, e.Y));
            }
            else if (e.Button == MouseButtons.Middle &&
                     DockPane.Appearance == DockPane.AppearanceStyle.Document)
            {
                // Get the content located under the click (if there is one)
                int index = HitTest();
                if (index != -1)
                {
                    // Close the specified content.
                    IDockContent content = Tabs[index].Content;
                    DockPane.CloseContent(content);
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)Msgs.WM_LBUTTONDBLCLK)
            {
                base.WndProc(ref m);

                int index = HitTest();
                if (DockPane.DockPanel.AllowEndUserDocking && index != -1)
                {
                    IDockContent content = Tabs[index].Content;
                    if (content.DockHandler.CheckDockState(!content.DockHandler.IsFloat) != DockState.Unknown)
                    {
                        content.DockHandler.IsFloat = !content.DockHandler.IsFloat;
                    }
                }

                return;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Event handling Methods

        protected virtual void OnRefreshChanges()
        {
        }

        #endregion

        #region Private Methods

        protected int HitTest()
        {
            return HitTest(PointToClient(MousePosition));
        }

        protected void ShowTabPageContextMenu(Point position)
        {
            DockPane.ShowTabPageContextMenu(this, position);
        }

        #endregion

        #region Nested type: Tab

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected internal class Tab : IDisposable
        {
            #region Ctor

            public Tab(IDockContent content)
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

            public Form ContentForm => Content as Form;

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

            public Tab this[int index]
            {
                get
                {
                    IDockContent content = DockPane.DisplayingContents[index];
                    if (content == null)
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }

                    return content.DockHandler.GetTab(DockPane.TabStripControl);
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

                return DockPane.DisplayingContents.IndexOf(tab.Content);
            }

            public int IndexOf(IDockContent content)
            {
                return DockPane.DisplayingContents.IndexOf(content);
            }

            #endregion
        }

        #endregion

        internal void RefreshChanges()
        {
            if (IsDisposed)
            {
                return;
            }

            OnRefreshChanges();
        }

        protected internal abstract int MeasureHeight();

        protected internal abstract void EnsureTabVisible(IDockContent content);

        protected internal abstract int HitTest(Point point);

        protected internal abstract GraphicsPath GetOutline(int index);

        protected internal virtual Tab CreateTab(IDockContent content)
        {
            return new Tab(content);
        }
    }
}
