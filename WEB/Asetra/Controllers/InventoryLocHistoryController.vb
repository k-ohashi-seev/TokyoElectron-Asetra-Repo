Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers
    ''' <summary>
    ''' 棚卸実績ロケ別一覧画面表示用のクラス
    ''' </summary>
    Public Class InventoryLocHistoryController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス変数"
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _ViewModel As InventoryLocHistoryViewModel = New InventoryLocHistoryViewModel                         'InventoryHistory一覧表示用モデル
        Protected _viewResult As List(Of InventoryLocHistoryViewModel) = New List(Of InventoryLocHistoryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing

        Private _invId As String = ""

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
        Public Function Index(ByVal ViewModel As InventoryLocHistoryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_INVENTORY_HISTORY)
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As InventoryLocHistoryViewModel = Session(CommonSessionKey.CREAGE_INVENTORY_HISTORY)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else

                '当該棚卸ID
                _invId = Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY).Split("/")(1)

                '棚卸開始時間、当該棚卸ID、当該ロケ大コード、当該ロケ大コードとロケ名どれかがNULLの場合、元の画面に戻る
                If String.IsNullOrEmpty(_invId) Then
                    Return RedirectToAction("Index", "InventoryHistory")
                End If

                '--2021/11/22 add start nv iwamoto
                '権限情報の取得
                Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryLocHistory, _LoginInfo)
                If authInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                Else
                    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                End If
                '--2021/11/22 add end nv iwamoto

                '一覧表示処理
                MyBase.BaseIndex(_ViewModel)
                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.INVENTORY_LOC_HISTORY_TOP)
            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If
            colSetModel = _db.ColSet.Find(DispId.INVENTORY_LOC_HISTORY_BOTTOM)
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
                Session.Remove(CommonSessionKey.CREAGE_INVENTORY_HISTORY)
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
            Dim query As DbSqlQuery(Of InventoryLocHistoryViewModel) = _db.InventoryLocHistoryView.SqlQuery(_SQL, _paramSqlList.ToArray)
            '画面に表示するViewを設定
            _viewResult = query.ToList
        End Sub

#End Region

#Region "登録・更新処理"
        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim blnReturn As Boolean = True
            Return blnReturn
        End Function

        ''' <summary>
        ''' 登録処理をコンテキストに追加します
        ''' </summary>
        Protected Overrides Sub CreateQuery()
        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Return blnReturn
        End Function

        ''' <summary>
        ''' 更新処理のコンテキストを変更します
        ''' </summary>
        Protected Overrides Sub EditQuery()
        End Sub

#End Region

#Region "削除処理"

        ''' <summary>
        ''' 削除処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function DeleteChcek() As Boolean
            Return True
        End Function

        ''' <summary>
        ''' 削除処理のクエリを作成します
        ''' </summary>
        Protected Overrides Sub DeleteQuery()
        End Sub

