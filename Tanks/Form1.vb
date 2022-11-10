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
        aimCircle = New Rectangle(New Point(tank.Location.X - 50, tank.Location.Y - 50), New Size(100, 100))

#End Region

#Region "Debug"
        'MessageBox.Show("y: " & points(i).Y & " x: " & points(i).Y & " location " & i)
        ' Next
#End Region


    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If (e.KeyCode = Keys.A) And (animation = False) Then
            tp += 1

        ElseIf (e.KeyCode = Keys.D) And (animation = False) Then
            tp -= 1
        ElseIf e.KeyCode = Keys.Space Then
            FrameTime.Stop()
            shot.Location = New Point(1, 1)
            r = 1
            animationTimer.Start()
            animation = True
        End If

#Region "keep Tank on screen"
        If (tp >= 0) And (tp <= points.Length) Then
            tank.Location = New Point(points(tp).X, points(tp).Y)
        ElseIf (tp <= 0) Then
            tank.Location = New Point(points(1).X, points(1).Y)
        ElseIf (tp >= points.Length) Then
            tank.Location = New Point(points(points.Length).X, points(points.Length).Y)
        End If
#End Region


    End Sub



    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs) Handles animationTimer.Tick

        ShootAnimation()

        r += 1 'increment calcuated x position of shot (determines how many shot frames are calculated)

    End Sub

    Private Sub FrameTime_Tick(sender As Object, e As EventArgs) Handles FrameTime.Tick
        tankPosAdjusted = New Point(tank.Location.X - (tank.Width / 2), tank.Location.Y - (tank.Height / 2))
        velocity = Distance(tankPosAdjusted, mousepos)
        If velocity > 200 Then
            velocity = 200
        End If
        angle = CalculateAngle(mousepos, tankPosAdjusted)
        DrawOnTick()

    End Sub

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove
        mousepos = e.Location
    End Sub





#Region "Game Methods"
    'animates the shot flight
    ''' <summary>
    ''' Animates the shot flying through the air
    ''' </summary>
    Private Sub ShootAnimation()

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

    'ground collision detection
    ''' <summary>
    ''' Checks if there is a line between a point and a line given the lines start and end coordinates
    ''' </summary>
    ''' <param name="ptstart"></param>
    ''' <param name="ptend"></param>
    Private Sub PolygonDetection(ptstart As Point, ptend As Point)
        Dim lineVec As Vector2 = New Vector2(ptend.X - ptstart.X, ptend.Y - ptstart.Y)
        Dim lineLength As Double = lineVec.Length
        lineVec /= lineLength
        Dim perpLineVec As Vector2 = New Vector2(-lineVec.Y, lineVec.X)

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
        g1.DrawRectangle(pen, tank)
        g1.FillEllipse(brush, aimCircle)
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
