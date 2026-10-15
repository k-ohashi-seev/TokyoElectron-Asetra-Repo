Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers
    ''' <summary>
    ''' ゲート持ち出し履歴画面表示用のクラス
    ''' </summary>
    Public Class GatePassLogController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)



#Region "クラス定数"

#End Region

#Region "クラス変数"
        Private _Model As GatePassLogModel = New GatePassLogModel                                 'GatePassLogモデル
        Private _ViewModel As GatePassLogViewModel = New GatePassLogViewModel                     'GatePassLog一覧表示用モデル
        Protected _viewResult As List(Of GatePassLogViewModel) = New List(Of GatePassLogViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
#End Region

#Region "一覧表示処理"
        ''' <summary>
        ''' アクションメソッド(Index)
        ''' スーパークラスの一覧表示処理を呼出します
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns>View表示内容</returns>
        <ValidateInput(False)>
        Public Function Index(ByVal ViewModel As GatePassLogViewModel, ByVal actionname As String) As ActionResult

            'モデルを設定(セッションにある場合はセッションから設定)
            _ViewModel = ViewModel

            '20220328 ADD Start Kawamoto v2.1
            '使用ゲート取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim settingGateDiv As String = Dao.ToolSetting.GetUseGate(Setting)
            _ViewModel.GATE_DIV = settingGateDiv
            '20220328 ADD End Kawamoto v2.1

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '20220328 ADD Start Kawamoto v2.1
                ' 期間の初期値(本日)
                If String.IsNullOrEmpty(ViewModel.SearchGatePassDtFrom) Then
                    ViewModel.SearchGatePassDtFrom = System.DateTime.Now.ToString("yyyy-MM-dd")
                End If
                If String.IsNullOrEmpty(ViewModel.SearchGatePassDtTo) Then
                    ViewModel.SearchGatePassDtTo = System.DateTime.Now.ToString("yyyy-MM-dd")
                End If
                '20220328 ADD End Kawamoto v2.1

                '一覧表示処理
                MyBase.BaseIndex(_ViewModel)

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            'コンボボックス用ゲートマスタ取得処理
            DropDownListCreate()

            Dim gateId As String = DispId.GATE_PASS_LOG_WAVE_GATE
            If settingGateDiv = GateDiv.Cd.SW Then
                gateId = DispId.GATE_PASS_LOG_SPEED_WAY
            End If

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(gateId)
            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If

            '一覧情報にページ情報を設定
            CopyModelData()

            '結果表示
            Return View(_viewResult)

        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()

            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                'Session.Remove(CommonSessionKey.CREAGE_DEPARTMENT) 'ゲート持ち出し履歴画面はセッション必要なし
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            'クエリを返却
            MyBase._SQL = CreateSQL()

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of GatePassLogViewModel) = _db.GatePassLogView.SqlQuery(_SQL, _paramSqlList.ToArray)

            '画面に表示するViewを設定
            _viewResult = query.ToList

        End Sub

