using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class VS2005DockPaneStrip : DockPaneStripBase
    {
        #region Static/Constants

        private const int _DocumentButtonGapBetween = 0;
        private const int _DocumentButtonGapBottom = 4;
        private const int _DocumentButtonGapRight = 3;
        private const int _DocumentButtonGapTop = 4;
        private const int _DocumentIconGapBottom = 2;
        private const int _DocumentIconGapLeft = 8;
        private const int _DocumentIconGapRight = 0;
        private const int _DocumentIconHeight = 16;
        private const int _DocumentIconWidth = 16;
        private const int _DocumentStripGapBottom = 1;

        private const int _DocumentStripGapTop = 0;
        private const int _DocumentTabGapLeft = 3;
        private const int _DocumentTabGapRight = 3;
        private const int _DocumentTabGapTop = 3;
        private const int _DocumentTabMaxWidth = 200;
        private const int _DocumentTextGapRight = 3;
        private const int _ToolWindowImageGapBottom = 1;
        private const int _ToolWindowImageGapLeft = 2;
        private const int _ToolWindowImageGapRight = 0;
        private const int _ToolWindowImageGapTop = 3;
        private const int _ToolWindowImageHeight = 16;
        private const int _ToolWindowImageWidth = 16;
        private const int _ToolWindowStripGapBottom = 1;
        private const int _ToolWindowStripGapLeft = 0;
        private const int _ToolWindowStripGapRight = 0;

        private const int _ToolWindowStripGapTop = 0;
        private const int _ToolWindowTabSeperatorGapBottom = 3;
        private const int _ToolWindowTabSeperatorGapTop = 3;
        private const int _ToolWindowTextGapRight = 3;

        private static Bitmap m_imageButtonClose;
        private static Bitmap m_imageButtonWindowList;
        private static Bitmap m_imageButtonWindowListOverflow;
        private static string m_toolTipClose;
        private static string m_toolTipSelect;

        #endregion

        #region Private Fields

        private readonly ToolTip m_toolTip;
        private Font m_boldFont;
        private InertButton m_buttonClose;
        private InertButton m_buttonWindowList;
        private bool m_closeButtonVisible;
        private bool m_documentTabsOverflow;
        private Font m_font;
        private int m_startDisplayingTab;

        #endregion

        #region Ctor

        public VS2005DockPaneStrip(DockPane pane)
            : base(pane)
        {
            SetStyle(ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            SuspendLayout();

            Components = new Container();
            m_toolTip = new ToolTip(Components);
            SelectMenu = new ContextMenuStrip(Components);

            ResumeLayout();
        }

        #endregion

        #region Properties / Indexers

        public Font TextFont => DockPane.DockPanel.Skin.DockPaneStripSkin.TextFont;

        private Font BoldFont
        {
            get
            {
                if (IsDisposed)
                {
                    return null;
                }

                if (m_boldFont == null)
                {
                    m_font = TextFont;
                    m_boldFont = new Font(TextFont, FontStyle.Bold);
                }
                else if (m_font != TextFont)
                {
                    m_boldFont.Dispose();
                    m_font = TextFont;
                    m_boldFont = new Font(TextFont, FontStyle.Bold);
                }

                return m_boldFont;
            }
        }

        private InertButton ButtonClose
        {
            get
            {
                if (m_buttonClose == null)
                {
                    m_buttonClose = new InertButton(ImageButtonClose, ImageButtonClose);
                    m_toolTip.SetToolTip(m_buttonClose, ToolTipClose);
                    m_buttonClose.Click += Close_Click;
                    Controls.Add(m_buttonClose);
                }

                return m_buttonClose;
            }
        }

        private InertButton ButtonWindowList
        {
            get
            {
                if (m_buttonWindowList == null)
                {
                    m_buttonWindowList = new InertButton(ImageButtonWindowList, ImageButtonWindowListOverflow);
                    m_toolTip.SetToolTip(m_buttonWindowList, ToolTipSelect);
                    m_buttonWindowList.Click += WindowList_Click;
                    Controls.Add(m_buttonWindowList);
                }

                return m_buttonWindowList;
            }
        }

        private IContainer Components { get; }

        private static int DocumentButtonGapBetween => _DocumentButtonGapBetween;

        private static int DocumentButtonGapBottom => _DocumentButtonGapBottom;

        private static int DocumentButtonGapRight => _DocumentButtonGapRight;

        private static int DocumentButtonGapTop => _DocumentButtonGapTop;

        private static int DocumentIconGapBottom => _DocumentIconGapBottom;

        private static int DocumentIconGapLeft => _DocumentIconGapLeft;

        private static int DocumentIconGapRight => _DocumentIconGapRight;

        private static int DocumentIconHeight => _DocumentIconHeight;

        private static int DocumentIconWidth => _DocumentIconWidth;

        private static int DocumentStripGapBottom => _DocumentStripGapBottom;

        private static int DocumentStripGapTop => _DocumentStripGapTop;

        private static int DocumentTabGapLeft => _DocumentTabGapLeft;

        private static int DocumentTabGapRight => _DocumentTabGapRight;

        private static int DocumentTabGapTop => _DocumentTabGapTop;

        private static int DocumentTabMaxWidth => _DocumentTabMaxWidth;

        private bool DocumentTabsOverflow
        {
            set
            {
                if (m_documentTabsOverflow == value)
                {
                    return;
                }

                m_documentTabsOverflow = value;
                if (value)
                {
                    ButtonWindowList.ImageCategory = 1;
                }
                else
                {
                    ButtonWindowList.ImageCategory = 0;
                }
            }
        }

        private TextFormatFlags DocumentTextFormat
        {
            get
            {
                TextFormatFlags textFormat = TextFormatFlags.EndEllipsis |
                                             TextFormatFlags.SingleLine |
                                             TextFormatFlags.VerticalCenter |
                                             TextFormatFlags.HorizontalCenter;
                if (RightToLeft == RightToLeft.Yes)
                {
                    return textFormat | TextFormatFlags.RightToLeft;
                }

                return textFormat;
            }
        }

        private static int DocumentTextGapRight => _DocumentTextGapRight;

        private int EndDisplayingTab { get; set; }

        private int FirstDisplayingTab { get; set; }

        private static GraphicsPath GraphicsPath => VS2005AutoHideStrip.GraphicsPath;

        private static Bitmap ImageButtonClose
        {
            get
            {
                if (m_imageButtonClose == null)
                {
                    m_imageButtonClose = Resources.DockPane_Close;
                }

                return m_imageButtonClose;
            }
        }

        private static Bitmap ImageButtonWindowList
        {
            get
            {
                if (m_imageButtonWindowList == null)
                {
                    m_imageButtonWindowList = Resources.DockPane_Option;
                }

                return m_imageButtonWindowList;
            }
        }

        private static Bitmap ImageButtonWindowListOverflow
        {
            get
            {
                if (m_imageButtonWindowListOverflow == null)
                {
                    m_imageButtonWindowListOverflow = Resources.DockPane_OptionOverflow;
                }

                return m_imageButtonWindowListOverflow;
            }
        }

        private static Pen PenDocumentTabActiveBorder => SystemPens.ControlDarkDark;

        private static Pen PenDocumentTabInactiveBorder => SystemPens.GrayText;

        private static Pen PenToolWindowTabBorder => SystemPens.GrayText;

        private ContextMenuStrip SelectMenu { get; }

        private int StartDisplayingTab
        {
            get => m_startDisplayingTab;
            set
            {
                m_startDisplayingTab = value;
                Invalidate();
            }
        }

        private Rectangle TabsRectangle
        {
            get
            {
                if (Appearance == DockPane.AppearanceStyle.ToolWindow)
                {
                    return TabStripRectangle;
                }

                Rectangle rectWindow = TabStripRectangle;
                int x = rectWindow.X;
                int y = rectWindow.Y;
                int width = rectWindow.Width;
                int height = rectWindow.Height;

                x += DocumentTabGapLeft;
                width -= DocumentTabGapLeft +
                         DocumentTabGapRight +
                         DocumentButtonGapRight +
                         ButtonClose.Width +
                         ButtonWindowList.Width +
                         2 * DocumentButtonGapBetween;

                return new Rectangle(x, y, width, height);
            }
        }

        private Rectangle TabStripRectangle
        {
            get
            {
                if (Appearance == DockPane.AppearanceStyle.Document)
                {
                    return TabStripRectangle_Document;
                }

                return TabStripRectangle_ToolWindow;
            }
        }

        private Rectangle TabStripRectangle_Document
        {
            get
            {
                Rectangle rect = ClientRectangle;
                return new Rectangle(rect.X, rect.Top + DocumentStripGapTop, rect.Width,
                    rect.Height - DocumentStripGapTop - ToolWindowStripGapBottom);
            }
        }

        private Rectangle TabStripRectangle_ToolWindow
        {
            get
            {
                Rectangle rect = ClientRectangle;
                return new Rectangle(rect.X, rect.Top + ToolWindowStripGapTop, rect.Width,
                    rect.Height - ToolWindowStripGapTop - ToolWindowStripGapBottom);
            }
        }

        private static string ToolTipClose
        {
            get
            {
                if (m_toolTipClose == null)
                {
                    m_toolTipClose = Strings.DockPaneStrip_ToolTipClose;
                }

                return m_toolTipClose;
            }
        }

        private static string ToolTipSelect
        {
            get
            {
                if (m_toolTipSelect == null)
                {
                    m_toolTipSelect = Strings.DockPaneStrip_ToolTipWindowList;
                }

                return m_toolTipSelect;
            }
        }

        private static int ToolWindowImageGapBottom => _ToolWindowImageGapBottom;

        private static int ToolWindowImageGapLeft => _ToolWindowImageGapLeft;

        private static int ToolWindowImageGapRight => _ToolWindowImageGapRight;

        private static int ToolWindowImageGapTop => _ToolWindowImageGapTop;

        private static int ToolWindowImageHeight => _ToolWindowImageHeight;

        private static int ToolWindowImageWidth => _ToolWindowImageWidth;

        private static int ToolWindowStripGapBottom => _ToolWindowStripGapBottom;

        private static int ToolWindowStripGapLeft => _ToolWindowStripGapLeft;

        private static int ToolWindowStripGapRight => _ToolWindowStripGapRight;

        private static int ToolWindowStripGapTop => _ToolWindowStripGapTop;

        private static int ToolWindowTabSeperatorGapBottom => _ToolWindowTabSeperatorGapBottom;

        private static int ToolWindowTabSeperatorGapTop => _ToolWindowTabSeperatorGapTop;

        private TextFormatFlags ToolWindowTextFormat
        {
            get
            {
                TextFormatFlags textFormat = TextFormatFlags.EndEllipsis |
                                             TextFormatFlags.HorizontalCenter |
                                             TextFormatFlags.SingleLine |
                                             TextFormatFlags.VerticalCenter;
                if (RightToLeft == RightToLeft.Yes)
                {
                    return textFormat | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
                }

                return textFormat;
            }
        }

        private static int ToolWindowTextGapRight => _ToolWindowTextGapRight;

        #endregion

        #region Overrides

        protected internal override Tab CreateTab(IDockContent content)
        {
            return new TabVS2005(content);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Components.Dispose();
                if (m_boldFont != null)
                {
                    m_boldFont.Dispose();
                    m_boldFont = null;
                }
            }

            base.Dispose(disposing);
        }

        protected internal override void EnsureTabVisible(IDockContent content)
        {
            if (Appearance != DockPane.AppearanceStyle.Document || !Tabs.Contains(content))
            {
                return;
            }

            CalculateTabs();
            EnsureDocumentTabVisible(content, true);
        }

        protected internal override GraphicsPath GetOutline(int index)
        {
            if (Appearance == DockPane.AppearanceStyle.Document)
            {
                return GetOutline_Document(index);
            }

            return GetOutline_ToolWindow(index);
        }

        protected internal override int HitTest(Point ptMouse)
        {
            if (!TabsRectangle.Contains(ptMouse))
            {
                return -1;
            }

            foreach (Tab tab in Tabs)
            {
                GraphicsPath path = GetTabOutline(tab, true, false);
                if (path.IsVisible(ptMouse))
                {
                    return Tabs.IndexOf(tab);
                }
            }

            return -1;
        }

        protected internal override int MeasureHeight()
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
            {
                return MeasureHeight_ToolWindow();
            }

            return MeasureHeight_Document();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (Appearance == DockPane.AppearanceStyle.Document)
            {
                LayoutButtons();
                OnRefreshChanges();
            }

            base.OnLayout(levent);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            int index = HitTest(PointToClient(MousePosition));
            string toolTip = string.Empty;

            base.OnMouseHover(e);

            if (index != -1)
            {
                TabVS2005 tab = Tabs[index] as TabVS2005;
                if (!string.IsNullOrEmpty(tab.Content.DockHandler.ToolTipText))
                {
                    toolTip = tab.Content.DockHandler.ToolTipText;
                }
                else if (tab.MaxWidth > tab.TabWidth)
                {
                    toolTip = tab.Content.DockHandler.TabText;
                }
            }

            if (m_toolTip.GetToolTip(this) != toolTip)
            {
                m_toolTip.Active = false;
                m_toolTip.SetToolTip(this, toolTip);
                m_toolTip.Active = true;
            }

            // requires further tracking of mouse hover behavior,
            ResetMouseEventArgs();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = TabsRectangle;

            if (Appearance == DockPane.AppearanceStyle.Document)
            {
                rect.X -= DocumentTabGapLeft;

                // Add these values back in so that the DockStrip color is drawn
                // beneath the close button and window list button.
                rect.Width += DocumentTabGapLeft +
                              DocumentTabGapRight +
                              DocumentButtonGapRight +
                              ButtonClose.Width +
                              ButtonWindowList.Width;

                // It is possible depending on the DockPanel DocumentStyle to have
                // a Document without a DockStrip.
                if (rect.Width > 0 && rect.Height > 0)
                {
                    Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.DockStripGradient
                        .StartColor;
                    Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.DockStripGradient
                        .EndColor;
                    LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient
                        .DockStripGradient.LinearGradientMode;
                    using (LinearGradientBrush brush = new LinearGradientBrush(rect, startColor, endColor, gradientMode)
                    )
                    {
                        e.Graphics.FillRectangle(brush, rect);
                    }
                }
            }
            else
            {
                Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient
                    .StartColor;
                Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient
                    .EndColor;
                LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient
                    .DockStripGradient.LinearGradientMode;
                using (LinearGradientBrush brush = new LinearGradientBrush(rect, startColor, endColor, gradientMode))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            }

            base.OnPaint(e);
            CalculateTabs();
            if (Appearance == DockPane.AppearanceStyle.Document && DockPane.ActiveContent != null)
            {
                if (EnsureDocumentTabVisible(DockPane.ActiveContent, false))
                {
                    CalculateTabs();
                }
            }

            DrawTabStrip(e.Graphics);
        }

        protected override void OnRefreshChanges()
        {
            SetInertButtons();
            Invalidate();
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            PerformLayout();
        }

        #endregion

        #region Event raising Methods

        private bool CalculateDocumentTab(Rectangle rectTabStrip, ref int x, int index)
        {
            bool overflow = false;

            TabVS2005 tab = Tabs[index] as TabVS2005;
            tab.MaxWidth = GetMaxTabWidth(index);
            int width = Math.Min(tab.MaxWidth, DocumentTabMaxWidth);
            if (x + width < rectTabStrip.Right || index == StartDisplayingTab)
            {
                tab.TabX = x;
                tab.TabWidth = width;
                EndDisplayingTab = index;
            }
            else
            {
                tab.TabX = 0;
                tab.TabWidth = 0;
                overflow = true;
            }

            x += width;

            return overflow;
        }

        /// <summary>
        ///     Calculate which tabs are displayed and in what order.
        /// </summary>
        private void CalculateTabs_Document()
        {
            if (m_startDisplayingTab >= Tabs.Count)
            {
                m_startDisplayingTab = 0;
            }

            Rectangle rectTabStrip = TabsRectangle;

            int x = rectTabStrip.X + rectTabStrip.Height / 2;
            bool overflow = false;

            // Originally all new documents that were considered overflow
            // (not enough pane strip space to show all tabs) were added to
            // the far left (assuming not right to left) and the tabs on the
            // right were dropped from view. If StartDisplayingTab is not 0
            // then we are dealing with making sure a specific tab is kept in focus.
            if (m_startDisplayingTab > 0)
            {
                int tempX = x;
                TabVS2005 tab = Tabs[m_startDisplayingTab] as TabVS2005;
                tab.MaxWidth = GetMaxTabWidth(m_startDisplayingTab);

                // Add the active tab and tabs to the left
                for (int i = StartDisplayingTab; i >= 0; i--)
                {
                    CalculateDocumentTab(rectTabStrip, ref tempX, i);
                }

                // Store which tab is the first one displayed so that it
                // will be drawn correctly (without part of the tab cut off)
                FirstDisplayingTab = EndDisplayingTab;

                tempX = x; // Reset X location because we are starting over

                // Start with the first tab displayed - name is a little misleading.
                // Loop through each tab and set its location. If there is not enough
                // room for all of them overflow will be returned.
                for (int i = EndDisplayingTab; i < Tabs.Count; i++)
                {
                    overflow = CalculateDocumentTab(rectTabStrip, ref tempX, i);
                }

                // If not all tabs are shown then we have an overflow.
                if (FirstDisplayingTab != 0)
                {
                    overflow = true;
                }
            }
            else
            {
                for (int i = StartDisplayingTab; i < Tabs.Count; i++)
                {
                    overflow = CalculateDocumentTab(rectTabStrip, ref x, i);
                }

                for (int i = 0; i < StartDisplayingTab; i++)
                {
                    overflow = CalculateDocumentTab(rectTabStrip, ref x, i);
                }

                FirstDisplayingTab = StartDisplayingTab;
            }

            if (!overflow)
            {
                m_startDisplayingTab = 0;
                FirstDisplayingTab = 0;
                x = rectTabStrip.X + rectTabStrip.Height / 2;
                foreach (TabVS2005 tab in Tabs)
                {
                    tab.TabX = x;
                    x += tab.TabWidth;
                }
            }

            DocumentTabsOverflow = overflow;
        }

        private void DrawTab_Document(Graphics g, TabVS2005 tab, Rectangle rect)
        {
            if (tab.TabWidth == 0)
            {
                return;
            }

            Rectangle rectIcon = new Rectangle(
                rect.X + DocumentIconGapLeft,
                rect.Y + rect.Height - 1 - DocumentIconGapBottom - DocumentIconHeight,
                DocumentIconWidth, DocumentIconHeight);
            Rectangle rectText = rectIcon;
            if (DockPane.DockPanel.ShowDocumentIcon)
            {
                rectText.X += rectIcon.Width + DocumentIconGapRight;
                rectText.Y = rect.Y;
                rectText.Width = rect.Width - rectIcon.Width - DocumentIconGapLeft -
                                 DocumentIconGapRight - DocumentTextGapRight;
                rectText.Height = rect.Height;
            }
            else
            {
                rectText.Width = rect.Width - DocumentIconGapLeft - DocumentTextGapRight;
            }

            Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
            Rectangle rectBack = DrawHelper.RtlTransform(this, rect);
            rectBack.Width += rect.X;
            rectBack.X = 0;

            rectText = DrawHelper.RtlTransform(this, rectText);
            rectIcon = DrawHelper.RtlTransform(this, rectIcon);
            GraphicsPath path = GetTabOutline(tab, true, false);
            if (DockPane.ActiveContent == tab.Content)
            {
                Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient
                    .StartColor;
                Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.EndColor;
                LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient
                    .ActiveTabGradient.LinearGradientMode;
                g.FillPath(new LinearGradientBrush(rectBack, startColor, endColor, gradientMode), path);
                g.DrawPath(PenDocumentTabActiveBorder, path);

                Color textColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient
                    .TextColor;
                if (DockPane.IsActiveDocumentPane)
                {
                    TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, BoldFont, rectText, textColor,
                        DocumentTextFormat);
                }
                else
                {
                    TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, textColor,
                        DocumentTextFormat);
                }
            }
            else
            {
                Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient
                    .StartColor;
                Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient
                    .EndColor;
                LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient
                    .InactiveTabGradient.LinearGradientMode;
                g.FillPath(new LinearGradientBrush(rectBack, startColor, endColor, gradientMode), path);
                g.DrawPath(PenDocumentTabInactiveBorder, path);

                Color textColor = DockPane.DockPanel.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient
                    .TextColor;
                TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, textColor,
                    DocumentTextFormat);
            }

            if (rectTab.Contains(rectIcon) && DockPane.DockPanel.ShowDocumentIcon)
            {
                g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
            }
        }

        private void DrawTabStrip_Document(Graphics g)
        {
            int count = Tabs.Count;
            if (count == 0)
            {
                return;
            }

            Rectangle rectTabStrip = TabStripRectangle;

            // Draw the tabs
            Rectangle rectTabOnly = TabsRectangle;
            Rectangle rectTab = Rectangle.Empty;
            TabVS2005 tabActive = null;
            g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
            for (int i = 0; i < count; i++)
            {
                rectTab = GetTabRectangle(i);
                if (Tabs[i].Content == DockPane.ActiveContent)
                {
                    tabActive = Tabs[i] as TabVS2005;
                    continue;
                }

                if (rectTab.IntersectsWith(rectTabOnly))
                {
                    DrawTab(g, Tabs[i] as TabVS2005, rectTab);
                }
            }

            g.SetClip(rectTabStrip);

            if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
            {
                g.DrawLine(PenDocumentTabActiveBorder, rectTabStrip.Left, rectTabStrip.Top + 1,
                    rectTabStrip.Right, rectTabStrip.Top + 1);
            }
            else
            {
                g.DrawLine(PenDocumentTabActiveBorder, rectTabStrip.Left, rectTabStrip.Bottom - 1,
                    rectTabStrip.Right, rectTabStrip.Bottom - 1);
            }

            g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
            if (tabActive != null)
            {
                rectTab = GetTabRectangle(Tabs.IndexOf(tabActive));
                if (rectTab.IntersectsWith(rectTabOnly))
                {
                    DrawTab(g, tabActive, rectTab);
                }
            }
        }

        private bool EnsureDocumentTabVisible(IDockContent content, bool repaint)
        {
            int index = Tabs.IndexOf(content);
            TabVS2005 tab = Tabs[index] as TabVS2005;
            if (tab.TabWidth != 0)
            {
                return false;
            }

            StartDisplayingTab = index;
            if (repaint)
            {
                Invalidate();
            }

            return true;
        }

        private int GetMaxTabWidth_Document(int index)
        {
            IDockContent content = Tabs[index].Content;

            int height = GetTabRectangle_Document(index).Height;

            Size sizeText = TextRenderer.MeasureText(content.DockHandler.TabText, BoldFont,
                new Size(DocumentTabMaxWidth, height), DocumentTextFormat);

            if (DockPane.DockPanel.ShowDocumentIcon)
            {
                return sizeText.Width + DocumentIconWidth + DocumentIconGapLeft + DocumentIconGapRight +
                       DocumentTextGapRight;
            }

            return sizeText.Width + DocumentIconGapLeft + DocumentTextGapRight;
        }

        private GraphicsPath GetOutline_Document(int index)
        {
            Rectangle rectTab = GetTabRectangle(index);
            rectTab.X -= rectTab.Height / 2;
            rectTab.Intersect(TabsRectangle);
            rectTab = RectangleToScreen(DrawHelper.RtlTransform(this, rectTab));
            Rectangle rectPaneClient = DockPane.RectangleToScreen(DockPane.ClientRectangle);

            GraphicsPath path = new GraphicsPath();
            GraphicsPath pathTab = GetTabOutline_Document(Tabs[index], true, true, true);
            path.AddPath(pathTab, true);

            if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
            {
                path.AddLine(rectTab.Right, rectTab.Top, rectPaneClient.Right, rectTab.Top);
                path.AddLine(rectPaneClient.Right, rectTab.Top, rectPaneClient.Right, rectPaneClient.Top);
                path.AddLine(rectPaneClient.Right, rectPaneClient.Top, rectPaneClient.Left, rectPaneClient.Top);
                path.AddLine(rectPaneClient.Left, rectPaneClient.Top, rectPaneClient.Left, rectTab.Top);
                path.AddLine(rectPaneClient.Left, rectTab.Top, rectTab.Right, rectTab.Top);
            }
            else
            {
                path.AddLine(rectTab.Right, rectTab.Bottom, rectPaneClient.Right, rectTab.Bottom);
                path.AddLine(rectPaneClient.Right, rectTab.Bottom, rectPaneClient.Right, rectPaneClient.Bottom);
                path.AddLine(rectPaneClient.Right, rectPaneClient.Bottom, rectPaneClient.Left, rectPaneClient.Bottom);
                path.AddLine(rectPaneClient.Left, rectPaneClient.Bottom, rectPaneClient.Left, rectTab.Bottom);
                path.AddLine(rectPaneClient.Left, rectTab.Bottom, rectTab.Right, rectTab.Bottom);
            }

            return path;
        }

        private GraphicsPath GetTabOutline_Document(Tab tab, bool rtlTransform, bool toScreen, bool full)
        {
            int curveSize = 6;

            GraphicsPath.Reset();
            Rectangle rect = GetTabRectangle(Tabs.IndexOf(tab));
            if (rtlTransform)
            {
                rect = DrawHelper.RtlTransform(this, rect);
            }

            if (toScreen)
            {
                rect = RectangleToScreen(rect);
            }

            // Draws the full angle piece for active content (or first tab)
            if (tab.Content == DockPane.ActiveContent || full || Tabs.IndexOf(tab) == FirstDisplayingTab)
            {
                if (RightToLeft == RightToLeft.Yes)
                {
                    if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        // For some reason the next line draws a line that is not hidden like it is when drawing the tab strip on top.
                        // It is not needed so it has been commented out.
                        // GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right + rect.Height / 2, rect.Bottom);
                        GraphicsPath.AddLine(rect.Right + rect.Height / 2, rect.Top,
                            rect.Right - rect.Height / 2 + curveSize / 2, rect.Bottom - curveSize / 2);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right + rect.Height / 2, rect.Bottom);
                        GraphicsPath.AddLine(rect.Right + rect.Height / 2, rect.Bottom,
                            rect.Right - rect.Height / 2 + curveSize / 2, rect.Top + curveSize / 2);
                    }
                }
                else
                {
                    if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        // For some reason the next line draws a line that is not hidden like it is when drawing the tab strip on top.
                        // It is not needed so it has been commented out.
                        // GraphicsPath.AddLine(rect.Left, rect.Top, rect.Left - rect.Height / 2, rect.Top);
                        GraphicsPath.AddLine(rect.Left - rect.Height / 2, rect.Top,
                            rect.Left + rect.Height / 2 - curveSize / 2, rect.Bottom - curveSize / 2);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Bottom, rect.Left - rect.Height / 2, rect.Bottom);
                        GraphicsPath.AddLine(rect.Left - rect.Height / 2, rect.Bottom,
                            rect.Left + rect.Height / 2 - curveSize / 2, rect.Top + curveSize / 2);
                    }
                }
            }

