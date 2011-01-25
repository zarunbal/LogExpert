Imports System.ComponentModel

<DesignTimeVisible(True)> _
Public Class TabControl

#Region " Class GetTabRegionEventArgs "

    <Description("Provides data for the MdiTabControl.TabControl.GetTabRegion event.")> _
    Public Class GetTabRegionEventArgs
        Inherits System.EventArgs

        Private m_Points() As Point
        Private m_TabWidth As Integer
        Private m_TabHeight As Integer
        Private m_Selected As Boolean

        Private Sub New()
        End Sub

        <Description("Initializes a new instance of the MdiTabControl.TabControl.GetTabRegionEventArgs class.")> _
        Public Sub New(ByVal Points() As Point, ByVal Width As Integer, ByVal Height As Integer, ByVal Selected As Boolean)
            MyBase.new()
            m_Points = Points
            m_TabWidth = Width
            m_TabHeight = Height
            m_Selected = Selected
        End Sub

        <Description("Returns whether the tab is selected or not.")> _
        Public ReadOnly Property Selected() As Integer
            Get
                Return m_Selected
            End Get
        End Property

        <Description("Returns the tab width.")> _
        Public ReadOnly Property TabWidth() As Integer
            Get
                Return m_TabWidth
            End Get
        End Property

        <Description("Returns the tab height.")> _
        Public ReadOnly Property TabHeight() As Integer
            Get
                Return m_TabHeight
            End Get
        End Property

        <Description("Gets or sets an array of System.Drawing.Point structures that represents the points through which the tab path is constructed.")> _
        Public Property Points() As Point()
            Get
                Return m_Points
            End Get
            Set(ByVal value As Point())
                m_Points = value
            End Set
        End Property

    End Class

#End Region

#Region " Class TabPaintEventArgs "

    <Description("Provides data for the MdiTabControl.TabControl.TabPaint event.")> _
    Public Class TabPaintEventArgs
        Inherits PaintEventArgs

        Private m_Handled As Boolean = False
        Private m_Selected As Boolean = False
        Private m_Hot As Boolean = False
        Private m_GraphicPath As Drawing2D.GraphicsPath
        Private m_TabWidth As Integer
        Private m_TabHeight As Integer

        <Description("Initializes a new instance of the MdiTabControl.TabControl.GetTabRegionEventArgs class.")> _
        Public Sub New(ByVal graphics As Graphics, ByVal clipRect As Rectangle, ByVal Selected As Boolean, ByVal Hot As Boolean, ByVal GraphicPath As Drawing2D.GraphicsPath, ByVal Width As Integer, ByVal Height As Integer)
            MyBase.New(graphics, clipRect)
            m_Selected = Selected
            m_Hot = Hot
            m_GraphicPath = GraphicPath
            m_TabWidth = Width
            m_TabHeight = Height
        End Sub

        <Description("Returns the tab's hot state.")> _
        Public ReadOnly Property Hot() As Boolean
            Get
                Return m_Hot
            End Get
        End Property

        <Description("Returns whether the tab is selected or not.")> _
        Public ReadOnly Property Selected() As Boolean
            Get
                Return m_Selected
            End Get
        End Property

        <Description("Gets or sets a value that indicates whether the event handler has completely handled the paint or whether the system should continue its own processing.")> _
        Public Property Handled() As Boolean
            Get
                Return m_Handled
            End Get
            Set(ByVal value As Boolean)
                m_Handled = value
            End Set
        End Property

        <Description("Returns the tab width.")> _
        Public ReadOnly Property TabWidth() As Integer
            Get
                Return m_TabWidth
            End Get
        End Property

        <Description("Returns the tab height.")> _
        Public ReadOnly Property TabHeight() As Integer
            Get
                Return m_TabHeight
            End Get
        End Property

        <Description("Represents a series of connected lines and curves which the tab path is constructed.")> _
        Public ReadOnly Property GraphicPath() As Drawing2D.GraphicsPath
            Get
                Return m_GraphicPath
            End Get
        End Property

    End Class

#End Region

