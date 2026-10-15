Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient
Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports GrapeCity.Documents.Excel
Imports GrapeCity.Documents.Excel.Drawing
Imports Asetra.Dao

Namespace Controllers
    Public Class AssetListController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"



        '2019/12/20 nv miyaji　commonconstに移動
        '''' <summary>
        '''' データ取込画面　結果表示用クラス
        '''' </summary>
        '''' <remarks>
        '''' 資産マスタのCSV取り込み時に作成するデータテーブルの項目並び順と同じにする。
        '''' (commonconst.CsvMapMei_Asset.CharCdに定義)
        '''' </remarks>
        'Private Class CsvAsset
        '    Public Enum ColIdx          '■■■マッピング仕様でも問題ない？　後、必ず、値を記述すること
        '        ''' <summary>資産番号</summary>
        '        AssetNo = 0
        '        ''' <summary>資産ID</summary>
        '        AssetID = 1
        '        ''' <summary>資産区分</summary>
        '        AssetDiv = 2
        '        ''' <summary>資産名称</summary>
        '        AssetNm = 3
        '        ''' <summary>ロケ大区分コード</summary>
        '        LocLCd = 4
        '        ''' <summary>ロケ大区分名</summary>
        '        LocLNm = 5
        '        ''' <summary>ロケ中区分コード</summary>
        '        LocMCd = 6
        '        ''' <summary>ロケ中区分名</summary>
        '        LocMNm = 7
        '        ''' <summary>ロケ小区分コード</summary>
        '        LocSCd = 8
        '        ''' <summary>ロケ小区分名</summary>
        '        LocSNm = 9
        '        ''' <summary>カテゴリ大分類コード</summary>
        '        CatLCd = 10
        '        ''' <summary>カテゴリ大分類名</summary>
        '        CatLNm = 11
        '        ''' <summary>カテゴリ中分類コード</summary>
        '        CatMCd = 12
        '        ''' <summary>カテゴリ中分類名</summary>
        '        CatNm = 13
        '        ''' <summary>カテゴリ小分類コード</summary>
        '        CatSCd = 14
        '        ''' <summary>カテゴリ小分類名</summary>
        '        CatSNm = 15
        '        ''' <summary>メーカ</summary>
        '        Maker = 16
        '        ''' <summary>品番</summary>
        '        ProductNo = 17
        '        ''' <summary>型番</summary>
        '        ModelNo = 18
        '        ''' <summary>リース管理番号</summary>
        '        LeaseNo = 19
        '        ''' <summary>リース期限</summary>
        '        LeaseLimit = 20
        '        ''' <summary>○○期限</summary>
        '        GenLimit = 21
        '        ''' <summary>フリー項目1</summary>
        '        FreeItem1 = 22
        '        ''' <summary>フリー項目2</summary>
        '        FreeItem2 = 23
        '        ''' <summary>フリー項目3</summary>
        '        FreeItem3 = 24
        '        ''' <summary>フリー項目4</summary>
        '        FreeItem4 = 25
        '        ''' <summary>フリー項目5</summary>
        '        FreeItem5 = 26
        '        ''' <summary>フリー項目6</summary>
        '        FreeItem6 = 27
        '        ''' <summary>フリー項目7</summary>
        '        FreeItem7 = 28
        '        ''' <summary>フリー項目8</summary>
        '        FreeItem8 = 29
        '        ''' <summary>フリー項目9</summary>
        '        FreeItem9 = 30
        '        ''' <summary>フリー項目10</summary>
        '        FreeItem10 = 31
        '        ''' <summary>資産画像ファイル名</summary>
        '        AssetPicFileNm = 32
        '    End Enum
        'End Class

        ''' <summary>
        ''' ページ間のチェックボックス対応
        ''' </summary>
        Private Enum AssetListKey
            ASSET_NO = 0
            ASSET_ID = 1
            CATEGORY_L = 2
        End Enum

        ''' <summary>
        ''' 既存DBデータ区分　　ADD  Inoue 2019/12/18
        ''' </summary>
        Private Enum DbDataKbn
            ''' <summary>DBデータなし</summary>
            Non = 0
            ''' <summary>DBデータあり</summary>
            Exist = 1
            ''' <summary>DBデータ除却</summary>
            Removed = 9
        End Enum

        Public Const GRAI_PREFIX As String = "0011001105"     'GRAIヘッダ部＋フィルター値＋パーティション
        Public Const GIAI_PREFIX As String = "0011010005"     'GIAIヘッダ部＋フィルター値＋パーティション
        Public Const AUTH_OPTION As String = "U_CSVUPLOAD"     '特殊権限項目

        Public Const MIN_DATE As Integer = 10000101           '最小日付
#End Region

#Region "クラス変数"
        Private _Model As AssetListModel = New AssetListModel                                 'Assetモデル
        Private _ViewModel As AssetListViewModel = New AssetListViewModel                     'Asset一覧表示用モデル
        Private _HistoryModel As AssetHistoryModel = New AssetHistoryModel                    '資産履歴登録用モデル
        Protected _viewResult As List(Of AssetListViewModel) = New List(Of AssetListViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _blRetireFlg As Boolean = False    '除却ボタン押下フラグ
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】CSV取込用のクラス変数（_dtCsvImport・_lstCsvHed）"
        'Private _dtCsvImport As DataTable = Nothing 'CSV取込用作業テーブル
        'Private _lstCsvHed As List(Of String) = Nothing 'CSVヘッダ文字コード格納用リスト
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End
        Private _sBeforeAssetID As String = String.Empty '新規登録時、0埋め・自動生成前の資産IDを格納する
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】CSV取込件数のクラス変数（_iCsvImportRowCount）"
        'Private _iCsvImportRowCount As Integer = 0 'CSV取込件数 '2020/03/16 nv miyaji add
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

        Private _LocationLList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationMList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationSList As List(Of LocationDropdownModel)     'ロケーションマスタリスト

        Private _RegistInfo As New List(Of Dictionary(Of String, String))     '登録情報
        Private _isSort As Boolean = True                       'ソートフラグ
        Private _AuthInfo As New AuthInfo                 '権限情報

#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As AssetListViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(RedirectAssetListInfo.ID) IsNot Nothing Then
                Session.Remove(RedirectAssetListInfo.ID)
                'チェックボックス状態の選択状態をクリアする  '2020/02/12 nv miyaji　
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする  '2020/02/12 nv miyaji　　
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
            End If

            If Session(CommonSessionKey.SEARCH_FLG) IsNot Nothing Then
                Session.Remove(CommonSessionKey.SEARCH_FLG)
            End If

            'リダイレクト情報作成
            Dim info As New RedirectAssetListInfo
            info.RedirectPage = RedirectAssetListInfo.RedirectPageType.Menu
            info.KeyValue = String.Empty

            Session(RedirectAssetListInfo.ID) = info

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
        Public Function Index(ByVal ViewModel As AssetListViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LIST)
            End If

            '上限確認フラグ 初期化
            ViewBag.isLimitFlg = False
            '一覧件数　初期化
            ViewBag.cntList = 0
            '設定マスタ上限件数　初期化
            ViewBag.cntLimit = 0
            '一覧表示フラグ
            Dim isDisp As Boolean = True

            'メッセージ一覧取得
            Dim Message As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '2023/02/08 Kawamoto ADD START
            '設定マスタ（薬品管理機能）の取得
            Dim DrugsFunc As SettingInfo = Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv, 
                                                                  CommonSettingKey.Drugs_Function.Key1, 
                                                                  CommonSettingKey.Drugs_Function.Key2, 
                                                                  CommonSettingKey.Drugs_Function.Key3, True)
            '2023/02/08 Kawamoto ADD END

            '20230901 ADD Start Shirakawa
            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Asset_List.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Asset_List.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Asset_List.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Asset_List.Key3)
            '20230901 ADD End Shirakawa

            '検索ボタンがクリックされた場合、選択状態のセッションをクリアする
            If Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする  '2020/02/12 nv miyaji　　
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                'セッションSEARCH_FLGを設定
                Session(CommonSessionKey.SEARCH_FLG) = True

                '2023/02/08 Kawamoto ADD START
            Else
                '薬品機能がONの場合
                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                    If ViewModel.OpenFlg = String.Empty Then
                        '表示順（初期値：残量割合順）
                        ViewModel.SearchDispSelect = AssetDispSelect.Rate
                        ViewModel.DetailOpenFlg = Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)
                    End If
                End If
                '2023/02/08 Kawamoto ADD END
            End If

            '資産番号の自動生成設定を取得
            Dim AssetNoSelfReg As SettingInfo = Message.getSettingInfo(CommonSettingKey.AssetNo_Self_Regist.KeyDiv,
                                                                  CommonSettingKey.AssetNo_Self_Regist.Key1,
                                                                  CommonSettingKey.AssetNo_Self_Regist.Key2,
                                                                  CommonSettingKey.AssetNo_Self_Regist.Key3, True)

            If Not IsNothing(AssetNoSelfReg) AndAlso AssetNoSelfReg.DATA1 = ValidFlg.Cd.Valid Then
                '資産番号の自動生成あり
                ViewBag.AssetNoSelfReg = ValidFlg.Cd.Valid
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As AssetListViewModel = Session(CommonSessionKey.CREAGE_ASSET_LIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                _ViewModel.DrugsFunc = True
            End If
            '最新のチェック状態でセッション情報を更新する(追加および削除)
            'UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys) '2020/01/07 nv miyaji コメントアウト

            ''表示時に使用するキーが存在する場合はViewBagに格納する
            'Dim sKeyValue As String = Request.QueryString("keyValue")
            'If Not String.IsNullOrEmpty(sKeyValue) Then
            '    ViewBag.AssetListKey = sKeyValue
            'Else
            '    ViewBag.AssetListKey = AssetListKey
            'End If
            Dim info As RedirectAssetListInfo = Session(RedirectAssetListInfo.ID)
            If info IsNot Nothing Then
                If info.RedirectPage = RedirectAssetListInfo.RedirectPageType.AssetList Then
                    '履歴照会画面から戻ったときの処理
                    Dim senimaeModel As AssetListViewModel = Session(CommonSessionKey.CREAGE_ASSET_SERCH_KEY)
                    _ViewModel = senimaeModel
                    Session.Remove(CommonSessionKey.CREAGE_ASSET_SERCH_KEY)

                    'リダイレクト情報作成(メニューから呼び出した時と同じ状態にする)
                    Dim info2 As New RedirectAssetListInfo
                    info2.RedirectPage = RedirectAssetListInfo.RedirectPageType.Menu
                    info2.KeyValue = String.Empty

                    Session(RedirectAssetListInfo.ID) = info2
                Else
                    ViewBag.AssetListKey = info.KeyValue
                End If
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '項目配置情報取得
                Dim dictGridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_LIST, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL))
                If dictGridInfo.Count = 0 Then
                    dictGridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_LIST, COLSET_LOC.TEMPLATE)
                End If

                '20230904 UPD Start Shirakawa

                '未タグ紐づけ資産 存在チェック用
                ViewQuery()

                '検索条件の復元有無
                _ViewModel.IsRestoreOwnSearchConditions = True

                '資産在庫照会以外からの遷移の場合
                If info Is Nothing OrElse info.RedirectPage <> RedirectAssetListInfo.RedirectPageType.StockList Then
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
                End If


                '20230904 UPD End Shirakawa

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm

                ViewBag.LoginInfo = _LoginInfo


                ''未タグ紐付けの資産があるかどうか
                'ViewBag.MiTagHimoduke = ""
                ''--2020/01/07 miyaji update start
                'If isDisp AndAlso IsExistsMitagHimoduke() Then
                '    ViewBag.MiTagHimoduke = Me._Message.getMessage(CommonLabel.MSG_ASSET_MIHIMODUKEARI)
                'End If
                'For Each row As AssetListViewModel In _viewResult
                '    If row.TAG_LINK_STATUS = TagLinkStatus.Mi AndAlso row.ASSET_STATUS <> ASSET_STATUS.Removed Then
                '        ViewBag.MiTagHimoduke = Me._Message.getMessage(CommonLabel.MSG_ASSET_MIHIMODUKEARI)
                '        Exit For
                '    End If
                'Next
                '--2020/01/07 miyaji update end

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
                '未タグ紐付けの資産　文言クリア
                ViewBag.MiTagHimoduke = ""

                '読取データをクリア
                _viewResult.Clear()
            ElseIf isDisp Then
                'ソートする
                _isSort = True
                '一覧データ取得
                MyBase.BaseIndex(_ViewModel)
            End If

            '20230901 ADD End Shirakawa

            '設定情報(ロケ使用区分)取得     2020/03/17 ADD nv.miyaji
            Dim modelLocationUsage As New LocationUsageModel       'ロケーション使用区分
            Dim setting As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingInfo.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            ViewBag.LocUsage_L = setting.DATA1
            ViewBag.LocUsage_M = setting.DATA2
            ViewBag.LocUsage_S = setting.DATA3

            '--2020/01/07 miyaji add start
            '最新のチェック状態でセッション情報を更新する(追加および削除)
            UpdateChecedInfo(_ViewModel.SelectKeys, _ViewModel.NoneSelectKeys)
            '--2020/01/07 miyaji add end

            'ログインセッション切れ以外のエラーが設定されていたらモーダルのコンボボックスを設定
            If _ViewModel.ErrCode = String.Empty OrElse _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY Then
                'ロケーション
                ' 大区分ドロップダウンリスト作成
                GetLocationLData()
                ' 中区分
                GetLocationMData(_ViewModel.SearchLocL)
                ' 小区分（中区分は複数選択）
                GetLocationSData(_ViewModel.SearchLocL, _ViewModel.SearchLocM)
                '[CC431] ADD Start 20261007
                ' 検索条件の中区分・小区分（資産Mに存在するもの。場所から選ぶの ON／OFF で連動）
                ' ※上の GetLocationMData／GetLocationSData はモーダル用のコンボ（modalmodelLocM／S）も設定するため残し、検索条件のコンボだけ上書きする
                ' ※ログインセッション切れの場合は権限が取れないため上書きしない
                If Not IsNothing(_LoginInfo) Then
                    SetSearchLocationOptions(Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo))
                End If
                '[CC431] ADD End 20261007

                'カテゴリー
                ' 大分類ドロップダウンリスト作成
                GetCategoryLData()
                ' 中分類（大分類は複数選択のため先頭の値を使用）
                GetCategoryMData(CommonRfid.GetFirstSelectedValue(_ViewModel.SearchCatL))
                ' 小分類（大分類は複数選択のため先頭の値を使用）
                GetCategorySData(CommonRfid.GetFirstSelectedValue(_ViewModel.SearchCatL), _ViewModel.SearchCatM)

            Else
                'ロケーション
                ' 大区分ドロップダウンリスト作成
                GetLocationLData()
                ' 中区分
                GetLocationMData(_ViewModel.LOC_L_CD)
                ' 小区分
                GetLocationSData(_ViewModel.LOC_L_CD, New String() {_ViewModel.LOC_M_CD})
                '[CC431] ADD Start 20261007
                ' 検索条件の中区分・小区分は、検索条件の会社・選択値で作り直す
                ' ※上の GetLocationMData／GetLocationSData はモーダルの値で検索条件のコンボも作るため、検索条件の選択値が外れる
                If Not IsNothing(_LoginInfo) Then
                    SetSearchLocationOptions(Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo))
                End If
                '[CC431] ADD End 20261007

                'カテゴリー
                ' 大分類ドロップダウンリスト作成
                GetCategoryLData()
                ' 中分類
                GetCategoryMData(_ViewModel.CAT_L_CD)
                ' 小分類
                GetCategorySData(_ViewModel.CAT_L_CD, _ViewModel.CAT_M_CD)

                '資産ID入力可不可設定
                ViewBag.AssetIdDivReg = _ViewModel.AssetIdDivReg

                '除却/除却解除ボタンを設定
                If Not _ViewModel.RETIRE_TS Is Nothing Then
                    ViewBag.RetireBtnStr = Me._Message.getMessage(CommonLabel.BUTTON_RETIRE_OFF)
                Else
                    ViewBag.RetireBtnStr = Me._Message.getMessage(CommonLabel.VALUE_RETIRE)
                End If

                '資産状態が保管中でない場合、貸出状態コンボボックスを非活性にする。
                If _ViewModel.ASSET_STATUS <> ASSET_STATUS.Storage Then
                    ViewBag.NotEnableLeaseStatus = True
                End If
            End If

            '管理ロケーションドロップダウンリスト作成   '2021/10/01 nv miyaji add
            GetMngLocationLData()

            '貸出先
            GetLeasePlaceData()

            '貸出状態   '2021/09/17 nv miyaji add
            GetLeaseStatusData()

            ' プリンタ情報取得
            GetPrinter()

            '資産状態ドロップダウンリストデータ取得
            GetAssetStatusData()
            '資産ID状態ドロップダウンリストデータ取得
            GetAssetIdStatusData()
            'タグ紐づけ状態ドロップダウンリストデータ取得
            GetTagLinkStatusData()
            '貸出ドロップダウンリストデータ取得
            GetLending()
            '表示順ドロップダウンリストデータ取得
            GetDispOrder()
            '〇〇期限　期限切れドロップダウンリストデータ取得
            GetGenericLimitExpiredData()
            '〇〇期限　アラートドロップダウンリストデータ取得
            GetGenericLimitAlertData()
            'リース期限　期限切れドロップダウンリストデータ取得
            GetLeaseLimitExpiredData()
            'リース期限　アラートドロップダウンリストデータ取得
            GetLeaseLimitAlertData()
            '返却期限　期限切れドロップダウンリストデータ取得
            GetReturnLimitExpiredData()
            'フリー項目10（貸与資産状態区分）ドロップダウンリスト作成
            GetFreeItem10Data()
            'タグ紐付状態ドロップダウンリスト作成
            GetTagLinkStatusData()

            '2023/02/08 Kawamoto ADD START
            '薬品機能がONの場合
            If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                ViewBag.DrugsFunc = True
                'フリー項目3（消防法の分類）ドロップダウンリスト作成
                GetFreeItem3Data()
                'フリー項目4（有機則の該当有無）ドロップダウンリスト作成
                GetFreeItem4Data()
                'フリー項目5（毒物及び劇薬取扱法の該当）ドロップダウンリスト作成
                GetFreeItem5Data()
                'フリー項目6（特化則の該当有無）ドロップダウンリスト作成
                GetFreeItem6Data()
                'フリー項目7（PRTR制度の該当有無）ドロップダウンリスト作成
                GetFreeItem7Data()
                'フリー項目8（化学物質ﾘｽｸｱｾｽﾒﾝﾄ実施義務の有無）ドロップダウンリスト作成
                GetFreeItem8Data()
                'フリー項目9（購入者）ドロップダウンリスト作成
                GetFreeItem9Data()
                'フリー項目15（計量器）ドロップダウンリスト作成
                GetFreeItem15Data()

                '2023/05/10 Kawamoto ADD START
                '計量器連携あり情報の取得
                ViewBag.WeighingMachineLinkList = Dao.ToolSetting.GetWeighingMachineLinkList()
                ViewBag.WeighingMachine = _ViewModel.FREE_ITEM15
                ViewBag.RemainingAmount = _ViewModel.RemainingAmount
            Else
                ViewBag.WeighingMachineLinkList = New List(Of String)
                '2023/05/10 Kawamoto ADD END
            End If

            '設定マスタ（資産CSVファイル取込全件対象）の取得
            Dim CsvRegistAll As SettingInfo = Message.getSettingInfo(CommonSettingKey.Asset_CSV_Regist_ALL.KeyDiv,
                                                                     CommonSettingKey.Asset_CSV_Regist_ALL.Key1,
                                                                     CommonSettingKey.Asset_CSV_Regist_ALL.Key2,
                                                                     CommonSettingKey.Asset_CSV_Regist_ALL.Key3, True)
            '資産CSVファイル取込全件対象が「全件対象」の場合
            If CsvRegistAll.DATA1 = AssetCSVRegistAll.All Then
                ViewBag.RegistAll = True
            Else
                ViewBag.RegistAll = False
            End If
            '2023/02/08 Kawamoto ADD END

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.ASSET_LIST, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL))
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
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()

            'クエリを返却
            MyBase._SQL = CreateSQL(_isSort)

            '検索条件(全件)を指定したSQLとパラメーターをセッションに保持する
            Session(CommonSessionKey.LIST_VIEW_SQL) = MyBase._SQL
            Session(CommonSessionKey.LIST_VIEW_PARAM) = MyBase._paramSqlList

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()

            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LIST)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of AssetListViewModel) = _db.AssetListView.SqlQuery(_SQL, _paramSqlList.ToArray)

            '画面に表示するViewを設定
            _viewResult = query.ToList

        End Sub

#End Region

#Region "登録・更新処理"
        ''' <summary>
        ''' アクションメソッド(Create)
        ''' 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        ''' 詳細については、http://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <param name="OnCreteButton">クリックされたボタン</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function Create(ByVal ViewModel As AssetListViewModel, ByVal OnCreteButton As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Dim db As New SystemDBContext

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '画像ファイル保存場所取得
            Dim Setting_Image As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Image.KeyDiv, CommonSettingKey.Image.Key1, CommonSettingKey.Image.Key2, CommonSettingKey.Image.Key3, True)
            Dim sImagePath As String = Setting_Image.DATA1
            Dim sFullPath As String = Server.MapPath(CommonConst.ROOT_FOLDERNM & sImagePath)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            Dim autoAssetNo As Boolean = False

            '画像の登録処理
            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '新規登録の場合
                '画像が選択されている場合
                If Not String.IsNullOrEmpty(_ViewModel.ASSET_PIC_FILENAME) Then
                    '変更後の画像を一時フォルダからコピーする
                    Dim sTmpImagePath As String = Misc.CommonConst.IMAGETMP_FOLDERNM
                    Dim sTmpFullPath As String = Server.MapPath(CommonConst.ROOT_FOLDERNM & sTmpImagePath)
                    '--20211130 sv suzuki add start
                    If Not IO.File.Exists(sTmpFullPath + "\" + _ViewModel.ASSET_PIC_FILENAME) Then
                        _ViewModel.ErrCode = CommonLabel.MSG_ERR_PIC_FILE_NOT_FOUND
                    Else
                        '--20211130 sv suzuki add end
                        System.IO.File.Copy(sTmpFullPath + "\" + _ViewModel.ASSET_PIC_FILENAME, sFullPath + "\" + _ViewModel.ASSET_PIC_FILENAME, True)
                    End If
                End If

                '資産番号の自動生成確認
                autoAssetNo = IsAutoAssetNo()
                If IsAutoAssetNo() Then
                    '資産番号の自動生成あり

                    '資産番号を生成
                    _ViewModel.ASSET_NO = AssetNoAutoGenelate()
                End If

            Else
                '変更の場合
                '画像が変更されていたら変更前の画像を削除し、変更後の画像を登録する
                Dim beforemodel As AssetListModel = db.AssetList.Find(_ViewModel.ASSET_NO)
                If CommonRfid.ToStringNvl(_ViewModel.ASSET_PIC_FILENAME) <> CommonRfid.ToStringNvl(beforemodel.ASSET_PIC_FILENAME) Then
                    '変更前の画像ファイルが他の資産で使われていなければ削除
                    If Not String.IsNullOrEmpty(beforemodel.ASSET_PIC_FILENAME) Then
                        If Dao.ToolAsset.IsUseImg(beforemodel.ASSET_NO, beforemodel.ASSET_PIC_FILENAME) = False Then
                            System.IO.File.Delete(sFullPath + "\" + beforemodel.ASSET_PIC_FILENAME)
                        End If
                    End If
                    '画像が選択されている場合
                    If Not String.IsNullOrEmpty(_ViewModel.ASSET_PIC_FILENAME) Then
                        '変更後の画像を一時フォルダからコピーする
                        Dim sTmpImagePath As String = Misc.CommonConst.IMAGETMP_FOLDERNM
                        Dim sTmpFullPath As String = Server.MapPath(CommonConst.ROOT_FOLDERNM & sTmpImagePath)
                        '--20211130 sv suzuki add start
                        If Not IO.File.Exists(sTmpFullPath + "\" + _ViewModel.ASSET_PIC_FILENAME) Then
                            _ViewModel.ErrCode = CommonLabel.MSG_ERR_PIC_FILE_NOT_FOUND
                        Else
                            '--20211130 sv suzuki add end
                            System.IO.File.Copy(sTmpFullPath + "\" + _ViewModel.ASSET_PIC_FILENAME, sFullPath + "\" + _ViewModel.ASSET_PIC_FILENAME, True)
                        End If
                    End If
                End If
            End If

            'ロケーションコード・カテゴリコードが空の場合、空白を設定する
            If String.IsNullOrEmpty(_ViewModel.LOC_L_CD) Then
                _ViewModel.LOC_L_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.LOC_M_CD) Then
                _ViewModel.LOC_M_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.LOC_S_CD) Then
                _ViewModel.LOC_S_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.CAT_M_CD) Then
                _ViewModel.CAT_M_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.CAT_S_CD) Then
                _ViewModel.CAT_S_CD = " "
            End If
            '--2021/09/17 nv miyaji add start
            '管理ロケーションコードが空の場合、空白を設定する。
            If String.IsNullOrEmpty(_ViewModel.MNG_LOC_L_CD) Then
                _ViewModel.MNG_LOC_L_CD = " "
            End If
            '--2021/09/17 nv miyaji add end

            'リース期限・○○期限を設定する
            If String.IsNullOrEmpty(_ViewModel.LeaseLimitString) Then
                _ViewModel.LEASE_LIMIT = Nothing
            Else
                _ViewModel.LEASE_LIMIT = CInt(CDate(_ViewModel.LeaseLimitString).ToString("yyyyMMdd"))
            End If
            If String.IsNullOrEmpty(_ViewModel.GenLimitString) Then
                _ViewModel.GENERIC_LIMIT = Nothing
            Else
                _ViewModel.GENERIC_LIMIT = CInt(CDate(_ViewModel.GenLimitString).ToString("yyyyMMdd"))
            End If

            '資産購入日を設定する
            'If String.IsNullOrEmpty(_ViewModel.AssetKounyuDateString) Then
            '    _ViewModel.FREE_ITEM1 = Nothing
            'Else
            '    _ViewModel.FREE_ITEM1 = CDate(_ViewModel.AssetKounyuDateString).ToString("yyyy/MM/dd")
            'End If

            '--2021/09/30 nv miyaji add start
            '貸出日数の稼働日フラグを設定する
            If _ViewModel.DispLeaseDayWorkFlg Then
                _ViewModel.MAINTE_LEASE_DAY_WORK_FLAG = MainteWorkFlg.Yes
            Else
                If _ViewModel.MAINTE_LEASE_DAY Is Nothing Then
                    _ViewModel.MAINTE_LEASE_DAY_WORK_FLAG = Nothing
                Else
                    _ViewModel.MAINTE_LEASE_DAY_WORK_FLAG = MainteWorkFlg.No
                End If
            End If

            'メンテ日数の稼働日フラグを設定する
            If _ViewModel.DispMainteDayWorkFlg Then
                _ViewModel.MAINTE_DAY_WORK_FLAG = MainteWorkFlg.Yes
            Else
                If _ViewModel.MAINTE_DAY Is Nothing Then
                    _ViewModel.MAINTE_DAY_WORK_FLAG = Nothing
                Else
                    _ViewModel.MAINTE_DAY_WORK_FLAG = MainteWorkFlg.No
                End If
            End If
            '--2021/09/30 nv miyaji add end

            '2023/02/08 Kawamoto ADD START
            '設定マスタ(ロケごとの資産登録上限数)を取得
            Dim Settinginfo As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Asset_Registration_Limit.KeyDiv,
                                                                        CommonSettingKey.Asset_Registration_Limit.Key1,
                                                                        CommonSettingKey.Asset_Registration_Limit.Key2,
                                                                        CommonSettingKey.Asset_Registration_Limit.Key3, True)

            'ロケごとの資産登録上限数 > 0の場合
            If Settinginfo.DATA1 IsNot Nothing AndAlso Integer.Parse(Settinginfo.DATA1) > 0 Then

                Dim LocLInfo As New Dictionary(Of String, String)
                If _ViewModel.MNG_LOC_L_CD.Trim <> String.Empty Then
                    '資産マスタ(管理ロケ大区分がロケ大区分、または管理ロケ大区分と一致するデータの総数)を取得
                    LocLInfo.Add("TargetLoc", "Target_MNG_LOC_L_CD")
                    LocLInfo.Add("LocLCode", _ViewModel.MNG_LOC_L_CD)
                    LocLInfo.Add("RegistNum", Dao.ToolAsset.GetAssetLocLCodeTotal(_ViewModel.ASSET_NO, _ViewModel.MNG_LOC_L_CD) + 1.ToString)
                    LocLInfo.Add("RegistLimit", Settinginfo.DATA1)
                    _RegistInfo.Add(LocLInfo)
                End If

                If ViewModel.LOC_L_CD.Trim <> String.Empty AndAlso _ViewModel.MNG_LOC_L_CD.Trim <> ViewModel.LOC_L_CD.Trim Then
                    '資産マスタ(ロケ大区分がロケ大区分、または管理ロケ大区分と一致するデータの総数)を取得
                    LocLInfo = New Dictionary(Of String, String)
                    LocLInfo.Add("TargetLoc", "Target_LOC_L_CD")
                    LocLInfo.Add("LocLCode", _ViewModel.LOC_L_CD)
                    LocLInfo.Add("RegistNum", Dao.ToolAsset.GetAssetLocLCodeTotal(_ViewModel.ASSET_NO, _ViewModel.LOC_L_CD) + 1.ToString)
                    LocLInfo.Add("RegistLimit", Settinginfo.DATA1)
                    _RegistInfo.Add(LocLInfo)
                End If

                For i As Integer = 0 To _RegistInfo.Count - 1
                    If _RegistInfo.Count = 0 Then Exit For
                    If Integer.Parse(_RegistInfo(i)("RegistNum")) > Integer.Parse(Settinginfo.DATA1) Then
                        _ViewModel.ErrCode = CommonLabel.MSG_ERR_EXCEED_MAX_NUM
                        Exit For
                    Else
                        _RegistInfo.RemoveAt(i)
                        i -= 1
                    End If
                Next
            End If

            '設定マスタ（薬品管理機能）の取得
            Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
                                                                      CommonSettingKey.Drugs_Function.Key1,
                                                                      CommonSettingKey.Drugs_Function.Key2,
                                                                      CommonSettingKey.Drugs_Function.Key3, True)

            '薬品機能がONの場合
            'If DrugsFunc.DATA1 = DrugsFunction.Yes Then

            '    If _ViewModel.FREE_ITEM3 IsNot Nothing AndAlso _ViewModel.FREE_ITEM14 IsNot Nothing AndAlso _ViewModel.FREE_ITEM16 IsNot Nothing Then

            '        'FreeItem17(残量割合(残量/内容量))の設定
            '        _ViewModel.FREE_ITEM17 = CalsConsumptionRate(_ViewModel.FREE_ITEM10,
            '                                                     _ViewModel.FREE_ITEM11,
            '                                                     _ViewModel.FREE_ITEM12,
            '                                                     _ViewModel.FREE_ITEM14,
            '                                                     _ViewModel.FREE_ITEM16)
            '    End If
            'End If
            '2023/02/08 Kawamoto ADD END

            '2023/03/22 Kawamoto ADD START
            '添付ファイル件数の取得
            Dim sAttachmentPath As String = ConfigurationManager.AppSettings("AttachmentFolderPath")
            Dim sFolderPath As String = Path.Combine(sAttachmentPath, CommonRfid.FncStringToHex(_ViewModel.ASSET_NO))
            _ViewModel.ATTACHMENT_NUM = SelectAttachmentList(ConvertEnablePath(sFolderPath), _ViewModel.ASSET_NO).Count
            '2023/03/22 Kawamoto ADD END

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '新規登録の場合

                '登録処理
                MyBase.BaseCreate(_ViewModel)

            Else
                '変更時
                ''除却状態からの変更の場合、除却解除＋変更をする
                'If Not _ViewModel.RETIRE_TS Is Nothing Then
                '    '資産ＩＤを生成する
                '    Select Case model.ASSET_ID_REG_DIV
                '        Case AssetIDGenerationMethod.None, AssetIDGenerationMethod.Auto
                '            _ViewModel.GEN_ASSET_ID = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, String.Empty, model.EPC_SIZE, _LoginInfo)
                '        Case AssetIDGenerationMethod.AssetNo
                '            _ViewModel.GEN_ASSET_ID = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, _ViewModel.ASSET_NO, model.EPC_SIZE, _LoginInfo)
                '        Case AssetIDGenerationMethod.Inp
                '            If _ViewModel.ASSET_ID = String.Empty Then
                '                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_NOT_HEX)
                '            Else
                '                '16進文字列チェック
                '                Dim IsError As Boolean = False
                '                For Each Chr As Char In _ViewModel.ASSET_ID
                '                    If Not Uri.IsHexDigit(Chr) Then
                '                        ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_NOT_HEX)
                '                        IsError = True
                '                        Exit For
                '                    End If
                '                Next
                '                If Not _ViewModel.ASSET_ID.Length Mod 2 = 0 Then   '文字の長さが奇数の場合エラー
                '                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_NOT_HEX)
                '                    IsError = True
                '                End If
                '                If Not IsError Then
                '                    '資産IDの長さチェック
                '                    Dim sCheckID As String = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, _ViewModel.ASSET_ID, model.EPC_SIZE, _LoginInfo)
                '                    If (model.EPC_SIZE / 4) < sCheckID.Length Then
                '                        ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_ASSETID_LENGTH)
                '                    End If
                '                    _ViewModel.ASSET_ID = sCheckID
                '                    _ViewModel.GEN_ASSET_ID = sCheckID
                '                End If
                '            End If
                '    End Select

                '    '16進変換
                '    'If Not model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Inp Then　'2019/09/30 nv miyaji コメントアウト
                '    If _ViewModel.GEN_ASSET_ID = String.Empty Then
                '        _ViewModel.ASSET_ID = String.Empty
                '    Else
                '        Dim aryByte As Byte() = Encoding.UTF8.GetBytes(_ViewModel.GEN_ASSET_ID)
                '        _ViewModel.ASSET_ID = BitConverter.ToString(aryByte).Replace("-", String.Empty)
                '    End If
                '    '資産IDの長さチェック
                '    If (model.EPC_SIZE / 4) < _ViewModel.ASSET_ID.Length Then
                '        ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_ASSETID_LENGTH)
                '    End If
                '    'End If

                '    '資産状態の設定
                '    'UPDATE Inoue 2019/09/05
                '    If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.None OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Auto Then
                '        '生成しない(タグ紐付で行う)
                '        _ViewModel.ASSET_STATUS = ASSET_STATUS.Preparation
                '    Else
                '        _ViewModel.ASSET_STATUS = ASSET_STATUS.Storage
                '    End If

                '    'EPCエンコード状態の設定
                '    If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.None Then
                '        _ViewModel.EPC_ENC_STATUS = Misc.EpcEncStatus.Mi
                '    Else
                '        _ViewModel.EPC_ENC_STATUS = Misc.EpcEncStatus.Sumi
                '    End If

                '    'タグ紐付け状態の設定
                '    If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Inp OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.AssetNo Then
                '        _ViewModel.TAG_LINK_STATUS = TagLinkStatus.Sumi
                '    Else
                '        _ViewModel.TAG_LINK_STATUS = TagLinkStatus.Mi
                '    End If

                'End If

                '更新処理
                MyBase.BaseEdit(_ViewModel)

            End If

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                '登録処理
                If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then

                    '2023/02/08 Kawamoto ADD START
                    '薬品機能がONの場合
                    If DrugsFunc.DATA1 = DrugsFunction.Yes Then

                        Try
                            '薬品重量履歴の登録
                            Dao.ToolDrugDueLog.InsDrugDueLog(DrugsWorkDiv.Accept,
                                                             _ViewModel.ASSET_NO,
                                                             Dao.ToolEmployee.GetEmployeeDataEmployeeName(_ViewModel.FREE_ITEM9).Rows(0).Item("EMPLOYEE_CD"),
                                                             _ViewModel.FREE_ITEM9,
                                                             _ViewModel.FREE_ITEM14,
                                                             _ViewModel.FREE_ITEM16,             '返却残量
                                                             _LoginInfo,
                                                             Me._Message.getMessage(CommonLabelAssetList.TITLE),
                                                             _ViewModel.FREE_ITEM13                 '納入日
                                                             )

                        Catch ex As Exception
                            _tran.Rollback()
                            logger.Error(ex.Message)
                            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                        End Try
                    End If
                    '2023/02/08 Kawamoto ADD END

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.ASSET_NO).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.ASSET_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE), _ViewModel.LOC_L_CD, _ViewModel.LOC_M_CD, _ViewModel.LOC_S_CD)

                    _ViewModel.States = StatusType.SuccessCreate

                    '更新処理
                Else

                    '2023/02/08 Kawamoto ADD START
                    '薬品機能がONの場合
                    If DrugsFunc.DATA1 = DrugsFunction.Yes Then

                        '薬品重量履歴(薬品作業区分：受入以外)の取得
                        If Dao.ToolDrugDueLog.GetDrugDueLogNotAccept(_ViewModel.ASSET_NO).Rows.Count = 0 Then

                            'トランザクションスタート
                            Using tran = db.Database.BeginTransaction()
                                Try
                                    '薬品重量履歴(薬品作業区分：受入)の削除
                                    Dao.ToolDrugDueLog.DelDrugDueLogAccept(db, _ViewModel.ASSET_NO)

                                    '薬品重量履歴の登録
                                    Dao.ToolDrugDueLog.InsDrugDueLog(DrugsWorkDiv.Accept,
                                                                     _ViewModel.ASSET_NO,
                                                                     Dao.ToolEmployee.GetEmployeeDataEmployeeName(_ViewModel.FREE_ITEM9).Rows(0).Item("EMPLOYEE_CD"),
                                                                     _ViewModel.FREE_ITEM9,
                                                                     _ViewModel.FREE_ITEM14,
                                                                     _ViewModel.FREE_ITEM16,             '返却残量
                                                                     _LoginInfo,
                                                                     Me._Message.getMessage(CommonLabelAssetList.TITLE),
                                                                     _ViewModel.FREE_ITEM13                 '納入日
                                                                     )
                                    tran.Commit()

                                Catch ex As Exception
                                    _tran.Rollback()
                                    tran.Rollback()
                                    logger.Error(ex.Message)
                                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                                End Try
                            End Using
                        End If
                    End If
                    '2023/02/08 Kawamoto ADD END

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                 Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                 Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " &
                                                 CommonRfid.ToStringNvl(_ViewModel.ASSET_NO).Trim & "  " &
                                                 Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
                                                 CommonRfid.ToStringNvl(_ViewModel.ASSET_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE), _ViewModel.LOC_L_CD, _ViewModel.LOC_M_CD, _ViewModel.LOC_S_CD)

                    _ViewModel.States = StatusType.SuccessEdit

                End If
            Else
                If autoAssetNo Then
                    _ViewModel.ASSET_NO = "_"
                End If
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeCreate()

            '親の初期処理
            MyBase.InitializeCreate()

            Dim IsError As Boolean = False

            'カテゴリマスタデータを取得し、資産ID、EPC領域サイズを設定する
            'IDをキーにカテゴリーモデルを取得する
            Dim db As New SystemDBContext
            Dim findKeys As String() = {CAT_DIV.CategoryL, _ViewModel.CAT_L_CD, " ", " "}
            Dim model As CategoryModel = db.Category.Find(findKeys)

            If Not _ViewModel.CAT_L_CD = String.Empty Then
                '資産ＩＤを生成する
                '画面入力された資産IDを退避しておく
                Me._sBeforeAssetID = _ViewModel.ASSET_ID

                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                Select Case model.ASSET_ID_REG_DIV
                    Case AssetIDGenerationMethod.None, AssetIDGenerationMethod.Auto
                        '--2019/10/24 nv miyaji upd start　生成資産IDには資産IDと同じ値を入れる
                        _ViewModel.ASSET_ID = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, String.Empty, model.EPC_SIZE, _LoginInfo)
                        _ViewModel.GEN_ASSET_ID = _ViewModel.ASSET_ID
                        '_ViewModel.GEN_ASSET_ID = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, String.Empty, model.EPC_SIZE, _LoginInfo)
                        '--2019/10/241 nv miyaji upd end
                    Case AssetIDGenerationMethod.AssetNo
                        '--2019/10/24 nv miyaji upd start　生成資産IDには資産IDと同じ値を入れる
                        _ViewModel.ASSET_ID = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, _ViewModel.ASSET_NO, model.EPC_SIZE, _LoginInfo)
                        _ViewModel.GEN_ASSET_ID = _ViewModel.ASSET_ID
                        '_ViewModel.GEN_ASSET_ID = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, _ViewModel.ASSET_NO, model.EPC_SIZE, _LoginInfo)
                        '--2019/10/241 nv miyaji upd end
                    Case AssetIDGenerationMethod.Inp
                        If _ViewModel.ASSET_ID = String.Empty Then
                            ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_NOT_HEX)
                        Else
                            '16進文字列チェック
                            For Each Chr As Char In _ViewModel.ASSET_ID
                                If Not Uri.IsHexDigit(Chr) Then
                                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_NOT_HEX)
                                    IsError = True
                                    Exit For
                                End If
                            Next
                            '--2020/02/12 nv inoue del start  後のロジックで、桁数チェックするため、削除
                            'If Not IsError AndAlso Not _ViewModel.ASSET_ID.Length Mod 2 = 0 Then   '文字の長さが奇数の場合エラー
                            '    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_NOT_HEX)
                            '    IsError = True
                            'End If
                            '--2020/02/12 nv inoue del end
                            If Not IsError Then
                                ''資産IDの長さチェック
                                'Dim sCheckID As String = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, _ViewModel.ASSET_ID, model.EPC_SIZE, _LoginInfo)
                                'If (model.EPC_SIZE / 4) < sCheckID.Length Then
                                '    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_ASSETID_LENGTH)
                                'End If
                                '_ViewModel.ASSET_ID = sCheckID
                                '_ViewModel.GEN_ASSET_ID = sCheckID
                                '--2019/10/24 nv miyaji upd start　生成資産IDには資産IDと同じ値を入れる
                                '_ViewModel.GEN_ASSET_ID = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, Dao.ToolAsset.HexToStr(_ViewModel.ASSET_ID), model.EPC_SIZE, _LoginInfo) '2019/10/24 nv miyaji
                                _ViewModel.ASSET_ID = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, _ViewModel.ASSET_ID, model.EPC_SIZE, _LoginInfo) '2019/10/24 nv miyaji
                                _ViewModel.GEN_ASSET_ID = _ViewModel.ASSET_ID
                                '--2019/10/241 nv miyaji upd end
                            End If
                        End If
                        '--2020/02/10 miyaji add start
                    Case AssetIDGenerationMethod.GIAI
                        Dim Settinginfo As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.AssetIDGenerationCoPrefixGIAI.KeyDiv, CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key1, model.EPC_COMPANY_KEY, CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key3)
                        '設定マスタからデータを取得できなかった場合、エラー
                        If Settinginfo Is Nothing Then
                            Throw New Exception(String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), (CommonSettingKey.AssetIDGenerationCoPrefixGIAI.KeyDiv & " " & CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key1 & " " & model.EPC_COMPANY_KEY)))
                            'ModelState.AddModelError("ASSET_ID", String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), (CommonSettingKey.AssetIDGenerationCoPrefixGIAI.KeyDiv & " " & CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key1)))
                        Else

                            'シリアルNo取得
                            Dim iSerialNo As String = Dao.ToolAsset.GetSerialNo(AssetIDGenerationMethod.GIAI, Settinginfo.DATA1, "**", _LoginInfo)

                            '資産ID取得

                            Dim libEpc As AsetraLibEpc.LibEpc = New AsetraLibEpc.LibEpc
                            _ViewModel.ASSET_ID = libEpc.MakeCodeGIAI96(Settinginfo.DATA1, CLng(iSerialNo))
                            Select Case libEpc.Stat
                                Case AsetraLibEpc.LibEpc.EpcStatus.Ok
                                    _ViewModel.GEN_ASSET_ID = _ViewModel.ASSET_ID
                                Case AsetraLibEpc.LibEpc.EpcStatus.ErrCompanyPrefix
                                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_COM_CD)
                                    IsError = True
                                Case AsetraLibEpc.LibEpc.EpcStatus.ErrAssetType
                                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_ASSET_TYPE)
                                    IsError = True
                                Case AsetraLibEpc.LibEpc.EpcStatus.ErrSerialNo
                                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_SERIAL_NO)
                                    IsError = True
                                Case AsetraLibEpc.LibEpc.EpcStatus.ErrOther
                                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_SYSTEM)
                                    IsError = True
                            End Select
                        End If
                    Case AssetIDGenerationMethod.GRAI
                        Dim Settinginfo As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.AssetIDGenerationCoPrefixGRAI.KeyDiv, CommonSettingKey.AssetIDGenerationCoPrefixGRAI.Key1, model.EPC_COMPANY_KEY, CommonSettingKey.AssetIDGenerationCoPrefixGRAI.Key3)
                        '設定マスタからデータを取得できなかった場合、エラー
                        If Settinginfo Is Nothing Then
                            Throw New Exception(String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), (CommonSettingKey.AssetIDGenerationCoPrefixGRAI.KeyDiv & " " & CommonSettingKey.AssetIDGenerationCoPrefixGRAI.Key1 & " " & model.EPC_COMPANY_KEY)))
                            'ModelState.AddModelError("ASSET_ID", String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), (CommonSettingKey.AssetIDGenerationCoPrefixGIAI.KeyDiv & " " & CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key1)))
                        Else
                            'シリアルNo取得＆更新
                            Dim iSerialNo As String = Dao.ToolAsset.GetSerialNo(AssetIDGenerationMethod.GRAI, Settinginfo.DATA1, model.EPC_ASSET_TYPE, _LoginInfo)

                            '資産ID取得
                            Dim libEpc As AsetraLibEpc.LibEpc = New AsetraLibEpc.LibEpc
                            _ViewModel.ASSET_ID = libEpc.MakeCodeGRAI96(Settinginfo.DATA1, model.EPC_ASSET_TYPE, CLng(iSerialNo))
                            Select Case libEpc.Stat
                                Case AsetraLibEpc.LibEpc.EpcStatus.Ok
                                    _ViewModel.GEN_ASSET_ID = _ViewModel.ASSET_ID
                                Case AsetraLibEpc.LibEpc.EpcStatus.ErrCompanyPrefix
                                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_COM_CD)
                                    IsError = True
                                Case AsetraLibEpc.LibEpc.EpcStatus.ErrAssetType
                                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_ASSET_TYPE)
                                    IsError = True
                                Case AsetraLibEpc.LibEpc.EpcStatus.ErrSerialNo
                                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_SERIAL_NO)
                                    IsError = True
                                Case AsetraLibEpc.LibEpc.EpcStatus.ErrOther
                                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_SYSTEM)
                                    IsError = True
                            End Select
                        End If

                        '--2020/02/10 miyaji add end
                End Select

                '--2019/10/24 nv miyaji コメントアウト start
                ''16進変換
                ''If Not model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Inp Then   '2019/09/30 nv miyaji コメントアウト
                'If _ViewModel.GEN_ASSET_ID = String.Empty Then
                '    _ViewModel.ASSET_ID = String.Empty
                'Else
                '    Dim aryByte As Byte() = Encoding.UTF8.GetBytes(_ViewModel.GEN_ASSET_ID)
                '    _ViewModel.ASSET_ID = BitConverter.ToString(aryByte).Replace("-", String.Empty)
                'End If
                '--2019/10/24 nv miyaji コメントアウト end

                '資産IDの長さチェック
                If Not IsError AndAlso Not _ViewModel.ASSET_ID = String.Empty AndAlso (model.EPC_SIZE / 4) <> _ViewModel.ASSET_ID.Length Then
                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_ASSETID_LENGTH)
                End If

                'EPC領域サイズを設定する
                _ViewModel.EPC_SIZE = model.EPC_SIZE

                '貸出回数・資産状態・EPCエンコード状態・タグ紐付け状態を設定する
                '_ViewModel.LEASE_CNT = 0

                'UPDATE Inoue 2019/09/05
                'If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.None OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Auto Then '2020/02/13 nv miyaji
                If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.None OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Auto OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.GIAI OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.GRAI Then '2020/02/13 nv miyaji
                    '生成しない(タグ紐付で行う) 
                    _ViewModel.ASSET_STATUS = ASSET_STATUS.Preparation
                Else
                    '以外
                    _ViewModel.ASSET_STATUS = ASSET_STATUS.Storage
                End If

                'EPCエンコード状態の設定
                If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.None Then
                    _ViewModel.EPC_ENC_STATUS = Misc.EpcEncStatus.Mi
                Else
                    _ViewModel.EPC_ENC_STATUS = Misc.EpcEncStatus.Sumi
                End If

                'タグ紐付け状態の設定
                If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Inp OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.AssetNo Then
                    _ViewModel.TAG_LINK_STATUS = TagLinkStatus.Sumi
                Else
                    _ViewModel.TAG_LINK_STATUS = TagLinkStatus.Mi
                End If
                '_ViewModel.TAG_LINK_STATUS = Misc.TagLinkStatus.Mi

            End If


            'モデルを登録用モデルにコピー
            CopyViewModeltoModel(_ViewModel)

            'モデルを処理履歴登録用モデルにコピー
            CopyViewModeltoRireki(_ViewModel, ProcessDiv.MI)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeCreate()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_ASSET_LIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeEdit()

            '親の初期処理
            MyBase.InitializeCreate()

            If _blRetireFlg Then
                '除却/除却解除ボタン押下
                'DBのデータを取得する
                Dim db As New SystemDBContext
                _Model = db.AssetList.Find(_ViewModel.ASSET_NO)
                '--2019/10/11 nv miyaji add start 更新日時チェックのため、更新日時は画面取得時のものを入れる
                _Model.UPD_DATETIME = _ViewModel.UPD_DATETIME
                '--2019/10/11 nv miyaji add end

                'IDをキーにカテゴリーモデルを取得する
                Dim findKeys As String() = {CAT_DIV.CategoryL, _ViewModel.CAT_L_CD, " ", " "}
                Dim model As CategoryModel = db.Category.Find(findKeys)

                If Not _ViewModel.RETIRE_TS Is Nothing Then
                    '除却解除処理時、資産ID再作成
                    If Not _ViewModel.RETIRE_TS Is Nothing Then
                        '資産ＩＤを生成する
                        '--2019/10/24 nv miyaji upd start
                        'If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Inp Then　'2019/03/11 nv miyaji　コメントアウト
                        '    _Model.ASSET_ID = _ViewModel.GEN_ASSET_ID
                        'Else
                        'If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Auto Then
                        '    _Model.GEN_ASSET_ID = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, String.Empty, model.EPC_SIZE, _LoginInfo)
                        'End If
                        ''16進変換
                        'If _ViewModel.GEN_ASSET_ID = String.Empty Then
                        '    _Model.ASSET_ID = String.Empty
                        'Else
                        '    Dim aryByte As Byte() = Encoding.UTF8.GetBytes(_ViewModel.GEN_ASSET_ID)
                        '    _Model.ASSET_ID = BitConverter.ToString(aryByte).Replace("-", String.Empty)
                        'End If
                        '方法３の時のみ再作成、それ以外の時は生成資産IDをそのまま資産IDに入れる
                        If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Auto Then
                            _Model.ASSET_ID = Dao.ToolAsset.MakeAssetId(model.ASSET_ID_REG_DIV, String.Empty, model.EPC_SIZE, _LoginInfo)
                            _Model.GEN_ASSET_ID = _Model.ASSET_ID
                        Else
                            _Model.ASSET_ID = _Model.GEN_ASSET_ID
                        End If
                        '--2019/10/24 nv miyaji upd end
                    End If
                    'End If

                    '資産状態の設定
                    If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.None OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Auto OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.GIAI OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.GRAI Then    '2020/02/17 nv miyaji
                        'If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.None OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Auto Then    '2020/02/17 nv miyaji
                        _Model.ASSET_STATUS = ASSET_STATUS.Preparation
                    Else
                        _Model.ASSET_STATUS = ASSET_STATUS.Storage
                    End If

                    'EPCエンコード状態の設定
                    If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.None Then
                        _Model.EPC_ENC_STATUS = Misc.EpcEncStatus.Mi
                    Else
                        _Model.EPC_ENC_STATUS = Misc.EpcEncStatus.Sumi
                    End If

                    'タグ紐付け状態の設定
                    If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Inp OrElse model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.AssetNo Then
                        _Model.TAG_LINK_STATUS = TagLinkStatus.Sumi
                    Else
                        _Model.TAG_LINK_STATUS = TagLinkStatus.Mi
                    End If

                Else
                    '除却時
                    'ＩＤ生成方法①の時、生成資産ＩＤをクリアする
                    If model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.None Then
                        _Model.GEN_ASSET_ID = String.Empty
                    End If
                End If

                'モデルを処理履歴登録用モデルにコピー
                CopyModeltoRireki(_Model, ProcessDiv.MU)

            Else
                '変更ボタン押下

                'モデルを登録用モデルにコピー
                CopyViewModeltoModel(_ViewModel)

                'モデルを処理履歴登録用モデルにコピー
                CopyViewModeltoRireki(_ViewModel, ProcessDiv.MU)

            End If

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeEdit()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_ASSET_LIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            ''2023/02/08 Kawamoto ADD START
            ''設定マスタ（薬品管理機能）の取得
            'Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
            '                                                          CommonSettingKey.Drugs_Function.Key1,
            '                                                          CommonSettingKey.Drugs_Function.Key2,
            '                                                          CommonSettingKey.Drugs_Function.Key3, True)
            ''薬品機能がONの場合
            'If DrugsFunc.DATA1 = DrugsFunction.Yes Then
            '    '薬品機能の入力チェック
            '    If Not InputChkDrugsFunc() Then
            '        blnReturn = False
            '    End If
            'End If
            ''2023/02/08 Kawamoto ADD END

            '資産番号が登録済みかチェックする
            Dim model As AssetListModel = db.AssetList.Find(_Model.ASSET_NO)

            '重複エラーの場合、エラー情報を保持する
            If Not IsNothing(model) Then
                ModelState.AddModelError("ASSET_NO", CommonLabel.MSG_ERR_DATAJUUFUKU)
                blnReturn = False
            End If

            '--2019/10/04 nv miyaji add start
            '選択しているカテゴリーの資産ID生成方法が方法２、方法３の場合、資産IDが入力されていたらエラー
            'IDをキーにカテゴリーモデルを取得する
            If ModelState.IsValid Then  '資産IDに既にAddModelErrorされていなければチェック   '2020/02/12 nv miyaji
                Dim findKeys As String() = {CAT_DIV.CategoryL, _ViewModel.CAT_L_CD, " ", " "}
                Dim catmodel As CategoryModel = db.Category.Find(findKeys)
                If Not catmodel.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Inp Then
                    If Not String.IsNullOrEmpty(_sBeforeAssetID) Then
                        Select Case catmodel.ASSET_ID_REG_DIV
                            Case AssetIDGenerationMethod.AssetNo
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_HOUHOU2_INPUT)
                            Case AssetIDGenerationMethod.Auto
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_HOUHOU3_INPUT)
                            Case AssetIDGenerationMethod.None
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_HOUHOU1_INPUT)
                                '--2020/02/17 nv miyaji add start
                            Case AssetIDGenerationMethod.GIAI
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_HOUHOU5_INPUT)
                            Case AssetIDGenerationMethod.GRAI
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_HOUHOU6_INPUT)
                                '--2020/02/17 nv miyaji add end
                        End Select
                        blnReturn = False
                    End If
                End If
            End If

            '資産番号、サブ資産番号が既に使われている場合、エラー情報を保持する
            If Dao.ToolAsset.IsExistsSubAssetNo(_Model.ASSET_NO, _Model.SUB_ASSET_NO) Then
                ModelState.AddModelError("ASSET_NO", CommonErrorMsg.MSG_ERRNO_ASSETNO_JUUFUKU)
                blnReturn = False
            End If

            '資産IDが既に使われている場合、エラー情報を保持する
            If ModelState.IsValid Then  '資産IDに既にAddModelErrorされていなければチェック
                If Not String.IsNullOrEmpty(_Model.ASSET_ID) Then
                    If Dao.ToolAsset.IsExistsAssetId(_Model.ASSET_NO, _Model.ASSET_ID) Then
                        ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_DATAJUUFUKU)
                        blnReturn = False
                    End If
                End If
            End If

            '2021/11/25 nv miyaji 0の時エラーにしないよう修正
            ''メンテ発生貸出日数が０の場合エラー
            'If _Model.MAINTE_LEASE_DAY IsNot Nothing AndAlso _Model.MAINTE_LEASE_DAY = 0 Then
            '    ModelState.AddModelError("MAINTE_LEASE_DAY", CommonLabel.MSG_ERR_NISSU_ZERO)
            '    blnReturn = False
            'End If
            ''メンテ日数が０の場合エラー
            'If _Model.MAINTE_DAY IsNot Nothing AndAlso _Model.MAINTE_DAY = 0 Then
            '    ModelState.AddModelError("MAINTE_DAY", CommonLabel.MSG_ERR_NISSU_ZERO)
            '    blnReturn = False
            'End If
            'メンテ発生貸出日数が空欄でメンテ発生貸出日数稼働日フラグがチェックされている場合エラー
            If _Model.MAINTE_LEASE_DAY Is Nothing AndAlso _Model.MAINTE_LEASE_DAY_WORK_FLAG = MainteWorkFlg.Yes Then
                ModelState.AddModelError("MAINTE_LEASE_DAY", CommonLabel.MSG_ERR_NOT_ENTERD)
                blnReturn = False
            End If
            'メンテ日数が空欄でメンテ日数稼働日フラグがチェックされている場合エラー
            If _Model.MAINTE_DAY Is Nothing AndAlso _Model.MAINTE_DAY_WORK_FLAG = MainteWorkFlg.Yes Then
                ModelState.AddModelError("MAINTE_DAY", CommonLabel.MSG_ERR_NOT_ENTERD)
                blnReturn = False
            End If

            'リース期限、○○期限 1000年以上チェック
            If _Model.LEASE_LIMIT < MIN_DATE Then
                ModelState.AddModelError("LeaseLimitString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If _Model.GENERIC_LIMIT < MIN_DATE Then
                ModelState.AddModelError("GenLimitString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            'エラーがあった場合、資産IDを画面入力値に戻す
            If Not ModelState.IsValid Then
                _ViewModel.ASSET_ID = _sBeforeAssetID
                blnReturn = False  '2020/02/10 miyaji add エラーがあった場合、Falseを返す
            End If

            '--2019/10/04 nv miyaji add end

            '--20211130 sv suzuki add start
            If _ViewModel.ErrCode = CommonLabel.MSG_ERR_PIC_FILE_NOT_FOUND Then
                ModelState.AddModelError("ASSET_PIC_FILENAME", CommonLabel.MSG_ERR_PIC_FILE_NOT_FOUND)
                blnReturn = False
            End If
            '--20211130 sv suzuki add end

            '2023/02/08 Kawamoto ADD START
            'ロケごとの資産登録上限数を超えている場合エラー
            If _ViewModel.ErrCode = CommonLabel.MSG_ERR_EXCEED_MAX_NUM Then
                Dim loc As String = _RegistInfo(0)("TargetLoc")
                Dim message As String = Me._Message.getMessage(CommonLabel.MSG_ERR_EXCEED_MAX_NUM).Replace("{0}", _RegistInfo(0)("LocLCode")).Replace("{1}", _RegistInfo(0)("RegistNum")).Replace("{2}", _RegistInfo(0)("RegistLimit"))
                ModelState.AddModelError(loc, message)
                blnReturn = False
            End If

            'If blnReturn Then
            ''薬品機能がONの場合
            'If DrugsFunc.DATA1 = DrugsFunction.Yes Then
            '    'FreeItem(購入日)を設定する
            '    If String.IsNullOrEmpty(_ViewModel.FREE_ITEM13) Then
            '        _Model.FREE_ITEM13 = Nothing
            '    Else
            '        _Model.FREE_ITEM13 = CDate(_ViewModel.FREE_ITEM13).ToString("yyyy/MM/dd")
            '    End If
            'End If
            'End If
            '2023/02/08 Kawamoto ADD END

            Return blnReturn

        End Function

        ''' <summary>
        ''' 登録処理をコンテキストに追加します
        ''' </summary>
        Protected Overrides Sub CreateQuery()

            '--2020/02/12 nv inoue add start  棚卸開始チェック
            If Dao.ToolCommon.CheckInventorytStart() <> 0 And _Model.ASSET_STATUS = Misc.ASSET_STATUS.Storage Then
                _Model.PHYINV_STATUS = Misc.PHYINV_STATUS.NoSpot
            End If
            '--2020/02/12 nv inoue add end

            '登録日時・登録社員コード・登録社員名・登録プロセスを登録
            _Model.INS_DATETIME = CommonRfid.getNowDate
            _Model.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelAssetList.TITLE)

            '処理日時を登録
            _Model.TASK_TS = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))

            '資産マスタの更新
            _db.AssetList.Add(_Model)

            '履歴の更新日時・更新社員コード・更新社員名・更新プロセスを登録
            _HistoryModel.UPD_DATETIME = CommonRfid.getNowDate
            _HistoryModel.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _HistoryModel.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelAssetList.TITLE)

            '資産履歴テーブルの更新
            _db.AssetHistory.Add(_HistoryModel)

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            '2023/02/08 Kawamoto ADD START
            ''設定マスタ（薬品管理機能）の取得
            'Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
            '                                                          CommonSettingKey.Drugs_Function.Key1,
            '                                                          CommonSettingKey.Drugs_Function.Key2,
            '                                                          CommonSettingKey.Drugs_Function.Key3, True)
            ''薬品機能がONの場合
            'If DrugsFunc.DATA1 = DrugsFunction.Yes Then
            '    '薬品機能の入力チェック
            '    If Not InputChkDrugsFunc() Then
            '        blnReturn = False
            '    End If
            'End If
            '2023/02/08 Kawamoto ADD END

            'DBのデータを取得する
            Dim model As AssetListModel = db.AssetList.Find(_Model.ASSET_NO)

            '他端末で更新されているかチェックする
            If _Model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") <> model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") Then
                'モデルとDBの更新日付が異なっている場合エラー
                ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                blnReturn = False
            End If

            '資産IDが既に使われている場合、エラー情報を保持する
            If Not String.IsNullOrEmpty(_Model.ASSET_ID) Then
                If Dao.ToolAsset.IsExistsAssetId(_Model.ASSET_NO, _Model.ASSET_ID) Then
                    ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_DATAJUUFUKU)
                    blnReturn = False
                End If
            End If

            '資産番号、サブ資産番号が既に使われている場合、エラー情報を保持する
            If Dao.ToolAsset.IsExistsSubAssetNo(_Model.ASSET_NO, _Model.SUB_ASSET_NO) Then
                ModelState.AddModelError("SUB_ASSET_NO", CommonErrorMsg.MSG_ERRNO_ASSETNO_JUUFUKU)
                blnReturn = False
            End If

            '2021/11/25 nv miyaji 0の時エラーにしないよう修正
            ''メンテ発生貸出日数が０の場合エラー
            'If _Model.MAINTE_LEASE_DAY IsNot Nothing AndAlso _Model.MAINTE_LEASE_DAY = 0 Then
            '    ModelState.AddModelError("MAINTE_LEASE_DAY", CommonLabel.MSG_ERR_NISSU_ZERO)
            '    blnReturn = False
            'End If
            ''メンテ日数が０の場合エラー
            'If _Model.MAINTE_DAY IsNot Nothing AndAlso _Model.MAINTE_DAY = 0 Then
            '    ModelState.AddModelError("MAINTE_DAY", CommonLabel.MSG_ERR_NISSU_ZERO)
            '    blnReturn = False
            'End If
            'メンテ発生貸出日数が空欄でメンテ発生貸出日数稼働日フラグがチェックされている場合エラー
            If _Model.MAINTE_LEASE_DAY Is Nothing AndAlso _Model.MAINTE_LEASE_DAY_WORK_FLAG = MainteWorkFlg.Yes Then
                ModelState.AddModelError("MAINTE_LEASE_DAY", CommonLabel.MSG_ERR_NOT_ENTERD)
                blnReturn = False
            End If
            'メンテ日数が空欄でメンテ日数稼働日フラグがチェックされている場合エラー
            If _Model.MAINTE_DAY Is Nothing AndAlso _Model.MAINTE_DAY_WORK_FLAG = MainteWorkFlg.Yes Then
                ModelState.AddModelError("MAINTE_DAY", CommonLabel.MSG_ERR_NOT_ENTERD)
                blnReturn = False
            End If

            'リース期限、○○期限 1000年以上チェック
            If _Model.LEASE_LIMIT < MIN_DATE Then
                ModelState.AddModelError("LeaseLimitString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If _Model.GENERIC_LIMIT < MIN_DATE Then
                ModelState.AddModelError("GenLimitString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            '--20211130 sv suzuki add start
            If _ViewModel.ErrCode = CommonLabel.MSG_ERR_PIC_FILE_NOT_FOUND Then
                ModelState.AddModelError("ASSET_PIC_FILENAME", CommonLabel.MSG_ERR_PIC_FILE_NOT_FOUND)
                blnReturn = False
            End If
            '--20211130 sv suzuki add end

            '2023/02/08 Kawamoto ADD START
            'ロケごとの資産登録上限数を超えている場合エラー
            If _ViewModel.ErrCode = CommonLabel.MSG_ERR_EXCEED_MAX_NUM Then
                Dim loc As String = _RegistInfo(0)("TargetLoc")
                Dim message As String = Me._Message.getMessage(CommonLabel.MSG_ERR_EXCEED_MAX_NUM).Replace("{0}", _RegistInfo(0)("LocLCode")).Replace("{1}", _RegistInfo(0)("RegistNum")).Replace("{2}", _RegistInfo(0)("RegistLimit"))
                ModelState.AddModelError(loc, message)
                blnReturn = False
            End If

            'If blnReturn Then
            '    '薬品機能がONの場合
            '    If DrugsFunc.DATA1 = DrugsFunction.Yes Then
            '        'FreeItem(購入日)を設定する
            '        If String.IsNullOrEmpty(_ViewModel.FREE_ITEM13) Then
            '            _Model.FREE_ITEM13 = Nothing
            '        Else
            '            _Model.FREE_ITEM13 = CDate(_ViewModel.FREE_ITEM13).ToString("yyyy/MM/dd")
            '        End If
            '    End If
            'End If
            '2023/02/08 Kawamoto ADD END

            Return blnReturn
        End Function

        ''' <summary>
        ''' 更新処理のコンテキストを変更します
        ''' </summary>
        Protected Overrides Sub EditQuery()
            '更新日時・更新社員コード・更新社員名・更新プロセスを登録
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelAssetList.TITLE)

            '履歴の更新日時・更新社員コード・更新社員名・更新プロセスを登録
            _HistoryModel.UPD_DATETIME = CommonRfid.getNowDate
            _HistoryModel.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _HistoryModel.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelAssetList.TITLE)

            If _blRetireFlg Then
                '除却/除却解除ボタン押下処理
                If _Model.RETIRE_TS Is Nothing Then
                    '除却処理
                    '資産マスタ
                    '除却日・除却社員コード・除却社員名・除却資産ＩＤを設定
                    _Model.RETIRE_TS = CDate(CommonRfid.getNowDate).ToString("yyyyMMddHHmmss")
                    _Model.RETIRE_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                    _Model.RETIRE_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                    _Model.RETIRE_ASSET_ID = _ViewModel.ASSET_ID

                    '資産状態を除却済みにする
                    _Model.ASSET_STATUS = ASSET_STATUS.Removed

                    '資産IDをクリア
                    _Model.ASSET_ID = String.Empty
                    '_Model.GEN_ASSET_ID = String.Empty

                    '_Model.LEASE_CNT = 0
                    _Model.EPC_ENC_STATUS = EpcEncStatus.Mi
                    _Model.TAG_LINK_STATUS = TagLinkStatus.Mi
                    _Model.LEASE_PLACE_CD = Nothing
                    _Model.LEASE_PLACE_NAME = Nothing
                    _Model.REMARK = Nothing
                    _Model.LEASE_EMPLOYEE_CD = Nothing
                    _Model.LEASE_EMPLOYEE_NAME = Nothing
                    _Model.LEASE_DT = Nothing
                    _Model.LEASE_DUE_DT = Nothing
                    _Model.PHYINV_TS = Nothing
                    _Model.PHYINV_STATUS = Nothing
                    _Model.PHYINV_LOC_L_CD = Nothing
                    _Model.PHYINV_LOC_M_CD = Nothing
                    _Model.PHYINV_LOC_S_CD = Nothing
                    _Model.PHYINV_LOC_L_NAME = Nothing
                    _Model.PHYINV_LOC_M_NAME = Nothing
                    _Model.PHYINV_LOC_S_NAME = Nothing
                    _Model.PHYINV_EMPLOYEE_CD = Nothing
                    _Model.PHYINV_EMPLOYEE_NAME = Nothing

                    '履歴
                    '資産IDをクリア
                    _HistoryModel.ASSET_ID = String.Empty

                Else
                    '除却解除処理
                    '除却日・除却社員コード・除却社員名・除却資産ＩＤを設定

                    '--2020/02/12 nv inoue add start  棚卸開始チェック
                    If Dao.ToolCommon.CheckInventorytStart() <> 0 And _Model.ASSET_STATUS = Misc.ASSET_STATUS.Storage Then
                        _Model.PHYINV_STATUS = Misc.PHYINV_STATUS.NoSpot
                    End If
                    '--2020/02/12 nv inoue add end

                    _Model.RETIRE_TS = Nothing
                    _Model.RETIRE_EMPLOYEE_CD = Nothing
                    _Model.RETIRE_EMPLOYEE_NAME = Nothing
                    _Model.RETIRE_ASSET_ID = Nothing

                End If
            End If

            '処理日時を登録
            _Model.TASK_TS = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))

            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified

            '資産履歴テーブルの更新
            _db.AssetHistory.Add(_HistoryModel)

        End Sub

        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="ID">ID</param>
        ''' <returns>View表示内容</returns>
        Public Function GetEditData(ByVal ID As String) As JsonResult
            Dim db As New SystemDBContext

            'ログイン情報を取得。
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)

            'IDをキーにモデルを取得する
            Dim model As AssetListModel = db.AssetList.Find(ID)
            model.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")

            '2019/09/16 nv miyaji
            ''■■■　画像ファイル名をセッションに保存
            'If String.IsNullOrEmpty(model.ASSET_PIC_FILENAME) Then
            '    Session.Remove(CommonSessionKey.ASSET_IMAGEFILE_KEY)
            'Else
            '    Session(CommonSessionKey.ASSET_IMAGEFILE_KEY) = model.ASSET_PIC_FILENAME
            'End If

            ' ロケーションドロップダウンリスト作成
            GetModalLocationData(model)

            ' カテゴリードロップダウンリスト作成
            GetModalCategoryData(model)

            ' 管理ロケーションドロップダウンリスト作成
            GetModalMngLocationData(model)

            'IDをキーにカテゴリーモデルを取得する
            Dim findKeys As String() = {CAT_DIV.CategoryL, model.CAT_L_CD, " ", " "}
            Dim catmodel As CategoryModel = db.Category.Find(findKeys)
            '2019/09/30 nv miyaji　方法④で除却でない資産も資産IDは触れなくするように修正
            'If catmodel.ASSET_ID_REG_DIV = AssetIDGenerationMethod.Inp AndAlso model.RETIRE_TS Is Nothing Then
            '    model.AssetIdDivReg = True
            'Else
            model.AssetIdDivReg = False
            'End If

            '除却日時をyyyy/MM/dd HH:mmで表示する
            If Not model.RETIRE_TS Is Nothing Then
                Dim sRetireTs As String = String.Empty
                sRetireTs = CommonRfid.ToStringLongYMDHM(model.RETIRE_TS)
                model.RetireTsString = sRetireTs
            End If

            'リース期限をyyyy/MM/ddで表示する
            If Not model.LEASE_LIMIT Is Nothing Then
                If model.LEASE_LIMIT.ToString.Length <> itemlength.LeaseLimit Then
                    model.LeaseLimitString = String.Empty
                Else
                    Dim sLeaseLimit As String = String.Empty
                    sLeaseLimit = CommonRfid.ToStringIntYMD(model.LEASE_LIMIT)
                    model.LeaseLimitString = CommonRfid.ToHaifunYMD(sLeaseLimit)
                End If
            End If


            '○○期限をyyyy/MM/ddで表示する
            If Not model.GENERIC_LIMIT Is Nothing Then
                If model.GENERIC_LIMIT.ToString.Length <> itemlength.GenLimit Then
                    model.GenLimitString = String.Empty
                Else
                    Dim sGenLimit As String = String.Empty
                    sGenLimit = CommonRfid.ToStringIntYMD(model.GENERIC_LIMIT)
                    model.GenLimitString = CommonRfid.ToHaifunYMD(sGenLimit)
                End If
            End If

            ''資産購入日をyyyy/MM/ddで表示する
            'If Not model.FREE_ITEM1 Is Nothing Then
            '    If model.FREE_ITEM1.ToString.Length <> itemlength.AssetKounyuDate Then
            '        model.AssetKounyuDateString = String.Empty
            '    Else
            '        model.AssetKounyuDateString = CommonRfid.ToHaifunYMD(model.FREE_ITEM1)
            '    End If
            'End If

            '2023/02/08 Kawamoto ADD START
            '設定マスタ（薬品管理機能）の取得
            'Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
            '                                                          CommonSettingKey.Drugs_Function.Key1,
            '                                                          CommonSettingKey.Drugs_Function.Key2,
            '                                                          CommonSettingKey.Drugs_Function.Key3, True)
            'If DrugsFunc.DATA1 = DrugsFunction.Yes Then
            '    model.DrugsFunc = True

            '    'FreeItem13(購入日)をyyyy/MM/ddで表示する
            '    If model.FREE_ITEM13.ToString.Length <> itemlength.KounyuDate Then
            '        model.FREE_ITEM13 = String.Empty
            '    Else
            '        model.FREE_ITEM13 = CommonRfid.ToHaifunYMD(model.FREE_ITEM13)
            '    End If

            '    '2023/05/10 Kawamoto ADD START
            '    '計量器連携あり情報の取得
            '    model.WeighingMachineLinkList = Dao.ToolSetting.GetWeighingMachineLinkList()
            '    '2023/05/10 Kawamoto ADD END

            'End If
            '2023/02/08 Kawamoto ADD END

            '2023/03/22 Kawamoto ADD START
            '添付ファイル件数の取得
            Dim sAttachmentPath As String = ConfigurationManager.AppSettings("AttachmentFolderPath")
            Dim sFolderPath As String = Path.Combine(sAttachmentPath, CommonRfid.FncStringToHex(model.ASSET_NO))
            model.ATTACHMENT_NUM = SelectAttachmentList(ConvertEnablePath(sFolderPath), model.ASSET_NO).Count
            '2023/03/22 Kawamoto ADD END

            Return Json(model)

        End Function

#End Region

        '2023/02/08 Kawamoto ADD START
#Region "薬品機能の入力チェック"
        ''' <summary>
        ''' 薬品機能の入力チェック
        ''' <returns>True:チェックOK False:チェックNG</returns>
        ''' </summary>
        Private Function InputChkDrugsFunc() As Boolean
            Dim blnReturn As Boolean = True

            'MODEL_NO(ロットNo)
            '必須チェック
            If _Model.MODEL_NO Is Nothing Then
                ModelState.AddModelError("MODEL_NO", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If

            'FreeItem2(CasNo)
            '必須チェック
            If _Model.FREE_ITEM2 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM2", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If

            'FreeItem3(消防法の分類)
            '必須チェック
            If _Model.FREE_ITEM3 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM3", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If

            '2023/05/10 Kawamoto ADD START
            'FreeItem4(有機則の該当有無)
            '必須チェック
            If _Model.FREE_ITEM4 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM4", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If

            'FreeItem5(毒物及び劇物取扱法の該当)
            '必須チェック
            If _Model.FREE_ITEM5 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM5", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If

            'FreeItem6(特化則の該当有無)
            '必須チェック
            If _Model.FREE_ITEM6 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM6", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If

            'FreeItem7(PRTR制度の該当有無)
            '必須チェック
            If _Model.FREE_ITEM7 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM7", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If

            'FreeItem8(科学物質ﾘｽｸｱｾｽﾒﾝﾄ実施義務の有無)
            '必須チェック
            If _Model.FREE_ITEM8 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM8", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If
            '2023/05/10 Kawamoto ADD END

            'FreeItem9(購入者)
            '必須チェック
            If _Model.FREE_ITEM9 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM9", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If

            'FreeItem10(内容量(kg))とFreeItem11(内容量(L))
            If _Model.FREE_ITEM3 IsNot Nothing Then
                '未入力チェック
                If String.IsNullOrEmpty(_Model.FREE_ITEM10) AndAlso String.IsNullOrEmpty(_Model.FREE_ITEM11) Then
                    ModelState.AddModelError("FREE_ITEM10", CommonLabel.MSG_ERR_KG_OR_L_ONE_SIDE_INPUT)
                    Return False
                End If

                '両方入力済みチェック
                If Not String.IsNullOrEmpty(_Model.FREE_ITEM10) AndAlso Not String.IsNullOrEmpty(_Model.FREE_ITEM11) Then
                    ModelState.AddModelError("FREE_ITEM10", CommonLabel.MSG_ERR_KG_OR_L_ONE_SIDE_INPUT)
                    Return False
                End If

                'FreeItem10(内容量(kg))
                If Not String.IsNullOrEmpty(_Model.FREE_ITEM10) Then
                    '小数値チェック
                    Dim ret As String = CommonRfid.IsDecimal(_Model.FREE_ITEM10, 5, 3, False)
                    If Not String.IsNullOrEmpty(ret) Then
                        ModelState.AddModelError("FREE_ITEM10", ret)
                        Return False
                    End If
                End If

                'FreeItem11(内容量(L))
                If Not String.IsNullOrEmpty(_Model.FREE_ITEM11) Then
                    '小数値チェック
                    Dim ret As String = CommonRfid.IsDecimal(_Model.FREE_ITEM11, 5, 3, False)
                    If Not String.IsNullOrEmpty(ret) Then
                        ModelState.AddModelError("FREE_ITEM11", ret)
                        Return False
                    End If
                End If

            End If

            'FreeItem12(比重又は密度)
            '必須チェック
            If _Model.FREE_ITEM12 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM12", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            Else
                '小数値チェック
                Dim ret As String = CommonRfid.IsDecimal(_Model.FREE_ITEM12, 5, 3, False)
                If Not String.IsNullOrEmpty(ret) Then
                    ModelState.AddModelError("FREE_ITEM12", ret)
                    Return False
                End If
            End If

            'FreeItem13(納入日)
            '必須チェック
            If _Model.FREE_ITEM13 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM13", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If

            'FreeItem14(受入時の容器を含む重量(kg))
            '必須チェック
            If _Model.FREE_ITEM14 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM14", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            Else
                '小数値チェック
                Dim ret As String = CommonRfid.IsDecimal(_Model.FREE_ITEM14, 5, 3, False)
                If Not String.IsNullOrEmpty(ret) Then
                    ModelState.AddModelError("FREE_ITEM14", ret)
                    Return False
                End If
            End If

            'FreeItem15(計量器)
            '必須チェック
            If _Model.FREE_ITEM15 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM15", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            End If

            'FreeItem16(残量(NET重量))
            '必須チェック
            If _Model.FREE_ITEM16 Is Nothing Then
                ModelState.AddModelError("FREE_ITEM16", CommonLabel.MSG_ERR_NOT_ENTERD)
                Return False
            Else
                '小数値チェック
                Dim ret As String = CommonRfid.IsDecimal(_Model.FREE_ITEM16, 5, 3, False)
                If Not String.IsNullOrEmpty(ret) Then
                    ModelState.AddModelError("FREE_ITEM16", ret)
                    Return False
                End If
            End If

            Return blnReturn
        End Function
#End Region
        '2023/02/08 Kawamoto ADD END

#Region "削除処理"
        ''' <summary>
        ''' アクションメソッド(Delete)
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Overridable Function Delete(ByVal ViewModel As AssetListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'ロケーションコード・カテゴリコードが空の場合、空白を設定する
            If String.IsNullOrEmpty(_ViewModel.LOC_L_CD) Then
                _ViewModel.LOC_L_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.LOC_M_CD) Then
                _ViewModel.LOC_M_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.LOC_S_CD) Then
                _ViewModel.LOC_S_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.CAT_M_CD) Then
                _ViewModel.CAT_M_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.CAT_S_CD) Then
                _ViewModel.CAT_S_CD = " "
            End If
            '--2021/09/17 nv miyaji add start
            '管理ロケーションコードが空の場合、空白を設定する。
            If String.IsNullOrEmpty(_ViewModel.MNG_LOC_L_CD) Then
                _ViewModel.MNG_LOC_L_CD = " "
            End If
            '--2021/09/17 nv miyaji add end

            '画像削除時に使用
            Dim sAssetNo As String = _ViewModel.ASSET_NO
            Dim sPicFilNm As String = CommonRfid.ToStringNvl(_ViewModel.ASSET_PIC_FILENAME)

            '2020/10/22 戸祭 DBトランザクションによるロック回避のため、先に画像の利用チェックを行う
            Dim bPicUsed As Boolean = Dao.ToolAsset.IsUseImg(sAssetNo, sPicFilNm)

            '削除処理
            MyBase.BaseDelete(_ViewModel)

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                _ViewModel.States = StatusType.SuccessDelete

                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '資産画像が設定されていたら削除
                If sPicFilNm <> String.Empty Then
                    '画像ファイル保存場所取得
                    Dim Setting_Image As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Image.KeyDiv, CommonSettingKey.Image.Key1, CommonSettingKey.Image.Key2, CommonSettingKey.Image.Key3, True)
                    Dim sImagePath As String = Setting_Image.DATA1
                    Dim sFullPath As String = Server.MapPath(CommonConst.ROOT_FOLDERNM & sImagePath)
                    If Not Directory.Exists(sFullPath) Then
                        Directory.CreateDirectory(sFullPath)
                    End If

                    '変更前の画像ファイルが他の資産で使われていなければ削除
                    '2020/10/22 戸祭 修正 START
                    'If Dao.ToolAsset.IsUseImg(sAssetNo, sPicFilNm) = False Then
                    If Not bPicUsed Then
                        '2020/10/22 戸祭 修正 END
                        System.IO.File.Delete(sFullPath + "\" + sPicFilNm)
                    End If
                End If

                '2023/03/22 Kawamoto ADD START
                '添付ファイルフォルダの削除
                Dim sAttachmentPath As String = ConfigurationManager.AppSettings("AttachmentFolderPath")
                Dim sFolderPath As String = Path.Combine(sAttachmentPath, CommonRfid.FncStringToHex(sAssetNo))

                '使用可能なパス形式に変換
                sFolderPath = ConvertEnablePath(sFolderPath)

                'フォルダがあれば削除
                If Directory.Exists(sFolderPath) Then
                    Directory.Delete(sFolderPath, True)
                End If
                '2023/03/22 Kawamoto ADD END

                '操作ログ出力
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                 Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                 Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " &
                                                 CommonRfid.ToStringNvl(_ViewModel.ASSET_NO).Trim & "  " &
                                                 Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
                                                 CommonRfid.ToStringNvl(_ViewModel.ASSET_NAME).Trim
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE), _ViewModel.LOC_L_CD, _ViewModel.LOC_M_CD, _ViewModel.LOC_S_CD)
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeDelete()

            '親の初期処理
            MyBase.InitializeDelete()

            'モデルを削除用モデルにコピー
            CopyViewModeltoModel(_ViewModel)

            'モデルを処理履歴登録用モデルにコピー
            CopyViewModeltoRireki(_ViewModel, ProcessDiv.MD)
        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        ''' <param name="model">基底モデル</param>
        Protected Overrides Sub DisposeDelete(ByRef model As BaseModel)

            MyBase.DisposeDelete(model)

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_ASSET_LIST) = _ViewModel

        End Sub

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
            Dim assetList As AssetListModel = _db.AssetList.Find(_Model.ASSET_NO)

            '削除を実行する
            _db.AssetList.Remove(assetList)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '履歴の更新日時・更新社員コード・更新社員名・更新プロセスを登録
            _HistoryModel.UPD_DATETIME = CommonRfid.getNowDate
            _HistoryModel.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _HistoryModel.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelAssetList.TITLE)

            '資産履歴テーブルの更新
            _db.AssetHistory.Add(_HistoryModel)

        End Sub
#End Region

#Region "除却/除却解除処理"
        ''' <summary>
        ''' アクションメソッド(Retire)
        ''' 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        ''' 詳細については、http://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function Retire(ByVal ViewModel As AssetListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '除却ボタン押下フラグオン
            _blRetireFlg = True

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '2023/02/08 Kawamoto ADD START
            '除却解除時、資産登録上限数チェックを行う
            If _ViewModel.RETIRE_TS IsNot Nothing Then

                '設定マスタ(ロケごとの資産登録上限数)を取得
                Dim Settinginfo As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Asset_Registration_Limit.KeyDiv,
                                                                            CommonSettingKey.Asset_Registration_Limit.Key1,
                                                                            CommonSettingKey.Asset_Registration_Limit.Key2,
                                                                            CommonSettingKey.Asset_Registration_Limit.Key3, True)

                'ロケごとの資産登録上限数 > 0の場合
                If Settinginfo.DATA1 IsNot Nothing AndAlso Integer.Parse(Settinginfo.DATA1) > 0 Then

                    Dim LocLInfo As New Dictionary(Of String, String)
                    If _ViewModel.MNG_LOC_L_CD.Trim <> String.Empty Then
                        '資産マスタ(管理ロケ大区分がロケ大区分、または管理ロケ大区分と一致するデータの総数)を取得
                        LocLInfo.Add("TargetLoc", "Target_MNG_LOC_L_CD")
                        LocLInfo.Add("LocLCode", _ViewModel.MNG_LOC_L_CD)
                        LocLInfo.Add("RegistNum", Dao.ToolAsset.GetAssetLocLCodeTotal(_ViewModel.ASSET_NO, _ViewModel.MNG_LOC_L_CD) + 1.ToString)
                        LocLInfo.Add("RegistLimit", Settinginfo.DATA1)
                        _RegistInfo.Add(LocLInfo)
                    End If

                    If ViewModel.LOC_L_CD.Trim <> String.Empty AndAlso _ViewModel.MNG_LOC_L_CD.Trim <> ViewModel.LOC_L_CD.Trim Then
                        '資産マスタ(ロケ大区分がロケ大区分、または管理ロケ大区分と一致するデータの総数)を取得
                        LocLInfo = New Dictionary(Of String, String)
                        LocLInfo.Add("TargetLoc", "Target_LOC_L_CD")
                        LocLInfo.Add("LocLCode", _ViewModel.LOC_L_CD)
                        LocLInfo.Add("RegistNum", Dao.ToolAsset.GetAssetLocLCodeTotal(_ViewModel.ASSET_NO, _ViewModel.LOC_L_CD) + 1.ToString)
                        LocLInfo.Add("RegistLimit", Settinginfo.DATA1)
                        _RegistInfo.Add(LocLInfo)
                    End If

                    For i As Integer = 0 To _RegistInfo.Count - 1
                        If _RegistInfo.Count = 0 Then Exit For
                        If Integer.Parse(_RegistInfo(i)("RegistNum")) > Integer.Parse(Settinginfo.DATA1) Then
                            _ViewModel.ErrCode = CommonLabel.MSG_ERR_EXCEED_MAX_NUM
                            Exit For
                        Else
                            _RegistInfo.RemoveAt(i)
                            i -= 1
                        End If
                    Next
                End If
            End If
            '2023/02/08 Kawamoto ADD END

            'ロケーションコード・カテゴリコードが空の場合、空白を設定する
            If String.IsNullOrEmpty(_ViewModel.LOC_L_CD) Then
                _ViewModel.LOC_L_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.LOC_M_CD) Then
                _ViewModel.LOC_M_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.LOC_S_CD) Then
                _ViewModel.LOC_S_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.CAT_M_CD) Then
                _ViewModel.CAT_M_CD = " "
            End If
            If String.IsNullOrEmpty(_ViewModel.CAT_S_CD) Then
                _ViewModel.CAT_S_CD = " "
            End If
            '--2021/09/17 nv miyaji add start
            If String.IsNullOrEmpty(_ViewModel.MNG_LOC_L_CD) Then
                _ViewModel.MNG_LOC_L_CD = " "
            End If
            '--2021/09/17 nv miyaji add end

            '更新処理
            MyBase.BaseEdit(_ViewModel)

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                If _ViewModel.RETIRE_TS Is Nothing Then
                    _ViewModel.States = StatusType.SuccessRetire

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_RETIRE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.ASSET_NO).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.ASSET_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE), _ViewModel.LOC_L_CD, _ViewModel.LOC_M_CD, _ViewModel.LOC_S_CD)
                Else
                    _ViewModel.States = StatusType.SuccessRetireOff

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_RETIRE_OFF) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.ASSET_NO).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.ASSET_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE), _ViewModel.LOC_L_CD, _ViewModel.LOC_M_CD, _ViewModel.LOC_S_CD)
                End If
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

#End Region

        '--2021/10/11 sv suzuki add start
#Region "品番に紐づく情報の取得処理"
        ''' <summary>
        ''' アクションメソッド(GetProductNoInfo)
        ''' </summary>
        ''' <returns>View表示内容</returns>
        Public Function GetProductNoInfo(ByVal ProductNo As String, ByVal CatLCd As String) As JsonResult
            Dim db As New SystemDBContext
            Dim viewModel As New AssetListViewModel

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            If String.IsNullOrEmpty(ProductNo) Then
                '品番が未入力の場合、エラーを返す
                viewModel.ErrCode = CommonLabel.MSG_ERR_NOT_ENTERD
                Return Json(viewModel)
            Else
                '商品マスタデータ取得
                Dim GoodsData As DataTable
                GoodsData = Dao.ToolGoods.GetGoodsData(ProductNo)

                'カテゴリマスタデータ取得
                Dim CategoryData As DataTable
                CategoryData = Dao.ToolCategory.GetCategoryDataAll()

                '設定マスタから文字コード情報を取得
                Dim dtReflectItem As DataTable = Dao.ToolSetting.GetGoodsReflectItemData()

                If GoodsData.Rows.Count = 0 Then
                    '入力された品番の情報がない場合、何も更新しない
                    viewModel.ErrCode = CommonLabel.MSG_ERR_GOODS_NOT_EXISTS
                    Return Json(viewModel)

                Else
                    '商品マスタ情報から導出されるカテゴリー大区分情報
                    Dim CatLrow As DataRow() = CategoryData.Select("CAT_L_CD = '" + GoodsData.Rows(0).Item("CAT_L_CD").ToString.Trim + "' AND CAT_DIV = '" + CAT_DIV.CategoryL + "'")

                    '画面情報から導出されるカテゴリー大区分情報
                    Dim CatLrowView As DataRow() = CategoryData.Select("CAT_L_CD = '" + CatLCd + "' AND CAT_DIV = '" + CAT_DIV.CategoryL + "'")

                    If CatLrow.Count <> 0 AndAlso CatLrowView.Count <> 0 AndAlso GoodsData.Rows(0).Item("CAT_L_CD").ToString.Trim <> String.Empty AndAlso
                       GoodsData.Rows(0).Item("CAT_L_CD").ToString.Trim <> CatLCd Then
                        'カテゴリ大分類コードが変更になる場合はエラー
                        viewModel.ErrCode = CommonLabel.MSG_ERR_ASSET_ID_REG

                        '設定マスタで更新するとされている項目(カテゴリ以外)を更新する
                        Call SetGoodsData(dtReflectItem, GoodsData, viewModel, True)
                    Else
                        '設定マスタで更新するとされている項目を更新する
                        Call SetGoodsData(dtReflectItem, GoodsData, viewModel, False)
                    End If
                End If
            End If

            '結果表示
            Return Json(viewModel)

        End Function

        ''' <summary>
        ''' 商品マスタ情報の反映
        ''' </summary>
        ''' <param name="dtReflectItem"></param>
        ''' <param name="GoodsData"></param>
        ''' <param name="viewModel"></param>
        ''' <param name="blnDiffentIdReg"></param>
        Private Sub SetGoodsData(ByVal dtReflectItem As DataTable, ByVal GoodsData As DataTable, ByRef viewModel As AssetListViewModel, ByVal blnDiffentIdReg As Boolean)

            '2023/03/22 Kawamoto ADD START
            '設定マスタ（薬品管理機能）の取得
            Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
                                                                      CommonSettingKey.Drugs_Function.Key1,
                                                                      CommonSettingKey.Drugs_Function.Key2,
                                                                      CommonSettingKey.Drugs_Function.Key3, True)
            '2023/03/22 Kawamoto ADD END

            If dtReflectItem.Select("DATA1 = 'ASSET_DIV'").Count <> 0 Then
                viewModel.ASSET_DIV = GoodsData.Rows(0).Item("ASSET_DIV").ToString.Trim
            Else
                viewModel.ASSET_DIV = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'ASSET_NAME'").Count <> 0 Then
                viewModel.ASSET_NAME = GoodsData.Rows(0).Item("ASSET_NAME").ToString.Trim
            Else
                viewModel.ASSET_NAME = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'MNG_LOC_L_CD'").Count <> 0 Then
                viewModel.MNG_LOC_L_CD = GoodsData.Rows(0).Item("MNG_LOC_L_CD").ToString.Trim
            Else
                viewModel.MNG_LOC_L_CD = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'LOC_L_CD'").Count <> 0 Then
                viewModel.LOC_L_CD = GoodsData.Rows(0).Item("LOC_L_CD").ToString.Trim
            Else
                viewModel.LOC_L_CD = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'LOC_M_CD'").Count <> 0 Then
                viewModel.LOC_M_CD = GoodsData.Rows(0).Item("LOC_M_CD").ToString.Trim
            Else
                viewModel.LOC_M_CD = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'LOC_S_CD'").Count <> 0 Then
                viewModel.LOC_S_CD = GoodsData.Rows(0).Item("LOC_S_CD").ToString.Trim
            Else
                viewModel.LOC_S_CD = CommonConst.NOT_UPD_STR
            End If
            If Not blnDiffentIdReg Then
                '資産ID登録方法が同じ場合
                If dtReflectItem.Select("DATA1 = 'CAT_L_CD'").Count <> 0 Then
                    viewModel.CAT_L_CD = GoodsData.Rows(0).Item("CAT_L_CD").ToString.Trim
                Else
                    viewModel.CAT_L_CD = CommonConst.NOT_UPD_STR
                End If
                If dtReflectItem.Select("DATA1 = 'CAT_M_CD'").Count <> 0 Then
                    viewModel.CAT_M_CD = GoodsData.Rows(0).Item("CAT_M_CD").ToString.Trim
                Else
                    viewModel.CAT_M_CD = CommonConst.NOT_UPD_STR
                End If
                If dtReflectItem.Select("DATA1 = 'CAT_S_CD'").Count <> 0 Then
                    viewModel.CAT_S_CD = GoodsData.Rows(0).Item("CAT_S_CD").ToString.Trim
                Else
                    viewModel.CAT_S_CD = CommonConst.NOT_UPD_STR
                End If
            Else
                '資産ID登録方法が異なる場合
                If dtReflectItem.Select("DATA1 = 'CAT_L_CD'").Count <> 0 Then
                    viewModel.CAT_L_CD = CommonConst.NOT_UPD_STR
                End If
                If dtReflectItem.Select("DATA1 = 'CAT_M_CD'").Count <> 0 Then
                    viewModel.CAT_M_CD = CommonConst.NOT_UPD_STR
                End If
                If dtReflectItem.Select("DATA1 = 'CAT_S_CD'").Count <> 0 Then
                    viewModel.CAT_S_CD = CommonConst.NOT_UPD_STR
                End If

            End If

            If dtReflectItem.Select("DATA1 = 'MAKER'").Count <> 0 Then
                viewModel.MAKER = GoodsData.Rows(0).Item("MAKER").ToString.Trim
            Else
                viewModel.MAKER = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'MODEL_NO'").Count <> 0 Then
                viewModel.MODEL_NO = GoodsData.Rows(0).Item("MODEL_NO").ToString.Trim
            Else
                viewModel.MODEL_NO = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'LEASE_NO'").Count <> 0 Then
                viewModel.LEASE_NO = GoodsData.Rows(0).Item("LEASE_NO").ToString.Trim
            Else
                viewModel.LEASE_NO = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'LEASE_LIMIT'").Count <> 0 Then
                If GoodsData.Rows(0).Item("LEASE_LIMIT").ToString.Trim.Length <> itemlength.LeaseLimit Then
                    viewModel.LeaseLimitString = String.Empty
                Else
                    Dim sLeaseLimit As String = String.Empty
                    sLeaseLimit = CommonRfid.ToStringIntYMD(GoodsData.Rows(0).Item("LEASE_LIMIT").ToString.Trim)
                    viewModel.LeaseLimitString = CommonRfid.ToHaifunYMD(sLeaseLimit)
                End If
            Else
                viewModel.LeaseLimitString = CommonConst.NOT_UPD_INT
            End If
            If dtReflectItem.Select("DATA1 = 'GENERIC_LIMIT'").Count <> 0 Then
                If GoodsData.Rows(0).Item("LEASE_LIMIT").ToString.Trim.Length <> itemlength.GenLimit Then
                    viewModel.GenLimitString = String.Empty
                Else
                    Dim sGenLimit As String = String.Empty
                    sGenLimit = CommonRfid.ToStringIntYMD(GoodsData.Rows(0).Item("GENERIC_LIMIT").ToString.Trim)
                    viewModel.GenLimitString = CommonRfid.ToHaifunYMD(sGenLimit)
                End If
            Else
                viewModel.GenLimitString = CommonConst.NOT_UPD_INT
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM1'").Count <> 0 Then
                viewModel.FREE_ITEM1 = GoodsData.Rows(0).Item("FREE_ITEM1").ToString.Trim.Replace("/", "-")
            Else
                viewModel.FREE_ITEM1 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM2'").Count <> 0 Then
                viewModel.FREE_ITEM2 = GoodsData.Rows(0).Item("FREE_ITEM2").ToString.Trim
            Else
                viewModel.FREE_ITEM2 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM3'").Count <> 0 Then
                viewModel.FREE_ITEM3 = GoodsData.Rows(0).Item("FREE_ITEM3").ToString.Trim
            Else
                viewModel.FREE_ITEM3 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM4'").Count <> 0 Then
                viewModel.FREE_ITEM4 = GoodsData.Rows(0).Item("FREE_ITEM4").ToString.Trim
            Else
                viewModel.FREE_ITEM4 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM5'").Count <> 0 Then
                viewModel.FREE_ITEM5 = GoodsData.Rows(0).Item("FREE_ITEM5").ToString.Trim
            Else
                viewModel.FREE_ITEM5 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM6'").Count <> 0 Then
                viewModel.FREE_ITEM6 = GoodsData.Rows(0).Item("FREE_ITEM6").ToString.Trim
            Else
                viewModel.FREE_ITEM6 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM7'").Count <> 0 Then
                viewModel.FREE_ITEM7 = GoodsData.Rows(0).Item("FREE_ITEM7").ToString.Trim
            Else
                viewModel.FREE_ITEM7 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM8'").Count <> 0 Then
                viewModel.FREE_ITEM8 = GoodsData.Rows(0).Item("FREE_ITEM8").ToString.Trim
            Else
                viewModel.FREE_ITEM8 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM9'").Count <> 0 Then
                viewModel.FREE_ITEM9 = GoodsData.Rows(0).Item("FREE_ITEM9").ToString.Trim
            Else
                viewModel.FREE_ITEM9 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM10'").Count <> 0 Then
                viewModel.FREE_ITEM10 = GoodsData.Rows(0).Item("FREE_ITEM10").ToString.Trim
            Else
                viewModel.FREE_ITEM10 = CommonConst.NOT_UPD_STR
            End If
            '2023/02/08 Kawamoto ADD START
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM11'").Count <> 0 Then
                viewModel.FREE_ITEM11 = GoodsData.Rows(0).Item("FREE_ITEM11").ToString.Trim
            Else
                viewModel.FREE_ITEM11 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM12'").Count <> 0 Then
                viewModel.FREE_ITEM12 = GoodsData.Rows(0).Item("FREE_ITEM12").ToString.Trim
            Else
                viewModel.FREE_ITEM12 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM13'").Count <> 0 Then
                '2023/03/22 Kawamoto ADD START
                '薬品機能がONの場合
                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                    Dim dt As DateTime
                    If DateTime.TryParse(GoodsData.Rows(0).Item("FREE_ITEM13").ToString.Trim, dt) Then
                        viewModel.FREE_ITEM13 = CommonRfid.ToHaifunYMD(dt)
                    Else
                        viewModel.FREE_ITEM13 = GoodsData.Rows(0).Item("FREE_ITEM13").ToString.Trim
                    End If
                Else
                    '2023/03/22 Kawamoto ADD END
                    viewModel.FREE_ITEM13 = GoodsData.Rows(0).Item("FREE_ITEM13").ToString.Trim
                End If
            Else
                viewModel.FREE_ITEM13 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM14'").Count <> 0 Then
                viewModel.FREE_ITEM14 = GoodsData.Rows(0).Item("FREE_ITEM14").ToString.Trim
            Else
                viewModel.FREE_ITEM14 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM15'").Count <> 0 Then
                viewModel.FREE_ITEM15 = GoodsData.Rows(0).Item("FREE_ITEM15").ToString.Trim
            Else
                viewModel.FREE_ITEM15 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM16'").Count <> 0 Then
                viewModel.FREE_ITEM16 = GoodsData.Rows(0).Item("FREE_ITEM16").ToString.Trim
            Else
                viewModel.FREE_ITEM16 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM17'").Count <> 0 Then
                viewModel.FREE_ITEM17 = GoodsData.Rows(0).Item("FREE_ITEM17").ToString.Trim
            Else
                viewModel.FREE_ITEM17 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM18'").Count <> 0 Then
                viewModel.FREE_ITEM18 = GoodsData.Rows(0).Item("FREE_ITEM18").ToString.Trim
            Else
                viewModel.FREE_ITEM18 = CommonConst.NOT_UPD_STR
            End If
            If dtReflectItem.Select("DATA1 = 'FREE_ITEM19'").Count <> 0 Then
                viewModel.FREE_ITEM19 = GoodsData.Rows(0).Item("FREE_ITEM19").ToString.Trim
            Else
                viewModel.FREE_ITEM19 = CommonConst.NOT_UPD_STR
            End If
            '2023/02/08 Kawamoto ADD END
            If dtReflectItem.Select("DATA1 = 'ASSET_PIC_FILENAME'").Count <> 0 Then
                viewModel.ASSET_PIC_FILENAME = GoodsData.Rows(0).Item("ASSET_PIC_FILENAME").ToString.Trim
            Else
                viewModel.ASSET_PIC_FILENAME = CommonConst.NOT_UPD_STR
            End If
        End Sub
#End Region
        '--2021/10/11 sv suzuki add end

#Region "ロケーション存在チェック"
        ''' <summary>
        ''' ロケーション存在チェック
        ''' </summary>
        ''' <param name="LocLCd">ロケーション大</param>
        ''' <param name="LocMCd">ロケーション中</param>
        ''' <param name="LocSCd">ロケーション小</param>
        ''' <returns></returns>
        Public Function CheckLocation(ByVal LocLCd As String, ByVal LocMCd As String, ByVal LocSCd As String) As JsonResult
            Dim db As New SystemDBContext
            Dim viewModel As New AssetListViewModel
            Dim locData As LocationModel

            'ロケーション大チェック
            locData = _db.Location.Find(LOC_DIV.LocationL, LocLCd, "", "")

            If (LocMCd IsNot "" AndAlso locData IsNot Nothing) Then
                'ロケーション中チェック
                locData = _db.Location.Find(LOC_DIV.LocationM, LocLCd, LocMCd, "")
            End If

            If (LocSCd IsNot "" AndAlso locData IsNot Nothing) Then
                'ロケーション小チェック
                locData = _db.Location.Find(LOC_DIV.LocationS, LocLCd, LocMCd, LocSCd)
            End If

            If locData IsNot Nothing Then
                Return Json(True)
            Else
                Return Json(False)
            End If

        End Function
#End Region

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
        '　関数内の ver4.3.1 修正（処理ログへの備考1～10追加、6ブロック）は物理削除した（2026/10/02 指示）
#Region "【未使用・コメントアウト】ImportCsvFile（CSVデータ取込処理）"
'#Region "CSVデータ取込処理"
        '''' <summary>
        '''' 変更内容を非同期通信で取得します（AJax)
        '''' </summary>
        '''' <param name="CsvFile">登録するCSVファイル</param>
        '''' <returns>View表示内容</returns>
        'Public Function ImportCsvFile(ByVal CsvFile As HttpPostedFileBase) As ActionResult
        '    Me._lstErr = New List(Of DataInportResult)
        '    Dim errRow As DataInportResult = New DataInportResult
        '    Me._lstErr.Add(errRow)
        '    Dim sCsvPath As String = String.Empty   'CSVアップロードフルパス

        '    Dim sProcessContents As String          '操作ログ出力

        '    'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
        '    _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
        '    If IsNothing(_LoginInfo) Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.LoginSession
        '        '結果表示
        '        Return Json(Me._lstErr)
        '    End If

        '    ''●●●
        '    'logger.Info("STEP1 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))


        '    'メッセージ一覧取得
        '    Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

        '    'CSVマッピング明細マスタ取得
        '    Dim tblCsvMapMei As DataTable = GetCsvMapDetail()
        '    '取得したCSVマッピング明細マスタの項目数、項目のチェック
        '    If Not IsCsvMapMeiKoumokuOK(tblCsvMapMei) Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.CsvMapMei
        '        '操作ログ出力
        '        sProcessContents = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
        '                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
        '                                             Me._Message.getMessage(CommonLabel.MSG_ERR_CSVMAPMEI) & " " &
        '                                             Path.GetFileName(CsvFile.FileName)
        '        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

        '        Return Json(Me._lstErr)
        '    End If

        '    '読込むCSVファイルがあるかどうか
        '    If Not IsCsvFileError(CsvFile) Then
        '        Dim sErrGaiyou As String = String.Empty
        '        Select Case Me._lstErr(0).ResultCd
        '            Case DataInport.DataInportResultCd.NoSelect
        '                sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECT)
        '            Case DataInport.DataInportResultCd.Empty
        '                sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_EMPTY)
        '            Case DataInport.DataInportResultCd.NotCsv
        '                sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTCSV)
        '        End Select

        '        '操作ログ出力
        '        sProcessContents = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
        '                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
        '                                             sErrGaiyou
        '        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

        '        Return Json(Me._lstErr)
        '    End If

        '    '◆CSVファイル出力先フォルダ存在チェック
        '    Dim sFullPath As String = Server.MapPath(CommonConst.CSVIMPORT_FOLDERNM)
        '    If Not Directory.Exists(sFullPath) Then
        '        Directory.CreateDirectory(sFullPath)
        '    End If

        '    'CSVファイルをアップロード
        '    sCsvPath = Path.Combine(sFullPath, CsvFile.FileName)
        '    CsvFile.SaveAs(sCsvPath)

        '    ''●●●
        '    'logger.Info("STEP2 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

        '    Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

        '    'CSVマッピングマスタの文字コード取得
        '    Dim sCharCd As String = GetCsvMap()

        '    'CSVマッピングマスタの文字コードがutf-8・shift_jis以外の場合、エラー
        '    If Not sCharCd = CommonSettingKey.CsvMapUtf.Key1 AndAlso Not sCharCd = CommonSettingKey.CsvMapShiftJis.Key1 Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.CsvMapCharCd
        '        '結果表示
        '        Return Json(Me._lstErr)
        '    End If

        '    '設定マスタから文字コード情報を取得
        '    Dim data As SettingInfo = New SettingInfo
        '    Dim skeyDiv As String = String.Empty    'エラーメッセージ用
        '    If sCharCd = CommonSettingKey.CsvMapUtf.Key1 Then
        '        skeyDiv = CommonSettingKey.CsvMapUtf.KeyDiv
        '        data = Me._Message.getSettingInfo(CommonSettingKey.CsvMapUtf.KeyDiv, CommonSettingKey.CsvMapUtf.Key1, CommonSettingKey.CsvMapUtf.Key2, CommonSettingKey.CsvMapUtf.Key3)
        '    ElseIf sCharCd = CommonSettingKey.CsvMapUtf.Key2 Then
        '        skeyDiv = CommonSettingKey.CsvMapShiftJis.KeyDiv
        '        data = Me._Message.getSettingInfo(CommonSettingKey.CsvMapShiftJis.KeyDiv, CommonSettingKey.CsvMapShiftJis.Key1, CommonSettingKey.CsvMapShiftJis.Key2, CommonSettingKey.CsvMapShiftJis.Key3)
        '    End If
        '    '設定マスタからデータを取得できなかった場合、エラー
        '    If data Is Nothing Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.SettingInfo
        '        Me._lstErr(0).ErrGaiyou = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), skeyDiv & " " & sCharCd)
        '        '結果表示
        '        Return Json(Me._lstErr)
        '    End If

        '    ''全データ出力ヘッダ取得
        '    'Dim sHedder As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_ALL)
        '    'Dim stArrayData As String() = sHedder.Split(","c)

        '    '--登録処理開始
        '    Dim db As New SystemDBContext
        '    Dim model As List(Of AssetListWorkModel) = New List(Of AssetListWorkModel)    '登録用モデル
        '    Dim sbSQL As StringBuilder = New StringBuilder
        '    Dim params As List(Of SqlParameter)

        '    'トランザクションスタート
        '    Using tran = db.Database.BeginTransaction()
        '        Try
        '            'CSVデータ取得 & データ登録
        '            Dim blErr As Boolean = False    'CSVファイルにエラーがあるかどうか
        '            Dim isKomokusuuErr As Boolean = False   '項目数エラーかどうか

        '            ''●●●
        '            'logger.Info("STEP3 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

        '            '-----データチェック & 一時表に書き込み-----
        '            Dim sTmpTblNm = "#TASSET" & Now.ToString("HHmmss") & (New Random).ToString("000")       '作成する一時表の名称

        '            '----- 2020/03/12 ADD INOUE 一時表作成を必ず作成するようにするため、ここで行なう
        '            ''作業用資産データテーブルを全削除
        '            'db.Database.ExecuteSqlCommand("DELETE FROM W_ASSET")

        '            '一時表作成
        '            Dim sCreate As String = String.Format("Select * INTO {0} FROM M_ASSET WHERE 1 = 0", sTmpTblNm)
        '            db.Database.ExecuteSqlCommand(sCreate)

        '            Dim sAddColumn As String
        '            ''一時表に、列「SEQNO」を追加する  (CSVファイルの行NO)
        '            'sAddColumn = String.Format("ALTER TABLE {0} ADD UPD_FLG bigint DEFAULT 0", sTmpTblNm)
        '            'db.Database.ExecuteSqlCommand(sAddColumn)
        '            '一時表に、列「UPD_FLG」を追加する　(0:変更なし 1:変更あり  ※新規登録行はこの区分を使用しない)
        '            sAddColumn = String.Format("ALTER TABLE {0} ADD UPD_FLG Bit DEFAULT 0", sTmpTblNm)
        '            db.Database.ExecuteSqlCommand(sAddColumn)
        '            '-----2020/03/12 ADD INOUE 一時表作成を必ず作成するようにするため、ここで行なう　END

        '            '2023/02/08 Kawamoto ADD START
        '            '一時表に、列「ACTIVITY」を追加する ※資産CSVファイル取込全件対象が「差分取込」の場合に使用する
        '            sAddColumn = String.Format("ALTER TABLE {0} ADD ACTIVITY CHAR DEFAULT NULL", sTmpTblNm)
        '            db.Database.ExecuteSqlCommand(sAddColumn)
        '            '2023/02/08 Kawamoto ADD END

        '            blErr = ReadCsvFile(sCsvPath, Encoding.GetEncoding(CInt(data.DATA2)), isKomokusuuErr, tblCsvMapMei, sTmpTblNm, db)
        '            'Me._dtCsvImport = clsSV_CsvFile.ReadCsvFileToDataTable(sCsvPath, Encoding.GetEncoding(CInt(data.DATA2)), CChar(","), True, "Asset", rows.Count, isKomokusuuErr)
        '            'Me._dtCsvImport = clsSV_CsvFile.ReadCsvFileToDataTable(sCsvPath, Encoding.UTF8, CChar(","), True, "Asset", stArrayData, isKomokusuuErr)

        '            ''●●●
        '            'logger.Info("STEP4 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

        '            '項目数エラーの場合
        '            If isKomokusuuErr Then
        '                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
        '                '操作ログ出力
        '                sProcessContents = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
        '                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
        '                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " &
        '                                                     Path.GetFileName(CsvFile.FileName)
        '                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

        '                'ロールバック
        '                tran.Rollback()

        '                Return Json(Me._lstErr)
        '            End If

        '            'エラーがあった場合
        '            If blErr Then
        '                '操作ログ出力
        '                sProcessContents = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
        '                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
        '                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " &
        '                                                     Path.GetFileName(CsvFile.FileName) & " " &
        '                                                     Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
        '                                                     Me._lstErr.Count.ToString
        '                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

        '                'ロールバック
        '                tran.Rollback()

        '                Return Json(Me._lstErr)
        '            End If

        '            ''●●●
        '            'logger.Info("STEP5 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

        '            '2023/02/08 Kawamoto ADD START
        '            '権限情報の取得
        '            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)
        '            Dim clsDB As clsSV_DB = New clsSV_DB
        '            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
        '                '接続失敗
        '                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        '            End If


        '            '設定マスタ(資産CSVファイル取込全件対象)を取得
        '            Dim CsvRegistALL As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Asset_CSV_Regist_ALL.KeyDiv,
        '                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key1,
        '                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key2,
        '                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key3, True)
        '            '設定マスタ（薬品管理機能）の取得
        '            Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
        '                                                                      CommonSettingKey.Drugs_Function.Key1,
        '                                                                      CommonSettingKey.Drugs_Function.Key2,
        '                                                                      CommonSettingKey.Drugs_Function.Key3, True)
        '            '2023/02/08 Kawamoto ADD END

        '            '差分について、除却か削除かを取得する   ADD 2019/12/20 Inoue
        '            Dim sAssetDelKbn As String
        '            Dim setting As SettingInfo = New SettingInfo
        '            setting = Me._Message.getSettingInfo(CommonSettingKey.DeleteAsset.KeyDiv, CommonSettingKey.DeleteAsset.Key1, CommonSettingKey.DeleteAsset.Key2, CommonSettingKey.DeleteAsset.Key3)
        '            '設定マスタからデータを取得できなかった場合、除却で処理する
        '            If setting IsNot Nothing AndAlso setting.DATA1 = Misc.AssetDelete.Delete Then
        '                sAssetDelKbn = Misc.AssetDelete.Delete
        '            Else
        '                sAssetDelKbn = Misc.AssetDelete.Remove
        '            End If

        '            '2023/03/22 Kawamoto ADD START
        '            '添付ファイルフォルダの削除情報を取得する
        '            sbSQL.Clear()
        '            Dim DeleteAssetNoList As New List(Of MstAssetModel)

        '            '資産CSVファイル取込全件対象が「全件対象」の場合
        '            If CsvRegistALL.DATA1 = AssetCSVRegistAll.All Then
        '                If sAssetDelKbn = Misc.AssetDelete.Delete Then
        '                    sbSQL.AppendLine("SELECT M_ASSET.ASSET_NO FROM M_ASSET")
        '                    sbSQL.AppendLine("WHERE")
        '                    sbSQL.AppendLine("  Not EXISTS")
        '                    sbSQL.AppendLine("  (")
        '                    sbSQL.AppendLine("      SELECT ASSET_NO ")
        '                    sbSQL.AppendLine("      FROM " & sTmpTblNm & " WK")
        '                    sbSQL.AppendLine("      WHERE WK.ASSET_NO = M_ASSET.ASSET_NO")
        '                    sbSQL.AppendLine("  )")
        '                End If
        '                '資産CSVファイル取込全件対象が「差分取込」の場合
        '            Else
        '                sbSQL.AppendLine("SELECT M_ASSET.ASSET_NO FROM M_ASSET")
        '                sbSQL.AppendLine("WHERE")
        '                sbSQL.AppendLine("  EXISTS")
        '                sbSQL.AppendLine("  (")
        '                sbSQL.AppendLine("      SELECT ASSET_NO ")
        '                sbSQL.AppendLine("      FROM " & sTmpTblNm & " WK")
        '                sbSQL.AppendLine("      WHERE WK.ASSET_NO = M_ASSET.ASSET_NO")
        '                sbSQL.AppendLine("      AND WK.ACTIVITY = '" & CsvImportProcDiv.Delete & "'")  '処理区分が「D:削除」
        '                sbSQL.AppendLine("  )")
        '            End If
        '            If Not String.IsNullOrEmpty(sbSQL.ToString) Then
        '                DeleteAssetNoList = CType(db.Database.SqlQuery(Of MstAssetModel)(sbSQL.ToString).ToList, List(Of MstAssetModel))
        '            End If
        '            '2023/03/22 Kawamoto ADD END


        '            '--先に資産処理履歴ファイルを更新する
        '            '差分除却分
        '            sbSQL.Clear()
        '            params = New List(Of SqlParameter)

        '            sbSQL.AppendLine("INSERT INTO T_PROCESS_LOG")
        '            sbSQL.AppendLine("(")
        '            sbSQL.AppendLine("   PROCESS_DT")
        '            sbSQL.AppendLine("  ,PROCESS_TM")
        '            sbSQL.AppendLine("  ,ASSET_NO")
        '            sbSQL.AppendLine("  ,SUB_ASSET_NO")
        '            sbSQL.AppendLine("  ,ASSET_ID")
        '            sbSQL.AppendLine("  ,PROCESS_TOOL_DIV")
        '            sbSQL.AppendLine("  ,PROCESS_DIV")
        '            sbSQL.AppendLine("  ,LEASE_PLACE_CD")
        '            sbSQL.AppendLine("  ,LEASE_PLACE_NAME")
        '            sbSQL.AppendLine("  ,REMARK")   '2019/11/19 nv miyaji add
        '            sbSQL.AppendLine("  ,LEASE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,LEASE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,LEASE_DT") '2020/03/06 nv miyaji add
        '            sbSQL.AppendLine("  ,LEASE_DUE_DT")
        '            sbSQL.AppendLine("  ,LOC_L_CD")
        '            sbSQL.AppendLine("  ,LOC_M_CD")
        '            sbSQL.AppendLine("  ,LOC_S_CD")
        '            sbSQL.AppendLine("  ,LOC_L_NAME")
        '            sbSQL.AppendLine("  ,LOC_M_NAME")
        '            sbSQL.AppendLine("  ,LOC_S_NAME")
        '            sbSQL.AppendLine("  ,CAT_L_CD")
        '            sbSQL.AppendLine("  ,CAT_NAME")
        '            sbSQL.AppendLine("  ,ASSET_NAME")
        '            sbSQL.AppendLine("  ,MAKER")
        '            sbSQL.AppendLine("  ,PRODUCT_NO")
        '            sbSQL.AppendLine("  ,MODEL_NO")
        '            '--2021/10/05 nv miyaji add start
        '            sbSQL.AppendLine("  ,MNG_LOC_L_CD ")
        '            sbSQL.AppendLine("  ,LEASE_STATUS ")
        '            '--2021/10/05 nv miyaji add end
        '            sbSQL.AppendLine("  ,TASK_RESULT")
        '            sbSQL.AppendLine("  ,UPD_DATETIME")
        '            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,UPD_TASK")
        '            sbSQL.AppendLine(")")
        '            sbSQL.AppendLine("SELECT")
        '            sbSQL.AppendLine("   CONVERT(int, FORMAT(GetDate(), 'yyyyMMdd'))")
        '            sbSQL.AppendLine("  ,FORMAT(GetDate(), 'HHmmssfff')")
        '            sbSQL.AppendLine("  ,ASSET.ASSET_NO")
        '            sbSQL.AppendLine("  ,ASSET.SUB_ASSET_NO")
        '            sbSQL.AppendLine("  ,ASSET.ASSET_ID")
        '            sbSQL.AppendLine("  ,'" & Misc.ProcessToolDiv.Web & "'")

        '            '2023/02/08 Kawamoto UPD START
        '            'If sAssetDelKbn = Misc.AssetDelete.Delete Then                   'UPDATE 2019/12/20 Inoue
        '            '    sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.CD & "'")
        '            'Else
        '            '    sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.CR & "'")
        '            'End If

        '            '資産CSVファイル取込全件対象が「全件対象」の場合
        '            If CsvRegistALL.DATA1 = AssetCSVRegistAll.All Then
        '                If sAssetDelKbn = Misc.AssetDelete.Delete Then                   'UPDATE 2019/12/20 Inoue
        '                    sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.CD & "'")
        '                Else
        '                    sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.CR & "'")
        '                End If
        '                '資産CSVファイル取込全件対象が「差分取込」の場合
        '            Else
        '                sbSQL.AppendLine("  ,CASE WK.ACTIVITY WHEN '" & CsvImportProcDiv.Delete & "' THEN '" & Misc.ProcessDiv.CD & "'")        '処理区分が「D:削除」
        '                sbSQL.AppendLine("                    WHEN '" & CsvImportProcDiv.Retirement & "' THEN '" & Misc.ProcessDiv.CR & "'")    '処理区分が「R:除却」
        '                sbSQL.AppendLine("                    ELSE NULL END")
        '            End If
        '            '2023/02/08 Kawamoto UPD END

        '            sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_CD")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_NAME")
        '            sbSQL.AppendLine("  ,ASSET.REMARK")   '2019/11/19 nv miyaji add
        '            sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_DT") '2020/03/06 nv miyaji add
        '            sbSQL.AppendLine("  ,ASSET.LEASE_DUE_DT")
        '            sbSQL.AppendLine("  ,ASSET.LOC_L_CD")
        '            sbSQL.AppendLine("  ,ASSET.LOC_M_CD")
        '            sbSQL.AppendLine("  ,ASSET.LOC_S_CD")
        '            sbSQL.AppendLine("  ,LOCL.LOC_NAME AS LocLName ")
        '            sbSQL.AppendLine("  ,LOCM.LOC_NAME AS LocMName ")
        '            sbSQL.AppendLine("  ,LOCS.LOC_NAME AS LocSName ")
        '            sbSQL.AppendLine("  ,ASSET.CAT_L_CD")
        '            sbSQL.AppendLine("  ,CAT.CAT_NAME AS CatName ")
        '            sbSQL.AppendLine("  ,ASSET.ASSET_NAME")
        '            sbSQL.AppendLine("  ,ASSET.MAKER")
        '            sbSQL.AppendLine("  ,ASSET.PRODUCT_NO")
        '            sbSQL.AppendLine("  ,ASSET.MODEL_NO")
        '            '--2021/10/05 nv miyaji add start
        '            sbSQL.AppendLine("  ,ASSET.MNG_LOC_L_CD ")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_STATUS ")
        '            '--2021/10/05 nv miyaji add end
        '            sbSQL.AppendLine("  ,'" & Misc.TaskResult.OK & "'")
        '            sbSQL.AppendLine("  ,ASSET.UPD_DATETIME")
        '            sbSQL.AppendLine("  ,ASSET.UPD_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,ASSET.UPD_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,ASSET.UPD_TASK")
        '            sbSQL.AppendLine("FROM M_ASSET ASSET")

        '            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
        '            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
        '            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
        '            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM ")
        '            sbSQL.AppendLine("ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
        '            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCM.LOC_L_CD")
        '            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
        '            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS ")
        '            sbSQL.AppendLine("ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
        '            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCS.LOC_L_CD")
        '            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCS.LOC_M_CD")
        '            sbSQL.AppendLine("AND ASSET.LOC_S_CD = LOCS.LOC_S_CD")
        '            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT ")
        '            sbSQL.AppendLine("ON CAT.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
        '            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CAT.CAT_L_CD")

        '            '2023/02/08 Kawamoto UPD START
        '            'sbSQL.AppendLine("WHERE")
        '            'sbSQL.AppendLine("  Not EXISTS")
        '            'sbSQL.AppendLine("  (")
        '            'sbSQL.AppendLine("      Select ASSET_NO ")
        '            'sbSQL.AppendLine("      FROM " & sTmpTblNm & " WK")
        '            'sbSQL.AppendLine("      WHERE WK.ASSET_NO = ASSET.ASSET_NO")
        '            'sbSQL.AppendLine("  )")

        '            '資産CSVファイル取込全件対象が「全件対象」の場合
        '            If CsvRegistALL.DATA1 = AssetCSVRegistAll.All Then
        '                sbSQL.AppendLine("WHERE")
        '                sbSQL.AppendLine("  Not EXISTS")
        '                sbSQL.AppendLine("  (")
        '                sbSQL.AppendLine("      SELECT ASSET_NO ")
        '                sbSQL.AppendLine("      FROM " & sTmpTblNm & " WK")
        '                sbSQL.AppendLine("      WHERE WK.ASSET_NO = ASSET.ASSET_NO")
        '                sbSQL.AppendLine("  )")
        '                '資産CSVファイル取込全件対象が「差分取込」の場合
        '            Else
        '                sbSQL.AppendLine("INNER JOIN " & sTmpTblNm & " WK")
        '                sbSQL.AppendLine(" ON WK.ASSET_NO = ASSET.ASSET_NO")
        '                sbSQL.AppendLine("WHERE")
        '                sbSQL.AppendLine("   WK.ACTIVITY = '" & CsvImportProcDiv.Delete & "'")      '処理区分が「D:削除」
        '                sbSQL.AppendLine("OR WK.ACTIVITY = '" & CsvImportProcDiv.Retirement & "'")  '処理区分が「R:除却」
        '            End If
        '            '2023/02/08 Kawamoto UPD END

        '            sbSQL.AppendLine("AND ASSET.ASSET_STATUS <> '" & Misc.ASSET_STATUS.Removed & "'")   '元々、除却は更新しない

        '            db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)


        '            ''●●●
        '            'logger.Info("STEP6 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

        '            '差分除却処理  2019/09/10 Inoue ADD
        '            sbSQL.Clear()
        '            params = New List(Of SqlParameter)

        '            '資産CSVファイル取込全件対象が「全件対象」の場合
        '            If CsvRegistALL.DATA1 = AssetCSVRegistAll.All Then  '2023/02/08 Kawamoto ADD START

        '                If sAssetDelKbn = Misc.AssetDelete.Delete Then                   'UPDATE 2019/12/20 Inoue
        '                    sbSQL.AppendLine("DELETE FROM M_ASSET")
        '                    sbSQL.AppendLine("WHERE")
        '                    sbSQL.AppendLine("  Not EXISTS")
        '                    sbSQL.AppendLine("  (")
        '                    sbSQL.AppendLine("      Select ASSET_NO ")
        '                    'sbSQL.AppendLine("      FROM W_ASSET WK")
        '                    sbSQL.AppendLine("      FROM " & sTmpTblNm & " WK")
        '                    sbSQL.AppendLine("      WHERE WK.ASSET_NO = M_ASSET.ASSET_NO")
        '                    sbSQL.AppendLine("  )")
        '                Else
        '                    sbSQL.AppendLine("UPDATE M_ASSET Set")
        '                    sbSQL.AppendLine("	  ASSET_ID = NULL")

        '                    sbSQL.AppendLine("	, GEN_ASSET_ID = ")
        '                    sbSQL.AppendLine("	  CASE WHEN M_CATEGORY.ASSET_ID_REG_DIV = " & Misc.AssetIDGenerationMethod.None.ToString & " THEN null")
        '                    sbSQL.AppendLine("	       ELSE GEN_ASSET_ID")
        '                    sbSQL.AppendLine("	  END")

        '                    'sbSQL.AppendLine("	, LEASE_CNT = 0")                   '2019/09/18 ここは更新しない
        '                    sbSQL.AppendLine("	, ASSET_STATUS = @ASSET_STATUS")            '9:除却済
        '                    params.Add(New SqlParameter($"@ASSET_STATUS", Misc.ASSET_STATUS.Removed))

        '                    sbSQL.AppendLine("	, EPC_ENC_STATUS = @EPC_ENC_STATUS")                      '0:未
        '                    params.Add(New SqlParameter($"@EPC_ENC_STATUS", Misc.EpcEncStatus.Mi))

        '                    sbSQL.AppendLine("	, TAG_LINK_STATUS = @TAG_LINK_STATUS")                     '0:未
        '                    params.Add(New SqlParameter($"@TAG_LINK_STATUS", Misc.TagLinkStatus.Mi))

        '                    sbSQL.AppendLine("	, LEASE_PLACE_CD = null")
        '                    sbSQL.AppendLine("	, LEASE_PLACE_NAME = null")
        '                    sbSQL.AppendLine("	, REMARK = null")
        '                    sbSQL.AppendLine("	, LEASE_EMPLOYEE_CD = null")
        '                    sbSQL.AppendLine("	, LEASE_EMPLOYEE_NAME = null")
        '                    sbSQL.AppendLine("	, LEASE_DT = null")
        '                    sbSQL.AppendLine("	, LEASE_DUE_DT = null")

        '                    sbSQL.AppendLine("	, RETIRE_TS = @RETIRE_TS")
        '                    params.Add(New SqlParameter($"@RETIRE_TS", Now.ToString("yyyyMMddHHmmss")))

        '                    sbSQL.AppendLine("	, RETIRE_EMPLOYEE_CD = @RETIRE_EMPLOYEE_CD")
        '                    params.Add(New SqlParameter($"@RETIRE_EMPLOYEE_CD", Me._LoginInfo.EmployeeCd))

        '                    sbSQL.AppendLine("	, RETIRE_EMPLOYEE_NAME = @RETIRE_EMPLOYEE_NAME")
        '                    params.Add(New SqlParameter($"@RETIRE_EMPLOYEE_NAME", Me._LoginInfo.EmployeeNm))

        '                    sbSQL.AppendLine("	, RETIRE_ASSET_ID = ASSET_ID")

        '                    sbSQL.AppendLine("	, PHYINV_TS = null")
        '                    sbSQL.AppendLine("	, PHYINV_STATUS = null")
        '                    sbSQL.AppendLine("	, PHYINV_LOC_L_CD = null")
        '                    sbSQL.AppendLine("	, PHYINV_LOC_M_CD = null")
        '                    sbSQL.AppendLine("	, PHYINV_LOC_S_CD = null")
        '                    sbSQL.AppendLine("	, PHYINV_LOC_L_NAME = null")
        '                    sbSQL.AppendLine("	, PHYINV_LOC_M_NAME = null")
        '                    sbSQL.AppendLine("	, PHYINV_LOC_S_NAME = null")
        '                    sbSQL.AppendLine("	, PHYINV_EMPLOYEE_CD = null")
        '                    sbSQL.AppendLine("	, PHYINV_EMPLOYEE_NAME = null")

        '                    sbSQL.AppendLine("  , TASK_TS = FORMAT(GetDate(), 'yyyyMMddHHmmss')")

        '                    sbSQL.AppendLine("  , UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
        '                    sbSQL.AppendLine("  , UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
        '                    sbSQL.AppendLine("  , UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
        '                    sbSQL.AppendLine("  , UPD_TASK = @UPD_TASK")

        '                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", Me._LoginInfo.EmployeeCd))
        '                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", Me._LoginInfo.EmployeeNm))
        '                    params.Add(New SqlParameter($"@UPD_TASK", "AssetListController.ImportCsvFile"))

        '                    '2019/10/24 ADD  資産ID生成方法①の場合、除却時、生成資産IDをクリアする
        '                    sbSQL.AppendLine("FROM M_ASSET")
        '                    sbSQL.AppendLine("LEFT JOIN M_CATEGORY")
        '                    sbSQL.AppendLine("ON   M_CATEGORY.CAT_DIV = '" & CAT_DIV.CategoryL & "'")
        '                    sbSQL.AppendLine("AND  M_CATEGORY.CAT_L_CD = M_ASSET.CAT_L_CD")


        '                    sbSQL.AppendLine("WHERE")
        '                    sbSQL.AppendLine("  Not EXISTS")
        '                    sbSQL.AppendLine("  (")
        '                    sbSQL.AppendLine("      Select ASSET_NO ")
        '                    'sbSQL.AppendLine("      FROM W_ASSET WK")
        '                    sbSQL.AppendLine("      FROM " & sTmpTblNm & " WK")
        '                    sbSQL.AppendLine("      WHERE WK.ASSET_NO = M_ASSET.ASSET_NO")
        '                    sbSQL.AppendLine("  )")
        '                    sbSQL.AppendLine("AND ASSET_STATUS <> '" & Misc.ASSET_STATUS.Removed & "'")   '元々、除却は更新しない
        '                End If


        '                '2023/02/08 Kawamoto ADD START
        '                '資産CSVファイル取込全件対象が「差分取込」の場合
        '            Else
        '                '削除処理
        '                sbSQL.AppendLine("DELETE FROM M_ASSET")
        '                sbSQL.AppendLine("WHERE")
        '                sbSQL.AppendLine("  EXISTS")
        '                sbSQL.AppendLine("  (")
        '                sbSQL.AppendLine("      SELECT ASSET_NO ")
        '                sbSQL.AppendLine("      FROM " & sTmpTblNm & " WK")
        '                sbSQL.AppendLine("      WHERE WK.ASSET_NO = M_ASSET.ASSET_NO")
        '                sbSQL.AppendLine("      AND WK.ACTIVITY = '" & CsvImportProcDiv.Delete & "'")  '処理区分が「D:削除」
        '                sbSQL.AppendLine("  )")

        '                db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)


        '                '除却処理
        '                sbSQL.Clear()
        '                sbSQL.AppendLine("UPDATE M_ASSET Set")
        '                sbSQL.AppendLine("	  ASSET_ID = NULL")
        '                sbSQL.AppendLine("	, GEN_ASSET_ID = ")
        '                sbSQL.AppendLine("	  CASE WHEN M_CATEGORY.ASSET_ID_REG_DIV = " & Misc.AssetIDGenerationMethod.None.ToString & " THEN null")
        '                sbSQL.AppendLine("	       ELSE GEN_ASSET_ID")
        '                sbSQL.AppendLine("	  END")
        '                sbSQL.AppendLine("	, ASSET_STATUS = @ASSET_STATUS")            '9:除却済
        '                params.Add(New SqlParameter($"@ASSET_STATUS", Misc.ASSET_STATUS.Removed))

        '                sbSQL.AppendLine("	, EPC_ENC_STATUS = @EPC_ENC_STATUS")                    '0:未
        '                params.Add(New SqlParameter($"@EPC_ENC_STATUS", Misc.EpcEncStatus.Mi))

        '                sbSQL.AppendLine("	, TAG_LINK_STATUS = @TAG_LINK_STATUS")                  '0:未
        '                params.Add(New SqlParameter($"@TAG_LINK_STATUS", Misc.TagLinkStatus.Mi))

        '                sbSQL.AppendLine("	, LEASE_PLACE_CD = null")
        '                sbSQL.AppendLine("	, LEASE_PLACE_NAME = null")
        '                sbSQL.AppendLine("	, REMARK = null")
        '                sbSQL.AppendLine("	, LEASE_EMPLOYEE_CD = null")
        '                sbSQL.AppendLine("	, LEASE_EMPLOYEE_NAME = null")
        '                sbSQL.AppendLine("	, LEASE_DT = null")
        '                sbSQL.AppendLine("	, LEASE_DUE_DT = null")

        '                sbSQL.AppendLine("	, RETIRE_TS = @RETIRE_TS")
        '                params.Add(New SqlParameter($"@RETIRE_TS", Now.ToString("yyyyMMddHHmmss")))

        '                sbSQL.AppendLine("	, RETIRE_EMPLOYEE_CD = @RETIRE_EMPLOYEE_CD")
        '                params.Add(New SqlParameter($"@RETIRE_EMPLOYEE_CD", Me._LoginInfo.EmployeeCd))

        '                sbSQL.AppendLine("	, RETIRE_EMPLOYEE_NAME = @RETIRE_EMPLOYEE_NAME")
        '                params.Add(New SqlParameter($"@RETIRE_EMPLOYEE_NAME", Me._LoginInfo.EmployeeNm))

        '                sbSQL.AppendLine("	, RETIRE_ASSET_ID = ASSET_ID")

        '                sbSQL.AppendLine("	, PHYINV_TS = null")
        '                sbSQL.AppendLine("	, PHYINV_STATUS = null")
        '                sbSQL.AppendLine("	, PHYINV_LOC_L_CD = null")
        '                sbSQL.AppendLine("	, PHYINV_LOC_M_CD = null")
        '                sbSQL.AppendLine("	, PHYINV_LOC_S_CD = null")
        '                sbSQL.AppendLine("	, PHYINV_LOC_L_NAME = null")
        '                sbSQL.AppendLine("	, PHYINV_LOC_M_NAME = null")
        '                sbSQL.AppendLine("	, PHYINV_LOC_S_NAME = null")
        '                sbSQL.AppendLine("	, PHYINV_EMPLOYEE_CD = null")
        '                sbSQL.AppendLine("	, PHYINV_EMPLOYEE_NAME = null")

        '                sbSQL.AppendLine("  , TASK_TS = FORMAT(GetDate(), 'yyyyMMddHHmmss')")

        '                sbSQL.AppendLine("  , UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
        '                sbSQL.AppendLine("  , UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
        '                sbSQL.AppendLine("  , UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
        '                sbSQL.AppendLine("  , UPD_TASK = @UPD_TASK")

        '                params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", Me._LoginInfo.EmployeeCd))
        '                params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", Me._LoginInfo.EmployeeNm))
        '                params.Add(New SqlParameter($"@UPD_TASK", "AssetListController.ImportCsvFile"))

        '                sbSQL.AppendLine("FROM M_ASSET")
        '                sbSQL.AppendLine("LEFT JOIN M_CATEGORY")
        '                sbSQL.AppendLine("ON   M_CATEGORY.CAT_DIV = '" & CAT_DIV.CategoryL & "'")
        '                sbSQL.AppendLine("AND  M_CATEGORY.CAT_L_CD = M_ASSET.CAT_L_CD")

        '                sbSQL.AppendLine("WHERE")
        '                sbSQL.AppendLine("  EXISTS")
        '                sbSQL.AppendLine("  (")
        '                sbSQL.AppendLine("      SELECT ASSET_NO ")
        '                sbSQL.AppendLine("      FROM " & sTmpTblNm & " WK")
        '                sbSQL.AppendLine("      WHERE WK.ASSET_NO = M_ASSET.ASSET_NO")
        '                sbSQL.AppendLine("      AND WK.ACTIVITY = '" & CsvImportProcDiv.Retirement & "'")  '処理区分が「R:除却」
        '                sbSQL.AppendLine("  )")
        '                sbSQL.AppendLine("AND ASSET_STATUS <> '" & Misc.ASSET_STATUS.Removed & "'")   '元々、除却は更新しない
        '            End If
        '            '2023/02/08 Kawamoto ADD END

        '            db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)


        '            ''●●●
        '            'logger.Info("STEP7 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

        '            '--先に資産処理履歴ファイルを更新する
        '            '挿入分
        '            sbSQL.Clear()
        '            params = New List(Of SqlParameter)

        '            sbSQL.AppendLine("INSERT INTO T_PROCESS_LOG")
        '            sbSQL.AppendLine("(")
        '            sbSQL.AppendLine("   PROCESS_DT")
        '            sbSQL.AppendLine("  ,PROCESS_TM")
        '            sbSQL.AppendLine("  ,ASSET_NO")
        '            sbSQL.AppendLine("  ,SUB_ASSET_NO")
        '            sbSQL.AppendLine("  ,ASSET_ID")
        '            sbSQL.AppendLine("  ,PROCESS_TOOL_DIV")
        '            sbSQL.AppendLine("  ,PROCESS_DIV")
        '            sbSQL.AppendLine("  ,LEASE_PLACE_CD")
        '            sbSQL.AppendLine("  ,LEASE_PLACE_NAME")
        '            sbSQL.AppendLine("  ,REMARK")   '2019/11/19 nv miyaji add
        '            sbSQL.AppendLine("  ,LEASE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,LEASE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,LEASE_DT") '2020/03/06 nv miyaji add
        '            sbSQL.AppendLine("  ,LEASE_DUE_DT")
        '            sbSQL.AppendLine("  ,LOC_L_CD")
        '            sbSQL.AppendLine("  ,LOC_M_CD")
        '            sbSQL.AppendLine("  ,LOC_S_CD")
        '            sbSQL.AppendLine("  ,LOC_L_NAME")
        '            sbSQL.AppendLine("  ,LOC_M_NAME")
        '            sbSQL.AppendLine("  ,LOC_S_NAME")
        '            sbSQL.AppendLine("  ,CAT_L_CD")
        '            sbSQL.AppendLine("  ,CAT_NAME")
        '            sbSQL.AppendLine("  ,ASSET_NAME")
        '            sbSQL.AppendLine("  ,MAKER")
        '            sbSQL.AppendLine("  ,PRODUCT_NO")
        '            sbSQL.AppendLine("  ,MODEL_NO")
        '            '--2021/10/05 nv miyaji add start
        '            sbSQL.AppendLine("  ,MNG_LOC_L_CD ")
        '            sbSQL.AppendLine("  ,LEASE_STATUS ")
        '            '--2021/10/05 nv miyaji add end
        '            sbSQL.AppendLine("  ,TASK_RESULT")
        '            sbSQL.AppendLine("  ,UPD_DATETIME")
        '            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,UPD_TASK")
        '            sbSQL.AppendLine(")")
        '            sbSQL.AppendLine("SELECT")
        '            sbSQL.AppendLine("   CONVERT(int, FORMAT(GetDate(), 'yyyyMMdd'))")
        '            sbSQL.AppendLine("  ,FORMAT(GetDate(), 'HHmmssfff')")
        '            sbSQL.AppendLine("  ,ASSET.ASSET_NO")
        '            sbSQL.AppendLine("  ,ASSET.SUB_ASSET_NO")
        '            sbSQL.AppendLine("  ,ASSET.ASSET_ID")
        '            sbSQL.AppendLine("  ,'" & Misc.ProcessToolDiv.Web & "'")
        '            sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.CI & "'")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_CD")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_NAME")
        '            sbSQL.AppendLine("  ,ASSET.REMARK")   '2019/11/19 nv miyaji add
        '            sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_DT") '2020/03/06 nv miyaji add
        '            sbSQL.AppendLine("  ,ASSET.LEASE_DUE_DT")
        '            sbSQL.AppendLine("  ,ASSET.LOC_L_CD")
        '            sbSQL.AppendLine("  ,ASSET.LOC_M_CD")
        '            sbSQL.AppendLine("  ,ASSET.LOC_S_CD")
        '            sbSQL.AppendLine("  ,LOCL.LOC_NAME AS LocLName ")
        '            sbSQL.AppendLine("  ,LOCM.LOC_NAME AS LocMName ")
        '            sbSQL.AppendLine("  ,LOCS.LOC_NAME AS LocSName ")
        '            sbSQL.AppendLine("  ,ASSET.CAT_L_CD")
        '            sbSQL.AppendLine("  ,CAT.CAT_NAME AS CatName ")
        '            sbSQL.AppendLine("  ,ASSET.ASSET_NAME")
        '            sbSQL.AppendLine("  ,ASSET.MAKER")
        '            sbSQL.AppendLine("  ,ASSET.PRODUCT_NO")
        '            sbSQL.AppendLine("  ,ASSET.MODEL_NO")
        '            '--2021/10/05 nv miyaji add start
        '            sbSQL.AppendLine("  ,ASSET.MNG_LOC_L_CD ")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_STATUS ")
        '            '--2021/10/05 nv miyaji add end
        '            sbSQL.AppendLine("  ,'" & Misc.TaskResult.OK & "'")
        '            sbSQL.AppendLine("  ,ASSET.UPD_DATETIME")
        '            sbSQL.AppendLine("  ,ASSET.UPD_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,ASSET.UPD_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,ASSET.UPD_TASK")
        '            sbSQL.AppendLine("FROM " & sTmpTblNm & " ASSET")

        '            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
        '            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
        '            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
        '            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM ")
        '            sbSQL.AppendLine("ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
        '            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCM.LOC_L_CD")
        '            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
        '            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS ")
        '            sbSQL.AppendLine("ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
        '            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCS.LOC_L_CD")
        '            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCS.LOC_M_CD")
        '            sbSQL.AppendLine("AND ASSET.LOC_S_CD = LOCS.LOC_S_CD")
        '            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT ")
        '            sbSQL.AppendLine("ON CAT.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
        '            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CAT.CAT_L_CD")

        '            '2023/02/08 Kawamoto UPD START
        '            'sbSQL.AppendLine("WHERE")
        '            'sbSQL.AppendLine("  Not EXISTS")
        '            'sbSQL.AppendLine("  (")
        '            'sbSQL.AppendLine("      Select ASSET_NO ")
        '            'sbSQL.AppendLine("      FROM M_ASSET MST")
        '            'sbSQL.AppendLine("      WHERE MST.ASSET_NO = ASSET.ASSET_NO")
        '            'sbSQL.AppendLine("  )")

        '            '資産CSVファイル取込全件対象が「全件対象」の場合
        '            If CsvRegistALL.DATA1 = AssetCSVRegistAll.All Then
        '                sbSQL.AppendLine("WHERE")
        '                sbSQL.AppendLine("  Not EXISTS")
        '                sbSQL.AppendLine("  (")
        '                sbSQL.AppendLine("      Select ASSET_NO ")
        '                sbSQL.AppendLine("      FROM M_ASSET MST")
        '                sbSQL.AppendLine("      WHERE MST.ASSET_NO = ASSET.ASSET_NO")
        '                sbSQL.AppendLine("  )")

        '                '資産CSVファイル取込全件対象が「差分取込」の場合
        '            Else
        '                sbSQL.AppendLine("INNER JOIN " & sTmpTblNm & " WK")
        '                sbSQL.AppendLine("   ON WK.ASSET_NO = ASSET.ASSET_NO")
        '                sbSQL.AppendLine("WHERE")
        '                sbSQL.AppendLine("   WK.ACTIVITY = '" & CsvImportProcDiv.Insert & "'")  '処理区分が「I:追加」
        '            End If
        '            '2023/02/08 Kawamoto UPD END

        '            db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)


        '            ''●●●
        '            'logger.Info("STEP8 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

        '            '--先に資産処理履歴ファイルを更新する
        '            '更新分
        '            sbSQL.Clear()
        '            params = New List(Of SqlParameter)

        '            sbSQL.AppendLine("INSERT INTO T_PROCESS_LOG")
        '            sbSQL.AppendLine("(")
        '            sbSQL.AppendLine("   PROCESS_DT")
        '            sbSQL.AppendLine("  ,PROCESS_TM")
        '            sbSQL.AppendLine("  ,ASSET_NO")
        '            sbSQL.AppendLine("  ,SUB_ASSET_NO")
        '            sbSQL.AppendLine("  ,ASSET_ID")
        '            sbSQL.AppendLine("  ,PROCESS_TOOL_DIV")
        '            sbSQL.AppendLine("  ,PROCESS_DIV")
        '            sbSQL.AppendLine("  ,LEASE_PLACE_CD")
        '            sbSQL.AppendLine("  ,LEASE_PLACE_NAME")
        '            sbSQL.AppendLine("  ,REMARK")   '2019/11/19 nv miyaji add
        '            sbSQL.AppendLine("  ,LEASE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,LEASE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,LEASE_DT") '2020/03/06 nv miyaji add
        '            sbSQL.AppendLine("  ,LEASE_DUE_DT")
        '            sbSQL.AppendLine("  ,LOC_L_CD")
        '            sbSQL.AppendLine("  ,LOC_M_CD")
        '            sbSQL.AppendLine("  ,LOC_S_CD")
        '            sbSQL.AppendLine("  ,LOC_L_NAME")
        '            sbSQL.AppendLine("  ,LOC_M_NAME")
        '            sbSQL.AppendLine("  ,LOC_S_NAME")
        '            sbSQL.AppendLine("  ,CAT_L_CD")
        '            sbSQL.AppendLine("  ,CAT_NAME ")
        '            sbSQL.AppendLine("  ,ASSET_NAME")
        '            sbSQL.AppendLine("  ,MAKER")
        '            sbSQL.AppendLine("  ,PRODUCT_NO")
        '            sbSQL.AppendLine("  ,MODEL_NO")
        '            '--2021/10/05 nv miyaji add start
        '            sbSQL.AppendLine("  ,MNG_LOC_L_CD ")
        '            sbSQL.AppendLine("  ,LEASE_STATUS ")
        '            '--2021/10/05 nv miyaji add end
        '            sbSQL.AppendLine("  ,TASK_RESULT")
        '            sbSQL.AppendLine("  ,UPD_DATETIME")
        '            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,UPD_TASK")
        '            sbSQL.AppendLine(")")
        '            sbSQL.AppendLine("SELECT")
        '            sbSQL.AppendLine("   CONVERT(int, FORMAT(GetDate(), 'yyyyMMdd'))")
        '            sbSQL.AppendLine("  ,FORMAT(GetDate(), 'HHmmssfff')")
        '            sbSQL.AppendLine("  ,ASSET.ASSET_NO")
        '            sbSQL.AppendLine("  ,ASSET.SUB_ASSET_NO")
        '            sbSQL.AppendLine("  ,ASSET.ASSET_ID")
        '            sbSQL.AppendLine("  ,'" & Misc.ProcessToolDiv.Web & "'")
        '            sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.CU & "'")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_CD")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_NAME")
        '            sbSQL.AppendLine("  ,ASSET.REMARK")   '2019/11/19 nv miyaji add
        '            sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_DT") '2020/03/06 nv miyaji add
        '            sbSQL.AppendLine("  ,ASSET.LEASE_DUE_DT")
        '            sbSQL.AppendLine("  ,ASSET.LOC_L_CD")
        '            sbSQL.AppendLine("  ,ASSET.LOC_M_CD")
        '            sbSQL.AppendLine("  ,ASSET.LOC_S_CD")
        '            sbSQL.AppendLine("  ,LOCL.LOC_NAME AS LocLName ")
        '            sbSQL.AppendLine("  ,LOCM.LOC_NAME AS LocMName ")
        '            sbSQL.AppendLine("  ,LOCS.LOC_NAME AS LocSName ")
        '            sbSQL.AppendLine("  ,ASSET.CAT_L_CD")
        '            sbSQL.AppendLine("  ,CAT.CAT_NAME AS CatName ")
        '            sbSQL.AppendLine("  ,ASSET.ASSET_NAME")
        '            sbSQL.AppendLine("  ,ASSET.MAKER")
        '            sbSQL.AppendLine("  ,ASSET.PRODUCT_NO")
        '            sbSQL.AppendLine("  ,ASSET.MODEL_NO")
        '            '--2021/10/05 nv miyaji add start
        '            sbSQL.AppendLine("  ,ASSET.MNG_LOC_L_CD ")
        '            sbSQL.AppendLine("  ,ASSET.LEASE_STATUS ")
        '            '--2021/10/05 nv miyaji add end
        '            sbSQL.AppendLine("  ,'" & Misc.TaskResult.OK & "'")
        '            sbSQL.AppendLine("  ,ASSET.UPD_DATETIME")
        '            sbSQL.AppendLine("  ,ASSET.UPD_EMPLOYEE_CD")
        '            sbSQL.AppendLine("  ,ASSET.UPD_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("  ,ASSET.UPD_TASK")
        '            sbSQL.AppendLine("FROM " & sTmpTblNm & " ASSET")

        '            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
        '            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
        '            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
        '            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM ")
        '            sbSQL.AppendLine("ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
        '            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCM.LOC_L_CD")
        '            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
        '            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS ")
        '            sbSQL.AppendLine("ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
        '            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCS.LOC_L_CD")
        '            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCS.LOC_M_CD")
        '            sbSQL.AppendLine("AND ASSET.LOC_S_CD = LOCS.LOC_S_CD")
        '            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT ")
        '            sbSQL.AppendLine("ON CAT.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
        '            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CAT.CAT_L_CD")

        '            sbSQL.AppendLine("WHERE")
        '            sbSQL.AppendLine("  EXISTS")
        '            sbSQL.AppendLine("  (")
        '            sbSQL.AppendLine("      Select ASSET_NO ")
        '            sbSQL.AppendLine("      FROM M_ASSET MST")
        '            sbSQL.AppendLine("      WHERE MST.ASSET_NO = ASSET.ASSET_NO")
        '            sbSQL.AppendLine("  )")
        '            sbSQL.AppendLine("AND  ASSET.UPD_FLG <> 0")

        '            '2023/02/08 Kawamoto ADD START
        '            '資産CSVファイル取込全件対象が「差分取込」の場合
        '            If CsvRegistALL.DATA1 = AssetCSVRegistAll.Difference Then
        '                sbSQL.AppendLine("AND ASSET.ACTIVITY = '" & CsvImportProcDiv.Update & "'")  '処理区分が「U:更新」
        '            End If
        '            '2023/02/08 Kawamoto ADD END

        '            db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

        '            '2023/02/08 Kawamoto ADD START
        '            '薬品機能がONの場合
        '            If DrugsFunc.DATA1 = DrugsFunction.Yes Then

        '                '◆薬品重量履歴の削除
        '                sbSQL.Clear()

        '                sbSQL.AppendLine("DELETE DUELOG")
        '                sbSQL.AppendLine("FROM T_DRUG_DUE_LOG DUELOG")
        '                sbSQL.AppendLine("WHERE EXISTS")
        '                sbSQL.AppendLine("(")
        '                sbSQL.AppendLine("  SELECT 1 FROM M_ASSET ASSET")
        '                sbSQL.AppendLine("   INNER JOIN " & sTmpTblNm & " VALT")
        '                sbSQL.AppendLine("      ON ASSET.ASSET_NO = VALT.ASSET_NO")
        '                sbSQL.AppendLine("   INNER JOIN")
        '                sbSQL.AppendLine("      (SELECT ASSET_NO, MAX(DRUG_WORK_DIV) AS DRUG_WORK_DIV FROM T_DRUG_DUE_LOG GROUP BY ASSET_NO) DUE")
        '                sbSQL.AppendLine("      ON DUE.ASSET_NO = VALT.ASSET_NO")
        '                sbSQL.AppendLine("   WHERE DRUG_WORK_DIV = '" & DrugsWorkDiv.Accept & "'")
        '                sbSQL.AppendLine("     AND VALT.UPD_FLG <> 0")
        '                sbSQL.AppendLine("     AND DUELOG.ASSET_NO = VALT.ASSET_NO")
        '                '資産CSVファイル取込全件対象が「差分取込」の場合
        '                If CsvRegistALL.DATA1 = AssetCSVRegistAll.Difference Then
        '                    sbSQL.AppendLine("AND VALT.ACTIVITY = '" & CsvImportProcDiv.Update & "'")  '処理区分が「U:更新」
        '                End If
        '                sbSQL.AppendLine(")")

        '                db.Database.ExecuteSqlCommand(sbSQL.ToString)


        '                '◆薬品重量履歴の登録
        '                sbSQL.Clear()

        '                sbSQL.AppendLine("INSERT INTO T_DRUG_DUE_LOG")
        '                sbSQL.AppendLine("(")
        '                sbSQL.AppendLine("     DRUG_WORK_DIV")
        '                sbSQL.AppendLine("    ,PROCESS_DT")
        '                sbSQL.AppendLine("    ,PROCESS_TM")
        '                sbSQL.AppendLine("    ,ASSET_NO")
        '                sbSQL.AppendLine("    ,WORKER_EMPLOYEE_CD")
        '                sbSQL.AppendLine("    ,WORKER_EMPLOYEE_NAME")   '2023/03/22 Kawamoto ADD
        '                sbSQL.AppendLine("    ,RESPONSE_EMPLOYEE_CD")
        '                sbSQL.AppendLine("    ,RESPONSE_EMPLOYEE_NAME") '2023/03/22 Kawamoto ADD
        '                sbSQL.AppendLine("    ,USE_QTY")
        '                sbSQL.AppendLine("    ,DUE_QTY")
        '                sbSQL.AppendLine("    ,REMAIN_QTY")
        '                sbSQL.AppendLine("    ,WORK_OUTLINE")
        '                sbSQL.AppendLine("    ,WORK_TIME")
        '                sbSQL.AppendLine("    ,REMARK1")
        '                sbSQL.AppendLine("    ,REMARK2")
        '                sbSQL.AppendLine("    ,UPD_DATETIME")
        '                sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
        '                sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
        '                sbSQL.AppendLine("    ,UPD_TASK")
        '                sbSQL.AppendLine(")")

        '                '新規登録分
        '                sbSQL.AppendLine("SELECT")
        '                sbSQL.AppendLine("     '" & DrugsWorkDiv.Accept & "'")
        '                sbSQL.AppendLine("    ,REPLACE(VALT.FREE_ITEM13, '/', '')")
        '                sbSQL.AppendLine("    ,'000000000'")
        '                sbSQL.AppendLine("    ,VALT.ASSET_NO")
        '                sbSQL.AppendLine("    ,EMP.EMPLOYEE_CD")
        '                sbSQL.AppendLine("    ,VALT.FREE_ITEM9")    '2023/03/22 Kawamoto ADD
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,NULL")               '2023/03/22 Kawamoto ADD
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,VALT.FREE_ITEM16")
        '                sbSQL.AppendLine("    ,CASE WHEN DATALENGTH(VALT.FREE_ITEM11) = 0 THEN CONVERT(decimal, VALT.FREE_ITEM16) - (CONVERT(decimal, VALT.FREE_ITEM14) - CONVERT(decimal, VALT.FREE_ITEM10))")
        '                sbSQL.AppendLine("                                                ELSE CONVERT(decimal, VALT.FREE_ITEM16) - (CONVERT(decimal, VALT.FREE_ITEM14) - (CONVERT(decimal, VALT.FREE_ITEM11) * CONVERT(decimal, VALT.FREE_ITEM12)))")
        '                sbSQL.AppendLine("     END")
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,VALT.UPD_DATETIME")
        '                sbSQL.AppendLine("    ,VALT.UPD_EMPLOYEE_CD")
        '                sbSQL.AppendLine("    ,VALT.UPD_EMPLOYEE_NAME")
        '                sbSQL.AppendLine("    ,VALT.UPD_TASK")
        '                sbSQL.AppendLine("FROM " & sTmpTblNm & " VALT")
        '                sbSQL.AppendLine("LEFT OUTER JOIN M_EMPLOYEE EMP")
        '                sbSQL.AppendLine("  ON EMP.EMPLOYEE_NAME = VALT.FREE_ITEM9")
        '                sbSQL.AppendLine("WHERE NOT EXISTS")
        '                sbSQL.AppendLine("(")
        '                sbSQL.AppendLine("  SELECT 1 FROM M_ASSET")
        '                sbSQL.AppendLine("  WHERE M_ASSET.ASSET_NO = VALT.ASSET_NO")
        '                sbSQL.AppendLine(")")
        '                '資産CSVファイル取込全件対象が「差分取込」の場合
        '                If CsvRegistALL.DATA1 = AssetCSVRegistAll.Difference Then
        '                    sbSQL.AppendLine("AND VALT.ACTIVITY = '" & CsvImportProcDiv.Insert & "'")  '処理区分が「I:追加」
        '                End If

        '                sbSQL.AppendLine("UNION")

        '                '更新分
        '                sbSQL.AppendLine("SELECT")
        '                sbSQL.AppendLine("     '" & DrugsWorkDiv.Accept & "'")
        '                sbSQL.AppendLine("    ,REPLACE(VALT.FREE_ITEM13, '/', '')")
        '                sbSQL.AppendLine("    ,'000000000'")
        '                sbSQL.AppendLine("    ,VALT.ASSET_NO")
        '                sbSQL.AppendLine("    ,EMP.EMPLOYEE_CD")
        '                sbSQL.AppendLine("    ,VALT.FREE_ITEM9")    '2023/03/22 Kawamoto ADD
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,NULL")               '2023/03/22 Kawamoto ADD
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,VALT.FREE_ITEM16")
        '                sbSQL.AppendLine("    ,CASE WHEN DATALENGTH(VALT.FREE_ITEM11) = 0 THEN CONVERT(decimal, VALT.FREE_ITEM16) - (CONVERT(decimal, VALT.FREE_ITEM14) - CONVERT(decimal, VALT.FREE_ITEM10))")
        '                sbSQL.AppendLine("                                                ELSE CONVERT(decimal, VALT.FREE_ITEM16) - (CONVERT(decimal, VALT.FREE_ITEM14) - (CONVERT(decimal, VALT.FREE_ITEM11) * CONVERT(decimal, VALT.FREE_ITEM12)))")
        '                sbSQL.AppendLine("     END")
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,NULL")
        '                sbSQL.AppendLine("    ,VALT.UPD_DATETIME")
        '                sbSQL.AppendLine("    ,VALT.UPD_EMPLOYEE_CD")
        '                sbSQL.AppendLine("    ,VALT.UPD_EMPLOYEE_NAME")
        '                sbSQL.AppendLine("    ,VALT.UPD_TASK")
        '                sbSQL.AppendLine("FROM " & sTmpTblNm & " VALT")
        '                sbSQL.AppendLine("INNER JOIN M_ASSET ASSET")
        '                sbSQL.AppendLine("   ON ASSET.ASSET_NO = VALT.ASSET_NO")
        '                sbSQL.AppendLine(" LEFT OUTER JOIN M_EMPLOYEE EMP")
        '                sbSQL.AppendLine("   ON EMP.EMPLOYEE_NAME = VALT.FREE_ITEM9")
        '                sbSQL.AppendLine("WHERE")
        '                sbSQL.AppendLine("      NOT EXISTS(SELECT 1 FROM T_DRUG_DUE_LOG LOG WHERE LOG.ASSET_NO = VALT.ASSET_NO)")
        '                sbSQL.AppendLine("  AND VALT.UPD_FLG <> 0")
        '                '資産CSVファイル取込全件対象が「差分取込」の場合
        '                If CsvRegistALL.DATA1 = AssetCSVRegistAll.Difference Then
        '                    sbSQL.AppendLine("AND VALT.ACTIVITY = '" & CsvImportProcDiv.Update & "'")  '処理区分が「U:更新」
        '                End If

        '                db.Database.ExecuteSqlCommand(sbSQL.ToString)
        '            End If


        '            ''●●●
        '            'logger.Info("STEP9 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

        '            '更新処理  2019/10/15 Inoue 
        '            sbSQL.Clear()

        '            sbSQL.AppendLine("UPDATE M_ASSET SET")
        '            sbSQL.AppendLine("     ASSET_NO = VALT.ASSET_NO")
        '            sbSQL.AppendLine("    ,ASSET_ID = VALT.ASSET_ID")
        '            sbSQL.AppendLine("    ,EPC_SIZE = VALT.EPC_SIZE")
        '            sbSQL.AppendLine("    ,GEN_ASSET_ID = VALT.GEN_ASSET_ID")
        '            sbSQL.AppendLine("    ,ASSET_DIV = VALT.ASSET_DIV")
        '            sbSQL.AppendLine("    ,ASSET_NAME = VALT.ASSET_NAME")
        '            sbSQL.AppendLine("    ,MNG_LOC_L_CD = VALT.MNG_LOC_L_CD")   '2021/10/05 nv miyaji add
        '            sbSQL.AppendLine("    ,LOC_L_CD = VALT.LOC_L_CD")
        '            sbSQL.AppendLine("    ,LOC_M_CD = VALT.LOC_M_CD")
        '            sbSQL.AppendLine("    ,LOC_S_CD = VALT.LOC_S_CD")
        '            sbSQL.AppendLine("    ,CAT_L_CD = VALT.CAT_L_CD")
        '            sbSQL.AppendLine("    ,CAT_M_CD = VALT.CAT_M_CD")
        '            sbSQL.AppendLine("    ,CAT_S_CD = VALT.CAT_S_CD")
        '            sbSQL.AppendLine("    ,MAKER = VALT.MAKER")
        '            sbSQL.AppendLine("    ,PRODUCT_NO = VALT.PRODUCT_NO")
        '            sbSQL.AppendLine("    ,MODEL_NO = VALT.MODEL_NO")
        '            sbSQL.AppendLine("    ,LEASE_NO = VALT.LEASE_NO")
        '            sbSQL.AppendLine("    ,LEASE_LIMIT = VALT.LEASE_LIMIT")
        '            sbSQL.AppendLine("    ,GENERIC_LIMIT = VALT.GENERIC_LIMIT")
        '            sbSQL.AppendLine("    ,FREE_ITEM1 = VALT.FREE_ITEM1")
        '            sbSQL.AppendLine("    ,FREE_ITEM2 = VALT.FREE_ITEM2")
        '            sbSQL.AppendLine("    ,FREE_ITEM3 = VALT.FREE_ITEM3")
        '            sbSQL.AppendLine("    ,FREE_ITEM4 = VALT.FREE_ITEM4")
        '            sbSQL.AppendLine("    ,FREE_ITEM5 = VALT.FREE_ITEM5")
        '            sbSQL.AppendLine("    ,FREE_ITEM6 = VALT.FREE_ITEM6")
        '            sbSQL.AppendLine("    ,FREE_ITEM7 = VALT.FREE_ITEM7")
        '            sbSQL.AppendLine("    ,FREE_ITEM8 = VALT.FREE_ITEM8")
        '            sbSQL.AppendLine("    ,FREE_ITEM9 = VALT.FREE_ITEM9")
        '            sbSQL.AppendLine("    ,FREE_ITEM10 = VALT.FREE_ITEM10")
        '            '2023/02/08 Kawamoto ADD START
        '            sbSQL.AppendLine("    ,FREE_ITEM11 = VALT.FREE_ITEM11")
        '            sbSQL.AppendLine("    ,FREE_ITEM12 = VALT.FREE_ITEM12")
        '            sbSQL.AppendLine("    ,FREE_ITEM13 = VALT.FREE_ITEM13")
        '            sbSQL.AppendLine("    ,FREE_ITEM14 = VALT.FREE_ITEM14")
        '            sbSQL.AppendLine("    ,FREE_ITEM15 = VALT.FREE_ITEM15")
        '            sbSQL.AppendLine("    ,FREE_ITEM16 = VALT.FREE_ITEM16")
        '            sbSQL.AppendLine("    ,FREE_ITEM17 = VALT.FREE_ITEM17")
        '            sbSQL.AppendLine("    ,FREE_ITEM18 = VALT.FREE_ITEM18")
        '            sbSQL.AppendLine("    ,FREE_ITEM19 = VALT.FREE_ITEM19")
        '            '2023/02/08 Kawamoto ADD END
        '            sbSQL.AppendLine("    ,ASSET_PIC_FILENAME = VALT.ASSET_PIC_FILENAME")
        '            sbSQL.AppendLine("    ,LEASE_CNT = VALT.LEASE_CNT")
        '            sbSQL.AppendLine("    ,ASSET_STATUS = VALT.ASSET_STATUS")
        '            sbSQL.AppendLine("    ,EPC_ENC_STATUS = VALT.EPC_ENC_STATUS")
        '            sbSQL.AppendLine("    ,TAG_LINK_STATUS = VALT.TAG_LINK_STATUS")
        '            sbSQL.AppendLine("    ,LEASE_STATUS = VALT.LEASE_STATUS")   '2021/10/05 nv miyaji add
        '            sbSQL.AppendLine("    ,LEASE_PLACE_CD = VALT.LEASE_PLACE_CD")
        '            sbSQL.AppendLine("    ,LEASE_PLACE_NAME = VALT.LEASE_PLACE_NAME")
        '            sbSQL.AppendLine("    ,REMARK = VALT.REMARK")
        '            sbSQL.AppendLine("    ,LEASE_EMPLOYEE_CD = VALT.LEASE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,LEASE_EMPLOYEE_NAME = VALT.LEASE_EMPLOYEE_NAME")
        '            'sbSQL.AppendLine("    ,LEASE_DT = VALT.LEASE_DT")          '2020/12/08 kawamoto 更新しないように修正
        '            'sbSQL.AppendLine("    ,LEASE_DUE_DT = VALT.LEASE_DUE_DT")  '2020/12/08 kawamoto 更新しないように修正
        '            '--2021/10/05 nv miyaji add start
        '            sbSQL.AppendLine("    ,MAINTE_LEASE_NUM = VALT.MAINTE_LEASE_NUM")
        '            sbSQL.AppendLine("    ,MAINTE_LEASE_DAY = VALT.MAINTE_LEASE_DAY")
        '            sbSQL.AppendLine("    ,MAINTE_LEASE_DAY_WORK_FLAG = VALT.MAINTE_LEASE_DAY_WORK_FLAG")
        '            sbSQL.AppendLine("    ,MAINTE_DAY = VALT.MAINTE_DAY")
        '            sbSQL.AppendLine("    ,MAINTE_DAY_WORK_FLAG = VALT.MAINTE_DAY_WORK_FLAG")
        '            '--2021/10/05 nv miyaji add end
        '            sbSQL.AppendLine("    ,RETIRE_TS = VALT.RETIRE_TS")
        '            sbSQL.AppendLine("    ,RETIRE_EMPLOYEE_CD = VALT.RETIRE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,RETIRE_EMPLOYEE_NAME = VALT.RETIRE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,RETIRE_ASSET_ID = VALT.RETIRE_ASSET_ID")
        '            sbSQL.AppendLine("    ,PHYINV_TS = VALT.PHYINV_TS")

        '            '--2020/02/12 nv inoue update start  棚卸開始チェック(資産状態 = 1:保管中 and　元の資産状態 in (0:準備中,9:除却済) )  で登録する場合のみ)
        '            'sbSQL.AppendLine("    ,PHYINV_STATUS = VALT.PHYINV_STATUS")
        '            sbSQL.AppendLine("    ,PHYINV_STATUS = ")
        '            sbSQL.AppendLine("     CASE WHEN (" & Dao.ToolCommon.GetSqlStrChkInvt() & ") > 0 ")
        '            sbSQL.AppendLine("     　　　 AND VALT.ASSET_STATUS = '" & Misc.ASSET_STATUS.Storage & "'")
        '            sbSQL.AppendLine("     　　　 AND M_ASSET.ASSET_STATUS in ('" & Misc.ASSET_STATUS.Preparation & "'")
        '            sbSQL.AppendLine("     　　　                              ,'" & Misc.ASSET_STATUS.Removed & "') Then '" & Misc.PHYINV_STATUS.NoSpot & "'")
        '            sbSQL.AppendLine("          ELSE VALT.PHYINV_STATUS  End")
        '            '--2020/02/12 nv inoue update end

        '            sbSQL.AppendLine("    ,PHYINV_LOC_L_CD = VALT.PHYINV_LOC_L_CD")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_M_CD = VALT.PHYINV_LOC_M_CD")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_S_CD = VALT.PHYINV_LOC_S_CD")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_L_NAME = VALT.PHYINV_LOC_L_NAME")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_M_NAME = VALT.PHYINV_LOC_M_NAME")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_S_NAME = VALT.PHYINV_LOC_S_NAME")
        '            sbSQL.AppendLine("    ,PHYINV_EMPLOYEE_CD = VALT.PHYINV_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,PHYINV_EMPLOYEE_NAME = VALT.PHYINV_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,TASK_TS = VALT.TASK_TS")
        '            sbSQL.AppendLine("    ,INS_DATETIME = VALT.INS_DATETIME")
        '            sbSQL.AppendLine("    ,INS_EMPLOYEE_CD = VALT.INS_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME = VALT.INS_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,UPD_DATETIME = VALT.UPD_DATETIME")
        '            sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = VALT.UPD_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = VALT.UPD_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,UPD_TASK = VALT.UPD_TASK")

        '            sbSQL.AppendLine("FROM M_ASSET")
        '            sbSQL.AppendLine("INNER JOIN " & sTmpTblNm & " VALT")
        '            sbSQL.AppendLine("ON  M_ASSET.ASSET_NO = VALT.ASSET_NO")
        '            sbSQL.AppendLine("WHERE  VALT.UPD_FLG <> 0")

        '            '2023/02/08 Kawamoto ADD START
        '            '資産CSVファイル取込全件対象が「差分取込」の場合
        '            If CsvRegistALL.DATA1 = AssetCSVRegistAll.Difference Then
        '                sbSQL.AppendLine("AND VALT.ACTIVITY = '" & CsvImportProcDiv.Update & "'")  '処理区分が「U:更新」
        '            End If
        '            '2023/02/08 Kawamoto ADD END

        '            db.Database.ExecuteSqlCommand(sbSQL.ToString)


        '            ''●●●
        '            'logger.Info("STEP10 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

        '            '挿入・更新処理  2019/09/12 Inoue ADD
        '            sbSQL.Clear()

        '            sbSQL.AppendLine("  INSERT INTO M_ASSET")
        '            sbSQL.AppendLine("  (")

        '            sbSQL.AppendLine("     ASSET_NO")
        '            sbSQL.AppendLine("    ,ASSET_ID")
        '            sbSQL.AppendLine("    ,EPC_SIZE")
        '            sbSQL.AppendLine("    ,GEN_ASSET_ID")
        '            sbSQL.AppendLine("    ,ASSET_DIV")
        '            sbSQL.AppendLine("    ,ASSET_NAME")
        '            sbSQL.AppendLine("    ,MNG_LOC_L_CD")   '2021/10/05 nv miyaji add
        '            sbSQL.AppendLine("    ,LOC_L_CD")
        '            sbSQL.AppendLine("    ,LOC_M_CD")
        '            sbSQL.AppendLine("    ,LOC_S_CD")
        '            sbSQL.AppendLine("    ,CAT_L_CD")
        '            sbSQL.AppendLine("    ,CAT_M_CD")
        '            sbSQL.AppendLine("    ,CAT_S_CD")
        '            sbSQL.AppendLine("    ,MAKER")
        '            sbSQL.AppendLine("    ,PRODUCT_NO")
        '            sbSQL.AppendLine("    ,MODEL_NO")
        '            sbSQL.AppendLine("    ,LEASE_NO")
        '            sbSQL.AppendLine("    ,LEASE_LIMIT")
        '            sbSQL.AppendLine("    ,GENERIC_LIMIT")
        '            sbSQL.AppendLine("    ,FREE_ITEM1")
        '            sbSQL.AppendLine("    ,FREE_ITEM2")
        '            sbSQL.AppendLine("    ,FREE_ITEM3")
        '            sbSQL.AppendLine("    ,FREE_ITEM4")
        '            sbSQL.AppendLine("    ,FREE_ITEM5")
        '            sbSQL.AppendLine("    ,FREE_ITEM6")
        '            sbSQL.AppendLine("    ,FREE_ITEM7")
        '            sbSQL.AppendLine("    ,FREE_ITEM8")
        '            sbSQL.AppendLine("    ,FREE_ITEM9")
        '            sbSQL.AppendLine("    ,FREE_ITEM10")
        '            '2023/02/08 Kawamoto ADD START
        '            sbSQL.AppendLine("    ,FREE_ITEM11")
        '            sbSQL.AppendLine("    ,FREE_ITEM12")
        '            sbSQL.AppendLine("    ,FREE_ITEM13")
        '            sbSQL.AppendLine("    ,FREE_ITEM14")
        '            sbSQL.AppendLine("    ,FREE_ITEM15")
        '            sbSQL.AppendLine("    ,FREE_ITEM16")
        '            sbSQL.AppendLine("    ,FREE_ITEM17")
        '            sbSQL.AppendLine("    ,FREE_ITEM18")
        '            sbSQL.AppendLine("    ,FREE_ITEM19")
        '            '2023/02/08 Kawamoto ADD END
        '            sbSQL.AppendLine("    ,ASSET_PIC_FILENAME")
        '            sbSQL.AppendLine("    ,LEASE_CNT")
        '            sbSQL.AppendLine("    ,ASSET_STATUS")
        '            sbSQL.AppendLine("    ,EPC_ENC_STATUS")
        '            sbSQL.AppendLine("    ,TAG_LINK_STATUS")
        '            sbSQL.AppendLine("    ,LEASE_STATUS")   '2021/10/05 nv miyaji add
        '            sbSQL.AppendLine("    ,LEASE_PLACE_CD")
        '            sbSQL.AppendLine("    ,LEASE_PLACE_NAME")
        '            sbSQL.AppendLine("    ,REMARK")
        '            sbSQL.AppendLine("    ,LEASE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,LEASE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,LEASE_DT")
        '            sbSQL.AppendLine("    ,LEASE_DUE_DT")
        '            '--2021/10/05 nv miyaji add start
        '            sbSQL.AppendLine("    ,MAINTE_LEASE_NUM ")
        '            sbSQL.AppendLine("    ,MAINTE_LEASE_DAY ")
        '            sbSQL.AppendLine("    ,MAINTE_LEASE_DAY_WORK_FLAG ")
        '            sbSQL.AppendLine("    ,MAINTE_DAY ")
        '            sbSQL.AppendLine("    ,MAINTE_DAY_WORK_FLAG ")
        '            '--2021/10/05 nv miyaji add end
        '            sbSQL.AppendLine("    ,RETIRE_TS")
        '            sbSQL.AppendLine("    ,RETIRE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,RETIRE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,RETIRE_ASSET_ID")
        '            sbSQL.AppendLine("    ,PHYINV_TS")
        '            sbSQL.AppendLine("    ,PHYINV_STATUS")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_L_CD")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_M_CD")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_S_CD")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_L_NAME")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_M_NAME")
        '            sbSQL.AppendLine("    ,PHYINV_LOC_S_NAME")
        '            sbSQL.AppendLine("    ,PHYINV_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,PHYINV_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,TASK_TS")
        '            sbSQL.AppendLine("    ,INS_DATETIME")
        '            sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,UPD_DATETIME")
        '            sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,UPD_TASK")
        '            sbSQL.AppendLine("  )")
        '            sbSQL.AppendLine("SELECT")
        '            sbSQL.AppendLine("     VALT.ASSET_NO")
        '            sbSQL.AppendLine("    ,VALT.ASSET_ID")
        '            sbSQL.AppendLine("    ,VALT.EPC_SIZE")
        '            sbSQL.AppendLine("    ,VALT.GEN_ASSET_ID")
        '            sbSQL.AppendLine("    ,VALT.ASSET_DIV")
        '            sbSQL.AppendLine("    ,VALT.ASSET_NAME")
        '            sbSQL.AppendLine("    ,VALT.MNG_LOC_L_CD")  '2021/10/05 nv miyaji add
        '            sbSQL.AppendLine("    ,VALT.LOC_L_CD")
        '            sbSQL.AppendLine("    ,VALT.LOC_M_CD")
        '            sbSQL.AppendLine("    ,VALT.LOC_S_CD")
        '            sbSQL.AppendLine("    ,VALT.CAT_L_CD")
        '            sbSQL.AppendLine("    ,VALT.CAT_M_CD")
        '            sbSQL.AppendLine("    ,VALT.CAT_S_CD")
        '            sbSQL.AppendLine("    ,VALT.MAKER")
        '            sbSQL.AppendLine("    ,VALT.PRODUCT_NO")
        '            sbSQL.AppendLine("    ,VALT.MODEL_NO")
        '            sbSQL.AppendLine("    ,VALT.LEASE_NO")
        '            sbSQL.AppendLine("    ,VALT.LEASE_LIMIT")
        '            sbSQL.AppendLine("    ,VALT.GENERIC_LIMIT")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM1")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM2")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM3")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM4")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM5")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM6")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM7")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM8")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM9")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM10")
        '            '2023/02/08 Kawamoto ADD START
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM11")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM12")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM13")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM14")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM15")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM16")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM17")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM18")
        '            sbSQL.AppendLine("    ,VALT.FREE_ITEM19")
        '            '2023/02/08 Kawamoto ADD END
        '            sbSQL.AppendLine("    ,VALT.ASSET_PIC_FILENAME")
        '            sbSQL.AppendLine("    ,VALT.LEASE_CNT")
        '            sbSQL.AppendLine("    ,VALT.ASSET_STATUS")
        '            sbSQL.AppendLine("    ,VALT.EPC_ENC_STATUS")
        '            sbSQL.AppendLine("    ,VALT.TAG_LINK_STATUS")
        '            sbSQL.AppendLine("    ,VALT.LEASE_STATUS")  '2021/10/05 nv miyaji add
        '            sbSQL.AppendLine("    ,VALT.LEASE_PLACE_CD")
        '            sbSQL.AppendLine("    ,VALT.LEASE_PLACE_NAME")
        '            sbSQL.AppendLine("    ,VALT.REMARK")
        '            sbSQL.AppendLine("    ,VALT.LEASE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,VALT.LEASE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,VALT.LEASE_DT")
        '            sbSQL.AppendLine("    ,VALT.LEASE_DUE_DT")
        '            '--2021/10/05 nv miyaji add start
        '            sbSQL.AppendLine("    ,VALT.MAINTE_LEASE_NUM")
        '            sbSQL.AppendLine("    ,VALT.MAINTE_LEASE_DAY")
        '            sbSQL.AppendLine("    ,VALT.MAINTE_LEASE_DAY_WORK_FLAG")
        '            sbSQL.AppendLine("    ,VALT.MAINTE_DAY")
        '            sbSQL.AppendLine("    ,VALT.MAINTE_DAY_WORK_FLAG")
        '            '--2021/10/05 nv miyaji add end
        '            sbSQL.AppendLine("    ,VALT.RETIRE_TS")
        '            sbSQL.AppendLine("    ,VALT.RETIRE_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,VALT.RETIRE_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,VALT.RETIRE_ASSET_ID")
        '            sbSQL.AppendLine("    ,VALT.PHYINV_TS")

        '            '--2020/02/12 nv inoue update start  棚卸開始チェック(資産状態 = 1:保管中　で登録する場合のみ)
        '            'sbSQL.AppendLine("    , VALT.PHYINV_STATUS")
        '            sbSQL.AppendLine("    ,CASE WHEN (" & Dao.ToolCommon.GetSqlStrChkInvt() & ") > 0 ")
        '            sbSQL.AppendLine("     　　　 AND VALT.ASSET_STATUS = '" & Misc.ASSET_STATUS.Storage & "' Then '" & Misc.PHYINV_STATUS.NoSpot & "'")
        '            sbSQL.AppendLine("          ELSE VALT.PHYINV_STATUS  End")
        '            '--2020/02/12 nv inoue update end

        '            sbSQL.AppendLine("    ,VALT.PHYINV_LOC_L_CD")
        '            sbSQL.AppendLine("    ,VALT.PHYINV_LOC_M_CD")
        '            sbSQL.AppendLine("    ,VALT.PHYINV_LOC_S_CD")
        '            sbSQL.AppendLine("    ,VALT.PHYINV_LOC_L_NAME")
        '            sbSQL.AppendLine("    ,VALT.PHYINV_LOC_M_NAME")
        '            sbSQL.AppendLine("    ,VALT.PHYINV_LOC_S_NAME")
        '            sbSQL.AppendLine("    ,VALT.PHYINV_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,VALT.PHYINV_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,VALT.TASK_TS")
        '            sbSQL.AppendLine("    ,VALT.INS_DATETIME")
        '            sbSQL.AppendLine("    ,VALT.INS_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,VALT.INS_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,VALT.UPD_DATETIME")
        '            sbSQL.AppendLine("    ,VALT.UPD_EMPLOYEE_CD")
        '            sbSQL.AppendLine("    ,VALT.UPD_EMPLOYEE_NAME")
        '            sbSQL.AppendLine("    ,VALT.UPD_TASK")
        '            sbSQL.AppendLine("FROM " & sTmpTblNm & " VALT")

        '            sbSQL.AppendLine("WHERE NOT EXISTS")
        '            sbSQL.AppendLine("(")
        '            sbSQL.AppendLine("  SELECT 1 FROM M_ASSET")
        '            sbSQL.AppendLine("  WHERE M_ASSET.ASSET_NO = VALT.ASSET_NO")
        '            sbSQL.AppendLine(")")

        '            '2023/02/08 Kawamoto ADD START
        '            '資産CSVファイル取込全件対象が「差分取込」の場合
        '            If CsvRegistALL.DATA1 = AssetCSVRegistAll.Difference Then
        '                sbSQL.AppendLine("AND VALT.ACTIVITY = '" & CsvImportProcDiv.Insert & "'")  '処理区分が「I:追加」
        '            End If
        '            '2023/02/08 Kawamoto ADD END

        '            db.Database.ExecuteSqlCommand(sbSQL.ToString)


        '            '設定マスタ(ロケごとの資産登録上限数)を取得
        '            Dim AssetRegistLimit As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Asset_Registration_Limit.KeyDiv,
        '                                                                             CommonSettingKey.Asset_Registration_Limit.Key1,
        '                                                                             CommonSettingKey.Asset_Registration_Limit.Key2,
        '                                                                             CommonSettingKey.Asset_Registration_Limit.Key3, True)

        '            'ロケごとの資産登録上限数 > 0の場合
        '            If AssetRegistLimit.DATA1 IsNot Nothing AndAlso Integer.Parse(AssetRegistLimit.DATA1) > 0 Then

        '                Dim flg As Boolean = True
        '                '管理ロケ、またはロケ大区分毎の登録件数を取得する
        '                Dim LocLInfo As DataTable = Dao.ToolAsset.GetAssetLocLCodeTotal()
        '                For i As Integer = 0 To LocLInfo.Rows.Count - 1
        '                    If Integer.Parse(LocLInfo(i).Item("CNT")) > Integer.Parse(AssetRegistLimit.DATA1) Then

        '                        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.AssetRegistrationLimit
        '                        Me._lstErr(0).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_EXCEED_MAX_NUM).Replace("{0}", LocLInfo(i).Item("LOC_L_CD")).Replace("{1}", LocLInfo(i).Item("CNT")).Replace("{2}", AssetRegistLimit.DATA1)
        '                        '操作ログ出力
        '                        sProcessContents = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
        '                                           Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
        '                                           Me._lstErr(0).ErrGaiyou

        '                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

        '                        'ロールバック
        '                        tran.Rollback()

        '                        Return Json(Me._lstErr)
        '                    End If
        '                Next
        '            End If
        '            '2023/02/08 Kawamoto ADD END

        '            tran.Commit()

        '            '2023/03/22 Kawamoto ADD START
        '            '添付ファイルフォルダの削除処理
        '            For i As Integer = 0 To DeleteAssetNoList.Count - 1
        '                Try
        '                    Dim sAttachmentPath As String = ConfigurationManager.AppSettings("AttachmentFolderPath")
        '                    Dim sFolderPath As String = Path.Combine(sAttachmentPath, CommonRfid.FncStringToHex(DeleteAssetNoList(i).ASSET_NO))

        '                    '使用可能なパス形式に変換
        '                    sFolderPath = ConvertEnablePath(sFolderPath)

        '                    'フォルダがあれば削除
        '                    If Directory.Exists(sFolderPath) Then
        '                        Directory.Delete(sFolderPath, True)
        '                    End If
        '                Catch ex As Exception
        '                    Continue For
        '                End Try
        '            Next
        '            '2023/03/22 Kawamoto ADD END

        '            ''●●●
        '            'logger.Info("STEP END " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

        '            '操作ログ出力
        '            sProcessContents = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
        '                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
        '                                             Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
        '                                             Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
        '                                            _iCsvImportRowCount.ToString    '2020/03/16 nv miyaji
        '            'Me._dtCsvImport.Rows.Count.ToString    '2020/03/16 nv miyaji
        '            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

        '        Catch ex As Exception
        '            tran.Rollback()

        '            logger.Error(ex.Message)
        '            logger.Error(sbSQL)

        '            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

        '        End Try
        '    End Using

        '    Return Json(Me._lstErr)
        'End Function
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function IchiranCsvExport(ViewModel As AssetListViewModel) As ActionResult
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
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Asset_List.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Asset_List.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Asset_List.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Asset_List.Key3)
            '20230901 ADD End Shirakawa

            'モデルを設定
            _ViewModel = ViewModel

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim AssetData As List(Of AssetListViewModel)

            '資産マスタデータ取得
            AssetData = GetAssetListData()

            '20230901 ADD Start Shirakawa
            '設定マスタから取得できた場合
            '設定マスタから取得できた場合 かつ 上限値(Data2)が存在している場合 かつ 上限を超えていた場合
            If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA2) = False AndAlso WebReadData.DATA2 < AssetData.Count Then
                '上限を超えていた場合　CSV出力しない

                '上限確認フラグ
                ViewBag.isLimitFlg = True
                '一覧データ取得件数
                ViewBag.cntList = AssetData.Count
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
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.AssetList.KeyDiv, CommonSettingKey.AssetList.Key1, CommonSettingKey.AssetList.Key2, CommonSettingKey.AssetList.Key3, True)
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
            '2023/02/08 Kawamoto UPD START
            '設定マスタ（資産CSVファイル取込全件対象）の取得
            Dim CsvRegistAll As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Asset_CSV_Regist_ALL.KeyDiv,
                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key1,
                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key2,
                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key3, True)
            'CSVヘッダー取得
            Dim sHeader As String
            Dim singleQuotation As String = "'"
            '資産CSVファイル取込全件対象が「全件対象」の場合
            If CsvRegistAll.DATA1 = AssetCSVRegistAll.All Then
                'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_ALL)
                sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_ALL)
                '資産CSVファイル取込全件対象が「差分取込」の場合
            Else
                'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_DIFFERENCE)
                sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_DIFFERENCE)
            End If
            '2023/02/08 Kawamoto UPD END

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As AssetListViewModel In AssetData
                col = ""

                '2023/02/08 Kawamoto ADD START
                col += SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                '2023/02/08 Kawamoto ADD END

                '--2019/12/26 miyaji upde start 資産一覧に表示されている全項目を出力する
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.ASSET_NO))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.SUB_ASSET_NO))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.MNG_LOC_L_CD.Trim)) '2021/09/30 nv miyaji add
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MngLocLName) '2021/09/30 nv miyaji add
                Select Case row.ASSET_STATUS
                    Case ASSET_STATUS.Preparation
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_JUNBI))
                    Case ASSET_STATUS.Storage
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_HOKAN))
                    Case ASSET_STATUS.Lending
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_LEASE))
                    Case ASSET_STATUS.Removed
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_RETIRE))
                End Select
                '--2020/03/16 nv miyaji update start
                '--2021/11/11 nv miyaji update start
                Dim sLeaseStatus As String = LeaseStatusAsset_Csv.Kanou
                If row.LEASE_STATUS <> LeaseStatusAsset.Kanou Then
                    sLeaseStatus = LeaseStatusAsset_Csv.Fuka
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sLeaseStatus)
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_STATUS) '2021/09/30 nv miyaji add 
                '--2021/11/11 nv miyaji update end
                If row.LEASE_DT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_DT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                If row.LEASE_DUE_DT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_DUE_DT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_DT)
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_DUE_DT)
                '--2020/03/16 nv miyaji update end
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LEASE_PLACE_CD))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LEASE_EMPLOYEE_CD))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_EMPLOYEE_NAME)
                '--2021/09/30 nv miyaji add start
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_DAY)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_DAY_WORK_FLAG)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_DAY)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_DAY_WORK_FLAG)
                '--2021/09/30 nv miyaji add end
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LOC_L_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocLName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LOC_M_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocMName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LOC_S_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocSName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.CAT_L_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CatLName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.CAT_M_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CatMName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.CAT_S_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CatSName)
                If row.GENERIC_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.GENERIC_LIMIT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.PRODUCT_NO))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_CNT)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_DIV)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LEASE_NO))
                If row.LEASE_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_LIMIT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM8)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM9)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM10)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM11)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM12)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM13)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM14)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM15)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM16)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM17)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM18)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM19)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM20)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM21)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM22)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM23)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM24)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM25)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM26)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM27)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM28)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM29)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM30)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM31)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM32)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM33)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM34)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM35)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM36)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM37)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM38)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM39)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM40)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM41)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM42)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM43)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM44)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM45)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM46)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM47)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM48)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM49)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM50)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM51)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM52)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM53)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM54)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM55)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM56)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM57)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM58)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM59)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM60)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK8)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK9)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK10)

                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.ASSET_ID))
                If row.EPC_ENC_STATUS = EpcEncStatus.Mi Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_MI))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_SUMI))
                End If
                If row.TAG_LINK_STATUS = TagLinkStatus.Mi Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_MI))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_SUMI))
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.INS_DATETIME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.INS_EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.UPD_DATETIME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.UPD_EMPLOYEE_NAME)

                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_PIC_FILENAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.IS_TANGIBLE)
                '--2019/12/26 miyaji upd end 資産一覧に表示されている全項目を出力する

                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                'ファイルをダウンロードさせる
                Call FileDownload(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)

                'ダウンロードさせた後、ファイルを削除。
                System.IO.File.Delete(sFullPath + "\" + sUrlFileName)

            Else
                ''30001,CRT,,,%1に失敗しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     AssetData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetAssetListData() As List(Of AssetListViewModel)
            Dim lstAssetList As List(Of AssetListViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstAssetList = db.AssetListView.SqlQuery(CreateSQL(), _paramSqlList.ToArray).ToList

            'データを返却
            Return lstAssetList
        End Function
#End Region

#Region "全データCSV出力処理"
        ''' <summary>
        ''' 全データCSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ByVal ViewModel As AssetListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)

            Dim AssetData As List(Of AssetListViewModel)

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '資産マスタデータ取得
            AssetData = GetAllAssetData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.AssetList.KeyDiv, CommonSettingKey.AssetList.Key1, CommonSettingKey.AssetList.Key2, CommonSettingKey.AssetList.Key3, True)
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
            '2023/02/08 Kawamoto UPD START
            '設定マスタ（資産CSVファイル取込全件対象）の取得
            Dim CsvRegistAll As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Asset_CSV_Regist_ALL.KeyDiv,
                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key1,
                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key2,
                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key3, True)
            'CSVヘッダー取得
            Dim sHeader As String
            Dim singleQuotation As String = "'"

            '資産CSVファイル取込全件対象が「全件対象」の場合
            If CsvRegistAll.DATA1 = AssetCSVRegistAll.All Then
                'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_ALL)
                sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_ALL)
                '資産CSVファイル取込全件対象が「差分取込」の場合
            Else
                'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_DIFFERENCE)
                sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_DIFFERENCE)
            End If
            '2023/02/08 Kawamoto UPD END

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))

            'データ行挿入
            Dim col As String = ""
            For Each row As AssetListViewModel In AssetData
                col = ""
                '2023/02/08 Kawamoto ADD START
                col += SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                '2023/02/08 Kawamoto ADD END

                '--2019/12/17 miyaji upde start 資産一覧に表示されている全項目を出力する
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.ASSET_NO))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.SUB_ASSET_NO))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.MNG_LOC_L_CD.Trim) '2021/09/30 nv miyaji add
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MngLocLName) '2021/09/30 nv miyaji add
                Select Case row.ASSET_STATUS
                    Case ASSET_STATUS.Preparation
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_JUNBI))
                    Case ASSET_STATUS.Storage
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_HOKAN))
                    Case ASSET_STATUS.Lending
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_LEASE))
                    Case ASSET_STATUS.Removed
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_RETIRE))
                End Select
                '--2021/11/11 nv miyaji update start
                Dim sLeaseStatus As String = SVClsCSVExp.SVFncCSVAddDquot(LeaseStatusAsset_Csv.Kanou)   'UPDATE 2021/12/06 kawamoto
                If row.LEASE_STATUS <> LeaseStatusAsset.Kanou Then
                    sLeaseStatus = SVClsCSVExp.SVFncCSVAddDquot(LeaseStatusAsset_Csv.Fuka)
                End If
                col += "," & sLeaseStatus
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_STATUS) '2021/09/30 nv miyaji add 
                '--2021/11/11 nv miyaji update end
                '--2020/03/16 nv miyaji update start
                If row.LEASE_DT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_DT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                If row.LEASE_DUE_DT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_DUE_DT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_DT)
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_DUE_DT)
                '--2020/03/16 nv miyaji update end
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LEASE_PLACE_CD))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LEASE_EMPLOYEE_CD))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_EMPLOYEE_NAME)
                '--2021/09/30 nv miyaji add start
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_DAY)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_DAY_WORK_FLAG)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_DAY)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_DAY_WORK_FLAG)
                '--2021/09/30 nv miyaji add end
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LOC_L_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocLName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LOC_M_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocMName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.LOC_S_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocSName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.CAT_L_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CatLName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.CAT_M_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CatMName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.CAT_S_CD).Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CatSName)
                If row.GENERIC_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.GENERIC_LIMIT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.PRODUCT_NO))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_CNT)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_DIV)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_NO)
                If row.LEASE_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_LIMIT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM8)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM9)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM10)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM11)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM12)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM13)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM14)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM15)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM16)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM17)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM18)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM19)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM20)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM21)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM22)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM23)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM24)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM25)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM26)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM27)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM28)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM29)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM30)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM31)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM32)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM33)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM34)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM35)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM36)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM37)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM38)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM39)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM40)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM41)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM42)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM43)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM44)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM45)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM46)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM47)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM48)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM49)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM50)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM51)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM52)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM53)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM54)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM55)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM56)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM57)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM58)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM59)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM60)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK8)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK9)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK10)

                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + CommonRfid.ToStringNvl(row.ASSET_ID))
                If row.EPC_ENC_STATUS = EpcEncStatus.Mi Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_MI))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_SUMI))
                End If
                If row.TAG_LINK_STATUS = TagLinkStatus.Mi Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_MI))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_SUMI))
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.INS_DATETIME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.INS_EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.UPD_DATETIME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.UPD_EMPLOYEE_NAME)

                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_PIC_FILENAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.IS_TANGIBLE)
                '--2019/12/17 miyaji upd end 資産一覧に表示されている全項目を出力する

                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                'ファイルをダウンロードさせる
                Call FileDownload(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)

                'ダウンロードさせた後、ファイルを削除。
                System.IO.File.Delete(sFullPath + "\" + sUrlFileName)

            Else
                ''30001,CRT,,,%1に失敗しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     AssetData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "全データCSV出力用のデータを取得"
        ''' <summary>
        ''' 全データCSV出力用のデータを取得します
        ''' </summary>
        Private Function GetAllAssetData() As List(Of AssetListViewModel)
            Dim sbSQL As StringBuilder = New StringBuilder

            'CSV出力用SQL
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  ASSET.ASSET_NO ")
            sbSQL.AppendLine("  , ASSET.SUB_ASSET_NO ")
            sbSQL.AppendLine("  , ASSET.ASSET_ID ")
            sbSQL.AppendLine("  , ASSET.EPC_SIZE ")
            sbSQL.AppendLine("  , ASSET.GEN_ASSET_ID ")
            sbSQL.AppendLine("  , ASSET.ASSET_DIV ")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("  , ASSET.MNG_LOC_L_CD ")   '2021/09/30 nv miyaji
            sbSQL.AppendLine("  , ASSET.LOC_L_CD ")
            sbSQL.AppendLine("  , ASSET.LOC_M_CD ")
            sbSQL.AppendLine("  , ASSET.LOC_S_CD ")
            sbSQL.AppendLine("  , ASSET.CAT_L_CD ")
            sbSQL.AppendLine("  , ASSET.CAT_M_CD ")
            sbSQL.AppendLine("  , ASSET.CAT_S_CD ")
            sbSQL.AppendLine("  , ASSET.MAKER ")
            sbSQL.AppendLine("  , ASSET.PRODUCT_NO ")
            sbSQL.AppendLine("  , ASSET.MODEL_NO ")
            sbSQL.AppendLine("  , ASSET.LEASE_NO ")
            sbSQL.AppendLine("  , ASSET.LEASE_LIMIT ")
            sbSQL.AppendLine("  , ASSET.GENERIC_LIMIT ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM1 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM2 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM3 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM4 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM5 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM6 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM7 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM8 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM9 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM10 ")
            '2023/02/08 Kawamoto ADD START
            sbSQL.AppendLine("  , ASSET.FREE_ITEM11 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM12 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM13 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM14 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM15 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM16 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM17 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM18 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM19 ")
            '2023/02/08 Kawamoto ADD END
            sbSQL.AppendLine("  , ASSET.FREE_ITEM20 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM21 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM22 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM23 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM24 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM25 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM26 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM27 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM28 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM29 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM30 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM31 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM32 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM33 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM34 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM35 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM36 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM37 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM38 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM39 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM40 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM41 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM42 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM43 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM44 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM45 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM46 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM47 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM48 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM49 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM50 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM51 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM52 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM53 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM54 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM55 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM56 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM57 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM58 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM59 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM60 ")
            sbSQL.AppendLine("  , ASSET.ASSET_PIC_FILENAME ")
            sbSQL.AppendLine("  , ASSET.LEASE_CNT ")
            sbSQL.AppendLine("  , ASSET.ASSET_STATUS ")
            sbSQL.AppendLine("  , ASSET.EPC_ENC_STATUS ")
            sbSQL.AppendLine("  , ASSET.TAG_LINK_STATUS ")
            sbSQL.AppendLine("  , ASSET.LEASE_STATUS ")   '2021/09/30 nv miyaji
            sbSQL.AppendLine("  , ASSET.LEASE_PLACE_CD ")
            sbSQL.AppendLine("  , ASSET.LEASE_PLACE_NAME ")
            sbSQL.AppendLine("  , ASSET.REMARK ")
            sbSQL.AppendLine("  , ASSET.LEASE_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.LEASE_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.LEASE_DT ")
            sbSQL.AppendLine("  , ASSET.LEASE_DUE_DT ")
            '--2021/09/16 nv miyaji add start
            sbSQL.AppendLine("  , ASSET.MAINTE_LEASE_NUM ")
            sbSQL.AppendLine("  , ASSET.MAINTE_LEASE_DAY ")
            sbSQL.AppendLine("  , ASSET.MAINTE_LEASE_DAY_WORK_FLAG ")
            sbSQL.AppendLine("  , ASSET.MAINTE_DAY ")
            sbSQL.AppendLine("  , ASSET.MAINTE_DAY_WORK_FLAG ")
            '--2021/09/16 nv miyaji add end
            sbSQL.AppendLine("  , ASSET.RETIRE_TS ")
            sbSQL.AppendLine("  , ASSET.RETIRE_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.RETIRE_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.RETIRE_ASSET_ID ")
            sbSQL.AppendLine("  , ASSET.PHYINV_TS ")
            sbSQL.AppendLine("  , ASSET.PHYINV_STATUS ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_L_CD ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_M_CD ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_S_CD ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_L_NAME ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_M_NAME ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_S_NAME ")
            sbSQL.AppendLine("  , ASSET.PHYINV_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.PHYINV_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.TASK_TS ")
            sbSQL.AppendLine("  , ASSET.IS_TANGIBLE ")
            sbSQL.AppendLine("  , ASSET.REMARK1 ")
            sbSQL.AppendLine("  , ASSET.REMARK2 ")
            sbSQL.AppendLine("  , ASSET.REMARK3 ")
            sbSQL.AppendLine("  , ASSET.REMARK4 ")
            sbSQL.AppendLine("  , ASSET.REMARK5 ")
            sbSQL.AppendLine("  , ASSET.REMARK6 ")
            sbSQL.AppendLine("  , ASSET.REMARK7 ")
            sbSQL.AppendLine("  , ASSET.REMARK8 ")
            sbSQL.AppendLine("  , ASSET.REMARK9 ")
            sbSQL.AppendLine("  , ASSET.REMARK10 ")
            sbSQL.AppendLine("  , MNGEMP.EMPLOYEE_NAME AS FREE_ITEM6_NAME ")
            sbSQL.AppendLine("  , ASSET.INS_DATETIME ")
            sbSQL.AppendLine("  , ASSET.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.UPD_DATETIME ")
            sbSQL.AppendLine("  , ASSET.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.UPD_TASK ")
            sbSQL.AppendLine("  , LOCL.LOC_NAME AS LocLName ")
            sbSQL.AppendLine("  , LOCM.LOC_NAME AS LocMName ")
            sbSQL.AppendLine("  , LOCS.LOC_NAME AS LocSName ")
            sbSQL.AppendLine("  , CATL.CAT_NAME AS CatLName ")
            sbSQL.AppendLine("  , CATM.CAT_NAME AS CatMName ")
            sbSQL.AppendLine("  , CATS.CAT_NAME AS CatSName ")
            sbSQL.AppendLine("  , ULOC.UPD_FLG AS UPD_FLG ") '2021/10/01 nv miyaji
            sbSQL.AppendLine("  , MNGLOCL.LOC_NAME AS MngLocLName ")
            sbSQL.AppendLine("  , 0 AS LEASE_LIMIT_BEF_DAYS ")  '使わないので0を設定
            sbSQL.AppendLine("  , 0 AS GENERIC_LIMIT_BEF_DAYS ") '使わないので0を設定
            sbSQL.AppendLine("  , 0 AS LEASE_ALERT_CNT ") '使わないので0を設定
            sbSQL.AppendLine("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  M_ASSET ASSET ")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM ")
            sbSQL.AppendLine("ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCM.LOC_L_CD")
            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS ")
            sbSQL.AppendLine("ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCS.LOC_L_CD")
            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCS.LOC_M_CD")
            sbSQL.AppendLine("AND ASSET.LOC_S_CD = LOCS.LOC_S_CD")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATL ")
            sbSQL.AppendLine("ON CATL.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATL.CAT_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATM ")
            sbSQL.AppendLine("ON CATM.CAT_DIV = '" + CAT_DIV.CategoryM + "'")
            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATM.CAT_L_CD")
            sbSQL.AppendLine("AND ASSET.CAT_M_CD = CATM.CAT_M_CD")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATS ")
            sbSQL.AppendLine("ON CATS.CAT_DIV = '" + CAT_DIV.CategoryS + "'")
            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATS.CAT_L_CD")
            sbSQL.AppendLine("AND ASSET.CAT_M_CD = CATS.CAT_M_CD")
            sbSQL.AppendLine("AND ASSET.CAT_S_CD = CATS.CAT_S_CD")
            sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE MNGEMP ")
            sbSQL.AppendLine("ON ASSET.FREE_ITEM6 = MNGEMP.EMPLOYEE_CD")
            '--2021/09/17 nv miyaji add start
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL ")
            sbSQL.AppendLine("ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")
            '--2021/09/17 nv miyaji add end
            '--2021/10/01 nv miyaji add start
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC")
            sbSQL.AppendLine("ON ASSET.MNG_LOC_L_CD = ULOC.LOC_L_CD ")
            sbSQL.AppendLine("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = ASSET.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{_AuthInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{_AuthInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{_AuthInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{_AuthInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthOption", $"{_AuthInfo.AuthOption}")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  ASSET.RETIRE_TS IS NULL")
            '表示順 SearchDispSelect　'2019/10/03 nv miyaji
            If Not IsNothing(_ViewModel.SearchDispSelect) Then
                Select Case _ViewModel.SearchDispSelect
                    Case AssetDispSelect.Loca
                        sbSQL.AppendLine(" ORDER BY ASSET.LOC_L_CD,")
                        sbSQL.AppendLine("          ASSET.LOC_M_CD,")
                        sbSQL.AppendLine("          ASSET.LOC_S_CD,")
                        sbSQL.AppendLine("          ASSET_NO")
                    Case AssetDispSelect.Cat
                        sbSQL.AppendLine(" ORDER BY ASSET.CAT_L_CD,")
                        sbSQL.AppendLine("          ASSET.CAT_M_CD,")
                        sbSQL.AppendLine("          ASSET.CAT_S_CD,")
                        sbSQL.AppendLine("          ASSET_NO")
                    Case AssetDispSelect.Asset
                        sbSQL.AppendLine(" ORDER BY ASSET_NO")
                    ' 2023/02/06 Kawamoto ADD START
                    Case AssetDispSelect.Rate
                        sbSQL.AppendLine(" ORDER BY FREE_ITEM17")
                        ' 2023/02/06 Kawamoto ADD END
                    Case AssetDispSelect.AssetNoAsc
                        sbSQL.AppendLine(" ORDER BY ASSET_NO")
                    Case Else
                        sbSQL.AppendLine(" ORDER BY " & GetSortColumn(_ViewModel.SearchDispSelect) & ",")
                        sbSQL.AppendLine("          ASSET_NO")
                End Select
            End If

            Dim lstAsset As List(Of AssetListViewModel)   '資産リスト
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstAsset = db.AssetListView.SqlQuery(sbSQL.ToString, _paramSqlList.ToArray).ToList

            'データを返却
            Return lstAsset
        End Function
#End Region

#Region "タグ発行用CSV出力処理"
        ''' <summary>
        ''' タグ発行用CSV出力処理
        ''' </summary>
        ''' <param name="SelectKeys">画面で選択された項目のキー値(カンマ区切り)</param>
        ''' <param name="PrinterKey">選択したラベルプリンターのKEY1</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExportForLabel(ViewModel As AssetListViewModel, SelectKeys As String, PrinterKey As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            Dim AssetListData As List(Of AssetListLabelModel)

            '最新のチェック状態でセッション情報を更新する(追加および削除) (キー：資産番号, 資産ID, カテゴリーL)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            'マスタデータ取得
            'AssetListData = GetSelectAssetData(SelectKeys)
            'AssetListData = GetSelectAssetData(String.Join(",", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetListKey.ASSET_NO))))
            AssetListData = GetSelectAssetData("'" & String.Join("','", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetListKey.ASSET_NO))) & "'")    '2020/01/30 そのままSQLで使用するため ' で囲む UPDATE Inoue

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ラベルプリンター情報の取得
            Dim labelPrinter As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LabelPrinter.KeyDiv, PrinterKey, CommonSettingKey.LabelPrinter.Key2, CommonSettingKey.LabelPrinter.Key3, True)

            '出力ファイル名取得
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.AssetLabel.KeyDiv, CommonSettingKey.AssetLabel.Key1, CommonSettingKey.AssetLabel.Key2, CommonSettingKey.AssetLabel.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

            '◆CSVファイル出力先フォルダ存在チェック
            'Dim sFullPath As String = Server.MapPath(CommonConst.ROOT_FOLDERNM & labelPrinter.DATA3)
            'Dim sFullPath As String = Server.MapPath(labelPrinter.DATA3)
            Dim sFullPath As String = System.IO.Path.GetFullPath(labelPrinter.DATA3)

            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆CSVファイル出力先設定
            Dim sUrlFileName As String = Path.GetRandomFileName().Substring(0, 8)
            Dim sPrnFileName As String = String.Format(sFileNm & sUrlFileName & "{0}.csv", Now.ToString("HHmmss"))
            SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sPrnFileName)

            '◆CSV出力用リスト作成
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'データ行挿入
            Dim col As String = ""
            For Each row As AssetListLabelModel In AssetListData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(LabelType.AS)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(labelPrinter.DATA1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(labelPrinter.DATA2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_ID)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.EPC_SIZE)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.GEN_ASSET_ID)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_DIV)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PRODUCT_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_NO)
                If row.LEASE_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_LIMIT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                If row.GENERIC_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.GENERIC_LIMIT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)LEASE_LIMIT
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.GENERIC_LIMIT)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM8)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM9)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM10)
                '2023/02/08 Kawamoto ADD START
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM11)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM12)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM13)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM14)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM15)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM16)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM17)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM18)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM19)
                '2023/02/08 Kawamoto END START
                col += "," & SVClsCSVExp.SVFncCSVAddDquot("1")                          '発行枚数の1 を追加  Inoue 2019/09/03
                '--2021/11/11 nv miyaji add start
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_NAME)
                '--2021/11/11 nv miyaji add end

                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                Dim labelPrintMethodSelect As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LabelPrntMethodSelect.KeyDiv, CommonSettingKey.LabelPrntMethodSelect.Key1, CommonSettingKey.LabelPrntMethodSelect.Key2, CommonSettingKey.LabelPrntMethodSelect.Key3, False)

                If labelPrintMethodSelect IsNot Nothing AndAlso labelPrintMethodSelect.DATA1 = "Y" Then
                    'ファイルをダウンロードさせる
                    Call FileDownload(sPrnFileName, "text/comma-separated-values", sFullPath + "\" + sPrnFileName)
                    'ダウンロードさせた後、ファイルを削除。
                    System.IO.File.Delete(sFullPath + "\" + sPrnFileName)
                End If

                'ファイル更新
                Dao.ToolAsset.UpdateAssetStat(AssetListData, _LoginInfo)

                _ViewModel.States = StatusType.SuccessHakko

                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_ASSET_LIST) = _ViewModel

                ''10025,INF,,,%1が完了しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 10025, "CSVファイルの出力")
            Else
                ''30001,CRT,,,%1に失敗しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_PRINT_TAG) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     AssetListData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function
#End Region

#Region "タグ発行用CSV出力のデータを取得"
        ''' <summary>
        ''' タグ発行用CSV出力のデータを取得します
        ''' </summary>
        Private Function GetSelectAssetData(SelectKeys As String) As List(Of AssetListLabelModel)
            'Dim lstAsset As New List(Of AssetListLabelModel)
            Dim db As SystemDBContext = New SystemDBContext

            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  ASSET.ASSET_NO ")
            sbSQL.AppendLine("  , ASSET.ASSET_ID ")
            sbSQL.AppendLine("  , ASSET.EPC_SIZE ")
            sbSQL.AppendLine("  , ASSET.GEN_ASSET_ID ")
            sbSQL.AppendLine("  , ASSET.ASSET_DIV ")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("  , IsNull(ASSET.LOC_L_CD, '') AS LOC_L_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.LOC_M_CD, '') AS LOC_M_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.LOC_S_CD, '') AS LOC_S_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.CAT_L_CD, '') AS CAT_L_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.CAT_M_CD, '') AS CAT_M_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.CAT_S_CD, '') AS CAT_S_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.MNG_LOC_L_CD, '') AS MNG_LOC_L_CD")  '2021/11/11 nv miyaji add
            sbSQL.AppendLine("  , ASSET.MAKER ")
            sbSQL.AppendLine("  , ASSET.PRODUCT_NO ")
            sbSQL.AppendLine("  , ASSET.MODEL_NO ")
            sbSQL.AppendLine("  , ASSET.LEASE_NO ")
            sbSQL.AppendLine("  , ASSET.LEASE_LIMIT ")
            sbSQL.AppendLine("  , ASSET.GENERIC_LIMIT ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM1 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM2 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM3 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM4 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM5 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM6 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM7 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM8 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM9 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM10 ")
            '2023/02/08 Kawamoto ADD START
            sbSQL.AppendLine("  , ASSET.FREE_ITEM11 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM12 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM13 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM14 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM15 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM16 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM17 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM18 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM19 ")
            '2023/02/08 Kawamoto ADD END
            sbSQL.AppendLine("  , ASSET.ASSET_STATUS ")
            sbSQL.AppendLine("  , ASSET.TAG_LINK_STATUS ")
            sbSQL.AppendLine("  , LOCL.LOC_NAME AS LOC_L_NAME ")
            sbSQL.AppendLine("  , LOCM.LOC_NAME AS LOC_M_NAME ")
            sbSQL.AppendLine("  , LOCS.LOC_NAME AS LOC_S_NAME ")
            sbSQL.AppendLine("  , CATL.CAT_NAME AS CAT_L_NAME ")
            sbSQL.AppendLine("  , CATM.CAT_NAME AS CAT_M_NAME ")
            sbSQL.AppendLine("  , CATS.CAT_NAME AS CAT_S_NAME ")
            sbSQL.AppendLine("  , MNG_LOCL.LOC_NAME AS MNG_LOC_L_NAME ")    '2021/11/11 nv miyaji add
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  M_ASSET ASSET ")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM ")
            sbSQL.AppendLine("ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCM.LOC_L_CD")
            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS ")
            sbSQL.AppendLine("ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCS.LOC_L_CD")
            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCS.LOC_M_CD")
            sbSQL.AppendLine("AND ASSET.LOC_S_CD = LOCS.LOC_S_CD")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATL ")
            sbSQL.AppendLine("ON CATL.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATL.CAT_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATM ")
            sbSQL.AppendLine("ON CATM.CAT_DIV = '" + CAT_DIV.CategoryM + "'")
            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATM.CAT_L_CD")
            sbSQL.AppendLine("AND ASSET.CAT_M_CD = CATM.CAT_M_CD")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATS ")
            sbSQL.AppendLine("ON CATS.CAT_DIV = '" + CAT_DIV.CategoryS + "'")
            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATS.CAT_L_CD")
            sbSQL.AppendLine("AND ASSET.CAT_M_CD = CATS.CAT_M_CD")
            sbSQL.AppendLine("AND ASSET.CAT_S_CD = CATS.CAT_S_CD")
            '--2021/11/11 nv miyaji add start
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNG_LOCL ")
            sbSQL.AppendLine("ON MNG_LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.MNG_LOC_L_CD = MNG_LOCL.LOC_L_CD")
            '--2021/11/11 nv miyaji add end
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  ASSET.ASSET_NO in (" & SelectKeys & ") ")
            sbSQL.AppendLine(" ORDER BY ASSET_NO")

            'SQLを実行する
            Dim lst As List(Of AssetListLabelModel)
            lst = db.AssetListLabel.SqlQuery(sbSQL.ToString).ToList

            'データを返却
            Return lst

        End Function
#End Region

#Region "QR台帳出力処理"
        ''' <summary>
        ''' QR台帳出力処理
        ''' </summary>
        ''' <param name="ViewModel">ビューモデル</param>
        ''' <param name="SelectKeys">画面で選択された項目のキー値(カンマ区切り)</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function QRDaicho(ViewModel As AssetListViewModel, SelectKeys As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '最新のチェック状態でセッション情報を更新する(追加および削除) (キー：資産番号, 資産ID, カテゴリーL)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)


            '資産マスタデータ取得
            Dim AssetData As List(Of AssetListLabelModel)
            'AssetData = GetSelectAssetData(SelectKeys) 
            'AssetData = GetSelectAssetData(String.Join(",", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetListKey.ASSET_NO))))
            AssetData = GetSelectAssetData("'" & String.Join("','", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetListKey.ASSET_NO))) & "'")    '2019/12/19 そのままSQLで使用するため ' で囲む UPDATE Inoue


            'Dim AssetData As List(Of AssetListViewModel)
            'AssetData = GetAssetListData()

            'ライセンスキー
            GrapeCity.Documents.Excel.Workbook.SetLicenseKey("株式会社シービジョン,496378668853486#A03NJgzNzYTO4IiOiQWSisnOiQkIsISP3cWe6QTZ95WWvEmMYdTStZ5MltGdv26dWlWazcmYBREamhXNyUEVRdGbOl6SuREarg4L6dVRwZnUz4EZFZzKzYmZv2Ee7gje9tWb5IzZ6x6arQGUVl5UiojITJCL7YzM8cTM8gDO0IicfJye#4Xfd5nI9g4NyIiOiMkIsIiMWBCdl9kLgwWZjhXRgI7bmBCduVWb5N6bEByQHJiOi8kI1tlOiQmcQJCLiEDNzIDNwAiNwITM9EDMyIiOiQncDJCLiM1gjf0gjjrgjP9gjz1gjfrgj1LpnrJvk/IvlrKomLiOiEmTDJCLiYDO4M5Ntg")

            'ファイル出力
            Dim WB As IWorkbook = New Workbook
            Dim WS As IWorksheet = Nothing

            '2023/10/17 Sato v2.5 UPD START
            '設定Mの設定値に従って出力するQR台帳の種別を分岐する
            Dim qrLedgerTypeSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.QR_Ledger_Type.KeyDiv, CommonSettingKey.QR_Ledger_Type.Key1, CommonSettingKey.QR_Ledger_Type.Key2, CommonSettingKey.QR_Ledger_Type.Key3, False)

            If IsNothing(qrLedgerTypeSetting) Then
                '設定Mに設定が存在しない場合は標準テンプレートで出力
                Call CreateQRDaichoStandardXlsx(WB, WS, AssetData)
            Else
                '設定Mに設定が存在する
                Select Case qrLedgerTypeSetting.DATA1
                    Case QrLedgerType.Cunstom1
                        'カスタム1
                        Call CreateQRDaichoCustom1Xlsx(WB, WS, AssetData)

                    Case Else
                        '標準
                        Call CreateQRDaichoStandardXlsx(WB, WS, AssetData)
                End Select
            End If
            '2023/10/17 Sato v2.5 UPD END

            'エクセルファイル出力
            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'ファイル出力先
            Dim sOutputFileName As String = Path.GetRandomFileName()
            Dim oFile As String = sFullPath & "\" & sOutputFileName & ".xlsx"                             '出力Excelファイル名
            'wFileStream = New FileStream(oFile, FileMode.Create, FileAccess.Write)
            '.xlsxドキュメントをエクスポートします
            WB.Save(oFile, SaveFileFormat.Xlsx)
            'WB.Write(wFileStream)

            'エクセルファイル出力名取得
            Dim setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.AssetQRList.KeyDiv, CommonSettingKey.AssetQRList.Key1, CommonSettingKey.AssetQRList.Key2, CommonSettingKey.AssetQRList.Key3, True)
            Dim sDspFileName As String = String.Format(setting.DATA1 + "_{0}.pdf", Now.ToString("yyyyMMdd"))


            ' 新しいワークブックを生成
            Dim workbook = New Workbook()
            ' Excelファイルを開く
            Dim fileStream As New IO.FileStream(oFile, IO.FileMode.Open, IO.FileAccess.ReadWrite)
            workbook.Open(fileStream)
            ' PDFファイルに保存
            Dim newFn As String = sFullPath & "\" & sOutputFileName & ".pdf"                                  '出力PDFファイル名
            workbook.Save(newFn, SaveFileFormat.Pdf)
            fileStream.Close()
            System.IO.File.Delete(oFile)

            ''PDF出力準備
            'Dim rtn As Boolean = False
            'Dim fn As String = oFile
            'Dim xlApp As Object
            'xlApp = CreateObject("Excel.Application")
            'xlApp.DisplayAlerts = False
            'Dim xlBooks As Object = xlApp.Workbooks
            'Dim xlBook As Object = xlBooks.Open(fn)
            'Dim xlSheets As Object = xlBook.Sheets
            'Dim newFn As String = sFullPath & "\" & sOutputFileName & ".pdf"                                  '出力PDFファイル名


            ''PDF出力処理
            'xlBook.ExportAsFixedFormat(Type:=0,
            '                   Filename:=newFn,
            '                   Quality:=0,
            '                   IncludeDocProperties:=True,
            '                   IgnorePrintAreas:=False,
            '                   OpenAfterPublish:=False)
            'xlBook.Close()


            '宣言と逆にして開放する。
            WS = Nothing
            WB = Nothing
            'xlSheets = Nothing
            'xlBook = Nothing
            'xlBooks = Nothing
            'xlApp = Nothing

            ''ファイルを削除。
            'System.IO.File.Delete(oFile)

            'ファイルをダウンロードさせる
            Call FileDownload(sDspFileName, "application/pdf", newFn)
            'Call FileDownload(sDspFileName, "text/comma-separated-values", oFile)

            'ダウンロードさせた後、ファイルを削除。
            System.IO.File.Delete(newFn)

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_PRINT_QR_DAICHO) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     AssetData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))



            '結果表示
            Return New EmptyResult()
            'Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})


        End Function
#End Region

#Region "QR台帳(標準)用のExcelファイル作成"
        ''' <summary>
        ''' QR台帳(標準)用のExcelファイル作成   2023/10/17 ADD Sato v2.5
        ''' </summary>
        ''' <param name="WB">ブック</param>
        ''' <param name="WS">シート</param>
        ''' <param name="AssetData">資産マスタデータ</param>
        Private Sub CreateQRDaichoStandardXlsx(ByRef WB As IWorkbook, ByRef WS As IWorksheet, ByRef AssetData As List(Of AssetListLabelModel))

            'ブックの設定
            Dim sTempFullPath As String = Server.MapPath(CommonConst.TEMPLATE_FOLDERNM)
            WB.Open(sTempFullPath & "\" & CommonConst.DAICHO_TEMPLATE_FILENM) '読み込むテンプレートファイル
            'WB = WorkbookFactory.Create(sTempFullPath & "\" & CommonConst.DAICHO_TEMPLATE_FILENM)  '読み込むテンプレートファイル

            'テンプレートシート取得する
            Dim WSTemp As IWorksheet = WB.Worksheets(0)
            'Dim WSTemp As ISheet = WB.GetSheetAt(0)
            'テンプレートシートを次のシートにコピーする
            WS = WSTemp.Copy()

            '画像ファイル貼り付け作業用
            Dim sQrFileName As String = String.Empty
            Dim sCallPath As String = String.Empty
            Dim ret = Nothing

            'エクセルデータ作成
            Dim iRowCount As Integer = 0    'ページの縦何個目のラベルか
            Dim iQRCount As Integer = 1
            For iRowCnt As Integer = 0 To AssetData.Count - 1

                'QR画像ファイルを作成
                'QRコードを作成する
                sQrFileName = Now.ToString("yyyyMMdd_") & Path.GetRandomFileName() & ".png"
                sCallPath = System.IO.Path.GetFullPath(HttpContext.Server.MapPath(Misc.CommonConst.QRIMAGE_FOLDERNM) & "\\" & sQrFileName)
                ret = Dao.MakeQr.GetQRData(AssetData(iRowCnt).ASSET_NO, sCallPath)
                If ret = 0 Then
                    Dim QRInfo As QRImageInfo = QRImageInfo.GetQRPosition(WS, WS.Range($"QR_IMAGE{iQRCount}"))

                    'ピクチャを追加します
                    Using data As Stream = IO.File.OpenRead(sCallPath)    '読み込む画像ファイル
                        Dim picture1 As IShape = WS.Shapes.AddPictureInPixel(data, ImageType.PNG, QRInfo.Left, QRInfo.Top, QRInfo.Width, QRInfo.Height)
                    End Using

                    'ファイルを削除。
                    System.IO.File.Delete(sCallPath)
                End If

                If iQRCount = 16 Then
                    '16個目のQR印字後にカウントを初期化
                    iQRCount = 1
                Else
                    iQRCount += 1
                End If

                Dim str As String = String.Empty
                If (iRowCnt + 1) Mod 2 = 0 Then
                    WS.Rows(LabelQRDaicho.Row.AssetNo + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic + LabelQRDaicho.ColCount).Value = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) + " " + AssetData(iRowCnt).ASSET_NO
                    str = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) + " " + AssetData(iRowCnt).ASSET_NAME
                    '2020/01/23 nv miyaji 指定桁数でカットしないようにする
                    'If str.Length > LabelQRDaicho.StrLength Then
                    '    str = str.Substring(0, LabelQRDaicho.StrLength)
                    'End If
                    WS.Rows(LabelQRDaicho.Row.AssetNm + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic + LabelQRDaicho.ColCount).Value = str
                    str = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) + " " + AssetData(iRowCnt).LOC_L_CD
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).LOC_M_CD.Trim) Then
                        str = str & "-" + AssetData(iRowCnt).LOC_M_CD
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).LOC_S_CD.Trim) Then
                        str = str & "-" + AssetData(iRowCnt).LOC_S_CD
                    End If
                    WS.Rows(LabelQRDaicho.Row.LocaCd + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic + LabelQRDaicho.ColCount).Value = str
                    str = String.Empty
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).LOC_L_NAME) Then
                        str = "　" + AssetData(iRowCnt).LOC_L_NAME.Trim
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).LOC_M_NAME) Then
                        str = str & " " + AssetData(iRowCnt).LOC_M_NAME.Trim
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).LOC_S_NAME) Then
                        str = str & " " + AssetData(iRowCnt).LOC_S_NAME.Trim
                    End If
                    '2020/01/23 nv miyaji 指定桁数でカットしないようにする
                    'If str.Length > LabelQRDaicho.StrLength Then
                    '    str = str.Substring(0, LabelQRDaicho.StrLength)
                    'End If
                    WS.Rows(LabelQRDaicho.Row.LocaNm + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic + LabelQRDaicho.ColCount).Value = str
                    str = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) + " " + AssetData(iRowCnt).CAT_L_CD
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).CAT_M_CD.Trim) Then
                        str = str & "-" + AssetData(iRowCnt).CAT_M_CD.Trim
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).CAT_S_CD.Trim) Then
                        str = str & "-" + AssetData(iRowCnt).CAT_S_CD.Trim
                    End If
                    '2020/01/23 nv miyaji 指定桁数でカットしないようにする
                    'If str.Length > LabelQRDaicho.StrLength Then
                    '    str = str.Substring(0, LabelQRDaicho.StrLength)
                    'End If
                    WS.Rows(LabelQRDaicho.Row.CatCd + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic + LabelQRDaicho.ColCount).Value = str
                    str = String.Empty
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).CAT_L_NAME) Then
                        str = "　" + AssetData(iRowCnt).CAT_L_NAME.Trim
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).CAT_M_NAME) Then
                        str = str & " " + AssetData(iRowCnt).CAT_M_NAME.Trim
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).CAT_S_NAME) Then
                        str = str & " " + AssetData(iRowCnt).CAT_S_NAME.Trim
                    End If
                    '2020/01/23 nv miyaji 指定桁数でカットしないようにする
                    'If str.Length > LabelQRDaicho.StrLength Then
                    '    str = str.Substring(0, LabelQRDaicho.StrLength)
                    'End If
                    WS.Rows(LabelQRDaicho.Row.CatNm + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic + LabelQRDaicho.ColCount).Value = str

                    If iRowCount = LabelQRDaicho.ColLabelSuu - 1 AndAlso iRowCnt < AssetData.Count - 1 Then
                        'テンプレートシートを次のシートにコピーする
                        WS = WSTemp.CopyAfter(WS)
                        'WS = WSTemp.CopySheet(1 + (iRowCnt + 1) / LabelQRDaicho.PageLabelSuu)
                        iRowCount = 0
                    Else
                        iRowCount += 1
                    End If
                Else
                    WS.Rows(LabelQRDaicho.Row.AssetNo + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic).Value = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) + " " + AssetData(iRowCnt).ASSET_NO
                    str = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) + " " + AssetData(iRowCnt).ASSET_NAME
                    '2020/01/23 nv miyaji 指定桁数でカットしないようにする
                    'If str.Length > LabelQRDaicho.StrLength Then
                    '    str = str.Substring(0, LabelQRDaicho.StrLength)
                    'End If
                    WS.Rows(LabelQRDaicho.Row.AssetNm + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic).Value = str
                    str = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) + " " + AssetData(iRowCnt).LOC_L_CD
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).LOC_M_CD.Trim) Then
                        str = str & "-" + AssetData(iRowCnt).LOC_M_CD
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).LOC_S_CD.Trim) Then
                        str = str & "-" + AssetData(iRowCnt).LOC_S_CD
                    End If
                    WS.Rows(LabelQRDaicho.Row.LocaCd + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic).Value = str
                    str = String.Empty
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).LOC_L_NAME) Then
                        str = "　" + AssetData(iRowCnt).LOC_L_NAME.Trim
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).LOC_M_NAME) Then
                        str = str & " " + AssetData(iRowCnt).LOC_M_NAME.Trim
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).LOC_S_NAME) Then
                        str = str & " " + AssetData(iRowCnt).LOC_S_NAME.Trim
                    End If
                    '2020/01/23 nv miyaji 指定桁数でカットしないようにする
                    'If str.Length > LabelQRDaicho.StrLength Then
                    '    str = str.Substring(0, LabelQRDaicho.StrLength)
                    'End If
                    WS.Rows(LabelQRDaicho.Row.LocaNm + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic).Value = str
                    str = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) + " " + AssetData(iRowCnt).CAT_L_CD
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).CAT_M_CD.Trim) Then
                        str = str & "-" + AssetData(iRowCnt).CAT_M_CD.Trim
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).CAT_S_CD.Trim) Then
                        str = str & "-" + AssetData(iRowCnt).CAT_S_CD.Trim
                    End If
                    '2020/01/23 nv miyaji 指定桁数でカットしないようにする
                    'If str.Length > LabelQRDaicho.StrLength Then
                    '    str = str.Substring(0, LabelQRDaicho.StrLength)
                    'End If
                    WS.Rows(LabelQRDaicho.Row.CatCd + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic).Value = str
                    str = String.Empty
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).CAT_L_NAME) Then
                        str = "　" + AssetData(iRowCnt).CAT_L_NAME.Trim
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).CAT_M_NAME) Then
                        str = str & " " + AssetData(iRowCnt).CAT_M_NAME.Trim
                    End If
                    If Not String.IsNullOrEmpty(AssetData(iRowCnt).CAT_S_NAME) Then
                        str = str & " " + AssetData(iRowCnt).CAT_S_NAME.Trim
                    End If
                    '2020/01/23 nv miyaji 指定桁数でカットしないようにする
                    'If str.Length > LabelQRDaicho.StrLength Then
                    '    str = str.Substring(0, LabelQRDaicho.StrLength)
                    'End If
                    WS.Rows(LabelQRDaicho.Row.CatNm + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.Col.NotPic).Value = str

                End If

                If iRowCnt = AssetData.Count - 1 Then
                    '最終行の時、罫線を削除する

                    'カスタム名のスタイルを追加します。
                    Dim Style As IStyle = WB.Styles.Add("NoneStyle")
                    Style.Borders(BordersIndex.EdgeLeft).LineStyle = BorderLineStyle.None
                    Style.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.None
                    Style.Borders(BordersIndex.EdgeRight).LineStyle = BorderLineStyle.None
                    Style.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.None

                    '右の列だけにデータ行がある時、左の列だけ罫線を消す
                    If iRowCnt Mod 2 = 0 Then
                        For idelcol As Integer = 0 To LabelQRDaicho.LabelColSuu - 1
                            '上下の罫線の削除
                            WS.Rows(LabelQRDaicho.DelLineTop + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.DelLineLeft + LabelQRDaicho.ColCount + idelcol).Style = Style
                            WS.Rows(LabelQRDaicho.DelLineBottom + (LabelQRDaicho.RowCount * iRowCount)).Cells(LabelQRDaicho.DelLineLeft + LabelQRDaicho.ColCount + idelcol).Style = Style
                        Next
                        For idelrow As Integer = 0 To LabelQRDaicho.LabelRowSuu - 1
                            '左右の罫線の削除
                            WS.Rows(LabelQRDaicho.DelLineTop + (LabelQRDaicho.RowCount * iRowCount) + idelrow).Cells(LabelQRDaicho.DelLineLeft + LabelQRDaicho.ColCount).Style = Style
                            WS.Rows(LabelQRDaicho.DelLineTop + (LabelQRDaicho.RowCount * iRowCount) + idelrow).Cells(LabelQRDaicho.DelLineRight + LabelQRDaicho.ColCount).Style = Style
                        Next
                        iRowCount += 1
                    End If

                    'データ行の下の罫線を全て消す
                    If iRowCount < LabelQRDaicho.ColLabelSuu Then
                        For icnt As Integer = iRowCount To LabelQRDaicho.ColLabelSuu - 1
                            For idelcol As Integer = 0 To LabelQRDaicho.DelLineRight + LabelQRDaicho.ColCount - 1
                                '上下の罫線の削除
                                If Not idelcol = LabelQRDaicho.DelLineRight + 1 Then
                                    WS.Rows(LabelQRDaicho.DelLineTop + (LabelQRDaicho.RowCount * icnt)).Cells(idelcol).Style = Style
                                    WS.Rows(LabelQRDaicho.DelLineBottom + (LabelQRDaicho.RowCount * icnt)).Cells(idelcol).Style = Style
                                End If
                            Next
                            For idelrow As Integer = 0 To LabelQRDaicho.LabelRowSuu - 1
                                '左右の罫線の削除
                                WS.Rows(LabelQRDaicho.DelLineTop + (LabelQRDaicho.RowCount * icnt) + idelrow).Cells(LabelQRDaicho.DelLineLeft + LabelQRDaicho.ColCount).Style = Style
                                WS.Rows(LabelQRDaicho.DelLineTop + (LabelQRDaicho.RowCount * icnt) + idelrow).Cells(LabelQRDaicho.DelLineRight + LabelQRDaicho.ColCount).Style = Style
                                WS.Rows(LabelQRDaicho.DelLineTop + (LabelQRDaicho.RowCount * icnt) + idelrow).Cells(LabelQRDaicho.DelLineLeft).Style = Style
                                WS.Rows(LabelQRDaicho.DelLineTop + (LabelQRDaicho.RowCount * icnt) + idelrow).Cells(LabelQRDaicho.DelLineRight).Style = Style
                            Next
                        Next
                    End If
                End If

            Next

            'テンプレートシートを削除する
            WB.Worksheets(0).Delete()
            'WB.RemoveSheetAt(0)

        End Sub
#End Region

#Region "QR台帳(カスタム1)用のExcelファイル作成"
        ''' <summary>
        ''' QR台帳(カスタム1)用のExcelファイル作成   2023/10/17 ADD Sato v2.5
        ''' </summary>
        ''' <param name="WB">ブック</param>
        ''' <param name="WS">シート</param>
        ''' <param name="AssetData">資産マスタデータ</param>
        Private Sub CreateQRDaichoCustom1Xlsx(ByRef WB As IWorkbook, ByRef WS As IWorksheet, ByRef AssetData As List(Of AssetListLabelModel))

            'ブックの設定
            Dim sTempFullPath As String = Server.MapPath(CommonConst.TEMPLATE_FOLDERNM)
            WB.Open(sTempFullPath & "\" & CommonConst.DAICHO_CUSTOM1_TEMPLATE_FILENM) '読み込むテンプレートファイル

            'テンプレートシート取得する
            Dim WSTemp As IWorksheet = WB.Worksheets(0)
            'テンプレートシートを次のシートにコピーする
            WS = WSTemp.Copy()

            '画像ファイル貼り付け作業用
            Dim sQrFileName As String = String.Empty
            Dim sCallPath As String = String.Empty
            Dim ret = Nothing

            'エクセルデータ作成
            For i As Integer = 0 To AssetData.Count - 1
                '※DBの備考2がAsetraでは備考1として扱われる
                '※DBの備考とAsetraの備考は1つずつズレている
                WS.Range("ASSET_NO").Value = AssetData(i).ASSET_NO
                WS.Range("ASSET_NAME").Value = CommonRfid.TruncateString(AssetData(i).ASSET_NAME, QRDaichoCumstom1.TruncLength.AssetNm)
                WS.Range("FREE_ITEM1").Value = CommonRfid.TruncateString(AssetData(i).FREE_ITEM2, QRDaichoCumstom1.TruncLength.FreeItem1)
                WS.Range("FREE_ITEM2").Value = CommonRfid.TruncateString(AssetData(i).FREE_ITEM3, QRDaichoCumstom1.TruncLength.FreeItem2)
                WS.Range("FREE_ITEM3").Value = AssetData(i).FREE_ITEM4
                WS.Range("FREE_ITEM5").Value = AssetData(i).FREE_ITEM6
                WS.Range("FREE_ITEM6").Value = AssetData(i).FREE_ITEM7
                WS.Range("FREE_ITEM7").Value = AssetData(i).FREE_ITEM8

                'QR画像ファイルを作成
                'QRコードを作成する
                sQrFileName = Now.ToString("yyyyMMdd_") & Path.GetRandomFileName() & ".png"
                sCallPath = System.IO.Path.GetFullPath(HttpContext.Server.MapPath(Misc.CommonConst.QRIMAGE_FOLDERNM) & "\\" & sQrFileName)
                ret = Dao.MakeQr.GetQRData(AssetData(i).ASSET_NO, sCallPath)

                If ret = 0 Then
                    '各QRの情報を取得
                    Dim QR1 As QRImageInfo = QRImageInfo.GetQRPosition(WS, WS.Range("QR_IMAGE1"))
                    Dim QR2 As QRImageInfo = QRImageInfo.GetQRPosition(WS, WS.Range("QR_IMAGE2"))
                    Dim QR3 As QRImageInfo = QRImageInfo.GetQRPosition(WS, WS.Range("QR_IMAGE3"))

                    'ピクチャを追加します
                    Using data As Stream = IO.File.OpenRead(sCallPath)    '読み込む画像ファイル
                        Dim picture1 As IShape = WS.Shapes.AddPictureInPixel(data, ImageType.PNG, QR1.Left, QR1.Top, QR1.Width, QR1.Height)
                        Dim picture2 As IShape = WS.Shapes.AddPictureInPixel(data, ImageType.PNG, QR2.Left, QR2.Top, QR2.Width, QR2.Height)
                        Dim picture3 As IShape = WS.Shapes.AddPictureInPixel(data, ImageType.PNG, QR3.Left, QR3.Top, QR3.Width, QR3.Height)
                    End Using

                    'ファイルを削除。
                    If System.IO.File.Exists(sCallPath) Then
                        System.IO.File.Delete(sCallPath)
                    End If
                End If

                If i <> AssetData.Count - 1 Then
                    'テンプレートシートを次のシートにコピーする
                    WS = WSTemp.CopyAfter(WS)
                End If
            Next

            'テンプレートシートを削除する
            WB.Worksheets(0).Delete()

        End Sub
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

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】IsCsvFileError（CSVファイルのエラーチェック）"
'#Region "CSVファイルのエラーチェック"
        '''' <summary>
        '''' CSVファイルのエラーチェック
        '''' </summary>
        '''' <param name="file">チェック対象のファイル</param>
        '''' <returns>True:エラー無し　False:エラー有り</returns>
        'Public Function IsCsvFileError(file As HttpPostedFileBase) As Boolean
        '    Dim blRet As Boolean = False

        '    'ファイルがあるかどうか
        '    If IsNothing(file) Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NoSelect
        '        Return blRet
        '    End If

        '    'ファイルサイズ
        '    If file.ContentLength = 0 Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.Empty
        '        Return blRet
        '    End If

        '    '拡張子を検証
        '    Dim extention As String = Path.GetExtension(file.FileName).Replace(".", "")
        '    If String.IsNullOrEmpty(extention) OrElse extention.ToLower <> "csv" Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NotCsv
        '        Return blRet
        '    End If

        '    blRet = True
        '    Return blRet
        'End Function
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

#Region "管理ロケーションドロップダウンリストデータ取得"
        ''' <summary>
        ''' 管理ロケーションドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>2021/09/16 nv miyaji create</remarks>
        Private Function GetMngLocationLData() As JsonResult
            Dim lstLoc_Shokai As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            'ログイン情報がない場合、何もしない
            If _LoginInfo Is Nothing Then
                ViewBag.modelMngLocL = New SelectList(lstLoc_Shokai, "LOC_CD", "LOC_NAME", "")
                ViewBag.modalmodelMngLocL = New SelectList(lstLoc_Shokai, "LOC_CD", "LOC_NAME", "")
                Return Nothing
            End If

            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)

            '一覧画面用
            Dim tblShokai As DataTable = Dao.ToolLocation.GetLocationLData(_LoginInfo, AuthInfo, False)
            For Each row As DataRow In tblShokai.Rows
                lstLoc_Shokai.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            '一覧画面の管理ロケーションコンボ設定
            ViewBag.modelMngLocL = New SelectList(lstLoc_Shokai, "LOC_CD", "LOC_NAME", _ViewModel.SearchMngLocL)
            '_ViewModel.MnglocLDropdownData = lstLoc_Shokai

            'モーダル画面の管理ロケーションコンボ設定
            Dim SelectedCd As String = _ViewModel.MNG_LOC_L_CD
            If AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                '編集権限がある社員の場合（編集可能なモーダルダイアログ用）
                Dim tblEdit As DataTable
                Dim lstLoc_Edit As New List(Of LocationDropdownModel)     'ロケーションマスタリスト
                '編集可能なロケーションを取得
                tblEdit = Dao.ToolLocation.GetLocationLData(_LoginInfo, AuthInfo, True)
                For Each row As DataRow In tblEdit.Rows
                    lstLoc_Edit.Add(
                        New LocationDropdownModel With {
                            .LOC_CD = row("LOC_CD").ToString,
                            .LOC_NAME = row("LOC_NAME").ToString
                        }
                    )
                Next

                'ビューモデルを設定
                SelectedCd = _ViewModel.MNG_LOC_L_CD
                If String.IsNullOrEmpty(SelectedCd) Then
                    ViewBag.modalmodelMngLocL = New SelectList(lstLoc_Edit, "LOC_CD", "LOC_NAME", "")
                Else
                    ViewBag.modalmodelMngLocL = New SelectList(lstLoc_Edit, "LOC_CD", "LOC_NAME", SelectedCd)
                End If
            Else
                '編集権限がない社員の場合（照会のみ可能なモーダルダイアログ用）
                'ビューモデルを設定
                SelectedCd = _ViewModel.MNG_LOC_L_CD
                If String.IsNullOrEmpty(SelectedCd) Then
                    ViewBag.modalmodelMngLocL = New SelectList(lstLoc_Shokai, "LOC_CD", "LOC_NAME", "")
                Else
                    ViewBag.modalmodelMngLocL = New SelectList(lstLoc_Shokai, "LOC_CD", "LOC_NAME", SelectedCd)
                End If

            End If

            Return Json(lstLoc_Shokai)
        End Function
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
            Dim settingRfid As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)               'メッセージ兼設定リスト
            Dim modelLocationUsage As New LocationUsageModel        'ロケーション使用区分
            Dim setting As SettingInfo = settingRfid.getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingRfid.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            modelLocationUsage.LocUsage_L = setting.DATA1
            modelLocationUsage.LocUsage_M = setting.DATA2
            modelLocationUsage.LocUsage_S = setting.DATA3

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)

            If locationLevel = LOC_DIV.LocationM Then
                If modelLocationUsage.LocUsage_M = Misc.ItemUse.Yes Then
                    Return Json(GetLocationMData(selectLCode))
                Else
                    '中区分 使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            Else

                If modelLocationUsage.LocUsage_S = Misc.ItemUse.Yes Then
                    '中区分は複数選択のため、「,」区切りで複数のコードが渡される
                    Return Json(GetLocationSData(selectLCode, CommonRfid.GetSelectedValues(CommonRfid.ToStringNvl(selectMCode).Split(","c))))
                Else
                    '小区分　使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

        '[CC431] ADD Start 20261007
#Region "検索条件のロケーションの選択肢（場所から選ぶ）"
        ''' <summary>
        ''' 検索条件のロケーション（原価センタ・場所）の選択肢を返す（画面の ajax から呼ぶ）
        ''' </summary>
        ''' <param name="selectLCode">会社（大区分コード）</param>
        ''' <param name="level">返す側（M：中区分、S：小区分）</param>
        ''' <param name="reverse">場所から選ぶ（True：場所で原価センタを絞り込む、False：原価センタで場所を絞り込む）</param>
        ''' <param name="codes">絞り込みに使う、反対側の選択中のコード（「,」区切り）</param>
        ''' <returns>選択肢（LOC_CD／LOC_NAME のリスト）</returns>
        Public Function GetSearchLocationOptions(ByVal selectLCode As String, ByVal level As String, ByVal reverse As Boolean, ByVal codes As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

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
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)

            Return Json(GetSearchLocationList(level, reverse, CommonRfid.ToStringNvl(codes).Split(","c), selectLCode, authInfo))

        End Function

        ''' <summary>
        ''' 検索条件の原価センタ・場所の選択肢を、場所から選ぶの ON／OFF と選択値に合わせて作る
        ''' </summary>
        ''' <param name="authInfo">権限情報</param>
        Private Sub SetSearchLocationOptions(authInfo As AuthInfo)
            Dim selLocM As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocM)
            Dim selLocS As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocS)

            ViewBag.modelLocM = New MultiSelectList(
                GetSearchLocationList(LOC_DIV.LocationM, _ViewModel.SearchLocReverse, selLocS, _ViewModel.SearchLocL, authInfo),
                "LOC_CD", "LOC_NAME", selLocM)
            ViewBag.modelLocS = New MultiSelectList(
                GetSearchLocationList(LOC_DIV.LocationS, _ViewModel.SearchLocReverse, selLocM, _ViewModel.SearchLocL, authInfo),
                "LOC_CD", "LOC_NAME", selLocS)
        End Sub

        ''' <summary>
        ''' 検索条件のロケーションの選択肢を取得する（権限あり かつ 資産M（除却済以外）に存在するもの）
        ''' 場所から選ぶ OFF：中区分は絞り込みなし、小区分は選択中の中区分で絞り込む
        ''' 場所から選ぶ ON ：小区分は絞り込みなし、中区分は選択中の小区分で絞り込む
        ''' 会社が未選択の場合、絞り込まれる側で絞り込む側が未選択の場合は空
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
            Dim exTarget As Dao.ToolLocation.LocExistTarget = Dao.ToolLocation.LocExistTarget.Asset()
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

#Region "並び順のソートカラム取得"
        ''' <summary>
        ''' 並び順(表示順)に対応するソートカラムを取得する。
        ''' </summary>
        ''' <param name="dispSelect">検索条件 表示順(AssetDispSelect)</param>
        ''' <returns>ORDER BY に指定するカラム名</returns>
        ''' <remarks>
        ''' 検索条件の並び順ラジオ(16種)で選ばれた値を、ORDER BY のカラム名に変換する。
        ''' 一覧表示用SQLとCSV出力用SQLの双方から呼び出し、並び順のロジックを1箇所に集約している。
        ''' 第2ソートキーの資産番号(ASSET_NO)は呼び出し元で付与する。
        ''' 想定外の値が渡された場合は、既定の並び順である資産クラス(CAT_L_CD)を返す。
        ''' </remarks>
        Private Function GetSortColumn(ByVal dispSelect As Integer) As String
            Select Case dispSelect
                Case AssetDispSelect.CatLAsc
                    Return "ASSET.CAT_L_CD"
                Case AssetDispSelect.AssetNoAsc
                    Return "ASSET.ASSET_NO"
                Case AssetDispSelect.LocMAsc
                    Return "ASSET.LOC_M_CD"
                Case AssetDispSelect.LocSAsc
                    Return "ASSET.LOC_S_CD"
                Case AssetDispSelect.FreeItem2Asc
                    Return "ASSET.FREE_ITEM2"
                Case AssetDispSelect.FreeItem6Asc
                    Return "ASSET.FREE_ITEM6"
                Case AssetDispSelect.Remark1Asc
                    Return "ASSET.REMARK1"
                Case AssetDispSelect.Remark2Asc
                    Return "ASSET.REMARK2"
                Case AssetDispSelect.Remark3Asc
                    Return "ASSET.REMARK3"
                Case AssetDispSelect.Remark4Asc
                    Return "ASSET.REMARK4"
                Case AssetDispSelect.Remark5Asc
                    Return "ASSET.REMARK5"
                Case AssetDispSelect.Remark6Asc
                    Return "ASSET.REMARK6"
                Case AssetDispSelect.Remark7Asc
                    Return "ASSET.REMARK7"
                Case AssetDispSelect.Remark8Asc
                    Return "ASSET.REMARK8"
                Case AssetDispSelect.Remark9Asc
                    Return "ASSET.REMARK9"
                Case AssetDispSelect.Remark10Asc
                    Return "ASSET.REMARK10"
                Case Else
                    Return "ASSET.CAT_L_CD"
            End Select
        End Function
#End Region

#Region "大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 大区分ドロップダウンリストデータ取得
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
            '一覧画面のロケーションコンボ設定
            ViewBag.modelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchLocL)
            _ViewModel.locLDropdownData = lstLocation

            'モーダル画面のロケーションコンボ設定
            Dim SelectedCd As String = _ViewModel.LOC_L_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", "")
            Else
                ViewBag.modalmodelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", SelectedCd)
            End If

            Return Json(lstLocation)
        End Function
#End Region

#Region "中区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 中区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLocationMData(ByVal selectLCode As String) As JsonResult
            Dim tbl As DataTable
            If Not _LoginInfo Is Nothing Then
                tbl = Dao.ToolLocation.GetLocationMData(selectLCode, _LoginInfo, _AuthInfo)
            Else
                tbl = Dao.ToolLocation.GetLocationMData(selectLCode)
            End If
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            'データを入れる
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

            'モーダル画面のロケーションコンボ設定
            Dim SelectedCd As String = _ViewModel.LOC_M_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelLocM = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", "")
            Else
                ViewBag.modalmodelLocM = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", SelectedCd)
            End If

            Return Json(lstLocation)
        End Function
#End Region

#Region "小区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCodes As String()) As JsonResult
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            '中区分は複数選択のため、選択された中区分ごとに小区分を取得してマージする
            For Each selectMCode As String In CommonRfid.GetSelectedValues(selectMCodes)
                Dim tbl As DataTable = Dao.ToolLocation.GetLocationSData(selectLCode, selectMCode)

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

            'モーダル画面のロケーションコンボ設定
            Dim SelectedCd As String = _ViewModel.LOC_S_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", "")
            Else
                ViewBag.modalmodelLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", SelectedCd)
            End If

            Return Json(lstLocation)
        End Function
#End Region

#Region "カテゴリー　分類区分ドロップダウン変更処理"
        ''' <summary>
        ''' カテゴリー　分類区分ドロップダウン変更処理
        ''' </summary>
        ''' <param name="categoryLevel"></param>
        ''' <param name="selectLCode"></param>
        ''' <param name="selectMCode"></param>
        ''' <returns></returns>
        Public Function ChangeCategory(ByVal categoryLevel As String, ByVal selectLCode As String, ByVal selectMCode As String) As ActionResult
            If categoryLevel = CAT_DIV.CategoryM Then
                Return Json(GetCategoryMData(selectLCode))
            Else
                Return Json(GetCategorySData(selectLCode, selectMCode))
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
#End Region

#Region "カテゴリー　大分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' カテゴリー　大分類ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCategoryLData() As JsonResult
            Dim tbl As DataTable = Dao.ToolCategory.GetCategoryLData()
            Dim lst As New List(Of CategoryDropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New CategoryDropdownModel With {
                        .CAT_CD = row("CAT_CD").ToString,
                        .CAT_NAME = row("CAT_NAME").ToString
                    }
                )
            Next

            '一覧画面のカテゴリーコンボ設定（複数選択のため MultiSelectList）
            ViewBag.modelCatL = New MultiSelectList(lst, "CAT_CD", "CAT_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchCatL))
            _ViewModel.catLDropdownData = lst

            'モーダル画面のカテゴリーコンボ設定
            Dim SelectedCd As String = _ViewModel.CAT_L_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelCatL = New SelectList(lst, "CAT_CD", "CAT_NAME", "")
            Else
                ViewBag.modalmodelCatL = New SelectList(lst, "CAT_CD", "CAT_NAME", SelectedCd)
            End If

            Return Json(lst)
        End Function
#End Region

#Region "カテゴリー　中分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' カテゴリー　中分類ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCategoryMData(selectLCode As String) As JsonResult
            Dim tbl As DataTable = Dao.ToolCategory.GetCategoryMData(selectLCode)
            Dim lst As New List(Of CategoryDropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New CategoryDropdownModel With {
                        .CAT_CD = row("CAT_CD").ToString,
                        .CAT_NAME = row("CAT_NAME").ToString
                    }
                )
            Next

            '一覧画面のカテゴリーコンボ設定
            ViewBag.modelCatM = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatM)
            _ViewModel.catMDropdownData = lst

            'モーダル画面のカテゴリーコンボ設定
            Dim SelectedCd As String = _ViewModel.CAT_M_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelCatM = New SelectList(lst, "CAT_CD", "CAT_NAME", "")
            Else
                ViewBag.modalmodelCatM = New SelectList(lst, "CAT_CD", "CAT_NAME", SelectedCd)
            End If

            Return Json(lst)
        End Function
#End Region

#Region "カテゴリー　小分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' カテゴリー　小分類ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCategorySData(selectLCode As String, selectMCode As String) As JsonResult
            Dim tbl As DataTable = Dao.ToolCategory.GetCategorySData(selectLCode, selectMCode)
            Dim lst As New List(Of CategoryDropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New CategoryDropdownModel With {
                        .CAT_CD = row("CAT_CD").ToString,
                        .CAT_NAME = row("CAT_NAME").ToString
                    }
                )
            Next

            '一覧画面のカテゴリーコンボ設定
            ViewBag.modelCatS = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatS)
            _ViewModel.catSDropdownData = lst

            'モーダル画面のカテゴリーコンボ設定
            Dim SelectedCd As String = _ViewModel.CAT_S_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelCatS = New SelectList(lst, "CAT_CD", "CAT_NAME", "")
            Else
                ViewBag.modalmodelCatS = New SelectList(lst, "CAT_CD", "CAT_NAME", SelectedCd)
            End If

            Return Json(lst)
        End Function
#End Region

#Region "貸出先マスタドロップダウンリストデータ取得処理"
        ''' <summary>
        ''' 貸出先マスタドロップダウンリストデータ取得処理
        ''' </summary>
        Public Sub GetLeasePlaceData()

            Dim lst As List(Of LeasePlaceModel)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext
            'M_DEPARTMENTテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT *")
            sbSQL.AppendLine(" FROM ")
            sbSQL.AppendLine("  M_LEASE_PLACE ")
            sbSQL.AppendLine("ORDER BY LEASE_PLACE_CD") '2020/03/10 nv miyaji add
            'SQLを実行する
            lst = db.LeasePlace.SqlQuery(sbSQL.ToString).ToList
            '貸出先コンボ設定
            If String.IsNullOrEmpty(_ViewModel.SearchLeasePlaceCd) Then
                ViewBag.LeasePlaceList = New SelectList(lst, "LEASE_PLACE_CD", "LEASE_PLACE_NAME", "")
            Else
                ViewBag.LeasePlaceList = New SelectList(lst, "LEASE_PLACE_CD", "LEASE_PLACE_NAME", _ViewModel.SearchLeasePlaceCd)
            End If

        End Sub
#End Region

#Region "貸出状態ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 貸出状態ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>2021/09/17 nv miyaji create</remarks>
        Private Function GetLeaseStatusData() As JsonResult
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim lst As New List(Of EpcSizeDropdownModel)
            lst.Add(New EpcSizeDropdownModel With {.Cd = AssetLeaseStatus.Ng, .Name = Me._Message.getMessage(CommonLabel.LABEL_FUKA)})
            lst.Add(New EpcSizeDropdownModel With {.Cd = AssetLeaseStatus.Ok, .Name = Me._Message.getMessage(CommonLabel.LABEL_KANOU)})

            '初期値設定
            'Dim sLeaseStatus As String = AssetLeaseStatus.Ng
            'If Not String.IsNullOrEmpty(_ViewModel.LEASE_STATUS) Then
            '    sLeaseStatus = _ViewModel.LEASE_STATUS
            'End If

            'ViewBag.LeaseStatus = New SelectList(lst, "Cd", "Name", sLeaseStatus)
            ViewBag.LeaseStatus = New SelectList(lst, "Cd", "Name", _ViewModel.SearchLeaseStatus)


            '_ViewModel.epcSizeDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "プリンター選択プルダウン用データ取得"
        ''' <summary>
        ''' プリンター選択プルダウン用データ取得
        ''' </summary>
        ''' <returns></returns>
        ''' Public Function ShowPrinterList(Cals As String()) As ActionResult
        Public Function ShowPrinterList(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As ActionResult

            '最新のチェック状態でセッション情報を更新する(追加および削除)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(SelectKeys, NoneSelectKeys)

            Dim db As New SystemDBContext       'ここに移動　　2021/10/05 nv Inoue

            '2021/10/05 ADD nv Inoue start
            '
            '  権限による実行可能チェック
            '
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)

            Dim sbAuthChkSQL As StringBuilder = New StringBuilder
            Dim lstParam As New List(Of SqlParameter)

            Dim sSelASSET_NO As String = "'" & String.Join("','", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetListKey.ASSET_NO))) & "'"

            sbAuthChkSQL.AppendLine("SELECT TOP 1 *")
            sbAuthChkSQL.AppendLine("FROM M_ASSET")
            sbAuthChkSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
            sbAuthChkSQL.AppendLine("    ON AUTH.LOC_L_CD = M_ASSET.MNG_LOC_L_CD ")
            lstParam.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
            lstParam.Add(New SqlParameter("@ReadAuthFlg", AuthInfo.ReadAuthFlg))
            lstParam.Add(New SqlParameter("@UpdAuthFlg", AuthInfo.UpdAuthFlg))
            lstParam.Add(New SqlParameter("@ReadHubAuthFlg", AuthInfo.ReadHubAuthFlg))
            lstParam.Add(New SqlParameter("@UpdHubAuthFlg", AuthInfo.UpdHubAuthFlg))
            lstParam.Add(New SqlParameter("@UpdAuthType", Misc.UpdAuthType.UPD_AUTH))
            sbAuthChkSQL.AppendLine("WHERE M_ASSET.ASSET_NO in (" & sSelASSET_NO & ") ")
            sbAuthChkSQL.AppendLine("AND   AUTH.UPD_FLG = '" & Misc.UpdFlg.No & "'")

            'SQLを実行する
            Dim chkModel As List(Of AssetListModel) = db.AssetList.SqlQuery(sbAuthChkSQL.ToString, lstParam.ToArray).ToList
            If chkModel.Count > 0 Then
                Return Json("auth-error")
            End If
            '2021/10/05 ADD nv Inoue end

            'カテゴリーを重複なしで取得する(資産番号, 資産ID, カテゴリーL)
            Dim Cals As String() = lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetListKey.CATEGORY_L)).Distinct.ToArray
            Dim sCalsStr As String = "'" & String.Join("','", Cals) & "'"

            ''カテゴリーを取得し、資産ＩＤ生成パターンが方法①の時、エラーを返す
            'Dim db As New SystemDBContext
            'For Each Str As String In Cals
            '    'IDをキーにロケーションモデルを取得する
            '    Dim findKeys As String() = {CAT_DIV.CategoryL, Str, " ", " "}
            '    Dim catmodel As CategoryModel = db.Category.Find(findKeys)
            '    If catmodel.ASSET_ID_REG_DIV = AssetIDGenerationMethod.None Then
            '        Return Json("error")
            '    End If
            'Next


            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT TOP 1 *")
            sbSQL.AppendLine("FROM M_CATEGORY")
            sbSQL.AppendLine("WHERE CAT_DIV = '" & Misc.CAT_DIV.CategoryL & "'")
            sbSQL.AppendLine("AND   CAT_L_CD in (" & sCalsStr & ") ")
            sbSQL.AppendLine("AND   ASSET_ID_REG_DIV = " & Misc.AssetIDGenerationMethod.None.ToString)

            'SQLを実行する
            Dim catmodel As List(Of CategoryModel) = db.Category.SqlQuery(sbSQL.ToString).ToList
            If catmodel.Count > 0 Then
                Return Json("error")
            End If


            Dim lstSetting As List(Of PrinterDropdownModel)   'プリンタリスト
            lstSetting = GetPrinter()
            Return Json(lstSetting)
        End Function
#End Region

#Region "プリンタ情報取得"
        ''' <summary>
        ''' プリンタ情報取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetPrinter() As List(Of PrinterDropdownModel)
            Dim tbl As DataTable = Dao.ToolSetting.GetMstPrinterInfo()
            Dim lst As New List(Of PrinterDropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New PrinterDropdownModel With {
                        .KEY1 = row("KEY1").ToString,
                        .DATA1 = row("DATA1").ToString
                    }
                )
            Next
            ViewBag.modelPrinter = New SelectList(lst, "KEY1", "DATA1")
            Return lst
        End Function
#End Region

        '2023/02/08 Kawamoto ADD START
#Region "フリー項目3（消防法の分類）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' フリー項目3（消防法の分類）ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetFreeItem3Data() As JsonResult
            Dim tbl As DataTable = Dao.ToolSetting.GetDrugsFSActNumberInfo()
            Dim lst As New List(Of DropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New DropdownModel With {
                        .Cd = row("DATA1").ToString,
                        .Name = row("DATA1").ToString
                    }
                )
            Next

            '一覧画面のフリー項目3（消防法の分類）コンボ設定
            ViewBag.FreeItem3List = New SelectList(lst, "Cd", "Name", _ViewModel.SearchFreeItem3)

            'モーダル画面のフリー項目3（消防法の分類）コンボ設定
            If String.IsNullOrEmpty(_ViewModel.FREE_ITEM3) Then
                ViewBag.modalFreeItem3List = New SelectList(lst, "Cd", "Name", "")
            Else
                ViewBag.modalFreeItem3List = New SelectList(lst, "Cd", "Name", _ViewModel.FREE_ITEM3)
            End If

            Return Json(lst)
        End Function
#End Region

#Region "フリー項目4（有機則の該当有無）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' フリー項目4（有機則の該当有無）ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetFreeItem4Data() As JsonResult
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim lst As New List(Of DropdownModel)
            lst.Add(New DropdownModel With {.Cd = Exist.Yes, .Name = Me._Message.getMessage(CommonLabel.VALUE_YES)})
            lst.Add(New DropdownModel With {.Cd = Exist.No, .Name = Me._Message.getMessage(CommonLabel.VALUE_NO)})
            'lst.Add(New DropdownModel With {.Cd = Exist.None, .Name = Me._Message.getMessage(CommonLabel.VALUE_NONE)})  '2023/05/10 Kawamoto DEL

            '一覧画面のフリー項目4（有機則の該当有無）コンボ設定
            ViewBag.FreeItem4List = New SelectList(lst, "Cd", "Name", _ViewModel.SearchFreeItem4)

            'モーダル画面のフリー項目4（有機則の該当有無）コンボ設定
            If String.IsNullOrEmpty(_ViewModel.FREE_ITEM4) Then
                ViewBag.modalFreeItem4List = New SelectList(lst, "Cd", "Name", "")
            Else
                ViewBag.modalFreeItem4List = New SelectList(lst, "Cd", "Name", _ViewModel.FREE_ITEM4)
            End If

            Return Json(lst)
        End Function
#End Region

#Region "フリー項目5（毒物及び劇薬取扱法の該当）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' フリー項目5（毒物及び劇薬取扱法の該当）ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetFreeItem5Data() As JsonResult
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim lst As New List(Of DropdownModel)
            lst.Add(New DropdownModel With {.Cd = PoisonAndDeleterious.Poison, .Name = PoisonAndDeleterious.Poison & ":" & Me._Message.getMessage(CommonLabel.VALUE_POISIN)})
            lst.Add(New DropdownModel With {.Cd = PoisonAndDeleterious.Deleterious, .Name = PoisonAndDeleterious.Deleterious & ":" & Me._Message.getMessage(CommonLabel.VALUE_DELETERIOUS)})
            lst.Add(New DropdownModel With {.Cd = PoisonAndDeleterious.None, .Name = PoisonAndDeleterious.None & ":" & Me._Message.getMessage(CommonLabel.VALUE_UNSPECIFIED)})

            '一覧画面のフリー項目5（毒物及び劇薬取扱法の該当）コンボ設定
            ViewBag.FreeItem5List = New SelectList(lst, "Cd", "Name", _ViewModel.SearchFreeItem5)

            'モーダル画面のフリー項目5（毒物及び劇薬取扱法の該当）コンボ設定
            If String.IsNullOrEmpty(_ViewModel.FREE_ITEM5) Then
                ViewBag.modalFreeItem5List = New SelectList(lst, "Cd", "Name", "")
            Else
                ViewBag.modalFreeItem5List = New SelectList(lst, "Cd", "Name", _ViewModel.FREE_ITEM5)
            End If

            Return Json(lst)
        End Function
#End Region

#Region "フリー項目6（特化則の該当有無）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' フリー項目6（特化則の該当有無）ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetFreeItem6Data() As JsonResult
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim lst As New List(Of DropdownModel)
            lst.Add(New DropdownModel With {.Cd = Exist.Yes, .Name = Me._Message.getMessage(CommonLabel.VALUE_YES)})
            lst.Add(New DropdownModel With {.Cd = Exist.No, .Name = Me._Message.getMessage(CommonLabel.VALUE_NO)})
            'lst.Add(New DropdownModel With {.Cd = Exist.None, .Name = Me._Message.getMessage(CommonLabel.VALUE_NONE)})  '2023/05/10 Kawamoto DEL

            '一覧画面のフリー項目6（特化則の該当有無）コンボ設定
            ViewBag.FreeItem6List = New SelectList(lst, "Cd", "Name", _ViewModel.SearchFreeItem6)

            'モーダル画面のフリー項目6（特化則の該当有無）コンボ設定
            If String.IsNullOrEmpty(_ViewModel.FREE_ITEM6) Then
                ViewBag.modalFreeItem6List = New SelectList(lst, "Cd", "Name", "")
            Else
                ViewBag.modalFreeItem6List = New SelectList(lst, "Cd", "Name", _ViewModel.FREE_ITEM6)
            End If

            Return Json(lst)
        End Function
#End Region

#Region "フリー項目7（PRTR制度の該当有無）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' フリー項目7（PRTR制度の該当有無）ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetFreeItem7Data() As JsonResult
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim lst As New List(Of DropdownModel)
            lst.Add(New DropdownModel With {.Cd = Exist.Yes, .Name = Me._Message.getMessage(CommonLabel.VALUE_YES)})
            lst.Add(New DropdownModel With {.Cd = Exist.No, .Name = Me._Message.getMessage(CommonLabel.VALUE_NO)})
            'lst.Add(New DropdownModel With {.Cd = Exist.None, .Name = Me._Message.getMessage(CommonLabel.VALUE_NONE)})  '2023/05/10 Kawamoto DEL

            '一覧画面のフリー項目7（PRTR制度の該当有無）コンボ設定
            ViewBag.FreeItem7List = New SelectList(lst, "Cd", "Name", _ViewModel.SearchFreeItem7)

            'モーダル画面のフリー項目7（PRTR制度の該当有無）コンボ設定
            If String.IsNullOrEmpty(_ViewModel.FREE_ITEM7) Then
                ViewBag.modalFreeItem7List = New SelectList(lst, "Cd", "Name", "")
            Else
                ViewBag.modalFreeItem7List = New SelectList(lst, "Cd", "Name", _ViewModel.FREE_ITEM7)
            End If

            Return Json(lst)
        End Function
#End Region

#Region "フリー項目8（化学物質ﾘｽｸｱｾｽﾒﾝﾄ実施義務の有無）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' フリー項目8（化学物質ﾘｽｸｱｾｽﾒﾝﾄ実施義務の有無）ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetFreeItem8Data() As JsonResult
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim lst As New List(Of DropdownModel)
            lst.Add(New DropdownModel With {.Cd = Exist.Yes, .Name = Me._Message.getMessage(CommonLabel.VALUE_YES)})
            lst.Add(New DropdownModel With {.Cd = Exist.No, .Name = Me._Message.getMessage(CommonLabel.VALUE_NO)})
            'lst.Add(New DropdownModel With {.Cd = Exist.None, .Name = Me._Message.getMessage(CommonLabel.VALUE_NONE)})  '2023/05/10 Kawamoto DEL

            '一覧画面のフリー項目8（化学物質ﾘｽｸｱｾｽﾒﾝﾄ実施義務の有無）コンボ設定
            ViewBag.FreeItem8List = New SelectList(lst, "Cd", "Name", _ViewModel.SearchFreeItem8)

            'モーダル画面のフリー項目8（化学物質ﾘｽｸｱｾｽﾒﾝﾄ実施義務の有無）コンボ設定
            If String.IsNullOrEmpty(_ViewModel.FREE_ITEM8) Then
                ViewBag.modalFreeItem8List = New SelectList(lst, "Cd", "Name", "")
            Else
                ViewBag.modalFreeItem8List = New SelectList(lst, "Cd", "Name", _ViewModel.FREE_ITEM8)
            End If

            Return Json(lst)
        End Function
#End Region

#Region "フリー項目9（購入者）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' フリー項目9（購入者）ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetFreeItem9Data() As JsonResult
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB

            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Try
                'M_EMPLOYEEテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  EMPLOYEE_NAME ")
                sbSQL.AppendLine(" ,CONCAT(EMPLOYEE_CD, '" & CommonConst.CodeNameDelimiter & "', EMPLOYEE_NAME) AS EMPLOYEE_CD_NAME ")
                sbSQL.AppendLine("FROM ")
                sbSQL.AppendLine("  M_EMPLOYEE ")
                sbSQL.AppendLine("WHERE ")
                sbSQL.AppendLine("  AUTH_DIV <> '" & Misc.AUTH_DIV.SuperAdmin & "'")
                sbSQL.AppendLine("AND ")
                sbSQL.AppendLine("  RETIRE_FLG <> '" & Misc.RetireFlg.Cd.Yes & "'")
                sbSQL.AppendLine("ORDER BY EMPLOYEE_NAME")
                'SQLを実行する
                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                Dim lst As New List(Of DropdownModel)
                For Each row As DataRow In tbl.Rows
                    lst.Add(
                        New DropdownModel With {
                            .Cd = row("EMPLOYEE_NAME").ToString,
                            .Name = row("EMPLOYEE_CD_NAME").ToString
                        }
                    )
                Next

                'モーダル画面のフリー項目9（購入者）コンボ設定
                If String.IsNullOrEmpty(_ViewModel.FREE_ITEM9) Then
                    ViewBag.modalFreeItem9List = New SelectList(lst, "Cd", "Name", "")
                Else
                    ViewBag.modalFreeItem9List = New SelectList(lst, "Cd", "Name", _ViewModel.FREE_ITEM9)
                End If

                Return Json(lst)
            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "フリー項目15（計量器）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' フリー項目15（計量器）ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetFreeItem15Data() As JsonResult
            Dim tbl As DataTable = Dao.ToolSetting.GetDrugsWeighingMachineInfo()
            Dim lst As New List(Of DropdownModel)

            For Each row As DataRow In tbl.Rows
                If row("DATA2").ToString = DrugsWeighingMachine.Data2.Yes Then
                    lst.Add(
                        New DropdownModel With {
                            .Cd = row("DATA3").ToString,
                            .Name = row("DATA3").ToString
                        }
                    )
                End If
            Next

            '一覧画面のフリー項目15（計量器）コンボ設定
            ViewBag.FreeItem15List = New SelectList(lst, "Cd", "Name", _ViewModel.SearchFreeItem15)

            'モーダル画面のフリー項目15（計量器）設定
            If String.IsNullOrEmpty(_ViewModel.FREE_ITEM15) Then
                ViewBag.modalFreeItem15List = New SelectList(lst, "Cd", "Name", "")
            Else
                ViewBag.modalFreeItem15List = New SelectList(lst, "Cd", "Name", _ViewModel.FREE_ITEM15)
            End If

            Return Json(lst)
        End Function
#End Region
        '2023/02/08 Kawamoto ADD END

#Region "フリー項目10（資産貸与状態区分）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' フリー項目10（資産貸与状態区分）ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetFreeItem10Data() As JsonResult
            Dim lst As New List(Of DropdownModel)
            lst.Add(
                    New DropdownModel With {
                        .Cd = LOAN_ASSET_STATUS.BLANK,
                        .Name = LOAN_ASSET_STATUS_VALUE.BLANK
                    }
                )
            lst.Add(
                    New DropdownModel With {
                        .Cd = LOAN_ASSET_STATUS.ON_LOAN,
                        .Name = LOAN_ASSET_STATUS_VALUE.ON_LOAN
                    }
                )
            lst.Add(
                    New DropdownModel With {
                        .Cd = LOAN_ASSET_STATUS.RETURNED,
                        .Name = LOAN_ASSET_STATUS_VALUE.RETURNED
                    }
                )

            '一覧画面のフリー項目10（資産貸与状態区分）コンボ設定
            ViewBag.FreeItem10List = New SelectList(lst, "Cd", "Name", _ViewModel.SearchFreeItem10)

            Return Json(lst)
        End Function
#End Region

#Region "編集ボタンクリック時、モーダル表示のロケーションドロップダウンリストデータを取得する"
        ''' <summary>
        ''' 編集ボタンクリック時、モーダル表示のロケーションドロップダウンリストデータを取得する
        ''' </summary>
        Private Sub GetModalLocationData(ByRef model As AssetListModel)
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationLData()
            Dim lstLocationL As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            '大区分取得
            For Each row As DataRow In tbl.Rows
                lstLocationL.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.modalmodelLocL = New SelectList(lstLocationL, "LOC_CD", "LOC_NAME", model.LOC_L_CD)
            model.locLDropdownData = lstLocationL

            '中区分取得
            If Not String.IsNullOrEmpty(model.LOC_L_CD) Then
                tbl = Dao.ToolLocation.GetLocationMData(model.LOC_L_CD)
                Dim lstLocationM As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

                For Each row As DataRow In tbl.Rows
                    lstLocationM.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
                Next

                ViewBag.modalmodelLocM = New SelectList(lstLocationM, "LOC_CD", "LOC_NAME", model.LOC_M_CD)
                model.locMDropdownData = lstLocationM

                '小区分取得
                If Not String.IsNullOrEmpty(model.LOC_M_CD) Then
                    tbl = Dao.ToolLocation.GetLocationSData(model.LOC_L_CD, model.LOC_M_CD)
                    Dim lstLocationS As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

                    For Each row As DataRow In tbl.Rows
                        lstLocationS.Add(
                        New LocationDropdownModel With {
                            .LOC_CD = row("LOC_CD").ToString,
                            .LOC_NAME = row("LOC_NAME").ToString
                        }
                    )
                    Next

                    ViewBag.modalmodelLocS = New SelectList(lstLocationS, "LOC_CD", "LOC_NAME", model.LOC_S_CD)
                    model.locSDropdownData = lstLocationS
                End If
            End If

        End Sub
#End Region

#Region "編集ボタンクリック時、モーダル表示のカテゴリードロップダウンリストデータを取得する"
        ''' <summary>
        ''' 編集ボタンクリック時、モーダル表示のカテゴリードロップダウンリストデータを取得する
        ''' </summary>
        Private Sub GetModalCategoryData(ByRef model As AssetListModel)
            Dim tbl As DataTable = Dao.ToolCategory.GetCategoryLData()
            Dim lstL As New List(Of CategoryDropdownModel)

            For Each row As DataRow In tbl.Rows
                lstL.Add(
                    New CategoryDropdownModel With {
                        .CAT_CD = row("CAT_CD").ToString,
                        .CAT_NAME = row("CAT_NAME").ToString
                    }
                )
            Next
            ViewBag.modalmodelCatL = New SelectList(lstL, "CAT_CD", "CAT_NAME", model.CAT_L_CD)
            model.catLDropdownData = lstL

            '中区分取得
            If Not String.IsNullOrEmpty(model.CAT_L_CD) Then
                tbl = Dao.ToolCategory.GetCategoryMData(model.CAT_L_CD)
                Dim lstM As New List(Of CategoryDropdownModel)

                For Each row As DataRow In tbl.Rows
                    lstM.Add(
                    New CategoryDropdownModel With {
                        .CAT_CD = row("CAT_CD").ToString,
                        .CAT_NAME = row("CAT_NAME").ToString
                    }
                )
                Next
                ViewBag.modalmodelCatM = New SelectList(lstM, "CAT_CD", "CAT_NAME", model.CAT_M_CD)
                model.catMDropdownData = lstM

                '小区分取得
                If Not String.IsNullOrEmpty(model.CAT_M_CD) Then
                    tbl = Dao.ToolCategory.GetCategorySData(model.CAT_L_CD, model.CAT_M_CD)
                    Dim lstS As New List(Of CategoryDropdownModel)

                    For Each row As DataRow In tbl.Rows
                        lstS.Add(
                    New CategoryDropdownModel With {
                        .CAT_CD = row("CAT_CD").ToString,
                        .CAT_NAME = row("CAT_NAME").ToString
                    }
                )
                    Next
                    ViewBag.modalmodelCatS = New SelectList(lstS, "CAT_CD", "CAT_NAME", model.CAT_S_CD)
                    model.catSDropdownData = lstS
                End If
            End If

        End Sub
#End Region

#Region "照会ボタンクリック時、モーダル表示の管理ロケーションドロップダウンリストデータを取得する"
        ''' <summary>
        ''' 照会ボタンクリック時、モーダル表示の管理ロケーションドロップダウンリストデータを取得する
        ''' </summary>
        Private Sub GetModalMngLocationData(ByRef model As AssetListModel)
            '編集権限がない社員の場合（照会のみ可能なモーダルダイアログ用）
            Dim tblShokai As DataTable
            Dim lstLoc_Shokai As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            'ログイン情報がない場合、何もしない。
            If _LoginInfo Is Nothing Then
                ViewBag.modelMngLocL = New SelectList(lstLoc_Shokai, "LOC_CD", "LOC_NAME", "")
                ViewBag.modalmodelMngLocL = New SelectList(lstLoc_Shokai, "LOC_CD", "LOC_NAME", "")
                Return
            End If

            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)
            If AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                '照会可能なロケーションを取得
                tblShokai = Dao.ToolLocation.GetLocationLData(_LoginInfo, AuthInfo, False)
                For Each row As DataRow In tblShokai.Rows
                    lstLoc_Shokai.Add(
                        New LocationDropdownModel With {
                            .LOC_CD = row("LOC_CD").ToString,
                            .LOC_NAME = row("LOC_NAME").ToString
                        }
                    )
                Next

                'ドロップダウンリスト設定
                model.MnglocLDropdownData = lstLoc_Shokai
            End If

        End Sub
#End Region

#Region "QRコードデータを取得する"

        ''' <summary>
        ''' QRコードデータを取得する（AJax)
        '''   参考　https://blog.ch3cooh.jp/entry/20140722/1405990800
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        Public Function GetQRData(ByVal sAssetNo As String) As String
            'QRコードを作成する

            'Dim sAssetId As String = Dao.ToolAsset.GetAssetId(sAssetNo)
            If String.IsNullOrEmpty(sAssetNo) Then
                Return String.Empty
            End If

            'QRコードを作成する
            Dim sQrFileName As String = Now.ToString("yyyyMMdd_") & Path.GetRandomFileName() & ".png"
            Dim sCallPath As String = System.IO.Path.GetFullPath(HttpContext.Server.MapPath(Misc.CommonConst.QRIMAGE_FOLDERNM) & "\\" & sQrFileName)
            Dim ret = Dao.MakeQr.GetQRData(sAssetNo, sCallPath)
            If ret <> 0 Then
                'TODO エラーメッセージを表示する
                Dim errmsg As String = Dao.MakeQr.GetQRMakerMsg(ret)
                Return String.Empty
            End If

            Return sQrFileName


        End Function

#End Region

#Region "資産番号・元画面の検索条件をセッションに保存し、履歴照会画面に遷移します"
        ''' <summary>
        ''' 資産番号・元画面の検索条件をセッションに保存し、履歴照会画面に遷移します
        ''' </summary>
        <ValidateInput(False)>
        Public Function SetAssetNo(ByVal ViewModel As AssetListViewModel, ByVal SelectedAssetNo As String) As ActionResult
            'Public Sub SetAssetNo(Model As AssetListViewModel)
            Session.Remove(CommonSessionKey.CREAGE_ASSET_DETAIL_KEY)
            Session(CommonSessionKey.CREAGE_ASSET_DETAIL_KEY) = SelectedAssetNo

            '検索条件をセッションに保存
            Session(CommonSessionKey.CREAGE_ASSET_SERCH_KEY) = ViewModel

            'リダイレクト情報作成
            '--2019/11/14 nv miyaji upd start セッション情報を取得し、if文追加
            'Dim info As RedirectAssetListInfo
            Dim info As RedirectAssetListInfo = Session(RedirectAssetListInfo.ID)
            If info Is Nothing OrElse info.RedirectPage <> RedirectAssetListInfo.RedirectPageType.StockList Then
                '遷移情報セッションクリア
                If Session(RedirectAssetListInfo.ID) IsNot Nothing Then
                    Session.Remove(RedirectAssetListInfo.ID)
                End If
                Dim info2 As New RedirectAssetListInfo
                info2.RedirectPage = RedirectAssetListInfo.RedirectPageType.AssetList
                info2.KeyValue = String.Empty

                Session(RedirectAssetListInfo.ID) = info2
            End If
            '--2019/11/14 nv miyaji upd end

            '結果表示
            Return RedirectToAction("Index", "AssetDetail")
        End Function
        'Public Sub SetAssetNo(assetNo As String)
        '    Session.Remove(CommonSessionKey.CREAGE_ASSET_DETAIL_KEY)
        '    Session(CommonSessionKey.CREAGE_ASSET_DETAIL_KEY) = assetNo

        '    '検索条件をセッションに保存

        'End Sub
#End Region

#Region "画像アップロード処理"
        ''' <summary>
        ''' 画像アップロード処理（AJax)
        ''' </summary>
        ''' <param name="NewImage">登録する画像ファイル</param>
        ''' <returns>View表示内容</returns>
        Public Function UploadImage(ByVal NewImage As HttpPostedFileBase, ByVal ASSET_PIC_FILENAME As String, ByVal ASSET_NO As String) As ActionResult
            ''ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            '_LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            'If IsNothing(_LoginInfo) Then
            '    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.LoginSession
            '    '結果表示
            '    Return Json(Me._lstErr)
            'End If

            'UPDATE Inoue ここでは、イメージ画像一時保存フォルダに保存する
            ''◆画像ファイル出力先フォルダ存在チェック
            ''メッセージ一覧・システム設定取得
            'Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            'Dim Setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Image.KeyDiv, CommonSettingKey.Image.Key1, CommonSettingKey.Image.Key2, CommonSettingKey.Image.Key3)
            'Dim sImagePath As String = Setting.DATA1
            'Dim sFullPath As String = Server.MapPath("~/" + sImagePath)
            'If Not Directory.Exists(sFullPath) Then
            '    Directory.CreateDirectory(sFullPath)
            'End If

            Dim sImagePath As String = Misc.CommonConst.IMAGETMP_FOLDERNM
            Dim sFullPath As String = Server.MapPath(CommonConst.ROOT_FOLDERNM & sImagePath)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'DELETE Inoue 前回画像の削除処理は、登録・変更ボタン押下時に行なう
            ''変更前の画像ファイルが他の資産で使われていなければ削除
            'If Not ASSET_PIC_FILENAME = String.Empty Then
            '    If Dao.ToolAsset.IsUseImg(ASSET_NO, ASSET_PIC_FILENAME) = False Then
            '        System.IO.File.Delete(sFullPath + "\" + ASSET_PIC_FILENAME)
            '    End If
            'End If

            '画像ファイルをアップロード
            Dim sImageUrl As String = Path.Combine(sFullPath, NewImage.FileName)
            NewImage.SaveAs(sImageUrl)

            ''2019/09/16 nv miyaji　コメントアウト
            ''■■■　画像ファイル名をセッションに保存
            'Session(CommonSessionKey.ASSET_IMAGEFILE_KEY) = NewImage.FileName

            Return Json(NewImage.FileName)

        End Function
#End Region

#Region "Viewモデルを資産履歴更新用のモデルにコピーする"
        ''' <summary>
        ''' Viewモデルを資産履歴更新用のモデルにコピーする
        ''' </summary>
        ''' <param name="model">Viewモデル</param>
        ''' <param name="sProcessDiv">処理区分</param>
        Private Sub CopyViewModeltoRireki(ByVal model As AssetListViewModel, sProcessDiv As String)
            Dim db As New SystemDBContext
            Dim sLocLName As String = String.Empty
            Dim sLocMName As String = String.Empty
            Dim sLocSName As String = String.Empty
            Dim sCatName As String = String.Empty
            '_HistoryModel.PROCESS_LOG_ID = ""  '自動採番
            _HistoryModel.PROCESS_DT = CInt(DateTime.Now.ToString("yyyyMMdd"))
            _HistoryModel.PROCESS_TM = DateTime.Now.ToString("HHmmssfff")
            _HistoryModel.ASSET_NO = model.ASSET_NO
            _HistoryModel.SUB_ASSET_NO = model.SUB_ASSET_NO
            _HistoryModel.ASSET_ID = model.ASSET_ID
            _HistoryModel.PROCESS_TOOL_DIV = ProcessToolDiv.Web
            _HistoryModel.PROCESS_DIV = sProcessDiv
            If model.RETIRE_TS Is Nothing Then
                _HistoryModel.LEASE_PLACE_CD = model.LEASE_PLACE_CD
                _HistoryModel.LEASE_PLACE_NAME = model.LEASE_PLACE_NAME
                _HistoryModel.REMARK = model.REMARK   '2019/11/19 nv miyaji add
                _HistoryModel.LEASE_EMPLOYEE_CD = model.LEASE_EMPLOYEE_CD
                _HistoryModel.LEASE_EMPLOYEE_NAME = model.LEASE_EMPLOYEE_NAME
                _HistoryModel.LEASE_DT = model.LEASE_DT '2020/03/06 nv miyaji add
                _HistoryModel.LEASE_DUE_DT = model.LEASE_DUE_DT
            Else
                _HistoryModel.LEASE_PLACE_CD = Nothing
                _HistoryModel.LEASE_PLACE_NAME = Nothing
                _HistoryModel.REMARK = Nothing   '2019/11/19 nv miyaji add
                _HistoryModel.LEASE_EMPLOYEE_CD = Nothing
                _HistoryModel.LEASE_EMPLOYEE_NAME = Nothing
                _HistoryModel.LEASE_DT = Nothing '2020/03/06 nv miyaji add
                _HistoryModel.LEASE_DUE_DT = Nothing
            End If
            _HistoryModel.LEASE_STATUS = model.LEASE_STATUS   '2021/10/04 nv miyaji
            _HistoryModel.MNG_LOC_L_CD = model.MNG_LOC_L_CD   '2021/10/04 nv miyaji
            _HistoryModel.LOC_L_CD = model.LOC_L_CD
            _HistoryModel.LOC_M_CD = model.LOC_M_CD
            _HistoryModel.LOC_S_CD = model.LOC_S_CD
            If Not String.IsNullOrEmpty(model.LOC_L_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationL, model.LOC_L_CD, " ", " "}
                Dim locamodel As LocationModel = db.Location.Find(findKeys)
                sLocLName = locamodel.LOC_NAME
            End If
            If Not String.IsNullOrEmpty(model.LOC_M_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationM, model.LOC_L_CD, model.LOC_M_CD, " "}
                Dim locamodel As LocationModel = db.Location.Find(findKeys)
                sLocMName = locamodel.LOC_NAME
            End If
            If Not String.IsNullOrEmpty(model.LOC_S_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationS, model.LOC_L_CD, model.LOC_M_CD, model.LOC_S_CD}
                Dim locamodel As LocationModel = db.Location.Find(findKeys)
                sLocSName = locamodel.LOC_NAME
            End If
            _HistoryModel.LOC_L_NAME = sLocLName
            _HistoryModel.LOC_M_NAME = sLocMName
            _HistoryModel.LOC_S_NAME = sLocSName
            If Not String.IsNullOrEmpty(model.CAT_L_CD.Trim) Then
                'IDをキーにカテゴリーモデルを取得する
                Dim findKeys As String() = {Cat_DIV.CategoryL, model.CAT_L_CD, " ", " "}
                Dim catmodel As CategoryModel = db.Category.Find(findKeys)
                sCatName = catmodel.CAT_NAME
            End If
            _HistoryModel.CAT_L_CD = model.CAT_L_CD
            _HistoryModel.CAT_NAME = sCatName
            _HistoryModel.ASSET_NAME = model.ASSET_NAME
            _HistoryModel.MAKER = model.MAKER
            _HistoryModel.PRODUCT_NO = model.PRODUCT_NO
            _HistoryModel.MODEL_NO = model.MODEL_NO
            _HistoryModel.REMARK1 = model.REMARK1
            _HistoryModel.REMARK2 = model.REMARK2
            _HistoryModel.REMARK3 = model.REMARK3
            _HistoryModel.REMARK4 = model.REMARK4
            _HistoryModel.REMARK5 = model.REMARK5
            _HistoryModel.REMARK6 = model.REMARK6
            _HistoryModel.REMARK7 = model.REMARK7
            _HistoryModel.REMARK8 = model.REMARK8
            _HistoryModel.REMARK9 = model.REMARK9
            _HistoryModel.REMARK10 = model.REMARK10
            _HistoryModel.TASK_RESULT = TaskResult.OK
        End Sub
#End Region

#Region "モデルを資産履歴更新用のモデルにコピーする"
        ''' <summary>
        ''' モデルを資産履歴更新用のモデルにコピーする
        ''' </summary>
        ''' <param name="model">Viewモデル</param>
        ''' <param name="sProcessDiv">処理区分</param>
        Private Sub CopyModeltoRireki(ByVal model As AssetListModel, sProcessDiv As String)
            Dim db As New SystemDBContext
            Dim sLocLName As String = String.Empty
            Dim sLocMName As String = String.Empty
            Dim sLocSName As String = String.Empty
            '_HistoryModel.PROCESS_LOG_ID = ""  '自動採番
            _HistoryModel.PROCESS_DT = CInt(DateTime.Now.ToString("yyyyMMdd"))
            _HistoryModel.PROCESS_TM = DateTime.Now.ToString("HHmmssfff")
            _HistoryModel.ASSET_NO = model.ASSET_NO
            _HistoryModel.PROCESS_TOOL_DIV = ProcessToolDiv.Web
            If model.RETIRE_TS Is Nothing Then
                _HistoryModel.ASSET_ID = Nothing
                _HistoryModel.PROCESS_DIV = ProcessDiv.MR
                _HistoryModel.LEASE_PLACE_CD = Nothing
                _HistoryModel.LEASE_PLACE_NAME = Nothing
                _HistoryModel.REMARK = Nothing   '2019/11/19 nv miyaji add
                _HistoryModel.LEASE_EMPLOYEE_CD = Nothing
                _HistoryModel.LEASE_EMPLOYEE_NAME = Nothing
                _HistoryModel.LEASE_DT = Nothing '2020/03/06 nv miyaji add
                _HistoryModel.LEASE_DUE_DT = Nothing
            Else
                _HistoryModel.ASSET_ID = model.ASSET_ID
                _HistoryModel.PROCESS_DIV = sProcessDiv
                _HistoryModel.LEASE_PLACE_CD = model.LEASE_PLACE_CD
                _HistoryModel.LEASE_PLACE_NAME = model.LEASE_PLACE_NAME
                _HistoryModel.REMARK = model.REMARK   '2019/11/19 nv miyaji add
                _HistoryModel.LEASE_EMPLOYEE_CD = model.LEASE_EMPLOYEE_CD
                _HistoryModel.LEASE_EMPLOYEE_NAME = model.LEASE_EMPLOYEE_NAME
                _HistoryModel.LEASE_DT = model.LEASE_DT '2020/03/06 nv miyaji add
                _HistoryModel.LEASE_DUE_DT = model.LEASE_DUE_DT
            End If
            _HistoryModel.LEASE_STATUS = model.LEASE_STATUS   '2021/10/04 nv miyaji
            _HistoryModel.MNG_LOC_L_CD = model.MNG_LOC_L_CD   '2021/10/04 nv miyaji
            _HistoryModel.LOC_L_CD = model.LOC_L_CD
            _HistoryModel.LOC_M_CD = model.LOC_M_CD
            _HistoryModel.LOC_S_CD = model.LOC_S_CD
            If Not String.IsNullOrEmpty(model.LOC_L_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationL, model.LOC_L_CD, " ", " "}
                Dim locamodel As LocationModel = db.Location.Find(findKeys)
                sLocLName = locamodel.LOC_NAME
            End If
            If Not String.IsNullOrEmpty(model.LOC_M_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationM, model.LOC_L_CD, model.LOC_M_CD, " "}
                Dim locamodel As LocationModel = db.Location.Find(findKeys)
                sLocMName = locamodel.LOC_NAME
            End If
            If Not String.IsNullOrEmpty(model.LOC_S_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationS, model.LOC_L_CD, model.LOC_M_CD, model.LOC_S_CD}
                Dim locamodel As LocationModel = db.Location.Find(findKeys)
                sLocSName = locamodel.LOC_NAME
            End If
            _HistoryModel.LOC_L_NAME = sLocLName
            _HistoryModel.LOC_M_NAME = sLocMName
            _HistoryModel.LOC_S_NAME = sLocSName
            _HistoryModel.ASSET_NAME = model.ASSET_NAME
            _HistoryModel.MAKER = model.MAKER
            _HistoryModel.PRODUCT_NO = model.PRODUCT_NO
            _HistoryModel.MODEL_NO = model.MODEL_NO
            _HistoryModel.REMARK1 = model.REMARK1
            _HistoryModel.REMARK2 = model.REMARK2
            _HistoryModel.REMARK3 = model.REMARK3
            _HistoryModel.REMARK4 = model.REMARK4
            _HistoryModel.REMARK5 = model.REMARK5
            _HistoryModel.REMARK6 = model.REMARK6
            _HistoryModel.REMARK7 = model.REMARK7
            _HistoryModel.REMARK8 = model.REMARK8
            _HistoryModel.REMARK9 = model.REMARK9
            _HistoryModel.REMARK10 = model.REMARK10
            _HistoryModel.TASK_RESULT = TaskResult.OK
        End Sub
#End Region

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】IsHex（16進数に変換できるかをチェックする）"
'#Region "16進数に変換できるかをチェックする"
        '''' <summary>
        '''' 16進数に変換できるかをチェックする
        '''' </summary>
        '''' <param name="str">チェックする文字列</param>
        'Private Function IsHex(str As String) As Boolean
        '    Dim IsError As Boolean = False
        '    For Each Chr As Char In str
        '        If Not Uri.IsHexDigit(Chr) Then
        '            IsError = True
        '            Exit For
        '        End If
        '    Next
        '    Return IsError
        'End Function
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】GetCsvMap（CSVマッピングマスタを取得する）"
'#Region "CSVマッピングマスタを取得する"
        '''' <summary>
        '''' CSVマッピングマスタを取得する
        '''' </summary>
        'Private Function GetCsvMap() As String
        '    Dim tbl As New DataTable
        '    Dim sCharCd As String = String.Empty

        '    Dim sbSQL As StringBuilder = New StringBuilder
        '    Dim clsDB As clsSV_DB = New clsSV_DB
        '    If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
        '        '接続失敗
        '        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        '    End If
        '    Try
        '        'M_CSVMAP_DETAILテーブルから取得するSQLを作成
        '        sbSQL.AppendLine("SELECT CHAR_CD")
        '        sbSQL.AppendLine("FROM")
        '        sbSQL.AppendLine("M_CSVMAP")
        '        sbSQL.AppendLine("WHERE")
        '        sbSQL.AppendLine("CSV_DIV = '" & CsvDiv.Asset & "'")

        '        tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

        '        If Not IsNothing(tbl) Then
        '            sCharCd = tbl.Rows(0).Item("CHAR_CD")
        '        End If

        '    Catch ex As Exception
        '        logger.Error(ex.Message)
        '        logger.Error(sbSQL)
        '        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        '    Finally
        '        'DB CLOSE
        '        clsDB.SVSubCloseConnection()
        '    End Try

        '    Return sCharCd
        'End Function
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】GetCsvMapDetail（CSVマッピング明細マスタを取得する）"
'#Region "CSVマッピング明細マスタを取得する"
        '''' <summary>
        '''' CSVマッピング明細マスタを取得する
        '''' </summary>
        'Private Function GetCsvMapDetail() As DataTable
        '    Dim tbl As New DataTable

        '    Dim sbSQL As StringBuilder = New StringBuilder
        '    Dim clsDB As clsSV_DB = New clsSV_DB
        '    If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
        '        '接続失敗
        '        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        '    End If
        '    Try
        '        'M_CSVMAP_DETAILテーブルから取得するSQLを作成
        '        sbSQL.Append("SELECT ")
        '        sbSQL.Append("  CSV_DIV ")
        '        sbSQL.Append("  , CSV_NO ")
        '        sbSQL.Append("  , TOOL_DIV ")
        '        sbSQL.Append("  , WORD_DIV ")
        '        sbSQL.Append("  , WORD_CD ")
        '        sbSQL.Append("  , REQUIRED_ITEM_FLAG ")
        '        sbSQL.Append("  , SPECIFIED_DIV ")
        '        sbSQL.Append("  , SPEC_CAN_DIV ")        '20230203 ADD Shirakawa
        '        sbSQL.Append("  , CSV_COL_NO ")
        '        sbSQL.Append("  , CSV_FIX_VAL ")
        '        sbSQL.Append("  , INS_DATETIME ")
        '        sbSQL.Append("  , INS_EMPLOYEE_CD ")
        '        sbSQL.Append("  , INS_EMPLOYEE_NAME ")
        '        sbSQL.Append("  , UPD_DATETIME ")
        '        sbSQL.Append("  , UPD_EMPLOYEE_CD ")
        '        sbSQL.Append("  , UPD_EMPLOYEE_NAME ")
        '        sbSQL.Append("  , UPD_TASK ")
        '        sbSQL.Append("   ,TOOL_DIV + WORD_DIV + WORD_CD AS RenketsuWordCd ")
        '        sbSQL.Append("FROM ")
        '        sbSQL.Append("  M_CSVMAP_DETAIL ")
        '        sbSQL.Append("WHERE ")
        '        sbSQL.Append("CSV_DIV = '" & CsvDiv.Asset & "'")

        '        sbSQL.Append(" ORDER BY CSV_DIV,CSV_NO")

        '        tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

        '        Return tbl

        '    Catch ex As Exception
        '        logger.Error(ex.Message)
        '        logger.Error(sbSQL)
        '        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        '    Finally
        '        'DB CLOSE
        '        clsDB.SVSubCloseConnection()
        '    End Try

        '    Return tbl
        'End Function
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】IsCsvMapMeiKoumokuOK（CSVマッピング明細マスタの項目チェック）"
'#Region "CSVマッピング明細マスタの項目チェック"
        '''' <summary>
        '''' CSVマッピング明細マスタの項目チェック
        '''' </summary>
        '''' <param name="tblCsvMapMei">CSVマッピング明細マスタのデータテーブル</param>
        'Private Function IsCsvMapMeiKoumokuOK(tblCsvMapMei As DataTable) As Boolean
        '    _lstCsvHed = New List(Of String)
        '    '資産マスタのCSVマッピング明細の項目定義を取得
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.Activity) '2023/02/03 ADD Shiirakawa
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.AssetNo)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.AssetID)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.AssetDiv)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.AssetNm)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.LocLCd)
        '    '_lstCsvHed.Add(CsvMapMei_Asset.CharCd.LocLNm)  '2019/12/26 nv miyaji
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.LocMCd)
        '    '_lstCsvHed.Add(CsvMapMei_Asset.CharCd.LocMNm)  '2019/12/26 nv miyaji
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.LocSCd)
        '    '_lstCsvHed.Add(CsvMapMei_Asset.CharCd.LocSNm)  '2019/12/26 nv miyaji
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.CatLCd)
        '    '_lstCsvHed.Add(CsvMapMei_Asset.CharCd.CatLNm)  '2019/12/26 nv miyaji
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.CatMCd)
        '    '_lstCsvHed.Add(CsvMapMei_Asset.CharCd.CatNm)  '2019/12/26 nv miyaji
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.CatSCd)
        '    '_lstCsvHed.Add(CsvMapMei_Asset.CharCd.CatSNm)  '2019/12/26 nv miyaji
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.Maker)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.ProductNo)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.ModelNo)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.LeaseNo)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.LeaseLimit)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.GenLimit)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem1)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem2)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem3)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem4)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem5)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem6)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem7)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem8)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem9)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem10)
        '    '2023/02/03 ADD Start Shirakawa
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem11)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem12)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem13)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem14)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem15)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem16)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem17)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem18)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.FreeItem19)
        '    '2023/02/03 ADD End Shirakawa
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.AssetPicFileNm)
        '    '--2021/10/04 nv miyaji add start
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.MngLocLCd)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.LeaseStatus)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.MainteLeaseNum)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.MainteLeaseDay)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.MainteLeaseDayWorkFlg)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.MainteDay)
        '    _lstCsvHed.Add(CsvMapMei_Asset.CharCd.MainteDayWorkFlg)
        '    '--2021/10/04 nv miyaji add end


        '    '項目数チェック
        '    If _lstCsvHed.Count <> tblCsvMapMei.Rows.Count Then
        '        Return False
        '    End If

        '    '項目値チェック
        '    For icnt As Integer = 0 To _lstCsvHed.Count - 1
        '        Dim row As DataRow()
        '        row = tblCsvMapMei.Select("RenketsuWordCd = '" & _lstCsvHed(icnt).ToString & "'")
        '        If row.Count = 0 Then
        '            '該当項目がない場合、エラー
        '            Return False
        '        End If
        '    Next

        '    Return True
        'End Function
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】ReadCsvFile（指定したCSVファイルからデータを読み込み、登録する）"
'#Region "指定したCSVファイルからデータを読み込み、登録する"
        '''' <summary>
        '''' 指定したCSVファイルからデータを読み込み、登録する
        '''' </summary>
        '''' <param name="path">読み取るファイルのパス</param>
        '''' <param name="enc">エンコード</param>
        '''' <param name="isKomokusuuErr">項目数エラーかどうか</param>
        '''' <param name="tblCsvMapMei">CSVマッピング明細マスタのデータテーブル</param>
        '''' <param name="sTmpTblNm">登録するデータベースのテーブル名</param>
        '''' <param name="db">db</param>
        '''' <returns>true:正常終了 false:エラー：</returns>
        '''' <remarks></remarks>
        'Public Function ReadCsvFile(ByVal path As String, ByVal enc As System.Text.Encoding, ByRef isKomokusuuErr As Boolean, tblCsvMapMei As DataTable, sTmpTblNm As String, db As SystemDBContext) As Boolean

        '    Dim blErr As Boolean = False 'エラーがあるかどうか
        '    Dim sline As String = ""    'CSVファイル読込行データ格納用

        '    'CSV登録用データテーブル作成
        '    CreatetblImportCsv()

        '    'カテゴリマスタデータ取得
        '    Dim CategoryData As DataTable
        '    CategoryData = Dao.ToolCategory.GetCategoryDataAll()

        '    'ロケーションマスタデータ取得
        '    Dim LocaData As DataTable
        '    LocaData = Dao.ToolLocation.GetLocationDataAll()

        '    '資産マスタデータ取得     Add Inoue 2019/12/18
        '    Dim tblAsset As DataTable
        '    tblAsset = Dao.ToolAsset.GetAssetDataAll()

        '    '重複チェック用変数（ハッシュテーブルに変更）     Update Inoue
        '    'Dim lstSumiCd As List(Of String) = New List(Of String)     '資産番号重複チェック用
        '    'Dim lstIDSumiCd As List(Of String) = New List(Of String)   '資産ID重複チェック用
        '    Dim htSumiCd As Hashtable = New Hashtable                   '資産番号重複チェック用（Key:資産番号 Value:資産ID）
        '    Dim htIDSumiCd As Hashtable = New Hashtable                 '資産ID重複チェック用（Key:資産ID Value:資産番号）

        '    'CSVマッピング明細マスタで列番指定の項目数取得
        '    Dim Colrows() As DataRow = tblCsvMapMei.Select("CSV_COL_NO = MAX(CSV_COL_NO) ")
        '    Dim iColNoCnt As Integer = CInt(Colrows(0).Item("CSV_COL_NO"))
        '    'Dim Colrows() As DataRow = tblCsvMapMei.Select("SPECIFIED_DIV = '" & CsvMapping_SpecifiedDiv.Index & "'")
        '    'Dim iColNoCnt As Integer = Colrows.Length

        '    '2023/02/08 Kawamoto ADD START
        '    '登録/更新可能な拠点情報を取得
        '    Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)
        '    Dim lstAuthLocation As List(Of String) = Dao.ToolCommon.GetAuthLocation(_LoginInfo, AuthInfo)
        '    '2023/02/08 Kawamoto ADD END

        '    Dim CsvRegistAll As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Asset_CSV_Regist_ALL.KeyDiv,
        '                                                                 CommonSettingKey.Asset_CSV_Regist_ALL.Key1,
        '                                                                 CommonSettingKey.Asset_CSV_Regist_ALL.Key2,
        '                                                                 CommonSettingKey.Asset_CSV_Regist_ALL.Key3, True)

        '    '資産番号の自動生成設定を取得
        '    Dim AssetNoSelfReg As SettingInfo = _Message.getSettingInfo(CommonSettingKey.AssetNo_Self_Regist.KeyDiv,
        '                                                          CommonSettingKey.AssetNo_Self_Regist.Key1,
        '                                                          CommonSettingKey.AssetNo_Self_Regist.Key2,
        '                                                          CommonSettingKey.AssetNo_Self_Regist.Key3, True)

        '    Dim assetNoSeq As Integer = 0                               '資産番号 自動生成連番
        '    Dim assetNoAutoRegFlg As Boolean = False                    '資産新規登録フラグ

        '    'ファイルから読み込み
        '    Using sr As StreamReader = New StreamReader(path, enc)
        '        Dim iRowCnt As Integer = 0  '行カウント用
        '        '１行取得
        '        sline = sr.ReadLine()
        '        Do Until sline Is Nothing
        '            If iRowCnt = 0 Then
        '                iRowCnt += 1
        '                sline = sr.ReadLine()
        '                Continue Do
        '            Else
        '                iRowCnt += 1
        '            End If

        '            ''●●●
        '            'If (iRowCnt Mod 10000) = 0 Then
        '            '    logger.Info("STEP3 " & (iRowCnt + 1).ToString & "  " & Now.ToString("yyyy/MM/dd HH:mm:ss"))
        '            'End If

        '            'エラー件数が１０００件超えていたら以降処理をしない  '2019/12/20 nv miyaji
        '            If Me._lstErr.Count > CommonConst.CSV_ERROR_MAXCOUNT Then
        '                Exit Do
        '            End If

        '            '--2021/10/06 nv miyaji update start
        '            '--行を分解しString配列にする。
        '            Dim fields As String() = New String() {}
        '            If sline.Length > 0 Then
        '                fields = clsSV_CsvFile.ReadCsvLineToArray(sline)
        '            End If
        '            'Dim fields As String() = clsSV_CsvFile.ReadCsvLineToArray(sline)
        '            '--2021/10/06 nv miyaji update end

        '            '項目数チェック
        '            If fields.Length < iColNoCnt Then
        '                'If fields.Length <> iColNoCnt Then
        '                isKomokusuuErr = True
        '                Return blErr
        '            End If

        '            _dtCsvImport.Clear()    'データテーブルクリア
        '            'データ行追加
        '            Dim newrow As DataRow = _dtCsvImport.NewRow
        '            For iCol As Integer = 0 To _lstCsvHed.Count - 1
        '                Dim sStr As String = String.Empty
        '                Dim rows As DataRow() = tblCsvMapMei.Select("RenketsuWordCd = '" & _lstCsvHed(iCol).ToString & "'")
        '                If rows(0).Item("SPECIFIED_DIV") = CsvMapping_SpecifiedDiv.Index Then
        '                    If CommonRfid.ToStringNvl(rows(0).Item("CSV_COL_NO")).Trim = String.Empty OrElse CInt(CommonRfid.ToStringNvl(rows(0).Item("CSV_COL_NO")).Trim) = 0 Then
        '                        sStr = String.Empty
        '                    Else
        '                        sStr = fields(CInt(rows(0).Item("CSV_COL_NO")) - 1).Trim.TrimStart("'"c)
        '                    End If
        '                ElseIf rows(0).Item("SPECIFIED_DIV") = CsvMapping_SpecifiedDiv.Kotei Then
        '                    sStr = tblCsvMapMei(iCol).Item("CSV_FIX_VAL").ToString
        '                End If
        '                newrow(iCol) = sStr
        '            Next
        '            _dtCsvImport.Rows.Add(newrow)

        '            '資産番号の自動生成フラグを初期化
        '            assetNoAutoRegFlg = False

        '            '項目値のエラーチェック
        '            '※assetNoAutoRegFlg, assetNoSeqはIsDataErrorLine内で値を設定
        '            If Not IsDataErrorLine(iRowCnt, tblAsset, CategoryData, LocaData, htSumiCd, htIDSumiCd, lstAuthLocation, AssetNoSelfReg, assetNoAutoRegFlg, assetNoSeq) Then
        '                'エラーの場合、登録処理は行わない
        '                sline = sr.ReadLine()
        '                blErr = True
        '                Continue Do
        '            End If

        '            '登録処理
        '            If Not blErr Then
        '                Try
        '                    InsertWork(iRowCnt, sTmpTblNm, db, assetNoAutoRegFlg, assetNoSeq)
        '                Catch ex As Exception
        '                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        '                End Try
        '            End If

        '            sline = sr.ReadLine()
        '        Loop

        '        _iCsvImportRowCount = iRowCnt - 1   '2020/03/16 nv miyaji　件数。操作ログ出力用。

        '        'データ項目にエラーがあった場合
        '        If blErr Then
        '            Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG

        '            '最終行が空白行であれば削除する。
        '            If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = 0 Then
        '                Dim lstRow As DataInportResult = New DataInportResult
        '                Me._lstErr.RemoveAt(Me._lstErr.Count - 1)
        '            End If
        '        End If

        '    End Using

        '    Return blErr

        'End Function
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】IsDataErrorLine（項目値のエラーチェック）"
'#Region "項目値のエラーチェック"
        '''' <summary>
        '''' 項目値のエラーチェック
        '''' </summary>
        '''' <param name="iRowCnt">データ行</param>
        '''' <param name="tblAsset">資産マスタのデータテーブル</param>
        '''' <param name="CategoryData">カテゴリーマスタのデータテーブル</param>
        '''' <param name="LocaData">ロケーションマスタのデータテーブル</param>
        '''' <param name="htSumiCd">資産番号重複チェック用ハッシュテーブル</param>
        '''' <param name="htIDSumiCd">資産ID重複チェックハッシュテーブル</param>
        '''' <param name="lstAuthLocation">登録/更新可能な拠点リスト</param>
        '''' <param name="AssetNoSelfReg">設定M資産番号 自動生成設定</param>
        '''' <param name="assetNoAutoRegFlg">資産番号自動生成フラグ</param>
        '''' <param name="assetNoSeq">資産番号 自動生成連番値</param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Private Function IsDataErrorLine(ByVal iRowCnt As Integer,
        '                                 ByVal tblAsset As DataTable,               'Add Inoue 2019/12/18
        '                                 ByVal CategoryData As DataTable,
        '                                 ByVal LocaData As DataTable,
        '                                 ByVal htSumiCd As Hashtable,               'Add Inoue 2019/12/18
        '                                 ByVal htIDSumiCd As Hashtable,             'Add Inoue 2019/12/18
        '                                 ByVal lstAuthLocation As List(Of String),  '2023/02/08 Kawamoto ADD
        '                                 ByVal AssetNoSelfReg As SettingInfo,
        '                                 ByRef assetNoAutoRegFlg As Boolean,
        '                                 ByRef assetNoSeq As Integer
        '                                 ) As Boolean

        '    Dim blRet As Boolean = True

        '    Dim db As New SystemDBContext

        '    'オブジェクトの編集開始
        '    Me._dtCsvImport.Rows(0).BeginEdit()

        '    '--データチェック

        '    'For iRowCnt As Integer = 0 To _lstCsvMapMei.Count - 1
        '    'IDをキーにモデルを取得する
        '    'Dim model As AssetListModel = db.AssetList.Find(_lstCsvMapMei(CsvMapMei_Asset.ColIdx.AssetNo))

        '    'Datatable より、検索する  2019/12/18
        '    'Dim model As AssetListModel = db.AssetList.Find(Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetNo))
        '    Dim rowsAsset() As DataRow = tblAsset.Select("ASSET_NO = '" & Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetNo) & "'")

        '    '既存DBデータ区分をセット     ADD  Inoue 2019/12/18
        '    Dim enumDbDataKbn As DbDataKbn
        '    If rowsAsset.Count = 0 Then
        '        enumDbDataKbn = DbDataKbn.Non
        '    Else
        '        If rowsAsset(0).Item("ASSET_STATUS").ToString.Trim = Misc.ASSET_STATUS.Removed Then
        '            enumDbDataKbn = DbDataKbn.Removed
        '        Else
        '            enumDbDataKbn = DbDataKbn.Exist
        '        End If
        '    End If

        '    '既存(除却)がある時、変更フラグをtrueにする 2019/10/15 miyaji add → 既存DBデータ区分で判断 2019/12/18
        '    'If Not model Is Nothing AndAlso Not model.RETIRE_TS Is Nothing Then
        '    If enumDbDataKbn = DbDataKbn.Removed Then
        '        Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '    End If

        '    '該当のカテゴリーマスタデータを取得する
        '    Dim CatLrow As DataRow() = Nothing  '大分類
        '    If Not Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatLCd).ToString.Trim = String.Empty Then
        '        CatLrow = CategoryData.Select("CAT_L_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatLCd).ToString.Trim + "' AND CAT_DIV = '" + CAT_DIV.CategoryL + "'")
        '    End If
        '    Dim CatMrow As DataRow() = Nothing  '中分類
        '    If Not Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatMCd).ToString.Trim = String.Empty AndAlso Not CatLrow Is Nothing AndAlso CatLrow.Length > 0 Then
        '        CatMrow = CategoryData.Select("CAT_L_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatLCd).ToString.Trim + "' AND CAT_M_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatMCd).ToString.Trim + "' AND CAT_DIV = '" + CAT_DIV.CategoryM + "'")
        '    End If
        '    '該当のロケーションマスタデータを取得する
        '    Dim LocLrow As DataRow() = Nothing  '大区分
        '    If Not Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocLCd).ToString.Trim = String.Empty Then
        '        LocLrow = LocaData.Select("LOC_L_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocLCd).ToString.Trim + "' AND LOC_DIV = '" + LOC_DIV.LocationL + "'")
        '    End If
        '    Dim LocMrow As DataRow() = Nothing  '中区分
        '    If Not Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocMCd).ToString.Trim = String.Empty AndAlso Not LocLrow Is Nothing AndAlso LocLrow.Length > 0 Then
        '        LocMrow = LocaData.Select("LOC_L_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocLCd).ToString.Trim + "' AND LOC_M_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocMCd).ToString.Trim + "' AND LOC_DIV = '" + LOC_DIV.LocationM + "'")
        '    End If
        '    '該当の管理ロケーションマスタデータを取得する '2021/10/04 nv miyaji
        '    Dim MngLocLrow As DataRow() = Nothing  '大区分
        '    If Not Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocLCd).ToString.Trim = String.Empty Then
        '        MngLocLrow = LocaData.Select("LOC_L_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MngLocLCd).ToString.Trim + "' AND LOC_DIV = '" + LOC_DIV.LocationL + "'")
        '    End If

        '    '2023/02/08 Kawamoto ADD START
        '    '設定マスタ（資産CSVファイル取込全件対象）の取得
        '    Dim CsvRegistAll As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Asset_CSV_Regist_ALL.KeyDiv,
        '                                                                 CommonSettingKey.Asset_CSV_Regist_ALL.Key1,
        '                                                                 CommonSettingKey.Asset_CSV_Regist_ALL.Key2,
        '                                                                 CommonSettingKey.Asset_CSV_Regist_ALL.Key3, True)

        '    '設定マスタ（薬品管理機能）の取得
        '    Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
        '                                                              CommonSettingKey.Drugs_Function.Key1,
        '                                                              CommonSettingKey.Drugs_Function.Key2,
        '                                                              CommonSettingKey.Drugs_Function.Key3, True)
        '    '2023/02/08 Kawamoto ADD END

        '    '内容量(kg)未入力フラグ  '2023/6/19 Sato ADD
        '    Dim internalCapEmptyFlg As Boolean = False

        '    Dim AssetNo As String = Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetNo).ToString.Trim
        '    Dim shoriKbn As String = Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.Activity).ToString.Trim            '処理区分を事前に取得
        '    For iCalCnt As Integer = 0 To _lstCsvHed.Count - 1

        '        '新規行がなければ追加する
        '        If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
        '            Dim lstRow As DataInportResult = New DataInportResult
        '            Me._lstErr.Add(lstRow)
        '        End If

        '        '処理区分が削除または除却の場合
        '        If shoriKbn = CsvImportProcDiv.Delete OrElse shoriKbn = CsvImportProcDiv.Retirement Then

        '            '項目が処理区分と資産番号以外
        '            If Not (iCalCnt = CsvMapMei_Asset.ColIdx.Activity OrElse iCalCnt = CsvMapMei_Asset.ColIdx.AssetNo) Then
        '                'チェックをしない
        '                Continue For
        '            End If
        '        End If


        '        '列によってチェック条件・設定項目値を分ける
        '        Dim sStr As String = Me._dtCsvImport.Rows(0).Item(iCalCnt).ToString.Trim
        '        Select Case iCalCnt
        '            '2023/02/08 Kawamoto ADD START
        '            Case CsvMapMei_Asset.ColIdx.Activity

        '                '処理区分
        '                '資産CSVファイル取込全件対象が「差分取込」の場合
        '                If CsvRegistAll.DATA1 = AssetCSVRegistAll.Difference Then
        '                    '空白チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_DIV)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                        '区分値チェック
        '                    ElseIf sStr <> CsvImportProcDiv.Insert AndAlso sStr <> CsvImportProcDiv.Update AndAlso sStr <> CsvImportProcDiv.Delete AndAlso sStr <> CsvImportProcDiv.Retirement Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_DIV)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
        '                        blRet = False
        '                    End If
        '                End If
        '            '2023/02/08 Kawamoto ADD END

        '            Case CsvMapMei_Asset.ColIdx.AssetNo

        '                '資産番号
        '                If AssetNoSelfReg.DATA1 = ValidFlg.Cd.Valid Then
        '                    '自動生成設定が有効
        '                    If sStr <> "" AndAlso CsvRegistAll.DATA1 = AssetCSVRegistAll.All Then
        '                        '資産番号入力済み　かつ　全件取込
        '                        Dim model As AssetListModel = db.AssetList.Find(sStr)
        '                        If IsNothing(model) Then
        '                            'DBに存在しない
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXISTS_ASSET_NO)
        '                            blRet = False
        '                        End If
        '                    ElseIf sStr <> "" AndAlso CsvRegistAll.DATA1 = AssetCSVRegistAll.Difference AndAlso _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.Activity).ToString.Trim = CsvImportProcDiv.Insert Then
        '                        '資産番号入力済み　かつ　差分取込 かつ　処理区分：I(追加)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_AUTO_GENERATE_NOT_INPUT)
        '                        blRet = False
        '                    ElseIf sStr = "" Then
        '                        '自動生成が必要かチェック
        '                        '※以下の場合に自動生成ありとなる
        '                        '1. 全件取込　かつ　資産番号が未入力
        '                        '2. 差分取込　かつ　処理区分：I(追加)　かつ　資産番号が未入力
        '                        assetNoAutoRegFlg = (CsvRegistAll.DATA1 = AssetCSVRegistAll.All OrElse
        '                                            (CsvRegistAll.DATA1 = AssetCSVRegistAll.Difference AndAlso
        '                                            _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.Activity).ToString.Trim = CsvImportProcDiv.Insert))

        '                        If assetNoAutoRegFlg Then
        '                            '自動生成あり
        '                            If assetNoSeq <= 0 Then
        '                                '連番未採番なら採番する
        '                                assetNoSeq = AssetNoAutoNumberingSeq()
        '                            Else
        '                                '連番を加算
        '                                assetNoSeq += 1
        '                            End If

        '                            If assetNoSeq >= DataInport.ASSET_NO_AUTO_REG_MAX_SIZE Then
        '                                '資産番号生成上限
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO)
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_AUTO_GENERATE_MAX_SIZE)
        '                                blRet = False
        '                            End If

        '                            '自動生成される場合は以降のチェックは実施なし
        '                            Continue For
        '                        End If
        '                    End If
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                    blRet = False
        '                ElseIf sStr.Length > itemlength.AssetNo Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                ElseIf Not CommonRfid.IsAssetNo(sStr) Then      '2022/8/31 Upd Kawamoto
        '                    '半角英数字チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU_ASSET_NO)     '2022/9/13 Upd Kawamoto
        '                    blRet = False
        '                ElseIf htSumiCd.ContainsKey(sStr) Then      'Update Inoue 2019/12/18
        '                    '重複チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & "[{0}]", sStr)
        '                    blRet = False
        '                    '資産CSVファイル取込全件対象が「差分取込」の場合
        '                ElseIf CsvRegistAll.DATA1 = AssetCSVRegistAll.Difference Then
        '                    '資産番号存在チェック
        '                    Dim model As AssetListModel = db.AssetList.Find(sStr)

        '                    Select Case Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.Activity).ToString.Trim
        '                        '処理区分：I(追加)の場合
        '                        Case CsvImportProcDiv.Insert
        '                            '資産番号が存在している場合エラー
        '                            If Not IsNothing(model) Then
        '                                If AssetNoSelfReg.DATA1 <> ValidFlg.Cd.Valid Then       ' 資産番号が自動生成ではない場合、エラー判定する
        '                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO)
        '                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & "[{0}]", sStr)
        '                                    blRet = False
        '                                End If
        '                            End If
        '                        '処理区分：U(更新)、D(削除)、R(除却)の場合
        '                        Case CsvImportProcDiv.Update, CsvImportProcDiv.Delete, CsvImportProcDiv.Retirement
        '                            '資産番号が存在しない場合エラー
        '                            If IsNothing(model) Then
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO)
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXISTS_ASSET_NO)
        '                                blRet = False
        '                            End If
        '                    End Select
        '                Else
        '                    '資産番号重複チェック用ハッシュテーブルに追加（value には、一旦、資産IDを入れる）   'Update Inoue 2019/12/18
        '                    htSumiCd.Add(sStr, Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID).ToString.Trim)
        '                End If

        '            Case CsvMapMei_Asset.ColIdx.AssetID
        '                'カテゴリ大分類チェック
        '                If CatLrow Is Nothing OrElse CatLrow.Length = 0 Then
        '                    '該当のカテゴリマスタデータが取得できなかった場合、エラー。エラーはカテゴリ大分類チェック時にセット。
        '                    Continue For
        '                End If

        '                'If model Is Nothing OrElse Not model.RETIRE_TS Is Nothing Then     既存DBデータ区分で判断 2019/12/18
        '                If enumDbDataKbn = DbDataKbn.Non Or enumDbDataKbn = DbDataKbn.Removed Then
        '                    '●データベースにない、もしくは、既存データが除却の場合

        '                    ''2021/10/06 nv miyaji カテゴリーがない場合の対応は既存データがある場合にも必要なので、IF文の外に移動
        '                    'If CatLrow Is Nothing OrElse CatLrow.Length = 0 Then
        '                    '    '該当のカテゴリマスタデータが取得できなかった場合、エラー。エラーはカテゴリ大区分チェック時にセット。
        '                    '    Continue For
        '                    'End If

        '                    '資産ＩＤを生成する
        '                    'Dim sGenAssetID As String = String.Empty '2019/10/24 nv miyaji コメントアウト
        '                    Select Case CInt(CatLrow(0).Item("ASSET_ID_REG_DIV"))
        '                        Case AssetIDGenerationMethod.None
        '                            '★★方法①　資産IDは生成せずタグのEPCエンコード値を資産IDとする。

        '                            'sGenAssetID = Dao.ToolAsset.MakeAssetId(CInt(CatLrow(0).Item("ASSET_ID_REG_DIV")), String.Empty, CInt(CatLrow(0).Item("EPC_SIZE")), _LoginInfo)
        '                            If String.IsNullOrEmpty(sStr) Then
        '                                '空白もOK。そのまま登録する。
        '                            Else
        '                                If IsChkAssetId(iRowCnt, sStr) = False Then             'チェック関数化  2019/12/19 Update Inoue
        '                                    blRet = False
        '                                    Continue For
        '                                Else
        '                                    'エラー無し
        '                                    '16進数の文字列を0埋めする　方法4と同じ処理なので方法4を引数とする
        '                                    sStr = Dao.ToolAsset.MakeAssetId(AssetIDGenerationMethod.Inp, sStr, CatLrow(0).Item("EPC_SIZE"), _LoginInfo) '2019/10/24 nv miyaji
        '                                    'sGenAssetID = Dao.ToolAsset.MakeAssetId(AssetIDGenerationMethod.Inp, Dao.ToolAsset.HexToStr(sStr), CatLrow(0).Item("EPC_SIZE"), _LoginInfo) '2019/10/24 nv miyaji
        '                                    'sGenAssetID = sStr.PadRight(CInt(CatLrow(0).Item("EPC_SIZE")) / 8, "0"c)
        '                                End If
        '                            End If

        '                        Case AssetIDGenerationMethod.AssetNo
        '                            '★★方法②　資産番号を16進化して資産IDとする。

        '                            '資産IDに値が入っている場合、エラー
        '                            If Not String.IsNullOrEmpty(sStr) Then
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_HOUHOU2_INPUT)
        '                                blRet = False
        '                                Continue For
        '                            End If

        '                            If assetNoAutoRegFlg Then
        '                                '自動生成ありの場合、生成予定の資産番号を使用する
        '                                AssetNo = AssetNoAutoGenelate(assetNoSeq)
        '                            End If

        '                            sStr = Dao.ToolAsset.MakeAssetId(CInt(CatLrow(0).Item("ASSET_ID_REG_DIV")), AssetNo, CInt(CatLrow(0).Item("EPC_SIZE")), _LoginInfo) '2019/10/24 nv miyaji
        '                                'sGenAssetID = Dao.ToolAsset.MakeAssetId(CInt(CatLrow(0).Item("ASSET_ID_REG_DIV")), AssetNo, CInt(CatLrow(0).Item("EPC_SIZE")), _LoginInfo) '2019/10/24 nv miyaji
        '                        Case AssetIDGenerationMethod.Auto
        '                            '★★方法③　システムで自動生成してから16進化して資産IDとする。

        '                            '資産IDに値が入っている場合、エラー
        '                            If Not String.IsNullOrEmpty(sStr) Then
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_HOUHOU3_INPUT)
        '                                blRet = False
        '                                Continue For
        '                            End If
        '                            sStr = Dao.ToolAsset.MakeAssetId(CInt(CatLrow(0).Item("ASSET_ID_REG_DIV")), String.Empty, CInt(CatLrow(0).Item("EPC_SIZE")), _LoginInfo) '2019/10/24 nv miyaji
        '                                'sGenAssetID = Dao.ToolAsset.MakeAssetId(CInt(CatLrow(0).Item("ASSET_ID_REG_DIV")), String.Empty, CInt(CatLrow(0).Item("EPC_SIZE")), _LoginInfo) '2019/10/24 nv miyaji
        '                        Case AssetIDGenerationMethod.Inp
        '                            '★★方法④　資産マスタ新規登録時の入力値(CSV値)をそのまま資産IDとする。

        '                            If String.IsNullOrEmpty(sStr) Then
        '                                '空白チェック
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                                blRet = False
        '                                Continue For
        '                            ElseIf IsChkAssetId(iRowCnt, sStr) = False Then             'チェック関数化  2019/12/19 Update Inoue
        '                                '基本エラーチェックエラー時
        '                                blRet = False
        '                                Continue For
        '                            Else

        '                                If IsAssetIdDuplicate(sStr) Then
        '                                    '資産ID重複チェック
        '                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_REGISTED_ASSET_ID)
        '                                    blRet = False
        '                                End If

        '                                'エラー無し

        '                                '16進数の文字列を0埋めする
        '                                sStr = Dao.ToolAsset.MakeAssetId(AssetIDGenerationMethod.Inp, sStr, CatLrow(0).Item("EPC_SIZE"), _LoginInfo) '2019/10/24 nv miyaji
        '                                'sGenAssetID = Dao.ToolAsset.MakeAssetId(AssetIDGenerationMethod.Inp, Dao.ToolAsset.HexToStr(sStr), CatLrow(0).Item("EPC_SIZE"), _LoginInfo) '2019/10/24 nv miyaji
        '                                'sGenAssetID = sStr.PadRight(CInt(CatLrow(0).Item("EPC_SIZE")) / 8, "0"c)

        '                                '2019/09/30 nv miyaji コメントアウト
        '                                'Dim sAssetID As String = Dao.ToolAsset.MakeAssetId(CInt(CatLrow(0).Item("ASSET_ID_REG_DIV")), sStr, CInt(CatLrow(0).Item("EPC_SIZE")), _LoginInfo)
        '                                ''資産IDの長さチェック
        '                                'If (CInt(CatLrow(0).Item("EPC_SIZE")) / 4) < sAssetID.Length Then
        '                                '    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                '    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                '    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_ASSETID_LENGTH)
        '                                '    blRet = False
        '                                'ElseIf Not sStr = String.Empty AndAlso lstIDSumiCd.IndexOf(sAssetID) > -1 Then
        '                                '    '重複チェック
        '                                '    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                '    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                '    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID) & "[{0}]", sStr)
        '                                '    blRet = False
        '                                'Else
        '                                '    'エラー無し

        '                                '    'オブジェクトの編集開始
        '                                '    row.BeginEdit()

        '                                '    Me._dtCsvImport.Rows(0).Item("GEN_ASSET_ID") = sAssetID
        '                                '    Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID) = sAssetID

        '                                '    'オブジェクトの編集終了
        '                                '    row.EndEdit()

        '                                '    lstIDSumiCd.Add(sStr)
        '                                'End If
        '                            End If
        '                            '--2020/02/17 nv miyaji add start
        '                        Case AssetIDGenerationMethod.GIAI
        '                            '★★方法⑤　固定値＋設定マスタの事業所コード＋シリアルNoを16進化して資産IDとする

        '                            '資産IDに値が入っている場合、エラー
        '                            If Not String.IsNullOrEmpty(sStr) Then
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_HOUHOU5_INPUT)
        '                                blRet = False
        '                                Continue For
        '                            End If

        '                            '事業所コード
        '                            Dim sComKey As String = CatLrow(0).Item("EPC_COMPANY_KEY")

        '                            Dim Settinginfo As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.AssetIDGenerationCoPrefixGIAI.KeyDiv, CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key1, sComKey, CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key3)
        '                            '設定マスタからデータを取得できなかった場合、エラー
        '                            If Settinginfo Is Nothing Then
        '                                Throw New Exception(String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), (CommonSettingKey.AssetIDGenerationCoPrefixGIAI.KeyDiv & " " & CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key1 & " " & sComKey)))
        '                                'ModelState.AddModelError("ASSET_ID", String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), (CommonSettingKey.AssetIDGenerationCoPrefixGIAI.KeyDiv & " " & CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key1)))
        '                            Else
        '                                'シリアルNo取得
        '                                Dim iSerialNo As String = Dao.ToolAsset.GetSerialNo(AssetIDGenerationMethod.GIAI, Settinginfo.DATA1, "**", _LoginInfo)

        '                                '資産ID取得
        '                                Dim libEpc As AsetraLibEpc.LibEpc = New AsetraLibEpc.LibEpc
        '                                sStr = libEpc.MakeCodeGIAI96(Settinginfo.DATA1, CLng(iSerialNo))
        '                                Select Case libEpc.Stat
        '                                    Case AsetraLibEpc.LibEpc.EpcStatus.ErrCompanyPrefix
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_COM_CD)
        '                                        blRet = False
        '                                        Continue For
        '                                    Case AsetraLibEpc.LibEpc.EpcStatus.ErrAssetType
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_ASSET_TYPE)
        '                                        blRet = False
        '                                        Continue For
        '                                    Case AsetraLibEpc.LibEpc.EpcStatus.ErrSerialNo
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SERIAL_NO)
        '                                        blRet = False
        '                                        Continue For
        '                                    Case AsetraLibEpc.LibEpc.EpcStatus.ErrOther
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)
        '                                        blRet = False
        '                                        Continue For
        '                                End Select
        '                            End If
        '                        Case AssetIDGenerationMethod.GRAI
        '                            '★★方法⑤　固定値＋設定マスタの事業所コード＋カテゴリマスタのEPC資産タイプ＋シリアルNoを16進化して資産IDとする

        '                            '資産IDに値が入っている場合、エラー
        '                            If Not String.IsNullOrEmpty(sStr) Then
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_HOUHOU6_INPUT)
        '                                blRet = False
        '                                Continue For
        '                            End If

        '                            '事業所コード
        '                            Dim sComKey As String = CatLrow(0).Item("EPC_COMPANY_KEY")

        '                            Dim Settinginfo As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.AssetIDGenerationCoPrefixGRAI.KeyDiv, CommonSettingKey.AssetIDGenerationCoPrefixGRAI.Key1, sComKey, CommonSettingKey.AssetIDGenerationCoPrefixGRAI.Key3)
        '                            '設定マスタからデータを取得できなかった場合、エラー
        '                            If Settinginfo Is Nothing Then
        '                                Throw New Exception(String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), (CommonSettingKey.AssetIDGenerationCoPrefixGRAI.KeyDiv & " " & CommonSettingKey.AssetIDGenerationCoPrefixGRAI.Key1 & " " & sComKey)))
        '                            Else
        '                                'EPC資産タイプ
        '                                Dim sAssetType As String = CatLrow(0).Item("EPC_ASSET_TYPE")
        '                                'シリアルNo取得
        '                                Dim iSerialNo As String = Dao.ToolAsset.GetSerialNo(AssetIDGenerationMethod.GRAI, Settinginfo.DATA1, sAssetType, _LoginInfo)

        '                                '資産ID取得
        '                                Dim libEpc As AsetraLibEpc.LibEpc = New AsetraLibEpc.LibEpc
        '                                sStr = libEpc.MakeCodeGRAI96(Settinginfo.DATA1, sAssetType, CLng(iSerialNo))
        '                                Select Case libEpc.Stat
        '                                    Case AsetraLibEpc.LibEpc.EpcStatus.ErrCompanyPrefix
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_COM_CD)
        '                                        blRet = False
        '                                        Continue For
        '                                    Case AsetraLibEpc.LibEpc.EpcStatus.ErrAssetType
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_ASSET_TYPE)
        '                                        blRet = False
        '                                        Continue For
        '                                    Case AsetraLibEpc.LibEpc.EpcStatus.ErrSerialNo
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SERIAL_NO)
        '                                        blRet = False
        '                                        Continue For
        '                                    Case AsetraLibEpc.LibEpc.EpcStatus.ErrOther
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)
        '                                        blRet = False
        '                                        Continue For
        '                                End Select
        '                            End If
        '                            '--2020/02/17 nv miyaji add end
        '                    End Select

        '                    '編集したCSV文字列をデータテーブルに格納する
        '                    'If Not CInt(CatLrow(0).Item("ASSET_ID_REG_DIV")) = AssetIDGenerationMethod.Inp Then　'2019/09/30 nv miyaji コメントアウト
        '                    If sStr = String.Empty Then
        '                        Me._dtCsvImport.Rows(0).Item("GEN_ASSET_ID") = String.Empty
        '                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID) = String.Empty
        '                    Else
        '                        '--2019/10/24 nv miyaji コメントアウト start
        '                        'Dim aryByte As Byte() = Encoding.UTF8.GetBytes(sGenAssetID)
        '                        ''オブジェクトの編集開始
        '                        'row.BeginEdit()

        '                        'Me._dtCsvImport.Rows(0).Item("GEN_ASSET_ID") = sGenAssetID
        '                        'row(CsvMapMei_Asset.ColIdx.AssetID) = BitConverter.ToString(aryByte).Replace("-", String.Empty)
        '                        'sStr = BitConverter.ToString(aryByte).Replace("-", String.Empty)

        '                        ''オブジェクトの編集終了
        '                        'row.EndEdit()
        '                        '--2019/10/24 nv miyaji コメントアウト end

        '                        'If (CatLrow(0).Item("EPC_SIZE") / 4) < sStr.Length Then '2020/02/10 nv miyaji コメントアウト
        '                        If (CatLrow(0).Item("EPC_SIZE") / 4) <> sStr.Length Then '2020/02/10 nv miyaji EPCサイズで設定された文字の長さでなければエラーにする
        '                            '資産IDの長さチェック
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_ASSETID_LENGTH)
        '                            blRet = False
        '                        ElseIf (Not sStr = String.Empty) AndAlso htIDSumiCd.ContainsKey(sStr) Then      'Update Inoue 2019/12/18
        '                            '重複チェック
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID) & "[{0}]", sStr)
        '                            blRet = False
        '                        Else
        '                            '--2019/10/24 nv miyaji add start
        '                            Me._dtCsvImport.Rows(0).Item("GEN_ASSET_ID") = sStr
        '                            Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID) = sStr
        '                            '--2019/10/24 nv miyaji add end

        '                            '資産ID重複チェック用ハッシュテーブルに追加（value には、一旦、資産番号を入れる）   'Update Inoue 2019/12/18
        '                            htIDSumiCd.Add(sStr, Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetNo).ToString.Trim)

        '                        End If
        '                    End If

        '                    'End If

        '                Else
        '                    '●データベースにあり、除却以外の場合

        '                    '内容一致チェック
        '                    'If Not CommonRfid.ToStringNvl(model.ASSET_ID) = CommonRfid.ToStringNvl(sStr) Then      2019/12/18 UPDATE    '2019/12/19 nv miyaji
        '                    If Not CommonRfid.ToStringNvl(rowsAsset(0).Item("ASSET_ID")) = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji

        '                        If CInt(CatLrow(0).Item("ASSET_ID_REG_DIV")) = AssetIDGenerationMethod.None Then
        '                            'If model.TAG_LINK_STATUS = TagLinkStatus.Mi Then      2019/12/18 UPDATE
        '                            If rowsAsset(0).Item("TAG_LINK_STATUS").ToString = TagLinkStatus.Mi Then
        '                                '資産ＩＤ生成方法１（タグ紐付け）の場合、既存が空白(null)ならIDを設定する　（←タグ紐付状態＝未　で判断しているようだ）

        '                                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                                'If Not CommonRfid.ToStringNvl(model.ASSET_ID) = CommonRfid.ToStringNvl(sStr) Then
        '                                If Not CommonRfid.ToStringNvl(rowsAsset(0).Item("ASSET_ID")) = CommonRfid.ToStringNvl(sStr) Then
        '                                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                                End If

        '                                'Dim sGenAssetID As String = String.Empty '2019/10/24 nv miyaji　コメントアウト
        '                                If IsChkAssetId(iRowCnt, sStr) = False Then             'チェック関数化  2019/12/19 Update Inoue
        '                                    '基本エラーチェックエラー時
        '                                    blRet = False
        '                                Else
        '                                    'エラー無し

        '                                    '16進数の文字列を0埋めする　方法4と同じ処理なので方法4を引数とする
        '                                    sStr = Dao.ToolAsset.MakeAssetId(AssetIDGenerationMethod.Inp, sStr, CInt(rowsAsset(0).Item("EPC_SIZE")), _LoginInfo) '2019/12/19 nv miyaji
        '                                    'sStr = Dao.ToolAsset.MakeAssetId(AssetIDGenerationMethod.Inp, sStr, model.EPC_SIZE, _LoginInfo) '2019/10/24 nv miyaji '2019/12/19 nv miyaji


        '                                    'sGenAssetID = Dao.ToolAsset.MakeAssetId(AssetIDGenerationMethod.Inp, Dao.ToolAsset.HexToStr(sStr), model.EPC_SIZE, _LoginInfo) '2019/10/24 nv miyaji
        '                                    'sGenAssetID = sStr.PadRight(CInt(CatLrow(0).Item("EPC_SIZE")) / 8, "0"c)

        '                                    '--2019/10/24 nv miyaji コメントアウト start
        '                                    '16進変換
        '                                    'Dim aryByte As Byte() = Encoding.UTF8.GetBytes(sGenAssetID)
        '                                    ''オブジェクトの編集開始
        '                                    'row.BeginEdit()

        '                                    'Me._dtCsvImport.Rows(0).Item("GEN_ASSET_ID") = sGenAssetID
        '                                    'row(CsvMapMei_Asset.ColIdx.AssetID) = BitConverter.ToString(aryByte).Replace("-", String.Empty)
        '                                    'sStr = BitConverter.ToString(aryByte).Replace("-", String.Empty)

        '                                    ''オブジェクトの編集終了
        '                                    'row.EndEdit()
        '                                    '--2019/10/24 nv miyaji コメントアウト end

        '                                    'If (model.EPC_SIZE / 4) < sStr.Length Then '2019/12/19 nv miyaji
        '                                    'If (CInt(rowsAsset(0).Item("EPC_SIZE")) / 4) < sStr.Length Then '2019/12/19 nv miyaji　'2020/02/10 nv miyaji コメントアウト
        '                                    If (CInt(rowsAsset(0).Item("EPC_SIZE")) / 4) <> sStr.Length Then '2020/02/10 nv miyaji EPCサイズで設定された文字の長さでなければエラーにする
        '                                        '資産IDの長さチェック
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_ASSETID_LENGTH)
        '                                        blRet = False
        '                                    ElseIf Not sStr = String.Empty AndAlso htIDSumiCd.ContainsKey(sStr) Then      'Update Inoue 2019/12/18
        '                                        '重複チェック
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID) & "[{0}]", sStr)
        '                                        blRet = False
        '                                    Else
        '                                        '--2019/10/24 nv miyaji add start
        '                                        Me._dtCsvImport.Rows(0).Item("GEN_ASSET_ID") = sStr
        '                                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID) = sStr
        '                                        '--2019/10/24 nv miyaji add end

        '                                        '資産ID重複チェック用ハッシュテーブルに追加（value には、一旦、資産番号を入れる）   'Update Inoue 2019/12/18
        '                                        htIDSumiCd.Add(sStr, Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetNo).ToString.Trim)
        '                                    End If

        '                                End If
        '                            Else
        '                                '既存がタグ紐付け済みの場合
        '                                If Not CommonRfid.ToStringNvl(sStr) = String.Empty Then
        '                                    'ファイルの資産IDに値があればエラー
        '                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_CHANGED_ID)
        '                                    blRet = False
        '                                    '--2019/10/24 nv miyaji add start
        '                                Else
        '                                    '既存が空白でなくファイルの資産IDに値がない場合、DB値を設定する
        '                                    sStr = rowsAsset(0).Item("ASSET_ID")   '2019/12/19 nv miyaji

        '                                    If htIDSumiCd.ContainsKey(sStr) Then      'Update Inoue 2019/12/18
        '                                        '重複チェック
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID) & "[{0}]", sStr)
        '                                        blRet = False
        '                                        '--2019/10/24 nv miyaji add start
        '                                    Else
        '                                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID) = sStr                        '2019/12/19 nv miyaji
        '                                        Me._dtCsvImport.Rows(0).Item("GEN_ASSET_ID") = rowsAsset(0).Item("GEN_ASSET_ID")    '2019/12/19 nv miyaji

        '                                        '資産ID重複チェック用ハッシュテーブルに追加（value には、一旦、資産番号を入れる）   'Update Inoue 2019/12/18
        '                                        htIDSumiCd.Add(sStr, Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetNo).ToString.Trim)
        '                                    End If
        '                                End If
        '                            End If
        '                        Else
        '                            '資産ＩＤ生成方法１（タグ紐付け）以外の場合、資産ID内容が一致しなければエラー
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_CHANGED_ID)
        '                            blRet = False
        '                        End If
        '                        '--2019/10/24 nv miyaji add start
        '                        '内容一致時の重複チェック
        '                    ElseIf htIDSumiCd.ContainsKey(sStr) Then      'Update Inoue 2019/12/18
        '                        '重複チェック
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID) & "[{0}]", sStr)
        '                        blRet = False
        '                        '--2019/10/24 nv miyaji add start
        '                    Else
        '                        'エラー無し
        '                        Me._dtCsvImport.Rows(0).Item("GEN_ASSET_ID") = rowsAsset(0).Item("GEN_ASSET_ID")   '2019/12/19 nv miyaji
        '                        'Me._dtCsvImport.Rows(0).Item("GEN_ASSET_ID") = model.GEN_ASSET_ID   '2019/12/19 nv miyaji

        '                        If Not String.IsNullOrEmpty(sStr) Then

        '                            '資産ID重複チェック用ハッシュテーブルに追加（value には、一旦、資産番号を入れる）   'Update Inoue 2019/12/18
        '                            htIDSumiCd.Add(sStr, Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetNo).ToString.Trim)
        '                        End If
        '                    End If

        '                End If

        '            Case CsvMapMei_Asset.ColIdx.AssetDiv
        '                '資産区分
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("ASSET_DIV")).Trim = CommonRfid.ToStringNvl(sStr) Then   '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.ASSET_DIV).Trim = CommonRfid.ToStringNvl(sStr) Then   '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True

        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.AssetDiv Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_DIV)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.AssetNm
        '                '資産名称
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.ASSET_NAME).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("ASSET_NAME")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.AssetNm Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '                '--2021/10/04 nv miyaji add start
        '            Case CsvMapMei_Asset.ColIdx.MngLocLCd
        '                '管理ロケ分コード
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("MNG_LOC_L_CD")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MNG_LOC_KBN_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                    blRet = False
        '                ElseIf sStr.Length > itemlength.MngLocLCd Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MNG_LOC_KBN_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                    '半角英数字チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MNG_LOC_KBN_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                ElseIf MngLocLrow Is Nothing OrElse MngLocLrow.Length = 0 Then
        '                    'ロケーションマスタに登録されているロケーションコードかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MNG_LOC_KBN_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                    blRet = False
        '                    '2023/02/08 Kawamoto ADD START
        '                ElseIf Not lstAuthLocation.Contains(sStr) Then
        '                    '登録/更新権限のあるロケーションコードかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MNG_LOC_KBN_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_AUTH_LOCATION)
        '                    blRet = False
        '                ElseIf CsvRegistAll.DATA1 = AssetCSVRegistAll.Difference Then
        '                    '資産CSVファイル取込全件対象が「差分取込」の場合
        '                    Select Case Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.Activity).ToString.Trim
        '                        '処理区分：U(更新)、D(削除)、R(除却)の場合
        '                        Case CsvImportProcDiv.Update, CsvImportProcDiv.Delete, CsvImportProcDiv.Retirement
        '                            '資産情報を取得
        '                            Dim model As AssetListModel = db.AssetList.Find(Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetNo).ToString.Trim)
        '                            ''登録/更新権限のあるロケーションコードかチェック
        '                            If Not IsNothing(model) AndAlso Not lstAuthLocation.Contains(model.MNG_LOC_L_CD) Then
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MNG_LOC_KBN_CD)
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_AUTH_LOCATION)
        '                                blRet = False
        '                            End If
        '                    End Select
        '                End If
        '                '2023/02/08 Kawamoto ADD END
        '            Case CsvMapMei_Asset.ColIdx.LeaseStatus
        '                '貸出状態
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("LEASE_STATUS")).Trim = CommonRfid.ToStringNvl(sStr) Then
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                    blRet = False
        '                ElseIf sStr <> LeaseStatusAsset_Csv.Kanou AndAlso sStr <> LeaseStatusAsset_Csv.Fuka Then
        '                    '「Y」or「N」チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                Else
        '                    '資産状態が保管中以外の場合、データベースの値と違う場合エラー
        '                    Dim sLeaseStatus As String = LeaseStatusAsset.Kanou
        '                    If sStr <> LeaseStatusAsset_Csv.Kanou Then
        '                        sLeaseStatus = LeaseStatusAsset.Fuka
        '                    End If
        '                    If enumDbDataKbn = DbDataKbn.Exist AndAlso rowsAsset(0).Item("ASSET_STATUS").ToString.Trim <> ASSET_STATUS.Storage AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("LEASE_STATUS")).Trim = sLeaseStatus Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_UPD_LEASE_STATUS)
        '                        blRet = False
        '                        '2023/02/08 Kawamoto ADD START
        '                    ElseIf enumDbDataKbn = DbDataKbn.Non AndAlso CommonRfid.ToStringNvl(sStr) <> LeaseStatusAsset_Csv.Fuka Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_STATUS)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_REGIST_LEASE_STATUS)
        '                        blRet = False
        '                        '2023/02/08 Kawamoto ADD END
        '                    End If
        '                End If
        '            'メンテ発生貸出回数
        '            Case CsvMapMei_Asset.ColIdx.MainteLeaseNum
        '                If Not String.IsNullOrEmpty(sStr) Then
        '                    '項目長チェック
        '                    If sStr.Length > itemlength.MainteLeaseNum Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_NUM)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                        blRet = False
        '                    ElseIf Not IsNumeric(sStr) Then
        '                        '半角数字チェック
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_NUM)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                        blRet = False
        '                    End If
        '                End If
        '            'メンテ発生貸出日数
        '            Case CsvMapMei_Asset.ColIdx.MainteLeaseDay
        '                If Not String.IsNullOrEmpty(sStr) Then
        '                    '項目長チェック
        '                    If sStr.Length > itemlength.MainteLeaseDay Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                        blRet = False
        '                    ElseIf Not IsNumeric(sStr) Then
        '                        '半角数字チェック
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                        blRet = False
        '                        '2021/11/25 nv miyaji 0の時エラーにしないよう修正
        '                        'ElseIf Not String.IsNullOrEmpty(sStr) AndAlso CInt(sStr) = 0 Then
        '                        '    'メンテ発生貸出日数が０の場合エラー
        '                        '    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        '    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY)
        '                        '    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NISSU_ZERO)
        '                        '    blRet = False
        '                    End If
        '                End If
        '            'メンテ発生貸出日数稼働日フラグ
        '            Case CsvMapMei_Asset.ColIdx.MainteLeaseDayWorkFlg
        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白チェック
        '                    'メンテ発生貸出日数が空でない時
        '                    If Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseDay).ToString.Trim <> String.Empty Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY_WORK_FLG)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    End If
        '                ElseIf sStr <> MainteWorkFlg.Yes AndAlso sStr <> MainteWorkFlg.No Then
        '                    '「Y」or「N」チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY_WORK_FLG)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                Else
        '                    'メンテ発生貸出日数稼働日フラグにYorNが入っている時
        '                    'メンテ発生貸出日数が空の時
        '                    If Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseDay).ToString.Trim = String.Empty Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    End If
        '                End If
        '            'メンテ日数
        '            Case CsvMapMei_Asset.ColIdx.MainteDay
        '                If Not String.IsNullOrEmpty(sStr) Then
        '                    '項目長チェック
        '                    If sStr.Length > itemlength.MainteDay Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                        blRet = False
        '                    ElseIf Not IsNumeric(sStr) Then
        '                        '半角数字チェック
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                        blRet = False
        '                        '2021/11/25 nv miyaji 0の時エラーにしないよう修正
        '                        'ElseIf Not String.IsNullOrEmpty(sStr) AndAlso CInt(sStr) = 0 Then
        '                        '    'メンテ日数が０の場合エラー
        '                        '    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        '    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY)
        '                        '    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NISSU_ZERO)
        '                        '    blRet = False
        '                    End If
        '                End If
        '            'メンテ日数稼働日フラグ
        '            Case CsvMapMei_Asset.ColIdx.MainteDayWorkFlg
        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白チェック
        '                    'メンテ日数が空でない時
        '                    If Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteDay).ToString.Trim <> String.Empty Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY_WORK_FLG)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    End If
        '                ElseIf sStr <> MainteWorkFlg.Yes AndAlso sStr <> MainteWorkFlg.No Then
        '                    '「Y」or「N」チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY_WORK_FLG)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                Else
        '                    'メンテ日数稼働日フラグにYorNが入っている時
        '                    'メンテ日数が空の時
        '                    If Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteDay).ToString.Trim = String.Empty Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    End If
        '                End If
        '                '--2021/10/04 nv miyaji add end
        '            Case CsvMapMei_Asset.ColIdx.LocLCd
        '                'ロケ大区分コード
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("LOC_L_CD")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.LOC_L_CD).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                    blRet = False
        '                    '--2020/03/13 nv miyaji update start
        '                ElseIf sStr.Length > itemlength.LocLCd Then
        '                    'End If
        '                    'If sStr.Length > itemlength.LocLCd Then
        '                    '--2020/03/13 nv miyaji update end
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                    '半角英数字チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                ElseIf LocLrow Is Nothing OrElse LocLrow.Length = 0 Then
        '                    'ロケーションマスタに登録されているロケーションコードかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                    blRet = False
        '                End If

        '            Case CsvMapMei_Asset.ColIdx.LocMCd
        '                'ロケ中区分コード
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("LOC_M_CD")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.LOC_M_CD).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '値がない場合、空白を入れる
        '                    Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocMCd) = " "
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.LocMCd Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                    '半角英数字チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                ElseIf LocLrow Is Nothing OrElse LocLrow.Length = 0 Then
        '                    '上位の大区分が登録されているかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_LOC_L_CD)
        '                    blRet = False
        '                ElseIf LocMrow Is Nothing OrElse LocMrow.Length = 0 Then
        '                    'ロケーションマスタに登録されているロケーションコードかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.LocSCd
        '                'ロケ小区分コード
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("LOC_S_CD")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.LOC_S_CD).Trim = CommonRfid.ToStringNvl(sStr) Then   '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '値がない場合、空白を入れる
        '                    Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocSCd) = " "
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.LocSCd Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                    '半角英数字チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                ElseIf LocLrow Is Nothing OrElse LocLrow.Length = 0 Then
        '                    '上位の大区分が登録されているかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_LOC_L_CD)
        '                    blRet = False
        '                ElseIf LocMrow Is Nothing OrElse LocMrow.Length = 0 Then
        '                    '上位の中区分が登録されているかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_LOC_M_CD)
        '                    blRet = False
        '                Else
        '                    'ロケーションマスタに登録されているロケーションコードかチェック
        '                    Dim Locrow As DataRow() = LocaData.Select("LOC_L_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocLCd).ToString + "' AND LOC_M_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocMCd).ToString + "' AND LOC_S_CD = '" + sStr + "' AND LOC_DIV = '" + LOC_DIV.LocationS + "'")
        '                    If Locrow Is Nothing OrElse Locrow.Length = 0 Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION) & Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                        blRet = False
        '                    End If
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.CatLCd
        '                'If model Is Nothing OrElse Not model.RETIRE_TS Is Nothing Then     既存DBデータ区分で判断 2019/12/18
        '                If enumDbDataKbn = DbDataKbn.Non Or enumDbDataKbn = DbDataKbn.Removed Then
        '                    '新規又は既存が除却の場合
        '                    'カテゴリ大分類コード
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白チェック
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    ElseIf sStr.Length > itemlength.CatLCd Then
        '                        '項目長チェック
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                        blRet = False
        '                    ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                        '半角英数字チェック
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                        blRet = False
        '                    Else
        '                        'カテゴリマスタに登録されているカテゴリコードかチェック
        '                        If CatLrow Is Nothing OrElse CatLrow.Length = 0 Then
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                            blRet = False
        '                        Else
        '                            'EPCサイズを設定する
        '                            Me._dtCsvImport.Rows(0).Item("EPC_SIZE") = CInt(CatLrow(0)("EPC_SIZE"))
        '                        End If
        '                    End If
        '                Else
        '                    'データベースにある場合
        '                    '内容一致チェック
        '                    If Not rowsAsset(0).Item("CAT_L_CD").ToString.Trim = sStr Then    '2019/12/19 nv miyaji
        '                        'If Not model.CAT_L_CD.Trim = sStr Then    '2019/12/19 nv miyaji
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_CHANGED_CAT)
        '                        blRet = False
        '                    Else
        '                        'カテゴリマスタに登録されているカテゴリコードかチェック
        '                        If CatLrow Is Nothing OrElse CatLrow.Length = 0 Then
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                            blRet = False
        '                        Else
        '                            '既存モデルのEPCサイズを設定する
        '                            Me._dtCsvImport.Rows(0).Item("EPC_SIZE") = rowsAsset(0).Item("EPC_SIZE")
        '                            'Me._dtCsvImport.Rows(0).Item("EPC_SIZE") = model.EPC_SIZE
        '                        End If
        '                    End If
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.CatMCd
        '                'カテゴリ中分類コード
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("CAT_M_CD")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.CAT_M_CD).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '値がない場合、空白を入れる
        '                    Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatMCd) = " "
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.CatMCd Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                    '半角英数字チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                ElseIf CatLrow Is Nothing OrElse CatLrow.Length = 0 Then
        '                    '上位の大分類が登録されているかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_CAT_L_CD)
        '                    blRet = False
        '                ElseIf CatMrow Is Nothing OrElse CatMrow.Length = 0 Then
        '                    'カテゴリーマスタに登録されているカテゴリーコードかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.CatSCd
        '                'カテゴリ小分類コード
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("CAT_S_CD")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.CAT_S_CD).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True

        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '値がない場合、空白を入れる
        '                    Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatSCd) = " "
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.CatSCd Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                    '半角英数字チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                ElseIf CatLrow Is Nothing OrElse CatLrow.Length = 0 Then
        '                    '上位の大分類が登録されているかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_CAT_L_CD)
        '                    blRet = False
        '                ElseIf CatMrow Is Nothing OrElse CatMrow.Length = 0 Then
        '                    '上位の中分類が登録されているかチェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_CAT_M_CD)
        '                    blRet = False
        '                Else
        '                    'カテゴリーマスタに登録されているカテゴリーコードかチェック
        '                    Dim Catrow As DataRow() = CategoryData.Select("CAT_L_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatLCd).ToString + "' AND CAT_M_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatMCd).ToString + "' AND CAT_S_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatSCd).ToString + "' AND CAT_DIV = '" + CAT_DIV.CategoryS + "'")
        '                    If Catrow Is Nothing OrElse Catrow.Length = 0 Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY) & Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                        blRet = False
        '                    End If
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.Maker
        '                'メーカ
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("MAKER")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.MAKER).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.Maker Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAKER)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.ProductNo
        '                '品番
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("PRODUCT_NO")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.PRODUCT_NO).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True

        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.ProductNo Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
        '                    '半角英数字記号チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PRODUCT_NO)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.ModelNo
        '                '型番
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("MODEL_NO")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.MODEL_NO).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '2023/02/08 Kawamoto UPD START
        '                'If String.IsNullOrEmpty(sStr) Then
        '                '    '空白ならチェックしない
        '                '    Continue For
        '                'End If
        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '空白チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MODEL_NO)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If
        '                '2023/02/08 Kawamoto UPD END

        '                If sStr.Length > itemlength.ModelNo Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MODEL_NO)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                    '--2021/10/13 sv suzuki del start
        '                    'ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
        '                    '    '半角英数字記号チェック
        '                    '    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    '    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MODEL_NO)
        '                    '    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    '    blRet = False
        '                    '--2021/10/13 sv suzuki del end
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.LeaseNo
        '                'リース管理番号
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("LEASE_NO")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.LEASE_NO).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.LeaseNo Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_NO)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.LeaseLimit
        '                'リース期限
        '                If String.IsNullOrEmpty(sStr) Then
        '                    '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                    If enumDbDataKbn = DbDataKbn.Exist Then    '2019/12/19 nv miyaji
        '                        'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing Then    '2019/12/19 nv miyaji
        '                        If Not CommonRfid.ToStringNvl(rowsAsset(0).Item("LEASE_LIMIT")) = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                            'If Not CommonRfid.ToStringNvl(model.LEASE_LIMIT) = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                            Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                        End If
        '                    End If

        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.LeaseLimit + 2 Then '/が入るのでその分プラスする。
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
        '                    '半角英数字記号チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                Else
        '                    '日付チェック
        '                    'DateTimeに変換できるか確かめる
        '                    Dim dt As DateTime
        '                    If Not DateTime.TryParse(sStr, dt) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                        blRet = False
        '                    End If

        '                    '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                    If enumDbDataKbn = DbDataKbn.Exist Then    '2019/12/19 nv miyaji
        '                        'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing Then    '2019/12/19 nv miyaji
        '                        If Not CommonRfid.ToStringNvl(rowsAsset(0).Item("LEASE_LIMIT")) = dt.ToString("yyyyMMdd") Then    '2019/12/19 nv miyaji
        '                            'If Not CommonRfid.ToStringNvl(model.LEASE_LIMIT) = dt.ToString("yyyyMMdd") Then    '2019/12/19 nv miyaji
        '                            Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                        End If
        '                    End If

        '                    '1000年未満の場合エラー
        '                    ' / を取り除いて数字だけにする
        '                    Dim sCleanStr As String = System.Text.RegularExpressions.Regex.Replace(sStr, "[^0-9]", "")
        '                    Dim num As Integer

        '                    ' TryParseで日付チェック ＆ 年が1000未満ならエラー
        '                    If Integer.TryParse(sCleanStr, num) Then
        '                        If num < MIN_DATE Then
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_INCORRECT_DATE)
        '                            blRet = False
        '                        End If

        '                    End If

        '                End If
        '            Case CsvMapMei_Asset.ColIdx.GenLimit
        '                '○○期限
        '                If String.IsNullOrEmpty(sStr) Then
        '                    '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                    If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("GENERIC_LIMIT")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                        'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.GENERIC_LIMIT) = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                        Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                    End If

        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.GenLimit + 2 Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
        '                    '半角英数字記号チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    blRet = False
        '                Else
        '                    '日付チェック
        '                    'DateTimeに変換できるか確かめる
        '                    Dim dt As DateTime
        '                    If Not DateTime.TryParse(sStr, dt) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                        blRet = False
        '                    End If

        '                    '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                    If enumDbDataKbn = DbDataKbn.Exist Then    '2019/12/19 nv miyaji
        '                        'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing Then    '2019/12/19 nv miyaji
        '                        If Not CommonRfid.ToStringNvl(rowsAsset(0).Item("GENERIC_LIMIT")) = dt.ToString("yyyyMMdd") Then   '2019/12/19 nv miya
        '                            'If Not CommonRfid.ToStringNvl(model.GENERIC_LIMIT) = dt.ToString("yyyyMMdd") Then   '2019/12/19 nv miya
        '                            Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                        End If
        '                    End If

        '                    '1000年未満の場合エラー
        '                    ' / を取り除いて数字だけにする
        '                    Dim sCleanStr As String = System.Text.RegularExpressions.Regex.Replace(sStr, "[^0-9]", "")
        '                    Dim num As Integer

        '                    ' TryParseで日付チェック ＆ 年が1000未満ならエラー
        '                    If Integer.TryParse(sCleanStr, num) Then
        '                        If num < MIN_DATE Then
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_INCORRECT_DATE)
        '                            blRet = False
        '                        End If
        '                    End If

        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem1
        '                'フリー項目1
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM1")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.FREE_ITEM1).Trim = CommonRfid.ToStringNvl(sStr) Then   '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_KOUNYU_DATE)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                Else
        '                    '日付チェック '2019/10/01 nv miyaji add
        '                    'DateTimeに変換できるか確かめる
        '                    Dim dt As DateTime
        '                    If Not DateTime.TryParse(sStr, dt) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_KOUNYU_DATE)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                        blRet = False
        '                    Else
        '                        'スラッシュ区切りの日付形式に編集する
        '                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem1) = CommonRfid.ToStringDateYMD(dt)
        '                    End If
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem2
        '                'フリー項目2
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM2")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.FREE_ITEM2).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '2023/02/08 Kawamoto UPD START
        '                'If String.IsNullOrEmpty(sStr) Then
        '                '    '空白ならチェックしない
        '                '    Continue For
        '                'End If
        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '空白チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM2)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If
        '                '2023/02/08 Kawamoto UPD END

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM2)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem3
        '                'フリー項目3
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM3")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.FREE_ITEM3).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '2023/02/08 Kawamoto UPD START
        '                'If String.IsNullOrEmpty(sStr) Then
        '                '    '空白ならチェックしない
        '                '    Continue For
        '                'End If
        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '空白チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM3)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                        'マスタ存在チェック
        '                    ElseIf Not Dao.ToolSetting.ChkExistDrugsFSActNumber(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM3)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), sStr)
        '                        blRet = False
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If
        '                '2023/02/08 Kawamoto UPD END

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM3)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem4
        '                'フリー項目4
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM4")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.FREE_ITEM4).Trim = CommonRfid.ToStringNvl(sStr) Then   '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '2023/02/08 Kawamoto UPD START
        '                'If String.IsNullOrEmpty(sStr) Then
        '                '    '空白ならチェックしない
        '                '    Continue For
        '                'End If
        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '入力値(Y/N/-)チェック
        '                    If Not String.IsNullOrEmpty(sStr) AndAlso sStr <> Exist.Yes AndAlso sStr <> Exist.No AndAlso sStr <> Exist.None Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM4)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
        '                        blRet = False
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If
        '                '2023/02/08 Kawamoto UPD END

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM4)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem5
        '                'フリー項目5
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM5")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.FREE_ITEM5).Trim = CommonRfid.ToStringNvl(sStr) Thenv
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '2023/02/08 Kawamoto UPD START
        '                'If String.IsNullOrEmpty(sStr) Then
        '                '    '空白ならチェックしない
        '                '    Continue For
        '                'End If
        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '入力値(1:毒物/2：劇物/-：指定なし)チェック
        '                    If Not String.IsNullOrEmpty(sStr) AndAlso sStr <> PoisonAndDeleterious.Poison AndAlso sStr <> PoisonAndDeleterious.Deleterious AndAlso sStr <> PoisonAndDeleterious.None Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM5)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
        '                        blRet = False
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If
        '                '2023/02/08 Kawamoto UPD END

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM5)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem6
        '                'フリー項目6
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM6")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.FREE_ITEM6).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '2023/02/08 Kawamoto UPD START
        '                'If String.IsNullOrEmpty(sStr) Then
        '                '    '空白ならチェックしない
        '                '    Continue For
        '                'End If
        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '入力値(Y/N/-)チェック
        '                    If Not String.IsNullOrEmpty(sStr) AndAlso sStr <> Exist.Yes AndAlso sStr <> Exist.No AndAlso sStr <> Exist.None Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM6)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
        '                        blRet = False
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If
        '                '2023/02/08 Kawamoto UPD END

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM6)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem7
        '                'フリー項目7
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM7")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.FREE_ITEM7).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '2023/02/08 Kawamoto UPD START
        '                'If String.IsNullOrEmpty(sStr) Then
        '                '    '空白ならチェックしない
        '                '    Continue For
        '                'End If
        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '入力値(Y/N/-)チェック
        '                    If Not String.IsNullOrEmpty(sStr) AndAlso sStr <> Exist.Yes AndAlso sStr <> Exist.No AndAlso sStr <> Exist.None Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM7)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
        '                        blRet = False
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If
        '                '2023/02/08 Kawamoto UPD END

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM7)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem8
        '                'フリー項目8
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM8")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.FREE_ITEM8).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '2023/02/08 Kawamoto UPD START
        '                'If String.IsNullOrEmpty(sStr) Then
        '                '    '空白ならチェックしない
        '                '    Continue For
        '                'End If
        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '入力値(Y/N/-)チェック
        '                    If Not String.IsNullOrEmpty(sStr) AndAlso sStr <> Exist.Yes AndAlso sStr <> Exist.No AndAlso sStr <> Exist.None Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM8)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
        '                        blRet = False
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If
        '                '2023/02/08 Kawamoto UPD END

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM8)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem9
        '                'フリー項目9
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM9")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.FREE_ITEM9).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '2023/02/08 Kawamoto UPD START
        '                'If String.IsNullOrEmpty(sStr) Then
        '                '    '空白ならチェックしない
        '                '    Continue For
        '                'End If
        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '空白チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM9)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                        'マスタ存在チェック
        '                    ElseIf Not Dao.ToolEmployee.ExistEmployeeName(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM9)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_EMPLOYEECD)
        '                        blRet = False
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If
        '                '2023/02/08 Kawamoto UPD END

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM9)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem10
        '                'フリー項目10
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM10")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.FREE_ITEM10).Trim = CommonRfid.ToStringNvl(sStr) Then   '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '2023/02/08 Kawamoto UPD START
        '                'If String.IsNullOrEmpty(sStr) Then
        '                '    '空白ならチェックしない
        '                '    Continue For
        '                'End If
        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '未入力チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '未入力であればフラグON
        '                        internalCapEmptyFlg = True
        '                    Else
        '                        Dim ret As String = CommonRfid.IsDecimal(sStr, 5, 3, False)
        '                        If Not String.IsNullOrEmpty(ret) Then
        '                            '小数値チェック
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM10)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(ret)
        '                            blRet = False
        '                        End If
        '                    End If

        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If
        '                '2023/02/08 Kawamoto UPD END

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM10)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            '2023/02/08 Kawamoto ADD START
        '            Case CsvMapMei_Asset.ColIdx.FreeItem11
        '                'フリー項目11
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM11")).Trim = CommonRfid.ToStringNvl(sStr) Then
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '未入力チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '未入力であればフラグON
        '                        If internalCapEmptyFlg = True Then
        '                            '内容量(kg)と内容量(L)がどちらも値なし
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM11)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_KG_OR_L_ONE_SIDE_INPUT)
        '                            blRet = False
        '                        End If
        '                    Else
        '                        If internalCapEmptyFlg = False Then
        '                            '内容量(kg)と内容量(L)がどちらも値あり
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM11)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_KG_OR_L_ONE_SIDE_INPUT)
        '                            blRet = False

        '                        Else
        '                            Dim ret As String = CommonRfid.IsDecimal(sStr, 5, 3, False)
        '                            If Not String.IsNullOrEmpty(ret) Then
        '                                '小数値チェック
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM11)
        '                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(ret)
        '                                blRet = False
        '                            End If
        '                        End If
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM11)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem12
        '                'フリー項目12
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM12")).Trim = CommonRfid.ToStringNvl(sStr) Then
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '空白チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM12)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    Else
        '                        Dim ret As String = CommonRfid.IsDecimal(sStr, 5, 3, False)
        '                        If Not String.IsNullOrEmpty(ret) Then
        '                            '小数値チェック
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM12)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(ret)
        '                            blRet = False
        '                        End If
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM12)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem13
        '                'フリー項目13
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM13")).Trim = CommonRfid.ToStringNvl(sStr) Then
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '空白チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM13)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    Else
        '                        '日付チェック
        '                        Dim dt As DateTime
        '                        If Not DateTime.TryParse(sStr, dt) Then
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM13)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                            blRet = False
        '                        Else
        '                            'スラッシュ区切りの日付形式に編集する
        '                            Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem13) = CommonRfid.ToStringDateYMD(dt)
        '                        End If
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM13)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem14
        '                'フリー項目14
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM14")).Trim = CommonRfid.ToStringNvl(sStr) Then
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '空白チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM14)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    Else
        '                        Dim ret As String = CommonRfid.IsDecimal(sStr, 5, 3, False)
        '                        If Not String.IsNullOrEmpty(ret) Then
        '                            '小数値チェック
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM14)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(ret)
        '                            blRet = False
        '                        End If
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM14)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem15
        '                'フリー項目15
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM15")).Trim = CommonRfid.ToStringNvl(sStr) Then
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '空白チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM15)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                        'マスタ存在チェック
        '                    ElseIf Not Dao.ToolSetting.ChkExistDrugsWeighingMachine(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM15)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), sStr)
        '                        blRet = False
        '                    End If
        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM15)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem16
        '                'フリー項目16
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM16")).Trim = CommonRfid.ToStringNvl(sStr) Then
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    '空白チェック
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM16)
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        blRet = False
        '                    Else
        '                        Dim ret As String = CommonRfid.IsDecimal(sStr, 5, 3, False)
        '                        If Not String.IsNullOrEmpty(ret) Then
        '                            '小数値チェック
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM16)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(ret)
        '                            blRet = False
        '                        End If
        '                    End If

        '                    '既存の値と異なる場合
        '                    If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM16")).Trim = CommonRfid.ToStringNvl(sStr) Then

        '                        '計量器情報の取得
        '                        Dim dtWeighingMachine As DataTable = Dao.ToolSetting.GetDrugsWeighingMachineInfo()
        '                        Dim listWeighingMachineLink As New List(Of String)
        '                        For Each row As DataRow In dtWeighingMachine.Rows
        '                            If row.Item("DATA1") = Exist.Yes Then
        '                                '計量器連携あり場合、リストに追加
        '                                listWeighingMachineLink.Add(row.Item("DATA3"))
        '                            End If
        '                        Next

        '                        '計量器連携ありの計測器の場合、値の変更不可
        '                        If listWeighingMachineLink.Contains(Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem15).ToString.Trim) Then
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM16)
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_CHANGE_REMAINING_AMOUNT)
        '                            blRet = False
        '                        End If
        '                    End If

        '                Else
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白ならチェックしない
        '                        Continue For
        '                    End If
        '                End If

        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM16)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem17
        '                'フリー項目17
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM17")).Trim = CommonRfid.ToStringNvl(sStr) Then
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '薬品機能がONの場合
        '                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '                    Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem17) = String.Empty
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM17)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem18
        '                'フリー項目18
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM18")).Trim = CommonRfid.ToStringNvl(sStr) Then
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM18)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            Case CsvMapMei_Asset.ColIdx.FreeItem19
        '                'フリー項目19
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("FREE_ITEM19")).Trim = CommonRfid.ToStringNvl(sStr) Then
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.FreeItem Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_FREE_ITEM19)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '            '2023/02/08 Kawamoto ADD END
        '            Case CsvMapMei_Asset.ColIdx.AssetPicFileNm
        '                '既存(除却以外)がある時、既存と値が違えば変更フラグをtrueにする 2019/10/15 miyaji add
        '                If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsAsset(0).Item("ASSET_PIC_FILENAME")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    'If Not model Is Nothing AndAlso model.RETIRE_TS Is Nothing AndAlso Not CommonRfid.ToStringNvl(model.ASSET_PIC_FILENAME).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
        '                End If

        '                '資産画像ファイル名
        '                If String.IsNullOrEmpty(sStr) Then
        '                    '空白ならチェックしない
        '                    Continue For
        '                End If
        '                If sStr.Length > itemlength.AssetPicFileNm Then
        '                    '項目長チェック
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_PIC_FILENAME)
        '                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '                    blRet = False
        '                End If
        '        End Select
        '    Next

        '    'CSVにない項目の設定
        '    'データ項目にエラーがない場合
        '    If blRet Then
        '        'If model Is Nothing OrElse Not model.RETIRE_TS Is Nothing Then     既存DBデータ区分で判断 2019/12/18
        '        If enumDbDataKbn = DbDataKbn.Non Or enumDbDataKbn = DbDataKbn.Removed Then
        '            '新規又は既存が除却
        '            Select Case CInt(CatLrow(0).Item("ASSET_ID_REG_DIV"))
        '                Case AssetIDGenerationMethod.None
        '                    If String.IsNullOrEmpty(Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID).ToString) Then
        '                        Me._dtCsvImport.Rows(0).Item("TAG_LINK_STATUS") = TagLinkStatus.Mi
        '                        Me._dtCsvImport.Rows(0).Item("ASSET_STATUS") = ASSET_STATUS.Preparation
        '                    Else
        '                        Me._dtCsvImport.Rows(0).Item("TAG_LINK_STATUS") = TagLinkStatus.Sumi
        '                        Me._dtCsvImport.Rows(0).Item("ASSET_STATUS") = ASSET_STATUS.Storage
        '                    End If
        '                Case AssetIDGenerationMethod.AssetNo
        '                    Me._dtCsvImport.Rows(0).Item("ASSET_STATUS") = ASSET_STATUS.Storage
        '                    Me._dtCsvImport.Rows(0).Item("TAG_LINK_STATUS") = TagLinkStatus.Sumi
        '                Case AssetIDGenerationMethod.Auto, AssetIDGenerationMethod.GIAI, AssetIDGenerationMethod.GRAI   '2020/02/17 nv miyaji
        '                    'Case AssetIDGenerationMethod.Auto   '2020/02/17 nv miyaji
        '                    Me._dtCsvImport.Rows(0).Item("ASSET_STATUS") = ASSET_STATUS.Preparation
        '                    Me._dtCsvImport.Rows(0).Item("TAG_LINK_STATUS") = TagLinkStatus.Mi
        '                Case AssetIDGenerationMethod.Inp
        '                    Me._dtCsvImport.Rows(0).Item("ASSET_STATUS") = ASSET_STATUS.Storage
        '                    Me._dtCsvImport.Rows(0).Item("TAG_LINK_STATUS") = TagLinkStatus.Sumi
        '            End Select

        '            If String.IsNullOrEmpty(Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID).ToString) Then
        '                Me._dtCsvImport.Rows(0).Item("EPC_ENC_STATUS") = EpcEncStatus.Mi
        '            Else
        '                Me._dtCsvImport.Rows(0).Item("EPC_ENC_STATUS") = EpcEncStatus.Sumi
        '            End If
        '            Me._dtCsvImport.Rows(0).Item("LEASE_CNT") = 0
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_PLACE_CD") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_PLACE_NAME") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("REMARK") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_EMPLOYEE_CD") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_EMPLOYEE_NAME") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_DT") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_DUE_DT") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("RETIRE_TS") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("RETIRE_EMPLOYEE_CD") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("RETIRE_EMPLOYEE_NAME") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("RETIRE_ASSET_ID") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_TS") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_STATUS") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_L_CD") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_M_CD") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_S_CD") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_L_NAME") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_M_NAME") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_S_NAME") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_EMPLOYEE_CD") = DBNull.Value
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_EMPLOYEE_NAME") = DBNull.Value
        '            Me._dtCsvImport.Rows(0).Item("TASK_TS") = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))
        '            Me._dtCsvImport.Rows(0).Item("INS_DATETIME") = CommonRfid.getNowDate
        '            Me._dtCsvImport.Rows(0).Item("INS_EMPLOYEE_CD") = _LoginInfo.EmployeeCd
        '            Me._dtCsvImport.Rows(0).Item("INS_EMPLOYEE_NAME") = _LoginInfo.EmployeeNm
        '            Me._dtCsvImport.Rows(0).Item("UPD_DATETIME") = CommonRfid.getNowDate
        '            Me._dtCsvImport.Rows(0).Item("UPD_EMPLOYEE_CD") = _LoginInfo.EmployeeCd
        '            Me._dtCsvImport.Rows(0).Item("UPD_EMPLOYEE_NAME") = _LoginInfo.EmployeeNm
        '            Me._dtCsvImport.Rows(0).Item("UPD_TASK") = Me._Message.getMessage(CommonLabelAssetList.TITLE)
        '        Else
        '            '既存が除却以外
        '            '--2019/12/19 miyaji update start
        '            Me._dtCsvImport.Rows(0).Item("LEASE_CNT") = rowsAsset(0).Item("LEASE_CNT")
        '            Me._dtCsvImport.Rows(0).Item("ASSET_STATUS") = rowsAsset(0).Item("ASSET_STATUS")
        '            Me._dtCsvImport.Rows(0).Item("EPC_ENC_STATUS") = rowsAsset(0).Item("EPC_ENC_STATUS")
        '            Me._dtCsvImport.Rows(0).Item("TAG_LINK_STATUS") = rowsAsset(0).Item("TAG_LINK_STATUS")
        '            If Not CatLrow Is Nothing AndAlso CInt(CatLrow(0).Item("ASSET_ID_REG_DIV")) = AssetIDGenerationMethod.None Then
        '                If rowsAsset(0).Item("TAG_LINK_STATUS") = TagLinkStatus.Mi AndAlso Not String.IsNullOrEmpty(Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID).ToString) Then
        '                    Me._dtCsvImport.Rows(0).Item("TAG_LINK_STATUS") = TagLinkStatus.Sumi
        '                    Me._dtCsvImport.Rows(0).Item("ASSET_STATUS") = ASSET_STATUS.Storage
        '                    Me._dtCsvImport.Rows(0).Item("EPC_ENC_STATUS") = EpcEncStatus.Sumi
        '                End If
        '            End If
        '            Me._dtCsvImport.Rows(0).Item("LEASE_PLACE_CD") = rowsAsset(0).Item("LEASE_PLACE_CD")
        '            Me._dtCsvImport.Rows(0).Item("LEASE_PLACE_NAME") = rowsAsset(0).Item("LEASE_PLACE_NAME")
        '            Me._dtCsvImport.Rows(0).Item("REMARK") = rowsAsset(0).Item("REMARK")
        '            Me._dtCsvImport.Rows(0).Item("LEASE_EMPLOYEE_CD") = rowsAsset(0).Item("LEASE_EMPLOYEE_CD")
        '            Me._dtCsvImport.Rows(0).Item("LEASE_EMPLOYEE_NAME") = rowsAsset(0).Item("LEASE_EMPLOYEE_NAME")
        '            If Not rowsAsset(0).Item("LEASE_DT") Is Nothing Then
        '                Me._dtCsvImport.Rows(0).Item("LEASE_DT") = rowsAsset(0).Item("LEASE_DT")
        '            End If
        '            If Not rowsAsset(0).Item("LEASE_DUE_DT") Is Nothing Then
        '                Me._dtCsvImport.Rows(0).Item("LEASE_DUE_DT") = rowsAsset(0).Item("LEASE_DUE_DT")
        '            End If
        '            If Not rowsAsset(0).Item("RETIRE_TS") Is Nothing Then
        '                Me._dtCsvImport.Rows(0).Item("RETIRE_TS") = rowsAsset(0).Item("RETIRE_TS")
        '            End If
        '            Me._dtCsvImport.Rows(0).Item("RETIRE_EMPLOYEE_CD") = rowsAsset(0).Item("RETIRE_EMPLOYEE_CD")
        '            Me._dtCsvImport.Rows(0).Item("RETIRE_EMPLOYEE_NAME") = rowsAsset(0).Item("RETIRE_EMPLOYEE_NAME")
        '            Me._dtCsvImport.Rows(0).Item("RETIRE_ASSET_ID") = rowsAsset(0).Item("RETIRE_ASSET_ID")
        '            If Not rowsAsset(0).Item("PHYINV_TS") Is Nothing Then
        '                Me._dtCsvImport.Rows(0).Item("PHYINV_TS") = rowsAsset(0).Item("PHYINV_TS")
        '            End If
        '            Me._dtCsvImport.Rows(0).Item("PHYINV_STATUS") = rowsAsset(0).Item("PHYINV_STATUS")
        '            Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_L_CD") = rowsAsset(0).Item("PHYINV_LOC_L_CD")
        '            Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_M_CD") = rowsAsset(0).Item("PHYINV_LOC_M_CD")
        '            Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_S_CD") = rowsAsset(0).Item("PHYINV_LOC_S_CD")
        '            Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_L_NAME") = rowsAsset(0).Item("PHYINV_LOC_L_NAME")
        '            Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_M_NAME") = rowsAsset(0).Item("PHYINV_LOC_M_NAME")
        '            Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_S_NAME") = rowsAsset(0).Item("PHYINV_LOC_S_NAME")
        '            Me._dtCsvImport.Rows(0).Item("PHYINV_EMPLOYEE_CD") = rowsAsset(0).Item("PHYINV_EMPLOYEE_CD")
        '            Me._dtCsvImport.Rows(0).Item("PHYINV_EMPLOYEE_NAME") = rowsAsset(0).Item("PHYINV_EMPLOYEE_NAME")
        '            Me._dtCsvImport.Rows(0).Item("TASK_TS") = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))
        '            Me._dtCsvImport.Rows(0).Item("INS_DATETIME") = rowsAsset(0).Item("INS_DATETIME")
        '            Me._dtCsvImport.Rows(0).Item("INS_EMPLOYEE_CD") = rowsAsset(0).Item("INS_EMPLOYEE_CD")
        '            Me._dtCsvImport.Rows(0).Item("INS_EMPLOYEE_NAME") = rowsAsset(0).Item("INS_EMPLOYEE_NAME")
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_CNT") = model.LEASE_CNT
        '            'Me._dtCsvImport.Rows(0).Item("ASSET_STATUS") = model.ASSET_STATUS
        '            'Me._dtCsvImport.Rows(0).Item("EPC_ENC_STATUS") = model.EPC_ENC_STATUS
        '            'Me._dtCsvImport.Rows(0).Item("TAG_LINK_STATUS") = model.TAG_LINK_STATUS
        '            'If CInt(CatLrow(0).Item("ASSET_ID_REG_DIV")) = AssetIDGenerationMethod.None Then
        '            '    If model.TAG_LINK_STATUS = TagLinkStatus.Mi AndAlso Not String.IsNullOrEmpty(Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID).ToString) Then
        '            '        Me._dtCsvImport.Rows(0).Item("TAG_LINK_STATUS") = TagLinkStatus.Sumi
        '            '        Me._dtCsvImport.Rows(0).Item("ASSET_STATUS") = ASSET_STATUS.Storage
        '            '        Me._dtCsvImport.Rows(0).Item("EPC_ENC_STATUS") = EpcEncStatus.Sumi
        '            '    End If
        '            'End If
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_PLACE_CD") = model.LEASE_PLACE_CD
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_PLACE_NAME") = model.LEASE_PLACE_NAME
        '            'Me._dtCsvImport.Rows(0).Item("REMARK") = model.REMARK
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_EMPLOYEE_CD") = model.LEASE_EMPLOYEE_CD
        '            'Me._dtCsvImport.Rows(0).Item("LEASE_EMPLOYEE_NAME") = model.LEASE_EMPLOYEE_NAME
        '            'If Not model.LEASE_DT Is Nothing Then
        '            '    Me._dtCsvImport.Rows(0).Item("LEASE_DT") = model.LEASE_DT
        '            'End If
        '            'If Not model.LEASE_DUE_DT Is Nothing Then
        '            '    Me._dtCsvImport.Rows(0).Item("LEASE_DUE_DT") = model.LEASE_DUE_DT
        '            'End If
        '            'If Not model.RETIRE_TS Is Nothing Then
        '            '    Me._dtCsvImport.Rows(0).Item("RETIRE_TS") = model.RETIRE_TS
        '            'End If
        '            'Me._dtCsvImport.Rows(0).Item("RETIRE_EMPLOYEE_CD") = model.RETIRE_EMPLOYEE_CD
        '            'Me._dtCsvImport.Rows(0).Item("RETIRE_EMPLOYEE_NAME") = model.RETIRE_EMPLOYEE_NAME
        '            'Me._dtCsvImport.Rows(0).Item("RETIRE_ASSET_ID") = model.RETIRE_ASSET_ID
        '            'If Not model.PHYINV_TS Is Nothing Then
        '            '    Me._dtCsvImport.Rows(0).Item("PHYINV_TS") = model.PHYINV_TS
        '            'End If
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_STATUS") = model.PHYINV_STATUS
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_L_CD") = model.PHYINV_LOC_L_CD
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_M_CD") = model.PHYINV_LOC_M_CD
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_S_CD") = model.PHYINV_LOC_S_CD
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_L_NAME") = model.PHYINV_LOC_L_NAME
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_M_NAME") = model.PHYINV_LOC_M_NAME
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_LOC_S_NAME") = model.PHYINV_LOC_S_NAME
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_EMPLOYEE_CD") = model.PHYINV_EMPLOYEE_CD
        '            'Me._dtCsvImport.Rows(0).Item("PHYINV_EMPLOYEE_NAME") = model.PHYINV_EMPLOYEE_NAME
        '            'Me._dtCsvImport.Rows(0).Item("TASK_TS") = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))
        '            'Me._dtCsvImport.Rows(0).Item("INS_DATETIME") = model.INS_DATETIME
        '            'Me._dtCsvImport.Rows(0).Item("INS_EMPLOYEE_CD") = model.INS_EMPLOYEE_CD
        '            'Me._dtCsvImport.Rows(0).Item("INS_EMPLOYEE_NAME") = model.INS_EMPLOYEE_NAME
        '            '--2019/12/19 miyaji update end
        '            Me._dtCsvImport.Rows(0).Item("UPD_DATETIME") = CommonRfid.getNowDate
        '            Me._dtCsvImport.Rows(0).Item("UPD_EMPLOYEE_CD") = _LoginInfo.EmployeeCd
        '            Me._dtCsvImport.Rows(0).Item("UPD_EMPLOYEE_NAME") = _LoginInfo.EmployeeNm
        '            Me._dtCsvImport.Rows(0).Item("UPD_TASK") = Me._Message.getMessage(CommonLabelAssetList.TITLE)
        '        End If

        '        '2023/02/08 Kawamoto ADD START
        '        '薬品機能がONの場合
        '        If DrugsFunc.DATA1 = DrugsFunction.Yes Then
        '            'FreeItem17(残量割合(残量/内容量))をセット
        '            Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem17) = CalsConsumptionRate(Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem10),
        '                                                                                                  Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem11),
        '                                                                                                  Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem12),
        '                                                                                                  Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem14),
        '                                                                                                  Me._dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem16))
        '        End If
        '        '2023/02/08 Kawamoto ADD END
        '    End If

        '    'オブジェクトの編集終了
        '    Me._dtCsvImport.Rows(0).EndEdit()

        '    Return blRet

        'End Function
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】IsChkAssetId（資産IDの基本エラーチェック）"
'#Region "資産IDの基本エラーチェック"
        '''' <summary>
        '''' 資産IDの基本エラーチェック
        '''' </summary>
        '''' <param name="iRowCnt">データ行</param>
        '''' <param name="sAssetId">資産ID</param>
        '''' <returns>True:チェックOK False:チェックNG</returns>
        '''' <remarks></remarks>
        'Private Function IsChkAssetId(ByVal iRowCnt As Integer, ByVal sAssetId As String) As Boolean
        '    Dim isRet As Boolean = False

        '    If sAssetId.Length > itemlength.AssetID Then
        '        '項目長チェック
        '        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
        '    ElseIf Not CommonRfid.IsHankaku(sAssetId) Then
        '        '半角英数字チェック
        '        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '    ElseIf IsHex(sAssetId) Then
        '        '16進数チェック
        '        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_HEX)
        '        '--2020/02/12 nv miyaji コメントアウト start　長さチェックするようにしたので削除
        '        'ElseIf Not sAssetId.Length Mod 2 = 0 Then
        '        '    '文字の長さが奇数の場合エラー
        '        '    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
        '        '    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_ID)
        '        '    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_HEX)
        '        '--2020/02/12 nv miyaji コメントアウト end
        '    Else
        '        isRet = True
        '    End If

        '    Return isRet

        'End Function
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】InsertWork（一時表に登録）"
'#Region "一時表に登録"
        '''' <summary>
        '''' 一時表に登録
        '''' </summary>
        '''' <param name="icnt">登録する行</param>
        '''' <param name="sTmpTblNm">登録するデータテーブルのテーブル名</param>
        '''' <param name="db">db</param>
        'Public Sub InsertWork(icnt As Integer, sTmpTblNm As String, db As SystemDBContext, assetAutoRegFlg As Boolean, assetNoSeq As Integer)
        '    '--登録処理開始
        '    Dim model As List(Of AssetListWorkModel) = New List(Of AssetListWorkModel)    '登録用モデル

        '    Dim sbSQL As StringBuilder = New StringBuilder
        '    Dim params As List(Of SqlParameter)

        '    Try

        '        '2020/03/12 DELETE INOUE 一時表作成を必ず作成するようにするため、上位関数で行なう
        '        '' '作業用資産データテーブルを全削除
        '        ''db.Database.ExecuteSqlCommand("DELETE FROM W_ASSET")

        '        ''一番最初のみテーブル作成　& 列追加をする(一番最初に来るのが2)
        '        'If icnt = 2 Then
        '        '    '一時表作成
        '        '    Dim sCreate As String = String.Format("Select * INTO {0} FROM M_ASSET WHERE 1 = 0", sTmpTblNm)
        '        '    db.Database.ExecuteSqlCommand(sCreate)

        '        '    Dim sAddColumn As String
        '        '    ''一時表に、列「SEQNO」を追加する  (CSVファイルの行NO)
        '        '    'sAddColumn = String.Format("ALTER TABLE {0} ADD UPD_FLG bigint DEFAULT 0", sTmpTblNm)
        '        '    'db.Database.ExecuteSqlCommand(sAddColumn)
        '        '    '一時表に、列「UPD_FLG」を追加する　(0:変更なし 1:変更あり  ※新規登録行はこの区分を使用しない)
        '        '    sAddColumn = String.Format("ALTER TABLE {0} ADD UPD_FLG Bit DEFAULT 0", sTmpTblNm)
        '        '    db.Database.ExecuteSqlCommand(sAddColumn)
        '        'End If
        '        '2020/03/12 DELETE INOUE 一時表作成を必ず作成するようにするため、上位関数で行なう　END

        '        'Insert SQL 作成
        '        'sbSQL.AppendLine("INSERT INTO W_ASSET")
        '        sbSQL.AppendLine("INSERT INTO " & sTmpTblNm)
        '        sbSQL.AppendLine("(")

        '        sbSQL.AppendLine("     ACTIVITY")       '2023/02/08 Kawamoto ADD
        '        sbSQL.AppendLine("    ,ASSET_NO")
        '        sbSQL.AppendLine("    ,ASSET_ID")
        '        sbSQL.AppendLine("    ,EPC_SIZE")
        '        sbSQL.AppendLine("    ,GEN_ASSET_ID")
        '        sbSQL.AppendLine("    ,ASSET_DIV")
        '        sbSQL.AppendLine("    ,ASSET_NAME")
        '        sbSQL.AppendLine("    ,MNG_LOC_L_CD")   '2021/10/04 nv miyaji add
        '        sbSQL.AppendLine("    ,LOC_L_CD")
        '        sbSQL.AppendLine("    ,LOC_M_CD")
        '        sbSQL.AppendLine("    ,LOC_S_CD")
        '        sbSQL.AppendLine("    ,CAT_L_CD")
        '        sbSQL.AppendLine("    ,CAT_M_CD")
        '        sbSQL.AppendLine("    ,CAT_S_CD")
        '        sbSQL.AppendLine("    ,MAKER")
        '        sbSQL.AppendLine("    ,PRODUCT_NO")
        '        sbSQL.AppendLine("    ,MODEL_NO")
        '        sbSQL.AppendLine("    ,LEASE_NO")
        '        sbSQL.AppendLine("    ,LEASE_LIMIT")
        '        sbSQL.AppendLine("    ,GENERIC_LIMIT")
        '        sbSQL.AppendLine("    ,FREE_ITEM1")
        '        sbSQL.AppendLine("    ,FREE_ITEM2")
        '        sbSQL.AppendLine("    ,FREE_ITEM3")
        '        sbSQL.AppendLine("    ,FREE_ITEM4")
        '        sbSQL.AppendLine("    ,FREE_ITEM5")
        '        sbSQL.AppendLine("    ,FREE_ITEM6")
        '        sbSQL.AppendLine("    ,FREE_ITEM7")
        '        sbSQL.AppendLine("    ,FREE_ITEM8")
        '        sbSQL.AppendLine("    ,FREE_ITEM9")
        '        sbSQL.AppendLine("    ,FREE_ITEM10")
        '        '2023/02/08 Kawamoto ADD START
        '        sbSQL.AppendLine("    ,FREE_ITEM11")
        '        sbSQL.AppendLine("    ,FREE_ITEM12")
        '        sbSQL.AppendLine("    ,FREE_ITEM13")
        '        sbSQL.AppendLine("    ,FREE_ITEM14")
        '        sbSQL.AppendLine("    ,FREE_ITEM15")
        '        sbSQL.AppendLine("    ,FREE_ITEM16")
        '        sbSQL.AppendLine("    ,FREE_ITEM17")
        '        sbSQL.AppendLine("    ,FREE_ITEM18")
        '        sbSQL.AppendLine("    ,FREE_ITEM19")
        '        '2023/02/08 Kawamoto ADD END
        '        sbSQL.AppendLine("    ,ASSET_PIC_FILENAME")
        '        sbSQL.AppendLine("    ,LEASE_CNT")
        '        sbSQL.AppendLine("    ,ASSET_STATUS")
        '        sbSQL.AppendLine("    ,EPC_ENC_STATUS")
        '        sbSQL.AppendLine("    ,TAG_LINK_STATUS")
        '        sbSQL.AppendLine("    ,LEASE_STATUS")   '2021/10/04 nv miyaji add
        '        sbSQL.AppendLine("    ,LEASE_PLACE_CD")
        '        sbSQL.AppendLine("    ,LEASE_PLACE_NAME")
        '        sbSQL.AppendLine("    ,REMARK")
        '        sbSQL.AppendLine("    ,LEASE_EMPLOYEE_CD")
        '        sbSQL.AppendLine("    ,LEASE_EMPLOYEE_NAME")
        '        sbSQL.AppendLine("    ,LEASE_DT")
        '        sbSQL.AppendLine("    ,LEASE_DUE_DT")
        '        '--2021/10/04 nv miyaji add start
        '        sbSQL.AppendLine("    ,MAINTE_LEASE_NUM")
        '        sbSQL.AppendLine("    ,MAINTE_LEASE_DAY")
        '        sbSQL.AppendLine("    ,MAINTE_LEASE_DAY_WORK_FLAG")
        '        sbSQL.AppendLine("    ,MAINTE_DAY")
        '        sbSQL.AppendLine("    ,MAINTE_DAY_WORK_FLAG")
        '        '--2021/10/04 nv miyaji add end
        '        sbSQL.AppendLine("    ,RETIRE_TS")
        '        sbSQL.AppendLine("    ,RETIRE_EMPLOYEE_CD")
        '        sbSQL.AppendLine("    ,RETIRE_EMPLOYEE_NAME")
        '        sbSQL.AppendLine("    ,RETIRE_ASSET_ID")
        '        sbSQL.AppendLine("    ,PHYINV_TS")
        '        sbSQL.AppendLine("    ,PHYINV_STATUS")
        '        sbSQL.AppendLine("    ,PHYINV_LOC_L_CD")
        '        sbSQL.AppendLine("    ,PHYINV_LOC_M_CD")
        '        sbSQL.AppendLine("    ,PHYINV_LOC_S_CD")
        '        sbSQL.AppendLine("    ,PHYINV_LOC_L_NAME")
        '        sbSQL.AppendLine("    ,PHYINV_LOC_M_NAME")
        '        sbSQL.AppendLine("    ,PHYINV_LOC_S_NAME")
        '        sbSQL.AppendLine("    ,PHYINV_EMPLOYEE_CD")
        '        sbSQL.AppendLine("    ,PHYINV_EMPLOYEE_NAME")
        '        sbSQL.AppendLine("    ,TASK_TS")
        '        sbSQL.AppendLine("    ,INS_DATETIME")
        '        sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
        '        sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
        '        sbSQL.AppendLine("    ,UPD_DATETIME")
        '        sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
        '        sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
        '        sbSQL.AppendLine("    ,UPD_TASK")
        '        sbSQL.AppendLine("    ,UPD_FLG")                                       '2019/10/15 ADD Inoue
        '        sbSQL.AppendLine(") VALUES (")

        '        sbSQL.AppendLine("     @ACTIVITY")      '2023/02/08 Kawamoto ADD
        '        sbSQL.AppendLine("    ,@ASSET_NO")
        '        sbSQL.AppendLine("    ,@ASSET_ID")
        '        sbSQL.AppendLine("    ,@EPC_SIZE")
        '        sbSQL.AppendLine("    ,@GEN_ASSET_ID")
        '        sbSQL.AppendLine("    ,@ASSET_DIV")
        '        sbSQL.AppendLine("    ,@ASSET_NAME")
        '        sbSQL.AppendLine("    ,@MNG_LOC_L_CD")   '2021/10/04 nv miyaji add
        '        sbSQL.AppendLine("    ,@LOC_L_CD")
        '        sbSQL.AppendLine("    ,@LOC_M_CD")
        '        sbSQL.AppendLine("    ,@LOC_S_CD")
        '        sbSQL.AppendLine("    ,@CAT_L_CD")
        '        sbSQL.AppendLine("    ,@CAT_M_CD")
        '        sbSQL.AppendLine("    ,@CAT_S_CD")
        '        sbSQL.AppendLine("    ,@MAKER")
        '        sbSQL.AppendLine("    ,@PRODUCT_NO")
        '        sbSQL.AppendLine("    ,@MODEL_NO")
        '        sbSQL.AppendLine("    ,@LEASE_NO")
        '        sbSQL.AppendLine("    ,@LEASE_LIMIT")
        '        sbSQL.AppendLine("    ,@GENERIC_LIMIT")
        '        sbSQL.AppendLine("    ,@FREE_ITEM1")
        '        sbSQL.AppendLine("    ,@FREE_ITEM2")
        '        sbSQL.AppendLine("    ,@FREE_ITEM3")
        '        sbSQL.AppendLine("    ,@FREE_ITEM4")
        '        sbSQL.AppendLine("    ,@FREE_ITEM5")
        '        sbSQL.AppendLine("    ,@FREE_ITEM6")
        '        sbSQL.AppendLine("    ,@FREE_ITEM7")
        '        sbSQL.AppendLine("    ,@FREE_ITEM8")
        '        sbSQL.AppendLine("    ,@FREE_ITEM9")
        '        sbSQL.AppendLine("    ,@FREE_ITEM10")
        '        '2023/02/08 Kawamoto ADD START
        '        sbSQL.AppendLine("    ,@FREE_ITEM11")
        '        sbSQL.AppendLine("    ,@FREE_ITEM12")
        '        sbSQL.AppendLine("    ,@FREE_ITEM13")
        '        sbSQL.AppendLine("    ,@FREE_ITEM14")
        '        sbSQL.AppendLine("    ,@FREE_ITEM15")
        '        sbSQL.AppendLine("    ,@FREE_ITEM16")
        '        sbSQL.AppendLine("    ,@FREE_ITEM17")
        '        sbSQL.AppendLine("    ,@FREE_ITEM18")
        '        sbSQL.AppendLine("    ,@FREE_ITEM19")
        '        '2023/02/08 Kawamoto ADD END
        '        sbSQL.AppendLine("    ,@ASSET_PIC_FILENAME")
        '        sbSQL.AppendLine("    ,@LEASE_CNT")
        '        sbSQL.AppendLine("    ,@ASSET_STATUS")
        '        sbSQL.AppendLine("    ,@EPC_ENC_STATUS")
        '        sbSQL.AppendLine("    ,@TAG_LINK_STATUS")
        '        sbSQL.AppendLine("    ,@LEASE_STATUS")   '2021/10/04 nv miyaji add
        '        sbSQL.AppendLine("    ,@LEASE_PLACE_CD")
        '        sbSQL.AppendLine("    ,@LEASE_PLACE_NAME")
        '        sbSQL.AppendLine("    ,@REMARK")
        '        sbSQL.AppendLine("    ,@LEASE_EMPLOYEE_CD")
        '        sbSQL.AppendLine("    ,@LEASE_EMPLOYEE_NAME")
        '        sbSQL.AppendLine("    ,@LEASE_DT")
        '        sbSQL.AppendLine("    ,@LEASE_DUE_DT")
        '        '--2021/10/04 nv miyaji add start
        '        sbSQL.AppendLine("    ,@MAINTE_LEASE_NUM")
        '        sbSQL.AppendLine("    ,@MAINTE_LEASE_DAY")
        '        sbSQL.AppendLine("    ,@MAINTE_LEASE_DAY_WORK_FLAG")
        '        sbSQL.AppendLine("    ,@MAINTE_DAY")
        '        sbSQL.AppendLine("    ,@MAINTE_DAY_WORK_FLAG")
        '        '--2021/10/04 nv miyaji add start
        '        sbSQL.AppendLine("    ,@RETIRE_TS")
        '        sbSQL.AppendLine("    ,@RETIRE_EMPLOYEE_CD")
        '        sbSQL.AppendLine("    ,@RETIRE_EMPLOYEE_NAME")
        '        sbSQL.AppendLine("    ,@RETIRE_ASSET_ID")
        '        sbSQL.AppendLine("    ,@PHYINV_TS")
        '        sbSQL.AppendLine("    ,@PHYINV_STATUS")
        '        sbSQL.AppendLine("    ,@PHYINV_LOC_L_CD")
        '        sbSQL.AppendLine("    ,@PHYINV_LOC_M_CD")
        '        sbSQL.AppendLine("    ,@PHYINV_LOC_S_CD")
        '        sbSQL.AppendLine("    ,@PHYINV_LOC_L_NAME")
        '        sbSQL.AppendLine("    ,@PHYINV_LOC_M_NAME")
        '        sbSQL.AppendLine("    ,@PHYINV_LOC_S_NAME")
        '        sbSQL.AppendLine("    ,@PHYINV_EMPLOYEE_CD")
        '        sbSQL.AppendLine("    ,@PHYINV_EMPLOYEE_NAME")
        '        sbSQL.AppendLine("    ,@TASK_TS")
        '        sbSQL.AppendLine("    ,@INS_DATETIME")
        '        sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
        '        sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
        '        sbSQL.AppendLine("    ,@UPD_DATETIME")
        '        sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
        '        sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
        '        sbSQL.AppendLine("    ,@UPD_TASK")
        '        sbSQL.AppendLine("    ,@UPD_FLG")                   '2019/10/15 ADD Inoue
        '        sbSQL.AppendLine(")")

        '        'パラメーターにデータを入れる

        '        'model.Add(New AssetListWorkModel())
        '        params = New List(Of SqlParameter)

        '        params.Add(New SqlParameter($"@ACTIVITY", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.Activity).ToString.Trim))    '2023/02/08 Kawamoto ADD

        '        If assetAutoRegFlg Then
        '            '資産番号自動生成あり
        '            params.Add(New SqlParameter($"@ASSET_NO", AssetNoAutoGenelate(assetNoSeq)))
        '        Else
        '            '資産番号自動生成なし
        '            params.Add(New SqlParameter($"@ASSET_NO", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetNo).ToString.Trim))
        '        End If

        '        params.Add(New SqlParameter($"@ASSET_ID", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetID).ToString.Trim))
        '        If IsNumeric(_dtCsvImport.Rows(0).Item("EPC_SIZE")) Then
        '            params.Add(New SqlParameter($"@EPC_SIZE", CInt(_dtCsvImport.Rows(0).Item("EPC_SIZE"))))
        '        Else
        '            params.Add(New SqlParameter($"@EPC_SIZE", 0))
        '        End If
        '        params.Add(New SqlParameter($"@GEN_ASSET_ID", _dtCsvImport.Rows(0).Item("GEN_ASSET_ID").ToString.Trim))
        '        params.Add(New SqlParameter($"@ASSET_DIV", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetDiv).ToString.Trim))
        '        params.Add(New SqlParameter($"@ASSET_NAME", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetNm).ToString.Trim))
        '        params.Add(New SqlParameter($"@MNG_LOC_L_CD", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MngLocLCd).ToString.Trim))   '2021/10/04 nv miyaji add
        '        params.Add(New SqlParameter($"@LOC_L_CD", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocLCd).ToString.Trim))
        '        If _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocMCd).ToString.Trim = String.Empty Then
        '            params.Add(New SqlParameter($"@LOC_M_CD", " "))
        '        Else
        '            params.Add(New SqlParameter($"@LOC_M_CD", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocMCd).ToString.Trim))
        '        End If
        '        If _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocSCd).ToString.Trim = String.Empty Then
        '            params.Add(New SqlParameter($"@LOC_S_CD", " "))
        '        Else
        '            params.Add(New SqlParameter($"@LOC_S_CD", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LocSCd).ToString.Trim))
        '        End If
        '        params.Add(New SqlParameter($"@CAT_L_CD", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatLCd).ToString.Trim))
        '        If _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatMCd).ToString.Trim = String.Empty Then
        '            params.Add(New SqlParameter($"@CAT_M_CD", " "))
        '        Else
        '            params.Add(New SqlParameter($"@CAT_M_CD", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatMCd).ToString.Trim))
        '        End If
        '        If _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatSCd).ToString.Trim = String.Empty Then
        '            params.Add(New SqlParameter($"@CAT_S_CD", " "))
        '        Else
        '            params.Add(New SqlParameter($"@CAT_S_CD", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.CatSCd).ToString.Trim))
        '        End If

        '        params.Add(New SqlParameter($"@MAKER", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.Maker).ToString.Trim))
        '        params.Add(New SqlParameter($"@PRODUCT_NO", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.ProductNo).ToString.Trim))
        '        params.Add(New SqlParameter($"@MODEL_NO", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.ModelNo).ToString.Trim))
        '        params.Add(New SqlParameter($"@LEASE_NO", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LeaseNo).ToString.Trim))
        '        Dim dtLease As DateTime
        '        If DateTime.TryParse(_dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LeaseLimit).ToString.Trim, dtLease) Then
        '            params.Add(New SqlParameter($"@LEASE_LIMIT", CInt(dtLease.ToString("yyyyMMdd"))))
        '        Else
        '            params.Add(New SqlParameter($"@LEASE_LIMIT", DBNull.Value))
        '        End If
        '        Dim dtGen As DateTime
        '        If DateTime.TryParse(_dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.GenLimit).ToString.Trim, dtGen) Then
        '            params.Add(New SqlParameter($"@GENERIC_LIMIT", CInt(dtGen.ToString("yyyyMMdd"))))
        '        Else
        '            params.Add(New SqlParameter($"@GENERIC_LIMIT", DBNull.Value))
        '        End If
        '        params.Add(New SqlParameter($"@FREE_ITEM1", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem1).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM2", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem2).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM3", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem3).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM4", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem4).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM5", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem5).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM6", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem6).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM7", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem7).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM8", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem8).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM9", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem9).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM10", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem10).ToString.Trim))
        '        '2023/02/08 Kawamoto ADD START
        '        params.Add(New SqlParameter($"@FREE_ITEM11", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem11).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM12", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem12).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM13", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem13).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM14", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem14).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM15", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem15).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM16", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem16).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM17", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem17).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM18", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem18).ToString.Trim))
        '        params.Add(New SqlParameter($"@FREE_ITEM19", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.FreeItem19).ToString.Trim))
        '        '2023/02/08 Kawamoto ADD START
        '        params.Add(New SqlParameter($"@ASSET_PIC_FILENAME", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.AssetPicFileNm).ToString.Trim))

        '        params.Add(New SqlParameter($"@LEASE_CNT", _dtCsvImport.Rows(0).Item("LEASE_CNT").ToString.Trim))
        '        params.Add(New SqlParameter($"@ASSET_STATUS", _dtCsvImport.Rows(0).Item("ASSET_STATUS").ToString.Trim))
        '        params.Add(New SqlParameter($"@EPC_ENC_STATUS", _dtCsvImport.Rows(0).Item("EPC_ENC_STATUS").ToString.Trim))
        '        params.Add(New SqlParameter($"@TAG_LINK_STATUS", _dtCsvImport.Rows(0).Item("TAG_LINK_STATUS").ToString.Trim))
        '        '--2021/11/11 nv miyaji update start
        '        If _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LeaseStatus).ToString.Trim = LeaseStatusAsset_Csv.Kanou Then
        '            params.Add(New SqlParameter($"@LEASE_STATUS", LeaseStatusAsset.Kanou.ToString))
        '        Else
        '            params.Add(New SqlParameter($"@LEASE_STATUS", LeaseStatusAsset.Fuka.ToString))
        '        End If
        '        'params.Add(New SqlParameter($"@LEASE_STATUS", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.LeaseStatus).ToString.Trim))   '2021/10/04 nv miyaji add
        '        '--2021/11/11 nv miyaji update end
        '        params.Add(New SqlParameter($"@LEASE_PLACE_CD", _dtCsvImport.Rows(0).Item("LEASE_PLACE_CD").ToString.Trim))
        '        params.Add(New SqlParameter($"@LEASE_PLACE_NAME", _dtCsvImport.Rows(0).Item("LEASE_PLACE_NAME").ToString.Trim))
        '        params.Add(New SqlParameter($"@REMARK", _dtCsvImport.Rows(0).Item("REMARK").ToString.Trim))
        '        params.Add(New SqlParameter($"@LEASE_EMPLOYEE_CD", _dtCsvImport.Rows(0).Item("LEASE_EMPLOYEE_CD").ToString.Trim))
        '        params.Add(New SqlParameter($"@LEASE_EMPLOYEE_NAME", _dtCsvImport.Rows(0).Item("LEASE_EMPLOYEE_NAME").ToString.Trim))
        '        Dim dtLeasedt As DateTime
        '        If DateTime.TryParse(_dtCsvImport.Rows(0).Item("LEASE_DT").ToString.Trim, dtLeasedt) Then
        '            params.Add(New SqlParameter($"@LEASE_DT", _dtCsvImport.Rows(0).Item("LEASE_DT").ToString.Trim))
        '        Else
        '            params.Add(New SqlParameter($"@LEASE_DT", DBNull.Value))
        '        End If
        '        Dim dtLeaseDueDt As DateTime
        '        If DateTime.TryParse(_dtCsvImport.Rows(0).Item("LEASE_DUE_DT").ToString.Trim, dtLeaseDueDt) Then
        '            params.Add(New SqlParameter($"@LEASE_DUE_DT", _dtCsvImport.Rows(0).Item("LEASE_DUE_DT").ToString.Trim))
        '        Else
        '            params.Add(New SqlParameter($"@LEASE_DUE_DT", DBNull.Value))
        '        End If
        '        '--2021/10/04 nv miyaji add start
        '        '--2021/12/01 nv miyaji update start
        '        If _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseNum).ToString.Trim = String.Empty Then
        '            params.Add(New SqlParameter($"@MAINTE_LEASE_NUM", DBNull.Value))
        '        Else
        '            params.Add(New SqlParameter($"@MAINTE_LEASE_NUM", CInt(_dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseNum).ToString.Trim)))
        '        End If
        '        'params.Add(New SqlParameter($"@MAINTE_LEASE_NUM", CInt(_dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseNum).ToString.Trim)))
        '        '--2021/12/01 nv miyaji update end
        '        '--2021/11/12 nv miyaji update start
        '        If _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseDay).ToString.Trim = String.Empty Then
        '            params.Add(New SqlParameter($"@MAINTE_LEASE_DAY", DBNull.Value))
        '        Else
        '            params.Add(New SqlParameter($"@MAINTE_LEASE_DAY", CInt(_dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseDay).ToString.Trim)))
        '        End If
        '        'params.Add(New SqlParameter($"@MAINTE_LEASE_DAY", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseDay).ToString.Trim))
        '        If _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseDayWorkFlg).ToString.Trim = String.Empty Then
        '            params.Add(New SqlParameter($"@MAINTE_LEASE_DAY_WORK_FLAG", DBNull.Value))
        '        Else
        '            params.Add(New SqlParameter($"@MAINTE_LEASE_DAY_WORK_FLAG", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseDayWorkFlg).ToString.Trim))
        '        End If
        '        'params.Add(New SqlParameter($"@MAINTE_LEASE_DAY_WORK_FLAG", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteLeaseDayWorkFlg).ToString.Trim))
        '        '--2021/11/12 nv miyaji update end
        '        '--2021/11/12 nv miyaji update start
        '        If _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteDay).ToString.Trim = String.Empty Then
        '            params.Add(New SqlParameter($"@MAINTE_DAY", DBNull.Value))
        '        Else
        '            params.Add(New SqlParameter($"@MAINTE_DAY", CInt(_dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteDay).ToString.Trim)))
        '        End If
        '        'params.Add(New SqlParameter($"@MAINTE_DAY", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteDay).ToString.Trim))
        '        If _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteDayWorkFlg).ToString.Trim = String.Empty Then
        '            params.Add(New SqlParameter($"@MAINTE_DAY_WORK_FLAG", DBNull.Value))
        '        Else
        '            params.Add(New SqlParameter($"@MAINTE_DAY_WORK_FLAG", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteDayWorkFlg).ToString.Trim))
        '        End If
        '        'params.Add(New SqlParameter($"@MAINTE_DAY_WORK_FLAG", _dtCsvImport.Rows(0).Item(CsvMapMei_Asset.ColIdx.MainteDayWorkFlg).ToString.Trim))
        '        '--2021/11/12 nv miyaji update end
        '        '--2021/10/04 nv miyaji add end
        '        If Not String.IsNullOrEmpty(_dtCsvImport.Rows(0).Item("RETIRE_TS").ToString.Trim) Then
        '            params.Add(New SqlParameter($"@RETIRE_TS", _dtCsvImport.Rows(0).Item("RETIRE_TS").ToString.Trim))
        '        Else
        '            params.Add(New SqlParameter($"@RETIRE_TS", DBNull.Value))
        '        End If
        '        params.Add(New SqlParameter($"@RETIRE_EMPLOYEE_CD", _dtCsvImport.Rows(0).Item("RETIRE_EMPLOYEE_CD").ToString.Trim))
        '        params.Add(New SqlParameter($"@RETIRE_EMPLOYEE_NAME", _dtCsvImport.Rows(0).Item("RETIRE_EMPLOYEE_NAME").ToString.Trim))
        '        params.Add(New SqlParameter($"@RETIRE_ASSET_ID", _dtCsvImport.Rows(0).Item("RETIRE_ASSET_ID").ToString.Trim))
        '        If Not String.IsNullOrEmpty(_dtCsvImport.Rows(0).Item("PHYINV_TS").ToString.Trim) Then
        '            params.Add(New SqlParameter($"@PHYINV_TS", _dtCsvImport.Rows(0).Item("PHYINV_TS").ToString.Trim))
        '        Else
        '            params.Add(New SqlParameter($"@PHYINV_TS", DBNull.Value))
        '        End If
        '        params.Add(New SqlParameter($"@PHYINV_STATUS", _dtCsvImport.Rows(0).Item("PHYINV_STATUS").ToString.Trim))
        '        params.Add(New SqlParameter($"@PHYINV_LOC_L_CD", _dtCsvImport.Rows(0).Item("PHYINV_LOC_L_CD").ToString.Trim))
        '        params.Add(New SqlParameter($"@PHYINV_LOC_M_CD", _dtCsvImport.Rows(0).Item("PHYINV_LOC_M_CD").ToString.Trim))
        '        params.Add(New SqlParameter($"@PHYINV_LOC_S_CD", _dtCsvImport.Rows(0).Item("PHYINV_LOC_S_CD").ToString.Trim))
        '        params.Add(New SqlParameter($"@PHYINV_LOC_L_NAME", _dtCsvImport.Rows(0).Item("PHYINV_LOC_L_NAME").ToString.Trim))
        '        params.Add(New SqlParameter($"@PHYINV_LOC_M_NAME", _dtCsvImport.Rows(0).Item("PHYINV_LOC_M_NAME").ToString.Trim))
        '        params.Add(New SqlParameter($"@PHYINV_LOC_S_NAME", _dtCsvImport.Rows(0).Item("PHYINV_LOC_S_NAME").ToString.Trim))
        '        params.Add(New SqlParameter($"@PHYINV_EMPLOYEE_CD", _dtCsvImport.Rows(0).Item("PHYINV_EMPLOYEE_CD").ToString.Trim))
        '        params.Add(New SqlParameter($"@PHYINV_EMPLOYEE_NAME", _dtCsvImport.Rows(0).Item("PHYINV_EMPLOYEE_NAME").ToString.Trim))
        '        If Not String.IsNullOrEmpty(_dtCsvImport.Rows(0).Item("TASK_TS").ToString.Trim) Then
        '            params.Add(New SqlParameter($"@TASK_TS", _dtCsvImport.Rows(0).Item("TASK_TS").ToString.Trim))
        '        Else
        '            params.Add(New SqlParameter($"@TASK_TS", DBNull.Value))
        '        End If
        '        params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
        '        params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _dtCsvImport.Rows(0).Item("INS_EMPLOYEE_CD").ToString.Trim))
        '        params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _dtCsvImport.Rows(0).Item("INS_EMPLOYEE_NAME").ToString.Trim))
        '        params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
        '        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
        '        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
        '        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)))    '2019/11/21 nv miyaji 最終登録更新処理に画面名ではなく処理名を入れる
        '        'params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelAssetList.TITLE)))   '2019/11/21 nv miyaji 最終登録更新処理に画面名ではなく処理名を入れる

        '        params.Add(New SqlParameter($"@UPD_FLG", _dtCsvImport.Rows(0).Item("UPD_FLG")))

        '        db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

        '    Catch ex As Exception
        '        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        '    End Try

        'End Sub

'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】CreatetblImportCsv（CSV登録用データテーブルの作成）"
'#Region "CSV登録用データテーブルの作成"
        '''' <summary>
        '''' CSV登録用データテーブルの作成
        '''' </summary>
        'Public Sub CreatetblImportCsv()
        '    'CSVマッピング明細マスタで定義されている項目の列を追加
        '    _dtCsvImport = New DataTable("AssetList")
        '    For icnt As Integer = 0 To _lstCsvHed.Count - 1
        '        _dtCsvImport.Columns.Add(_lstCsvHed(icnt).ToString, GetType(String))
        '    Next

        '    '資産マスタ登録に足りない列を追加する
        '    Dim dcEpc = New DataColumn
        '    Dim dcGenAssetId = New DataColumn
        '    Dim dcLeaseCnt = New DataColumn
        '    Dim dcAssetStatus = New DataColumn
        '    Dim dcEpcEncStatus = New DataColumn
        '    Dim dcTagLinkStatus = New DataColumn
        '    Dim dcLeasePlaceCd = New DataColumn
        '    Dim dcLeasePlaceNm = New DataColumn
        '    Dim dcRemark = New DataColumn
        '    Dim dcLeaseEmployeeCd = New DataColumn
        '    Dim dcLeaseEmployeeNm = New DataColumn
        '    Dim dcLeaseDt = New DataColumn
        '    Dim dcLeaseDueDt = New DataColumn
        '    Dim dcRetireTs = New DataColumn
        '    Dim dcRetireEmployeeCd = New DataColumn
        '    Dim dcRetireEmployeeNm = New DataColumn
        '    Dim dcRetireAssetId = New DataColumn
        '    Dim dcPhyinvTs = New DataColumn
        '    Dim dcPhyinvStatus = New DataColumn
        '    Dim dcPhyinvLocLCd = New DataColumn
        '    Dim dcPhyinvLocMCd = New DataColumn
        '    Dim dcPhyinvLocSCd = New DataColumn
        '    Dim dcPhyinvLocLNm = New DataColumn
        '    Dim dcPhyinvLocMNm = New DataColumn
        '    Dim dcPhyinvLocSNm = New DataColumn
        '    Dim dcPhyinvEmployeeCd = New DataColumn
        '    Dim dcPhyinvEmployeeNm = New DataColumn
        '    Dim dcTaskTs = New DataColumn
        '    Dim dcInsDate = New DataColumn
        '    Dim dcInsEmployeeCd = New DataColumn
        '    Dim dcInsEmployeeNm = New DataColumn
        '    Dim dcUpdDate = New DataColumn
        '    Dim dcUpdEmployeeCd = New DataColumn
        '    Dim dcUpdEmployeeNm = New DataColumn
        '    Dim dcUpdTask = New DataColumn
        '    Dim dcUpdFlg = New DataColumn   '2019/10/15 nv miyaji 既存が更新されたか

        '    ' 列名
        '    dcEpc.ColumnName = "EPC_SIZE"
        '    dcGenAssetId.ColumnName = "GEN_ASSET_ID"

        '    dcLeaseCnt.ColumnName = "LEASE_CNT"
        '    dcAssetStatus.ColumnName = "ASSET_STATUS"
        '    dcEpcEncStatus.ColumnName = "EPC_ENC_STATUS"
        '    dcTagLinkStatus.ColumnName = "TAG_LINK_STATUS"
        '    dcLeasePlaceCd.ColumnName = "LEASE_PLACE_CD"
        '    dcLeasePlaceNm.ColumnName = "LEASE_PLACE_NAME"
        '    dcRemark.ColumnName = "REMARK"
        '    dcLeaseEmployeeCd.ColumnName = "LEASE_EMPLOYEE_CD"
        '    dcLeaseEmployeeNm.ColumnName = "LEASE_EMPLOYEE_NAME"
        '    dcLeaseDt.ColumnName = "LEASE_DT"
        '    dcLeaseDueDt.ColumnName = "LEASE_DUE_DT"
        '    dcRetireTs.ColumnName = "RETIRE_TS"
        '    dcRetireEmployeeCd.ColumnName = "RETIRE_EMPLOYEE_CD"
        '    dcRetireEmployeeNm.ColumnName = "RETIRE_EMPLOYEE_NAME"
        '    dcRetireAssetId.ColumnName = "RETIRE_ASSET_ID"
        '    dcPhyinvTs.ColumnName = "PHYINV_TS"
        '    dcPhyinvStatus.ColumnName = "PHYINV_STATUS"
        '    dcPhyinvLocLCd.ColumnName = "PHYINV_LOC_L_CD"
        '    dcPhyinvLocMCd.ColumnName = "PHYINV_LOC_M_CD"
        '    dcPhyinvLocSCd.ColumnName = "PHYINV_LOC_S_CD"
        '    dcPhyinvLocLNm.ColumnName = "PHYINV_LOC_L_NAME"
        '    dcPhyinvLocMNm.ColumnName = "PHYINV_LOC_M_NAME"
        '    dcPhyinvLocSNm.ColumnName = "PHYINV_LOC_S_NAME"
        '    dcPhyinvEmployeeCd.ColumnName = "PHYINV_EMPLOYEE_CD"
        '    dcPhyinvEmployeeNm.ColumnName = "PHYINV_EMPLOYEE_NAME"
        '    dcTaskTs.ColumnName = "TASK_TS"
        '    dcInsDate.ColumnName = "INS_DATETIME"
        '    dcInsEmployeeCd.ColumnName = "INS_EMPLOYEE_CD"
        '    dcInsEmployeeNm.ColumnName = "INS_EMPLOYEE_NAME"
        '    dcUpdDate.ColumnName = "UPD_DATETIME"
        '    dcUpdEmployeeCd.ColumnName = "UPD_EMPLOYEE_CD"
        '    dcUpdEmployeeNm.ColumnName = "UPD_EMPLOYEE_NAME"
        '    dcUpdTask.ColumnName = "UPD_TASK"
        '    dcUpdFlg.ColumnName = "UPD_FLG"  '2019/10/15 nv miyaji 既存が更新されたか。新規or既存(除却済)はFalseのまま。
        '    ' 初期値
        '    dcEpc.DefaultValue = 0
        '    dcGenAssetId.DefaultValue = String.Empty

        '    dcLeaseCnt.DefaultValue = 0
        '    dcAssetStatus.DefaultValue = 0
        '    dcEpcEncStatus.DefaultValue = 0
        '    dcTagLinkStatus.DefaultValue = 0
        '    dcLeasePlaceCd.DefaultValue = DBNull.Value
        '    dcLeasePlaceNm.DefaultValue = DBNull.Value
        '    dcRemark.DefaultValue = DBNull.Value
        '    dcLeaseEmployeeCd.DefaultValue = DBNull.Value
        '    dcLeaseEmployeeNm.DefaultValue = DBNull.Value
        '    dcLeaseDt.DefaultValue = DBNull.Value
        '    dcLeaseDueDt.DefaultValue = DBNull.Value
        '    dcRetireTs.DefaultValue = DBNull.Value
        '    dcRetireEmployeeCd.DefaultValue = DBNull.Value
        '    dcRetireEmployeeNm.DefaultValue = DBNull.Value
        '    dcRetireAssetId.DefaultValue = DBNull.Value
        '    dcPhyinvTs.DefaultValue = DBNull.Value
        '    dcPhyinvStatus.DefaultValue = DBNull.Value
        '    dcPhyinvLocLCd.DefaultValue = DBNull.Value
        '    dcPhyinvLocMCd.DefaultValue = DBNull.Value
        '    dcPhyinvLocSCd.DefaultValue = DBNull.Value
        '    dcPhyinvLocLNm.DefaultValue = DBNull.Value
        '    dcPhyinvLocMNm.DefaultValue = DBNull.Value
        '    dcPhyinvLocSNm.DefaultValue = DBNull.Value
        '    dcPhyinvEmployeeCd.DefaultValue = DBNull.Value
        '    dcPhyinvEmployeeNm.DefaultValue = DBNull.Value
        '    dcTaskTs.DefaultValue = DBNull.Value
        '    dcInsDate.DefaultValue = CommonRfid.getNowDate
        '    dcInsEmployeeCd.DefaultValue = _LoginInfo.EmployeeCd
        '    dcInsEmployeeNm.DefaultValue = _LoginInfo.EmployeeNm
        '    dcUpdDate.DefaultValue = CommonRfid.getNowDate
        '    dcUpdEmployeeCd.DefaultValue = _LoginInfo.EmployeeCd
        '    dcUpdEmployeeNm.DefaultValue = _LoginInfo.EmployeeNm
        '    dcUpdTask.DefaultValue = Me._Message.getMessage(CommonLabelAssetList.TITLE)
        '    dcUpdFlg.DefaultValue = False  '2019/10/15 nv miyaji 既存が更新されたか。新規or既存(除却済)はFalseのまま。
        '    ' データ型
        '    dcEpc.DataType = Type.GetType("System.Int32")
        '    dcGenAssetId.DataType = Type.GetType("System.String")
        '    dcLeaseCnt.DataType = Type.GetType("System.Int32")
        '    dcAssetStatus.DataType = Type.GetType("System.Int32")
        '    dcEpcEncStatus.DataType = Type.GetType("System.Int32")
        '    dcTagLinkStatus.DataType = Type.GetType("System.Int32")
        '    dcLeasePlaceCd.DataType = Type.GetType("System.String")
        '    dcLeasePlaceNm.DataType = Type.GetType("System.String")
        '    dcRemark.DataType = Type.GetType("System.String")
        '    dcLeaseEmployeeCd.DataType = Type.GetType("System.String")
        '    dcLeaseEmployeeNm.DataType = Type.GetType("System.String")
        '    dcLeaseDt.DataType = Type.GetType("System.String")
        '    dcLeaseDueDt.DataType = Type.GetType("System.String")
        '    dcRetireTs.DataType = Type.GetType("System.Int64")
        '    dcRetireEmployeeCd.DataType = Type.GetType("System.String")
        '    dcRetireEmployeeNm.DataType = Type.GetType("System.String")
        '    dcRetireAssetId.DataType = Type.GetType("System.String")
        '    dcPhyinvTs.DataType = Type.GetType("System.Int64")
        '    dcPhyinvStatus.DataType = Type.GetType("System.String")
        '    dcPhyinvLocLCd.DataType = Type.GetType("System.String")
        '    dcPhyinvLocMCd.DataType = Type.GetType("System.String")
        '    dcPhyinvLocSCd.DataType = Type.GetType("System.String")
        '    dcPhyinvLocLNm.DataType = Type.GetType("System.String")
        '    dcPhyinvLocMNm.DataType = Type.GetType("System.String")
        '    dcPhyinvLocSNm.DataType = Type.GetType("System.String")
        '    dcPhyinvEmployeeCd.DataType = Type.GetType("System.String")
        '    dcPhyinvEmployeeNm.DataType = Type.GetType("System.String")
        '    dcTaskTs.DataType = Type.GetType("System.Int64")
        '    dcInsDate.DataType = Type.GetType("System.DateTime")
        '    dcInsEmployeeCd.DataType = Type.GetType("System.String")
        '    dcInsEmployeeNm.DataType = Type.GetType("System.String")
        '    dcUpdDate.DataType = Type.GetType("System.DateTime")
        '    dcUpdEmployeeCd.DataType = Type.GetType("System.String")
        '    dcUpdEmployeeNm.DataType = Type.GetType("System.String")
        '    dcUpdTask.DataType = Type.GetType("System.String")
        '    dcUpdFlg.DataType = Type.GetType("System.Boolean")  '2019/10/15 nv miyaji 既存が更新されたか。新規or既存(除却済)はFalseのまま。

        '    ' 列を追加
        '    Me._dtCsvImport.Columns.Add(dcEpc)
        '    Me._dtCsvImport.Columns.Add(dcGenAssetId)

        '    Me._dtCsvImport.Columns.Add(dcLeaseCnt)
        '    Me._dtCsvImport.Columns.Add(dcAssetStatus)
        '    Me._dtCsvImport.Columns.Add(dcEpcEncStatus)
        '    Me._dtCsvImport.Columns.Add(dcTagLinkStatus)
        '    Me._dtCsvImport.Columns.Add(dcLeasePlaceCd)
        '    Me._dtCsvImport.Columns.Add(dcLeasePlaceNm)
        '    Me._dtCsvImport.Columns.Add(dcRemark)
        '    Me._dtCsvImport.Columns.Add(dcLeaseEmployeeCd)
        '    Me._dtCsvImport.Columns.Add(dcLeaseEmployeeNm)
        '    Me._dtCsvImport.Columns.Add(dcLeaseDt)
        '    Me._dtCsvImport.Columns.Add(dcLeaseDueDt)
        '    Me._dtCsvImport.Columns.Add(dcRetireTs)
        '    Me._dtCsvImport.Columns.Add(dcRetireEmployeeCd)
        '    Me._dtCsvImport.Columns.Add(dcRetireEmployeeNm)
        '    Me._dtCsvImport.Columns.Add(dcRetireAssetId)
        '    Me._dtCsvImport.Columns.Add(dcPhyinvTs)
        '    Me._dtCsvImport.Columns.Add(dcPhyinvStatus)
        '    Me._dtCsvImport.Columns.Add(dcPhyinvLocLCd)
        '    Me._dtCsvImport.Columns.Add(dcPhyinvLocMCd)
        '    Me._dtCsvImport.Columns.Add(dcPhyinvLocSCd)
        '    Me._dtCsvImport.Columns.Add(dcPhyinvLocLNm)
        '    Me._dtCsvImport.Columns.Add(dcPhyinvLocMNm)
        '    Me._dtCsvImport.Columns.Add(dcPhyinvLocSNm)
        '    Me._dtCsvImport.Columns.Add(dcPhyinvEmployeeCd)
        '    Me._dtCsvImport.Columns.Add(dcPhyinvEmployeeNm)
        '    Me._dtCsvImport.Columns.Add(dcTaskTs)
        '    Me._dtCsvImport.Columns.Add(dcInsDate)
        '    Me._dtCsvImport.Columns.Add(dcInsEmployeeCd)
        '    Me._dtCsvImport.Columns.Add(dcInsEmployeeNm)
        '    Me._dtCsvImport.Columns.Add(dcUpdDate)
        '    Me._dtCsvImport.Columns.Add(dcUpdEmployeeCd)
        '    Me._dtCsvImport.Columns.Add(dcUpdEmployeeNm)
        '    Me._dtCsvImport.Columns.Add(dcUpdTask)
        '    Me._dtCsvImport.Columns.Add(dcUpdFlg)  '2019/10/15 nv miyaji 既存が更新されたか。新規or既存(除却済)はFalseのまま。

        'End Sub
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

#Region "全選択ボタン押下時処理"
        ''' <summary>
        ''' 全選択ボタン押下時処理
        ''' 表示されている検索条件で全件検索し
        ''' チェック項目としてセッションにセットする
        ''' </summary>
        ''' <param name="blnSelectAll"></param>
        ''' <returns></returns>
        Public Function SelectAll(ByVal blnSelectAll As Boolean) As ActionResult
            Dim sbSQL As New StringBuilder
            Dim lstAssetList As New List(Of AssetListViewModel)

            '--2020/02/12 nv miyaji upd start
            '選択解除の場合、初期化する
            If Not blnSelectAll Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                Return Json("0")
            Else
                '全選択ボタン押下フラグをセットする　'2020/02/12 nv miyaji　
                Session(CommonSessionKey.ALL_SELECT_FLG) = True
            End If
            '--2020/02/12 nv miyaji upd end

            '現在表示検索時のSQLを取得する
            Dim strSQL As String = Session(CommonSessionKey.LIST_VIEW_SQL)
            Dim lstParam As List(Of SqlParameter) = Session(CommonSessionKey.LIST_VIEW_PARAM)

            'パラメーターをコピーする(SqlParameterがSqlParameterCollectionに含まれるエラー回避のため)
            Dim lstParamCopy As New List(Of SqlParameter)
            Dim param As New SqlParameter
            For Each param In lstParam
                lstParamCopy.Add(New SqlParameter(param.ParameterName, param.Value))
            Next

            'SQLを実行する
            If Not IsNothing(strSQL) Then
                If lstParam.Count <= 0 Then
                    lstAssetList = _db.AssetListView.SqlQuery(strSQL).ToList
                Else

                    lstAssetList = _db.AssetListView.SqlQuery(strSQL, lstParamCopy.ToArray).ToList
                End If
            End If

            'チェックボックスlstAssetListの選択状態のキー(資産番号)を更新する
            If lstAssetList.Count > 0 Then
                Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstAssetList.Select(Function(d) $"{d.ASSET_NO},{d.ASSET_ID},{d.CAT_L_CD}").ToList
            End If

            '件数を返却する
            Return Json(lstAssetList.Count)

        End Function
#End Region

#Region "最新のチェック状態でセッション情報を更新する"
        ''' <summary>
        ''' 最新のチェック状態でセッション情報を更新する
        ''' 重複を排除したチェックされたキーを返却する
        ''' </summary>
        ''' <param name="SelectKeys">選択キー</param>
        ''' <param name="NoneSelectKeys">未選択キー</param>
        ''' <returns>チェックされた行のキーリスト</returns>
        Private Function UpdateChecedInfo(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As List(Of String)
            '--2020/01/07 miyaji add start
            '一覧にない資産は未選択キーとする
            If Not IsNothing(_ViewModel.ASSET_NO) Then
                Dim results = From data In _viewResult Where data.ASSET_NO = _ViewModel.ASSET_NO
                If results.Count <= 0 Then
                    NoneSelectKeys = String.Join("/", New String() {NoneSelectKeys, $"{_ViewModel.ASSET_NO},{_ViewModel.ASSET_ID},{_ViewModel.CAT_L_CD}"})
                End If
            End If
            '--2020/01/07 miyaji add end

            'セッションの選択キー情報を取得する
            Dim lstSelectKeys As New List(Of String)
            If Not IsNothing(Session(CommonSessionKey.LIST_CHECKED_KEYS)) Then
                lstSelectKeys = Session(CommonSessionKey.LIST_CHECKED_KEYS)
            End If

            '選択キーを追加する
            If Not IsNothing(SelectKeys) AndAlso SelectKeys <> String.Empty Then
                lstSelectKeys.AddRange(SelectKeys.Split("/").ToList)
            End If

            '--2020/01/07 miyaji add start
            '重複を排除する
            lstSelectKeys = lstSelectKeys.Distinct.ToList
            '--2020/01/07 miyaji add end

            '未選択キーを削除する
            If Not IsNothing(NoneSelectKeys) AndAlso NoneSelectKeys <> String.Empty Then
                For Each deleteKey In NoneSelectKeys.Split("/").ToList
                    lstSelectKeys.Remove(deleteKey)
                Next
            End If

            ''重複を排除する   '2020/01/07 nv miyaji　コメントアウト
            'lstSelectKeys = lstSelectKeys.Distinct.ToList

            'セッションを更新する
            Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstSelectKeys

            'チェックされた行のキーリストを返却する
            Return lstSelectKeys
        End Function
#End Region

#Region "未タグ紐づけ資産 存在チェック"
        ''' <summary>
        ''' 未タグ紐づけの資産があるかどうかを返す
        ''' </summary>
        ''' <returns>True:未タグ紐づけ資産有  False:未タグ紐づけ資産無</returns>
        Private Function IsExistsMitagHimoduke() As Boolean

            '現在表示検索時のSQLのWhere文とパラメータを取得する
            Dim sbWhereSQL As String = Session(CommonSessionKey.LIST_VIEW_SQL_WHERE)
            Dim lstParam As List(Of SqlParameter) = Session(CommonSessionKey.LIST_VIEW_PARAM)

            'パラメーターをコピーする(SqlParameterがSqlParameterCollectionに含まれるエラー回避のため)
            Dim lstParamCopy As New List(Of SqlParameter)
            Dim param As New SqlParameter
            '20230914 UPD Start Shirakawa
            If lstParam IsNot Nothing Then
                For Each param In lstParam
                    lstParamCopy.Add(New SqlParameter(param.ParameterName, param.Value))
                Next
            End If
            '20230914 UPD End Shirakawa

            '未タグ紐づけ資産 存在チェック
            If Dao.ToolAsset.IsExistsMitagHimoduke(sbWhereSQL, lstParamCopy) Then
                Return True
            Else
                Return False
            End If

        End Function
#End Region

        '2023/03/22 Kawamoto ADD START
#Region "添付ファイル情報取得処理（AJax)"
        ''' <summary>
        ''' 添付ファイル情報取得処理（AJax)
        ''' </summary>
        ''' <param name="AssetNo">資産番号</param>
        ''' <returns>View表示内容</returns>
        Public Function SelectAttachment(ByVal AssetNo As String) As ActionResult

            Dim lstRet As New List(Of AttachmentInfo)
            Dim sAttachmentPath As String = ConfigurationManager.AppSettings("AttachmentFolderPath")
            Dim sFolderPath As String = Path.Combine(sAttachmentPath, CommonRfid.FncStringToHex(AssetNo))

            '添付ファイル情報の取得
            lstRet = SelectAttachmentList(ConvertEnablePath(sFolderPath), AssetNo)

            Return Json(lstRet)

        End Function
#End Region

#Region "添付ファイル追加処理（AJax)"
        ''' <summary>
        ''' 添付ファイル追加処理（AJax)
        ''' </summary>
        ''' <param name="AssetNo">資産番号</param>
        ''' <param name="SelectedFiles">アップロードするファイル</param>
        ''' <returns>View表示内容</returns>
        Public Function AddAttachment(ByVal AssetNo As String, ByVal SelectedFiles As List(Of HttpPostedFileWrapper)) As ActionResult

            Dim lstRet As New List(Of AttachmentInfo)
            Dim sAttachmentPath As String = ConfigurationManager.AppSettings("AttachmentFolderPath")
            Dim sFolderPath As String = Path.Combine(sAttachmentPath, CommonRfid.FncStringToHex(AssetNo))

            'Webサイトのルート物理パスに変換
            sFolderPath = ConvertEnablePath(sFolderPath)

            Dim files = Request.Files("SelectedFiles")

            'フォルダがなければ作成
            If Not Directory.Exists(sFolderPath) Then
                Directory.CreateDirectory(sFolderPath)
            End If

            If SelectedFiles IsNot Nothing Then
                For Each file As HttpPostedFileWrapper In SelectedFiles
                    file.SaveAs(Path.Combine(sFolderPath, Path.GetFileName(file.FileName)))
                Next
            End If

            '添付ファイル情報の取得
            lstRet = SelectAttachmentList(sFolderPath, AssetNo)

            Return Json(lstRet)

        End Function
#End Region

#Region "添付ファイル削除処理（AJax)"
        ''' <summary>
        ''' 添付ファイル削除処理（AJax)
        ''' </summary>
        ''' <param name="AssetNo">資産番号</param>
        ''' <param name="DelFiles">削除するファイル</param>
        ''' <returns>View表示内容</returns>
        Public Function DeleteAttachment(ByVal AssetNo As String, ByVal DelFiles As String()) As ActionResult

            Dim lstRet As New List(Of AttachmentInfo)
            Dim sAttachmentPath As String = ConfigurationManager.AppSettings("AttachmentFolderPath")
            Dim sFolderPath As String = Path.Combine(sAttachmentPath, CommonRfid.FncStringToHex(AssetNo))

            'Webサイトのルート物理パスに変換
            sFolderPath = ConvertEnablePath(sFolderPath)

            'ファイルの削除
            For Each sName As String In DelFiles
                Dim sFilePath As String = Path.Combine(sFolderPath, sName.TrimEnd("/"c))
                System.IO.File.Delete(sFilePath)
            Next

            '添付ファイル情報の取得
            lstRet = SelectAttachmentList(sFolderPath, AssetNo)

            Return Json(lstRet)

        End Function
#End Region

#Region "添付ファイル情報の取得処理"
        ''' <summary>
        ''' 添付ファイル情報の取得処理
        ''' </summary>
        ''' <param name="prmFolderPath">フォルダパス（物理パス）</param>
        ''' <param name="prmAssetNo">資産番号</param>
        ''' <returns>View表示内容</returns>
        Private Function SelectAttachmentList(ByVal prmFolderPath As String, ByVal prmAssetNo As String) As List(Of AttachmentInfo)
            Dim lstRet As New List(Of AttachmentInfo)

            'リンクに設定するためのファイルURL情報を取得
            Dim sAttachUrlValue As String = ConfigurationManager.AppSettings("AttachUrlValue")
            Dim sFileUrl As String = $"~{sAttachUrlValue}/{CommonRfid.FncStringToHex(prmAssetNo)}/"

            If Directory.Exists(prmFolderPath) Then
                '表示データをリストに格納します
                Dim lst As New List(Of AttachmentInfo)
                For Each sFilePath In IO.Directory.GetFiles(prmFolderPath)
                    'ファイル情報
                    Dim fi As New System.IO.FileInfo(sFilePath)

                    Dim AttachmentInfo As New AttachmentInfo
                    AttachmentInfo.FileNm = fi.Name
                    AttachmentInfo.ByteCount = Math.Ceiling(fi.Length / 1024).ToString("#,##0")
                    AttachmentInfo.FileUpdateTime = fi.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss")
                    AttachmentInfo.FileDownloadLink = Url.Content(sFileUrl + System.Uri.EscapeDataString(fi.Name))

                    lst.Add(AttachmentInfo)
                Next

                '表示順はファイル名の昇順でソートする
                lstRet = lst.OrderBy(Function(n) n.FileNm.Split("."c)(0)).ToList()
            End If

            Return lstRet

        End Function
#End Region
        '2023/03/22 Kawamoto ADD END

        '2023/05/10 Kawamoto ADD START
#Region "計測器ありの計測器情報の取得処理（AJax)"
        ''' <summary>
        ''' 計測器ありの計測器情報の取得処理（AJax)
        ''' </summary>
        ''' <returns>危険物の指定数量の単位</returns>
        Public Function GetWeighingMachine() As JsonResult

            '計量器連携あり情報の取得
            Dim WeighingMachineLinkList As List(Of String) = Dao.ToolSetting.GetWeighingMachineLinkList()

            Return Json(WeighingMachineLinkList)

        End Function
#End Region
        '2023/05/10 Kawamoto ADD END

#Region "共通"

        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL(Optional ByVal isSort As Boolean = True) As String
            'Dim sSelKey As String = Request.QueryString("keyValue")            '在庫照会からの呼出し時、値がセットされる              
            Dim sSelKey As String = String.Empty
            'If Not String.IsNullOrEmpty(Request.QueryString("keyValue")) Then
            '    sSelKey = Request.QueryString("keyValue")
            'Else
            '    sSelKey = ViewBag.AssetListKey
            'End If
            Dim info As RedirectAssetListInfo = Session(RedirectAssetListInfo.ID)
            If info IsNot Nothing Then
                sSelKey = info.KeyValue
            End If

            '--2021/10/01 nv miyaji add start
            '権限情報の取得
            If Not _LoginInfo Is Nothing Then
                _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)
                If _AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    '自拠点の更新・照会権限有り
                    If _AuthInfo.UpdHubAuthFlg = AUTH_FLG.Yes Then
                        '他拠点の更新・照会権限有り
                        ViewBag.AuthProcFlg = AuthProcFlg.All
                    Else
                        '他拠点の更新・照会権限無し
                        ViewBag.AuthProcFlg = AuthProcFlg.JiAll
                    End If
                    ViewBag.CsvImportDispFlg = True
                Else
                    '照会のみ
                    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                    ViewBag.CsvImportDispFlg = False
                End If
            Else
                ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                ViewBag.CsvImportDispFlg = False
            End If

            ViewBag.ExistRFIDLoc = Dao.ToolLocation.IsRFIDLoc(_ViewModel.SearchLocL)

#Region "CASE条件SQLを作成_リース期限何日前,□□期限何日前,貸出アラート回数"
            '--リース期限何日前 を求める        2019/10/03 日数 が NULL 場合は、-1
            Dim sbAlertDays As StringBuilder = New StringBuilder
            '小分類
            sbAlertDays.AppendLine("  CASE ")
            sbAlertDays.AppendLine("     WHEN CATS.LEASE_LIMIT_ALERT_DIV = '" & Misc.LeaseLimitAlertDiv.No & "' THEN")
            'sbAlertDays.AppendLine("         0")
            sbAlertDays.AppendLine("         -1")
            sbAlertDays.AppendLine("     WHEN CATS.LEASE_LIMIT_ALERT_DIV = '" & Misc.LeaseLimitAlertDiv.Yes & "' THEN")
            'sbAlertDays.AppendLine("         ISNULL(CATS.LEASE_LIMIT_BEF_DAYS , 0)")
            sbAlertDays.AppendLine("         ISNULL(CATS.LEASE_LIMIT_BEF_DAYS , -1)")
            sbAlertDays.AppendLine("     ELSE")

            '中分類
            sbAlertDays.AppendLine("         CASE ")
            sbAlertDays.AppendLine("             WHEN CATM.LEASE_LIMIT_ALERT_DIV = '" & Misc.LeaseLimitAlertDiv.No & "' THEN")
            'sbAlertDays.AppendLine("                 0")
            sbAlertDays.AppendLine("                 -1")
            sbAlertDays.AppendLine("             WHEN CATM.LEASE_LIMIT_ALERT_DIV = '" & Misc.LeaseLimitAlertDiv.Yes & "' THEN")
            'sbAlertDays.AppendLine("                 ISNULL(CATM.LEASE_LIMIT_BEF_DAYS , 0)")
            sbAlertDays.AppendLine("                 ISNULL(CATM.LEASE_LIMIT_BEF_DAYS , -1)")
            sbAlertDays.AppendLine("             ELSE")

            '大分類
            sbAlertDays.AppendLine("         CASE ")
            sbAlertDays.AppendLine("             WHEN CATL.LEASE_LIMIT_ALERT_DIV = '" & Misc.LeaseLimitAlertDiv.No & "' THEN")
            'sbAlertDays.AppendLine("                 0")
            sbAlertDays.AppendLine("                 -1")
            sbAlertDays.AppendLine("             WHEN CATL.LEASE_LIMIT_ALERT_DIV = '" & Misc.LeaseLimitAlertDiv.Yes & "' THEN")
            'sbAlertDays.AppendLine("                 ISNULL(CATL.LEASE_LIMIT_BEF_DAYS , 0)")
            sbAlertDays.AppendLine("                 ISNULL(CATL.LEASE_LIMIT_BEF_DAYS , -1)")
            sbAlertDays.AppendLine("             ELSE")
            'sbAlertDays.AppendLine("                 0")
            sbAlertDays.AppendLine("                 -1")
            sbAlertDays.AppendLine("         END ")

            sbAlertDays.AppendLine("     END ")

            sbAlertDays.AppendLine("  END ")

            '--□□期限何日前 を求める        2019/10/03 日数 が NULL 場合は、-1
            Dim sbGenLimitDays As StringBuilder = New StringBuilder
            '小分類
            sbGenLimitDays.AppendLine("  CASE ")
            sbGenLimitDays.AppendLine("     WHEN CATS.GENERIC_LIMIT_ALERT_DIV = '" & Misc.GenericLimitAlertDiv.No & "' THEN")
            'sbGenLimitDays.AppendLine("         0")
            sbGenLimitDays.AppendLine("         -1")
            sbGenLimitDays.AppendLine("     WHEN CATS.GENERIC_LIMIT_ALERT_DIV = '" & Misc.GenericLimitAlertDiv.Yes & "' THEN")
            'sbGenLimitDays.AppendLine("         ISNULL(CATS.GENERIC_LIMIT_BEF_DAYS , 0)")
            sbGenLimitDays.AppendLine("         ISNULL(CATS.GENERIC_LIMIT_BEF_DAYS , -1)")
            sbGenLimitDays.AppendLine("     ELSE")

            '中分類
            sbGenLimitDays.AppendLine("         CASE ")
            sbGenLimitDays.AppendLine("             WHEN CATM.GENERIC_LIMIT_ALERT_DIV = '" & Misc.GenericLimitAlertDiv.No & "' THEN")
            'sbGenLimitDays.AppendLine("                 0")
            sbGenLimitDays.AppendLine("                 -1")
            sbGenLimitDays.AppendLine("             WHEN CATM.GENERIC_LIMIT_ALERT_DIV = '" & Misc.GenericLimitAlertDiv.Yes & "' THEN")
            'sbGenLimitDays.AppendLine("                 ISNULL(CATM.GENERIC_LIMIT_BEF_DAYS , 0)")
            sbGenLimitDays.AppendLine("                 ISNULL(CATM.GENERIC_LIMIT_BEF_DAYS , -1)")
            sbGenLimitDays.AppendLine("             ELSE")

            '大分類
            sbGenLimitDays.AppendLine("         CASE ")
            sbGenLimitDays.AppendLine("             WHEN CATL.GENERIC_LIMIT_ALERT_DIV = '" & Misc.GenericLimitAlertDiv.No & "' THEN")
            'sbGenLimitDays.AppendLine("                 0")
            sbGenLimitDays.AppendLine("                 -1")
            sbGenLimitDays.AppendLine("             WHEN CATL.GENERIC_LIMIT_ALERT_DIV = '" & Misc.GenericLimitAlertDiv.Yes & "' THEN")
            'sbGenLimitDays.AppendLine("                 ISNULL(CATL.GENERIC_LIMIT_BEF_DAYS , 0)")
            sbGenLimitDays.AppendLine("                 ISNULL(CATL.GENERIC_LIMIT_BEF_DAYS , -1)")
            sbGenLimitDays.AppendLine("             ELSE")
            'sbGenLimitDays.AppendLine("                 0")
            sbGenLimitDays.AppendLine("                 -1")
            sbGenLimitDays.AppendLine("         END ")

            sbGenLimitDays.AppendLine("     END ")

            sbGenLimitDays.AppendLine("  END ")

            '--貸出アラート回数 を求める 回数 が NULL 場合は、-1        2020/03/03 nv inoue add
            Dim sbLeaseAlertCnt As StringBuilder = New StringBuilder
            '小分類
            sbLeaseAlertCnt.AppendLine("  CASE ")
            sbLeaseAlertCnt.AppendLine("     WHEN CATS.LEASE_ALERT_DIV = '" & Misc.LeaseAlertDiv.No & "' THEN")
            sbLeaseAlertCnt.AppendLine("         -1")
            sbLeaseAlertCnt.AppendLine("     WHEN CATS.LEASE_ALERT_DIV = '" & Misc.LeaseAlertDiv.Yes & "' THEN")
            sbLeaseAlertCnt.AppendLine("         ISNULL(CATS.LEASE_ALERT_CNT , -1)")
            sbLeaseAlertCnt.AppendLine("     ELSE")

            '中分類
            sbLeaseAlertCnt.AppendLine("         CASE ")
            sbLeaseAlertCnt.AppendLine("             WHEN CATM.LEASE_ALERT_DIV = '" & Misc.LeaseAlertDiv.No & "' THEN")
            sbLeaseAlertCnt.AppendLine("                 -1")
            sbLeaseAlertCnt.AppendLine("             WHEN CATM.LEASE_ALERT_DIV = '" & Misc.LeaseAlertDiv.Yes & "' THEN")
            sbLeaseAlertCnt.AppendLine("                 ISNULL(CATM.LEASE_ALERT_CNT , -1)")
            sbLeaseAlertCnt.AppendLine("             ELSE")

            '大分類
            sbLeaseAlertCnt.AppendLine("         CASE ")
            sbLeaseAlertCnt.AppendLine("             WHEN CATL.LEASE_ALERT_DIV = '" & Misc.LeaseAlertDiv.No & "' THEN")
            sbLeaseAlertCnt.AppendLine("                 -1")
            sbLeaseAlertCnt.AppendLine("             WHEN CATL.LEASE_ALERT_DIV = '" & Misc.LeaseAlertDiv.Yes & "' THEN")
            sbLeaseAlertCnt.AppendLine("                 ISNULL(CATL.LEASE_ALERT_CNT , -1)")
            sbLeaseAlertCnt.AppendLine("             ELSE")
            sbLeaseAlertCnt.AppendLine("                 -1")
            sbLeaseAlertCnt.AppendLine("         END ")

            sbLeaseAlertCnt.AppendLine("     END ")

            sbLeaseAlertCnt.AppendLine("  END ")
#End Region

#Region "検索条件SQLを作成"
            Dim sbWhr As New StringBuilder
            If String.IsNullOrEmpty(sSelKey) Then
                '----- 通常時の検索条件

                'ロケ大区分コード
                If Not String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                    sbWhr.AppendLine("  AND ASSET.LOC_L_CD = @SearchLocL")
                    addSqlParam("SearchLocL", $"{_ViewModel.SearchLocL}")
                End If

                'ロケ中区分コード（複数選択）
                Dim sLocMCond As String = CreateInCondition("ASSET.LOC_M_CD", "SearchLocM", _ViewModel.SearchLocM)
                If Not String.IsNullOrEmpty(sLocMCond) Then
                    sbWhr.AppendLine(sLocMCond)
                End If

                'ロケ小区分コード（複数選択）
                Dim sLocSCond As String = CreateInCondition("ASSET.LOC_S_CD", "SearchLocS", _ViewModel.SearchLocS)
                If Not String.IsNullOrEmpty(sLocSCond) Then
                    sbWhr.AppendLine(sLocSCond)
                End If

                'カテゴリ大分類 SearchCatL（複数選択）
                Dim sCatLCond As String = CreateInCondition("ASSET.CAT_L_CD", "SearchCatL", _ViewModel.SearchCatL)
                If Not String.IsNullOrEmpty(sCatLCond) Then
                    sbWhr.AppendLine(sCatLCond)
                End If

                '資産番号 SearchAssetNo（「|」区切りで複数指定）
                '（資産番号・旧資産番号のいずれかに前方一致）
                Dim sAssetNoCond As String = CreateLikeCondition({"ASSET.ASSET_NO", "ASSET.SUB_ASSET_NO"}, "SearchAssetNo", CommonRfid.GetMultiInputValues(_ViewModel.SearchAssetNo))
                If Not String.IsNullOrEmpty(sAssetNoCond) Then
                    sbWhr.AppendLine(sAssetNoCond)
                End If

                '資産名称 SearchAssetName
                If (Not IsNothing(_ViewModel.SearchAssetName)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchAssetName.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.ASSET_NAME like " & "@SearchAssetName")
                    addSqlParam("SearchAssetName", $"{"%" & _ViewModel.SearchAssetName.Trim & "%"}")
                End If

                '有形・無形区分
                If _ViewModel.SearchIsTangibleY <> _ViewModel.SearchIsTangibleN Then
                    '有形資産、無形資産の値が違う場合は条件に追加
                    sbWhr.AppendLine("  AND ASSET.IS_TANGIBLE = " & "@SearchIsTangible")
                    If _ViewModel.SearchIsTangibleY Then
                        addSqlParam("SearchIsTangible", $"{ TANGIBLE.TANGIBLE }")
                    Else
                        addSqlParam("SearchIsTangible", $"{ TANGIBLE.NOT_TANGIBLE }")
                    End If
                End If

                'フリー項目10 SearchFreeItem10 貸与資産状態区分
                If (Not IsNothing(_ViewModel.SearchFreeItem10)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem10.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM10 IN (" & "@SearchFreeItem10" & "," & "@SearchFreeItem10Name" & ")")
                    Select Case _ViewModel.SearchFreeItem10.Trim
                        Case LOAN_ASSET_STATUS.BLANK
                            addSqlParam("SearchFreeItem10", "")
                            addSqlParam("SearchFreeItem10Name", LOAN_ASSET_STATUS_VALUE.BLANK)
                        Case LOAN_ASSET_STATUS.ON_LOAN
                            addSqlParam("SearchFreeItem10", LOAN_ASSET_STATUS.ON_LOAN)
                            addSqlParam("SearchFreeItem10Name", LOAN_ASSET_STATUS_VALUE.ON_LOAN)
                        Case LOAN_ASSET_STATUS.RETURNED
                            addSqlParam("SearchFreeItem10", LOAN_ASSET_STATUS.RETURNED)
                            addSqlParam("SearchFreeItem10Name", LOAN_ASSET_STATUS_VALUE.RETURNED)
                    End Select
                End If

                Const sDateFormatCond As String = " LIKE '[0-9][0-9][0-9][0-9]/[0-9][0-9]/[0-9][0-9]'"

                If (Not IsNothing(_ViewModel.SearchFreeItem2From)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem2From.Trim)) Then
                    Dim sDate As String = CDate(_ViewModel.SearchFreeItem2From).ToString("yyyy/MM/dd")
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM2" & sDateFormatCond)
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM2 >= " & "@SearchFreeItem2From")
                    addSqlParam("SearchFreeItem2From", $"{sDate}")
                End If

                If (Not IsNothing(_ViewModel.SearchFreeItem2To)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem2To.Trim)) Then
                    Dim sDate As String = CDate(_ViewModel.SearchFreeItem2To).ToString("yyyy/MM/dd")
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM2" & sDateFormatCond)
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM2 <= " & "@SearchFreeItem2To")
                    addSqlParam("SearchFreeItem2To", $"{sDate}")
                End If

                'フリー項目4 SearchFreeItem4 稼働区分名
                If (Not IsNothing(_ViewModel.SearchFreeItem4)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem4.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM4 like " & "@SearchFreeItem4")
                    addSqlParam("SearchFreeItem4", $"{"%" & _ViewModel.SearchFreeItem4.Trim & "%"}")
                End If

                'フリー項目5 SearchFreeItem5 型式
                If (Not IsNothing(_ViewModel.SearchFreeItem5)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem5.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM5 like " & "@SearchFreeItem5")
                    addSqlParam("SearchFreeItem5", $"{"%" & _ViewModel.SearchFreeItem5.Trim & "%"}")
                End If

                'フリー項目6 SearchFreeItem6 管理社員番号
                If (Not IsNothing(_ViewModel.SearchFreeItem6)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem6.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM6 like " & "@SearchFreeItem6")
                    addSqlParam("SearchFreeItem6", $"{"%" & _ViewModel.SearchFreeItem6.Trim & "%"}")
                End If

                'フリー項目7 SearchFreeItem7 シリアルNo
                If (Not IsNothing(_ViewModel.SearchFreeItem7)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem7.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM7 like " & "@SearchFreeItem7")
                    addSqlParam("SearchFreeItem7", $"{"%" & _ViewModel.SearchFreeItem7.Trim & "%"}")
                End If

                'フリー項目8 SearchFreeItem8 WBS番号
                If (Not IsNothing(_ViewModel.SearchFreeItem8)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem8.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM8 like " & "@SearchFreeItem8")
                    addSqlParam("SearchFreeItem8", $"{"%" & _ViewModel.SearchFreeItem8.Trim & "%"}")
                End If

                'フリー項目9 SearchFreeItem9 備考
                If (Not IsNothing(_ViewModel.SearchFreeItem9)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem9.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM9 like " & "@SearchFreeItem9")
                    addSqlParam("SearchFreeItem9", $"{"%" & _ViewModel.SearchFreeItem9.Trim & "%"}")
                End If

                'フリー項目11From SearchFreeItem11From 取得金額
                If (Not IsNothing(_ViewModel.SearchFreeItem11From)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem11From.Trim)) Then
                    sbWhr.AppendLine("  AND TRY_CAST(ASSET.FREE_ITEM11 AS decimal(23, 2)) >= " & "@SearchFreeItem11From")
                    addSqlParam("SearchFreeItem11From", $"{Decimal.Parse(_ViewModel.SearchFreeItem11From.Trim)}")
                End If

                'フリー項目11To SearchFreeItem11To 所得金額
                If (Not IsNothing(_ViewModel.SearchFreeItem11To)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem11To.Trim)) Then
                    sbWhr.AppendLine("  AND TRY_CAST(ASSET.FREE_ITEM11 AS decimal(23, 2)) <= " & "@SearchFreeItem11To")
                    addSqlParam("SearchFreeItem11To", $"{Decimal.Parse(_ViewModel.SearchFreeItem11To.Trim)}")
                End If

                'フリー項目12From SearchFreeItem12From 当月末簿価
                If (Not IsNothing(_ViewModel.SearchFreeItem12From)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem12From.Trim)) Then
                    sbWhr.AppendLine("  AND TRY_CAST(ASSET.FREE_ITEM12 AS decimal(23, 2)) >= " & "@SearchFreeItem12From")
                    addSqlParam("SearchFreeItem12From", $"{Decimal.Parse(_ViewModel.SearchFreeItem12From.Trim)}")
                End If

                'フリー項目12To SearchFreeItem12To 当月末簿価
                If (Not IsNothing(_ViewModel.SearchFreeItem12To)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem12To.Trim)) Then
                    sbWhr.AppendLine("  AND TRY_CAST(ASSET.FREE_ITEM12 AS decimal(23, 2)) <= " & "@SearchFreeItem12To")
                    addSqlParam("SearchFreeItem12To", $"{Decimal.Parse(_ViewModel.SearchFreeItem12To.Trim)}")
                End If

                'フリー項目14 SearchFreeItem14 シリアル番号
                If (Not IsNothing(_ViewModel.SearchFreeItem14)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem14.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM14 like " & "@SearchFreeItem14")
                    addSqlParam("SearchFreeItem14", $"{"%" & _ViewModel.SearchFreeItem14.Trim & "%"}")
                End If

                'フリー項目15 SearchFreeItem15 資産グループコード
                If (Not IsNothing(_ViewModel.SearchFreeItem15)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem15.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM15 like " & "@SearchFreeItem15")
                    addSqlParam("SearchFreeItem15", $"{"%" & _ViewModel.SearchFreeItem15.Trim & "%"}")
                End If

                'フリー項目16 SearchFreeItem16 貸与先
                If (Not IsNothing(_ViewModel.SearchFreeItem16)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem16.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM16 like " & "@SearchFreeItem16")
                    addSqlParam("SearchFreeItem16", $"{"%" & _ViewModel.SearchFreeItem16.Trim & "%"}")
                End If

                'フリー項目17 SearchFreeItem17 貸与先名
                If (Not IsNothing(_ViewModel.SearchFreeItem17)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem17.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM17 like " & "@SearchFreeItem17")
                    addSqlParam("SearchFreeItem17", $"{"%" & _ViewModel.SearchFreeItem17.Trim & "%"}")
                End If

                If (Not IsNothing(_ViewModel.SearchFreeItem18From)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem18From.Trim)) Then
                    Dim sDate As String = CDate(_ViewModel.SearchFreeItem18From).ToString("yyyy/MM/dd")
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM18" & sDateFormatCond)
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM18 >= " & "@SearchFreeItem18From")
                    addSqlParam("SearchFreeItem18From", $"{sDate}")
                End If

                If (Not IsNothing(_ViewModel.SearchFreeItem18To)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem18To.Trim)) Then
                    Dim sDate As String = CDate(_ViewModel.SearchFreeItem18To).ToString("yyyy/MM/dd")
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM18" & sDateFormatCond)
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM18 <= " & "@SearchFreeItem18To")
                    addSqlParam("SearchFreeItem18To", $"{sDate}")
                End If

                If (Not IsNothing(_ViewModel.SearchFreeItem19From)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem19From.Trim)) Then
                    Dim sDate As String = CDate(_ViewModel.SearchFreeItem19From).ToString("yyyy/MM/dd")
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM19" & sDateFormatCond)
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM19 >= " & "@SearchFreeItem19From")
                    addSqlParam("SearchFreeItem19From", $"{sDate}")
                End If

                If (Not IsNothing(_ViewModel.SearchFreeItem19To)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem19To.Trim)) Then
                    Dim sDate As String = CDate(_ViewModel.SearchFreeItem19To).ToString("yyyy/MM/dd")
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM19" & sDateFormatCond)
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM19 <= " & "@SearchFreeItem19To")
                    addSqlParam("SearchFreeItem19To", $"{sDate}")
                End If

                'フリー項目20From SearchFreeItem20From 貸与回数
                If (Not IsNothing(_ViewModel.SearchFreeItem20From)) AndAlso
                    (IsNumeric(_ViewModel.SearchFreeItem20From.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM20 >= " & "@SearchFreeItem20From")
                    addSqlParam("SearchFreeItem20From", $"{CInt(_ViewModel.SearchFreeItem20From.Trim)}")
                End If

                'フリー項目20To SearchFreeItem20To 貸与回数
                If (Not IsNothing(_ViewModel.SearchFreeItem20To)) AndAlso
                    (IsNumeric(_ViewModel.SearchFreeItem20To.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM20 <= " & "@SearchFreeItem20To")
                    addSqlParam("SearchFreeItem20To", $"{CInt(_ViewModel.SearchFreeItem20To.Trim)}")
                End If

                'フリー項目21 SearchFreeItem21 備考欄
                If (Not IsNothing(_ViewModel.SearchFreeItem21)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem21.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM21 like " & "@SearchFreeItem21")
                    addSqlParam("SearchFreeItem21", $"{"%" & _ViewModel.SearchFreeItem21.Trim & "%"}")
                End If

                'フリー項目39 SearchFreeItem39 市区町村コード名称
                If (Not IsNothing(_ViewModel.SearchFreeItem39)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem39.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM39 like " & "@SearchFreeItem39")
                    addSqlParam("SearchFreeItem39", $"{"%" & _ViewModel.SearchFreeItem39.Trim & "%"}")
                End If

                'フリー項目42 SearchFreeItem42 CAPS番号
                If (Not IsNothing(_ViewModel.SearchFreeItem42)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem42.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM42 like " & "@SearchFreeItem42")
                    addSqlParam("SearchFreeItem42", $"{"%" & _ViewModel.SearchFreeItem42.Trim & "%"}")
                End If

                'フリー項目37 SearchFreeItem37 資産リース区分
                If (Not IsNothing(_ViewModel.SearchFreeItem37)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem37.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM37 like " & "@SearchFreeItem37")
                    addSqlParam("SearchFreeItem37", $"{"%" & _ViewModel.SearchFreeItem37.Trim & "%"}")
                End If

                If (Not IsNothing(_ViewModel.SearchFreeItem27)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem27.Trim)) Then
                    sbWhr.AppendLine("  AND ASSET.FREE_ITEM27 like " & "@SearchFreeItem27")
                    addSqlParam("SearchFreeItem27", $"{"%" & _ViewModel.SearchFreeItem27.Trim & "%"}")
                End If

                Dim dicRemark As New Dictionary(Of String, String) From {
                    {"REMARK1", _ViewModel.SearchRemark1},
                    {"REMARK2", _ViewModel.SearchRemark2},
                    {"REMARK3", _ViewModel.SearchRemark3},
                    {"REMARK4", _ViewModel.SearchRemark4},
                    {"REMARK5", _ViewModel.SearchRemark5},
                    {"REMARK6", _ViewModel.SearchRemark6},
                    {"REMARK7", _ViewModel.SearchRemark7},
                    {"REMARK8", _ViewModel.SearchRemark8},
                    {"REMARK9", _ViewModel.SearchRemark9},
                    {"REMARK10", _ViewModel.SearchRemark10}
                }
                For Each kvRemark As KeyValuePair(Of String, String) In dicRemark
                    Dim sRemarkCond As String = CreateLikeCondition({$"ASSET.{kvRemark.Key}"}, $"Search{kvRemark.Key}", CommonRfid.GetMultiInputValues(kvRemark.Value), True)
                    If Not String.IsNullOrEmpty(sRemarkCond) Then
                        sbWhr.AppendLine(sRemarkCond)
                    End If
                Next

                'タグ紐付状態 SearchTagLinkStatus
                If (Not IsNothing(_ViewModel.SearchTagLinkStatus)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTagLinkStatus)) Then
                    sbWhr.AppendLine("  And ASSET.TAG_LINK_STATUS = " & "@SearchTagLinkStatus")
                    addSqlParam("SearchTagLinkStatus", $"{_ViewModel.SearchTagLinkStatus}")
                End If
            Else
                '----- 在庫照会時の検索条件
                Dim keys As String() = sSelKey.Split(",")       'ロケ大区分コード,ロケ中区分コード,ロケ小区分コード,カテゴリー大分類コード,カテゴリー中分類コード,カテゴリー小分類コード

                '--2020/03/16 nv miyaji add start
                '在庫照会画面の検索条件を取得
                Dim senimaeModel As StockListViewModel = Session(CommonSessionKey.CREAGE_STOCK_SERCH_KEY)

                'String.Empty は、" " にする
                For ii As Integer = 0 To keys.Length - 1
                    If String.IsNullOrEmpty(keys(ii)) Then
                        keys(ii) = " "
                    End If
                Next

                '資産状態 SearchAssetStatus
                sbWhr.AppendLine("  And ASSET.ASSET_STATUS in (" & Misc.ASSET_STATUS.Storage & "," & Misc.ASSET_STATUS.Lending & ")")

                '--2020/03/17 nv Inoue update start
                'ロケ大区分コード
                sbWhr.AppendLine("  AND ASSET.LOC_L_CD = @SearchLocL")
                addSqlParam("SearchLocL", $"{keys(0)}")

                'ロケ中区分コード
                If senimaeModel.SearchDispSelectLocation = LOC_DIV.LocationM Or senimaeModel.SearchDispSelectLocation = LOC_DIV.LocationS Then
                    sbWhr.AppendLine("  AND ASSET.LOC_M_CD = @SearchLocM")
                    addSqlParam("SearchLocM", $"{keys(1)}")
                End If

                'ロケ小区分コード
                If senimaeModel.SearchDispSelectLocation = LOC_DIV.LocationS Then
                    sbWhr.AppendLine("  AND ASSET.LOC_S_CD = @SearchLocS")
                    addSqlParam("SearchLocS", $"{keys(2)}")
                End If

                'カテゴリ大分類 SearchCatL
                sbWhr.AppendLine("  AND ASSET.CAT_L_CD = @SearchCatL")
                addSqlParam("SearchCatL", $"{keys(3)}")

                If senimaeModel.SearchDispSelectCategory = CAT_DIV.CategoryM Or senimaeModel.SearchDispSelectCategory = CAT_DIV.CategoryS Then
                    'カテゴリ中分類 SearchCatM
                    sbWhr.AppendLine("  AND ASSET.CAT_M_CD = @SearchCatM")
                    addSqlParam("SearchCatM", $"{keys(4)}")
                End If

                'カテゴリ小分類 SearchCatS
                If senimaeModel.SearchDispSelectCategory = CAT_DIV.CategoryS Then
                    sbWhr.AppendLine("  AND ASSET.CAT_S_CD = @SearchCatS")
                    addSqlParam("SearchCatS", $"{keys(5)}")
                End If
                '--2020/03/17 nv Inoue update end

                '--2021/10/12 nv miyaji add start
                '管理ロケーションコード
                sbWhr.AppendLine("  AND ASSET.MNG_LOC_L_CD = @SearchMngLocL")
                addSqlParam("SearchMngLocL", $"{keys(6)}")
                '--2021/10/12 nv miyaji add start

                '資産番号（在庫照会の検索条件。「|」区切りで複数指定されている場合がある）
                Dim sAssetNoCond As String = CreateLikeCondition({"ASSET.ASSET_NO"}, "assetNo", CommonRfid.GetMultiInputValues(senimaeModel.SearchAssetNo))
                If Not String.IsNullOrEmpty(sAssetNoCond) Then
                    sbWhr.AppendLine(sAssetNoCond)
                End If

                '資産ID
                If Not String.IsNullOrEmpty(senimaeModel.SearchAssetID) Then
                    sbWhr.AppendLine("  AND ASSET.ASSET_ID Like @assetID")
                    addSqlParam("assetID", $"%{senimaeModel.SearchAssetID}%")
                End If

                ' 資産名称
                If Not IsNothing(senimaeModel.SearchAssetName) AndAlso senimaeModel.SearchAssetName <> "" Then
                    sbWhr.AppendLine("  AND ASSET.ASSET_NAME Like @assetName")
                    addSqlParam("assetName", $"%{senimaeModel.SearchAssetName}%")
                End If
                ' メーカ
                If Not IsNothing(senimaeModel.SearchMaker) AndAlso senimaeModel.SearchMaker <> "" Then
                    sbWhr.AppendLine("  AND ASSET.MAKER Like @maker")
                    addSqlParam("maker", $"%{senimaeModel.SearchMaker}%")
                End If
                ' 品番
                If Not IsNothing(senimaeModel.SearchProductNo) AndAlso senimaeModel.SearchProductNo <> "" Then
                    sbWhr.AppendLine("  AND ASSET.PRODUCT_NO Like @productNo")
                    addSqlParam("productNo", $"%{senimaeModel.SearchProductNo}%")
                End If
                ' 型番
                If Not IsNothing(senimaeModel.SearchModelNo) AndAlso senimaeModel.SearchModelNo <> "" Then
                    sbWhr.AppendLine("  AND ASSET.MODEL_NO Like @modelNo")
                    addSqlParam("modelNo", $"%{senimaeModel.SearchModelNo}%")
                End If
                '--2020/03/16 nv miyaji add end

            End If
#End Region

            Dim sbSQL As New StringBuilder

            '一覧表示表用SQL
            If isSort Then
                sbSQL.AppendLine("SELECT ")
            Else
                sbSQL.AppendLine("SELECT TOP 1")
            End If
            sbSQL.AppendLine("  ASSET.ASSET_NO ")
            sbSQL.AppendLine("  , ASSET.ASSET_ID ")
            sbSQL.AppendLine("  , ASSET.SUB_ASSET_NO ")
            sbSQL.AppendLine("  , ASSET.EPC_SIZE ")
            sbSQL.AppendLine("  , ASSET.GEN_ASSET_ID ")
            sbSQL.AppendLine("  , ASSET.ASSET_DIV ")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("  , ASSET.MNG_LOC_L_CD ") '2021/09/16 nv miyaji add
            sbSQL.AppendLine("  , ASSET.LOC_L_CD ")
            sbSQL.AppendLine("  , ASSET.LOC_M_CD ")
            sbSQL.AppendLine("  , ASSET.LOC_S_CD ")
            sbSQL.AppendLine("  , ASSET.CAT_L_CD ")
            sbSQL.AppendLine("  , ASSET.CAT_M_CD ")
            sbSQL.AppendLine("  , ASSET.CAT_S_CD ")
            sbSQL.AppendLine("  , ASSET.MAKER ")
            sbSQL.AppendLine("  , ASSET.PRODUCT_NO ")
            sbSQL.AppendLine("  , ASSET.MODEL_NO ")
            sbSQL.AppendLine("  , ASSET.LEASE_NO ")
            sbSQL.AppendLine("  , ASSET.LEASE_LIMIT ")
            sbSQL.AppendLine("  , ASSET.GENERIC_LIMIT ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM1 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM2 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM3 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM4 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM5 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM6 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM7 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM8 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM9 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM10 ")
            '2023/02/08 Kawamoto ADD START
            sbSQL.AppendLine("  , ASSET.FREE_ITEM11 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM12 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM13 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM14 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM15 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM16 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM17 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM18 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM19 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM20 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM21 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM22 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM23 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM24 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM25 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM26 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM27 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM28 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM29 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM30 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM31 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM32 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM33 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM34 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM35 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM36 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM37 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM38 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM39 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM40 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM41 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM42 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM43 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM44 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM45 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM46 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM47 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM48 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM49 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM50 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM51 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM52 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM53 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM54 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM55 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM56 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM57 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM58 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM59 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM60 ")
            '2023/02/08 Kawamoto ADD START
            sbSQL.AppendLine("  , ASSET.ASSET_PIC_FILENAME ")
            sbSQL.AppendLine("  , ASSET.LEASE_CNT ")
            sbSQL.AppendLine("  , ASSET.ASSET_STATUS ")
            sbSQL.AppendLine("  , ASSET.EPC_ENC_STATUS ")
            sbSQL.AppendLine("  , ASSET.TAG_LINK_STATUS ")
            sbSQL.AppendLine("  , ASSET.LEASE_STATUS ") '2021/09/16 nv miyaji add
            sbSQL.AppendLine("  , ASSET.LEASE_PLACE_CD ")
            sbSQL.AppendLine("  , ASSET.LEASE_PLACE_NAME ")
            sbSQL.AppendLine("  , ASSET.REMARK ")
            sbSQL.AppendLine("  , ASSET.LEASE_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.LEASE_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.LEASE_DT ")
            sbSQL.AppendLine("  , ASSET.LEASE_DUE_DT ")
            '--2021/09/16 nv miyaji add start
            sbSQL.AppendLine("  , ASSET.MAINTE_LEASE_NUM ")
            sbSQL.AppendLine("  , ASSET.MAINTE_LEASE_DAY ")
            sbSQL.AppendLine("  , ASSET.MAINTE_LEASE_DAY_WORK_FLAG ")
            sbSQL.AppendLine("  , ASSET.MAINTE_DAY ")
            sbSQL.AppendLine("  , ASSET.MAINTE_DAY_WORK_FLAG ")
            '--2021/09/16 nv miyaji add end
            sbSQL.AppendLine("  , ASSET.RETIRE_TS ")
            sbSQL.AppendLine("  , ASSET.RETIRE_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.RETIRE_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.RETIRE_ASSET_ID ")
            sbSQL.AppendLine("  , ASSET.PHYINV_TS ")
            sbSQL.AppendLine("  , ASSET.PHYINV_STATUS ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_L_CD ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_M_CD ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_S_CD ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_L_NAME ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_M_NAME ")
            sbSQL.AppendLine("  , ASSET.PHYINV_LOC_S_NAME ")
            sbSQL.AppendLine("  , ASSET.PHYINV_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.PHYINV_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.TASK_TS ")
            sbSQL.AppendLine("  , ASSET.IS_TANGIBLE ")
            sbSQL.AppendLine("  , ASSET.REMARK1 ")
            sbSQL.AppendLine("  , ASSET.REMARK2 ")
            sbSQL.AppendLine("  , ASSET.REMARK3 ")
            sbSQL.AppendLine("  , ASSET.REMARK4 ")
            sbSQL.AppendLine("  , ASSET.REMARK5 ")
            sbSQL.AppendLine("  , ASSET.REMARK6 ")
            sbSQL.AppendLine("  , ASSET.REMARK7 ")
            sbSQL.AppendLine("  , ASSET.REMARK8 ")
            sbSQL.AppendLine("  , ASSET.REMARK9 ")
            sbSQL.AppendLine("  , ASSET.REMARK10 ")
            sbSQL.AppendLine("  , MNGEMP.EMPLOYEE_NAME AS FREE_ITEM6_NAME ")
            sbSQL.AppendLine("  , ASSET.INS_DATETIME ")
            sbSQL.AppendLine("  , ASSET.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.UPD_DATETIME ")
            sbSQL.AppendLine("  , ASSET.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.UPD_TASK ")
            sbSQL.AppendLine("  , LOCL.LOC_NAME AS LocLName ")
            sbSQL.AppendLine("  , LOCM.LOC_NAME AS LocMName ")
            sbSQL.AppendLine("  , LOCS.LOC_NAME AS LocSName ")
            sbSQL.AppendLine("  , CATL.CAT_NAME AS CatLName ")
            sbSQL.AppendLine("  , CATM.CAT_NAME AS CatMName ")
            sbSQL.AppendLine("  , CATS.CAT_NAME AS CatSName ")
            sbSQL.AppendLine("  , ULOC.UPD_FLG AS UPD_FLG ") '2021/10/01 nv miyaji
            sbSQL.AppendLine("  , MNGLOCL.LOC_NAME AS MngLocLName ")    '2021/09/17 nv miyaji add
            sbSQL.AppendLine("  , " & sbAlertDays.ToString & " AS LEASE_LIMIT_BEF_DAYS")
            sbSQL.AppendLine("  , " & sbGenLimitDays.ToString & " AS GENERIC_LIMIT_BEF_DAYS")
            sbSQL.AppendLine("  , " & sbLeaseAlertCnt.ToString & " AS LEASE_ALERT_CNT")
            sbSQL.AppendLine("  , TotalCount AS TotalCount ")
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  M_ASSET ASSET ")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC")
            sbSQL.AppendLine("ON ASSET.MNG_LOC_L_CD = ULOC.LOC_L_CD ")
            sbSQL.AppendLine("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = ASSET.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")

            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
            '--2021/09/17 nv miyaji add start
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL ")
            sbSQL.AppendLine("ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")
            '--2021/09/17 nv miyaji add end
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM ")
            sbSQL.AppendLine("ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCM.LOC_L_CD")
            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS ")
            sbSQL.AppendLine("ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCS.LOC_L_CD")
            sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCS.LOC_M_CD")
            sbSQL.AppendLine("AND ASSET.LOC_S_CD = LOCS.LOC_S_CD")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATL ")
            sbSQL.AppendLine("ON CATL.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATL.CAT_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATM ")
            sbSQL.AppendLine("ON CATM.CAT_DIV = '" + CAT_DIV.CategoryM + "'")
            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATM.CAT_L_CD")
            sbSQL.AppendLine("AND ASSET.CAT_M_CD = CATM.CAT_M_CD")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATS ")
            sbSQL.AppendLine("ON CATS.CAT_DIV = '" + CAT_DIV.CategoryS + "'")
            sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATS.CAT_L_CD")
            sbSQL.AppendLine("AND ASSET.CAT_M_CD = CATS.CAT_M_CD")
            sbSQL.AppendLine("AND ASSET.CAT_S_CD = CATS.CAT_S_CD")

            sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE MNGEMP ")
            sbSQL.AppendLine("ON ASSET.FREE_ITEM6 = MNGEMP.EMPLOYEE_CD")

            sbSQL.AppendLine("LEFT JOIN (")
            sbSQL.AppendLine("  SELECT COUNT(*) TotalCount")
            sbSQL.AppendLine("  FROM M_ASSET ASSET ")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC")
            sbSQL.AppendLine("ON ASSET.MNG_LOC_L_CD = ULOC.LOC_L_CD ")
            sbSQL.AppendLine("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = ASSET.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
            sbSQL.AppendLine("  WHERE 1 = 1")
            sbSQL.AppendLine("  " & sbWhr.ToString)
            sbSQL.AppendLine(") A")
            sbSQL.AppendLine("ON 1=1")

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
            sbSQL.AppendLine("  1 = 1 ")

            sbSQL.AppendLine(sbWhr.ToString)
            'Where文を未タグ紐づけ状態メッセージ表示のためのセッションに保存
            Session(CommonSessionKey.LIST_VIEW_SQL_WHERE) = sbWhr.ToString

            'ORDER句の設定
            If _isSort Then
                If String.IsNullOrEmpty(sSelKey) Then
                    '表示順 SearchDispSelect
                    If Not IsNothing(_ViewModel.SearchDispSelect) Then
                        Select Case _ViewModel.SearchDispSelect
                            Case AssetDispSelect.Loca
                                sbSQL.AppendLine(" ORDER BY ASSET.LOC_L_CD,")
                                sbSQL.AppendLine("          ASSET.LOC_M_CD,")
                                sbSQL.AppendLine("          ASSET.LOC_S_CD,")
                                sbSQL.AppendLine("          ASSET_NO")
                            Case AssetDispSelect.Cat
                                sbSQL.AppendLine(" ORDER BY ASSET.CAT_L_CD,")
                                sbSQL.AppendLine("          ASSET.CAT_M_CD,")
                                sbSQL.AppendLine("          ASSET.CAT_S_CD,")
                                sbSQL.AppendLine("          ASSET_NO")
                            Case AssetDispSelect.Asset
                                sbSQL.AppendLine(" ORDER BY ASSET_NO")
                        ' 2023/02/06 Kawamoto ADD START
                            Case AssetDispSelect.Rate
                                sbSQL.AppendLine(" ORDER BY FREE_ITEM17")
                                ' 2023/02/06 Kawamoto ADD END
                            Case AssetDispSelect.AssetNoAsc
                                sbSQL.AppendLine(" ORDER BY ASSET_NO")
                            Case Else
                                sbSQL.AppendLine(" ORDER BY " & GetSortColumn(_ViewModel.SearchDispSelect) & ",")
                                sbSQL.AppendLine("          ASSET_NO")
                        End Select
                    End If
                Else
                    '----- 在庫照会時の検索条件
                    sbSQL.AppendLine(" ORDER BY ASSET.LOC_L_CD,")
                    sbSQL.AppendLine("          ASSET.LOC_M_CD,")
                    sbSQL.AppendLine("          ASSET.LOC_S_CD,")
                    sbSQL.AppendLine("          ASSET.CAT_L_CD,")
                    sbSQL.AppendLine("          ASSET.CAT_M_CD,")
                    sbSQL.AppendLine("          ASSET.CAT_S_CD,")
                    sbSQL.AppendLine("          ASSET_NO")
                End If
            End If


            'クエリを返却
            Return sbSQL.ToString
        End Function

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New AssetListViewModel())
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
            _viewResult.ElementAt(0).SearchCatM = _ViewModel.SearchCatM
            _viewResult.ElementAt(0).SearchCatS = _ViewModel.SearchCatS
            _viewResult.ElementAt(0).SearchMaker = _ViewModel.SearchMaker
            _viewResult.ElementAt(0).SearchProductNo = _ViewModel.SearchProductNo
            _viewResult.ElementAt(0).SearchModelNo = _ViewModel.SearchModelNo
            _viewResult.ElementAt(0).SearchAssetNo = _ViewModel.SearchAssetNo
            _viewResult.ElementAt(0).SearchAssetName = _ViewModel.SearchAssetName
            _viewResult.ElementAt(0).SearchLeasePlaceCd = _ViewModel.SearchLeasePlaceCd
            _viewResult.ElementAt(0).SearchIsTangibleY = _ViewModel.SearchIsTangibleY
            _viewResult.ElementAt(0).SearchIsTangibleN = _ViewModel.SearchIsTangibleN
            _viewResult.ElementAt(0).SearchFreeItem1 = _ViewModel.SearchFreeItem1
            _viewResult.ElementAt(0).SearchFreeItem2 = _ViewModel.SearchFreeItem2
            _viewResult.ElementAt(0).SearchFreeItem3 = _ViewModel.SearchFreeItem3
            _viewResult.ElementAt(0).SearchFreeItem4 = _ViewModel.SearchFreeItem4
            _viewResult.ElementAt(0).SearchFreeItem5 = _ViewModel.SearchFreeItem5
            _viewResult.ElementAt(0).SearchFreeItem6 = _ViewModel.SearchFreeItem6
            _viewResult.ElementAt(0).SearchFreeItem7 = _ViewModel.SearchFreeItem7
            _viewResult.ElementAt(0).SearchFreeItem8 = _ViewModel.SearchFreeItem8
            _viewResult.ElementAt(0).SearchFreeItem9 = _ViewModel.SearchFreeItem9
            _viewResult.ElementAt(0).SearchFreeItem10 = _ViewModel.SearchFreeItem10
            _viewResult.ElementAt(0).SearchFreeItem11 = _ViewModel.SearchFreeItem11
            _viewResult.ElementAt(0).SearchFreeItem11From = _ViewModel.SearchFreeItem11From
            _viewResult.ElementAt(0).SearchFreeItem11To = _ViewModel.SearchFreeItem11To
            _viewResult.ElementAt(0).SearchFreeItem12 = _ViewModel.SearchFreeItem12
            _viewResult.ElementAt(0).SearchFreeItem12From = _ViewModel.SearchFreeItem12From
            _viewResult.ElementAt(0).SearchFreeItem12To = _ViewModel.SearchFreeItem12To
            _viewResult.ElementAt(0).SearchFreeItem13 = _ViewModel.SearchFreeItem13
            _viewResult.ElementAt(0).SearchFreeItem14 = _ViewModel.SearchFreeItem14
            _viewResult.ElementAt(0).SearchFreeItem15 = _ViewModel.SearchFreeItem15
            _viewResult.ElementAt(0).SearchFreeItem16 = _ViewModel.SearchFreeItem16
            _viewResult.ElementAt(0).SearchFreeItem17 = _ViewModel.SearchFreeItem17
            _viewResult.ElementAt(0).SearchFreeItem18 = _ViewModel.SearchFreeItem18
            _viewResult.ElementAt(0).SearchFreeItem19 = _ViewModel.SearchFreeItem19
            _viewResult.ElementAt(0).SearchFreeItem20 = _ViewModel.SearchFreeItem20
            _viewResult.ElementAt(0).SearchFreeItem20From = _ViewModel.SearchFreeItem20From
            _viewResult.ElementAt(0).SearchFreeItem20To = _ViewModel.SearchFreeItem20To
            _viewResult.ElementAt(0).SearchFreeItem21 = _ViewModel.SearchFreeItem21
            _viewResult.ElementAt(0).SearchFreeItem37 = _ViewModel.SearchFreeItem37
            _viewResult.ElementAt(0).SearchFreeItem39 = _ViewModel.SearchFreeItem39
            _viewResult.ElementAt(0).SearchFreeItem42 = _ViewModel.SearchFreeItem42
            _viewResult.ElementAt(0).SearchFreeItem2From = _ViewModel.SearchFreeItem2From
            _viewResult.ElementAt(0).SearchFreeItem2To = _ViewModel.SearchFreeItem2To
            _viewResult.ElementAt(0).SearchFreeItem18From = _ViewModel.SearchFreeItem18From
            _viewResult.ElementAt(0).SearchFreeItem18To = _ViewModel.SearchFreeItem18To
            _viewResult.ElementAt(0).SearchFreeItem19From = _ViewModel.SearchFreeItem19From
            _viewResult.ElementAt(0).SearchFreeItem19To = _ViewModel.SearchFreeItem19To
            _viewResult.ElementAt(0).SearchFreeItem27 = _ViewModel.SearchFreeItem27
            _viewResult.ElementAt(0).SearchRemark1 = _ViewModel.SearchRemark1
            _viewResult.ElementAt(0).SearchRemark2 = _ViewModel.SearchRemark2
            _viewResult.ElementAt(0).SearchRemark3 = _ViewModel.SearchRemark3
            _viewResult.ElementAt(0).SearchRemark4 = _ViewModel.SearchRemark4
            _viewResult.ElementAt(0).SearchRemark5 = _ViewModel.SearchRemark5
            _viewResult.ElementAt(0).SearchRemark6 = _ViewModel.SearchRemark6
            _viewResult.ElementAt(0).SearchRemark7 = _ViewModel.SearchRemark7
            _viewResult.ElementAt(0).SearchRemark8 = _ViewModel.SearchRemark8
            _viewResult.ElementAt(0).SearchRemark9 = _ViewModel.SearchRemark9
            _viewResult.ElementAt(0).SearchRemark10 = _ViewModel.SearchRemark10
            _viewResult.ElementAt(0).SearchRemarks = _ViewModel.SearchRemarks
            _viewResult.ElementAt(0).SearchLeaseCntFrom = _ViewModel.SearchLeaseCntFrom
            _viewResult.ElementAt(0).SearchLeaseCntTo = _ViewModel.SearchLeaseCntTo
            _viewResult.ElementAt(0).SearchLeaseLimitExpired = _ViewModel.SearchLeaseLimitExpired
            _viewResult.ElementAt(0).SearchLeaseLimitAlert = _ViewModel.SearchLeaseLimitAlert
            _viewResult.ElementAt(0).SearchLeaseLimitKikan = _ViewModel.SearchLeaseLimitKikan
            _viewResult.ElementAt(0).SearchLeaseLimitFrom = _ViewModel.SearchLeaseLimitFrom
            _viewResult.ElementAt(0).SearchLeaseLimitTo = _ViewModel.SearchLeaseLimitTo
            _viewResult.ElementAt(0).SearchAssetStatus = CommonRfid.ToStringNvl(_ViewModel.SearchAssetStatus)
            _viewResult.ElementAt(0).SearchGenericLimitExpired = _ViewModel.SearchGenericLimitExpired
            _viewResult.ElementAt(0).SearchGenericLimitAlert = _ViewModel.SearchGenericLimitAlert
            _viewResult.ElementAt(0).SearchGenericLimitKikan = _ViewModel.SearchGenericLimitKikan
            _viewResult.ElementAt(0).SearchGenericLimitFrom = _ViewModel.SearchGenericLimitFrom
            _viewResult.ElementAt(0).SearchGenericLimitTo = _ViewModel.SearchGenericLimitTo
            _viewResult.ElementAt(0).SearchEpcEncStatus = CommonRfid.ToStringNvl(_ViewModel.SearchEpcEncStatus)
            _viewResult.ElementAt(0).SearchTagLinkStatus = CommonRfid.ToStringNvl(_ViewModel.SearchTagLinkStatus)
            _viewResult.ElementAt(0).SearchDispSelect = _ViewModel.SearchDispSelect
            _viewResult.ElementAt(0).SearchLeaseAlert = _ViewModel.SearchLeaseAlert '2020/02/28 nv miyaji add
            _viewResult.ElementAt(0).SearchLeaseStatus_Ok = _ViewModel.SearchLeaseStatus_Ok '2021/09/17 nv miyaji add
            _viewResult.ElementAt(0).SearchLeaseStatus_Ng = _ViewModel.SearchLeaseStatus_Ng '2021/09/17 nv miyaji add
            _viewResult.ElementAt(0).SearchMngLocL = _ViewModel.SearchMngLocL '2021/09/17 nv miyaji add
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_LIST, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL))
            If _viewResult.ElementAt(0).GridInfo.Count > 0 Then
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.ASSET_LIST, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL)).FIX_COL_CNT
            Else
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.ASSET_LIST, COLSET_LOC.TEMPLATE).FIX_COL_CNT
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_LIST, COLSET_LOC.TEMPLATE)
            End If
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
            _viewResult.ElementAt(0).DetailOpenFlg = _ViewModel.DetailOpenFlg
            _viewResult.ElementAt(0).ATTACHMENT_NUM = _ViewModel.ATTACHMENT_NUM '2023/03/22 Kawamoto ADD
            _viewResult.ElementAt(0).IsRestoreOwnSearchConditions = _ViewModel.IsRestoreOwnSearchConditions
            _viewResult.ElementAt(0).FirstSearch = _ViewModel.FirstSearch

        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As AssetListViewModel)
            _Model.ASSET_NO = model.ASSET_NO
            _Model.SUB_ASSET_NO = model.SUB_ASSET_NO
            _Model.ASSET_ID = model.ASSET_ID
            _Model.EPC_SIZE = model.EPC_SIZE
            _Model.GEN_ASSET_ID = model.GEN_ASSET_ID
            _Model.ASSET_DIV = model.ASSET_DIV
            _Model.ASSET_NAME = model.ASSET_NAME
            _Model.MNG_LOC_L_CD = model.MNG_LOC_L_CD '2021/09/16 nv miyaji add
            _Model.LOC_L_CD = model.LOC_L_CD
            _Model.LOC_M_CD = model.LOC_M_CD
            _Model.LOC_S_CD = model.LOC_S_CD
            _Model.CAT_L_CD = model.CAT_L_CD
            _Model.CAT_M_CD = model.CAT_M_CD
            _Model.CAT_S_CD = model.CAT_S_CD
            _Model.MAKER = model.MAKER
            _Model.PRODUCT_NO = model.PRODUCT_NO
            _Model.MODEL_NO = model.MODEL_NO
            _Model.LEASE_NO = model.LEASE_NO
            _Model.LEASE_LIMIT = model.LEASE_LIMIT
            _Model.GENERIC_LIMIT = model.GENERIC_LIMIT
            _Model.FREE_ITEM1 = model.FREE_ITEM1
            _Model.FREE_ITEM2 = model.FREE_ITEM2
            _Model.FREE_ITEM3 = model.FREE_ITEM3
            _Model.FREE_ITEM4 = model.FREE_ITEM4
            _Model.FREE_ITEM5 = model.FREE_ITEM5
            _Model.FREE_ITEM6 = model.FREE_ITEM6
            _Model.FREE_ITEM7 = model.FREE_ITEM7
            _Model.FREE_ITEM8 = model.FREE_ITEM8
            _Model.FREE_ITEM9 = model.FREE_ITEM9
            _Model.FREE_ITEM10 = model.FREE_ITEM10
            _Model.FREE_ITEM11 = model.FREE_ITEM11
            _Model.FREE_ITEM12 = model.FREE_ITEM12
            _Model.FREE_ITEM13 = model.FREE_ITEM13
            _Model.FREE_ITEM14 = model.FREE_ITEM14
            _Model.FREE_ITEM15 = model.FREE_ITEM15
            _Model.FREE_ITEM16 = model.FREE_ITEM16
            _Model.FREE_ITEM17 = model.FREE_ITEM17
            _Model.FREE_ITEM18 = model.FREE_ITEM18
            _Model.FREE_ITEM19 = model.FREE_ITEM19
            _Model.FREE_ITEM20 = model.FREE_ITEM20
            _Model.FREE_ITEM21 = model.FREE_ITEM21
            _Model.FREE_ITEM22 = model.FREE_ITEM22
            _Model.FREE_ITEM23 = model.FREE_ITEM23
            _Model.FREE_ITEM24 = model.FREE_ITEM24
            _Model.FREE_ITEM25 = model.FREE_ITEM25
            _Model.FREE_ITEM26 = model.FREE_ITEM26
            _Model.FREE_ITEM27 = model.FREE_ITEM27
            _Model.FREE_ITEM28 = model.FREE_ITEM28
            _Model.FREE_ITEM29 = model.FREE_ITEM29
            _Model.FREE_ITEM30 = model.FREE_ITEM30
            _Model.FREE_ITEM31 = model.FREE_ITEM31
            _Model.FREE_ITEM32 = model.FREE_ITEM32
            _Model.FREE_ITEM33 = model.FREE_ITEM33
            _Model.FREE_ITEM34 = model.FREE_ITEM34
            _Model.FREE_ITEM35 = model.FREE_ITEM35
            _Model.FREE_ITEM36 = model.FREE_ITEM36
            _Model.FREE_ITEM37 = model.FREE_ITEM37
            _Model.FREE_ITEM38 = model.FREE_ITEM38
            _Model.FREE_ITEM39 = model.FREE_ITEM39
            _Model.FREE_ITEM40 = model.FREE_ITEM40
            _Model.FREE_ITEM41 = model.FREE_ITEM41
            _Model.FREE_ITEM42 = model.FREE_ITEM42
            _Model.FREE_ITEM43 = model.FREE_ITEM43
            _Model.FREE_ITEM44 = model.FREE_ITEM44
            _Model.FREE_ITEM45 = model.FREE_ITEM45
            _Model.FREE_ITEM46 = model.FREE_ITEM46
            _Model.FREE_ITEM47 = model.FREE_ITEM47
            _Model.FREE_ITEM48 = model.FREE_ITEM48
            _Model.FREE_ITEM49 = model.FREE_ITEM49
            _Model.FREE_ITEM50 = model.FREE_ITEM50
            _Model.FREE_ITEM51 = model.FREE_ITEM51
            _Model.FREE_ITEM52 = model.FREE_ITEM52
            _Model.FREE_ITEM53 = model.FREE_ITEM53
            _Model.FREE_ITEM54 = model.FREE_ITEM54
            _Model.FREE_ITEM55 = model.FREE_ITEM55
            _Model.FREE_ITEM56 = model.FREE_ITEM56
            _Model.FREE_ITEM57 = model.FREE_ITEM57
            _Model.FREE_ITEM58 = model.FREE_ITEM58
            _Model.FREE_ITEM59 = model.FREE_ITEM59
            _Model.FREE_ITEM60 = model.FREE_ITEM60
            _Model.REMARK1 = model.REMARK1
            _Model.REMARK2 = model.REMARK2
            _Model.REMARK3 = model.REMARK3
            _Model.REMARK4 = model.REMARK4
            _Model.REMARK5 = model.REMARK5
            _Model.REMARK6 = model.REMARK6
            _Model.REMARK7 = model.REMARK7
            _Model.REMARK8 = model.REMARK8
            _Model.REMARK9 = model.REMARK9
            _Model.REMARK10 = model.REMARK10
            _Model.ASSET_PIC_FILENAME = model.ASSET_PIC_FILENAME
            _Model.ASSET_STATUS = model.ASSET_STATUS
            _Model.EPC_ENC_STATUS = model.EPC_ENC_STATUS
            _Model.TAG_LINK_STATUS = model.TAG_LINK_STATUS
            _Model.LEASE_STATUS = model.LEASE_STATUS '2021/09/16 nv miyaji add
            If model.RETIRE_TS Is Nothing Then
                '登録、除却以外からの変更
                _Model.LEASE_CNT = model.LEASE_CNT
                _Model.LEASE_PLACE_CD = model.LEASE_PLACE_CD
                _Model.LEASE_PLACE_NAME = model.LEASE_PLACE_NAME
                _Model.REMARK = model.REMARK
                _Model.LEASE_EMPLOYEE_CD = model.LEASE_EMPLOYEE_CD
                _Model.LEASE_EMPLOYEE_NAME = model.LEASE_EMPLOYEE_NAME
                _Model.LEASE_DT = model.LEASE_DT
                _Model.LEASE_DUE_DT = model.LEASE_DUE_DT
                _Model.RETIRE_TS = model.RETIRE_TS
                _Model.RETIRE_EMPLOYEE_CD = model.RETIRE_EMPLOYEE_CD
                _Model.RETIRE_EMPLOYEE_NAME = model.RETIRE_EMPLOYEE_NAME
                _Model.RETIRE_ASSET_ID = model.RETIRE_ASSET_ID
                _Model.PHYINV_TS = model.PHYINV_TS
                _Model.PHYINV_STATUS = model.PHYINV_STATUS
                _Model.PHYINV_LOC_L_CD = model.PHYINV_LOC_L_CD
                _Model.PHYINV_LOC_M_CD = model.PHYINV_LOC_M_CD
                _Model.PHYINV_LOC_S_CD = model.PHYINV_LOC_S_CD
                _Model.PHYINV_LOC_L_NAME = model.PHYINV_LOC_L_NAME
                _Model.PHYINV_LOC_M_NAME = model.PHYINV_LOC_M_NAME
                _Model.PHYINV_LOC_S_NAME = model.PHYINV_LOC_S_NAME
                _Model.PHYINV_EMPLOYEE_CD = model.PHYINV_EMPLOYEE_CD
                _Model.PHYINV_EMPLOYEE_NAME = model.PHYINV_EMPLOYEE_NAME
            Else
                '除却状態からの変更の場合、除却解除処理＋変更をする
                _Model.LEASE_CNT = 0
                _Model.LEASE_PLACE_CD = Nothing
                _Model.LEASE_PLACE_NAME = Nothing
                _Model.REMARK = Nothing
                _Model.LEASE_EMPLOYEE_CD = Nothing
                _Model.LEASE_EMPLOYEE_NAME = Nothing
                _Model.LEASE_DT = Nothing
                _Model.LEASE_DUE_DT = Nothing
                _Model.RETIRE_TS = Nothing
                _Model.RETIRE_EMPLOYEE_CD = Nothing
                _Model.RETIRE_EMPLOYEE_NAME = Nothing
                _Model.RETIRE_ASSET_ID = Nothing
                _Model.PHYINV_TS = Nothing
                _Model.PHYINV_STATUS = Nothing
                _Model.PHYINV_LOC_L_CD = Nothing
                _Model.PHYINV_LOC_M_CD = Nothing
                _Model.PHYINV_LOC_S_CD = Nothing
                _Model.PHYINV_LOC_L_NAME = Nothing
                _Model.PHYINV_LOC_M_NAME = Nothing
                _Model.PHYINV_LOC_S_NAME = Nothing
                _Model.PHYINV_EMPLOYEE_CD = Nothing
                _Model.PHYINV_EMPLOYEE_NAME = Nothing
            End If
            '--2021/09/16 nv miyaji add start
            _Model.MAINTE_LEASE_NUM = model.MAINTE_LEASE_NUM
            _Model.MAINTE_LEASE_DAY = model.MAINTE_LEASE_DAY
            _Model.MAINTE_LEASE_DAY_WORK_FLAG = model.MAINTE_LEASE_DAY_WORK_FLAG
            _Model.MAINTE_DAY = model.MAINTE_DAY
            _Model.MAINTE_DAY_WORK_FLAG = model.MAINTE_DAY_WORK_FLAG
            '--2021/09/16 nv miyaji add end
            _Model.TASK_TS = model.TASK_TS
            _Model.IS_TANGIBLE = model.IS_TANGIBLE
            _Model.INS_DATETIME = model.INS_DATETIME
            _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub
#End Region

        '2023/02/08 Kawamoto ADD START
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
#Region "【未使用・コメントアウト】CalsConsumptionRate（残量割合(残量/内容量)の算出）"
'#Region "残量割合(残量/内容量)の算出"
        '''' <summary>
        '''' 残量割合(残量/内容量)の算出
        '''' </summary>
        '''' <param name="prmKg">内容量(kg)</param>
        '''' <param name="prmL">内容量(L)</param>
        '''' <param name="prmSpecificGravity">比重</param>
        '''' <param name="prmWeightIncludContainer">受入時の容器を含む重量(kg)</param>
        '''' <param name="prmWeight">残量(NET重量)</param>
        '''' <returns>残量割合(残量/内容量)</returns>
        'Private Function CalsConsumptionRate(ByVal prmKg As String,
        '                                     ByVal prmL As String,
        '                                     ByVal prmSpecificGravity As String,
        '                                     ByVal prmWeightIncludContainer As String,
        '                                     ByVal prmWeight As String) As String
        '    CalsConsumptionRate = String.Empty
        '    Dim rate As Decimal = 0

        '    'kgとLのどちらを使用するか判定
        '    If String.IsNullOrEmpty(prmKg) Then
        '        '◆L
        '        '残量(NET重量) / ( 内容量(L) * 比重 )
        '        If Not String.IsNullOrEmpty(prmL) Then
        '            rate = Decimal.Parse(prmWeight) / (Decimal.Parse(prmL) * Decimal.Parse(prmSpecificGravity))
        '        End If
        '    Else
        '        '◆kg
        '        '残量(NET重量) / 内容量(kg)
        '        rate = Decimal.Parse(prmWeight) / Decimal.Parse(prmKg)
        '    End If

        '    '小数点2位以下を切り捨て
        '    'CalsConsumptionRate = Math.Round(rate, 2, MidpointRounding.AwayFromZero).ToString
        '    CalsConsumptionRate = (Math.Floor(rate * 10) / 10).ToString

        'End Function
'#End Region
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End

#Region "返却残量の算出"
        '''' <summary>
        '''' 返却残量の算出    '2023/6/19 DEL Sato 未使用
        '''' </summary>
        '''' <param name="prmUnit">単位("L" or "KG")</param>
        '''' <param name="prmKg">内容量(kg)</param>
        '''' <param name="prmL">内容量(L)</param>
        '''' <param name="prmSpecificGravity">比重</param>
        '''' <param name="prmWeightIncludContainer">受入時の容器を含む重量(kg)</param>
        '''' <param name="prmWeight">残量(NET重量)</param>
        '''' <returns>返却残量</returns>
        'Private Function CalsRemainQty(ByVal prmUnit As String, 
        '                               ByVal prmKg As String,
        '                               ByVal prmL As String,
        '                               ByVal prmSpecificGravity As String,
        '                               ByVal prmWeightIncludContainer As String,
        '                               ByVal prmWeight As String) As String
        '    CalsRemainQty = String.Empty
        '    Dim qty As Decimal = 0

        '    '単位が"KG"の場合
        '    If prmUnit = DrugsFSActNumber.Data3.KG Then
        '        If Not String.IsNullOrEmpty(prmKg) Then
        '            '残量(NET重量) - ( 受入時の容器を含む重量(kg) - 内容量(kg) )
        '            qty = Decimal.Parse(prmWeight) - (Decimal.Parse(prmWeightIncludContainer) - Decimal.Parse(prmKg))
        '        End If

        '    '単位が"L"の場合
        '    Else
        '        If Not String.IsNullOrEmpty(prmL) AndAlso Not String.IsNullOrEmpty(prmSpecificGravity) Then
        '            '残量(NET重量) - ( 受入時の容器を含む重量(kg) - ( 内容量(L) * 比重 ) )
        '            qty = Decimal.Parse(prmWeight) - (Decimal.Parse(prmWeightIncludContainer) - (Decimal.Parse(prmL) * Decimal.Parse(prmSpecificGravity)))
        '        End If

        '    End If

        '    CalsRemainQty = Math.Round(qty, 2, MidpointRounding.AwayFromZero).ToString

        'End Function
#End Region
        '2023/02/08 Kawamoto ADD END

#Region "Webで使用可能なパスへ変換"
        ''' <summary>
        ''' Webで使用可能なパスへ変換
        ''' </summary>
        ''' <param name="checkPath">チェック対象のパス</param>
        ''' <returns></returns>
        Private Function ConvertEnablePath(ByVal checkPath As String) As String

            '相対パスの場合、変換
            If Path.IsPathRooted(checkPath) = False Then
                Return Server.MapPath(checkPath)
            End If

            '絶対パスの場合、そのまま返却
            Return checkPath
        End Function
#End Region

#Region "資産番号の自動生成要求"
        ''' <summary>
        ''' 資産番号の自動生成要求
        ''' </summary>
        ''' <returns></returns>
        Public Function RequestAssetNo() As ActionResult
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '資産番号の自動生成確認
            If Not IsAutoAssetNo() Then
                '資産番号の自動生成なし
                Return Content("")
            End If

            '資産番号を生成
            Dim assetNo As String = AssetNoAutoGenelate()

            Return Content(assetNo)
        End Function

#End Region

#Region "資産番号の自動生成有無確認"
        Private Function IsAutoAssetNo() As Boolean
            '資産番号の自動生成設定を取得
            Dim AssetNoSelfReg As SettingInfo = _Message.getSettingInfo(CommonSettingKey.AssetNo_Self_Regist.KeyDiv,
                                                                  CommonSettingKey.AssetNo_Self_Regist.Key1,
                                                                  CommonSettingKey.AssetNo_Self_Regist.Key2,
                                                                  CommonSettingKey.AssetNo_Self_Regist.Key3, True)

            If Not IsNothing(AssetNoSelfReg) AndAlso AssetNoSelfReg.DATA1 <> ValidFlg.Cd.Valid Then
                Return False
            Else
                Return True
            End If
        End Function
#End Region

#Region "資産番号の自動生成"
        ''' <summary>
        ''' 資産番号の自動生成
        ''' </summary>
        ''' <returns></returns>
        Private Function AssetNoAutoGenelate(Optional assetNoSeq As Integer = 0) As String
            Dim sToDayYMD As String = Now.ToString("yyMMdd")
            Dim iNextAssetSeq As Integer

            If assetNoSeq > 0 Then
                '指定の連番を使用
                iNextAssetSeq = assetNoSeq
            Else
                '連番を採番
                iNextAssetSeq = AssetNoAutoNumberingSeq()
            End If

            '資産番号を生成
            Dim assetNo As String = sToDayYMD & "-" & iNextAssetSeq.ToString.PadLeft(itemlength.AssetNoAutoSeq, "0")

            Return assetNo
        End Function

        ''' <summary>
        ''' 資産番号の自動採番 連番を取得
        ''' </summary>
        ''' <returns></returns>
        Private Function AssetNoAutoNumberingSeq() As Integer
            Dim sToDayYMD As String = Now.ToString("yyMMdd")
            Dim iNextAssetSeq As Integer = 0

            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT TOP 1")
            sbSQL.AppendLine("  ASSET_NO")
            sbSQL.AppendLine("FROM M_ASSET")
            sbSQL.AppendLine("WHERE ASSET_NO LIKE '" & sToDayYMD & "%'")
            sbSQL.AppendLine("ORDER BY ASSET_NO DESC")

            'SQLを実行する
            Dim selectData As DbRawSqlQuery(Of MstAssetModel) = _db.Database.SqlQuery(Of MstAssetModel)(sbSQL.ToString)
            If selectData.Count > 0 Then
                Dim assetModel As MstAssetModel = selectData.ToList(0)

                '資産番号から連番を切り出し
                Dim sTmpAssetSeq As String = assetModel.ASSET_NO.Substring(assetModel.ASSET_NO.Length - itemlength.AssetNoAutoSeq)

                '数値ではない場合は連番値を1にする
                Integer.TryParse(sTmpAssetSeq, iNextAssetSeq)
            End If

            iNextAssetSeq += 1

            Return iNextAssetSeq
        End Function

        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト Start
        '　資産一覧のCSV取込は今回のカスタマイズに無い（CSV取込ボタンは 2021/10/01 にコメントアウト済みで呼び出し元が無い）ため、CSV取込からしか呼ばれない処理をコメントアウトする（2026/10/02 指示）
        '　「資産番号の自動生成」の他の2関数（AssetNoAutoGenelate・AssetNoAutoNumberingSeq）は新規登録・資産番号採番から呼ばれるため残す
#Region "【未使用・コメントアウト】IsAssetIdDuplicate（資産IDの重複確認）"
        '''' <summary>
        '''' 資産IDの重複確認
        '''' </summary>
        '''' <returns>True:重複あり、False：重複なし</returns>
        'Private Function IsAssetIdDuplicate(ByVal assetId As String) As Boolean

        '    Dim sbSQL As StringBuilder = New StringBuilder
        '    sbSQL.AppendLine("SELECT TOP 1")
        '    sbSQL.AppendLine("  1")
        '    sbSQL.AppendLine("FROM M_ASSET")
        '    sbSQL.AppendLine("WHERE ASSET_ID = '" & assetId & "'")

        '    'SQLを実行する
        '    Dim selectData As DbRawSqlQuery(Of MstAssetModel) = _db.Database.SqlQuery(Of MstAssetModel)(sbSQL.ToString)
        '    If selectData.Count > 0 Then
        '        '同じ資産IDあり
        '        Return True
        '    End If

        '    Return False
        'End Function
#End Region
        '20261001 Y.Ono ver4.3.1未使用のCSV取込処理をコメントアウト End
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

#Region "資産状態ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 資産状態ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetAssetStatusData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = "",
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SELECT_ALL).ToString
                },
                New DropdownModel With {
                    .Cd = ASSET_STATUS.Storage,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_HOKAN).ToString
                },
                New DropdownModel With {
                    .Cd = ASSET_STATUS.Lending,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_LEASE).ToString
                },
                New DropdownModel With {
                    .Cd = ASSET_STATUS.Removed,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_RETIRE).ToString
                },
                New DropdownModel With {
                    .Cd = ASSET_STATUS.Preparation,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_JUNBI).ToString
                }
            }


            '一覧画面の資産状態コンボ設定
            ViewBag.AssetStatusList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchAssetStatus)

            Return Json(lst)
        End Function
#End Region

#Region "資産ID状態ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 資産ID状態ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetAssetIdStatusData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = "",
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SELECT_ALL).ToString
                },
                New DropdownModel With {
                    .Cd = EpcEncStatus.Sumi,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_SETTING_SUMI).ToString
                },
                New DropdownModel With {
                    .Cd = EpcEncStatus.Mi,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_SETTING_MI).ToString
                }
            }

            '一覧画面の資産ID状態コンボ設定
            ViewBag.EpcEncStatusList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchEpcEncStatus)

            Return Json(lst)
        End Function
#End Region

#Region "タグ紐づけ状態ドロップダウンリストデータ取得"
        ''' <summary>
        ''' タグ紐づけ状態ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetTagLinkStatusData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = "",
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SELECT_ALL).ToString
                },
                New DropdownModel With {
                    .Cd = TagLinkStatus.Sumi,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_SUMI).ToString
                },
                New DropdownModel With {
                    .Cd = TagLinkStatus.Mi,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_MI).ToString
                }
            }

            '一覧画面のタグ紐づけ状態コンボ設定
            ViewBag.TagLinkStatusList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchTagLinkStatus)

            Return Json(lst)
        End Function
#End Region

#Region "〇〇期限　期限切れドロップダウンリストデータ取得"
        ''' <summary>
        ''' 〇〇期限　期限切れドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetGenericLimitExpiredData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = "False",
                    .Name = ""
                },
                New DropdownModel With {
                    .Cd = "True",
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT_EXPIRED).ToString
                }
            }

            '一覧画面の資産状態コンボ設定
            ViewBag.GenericLimitExpiredList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchGenericLimitExpired)

            Return Json(lst)
        End Function
#End Region

#Region "〇〇期限　アラートドロップダウンリストデータ取得"
        ''' <summary>
        ''' 〇〇期限　アラートドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetGenericLimitAlertData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = "False",
                    .Name = ""
                },
                New DropdownModel With {
                    .Cd = "True",
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_ALERT).ToString
                }
            }

            '一覧画面のリース期限　アラートコンボ設定
            ViewBag.GenericLimitAlertList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchGenericLimitAlert)

            Return Json(lst)
        End Function
#End Region

#Region "リース期限　期限切れドロップダウンリストデータ取得"
        ''' <summary>
        ''' リース期限　期限切れドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLeaseLimitExpiredData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = "False",
                    .Name = ""
                },
                New DropdownModel With {
                    .Cd = "True",
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT_EXPIRED).ToString
                }
            }

            '一覧画面のリース期限　期限切れコンボ設定
            ViewBag.LeaseLimitExpiredList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchLeaseLimitExpired)

            Return Json(lst)
        End Function
#End Region

#Region "リース期限　アラートドロップダウンリストデータ取得"
        ''' <summary>
        ''' リース期限　アラートドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLeaseLimitAlertData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = "False",
                    .Name = ""
                },
                New DropdownModel With {
                    .Cd = "True",
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_ALERT).ToString
                }
            }

            '一覧画面のリース期限　アラートコンボ設定
            ViewBag.LeaseLimitAlertList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchLeaseLimitAlert)

            Return Json(lst)
        End Function
#End Region

#Region "返却期限　期限切れドロップダウンリストデータ取得"
        ''' <summary>
        ''' 返却期限　期限切れドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetReturnLimitExpiredData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = "False",
                    .Name = ""
                },
                New DropdownModel With {
                    .Cd = "True",
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT_EXPIRED).ToString
                }
            }

            '一覧画面の資産状態コンボ設定
            ViewBag.HenkyakuLimitExpiredList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchHenkyakuLimitExpired)

            Return Json(lst)
        End Function
#End Region

#Region "貸出ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 貸出ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLending() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = "False",
                    .Name = ""
                },
                New DropdownModel With {
                    .Cd = "True",
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_ALERT).ToString
                }
            }

            '一覧画面の貸出コンボ設定
            ViewBag.LendingList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchLeaseAlert)

            Return Json(lst)
        End Function
#End Region

#Region "表示順ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 表示順ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetDispOrder() As JsonResult

            'メッセージ一覧取得
            Dim Message As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '設定マスタ（薬品管理機能）の取得
            Dim DrugsFunc As SettingInfo = Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
                                                                  CommonSettingKey.Drugs_Function.Key1,
                                                                  CommonSettingKey.Drugs_Function.Key2,
                                                                  CommonSettingKey.Drugs_Function.Key3, True)

            Dim lst As List(Of DropdownModel)
            If DrugsFunc.DATA1 = DrugsFunction.Yes Then

                lst = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = AssetDispSelect.Loca,
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SORT_LOC_ASSET_NO).ToString
                },
                New DropdownModel With {
                    .Cd = AssetDispSelect.Cat,
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SORT_CAT_ASSET_NO).ToString
                },
                New DropdownModel With {
                    .Cd = AssetDispSelect.Asset,
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SORT_ASSET_NO).ToString
                },
                New DropdownModel With {
                    .Cd = AssetDispSelect.Rate,
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SORT_CONSUMPTION_RATE).ToString
                }
            }

            Else
                lst = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = AssetDispSelect.Loca,
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SORT_LOC_ASSET_NO).ToString
                },
                New DropdownModel With {
                    .Cd = AssetDispSelect.Cat,
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SORT_CAT_ASSET_NO).ToString
                },
                New DropdownModel With {
                    .Cd = AssetDispSelect.Asset,
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SORT_ASSET_NO).ToString
                }
            }

            End If

            '一覧画面の表示順コンボ設定
            ViewBag.DispOrderList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchDispSelect)

            Return Json(lst)
        End Function
#End Region


#Region "Excelデータ取込処理（資産台帳取込）"
        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="ExcelFile">登録するExcelファイル</param>
        ''' <returns>View表示内容</returns>
        Public Function ImportExcelFile(ByVal ExcelFile As HttpPostedFileBase, ByVal LocLCd As String) As ActionResult

            Me._lstErr = New List(Of DataInportResult)
            Dim errRow As DataInportResult = New DataInportResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr.Add(errRow)
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.LoginSession
                '結果表示
                Return Json(Me._lstErr)
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim sProcessContents As String          '操作ログ出力
            Dim sTitle As String = Me._Message.getMessage(CommonLabelAssetList.TITLE)
            Dim sAction As String = Me._Message.getMessage(CommonLabel.BUTTON_ASSET_LEDGER_IMPORT)

            '操作ログ出力(取込開始)
            sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.BUTTON_INPORT)
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle, LocLCd)

            '読込むExcelファイルがあるかどうか
            If Not IsImportFileError(ExcelFile) Then
                Dim sErrGaiyou As String = String.Empty
                Select Case Me._lstErr(0).ResultCd
                    Case DataInport.DataInportResultCd.NoSelect
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECT)
                    Case DataInport.DataInportResultCd.Empty
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_EMPTY)
                    Case DataInport.DataInportResultCd.NotExcel
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXCEL)
                End Select

                '操作ログ出力
                sProcessContents = sTitle & " " & sAction & " " & sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)

                Return Json(Me._lstErr)
            End If

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVIMPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            Dim sFilePath As String = Path.Combine(sFullPath, ExcelFile.FileName)
            Dim excelRowCount As Integer = 0
            Dim blRet As Boolean
            Try
                'Excelファイルをアップロード
                ExcelFile.SaveAs(sFilePath)

                'Excelファイル読込＆DB登録処理
                blRet = Dao.ToolAssetExcelImport.Execute(Misc.Auth.FunctionId.AssetList, sFilePath, LocLCd, _LoginInfo, _lstErr, excelRowCount)
            Catch
                InsImportExceptionLog(sTitle, sAction, Path.GetFileName(ExcelFile.FileName))
                Throw
            End Try

            'エラーがあった場合
            If Not blRet Then
                Dim sFileName As String = Path.GetFileName(ExcelFile.FileName)

                If Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportDeleteConfirm Then
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_CONF_ASSET_IMPORT_DELETE).Replace("{0}", Me._lstErr(0).DeleteCount.ToString) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr Then
                    '項目数エラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportSpSystemErr Then
                    'ストアドで予期しないエラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_UNEXPECTED_ERR) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportSpParamErr Then
                    'ストアドで引数エラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_PARAM_ERR) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportSpLocMaterErr Then
                    'ストアドでロケマスタなしエラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_LOC_ERR) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportSpEmployeeErr Then
                    'ストアドで登録担当者なしエラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_EMPLOYEE_ERR) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportNotUpdatableComErr Then
                    'Excel取込エラー（更新可能拠点以外を含む）
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_EXIST_ASSET_COMPANY_NOT_UPDATABLE) & " " & sFileName
                Else
                    '入力エラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " & sFileName & " " &
                                                                      Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                                                      Me._lstErr.Count.ToString
                End If

                '操作ログ出力
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)

                Return Json(Me._lstErr)
            End If

            '操作ログ出力
            sProcessContents = sTitle & " " & sAction & " " &
                                Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                excelRowCount.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)

            Me._lstErr.Add(errRow)
            Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.OK

            Return Json(Me._lstErr)
        End Function

        ''' <summary>
        ''' 資産台帳取込の削除確認で「はい」が選ばれた場合の取込処理（AJax）
        ''' </summary>
        ''' <param name="ImportKey">取込ごとのキー（初回の取込で返したもの）</param>
        ''' <param name="LocLCd">ロケ大コード（画面の検索条件-ロケーション会社）</param>
        ''' <returns>取込結果</returns>
        ''' <remarks>Excelファイルは送られない。初回の取込で入庫予定ワークに保存したデータで取込を行う。</remarks>
        Public Function ImportExcelFileConfirmed(ByVal ImportKey As String, ByVal LocLCd As String) As ActionResult

            Me._lstErr = New List(Of DataInportResult)

            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.LoginSession})
                Return Json(Me._lstErr)
            End If

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim sTitle As String = Me._Message.getMessage(CommonLabelAssetList.TITLE)
            Dim sAction As String = Me._Message.getMessage(CommonLabel.BUTTON_ASSET_LEDGER_IMPORT)

            Dim rowCount As Integer = 0
            Dim sFileName As String = String.Empty
            Dim blRet As Boolean
            Try
                blRet = Dao.ToolAssetExcelImport.ExecuteConfirmed(Misc.Auth.FunctionId.AssetList, ImportKey, LocLCd, _LoginInfo, _lstErr, rowCount, sFileName)
            Catch
                InsImportExceptionLog(sTitle, sAction, sFileName)
                Throw
            End Try

            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, GetImportResultLog(blRet, sTitle, sAction, sFileName, rowCount), _LoginInfo, sTitle)

            If blRet Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.OK})
            End If

            Return Json(Me._lstErr)
        End Function

        ''' <summary>
        ''' 資産台帳取込の削除確認で「いいえ」が選ばれた場合の処理（AJax）
        ''' </summary>
        ''' <param name="ImportKey">取込ごとのキー（初回の取込で返したもの）</param>
        ''' <returns>処理結果</returns>
        ''' <remarks>取込ごとのキーを条件に入庫予定ワークのデータを削除して中断する。</remarks>
        Public Function ImportExcelFileCancel(ByVal ImportKey As String) As ActionResult

            Me._lstErr = New List(Of DataInportResult)

            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.LoginSession})
                Return Json(Me._lstErr)
            End If

            Dao.ToolAssetExcelImport.CancelImport(ImportKey, _LoginInfo)

            Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.OK})
            Return Json(Me._lstErr)
        End Function

        ''' <summary>
        ''' 資産台帳取込の結果から、操作ログに出力する内容を作成する（「はい」の取込用）
        ''' </summary>
        ''' <param name="blRet">取込結果</param>
        ''' <param name="sTitle">画面タイトル</param>
        ''' <param name="sAction">ボタン名</param>
        ''' <param name="sFileName">取込ファイル名</param>
        ''' <param name="rowCount">取込件数</param>
        ''' <returns>操作ログの内容</returns>
        ''' <remarks>出力内容は ImportExcelFile と同じ。ワークにデータが無い場合は取込失敗（ImportExcelErr）。</remarks>
        Private Function GetImportResultLog(ByVal blRet As Boolean,
                                            ByVal sTitle As String,
                                            ByVal sAction As String,
                                            ByVal sFileName As String,
                                            ByVal rowCount As Integer) As String
            If blRet Then
                Return sTitle & " " & sAction & " " &
                       Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                       Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                       rowCount.ToString
            End If

            Select Case Me._lstErr(0).ResultCd
                Case DataInport.DataInportResultCd.ImportExcelErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_ERR) & " " & sFileName
                Case DataInport.DataInportResultCd.ImportSpSystemErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_UNEXPECTED_ERR) & " " & sFileName
                Case DataInport.DataInportResultCd.ImportSpParamErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_PARAM_ERR) & " " & sFileName
                Case DataInport.DataInportResultCd.ImportSpLocMaterErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_LOC_ERR) & " " & sFileName
                Case DataInport.DataInportResultCd.ImportSpEmployeeErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_EMPLOYEE_ERR) & " " & sFileName
                Case DataInport.DataInportResultCd.ImportNotUpdatableComErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_EXIST_ASSET_COMPANY_NOT_UPDATABLE) & " " & sFileName
                Case Else
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " & sFileName & " " &
                           Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                           Me._lstErr.Count.ToString
            End Select
        End Function

        ''' <summary>
        ''' 資産台帳取込で例外が発生した場合の操作ログを出力する
        ''' </summary>
        ''' <param name="sTitle">画面タイトル</param>
        ''' <param name="sAction">ボタン名</param>
        ''' <param name="sFileName">取込ファイル名</param>
        ''' <remarks>操作ログの出力に失敗しても、呼び出し元の例外を優先するため例外は投げない。</remarks>
        Private Sub InsImportExceptionLog(ByVal sTitle As String,
                                          ByVal sAction As String,
                                          ByVal sFileName As String)
            Try
                Dim sProcessContents As String = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_SYSTEM) & " " & sFileName
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)
            Catch ex As Exception
                logger.Error(ex.Message)
            End Try
        End Sub

#End Region

#Region "EXCELファイルのエラーチェック"
        ''' <summary>
        ''' EXCELファイルのエラーチェック
        ''' </summary>
        ''' <param name="file">チェック対象のファイル</param>
        ''' <returns>True:エラー無し　False:エラー有り</returns>
        Public Function IsImportFileError(file As HttpPostedFileBase) As Boolean
            Dim blRet As Boolean = False

            'ファイルがあるかどうか
            If IsNothing(file) Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.NoSelect})
                Return blRet
            End If

            'ファイルサイズ
            If file.ContentLength = 0 Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.Empty})
                Return blRet
            End If

            '拡張子を検証
            Dim extention As String = Path.GetExtension(file.FileName).Replace(".", "")
            If String.IsNullOrEmpty(extention) OrElse Not {"xls", "xlsx"}.Contains(extention.ToLower()) Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.NotExcel})
                Return blRet
            End If

            blRet = True
            Return blRet
        End Function
#End Region

    End Class
End Namespace