#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ByVal ViewModel As InventoryLocHistoryViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '当該棚卸ID
            _invId = Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY).Split("/")(1)

            _ViewModel = ViewModel

            Dim InventoryLocHistoryData As List(Of InventoryLocHistoryViewModel)

            '棚卸実績一覧取得
            InventoryLocHistoryData = GetInventoryLocHistoryData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.InventoryLocHistory.KeyDiv, CommonSettingKey.InventoryLocHistory.Key1, CommonSettingKey.InventoryLocHistory.Key2, CommonSettingKey.InventoryLocHistory.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_INVENTORY_LOC_HISTORY)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_INVENTORY_LOC_HISTORY)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))

            'データ行挿入
            Dim col As String = ""
            For Each row As InventoryLocHistoryViewModel In InventoryLocHistoryData
                '棚卸開始宣言日時
                col = ""
                col += SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringLongYMDHMS(row.START_DEC_TS))

                '開始宣言者コード
                If row.START_DEC_EMPLOYEE_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.START_DEC_EMPLOYEE_CD.Trim)
                End If

                '開始宣言者
                If row.START_DEC_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.START_DEC_NAME.Trim)
                End If

                '棚卸名
                If row.INV_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.INV_NAME.Trim)
                End If

                '棚卸確定日時
                If row.INV_CONFIRM_TS Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringLongYMDHMS(row.INV_CONFIRM_TS))
                End If

                '棚卸確定者コード
                If row.INV_CONFIRM_EMPLOYEE_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.INV_CONFIRM_EMPLOYEE_CD.Trim)
                End If

                '棚卸確定者名
                If row.INV_CONFIRM_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.INV_CONFIRM_NAME.Trim)
                End If

                'ロケーション大区分
                If row.LOC_L_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_CD.Trim)
                End If

                'ロケーション大区分名
                If row.LOC_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_NAME.Trim)
                End If

                '帳簿在庫数
                If row.BOOK_INVENTORY_NUMBER Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.BOOK_INVENTORY_NUMBER)
                End If

                '貸出数
                If row.NUMBER_OF_LOANS_WITHIN Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.NUMBER_OF_LOANS_WITHIN)
                End If

                '実棚数
                If row.ACTUAL_NUMBER_OF_SHELVES Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ACTUAL_NUMBER_OF_SHELVES)
                End If

                '正常数
                If row.NORMAL_NUMBER Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.NORMAL_NUMBER)
                End If

                'ロケ違い数
                If row.LOCATION_DIFFERENCE_NUMBER Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOCATION_DIFFERENCE_NUMBER)
                End If

                '在庫なし数
                If row.OUT_OF_STOCK Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.OUT_OF_STOCK)
                End If

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     InventoryLocHistoryData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryLocHistoryController.LABEL_TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetInventoryLocHistoryData() As List(Of InventoryLocHistoryViewModel)
            Dim lstInventoryLocHistory As List(Of InventoryLocHistoryViewModel)
            Dim db As SystemDBContext = New SystemDBContext

            Dim query As DbSqlQuery(Of InventoryLocHistoryViewModel) = db.InventoryLocHistoryView.SqlQuery(CreateSQL(), _paramSqlList.ToArray)

            'SQLを実行する
            lstInventoryLocHistory = query.ToList
            'データを返却
            Return lstInventoryLocHistory
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
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String
            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            '棚卸管理Tから棚卸確定日時<>NULLのデータ取得
            '一覧表示表用SQL
            sbSQL.Clear()
            sbSQL.AppendLine("SELECT DISTINCT ")
            'ヘッダーエリア---------
            sbSQL.AppendLine("START_DEC_TS ")                       '棚卸宣言日時
            sbSQL.AppendLine(",START_DEC_EMPLOYEE_CD ")             '開始宣言者コード
            sbSQL.AppendLine(",START_DEC_NAME ")                    '開始宣言者
            sbSQL.AppendLine(",INV_NAME ")                          '棚卸名
            sbSQL.AppendLine(",INV.INV_ALL_CONFIRM_TS INV_CONFIRM_TS ") '棚卸確定日時
            sbSQL.AppendLine(",INV_CONFIRM_EMPLOYEE_CD ")           '棚卸確定者コード
            sbSQL.AppendLine(",INV_CONFIRM_NAME ")                  '棚卸確定者
            '-----------------------
            sbSQL.AppendLine(",ISNULL(ML.LOC_L_CD, ' ') AS LOC_L_CD ") 'ロケ大コード
            sbSQL.AppendLine(",ML.LOC_NAME ")                          'ロケ大名

            '-- 明細部　帳簿在庫数 SUM(保管、貸出)
            sbSQL.AppendLine("  ,CASE WHEN INV.START_DEC_TS IS NOT NULL THEN (SELECT COUNT_BIG(*) FROM T_INV_RESULT M_ASSET")
            sbSQL.AppendLine("          WHERE")
            sbSQL.AppendLine("              M_ASSET.LOC_L_CD = MAST.LOC_L_CD")
            sbSQL.AppendLine("          And M_ASSET.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ")")
            sbSQL.AppendLine($"         And M_ASSET.INV_ID = {_invId}")
            sbSQL.AppendLine("   ) ELSE 0 END AS BOOK_INVENTORY_NUMBER")

            '-- 明細部　内貸出数 SUM(貸出)
            sbSQL.AppendLine("  ,CASE WHEN INV.START_DEC_TS IS NOT NULL THEN (SELECT COUNT_BIG(*) FROM T_INV_RESULT M_ASSET")
            sbSQL.AppendLine("          WHERE")
            sbSQL.AppendLine("              M_ASSET.LOC_L_CD = MAST.LOC_L_CD")
            sbSQL.AppendLine("          And M_ASSET.ASSET_STATUS = " & ASSET_STATUS.Lending)
            sbSQL.AppendLine($"         And M_ASSET.INV_ID = {_invId}")
            sbSQL.AppendLine("  ) ELSE 0 END AS NUMBER_OF_LOANS_WITHIN")

            '-- 明細部　実棚数 SUM(正常、警告)
            sbSQL.AppendLine("  ,CASE WHEN INV.START_DEC_TS IS NOT NULL THEN (SELECT COUNT_BIG(*) FROM T_INV_RESULT M_ASSET")
            sbSQL.AppendLine("          WHERE")
            sbSQL.AppendLine("              M_ASSET.LOC_L_CD = MAST.LOC_L_CD")
            sbSQL.AppendLine("          And M_ASSET.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ")")
            sbSQL.AppendLine("          And M_ASSET.PHYINV_STATUS IN ('" & PHYINV_STATUS.Normal & "', '" & PHYINV_STATUS.Warning & "')")
            sbSQL.AppendLine($"         And M_ASSET.INV_ID = {_invId}")
            sbSQL.AppendLine("  )  ELSE 0 END AS ACTUAL_NUMBER_OF_SHELVES")

            '-- 明細部　正常数 SUM(正常)
            sbSQL.AppendLine("  ,CASE WHEN INV.START_DEC_TS IS NOT NULL THEN (SELECT COUNT_BIG(*) FROM T_INV_RESULT M_ASSET")
            sbSQL.AppendLine("          WHERE")
            sbSQL.AppendLine("              M_ASSET.LOC_L_CD = MAST.LOC_L_CD")
            sbSQL.AppendLine("          And M_ASSET.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ")")
            sbSQL.AppendLine("          And M_ASSET.PHYINV_STATUS = '" & PHYINV_STATUS.Normal & "'")
            sbSQL.AppendLine($"         And M_ASSET.INV_ID = {_invId}")
            sbSQL.AppendLine("  ) ELSE 0 END AS NORMAL_NUMBER")

            '-- 明細部　ロケ違い数 SUM(警告)
            sbSQL.AppendLine("  ,CASE WHEN INV.START_DEC_TS IS NOT NULL THEN (SELECT COUNT_BIG(*) FROM T_INV_RESULT M_ASSET")
            sbSQL.AppendLine("          WHERE")
            sbSQL.AppendLine("              M_ASSET.LOC_L_CD = MAST.LOC_L_CD")
            sbSQL.AppendLine("          And M_ASSET.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ")")
            sbSQL.AppendLine("          And M_ASSET.PHYINV_STATUS = '" & PHYINV_STATUS.Warning & "'")
            sbSQL.AppendLine($"         And M_ASSET.INV_ID = {_invId}")
            sbSQL.AppendLine("  ) ELSE 0 END AS LOCATION_DIFFERENCE_NUMBER")

            '-- 明細部　在庫なし数 SUM(現物なし(未処理を含む))
            sbSQL.AppendLine("  ,CASE WHEN INV.START_DEC_TS IS NOT NULL THEN (SELECT COUNT_BIG(*) FROM T_INV_RESULT M_ASSET")
            sbSQL.AppendLine("          WHERE")
            sbSQL.AppendLine("              M_ASSET.LOC_L_CD = MAST.LOC_L_CD")
            sbSQL.AppendLine("          And M_ASSET.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ")")
            sbSQL.AppendLine("          And M_ASSET.PHYINV_STATUS = '" & PHYINV_STATUS.NoSpot & "'")
            sbSQL.AppendLine($"         And M_ASSET.INV_ID = {_invId}")
            sbSQL.AppendLine("  ) ELSE 0 END AS OUT_OF_STOCK")

            sbSQL.AppendLine(",INV.UPD_DATETIME ")                  '最終登録更新日時
            sbSQL.AppendLine(",INV.UPD_EMPLOYEE_CD ")               '最終登録更新者コード
            sbSQL.AppendLine(",INV.UPD_EMPLOYEE_NAME ")             '最終登録更新者氏名
            sbSQL.AppendLine(",INV.UPD_TASK ")                      '最終登録更新者氏名
            'sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount  ")   '取得件数
            sbSQL.AppendLine(",TotalCount AS TotalCount ")

            sbSQL.AppendLine("FROM T_INVENTORY INV ")
            sbSQL.AppendLine("LEFT JOIN T_INV_DETAIL INVD ")
            sbSQL.AppendLine("ON  INVD.INV_ID = INV.INV_ID ")
            sbSQL.AppendLine("AND INVD.INV_ALL_CONFIRM_TS IS NOT NULL ")


            sbSQL.AppendLine("LEFT JOIN M_LOCATION ML ")
            sbSQL.AppendLine("ON ML.LOC_DIV = '" & LOC_DIV.LocationL & "'")
            sbSQL.AppendLine("AND ML.LOC_L_CD = INVD.INV_LOC_L_CD ")

            sbSQL.AppendLine("LEFT JOIN M_ASSET MAST ")
            sbSQL.AppendLine("ON INVD.INV_LOC_L_CD = MAST.LOC_L_CD ")

            '一覧の取得件数取得--------------------------------
            sbSQL.AppendLine("INNER JOIN (")
            sbSQL.AppendLine("  SELECT COUNT(*) AS totalCount")
            sbSQL.AppendLine("  FROM T_INVENTORY INV ")
            sbSQL.AppendLine("LEFT JOIN T_INV_DETAIL INVD ")
            sbSQL.AppendLine("ON  INVD.INV_ID = INV.INV_ID ")
            sbSQL.AppendLine("AND INVD.INV_ALL_CONFIRM_TS IS NOT NULL ")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("INV_CONFIRM_TS IS NOT NULL ")  '棚卸確定日時<>NULL
            sbSQL.AppendLine($" AND INV.INV_ID = {_invId} ")   '選択した棚卸ID
            sbSQL.AppendLine(") C ")
            sbSQL.AppendLine("ON  1 = 1")
            '--------------------------------------------------

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("INV_CONFIRM_TS IS NOT NULL ")  '棚卸確定日時<>NULL
            sbSQL.AppendLine($" AND INV.INV_ID = {_invId} ")   '選択した棚卸ID

            sbSQL.AppendLine("ORDER BY")
            sbSQL.AppendLine("LOC_L_CD ")

            Return sbSQL.ToString()
            '20231013 UPD End Shirakawa
        End Function
        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New InventoryLocHistoryViewModel())
            End If

            'ヘッダーエリア
            _viewResult.ElementAt(0).SHOW_START_DEC_TS = _viewResult.ElementAt(0).START_DEC_TS
            _viewResult.ElementAt(0).SHOW_START_DEC_NAME = _viewResult.ElementAt(0).START_DEC_NAME
            _viewResult.ElementAt(0).SHOW_INV_NAME = _viewResult.ElementAt(0).INV_NAME
            _viewResult.ElementAt(0).SHOW_INV_CONFIRM_TS = _viewResult.ElementAt(0).INV_CONFIRM_TS
            _viewResult.ElementAt(0).SHOW_INV_CONFIRM_NAME = _viewResult.ElementAt(0).INV_CONFIRM_NAME

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).CsvPath = _ViewModel.CsvPath
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).GridInfoTop = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_LOC_HISTORY_TOP)
            _viewResult.ElementAt(0).FixColCntTop = _db.ColSet.Find(DispId.INVENTORY_LOC_HISTORY_TOP).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfoBottom = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_LOC_HISTORY_BOTTOM)
            _viewResult.ElementAt(0).FixColCntBottom = _db.ColSet.Find(DispId.INVENTORY_LOC_HISTORY_BOTTOM).FIX_COL_CNT

        End Sub

#End Region

    End Class
End Namespace