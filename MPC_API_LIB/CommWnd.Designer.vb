Imports System.Runtime.InteropServices

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CommWnd
    Inherits System.Windows.Forms.UserControl

    'UserControl esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Richiesto da Progettazione Windows Form
    Private components As System.ComponentModel.IContainer

    'NOTA: la procedura che segue è richiesta da Progettazione Windows Form
    'Può essere modificata in Progettazione Windows Form.  
    'Non modificarla nell'editor del codice.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CommWnd))
        Me.SuspendLayout()
        '
        'CommWnd
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.Name = "CommWnd"
        Me.Size = New System.Drawing.Size(36, 36)
        Me.ResumeLayout(False)

    End Sub

    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        If m.Msg = SYSMSG.WM_COPYDATA Then
            Dim data As CopyData
            Dim message As String
            ' get the data...
            data = CType(m.GetLParam(GetType(CopyData)), CopyData)
            message = Marshal.PtrToStringAuto(data.lpData, data.cbData \ Marshal.SystemDefaultCharSize)

            lastDataReceived = data
            lastMsgReceived = message

            ' let them know we processed the message...
            m.Result = New IntPtr(1)
            RaiseEvent MsgReceived(m)
        Else
            MyBase.WndProc(m)
        End If
    End Sub

    Protected Overrides Sub SetVisibleCore(ByVal value As Boolean)
        MyBase.SetVisibleCore(False)
    End Sub

End Class
