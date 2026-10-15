Imports System.Data.SqlClient
Imports System.Net
Imports System.Web.Http
Imports System.Web.Http.Cors
Imports Newtonsoft.Json

Namespace Controllers
    ''' <summary>
    ''' 操作ログを登録するコントローラー
    ''' </summary>
    Public Class HtEntryLogCmdController
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
        ''' 操作ログ登録処理
        ''' </summary>
        ''' <param name="value">操作ログ情報</param>
        ''' <returns>戻り値クラス</returns>
        ''' <remarks>api/HtEntryInventDataCmd呼び出し</remarks>
        <HttpPost>
        <EnableCors("*", "*", "*")>
        Public Function Post(<FromBody> ByVal value As HtEntryLogCmdReqParameter) As CommonResParameter
            Dim oldMst As Boolean = False
            Dim ret As CommonResParameter = New CommonResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.OK)}
            logger.Info($"Input：{Newtonsoft.Json.JsonConvert.SerializeObject(value)}")
            If Not ModelState.IsValid Then
                '送信情報が不正
                Return New CommonResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .wordCd = CommonWord.MSG_EXCEPTION}
            End If

            Dim db As New SystemDBContext

            If db.p_DataBaseOpenFlag = False Then
                'DB接続異常
                Return New CommonResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .wordCd = CommonWord.MSG_ERR_DB}
            End If

            logger.Info($"SEND COUNT {value.logDataDetailList.Count}")

            If value.logDataDetailList.Count <= 0 Then
                '送信情報が0件は正常終了
                Return ret
            End If

            Try
                Dim param As List(Of SqlParameter)
                Dim sql As String

                Using tx = db.Database.BeginTransaction((System.Data.IsolationLevel.Serializable))
                    logger.Info("TRANSACTION START")

                    '一時表の作成
                    db.Database.ExecuteSqlCommand(CreateTempTable())

                    '一時表へ操作ログ情報を登録する
                    For Each item As EntryLogDataDetail In value.logDataDetailList
                        param = New List(Of SqlParameter)
                        sql = InsertTempTable(item, param)
                        db.Database.ExecuteSqlCommand(sql, param.ToArray)

                        '操作者社員名称を入手する
                        If Not String.IsNullOrEmpty(item.opeEmployeeCd) Then
                            If db.Database.ExecuteSqlCommand(UpdateEmployeeName(item.opeLogId)) = 0 Then
                                oldMst = True
                                logger.Info($"NOT FOUND OPE_EMPLOYEE_CD={item.opeEmployeeCd}")
                            End If
                        End If
                    Next

                    '操作ログテーブルへ一括反映
                    Dim logUpdCnt As Integer = db.Database.ExecuteSqlCommand(InsertLogInfo())
                    If logUpdCnt <> value.logDataDetailList.Count Then
                        '更新件数が送信件数と異なる場合は、マスタ不整合扱い
                        oldMst = True
                    End If

                    If oldMst Then
                        'マスタが古い場合は警告終了
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_OLD_MST))
                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN)
                        ret.wordCd = CommonWord.MSG_OLD_MST
                    Else
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_RECEIVE_SUCCSE))
                    End If

                    db.SaveChanges()
                    tx.Commit()
                    logger.Info("COMMIT")
                End Using

            Catch ex As System.Data.Common.DbException
                'DB系の例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.wordCd = CommonWord.MSG_ERR_DB
            Catch ex As Exception
                '予期せぬ例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.wordCd = CommonWord.MSG_EXCEPTION
            End Try

            'DB切断
            db.subTerminate()
            Return ret
        End Function
#End Region

