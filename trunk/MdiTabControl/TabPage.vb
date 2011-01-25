Imports System.ComponentModel
Imports System.Drawing.Drawing2D

<DesignTimeVisible(False), Description("Represents a single tab page in a MdiTabControl.TabControl.")> _
Public Class TabPage

  Private m_BackHighColor As Color
  Private m_BackHighColorDisabled As Color
  Private m_BackLowColor As Color
  Private m_BackLowColorDisabled As Color
  Private m_BorderColor As Color
  Private m_BorderColorDisabled As Color
  Private m_ForeColorDisabled As Color
  Private m_Selected As Boolean = False
  Private m_Hot As Boolean = False
  Private m_MaximumWidth As Integer
  Private m_MinimumWidth As Integer
  Private m_PadLeft As Integer
  Private m_PadRight As Integer
  Private m_CloseButtonVisible As Boolean
  Private m_CloseButton As Image
  Private m_CloseButtonImageHot As Image
  Private m_CloseButtonImageDisabled As Image
  Private m_CloseButtonBackHighColor As Color
  Private m_CloseButtonBackLowColor As Color
  Private m_CloseButtonBorderColor As Color
  Private m_CloseButtonForeColor As Color
  Private m_CloseButtonBackHighColorDisabled As Color
  Private m_CloseButtonBackLowColorDisabled As Color
  Private m_CloseButtonBorderColorDisabled As Color
  Private m_CloseButtonForeColorDisabled As Color
  Private m_CloseButtonBackHighColorHot As Color
  Private m_CloseButtonBackLowColorHot As Color
  Private m_CloseButtonBorderColorHot As Color
  Private m_CloseButtonForeColorHot As Color
  Private m_HotTrack As Boolean
  Private m_CloseButtonSize As Size
  Private m_FontBoldOnSelect As Boolean
  Private m_IconSize As Size
  Private m_SmoothingMode As SmoothingMode
  Private m_Alignment As TabControl.TabAlignment
  Private m_GlassGradient As Boolean
  Private m_BorderEnhanced As Boolean
  Private m_RenderMode As ToolStripRenderMode
  Private m_BorderEnhanceWeight As TabControl.Weight
  Private m_closeButtonClicked As Boolean

  Friend WithEvents m_Form As Form
  Friend TabVisible As Boolean
  Friend TabLeft As Integer
  Friend MenuItem As New ToolStripMenuItem

  Private MouseOverCloseButton As Boolean = False

  <Description("Occurs when the user clicks the Tab Control.")> _
  Public Shadows Event Click(ByVal sender As Object, ByVal e As EventArgs)
  Public Event TabDoubleClick(ByVal sender As Object, ByVal e As EventArgs)

  Friend Event Close(ByVal sender As Object, ByVal e As EventArgs)
  Friend Event GetTabRegion(ByVal sender As Object, ByVal e As TabControl.GetTabRegionEventArgs)
  Friend Event TabPaintBackground(ByVal sender As Object, ByVal e As TabControl.TabPaintEventArgs)
  Friend Event TabPaintBorder(ByVal sender As Object, ByVal e As TabControl.TabPaintEventArgs)
  Friend Event Draging(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

  Sub New(ByVal Form As System.Windows.Forms.Form)
    ' This call is required by the Windows Form Designer.
    InitializeComponent()
    ' Add any initialization after the InitializeComponent() call.
    Me.SuspendLayout()
    Me.SetStyle(ControlStyles.DoubleBuffer, True)
    Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
    Me.SetStyle(ControlStyles.UserPaint, True)
    Me.SetStyle(ControlStyles.SupportsTransparentBackColor, True)
    MyBase.BackColor = Color.Transparent
    Me.Visible = False
    Me.Size = New System.Drawing.Size(1, 1)
    Form.TopLevel = False
    Form.MdiParent = Nothing
    Form.FormBorderStyle = Windows.Forms.FormBorderStyle.None
    Form.Dock = DockStyle.Fill
    Me.m_Form = Form
    MenuItem.Text = Form.Text
    MenuItem.Image = Form.Icon.ToBitmap
    MenuItem.Tag = Me
    Me.ResumeLayout(False)
  End Sub

  <Description("Gets the form associated with the tab page")> _
  Public ReadOnly Property Form() As Object
    Get
      Return m_Form
    End Get
  End Property

  <Description("Gets or sets the System.Drawing.Color structure that represents the starting color of the Background linear gradient for the tab.")> _
  Public Property BackHighColor() As Color
    Get
      Return m_BackHighColor
    End Get
    Set(ByVal value As Color)
      m_BackHighColor = value
      Invalidate()
    End Set
  End Property

  <Description("Gets or sets the System.Drawing.Color structure that represents the ending color of the Background linear gradient for the tab.")> _
  Public Property BackLowColor() As Color
    Get
      Return m_BackLowColor
    End Get
    Set(ByVal value As Color)
      m_BackLowColor = value
      Invalidate()
    End Set
  End Property

  <Description("Gets or sets the System.Drawing.Color structure that represents the border color.")> _
  Public Property BorderColor() As Color
    Get
      Return m_BorderColor
    End Get
    Set(ByVal value As Color)
      m_BorderColor = value
      Invalidate()
    End Set
  End Property

  <Description("Gets or sets the System.Drawing.Color structure that represents the starting color of the Background linear gradient for a non selected tab.")> _
  Public Property BackHighColorDisabled() As Color
    Get
      Return m_BackHighColorDisabled
    End Get
    Set(ByVal value As Color)
      m_BackHighColorDisabled = value
      Invalidate()
    End Set
  End Property

  <Description("Gets or sets the System.Drawing.Color structure that represents the ending color of the Background linear gradient for a non selected tab.")> _
  Public Property BackLowColorDisabled() As Color
    Get
      Return m_BackLowColorDisabled
    End Get
    Set(ByVal value As Color)
      m_BackLowColorDisabled = value
      Invalidate()
    End Set
  End Property

  <Description("Gets or sets the System.Drawing.Color structure that represents the border color of the tab when not selected.")> _
  Public Property BorderColorDisabled() As Color
    Get
      Return m_BorderColorDisabled
    End Get
    Set(ByVal value As Color)
      m_BorderColorDisabled = value
      Invalidate()
    End Set
  End Property

  <Description("Gets or sets the System.Drawing.Color structure that represents the fore color of the tab when not selected.")> _
  Public Property ForeColorDisabled() As Color
    Get
      Return m_ForeColorDisabled
    End Get
    Set(ByVal value As Color)
      m_ForeColorDisabled = value
      Invalidate()
    End Set
  End Property

  Friend Property IsSelected() As Boolean
    Get
      Return m_Selected
    End Get
    Set(ByVal Value As Boolean)
      If m_Selected <> Value Then
        m_Selected = Value
        If m_Selected Then
          m_Hot = False
        End If
        Invalidate()
      End If
    End Set
  End Property

  <Description("Returns whether the tab is selected or not.")> _
  Public ReadOnly Property Selected() As Boolean
    Get
      Return IsSelected
    End Get
  End Property

  Friend Property MaximumWidth() As Integer
    Get
      Return m_MaximumWidth
    End Get
    Set(ByVal value As Integer)
      m_MaximumWidth = value
      CalculateWidth()
      Invalidate()
    End Set
  End Property

  Friend Property MinimumWidth() As Integer
    Get
      Return m_MinimumWidth
    End Get
    Set(ByVal value As Integer)
      m_MinimumWidth = value
      CalculateWidth()
      Invalidate()
    End Set
  End Property

  Friend Property PadLeft() As Integer
    Get
      Return m_PadLeft
    End Get
    Set(ByVal value As Integer)
      m_PadLeft = value
      CalculateWidth()
      Invalidate()
    End Set
  End Property

  Friend Property PadRight() As Integer
    Get
      Return m_PadRight
    End Get
    Set(ByVal value As Integer)
      m_PadRight = value
      CalculateWidth()
      Invalidate()
    End Set
  End Property

  <Description("Gets or sets whether the tab close button is visble or not.")> _
  Public Property CloseButtonVisible() As Boolean
    Get
      Return m_CloseButtonVisible
    End Get
    Set(ByVal value As Boolean)
      If m_CloseButtonVisible <> value Then
        m_CloseButtonVisible = value
        CalculateWidth()
        Invalidate()
      End If
    End Set
  End Property

  Public Property CloseButtonImage() As Image
    Get
      Return m_CloseButton
    End Get
    Set(ByVal value As Image)
      m_CloseButton = value
      Invalidate()
    End Set
  End Property

  Public Property CloseButtonImageHot() As Image
    Get
      Return m_CloseButtonImageHot
    End Get
    Set(ByVal value As Image)
      m_CloseButtonImageHot = value
      Invalidate()
    End Set
  End Property

  Public Property CloseButtonImageDisabled() As Image
    Get
      Return m_CloseButtonImageDisabled
    End Get
    Set(ByVal value As Image)
      m_CloseButtonImageDisabled = value
      Invalidate()
    End Set
  End Property

  Public Property CloseButtonBackHighColor() As System.Drawing.Color
    Get
      Return m_CloseButtonBackHighColor
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonBackHighColor = Value
    End Set
  End Property

  Public Property CloseButtonBackLowColor() As System.Drawing.Color
    Get
      Return m_CloseButtonBackLowColor
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonBackLowColor = Value
    End Set
  End Property

  Public Property CloseButtonBorderColor() As System.Drawing.Color
    Get
      Return m_CloseButtonBorderColor
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonBorderColor = Value
    End Set
  End Property

  Public Property CloseButtonForeColor() As System.Drawing.Color
    Get
      Return m_CloseButtonForeColor
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonForeColor = Value
    End Set
  End Property

  Public Property CloseButtonBackHighColorDisabled() As System.Drawing.Color
    Get
      Return m_CloseButtonBackHighColorDisabled
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonBackHighColorDisabled = Value
    End Set
  End Property

  Public Property CloseButtonBackLowColorDisabled() As System.Drawing.Color
    Get
      Return m_CloseButtonBackLowColorDisabled
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonBackLowColorDisabled = Value
    End Set
  End Property

  Public Property CloseButtonBorderColorDisabled() As System.Drawing.Color
    Get
      Return m_CloseButtonBorderColorDisabled
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonBorderColorDisabled = Value
    End Set
  End Property

  Public Property CloseButtonForeColorDisabled() As System.Drawing.Color
    Get
      Return m_CloseButtonForeColorDisabled
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonForeColorDisabled = Value
    End Set
  End Property

  Public Property CloseButtonBackHighColorHot() As System.Drawing.Color
    Get
      Return m_CloseButtonBackHighColorHot
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonBackHighColorHot = Value
    End Set
  End Property

  Public Property CloseButtonBackLowColorHot() As System.Drawing.Color
    Get
      Return m_CloseButtonBackLowColorHot
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonBackLowColorHot = Value
    End Set
  End Property

  Public Property CloseButtonBorderColorHot() As System.Drawing.Color
    Get
      Return m_CloseButtonBorderColorHot
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonBorderColorHot = Value
    End Set
  End Property

  Public Property CloseButtonForeColorHot() As System.Drawing.Color
    Get
      Return m_CloseButtonForeColorHot
    End Get
    Set(ByVal Value As Color)
      m_CloseButtonForeColorHot = Value
    End Set
  End Property

  Friend Property HotTrack() As Boolean
    Get
      Return m_HotTrack
    End Get
    Set(ByVal value As Boolean)
      m_HotTrack = value
      Invalidate()
    End Set
  End Property

  Friend Property CloseButtonSize() As Size
    Get
      Return m_CloseButtonSize
    End Get
    Set(ByVal value As Size)
      m_CloseButtonSize = value
      CalculateWidth()
      Invalidate()
    End Set
  End Property

  Friend Property FontBoldOnSelect() As Boolean
    Get
      Return m_FontBoldOnSelect
    End Get
    Set(ByVal value As Boolean)
      m_FontBoldOnSelect = value
      CalculateWidth()
      Invalidate()
    End Set
  End Property

  Friend Property IconSize() As Size
    Get
      Return m_IconSize
    End Get
    Set(ByVal value As Size)
      m_IconSize = value
      CalculateWidth()
      Invalidate()
    End Set
  End Property

  Friend Property SmoothingMode() As SmoothingMode
    Get
      Return m_SmoothingMode
    End Get
    Set(ByVal value As SmoothingMode)
      m_SmoothingMode = value
      Invalidate()
    End Set
  End Property

  Friend Property Alignment() As TabControl.TabAlignment
    Get
      Return m_Alignment
    End Get
    Set(ByVal value As TabControl.TabAlignment)
      m_Alignment = value
      Invalidate()
    End Set
  End Property

  Friend Property GlassGradient() As Boolean
    Get
      Return m_GlassGradient
    End Get
    Set(ByVal value As Boolean)
      m_GlassGradient = value
    End Set
  End Property

  Friend Property BorderEnhanced() As Boolean
    Get
      Return m_BorderEnhanced
    End Get
    Set(ByVal value As Boolean)
      m_BorderEnhanced = value
    End Set
  End Property

  Friend Property RenderMode() As ToolStripRenderMode
    Get
      Return m_RenderMode
    End Get
    Set(ByVal value As ToolStripRenderMode)
      m_RenderMode = value
      Invalidate()
    End Set
  End Property

  Friend Property BorderEnhanceWeight() As TabControl.Weight
    Get
      Return m_BorderEnhanceWeight
    End Get
    Set(ByVal value As TabControl.Weight)
      m_BorderEnhanceWeight = value
    End Set
  End Property

  Public Property Icon() As Icon
    Get
      Return m_Form.Icon
    End Get
    Set(ByVal value As Icon)
      m_Form.Icon = value
      Dim r As New Region(New Rectangle(PadLeft, (Me.Height / 2 - m_IconSize.Height / 2), m_IconSize.Width, m_IconSize.Height))
      Me.Invalidate(r)
      r.Dispose()
      r = Nothing
      MenuItem.Image = value.ToBitmap
    End Set
  End Property

  <Description("Selects the TabPage.")> _
  Public Shadows Sub [Select]()
    If Not IsSelected Then
      RaiseEvent Click(Me, New EventArgs)
    End If
  End Sub

  Private Function CreateGradientBrush(ByVal Rectangle As Rectangle, ByVal Color1 As Color, ByVal Color2 As Color) As Drawing2D.LinearGradientBrush
    If m_GlassGradient Then
      Return Helper.CreateGlassGradientBrush(Rectangle, Color1, Color2)
    Else
      Return New Drawing2D.LinearGradientBrush(Rectangle, Color1, Color2, Drawing2D.LinearGradientMode.Vertical)
    End If
  End Function

  Private Sub TabContent_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles m_Form.FormClosed
    ' if the form is closed closes the tabpage
    RaiseEvent Close(Me, New EventArgs)
  End Sub

  Private Sub TabContent_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_Form.TextChanged
    CalculateWidth()
    Invalidate()
    MenuItem.Text = m_Form.Text
  End Sub

  Private Sub TabPage_Close(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Close

  End Sub

  Private Sub Tab_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
    If m_Selected And Not (MouseOverCloseButton And m_CloseButtonVisible) Then Exit Sub
    If e.Button = Windows.Forms.MouseButtons.Left Then
      ' Close button was clicked
      If MouseOverCloseButton And m_CloseButtonVisible Then
        ' try to close the form
        'm_Form.Close()
        m_closeButtonClicked = True
      Else ' tab was clicked
        ' select the tab
        [Select]()
      End If
    End If
  End Sub

  Private Sub Tab_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
    If m_Selected And Not (MouseOverCloseButton And m_CloseButtonVisible) Then
      m_closeButtonClicked = False
      Exit Sub
    End If
    If e.Button = Windows.Forms.MouseButtons.Left Then
      ' Close button was clicked
      If MouseOverCloseButton And m_CloseButtonVisible And m_closeButtonClicked Then
        ' try to close the form
        m_closeButtonClicked = False
        m_Form.Close()
      Else ' tab was clicked
        ' select the tab
        m_closeButtonClicked = False
        [Select]()
      End If
    End If
  End Sub

  Private Sub Tab_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MouseEnter
    If m_Selected Then Exit Sub
    If m_HotTrack Then m_Hot = True
    Invalidate()
  End Sub

  Private Sub Tab_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MouseLeave
    MouseOverCloseButton = False
    m_Hot = False
    Invalidate()
  End Sub

  Private Sub Tab_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
    Static State As Boolean = False
    If m_CloseButtonVisible Then
      ' verify if the mouse is over the close button
      Dim x As Integer = Me.Width - PadRight - m_CloseButtonSize.Width - 2
      Dim y As Integer = Me.Height / 2 - m_CloseButtonSize.Height / 2
      MouseOverCloseButton = e.X >= x And e.X <= x + m_CloseButtonSize.Width - 1 And e.Y >= y And e.Y <= y + m_CloseButtonSize.Height - 1
      If State <> MouseOverCloseButton And m_CloseButtonVisible Then
        State = MouseOverCloseButton
        Dim r As New Region(New Rectangle(x, y, m_CloseButtonSize.Width, m_CloseButtonSize.Height))
        Me.Invalidate(r)
        r.Dispose()
        r = Nothing
      End If
    End If
    If Me.RectangleToScreen(Me.ClientRectangle).Contains(Me.PointToScreen(New Point(e.X, e.Y))) Then
      Cursor = Cursors.Default
    Else ' the mouse is outside the tab (it happens only when the mouse was pressed on the tab and moved away while pressed)
      RaiseEvent Draging(Me, e)
      Cursor = Cursors.No
    End If
  End Sub

  ' Draws the tab text (the form text)
  Private Sub DrawText(ByVal g As Graphics)
    Dim f As Font = New Font(Font, IIf(m_Selected And m_FontBoldOnSelect, FontStyle.Bold, FontStyle.Regular))
    Dim b As Brush = New SolidBrush(IIf(m_Selected Or m_Hot, ForeColor, m_ForeColorDisabled))
    Dim bounds As RectangleF = New RectangleF(PadLeft + IIf(m_Form.Icon Is Nothing, 0, m_IconSize.Width) + 2, 1, Width - PadLeft - IIf(m_Form.Icon Is Nothing, 0, m_IconSize.Height) - 5 - IIf(m_CloseButtonVisible, m_CloseButtonSize.Width, 0) - PadRight, Me.DisplayRectangle.Height)
    Dim MyFormat As StringFormat = New StringFormat
    MyFormat.FormatFlags = StringFormatFlags.NoWrap
    MyFormat.LineAlignment = StringAlignment.Center
    MyFormat.Trimming = StringTrimming.EllipsisCharacter
    g.DrawString(m_Form.Text, f, b, bounds, MyFormat)
    MyFormat.Dispose()
    b.Dispose()
    f.Dispose()
    MyFormat = Nothing
    b = Nothing
    f = Nothing
  End Sub

  ' Draws the tab icon if exists (the form icon)
  Private Sub DrawIcon(ByVal g As Graphics)
    Try
      If m_Form.Icon Is Nothing Then Exit Sub
      Dim r As Rectangle = New Rectangle(PadLeft, (Me.Height / 2 - m_IconSize.Height / 2), m_IconSize.Width, m_IconSize.Height)
      Dim i As Icon = New Icon(m_Form.Icon, m_IconSize)
      g.DrawIcon(i, r)
      DestroyIcon(i.Handle)
      i.Dispose()
      i = Nothing
    Catch ex As Exception
    End Try
  End Sub

  <System.Runtime.InteropServices.DllImportAttribute("user32.dll")> _
  Private Shared Function DestroyIcon(ByVal handle As IntPtr) As Boolean
  End Function

  ' Draws the Close Button
  Private Sub DrawCloseButton(ByVal g As Graphics)
    Try
      Dim I As Bitmap
      Dim x As Integer = Me.Width - (m_CloseButtonSize.Width + PadRight + 2)
      Dim y As Integer = Me.Height / 2 - m_CloseButtonSize.Height / 2
      If MouseOverCloseButton Then
        I = m_CloseButtonImageHot
      ElseIf m_Selected Then
        I = m_CloseButton
      Else
        I = m_CloseButtonImageDisabled
      End If
      Dim IsDisposable As Boolean = False
      If I Is Nothing Then
        I = GetButton()
        IsDisposable = True
      End If
      Dim icon As Icon = Drawing.Icon.FromHandle(I.GetHicon)
      Dim r As Rectangle = New Rectangle(x, y, m_CloseButtonSize.Width, m_CloseButtonSize.Height)
      g.DrawIcon(icon, r)
      If IsDisposable Then
        I.Dispose()
        I = Nothing
      End If
      DestroyIcon(icon.Handle)
      icon.Dispose()
      icon = Nothing
    Catch ex As Exception
    End Try
  End Sub

  ' Generates the close button image
  Private Function GetButton() As Bitmap
    Dim Points() As System.Drawing.Point = {New Point(1, 0), New Point(3, 0), New Point(5, 2), New Point(7, 0), New Point(9, 0), New Point(6, 3), New Point(6, 4), New Point(9, 7), New Point(7, 7), New Point(5, 5), New Point(3, 7), New Point(1, 7), New Point(4, 4), New Point(4, 3)}
    Dim gp As New Drawing2D.GraphicsPath
    Dim bch As Color
    Dim bcl As Color
    Dim bc As Color
    Dim fc As Color
    Dim B As Bitmap
    Dim m As New Drawing2D.Matrix
    Dim path() As System.Drawing.Point = {New Point(0, 1), New Point(1, 0), New Point(15, 0), New Point(16, 1), New Point(16, 14), New Point(15, 15), New Point(1, 15), New Point(0, 14)}
    Dim g As Graphics
    If MouseOverCloseButton Then
      bch = RenderColors.TabCloseButtonBackHighColorHot(m_RenderMode, CloseButtonBackHighColorHot)
      bcl = RenderColors.TabCloseButtonBackLowColorHot(m_RenderMode, CloseButtonBackLowColorHot)
      bc = RenderColors.TabCloseButtonBorderColorHot(m_RenderMode, CloseButtonBorderColorHot)
      fc = RenderColors.TabCloseButtonForeColorHot(m_RenderMode, CloseButtonForeColorHot)
    ElseIf m_Selected Then
      bch = RenderColors.TabCloseButtonBackHighColor(m_RenderMode, CloseButtonBackHighColor)
      bcl = RenderColors.TabCloseButtonBackLowColor(m_RenderMode, CloseButtonBackLowColor)
      bc = RenderColors.TabCloseButtonBorderColor(m_RenderMode, CloseButtonBorderColor)
      fc = RenderColors.TabCloseButtonForeColor(m_RenderMode, CloseButtonForeColor)
    Else
      bch = RenderColors.TabCloseButtonBackHighColorDisabled(m_RenderMode, CloseButtonBackHighColorDisabled)
      bcl = RenderColors.TabCloseButtonBackLowColorDisabled(m_RenderMode, CloseButtonBackLowColorDisabled)
      bc = RenderColors.TabCloseButtonBorderColorDisabled(m_RenderMode, CloseButtonBorderColorDisabled)
      fc = RenderColors.TabCloseButtonForeColorDisabled(m_RenderMode, CloseButtonForeColorDisabled)
    End If
    B = New Bitmap(17, 17)
    B.MakeTransparent()
    g = Graphics.FromImage(B)
    ' draw the border and background
    g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
    Dim l As New Drawing2D.LinearGradientBrush(New Point(0, 0), New Point(0, 15), bch, bcl)
    g.FillPolygon(l, path)
    Dim p As New Pen(bc)
    g.DrawPolygon(p, path)
    g.SmoothingMode = Drawing2D.SmoothingMode.Default
    ' draw the foreground
    gp.AddPolygon(Points)
    m.Translate(3, 4)
    gp.Transform(m)
    p.Dispose()
    p = New Pen(fc)
    g.DrawPolygon(p, gp.PathPoints)
    Dim sb As New SolidBrush(fc)
    g.FillPolygon(sb, gp.PathPoints)
    sb.Dispose()
    p.Dispose()
    gp.Dispose()
    g.Dispose()
    m.Dispose()
    Return B
  End Function

  ' Calculates the tab width
  Private Sub CalculateWidth()
    Dim g As Graphics = Me.CreateGraphics()
    Dim iw As Integer = 0
    Dim cbw As Integer = 0
    Dim w As Integer = Width
    If m_Form.Icon IsNot Nothing Then iw = m_IconSize.Width
    If m_CloseButtonVisible Then cbw = m_CloseButtonSize.Width
    Dim f As New Font(Font, IIf(m_FontBoldOnSelect, FontStyle.Bold, FontStyle.Regular))
    w = PadLeft + iw + 3 + g.MeasureString(m_Form.Text, f).Width + 3 + cbw + m_PadRight + 2
    f.Dispose()
    If w < m_MinimumWidth + 1 Then
      w = m_MinimumWidth + 1
    ElseIf w > m_MaximumWidth + 1 Then
      w = m_MaximumWidth + 1
    End If
    If w <> Width Then
      Width = w
    End If
    g.Dispose()
  End Sub

  ' Get the tab region shape
  Private Function GetRegion(ByVal W As Integer, ByVal H As Integer, ByVal H1 As Integer) As Point()
    Dim R() As Point = {New Point(0, H), New Point(0, 2), New Point(2, 0), New Point(W - 3, 0), New Point(W - 1, 2), New Point(W - 1, H)}
    Dim e As New TabControl.GetTabRegionEventArgs(R, W, H, Me.IsSelected)
    RaiseEvent GetTabRegion(Me, e)
    Array.Resize(e.Points, e.Points.Length + 2)
    Array.Copy(e.Points, 0, e.Points, 1, e.Points.Length - 1)
    e.Points(0) = New Point(e.Points(1).X, H1)
    e.Points(e.Points.Length - 1) = New Point(e.Points(e.Points.Length - 2).X, H1)
    Return e.Points
  End Function

  Private Sub MirrorPath(ByVal GraphicPath As Drawing2D.GraphicsPath)
    Dim m As New Matrix
    m.Translate(0, Height - 1)
    m.Scale(1, -1)
    GraphicPath.Transform(m)
    m.Dispose()
  End Sub

  ' Paint the tab
  Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
    Dim Painting As Boolean = False
    If Painting Then Exit Sub
    Painting = True
    Me.SuspendLayout()
    Dim RenderBorderColor As Color
    Dim RenderBottomColor As Color
    Dim RenderHighColor As Color
    Dim RenderLowColor As Color
    Dim GraphicPath As New Drawing2D.GraphicsPath

    Dim w As Integer = Me.Width
    CalculateWidth()
    If w <> Me.Width Then
      GraphicPath.Dispose()
      Exit Sub
    End If

    If m_Selected Then
      RenderBorderColor = RenderColors.BorderColor(m_RenderMode, BorderColor)
      RenderHighColor = RenderColors.TabBackHighColor(m_RenderMode, BackHighColor)
      RenderLowColor = RenderColors.TabBackLowColor(m_RenderMode, BackLowColor)
      RenderBottomColor = RenderColors.TabBackLowColor(m_RenderMode, BackLowColor)
    ElseIf m_Hot Then
      RenderBorderColor = RenderColors.BorderColor(m_RenderMode, BorderColor)
      RenderHighColor = RenderColors.TabBackHighColor(m_RenderMode, BackHighColor)
      RenderLowColor = RenderColors.TabBackLowColor(m_RenderMode, BackLowColor)
      RenderBottomColor = RenderColors.BorderColor(m_RenderMode, BorderColor)
    Else
      RenderBorderColor = RenderColors.BorderColorDisabled(m_RenderMode, BorderColorDisabled)
      RenderHighColor = RenderColors.TabBackHighColorDisabled(m_RenderMode, BackHighColorDisabled)
      RenderLowColor = RenderColors.TabBackLowColorDisabled(m_RenderMode, BackLowColorDisabled)
      RenderBottomColor = RenderColors.BorderColor(m_RenderMode, BorderColorDisabled)
    End If

    e.Graphics.SmoothingMode = m_SmoothingMode

    GraphicPath.AddPolygon(GetRegion(Width - 1, Height - 1, IIf(Me.IsSelected, Height, Height - 1)))

    ' if is bottom mirror the button vertically
    If m_Alignment = TabControl.TabAlignment.Bottom Then
      MirrorPath(GraphicPath)
      Dim x As Color = RenderHighColor
      RenderHighColor = RenderLowColor
      RenderLowColor = x
    End If

    ' Get the correct region including all the borders
    Dim R As Region = New Region(GraphicPath)
    Dim R1 As Region = New Region(GraphicPath)
    Dim R2 As Region = New Region(GraphicPath)
    Dim R3 As Region = New Region(GraphicPath)
    Dim M1 As Matrix = New Matrix
    Dim M2 As Matrix = New Matrix
    Dim M3 As Matrix = New Matrix
    M1.Translate(0, -0.5)
    M2.Translate(0, 0.5)
    M3.Translate(1, 0)
    R1.Transform(M1)
    R2.Transform(M2)
    R3.Transform(M3)
    R.Union(R1)
    R.Union(R2)
    R.Union(R3)
    Me.Region = R

    Dim RF As RectangleF = R.GetBounds(e.Graphics)
    Dim rec As New Rectangle(0, 0, RF.Width, RF.Height)
    Dim te As TabControl.TabPaintEventArgs

    te = New TabControl.TabPaintEventArgs(e.Graphics, rec, m_Selected, m_Hot, GraphicPath, Width, Height)
    RaiseEvent TabPaintBackground(Me, te) ' try to owner draw
    Dim gb As LinearGradientBrush = CreateGradientBrush(New Rectangle(0, 0, Me.Width, Me.Height), RenderHighColor, RenderLowColor)
    If Not te.Handled Then e.Graphics.FillPath(gb, GraphicPath)
    gb.Dispose()
    te.Dispose()

    te = New TabControl.TabPaintEventArgs(e.Graphics, rec, m_Selected, m_Hot, GraphicPath, Width, Height)
    RaiseEvent TabPaintBorder(Me, te) ' try to owner draw
    If Not te.Handled Then
      If m_BorderEnhanced Then
        Dim c As Color = IIf(m_Alignment = TabControl.TabAlignment.Bottom, RenderLowColor, RenderHighColor)
        Dim p As New Pen(c, m_BorderEnhanceWeight)
        e.Graphics.DrawLines(p, GraphicPath.PathPoints)
        p.Dispose()
      End If
      Dim p1 As New Pen(RenderBorderColor)
      e.Graphics.DrawLines(p1, GraphicPath.PathPoints)
      p1.Dispose()
    End If
    te.Dispose()

    e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.None
    e.Graphics.DrawLine(New Pen(RenderBottomColor), GraphicPath.PathPoints(0), GraphicPath.PathPoints(GraphicPath.PointCount - 1))
    e.Graphics.SmoothingMode = m_SmoothingMode

    DrawIcon(e.Graphics)
    DrawText(e.Graphics)
    If m_CloseButtonVisible Then DrawCloseButton(e.Graphics)
    Me.ResumeLayout()

    ' do the memory cleanup
    GraphicPath.Dispose()
    M1.Dispose()
    M2.Dispose()
    M3.Dispose()
    R1.Dispose()
    R2.Dispose()
    R3.Dispose()
    R.Dispose()
    te.Dispose()
    Painting = False
  End Sub

#Region " Obsolete properties "

  <EditorBrowsable(EditorBrowsableState.Never)> _
  Public Overrides Property MinimumSize() As Size
    Get
    End Get
    Set(ByVal value As Size)
    End Set
  End Property

  <EditorBrowsable(EditorBrowsableState.Never)> _
  Public Overrides Property MaximumSize() As Size
    Get
    End Get
    Set(ByVal value As Size)
    End Set
  End Property

  <EditorBrowsable(EditorBrowsableState.Never)> _
  Public Shadows Property Padding() As Padding
    Get
    End Get
    Set(ByVal value As Padding)
    End Set
  End Property

  <EditorBrowsable(EditorBrowsableState.Never)> _
  Public Overrides Property BackColor() As Color
    Get
    End Get
    Set(ByVal value As Color)
    End Set
  End Property

  <EditorBrowsable(EditorBrowsableState.Never)> _
  Public Overrides Property Dock() As System.Windows.Forms.DockStyle
    Get
    End Get
    Set(ByVal value As System.Windows.Forms.DockStyle)
    End Set
  End Property

  <EditorBrowsable(EditorBrowsableState.Never)> _
  Public Overrides Property Anchor() As System.Windows.Forms.AnchorStyles
    Get
    End Get
    Set(ByVal value As System.Windows.Forms.AnchorStyles)
    End Set
  End Property

  <EditorBrowsable(EditorBrowsableState.Never)> _
  Public Overrides Property Text() As String
    Get
      Return Nothing
    End Get
    Set(ByVal value As String)
    End Set
  End Property

#End Region

  Private Sub TabPage_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.DoubleClick
    RaiseEvent TabDoubleClick(Me, New EventArgs)
  End Sub
End Class
