'/******************************************************************************
'*                                                                             *
'*   PROJECT : EOS Digital Software Development Kit EDSDK                      *
'*      NAME : CameraController.vb                                             *
'*                                                                             *
'*   Description: This is the Sample code to show the usage of EDSDK.          *
'*                                                                             *
'*******************************************************************************
'*                                                                             *
'*   Written and developed by Camera Design Dept.53                            *
'*   Copyright Canon Inc. 2006 All Rights Reserved                             *
'*                                                                             *
'*******************************************************************************
'*   File Update Information:                                                  *
'*     DATE      Identify    Comment                                           *
'*   -----------------------------------------------------------------------   *
'*   06-03-22    F-001        create first version.                            *
'*   -----------------------------------------------------------------------   *
'*   19-06-03    NQuinones    modify                                           *
'******************************************************************************/

Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.IO
Imports System.Net.Mail

Public Class VBSample
    Inherits System.Windows.Forms.Form
    Implements Observer
    ' Changes dirItemInfo.szFileName in DownloadCommand.vb
    Public Shared savepath As String

#Region "Lightbox definitions"
    Public mCherry_light As Integer = 6
    Public CFP_light As Integer = 3
    Public YFP_light As Integer = 4
    Public GFP_light As Integer = 4
    Public white_light As Integer = 5
    Public control_light As Integer = 0
    Public back_light As Integer = 6
    Friend WithEvents LightsOff As System.Windows.Forms.Button
#End Region

#Region "filter definitions"
    Public mCherry_filter As Integer = 4
    Public CFP_filter As Integer = 3
    Public YFP_filter As Integer = 2
    Public GFP_filter As Integer = 2
    Public BFP_filter As Integer = 5


    Public no_filter As Integer = 1
    Public filterpos As Integer = 0
    Public Shared ismoving As Boolean = False

    Public bfpwarn As Boolean = False

    'ADDED BY ERIC AND KALIN
    Public filter_cycle_EK As Integer = 0

    Private Const VendorID As Integer = &H1278    'Replace with your device's
    Private Const ProductID As Integer = &H920     'product and vendor IDs

    ' read and write buffers
    Private Const BufferInSize As Integer = 3 'Size of the data buffer coming IN to the PC
    Private Const BufferOutSize As Integer = 3    'Size of the data buffer going OUT from the PC
    Dim BufferIn(BufferInSize) As Byte
    Dim pHandle As Integer
    Friend WithEvents DiscoButton As Button
    Friend WithEvents LightsOutBox As CheckBox
    Friend WithEvents ManExposure As ComboBox
    Friend WithEvents ManTakePicture As Button
    Friend WithEvents ExposureLab As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents OuterWhiteLight As Button
    Friend WithEvents PickDirectory As Button
    Friend WithEvents TcpClientActivex1 As TCPCamActivex.TCPClientActivex
    'Received data will be stored here - the first byte in the array is unused
    Dim BufferOut(BufferOutSize) As Byte    'Transmitted data is stored here - the first item in the array must be 0

    '*****************************************************************
    ' disconnect from the HID controller...
    '*****************************************************************
    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed

    End Sub

    '*****************************************************************
    ' a HID device has been plugged in...
    '*****************************************************************
    Public Sub OnPlugged(ByVal pHandle As Integer)
        'Console.WriteLine("on plugged outside if")

        If hidGetVendorID(pHandle) = VendorID And hidGetProductID(pHandle) = ProductID Then
            'Console.WriteLine("on plugged")
            Debug.Print("Filter Wheel Detected,  Input Length: " & hidGetInputReportLength(pHandle) & " Output Report Length: " & hidGetOutputReportLength(pHandle))

        End If
    End Sub

    '*****************************************************************
    ' a HID device has been unplugged...
    '*****************************************************************
    Public Sub OnUnplugged(ByVal pHandle As Integer)
        If hidGetVendorID(pHandle) = VendorID And hidGetProductID(pHandle) = ProductID Then
            hidSetReadNotify(hidGetHandle(VendorID, ProductID), False)
            ' ** YOUR CODE HERE **
        End If
    End Sub

    '*****************************************************************
    ' controller changed notification - called
    ' after ALL HID devices are plugged or unplugged
    '*****************************************************************
    Public Sub OnChanged()
        ' get the handle of the device we are interested in, then set
        ' its read notify flag to true - this ensures you get a read
        ' notification message when there is some data to read...
        Debug.Print("OnChanged")
        pHandle = hidGetHandle(VendorID, ProductID)
        hidSetReadNotify(hidGetHandle(VendorID, ProductID), True)
    End Sub

    '*****************************************************************
    ' on read event...
    '*****************************************************************
    Public Sub OnRead(ByVal pHandle As Integer)
        ' read the data (don't forget, pass the whole array)...
        'Debug.Print("BufferIn = " & BufferIn(0) & "," & BufferIn(1) & "," & BufferIn(2))

        Thread.Sleep(10)
        If (hidRead(pHandle, BufferIn(0))) Then
            Dim bufferone As Integer = BufferIn(1)
            Dim buffertwo As Integer = BufferIn(2)
            Debug.Print("Read Buffer = " & bufferone & "," & buffertwo)

            ' ** YOUR CODE HERE **
            If bufferone = 0 Then
                ismoving = True
            Else
                ismoving = False
            End If
            'Thread.Sleep(10)
            Debug.Print("Ismoving: " & ismoving)
            'QueueBox.Text = ismoving
            ' first byte is the report ID, e.g. BufferIn(0)
            ' the other bytes are the data from the microcontroller...
        End If
    End Sub




    Public Sub Goto_Filter(ByVal filternum As Integer)
        ismoving = True
        'Console.WriteLine("pos " & filterpos)
        'Console.WriteLine("num " & filternum)
        BufferOut(0) = 0
        BufferOut(1) = filternum + 128
        BufferOut(2) = 0
        'Console.WriteLine(BufferOut)
        hidWrite(pHandle, BufferOut(0))
        'hidWriteEx(VendorID, ProductID, BufferOut(0))
        'hidWriteEx(VendorID, ProductID, BufferOut(1))
        'hidWriteEx(VendorID, ProductID, BufferOut(2))

        Debug.Print("Sent " & BufferOut(0) & "," & BufferOut(1))
        'Debug.Print(hidReadEx(VendorID, ProductID, BufferIn(0)))
        filterpos = filternum
    End Sub

    Public Sub Wheel_Query()
        BufferOut(0) = 0
        BufferOut(1) = 0
        'BufferOut(2) = 0
        hidWrite(pHandle, BufferOut(0))
        'hidWriteEx(VendorID, ProductID, BufferOut(0))
        'hidWriteEx(VendorID, ProductID, BufferOut(1))
    End Sub

#End Region

    ' Modifications: auto.