#Region "Private メソッド"
        ''' <summary>
        ''' 操作ログ登録用の一時表を作成するDDLを返す
        ''' </summary>
        ''' <returns>操作ログ登録用一時表DDL</returns>
        Private Function CreateTempTable() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("CREATE TABLE #OPERATION_LOG_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[OPE_LOG_ID] [BIGINT] NOT NULL ,        /* 処理順 */")
            sb.AppendLine("	[PROCESS_DT] [INT] NOT NULL ,               /* 処理日 */")
            sb.AppendLine("	[PROCESS_TM] [CHAR] (9) NOT NULL ,               /* 処理時刻 */")
            sb.AppendLine("	[PROCESS_TOOL_DIV] [CHAR] (1) NOT NULL ,    /* 処理ツール区分 */")
            sb.AppendLine("	[PROCESS_CAT] [CHAR] (2) NOT NULL ,			/* 処理種別 */")
            sb.AppendLine("	[PROCESS_CONTENTS] [NVARCHAR] (1024) NULL ,  /* 処理内容 */")
            sb.AppendLine("	[OPE_EMPLOYEE_CD] [NVARCHAR] (15) NULL ,	/* 操作者社員コード */")
            sb.AppendLine("	[OPE_EMPLOYEE_NAME] [NVARCHAR] (60) NULL 　	/* 操作者氏名 */")
            sb.AppendLine(")")

            logger.Info(vbcrlf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 操作ログ登録用の一時表にデータを登録するクエリとパラメータを返す
        ''' </summary>
        ''' <param name="item">操作ログ情報</param>
        ''' <param name="param">返却用パラメータ</param>
        ''' <returns>登録DML</returns>
        Private Function InsertTempTable(ByVal item As EntryLogDataDetail, ByRef param As List(Of SqlParameter)) As String
            Dim sb As StringBuilder = New StringBuilder()

            '一時表への登録
            sb.AppendLine("INSERT INTO #OPERATION_LOG_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[OPE_LOG_ID],")
            sb.AppendLine("	[PROCESS_DT],")
            sb.AppendLine("	[PROCESS_TM],")
            sb.AppendLine("	[PROCESS_TOOL_DIV],")
            sb.AppendLine("	[PROCESS_CAT],")
            sb.AppendLine("	[PROCESS_CONTENTS],")
            sb.AppendLine("	[OPE_EMPLOYEE_CD]")
            sb.AppendLine(")")
            sb.AppendLine("VALUES")
            sb.AppendLine("(")
            sb.AppendLine("	@ProcessLogId,")
            sb.AppendLine("	@ProcessDt,")
            sb.AppendLine("	@ProcessTm,")
            sb.AppendLine("	@ProcessToolDiv,")
            sb.AppendLine("	@ProcessCat,")
            sb.AppendLine("	@ProcessContents,")
            sb.AppendLine("	@OpeEmployeeCd")
            sb.AppendLine(")")

            'パラメータ設定
            param.Add(New SqlParameter("@ProcessLogId", item.opeLogId))
            param.Add(New SqlParameter("@ProcessDt", IIf(item.processDt Is Nothing, DBNull.Value, item.processDt)))
            param.Add(New SqlParameter("@ProcessTm", IIf(item.processTm Is Nothing, DBNull.Value, item.processTm)))
            param.Add(New SqlParameter("@ProcessToolDiv", IIf(item.processToolDiv Is Nothing, DBNull.Value, item.processToolDiv)))
            param.Add(New SqlParameter("@ProcessCat", IIf(item.processCat Is Nothing, DBNull.Value, item.processCat)))
            param.Add(New SqlParameter("@ProcessContents", IIf(item.processContents Is Nothing, DBNull.Value, item.processContents)))
            param.Add(New SqlParameter("@OpeEmployeeCd", IIf(item.opeEmployeeCd Is Nothing, DBNull.Value, item.opeEmployeeCd)))

            logger.Info(vbcrlf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 社員名を更新する
        ''' </summary>
        ''' <returns>更新件数</returns>
        Private Function UpdateEmployeeName(ByVal processLogId As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	OPERATION_LOG_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	 OPERATION_LOG_TEMP.OPE_EMPLOYEE_NAME = EMP.EMPLOYEE_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#OPERATION_LOG_TEMP AS OPERATION_LOG_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_EMPLOYEE AS EMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			OPERATION_LOG_TEMP.OPE_EMPLOYEE_CD = EMP.EMPLOYEE_CD")
            sb.AppendLine("WHERE")
            sb.AppendLine($"	OPE_LOG_ID = '{processLogId}'")

            logger.Info(vbcrlf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 操作ログテーブルにログを登録する。
        ''' </summary>
        ''' <returns>更新件数</returns>
        Private Function InsertLogInfo() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("INSERT INTO T_OPERATION_LOG")
            sb.AppendLine("(")
            sb.AppendLine("	 PROCESS_DT")
            sb.AppendLine("	,PROCESS_TM")
            sb.AppendLine("	,PROCESS_TOOL_DIV")
            sb.AppendLine("	,PROCESS_CAT")
            sb.AppendLine("	,PROCESS_CONTENTS")
            sb.AppendLine("	,OPE_EMPLOYEE_CD")
            sb.AppendLine("	,OPE_EMPLOYEE_NAME")
            sb.AppendLine("	,UPD_DATETIME")
            sb.AppendLine("	,UPD_EMPLOYEE_CD")
            sb.AppendLine("	,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")
            sb.AppendLine(")")
            sb.AppendLine("SELECT")
            sb.AppendLine("	 OPERATION_LOG_TEMP.PROCESS_DT ")
            sb.AppendLine("	,OPERATION_LOG_TEMP.PROCESS_TM ")
            sb.AppendLine(" ,OPERATION_LOG_TEMP.PROCESS_TOOL_DIV")
            sb.AppendLine("	,OPERATION_LOG_TEMP.PROCESS_CAT ")
            sb.AppendLine("	,OPERATION_LOG_TEMP.PROCESS_CONTENTS ")
            sb.AppendLine("	,OPERATION_LOG_TEMP.OPE_EMPLOYEE_CD ")
            sb.AppendLine("	,OPERATION_LOG_TEMP.OPE_EMPLOYEE_NAME ")
            sb.AppendLine("	,GETDATE()")
            sb.AppendLine("	,ISNULL(OPERATION_LOG_TEMP.OPE_EMPLOYEE_CD, ' ') ")
            sb.AppendLine("	,ISNULL(OPERATION_LOG_TEMP.OPE_EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($",'{TASK_NAME}'")
            sb.AppendLine("FROM")
            sb.AppendLine(" #OPERATION_LOG_TEMP AS OPERATION_LOG_TEMP")
            sb.AppendLine("ORDER BY")
            sb.AppendLine(" OPERATION_LOG_TEMP.OPE_LOG_ID,")
            sb.AppendLine(" OPERATION_LOG_TEMP.PROCESS_DT,")
            sb.AppendLine(" OPERATION_LOG_TEMP.PROCESS_TM")

            logger.Info(vbcrlf & sb.ToString)

            Return sb.ToString
        End Function

#End Region

    End Class
End Namespace