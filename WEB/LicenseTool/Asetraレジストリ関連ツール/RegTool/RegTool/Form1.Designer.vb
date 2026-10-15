<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Me.cmdGetTrialStartDate = New System.Windows.Forms.Button()
        Me.cmdSetTrialStartDate = New System.Windows.Forms.Button()
        Me.txtTrialStartDate = New System.Windows.Forms.TextBox()
        Me.txtProductKey = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmdClrSetTrialStartDate = New System.Windows.Forms.Button()
        Me.cmdClrProductKey = New System.Windows.Forms.Button()
        Me.cmdSetProductKey = New System.Windows.Forms.Button()
        Me.cmdGetProductKey = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'cmdGetTrialStartDate
        '
        Me.cmdGetTrialStartDate.Location = New System.Drawing.Point(72, 90)
        Me.cmdGetTrialStartDate.Name = "cmdGetTrialStartDate"
        Me.cmdGetTrialStartDate.Size = New System.Drawing.Size(103, 57)
        Me.cmdGetTrialStartDate.TabIndex = 0
        Me.cmdGetTrialStartDate.Text = "取得"
        Me.cmdGetTrialStartDate.UseVisualStyleBackColor = True
        '
        'cmdSetTrialStartDate
        '
        Me.cmdSetTrialStartDate.Location = New System.Drawing.Point(72, 173)
        Me.cmdSetTrialStartDate.Name = "cmdSetTrialStartDate"
        Me.cmdSetTrialStartDate.Size = New System.Drawing.Size(103, 57)
        Me.cmdSetTrialStartDate.TabIndex = 1
        Me.cmdSetTrialStartDate.Text = "セット"
        Me.cmdSetTrialStartDate.UseVisualStyleBackColor = True
        '
        'txtTrialStartDate
        '
        Me.txtTrialStartDate.Location = New System.Drawing.Point(45, 44)
        Me.txtTrialStartDate.Name = "txtTrialStartDate"
        Me.txtTrialStartDate.Size = New System.Drawing.Size(160, 19)
        Me.txtTrialStartDate.TabIndex = 2
        '
        'txtProductKey
        '
        Me.txtProductKey.Location = New System.Drawing.Point(241, 44)
        Me.txtProductKey.Name = "txtProductKey"
        Me.txtProductKey.Size = New System.Drawing.Size(330, 19)
        Me.txtProductKey.TabIndex = 3
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(70, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(85, 12)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "トライアル開始日"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(385, 9)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(68, 12)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "プロダクトキー"
        '
        'cmdClrSetTrialStartDate
        '
        Me.cmdClrSetTrialStartDate.Location = New System.Drawing.Point(72, 254)
        Me.cmdClrSetTrialStartDate.Name = "cmdClrSetTrialStartDate"
        Me.cmdClrSetTrialStartDate.Size = New System.Drawing.Size(103, 57)
        Me.cmdClrSetTrialStartDate.TabIndex = 6
        Me.cmdClrSetTrialStartDate.Text = "クリア"
        Me.cmdClrSetTrialStartDate.UseVisualStyleBackColor = True
        '
        'cmdClrProductKey
        '
        Me.cmdClrProductKey.Location = New System.Drawing.Point(371, 254)
        Me.cmdClrProductKey.Name = "cmdClrProductKey"
        Me.cmdClrProductKey.Size = New System.Drawing.Size(103, 57)
        Me.cmdClrProductKey.TabIndex = 9
        Me.cmdClrProductKey.Text = "クリア"
        Me.cmdClrProductKey.UseVisualStyleBackColor = True
        '
        'cmdSetProductKey
        '
        Me.cmdSetProductKey.Location = New System.Drawing.Point(371, 173)
        Me.cmdSetProductKey.Name = "cmdSetProductKey"
        Me.cmdSetProductKey.Size = New System.Drawing.Size(103, 57)
        Me.cmdSetProductKey.TabIndex = 8
        Me.cmdSetProductKey.Text = "セット"
        Me.cmdSetProductKey.UseVisualStyleBackColor = True
        '
        'cmdGetProductKey
        '
        Me.cmdGetProductKey.Location = New System.Drawing.Point(371, 90)
        Me.cmdGetProductKey.Name = "cmdGetProductKey"
        Me.cmdGetProductKey.Size = New System.Drawing.Size(103, 57)
        Me.cmdGetProductKey.TabIndex = 7
        Me.cmdGetProductKey.Text = "取得"
        Me.cmdGetProductKey.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(716, 361)
        Me.Controls.Add(Me.cmdClrProductKey)
        Me.Controls.Add(Me.cmdSetProductKey)
        Me.Controls.Add(Me.cmdGetProductKey)
        Me.Controls.Add(Me.cmdClrSetTrialStartDate)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtProductKey)
        Me.Controls.Add(Me.txtTrialStartDate)
        Me.Controls.Add(Me.cmdSetTrialStartDate)
        Me.Controls.Add(Me.cmdGetTrialStartDate)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents cmdGetTrialStartDate As Button
    Friend WithEvents cmdSetTrialStartDate As Button
    Friend WithEvents txtTrialStartDate As TextBox
    Friend WithEvents txtProductKey As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents cmdClrSetTrialStartDate As Button
    Friend WithEvents cmdClrProductKey As Button
    Friend WithEvents cmdSetProductKey As Button
    Friend WithEvents cmdGetProductKey As Button
End Class
