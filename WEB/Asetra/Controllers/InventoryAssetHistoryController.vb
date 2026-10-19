Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers
    ''' <summary>
    ''' 棚卸実績照会画面表示用のクラス
    ''' </summary>
    Public Class InventoryAssetHistoryController
        Inherits PageBaseController

#Region "クラス変数"
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        'Private _post As Boolean = False
        Private _Model As InventoryInquiryModel = New InventoryInquiryModel                                     'InventoryInquiryモデル
        Private _ViewModel As InventoryAssetHistoryViewModel = New InventoryAssetHistoryViewModel                         'InventoryInquiry一覧表示用モデル
        Protected _viewResult As List(Of InventoryAssetHistoryViewModel) = New List(Of InventoryAssetHistoryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _isSort As Boolean = True                       'ソートフラグ

        Private _LocationLList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationMList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationSList As List(Of LocationDropdownModel)     'ロケーションマスタリスト

        Private _viewInventory As List(Of IASInventoryViewModel)
        Private _startDecTs As String = ""
        '20231018 ADD Start Shirakawa
        Private _invId As String = ""
        Private _invLocLCds As String = ""
        Private _invLocLCdNames As String = ""
        Private _invLocLCd() As String = Nothing
        '20231018 ADD End Shirakawa
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
        Public Function Init(ByVal ViewModel As InventoryAssetHistoryViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(CommonSessionKey.SEARCH_FLG) IsNot Nothing Then
                Session.Remove(CommonSessionKey.SEARCH_FLG)
            End If

            If Session(CommonSessionKey.INV_RETURN_LOC) IsNot Nothing Then
                Session.Remove(CommonSessionKey.INV_RETURN_LOC)
            End If

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
        Public Function Index(ByVal ViewModel As InventoryAssetHistoryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_INVENTORY_INQUIRY)
            End If

            '上限確認フラグ 初期化
            ViewBag.isLimitFlg = False
            '一覧件数　初期化
            ViewBag.cntList = 0
            '設定マスタ上限件数　初期化
            ViewBag.cntLimit = 0
            '一覧表示フラグ
            Dim isDisp As Boolean = True

            '20231018 UPD Start Shirakawa
            '--2019/10/15 nv miyaji upd start
            If String.IsNullOrEmpty(Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY)) Then
                Return RedirectToAction("Index", "InventoryHistory")
            End If
            _startDecTs = Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY).Split("/")(0)

            '当該棚卸ID
            _invId = Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY).Split("/")(1)
            '当該ロケ大コード
            _invLocLCds = Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY).Split("/")(2)
            '当該ロケ大コードとロケ名
            _invLocLCdNames = Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY).Split("/")(3)

            '棚卸開始時間、当該棚卸ID、当該ロケ大コード、当該ロケ大コードとロケ名どれかがNULLの場合、元の画面に戻る
            If String.IsNullOrEmpty(_startDecTs) OrElse String.IsNullOrEmpty(_invId) OrElse String.IsNullOrEmpty(_invLocLCds) Then
                Return RedirectToAction("Index", "InventoryHistory")
            End If

            'ロケコードを分離させ配列に格納
            _invLocLCd = _invLocLCds.Split("：")

            '20231018 UPD End Shirakawa
            '' クエリパラメータを取得する
            'If Request.QueryString("startDecTs") IsNot Nothing Then
            '    _startDecTs = Request.QueryString("startDecTs").ToString()
            'End If
            '--2019/10/15 nv miyaji upd end

            '20230901 ADD Start Shirakawa
            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid                      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Inventory_Results_Detail.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Results_Detail.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Results_Detail.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Results_Detail.Key3)
            '20230901 ADD End Shirakawa

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As InventoryAssetHistoryViewModel = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'viewModelのロケ検索条件に含める
            _ViewModel.SearchLocL = _invLocLCds

            'セッションに大ロケをいれる
            Session(CommonSessionKey.INV_RETURN_LOC) = _invLocLCds

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '--2019/10/15 nv miyaji コメントアウト
            'If String.IsNullOrEmpty(_startDecTs) AndAlso _ViewModel.SelectStartDecTs <> "" Then
            '    _startDecTs = _ViewModel.SelectStartDecTs
            'End If

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
                ViewBag.modelLocL = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                ViewBag.modelLocM = New MultiSelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME")
                ViewBag.modelLocS = New MultiSelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME")
                ViewBag.modelCatL = New MultiSelectList(New List(Of CategoryDropdownModel), "CAT_CD", "CAT_NAME")
                ViewBag.FreeItem10List = New SelectList(New List(Of DropdownModel), "Cd", "Name", "")
                ViewBag.TagLinkStatusList = New SelectList(New List(Of DropdownModel), "Cd", "Name", "")
                '[CC431] ADD Start 20261009
                ViewBag.PhyinvToolDivList = New SelectList(New List(Of DropdownModel), "Cd", "Name", "")
                '[CC431] ADD End 20261009
                CopyModelData()
                Return View(_viewResult)
            Else
                _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryAssetHistory, _LoginInfo)

                '20230904 UPD Start Shirakawa
                '件数確認用データのためソートしない
                _isSort = False
                'セッションSEARCH_FLGがNothingの場合
                If Session(CommonSessionKey.SEARCH_FLG) Is Nothing Then
                    '初期表示
                    '設定マスタから取得できた場合 かつ 初期表示フラグ(Data1)がある場合 かつ 設定マスタ(web読取データ)の初期表示Nの場合
                    If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA1) = False AndAlso WebReadData.DATA1 = RedeDataFlg.No Then
                        '表示しない
                        isDisp = False
                    Else
                        '表示する
                        MyBase.BaseIndex(_ViewModel, False)
                    End If
                Else
                    '検索ボタンから表示
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

            '20230901 ADD End Shirakawa

            '設定情報(実棚ロケ使用区分)取得     2020/03/17 ADD nv.miyaji
            Dim modelLocationUsage As New LocationUsageModel       'ロケーション使用区分
            Dim setting As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.ActualShelfLocation.KeyDiv, CommonSettingKey.ActualShelfLocation.Key1, CommonSettingKey.ActualShelfLocation.Key2, CommonSettingKey.ActualShelfLocation.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingInfo.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            ViewBag.LocUsage_L = setting.DATA1
            ViewBag.LocUsage_M = setting.DATA2
            ViewBag.LocUsage_S = setting.DATA3

            ' (ロケーション)大区分ドロップダウンリスト作成
            GetLocationLData()
            '[CC431] UPD Start 20261007
            ' (ロケーション)中区分・小区分（当該棚卸の過去棚卸実績Tに存在するもの。場所から選ぶの ON／OFF で連動）
            Dim invIdForLoc As Long
            Long.TryParse(_invId, invIdForLoc)
            SetSearchLocationOptions(invIdForLoc, _ViewModel.SearchLocL, _AuthInfo)
            '[CC431] UPD End 20261007

            ' (カテゴリー)大区分ドロップダウンリスト作成
            GetCategoryLData()

            'フリー項目10（貸与資産状態区分）ドロップダウンリスト作成
            GetFreeItem10Data()

            'タグ紐づけ状態ドロップダウンリストデータ作成
            GetTagLinkStatusData()

            '[CC431] ADD Start 20261009
            ' 棚卸実施端末（実棚処理ツール区分。C:CS は使用しない）
            '[CC431] UPD Start 20261015
            ' 仕様変更3：棚卸実施端末の選択肢に「-」（未設定）を追加（空の選択肢、-、HT、Web の順）
            Dim phyinvToolDivList As New List(Of DropdownModel) From {
                New DropdownModel With {.Cd = ProcessToolDiv.NoValue, .Name = "-"},
                New DropdownModel With {.Cd = ProcessToolDiv.HT, .Name = _Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)},
                New DropdownModel With {.Cd = ProcessToolDiv.Web, .Name = _Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)}
            }
            '[CC431] UPD End 20261015
            ViewBag.PhyinvToolDivList = New SelectList(phyinvToolDivList, "Cd", "Name")
            '[CC431] ADD End 20261009

            'If Request.HttpMethod = "POST" Or _startDecTs <> "" Then
            '_post = True
            'End If

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.INVENTORY_ASSET_HISTORY, COLSET_LOC.TEMPLATE)
            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If

            '一覧情報にページ情報を設定
            CopyModelData()

            ' 棚卸管理テーブルを取得
            GetInventry()

            '結果表示
            Return View(_viewResult)

        End Function

        ''' <summary>
        ''' ロケーション変更
        ''' </summary>
        ''' <param name="locationLevel"></param>
        ''' <param name="selectLCode"></param>
        ''' <param name="selectMCode"></param>
        ''' <returns></returns>
        Public Function ChangeLocation(ByVal locationLevel As String, ByVal selectLCode As String, ByVal selectMCode As String) As ActionResult

            '2020/03/13 実棚ロケ使用区分　の判断を追加 ADD Inoue
            'If locationLevel = "M" Then
            '    Return Json(GetLocationMData(selectLCode))
            'Else
            '    Return Json(GetLocationSData(selectLCode, selectMCode))
            'End If

            ''結果表示
            'Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

            Dim settingRfid As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)               'メッセージ兼設定リスト
            Dim modelLocationUsage As New LocationUsageModel        'ロケーション使用区分

            '棚卸処理では、ActualShelfLocation　を使用する
            Dim setting As SettingInfo = settingRfid.getSettingInfo(CommonSettingKey.ActualShelfLocation.KeyDiv, CommonSettingKey.ActualShelfLocation.Key1, CommonSettingKey.ActualShelfLocation.Key2, CommonSettingKey.ActualShelfLocation.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingRfid.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.ActualShelfLocation.KeyDiv))
            End If
            modelLocationUsage.LocUsage_L = setting.DATA1
            modelLocationUsage.LocUsage_M = setting.DATA2
            modelLocationUsage.LocUsage_S = setting.DATA3

            If locationLevel = LOC_DIV.LocationM Then
                If modelLocationUsage.LocUsage_M = Misc.ItemUse.Yes Then
                    Return Json(GetLocationMData(selectLCode))
                Else
                    '中区分 使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            Else

                If modelLocationUsage.LocUsage_S = Misc.ItemUse.Yes Then
                    '中区分は複数選択のため「,」区切りで受け取る
                    Return Json(GetLocationSData(selectLCode, CommonRfid.GetSelectedValues(CommonRfid.ToStringNvl(selectMCode).Split(","c))))
                Else
                    '小区分　使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

        '[CC431] ADD Start 20261007
        ''' <summary>
        ''' 検索条件のロケーション（原価センタ・場所）の選択肢を返す（画面の ajax から呼ぶ）
        ''' </summary>
        ''' <param name="level">返す側（M：中区分、S：小区分）</param>
        ''' <param name="reverse">場所から選ぶ（True：場所で原価センタを絞り込む、False：原価センタで場所を絞り込む）</param>
        ''' <param name="codes">絞り込みに使う、反対側の選択中のコード（「,」区切り）</param>
        ''' <param name="selectLCode">会社（画面から送られるが、この画面では当該棚卸の大区分を使うため使用しない）</param>
        ''' <returns>選択肢（LOC_CD／LOC_NAME のリスト）</returns>
        Public Function GetSearchLocationOptions(ByVal level As String, ByVal reverse As Boolean, ByVal codes As String, Optional ByVal selectLCode As String = Nothing) As ActionResult
            'ログインセッション・当該棚卸の情報が切れていたら、元の画面に戻る
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            Dim key As String = Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY)
            If IsNothing(_LoginInfo) OrElse String.IsNullOrEmpty(key) Then
                Return RedirectToAction("Index", "InventoryHistory")
            End If

            '当該棚卸ID・当該ロケ大コード
            Dim keys As String() = key.Split("/")
            Dim invId As Long
            Long.TryParse(keys(1), invId)
            Dim invLocL As String = keys(2)

            '棚卸処理では、ActualShelfLocation　を使用する
            Dim settingRfid As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)               'メッセージ兼設定リスト
            Dim setting As SettingInfo = settingRfid.getSettingInfo(CommonSettingKey.ActualShelfLocation.KeyDiv, CommonSettingKey.ActualShelfLocation.Key1, CommonSettingKey.ActualShelfLocation.Key2, CommonSettingKey.ActualShelfLocation.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingRfid.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.ActualShelfLocation.KeyDiv))
            End If
            '中区分・小区分を使用しない場合、Nothing を返す（ChangeLocation と同じ）
            If (level = LOC_DIV.LocationM AndAlso setting.DATA2 <> Misc.ItemUse.Yes) OrElse
               (level = LOC_DIV.LocationS AndAlso setting.DATA3 <> Misc.ItemUse.Yes) Then
                Return Nothing
            End If

            '権限はサーバー側で取得する
            Me._Message = settingRfid
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryAssetHistory, _LoginInfo)

            Return Json(GetSearchLocationList(level, reverse, CommonRfid.ToStringNvl(codes).Split(","c), invId, invLocL, authInfo))

        End Function
        '[CC431] ADD End 20261007

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()

            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_INVENTORY_INQUIRY)
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

            'If Not (_post) Then
            'Return
            'End If

            'SQLを実行
            Dim query As DbSqlQuery(Of InventoryAssetHistoryViewModel) = _db.InventoryAssetHistoryView.SqlQuery(_SQL, _paramSqlList.ToArray)

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

