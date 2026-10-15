Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Data.SqlClient
Imports Asetra.Dao
Imports System.Globalization

Namespace Controllers

    ''' <summary>
    ''' 予約状況照会画面表示用のクラス
    ''' </summary>
    Public Class LendingStatusController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        Private _back As List(Of LendingStatusViewModel)

#Region "クラス定数"
#End Region

#Region "クラス変数"
        'Private _Model As LendingStatusModel = New LendingStatusModel                                   'LendingStatusモデル
        Private _ViewModel As LendingStatusViewModel = New LendingStatusViewModel                       'LendingStatus一覧表示用モデル
        Protected _viewResult As List(Of LendingStatusViewModel) = New List(Of LendingStatusViewModel)  '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _isSort As Boolean = True                       'ソートフラグ

        '2022/7/25 sv sato add start
        Private _LendEntryViewModel As LendingEntryViewModel = New LendingEntryViewModel                     'LendingEntry一覧表示用モデル
        Private _LendEntryModel As LendingEntryModel = New LendingEntryModel                            'LendingEntryモデル
        Private _CsvImportItems As List(Of ReserveCsvImportModel) = New List(Of ReserveCsvImportModel)                'CSV取込項目リスト

        'DB関数 資産メンテナンス登録情報取得　の戻り値のDataset  CreateChcek() でNew 
        Private _dsMentInfo As DataSet
        '2022/7/25 sv sato add end

        ''' <summary>
        ''' CSV取込項目(資産番号以外)
        ''' </summary>
        Private Enum CsvImportItem
            PROCESS_DIV
            RESERVE_ID
            REQ_DATE
            REQ_EMP_CD
            CHARGE_EMP_CD
            REAL_START_DT
            REAL_END_DT
            LEASE_PLACE_CD
            LEASE_USE
            DEST_NAME1
            DEST_NAME2
            TO_DEST_NAME    '2023/03/22 Kawamoto ADD
            DEST_ADDRESS1
            DEST_ADDRESS2
            DEST_ADDRESS3
            DEST_PHONE_NUM
            DEST_ZIP_CODE
            '2023/03/22 Kawamoto ADD START
            RET_LEASE_PLACE_CD
            RET_ADDRESS1
            RET_ADDRESS2
            RET_ADDRESS3
            RET_ZIP_CODE
            '2023/03/22 Kawamoto ADD END
            REMARK
            REMARK_INTERNAL
            DEST_NO
        End Enum
        '2022/7/25 sv sato add end
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
            If Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) Is Nothing Then
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                'カゴ情報をクリアする
                Session.Remove(CommonSessionKey.CREAGE_CART_INFO)
            End If

            If Session(CommonSessionKey.SEARCH_FLG) IsNot Nothing Then
                Session.Remove(CommonSessionKey.SEARCH_FLG)
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
        Public Function Index(ByVal ViewModel As LendingStatusViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_LENDING_STATUS)
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
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Lending_Ref.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Lending_Ref.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Lending_Ref.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Lending_Ref.Key3)
            '20230901 ADD End Shirakawa

            '設定マスタ メンテナンス区分設定取得　メンテナンス
            Dim reserveMainteStatusM As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Reserve_Mainte_Status_Setting_M.KeyDiv,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_M.Key1,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_M.Key2,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_M.Key3)

            If reserveMainteStatusM Is Nothing Then
                'メンテナンス区分 メンテナンス 色取得
                ViewBag.mainteStatusMColor = CalendarColor.Mainte
                'メンテナンス区分 メンテナンス 表示区分
                ViewBag.mainteStatusMDisp = "Y"
            Else
                'メンテナンス区分 メンテナンス 色取得
                ViewBag.mainteStatusMColor = reserveMainteStatusM.DATA2
                'メンテナンス区分 メンテナンス 表示区分
                ViewBag.mainteStatusMDisp = reserveMainteStatusM.DATA1
            End If

            '設定マスタ メンテナンス区分設定取得　〇〇登録
            Dim reserveMainteStatusO As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Reserve_Mainte_Status_Setting_O.KeyDiv,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_O.Key1,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_O.Key2,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_O.Key3)

            If reserveMainteStatusO Is Nothing Then
                'メンテナンス区分 〇〇登録 色取得
                ViewBag.mainteStatusOColor = "background-color:#5AFF19"
                'メンテナンス区分 〇〇登録 表示区分
                ViewBag.mainteStatusODisp = "N"
            Else
                'メンテナンス区分 〇〇登録 色取得
                ViewBag.mainteStatusOColor = reserveMainteStatusO.DATA2
                'メンテナンス区分 〇〇登録 表示区分
                ViewBag.mainteStatusODisp = reserveMainteStatusO.DATA1
            End If

            '設定マスタ メンテナンス区分設定取得　予備登録１
            Dim reserveMainteStatusP As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Reserve_Mainte_Status_Setting_P.KeyDiv,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_P.Key1,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_P.Key2,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_P.Key3)

            If reserveMainteStatusP Is Nothing Then
                'メンテナンス区分 予備登録１ 色取得
                ViewBag.mainteStatusPColor = "background-color:#EC008C"
                'メンテナンス区分 予備登録１ 表示区分
                ViewBag.mainteStatusPDisp = "N"
            Else
                'メンテナンス区分 予備登録１ 色取得
                ViewBag.mainteStatusPColor = reserveMainteStatusP.DATA2
                'メンテナンス区分 予備登録１ 表示区分
                ViewBag.mainteStatusPDisp = reserveMainteStatusP.DATA1
            End If


            '設定マスタ メンテナンス区分設定取得　予備登録２
            Dim reserveMainteStatusQ As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Reserve_Mainte_Status_Setting_Q.KeyDiv,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_Q.Key1,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_Q.Key2,
                                                                                  CommonSettingKey.Reserve_Mainte_Status_Setting_Q.Key3)

            If reserveMainteStatusQ Is Nothing Then
                'メンテナンス区分 予備登録２ 色取得
                ViewBag.mainteStatusQColor = "background-color:#F58220"
                'メンテナンス区分 予備登録２ 表示区分
                ViewBag.mainteStatusQDisp = "N"
            Else
                'メンテナンス区分 予備登録２ 色取得
                ViewBag.mainteStatusQColor = reserveMainteStatusQ.DATA2
                'メンテナンス区分 予備登録２ 表示区分
                ViewBag.mainteStatusQDisp = reserveMainteStatusQ.DATA1
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)


            '凡例設定取得
            Call SubGetLegendDispSettingAndWidthDecision(settingInfo)



            '検索ボタンがクリックされた場合、選択状態のセッションをクリアする
            If _Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then

                Session(CommonSessionKey.SEARCH_FLG) = True

            End If

            '照合機能設定を取得
            Dim MatchingFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.MatchingFunction.KeyDiv,
                                                                         CommonSettingKey.MatchingFunction.Key1,
                                                                         CommonSettingKey.MatchingFunction.Key2,
                                                                         CommonSettingKey.MatchingFunction.Key3, True)

            If Not IsNothing(MatchingFunc) AndAlso MatchingFunc.DATA1 = ValidFlg.Cd.Valid Then
                '照合機能あり
                ViewBag.MatchingFuncFlg = ValidFlg.Cd.Valid
            End If

            '予約機能設定を取得
            Dim ReserveFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv,
                                                                        CommonSettingKey.ReservationFunction.Key1,
                                                                        CommonSettingKey.ReservationFunction.Key2,
                                                                        CommonSettingKey.ReservationFunction.Key3, True)

            If Not IsNothing(ReserveFunc) AndAlso ReserveFunc.DATA1 = ValidFlg.Cd.Valid Then
                '予約機能あり
                ViewBag.ReserveFuncFlg = ValidFlg.Cd.Valid
            End If

            '照合予約機能設定を取得
            Dim MatchingReserveFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.MatchingReserveFunction.KeyDiv,
                                                                        CommonSettingKey.MatchingReserveFunction.Key1,
                                                                        CommonSettingKey.MatchingReserveFunction.Key2,
                                                                        CommonSettingKey.MatchingReserveFunction.Key3, True)

            If Not IsNothing(MatchingReserveFunc) Then
                '照合予約機能あり
                ViewBag.MatchingReserveDetail = MatchingReserveFunc.DATA3
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As LendingStatusViewModel = Session(CommonSessionKey.CREAGE_LENDING_STATUS)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'リダイレクト用モデルの設定
            Dim modelRedirect As LendingStatusViewModel = Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT)
            If Not IsNothing(modelRedirect) Then
                _ViewModel = modelRedirect
                Session.Remove(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT)
            End If

            'セッションのカゴ情報を取得する
            Dim lstCartInfo As New List(Of CartInfo)
            If Not IsNothing(Session(CommonSessionKey.CREAGE_CART_INFO)) Then
                lstCartInfo = Session(CommonSessionKey.CREAGE_CART_INFO)
            End If
            '選択数
            ViewBag.CartSelectNumber = lstCartInfo.Count

            'メッセージ表示用に予約申請の明細数を取得する
            Dim lendingNumberItems As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Number_Items.KeyDiv, CommonSettingKey.Lending_Number_Items.Key1, CommonSettingKey.Lending_Number_Items.Key2, CommonSettingKey.Lending_Number_Items.Key3, True)
            Dim iLendingNumber As Integer = Integer.Parse(lendingNumberItems.DATA1)
            ViewBag.LendingNumber = iLendingNumber

            '予約状況照会画面の設定取得
            Dim lendingStatusSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Status_Setting.KeyDiv, CommonSettingKey.Lending_Status_Setting.Key1, CommonSettingKey.Lending_Status_Setting.Key2, CommonSettingKey.Lending_Status_Setting.Key3, True)
            '表示月数
            _ViewModel.DspMonthCount = Integer.Parse(lendingStatusSetting.DATA2)

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '管理ロケーションチェックボックスリスト作成
                _ViewModel.mngLocCheckBoxListData = GetMngLocData()

                '検索条件の初期値設定
                If String.IsNullOrEmpty(_ViewModel.SelectMngLocKeys) Then
                    _ViewModel.SearchLeaseStatus_NoReserve = True
                    _ViewModel.SearchLeaseStatus_NotRented = True
                    _ViewModel.SearchLeaseStatus_Lending = True
                    _ViewModel.SearchLeaseStatus_LendingDelay = False
                    _ViewModel.SearchLeaseStatus_Returned = True
                    _ViewModel.SearchLeaseStatus_Mainte = True
                    _ViewModel.SearchLeaseStatus_BlaRegist = True
                    _ViewModel.SearchLeaseStatus_PreRegist1 = True
                    _ViewModel.SearchLeaseStatus_PreRegist2 = True
                    _ViewModel.SearchShippingNotification_NotNotified = True
                    _ViewModel.SearchShippingNotification_Notified = True
                    _ViewModel.SearchSort = LendingStatusSort.Reserve
                    '表示開始日：システム日時 + 表示開始日補正月数
                    Dim iLeadTime As Decimal = Integer.Parse(lendingStatusSetting.DATA1)
                    _ViewModel.SearchDspStartDate = Now.AddMonths(iLeadTime).ToString("yyyy-MM-dd")
                    '管理ロケーションの選択キーリスト
                    _ViewModel.SelectMngLocKeys = GetDefaultSelectMngLocKeys()
                End If

                'カレンダー表示用の表示開始日
                ViewBag.DspStartDate = _ViewModel.SearchDspStartDate

                '権限情報の取得
                Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)
                If authInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                Else
                    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                End If
                'メンテナンス登録ボタン表示可否
                Dim isExistAuthOption_Mainte As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_MAINTE)
                Dim isExistAuthOption_MainteHub As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_MAINTEHUB)
                If isExistAuthOption_Mainte OrElse isExistAuthOption_MainteHub Then
                    ViewBag.CanDspMainteEntry = True
                Else
                    ViewBag.CanDspMainteEntry = False
                End If

                '2022/7/25 sv sato add start
                'CSV取込ボタン表示可否
                Dim reserveRegOption As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reserve_Regist_Option.KeyDiv, CommonSettingKey.Reserve_Regist_Option.Key1, CommonSettingKey.Reserve_Regist_Option.Key2, CommonSettingKey.Reserve_Regist_Option.Key3, True)

                If reserveRegOption.DATA2 = ValidFlg.Cd.Valid Then
                    '表示切替2が有効の場合
                    ViewBag.CanDspOptionData2 = True
                Else
                    '表示切替2が無効の場合
                    ViewBag.CanDspOptionData2 = False
                End If

                If authInfo.AuthOption.Contains(AuthOptionName.U_ADMIN) AndAlso reserveRegOption.DATA1 = ValidFlg.Cd.Valid Then
                    '管理者 かつ 設定Mの表示有無が有効の場合
                    ViewBag.CanDspCsvImport = True
                Else
                    ViewBag.CanDspCsvImport = False
                End If
                '2022/7/25 sv sato add end

                '一覧表示処理（管理ロケーションチェックボックスリストが存在しない場合は、初期表示と判断し、グリッド表示しない）  nv Inoue
                If Not String.IsNullOrEmpty(_ViewModel.SelectMngLocKeys) Then
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
                End If
                'MyBase.BaseIndex(_ViewModel)

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

            ' 大分類ドロップダウンリスト作成
            GetCategoryLData()
            ' 中分類
            GetCategoryMData(_ViewModel.SearchCatL)
            ' 小分類
            GetCategorySData(_ViewModel.SearchCatL, _ViewModel.SearchCatM)

            '一覧情報にページ情報を設定
            CopyModelData()

            Session.Add("ViewModel", _viewResult)

            '結果表示
            Return View(_viewResult)

        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()

            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_LENDING_STATUS)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()

            MyBase._SQL = CreateSQL()

            If _ViewModel.SearchLeaseStatus_LendingDelay Then
                Dim reSearchFlg = False
                If _isSort Then
                    ViewQueryPager(_ViewModel.PageNum, _ViewModel)
                End If
                'SQLを実行
                Dim db = New SystemDBContext
                Dim query As DbSqlQuery(Of LendingStatusViewModel) = db.LendingStatusView.SqlQuery(_SQL, _paramSqlList.ToArray)
                _viewResult = query.ToList
                db.Dispose()

                If _viewResult.Count > 0 Then
                    reSearchFlg = True
                End If

                'クエリを返却
                _paramSqlList.Clear()           'SQLパラメータのクリア
                '貸出中(遅延)がチェックされている場合再検索
                MyBase._SQL = CreateSQL(reSearchFlg)
            End If
        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of LendingStatusViewModel) = _db.LendingStatusView.SqlQuery(_SQL, _paramSqlList.ToArray)

            '画面に表示するViewを設定
            _viewResult = query.ToList

        End Sub

#End Region

#Region "登録・更新処理"
        ''' <summary>
        ''' CSV取込データ登録処理 2022/7/25 sv sato add
        ''' ※引継明細関連は予約申請登録画面から引用したデッドロジック
        ''' </summary>
        ''' <param name="entryCsvModel">登録・更新データ</param>
        ''' <param name="procDiv">処理区分</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Create(ByVal entryCsvModel As LendingEntryViewModel, ByVal procDiv As String) As Boolean
            Try
                '登録用データ(1レコード分)をクラス変数に保持
                _LendEntryViewModel = entryCsvModel

                Dim lstReserveDetail As List(Of ReserveDetailViewModel) = entryCsvModel.ReserveDetailData

                '変更処理の場合、データベースのモデルを取得
                Dim lstReserveDetailOld As New List(Of ReserveDetailViewModel)
                Dim db As New SystemDBContext
                If procDiv = CsvImportProcDiv.Update Then
                    '予約明細テーブルを取得
                    Dim sbSQL As StringBuilder = New StringBuilder
                    Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
                    'T_RESERVE_DETAILテーブルから取得するSQLを作成
                    sbSQL.AppendLine("SELECT RESERVE.*")
                    sbSQL.AppendLine("       ,ASSET.ASSET_NAME As AssetName")
                    sbSQL.AppendLine("       ,ASSET.FREE_ITEM10 As SerialNo")
                    sbSQL.AppendLine("       ,EMP.EMPLOYEE_NAME As EmployeeName")
                    sbSQL.AppendLine("FROM T_RESERVE_DETAIL RESERVE")
                    sbSQL.AppendLine("INNER JOIN M_ASSET ASSET")
                    sbSQL.AppendLine("ON ASSET.ASSET_NO = RESERVE.ASSET_NO ")
                    sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE EMP")
                    sbSQL.AppendLine("ON EMP.EMPLOYEE_CD = RESERVE.CHARGE_EMP_CD ")
                    sbSQL.AppendLine("WHERE")
                    sbSQL.AppendLine("RESERVE.RESERVE_ID = @ReserveId")
                    params.Add(New SqlParameter("@ReserveId", entryCsvModel.RESERVE_ID))
                    sbSQL.AppendLine("ORDER BY RESERVE.ASSET_NO")
                    'SQLを実行する
                    lstReserveDetailOld = _db.ReserveDetailView.SqlQuery(sbSQL.ToString, params.ToArray).ToList
                End If


                '実貸出開始日・実貸出終了日、ユーザー貸出開始日・ユーザー貸出貸出終了日、○○期限、申請日を設定する
                If String.IsNullOrEmpty(entryCsvModel.RealStartDtString) Then
                    entryCsvModel.REAL_START_DT = 0
                Else
                    entryCsvModel.REAL_START_DT = CInt(CDate(entryCsvModel.RealStartDtString).ToString("yyyyMMdd"))
                End If
                If String.IsNullOrEmpty(entryCsvModel.RealEndDtString) Then
                    entryCsvModel.REAL_END_DT = 0
                Else
                    entryCsvModel.REAL_END_DT = CInt(CDate(entryCsvModel.RealEndDtString).ToString("yyyyMMdd"))
                End If
                If String.IsNullOrEmpty(entryCsvModel.UserStartDtString) Then
                    entryCsvModel.USER_START_DT = 0
                Else
                    entryCsvModel.USER_START_DT = CInt(CDate(entryCsvModel.UserStartDtString).ToString("yyyyMMdd"))
                End If
                If String.IsNullOrEmpty(entryCsvModel.UserEndDtString) Then
                    entryCsvModel.USER_END_DT = 0
                Else
                    entryCsvModel.USER_END_DT = CInt(CDate(entryCsvModel.UserEndDtString).ToString("yyyyMMdd"))
                End If
                entryCsvModel.REQUEST_DT = CInt(CDate(CommonRfid.getNowDate).ToString("yyyyMMdd"))

                '登録・更新内容のログ出力  
                Dim sEntryType As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU)
                For icnt As Integer = 0 To lstReserveDetail.Count - 1
                    Dim sLogEntry As String = sEntryType & " " &
                             "申請者コード：" & entryCsvModel.REQ_EMP_CD & " " &
                             "ヘッダ実貸出開始日：" & entryCsvModel.REAL_START_DT & " " &
                             "ヘッダ実貸出終了日：" & entryCsvModel.REAL_END_DT & " " &
                             "資産番号：" & lstReserveDetail(icnt).ASSET_NO & " " &
                             "資産名：" & lstReserveDetail(icnt).AssetName & " " &
                             "実貸出開始日：" & lstReserveDetail(icnt).REAL_START_DT & " " &
                             "実貸出終了日：" & lstReserveDetail(icnt).REAL_END_DT & " " &
                             Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
                             CommonRfid.ToStringNvl(entryCsvModel.RESERVE_ID).Trim
                    logger.Info(sLogEntry)
                Next

                '※ 更新時は対象データを削除してから登録する
                If procDiv = CsvImportProcDiv.Update Then
                    If Delete(entryCsvModel, False) = False Then
                        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                        Return False
                    End If
                End If

                '登録処理
                MyBase.BaseCreate(entryCsvModel)

                '正常時、正常終了のステータスを返す
                If String.IsNullOrEmpty(entryCsvModel.ErrCode) Then

                    '※ 連続での登録を可能にする為にデタッチ
                    _db.Entry(_LendEntryModel).State = Entity.EntityState.Detached

                    Dim sTaskNm As String

                    '更新した予約IDを取得
                    Dim lReserveId As Long = _LendEntryModel.RESERVE_ID


                    If procDiv = CsvImportProcDiv.Insert Then
                        '新規登録時
                        sTaskNm = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_XI)
                    Else
                        '変更時
                        sTaskNm = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_XU)
                    End If


                    '予約明細テーブルの削除
                    Dim sbSQL_DelDetail As StringBuilder = New StringBuilder
                    Dim params_DelDetail As List(Of SqlParameter) = New List(Of SqlParameter)
                    sbSQL_DelDetail.AppendLine("DELETE FROM T_RESERVE_DETAIL WHERE RESERVE_ID = @ReserveId")
                    params_DelDetail.Add(New SqlParameter("@ReserveId", lReserveId))
                    _db.Database.ExecuteSqlCommand(sbSQL_DelDetail.ToString, params_DelDetail.ToArray)

                    ''グリッドの情報を取得    
                    Dim model As List(Of ReserveDetailModel) = New List(Of ReserveDetailModel)    '登録用モデル

                    '処理済引継先予約Noのリスト（予約を複数回更新しようとするとエラーになるため）
                    Dim lstFinReserveId As New List(Of Long)

                    '登録用モデルにデータを入れる
                    For icnt As Integer = 0 To lstReserveDetail.Count - 1
                        model.Add(New ReserveDetailModel())
                        model(icnt).RESERVE_ID = lReserveId
                        model(icnt).ASSET_NO = lstReserveDetail(icnt).ASSET_NO
                        model(icnt).REAL_START_DT = lstReserveDetail(icnt).REAL_START_DT
                        model(icnt).REAL_END_DT = lstReserveDetail(icnt).REAL_END_DT
                        model(icnt).HANDOVER_DIV = lstReserveDetail(icnt).HANDOVER_DIV
                        model(icnt).HANDOVER_RESERVE_ID = lstReserveDetail(icnt).HANDOVER_RESERVE_ID
                        '引継元明細の場合
                        If lstReserveDetail(icnt).HANDOVER_DIV = HandOverDiv.HikitsugiMoto Then
                            '前回登録時、該当資産番号が引継元明細だったかどうかをチェック
                            Dim sAssetNo As String = lstReserveDetail(icnt).ASSET_NO
                            '既存のデータベースから該当資産情報を取得
                            Dim lstModelSelDetail As ReserveDetailViewModel() = (From motoOld In lstReserveDetailOld
                                                                                 Where (motoOld.ASSET_NO = sAssetNo)
                                                                                 Order By motoOld.ASSET_NO).ToArray
                            If lstModelSelDetail(0).HANDOVER_RESERVE_ID Is Nothing Then
                                '前回登録時、該当資産番号が引継明細でなかった場合
                                '予約モデルを取得。
                                Dim dbNext As New SystemDBContext

                                '更新予約をチェックし実行する
                                If lstFinReserveId.IndexOf(lstReserveDetail(icnt).HANDOVER_RESERVE_ID.Value) = -1 Then
                                    Dim modelNextReserve As LendingEntryModel = dbNext.LendingEntry.Find(lstReserveDetail(icnt).HANDOVER_RESERVE_ID.Value)
                                    'modelNextReserve.RESERVE_STATUS = ReserveStatus.Lending
                                    'アタッチする(予約)　　予約と予約明細の更新タイミングを分ける
                                    _db.Entry(modelNextReserve).State = Entity.EntityState.Modified

                                    lstFinReserveId.Add(lstReserveDetail(icnt).HANDOVER_RESERVE_ID.Value)
                                End If

                                '予約明細モデルを取得。
                                Dim modelNextReserveDetail As ReserveDetailModel = dbNext.ReserveDetail.Find(lstReserveDetail(icnt).HANDOVER_RESERVE_ID, lstReserveDetail(icnt).ASSET_NO, lstReserveDetail(icnt).NextRealStartDt)
                                Dim iNextRealStartDt As Integer = 0
                                If lstReserveDetail.Count - 1 >= icnt + 1 AndAlso lstReserveDetail(icnt + 1).HANDOVER_DIV = HandOverDiv.HikitsugiSaki Then
                                    '引継先明細がある場合、引継先明細の実貸出終了日＋１の日付をセット。
                                    iNextRealStartDt = CInt(CDate(CommonRfid.ToStringDateYMD(lstReserveDetail(icnt + 1).REAL_END_DT, "yyyyMMdd")).AddDays(1).ToString("yyyyMMdd"))
                                Else
                                    '引継先明細がない場合、引継元明細の実貸出終了日＋１の日付をセット。
                                    iNextRealStartDt = CInt(CDate(CommonRfid.ToStringDateYMD(lstReserveDetail(icnt).REAL_END_DT, "yyyyMMdd")).AddDays(1).ToString("yyyyMMdd"))
                                End If
                                modelNextReserveDetail.LENDING_DT = iNextRealStartDt
                                modelNextReserveDetail.PR_ST_DT = iNextRealStartDt
                                modelNextReserveDetail.RESERVE_STATUS = ReserveStatus.Lending

                                'アタッチする(予約明細)
                                _db.Entry(modelNextReserveDetail).State = Entity.EntityState.Modified

                                '引継元の資産の実貸出終了日から、次の予約の実貸出開始日までの間にあるメンテナンスデータは削除する。
                                Dim sbSQL_DelMainte As StringBuilder = New StringBuilder
                                Dim params_DelMainte As List(Of SqlParameter) = New List(Of SqlParameter)
                                sbSQL_DelMainte.AppendLine("DELETE FROM T_RESERVE_MAINTE WHERE ASSET_NO = @AssetNo AND MAINTE_START_DT < @NextRealStartDt AND @RealEndDt < MAINTE_END_DT")
                                params_DelMainte.Add(New SqlParameter("@AssetNo", model(icnt).ASSET_NO))
                                params_DelMainte.Add(New SqlParameter("@NextRealStartDt", lstReserveDetail(icnt).NextRealStartDt))
                                params_DelMainte.Add(New SqlParameter("@RealEndDt", lstReserveDetail(icnt).REAL_END_DT))
                                _db.Database.ExecuteSqlCommand(sbSQL_DelMainte.ToString, params_DelMainte.ToArray)

                                '引継明細作成時、前予約から引継された明細であれば、前予約の引継先明細の予約ステータスを「返却済」に変更する処理
                                UpdBeforeReserve(Me._db, _LendEntryModel.RESERVE_ID, lstReserveDetail(icnt).ASSET_NO)
                            End If
                        End If

                        '引継先明細の場合
                        If lstReserveDetail(icnt).HANDOVER_DIV = HandOverDiv.HikitsugiSaki Then
                            'グリッドの担当者をセットする。
                            model(icnt).CHARGE_EMP_CD = lstReserveDetail(icnt).CHARGE_EMP_CD
                        Else
                            '担当者コンボの担当者をセットする。
                            model(icnt).CHARGE_EMP_CD = _LendEntryModel.CHARGE_EMP_CD
                        End If
                        model(icnt).UPD_TASK = sTaskNm

                        model(icnt).LENDING_DT = lstReserveDetail(icnt).LENDING_DT
                        model(icnt).RETURN_DT = lstReserveDetail(icnt).RETURN_DT
                        If lstReserveDetail(icnt).RESERVE_STATUS = ReserveStatus.Mi Then
                            '未貸出の場合、実質貸出開始日をセット
                            model(icnt).PR_ST_DT = model(icnt).REAL_START_DT
                        ElseIf lstReserveDetail(icnt).RESERVE_STATUS = ReserveStatus.Lending Then
                            '貸出中の場合、貸出日をセット
                            model(icnt).PR_ST_DT = model(icnt).LENDING_DT
                        Else
                            'それ以外は、そのままセット
                            model(icnt).PR_ST_DT = lstReserveDetail(icnt).PR_ST_DT
                        End If
                        If lstReserveDetail(icnt).RESERVE_STATUS = ReserveStatus.HenkyakuSumi Then
                            '返却済みの場合、返却日をセット
                            model(icnt).PR_END_DT = model(icnt).RETURN_DT
                        Else
                            'それ以外の場合、実質貸出終了日をセット
                            model(icnt).PR_END_DT = model(icnt).REAL_END_DT
                        End If

                        '期間延長回数
                        model(icnt).LEASE_CHG_TIMES = lstReserveDetail(icnt).LEASE_CHG_TIMES
                        '予約変更可能日数を取得する。
                        Dim Setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Update_Enabled.KeyDiv, CommonSettingKey.Lending_Update_Enabled.Key1, CommonSettingKey.Lending_Update_Enabled.Key2, CommonSettingKey.Lending_Update_Enabled.Key3, True)
                        Dim sUpdateOkDays As Integer = Setting.DATA1
                        Dim sWorkflg As String = Setting.DATA2
                        '対象日付取得(予約変更可能日付)
                        Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, sUpdateOkDays * -1, sWorkflg)   '稼働日フラグを設定マスタから取る

                        '予約変更可能日付 ＞ 実質貸出期間Fromで、かつ実質貸出期間Toがデータベースの値より大きい値に変更されていた場合にカウントアップ。（引継データは除く）
                        If iTargetDate > model(icnt).REAL_START_DT AndAlso model(icnt).HANDOVER_DIV = HandOverDiv.Mi Then
                            For Each row As ReserveDetailViewModel In lstReserveDetailOld
                                If row.ASSET_NO = model(icnt).ASSET_NO Then
                                    '実貸出終了日がデータベースの値より大きい値に変更されていた場合にカウントアップする。
                                    If row.REAL_END_DT < model(icnt).REAL_END_DT Then
                                        model(icnt).LEASE_CHG_TIMES = lstReserveDetail(icnt).LEASE_CHG_TIMES + 1
                                    End If
                                    Exit For
                                End If
                            Next
                        End If

                        model(icnt).RESERVE_STATUS = lstReserveDetail(icnt).RESERVE_STATUS
                        model(icnt).INS_DATETIME = CommonRfid.getNowDate
                        model(icnt).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(icnt).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(icnt).UPD_DATETIME = CommonRfid.getNowDate
                        model(icnt).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(icnt).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                    Next

                    'システムコンテキストにモデルを追加
                    _db.ReserveDetail.AddRange(model)

                    'DB更新用の処理区分を取得
                    Dim hisProcDiv As String = String.Empty
                    If procDiv = CsvImportProcDiv.Insert Then
                        '新規登録時
                        hisProcDiv = ProcessDiv.XI
                    Else
                        '変更時
                        hisProcDiv = ProcessDiv.XU
                    End If

                    'システムコンテキストに予約履歴モデルを追加。
                    InsertReserveLog(_LendEntryModel, hisProcDiv, entryCsvModel.ReserveDetailData)

                    'システムコンテキストに資産履歴モデルを追加。
                    For Each row As ReserveDetailViewModel In entryCsvModel.ReserveDetailData
                        Dim modelAsset As AssetListModel = db.AssetList.Find(row.ASSET_NO)
                        InsertAssetHistory(modelAsset, hisProcDiv)
                    Next

                    '今回削除された明細の資産のメンテナンスデータは削除する。
                    For Each row As ReserveDetailViewModel In lstReserveDetailOld
                        'グリッドデータから該当資産情報を取得
                        Dim lstModelSelDetail As ReserveDetailViewModel() = (From selModel In lstReserveDetail
                                                                             Where (selModel.ASSET_NO = row.ASSET_NO)
                                                                             Order By selModel.ASSET_NO).ToArray
                        If lstModelSelDetail.Length = 0 Then
                            '明細が削除されていた場合
                            Dim sbSQL_DelMei As StringBuilder = New StringBuilder
                            Dim params_DelMei As List(Of SqlParameter) = New List(Of SqlParameter)
                            sbSQL_DelMei.AppendLine("DELETE FROM T_RESERVE_MAINTE WHERE ASSET_NO = @AssetNo AND RESERVE_ID = @ReserveId")
                            params_DelMei.Add(New SqlParameter("@AssetNo", row.ASSET_NO))
                            params_DelMei.Add(New SqlParameter("@ReserveId", row.RESERVE_ID))
                            _db.Database.ExecuteSqlCommand(sbSQL_DelMei.ToString, params_DelMei.ToArray)
                        End If
                    Next

                    '登録処理を実行
                    _db.SaveChanges()

                    '資産メンテナンス登録処理　
                    Call Dao.ToolReserve.UpdAssetMainte(Me._db, _LendEntryModel.RESERVE_ID, lstReserveDetail, Me._dsMentInfo, Me._LoginInfo, sTaskNm)
                    Me._dsMentInfo.Dispose()

                    If procDiv = CsvImportProcDiv.Insert Then

                        '操作ログ出力
                        Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU) & " " &
                                                         Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                         Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
                                                         CommonRfid.ToStringNvl(_LendEntryModel.RESERVE_ID).Trim
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU))

                        entryCsvModel.States = StatusType.SuccessCreate

                    Else

                        '操作ログ出力
                        Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU) & " " &
                                                         Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                         Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
                                                         CommonRfid.ToStringNvl(_LendEntryModel.RESERVE_ID).Trim
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU))

                        entryCsvModel.States = StatusType.SuccessEdit
                    End If
                End If

                Return True
            Catch ex As Exception
                logger.Error(ex)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeCreate()
            '親の初期処理
            MyBase.InitializeCreate()

            'モデルを登録用モデルにコピー
            CopyViewModeltoModel(_LendEntryViewModel)
        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' ※チェック処理はCSV取込時に実施するため、登録時はチェックなし
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

            '登録日時・登録社員コード・登録社員名・登録プロセスを登録
            _LendEntryModel.INS_DATETIME = CommonRfid.getNowDate
            _LendEntryModel.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _LendEntryModel.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _LendEntryModel.UPD_DATETIME = CommonRfid.getNowDate
            _LendEntryModel.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _LendEntryModel.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _LendEntryModel.UPD_TASK = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_XI)

            _db.LendingEntry.Add(_LendEntryModel)

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' ※実行しないため空
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Return blnReturn
        End Function

        ''' <summary>
        ''' 更新処理のコンテキストを変更します　2022/07/25 sv sato add
        ''' ※実行しないため空
        ''' </summary>
        Protected Overrides Sub EditQuery()
        End Sub
