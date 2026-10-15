<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UpdateLicenseData
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.cmdDbUpd = New System.Windows.Forms.Button()
        Me.cmdRegUpd = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.dtTrial = New System.Windows.Forms.DateTimePicker()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'cmdDbUpd
        '
        Me.cmdDbUpd.Font = New System.Drawing.Font("MS UI Gothic", 12.0!)
        Me.cmdDbUpd.Location = New System.Drawing.Point(44, 32)
        Me.cmdDbUpd.Name = "cmdDbUpd"
        Me.cmdDbUpd.Size = New System.Drawing.Size(422, 85)
        Me.cmdDbUpd.TabIndex = 0
        Me.cmdDbUpd.Text = "Asetra information acquisition"
        Me.cmdDbUpd.UseVisualStyleBackColor = True
        '
        'cmdRegUpd
        '
        Me.cmdRegUpd.Font = New System.Drawing.Font("MS UI Gothic", 12.0!)
        Me.cmdRegUpd.Location = New System.Drawing.Point(44, 287)
        Me.cmdRegUpd.Name = "cmdRegUpd"
        Me.cmdRegUpd.Size = New System.Drawing.Size(422, 85)
        Me.cmdRegUpd.TabIndex = 1
        Me.cmdRegUpd.Text = "Update Trial Date"
        Me.cmdRegUpd.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(42, 215)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(56, 12)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Trial Date"
        '
        'dtTrial
        '
        Me.dtTrial.Location = New System.Drawing.Point(127, 210)
        Me.dtTrial.Name = "dtTrial"
        Me.dtTrial.Size = New System.Drawing.Size(200, 19)
        Me.dtTrial.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.Label2.Location = New System.Drawing.Point(42, 184)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(44, 12)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Version"
        '
        'lblVersion
        '
        Me.lblVersion.AutoSize = True
        Me.lblVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.lblVersion.Location = New System.Drawing.Point(125, 184)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(45, 12)
        Me.lblVersion.TabIndex = 2
        Me.lblVersion.Text = "          "
        '
        'UpdateLicenseData
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(524, 396)
        Me.Controls.Add(Me.dtTrial)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.cmdRegUpd)
        Me.Controls.Add(Me.cmdDbUpd)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "UpdateLicenseData"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Asetra Update Trial Dates"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents cmdDbUpd As Button
    Friend WithEvents cmdRegUpd As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents dtTrial As DateTimePicker
    Friend WithEvents Label2 As Label
    Friend WithEvents lblVersion As Label
End Class
