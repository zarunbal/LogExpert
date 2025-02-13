using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

// To simplify the process of finding the toolbox bitmap resource:
// #1 Create an internal class called "resfinder" outside of the root namespace.
// #2 Use "resfinder" in the toolbox bitmap attribute instead of the control name.
// #3 use the "<default namespace>.<resourcename>" string to locate the resource.
// See: http://www.bobpowell.net/toolboxbitmap.htm
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
internal class resfinder
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
{
}

namespace WeifenLuo.WinFormsUI.Docking
{
    [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "0#")]
    public delegate IDockContent DeserializeDockContent(string persistString);

    [LocalizedDescription("DockPanel_Description")]
    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    [ToolboxBitmap(typeof(resfinder), "WeifenLuo.WinFormsUI.Docking.DockPanel.bmp")]
    [DefaultProperty("DocumentStyle")]
    [DefaultEvent("ActiveContentChanged")]
    public partial class DockPanel : Panel
    {
        #region Fields

        private static readonly object ContentAddedEvent = new object();

        private static readonly object ContentRemovedEvent = new object();

        private AutoHideStripBase m_autoHideStripControl = null;

        private Color m_BackColor;

        private Rectangle[] m_clipRects = null;

        private bool m_disposed = false;

        private double m_dockBottomPortion = 0.25;

        private double m_dockLeftPortion = 0.25;

        private double m_dockRightPortion = 0.25;

        private double m_dockTopPortion = 0.25;

        private DocumentStyle m_documentStyle = DocumentStyle.DockingMdi;

        private PaintEventHandler m_dummyControlPaintEventHandler = null;
        private readonly FocusManagerImpl m_focusManager;

        private bool m_rightToLeftLayout = false;

        private bool m_showDocumentIcon = false;

        #endregion

        #region cTor

        public DockPanel()
        {
            m_focusManager = new FocusManagerImpl(this);
            Extender = new DockPanelExtender(this);
            Panes = new DockPaneCollection();
            FloatWindows = new FloatWindowCollection();

            SuspendLayout();

            AutoHideWindow = new AutoHideWindowControl(this);
            AutoHideWindow.Visible = false;
            SetAutoHideWindowParent();

            DummyControl = new DummyControl();
            DummyControl.Bounds = new Rectangle(0, 0, 1, 1);
            Controls.Add(DummyControl);

            DockWindows = new DockWindowCollection(this);
            Controls.AddRange(new Control[]
            {
                DockWindows[DockState.Document],
                DockWindows[DockState.DockLeft],
                DockWindows[DockState.DockRight],
                DockWindows[DockState.DockTop],
                DockWindows[DockState.DockBottom]
            });

            DummyContent = new DockContent();
            ResumeLayout();
        }

        #endregion

        #region Events

        [LocalizedCategory("Category_DockingNotification")]
        [LocalizedDescription("DockPanel_ContentAdded_Description")]
        public event EventHandler<DockContentEventArgs> ContentAdded
        {
            add { Events.AddHandler(ContentAddedEvent, value); }
            remove { Events.RemoveHandler(ContentAddedEvent, value); }
        }

        [LocalizedCategory("Category_DockingNotification")]
        [LocalizedDescription("DockPanel_ContentRemoved_Description")]
        public event EventHandler<DockContentEventArgs> ContentRemoved
        {
            add { Events.AddHandler(ContentRemovedEvent, value); }
            remove { Events.RemoveHandler(ContentRemovedEvent, value); }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Determines the color with which the client rectangle will be drawn.
        /// If this property is used instead of the BackColor it will not have any influence on the borders to the surrounding controls (DockPane).
        /// The BackColor property changes the borders of surrounding controls (DockPane).
        /// Alternatively both properties may be used (BackColor to draw and define the color of the borders and DockBackColor to define the color of the client rectangle).
        /// For Backgroundimages: Set your prefered Image, then set the DockBackColor and the BackColor to the same Color (Control)
        /// </summary>
        [Description("Determines the color with which the client rectangle will be drawn.\r\n" +
                     "If this property is used instead of the BackColor it will not have any influence on the borders to the surrounding controls (DockPane).\r\n" +
                     "The BackColor property changes the borders of surrounding controls (DockPane).\r\n" +
                     "Alternatively both properties may be used (BackColor to draw and define the color of the borders and DockBackColor to define the color of the client rectangle).\r\n" +
                     "For Backgroundimages: Set your prefered Image, then set the DockBackColor and the BackColor to the same Color (Control).")]
        public Color DockBackColor
        {
            get { return !m_BackColor.IsEmpty ? m_BackColor : base.BackColor; }
            set
            {
                if (m_BackColor != value)
                {
                    m_BackColor = value;
                    Refresh();
                }
            }
        }

        internal AutoHideStripBase AutoHideStripControl
        {
            get
            {
                if (m_autoHideStripControl == null)
                {
                    m_autoHideStripControl = AutoHideStripFactory.CreateAutoHideStrip(this);
                    Controls.Add(m_autoHideStripControl);
                }
                return m_autoHideStripControl;
            }
        }

        [Browsable(false)]
        public IDockContent ActiveAutoHideContent
        {
            get { return AutoHideWindow.ActiveContent; }
            set { AutoHideWindow.ActiveContent = value; }
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_AllowEndUserDocking_Description")]
        [DefaultValue(true)]
        public bool AllowEndUserDocking { get; set; } = true;

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_AllowEndUserNestedDocking_Description")]
        [DefaultValue(true)]
        public bool AllowEndUserNestedDocking { get; set; } = true;

        [Browsable(false)]
        public DockContentCollection Contents { get; } = new DockContentCollection();

        internal DockContent DummyContent { get; }

        [DefaultValue(false)]
        [LocalizedCategory("Appearance")]
        [LocalizedDescription("DockPanel_RightToLeftLayout_Description")]
        public bool RightToLeftLayout
        {
            get { return m_rightToLeftLayout; }
            set
            {
                if (m_rightToLeftLayout == value)
                {
                    return;
                }

                m_rightToLeftLayout = value;
                foreach (FloatWindow floatWindow in FloatWindows)
                {
                    floatWindow.RightToLeftLayout = value;
                }
            }
        }

        [DefaultValue(false)]
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_ShowDocumentIcon_Description")]
        public bool ShowDocumentIcon
        {
            get { return m_showDocumentIcon; }
            set
            {
                if (m_showDocumentIcon == value)
                {
                    return;
                }

                m_showDocumentIcon = value;
                Refresh();
            }
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockPanelSkin")]
        public DockPanelSkin Skin { get; set; } = new DockPanelSkin();

        [DefaultValue(DocumentTabStripLocation.Top)]
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DocumentTabStripLocation")]
        public DocumentTabStripLocation DocumentTabStripLocation { get; set; } = DocumentTabStripLocation.Top;

        [Browsable(false)]
        public DockPanelExtender Extender { get; }

        [Browsable(false)]
        public DockPanelExtender.IDockPaneFactory DockPaneFactory
        {
            get { return Extender.DockPaneFactory; }
        }

        [Browsable(false)]
        public DockPanelExtender.IFloatWindowFactory FloatWindowFactory
        {
            get { return Extender.FloatWindowFactory; }
        }

        internal DockPanelExtender.IDockPaneCaptionFactory DockPaneCaptionFactory
        {
            get { return Extender.DockPaneCaptionFactory; }
        }

        internal DockPanelExtender.IDockPaneStripFactory DockPaneStripFactory
        {
            get { return Extender.DockPaneStripFactory; }
        }

        internal DockPanelExtender.IAutoHideStripFactory AutoHideStripFactory
        {
            get { return Extender.AutoHideStripFactory; }
        }

        [Browsable(false)]
        public DockPaneCollection Panes { get; }

        internal Rectangle DockArea
        {
            get
            {
                return new Rectangle(DockPadding.Left, DockPadding.Top,
                    ClientRectangle.Width - DockPadding.Left - DockPadding.Right,
                    ClientRectangle.Height - DockPadding.Top - DockPadding.Bottom);
            }
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockBottomPortion_Description")]
        [DefaultValue(0.25)]
        public double DockBottomPortion
        {
            get { return m_dockBottomPortion; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value == m_dockBottomPortion)
                {
                    return;
                }

                m_dockBottomPortion = value;

                if (m_dockBottomPortion < 1 && m_dockTopPortion < 1)
                {
                    if (m_dockTopPortion + m_dockBottomPortion > 1)
                    {
                        m_dockTopPortion = 1 - m_dockBottomPortion;
                    }
                }

                PerformLayout();
            }
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockLeftPortion_Description")]
        [DefaultValue(0.25)]
        public double DockLeftPortion
        {
            get { return m_dockLeftPortion; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value == m_dockLeftPortion)
                {
                    return;
                }

                m_dockLeftPortion = value;

                if (m_dockLeftPortion < 1 && m_dockRightPortion < 1)
                {
                    if (m_dockLeftPortion + m_dockRightPortion > 1)
                    {
                        m_dockRightPortion = 1 - m_dockLeftPortion;
                    }
                }
                PerformLayout();
            }
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockRightPortion_Description")]
        [DefaultValue(0.25)]
        public double DockRightPortion
        {
            get { return m_dockRightPortion; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value == m_dockRightPortion)
                {
                    return;
                }

                m_dockRightPortion = value;

                if (m_dockLeftPortion < 1 && m_dockRightPortion < 1)
                {
                    if (m_dockLeftPortion + m_dockRightPortion > 1)
                    {
                        m_dockLeftPortion = 1 - m_dockRightPortion;
                    }
                }
                PerformLayout();
            }
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockTopPortion_Description")]
        [DefaultValue(0.25)]
        public double DockTopPortion
        {
            get { return m_dockTopPortion; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value == m_dockTopPortion)
                {
                    return;
                }

                m_dockTopPortion = value;

                if (m_dockTopPortion < 1 && m_dockBottomPortion < 1)
                {
                    if (m_dockTopPortion + m_dockBottomPortion > 1)
                    {
                        m_dockBottomPortion = 1 - m_dockTopPortion;
                    }
                }
                PerformLayout();
            }
        }

        [Browsable(false)]
        public DockWindowCollection DockWindows { get; }

        [Browsable(false)]
        public int DocumentsCount
        {
            get
            {
                int count = 0;
                foreach (IDockContent content in Documents)
                {
                    count++;
                }

                return count;
            }
        }

        [Browsable(false)]
        public IEnumerable<IDockContent> Documents
        {
            get
            {
                foreach (IDockContent content in Contents)
                {
                    if (content.DockHandler.DockState == DockState.Document)
                    {
                        yield return content;
                    }
                }
            }
        }

        private Rectangle DocumentRectangle
        {
            get
            {
                Rectangle rect = DockArea;
                if (DockWindows[DockState.DockLeft].VisibleNestedPanes.Count != 0)
                {
                    rect.X += (int)(DockArea.Width * DockLeftPortion);
                    rect.Width -= (int)(DockArea.Width * DockLeftPortion);
                }
                if (DockWindows[DockState.DockRight].VisibleNestedPanes.Count != 0)
                {
                    rect.Width -= (int)(DockArea.Width * DockRightPortion);
                }
                if (DockWindows[DockState.DockTop].VisibleNestedPanes.Count != 0)
                {
                    rect.Y += (int)(DockArea.Height * DockTopPortion);
                    rect.Height -= (int)(DockArea.Height * DockTopPortion);
                }
                if (DockWindows[DockState.DockBottom].VisibleNestedPanes.Count != 0)
                {
                    rect.Height -= (int)(DockArea.Height * DockBottomPortion);
                }

                return rect;
            }
        }

        private Control DummyControl { get; }

        [Browsable(false)]
        public FloatWindowCollection FloatWindows { get; }

        [Category("Layout")]
        [LocalizedDescription("DockPanel_DefaultFloatWindowSize_Description")]
        public Size DefaultFloatWindowSize { get; set; } = new Size(300, 300);

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DocumentStyle_Description")]
        [DefaultValue(DocumentStyle.DockingMdi)]
        public DocumentStyle DocumentStyle
        {
            get { return m_documentStyle; }
            set
            {
                if (value == m_documentStyle)
                {
                    return;
                }

                if (!Enum.IsDefined(typeof(DocumentStyle), value))
                {
                    throw new InvalidEnumArgumentException();
                }

                if (value == DocumentStyle.SystemMdi && DockWindows[DockState.Document].VisibleNestedPanes.Count > 0)
                {
                    throw new InvalidEnumArgumentException();
                }

                m_documentStyle = value;

                SuspendLayout(true);

                SetAutoHideWindowParent();
                SetMdiClient();
                InvalidateWindowRegion();

                foreach (IDockContent content in Contents)
                {
                    if (content.DockHandler.DockState == DockState.Document)
                    {
                        content.DockHandler.SetPaneAndVisible(content.DockHandler.Pane);
                    }
                }

                PerformMdiClientLayout();

                ResumeLayout(true, true);
            }
        }

        internal Form ParentForm
        {
            get
            {
                if (!IsParentFormValid())
                {
                    throw new InvalidOperationException(Strings.DockPanel_ParentForm_Invalid);
                }

                return GetMdiClientController().ParentForm;
            }
        }

        private Rectangle SystemMdiClientBounds
        {
            get
            {
                if (!IsParentFormValid() || !Visible)
                {
                    return Rectangle.Empty;
                }

                Rectangle rect = ParentForm.RectangleToClient(RectangleToScreen(DocumentWindowBounds));
                return rect;
            }
        }

        internal Rectangle DocumentWindowBounds
        {
            get
            {
                Rectangle rectDocumentBounds = DisplayRectangle;
                if (DockWindows[DockState.DockLeft].Visible)
                {
                    rectDocumentBounds.X += DockWindows[DockState.DockLeft].Width;
                    rectDocumentBounds.Width -= DockWindows[DockState.DockLeft].Width;
                }
                if (DockWindows[DockState.DockRight].Visible)
                {
                    rectDocumentBounds.Width -= DockWindows[DockState.DockRight].Width;
                }
                if (DockWindows[DockState.DockTop].Visible)
                {
                    rectDocumentBounds.Y += DockWindows[DockState.DockTop].Height;
                    rectDocumentBounds.Height -= DockWindows[DockState.DockTop].Height;
                }
                if (DockWindows[DockState.DockBottom].Visible)
                {
                    rectDocumentBounds.Height -= DockWindows[DockState.DockBottom].Height;
                }

                return rectDocumentBounds;
            }
        }

        #endregion

        #region Public methods

        public void UpdateDockWindowZOrder(DockStyle dockStyle, bool fullPanelEdge)
        {
            if (dockStyle == DockStyle.Left)
            {
                if (fullPanelEdge)
                {
                    DockWindows[DockState.DockLeft].SendToBack();
                }
                else
                {
                    DockWindows[DockState.DockLeft].BringToFront();
                }
            }
            else if (dockStyle == DockStyle.Right)
            {
                if (fullPanelEdge)
                {
                    DockWindows[DockState.DockRight].SendToBack();
                }
                else
                {
                    DockWindows[DockState.DockRight].BringToFront();
                }
            }
            else if (dockStyle == DockStyle.Top)
            {
                if (fullPanelEdge)
                {
                    DockWindows[DockState.DockTop].SendToBack();
                }
                else
                {
                    DockWindows[DockState.DockTop].BringToFront();
                }
            }
            else if (dockStyle == DockStyle.Bottom)
            {
                if (fullPanelEdge)
                {
                    DockWindows[DockState.DockBottom].SendToBack();
                }
                else
                {
                    DockWindows[DockState.DockBottom].BringToFront();
                }
            }
        }

        public IDockContent[] DocumentsToArray()
        {
            int count = DocumentsCount;
            IDockContent[] documents = new IDockContent[count];
            int i = 0;
            foreach (IDockContent content in Documents)
            {
                documents[i] = content;
                i++;
            }

            return documents;
        }

        public void SetPaneIndex(DockPane pane, int index)
        {
            int oldIndex = Panes.IndexOf(pane);
            if (oldIndex == -1)
            {
                throw new ArgumentException(Strings.DockPanel_SetPaneIndex_InvalidPane);
            }

            if (index < 0 || index > Panes.Count - 1)
            {
                if (index != -1)
                {
                    throw new ArgumentOutOfRangeException(Strings.DockPanel_SetPaneIndex_InvalidIndex);
                }
            }

            if (oldIndex == index)
            {
                return;
            }
            if (oldIndex == Panes.Count - 1 && index == -1)
            {
                return;
            }

            Panes.Remove(pane);
            if (index == -1)
            {
                Panes.Add(pane);
            }
            else if (oldIndex < index)
            {
                Panes.AddAt(pane, index - 1);
            }
            else
            {
                Panes.AddAt(pane, index);
            }
        }

        public void SuspendLayout(bool allWindows)
        {
            FocusManager.SuspendFocusTracking();
            SuspendLayout();
            if (allWindows)
            {
                SuspendMdiClientLayout();
            }
        }

        public void ResumeLayout(bool performLayout, bool allWindows)
        {
            FocusManager.ResumeFocusTracking();
            ResumeLayout(performLayout);
            if (allWindows)
            {
                ResumeMdiClientLayout(performLayout);
            }
        }

        #endregion

        #region Internals

        internal void ResetAutoHideStripControl()
        {
            if (m_autoHideStripControl != null)
            {
                m_autoHideStripControl.Dispose();
            }

            m_autoHideStripControl = null;
        }

        internal Rectangle GetTabStripRectangle(DockState dockState)
        {
            return AutoHideStripControl.GetTabStripRectangle(dockState);
        }

        internal void AddContent(IDockContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException();
            }

            if (!Contents.Contains(content))
            {
                Contents.Add(content);
                OnContentAdded(new DockContentEventArgs(content));
            }
        }

        internal void AddPane(DockPane pane)
        {
            if (Panes.Contains(pane))
            {
                return;
            }

            Panes.Add(pane);
        }

        internal void AddFloatWindow(FloatWindow floatWindow)
        {
            if (FloatWindows.Contains(floatWindow))
            {
                return;
            }

            FloatWindows.Add(floatWindow);
        }

        internal void RemoveContent(IDockContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException();
            }

            if (Contents.Contains(content))
            {
                Contents.Remove(content);
                OnContentRemoved(new DockContentEventArgs(content));
            }
        }

        internal void RemovePane(DockPane pane)
        {
            if (!Panes.Contains(pane))
            {
                return;
            }

            Panes.Remove(pane);
        }

        internal void RemoveFloatWindow(FloatWindow floatWindow)
        {
            if (!FloatWindows.Contains(floatWindow))
            {
                return;
            }

            FloatWindows.Remove(floatWindow);
        }

        #endregion

        #region Overrides

        protected override void Dispose(bool disposing)
        {
            lock (this)
            {
                if (!m_disposed && disposing)
                {
                    m_focusManager.Dispose();
                    if (m_mdiClientController != null)
                    {
                        m_mdiClientController.HandleAssigned -= new EventHandler(MdiClientHandleAssigned);
                        m_mdiClientController.MdiChildActivate -= new EventHandler(ParentFormMdiChildActivate);
                        m_mdiClientController.Layout -= new LayoutEventHandler(MdiClient_Layout);
                        m_mdiClientController.Dispose();
                    }
                    FloatWindows.Dispose();
                    Panes.Dispose();
                    DummyContent.Dispose();

                    m_disposed = true;
                }

                base.Dispose(disposing);
            }
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            foreach (FloatWindow floatWindow in FloatWindows)
            {
                if (floatWindow.RightToLeft != RightToLeft)
                {
                    floatWindow.RightToLeft = RightToLeft;
                }
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            SuspendLayout(true);

            AutoHideStripControl.Bounds = ClientRectangle;

            CalculateDockPadding();

            DockWindows[DockState.DockLeft].Width = GetDockWindowSize(DockState.DockLeft);
            DockWindows[DockState.DockRight].Width = GetDockWindowSize(DockState.DockRight);
            DockWindows[DockState.DockTop].Height = GetDockWindowSize(DockState.DockTop);
            DockWindows[DockState.DockBottom].Height = GetDockWindowSize(DockState.DockBottom);

            AutoHideWindow.Bounds = GetAutoHideWindowBounds(AutoHideWindowRectangle);

            DockWindows[DockState.Document].BringToFront();
            AutoHideWindow.BringToFront();

            base.OnLayout(levent);

            if (DocumentStyle == DocumentStyle.SystemMdi && MdiClientExists)
            {
                SetMdiClientBounds(SystemMdiClientBounds);
                InvalidateWindowRegion();
            }
            else if (DocumentStyle == DocumentStyle.DockingMdi)
            {
                InvalidateWindowRegion();
            }

            ResumeLayout(true, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (DockBackColor == BackColor)
            {
                return;
            }

            Graphics g = e.Graphics;
            SolidBrush bgBrush = new SolidBrush(DockBackColor);
            g.FillRectangle(bgBrush, ClientRectangle);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            SetAutoHideWindowParent();
            GetMdiClientController().ParentForm = this.Parent as Form;
            base.OnParentChanged(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                SetMdiClient();
            }
        }

        #endregion

        #region Private Methods

        private bool ShouldSerializeDefaultFloatWindowSize()
        {
            return DefaultFloatWindowSize != new Size(300, 300);
        }

        private int GetDockWindowSize(DockState dockState)
        {
            if (dockState == DockState.DockLeft || dockState == DockState.DockRight)
            {
                int width = ClientRectangle.Width - DockPadding.Left - DockPadding.Right;
                int dockLeftSize = m_dockLeftPortion >= 1 ? (int)m_dockLeftPortion : (int)(width * m_dockLeftPortion);
                int dockRightSize = m_dockRightPortion >= 1
                    ? (int)m_dockRightPortion
                    : (int)(width * m_dockRightPortion);

                if (dockLeftSize < MeasurePane.MinSize)
                {
                    dockLeftSize = MeasurePane.MinSize;
                }
                if (dockRightSize < MeasurePane.MinSize)
                {
                    dockRightSize = MeasurePane.MinSize;
                }

                if (dockLeftSize + dockRightSize > width - MeasurePane.MinSize)
                {
                    int adjust = dockLeftSize + dockRightSize - (width - MeasurePane.MinSize);
                    dockLeftSize -= adjust / 2;
                    dockRightSize -= adjust / 2;
                }

                return dockState == DockState.DockLeft ? dockLeftSize : dockRightSize;
            }
            else if (dockState == DockState.DockTop || dockState == DockState.DockBottom)
            {
                int height = ClientRectangle.Height - DockPadding.Top - DockPadding.Bottom;
                int dockTopSize = m_dockTopPortion >= 1 ? (int)m_dockTopPortion : (int)(height * m_dockTopPortion);
                int dockBottomSize = m_dockBottomPortion >= 1
                    ? (int)m_dockBottomPortion
                    : (int)(height * m_dockBottomPortion);

                if (dockTopSize < MeasurePane.MinSize)
                {
                    dockTopSize = MeasurePane.MinSize;
                }
                if (dockBottomSize < MeasurePane.MinSize)
                {
                    dockBottomSize = MeasurePane.MinSize;
                }

                if (dockTopSize + dockBottomSize > height - MeasurePane.MinSize)
                {
                    int adjust = dockTopSize + dockBottomSize - (height - MeasurePane.MinSize);
                    dockTopSize -= adjust / 2;
                    dockBottomSize -= adjust / 2;
                }

                return dockState == DockState.DockTop ? dockTopSize : dockBottomSize;
            }
            else
            {
                return 0;
            }
        }

        private void CalculateDockPadding()
        {
            DockPadding.All = 0;

            int height = AutoHideStripControl.MeasureHeight();

            if (AutoHideStripControl.GetNumberOfPanes(DockState.DockLeftAutoHide) > 0)
            {
                DockPadding.Left = height;
            }
            if (AutoHideStripControl.GetNumberOfPanes(DockState.DockRightAutoHide) > 0)
            {
                DockPadding.Right = height;
            }
            if (AutoHideStripControl.GetNumberOfPanes(DockState.DockTopAutoHide) > 0)
            {
                DockPadding.Top = height;
            }
            if (AutoHideStripControl.GetNumberOfPanes(DockState.DockBottomAutoHide) > 0)
            {
                DockPadding.Bottom = height;
            }
        }

        private bool IsParentFormValid()
        {
            if (DocumentStyle == DocumentStyle.DockingSdi || DocumentStyle == DocumentStyle.DockingWindow)
            {
                return true;
            }

            if (!MdiClientExists)
            {
                GetMdiClientController().RenewMdiClient();
            }

            return MdiClientExists;
        }

        private void SetAutoHideWindowParent()
        {
            Control parent;
            if (DocumentStyle == DocumentStyle.DockingMdi ||
                DocumentStyle == DocumentStyle.SystemMdi)
            {
                parent = this.Parent;
            }
            else
            {
                parent = this;
            }
            if (AutoHideWindow.Parent != parent)
            {
                AutoHideWindow.Parent = parent;
                AutoHideWindow.BringToFront();
            }
        }

        private void InvalidateWindowRegion()
        {
            if (DesignMode)
            {
                return;
            }

            if (m_dummyControlPaintEventHandler == null)
            {
                m_dummyControlPaintEventHandler = new PaintEventHandler(DummyControl_Paint);
            }

            DummyControl.Paint += m_dummyControlPaintEventHandler;
            DummyControl.Invalidate();
        }

        private void UpdateWindowRegion()
        {
            if (this.DocumentStyle == DocumentStyle.DockingMdi)
            {
                UpdateWindowRegion_ClipContent();
            }
            else if (this.DocumentStyle == DocumentStyle.DockingSdi ||
                     this.DocumentStyle == DocumentStyle.DockingWindow)
            {
                UpdateWindowRegion_FullDocumentArea();
            }
            else if (this.DocumentStyle == DocumentStyle.SystemMdi)
            {
                UpdateWindowRegion_EmptyDocumentArea();
            }
        }

        private void UpdateWindowRegion_FullDocumentArea()
        {
            SetRegion(null);
        }

        private void UpdateWindowRegion_EmptyDocumentArea()
        {
            Rectangle rect = DocumentWindowBounds;
            SetRegion(new Rectangle[] { rect });
        }

        private void UpdateWindowRegion_ClipContent()
        {
            int count = 0;
            foreach (DockPane pane in this.Panes)
            {
                if (!pane.Visible || pane.DockState != DockState.Document)
                {
                    continue;
                }

                count++;
            }

            if (count == 0)
            {
                SetRegion(null);
                return;
            }

            Rectangle[] rects = new Rectangle[count];
            int i = 0;
            foreach (DockPane pane in this.Panes)
            {
                if (!pane.Visible || pane.DockState != DockState.Document)
                {
                    continue;
                }

                rects[i] = RectangleToClient(pane.RectangleToScreen(pane.ContentRectangle));
                i++;
            }

            SetRegion(rects);
        }

        private void SetRegion(Rectangle[] clipRects)
        {
            if (!IsClipRectsChanged(clipRects))
            {
                return;
            }

            m_clipRects = clipRects;

            if (m_clipRects == null || m_clipRects.GetLength(0) == 0)
            {
                Region = null;
            }
            else
            {
                Region region = new Region(new Rectangle(0, 0, this.Width, this.Height));
                foreach (Rectangle rect in m_clipRects)
                {
                    region.Exclude(rect);
                }
                Region = region;
            }
        }

        private bool IsClipRectsChanged(Rectangle[] clipRects)
        {
            if (clipRects == null && m_clipRects == null)
            {
                return false;
            }
            else if (clipRects == null != (m_clipRects == null))
            {
                return true;
            }

            foreach (Rectangle rect in clipRects)
            {
                bool matched = false;
                foreach (Rectangle rect2 in m_clipRects)
                {
                    if (rect == rect2)
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                {
                    return true;
                }
            }

            foreach (Rectangle rect2 in m_clipRects)
            {
                bool matched = false;
                foreach (Rectangle rect in clipRects)
                {
                    if (rect == rect2)
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Events handler

        private void MdiClientHandleAssigned(object sender, EventArgs e)
        {
            SetMdiClient();
            PerformLayout();
        }

        private void MdiClient_Layout(object sender, LayoutEventArgs e)
        {
            if (DocumentStyle != DocumentStyle.DockingMdi)
            {
                return;
            }

            foreach (DockPane pane in Panes)
            {
                if (pane.DockState == DockState.Document)
                {
                    pane.SetContentBounds();
                }
            }

            InvalidateWindowRegion();
        }

        private void DummyControl_Paint(object sender, PaintEventArgs e)
        {
            DummyControl.Paint -= m_dummyControlPaintEventHandler;
            UpdateWindowRegion();
        }

        #endregion

        protected virtual void OnContentAdded(DockContentEventArgs e)
        {
            ((EventHandler<DockContentEventArgs>)Events[ContentAddedEvent])?.Invoke(this, e);
        }

        protected virtual void OnContentRemoved(DockContentEventArgs e)
        {
            ((EventHandler<DockContentEventArgs>)Events[ContentRemovedEvent])?.Invoke(this, e);
        }
    }
}