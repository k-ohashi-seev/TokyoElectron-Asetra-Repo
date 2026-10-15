<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormQRMaker
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
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

    'Windows フォーム デザイナーで必要です。
    Private components As System.ComponentModel.IContainer

    'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
    'Windows フォーム デザイナーを使用して変更できます。  
    'コード エディターを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormQRMaker))
        Me.QRmakerAdCtl = New AxQRMAKERADLib.AxQRmakerAd()
        CType(Me.QRmakerAdCtl, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'QRmakerAdCtl
        '
        Me.QRmakerAdCtl.Enabled = True
        Me.QRmakerAdCtl.Location = New System.Drawing.Point(45, 45)
        Me.QRmakerAdCtl.Name = "QRmakerAdCtl"
        Me.QRmakerAdCtl.OcxState = CType(resources.GetObject("QRmakerAdCtl.OcxState"), System.Windows.Forms.AxHost.State)
        Me.QRmakerAdCtl.Size = New System.Drawing.Size(56, 57)
        Me.QRmakerAdCtl.TabIndex = 109
        '
        'FormQRMaker
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(149, 147)
        Me.Controls.Add(Me.QRmakerAdCtl)
        Me.Name = "FormQRMaker"
        Me.Text = "FormQRMaker"
        CType(Me.QRmakerAdCtl, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents QRmakerAdCtl As AxQRMAKERADLib.AxQRmakerAd
End Class