#Region "Created by Windows form designer."

    Public Sub New()
        MyBase.New()

        InitializeComponent()


    End Sub

    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private components As System.ComponentModel.IContainer

    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents TakeBtn As System.Windows.Forms.Button
    Friend WithEvents ISOSpeedCmb As System.Windows.Forms.ComboBox
    Friend WithEvents AvCmb As System.Windows.Forms.ComboBox
    Friend WithEvents TvCmb As System.Windows.Forms.ComboBox
    Private WithEvents AEModeCmb As System.Windows.Forms.ComboBox
    Friend WithEvents MeteringModeCmb As System.Windows.Forms.ComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ExitBtn As System.Windows.Forms.Button
    Friend WithEvents ImageQualityCmb As System.Windows.Forms.ComboBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents LabelSavePath As Label
    Friend WithEvents TextBoxSavePath As TextBox
    Friend WithEvents ButtonTimelapseStart As Button
    Friend WithEvents ButtonTimelapseStop As Button
    Friend WithEvents PhotosTakenBox As Label
    Friend WithEvents NextPictureSeconds As Label
    Friend WithEvents Button1 As Button
    Friend WithEvents DelaySecondsTB As TextBox
    Friend WithEvents DelaySecondsLab As Label
    Friend WithEvents LenghtTimelapseLab As Label
    Friend WithEvents MaxTime As TextBox
    Friend WithEvents Pictures As RadioButton
    Friend WithEvents Seconds As RadioButton
    Friend WithEvents ControlTabs As TabControl
    Friend WithEvents ImagingControls As TabPage
    Friend WithEvents TimeLapseControls As TabPage
    Friend WithEvents FilePrefixLab As Label
    Friend WithEvents FilePrefix As TextBox
    Friend WithEvents mCherry As CheckBox
    Friend WithEvents CFP As CheckBox
    Friend WithEvents GFP As CheckBox
    Friend WithEvents Brightfield As CheckBox
    Friend WithEvents LiveViewLab As Label
    Friend WithEvents LiveViewOn As RadioButton
    Friend WithEvents LiveViewOff As RadioButton
    Friend WithEvents Backlight As CheckBox
    Friend WithEvents mCHbox As ComboBox
    Friend WithEvents CFPbox As ComboBox
    Friend WithEvents GFPbox As ComboBox
    Friend WithEvents BLbox As ComboBox
    Friend WithEvents ManualControls As TabPage
    Friend WithEvents Label4 As Label
    Friend WithEvents mChFilter As Button
    Friend WithEvents CFPfilter As Button
    Friend WithEvents GFPfilter As Button
    Friend WithEvents NoFilter As Button
    Friend WithEvents mChrLight As Button
    Friend WithEvents CFPlight As Button
    Friend WithEvents GFPlight As Button
    Friend WithEvents WhiteLight As Button
    Friend WithEvents LightsOffButton As Button
    Friend WithEvents progressBar As System.Windows.Forms.ProgressBar
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.TakeBtn = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.AEModeCmb = New System.Windows.Forms.ComboBox()
        Me.ISOSpeedCmb = New System.Windows.Forms.ComboBox()
        Me.AvCmb = New System.Windows.Forms.ComboBox()
        Me.TvCmb = New System.Windows.Forms.ComboBox()
        Me.MeteringModeCmb = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ExitBtn = New System.Windows.Forms.Button()
        Me.progressBar = New System.Windows.Forms.ProgressBar()
        Me.ImageQualityCmb = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.LabelSavePath = New System.Windows.Forms.Label()
        Me.TextBoxSavePath = New System.Windows.Forms.TextBox()
        Me.ButtonTimelapseStart = New System.Windows.Forms.Button()
        Me.PhotosTakenBox = New System.Windows.Forms.Label()
        Me.NextPictureSeconds = New System.Windows.Forms.Label()
        Me.ButtonTimelapseStop = New System.Windows.Forms.Button()
        Me.DelaySecondsTB = New System.Windows.Forms.TextBox()
        Me.DelaySecondsLab = New System.Windows.Forms.Label()
        Me.LenghtTimelapseLab = New System.Windows.Forms.Label()
        Me.MaxTime = New System.Windows.Forms.TextBox()
        Me.Pictures = New System.Windows.Forms.RadioButton()
        Me.Seconds = New System.Windows.Forms.RadioButton()
        Me.ControlTabs = New System.Windows.Forms.TabControl()
        Me.ImagingControls = New System.Windows.Forms.TabPage()
        Me.LightsOutBox = New System.Windows.Forms.CheckBox()
        Me.mCHbox = New System.Windows.Forms.ComboBox()
        Me.CFPbox = New System.Windows.Forms.ComboBox()
        Me.GFPbox = New System.Windows.Forms.ComboBox()
        Me.BLbox = New System.Windows.Forms.ComboBox()
        Me.Backlight = New System.Windows.Forms.CheckBox()
        Me.ExposureLab = New System.Windows.Forms.Label()
        Me.mCherry = New System.Windows.Forms.CheckBox()
        Me.CFP = New System.Windows.Forms.CheckBox()
        Me.GFP = New System.Windows.Forms.CheckBox()
        Me.Brightfield = New System.Windows.Forms.CheckBox()
        Me.ManualControls = New System.Windows.Forms.TabPage()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.ManTakePicture = New System.Windows.Forms.Button()
        Me.ManExposure = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.mChFilter = New System.Windows.Forms.Button()
        Me.CFPfilter = New System.Windows.Forms.Button()
        Me.GFPfilter = New System.Windows.Forms.Button()
        Me.NoFilter = New System.Windows.Forms.Button()
        Me.mChrLight = New System.Windows.Forms.Button()
        Me.CFPlight = New System.Windows.Forms.Button()
        Me.GFPlight = New System.Windows.Forms.Button()
        Me.WhiteLight = New System.Windows.Forms.Button()
        Me.TimeLapseControls = New System.Windows.Forms.TabPage()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.FilePrefixLab = New System.Windows.Forms.Label()
        Me.FilePrefix = New System.Windows.Forms.TextBox()
        Me.LiveViewLab = New System.Windows.Forms.Label()
        Me.LiveViewOn = New System.Windows.Forms.RadioButton()
        Me.LiveViewOff = New System.Windows.Forms.RadioButton()
        Me.LightsOffButton = New System.Windows.Forms.Button()
        Me.DiscoButton = New System.Windows.Forms.Button()
        Me.OuterWhiteLight = New System.Windows.Forms.Button()
        Me.PickDirectory = New System.Windows.Forms.Button()
        Me.TcpClientActivex1 = New TCPCamActivex.TCPClientActivex()
        Me.ControlTabs.SuspendLayout()
        Me.ImagingControls.SuspendLayout()
        Me.ManualControls.SuspendLayout()
        Me.TimeLapseControls.SuspendLayout()
        Me.SuspendLayout()
        '
        'TakeBtn
        '
        Me.TakeBtn.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TakeBtn.Location = New System.Drawing.Point(204, 235)
        Me.TakeBtn.Name = "TakeBtn"
        Me.TakeBtn.Size = New System.Drawing.Size(90, 37)
        Me.TakeBtn.TabIndex = 0
        Me.TakeBtn.Text = "Take Picture"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label2.Location = New System.Drawing.Point(12, 21)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(54, 13)
        Me.Label2.TabIndex = 6
        Me.Label2.Text = "AE Mode:"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label3.Location = New System.Drawing.Point(12, 90)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(28, 13)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "ISO:"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label5.Location = New System.Drawing.Point(12, 55)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(23, 13)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Av:"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'AEModeCmb
        '
        Me.AEModeCmb.Location = New System.Drawing.Point(115, 17)
        Me.AEModeCmb.Name = "AEModeCmb"
        Me.AEModeCmb.Size = New System.Drawing.Size(166, 21)
        Me.AEModeCmb.TabIndex = 11
        '
        'ISOSpeedCmb
        '
        Me.ISOSpeedCmb.Location = New System.Drawing.Point(115, 86)
        Me.ISOSpeedCmb.Name = "ISOSpeedCmb"
        Me.ISOSpeedCmb.Size = New System.Drawing.Size(166, 21)
        Me.ISOSpeedCmb.TabIndex = 12
        '
        'AvCmb
        '
        Me.AvCmb.Location = New System.Drawing.Point(115, 52)
        Me.AvCmb.Name = "AvCmb"
        Me.AvCmb.Size = New System.Drawing.Size(166, 21)
        Me.AvCmb.TabIndex = 14
        '
        'TvCmb
        '
        Me.TvCmb.Location = New System.Drawing.Point(128, 38)
        Me.TvCmb.Name = "TvCmb"
        Me.TvCmb.Size = New System.Drawing.Size(166, 21)
        Me.TvCmb.TabIndex = 15
        Me.TvCmb.Text = "1"
        '
        'MeteringModeCmb
        '
        Me.MeteringModeCmb.Location = New System.Drawing.Point(115, 120)
        Me.MeteringModeCmb.Name = "MeteringModeCmb"
        Me.MeteringModeCmb.Size = New System.Drawing.Size(166, 21)
        Me.MeteringModeCmb.TabIndex = 17
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(12, 123)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(81, 13)
        Me.Label1.TabIndex = 16
        Me.Label1.Text = "Metering Mode:"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'ExitBtn
        '
        Me.ExitBtn.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ExitBtn.Location = New System.Drawing.Point(313, 17)
        Me.ExitBtn.Name = "ExitBtn"
        Me.ExitBtn.Size = New System.Drawing.Size(80, 35)
        Me.ExitBtn.TabIndex = 4
        Me.ExitBtn.Text = "Quit"
        '
        'progressBar
        '
        Me.progressBar.Location = New System.Drawing.Point(115, 193)
        Me.progressBar.Name = "progressBar"
        Me.progressBar.Size = New System.Drawing.Size(166, 22)
        Me.progressBar.TabIndex = 21
        '
        'ImageQualityCmb
        '
        Me.ImageQualityCmb.Location = New System.Drawing.Point(115, 157)
        Me.ImageQualityCmb.Name = "ImageQualityCmb"
        Me.ImageQualityCmb.Size = New System.Drawing.Size(166, 21)
        Me.ImageQualityCmb.TabIndex = 22
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label7.Location = New System.Drawing.Point(12, 160)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(71, 13)
        Me.Label7.TabIndex = 23
        Me.Label7.Text = "ImageQuality:"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LabelSavePath
        '
        Me.LabelSavePath.AutoSize = True
        Me.LabelSavePath.Location = New System.Drawing.Point(12, 264)
        Me.LabelSavePath.Name = "LabelSavePath"
        Me.LabelSavePath.Size = New System.Drawing.Size(59, 13)
        Me.LabelSavePath.TabIndex = 24
        Me.LabelSavePath.Text = "Save path:"
        '
        'TextBoxSavePath
        '
        Me.TextBoxSavePath.Location = New System.Drawing.Point(115, 264)
        Me.TextBoxSavePath.Name = "TextBoxSavePath"
        Me.TextBoxSavePath.Size = New System.Drawing.Size(278, 20)
        Me.TextBoxSavePath.TabIndex = 25
        Me.TextBoxSavePath.Text = "C:\tmp\"
        '
        'ButtonTimelapseStart
        '
        Me.ButtonTimelapseStart.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonTimelapseStart.Location = New System.Drawing.Point(191, 170)
        Me.ButtonTimelapseStart.Name = "ButtonTimelapseStart"
        Me.ButtonTimelapseStart.Size = New System.Drawing.Size(103, 34)
        Me.ButtonTimelapseStart.TabIndex = 26
        Me.ButtonTimelapseStart.Text = "Start Timelapse"
        Me.ButtonTimelapseStart.UseVisualStyleBackColor = True
        '
        'PhotosTakenBox
        '
        Me.PhotosTakenBox.AutoSize = True
        Me.PhotosTakenBox.Location = New System.Drawing.Point(195, 118)
        Me.PhotosTakenBox.Name = "PhotosTakenBox"
        Me.PhotosTakenBox.Size = New System.Drawing.Size(99, 13)
        Me.PhotosTakenBox.TabIndex = 27
        Me.PhotosTakenBox.Text = "Taken 0 photo sets"
        Me.PhotosTakenBox.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'NextPictureSeconds
        '
        Me.NextPictureSeconds.AutoSize = True
        Me.NextPictureSeconds.Location = New System.Drawing.Point(162, 143)
        Me.NextPictureSeconds.Name = "NextPictureSeconds"
        Me.NextPictureSeconds.Size = New System.Drawing.Size(132, 13)
        Me.NextPictureSeconds.TabIndex = 29
        Me.NextPictureSeconds.Text = "__ seconds to next picture"
        Me.NextPictureSeconds.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'ButtonTimelapseStop
        '
        Me.ButtonTimelapseStop.Location = New System.Drawing.Point(191, 210)
        Me.ButtonTimelapseStop.Name = "ButtonTimelapseStop"
        Me.ButtonTimelapseStop.Size = New System.Drawing.Size(103, 35)
        Me.ButtonTimelapseStop.TabIndex = 30
        Me.ButtonTimelapseStop.Text = "Stop Timelapse"
        Me.ButtonTimelapseStop.UseVisualStyleBackColor = True
        '
        'DelaySecondsTB
        '
        Me.DelaySecondsTB.AccessibleDescription = ""
        Me.DelaySecondsTB.AccessibleName = ""
        Me.DelaySecondsTB.Location = New System.Drawing.Point(17, 111)
        Me.DelaySecondsTB.Name = "DelaySecondsTB"
        Me.DelaySecondsTB.Size = New System.Drawing.Size(87, 20)
        Me.DelaySecondsTB.TabIndex = 31
        Me.DelaySecondsTB.Text = "30"
        '
        'DelaySecondsLab
        '
        Me.DelaySecondsLab.Location = New System.Drawing.Point(14, 78)
        Me.DelaySecondsLab.Name = "DelaySecondsLab"
        Me.DelaySecondsLab.Size = New System.Drawing.Size(131, 26)
        Me.DelaySecondsLab.TabIndex = 32
        Me.DelaySecondsLab.Text = "Time between pictures (s): Minimum 30s"
        '
        'LenghtTimelapseLab
        '
        Me.LenghtTimelapseLab.AutoSize = True
        Me.LenghtTimelapseLab.Location = New System.Drawing.Point(14, 145)
        Me.LenghtTimelapseLab.Name = "LenghtTimelapseLab"
        Me.LenghtTimelapseLab.Size = New System.Drawing.Size(106, 13)
        Me.LenghtTimelapseLab.TabIndex = 33
        Me.LenghtTimelapseLab.Text = "Length of Timelapse:"
        '
        'MaxTime
        '
        Me.MaxTime.Location = New System.Drawing.Point(17, 166)
        Me.MaxTime.Name = "MaxTime"
        Me.MaxTime.Size = New System.Drawing.Size(87, 20)
        Me.MaxTime.TabIndex = 35
        Me.MaxTime.Text = "10"
        '
        'Pictures
        '
        Me.Pictures.AutoSize = True
        Me.Pictures.Checked = True
        Me.Pictures.Location = New System.Drawing.Point(17, 201)
        Me.Pictures.Name = "Pictures"
        Me.Pictures.Size = New System.Drawing.Size(63, 17)
        Me.Pictures.TabIndex = 36
        Me.Pictures.TabStop = True
        Me.Pictures.Text = "Pictures"
        Me.Pictures.UseVisualStyleBackColor = True
        '
        'Seconds
        '
        Me.Seconds.Location = New System.Drawing.Point(17, 224)
        Me.Seconds.Name = "Seconds"
        Me.Seconds.Size = New System.Drawing.Size(87, 21)
        Me.Seconds.TabIndex = 37
        Me.Seconds.TabStop = True
        Me.Seconds.Text = "Seconds"
        Me.Seconds.UseVisualStyleBackColor = True
        '
        'ControlTabs
        '
        Me.ControlTabs.Controls.Add(Me.ImagingControls)
        Me.ControlTabs.Controls.Add(Me.ManualControls)
        Me.ControlTabs.Controls.Add(Me.TimeLapseControls)
        Me.ControlTabs.Location = New System.Drawing.Point(414, 17)
        Me.ControlTabs.Name = "ControlTabs"
        Me.ControlTabs.SelectedIndex = 0
        Me.ControlTabs.Size = New System.Drawing.Size(320, 304)
        Me.ControlTabs.TabIndex = 38
        '
        'ImagingControls
        '
        Me.ImagingControls.Controls.Add(Me.LightsOutBox)
        Me.ImagingControls.Controls.Add(Me.mCHbox)
        Me.ImagingControls.Controls.Add(Me.CFPbox)
        Me.ImagingControls.Controls.Add(Me.GFPbox)
        Me.ImagingControls.Controls.Add(Me.BLbox)
        Me.ImagingControls.Controls.Add(Me.Backlight)
        Me.ImagingControls.Controls.Add(Me.ExposureLab)
        Me.ImagingControls.Controls.Add(Me.mCherry)
        Me.ImagingControls.Controls.Add(Me.CFP)
        Me.ImagingControls.Controls.Add(Me.GFP)
        Me.ImagingControls.Controls.Add(Me.Brightfield)
        Me.ImagingControls.Controls.Add(Me.TvCmb)
        Me.ImagingControls.Controls.Add(Me.TakeBtn)
        Me.ImagingControls.Location = New System.Drawing.Point(4, 22)
        Me.ImagingControls.Name = "ImagingControls"
        Me.ImagingControls.Padding = New System.Windows.Forms.Padding(3)
        Me.ImagingControls.Size = New System.Drawing.Size(312, 278)
        Me.ImagingControls.TabIndex = 0
        Me.ImagingControls.Text = "Imaging Controls"
        Me.ImagingControls.UseVisualStyleBackColor = True
        '
        'LightsOutBox
        '
        Me.LightsOutBox.AutoSize = True
        Me.LightsOutBox.Checked = True
        Me.LightsOutBox.CheckState = System.Windows.Forms.CheckState.Checked
        Me.LightsOutBox.Location = New System.Drawing.Point(14, 246)
        Me.LightsOutBox.Name = "LightsOutBox"
        Me.LightsOutBox.Size = New System.Drawing.Size(131, 17)
        Me.LightsOutBox.TabIndex = 21
        Me.LightsOutBox.Text = "Lights out after photos"
        Me.LightsOutBox.UseVisualStyleBackColor = True
        '
        'mCHbox
        '
        Me.mCHbox.FormattingEnabled = True
        Me.mCHbox.Location = New System.Drawing.Point(128, 202)
        Me.mCHbox.Name = "mCHbox"
        Me.mCHbox.Size = New System.Drawing.Size(166, 21)
        Me.mCHbox.TabIndex = 19
        Me.mCHbox.Text = "1"
        '
        'CFPbox
        '
        Me.CFPbox.FormattingEnabled = True
        Me.CFPbox.Location = New System.Drawing.Point(128, 164)
        Me.CFPbox.Name = "CFPbox"
        Me.CFPbox.Size = New System.Drawing.Size(166, 21)
        Me.CFPbox.TabIndex = 19
        Me.CFPbox.Text = "1"
        '
        'GFPbox
        '
        Me.GFPbox.FormattingEnabled = True
        Me.GFPbox.Location = New System.Drawing.Point(128, 126)
        Me.GFPbox.Name = "GFPbox"
        Me.GFPbox.Size = New System.Drawing.Size(166, 21)
        Me.GFPbox.TabIndex = 19
        Me.GFPbox.Text = "1"
        '
        'BLbox
        '
        Me.BLbox.FormattingEnabled = True
        Me.BLbox.Location = New System.Drawing.Point(128, 90)
        Me.BLbox.Name = "BLbox"
        Me.BLbox.Size = New System.Drawing.Size(166, 21)
        Me.BLbox.TabIndex = 19
        Me.BLbox.Text = "1"
        '
        'Backlight
        '
        Me.Backlight.AutoSize = True
        Me.Backlight.Location = New System.Drawing.Point(14, 94)
        Me.Backlight.Name = "Backlight"
        Me.Backlight.Size = New System.Drawing.Size(70, 17)
        Me.Backlight.TabIndex = 20
        Me.Backlight.Text = "Backlight"
        Me.Backlight.UseVisualStyleBackColor = True
        '
        'ExposureLab
        '
        Me.ExposureLab.AutoSize = True
        Me.ExposureLab.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ExposureLab.Location = New System.Drawing.Point(125, 13)
        Me.ExposureLab.Name = "ExposureLab"
        Me.ExposureLab.Size = New System.Drawing.Size(54, 13)
        Me.ExposureLab.TabIndex = 4
        Me.ExposureLab.Text = "Exposure:"
        '
        'mCherry
        '
        Me.mCherry.AutoSize = True
        Me.mCherry.Location = New System.Drawing.Point(14, 206)
        Me.mCherry.Name = "mCherry"
        Me.mCherry.Size = New System.Drawing.Size(64, 17)
        Me.mCherry.TabIndex = 3
        Me.mCherry.Text = "mCherry"
        Me.mCherry.UseVisualStyleBackColor = True
        '
        'CFP
        '
        Me.CFP.AutoSize = True
        Me.CFP.Location = New System.Drawing.Point(14, 168)
        Me.CFP.Name = "CFP"
        Me.CFP.Size = New System.Drawing.Size(46, 17)
        Me.CFP.TabIndex = 2
        Me.CFP.Text = "CFP"
        Me.CFP.UseVisualStyleBackColor = True
        '
        'GFP
        '
        Me.GFP.AutoSize = True
        Me.GFP.Location = New System.Drawing.Point(14, 130)
        Me.GFP.Name = "GFP"
        Me.GFP.Size = New System.Drawing.Size(47, 17)
        Me.GFP.TabIndex = 1
        Me.GFP.Text = "GFP"
        Me.GFP.UseVisualStyleBackColor = True
        '
        'Brightfield
        '
        Me.Brightfield.AutoSize = True
        Me.Brightfield.Location = New System.Drawing.Point(14, 40)
        Me.Brightfield.Name = "Brightfield"
        Me.Brightfield.Size = New System.Drawing.Size(72, 17)
        Me.Brightfield.TabIndex = 0
        Me.Brightfield.Text = "Brightfield"
        Me.Brightfield.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        Me.Brightfield.UseVisualStyleBackColor = True
        '
        'ManualControls
        '
        Me.ManualControls.Controls.Add(Me.Label6)
        Me.ManualControls.Controls.Add(Me.ManTakePicture)
        Me.ManualControls.Controls.Add(Me.ManExposure)
        Me.ManualControls.Controls.Add(Me.Label4)
        Me.ManualControls.Controls.Add(Me.mChFilter)
        Me.ManualControls.Controls.Add(Me.CFPfilter)
        Me.ManualControls.Controls.Add(Me.GFPfilter)
        Me.ManualControls.Controls.Add(Me.NoFilter)
        Me.ManualControls.Controls.Add(Me.mChrLight)
        Me.ManualControls.Controls.Add(Me.CFPlight)
        Me.ManualControls.Controls.Add(Me.GFPlight)
        Me.ManualControls.Controls.Add(Me.WhiteLight)
        Me.ManualControls.Location = New System.Drawing.Point(4, 22)
        Me.ManualControls.Name = "ManualControls"
        Me.ManualControls.Size = New System.Drawing.Size(312, 278)
        Me.ManualControls.TabIndex = 2
        Me.ManualControls.Text = "Manual Controls"
        Me.ManualControls.UseVisualStyleBackColor = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(40, 188)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(54, 13)
        Me.Label6.TabIndex = 18
        Me.Label6.Text = "Exposure:"
        '
        'ManTakePicture
        '
        Me.ManTakePicture.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.ManTakePicture.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ManTakePicture.Location = New System.Drawing.Point(176, 191)
        Me.ManTakePicture.Name = "ManTakePicture"
        Me.ManTakePicture.Size = New System.Drawing.Size(96, 38)
        Me.ManTakePicture.TabIndex = 17
        Me.ManTakePicture.Text = "Take Picture"
        Me.ManTakePicture.UseVisualStyleBackColor = True
        '
        'ManExposure
        '
        Me.ManExposure.Location = New System.Drawing.Point(43, 204)
        Me.ManExposure.Name = "ManExposure"
        Me.ManExposure.Size = New System.Drawing.Size(96, 21)
        Me.ManExposure.TabIndex = 16
        Me.ManExposure.Text = "1"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(16, 247)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(277, 13)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Remember to select both a light and a filter for your image"
        '
        'mChFilter
        '
        Me.mChFilter.Location = New System.Drawing.Point(176, 141)
        Me.mChFilter.Name = "mChFilter"
        Me.mChFilter.Size = New System.Drawing.Size(96, 22)
        Me.mChFilter.TabIndex = 7
        Me.mChFilter.Text = "RFP Filter"
        Me.mChFilter.UseVisualStyleBackColor = True
        '
        'CFPfilter
        '
        Me.CFPfilter.Location = New System.Drawing.Point(176, 100)
        Me.CFPfilter.Name = "CFPfilter"
        Me.CFPfilter.Size = New System.Drawing.Size(96, 22)
        Me.CFPfilter.TabIndex = 6
        Me.CFPfilter.Text = "CFP Filter"
        Me.CFPfilter.UseVisualStyleBackColor = True
        '
        'GFPfilter
        '
        Me.GFPfilter.Location = New System.Drawing.Point(176, 58)
        Me.GFPfilter.Name = "GFPfilter"
        Me.GFPfilter.Size = New System.Drawing.Size(96, 22)
        Me.GFPfilter.TabIndex = 5
        Me.GFPfilter.Text = "GFP Filter"
        Me.GFPfilter.UseVisualStyleBackColor = True
        '
        'NoFilter
        '
        Me.NoFilter.Location = New System.Drawing.Point(176, 18)
        Me.NoFilter.Name = "NoFilter"
        Me.NoFilter.Size = New System.Drawing.Size(96, 22)
        Me.NoFilter.TabIndex = 4
        Me.NoFilter.Text = "#NoFilter"
        Me.NoFilter.UseVisualStyleBackColor = True
        '
        'mChrLight
        '
        Me.mChrLight.Location = New System.Drawing.Point(43, 141)
        Me.mChrLight.Name = "mChrLight"
        Me.mChrLight.Size = New System.Drawing.Size(96, 22)
        Me.mChrLight.TabIndex = 3
        Me.mChrLight.Text = "RFP Light"
        Me.mChrLight.UseVisualStyleBackColor = True
        '
        'CFPlight
        '
        Me.CFPlight.Location = New System.Drawing.Point(43, 100)
        Me.CFPlight.Name = "CFPlight"
        Me.CFPlight.Size = New System.Drawing.Size(96, 22)
        Me.CFPlight.TabIndex = 2
        Me.CFPlight.Text = "CFP Light"
        Me.CFPlight.UseVisualStyleBackColor = True
        '
        'GFPlight
        '
        Me.GFPlight.Location = New System.Drawing.Point(43, 58)
        Me.GFPlight.Name = "GFPlight"
        Me.GFPlight.Size = New System.Drawing.Size(96, 22)
        Me.GFPlight.TabIndex = 1
        Me.GFPlight.Text = "GFP Light"
        Me.GFPlight.UseVisualStyleBackColor = True
        '
        'WhiteLight
        '
        Me.WhiteLight.Location = New System.Drawing.Point(43, 18)
        Me.WhiteLight.Name = "WhiteLight"
        Me.WhiteLight.Size = New System.Drawing.Size(96, 22)
        Me.WhiteLight.TabIndex = 0
        Me.WhiteLight.Text = "White Light"
        Me.WhiteLight.UseVisualStyleBackColor = True
        '
        'TimeLapseControls
        '
        Me.TimeLapseControls.Controls.Add(Me.Label8)
        Me.TimeLapseControls.Controls.Add(Me.ButtonTimelapseStop)
        Me.TimeLapseControls.Controls.Add(Me.Seconds)
        Me.TimeLapseControls.Controls.Add(Me.ButtonTimelapseStart)
        Me.TimeLapseControls.Controls.Add(Me.Pictures)
        Me.TimeLapseControls.Controls.Add(Me.PhotosTakenBox)
        Me.TimeLapseControls.Controls.Add(Me.MaxTime)
        Me.TimeLapseControls.Controls.Add(Me.NextPictureSeconds)
        Me.TimeLapseControls.Controls.Add(Me.LenghtTimelapseLab)
        Me.TimeLapseControls.Controls.Add(Me.DelaySecondsTB)
        Me.TimeLapseControls.Controls.Add(Me.DelaySecondsLab)
        Me.TimeLapseControls.Location = New System.Drawing.Point(4, 22)
        Me.TimeLapseControls.Name = "TimeLapseControls"
        Me.TimeLapseControls.Padding = New System.Windows.Forms.Padding(3)
        Me.TimeLapseControls.Size = New System.Drawing.Size(312, 278)
        Me.TimeLapseControls.TabIndex = 1
        Me.TimeLapseControls.Text = "Timelapse Controls"
        Me.TimeLapseControls.UseVisualStyleBackColor = True
        '
        'Label8
        '
        Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(14, 31)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(112, 42)
        Me.Label8.TabIndex = 39
        Me.Label8.Text = "Exposure set on Imaging Controls Tab"
        '
        'FilePrefixLab
        '
        Me.FilePrefixLab.AutoSize = True
        Me.FilePrefixLab.Location = New System.Drawing.Point(12, 300)
        Me.FilePrefixLab.Name = "FilePrefixLab"
        Me.FilePrefixLab.Size = New System.Drawing.Size(51, 13)
        Me.FilePrefixLab.TabIndex = 39
        Me.FilePrefixLab.Text = "File prefix"
        '
        'FilePrefix
        '
        Me.FilePrefix.Location = New System.Drawing.Point(115, 297)
        Me.FilePrefix.Name = "FilePrefix"
        Me.FilePrefix.Size = New System.Drawing.Size(278, 20)
        Me.FilePrefix.TabIndex = 40
        Me.FilePrefix.Text = "test"
        '
        'LiveViewLab
        '
        Me.LiveViewLab.AutoSize = True
        Me.LiveViewLab.Location = New System.Drawing.Point(760, 25)
        Me.LiveViewLab.Name = "LiveViewLab"
        Me.LiveViewLab.Size = New System.Drawing.Size(52, 13)
        Me.LiveViewLab.TabIndex = 42
        Me.LiveViewLab.Text = "Live view"
        '
        'LiveViewOn
        '
        Me.LiveViewOn.AutoSize = True
        Me.LiveViewOn.Location = New System.Drawing.Point(818, 23)
        Me.LiveViewOn.Name = "LiveViewOn"
        Me.LiveViewOn.Size = New System.Drawing.Size(39, 17)
        Me.LiveViewOn.TabIndex = 43
        Me.LiveViewOn.TabStop = True
        Me.LiveViewOn.Text = "On"
        Me.LiveViewOn.UseVisualStyleBackColor = True
        '
        'LiveViewOff
        '
        Me.LiveViewOff.AutoSize = True
        Me.LiveViewOff.Checked = True
        Me.LiveViewOff.Location = New System.Drawing.Point(863, 23)
        Me.LiveViewOff.Name = "LiveViewOff"
        Me.LiveViewOff.Size = New System.Drawing.Size(39, 17)
        Me.LiveViewOff.TabIndex = 44
        Me.LiveViewOff.TabStop = True
        Me.LiveViewOff.Text = "Off"
        Me.LiveViewOff.UseVisualStyleBackColor = True
        '
        'LightsOffButton
        '
        Me.LightsOffButton.Location = New System.Drawing.Point(313, 89)
        Me.LightsOffButton.Name = "LightsOffButton"
        Me.LightsOffButton.Size = New System.Drawing.Size(80, 24)
        Me.LightsOffButton.TabIndex = 9
        Me.LightsOffButton.Text = "Lights Off"
        Me.LightsOffButton.UseVisualStyleBackColor = True
        '
        'DiscoButton
        '
        Me.DiscoButton.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.DiscoButton.ForeColor = System.Drawing.Color.Crimson
        Me.DiscoButton.Location = New System.Drawing.Point(313, 119)
        Me.DiscoButton.Name = "DiscoButton"
        Me.DiscoButton.Size = New System.Drawing.Size(80, 39)
        Me.DiscoButton.TabIndex = 45
        Me.DiscoButton.Text = "DISCO"
        Me.DiscoButton.UseVisualStyleBackColor = True
        '
        'OuterWhiteLight
        '
        Me.OuterWhiteLight.Location = New System.Drawing.Point(313, 59)
        Me.OuterWhiteLight.Name = "OuterWhiteLight"
        Me.OuterWhiteLight.Size = New System.Drawing.Size(80, 24)
        Me.OuterWhiteLight.TabIndex = 46
        Me.OuterWhiteLight.Text = "White Light"
        Me.OuterWhiteLight.UseVisualStyleBackColor = True
        '
        'PickDirectory
        '
        Me.PickDirectory.Location = New System.Drawing.Point(115, 230)
        Me.PickDirectory.Name = "PickDirectory"
        Me.PickDirectory.Size = New System.Drawing.Size(96, 23)
        Me.PickDirectory.TabIndex = 47
        Me.PickDirectory.Text = "Pick Directory"
        Me.PickDirectory.UseVisualStyleBackColor = True
        '
        'TcpClientActivex1
        '
        Me.TcpClientActivex1.AlwaysOverwrite = False
        Me.TcpClientActivex1.AVIBlockSize = 0
        Me.TcpClientActivex1.CaptureAudio = True
        Me.TcpClientActivex1.CaptureFPS = 15
        Me.TcpClientActivex1.CapturePathAndFileName = "C:\capture.avi"
        Me.TcpClientActivex1.Location = New System.Drawing.Point(736, 39)
        Me.TcpClientActivex1.MaxFramesToCapture = 324000
        Me.TcpClientActivex1.Name = "TcpClientActivex1"
        Me.TcpClientActivex1.NoDelay = False
        Me.TcpClientActivex1.PreviewFPS = 30
        Me.TcpClientActivex1.ReceiveBufferSize = 65536
        Me.TcpClientActivex1.RequireOKToCapture = False
        Me.TcpClientActivex1.SendBufferSize = 65536
        Me.TcpClientActivex1.Size = New System.Drawing.Size(299, 278)
        Me.TcpClientActivex1.SpawnCaptureInNewThread = True
        Me.TcpClientActivex1.TabIndex = 48
        Me.TcpClientActivex1.VideoSourceIndex = 0
        '
        'VBSample
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(1028, 352)
        Me.Controls.Add(Me.PickDirectory)
        Me.Controls.Add(Me.OuterWhiteLight)
        Me.Controls.Add(Me.DiscoButton)
        Me.Controls.Add(Me.LightsOffButton)
        Me.Controls.Add(Me.LiveViewOff)
        Me.Controls.Add(Me.LiveViewOn)
        Me.Controls.Add(Me.LiveViewLab)
        Me.Controls.Add(Me.FilePrefix)
        Me.Controls.Add(Me.FilePrefixLab)
        Me.Controls.Add(Me.ControlTabs)
        Me.Controls.Add(Me.TextBoxSavePath)
        Me.Controls.Add(Me.LabelSavePath)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.ImageQualityCmb)
        Me.Controls.Add(Me.progressBar)
        Me.Controls.Add(Me.MeteringModeCmb)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.AvCmb)
        Me.Controls.Add(Me.ISOSpeedCmb)
        Me.Controls.Add(Me.AEModeCmb)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.ExitBtn)
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.MaximizeBox = False
        Me.Name = "VBSample"
        Me.Text = "S"
        Me.ControlTabs.ResumeLayout(False)
        Me.ImagingControls.ResumeLayout(False)
        Me.ImagingControls.PerformLayout()
        Me.ManualControls.ResumeLayout(False)
        Me.ManualControls.PerformLayout()
        Me.TimeLapseControls.ResumeLayout(False)
        Me.TimeLapseControls.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    ' Modifications: none.