#End Region

#Region "削除処理"
        ''' <summary>
        ''' 削除処理
        ''' </summary>
        ''' <param name="entryCsvModel"></param>
        ''' <returns></returns>
        Public Function Delete(ByVal entryCsvModel As LendingEntryViewModel, Optional isHisInsert As Boolean = True) As Boolean
            Try
                'クラス変数に保持
                _LendEntryViewModel = entryCsvModel

                '削除処理
                MyBase.BaseDelete(entryCsvModel)

                '正常時、正常終了のステータスを返す
                If String.IsNullOrEmpty(_ViewModel.ErrCode) Then

                    '※本来はDeleteQuery内で更新を行うが、履歴登録なしの場合を考慮
                    If isHisInsert Then
                        '資産履歴テーブルを更新する。
                        'グリッドの情報を取得
                        Dim lstGrid As List(Of ReserveDetailViewModel) = _LendEntryViewModel.ReserveDetailData
                        For Each row As ReserveDetailViewModel In lstGrid
                            Dim db As New SystemDBContext
                            Dim modelAsset As AssetListModel = db.AssetList.Find(row.ASSET_NO)
                            InsertAssetHistory(modelAsset, ProcessDiv.XD)
                        Next

                        '予約履歴テーブルを更新する。
                        InsertReserveLog(_LendEntryModel, ProcessDiv.XD, _LendEntryViewModel.ReserveDetailData)

                        '登録処理を実行
                        _db.SaveChanges()
                    End If

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
                                                     CommonRfid.ToStringNvl(entryCsvModel.RESERVE_ID).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU))

                    _ViewModel.States = StatusType.SuccessDelete
                End If

                Return True
            Catch ex As Exception
                logger.Error(ex)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeDelete()

            '親の初期処理
            MyBase.InitializeDelete()

            'モデルを削除用モデルにコピー
            CopyViewModeltoModel(_LendEntryViewModel)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        ''' <param name="model">基底モデル</param>
        Protected Overrides Sub DisposeDelete(ByRef model As BaseModel)
            MyBase.DisposeDelete(model)
        End Sub

        ''' <summary>
        ''' 削除処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function DeleteChcek() As Boolean
            Dim blnReturn As Boolean = True

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = _LendEntryViewModel.ReserveDetailData

            '予約ステータスが未貸出以外のデータを取得
            Dim lstModelSelDetail As ReserveDetailViewModel() = (From model In lstGrid
                                                                 Where (model.RESERVE_STATUS <> ReserveStatus.Mi)
                                                                 Order By model.ASSET_NO).ToArray
            '未貸出以外の明細があればエラー
            If lstModelSelDetail.Count > 0 Then
                '未貸出以外は処理を止めずにスキップ
                'ModelState.AddModelError("", CommonLabel.MSG_ERR_EXIST_NOT_RESERVE_STATUS_MI)
                blnReturn = False
            End If

            Return blnReturn
        End Function

        ''' <summary>
        ''' 削除処理のクエリを作成します
        ''' </summary>
        Protected Overrides Sub DeleteQuery()
            Dim entry As LendingEntryModel = _db.LendingEntry.Find(_LendEntryModel.RESERVE_ID)
            '削除を実行する
            _db.LendingEntry.Remove(entry)

            '更新した予約IDを取得
            Dim lReserveId As Long = _LendEntryModel.RESERVE_ID

            '予約明細テーブルの削除
            Dim sbSQL_DelDetail As StringBuilder = New StringBuilder
            Dim params_DelDetail As List(Of SqlParameter) = New List(Of SqlParameter)
            sbSQL_DelDetail.AppendLine("DELETE FROM T_RESERVE_DETAIL WHERE RESERVE_ID = @ReserveId")
            params_DelDetail.Add(New SqlParameter("@ReserveId", lReserveId))
            _db.Database.ExecuteSqlCommand(sbSQL_DelDetail.ToString, params_DelDetail.ToArray)

            'メンテナンステーブルの削除
            Dim sbSQL_DelMainte As StringBuilder = New StringBuilder
            Dim params_DelMainte As List(Of SqlParameter) = New List(Of SqlParameter)
            sbSQL_DelMainte.AppendLine("DELETE FROM T_RESERVE_MAINTE WHERE RESERVE_ID = @ReserveId")
            params_DelMainte.Add(New SqlParameter("@ReserveId", lReserveId))
            _db.Database.ExecuteSqlCommand(sbSQL_DelMainte.ToString, params_DelMainte.ToArray)

        End Sub
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' 予約照会一覧CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExportLendingRefList(ViewModel As LendingStatusViewModel) As ActionResult
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
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Lending_Ref.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Lending_Ref.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Lending_Ref.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Lending_Ref.Key3)
            '20230901 ADD End Shirakawa

            '照合機能設定を取得
            Dim MatchingFunc As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.MatchingFunction.KeyDiv,
                                                                         CommonSettingKey.MatchingFunction.Key1,
                                                                         CommonSettingKey.MatchingFunction.Key2,
                                                                         CommonSettingKey.MatchingFunction.Key3, True)

            'モデルを設定
            _ViewModel = ViewModel

            Dim LendingRefListData As List(Of LendingRefListCsvModel)

            'CSV出力データ取得
            LendingRefListData = GetCsvLendingRefList()

            '20230901 ADD Start Shirakawa
            '設定マスタから取得できた場合
            '設定マスタから取得できた場合 かつ 上限値(Data2)が存在している場合 かつ 上限を超えていた場合
            If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA2) = False AndAlso WebReadData.DATA2 < LendingRefListData.Count Then
                '上限を超えていた場合　CSV出力しない

                '上限確認フラグ
                ViewBag.isLimitFlg = True
                '一覧データ取得件数
                ViewBag.cntList = LendingRefListData.Count
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
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LendingRefList.KeyDiv, CommonSettingKey.LendingRefList.Key1, CommonSettingKey.LendingRefList.Key2, CommonSettingKey.LendingRefList.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_LENDING_STATUS_REF_LIST)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_LENDING_STATUS_REF_LIST)
            If Not IsNothing(MatchingFunc) AndAlso MatchingFunc.DATA1 = ValidFlg.Cd.Valid Then
                '照合機能ONの場合
                sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_LENDING_STATUS_REF_LIST_MATCHING)
            Else
                sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_LENDING_STATUS_REF_LIST)
            End If

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As LendingRefListCsvModel In LendingRefListData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)       '2022/08/31 Add Kawamoto
                '--貸出状態
                Dim sReserveDspStat As String = String.Empty
                Select Case row.RESERVE_DSPSTAT
                    Case ReserveDspStat.Mi
                        sReserveDspStat = Me._Message.getMessage(CommonLabel.VALUE_NOT_RENTED)
                    Case ReserveDspStat.Lending
                        sReserveDspStat = Me._Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)
                    Case ReserveDspStat.Delay
                        sReserveDspStat = Me._Message.getMessage(CommonLabel.VALUE_LENDING_DELAY)
                    Case ReserveDspStat.HenkyakuSumi
                        sReserveDspStat = Me._Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sReserveDspStat)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.RESERVE_ID)
                '--出荷通知
                Dim sDestNo As String = String.Empty
                If row.DEST_NO Is Nothing Then
                    sDestNo = "-"
                ElseIf String.IsNullOrEmpty(row.DEST_NO) Then
                    sDestNo = Me._Message.getMessage(CommonLabel.VALUE_MI)
                Else
                    sDestNo = Me._Message.getMessage(CommonLabel.VALUE_SUMI)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sDestNo)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHARGE_DEP_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHARGE_DEP_NM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHARGE_EMP_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHARGE_EMP_NM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REQ_DEP_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REQ_DEP_NM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REQ_EMP_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REQ_EMP_NM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.MIN_PR_ST_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.PR_END_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.USER_START_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.USER_END_DT))
                If Not IsNothing(MatchingFunc) AndAlso MatchingFunc.DATA1 = ValidFlg.Cd.Valid Then
                    '照合機能ONの場合
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_NO)
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(If(row.CONST_START_DT.HasValue, CommonRfid.ToStringIntYMD(Integer.Parse(row.CONST_START_DT)), ""))
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(If(row.CONST_END_DT.HasValue, CommonRfid.ToStringIntYMD(Integer.Parse(row.CONST_END_DT)), ""))
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_CONTENTS)
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_EMPLOYEE_CD)
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_EMPLOYEE_NAME)
                End If

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_STATUS) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     LendingRefListData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_STATUS))


            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()
        End Function

        ''' <summary>
        ''' 予約照会メンテナンスCSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExportLendingRefMainte(ViewModel As LendingStatusViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            Dim LendingRefMainteData As List(Of LendingRefMainteCsvModel)

            'CSV出力データ取得
            LendingRefMainteData = GetCsvLendingRefMainte()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LendingRefMainte.KeyDiv, CommonSettingKey.LendingRefMainte.Key1, CommonSettingKey.LendingRefMainte.Key2, CommonSettingKey.LendingRefMainte.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_LENDING_STATUS_REF_MAINTE)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_LENDING_STATUS_REF_MAINTE)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As LendingRefMainteCsvModel In LendingRefMainteData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)         '2022/08/31 Add Kawamoto
                '--貸出状態
                Dim sLeaseStatus As String = String.Empty
                Select Case row.LEASE_STATUS.ToString
                    Case AssetLeaseStatus.Ng
                        sLeaseStatus = Me._Message.getMessage(CommonLabel.VALUE_FUKA)
                    Case AssetLeaseStatus.Ok
                        sLeaseStatus = Me._Message.getMessage(CommonLabel.VALUE_KANOU)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sLeaseStatus)
                '--メンテナンス開始日
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.MAINTE_START_DT))
                '--メンテナンス終了日
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.MAINTE_END_DT))
                '--次予約番号
                If row.NEXT_RESERVE_ID.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.NEXT_RESERVE_ID.Value)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                '--次貸出予定日
                Dim sNextStDate As String = String.Empty
                If row.NEXT_ST_DT.HasValue Then
                    sNextStDate = CommonRfid.ToStringIntYMD(row.NEXT_ST_DT.Value)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sNextStDate)

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_STATUS) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     LendingRefMainteData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_STATUS))


            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()
        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' 予約照会一覧用CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetCsvLendingRefList() As List(Of LendingRefListCsvModel)
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)

            '表示月数
            Dim lendingStatusSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Status_Setting.KeyDiv, CommonSettingKey.Lending_Status_Setting.Key1, CommonSettingKey.Lending_Status_Setting.Key2, CommonSettingKey.Lending_Status_Setting.Key3, True)
            Dim iDspMonthCount As Integer = Integer.Parse(lendingStatusSetting.DATA2)
            '表示開始日
            Dim dtStartDate As Date = Date.Parse(_ViewModel.SearchDspStartDate)
            '表示終了日
            Dim dtEndDate As Date = dtStartDate.AddMonths(iDspMonthCount)

            '稼働日考慮フラグ
            Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
            Dim sWorkDayFlg As String = reservationFunction.DATA2
            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If

            '出荷通知確認日の取得
            Dim lendingLTReserveData As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_LT_ReserveData.KeyDiv, CommonSettingKey.Lending_LT_ReserveData.Key1, CommonSettingKey.Lending_LT_ReserveData.Key2, CommonSettingKey.Lending_LT_ReserveData.Key3, True)
            Dim iShippingNotificationDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, Integer.Parse(lendingLTReserveData.DATA1), lendingLTReserveData.DATA2)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT *")
            sbSQL.AppendLine("FROM")
            sbSQL.AppendLine("(")
            sbSQL.AppendLine("  SELECT")
            sbSQL.AppendLine("      ASSET.CAT_L_CD ")
            sbSQL.AppendLine("    , ASSET.CAT_M_CD ")
            sbSQL.AppendLine("    , ASSET.CAT_S_CD")
            sbSQL.AppendLine("    , CATL.CAT_NAME AS CAT_L_NAME ")
            sbSQL.AppendLine("    , CATM.CAT_NAME AS CAT_M_NAME ")
            sbSQL.AppendLine("    , CATS.CAT_NAME AS CAT_S_NAME ")
            sbSQL.AppendLine("    , ASSET.MNG_LOC_L_CD ")
            sbSQL.AppendLine("    , MNGLOC.LOC_NAME AS MNG_LOC_L_NAME ")
            sbSQL.AppendLine("    , ASSET.ASSET_NO ")
            sbSQL.AppendLine("    , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("    , ASSET.MODEL_NO ")               '2022/08/31 Add Kawamoto
            sbSQL.AppendLine("    , ASSET.LEASE_STATUS ")
            sbSQL.AppendLine("    , ASSET.LEASE_CNT ")
            sbSQL.AppendLine("    , AUTH.SHARED_FLG")
            sbSQL.AppendLine("    , dbo.RESERVE_DEST_NO_SF(ASSET.ASSET_NO, @ShippingNotificationDate) AS DEST_NO")  '-- '':未　NULL:予約なし「-」表示する　 以外:送り状番号
            params.Add(New SqlParameter($"@ShippingNotificationDate", iShippingNotificationDate))
            sbSQL.AppendLine("    , RESERVE_DSPSTAT")
            sbSQL.AppendLine("    , MIN_PR_ST_DT")

            sbSQL.AppendLine("    ,PR_END_DT")
            sbSQL.AppendLine("    ,RESERVE_ID")
            sbSQL.AppendLine("    ,USER_START_DT")
            sbSQL.AppendLine("    ,USER_END_DT")
            sbSQL.AppendLine("    ,CHARGE_DEP_CD")
            sbSQL.AppendLine("    ,CHARGE_EMP_CD")
            sbSQL.AppendLine("    ,CHARGE_DEP_NM")
            sbSQL.AppendLine("    ,CHARGE_EMP_NM")
            sbSQL.AppendLine("    ,REQ_DEP_CD")
            sbSQL.AppendLine("    ,REQ_EMP_CD")
            sbSQL.AppendLine("    ,REQ_DEP_NM")
            sbSQL.AppendLine("    ,REQ_EMP_NM")
            sbSQL.AppendLine("    ,CONST_NO")
            sbSQL.AppendLine("    ,CONST_START_DT")
            sbSQL.AppendLine("    ,CONST_END_DT")
            sbSQL.AppendLine("    ,CONST_CONTENTS")
            sbSQL.AppendLine("    ,CONST_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,CONST_EMPLOYEE_NAME")
            sbSQL.AppendLine("  FROM M_ASSET ASSET")
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuthHub) AUTH")
            sbSQL.AppendLine("  ON ASSET.MNG_LOC_L_CD = AUTH.LOC_L_CD")
            params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@ReadAuthFlg", authInfo.ReadAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthFlg", authInfo.UpdAuthFlg))
            params.Add(New SqlParameter($"@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
            params.Add(New SqlParameter($"@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthType_UpdAuthHub", UpdAuthType.UPD_AUTH_HUB))
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATL ")
            sbSQL.AppendLine("  ON CATL.CAT_DIV = @CatDivL")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATL.CAT_L_CD")
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATM ")
            sbSQL.AppendLine("  ON CATM.CAT_DIV = @CatDivM")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATM.CAT_L_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_M_CD = CATM.CAT_M_CD")
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATS ")
            sbSQL.AppendLine("  ON CATS.CAT_DIV = @CatDivS")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATS.CAT_L_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_M_CD = CATS.CAT_M_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_S_CD = CATS.CAT_S_CD")
            sbSQL.AppendLine("  LEFT JOIN M_LOCATION MNGLOC ")
            sbSQL.AppendLine("  ON MNGLOC.LOC_DIV = @LocDivL")
            sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD = MNGLOC.LOC_L_CD")
            params.Add(New SqlParameter($"@CatDivL", CAT_DIV.CategoryL))
            params.Add(New SqlParameter($"@CatDivM", CAT_DIV.CategoryM))
            params.Add(New SqlParameter($"@CatDivS", CAT_DIV.CategoryS))
            params.Add(New SqlParameter($"@LocDivL", LOC_DIV.LocationL))

            sbSQL.AppendLine("  INNER JOIN")
            sbSQL.AppendLine("  (")
            sbSQL.AppendLine("    SELECT")
            sbSQL.AppendLine("         ASSET_NO")
            sbSQL.AppendLine("        ,CASE WHEN RESERVE_STATUS = '" & Misc.ReserveStatus.Lending & "' THEN ")
            sbSQL.AppendLine("        	  CASE WHEN REAL_END_DT < CONVERT(int, CONVERT(VARCHAR, GETDATE(), 112)) THEN '" & Misc.ReserveDspStat.Delay & "'")
            sbSQL.AppendLine("        	  ELSE '" & Misc.ReserveDspStat.Lending & "'")
            sbSQL.AppendLine("            END")
            sbSQL.AppendLine("         ELSE RESERVE_STATUS")
            sbSQL.AppendLine("         END AS RESERVE_DSPSTAT")
            sbSQL.AppendLine("        ,PR_ST_DT AS MIN_PR_ST_DT")
            sbSQL.AppendLine("        ,PR_END_DT")

            sbSQL.AppendLine("        ,RESERVE_ID")
            sbSQL.AppendLine("        ,USER_START_DT")
            sbSQL.AppendLine("        ,USER_END_DT")

            sbSQL.AppendLine("        ,CHARGE_DEP_CD")
            sbSQL.AppendLine("        ,CHARGE_EMP_CD")
            sbSQL.AppendLine("        ,CHARGE_DEP_NM")
            sbSQL.AppendLine("        ,CHARGE_EMP_NM")
            sbSQL.AppendLine("        ,REQ_DEP_CD")
            sbSQL.AppendLine("        ,REQ_EMP_CD")
            sbSQL.AppendLine("        ,REQ_DEP_NM")
            sbSQL.AppendLine("        ,REQ_EMP_NM")
            sbSQL.AppendLine("        ,CONST_NO")
            sbSQL.AppendLine("        ,CONST_START_DT")
            sbSQL.AppendLine("        ,CONST_END_DT")
            sbSQL.AppendLine("        ,CONST_CONTENTS")
            sbSQL.AppendLine("        ,CONST_EMPLOYEE_CD")
            sbSQL.AppendLine("        ,CONST_EMPLOYEE_NAME")

            sbSQL.AppendLine("    FROM")
            sbSQL.AppendLine("    (")
            sbSQL.AppendLine("      SELECT")
            sbSQL.AppendLine("            RESVM.ASSET_NO")
            sbSQL.AppendLine("          , RESVM.RESERVE_STATUS")
            sbSQL.AppendLine("          , RESVM.PR_ST_DT")
            sbSQL.AppendLine("          , RESVM.PR_END_DT")
            sbSQL.AppendLine("          , RESVM.REAL_END_DT")

            sbSQL.AppendLine("          , RESVH.RESERVE_ID")
            sbSQL.AppendLine("          , RESVH.USER_START_DT")
            sbSQL.AppendLine("          , RESVH.USER_END_DT")
            sbSQL.AppendLine("          , CHARGE_EMP.DEPARTMENT_CD AS CHARGE_DEP_CD")
            sbSQL.AppendLine("          , RESVH.CHARGE_EMP_CD")
            sbSQL.AppendLine("          , CHARGE_DEP.DEPARTMENT_NAME AS CHARGE_DEP_NM")
            sbSQL.AppendLine("          , CHARGE_EMP.EMPLOYEE_NAME AS CHARGE_EMP_NM")
            sbSQL.AppendLine("          , REQ_EMP.DEPARTMENT_CD AS REQ_DEP_CD")
            sbSQL.AppendLine("          , RESVH.REQ_EMP_CD")
            sbSQL.AppendLine("          , REQ_DEP.DEPARTMENT_NAME AS REQ_DEP_NM")
            sbSQL.AppendLine("          , REQ_EMP.EMPLOYEE_NAME AS REQ_EMP_NM")
            sbSQL.AppendLine("          , CONST.CONST_NO AS CONST_NO")
            sbSQL.AppendLine("          , CONST.CONST_START_DT AS CONST_START_DT")
            sbSQL.AppendLine("          , CONST.CONST_END_DT AS CONST_END_DT")
            sbSQL.AppendLine("          , CONST.CONST_CONTENTS AS CONST_CONTENTS")
            sbSQL.AppendLine("          , CONST.CONST_EMPLOYEE_CD AS CONST_EMPLOYEE_CD")
            sbSQL.AppendLine("          , CONST.CONST_EMPLOYEE_NAME AS CONST_EMPLOYEE_NAME")

            sbSQL.AppendLine("          , ROW_NUMBER() OVER(PARTITION BY RESVM.ASSET_NO ORDER BY RESVM.PR_ST_DT ASC) NUM")
            sbSQL.AppendLine("      FROM T_RESERVE_DETAIL RESVM")
            sbSQL.AppendLine("      INNER JOIN T_RESERVE RESVH")
            sbSQL.AppendLine("      ON  RESVH.RESERVE_ID = RESVM.RESERVE_ID")
            sbSQL.AppendLine("      LEFT JOIN M_CONSTRUCTION CONST")
            sbSQL.AppendLine("      ON  CONST.CONST_NO = RESVH.REMARK")
            sbSQL.AppendLine("      LEFT JOIN M_EMPLOYEE CHARGE_EMP")
            sbSQL.AppendLine("      ON  CHARGE_EMP.EMPLOYEE_CD = RESVH.CHARGE_EMP_CD")
            sbSQL.AppendLine("      LEFT JOIN M_DEPARTMENT CHARGE_DEP")
            sbSQL.AppendLine("      ON   CHARGE_DEP.DEPARTMENT_CD = CHARGE_EMP.DEPARTMENT_CD")
            sbSQL.AppendLine("      LEFT JOIN M_EMPLOYEE REQ_EMP")
            sbSQL.AppendLine("      ON  REQ_EMP.EMPLOYEE_CD = RESVH.REQ_EMP_CD")
            sbSQL.AppendLine("      LEFT JOIN M_DEPARTMENT REQ_DEP")
            sbSQL.AppendLine("      ON   REQ_DEP.DEPARTMENT_CD = REQ_EMP.DEPARTMENT_CD")

            sbSQL.AppendLine("      WHERE (@StartDate <= RESVM.PR_END_DT and RESVM.PR_ST_DT <= @EndDate)")
            params.Add(New SqlParameter($"@StartDate", dtStartDate.ToString("yyyyMMdd")))
            params.Add(New SqlParameter($"@EndDate", dtEndDate.ToString("yyyyMMdd")))

            '貸出状態(予約関連)
            If Not _ViewModel.SearchLeaseStatus_NotRented OrElse Not _ViewModel.SearchLeaseStatus_Lending OrElse
                Not _ViewModel.SearchLeaseStatus_LendingDelay OrElse Not _ViewModel.SearchLeaseStatus_Returned Then
                sbSQL.AppendLine("      AND")
                sbSQL.AppendLine("      (")

                Dim sbWkJoken As StringBuilder = New StringBuilder
                If _ViewModel.SearchLeaseStatus_NotRented Then
                    '未貸出
                    If sbWkJoken.Length > 0 Then
                        sbWkJoken.Append("          or ")
                    End If
                    sbWkJoken.AppendLine("          (RESVM.RESERVE_STATUS = '" & Misc.ReserveStatus.Mi & "')")
                End If
                If _ViewModel.SearchLeaseStatus_Lending Then
                    '貸出中
                    If sbWkJoken.Length > 0 Then
                        sbWkJoken.Append("          or ")
                    End If
                    sbWkJoken.AppendLine("          (RESVM.RESERVE_STATUS = '" & Misc.ReserveStatus.Lending & "' and RESVM.REAL_END_DT >= CONVERT(int, CONVERT(VARCHAR, GETDATE(), 112)) )")
                End If
                If _ViewModel.SearchLeaseStatus_LendingDelay Then
                    '貸出中(遅延)
                    If sbWkJoken.Length > 0 Then
                        sbWkJoken.Append("          or ")
                    End If
                    sbWkJoken.AppendLine("          (RESVM.RESERVE_STATUS = '" & Misc.ReserveStatus.Lending & "' and RESVM.REAL_END_DT < CONVERT(int, CONVERT(VARCHAR, GETDATE(), 112)) )")
                End If
                If _ViewModel.SearchLeaseStatus_Returned Then
                    '返却済
                    If sbWkJoken.Length > 0 Then
                        sbWkJoken.Append("          or ")
                    End If
                    sbWkJoken.AppendLine("          (RESVM.RESERVE_STATUS = '" & Misc.ReserveStatus.HenkyakuSumi & "')")
                End If
                If sbWkJoken.Length = 0 Then                '貸出の条件がない場合は、ダミー条件を入れる
                    sbWkJoken.Append("           1 = 1")
                End If

                sbSQL.AppendLine(sbWkJoken.ToString)
                sbSQL.AppendLine("      )")
            End If

            '担当者
            If Not String.IsNullOrEmpty(_ViewModel.SearchEmpName) Then
                sbSQL.AppendLine("      AND   CHARGE_EMP.EMPLOYEE_NAME LIKE @ChargeEmpName")
                params.Add(New SqlParameter($"@ChargeEmpName", "%" & _ViewModel.SearchEmpName & "%"))
            End If
            '申請者
            If Not String.IsNullOrEmpty(_ViewModel.SearchReqEmpName) Then
                sbSQL.AppendLine("      AND   REQ_EMP.EMPLOYEE_NAME LIKE @ReqEmpName")
                params.Add(New SqlParameter($"@ReqEmpName", "%" & _ViewModel.SearchReqEmpName & "%"))
            End If
            '工事番号
            If Not String.IsNullOrEmpty(_ViewModel.SearchConstNo) Then
                sbSQL.AppendLine("      AND   CONST.CONST_NO = @ConstNo")
                params.Add(New SqlParameter($"@ConstNo", _ViewModel.SearchConstNo))
            End If

            sbSQL.AppendLine("    )  AS TBL")
            sbSQL.AppendLine("    WHERE NUM = 1")
            sbSQL.AppendLine("  ) AS RSVAST")
            sbSQL.AppendLine("  ON RSVAST.ASSET_NO = ASSET.ASSET_NO")
            sbSQL.AppendLine("  WHERE ASSET.ASSET_STATUS IN (@AssetStatus_Lending, @AssetStatus_Storage)")
            params.Add(New SqlParameter($"@AssetStatus_Lending", ASSET_STATUS.Lending))
            params.Add(New SqlParameter($"@AssetStatus_Storage", ASSET_STATUS.Storage))
            sbSQL.AppendLine("  AND ASSET.LEASE_STATUS ='" & AssetLeaseStatus.Ok & "'")
            '管理ロケ大区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SelectMngLocKeys) Then
                Dim sbSQLMngLocKey As StringBuilder = New StringBuilder
                Dim selectMngLocKeys As String() = _ViewModel.SelectMngLocKeys.Split(", ")
                For idx As Integer = 0 To selectMngLocKeys.Length - 1
                    If sbSQLMngLocKey.Length <> 0 Then
                        sbSQLMngLocKey.Append(", ")
                    End If
                    sbSQLMngLocKey.Append("@MngLocLCd" & idx)
                    params.Add(New SqlParameter($"MngLocLCd" & idx, selectMngLocKeys(idx)))
                Next
                sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD IN (" & sbSQLMngLocKey.ToString & ")")
            End If
            'カテゴリ大分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatL) Then
                sbSQL.AppendLine("  AND ASSET.CAT_L_CD = @SearchCatL")
                params.Add(New SqlParameter($"@SearchCatL", _ViewModel.SearchCatL))
            End If
            'カテゴリ中分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatM) Then
                sbSQL.AppendLine("  AND ASSET.CAT_M_CD = @SearchCatM")
                params.Add(New SqlParameter($"@SearchCatM", _ViewModel.SearchCatM))
            End If
            'カテゴリ小分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatS) Then
                sbSQL.AppendLine("  AND ASSET.CAT_S_CD = @SearchCatS")
                params.Add(New SqlParameter($"@SearchCatS", _ViewModel.SearchCatS))
            End If

            'TODO:'2022/02/25 nv miyaji 資産一覧にある資産番号と資産名称の検索SQL　井上さんSQLお願いします。
            ''資産番号 SearchAssetNo
            'If (Not IsNothing(_ViewModel.SearchAssetNo)) AndAlso
            '        (Not String.IsNullOrEmpty(_ViewModel.SearchAssetNo.Trim)) Then
            '    sbWhr.AppendLine("  AND ASSET.ASSET_NO like " & "@SearchAssetNo")
            '    addSqlParam("SearchAssetNo", $"{_ViewModel.SearchAssetNo.Trim & "%"}")
            'End If

            ''資産名称 SearchAssetName
            'If (Not IsNothing(_ViewModel.SearchAssetName)) AndAlso
            '        (Not String.IsNullOrEmpty(_ViewModel.SearchAssetName.Trim)) Then
            '    sbWhr.AppendLine("  AND ASSET.ASSET_NAME like " & "@SearchAssetName")
            '    addSqlParam("SearchAssetName", $"{"%" & _ViewModel.SearchAssetName.Trim & "%"}")
            'End If

            sbSQL.AppendLine(") MAINTBL")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '出荷通知
            If _ViewModel.SearchShippingNotification_NotNotified AndAlso _ViewModel.SearchShippingNotification_Notified = False Then
                '-- 出荷通知の未通知 のみチェック時
                sbSQL.AppendLine("  AND  (MAINTBL.DEST_NO IS NULL or MAINTBL.DEST_NO = '')")
            ElseIf _ViewModel.SearchShippingNotification_NotNotified = False AndAlso _ViewModel.SearchShippingNotification_Notified Then
                '-- 出荷通知の通知 のみチェック時
                sbSQL.AppendLine("  AND  (MAINTBL.DEST_NO <> '')")
            End If

            '並び替え
            sbSQL.AppendLine("ORDER BY ")
            sbSQL.AppendLine("    CAT_L_CD ")
            sbSQL.AppendLine("  , CAT_M_CD ")
            sbSQL.AppendLine("  , CAT_S_CD ")
            sbSQL.AppendLine("  , SHARED_FLG DESC ")
            sbSQL.AppendLine("  , MNG_LOC_L_CD ")
            sbSQL.AppendLine("  , LEASE_CNT ")
            sbSQL.AppendLine("  , ASSET_NO ")

            Dim lstLendingRefList As List(Of LendingRefListCsvModel)
            Dim db As SystemDBContext = New SystemDBContext

            'SQLを実行する
            lstLendingRefList = db.LendingRefListCsv.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            'データを返却
            Return lstLendingRefList
        End Function

        ''' <summary>
        ''' 予約照会メンテナンスCSV出力用のデータを取得します
        ''' </summary>
        Private Function GetCsvLendingRefMainte() As List(Of LendingRefMainteCsvModel)
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)

            '表示月数
            Dim lendingStatusSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Status_Setting.KeyDiv, CommonSettingKey.Lending_Status_Setting.Key1, CommonSettingKey.Lending_Status_Setting.Key2, CommonSettingKey.Lending_Status_Setting.Key3, True)
            Dim iDspMonthCount As Integer = Integer.Parse(lendingStatusSetting.DATA2)
            '表示開始日
            Dim dtStartDate As Date = Date.Parse(_ViewModel.SearchDspStartDate)
            '表示終了日
            Dim dtEndDate As Date = dtStartDate.AddMonths(iDspMonthCount)

            '稼働日考慮フラグ
            Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
            Dim sWorkDayFlg As String = reservationFunction.DATA2
            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("   ASSET.CAT_L_CD ")
            sbSQL.AppendLine("  ,ASSET.CAT_M_CD ")
            sbSQL.AppendLine("  ,ASSET.CAT_S_CD")
            sbSQL.AppendLine("  ,CATL.CAT_NAME AS CAT_L_NAME ")
            sbSQL.AppendLine("  ,CATM.CAT_NAME AS CAT_M_NAME ")
            sbSQL.AppendLine("  ,CATS.CAT_NAME AS CAT_S_NAME ")
            sbSQL.AppendLine("  ,ASSET.MNG_LOC_L_CD ")
            sbSQL.AppendLine("  ,MNGLOC.LOC_NAME AS MNG_LOC_L_NAME ")
            sbSQL.AppendLine("	,MNT.ASSET_NO")
            sbSQL.AppendLine("  ,ASSET.ASSET_NAME ")
            sbSQL.AppendLine("  ,ASSET.MODEL_NO ")          '2022/08/31 Add Kawamoto
            sbSQL.AppendLine("  ,ASSET.LEASE_STATUS ")
            sbSQL.AppendLine("	,MNT.MAINTE_ID")
            sbSQL.AppendLine("	,MNT.RESERVE_ID")
            sbSQL.AppendLine("	,MNT.MAINTE_START_DT")
            sbSQL.AppendLine("	,MNT.MAINTE_END_DT")
            sbSQL.AppendLine("	,MNT.REMARK")
            sbSQL.AppendLine("	,(SELECT RESERVE_ID FROM dbo.NEXT_RESERVE_SF(MNT.ASSET_NO, NULL, MNT.MAINTE_END_DT, @WorkFlg)) AS NEXT_RESERVE_ID")
            sbSQL.AppendLine("	,(SELECT ST_DT FROM dbo.NEXT_RESERVE_SF(MNT.ASSET_NO, NULL, MNT.MAINTE_END_DT, @WorkFlg)) AS NEXT_ST_DT")
            params.Add(New SqlParameter($"@WorkFlg", sWorkFlg))
            sbSQL.AppendLine("FROM T_RESERVE_MAINTE MNT")
            sbSQL.AppendLine("  INNER JOIN M_ASSET ASSET")
            sbSQL.AppendLine("  ON MNT.ASSET_NO = ASSET.ASSET_NO")
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuthHub) AUTH")
            sbSQL.AppendLine("  ON ASSET.MNG_LOC_L_CD = AUTH.LOC_L_CD")
            params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@ReadAuthFlg", authInfo.ReadAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthFlg", authInfo.UpdAuthFlg))
            params.Add(New SqlParameter($"@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
            params.Add(New SqlParameter($"@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthType_UpdAuthHub", UpdAuthType.UPD_AUTH_HUB))

            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATL ")
            sbSQL.AppendLine("  ON CATL.CAT_DIV = @CatDivL")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATL.CAT_L_CD")
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATM ")
            sbSQL.AppendLine("  ON CATM.CAT_DIV = @CatDivM")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATM.CAT_L_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_M_CD = CATM.CAT_M_CD")
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATS ")
            sbSQL.AppendLine("  ON CATS.CAT_DIV = @CatDivS")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATS.CAT_L_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_M_CD = CATS.CAT_M_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_S_CD = CATS.CAT_S_CD")
            sbSQL.AppendLine("  LEFT JOIN M_LOCATION MNGLOC ")
            sbSQL.AppendLine("  ON MNGLOC.LOC_DIV = @LocDivL")
            sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD = MNGLOC.LOC_L_CD")
            params.Add(New SqlParameter($"@CatDivL", CAT_DIV.CategoryL))
            params.Add(New SqlParameter($"@CatDivM", CAT_DIV.CategoryM))
            params.Add(New SqlParameter($"@CatDivS", CAT_DIV.CategoryS))
            params.Add(New SqlParameter($"@LocDivL", LOC_DIV.LocationL))

            sbSQL.AppendLine("  WHERE (@StartDate <= MNT.MAINTE_END_DT and MNT.MAINTE_START_DT <= @EndDate)")
            params.Add(New SqlParameter($"@StartDate", dtStartDate.ToString("yyyyMMdd")))
            params.Add(New SqlParameter($"@EndDate", dtEndDate.ToString("yyyyMMdd")))

            '管理ロケ大区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SelectMngLocKeys) Then
                Dim sbSQLMngLocKey As StringBuilder = New StringBuilder
                Dim selectMngLocKeys As String() = _ViewModel.SelectMngLocKeys.Split(", ")
                For idx As Integer = 0 To selectMngLocKeys.Length - 1
                    If sbSQLMngLocKey.Length <> 0 Then
                        sbSQLMngLocKey.Append(", ")
                    End If
                    sbSQLMngLocKey.Append("@MngLocLCd" & idx)
                    params.Add(New SqlParameter($"MngLocLCd" & idx, selectMngLocKeys(idx)))
                Next
                sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD IN (" & sbSQLMngLocKey.ToString & ")")
            End If

            sbSQL.AppendLine("  AND ASSET.LEASE_STATUS ='" & AssetLeaseStatus.Ok & "'")

            'sbSQL.Append(" ORDER BY MNT.MAINTE_START_DT, MNT.MAINTE_ID")

            sbSQL.AppendLine("  ORDER BY ")
            sbSQL.AppendLine("      AUTH.SHARED_FLG DESC ")
            sbSQL.AppendLine("    , ASSET.MNG_LOC_L_CD ")
            sbSQL.AppendLine("    , MNT.MAINTE_START_DT")
            sbSQL.AppendLine("    , MNT.ASSET_NO ")
            sbSQL.AppendLine("    , MNT.MAINTE_ID ")

            Dim lstLendingRefMainte As List(Of LendingRefMainteCsvModel)
            Dim db As SystemDBContext = New SystemDBContext

            'SQLを実行する
            lstLendingRefMainte = db.LendingRefMainteCsv.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            'データを返却
            Return lstLendingRefMainte
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

#Region "CSV取込処理"
        ''' <summary>
        ''' CSV取込処理（AJax) 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="CsvFile">登録するCSVファイル</param>
        ''' <returns>View表示内容</returns>
        Public Function ImportCsvFile(ByVal CsvFile As HttpPostedFileBase) As ActionResult
            Me._lstErr = New List(Of DataInportResult)
            Dim errRow As DataInportResult = New DataInportResult
            Me._lstErr.Add(errRow)
            Dim sCsvPath As String = String.Empty                  'CSVアップロードフルパス
            Dim sProcessContents As String = String.Empty          '操作ログ出力

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.LoginSession
                '結果表示
                Return Json(Me._lstErr)
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '読込むCSVファイルがあるかどうか
            If Not IsCsvFileError(CsvFile) Then
                Dim sErrGaiyou As String = String.Empty
                Select Case Me._lstErr(0).ResultCd
                    Case DataInport.DataInportResultCd.NoSelect
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECT)
                    Case DataInport.DataInportResultCd.Empty
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_EMPTY)
                    Case DataInport.DataInportResultCd.NotCsv
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTCSV)
                End Select

                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

                Return Json(Me._lstErr)
            End If

            'CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVIMPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'CSVファイルをアップロード
            sCsvPath = Path.Combine(sFullPath, CsvFile.FileName)
            CsvFile.SaveAs(sCsvPath)

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim isErrLogPut As Boolean = False          'エラーログを排出するかどうか

            Try
                '◆CSVデータ取得
                Dim blErr As Boolean = False            'CSVファイルにエラーがあるかどうか
                Dim isKomokusuuErr As Boolean = False   '項目数エラーかどうか

                blErr = ReadCsvFile(sCsvPath, isKomokusuuErr)

                '項目数エラーの場合
                If isKomokusuuErr Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
                    isErrLogPut = True
                    Return Json(Me._lstErr)
                End If

                'エラーがあった場合
                If blErr Then
                    isErrLogPut = True
                    Return Json(Me._lstErr)
                End If

                '◆重複チェックなし予約番号リスト作成
                Dim lstNotDupChkReserveId As List(Of String) = FncCreateNotDupChkDatas()

                '◆設定Mから貸出用途情報を取得
                Dim lstReservMethod As List(Of SettingInfo) = FncGetReservMethodInfo()

                '◆CSV共通仕様チェック
                If FuncCsvImportBasicCheck(lstNotDupChkReserveId, lstReservMethod) = False Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                    isErrLogPut = True
                    Return Json(Me._lstErr)
                End If

                '◆削除に必要な情報を設定
                Dim del_i As Integer = 1    'ヘッダーを除くため1から開始
                For Each csvItem As ReserveCsvImportModel In _CsvImportItems.Where(Function(x) x.PROCESS_DIV = CsvImportProcDiv.Delete)
                    del_i += 1

                    '予約明細情報を設定
                    csvItem.RESERVE_DETAIL = FuncSetDelGridInfo(csvItem)
                Next

                '◆予約削除のスキップメッセージ取得
                Dim lstErr As List(Of DataInportResult) = FncSetRetReserveMsg()

                '◆予約削除実施
                If FuncCsvImportDelete() = False Then
                    ModelState.AddModelError("", CommonLabel.MSG_ERR_SYSTEM)    'ロールバックさせるために追加
                    isErrLogPut = True
                    Return Json(Me._lstErr)
                End If

                '◆予約削除のスキップメッセージ追加
                If Not IsNothing(lstErr) AndAlso lstErr.Count > 0 Then
                    If _lstErr(0).ErrRowIdx <= 0 Then
                        _lstErr = lstErr
                    Else
                        _lstErr.AddRange(lstErr)
                    End If
                End If

                '◆貸出用途 コード→名称変換
                Call SubCsvLeaseUseCdToName(lstReservMethod)

                '◆資産情報設定処理
                If FncSetAssetInfo() = False Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                    isErrLogPut = True
                    Return Json(Me._lstErr)
                End If

                '◆登録・更新に必要な情報を設定
                Dim row_i As Integer = 1    'ヘッダーを除くため1から開始
                For Each csvItem As ReserveCsvImportModel In _CsvImportItems.Where(Function(x) x.PROCESS_DIV <> CsvImportProcDiv.Delete)
                    row_i += 1

                    '予約明細情報を設定
                    csvItem.RESERVE_DETAIL = FuncSetInsGridInfo(csvItem)

                    '実貸出開始日・実貸出終了日を設定
                    SetRealStartDt(csvItem.RESERVE_DETAIL, csvItem.REAL_START_DT, csvItem.REAL_END_DT)
                    SetRealEndDt(row_i, csvItem, csvItem.REAL_START_DT, csvItem.REAL_END_DT)   'ユーザー終了日も設定
                Next

                '◆処理区分別チェック
                If FuncProcDivCheck(lstNotDupChkReserveId) = False Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                    isErrLogPut = True
                    Return Json(Me._lstErr)
                End If

                '◆登録・更新・削除を実行
                If FuncCsvImportInsAndUpd() = False Then
                    ModelState.AddModelError("", CommonLabel.MSG_ERR_SYSTEM)    'ロールバックさせるために追加
                    isErrLogPut = True
                    Return Json(Me._lstErr)
                End If

                '◆資産ごとの予約期間重複チェック
                If FncChkDupReserveRange() = False Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                    ModelState.AddModelError("", CommonLabel.MSG_ERR_SYSTEM)    'ロールバックさせるために追加
                    isErrLogPut = True
                    Return Json(Me._lstErr)
                End If

                '◆操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                    _CsvImportItems.Count.ToString

                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))

            Catch ex As Exception
                logger.Error(ex.Message)
                isErrLogPut = True
                Throw New Exception()
            Finally
                If isErrLogPut Then
                    'エラーログ出力
                    sProcessContents = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_STATUS) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " &
                                                     Path.GetFileName(CsvFile.FileName) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                                     Me._lstErr.Count.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_STATUS))
                End If
            End Try

            Return Json(Me._lstErr)
        End Function
#End Region

#Region "CSVファイルのエラーチェック"
        ''' <summary>
        ''' CSVファイルのエラーチェック 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="file">チェック対象のファイル</param>
        ''' <returns>True:エラー無し　False:エラー有り</returns>
        Public Function IsCsvFileError(file As HttpPostedFileBase) As Boolean
            Dim blRet As Boolean = False

            'ファイルがあるかどうか
            If IsNothing(file) Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NoSelect
                Return blRet
            End If

            'ファイルサイズ
            If file.ContentLength = 0 Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.Empty
                Return blRet
            End If

            '拡張子を検証
            Dim extention As String = Path.GetExtension(file.FileName).Replace(".", "")
            If String.IsNullOrEmpty(extention) OrElse extention.ToLower <> "csv" Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NotCsv
                Return blRet
            End If

            blRet = True
            Return blRet
        End Function
#End Region

#Region "指定したCSVファイルからデータを読込"
        ''' <summary>
        ''' 指定したCSVファイルからデータを読込 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="path">読み取るファイルのパス</param>
        ''' <param name="isKomokusuuErr">項目数エラーかどうか</param>
        ''' <returns>true:正常終了 false:エラー：</returns>
        ''' <remarks></remarks>
        Public Function ReadCsvFile(ByVal path As String, ByRef isKomokusuuErr As Boolean) As Boolean

            Dim blErr As Boolean = False 'エラーがあるかどうか
            Dim sline As String = ""    'CSVファイル読込行データ格納用

            _CsvImportItems.Clear()

            '設定マスタから取込可能な資産番号の最大項目数を取得する
            Dim lendingNumberItems As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Number_Items.KeyDiv, CommonSettingKey.Lending_Number_Items.Key1, CommonSettingKey.Lending_Number_Items.Key2, CommonSettingKey.Lending_Number_Items.Key3, True)

            'CSV項目数(資産番号以外)
            Dim minCsvItems As Integer = [Enum].GetValues(GetType(CsvImportItem)).Length

            '最大項目数を数値に変換
            Dim maxKomokusuu As Integer
            If Integer.TryParse(lendingNumberItems.DATA1, maxKomokusuu) = False Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG

                '最終行が空白行であれば削除する。
                If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = 0 Then
                    Dim lstRow As DataInportResult = New DataInportResult
                    Me._lstErr.RemoveAt(Me._lstErr.Count - 1)
                End If

                Return False
            End If

            '資産番号最大項目数 + CSV項目数
            maxKomokusuu += minCsvItems

            'ファイルから読み込み
            Using sr As StreamReader = New StreamReader(path)
                Dim iRowCnt As Integer = 0  '行カウント用
                '１行取得
                sline = sr.ReadLine()
                Do Until sline Is Nothing
                    If iRowCnt = 0 Then
                        iRowCnt += 1
                        sline = sr.ReadLine()
                        Continue Do
                    Else
                        iRowCnt += 1
                    End If

                    'エラー件数が１０００件超えていたら以降処理をしない
                    If Me._lstErr.Count > CommonConst.CSV_ERROR_MAXCOUNT Then
                        Exit Do
                    End If

                    '--行を分解しString配列にする。
                    Dim fields As String() = New String() {}
                    If sline.Length > 0 Then
                        fields = clsSV_CsvFile.ReadCsvLineToArray(sline)
                    End If

                    '項目数チェック
                    If fields.Length <= minCsvItems OrElse fields.Length > maxKomokusuu Then
                        isKomokusuuErr = True
                        Return blErr
                    End If

                    Dim csvItem As ReserveCsvImportModel = New ReserveCsvImportModel
                    csvItem.PROCESS_DIV = fields(CsvImportItem.PROCESS_DIV)                     '処理区分
                    csvItem.RESERVE_ID = fields(CsvImportItem.RESERVE_ID)                       '予約番号
                    csvItem.REQUEST_DT = fields(CsvImportItem.REQ_DATE)                         '申請日
                    csvItem.REQ_EMP_CD = fields(CsvImportItem.REQ_EMP_CD)                       '申請社員コード
                    csvItem.CHARGE_EMP_CD = fields(CsvImportItem.CHARGE_EMP_CD)                 '担当社員コード
                    csvItem.REAL_START_DT = fields(CsvImportItem.REAL_START_DT)                 '実貸出開始日
                    csvItem.REAL_END_DT = fields(CsvImportItem.REAL_END_DT)                     '実貸出終了日
                    csvItem.LEASE_PLACE_CD = fields(CsvImportItem.LEASE_PLACE_CD)               '貸出先コード
                    csvItem.LEASE_USE = fields(CsvImportItem.LEASE_USE)                         '貸出用途
                    csvItem.DEST_NAME1 = fields(CsvImportItem.DEST_NAME1)                       '送り先名1
                    csvItem.DEST_NAME2 = fields(CsvImportItem.DEST_NAME2)                       '送り先名2
                    csvItem.TO_DEST_NAME = fields(CsvImportItem.TO_DEST_NAME)                   '貸出確認書宛名    '2023/03/22 Kawamoto ADD
                    csvItem.DEST_ADDRESS1 = fields(CsvImportItem.DEST_ADDRESS1)                 '送り先住所1
                    csvItem.DEST_ADDRESS2 = fields(CsvImportItem.DEST_ADDRESS2)                 '送り先住所2
                    csvItem.DEST_ADDRESS3 = fields(CsvImportItem.DEST_ADDRESS3)                 '送り先住所3
                    csvItem.DEST_PHONE_NUM = fields(CsvImportItem.DEST_PHONE_NUM).Trim          '送り先電話番号
                    csvItem.DEST_ZIP_CODE = fields(CsvImportItem.DEST_ZIP_CODE).Trim            '送り先郵便番号
                    '2023/03/22 Kawamoto ADD START
                    csvItem.RET_LEASE_PLACE_CD = fields(CsvImportItem.RET_LEASE_PLACE_CD).Trim  '送り先郵便番号
                    csvItem.RET_ADDRESS1 = fields(CsvImportItem.RET_ADDRESS1)                   '返送先住所1
                    csvItem.RET_ADDRESS2 = fields(CsvImportItem.RET_ADDRESS2)                   '返送先住所2
                    csvItem.RET_ADDRESS3 = fields(CsvImportItem.RET_ADDRESS3)                   '返送先住所3
                    csvItem.RET_ZIP_CODE = fields(CsvImportItem.RET_ZIP_CODE).Trim              '返送先郵便番号
                    '2023/03/22 Kawamoto ADD END
                    csvItem.REMARK = fields(CsvImportItem.REMARK)                               '備考
                    csvItem.REMARK_INTERNAL = fields(CsvImportItem.REMARK_INTERNAL)             '社内備考
                    csvItem.DEST_NO = fields(CsvImportItem.DEST_NO)                             '送り状番号

                    '配列から資産番号のみ切り出してループ
                    Dim assetNo As String
                    For i As Integer = 0 To fields.Skip(minCsvItems).Take(fields.Length - minCsvItems).Count - 1
                        assetNo = fields(minCsvItems + i)

                        If String.IsNullOrEmpty(assetNo) Then
                            '資産番号が空白
                            Continue For
                        End If

                        csvItem.ASSET_INFO_LIST.Add(New ReserveCsvImportModel.AssetInfo With {.ASSET_NO = assetNo})
                    Next

                    If csvItem.PROCESS_DIV <> CsvImportProcDiv.Delete AndAlso csvItem.ASSET_INFO_LIST.Count <= 0 Then
                        '処理区分が削除以外
                        '資産番号が未入力
                        setDspErrContent(iRowCnt, _Message.getMessage(CommonLabel.LABEL_ASSET_NO), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_ASSET_NO))
                        blErr = True
                    End If

                    _CsvImportItems.Add(csvItem)

                    sline = sr.ReadLine()
                Loop

                'データ項目にエラーがあった場合
                If blErr Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG

                    '最終行が空白行であれば削除する。
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.RemoveAt(Me._lstErr.Count - 1)
                    End If
                End If

            End Using

            Return blErr

        End Function
#End Region

#Region "CSV取込チェック処理"
        ''' <summary>
        ''' CSV取込 基本チェック処理 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="prmLstNotDupChkReserveId">登録時、重複チェックなし予約番号リスト</param>
        ''' <param name="prmLstReservMethod">貸出用途情報リスト　設定Mから取得</param>
        ''' <returns></returns>
        Private Function FuncCsvImportBasicCheck(ByVal prmLstNotDupChkReserveId As List(Of String), ByVal prmLstReservMethod As List(Of SettingInfo)) As Boolean
            Try
                Dim blnReturn As Boolean = True

                '◆CSV取込済みかチェック
                If _CsvImportItems.Count <= 0 Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NoSelect
                    blnReturn = False
                End If

                Dim row_i As Integer = 1    'ヘッダーを除くため1から開始
                For Each importItem As ReserveCsvImportModel In _CsvImportItems
                    row_i += 1

                    '◆処理区分
                    If importItem.PROCESS_DIV.Length <= 0 Then
                        '必須チェックエラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_PROCESS_DIV), _Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY))
                        blnReturn = False
                    End If
                    If importItem.PROCESS_DIV.Length > itemlength.ProcDiv Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_PROCESS_DIV), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    If importItem.PROCESS_DIV <> CsvImportProcDiv.Insert AndAlso
                        importItem.PROCESS_DIV <> CsvImportProcDiv.Update AndAlso
                        importItem.PROCESS_DIV <> CsvImportProcDiv.Delete Then
                        '区分値範囲エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_PROCESS_DIV), _Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL))
                        blnReturn = False
                    End If

                    '◆予約番号
                    If importItem.RESERVE_ID.Length <= 0 Then
                        '必須チェックエラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER), _Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY))
                        blnReturn = False
                    End If
                    If importItem.RESERVE_ID.Length > itemlength.ReserveNum Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    If CommonRfid.IsNumber(importItem.RESERVE_ID) = False Then
                        '数値チェックエラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER), _Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU))
                        blnReturn = False
                    Else
                        '予約番号存在チェック
                        If importItem.PROCESS_DIV <> CsvImportProcDiv.Insert OrElse prmLstNotDupChkReserveId.Contains(importItem.RESERVE_ID) = False Then
                            '更新・削除時　もしくは　重複チェック対象の予約番号の場合
                            Dim errMsg As String = String.Empty
                            If FuncExistReserveId(importItem.PROCESS_DIV, Convert.ToInt64(importItem.RESERVE_ID), errMsg) = False Then
                                setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER), errMsg)
                                blnReturn = False
                            End If
                        End If
                    End If
                    If _CsvImportItems.Where(Function(x) x.RESERVE_ID = importItem.RESERVE_ID AndAlso x.PROCESS_DIV <> CsvImportProcDiv.Insert).Count > 1 Then
                        'CSVファイル内、予約番号重複チェック（更新と削除の重複）
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER), _Message.getMessage(CommonLabel.MSG_ERR_EXITS_RESERVE_ID))
                        blnReturn = False
                    End If
                    If _CsvImportItems.Where(Function(x) x.RESERVE_ID = importItem.RESERVE_ID AndAlso x.PROCESS_DIV <> CsvImportProcDiv.Delete).Count > 1 Then
                        'CSVファイル内、予約番号重複チェック（登録と更新の重複）
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER), _Message.getMessage(CommonLabel.MSG_ERR_EXITS_RESERVE_ID))
                        blnReturn = False
                    End If

                    '※ 処理区分が削除の場合は予約番号までがチェック対象
                    If importItem.PROCESS_DIV = CsvImportProcDiv.Delete Then
                        Continue For
                    End If

                    '◆申請日
                    If importItem.REQUEST_DT.Length < itemlength.ReqestDate Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REQUEST_DT), _Message.getMessage(CommonLabel.MSG_ERR_SHORTAGE_STR_LEN))
                        blnReturn = False
                    End If
                    If importItem.REQUEST_DT.Length > itemlength.ReqestDate Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REQUEST_DT), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    If CommonRfid.IsDate(importItem.REQUEST_DT, "yyyyMMdd") = False Then
                        '日付変換エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REQUEST_DT), _Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU))
                        blnReturn = False
                    End If

                    '◆申請社員コード
                    If importItem.REQ_EMP_CD.Length > itemlength.EmployeeCd Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REQ_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    If CommonRfid.IsHanEiKigou(importItem.REQ_EMP_CD) = False Then
                        '半角英数記号エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REQ_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU))
                        blnReturn = False
                    End If
                    If Dao.ToolEmployee.ExistEmployeeCd(_db, importItem.REQ_EMP_CD) = False Then
                        '社員Mに存在しない
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REQ_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB))
                        blnReturn = False
                    Else
                        '社員Mに存在する
                        If Dao.ToolEmployee.GetAuthDiv(importItem.REQ_EMP_CD) = AUTH_DIV.SuperAdmin Then
                            'スーパー管理者の場合
                            setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REQ_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_EMPLOYEECD))
                            blnReturn = False
                        End If
                    End If

                    '◆担当社員コード
                    If importItem.CHARGE_EMP_CD.Length > itemlength.EmployeeCd Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_CHARGE_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    If CommonRfid.IsHanEiKigou(importItem.CHARGE_EMP_CD) = False Then
                        '半角英数記号エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_CHARGE_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU))
                        blnReturn = False
                    End If
                    If Dao.ToolEmployee.ExistEmployeeCd(_db, importItem.REQ_EMP_CD) = False Then
                        '社員Mに存在しない
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_CHARGE_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB))
                        blnReturn = False
                    Else
                        '社員Mに存在する
                        If Dao.ToolEmployee.GetAuthDiv(importItem.CHARGE_EMP_CD) = AUTH_DIV.SuperAdmin Then
                            'スーパー管理者の場合
                            setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_CHARGE_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_EMPLOYEECD))
                            blnReturn = False
                        End If
                    End If

                    '◆実貸出開始日
                    If importItem.REAL_START_DT.Length < itemlength.RealStartDt Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE), _Message.getMessage(CommonLabel.MSG_ERR_SHORTAGE_STR_LEN))
                        blnReturn = False
                    End If
                    If importItem.REAL_START_DT.Length > itemlength.RealStartDt Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    If CommonRfid.IsNumber(importItem.REAL_START_DT) = False Then
                        '数値チェックエラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE), _Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU))
                        blnReturn = False
                    End If
                    If CommonRfid.IsDate(importItem.REAL_START_DT, "yyyyMMdd") = False Then
                        '日付変換エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE), _Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU))
                        blnReturn = False
                    End If

                    '◆実貸出終了日
                    If importItem.REAL_END_DT.Length < itemlength.RealEndDt Then
                        '項目長不足エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE), _Message.getMessage(CommonLabel.MSG_ERR_SHORTAGE_STR_LEN))
                        blnReturn = False
                    End If
                    If importItem.REAL_END_DT.Length > itemlength.RealEndDt Then
                        '項目長超過エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    If CommonRfid.IsNumber(importItem.REAL_END_DT) = False Then
                        '数値チェックエラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE), _Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU))
                        blnReturn = False
                    End If
                    If CommonRfid.IsDate(importItem.REAL_END_DT, "yyyyMMdd") = False Then
                        '日付変換エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE), _Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU))
                        blnReturn = False
                    End If

                    '◆貸出先コード
                    If importItem.LEASE_PLACE_CD.Length > itemlength.LeasePlaceCd Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    If CommonRfid.IsHanEiKigou(importItem.LEASE_PLACE_CD) = False Then
                        '半角英数記号エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD), _Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU))
                        blnReturn = False
                    End If
                    If Dao.ToolLeasePlace.ExistLeasePlaceCd(_db, importItem.LEASE_PLACE_CD) = False Then
                        '貸出先Mに存在しない
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD), _Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB))
                        blnReturn = False
                    End If

                    '◆貸出用途
                    If prmLstReservMethod.Exists(Function(x) x.Key1 = importItem.LEASE_USE) = False Then
                        '設定Mに存在しない貸出用途はエラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_YOUTO), _Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB))
                        blnReturn = False
                    End If
                    If importItem.LEASE_USE.Length > itemlength.LeaseUse Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_LEASE_YOUTO), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆送り先名1
                    If importItem.DEST_NAME1.Length > itemlength.DestName Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_DEST_NAME1), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆送り先名2
                    If importItem.DEST_NAME2.Length > itemlength.DestName Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_DEST_NAME2), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '2023/03/22 Kawamoto ADD START
                    '◆貸出確認書宛名
                    If importItem.TO_DEST_NAME.Length > itemlength.ToDestName Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_TO_DEST_NAME), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    '2023/03/22 Kawamoto ADD END

                    '◆送り先住所1
                    If importItem.DEST_ADDRESS1.Length > itemlength.DestAddress Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_ADDRESS1), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆送り先住所2
                    If importItem.DEST_ADDRESS2.Length > itemlength.DestAddress Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_ADDRESS2), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆送り先住所3
                    If importItem.DEST_ADDRESS3.Length > itemlength.DestAddress Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_ADDRESS3), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆送り先電話番号
                    If importItem.DEST_PHONE_NUM.Length > itemlength.PhoneNumber Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_SEND_PHONE_NUMBER), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆送り先郵便番号
                    If importItem.DEST_ZIP_CODE.Length > itemlength.ZipCode Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_SEND_ZIP_CODE), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '2023/03/22 Kawamoto ADD START
                    '◆返送先(貸出先コード)
                    If importItem.RET_LEASE_PLACE_CD.Length > itemlength.LeasePlaceCd Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_CODE), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    If CommonRfid.IsHanEiKigou(importItem.RET_LEASE_PLACE_CD) = False Then
                        '半角英数記号エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_CODE), _Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU))
                        blnReturn = False
                    End If
                    If Not String.IsNullOrEmpty(importItem.RET_LEASE_PLACE_CD) AndAlso Not Dao.ToolLeasePlace.ExistLeasePlaceCd(_db, importItem.RET_LEASE_PLACE_CD, False) Then
                        '貸出先Mに存在しない
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_CODE), _Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB))
                        blnReturn = False
                    End If

                    '◆返送先住所1
                    If importItem.RET_ADDRESS1.Length > itemlength.RetAddress Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_ADDRESS1), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆返送先住所2
                    If importItem.RET_ADDRESS2.Length > itemlength.RetAddress Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_ADDRESS2), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆返送先住所3
                    If importItem.RET_ADDRESS3.Length > itemlength.RetAddress Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_ADDRESS3), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆返送先郵便番号
                    If importItem.RET_ZIP_CODE.Length > itemlength.ZipCode Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_ZIP_CODE), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If
                    '2023/03/22 Kawamoto ADD END

                    '◆備考
                    If importItem.REMARK.Length > itemlength.Remark Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_REMARK), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆社内備考
                    If importItem.REMARK_INTERNAL.Length > itemlength.Remark Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_SHANAI_BIKO), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆送り状番号
                    If importItem.DEST_NO.Length > itemlength.DestNo Then
                        '項目長エラー
                        setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_SEND_DEST_NUMBER), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                        blnReturn = False
                    End If

                    '◆資産番号
                    For asset_i As Integer = 0 To importItem.ASSET_INFO_LIST.Count - 1
                        If importItem.ASSET_INFO_LIST(asset_i).ASSET_NO.Length > itemlength.AssetNo Then
                            '項目長エラー
                            setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_ASSET_NO), _Message.getMessage(CommonLabel.MSG_ERR_DATAOVER))
                            blnReturn = False
                        End If
                    Next
                Next

                Return blnReturn
            Catch ex As Exception
                logger.Error(ex.Message)
                Return False
            End Try
        End Function
#End Region

#Region "CSV取込 処理区分別チェック処理"
        ''' <summary>
        ''' CSV取込 処理区分別チェック処理 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="prmlstNotDupChkReserveId">登録時、重複チェックなし予約番号リスト</param>
        ''' <returns></returns>
        Private Function FuncProcDivCheck(ByVal prmlstNotDupChkReserveId As List(Of String)) As Boolean
            Try
                Dim blnReturn As Boolean = True

                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '◆CSV取込済みかチェック
                If _CsvImportItems.Count <= 0 Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NoSelect
                    Return False
                End If

                Dim row_i As Integer = 1    'ヘッダーを除くため1から開始
                For Each importItem As ReserveCsvImportModel In _CsvImportItems
                    row_i += 1

                    '◆処理区分
                    Select Case importItem.PROCESS_DIV
                        Case CsvImportProcDiv.Insert
                            '登録
                            If FuncProcDivInsCheck(row_i, importItem, prmlstNotDupChkReserveId) = False Then
                                blnReturn = False
                            End If
                        Case CsvImportProcDiv.Update
                            '更新
                            If FuncProcDivUpdCheck(row_i, importItem) = False Then
                                blnReturn = False
                            End If

                        Case CsvImportProcDiv.Delete
                            '削除の場合、チェックなし
                            Continue For
                    End Select
                Next

                If blnReturn = False Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                End If

                Return blnReturn
            Catch ex As Exception
                logger.Error(ex.Message)
                Return False
            End Try
        End Function
#End Region

#Region "処理区分 登録時のチェック処理"
        ''' <summary>
        ''' 処理区分 登録時のチェック処理 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="index">行番号</param>
        ''' <param name="importModel">CSV取込データ(1行分)</param>
        ''' <param name="prmlstNotDupChkReserveId">登録時、重複チェックなし予約番号リスト</param>
        ''' <returns></returns>
        Private Function FuncProcDivInsCheck(ByVal index As Integer,
                                             ByVal importModel As ReserveCsvImportModel,
                                             ByVal prmlstNotDupChkReserveId As List(Of String)) As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            '◆実貸出開始日
            '--入力されていない場合、エラー
            If importModel.REAL_START_DT = 0 Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE), _Message.getMessage(CommonLabel.MSG_ERR_REAL_START_DT_EMPTY))
                blnReturn = False
            End If
            '非稼働日の場合、エラー
            If Dao.ToolReserve.GetNotWorkFlg(importModel.REAL_START_DT) = NotWorkFlg.Yes Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE), _Message.getMessage(CommonLabel.MSG_ERR_NOT_WORK_FRG_YES))
                blnReturn = False
            End If

            '◆実貸出終了日
            '入力されていない場合、エラー
            If importModel.REAL_END_DT = 0 Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE), _Message.getMessage(CommonLabel.MSG_ERR_REAL_START_DT_EMPTY))
                blnReturn = False
            End If
            '非稼働日の場合、エラー
            If Dao.ToolReserve.GetNotWorkFlg(importModel.REAL_END_DT) = NotWorkFlg.Yes Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE), _Message.getMessage(CommonLabel.MSG_ERR_NOT_WORK_FRG_YES))
                blnReturn = False
            End If

            '◆実貸出開始日＞実貸出終了日の場合、エラー
            If importModel.REAL_START_DT > importModel.REAL_END_DT Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE), _Message.getMessage(CommonLabel.MSG_ERR_REAL_END_DT_KAKOBI))
                blnReturn = False
            End If

            '◆貸出先
            If importModel.LEASE_PLACE_CD = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_LEASE_PLACE_CD))
                blnReturn = False
            End If
            '◆送り先名
            If importModel.DEST_NAME1 = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_SEND_NAME), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_DEST_NAME))
                blnReturn = False
            End If
            '2023/03/22 Kawamoto ADD START
            '◆貸出確認書宛名
            If importModel.TO_DEST_NAME = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_TO_DEST_NAME), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_TO_DEST_NAME))
                blnReturn = False
            End If
            '2023/03/22 Kawamoto ADD END
            '◆送り先住所
            If importModel.DEST_ADDRESS1 = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_SEND_ADDRESS), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_DEST_ADDRESS))
                blnReturn = False
            End If
            '◆申請日
            If importModel.REQUEST_DT = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REQUEST_DT), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_REQUEST_DT))
                blnReturn = False
            End If
            '◆申請社員コード
            If importModel.REQ_EMP_CD = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REQ_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_REQ_EMP_CD))
                blnReturn = False
            End If
            '◆担当社員コード
            If importModel.CHARGE_EMP_CD = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_CHARGE_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_CHARGE_EMP_CD))
                blnReturn = False
            End If
            '◆貸出用途
            If importModel.LEASE_USE = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_LEASE_YOUTO), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_LEASE_USE))
                blnReturn = False
            End If
            '2023/03/22 Kawamoto ADD START
            '◆返送先(貸出先コード)
            If importModel.RET_LEASE_PLACE_CD = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_CODE), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_RET_LEASE_PLACE_CD))
                blnReturn = False
            End If
            '◆返送先住所1
            If importModel.RET_ADDRESS1 = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_ADDRESS1), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_RET_ADDRESS1))
                blnReturn = False
            End If
            '◆返送先郵便番号
            If importModel.RET_ZIP_CODE = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_ZIP_CODE), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_RET_ZIP_CODE))
                blnReturn = False
            End If
            '2023/03/22 Kawamoto ADD END

            '管理ロケ大区分を取得
            Dim mngLocLCd As String = String.Empty
            For Each assetInfo As ReserveCsvImportModel.AssetInfo In importModel.ASSET_INFO_LIST
                If assetInfo.MNG_LOC_L_CD <> String.Empty Then
                    '空白でないものを取得する
                    mngLocLCd = assetInfo.MNG_LOC_L_CD
                    Exit For
                End If
            Next

            '◆貸出期間チェック
            '一般者か又は管理者でもU_HUB権限があり自拠点でない場合のチェックが必要かを取得
            Dim isNeedCheck As Boolean = GetIsNeedCheck(_LoginInfo, mngLocLCd)
            If importModel.LEASE_USE <> String.Empty AndAlso isNeedCheck Then
                '一般者の場合のみ、チェックする。
                Dim iTargetDate As Integer = 0

                Dim iCnt As Integer = 1
                While iCnt > 0
                    Dim Youto As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method.KeyDiv, iCnt, CommonSettingKey.Reservation_Method.Key2, CommonSettingKey.Reservation_Method.Key3)
                    If Youto Is Nothing Then
                        iCnt = 0
                    Else
                        If importModel.LEASE_USE = Youto.DATA1 Then
                            '対象日付取得(貸出可能期間)
                            iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(importModel.REAL_START_DT, "yyyyMMdd")), Youto.DATA2 - 1, Youto.DATA3)
                            If iTargetDate = 0 Then
                                '0が帰ってきた場合、エラーメッセージを出す。
                                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_LEASE_YOUTO), _Message.getMessage(CommonLabel.MSG_ERR_DATE_SETTEIGAI))
                                blnReturn = False
                            End If
                            Exit While
                        End If
                        iCnt += 1
                    End If
                End While

                If importModel.REAL_END_DT > iTargetDate Then
                    setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_LEASE_YOUTO), _Message.getMessage(CommonLabel.MSG_ERR_REAL_DT_TOO_LONG))
                    blnReturn = False
                End If
            End If

            '◆管理ロケ大区分が全て一致するか確認
            For Each assetInfo As ReserveCsvImportModel.AssetInfo In importModel.ASSET_INFO_LIST
                If mngLocLCd <> assetInfo.MNG_LOC_L_CD Then
                    setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_ASSET_NO), String.Format(_Message.getMessage(CommonLabel.MSG_ERR_NOT_ASSET_MNG_LOC_CD), mngLocLCd, assetInfo.MNG_LOC_L_CD))
                    blnReturn = False
                End If
            Next

            '◆予約期間の重複確認
            For Each row As ReserveDetailViewModel In importModel.RESERVE_DETAIL
                '重複チェックしない予約番号はスルー
                If prmlstNotDupChkReserveId.Contains(importModel.RESERVE_ID) Then
                    Continue For
                End If

                If Dao.ToolReserve.GetReserveMainteJufukuSuuEF(_db, row.ASSET_NO, 0, 0, row.REAL_START_DT, row.REAL_END_DT) > 0 Then
                    setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_PERIOD), String.Format(_Message.getMessage(CommonLabel.MSG_ERR_YOYAKU_DT_JUUFUKU), row.ASSET_NO))
                    blnReturn = False
                End If
            Next

            Return blnReturn
        End Function
#End Region

#Region "処理区分 更新時のチェック処理"
        ''' <summary>
        ''' 処理区分 更新時のチェック処理 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="index">行番号</param>
        ''' <param name="importModel">CSV取込データ(1行分)</param>
        ''' <returns></returns>
        Private Function FuncProcDivUpdCheck(ByVal index As Integer, ByVal importModel As ReserveCsvImportModel) As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            'DBのデータを取得する
            Dim model As LendingEntryModel = db.LendingEntry.Find(Long.Parse(importModel.RESERVE_ID))

            '◆実貸出開始日
            '--入力されていない場合、エラー
            If importModel.REAL_START_DT = 0 Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE), _Message.getMessage(CommonLabel.MSG_ERR_REAL_START_DT_EMPTY))
                blnReturn = False
            End If
            '非稼働日の場合、エラー
            If Dao.ToolReserve.GetNotWorkFlg(importModel.REAL_START_DT) = NotWorkFlg.Yes Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_START_DATE), _Message.getMessage(CommonLabel.MSG_ERR_NOT_WORK_FRG_YES))
                blnReturn = False
            End If

            '◆実貸出終了日
            '入力されていない場合、エラー
            If importModel.REAL_END_DT = 0 Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE), _Message.getMessage(CommonLabel.MSG_ERR_REAL_START_DT_EMPTY))
                blnReturn = False
            End If
            '非稼働日の場合、エラー
            If Dao.ToolReserve.GetNotWorkFlg(importModel.REAL_END_DT) = NotWorkFlg.Yes Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE), _Message.getMessage(CommonLabel.MSG_ERR_NOT_WORK_FRG_YES))
                blnReturn = False
            End If

            '◆実貸出開始日＞実貸出終了日の場合、エラー
            If importModel.REAL_START_DT > importModel.REAL_END_DT Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REAL_LENDING_END_DATE), _Message.getMessage(CommonLabel.MSG_ERR_REAL_END_DT_KAKOBI))
                blnReturn = False
            End If

            '◆貸出先
            If importModel.LEASE_PLACE_CD = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_LEASE_PLACE_CD))
                blnReturn = False
            End If
            '◆送り先名
            If importModel.DEST_NAME1 = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_SEND_NAME), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_DEST_NAME))
                blnReturn = False
            End If
            '2023/03/22 Kawamoto ADD START
            '◆貸出確認書宛名
            If importModel.TO_DEST_NAME = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_TO_DEST_NAME), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_TO_DEST_NAME))
                blnReturn = False
            End If
            '2023/03/22 Kawamoto ADD END
            '◆送り先住所
            If importModel.DEST_ADDRESS1 = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_SEND_ADDRESS), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_DEST_ADDRESS))
                blnReturn = False
            End If
            '◆申請日
            If importModel.REQUEST_DT = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REQUEST_DT), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_REQUEST_DT))
                blnReturn = False
            End If
            '◆申請社員コード
            If importModel.REQ_EMP_CD = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_REQ_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_REQ_EMP_CD))
                blnReturn = False
            End If
            '◆担当社員コード
            If importModel.CHARGE_EMP_CD = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_CHARGE_EMP_CD), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_CHARGE_EMP_CD))
                blnReturn = False
            End If
            '◆貸出用途
            If importModel.LEASE_USE = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_LEASE_YOUTO), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_LEASE_USE))
                blnReturn = False
            End If
            '2023/03/22 Kawamoto ADD START
            '◆返送先(貸出先コード)
            If importModel.RET_LEASE_PLACE_CD = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_CODE), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_RET_LEASE_PLACE_CD))
                blnReturn = False
            End If
            '◆返送先住所1
            If importModel.RET_ADDRESS1 = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_ADDRESS1), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_RET_ADDRESS1))
                blnReturn = False
            End If
            '◆返送先郵便番号
            If importModel.RET_ZIP_CODE = String.Empty Then
                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_RETURN_PLACE_ZIP_CODE), _Message.getMessage(CommonLabel.MSG_ERR_NOT_INPUT_RET_ZIP_CODE))
                blnReturn = False
            End If
            '2023/03/22 Kawamoto ADD END

            '管理ロケ大区分を取得
            Dim mngLocLCd As String = String.Empty
            For Each assetInfo As ReserveCsvImportModel.AssetInfo In importModel.ASSET_INFO_LIST
                If assetInfo.MNG_LOC_L_CD <> String.Empty Then
                    '空白でないものを取得する
                    mngLocLCd = assetInfo.MNG_LOC_L_CD
                    Exit For
                End If
            Next

            '◆貸出期間チェック
            '一般者か又は管理者でもU_HUB権限があり自拠点でない場合のチェックが必要かを取得
            Dim isNeedCheck As Boolean = GetIsNeedCheck(_LoginInfo, mngLocLCd)
            If importModel.LEASE_USE <> String.Empty AndAlso isNeedCheck Then
                '一般者の場合のみ、チェックする。
                Dim iTargetDate As Integer = 0

                Dim iCnt As Integer = 1
                While iCnt > 0
                    Dim Youto As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method.KeyDiv, iCnt, CommonSettingKey.Reservation_Method.Key2, CommonSettingKey.Reservation_Method.Key3)
                    If Youto Is Nothing Then
                        iCnt = 0
                    Else
                        If importModel.LEASE_USE = Youto.DATA1 Then
                            '対象日付取得(貸出可能期間)
                            If model.REAL_END_DT - model.REAL_START_DT >= importModel.REAL_END_DT - importModel.REAL_START_DT Then
                                '期間が変わらないまたは縮んだ場合、貸出用途の内容２-1と3で実質貸出開始日～終了日チェック
                                iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(model.REAL_START_DT, "yyyyMMdd")), Youto.DATA2 - 1, Youto.DATA3)  '2021/12/07 nv miyaji 仕様変更
                            Else
                                '期間が延びた場合、現状の内容5と6でチェック
                                iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(model.REAL_END_DT, "yyyyMMdd")), Youto.DATA5, Youto.DATA6)  '2021/12/07 nv miyaji 仕様変更
                            End If
                            If iTargetDate = 0 Then
                                '0が帰ってきた場合、エラーメッセージを出す。
                                setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_LEASE_YOUTO), _Message.getMessage(CommonLabel.MSG_ERR_DATE_SETTEIGAI))
                                blnReturn = False
                            End If
                            Exit While
                        End If
                        iCnt += 1
                    End If
                End While

                If importModel.REAL_END_DT > iTargetDate Then
                    setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_LEASE_YOUTO), _Message.getMessage(CommonLabel.MSG_ERR_REAL_DT_TOO_LONG))
                    blnReturn = False
                End If
            End If

            '◆管理ロケ大区分が全て一致するか確認
            For Each assetInfo As ReserveCsvImportModel.AssetInfo In importModel.ASSET_INFO_LIST
                If mngLocLCd <> assetInfo.MNG_LOC_L_CD Then
                    setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_ASSET_NO), String.Format(_Message.getMessage(CommonLabel.MSG_ERR_NOT_ASSET_MNG_LOC_CD), mngLocLCd, assetInfo.MNG_LOC_L_CD))
                    blnReturn = False
                End If
            Next

            '◆予約期間の重複確認
            For Each row As ReserveDetailViewModel In importModel.RESERVE_DETAIL
                '明細が引継元でない　かつ　返却済みでない時にチェック
                If row.HANDOVER_DIV <> HandOverDiv.HikitsugiMoto AndAlso row.RESERVE_STATUS <> ReserveStatus.HenkyakuSumi Then  '返却済みでない場合
                    If Dao.ToolReserve.GetReserveMainteJufukuSuuEF(_db, row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_START_DT, row.REAL_END_DT) > 0 Then
                        setDspErrContent(index, _Message.getMessage(CommonLabel.LABEL_PERIOD), String.Format(_Message.getMessage(CommonLabel.MSG_ERR_YOYAKU_DT_JUUFUKU), row.ASSET_NO))
                        blnReturn = False
                    End If
                End If
            Next

            Return blnReturn
        End Function
#End Region

#Region "処理区分別　予約番号存在チェック処理"
        ''' <summary>
        ''' 処理区分別　予約番号存在チェック処理  2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="procDiv"></param>
        ''' <param name="reserveId"></param>
        ''' <returns></returns>
        Private Function FuncExistReserveId(ByVal procDiv As String, ByVal reserveId As Long, ByRef errMsg As String) As Boolean
            Try
                Dim isExist As Boolean = Dao.ToolReserve.ExistReserveId(reserveId)

                If isExist Then
                    '処理区分が登録　かつ　データありはエラー
                    If procDiv = CsvImportProcDiv.Insert Then
                        errMsg = _Message.getMessage(CommonLabel.MSG_ERR_EXITS_RESERVE_ID)
                        Return False
                    End If
                Else
                    '処理区分が更新・削除　かつ　データなしはエラー
                    If procDiv <> CsvImportProcDiv.Insert Then
                        errMsg = _Message.getMessage(CommonLabel.MSG_ERR_NOT_EXITS_RESERVE_ID)
                        Return False
                    End If
                End If

                Return True
            Catch ex As Exception
                logger.Error(ex.Message)
                Return False
            End Try
        End Function
#End Region

#Region "CSV取込　資産情報設定処理"
        ''' <summary>
        ''' CSV取込　資産情報設定処理
        ''' </summary>
        ''' <returns></returns>
        Private Function FncSetAssetInfo() As Boolean
            Dim blnReturn As Boolean = True
            Try
                '◆CSV取込済みかチェック
                If _CsvImportItems.Count <= 0 Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NoSelect
                    blnReturn = False
                End If

                '◆処理区分が登録・更新のデータのみ対象
                Dim row_i As Integer = 1    'ヘッダーを除くため1から開始
                For Each importItem As ReserveCsvImportModel In _CsvImportItems
                    row_i += 1

                    If importItem.PROCESS_DIV = CsvImportProcDiv.Delete Then
                        '削除時は設定なし
                        Continue For
                    End If

                    For Each assetInfo In importItem.ASSET_INFO_LIST
                        If String.IsNullOrEmpty(assetInfo.ASSET_NO) Then
                            '資産番号が空白
                            Continue For
                        End If

                        '資産マスタ情報取得
                        Dim assetModel As AssetListModel = _db.AssetList.Find(assetInfo.ASSET_NO)
                        If Not IsNothing(assetModel) Then
                            '資産が存在する場合、管理ロケ大区分を保持
                            assetInfo.MNG_LOC_L_CD = assetModel.MNG_LOC_L_CD

                            '資産が貸出不可状態の場合、エラーメッセージ
                            If assetModel.LEASE_STATUS = AssetLeaseStatus.Ng Then
                                setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_ASSET_NO), String.Format(_Message.getMessage(CommonLabel.MSG_ERR_FORMAT_STATUS_FUKA), assetInfo.ASSET_NO))
                                blnReturn = False
                            End If

                            '資産状態が「保管中」「貸出中」以外の場合、エラーメッセージ
                            If assetModel.ASSET_STATUS <> ASSET_STATUS.Storage AndAlso assetModel.ASSET_STATUS <> ASSET_STATUS.Lending Then
                                setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_ASSET_NO), String.Format(_Message.getMessage(CommonLabel.MSG_ERR_FORMAT_INPUT_ASSET_STATUS), assetInfo.ASSET_NO))
                                blnReturn = False
                            End If

                        Else
                            '資産が存在しない場合
                            setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_ASSET_NO), String.Format(_Message.getMessage(CommonLabel.MSG_ERR_NOT_EXITS_ASSET_NO), assetInfo.ASSET_NO))
                            blnReturn = False
                        End If
                    Next
                Next

                Return blnReturn
            Catch ex As Exception
                logger.Error(ex.Message)
                Return False
            End Try
        End Function
#End Region

#Region "CSV取込　登録・更新用予約明細情報設定"
        ''' <summary>
        ''' CSV取込　登録・更新用予約明細情報設定 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="csvRow">CSVデータ1行分</param>
        ''' <returns></returns>
        Private Function FuncSetInsGridInfo(ByVal csvRow As ReserveCsvImportModel) As List(Of ReserveDetailViewModel)
            Try
                Dim db As New SystemDBContext

                'グリッド情報
                Dim lstReserveDetail As List(Of ReserveDetailViewModel) = New List(Of ReserveDetailViewModel)
                For i As Integer = 0 To csvRow.ASSET_INFO_LIST.Count - 1
                    'IDをキーに資産モデルを取得する
                    Dim modelAsset As AssetListModel = db.AssetList.Find(csvRow.ASSET_INFO_LIST(i).ASSET_NO)

                    Dim rowData As New ReserveDetailViewModel
                    If csvRow.PROCESS_DIV = CsvImportProcDiv.Insert Then
                        '登録
                        rowData.RESERVE_ID = 0
                    Else
                        '更新
                        rowData.RESERVE_ID = Long.Parse(csvRow.RESERVE_ID)
                    End If

                    rowData.ASSET_NO = csvRow.ASSET_INFO_LIST(i).ASSET_NO
                    If IsNothing(modelAsset) OrElse modelAsset.ASSET_NAME Is Nothing Then
                        rowData.AssetName = String.Empty
                    Else
                        rowData.AssetName = modelAsset.ASSET_NAME
                    End If
                    rowData.REAL_START_DT = 0
                    rowData.REAL_END_DT = 0
                    rowData.CHARGE_EMP_CD = String.Empty
                    rowData.LENDING_DT = Nothing
                    rowData.RETURN_DT = Nothing
                    rowData.PR_ST_DT = 0
                    rowData.PR_END_DT = 0
                    rowData.LEASE_CHG_TIMES = 0
                    rowData.HANDOVER_DIV = HandOverDiv.Mi
                    rowData.HANDOVER_RESERVE_ID = Nothing
                    rowData.RESERVE_STATUS = ReserveStatus.Mi
                    If IsNothing(modelAsset) OrElse modelAsset.FREE_ITEM10 Is Nothing Then
                        rowData.SerialNo = String.Empty
                    Else
                        rowData.SerialNo = modelAsset.FREE_ITEM10
                    End If
                    rowData.LeaseKigen = String.Empty
                    rowData.EmployeeName = String.Empty
                    lstReserveDetail.Add(rowData)
                Next

                Return lstReserveDetail
            Catch ex As Exception
                logger.Error(ex.Message)
                Throw ex
            End Try
        End Function
#End Region

#Region "CSV取込  削除用予約明細情報設定"
        ''' <summary>
        ''' CSV取込　削除用予約明細情報設定 2022/7/25 sv sato add
        ''' </summary>
        ''' <returns></returns>
        Private Function FuncSetDelGridInfo(ByVal csvRow As ReserveCsvImportModel) As List(Of ReserveDetailViewModel)
            Try
                Dim db As New SystemDBContext

                '予約明細テーブルを取得
                Dim sbSQL As StringBuilder = New StringBuilder
                'Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
                'T_RESERVE_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT RESERVE.*")
                sbSQL.AppendLine("       ,ISNULL(ASSET.ASSET_NAME,'') As AssetName")
                sbSQL.AppendLine("       ,ISNULL(ASSET.FREE_ITEM10,'') As SerialNo")
                sbSQL.AppendLine("       ,ISNULL(EMP.EMPLOYEE_NAME,'') As EmployeeName")
                sbSQL.AppendLine("FROM T_RESERVE_DETAIL RESERVE")
                sbSQL.AppendLine("INNER JOIN M_ASSET ASSET")
                sbSQL.AppendLine("ON ASSET.ASSET_NO = RESERVE.ASSET_NO ")
                sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE EMP")
                sbSQL.AppendLine("ON EMP.EMPLOYEE_CD = RESERVE.CHARGE_EMP_CD ")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("RESERVE.RESERVE_ID = " & csvRow.RESERVE_ID)
                'params.Add(New SqlParameter("@ReserveId", csvRow.RESERVE_ID))
                sbSQL.AppendLine("ORDER BY RESERVE.ASSET_NO")
                'SQLを実行する
                Dim lstReserveDetail As List(Of ReserveDetailViewModel) = db.ReserveDetailView.SqlQuery(sbSQL.ToString).ToList

                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '稼働日考慮フラグ
                Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
                Dim sWorkDayFlg As String = reservationFunction.DATA2
                '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                Dim sWorkFlg As String = WorkFlg.Jitsubi
                If sWorkDayFlg = WorkDayFlg.Yes Then
                    sWorkFlg = WorkFlg.Kadobi
                End If


                '実貸出開始日文字列、実貸出終了日文字列、期限日を設定する。
                For Each row As ReserveDetailViewModel In lstReserveDetail
                    '期限日
                    row.LeaseKigen = CommonConst.MAX_KIGENBI_STR
                    If row.REAL_START_DT <> 0 AndAlso row.REAL_END_DT <> 0 Then
                        '開始日と終了日に両方値がある場合、資産メンテナンス登録情報取得
                        Dim tblInfo As DataTable = Dao.ToolReserve.GetReserveMainteInfo(row.ASSET_NO, row.RESERVE_ID, row.REAL_START_DT, row.REAL_END_DT, sWorkFlg)
                        If tblInfo IsNot Nothing AndAlso tblInfo.Rows.Count > 0 AndAlso tblInfo.Rows(0).Item("LIMIT_DT") <> CommonConst.MAX_KIGENBI AndAlso tblInfo.Rows(0).Item("LIMIT_DT") >= row.REAL_START_DT Then
                            row.LeaseKigen = CommonRfid.ToStringDateYMD(tblInfo.Rows(0).Item("LIMIT_DT"), "yyyyMMdd")
                        End If
                    End If
                    '実貸出開始日文字列
                    If row.REAL_START_DT <> 0 Then
                        row.RealStartDtString = CommonRfid.ToStringDateYMD(row.REAL_START_DT, "yyyyMMdd")
                    End If
                    '実貸出終了日文字列
                    If row.REAL_END_DT <> 0 Then
                        row.RealEndDtString = CommonRfid.ToStringDateYMD(row.REAL_END_DT, "yyyyMMdd")
                    End If
                Next

                Return lstReserveDetail
            Catch ex As Exception
                logger.Error(ex.Message)
                Return Nothing
            End Try
        End Function
#End Region

#Region "CSV取込　重複チェックなし予約番号リスト作成処理"
        ''' <summary>
        ''' CSV取込　重複チェックなし予約番号リスト作成処理
        ''' </summary>
        ''' <returns></returns>
        Private Function FncCreateNotDupChkDatas() As List(Of String)
            '◆CSV取込済みかチェック
            If _CsvImportItems.Count <= 0 Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NoSelect
                Throw New Exception(_Message.getMessage(CommonLabel.MSG_ERR_ELSE))
            End If

            '◆重複チェックなし予約番号リスト
            Dim lstNotDupChkReserveId As New List(Of String)

            '◆予約番号と処理区分をキーにグループ化して抽出
            '※lstCsvGroupは、予約番号と処理区分を要素としたリストとなる
            Dim lstCsvGroup = _CsvImportItems.
                                    GroupBy(Function(x) New With {Key x.RESERVE_ID, Key x.PROCESS_DIV}).ToList


            '※予約番号が同じレコードの内、処理区分が削除のデータと、削除以外のデータが存在する場合、↴
            '登録・更新時は重複チェックを実施しない
            For Each item In lstCsvGroup
                '予約番号が存在する　かつ　処理区分が削除のデータが存在しない
                If lstCsvGroup.Exists(Function(x) x.Key.RESERVE_ID = item.Key.RESERVE_ID AndAlso
                                                      x.Key.PROCESS_DIV = CsvImportProcDiv.Delete) = False Then
                    Continue For
                End If

                If lstCsvGroup.Exists(Function(x) x.Key.RESERVE_ID = item.Key.RESERVE_ID AndAlso
                                                      x.Key.PROCESS_DIV <> CsvImportProcDiv.Delete) = False Then
                    '予約番号が存在する　かつ　処理区分が登録・更新のデータが存在しない
                    Continue For
                End If

                '重複チェックを実施しない予約番号をセット
                If lstNotDupChkReserveId.Contains(item.Key.RESERVE_ID) = False Then
                    lstNotDupChkReserveId.Add(item.Key.RESERVE_ID)
                End If
            Next

            Return lstNotDupChkReserveId
        End Function
#End Region

#Region "CSV取込　資産毎の予約期間重複チェック処理"
        ''' <summary>
        ''' CSV取込　資産毎の予約期間重複チェック処理
        ''' </summary>
        ''' <returns></returns>
        Private Function FncChkDupReserveRange() As Boolean
            Try
                Dim blnReturn As Boolean = True
                Dim details As IQueryable(Of ReserveDetailModel)

                Dim row_i As Integer = 1    'ヘッダーを除くため1から開始
                For Each reserveItem As ReserveCsvImportModel In _CsvImportItems.Where(Function(x) x.PROCESS_DIV <> CsvImportProcDiv.Delete).ToList
                    row_i += 1

                    '予約明細データ取得
                    details = Nothing
                    details = From record In _db.ReserveDetail Where record.RESERVE_ID = reserveItem.RESERVE_ID

                    '予約期間重複チェック
                    For Each detail In details
                        If Dao.ToolReserve.GetReserveMainteJufukuSuuEF(_db, detail.ASSET_NO, 0, 0, detail.PR_ST_DT, detail.PR_END_DT) > 1 Then
                            setDspErrContent(row_i, _Message.getMessage(CommonLabel.LABEL_PERIOD), String.Format(_Message.getMessage(CommonLabel.MSG_ERR_YOYAKU_DT_JUUFUKU), detail.ASSET_NO))
                            blnReturn = False
                        End If
                    Next
                Next

                Return blnReturn
            Catch ex As Exception
                logger.Error(ex.Message)
                Return False
            End Try
        End Function
#End Region

#Region "実貸出開始日の設定処理"
        ''' <summary>
        ''' 実貸出開始日の設定処理 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="reserveDetail">予約明細情報</param>
        ''' <param name="sFromDt">貸出開始日</param>
        ''' <param name="sToDt">貸出終了日</param>
        Public Sub SetRealStartDt(ByRef reserveDetail As List(Of ReserveDetailViewModel), ByVal sFromDt As String, ByVal sToDt As String)
            Dim db As New SystemDBContext

            '情報がない場合、更新はしない。
            If reserveDetail Is Nothing OrElse reserveDetail.Count = 0 Then
                Return
            End If

            'yyyy/MM/dd形式に変換
            sFromDt = FuncConvStrDateSlash(sFromDt)
            sToDt = FuncConvStrDateSlash(sToDt)

            '稼働日考慮フラグ
            Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
            Dim sWorkDayFlg As String = reservationFunction.DATA2
            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If

            'リストの値を更新
            For Each row As ReserveDetailViewModel In reserveDetail
                row.JuufukuAriFrom = False  '初期値設定
                row.JuufukuAriNaka = False  '初期値設定
                row.JuufukuAriTo = False  '初期値設定
                If row.RESERVE_STATUS <> ReserveStatus.HenkyakuSumi Then
                    '期限日
                    row.LeaseKigen = CommonConst.MAX_KIGENBI_STR
                    If sFromDt <> String.Empty AndAlso sToDt <> String.Empty Then
                        '開始日と終了日に両方値がある場合、資産メンテナンス登録情報取得
                        Dim tblInfo As List(Of ReserveMainteInfoSF) = Dao.ToolReserve.GetReserveMainteInfoEF(_db, row.ASSET_NO, row.RESERVE_ID, CInt(CDate(sFromDt).ToString("yyyyMMdd")), CInt(CDate(sToDt).ToString("yyyyMMdd")), sWorkFlg)
                        If tblInfo IsNot Nothing AndAlso tblInfo.Count > 0 AndAlso tblInfo(0).LIMIT_DT <> CommonConst.MAX_KIGENBI AndAlso tblInfo(0).LIMIT_DT >= CInt(CDate(sFromDt).ToString("yyyyMMdd")) Then
                            row.LeaseKigen = CommonRfid.ToStringDateYMD(tblInfo(0).LIMIT_DT, "yyyyMMdd")
                        End If
                    End If

                    '実質貸出開始日と表示文字列をセット
                    If sFromDt = String.Empty Then
                        '実質貸出開始日
                        row.REAL_START_DT = 0
                        '貸出予実開始日
                        row.PR_ST_DT = 0
                        '実貸出開始日表示文字列
                        row.RealStartDtString = String.Empty
                    Else
                        '実質貸出開始日
                        row.REAL_START_DT = CInt(CDate(sFromDt).ToString("yyyyMMdd"))
                        '貸出予実開始日
                        row.PR_ST_DT = CInt(CDate(sFromDt).ToString("yyyyMMdd"))
                        '実貸出開始日表示文字列
                        row.RealStartDtString = CDate(sFromDt).ToString("yyyy/MM/dd")
                    End If

                    '実質貸出開始日と実質貸出終了日に値が入っている時
                    If row.REAL_START_DT <> 0 AndAlso row.REAL_END_DT <> 0 AndAlso row.REAL_START_DT <= row.REAL_END_DT Then
                        '予約期間の重複確認(From)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_START_DT, row.REAL_START_DT) > 0 Then
                            row.JuufukuAriFrom = True
                        End If
                        '予約期間の重複確認(From+1～To-1)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, CInt(CDate(sFromDt).AddDays(1).ToString("yyyyMMdd")), CInt(CDate(sToDt).AddDays(-1).ToString("yyyyMMdd"))) > 0 Then
                            row.JuufukuAriNaka = True
                        End If
                        '予約期間の重複確認(To)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_END_DT, row.REAL_END_DT) > 0 Then
                            row.JuufukuAriTo = True
                        End If
                        '期間From＝期間Toで、重複有りの時、「～」も赤にする。
                        If sFromDt = sToDt AndAlso row.JuufukuAriFrom = True Then
                            row.JuufukuAriNaka = True
                        End If
                    End If
                End If
            Next
        End Sub
#End Region

#Region "実貸出終了日の設定処理"
        ''' <summary>
        ''' 実貸出終了日の設定処理
        ''' </summary>
        ''' <param name="sFromDt">貸出開始日</param>
        ''' <param name="sToDt">貸出終了日</param>
        Public Sub SetRealEndDt(ByVal index As Integer, ByRef importModel As ReserveCsvImportModel, ByVal sFromDt As String, ByVal sToDt As String)
            Dim db As New SystemDBContext

            '資産情報にセットする実貸出終了日
            Dim sUpdEndDt As String = sToDt

            'yyyy/MM/dd形式に変換
            sFromDt = FuncConvStrDateSlash(sFromDt)
            sUpdEndDt = FuncConvStrDateSlash(sUpdEndDt)

            Dim isReturn As Boolean = False 'グリッドを更新せずに戻るエラーがあるフラグ

            'ユーザー貸出終了日を設定
            If String.IsNullOrEmpty(sUpdEndDt) Then
                'ユーザー貸出終了日を更新
                importModel.USER_END_DT = String.Empty
            Else
                '返却リードタイム
                Dim setting_HenkyakuLT As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Return_LeadTime.KeyDiv, CommonSettingKey.Return_LeadTime.Key1, CommonSettingKey.Return_LeadTime.Key2, CommonSettingKey.Return_LeadTime.Key3, True)
                Dim iHenkyakuLT As Integer = CInt(setting_HenkyakuLT.DATA1)
                '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                Dim sHenkyakuWorkFlg As String = WorkFlg.Jitsubi
                If setting_HenkyakuLT.DATA2 = WorkFlg.Kadobi Then
                    sHenkyakuWorkFlg = WorkFlg.Kadobi
                End If

                '対象日付取得
                Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(CDate(sUpdEndDt), iHenkyakuLT, sHenkyakuWorkFlg)
                If iTargetDate = 0 Then
                    '0が帰ってきた場合、エラーメッセージを出す。
                    isReturn = True
                    sUpdEndDt = String.Empty
                    importModel.USER_END_DT = String.Empty
                    setDspErrContent(index, _Message.getMessage(CommonLabel.USER_END_DT), _Message.getMessage(CommonLabel.MSG_ERR_DATE_SETTEIGAI))

                Else
                    'ユーザー貸出終了日を更新
                    '対象日付が貸出開始日より前になった場合、ユーザー貸出終了日に貸出開始日を入れる。
                    If sFromDt <> String.Empty AndAlso iTargetDate < CInt(CDate(sFromDt).ToString("yyyyMMdd")) Then
                        iTargetDate = CInt(CDate(sFromDt).ToString("yyyyMMdd"))
                    End If
                    importModel.USER_END_DT = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(iTargetDate))
                End If
            End If

            '明細がないか、更新せずに戻るエラーがある場合
            If IsNothing(importModel.RESERVE_DETAIL) OrElse importModel.RESERVE_DETAIL.Count = 0 OrElse isReturn Then
                '更新しない
                Return
            End If

            '稼働日考慮フラグ
            Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
            Dim sWorkDayFlg As String = reservationFunction.DATA2
            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If

            'リストの値を更新
            For Each row As ReserveDetailViewModel In importModel.RESERVE_DETAIL
                row.JuufukuAriFrom = False  '初期値設定
                row.JuufukuAriNaka = False  '初期値設定
                row.JuufukuAriTo = False  '初期値設定
                If row.RESERVE_STATUS <> ReserveStatus.HenkyakuSumi Then
                    '期限日
                    row.LeaseKigen = CommonConst.MAX_KIGENBI_STR
                    If sFromDt <> String.Empty AndAlso sUpdEndDt <> String.Empty Then
                        '開始日と終了日に両方値がある場合、資産メンテナンス登録情報取得
                        Dim tblInfo As List(Of ReserveMainteInfoSF) = Dao.ToolReserve.GetReserveMainteInfoEF(_db, row.ASSET_NO, row.RESERVE_ID, CInt(CDate(sFromDt).ToString("yyyyMMdd")), CInt(CDate(sUpdEndDt).ToString("yyyyMMdd")), sWorkFlg)
                        If tblInfo IsNot Nothing AndAlso tblInfo.Count > 0 AndAlso tblInfo(0).LIMIT_DT <> CommonConst.MAX_KIGENBI AndAlso tblInfo(0).LIMIT_DT >= CInt(CDate(sFromDt).ToString("yyyyMMdd")) Then
                            row.LeaseKigen = CommonRfid.ToStringDateYMD(tblInfo(0).LIMIT_DT, "yyyyMMdd")
                        End If
                    End If

                    '実質貸出開始日と表示文字列をセット
                    If sUpdEndDt = String.Empty Then
                        '実貸出終了日
                        row.REAL_END_DT = 0
                        '貸出予実終了日
                        row.PR_END_DT = 0
                        '実貸出終了日表示文字列
                        row.RealEndDtString = String.Empty
                    Else
                        '実貸出終了日
                        row.REAL_END_DT = CInt(CDate(sUpdEndDt).ToString("yyyyMMdd"))
                        '貸出予実終了日
                        row.PR_END_DT = CInt(CDate(sUpdEndDt).ToString("yyyyMMdd"))
                        '実貸出終了日表示文字列
                        row.RealEndDtString = CDate(sUpdEndDt).ToString("yyyy/MM/dd")
                    End If


                    '実質貸出開始日と実質貸出終了日に値が入っている時
                    If row.REAL_START_DT <> 0 AndAlso row.REAL_END_DT <> 0 AndAlso row.REAL_START_DT <= row.REAL_END_DT Then
                        '予約期間の重複確認(From)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuuEF(_db, row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_START_DT, row.REAL_START_DT) > 0 Then
                            row.JuufukuAriFrom = True
                        End If
                        '予約期間の重複確認(From+1～To-1)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuuEF(_db, row.ASSET_NO, row.RESERVE_ID, 0, CInt(CDate(sFromDt).AddDays(1).ToString("yyyyMMdd")), CInt(CDate(sUpdEndDt).AddDays(-1).ToString("yyyyMMdd"))) > 0 Then
                            row.JuufukuAriNaka = True
                        End If
                        '予約期間の重複確認(To)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuuEF(_db, row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_END_DT, row.REAL_END_DT) > 0 Then
                            row.JuufukuAriTo = True
                        End If

                        '期間From＝期間Toで、重複有りの時、「～」も赤にする。
                        If sFromDt = sUpdEndDt AndAlso row.JuufukuAriFrom = True Then
                            row.JuufukuAriNaka = True
                        End If
                    End If

                End If
            Next
        End Sub
#End Region

#Region "返却済み予約削除のスキップメッセージ作成処理"
        ''' <summary>
        ''' 返却済み予約削除のスキップメッセージ作成処理
        ''' </summary>
        ''' <returns></returns>
        Private Function FncSetRetReserveMsg() As List(Of DataInportResult)
            Try
                '◆CSV取込済みかチェック
                If _CsvImportItems.Count <= 0 Then
                    Return Nothing
                End If

                Dim lstErr As New List(Of DataInportResult)

                '処理区分が削除のデータのみが対象
                Dim row_i As Integer = 1    'ヘッダーを除くため1から開始
                For Each importItem As ReserveCsvImportModel In _CsvImportItems
                    row_i += 1

                    If importItem.PROCESS_DIV <> CsvImportProcDiv.Delete Then
                        Continue For
                    End If

                    '予約データ取得
                    Dim model As LendingEntryModel = _db.LendingEntry.Find(Long.Parse(importItem.RESERVE_ID))
                    If IsNothing(model) Then
                        '取得失敗時
                        Continue For
                    End If

                    If model.RESERVE_STATUS = ReserveStatus.HenkyakuSumi OrElse
                        model.RESERVE_STATUS = ReserveStatus.Lending Then
                        '貸出中もしくは返却済みの場合
                        Dim lstRow As DataInportResult = New DataInportResult
                        lstRow.ErrRowIdx = row_i
                        lstRow.ErrKoumoku = _Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER)

                        'メッセージ設定
                        If model.RESERVE_STATUS = ReserveStatus.Lending Then
                            '貸出中
                            lstRow.ErrGaiyou = _Message.getMessage(CommonLabel.MSG_ERR_LENDING_RESERVE_SKIP)
                        Else
                            '返却済み
                            lstRow.ErrGaiyou = _Message.getMessage(CommonLabel.MSG_ERR_RETRUN_RESERVE_SKIP)
                        End If

                        lstErr.Add(lstRow)
                    End If
                Next

                Return lstErr
            Catch ex As Exception
                logger.Error(ex)
                Return Nothing
            End Try
        End Function
#End Region

#Region "yyyymmdd→yyyy/MM/dd形式に変換処理"
        ''' <summary>
        ''' 日付文字列　区切り文字追加処理
        ''' </summary>
        ''' <param name="strDate">日付文字列</param>
        ''' <returns></returns>
        Private Function FuncConvStrDateSlash(ByVal strDate As String) As String
            Try
                Dim intDate As Integer
                If Integer.TryParse(strDate, intDate) = False Then
                    '変換可能な文字列ではない場合
                    Return strDate
                End If

                Return intDate.ToString("0000/00/00")
            Catch ex As Exception
                logger.Error(ex)
                Return strDate
            End Try
        End Function
#End Region

#Region "各資産の　資産メンテナンス登録情報 セット　&　チェック"
        ''' <summary>
        ''' 各資産の　資産メンテナンス登録情報 セット　チェック 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="lstGrid">List(Of ReserveDetailViewModel)</param>
        ''' <returns>
        ''' チェックOK時:String.Empty  チェックNG時:資産番号
        ''' </returns>
        Public Function CheckAssetMainteInfo(lstGrid As List(Of ReserveDetailViewModel)) As String
            '稼働日考慮フラグ
            Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
            Dim sWorkDayFlg As String = reservationFunction.DATA2
            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If

            Me._dsMentInfo = New DataSet
            Dim isMentInfoChk As Boolean = True
            Dim sNgAssetNo As String = String.Empty
            Dim iCnt As Integer = 0
            Dim reserveMainteInfo As List(Of ReserveMainteInfoSF)
            For Each row As ReserveDetailViewModel In lstGrid
                sNgAssetNo = row.ASSET_NO                  'NG用に最初にセットしておく

                '資産メンテナンス登録情報取得
                reserveMainteInfo = Dao.ToolReserve.GetReserveMainteInfoEF(_db, row.ASSET_NO, row.RESERVE_ID, row.PR_ST_DT, row.PR_END_DT, sWorkFlg)
                Me._dsMentInfo.Tables.Add(FuncMainteInfoToDataTable(reserveMainteInfo))

                '--引継区分 未引継ぎ　のみチェックする(配列順を考慮し、ここで行なう)
                If row.HANDOVER_DIV = Misc.HandOverDiv.Mi Then

                    If Me._dsMentInfo.Tables(iCnt).Rows(0).Item("MAINTE_EXEC_FLG").ToString = Misc.MainteExecFlg.Yes Then
                        'メンテナンス必要時

                        If Me._dsMentInfo.Tables(iCnt).Rows(0).Item("MAINTE_DIV").ToString = Misc.MainteDiv.Ng Or
                           Me._dsMentInfo.Tables(iCnt).Rows(0).Item("MAINTE_DIV").ToString = Misc.MainteDiv.Chg Then
                            'この場合は、みんな不可
                            isMentInfoChk = False
                            Exit For
                        ElseIf Me._dsMentInfo.Tables(iCnt).Rows(0).Item("MAINTE_DIV").ToString = Misc.MainteDiv.Ok Then
                            'この場合は、みんな可
                        Else
                            If Me._LoginInfo.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.SuperAdmin Or
                               Me._LoginInfo.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Honbu_Admin Or
                               Me._LoginInfo.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Kyoten_Admin Or
                               Me._LoginInfo.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Tsujo_Admin Then
                                'ログインが管理者以上の場合は、OKとする
                            Else
                                '以外は、不可
                                isMentInfoChk = False
                                Exit For
                            End If
                        End If
                    End If
                End If

                iCnt += 1
            Next

            If isMentInfoChk Then
                Return String.Empty
            Else
                Return sNgAssetNo
            End If

        End Function

        ''' <summary>
        ''' 資産メンテナンス登録情報→DataTable変換処理
        ''' </summary>
        ''' <returns></returns>
        Private Function FuncMainteInfoToDataTable(ByVal prmMainteInfo As List(Of ReserveMainteInfoSF)) As DataTable
            Try
                Dim dt As New DataTable
                dt.Columns.Add("MAINTE_EXEC_FLG")
                dt.Columns.Add("MAINTE_DIV")
                dt.Columns.Add("ST_DT")
                dt.Columns.Add("END_DT")
                dt.Columns.Add("LIMIT_DT")

                'Entity Frameworkで取得したデータをDataTableに入れ替える
                For Each mainteInfo As ReserveMainteInfoSF In prmMainteInfo
                    Dim dtRow As DataRow = dt.NewRow
                    dtRow("MAINTE_EXEC_FLG") = mainteInfo.MAINTE_EXEC_FLG
                    dtRow("MAINTE_DIV") = mainteInfo.MAINTE_DIV
                    dtRow("ST_DT") = mainteInfo.ST_DT
                    dtRow("END_DT") = mainteInfo.END_DT
                    dtRow("LIMIT_DT") = mainteInfo.LIMIT_DT
                    dt.Rows.Add(dtRow)
                Next

                Return dt
            Catch ex As Exception
                logger.Error(ex)
                Throw ex
            End Try
        End Function
#End Region

#Region "CSV取込データをDB登録用データに変換"
        ''' <summary>
        ''' CSV取込データをDB登録用データに変換 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="procDiv">処理区分</param>
        ''' <returns></returns>
        Private Function FuncConvertImportToDbOpeData(ByVal procDiv As String) As List(Of LendingEntryViewModel)
            Try
                'CSV取込済みかチェック
                If _CsvImportItems.Count <= 0 Then
                    Return Nothing
                End If

                Dim lstEntryModel As New List(Of LendingEntryViewModel)
                Dim lendEntryModel As LendingEntryModel = Nothing

                '対象処理区分のデータのみ抽出して変換
                For Each importItem As ReserveCsvImportModel In _CsvImportItems.Where(Function(x) x.PROCESS_DIV = procDiv).ToList
                    Dim viewModel As New LendingEntryViewModel

                    '登録済み予約データ取得
                    If importItem.PROCESS_DIV <> CsvImportProcDiv.Insert Then
                        lendEntryModel = _db.LendingEntry.Find(Long.Parse(importItem.RESERVE_ID))
                        viewModel = CopyLendEntryModeltoViewModel(lendEntryModel)
                    End If

                    viewModel.RESERVE_ID = importItem.RESERVE_ID

                    '削除時は履歴登録用にDBから取得したデータを使用するためCSVのデータはセットしない
                    If importItem.PROCESS_DIV <> CsvImportProcDiv.Delete Then
                        '登録・更新時
                        viewModel.MNG_LOC_L_CD = importItem.ASSET_INFO_LIST(0).MNG_LOC_L_CD
                        viewModel.REQUEST_DT = importItem.REQUEST_DT
                        viewModel.REQ_EMP_CD = importItem.REQ_EMP_CD
                        viewModel.CHARGE_EMP_CD = importItem.REQ_EMP_CD
                        viewModel.RealStartDtString = FuncConvStrDateSlash(importItem.REAL_START_DT)
                        viewModel.RealEndDtString = FuncConvStrDateSlash(importItem.REAL_END_DT)
                        viewModel.UserStartDtString = FuncConvStrDateSlash(importItem.REAL_START_DT)
                        viewModel.UserEndDtString = FuncConvStrDateSlash(importItem.USER_END_DT)
                        viewModel.LEASE_PLACE_CD = importItem.LEASE_PLACE_CD
                        viewModel.LEASE_USE = importItem.LEASE_USE
                        viewModel.DEST_NAME1 = importItem.DEST_NAME1
                        viewModel.DEST_NAME2 = importItem.DEST_NAME2
                        viewModel.TO_DEST_NAME = importItem.TO_DEST_NAME        '2023/03/22 Kawamoto ADD
                        viewModel.DEST_ADDRESS1 = importItem.DEST_ADDRESS1
                        viewModel.DEST_ADDRESS2 = importItem.DEST_ADDRESS2
                        viewModel.DEST_ADDRESS3 = importItem.DEST_ADDRESS3

                        If importItem.DEST_PHONE_NUM = String.Empty Then
                            '空白ならNULL登録
                            viewModel.DEST_PHONE_NUMBER = Nothing
                        Else
                            viewModel.DEST_PHONE_NUMBER = importItem.DEST_PHONE_NUM
                        End If

                        If importItem.DEST_ZIP_CODE = String.Empty Then
                            '空白ならNULL登録
                            viewModel.DEST_ZIP_CODE = Nothing
                        Else
                            viewModel.DEST_ZIP_CODE = importItem.DEST_ZIP_CODE
                        End If

                        '2023/03/22 Kawamoto ADD START
                        viewModel.RET_LEASE_PLACE_CD = importItem.RET_LEASE_PLACE_CD
                        viewModel.RET_ADDRESS1 = importItem.RET_ADDRESS1
                        viewModel.RET_ADDRESS2 = importItem.RET_ADDRESS2
                        viewModel.RET_ADDRESS3 = importItem.RET_ADDRESS3
                        If importItem.RET_ZIP_CODE = String.Empty Then
                            '空白ならNULL登録
                            viewModel.RET_ZIP_CODE = Nothing
                        Else
                            viewModel.RET_ZIP_CODE = importItem.RET_ZIP_CODE
                        End If
                        '2023/03/22 Kawamoto ADD END

                        viewModel.REMARK = importItem.REMARK
                        viewModel.REMARK_INTERNAL = importItem.REMARK_INTERNAL
                        viewModel.DEST_NO = importItem.DEST_NO
                    End If

                    '資産情報
                    viewModel.ReserveDetailData = importItem.RESERVE_DETAIL

                    lstEntryModel.Add(viewModel)
                Next

                '0件の場合、空で返却
                If lstEntryModel.Count <= 0 Then
                    Return Nothing
                End If

                Return lstEntryModel
            Catch ex As Exception
                logger.Error(ex)
                Throw ex
            End Try
        End Function
#End Region

#Region "CSV取込　登録・更新処理"
        ''' <summary>
        ''' CSV取込　登録・更新処理 2022/7/25 sv sato add
        ''' </summary>
        ''' <returns></returns>
        Private Function FuncCsvImportInsAndUpd() As Boolean
            Try
                Dim lstUpdate As List(Of LendingEntryViewModel) = FuncConvertImportToDbOpeData(CsvImportProcDiv.Update)
                Dim lstInsert As List(Of LendingEntryViewModel) = FuncConvertImportToDbOpeData(CsvImportProcDiv.Insert)

                '◆更新
                If Not IsNothing(lstUpdate) Then
                    Dim upd_i As Integer = 1    'ヘッダーを除くため1から開始

                    '1件ずつ更新
                    For Each updModel As LendingEntryViewModel In lstUpdate
                        upd_i += 1
                        If updModel.RESERVE_STATUS = ReserveStatus.HenkyakuSumi Then
                            '返却済みの予約データの更新はスキップする
                            setDspErrContent(upd_i, _Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER), _Message.getMessage(CommonLabel.MSG_ERR_HENKYAKUZUMI_RESERVE_SKIP))
                            Continue For
                        End If

                        '各資産の　資産メンテナンス登録情報 セット　&　チェック
                        Dim sNgAssetNo As String = CheckAssetMainteInfo(updModel.ReserveDetailData)
                        If Not String.IsNullOrEmpty(sNgAssetNo) Then
                            setDspErrContent(upd_i, _Message.getMessage(CommonLabel.LABEL_MNG_LOC), String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_MUST_MAINTE), sNgAssetNo))
                            Return False
                        End If

                        '更新実施
                        If Create(updModel, CsvImportProcDiv.Update) = False Then
                            Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                            setDspErrContent(upd_i, _Message.getMessage(CommonLabel.BUTTON_CSVIMPORT), _Message.getMessage(CommonLabel.MSG_ERR_UPDATE_FAILURE))
                            Return False
                        End If
                    Next
                End If

                '◆登録
                If Not IsNothing(lstInsert) Then
                    Dim ins_i As Integer = 1    'ヘッダーを除くため1から開始

                    '1件ずつ登録
                    For Each insModel As LendingEntryViewModel In lstInsert
                        ins_i += 1

                        '各資産の　資産メンテナンス登録情報 セット　&　チェック
                        Dim sNgAssetNo As String = CheckAssetMainteInfo(insModel.ReserveDetailData)
                        If Not String.IsNullOrEmpty(sNgAssetNo) Then
                            setDspErrContent(ins_i, _Message.getMessage(CommonLabel.LABEL_MNG_LOC), String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_MUST_MAINTE), sNgAssetNo))
                            Return False
                        End If

                        If Create(insModel, CsvImportProcDiv.Insert) = False Then
                            Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                            setDspErrContent(ins_i, _Message.getMessage(CommonLabel.BUTTON_CSVIMPORT), _Message.getMessage(CommonLabel.MSG_ERR_INSERT_FAILURE))
                            Return False
                        End If
                    Next
                End If

                Return True
            Catch ex As Exception
                logger.Error(ex.Message)
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                setDspErrContent(999, _Message.getMessage(CommonLabel.BUTTON_CSVIMPORT), _Message.getMessage(CommonLabel.MSG_ERR_SOUTEIGAI))
                Return False
            End Try
        End Function

        ''' <summary>
        ''' CSV取込　予約削除実施 2022/7/25 sv sato add
        ''' </summary>
        ''' <returns></returns>
        Private Function FuncCsvImportDelete() As Boolean
            Try
                Dim lstDelete As List(Of LendingEntryViewModel) = FuncConvertImportToDbOpeData(CsvImportProcDiv.Delete)

                If Not IsNothing(lstDelete) Then
                    Dim del_i As Integer = 1    'ヘッダーを除くため1から開始

                    '1件ずつ削除
                    For Each delModel As LendingEntryViewModel In lstDelete
                        del_i += 1

                        '削除実施
                        If Delete(delModel) = False Then
                            Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                            setDspErrContent(del_i, _Message.getMessage(CommonLabel.BUTTON_CSVIMPORT), _Message.getMessage(CommonLabel.MSG_ERR_DELETE_FAILURE))
                            Return False
                        End If
                    Next
                End If

                Return True
            Catch ex As Exception
                logger.Error(ex.Message)
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                setDspErrContent(999, _Message.getMessage(CommonLabel.BUTTON_CSVIMPORT), _Message.getMessage(CommonLabel.MSG_ERR_SOUTEIGAI))
                Return False
            End Try
        End Function
