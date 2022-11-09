Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Numerics
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Devices
Imports SimplexNoise


Public Class Form1
#Region "graphics"
    Dim g, g1 As Graphics
    Dim bm As Bitmap
    Dim pen As Pen = New Pen(Color.Green, 5)
    Dim brush1 As New SolidBrush(Color.SaddleBrown)
    Dim brush2 As New SolidBrush(Color.Yellow)
    Dim brush As SolidBrush = New SolidBrush(Color.FromArgb(50, Color.White))
#End Region 'contains all graphics object variables

#Region "Rendering Varables"
    Dim angle As Integer
    Dim velocity As Double
    Dim points(21) As PointF
    Dim tank, shot As Rectangle
#End Region'contains Variables for calculations and rendering

#Region "game Variables"
    Dim animation As Boolean
    Dim r As Integer = 5 'x position of the tank shot for calcuations
    Dim tp As Integer 'what position is the tank at
#End Region 'contains vaiables for game mechanics


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
#Region "setup timers and graphics"
        g = Me.CreateGraphics()
        bm = New Bitmap(Me.Width, Me.Height)
        g1 = Graphics.FromImage(bm)
        animation = False
        animationTimer.Enabled = True
        animationTimer.Stop()
#End Region

#Region "ground Generation"
        Noise.Seed = 209323094
        Dim width As Integer = points.Length - 1
        Dim x(width) As Single
        Dim num = (Me.Width / width)
        x = Noise.Calc1D(width, 0.3F)
        For i As Integer = 2 To width - 1
            points(i) = New Point(num * i, Me.Height - x(i))

        Next
        points(20) = New PointF(Me.Width, Me.Height - 100)
        points(21) = New PointF(Me.Width, Me.Height)
        points(1) = New PointF(0, Me.Height - 100)
        points(0) = New PointF(0, Me.Height)
#End Region

#Region "init positons"
        tank = New Rectangle(New Point(50, Me.Height - 120), New Size(New Point(30, 20)))
        shot = New Rectangle(New Point(6, 6), New Size(5, 5))
#End Region

        'MessageBox.Show("y: " & points(i).Y & " x: " & points(i).Y & " location " & i)
        ' Next

    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If (e.KeyCode = Keys.A) And (animation = False) Then
            tp += 1
            MessageBox.Show("hello")
        ElseIf (e.KeyCode = Keys.D) And (animation = False) Then
            tp -= 1
        ElseIf e.KeyCode = Keys.Space Then
            FrameTime.Stop()
            shot.Location = New Point(1, 1)
            r = 1
            animationTimer.Start()
            animation = True
        End If

        tank.Location = New Point(points(tp).X, points(tp).Y)
    End Sub



    Private Sub animationTimer_Tick(sender As Object, e As EventArgs) Handles animationTimer.Tick

        shootAnimation()

        r += 1 'increment calcuated x position of shot

    End Sub

    Private Sub FrameTime_Tick(sender As Object, e As EventArgs) Handles FrameTime.Tick
        drawOnTick()
    End Sub

    Private Sub shootAnimation()

        'draw tank and ground
        g1.DrawPolygon(pen, points)
        g1.DrawRectangle(pen, tank)

#Region "calculate shot"
        If shot.Location.Y < points(1).Y Then
            shot.Location = New Point(tank.Location.X + r, tank.Location.Y + calculateBallPath(angle, velocity, r))
            Label1.Text = shot.Location.Y
            g1.DrawEllipse(pen, shot)

        Else
            animationTimer.Stop()
            g1.Clear(Color.White)
            FrameTime.Start()
            animation = False

        End If
#End Region

        'reset
        g.DrawImage(bm, 0, 0)
        g1.Clear(Color.Blue)

    End Sub
    'game timer for when things are not being shot
    Private Sub drawOnTick()

#Region "draw Objects to bitmap"
        'drawing objects to the bitmap 
        g1.FillEllipse(brush2, New RectangleF(New Point(-100, -100), New Size(200, 200)))
        g1.FillPolygon(brush1, points)
        g1.DrawLines(pen, points)
        g1.DrawRectangle(pen, tank)
        g1.FillEllipse(brush, New RectangleF(New Point(tank.Location.X - (50), tank.Location.Y - (50)), New Size(100, 100)))
#End Region

        'load bitmap onto form 
        g.DrawImage(bm, 0, 0)
        'clear bitmap to remove old frames
        g1.Clear(Color.Blue)
    End Sub

    Private Function calculateBallPath(angle As Double, velocity As Double, xcoor As Integer)
        Dim ycoor As Double
        Dim radians = angle * (Math.PI / 180)
        ycoor = (xcoor * Math.Tan(radians)) - ((9.8 * (xcoor ^ 2)) / (2 * (velocity) ^ 2 * (Math.Cos(radians) ^ 2)))
        Return -ycoor
    End Function

    'there is no way to do polygon collision detection natively in vb so I am making my own
    Private Sub PolygonDetection(ptstart As Point, ptend As Point)
        Dim lineVec As Vector2 = New Vector2(ptend.X - ptstart.X, ptend.Y - ptstart.Y)
        Dim lineLength As Double = lineVec.Length
        lineVec /= lineLength
        Dim perpLineVec As Vector2 = New Vector2(-lineVec.Y, lineVec.X)

    End Sub
End Class
