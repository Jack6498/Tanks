Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Numerics
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Devices
Imports SimplexNoise


Public Class Form1
    Dim g, g1 As Graphics
    Dim bm As Bitmap
    Dim pen As Pen = New Pen(Color.Green, 5)
    Dim brush1 As New SolidBrush(Color.SaddleBrown)
    Dim brush2 As New SolidBrush(Color.Yellow)
    Dim brush As SolidBrush = New SolidBrush(Color.FromArgb(50, Color.White))
    Dim points(21) As PointF
    Dim tank, shot As Rectangle
    Dim animation As Boolean
    Dim angle As Integer
    Dim velocity As Double

    Dim animationdone As Boolean
    Dim r As Integer = 5
    Dim mouse As Mouse
    Dim path As GraphicsPath = New GraphicsPath()
    Dim noise As Noise
    'this project uses a manual double buffered graphics method that makes everything look smoother
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'set points for the ground line

        'set the tank position and size
        tank = New Rectangle(New Point(50, Me.Height - 120), New Size(New Point(30, 20)))
        'set projectile position and size

        'make graphics and bitmap to be loaded
        g = Me.CreateGraphics()
        bm = New Bitmap(Me.Width, Me.Height)
        g1 = Graphics.FromImage(bm)
        animation = False
        animationTimer.Enabled = True
        animationTimer.Stop()
        Dim NumOfPoints As Integer = 10

        'generates the terrain based on simplex noise
        Noise.Seed = 209323094
        Dim width As Integer = points.Length - 1
        Dim x(width) As Single
        Dim num = (Me.Width / width)
        x = Noise.Calc1D(width, 0.3F)
        For i As Integer = 2 To width - 1
            points(i) = New Point(num * i, Me.Height - x(i))

        Next
        'test shot peramaters
        angle = 66.4
        velocity = 45


        Dim ep As Integer = 0

        Dim scale As Integer = 2

        points(20) = New PointF(Me.Width, Me.Height - 100)
        points(21) = New PointF(Me.Width, Me.Height)
        points(1) = New PointF(0, Me.Height - 100)
        points(0) = New PointF(0, Me.Height)
        path.AddLines(points)
        shot = New Rectangle(New Point(6, 6), New Size(5, 5))
        ' For i As Integer = 0 To points.Length - 1
        'MessageBox.Show("y: " & points(i).Y & " x: " & points(i).Y & " location " & i)
        ' Next
    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If (e.KeyCode = Keys.A) And (animation = False) Then
            tank.Location = New Point(tank.Location.X - 20, tank.Location.Y)
            MessageBox.Show("hello")
        ElseIf (e.KeyCode = Keys.D) And (animation = False) Then
            tank.Location = New Point(tank.Location.X + 20, tank.Location.Y)
        ElseIf e.KeyCode = Keys.Space Then
            FrameTime.Stop()
            shot.Location = New Point(1, 1)
            r = 1
            animationTimer.Start()
            animation = True
        End If

    End Sub



    Private Sub animationTimer_Tick(sender As Object, e As EventArgs) Handles animationTimer.Tick

        shootAnimation()
        r += 1
    End Sub

    Private Sub FrameTime_Tick(sender As Object, e As EventArgs) Handles FrameTime.Tick
        drawOnTick()
    End Sub

    Private Sub Shoot_Click(sender As Object, e As EventArgs)
        animation = True

        MessageBox.Show(shot.Location.Y)
        FrameTime.Stop()
        animationTimer.Start()

    End Sub



    Private Sub shootAnimation()

        g1.DrawPolygon(pen, points)
        g1.DrawRectangle(pen, tank)


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


        g.DrawImage(bm, 0, 0)
        g1.Clear(Color.White)

    End Sub
    'game timer for when things are not being shot
    Private Sub drawOnTick()
        'drawing objects to the bitmap 
        g1.FillEllipse(brush2, New RectangleF(New Point(-100, -100), New Size(200, 200)))
        g1.FillPolygon(brush1, points)
        g1.DrawLines(pen, points)
        g1.DrawRectangle(pen, tank)
        g1.FillEllipse(brush, New RectangleF(New Point(tank.Location.X - (50), tank.Location.Y - (50)), New Size(100, 100)))


        'load bitmap onto form 
        g.DrawImage(bm, 0, 0)
        'clearing bitmap to remove old frames
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