#End Region

#Region "設定Mから貸出用途情報を全て取得"
        ''' <summary>
        ''' 設定Mから貸出用途情報を全て取得 2022/7/25 sv sato add
        ''' </summary>
        ''' <returns></returns>
        Private Function FncGetReservMethodInfo() As List(Of SettingInfo)
            Try
                Dim lstReservMethod As List(Of SettingInfo) = New List(Of SettingInfo)

                Dim iCnt As Integer = 1
                While iCnt > 0
                    Dim Youto As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method.KeyDiv, iCnt, CommonSettingKey.Reservation_Method.Key2, CommonSettingKey.Reservation_Method.Key3)
                    If Youto Is Nothing Then
                        iCnt = 0
                    Else
                        lstReservMethod.Add(Youto)
                        iCnt += 1
                    End If
                End While

                Return lstReservMethod
            Catch ex As Exception
                logger.Error(ex.Message)
                Throw ex
            End Try
        End Function
#End Region

#Region "CSV取込　貸出用途をコードから名称に変換"
        ''' <summary>
        ''' CSV取込　貸出用途をコードから名称に変換 2022/7/25 sv sato add
        ''' ※ CSV取込リストに不正な貸出用途コードが含まれていないことが前提
        ''' <param name="prmLstReservMethod">貸出用途情報リスト　設定Mから取得</param>
        ''' </summary>
        Private Sub SubCsvLeaseUseCdToName(ByVal prmLstReservMethod As List(Of SettingInfo))
            Try
                '◆CSV取込済みかチェック
                If _CsvImportItems.Count <= 0 Then
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NoSelect
                    Throw New Exception(_Message.getMessage(CommonLabel.MSG_ERR_ELSE))
                End If

                '◆削除以外の取込データが対象
                For Each item As ReserveCsvImportModel In _CsvImportItems.Where(Function(x) x.PROCESS_DIV <> CsvImportProcDiv.Delete).ToList
                    'コードと対応する名称を取得して上書き
                    item.LEASE_USE = prmLstReservMethod.Find(Function(x) x.Key1 = item.LEASE_USE).DATA1
                Next
            Catch ex As Exception
                logger.Error(ex.Message)
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                Throw ex
            End Try
        End Sub
#End Region

#Region "画面に表示するエラー内容を設定"
        ''' <summary>
        ''' 画面に表示するエラー内容を設定 2022/7/25 sv sato add
        ''' </summary>
        ''' <param name="index">エラー行番号</param>
        ''' <param name="errKoumoku">エラー発生項目</param>
        ''' <param name="errGaiyou">エラー内容</param>
        Private Sub setDspErrContent(ByVal index As Integer, ByVal errKoumoku As String, ByVal errGaiyou As String)
            '新規行がなければ追加する
            If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                Dim lstRow As DataInportResult = New DataInportResult
                Me._lstErr.Add(lstRow)
            End If

            'エラー内容をセット
            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = index
            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = errKoumoku
            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = errGaiyou
        End Sub
#End Region

#Region "DB登録用データをモデルにコピーする"
        ''' <summary>
        ''' DB登録用データをモデルにコピーする 2022/7/25 sv sato add
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As LendingEntryViewModel)
            _LendEntryModel.RESERVE_ID = model.RESERVE_ID
            _LendEntryModel.REQ_EMP_CD = model.REQ_EMP_CD
            _LendEntryModel.CHARGE_EMP_CD = model.CHARGE_EMP_CD
            _LendEntryModel.REQUEST_DT = model.REQUEST_DT
            _LendEntryModel.REAL_START_DT = model.REAL_START_DT
            _LendEntryModel.REAL_END_DT = model.REAL_END_DT
            _LendEntryModel.USER_START_DT = model.USER_START_DT
            _LendEntryModel.USER_END_DT = model.USER_END_DT
            _LendEntryModel.LEASE_PLACE_CD = model.LEASE_PLACE_CD
            _LendEntryModel.LEASE_USE = model.LEASE_USE
            _LendEntryModel.MNG_LOC_L_CD = model.MNG_LOC_L_CD
            _LendEntryModel.DEST_NAME1 = model.DEST_NAME1
            _LendEntryModel.DEST_NAME2 = model.DEST_NAME2
            _LendEntryModel.TO_DEST_NAME = model.TO_DEST_NAME           '2023/03/22 Kawamoto ADD
            _LendEntryModel.DEST_ADDRESS1 = model.DEST_ADDRESS1
            _LendEntryModel.DEST_ADDRESS2 = model.DEST_ADDRESS2
            _LendEntryModel.DEST_ADDRESS3 = model.DEST_ADDRESS3
            _LendEntryModel.DEST_PHONE_NUMBER = model.DEST_PHONE_NUMBER
            _LendEntryModel.DEST_ZIP_CODE = model.DEST_ZIP_CODE
            '2023/03/22 Kawamoto ADD START
            _LendEntryModel.RET_LEASE_PLACE_CD = model.RET_LEASE_PLACE_CD
            _LendEntryModel.RET_ADDRESS1 = model.RET_ADDRESS1
            _LendEntryModel.RET_ADDRESS2 = model.RET_ADDRESS2
            _LendEntryModel.RET_ADDRESS3 = model.RET_ADDRESS3
            _LendEntryModel.RET_ZIP_CODE = model.RET_ZIP_CODE
            '2023/03/22 Kawamoto ADD END
            _LendEntryModel.REMARK = model.REMARK
            _LendEntryModel.REMARK_INTERNAL = model.REMARK_INTERNAL
            _LendEntryModel.DEST_NO = model.DEST_NO
            _LendEntryModel.LEASE_CHG_TIMES = model.LEASE_CHG_TIMES
            If String.IsNullOrEmpty(model.RESERVE_STATUS) Then
                _LendEntryModel.RESERVE_STATUS = ReserveStatus.Mi
            Else
                _LendEntryModel.RESERVE_STATUS = model.RESERVE_STATUS
            End If
            _LendEntryModel.INS_DATETIME = model.INS_DATETIME
            _LendEntryModel.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            _LendEntryModel.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            _LendEntryModel.UPD_DATETIME = model.UPD_DATETIME
            _LendEntryModel.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _LendEntryModel.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _LendEntryModel.UPD_TASK = model.UPD_TASK
        End Sub
#End Region

#Region "予約登録用モデルを予約用ビューモデルにコピーする"
        Private Function CopyLendEntryModeltoViewModel(ByVal model As LendingEntryModel) As LendingEntryViewModel
            Dim viewModel As New LendingEntryViewModel
            viewModel.RESERVE_ID = model.RESERVE_ID
            viewModel.REQ_EMP_CD = model.REQ_EMP_CD
            viewModel.CHARGE_EMP_CD = model.CHARGE_EMP_CD
            viewModel.REQUEST_DT = model.REQUEST_DT
            viewModel.REAL_START_DT = model.REAL_START_DT
            viewModel.REAL_END_DT = model.REAL_END_DT
            viewModel.USER_START_DT = model.USER_START_DT
            viewModel.USER_END_DT = model.USER_END_DT
            viewModel.LEASE_PLACE_CD = model.LEASE_PLACE_CD
            viewModel.LEASE_USE = model.LEASE_USE
            viewModel.MNG_LOC_L_CD = model.MNG_LOC_L_CD
            viewModel.DEST_NAME1 = model.DEST_NAME1
            viewModel.DEST_NAME2 = model.DEST_NAME2
            viewModel.TO_DEST_NAME = model.TO_DEST_NAME           '2023/03/22 Kawamoto ADD
            viewModel.DEST_ADDRESS1 = model.DEST_ADDRESS1
            viewModel.DEST_ADDRESS2 = model.DEST_ADDRESS2
            viewModel.DEST_ADDRESS3 = model.DEST_ADDRESS3
            viewModel.DEST_PHONE_NUMBER = model.DEST_PHONE_NUMBER
            viewModel.DEST_ZIP_CODE = model.DEST_ZIP_CODE
            '2023/03/22 Kawamoto ADD START
            viewModel.RET_LEASE_PLACE_CD = model.RET_LEASE_PLACE_CD
            viewModel.RET_ADDRESS1 = model.RET_ADDRESS1
            viewModel.RET_ADDRESS2 = model.RET_ADDRESS2
            viewModel.RET_ADDRESS3 = model.RET_ADDRESS3
            viewModel.RET_ZIP_CODE = model.RET_ZIP_CODE
            '2023/03/22 Kawamoto ADD END
            viewModel.REMARK = model.REMARK
            viewModel.REMARK_INTERNAL = model.REMARK_INTERNAL
            viewModel.DEST_NO = model.DEST_NO
            viewModel.LEASE_CHG_TIMES = model.LEASE_CHG_TIMES
            viewModel.RESERVE_STATUS = model.RESERVE_STATUS
            viewModel.INS_DATETIME = model.INS_DATETIME
            viewModel.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            viewModel.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            viewModel.UPD_DATETIME = model.UPD_DATETIME
            viewModel.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            viewModel.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            viewModel.UPD_TASK = model.UPD_TASK
            viewModel.RealStartDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(model.REAL_START_DT))   '実貸出開始日表示文字列
            viewModel.RealEndDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(model.REAL_END_DT))    '実貸出終了日表示文字列
            viewModel.UserStartDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(model.USER_START_DT))   'ユーザー貸出開始日表示文字列
            viewModel.UserEndDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(model.USER_END_DT))    'ユーザー貸出終了日表示文字列
            viewModel.ReserveIdString = model.RESERVE_ID
            Return viewModel
        End Function
#End Region

#Region "引継明細作成時、前予約から引継された明細であれば、前予約の引継先明細の予約ステータスを「返却済」に変更する処理"
        ''' <summary>
        ''' 引継明細作成時、前予約から引継された明細であれば、前予約の引継先明細の予約ステータスを「返却済」に変更する処理
        ''' </summary>
        ''' <param name="db">SystemDBContext</param>
        ''' <param name="lReserveId">予約ID</param>
        ''' <param name="sAssetNo">資産番号 viewmodel</param>
        Public Sub UpdBeforeReserve(ByVal db As SystemDBContext, ByVal lReserveId As Long, ByVal sAssetNo As String)
            '予約明細を取得
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            'T_RESERVE_DETAILテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT RESERVE.*")
            sbSQL.AppendLine("FROM T_RESERVE_DETAIL RESERVE")
            sbSQL.AppendLine("WHERE")
            sbSQL.AppendLine("RESERVE.HANDOVER_DIV = '" & HandOverDiv.HikitsugiSaki & "'")
            sbSQL.AppendLine("AND HANDOVER_RESERVE_ID = @ReserveId")
            sbSQL.AppendLine("AND ASSET_NO = @AssetNo")
            params.Add(New SqlParameter("@ReserveId", lReserveId))
            params.Add(New SqlParameter("@AssetNo", sAssetNo))
            'SQLを実行する
            Dim BeforeReserve As List(Of ReserveDetailModel) = db.ReserveDetail.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            If BeforeReserve IsNot Nothing AndAlso BeforeReserve.Count > 0 Then
                BeforeReserve(0).RESERVE_STATUS = ReserveStatus.HenkyakuSumi

                'アタッチする(予約明細)  予約と予約明細の更新タイミングを分ける
                _db.Entry(BeforeReserve(0)).State = Entity.EntityState.Modified
            End If

        End Sub
#End Region

#Region "一般者か又は管理者でもU_HUB権限があり自拠点でない場合のチェックが必要かを取得"
        ''' <summary>
        ''' 一般者か又は管理者でもU_HUB権限があり自拠点でない場合のチェックが必要かを取得 2022/7/25 sv sato add
        ''' </summary>
        Private Function GetIsNeedCheck(LoginInfo As RfidEntity.LoginUser, sMngLocLCd As String) As Boolean

            '一般者の場合
            If _LoginInfo.KengenKbn = AUTH_DIV.Nomal Then
                Return True
            End If

            '管理者の場合
            'ログイン社員の所属拠点データ取得
            Dim tblEmpHub As DataTable = Dao.ToolEmployee.GetHubLocData(LoginInfo.EmployeeCd)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, LoginInfo)
            '特殊権限が存在するかどうかを取得
            Dim isExistAuthOption_Hub As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_HUB)
            If isExistAuthOption_Hub Then
                Dim isJikyoten As Boolean = False    '自拠点か否か
                For Each rowEmpHub As DataRow In tblEmpHub.Rows
                    If sMngLocLCd = rowEmpHub("HUB_LOC_L_CD").ToString Then
                        isJikyoten = True
                        Exit For
                    End If
                Next
                If Not isJikyoten Then
                    Return True
                End If
            End If

            Return False

        End Function
