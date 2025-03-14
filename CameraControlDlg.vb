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
    Implements IMessageFilter
    ' Changes dirItemInfo.szFileName in DownloadCommand.vb
    Public Shared savepath As String
    Dim WithEvents GX As GXRobotControlNamespace.GXRobotControl

#Region "Lightbox definitions"
    Public mCherry_light As Integer = 6
    Public CFP_light As Integer = 3
    Public YFP_light As Integer = 4
    Public GFP_light As Integer = 4
    Public white_light As Integer = 5
    Public control_light As Integer = 0
    Public back_light As Integer = 10
    Friend WithEvents LightsOff As System.Windows.Forms.Button
#End Region

#Region "filter definitions and functions"
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
    Friend WithEvents BFBox As ComboBox
    Friend WithEvents ManTakePicture As Button
    Friend WithEvents ExposureLab As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents OuterWhiteLight As Button
    Friend WithEvents PickDirectory As Button
    ' Friend WithEvents TcpClientActivex1 As TCPCamActivex.TCPClientActivex
    Friend WithEvents BackLightButton As Button
    Friend WithEvents LoadButton As Button
    Friend WithEvents SaveButton As Button
    Friend WithEvents OpenDirectory As Button
    Public WithEvents LastImageBox As PictureBox
    Friend WithEvents Label9 As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents CameraRelease As Button
    Friend WithEvents RobotOff As RadioButton
    Friend WithEvents RobotOn As RadioButton
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents Label11 As Label
    Friend WithEvents StartRobotButton As Button
    Friend WithEvents CloseGripper As Button
    Friend WithEvents OpenGripper As Button
    Friend WithEvents Label12 As Label
    Friend WithEvents PlateHeight As NumericUpDown
    Friend WithEvents StackFourFull As CheckBox
    Friend WithEvents StackThreeFull As CheckBox
    Friend WithEvents StackTwoFull As CheckBox
    Friend WithEvents StackOneFull As CheckBox
    Friend WithEvents CheckFullStacksLabel As Label
    Friend WithEvents Reset As Button
    Friend WithEvents EmailTextBox As TextBox
    Friend WithEvents Timer1 As Windows.Forms.Timer
    Friend WithEvents RobotOffAfterImaging As CheckBox
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
        Me.components = New System.ComponentModel.Container()
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
        Me.LoadButton = New System.Windows.Forms.Button()
        Me.SaveButton = New System.Windows.Forms.Button()
        Me.LightsOutBox = New System.Windows.Forms.CheckBox()
        Me.mCHbox = New System.Windows.Forms.ComboBox()
        Me.CFPbox = New System.Windows.Forms.ComboBox()
        Me.BFBox = New System.Windows.Forms.ComboBox()
        Me.GFPbox = New System.Windows.Forms.ComboBox()
        Me.BLbox = New System.Windows.Forms.ComboBox()
        Me.Backlight = New System.Windows.Forms.CheckBox()
        Me.ExposureLab = New System.Windows.Forms.Label()
        Me.mCherry = New System.Windows.Forms.CheckBox()
        Me.CFP = New System.Windows.Forms.CheckBox()
        Me.GFP = New System.Windows.Forms.CheckBox()
        Me.Brightfield = New System.Windows.Forms.CheckBox()
        Me.ManualControls = New System.Windows.Forms.TabPage()
        Me.BackLightButton = New System.Windows.Forms.Button()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.ManTakePicture = New System.Windows.Forms.Button()
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
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.EmailTextBox = New System.Windows.Forms.TextBox()
        Me.Reset = New System.Windows.Forms.Button()
        Me.StackFourFull = New System.Windows.Forms.CheckBox()
        Me.StackThreeFull = New System.Windows.Forms.CheckBox()
        Me.StackTwoFull = New System.Windows.Forms.CheckBox()
        Me.StackOneFull = New System.Windows.Forms.CheckBox()
        Me.CheckFullStacksLabel = New System.Windows.Forms.Label()
        Me.PlateHeight = New System.Windows.Forms.NumericUpDown()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.CloseGripper = New System.Windows.Forms.Button()
        Me.OpenGripper = New System.Windows.Forms.Button()
        Me.StartRobotButton = New System.Windows.Forms.Button()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.RobotOff = New System.Windows.Forms.RadioButton()
        Me.RobotOn = New System.Windows.Forms.RadioButton()
        Me.FilePrefixLab = New System.Windows.Forms.Label()
        Me.FilePrefix = New System.Windows.Forms.TextBox()
        Me.LiveViewLab = New System.Windows.Forms.Label()
        Me.LiveViewOn = New System.Windows.Forms.RadioButton()
        Me.LiveViewOff = New System.Windows.Forms.RadioButton()
        Me.LightsOffButton = New System.Windows.Forms.Button()
        Me.DiscoButton = New System.Windows.Forms.Button()
        Me.OuterWhiteLight = New System.Windows.Forms.Button()
        Me.PickDirectory = New System.Windows.Forms.Button()
        Me.OpenDirectory = New System.Windows.Forms.Button()
        Me.LastImageBox = New System.Windows.Forms.PictureBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.CameraRelease = New System.Windows.Forms.Button()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.RobotOffAfterImaging = New System.Windows.Forms.CheckBox()
        Me.ControlTabs.SuspendLayout()
        Me.ImagingControls.SuspendLayout()
        Me.ManualControls.SuspendLayout()
        Me.TimeLapseControls.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        CType(Me.PlateHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LastImageBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TakeBtn
        '
        Me.TakeBtn.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TakeBtn.Location = New System.Drawing.Point(184, 205)
        Me.TakeBtn.Name = "TakeBtn"
        Me.TakeBtn.Size = New System.Drawing.Size(110, 67)
        Me.TakeBtn.TabIndex = 0
        Me.TakeBtn.Text = "Take Picture"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label2.Location = New System.Drawing.Point(26, 460)
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
        Me.Label3.Location = New System.Drawing.Point(47, 79)
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
        Me.Label5.Location = New System.Drawing.Point(25, 52)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(50, 13)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Aperture:"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'AEModeCmb
        '
        Me.AEModeCmb.Location = New System.Drawing.Point(129, 456)
        Me.AEModeCmb.Name = "AEModeCmb"
        Me.AEModeCmb.Size = New System.Drawing.Size(166, 21)
        Me.AEModeCmb.TabIndex = 11
        '
        'ISOSpeedCmb
        '
        Me.ISOSpeedCmb.Location = New System.Drawing.Point(81, 76)
        Me.ISOSpeedCmb.Name = "ISOSpeedCmb"
        Me.ISOSpeedCmb.Size = New System.Drawing.Size(166, 21)
        Me.ISOSpeedCmb.TabIndex = 12
        '
        'AvCmb
        '
        Me.AvCmb.Location = New System.Drawing.Point(81, 49)
        Me.AvCmb.Name = "AvCmb"
        Me.AvCmb.Size = New System.Drawing.Size(166, 21)
        Me.AvCmb.TabIndex = 14
        '
        'TvCmb
        '
        Me.TvCmb.Location = New System.Drawing.Point(43, 209)
        Me.TvCmb.Name = "TvCmb"
        Me.TvCmb.Size = New System.Drawing.Size(111, 21)
        Me.TvCmb.TabIndex = 15
        Me.TvCmb.Text = "Waiting for values"
        '
        'MeteringModeCmb
        '
        Me.MeteringModeCmb.Location = New System.Drawing.Point(125, 421)
        Me.MeteringModeCmb.Name = "MeteringModeCmb"
        Me.MeteringModeCmb.Size = New System.Drawing.Size(166, 21)
        Me.MeteringModeCmb.TabIndex = 17
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(26, 421)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(81, 13)
        Me.Label1.TabIndex = 16
        Me.Label1.Text = "Metering Mode:"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'ExitBtn
        '
        Me.ExitBtn.BackColor = System.Drawing.SystemColors.Control
        Me.ExitBtn.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ExitBtn.Location = New System.Drawing.Point(9, 317)
        Me.ExitBtn.Name = "ExitBtn"
        Me.ExitBtn.Size = New System.Drawing.Size(119, 29)
        Me.ExitBtn.TabIndex = 4
        Me.ExitBtn.Text = "Quit"
        Me.ExitBtn.UseVisualStyleBackColor = False
        '
        'progressBar
        '
        Me.progressBar.Location = New System.Drawing.Point(9, 289)
        Me.progressBar.Name = "progressBar"
        Me.progressBar.Size = New System.Drawing.Size(245, 10)
        Me.progressBar.TabIndex = 21
        '
        'ImageQualityCmb
        '
        Me.ImageQualityCmb.Location = New System.Drawing.Point(81, 103)
        Me.ImageQualityCmb.Name = "ImageQualityCmb"
        Me.ImageQualityCmb.Size = New System.Drawing.Size(166, 21)
        Me.ImageQualityCmb.TabIndex = 22
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label7.Location = New System.Drawing.Point(4, 106)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(74, 13)
        Me.Label7.TabIndex = 23
        Me.Label7.Text = "Image Quality:"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LabelSavePath
        '
        Me.LabelSavePath.AutoSize = True
        Me.LabelSavePath.Location = New System.Drawing.Point(18, 195)
        Me.LabelSavePath.Name = "LabelSavePath"
        Me.LabelSavePath.Size = New System.Drawing.Size(59, 13)
        Me.LabelSavePath.TabIndex = 24
        Me.LabelSavePath.Text = "Save path:"
        Me.LabelSavePath.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'TextBoxSavePath
        '
        Me.TextBoxSavePath.Location = New System.Drawing.Point(11, 211)
        Me.TextBoxSavePath.Name = "TextBoxSavePath"
        Me.TextBoxSavePath.Size = New System.Drawing.Size(243, 20)
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
        Me.ControlTabs.Controls.Add(Me.TabPage1)
        Me.ControlTabs.Location = New System.Drawing.Point(271, 12)
        Me.ControlTabs.Name = "ControlTabs"
        Me.ControlTabs.SelectedIndex = 0
        Me.ControlTabs.Size = New System.Drawing.Size(380, 304)
        Me.ControlTabs.TabIndex = 38
        '
        'ImagingControls
        '
        Me.ImagingControls.Controls.Add(Me.LoadButton)
        Me.ImagingControls.Controls.Add(Me.SaveButton)
        Me.ImagingControls.Controls.Add(Me.LightsOutBox)
        Me.ImagingControls.Controls.Add(Me.mCHbox)
        Me.ImagingControls.Controls.Add(Me.CFPbox)
        Me.ImagingControls.Controls.Add(Me.BFBox)
        Me.ImagingControls.Controls.Add(Me.GFPbox)
        Me.ImagingControls.Controls.Add(Me.BLbox)
        Me.ImagingControls.Controls.Add(Me.Backlight)
        Me.ImagingControls.Controls.Add(Me.ExposureLab)
        Me.ImagingControls.Controls.Add(Me.mCherry)
        Me.ImagingControls.Controls.Add(Me.CFP)
        Me.ImagingControls.Controls.Add(Me.GFP)
        Me.ImagingControls.Controls.Add(Me.Brightfield)
        Me.ImagingControls.Controls.Add(Me.TakeBtn)
        Me.ImagingControls.Location = New System.Drawing.Point(4, 22)
        Me.ImagingControls.Name = "ImagingControls"
        Me.ImagingControls.Padding = New System.Windows.Forms.Padding(3)
        Me.ImagingControls.Size = New System.Drawing.Size(372, 278)
        Me.ImagingControls.TabIndex = 0
        Me.ImagingControls.Text = "Imaging Controls"
        Me.ImagingControls.UseVisualStyleBackColor = True
        '
        'LoadButton
        '
        Me.LoadButton.Location = New System.Drawing.Point(15, 239)
        Me.LoadButton.Name = "LoadButton"
        Me.LoadButton.Size = New System.Drawing.Size(113, 25)
        Me.LoadButton.TabIndex = 23
        Me.LoadButton.Text = "Load Settings"
        Me.LoadButton.UseVisualStyleBackColor = True
        '
        'SaveButton
        '
        Me.SaveButton.Location = New System.Drawing.Point(15, 208)
        Me.SaveButton.Name = "SaveButton"
        Me.SaveButton.Size = New System.Drawing.Size(113, 25)
        Me.SaveButton.TabIndex = 22
        Me.SaveButton.Text = "Save Settings"
        Me.SaveButton.UseVisualStyleBackColor = True
        '
        'LightsOutBox
        '
        Me.LightsOutBox.AutoSize = True
        Me.LightsOutBox.Location = New System.Drawing.Point(14, 184)
        Me.LightsOutBox.Name = "LightsOutBox"
        Me.LightsOutBox.Size = New System.Drawing.Size(131, 17)
        Me.LightsOutBox.TabIndex = 21
        Me.LightsOutBox.Text = "Lights out after photos"
        Me.LightsOutBox.UseVisualStyleBackColor = True
        '
        'mCHbox
        '
        Me.mCHbox.FormattingEnabled = True
        Me.mCHbox.Location = New System.Drawing.Point(128, 150)
        Me.mCHbox.Name = "mCHbox"
        Me.mCHbox.Size = New System.Drawing.Size(166, 21)
        Me.mCHbox.TabIndex = 19
        Me.mCHbox.Text = "Waiting for values"
        '
        'CFPbox
        '
        Me.CFPbox.FormattingEnabled = True
        Me.CFPbox.Location = New System.Drawing.Point(128, 117)
        Me.CFPbox.Name = "CFPbox"
        Me.CFPbox.Size = New System.Drawing.Size(166, 21)
        Me.CFPbox.TabIndex = 19
        Me.CFPbox.Text = "Waiting for values"
        '
        'BFBox
        '
        Me.BFBox.Location = New System.Drawing.Point(128, 18)
        Me.BFBox.Name = "BFBox"
        Me.BFBox.Size = New System.Drawing.Size(166, 21)
        Me.BFBox.TabIndex = 16
        Me.BFBox.Text = "Waiting for values"
        '
        'GFPbox
        '
        Me.GFPbox.FormattingEnabled = True
        Me.GFPbox.Location = New System.Drawing.Point(128, 84)
        Me.GFPbox.Name = "GFPbox"
        Me.GFPbox.Size = New System.Drawing.Size(166, 21)
        Me.GFPbox.TabIndex = 19
        Me.GFPbox.Text = "Waiting for values"
        '
        'BLbox
        '
        Me.BLbox.FormattingEnabled = True
        Me.BLbox.Location = New System.Drawing.Point(128, 51)
        Me.BLbox.Name = "BLbox"
        Me.BLbox.Size = New System.Drawing.Size(166, 21)
        Me.BLbox.TabIndex = 19
        Me.BLbox.Text = "Waiting for values"
        '
        'Backlight
        '
        Me.Backlight.AutoSize = True
        Me.Backlight.Location = New System.Drawing.Point(40, 53)
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
        Me.ExposureLab.Location = New System.Drawing.Point(106, 3)
        Me.ExposureLab.Name = "ExposureLab"
        Me.ExposureLab.Size = New System.Drawing.Size(54, 13)
        Me.ExposureLab.TabIndex = 4
        Me.ExposureLab.Text = "Exposure:"
        '
        'mCherry
        '
        Me.mCherry.AutoSize = True
        Me.mCherry.Location = New System.Drawing.Point(40, 152)
        Me.mCherry.Name = "mCherry"
        Me.mCherry.Size = New System.Drawing.Size(64, 17)
        Me.mCherry.TabIndex = 3
        Me.mCherry.Text = "mCherry"
        Me.mCherry.UseVisualStyleBackColor = True
        '
        'CFP
        '
        Me.CFP.AutoSize = True
        Me.CFP.Location = New System.Drawing.Point(40, 119)
        Me.CFP.Name = "CFP"
        Me.CFP.Size = New System.Drawing.Size(46, 17)
        Me.CFP.TabIndex = 2
        Me.CFP.Text = "CFP"
        Me.CFP.UseVisualStyleBackColor = True
        '
        'GFP
        '
        Me.GFP.AutoSize = True
        Me.GFP.Location = New System.Drawing.Point(40, 86)
        Me.GFP.Name = "GFP"
        Me.GFP.Size = New System.Drawing.Size(47, 17)
        Me.GFP.TabIndex = 1
        Me.GFP.Text = "GFP"
        Me.GFP.UseVisualStyleBackColor = True
        '
        'Brightfield
        '
        Me.Brightfield.AutoSize = True
        Me.Brightfield.Location = New System.Drawing.Point(40, 20)
        Me.Brightfield.Name = "Brightfield"
        Me.Brightfield.Size = New System.Drawing.Size(72, 17)
        Me.Brightfield.TabIndex = 0
        Me.Brightfield.Text = "Brightfield"
        Me.Brightfield.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        Me.Brightfield.UseVisualStyleBackColor = True
        '
        'ManualControls
        '
        Me.ManualControls.Controls.Add(Me.BackLightButton)
        Me.ManualControls.Controls.Add(Me.Label6)
        Me.ManualControls.Controls.Add(Me.ManTakePicture)
        Me.ManualControls.Controls.Add(Me.Label4)
        Me.ManualControls.Controls.Add(Me.mChFilter)
        Me.ManualControls.Controls.Add(Me.CFPfilter)
        Me.ManualControls.Controls.Add(Me.GFPfilter)
        Me.ManualControls.Controls.Add(Me.NoFilter)
        Me.ManualControls.Controls.Add(Me.mChrLight)
        Me.ManualControls.Controls.Add(Me.CFPlight)
        Me.ManualControls.Controls.Add(Me.GFPlight)
        Me.ManualControls.Controls.Add(Me.WhiteLight)
        Me.ManualControls.Controls.Add(Me.TvCmb)
        Me.ManualControls.Location = New System.Drawing.Point(4, 22)
        Me.ManualControls.Name = "ManualControls"
        Me.ManualControls.Size = New System.Drawing.Size(372, 278)
        Me.ManualControls.TabIndex = 2
        Me.ManualControls.Text = "Manual Controls"
        Me.ManualControls.UseVisualStyleBackColor = True
        '
        'BackLightButton
        '
        Me.BackLightButton.Location = New System.Drawing.Point(43, 50)
        Me.BackLightButton.Name = "BackLightButton"
        Me.BackLightButton.Size = New System.Drawing.Size(96, 22)
        Me.BackLightButton.TabIndex = 19
        Me.BackLightButton.Text = "Back Light"
        Me.BackLightButton.UseVisualStyleBackColor = True
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
        Me.ManTakePicture.Location = New System.Drawing.Point(176, 188)
        Me.ManTakePicture.Name = "ManTakePicture"
        Me.ManTakePicture.Size = New System.Drawing.Size(117, 53)
        Me.ManTakePicture.TabIndex = 17
        Me.ManTakePicture.Text = "Take Picture"
        Me.ManTakePicture.UseVisualStyleBackColor = True
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
        Me.CFPlight.Location = New System.Drawing.Point(43, 112)
        Me.CFPlight.Name = "CFPlight"
        Me.CFPlight.Size = New System.Drawing.Size(96, 22)
        Me.CFPlight.TabIndex = 2
        Me.CFPlight.Text = "CFP Light"
        Me.CFPlight.UseVisualStyleBackColor = True
        '
        'GFPlight
        '
        Me.GFPlight.Location = New System.Drawing.Point(43, 81)
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
        Me.TimeLapseControls.Size = New System.Drawing.Size(372, 278)
        Me.TimeLapseControls.TabIndex = 1
        Me.TimeLapseControls.Text = "Time Lapse Controls"
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
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.RobotOffAfterImaging)
        Me.TabPage1.Controls.Add(Me.EmailTextBox)
        Me.TabPage1.Controls.Add(Me.Reset)
        Me.TabPage1.Controls.Add(Me.StackFourFull)
        Me.TabPage1.Controls.Add(Me.StackThreeFull)
        Me.TabPage1.Controls.Add(Me.StackTwoFull)
        Me.TabPage1.Controls.Add(Me.StackOneFull)
        Me.TabPage1.Controls.Add(Me.CheckFullStacksLabel)
        Me.TabPage1.Controls.Add(Me.PlateHeight)
        Me.TabPage1.Controls.Add(Me.Label12)
        Me.TabPage1.Controls.Add(Me.CloseGripper)
        Me.TabPage1.Controls.Add(Me.OpenGripper)
        Me.TabPage1.Controls.Add(Me.StartRobotButton)
        Me.TabPage1.Controls.Add(Me.Label11)
        Me.TabPage1.Controls.Add(Me.RobotOff)
        Me.TabPage1.Controls.Add(Me.RobotOn)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(372, 278)
        Me.TabPage1.TabIndex = 3
        Me.TabPage1.Text = "Robot Controls"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'EmailTextBox
        '
        Me.EmailTextBox.Location = New System.Drawing.Point(195, 30)
        Me.EmailTextBox.Name = "EmailTextBox"
        Me.EmailTextBox.Size = New System.Drawing.Size(157, 20)
        Me.EmailTextBox.TabIndex = 15
        '
        'Reset
        '
        Me.Reset.Location = New System.Drawing.Point(287, 70)
        Me.Reset.Name = "Reset"
        Me.Reset.Size = New System.Drawing.Size(65, 25)
        Me.Reset.TabIndex = 14
        Me.Reset.Text = "Test stuff"
        Me.Reset.UseVisualStyleBackColor = True
        '
        'StackFourFull
        '
        Me.StackFourFull.AutoSize = True
        Me.StackFourFull.Location = New System.Drawing.Point(22, 162)
        Me.StackFourFull.Name = "StackFourFull"
        Me.StackFourFull.Size = New System.Drawing.Size(63, 17)
        Me.StackFourFull.TabIndex = 13
        Me.StackFourFull.Text = "Stack 4"
        Me.StackFourFull.UseVisualStyleBackColor = True
        '
        'StackThreeFull
        '
        Me.StackThreeFull.AutoSize = True
        Me.StackThreeFull.Location = New System.Drawing.Point(22, 139)
        Me.StackThreeFull.Name = "StackThreeFull"
        Me.StackThreeFull.Size = New System.Drawing.Size(63, 17)
        Me.StackThreeFull.TabIndex = 12
        Me.StackThreeFull.Text = "Stack 3"
        Me.StackThreeFull.UseVisualStyleBackColor = True
        '
        'StackTwoFull
        '
        Me.StackTwoFull.AutoSize = True
        Me.StackTwoFull.Location = New System.Drawing.Point(22, 116)
        Me.StackTwoFull.Name = "StackTwoFull"
        Me.StackTwoFull.Size = New System.Drawing.Size(63, 17)
        Me.StackTwoFull.TabIndex = 11
        Me.StackTwoFull.Text = "Stack 2"
        Me.StackTwoFull.UseVisualStyleBackColor = True
        '
        'StackOneFull
        '
        Me.StackOneFull.AutoSize = True
        Me.StackOneFull.Location = New System.Drawing.Point(22, 93)
        Me.StackOneFull.Name = "StackOneFull"
        Me.StackOneFull.Size = New System.Drawing.Size(63, 17)
        Me.StackOneFull.TabIndex = 10
        Me.StackOneFull.Text = "Stack 1"
        Me.StackOneFull.UseVisualStyleBackColor = True
        '
        'CheckFullStacksLabel
        '
        Me.CheckFullStacksLabel.AutoSize = True
        Me.CheckFullStacksLabel.Location = New System.Drawing.Point(19, 61)
        Me.CheckFullStacksLabel.Name = "CheckFullStacksLabel"
        Me.CheckFullStacksLabel.Size = New System.Drawing.Size(169, 26)
        Me.CheckFullStacksLabel.TabIndex = 9
        Me.CheckFullStacksLabel.Text = "Check all stacks with plates:" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "(At least one stack must be empty)" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'PlateHeight
        '
        Me.PlateHeight.Location = New System.Drawing.Point(22, 209)
        Me.PlateHeight.Name = "PlateHeight"
        Me.PlateHeight.Size = New System.Drawing.Size(120, 20)
        Me.PlateHeight.TabIndex = 8
        Me.PlateHeight.Value = New Decimal(New Integer() {16, 0, 0, 0})
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(19, 193)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(91, 13)
        Me.Label12.TabIndex = 6
        Me.Label12.Text = "Plate height (mm):"
        '
        'CloseGripper
        '
        Me.CloseGripper.Location = New System.Drawing.Point(254, 157)
        Me.CloseGripper.Name = "CloseGripper"
        Me.CloseGripper.Size = New System.Drawing.Size(98, 26)
        Me.CloseGripper.TabIndex = 5
        Me.CloseGripper.Text = "Close Gripper"
        Me.CloseGripper.UseVisualStyleBackColor = True
        '
        'OpenGripper
        '
        Me.OpenGripper.Location = New System.Drawing.Point(254, 114)
        Me.OpenGripper.Name = "OpenGripper"
        Me.OpenGripper.Size = New System.Drawing.Size(98, 26)
        Me.OpenGripper.TabIndex = 4
        Me.OpenGripper.Text = "Open Gripper"
        Me.OpenGripper.UseVisualStyleBackColor = True
        '
        'StartRobotButton
        '
        Me.StartRobotButton.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.StartRobotButton.Location = New System.Drawing.Point(254, 200)
        Me.StartRobotButton.Name = "StartRobotButton"
        Me.StartRobotButton.Size = New System.Drawing.Size(98, 48)
        Me.StartRobotButton.TabIndex = 3
        Me.StartRobotButton.Text = "Start Robot"
        Me.StartRobotButton.UseVisualStyleBackColor = True
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(19, 14)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(39, 13)
        Me.Label11.TabIndex = 2
        Me.Label11.Text = "Robot:"
        '
        'RobotOff
        '
        Me.RobotOff.AutoSize = True
        Me.RobotOff.Location = New System.Drawing.Point(22, 30)
        Me.RobotOff.Name = "RobotOff"
        Me.RobotOff.Size = New System.Drawing.Size(39, 17)
        Me.RobotOff.TabIndex = 1
        Me.RobotOff.Text = "Off"
        Me.RobotOff.UseVisualStyleBackColor = True
        '
        'RobotOn
        '
        Me.RobotOn.AutoSize = True
        Me.RobotOn.Location = New System.Drawing.Point(67, 30)
        Me.RobotOn.Name = "RobotOn"
        Me.RobotOn.Size = New System.Drawing.Size(39, 17)
        Me.RobotOn.TabIndex = 0
        Me.RobotOn.Text = "On"
        Me.RobotOn.UseVisualStyleBackColor = True
        '
        'FilePrefixLab
        '
        Me.FilePrefixLab.AutoSize = True
        Me.FilePrefixLab.Location = New System.Drawing.Point(11, 266)
        Me.FilePrefixLab.Name = "FilePrefixLab"
        Me.FilePrefixLab.Size = New System.Drawing.Size(53, 13)
        Me.FilePrefixLab.TabIndex = 39
        Me.FilePrefixLab.Text = "File suffix:"
        Me.FilePrefixLab.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'FilePrefix
        '
        Me.FilePrefix.Location = New System.Drawing.Point(70, 263)
        Me.FilePrefix.Name = "FilePrefix"
        Me.FilePrefix.Size = New System.Drawing.Size(184, 20)
        Me.FilePrefix.TabIndex = 40
        Me.FilePrefix.Text = "img"
        '
        'LiveViewLab
        '
        Me.LiveViewLab.AutoSize = True
        Me.LiveViewLab.Location = New System.Drawing.Point(422, 406)
        Me.LiveViewLab.Name = "LiveViewLab"
        Me.LiveViewLab.Size = New System.Drawing.Size(52, 13)
        Me.LiveViewLab.TabIndex = 42
        Me.LiveViewLab.Text = "Live view"
        '
        'LiveViewOn
        '
        Me.LiveViewOn.AutoSize = True
        Me.LiveViewOn.Location = New System.Drawing.Point(480, 404)
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
        Me.LiveViewOff.Location = New System.Drawing.Point(525, 404)
        Me.LiveViewOff.Name = "LiveViewOff"
        Me.LiveViewOff.Size = New System.Drawing.Size(39, 17)
        Me.LiveViewOff.TabIndex = 44
        Me.LiveViewOff.TabStop = True
        Me.LiveViewOff.Text = "Off"
        Me.LiveViewOff.UseVisualStyleBackColor = True
        '
        'LightsOffButton
        '
        Me.LightsOffButton.Location = New System.Drawing.Point(390, 322)
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
        Me.DiscoButton.Location = New System.Drawing.Point(485, 322)
        Me.DiscoButton.Name = "DiscoButton"
        Me.DiscoButton.Size = New System.Drawing.Size(83, 24)
        Me.DiscoButton.TabIndex = 45
        Me.DiscoButton.Text = "DISCO"
        Me.DiscoButton.UseVisualStyleBackColor = True
        '
        'OuterWhiteLight
        '
        Me.OuterWhiteLight.Location = New System.Drawing.Point(275, 322)
        Me.OuterWhiteLight.Name = "OuterWhiteLight"
        Me.OuterWhiteLight.Size = New System.Drawing.Size(80, 24)
        Me.OuterWhiteLight.TabIndex = 46
        Me.OuterWhiteLight.Text = "White Light"
        Me.OuterWhiteLight.UseVisualStyleBackColor = True
        '
        'PickDirectory
        '
        Me.PickDirectory.Location = New System.Drawing.Point(9, 237)
        Me.PickDirectory.Name = "PickDirectory"
        Me.PickDirectory.Size = New System.Drawing.Size(131, 23)
        Me.PickDirectory.TabIndex = 47
        Me.PickDirectory.Text = "Pick Directory"
        Me.PickDirectory.UseVisualStyleBackColor = True
        '
        'OpenDirectory
        '
        Me.OpenDirectory.Location = New System.Drawing.Point(149, 237)
        Me.OpenDirectory.Name = "OpenDirectory"
        Me.OpenDirectory.Size = New System.Drawing.Size(105, 23)
        Me.OpenDirectory.TabIndex = 48
        Me.OpenDirectory.Text = "Open Save Directory"
        Me.OpenDirectory.UseVisualStyleBackColor = True
        '
        'LastImageBox
        '
        Me.LastImageBox.Location = New System.Drawing.Point(674, 76)
        Me.LastImageBox.Name = "LastImageBox"
        Me.LastImageBox.Size = New System.Drawing.Size(233, 188)
        Me.LastImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.LastImageBox.TabIndex = 49
        Me.LastImageBox.TabStop = False
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(11, 18)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(87, 13)
        Me.Label9.TabIndex = 20
        Me.Label9.Text = "Camera Settings:"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(671, 34)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(96, 13)
        Me.Label10.TabIndex = 50
        Me.Label10.Text = "Last Image Taken:"
        '
        'CameraRelease
        '
        Me.CameraRelease.Location = New System.Drawing.Point(149, 130)
        Me.CameraRelease.Name = "CameraRelease"
        Me.CameraRelease.Size = New System.Drawing.Size(98, 23)
        Me.CameraRelease.TabIndex = 51
        Me.CameraRelease.Text = "Release Camera"
        Me.CameraRelease.UseVisualStyleBackColor = True
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        Me.Timer1.Interval = 600000
        '
        'RobotOffAfterImaging
        '
        Me.RobotOffAfterImaging.AutoSize = True
        Me.RobotOffAfterImaging.Location = New System.Drawing.Point(22, 245)
        Me.RobotOffAfterImaging.Name = "RobotOffAfterImaging"
        Me.RobotOffAfterImaging.Size = New System.Drawing.Size(153, 17)
        Me.RobotOffAfterImaging.TabIndex = 16
        Me.RobotOffAfterImaging.Text = "Turn robot off after imaging"
        Me.RobotOffAfterImaging.UseVisualStyleBackColor = True
        '
        'VBSample
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ClientSize = New System.Drawing.Size(919, 362)
        Me.Controls.Add(Me.CameraRelease)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.LastImageBox)
        Me.Controls.Add(Me.OpenDirectory)
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
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "VBSample"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Text = "Roboscope (no robot yet)"
        Me.ControlTabs.ResumeLayout(False)
        Me.ImagingControls.ResumeLayout(False)
        Me.ImagingControls.PerformLayout()
        Me.ManualControls.ResumeLayout(False)
        Me.ManualControls.PerformLayout()
        Me.TimeLapseControls.ResumeLayout(False)
        Me.TimeLapseControls.PerformLayout()
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        CType(Me.PlateHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LastImageBox, System.ComponentModel.ISupportInitialize).EndInit()
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
                LastImageBox.ImageLocation = savepath
                Debug.Print("Displaying image: " & savepath)
                LastImageBox.Update()

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
                TvCmb.Text = propList.Item(data)
                If BFBox.Text = "Waiting for values" Then
                    BFBox.Text = propList.Item(data)
                    BLbox.Text = propList.Item(data)
                    GFPbox.Text = propList.Item(data)
                    CFPbox.Text = propList.Item(data)
                    mCHbox.Text = propList.Item(data)
                End If

            Case kEdsPropID_ExposureCompensation
                'ExposureCompCmb.Text = propList.Item(data)
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
                    BFBox.Items.Add(propStr)
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
            BFBox.Tag = propValueList
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
        If RobotOn.Checked Then
            GX.ShutDown()
        End If
        Me.Close()

        End
    End Sub

