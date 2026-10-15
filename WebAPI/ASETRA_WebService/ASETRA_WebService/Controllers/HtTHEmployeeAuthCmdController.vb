Imports System.Security.Cryptography
Imports System.Web.Http
Imports System.Web.Http.Cors

Namespace Controllers
    ''' <summary>
    ''' 社員認証を行うコントローラー
    ''' </summary>
    Public Class HtTHEmployeeAuthCmdController
        Inherits ApiController

#Region "固定値定義"
        ''' <summary>
        ''' 処理名
        ''' </summary>
        Private TASK_NAME As String = Me.GetType().Name
#End Region

#Region "変数定義"

        ''' <summary>
        ''' ログ出力クラス
        ''' </summary>
        Private logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
#End Region

#Region "Public メソッド"
        ''' <summary>
        ''' 社員認証処理
        ''' </summary>
        ''' <param name="value">社員情報</param>
        ''' <returns>戻り値クラス</returns>
        ''' <remarks>api/HtTHEmployeeAuthCmd呼び出し</remarks>
        <HttpPost>
        <EnableCors("*", "*", "*")>
        Public Function Post(<FromBody> ByVal value As HtTHEmployeeAuthCmdReqParameter) As HtTHEmployeeAuthCmdResParameter
            Dim ret As HtTHEmployeeAuthCmdResParameter = New HtTHEmployeeAuthCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.OK)}
            logger.Info($"Input：{Newtonsoft.Json.JsonConvert.SerializeObject(value)}")
            If Not ModelState.IsValid Then
                '送信情報が不正
                logger.Info($"SEND INFO ERROR")
                Return New HtTHEmployeeAuthCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .message = CommonConst.MSG_INV_ARG}
            End If

            Dim db As New SystemDBContext

            If db.p_DataBaseOpenFlag = False Then
                'DB接続異常
                logger.Info($"DB ERROR")
                Return New HtTHEmployeeAuthCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .message = CommonConst.MSG_ERR_DB}
            End If

            Try
                '引数.ログイン担当者コードに該当するスーパー管理者以外の社員コードのレコードを抽出する。
                Dim emp As M_EMPLOYEE = db.M_EMPLOYEE _
                                                            .Where(Function(x) x.EMPLOYEE_CD = value.employeeCd AndAlso x.AUTH_DIV <> "S") _
                                                            .FirstOrDefault

                If IsNothing(emp) Then
                    '存在しない場合
                    ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                    ret.message = CommonFunction.GetMessage(db, CommonWord.MSG_ERR_EMPLOYEENOT_REGIST)
                ElseIf emp.PASSWORD <> DecryptString(value.password, "asetra_system_key") Then
                    'パスワードが一致しない場合
                    ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                    ret.message = CommonFunction.GetMessage(db, CommonWord.MSG_ERR_PASSWORD)
                ElseIf emp.RETIRE_FLG = CommonConst.RETIRE_FLG.YES Then
                    '退職済みの場合
                    ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                    ret.message = CommonFunction.GetMessage(db, CommonWord.MSG_ERR_RETIRED)
                Else
                    '存在する場合

                    '本部フラグ
                    Dim stringHeadOffice As String = db.M_DEPARTMENT _
                                                            .Where(Function(x) x.DEPARTMENT_CD = emp.DEPARTMENT_CD) _
                                                            .Select(Function(x) x.HEAD_OFFICE_FLG) _
                                                            .FirstOrDefault
                    Dim largeLocList As List(Of String) = db.M_EMPLOYEE_HUB _
                                                            .Where(Function(x) x.EMPLOYEE_CD = value.employeeCd) _
                                                            .Select(Function(x) x.HUB_LOC_L_CD) _
                                                            .ToList()

                    If False = largeLocList.Any() Then
                        '所属拠点が1件もない場合
                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                        ret.message = CommonFunction.GetMessage(db, CommonWord.MSG_ERR_NO_ASSIGNED_BASE)
                        logger.Info("RETURN " + ret.returnCode.ToString)
                        Return ret
                    End If

                    Dim isLocRfid As Boolean = db.M_LOCATION _
                                                .Where(Function(x) largeLocList.Contains(x.LOC_L_CD)) _
                                                .Any(Function(x) x.LOC_RFID_FLG = "Y")

                    ret.headOfficeFlg = stringHeadOffice
                    ret.employeeAuth = emp.AUTH_DIV
                    If isLocRfid Then
                        ret.locRFIDFlg = "Y"
                    Else
                        ret.locRFIDFlg = "N"
                    End If

                    ret.employeeName = emp.EMPLOYEE_NAME

                End If
            Catch ex As System.Data.Common.DbException
                'DB系の例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.message = CommonConst.MSG_ERR_DB
            Catch ex As Exception
                '予期せぬ例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.message = CommonConst.MSG_EXCEPTION
            End Try

            logger.Info("RETURN " + ret.returnCode.ToString)
            Return ret
        End Function

        ' 復号化関数
        Public Function DecryptString(cipherText As String, key As String) As String
            Dim fullCipher As Byte() = Convert.FromBase64String(cipherText)


            ' 先頭 16 バイトが IV
            Dim iv(15) As Byte
            Array.Copy(fullCipher, iv, 16)

            ' 残りが暗号文
            Dim cipher(fullCipher.Length - 16 - 1) As Byte
            Array.Copy(fullCipher, 16, cipher, 0, cipher.Length)

            Using aes As Aes = Aes.Create()
                ' Java の getKeyBytes(key) と同じ処理をする必要がある
                aes.Key = GetKeyBytes(key)  ' ← ここ重要！
                aes.IV = iv
                aes.Mode = CipherMode.CBC
                aes.Padding = PaddingMode.PKCS7

                Using decryptor As ICryptoTransform = aes.CreateDecryptor()

                    Dim decryptedBytes As Byte() = decryptor.TransformFinalBlock(cipher, 0, cipher.Length)
                    Return Encoding.UTF8.GetString(decryptedBytes)
                End Using
            End Using
        End Function

        Private Function GetKeyBytes(key As String) As Byte()
            ' Java 側の getKeyBytes(key) と合わせる必要がある
            ' 例えば「UTF-8でエンコード → 長さを16/24/32に調整」など
            Dim bytes = Encoding.UTF8.GetBytes(key)
            Dim keyBytes(31) As Byte ' 32バイトに固定（AES-256）
            Array.Copy(bytes, keyBytes, Math.Min(bytes.Length, keyBytes.Length))
            Return keyBytes
        End Function

#End Region

    End Class
End Namespace