#End Region

#Region "資産履歴を追加します"
        ''' <summary>
        ''' 資産履歴を追加します
        ''' </summary>
        ''' <param name="model">モデル</param>
        ''' <param name="sProcessDiv">処理区分</param>
        Private Sub InsertAssetHistory(model As AssetListModel, sProcessDiv As String)
            Dim HistoryModel As AssetHistoryModel = New AssetHistoryModel                    '資産履歴登録用モデル

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim sLocLName As String = String.Empty
            Dim sLocMName As String = String.Empty
            Dim sLocSName As String = String.Empty
            'HistoryModel.PROCESS_LOG_ID = ""  '自動採番
            HistoryModel.PROCESS_DT = CInt(DateTime.Now.ToString("yyyyMMdd"))
            HistoryModel.PROCESS_TM = DateTime.Now.ToString("HHmmssfff")
            HistoryModel.ASSET_NO = model.ASSET_NO
            HistoryModel.PROCESS_TOOL_DIV = ProcessToolDiv.Web
            HistoryModel.ASSET_ID = model.ASSET_ID
            HistoryModel.PROCESS_DIV = sProcessDiv
            HistoryModel.LEASE_PLACE_CD = model.LEASE_PLACE_CD
            HistoryModel.LEASE_PLACE_NAME = model.LEASE_PLACE_NAME
            HistoryModel.REMARK = model.REMARK
            HistoryModel.LEASE_EMPLOYEE_CD = model.LEASE_EMPLOYEE_CD
            HistoryModel.LEASE_EMPLOYEE_NAME = model.LEASE_EMPLOYEE_NAME
            HistoryModel.LEASE_DT = model.LEASE_DT
            HistoryModel.LEASE_DUE_DT = model.LEASE_DUE_DT
            HistoryModel.LEASE_STATUS = model.LEASE_STATUS
            HistoryModel.MNG_LOC_L_CD = model.MNG_LOC_L_CD
            HistoryModel.LOC_L_CD = model.LOC_L_CD
            HistoryModel.LOC_M_CD = model.LOC_M_CD
            HistoryModel.LOC_S_CD = model.LOC_S_CD
            If Not String.IsNullOrEmpty(model.LOC_L_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationL, model.LOC_L_CD, " ", " "}
                Dim locamodel As LocationModel = _db.Location.Find(findKeys)
                sLocLName = locamodel.LOC_NAME
            End If
            If Not String.IsNullOrEmpty(model.LOC_M_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationM, model.LOC_L_CD, model.LOC_M_CD, " "}
                Dim locamodel As LocationModel = _db.Location.Find(findKeys)
                sLocMName = locamodel.LOC_NAME
            End If
            If Not String.IsNullOrEmpty(model.LOC_S_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationS, model.LOC_L_CD, model.LOC_M_CD, model.LOC_S_CD}
                Dim locamodel As LocationModel = _db.Location.Find(findKeys)
                sLocSName = locamodel.LOC_NAME
            End If
            HistoryModel.LOC_L_NAME = sLocLName
            HistoryModel.LOC_M_NAME = sLocMName
            HistoryModel.LOC_S_NAME = sLocSName
            HistoryModel.ASSET_NAME = model.ASSET_NAME
            HistoryModel.MAKER = model.MAKER
            HistoryModel.PRODUCT_NO = model.PRODUCT_NO
            HistoryModel.MODEL_NO = model.MODEL_NO
            HistoryModel.TASK_RESULT = TaskResult.OK

            '履歴の更新日時・更新社員コード・更新社員名・更新プロセスを登録
            HistoryModel.UPD_DATETIME = CommonRfid.getNowDate
            HistoryModel.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            HistoryModel.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_STATUS)

            '資産履歴テーブルの更新
            _db.AssetHistory.Add(HistoryModel)
        End Sub
#End Region

#Region "予約履歴を追加します"
        ''' <summary>
        ''' 予約履歴を追加します
        ''' </summary>
        Public Sub InsertReserveLog(model As LendingEntryModel, sProcessDiv As String, ByVal reserveDetail As List(Of ReserveDetailViewModel))
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = reserveDetail
            Dim iCount As Integer = 1   '予約連番用
            For Each row As ReserveDetailViewModel In lstGrid
                Dim HistoryModel As LendingHistoryModel = New LendingHistoryModel                    '資産履歴登録用モデル
                HistoryModel.RESERVE_LOG_ID = 0 '自動採番
                HistoryModel.PROCESS_DT = CInt(DateTime.Now.ToString("yyyyMMdd"))
                HistoryModel.PROCESS_TM = DateTime.Now.ToString("HHmmssfff")
                HistoryModel.PROCESS_TOOL_DIV = ProcessToolDiv.Web
                HistoryModel.PROCESS_DIV = sProcessDiv
                HistoryModel.RESERVE_ID = model.RESERVE_ID
                HistoryModel.RESERVE_SUB_NO = iCount
                HistoryModel.REQ_EMP_CD = model.REQ_EMP_CD
                HistoryModel.CHARGE_EMP_CD = model.CHARGE_EMP_CD
                HistoryModel.REQUEST_DT = model.REQUEST_DT
                HistoryModel.REAL_START_DT = model.REAL_START_DT
                HistoryModel.REAL_END_DT = model.REAL_END_DT
                HistoryModel.USER_START_DT = model.USER_START_DT
                HistoryModel.USER_END_DT = model.USER_END_DT
                HistoryModel.LEASE_PLACE_CD = model.LEASE_PLACE_CD
                HistoryModel.LEASE_USE = model.LEASE_USE
                HistoryModel.MNG_LOC_L_CD = model.MNG_LOC_L_CD
                HistoryModel.DEST_NAME1 = model.DEST_NAME1
                HistoryModel.DEST_NAME2 = model.DEST_NAME2
                HistoryModel.DEST_ADDRESS1 = model.DEST_ADDRESS1
                HistoryModel.DEST_ADDRESS2 = model.DEST_ADDRESS2
                HistoryModel.DEST_ADDRESS3 = model.DEST_ADDRESS3
                HistoryModel.DEST_PHONE_NUMBER = model.DEST_PHONE_NUMBER
                HistoryModel.DEST_ZIP_CODE = model.DEST_ZIP_CODE
                HistoryModel.REMARK = model.REMARK
                HistoryModel.REMARK_INTERNAL = model.REMARK_INTERNAL
                HistoryModel.DEST_NO = model.DEST_NO
                HistoryModel.RESERVE_STATUS = model.RESERVE_STATUS

                HistoryModel.ASSET_NO = row.ASSET_NO
                HistoryModel.DTL_REAL_START_DT = row.REAL_START_DT
                HistoryModel.DTL_REAL_END_DT = row.REAL_END_DT
                If row.HANDOVER_DIV = HandOverDiv.HikitsugiSaki Then
                    HistoryModel.DTL_CHARGE_EMP_CD = row.CHARGE_EMP_CD
                Else
                    HistoryModel.DTL_CHARGE_EMP_CD = model.CHARGE_EMP_CD
                End If
                HistoryModel.DTL_LENDING_DT = row.LENDING_DT
                HistoryModel.DTL_RETURN_DT = row.RETURN_DT
                HistoryModel.DTL_HANDOVER_DIV = row.HANDOVER_DIV
                HistoryModel.DTL_RESERVE_STATUS = row.RESERVE_STATUS

                '履歴の登録日時・登録社員コード・登録社員名・更新日時・更新社員コード・更新社員名・更新プロセスを登録
                HistoryModel.INS_DATETIME = CommonRfid.getNowDate
                HistoryModel.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                HistoryModel.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                HistoryModel.UPD_DATETIME = CommonRfid.getNowDate
                HistoryModel.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                HistoryModel.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_STATUS)

                '予約履歴テーブルの更新
                _db.LendingHistory.Add(HistoryModel)

                '予約連番をカウントアップ
                iCount += 1
            Next

        End Sub
