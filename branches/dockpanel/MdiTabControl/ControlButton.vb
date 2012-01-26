Imports System.ComponentModel

Friend Class ControlButton

    Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        Me.SuspendLayout()
        Me.SetStyle(ControlStyles.SupportsTransparentBackColor, True)
        MyBase.BackColor = Color.Transparent
        ResetBackHighColor()
        ResetBackLowColor()
        ResetBorderColor()
        Me.ResumeLayout()
    End Sub

    Enum ButtonStyle
        Close
        Drop
    End Enum

    Private m_hot As Boolean = False
    Private m_BackHighColor As Color
    Private m_BackLowColor As Color
    Private m_BorderColor As Color
    Private m_style As ButtonStyle
    Private m_RenderMode As ToolStripRenderMode

    Private ReadOnly defaultBackHighColor As Color = SystemColors.ControlLightLight
    Private ReadOnly defaultBackLowColor As Color = SystemColors.ControlDark
    Private ReadOnly defaultBorderColor As Color = SystemColors.ControlDarkDark

    Friend Property RenderMode() As ToolStripRenderMode
        Get
            Return m_RenderMode
        End Get
        Set(ByVal value As ToolStripRenderMode)
            m_RenderMode = value
            Invalidate()
        End Set
    End Property

    Property Style() As ButtonStyle
        Get
            Return m_style
        End Get
        Set(ByVal value As ButtonStyle)
            m_style = value
        End Set
    End Property

    <Browsable(True), Category("Appearance")> _
    Public Property BackHighColor() As System.Drawing.Color
        Get
            Return m_BackHighColor
        End Get
        Set(ByVal Value As Color)
            m_BackHighColor = Value
        End Set
    End Property

    Public Function ShouldSerializeBackHighColor() As Boolean
        Return m_BackHighColor <> Me.defaultBackHighColor
    End Function

    Public Sub ResetBackHighColor()
        m_BackHighColor = Me.defaultBackHighColor
    End Sub

    <Browsable(True), Category("Appearance")> _
    Public Property BackLowColor() As Color
        Get
            Return m_BackLowColor
        End Get
        Set(ByVal Value As Color)
            m_BackLowColor = Value
        End Set
    End Property

    Public Function ShouldSerializeBackLowColor() As Boolean
        Return m_BackLowColor <> Me.defaultBackLowColor
    End Function

    Public Sub ResetBackLowColor()
        m_BackLowColor = Me.defaultBackLowColor
    End Sub

    <Browsable(True), Category("Appearance")> _
    Public Property BorderColor() As Color
        Get
            Return m_BorderColor
        End Get
        Set(ByVal Value As Color)
            m_BorderColor = Value
        End Set
    End Property

    Public Function ShouldSerializeBorderColor() As Boolean
        Return m_BorderColor <> Me.defaultBorderColor
    End Function

    Public Sub ResetBorderColor()
        m_BorderColor = Me.defaultBorderColor
    End Sub

    <DebuggerStepThrough()> _
    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        Dim DropPoints() As System.Drawing.Point = {New Point(0, 0), New Point(11, 0), New Point(5, 6)}
        Dim ClosePoints() As System.Drawing.Point = {New Point(0, 0), New Point(2, 0), New Point(5, 3), New Point(8, 0), New Point(10, 0), New Point(6, 4), New Point(10, 8), New Point(8, 8), New Point(5, 5), New Point(2, 8), New Point(0, 8), New Point(4, 4)}
        Dim rec As New Rectangle
        rec.Size = New Size(Me.Width - 1, Me.Height - 1)
        If m_hot Then
            e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            e.Graphics.FillRectangle(New Drawing2D.LinearGradientBrush(New Point(0, 0), New Point(0, Me.Height), RenderColors.ControlButtonBackHighColor(m_RenderMode, m_BackHighColor), RenderColors.ControlButtonBackLowColor(m_RenderMode, m_BackLowColor)), rec)
            e.Graphics.DrawRectangle(New Pen(RenderColors.ControlButtonBorderColor(m_RenderMode, m_BorderColor)), rec)
            e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.Default
        End If
        Dim g As New Drawing2D.GraphicsPath
        Dim m As New Drawing2D.Matrix
        Dim x As Integer = (Me.Width - 11) / 2
        Dim y As Integer = (Me.Height - 11) / 2 + 1
        If m_style = ButtonStyle.Drop Then
            e.Graphics.FillRectangle(New SolidBrush(ForeColor), x, y, 11, 2)
            g.AddPolygon(DropPoints)
            m.Translate(x, y + 3)
            g.Transform(m)
            e.Graphics.FillPolygon(New SolidBrush(ForeColor), g.PathPoints)
        Else
            g.AddPolygon(ClosePoints)
            m.Translate(x, y)
            g.Transform(m)
            e.Graphics.DrawPolygon(New Pen(ForeColor), g.PathPoints)
            e.Graphics.FillPolygon(New SolidBrush(ForeColor), g.PathPoints)
        End If
        g.Dispose()
        m.Dispose()
    End Sub

    Private Sub MdiTab_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MouseEnter
        m_hot = True
        Invalidate()
    End Sub

    Private Sub MdiTab_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MouseLeave
        m_hot = False
        Invalidate()
    End Sub

End Class
