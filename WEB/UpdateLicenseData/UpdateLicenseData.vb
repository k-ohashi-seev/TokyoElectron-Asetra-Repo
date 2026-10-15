Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text

Public Class UpdateLicenseData
    Private Sub cmdDbUpd_Click(sender As Object, e As EventArgs) Handles cmdDbUpd.Click
        Dim sLicenseKey As String = SubRegist.SubGetMacAddrAesStr()

        Dim sbSQL As StringBuilder = New StringBuilder
        Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
        Dim clsDB As clsSV_DB = New clsSV_DB

        If clsDB.SVFncOpenConnection("DefaultConnection") = False Then
            Call MessageBox.Show("Couldn't connect to Asetra's DB.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Exit Sub
        End If

        Try
            Dim tbl As New DataTable
            sbSQL.AppendLine("SELECT DB_VER, SERIAL_NO FROM M_ASETRA_VER")
            tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

            If tbl.Rows.Count = 0 Then
                Call MessageBox.Show("Asetra license information does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                Exit Sub
            End If

            'バージョンセット
            lblVersion.Text = tbl.Rows(0).Item("DB_VER").ToString

            Try
                'トライアル日付を取得
                Dim dtMkDbDate As Date? = GetMkDbDateDecrypt2(tbl.Rows(0).Item("SERIAL_NO").ToString)
                If dtMkDbDate Is Nothing Then
                    Call MessageBox.Show("Trial date has not yet been set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                    Exit Sub
                End If
                'dtRegist.Value = CType(dtMkDbDate, Date)

                Dim registKey As String = REGIST.FolderNm
                If lblVersion.Text < "2_0" Then
                    registKey = REGIST.OldFolderNm
                End If
                Dim sTrialStartDate As String = GetRegValue(REGIST.TrialStartDate, registKey)
                'BASE64、AES復号化
                sTrialStartDate = sTrialStartDate & "=="
                Dim byteBase64 As Byte() = System.Convert.FromBase64String(sTrialStartDate)
                sTrialStartDate = AesDecrypt(byteBase64)

                Dim dtTrialStartDate As Date = New Date(CInt(sTrialStartDate.Substring(0, 4)), CInt(sTrialStartDate.Substring(4, 2)), CInt(sTrialStartDate.Substring(6, 2)))
                dtTrial.Value = dtTrialStartDate

                cmdRegUpd.Enabled = True
                cmdDbUpd.Enabled = False

                Call MessageBox.Show("Retrieved from Asetra.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)

            Catch ex As Exception
                Call MessageBox.Show("The trial date setting is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                Exit Sub
            End Try
        Catch ex As Exception
            Call MessageBox.Show("Failed to obtain Asetra license information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Exit Sub
        Finally
            clsDB.SVSubCloseConnection()
        End Try

    End Sub
    Private Sub cmdRegUpd_Click(sender As Object, e As EventArgs) Handles cmdRegUpd.Click
        If MessageBox.Show("Want to update your trial date for Asetra?", "Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = DialogResult.Cancel Then
            Exit Sub
        End If


        'AES暗号化後、BASE64文字列に変換
        Dim byteAes As Byte() = AesEncrypt(dtTrial.Value.ToString("yyyyMMdd"))
        Dim sBase64Date As String = System.Convert.ToBase64String(byteAes)
        sBase64Date = sBase64Date.Substring(0, sBase64Date.Length - 2)      '後ろ2文字の"=="を取る

        Dim registKey As String = REGIST.FolderNm
        If lblVersion.Text < "2_0" Then
            registKey = REGIST.OldFolderNm
        End If

        Try
            'トライアル日付
            Call SetRegValue(REGIST.TrialStartDate, sBase64Date, registKey)

            'ライセンス情報は必ずクリアする
            Try
                Call DeleteRegValue(REGIST.ProductKey, registKey)
                'Call SetRegValue(REGIST.ProductKey, "", registKey)
            Catch ex As Exception
                'ここの削除エラーは握りつぶす
            End Try
        Catch ex As Exception
            Call MessageBox.Show("Failed to update the registry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Exit Sub
        End Try


        Dim clsDB As clsSV_DB = New clsSV_DB
        Dim sbSQL As New StringBuilder

        If clsDB.SVFncOpenConnection("DefaultConnection") = False Then
            Call MessageBox.Show("Couldn't connect to Asetra's DB.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Exit Sub
        End If

        Try
            sbSQL.AppendLine($"UPDATE M_ASETRA_VER SET SERIAL_NO = '{GetMkDbDateDecrypt(dtTrial.Value.ToString("yyyyMMdd"))}'")
            clsDB.SVFncExecuteNonQuery(sbSQL.ToString)

            Call MessageBox.Show("Trial date successfully updated.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)

        Catch ex As Exception
            Call MessageBox.Show("DB update failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Exit Sub
        Finally
            clsDB.SVFncFinishTransaction(True)
            clsDB.SVSubCloseConnection()
        End Try

    End Sub

    Private Sub UpdateLicenseData_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cmdRegUpd.Enabled = False
    End Sub

    Private Function AesEncrypt(text As String) As Byte()

        ' AES暗号化サービスプロバイダ
        Dim aes As New AesCryptoServiceProvider()
        aes.BlockSize = AESPARAM.BlockSize
        aes.KeySize = AESPARAM.KeySize
        aes.IV = Encoding.UTF8.GetBytes(AESPARAM.IV)
        aes.Key = Encoding.UTF8.GetBytes(AESPARAM.Key)
        aes.Mode = AESPARAM.Mode
        aes.Padding = AESPARAM.Padding

        ' 文字列をバイト型配列に変換
        'Dim src As Byte() = Encoding.Unicode.GetBytes(text)
        Dim src As Byte() = Encoding.UTF8.GetBytes(text)

        ' 暗号化する
        Using enc As ICryptoTransform = aes.CreateEncryptor()
            Dim dest As Byte() = enc.TransformFinalBlock(src, 0, src.Length)
            Return dest
        End Using

    End Function

    Private Sub SetRegValue(sNm As String, sValue As String, sRegistKey As String)
        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(sRegistKey, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree)

        Call regkey.SetValue(sNm, sValue)
    End Sub
    Private Sub DeleteRegValue(sNm As String, sRegistKey As String)
        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(sRegistKey, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree)

        regkey.DeleteValue(sNm)
    End Sub

    Private Class AESPARAM
        Public Shared BlockSize As Integer = 128
        Public Shared KeySize As Integer = 128
        Public Shared IV As String = "K6Jdw9B40M7BeGbq"
        Public Shared Key As String = "SAT0ASETRAWEBKEY"
        Public Shared Mode As CipherMode = CipherMode.CBC
        Public Shared Padding As PaddingMode = PaddingMode.PKCS7
    End Class

    ''' <summary>
    ''' レジストリ関連 定数クラス
    ''' </summary>
    Private Class REGIST
        ''' <summary>レジストリ登録フォルダ</summary>
        Public Shared FolderNm As String = "Software\STSV"
        ''' <summary>レジストリ旧登録フォルダ</summary>
        Public Shared OldFolderNm As String = "Software\Asetra"

        ''' <summary>レジストリエラーっメッセージ</summary>
        Public Shared ErrMsg As String = "registry value error [{0}]"

        ''' <summary>レジストリ名　プロダクトキー</summary>
        Public Shared ProductKey As String = "ProductKey"
        ''' <summary>レジストリ　トライアル開始日</summary>
        Public Shared TrialStartDate As String = "TrialStartDate"
    End Class

    Private Function GetRegValue(sNm As String, sRegistKey As String) As String

        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sRegistKey, False)
        'キーが存在しない時
        If regkey Is Nothing Then
            Return String.Empty
        End If

        '値取得
        Dim sRet As String = DirectCast(regkey.GetValue(sNm, String.Empty), String)

        Return sRet
    End Function

    Private Function AesDecrypt(src As Byte()) As String

        ' AES暗号化サービスプロバイダ
        Dim aes As New AesCryptoServiceProvider()
        aes.BlockSize = AESPARAM.BlockSize
        aes.KeySize = AESPARAM.KeySize
        aes.IV = Encoding.UTF8.GetBytes(AESPARAM.IV)
        aes.Key = Encoding.UTF8.GetBytes(AESPARAM.Key)
        aes.Mode = AESPARAM.Mode
        aes.Padding = AESPARAM.Padding

        ' 複号化する
        Using dec As ICryptoTransform = aes.CreateDecryptor()
            Dim dest As Byte() = dec.TransformFinalBlock(src, 0, src.Length)
            'Return Encoding.Unicode.GetString(dest)
            Return Encoding.UTF8.GetString(dest)
        End Using

    End Function

    Public Function GetMkDbDateDecrypt(sSerialNo As String) As String

        Dim dtMkDbDate As Date?

        '空値チェック
        If String.IsNullOrEmpty(sSerialNo) Then
            Return Nothing
        End If

        Dim sCHKDT As String = sSerialNo            '日付(YYYYMMDD)をそのままセット
        Dim sDT As String = (Convert.ToInt32(sSerialNo) + 54321234).ToString
        Dim nCHKSUM As Integer = 0

        For i As Integer = 1 To 8
            nCHKSUM = nCHKSUM + Convert.ToInt32(sCHKDT.Substring(i - 1, 1))
        Next

        'チェックデジット取得
        Return sDT + (nCHKSUM Mod 10).ToString
    End Function

    Public Function GetMkDbDateDecrypt2(sSerialNo As String) As Date?

        Dim dtMkDbDate As Date?

        '空値チェック
        If String.IsNullOrEmpty(sSerialNo) Then
            Return Nothing
        End If

        'チェックデジット取得
        Dim sChkDigit As String = sSerialNo.Substring(sSerialNo.Length - 1, 1)

        Dim iDate As Integer
        Try
            iDate = CInt(sSerialNo.Substring(0, sSerialNo.Length - 1)) - 54321234
        Catch ex As Exception
            Return Nothing
        End Try

        '数値から得られるチェックデジットを求める
        Dim sMkDbDate As String = iDate.ToString
        If sMkDbDate.Length <> 8 Then
            Return Nothing
        End If
        Dim iCalcChkDigit As Integer = 0
        For ii As Integer = 0 To 7
            iCalcChkDigit += CInt(sMkDbDate.Substring(ii, 1))
        Next
        Dim sCalcChkDigit As String = (iCalcChkDigit Mod 10).ToString

        'チェックデジットが計算値と同じでない時、エラー
        If sChkDigit <> sCalcChkDigit Then
            Return Nothing
        End If

        Try
            dtMkDbDate = New Date(CInt(sMkDbDate.Substring(0, 4)), CInt(sMkDbDate.Substring(4, 2)), CInt(sMkDbDate.Substring(6, 2)))
        Catch ex As Exception
            Return Nothing
        End Try

        Return dtMkDbDate
    End Function
End Class