#End Region

#Region "管理ロケーションリストデータ取得"
        ''' <summary>
        ''' 管理ロケーションリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetMngLocData() As List(Of LocationDropdownModel)
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)

            Dim tbl As DataTable = Dao.ToolLocation.GetMngLocationLData(_LoginInfo.EmployeeCd, authInfo, False, Misc.UpdAuthType.UPD_AUTH_HUB)
            Dim lst As New List(Of LocationDropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            Return lst
        End Function
#End Region

#Region "管理ロケーションの選択キーリストの初期値を取得します"
        ''' <summary>
        ''' 管理ロケーションの選択キーリストの初期値を取得します
        ''' </summary>
        Private Function GetDefaultSelectMngLocKeys() As String
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)

            Dim tbl As DataTable = Dao.ToolLocation.GetMngLocationLData(_LoginInfo.EmployeeCd, authInfo, False, Misc.UpdAuthType.UPD_AUTH_HUB)
            Dim lst As New List(Of String)
            For Each row As DataRow In tbl.Rows
                If row("SHARED_FLG").ToString <> SharedFlg.Other Then
                    lst.Add(row("LOC_CD").ToString)
                End If
            Next

            'カンマ区切りのデータを設定
            Return String.Join(",", lst.ToArray())
        End Function
#End Region

#Region "分類区分ドロップダウン変更処理"
        ''' <summary>
        ''' 分類区分ドロップダウン変更処理
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

#Region "大分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 大分類ドロップダウンリストデータ取得
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

            ViewBag.modelCatL = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatL)
            _ViewModel.catLDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "中分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 中分類ドロップダウンリストデータ取得
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

            ViewBag.modelCatM = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatM)
            _ViewModel.catMDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "小分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 小分類ドロップダウンリストデータ取得
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

            ViewBag.modelCatS = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatS)
            _ViewModel.catSDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "予約申請登録/更新画面に遷移します(新規登録)"
        ''' <summary>
        ''' 予約申請登録/更新画面に遷移します(新規登録)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingEntry_Create(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '予約申請登録/変更画面への引渡し情報クラス作成
            '登録の場合、「newInfo.DispKbn」に「DispKbn.Create」を入れる。
            Dim newInfo As New LendingEntryInfo
            newInfo.SenimotoKbn = SenimotoKbn.LendingStatus
            newInfo.DispKbn = DispKbn.Create
            newInfo.ReserveId = 0   '当メソッドは新規登録時のみ呼び出されるため0固定
            'セッションに保存
            Session(CommonSessionKey.LENDING_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingEntry")
        End Function
#End Region

#Region "予約申請登録/更新画面に遷移します(変更)"
        ''' <summary>
        ''' 予約申請登録/更新画面に遷移します(変更)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingEntry_Edit(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '予約ID
            Dim lReserveId As Long = Long.Parse(_ViewModel.ReserveIdKey)

            '予約申請登録/変更画面への引渡し情報クラス作成
            '変更の場合、「newInfo.DispKbn」に「DispKbn.Edit」を入れ、「ReserveId」をセットする。
            Dim newInfo As New LendingEntryInfo
            newInfo.SenimotoKbn = SenimotoKbn.LendingStatus
            newInfo.DispKbn = DispKbn.Edit
            newInfo.ReserveId = lReserveId
            'セッションに保存
            Session(CommonSessionKey.LENDING_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingEntry")
        End Function
#End Region

#Region "予約申請登録/更新画面に遷移します(照会)"
        ''' <summary>
        ''' 予約申請登録/更新画面に遷移します(照会)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingEntry_Disp(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '予約ID
            Dim lReserveId As Long = Long.Parse(_ViewModel.ReserveIdKey)

            '予約申請登録/変更画面への引渡し情報クラス作成
            '照会の場合、「newInfo.DispKbn」に「DispKbn.Disp」を入れ、「ReserveId」をセットする。
            Dim newInfo As New LendingEntryInfo
            newInfo.SenimotoKbn = SenimotoKbn.LendingStatus
            newInfo.DispKbn = DispKbn.Disp
            newInfo.ReserveId = lReserveId
            'セッションに保存
            Session(CommonSessionKey.LENDING_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingEntry")
        End Function
#End Region

#Region "メンテナンス登録画面に遷移します(新規登録)"
        ''' <summary>
        ''' メンテナンス登録画面に遷移します(新規登録)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingMainteEntry_Create(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '資産番号
            Dim sAssetNo As String = _ViewModel.SelectKeys.Split(",")(0)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '登録の場合、「newInfo.DispKbn」に「DispKbn.Create」を入れ、「AssetNo」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Create
            newInfo.AssetNo = sAssetNo
            newInfo.MainteId = 0    '当メソッドは新規登録時のみ呼び出されるため0固定
            newInfo.MainteKbn = MainteKbn.Mainte
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "メンテナンス登録画面に遷移します(変更)"
        ''' <summary>
        ''' メンテナンス登録画面に遷移します(変更)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingMainteEntry_Edit(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'メンテナンスID
            Dim lMainteId As Long = Long.Parse(_ViewModel.MainteIdKey)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '登録の場合、「newInfo.DispKbn」に「DispKbn.Create」を入れ、「AssetNo」をセットする。
            '変更の場合、「newInfo.DispKbn」に「DispKbn.Edit」を入れ、「MainteId」をセットする。
            '照会の場合、「newInfo.DispKbn」に「DispKbn.Disp」を入れ、「MainteId」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Edit
            newInfo.AssetNo = String.Empty
            newInfo.MainteId = lMainteId
            newInfo.MainteKbn = MainteKbn.Mainte
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "メンテナンス登録画面に遷移します(照会)"
        ''' <summary>
        ''' メンテナンス登録画面に遷移します(照会)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingMainteEntry_Disp(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'メンテナンスID
            Dim lMainteId As Long = Long.Parse(_ViewModel.MainteIdKey)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '照会の場合、「newInfo.DispKbn」に「DispKbn.Disp」を入れ、「MainteId」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Disp
            newInfo.AssetNo = String.Empty
            newInfo.MainteId = lMainteId
            newInfo.MainteKbn = MainteKbn.Mainte
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "〇〇登録画面に遷移します(新規登録)"
        ''' <summary>
        ''' 〇〇登録画面に遷移します(新規登録)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingBlaRegist_Create(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '資産番号
            Dim sAssetNo As String = _ViewModel.SelectKeys.Split(",")(0)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '登録の場合、「newInfo.DispKbn」に「DispKbn.Create」を入れ、「AssetNo」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Create
            newInfo.AssetNo = sAssetNo
            newInfo.MainteId = 0    '当メソッドは新規登録時のみ呼び出されるため0固定
            newInfo.MainteKbn = MainteKbn.BlaRegist
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "〇〇登録画面に遷移します(変更)"
        ''' <summary>
        ''' 〇〇登録画面に遷移します(変更)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingBlaRegist_Edit(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'メンテナンスID
            Dim lMainteId As Long = Long.Parse(_ViewModel.MainteIdKey)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '登録の場合、「newInfo.DispKbn」に「DispKbn.Create」を入れ、「AssetNo」をセットする。
            '変更の場合、「newInfo.DispKbn」に「DispKbn.Edit」を入れ、「MainteId」をセットする。
            '照会の場合、「newInfo.DispKbn」に「DispKbn.Disp」を入れ、「MainteId」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Edit
            newInfo.AssetNo = String.Empty
            newInfo.MainteId = lMainteId
            newInfo.MainteKbn = MainteKbn.BlaRegist
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "〇〇登録画面に遷移します(照会)"
        ''' <summary>
        ''' 〇〇登録画面に遷移します(照会)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingBlaRegist_Disp(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'メンテナンスID
            Dim lMainteId As Long = Long.Parse(_ViewModel.MainteIdKey)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '照会の場合、「newInfo.DispKbn」に「DispKbn.Disp」を入れ、「MainteId」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Disp
            newInfo.AssetNo = String.Empty
            newInfo.MainteId = lMainteId
            newInfo.MainteKbn = MainteKbn.BlaRegist
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "予備登録１画面に遷移します(新規登録)"
        ''' <summary>
        ''' 予備登録１画面に遷移します(新規登録)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingPreRegist1_Create(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '資産番号
            Dim sAssetNo As String = _ViewModel.SelectKeys.Split(",")(0)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '登録の場合、「newInfo.DispKbn」に「DispKbn.Create」を入れ、「AssetNo」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Create
            newInfo.AssetNo = sAssetNo
            newInfo.MainteId = 0    '当メソッドは新規登録時のみ呼び出されるため0固定
            newInfo.MainteKbn = MainteKbn.PreRegist1
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "予備登録１画面に遷移します(変更)"
        ''' <summary>
        ''' 予備登録１画面に遷移します(変更)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingPreRegist1_Edit(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'メンテナンスID
            Dim lMainteId As Long = Long.Parse(_ViewModel.MainteIdKey)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '登録の場合、「newInfo.DispKbn」に「DispKbn.Create」を入れ、「AssetNo」をセットする。
            '変更の場合、「newInfo.DispKbn」に「DispKbn.Edit」を入れ、「MainteId」をセットする。
            '照会の場合、「newInfo.DispKbn」に「DispKbn.Disp」を入れ、「MainteId」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Edit
            newInfo.AssetNo = String.Empty
            newInfo.MainteId = lMainteId
            newInfo.MainteKbn = MainteKbn.PreRegist1
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "予備登録１画面に遷移します(照会)"
        ''' <summary>
        ''' 予備登録１画面に遷移します(照会)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingPreRegist1_Disp(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'メンテナンスID
            Dim lMainteId As Long = Long.Parse(_ViewModel.MainteIdKey)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '照会の場合、「newInfo.DispKbn」に「DispKbn.Disp」を入れ、「MainteId」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Disp
            newInfo.AssetNo = String.Empty
            newInfo.MainteId = lMainteId
            newInfo.MainteKbn = MainteKbn.PreRegist1
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "予備登録２画面に遷移します(新規登録)"
        ''' <summary>
        ''' メンテナンス登録画面に遷移します(新規登録)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingPreRegist2_Create(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '資産番号
            Dim sAssetNo As String = _ViewModel.SelectKeys.Split(",")(0)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '登録の場合、「newInfo.DispKbn」に「DispKbn.Create」を入れ、「AssetNo」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Create
            newInfo.AssetNo = sAssetNo
            newInfo.MainteId = 0    '当メソッドは新規登録時のみ呼び出されるため0固定
            newInfo.MainteKbn = MainteKbn.PreRegist2
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "予備登録２画面に遷移します(変更)"
        ''' <summary>
        ''' 予備登録１画面に遷移します(変更)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingPreRegist2_Edit(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'メンテナンスID
            Dim lMainteId As Long = Long.Parse(_ViewModel.MainteIdKey)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '登録の場合、「newInfo.DispKbn」に「DispKbn.Create」を入れ、「AssetNo」をセットする。
            '変更の場合、「newInfo.DispKbn」に「DispKbn.Edit」を入れ、「MainteId」をセットする。
            '照会の場合、「newInfo.DispKbn」に「DispKbn.Disp」を入れ、「MainteId」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Edit
            newInfo.AssetNo = String.Empty
            newInfo.MainteId = lMainteId
            newInfo.MainteKbn = MainteKbn.PreRegist2
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "予備登録２画面に遷移します(照会)"
        ''' <summary>
        ''' 予備登録２画面に遷移します(照会)
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingPreRegist2_Disp(ByVal ViewModel As LendingStatusViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'メンテナンスID
            Dim lMainteId As Long = Long.Parse(_ViewModel.MainteIdKey)

            'メンテナンス登録/変更画面への引渡し情報クラス作成
            '照会の場合、「newInfo.DispKbn」に「DispKbn.Disp」を入れ、「MainteId」をセットする。
            Dim newInfo As New LendingMainteEntryInfo
            newInfo.DispKbn = DispKbn.Disp
            newInfo.AssetNo = String.Empty
            newInfo.MainteId = lMainteId
            newInfo.MainteKbn = MainteKbn.PreRegist2
            'セッションに保存
            Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_STATUS_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingMainteEntry")
        End Function
#End Region

#Region "全選択ボタン押下時処理"
        ''' <summary>
        ''' 全選択ボタン押下時処理
        ''' </summary>
        ''' <param name="blnSelectAll"></param>
        ''' <returns></returns>
        Public Function SelectAll(ByVal blnSelectAll As Boolean) As ActionResult
            If Not blnSelectAll Then
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

#Region "カゴに追加処理"
        ''' <summary>
        ''' カゴに追加処理
        ''' </summary>
        ''' <returns></returns>
        Public Function AddCartInfo(ByVal SelectKeys As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'セッションのカゴ情報を取得する
            Dim lstCartInfo As New List(Of CartInfo)
            If Not IsNothing(Session(CommonSessionKey.CREAGE_CART_INFO)) Then
                lstCartInfo = Session(CommonSessionKey.CREAGE_CART_INFO)
            End If

            'カゴ情報から資産番号、管理ロケーションを取得
            Dim lstAssetNo As New List(Of String)
            Dim lstMngLocLCd As New List(Of String)
            For Each info As CartInfo In lstCartInfo
                lstAssetNo.Add(info.AssetNo)
                If Not lstMngLocLCd.Contains(info.MngLocLCd) Then
                    lstMngLocLCd.Add(info.MngLocLCd)
                End If
            Next

            Dim lstNewAssetNo As New List(Of String)
            For Each key As String In SelectKeys.Split("/")
                '--資産番号
                Dim sAssetNo As String = key.Split(",")(0)
                If lstAssetNo.Contains(sAssetNo) Then
                    'カゴに追加済の資産が選択された場合はエラー
                    Return Json("error-asset-no-already-added")
                Else
                    lstNewAssetNo.Add(sAssetNo)
                End If
                '--管理ロケーション
                Dim sMngLocLCd As String = key.Split(",")(1)
                If lstMngLocLCd.Count = 0 Then
                    lstMngLocLCd.Add(sMngLocLCd)
                Else
                    '異なるロケーションが存在する場合はエラー
                    If Not lstMngLocLCd.Contains(sMngLocLCd) Then
                        Return Json("error-mngloc")
                    End If
                End If
            Next

            'カゴに追加できる最大件数を超える場合はエラー
            Dim lendingNumberItems As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Number_Items.KeyDiv, CommonSettingKey.Lending_Number_Items.Key1, CommonSettingKey.Lending_Number_Items.Key2, CommonSettingKey.Lending_Number_Items.Key3, True)
            Dim iLendingNumber As Integer = Integer.Parse(lendingNumberItems.DATA1)
            If iLendingNumber < (lstAssetNo.Count + lstNewAssetNo.Count) Then
                Return Json("error-cart-over-number")
            End If

            If lstNewAssetNo.Count <> 0 Then
                '資産マスタデータ取得
                Dim tbl As DataTable = Dao.ToolAsset.GetMstAssetByList(lstNewAssetNo)
                'カゴ情報追加
                For Each row As DataRow In tbl.Rows
                    Dim newInfo As New CartInfo
                    newInfo.MngLocLCd = row("MNG_LOC_L_CD").ToString
                    newInfo.MngLocLName = row("MNG_LOC_L_NAME").ToString
                    newInfo.AssetNo = row("ASSET_NO").ToString
                    newInfo.AssetName = row("ASSET_NAME").ToString
                    lstCartInfo.Add(newInfo)
                Next
            End If

            'セッションに保存
            Session(CommonSessionKey.CREAGE_CART_INFO) = lstCartInfo

            '選択数
            Return Json(lstCartInfo.Count.ToString)
        End Function
#End Region

#Region "カゴクリア処理"
        ''' <summary>
        ''' カゴクリア処理
        ''' </summary>
        ''' <returns></returns>
        Public Function ClearCartInfo() As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'セッションクリア
            Session.Remove(CommonSessionKey.CREAGE_CART_INFO)

            'セッションのカゴ情報を取得する
            Dim lstCartInfo As New List(Of CartInfo)
            If Not IsNothing(Session(CommonSessionKey.CREAGE_CART_INFO)) Then
                lstCartInfo = Session(CommonSessionKey.CREAGE_CART_INFO)
            End If

            '選択数
            Return Json(lstCartInfo.Count.ToString)
        End Function
#End Region

#Region "カゴ情報表示処理"
        ''' <summary>
        ''' カゴ情報表示処理
        ''' </summary>
        ''' <returns></returns>
        Public Function ShowCartInfo() As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'セッションのカゴ情報を取得する
            Dim lstCartInfo As New List(Of CartInfo)
            If Not IsNothing(Session(CommonSessionKey.CREAGE_CART_INFO)) Then
                lstCartInfo = Session(CommonSessionKey.CREAGE_CART_INFO)
            End If

            Return Json(lstCartInfo)
        End Function
#End Region

#Region "予約情報表示処理"
        ''' <summary>
        ''' 予約情報表示処理
        ''' </summary>
        ''' <remarks>
        '''   2021/11/30 upd nv Inoue
        ''' </remarks>
        ''' <returns></returns>
        Public Function ShowReserveInfo(SelectKeys As String) As ActionResult
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)

            Dim db As New SystemDBContext

            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)
            '特殊権限が存在するかどうかを取得
            Dim isExistAuthOption_Self As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_SELF)
            Dim isExistAuthOption_Hub As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_HUB)
            Dim isExistAuthOption_EmpHub As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_EMPHUB)
            Dim isExistAuthOption_Admin As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_ADMIN)
            Dim isExistAuthOption_Mainte As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_MAINTE)
            Dim isExistAuthOption_MainteHub As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_MAINTEHUB)
            'ログイン社員の所属拠点データ取得
            Dim tblEmpHub As DataTable = Dao.ToolEmployee.GetHubLocData(_LoginInfo.EmployeeCd)

            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Dim key As String() = SelectKeys.Split(",")
            Dim sAssetNo As String = key(0)
            Dim iTargetDate As Integer = Integer.Parse(key(1))

            '予約機能の表示切替設定を取得  2022/07/25 add sv sato
            Dim reserveRegOption As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reserve_Regist_Option.KeyDiv, CommonSettingKey.Reserve_Regist_Option.Key1, CommonSettingKey.Reserve_Regist_Option.Key2, CommonSettingKey.Reserve_Regist_Option.Key3, True)

            '照合機能設定を取得
            Dim MatchingFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.MatchingFunction.KeyDiv, CommonSettingKey.MatchingFunction.Key1, CommonSettingKey.MatchingFunction.Key2, CommonSettingKey.MatchingFunction.Key3, True)


            'データ取得
            Dim tbl As DataTable = Dao.ToolReserve.GetAssetCalDateReserveInfo(sAssetNo, iTargetDate)
            Dim lst As New List(Of LendingStatusReserveInfoModel)           'リスト構造にする必要がないが、既にindex側もその構造で読むようになっているため、このままの構造で。

            'For Each row As DataRow In tbl.Rows
            '    '変換処理
            '    Dim result As Long
            '    '--予約ID
            '    Dim reserveId As Long? = Nothing
            '    If Long.TryParse(row("RESERVE_ID").ToString, result) Then
            '        reserveId = result
            '    End If
            '    '--メンテナンスID
            '    Dim mainteId As Long? = Nothing
            '    If Long.TryParse(row("MAINTE_ID").ToString, result) Then
            '        mainteId = result
            '    End If

            '    lst.Add(
            '        New LendingStatusReserveInfoModel With {
            '            .ASSET_NO = row("ASSET_NO").ToString,
            '            .RESERVE_ID = reserveId,
            '            .MAINTE_ID = mainteId,
            '            .RESERVE_STATUS = row("RESERVE_STATUS").ToString,
            '            .RSVFLG = row("RSVFLG").ToString,
            '            .CHARGE_EMP_NAME = row("CHARGE_EMP_NM").ToString,
            '            .REQ_EMP_NAME = row("REQ_EMP_NM").ToString
            '        }
            '    )
            'Next

            If tbl.Rows.Count > 0 Then
                Dim row As DataRow = tbl.Rows(0)

                Dim IsUpd As Boolean = False        '更新可か否か

                '変換処理
                Dim result As Long

                Dim reserveId As Long? = Nothing        '--予約ID
                Dim mainteId As Long? = Nothing         '--メンテナンスID

                If Long.TryParse(row("RESERVE_ID").ToString, result) Then
                    '予約時
                    reserveId = result

                    'U_ADMINが存在する場合は表示可能。
                    If isExistAuthOption_Admin Then
                        IsUpd = True
                    End If

                    '自分の所属拠点の予約だけが編集対象な権限の場合、管理ロケーションと所属拠点が合致すれば表示可能。
                    If isExistAuthOption_Hub Then
                        For Each rowEmpHub As DataRow In tblEmpHub.Rows
                            If row("MNG_LOC_L_CD").ToString = rowEmpHub("HUB_LOC_L_CD").ToString Then
                                IsUpd = True
                                Exit For
                            End If
                        Next
                    End If

                    'EmpHub がついている場合の条件
                    If isExistAuthOption_EmpHub Then
                        If Dao.ToolEmployee.IsChkAuthEmpHub(_LoginInfo.EmployeeCd, row("REQ_EMP_CD").ToString, row("CHARGE_EMP_CD").ToString) Then
                            IsUpd = True
                        End If
                    End If

                    '自分が登録した予約だけが編集対象の権限の場合、ログイン社員と申請者or担当者が合致すれば表示可能。
                    If isExistAuthOption_Self Then
                        If row("REQ_EMP_CD").ToString = _LoginInfo.EmployeeCd Then
                            IsUpd = True
                        End If
                        If row("CHARGE_EMP_CD").ToString = _LoginInfo.EmployeeCd Then
                            IsUpd = True
                        End If
                    End If

                Else
                    If Long.TryParse(row("MAINTE_ID").ToString, result) Then
                        'メンテ時
                        mainteId = result

                        'U_MAINTE_HUB が存在する場合は拠点一致で表示可能。
                        If isExistAuthOption_MainteHub Then
                            For Each rowEmpHub As DataRow In tblEmpHub.Rows
                                If row("MNG_LOC_L_CD").ToString = rowEmpHub("HUB_LOC_L_CD").ToString Then
                                    IsUpd = True
                                End If
                            Next
                        End If
                        'U_MAINTE が存在する場合は表示可能。
                        If isExistAuthOption_Mainte Then
                            IsUpd = True
                        End If
                    End If
                End If

                '2022/7/25 sv sato add start
                '表示切替設定が無効の場合は更新権限ありでも更新できないようにする
                If reserveRegOption.DATA2 <> ValidFlg.Cd.Valid Then
                    IsUpd = False
                End If
                '2022/7/25 sv sato add end

                '2022/09/22 sv tomatsuri add start
                Dim reserveData As LendingEntryModel = Nothing
                Dim strReserveId As String = ""
                If reserveId IsNot Nothing Then
                    strReserveId = reserveId.ToString
                    '予約情報取得
                    reserveData = db.LendingEntry.Find(reserveId)
                End If
                Dim strMainteId As String = ""
                If mainteId IsNot Nothing Then
                    strMainteId = mainteId.ToString
                End If
                '2022/09/22 sv tomatsuri add end

                '照合機能用パラメータ追加
                Dim constNo = ""
                Dim constDt = ""
                Dim constContents = ""
                Dim constEmployeeName = ""
                If Not IsNothing(MatchingFunc) AndAlso MatchingFunc.DATA1 = ValidFlg.Cd.Valid Then
                    '更新ボタン利用不可
                    IsUpd = False

                    If MatchingFunc.DATA2 = ValidFlg.Cd.Invalid Then
                        '簡易表示の場合は予約情報からそのままセットする
                        If Not IsNothing(reserveData) Then
                            constNo = reserveData.REMARK
                        End If
                    Else
                        '工事情報取得
                        If Not IsNothing(reserveData) Then
                            Dim constData = db.ConstructionList.Find(reserveData.REMARK)
                            If Not IsNothing(constData) Then
                                constNo = constData.CONST_NO

                                If constData.CONST_START_DT.ToString.Length <> itemlength.ConstStartDt Then
                                    constDt += String.Empty
                                Else
                                    constDt += CommonRfid.ToStringIntYMD(constData.CONST_START_DT)
                                End If
                                constDt += "～"
                                If constData.CONST_END_DT.ToString.Length <> itemlength.ConstEndDt Then
                                    constDt += String.Empty
                                Else
                                    constDt += CommonRfid.ToStringIntYMD(constData.CONST_END_DT)
                                End If

                                constContents = constData.CONST_CONTENTS
                                constEmployeeName = constData.CONST_EMPLOYEE_NAME
                            End If
                        End If
                    End If
                End If

                lst.Add(
                    New LendingStatusReserveInfoModel With {
                        .ASSET_NO = row("ASSET_NO").ToString,
                        .RESERVE_ID = strReserveId,                         '2022/09/22 sv tomatsuri 変数変更
                        .MAINTE_ID = strMainteId,                           '2022/09/22 sv tomatsuri 変数変更
                        .RESERVE_STATUS = row("RESERVE_STATUS").ToString,
                        .RSVFLG = row("RSVFLG").ToString,
                        .CHARGE_EMP_NAME = row("CHARGE_EMP_NM").ToString,
                        .REQ_EMP_NAME = row("REQ_EMP_NM").ToString,
                        .IS_UPD = IsUpd,
                        .CONST_NO = constNo,
                        .CONST_DT = constDt,
                        .CONST_CONTENTS = constContents,
                        .CONST_EMPLOYEE_NAME = constEmployeeName
                    }
                )

            End If

            Return Json(lst)
        End Function
#End Region

#Region "isClosedColumnListの設定"
        ''' <summary>
        ''' isClosedColumnListの設定
        ''' </summary>
        Private Sub setIsClosedColumnList()
            Const DataColumnCount = 10          '2022/08/31 Upd Kawamoto
            If _ViewModel.isClosedColumns.Count < DataColumnCount Then
                For i = 0 To DataColumnCount
                    _ViewModel.isClosedColumns.Add(
                           New closedColumn With {
                                .isClosed = False,
                                .columnName = "kotei"
                           }
                           )
                Next
            End If

            '開始日がない場合（初期状態）は処理終了
            If Not IsDate(_ViewModel.SearchDspStartDate) Then
                Return
            End If

            '表示月数の判定
            Dim dtDspStartDate = Date.Parse(_ViewModel.SearchDspStartDate)
            Dim dtDspEndDate As Date = dtDspStartDate.AddMonths(_ViewModel.DspMonthCount).AddDays(-1)
            Dim intDiffMonth As Integer = DateDiff(DateInterval.Month, dtDspStartDate, dtDspEndDate)
            '予約状況照会画面の設定取得
            Dim lendingStatusSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Status_Setting.KeyDiv, CommonSettingKey.Lending_Status_Setting.Key1, CommonSettingKey.Lending_Status_Setting.Key2, CommonSettingKey.Lending_Status_Setting.Key3, True)
            Dim iLeadTime As Decimal = Integer.Parse(lendingStatusSetting.DATA1)

            '表示月の存在を確認し、なければ追加する。
            For i = 0 To intDiffMonth
                Dim targetMonth = dtDspStartDate.AddMonths(i)
                If _ViewModel.isClosedColumns.Any(Function(x) x.columnName = targetMonth.ToString("yyyyMM")) Then
                Else
                    '追加時のカレンダーの開閉状態の初期値は、初月以前は閉じる状態にする。
                    'Dim isFirstMonth = (Now.AddMonths(iLeadTime).ToString("yyyyMM").Equals(targetMonth.ToString("yyyyMM"))) '初月か
                    Dim isBeforeFirstMonth = (targetMonth.ToString("yyyyMM") <= Now.AddMonths(iLeadTime).ToString("yyyyMM")) '初月より前

                    _ViewModel.isClosedColumns.Add(
                           New closedColumn With {
                                .isClosed = isBeforeFirstMonth,
                                .columnName = targetMonth.ToString("yyyyMM")
                           }
                           )
                End If
            Next
        End Sub