#Region "User defined attributes"

    ' Save as class variable, new delegates of event handlers.
    Public inPropertyEventHandler As New _
                EdsPropertyEventHandler(AddressOf CameraEventListener.handlePropertyEvent)
    Public inObjectEventHandler As New _
                EdsObjectEventHandler(AddressOf CameraEventListener.handleObjectEvent)
    Public inStateEventHandler As New _
                EdsStateEventHandler(AddressOf CameraEventListener.handleStateEvent)
    '
    Public Shared controller As CameraController
    Public Shared model As CameraModel
    Public Shared m_cmbTbl As Hashtable = New Hashtable


#End Region

    ' Modifications: some, see comments.
#Region "Camera Control (Mainly Canon written)"
    Delegate Sub UpdateDelegate(
        ByVal from As Observable,
        ByVal msg As Integer,
        ByVal data As Integer)

    ' Modifications: none. TO_DO: changes regarding lasty image display
    Sub UpdateWindow(ByVal from As Observable, ByVal msg As Integer, ByVal data As Integer) _
        Implements Observer.update

        'Get the name of this thread.
        Dim threadName As String =
        System.Threading.Thread.CurrentThread.Name()

        '// Make this form be able to be updated by other thread.
        If InvokeRequired Then
            'Create UpdateDelegate
            Dim dlg As New UpdateDelegate(AddressOf UpdateWindow)
            Try
                BeginInvoke(dlg, New Object() {from, msg, data})
            Catch e As Exception
                Return
            Finally
            End Try
            Return
        End If



        Select Case msg
            Case prog '//Progress of image downloading .
                progressBar.Value = data

            Case strt '// Start downloading.
                '//_progress.SetPos(0);

            Case cplt '// Complete downloading.
                progressBar.Value = 0

            Case updt '// Update properties.
                Dim propertyID As Integer = data
                Dim propData As Integer = model.getPropertyUInt32(propertyID)
                UpdateProperty(propertyID, propData)

            Case upls '// Update an available property list.
                Dim propertyID As Integer = data
                Dim desc As EdsPropertyDesc = model.getPropertyDesc(propertyID)
                UpdatePropertyDesc(propertyID, desc)

            Case warn '// Warning
                'InfoTextBox.Text = "Device Busy"

            Case errr '// Error
                '// Nothing to do because the first getting property from model 30D is sure to fail. 
                Dim ss As String
                ss = String.Format("%x", data)
                'InfoTextBox.Text = ss

            Case clse '// Close
                TakeBtn.Enabled = False
                progressBar.Enabled = False
                'InfoTextBox.Enabled = False
                AEModeCmb.Enabled = False
                TvCmb.Enabled = False
                AvCmb.Enabled = False
                ISOSpeedCmb.Enabled = False

        End Select

        If msg <> errr And msg <> warn Then
            'InfoTextBox.Text = ""
        End If


    End Sub

    ' Modifications: none.
    Sub UpdateProperty(ByVal propertyID As Integer, ByVal data As Integer)
        Dim propList As Hashtable = CameraProperty.g_PropList.Item(propertyID)
        Select Case propertyID
            Case kEdsPropID_AEModeSelect
                AEModeCmb.Text = propList.Item(data)
            Case kEdsPropID_ISOSpeed
                ISOSpeedCmb.Text = propList.Item(data)
            Case kEdsPropID_MeteringMode
                MeteringModeCmb.Text = propList.Item(data)
            Case kEdsPropID_Av
                AvCmb.Text = propList.Item(data)
            Case kEdsPropID_Tv
                'TvCmb.Text = propList.Item(data)
            Case kEdsPropID_ExposureCompensation
                'ExposureCompCmb.Text = propList.Item(data)
                'BLbox.Text = propList.Item(data)
                'GFPbox.Text = propList.Item(data)
                'CFPbox.Text = propList.Item(data)
                'mCHbox.Text = propList.Item(data)
                'mCHbox.Text = propList.Item(data)
            Case kEdsPropID_ImageQuality
                ImageQualityCmb.Text = propList.Item(data)
        End Select

    End Sub

    ' Modifications: none. TO_DO: changes regarding exposure in auto box.
    Sub UpdatePropertyDesc(ByVal propertyID As Integer, ByVal desc As EdsPropertyDesc)
        Dim err As Integer
        Dim iCnt As Integer
        Dim cmb As ComboBox = m_cmbTbl.Item(propertyID)
        Dim propList As Hashtable = CameraProperty.g_PropList.Item(propertyID)
        Dim propStr As String
        Dim propValueList As ArrayList = New ArrayList

        If cmb Is Nothing Then
            Return
        End If

        cmb.BeginUpdate()
        cmb.Items.Clear()
        For iCnt = 0 To desc.numElements - 1
            propStr = propList(desc.propDesc(iCnt))
            If propStr <> Nothing Then
                err = cmb.Items.Add(propStr)

                If cmb.Name = TvCmb.Name Then
                    BLbox.Items.Add(propStr)
                    CFPbox.Items.Add(propStr)
                    GFPbox.Items.Add(propStr)
                    mCHbox.Items.Add(propStr)
                    ManExposure.Items.Add(propStr)
                End If

                propValueList.Add(desc.propDesc(iCnt))
            End If
        Next

        cmb.Tag = propValueList ' Set the property value list

        If cmb.Name = TvCmb.Name Then
            BLbox.Tag = propValueList
            CFPbox.Tag = propValueList
            GFPbox.Tag = propValueList
            mCHbox.Tag = propValueList
            ManExposure.Tag = propValueList
        End If

        cmb.EndUpdate()
        If cmb.Items.Count = 0 Then
            cmb.Enabled = False '// No available item.
        Else
            cmb.Enabled = True
        End If

    End Sub

    ' Modifications: none.
    Public Function cameraModelFactory(ByVal camera As IntPtr, ByVal deviceInfo As EdsDeviceInfo) As CameraModel

        ' if Legacy protocol.
        If deviceInfo.DeviceSubType = 0 Then
            Return New CameraModelLegacy(camera)
        End If

        ' PTP protocol.
        Return New CameraModel(camera)

    End Function

    ' Modifications: none.
