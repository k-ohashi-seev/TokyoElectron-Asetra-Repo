Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports System.IO
Imports System.Web.WebPages
Imports Asetra.Dao
Imports Asetra.Misc
Imports Asetra.Models

Namespace Controllers

    ''' <summary>
    ''' 在庫照会画面表示用のクラス
    ''' </summary>
    Public Class StockListController
        Inherits PageBaseController

#Region "クラス変数"
        Private _Model As StockListModel = New StockListModel                                   'StockListモデル
        Private _ViewModel As StockListViewModel = New StockListViewModel                       'StockList一覧表示用モデル
        Protected _viewResult As List(Of StockListViewModel) = New List(Of StockListViewModel)  '結果View
        Private _Message As MessageRfid = Nothing
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _isSort As Boolean = True                       'ソートフラグ
        Private _AuthInfo As AuthInfo = Nothing                 '権限情報

        Private _LocationLList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationMList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationSList As List(Of LocationDropdownModel)     'ロケーションマスタリスト

        Private _CategoryLList As List(Of CategoryDropdownModel)     'カテゴリマスタリスト
        Private _CategoryMList As List(Of CategoryDropdownModel)     'カテゴリマスタリスト
        Private _CategorySList As List(Of CategoryDropdownModel)     'カテゴリマスタリスト
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As StockListViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(RedirectAssetListInfo.ID) IsNot Nothing Then
                Session.Remove(RedirectAssetListInfo.ID)
            End If

            If Session(CommonSessionKey.SEARCH_FLG) IsNot Nothing Then
                Session.Remove(CommonSessionKey.SEARCH_FLG)
            End If

            ''リダイレクト情報作成
            'Dim info As New RedirectAssetListInfo
            'info.RedirectPage = RedirectAssetListInfo.RedirectPageType.Menu
            'info.KeyValue = String.Empty

            'Session(RedirectAssetListInfo.ID) = info

            Return RedirectToAction("Index")
        End Function
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
        Public Function Index(ByVal ViewModel As StockListViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_STOCK_LIST)
            End If

            '上限確認フラグ 初期化
            ViewBag.isLimitFlg = False
            '一覧件数　初期化
            ViewBag.cntList = 0
            '設定マスタ上限件数　初期化
            ViewBag.cntLimit = 0
            '一覧表示フラグ
            Dim isDisp As Boolean = True

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As StockListViewModel = Session(CommonSessionKey.CREAGE_STOCK_LIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '検索ボタンがクリックされた場合
            If Me._Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session(CommonSessionKey.SEARCH_FLG) = True
            End If

            '資産一覧画面から戻ってきたときの処理
            Dim info As RedirectAssetListInfo = Session(RedirectAssetListInfo.ID)
            If info IsNot Nothing Then
                If info.RedirectPage = RedirectAssetListInfo.RedirectPageType.StockList Then
                    '履歴照会画面から戻ったときの処理
                    Dim senimaeModel As StockListViewModel = Session(CommonSessionKey.CREAGE_STOCK_SERCH_KEY)
                    _ViewModel = senimaeModel
                    Session.Remove(CommonSessionKey.CREAGE_STOCK_SERCH_KEY)

                    '遷移元情報セッションクリア
                    Session(RedirectAssetListInfo.ID) = Nothing
                End If
            End If

            '設定情報(ロケ使用区分)取得         Add 2020.03.16 Iwamoto
            Dim settingInfo As MessageRfid                      'メッセージ兼設定リスト
            Dim modelLocationUsage As New LocationUsageModel    'ロケーション使用区分
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim setting As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            If setting Is Nothing Then
                Throw New Exception(String.Format(settingInfo.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            ViewBag.LocUsage_L = setting.DATA1
            ViewBag.LocUsage_M = setting.DATA2
            ViewBag.LocUsage_S = setting.DATA3

            '20230901 ADD Start Shirakawa
            '設定マスタ(web読取データ)取得

            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Stock_List.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Stock_List.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Stock_List.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Stock_List.Key3)
            '20230901 ADD End Shirakawa

            '集約表示(ロケーション)初期値設定  Add 2020.03.16 Iwamoto
            If String.IsNullOrEmpty(_ViewModel.SearchDispSelectLocation) Then
                Dim defaultDispSelect As String = DISP_SELECT.S
                If ViewBag.LocUsage_S <> Misc.ItemUse.Yes Then
                    defaultDispSelect = DISP_SELECT.M
                End If
                If ViewBag.LocUsage_M <> Misc.ItemUse.Yes Then
                    defaultDispSelect = DISP_SELECT.L
                End If
                _ViewModel.SearchDispSelectLocation = defaultDispSelect
            End If

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
                ViewBag.modelLocL = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                ViewBag.modelLocM = New MultiSelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME")
                ViewBag.modelLocS = New MultiSelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME")
                ViewBag.modelCatL = New MultiSelectList(New List(Of CategoryDropdownModel), "CAT_CD", "CAT_NAME")
                CopyModelData()
                Return View(_viewResult)
            Else
                If _ViewModel.SearchDispSelectLocation = "" Then
                    _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationS
                End If

                If _ViewModel.SearchDispSelectCategory = "" Then
                    _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryS
                End If

                '権限情報の取得
                _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.StockList, _LoginInfo)

                '検索条件の復元有無
                _ViewModel.IsRestoreOwnSearchConditions = True

                '20230904 UPD Start Shirakawa
                '件数確認用データのためソートしない
                _isSort = False
                'セッションSEARCH_FLGがNothingの場合
                If Session(CommonSessionKey.SEARCH_FLG) Is Nothing Then
                    '初期表示

                    '初回表示フラグ True
                    _ViewModel.FirstSearch = True
                    '表示しない
                    isDisp = False

                    '設定マスタから取得できた場合 かつ 初期表示フラグ(Data1)がある場合 かつ 設定マスタ(web読取データ)の初期表示Nの場合
                    If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA1) = False AndAlso WebReadData.DATA1 = RedeDataFlg.No Then
                        '初回表示フラグ False
                        _ViewModel.FirstSearch = False
                    End If
                Else
                    '検索ボタンから表示
                    '初回表示フラグ False
                    _ViewModel.FirstSearch = False
                    '表示する
                    MyBase.BaseIndex(_ViewModel, False)

                End If

                '20230904 UPD End Shirakawa

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            '20230901 ADD Start Shirakawa
            '設定マスタから取得できた場合 かつ 上限値(Data2)が存在している場合 かつ 上限を超えていた場合
            If _viewResult.Count > 0 AndAlso WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA2) = False AndAlso WebReadData.DATA2 < _viewResult.ElementAt(0).TotalCount Then

                '上限確認フラグ
                ViewBag.isLimitFlg = True
                '一覧データ取得件数
                ViewBag.cntList = _viewResult.ElementAt(0).TotalCount
                '設定マスタ上限件数　初期化
                ViewBag.cntLimit = WebReadData.DATA2

                '読取データをクリア
                _viewResult.Clear()
            ElseIf isDisp Then
                'ソートする
                _isSort = True
                '一覧データ取得
                MyBase.BaseIndex(_ViewModel)
            End If

            ' (ロケーション)大区分ドロップダウンリスト作成
            GetLocationLData()
            '[CC431] UPD Start 20261007
            ' (ロケーション)中区分・小区分（資産Mに存在するもの。場所から選ぶの ON／OFF で連動）
            SetSearchLocationOptions(Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.StockList, _LoginInfo))
            '[CC431] UPD End 20261007

            ' (カテゴリ)大区分ドロップダウンリスト作成
            GetCategoryLData()

            '一覧情報にページ情報を設定
            CopyModelData()

            ' 資産一覧に渡すキー値を設定
            SettingKeyData()

            '結果表示
            Return View(_viewResult)

        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()

            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_STOCK_LIST)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            'クエリを返却
            MyBase._SQL = CreateSQL(_isSort)
        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of StockListViewModel) = _db.StockListView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function CsvExport(ByVal ViewModel As StockListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '上限確認フラグ 初期化
            ViewBag.isLimitFlg = False
            '一覧件数　初期化
            ViewBag.cntList = 0
            '設定マスタ上限件数　初期化
            ViewBag.cntLimit = 0

            '20230901 ADD Start Shirakawa
            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid                      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Stock_List.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Stock_List.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Stock_List.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Stock_List.Key3)
            '20230901 ADD End Shirakawa

            _ViewModel = ViewModel

            Dim StockListData As List(Of StockListViewModel)

            '在庫データ取得
            StockListData = GetStockListData()

            '20230901 ADD Start Shirakawa
            '設定マスタから取得できた場合
            '設定マスタから取得できた場合 かつ 上限値(Data2)が存在している場合 かつ 上限を超えていた場合
            If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA2) = False AndAlso WebReadData.DATA2 < StockListData.Count Then
                '上限を超えていた場合　CSV出力しない

                '上限確認フラグ
                ViewBag.isLimitFlg = True
                '一覧データ取得件数
                ViewBag.cntList = StockListData.Count
                '設定マスタ上限件数　初期化
                ViewBag.cntLimit = WebReadData.DATA2

                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '20230901 ADD End Shirakawa

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.StockList.KeyDiv, CommonSettingKey.StockList.Key1, CommonSettingKey.StockList.Key2, CommonSettingKey.StockList.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_STOCK_LIST)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_STOCK_LIST)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))

            'データ行挿入
            Dim col As String = ""
            For Each row As StockListViewModel In StockListData
                col = ""

                '2021/10/09 nv Inoue add start
                If row.MNG_LOC_L_CD Is Nothing Then
                    col += SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD.Trim)
                End If
                If row.MNG_LOC_L_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_NAME.Trim)
                End If
                '2021/10/09 nv Inoue add end

                If row.LOC_L_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_CD.Trim)
                End If

                If row.LOC_M_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_CD.Trim)
                End If

                If row.LOC_S_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_CD.Trim)
                End If

                If row.LOC_L_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_NAME)
                End If

                If row.LOC_M_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_NAME)
                End If

                If row.LOC_S_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_NAME)
                End If

                If row.CAT_L_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_CD.Trim)
                End If

                If row.CAT_M_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_CD.Trim)
                End If

                If row.CAT_S_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_CD.Trim)
                End If

                If row.CAT_L_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_NAME)
                End If

                If row.CAT_M_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_NAME)
                End If

                If row.CAT_S_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_NAME)
                End If

                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.QUANTITY.ToString)


                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                'ファイルをダウンロードさせる
                Call FileDownload(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)
                'Return File(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)

                'ダウンロードさせた後、ファイルを削除。
                System.IO.File.Delete(sFullPath + "\" + sUrlFileName)
                'Response.Redirect("index")

                ''10025,INF,,,%1が完了しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 10025, "CSVファイルの出力")
            Else
                ''30001,CRT,,,%1に失敗しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelStockList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     StockListData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelStockList.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetStockListData() As List(Of StockListViewModel)
            Dim lstStockList As List(Of StockListViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstStockList = db.StockListView.SqlQuery(CreateSQL(), _paramSqlList.ToArray).ToList
            'データを返却
            Return lstStockList
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

            'ダウンロード完了検知用にクッキーを設定
            Response.Cookies.Add(New HttpCookie("fileDownloadToken", "done") With {.Path = "/"})

            'HTTPヘッダー情報・MIMEタイプ設定
            Response.AddHeader("Content-Disposition", String.Format("attachment;filename={0}", dlFileName))
            Response.ContentType = MimeType

            'ファイルを書き出し
            Response.WriteFile(sFileFullPath)

            Response.Flush()
            Response.End()
        End Sub
#End Region

#Region "ロケ区分ドロップダウン変更処理"
        ''' <summary>
        ''' ロケ区分ドロップダウン変更処理
        ''' </summary>
        ''' <param name="locationLevel"></param>
        ''' <param name="selectLCode"></param>
        ''' <param name="selectMCode"></param>
        ''' <returns></returns>
        Public Function ChangeLocation(ByVal locationLevel As String, ByVal selectLCode As String, ByVal selectMCode As String) As ActionResult
            Dim settingInfo As MessageRfid      'メッセージ兼設定リスト
            Dim modelLocationUsage As New LocationUsageModel        'ロケーション使用区分
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim setting As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingInfo.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            modelLocationUsage.LocUsage_L = setting.DATA1
            modelLocationUsage.LocUsage_M = setting.DATA2
            modelLocationUsage.LocUsage_S = setting.DATA3

            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            End If

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '権限情報の取得
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.StockList, _LoginInfo)

            If locationLevel = LOC_DIV.LocationM Then
                If modelLocationUsage.LocUsage_M = Misc.ItemUse.Yes Then
                    Return GetLocationMData(selectLCode)
                Else
                    '中区分 使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            Else

                If modelLocationUsage.LocUsage_S = Misc.ItemUse.Yes Then
                    '中区分は複数選択のため、「,」区切りで複数のコードが渡される
                    Return GetLocationSData(selectLCode, CommonRfid.GetSelectedValues(CommonRfid.ToStringNvl(selectMCode).Split(","c)))
                Else
                    '小区分　使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            End If
            'Upd End 2020.03.16 iwamoto

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

        '[CC431] ADD Start 20261007