#End Region

#Region "グリッド取得"

        Public Function GetGridData() As JsonResult

            'Dim ViewModel As LendingStatusViewModel = Session("ViewModel")
            'If Not IsNothing(ViewModel) Then
            '    _ViewModel = ViewModel
            'End If


            ''ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            '_LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            'If IsNothing(_LoginInfo) Then
            '    '結果表示
            '    Throw New Exception("セッション切れ")
            'End If

            'If _ViewModel.SearchDspStartDate = Nothing Then
            '    Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '    Dim lendingStatusSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Status_Setting.KeyDiv, CommonSettingKey.Lending_Status_Setting.Key1, CommonSettingKey.Lending_Status_Setting.Key2, CommonSettingKey.Lending_Status_Setting.Key3, True)
            '    Dim iLeadTime As Decimal = Integer.Parse(lendingStatusSetting.DATA1)
            '    _ViewModel.SearchDspStartDate = Now.AddMonths(iLeadTime).ToString("yyyy-MM-dd")
            'End If

            '' Dim model As DepartmentModel = _viewResult
            ''一覧表示処理
            'MyBase.BaseIndex(_ViewModel)

            ''セッションクリア
            ''TODO DisposeIndexで消される前に、何らかの対応をしたい。
            'If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
            '    Session.Remove(CommonSessionKey.CREAGE_LENDING_STATUS)
            'End If


            '結果表示
            Return Json(Session("ViewModel"))

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

