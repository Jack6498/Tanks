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
    Dim pen As Pen = New Pen(Color.Black, 5)
    Dim pen1 As New Pen(Color.Green, 3)
    Dim brush1 As New SolidBrush(Color.SaddleBrown)
    Dim brush2 As New SolidBrush(Color.Yellow)
    Dim brush3 As New SolidBrush(Color.Red)
    Dim brush As SolidBrush = New SolidBrush(Color.FromArgb(50, Color.White))
    Dim path As New GraphicsPath()
#End Region 'contains all graphics object variables

#Region "Rendering Varables"
    Dim angle As Integer
    Dim velocity As Double
    Dim points(21) As PointF
    Dim tank, shot, sun, aimCircle As Rectangle
#End Region'contains Variables for calculations and rendering

#Region "game Variables"
    Dim animation As Boolean
    Dim r As Integer = 5 'x position of the tank shot for calcuations
    Dim tp As Integer = 1 'what position is the tank at
    Dim mousepos, tankPosAdjusted As New Point
    Shadows font As New Font("Arial", 16)
#End Region 'contains vaiables for game mechanics

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

#Region "setup timers and graphics"
        g = Me.CreateGraphics()
        bm = New Bitmap(Me.Width, Me.Height)
        g1 = Graphics.FromImage(bm)
        animation = False
        animationTimer.Enabled = True
        animationTimer.Stop()
        path.AddLines(points)
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
        shot = New Rectangle(New Point(points(tp).X, points(tp).Y), New Size(5, 5))
        sun = New Rectangle(New Point(-100, -100), New Size(200, 200))
        aimCircle = New Rectangle(New Point(tank.Location.X - 50, tank.Location.Y - 50), New Size(200, 200))

#End Region

#Region "Debug"
        'MessageBox.Show("y: " & points(i).Y & " x: " & points(i).Y & " location " & i)
        ' Next
#End Region


    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If (e.KeyCode = Keys.A) And (animation = False) Then
            tp -= 1

        ElseIf (e.KeyCode = Keys.D) And (animation = False) Then
            tp += 1
        ElseIf e.KeyCode = Keys.Space Then
            FrameTime.Stop()
            shot.Location = New Point(50, 50)
            r = 1
            animationTimer.Start()
            animation = True
        End If

#Region "keep Tank on screen"
        If (tp >= 0) And (tp <= points.Length) Then
            tank.Location = New Point(points(tp).X - (tank.Width / 2), points(tp).Y - (tank.Height))
        ElseIf (tp <= 0) Then
            tank.Location = New Point(points(1).X - (tank.Width / 2), points(1).Y - (tank.Height))
        ElseIf (tp >= points.Length) Then
            tank.Location = New Point(points(points.Length).X - (tank.Width / 2), points(points.Length).Y - (tank.Height))
        End If
#End Region


    End Sub



    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs) Handles animationTimer.Tick

        ShootAnimation()

        r += 2 'increment calcuated x position of shot (determines how many shot frames are calculated)

    End Sub

    Private Sub FrameTime_Tick(sender As Object, e As EventArgs) Handles FrameTime.Tick
        tankPosAdjusted = New Point(tank.Location.X + (tank.Width / 2), tank.Location.Y + (tank.Height / 2))
        aimCircle.Location = New Point(tank.Location.X - 100, tank.Location.Y - 100)
        velocity = Distance(tankPosAdjusted, mousepos)
        If velocity > 100 Then
            velocity = 100
        End If
        angle = CalculateAngle(mousepos, tankPosAdjusted)
        DrawOnTick()

    End Sub

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove
        If animation = False Then
            mousepos = e.Location
        End If

    End Sub





