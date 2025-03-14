﻿Imports System.IO
Imports System.IO.Ports

Public Class LightControl

    Private lightcomport As String = "COM3"
    Private lightport As SerialPort
    Dim lights() As Integer = {3, 5, 6, 9, 10, 11}

    Public Sub New()
        lightport = New SerialPort
        lightport.Close()
        lightport.PortName = lightcomport
        lightport.BaudRate = 9600
        lightport.DataBits = 8
        lightport.Parity = Parity.None
        lightport.StopBits = StopBits.One
        lightport.Handshake = Handshake.None
        lightport.Encoding = System.Text.Encoding.Default
        disco(100)
        pick_light(10)
    End Sub

    Public Sub light_dim(ByVal lightnum As Integer, ByVal lightvalue As Integer)
        Debug.Print("lightdim called")
        If Not lightport.IsOpen Then
            'Debug.Print("port opening")
            lightport.Open()
        End If
        'Debug.Print("writing")
        lightport.Write(lightnum & "," & lightvalue & vbLf)
        Threading.Thread.Sleep(20)
        Debug.Print(lightnum & "," & lightvalue & vbLf)
    End Sub

    Public Sub pick_light(ByVal lightnum As Integer)
        For Each i As Integer In lights
            If Not i = lightnum Then
                light_off(i)
            End If
        Next
        light_on(lightnum)
    End Sub

    Public Sub light_on(ByVal lightnum As Integer)
        light_dim(lightnum, 255)
    End Sub

    Public Sub light_off(ByVal lightnum As Integer)
        light_dim(lightnum, 0)
    End Sub


    Public Sub all_off()
        For Each i As Integer In lights
            light_off(i)
        Next
    End Sub

    Public Sub disco()
        disco(200)
    End Sub

    Public Sub disco(ByVal delaytime As Integer)
        'pick_light(7)
        'Threading.Thread.Sleep(delaytime)
        pick_light(0)
        light_on(4)
        Threading.Thread.Sleep(delaytime)
        pick_light(2)
        Threading.Thread.Sleep(delaytime)
        pick_light(1)
        light_on(4)
        Threading.Thread.Sleep(delaytime)
        pick_light(3)
        Threading.Thread.Sleep(delaytime)
        'pick_light(7)
    End Sub

End Class