#Region "凡例　表示設定取得、幅確定"
        ''' <summary>
        ''' 日付文字列　区切り文字追加処理
        ''' </summary>
        ''' <param name="settingInfo">設定情報</param>
        Private Sub SubGetLegendDispSettingAndWidthDecision(ByVal settingInfo As MessageRfid)
            Try

                '設定マスタ　凡例表示可否設定取得
                Dim ReserveLegendDisp As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Reserve_Legend_Disp.KeyDiv,
                                                                            CommonSettingKey.Reserve_Legend_Disp.Key1,
                                                                            CommonSettingKey.Reserve_Legend_Disp.Key2,
                                                                            CommonSettingKey.Reserve_Legend_Disp.Key3)

                If ReserveLegendDisp Is Nothing Then
                    '予約中
                    ViewBag.legendReserve = "Y"
                    '貸出中
                    ViewBag.legendLease = "Y"
                    'メンテナンス
                    ViewBag.legendMainte = "Y"
                    '返却済
                    ViewBag.legendHenkyaku = "Y"
                    '〇〇登録
                    ViewBag.legendBlaRegist = "N"
                    '予備登録１
                    ViewBag.legendPreRegist1 = "N"
                    '予備登録２
                    ViewBag.legendPreRegist2 = "N"
                Else
                    '予約中
                    ViewBag.legendReserve = ReserveLegendDisp.DATA1
                    '貸出中
                    ViewBag.legendLease = ReserveLegendDisp.DATA2
                    'メンテナンス
                    ViewBag.legendMainte = ReserveLegendDisp.DATA3
                    '返却済
                    ViewBag.legendHenkyaku = ReserveLegendDisp.DATA4
                    '〇〇登録
                    ViewBag.legendBlaRegist = ReserveLegendDisp.DATA5
                    '予備登録１
                    ViewBag.legendPreRegist1 = ReserveLegendDisp.DATA6
                    '予備登録２
                    ViewBag.legendPreRegist2 = ReserveLegendDisp.DATA7
                End If

                '英語時の文言取得
                Dim reserveWidth As String = settingInfo.getMessage(CommonLabel.VALUE_LEASESTATUS_RESERVE)
                Dim leaseWidth As String = settingInfo.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)
                Dim mainteWidth As String = settingInfo.getMessage(CommonLabel.VALUE_MAINTE)
                Dim retuenWidth As String = settingInfo.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)
                Dim blaRegWidth As String = settingInfo.getMessage(CommonLabel.VALUE_BLAREGIST)
                Dim preReg1Width As String = settingInfo.getMessage(CommonLabel.VALUE_PREREGIST1)
                Dim preReg2Width As String = settingInfo.getMessage(CommonLabel.VALUE_PREREGIST2)

                If ReserveLegendDisp Is Nothing Then
                    ViewBag.LegendDispWidth = "380px"
                Else
                    Dim legendWidth As Integer = 0
                    If ReserveLegendDisp.DATA1 = "Y" Then
                        legendWidth += 90
                        If reserveWidth = "予約中" Then
                            legendWidth += 5
                        End If
                    End If
                    If ReserveLegendDisp.DATA2 = "Y" Then
                        legendWidth += 90
                        If leaseWidth = "貸出中" Then
                            legendWidth += 5
                        End If
                    End If
                    If ReserveLegendDisp.DATA3 = "Y" Then
                        legendWidth += 90
                        If mainteWidth.Count >= 6 Then
                            legendWidth += 60
                        End If
                        If mainteWidth = "メンテ" Then
                            legendWidth += 5
                        End If
                    End If
                    If ReserveLegendDisp.DATA4 = "Y" Then
                        legendWidth += 90
                        If retuenWidth.Count >= 6 Then
                            legendWidth += 80
                        End If
                        If retuenWidth = "返却済" Then
                            legendWidth += 5
                        End If
                    End If
                    If ReserveLegendDisp.DATA5 = "Y" Then
                        legendWidth += 100
                    End If
                    If ReserveLegendDisp.DATA6 = "Y" Then
                        legendWidth += 120
                    End If
                    If ReserveLegendDisp.DATA7 = "Y" Then
                        legendWidth += 120
                    End If

                    ViewBag.LegendDispWidth = $"{legendWidth}px"
                End If

            Catch ex As Exception
                logger.Error(ex)
            End Try
        End Sub
#End Region