#End Region

    ' Modifications: none. TO_DO: changes regarding connection to HID and go to filter, and lights off.
    Private Sub VBSample_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConnectToHID(Me)
        GX = New GXRobotControlNamespace.GXRobotControl
        GX.SetDefaultServoGripperOpenPosition(6)
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
            TakeColorPicture(1, no_filter, BFBox, path & "_BF.jpg")
        End If

        If Brightfield.Checked Then
            Debug.WriteLine("BF Called")
            TakeColorPicture(white_light, no_filter, BFBox, path & "_BF.jpg")
            'Console.WriteLine(DateTime.Now)
            'Console.WriteLine("brightfield")
            'Thread.Sleep(1500)
            'Console.WriteLine(DateTime.Now)
        End If

        If Backlight.Checked Then

            Debug.WriteLine("BL Called")
            TakeColorPicture(back_light, no_filter, BLbox, path & "_BL.jpg")
            'Console.WriteLine("Backlight")
            'Thread.Sleep(1500)
        End If

        If GFP.Checked Then
            Debug.WriteLine("GFP Called")
            TakeColorPicture(GFP_light, GFP_filter, GFPbox, path & "_GFP.jpg")
            'Console.WriteLine("GFP")
            'Thread.Sleep(1500)
        End If

        If CFP.Checked Then
            Debug.WriteLine("CFP Called")
            TakeColorPicture(CFP_light, CFP_filter, CFPbox, path & "_CFP.jpg")
            'Thread.Sleep(1500)
        End If

        If mCherry.Checked Then
            Debug.WriteLine("RFP Called")
            TakeColorPicture(mCherry_light, mCherry_filter, mCHbox, path & "_mCher.jpg")
        End If

        Goto_Filter(no_filter)
        If LightsOutBox.Checked Then
            Debug.WriteLine("Lights off")
            lights.all_off()
        Else
            Debug.WriteLine("White Light")
            lights.pick_light(white_light)
        End If

    End Sub

    Private Sub timeoutsavequeue()
        If (DateAndTime.DateDiff(DateInterval.Second, lastpicturesend, DateAndTime.Now) > 10) Then
            savequeue.Clear()
            'MsgBox("Queue cleared!")
            Debug.WriteLine("SaveQueue timed out")
        End If
    End Sub

    Private Sub SaveButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveButton.Click
        Dim myStream As StreamWriter = Nothing
        Dim saveFileDialog1 As New SaveFileDialog()
        Dim streamname As String

        saveFileDialog1.InitialDirectory = TextBoxSavePath.Text
        saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
        saveFileDialog1.FilterIndex = 2
        saveFileDialog1.RestoreDirectory = True

        If saveFileDialog1.ShowDialog() = DialogResult.OK Then
            streamname = saveFileDialog1.FileName()
            myStream = New StreamWriter(streamname)

            If (myStream IsNot Nothing) Then
                If AvCmb.SelectedIndex = -1 Then 'save as default maximum aperture unless set otherwise
                    myStream.WriteLine("Av " & "0")
                Else
                    myStream.WriteLine("Av " & AvCmb.SelectedIndex)
                End If

                If ISOSpeedCmb.SelectedIndex = -1 Then 'save as default maximum ISO unless set otherwise
                    myStream.WriteLine("ISO " & "1")
                Else
                    myStream.WriteLine("ISO " & ISOSpeedCmb.SelectedIndex)
                End If

                If ImageQualityCmb.SelectedIndex = -1 Then 'save as default maximum quality unless set otherwise
                    myStream.WriteLine("IQ " & "0")
                Else
                    myStream.WriteLine("IQ " & ImageQualityCmb.SelectedIndex)
                End If

                myStream.WriteLine("Dir " & TextBoxSavePath.Text)
                myStream.WriteLine("Prefix " & FilePrefix.Text)

                If Brightfield.Checked Then
                    myStream.WriteLine("BF " & BFBox.SelectedIndex)
                Else
                    myStream.WriteLine("BF " & "-1")
                End If


                If Backlight.Checked Then
                    myStream.WriteLine("BL " & BLbox.SelectedIndex)
                Else
                    myStream.WriteLine("BL " & "-1")
                End If

                If GFP.Checked Then
                    myStream.WriteLine("GFP " & GFPbox.SelectedIndex)
                Else
                    myStream.WriteLine("GFP " & "-1")
                End If

                If CFP.Checked Then
                    myStream.WriteLine("CFP " & CFPbox.SelectedIndex)
                Else
                    myStream.WriteLine("CFP " & "-1")
                End If

                If mCherry.Checked Then
                    myStream.WriteLine("mCherry " & mCHbox.SelectedIndex)
                Else
                    myStream.WriteLine("mCherry " & "-1")
                End If




                myStream.Close()
            End If
        End If


    End Sub

    Private Sub LoadButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LoadButton.Click
        Dim myStream As StreamReader = Nothing
        Dim openFileDialog1 As New OpenFileDialog()

        openFileDialog1.InitialDirectory = TextBoxSavePath.Text
        openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
        openFileDialog1.FilterIndex = 2
        openFileDialog1.RestoreDirectory = True

        If openFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Try
                myStream = New StreamReader(openFileDialog1.FileName)
                If (myStream IsNot Nothing) Then
                    ' Insert code to read the stream here.
                    Dim lineparse As String()
                    Dim data As Integer
                    Dim propValueList As ArrayList
                    ' Read and display the lines from the file until the end 
                    ' of the file is reached.

                    'Read the Av
                    lineparse = Split(myStream.ReadLine())
                    propValueList = CType(AvCmb.Tag, ArrayList)
                    data = propValueList.Item(lineparse(1))
                    controller.actionPerformed("set", kEdsPropID_Av, data)

                    'read the Iso
                    lineparse = Split(myStream.ReadLine())
                    propValueList = CType(ISOSpeedCmb.Tag, ArrayList)
                    data = propValueList.Item(lineparse(1))
                    controller.actionPerformed("set", kEdsPropID_ISOSpeed, data)

                    'read the Image Quality
                    lineparse = Split(myStream.ReadLine())
                    propValueList = CType(ImageQualityCmb.Tag, ArrayList)
                    data = propValueList.Item(lineparse(1))
                    controller.actionPerformed("set", kEdsPropID_ImageQuality, data)

                    'read the Directory
                    Dim dirname As String
                    lineparse = Split(myStream.ReadLine())
                    If lineparse.Length > 1 Then
                        dirname = lineparse(1)
                        For i As Integer = 2 To (lineparse.Length - 1)
                            dirname = dirname + " " + lineparse(i)
                        Next
                        TextBoxSavePath.Text = dirname
                    End If

                    'read the Prefix if there is one
                    lineparse = Split(myStream.ReadLine())
                    If lineparse.Length > 1 Then
                        FilePrefix.Text = lineparse(1)
                    End If


                    'read the BF setting
                    lineparse = Split(myStream.ReadLine())
                    If lineparse(1) > -1 Then
                        Debug.Print("BF " + lineparse(1))
                        Brightfield.Checked = True
                        BFBox.SelectedIndex = lineparse(1)
                    Else
                        Brightfield.Checked = False
                    End If

                    'read the BL setting
                    lineparse = Split(myStream.ReadLine())
                    If lineparse(1) > -1 Then
                        Backlight.Checked = True
                        BLbox.SelectedIndex = lineparse(1)
                    Else
                        Backlight.Checked = False
                    End If


                    'read the GFP setting
                    lineparse = Split(myStream.ReadLine())
                    If lineparse(1) > -1 Then
                        GFP.Checked = True
                        GFPbox.SelectedIndex = lineparse(1)
                    Else
                        GFP.Checked = False
                    End If

                    'read the CFP setting
                    lineparse = Split(myStream.ReadLine())
                    If lineparse(1) > -1 Then
                        CFP.Checked = True
                        CFPbox.SelectedIndex = lineparse(1)
                    Else
                        CFP.Checked = False
                    End If


                    'read the BF setting
                    lineparse = Split(myStream.ReadLine())
                    If lineparse(1) > -1 Then
                        mCherry.Checked = True
                        mCHbox.SelectedIndex = lineparse(1)
                    Else
                        mCherry.Checked = False
                    End If




                End If
            Catch Ex As Exception
                MessageBox.Show("Cannot read file from disk. Original error: " & Ex.Message)
            Finally
                ' Check this again, since we need to make sure we didn't throw an exception on open.
                If (myStream IsNot Nothing) Then
                    myStream.Close()
                End If
            End Try
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

            If timeoutcount = 200 Then
                'While BufferIn(1) <> 3
                'Thread.Sleep(50)
                'End While
                Debug.Print("Wheel Timeout!")

            End If
        End If
        If lightnum <> 1 Then
            lights.pick_light(lightnum)
        End If

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

        Thread.Sleep(100)
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

    'Private LV As Thread
    'Private Sub Timer1_Tick(sender As Object, e As EventArgs)
    '    LV = New Thread(AddressOf take_automatic_pictures)
    '    LV.Start()

    '    'Console.WriteLine("timer")
    'End Sub





    Private Sub ManTakePicture_Click(sender As Object, e As EventArgs) Handles ManTakePicture.Click
        Dim propValueList As ArrayList = CType(TvCmb.Tag, ArrayList)
        Dim data As Integer
        Dim exposurelengthfrommenu As Integer = TvCmb.SelectedIndex

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
        savepath = path & nameprefix & "_" & FilePrefix.Text & ".jpeg"

        TakePictureWithName(path)
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

    Private Sub OpenDirectory_Click(sender As Object, e As EventArgs) Handles OpenDirectory.Click
        Shell("C:\WINDOWS\explorer.exe """ & TextBoxSavePath.Text & "", vbNormalFocus)
    End Sub

    Private Sub CameraRelease_Click(sender As Object, e As EventArgs) Handles CameraRelease.Click
        controller.actionPerformed("close")

        If IsNothing(model) Then
            If IsNothing(model.getCameraObject()) = False Then
                EdsRelease(model.getCameraObject())
            End If
        End If

    End Sub

    ' Natalia tests


    ' Michael

