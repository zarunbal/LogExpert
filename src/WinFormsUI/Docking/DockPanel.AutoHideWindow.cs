using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        #region Properties / Indexers

        internal Control AutoHideControl => AutoHideWindow;

        internal Rectangle AutoHideWindowRectangle
        {
            get
            {
                DockState state = AutoHideWindow.DockState;
                Rectangle rectDockArea = DockArea;
                if (ActiveAutoHideContent == null)
                {
                    return Rectangle.Empty;
                }

                if (Parent == null)
                {
                    return Rectangle.Empty;
                }

                Rectangle rect = Rectangle.Empty;
                double autoHideSize = ActiveAutoHideContent.DockHandler.AutoHidePortion;
                if (state == DockState.DockLeftAutoHide)
                {
                    if (autoHideSize < 1)
                    {
                        autoHideSize = rectDockArea.Width * autoHideSize;
                    }

                    if (autoHideSize > rectDockArea.Width - MeasurePane.MinSize)
                    {
                        autoHideSize = rectDockArea.Width - MeasurePane.MinSize;
                    }

                    rect.X = rectDockArea.X;
                    rect.Y = rectDockArea.Y;
                    rect.Width = (int)autoHideSize;
                    rect.Height = rectDockArea.Height;
                }
                else if (state == DockState.DockRightAutoHide)
                {
                    if (autoHideSize < 1)
                    {
                        autoHideSize = rectDockArea.Width * autoHideSize;
                    }

                    if (autoHideSize > rectDockArea.Width - MeasurePane.MinSize)
                    {
                        autoHideSize = rectDockArea.Width - MeasurePane.MinSize;
                    }

                    rect.X = rectDockArea.X + rectDockArea.Width - (int)autoHideSize;
                    rect.Y = rectDockArea.Y;
                    rect.Width = (int)autoHideSize;
                    rect.Height = rectDockArea.Height;
                }
                else if (state == DockState.DockTopAutoHide)
                {
                    if (autoHideSize < 1)
                    {
                        autoHideSize = rectDockArea.Height * autoHideSize;
                    }

                    if (autoHideSize > rectDockArea.Height - MeasurePane.MinSize)
                    {
                        autoHideSize = rectDockArea.Height - MeasurePane.MinSize;
                    }

                    rect.X = rectDockArea.X;
                    rect.Y = rectDockArea.Y;
                    rect.Width = rectDockArea.Width;
                    rect.Height = (int)autoHideSize;
                }
                else if (state == DockState.DockBottomAutoHide)
                {
                    if (autoHideSize < 1)
                    {
                        autoHideSize = rectDockArea.Height * autoHideSize;
                    }

                    if (autoHideSize > rectDockArea.Height - MeasurePane.MinSize)
                    {
                        autoHideSize = rectDockArea.Height - MeasurePane.MinSize;
                    }

                    rect.X = rectDockArea.X;
                    rect.Y = rectDockArea.Y + rectDockArea.Height - (int)autoHideSize;
                    rect.Width = rectDockArea.Width;
                    rect.Height = (int)autoHideSize;
                }

                return rect;
            }
        }

        private AutoHideWindowControl AutoHideWindow { get; }

        #endregion

        #region Nested type: AutoHideWindowControl

        private class AutoHideWindowControl : Panel, ISplitterDragSource
        {
            #region Static/Constants

            private const int ANIMATE_TIME = 100; // in mini-seconds

            #endregion

            #region Private Fields

            private readonly SplitterControl m_splitter;

            private readonly Timer m_timerMouseTrack;

            private IDockContent m_activeContent;

            private bool m_flagDragging;

            #endregion

            #region Ctor

            public AutoHideWindowControl(DockPanel dockPanel)
            {
                DockPanel = dockPanel;

                m_timerMouseTrack = new Timer();
                m_timerMouseTrack.Tick += TimerMouseTrack_Tick;

                Visible = false;
                m_splitter = new SplitterControl(this);
                Controls.Add(m_splitter);
            }

            #endregion

            #region Interface ISplitterDragSource

            Control IDragSource.DragControl => this;

            Rectangle ISplitterDragSource.DragLimitBounds
            {
                get
                {
                    Rectangle rectLimit = DockPanel.DockArea;

                    if ((this as ISplitterDragSource).IsVertical)
                    {
                        rectLimit.X += MeasurePane.MinSize;
                        rectLimit.Width -= 2 * MeasurePane.MinSize;
                    }
                    else
                    {
                        rectLimit.Y += MeasurePane.MinSize;
                        rectLimit.Height -= 2 * MeasurePane.MinSize;
                    }

                    return DockPanel.RectangleToScreen(rectLimit);
                }
            }

            bool ISplitterDragSource.IsVertical => DockState == DockState.DockLeftAutoHide || DockState == DockState.DockRightAutoHide;

            void ISplitterDragSource.BeginDrag(Rectangle rectSplitter)
            {
                FlagDragging = true;
            }

            void ISplitterDragSource.EndDrag()
            {
                FlagDragging = false;
            }

            void ISplitterDragSource.MoveSplitter(int offset)
            {
                Rectangle rectDockArea = DockPanel.DockArea;
                IDockContent content = ActiveContent;
                if (DockState == DockState.DockLeftAutoHide && rectDockArea.Width > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                    {
                        content.DockHandler.AutoHidePortion += offset / (double)rectDockArea.Width;
                    }
                    else
                    {
                        content.DockHandler.AutoHidePortion = Width + offset;
                    }
                }
                else if (DockState == DockState.DockRightAutoHide && rectDockArea.Width > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                    {
                        content.DockHandler.AutoHidePortion -= offset / (double)rectDockArea.Width;
                    }
                    else
                    {
                        content.DockHandler.AutoHidePortion = Width - offset;
                    }
                }
                else if (DockState == DockState.DockBottomAutoHide && rectDockArea.Height > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                    {
                        content.DockHandler.AutoHidePortion -= offset / (double)rectDockArea.Height;
                    }
                    else
                    {
                        content.DockHandler.AutoHidePortion = Height - offset;
                    }
                }
                else if (DockState == DockState.DockTopAutoHide && rectDockArea.Height > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                    {
                        content.DockHandler.AutoHidePortion += offset / (double)rectDockArea.Height;
                    }
                    else
                    {
                        content.DockHandler.AutoHidePortion = Height + offset;
                    }
                }
            }

            #endregion

            #region Properties / Indexers

            public IDockContent ActiveContent
            {
                get => m_activeContent;
                set
                {
                    if (value == m_activeContent)
                    {
                        return;
                    }

                    if (value != null)
                    {
                        if (!DockHelper.IsDockStateAutoHide(value.DockHandler.DockState) ||
                            value.DockHandler.DockPanel != DockPanel)
                        {
                            throw new InvalidOperationException(Strings.DockPanel_ActiveAutoHideContent_InvalidValue);
                        }
                    }

                    DockPanel.SuspendLayout();

                    if (m_activeContent != null)
                    {
                        if (m_activeContent.DockHandler.Form.ContainsFocus)
                        {
                            DockPanel.ContentFocusManager.GiveUpFocus(m_activeContent);
                        }

                        AnimateWindow(false);
                    }

                    m_activeContent = value;
                    SetActivePane();
                    if (ActivePane != null)
                    {
                        ActivePane.ActiveContent = m_activeContent;
                    }

                    if (m_activeContent != null)
                    {
                        AnimateWindow(true);
                    }

                    DockPanel.ResumeLayout();
                    DockPanel.RefreshAutoHideStrip();

                    SetTimerMouseTrack();
                }
            }

            public DockPane ActivePane { get; private set; }

            public DockPanel DockPanel { get; }

            public DockState DockState => ActiveContent == null ? DockState.Unknown : ActiveContent.DockHandler.DockState;

            protected virtual Rectangle DisplayingRectangle
            {
                get
                {
                    Rectangle rect = ClientRectangle;

                    // exclude the border and the splitter
                    if (DockState == DockState.DockBottomAutoHide)
                    {
                        rect.Y += 2 + Measures.SplitterSize;
                        rect.Height -= 2 + Measures.SplitterSize;
                    }
                    else if (DockState == DockState.DockRightAutoHide)
                    {
                        rect.X += 2 + Measures.SplitterSize;
                        rect.Width -= 2 + Measures.SplitterSize;
                    }
                    else if (DockState == DockState.DockTopAutoHide)
                    {
                        rect.Height -= 2 + Measures.SplitterSize;
                    }
                    else if (DockState == DockState.DockLeftAutoHide)
                    {
                        rect.Width -= 2 + Measures.SplitterSize;
                    }

                    return rect;
                }
            }

            internal bool FlagDragging
            {
                get => m_flagDragging;
                set
                {
                    if (m_flagDragging == value)
                    {
                        return;
                    }

                    m_flagDragging = value;
                    SetTimerMouseTrack();
                }
            }

            private bool FlagAnimate { get; set; } = true;

            #endregion

            #region Public Methods

            public void RefreshActiveContent()
            {
                if (ActiveContent == null)
                {
                    return;
                }

                if (!DockHelper.IsDockStateAutoHide(ActiveContent.DockHandler.DockState))
                {
                    FlagAnimate = false;
                    ActiveContent = null;
                    FlagAnimate = true;
                }
            }

            public void RefreshActivePane()
            {
                SetTimerMouseTrack();
            }

            #endregion

            #region Overrides

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    m_timerMouseTrack.Dispose();
                }

                base.Dispose(disposing);
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                DockPadding.All = 0;
                if (DockState == DockState.DockLeftAutoHide)
                {
                    DockPadding.Right = 2;
                    m_splitter.Dock = DockStyle.Right;
                }
                else if (DockState == DockState.DockRightAutoHide)
                {
                    DockPadding.Left = 2;
                    m_splitter.Dock = DockStyle.Left;
                }
                else if (DockState == DockState.DockTopAutoHide)
                {
                    DockPadding.Bottom = 2;
                    m_splitter.Dock = DockStyle.Bottom;
                }
                else if (DockState == DockState.DockBottomAutoHide)
                {
                    DockPadding.Top = 2;
                    m_splitter.Dock = DockStyle.Top;
                }

                Rectangle rectDisplaying = DisplayingRectangle;
                Rectangle rectHidden = new Rectangle(-rectDisplaying.Width, rectDisplaying.Y, rectDisplaying.Width,
                    rectDisplaying.Height);
                foreach (Control c in Controls)
                {
                    DockPane pane = c as DockPane;
                    if (pane == null)
                    {
                        continue;
                    }


                    if (pane == ActivePane)
                    {
                        pane.Bounds = rectDisplaying;
                    }
                    else
                    {
                        pane.Bounds = rectHidden;
                    }
                }

                base.OnLayout(levent);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                // Draw the border
                Graphics g = e.Graphics;

                if (DockState == DockState.DockBottomAutoHide)
                {
                    g.DrawLine(SystemPens.ControlLightLight, 0, 1, ClientRectangle.Right, 1);
                }
                else if (DockState == DockState.DockRightAutoHide)
                {
                    g.DrawLine(SystemPens.ControlLightLight, 1, 0, 1, ClientRectangle.Bottom);
                }
                else if (DockState == DockState.DockTopAutoHide)
                {
                    g.DrawLine(SystemPens.ControlDark, 0, ClientRectangle.Height - 2, ClientRectangle.Right,
                        ClientRectangle.Height - 2);
                    g.DrawLine(SystemPens.ControlDarkDark, 0, ClientRectangle.Height - 1, ClientRectangle.Right,
                        ClientRectangle.Height - 1);
                }
                else if (DockState == DockState.DockLeftAutoHide)
                {
                    g.DrawLine(SystemPens.ControlDark, ClientRectangle.Width - 2, 0, ClientRectangle.Width - 2,
                        ClientRectangle.Bottom);
                    g.DrawLine(SystemPens.ControlDarkDark, ClientRectangle.Width - 1, 0, ClientRectangle.Width - 1,
                        ClientRectangle.Bottom);
                }

                base.OnPaint(e);
            }

            #endregion

            #region Private Methods

            private void AnimateWindow(bool show)
            {
                if (!FlagAnimate && Visible != show)
                {
                    Visible = show;
                    return;
                }

                Parent.SuspendLayout();

                Rectangle rectSource = GetRectangle(!show);
                Rectangle rectTarget = GetRectangle(show);
                int dxLoc, dyLoc;
                int dWidth, dHeight;
                dxLoc = dyLoc = dWidth = dHeight = 0;
                if (DockState == DockState.DockTopAutoHide)
                {
                    dHeight = show ? 1 : -1;
                }
                else if (DockState == DockState.DockLeftAutoHide)
                {
                    dWidth = show ? 1 : -1;
                }
                else if (DockState == DockState.DockRightAutoHide)
                {
                    dxLoc = show ? -1 : 1;
                    dWidth = show ? 1 : -1;
                }
                else if (DockState == DockState.DockBottomAutoHide)
                {
                    dyLoc = show ? -1 : 1;
                    dHeight = show ? 1 : -1;
                }

                if (show)
                {
                    Bounds = DockPanel.GetAutoHideWindowBounds(new Rectangle(-rectTarget.Width, -rectTarget.Height,
                        rectTarget.Width, rectTarget.Height));
                    if (Visible == false)
                    {
                        Visible = true;
                    }

                    PerformLayout();
                }

                SuspendLayout();

                LayoutAnimateWindow(rectSource);
                if (Visible == false)
                {
                    Visible = true;
                }

                int speedFactor = 1;
                int totalPixels = rectSource.Width != rectTarget.Width
                    ? Math.Abs(rectSource.Width - rectTarget.Width)
                    : Math.Abs(rectSource.Height - rectTarget.Height);
                int remainPixels = totalPixels;
                DateTime startingTime = DateTime.Now;
                while (rectSource != rectTarget)
                {
                    DateTime startPerMove = DateTime.Now;

                    rectSource.X += dxLoc * speedFactor;
                    rectSource.Y += dyLoc * speedFactor;
                    rectSource.Width += dWidth * speedFactor;
                    rectSource.Height += dHeight * speedFactor;
                    if (Math.Sign(rectTarget.X - rectSource.X) != Math.Sign(dxLoc))
                    {
                        rectSource.X = rectTarget.X;
                    }

                    if (Math.Sign(rectTarget.Y - rectSource.Y) != Math.Sign(dyLoc))
                    {
                        rectSource.Y = rectTarget.Y;
                    }

                    if (Math.Sign(rectTarget.Width - rectSource.Width) != Math.Sign(dWidth))
                    {
                        rectSource.Width = rectTarget.Width;
                    }

                    if (Math.Sign(rectTarget.Height - rectSource.Height) != Math.Sign(dHeight))
                    {
                        rectSource.Height = rectTarget.Height;
                    }

                    LayoutAnimateWindow(rectSource);
                    if (Parent != null)
                    {
                        Parent.Update();
                    }

                    remainPixels -= speedFactor;

                    while (true)
                    {
                        TimeSpan time = new TimeSpan(0, 0, 0, 0, ANIMATE_TIME);
                        TimeSpan elapsedPerMove = DateTime.Now - startPerMove;
                        TimeSpan elapsedTime = DateTime.Now - startingTime;
                        if ((int)(time - elapsedTime).TotalMilliseconds <= 0)
                        {
                            speedFactor = remainPixels;
                            break;
                        }

                        speedFactor = remainPixels * (int)elapsedPerMove.TotalMilliseconds /
                                      (int)(time - elapsedTime).TotalMilliseconds;
                        if (speedFactor >= 1)
                        {
                            break;
                        }
                    }
                }

                ResumeLayout();
                Parent.ResumeLayout();
            }

            private Rectangle GetRectangle(bool show)
            {
                if (DockState == DockState.Unknown)
                {
                    return Rectangle.Empty;
                }

                Rectangle rect = DockPanel.AutoHideWindowRectangle;

                if (show)
                {
                    return rect;
                }

                if (DockState == DockState.DockLeftAutoHide)
                {
                    rect.Width = 0;
                }
                else if (DockState == DockState.DockRightAutoHide)
                {
                    rect.X += rect.Width;
                    rect.Width = 0;
                }
                else if (DockState == DockState.DockTopAutoHide)
                {
                    rect.Height = 0;
                }
                else
                {
                    rect.Y += rect.Height;
                    rect.Height = 0;
                }

                return rect;
            }

            private void LayoutAnimateWindow(Rectangle rect)
            {
                Bounds = DockPanel.GetAutoHideWindowBounds(rect);

                Rectangle rectClient = ClientRectangle;

                if (DockState == DockState.DockLeftAutoHide)
                {
                    ActivePane.Location = new Point(rectClient.Right - 2 - Measures.SplitterSize - ActivePane.Width,
                        ActivePane.Location.Y);
                }
                else if (DockState == DockState.DockTopAutoHide)
                {
                    ActivePane.Location = new Point(ActivePane.Location.X,
                        rectClient.Bottom - 2 - Measures.SplitterSize - ActivePane.Height);
                }
            }

            private void SetActivePane()
            {
                DockPane value = ActiveContent == null ? null : ActiveContent.DockHandler.Pane;

                if (value == ActivePane)
                {
                    return;
                }

                ActivePane = value;
            }

            private void SetTimerMouseTrack()
            {
                if (ActivePane == null || ActivePane.IsActivated || FlagDragging)
                {
                    m_timerMouseTrack.Enabled = false;
                    return;
                }

                // start the timer
                int hovertime = SystemInformation.MouseHoverTime;

                // assign a default value 400 in case of setting Timer.Interval invalid value exception
                if (hovertime <= 0)
                {
                    hovertime = 400;
                }

                m_timerMouseTrack.Interval = 2 * hovertime;
                m_timerMouseTrack.Enabled = true;
            }

            private void TimerMouseTrack_Tick(object sender, EventArgs e)
            {
                if (IsDisposed)
                {
                    return;
                }

                if (ActivePane == null || ActivePane.IsActivated)
                {
                    m_timerMouseTrack.Enabled = false;
                    return;
                }

                DockPane pane = ActivePane;
                Point ptMouseInAutoHideWindow = PointToClient(MousePosition);
                Point ptMouseInDockPanel = DockPanel.PointToClient(MousePosition);

                Rectangle rectTabStrip = DockPanel.GetTabStripRectangle(pane.DockState);

                if (!ClientRectangle.Contains(ptMouseInAutoHideWindow) && !rectTabStrip.Contains(ptMouseInDockPanel))
                {
                    ActiveContent = null;
                    m_timerMouseTrack.Enabled = false;
                }
            }

            #endregion

            #region Nested type: SplitterControl

            private class SplitterControl : SplitterBase
            {
                #region Ctor

                public SplitterControl(AutoHideWindowControl autoHideWindow)
                {
                    AutoHideWindow = autoHideWindow;
                }

                #endregion

                #region Properties / Indexers

                protected override int SplitterSize => Measures.SplitterSize;

                private AutoHideWindowControl AutoHideWindow { get; }

                #endregion

                #region Overrides

                protected override void StartDrag()
                {
                    AutoHideWindow.DockPanel.BeginDrag(AutoHideWindow, AutoHideWindow.RectangleToScreen(Bounds));
                }

                #endregion
            }

            #endregion
        }

        #endregion

        internal void RefreshActiveAutoHideContent()
        {
            AutoHideWindow.RefreshActiveContent();
        }

        internal Rectangle GetAutoHideWindowBounds(Rectangle rectAutoHideWindow)
        {
            if (DocumentStyle == DocumentStyle.SystemMdi ||
                DocumentStyle == DocumentStyle.DockingMdi)
            {
                return Parent == null
                    ? Rectangle.Empty
                    : Parent.RectangleToClient(RectangleToScreen(rectAutoHideWindow));
            }

            return rectAutoHideWindow;
        }

        internal void RefreshAutoHideStrip()
        {
            AutoHideStripControl.RefreshChanges();
        }
    }
}
