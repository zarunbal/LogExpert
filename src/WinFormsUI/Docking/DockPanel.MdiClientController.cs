using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking.Win32;
using ScrollBars = WeifenLuo.WinFormsUI.Docking.Win32.ScrollBars;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        #region Private Fields

        private MdiClientController m_mdiClientController;

        #endregion

        #region Properties / Indexers

        private bool MdiClientExists => GetMdiClientController().MdiClient != null;

        #endregion

        #region Private Methods

        private MdiClientController GetMdiClientController()
        {
            if (m_mdiClientController == null)
            {
                m_mdiClientController = new MdiClientController();
                m_mdiClientController.HandleAssigned += MdiClientHandleAssigned;
                m_mdiClientController.MdiChildActivate += ParentFormMdiChildActivate;
                m_mdiClientController.Layout += MdiClient_Layout;
            }

            return m_mdiClientController;
        }

        private void ParentFormMdiChildActivate(object sender, EventArgs e)
        {
            if (GetMdiClientController().ParentForm == null)
            {
                return;
            }

            IDockContent content = GetMdiClientController().ParentForm.ActiveMdiChild as IDockContent;
            if (content == null)
            {
                return;
            }

            if (content.DockHandler.DockPanel == this && content.DockHandler.Pane != null)
            {
                content.DockHandler.Pane.ActiveContent = content;
            }
        }

        private void PerformMdiClientLayout()
        {
            if (GetMdiClientController().MdiClient != null)
            {
                GetMdiClientController().MdiClient.PerformLayout();
            }
        }

        private void ResumeMdiClientLayout(bool perform)
        {
            if (GetMdiClientController().MdiClient != null)
            {
                GetMdiClientController().MdiClient.ResumeLayout(perform);
            }
        }

        // Called when:
        // 1. DockPanel.DocumentStyle changed
        // 2. DockPanel.Visible changed
        // 3. MdiClientController.Handle assigned
        private void SetMdiClient()
        {
            MdiClientController controller = GetMdiClientController();

            if (DocumentStyle == DocumentStyle.DockingMdi)
            {
                controller.AutoScroll = false;
                controller.BorderStyle = BorderStyle.None;
                if (MdiClientExists)
                {
                    controller.MdiClient.Dock = DockStyle.Fill;
                }
            }
            else if (DocumentStyle == DocumentStyle.DockingSdi || DocumentStyle == DocumentStyle.DockingWindow)
            {
                controller.AutoScroll = true;
                controller.BorderStyle = BorderStyle.Fixed3D;
                if (MdiClientExists)
                {
                    controller.MdiClient.Dock = DockStyle.Fill;
                }
            }
            else if (DocumentStyle == DocumentStyle.SystemMdi)
            {
                controller.AutoScroll = true;
                controller.BorderStyle = BorderStyle.Fixed3D;
                if (controller.MdiClient != null)
                {
                    controller.MdiClient.Dock = DockStyle.None;
                    controller.MdiClient.Bounds = SystemMdiClientBounds;
                }
            }
        }

        private void SetMdiClientBounds(Rectangle bounds)
        {
            GetMdiClientController().MdiClient.Bounds = bounds;
        }

        private void SuspendMdiClientLayout()
        {
            if (GetMdiClientController().MdiClient != null)
            {
                GetMdiClientController().MdiClient.SuspendLayout();
            }
        }

        #endregion

        #region Nested type: MdiClientController

        // This class comes from Jacob Slusser's MdiClientController class:
        // http://www.codeproject.com/cs/miscctrl/mdiclientcontroller.asp
        private class MdiClientController : NativeWindow, IComponent, IDisposable
        {
            #region Private Fields

            private bool m_autoScroll = true;
            private BorderStyle m_borderStyle = BorderStyle.Fixed3D;
            private Form m_parentForm;
            private ISite m_site;

            #endregion

            #region Public Events

            public event EventHandler HandleAssigned;

            public event LayoutEventHandler Layout;

            public event EventHandler MdiChildActivate;

            public event PaintEventHandler Paint;

            #endregion

            #region Interface IComponent

            public event EventHandler Disposed;

            public ISite Site
            {
                get => m_site;
                set
                {
                    m_site = value;

                    if (m_site == null)
                    {
                        return;
                    }

                    // If the component is dropped onto a form during design-time,
                    // set the ParentForm property.
                    IDesignerHost host = value.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (host != null)
                    {
                        Form parent = host.RootComponent as Form;
                        if (parent != null)
                        {
                            ParentForm = parent;
                        }
                    }
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Properties / Indexers

            public bool AutoScroll
            {
                get => m_autoScroll;
                set
                {
                    // By default the MdiClient control scrolls. It can appear though that
                    // there are no scrollbars by turning them off when the non-client
                    // area is calculated. I decided to expose this method following
                    // the .NET vernacular of an AutoScroll property.
                    m_autoScroll = value;
                    if (MdiClient != null)
                    {
                        UpdateStyles();
                    }
                }
            }

            public BorderStyle BorderStyle
            {
                set
                {
                    // Error-check the enum.
                    if (!Enum.IsDefined(typeof(BorderStyle), value))
                    {
                        throw new InvalidEnumArgumentException();
                    }

                    m_borderStyle = value;

                    if (MdiClient == null)
                    {
                        return;
                    }

                    // This property can actually be visible in design-mode,
                    // but to keep it consistent with the others,
                    // prevent this from being show at design-time.
                    if (Site != null && Site.DesignMode)
                    {
                        return;
                    }

                    // There is no BorderStyle property exposed by the MdiClient class,
                    // but this can be controlled by Win32 functions. A Win32 ExStyle
                    // of WS_EX_CLIENTEDGE is equivalent to a Fixed3D border and a
                    // Style of WS_BORDER is equivalent to a FixedSingle border.

                    // This code is inspired Jason Dori's article:
                    // "Adding designable borders to user controls".
                    // http://www.codeproject.com/cs/miscctrl/CsAddingBorders.asp

                    // Get styles using Win32 calls
                    int style = NativeMethods.GetWindowLong(MdiClient.Handle, (int)GetWindowLongIndex.GWL_STYLE);
                    int exStyle =
                        NativeMethods.GetWindowLong(MdiClient.Handle, (int)GetWindowLongIndex.GWL_EXSTYLE);

                    // Add or remove style flags as necessary.
                    switch (m_borderStyle)
                    {
                        case BorderStyle.Fixed3D:
                            exStyle |= (int)WindowExStyles.WS_EX_CLIENTEDGE;
                            style &= ~(int)WindowStyles.WS_BORDER;
                            break;

                        case BorderStyle.FixedSingle:
                            exStyle &= ~(int)WindowExStyles.WS_EX_CLIENTEDGE;
                            style |= (int)WindowStyles.WS_BORDER;
                            break;

                        case BorderStyle.None:
                            style &= ~(int)WindowStyles.WS_BORDER;
                            exStyle &= ~(int)WindowExStyles.WS_EX_CLIENTEDGE;
                            break;
                    }

                    // Set the styles using Win32 calls
                    NativeMethods.SetWindowLong(MdiClient.Handle, (int)GetWindowLongIndex.GWL_STYLE, style);
                    NativeMethods.SetWindowLong(MdiClient.Handle, (int)GetWindowLongIndex.GWL_EXSTYLE, exStyle);

                    // Cause an update of the non-client area.
                    UpdateStyles();
                }
            }

            public MdiClient MdiClient { get; private set; }

            [Browsable(false)]
            public Form ParentForm
            {
                get => m_parentForm;
                set
                {
                    // If the ParentForm has previously been set,
                    // unwire events connected to the old parent.
                    if (m_parentForm != null)
                    {
                        m_parentForm.HandleCreated -= ParentFormHandleCreated;
                        m_parentForm.MdiChildActivate -= ParentFormMdiChildActivate;
                    }

                    m_parentForm = value;

                    if (m_parentForm == null)
                    {
                        return;
                    }

                    // If the parent form has not been created yet,
                    // wait to initialize the MDI client until it is.
                    if (m_parentForm.IsHandleCreated)
                    {
                        InitializeMdiClient();
                        RefreshProperties();
                    }
                    else
                    {
                        m_parentForm.HandleCreated += ParentFormHandleCreated;
                    }

                    m_parentForm.MdiChildActivate += ParentFormMdiChildActivate;
                }
            }

            #endregion

            #region Public Methods

            public void RenewMdiClient()
            {
                // Reinitialize the MdiClient and its properties.
                InitializeMdiClient();
                RefreshProperties();
            }

            #endregion

            #region Overrides

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case (int)Msgs.WM_NCCALCSIZE:
                        // If AutoScroll is set to false, hide the scrollbars when the control
                        // calculates its non-client area.
                        if (!AutoScroll)
                        {
                            NativeMethods.ShowScrollBar(m.HWnd, (int)ScrollBars.SB_BOTH, 0 /*false*/);
                        }

                        break;
                }

                base.WndProc(ref m);
            }

            #endregion

            #region Event handling Methods

            protected virtual void OnHandleAssigned(EventArgs e)
            {
                // Raise the HandleAssigned event.
                if (HandleAssigned != null)
                {
                    HandleAssigned(this, e);
                }
            }

            protected virtual void OnLayout(LayoutEventArgs e)
            {
                // Raise the Layout event
                if (Layout != null)
                {
                    Layout(this, e);
                }
            }

            protected virtual void OnMdiChildActivate(EventArgs e)
            {
                // Raise the MdiChildActivate event
                if (MdiChildActivate != null)
                {
                    MdiChildActivate(this, e);
                }
            }

            protected virtual void OnPaint(PaintEventArgs e)
            {
                // Raise the Paint event.
                if (Paint != null)
                {
                    Paint(this, e);
                }
            }

            #endregion

            #region Private Methods

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    lock (this)
                    {
                        if (Site != null && Site.Container != null)
                        {
                            Site.Container.Remove(this);
                        }

                        if (Disposed != null)
                        {
                            Disposed(this, EventArgs.Empty);
                        }
                    }
                }
            }

            private void InitializeMdiClient()
            {
                // If the mdiClient has previously been set, unwire events connected
                // to the old MDI.
                if (MdiClient != null)
                {
                    MdiClient.HandleDestroyed -= MdiClientHandleDestroyed;
                    MdiClient.Layout -= MdiClientLayout;
                }

                if (ParentForm == null)
                {
                    return;
                }

                // Get the MdiClient from the parent form.
                foreach (Control control in ParentForm.Controls)
                {
                    // If the form is an MDI container, it will contain an MdiClient control
                    // just as it would any other control.
                    MdiClient = control as MdiClient;
                    if (MdiClient == null)
                    {
                        continue;
                    }

                    // Assign the MdiClient Handle to the NativeWindow.
                    ReleaseHandle();
                    AssignHandle(MdiClient.Handle);

                    // Raise the HandleAssigned event.
                    OnHandleAssigned(EventArgs.Empty);

                    // Monitor the MdiClient for when its handle is destroyed.
                    MdiClient.HandleDestroyed += MdiClientHandleDestroyed;
                    MdiClient.Layout += MdiClientLayout;

                    break;
                }
            }

            private void MdiClientHandleDestroyed(object sender, EventArgs e)
            {
                // If the MdiClient handle has been released, drop the reference and
                // release the handle.
                if (MdiClient != null)
                {
                    MdiClient.HandleDestroyed -= MdiClientHandleDestroyed;
                    MdiClient = null;
                }

                ReleaseHandle();
            }

            private void MdiClientLayout(object sender, LayoutEventArgs e)
            {
                OnLayout(e);
            }

            private void ParentFormHandleCreated(object sender, EventArgs e)
            {
                // The form has been created, unwire the event, and initialize the MdiClient.
                m_parentForm.HandleCreated -= ParentFormHandleCreated;
                InitializeMdiClient();
                RefreshProperties();
            }

            private void ParentFormMdiChildActivate(object sender, EventArgs e)
            {
                OnMdiChildActivate(e);
            }

            private void RefreshProperties()
            {
                // Refresh all the properties
                BorderStyle = m_borderStyle;
                AutoScroll = m_autoScroll;
            }

            private void UpdateStyles()
            {
                // To show style changes, the non-client area must be repainted. Using the
                // control's Invalidate method does not affect the non-client area.
                // Instead use a Win32 call to signal the style has changed.
                NativeMethods.SetWindowPos(MdiClient.Handle, IntPtr.Zero, 0, 0, 0, 0,
                    FlagsSetWindowPos.SWP_NOACTIVATE |
                    FlagsSetWindowPos.SWP_NOMOVE |
                    FlagsSetWindowPos.SWP_NOSIZE |
                    FlagsSetWindowPos.SWP_NOZORDER |
                    FlagsSetWindowPos.SWP_NOOWNERZORDER |
                    FlagsSetWindowPos.SWP_FRAMECHANGED);
            }

            #endregion
        }

        #endregion

        internal Rectangle RectangleToMdiClient(Rectangle rect)
        {
            if (MdiClientExists)
            {
                return GetMdiClientController().MdiClient.RectangleToClient(rect);
            }

            return Rectangle.Empty;
        }
    }
}