#End Region

#Region "lights and filters"

    Private Sub GFPlight_Click(sender As Object, e As EventArgs) Handles GFPlight.Click
        lights.pick_light(GFP_light)
    End Sub

    Private Sub WhiteLight_Click(sender As Object, e As EventArgs) Handles WhiteLight.Click
        lights.pick_light(white_light)
    End Sub

    Private Sub BackLightButton_Click(sender As Object, e As EventArgs) Handles BackLightButton.Click
        lights.pick_light(back_light)
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

    Public Sub white_lights()
        lights.pick_light(white_light)
    End Sub

    Private Sub LightsOutBox_CheckedChanged(sender As Object, e As EventArgs) Handles LightsOutBox.CheckedChanged

    End Sub

    Private Sub YFPlight_Click(sender As Object, e As EventArgs)
        lights.pick_light(YFP_light)
    End Sub

    Private Sub YFPfilter_Click(sender As Object, e As EventArgs)
        Goto_Filter(YFP_filter)
    End Sub

    Private Sub OuterWhiteLight_Click(sender As Object, e As EventArgs) Handles OuterWhiteLight.Click
        lights.pick_light(white_light)
    End Sub
#End Region

#Region "Disco"
    Private discofo As DiscoForm = New DiscoForm
    Public Sub discodisco()
        If RobotOn.Checked = False Then
            GX.Initialize()
            RobotOn.Checked = True
        End If

        Do
            'lights.disco(544)
            lights.disco(294)

            RobotDisco()
        Loop
        'GX.ShutDown()
    End Sub

    Public Sub RobotDisco()
        GX.SetOutput(2, 4, 0)

        GX.SetOutput(2, 2, 1)
        GX.TeachPointMoveTo("Stack1Approach", 10, 10, True)
        Threading.Thread.Sleep(200)
        GX.SetOutput(2, 2, 0)

        GX.SetOutput(2, 3, 1)
        GX.TeachPointMoveTo("Stack2Approach", 10, 10, True)
        Threading.Thread.Sleep(200)
        GX.SetOutput(2, 3, 0)

        GX.SetOutput(2, 1, 1)
        GX.TeachPointMoveTo("Stack4Approach", 10, 10, True)
        Threading.Thread.Sleep(200)
        GX.SetOutput(2, 1, 0)

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
#End Region