#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ViewModel As GatePassLogViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            Dim GateData As List(Of GatePassLogViewModel)

            'ゲートマスタデータ取得
            GateData = GetListData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim objSetting As SettingInfo = Setting.getSettingInfo(CommonSettingKey.GatePassLog.KeyDiv, CommonSettingKey.GatePassLog.Key1, CommonSettingKey.GatePassLog.Key2, CommonSettingKey.GatePassLog.Key3, True)
            Dim sFileNm As String = objSetting.DATA1

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆CSVファイル出力先設定
            Dim sUrlFileName As String = Path.GetRandomFileName()
            Dim sDspFileName As String = String.Format(sFileNm + "_{0}.csv", Now.ToString("yyyyMMdd"))
            SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sUrlFileName)

            '◆CSV出力用リスト作成
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'CSVヘッダー取得
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_GATE_PASS_LOG)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_GATE_PASS_LOG)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As GatePassLogViewModel In GateData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(row.GATE_NAME)
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.GATE_PASS_DT) & " " & CommonRfid.ToStringLongHM(row.GATE_PASS_TM))
                Dim sGatePassDt As String = String.Empty
                If Not row.PROCESS_DATETIME Is Nothing Then
                    sGatePassDt = CommonRfid.ToStringLongYMDHMSF(row.PROCESS_DATETIME)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sGatePassDt)
                Select Case row.GATE_PASS_DIV
                    Case GatePassDiv.Cd.ER
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_ER))
                    Case GatePassDiv.Cd.IO
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_IO))
                    Case GatePassDiv.Cd.LR
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_LR))
                    Case GatePassDiv.Cd.SE
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_GATE_PASS_DIV_SE))          '20220328 ADD Kawamoto v2.1
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PRODUCT_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ERR_CONTENTS)             '20220328 ADD Kawamoto v2.1

                col += ""
                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                'ファイルをダウンロードさせる
                Call FileDownload(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)

                'ダウンロードさせた後、ファイルを削除。
                System.IO.File.Delete(sFullPath + "\" + sUrlFileName)

                ''10025,INF,,,%1が完了しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 10025, "CSVファイルの出力")
            Else
                ''30001,CRT,,,%1に失敗しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelGatePassLog.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     GateData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelGatePassLog.TITLE))


            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetListData() As List(Of GatePassLogViewModel)
            Dim lstGatePassLog As List(Of GatePassLogViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstGatePassLog = db.GatePassLogView.SqlQuery(CreateSQL(), _paramSqlList.ToArray).ToList

            'データを返却
            Return lstGatePassLog

        End Function
#End Region

#Region "ファイルをダウンロードさせます。"
        ''' <summary>
        ''' ファイルをダウンロードさせます。
        ''' </summary>
        ''' <param name="FileName">ダウンロードするファイル名を指定します。</param>
        ''' <param name="MimeType">ダウンロードするファイルのMIMEタイプを指定します。</param>
        ''' <param name="sFileFullPath">ダウンロードするファイルのフルパス</param>
        Private Sub FileDownload(FileName As String, MimeType As String, ByVal sFileFullPath As String)
            'ダウンロードするファイル名
            Dim dlFileName As String

            'ファイル名が日本語の場合を考慮したダウンロードファイル名を作成
            If Request.Browser.Browser = "IE" Then
                'IEの場合はファイル名をURLエンコード
                dlFileName = HttpUtility.UrlEncode(FileName)
            Else
                'IE以外の場合はそのままでOK
                dlFileName = FileName
            End If

            'ダウンロード処理
            'Response情報クリア
            Response.ClearContent()

            'バッファリング
            Response.Buffer = True

            'HTTPヘッダー情報・MIMEタイプ設定
            Response.AddHeader("Content-Disposition", String.Format("attachment;filename={0}", dlFileName))
            Response.ContentType = MimeType

            'ファイルを書き出し
            Response.WriteFile(sFileFullPath)

            Response.Flush()
            Response.End()
        End Sub
#End Region

#Region "コンボボックス用ゲートマスタ取得処理"
        ''' <summary>
        ''' コンボボックス用にゲートマスタを取得します
        ''' </summary>
        Public Sub DropDownListCreate()

            Dim lstGate As List(Of GateModel)     'ゲートリスト
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext
            'M_GATEテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT *")
            sbSQL.AppendLine("FROM M_GATE")
            sbSQL.AppendLine($"WHERE GATE_DIV = '{_ViewModel.GATE_DIV}'")            '20220328 ADD Kawamoto v2.1
            sbSQL.AppendLine("ORDER BY GATE_CD")
            'SQLを実行する
            lstGate = db.Gate.SqlQuery(sbSQL.ToString).ToList
            '一覧画面のゲートコンボ設定
            If String.IsNullOrEmpty(_ViewModel.SearchCode) Then
                ViewBag.gate = New SelectList(lstGate, "GATE_CD", "GATE_NAME", String.Empty)
            Else
                ViewBag.gate = New SelectList(lstGate, "GATE_CD", "GATE_NAME", _ViewModel.SearchCode)
            End If

        End Sub
#End Region

#Region "一覧データ取得用SQLを作成する"
        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String
            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  GATE_LOG_ID ")
            sbSQL.AppendLine("  , GATE_PASS_DT ")
            sbSQL.AppendLine("  , GATE_PASS_TM ")
            sbSQL.AppendLine(",CAST(GATE_PASS_DT AS VARCHAR(8)) + GATE_PASS_TM PROCESS_DATETIME")
            sbSQL.AppendLine("  , GATE_CD ")
            sbSQL.AppendLine("  , GATE_NAME ")
            sbSQL.AppendLine("  , GATE_PASS_DIV ")

            '2020/03/16 出力しないので、コメント化　DELETE Inoue 生かす時は、NULLがあることを考慮すること
            'sbSQL.AppendLine("  , ANTENNA_NO ")
            'sbSQL.AppendLine("  , THERMO_NO ")

            sbSQL.AppendLine("  , ASSET_NO ")
            sbSQL.AppendLine("  , ASSET_ID ")
            sbSQL.AppendLine("  , ASSET_NAME ")
            sbSQL.AppendLine("  , MAKER ")
            sbSQL.AppendLine("  , PRODUCT_NO ")
            sbSQL.AppendLine("  , MODEL_NO ")
            sbSQL.AppendLine("  , UPD_DATETIME ")
            sbSQL.AppendLine("  , UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , UPD_TASK ")
            sbSQL.AppendLine("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("  , ERR_CONTENTS ")           '20220328 ADD Kawamoto v2.1
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  T_GATE_PASS_LOG ")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '検索条件を設定する
            'ゲートコード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCode) Then
                sbSQL.AppendLine("  AND GATE_CD Like @SearchCode")
                addSqlParam("SearchCode", $"{_ViewModel.SearchCode}%")
            End If

            '資産番号
            If Not String.IsNullOrEmpty(_ViewModel.SearchAssetNo) Then
                sbSQL.AppendLine("  AND ASSET_NO Like @SearchAssetNo")
                addSqlParam("SearchAssetNo", $"{_ViewModel.SearchAssetNo}%")
            End If

            '20220328 UPD Start Kawamoto v2.1
            '資産名称
            If Not String.IsNullOrEmpty(_ViewModel.SearchAssetName) Then
                sbSQL.AppendLine("  AND ASSET_NAME Like @SearchAssetName")
                addSqlParam("SearchAssetName", $"%{_ViewModel.SearchAssetName}%")
            End If
            '通過日From
            'If (Not IsNothing(_ViewModel.SearchGatePassDtFrom)) Then
            '    sbSQL.AppendLine("  AND GATE_PASS_DT >= " & "@SearchGatePassDtFrom")
            '    addSqlParam("SearchGatePassDtFrom", $"{CDate(_ViewModel.SearchGatePassDtFrom).ToString("yyyyMMdd")}")
            'End If
            If Not IsNothing(_ViewModel.SearchGatePassDtFrom) AndAlso _ViewModel.SearchGatePassDtFrom <> "" Then
                sbSQL.AppendLine("  AND GATE_PASS_DT >= " & "@SearchGatePassDtFrom")
                addSqlParam("SearchGatePassDtFrom", Replace(_ViewModel.SearchGatePassDtFrom, "-", ""))
            End If

            '通過日To
            'If (Not IsNothing(_ViewModel.SearchGatePassDtTo)) Then
            '    sbSQL.AppendLine("  AND GATE_PASS_DT <= " & "@SearchGatePassDtTo")
            '    addSqlParam("SearchGatePassDtTo", $"{CDate(_ViewModel.SearchGatePassDtTo).ToString("yyyyMMdd")}")
            'End If
            If Not IsNothing(_ViewModel.SearchGatePassDtTo) AndAlso _ViewModel.SearchGatePassDtTo <> "" Then
                sbSQL.AppendLine("  AND GATE_PASS_DT <= " & "@SearchGatePassDtTo")
                addSqlParam("SearchGatePassDtTo", Replace(_ViewModel.SearchGatePassDtTo, "-", ""))
            End If
            '20220328 UPD End Kawamoto v2.1

            Dim sbSQLCheck As StringBuilder = New StringBuilder
            'ゲート通過区分　持出/返却エラー
            If _ViewModel.SearchGatePassDivER Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & GatePassDiv.Cd.ER & "'")
            End If

            If _ViewModel.GATE_DIV = GateDiv.Cd.WG Then
                'WaveGate
                'ゲート通過区分　入退室
                If _ViewModel.SearchGatePassDivIO Then
                    If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                    sbSQLCheck.Append("'" & GatePassDiv.Cd.IO & "'")
                End If
            End If

            '20220328 ADD Start Kawamoto v2.1
            'ゲート通過区分　システムエラー
            If _ViewModel.SearchGatePassDivSE Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & GatePassDiv.Cd.SE & "'")
            End If
            '20220328 ADD End Kawamoto v2.1

            'ゲート通過区分　持出/返却
            If _ViewModel.SearchGatePassDivLR Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & GatePassDiv.Cd.LR & "'")
            End If
            'SQL連結
            If sbSQLCheck.Length <> 0 Then
                sbSQL.Append(" AND GATE_PASS_DIV IN(" & sbSQLCheck.ToString & ") ")
            End If

            sbSQL.AppendLine($"  AND GATE_DIV = '{_ViewModel.GATE_DIV}'")           '20220328 ADD Kawamoto v2.1

            sbSQL.AppendLine(" ORDER BY GATE_PASS_DT DESC")
            sbSQL.AppendLine("          ,GATE_PASS_TM DESC")
            sbSQL.AppendLine("          ,GATE_NAME")
            sbSQL.AppendLine("          ,ASSET_NO")

            Return sbSQL.ToString()
        End Function
#End Region

#Region "CSVヘッダー取得"
        ''' <summary>
        ''' プリンタ情報取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCSVHeader(prmFunctionId As String) As String
            Dim csvHeader As String = String.Empty
            Dim wordList As New List(Of String)
            Dim csvHeaderWord As List(Of String) = Dao.ToolCSVHeader.GetCSVHeaderOrder(prmFunctionId)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            For Each word As String In csvHeaderWord
                wordList.Add(Me._Message.getMessage(word))
            Next

            csvHeader = String.Join(",", wordList)

            Return csvHeader
        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New GatePassLogViewModel())
            End If

            '使用ゲート取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim settingGateDiv As String = Dao.ToolSetting.GetUseGate(Setting)

            Dim gateId As String = DispId.GATE_PASS_LOG_WAVE_GATE
            If settingGateDiv = GateDiv.Cd.SW Then
                gateId = DispId.GATE_PASS_LOG_SPEED_WAY
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchCode = _ViewModel.SearchCode
            _viewResult.ElementAt(0).SearchAssetNo = _ViewModel.SearchAssetNo
            _viewResult.ElementAt(0).SearchGatePassDtFrom = _ViewModel.SearchGatePassDtFrom
            _viewResult.ElementAt(0).SearchGatePassDtTo = _ViewModel.SearchGatePassDtTo
            _viewResult.ElementAt(0).SearchGatePassDivER = _ViewModel.SearchGatePassDivER
            _viewResult.ElementAt(0).SearchGatePassDivIO = _ViewModel.SearchGatePassDivIO
            _viewResult.ElementAt(0).SearchGatePassDivLR = _ViewModel.SearchGatePassDivLR
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            '20220328 ADD Start Kawamoto v2.1
            _viewResult.ElementAt(0).GATE_DIV = _ViewModel.GATE_DIV
            _viewResult.ElementAt(0).SearchGatePassDivSE = _ViewModel.SearchGatePassDivSE
            _viewResult.ElementAt(0).SearchAssetName = _ViewModel.SearchAssetName
            '20220328 ADD End Kawamoto v2.1
            _viewResult.ElementAt(0).GridInfo = GetGridInfo(_db)
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(gateId).FIX_COL_CNT
        End Sub

        '''' <summary>
        '''' Viewモデルを更新用のモデルにコピーする
        '''' </summary>
        'Private Sub CopyViewModeltoModel(ByVal model As GatePassLogViewModel)
        '    _Model.DEPARTMENT_CD = model.DEPARTMENT_CD
        '    _Model.DEPARTMENT_NAME = model.DEPARTMENT_NAME
        '    _Model.INS_DATETIME = model.INS_DATETIME
        '    _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
        '    _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
        '    _Model.UPD_DATETIME = model.UPD_DATETIME
        '    _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
        '    _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
        '    _Model.UPD_TASK = model.UPD_TASK
        'End Sub
#End Region

#Region "項目配置情報を取得"
        ''' <summary>
        ''' 項目配置情報を取得
        ''' </summary>
        ''' <param name="db">DBコンテキスト</param>
        ''' <param name="dispId">画面ID</param>
        ''' <returns></returns>
        Public Shared Function GetGridInfo(ByRef db As SystemDBContext) As Dictionary(Of String, SlickGridInfo)
            '項目配置詳細マスタを取得
            Dim sbSQL As New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("  DISP_ID")
            sbSQL.AppendLine(", COLUMN_NAME")
            sbSQL.AppendLine(", DISP_ORDER")
            sbSQL.AppendLine(", TOOL_DIV")
            sbSQL.AppendLine(", WORD_DIV")
            sbSQL.AppendLine(", WORD_CD")
            sbSQL.AppendLine(", DISP_FLAG")
            sbSQL.AppendLine(", INS_DATETIME")
            sbSQL.AppendLine(", INS_EMPLOYEE_CD")
            sbSQL.AppendLine(", INS_EMPLOYEE_NAME")
            sbSQL.AppendLine(", UPD_DATETIME")
            sbSQL.AppendLine(", UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(", UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(", UPD_TASK")
            sbSQL.AppendLine("FROM M_COL_SET_DETAIL")
            sbSQL.AppendLine("WHERE DISP_ID = @DISP_ID")
            sbSQL.AppendLine("OR DISP_ID = @DISP_ID2")

            params.Add(New SqlParameter($"@DISP_ID", DispId.GATE_PASS_LOG_WAVE_GATE))
            params.Add(New SqlParameter($"@DISP_ID2", DispId.GATE_PASS_LOG_SPEED_WAY))

            Dim colSetDetail As List(Of ColSetDetailModel) = db.ColSetDetail.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            'Viewに渡す用に変換
            Dim dictGridInfo As New Dictionary(Of String, SlickGridInfo)
            For Each row In colSetDetail
                If Not dictGridInfo.ContainsKey(row.COLUMN_NAME) Then
                    'Key:DB項目名 Value:Grid表示情報
                    dictGridInfo.Add(row.COLUMN_NAME, New SlickGridInfo With {.DispOrder = row.DISP_ORDER, .DispFlag = row.DISP_FLAG})
                End If
            Next

            Return dictGridInfo
        End Function
#End Region

        Protected Overrides Sub CreateQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub DeleteQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub EditQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Function CreateChcek() As Boolean
            Throw New NotImplementedException()
        End Function

        Protected Overrides Function DeleteChcek() As Boolean
            Throw New NotImplementedException()
        End Function

        Protected Overrides Function EditChcek() As Boolean
            Throw New NotImplementedException()
        End Function

    End Class
End Namespace