#Region "(ロケーション)大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)大区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationLData() As JsonResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationLDataInvAseetHistory(_invId)
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            'If _invLocLCd.Count > 1 Then
            '    lstLocation.Add(
            '        New LocationDropdownModel With {
            '            .LOC_CD = _invLocLCds,
            '            .LOC_NAME = _invLocLCdNames.ToString
            '        })
            'End If
            ViewBag.modelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", lstLocation(0).LOC_CD)
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

            '検索条件のロケーションコンボ設定（複数選択のため MultiSelectList）
            ViewBag.modelLocM = New MultiSelectList(lstLocation, "LOC_CD", "LOC_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchLocM))
            _ViewModel.locMDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "(ロケーション)小区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCodes As String()) As JsonResult
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

            '検索条件のロケーションコンボ設定（複数選択のため MultiSelectList）
            ViewBag.modelLocS = New MultiSelectList(lstLocation, "LOC_CD", "LOC_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchLocS))
            _ViewModel.locSDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

        '[CC431] ADD Start 20261007
#Region "検索条件 ロケーション（原価センタ・場所）の選択肢"
        ''' <summary>
        ''' 検索条件の原価センタ・場所のコンボの選択肢を、場所から選ぶの ON／OFF と選択値に合わせて作る
        ''' </summary>
        ''' <param name="invId">棚卸ID</param>
        ''' <param name="invLocL">ロケ大区分コード</param>
        ''' <param name="authInfo">権限情報</param>
        Private Sub SetSearchLocationOptions(invId As Long, invLocL As String, authInfo As AuthInfo)
            Dim selLocM As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocM)
            Dim selLocS As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocS)

            Dim lstLocM As List(Of LocationDropdownModel) = GetSearchLocationList(LOC_DIV.LocationM, _ViewModel.SearchLocReverse, selLocS, invId, invLocL, authInfo)
            Dim lstLocS As List(Of LocationDropdownModel) = GetSearchLocationList(LOC_DIV.LocationS, _ViewModel.SearchLocReverse, selLocM, invId, invLocL, authInfo)

            ViewBag.modelLocM = New MultiSelectList(lstLocM, "LOC_CD", "LOC_NAME", selLocM)
            _ViewModel.locMDropdownData = lstLocM
            ViewBag.modelLocS = New MultiSelectList(lstLocS, "LOC_CD", "LOC_NAME", selLocS)
            _ViewModel.locSDropdownData = lstLocS
        End Sub

        ''' <summary>
        ''' 検索条件のロケーションの選択肢を取得する（権限あり かつ 当該棚卸の過去棚卸実績Tに存在するもの）
        ''' 場所から選ぶ OFF：中区分は絞り込みなし、小区分は選択中の中区分で絞り込む
        ''' 場所から選ぶ ON ：小区分は絞り込みなし、中区分は選択中の小区分で絞り込む
        ''' 絞り込まれる側で、絞り込む側が未選択の場合は空
        ''' </summary>
        ''' <param name="level">取得する側（M：中区分、S：小区分）</param>
        ''' <param name="reverse">場所から選ぶ</param>
        ''' <param name="filterCodes">反対側の選択中のコード</param>
        ''' <param name="invId">棚卸ID</param>
        ''' <param name="invLocL">ロケ大区分コード</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <returns>選択肢</returns>
        Private Function GetSearchLocationList(level As String, reverse As Boolean, filterCodes As String(),
                                               invId As Long, invLocL As String, authInfo As AuthInfo) As List(Of LocationDropdownModel)
            Dim lstLocation As New List(Of LocationDropdownModel)

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

            Dim exTarget As Dao.ToolLocation.LocExistTarget = Dao.ToolLocation.LocExistTarget.InvResult(invId)
            Dim tbl As DataTable
            If level = LOC_DIV.LocationM Then
                tbl = Dao.ToolLocation.GetLocationMDataExists(invLocL, _LoginInfo, authInfo, exTarget, codes)
            Else
                tbl = Dao.ToolLocation.GetLocationSDataExists(invLocL, _LoginInfo, authInfo, exTarget, codes)
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

#Region "(カテゴリー)大分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (カテゴリー)大分類ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCategoryLData() As JsonResult
            Dim tbl As DataTable = Dao.ToolCategory.GetCategoryLData()
            Dim lstCategory As New List(Of CategoryDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstCategory.Add(
                    New CategoryDropdownModel With {
                        .CAT_CD = row("CAT_CD").ToString,
                        .CAT_NAME = row("CAT_NAME").ToString
                    }
                )
            Next

            '検索条件のカテゴリーコンボ設定（複数選択のため MultiSelectList）
            'ViewBag.modelCatL = New SelectList(lstCategory, "CAT_CD", "CAT_NAME", _invLocLCd) '初期表示時に選択したテキスト表示 20231012 ADD Shirakawa
            ViewBag.modelCatL = New MultiSelectList(lstCategory, "CAT_CD", "CAT_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchCatL))
            _ViewModel.catLDropdownData = lstCategory

            Return Json(lstCategory)
        End Function
#End Region

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

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ByVal ViewModel As InventoryAssetHistoryViewModel) As ActionResult
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

            '20231031 ADD Start Shirakawa
            '当該棚卸ID
            _invId = Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY).Split("/")(1)
            '当該ロケ大コード
            _invLocLCds = Session(CommonSessionKey.CREAGE_INVENTORY_DETAIL_KEY).Split("/")(2)

            'ロケコードを分離させ配列に格納
            _invLocLCd = _invLocLCds.Split("：")

            '20231031 ADD End Shirakawa


            '20230901 ADD Start Shirakawa
            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid                      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Inventory_Results_Detail.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Results_Detail.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Results_Detail.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Results_Detail.Key3)
            '20230901 ADD End Shirakawa

            _ViewModel = ViewModel
            '[CC431] ADD Start 20261009
            ' 画面の会社（非活性のため送信されない）の代わりに、当該棚卸の会社を検索条件に含める（Index と同じ）
            _ViewModel.SearchLocL = _invLocLCds
            '[CC431] ADD End 20261009
            _startDecTs = _ViewModel.SelectStartDecTs

            Dim InventoryAssetHistoryData As List(Of InventoryAssetHistoryViewModel)

            '実棚照会取得
            InventoryAssetHistoryData = GetInventoryAssetHistoryData()

            '20230901 ADD Start Shirakawa
            '設定マスタから取得できた場合
            '設定マスタから取得できた場合 かつ 上限値(Data2)が存在している場合 かつ 上限を超えていた場合
            If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA2) = False AndAlso WebReadData.DATA2 < InventoryAssetHistoryData.Count Then
                '上限を超えていた場合　CSV出力しない

                '上限確認フラグ
                ViewBag.isLimitFlg = True
                '一覧データ取得件数
                ViewBag.cntList = InventoryAssetHistoryData.Count
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
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.InventoryAssetHistory.KeyDiv, CommonSettingKey.InventoryAssetHistory.Key1, CommonSettingKey.InventoryAssetHistory.Key2, CommonSettingKey.InventoryAssetHistory.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_INVENTORY_ASSET_HISTORY)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_INVENTORY_ASSET_HISTORY)
            Dim singleQuotation As String = "'"

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As InventoryAssetHistoryViewModel In InventoryAssetHistoryData
                col = ""

                If row.ASSET_NO Is Nothing Then
                    col += SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.ASSET_NO.Trim)
                End If

                If row.SUB_ASSET_NO Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.SUB_ASSET_NO.Trim)
                End If

                If row.ASSET_STATUS_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_STATUS_NAME.Trim)
                End If

                If row.PHYINV_STATUS_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_STATUS_NAME.Trim)
                End If

                If row.LOC_L_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_L_CD.Trim)
                End If

                If row.LOC_L_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_NAME.Trim)
                End If

                If row.LOC_M_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_M_CD.Trim)
                End If

                If row.LOC_M_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_NAME.Trim)
                End If

                If row.LOC_S_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_S_CD.Trim)
                End If

                If row.LOC_S_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_NAME.Trim)
                End If

                If row.PHYINV_LOC_L_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.PHYINV_LOC_L_CD.Trim)
                End If

                If row.PHYINV_LOC_L_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_LOC_L_NAME.Trim)
                End If

                If row.PHYINV_LOC_M_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.PHYINV_LOC_M_CD.Trim)
                End If

                If row.PHYINV_LOC_M_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_LOC_M_NAME.Trim)
                End If

                If row.PHYINV_LOC_S_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.PHYINV_LOC_S_CD.Trim)
                End If

                If row.PHYINV_LOC_S_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_LOC_S_NAME.Trim)
                End If

                If row.CAT_L_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CAT_L_CD.Trim)
                End If

                If row.CAT_L_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_NAME.Trim)
                End If

                If row.IS_TANGIBLE Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    Dim isTangible As String = ""
                    Select Case row.IS_TANGIBLE.Trim
                        Case TANGIBLE.TANGIBLE
                            isTangible = Me._Message.getMessage(CommonLabel.LABEL_TANGIBLE_ASSET_NAME)
                        Case TANGIBLE.NOT_TANGIBLE
                            isTangible = Me._Message.getMessage(CommonLabel.LABEL_INTANGIBLE_ASSET_NAME)
                    End Select

                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(isTangible)
                End If

                If row.ASSET_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME.Trim)
                End If

                '資産製造元
                If row.MAKER Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER.Trim)
                End If
                '品番
                If row.PRODUCT_NO Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PRODUCT_NO.Trim)
                End If
                '型式
                If row.MODEL_NO Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO.Trim)
                End If

                If row.PHYINV_DATE Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_DATE)
                End If

                If row.PHYINV_EMPLOYEE_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.PHYINV_EMPLOYEE_CD.Trim)
                End If

                If row.PHYINV_EMPLOYEE_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_EMPLOYEE_NAME.Trim)
                End If

                '[CC431] ADD Start 20261009
                '棚卸実施端末（H:HT、W:Web）。想定外の値はそのまま出力
                Select Case row.PHYINV_TOOL_DIV
                    Case ProcessToolDiv.HT
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(Me._Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT).ToString())
                    Case ProcessToolDiv.Web
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(Me._Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB).ToString())
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(If(row.PHYINV_TOOL_DIV Is Nothing, String.Empty, row.PHYINV_TOOL_DIV.Trim))
                End Select
                '[CC431] ADD End 20261009

                If row.FREE_ITEM6 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.FREE_ITEM6.Trim)
                End If

                If row.FREE_ITEM6_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6_NAME.Trim)
                End If

                If row.FREE_ITEM1 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM1.Trim)
                End If

                If row.FREE_ITEM2 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM2.Trim)
                End If

                If row.FREE_ITEM3 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.FREE_ITEM3.Trim)
                End If

                If row.FREE_ITEM4 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM4.Trim)
                End If

                If row.FREE_ITEM5 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM5.Trim)
                End If

                If row.FREE_ITEM7 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM7.Trim)
                End If

                If row.FREE_ITEM8 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM8.Trim)
                End If

                If row.FREE_ITEM9 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM9.Trim)
                End If

                If row.FREE_ITEM10 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM10.Trim)
                End If

                '[CC431] UPD Start 20261009
                If row.FREE_ITEM11 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(FormatCommaNumber(row.FREE_ITEM11.Trim))
                End If
                '[CC431] UPD End 20261009

                '[CC431] UPD Start 20261009
                If row.FREE_ITEM12 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(FormatCommaNumber(row.FREE_ITEM12.Trim))
                End If
                '[CC431] UPD End 20261009

                If row.FREE_ITEM13 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM13.Trim)
                End If

                If row.FREE_ITEM14 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM14.Trim)
                End If

                If row.FREE_ITEM15 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM15.Trim)
                End If

                If row.FREE_ITEM16 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM16.Trim)
                End If

                If row.FREE_ITEM17 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM17.Trim)
                End If

                If row.FREE_ITEM18 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM18.Trim)
                End If

                If row.FREE_ITEM19 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM19.Trim)
                End If

                If row.FREE_ITEM20 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM20.Trim)
                End If

                If row.FREE_ITEM21 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM21.Trim)
                End If

                If row.FREE_ITEM22 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM22.Trim)
                End If

                If row.FREE_ITEM23 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM23.Trim)
                End If

                If row.FREE_ITEM24 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM24.Trim)
                End If

                If row.FREE_ITEM25 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM25.Trim)
                End If

                If row.FREE_ITEM26 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM26.Trim)
                End If

                If row.FREE_ITEM27 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM27.Trim)
                End If

                If row.FREE_ITEM28 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM28.Trim)
                End If

                If row.FREE_ITEM29 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM29.Trim)
                End If

                If row.FREE_ITEM30 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM30.Trim)
                End If

                If row.FREE_ITEM31 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM31.Trim)
                End If

                If row.FREE_ITEM32 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM32.Trim)
                End If

                If row.FREE_ITEM33 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM33.Trim)
                End If

                If row.FREE_ITEM34 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM34.Trim)
                End If

                If row.FREE_ITEM35 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM35.Trim)
                End If

                If row.FREE_ITEM36 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM36.Trim)
                End If

                If row.FREE_ITEM37 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM37.Trim)
                End If

                If row.FREE_ITEM38 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM38.Trim)
                End If

                If row.FREE_ITEM39 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM39.Trim)
                End If

                If row.FREE_ITEM40 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM40.Trim)
                End If

                If row.FREE_ITEM41 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM41.Trim)
                End If

                If row.FREE_ITEM42 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM42.Trim)
                End If

                If row.FREE_ITEM43 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM43.Trim)
                End If

                If row.FREE_ITEM44 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM44.Trim)
                End If

                If row.FREE_ITEM45 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM45.Trim)
                End If

                If row.FREE_ITEM46 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM46.Trim)
                End If

                If row.FREE_ITEM47 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM47.Trim)
                End If

                If row.FREE_ITEM48 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM48.Trim)
                End If

                If row.FREE_ITEM49 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM49.Trim)
                End If

                If row.FREE_ITEM50 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM50.Trim)
                End If

                If row.FREE_ITEM51 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM51.Trim)
                End If

                If row.FREE_ITEM52 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM52.Trim)
                End If

                If row.FREE_ITEM53 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM53.Trim)
                End If

                If row.FREE_ITEM54 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM54.Trim)
                End If

                If row.FREE_ITEM55 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM55.Trim)
                End If

                If row.FREE_ITEM56 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM56.Trim)
                End If

                If row.FREE_ITEM57 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM57.Trim)
                End If

                If row.FREE_ITEM58 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM58.Trim)
                End If

                If row.FREE_ITEM59 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM59.Trim)
                End If

                If row.FREE_ITEM60 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM60.Trim)
                End If

                Select Case row.TAG_LINK_STATUS
                    Case Byte.Parse(TagLinkStatus.Mi)
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(Me._Message.getMessage(CommonLabel.VALUE_MI).ToString())
                    Case Byte.Parse(TagLinkStatus.Sumi)
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(Me._Message.getMessage(CommonLabel.VALUE_SUMI).ToString())
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select

                '[CC431] ADD Start 20261009
                If row.ENT_EMPLOYEE_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.ENT_EMPLOYEE_CD.Trim)
                End If

                If row.ENT_EMPLOYEE_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ENT_EMPLOYEE_NAME.Trim)
                End If
                '[CC431] ADD End 20261009

                If row.OPE_STATUS Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.OPE_STATUS.Trim)
                End If

                If row.OPE_STATUS_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.OPE_STATUS_NAME.Trim)
                End If

                If row.DIVERSION_REMOVE Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.DIVERSION_REMOVE.Trim)
                End If

                If row.REMOVE_DATE Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.REMOVE_DATE.Trim)
                End If

                If row.REMOVE_REASON Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.REMOVE_REASON.Trim)
                End If

                If row.REMOVE_REASON_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMOVE_REASON_NAME.Trim)
                End If

                If row.REMOVE_CONTENT Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMOVE_CONTENT.Trim)
                End If

                If row.CHANGE_LOC_M_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CHANGE_LOC_M_CD.Trim)
                End If

                If row.CHANGE_LOC_M_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHANGE_LOC_M_NAME.Trim)
                End If

                If row.CHANGE_LOC_S_CD Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CHANGE_LOC_S_CD.Trim)
                End If

                If row.CHANGE_LOC_S_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHANGE_LOC_S_NAME.Trim)
                End If

                If row.CHANGE_EMPLOYEE Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CHANGE_EMPLOYEE.Trim)
                End If

                If row.CHANGE_EMPLOYEE_NAME Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHANGE_EMPLOYEE_NAME.Trim)
                End If

                If row.REMARK1 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK1.Trim)
                End If

                If row.REMARK2 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK2.Trim)
                End If

                If row.REMARK3 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK3.Trim)
                End If

                If row.REMARK4 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK4.Trim)
                End If

                If row.REMARK5 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK5.Trim)
                End If

                If row.REMARK6 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK6.Trim)
                End If

                If row.REMARK7 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK7.Trim)
                End If

                If row.REMARK8 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK8.Trim)
                End If

                If row.REMARK9 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK9.Trim)
                End If

                If row.REMARK10 Is Nothing Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK10.Trim)
                End If


                col += ""
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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryAssetHistory.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     InventoryAssetHistoryData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryAssetHistory.LABEL_TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function

        '[CC431] ADD Start 20261009
        ''' <summary>
        ''' 数値をカンマ区切りの文字列にします。数値でない値、空はそのまま返します（小数の桁は元の値のまま）
        ''' </summary>
        ''' <param name="value">変換する値</param>
        Private Function FormatCommaNumber(ByVal value As String) As String
            Dim num As Decimal
            If Not Decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, num) Then
                Return value
            End If
            Dim decimals As Integer = 0
            Dim pos As Integer = value.IndexOf("."c)
            If pos >= 0 Then decimals = value.Length - pos - 1
            Return num.ToString("N" & decimals, System.Globalization.CultureInfo.InvariantCulture)
        End Function
        '[CC431] ADD End 20261009
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetInventoryAssetHistoryData() As List(Of InventoryAssetHistoryViewModel)
            Dim lstInventoryAssetHistory As List(Of InventoryAssetHistoryViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            'lstInventoryAssetHistory = db.InventoryAssetHistoryView.SqlQuery(CreateSQL()).ToList

            Dim query As DbSqlQuery(Of InventoryAssetHistoryViewModel) = db.InventoryAssetHistoryView.SqlQuery(CreateSQL(), _paramSqlList.ToArray)
            lstInventoryAssetHistory = query.ToList

            'データを返却
            Return lstInventoryAssetHistory
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

#Region "棚卸管理テーブル取得"
        ''' <summary>
        ''' 棚卸管理テーブル取得
        ''' </summary>
        Private Sub GetInventry()
            'Dim db As SystemDBContext = New SystemDBContext
            'Dim sbSQL As StringBuilder = New StringBuilder

            'sbSQL.Clear()
            'sbSQL.AppendLine("SELECT")
            'sbSQL.AppendLine("START_DEC_TS IAS_START_DEC_TS")
            'sbSQL.AppendLine(",LOGIC_INV_NUM IAS_LOGIC_INV_NUM")
            'sbSQL.AppendLine(",PHYINV_NUM IAS_PHYINV_NUM")
            'sbSQL.AppendLine(",LOC_DIFF_NUM IAS_LOC_DIFF_NUM")
            'sbSQL.AppendLine(",NO_ITEM_NUM IAS_NO_ITEM_NUM")
            'sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount ")
            'sbSQL.AppendLine("FROM T_INVENTORY")
            'sbSQL.AppendLine("WHERE")
            'sbSQL.AppendLine("INV_CONFIRM_TS IS NULL")

            '_viewInventory = db.IASInventoryView.SqlQuery(sbSQL.ToString).ToList

            'ViewBag.LogicInvNum = 0
            'ViewBag.PhyinvNum = 0
            'ViewBag.LocDiffNum = 0
            'ViewBag.NoItemNum = 0
            'If _viewInventory.Count > 0 Then
            '    ViewBag.LogicInvNum = _viewInventory(0).IAS_LOGIC_INV_NUM
            '    ViewBag.PhyinvNum = _viewInventory(0).IAS_PHYINV_NUM
            '    ViewBag.LocDiffNum = _viewInventory(0).IAS_LOC_DIFF_NUM
            '    ViewBag.NoItemNum = _viewInventory(0).IAS_NO_ITEM_NUM
            'End If

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)

            sbSQL.Clear()
            params = New List(Of SqlParameter)

            sbSQL.AppendLine("SELECT INV.START_DEC_TS AS IAS_START_DEC_TS")

            '帳簿在庫数 SUM(保管、貸出)
            sbSQL.AppendLine("       ,COUNT_BIG(")
            sbSQL.AppendLine("           CASE WHEN MAST.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ") THEN MAST.ASSET_NO")
            sbSQL.AppendLine("           ELSE NULL END")
            sbSQL.AppendLine("        ) AS IAS_LOGIC_INV_NUM")

            '実棚数 SUM(正常、警告)
            sbSQL.AppendLine("       ,COUNT_BIG(")
            sbSQL.AppendLine("           CASE WHEN MAST.PHYINV_STATUS IN ('" & PHYINV_STATUS.Normal & "', '" & PHYINV_STATUS.Warning & "') THEN MAST.ASSET_NO")
            sbSQL.AppendLine("           ELSE NULL END")
            sbSQL.AppendLine("        ) AS ACTUAL_NUMBER_OF_SHELVES")

            '正常数 SUM(正常)
            sbSQL.AppendLine("       ,COUNT_BIG(")
            sbSQL.AppendLine("           CASE WHEN MAST.PHYINV_STATUS IN ('" & PHYINV_STATUS.Normal & "') THEN MAST.ASSET_NO")
            sbSQL.AppendLine("           ELSE NULL END")
            sbSQL.AppendLine("        ) AS IAS_PHYINV_NUM")

            'ロケ違い数 SUM(警告)
            sbSQL.AppendLine("       ,COUNT_BIG(")
            sbSQL.AppendLine("           CASE WHEN MAST.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ") ")
            sbSQL.AppendLine("                       AND   MAST.PHYINV_STATUS = '" & PHYINV_STATUS.Warning & "' THEN MAST.ASSET_NO")
            sbSQL.AppendLine("           ELSE NULL END")
            sbSQL.AppendLine("        ) AS IAS_LOC_DIFF_NUM")

            '在庫なし数 SUM(現物なし(未処理を含む))
            sbSQL.AppendLine("       ,COUNT_BIG(")
            sbSQL.AppendLine("           CASE WHEN MAST.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ") ")
            sbSQL.AppendLine("                       AND   MAST.PHYINV_STATUS = '" & PHYINV_STATUS.NoSpot & "' THEN MAST.ASSET_NO")
            sbSQL.AppendLine("           ELSE NULL END")
            sbSQL.AppendLine("        ) AS IAS_NO_ITEM_NUM")

            sbSQL.AppendLine("      ,COUNT(*) OVER () AS TotalCount ")

            sbSQL.AppendLine("FROM T_INV_RESULT MAST")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AUTH")
            sbSQL.AppendLine("ON  AUTH.LOC_L_CD = MAST.LOC_L_CD")
            sbSQL.AppendLine("AND ((AUTH.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND AUTH.LOC_M_CD = MAST.LOC_M_CD) OR (AUTH.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryAssetHistory, _LoginInfo)
            params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@ReadAuthFlg", authInfo.ReadAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthFlg", authInfo.UpdAuthFlg))
            params.Add(New SqlParameter($"@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
            params.Add(New SqlParameter($"@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthOption", authInfo.AuthOption))

            '20231019 ADD Start Shirakawa
            sbSQL.AppendLine("INNER JOIN T_INVENTORY INV")
            sbSQL.AppendLine("ON INV.INV_ID = MAST.INV_ID ")
            '20231019 ADD End Shirakawa

            sbSQL.AppendLine("WHERE INV.START_DEC_TS = " & Me._startDecTs)

            '検索条件を設定する
            '[CC431] DEL 20261009 検索条件の組み立て（ロケーション 大区分〜資産リース区分）を SetSearchCondition に移動
            '[CC431] UPD Start 20261009
            ' 検索条件は一覧（CreateSQL）と共通の SetSearchCondition で作成する。
            ' SetSearchCondition が _paramSqlList に追加したパラメータを、本処理の params へ移す
            Dim iParamStart As Integer = _paramSqlList.Count
            sbSQL.Append(SetSearchCondition("MAST").ToString())
            params.AddRange(_paramSqlList.Skip(iParamStart))
            _paramSqlList.RemoveRange(iParamStart, _paramSqlList.Count - iParamStart)
            '[CC431] UPD End 20261009



            sbSQL.AppendLine("GROUP BY INV.START_DEC_TS")

            _viewInventory = db.IASInventoryView.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            ViewBag.LogicInvNum = 0
            ViewBag.PhyinvNum = 0
            ViewBag.LocDiffNum = 0
            ViewBag.NoItemNum = 0
            If _viewInventory.Count > 0 Then
                ViewBag.LogicInvNum = _viewInventory(0).IAS_LOGIC_INV_NUM
                ViewBag.PhyinvNum = _viewInventory(0).IAS_PHYINV_NUM
                ViewBag.LocDiffNum = _viewInventory(0).IAS_LOC_DIFF_NUM
                ViewBag.NoItemNum = _viewInventory(0).IAS_NO_ITEM_NUM
            End If

        End Sub
#End Region

        '20230904 ADD Start Shirakawa
#Region "検索ボタン押下"
        ''' <summary>
        ''' 検索ボタン押下
        ''' </summary>
        ''' <returns></returns>
        Public Function SearchClick() As ActionResult
            Session(CommonSessionKey.SEARCH_FLG) = True

            '結果表示
            Return RedirectToAction("Index")
        End Function
#End Region
        '20230904 ADD End Shirakawa

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
            Dim m = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim params As List(Of SqlParameter)

            '権限情報の取得        '--2021/11/10 add nv iwamoto
            Dim authInfo As AuthInfo = m.GetFunctionAuth(Misc.Auth.FunctionId.InventoryAssetHistory, _LoginInfo)

#Region "検索条件を設定する"
            Dim sbWhr As New StringBuilder
            'sbWhr.AppendLine("WHERE INV.START_DEC_TS = " & _startDecTs)

            sbWhr.AppendLine("WHERE TIR.INV_ID = " & _invId)   '20321019 ADD Shirakawa



            'Dim SqlLocCd As String = String.Empty
            ''配列から1つの文字列に結合
            'For i As Integer = 0 To _invLocLCd.Count - 1
            '    SqlLocCd = $"{SqlLocCd}'{_invLocLCd(i)}'"
            '    '次にまだ要素がある場合、後ろに,(カンマ)をつける
            '    If i <> _invLocLCd.Count - 1 Then
            '        SqlLocCd = $"{SqlLocCd},"
            '    End If
            'Next

            ''初回(_ViewModel.SearchLocL = "")の時に遷移時に受け取った値をロケ大CDを使う
            ''コンボボックスの空白(全ロケCD取得)を選んだ場合Nothingが返ってくる
            'If _ViewModel.SearchLocL Is String.Empty Then
            '    _ViewModel.SearchLocL = SqlLocCd
            'Else
            '    If _ViewModel.SearchLocL IsNot Nothing Then
            '        'コロンが含まれている場合
            '        If _ViewModel.SearchLocL.Contains("：") Then
            '            _ViewModel.SearchLocL = SqlLocCd
            '        End If
            '    End If
            'End If
            ''20231019 ADD End Shirakawa

            '[CC431] DEL 20261009 検索条件の組み立て（ロケーション 大区分〜タグ紐付状態）を SetSearchCondition に移動
            '[CC431] UPD Start 20261009
            sbWhr.Append(SetSearchCondition("TIR").ToString())
            '[CC431] UPD End 20261009

#End Region

            Dim sbSQL As New StringBuilder
            '一覧表示表用SQL
            sbSQL.Clear()
            params = New List(Of SqlParameter)

            If isSort Then
                sbSQL.AppendLine("SELECT ")
            Else
                sbSQL.AppendLine("SELECT TOP 1")
            End If
            'sbSQL.AppendLine("INV.START_DEC_TS")
            sbSQL.AppendLine("TIR.ASSET_NO")
            sbSQL.AppendLine(",TIR.SUB_ASSET_NO")
            sbSQL.AppendLine(",CASE")
            sbSQL.AppendLine("WHEN TIR.ASSET_STATUS = " & ASSET_STATUS.Preparation & " THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_UNLINKED) & "'")  '未紐付け
            sbSQL.AppendLine("WHEN TIR.ASSET_STATUS = " & ASSET_STATUS.Storage & " THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_STOCK) & "'")     '在庫
            sbSQL.AppendLine("WHEN TIR.ASSET_STATUS = " & ASSET_STATUS.Lending & " THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_LENDING) & "'")   '貸出
            sbSQL.AppendLine("WHEN TIR.ASSET_STATUS = " & ASSET_STATUS.Removed & " THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_REMOVALE) & "'")  '除却
            sbSQL.AppendLine("ELSE ''")
            sbSQL.AppendLine("END ASSET_STATUS_NAME")
            sbSQL.AppendLine(",TIR.PHYINV_STATUS")
            sbSQL.AppendLine(",CASE")
            sbSQL.AppendLine("WHEN TIR.PHYINV_STATUS = '" & PHYINV_STATUS.Excluded & "' THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_EXCLUDED) & "'")   '対象外
            sbSQL.AppendLine("WHEN TIR.PHYINV_STATUS = '" & PHYINV_STATUS.NoSpot & "' THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_NOSPOT) & "'")     '現物なし(未処理を含む)
            sbSQL.AppendLine("WHEN TIR.PHYINV_STATUS = '" & PHYINV_STATUS.Normal & "' THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_NORMAL) & "'")     '正常
            sbSQL.AppendLine("WHEN TIR.PHYINV_STATUS = '" & PHYINV_STATUS.Warning & "' THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_INKIND) & "'")     '現物あり(ロケ違い)
            sbSQL.AppendLine("ELSE ''")
            sbSQL.AppendLine("END PHYINV_STATUS_NAME")

            sbSQL.AppendLine(",TIR.LOC_L_CD")
            sbSQL.AppendLine(",ml.LOC_NAME LOC_L_NAME")
            sbSQL.AppendLine(",TIR.LOC_M_CD")
            sbSQL.AppendLine(",mm.LOC_NAME LOC_M_NAME")
            sbSQL.AppendLine(",TIR.LOC_S_CD")
            sbSQL.AppendLine(",ms.LOC_NAME LOC_S_NAME")
            sbSQL.AppendLine(",TIR.PHYINV_LOC_L_CD")
            sbSQL.AppendLine(",TIR.PHYINV_LOC_L_NAME")
            sbSQL.AppendLine(",TIR.PHYINV_LOC_M_CD")
            sbSQL.AppendLine(",TIR.PHYINV_LOC_M_NAME")
            sbSQL.AppendLine(",TIR.PHYINV_LOC_S_CD")
            sbSQL.AppendLine(",TIR.PHYINV_LOC_S_NAME")
            sbSQL.AppendLine(",TIR.CAT_L_CD")
            sbSQL.AppendLine(",mc.CAT_NAME CAT_L_NAME")
            sbSQL.AppendLine(",TIR.MAKER")
            sbSQL.AppendLine(",TIR.PRODUCT_NO")
            sbSQL.AppendLine(",TIR.MODEL_NO")

            sbSQL.AppendLine(",TIR.IS_TANGIBLE")
            sbSQL.AppendLine(",TIR.ASSET_NAME")
            sbSQL.AppendLine(",TIR.PHYINV_TS PHYINV_DATE")
            sbSQL.AppendLine(",TIR.PHYINV_EMPLOYEE_CD")
            sbSQL.AppendLine(",TIR.PHYINV_EMPLOYEE_NAME")

            sbSQL.AppendLine(",TIR.FREE_ITEM6")
            sbSQL.AppendLine(",IsNull(EM.EMPLOYEE_NAME, '') AS FREE_ITEM6_NAME")

            sbSQL.AppendLine(",TIR.FREE_ITEM1")
            sbSQL.AppendLine(",TIR.FREE_ITEM2")
            sbSQL.AppendLine(",TIR.FREE_ITEM3")
            sbSQL.AppendLine(",TIR.FREE_ITEM4")
            sbSQL.AppendLine(",TIR.FREE_ITEM5")
            sbSQL.AppendLine(",TIR.FREE_ITEM7")
            sbSQL.AppendLine(",TIR.FREE_ITEM8")
            sbSQL.AppendLine(",TIR.FREE_ITEM9")
            sbSQL.AppendLine(",TIR.FREE_ITEM10")
            sbSQL.AppendLine(",TIR.FREE_ITEM11")
            sbSQL.AppendLine(",TIR.FREE_ITEM12")
            sbSQL.AppendLine(",TIR.FREE_ITEM13")
            sbSQL.AppendLine(",TIR.FREE_ITEM14")
            sbSQL.AppendLine(",TIR.FREE_ITEM15")
            sbSQL.AppendLine(",TIR.FREE_ITEM16")
            sbSQL.AppendLine(",TIR.FREE_ITEM17")
            sbSQL.AppendLine(",TIR.FREE_ITEM18")
            sbSQL.AppendLine(",TIR.FREE_ITEM19")
            sbSQL.AppendLine(",TIR.FREE_ITEM20")
            sbSQL.AppendLine(",TIR.FREE_ITEM21")
            sbSQL.AppendLine(",TIR.FREE_ITEM22")
            sbSQL.AppendLine(",TIR.FREE_ITEM23")
            sbSQL.AppendLine(",TIR.FREE_ITEM24")
            sbSQL.AppendLine(",TIR.FREE_ITEM25")
            sbSQL.AppendLine(",TIR.FREE_ITEM26")
            sbSQL.AppendLine(",TIR.FREE_ITEM27")
            sbSQL.AppendLine(",TIR.FREE_ITEM28")
            sbSQL.AppendLine(",TIR.FREE_ITEM29")
            sbSQL.AppendLine(",TIR.FREE_ITEM30")
            sbSQL.AppendLine(",TIR.FREE_ITEM31")
            sbSQL.AppendLine(",TIR.FREE_ITEM32")
            sbSQL.AppendLine(",TIR.FREE_ITEM33")
            sbSQL.AppendLine(",TIR.FREE_ITEM34")
            sbSQL.AppendLine(",TIR.FREE_ITEM35")
            sbSQL.AppendLine(",TIR.FREE_ITEM36")
            sbSQL.AppendLine(",TIR.FREE_ITEM37")
            sbSQL.AppendLine(",TIR.FREE_ITEM38")
            sbSQL.AppendLine(",TIR.FREE_ITEM39")
            sbSQL.AppendLine(",TIR.FREE_ITEM40")
            sbSQL.AppendLine(",TIR.FREE_ITEM41")
            sbSQL.AppendLine(",TIR.FREE_ITEM42")
            sbSQL.AppendLine(",TIR.FREE_ITEM43")
            sbSQL.AppendLine(",TIR.FREE_ITEM44")
            sbSQL.AppendLine(",TIR.FREE_ITEM45")
            sbSQL.AppendLine(",TIR.FREE_ITEM46")
            sbSQL.AppendLine(",TIR.FREE_ITEM47")
            sbSQL.AppendLine(",TIR.FREE_ITEM48")
            sbSQL.AppendLine(",TIR.FREE_ITEM49")
            sbSQL.AppendLine(",TIR.FREE_ITEM50")
            sbSQL.AppendLine(",TIR.FREE_ITEM51")
            sbSQL.AppendLine(",TIR.FREE_ITEM52")
            sbSQL.AppendLine(",TIR.FREE_ITEM53")
            sbSQL.AppendLine(",TIR.FREE_ITEM54")
            sbSQL.AppendLine(",TIR.FREE_ITEM55")
            sbSQL.AppendLine(",TIR.FREE_ITEM56")
            sbSQL.AppendLine(",TIR.FREE_ITEM57")
            sbSQL.AppendLine(",TIR.FREE_ITEM58")
            sbSQL.AppendLine(",TIR.FREE_ITEM59")
            sbSQL.AppendLine(",TIR.FREE_ITEM60")
            sbSQL.AppendLine(",TIR.TAG_LINK_STATUS ")
            sbSQL.AppendLine(",TIR.OPE_STATUS")
            sbSQL.AppendLine(",TIR.DIVERSION_REMOVE")
            sbSQL.AppendLine(",IsNull(OPE.DATA2, '') AS OPE_STATUS_NAME")

            sbSQL.AppendLine(",TIR.REMOVE_DATE")
            sbSQL.AppendLine(",TIR.REMOVE_REASON")
            sbSQL.AppendLine(",IsNull(RR.DATA2, '') AS REMOVE_REASON_NAME")
            sbSQL.AppendLine(",TIR.REMOVE_CONTENT")
            sbSQL.AppendLine(",TIR.CHANGE_LOC_M_CD")
            sbSQL.AppendLine(",IsNull(CLM.LOC_NAME, '') AS CHANGE_LOC_M_NAME")
            sbSQL.AppendLine(",TIR.CHANGE_LOC_S_CD")
            sbSQL.AppendLine(",IsNull(CLS.LOC_NAME, '') AS CHANGE_LOC_S_NAME")
            sbSQL.AppendLine(",TIR.CHANGE_EMPLOYEE")
            sbSQL.AppendLine(",IsNull(CE.EMPLOYEE_NAME, '') AS CHANGE_EMPLOYEE_NAME")

            sbSQL.AppendLine(",TIR.REMARK1")
            sbSQL.AppendLine(",TIR.REMARK2")
            sbSQL.AppendLine(",TIR.REMARK3")
            sbSQL.AppendLine(",TIR.REMARK4")
            sbSQL.AppendLine(",TIR.REMARK5")
            sbSQL.AppendLine(",TIR.REMARK6")
            sbSQL.AppendLine(",TIR.REMARK7")
            sbSQL.AppendLine(",TIR.REMARK8")
            sbSQL.AppendLine(",TIR.REMARK9")
            sbSQL.AppendLine(",TIR.REMARK10")
            '[CC431] ADD Start 20261009
            sbSQL.AppendLine(",TIR.PHYINV_TOOL_DIV")
            sbSQL.AppendLine(",TIR.ENT_EMPLOYEE_CD")
            sbSQL.AppendLine(",EE.EMPLOYEE_NAME AS ENT_EMPLOYEE_NAME")
            '[CC431] ADD End 20261009


            sbSQL.AppendLine(",TotalCount AS TotalCount ")

            sbSQL.AppendLine("FROM T_INV_RESULT TIR")
            '20231019 ADD Start Shirakawa
            sbSQL.AppendLine("INNER JOIN T_INVENTORY INV")
            sbSQL.AppendLine("ON INV.INV_ID = TIR.INV_ID ")
            '20231019 ADD Start Shirakawa
            '--2021/11/10 add start nv iwamoto
            'sbSQL.AppendLine("INNER JOIN M_ASSET ASSET")
            'sbSQL.AppendLine("ON  ASSET.ASSET_NO = TIR.ASSET_NO")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuth) AUTH")
            sbSQL.AppendLine("ON  AUTH.LOC_L_CD = TIR.LOC_L_CD")
            sbSQL.Append("AND ((AUTH.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND AUTH.LOC_M_CD = TIR.LOC_M_CD) OR (AUTH.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{authInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{authInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{authInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{authInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthType_UpdAuth", $"{authInfo.AuthOption}")
            '--2021/11/10 add end nv iwamoto

            sbSQL.AppendLine("INNER JOIN (")
            sbSQL.AppendLine("  SELECT COUNT(*) AS totalCount")
            sbSQL.AppendLine("  FROM T_INV_RESULT TIR")
            '20231019 ADD Start Shirakawa
            sbSQL.AppendLine("INNER JOIN T_INVENTORY INV")
            sbSQL.AppendLine("ON INV.INV_ID = TIR.INV_ID ")
            '20231019 ADD Start Shirakawa
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuth) AUTH")
            sbSQL.AppendLine("  ON  AUTH.LOC_L_CD = TIR.LOC_L_CD")
            sbSQL.Append("AND ((AUTH.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND AUTH.LOC_M_CD = TIR.LOC_M_CD) OR (AUTH.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
            sbSQL.AppendLine(" " & sbWhr.ToString)
            sbSQL.AppendLine(") C")
            sbSQL.AppendLine("ON  1 = 1")

            sbSQL.AppendLine("LEFT JOIN M_LOCATION ml ON")
            sbSQL.AppendLine("ml.LOC_DIV = '" & LOC_DIV.LocationL & "'")
            sbSQL.AppendLine("AND ml.LOC_L_CD = TIR.LOC_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION mm ON")
            sbSQL.AppendLine("mm.LOC_DIV = '" & LOC_DIV.LocationM & "'")
            sbSQL.AppendLine("AND mm.LOC_L_CD = TIR.LOC_L_CD")
            sbSQL.AppendLine("AND mm.LOC_M_CD = TIR.LOC_M_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION ms ON")
            sbSQL.AppendLine("ms.LOC_DIV = '" & LOC_DIV.LocationS & "'")
            sbSQL.AppendLine("AND ms.LOC_L_CD = TIR.LOC_L_CD")
            sbSQL.AppendLine("AND ms.LOC_M_CD = TIR.LOC_M_CD")
            sbSQL.AppendLine("AND ms.LOC_S_CD = TIR.LOC_S_CD")

            sbSQL.AppendLine(" LEFT JOIN M_LOCATION CLM ON CLM.LOC_DIV = 'M' AND CLM.LOC_L_CD = TIR.LOC_L_CD AND CLM.LOC_M_CD = TIR.CHANGE_LOC_M_CD ")
            sbSQL.AppendLine(" LEFT JOIN M_LOCATION CLS ON CLS.LOC_DIV = 'S' AND CLS.LOC_L_CD = TIR.LOC_L_CD AND CLS.LOC_M_CD = TIR.CHANGE_LOC_M_CD AND CLS.LOC_S_CD = TIR.CHANGE_LOC_S_CD")
            sbSQL.AppendLine(" LEFT JOIN M_EMPLOYEE CE ON CE.EMPLOYEE_CD = TIR.CHANGE_EMPLOYEE ")
            sbSQL.AppendLine(" LEFT JOIN M_EMPLOYEE EM ON EM.EMPLOYEE_CD = TIR.FREE_ITEM6 ")
            '[CC431] ADD Start 20261009
            sbSQL.AppendLine(" LEFT JOIN M_EMPLOYEE EE ON EE.EMPLOYEE_CD = TIR.ENT_EMPLOYEE_CD ")
            '[CC431] ADD End 20261009

            sbSQL.AppendLine(" LEFT JOIN M_SETTING OPE ON OPE.KEY_DIV = 'Ope_Status_Item' AND OPE.KEY1 = TIR.OPE_STATUS ")
            sbSQL.AppendLine(" LEFT JOIN M_SETTING RR ON RR.KEY_DIV = 'Remove_Reason_Item' AND RR.KEY1 = TIR.REMOVE_REASON ")

            'カテゴリ
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY MC")
            sbSQL.AppendLine($"  ON MC.CAT_DIV = '{CAT_DIV.CategoryL}' AND tir.CAT_L_CD = MC.CAT_L_CD")

            sbSQL.AppendLine(sbWhr.ToString)

            If isSort Then
                sbSQL.AppendLine("ORDER BY")
                '[CC431] DEL 20261009 固定の並び順（ロケ大区分、中区分、小区分、資産番号）を削除し、検索条件の並び順に変更
                '[CC431] UPD Start 20261009
                ' 検索条件で選択された並び順で並べる。第2キーは資産番号昇順（一覧・データ出力の両方で使用）
                ' 並び順が資産番号のときは、第2キーを付けない（同じ列を ORDER BY に2回指定するとエラーになるため）
                '[CC431] UPD Start 20261015
                ' 仕様変更2：並び順ごとのキーの並び（原価センタ・場所は2キー、空白は後ろ）をそのまま指定する
                sbSQL.AppendLine(GetOrderBy(_ViewModel.SearchSortOrder))
                '[CC431] UPD End 20261015
                '[CC431] UPD End 20261009
            End If

            Return sbSQL.ToString()

        End Function

        '[CC431] ADD Start 20261009
        ''' <summary>
        ''' 検索条件の条件句を作成し、SQLパラメータを設定します（一覧・数量の集計・データ出力で共通）
        ''' </summary>
        ''' <param name="tblAlias">過去棚卸実績T（T_INV_RESULT）の別名（一覧：TIR、数量の集計：MAST）</param>
        ''' <returns>「AND …」形式の条件（WHERE 句の先頭の条件は呼び出し側で指定する）</returns>
        ''' <remarks>パラメータは addSqlParam で _paramSqlList に追加する。検索条件を追加するときは、ここに追加する</remarks>
        Private Function SetSearchCondition(ByVal tblAlias As String) As StringBuilder
            Dim sbCond As New StringBuilder

            'ロケーション 大区分
            If Not IsNothing(_ViewModel.SearchLocL) AndAlso _ViewModel.SearchLocL <> "" Then
                sbCond.AppendLine("AND " & tblAlias & ".LOC_L_CD = @locLCd")
                addSqlParam("locLCd", _ViewModel.SearchLocL)
            End If
            'ロケーション 中区分（複数選択）
            sbCond.AppendLine(CreateInCondition(tblAlias & ".LOC_M_CD", "locMCd", _ViewModel.SearchLocM))
            'ロケーション 小区分（複数選択）
            sbCond.AppendLine(CreateInCondition(tblAlias & ".LOC_S_CD", "locSCd", _ViewModel.SearchLocS))

            'カテゴリー 大分類（複数選択）
            sbCond.AppendLine(CreateInCondition(tblAlias & ".CAT_L_CD", "catLCd", _ViewModel.SearchCatL))

            ' 実棚ステータス 全て
            If Not String.IsNullOrEmpty(_ViewModel.SearchPhyinvStatusSelect) Then
                Select Case _ViewModel.SearchPhyinvStatusSelect
                    Case PHYINV_STATUS.Normal
                        sbCond.AppendLine("AND " & tblAlias & ".PHYINV_STATUS = @SearchPhyinvStatusSelect ")
                        addSqlParam($"SearchPhyinvStatusSelect", $"{_ViewModel.SearchPhyinvStatusSelect}")
                    Case PHYINV_STATUS.Warning
                        sbCond.AppendLine("AND " & tblAlias & ".PHYINV_STATUS = @SearchPhyinvStatusSelect ")
                        addSqlParam($"SearchPhyinvStatusSelect", $"{_ViewModel.SearchPhyinvStatusSelect}")
                    Case PHYINV_STATUS.NoSpot
                        sbCond.AppendLine("AND " & tblAlias & ".PHYINV_STATUS = @SearchPhyinvStatusSelect ")
                        addSqlParam($"SearchPhyinvStatusSelect", $"{_ViewModel.SearchPhyinvStatusSelect}")
                End Select
            End If

            ' 資産番号（「|」区切りで複数指定。資産番号・子番号のいずれかに前方一致）
            sbCond.AppendLine(CreateLikeCondition({tblAlias & ".ASSET_NO", tblAlias & ".SUB_ASSET_NO"}, "assetNo", CommonRfid.GetMultiInputValues(_ViewModel.SearchAssetNo)))
            ' 資産名称
            If Not IsNothing(_ViewModel.SearchAssetName) AndAlso _ViewModel.SearchAssetName <> "" Then
                sbCond.AppendLine("  AND " & tblAlias & ".ASSET_NAME Like @assetName")
                addSqlParam("assetName", $"%{_ViewModel.SearchAssetName}%")
            End If

            '有形・無形区分
            If _ViewModel.SearchTangibleCheck <> _ViewModel.SearchNotTangibleCheck Then
                '有形資産、無形資産の値が違う場合は条件に追加
                sbCond.AppendLine("  AND " & tblAlias & ".IS_TANGIBLE = " & "@SearchIsTangible")
                If _ViewModel.SearchTangibleCheck Then
                    addSqlParam("SearchIsTangible", $"{ TANGIBLE.TANGIBLE }")
                Else
                    addSqlParam("SearchIsTangible", $"{ TANGIBLE.NOT_TANGIBLE }")
                End If
            End If

            'フリー項目10 SearchFreeItem10 貸与資産状態区分
            If (Not IsNothing(_ViewModel.SearchFreeItem10)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem10.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM10 IN (" & "@SearchFreeItem10" & "," & "@SearchFreeItem10Name" & ")")
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

            '日付範囲の検索は "yyyy/MM/dd" の文字列の大小比較で行うため、書式外の値は検索対象にしない（ヒットさせない）
            Const sDateFormatCond As String = " LIKE '[0-9][0-9][0-9][0-9]/[0-9][0-9]/[0-9][0-9]'"

            'フリー項目2From SearchFreeItem2From 資本化日付
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem2From) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem2From).ToString("yyyy/MM/dd")
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM2" & sDateFormatCond)
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM2 >= " & "@SearchFreeItem2From")
                addSqlParam("SearchFreeItem2From", sDate)
            End If

            'フリー項目2To SearchFreeItem2To 資本化日付
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem2To) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem2To).ToString("yyyy/MM/dd")
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM2" & sDateFormatCond)
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM2 <= " & "@SearchFreeItem2To")
                addSqlParam("SearchFreeItem2To", sDate)
            End If

            'フリー項目4 SearchFreeItem4 稼働区分名
            If (Not IsNothing(_ViewModel.SearchFreeItem4)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem4.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM4 like " & "@SearchFreeItem4")
                addSqlParam("SearchFreeItem4", $"{"%" & _ViewModel.SearchFreeItem4.Trim & "%"}")
            End If

            'フリー項目5 SearchFreeItem5 型式
            If (Not IsNothing(_ViewModel.SearchFreeItem5)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem5.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM5 like " & "@SearchFreeItem5")
                addSqlParam("SearchFreeItem5", $"{"%" & _ViewModel.SearchFreeItem5.Trim & "%"}")
            End If

            'フリー項目6 SearchFreeItem6 管理社員番号
            If (Not IsNothing(_ViewModel.SearchFreeItem6)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem6.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM6 like " & "@SearchFreeItem6")
                addSqlParam("SearchFreeItem6", $"{"%" & _ViewModel.SearchFreeItem6.Trim & "%"}")
            End If

            'フリー項目7 SearchFreeItem7 シリアルNo
            If (Not IsNothing(_ViewModel.SearchFreeItem7)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem7.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM7 like " & "@SearchFreeItem7")
                addSqlParam("SearchFreeItem7", $"{"%" & _ViewModel.SearchFreeItem7.Trim & "%"}")
            End If

            'フリー項目8 SearchFreeItem8 WBS番号
            If (Not IsNothing(_ViewModel.SearchFreeItem8)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem8.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM8 like " & "@SearchFreeItem8")
                addSqlParam("SearchFreeItem8", $"{"%" & _ViewModel.SearchFreeItem8.Trim & "%"}")
            End If

            'フリー項目9 SearchFreeItem9 備考
            If (Not IsNothing(_ViewModel.SearchFreeItem9)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem9.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM9 like " & "@SearchFreeItem9")
                addSqlParam("SearchFreeItem9", $"{"%" & _ViewModel.SearchFreeItem9.Trim & "%"}")
            End If

            'フリー項目11From SearchFreeItem11From 取得金額
            If (Not IsNothing(_ViewModel.SearchFreeItem11From)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem11From.Trim)) Then
                sbCond.AppendLine("  AND TRY_CAST(" & tblAlias & ".FREE_ITEM11 AS decimal(23, 2)) >= " & "@SearchFreeItem11From")
                addSqlParam("SearchFreeItem11From", $"{Decimal.Parse(_ViewModel.SearchFreeItem11From.Trim)}")
            End If

            'フリー項目11To SearchFreeItem11To 取得金額
            If (Not IsNothing(_ViewModel.SearchFreeItem11To)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem11To.Trim)) Then
                sbCond.AppendLine("  AND TRY_CAST(" & tblAlias & ".FREE_ITEM11 AS decimal(23, 2)) <= " & "@SearchFreeItem11To")
                addSqlParam("SearchFreeItem11To", $"{Decimal.Parse(_ViewModel.SearchFreeItem11To.Trim)}")
            End If

            'フリー項目12From SearchFreeItem12From 当月末簿価
            If (Not IsNothing(_ViewModel.SearchFreeItem12From)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem12From.Trim)) Then
                sbCond.AppendLine("  AND TRY_CAST(" & tblAlias & ".FREE_ITEM12 AS decimal(23, 2)) >= " & "@SearchFreeItem12From")
                addSqlParam("SearchFreeItem12From", $"{Decimal.Parse(_ViewModel.SearchFreeItem12From.Trim)}")
            End If

            'フリー項目12To SearchFreeItem12To 当月末簿価
            If (Not IsNothing(_ViewModel.SearchFreeItem12To)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem12To.Trim)) Then
                sbCond.AppendLine("  AND TRY_CAST(" & tblAlias & ".FREE_ITEM12 AS decimal(23, 2)) <= " & "@SearchFreeItem12To")
                addSqlParam("SearchFreeItem12To", $"{Decimal.Parse(_ViewModel.SearchFreeItem12To.Trim)}")
            End If

            'フリー項目14 SearchFreeItem14 シリアル番号
            If (Not IsNothing(_ViewModel.SearchFreeItem14)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem14.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM14 like " & "@SearchFreeItem14")
                addSqlParam("SearchFreeItem14", $"{"%" & _ViewModel.SearchFreeItem14.Trim & "%"}")
            End If

            'フリー項目15 SearchFreeItem15 資産グループコード
            If (Not IsNothing(_ViewModel.SearchFreeItem15)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem15.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM15 like " & "@SearchFreeItem15")
                addSqlParam("SearchFreeItem15", $"{"%" & _ViewModel.SearchFreeItem15.Trim & "%"}")
            End If

            'フリー項目16 SearchFreeItem16 貸与先
            If (Not IsNothing(_ViewModel.SearchFreeItem16)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem16.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM16 like " & "@SearchFreeItem16")
                addSqlParam("SearchFreeItem16", $"{"%" & _ViewModel.SearchFreeItem16.Trim & "%"}")
            End If

            'フリー項目17 SearchFreeItem17 貸与先名
            If (Not IsNothing(_ViewModel.SearchFreeItem17)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem17.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM17 like " & "@SearchFreeItem17")
                addSqlParam("SearchFreeItem17", $"{"%" & _ViewModel.SearchFreeItem17.Trim & "%"}")
            End If

            'フリー項目18From SearchFreeItem18From 開始日(貸与)
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem18From) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem18From).ToString("yyyy/MM/dd")
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM18" & sDateFormatCond)
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM18 >= " & "@SearchFreeItem18From")
                addSqlParam("SearchFreeItem18From", sDate)
            End If

            'フリー項目18To SearchFreeItem18To 開始日(貸与)
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem18To) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem18To).ToString("yyyy/MM/dd")
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM18" & sDateFormatCond)
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM18 <= " & "@SearchFreeItem18To")
                addSqlParam("SearchFreeItem18To", sDate)
            End If

            'フリー項目19From SearchFreeItem19From 終了日(貸与)
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem19From) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem19From).ToString("yyyy/MM/dd")
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM19" & sDateFormatCond)
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM19 >= " & "@SearchFreeItem19From")
                addSqlParam("SearchFreeItem19From", sDate)
            End If

            'フリー項目19To SearchFreeItem19To 終了日(貸与)
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem19To) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem19To).ToString("yyyy/MM/dd")
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM19" & sDateFormatCond)
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM19 <= " & "@SearchFreeItem19To")
                addSqlParam("SearchFreeItem19To", sDate)
            End If

            'フリー項目20From SearchFreeItem20From 貸与回数
            If (Not IsNothing(_ViewModel.SearchFreeItem20From)) AndAlso
                    (IsNumeric(_ViewModel.SearchFreeItem20From.Trim)) Then
                sbCond.AppendLine("  AND TRY_CAST(" & tblAlias & ".FREE_ITEM20 AS INT) >= " & "@SearchFreeItem20From")
                addSqlParam("SearchFreeItem20From", $"{CInt(_ViewModel.SearchFreeItem20From.Trim)}")
            End If

            'フリー項目20To SearchFreeItem20To 貸与回数
            If (Not IsNothing(_ViewModel.SearchFreeItem20To)) AndAlso
                    (IsNumeric(_ViewModel.SearchFreeItem20To.Trim)) Then
                sbCond.AppendLine("  AND TRY_CAST(" & tblAlias & ".FREE_ITEM20 AS INT) <= " & "@SearchFreeItem20To")
                addSqlParam("SearchFreeItem20To", $"{CInt(_ViewModel.SearchFreeItem20To.Trim)}")
            End If

            'フリー項目21 SearchFreeItem21 備考欄
            If (Not IsNothing(_ViewModel.SearchFreeItem21)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem21.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM21 like " & "@SearchFreeItem21")
                addSqlParam("SearchFreeItem21", $"{"%" & _ViewModel.SearchFreeItem21.Trim & "%"}")
            End If

            'フリー項目39 SearchFreeItem39 市区町村コード名称
            If (Not IsNothing(_ViewModel.SearchFreeItem39)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem39.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM39 like " & "@SearchFreeItem39")
                addSqlParam("SearchFreeItem39", $"{"%" & _ViewModel.SearchFreeItem39.Trim & "%"}")
            End If

            'フリー項目42 SearchFreeItem42 CAPS番号
            If (Not IsNothing(_ViewModel.SearchFreeItem42)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem42.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM42 like " & "@SearchFreeItem42")
                addSqlParam("SearchFreeItem42", $"{"%" & _ViewModel.SearchFreeItem42.Trim & "%"}")
            End If

            'フリー項目37 SearchFreeItem37 資産リース区分
            If (Not IsNothing(_ViewModel.SearchFreeItem37)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem37.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM37 like " & "@SearchFreeItem37")
                addSqlParam("SearchFreeItem37", $"{"%" & _ViewModel.SearchFreeItem37.Trim & "%"}")
            End If

            'タグ紐付状態 SearchTagLinkStatus
            If (Not IsNothing(_ViewModel.SearchTagLinkStatus)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTagLinkStatus)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".TAG_LINK_STATUS = " & "@SearchTagLinkStatus")
                addSqlParam("SearchTagLinkStatus", $"{_ViewModel.SearchTagLinkStatus}")
            End If

            '実棚日From（実棚日時は YYYYMMDDHHMMSS の数値。From はその日の始まりから）
            If Not String.IsNullOrEmpty(_ViewModel.SearchPhyinvDateFrom) Then
                sbCond.AppendLine("  AND " & tblAlias & ".PHYINV_TS >= " & "@SearchPhyinvDateFrom")
                addSqlParam("SearchPhyinvDateFrom", CDate(_ViewModel.SearchPhyinvDateFrom).ToString("yyyyMMdd") & "000000")
            End If

            '実棚日To（To はその日の終わりまで含める）
            If Not String.IsNullOrEmpty(_ViewModel.SearchPhyinvDateTo) Then
                sbCond.AppendLine("  AND " & tblAlias & ".PHYINV_TS <= " & "@SearchPhyinvDateTo")
                addSqlParam("SearchPhyinvDateTo", CDate(_ViewModel.SearchPhyinvDateTo).ToString("yyyyMMdd") & "235959")
            End If

            '実棚者コード（前方一致）
            If (Not IsNothing(_ViewModel.SearchPhyinvEmployee)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchPhyinvEmployee.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".PHYINV_EMPLOYEE_CD like " & "@SearchPhyinvEmployee")
                addSqlParam("SearchPhyinvEmployee", $"{_ViewModel.SearchPhyinvEmployee.Trim & "%"}")
            End If

            '棚卸実施端末（実棚処理ツール区分）
            If Not String.IsNullOrEmpty(_ViewModel.SearchPhyinvToolDiv) Then
                '[CC431] UPD Start 20261015
                ' 「-」は棚卸実施端末が未設定（NULL・空文字）のデータを抽出する
                If _ViewModel.SearchPhyinvToolDiv = ProcessToolDiv.NoValue Then
                    sbCond.AppendLine("  AND (" & tblAlias & ".PHYINV_TOOL_DIV IS NULL OR " & tblAlias & ".PHYINV_TOOL_DIV = '')")
                Else
                    sbCond.AppendLine("  AND " & tblAlias & ".PHYINV_TOOL_DIV = " & "@SearchPhyinvToolDiv")
                    addSqlParam("SearchPhyinvToolDiv", _ViewModel.SearchPhyinvToolDiv)
                End If
                '[CC431] UPD End 20261015
            End If

            'フリー項目27 SearchFreeItem27 棚卸ノート（部分一致）
            If (Not IsNothing(_ViewModel.SearchFreeItem27)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem27.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".FREE_ITEM27 like " & "@SearchFreeItem27")
                addSqlParam("SearchFreeItem27", $"{"%" & _ViewModel.SearchFreeItem27.Trim & "%"}")
            End If

            '入力作業者（前方一致）
            If (Not IsNothing(_ViewModel.SearchEntEmployee)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchEntEmployee.Trim)) Then
                sbCond.AppendLine("  AND " & tblAlias & ".ENT_EMPLOYEE_CD like " & "@SearchEntEmployee")
                addSqlParam("SearchEntEmployee", $"{_ViewModel.SearchEntEmployee.Trim & "%"}")
            End If

            '詳細検索 備考1～備考10（「|」区切りで複数指定。同じ項目内は OR、項目同士は AND。部分一致）
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
                sbCond.AppendLine(CreateLikeCondition({$"{tblAlias}.{kvRemark.Key}"}, $"Search{kvRemark.Key}", CommonRfid.GetMultiInputValues(kvRemark.Value), True))
            Next

            Return sbCond
        End Function

        '[CC431] DEL 20261015 GetSortColumn（1列だけ返す）を削除し、GetOrderBy（キーの並び全体を返す）に置き換え
        '[CC431] ADD Start 20261015
        ''' <summary>
        ''' 並び順に対応する ORDER BY の中身（キーの並び）を取得する
        ''' </summary>
        ''' <param name="sortOrder">検索条件 並び順(InventoryInquirySort)</param>
        ''' <returns>ORDER BY に続けて指定する文字列（想定外の値は資産クラス）</returns>
        ''' <remarks>
        ''' 原価センタ・場所はコード順（仕様変更2）。第2キー以降は並び順ごとに定義し、資産番号を最後に付ける。
        ''' 資本化日付・管理者社員番号・実棚者コード・備考は、空白（NULL・空文字）を後ろにする。
        ''' </remarks>
        Private Function GetOrderBy(ByVal sortOrder As Integer) As String
            Select Case sortOrder
                Case InventoryInquirySort.CatLAsc
                    Return "TIR.CAT_L_CD,TIR.ASSET_NO"
                Case InventoryInquirySort.AssetNoAsc
                    Return "TIR.ASSET_NO"
                Case InventoryInquirySort.LocMAsc
                    Return "TIR.LOC_M_CD,TIR.LOC_S_CD,TIR.ASSET_NO"
                Case InventoryInquirySort.LocSAsc
                    Return "TIR.LOC_S_CD,TIR.LOC_M_CD,TIR.ASSET_NO"
                Case InventoryInquirySort.FreeItem2Asc
                    Return GetBlankLastOrder("TIR.FREE_ITEM2") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.FreeItem6Asc
                    Return GetBlankLastOrder("TIR.FREE_ITEM6") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.PhyinvEmployeeAsc
                    Return GetBlankLastOrder("TIR.PHYINV_EMPLOYEE_CD") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.Remark1Asc
                    Return GetBlankLastOrder("TIR.REMARK1") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.Remark2Asc
                    Return GetBlankLastOrder("TIR.REMARK2") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.Remark3Asc
                    Return GetBlankLastOrder("TIR.REMARK3") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.Remark4Asc
                    Return GetBlankLastOrder("TIR.REMARK4") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.Remark5Asc
                    Return GetBlankLastOrder("TIR.REMARK5") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.Remark6Asc
                    Return GetBlankLastOrder("TIR.REMARK6") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.Remark7Asc
                    Return GetBlankLastOrder("TIR.REMARK7") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.Remark8Asc
                    Return GetBlankLastOrder("TIR.REMARK8") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.Remark9Asc
                    Return GetBlankLastOrder("TIR.REMARK9") & ",TIR.ASSET_NO"
                Case InventoryInquirySort.Remark10Asc
                    Return GetBlankLastOrder("TIR.REMARK10") & ",TIR.ASSET_NO"
                Case Else
                    Return "TIR.CAT_L_CD,TIR.ASSET_NO"
            End Select
        End Function

        ''' <summary>
        ''' 空白（NULL・空文字）を後ろにする並びを返す。スペースのみは値ありの扱い
        ''' </summary>
        Private Function GetBlankLastOrder(ByVal col As String) As String
            Return "CASE WHEN " & col & " IS NULL OR " & col & " = '' THEN 1 ELSE 0 END," & col
        End Function
        '[CC431] ADD End 20261015
        '[CC431] ADD End 20261009

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New InventoryAssetHistoryViewModel())
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
            _viewResult.ElementAt(0).SearchAssetNo = _ViewModel.SearchAssetNo
            _viewResult.ElementAt(0).SearchAssetName = _ViewModel.SearchAssetName

            _viewResult.ElementAt(0).SearchTangibleCheck = _ViewModel.SearchTangibleCheck
            _viewResult.ElementAt(0).SearchNotTangibleCheck = _ViewModel.SearchNotTangibleCheck
            _viewResult.ElementAt(0).SearchAssetAdmin = _ViewModel.SearchAssetAdmin

            '[CC431] DEL 20261009 資本化日付（年月）を日付範囲に変更
            '[CC431] ADD Start 20261009
            _viewResult.ElementAt(0).SearchFreeItem2From = _ViewModel.SearchFreeItem2From
            _viewResult.ElementAt(0).SearchFreeItem2To = _ViewModel.SearchFreeItem2To
            '[CC431] ADD End 20261009
            _viewResult.ElementAt(0).SearchFreeItem4 = _ViewModel.SearchFreeItem4
            _viewResult.ElementAt(0).SearchFreeItem5 = _ViewModel.SearchFreeItem5
            _viewResult.ElementAt(0).SearchFreeItem6 = _ViewModel.SearchFreeItem6
            _viewResult.ElementAt(0).SearchFreeItem7 = _ViewModel.SearchFreeItem7
            _viewResult.ElementAt(0).SearchFreeItem8 = _ViewModel.SearchFreeItem8
            _viewResult.ElementAt(0).SearchFreeItem9 = _ViewModel.SearchFreeItem9
            _viewResult.ElementAt(0).SearchFreeItem10 = _ViewModel.SearchFreeItem10
            _viewResult.ElementAt(0).SearchFreeItem11From = _ViewModel.SearchFreeItem11From
            _viewResult.ElementAt(0).SearchFreeItem11To = _ViewModel.SearchFreeItem11To
            _viewResult.ElementAt(0).SearchFreeItem12From = _ViewModel.SearchFreeItem12From
            _viewResult.ElementAt(0).SearchFreeItem12To = _ViewModel.SearchFreeItem12To
            _viewResult.ElementAt(0).SearchFreeItem14 = _ViewModel.SearchFreeItem14
            _viewResult.ElementAt(0).SearchFreeItem15 = _ViewModel.SearchFreeItem15
            _viewResult.ElementAt(0).SearchFreeItem16 = _ViewModel.SearchFreeItem16
            _viewResult.ElementAt(0).SearchFreeItem17 = _ViewModel.SearchFreeItem17
            '[CC431] DEL 20261009 開始日(貸与)・終了日(貸与)を日付範囲に変更
            '[CC431] ADD Start 20261009
            _viewResult.ElementAt(0).SearchFreeItem18From = _ViewModel.SearchFreeItem18From
            _viewResult.ElementAt(0).SearchFreeItem18To = _ViewModel.SearchFreeItem18To
            _viewResult.ElementAt(0).SearchFreeItem19From = _ViewModel.SearchFreeItem19From
            _viewResult.ElementAt(0).SearchFreeItem19To = _ViewModel.SearchFreeItem19To
            '[CC431] ADD End 20261009
            _viewResult.ElementAt(0).SearchFreeItem20From = _ViewModel.SearchFreeItem20From
            _viewResult.ElementAt(0).SearchFreeItem20To = _ViewModel.SearchFreeItem20To
            _viewResult.ElementAt(0).SearchFreeItem21 = _ViewModel.SearchFreeItem21
            _viewResult.ElementAt(0).SearchFreeItem37 = _ViewModel.SearchFreeItem37
            _viewResult.ElementAt(0).SearchFreeItem39 = _ViewModel.SearchFreeItem39
            _viewResult.ElementAt(0).SearchFreeItem42 = _ViewModel.SearchFreeItem42
            _viewResult.ElementAt(0).SearchTagLinkStatus = CommonRfid.ToStringNvl(_ViewModel.SearchTagLinkStatus)
            '[CC431] ADD Start 20261009
            _viewResult.ElementAt(0).SearchPhyinvDateFrom = _ViewModel.SearchPhyinvDateFrom
            _viewResult.ElementAt(0).SearchPhyinvDateTo = _ViewModel.SearchPhyinvDateTo
            _viewResult.ElementAt(0).SearchPhyinvEmployee = _ViewModel.SearchPhyinvEmployee
            _viewResult.ElementAt(0).SearchPhyinvToolDiv = _ViewModel.SearchPhyinvToolDiv
            _viewResult.ElementAt(0).SearchFreeItem27 = _ViewModel.SearchFreeItem27
            _viewResult.ElementAt(0).SearchEntEmployee = _ViewModel.SearchEntEmployee
            _viewResult.ElementAt(0).SearchSortOrder = _ViewModel.SearchSortOrder
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
            _viewResult.ElementAt(0).DetailOpenFlg = _ViewModel.DetailOpenFlg
            '[CC431] ADD End 20261009
            '[CC431] ADD Start 20261015
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
            '[CC431] ADD End 20261015

            If String.IsNullOrEmpty(_ViewModel.SearchPhyinvStatusSelect) Then
                _viewResult.ElementAt(0).SearchPhyinvStatusSelect = "ALL"
            Else
                _viewResult.ElementAt(0).SearchPhyinvStatusSelect = _ViewModel.SearchPhyinvStatusSelect
            End If


            _viewResult.ElementAt(0).CsvPath = _ViewModel.CsvPath
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton

            If String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_ASSET_HISTORY, COLSET_LOC.TEMPLATE)
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_ASSET_HISTORY, COLSET_LOC.TEMPLATE).FIX_COL_CNT
            Else
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_ASSET_HISTORY, _ViewModel.SearchLocL)
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_ASSET_HISTORY, _ViewModel.SearchLocL).FIX_COL_CNT
            End If

        End Sub

#End Region

    End Class
End Namespace