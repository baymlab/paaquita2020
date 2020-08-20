Imports System.Threading
Public Class DiscoForm
    Public t As Thread

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    Private Sub DiscoForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.CenterToParent()
        t = New Thread(AddressOf VBSample.discodisco)
        t.Start()
        Try
            My.Computer.Audio.Play("C:\Users\baymlab\Documents\GitHub\paaquita2020\stayinalooploud.wav", AudioPlayMode.BackgroundLoop)

        Catch ex As Exception
            MsgBox("No Sound File!")

        End Try



    End Sub


    Private Sub DiscoForm_Close(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Closing
        My.Computer.Audio.Stop()
        t.Abort()
        VBSample.white_lights()
    End Sub

End Class