#Region "Game Methods"
    'animates the shot flight
    ''' <summary>
    ''' Animates the shot flying through the air
    ''' </summary>
    Private Sub ShootAnimation()

        'draw tank and ground
        g1.FillPolygon(brush1, points)

        g1.FillRectangle(brush3, tank)

#Region "calculate shot"

        If angle > 0 Then
                shot.Location = New Point(tank.Location.X + r, tank.Location.Y + calculateBallPath(90 + angle, velocity, r))
            Else
                shot.Location = New Point(tank.Location.X - r, tank.Location.Y + calculateBallPath(90 + angle, velocity, -r))
            End If

        g1.DrawEllipse(pen1, shot)
        g1.DrawPath(pen, path)

#End Region

        GetColor(shot.Location)
        If bm.GetPixel(shot.Location.X + 10, shot.Location.Y + 10) = Color.Black Then
            MessageBox.Show("works")
        End If
        'reset
        g.DrawImage(bm, 0, 0)


    End Sub

    'ground collision detection
    ''' <summary>
    ''' Checks if a pixel is a certain color
    ''' </summary>
    ''' <param name="pt"></param>
    Private Sub GetColor(pt As Point)
        bm.SetPixel(pt.X + 10, pt.Y + 10, Color.Black)



        Label1.Text = "x:" + pt.X.ToString + " Y:" + pt.Y.ToString()
    End Sub

    'calculates the path of the ball
    ''' <summary>
    ''' Calculates the shot's y coordinate given the angle, velocity, and x coordinate.
    ''' </summary>
    ''' <param name="angle"></param>
    ''' <param name="velocity"></param>
    ''' <param name="xcoor"></param>
    ''' <returns></returns>\
    ''' 
    Private Function calculateBallPath(angle As Double, velocity As Double, xcoor As Integer)
        Dim ycoor As Double
        Dim radians = angle * (Math.PI / 180)
        ycoor = (xcoor * Math.Tan(radians)) - ((9.8 * (xcoor ^ 2)) / (2 * (velocity) ^ 2 * (Math.Cos(radians) ^ 2)))
        Return -ycoor
    End Function

    'draws objects while aiming
    ''' <summary>
    ''' Draws objects during non-shooting gameplay
    ''' </summary>
    Private Sub DrawOnTick()

        g1.FillEllipse(brush2, sun)
        g1.FillPolygon(brush1, points)
        g1.DrawLines(pen, points)
        g1.FillRectangle(brush3, tank)
        g1.FillEllipse(brush, aimCircle)
        g1.DrawPath(pen, path)
        g1.DrawString("Angle: " + angle.ToString + " Power:" + velocity.ToString(), font, Brushes.Black, New Point(mousepos.X + 10, mousepos.Y + 10))

        'load bitmap onto form 
        g.DrawImage(bm, 0, 0)
        'clear bitmap to remove old frames
        g1.Clear(Color.Blue)
    End Sub

    'calculate angle
    ''' <summary>
    ''' Calculates angle based on Tank and Cursor Position
    ''' </summary>
    ''' <param name="mousepos"></param>
    ''' <param name="tankpos"></param>
    ''' <returns>Angle</returns>
    Private Function CalculateAngle(mousepos As Point, tankpos As Point)
        Dim horizontalDiff, VerticleDiff As Double
        Dim angle As Single
        horizontalDiff = mousepos.X - tankpos.X
        VerticleDiff = mousepos.Y - tankpos.Y
        angle = (Math.Atan2(horizontalDiff, VerticleDiff)) * (180 / Math.PI)

        Return angle
    End Function

    'Distance Formula
    ''' <summary>
    ''' Finds the distance between 2 points
    ''' </summary>
    ''' <param name="p1"></param>
    ''' <param name="p2"></param>
    ''' <returns>Distance between 2 points</returns>
    Private Function Distance(p1 As Point, p2 As Point)
        Dim dist As Single
        dist = Math.Sqrt(((p1.X - p2.X) ^ 2) + ((p1.Y - p2.Y) ^ 2))
        Return dist
    End Function
#End Region
End Class