// Draws the partial angle for non-active content
            else
            {
                if (RightToLeft == RightToLeft.Yes)
                {
                    if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Top, rect.Right, rect.Top + rect.Height / 2);
                        GraphicsPath.AddLine(rect.Right, rect.Top + rect.Height / 2,
                            rect.Right - rect.Height / 2 + curveSize / 2, rect.Bottom - curveSize / 2);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom - rect.Height / 2);
                        GraphicsPath.AddLine(rect.Right, rect.Bottom - rect.Height / 2,
                            rect.Right - rect.Height / 2 + curveSize / 2, rect.Top + curveSize / 2);
                    }
                }
                else
                {
                    if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Top, rect.Left, rect.Top + rect.Height / 2);
                        GraphicsPath.AddLine(rect.Left, rect.Top + rect.Height / 2,
                            rect.Left + rect.Height / 2 - curveSize / 2, rect.Bottom - curveSize / 2);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Bottom - rect.Height / 2);
                        GraphicsPath.AddLine(rect.Left, rect.Bottom - rect.Height / 2,
                            rect.Left + rect.Height / 2 - curveSize / 2, rect.Top + curveSize / 2);
                    }
                }
            }

            if (RightToLeft == RightToLeft.Yes)
            {
                if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                {
                    // Draws the bottom horizontal line (short side)
                    GraphicsPath.AddLine(rect.Right - rect.Height / 2 - curveSize / 2, rect.Bottom,
                        rect.Left + curveSize / 2, rect.Bottom);

                    // Drawing the rounded corner is not necessary. The path is automatically connected
                    // GraphicsPath.AddArc(new Rectangle(rect.Left, rect.Top, curveSize, curveSize), 180, 90);
                }
                else
                {
                    // Draws the bottom horizontal line (short side)
                    GraphicsPath.AddLine(rect.Right - rect.Height / 2 - curveSize / 2, rect.Top,
                        rect.Left + curveSize / 2, rect.Top);
                    GraphicsPath.AddArc(new Rectangle(rect.Left, rect.Top, curveSize, curveSize), 180, 90);
                }
            }
            else
            {
                if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                {
                    // Draws the bottom horizontal line (short side)
                    GraphicsPath.AddLine(rect.Left + rect.Height / 2 + curveSize / 2, rect.Bottom,
                        rect.Right - curveSize / 2, rect.Bottom);

                    // Drawing the rounded corner is not necessary. The path is automatically connected
                    // GraphicsPath.AddArc(new Rectangle(rect.Right - curveSize, rect.Bottom, curveSize, curveSize), 90, -90);
                }
                else
                {
                    // Draws the top horizontal line (short side)
                    GraphicsPath.AddLine(rect.Left + rect.Height / 2 + curveSize / 2, rect.Top,
                        rect.Right - curveSize / 2, rect.Top);

                    // Draws the rounded corner oppposite the angled side
                    GraphicsPath.AddArc(new Rectangle(rect.Right - curveSize, rect.Top, curveSize, curveSize), -90, 90);
                }
            }

            if (Tabs.IndexOf(tab) != EndDisplayingTab && Tabs.IndexOf(tab) != Tabs.Count - 1 &&
                Tabs[Tabs.IndexOf(tab) + 1].Content == DockPane.ActiveContent && !full)
            {
                if (RightToLeft == RightToLeft.Yes)
                {
                    if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Bottom - curveSize / 2, rect.Left,
                            rect.Bottom - rect.Height / 2);
                        GraphicsPath.AddLine(rect.Left, rect.Bottom - rect.Height / 2, rect.Left + rect.Height / 2,
                            rect.Top);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Top + curveSize / 2, rect.Left,
                            rect.Top + rect.Height / 2);
                        GraphicsPath.AddLine(rect.Left, rect.Top + rect.Height / 2, rect.Left + rect.Height / 2,
                            rect.Bottom);
                    }
                }
                else
                {
                    if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Bottom - curveSize / 2, rect.Right,
                            rect.Bottom - rect.Height / 2);
                        GraphicsPath.AddLine(rect.Right, rect.Bottom - rect.Height / 2, rect.Right - rect.Height / 2,
                            rect.Top);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Top + curveSize / 2, rect.Right,
                            rect.Top + rect.Height / 2);
                        GraphicsPath.AddLine(rect.Right, rect.Top + rect.Height / 2, rect.Right - rect.Height / 2,
                            rect.Bottom);
                    }
                }
            }
            else
            {
                // Draw the vertical line opposite the angled side
                if (RightToLeft == RightToLeft.Yes)
                {
                    if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Bottom - curveSize / 2, rect.Left, rect.Top);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Left, rect.Top + curveSize / 2, rect.Left, rect.Bottom);
                    }
                }
                else
                {
                    if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Bottom - curveSize / 2, rect.Right, rect.Top);
                    }
                    else
                    {
                        GraphicsPath.AddLine(rect.Right, rect.Top + curveSize / 2, rect.Right, rect.Bottom);
                    }
                }
            }

            return GraphicsPath;
        }

        private Rectangle GetTabRectangle_Document(int index)
        {
            Rectangle rectTabStrip = TabStripRectangle;
            TabVS2005 tab = (TabVS2005)Tabs[index];

            Rectangle rect = new Rectangle();
            rect.X = tab.TabX;
            rect.Width = tab.TabWidth;
            rect.Height = rectTabStrip.Height - DocumentTabGapTop;

            if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
            {
                rect.Y = rectTabStrip.Y + DocumentStripGapBottom;
            }
            else
            {
                rect.Y = rectTabStrip.Y + DocumentTabGapTop;
            }

            return rect;
        }

        private int MeasureHeight_Document()
        {
            int height = Math.Max(TextFont.Height + DocumentTabGapTop,
                             ButtonClose.Height + DocumentButtonGapTop + DocumentButtonGapBottom)
                         + DocumentStripGapBottom + DocumentStripGapTop;

            return height;
        }

        #endregion

        #region Private Methods

        private void CalculateTabs()
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
            {
                CalculateTabs_ToolWindow();
            }
            else
            {
                CalculateTabs_Document();
            }
        }

        private void CalculateTabs_ToolWindow()
        {
            if (Tabs.Count <= 1 || DockPane.IsAutoHide)
            {
                return;
            }

            Rectangle rectTabStrip = TabStripRectangle;

            // Calculate tab widths
            int countTabs = Tabs.Count;
            foreach (TabVS2005 tab in Tabs)
            {
                tab.MaxWidth = GetMaxTabWidth(Tabs.IndexOf(tab));
                tab.Flag = false;
            }

            // Set tab whose max width less than average width
            bool anyWidthWithinAverage = true;
            int totalWidth = rectTabStrip.Width - ToolWindowStripGapLeft - ToolWindowStripGapRight;
            int totalAllocatedWidth = 0;
            int averageWidth = totalWidth / countTabs;
            int remainedTabs = countTabs;
            for (anyWidthWithinAverage = true; anyWidthWithinAverage && remainedTabs > 0;)
            {
                anyWidthWithinAverage = false;
                foreach (TabVS2005 tab in Tabs)
                {
                    if (tab.Flag)
                    {
                        continue;
                    }

                    if (tab.MaxWidth <= averageWidth)
                    {
                        tab.Flag = true;
                        tab.TabWidth = tab.MaxWidth;
                        totalAllocatedWidth += tab.TabWidth;
                        anyWidthWithinAverage = true;
                        remainedTabs--;
                    }
                }

                if (remainedTabs != 0)
                {
                    averageWidth = (totalWidth - totalAllocatedWidth) / remainedTabs;
                }
            }

            // If any tab width not set yet, set it to the average width
            if (remainedTabs > 0)
            {
                int roundUpWidth = totalWidth - totalAllocatedWidth - averageWidth * remainedTabs;
                foreach (TabVS2005 tab in Tabs)
                {
                    if (tab.Flag)
                    {
                        continue;
                    }

                    tab.Flag = true;
                    if (roundUpWidth > 0)
                    {
                        tab.TabWidth = averageWidth + 1;
                        roundUpWidth--;
                    }
                    else
                    {
                        tab.TabWidth = averageWidth;
                    }
                }
            }

            // Set the X position of the tabs
            int x = rectTabStrip.X + ToolWindowStripGapLeft;
            foreach (TabVS2005 tab in Tabs)
            {
                tab.TabX = x;
                x += tab.TabWidth;
            }
        }

        private void Close_Click(object sender, EventArgs e)
        {
            DockPane.CloseActiveContent();
        }

        private void ContextMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                IDockContent content = (IDockContent)item.Tag;
                DockPane.ActiveContent = content;
            }
        }

        private void DrawTab(Graphics g, TabVS2005 tab, Rectangle rect)
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
            {
                DrawTab_ToolWindow(g, tab, rect);
            }
            else
            {
                DrawTab_Document(g, tab, rect);
            }
        }

        private void DrawTab_ToolWindow(Graphics g, TabVS2005 tab, Rectangle rect)
        {
            Rectangle rectIcon = new Rectangle(
                rect.X + ToolWindowImageGapLeft,
                rect.Y + rect.Height - 1 - ToolWindowImageGapBottom - ToolWindowImageHeight,
                ToolWindowImageWidth, ToolWindowImageHeight);
            Rectangle rectText = rectIcon;
            rectText.X += rectIcon.Width + ToolWindowImageGapRight;
            rectText.Width = rect.Width - rectIcon.Width - ToolWindowImageGapLeft -
                             ToolWindowImageGapRight - ToolWindowTextGapRight;

            Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
            rectText = DrawHelper.RtlTransform(this, rectText);
            rectIcon = DrawHelper.RtlTransform(this, rectIcon);
            GraphicsPath path = GetTabOutline(tab, true, false);
            if (DockPane.ActiveContent == tab.Content)
            {
                Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient
                    .StartColor;
                Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient
                    .EndColor;
                LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient
                    .ActiveTabGradient.LinearGradientMode;
                g.FillPath(new LinearGradientBrush(rectTab, startColor, endColor, gradientMode), path);
                g.DrawPath(PenToolWindowTabBorder, path);

                Color textColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient
                    .TextColor;
                TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, textColor,
                    ToolWindowTextFormat);
            }
            else
            {
                Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient
                    .StartColor;
                Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient
                    .EndColor;
                LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient
                    .InactiveTabGradient.LinearGradientMode;
                g.FillPath(new LinearGradientBrush(rectTab, startColor, endColor, gradientMode), path);

                if (Tabs.IndexOf(DockPane.ActiveContent) != Tabs.IndexOf(tab) + 1)
                {
                    Point pt1 = new Point(rect.Right, rect.Top + ToolWindowTabSeperatorGapTop);
                    Point pt2 = new Point(rect.Right, rect.Bottom - ToolWindowTabSeperatorGapBottom);
                    g.DrawLine(PenToolWindowTabBorder, DrawHelper.RtlTransform(this, pt1),
                        DrawHelper.RtlTransform(this, pt2));
                }

                Color textColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient
                    .TextColor;
                TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, textColor,
                    ToolWindowTextFormat);
            }

            if (rectTab.Contains(rectIcon))
            {
                g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
            }
        }

        private void DrawTabStrip(Graphics g)
        {
            if (Appearance == DockPane.AppearanceStyle.Document)
            {
                DrawTabStrip_Document(g);
            }
            else
            {
                DrawTabStrip_ToolWindow(g);
            }
        }

        private void DrawTabStrip_ToolWindow(Graphics g)
        {
            Rectangle rectTabStrip = TabStripRectangle;

            g.DrawLine(PenToolWindowTabBorder, rectTabStrip.Left, rectTabStrip.Top,
                rectTabStrip.Right, rectTabStrip.Top);

            for (int i = 0; i < Tabs.Count; i++)
            {
                DrawTab(g, Tabs[i] as TabVS2005, GetTabRectangle(i));
            }
        }

        private int GetMaxTabWidth(int index)
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
            {
                return GetMaxTabWidth_ToolWindow(index);
            }

            return GetMaxTabWidth_Document(index);
        }

        private int GetMaxTabWidth_ToolWindow(int index)
        {
            IDockContent content = Tabs[index].Content;
            Size sizeString = TextRenderer.MeasureText(content.DockHandler.TabText, TextFont);
            return ToolWindowImageWidth + sizeString.Width + ToolWindowImageGapLeft
                   + ToolWindowImageGapRight + ToolWindowTextGapRight;
        }

        private GraphicsPath GetOutline_ToolWindow(int index)
        {
            Rectangle rectTab = GetTabRectangle(index);
            rectTab.Intersect(TabsRectangle);
            rectTab = RectangleToScreen(DrawHelper.RtlTransform(this, rectTab));
            Rectangle rectPaneClient = DockPane.RectangleToScreen(DockPane.ClientRectangle);

            GraphicsPath path = new GraphicsPath();
            GraphicsPath pathTab = GetTabOutline(Tabs[index], true, true);
            path.AddPath(pathTab, true);
            path.AddLine(rectTab.Left, rectTab.Top, rectPaneClient.Left, rectTab.Top);
            path.AddLine(rectPaneClient.Left, rectTab.Top, rectPaneClient.Left, rectPaneClient.Top);
            path.AddLine(rectPaneClient.Left, rectPaneClient.Top, rectPaneClient.Right, rectPaneClient.Top);
            path.AddLine(rectPaneClient.Right, rectPaneClient.Top, rectPaneClient.Right, rectTab.Top);
            path.AddLine(rectPaneClient.Right, rectTab.Top, rectTab.Right, rectTab.Top);
            return path;
        }

        private GraphicsPath GetTabOutline(Tab tab, bool rtlTransform, bool toScreen)
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
            {
                return GetTabOutline_ToolWindow(tab, rtlTransform, toScreen);
            }

            return GetTabOutline_Document(tab, rtlTransform, toScreen, false);
        }

        private GraphicsPath GetTabOutline_ToolWindow(Tab tab, bool rtlTransform, bool toScreen)
        {
            Rectangle rect = GetTabRectangle(Tabs.IndexOf(tab));
            if (rtlTransform)
            {
                rect = DrawHelper.RtlTransform(this, rect);
            }

            if (toScreen)
            {
                rect = RectangleToScreen(rect);
            }

            DrawHelper.GetRoundedCornerTab(GraphicsPath, rect, false);
            return GraphicsPath;
        }

        private Rectangle GetTabRectangle(int index)
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
            {
                return GetTabRectangle_ToolWindow(index);
            }

            return GetTabRectangle_Document(index);
        }

        private Rectangle GetTabRectangle_ToolWindow(int index)
        {
            Rectangle rectTabStrip = TabStripRectangle;

            TabVS2005 tab = (TabVS2005)Tabs[index];
            return new Rectangle(tab.TabX, rectTabStrip.Y, tab.TabWidth, rectTabStrip.Height);
        }

        private void LayoutButtons()
        {
            Rectangle rectTabStrip = TabStripRectangle;

            // Set position and size of the buttons
            int buttonWidth = ButtonClose.Image.Width;
            int buttonHeight = ButtonClose.Image.Height;
            int height = rectTabStrip.Height - DocumentButtonGapTop - DocumentButtonGapBottom;
            if (buttonHeight < height)
            {
                buttonWidth = buttonWidth * (height / buttonHeight);
                buttonHeight = height;
            }

            Size buttonSize = new Size(buttonWidth, buttonHeight);

            int x = rectTabStrip.X + rectTabStrip.Width - DocumentTabGapLeft
                                                        - DocumentButtonGapRight - buttonWidth;
            int y = rectTabStrip.Y + DocumentButtonGapTop;
            Point point = new Point(x, y);
            ButtonClose.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));

            // If the close button is not visible draw the window list button overtop.
            // Otherwise it is drawn to the left of the close button.
            if (m_closeButtonVisible)
            {
                point.Offset(-(DocumentButtonGapBetween + buttonWidth), 0);
            }

            ButtonWindowList.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
        }

        private int MeasureHeight_ToolWindow()
        {
            if (DockPane.IsAutoHide || Tabs.Count <= 1)
            {
                return 0;
            }

            int height = Math.Max(TextFont.Height,
                             ToolWindowImageHeight + ToolWindowImageGapTop + ToolWindowImageGapBottom)
                         + ToolWindowStripGapTop + ToolWindowStripGapBottom;

            return height;
        }

        private void SetInertButtons()
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
            {
                if (m_buttonClose != null)
                {
                    m_buttonClose.Left = -m_buttonClose.Width;
                }

                if (m_buttonWindowList != null)
                {
                    m_buttonWindowList.Left = -m_buttonWindowList.Width;
                }
            }
            else
            {
                ButtonClose.Enabled = DockPane.ActiveContent == null
                    ? true
                    : DockPane.ActiveContent.DockHandler.CloseButton;
                m_closeButtonVisible = DockPane.ActiveContent == null
                    ? true
                    : DockPane.ActiveContent.DockHandler.CloseButtonVisible;
                ButtonClose.Visible = m_closeButtonVisible;
                ButtonClose.RefreshChanges();
                ButtonWindowList.RefreshChanges();
            }
        }

        private void WindowList_Click(object sender, EventArgs e)
        {
            int x = 0;
            int y = ButtonWindowList.Location.Y + ButtonWindowList.Height;

            SelectMenu.Items.Clear();
            foreach (TabVS2005 tab in Tabs)
            {
                IDockContent content = tab.Content;
                ToolStripItem item =
                    SelectMenu.Items.Add(content.DockHandler.TabText, content.DockHandler.Icon.ToBitmap());
                item.Tag = tab.Content;
                item.Click += ContextMenuItem_Click;
            }

            SelectMenu.Show(ButtonWindowList, x, y);
        }

        #endregion

        #region Nested type: InertButton

        private sealed class InertButton : InertButtonBase
        {
            #region Private Fields

            private readonly Bitmap m_image0;
            private readonly Bitmap m_image1;

            private int m_imageCategory;

            #endregion

            #region Ctor

            public InertButton(Bitmap image0, Bitmap image1)
            {
                m_image0 = image0;
                m_image1 = image1;
            }

            #endregion

            #region Properties / Indexers

            public override Bitmap Image => ImageCategory == 0 ? m_image0 : m_image1;

            public int ImageCategory
            {
                get => m_imageCategory;
                set
                {
                    if (m_imageCategory == value)
                    {
                        return;
                    }

                    m_imageCategory = value;
                    Invalidate();
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: TabVS2005

        private class TabVS2005 : Tab
        {
            #region Ctor

            public TabVS2005(IDockContent content)
                : base(content)
            {
            }

            #endregion

            #region Properties / Indexers

            public int MaxWidth { get; set; }

            public int TabWidth { get; set; }

            public int TabX { get; set; }

            protected internal bool Flag { get; set; }

            #endregion
        }

        #endregion
    }
}
