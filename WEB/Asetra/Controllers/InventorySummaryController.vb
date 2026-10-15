Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers
    ''' <summary>
    ''' 棚卸概況画面表示用のクラス
    ''' </summary>
    Public Class InventorySummaryController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス変数"
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _ViewModel As InventorySummaryViewModel = New InventorySummaryViewModel                         'InventorySummary一覧表示用モデル
        Protected _viewResult As List(Of InventorySummaryViewModel) = New List(Of InventorySummaryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _isSort As Boolean = True                       'ソートフラグ

        Private _LocationLList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationMList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationSList As List(Of LocationDropdownModel)     'ロケーションマスタリスト

        Private _invLocCd As List(Of String) = New List(Of String)
        Private _AuthInfo As AuthInfo = Nothing                 '権限情報

#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As InventorySummaryViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(CommonSessionKey.SEARCH_FLG) IsNot Nothing Then
                Session.Remove(CommonSessionKey.SEARCH_FLG)
            End If
            '棚卸IDセッションクリア
            If Session(CommonSessionKey.INV_ID) IsNot Nothing Then
                Session.Remove(CommonSessionKey.INV_ID)
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
        Public Function Index(ByVal ViewModel As InventorySummaryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_INVENTORY_SUMMARY)
            End If

            '上限確認フラグ 初期化
            ViewBag.isLimitFlg = False
            '一覧件数　初期化
            ViewBag.cntList = 0
            '設定マスタ上限件数　初期化
            ViewBag.cntLimit = 0
            '一覧表示フラグ
            Dim isDisp As Boolean = True

            '20230901 ADD Start Shirakawa
            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid                      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Inventory_Overview.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Overview.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Overview.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Overview.Key3)
            '20230901 ADD End Shirakawa

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As InventorySummaryViewModel = Session(CommonSessionKey.CREAGE_INVENTORY_SUMMARY)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            '戻ってきた際の検索
            If Session(CommonSessionKey.INV_RETURN_LOC) IsNot Nothing Then
                _ViewModel.SearchLocL = Session(CommonSessionKey.INV_RETURN_LOC)
                'クリア
                Session.Remove(CommonSessionKey.INV_RETURN_LOC)
            End If

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
                ViewBag.modelLocL = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                CopyModelData()
                Return View(_viewResult)
            Else
                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
                ' 棚卸管理テーブルの状態を取得
                Dim m = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
                _ViewModel.START_DEC_TS = 0
                _ViewModel.INVENTORY_COUNT = 0
                _ViewModel.INVENTORY_MESSAGE = ""
                GetInventryInvConfirm()
                '--2019/09/12 upd start nv iwamoto
                'If _ViewModel.INVENTORY_COUNT > 0 Then
                '    _ViewModel.INVENTORY_MESSAGE = m.getMessage(CommonLabelInventorySummary.MSG_TANAOROSHI_KAISHI_ALREADY)
                'End If
                '20230929 DELETE Start Shirakawa
                'If _ViewModel.INVENTORY_COUNT > 0 Then
                '    _ViewModel.INVENTORY_MESSAGE = m.getMessage(CommonLabelInventorySummary.MSG_TANAOROSHI_KAISHI_ALREADY)
                'Else
                '    _ViewModel.INVENTORY_MESSAGE = m.getMessage(CommonLabelInventorySummary.LABEL_TANAOROSHI_KAISHI)
                'End If
                '20230929 DELETE End Shirakawa
                _ViewModel.INVENTORY_MESSAGE = m.getMessage(CommonLabelInventorySummary.LABEL_TANAOROSHI_KAISHI)
                '--2019/09/12 upd end nv iwamoto

                '--2021/11/08 add start nv iwamoto
                '権限情報の取得
                Dim authInfo As AuthInfo = m.GetFunctionAuth(Misc.Auth.FunctionId.InventorySummary, _LoginInfo)
                If authInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                Else
                    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                End If
                '--2021/11/08 add end nv iwamoto

                '検索条件の復元有無
                _ViewModel.IsRestoreOwnSearchConditions = True

                '20231025 DEL Shirakawa
                '' 棚卸開始宣言している場合のみ一覧表示
                'If _ViewModel.INVENTORY_COUNT <> 0 Then
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
            End If

            'End If

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

            'メッセージ一覧取得
            Dim Message As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '権限情報の取得
            _AuthInfo = Message.GetFunctionAuth(Misc.Auth.FunctionId.InventorySummary, _LoginInfo)

            ' (ロケーション)大区分ドロップダウンリスト作成
            GetLocationLData()
            '' (ロケーション)中区分
            'GetLocationMData(_ViewModel.SearchLocL)
            '' (ロケーション)小区分
            'GetLocationSData(_ViewModel.SearchLocL, _ViewModel.SearchLocM)

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = Nothing
            If String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                colSetModel = _db.ColSet.Find(DispId.INVENTORY_SUMMARY, COLSET_LOC.TEMPLATE)
            Else
                colSetModel = _db.ColSet.Find(DispId.INVENTORY_SUMMARY, _ViewModel.SearchLocL)
            End If

            If Not IsNothing(ColSetModel) AndAlso ColSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
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
        ''' ロケーション変更
        ''' </summary>
        ''' <param name="locationLevel"></param>
        ''' <param name="selectLCode"></param>
        ''' <param name="selectMCode"></param>
        ''' <returns></returns>
        Public Function ChangeLocation(ByVal locationLevel As String, ByVal selectLCode As String, ByVal selectMCode As String) As ActionResult

            '2020/03/13 実棚ロケ使用区分　の判断を追加 ADD Inoue
            'If locationLevel = LOC_DIV.LocationM Then
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
                    Return Json(GetLocationSData(selectLCode, selectMCode))
                Else
                    '小区分　使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})


        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()
            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_INVENTORY_SUMMARY)
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
            Dim query As DbSqlQuery(Of InventorySummaryViewModel) = _db.InventorySummaryView.SqlQuery(_SQL, _paramSqlList.ToArray)
            '画面に表示するViewを設定
            _viewResult = query.ToList
        End Sub

#End Region

#Region "棚卸確定処理"
        ''' <summary>
        ''' 棚卸確定処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function UpdateInventory(ByVal ViewModel As InventorySummaryViewModel, ByVal OnCreteButton As String, ByVal SelectKeys As String) As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '棚卸IDセッションクリア
            If Session(CommonSessionKey.INV_ID) IsNot Nothing Then
                Session.Remove(CommonSessionKey.INV_ID)
            End If

            '202231027 ADD Start Shirakawa
            Dim selectKey() As String = SelectKeys.Split(",")

            '棚卸IDを分離
            Dim invId As List(Of String) = New List(Of String)
            For i = 0 To selectKey.Count - 1
                invId.Add(selectKey(i).Split("/")(0))
            Next

            'ロケコードを分離させ配列に格納
            _invLocCd.Clear()
            For i = 0 To selectKey.Count - 1
                _invLocCd.Add(selectKey(i).Split("/")(1))
            Next

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            For Each id As String In invId
                If Not ToolInvAsset.CheckOpeStatus(id, _invLocCd) Then
                    '変更後稼働区分が入っていないデータがある場合はエラー
                    _ViewModel.States = StatusType.ErrorInvKakutei
                    TempData("ErrCode") = CommonLabelInventorySummary.MSG_OPE_STATUS_ERR
                    Session(CommonSessionKey.CREAGE_INVENTORY_SUMMARY) = _ViewModel
                    Session(CommonSessionKey.INV_ID) = invId
                    Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
                End If
            Next

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)
            Dim sTaskNm As String = "InventorySummaryController.UpdateInventory"
            Dim ret As Integer

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try

                    ' ①棚卸管理Tの棚卸確定関連の項目を設定

                    '20231013 ADD Start Shirakawa

                    '大ロケコード
                    Dim strLoc_Cd As String = ""

                    '20231013 ADD End Shirakawa

                    '20231012 ADD Start Shirakawa

                    ''20231012 ADD End Shirakawa 
                    ''20231013 DEL Start Shirakawa
                    ''この時点で数量を求め、更新するよう変更
                    ''sbSQL.AppendLine("  ,LOGIC_INV_NUM = ")     '帳簿在庫数 SUM(保管、貸出)
                    ''sbSQL.AppendLine(sbBookInvSuu.ToString)
                    ''sbSQL.AppendLine("  ,PHYINV_NUM    = ")     '実棚数 SUM(正常、警告)
                    ''sbSQL.AppendLine(sbActShelvesSuu.ToString)
                    ''sbSQL.AppendLine("  ,LOC_DIFF_NUM  = ")     'ロケ違い数 SUM(警告)
                    ''sbSQL.AppendLine(sbLocDiffSuu.ToString)
                    ''sbSQL.AppendLine("  ,NO_ITEM_NUM   = ")     '在庫なし数 SUM(現物なし(未処理を含む))
                    ''sbSQL.AppendLine(sbOutStockSuu.ToString)
                    ''20231013 DEL Start Shirakawa

                    '20231013 ADD Start Shirakawa

                    '棚卸明細T更新
                    '一覧で選択したロケーションの数ループ
                    For i As Integer = 0 To _invLocCd.Count - 1
                        strLoc_Cd = _invLocCd(i) '選択した各ロケーションを代入

                        params = New List(Of SqlParameter)

                        sbSQL.Clear()
                        sbSQL.AppendLine("UPDATE T_INV_DETAIL Set")
                        sbSQL.AppendLine("  LOGIC_INV_NUM = ")     '帳簿在庫数 SUM(保管、貸出)
                        sbSQL.AppendLine("       (SELECT COUNT_BIG(*) FROM T_INV_ASSET")
                        sbSQL.AppendLine("           WHERE T_INV_ASSET.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ")")
                        sbSQL.AppendLine($"       AND T_INV_ASSET.LOC_L_CD = '{strLoc_Cd}' ")  '20231026 ADD Shirakawa   大ロケ別に取得
                        sbSQL.AppendLine("       )")
                        sbSQL.AppendLine("  ,PHYINV_NUM    = ")     '実棚数 SUM(正常)
                        sbSQL.AppendLine("        (SELECT COUNT_BIG(*) FROM T_INV_ASSET")
                        sbSQL.AppendLine("            WHERE T_INV_ASSET.PHYINV_STATUS IN ('" & PHYINV_STATUS.Normal & "')")
                        sbSQL.AppendLine($"       AND T_INV_ASSET.LOC_L_CD = '{strLoc_Cd}' ")  '20231026 ADD Shirakawa   大ロケ別に取得
                        sbSQL.AppendLine("        )")
                        sbSQL.AppendLine("  ,LOC_DIFF_NUM  = ")     'ロケ違い数 SUM(警告)
                        sbSQL.AppendLine("          (SELECT COUNT_BIG(*) FROM T_INV_ASSET")
                        sbSQL.AppendLine("              WHERE T_INV_ASSET.PHYINV_STATUS = '" & PHYINV_STATUS.Warning & "'")
                        sbSQL.AppendLine($"       AND T_INV_ASSET.LOC_L_CD = '{strLoc_Cd}' ")  '20231026 ADD Shirakawa   大ロケ別に取得
                        sbSQL.AppendLine("          )")
                        sbSQL.AppendLine("  ,NO_ITEM_NUM   = ")     '在庫なし数 SUM(現物なし(未処理を含む))
                        sbSQL.AppendLine("          (SELECT COUNT_BIG(*) FROM T_INV_ASSET")
                        sbSQL.AppendLine("              WHERE T_INV_ASSET.PHYINV_STATUS = '" & PHYINV_STATUS.NoSpot & "'")
                        sbSQL.AppendLine($"       AND T_INV_ASSET.LOC_L_CD = '{strLoc_Cd}' ")  '20231026 ADD Shirakawa   大ロケ別に取得
                        sbSQL.AppendLine("          )")

                        sbSQL.AppendLine("  ,INV_ALL_CONFIRM_TS = FORMAT(GetDate(), 'yyyyMMddHHmmss')")         '棚卸完了日時
                        sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")
                        sbSQL.AppendLine(" WHERE INV_ID = @INV_ID")            '棚卸ID
                        sbSQL.AppendLine($" AND INV_LOC_L_CD = '{strLoc_Cd}'")      '棚卸大ロケコード

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))
                        params.Add(New SqlParameter($"@INV_ID", invId(i)))

                        ret = db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)


                        ' 棚卸管理更新
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)
                        sbSQL.AppendLine("UPDATE T_INVENTORY Set")
                        '20231027 DEL Start Shirakawa
                        'sbSQL.AppendLine("  ,START_DEC_TS = FORMAT(GetDate(), 'yyyyMMddHHmmss')")
                        'sbSQL.AppendLine("  ,START_DEC_EMPLOYEE_CD = @INV_CONFIRM_EMPLOYEE_CD")
                        'sbSQL.AppendLine("  ,START_DEC_NAME = @INV_CONFIRM_NAME")
                        'sbSQL.AppendLine($"   ,INV_NAME = '{CommonRfid.ToStringNvl(_ViewModel.INV_NAME).Trim}'")
                        '20231027 DEL End Shirakawa
                        sbSQL.AppendLine($"  INV_CONFIRM_TS = (SELECT FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                        sbSQL.AppendLine($" ,INV_ALL_CONFIRM_TS = (SELECT FORMAT(GetDate(), 'yyyyMMddHHmmss') WHERE NOT EXISTS(SELECT TOP 1 1 FROM T_INV_DETAIL WHERE T_INV_DETAIL.INV_ID = '{invId(i)}' AND INV_ALL_CONFIRM_TS IS NULL)) ")
                        sbSQL.AppendLine("  ,INV_CONFIRM_EMPLOYEE_CD = @INV_CONFIRM_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,INV_CONFIRM_NAME = @INV_CONFIRM_NAME")
                        sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")
                        'sbSQL.AppendLine("WHERE START_DEC_TS = @START_DEC_TS")
                        sbSQL.AppendLine("WHERE INV_ID = @INV_ID")

                        params.Add(New SqlParameter($"@INV_CONFIRM_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@INV_CONFIRM_NAME", _LoginInfo.EmployeeNm))

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))
                        params.Add(New SqlParameter($"@INV_ID", invId(i)))

                        ret = db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)


                        '20231013 ADD End Shirakawa
                        ' ②過去棚卸実績Tへ資産Mの実棚情報をコピー
                        params = New List(Of SqlParameter)

                        sbSQL.Clear()
                        sbSQL.AppendLine("INSERT INTO T_INV_RESULT")
                        sbSQL.AppendLine("(")
                        '20231013 ADD Start Shirakawa
                        '棚卸ID,棚卸ロケ大区分コード追加
                        sbSQL.AppendLine(" INV_ID")
                        sbSQL.AppendLine(",INV_LOC_L_CD")
                        '20231013 ADD End Shirakawa
                        sbSQL.AppendLine(",ASSET_NO")
                        sbSQL.AppendLine(",ASSET_ID")
                        sbSQL.AppendLine(",SUB_ASSET_NO")
                        sbSQL.AppendLine(",EPC_SIZE")
                        sbSQL.AppendLine(",GEN_ASSET_ID")
                        sbSQL.AppendLine(",ASSET_DIV")
                        sbSQL.AppendLine(",ASSET_NAME")
                        sbSQL.AppendLine(",LOC_L_CD")
                        sbSQL.AppendLine(",LOC_M_CD")
                        sbSQL.AppendLine(",LOC_S_CD")
                        sbSQL.AppendLine(",CAT_L_CD")
                        sbSQL.AppendLine(",CAT_M_CD")
                        sbSQL.AppendLine(",CAT_S_CD")
                        sbSQL.AppendLine(",MAKER")
                        sbSQL.AppendLine(",PRODUCT_NO")
                        sbSQL.AppendLine(",MODEL_NO")
                        sbSQL.AppendLine(",LEASE_NO")
                        sbSQL.AppendLine(",LEASE_LIMIT")
                        sbSQL.AppendLine(",GENERIC_LIMIT")
                        sbSQL.AppendLine(",FREE_ITEM1")
                        sbSQL.AppendLine(",FREE_ITEM2")
                        sbSQL.AppendLine(",FREE_ITEM3")
                        sbSQL.AppendLine(",FREE_ITEM4")
                        sbSQL.AppendLine(",FREE_ITEM5")
                        sbSQL.AppendLine(",FREE_ITEM6")
                        sbSQL.AppendLine(",FREE_ITEM7")
                        sbSQL.AppendLine(",FREE_ITEM8")
                        sbSQL.AppendLine(",FREE_ITEM9")
                        sbSQL.AppendLine(",FREE_ITEM10")
                        sbSQL.AppendLine(",FREE_ITEM11")
                        sbSQL.AppendLine(",FREE_ITEM12")
                        sbSQL.AppendLine(",FREE_ITEM13")
                        sbSQL.AppendLine(",FREE_ITEM14")
                        sbSQL.AppendLine(",FREE_ITEM15")
                        sbSQL.AppendLine(",FREE_ITEM16")
                        sbSQL.AppendLine(",FREE_ITEM17")
                        sbSQL.AppendLine(",FREE_ITEM18")
                        sbSQL.AppendLine(",FREE_ITEM19")
                        sbSQL.AppendLine(",FREE_ITEM20")
                        sbSQL.AppendLine(",FREE_ITEM21")
                        sbSQL.AppendLine(",FREE_ITEM22")
                        sbSQL.AppendLine(",FREE_ITEM23")
                        sbSQL.AppendLine(",FREE_ITEM24")
                        sbSQL.AppendLine(",FREE_ITEM25")
                        sbSQL.AppendLine(",FREE_ITEM26")
                        sbSQL.AppendLine(",FREE_ITEM27")
                        sbSQL.AppendLine(",FREE_ITEM28")
                        sbSQL.AppendLine(",FREE_ITEM29")
                        sbSQL.AppendLine(",FREE_ITEM30")
                        sbSQL.AppendLine(",FREE_ITEM31")
                        sbSQL.AppendLine(",FREE_ITEM32")
                        sbSQL.AppendLine(",FREE_ITEM33")
                        sbSQL.AppendLine(",FREE_ITEM34")
                        sbSQL.AppendLine(",FREE_ITEM35")
                        sbSQL.AppendLine(",FREE_ITEM36")
                        sbSQL.AppendLine(",FREE_ITEM37")
                        sbSQL.AppendLine(",FREE_ITEM38")
                        sbSQL.AppendLine(",FREE_ITEM39")
                        sbSQL.AppendLine(",FREE_ITEM40")
                        sbSQL.AppendLine(",FREE_ITEM41")
                        sbSQL.AppendLine(",FREE_ITEM42")
                        sbSQL.AppendLine(",FREE_ITEM43")
                        sbSQL.AppendLine(",FREE_ITEM44")
                        sbSQL.AppendLine(",FREE_ITEM45")
                        sbSQL.AppendLine(",FREE_ITEM46")
                        sbSQL.AppendLine(",FREE_ITEM47")
                        sbSQL.AppendLine(",FREE_ITEM48")
                        sbSQL.AppendLine(",FREE_ITEM49")
                        sbSQL.AppendLine(",FREE_ITEM50")
                        sbSQL.AppendLine(",FREE_ITEM51")
                        sbSQL.AppendLine(",FREE_ITEM52")
                        sbSQL.AppendLine(",FREE_ITEM53")
                        sbSQL.AppendLine(",FREE_ITEM54")
                        sbSQL.AppendLine(",FREE_ITEM55")
                        sbSQL.AppendLine(",FREE_ITEM56")
                        sbSQL.AppendLine(",FREE_ITEM57")
                        sbSQL.AppendLine(",FREE_ITEM58")
                        sbSQL.AppendLine(",FREE_ITEM59")
                        sbSQL.AppendLine(",FREE_ITEM60")
                        sbSQL.AppendLine(",ASSET_PIC_FILENAME")
                        sbSQL.AppendLine(",LEASE_CNT")
                        sbSQL.AppendLine(",ASSET_STATUS")
                        sbSQL.AppendLine(",EPC_ENC_STATUS")
                        sbSQL.AppendLine(",TAG_LINK_STATUS")
                        sbSQL.AppendLine(",RETIRE_TS")
                        sbSQL.AppendLine(",RETIRE_EMPLOYEE_CD")
                        sbSQL.AppendLine(",RETIRE_EMPLOYEE_NAME")
                        sbSQL.AppendLine(",RETIRE_ASSET_ID")
                        sbSQL.AppendLine(",LOC_L_NAME")
                        sbSQL.AppendLine(",LOC_M_NAME")
                        sbSQL.AppendLine(",LOC_S_NAME")
                        sbSQL.AppendLine(",CAT_L_NAME")
                        sbSQL.AppendLine(",CAT_M_NAME")
                        sbSQL.AppendLine(",CAT_S_NAME")
                        sbSQL.AppendLine(",PHYINV_TS")
                        sbSQL.AppendLine(",PHYINV_STATUS")
                        sbSQL.AppendLine(",PHYINV_LOC_L_CD")
                        sbSQL.AppendLine(",PHYINV_LOC_M_CD")
                        sbSQL.AppendLine(",PHYINV_LOC_S_CD")
                        sbSQL.AppendLine(",PHYINV_LOC_L_NAME")
                        sbSQL.AppendLine(",PHYINV_LOC_M_NAME")
                        sbSQL.AppendLine(",PHYINV_LOC_S_NAME")
                        sbSQL.AppendLine(",PHYINV_EMPLOYEE_CD")
                        sbSQL.AppendLine(",PHYINV_EMPLOYEE_NAME")


                        sbSQL.AppendLine(",OPE_STATUS")
                        sbSQL.AppendLine(",DIVERSION_REMOVE")
                        sbSQL.AppendLine(",REMOVE_DATE")
                        sbSQL.AppendLine(",REMOVE_REASON")
                        sbSQL.AppendLine(",REMOVE_CONTENT")
                        sbSQL.AppendLine(",CHANGE_LOC_M_CD")
                        sbSQL.AppendLine(",CHANGE_LOC_S_CD")
                        sbSQL.AppendLine(",CHANGE_EMPLOYEE")
                        sbSQL.AppendLine(",IS_TANGIBLE")
                        sbSQL.AppendLine(",REMARK1")
                        sbSQL.AppendLine(",REMARK2")
                        sbSQL.AppendLine(",REMARK3")
                        sbSQL.AppendLine(",REMARK4")
                        sbSQL.AppendLine(",REMARK5")
                        sbSQL.AppendLine(",REMARK6")
                        sbSQL.AppendLine(",REMARK7")
                        sbSQL.AppendLine(",REMARK8")
                        sbSQL.AppendLine(",REMARK9")
                        sbSQL.AppendLine(",REMARK10")
                        '[CC431] ADD Start 20261002
                        sbSQL.AppendLine(",PHYINV_TOOL_DIV")
                        sbSQL.AppendLine(",ENT_EMPLOYEE_CD")
                        '[CC431] ADD End 20261002

                        sbSQL.AppendLine(",UPD_DATETIME")
                        sbSQL.AppendLine(",UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine(",UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine(",UPD_TASK")
                        sbSQL.AppendLine(")")

                        sbSQL.AppendLine("SELECT ")
                        '20231013 ADD Start Shirakawa
                        '当該棚卸ID,当該大ロケコード追加
                        sbSQL.AppendLine(" MAST.INV_ID")
                        sbSQL.AppendLine(",MAST.INV_LOC_L_CD")
                        '20231013 ADD End Shirakawa
                        sbSQL.AppendLine(",MAST.ASSET_NO")
                        sbSQL.AppendLine(",MAST.ASSET_ID")
                        sbSQL.AppendLine(",MAST.SUB_ASSET_NO")
                        sbSQL.AppendLine(",MAST.EPC_SIZE")
                        sbSQL.AppendLine(",MAST.GEN_ASSET_ID")
                        sbSQL.AppendLine(",MAST.ASSET_DIV")
                        sbSQL.AppendLine(",MAST.ASSET_NAME")
                        sbSQL.AppendLine(",MAST.LOC_L_CD")
                        sbSQL.AppendLine(",MAST.LOC_M_CD")
                        sbSQL.AppendLine(",MAST.LOC_S_CD")
                        sbSQL.AppendLine(",MAST.CAT_L_CD")
                        sbSQL.AppendLine(",MAST.CAT_M_CD")
                        sbSQL.AppendLine(",MAST.CAT_S_CD")
                        sbSQL.AppendLine(",MAST.MAKER")
                        sbSQL.AppendLine(",MAST.PRODUCT_NO")
                        sbSQL.AppendLine(",MAST.MODEL_NO")
                        sbSQL.AppendLine(",MAST.LEASE_NO")
                        sbSQL.AppendLine(",MAST.LEASE_LIMIT")
                        sbSQL.AppendLine(",MAST.GENERIC_LIMIT")
                        sbSQL.AppendLine(",MAST.FREE_ITEM1")
                        sbSQL.AppendLine(",MAST.FREE_ITEM2")
                        sbSQL.AppendLine(",MAST.FREE_ITEM3")
                        sbSQL.AppendLine(",MAST.FREE_ITEM4")
                        sbSQL.AppendLine(",MAST.FREE_ITEM5")
                        sbSQL.AppendLine(",MAST.FREE_ITEM6")
                        sbSQL.AppendLine(",MAST.FREE_ITEM7")
                        sbSQL.AppendLine(",MAST.FREE_ITEM8")
                        sbSQL.AppendLine(",MAST.FREE_ITEM9")
                        sbSQL.AppendLine(",MAST.FREE_ITEM10")
                        sbSQL.AppendLine(",MAST.FREE_ITEM11")
                        sbSQL.AppendLine(",MAST.FREE_ITEM12")
                        sbSQL.AppendLine(",MAST.FREE_ITEM13")
                        sbSQL.AppendLine(",MAST.FREE_ITEM14")
                        sbSQL.AppendLine(",MAST.FREE_ITEM15")
                        sbSQL.AppendLine(",MAST.FREE_ITEM16")
                        sbSQL.AppendLine(",MAST.FREE_ITEM17")
                        sbSQL.AppendLine(",MAST.FREE_ITEM18")
                        sbSQL.AppendLine(",MAST.FREE_ITEM19")
                        sbSQL.AppendLine(",MAST.FREE_ITEM20")
                        sbSQL.AppendLine(",MAST.FREE_ITEM21")
                        sbSQL.AppendLine(",MAST.FREE_ITEM22")
                        sbSQL.AppendLine(",MAST.FREE_ITEM23")
                        sbSQL.AppendLine(",MAST.FREE_ITEM24")
                        sbSQL.AppendLine(",MAST.FREE_ITEM25")
                        sbSQL.AppendLine(",MAST.FREE_ITEM26")
                        sbSQL.AppendLine(",MAST.FREE_ITEM27")
                        sbSQL.AppendLine(",MAST.FREE_ITEM28")
                        sbSQL.AppendLine(",MAST.FREE_ITEM29")
                        sbSQL.AppendLine(",MAST.FREE_ITEM30")
                        sbSQL.AppendLine(",MAST.FREE_ITEM31")
                        sbSQL.AppendLine(",MAST.FREE_ITEM32")
                        sbSQL.AppendLine(",MAST.FREE_ITEM33")
                        sbSQL.AppendLine(",MAST.FREE_ITEM34")
                        sbSQL.AppendLine(",MAST.FREE_ITEM35")
                        sbSQL.AppendLine(",MAST.FREE_ITEM36")
                        sbSQL.AppendLine(",MAST.FREE_ITEM37")
                        sbSQL.AppendLine(",MAST.FREE_ITEM38")
                        sbSQL.AppendLine(",MAST.FREE_ITEM39")
                        sbSQL.AppendLine(",MAST.FREE_ITEM40")
                        sbSQL.AppendLine(",MAST.FREE_ITEM41")
                        sbSQL.AppendLine(",MAST.FREE_ITEM42")
                        sbSQL.AppendLine(",MAST.FREE_ITEM43")
                        sbSQL.AppendLine(",MAST.FREE_ITEM44")
                        sbSQL.AppendLine(",MAST.FREE_ITEM45")
                        sbSQL.AppendLine(",MAST.FREE_ITEM46")
                        sbSQL.AppendLine(",MAST.FREE_ITEM47")
                        sbSQL.AppendLine(",MAST.FREE_ITEM48")
                        sbSQL.AppendLine(",MAST.FREE_ITEM49")
                        sbSQL.AppendLine(",MAST.FREE_ITEM50")
                        sbSQL.AppendLine(",MAST.FREE_ITEM51")
                        sbSQL.AppendLine(",MAST.FREE_ITEM52")
                        sbSQL.AppendLine(",MAST.FREE_ITEM53")
                        sbSQL.AppendLine(",MAST.FREE_ITEM54")
                        sbSQL.AppendLine(",MAST.FREE_ITEM55")
                        sbSQL.AppendLine(",MAST.FREE_ITEM56")
                        sbSQL.AppendLine(",MAST.FREE_ITEM57")
                        sbSQL.AppendLine(",MAST.FREE_ITEM58")
                        sbSQL.AppendLine(",MAST.FREE_ITEM59")
                        sbSQL.AppendLine(",MAST.FREE_ITEM60")
                        sbSQL.AppendLine(",MAST.ASSET_PIC_FILENAME")
                        sbSQL.AppendLine(",MAST.LEASE_CNT")
                        sbSQL.AppendLine(",MAST.ASSET_STATUS")
                        sbSQL.AppendLine(",MAST.EPC_ENC_STATUS")
                        sbSQL.AppendLine(",MAST.TAG_LINK_STATUS")
                        sbSQL.AppendLine(",MAST.RETIRE_TS")
                        sbSQL.AppendLine(",MAST.RETIRE_EMPLOYEE_CD")
                        sbSQL.AppendLine(",MAST.RETIRE_EMPLOYEE_NAME")
                        sbSQL.AppendLine(",MAST.RETIRE_ASSET_ID")
                        sbSQL.AppendLine(",LOC_L.LOC_NAME")
                        sbSQL.AppendLine(",LOC_M.LOC_NAME")
                        sbSQL.AppendLine(",LOC_S.LOC_NAME")
                        sbSQL.AppendLine(",CAT_L.CAT_NAME")
                        sbSQL.AppendLine(",CAT_M.CAT_NAME")
                        sbSQL.AppendLine(",CAT_S.CAT_NAME")
                        sbSQL.AppendLine(",MAST.PHYINV_TS")
                        sbSQL.AppendLine(",MAST.PHYINV_STATUS")
                        sbSQL.AppendLine(",MAST.PHYINV_LOC_L_CD")
                        sbSQL.AppendLine(",MAST.PHYINV_LOC_M_CD")
                        sbSQL.AppendLine(",MAST.PHYINV_LOC_S_CD")
                        sbSQL.AppendLine(",MAST.PHYINV_LOC_L_NAME")
                        sbSQL.AppendLine(",MAST.PHYINV_LOC_M_NAME")
                        sbSQL.AppendLine(",MAST.PHYINV_LOC_S_NAME")
                        sbSQL.AppendLine(",MAST.PHYINV_EMPLOYEE_CD")
                        sbSQL.AppendLine(",MAST.PHYINV_EMPLOYEE_NAME")

                        sbSQL.AppendLine(",MAST.OPE_STATUS")
                        sbSQL.AppendLine(",MAST.DIVERSION_REMOVE")
                        sbSQL.AppendLine(",MAST.REMOVE_DATE")
                        sbSQL.AppendLine(",MAST.REMOVE_REASON")
                        sbSQL.AppendLine(",MAST.REMOVE_CONTENT")
                        sbSQL.AppendLine(",MAST.CHANGE_LOC_M_CD")
                        sbSQL.AppendLine(",MAST.CHANGE_LOC_S_CD")
                        sbSQL.AppendLine(",MAST.CHANGE_EMPLOYEE")
                        sbSQL.AppendLine(",MAST.IS_TANGIBLE")
                        sbSQL.AppendLine(",MAST.REMARK1")
                        sbSQL.AppendLine(",MAST.REMARK2")
                        sbSQL.AppendLine(",MAST.REMARK3")
                        sbSQL.AppendLine(",MAST.REMARK4")
                        sbSQL.AppendLine(",MAST.REMARK5")
                        sbSQL.AppendLine(",MAST.REMARK6")
                        sbSQL.AppendLine(",MAST.REMARK7")
                        sbSQL.AppendLine(",MAST.REMARK8")
                        sbSQL.AppendLine(",MAST.REMARK9")
                        sbSQL.AppendLine(",MAST.REMARK10")
                        '[CC431] ADD Start 20261002
                        sbSQL.AppendLine(",MAST.PHYINV_TOOL_DIV")
                        sbSQL.AppendLine(",MAST.ENT_EMPLOYEE_CD")
                        '[CC431] ADD End 20261002


                        sbSQL.AppendLine(", " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine(",@UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine(",@UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine(",@UPD_TASK")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", Me._LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", Me._LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

                        sbSQL.AppendLine("FROM T_INV_ASSET MAST")
                        sbSQL.AppendLine("LEFT JOIN M_LOCATION LOC_L")
                        sbSQL.AppendLine("ON")
                        sbSQL.AppendLine("LOC_L.LOC_DIV = '" & LOC_DIV.LocationL & "'")
                        sbSQL.AppendLine("AND LOC_L.LOC_L_CD = MAST.LOC_L_CD")
                        sbSQL.AppendLine("LEFT JOIN M_LOCATION LOC_M")
                        sbSQL.AppendLine("ON")
                        sbSQL.AppendLine("LOC_M.LOC_DIV = '" & LOC_DIV.LocationM & "'")
                        sbSQL.AppendLine("AND LOC_M.LOC_L_CD = MAST.LOC_L_CD")
                        sbSQL.AppendLine("AND LOC_M.LOC_M_CD = MAST.LOC_M_CD")
                        sbSQL.AppendLine("LEFT JOIN M_LOCATION LOC_S")
                        sbSQL.AppendLine("ON")
                        sbSQL.AppendLine("LOC_S.LOC_DIV = '" & LOC_DIV.LocationS & "'")
                        sbSQL.AppendLine("AND LOC_S.LOC_L_CD = MAST.LOC_L_CD")
                        sbSQL.AppendLine("AND LOC_S.LOC_M_CD = MAST.LOC_M_CD")
                        sbSQL.AppendLine("AND LOC_S.LOC_S_CD = MAST.LOC_S_CD")
                        sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_L")
                        sbSQL.AppendLine("ON")
                        sbSQL.AppendLine("CAT_L.CAT_DIV = '" & CAT_DIV.CategoryL & "'")
                        sbSQL.AppendLine("AND CAT_L.CAT_L_CD = MAST.CAT_L_CD")
                        sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_M")
                        sbSQL.AppendLine("ON")
                        sbSQL.AppendLine("CAT_M.CAT_DIV = '" & CAT_DIV.CategoryM & "'")
                        sbSQL.AppendLine("AND CAT_M.CAT_L_CD = MAST.CAT_L_CD")
                        sbSQL.AppendLine("AND CAT_M.CAT_M_CD = MAST.CAT_M_CD")
                        sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_S")
                        sbSQL.AppendLine("ON")
                        sbSQL.AppendLine("CAT_S.CAT_DIV = '" & CAT_DIV.CategoryS & "'")
                        sbSQL.AppendLine("AND CAT_S.CAT_L_CD = MAST.CAT_L_CD")
                        sbSQL.AppendLine("AND CAT_S.CAT_M_CD = MAST.CAT_M_CD")
                        sbSQL.AppendLine("AND CAT_S.CAT_S_CD = MAST.CAT_S_CD")

                        sbSQL.AppendLine($"WHERE MAST.INV_ID = '{invId(i)}'")               '棚卸ID
                        sbSQL.AppendLine($"AND MAST.INV_LOC_L_CD = '{strLoc_Cd}'")      '大ロケコード

                        ret = db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                        '[CC431] ADD Start 20261002
                        '[CC431] ADD Start 20261008
                        '資産M.備考1～10と棚卸資産T.備考1～10のいずれかに変更がある資産のみ対象にする条件
                        '(NULLと空文字は同じとみなす。スペースの有無・大文字小文字・全角半角は違いとする)
                        '(資産Mを更新すると変更がなくなるため、資産処理履歴Tの登録→資産Mの更新の順で行う)
                        Dim sbRemarkDiff As New StringBuilder
                        For n As Integer = 1 To 10
                            sbRemarkDiff.AppendLine($"{If(n = 1, "AND (", " OR ")}ISNULL(INV.REMARK{n}, N'') COLLATE Japanese_BIN2 <> ISNULL(ASSET.REMARK{n}, N'') COLLATE Japanese_BIN2")
                            sbRemarkDiff.AppendLine($" OR DATALENGTH(ISNULL(INV.REMARK{n}, N'')) <> DATALENGTH(ISNULL(ASSET.REMARK{n}, N''))")
                        Next
                        sbRemarkDiff.AppendLine(")")
                        '[CC431] ADD End 20261008

                        '[CC431] ADD Start 20261007
                        '[CC431] UPD Start 20261008
                        '資産処理履歴T登録(棚卸資産Tにある資産のうち備考1～10に変更がある資産ごとに1レコード。備考1～10は棚卸資産T(＝更新後の資産M)、最終登録更新関連以外は資産Mからセット)
                        '[CC431] UPD End 20261008
                        params = New List(Of SqlParameter)

                        sbSQL.Clear()
                        sbSQL.AppendLine("INSERT INTO T_PROCESS_LOG")
                        sbSQL.AppendLine("(")
                        sbSQL.AppendLine("   PROCESS_DT")
                        sbSQL.AppendLine("  ,PROCESS_TM")
                        sbSQL.AppendLine("  ,ASSET_NO")
                        sbSQL.AppendLine("  ,ASSET_ID")
                        sbSQL.AppendLine("  ,PROCESS_TOOL_DIV")
                        sbSQL.AppendLine("  ,PROCESS_DIV")
                        sbSQL.AppendLine("  ,LEASE_PLACE_CD")
                        sbSQL.AppendLine("  ,LEASE_PLACE_NAME")
                        sbSQL.AppendLine("  ,REMARK")
                        sbSQL.AppendLine("  ,LEASE_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,LEASE_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,LEASE_DT")
                        sbSQL.AppendLine("  ,LEASE_DUE_DT")
                        sbSQL.AppendLine("  ,LOC_L_CD")
                        sbSQL.AppendLine("  ,LOC_M_CD")
                        sbSQL.AppendLine("  ,LOC_S_CD")
                        sbSQL.AppendLine("  ,LOC_L_NAME")
                        sbSQL.AppendLine("  ,LOC_M_NAME")
                        sbSQL.AppendLine("  ,LOC_S_NAME")
                        sbSQL.AppendLine("  ,ASSET_NAME")
                        sbSQL.AppendLine("  ,MAKER")
                        sbSQL.AppendLine("  ,PRODUCT_NO")
                        sbSQL.AppendLine("  ,MODEL_NO")
                        sbSQL.AppendLine("  ,MNG_LOC_L_CD")
                        sbSQL.AppendLine("  ,LEASE_STATUS")
                        sbSQL.AppendLine("  ,CAT_L_CD")
                        sbSQL.AppendLine("  ,CAT_NAME")
                        sbSQL.AppendLine("  ,SUB_ASSET_NO")
                        sbSQL.AppendLine("  ,TASK_RESULT")
                        sbSQL.AppendLine("  ,REMARK1")
                        sbSQL.AppendLine("  ,REMARK2")
                        sbSQL.AppendLine("  ,REMARK3")
                        sbSQL.AppendLine("  ,REMARK4")
                        sbSQL.AppendLine("  ,REMARK5")
                        sbSQL.AppendLine("  ,REMARK6")
                        sbSQL.AppendLine("  ,REMARK7")
                        sbSQL.AppendLine("  ,REMARK8")
                        sbSQL.AppendLine("  ,REMARK9")
                        sbSQL.AppendLine("  ,REMARK10")
                        sbSQL.AppendLine("  ,UPD_DATETIME")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,UPD_TASK")
                        sbSQL.AppendLine(")")
                        sbSQL.AppendLine("SELECT")
                        sbSQL.AppendLine("   CONVERT(int, FORMAT(GetDate(), 'yyyyMMdd'))")
                        sbSQL.AppendLine("  ,FORMAT(GetDate(), 'HHmmssfff')")
                        sbSQL.AppendLine("  ,ASSET.ASSET_NO")
                        sbSQL.AppendLine("  ,ASSET.ASSET_ID")
                        sbSQL.AppendLine("  ,'" & Misc.ProcessToolDiv.Web & "'")
                        sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.IC & "'")
                        sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_CD")
                        sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_NAME")
                        sbSQL.AppendLine("  ,ASSET.REMARK")
                        sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,ASSET.LEASE_DT")
                        sbSQL.AppendLine("  ,ASSET.LEASE_DUE_DT")
                        sbSQL.AppendLine("  ,ASSET.LOC_L_CD")
                        sbSQL.AppendLine("  ,ASSET.LOC_M_CD")
                        sbSQL.AppendLine("  ,ASSET.LOC_S_CD")
                        sbSQL.AppendLine("  ,LOCL.LOC_NAME")
                        sbSQL.AppendLine("  ,LOCM.LOC_NAME")
                        sbSQL.AppendLine("  ,LOCS.LOC_NAME")
                        sbSQL.AppendLine("  ,ASSET.ASSET_NAME")
                        sbSQL.AppendLine("  ,ASSET.MAKER")
                        sbSQL.AppendLine("  ,ASSET.PRODUCT_NO")
                        sbSQL.AppendLine("  ,ASSET.MODEL_NO")
                        sbSQL.AppendLine("  ,ASSET.MNG_LOC_L_CD")
                        sbSQL.AppendLine("  ,ASSET.LEASE_STATUS")
                        sbSQL.AppendLine("  ,ASSET.CAT_L_CD")
                        sbSQL.AppendLine("  ,CATL.CAT_NAME")
                        sbSQL.AppendLine("  ,ASSET.SUB_ASSET_NO")
                        sbSQL.AppendLine("  ,'" & Misc.TaskResult.OK & "'")
                        '[CC431] UPD Start 20261008
                        sbSQL.AppendLine("  ,INV.REMARK1")
                        sbSQL.AppendLine("  ,INV.REMARK2")
                        sbSQL.AppendLine("  ,INV.REMARK3")
                        sbSQL.AppendLine("  ,INV.REMARK4")
                        sbSQL.AppendLine("  ,INV.REMARK5")
                        sbSQL.AppendLine("  ,INV.REMARK6")
                        sbSQL.AppendLine("  ,INV.REMARK7")
                        sbSQL.AppendLine("  ,INV.REMARK8")
                        sbSQL.AppendLine("  ,INV.REMARK9")
                        sbSQL.AppendLine("  ,INV.REMARK10")
                        '[CC431] UPD End 20261008
                        sbSQL.AppendLine("  ," & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("  ,@UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,@UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,@UPD_TASK")
                        sbSQL.AppendLine("FROM M_ASSET ASSET")
                        sbSQL.AppendLine("INNER JOIN T_INV_ASSET INV")
                        sbSQL.AppendLine("ON INV.ASSET_NO = ASSET.ASSET_NO")
                        sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL")
                        sbSQL.AppendLine("ON LOCL.LOC_DIV = '" & LOC_DIV.LocationL & "'")
                        sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
                        sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM")
                        sbSQL.AppendLine("ON LOCM.LOC_DIV = '" & LOC_DIV.LocationM & "'")
                        sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCM.LOC_L_CD")
                        sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
                        sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS")
                        sbSQL.AppendLine("ON LOCS.LOC_DIV = '" & LOC_DIV.LocationS & "'")
                        sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCS.LOC_L_CD")
                        sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCS.LOC_M_CD")
                        sbSQL.AppendLine("AND ASSET.LOC_S_CD = LOCS.LOC_S_CD")
                        sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATL")
                        sbSQL.AppendLine("ON CATL.CAT_DIV = '" & CAT_DIV.CategoryL & "'")
                        sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATL.CAT_L_CD")
                        sbSQL.AppendLine("WHERE INV.INV_ID = @INV_ID")                  '棚卸ID
                        sbSQL.AppendLine("AND INV.INV_LOC_L_CD = @INV_LOC_L_CD")        '大ロケコード
                        '[CC431] ADD Start 20261008
                        sbSQL.Append(sbRemarkDiff.ToString())                           '備考1～10に変更あり
                        '[CC431] ADD End 20261008

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))
                        params.Add(New SqlParameter($"@INV_ID", invId(i)))
                        params.Add(New SqlParameter($"@INV_LOC_L_CD", strLoc_Cd))

                        ret = db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)
                        '[CC431] ADD End 20261007

                        '[CC431] UPD Start 20261008
                        '資産M.備考1～10を棚卸資産T.備考1～10で更新(備考1～10に変更がある資産のみ。)
                        params = New List(Of SqlParameter)

                        sbSQL.Clear()
                        sbSQL.AppendLine("UPDATE ASSET SET")
                        sbSQL.AppendLine("  REMARK1 = INV.REMARK1")
                        sbSQL.AppendLine(" ,REMARK2 = INV.REMARK2")
                        sbSQL.AppendLine(" ,REMARK3 = INV.REMARK3")
                        sbSQL.AppendLine(" ,REMARK4 = INV.REMARK4")
                        sbSQL.AppendLine(" ,REMARK5 = INV.REMARK5")
                        sbSQL.AppendLine(" ,REMARK6 = INV.REMARK6")
                        sbSQL.AppendLine(" ,REMARK7 = INV.REMARK7")
                        sbSQL.AppendLine(" ,REMARK8 = INV.REMARK8")
                        sbSQL.AppendLine(" ,REMARK9 = INV.REMARK9")
                        sbSQL.AppendLine(" ,REMARK10 = INV.REMARK10")
                        sbSQL.AppendLine(" ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine(" ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine(" ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine(" ,UPD_TASK = @UPD_TASK")
                        sbSQL.AppendLine("FROM M_ASSET ASSET")
                        sbSQL.AppendLine("INNER JOIN T_INV_ASSET INV")
                        sbSQL.AppendLine("ON INV.ASSET_NO = ASSET.ASSET_NO")
                        sbSQL.AppendLine("WHERE INV.INV_ID = @INV_ID")                  '棚卸ID
                        sbSQL.AppendLine("AND INV.INV_LOC_L_CD = @INV_LOC_L_CD")        '大ロケコード
                        sbSQL.Append(sbRemarkDiff.ToString())                           '備考1～10に変更あり

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))
                        params.Add(New SqlParameter($"@INV_ID", invId(i)))
                        params.Add(New SqlParameter($"@INV_LOC_L_CD", strLoc_Cd))

                        ret = db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)
                        '[CC431] UPD End 20261008
                        '[CC431] ADD End 20261002

                        '棚卸資産T削除
                        sbSQL.Clear()
                        sbSQL.AppendLine("DELETE FROM T_INV_ASSET")
                        sbSQL.AppendLine($"WHERE INV_ID = '{invId(i)}'")
                        db.Database.ExecuteSqlCommand(sbSQL.ToString)


                        '' ③資産M.実棚情報をクリア
                        'params = New List(Of SqlParameter)
                        'sbSQL.Clear()
                        'sbSQL.AppendLine("UPDATE M_ASSET")
                        'sbSQL.AppendLine("SET")
                        'sbSQL.AppendLine(" PHYINV_TS = NULL")
                        'sbSQL.AppendLine(",PHYINV_STATUS = '" & PHYINV_STATUS.Excluded & "'")
                        'sbSQL.AppendLine(",PHYINV_LOC_L_CD = ''")
                        'sbSQL.AppendLine(",PHYINV_LOC_M_CD = ''")
                        'sbSQL.AppendLine(",PHYINV_LOC_S_CD = ''")
                        'sbSQL.AppendLine(",PHYINV_LOC_L_NAME = NULL")
                        'sbSQL.AppendLine(",PHYINV_LOC_M_NAME = NULL")
                        'sbSQL.AppendLine(",PHYINV_LOC_S_NAME = NULL")
                        'sbSQL.AppendLine(",PHYINV_EMPLOYEE_CD = NULL")
                        'sbSQL.AppendLine(",PHYINV_EMPLOYEE_NAME = NULL")

                        'sbSQL.AppendLine(",UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        'sbSQL.AppendLine(",UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        'sbSQL.AppendLine(",UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        'sbSQL.AppendLine(",UPD_TASK = @UPD_TASK")

                        'params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        'params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        'params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

                        ''準備中・除却は登録しない
                        'sbSQL.AppendLine("WHERE ASSET_STATUS IN ('" & ASSET_STATUS.Storage & "','" & ASSET_STATUS.Lending & "')")
                        'sbSQL.AppendLine($" AND LOC_L_CD = '{strLoc_Cd}'")                      'ロケ大コード


                        'ret = db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)
                    Next

                    tran.Commit()

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)
                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                End Try
            End Using

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                '操作ログ出力
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_INV_KAKUTEI)
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE))

                _ViewModel.States = StatusType.SuccessInvKakutei
            End If

            'Ver2.0 既存不具合対応
            DisposeCreate()

            'セッションにinvIdを追加
            Session(CommonSessionKey.INV_ID) = invId

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
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
        Public Function Create(ByVal ViewModel As InventorySummaryViewModel, ByVal OnCreteButton As String, ByVal SelectKeys As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '202231026 ADD Start Shirakawa
            Dim selectKey() As String = SelectKeys.Split(",")
            Session(CommonSessionKey.LIST_CHECKED_KEYS) = selectKey.ToList

            'ロケコードを分離させ配列に格納
            For i = 0 To selectKey.Count - 1
                _invLocCd.Add(selectKey(i).Split("/")(1))
            Next


            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理
                MyBase.BaseCreate(_ViewModel)
                If _ViewModel.ErrCode = "" Then
                    RegistInventory()
                End If
            Else
                '更新処理
                MyBase.BaseEdit(_ViewModel)
            End If

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_INV_START) & " " &
                                                     Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TANAOROSHI_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.INV_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE))

                    _ViewModel.States = StatusType.SuccessCreate
                Else
                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_INV_START) & " " &
                                                     Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TANAOROSHI_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.INV_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE))

                    _ViewModel.States = StatusType.SuccessEdit
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
            'モデルを登録用モデルにコピー
            'CopyViewModeltoModel(_ViewModel)
        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeCreate()
            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_INVENTORY_SUMMARY) = _ViewModel
        End Sub

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
            If IsNothing(_AuthInfo) Then
                '権限情報の取得
                _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetHistory, _LoginInfo)
            End If

            Dim tbl As DataTable = Dao.ToolLocation.GetLocationLData(_LoginInfo, _AuthInfo)
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
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationMData(selectLCode)
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.modelLocM = New SelectList(lstLocation, "LOC_CD", "LOC_NAME")
            _ViewModel.locLDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "(ロケーション)小区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCode As String) As JsonResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationSData(selectLCode, selectMCode)
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.modelLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME")
            _ViewModel.locLDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "システム設定　CSVファイル名取得処理"
        ''' <summary>
        ''' システム設定　CSVファイル名取得処理
        ''' </summary>
        Public Function GetCsvOutputFileNam() As String

            Dim lstSetting As List(Of SettingModel)     '設定リスト
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext
            'M_SETTINGテーブルから取得するSQLを作成
            sbSQL.Append("SELECT *")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_SETTING ")
            sbSQL.Append("WHERE ")
            sbSQL.Append("  1 = 1 ")

            '検索条件を設定する
            'KEY種別
            sbSQL.Append("  AND KEY_DIV = '" + CommonSettingKey.InventorySummary.KeyDiv + "'")

            'KEY
            sbSQL.Append("  AND KEY1 = '" + CommonSettingKey.InventorySummary.Key1 + "'")
            sbSQL.Append("  AND KEY2 = '" + CommonSettingKey.InventorySummary.Key2 + "'")
            sbSQL.Append("  AND KEY3 = '" + CommonSettingKey.InventorySummary.Key3 + "'")

            'SQLを実行する
            lstSetting = db.Setting.SqlQuery(sbSQL.ToString).ToList

            Return lstSetting(0).DATA1.ToString
        End Function
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ByVal ViewModel As InventorySummaryViewModel) As ActionResult
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
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Inventory_Overview.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Overview.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Overview.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Inventory_Overview.Key3)
            '20230901 ADD End Shirakawa

            _ViewModel = ViewModel

            Dim InventorySummaryData As List(Of InventorySummaryViewModel)

            '棚卸概況取得
            InventorySummaryData = GetInventorySummaryData()

            ''20230901 ADD Start Shirakawa
            ''設定マスタから取得できた場合
            ''設定マスタから取得できた場合 かつ 上限値(Data2)が存在している場合 かつ 上限を超えていた場合
            'If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA2) = False AndAlso WebReadData.DATA2 < InventorySummaryData.Count Then
            '    '上限を超えていた場合　CSV出力しない

            '    '上限確認フラグ
            '    ViewBag.isLimitFlg = True
            '    '一覧データ取得件数
            '    ViewBag.cntList = InventorySummaryData.Count
            '    '設定マスタ上限件数　初期化
            '    ViewBag.cntLimit = WebReadData.DATA2

            '    Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'End If

            ''20230901 ADD End Shirakawa

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.InventorySummary.KeyDiv, CommonSettingKey.InventorySummary.Key1, CommonSettingKey.InventorySummary.Key2, CommonSettingKey.InventorySummary.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_INVENTORY_SUMMARY)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_INVENTORY_SUMMARY)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))

            'データ行挿入
            Dim col As String = ""
            For Each row As InventorySummaryViewModel In InventorySummaryData
                col = ""
                col += SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_NAME)
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_CD.Trim)
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_NAME)
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_CD.Trim)
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_NAME)
                '20231031 ADD Start Shirakawa
                '棚卸ステータスがある場合
                If row.INVENTORY_STATUS.ToString <> "-" Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.INVENTORY_STATUS)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If

                '棚卸開始時間がある場合
                If row.INVENTORY_STARTDAY.ToString <> "0" Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringLongYMDHM(row.INVENTORY_STARTDAY))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                '20231031 ADD End Shirakawa
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.BOOK_INVENTORY_NUMBER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.NUMBER_OF_LOANS_WITHIN)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ACTUAL_NUMBER_OF_SHELVES)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.NORMAL_NUMBER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOCATION_DIFFERENCE_NUMBER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.OUT_OF_STOCK)

                '2020/03/16 ADD miyaji 直近棚卸日　の出力を追加
                If row.NEAR_INVENTORY_DATE.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CDate(row.NEAR_INVENTORY_DATE).ToString("yyyy/MM/dd"))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     InventorySummaryData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetInventorySummaryData() As List(Of InventorySummaryViewModel)
            Dim lstInventorySummaryList As List(Of InventorySummaryViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventorySummary, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim sbNIDate As StringBuilder = New StringBuilder

            sbNIDate.AppendLine("   (")
            sbNIDate.AppendLine("     SELECT CASE WHEN PHYINV_TS IS NULL THEN NULL ELSE CONVERT(DATETIME, LEFT (PHYINV_TS, 8), 112) END NEAR_INVENTORY_DATE ")
            sbNIDate.AppendLine("     FROM (")
            sbNIDate.AppendLine("       SELECT ISNULL( ")
            sbNIDate.AppendLine("         ( ")
            sbNIDate.AppendLine("           SELECT MAX(PHYINV_TS) FROM M_ASSET WHERE M_ASSET.LOC_L_CD = MLCL.LOC_L_CD ")
            sbNIDate.AppendLine("         ), ")
            sbNIDate.AppendLine("         ( ")
            sbNIDate.AppendLine("           SELECT MAX(DET.INV_ALL_CONFIRM_TS) FROM T_INV_DETAIL DET INNER JOIN T_INVENTORY INV ON INV.INV_ID = DET.INV_ID WHERE DET.INV_LOC_L_CD = MLCL.LOC_L_CD ")
            sbNIDate.AppendLine("         ) ")
            sbNIDate.AppendLine("       ) PHYINV_TS")
            sbNIDate.AppendLine("     ) PHYINV ")
            sbNIDate.AppendLine("   ) NEAR_INVENTORY_DATE")

            '一覧表示表用SQL
            sbSQL.Clear()

            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("   CASE WHEN INV.INV_LOC_L_CD IS NOT NULL THEN INV.INV_ID ELSE '' END AS INV_ID")
            sbSQL.AppendLine("  ,ISNULL(MLCL.LOC_L_CD, '') LOC_L_CD")
            sbSQL.AppendLine("  ,ISNULL(MLCL.LOC_NAME, '') LOC_L_NAME")
            sbSQL.AppendLine("  ,ia.UPD_FLG")
            sbSQL.AppendLine("  ,ISNULL(INV_NAME, '') INV_NAME_LIST ")
            '-- 明細部　棚卸状態
            sbSQL.AppendLine($"  , CASE WHEN INV.INV_LOC_L_CD IS NOT NULL THEN '{Me._Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STARTED)}' ELSE '-' END AS INVENTORY_STATUS")
            '-- 明細部　棚卸開始日
            sbSQL.AppendLine("  , CASE WHEN INV.INV_LOC_L_CD IS NOT NULL THEN INV.START_DEC_TS ELSE '-' END AS INVENTORY_STARTDAY")
            sbSQL.AppendLine("  , CONVERT(BIGINT,BOOK_INVENTORY_NUMBER) AS BOOK_INVENTORY_NUMBER")
            sbSQL.AppendLine("  , CONVERT(BIGINT,NUMBER_OF_LOANS_WITHIN) AS NUMBER_OF_LOANS_WITHIN")
            sbSQL.AppendLine("  , CONVERT(BIGINT,ACTUAL_NUMBER_OF_SHELVES) AS ACTUAL_NUMBER_OF_SHELVES")
            sbSQL.AppendLine("  , CONVERT(BIGINT,NORMAL_NUMBER) AS NORMAL_NUMBER")
            sbSQL.AppendLine("  , CONVERT(BIGINT,LOCATION_DIFFERENCE_NUMBER) AS LOCATION_DIFFERENCE_NUMBER")
            sbSQL.AppendLine("  , CONVERT(BIGINT,OUT_OF_STOCK) AS OUT_OF_STOCK")
            sbSQL.AppendLine("  ," & sbNIDate.ToString)
            sbSQL.AppendLine("  ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM M_LOCATION MLCL")
            sbSQL.AppendLine("LEFT JOIN (SELECT DISTINCT T_INVENTORY.* , T_INV_DETAIL.INV_LOC_L_CD FROM T_INV_DETAIL LEFT JOIN T_INVENTORY ON T_INVENTORY.INV_ID = T_INV_DETAIL.INV_ID WHERE T_INV_DETAIL.INV_ALL_CONFIRM_TS IS NULL) AS INV")
            sbSQL.AppendLine("ON  INV.INV_LOC_L_CD = MLCL.LOC_L_CD ")
            sbSQL.AppendLine("LEFT JOIN (")
            sbSQL.AppendLine("  SELECT")
            sbSQL.AppendLine("  ia.INV_ID,ia.LOC_L_CD,AUTH.UPD_FLG")
            '-- 明細部　帳簿在庫数 SUM(保管、貸出)
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN 1")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) BOOK_INVENTORY_NUMBER")
            '-- 明細部　内貸出数 SUM(貸出)
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS = '{ASSET_STATUS.Lending}' THEN 1")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) NUMBER_OF_LOANS_WITHIN")
            '-- 明細部　実棚数 SUM(正常、警告)
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sbSQL.AppendLine($"     CASE WHEN PHYINV_STATUS IN ('{PHYINV_STATUS.Normal}','{PHYINV_STATUS.Warning}') THEN 1 ELSE 0 END")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) ACTUAL_NUMBER_OF_SHELVES")
            '-- 明細部　正常数 SUM(正常)
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sbSQL.AppendLine($"     CASE WHEN PHYINV_STATUS = '{PHYINV_STATUS.Normal}' THEN 1 ELSE 0 END")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) NORMAL_NUMBER")
            '-- 明細部　ロケ違い数 SUM(警告)
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sbSQL.AppendLine($"     CASE WHEN PHYINV_STATUS = '{PHYINV_STATUS.Warning}' THEN 1 ELSE 0 END")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) LOCATION_DIFFERENCE_NUMBER")
            '-- 明細部　在庫なし数 SUM(現物なし(未処理を含む))
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sbSQL.AppendLine($"     CASE WHEN PHYINV_STATUS = '{PHYINV_STATUS.NoSpot}' THEN 1 ELSE 0 END")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) OUT_OF_STOCK")
            sbSQL.AppendLine("FROM T_INV_ASSET ia")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AUTH")
            sbSQL.AppendLine("ON  AUTH.LOC_L_CD = ia.LOC_L_CD")
            sbSQL.AppendLine("AND ((AUTH.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND AUTH.LOC_M_CD = ia.LOC_M_CD) OR (AUTH.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
            sbSQL.AppendLine("GROUP BY ia.INV_ID,ia.LOC_L_CD,AUTH.UPD_FLG ")
            sbSQL.AppendLine(") ia")
            sbSQL.AppendLine("ON  INV.INV_ID = ia.INV_ID AND INV.INV_LOC_L_CD = ia.LOC_L_CD ")

            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{authInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{authInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{authInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{authInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthOption", $"{authInfo.AuthOption}")

            sbSQL.AppendLine("WHERE MLCL.LOC_DIV = '" & LOC_DIV.LocationL & "'")
            ' 棚卸未実施
            If _ViewModel.SearchNoStockTaking Then
                sbSQL.AppendLine("AND INV.INV_LOC_L_CD IS NULL ")
            End If
            sbSQL.AppendLine("AND EXISTS (")
            sbSQL.AppendLine("SELECT 'X' FROM dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AUTH")
            sbSQL.AppendLine("WHERE AUTH.LOC_L_CD = MLCL.LOC_L_CD)")

            sbSQL.AppendLine("ORDER BY")
            sbSQL.AppendLine("   MLCL.LOC_L_CD")


            Dim query As DbSqlQuery(Of InventorySummaryViewModel) = db.InventorySummaryView.SqlQuery(sbSQL.ToString, _paramSqlList.ToArray)
            lstInventorySummaryList = query.ToList

            'データを返却
            Return lstInventorySummaryList
        End Function
#End Region

#Region "棚卸実績処理"
        ''' <summary>
        ''' 棚卸実績処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function InvResultExport(ByVal ViewModel As InventorySummaryViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Dim invId As List(Of String) = New List(Of String)
            invId = Session(CommonSessionKey.INV_ID)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            _ViewModel = ViewModel

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.InventoryHistory.KeyDiv, CommonSettingKey.InventoryHistory.Key1, CommonSettingKey.InventoryHistory.Key2, CommonSettingKey.InventoryHistory.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

            'エクセルファイル出力
            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'CSVヘッダー取得
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_STOCK_LIST)
            Dim sHeaderAbumn As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_CONSTRUCTION_ASSET_LIST)
            Dim sHeaderAS02 As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_CONSTRUCTION_HISTORY_LIST)

            '各件数取得
            Dim AbumnDatacnt As Integer = 0
            Dim Pattern1Datacnt As Integer = 0
            Dim Pattern2Datacnt As Integer = 0

            'テンプレートフォルダ
            Dim sTempFullPath As String = Server.MapPath(CommonConst.TEMPLATE_FOLDERNM)

            Dim ZipFile As String = ToolInventory.InvoiceResultOutPut(invId(0), sFullPath, sTempFullPath, sHeaderAbumn, sHeaderAS02, AbumnDatacnt, Pattern1Datacnt, Pattern2Datacnt)

            '棚卸IDで棚卸テーブルから棚卸名を取得
            Dim db As New SystemDBContext
            'DBのデータを取得する
            Dim model As InventoryModel = db.Inventory.Find(Convert.ToInt64(invId(0)))

            'zipファイル名前
            Dim ZipFileName As String = String.Format(model.INV_NAME + "_{0}.zip", Now.ToString("yyyyMMddHHmmdd"))
            'zipファイルダウンロード
            Call FileDownload(ZipFileName, "application/zip", ZipFile)

            'zipファイル削除
            System.IO.File.Delete(ZipFile)

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_INVENTRY_RESULT_OUTPUT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     "ABUMN_MALSY.xlsx　" & AbumnDatacnt & "," &
                                                    "AS02Pattern_1_MALSY.xlsx　" & Pattern1Datacnt & "," &
                                                    "AS02Pattern_2_MALSY.xlsx　" & Pattern2Datacnt
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryHistory.LABEL_TITLE))

            '棚卸IDセッションクリア
            If Session(CommonSessionKey.INV_ID) IsNot Nothing Then
                Session.Remove(CommonSessionKey.INV_ID)
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
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

#Region "棚卸開始宣言処理"
        ''' <summary>
        ''' 棚卸開始宣言処理
        ''' </summary>
        Private Sub RegistInventory()
            'ViewModelをクラス変数に保持
            '_ViewModel = ViewModel

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder

            Dim viewAssetStatusCount = New List(Of AssetStatusCountViewModel)
            Dim viewPhysicalInventoryCount = New List(Of PhysicalInventoryCountViewModel)
            Dim viewNoItemNumberCount = New List(Of NoItemNumberViewModel)
            Dim viewAsset = New List(Of AssetViewModel)

            Dim assetStatusCount As Int64 = 0
            Dim physicalInventoryCount As Int64 = 0
            Dim noItemNumberCount As Int64 = 0

            Dim params As List(Of SqlParameter)

            '20231013 ADD Start Shirakawa
            '棚卸ID番号を採番
            Dim intInvId As Integer = Dao.ToolReserve.GetNextReserveInvId

            '大ロケコード
            Dim strLoc_Cd As String = ""

            '20231026 ADD Start Shirakawa
            '' 資産マスタの値を取得する 帳簿在庫数
            'sbSQL.Clear()
            'sbSQL.AppendLine("SELECT")
            'sbSQL.AppendLine("COUNT_BIG(*) ASSET_STATUS_COUNT")
            'sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount ")
            'sbSQL.AppendLine("FROM M_ASSET")
            'sbSQL.AppendLine("WHERE")
            'sbSQL.AppendLine("ASSET_STATUS IN (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ")")
            'viewAssetStatusCount = db.AssetStatusCountView.SqlQuery(sbSQL.ToString).ToList
            'If viewAssetStatusCount.Count > 0 Then
            '    assetStatusCount = viewAssetStatusCount(0).ASSET_STATUS_COUNT
            'End If

            '' 資産マスタの値を取得する 実棚正常数
            'sbSQL.Clear()
            'sbSQL.AppendLine("SELECT")
            'sbSQL.AppendLine("COUNT_BIG(*) PHYINV_STATUS_COUNT")
            'sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount ")
            'sbSQL.AppendLine("FROM M_ASSET")
            'sbSQL.AppendLine("WHERE")
            'sbSQL.AppendLine("PHYINV_STATUS = '" & PHYINV_STATUS.Normal & "'")
            'viewPhysicalInventoryCount = db.PhysicalInventoryCountView.SqlQuery(sbSQL.ToString).ToList
            'If viewPhysicalInventoryCount.Count > 0 Then
            '    physicalInventoryCount = viewPhysicalInventoryCount(0).PHYINV_STATUS_COUNT
            'End If

            '' 資産マスタの値を取得する 正常の件数
            'sbSQL.Clear()
            'sbSQL.AppendLine("SELECT")
            'sbSQL.AppendLine("COUNT_BIG(*) NO_ITEM_NUMBER")
            'sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount ")
            'sbSQL.AppendLine("FROM M_ASSET")
            'sbSQL.AppendLine("WHERE")
            'sbSQL.AppendLine("PHYINV_STATUS = '" & PHYINV_STATUS.NoSpot & "'")
            'viewNoItemNumberCount = db.NoItemNumberView.SqlQuery(sbSQL.ToString).ToList
            'If viewNoItemNumberCount.Count > 0 Then
            '    noItemNumberCount = viewNoItemNumberCount(0).NO_ITEM_NUMBER
            'End If
            '20231026 ADD End Shirakawa


            ''20231026 ADD Start Shirakawa
            '' 資産マスタの実棚状態を更新する
            'sbSQL.Clear()
            'sbSQL.AppendLine("SELECT")
            'sbSQL.AppendLine("ma.ASSET_NO")
            'sbSQL.AppendLine(",ma.LOC_L_CD")
            'sbSQL.AppendLine(",ma.LOC_M_CD")
            'sbSQL.AppendLine(",ma.LOC_S_CD")
            'sbSQL.AppendLine(",ma.ASSET_STATUS")
            'sbSQL.AppendLine(",LOC_L.LOC_NAME LOC_L_NAME")
            'sbSQL.AppendLine(",LOC_M.LOC_NAME LOC_M_NAME")
            'sbSQL.AppendLine(",LOC_S.LOC_NAME LOC_S_NAME")
            'sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount ")
            'sbSQL.AppendLine("FROM M_ASSET ma")
            'sbSQL.AppendLine("LEFT JOIN M_LOCATION LOC_L")
            'sbSQL.AppendLine("ON")
            'sbSQL.AppendLine("LOC_L.LOC_DIV = '" & LOC_DIV.LocationL & "'")
            'sbSQL.AppendLine("AND LOC_L.LOC_L_CD = ma.LOC_L_CD")
            'sbSQL.AppendLine("LEFT JOIN M_LOCATION LOC_M")
            'sbSQL.AppendLine("ON")
            'sbSQL.AppendLine("LOC_M.LOC_DIV = '" & LOC_DIV.LocationM & "'")
            'sbSQL.AppendLine("AND LOC_M.LOC_L_CD = ma.LOC_L_CD")
            'sbSQL.AppendLine("AND LOC_M.LOC_M_CD = ma.LOC_M_CD")
            'sbSQL.AppendLine("LEFT JOIN M_LOCATION LOC_S")
            'sbSQL.AppendLine("ON")
            'sbSQL.AppendLine("LOC_S.LOC_DIV = '" & LOC_DIV.LocationS & "'")
            'sbSQL.AppendLine("AND LOC_S.LOC_L_CD = ma.LOC_L_CD")
            'sbSQL.AppendLine("AND LOC_S.LOC_M_CD = ma.LOC_M_CD")
            'sbSQL.AppendLine("AND LOC_S.LOC_S_CD = ma.LOC_S_CD")
            'viewAsset = db.AssetView.SqlQuery(sbSQL.ToString).ToList
            'For Each item In viewAsset
            '    sbSQL.Clear()
            '    sbSQL.AppendLine("UPDATE M_ASSET SET")
            '    sbSQL.AppendLine("PHYINV_TS = FORMAT(GetDate(), 'yyyyMMddHHmmss')")
            '    If item.ASSET_STATUS = ASSET_STATUS.Preparation Or item.ASSET_STATUS = ASSET_STATUS.Removed Then
            '        sbSQL.AppendLine(",PHYINV_STATUS = '" & PHYINV_STATUS.Excluded & "'")
            '    ElseIf item.ASSET_STATUS = 1 Then
            '        sbSQL.AppendLine(",PHYINV_STATUS = '" & PHYINV_STATUS.NoSpot & "'")
            '    Else
            '        sbSQL.AppendLine(",PHYINV_STATUS = '" & PHYINV_STATUS.Normal & "'")
            '    End If
            '    sbSQL.AppendLine(",PHYINV_LOC_L_CD = '" & item.LOC_L_CD & "'")
            '    sbSQL.AppendLine(",PHYINV_LOC_M_CD = '" & item.LOC_M_CD & "'")
            '    sbSQL.AppendLine(",PHYINV_LOC_S_CD = '" & item.LOC_S_CD & "'")
            '    sbSQL.AppendLine(",PHYINV_LOC_L_NAME = '" & item.LOC_L_NAME & "'")
            '    sbSQL.AppendLine(",PHYINV_LOC_M_NAME = '" & item.LOC_M_NAME & "'")
            '    sbSQL.AppendLine(",PHYINV_LOC_S_NAME = '" & item.LOC_S_NAME & "'")
            '    sbSQL.AppendLine(",PHYINV_EMPLOYEE_CD = '" & _LoginInfo.EmployeeCd & "'")
            '    sbSQL.AppendLine(",PHYINV_EMPLOYEE_NAME = '" & _LoginInfo.EmployeeNm & "'")
            '    sbSQL.AppendLine("WHERE")
            '    sbSQL.AppendLine("ASSET_NO = '" & item.ASSET_NO & "'")

            '    db.Database.ExecuteSqlCommand(sbSQL.ToString())
            'Next

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try
                    'sbSQL.Clear()
                    'params = New List(Of SqlParameter)

                    'sbSQL.AppendLine("UPDATE M_ASSET SET")

                    'sbSQL.AppendLine("    PHYINV_STATUS = ")
                    'sbSQL.AppendLine("      CASE")
                    'sbSQL.AppendLine("          WHEN ASSET_STATUS = '" & ASSET_STATUS.Storage & "' THEN '" & PHYINV_STATUS.NoSpot & "'")
                    'sbSQL.AppendLine("          WHEN ASSET_STATUS = '" & ASSET_STATUS.Lending & "' THEN '" & PHYINV_STATUS.Normal & "'")
                    'sbSQL.AppendLine("          ELSE '" & PHYINV_STATUS.Excluded & "'")
                    'sbSQL.AppendLine("      End")

                    'sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                    'sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                    'sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                    'sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                    ''20231026 ADD Shirakawa 大ロケ
                    'sbSQL.AppendLine("WHERE")
                    'Dim SqlLocCd As String = String.Empty
                    ''配列から1つの文字列に結合
                    'For i As Integer = 0 To _invLocCd.Count - 1
                    '    SqlLocCd = $"{SqlLocCd}'{_invLocCd(i)}'"
                    '    '次にまだ要素がある場合、後ろに,(カンマ)をつける
                    '    If i <> _invLocCd.Count - 1 Then
                    '        SqlLocCd = $"{SqlLocCd},"
                    '    End If
                    'Next
                    'sbSQL.AppendLine($" LOC_L_CD IN ({SqlLocCd})")


                    ''20231026 ADD End Shirakawa

                    'params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    'params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    'params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE)))

                    'db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)


                    '20231026 ADD Start Shirakawa
                    ' 棚卸管理追加
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("INSERT INTO T_INVENTORY")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine("INV_ID")   '20231026 ADD Shirakawa
                    sbSQL.AppendLine(", START_DEC_TS")
                    sbSQL.AppendLine(", START_DEC_EMPLOYEE_CD")
                    sbSQL.AppendLine(", START_DEC_NAME")
                    sbSQL.AppendLine(", INV_NAME")
                    sbSQL.AppendLine(", INV_CONFIRM_TS")
                    sbSQL.AppendLine(", INV_CONFIRM_EMPLOYEE_CD")
                    sbSQL.AppendLine(", INV_CONFIRM_NAME")
                    '20231026 DLE Start Shirakawa
                    'sbSQL.AppendLine(", LOGIC_INV_NUM")
                    'sbSQL.AppendLine(", PHYINV_NUM")
                    'sbSQL.AppendLine(", LOC_DIFF_NUM")
                    'sbSQL.AppendLine(", NO_ITEM_NUM")
                    'sbSQL.AppendLine(", INV_ALL_CONFIRM_TS")
                    '20231026 DLE End Shirakawa
                    sbSQL.AppendLine(", UPD_DATETIME")
                    sbSQL.AppendLine(", UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine(", UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine(", UPD_TASK")
                    sbSQL.AppendLine(") VALUES (")
                    sbSQL.AppendLine($"@INV_ID")    '20231026 ADD Shirakawa
                    sbSQL.AppendLine(",FORMAT(GetDate(), 'yyyyMMddHHmmss')")
                    sbSQL.AppendLine(",@START_DEC_EMPLOYEE_CD")
                    sbSQL.AppendLine(",@START_DEC_NAME")
                    sbSQL.AppendLine(",@INV_NAME")
                    sbSQL.AppendLine(",NULL")
                    sbSQL.AppendLine(",NULL")
                    sbSQL.AppendLine(",NULL")
                    '20231026 DLE Start Shirakawa
                    'sbSQL.AppendLine("," & assetStatusCount)
                    'sbSQL.AppendLine("," & physicalInventoryCount)
                    'sbSQL.AppendLine(",0")
                    'sbSQL.AppendLine("," & noItemNumberCount)
                    'sbSQL.AppendLine(",NULL")
                    '20231026 DLE End Shirakawa
                    sbSQL.AppendLine(",GetDate()")
                    sbSQL.AppendLine(",@UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine(",@UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine(",@UPD_TASK")
                    sbSQL.AppendLine(")")

                    params.Add(New SqlParameter($"@START_DEC_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@START_DEC_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@INV_NAME", _ViewModel.INV_NAME))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE)))
                    params.Add(New SqlParameter($"@INV_ID", intInvId))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '20231013 ADD Start Shirakawa
                    '棚卸明細T登録
                    params = New List(Of SqlParameter)

                    sbSQL.Clear()
                    sbSQL.AppendLine("INSERT INTO T_INV_DETAIL")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine(" INV_ID")
                    sbSQL.AppendLine(",INV_LOC_L_CD")
                    sbSQL.AppendLine(",LOGIC_INV_NUM")
                    sbSQL.AppendLine(",PHYINV_NUM")
                    sbSQL.AppendLine(",LOC_DIFF_NUM")
                    sbSQL.AppendLine(",NO_ITEM_NUM")

                    sbSQL.AppendLine(",UPD_DATETIME")
                    sbSQL.AppendLine(",UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine(",UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine(",UPD_TASK")
                    sbSQL.AppendLine(")")
                    sbSQL.AppendLine("VALUES")

                    '一覧で選択したロケーションの数ループ
                    For i As Integer = 0 To _invLocCd.Count - 1
                        'パラムクリア
                        params = New List(Of SqlParameter)

                        strLoc_Cd = _invLocCd(i) '選択した各ロケーションを代入

                        sbSQL.AppendLine("(")
                        sbSQL.AppendLine($"@INV_ID")                     '棚卸ID
                        sbSQL.AppendLine($",'{strLoc_Cd.ToString}'")       '大ロケ
                        '帳簿在庫数
                        sbSQL.AppendLine("       ,(SELECT COUNT_BIG(*) FROM M_ASSET")
                        sbSQL.AppendLine("           WHERE M_ASSET.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ")")
                        sbSQL.AppendLine($"       AND M_ASSET.LOC_L_CD = '{strLoc_Cd}' ")  '20231026 ADD Shirakawa   大ロケ別に取得
                        sbSQL.AppendLine("       )")

                        '実棚数
                        sbSQL.AppendLine("        ,(SELECT COUNT_BIG(*) FROM M_ASSET")
                        sbSQL.AppendLine("            WHERE M_ASSET.ASSET_STATUS IN (" & ASSET_STATUS.Lending & ")")
                        sbSQL.AppendLine($"       AND M_ASSET.LOC_L_CD = '{strLoc_Cd}' ")  '20231026 ADD Shirakawa   大ロケ別に取得
                        sbSQL.AppendLine("        )")

                        'ロケ違い数
                        sbSQL.AppendLine($",0")

                        '在庫なし数
                        sbSQL.AppendLine("          ,(SELECT COUNT_BIG(*) FROM M_ASSET")
                        sbSQL.AppendLine("              WHERE M_ASSET.ASSET_STATUS In (" & ASSET_STATUS.Storage & ")")
                        sbSQL.AppendLine($"       AND M_ASSET.LOC_L_CD = '{strLoc_Cd}' ")  '20231026 ADD Shirakawa   大ロケ別に取得
                        sbSQL.AppendLine("          )")

                        sbSQL.AppendLine($",{clsSV_DB.SQL_GET_DATE2}")
                        sbSQL.AppendLine($",@UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine($",@UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine($",@UPD_TASK")
                        sbSQL.AppendLine(")")

                        'ループが続くなら
                        If i <> _invLocCd.Count - 1 Then
                            sbSQL.AppendLine(",")   'カンマを追加
                        End If

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE)))
                        params.Add(New SqlParameter($"@INV_ID", intInvId))

                    Next

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '20231026 ADD End Shirakawa

                    '棚卸資産Tに該当拠点の資産Mのデータを登録する
                    params = New List(Of SqlParameter)

                    sbSQL.Clear()
                    sbSQL.AppendLine("INSERT INTO T_INV_ASSET")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine(" INV_ID")
                    sbSQL.AppendLine(",INV_LOC_L_CD")
                    sbSQL.AppendLine(",ASSET_NO")
                    sbSQL.AppendLine(",ASSET_ID")
                    sbSQL.AppendLine(",SUB_ASSET_NO")
                    sbSQL.AppendLine(",EPC_SIZE")
                    sbSQL.AppendLine(",GEN_ASSET_ID")
                    sbSQL.AppendLine(",ASSET_DIV")
                    sbSQL.AppendLine(",ASSET_NAME")
                    sbSQL.AppendLine(",LOC_L_CD")
                    sbSQL.AppendLine(",LOC_M_CD")
                    sbSQL.AppendLine(",LOC_S_CD")
                    sbSQL.AppendLine(",CAT_L_CD")
                    sbSQL.AppendLine(",CAT_M_CD")
                    sbSQL.AppendLine(",CAT_S_CD")
                    sbSQL.AppendLine(",MAKER")
                    sbSQL.AppendLine(",PRODUCT_NO")
                    sbSQL.AppendLine(",MODEL_NO")
                    sbSQL.AppendLine(",LEASE_NO")
                    sbSQL.AppendLine(",LEASE_LIMIT")
                    sbSQL.AppendLine(",GENERIC_LIMIT")
                    sbSQL.AppendLine(",FREE_ITEM1")
                    sbSQL.AppendLine(",FREE_ITEM2")
                    sbSQL.AppendLine(",FREE_ITEM3")
                    sbSQL.AppendLine(",FREE_ITEM4")
                    sbSQL.AppendLine(",FREE_ITEM5")
                    sbSQL.AppendLine(",FREE_ITEM6")
                    sbSQL.AppendLine(",FREE_ITEM7")
                    sbSQL.AppendLine(",FREE_ITEM8")
                    sbSQL.AppendLine(",FREE_ITEM9")
                    sbSQL.AppendLine(",FREE_ITEM10")
                    sbSQL.AppendLine(",FREE_ITEM11")
                    sbSQL.AppendLine(",FREE_ITEM12")
                    sbSQL.AppendLine(",FREE_ITEM13")
                    sbSQL.AppendLine(",FREE_ITEM14")
                    sbSQL.AppendLine(",FREE_ITEM15")
                    sbSQL.AppendLine(",FREE_ITEM16")
                    sbSQL.AppendLine(",FREE_ITEM17")
                    sbSQL.AppendLine(",FREE_ITEM18")
                    sbSQL.AppendLine(",FREE_ITEM19")
                    sbSQL.AppendLine(",FREE_ITEM20")
                    sbSQL.AppendLine(",FREE_ITEM21")
                    sbSQL.AppendLine(",FREE_ITEM22")
                    sbSQL.AppendLine(",FREE_ITEM23")
                    sbSQL.AppendLine(",FREE_ITEM24")
                    sbSQL.AppendLine(",FREE_ITEM25")
                    sbSQL.AppendLine(",FREE_ITEM26")
                    sbSQL.AppendLine(",FREE_ITEM27")
                    sbSQL.AppendLine(",FREE_ITEM28")
                    sbSQL.AppendLine(",FREE_ITEM29")
                    sbSQL.AppendLine(",FREE_ITEM30")
                    sbSQL.AppendLine(",FREE_ITEM31")
                    sbSQL.AppendLine(",FREE_ITEM32")
                    sbSQL.AppendLine(",FREE_ITEM33")
                    sbSQL.AppendLine(",FREE_ITEM34")
                    sbSQL.AppendLine(",FREE_ITEM35")
                    sbSQL.AppendLine(",FREE_ITEM36")
                    sbSQL.AppendLine(",FREE_ITEM37")
                    sbSQL.AppendLine(",FREE_ITEM38")
                    sbSQL.AppendLine(",FREE_ITEM39")
                    sbSQL.AppendLine(",FREE_ITEM40")
                    sbSQL.AppendLine(",FREE_ITEM41")
                    sbSQL.AppendLine(",FREE_ITEM42")
                    sbSQL.AppendLine(",FREE_ITEM43")
                    sbSQL.AppendLine(",FREE_ITEM44")
                    sbSQL.AppendLine(",FREE_ITEM45")
                    sbSQL.AppendLine(",FREE_ITEM46")
                    sbSQL.AppendLine(",FREE_ITEM47")
                    sbSQL.AppendLine(",FREE_ITEM48")
                    sbSQL.AppendLine(",FREE_ITEM49")
                    sbSQL.AppendLine(",FREE_ITEM50")
                    sbSQL.AppendLine(",FREE_ITEM51")
                    sbSQL.AppendLine(",FREE_ITEM52")
                    sbSQL.AppendLine(",FREE_ITEM53")
                    sbSQL.AppendLine(",FREE_ITEM54")
                    sbSQL.AppendLine(",FREE_ITEM55")
                    sbSQL.AppendLine(",FREE_ITEM56")
                    sbSQL.AppendLine(",FREE_ITEM57")
                    sbSQL.AppendLine(",FREE_ITEM58")
                    sbSQL.AppendLine(",FREE_ITEM59")
                    sbSQL.AppendLine(",FREE_ITEM60")
                    sbSQL.AppendLine(",ASSET_PIC_FILENAME")
                    sbSQL.AppendLine(",LEASE_CNT")
                    sbSQL.AppendLine(",ASSET_STATUS")
                    sbSQL.AppendLine(",EPC_ENC_STATUS")
                    sbSQL.AppendLine(",TAG_LINK_STATUS")
                    sbSQL.AppendLine(",LEASE_STATUS")
                    sbSQL.AppendLine(",LEASE_PLACE_CD")
                    sbSQL.AppendLine(",LEASE_PLACE_NAME")
                    sbSQL.AppendLine(",REMARK")
                    sbSQL.AppendLine(",LEASE_EMPLOYEE_CD")
                    sbSQL.AppendLine(",LEASE_EMPLOYEE_NAME")
                    sbSQL.AppendLine(",LEASE_DT")
                    sbSQL.AppendLine(",LEASE_DUE_DT")
                    sbSQL.AppendLine(",MAINTE_LEASE_NUM")
                    sbSQL.AppendLine(",MAINTE_LEASE_DAY")
                    sbSQL.AppendLine(",MAINTE_LEASE_DAY_WORK_FLAG")
                    sbSQL.AppendLine(",MAINTE_DAY")
                    sbSQL.AppendLine(",MAINTE_DAY_WORK_FLAG")
                    sbSQL.AppendLine(",RETIRE_TS")
                    sbSQL.AppendLine(",RETIRE_EMPLOYEE_CD")
                    sbSQL.AppendLine(",RETIRE_EMPLOYEE_NAME")
                    sbSQL.AppendLine(",RETIRE_ASSET_ID")

                    sbSQL.AppendLine(",LOC_L_NAME")
                    sbSQL.AppendLine(",LOC_M_NAME")
                    sbSQL.AppendLine(",LOC_S_NAME")
                    sbSQL.AppendLine(",CAT_L_NAME")
                    sbSQL.AppendLine(",CAT_M_NAME")
                    sbSQL.AppendLine(",CAT_S_NAME")

                    sbSQL.AppendLine(",PHYINV_TS")
                    sbSQL.AppendLine(",PHYINV_STATUS")
                    sbSQL.AppendLine(",PHYINV_LOC_L_CD")
                    sbSQL.AppendLine(",PHYINV_LOC_M_CD")
                    sbSQL.AppendLine(",PHYINV_LOC_S_CD")
                    sbSQL.AppendLine(",PHYINV_LOC_L_NAME")
                    sbSQL.AppendLine(",PHYINV_LOC_M_NAME")
                    sbSQL.AppendLine(",PHYINV_LOC_S_NAME")
                    sbSQL.AppendLine(",PHYINV_EMPLOYEE_CD")
                    sbSQL.AppendLine(",PHYINV_EMPLOYEE_NAME")

                    sbSQL.AppendLine(",OPE_STATUS")
                    sbSQL.AppendLine(",DIVERSION_REMOVE")
                    sbSQL.AppendLine(",REMOVE_DATE")
                    sbSQL.AppendLine(",REMOVE_REASON")
                    sbSQL.AppendLine(",REMOVE_CONTENT")
                    sbSQL.AppendLine(",CHANGE_LOC_M_CD")
                    sbSQL.AppendLine(",CHANGE_LOC_S_CD")
                    sbSQL.AppendLine(",CHANGE_EMPLOYEE")

                    sbSQL.AppendLine(",IS_TANGIBLE")

                    sbSQL.AppendLine(",REMARK1")
                    sbSQL.AppendLine(",REMARK2")
                    sbSQL.AppendLine(",REMARK3")
                    sbSQL.AppendLine(",REMARK4")
                    sbSQL.AppendLine(",REMARK5")
                    sbSQL.AppendLine(",REMARK6")
                    sbSQL.AppendLine(",REMARK7")
                    sbSQL.AppendLine(",REMARK8")
                    sbSQL.AppendLine(",REMARK9")
                    sbSQL.AppendLine(",REMARK10")

                    sbSQL.AppendLine(",LOCK_EMPLOYEE_CD")

                    sbSQL.AppendLine(",INS_DATETIME")
                    sbSQL.AppendLine(",INS_EMPLOYEE_CD")
                    sbSQL.AppendLine(",INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine(",UPD_DATETIME")
                    sbSQL.AppendLine(",UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine(",UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine(",UPD_TASK")



                    sbSQL.AppendLine(")")
                    sbSQL.AppendLine(" SELECT ")

                    sbSQL.AppendLine($"@INV_ID ")
                    sbSQL.AppendLine($",'{strLoc_Cd.ToString}'")       '大ロケ
                    sbSQL.AppendLine($",ASSET.ASSET_NO ")
                    sbSQL.AppendLine($",ASSET.ASSET_ID ")
                    sbSQL.AppendLine($",ASSET.SUB_ASSET_NO ")
                    sbSQL.AppendLine($",ASSET.EPC_SIZE ")
                    sbSQL.AppendLine($",ASSET.GEN_ASSET_ID ")
                    sbSQL.AppendLine($",ASSET.ASSET_DIV ")
                    sbSQL.AppendLine($",ASSET.ASSET_NAME ")
                    sbSQL.AppendLine($",ASSET.LOC_L_CD ")
                    sbSQL.AppendLine($",ASSET.LOC_M_CD ")
                    sbSQL.AppendLine($",ASSET.LOC_S_CD ")
                    sbSQL.AppendLine($",ASSET.CAT_L_CD ")
                    sbSQL.AppendLine($",ASSET.CAT_M_CD ")
                    sbSQL.AppendLine($",ASSET.CAT_S_CD ")
                    sbSQL.AppendLine($",ASSET.MAKER ")
                    sbSQL.AppendLine($",ASSET.PRODUCT_NO ")
                    sbSQL.AppendLine($",ASSET.MODEL_NO ")
                    sbSQL.AppendLine($",ASSET.LEASE_NO ")
                    sbSQL.AppendLine($",ASSET.LEASE_LIMIT ")
                    sbSQL.AppendLine($",ASSET.GENERIC_LIMIT ")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM1")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM2")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM3")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM4")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM5")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM6")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM7")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM8")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM9")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM10")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM11")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM12")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM13")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM14")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM15")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM16")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM17")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM18")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM19")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM20")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM21")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM22")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM23")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM24")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM25")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM26")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM27")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM28")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM29")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM30")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM31")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM32")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM33")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM34")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM35")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM36")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM37")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM38")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM39")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM40")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM41")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM42")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM43")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM44")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM45")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM46")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM47")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM48")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM49")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM50")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM51")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM52")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM53")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM54")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM55")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM56")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM57")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM58")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM59")
                    sbSQL.AppendLine(",ASSET.FREE_ITEM60")
                    sbSQL.AppendLine(",ASSET.ASSET_PIC_FILENAME")
                    sbSQL.AppendLine(",ASSET.LEASE_CNT")
                    sbSQL.AppendLine(",ASSET.ASSET_STATUS")
                    sbSQL.AppendLine(",ASSET.EPC_ENC_STATUS")
                    sbSQL.AppendLine(",ASSET.TAG_LINK_STATUS")
                    sbSQL.AppendLine(",ASSET.LEASE_STATUS")
                    sbSQL.AppendLine(",ASSET.LEASE_PLACE_CD")
                    sbSQL.AppendLine(",ASSET.LEASE_PLACE_NAME")
                    sbSQL.AppendLine(",ASSET.REMARK")
                    sbSQL.AppendLine(",ASSET.LEASE_EMPLOYEE_CD")
                    sbSQL.AppendLine(",ASSET.LEASE_EMPLOYEE_NAME")
                    sbSQL.AppendLine(",ASSET.LEASE_DT")
                    sbSQL.AppendLine(",ASSET.LEASE_DUE_DT")
                    sbSQL.AppendLine(",ASSET.MAINTE_LEASE_NUM")
                    sbSQL.AppendLine(",ASSET.MAINTE_LEASE_DAY")
                    sbSQL.AppendLine(",ASSET.MAINTE_LEASE_DAY_WORK_FLAG")
                    sbSQL.AppendLine(",ASSET.MAINTE_DAY")
                    sbSQL.AppendLine(",ASSET.MAINTE_DAY_WORK_FLAG")
                    sbSQL.AppendLine(",ASSET.RETIRE_TS")
                    sbSQL.AppendLine(",ASSET.RETIRE_EMPLOYEE_CD")
                    sbSQL.AppendLine(",ASSET.RETIRE_EMPLOYEE_NAME")
                    sbSQL.AppendLine(",ASSET.RETIRE_ASSET_ID")

                    sbSQL.AppendLine(",LOC_L.LOC_NAME")
                    sbSQL.AppendLine(",LOC_M.LOC_NAME")
                    sbSQL.AppendLine(",LOC_S.LOC_NAME")
                    sbSQL.AppendLine(",CAT_L.CAT_NAME")
                    sbSQL.AppendLine(",CAT_M.CAT_NAME")
                    sbSQL.AppendLine(",CAT_S.CAT_NAME")

                    sbSQL.AppendLine(",ASSET.PHYINV_TS")
                    '実棚ステータス　保管中は現物無し、貸出中は正常、以外は対象外
                    sbSQL.AppendLine(",CASE")
                    sbSQL.AppendLine(" WHEN ASSET.ASSET_STATUS = '" & ASSET_STATUS.Storage & "'" & "THEN '" & PHYINV_STATUS.NoSpot & "'")
                    sbSQL.AppendLine(" WHEN ASSET.ASSET_STATUS = '" & ASSET_STATUS.Lending & "'" & "THEN '" & PHYINV_STATUS.Normal & "'")
                    sbSQL.AppendLine(" ELSE '" & PHYINV_STATUS.Excluded & "'")
                    sbSQL.AppendLine(" END AS PHYINV_STATUS")

                    sbSQL.AppendLine(", ASSET.PHYINV_LOC_L_CD")
                    sbSQL.AppendLine(", ASSET.PHYINV_LOC_M_CD")
                    sbSQL.AppendLine(", ASSET.PHYINV_LOC_S_CD")
                    sbSQL.AppendLine(", ASSET.PHYINV_LOC_L_NAME")
                    sbSQL.AppendLine(", ASSET.PHYINV_LOC_M_NAME")
                    sbSQL.AppendLine(", ASSET.PHYINV_LOC_S_NAME")
                    sbSQL.AppendLine(", ASSET.PHYINV_EMPLOYEE_CD")
                    sbSQL.AppendLine(", ASSET.PHYINV_EMPLOYEE_NAME")

                    sbSQL.AppendLine(", ASSET.FREE_ITEM3")
                    sbSQL.AppendLine(", NULL")
                    sbSQL.AppendLine(", NULL")
                    sbSQL.AppendLine(", NULL")
                    sbSQL.AppendLine(", NULL")
                    sbSQL.AppendLine(", NULL")
                    sbSQL.AppendLine(", NULL")
                    sbSQL.AppendLine(", NULL")


                    sbSQL.AppendLine(", ASSET.IS_TANGIBLE")

                    '[CC431] UPD Start 20261002
                    sbSQL.AppendLine(", ASSET.REMARK1")
                    sbSQL.AppendLine(", ASSET.REMARK2")
                    sbSQL.AppendLine(", ASSET.REMARK3")
                    sbSQL.AppendLine(", ASSET.REMARK4")
                    sbSQL.AppendLine(", ASSET.REMARK5")
                    sbSQL.AppendLine(", ASSET.REMARK6")
                    sbSQL.AppendLine(", ASSET.REMARK7")
                    sbSQL.AppendLine(", ASSET.REMARK8")
                    sbSQL.AppendLine(", ASSET.REMARK9")
                    sbSQL.AppendLine(", ASSET.REMARK10")
                    '[CC431] UPD End 20261002
                    sbSQL.AppendLine(", NULL")
                    sbSQL.AppendLine(", GetDate()")
                    sbSQL.AppendLine(",@INS_EMPLOYEE_CD")
                    sbSQL.AppendLine(",@INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine(", GetDate()")
                    sbSQL.AppendLine(",@UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine(",@UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine(",@UPD_TASK")


                    sbSQL.AppendLine($"FROM ")
                    sbSQL.AppendLine($"M_ASSET ASSET ") '資産マスタから取得
                    sbSQL.AppendLine("LEFT JOIN M_LOCATION LOC_L")
                    sbSQL.AppendLine("ON")
                    sbSQL.AppendLine("LOC_L.LOC_DIV = '" & LOC_DIV.LocationL & "'")
                    sbSQL.AppendLine("AND LOC_L.LOC_L_CD = ASSET.LOC_L_CD")
                    sbSQL.AppendLine("LEFT JOIN M_LOCATION LOC_M")
                    sbSQL.AppendLine("ON")
                    sbSQL.AppendLine("LOC_M.LOC_DIV = '" & LOC_DIV.LocationM & "'")
                    sbSQL.AppendLine("AND LOC_M.LOC_L_CD = ASSET.LOC_L_CD")
                    sbSQL.AppendLine("AND LOC_M.LOC_M_CD = ASSET.LOC_M_CD")
                    sbSQL.AppendLine("LEFT JOIN M_LOCATION LOC_S")
                    sbSQL.AppendLine("ON")
                    sbSQL.AppendLine("LOC_S.LOC_DIV = '" & LOC_DIV.LocationS & "'")
                    sbSQL.AppendLine("AND LOC_S.LOC_L_CD = ASSET.LOC_L_CD")
                    sbSQL.AppendLine("AND LOC_S.LOC_M_CD = ASSET.LOC_M_CD")
                    sbSQL.AppendLine("AND LOC_S.LOC_S_CD = ASSET.LOC_S_CD")
                    sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_L")
                    sbSQL.AppendLine("ON")
                    sbSQL.AppendLine("CAT_L.CAT_DIV = '" & CAT_DIV.CategoryL & "'")
                    sbSQL.AppendLine("AND CAT_L.CAT_L_CD = ASSET.CAT_L_CD")
                    sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_M")
                    sbSQL.AppendLine("ON")
                    sbSQL.AppendLine("CAT_M.CAT_DIV = '" & CAT_DIV.CategoryM & "'")
                    sbSQL.AppendLine("AND CAT_M.CAT_L_CD = ASSET.CAT_L_CD")
                    sbSQL.AppendLine("AND CAT_M.CAT_M_CD = ASSET.CAT_M_CD")
                    sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_S")
                    sbSQL.AppendLine("ON")
                    sbSQL.AppendLine("CAT_S.CAT_DIV = '" & CAT_DIV.CategoryS & "'")
                    sbSQL.AppendLine("AND CAT_S.CAT_L_CD = ASSET.CAT_L_CD")
                    sbSQL.AppendLine("AND CAT_S.CAT_M_CD = ASSET.CAT_M_CD")
                    sbSQL.AppendLine("AND CAT_S.CAT_S_CD = ASSET.CAT_S_CD")

                    sbSQL.AppendLine($"WHERE ")
                    sbSQL.AppendLine($"ASSET.LOC_L_CD = '{strLoc_Cd.ToString}' ")


                    params.Add(New SqlParameter($"@START_DEC_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@START_DEC_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@INV_NAME", _ViewModel.INV_NAME))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelInventorySummary.LABEL_TITLE)))
                    params.Add(New SqlParameter($"@INV_ID", intInvId))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    tran.Commit()

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)
                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using
        End Sub
#End Region

        '20230929 ADD Start Shirakawa
#Region "全選択ボタン押下時処理"
        ''' <summary>
        ''' 全選択ボタン押下時処理
        ''' </summary>
        ''' <param name="isSelectAll"></param>
        ''' <returns></returns>
        Public Function SelectAll(ByVal isSelectAll As Boolean) As ActionResult
            If Not isSelectAll Then
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                Return Json("")
            Else
                '全選択ボタン押下フラグをセットする
                Session(CommonSessionKey.ALL_SELECT_FLG) = True
                Return Json("")
            End If
        End Function
#End Region
        '20230929 ADD End Shirakawa

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

        '20231005 ADD Start Shirakawa
#Region "キー(棚卸ID/棚卸ロケ大区分コード)をセッションに保存し、実棚照会画面に遷移します"
        ''' <summary>
        ''' キー(棚卸ID/棚卸ロケ大区分コード)をセッションに保存し、実棚照会画面に遷移します
        ''' </summary>
        Public Function SetKeys(ByVal SelectKeys As String) As ActionResult
            Session.Remove(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY)
            Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY) = SelectKeys

            '結果表示
            Return RedirectToAction("Index", "InventoryInquiry")
        End Function

#End Region
        '20231005 ADD End Shirakawa

#Region "共通"

        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL(Optional ByVal isSort As Boolean = True) As String
            '--2021/11/10 add start nv iwamoto
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventorySummary, _LoginInfo)
            '--2021/11/10 add end nv iwamoto

            Dim sbSQL As StringBuilder = New StringBuilder

            Dim sbNIDate As StringBuilder = New StringBuilder
            Dim sbWhr As StringBuilder = New StringBuilder

            '直近棚卸日項目
            'sbNIDate.AppendLine("   CASE")
            'sbNIDate.AppendLine("      WHEN MAX(IsNull(MAST.PHYINV_TS, 0)) = 0 AND MAX(INV.INV_CONFIRM_TS) IS NULL THEN NULL") '20231030 ADD Shirakawa 条件追加
            'sbNIDate.AppendLine("      ELSE CONVERT(DATETIME, LEFT(MAX(MAST.PHYINV_TS), 8), 112)")
            'sbNIDate.AppendLine("   END")
            sbNIDate.AppendLine("   (")
            sbNIDate.AppendLine("     SELECT CASE WHEN PHYINV_TS IS NULL THEN NULL ELSE CONVERT(DATETIME, LEFT (PHYINV_TS, 8), 112) END NEAR_INVENTORY_DATE ")
            sbNIDate.AppendLine("     FROM (")
            sbNIDate.AppendLine("       SELECT ISNULL( ")
            sbNIDate.AppendLine("         ( ")
            sbNIDate.AppendLine("           SELECT MAX(PHYINV_TS) FROM M_ASSET WHERE M_ASSET.LOC_L_CD = MLCL.LOC_L_CD ")
            sbNIDate.AppendLine("         ), ")
            sbNIDate.AppendLine("         ( ")
            sbNIDate.AppendLine("           SELECT MAX(DET.INV_ALL_CONFIRM_TS) FROM T_INV_DETAIL DET INNER JOIN T_INVENTORY INV ON INV.INV_ID = DET.INV_ID WHERE DET.INV_LOC_L_CD = MLCL.LOC_L_CD ")
            sbNIDate.AppendLine("         ) ")
            sbNIDate.AppendLine("       ) PHYINV_TS")
            sbNIDate.AppendLine("     ) PHYINV ")
            sbNIDate.AppendLine("   ) NEAR_INVENTORY_DATE")

            '検索条件部分
            'ロケーション 大区分
            If Not IsNothing(_ViewModel.SearchLocL) AndAlso _ViewModel.SearchLocL <> "" Then
                sbWhr.AppendLine("AND {0}.LOC_L_CD = @locLCd")
                addSqlParam("locLCd", _ViewModel.SearchLocL)
            End If
            'ロケーション 中区分
            If Not IsNothing(_ViewModel.SearchLocM) AndAlso _ViewModel.SearchLocM <> "" Then
                sbWhr.AppendLine("AND {0}.LOC_M_CD = @locMCd")
                addSqlParam("locMCd", _ViewModel.SearchLocM)
            End If
            'ロケーション 小区分
            If Not IsNothing(_ViewModel.SearchLocS) AndAlso _ViewModel.SearchLocS <> "" Then
                sbWhr.AppendLine("AND {0}.LOC_S_CD = @locSCd")
                addSqlParam("locSCd", _ViewModel.SearchLocS)
            End If

            '一覧表示表用SQL
            sbSQL.Clear()
            If isSort Then
                sbSQL.AppendLine("SELECT ")
            Else
                sbSQL.AppendLine("SELECT TOP 1")
            End If
            sbSQL.AppendLine("   CASE WHEN INV.INV_LOC_L_CD IS NOT NULL THEN INV.INV_ID ELSE '' END AS INV_ID") '20231027 ADD Shirakawa
            sbSQL.AppendLine("  ,ISNULL(MLCL.LOC_L_CD, '') LOC_L_CD")
            sbSQL.AppendLine("  ,ISNULL(MLCL.LOC_NAME, '') LOC_L_NAME")
            sbSQL.AppendLine("  ,ia.UPD_FLG")
            sbSQL.AppendLine("  ,ISNULL(INV_NAME, '') INV_NAME_LIST ")
            'sbSQL.AppendLine("  ,ISNULL(MAST.LOC_M_CD, '') LOC_M_CD")
            'sbSQL.AppendLine("  ,ISNULL(MLCM.LOC_NAME, '') LOC_M_NAME")
            'sbSQL.AppendLine("  ,ISNULL(MAST.LOC_S_CD, '') LOC_S_CD")
            'sbSQL.AppendLine("  ,ISNULL(MLCS.LOC_NAME, '') LOC_S_NAME")

            '20230929 ADD Start Shirakawa
            '-- 明細部　棚卸状態
            sbSQL.AppendLine($"  , CASE WHEN INV.INV_LOC_L_CD IS NOT NULL THEN '{Me._Message.getMessage(CommonLabelInventorySummary.LABEL_INVENTORY_STARTED)}' ELSE '-' END AS INVENTORY_STATUS")

            '-- 明細部　棚卸開始日
            sbSQL.AppendLine("  , CASE WHEN INV.INV_LOC_L_CD IS NOT NULL THEN INV.START_DEC_TS ELSE '-' END AS INVENTORY_STARTDAY")
            sbSQL.AppendLine("  , CONVERT(BIGINT,BOOK_INVENTORY_NUMBER) AS BOOK_INVENTORY_NUMBER")
            sbSQL.AppendLine("  , CONVERT(BIGINT,NUMBER_OF_LOANS_WITHIN) AS NUMBER_OF_LOANS_WITHIN")
            sbSQL.AppendLine("  , CONVERT(BIGINT,ACTUAL_NUMBER_OF_SHELVES) AS ACTUAL_NUMBER_OF_SHELVES")
            sbSQL.AppendLine("  , CONVERT(BIGINT,NORMAL_NUMBER) AS NORMAL_NUMBER")
            sbSQL.AppendLine("  , CONVERT(BIGINT,LOCATION_DIFFERENCE_NUMBER) AS LOCATION_DIFFERENCE_NUMBER")
            sbSQL.AppendLine("  , CONVERT(BIGINT,OUT_OF_STOCK) AS OUT_OF_STOCK")

            sbSQL.AppendLine("  ," & sbNIDate.ToString)
            '            sbSQL.AppendLine("   AS NEAR_INVENTORY_DATE")
            sbSQL.AppendLine("  ,COUNT(*) OVER () AS TotalCount ")

            sbSQL.AppendLine("FROM M_LOCATION MLCL")
            sbSQL.AppendLine("LEFT JOIN (SELECT DISTINCT T_INVENTORY.* , T_INV_DETAIL.INV_LOC_L_CD FROM T_INV_DETAIL LEFT JOIN T_INVENTORY ON T_INVENTORY.INV_ID = T_INV_DETAIL.INV_ID WHERE T_INV_DETAIL.INV_ALL_CONFIRM_TS IS NULL) AS INV")
            sbSQL.AppendLine("ON  INV.INV_LOC_L_CD = MLCL.LOC_L_CD ")

            sbSQL.AppendLine("LEFT JOIN (")
            sbSQL.AppendLine("  SELECT")
            sbSQL.AppendLine("  ia.INV_ID,ia.LOC_L_CD,AUTH.UPD_FLG")
            '-- 明細部　帳簿在庫数 SUM(保管、貸出)
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN 1")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) BOOK_INVENTORY_NUMBER")
            '-- 明細部　内貸出数 SUM(貸出)
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS = '{ASSET_STATUS.Lending}' THEN 1")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) NUMBER_OF_LOANS_WITHIN")
            '-- 明細部　実棚数 SUM(正常、警告)
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sbSQL.AppendLine($"     CASE WHEN PHYINV_STATUS IN ('{PHYINV_STATUS.Normal}','{PHYINV_STATUS.Warning}') THEN 1 ELSE 0 END")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) ACTUAL_NUMBER_OF_SHELVES")
            '-- 明細部　正常数 SUM(正常)
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sbSQL.AppendLine($"     CASE WHEN PHYINV_STATUS = '{PHYINV_STATUS.Normal}' THEN 1 ELSE 0 END")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) NORMAL_NUMBER")
            '-- 明細部　ロケ違い数 SUM(警告)
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sbSQL.AppendLine($"     CASE WHEN PHYINV_STATUS = '{PHYINV_STATUS.Warning}' THEN 1 ELSE 0 END")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) LOCATION_DIFFERENCE_NUMBER")
            '-- 明細部　在庫なし数 SUM(現物なし(未処理を含む))
            sbSQL.AppendLine($", SUM(")
            sbSQL.AppendLine($"  CASE WHEN ASSET_STATUS IN ('{ASSET_STATUS.Storage}','{ASSET_STATUS.Lending}') THEN")
            sbSQL.AppendLine($"     CASE WHEN PHYINV_STATUS = '{PHYINV_STATUS.NoSpot}' THEN 1 ELSE 0 END")
            sbSQL.AppendLine($"  ELSE 0 END")
            sbSQL.AppendLine($"  ) OUT_OF_STOCK")
            sbSQL.AppendLine("FROM T_INV_ASSET ia")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AUTH")
            sbSQL.AppendLine("ON  AUTH.LOC_L_CD = ia.LOC_L_CD")
            sbSQL.AppendLine("AND ((AUTH.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND AUTH.LOC_M_CD = ia.LOC_M_CD) OR (AUTH.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
            sbSQL.AppendLine("GROUP BY ia.INV_ID,ia.LOC_L_CD,AUTH.UPD_FLG ")
            sbSQL.AppendLine(") ia")
            sbSQL.AppendLine("ON  INV.INV_ID = ia.INV_ID AND INV.INV_LOC_L_CD = ia.LOC_L_CD ")

            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{authInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{authInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{authInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{authInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthOption", $"{authInfo.AuthOption}")

            sbSQL.AppendLine("WHERE MLCL.LOC_DIV = '" & LOC_DIV.LocationL & "'")

            ' 棚卸未実施
            If _ViewModel.SearchNoStockTaking Then
                sbSQL.AppendLine("AND INV.INV_LOC_L_CD IS NULL ")
            End If

            sbSQL.AppendLine(String.Format(sbWhr.ToString, "MLCL"))


            If isSort Then
                sbSQL.AppendLine("ORDER BY")
                sbSQL.AppendLine("   MLCL.LOC_L_CD")
            End If
            'sbSQL.AppendLine("  ,MAST.LOC_M_CD")
            'sbSQL.AppendLine("  ,MAST.LOC_S_CD")

            Return sbSQL.ToString()
        End Function

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New InventorySummaryViewModel())
            End If

            'ヘッダ部数量取得       2019/10/06 ADD Nicevision Inoue
            'Dim InventorySummaryHedViewModel = GeHedData()   '20231002 DEL Shirakawa

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount

            '20231002 DEL Start Shirakawa
            '_viewResult.ElementAt(0).SHOW_NUMBER_OF_ASSETS = _viewResult.ElementAt(0).NUMBER_OF_ASSETS
            '_viewResult.ElementAt(0).SHOW_NUMBER_OF_STOCKS = _viewResult.ElementAt(0).NUMBER_OF_STOCKS
            '_viewResult.ElementAt(0).SHOW_NUMBER_OF_LOANS = _viewResult.ElementAt(0).NUMBER_OF_LOANS
            '_viewResult.ElementAt(0).SHOW_REAL_SHELF = _viewResult.ElementAt(0).REAL_SHELF
            '_viewResult.ElementAt(0).SHOW_UNCONFIRMED = _viewResult.ElementAt(0).UNCONFIRMED

            '_viewResult.ElementAt(0).SHOW_NUMBER_OF_ASSETS = InventorySummaryHedViewModel.ElementAt(0).NUMBER_OF_ASSETS
            '_viewResult.ElementAt(0).SHOW_NUMBER_OF_STOCKS = InventorySummaryHedViewModel.ElementAt(0).NUMBER_OF_STOCKS
            '_viewResult.ElementAt(0).SHOW_NUMBER_OF_LOANS = InventorySummaryHedViewModel.ElementAt(0).NUMBER_OF_LOANS
            '_viewResult.ElementAt(0).SHOW_REAL_SHELF = InventorySummaryHedViewModel.ElementAt(0).REAL_SHELF
            '_viewResult.ElementAt(0).SHOW_UNCONFIRMED = InventorySummaryHedViewModel.ElementAt(0).UNCONFIRMED
            '20231002 DEL End Shirakawa

            _viewResult.ElementAt(0).CsvPath = _ViewModel.CsvPath
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).START_DEC_TS = _ViewModel.START_DEC_TS
            _viewResult.ElementAt(0).INVENTORY_COUNT = _ViewModel.INVENTORY_COUNT
            _viewResult.ElementAt(0).INVENTORY_MESSAGE = _ViewModel.INVENTORY_MESSAGE
            _viewResult.ElementAt(0).SearchLocL = _ViewModel.SearchLocL


            If String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_SUMMARY, COLSET_LOC.TEMPLATE)
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_SUMMARY, COLSET_LOC.TEMPLATE).FIX_COL_CNT
            Else
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_SUMMARY, _ViewModel.SearchLocL)
                If _viewResult.ElementAt(0).GridInfo.Count > 0 Then
                    _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_SUMMARY, _ViewModel.SearchLocL).FIX_COL_CNT
                Else
                    _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_SUMMARY, COLSET_LOC.TEMPLATE).FIX_COL_CNT
                    _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_SUMMARY, COLSET_LOC.TEMPLATE)
                End If
            End If

            If IsNothing(_LoginInfo) Then
                _viewResult.ElementAt(0).LOGIN_KENGEN = ""
            Else
                _viewResult.ElementAt(0).LOGIN_KENGEN = _LoginInfo.ShosaiAuthDiv
            End If

            _viewResult.ElementAt(0).IsRestoreOwnSearchConditions = _ViewModel.IsRestoreOwnSearchConditions
            _viewResult.ElementAt(0).FirstSearch = _ViewModel.FirstSearch
        End Sub

        ''' <summary>
        ''' ヘッダ表示数量の取得（現在、棚卸開始データが存在するかどうかの判断でのみ、使用）
        ''' </summary>
        Private Sub GetInventryInvConfirm()
            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim viewInventory = New List(Of ISInventoryViewModel)

            sbSQL.Clear()
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("INV_ID")
            sbSQL.AppendLine(",START_DEC_TS")
            sbSQL.AppendLine(",START_DEC_EMPLOYEE_CD")
            sbSQL.AppendLine(",START_DEC_NAME")
            sbSQL.AppendLine(",INV_NAME")
            sbSQL.AppendLine(",INV_CONFIRM_TS")
            sbSQL.AppendLine(",INV_CONFIRM_EMPLOYEE_CD")
            sbSQL.AppendLine(",INV_CONFIRM_NAME")
            sbSQL.AppendLine(",INV_ALL_CONFIRM_TS")
            '20230929 DEL Start Shirakawa
            'sbSQL.AppendLine(",LOGIC_INV_NUM")
            'sbSQL.AppendLine(",PHYINV_NUM")
            'sbSQL.AppendLine(",LOC_DIFF_NUM")
            'sbSQL.AppendLine(",NO_ITEM_NUM")
            '20230929 DEL End Shirakawa
            sbSQL.AppendLine(",UPD_DATETIME")
            sbSQL.AppendLine(",UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(",UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(",UPD_TASK")
            sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM T_INVENTORY WHERE INV_CONFIRM_TS IS NULL")
            viewInventory = db.ISInventoryView.SqlQuery(sbSQL.ToString).ToList
            _ViewModel.INVENTORY_COUNT = viewInventory.Count
            If viewInventory.Count > 0 Then
                _ViewModel.START_DEC_TS = viewInventory(0).START_DEC_TS
            End If

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


#Region "ヘッダ部数量取得"
        ''' <summary>
        ''' ヘッダ部数量取得
        ''' </summary>
        Private Function GeHedData() As List(Of InventorySummaryHedViewModel)

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim InventorySummaryHedViewModel = New List(Of InventorySummaryHedViewModel)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventorySummary, _LoginInfo)

            Dim paramSqlList As List(Of SqlParameter) = New List(Of SqlParameter) 'SQLパラメータ

            sbSQL.Clear()

            Dim sbLoc As StringBuilder = New StringBuilder

            '--ロケーション抽出条件の仮想テーブル
            sbLoc.AppendLine("  (")
            sbLoc.AppendLine("      SELECT")
            sbLoc.AppendLine("         MAST.LOC_L_CD")
            sbLoc.AppendLine("        ,MAST.LOC_M_CD")
            sbLoc.AppendLine("        ,MAST.LOC_S_CD")
            sbLoc.AppendLine("      FROM M_ASSET MAST")
            sbLoc.AppendLine("      INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuth) AUTH")
            sbLoc.AppendLine("      ON  AUTH.LOC_L_CD = MAST.MNG_LOC_L_CD")
            paramSqlList.Add(New SqlParameter("@EmployeeCd", $"{_LoginInfo.EmployeeCd}"))
            paramSqlList.Add(New SqlParameter("@ReadAuthFlg", $"{authInfo.ReadAuthFlg}"))
            paramSqlList.Add(New SqlParameter("@UpdAuthFlg", $"{authInfo.UpdAuthFlg}"))
            paramSqlList.Add(New SqlParameter("@ReadHubAuthFlg", $"{authInfo.ReadHubAuthFlg}"))
            paramSqlList.Add(New SqlParameter("@UpdHubAuthFlg", $"{authInfo.UpdHubAuthFlg}"))
            paramSqlList.Add(New SqlParameter("@UpdAuthType_UpdAuth", $"{UpdAuthType.UPD_AUTH}"))
            sbLoc.AppendLine("      WHERE 1 = 1")

            'ロケーション 大区分
            If Not IsNothing(_ViewModel.SearchLocL) AndAlso _ViewModel.SearchLocL <> "" Then
                sbLoc.AppendLine("      AND MAST.LOC_L_CD = @locLCd")
                paramSqlList.Add(New SqlParameter($"@locLCd", _ViewModel.SearchLocL))
            End If
            'ロケーション 中区分
            If Not IsNothing(_ViewModel.SearchLocM) AndAlso _ViewModel.SearchLocM <> "" Then
                sbLoc.AppendLine("      AND MAST.LOC_M_CD = @locMCd")
                paramSqlList.Add(New SqlParameter($"@locMCd", _ViewModel.SearchLocM))
            End If
            'ロケーション 小区分
            If Not IsNothing(_ViewModel.SearchLocS) AndAlso _ViewModel.SearchLocS <> "" Then
                sbLoc.AppendLine("      AND MAST.LOC_S_CD = @locSCd")
                paramSqlList.Add(New SqlParameter($"@locSCd", _ViewModel.SearchLocS))
            End If
            sbLoc.AppendLine("      GROUP BY")
            sbLoc.AppendLine("         MAST.LOC_L_CD")
            sbLoc.AppendLine("        ,MAST.LOC_M_CD")
            sbLoc.AppendLine("        ,MAST.LOC_S_CD")

            ' 棚卸未実施
            If _ViewModel.SearchNoStockTaking Then
                sbLoc.AppendLine("      HAVING ")
                '直近棚卸日項目
                sbLoc.AppendLine("       (")
                sbLoc.AppendLine("         CASE")
                sbLoc.AppendLine("            WHEN MAX(IsNull(MAST.PHYINV_TS, 0)) = 0 THEN NULL")
                sbLoc.AppendLine("            ELSE CONVERT(DATETIME, LEFT(MAX(MAST.PHYINV_TS), 8), 112)")
                sbLoc.AppendLine("         END")
                sbLoc.AppendLine("       ) IS NULL")
            End If

            sbLoc.AppendLine("      ) TMPLOC")

            '--一覧表示表用SQL
            sbSQL.Clear()
            sbSQL.AppendLine("SELECT '1' AS PKEY")
            '-- ヘッダ部　資産数 SUM(保管、貸出)
            sbSQL.AppendLine("  ,(SELECT IsNull(COUNT_BIG(T_INV_ASSET.ASSET_NO), 0) FROM T_INV_ASSET T_INV_ASSET")
            sbSQL.AppendLine("    INNER JOIN ")
            sbSQL.AppendLine("    " & sbLoc.ToString)
            sbSQL.AppendLine("          ON  TMPLOC.LOC_L_CD = T_INV_ASSET.LOC_L_CD")
            sbSQL.AppendLine("          AND TMPLOC.LOC_M_CD = T_INV_ASSET.LOC_M_CD")
            sbSQL.AppendLine("          AND TMPLOC.LOC_S_CD = T_INV_ASSET.LOC_S_CD")
            sbSQL.AppendLine("          WHERE T_INV_ASSET.ASSET_STATUS IN ('" & ASSET_STATUS.Storage & "','" & ASSET_STATUS.Lending & "')")
            sbSQL.AppendLine("   ) NUMBER_OF_ASSETS")
            '-- ヘッダ部　在庫数 SUM(保管)
            sbSQL.AppendLine("  ,(SELECT IsNull(COUNT_BIG(T_INV_ASSET.ASSET_NO), 0) FROM T_INV_ASSET T_INV_ASSET")
            sbSQL.AppendLine("    INNER JOIN ")
            sbSQL.AppendLine("    " & sbLoc.ToString)
            sbSQL.AppendLine("          ON  TMPLOC.LOC_L_CD = T_INV_ASSET.LOC_L_CD")
            sbSQL.AppendLine("          AND TMPLOC.LOC_M_CD = T_INV_ASSET.LOC_M_CD")
            sbSQL.AppendLine("          AND TMPLOC.LOC_S_CD = T_INV_ASSET.LOC_S_CD")
            sbSQL.AppendLine("          WHERE T_INV_ASSET.ASSET_STATUS = " & ASSET_STATUS.Storage)
            sbSQL.AppendLine("   ) NUMBER_OF_STOCKS")
            '-- ヘッダ部　貸出数 SUM(貸出)
            sbSQL.AppendLine("  ,(SELECT IsNull(COUNT_BIG(T_INV_ASSET.ASSET_NO), 0) FROM T_INV_ASSET T_INV_ASSET")
            sbSQL.AppendLine("    INNER JOIN ")
            sbSQL.AppendLine("    " & sbLoc.ToString)
            sbSQL.AppendLine("          ON  TMPLOC.LOC_L_CD = T_INV_ASSET.LOC_L_CD")
            sbSQL.AppendLine("          AND TMPLOC.LOC_M_CD = T_INV_ASSET.LOC_M_CD")
            sbSQL.AppendLine("          AND TMPLOC.LOC_S_CD = T_INV_ASSET.LOC_S_CD")
            sbSQL.AppendLine("          WHERE T_INV_ASSET.ASSET_STATUS = " & ASSET_STATUS.Lending)
            sbSQL.AppendLine("   ) NUMBER_OF_LOANS")
            '-- ヘッダ部　実棚 SUM(正常、警告）
            sbSQL.AppendLine("  ,(SELECT IsNull(COUNT_BIG(T_INV_ASSET.ASSET_NO), 0) FROM T_INV_ASSET T_INV_ASSET")
            sbSQL.AppendLine("    INNER JOIN ")
            sbSQL.AppendLine("    " & sbLoc.ToString)
            sbSQL.AppendLine("          ON  TMPLOC.LOC_L_CD = T_INV_ASSET.LOC_L_CD")
            sbSQL.AppendLine("          AND TMPLOC.LOC_M_CD = T_INV_ASSET.LOC_M_CD")
            sbSQL.AppendLine("          AND TMPLOC.LOC_S_CD = T_INV_ASSET.LOC_S_CD")
            sbSQL.AppendLine("          WHERE T_INV_ASSET.PHYINV_STATUS IN ('" & PHYINV_STATUS.Normal & "', '" & PHYINV_STATUS.Warning & "')")
            sbSQL.AppendLine("   ) REAL_SHELF")
            '-- ヘッダ部　未確認 SUM(現物無し)
            sbSQL.AppendLine("  ,(SELECT IsNull(COUNT_BIG(T_INV_ASSET.ASSET_NO), 0) FROM T_INV_ASSET T_INV_ASSET")
            sbSQL.AppendLine("    INNER JOIN ")
            sbSQL.AppendLine("    " & sbLoc.ToString)
            sbSQL.AppendLine("          ON  TMPLOC.LOC_L_CD = T_INV_ASSET.LOC_L_CD")
            sbSQL.AppendLine("          AND TMPLOC.LOC_M_CD = T_INV_ASSET.LOC_M_CD")
            sbSQL.AppendLine("          AND TMPLOC.LOC_S_CD = T_INV_ASSET.LOC_S_CD")
            sbSQL.AppendLine("          WHERE T_INV_ASSET.PHYINV_STATUS = '" & PHYINV_STATUS.NoSpot & "'")
            sbSQL.AppendLine("   ) UNCONFIRMED")

            sbSQL.AppendLine("  ,COUNT(*) OVER () AS TotalCount ")

            InventorySummaryHedViewModel = db.InventorySummaryHedView.SqlQuery(sbSQL.ToString, paramSqlList.ToArray).ToList

            'データを返却
            Return InventorySummaryHedViewModel
        End Function
#End Region

    End Class
End Namespace