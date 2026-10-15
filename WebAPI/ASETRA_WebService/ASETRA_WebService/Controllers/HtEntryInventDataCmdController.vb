Imports System.Data.SqlClient
Imports System.Net
Imports System.Web.Http
Imports System.Web.Http.Cors
Imports Newtonsoft.Json

Namespace Controllers
    ''' <summary>
    ''' 実棚情報を更新するコントローラー
    ''' </summary>
    Public Class HtEntryInventDataCmdController
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
        ''' 実棚情報更新処理
        ''' </summary>
        ''' <param name="value">実棚情報</param>
        ''' <returns>戻り値クラス</returns>
        ''' <remarks>api/HtEntryInventDataCmd呼び出し</remarks>
        <HttpPost>
        <EnableCors("*", "*", "*")>
        Public Function Post(<FromBody> ByVal value As HtEntryInventDataCmdReqParameter) As CommonResParameter
            Dim oldMst As Boolean = False
            Dim processToolDiv As String = CommonConst.PROCESS_TOOL_H
            Dim ret As CommonResParameter = New CommonResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.OK)}
            logger.Info($"Input：{Newtonsoft.Json.JsonConvert.SerializeObject(value)}")
            If Not ModelState.IsValid Then
                '送信情報が不正
                logger.Info($"SEND INFO ERROR")
                Return New CommonResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .wordCd = CommonWord.MSG_EXCEPTION}
            End If

            Dim db As New SystemDBContext

            If db.p_DataBaseOpenFlag = False Then
                'DB接続異常
                logger.Info($"DB ERROR")
                Return New CommonResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .wordCd = CommonWord.MSG_ERR_DB}
            End If

            logger.Info($"SEND COUNT {value.ie.Count}")

            If value.ie.Count <= 0 Then
                '送信情報が0件は正常終了
                Return ret
            End If

            Try
                Dim param As List(Of SqlParameter)
                Dim sql As String

                '棚卸開始中か判断する
                Dim inventoryStart As Integer() = CType(db.Database.SqlQuery(Of Integer)(GetInventoryStart()).ToArray(), Integer())
                If inventoryStart.Count = 0 Then
                    '棚卸終了状態
                    Return New CommonResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.CONFIRM), .wordCd = CommonWord.MSG_CONFIRM_INVENTORY}
                End If

                '最終データの実棚更新社員の名称を入手する
                Dim emp As M_EMPLOYEE = db.M_EMPLOYEE.Find(value.ie.Last.ec)
                processToolDiv = value.td

                '更新件数カウント用変数
                Dim assetUpdCnt As Integer

                'ロックで更新できなかったデータがあるか調べる
                Dim notUpdatedList As List(Of LockAsset)
                Dim assetList As String = String.Empty
                'ロックエラー時にログに残す用の文言
                Dim lockErrorMessage As String = String.Empty

                Using tx = db.Database.BeginTransaction((System.Data.IsolationLevel.Serializable))
                    logger.Info("TRANSACTION START")

                    '一時表の作成
                    db.Database.ExecuteSqlCommand(CreateTempTable())

                    '一時表へ実棚情報を登録する
                    For Each item As EntryInventDataDetail In value.ie
                        param = New List(Of SqlParameter)
                        '20260925 Y.Ono ver4.3.1要求事項7 Start
                        'sql = InsertTempTable(item, param)
                        sql = InsertTempTable(processToolDiv, item, param)
                        '20260925 Y.Ono ver4.3.1要求事項7 End
                        db.Database.ExecuteSqlCommand(sql, param.ToArray)
                    Next

                    'ロケ大区分名称をセット
                    Dim locLUpdCnt As Integer = db.Database.ExecuteSqlCommand(UpdateLargeLocName())
                    If locLUpdCnt <> value.ie.Where(Function(x) Not String.IsNullOrEmpty(x.lc)).Count() Then
                        '更新件数が送信件数と異なる場合は、マスタ不整合扱い
                        oldMst = True
                        logger.Info($"UPD COUNT DIFFERENCE locLUpdCnt={locLUpdCnt} sendDataCnt={value.ie.Where(Function(x) Not String.IsNullOrEmpty(x.lc)).Count()}")
                    End If

                    'ロケ中区分名称をセット
                    Dim locMUpdCnt As Integer = db.Database.ExecuteSqlCommand(UpdateMiddleLocName())
                    If locMUpdCnt <> value.ie.Where(Function(x) Not String.IsNullOrEmpty(x.mc)).Count() Then
                        '更新件数が送信件数と異なる場合は、マスタ不整合扱い
                        oldMst = True
                        logger.Info($"UPD COUNT DIFFERENCE locMUpdCnt={locMUpdCnt} sendDataCnt={value.ie.Where(Function(x) Not String.IsNullOrEmpty(x.mc)).Count()}")
                    End If

                    'ロケ小区分名称をセット
                    Dim locSUpdCnt As Integer = db.Database.ExecuteSqlCommand(UpdateSmallLocName())
                    If locSUpdCnt <> value.ie.Where(Function(x) Not String.IsNullOrEmpty(x.sc)).Count() Then
                        '更新件数が送信件数と異なる場合は、マスタ不整合扱い
                        oldMst = True
                        logger.Info($"UPD COUNT DIFFERENCE locSUpdCnt={locSUpdCnt} sendDataCnt={value.ie.Where(Function(x) Not String.IsNullOrEmpty(x.sc)).Count()}")
                    End If

                    '社員名称をセット
                    Dim empUpdCnt As Integer = db.Database.ExecuteSqlCommand(UpdateEmployeeName())
                    If empUpdCnt <> value.ie.Where(Function(x) Not String.IsNullOrEmpty(x.ec)).Count() Then
                        '更新件数が送信件数と異なる場合は、マスタ不整合扱い
                        oldMst = True
                        logger.Info($"UPD COUNT DIFFERENCE empUpdCnt={empUpdCnt} sendDataCnt={value.ie.Where(Function(x) Not String.IsNullOrEmpty(x.ec)).Count()}")
                    End If

                    '実棚ステータスをセット
                    Dim statusUpdCnt As Integer = db.Database.ExecuteSqlCommand(UpdatePhyinvStatus())
                    If statusUpdCnt <> value.ie.Where(Function(x) Not String.IsNullOrEmpty(x.ps)).Count() Then
                        '更新件数が送信件数と異なる場合は、マスタ不整合扱い
                        oldMst = True
                        logger.Info($"UPD COUNT DIFFERENCE statusUpdCnt={statusUpdCnt} sendDataCnt={value.ie.Where(Function(x) Not String.IsNullOrEmpty(x.ps)).Count()}")
                    End If

                    '更新不要なレコードを削除する
                    Dim delInvCount = db.Database.ExecuteSqlCommand(DeletePhyinvData())

                    '資産棚卸テーブル一括反映
                    assetUpdCnt = db.Database.ExecuteSqlCommand(UpdatePhyinvInfo())

                    ' 2019/12/26 戸祭 コメントアウト
                    'If assetUpdCnt <> (value.ie.Count - delInvCount) Then
                    '    '更新件数が送信件数と異なる場合は、マスタ不整合扱い
                    '    oldMst = True
                    'End If

                    '棚卸管理テーブルへ実棚件数を反映する。
                    param = New List(Of SqlParameter)
                    sql = UpdateInventoryNum(emp, param)
                    db.Database.ExecuteSqlCommand(sql, param.ToArray)

                    If oldMst Then
                        'マスタが古い場合は警告終了
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_OLD_MST, processToolDiv))
                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN)
                        ret.wordCd = CommonWord.MSG_OLD_MST
                    Else
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_RECEIVE_SUCCSE, processToolDiv))
                    End If

                    db.SaveChanges()
                    tx.Commit()
                    logger.Info("COMMIT")

                    'ロックで更新できなかったデータがあるか調べる
                    notUpdatedList = db.Database.SqlQuery(Of LockAsset)(GetNotUpdataByLock()).ToList
                    lockErrorMessage = CommonFunction.GetMessage(db, CommonWord.MSG_ERR_LOCK)

                    'DB切断
                    db.subTerminate()
                End Using

                If assetUpdCnt > 0 Then
                    '登録件数ログ出力
                    Dim registrationTitle As String = CommonFunction.GetMessage(db, CommonWord.MSG_ERR_REGISTRATION_COUNTS)
                    CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, registrationTitle + assetUpdCnt.ToString, processToolDiv)
                End If

                If notUpdatedList.Count > 0 Then
                    'ロックで更新できなかったデータがある場合ログ出力
                    For Each assetNo As LockAsset In notUpdatedList
                        assetList += assetNo.assetNo + ", "
                    Next
                    assetList = assetList.Substring(0, assetList.Length - 2)

                    CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, lockErrorMessage + $" :{assetList}", processToolDiv)
                    ret.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN_ASSET_NO_SEND)
                    ret.wordCd = CommonWord.MSG_ERR_LOCK
                    ret.lockAssetList = notUpdatedList
                End If

            Catch ex As System.Data.Common.DbException
                'DB系の例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString, processToolDiv)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.wordCd = CommonWord.MSG_ERR_DB
            Catch ex As Exception
                '予期せぬ例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString, processToolDiv)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.wordCd = CommonWord.MSG_EXCEPTION
            End Try

            logger.Info("RETURN " + ret.returnCode.ToString)
            Return ret
        End Function