#Region "検索条件のロケーション（原価センタ・場所）の選択肢"
        ''' <summary>
        ''' 検索条件のロケーションの選択肢を返す（画面の ajax から呼ぶ）
        ''' </summary>
        ''' <param name="selectLCode">会社（大区分コード）</param>
        ''' <param name="level">返す側（M：中区分、S：小区分）</param>
        ''' <param name="reverse">場所から選ぶ（True：場所で原価センタを絞り込む、False：原価センタで場所を絞り込む）</param>
        ''' <param name="codes">絞り込みに使う、反対側の選択中のコード（「,」区切り）</param>
        ''' <returns>選択肢（LOC_CD／LOC_NAME のリスト）</returns>
        Public Function GetSearchLocationOptions(ByVal selectLCode As String, ByVal level As String, ByVal reverse As Boolean, ByVal codes As String) As ActionResult
            'ログインセッションが切れていたら一覧画面に遷移する（一覧画面でエラーメッセージを表示する）
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '設定情報(ロケ使用区分)取得
            Dim settingRfid As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)               'メッセージ兼設定リスト
            Dim setting As SettingInfo = settingRfid.getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingRfid.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            '中区分・小区分を使用しない場合、Nothing を返す（ChangeLocation と同じ）
            If (level = LOC_DIV.LocationM AndAlso setting.DATA2 <> Misc.ItemUse.Yes) OrElse
               (level = LOC_DIV.LocationS AndAlso setting.DATA3 <> Misc.ItemUse.Yes) Then
                Return Nothing
            End If

            '権限はサーバー側で取得する
            Me._Message = settingRfid
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.StockList, _LoginInfo)

            Return Json(GetSearchLocationList(level, reverse, CommonRfid.ToStringNvl(codes).Split(","c), selectLCode, authInfo))

        End Function

        ''' <summary>
        ''' 検索条件のロケーション（原価センタ・場所）の選択肢を、場所から選ぶの ON／OFF と選択値に合わせて作る
        ''' </summary>
        ''' <param name="authInfo">権限情報</param>
        Private Sub SetSearchLocationOptions(authInfo As AuthInfo)
            Dim selLocM As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocM)
            Dim selLocS As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocS)

            '一覧画面のロケーションコンボ設定（複数選択のため MultiSelectList）
            ViewBag.modelLocM = New MultiSelectList(
                GetSearchLocationList(LOC_DIV.LocationM, _ViewModel.SearchLocReverse, selLocS, _ViewModel.SearchLocL, authInfo),
                "LOC_CD", "LOC_NAME", selLocM)
            ViewBag.modelLocS = New MultiSelectList(
                GetSearchLocationList(LOC_DIV.LocationS, _ViewModel.SearchLocReverse, selLocM, _ViewModel.SearchLocL, authInfo),
                "LOC_CD", "LOC_NAME", selLocS)
        End Sub

        ''' <summary>
        ''' 検索条件のロケーションの選択肢を取得する（権限あり かつ 資産M（保管中・貸出中）に存在するもの）
        ''' 場所から選ぶ OFF：中区分は絞り込みなし、小区分は選択中の中区分で絞り込む
        ''' 場所から選ぶ ON ：小区分は絞り込みなし、中区分は選択中の小区分で絞り込む
        ''' 会社が未選択、または絞り込まれる側で絞り込む側が未選択の場合は空
        ''' </summary>
        ''' <param name="level">取得する側（M：中区分、S：小区分）</param>
        ''' <param name="reverse">場所から選ぶ</param>
        ''' <param name="filterCodes">反対側の選択中のコード</param>
        ''' <param name="selectLCode">会社（大区分コード）</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <returns>選択肢</returns>
        Private Function GetSearchLocationList(level As String, reverse As Boolean, filterCodes As String(),
                                               selectLCode As String, authInfo As AuthInfo) As List(Of LocationDropdownModel)
            Dim lstLocation As New List(Of LocationDropdownModel)

            '会社が未選択
            If String.IsNullOrEmpty(selectLCode) Then
                Return lstLocation
            End If

            '絞り込まれる側か（OFF なら小区分、ON なら中区分）
            Dim isFiltered As Boolean = If(reverse, level = LOC_DIV.LocationM, level = LOC_DIV.LocationS)
            Dim codes As String() = Nothing
            If isFiltered Then
                codes = CommonRfid.GetSelectedValues(filterCodes)
                If codes.Length = 0 Then
                    '絞り込む側が未選択
                    Return lstLocation
                End If
            End If

            Dim tbl As DataTable
            Dim exTarget As Dao.ToolLocation.LocExistTarget = Dao.ToolLocation.LocExistTarget.StockAsset()
            If level = LOC_DIV.LocationM Then
                tbl = Dao.ToolLocation.GetLocationMDataExists(selectLCode, _LoginInfo, authInfo, exTarget, codes)
            Else
                tbl = Dao.ToolLocation.GetLocationSDataExists(selectLCode, _LoginInfo, authInfo, exTarget, codes)
            End If

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            Return lstLocation
        End Function