#Region " Class TabPageCollection "

    <Description("Contains a collection of MdiTabControl.TabPage objects.")> _
    Public Class TabPageCollection
        Inherits CollectionBase

        Private TabControl As TabControl
        Private IsReorder As Boolean = False

        Friend Event GetTabRegion(ByVal sender As Object, ByVal e As TabControl.GetTabRegionEventArgs)
        <Description("Occurs when the Tab Background has been painted.")> _
        Friend Event TabPaintBackground(ByVal sender As Object, ByVal e As TabControl.TabPaintEventArgs)
        <Description("Occurs when the Tab Border has been painted.")> _
        Friend Event TabPaintBorder(ByVal sender As Object, ByVal e As TabControl.TabPaintEventArgs)
        <Description("Occurs when the current Tab has been changed.")> _
        Friend Event TabPageChanged(ByVal sender As Object, ByVal e As EventArgs)

        Friend Sub New(ByVal Owner As TabControl)
            TabControl = Owner
        End Sub

        <Description("Create a new TabPage and adds it to the collection whit the Form associated and returns the created TabPage.")> _
        Public Function Add(ByVal Form As Form) As TabPage
            Dim TabPage As New TabPage(Form)
            TabPage.SuspendLayout()
            TabControl.SuspendLayout()

            ' Initialize all the tabpage defaults values 
            TabControl.AddingPage = True
            TabPage.BackHighColor = TabControl.TabBackHighColor
            TabPage.BackHighColorDisabled = TabControl.TabBackHighColorDisabled
            TabPage.BackLowColor = TabControl.TabBackLowColor
            TabPage.BackLowColorDisabled = TabControl.TabBackLowColorDisabled
            TabPage.BorderColor = TabControl.BorderColor
            TabPage.BorderColorDisabled = TabControl.BorderColorDisabled
            TabPage.ForeColor = TabControl.ForeColor
            TabPage.ForeColorDisabled = TabControl.ForeColorDisabled
            TabPage.MaximumWidth = TabControl.TabMaximumWidth
            TabPage.MinimumWidth = TabControl.TabMinimumWidth
            TabPage.PadLeft = TabControl.TabPadLeft
            TabPage.PadRight = TabControl.TabPadRight
            TabPage.CloseButtonVisible = TabControl.TabCloseButtonVisible
            TabPage.CloseButtonImage = TabControl.TabCloseButtonImage
            TabPage.CloseButtonImageHot = TabControl.TabCloseButtonImageHot
            TabPage.CloseButtonImageDisabled = TabControl.TabCloseButtonImageDisabled
            TabPage.CloseButtonSize = TabControl.TabCloseButtonSize
            TabPage.CloseButtonBackHighColor = TabControl.TabCloseButtonBackHighColor
            TabPage.CloseButtonBackLowColor = TabControl.TabCloseButtonBackLowColor
            TabPage.CloseButtonBorderColor = TabControl.TabCloseButtonBorderColor
            TabPage.CloseButtonForeColor = TabControl.TabCloseButtonForeColor
            TabPage.CloseButtonBackHighColorDisabled = TabControl.TabCloseButtonBackHighColorDisabled
            TabPage.CloseButtonBackLowColorDisabled = TabControl.TabCloseButtonBackLowColorDisabled
            TabPage.CloseButtonBorderColorDisabled = TabControl.TabCloseButtonBorderColorDisabled
            TabPage.CloseButtonForeColorDisabled = TabControl.TabCloseButtonForeColorDisabled
            TabPage.CloseButtonBackHighColorHot = TabControl.TabCloseButtonBackHighColorHot
            TabPage.CloseButtonBackLowColorHot = TabControl.TabCloseButtonBackLowColorHot
            TabPage.CloseButtonBorderColorHot = TabControl.TabCloseButtonBorderColorHot
            TabPage.CloseButtonForeColorHot = TabControl.TabCloseButtonForeColorHot
            TabPage.HotTrack = TabControl.HotTrack
            TabPage.Font = TabControl.Font
            TabPage.FontBoldOnSelect = TabControl.FontBoldOnSelect
            TabPage.IconSize = TabControl.TabIconSize
            TabPage.SmoothingMode = TabControl.SmoothingMode
            TabPage.Alignment = TabControl.Alignment
            TabPage.GlassGradient = TabControl.TabGlassGradient
            TabPage.BorderEnhanced = TabControl.m_TabBorderEnhanced
            TabPage.RenderMode = TabControl.RenderMode
            TabPage.BorderEnhanceWeight = TabControl.TabBorderEnhanceWeight

            TabPage.Top = 0
            TabPage.Left = TabControl.LeftOffset
            TabPage.Height = TabControl.TabHeight

            TabControl.TabToolTip.SetToolTip(TabPage, TabPage.m_Form.Text)

            ' Create the event handles 
            AddHandler TabPage.Click, AddressOf TabPage_Clicked
            AddHandler TabPage.Close, AddressOf TabPage_Closed
            AddHandler TabPage.GetTabRegion, AddressOf TabPage_GetTabRegion
            AddHandler TabPage.TabPaintBackground, AddressOf TabPage_TabPaintBackground
            AddHandler TabPage.TabPaintBorder, AddressOf TabPage_TabPaintBorder
            AddHandler TabPage.SizeChanged, AddressOf TabPage_SizeChanged
            AddHandler TabPage.Draging, AddressOf TabPage_Draging

            ' Insert the tabpage in the collection
      List.Add(TabPage)
            TabControl.ResumeLayout()
      TabPage.ResumeLayout()
      Return TabPage
        End Function

        <Description("Removes a TabPage from the collection.")> _
        Public Sub Remove(ByVal TabPage As TabPage)
            Try
                TabControl.IsDelete = True
                If TabControl.pnlBottom.Controls.Count > 1 Then
                    ' brings the next top tab
                    ' first dock the form in the body then display it
                    TabControl.pnlBottom.Controls(1).Dock = DockStyle.Fill
                    TabControl.pnlBottom.Controls(1).Visible = True
                End If
        List.Remove(TabPage)
        TabPage.m_Form = Nothing

        TabPage.Dispose()
        RemoveHandler TabPage.Click, AddressOf TabPage_Clicked
        RemoveHandler TabPage.Close, AddressOf TabPage_Closed
        RemoveHandler TabPage.GetTabRegion, AddressOf TabPage_GetTabRegion
        RemoveHandler TabPage.TabPaintBackground, AddressOf TabPage_TabPaintBackground
        RemoveHandler TabPage.TabPaintBorder, AddressOf TabPage_TabPaintBorder
        RemoveHandler TabPage.SizeChanged, AddressOf TabPage_SizeChanged
        RemoveHandler TabPage.Draging, AddressOf TabPage_Draging

            Catch ex As Exception
            End Try
        End Sub

        <Description("Gets a TabPage in the position Index from the collection.")> _
        Default Public ReadOnly Property Item(ByVal Index As Integer) As TabPage
            Get
                Return List.Item(Index)
            End Get
        End Property

        <Description("Gets a TabPage associated with the Form from the collection.")> _
        Default Public ReadOnly Property Item(ByVal Form As Form) As TabPage
            Get
                Dim x As Integer = IndexOf(Form)
                If x = -1 Then
                    Return Nothing
                Else
                    Return List.Item(x)
                End If
            End Get
        End Property

        <Description("Returns the index of the specified TabPage in the collection.")> _
        Public Property IndexOf(ByVal TabPage As TabPage) As Integer
            Get
                Return List.IndexOf(TabPage)
            End Get
            Set(ByVal value As Integer)
                IsReorder = True
                List.Remove(TabPage)
                List.Insert(value, TabPage)
                TabControl.ArrangeItems()
                IsReorder = False
            End Set
        End Property

        <Description("Returns the index of the specified TabPage associated with the Form in the collection.")> _
        Public ReadOnly Property IndexOf(ByVal Form As Form) As Integer
            Get
                Dim ret As Integer = -1
                For i As Integer = 0 To List.Count - 1
                    If DirectCast(List(i), TabPage).m_Form.Equals(Form) Then
                        ret = i
                        Exit For
                    End If
                Next
                Return ret
            End Get
        End Property

        Protected Overrides Sub OnInsertComplete(ByVal index As Integer, ByVal value As Object)
            MyBase.OnInsertComplete(index, value)
            If IsReorder Then Exit Sub
            ' insert the controls in the respective containers
            TabControl.pnlBottom.Controls.Add(DirectCast(value, TabPage).m_Form)
            TabControl.pnlTabs.Controls.Add(DirectCast(value, TabPage))
            ' select the inserted tabpage
            DirectCast(value, TabPage).Select()
            TabControl.AddingPage = False
            TabControl.ArrangeItems()
            TabControl.Background.Visible = False
        End Sub

        Protected Overrides Sub OnRemoveComplete(ByVal index As Integer, ByVal value As Object)
            MyBase.OnRemoveComplete(index, value)
            If IsReorder Then Exit Sub
            If List.Count = 0 Then TabControl.Background.Visible = True
            TabControl.ArrangeItems()
            TabControl.pnlBottom.Controls.Remove(DirectCast(value, TabPage).m_Form)
            DirectCast(value, TabPage).m_Form.Dispose()
            TabControl.pnlTabs.Controls.Remove(DirectCast(value, TabPage))
            DirectCast(value, TabPage).Dispose()
            TabControl.SelectItem(Nothing)
        End Sub

        Protected Overrides Sub OnClear()
            MyBase.OnClear()
            TabControl.Background.Visible = True
        End Sub

        Protected Overrides Sub OnClearComplete()
            MyBase.OnClearComplete()
            TabControl.pnlBottom.Controls.Clear()
            TabControl.pnlTabs.Controls.Clear()
        End Sub

        <Description("Returns the selected TabPage.")> _
        Public Function SelectedTab() As TabPage
            For Each T As TabPage In List
                If T.IsSelected Then Return T
            Next
            Return Nothing
        End Function

        <Description("Returns the index of the selected TabPage.")> _
        Public Function SelectedIndex() As Integer
            For Each T As TabPage In List
                If T.IsSelected Then Return List.IndexOf(T)
            Next
        End Function

        Private Sub TabPage_Clicked(ByVal sender As Object, ByVal e As EventArgs)
            TabControl.SelectItem(sender)
        End Sub

        Private Sub TabPage_Closed(ByVal sender As Object, ByVal e As EventArgs)
            Remove(sender)
        End Sub

        Private Sub TabPage_GetTabRegion(ByVal sender As Object, ByVal e As TabControl.GetTabRegionEventArgs)
            RaiseEvent GetTabRegion(sender, e)
        End Sub

        Private Sub TabPage_TabPaintBackground(ByVal sender As Object, ByVal e As TabControl.TabPaintEventArgs)
            RaiseEvent TabPaintBackground(sender, e)
        End Sub

        Private Sub TabPage_TabPaintBorder(ByVal sender As Object, ByVal e As TabControl.TabPaintEventArgs)
            RaiseEvent TabPaintBorder(sender, e)
        End Sub

        Private Sub TabPage_SizeChanged(ByVal sender As Object, ByVal e As EventArgs)
            TabControl.ArrangeItems()
        End Sub

        Private Sub TabPage_Draging(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
            If TabControl.AllowTabReorder AndAlso e.Button = Windows.Forms.MouseButtons.Left Then
                Dim t As TabPage = GetTabPage(DirectCast(sender, TabPage), e.X, e.Y)
                If t IsNot Nothing Then
                    ' swap the tabpages
                    IndexOf(t) = IndexOf(DirectCast(sender, TabPage))
                End If
            End If
        End Sub

        Private Function GetTabPage(ByVal TabPage As TabPage, ByVal x As Integer, ByVal y As Integer) As TabPage
            For i As Integer = 0 To List.Count - 1
                If DirectCast(List(i), TabPage) IsNot TabPage AndAlso DirectCast(List(i), TabPage).TabVisible Then
                    If DirectCast(List(i), TabPage).RectangleToScreen(DirectCast(List(i), TabPage).ClientRectangle).Contains(TabPage.PointToScreen(New Point(x, y))) Then
                        Return DirectCast(List(i), TabPage)
                    End If
                End If
            Next
            Return Nothing
        End Function

    End Class

#End Region

    <Description("Gets or sets the specified alignment for the control.")> _
    Public Enum TabAlignment
        Top = 0
        Bottom = 1
    End Enum

    <Description("Gets or sets the specified direction for the control.")> _
    Public Enum FlowDirection
        LeftToRight = 0
        RightToLeft = 2
    End Enum

    Public Enum Weight
        Soft = 2
        Medium = 3
        Strong = 4
        Strongest = 5
    End Enum

    Private AddingPage As Boolean = False
    Private LeftOffset As Integer = 3
    Private IsDelete As Boolean = False
    Private Background As New System.Windows.Forms.Panel
    Private WithEvents Items As New TabPageCollection(Me)
    Private m_TabsDirection As FlowDirection = FlowDirection.LeftToRight
    Private m_TabMaximumWidth As Integer = 200
    Private m_tabMinimumWidth As Integer = 100
    Private m_BackLowColor As Color
    Private m_BackHighColor As Color
    Private m_BorderColor As Color
    Private m_TabBackHighColor As Color
    Private m_TabBackLowColor As Color
    Private m_TabBackHighColorDisabled As Color
    Private m_TabBackLowColorDisabled As Color
    Private m_BorderColorDisabled As Color
    Private m_ForeColorDisabled As Color
    Private m_TopSeparator As Boolean = True
    Private m_TabTop As Integer = 3
    Private m_TabHeight As Integer = 28
    Private m_TabOffset As Integer = 3
    Private m_TabPadLeft As Integer = 5
    Private m_TabPadRight As Integer = 5
    Private m_TabSmoothingMode = Drawing2D.SmoothingMode.None
    Private m_TabIconSize As Size = New Size(16, 16)
    Private m_Alignment As TabAlignment = TabAlignment.Top
    Private m_FontBoldOnSelect As Boolean = True
    Private m_HotTrack As Boolean = True
    Private m_TabCloseButtonSize As Size = New Size(17, 17)
    Private m_TabCloseButtonVisible As Boolean = True
    Private m_TabCloseButtonImage As Image
    Private m_TabCloseButtonImageHot As Image
    Private m_TabCloseButtonImageDisabled As Image
    Private m_TabCloseButtonBackHighColor As Color
    Private m_TabCloseButtonBackLowColor As Color
    Private m_TabCloseButtonBorderColor As Color
    Private m_TabCloseButtonForeColor As Color
    Private m_TabCloseButtonBackHighColorDisabled As Color
    Private m_TabCloseButtonBackLowColorDisabled As Color
    Private m_TabCloseButtonBorderColorDisabled As Color
    Private m_TabCloseButtonForeColorDisabled As Color
    Private m_TabCloseButtonBackHighColorHot As Color
    Private m_TabCloseButtonBackLowColorHot As Color
    Private m_TabCloseButtonBorderColorHot As Color
    Private m_TabCloseButtonForeColorHot As Color
    Private m_AllowTabReorder As Boolean = True
    Private m_TabGlassGradient As Boolean = False
    Private m_TabBorderEnhanced As Boolean = False
    Private m_RenderMode As ToolStripRenderMode
    Private m_ContextMenuRenderer As ToolStripRenderer
    Private m_TabBorderEnhanceWeight As Weight = Weight.Medium

    Friend Shadows ReadOnly defaultPadding As Padding = New Padding(0, 0, 0, 0)
    Friend ReadOnly defaultBackLowColor As Color = SystemColors.ControlLightLight
    Friend ReadOnly defaultBackHighColor As Color = SystemColors.Control
    Friend ReadOnly defaultBorderColor As Color = SystemColors.ControlDarkDark
    Friend ReadOnly defaultTabBackHighColor As Color = SystemColors.Window
    Friend ReadOnly defaultTabBackLowColor As Color = SystemColors.Control
    Friend ReadOnly defaultTabBackHighColorDisabled As Color = SystemColors.Control
    Friend ReadOnly defaultTabBackLowColorDisabled As Color = SystemColors.ControlDark
    Friend ReadOnly defaultBorderColorDisabled As Color = SystemColors.ControlDark
    Friend ReadOnly defaultForeColorDisabled As Color = SystemColors.ControlText
    Friend ReadOnly defaultControlButtonBackHighColor As Color = SystemColors.GradientInactiveCaption
    Friend ReadOnly defaultControlButtonBackLowColor As Color = SystemColors.GradientInactiveCaption
    Friend ReadOnly defaultControlButtonBorderColor As Color = SystemColors.HotTrack
    Friend ReadOnly defaultControlButtonForeColor As Color = SystemColors.ControlText
    Friend ReadOnly defaultTabCloseButtonSize As Size = New Size(17, 17)
    Friend ReadOnly defaultTabIconSize As Size = New Size(16, 16)
    Friend ReadOnly defaultTabCloseButtonBackHighColor As Color = System.Drawing.Color.IndianRed
    Friend ReadOnly defaultTabCloseButtonBackHighColorDisabled As Color = System.Drawing.Color.LightGray
    Friend ReadOnly defaultTabCloseButtonBackHighColorHot As Color = System.Drawing.Color.LightCoral
    Friend ReadOnly defaultTabCloseButtonBackLowColor As Color = System.Drawing.Color.Firebrick
    Friend ReadOnly defaultTabCloseButtonBackLowColorDisabled As Color = System.Drawing.Color.DarkGray
    Friend ReadOnly defaultTabCloseButtonBackLowColorHot As Color = System.Drawing.Color.IndianRed
    Friend ReadOnly defaultTabCloseButtonBorderColor As Color = System.Drawing.Color.DarkRed
    Friend ReadOnly defaultTabCloseButtonBorderColorDisabled As Color = System.Drawing.Color.Gray
    Friend ReadOnly defaultTabCloseButtonBorderColorHot As Color = System.Drawing.Color.Firebrick
    Friend ReadOnly defaultTabCloseButtonForeColor As Color = System.Drawing.Color.White
    Friend ReadOnly defaultTabCloseButtonForeColorDisabled As Color = System.Drawing.Color.White
    Friend ReadOnly defaultTabCloseButtonForeColorHot As Color = System.Drawing.Color.White
    Friend ReadOnly defaultRenderMode As ToolStripRenderMode = ToolStripRenderMode.ManagerRenderMode

    <Description("Occurs when the Tab Page requests the tab region.")> _
    Public Event GetTabRegion(ByVal sender As Object, ByVal e As GetTabRegionEventArgs)
    <Description("Occurs when the Tab Background has been painted.")> _
    Public Event TabPaintBackground(ByVal sender As Object, ByVal e As TabPaintEventArgs)
    <Description("Occurs when the Tab Border has been painted.")> _
    Public Event TabPaintBorder(ByVal sender As Object, ByVal e As TabPaintEventArgs)
    <Description("Occurs when the current tab changes.")> _
    Public Event TabPageChanged(ByVal sender As Object, ByVal e As EventArgs)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        Me.SuspendLayout()
        'Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        'Me.SetStyle(ControlStyles.UserPaint, True)
        Me.SetStyle(ControlStyles.SupportsTransparentBackColor, True)
        Background.BackColor = SystemColors.AppWorkspace
        Background.BorderStyle = Windows.Forms.BorderStyle.Fixed3D
        Background.Dock = DockStyle.Fill
        Me.Controls.Add(Background)
        Background.BringToFront()
        ResetBackLowColor()
        ResetBackHighColor()
        ResetBorderColor()
        ResetTabBackHighColor()
        ResetTabBackLowColor()
        ResetTabBackHighColorDisabled()
        ResetTabBackLowColorDisabled()
        ResetBorderColorDisabled()
        ResetForeColorDisabled()
        ResetControlButtonBackHighColor()
        ResetControlButtonBackLowColor()
        ResetControlButtonBorderColor()
        ResetControlButtonForeColor()
        ResetTabCloseButtonBackHighColor()
        ResetTabCloseButtonBackLowColor()
        ResetTabCloseButtonBorderColor()
        ResetTabCloseButtonForeColor()
        ResetTabCloseButtonBackHighColorDisabled()
        ResetTabCloseButtonBackLowColorDisabled()
        ResetTabCloseButtonBorderColorDisabled()
        ResetTabCloseButtonForeColorDisabled()
        ResetTabCloseButtonBackHighColorHot()
        ResetTabCloseButtonBackLowColorHot()
        ResetTabCloseButtonBorderColorHot()
        ResetTabCloseButtonForeColorHot()
        ResetPadding()
        ResetTabCloseButtonSize()
        ResetTabIconSize()
        ResetRenderMode()
        AdjustHeight()
        DropButton.rendermode = Me.RenderMode
        CloseButton.rendermode = Me.RenderMode
        Me.ResumeLayout()
    End Sub

    <Browsable(False)> _
    Public ReadOnly Property SelectedForm() As Object
        Get
            If pnlBottom.Controls.Count > 0 Then
                Return pnlBottom.Controls(0)
            Else
                Return Nothing
            End If
        End Get
    End Property

    <Browsable(True), Category("Layout"), DefaultValue(FlowDirection.LeftToRight), Description("Gets or sets the the direction which the tabs are drawn.")> _
    Public Property TabsDirection() As FlowDirection
        Get
            Return m_TabsDirection
        End Get
        Set(ByVal value As FlowDirection)
            m_TabsDirection = value
            SelectItem(Nothing)
        End Set
    End Property

    <Browsable(True), Category("Appearance"), DefaultValue(False), Description("Gets or sets if the tab background will paint with glass style.")> _
    Public Property TabGlassGradient() As Boolean
        Get
            Return m_TabGlassGradient
        End Get
        Set(ByVal Value As Boolean)
            m_TabGlassGradient = Value
            For Each t As TabPage In TabPages
                t.GlassGradient = Value
            Next
        End Set
    End Property

    <Browsable(True), Category("Appearance"), DefaultValue(False), Description("Gets or sets if the tab border will paint with enhanced style.")> _
    Public Property TabBorderEnhanced() As Boolean
        Get
            Return m_TabBorderEnhanced
        End Get
        Set(ByVal Value As Boolean)
            m_TabBorderEnhanced = Value
            For Each t As TabPage In TabPages
                t.BorderEnhanced = Value
            Next
        End Set
    End Property

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the starting color of the Background linear gradient for the tab close button.")> _
    Public Property TabCloseButtonBackHighColor() As System.Drawing.Color
        Get
            Return m_TabCloseButtonBackHighColor
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonBackHighColor = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonBackHighColor() As Boolean
        Return m_TabCloseButtonBackHighColor <> Me.defaultTabCloseButtonBackHighColor
    End Function

    Friend Sub ResetTabCloseButtonBackHighColor()
        m_TabCloseButtonBackHighColor = Me.defaultTabCloseButtonBackHighColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the ending color of the Background linear gradient for the tab close button.")> _
    Public Property TabCloseButtonBackLowColor() As System.Drawing.Color
        Get
            Return m_TabCloseButtonBackLowColor
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonBackLowColor = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonBackLowColor() As Boolean
        Return m_TabCloseButtonBackLowColor <> Me.defaultTabCloseButtonBackLowColor
    End Function

    Friend Sub ResetTabCloseButtonBackLowColor()
        m_TabCloseButtonBackLowColor = Me.defaultTabCloseButtonBackLowColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the border color for the tab close button.")> _
    Public Property TabCloseButtonBorderColor() As System.Drawing.Color
        Get
            Return m_TabCloseButtonBorderColor
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonBorderColor = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonBorderColor() As Boolean
        Return m_TabCloseButtonBorderColor <> Me.defaultTabCloseButtonBorderColor
    End Function

    Friend Sub ResetTabCloseButtonBorderColor()
        m_TabCloseButtonBorderColor = Me.defaultTabCloseButtonBorderColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the fore color for the tab close button.")> _
    Public Property TabCloseButtonForeColor() As System.Drawing.Color
        Get
            Return m_TabCloseButtonForeColor
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonForeColor = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonForeColor() As Boolean
        Return m_TabCloseButtonForeColor <> Me.defaultTabCloseButtonForeColor
    End Function

    Friend Sub ResetTabCloseButtonForeColor()
        m_TabCloseButtonForeColor = Me.defaultTabCloseButtonForeColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the starting color of the Background linear gradient for the disabled tab close button.")> _
    Public Property TabCloseButtonBackHighColorDisabled() As System.Drawing.Color
        Get
            Return m_TabCloseButtonBackHighColorDisabled
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonBackHighColorDisabled = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonBackHighColorDisabled() As Boolean
        Return m_TabCloseButtonBackHighColorDisabled <> Me.defaultTabCloseButtonBackHighColorDisabled
    End Function

    Friend Sub ResetTabCloseButtonBackHighColorDisabled()
        m_TabCloseButtonBackHighColorDisabled = Me.defaultTabCloseButtonBackHighColorDisabled
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the ending color of the Background linear gradient for the disabled tab close button.")> _
    Public Property TabCloseButtonBackLowColorDisabled() As System.Drawing.Color
        Get
            Return m_TabCloseButtonBackLowColorDisabled
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonBackLowColorDisabled = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonBackLowColorDisabled() As Boolean
        Return m_TabCloseButtonBackLowColorDisabled <> Me.defaultTabCloseButtonBackLowColorDisabled
    End Function

    Friend Sub ResetTabCloseButtonBackLowColorDisabled()
        m_TabCloseButtonBackLowColorDisabled = Me.defaultTabCloseButtonBackLowColorDisabled
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the border color for the disabled tab close button.")> _
    Public Property TabCloseButtonBorderColorDisabled() As System.Drawing.Color
        Get
            Return m_TabCloseButtonBorderColorDisabled
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonBorderColorDisabled = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonBorderColorDisabled() As Boolean
        Return m_TabCloseButtonBorderColorDisabled <> Me.defaultTabCloseButtonBorderColorDisabled
    End Function

    Friend Sub ResetTabCloseButtonBorderColorDisabled()
        m_TabCloseButtonBorderColorDisabled = Me.defaultTabCloseButtonBorderColorDisabled
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the disabled fore color for the tab close button.")> _
    Public Property TabCloseButtonForeColorDisabled() As System.Drawing.Color
        Get
            Return m_TabCloseButtonForeColorDisabled
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonForeColorDisabled = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonForeColorDisabled() As Boolean
        Return m_TabCloseButtonForeColorDisabled <> Me.defaultTabCloseButtonForeColorDisabled
    End Function

    Friend Sub ResetTabCloseButtonForeColorDisabled()
        m_TabCloseButtonForeColorDisabled = Me.defaultTabCloseButtonForeColorDisabled
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the starting color of the Background linear gradient for the Hot tab close button.")> _
    Public Property TabCloseButtonBackHighColorHot() As System.Drawing.Color
        Get
            Return m_TabCloseButtonBackHighColorHot
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonBackHighColorHot = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonBackHighColorHot() As Boolean
        Return m_TabCloseButtonBackHighColorHot <> Me.defaultTabCloseButtonBackHighColorHot
    End Function

    Friend Sub ResetTabCloseButtonBackHighColorHot()
        m_TabCloseButtonBackHighColorHot = Me.defaultTabCloseButtonBackHighColorHot
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the ending color of the Background linear gradient for the Hot tab close button.")> _
    Public Property TabCloseButtonBackLowColorHot() As System.Drawing.Color
        Get
            Return m_TabCloseButtonBackLowColorHot
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonBackLowColorHot = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonBackLowColorHot() As Boolean
        Return m_TabCloseButtonBackLowColorHot <> Me.defaultTabCloseButtonBackLowColorHot
    End Function

    Friend Sub ResetTabCloseButtonBackLowColorHot()
        m_TabCloseButtonBackLowColorHot = Me.defaultTabCloseButtonBackLowColorHot
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the border color for the Hot tab close button.")> _
    Public Property TabCloseButtonBorderColorHot() As System.Drawing.Color
        Get
            Return m_TabCloseButtonBorderColorHot
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonBorderColorHot = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonBorderColorHot() As Boolean
        Return m_TabCloseButtonBorderColorHot <> Me.defaultTabCloseButtonBorderColorHot
    End Function

    Friend Sub ResetTabCloseButtonBorderColorHot()
        m_TabCloseButtonBorderColorHot = Me.defaultTabCloseButtonBorderColorHot
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the Hot fore color for the tab close button.")> _
    Public Property TabCloseButtonForeColorHot() As System.Drawing.Color
        Get
            Return m_TabCloseButtonForeColorHot
        End Get
        Set(ByVal Value As Color)
            m_TabCloseButtonForeColorHot = Value
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonForeColorHot() As Boolean
        Return m_TabCloseButtonForeColorHot <> Me.defaultTabCloseButtonForeColorHot
    End Function

    Friend Sub ResetTabCloseButtonForeColorHot()
        m_TabCloseButtonForeColorHot = Me.defaultTabCloseButtonForeColorHot
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the tab close button image.")> _
    Public Property TabCloseButtonImage() As Image
        Get
            Return m_TabCloseButtonImage
        End Get
        Set(ByVal value As Image)
            m_TabCloseButtonImage = value
            For Each t As TabPage In TabPages
                t.CloseButtonImage = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Appearance"), Description("Gets or sets the tab close button image in hot state.")> _
    Public Property TabCloseButtonImageHot() As Image
        Get
            Return m_TabCloseButtonImageHot
        End Get
        Set(ByVal value As Image)
            m_TabCloseButtonImageHot = value
            For Each t As TabPage In TabPages
                t.CloseButtonImageHot = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Appearance"), Description("Gets or sets the tab close button image in disabled state.")> _
    Public Property TabCloseButtonImageDisabled() As Image
        Get
            Return m_TabCloseButtonImageDisabled
        End Get
        Set(ByVal value As Image)
            m_TabCloseButtonImageDisabled = value
            For Each t As TabPage In TabPages
                t.CloseButtonImageDisabled = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Layout"), DefaultValue(True), Description("Gets or sets whether the tab close button is visble or not.")> _
    Public Property TabCloseButtonVisible() As Boolean
        Get
            Return m_TabCloseButtonVisible
        End Get
        Set(ByVal value As Boolean)
            m_TabCloseButtonVisible = value
            For Each t As TabPage In TabPages
                t.CloseButtonVisible = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Appearance"), Description("Gets or sets the size of the icon displayed at the tab.")> _
    Public Property TabIconSize() As Size
        Get
            Return m_TabIconSize
        End Get
        Set(ByVal value As Size)
            m_TabIconSize = value
            For Each t As TabPage In TabPages
                t.IconSize = value
            Next
        End Set
    End Property

    Friend Function ShouldSerializeTabIconSize() As Boolean
        Return m_TabIconSize <> Me.defaultTabIconSize
    End Function

    Friend Sub ResetTabIconSize()
        m_TabIconSize = Me.defaultTabIconSize
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the size of the close button displayed at the tab.")> _
    Public Property TabCloseButtonSize() As Size
        Get
            Return m_TabCloseButtonSize
        End Get
        Set(ByVal value As Size)
            m_TabCloseButtonSize = value
            For Each t As TabPage In TabPages
                t.CloseButtonSize = value
            Next
        End Set
    End Property

    Friend Function ShouldSerializeTabCloseButtonSize() As Boolean
        Return m_TabCloseButtonSize <> Me.defaultTabCloseButtonSize
    End Function

    Friend Sub ResetTabCloseButtonSize()
        m_TabCloseButtonSize = Me.defaultTabCloseButtonSize
    End Sub

    <Browsable(True), Category("Appearance"), DefaultValue(Drawing2D.SmoothingMode.None), Description("Specifies whether smoothing (antialiasing) is applied to lines and curves and the edges of filled areas.")> _
    Public Property SmoothingMode() As Drawing2D.SmoothingMode
        Get
            Return m_TabSmoothingMode
        End Get
        Set(ByVal value As Drawing2D.SmoothingMode)
            m_TabSmoothingMode = value
            For Each t As TabPage In TabPages
                t.SmoothingMode = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Layout"), DefaultValue(5), Description("Gets or sets the amount of space on the right side of the tab.")> _
    Public Property TabPadRight() As Integer
        Get
            Return m_TabPadRight
        End Get
        Set(ByVal value As Integer)
            m_TabPadRight = value
            For Each t As TabPage In TabPages
                t.PadRight = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Layout"), DefaultValue(5), Description("Gets or sets the amount of space on the left side of the tab.")> _
    Public Property TabPadLeft() As Integer
        Get
            Return m_TabPadLeft
        End Get
        Set(ByVal value As Integer)
            m_TabPadLeft = value
            For Each t As TabPage In TabPages
                t.PadLeft = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Layout"), DefaultValue(3), Description("Gets or sets the amount of space between the tabs.")> _
    Public Property TabOffset() As Integer
        Get
            Return m_TabOffset
        End Get
        Set(ByVal value As Integer)
            m_TabOffset = value
            ArrangeItems()
        End Set
    End Property

    <Browsable(True), Category("Layout"), DefaultValue(28), Description("Gets or sets the height of the tab.")> _
    Public Property TabHeight() As Integer
        Get
            Return m_TabHeight
        End Get
        Set(ByVal value As Integer)
            If m_TabHeight <> value Then
                m_TabHeight = value
                pnlTabs.Height = m_TabHeight
                pnlTabs.Top = pnlTop.Height - pnlTabs.Height
                AdjustHeight()
                For Each t As TabPage In TabPages
                    t.Height = value
                Next
            End If
        End Set
    End Property

    <Browsable(True), Category("Layout"), DefaultValue(3), Description("Gets or sets the distance between the top of the control and the top of the tab.")> _
    Public Property TabTop() As Integer
        Get
            Return m_TabTop
        End Get
        Set(ByVal value As Integer)
            m_TabTop = value
            AdjustHeight()
        End Set
    End Property

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the starting color of the Background linear gradient.")> _
    Public Property TabBackHighColor() As System.Drawing.Color
        Get
            Return m_TabBackHighColor
        End Get
        Set(ByVal Value As Color)
            m_TabBackHighColor = Value
            For Each t As TabPage In TabPages
                t.BackHighColor = Value
            Next
        End Set
    End Property

    Friend Function ShouldSerializeTabBackHighColor() As Boolean
        Return m_TabBackHighColor <> Me.defaultTabBackHighColor
    End Function

    Friend Sub ResetTabBackHighColor()
        m_TabBackHighColor = Me.defaultTabBackHighColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the ending color of the Background linear gradient.")> _
    Public Property TabBackLowColor() As Color
        Get
            Return m_TabBackLowColor
        End Get
        Set(ByVal Value As Color)
            m_TabBackLowColor = Value
            For Each t As TabPage In TabPages
                t.BackLowColor = Value
            Next
        End Set
    End Property

    Friend Function ShouldSerializeTabBackLowColor() As Boolean
        Return m_TabBackLowColor <> Me.defaultTabBackLowColor
    End Function

    Friend Sub ResetTabBackLowColor()
        m_TabBackLowColor = Me.defaultTabBackLowColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the starting color of the Background linear gradient for a non selected tab.")> _
        Public Property TabBackHighColorDisabled() As System.Drawing.Color
        Get
            Return m_TabBackHighColorDisabled
        End Get
        Set(ByVal Value As Color)
            m_TabBackHighColorDisabled = Value
            For Each t As TabPage In TabPages
                t.BackHighColorDisabled = Value
            Next
        End Set
    End Property

    Friend Function ShouldSerializeTabBackHighColorDisabled() As Boolean
        Return m_TabBackHighColorDisabled <> Me.defaultTabBackHighColorDisabled
    End Function

    Friend Sub ResetTabBackHighColorDisabled()
        m_TabBackHighColorDisabled = Me.defaultTabBackHighColorDisabled
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the ending color of the Background linear gradient for a non selected tab.")> _
    Public Property TabBackLowColorDisabled() As Color
        Get
            Return m_TabBackLowColorDisabled
        End Get
        Set(ByVal Value As Color)
            m_TabBackLowColorDisabled = Value
            For Each t As TabPage In TabPages
                t.BackLowColorDisabled = Value
            Next
        End Set
    End Property

    Friend Function ShouldSerializeTabBackLowColorDisabled() As Boolean
        Return m_TabBackLowColorDisabled <> Me.defaultTabBackLowColorDisabled
    End Function

    Friend Sub ResetTabBackLowColorDisabled()
        m_TabBackLowColorDisabled = Me.defaultTabBackLowColorDisabled
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the border color of the tab when not selected.")> _
    Public Property BorderColorDisabled() As Color
        Get
            Return m_BorderColorDisabled
        End Get
        Set(ByVal Value As Color)
            m_BorderColorDisabled = Value
            For Each t As TabPage In TabPages
                t.BorderColorDisabled = Value
            Next
        End Set
    End Property

    Friend Function ShouldSerializeBorderColorDisabled() As Boolean
        Return m_BorderColorDisabled <> Me.defaultBorderColorDisabled
    End Function

    Friend Sub ResetBorderColorDisabled()
        m_BorderColorDisabled = Me.defaultBorderColorDisabled
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the fore color of the tab when not selected.")> _
    Public Property ForeColorDisabled() As Color
        Get
            Return m_ForeColorDisabled
        End Get
        Set(ByVal Value As Color)
            m_ForeColorDisabled = Value
            For Each t As TabPage In TabPages
                t.ForeColorDisabled = Value
            Next
        End Set
    End Property

    Friend Function ShouldSerializeForeColorDisabled() As Boolean
        Return m_ForeColorDisabled <> Me.defaultForeColorDisabled
    End Function

    Friend Sub ResetForeColorDisabled()
        m_ForeColorDisabled = Me.defaultForeColorDisabled
    End Sub

    <Browsable(True), Category("Layout"), DefaultValue(100), Description("Gets or sets the minimum width for the tab.")> _
    Public Property TabMinimumWidth() As Integer
        Get
            Return m_tabMinimumWidth
        End Get
        Set(ByVal value As Integer)
            m_tabMinimumWidth = value
            For Each t As TabPage In TabPages
                t.MinimumWidth = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Layout"), DefaultValue(200), Description("Gets or sets the maximum width for the tab.")> _
    Public Property TabMaximumWidth() As Integer
        Get
            Return m_TabMaximumWidth
        End Get
        Set(ByVal value As Integer)
            m_TabMaximumWidth = value
            For Each t As TabPage In TabPages
                t.MaximumWidth = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Appearance"), DefaultValue(True), Description("Gets or sets whether the font on the selected tab is displayed in bold.")> _
    Public Property FontBoldOnSelect() As Boolean
        Get
            Return m_FontBoldOnSelect
        End Get
        Set(ByVal value As Boolean)
            m_FontBoldOnSelect = value
            For Each t As TabPage In TabPages
                t.FontBoldOnSelect = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Gets or sets a value indicating whether the control's tabs change in appearance when the mouse passes over them.")> _
    Public Property HotTrack() As Boolean
        Get
            Return m_HotTrack
        End Get
        Set(ByVal value As Boolean)
            m_HotTrack = value
            For Each t As TabPage In TabPages
                t.HotTrack = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Gets or sets a value indicating whether the user can reorder tabs draging.")> _
    Public Property AllowTabReorder() As Boolean
        Get
            Return m_AllowTabReorder
        End Get
        Set(ByVal value As Boolean)
            m_AllowTabReorder = value
        End Set
    End Property

    <Browsable(True), Category("Layout"), DefaultValue(False), Description("Gets or sets a value indicating whether the close button is displayed or not.")> _
    Public Property CloseButtonVisible() As Boolean
        Get
            Return CloseButton.Visible
        End Get
        Set(ByVal value As Boolean)
            If CloseButton.Visible <> value Then
                CloseButton.Visible = value
                SetControlsSizeLocation()
            End If
        End Set
    End Property

    <Browsable(True), Category("Layout"), DefaultValue(True), Description("Gets or sets a value indicating whether the drop button is displayed or not.")> _
    Public Property DropButtonVisible() As Boolean
        Get
            Return DropButton.Visible
        End Get
        Set(ByVal value As Boolean)
            If DropButton.Visible <> value Then
                DropButton.Visible = value
                SetControlsSizeLocation()
            End If
        End Set
    End Property

    <Browsable(True), Category("Appearance"), DefaultValue(True), Description("Gets or sets a value indicating whether a double line separator is displayed at the top of the control.")> _
    Public Property TopSeparator() As Boolean
        Get
            Return m_TopSeparator
        End Get
        Set(ByVal value As Boolean)
            m_TopSeparator = value
            AdjustHeight()
        End Set
    End Property

    <Browsable(True), Category("Behavior"), DefaultValue(TabAlignment.Top), Description("Gets or sets the area of the control (for example, along the top) where the tabs are aligned.")> _
    Public Property Alignment() As TabAlignment
        Get
            Return m_Alignment
        End Get
        Set(ByVal value As TabAlignment)
            m_Alignment = value
            AdjustHeight()
            PositionButtons()
            For Each t As TabPage In TabPages
                t.Alignment = value
            Next
        End Set
    End Property

    <Browsable(True), Category("Layout"), Description("Gets or sets the amount of space around the form on the control's tab pages.")> _
    Public Shadows Property Padding() As Padding
        Get
            Return pnlBottom.Padding
        End Get
        Set(ByVal value As Padding)
            pnlBottom.Padding = value
        End Set
    End Property

    Friend Function ShouldSerializePadding() As Boolean
        Return pnlBottom.Padding <> defaultPadding
    End Function

    Friend Sub ResetPadding()
        pnlBottom.Padding = defaultPadding
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the starting color of the Background linear gradient for the control button.")> _
    Public Property ControlButtonBackHighColor() As System.Drawing.Color
        Get
            Return DropButton.BackHighColor
        End Get
        Set(ByVal Value As Color)
            DropButton.BackHighColor = Value
            CloseButton.BackHighColor = Value
        End Set
    End Property

    Friend Function ShouldSerializeControlButtonBackHighColor() As Boolean
        Return DropButton.BackHighColor <> Me.defaultControlButtonBackHighColor
    End Function

    Friend Sub ResetControlButtonBackHighColor()
        DropButton.BackHighColor = Me.defaultControlButtonBackHighColor
        CloseButton.BackHighColor = Me.defaultControlButtonBackHighColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the ending color of the Background linear gradient for the control button.")> _
    Public Property ControlButtonBackLowColor() As Color
        Get
            Return DropButton.BackLowColor
        End Get
        Set(ByVal Value As Color)
            DropButton.BackLowColor = Value
            CloseButton.BackLowColor = Value
        End Set
    End Property

    Friend Function ShouldSerializeControlButtonBackLowColor() As Boolean
        Return DropButton.BackLowColor <> Me.defaultControlButtonBackLowColor
    End Function

    Friend Sub ResetControlButtonBackLowColor()
        DropButton.BackLowColor = Me.defaultControlButtonBackLowColor
        CloseButton.BackLowColor = Me.defaultControlButtonBackLowColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the border color for the control button.")> _
    Public Property ControlButtonBorderColor() As Color
        Get
            Return DropButton.BorderColor
        End Get
        Set(ByVal Value As Color)
            DropButton.BorderColor = Value
            CloseButton.BorderColor = Value
        End Set
    End Property

    Friend Function ShouldSerializeControlButtonBorderColor() As Boolean
        Return DropButton.BorderColor <> Me.defaultControlButtonBorderColor
    End Function

    Friend Sub ResetControlButtonBorderColor()
        DropButton.BorderColor = Me.defaultControlButtonBorderColor
        CloseButton.BorderColor = Me.defaultControlButtonBorderColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the ForeColor for the control button.")> _
    Public Property ControlButtonForeColor() As Color
        Get
            Return DropButton.ForeColor
        End Get
        Set(ByVal Value As Color)
            DropButton.ForeColor = Value
            CloseButton.ForeColor = Value
        End Set
    End Property

    Friend Function ShouldSerializeControlButtonForeColor() As Boolean
        Return DropButton.ForeColor <> Me.defaultControlButtonForeColor
    End Function

    Friend Sub ResetControlButtonForeColor()
        DropButton.ForeColor = Me.defaultControlButtonForeColor
        CloseButton.ForeColor = Me.defaultControlButtonForeColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the ending color of the Background linear gradient for the tabs region.")> _
    Public Property BackLowColor() As System.Drawing.Color
        Get
            Return m_BackLowColor
        End Get
        Set(ByVal Value As Color)
            m_BackLowColor = Value
            Invalidate()
        End Set
    End Property

    Friend Function ShouldSerializeBackLowColor() As Boolean
        Return m_BackLowColor <> Me.defaultBackLowColor
    End Function

    Friend Sub ResetBackLowColor()
        m_BackLowColor = Me.defaultBackLowColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the starting color of the Background linear gradient for the tabs region.")> _
    Public Property BackHighColor() As Color
        Get
            Return m_BackHighColor
        End Get
        Set(ByVal Value As Color)
            m_BackHighColor = Value
            Invalidate()
        End Set
    End Property

    Friend Function ShouldSerializeBackHighColor() As Boolean
        Return m_BackHighColor <> Me.defaultBackHighColor
    End Function

    Friend Sub ResetBackHighColor()
        m_BackHighColor = Me.defaultBackHighColor
    End Sub

    <Browsable(True), Category("Appearance"), Description("Gets or sets the System.Drawing.Color structure that represents the border color.")> _
    Public Property BorderColor() As Color
        Get
            Return m_BorderColor
        End Get
        Set(ByVal Value As Color)
            m_BorderColor = Value
            For Each t As TabPage In TabPages
                t.BorderColor = Value
            Next
            pnlTabs.Invalidate()
            pnlTop.Invalidate()
        End Set
    End Property

    Friend Function ShouldSerializeBorderColor() As Boolean
        Return m_BorderColor <> Me.defaultBorderColor
    End Function

    Friend Sub ResetBorderColor()
        m_BorderColor = Me.defaultBorderColor
    End Sub

    <Browsable(False), Description("Gets the collection of tab pages in this tab control.")> _
    Public ReadOnly Property TabPages() As TabPageCollection
        Get
            Return Items
        End Get
    End Property

    <Browsable(True), Category("Appearance"), Description("The painting style applied to the control.")> _
    Public Property RenderMode() As ToolStripRenderMode
        Get
            Return m_RenderMode
        End Get
        Set(ByVal value As ToolStripRenderMode)
            m_RenderMode = value
            DropButton.rendermode = value
            CloseButton.RenderMode = value
            WinMenu.RenderMode = value
            For Each t As TabPage In TabPages
                t.RenderMode = value
            Next
        End Set
    End Property

    <Browsable(False)> _
       Public Property MenuRenderer() As ToolStripRenderer
        Get
            Return m_ContextMenuRenderer
        End Get
        Set(ByVal value As ToolStripRenderer)
            m_ContextMenuRenderer = value
            WinMenu.Renderer = m_ContextMenuRenderer
        End Set
    End Property

    <Browsable(True), Category("Appearance"), DefaultValue(Weight.Medium), Description("The weight of the border.")> _
    Public Property TabBorderEnhanceWeight() As Weight
        Get
            Return m_TabBorderEnhanceWeight
        End Get
        Set(ByVal value As Weight)
            m_TabBorderEnhanceWeight = value
            For Each t As TabPage In TabPages
                t.BorderEnhanceWeight = value
            Next
        End Set
    End Property

    Friend Function ShouldSerializeRenderMode() As Boolean
        Return m_RenderMode <> Me.defaultRenderMode
    End Function

    Friend Sub ResetRenderMode()
        m_RenderMode = Me.defaultRenderMode
    End Sub

    Private Sub SetControlsSizeLocation()
        If DropButton.Visible And CloseButton.Visible Then
            pnlControls.Width = 43
        ElseIf DropButton.Visible Or CloseButton.Visible Then
            pnlControls.Width = 25
        Else
            pnlControls.Width = 3
        End If
        pnlControls.Left = Me.Width - pnlControls.Width
        CheckVisibility()
    End Sub

    Private Sub AdjustHeight()
        If Alignment = TabAlignment.Top Then
            pnlTop.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            pnlTop.Height = pnlTabs.Height + m_TabTop
            pnlTop.Top = IIf(m_TopSeparator, 2, 0)
            pnlTabs.Top = m_TabTop
            pnlBottom.Height = Me.Height - (pnlTop.Height + IIf(m_TopSeparator, 2, 0))
            pnlBottom.Top = Me.Height - pnlBottom.Height
        Else
            pnlTop.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
            pnlTop.Height = pnlTabs.Height + m_TabTop
            pnlTop.Top = Me.Height - pnlTop.Height
            pnlTabs.Top = 0
            pnlBottom.Height = Me.Height - (pnlTop.Height + IIf(m_TopSeparator, 2, 0))
            pnlBottom.Top = IIf(m_TopSeparator, 2, 0)
        End If
        pnlTop.Invalidate()
    End Sub

    Private Sub ArrangeItems()
        pnlTabs.SuspendLayout()
        If Items.Count = 0 Then Exit Sub
        Dim x As Integer = LeftOffset
        For i As Integer = 0 To Items.Count - 1
            Items(i).TabVisible = x + Items(i).Width < pnlControls.Left
            If Items(i).IsSelected And Not Items(i).TabVisible Then
                SelectItem(Items(i))
                Exit Sub
            End If
            Items(i).TabLeft = x
            x += Items(i).Width + m_TabOffset - 1
        Next
        If Not AddingPage Then
            If IsDelete Then
                For i As Integer = Items.Count - 1 To 0 Step -1
                    ShowTab(i)
                Next
                IsDelete = False
            Else
                For i As Integer = 0 To Items.Count - 1
                    ShowTab(i)
                Next
            End If
        End If
        pnlTabs.ResumeLayout()
    End Sub

    Private Sub CheckVisibility()
        If Items Is Nothing Then Exit Sub
        Dim x As Integer = LeftOffset
        For i As Integer = 0 To Items.Count - 1
            If Items(i).TabVisible <> (x + Items(i).Width < pnlControls.Left) Then
                If Items(i).TabVisible Then
                    Items(i).TabVisible = False
                    If Items(i).IsSelected Then
                        SelectItem(Items(i))
                        Exit Sub
                    Else
                        ShowTab(i)
                        Exit Sub
                    End If
                Else
                    Items(i).TabVisible = True
                    Items(i).TabLeft = x
                    ShowTab(i)
                End If
            ElseIf Not Items(i).TabVisible Then
                Exit Sub
            End If
            x += Items(i).Width + m_TabOffset - 1
            If x > pnlControls.Left Then Exit Sub
        Next
    End Sub

    Private Sub ShowTab(ByVal i As Integer)
        Items(i).Visible = Items(i).TabVisible
        If Items(0).Width <> 1 Then Items(i).Left = Items(i).TabLeft
    End Sub

    Private Sub SelectItem(ByVal TabPage As TabPage)
        For Each T As TabPage In TabPages
            T.IsSelected = False
        Next
        If TabPage IsNot Nothing Then
            For Each t As TabPage In TabPages
                If m_TabsDirection = FlowDirection.LeftToRight Then t.SendToBack() Else t.BringToFront()
            Next
            ' only the visible tab container has style doc.fill - when resize don't resize all tab containers
            TabPage.m_Form.Dock = DockStyle.Fill
      TabPage.m_Form.Visible = True
            TabPage.BringToFront()
            TabPage.m_Form.BringToFront()
            TabPage.m_Form.Focus()
            If pnlBottom.Controls.Count > 1 Then
                pnlBottom.Controls(1).Visible = False
                pnlBottom.Controls(1).Dock = DockStyle.None
            End If
            TabPage.IsSelected = True
            If Not TabPage.TabVisible And TabPages.IndexOf(TabPage) <> 0 Then
                TabPages.IndexOf(TabPage) = 0
            End If
        ElseIf pnlTabs.Controls.Count > 0 Then
            For Each t As TabPage In Items
                If t.m_Form.Equals(pnlBottom.Controls(0)) Then
                    t.Select()
                    Exit For
                End If
            Next
        End If
        RaiseEvent TabPageChanged(Me, New EventArgs)
    End Sub

    Private Sub TabControl_FontChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.FontChanged
        For Each t As TabPage In TabPages
            t.Font = Font
        Next
    End Sub

    Private Sub TabControl_ForeColorChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.ForeColorChanged
        For Each t As TabPage In TabPages
            t.ForeColor = ForeColor
        Next
    End Sub

    Private Sub TabControl_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If m_TopSeparator Then
            ControlPaint.DrawBorder3D(e.Graphics, New Rectangle(-2, 0, Me.Width + 4, -2))
        End If
    End Sub

    Private Sub TabControl_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        CheckVisibility()
    End Sub

    Private Sub pnlTop_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles pnlTop.SizeChanged
        PositionButtons()
    End Sub

    Private Sub PositionButtons()
        DropButton.Top = Math.Ceiling((pnlTop.Height - DropButton.Height) / 2) + IIf(Alignment = TabAlignment.Top And TopSeparator, -1, 0)
        CloseButton.Top = DropButton.Top
    End Sub

    Private Sub pnlTop_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pnlTop.Paint
        Dim Brush As New Drawing2D.LinearGradientBrush(New Point(0, 0), New Point(0, pnlTop.Height), RenderColors.BackHighColor(m_RenderMode, BackHighColor), RenderColors.BackLowColor(m_RenderMode, BackLowColor))
        e.Graphics.FillRectangle(Brush, New Rectangle(0, 0, pnlTop.Width, pnlTop.Height))
    Dim Pen As New Pen(RenderColors.BorderColor(m_RenderMode, BorderColorDisabled))
        If Alignment = TabAlignment.Top Then
            e.Graphics.DrawLine(Pen, 0, sender.Height - 1, sender.Width + 1, sender.Height - 1)
        Else
            e.Graphics.DrawLine(Pen, 0, 0, sender.Width + 1, 0)
        End If
        Pen.Dispose()
        Brush.Dispose()
    End Sub

    Private Sub DropButton_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles DropButton.MouseDown
        WinMenu.Items.Clear()
        For i As Integer = 0 To Items.Count - 1
            WinMenu.Items.Add(Items(i).MenuItem)
            AddHandler Items(i).MenuItem.Click, AddressOf MenuClick
        Next
        WinMenu.Show(pnlTop, pnlTop.Width - WinMenu.Width, pnlTop.Height - 1)
    End Sub

    Private Sub MenuClick(ByVal sender As Object, ByVal e As EventArgs)
        sender.Tag.Select()
    End Sub

    Private Sub CloseButton_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles CloseButton.MouseDown
        Items.SelectedTab.m_Form.Close()
    End Sub

    Private Sub Items_GetTabRegion(ByVal sender As Object, ByVal e As GetTabRegionEventArgs) Handles Items.GetTabRegion
        RaiseEvent GetTabRegion(sender, e)
    End Sub

    Private Sub Items_TabPaintBackground(ByVal sender As Object, ByVal e As TabPaintEventArgs) Handles Items.TabPaintBackground
        RaiseEvent TabPaintBackground(sender, e)
    End Sub

    Private Sub Items_TabPaintBorder(ByVal sender As Object, ByVal e As TabPaintEventArgs) Handles Items.TabPaintBorder
        RaiseEvent TabPaintBorder(sender, e)
    End Sub

    Public Sub SetColors(ByVal ColorTable As ProfessionalColorTable)
        BackHighColor = ColorTable.ToolStripGradientEnd
        BackLowColor = ColorTable.ToolStripGradientBegin
        BorderColor = ColorTable.GripDark
        BorderColorDisabled = ColorTable.SeparatorDark
        ControlButtonBackHighColor = ColorTable.ButtonSelectedGradientBegin
        ControlButtonBackLowColor = ColorTable.ButtonSelectedGradientEnd
        ControlButtonBorderColor = ColorTable.ButtonPressedBorder
        TabBackHighColor = ColorTable.MenuItemPressedGradientBegin
        TabBackLowColor = ColorTable.MenuItemPressedGradientEnd
        TabBackHighColorDisabled = ColorTable.ToolStripDropDownBackground
        TabBackLowColorDisabled = ColorTable.ToolStripGradientMiddle
        TabCloseButtonBackHighColor = Color.Transparent
        TabCloseButtonBackHighColorDisabled = Color.Transparent
        TabCloseButtonBackHighColorHot = Color.WhiteSmoke
        TabCloseButtonBackLowColor = Color.Transparent
        TabCloseButtonBackLowColorDisabled = Color.Transparent
        TabCloseButtonBackLowColorHot = Color.LightGray
        TabCloseButtonBorderColor = Color.Transparent
        TabCloseButtonBorderColorDisabled = Color.Transparent
        TabCloseButtonBorderColorHot = Color.Gray
        TabCloseButtonForeColor = Color.Gray
        TabCloseButtonForeColorDisabled = Color.Gray
        TabCloseButtonForeColorHot = Color.Firebrick
    End Sub

#Region " Obsolete properties "

    <EditorBrowsable(EditorBrowsableState.Never)> _
    <Browsable(False), Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Shadows Property BorderStyle() As BorderStyle
        Get
        End Get
        Set(ByVal value As BorderStyle)
        End Set
    End Property

#End Region

End Class

Friend Module Helper

    Friend Function CreateGlassGradientBrush(ByVal Rectangle As Rectangle, ByVal Color1 As Color, ByVal Color2 As Color) As Drawing2D.LinearGradientBrush
        Dim b As New Drawing2D.LinearGradientBrush(Rectangle, Color1, Color2, Drawing2D.LinearGradientMode.Vertical)
        Dim x As New Bitmap(1, Rectangle.Height)
        Dim g As Graphics = Graphics.FromImage(x)
        g.FillRectangle(b, New Rectangle(0, 0, 1, Rectangle.Height))
        Dim c As New Drawing2D.ColorBlend(4)
        c.Colors(0) = x.GetPixel(0, 0)
        c.Colors(1) = x.GetPixel(0, x.Height / 3)
        c.Colors(2) = x.GetPixel(0, x.Height - 1)
        c.Colors(3) = x.GetPixel(0, x.Height / 3)
        c.Positions(0) = 0
        c.Positions(1) = 0.335
        c.Positions(2) = 0.335
        c.Positions(3) = 1
        b.InterpolationColors = c
        g.Dispose()
        x.Dispose()
        Return b
    End Function

    Friend Class Colors

        Public Function BackHighColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return Color.Transparent
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.ToolStripGradientEnd
            Else
                Return ManagedColor
            End If
        End Function

        Public Function BackLowColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return Color.Transparent
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.ToolStripGradientBegin
            Else
                Return ManagedColor
            End If
        End Function

        Public Function BorderColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.ControlDarkDark
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.GripDark
            Else
                Return ManagedColor
            End If
        End Function

        Public Function BorderColorDisabled(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.ControlDark
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.SeparatorDark
            Else
                Return ManagedColor
            End If
        End Function

        Public Function ControlButtonBackHighColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.ButtonHighlight
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.ButtonSelectedGradientBegin
            Else
                Return ManagedColor
            End If
        End Function

        Public Function ControlButtonBackLowColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.ButtonHighlight
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.ButtonSelectedGradientEnd
            Else
                Return ManagedColor
            End If
        End Function

        Public Function ControlButtonBorderColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.HotTrack
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.ButtonPressedBorder
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabBackHighColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.Control
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.MenuItemPressedGradientBegin
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabBackLowColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.Control
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.MenuItemPressedGradientEnd
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabBackHighColorDisabled(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.Control
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.ToolStripDropDownBackground
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabBackLowColorDisabled(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.Control
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return ProfessionalColors.ToolStripGradientMiddle
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonBackHighColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return Color.Transparent
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.Transparent
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonBackHighColorDisabled(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return Color.Transparent
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.Transparent
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonBackHighColorHot(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.ButtonHighlight
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.WhiteSmoke
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonBackLowColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return Color.Transparent
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.Transparent
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonBackLowColorDisabled(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return Color.Transparent
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.Transparent
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonBackLowColorHot(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.ButtonHighlight
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.LightGray
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonBorderColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.ControlDark
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.Transparent
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonBorderColorDisabled(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.GrayText
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.Transparent
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonBorderColorHot(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.HotTrack
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.Gray
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonForeColor(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.ControlText
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.Gray
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonForeColorDisabled(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.GrayText
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.Gray
            Else
                Return ManagedColor
            End If
        End Function

        Public Function TabCloseButtonForeColorHot(ByVal RenderMode As ToolStripRenderMode, ByVal ManagedColor As Color) As Color
            If RenderMode = ToolStripRenderMode.System Then
                Return SystemColors.ControlText
            ElseIf RenderMode = ToolStripRenderMode.Professional Then
                Return Color.Firebrick
            Else
                Return ManagedColor
            End If
        End Function

    End Class

    Friend RenderColors As New Colors

End Module