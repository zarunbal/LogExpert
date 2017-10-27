using System;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;

namespace WeifenLuo.WinFormsUI.Docking
{
    public sealed class DockPanelExtender
    {
        #region Fields

        private IAutoHideStripFactory m_autoHideStripFactory = null;

        private IDockPaneCaptionFactory m_dockPaneCaptionFactory = null;

        private IDockPaneFactory m_dockPaneFactory = null;

        private IDockPaneStripFactory m_dockPaneStripFactory = null;

        private IFloatWindowFactory m_floatWindowFactory = null;

        #endregion

        #region cTor

        internal DockPanelExtender(DockPanel dockPanel)
        {
            DockPanel = dockPanel;
        }

        #endregion

        #region Properties

        private DockPanel DockPanel { get; }

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

        #endregion

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IDockPaneFactory
        {
            #region Public methods

            DockPane CreateDockPane(IDockContent content, DockState visibleState, bool show);

            [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "1#")]
            DockPane CreateDockPane(IDockContent content, FloatWindow floatWindow, bool show);

            DockPane CreateDockPane(IDockContent content, DockPane previousPane, DockAlignment alignment,
                double proportion, bool show);

            [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "1#")]
            DockPane CreateDockPane(IDockContent content, Rectangle floatWindowBounds, bool show);

            #endregion
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IFloatWindowFactory
        {
            #region Public methods

            FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane);
            FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds);

            #endregion
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IDockPaneCaptionFactory
        {
            #region Public methods

            DockPaneCaptionBase CreateDockPaneCaption(DockPane pane);

            #endregion
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IDockPaneStripFactory
        {
            #region Public methods

            DockPaneStripBase CreateDockPaneStrip(DockPane pane);

            #endregion
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public interface IAutoHideStripFactory
        {
            #region Public methods

            AutoHideStripBase CreateAutoHideStrip(DockPanel panel);

            #endregion
        }

        #region DefaultDockPaneFactory

        private class DefaultDockPaneFactory : IDockPaneFactory
        {
            #region Public methods

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

        #region DefaultFloatWindowFactory

        private class DefaultFloatWindowFactory : IFloatWindowFactory
        {
            #region Public methods

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

        #region DefaultDockPaneCaptionFactory

        private class DefaultDockPaneCaptionFactory : IDockPaneCaptionFactory
        {
            #region Public methods

            public DockPaneCaptionBase CreateDockPaneCaption(DockPane pane)
            {
                return new VS2005DockPaneCaption(pane);
            }

            #endregion
        }

        #endregion

        #region DefaultDockPaneTabStripFactory

        private class DefaultDockPaneStripFactory : IDockPaneStripFactory
        {
            #region Public methods

            public DockPaneStripBase CreateDockPaneStrip(DockPane pane)
            {
                return new VS2005DockPaneStrip(pane);
            }

            #endregion
        }

        #endregion

        #region DefaultAutoHideStripFactory

        private class DefaultAutoHideStripFactory : IAutoHideStripFactory
        {
            #region Public methods

            public AutoHideStripBase CreateAutoHideStrip(DockPanel panel)
            {
                return new VS2005AutoHideStrip(panel);
            }

            #endregion
        }

        #endregion
    }
}