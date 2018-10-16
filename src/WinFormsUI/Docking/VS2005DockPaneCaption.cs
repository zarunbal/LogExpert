using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class VS2005DockPaneCaption : DockPaneCaptionBase
    {
        #region Static/Constants

        private const int _ButtonGapBetween = 1;
        private const int _ButtonGapBottom = 1;
        private const int _ButtonGapLeft = 1;
        private const int _ButtonGapRight = 2;
        private const int _ButtonGapTop = 2;
        private const int _TextGapBottom = 0;
        private const int _TextGapLeft = 3;
        private const int _TextGapRight = 3;

        private const int _TextGapTop = 2;

        private static Blend _activeBackColorGradientBlend;

        private static Bitmap _imageButtonAutoHide;

        private static Bitmap _imageButtonClose;

        private static Bitmap _imageButtonDock;

        private static Bitmap _imageButtonOptions;

        private static readonly TextFormatFlags _textFormat =
            TextFormatFlags.SingleLine |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.VerticalCenter;

        private static string _toolTipAutoHide;

        private static string _toolTipClose;

        private static string _toolTipOptions;

        #endregion

        #region Private Fields

        private readonly ToolTip m_toolTip;

        private InertButton m_buttonAutoHide;

        private InertButton m_buttonClose;

        private InertButton m_buttonOptions;

        #endregion

        #region Ctor

        public VS2005DockPaneCaption(DockPane pane) : base(pane)
        {
            SuspendLayout();

            Components = new Container();
            m_toolTip = new ToolTip(Components);

            ResumeLayout();
        }

        #endregion

        #region Properties / Indexers

        public Font TextFont => DockPane.DockPanel.Skin.DockPaneStripSkin.TextFont;

        private static Blend ActiveBackColorGradientBlend
        {
            get
            {
                if (_activeBackColorGradientBlend == null)
                {
                    Blend blend = new Blend(2);

                    blend.Factors = new[] {0.5F, 1.0F};
                    blend.Positions = new[] {0.0F, 1.0F};
                    _activeBackColorGradientBlend = blend;
                }

                return _activeBackColorGradientBlend;
            }
        }

        private InertButton ButtonAutoHide
        {
            get
            {
                if (m_buttonAutoHide == null)
                {
                    m_buttonAutoHide = new InertButton(this, ImageButtonDock, ImageButtonAutoHide);
                    m_toolTip.SetToolTip(m_buttonAutoHide, ToolTipAutoHide);
                    m_buttonAutoHide.Click += AutoHide_Click;
                    Controls.Add(m_buttonAutoHide);
                }

                return m_buttonAutoHide;
            }
        }

        private InertButton ButtonClose
        {
            get
            {
                if (m_buttonClose == null)
                {
                    m_buttonClose = new InertButton(this, ImageButtonClose, ImageButtonClose);
                    m_toolTip.SetToolTip(m_buttonClose, ToolTipClose);
                    m_buttonClose.Click += Close_Click;
                    Controls.Add(m_buttonClose);
                }

                return m_buttonClose;
            }
        }

        private static int ButtonGapBetween => _ButtonGapBetween;

        private static int ButtonGapBottom => _ButtonGapBottom;

        private static int ButtonGapLeft => _ButtonGapLeft;

        private static int ButtonGapRight => _ButtonGapRight;

        private static int ButtonGapTop => _ButtonGapTop;

        private InertButton ButtonOptions
        {
            get
            {
                if (m_buttonOptions == null)
                {
                    m_buttonOptions = new InertButton(this, ImageButtonOptions, ImageButtonOptions);
                    m_toolTip.SetToolTip(m_buttonOptions, ToolTipOptions);
                    m_buttonOptions.Click += Options_Click;
                    Controls.Add(m_buttonOptions);
                }

                return m_buttonOptions;
            }
        }

        private bool CloseButtonEnabled => DockPane.ActiveContent != null ? DockPane.ActiveContent.DockHandler.CloseButton : false;

        /// <summary>
        ///     Determines whether the close button is visible on the content
        /// </summary>
        private bool CloseButtonVisible => DockPane.ActiveContent != null ? DockPane.ActiveContent.DockHandler.CloseButtonVisible : false;

        private IContainer Components { get; }

        private static Bitmap ImageButtonAutoHide
        {
            get
            {
                if (_imageButtonAutoHide == null)
                {
                    _imageButtonAutoHide = Resources.DockPane_AutoHide;
                }

                return _imageButtonAutoHide;
            }
        }

        private static Bitmap ImageButtonClose
        {
            get
            {
                if (_imageButtonClose == null)
                {
                    _imageButtonClose = Resources.DockPane_Close;
                }

                return _imageButtonClose;
            }
        }

        private static Bitmap ImageButtonDock
        {
            get
            {
                if (_imageButtonDock == null)
                {
                    _imageButtonDock = Resources.DockPane_Dock;
                }

                return _imageButtonDock;
            }
        }

        private static Bitmap ImageButtonOptions
        {
            get
            {
                if (_imageButtonOptions == null)
                {
                    _imageButtonOptions = Resources.DockPane_Option;
                }

                return _imageButtonOptions;
            }
        }

        private bool ShouldShowAutoHideButton => !DockPane.IsFloat;

        private Color TextColor
        {
            get
            {
                if (DockPane.IsActivated)
                {
                    return DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.TextColor;
                }

                return DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient
                    .TextColor;
            }
        }

        private TextFormatFlags TextFormat
        {
            get
            {
                if (RightToLeft == RightToLeft.No)
                {
                    return _textFormat;
                }

                return _textFormat | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
            }
        }

        private static int TextGapBottom => _TextGapBottom;

        private static int TextGapLeft => _TextGapLeft;

        private static int TextGapRight => _TextGapRight;

        private static int TextGapTop => _TextGapTop;

        private static string ToolTipAutoHide
        {
            get
            {
                if (_toolTipAutoHide == null)
                {
                    _toolTipAutoHide = Strings.DockPaneCaption_ToolTipAutoHide;
                }

                return _toolTipAutoHide;
            }
        }

        private static string ToolTipClose
        {
            get
            {
                if (_toolTipClose == null)
                {
                    _toolTipClose = Strings.DockPaneCaption_ToolTipClose;
                }

                return _toolTipClose;
            }
        }

        private static string ToolTipOptions
        {
            get
            {
                if (_toolTipOptions == null)
                {
                    _toolTipOptions = Strings.DockPaneCaption_ToolTipOptions;
                }

                return _toolTipOptions;
            }
        }

        #endregion

        #region Overrides

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Components.Dispose();
            }

            base.Dispose(disposing);
        }

        protected internal override int MeasureHeight()
        {
            int height = TextFont.Height + TextGapTop + TextGapBottom;

            if (height < ButtonClose.Image.Height + ButtonGapTop + ButtonGapBottom)
            {
                height = ButtonClose.Image.Height + ButtonGapTop + ButtonGapBottom;
            }

            return height;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            SetButtonsPosition();
            base.OnLayout(levent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawCaption(e.Graphics);
        }

        protected override void OnRefreshChanges()
        {
            SetButtons();
            Invalidate();
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            PerformLayout();
        }

        #endregion

        #region Private Methods

        private void AutoHide_Click(object sender, EventArgs e)
        {
            DockPane.DockState = DockHelper.ToggleAutoHideState(DockPane.DockState);
            if (DockHelper.IsDockStateAutoHide(DockPane.DockState))
            {
                DockPane.DockPanel.ActiveAutoHideContent = null;
                DockPane.NestedDockingStatus.NestedPanes.SwitchPaneWithFirstChild(DockPane);
            }
        }

        private void Close_Click(object sender, EventArgs e)
        {
            DockPane.CloseActiveContent();
        }

        private void DrawCaption(Graphics g)
        {
            if (ClientRectangle.Width == 0 || ClientRectangle.Height == 0)
            {
                return;
            }

            if (DockPane.IsActivated)
            {
                Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient
                    .StartColor;
                Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient
                    .EndColor;
                LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient
                    .ActiveCaptionGradient.LinearGradientMode;
                using (LinearGradientBrush brush =
                    new LinearGradientBrush(ClientRectangle, startColor, endColor, gradientMode))
                {
                    brush.Blend = ActiveBackColorGradientBlend;
                    g.FillRectangle(brush, ClientRectangle);
                }
            }
            else
            {
                Color startColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient
                    .StartColor;
                Color endColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient
                    .EndColor;
                LinearGradientMode gradientMode = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient
                    .InactiveCaptionGradient.LinearGradientMode;
                using (LinearGradientBrush brush =
                    new LinearGradientBrush(ClientRectangle, startColor, endColor, gradientMode))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
            }

            Rectangle rectCaption = ClientRectangle;

            Rectangle rectCaptionText = rectCaption;
            rectCaptionText.X += TextGapLeft;
            rectCaptionText.Width -= TextGapLeft + TextGapRight;
            rectCaptionText.Width -= ButtonGapLeft + ButtonClose.Width + ButtonGapRight;
            if (ShouldShowAutoHideButton)
            {
                rectCaptionText.Width -= ButtonAutoHide.Width + ButtonGapBetween;
            }

            if (HasTabPageContextMenu)
            {
                rectCaptionText.Width -= ButtonOptions.Width + ButtonGapBetween;
            }

            rectCaptionText.Y += TextGapTop;
            rectCaptionText.Height -= TextGapTop + TextGapBottom;

            Color colorText;
            if (DockPane.IsActivated)
            {
                colorText = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient
                    .TextColor;
            }
            else
            {
                colorText = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient
                    .TextColor;
            }

            TextRenderer.DrawText(g, DockPane.CaptionText, TextFont, DrawHelper.RtlTransform(this, rectCaptionText),
                colorText, TextFormat);
        }

        private void Options_Click(object sender, EventArgs e)
        {
            ShowTabPageContextMenu(PointToClient(MousePosition));
        }

        private void SetButtons()
        {
            ButtonClose.Enabled = CloseButtonEnabled;
            ButtonClose.Visible = CloseButtonVisible;
            ButtonAutoHide.Visible = ShouldShowAutoHideButton;
            ButtonOptions.Visible = HasTabPageContextMenu;
            ButtonClose.RefreshChanges();
            ButtonAutoHide.RefreshChanges();
            ButtonOptions.RefreshChanges();

            SetButtonsPosition();
        }

        private void SetButtonsPosition()
        {
            // set the size and location for close and auto-hide buttons
            Rectangle rectCaption = ClientRectangle;
            int buttonWidth = ButtonClose.Image.Width;
            int buttonHeight = ButtonClose.Image.Height;
            int height = rectCaption.Height - ButtonGapTop - ButtonGapBottom;
            if (buttonHeight < height)
            {
                buttonWidth = buttonWidth * (height / buttonHeight);
                buttonHeight = height;
            }

            Size buttonSize = new Size(buttonWidth, buttonHeight);
            int x = rectCaption.X + rectCaption.Width - 1 - ButtonGapRight - m_buttonClose.Width;
            int y = rectCaption.Y + ButtonGapTop;
            Point point = new Point(x, y);
            ButtonClose.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));

            // If the close button is not visible draw the auto hide button overtop.
            // Otherwise it is drawn to the left of the close button.
            if (CloseButtonVisible)
            {
                point.Offset(-(buttonWidth + ButtonGapBetween), 0);
            }

            ButtonAutoHide.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
            if (ShouldShowAutoHideButton)
            {
                point.Offset(-(buttonWidth + ButtonGapBetween), 0);
            }

            ButtonOptions.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
        }

        #endregion

        #region Nested type: InertButton

        private sealed class InertButton : InertButtonBase
        {
            #region Private Fields

            private readonly Bitmap m_image;
            private readonly Bitmap m_imageAutoHide;

            #endregion

            #region Ctor

            public InertButton(VS2005DockPaneCaption dockPaneCaption, Bitmap image, Bitmap imageAutoHide)
            {
                DockPaneCaption = dockPaneCaption;
                m_image = image;
                m_imageAutoHide = imageAutoHide;
                RefreshChanges();
            }

            #endregion

            #region Properties / Indexers

            public override Bitmap Image => IsAutoHide ? m_imageAutoHide : m_image;

            public bool IsAutoHide => DockPaneCaption.DockPane.IsAutoHide;

            private VS2005DockPaneCaption DockPaneCaption { get; }

            #endregion

            #region Overrides

            protected override void OnRefreshChanges()
            {
                if (DockPaneCaption.DockPane.DockPanel != null)
                {
                    if (DockPaneCaption.TextColor != ForeColor)
                    {
                        ForeColor = DockPaneCaption.TextColor;
                        Invalidate();
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}