#End Region
        '[CC431] ADD End 20261007

#Region "(ロケーション)大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)大区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationLData() As JsonResult
            Dim tbl As DataTable
            If Not _LoginInfo Is Nothing Then
                tbl = Dao.ToolLocation.GetLocationLData(_LoginInfo, _AuthInfo)
            Else
                tbl = Dao.ToolLocation.GetLocationLData()
            End If
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.modelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchLocL)
            _ViewModel.locLDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "(ロケーション)中区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)中区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationMData(ByVal selectLCode As String) As JsonResult
            Dim tbl As DataTable
            If Not _LoginInfo Is Nothing Then
                tbl = Dao.ToolLocation.GetLocationMData(selectLCode, _LoginInfo, _AuthInfo)
            Else
                tbl = Dao.ToolLocation.GetLocationMData(selectLCode)
            End If
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            '一覧画面のロケーションコンボ設定（複数選択のため MultiSelectList）
            ViewBag.modelLocM = New MultiSelectList(lstLocation, "LOC_CD", "LOC_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchLocM))
            _ViewModel.locMDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "(ロケーション)小区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <param name="selectLCode">大区分コード</param>
        ''' <param name="selectMCodes">中区分コード（複数選択）</param>
        ''' <returns></returns>
        Private Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCodes As String()) As JsonResult
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            '中区分は複数選択のため、選択された中区分ごとに小区分を取得してマージする
            For Each selectMCode As String In CommonRfid.GetSelectedValues(selectMCodes)
                Dim tbl As DataTable = Dao.ToolLocation.GetLocationSData(selectLCode, selectMCode, _LoginInfo, _AuthInfo)

                For Each row As DataRow In tbl.Rows
                    Dim locCd As String = row("LOC_CD").ToString
                    '複数の中区分に同じ小区分が存在する場合は1件にまとめる
                    If lstLocation.Any(Function(loc) loc.LOC_CD = locCd) Then
                        Continue For
                    End If

                    lstLocation.Add(
                        New LocationDropdownModel With {
                            .LOC_CD = locCd,
                            .LOC_NAME = row("LOC_NAME").ToString
                        }
                    )
                Next
            Next

            'コード順に並べ替える
            lstLocation = lstLocation.OrderBy(Function(loc) loc.LOC_CD).ToList

            '一覧画面のロケーションコンボ設定（複数選択のため MultiSelectList）
            ViewBag.modelLocS = New MultiSelectList(lstLocation, "LOC_CD", "LOC_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchLocS))
            _ViewModel.locSDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "(カテゴリ)大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (カテゴリ)大区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCategoryLData() As JsonResult
            Dim tbl As DataTable = Dao.ToolCategory.GetCategoryLData()
            Dim lstCategory As New List(Of CategoryDropdownModel)     'カテゴリマスタリスト

            For Each row As DataRow In tbl.Rows
                lstCategory.Add(
                    New CategoryDropdownModel With {
                        .CAT_CD = row("CAT_CD").ToString,
                        .CAT_NAME = row("CAT_NAME").ToString
                    }
                )
            Next

            ViewBag.modelCatL = New MultiSelectList(lstCategory, "CAT_CD", "CAT_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchCatL))
            _ViewModel.catLDropdownData = lstCategory

            Return Json(lstCategory)
        End Function
#End Region

#Region "ローカルストレージに保持しているロケ大コードを取得"
        ''' <summary>
        ''' ローカルストレージに保持しているロケ大コードを取得
        ''' </summary>
        ''' <returns></returns>
        Public Function SetLocCode(ByVal localStorageLCode As String) As ActionResult

            _ViewModel.SearchLocL = localStorageLCode

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_STOCK_LIST) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "資産一覧画面表示処理"
        ''' <summary>
        ''' 資産一覧画面表示処理
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function ShowAssetHistory(ByVal ViewModel As StockListViewModel, ByVal keyValue As String) As ActionResult
            '検索条件をセッションに保存
            Session(CommonSessionKey.CREAGE_STOCK_SERCH_KEY) = ViewModel

            'セッションクリア
            If Session(RedirectAssetListInfo.ID) IsNot Nothing Then
                Session.Remove(RedirectAssetListInfo.ID)
            End If

            'リダイレクト情報作成
            Dim info As New RedirectAssetListInfo
            info.RedirectPage = RedirectAssetListInfo.RedirectPageType.StockList
            info.KeyValue = keyValue

            Session(RedirectAssetListInfo.ID) = info

            Return RedirectToAction("Index", "AssetList")
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
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL(Optional ByVal isSort As Boolean = True) As String

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '権限情報の取得
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.StockList, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.Clear()
            If isSort Then
                sbSQL.AppendLine("SELECT ")
            Else
                sbSQL.AppendLine("SELECT TOP 1")
            End If
            sbSQL.AppendLine("ROW_NUMBER() OVER(ORDER BY")
            sbSQL.AppendLine("ma.LOC_L_CD")
            If _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationM Or _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationS Then
                sbSQL.AppendLine(",ma.LOC_M_CD")
                If _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationS Then
                    sbSQL.AppendLine(",ma.LOC_S_CD")
                End If
            End If
            sbSQL.AppendLine(",ma.CAT_L_CD")
            If _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryM Or _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryS Then
                sbSQL.AppendLine(",ma.CAT_M_CD")
                If _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryS Then
                    sbSQL.AppendLine(",ma.CAT_S_CD")
                End If
            End If

            sbSQL.AppendLine("ASC) SEQNO")
            sbSQL.AppendLine(", '' KEY_VALUE")
            sbSQL.AppendLine(",ma.MNG_LOC_L_CD") '2021/010/06 nv miyaji add
            sbSQL.AppendLine(",MNGLOCL.LOC_NAME MNG_LOC_L_NAME")    '2021/10/06 nv miyaji add
            sbSQL.AppendLine(", ma.LOC_L_CD")
            sbSQL.AppendLine(", mlocl.LOC_NAME LOC_L_NAME")
            If _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationL Then
                sbSQL.AppendLine(", '' LOC_M_CD")
                sbSQL.AppendLine(", '' LOC_M_NAME")
                sbSQL.AppendLine(", '' LOC_S_CD")
                sbSQL.AppendLine(", '' LOC_S_NAME")
            ElseIf _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationM Then
                sbSQL.AppendLine(", ma.LOC_M_CD")
                sbSQL.AppendLine(", mlocm.LOC_NAME LOC_M_NAME")
                sbSQL.AppendLine(", '' LOC_S_CD")
                sbSQL.AppendLine(", '' LOC_S_NAME")
            Else
                sbSQL.AppendLine(", ma.LOC_M_CD")
                sbSQL.AppendLine(", mlocm.LOC_NAME LOC_M_NAME")
                sbSQL.AppendLine(", ma.LOC_S_CD")
                sbSQL.AppendLine(", mlocs.LOC_NAME LOC_S_NAME")
            End If

            sbSQL.AppendLine(", ma.CAT_L_CD")
            sbSQL.AppendLine(", mcatl.CAT_NAME CAT_L_NAME")
            If _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryL Then
                sbSQL.AppendLine(", '' CAT_M_CD")
                sbSQL.AppendLine(", '' CAT_M_NAME")
                sbSQL.AppendLine(", '' CAT_S_CD")
                sbSQL.AppendLine(", '' CAT_S_NAME")
            ElseIf _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryM Then
                sbSQL.AppendLine(", ma.CAT_M_CD")
                sbSQL.AppendLine(", mcatm.CAT_NAME CAT_M_NAME")
                sbSQL.AppendLine(", '' CAT_S_CD")
                sbSQL.AppendLine(", '' CAT_S_NAME")
            Else
                sbSQL.AppendLine(", ma.CAT_M_CD")
                sbSQL.AppendLine(", mcatm.CAT_NAME CAT_M_NAME")
                sbSQL.AppendLine(", ma.CAT_S_CD")
                sbSQL.AppendLine(", mcats.CAT_NAME CAT_S_NAME")
            End If
            sbSQL.AppendLine(", COUNT(DISTINCT ASSET_NO) AS QUANTITY")

            sbSQL.AppendLine(", COUNT(*) OVER () As TotalCount")
            sbSQL.AppendLine("FROM M_ASSET ma")

            '--2021/09/17 nv miyaji add start
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL ")
            sbSQL.AppendLine("ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ma.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")
            '--2021/09/17 nv miyaji add end

            ' ロケーション(大)
            sbSQL.AppendLine("LEFT JOIN M_LOCATION mlocl ON")
            sbSQL.AppendLine("mlocl.LOC_DIV = '" & LOC_DIV.LocationL & "'")
            sbSQL.AppendLine("AND mlocl.LOC_L_CD = ma.LOC_L_CD")
            If _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationM Or _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationS Then
                ' ロケーション(中)
                sbSQL.AppendLine("LEFT JOIN M_LOCATION mlocm ON")
                sbSQL.AppendLine("mlocm.LOC_DIV = '" & LOC_DIV.LocationM & "'")
                sbSQL.AppendLine("AND mlocm.LOC_L_CD = ma.LOC_L_CD")
                sbSQL.AppendLine("AND mlocm.LOC_M_CD = ma.LOC_M_CD")
                If _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationS Then
                    ' ロケーション(小)
                    sbSQL.AppendLine("LEFT JOIN M_LOCATION mlocs ON")
                    sbSQL.AppendLine("mlocs.LOC_DIV = '" & LOC_DIV.LocationS & "'")
                    sbSQL.AppendLine("AND mlocs.LOC_L_CD = ma.LOC_L_CD")
                    sbSQL.AppendLine("AND mlocs.LOC_M_CD = ma.LOC_M_CD")
                    sbSQL.AppendLine("AND mlocs.LOC_S_CD = ma.LOC_S_CD")
                End If
            End If

            ' カテゴリー(大)
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY mcatl ON")
            sbSQL.AppendLine("mcatl.CAT_DIV = '" & CAT_DIV.CategoryL & "'")
            sbSQL.AppendLine("AND mcatl.CAT_L_CD = ma.CAT_L_CD")
            If _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryM Or _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryS Then
                ' カテゴリー(中)
                sbSQL.AppendLine("LEFT JOIN M_CATEGORY mcatm ON")
                sbSQL.AppendLine("mcatm.CAT_DIV = '" & CAT_DIV.CategoryM & "'")
                sbSQL.AppendLine("AND mcatm.CAT_L_CD = ma.CAT_L_CD")
                sbSQL.AppendLine("AND mcatm.CAT_M_CD = ma.CAT_M_CD")
                If _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryS Then
                    ' カテゴリー(小)
                    sbSQL.AppendLine("LEFT JOIN M_CATEGORY mcats ON")
                    sbSQL.AppendLine("mcats.CAT_DIV = '" & CAT_DIV.CategoryS & "'")
                    sbSQL.AppendLine("AND mcats.CAT_L_CD = ma.CAT_L_CD")
                    sbSQL.AppendLine("AND mcats.CAT_M_CD = ma.CAT_M_CD")
                    sbSQL.AppendLine("AND mcats.CAT_S_CD = ma.CAT_S_CD")
                End If
            End If
            '--2021/10/12 nv miyaji add start
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
            sbSQL.AppendLine("ON ma.MNG_LOC_L_CD = ULOC.LOC_L_CD ")
            sbSQL.AppendLine($"AND ((ULOC.LOC_M_FLG='{AUTH_LOC_M_FLG.Yes}' AND ULOC.LOC_M_CD = ma.LOC_M_CD) OR (ULOC.LOC_M_FLG = '{AUTH_LOC_M_FLG.No}')) ")
            If Not _LoginInfo Is Nothing Then
                addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
                addSqlParam("ReadAuthFlg", $"{_AuthInfo.ReadAuthFlg}")
                addSqlParam("UpdAuthFlg", $"{_AuthInfo.UpdAuthFlg}")
                addSqlParam("ReadHubAuthFlg", $"{_AuthInfo.ReadHubAuthFlg}")
                addSqlParam("UpdHubAuthFlg", $"{_AuthInfo.UpdHubAuthFlg}")
                addSqlParam("UpdAuthOption", $"{_AuthInfo.AuthOption}")
            Else
                'デバッグ時、_LoginInfoがNULLの場合があるため
                addSqlParam("EmployeeCd", "")
                addSqlParam("ReadAuthFlg", $"{AUTH_FLG.No}")
                addSqlParam("UpdAuthFlg", $"{AUTH_FLG.No}")
                addSqlParam("ReadHubAuthFlg", $"{AUTH_FLG.No}")
                addSqlParam("UpdHubAuthFlg", $"{AUTH_FLG.No}")
                addSqlParam("UpdAuthOption", "")
            End If

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("1 = 1")

            '検索条件を設定する

            '資産状態 SearchAssetStatus         2019/08/30 ADD Inoue 在庫は以下の状態の資産となります
            sbSQL.AppendLine("  And ma.ASSET_STATUS in (" & Misc.ASSET_STATUS.Storage & "," & Misc.ASSET_STATUS.Lending & ")")

            'ロケーション 大区分
            If Not IsNothing(_ViewModel.SearchLocL) AndAlso _ViewModel.SearchLocL <> "" Then
                sbSQL.AppendLine("AND ma.LOC_L_CD = @locLCd")
                addSqlParam("locLCd", _ViewModel.SearchLocL)
            End If
            'ロケーション 中区分（複数選択）
            Dim sLocMCond As String = CreateInCondition("ma.LOC_M_CD", "locMCd", _ViewModel.SearchLocM)
            If Not String.IsNullOrEmpty(sLocMCond) Then
                sbSQL.AppendLine(sLocMCond)
            End If
            'ロケーション 小区分（複数選択）
            Dim sLocSCond As String = CreateInCondition("ma.LOC_S_CD", "locSCd", _ViewModel.SearchLocS)
            If Not String.IsNullOrEmpty(sLocSCond) Then
                sbSQL.AppendLine(sLocSCond)
            End If

            'カテゴリ 大区分
            '（複数選択）
            Dim sCatLCond As String = CreateInCondition("ma.CAT_L_CD", "catLCd", _ViewModel.SearchCatL)
            If Not String.IsNullOrEmpty(sCatLCond) Then
                sbSQL.AppendLine(sCatLCond)
            End If
            ''カテゴリ 中区分
            'If Not IsNothing(_ViewModel.SearchCatM) AndAlso _ViewModel.SearchCatM <> "" Then
            '    sbSQL.AppendLine("AND ma.CAT_M_CD = @catMCd")
            '    addSqlParam("catMCd", _ViewModel.SearchCatM)
            'End If
            ''カテゴリ 小区分
            'If Not IsNothing(_ViewModel.SearchCatS) AndAlso _ViewModel.SearchCatS <> "" Then
            '    sbSQL.AppendLine("AND ma.CAT_S_CD = @catSCd")
            '    addSqlParam("catSCd", _ViewModel.SearchCatS)
            'End If

            '資産番号（「|」区切りで複数指定。資産番号・子番号のいずれかに前方一致）
            Dim sAssetNoCond As String = CreateLikeCondition({"ma.ASSET_NO", "ma.SUB_ASSET_NO"}, "ASSET_NO", CommonRfid.GetMultiInputValues(_ViewModel.SearchAssetNo))
            If Not String.IsNullOrEmpty(sAssetNoCond) Then
                sbSQL.AppendLine(sAssetNoCond)
            End If

            '資産ID
            If Not String.IsNullOrEmpty(_ViewModel.SearchAssetID) Then
                sbSQL.AppendLine("  AND ma.ASSET_ID Like @assetID")
                addSqlParam("assetID", $"%{_ViewModel.SearchAssetID}%")
            End If

            ' 資産名称
            If Not IsNothing(_ViewModel.SearchAssetName) AndAlso _ViewModel.SearchAssetName <> "" Then
                sbSQL.AppendLine("  AND ma.ASSET_NAME Like @assetName")
                addSqlParam("assetName", $"%{_ViewModel.SearchAssetName}%")
            End If
            '' メーカ
            'If Not IsNothing(_ViewModel.SearchMaker) AndAlso _ViewModel.SearchMaker <> "" Then
            '    sbSQL.AppendLine("  AND ma.MAKER Like @maker")
            '    addSqlParam("maker", $"%{_ViewModel.SearchMaker}%")
            'End If
            '' 品番
            'If Not IsNothing(_ViewModel.SearchProductNo) AndAlso _ViewModel.SearchProductNo <> "" Then
            '    sbSQL.AppendLine("  AND ma.PRODUCT_NO Like @productNo")
            '    addSqlParam("productNo", $"%{_ViewModel.SearchProductNo}%")
            'End If
            '' 型番
            'If Not IsNothing(_ViewModel.SearchModelNo) AndAlso _ViewModel.SearchModelNo <> "" Then
            '    sbSQL.AppendLine("  AND ma.MODEL_NO Like @modelNo")
            '    addSqlParam("modelNo", $"%{_ViewModel.SearchModelNo}%")
            'End If
            ''--2021/09/17 nv miyaji add start
            ''管理ロケーションコード
            'If Not String.IsNullOrEmpty(_ViewModel.SearchMngLocL) Then
            '    sbSQL.AppendLine("  AND ma.MNG_LOC_L_CD = @SearchMngLocL")
            '    addSqlParam("SearchMngLocL", $"{_ViewModel.SearchMngLocL}")
            'End If
            '--2021/09/17 nv miyaji add end
            sbSQL.AppendLine("GROUP BY")

            sbSQL.AppendLine("ma.LOC_L_CD")
            sbSQL.AppendLine(",mlocl.LOC_NAME")
            If _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationM Or _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationS Then
                sbSQL.AppendLine(",ma.LOC_M_CD")
                sbSQL.AppendLine(",mlocm.LOC_NAME")
                If _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationS Then
                    sbSQL.AppendLine(", ma.LOC_S_CD")
                    sbSQL.AppendLine(",mlocs.LOC_NAME")
                End If
            End If

            sbSQL.AppendLine(",ma.CAT_L_CD")
            sbSQL.AppendLine(",mcatl.CAT_NAME")
            If _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryM Or _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryS Then
                sbSQL.AppendLine(",ma.CAT_M_CD")
                sbSQL.AppendLine(",mcatm.CAT_NAME")
                If _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryS Then
                    sbSQL.AppendLine(",ma.CAT_S_CD")
                    sbSQL.AppendLine(",mcats.CAT_NAME")
                End If
            End If

            sbSQL.AppendLine(",ma.MNG_LOC_L_CD") '2021/010/06 nv miyaji add
            sbSQL.AppendLine(",MNGLOCL.LOC_NAME")    '2021/10/06 nv miyaji add

            If isSort Then
                sbSQL.AppendLine("ORDER BY")

                sbSQL.AppendLine("ma.LOC_L_CD")
                If _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationM Or _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationS Then
                    sbSQL.AppendLine(",ma.LOC_M_CD")
                    If _ViewModel.SearchDispSelectLocation = LOC_DIV.LocationS Then
                        sbSQL.AppendLine(",ma.LOC_S_CD")
                    End If
                End If

                sbSQL.AppendLine(",ma.CAT_L_CD")
                If _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryM Or _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryS Then
                    sbSQL.AppendLine(",ma.CAT_M_CD")
                    If _ViewModel.SearchDispSelectCategory = CAT_DIV.CategoryS Then
                        sbSQL.AppendLine(",ma.CAT_S_CD")
                    End If
                End If
            End If

            Return sbSQL.ToString()

        End Function

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New StockListViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchLocL = _ViewModel.SearchLocL
            _viewResult.ElementAt(0).SearchLocM = _ViewModel.SearchLocM
            _viewResult.ElementAt(0).SearchLocS = _ViewModel.SearchLocS
            '[CC431] ADD Start 20261007
            _viewResult.ElementAt(0).SearchLocReverse = _ViewModel.SearchLocReverse
            '[CC431] ADD End 20261007
            _viewResult.ElementAt(0).SearchCatL = _ViewModel.SearchCatL
            '_viewResult.ElementAt(0).SearchCatM = _ViewModel.SearchCatM
            '_viewResult.ElementAt(0).SearchCatS = _ViewModel.SearchCatS
            _viewResult.ElementAt(0).SearchAssetNo = _ViewModel.SearchAssetNo
            _viewResult.ElementAt(0).SearchAssetID = _ViewModel.SearchAssetID
            _viewResult.ElementAt(0).SearchAssetName = _ViewModel.SearchAssetName
            _viewResult.ElementAt(0).SearchMaker = _ViewModel.SearchMaker
            _viewResult.ElementAt(0).SearchProductNo = _ViewModel.SearchProductNo
            _viewResult.ElementAt(0).SearchModelNo = _ViewModel.SearchModelNo
            _viewResult.ElementAt(0).SearchDispSelectLocation = _ViewModel.SearchDispSelectLocation
            _viewResult.ElementAt(0).SearchDispSelectCategory = _ViewModel.SearchDispSelectCategory
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            If String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.STOCK_LIST, COLSET_LOC.TEMPLATE)
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.STOCK_LIST, COLSET_LOC.TEMPLATE).FIX_COL_CNT
            Else
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.STOCK_LIST, _ViewModel.SearchLocL)
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.STOCK_LIST, _ViewModel.SearchLocL).FIX_COL_CNT
            End If
            _viewResult.ElementAt(0).IsRestoreOwnSearchConditions = _ViewModel.IsRestoreOwnSearchConditions
            _viewResult.ElementAt(0).FirstSearch = _ViewModel.FirstSearch

        End Sub

        Private Sub SettingKeyData()
            Dim tempValue As String = ""
            For i As Integer = 0 To _viewResult.Count - 1
                tempValue = ""
                ' ロケーション 大
                If Not IsNothing(_viewResult.ElementAt(i).LOC_L_CD) Then
                    tempValue = _viewResult.ElementAt(i).LOC_L_CD
                End If
                ' ロケーション 中
                tempValue = tempValue & ","
                If Not IsNothing(_viewResult.ElementAt(i).LOC_M_CD) Then
                    tempValue = tempValue & _viewResult.ElementAt(i).LOC_M_CD
                End If
                ' ロケーション 小
                tempValue = tempValue & ","
                If Not IsNothing(_viewResult.ElementAt(i).LOC_S_CD) Then
                    tempValue = tempValue & _viewResult.ElementAt(i).LOC_S_CD
                End If
                ' カテゴリ 大
                tempValue = tempValue & ","
                If Not IsNothing(_viewResult.ElementAt(i).CAT_L_CD) Then
                    tempValue = tempValue & _viewResult.ElementAt(i).CAT_L_CD
                End If
                ' カテゴリ 中
                tempValue = tempValue & ","
                If Not IsNothing(_viewResult.ElementAt(i).CAT_M_CD) Then
                    tempValue = tempValue & _viewResult.ElementAt(i).CAT_M_CD
                End If
                ' カテゴリ 小
                tempValue = tempValue & ","
                If Not IsNothing(_viewResult.ElementAt(i).CAT_S_CD) Then
                    tempValue = tempValue & _viewResult.ElementAt(i).CAT_S_CD
                End If
                '--2021/10/12 nv miyaji add start
                ' 管理ロケーション
                tempValue = tempValue & ","
                If Not IsNothing(_viewResult.ElementAt(i).MNG_LOC_L_CD) Then
                    tempValue = tempValue & _viewResult.ElementAt(i).MNG_LOC_L_CD
                End If
                '--2021/10/12 nv miyaji add end
                _viewResult.ElementAt(i).KEY_VALUE = tempValue
            Next i
        End Sub

#End Region

    End Class
End Namespace