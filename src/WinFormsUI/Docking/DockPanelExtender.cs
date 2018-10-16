using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking
{
    public sealed class DockPanelExtender
    {
        #region Private Fields

        private IAutoHideStripFactory m_autoHideStripFactory;

        private IDockPaneCaptionFactory m_dockPaneCaptionFactory;

        private IDockPaneFactory m_dockPaneFactory;

        private IDockPaneStripFactory m_dockPaneStripFactory;

        private IFloatWindowFactory m_floatWindowFactory;

        #endregion

        #region Ctor

        internal DockPanelExtender(DockPanel dockPanel)
        {
            DockPanel = dockPanel;
        }

        #endregion

        #region Properties / Indexers

        public IAutoHideStripFactory AutoHideStripFactory
        {
            get
            {
                if (m_autoHideStripFactory == null)
                {
                    m_autoHideStripFactory = new DefaultAutoHideStripFactory();
                }

                return m_autoHideStripFactory;
            }

            set
            {
                if (DockPanel.Contents.Count > 0)
                {
                    throw new InvalidOperationException();
                }

                if (m_autoHideStripFactory == value)
                {
                    return;
                }

                m_autoHideStripFactory = value;
                DockPanel.ResetAutoHideStripControl();
            }
        }

        public IDockPaneCaptionFactory DockPaneCaptionFactory
        {
            get
            {
                if (m_dockPaneCaptionFactory == null)
                {
                    m_dockPaneCaptionFactory = new DefaultDockPaneCaptionFactory();
                }

                return m_dockPaneCaptionFactory;
            }

            set
            {
                if (DockPanel.Panes.Count > 0)
                {
                    throw new InvalidOperationException();
                }

                m_dockPaneCaptionFactory = value;
            }
        }

        public IDockPaneFactory DockPaneFactory
        {
            get
            {
                if (m_dockPaneFactory == null)
                {
                    m_dockPaneFactory = new DefaultDockPaneFactory();
                }

                return m_dockPaneFactory;
            }

            set
            {
                if (DockPanel.Panes.Count > 0)
                {
                    throw new InvalidOperationException();
                }

                m_dockPaneFactory = value;
            }
        }

        public IDockPaneStripFactory DockPaneStripFactory
        {
            get
            {
                if (m_dockPaneStripFactory == null)
                {
                    m_dockPaneStripFactory = new DefaultDockPaneStripFactory();
                }

                return m_dockPaneStripFactory;
            }

            set
            {
                if (DockPanel.Contents.Count > 0)
                {
                    throw new InvalidOperationException();
                }

                m_dockPaneStripFactory = value;
            }
        }

        public IFloatWindowFactory FloatWindowFactory
        {
            get
            {
                if (m_floatWindowFactory == null)
                {
                    m_floatWindowFactory = new DefaultFloatWindowFactory();
                }

                return m_floatWindowFactory;
            }

            set
            {
                if (DockPanel.FloatWindows.Count > 0)
                {
                    throw new InvalidOperationException();
                }

                m_floatWindowFactory = value;
            }
        }

        private DockPanel DockPanel { get; }

        #endregion

        #region Nested type: DefaultAutoHideStripFactory

        private class DefaultAutoHideStripFactory : IAutoHideStripFactory
        {
            #region Interface IAutoHideStripFactory

            public AutoHideStripBase CreateAutoHideStrip(DockPanel panel)
            {
                return new VS2005AutoHideStrip(panel);
            }

            #endregion
        }

        #endregion

        #region Nested type: DefaultDockPaneCaptionFactory

        private class DefaultDockPaneCaptionFactory : IDockPaneCaptionFactory
        {
            #region Interface IDockPaneCaptionFactory

            public DockPaneCaptionBase CreateDockPaneCaption(DockPane pane)
            {
                return new VS2005DockPaneCaption(pane);
            }

            #endregion
        }

        #endregion

        #region Nested type: DefaultDockPaneFactory

        private class DefaultDockPaneFactory : IDockPaneFactory
        {
            #region Interface IDockPaneFactory

            public DockPane CreateDockPane(IDockContent content, DockState visibleState, bool show)
            {
                return new DockPane(content, visibleState, show);
            }

            public DockPane CreateDockPane(IDockContent content, FloatWindow floatWindow, bool show)
            {
                return new DockPane(content, floatWindow, show);
            }

            public DockPane CreateDockPane(IDockContent content, DockPane prevPane, DockAlignment alignment,
                                           double proportion, bool show)
            {
                return new DockPane(content, prevPane, alignment, proportion, show);
            }

            public DockPane CreateDockPane(IDockContent content, Rectangle floatWindowBounds, bool show)
            {
                return new DockPane(content, floatWindowBounds, show);
            }

            #endregion
        }

        #endregion

        #region Nested type: DefaultDockPaneStripFactory

        private class DefaultDockPaneStripFactory : IDockPaneStripFactory
        {
            #region Interface IDockPaneStripFactory

            public DockPaneStripBase CreateDockPaneStrip(DockPane pane)
            {
                return new VS2005DockPaneStrip(pane);
            }

            #endregion
        }

        #endregion

        #region Nested type: DefaultFloatWindowFactory

        private class DefaultFloatWindowFactory : IFloatWindowFactory
        {
            #region Interface IFloatWindowFactory

            public FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane)
            {
                return new FloatWindow(dockPanel, pane);
            }

            public FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
            {
                return new FloatWindow(dockPanel, pane, bounds);
            }

            #endregion
        }

        #endregion

        #region Nested type: IAutoHideStripFactory

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IAutoHideStripFactory
        {
            #region Public Methods

            AutoHideStripBase CreateAutoHideStrip(DockPanel panel);

            #endregion
        }

        #endregion

        #region Nested type: IDockPaneCaptionFactory

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IDockPaneCaptionFactory
        {
            #region Public Methods

            DockPaneCaptionBase CreateDockPaneCaption(DockPane pane);

            #endregion
        }

        #endregion

        #region Nested type: IDockPaneFactory

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IDockPaneFactory
        {
            #region Public Methods

            DockPane CreateDockPane(IDockContent content, DockState visibleState, bool show);

            [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "1#")]
            DockPane CreateDockPane(IDockContent content, FloatWindow floatWindow, bool show);

            DockPane CreateDockPane(IDockContent content, DockPane previousPane, DockAlignment alignment,
                                    double proportion, bool show);

            [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "1#")]
            DockPane CreateDockPane(IDockContent content, Rectangle floatWindowBounds, bool show);

            #endregion
        }

        #endregion

        #region Nested type: IDockPaneStripFactory

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IDockPaneStripFactory
        {
            #region Public Methods

            DockPaneStripBase CreateDockPaneStrip(DockPane pane);

            #endregion
        }

        #endregion

        #region Nested type: IFloatWindowFactory

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IFloatWindowFactory
        {
            #region Public Methods

            FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane);
            FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds);

            #endregion
        }

        #endregion
    }
}
