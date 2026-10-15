Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：汎用DAOツール クラス
    ''' 作成日        ：2019/08/25      Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolCommon

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "マスタメンテ　直近登録日時、担当者データ取得"
        ''' <summary>
        ''' マスタメンテ　直近登録日時、担当者データ取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetMstInsInfo(sTableNm As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT TOP 1")
                sbSQL.AppendLine("    INS_DATETIME")
                sbSQL.AppendLine("  , INS_EMPLOYEE_CD ")
                sbSQL.AppendLine("  , INS_EMPLOYEE_NAME ")
                sbSQL.AppendLine("FROM " & sTableNm)
                sbSQL.AppendLine("WHERE  INS_DATETIME = (SELECT MAX(INS_DATETIME) FROM " & sTableNm & ")")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region

#Region "棚卸を開始しているか否かを求めるSQL構文文字列を返す"
        ''' <summary>
        ''' 棚卸を開始しているか否かを求めるSQL構文文字列を返す
        ''' </summary>
        ''' <returns>
        ''' SQL構文文字列（棚卸を開始していなければ、0, 開始していれば、1以上を返すSQL構文）
        ''' </returns>
        ''' <remarks>2020/02/12 nv inoue Add</remarks>
        Public Shared Function GetSqlStrChkInvt() As String

            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.Append("SELECT COUNT(START_DEC_TS) FROM T_INVENTORY WHERE INV_CONFIRM_TS IS NULL")

            Return sbSQL.ToString()

        End Function
#End Region

#Region "棚卸を開始しているか否かを取得する"
        ''' <summary>
        ''' 棚卸を開始しているか否かを取得する
        ''' </summary>
        ''' <returns>
        ''' 0:棚卸を開始していない  0以外:棚卸を開始している
        ''' </returns>
        ''' <remarks>2020/02/12 nv inoue Add</remarks>
        Public Shared Function CheckInventorytStart() As Integer
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim tbl As New DataTable

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.Append("SELECT COUNT(START_DEC_TS) FROM T_INVENTORY WHERE INV_CONFIRM_TS IS NULL")
                tbl = clsDB.SVFncSelectDataTable(GetSqlStrChkInvt())

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return CInt(tbl.Rows(0).Item(0))

        End Function
#End Region

#Region "対象日付の取得(稼働日考慮)"
        ''' <summary>
        ''' 対象日付の取得(稼働日考慮)
        ''' </summary>
        ''' <param name="selectDate">指定日</param>
        ''' <param name="addDays">加算日</param>
        ''' <param name="workFlg">稼働日実日区分</param>
        ''' <returns>対象日付 エラー時、0</returns>
        Public Shared Function GetTargetDate(selectDate As Date, addDays As Integer, workFlg As String) As Integer
            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT dbo.TARGET_DATE_SF(@SelectDate, @AddDays, @WorkFlg) AS TargetDate")

            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            params.Add(New SqlParameter($"@SelectDate", selectDate.ToString("yyyyMMdd")))
            params.Add(New SqlParameter($"@AddDays", addDays))
            params.Add(New SqlParameter($"@WorkFlg", workFlg))

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Dim ret As Integer = 0
            Try
                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                If tbl.Rows.Count <> 0 Then
                    If Not IsDBNull(tbl.Rows(0).Item("TargetDate")) Then
                        ret = Integer.Parse(tbl.Rows(0).Item("TargetDate"))
                    End If
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return ret
        End Function
#End Region

#Region "カレンダーマスタから稼働日の件数を取得します。"
        ''' <summary>
        ''' カレンダーマスタから稼働日の件数を取得します。
        ''' </summary>
        ''' <returns>稼働日の件数</returns>
        Public Shared Function GetWorkDayCount() As Integer
            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT COUNT(*) AS WorkDayCount FROM M_CALENDAR WHERE NOT_WORK_FLG = '" & NotWorkFlg.No & "'")

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Dim ret As Integer = 0
            Try
                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                If tbl.Rows.Count <> 0 Then
                    ret = Integer.Parse(tbl.Rows(0).Item("WorkDayCount"))
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return ret
        End Function
#End Region

