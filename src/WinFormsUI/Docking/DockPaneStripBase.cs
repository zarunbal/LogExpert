using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public abstract class DockPaneStripBase : Control
    {
        #region Fields

        private TabCollection m_tabs = null;

        #endregion

        #region cTor

        protected DockPaneStripBase(DockPane pane)
        {
            DockPane = pane;

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, false);
            AllowDrop = true;
        }

        #endregion

        #region Properties

        protected DockPane DockPane { get; }

        protected DockPane.AppearanceStyle Appearance
        {
            get { return DockPane.Appearance; }
        }

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

        protected bool HasTabPageContextMenu
        {
            get { return DockPane.HasTabPageContextMenu; }
        }

        #endregion

        #region Internals

        internal void RefreshChanges()
        {
            if (IsDisposed)
            {
                return;
            }

            OnRefreshChanges();
        }

        #endregion

        #region Overrides

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
                     DockPane.Appearance == Docking.DockPane.AppearanceStyle.Document)
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

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int) Win32.Msgs.WM_LBUTTONDBLCLK)
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
            return;
        }

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

        #endregion

        protected virtual void OnRefreshChanges()
        {
        }

        protected internal abstract int MeasureHeight();

        protected internal abstract void EnsureTabVisible(IDockContent content);

        protected int HitTest()
        {
            return HitTest(PointToClient(Control.MousePosition));
        }

        protected internal abstract int HitTest(Point point);

        protected internal abstract GraphicsPath GetOutline(int index);

        protected internal virtual Tab CreateTab(IDockContent content)
        {
            return new Tab(content);
        }

        protected void ShowTabPageContextMenu(Point position)
        {
            DockPane.ShowTabPageContextMenu(this, position);
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        protected internal class Tab : IDisposable
        {
            #region Fields

            #endregion

            #region cTor

            public Tab(IDockContent content)
            {
                Content = content;
            }

            #endregion

            #region Properties

            public IDockContent Content { get; }

            public Form ContentForm
            {
                get { return Content as Form; }
            }

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

            public DockPane DockPane { get; }

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
                    return content.DockHandler.GetTab(DockPane.TabStripControl);
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

                return DockPane.DisplayingContents.IndexOf(tab.Content);
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
    }
}