#Region "Window Events"

    ' Modifications: none.
    Private t As Thread
    Private Sub TakeBtn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TakeBtn.Click
        '
        ' Release button
        '
        CheckForIllegalCrossThreadCalls = False
        t = New Thread(AddressOf take_automatic_pictures)
        t.Start()

    End Sub

    ' Modifications: none.
    Private Sub ExitBtn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitBtn.Click
        ' Quit button
        Me.Close()

        End
    End Sub

#End Region

    ' Modifications: none. TO_DO: changes regarding connection to HID and go to filter, and lights off.
    Private Sub VBSample_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConnectToHID(Me)
        ' Modifications: none.
        Dim err As Integer = EDS_ERR_OK
        Dim cameraList As IntPtr = Nothing
        Dim camera As IntPtr = Nothing
        Dim count As Integer = 0
        Dim isSDKLoaded As Boolean = False
        Dim propObj As New CameraProperty

        ' connect property id to combobox. 
        m_cmbTbl.Add(kEdsPropID_AEMode, Me.AEModeCmb)
        m_cmbTbl.Add(kEdsPropID_ISOSpeed, Me.ISOSpeedCmb)
        m_cmbTbl.Add(kEdsPropID_Av, Me.AvCmb)
        m_cmbTbl.Add(kEdsPropID_Tv, Me.TvCmb)
        m_cmbTbl.Add(kEdsPropID_MeteringMode, Me.MeteringModeCmb)
        'm_cmbTbl.Add(kEdsPropID_ExposureCompensation, Me.ExposureCompCmb)
        'BLbox.Items.Add(m_cmbTbl.Item(Me.ExposureCompCmb))
        m_cmbTbl.Add(kEdsPropID_ImageQuality, Me.ImageQualityCmb)


        err = EdsInitializeSDK()

        ' Modifications: none.
        If err = EDS_ERR_OK Then

            isSDKLoaded = True

        End If

        ' Modifications: none.
        If err = EDS_ERR_OK Then

            err = EdsGetCameraList(cameraList)

        End If

        ' Modifications: none.
        If err = EDS_ERR_OK Then

            err = EdsGetChildCount(cameraList, count)
            If count = 0 Then
                err = EDS_ERR_DEVICE_NOT_FOUND
            End If

        End If

        ' Modifications: none.
        '// Get the first camera.
        If err = EDS_ERR_OK Then

            err = EdsGetChildAtIndex(cameraList, 0, camera)

        End If


        Dim deviceInfo As EdsDeviceInfo = Nothing

        ' Modifications: none.
        If err = EDS_ERR_OK Then

            err = EdsGetDeviceInfo(camera, deviceInfo)

            If err = EDS_ERR_OK And IsNothing(camera) = True Then
                err = EDS_ERR_DEVICE_NOT_FOUND
            End If

        End If

        ' Modifications: none.
        If IsNothing(cameraList) = False Then

            EdsRelease(cameraList)

        End If

        ' Modifications: none.
        '// Create the camera model 
        If err = EDS_ERR_OK Then

            model = cameraModelFactory(camera, deviceInfo)

            If IsNothing(model) = True Then
                err = EDS_ERR_DEVICE_NOT_FOUND

            End If
        End If

        ' Modifications: none. TO_DO: change regarding lights off.
        If err <> EDS_ERR_OK Then

            MessageBox.Show("Cannot detect camera")

        End If

        ' Modifications: none.
        If err = EDS_ERR_OK Then

            '// Create a controller
            controller = New CameraController

            '// Set the model to this controller.
            controller.setCameraModel(model)

            '// Make notify the model updating to the view.
            model.addObserver(Me)

            ' ------------------------------------------------------------------------
            ' ------------------------------------------------------------------------
            ' You should create class instance of delegates of event handlers 
            ' with 'new' expressly if its attribute is Shared, 
            ' because System sometimes do garbage-collect these delegates.
            '
            '
            ' This error occurs.
            '
            ' CallbackOnCollectedDelegate is detected.
            ' Message: Callback was called with
            ' garbage-collected delegate of  
            ' Type() 'VBSample3!VBSample3.EDSDKTypes+EdsPropertyEventHandler::Invoke' 
            ' 
            ' It will be able to make data loss or application clash.
            ' You should stock delegates when you want to send delegate to unmanaged code.
            '
            ' ------------------------------------------------------------------------
            If err = EDS_ERR_OK Then

                err = EdsSetPropertyEventHandler(camera, kEdsPropertyEvent_All,
                        inPropertyEventHandler, IntPtr.Zero)
            End If

            '// Set ObjectEventHandler
            If err = EDS_ERR_OK Then
                err = EdsSetObjectEventHandler(camera, kEdsObjectEvent_All,
                    inObjectEventHandler, IntPtr.Zero)

            End If

            '// Set StateEventHandler
            If err = EDS_ERR_OK Then
                err = EdsSetCameraStateEventHandler(camera, kEdsStateEvent_All,
                    inStateEventHandler, IntPtr.Zero)
            End If

        End If


        ' Modifications: none.
        If err <> EDS_ERR_OK Then

            If IsNothing(camera) = False Then
                EdsRelease(camera)
                camera = Nothing
            End If

            If (isSDKLoaded) Then
                EdsTerminateSDK()
            End If

            If IsNothing(model) = False Then
                model = Nothing
            End If


            If IsNothing(controller) = False Then
                controller = Nothing
            End If


            End

        End If

        '//Execute the controller.
        controller.run()

    End Sub

    ' Modifications: none. TO_DO: changes regarding lights off and no filter.
    Private Sub VBSample_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing

        controller.actionPerformed("close")

        If IsNothing(model) Then
            If IsNothing(model.getCameraObject()) = False Then
                EdsRelease(model.getCameraObject())
            End If
        End If

        EdsTerminateSDK()
        lights.all_off()
        Goto_Filter(no_filter)

    End Sub

    ' Modifications: none.
    Private Sub AEModeCmb_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AEModeCmb.SelectionChangeCommitted
        Dim cmb As ComboBox = CType(sender, ComboBox) ' "sender" is the combobox
        Dim propValueList As ArrayList = CType(cmb.Tag, ArrayList)
        Dim data As Integer = propValueList.Item(cmb.SelectedIndex)

        controller.actionPerformed("set", kEdsPropID_AEMode, data)

    End Sub

    ' Modifications: none.
    Private Sub TvCmb_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TvCmb.SelectionChangeCommitted
        Dim cmb As ComboBox = CType(sender, ComboBox) ' "sender" is the combobox
        Dim propValueList As ArrayList = CType(cmb.Tag, ArrayList)
        Dim data As Integer = propValueList.Item(cmb.SelectedIndex)
        Dim id As Integer

        'If cmb.Equals(Me.AEModeCmb) Then
        '    id = kEdsPropID_AEMode
        'ElseIf cmb.Equals(Me.TvCmb) Then
        '    id = kEdsPropID_Tv
        'ElseIf cmb.Equals(Me.AvCmb) Then
        '    id = kEdsPropID_Av
        'ElseIf cmb.Equals(Me.ISOSpeedCmb) Then
        '    id = kEdsPropID_ISOSpeed
        'ElseIf cmb.Equals(Me.MeteringModeCmb) Then
        '    id = kEdsPropID_MeteringMode
        'ElseIf cmb.Equals(Me.ExposureCompCmb) Then
        '    id = kEdsPropID_ExposureCompensation
        'Else
        '    Console.WriteLine("What's this?")
        'End If

        id = kEdsPropID_Tv
        controller.actionPerformed("set", id, data)

    End Sub

    ' Modifications: none.
    Private Sub AvCmb_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AvCmb.SelectionChangeCommitted
        Dim cmb As ComboBox = CType(sender, ComboBox) ' "sender" is the combobox
        Dim propValueList As ArrayList = CType(cmb.Tag, ArrayList)
        Dim data As Integer = propValueList.Item(cmb.SelectedIndex)

        controller.actionPerformed("set", kEdsPropID_Av, data)

    End Sub

    ' Modifications: none.
    Private Sub ISOSpeedCmb_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ISOSpeedCmb.SelectionChangeCommitted
        Dim cmb As ComboBox = CType(sender, ComboBox) ' "sender" is the combobox
        Dim propValueList As ArrayList = CType(cmb.Tag, ArrayList)
        Dim data As Integer = propValueList.Item(cmb.SelectedIndex)

        controller.actionPerformed("set", kEdsPropID_ISOSpeed, data)

    End Sub

    ' Modifications: none.
    Private Sub MeteringModeCmb_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MeteringModeCmb.SelectionChangeCommitted

        Dim cmb As ComboBox = CType(sender, ComboBox) ' "sender" is the combobox
        Dim propValueList As ArrayList = CType(cmb.Tag, ArrayList)
        Dim data As Integer = propValueList.Item(cmb.SelectedIndex)

        controller.actionPerformed("set", kEdsPropID_MeteringMode, data)

    End Sub

    ' Modifications: none.
    Private Sub ExposureCompCmb_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs)
        'Dim cmb As ComboBox = CType(sender, ComboBox) ' "sender" is the combobox
        'Dim propValueList As ArrayList = CType(cmb.Tag, ArrayList)
        'Dim data As Integer = propValueList.Item(cmb.SelectedIndex)

        'controller.actionPerformed("set", kEdsPropID_ExposureCompensation, data)
        'controller.actionPerformed("set", kEdsPropID_ExposureCompensation, data)

    End Sub

    ' Modifications: none.
    Private Sub ImageQualityCmb_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ImageQualityCmb.SelectionChangeCommitted


        Dim cmb As ComboBox = CType(sender, ComboBox) ' "sender" is the combobox
        Dim propValueList As ArrayList = CType(cmb.Tag, ArrayList)
        Dim data As Integer = propValueList.Item(cmb.SelectedIndex)

        controller.actionPerformed("set", kEdsPropID_ImageQuality, data)

    End Sub