#Region "引数に指定した文字列から、指定バイト数の文字列を取り出します。"

        ''' <summary>
        ''' 引数に指定した文字列から、指定バイト数の文字列を取り出します。
        ''' </summary>
        ''' <param name="sTarget">検査する文字列</param>
        ''' <param name="iByteLength">バイト数</param>
        ''' <returns>取り出した文字列</returns>
        ''' <remarks></remarks>
        Public Shared Function SubStringByte(ByVal sTarget As String, ByVal iByteLength As Integer) As String
            Dim ENC As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_Jis")
            '指定した文字列が指定したバイト数を超える場合、超えた文字を切り捨てます。
            If ENC.GetByteCount(sTarget) > iByteLength Then
                '--「あいuえお」という文字列の場合、「あいuえ」、「あいu」と検査していき
                '--指定バイト以内に収まった文字列を取り出す。
                For idx As Integer = sTarget.Length - 1 To 1 Step -1
                    If ENC.GetByteCount(sTarget.Substring(0, idx)) <= iByteLength Then
                        sTarget = sTarget.Substring(0, idx)
                        Exit For
                    End If
                Next
            End If
            Return sTarget
        End Function
#End Region

#Region "引数に指定した文字列を、指定したバイト数の固定長文字列に整形します。(Shift_Jis)"

        ''' <summary>
        ''' 引数に指定した文字列を、指定したバイト数の固定長文字列に整形します。
        ''' 指定した文字列が指定したバイト数に満たない場合、文字列の後ろに半角スペースが補われます。
        ''' 指定した文字列が指定したバイト数を超える場合、超えた文字を切り捨てます。。
        ''' ※注意
        ''' 検査する文字列を「123あ」、バイト数を4バイトとした場合、「123半角スペース」が返ります。
        ''' </summary>
        ''' <param name="sTarget">検査する文字列</param>
        ''' <param name="iByteLength">バイト数</param>
        ''' <returns>整形後の文字列</returns>
        ''' <remarks></remarks>
        Public Shared Function FixByteString(ByVal sTarget As String, ByVal iByteLength As Integer) As String
            Dim ENC As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_Jis")

            '指定した文字列が指定したバイト数を超える場合、超えた文字を切り捨てます。
            sTarget = SubStringByte(sTarget, iByteLength)

            '指定した文字列が指定したバイト数に満たない場合、文字列の後ろに半角スペースが補われます。
            If ENC.GetByteCount(sTarget) < iByteLength Then
                '--指定した文字列のバイトを、指定バイトの配列にリサイズし、空(=0)のバイトには半角スペース(=32)を格納する。
                Dim bytes As Byte() = ENC.GetBytes(sTarget)
                Array.Resize(bytes, iByteLength)
                For idx As Integer = 0 To bytes.Length - 1
                    If bytes(idx) = 0 Then
                        bytes(idx) = 32
                    End If
                Next
                sTarget = ENC.GetString(bytes)
            End If

            Return sTarget

        End Function
#End Region

#Region "引数に指定した文字列から、指定バイト数の文字列を取り出します。"

        ''' <summary>
        ''' yyyyMMdd の日付を yyyy/MM/dd の文字列に変換
        ''' </summary>
        ''' <param name="iDate">日付 yyyyMMdd</param>
        ''' <returns> yyyy/MM/dd の文字列</returns>
        ''' <remarks></remarks>
        Public Shared Function NumDateString(ByVal iDate As Integer) As String
            Try
                Dim dtWk As Date = New Date(iDate \ 10000, iDate \ 100 Mod 100, iDate Mod 100)

                Return dtWk.ToString("yyyy/MM/dd")

            Catch ex As Exception
                Return String.Empty
            End Try
        End Function
#End Region

#Region "引数の特殊設定名が特殊権限に存在するかどうかチェックします。"
        ''' <summary>
        ''' 引数の特殊設定名が特殊権限に存在するかどうかチェックします。
        ''' </summary>
        ''' <param name="authOption">特殊権限</param>
        ''' <param name="authOptionName">存在チェックしたい特殊設定名</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Shared Function IsExistsAuthOption(authOption As String, authOptionName As String) As Boolean
            If String.IsNullOrEmpty(authOption) Then
                Return False
            End If

            Dim authOptions As String() = authOption.Split(",")
            Dim isExist As Boolean = False
            For Each item As String In authOptions
                If item = authOptionName Then
                    isExist = True
                    Exit For
                End If
            Next

            Return isExist
        End Function
#End Region

'2023/02/08 Kawamoto ADD START
#Region "登録/更新可能な拠点情報を取得する"
        ''' <summary>
        ''' 登録/更新可能な拠点情報を取得する
        ''' <returns>権限一覧</returns>
        ''' </summary>
        Public Shared Function GetAuthLocation(ByRef prmLoginInfo As RfidEntity.LoginUser, ByRef prmAuthInfo As AuthInfo) As List(Of String)
            GetAuthLocation = New List(Of String)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try

                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("  FROM dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
                sbSQL.AppendLine(" WHERE AUTH.UPD_FLG = '" & Misc.UpdFlg.Yes & "'")
                params.Add(New SqlParameter("@EmployeeCd", prmLoginInfo.EmployeeCd))
                params.Add(New SqlParameter("@ReadAuthFlg", prmAuthInfo.ReadAuthFlg))
                params.Add(New SqlParameter("@UpdAuthFlg", prmAuthInfo.UpdAuthFlg))
                params.Add(New SqlParameter("@ReadHubAuthFlg", prmAuthInfo.ReadHubAuthFlg))
                params.Add(New SqlParameter("@UpdHubAuthFlg", prmAuthInfo.UpdHubAuthFlg))
                params.Add(New SqlParameter("@UpdAuthType", Misc.UpdAuthType.UPD_AUTH))

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                For i As Integer = 0 To tbl.Rows.Count - 1
                    GetAuthLocation.Add(tbl.Rows(i).Item("LOC_L_CD"))
                Next

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region
'2023/02/08 Kawamoto ADD END
    End Class
End Namespace