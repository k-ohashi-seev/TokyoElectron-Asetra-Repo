Public Class Form1

    ''' <summary>
    ''' レジストリ関連 定数クラス
    ''' </summary>
    Private Class REGIST
        ''' <summary>レジストリ登録フォルダ</summary>
        Public Shared FolderNm As String = "Software\Asetra"

        ''' <summary>レジストリエラーっメッセージ</summary>
        Public Shared ErrMsg As String = "registry value error [{0}]"

        ''' <summary>レジストリ名　プロダクトキー</summary>
        Public Shared ProductKey As String = "ProductKey"
        ''' <summary>レジストリ　トライアル開始日</summary>
        Public Shared TrialStartDate As String = "TrialStartDate"
    End Class



    Private Sub cmdGetTrialStartDate_Click(sender As Object, e As EventArgs) Handles cmdGetTrialStartDate.Click
        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REGIST.FolderNm, False)

        'キーが存在しない時
        If regkey Is Nothing Then
            Return
        End If

        '値取得
        txtTrialStartDate.Text = DirectCast(regkey.GetValue(REGIST.TrialStartDate, String.Empty), String)

    End Sub

    Private Sub cmdSetTrialStartDate_Click(sender As Object, e As EventArgs) Handles cmdSetTrialStartDate.Click
        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(REGIST.FolderNm, True)

        Call regkey.SetValue(REGIST.TrialStartDate, txtTrialStartDate.Text)

    End Sub

    Private Sub cmdClrSetTrialStartDate_Click(sender As Object, e As EventArgs) Handles cmdClrSetTrialStartDate.Click
        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(REGIST.FolderNm, True)

        Call regkey.DeleteValue(REGIST.TrialStartDate)
        txtTrialStartDate.Text = String.Empty

    End Sub

    Private Sub cmdGetProductKey_Click(sender As Object, e As EventArgs) Handles cmdGetProductKey.Click
        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REGIST.FolderNm, False)
        'キーが存在しない時
        If regkey Is Nothing Then
            Return
        End If

        '値取得
        txtProductKey.Text = DirectCast(regkey.GetValue(REGIST.ProductKey, String.Empty), String)

    End Sub

    Private Sub cmdSetProductKey_Click(sender As Object, e As EventArgs) Handles cmdSetProductKey.Click
        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(REGIST.FolderNm, True)

        Call regkey.SetValue(REGIST.ProductKey, txtProductKey.Text)

    End Sub

    Private Sub cmdClrProductKey_Click(sender As Object, e As EventArgs) Handles cmdClrProductKey.Click
        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(REGIST.FolderNm, True)

        Call regkey.DeleteValue(REGIST.ProductKey)
        txtProductKey.Text = String.Empty

    End Sub
End Class