#End Region

#Region "Private メソッド"
        ''' <summary>
        ''' 棚卸開始状態を取得する(取得件数があれば開始中)
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetInventoryStart() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 1")
            sb.AppendLine("FROM")
            sb.AppendLine("  T_INVENTORY")
            sb.AppendLine("WHERE")
            sb.AppendLine("  INV_CONFIRM_TS IS NULL")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 実棚更新用の一時表を作成するDDLを返す
        ''' </summary>
        ''' <returns>実棚更新用一時表DDL</returns>
        Private Function CreateTempTable() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("CREATE TABLE #ASSET_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[ASSET_NO] [NVARCHAR] (24) NOT NULL ,			/* 資産番号 */")
            sb.AppendLine("	[CUR_ASSET_STATUS] [INT] NULL ,					/* 現 資産状態 */")
            sb.AppendLine("	[CUR_PHYINV_TS] [BIGINT] NULL ,					/* 現 実棚日時 */")
            sb.AppendLine("	[PHYINV_TS] [BIGINT] NULL ,						/* 実棚日時 */")
            sb.AppendLine("	[PHYINV_STATUS] [CHAR] NULL ,					/* 実棚ステータス */")
            sb.AppendLine("	[PHYINV_LOC_L_CD] [NVARCHAR] (10) NULL ,		/* 実棚ロケ大区分コード */")      '20210510 UPD Suzuki v1.3
            sb.AppendLine("	[PHYINV_LOC_M_CD] [NVARCHAR] (10) NULL ,		/* 実棚ロケ中区分コード */")      '20210510 UPD Suzuki v1.3
            sb.AppendLine("	[PHYINV_LOC_S_CD] [NVARCHAR] (10) NULL ,		/* 実棚ロケ小区分コード */")      '20210510 UPD Suzuki v1.3
            sb.AppendLine("	[PHYINV_LOC_L_NAME] [NVARCHAR] (60) NULL ,		/* 実棚ロケ大区分名称 */")
            sb.AppendLine("	[PHYINV_LOC_M_NAME] [NVARCHAR] (60) NULL ,		/* 実棚ロケ中区分名称 */")
            sb.AppendLine("	[PHYINV_LOC_S_NAME] [NVARCHAR] (60) NULL ,		/* 実棚ロケ小区分名称 */")
            sb.AppendLine("	[PHYINV_EMPLOYEE_CD] [NVARCHAR] (15) NULL ,		/* 実棚実施者社員コード */")
            sb.AppendLine("	[PHYINV_EMPLOYEE_NAME] [NVARCHAR] (60) NULL ,	/* 実棚実施者氏名 */")
            '20260925 Y.Ono ver4.3.1要求事項7 Start
            sb.AppendLine(" [PHYINV_TOOL_DIV] [CHAR] (1) NULL ,             /* 実棚処理ツール区分 */ ")
            '20260925 Y.Ono ver4.3.1要求事項7 End
            sb.AppendLine("	[UPD_DATETIME] [DATETIME] NULL ,			    /* 最終登録更新日時 */")
            sb.AppendLine("	[UPD_EMPLOYEE_CD] [NVARCHAR] (15) NULL ,	    /* 最終登録更新者社員コード */")
            sb.AppendLine("	[UPD_EMPLOYEE_NAME] [NVARCHAR] (60) NULL ,	    /* 最終登録更新者氏名 */")
            sb.AppendLine("	[UPD_TASK] [NVARCHAR] (255) NULL 			    /* 最終登録更新処理 */")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function
        '20260925 Y.Ono ver4.3.1要求事項7 Start

        ''' <summary>
        ''' 実棚更新用の一時表にデータを登録するクエリとパラメータを返す
        ''' </summary>
        ''' <param name="processToolDiv"></param>
        ''' <param name="item">実棚情報</param>
        ''' <param name="param">返却用パラメータ</param>
        ''' <returns>登録DML</returns>
        Private Function InsertTempTable(ByVal processToolDiv As String, ByVal item As EntryInventDataDetail, ByRef param As List(Of SqlParameter)) As String
            'Private Function InsertTempTable(ByVal item As EntryInventDataDetail, ByRef param As List(Of SqlParameter)) As String
            '20260925 Y.Ono ver4.3.1要求事項7 End
            Dim sb As StringBuilder = New StringBuilder()

            '一時表への登録
            sb.AppendLine("INSERT INTO #ASSET_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[ASSET_NO],")
            sb.AppendLine("	[PHYINV_TS],")
            sb.AppendLine("	[PHYINV_STATUS],")
            sb.AppendLine("	[PHYINV_LOC_L_CD],")
            sb.AppendLine("	[PHYINV_LOC_M_CD],")
            sb.AppendLine("	[PHYINV_LOC_S_CD],")
            sb.AppendLine("	[PHYINV_EMPLOYEE_CD]")
            '20260925 Y.Ono ver4.3.1要求事項7 Start
            sb.AppendLine(" ,[PHYINV_TOOL_DIV]")
            '20260925 Y.Ono ver4.3.1要求事項7 End
            sb.AppendLine(")")
            sb.AppendLine("VALUES")
            sb.AppendLine("(")
            sb.AppendLine("	@AssetNo,")
            sb.AppendLine("	@PhyinvTs,")
            sb.AppendLine("	@PhyinvStatus,")
            sb.AppendLine("	@PhyInvLocLCd,")
            sb.AppendLine("	@PhyInvLocMCd,")
            sb.AppendLine("	@PhyInvLocSCd,")
            sb.AppendLine("	@PhyInvEmployeeCd")
            '20260925 Y.Ono ver4.3.1要求事項7 Start
            sb.AppendLine(" ,@PhyInvToolDiv")
            '20260925 Y.Ono ver4.3.1要求事項7 End
            sb.AppendLine(")")

            'パラメータ設定
            param.Add(New SqlParameter("@AssetNo", IIf(item.an Is Nothing, DBNull.Value, item.an)))
            param.Add(New SqlParameter("@PhyinvTs", IIf(item.pt Is Nothing, DBNull.Value, item.pt)))
            param.Add(New SqlParameter("@PhyinvStatus", IIf(item.ps Is Nothing, DBNull.Value, item.ps)))
            param.Add(New SqlParameter("@PhyInvLocLCd", IIf(item.lc Is Nothing, DBNull.Value, item.lc)))
            param.Add(New SqlParameter("@PhyInvLocMCd", IIf(item.mc Is Nothing, DBNull.Value, item.mc)))
            param.Add(New SqlParameter("@PhyInvLocSCd", IIf(item.sc Is Nothing, DBNull.Value, item.sc)))
            param.Add(New SqlParameter("@PhyInvEmployeeCd", IIf(item.ec Is Nothing, DBNull.Value, item.ec)))
            '20260925 Y.Ono ver4.3.1要求事項7 Start
            param.Add(New SqlParameter("@PhyInvToolDiv", IIf(processToolDiv Is Nothing, DBNull.Value, processToolDiv)))
            '20260925 Y.Ono ver4.3.1要求事項7 End
            'logger.Info(vbCrLf & sb.ToString)                      '2020/02/13 戸祭 無意味なログ出力を削除

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ロケ大区分名称を更新する
        ''' </summary>
        Private Function UpdateLargeLocName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	PHYINV_LOC_L_NAME = LOC.LOC_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#ASSET_TEMP AS ASSET_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LOCATION AS LOC")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET_TEMP.PHYINV_LOC_L_CD = LOC.LOC_L_CD")
            sb.AppendLine($"		AND LOC_DIV = '{CommonConst.LOC_L_DIV}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ロケ中区分名称を更新する
        ''' </summary>
        Private Function UpdateMiddleLocName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	PHYINV_LOC_M_NAME = LOC.LOC_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#ASSET_TEMP AS ASSET_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LOCATION AS LOC")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET_TEMP.PHYINV_LOC_L_CD = LOC.LOC_L_CD")
            sb.AppendLine("		AND ASSET_TEMP.PHYINV_LOC_M_CD = LOC.LOC_M_CD")
            sb.AppendLine($"	AND LOC_DIV = '{CommonConst.LOC_M_DIV}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ロケ小区分名称を更新する
        ''' </summary>
        Private Function UpdateSmallLocName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	PHYINV_LOC_S_NAME = LOC.LOC_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#ASSET_TEMP AS ASSET_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LOCATION AS LOC")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET_TEMP.PHYINV_LOC_L_CD = LOC.LOC_L_CD")
            sb.AppendLine("		AND ASSET_TEMP.PHYINV_LOC_M_CD = LOC.LOC_M_CD")
            sb.AppendLine("		AND ASSET_TEMP.PHYINV_LOC_S_CD = LOC.LOC_S_CD")
            sb.AppendLine($"	AND LOC_DIV = '{CommonConst.LOC_S_DIV}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 社員名を更新する
        ''' </summary>
        Private Function UpdateEmployeeName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	 PHYINV_EMPLOYEE_NAME = EMP.EMPLOYEE_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#ASSET_TEMP AS ASSET_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_EMPLOYEE AS EMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET_TEMP.PHYINV_EMPLOYEE_CD = EMP.EMPLOYEE_CD")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 実棚ステータスを更新する
        ''' </summary>
        Private Function UpdatePhyinvStatus() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	  ASSET_TEMP.CUR_ASSET_STATUS = ASSET.ASSET_STATUS")
            sb.AppendLine("	 ,ASSET_TEMP.CUR_PHYINV_TS = ASSET.PHYINV_TS")
            sb.AppendLine("	 ,ASSET_TEMP.PHYINV_STATUS = SETT.KEY1")
            sb.AppendLine("FROM")
            sb.AppendLine("	#ASSET_TEMP AS ASSET_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_SETTING AS SETT")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET_TEMP.PHYINV_STATUS = SETT.KEY1")
            sb.AppendLine($"    AND KEY_DIV = '{CommonConst.PHYINV_STATUS_VALUE}'")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		T_INV_ASSET AS ASSET")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET_TEMP.ASSET_NO = ASSET.ASSET_NO")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 一時表から反映不要な現物なしデータを削除する
        ''' </summary>
        ''' <remarks>
        ''' 削除条件は「現物なし」AND
        ''' ・棚卸済み
        ''' ・未棚卸の貸出中
        ''' </remarks>
        Private Function DeletePhyinvData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("DELETE FROM ")
            sb.AppendLine("	#ASSET_TEMP")
            sb.AppendLine("WHERE")
            sb.AppendLine("(")
            sb.AppendLine($"      PHYINV_STATUS = '{CommonConst.PHYINV_STATUS_ERROR}'") '現物なし
            sb.AppendLine("	  AND CUR_PHYINV_TS IS NOT NULL")                           '実棚済み
            sb.AppendLine(") OR (")
            sb.AppendLine($"      PHYINV_STATUS = '{CommonConst.PHYINV_STATUS_ERROR}'") '現物なし
            sb.AppendLine("	  AND CUR_PHYINV_TS IS NULL")                               '実棚未実施
            sb.AppendLine($"  AND CUR_ASSET_STATUS = {CommonConst.ASSET_STATUS_LEASE}") '貸出中
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産棚卸テーブルに実棚した結果を反映する
        ''' </summary>
        Private Function UpdatePhyinvInfo() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET")
            sb.AppendLine("SET")
            sb.AppendLine("	 ASSET.ASSET_NO = ASSET_TEMP.ASSET_NO ")
            sb.AppendLine("	,ASSET.PHYINV_TS = ASSET_TEMP.PHYINV_TS ")
            sb.AppendLine("	,ASSET.PHYINV_STATUS = ASSET_TEMP.PHYINV_STATUS ")
            sb.AppendLine("	,ASSET.PHYINV_LOC_L_CD = CASE ISNULL(ASSET_TEMP.PHYINV_LOC_L_CD, '') WHEN '' THEN ' ' ELSE ASSET_TEMP.PHYINV_LOC_L_CD END ")
            sb.AppendLine("	,ASSET.PHYINV_LOC_M_CD = CASE ISNULL(ASSET_TEMP.PHYINV_LOC_M_CD, '') WHEN '' THEN ' ' ELSE ASSET_TEMP.PHYINV_LOC_M_CD END ")
            sb.AppendLine("	,ASSET.PHYINV_LOC_S_CD = CASE ISNULL(ASSET_TEMP.PHYINV_LOC_S_CD, '') WHEN '' THEN ' ' ELSE ASSET_TEMP.PHYINV_LOC_S_CD END ")
            sb.AppendLine("	,ASSET.PHYINV_LOC_L_NAME = ASSET_TEMP.PHYINV_LOC_L_NAME ")
            sb.AppendLine("	,ASSET.PHYINV_LOC_M_NAME = ASSET_TEMP.PHYINV_LOC_M_NAME ")
            sb.AppendLine("	,ASSET.PHYINV_LOC_S_NAME = ASSET_TEMP.PHYINV_LOC_S_NAME ")
            sb.AppendLine("	,ASSET.PHYINV_EMPLOYEE_CD = ASSET_TEMP.PHYINV_EMPLOYEE_CD ")
            sb.AppendLine("	,ASSET.PHYINV_EMPLOYEE_NAME = ASSET_TEMP.PHYINV_EMPLOYEE_NAME ")
            sb.AppendLine("	,ASSET.UPD_DATETIME = GETDATE()")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_CD = ISNULL(ASSET_TEMP.PHYINV_EMPLOYEE_CD, ' ') ")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_NAME = ISNULL(ASSET_TEMP.PHYINV_EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($",ASSET.UPD_TASK = '{TASK_NAME}'")
            sb.AppendLine("	,ASSET.CHANGE_LOC_M_CD = 
                                CASE 
                                    WHEN (ASSET.CHANGE_LOC_M_CD IS NULL OR ASSET.CHANGE_LOC_M_CD = '' OR ASSET.CHANGE_LOC_M_CD = ' ')
                                     AND (ASSET.CHANGE_LOC_S_CD IS NULL OR ASSET.CHANGE_LOC_S_CD = '' OR ASSET.CHANGE_LOC_S_CD = ' ')
                                    THEN ISNULL(NULLIF(ASSET_TEMP.PHYINV_LOC_M_CD, ''), ' ')
                                    ELSE ASSET.CHANGE_LOC_M_CD
                                END ")
            sb.AppendLine("	,ASSET.CHANGE_LOC_S_CD = 
                                CASE 
                                    WHEN (ASSET.CHANGE_LOC_M_CD IS NULL OR ASSET.CHANGE_LOC_M_CD = '' OR ASSET.CHANGE_LOC_M_CD = ' ')
                                     AND (ASSET.CHANGE_LOC_S_CD IS NULL OR ASSET.CHANGE_LOC_S_CD = '' OR ASSET.CHANGE_LOC_S_CD = ' ')
                                    THEN ISNULL(NULLIF(ASSET_TEMP.PHYINV_LOC_S_CD, ''), ' ')
                                    ELSE ASSET.CHANGE_LOC_S_CD
                                END ")
            '20260925 Y.Ono ver4.3.1要求事項7 Start
            sb.AppendLine(" ,ASSET.PHYINV_TOOL_DIV = ASSET_TEMP.PHYINV_TOOL_DIV")
            '20260925 Y.Ono ver4.3.1要求事項7 End
            sb.AppendLine("FROM")
            sb.AppendLine("	T_INV_ASSET AS ASSET")
            sb.AppendLine("	INNER JOIN #ASSET_TEMP AS ASSET_TEMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET.ASSET_NO = ASSET_TEMP.ASSET_NO")
            sb.AppendLine("WHERE")
            sb.AppendLine("	    (ASSET.PHYINV_TS <= ASSET_TEMP.PHYINV_TS")
            sb.AppendLine("	OR  ASSET.PHYINV_TS IS NULL)")
            sb.AppendLine("	AND  (ASSET.LOCK_EMPLOYEE_CD = ASSET_TEMP.PHYINV_EMPLOYEE_CD")
            sb.AppendLine("	OR  ASSET.LOCK_EMPLOYEE_CD IS NULL)")
            sb.AppendLine("	AND  ASSET.PHYINV_STATUS != 'N'")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産マスタから実棚した件数を反映する
        ''' </summary>
        Private Function UpdateInventoryNum(ByVal emp As M_EMPLOYEE, ByRef param As List(Of SqlParameter)) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	T_INV_DETAIL")
            sb.AppendLine("SET")
            sb.AppendLine($" LOGIC_INV_NUM = (SELECT COUNT(1) FROM M_ASSET WHERE PHYINV_STATUS <> '{CommonConst.PHYINV_STATUS_EXCLUDED}' AND M_ASSET.LOC_L_CD = T_INV_DETAIL.INV_LOC_L_CD) ")
            sb.AppendLine($",PHYINV_NUM = (SELECT COUNT(1) FROM M_ASSET WHERE PHYINV_STATUS = '{CommonConst.PHYINV_STATUS_NORMAL}' AND M_ASSET.LOC_L_CD = T_INV_DETAIL.INV_LOC_L_CD)")
            sb.AppendLine($",LOC_DIFF_NUM = (SELECT COUNT(1) FROM M_ASSET WHERE PHYINV_STATUS = '{CommonConst.PHYINV_STATUS_WARN}' AND M_ASSET.LOC_L_CD = T_INV_DETAIL.INV_LOC_L_CD)")
            sb.AppendLine($",NO_ITEM_NUM = (SELECT COUNT(1) FROM M_ASSET WHERE PHYINV_STATUS = '{CommonConst.PHYINV_STATUS_ERROR }' AND M_ASSET.LOC_L_CD = T_INV_DETAIL.INV_LOC_L_CD)")
            sb.AppendLine("	,UPD_DATETIME = GETDATE()")
            sb.AppendLine("	,UPD_EMPLOYEE_CD = @UpdEmployeeCd ")
            sb.AppendLine("	,UPD_EMPLOYEE_NAME = @UpdEmployeeName ")
            sb.AppendLine($",UPD_TASK = '{TASK_NAME}'")
            sb.AppendLine("FROM")
            sb.AppendLine("	T_INV_DETAIL INNER JOIN T_INVENTORY ON T_INV_DETAIL.INV_ID = T_INVENTORY.INV_ID ")
            sb.AppendLine("WHERE")
            sb.AppendLine("	INV_CONFIRM_TS IS NULL")

            'パラメータ設定
            If emp IsNot Nothing Then
                param.Add(New SqlParameter("@UpdEmployeeCd", emp.EMPLOYEE_CD))
                param.Add(New SqlParameter("@UpdEmployeeName", emp.EMPLOYEE_NAME))
            Else
                param.Add(New SqlParameter("@UpdEmployeeCd", " "))
                param.Add(New SqlParameter("@UpdEmployeeName", " "))
            End If

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ロックで更新できなかったデータがあるか調べる
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetNotUpdataByLock() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 ASSET.ASSET_NO AS assetNo ")
            sb.AppendLine("FROM")
            sb.AppendLine("  T_INV_ASSET AS ASSET")
            sb.AppendLine("  INNER JOIN #ASSET_TEMP AS ASSET_TEMP")
            sb.AppendLine("  ON ASSET.ASSET_NO = ASSET_TEMP.ASSET_NO")
            sb.AppendLine("WHERE (ASSET.PHYINV_TS < ASSET_TEMP.PHYINV_TS
                                OR ASSET.PHYINV_TS IS NULL)")
            sb.AppendLine("  AND (ASSET.LOCK_EMPLOYEE_CD IS NOT NULL 
                                AND ASSET.LOCK_EMPLOYEE_CD != ASSET_TEMP.PHYINV_EMPLOYEE_CD)")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

#End Region

    End Class
End Namespace