#Region "Robot "
    Private Sub RobotOn_CheckedChanged(sender As Object, e As EventArgs) Handles RobotOn.CheckedChanged
        GX.Initialize()
        'GX.TeachPointMoveTo(14, 10, 10, True)
    End Sub

    Private Sub RobotOff_CheckedChanged(sender As Object, e As EventArgs) Handles RobotOff.CheckedChanged
        GX.ShutDown()
    End Sub

    Public Sub MoveToStackTop(stackNum As String)
        'Dim stackApproach As String = "Stack" & stackNum & "Approach"
        GX.TeachPointMoveTo(stackNum, 20, 10, True)
    End Sub

    Function PickUpPlateStack(stackTop As String, stackBottom As String) As Short
        MoveToStackTop(stackTop)
        Dim test As Short
        Dim measuredPlateNum As Short
        test = GX.RemovePlateFromStack(stackTop, stackBottom, 30, 1, 0, 20, True, -7, measuredPlateNum, 1000, False) ', SearchVelocity:=15)
        If test <> 0 Then
            Dim retry As MsgBoxResult
            retry = MsgBox("An error removing the plate has occured", 5, "Gripper error")
            If retry = 4 Then
                Dim test2 As Short
                GX.Initialize()
                test2 = GX.RemovePlateFromStack(stackTop, stackBottom, 30, 1, 0, 20, True, -7, measuredPlateNum, 1000, False)
                If test2 <> 0 Then
                    MsgBox("The error has not been resolved", 0, "Gripper error")
                End If
            End If
        End If
        Console.WriteLine("measured plate number in working stack is " & measuredPlateNum)
        Console.WriteLine("error code is " & test)
        Return measuredPlateNum
        'GX.ServoGripperOpen(10, True)
        'test = GX.FindObject(stackTop, 10, -400, -10)
        'Console.WriteLine("pick up plate error " & test)
        'GX.ServoGripperClose(10)
    End Function

    Public Sub MoveToStage()
        'Make sure that robot is not positioned in a stack first
        'GX.TeachPointMoveTo("StackStageMidPoint", 10, 10, True)
        GX.TeachPointMoveTo("StageApproach", 20, 20, True)
        GX.TeachPointMoveTo("StageBottom", 20, 20, True)

    End Sub

    Public Sub DropPlate()
        GX.ServoGripperOpen(10, True)
    End Sub

    Public Sub RemoveLid()
        GX.TeachPointMoveTo("StageBottomLid", 20, 10, True)
        GX.ServoGripperSetDefaultGrippingForce(50)
        GX.ServoGripperClose(10)
        'GX.TeachPointMoveTo("StageApproach", 10, 10, True)
        'GX.TeachPointMoveTo("StackStageMidPoint", 10, 10, True)

        GX.MoveRelativeSingleAxis(2, PlateHeight.Value * 3 / 4, 20, 10, True)
        Dim test As Short
        test = GX.MoveRelativeCartesian({150, 0, 0, 0}, 20, 10, True)
        Console.WriteLine("move relative cartesian error " & test)
    End Sub

    Public Sub ReplaceLid()
        'GX.TeachPointMoveTo("StageApproach", 10, 10, True)
        'GX.TeachPointMoveTo("StageBottomLid", 10, 10, True)
        Dim test As Short
        test = GX.MoveRelativeCartesian({-150, 0, 0, 0}, 20, 10, True)
        Console.WriteLine("move relative cartesian error " & test)
        GX.MoveRelativeSingleAxis(2, -(PlateHeight.Value * 3 / 4), 20, 10, True)
        GX.ServoGripperSetDefaultGrippingForce(80)
    End Sub

    Public Sub PickUpPlateStage()
        GX.ServoGripperOpen(10, True)
        GX.TeachPointMoveTo("StageBottom", 20, 10, True)
        GX.ServoGripperClose(10)
        GX.TeachPointMoveTo("StageApproach", 20, 10, True)
        'GX.TeachPointMoveTo("StackStageMidPoint", 10, 10, True)
    End Sub

    Public plateNum As Short = 1
    Public gripOffset As Integer = 0
    Public Sub PlacePlateStack(stackTop As String, stackBottom As String)
        MoveToStackTop(stackTop)
        Dim test As Short
        test = GX.PlacePlateInPitchStack(stackTop, stackBottom, plateNum, 0, gripOffset, PlateHeight.Value, 10, 20, 10, 3)
        'Console.WriteLine("place plate error " & test)
        'GX.PlacePlateInPitchStackResume(GX.GetErrorCode(2), stackTop, stackBottom, plateNum, 0, gripOffset, PlateHeight.Value, 10, 10, 10, 3)
        If test = 3 Then
            GX.Initialize()
            'GX.PlacePlateInPitchStackResume(GX.GetErrorCode(2), stackTop, stackBottom, plateNum, 0, gripOffset, PlateHeight.Value, 10, 10, 10, 3)
            GX.ServoGripperOpen(10,True)
            GX.RemovePlateFromStack(stackTop, stackBottom, 30, 1, 0, 10, True, -7, plateNum, 0, True, False)
            Exit Sub
        End If
        'Console.WriteLine(GX.GetErrorCode(3))
        Console.WriteLine(test)
        plateNum = CShort(plateNum + 1)
        Console.WriteLine("Plate number is: " & plateNum)
        'Console.WriteLine(PlateHeight.Value)
        'Console.WriteLine(emptyStack)
    End Sub

    Public Sub RobotTest()
        'Console.WriteLine("Stack" & emptyStack & "Approach" & " , " & "Stack" & workingStack & "Approach")

        'GX.TeachPointMoveTo("StageApproach", 10, 10, True)
        'Dim test As Short
        'test = GX.RemovePlateFromStack("StageApproach", "StageBottom", 10, 1, 0, 10, True, 0, 1, 0, True, False, MoveToCalculatedHeightAfterSearch:=False)
        'Console.WriteLine("error finding stage " & test)
        'Try
        '    GX.TeachPointMoveTo("StageApproach", 10, 10, True)
        '    Dim test As Short
        '    test = GX.FindObject("StageApproach", 10, -150, 0)
        '    Console.WriteLine("find object error " & test)
        'Catch test As Exception
        '    Console.WriteLine("Here")
        '    GX.TeachPointSetValue(26, Nothing)
        '    GX.TeachPointSetName(26, "Stage")
        '    GX.TeachPointsSave()
        '    GX.TeachPointMoveTo("StageApproach", 10, 10, True)
        '    GX.TeachPointMoveTo("Stage", 10, 10, True)
        'End Try

        'GX.ServoGripperOpen(10, True)
        'GX.TeachPointMoveTo("StageApproach", 10, 10, True)
        'Dim test As Short
        'test = GX.FindObject("StageApproach", 5, -150, 0)
        'Console.WriteLine("find object error " & test)
        'Console.WriteLine("Here")
        'GX.Initialize()
        'Dim shoulder As Double
        'GX.MotorGetCurrentPosition(1, shoulder)
        'Dim z As Double
        'GX.MotorGetCurrentPosition(2, z)
        'Dim elbow As Double
        'GX.MotorGetCurrentPosition(3, elbow)
        'Dim wrist As Double
        'GX.MotorGetCurrentPosition(4, wrist)



        'GX.TeachPointSetValue(26, {shoulder, z, elbow, wrist})
        'GX.TeachPointSetName(26, "Stage")
        'GX.TeachPointsSave()
        ''GX.AutoTeachWindowShow(True)
        'GX.TeachPointMoveTo("StageApproach", 10, 10, True)
        'GX.TeachPointMoveTo("Stage", 5, 5, True)
        'Dim value As Double()
        'GX.TeachPointGetValue(26, value)
        'Console.WriteLine("stage teach point value")
        'Console.WriteLine(value(0) & ", " & value(1) & ", " & value(2) & ", " & value(3) & ", " & value(4))
        'Dim test1 As Short
        'test1 = GX.TeachPointMoveTo("Stage", 10, 10, True)
        'Console.WriteLine("error " & test1)
        GX.ServoGripperSetDefaultGrippingForce(80)

        Dim numFullStacks As Short = Math.Abs(stack1Full + stack2Full + stack3Full + stack4Full)
        Dim workingStack As String = ""
        Dim emptyStack As String = ""

        If numFullStacks = 4 Then
            MsgBox("At least one stack must be empty", 0, "Stack error")
            Exit Sub
            'Throw New System.Exception("At least one stack must be empty")
        ElseIf numFullStacks = 0 Then
            MsgBox("At least one stack must have plates", 0, "Stack error")
            Exit Sub
        End If

        For i As Integer = 1 To numFullStacks

            For j As Integer = 0 To 3
                Dim tmp As Integer = Math.Abs(j - 4)
                If stacks(tmp - 1) = 1 Then
                    workingStack = CType(tmp, String)
                ElseIf stacks(tmp - 1) = 0 Then
                    emptyStack = CType(tmp, String)
                End If
            Next

            Console.WriteLine("empty stack is " & emptyStack)
            Console.WriteLine("working stack is " & workingStack)

            Dim measuredPlateNum As Short = 10
            While (measuredPlateNum > 1)
                measuredPlateNum = PickUpPlateStack("Stack" & workingStack & "Approach", "Stack" & workingStack & "Bottom")
                'Console.WriteLine("measured plate num within while loop is " & measuredPlateNum)

                If measuredPlateNum = 0 Then
                    plateNum = 1
                    stacks(CType(emptyStack, Integer) - 1) = 0
                    stacks(CType(workingStack, Integer) - 1) = 0
                    Continue For
                End If

                MoveToStage()
                DropPlate()

                RemoveLid()
                take_automatic_pictures()

                ReplaceLid()
                PickUpPlateStage()

                PlacePlateStack("Stack" & emptyStack & "Approach", "Stack" & emptyStack & "Bottom")
            End While
            plateNum = 1
            stacks(CType(emptyStack, Integer) - 1) = 2
            stacks(CType(workingStack, Integer) - 1) = 0
        Next
        If EmailTextBox.Text <> "" Then
            Dim emailError As String
            emailError = GX.SendEmailMessage(EmailTextBox.Text, "Macroscope imaging finished", "The Baym Lab macroscope has finished imaging your plates.")
            Console.WriteLine("email error is " & emailError)
        End If

        If RobotOffAfterImaging.Checked Then
            RobotOff.Checked = True
            MsgBox("All plates have been imaged and the robot was shut down", 0, "Imaging finished")

        Else
            MsgBox("All plates have been imaged", 0, "Imaging finished")
        End If

    End Sub


    'Public emptyStack As String = ""
    'Public workingStack As String = ""
    'Public numFullStacks As Integer = 0

    Public stacks As Integer() = New Integer() {0, 0, 0, 0}
    Private Sub StartRobotBotton_Click(sender As Object, e As EventArgs) Handles StartRobotButton.Click
        'emptyStack = ""
        'workingStack = ""
        'numFullStacks = Math.Abs(stack1Full + stack2Full + stack3Full + stack4Full)
        'Console.WriteLine("number of full stacks " & numFullStacks)



        'If StackFourFull.Checked Then
        '    workingStack = "4"
        'Else
        '    emptyStack = "4"
        'End If

        'If StackThreeFull.Checked Then
        '    workingStack = "3"
        'Else
        '    emptyStack = "3"
        'End If

        'If StackTwoFull.Checked Then
        '    workingStack = "2"
        'Else
        '    emptyStack = "2"
        'End If

        'If StackOneFull.Checked Then
        '    workingStack = "1"
        'Else
        '    emptyStack = "1"
        'End If

        'Console.WriteLine("Working Stack " & workingStack)
        'Console.WriteLine("Empty Stack " & emptyStack)

        'stacks = New Integer() {0, 0, 0, 0}

        plateNum = 1

        If stack1Full Then
            stacks(0) = 1
        Else
            stacks(0) = 0
        End If

        If stack2Full Then
            stacks(1) = 1
        Else
            stacks(1) = 0
        End If

        If stack3Full Then
            stacks(2) = 1
        Else
            stacks(2) = 0
        End If

        If stack4Full Then
            stacks(3) = 1
        Else
            stacks(3) = 0
        End If

        Dim rt As Thread
        rt = New Thread(AddressOf RobotTest)
        rt.Start()

        'GX.ScriptEditorShow(True)

        'MoveToStackTop("1")

        'PickUpPlateStack("Stack1Approach", "Stack1Bottom")
        'MoveToStage()
        'DropPlate()
        ''GX.ScriptRun("C:\ProgramData\Peak Analysis & Automation\GX Robot Control DLL\Sequences\Lid", "Lid", True)

        'RemoveLid()

        ''CheckForIllegalCrossThreadCalls = False
        ''t = New Thread(AddressOf take_automatic_pictures)
        ''t.Start()

        'take_automatic_pictures()

        ''Threading.Thread.Sleep(5000)

        ''GX.DelayMsec(4000)

        'ReplaceLid()
        'PickUpPlateStage()

        ''MoveToStackTop("2")

        'PlacePlateStack("Stack2Approach", "Stack2Bottom")

        'GX.TeachPointMoveTo("Stack2Approach", 10, 10, True)

    End Sub

    Private Sub OpenGripper_Click(sender As Object, e As EventArgs) Handles OpenGripper.Click
        GX.ServoGripperOpen(10, True)
    End Sub

    Private Sub CloseGripper_Click(sender As Object, e As EventArgs) Handles CloseGripper.Click
        GX.ServoGripperClose(10)
    End Sub

    Private Sub Reset_Click(sender As Object, e As EventArgs) Handles Reset.Click
        Dim gripforce As Byte
        GX.ServoGripperQueryGrippingForce(gripforce)
        Console.WriteLine("grip force is " & gripforce)


        'Dim emailError As String
        'emailError = GX.SendEmailMessage(EmailTextBox.Text, "Macroscope imaging finished", "The Baym Lab macroscope has finished imaging your plates.")
        'Console.WriteLine("email error is " & emailError)
        'plateNum = 1
        'Dim shoulder As Double
        'GX.MotorGetCurrentPosition(1, shoulder)
        'Dim z As Double
        'GX.MotorGetCurrentPosition(2, z)
        'Dim elbow As Double
        'GX.MotorGetCurrentPosition(3, elbow)
        'Dim wrist As Double
        'GX.MotorGetCurrentPosition(4, wrist)
        'Console.WriteLine("current position: shoulder " & shoulder & " z " & z & " elbow " & elbow & " wrist " & wrist)
        'Dim test As Short
        'test = GX.MoveRelativeCartesian({50, 0, 0, 50}, 5, 5, True)
        'test = GX.MoveRelativeCartesian({100, 0, 0, -270}, 10, 10, True)

        'Console.WriteLine("here " & test)
        'GX.TeachPointMoveTo("Stack3Approach", 10, 10, True)
        'GX.RemovePlateFromStack("Stack3Approach", "Stack3Bottom", 30, 1, 0, 10, True, -7, 1, 1000)
    End Sub

    Public stack1Full As Boolean = False
    Public stack2Full As Boolean = False
    Public stack3Full As Boolean = False
    Public stack4Full As Boolean = False
    Private Sub StackOneFull_CheckedChanged(sender As Object, e As EventArgs) Handles StackOneFull.CheckedChanged
        If stack1Full Then
            stack1Full = False
            'stacks(0) = 0
        Else
            stack1Full = True
            'stacks(0) = 1
        End If
    End Sub

    Private Sub StackTwoFull_CheckedChanged(sender As Object, e As EventArgs) Handles StackTwoFull.CheckedChanged
        If stack2Full Then
            stack2Full = False
            'stacks(1) = 0
        Else
            stack2Full = True
            'stacks(1) = 1
        End If
    End Sub

    Private Sub StackThreeFull_CheckedChanged(sender As Object, e As EventArgs) Handles StackThreeFull.CheckedChanged
        If stack3Full Then
            stack3Full = False
            'stacks(2) = 0
        Else
            stack3Full = True
            'stacks(2) = 1
        End If
    End Sub

    Private Sub StackFourFull_CheckedChanged(sender As Object, e As EventArgs) Handles StackFourFull.CheckedChanged
        If stack4Full Then
            stack4Full = False
            'stacks(3) = 0
        Else
            stack4Full = True
            'stacks(3) = 1
        End If
    End Sub


#End Region

#Region "lights timer"
    Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
        If (m.Msg >= &H100 And m.Msg <= &H109) Or (m.Msg >= &H200 And m.Msg <= &H20E) Or (m.Msg >= &HA0 And m.Msg <= &HAD) Then
            Timer1.Stop()
            Timer1.Start()
            Console.WriteLine("acitivty detected")
        End If
    End Function

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Timer1.Stop()
        If Not GX.IsInitialized Then
            lights.all_off()
            MessageBox.Show("Lights timed out")
        End If
        Timer1.Start()

    End Sub

    Private Sub VBSample_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)
        Timer1.Enabled = False : Timer1.Enabled = True
        Console.WriteLine("mouse movement detected")
    End Sub

#End Region


End Class

