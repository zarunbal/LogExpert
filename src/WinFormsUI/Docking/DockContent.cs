using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace WeifenLuo.WinFormsUI.Docking
{
    public class DockContent : Form, IDockContent
    {
        #region Fields

        private static readonly object DockStateChangedEvent = new object();

        [Localizable(true)] [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockContent_TabText_Description")] [DefaultValue(null)] private string m_tabText = null;

        #endregion

        #region cTor

        public DockContent()
        {
            DockHandler = new DockContentHandler(this, GetPersistString);
            DockHandler.DockStateChanged += DockHandler_DockStateChanged;
            //Suggested as a fix by bensty regarding form resize
            ParentChanged += DockContent_ParentChanged;
        }

        #endregion

        #region Events

        [LocalizedCategory("Category_PropertyChanged")]
        [LocalizedDescription("Pane_DockStateChanged_Description")]
        public event EventHandler DockStateChanged
        {

            add => Events.AddHandler(DockStateChangedEvent, value);
            remove => Events.RemoveHandler(DockStateChangedEvent, value);
        }

        #endregion

        #region Properties

        [Browsable(false)]
        public DockContentHandler DockHandler { get; } = null;

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockContent_AllowEndUserDocking_Description")]
        [DefaultValue(true)]
        public bool AllowEndUserDocking
        {
            get => DockHandler.AllowEndUserDocking;
            set => DockHandler.AllowEndUserDocking = value;
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockContent_DockAreas_Description")]
        [DefaultValue(DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.DockBottom |
                      DockAreas.Document | DockAreas.Float)]
        public DockAreas DockAreas
        {
            get => DockHandler.DockAreas;
            set => DockHandler.DockAreas = value;
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockContent_AutoHidePortion_Description")]
        [DefaultValue(0.25)]
        public double AutoHidePortion
        {
            get => DockHandler.AutoHidePortion;
            set => DockHandler.AutoHidePortion = value;
        }

        public string TabText
        {
            get => m_tabText;
            set => DockHandler.TabText = m_tabText = value;
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockContent_CloseButton_Description")]
        [DefaultValue(true)]
        public bool CloseButton
        {
            get => DockHandler.CloseButton;
            set => DockHandler.CloseButton = value;
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockContent_CloseButtonVisible_Description")]
        [DefaultValue(true)]
        public bool CloseButtonVisible
        {
            get => DockHandler.CloseButtonVisible;
            set => DockHandler.CloseButtonVisible = value;
        }

        [Browsable(false)]
        public DockPanel DockPanel
        {
            get => DockHandler.DockPanel;
            set => DockHandler.DockPanel = value;
        }

        [Browsable(false)]
        public DockState DockState
        {
            get => DockHandler.DockState;
            set => DockHandler.DockState = value;
        }

        [Browsable(false)]
        public DockPane Pane
        {
            get => DockHandler.Pane;
            set => DockHandler.Pane = value;
        }

        [Browsable(false)]
        public bool IsHidden
        {
            get => DockHandler.IsHidden;
            set => DockHandler.IsHidden = value;
        }

        [Browsable(false)]
        public DockState VisibleState
        {
            get => DockHandler.VisibleState;
            set => DockHandler.VisibleState = value;
        }

        [Browsable(false)]
        public bool IsFloat
        {
            get => DockHandler.IsFloat;
            set => DockHandler.IsFloat = value;
        }

        [Browsable(false)]
        public DockPane PanelPane
        {
            get => DockHandler.PanelPane;
            set => DockHandler.PanelPane = value;
        }

        [Browsable(false)]
        public DockPane FloatPane
        {
            get => DockHandler.FloatPane;
            set => DockHandler.FloatPane = value;
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockContent_HideOnClose_Description")]
        [DefaultValue(false)]
        public bool HideOnClose
        {
            get => DockHandler.HideOnClose;
            set => DockHandler.HideOnClose = value;
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockContent_ShowHint_Description")]
        [DefaultValue(DockState.Unknown)]
        public DockState ShowHint
        {
            get => DockHandler.ShowHint;
            set => DockHandler.ShowHint = value;
        }

        [Browsable(false)]
        public bool IsActivated => DockHandler.IsActivated;

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockContent_TabPageContextMenu_Description")]
        [DefaultValue(null)]
        public ContextMenuStrip TabPageContextMenu
        {
            get => DockHandler.TabPageContextMenu;
            set => DockHandler.TabPageContextMenu = value;
        }

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockContent_TabPageContextMenuStrip_Description")]
        [DefaultValue(null)]
        public ContextMenuStrip TabPageContextMenuStrip
        {
            get => DockHandler.TabPageContextMenuStrip;
            set => DockHandler.TabPageContextMenuStrip = value;
        }

        [Localizable(true)]
        [Category("Appearance")]
        [LocalizedDescription("DockContent_ToolTipText_Description")]
        [DefaultValue(null)]
        public string ToolTipText
        {
            get => DockHandler.ToolTipText;
            set => DockHandler.ToolTipText = value;
        }

        #endregion

        #region Public methods

        public bool IsDockStateValid(DockState dockState)
        {
            return DockHandler.IsDockStateValid(dockState);
        }

        public new void Activate()
        {
            DockHandler.Activate();
        }

        public new void Hide()
        {
            DockHandler.Hide();
        }

        public new void Show()
        {
            DockHandler.Show();
        }

        public void Show(DockPanel dockPanel)
        {
            DockHandler.Show(dockPanel);
        }

        public void Show(DockPanel dockPanel, DockState dockState)
        {
            DockHandler.Show(dockPanel, dockState);
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters")]
        public void Show(DockPanel dockPanel, Rectangle floatWindowBounds)
        {
            DockHandler.Show(dockPanel, floatWindowBounds);
        }

        public void Show(DockPane pane, IDockContent beforeContent)
        {
            DockHandler.Show(pane, beforeContent);
        }

        public void Show(DockPane previousPane, DockAlignment alignment, double proportion)
        {
            DockHandler.Show(previousPane, alignment, proportion);
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters")]
        public void FloatAt(Rectangle floatWindowBounds)
        {
            DockHandler.FloatAt(floatWindowBounds);
        }

        public void DockTo(DockPane paneTo, DockStyle dockStyle, int contentIndex)
        {
            DockHandler.DockTo(paneTo, dockStyle, contentIndex);
        }

        public void DockTo(DockPanel panel, DockStyle dockStyle)
        {
            DockHandler.DockTo(panel, dockStyle);
        }

        #endregion

        #region Private Methods

        private bool ShouldSerializeTabText()
        {
            return m_tabText != null;
        }

        #endregion

        #region Events handler

        //Suggested as a fix by bensty regarding form resize
        private void DockContent_ParentChanged(object Sender, EventArgs e)
        {
            if (Parent != null)
            {
                Font = Parent.Font;
            }
        }

        private void DockHandler_DockStateChanged(object sender, EventArgs e)
        {
            OnDockStateChanged(e);
        }

        #endregion

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual string GetPersistString()
        {
            return GetType().ToString();
        }

        protected virtual void OnDockStateChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) Events[DockStateChangedEvent];
            handler?.Invoke(this, e);
        }

        #region IDockContent Members

        void IDockContent.OnActivated(EventArgs e)
        {
            OnActivated(e);
        }

        void IDockContent.OnDeactivate(EventArgs e)
        {
            OnDeactivate(e);
        }

        #endregion
    }
}