#Region "共通"
        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL(Optional reSearchFlg As Boolean = False) As String
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)
            '表示月数
            Dim lendingStatusSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Status_Setting.KeyDiv, CommonSettingKey.Lending_Status_Setting.Key1, CommonSettingKey.Lending_Status_Setting.Key2, CommonSettingKey.Lending_Status_Setting.Key3, True)
            Dim iDspMonthCount As Integer = Integer.Parse(lendingStatusSetting.DATA2)
            '出荷通知確認日の取得
            Dim lendingLTReserveData As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_LT_ReserveData.KeyDiv, CommonSettingKey.Lending_LT_ReserveData.Key1, CommonSettingKey.Lending_LT_ReserveData.Key2, CommonSettingKey.Lending_LT_ReserveData.Key3, True)
            Dim iShippingNotificationDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, Integer.Parse(lendingLTReserveData.DATA1), lendingLTReserveData.DATA2)
            '表示開始日
            Dim dtStartDate As Date = Date.Parse(_ViewModel.SearchDspStartDate)
            '表示終了日
            Dim dtEndDate As Date = dtStartDate.AddMonths(iDspMonthCount)

            If reSearchFlg Then
                Dim tblDelayAsset = ToolReserve.GetDelayReserveInfo("'" & String.Join("','", _viewResult.Select(Function(x) x.ASSET_NO).ToList) & "'")
                '表示開始日更新
                If Not tblDelayAsset.Rows(0).IsNull("PR_ST_DT") AndAlso dtStartDate > Date.Parse(CommonRfid.ToStringIntYMD(tblDelayAsset.Rows(0)("PR_ST_DT").ToString)) Then
                    dtStartDate = Date.Parse(CommonRfid.ToStringIntYMD(tblDelayAsset.Rows(0)("PR_ST_DT").ToString))
                    Dim tempDspStartDate = CommonRfid.ToHaifunYMD(Date.Parse(CommonRfid.ToStringIntYMD(tblDelayAsset.Rows(0)("PR_ST_DT").ToString)))
                    _ViewModel.SearchDspStartDate = tempDspStartDate
                    'モデルバインディング用
                    ModelState.SetModelValue("SearchDspStartDate", New ValueProviderResult(New String() {tempDspStartDate}, tempDspStartDate, CultureInfo.CurrentCulture))
                End If

                '表示終了日更新
                Dim tempDate As Date

                If Not tblDelayAsset.Rows(0).IsNull("PR_END_DT") AndAlso dtEndDate < Date.Parse(CommonRfid.ToStringIntYMD(tblDelayAsset.Rows(0)("PR_END_DT").ToString)) Then
                    dtEndDate = Date.Parse(CommonRfid.ToStringIntYMD(tblDelayAsset.Rows(0)("PR_END_DT").ToString))
                    tempDate = dtStartDate
                    While tempDate < dtEndDate
                        tempDate = tempDate.AddMonths(1)
                    End While
                    dtEndDate = tempDate
                End If

                '表示月数
                Dim monthDiff As Integer = 0
                tempDate = dtStartDate

                Do
                    tempDate = tempDate.AddMonths(1)
                    monthDiff += 1
                Loop While (Not tempDate.Year = dtEndDate.Year) OrElse (Not tempDate.Month = dtEndDate.Month)
                _ViewModel.DspMonthCount = monthDiff

            End If

            '2022/08/31 Add Start Kawamoto
            '非稼働日を取得
            Dim dtNotWorkDay As DataTable = Dao.ToolReserve.GetReserveDate(dtStartDate.ToString("yyyyMMdd"), dtEndDate.ToString("yyyyMMdd"), NotWorkFlg.Yes)
            _ViewModel.NotWorkDay = String.Join(",", dtNotWorkDay.AsEnumerable().Select(Function(row) row("DT")).ToArray())
            '2022/08/31 Add End Kawamoto

            '予約情報仮想表
            Dim sbRevTblSQL As StringBuilder = New StringBuilder
            With sbRevTblSQL
                .AppendLine("       (")
                .AppendLine("         SELECT")
                .AppendLine("            RESVM.ASSET_NO")
                .AppendLine("           ,RESVM.PR_END_DT")
                .AppendLine("           ,CASE WHEN RESVM.RESERVE_STATUS = '" & Misc.ReserveStatus.Lending & "' THEN ")
                .AppendLine("           	    CASE WHEN RESVM.REAL_END_DT < CONVERT(int, CONVERT(VARCHAR, GETDATE(), 112)) THEN '" & Misc.ReserveDspStat.Delay & "'")
                .AppendLine("           	    ELSE '" & Misc.ReserveDspStat.Lending & "'")
                .AppendLine("                  END")
                .AppendLine("               ELSE RESVM.RESERVE_STATUS")
                .AppendLine("               END AS RESERVE_DSPSTAT")
                .AppendLine("         FROM T_RESERVE_DETAIL RESVM")
                .AppendLine("         INNER JOIN T_RESERVE RESVH")
                .AppendLine("         ON  RESVH.RESERVE_ID = RESVM.RESERVE_ID")
                .AppendLine("         LEFT JOIN M_EMPLOYEE CHARGE_EMP")
                .AppendLine("         ON  CHARGE_EMP.EMPLOYEE_CD = RESVH.CHARGE_EMP_CD")
                .AppendLine("         LEFT JOIN M_EMPLOYEE REQ_EMP")
                .AppendLine("         ON  REQ_EMP.EMPLOYEE_CD = RESVH.REQ_EMP_CD")
                .AppendLine("         WHERE (@StartDate <= RESVM.PR_END_DT and RESVM.PR_ST_DT <= @EndDate) ")
                '                .AppendLine("         WHERE (@StartDate <= RESVM.PR_END_DT and RESVM.PR_ST_DT <= @EndDate) AND NOT EXISTS ( SELECT 1 FROM T_RESERVE_DETAIL DET WHERE RESVM.ASSET_NO = DET.ASSET_NO AND RESVM.RESERVE_ID < DET.RESERVE_ID AND RESVM.PR_END_DT >= DET.PR_START_DT  AND RESVM.PR_START_DT <= DET.PR_END_DT) ")
                '担当者
                If Not String.IsNullOrEmpty(_ViewModel.SearchEmpName) Then
                    .AppendLine("         AND   CHARGE_EMP.EMPLOYEE_NAME LIKE @ChargeEmpName")
                    addSqlParam("ChargeEmpName", $"%{_ViewModel.SearchEmpName}%")
                End If
                '申請者
                If Not String.IsNullOrEmpty(_ViewModel.SearchReqEmpName) Then
                    .AppendLine("         AND   REQ_EMP.EMPLOYEE_NAME LIKE @ReqEmpName")
                    addSqlParam("ReqEmpName", $"%{_ViewModel.SearchReqEmpName}%")
                End If
                ''工事番号
                If Not String.IsNullOrEmpty(_ViewModel.SearchConstNo) Then
                    .AppendLine("         AND   RESVH.REMARK = @ConstNo")
                    addSqlParam("ConstNo", $"{_ViewModel.SearchConstNo}")
                End If

                .AppendLine("       )")
            End With

#Region "検索条件SQLを作成"
            '取得資産の判断フラグ
            Dim isNoReserve As Boolean = False
            Dim isReserve As Boolean = False
            Dim isMainte As Boolean = False
            Dim isBlaRegist As Boolean = False
            Dim isPleRegist1 As Boolean = False
            Dim isPleRegist2 As Boolean = False
            Dim sbNoReserveSQL As StringBuilder = New StringBuilder
            Dim sbReserveSQL As StringBuilder = New StringBuilder
            Dim sbMainteSQL As StringBuilder = New StringBuilder

            If _ViewModel.SearchLeaseStatus_NoReserve Then
                isNoReserve = True
            End If
            If _ViewModel.SearchLeaseStatus_NotRented Or
                    _ViewModel.SearchLeaseStatus_Lending Or
                    _ViewModel.SearchLeaseStatus_LendingDelay Or
                    _ViewModel.SearchLeaseStatus_Returned Then
                isReserve = True
            End If
            If _ViewModel.SearchLeaseStatus_Mainte Then
                isMainte = True
            End If
            If _ViewModel.SearchLeaseStatus_BlaRegist Then
                isBlaRegist = True
            End If
            If _ViewModel.SearchLeaseStatus_PreRegist1 Then
                isPleRegist1 = True
            End If
            If _ViewModel.SearchLeaseStatus_PreRegist2 Then
                isPleRegist2 = True
            End If

            If isNoReserve Then
                With sbNoReserveSQL
                    .AppendLine("      (NOT EXISTS")
                    .AppendLine("      (")
                    .AppendLine("          SELECT 1 FROM T_RESERVE_DETAIL RESVM")
                    .AppendLine("              WHERE RESVM.ASSET_NO = ASSET.ASSET_NO")
                    .AppendLine("              AND   (@StartDate <= RESVM.PR_END_DT and RESVM.PR_ST_DT <= @EndDate)")
                    .AppendLine("      )")
                    .AppendLine("      )")
                End With
            End If

            If isReserve Then
                With sbReserveSQL
                    Dim sbWkJoken As StringBuilder = New StringBuilder
                    If _ViewModel.SearchLeaseStatus_NotRented Then
                        '未貸出
                        If sbWkJoken.Length > 0 Then
                            sbWkJoken.Append("          or ")
                        End If
                        sbWkJoken.AppendLine("          (RSV.RESERVE_DSPSTAT = '" & Misc.ReserveDspStat.Mi & "')")
                    End If
                    If _ViewModel.SearchLeaseStatus_Lending Then
                        '貸出中
                        If sbWkJoken.Length > 0 Then
                            sbWkJoken.Append("          or ")
                        End If
                        sbWkJoken.AppendLine("          (RSV.RESERVE_DSPSTAT = '" & Misc.ReserveDspStat.Lending & "' )")
                    End If
                    If _ViewModel.SearchLeaseStatus_LendingDelay Then
                        '貸出中(遅延)
                        If sbWkJoken.Length > 0 Then
                            sbWkJoken.Append("          or ")
                        End If
                        sbWkJoken.AppendLine("          (RSV.RESERVE_DSPSTAT = '" & Misc.ReserveDspStat.Delay & "')")
                    End If
                    If _ViewModel.SearchLeaseStatus_Returned Then
                        '返却済
                        If sbWkJoken.Length > 0 Then
                            sbWkJoken.Append("          or ")
                        End If
                        sbWkJoken.AppendLine("          (RSV.RESERVE_DSPSTAT = '" & Misc.ReserveDspStat.HenkyakuSumi & "')")
                    End If

                    .AppendLine("      (EXISTS")
                    .AppendLine("      (")
                    .AppendLine("          SELECT 1 FROM ")
                    .AppendLine("              " & sbRevTblSQL.ToString & " AS RSV")
                    .AppendLine("              WHERE RSV.ASSET_NO = ASSET.ASSET_NO")
                    .AppendLine("              AND   (")
                    .AppendLine("                       " & sbWkJoken.ToString)
                    .AppendLine("                    )")
                    .AppendLine("      )")
                    .AppendLine("      )")
                End With
            End If

            Dim mainteKbn As String = String.Empty
            If isMainte OrElse isBlaRegist OrElse isPleRegist1 OrElse isPleRegist2 Then
                With sbMainteSQL
                    .AppendLine("      (EXISTS")
                    .AppendLine("      (")
                    .AppendLine("            SELECT 1 FROM T_RESERVE_MAINTE MNTT")
                    .AppendLine("               LEFT JOIN T_RESERVE RESVH")
                    .AppendLine("               ON  RESVH.RESERVE_ID = MNTT.RESERVE_ID")
                    .AppendLine("               LEFT JOIN M_EMPLOYEE CHARGE_EMP")
                    .AppendLine("               ON  CHARGE_EMP.EMPLOYEE_CD = RESVH.CHARGE_EMP_CD")
                    .AppendLine("               LEFT JOIN M_EMPLOYEE REQ_EMP")
                    .AppendLine("               ON  REQ_EMP.EMPLOYEE_CD = RESVH.REQ_EMP_CD")

                    .AppendLine("               WHERE MNTT.ASSET_NO = ASSET.ASSET_NO")
                    .AppendLine("               AND  (@StartDate <= MNTT.MAINTE_END_DT and MNTT.MAINTE_START_DT <= @EndDate)")

                    .AppendLine("         AND MNTT.MAINTE_KBN IN ( ")

                    'メンテナンス区分
                    'メンテナンス
                    If isMainte Then
                        .AppendLine("    '" & AssetReserveInfo.Mainte & "'")
                    End If
                    '〇〇登録
                    If isBlaRegist Then
                        If isMainte Then
                            .Append("    , ")
                        End If
                        .AppendLine("    '" & AssetReserveInfo.blaRegist & "'")
                    End If
                    '予備登録１
                    If isPleRegist1 Then
                        If isMainte OrElse isBlaRegist Then
                            .Append("    , ")
                        End If
                        .AppendLine("    '" & AssetReserveInfo.preRegist1 & "'")
                    End If
                    '予備登録１
                    If isPleRegist2 Then
                        If isMainte OrElse isBlaRegist OrElse isPleRegist1 Then
                            .Append("    , ")
                        End If
                        .AppendLine("    '" & AssetReserveInfo.preRegist2 & "'")
                    End If
                    .AppendLine("         ) ")

                    '担当者
                    If Not String.IsNullOrEmpty(_ViewModel.SearchEmpName) Then
                        .AppendLine("         AND   CHARGE_EMP.EMPLOYEE_NAME LIKE @ChargeEmpName")
                    End If
                    '申請者
                    If Not String.IsNullOrEmpty(_ViewModel.SearchReqEmpName) Then
                        .AppendLine("         AND   REQ_EMP.EMPLOYEE_NAME LIKE @ReqEmpName")
                    End If
                    '工事番号
                    If Not String.IsNullOrEmpty(_ViewModel.SearchConstNo) Then
                        .AppendLine("         AND   RESVH.REMARK = @ConstNo")
                    End If
                    .AppendLine("      )")
                    .AppendLine("      )")
                End With
            End If

            Dim sbWhr As New StringBuilder
            sbWhr.AppendLine("  WHERE ASSET.ASSET_STATUS IN (@AssetStatus_Lending, @AssetStatus_Storage)")
            addSqlParam("AssetStatus_Lending", $"{ASSET_STATUS.Lending}")
            addSqlParam("AssetStatus_Storage", $"{ASSET_STATUS.Storage}")

            '照合機能設定を取得
            Dim MatchingFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.MatchingFunction.KeyDiv,
                                                                         CommonSettingKey.MatchingFunction.Key1,
                                                                         CommonSettingKey.MatchingFunction.Key2,
                                                                         CommonSettingKey.MatchingFunction.Key3, True)

            If Not IsNothing(MatchingFunc) AndAlso MatchingFunc.DATA1 = ValidFlg.Cd.Valid Then
                '照合機能あり
                sbWhr.AppendLine("  AND ASSET.LEASE_STATUS IN ('" & AssetLeaseStatus.Ok & "','" & AssetLeaseStatus.Ng & "')")
            Else
                '照合機能無し
                sbWhr.AppendLine(" And ASSET.LEASE_STATUS ='" & AssetLeaseStatus.Ok & "'")
            End If


            '管理ロケ大区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SelectMngLocKeys) Then
                Dim sbSQLMngLocKey As StringBuilder = New StringBuilder
                Dim selectMngLocKeys As String() = _ViewModel.SelectMngLocKeys.Split(", ")
                For idx As Integer = 0 To selectMngLocKeys.Length - 1
                    If sbSQLMngLocKey.Length <> 0 Then
                        sbSQLMngLocKey.Append(", ")
                    End If
                    sbSQLMngLocKey.Append("@MngLocLCd" & idx)
                    addSqlParam("MngLocLCd" & idx, $"{selectMngLocKeys(idx)}")
                Next
                sbWhr.AppendLine("  AND ASSET.MNG_LOC_L_CD IN (" & sbSQLMngLocKey.ToString & ")")
            End If
            'カテゴリ大分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatL) Then
                sbWhr.AppendLine("  AND ASSET.CAT_L_CD = @SearchCatL")
                addSqlParam("SearchCatL", $"{_ViewModel.SearchCatL}")
            End If
            'カテゴリ中分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatM) Then
                sbWhr.AppendLine("  AND ASSET.CAT_M_CD = @SearchCatM")
                addSqlParam("SearchCatM", $"{_ViewModel.SearchCatM}")
            End If
            'カテゴリ小分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatS) Then
                sbWhr.AppendLine("  AND ASSET.CAT_S_CD = @SearchCatS")
                addSqlParam("SearchCatS", $"{_ViewModel.SearchCatS}")
            End If

            '--2022/03/07 nv miyaji add start
            '資産番号 SearchAssetNo
            If (Not IsNothing(_ViewModel.SearchAssetNo)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchAssetNo.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.ASSET_NO like " & "@SearchAssetNo")
                addSqlParam("SearchAssetNo", $"{_ViewModel.SearchAssetNo.Trim & "%"}")
            End If

            '資産名称 SearchAssetName
            If (Not IsNothing(_ViewModel.SearchAssetName)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchAssetName.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.ASSET_NAME like " & "@SearchAssetName")
                addSqlParam("SearchAssetName", $"{"%" & _ViewModel.SearchAssetName.Trim & "%"}")
            End If
            '--2022/03/07 nv miyaji add end

            '貸出状態ステータス条件
            If isNoReserve AndAlso isReserve AndAlso isMainte AndAlso isBlaRegist AndAlso isPleRegist1 AndAlso isPleRegist2 AndAlso
               String.IsNullOrEmpty(_ViewModel.SearchEmpName) AndAlso String.IsNullOrEmpty(_ViewModel.SearchReqEmpName) AndAlso String.IsNullOrEmpty(_ViewModel.SearchConstNo) Then
                '貸出状態いずれの状態も検索条件に含まれる＋担当者・申請者が絞り込みされていない場合は検索条件なし。

            Else
                Dim sChkJoken As String = String.Empty
                If isNoReserve Then
                    If String.IsNullOrEmpty(sChkJoken) Then
                        sChkJoken &= "   (" & vbCrLf
                    Else
                        sChkJoken &= " or " & vbCrLf
                    End If
                    sChkJoken &= sbNoReserveSQL.ToString
                End If
                If isReserve Then
                    If String.IsNullOrEmpty(sChkJoken) Then
                        sChkJoken &= "   (" & vbCrLf
                    Else
                        sChkJoken &= " or " & vbCrLf
                    End If
                    sChkJoken &= sbReserveSQL.ToString
                End If
                If isMainte OrElse isBlaRegist OrElse isPleRegist1 OrElse isPleRegist2 Then
                    If String.IsNullOrEmpty(sChkJoken) Then
                        sChkJoken &= "   (" & vbCrLf
                    Else
                        sChkJoken &= " or " & vbCrLf
                    End If
                    sChkJoken &= sbMainteSQL.ToString
                End If
                sChkJoken &= "   )"
                sbWhr.AppendLine("  AND ")
                sbWhr.AppendLine(sChkJoken)
            End If

            '品番
            If (Not IsNothing(_ViewModel.SearchProductNo)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchProductNo.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.PRODUCT_NO like " & "@SearchProductNo")
                addSqlParam("SearchProductNo", $"{"%" & _ViewModel.SearchProductNo.Trim & "%"}")
            End If
#End Region

            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            If _isSort Then
                sbSQL.AppendLine("SELECT *")
            Else
                sbSQL.AppendLine("SELECT TOP 1 *")
            End If
            sbSQL.AppendLine("  ,TotalCount")
            sbSQL.AppendLine("FROM")
            sbSQL.AppendLine("(")

            sbSQL.AppendLine("  SELECT")
            sbSQL.AppendLine("    ASSET.CAT_L_CD ")
            sbSQL.AppendLine("    , ASSET.CAT_M_CD ")
            sbSQL.AppendLine("    , ASSET.CAT_S_CD")
            sbSQL.AppendLine("    , CATL.CAT_NAME AS CAT_L_NAME ")
            sbSQL.AppendLine("    , CATM.CAT_NAME AS CAT_M_NAME ")
            sbSQL.AppendLine("    , CATS.CAT_NAME AS CAT_S_NAME ")
            sbSQL.AppendLine("    , ASSET.MNG_LOC_L_CD ")
            sbSQL.AppendLine("    , MNGLOC.LOC_NAME AS MNG_LOC_L_NAME ")
            sbSQL.AppendLine("    , ASSET.ASSET_NO ")
            sbSQL.AppendLine("    , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("    , ASSET.MODEL_NO ")       '2022/08/31 Add Kawamoto
            sbSQL.AppendLine("    , ASSET.LEASE_STATUS ")
            sbSQL.AppendLine("    , ASSET.LEASE_CNT ")
            sbSQL.AppendLine("    , AUTH.SHARED_FLG")
            sbSQL.AppendLine("    , AUTH.UPD_FLG")
            'sbSQL.AppendLine("    , CONST.CONST_NO")
            sbSQL.AppendLine("    , ASSET.PRODUCT_NO")
            sbSQL.AppendLine("    , dbo.RESERVE_DEST_NO_SF(ASSET.ASSET_NO, @ShippingNotificationDate) AS DEST_NO")  '-- '':未　NULL:予約なし「-」表示する　 以外:送り状番号
            'sbSQL.AppendLine("    , RESVM.RESERVE_ID ")
            'sbSQL.AppendLine("    , RESVM.PR_START_DT ")
            'sbSQL.AppendLine("    , RESVM.PR_END_DT ")
            addSqlParam("ShippingNotificationDate", $"{iShippingNotificationDate}")
            sbSQL.AppendLine("    , dbo.ASSET_RESERVE_INFOSTR(ASSET.ASSET_NO, @StartDate, @EndDate) AS CAL_STR")    '-- なし:' '  貸出中:'L'  予約:'R' 返却済み:'F'  メンテナンス:'M'
            addSqlParam("StartDate", $"{dtStartDate.ToString("yyyyMMdd")}")
            addSqlParam("EndDate", $"{dtEndDate.ToString("yyyyMMdd")}")
            'addSqlParam("ConstNo", $"{If(String.IsNullOrEmpty(_ViewModel.SearchConstNo), "", _ViewModel.SearchConstNo.Trim)}")
            sbSQL.AppendLine("    , ASSET.ASSET_STATUS")
            'sbSQL.AppendLine("    , MIN_PR_ST_DT")                         一旦、コメントアウト
            sbSQL.AppendLine("  FROM M_ASSET ASSET")
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuthHub) AUTH")
            sbSQL.AppendLine("  ON ASSET.MNG_LOC_L_CD = AUTH.LOC_L_CD")
            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{authInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{authInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{authInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{authInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthType_UpdAuthHub", $"{UpdAuthType.UPD_AUTH_HUB}")
            'sbSQL.AppendLine("  LEFT JOIN T_RESERVE_DETAIL RESVM")
            'sbSQL.AppendLine("  ON  RESVM.ASSET_NO = ASSET.ASSET_NO")
            'sbSQL.AppendLine("  LEFT JOIN T_RESERVE RESVH")
            'sbSQL.AppendLine("  ON  RESVH.RESERVE_ID = RESVM.RESERVE_ID")
            'sbSQL.AppendLine("  LEFT JOIN M_CONSTRUCTION CONST")
            'sbSQL.AppendLine("  ON  CONST.CONST_NO = RESVH.REMARK")
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATL ")
            sbSQL.AppendLine("  ON CATL.CAT_DIV = @CatDivL")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATL.CAT_L_CD")
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATM ")
            sbSQL.AppendLine("  ON CATM.CAT_DIV = @CatDivM")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATM.CAT_L_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_M_CD = CATM.CAT_M_CD")
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATS ")
            sbSQL.AppendLine("  ON CATS.CAT_DIV = @CatDivS")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATS.CAT_L_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_M_CD = CATS.CAT_M_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_S_CD = CATS.CAT_S_CD")
            sbSQL.AppendLine("  LEFT JOIN M_LOCATION MNGLOC ")
            sbSQL.AppendLine("  ON MNGLOC.LOC_DIV = @LocDivL")
            sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD = MNGLOC.LOC_L_CD")
            addSqlParam("CatDivL", $"{CAT_DIV.CategoryL}")
            addSqlParam("CatDivM", $"{CAT_DIV.CategoryM}")
            addSqlParam("CatDivS", $"{CAT_DIV.CategoryS}")
            addSqlParam("LocDivL", $"{LOC_DIV.LocationL}")
            sbSQL.AppendLine("")
            sbSQL.AppendLine(sbWhr.ToString)
            sbSQL.AppendLine(") MAINTBL")

            sbSQL.AppendLine("LEFT JOIN (")
            sbSQL.AppendLine("  SELECT COUNT(*) TotalCount")

            sbSQL.AppendLine(" FROM (")
            sbSQL.AppendLine("  SELECT dbo.RESERVE_DEST_NO_SF(ASSET.ASSET_NO, @ShippingNotificationDate) AS DEST_NO")
            sbSQL.AppendLine("  FROM M_ASSET ASSET ")
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuthHub) AS AUTH")
            sbSQL.AppendLine("  ON ASSET.MNG_LOC_L_CD = AUTH.LOC_L_CD")
            sbSQL.AppendLine("  " & sbWhr.ToString)
            sbSQL.AppendLine(") B")
            sbSQL.AppendLine("WHERE 1 = 1")

            '期間重複した予約を除外
            'sbSQL.AppendLine(" AND NOT EXISTS ( SELECT 1 FROM T_RESERVE_DETAIL DET WHERE B.ASSET_NO = DET.ASSET_NO AND B.RESERVE_ID < DET.RESERVE_ID AND B.PR_END_DT >= DET.PR_START_DT  AND B.PR_START_DT <= DET.PR_END_DT) ")


            '出荷通知
            If _ViewModel.SearchShippingNotification_NotNotified AndAlso _ViewModel.SearchShippingNotification_Notified = False Then
                '-- 出荷通知の未通知 のみチェック時
                sbSQL.AppendLine("  AND  (DEST_NO IS NULL or DEST_NO = '')")
            ElseIf _ViewModel.SearchShippingNotification_NotNotified = False AndAlso _ViewModel.SearchShippingNotification_Notified Then
                '-- 出荷通知の通知 のみチェック時
                sbSQL.AppendLine("  AND  (DEST_NO <> '')")
            End If
            sbSQL.AppendLine(") A")
            sbSQL.AppendLine("ON 1=1")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '出荷通知
            If _ViewModel.SearchShippingNotification_NotNotified AndAlso _ViewModel.SearchShippingNotification_Notified = False Then
                '-- 出荷通知の未通知 のみチェック時
                sbSQL.AppendLine("  AND  (MAINTBL.DEST_NO IS NULL or MAINTBL.DEST_NO = '')")
            ElseIf _ViewModel.SearchShippingNotification_NotNotified = False AndAlso _ViewModel.SearchShippingNotification_Notified Then
                '-- 出荷通知の通知 のみチェック時
                sbSQL.AppendLine("  AND  (MAINTBL.DEST_NO <> '')")
            End If

            If _isSort Then
                '並び替え
                If _ViewModel.SearchSort = LendingStatusSort.Reserve Then
                    If ViewBag.MatchingFuncFlg = ValidFlg.Cd.Valid Then
                        '--照合機能時の並び替え
                        sbSQL.AppendLine("ORDER BY ")
                        sbSQL.AppendLine("  PRODUCT_NO ")
                        sbSQL.AppendLine("  , SHARED_FLG DESC ")
                        sbSQL.AppendLine("  , MNG_LOC_L_CD ")
                        sbSQL.AppendLine("  , ASSET_NO ")
                    Else
                        '-- 並び替え条件が「予約観点」の場合
                        '-- カテゴリ大・中・小の昇順、管理ロケの共通/拠点区分順(共通が優先)の昇順、管理ロケコードの昇順、貸出回数の昇順、資産番号の昇順に表示する。
                        sbSQL.AppendLine("ORDER BY ")
                        sbSQL.AppendLine("  CAT_L_CD ")
                        sbSQL.AppendLine("  , CAT_M_CD ")
                        sbSQL.AppendLine("  , CAT_S_CD ")
                        sbSQL.AppendLine("  , SHARED_FLG DESC ")
                        sbSQL.AppendLine("  , MNG_LOC_L_CD ")
                        sbSQL.AppendLine("  , LEASE_CNT ")
                        sbSQL.AppendLine("  , ASSET_NO ")
                    End If


                Else
                    '-- 並び替え条件が「メンテナンス観点」の場合
                    '-- 管理ロケの共通/拠点区分順(共通が優先)の昇順、管理ロケコードの昇順、実質貸出開始日の昇順、資産番号の昇順の順に表示する。
                    sbSQL.AppendLine("ORDER BY ")
                    sbSQL.AppendLine("  SHARED_FLG DESC ")
                    sbSQL.AppendLine("  , MNG_LOC_L_CD ")
                    sbSQL.AppendLine("  , (")
                    sbSQL.AppendLine("      SELECT MIN(TMP.PR_END_DT) FROM ")
                    sbSQL.AppendLine("      " & sbRevTblSQL.ToString & "AS TMP")
                    sbSQL.AppendLine("      WHERE TMP.ASSET_NO = MAINTBL.ASSET_NO")
                    sbSQL.AppendLine("      GROUP BY TMP.ASSET_NO")
                    sbSQL.AppendLine("    )")
                    sbSQL.AppendLine("  , ASSET_NO ")
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
                _viewResult.Add(New LendingStatusViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).mngLocCheckBoxListData = _ViewModel.mngLocCheckBoxListData
            _viewResult.ElementAt(0).SelectMngLocKeys = _ViewModel.SelectMngLocKeys
            _viewResult.ElementAt(0).SearchAssetNo = _ViewModel.SearchAssetNo   '2022/02/25 nv miyaji add
            _viewResult.ElementAt(0).SearchAssetName = _ViewModel.SearchAssetName   '2022/02/25 nv miyaji add
            _viewResult.ElementAt(0).SearchEmpName = _ViewModel.SearchEmpName
            _viewResult.ElementAt(0).SearchReqEmpName = _ViewModel.SearchReqEmpName
            _viewResult.ElementAt(0).SearchConstNo = _ViewModel.SearchConstNo
            _viewResult.ElementAt(0).SearchCatL = _ViewModel.SearchCatL
            _viewResult.ElementAt(0).SearchCatM = _ViewModel.SearchCatM
            _viewResult.ElementAt(0).SearchCatS = _ViewModel.SearchCatS
            _viewResult.ElementAt(0).SearchDspStartDate = _ViewModel.SearchDspStartDate
            _viewResult.ElementAt(0).DspMonthCount = _ViewModel.DspMonthCount
            _viewResult.ElementAt(0).SearchLeaseStatus_NoReserve = _ViewModel.SearchLeaseStatus_NoReserve
            _viewResult.ElementAt(0).SearchLeaseStatus_NotRented = _ViewModel.SearchLeaseStatus_NotRented
            _viewResult.ElementAt(0).SearchLeaseStatus_Lending = _ViewModel.SearchLeaseStatus_Lending
            _viewResult.ElementAt(0).SearchLeaseStatus_LendingDelay = _ViewModel.SearchLeaseStatus_LendingDelay
            _viewResult.ElementAt(0).SearchLeaseStatus_Returned = _ViewModel.SearchLeaseStatus_Returned
            _viewResult.ElementAt(0).SearchLeaseStatus_Mainte = _ViewModel.SearchLeaseStatus_Mainte
            _viewResult.ElementAt(0).SearchLeaseStatus_BlaRegist = _ViewModel.SearchLeaseStatus_BlaRegist
            _viewResult.ElementAt(0).SearchLeaseStatus_PreRegist1 = _ViewModel.SearchLeaseStatus_PreRegist1
            _viewResult.ElementAt(0).SearchLeaseStatus_PreRegist2 = _ViewModel.SearchLeaseStatus_PreRegist2
            _viewResult.ElementAt(0).SearchShippingNotification_NotNotified = _ViewModel.SearchShippingNotification_NotNotified
            _viewResult.ElementAt(0).SearchShippingNotification_Notified = _ViewModel.SearchShippingNotification_Notified
            _viewResult.ElementAt(0).SearchSort = _ViewModel.SearchSort
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
            setIsClosedColumnList()
            _viewResult.ElementAt(0).isClosedColumns = _ViewModel.isClosedColumns
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).NotWorkDay = _ViewModel.NotWorkDay         '2022/08/31 Add Kawamoto

            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.LENDING_STATUS)
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.LENDING_STATUS).FIX_COL_CNT

        End Sub

#End Region

    End Class
End Namespace
