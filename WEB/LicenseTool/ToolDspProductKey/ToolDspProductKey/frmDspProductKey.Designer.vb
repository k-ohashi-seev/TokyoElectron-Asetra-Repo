<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmDspProductKey
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtProductNo = New System.Windows.Forms.TextBox()
        Me.txtProductKey = New System.Windows.Forms.TextBox()
        Me.txtLisenceKey = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cmdDsp = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(38, 107)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(72, 12)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "プロダクト番号"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(38, 189)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(68, 12)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "プロダクトキー"
        '
        'txtProductNo
        '
        Me.txtProductNo.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtProductNo.Font = New System.Drawing.Font("ＭＳ ゴシック", 20.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.txtProductNo.Location = New System.Drawing.Point(40, 122)
        Me.txtProductNo.Name = "txtProductNo"
        Me.txtProductNo.ReadOnly = True
        Me.txtProductNo.Size = New System.Drawing.Size(704, 34)
        Me.txtProductNo.TabIndex = 1
        '
        'txtProductKey
        '
        Me.txtProductKey.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtProductKey.Font = New System.Drawing.Font("ＭＳ ゴシック", 20.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.txtProductKey.Location = New System.Drawing.Point(40, 204)
        Me.txtProductKey.Name = "txtProductKey"
        Me.txtProductKey.ReadOnly = True
        Me.txtProductKey.Size = New System.Drawing.Size(704, 34)
        Me.txtProductKey.TabIndex = 2
        '
        'txtLisenceKey
        '
        Me.txtLisenceKey.BackColor = System.Drawing.Color.White
        Me.txtLisenceKey.Font = New System.Drawing.Font("ＭＳ ゴシック", 20.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.txtLisenceKey.Location = New System.Drawing.Point(42, 42)
        Me.txtLisenceKey.Name = "txtLisenceKey"
        Me.txtLisenceKey.Size = New System.Drawing.Size(704, 34)
        Me.txtLisenceKey.TabIndex = 0
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(40, 27)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(102, 12)
        Me.Label3.TabIndex = 4
        Me.Label3.Text = "DBのLICENSE_KEY"
        '
        'cmdDsp
        '
        Me.cmdDsp.Location = New System.Drawing.Point(353, 280)
        Me.cmdDsp.Name = "cmdDsp"
        Me.cmdDsp.Size = New System.Drawing.Size(391, 34)
        Me.cmdDsp.TabIndex = 6
        Me.cmdDsp.Text = "表示"
        Me.cmdDsp.UseVisualStyleBackColor = True
        '
        'frmDspProductKey
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(795, 359)
        Me.Controls.Add(Me.cmdDsp)
        Me.Controls.Add(Me.txtProductKey)
        Me.Controls.Add(Me.txtProductNo)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtLisenceKey)
        Me.Controls.Add(Me.Label3)
        Me.Name = "frmDspProductKey"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents txtProductNo As TextBox
    Friend WithEvents txtProductKey As TextBox
    Friend WithEvents txtLisenceKey As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents cmdDsp As Button
End Class
