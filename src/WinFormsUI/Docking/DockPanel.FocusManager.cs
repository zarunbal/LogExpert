using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking.Win32;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal interface IContentFocusManager
    {
        #region Public Methods

        void Activate(IDockContent content);
        void AddToList(IDockContent content);
        void GiveUpFocus(IDockContent content);
        void RemoveFromList(IDockContent content);

        #endregion
    }

    partial class DockPanel
    {
        #region Static/Constants

        private static readonly object ActiveContentChangedEvent = new object();

        private static readonly object ActiveDocumentChangedEvent = new object();

        private static readonly object ActivePaneChangedEvent = new object();

        #endregion

        #region Public Events

        [LocalizedCategory("Category_PropertyChanged")]
        [LocalizedDescription("DockPanel_ActiveContentChanged_Description")]
        public event EventHandler ActiveContentChanged
        {
            add => Events.AddHandler(ActiveContentChangedEvent, value);
            remove => Events.RemoveHandler(ActiveContentChangedEvent, value);
        }

        [LocalizedCategory("Category_PropertyChanged")]
        [LocalizedDescription("DockPanel_ActiveDocumentChanged_Description")]
        public event EventHandler ActiveDocumentChanged
        {
            add => Events.AddHandler(ActiveDocumentChangedEvent, value);
            remove => Events.RemoveHandler(ActiveDocumentChangedEvent, value);
        }

        [LocalizedCategory("Category_PropertyChanged")]
        [LocalizedDescription("DockPanel_ActivePaneChanged_Description")]
        public event EventHandler ActivePaneChanged
        {
            add => Events.AddHandler(ActivePaneChangedEvent, value);
            remove => Events.RemoveHandler(ActivePaneChangedEvent, value);
        }

        #endregion

        #region Properties / Indexers

        [Browsable(false)]
        public IDockContent ActiveContent => FocusManager.ActiveContent;

        [Browsable(false)]
        public IDockContent ActiveDocument => FocusManager.ActiveDocument;

        [Browsable(false)]
        public DockPane ActiveDocumentPane => FocusManager.ActiveDocumentPane;

        [Browsable(false)]
        public DockPane ActivePane => FocusManager.ActivePane;

        internal IContentFocusManager ContentFocusManager => m_focusManager;

        private IFocusManager FocusManager => m_focusManager;

        #endregion

        #region Event handling Methods

        protected void OnActiveContentChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)Events[ActiveContentChangedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnActiveDocumentChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)Events[ActiveDocumentChangedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnActivePaneChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)Events[ActivePaneChangedEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region Nested type: FocusManagerImpl

        private class FocusManagerImpl : Component, IContentFocusManager, IFocusManager
        {
            #region Private Fields

            private readonly LocalWindowsHook.HookEventHandler m_hookEventHandler;

            private readonly LocalWindowsHook m_localWindowsHook;

            private int m_countSuspendFocusTracking;

            private bool m_disposed;

            #endregion

            #region Ctor

            public FocusManagerImpl(DockPanel dockPanel)
            {
                DockPanel = dockPanel;
                m_localWindowsHook = new LocalWindowsHook(HookType.WH_CALLWNDPROCRET);
                m_hookEventHandler = HookEventHandler;
                m_localWindowsHook.HookInvoked += m_hookEventHandler;
                m_localWindowsHook.Install();
            }

            #endregion

            #region Interface IContentFocusManager

            public void Activate(IDockContent content)
            {
                if (IsFocusTrackingSuspended)
                {
                    ContentActivating = content;
                    return;
                }

                if (content == null)
                {
                    return;
                }

                DockContentHandler handler = content.DockHandler;
                if (handler.Form.IsDisposed)
                {
                    return; // Should not reach here, but better than throwing an exception
                }

                if (ContentContains(content, handler.ActiveWindowHandle))
                {
                    NativeMethods.SetFocus(handler.ActiveWindowHandle);
                }

                if (!handler.Form.ContainsFocus)
                {
                    if (!handler.Form.SelectNextControl(handler.Form.ActiveControl, true, true, true, true))
                    {
// Since DockContent Form is not selectalbe, use Win32 SetFocus instead
                        NativeMethods.SetFocus(handler.Form.Handle);
                    }
                }
            }

            public void AddToList(IDockContent content)
            {
                if (ListContent.Contains(content) || IsInActiveList(content))
                {
                    return;
                }

                ListContent.Add(content);
            }

            public void GiveUpFocus(IDockContent content)
            {
                DockContentHandler handler = content.DockHandler;
                if (!handler.Form.ContainsFocus)
                {
                    return;
                }

                if (IsFocusTrackingSuspended)
                {
                    DockPanel.DummyControl.Focus();
                }

                if (LastActiveContent == content)
                {
                    IDockContent prev = handler.PreviousActive;
                    if (prev != null)
                    {
                        Activate(prev);
                    }
                    else if (ListContent.Count > 0)
                    {
                        Activate(ListContent[ListContent.Count - 1]);
                    }
                }
                else if (LastActiveContent != null)
                {
                    Activate(LastActiveContent);
                }
                else if (ListContent.Count > 0)
                {
                    Activate(ListContent[ListContent.Count - 1]);
                }
            }

            public void RemoveFromList(IDockContent content)
            {
                if (IsInActiveList(content))
                {
                    RemoveFromActiveList(content);
                }

                if (ListContent.Contains(content))
                {
                    ListContent.Remove(content);
                }
            }

            #endregion

            #region Interface IFocusManager

            public IDockContent ActiveContent { get; private set; }

            public IDockContent ActiveDocument { get; private set; }

            public DockPane ActiveDocumentPane { get; private set; }

            public DockPane ActivePane { get; private set; }

            public bool IsFocusTrackingSuspended => m_countSuspendFocusTracking != 0;

            public void ResumeFocusTracking()
            {
                if (m_countSuspendFocusTracking > 0)
                {
                    m_countSuspendFocusTracking--;
                }

                if (m_countSuspendFocusTracking == 0)
                {
                    if (ContentActivating != null)
                    {
                        Activate(ContentActivating);
                        ContentActivating = null;
                    }

                    m_localWindowsHook.HookInvoked += m_hookEventHandler;
                    if (!InRefreshActiveWindow)
                    {
                        RefreshActiveWindow();
                    }
                }
            }

            public void SuspendFocusTracking()
            {
                m_countSuspendFocusTracking++;
                m_localWindowsHook.HookInvoked -= m_hookEventHandler;
            }

            #endregion

            #region Properties / Indexers

            public DockPanel DockPanel { get; }

            private IDockContent ContentActivating { get; set; }

            private bool InRefreshActiveWindow { get; set; }

            private IDockContent LastActiveContent { get; set; }

            private List<IDockContent> ListContent { get; } = new List<IDockContent>();

            #endregion

            #region Overrides

            protected override void Dispose(bool disposing)
            {
                lock (this)
                {
                    if (!m_disposed && disposing)
                    {
                        m_localWindowsHook.Dispose();
                        m_disposed = true;
                    }

                    base.Dispose(disposing);
                }
            }

            #endregion

            #region Event raising Methods

            private void SetActiveDocument()
            {
                IDockContent value = ActiveDocumentPane == null ? null : ActiveDocumentPane.ActiveContent;

                if (ActiveDocument == value)
                {
                    return;
                }

                ActiveDocument = value;
            }

            private void SetActiveDocumentPane()
            {
                DockPane value = null;

                if (ActivePane != null && ActivePane.DockState == DockState.Document)
                {
                    value = ActivePane;
                }

                if (value == null && DockPanel.DockWindows != null)
                {
                    if (ActiveDocumentPane == null)
                    {
                        value = DockPanel.DockWindows[DockState.Document].DefaultPane;
                    }
                    else if (ActiveDocumentPane.DockPanel != DockPanel ||
                             ActiveDocumentPane.DockState != DockState.Document)
                    {
                        value = DockPanel.DockWindows[DockState.Document].DefaultPane;
                    }
                    else
                    {
                        value = ActiveDocumentPane;
                    }
                }

                if (ActiveDocumentPane == value)
                {
                    return;
                }

                if (ActiveDocumentPane != null)
                {
                    ActiveDocumentPane.SetIsActiveDocumentPane(false);
                }

                ActiveDocumentPane = value;

                if (ActiveDocumentPane != null)
                {
                    ActiveDocumentPane.SetIsActiveDocumentPane(true);
                }
            }

            #endregion

            #region Private Methods

            private void AddLastToActiveList(IDockContent content)
            {
                IDockContent last = LastActiveContent;
                if (last == content)
                {
                    return;
                }

                DockContentHandler handler = content.DockHandler;

                if (IsInActiveList(content))
                {
                    RemoveFromActiveList(content);
                }

                handler.PreviousActive = last;
                handler.NextActive = null;
                LastActiveContent = content;
                if (last != null)
                {
                    last.DockHandler.NextActive = LastActiveContent;
                }
            }

            private static bool ContentContains(IDockContent content, IntPtr hWnd)
            {
                Control control = FromChildHandle(hWnd);
                for (Control parent = control; parent != null; parent = parent.Parent)
                {
                    if (parent == content.DockHandler.Form)
                    {
                        return true;
                    }
                }

                return false;
            }

            private DockPane GetPaneFromHandle(IntPtr hWnd)
            {
                Control control = FromChildHandle(hWnd);

                IDockContent content = null;
                DockPane pane = null;
                for (; control != null; control = control.Parent)
                {
                    content = control as IDockContent;
                    if (content != null)
                    {
                        content.DockHandler.ActiveWindowHandle = hWnd;
                    }

                    if (content != null && content.DockHandler.DockPanel == DockPanel)
                    {
                        return content.DockHandler.Pane;
                    }

                    pane = control as DockPane;
                    if (pane != null && pane.DockPanel == DockPanel)
                    {
                        break;
                    }
                }

                return pane;
            }

            // Windows hook event handler
            private void HookEventHandler(object sender, HookEventArgs e)
            {
                Msgs msg = (Msgs)Marshal.ReadInt32(e.lParam, IntPtr.Size * 3);

                if (msg == Msgs.WM_KILLFOCUS)
                {
                    IntPtr wParam = Marshal.ReadIntPtr(e.lParam, IntPtr.Size * 2);
                    DockPane pane = GetPaneFromHandle(wParam);
                    if (pane == null)
                    {
                        RefreshActiveWindow();
                    }
                }
                else if (msg == Msgs.WM_SETFOCUS)
                {
                    RefreshActiveWindow();
                }
            }

            private bool IsInActiveList(IDockContent content)
            {
                return !(content.DockHandler.NextActive == null && LastActiveContent != content);
            }

            private void RefreshActiveWindow()
            {
                SuspendFocusTracking();
                InRefreshActiveWindow = true;

                DockPane oldActivePane = ActivePane;
                IDockContent oldActiveContent = ActiveContent;
                IDockContent oldActiveDocument = ActiveDocument;

                SetActivePane();
                SetActiveContent();
                SetActiveDocumentPane();
                SetActiveDocument();
                DockPanel.AutoHideWindow.RefreshActivePane();

                ResumeFocusTracking();
                InRefreshActiveWindow = false;

                if (oldActiveContent != ActiveContent)
                {
                    DockPanel.OnActiveContentChanged(EventArgs.Empty);
                }

                if (oldActiveDocument != ActiveDocument)
                {
                    DockPanel.OnActiveDocumentChanged(EventArgs.Empty);
                }

                if (oldActivePane != ActivePane)
                {
                    DockPanel.OnActivePaneChanged(EventArgs.Empty);
                }
            }

            private void RemoveFromActiveList(IDockContent content)
            {
                if (LastActiveContent == content)
                {
                    LastActiveContent = content.DockHandler.PreviousActive;
                }

                IDockContent prev = content.DockHandler.PreviousActive;
                IDockContent next = content.DockHandler.NextActive;
                if (prev != null)
                {
                    prev.DockHandler.NextActive = next;
                }

                if (next != null)
                {
                    next.DockHandler.PreviousActive = prev;
                }

                content.DockHandler.PreviousActive = null;
                content.DockHandler.NextActive = null;
            }

            private void SetActivePane()
            {
                DockPane value = GetPaneFromHandle(NativeMethods.GetFocus());
                if (ActivePane == value)
                {
                    return;
                }

                if (ActivePane != null)
                {
                    ActivePane.SetIsActivated(false);
                }

                ActivePane = value;

                if (ActivePane != null)
                {
                    ActivePane.SetIsActivated(true);
                }
            }

            #endregion

            #region Nested type: HookEventArgs

            private class HookEventArgs : EventArgs
            {
                #region Private Fields

                [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
                public int HookCode;

                public IntPtr lParam;

                [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
                public IntPtr wParam;

                #endregion
            }

            #endregion

            #region Nested type: LocalWindowsHook

            private class LocalWindowsHook : IDisposable
            {
                #region Delegates

                // Event delegate
                public delegate void HookEventHandler(object sender, HookEventArgs e);

                #endregion

                #region Private Fields

                private readonly NativeMethods.HookProc m_filterFunc;

                private readonly HookType m_hookType;

                // Internal properties
                private IntPtr m_hHook = IntPtr.Zero;

                #endregion

                #region Public Events

                // Event: HookInvoked 
                public event HookEventHandler HookInvoked;

                #endregion

                #region Ctor

                public LocalWindowsHook(HookType hook)
                {
                    m_hookType = hook;
                    m_filterFunc = CoreHookProc;
                }

                #endregion

                #region Interface IDisposable

                public void Dispose()
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }

                #endregion

                #region Public Methods

                // Default filter function
                public IntPtr CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
                {
                    if (code < 0)
                    {
                        return NativeMethods.CallNextHookEx(m_hHook, code, wParam, lParam);
                    }

                    // Let clients determine what to do
                    HookEventArgs e = new HookEventArgs();
                    e.HookCode = code;
                    e.wParam = wParam;
                    e.lParam = lParam;
                    OnHookInvoked(e);

                    // Yield to the next hook in the chain
                    return NativeMethods.CallNextHookEx(m_hHook, code, wParam, lParam);
                }

                // Install the hook
                public void Install()
                {
                    if (m_hHook != IntPtr.Zero)
                    {
                        Uninstall();
                    }

                    int threadId = NativeMethods.GetCurrentThreadId();
                    m_hHook = NativeMethods.SetWindowsHookEx(m_hookType, m_filterFunc, IntPtr.Zero, threadId);
                }

                // Uninstall the hook
                public void Uninstall()
                {
                    if (m_hHook != IntPtr.Zero)
                    {
                        NativeMethods.UnhookWindowsHookEx(m_hHook);
                        m_hHook = IntPtr.Zero;
                    }
                }

                #endregion

                #region Event handling Methods

                protected void OnHookInvoked(HookEventArgs e)
                {
                    if (HookInvoked != null)
                    {
                        HookInvoked(this, e);
                    }
                }

                #endregion

                #region Private Methods

                protected virtual void Dispose(bool disposing)
                {
                    Uninstall();
                }

                #endregion

                ~LocalWindowsHook()
                {
                    Dispose(false);
                }
            }

            #endregion

            internal void SetActiveContent()
            {
                IDockContent value = ActivePane == null ? null : ActivePane.ActiveContent;

                if (ActiveContent == value)
                {
                    return;
                }

                if (ActiveContent != null)
                {
                    ActiveContent.DockHandler.IsActivated = false;
                }

                ActiveContent = value;

                if (ActiveContent != null)
                {
                    ActiveContent.DockHandler.IsActivated = true;
                    if (!DockHelper.IsDockStateAutoHide(ActiveContent.DockHandler.DockState))
                    {
                        AddLastToActiveList(ActiveContent);
                    }
                }
            }
        }

        #endregion

        #region Nested type: IFocusManager

        private interface IFocusManager
        {
            #region Properties / Indexers

            IDockContent ActiveContent { get; }
            IDockContent ActiveDocument { get; }
            DockPane ActiveDocumentPane { get; }
            DockPane ActivePane { get; }

            bool IsFocusTrackingSuspended { get; }

            #endregion

            #region Public Methods

            void ResumeFocusTracking();

            void SuspendFocusTracking();

            #endregion
        }

        #endregion

        internal void SaveFocus()
        {
            DummyControl.Focus();
        }
    }
}