#End Region

    ' NQ ER
#Region "NQuinones ERand: Camera additions"
    Public lastpicturesend As Date = DateAndTime.Now
    Public savequeue As Queue = New Queue
    Public lights As LightControl = New LightControl

    ' Name for photo
    Private Sub TextBoxSavePath_TextChanged(sender As Object, e As EventArgs) Handles TextBoxSavePath.TextChanged
        ' savepath = TextBoxSavePath.Text
        'Console.WriteLine("FromDlg:" & savepath)
    End Sub

    ' Timelapse button
    Private timelapse As Thread = New Thread(AddressOf take_timelapse)
    Private Sub ButtonTimelapseStart_Click(sender As Object, e As EventArgs) Handles ButtonTimelapseStart.Click
        'Console.WriteLine("Timelapse button clicked.")
        timelapse = New Thread(AddressOf take_timelapse)
        timelapse.Start()
    End Sub

    Private timeinterval As Integer = 60
    Private t2 As Thread

    Private Sub take_timelapse()
        Dim photostaken As Integer = 0
        Dim alltime As Integer = 0
        Dim stoppic As Integer = 0
        Dim stoptime As Integer = 0
        Dim measureinpics As Boolean = True
        If Pictures.Checked Then
            stoppic = MaxTime.Text
        End If
        If Seconds.Checked Then
            stoptime = MaxTime.Text
            measureinpics = False
        End If
        While True
            Me.CheckForIllegalCrossThreadCalls = False
            If measureinpics AndAlso (photostaken >= stoppic) Then ' stop timelapse if max number of pictures are taken
                NextPictureSeconds.Text = "Timelapse stopped"
                timelapse.Abort()
                Exit While
            ElseIf Not measureinpics AndAlso (alltime >= stoptime) Then ' stop timelapse if max time has passed
                NextPictureSeconds.Text = "Timelapse stopped"
                timelapse.Abort()
                Exit While
            End If

            t2 = New Thread(AddressOf take_automatic_pictures)
            t2.Start()
            photostaken += 1
            PhotosTakenBox.Text = "Taken " & photostaken & " photo sets"
            timeinterval = DelaySecondsTB.Text
            Dim mintime As Integer = 30
            If timeinterval < mintime Then
                timeinterval = mintime
                DelaySecondsTB.Text = mintime
            End If
            For secsleft As Integer = DelaySecondsTB.Text To 0 Step -1
                NextPictureSeconds.Text = secsleft & " seconds to next picture"
                Thread.Sleep(1000)
            Next
            alltime += DelaySecondsTB.Text
            If t2.IsAlive Then
                t2.Abort()
            End If

        End While

    End Sub

    Private Sub take_automatic_pictures()
        timeoutsavequeue()

        Dim nameprefix As String = DateTime.Now.ToString("yyyyMMdd_hhmmss")
        Dim path As String = TextBoxSavePath.Text
        Dim l As Integer = path.Length
        If path.Chars(l - 1) <> "\" Then
            path = path & "\"
        End If
        path = path & nameprefix & "_" & FilePrefix.Text

        'Console.WriteLine("fxn")

        If Not Brightfield.Checked And Not Backlight.Checked And Not GFP.Checked And Not CFP.Checked And Not mCherry.Checked Then
            'Console.WriteLine("no boxes checked")
            TakeColorPicture(white_light, no_filter, TvCmb, path & "_BF.jpg")
        End If

        If Brightfield.Checked Then
            TakeColorPicture(white_light, no_filter, TvCmb, path & "_BF.jpg")
            'Console.WriteLine(DateTime.Now)
            'Console.WriteLine("brightfield")
            'Thread.Sleep(1500)
            'Console.WriteLine(DateTime.Now)
        End If

        If Backlight.Checked Then
            TakeColorPicture(back_light, no_filter, BLbox, path & "_BL.jpg")
            'Console.WriteLine("Backlight")
            'Thread.Sleep(1500)
        End If

        If GFP.Checked Then
            TakeColorPicture(GFP_light, GFP_filter, GFPbox, path & "_GFP.jpg")
            'Console.WriteLine("GFP")
            'Thread.Sleep(1500)
        End If

        If CFP.Checked Then
            TakeColorPicture(CFP_light, CFP_filter, CFPbox, path & "_CFP.jpg")
            'Thread.Sleep(1500)
        End If

        If mCherry.Checked Then
            TakeColorPicture(mCherry_light, mCherry_filter, mCHbox, path & "_mCher.jpg")
        End If

        Goto_Filter(no_filter)
        If LightsOutBox.Checked Then
            lights.all_off()
        Else
            lights.pick_light(white_light)
        End If

    End Sub

    Private Sub timeoutsavequeue()
        If (DateAndTime.DateDiff(DateInterval.Second, lastpicturesend, DateAndTime.Now) > 10) Then
            savequeue.Clear()
            'MsgBox("Queue cleared!")
        End If
    End Sub

    Private Sub TakeColorPicture(ByVal lightnum As Integer, ByVal filternum As Integer, ByRef TvBox As ComboBox, ByVal name As String)
        Dim filteroffset As Integer = Math.Abs(filterpos - filternum)
        Dim timeoutcount As Integer = 0
        savepath = name

        If lightnum <> white_light Then
            lights.all_off()
        End If

        If filteroffset > 0 Then
            Goto_Filter(filternum)

            Debug.Print("Filter moving to" & filternum)
            Thread.Sleep(1000)
            ismoving = True

            While (ismoving And timeoutcount < 200)
                Wheel_Query()
                Debug.Print("Wheel Query " & timeoutcount)
                Thread.Sleep(50)
                timeoutcount += 1
            End While

            If timeoutcount = 20 Then
                'While BufferIn(1) <> 3
                'Thread.Sleep(50)
                'End While
                Debug.Print("Wheel Timeout!")

            End If
        End If

        lights.pick_light(lightnum)
        Thread.Sleep(200)

        'This stuff sets the exposure and F-stop information:
        Dim propValueList As ArrayList = CType(TvBox.Tag, ArrayList)
        Dim data As Integer
        Dim exposurelengthfrommenu As Integer = TvBox.SelectedIndex

        'Console.WriteLine(exposurelengthfrommenu)

        If exposurelengthfrommenu = -1 Then
            exposurelengthfrommenu = 25
        End If

        data = propValueList.Item(exposurelengthfrommenu)

        controller.actionPerformed("set", kEdsPropID_Tv, data)
        'end setting exposure info

        CameraEventListener.waitlock = True


        CheckForIllegalCrossThreadCalls = False

        TakePictureWithName(name)

        'QueueBox.Text = "Enqueued: " & savequeue.Count
        While CameraEventListener.waitlockis()
            Thread.Sleep(100)
        End While
        'Thread.Sleep(shutterwaitarray(exposurelengthfrommenu) + 600)

        'QueueBox.Text = "Enqueued: " & savequeue.Count

        'controller.actionPerformed("takepicture")
    End Sub

    Private Sub TakePictureWithName(name As String)
        savequeue.Enqueue(name)
        lastpicturesend = DateAndTime.Now
        controller.actionPerformed("takepicture")
    End Sub

    Private Sub ButtonTimeLapseStop_Click(sender As Object, e As EventArgs) Handles ButtonTimelapseStop.Click
        If timelapse.IsAlive Then
            timelapse.Abort()
        End If
        NextPictureSeconds.Text = "Timelapse stopped"
    End Sub

    Private Sub PhotosTakenBox_Click(sender As Object, e As EventArgs) Handles PhotosTakenBox.Click

    End Sub

    Private Sub DelaySeconds_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub NextPictureSeconds_Click(sender As Object, e As EventArgs) Handles NextPictureSeconds.Click

    End Sub

    Private Sub DelaySecondsTB_TextChanged(sender As Object, e As EventArgs) Handles DelaySecondsTB.TextChanged

    End Sub


    Private Sub progressBar_Click(sender As Object, e As EventArgs) Handles progressBar.Click

    End Sub

    Private Sub Label8_Click(sender As Object, e As EventArgs) Handles LenghtTimelapseLab.Click

    End Sub


    Private Sub Pictures_CheckedChanged(sender As Object, e As EventArgs) Handles Pictures.CheckedChanged

    End Sub

    Private Sub Seconds_CheckedChanged(sender As Object, e As EventArgs) Handles Seconds.CheckedChanged

    End Sub

    Private Sub MaxTime_TextChanged(sender As Object, e As EventArgs) Handles MaxTime.TextChanged

    End Sub

    Private Sub TabPage1_Click(sender As Object, e As EventArgs) Handles ImagingControls.Click

    End Sub

    Private Sub GFP_CheckedChanged(sender As Object, e As EventArgs) Handles GFP.CheckedChanged

    End Sub

    Private Sub CFP_CheckedChanged(sender As Object, e As EventArgs) Handles CFP.CheckedChanged

    End Sub

    Private Sub mCherry_CheckedChanged(sender As Object, e As EventArgs) Handles mCherry.CheckedChanged

    End Sub

    Private Sub Brightfield_CheckedChanged(sender As Object, e As EventArgs) Handles Brightfield.CheckedChanged

    End Sub

    Private Sub ExposureCompCmb_SelectedIndexChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub LiveViewOff_CheckedChanged(sender As Object, e As EventArgs) Handles LiveViewOff.CheckedChanged
        If LiveViewOff.Checked Then

        End If
    End Sub


    Private Sub LiveViewOn_CheckedChanged(sender As Object, e As EventArgs) Handles LiveViewOn.CheckedChanged
        If LiveViewOn.Checked Then
            'Timer1.Start()
            'EDSDK.EdsSetPropertyData()
            'Dim testLV = EdsCreateEvfImageRef(IntPtr.Zero, IntPtr.Zero)
        End If

        'Dim max As Integer = 0
        'While LiveViewOn.Checked
        'savepath = TextBoxSavePath.Text & FilePrefix.Text & "LiveViewTest.jpg"
        'controller.actionPerformed("takepicture")
        'Thread.Sleep(2000)
        'LiveView.Image = Image.FromFile(TextBoxSavePath.Text & FilePrefix.Text & "LiveViewTest.jpg")
        'Thread.Sleep(2000)
        'max += 1
        'If max > 3 Then
        'Exit While
        'End If
        'End While
    End Sub

    Private LV As Thread
    Private Sub Timer1_Tick(sender As Object, e As EventArgs)
        LV = New Thread(AddressOf take_automatic_pictures)
        LV.Start()

        'Console.WriteLine("timer")
    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click

    End Sub

    Private Sub GFPlight_Click(sender As Object, e As EventArgs) Handles GFPlight.Click
        lights.pick_light(GFP_light)
    End Sub

    Private Sub WhiteLight_Click(sender As Object, e As EventArgs) Handles WhiteLight.Click
        lights.pick_light(white_light)
    End Sub

    Private Sub CFPlight_Click(sender As Object, e As EventArgs) Handles CFPlight.Click
        lights.pick_light(CFP_light)
    End Sub

    Private Sub mChrLight_Click(sender As Object, e As EventArgs) Handles mChrLight.Click
        lights.pick_light(mCherry_light)
    End Sub

    Private Sub LightsOffButton_Click(sender As Object, e As EventArgs) Handles LightsOffButton.Click
        lights.all_off()
    End Sub

    Private Sub NoFilter_Click(sender As Object, e As EventArgs) Handles NoFilter.Click
        Goto_Filter(no_filter)
    End Sub

    Private Sub GFPfilter_Click(sender As Object, e As EventArgs) Handles GFPfilter.Click
        'Console.WriteLine("here")
        Goto_Filter(GFP_filter)
    End Sub

    Private Sub CFPfilter_Click(sender As Object, e As EventArgs) Handles CFPfilter.Click
        Goto_Filter(CFP_filter)
    End Sub

    Private Sub mChFilter_Click(sender As Object, e As EventArgs) Handles mChFilter.Click
        Goto_Filter(mCherry_filter)
    End Sub

    Private discofo As DiscoForm = New DiscoForm
    Public Sub discodisco()
        Do
            'lights.disco(544)
            lights.disco(294)

        Loop
    End Sub

    Public Sub white_lights()
        lights.pick_light(white_light)
    End Sub

    'Private Sub MegaDisco_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button11.Click
    '    discofo = New DiscoForm()
    '    discofo.Show()

    'End Sub
    Private Sub DiscoButton_Click(sender As Object, e As EventArgs) Handles DiscoButton.Click
        'Console.WriteLine(hidIsAvailable(VendorID, ProductID))
        discofo = New DiscoForm()
        discofo.Show()
    End Sub

    Private Sub LightsOutBox_CheckedChanged(sender As Object, e As EventArgs) Handles LightsOutBox.CheckedChanged

    End Sub

    Private Sub YFPlight_Click(sender As Object, e As EventArgs)
        lights.pick_light(YFP_light)
    End Sub

    Private Sub YFPfilter_Click(sender As Object, e As EventArgs)
        Goto_Filter(YFP_filter)
    End Sub

    Private Sub ManExposure_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ManExposure.SelectedIndexChanged

    End Sub

    Private Sub ManTakePicture_Click(sender As Object, e As EventArgs) Handles ManTakePicture.Click
        Dim propValueList As ArrayList = CType(ManExposure.Tag, ArrayList)
        Dim data As Integer
        Dim exposurelengthfrommenu As Integer = ManExposure.SelectedIndex

        'Console.WriteLine(exposurelengthfrommenu)

        If exposurelengthfrommenu = -1 Then
            exposurelengthfrommenu = 25
        End If

        data = propValueList.Item(exposurelengthfrommenu)

        controller.actionPerformed("set", kEdsPropID_Tv, data)

        Dim nameprefix As String = DateTime.Now.ToString("yyyyMMdd_hhmmss")
        Dim path As String = TextBoxSavePath.Text
        Dim l As Integer = path.Length
        If path.Chars(l - 1) <> "\" Then
            path = path & "\"
        End If
        path = path & nameprefix & "_" & FilePrefix.Text

        TakePictureWithName(path)
    End Sub

    Private Sub OuterWhiteLight_Click(sender As Object, e As EventArgs) Handles OuterWhiteLight.Click
        lights.pick_light(white_light)
    End Sub

    Private Sub PickDirectory_Click(sender As Object, e As EventArgs) Handles PickDirectory.Click
        Dim fbDialog1 As FolderBrowserDialog = New System.Windows.Forms.FolderBrowserDialog()

        fbDialog1.ShowDialog()


        Dim result As String = fbDialog1.SelectedPath

        If result <> "" Then
            result = result & "\"
            'savepath = result
            TextBoxSavePath.Text = result
        End If
    End Sub




    ' Natalia tests


    ' Michael

#End Region



End Class

