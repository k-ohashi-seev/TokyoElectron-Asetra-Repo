Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient
Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports GrapeCity.Documents.Excel
Imports GrapeCity.Documents.Excel.Drawing

Namespace Controllers
    Public Class ConstructionListController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"
        ''' <summary>
        ''' ページ間のチェックボックス対応
        ''' </summary>
        Private Enum ConstructionListKey
            CONST_NO = 0
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

        Public Const DELIMITER As String = "$$$"
#End Region

#Region "クラス変数"
        Private _Model As ConstructionListModel = New ConstructionListModel                   'Constructionモデル
        Private _ViewModel As ConstructionListViewModel = New ConstructionListViewModel       'Construction一覧表示用モデル
        Protected _viewResult As List(Of ConstructionListViewModel) = New List(Of ConstructionListViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _dtCsvImport As DataTable = Nothing 'CSV取込用作業テーブル
        Private _lstCsvHed As List(Of String) = Nothing 'CSVヘッダ文字コード格納用リスト
        Private _sBeforeAssetID As String = String.Empty '新規登録時、0埋め・自動生成前の資産IDを格納する
        Private _iCsvImportRowCount As Integer = 0 'CSV取込件数

        Private _LocationLList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationMList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationSList As List(Of LocationDropdownModel)     'ロケーションマスタリスト

        Private _RegistInfo As New List(Of Dictionary(Of String, String))     '登録情報

        Public Const MIN_DATE As Integer = 10000101
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As ConstructionListViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(RedirectConstructionListInfo.ID) IsNot Nothing Then
                Session.Remove(RedirectConstructionListInfo.ID)
                'チェックボックス状態の選択状態をクリアする
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
            End If

            If Session(CommonSessionKey.SEARCH_FLG) IsNot Nothing Then
                Session.Remove(CommonSessionKey.SEARCH_FLG)
            End If

            If Session(CommonSessionKey.CREAGE_CONSTRUCTION_EDIT) IsNot Nothing Then
                Session.Remove(CommonSessionKey.CREAGE_CONSTRUCTION_EDIT)
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
        Public Function Index(ByVal ViewModel As ConstructionListViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_CONSTRUCTION_LIST)
            End If

            'メッセージ一覧取得
            Dim Message As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '検索ボタンがクリックされた場合、選択状態のセッションをクリアする
            If Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                'セッションSEARCH_FLGを設定
                Session(CommonSessionKey.SEARCH_FLG) = True


                '一覧の選択状態をクリアする。
                Dim delModel As ConstructionListViewModel = Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST)
                If Not IsNothing(delModel) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                    delModel.SelectKeys = ""
                Else
                    ViewModel.SelectKeys = ""
                End If
            End If

            '工事番号の自動生成設定を取得
            Dim ConstNoSelfReg As SettingInfo = Message.getSettingInfo(CommonSettingKey.ConstNo_Self_Regist.KeyDiv,
                                                                  CommonSettingKey.ConstNo_Self_Regist.Key1,
                                                                  CommonSettingKey.ConstNo_Self_Regist.Key2,
                                                                  CommonSettingKey.ConstNo_Self_Regist.Key3, True)

            If Not IsNothing(ConstNoSelfReg) AndAlso ConstNoSelfReg.DATA1 = ValidFlg.Cd.Valid Then
                '工事番号の自動生成あり
                ViewBag.ConstNoSelfReg = ValidFlg.Cd.Valid
            End If

            '照合機能の予約実施設定を取得
            Dim MatchingReserveFlg As SettingInfo = Message.getSettingInfo(CommonSettingKey.MatchingReserveFunction.KeyDiv,
                                                                  CommonSettingKey.MatchingReserveFunction.Key1,
                                                                  CommonSettingKey.MatchingReserveFunction.Key2,
                                                                  CommonSettingKey.MatchingReserveFunction.Key3, True)

            If Not IsNothing(MatchingReserveFlg) AndAlso MatchingReserveFlg.DATA1 = ValidFlg.Cd.Valid Then
                '照合機能の予約実施あり
                ViewBag.MatchingReserveFlg = ValidFlg.Cd.Valid
            End If

            '仮置き機能設定を取得
            Dim TemporaryLeaveFlg As SettingInfo = Message.getSettingInfo(CommonSettingKey.TemporaryLeaveFunction.KeyDiv,
                                                                  CommonSettingKey.TemporaryLeaveFunction.Key1,
                                                                  CommonSettingKey.TemporaryLeaveFunction.Key2,
                                                                  CommonSettingKey.TemporaryLeaveFunction.Key3, True)

            If Not IsNothing(TemporaryLeaveFlg) AndAlso TemporaryLeaveFlg.DATA1 = ValidFlg.Cd.Valid Then
                '仮置き機能あり
                ViewBag.TemporaryLeaveFlg = ValidFlg.Cd.Valid
            End If

            '工事帳票印刷設定を取得
            Dim ConstPrintFlg As SettingInfo = Message.getSettingInfo(CommonSettingKey.ConstPrintSetting.KeyDiv,
                                                                  CommonSettingKey.ConstPrintSetting.Key1,
                                                                  CommonSettingKey.ConstPrintSetting.Key2,
                                                                  CommonSettingKey.ConstPrintSetting.Key3, True)

            If Not IsNothing(ConstPrintFlg) AndAlso ConstPrintFlg.DATA1 = ValidFlg.Cd.Valid Then
                '工事帳票印刷あり
                ViewBag.ConstPrintFlg = ValidFlg.Cd.Valid
            End If

            '実質貸出期間の補正日数設定を取得
            Dim ConstLentTerm As SettingInfo = Message.getSettingInfo(CommonSettingKey.Const_Lent_Term.KeyDiv,
                                                                  CommonSettingKey.Const_Lent_Term.Key1,
                                                                  CommonSettingKey.Const_Lent_Term.Key2,
                                                                  CommonSettingKey.Const_Lent_Term.Key3, True)

            If Not IsNothing(ConstLentTerm) Then
                '実質貸出期間の補正日数
                Dim tempDate = 0
                Integer.TryParse(ConstLentTerm.DATA1, tempDate)
                ViewBag.ConstLentTermFrom = tempDate

                tempDate = 0
                Integer.TryParse(ConstLentTerm.DATA2, tempDate)
                ViewBag.ConstLentTermTo = tempDate
            End If


            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As ConstructionListViewModel = Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            '最新のチェック状態でセッション情報を更新する(追加および削除)
            Dim info As RedirectConstructionListInfo = Session(RedirectConstructionListInfo.ID)
            If info IsNot Nothing Then
                If info.RedirectPage = RedirectConstructionListInfo.RedirectPageType.ConstructionList Then
                    '工事資産一覧、工事履歴一覧、仮置一覧画面から戻ったときの処理
                    Dim senimaeModel As ConstructionListViewModel = Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST_SEARCH_KEY)
                    _ViewModel = senimaeModel
                    Session.Remove(CommonSessionKey.CREAGE_CONSTRUCTION_LIST_SEARCH_KEY)

                    If Session(CommonSessionKey.CREAGE_CONSTRUCTION_EDIT) IsNot Nothing Then
                        Session.Remove(CommonSessionKey.CREAGE_CONSTRUCTION_EDIT)
                        ViewBag.ModalOpenFlg = "Y"
                    End If

                    '遷移元情報セッションクリア
                    Session(RedirectConstructionListInfo.ID) = Nothing
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

                '初期表示
                '項目配置情報取得
                Dim dictGridInfo = GetGridInfo()

                '検索条件リストデータ取得
                Dim searchItemList = GetSearchItem()

                'セッションSEARCH_FLGがNothingの場合
                If Session(CommonSessionKey.SEARCH_FLG) Is Nothing Then
                    '検索条件の初期値設定
                    SetInitCondition(searchItemList, dictGridInfo)
                    If String.IsNullOrEmpty(_ViewModel.SearchConstStatus) Then
                        _ViewModel.SearchConstStatus = CONST_STATUS.Incomplete
                    End If
                End If

                '表示する
                MyBase.BaseIndex(_ViewModel)

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            '最新のチェック状態でセッション情報を更新する(追加および削除)
            UpdateChecedInfo(_ViewModel.SelectKeys, _ViewModel.NoneSelectKeys)

            '入力設定リストデータ取得
            GetInputItem()

            '工事状態ドロップダウンリストデータ取得
            GetConstStatusData()

            '管理ロケーションドロップダウンリスト作成
            GetMngLocationLData()

            '作業責任者ドロップダウンリスト作成
            GetConstEmployeeData()

            ' プリンタ情報取得
            GetPrinter()

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.CONSTRUCTION_LIST)
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
            MyBase._SQL = CreateSQL()

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
                Session.Remove(CommonSessionKey.CREAGE_CONSTRUCTION_LIST)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of ConstructionListViewModel) = _db.ConstructionListView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function Create(ByVal ViewModel As ConstructionListViewModel, ByVal OnCreteButton As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '工事開始日・工事終了日・工事貸出終了日・工事貸出終了日を設定する
            If String.IsNullOrEmpty(_ViewModel.ConstStartDtString) Then
                _ViewModel.CONST_START_DT = Nothing
            Else
                _ViewModel.CONST_START_DT = CInt(CDate(_ViewModel.ConstStartDtString).ToString("yyyyMMdd"))
            End If
            If String.IsNullOrEmpty(_ViewModel.ConstEndDtString) Then
                _ViewModel.CONST_END_DT = Nothing
            Else
                _ViewModel.CONST_END_DT = CInt(CDate(_ViewModel.ConstEndDtString).ToString("yyyyMMdd"))
            End If
            If String.IsNullOrEmpty(_ViewModel.ConstLentStartDtString) Then
                _ViewModel.CONST_LENT_START_DT = Nothing
            Else
                _ViewModel.CONST_LENT_START_DT = CInt(CDate(_ViewModel.ConstLentStartDtString).ToString("yyyyMMdd"))
            End If
            If String.IsNullOrEmpty(_ViewModel.ConstLentEndDtString) Then
                _ViewModel.CONST_LENT_END_DT = Nothing
            Else
                _ViewModel.CONST_LENT_END_DT = CInt(CDate(_ViewModel.ConstLentEndDtString).ToString("yyyyMMdd"))
            End If

            '工事責任者氏名を設定する
            '--社員データを取得
            Dim tblEmployee As DataTable = Dao.ToolEmployee.GetEmployeeData(_ViewModel.CONST_EMPLOYEE_CD)
            If Not tblEmployee.Rows.Count = 0 Then
                _ViewModel.CONST_EMPLOYEE_NAME = tblEmployee.Rows(0)("EMPLOYEE_NAME").ToString
            End If

            '工事情報1～20を設定する(範囲入力の場合)
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO1_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO1_TO) Then
                _ViewModel.CONST_INFO1 = If(IsNothing(_ViewModel.CONST_INFO1_FROM), "", ViewModel.CONST_INFO1_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO1_TO), "", ViewModel.CONST_INFO1_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO2_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO2_TO) Then
                _ViewModel.CONST_INFO2 = If(IsNothing(_ViewModel.CONST_INFO2_FROM), "", ViewModel.CONST_INFO2_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO2_TO), "", ViewModel.CONST_INFO2_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO3_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO3_TO) Then
                _ViewModel.CONST_INFO3 = If(IsNothing(_ViewModel.CONST_INFO3_FROM), "", ViewModel.CONST_INFO3_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO3_TO), "", ViewModel.CONST_INFO3_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO4_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO4_TO) Then
                _ViewModel.CONST_INFO4 = If(IsNothing(_ViewModel.CONST_INFO4_FROM), "", ViewModel.CONST_INFO4_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO4_TO), "", ViewModel.CONST_INFO4_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO5_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO5_TO) Then
                _ViewModel.CONST_INFO5 = If(IsNothing(_ViewModel.CONST_INFO5_FROM), "", ViewModel.CONST_INFO5_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO5_TO), "", ViewModel.CONST_INFO5_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO6_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO6_TO) Then
                _ViewModel.CONST_INFO6 = If(IsNothing(_ViewModel.CONST_INFO6_FROM), "", ViewModel.CONST_INFO6_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO6_TO), "", ViewModel.CONST_INFO6_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO7_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO7_TO) Then
                _ViewModel.CONST_INFO7 = If(IsNothing(_ViewModel.CONST_INFO7_FROM), "", ViewModel.CONST_INFO7_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO7_TO), "", ViewModel.CONST_INFO7_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO8_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO8_TO) Then
                _ViewModel.CONST_INFO8 = If(IsNothing(_ViewModel.CONST_INFO8_FROM), "", ViewModel.CONST_INFO8_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO8_TO), "", ViewModel.CONST_INFO8_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO9_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO9_TO) Then
                _ViewModel.CONST_INFO9 = If(IsNothing(_ViewModel.CONST_INFO9_FROM), "", ViewModel.CONST_INFO9_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO9_TO), "", ViewModel.CONST_INFO9_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO10_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO10_TO) Then
                _ViewModel.CONST_INFO10 = If(IsNothing(_ViewModel.CONST_INFO10_FROM), "", ViewModel.CONST_INFO10_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO10_TO), "", ViewModel.CONST_INFO10_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO11_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO11_TO) Then
                _ViewModel.CONST_INFO11 = If(IsNothing(_ViewModel.CONST_INFO11_FROM), "", ViewModel.CONST_INFO11_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO11_TO), "", ViewModel.CONST_INFO11_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO12_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO12_TO) Then
                _ViewModel.CONST_INFO12 = If(IsNothing(_ViewModel.CONST_INFO12_FROM), "", ViewModel.CONST_INFO12_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO12_TO), "", ViewModel.CONST_INFO12_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO13_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO13_TO) Then
                _ViewModel.CONST_INFO13 = If(IsNothing(_ViewModel.CONST_INFO13_FROM), "", ViewModel.CONST_INFO13_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO13_TO), "", ViewModel.CONST_INFO13_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO14_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO14_TO) Then
                _ViewModel.CONST_INFO14 = If(IsNothing(_ViewModel.CONST_INFO14_FROM), "", ViewModel.CONST_INFO14_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO14_TO), "", ViewModel.CONST_INFO14_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO15_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO15_TO) Then
                _ViewModel.CONST_INFO15 = If(IsNothing(_ViewModel.CONST_INFO15_FROM), "", ViewModel.CONST_INFO15_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO15_TO), "", ViewModel.CONST_INFO15_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO16_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO16_TO) Then
                _ViewModel.CONST_INFO16 = If(IsNothing(_ViewModel.CONST_INFO16_FROM), "", ViewModel.CONST_INFO16_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO16_TO), "", ViewModel.CONST_INFO16_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO17_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO17_TO) Then
                _ViewModel.CONST_INFO17 = If(IsNothing(_ViewModel.CONST_INFO17_FROM), "", ViewModel.CONST_INFO17_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO17_TO), "", ViewModel.CONST_INFO17_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO18_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO18_TO) Then
                _ViewModel.CONST_INFO18 = If(IsNothing(_ViewModel.CONST_INFO18_FROM), "", ViewModel.CONST_INFO18_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO18_TO), "", ViewModel.CONST_INFO18_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO19_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO19_TO) Then
                _ViewModel.CONST_INFO19 = If(IsNothing(_ViewModel.CONST_INFO19_FROM), "", ViewModel.CONST_INFO19_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO19_TO), "", ViewModel.CONST_INFO19_TO)
            End If
            If Not String.IsNullOrEmpty(_ViewModel.CONST_INFO20_FROM) OrElse Not String.IsNullOrEmpty(_ViewModel.CONST_INFO20_TO) Then
                _ViewModel.CONST_INFO20 = If(IsNothing(_ViewModel.CONST_INFO20_FROM), "", ViewModel.CONST_INFO20_FROM) & DELIMITER & If(IsNothing(_ViewModel.CONST_INFO20_TO), "", ViewModel.CONST_INFO20_TO)
            End If

            '工事番号設定
            Dim autoConstNo As Boolean = False

            'モデルの再検証
            ModelState.Clear()
            TryValidateModel(_ViewModel)

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理
                '工事状態を設定する
                _ViewModel.CONST_STATUS = CONST_STATUS.Incomplete

                '工事番号の自動生成確認
                autoConstNo = IsAutoConstNo()
                If IsAutoConstNo() Then
                    '工事番号の自動生成あり

                    '工事番号を生成
                    _ViewModel.CONST_NO = ConstNoAutoGenelate()
                End If

                MyBase.BaseCreate(_ViewModel)
            Else
                '更新処理
                MyBase.BaseEdit(_ViewModel)
            End If

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                '登録処理
                If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then

                    _ViewModel.States = StatusType.SuccessCreate

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CONST_NO) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CONST_NO).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE), _ViewModel.MNG_LOC_L_CD)

                    '更新処理
                Else
                    Try
                        '予約を更新
                        Dim updReseult = Dao.ToolConstruction.AddAssetReserve(_db, _ViewModel, Me._LoginInfo, Me._Message)
                        If Not updReseult = "ok" Then
                            _ViewModel.ErrCode = updReseult
                            _ViewModel.ErrTarget = "Target_"

                            _tran.Rollback()
                        End If
                    Catch ex As Exception
                        _tran.Rollback()
                        logger.Error(ex.Message)
                        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                    End Try

                    _ViewModel.States = StatusType.SuccessEdit

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                 Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                 Me._Message.getMessage(CommonLabel.LABEL_CONST_NO) & ": " &
                                                 CommonRfid.ToStringNvl(_ViewModel.CONST_NO).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE), _ViewModel.MNG_LOC_L_CD)
                End If
            Else
                If autoConstNo Then
                    _ViewModel.CONST_NO = "_"
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
            CopyViewModeltoModel(_ViewModel)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeCreate()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeEdit()

            '親の初期処理
            MyBase.InitializeCreate()

            'モデルを登録用モデルにコピー
            CopyViewModeltoModel(_ViewModel)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeEdit()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            '工事番号が登録済みかチェックする
            Dim model As ConstructionListModel = db.ConstructionList.Find(_Model.CONST_NO)

            '重複エラーの場合、エラー情報を保持する
            If Not IsNothing(model) Then
                ModelState.AddModelError("CONST_NO", CommonLabel.MSG_ERR_DATAJUUFUKU)
                blnReturn = False
            End If

            '工事開始日、工事終了日 1000年以上かチェック
            If _ViewModel.CONST_START_DT < MIN_DATE Then
                ModelState.AddModelError("ConstStartDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If _ViewModel.CONST_END_DT < MIN_DATE Then
                ModelState.AddModelError("ConstEndDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            '工事貸出開始日、工事貸出終了日 1000年以上かチェック
            If _ViewModel.CONST_LENT_START_DT < MIN_DATE Then
                ModelState.AddModelError("ConstLentStartDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If _ViewModel.CONST_LENT_END_DT < MIN_DATE Then
                ModelState.AddModelError("ConstLentEndDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If Not ModelState.IsValid Then
                blnReturn = False  'エラーがあった場合、Falseを返す
            End If

            Return blnReturn

        End Function

        ''' <summary>
        ''' 登録処理をコンテキストに追加します
        ''' </summary>
        Protected Overrides Sub CreateQuery()

            '登録日時・登録社員コード・登録社員名・登録プロセスを登録
            _Model.INS_DATETIME = CommonRfid.getNowDate
            _Model.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelConstructionList.TITLE)

            '処理日時を登録
            _Model.TASK_TS = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))

            '工事マスタの更新
            _db.ConstructionList.Add(_Model)

            '工事履歴テーブル登録
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params = New List(Of SqlParameter)

            sbSQL.AppendLine("INSERT INTO T_CONST_LOG")
            sbSQL.AppendLine("(")
            sbSQL.AppendLine("     CONST_NO")
            sbSQL.AppendLine("    ,CONST_WORK_TS")
            sbSQL.AppendLine("    ,CONST_WORK_DIV")
            sbSQL.AppendLine("    ,CONST_MATCH_DIV")
            sbSQL.AppendLine("    ,LENT_NUM")
            sbSQL.AppendLine("    ,LEASE_NUM")
            sbSQL.AppendLine("    ,TEMP_REG_NUM")
            sbSQL.AppendLine("    ,CONST_IN_MATCH_NUM")
            sbSQL.AppendLine("    ,CONST_OUT_MATCH_NUM")
            sbSQL.AppendLine("    ,CONST_IN_NUM")
            sbSQL.AppendLine("    ,TEMP_NUM")
            sbSQL.AppendLine("    ,CONST_OUT_NUM")
            sbSQL.AppendLine("    ,INS_DATETIME")
            sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,UPD_DATETIME")
            sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,UPD_TASK")
            sbSQL.AppendLine(") VALUES (")
            sbSQL.AppendLine("     @CONST_NO")
            sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
            sbSQL.AppendLine("    ,'" + ConstWorkDiv.ConstRegist + "'")
            sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,@INS_DATETIME")
            sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,@UPD_DATETIME")
            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,@UPD_TASK")
            sbSQL.AppendLine(")")

            params.Add(New SqlParameter($"@CONST_NO", _Model.CONST_NO))
            params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

            _db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            '工事開始日、工事終了日 1000年以上かチェック
            If _ViewModel.CONST_START_DT < MIN_DATE Then
                ModelState.AddModelError("ConstStartDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If _ViewModel.CONST_END_DT < MIN_DATE Then
                ModelState.AddModelError("ConstEndDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            '工事貸出開始日、工事貸出終了日 1000年以上かチェック
            If _ViewModel.CONST_LENT_START_DT < MIN_DATE Then
                ModelState.AddModelError("ConstLentStartDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If _ViewModel.CONST_LENT_END_DT < MIN_DATE Then
                ModelState.AddModelError("ConstLentEndDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelConstructionList.TITLE)

            '処理日時を登録
            _Model.TASK_TS = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))

            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified

            '工事履歴テーブル登録
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params = New List(Of SqlParameter)

            sbSQL.AppendLine("INSERT INTO T_CONST_LOG")
            sbSQL.AppendLine("(")
            sbSQL.AppendLine("     CONST_NO")
            sbSQL.AppendLine("    ,CONST_WORK_TS")
            sbSQL.AppendLine("    ,CONST_WORK_DIV")
            sbSQL.AppendLine("    ,CONST_MATCH_DIV")
            sbSQL.AppendLine("    ,LENT_NUM")
            sbSQL.AppendLine("    ,LEASE_NUM")
            sbSQL.AppendLine("    ,TEMP_REG_NUM")
            sbSQL.AppendLine("    ,CONST_IN_MATCH_NUM")
            sbSQL.AppendLine("    ,CONST_OUT_MATCH_NUM")
            sbSQL.AppendLine("    ,CONST_IN_NUM")
            sbSQL.AppendLine("    ,TEMP_NUM")
            sbSQL.AppendLine("    ,CONST_OUT_NUM")
            sbSQL.AppendLine("    ,INS_DATETIME")
            sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,UPD_DATETIME")
            sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,UPD_TASK")
            sbSQL.AppendLine(")")
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("     CONST.CONST_NO")
            sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
            sbSQL.AppendLine("    ,'" + ConstWorkDiv.ConstRegist + "'")
            sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,ISNULL(LOG.CONST_IN_NUM,0)")
            sbSQL.AppendLine("    ,ISNULL(LOG.TEMP_NUM, 0)")
            sbSQL.AppendLine("    ,ISNULL(LOG.CONST_OUT_NUM,0)")
            sbSQL.AppendLine("    ,@INS_DATETIME")
            sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,@UPD_DATETIME")
            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,@UPD_TASK")
            sbSQL.AppendLine("FROM M_CONSTRUCTION CONST")

            sbSQL.AppendLine("OUTER APPLY (SELECT TOP 1 * FROM T_CONST_LOG WHERE CONST_NO = CONST.CONST_NO ORDER BY UPD_DATETIME DESC) LOG")

            sbSQL.AppendLine("WHERE CONST.CONST_NO = @CONST_NO")

            params.Add(New SqlParameter($"@CONST_NO", _Model.CONST_NO))
            params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

            _db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

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

            'IDをキーにモデルを取得する
            Dim model As ConstructionListModel = db.ConstructionList.Find(ID)
            model.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")

            '工事開始日をyyyy/MM/ddで表示する
            If model.CONST_START_DT.ToString.Length <> itemlength.ConstStartDt Then
                model.ConstStartDtString = String.Empty
            Else
                Dim sConstStartDt As String = String.Empty
                sConstStartDt = CommonRfid.ToStringIntYMD(model.CONST_START_DT)
                model.ConstStartDtString = CommonRfid.ToHaifunYMD(sConstStartDt)
            End If

            '工事終了日をyyyy/MM/ddで表示する
            If model.CONST_END_DT.ToString.Length <> itemlength.ConstEndDt Then
                model.ConstEndDtString = String.Empty
            Else
                Dim sConstEndDt As String = String.Empty
                sConstEndDt = CommonRfid.ToStringIntYMD(model.CONST_END_DT)
                model.ConstEndDtString = CommonRfid.ToHaifunYMD(sConstEndDt)
            End If

            '工事貸出開始日をyyyy/MM/ddで表示する
            If model.CONST_LENT_START_DT.ToString.Length <> itemlength.ConstLentStartDt Then
                model.ConstLentStartDtString = String.Empty
            Else
                Dim sConstLentStartDt As String = String.Empty
                sConstLentStartDt = CommonRfid.ToStringIntYMD(model.CONST_LENT_START_DT)
                model.ConstLentStartDtString = CommonRfid.ToHaifunYMD(sConstLentStartDt)
                model.BeforeConstLentStartDtString = CommonRfid.ToHaifunYMD(sConstLentStartDt)
            End If

            '工事貸出終了日をyyyy/MM/ddで表示する
            If model.CONST_LENT_END_DT.ToString.Length <> itemlength.ConstLentEndDt Then
                model.ConstLentEndDtString = String.Empty
            Else
                Dim sConstLentEndDt As String = String.Empty
                sConstLentEndDt = CommonRfid.ToStringIntYMD(model.CONST_LENT_END_DT)
                model.ConstLentEndDtString = CommonRfid.ToHaifunYMD(sConstLentEndDt)
                model.BeforeConstLentEndDtString = CommonRfid.ToHaifunYMD(sConstLentEndDt)
            End If

            '工事情報1～20のFrom、Toを設定
            If Not String.IsNullOrEmpty(model.CONST_INFO1) Then
                Dim index As Integer = model.CONST_INFO1.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    ' セパレータ以前の部分を取得
                    model.CONST_INFO1_FROM = model.CONST_INFO1.Substring(0, index)
                    model.CONST_INFO1_TO = model.CONST_INFO1.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO1_FROM = model.CONST_INFO1
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO2) Then
                Dim index As Integer = model.CONST_INFO2.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO2_FROM = model.CONST_INFO2.Substring(0, index)
                    model.CONST_INFO2_TO = model.CONST_INFO2.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO2_FROM = model.CONST_INFO2
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO3) Then
                Dim index As Integer = model.CONST_INFO3.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO3_FROM = model.CONST_INFO3.Substring(0, index)
                    model.CONST_INFO3_TO = model.CONST_INFO3.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO3_FROM = model.CONST_INFO3
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO4) Then
                Dim index As Integer = model.CONST_INFO4.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO4_FROM = model.CONST_INFO4.Substring(0, index)
                    model.CONST_INFO4_TO = model.CONST_INFO4.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO4_FROM = model.CONST_INFO4
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO5) Then
                Dim index As Integer = model.CONST_INFO5.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO5_FROM = model.CONST_INFO5.Substring(0, index)
                    model.CONST_INFO5_TO = model.CONST_INFO5.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO5_FROM = model.CONST_INFO5
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO6) Then
                Dim index As Integer = model.CONST_INFO6.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO6_FROM = model.CONST_INFO6.Substring(0, index)
                    model.CONST_INFO6_TO = model.CONST_INFO6.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO6_FROM = model.CONST_INFO6
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO7) Then
                Dim index As Integer = model.CONST_INFO7.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO7_FROM = model.CONST_INFO7.Substring(0, index)
                    model.CONST_INFO7_TO = model.CONST_INFO7.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO7_FROM = model.CONST_INFO7
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO8) Then
                Dim index As Integer = model.CONST_INFO8.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO8_FROM = model.CONST_INFO8.Substring(0, index)
                    model.CONST_INFO8_TO = model.CONST_INFO8.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO8_FROM = model.CONST_INFO8
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO9) Then
                Dim index As Integer = model.CONST_INFO9.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO9_FROM = model.CONST_INFO9.Substring(0, index)
                    model.CONST_INFO9_TO = model.CONST_INFO9.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO9_FROM = model.CONST_INFO9
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO10) Then
                Dim index As Integer = model.CONST_INFO10.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO10_FROM = model.CONST_INFO10.Substring(0, index)
                    model.CONST_INFO10_TO = model.CONST_INFO10.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO10_FROM = model.CONST_INFO10
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO11) Then
                Dim index As Integer = model.CONST_INFO11.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO11_FROM = model.CONST_INFO11.Substring(0, index)
                    model.CONST_INFO11_TO = model.CONST_INFO11.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO11_FROM = model.CONST_INFO11
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO12) Then
                Dim index As Integer = model.CONST_INFO12.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO12_FROM = model.CONST_INFO12.Substring(0, index)
                    model.CONST_INFO12_TO = model.CONST_INFO12.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO12_FROM = model.CONST_INFO12
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO13) Then
                Dim index As Integer = model.CONST_INFO13.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO13_FROM = model.CONST_INFO13.Substring(0, index)
                    model.CONST_INFO13_TO = model.CONST_INFO13.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO13_FROM = model.CONST_INFO13
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO14) Then
                Dim index As Integer = model.CONST_INFO14.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO14_FROM = model.CONST_INFO14.Substring(0, index)
                    model.CONST_INFO14_TO = model.CONST_INFO14.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO14_FROM = model.CONST_INFO14
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO15) Then
                Dim index As Integer = model.CONST_INFO15.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO15_FROM = model.CONST_INFO15.Substring(0, index)
                    model.CONST_INFO15_TO = model.CONST_INFO15.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO15_FROM = model.CONST_INFO15
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO16) Then
                Dim index As Integer = model.CONST_INFO16.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO16_FROM = model.CONST_INFO16.Substring(0, index)
                    model.CONST_INFO16_TO = model.CONST_INFO16.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO16_FROM = model.CONST_INFO16
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO17) Then
                Dim index As Integer = model.CONST_INFO17.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO17_FROM = model.CONST_INFO17.Substring(0, index)
                    model.CONST_INFO17_TO = model.CONST_INFO17.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO17_FROM = model.CONST_INFO17
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO18) Then
                Dim index As Integer = model.CONST_INFO18.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO18_FROM = model.CONST_INFO18.Substring(0, index)
                    model.CONST_INFO18_TO = model.CONST_INFO18.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO18_FROM = model.CONST_INFO18
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO19) Then
                Dim index As Integer = model.CONST_INFO19.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO19_FROM = model.CONST_INFO19.Substring(0, index)
                    model.CONST_INFO19_TO = model.CONST_INFO19.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO19_FROM = model.CONST_INFO19
                End If
            End If
            If Not String.IsNullOrEmpty(model.CONST_INFO20) Then
                Dim index As Integer = model.CONST_INFO20.LastIndexOf(DELIMITER)
                If index <> -1 Then
                    model.CONST_INFO20_FROM = model.CONST_INFO20.Substring(0, index)
                    model.CONST_INFO20_TO = model.CONST_INFO20.Substring(index + DELIMITER.Length)
                Else
                    model.CONST_INFO20_FROM = model.CONST_INFO20
                End If
            End If

            Return Json(model)

        End Function

#End Region

#Region "削除処理"
        ''' <summary>
        ''' アクションメソッド(Delete)
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Overridable Function Delete(ByVal ViewModel As ConstructionListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '削除処理

            MyBase.BaseDelete(_ViewModel)

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                _ViewModel.States = StatusType.SuccessDelete

                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '操作ログ出力
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                 Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                 Me._Message.getMessage(CommonLabel.LABEL_CONST_NO) & ": " &
                                                 CommonRfid.ToStringNvl(_ViewModel.CONST_NO).Trim
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE), _ViewModel.MNG_LOC_L_CD)
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

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        ''' <param name="model">基底モデル</param>
        Protected Overrides Sub DisposeDelete(ByRef model As BaseModel)

            MyBase.DisposeDelete(model)

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST) = _ViewModel

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
            '工事マスタ削除
            Dim constructionList As ConstructionListModel = _db.ConstructionList.Find(_Model.CONST_NO)

            '削除を実行する
            _db.ConstructionList.Remove(constructionList)

            '工事資産削除
            Dim constAssetList As List(Of ConstAssetListModel) = _db.ConstAssetList.Where(Function(x) x.CONST_NO = _Model.CONST_NO).ToList

            If constAssetList.Count > 0 Then
                '削除を実行する
                _db.ConstAssetList.RemoveRange(constAssetList)
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params = New List(Of SqlParameter)

            '予約テーブルデータ取得
            Dim reserveList As List(Of LendingEntryModel) = _db.LendingEntry.Where(Function(x) x.REMARK = _Model.CONST_NO).ToList

            If reserveList.Count > 0 Then

                '予約削除
                sbSQL.Clear()
                params = New List(Of SqlParameter)

                '削除処理
                sbSQL.AppendLine("DELETE FROM T_RESERVE")
                sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                params.Add(New SqlParameter($"@RESERVE_ID", reserveList.First.RESERVE_ID))

                _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                '予約明細削除
                sbSQL.Clear()
                params = New List(Of SqlParameter)

                '削除処理
                sbSQL.AppendLine("DELETE FROM T_RESERVE_DETAIL")
                sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                params.Add(New SqlParameter($"@RESERVE_ID", reserveList.First.RESERVE_ID))

                _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                '予約メンテナンス削除
                sbSQL.Clear()
                params = New List(Of SqlParameter)

                '削除処理
                sbSQL.AppendLine("DELETE FROM T_RESERVE_MAINTE")
                sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                params.Add(New SqlParameter($"@RESERVE_ID", reserveList.First.RESERVE_ID))

                _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
            End If

            '仮置きテーブル更新
            sbSQL.Clear()
            params = New List(Of SqlParameter)

            sbSQL.AppendLine("UPDATE T_TEMPORARY SET")
            sbSQL.AppendLine("    REL_CONST_NO = NULL")
            sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
            sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
            sbSQL.AppendLine("WHERE REL_CONST_NO = @REL_CONST_NO")

            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))
            params.Add(New SqlParameter($"@REL_CONST_NO", _Model.CONST_NO))

            _db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)
        End Sub
#End Region

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
            Dim viewModel As New ConstructionListViewModel
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

#Region "工事完了処理"
        ''' <summary>
        ''' 工事完了処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function ConstComplete(ViewModel As ConstructionListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            '正常時
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '最新のチェック状態でセッション情報を更新する(追加および削除) (キー：工事番号)
                Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

                Dim db As New SystemDBContext
                Dim sbSQL As StringBuilder = New StringBuilder
                Dim params As List(Of SqlParameter)
                Dim ret As Integer

                'トランザクションスタート
                Using tran = db.Database.BeginTransaction()
                    Try
                        For Each str As String In lstSelectKeys
                            Dim selectKey As String = str.Split(",")(ConstructionListKey.CONST_NO)

                            '工事マスタ更新
                            sbSQL.Clear()
                            params = New List(Of SqlParameter)

                            sbSQL.AppendLine("UPDATE M_CONSTRUCTION SET")
                            sbSQL.AppendLine("    CONST_STATUS = @CONST_STATUS")
                            sbSQL.AppendLine("  , CONST_CONFIRM_TS = FORMAT(GetDate(), 'yyyyMMddHHmmss')")
                            sbSQL.AppendLine("  , CONST_CONFIRM_EMPLOYEE_CD = @CONST_CONFIRM_EMPLOYEE_CD")
                            sbSQL.AppendLine("  , CONST_CONFIRM_EMPLOYEE_NAME = @CONST_CONFIRM_EMPLOYEE_NAME")
                            sbSQL.AppendLine("  , CONST_CONFIRM_REMARK = @CONST_CONFIRM_REMARK")
                            sbSQL.AppendLine("  , UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                            sbSQL.AppendLine("  , UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                            sbSQL.AppendLine("  , UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                            sbSQL.AppendLine("  , UPD_TASK = @UPD_TASK")
                            sbSQL.AppendLine(" WHERE CONST_NO = @CONST_NO")

                            params.Add(New SqlParameter($"@CONST_STATUS", CONST_STATUS.Complete))
                            params.Add(New SqlParameter($"@CONST_CONFIRM_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            params.Add(New SqlParameter($"@CONST_CONFIRM_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                            params.Add(New SqlParameter($"@CONST_CONFIRM_REMARK", _ViewModel.CONST_CONFIRM_REMARK))
                            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                            params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.BUTTON_CONST_COMPLETE)))
                            params.Add(New SqlParameter($"@CONST_NO", selectKey))

                            ret = db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                            '工事履歴登録
                            sbSQL.Clear()
                            params = New List(Of SqlParameter)

                            sbSQL.AppendLine("INSERT INTO T_CONST_LOG")
                            sbSQL.AppendLine("(")
                            sbSQL.AppendLine("     CONST_NO")
                            sbSQL.AppendLine("    ,CONST_WORK_TS")
                            sbSQL.AppendLine("    ,CONST_WORK_DIV")
                            sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                            sbSQL.AppendLine("    ,LENT_NUM")
                            sbSQL.AppendLine("    ,LEASE_NUM")
                            sbSQL.AppendLine("    ,TEMP_REG_NUM")
                            sbSQL.AppendLine("    ,CONST_IN_MATCH_NUM")
                            sbSQL.AppendLine("    ,CONST_OUT_MATCH_NUM")
                            sbSQL.AppendLine("    ,CONST_IN_NUM")
                            sbSQL.AppendLine("    ,TEMP_NUM")
                            sbSQL.AppendLine("    ,CONST_OUT_NUM")
                            sbSQL.AppendLine("    ,INS_DATETIME")
                            sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,UPD_DATETIME")
                            sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,UPD_TASK")
                            sbSQL.AppendLine(")")
                            sbSQL.AppendLine("SELECT")
                            sbSQL.AppendLine("     CONST.CONST_NO")
                            sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                            sbSQL.AppendLine("    ,'" + ConstWorkDiv.ConstComplete + "'")
                            sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                            sbSQL.AppendLine("    ,ISNULL(LENT_NUM, 0)")
                            sbSQL.AppendLine("    ,ISNULL(LEASE_NUM, 0)")
                            sbSQL.AppendLine("    ,ISNULL(TEMP_REG_NUM, 0)")
                            sbSQL.AppendLine("    ,ISNULL(CONST_IN_MATCH_NUM, 0)")
                            sbSQL.AppendLine("    ,ISNULL(CONST_OUT_MATCH_NUM, 0)")
                            sbSQL.AppendLine("    ,ISNULL(LOG.CONST_IN_NUM ,0)")
                            sbSQL.AppendLine("    ,ISNULL(LOG.TEMP_NUM, 0)")
                            sbSQL.AppendLine("    ,ISNULL(LOG.CONST_OUT_NUM ,0)")
                            sbSQL.AppendLine("    ,@INS_DATETIME")
                            sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,@UPD_DATETIME")
                            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,@UPD_TASK")
                            sbSQL.AppendLine("FROM M_CONSTRUCTION CONST")

                            sbSQL.AppendLine("OUTER APPLY (SELECT TOP 1 * FROM T_CONST_LOG WHERE CONST_NO = CONST.CONST_NO ORDER BY UPD_DATETIME DESC) LOG")

                            sbSQL.AppendLine("WHERE CONST.CONST_NO = @CONST_NO")

                            params.Add(New SqlParameter($"@CONST_NO", selectKey))
                            params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                            params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                            params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

                            db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                            '工事資産履歴登録
                            sbSQL.Clear()
                            params = New List(Of SqlParameter)

                            sbSQL.AppendLine("INSERT INTO T_CONST_ASSET_LOG")
                            sbSQL.AppendLine("(")
                            sbSQL.AppendLine("     CONST_NO")
                            sbSQL.AppendLine("    ,CONST_WORK_TS")
                            sbSQL.AppendLine("    ,ASSET_NO")
                            sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                            sbSQL.AppendLine("    ,CONST_MATCH_STATUS")
                            sbSQL.AppendLine("    ,TEMP_FLAG")
                            sbSQL.AppendLine("    ,TEMP_ID")
                            sbSQL.AppendLine("    ,INS_DATETIME")
                            sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,UPD_DATETIME")
                            sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,UPD_TASK")
                            sbSQL.AppendLine(")")
                            sbSQL.AppendLine("SELECT")
                            sbSQL.AppendLine("     @CONST_NO")
                            sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                            sbSQL.AppendLine("    ,CASSET.ASSET_NO")
                            sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                            sbSQL.AppendLine("    ,CASSET.CONST_ASSET_STATUS")
                            sbSQL.AppendLine("    ,IIF(TEMP.REG_CONST_NO IS NOT NULL AND TEMP.REL_CONST_NO IS NULL, '" + TempFlg.Cd.Yes + "', '" + TempFlg.Cd.No + "')")
                            sbSQL.AppendLine("    ,CASSET.TEMP_ID")
                            sbSQL.AppendLine("    ,@INS_DATETIME")
                            sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,@UPD_DATETIME")
                            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,@UPD_TASK")
                            sbSQL.AppendLine("FROM T_CONST_ASSET CASSET")
                            sbSQL.AppendLine("LEFT JOIN T_TEMPORARY TEMP ")
                            sbSQL.AppendLine("ON TEMP.REG_CONST_NO = CASSET.CONST_NO ")

                            sbSQL.AppendLine("WHERE  CASSET.CONST_NO = @CONST_NO")

                            params.Add(New SqlParameter($"@CONST_NO", selectKey))
                            params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                            params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                            params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))

                            db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                        Next

                        tran.Commit()

                        '操作ログ出力
                        Dim sProcessContents As String = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                         Me._Message.getMessage(CommonLabelConstructionList.BUTTON_CONST_COMPLETE) & " " &
                                                         Me._Message.getMessage(CommonLabelConstructionList.MSG_SUCCESS_CONST_COMPLETE) & " " &
                                                         Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                         lstSelectKeys.Count.ToString
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE_CONST_COMPLETE))

                        _ViewModel.States = StatusType.SuccessComplete

                    Catch ex As Exception
                        tran.Rollback()

                        logger.Error(ex.Message)
                        logger.Error(sbSQL)

                        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                    End Try
                End Using
            End If

            DisposeCreate()

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "CSVデータ取込処理"
        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="CsvFile">登録するCSVファイル</param>
        ''' <returns>View表示内容</returns>
        Public Function ImportCsvFile(ByVal CsvFile As HttpPostedFileBase) As ActionResult
            Me._lstErr = New List(Of DataInportResult)
            Dim errRow As DataInportResult = New DataInportResult
            Me._lstErr.Add(errRow)
            Dim sCsvPath As String = String.Empty   'CSVアップロードフルパス

            Dim sProcessContents As String          '操作ログ出力

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.LoginSession
                '結果表示
                Return Json(Me._lstErr)
            End If

            ''●●●
            'logger.Info("STEP1 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))


            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'CSVマッピング明細マスタ取得
            Dim tblCsvMapMei As DataTable = GetCsvMapDetail()
            '取得したCSVマッピング明細マスタの項目数、項目のチェック
            If Not IsCsvMapMeiKoumokuOK(tblCsvMapMei) Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.CsvMapMei
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_CSVMAPMEI) & " " &
                                                     Path.GetFileName(CsvFile.FileName)
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE))

                Return Json(Me._lstErr)
            End If

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
                sProcessContents = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE))

                Return Json(Me._lstErr)
            End If

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVIMPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'CSVファイルをアップロード
            sCsvPath = Path.Combine(sFullPath, CsvFile.FileName)
            CsvFile.SaveAs(sCsvPath)

            ''●●●
            'logger.Info("STEP2 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'CSVマッピングマスタの文字コード取得
            Dim sCharCd As String = GetCsvMap()

            'CSVマッピングマスタの文字コードがutf-8・shift_jis以外の場合、エラー
            If Not sCharCd = CommonSettingKey.CsvMapUtf.Key1 AndAlso Not sCharCd = CommonSettingKey.CsvMapShiftJis.Key1 Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.CsvMapCharCd
                '結果表示
                Return Json(Me._lstErr)
            End If

            '設定マスタから文字コード情報を取得
            Dim data As SettingInfo = New SettingInfo
            Dim skeyDiv As String = String.Empty    'エラーメッセージ用
            If sCharCd = CommonSettingKey.CsvMapUtf.Key1 Then
                skeyDiv = CommonSettingKey.CsvMapUtf.KeyDiv
                data = Me._Message.getSettingInfo(CommonSettingKey.CsvMapUtf.KeyDiv, CommonSettingKey.CsvMapUtf.Key1, CommonSettingKey.CsvMapUtf.Key2, CommonSettingKey.CsvMapUtf.Key3)
            ElseIf sCharCd = CommonSettingKey.CsvMapShiftJis.Key1 Then
                skeyDiv = CommonSettingKey.CsvMapShiftJis.KeyDiv
                data = Me._Message.getSettingInfo(CommonSettingKey.CsvMapShiftJis.KeyDiv, CommonSettingKey.CsvMapShiftJis.Key1, CommonSettingKey.CsvMapShiftJis.Key2, CommonSettingKey.CsvMapShiftJis.Key3)
            End If
            '設定マスタからデータを取得できなかった場合、エラー
            If data Is Nothing Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.SettingInfo
                Me._lstErr(0).ErrGaiyou = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), skeyDiv & " " & sCharCd)
                '結果表示
                Return Json(Me._lstErr)
            End If

            '--登録処理開始
            Dim db As New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try
                    'CSVデータ取得 & データ登録
                    Dim blErr As Boolean = False    'CSVファイルにエラーがあるかどうか
                    Dim isKomokusuuErr As Boolean = False   '項目数エラーかどうか

                    ''●●●
                    'logger.Info("STEP3 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

                    '-----データチェック & 一時表に書き込み-----
                    Dim sTmpTblNm = "#TCONSTRUCTION" & Now.ToString("HHmmss") & (New Random).ToString("000")       '作成する一時表の名称

                    '一時表作成
                    Dim sCreate As String = String.Format("Select * INTO {0} FROM M_CONSTRUCTION WHERE 1 = 0", sTmpTblNm)
                    db.Database.ExecuteSqlCommand(sCreate)

                    Dim sAddColumn As String
                    '一時表に、列「UPD_FLG」を追加する　(0:変更なし 1:変更あり  ※新規登録行はこの区分を使用しない)
                    sAddColumn = String.Format("ALTER TABLE {0} ADD UPD_FLG Bit DEFAULT 0", sTmpTblNm)
                    db.Database.ExecuteSqlCommand(sAddColumn)

                    '一時表に、列「ACTIVITY」を追加する
                    sAddColumn = String.Format("ALTER TABLE {0} ADD ACTIVITY CHAR DEFAULT NULL", sTmpTblNm)
                    db.Database.ExecuteSqlCommand(sAddColumn)

                    '一時表に、列「CSV_ROW_NUM」を追加する
                    sAddColumn = String.Format("ALTER TABLE {0} ADD CSV_ROW_NUM INT DEFAULT 0", sTmpTblNm)
                    db.Database.ExecuteSqlCommand(sAddColumn)

                    blErr = ReadCsvFile(sCsvPath, Encoding.GetEncoding(CInt(data.DATA2)), isKomokusuuErr, tblCsvMapMei, sTmpTblNm, db)

                    ''●●●
                    'logger.Info("STEP4 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

                    '項目数エラーの場合
                    If isKomokusuuErr Then
                        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
                        '操作ログ出力
                        sProcessContents = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                             Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " &
                                                             Path.GetFileName(CsvFile.FileName)
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE))

                        'ロールバック
                        tran.Rollback()

                        Return Json(Me._lstErr)
                    End If

                    'エラーがあった場合
                    If blErr Then
                        '操作ログ出力
                        sProcessContents = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                             Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " &
                                                             Path.GetFileName(CsvFile.FileName) & " " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                                             Me._lstErr.Count.ToString
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE))

                        'ロールバック
                        tran.Rollback()

                        Return Json(Me._lstErr)
                    End If

                    ''●●●
                    'logger.Info("STEP5 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

                    '権限情報の取得
                    Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstructionList, _LoginInfo)
                    Dim clsDB As clsSV_DB = New clsSV_DB
                    If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                        '接続失敗
                        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
                    End If

                    '添付ファイルフォルダの削除情報を取得する
                    sbSQL.Clear()
                    Dim DeleteConstNoList As New List(Of MstConstructionModel)

                    sbSQL.AppendLine("SELECT M_CONSTRUCTION.CONST_NO FROM M_CONSTRUCTION")
                    sbSQL.AppendLine("WHERE")
                    sbSQL.AppendLine("  EXISTS")
                    sbSQL.AppendLine("  (")
                    sbSQL.AppendLine("      SELECT CONST_NO ")
                    sbSQL.AppendLine("      FROM " & sTmpTblNm & " WK")
                    sbSQL.AppendLine("      WHERE WK.CONST_NO = M_CONSTRUCTION.CONST_NO")
                    sbSQL.AppendLine("      AND WK.ACTIVITY = '" & CsvImportProcDiv.Delete & "'")  '処理区分が「D:削除」
                    sbSQL.AppendLine("  )")

                    If Not String.IsNullOrEmpty(sbSQL.ToString) Then
                        DeleteConstNoList = CType(db.Database.SqlQuery(Of MstConstructionModel)(sbSQL.ToString).ToList, List(Of MstConstructionModel))
                    End If

                    ''●●●
                    'logger.Info("STEP6 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    '削除処理
                    sbSQL.AppendLine("DELETE FROM M_CONSTRUCTION")
                    sbSQL.AppendLine("WHERE")
                    sbSQL.AppendLine("  EXISTS")
                    sbSQL.AppendLine("  (")
                    sbSQL.AppendLine("      SELECT CONST_NO ")
                    sbSQL.AppendLine("      FROM " & sTmpTblNm & " WK")
                    sbSQL.AppendLine("      WHERE WK.CONST_NO = M_CONSTRUCTION.CONST_NO")
                    sbSQL.AppendLine("      AND WK.ACTIVITY = '" & CsvImportProcDiv.Delete & "'")  '処理区分が「D:削除」
                    sbSQL.AppendLine("  )")

                    db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                    For Each deleteConst In DeleteConstNoList
                        '工事資産削除
                        Dim constAssetList As List(Of ConstAssetListModel) = db.ConstAssetList.Where(Function(x) x.CONST_NO = deleteConst.CONST_NO).ToList

                        If constAssetList.Count > 0 Then
                            '削除を実行する
                            db.ConstAssetList.RemoveRange(constAssetList)
                            db.SaveChanges()
                        End If

                        '予約テーブルデータ取得
                        Dim reserveList As List(Of LendingEntryModel) = db.LendingEntry.Where(Function(x) x.REMARK = deleteConst.CONST_NO).ToList

                        If reserveList.Count > 0 Then

                            '予約削除
                            sbSQL.Clear()
                            params = New List(Of SqlParameter)

                            '削除処理
                            sbSQL.AppendLine("DELETE FROM T_RESERVE")
                            sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                            params.Add(New SqlParameter($"@RESERVE_ID", reserveList.First.RESERVE_ID))

                            db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                            '予約明細削除
                            sbSQL.Clear()
                            params = New List(Of SqlParameter)

                            '削除処理
                            sbSQL.AppendLine("DELETE FROM T_RESERVE_DETAIL")
                            sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                            params.Add(New SqlParameter($"@RESERVE_ID", reserveList.First.RESERVE_ID))

                            db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                            '予約メンテナンス削除
                            sbSQL.Clear()
                            params = New List(Of SqlParameter)

                            '削除処理
                            sbSQL.AppendLine("DELETE FROM T_RESERVE_MAINTE")
                            sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                            params.Add(New SqlParameter($"@RESERVE_ID", reserveList.First.RESERVE_ID))

                            db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
                        End If

                        '仮置きテーブル更新
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("UPDATE T_TEMPORARY SET")
                        sbSQL.AppendLine("    REL_CONST_NO = NULL")
                        sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                        sbSQL.AppendLine("WHERE REL_CONST_NO = @REL_CONST_NO")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))
                        params.Add(New SqlParameter($"@REL_CONST_NO", deleteConst.CONST_NO))

                        _db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)
                    Next


                    ''●●●
                    'logger.Info("STEP7 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

                    '--先に工事履歴ファイルを更新する
                    '挿入分
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("INSERT INTO T_CONST_LOG")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine("     CONST_NO")
                    sbSQL.AppendLine("    ,CONST_WORK_TS")
                    sbSQL.AppendLine("    ,CONST_WORK_DIV")
                    sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                    sbSQL.AppendLine("    ,LENT_NUM")
                    sbSQL.AppendLine("    ,LEASE_NUM")
                    sbSQL.AppendLine("    ,TEMP_REG_NUM")
                    sbSQL.AppendLine("    ,CONST_IN_MATCH_NUM")
                    sbSQL.AppendLine("    ,CONST_OUT_MATCH_NUM")
                    sbSQL.AppendLine("    ,CONST_IN_NUM")
                    sbSQL.AppendLine("    ,TEMP_NUM")
                    sbSQL.AppendLine("    ,CONST_OUT_NUM")
                    sbSQL.AppendLine("    ,INS_DATETIME")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_DATETIME")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK")
                    sbSQL.AppendLine(")")
                    sbSQL.AppendLine("SELECT")
                    sbSQL.AppendLine("     CONST.CONST_NO")
                    sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                    sbSQL.AppendLine("    ,'" + ConstWorkDiv.ConstRegist + "'")
                    sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,@INS_DATETIME")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_DATETIME")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_TASK")
                    sbSQL.AppendLine("FROM " & sTmpTblNm & " CONST")
                    sbSQL.AppendLine("WHERE")
                    sbSQL.AppendLine("   CONST.ACTIVITY = '" & CsvImportProcDiv.Insert & "'")  '処理区分が「I:追加」

                    params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)


                    ''●●●
                    'logger.Info("STEP8 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

                    '--先に資産処理履歴ファイルを更新する
                    '更新分
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("INSERT INTO T_CONST_LOG")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine("     CONST_NO")
                    sbSQL.AppendLine("    ,CONST_WORK_TS")
                    sbSQL.AppendLine("    ,CONST_WORK_DIV")
                    sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                    sbSQL.AppendLine("    ,LENT_NUM")
                    sbSQL.AppendLine("    ,LEASE_NUM")
                    sbSQL.AppendLine("    ,TEMP_REG_NUM")
                    sbSQL.AppendLine("    ,CONST_IN_MATCH_NUM")
                    sbSQL.AppendLine("    ,CONST_OUT_MATCH_NUM")
                    sbSQL.AppendLine("    ,CONST_IN_NUM")
                    sbSQL.AppendLine("    ,TEMP_NUM")
                    sbSQL.AppendLine("    ,CONST_OUT_NUM")
                    sbSQL.AppendLine("    ,INS_DATETIME")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_DATETIME")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK")
                    sbSQL.AppendLine(")")
                    sbSQL.AppendLine("SELECT")
                    sbSQL.AppendLine("     CONST.CONST_NO")
                    sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                    sbSQL.AppendLine("    ,'" + ConstWorkDiv.ConstRegist + "'")
                    sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,ISNULL(LOG.CONST_IN_NUM, 0)")
                    sbSQL.AppendLine("    ,ISNULL(LOG.TEMP_NUM, 0)")
                    sbSQL.AppendLine("    ,ISNULL(LOG.CONST_OUT_NUM, 0)")
                    sbSQL.AppendLine("    ,@INS_DATETIME")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_DATETIME")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_TASK")
                    sbSQL.AppendLine("FROM " & sTmpTblNm & " CONST")

                    sbSQL.AppendLine("OUTER APPLY (SELECT TOP 1 * FROM T_CONST_LOG WHERE CONST_NO = CONST.CONST_NO ORDER BY UPD_DATETIME DESC) LOG")

                    sbSQL.AppendLine("WHERE  CONST.UPD_FLG <> 0")
                    sbSQL.AppendLine("AND CONST.ACTIVITY = '" & CsvImportProcDiv.Update & "'")  '処理区分が「U:更新」

                    params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                    ''●●●
                    'logger.Info("STEP9 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

                    '更新処理
                    sbSQL.Clear()

                    sbSQL.AppendLine("UPDATE M_CONSTRUCTION SET")
                    sbSQL.AppendLine("     CONST_NO = VALT.CONST_NO")
                    sbSQL.AppendLine("    ,CONST_START_DT = VALT.CONST_START_DT")
                    sbSQL.AppendLine("    ,CONST_END_DT = VALT.CONST_END_DT")
                    sbSQL.AppendLine("    ,CONST_LENT_START_DT = VALT.CONST_LENT_START_DT")
                    sbSQL.AppendLine("    ,CONST_LENT_END_DT = VALT.CONST_LENT_END_DT")
                    sbSQL.AppendLine("    ,CONST_STATUS = VALT.CONST_STATUS")
                    sbSQL.AppendLine("    ,CONST_CONTENTS = VALT.CONST_CONTENTS")
                    sbSQL.AppendLine("    ,CONST_EMPLOYEE_CD = VALT.CONST_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,CONST_EMPLOYEE_NAME = VALT.CONST_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,MNG_LOC_L_CD = VALT.MNG_LOC_L_CD")
                    sbSQL.AppendLine("    ,CONST_INFO1 = VALT.CONST_INFO1")
                    sbSQL.AppendLine("    ,CONST_INFO2 = VALT.CONST_INFO2")
                    sbSQL.AppendLine("    ,CONST_INFO3 = VALT.CONST_INFO3")
                    sbSQL.AppendLine("    ,CONST_INFO4 = VALT.CONST_INFO4")
                    sbSQL.AppendLine("    ,CONST_INFO5 = VALT.CONST_INFO5")
                    sbSQL.AppendLine("    ,CONST_INFO6 = VALT.CONST_INFO6")
                    sbSQL.AppendLine("    ,CONST_INFO7 = VALT.CONST_INFO7")
                    sbSQL.AppendLine("    ,CONST_INFO8 = VALT.CONST_INFO8")
                    sbSQL.AppendLine("    ,CONST_INFO9 = VALT.CONST_INFO9")
                    sbSQL.AppendLine("    ,CONST_INFO10 = VALT.CONST_INFO10")
                    sbSQL.AppendLine("    ,CONST_INFO11 = VALT.CONST_INFO11")
                    sbSQL.AppendLine("    ,CONST_INFO12 = VALT.CONST_INFO12")
                    sbSQL.AppendLine("    ,CONST_INFO13 = VALT.CONST_INFO13")
                    sbSQL.AppendLine("    ,CONST_INFO14 = VALT.CONST_INFO14")
                    sbSQL.AppendLine("    ,CONST_INFO15 = VALT.CONST_INFO15")
                    sbSQL.AppendLine("    ,CONST_INFO16 = VALT.CONST_INFO16")
                    sbSQL.AppendLine("    ,CONST_INFO17 = VALT.CONST_INFO17")
                    sbSQL.AppendLine("    ,CONST_INFO18 = VALT.CONST_INFO18")
                    sbSQL.AppendLine("    ,CONST_INFO19 = VALT.CONST_INFO19")
                    sbSQL.AppendLine("    ,CONST_INFO20 = VALT.CONST_INFO20")
                    sbSQL.AppendLine("    ,PREV_TEMP_NUM = VALT.PREV_TEMP_NUM")
                    sbSQL.AppendLine("    ,CONST_IN_NUM = VALT.CONST_IN_NUM")
                    sbSQL.AppendLine("    ,NOW_TEMP_NUM = VALT.NOW_TEMP_NUM")
                    sbSQL.AppendLine("    ,CONST_OUT_NUM = VALT.CONST_OUT_NUM")
                    sbSQL.AppendLine("    ,CUR_LENT_NUM = VALT.CUR_LENT_NUM")
                    sbSQL.AppendLine("    ,CUR_TEMP_NUM = VALT.CUR_TEMP_NUM")
                    sbSQL.AppendLine("    ,SITE_EMPLOYEE_CD = VALT.SITE_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,SITE_EMPLOYEE_NAME = VALT.SITE_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,CONST_CONFIRM_TS = VALT.CONST_CONFIRM_TS")
                    sbSQL.AppendLine("    ,CONST_CONFIRM_EMPLOYEE_CD = VALT.CONST_CONFIRM_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,CONST_CONFIRM_EMPLOYEE_NAME = VALT.CONST_CONFIRM_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,CONST_CONFIRM_REMARK = VALT.CONST_CONFIRM_REMARK")
                    sbSQL.AppendLine("    ,TASK_TS = VALT.TASK_TS")
                    sbSQL.AppendLine("    ,INS_DATETIME = VALT.INS_DATETIME")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_CD = VALT.INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME = VALT.INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_DATETIME = VALT.UPD_DATETIME")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = VALT.UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = VALT.UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK = VALT.UPD_TASK")

                    sbSQL.AppendLine("FROM M_CONSTRUCTION")
                    sbSQL.AppendLine("INNER JOIN " & sTmpTblNm & " VALT")
                    sbSQL.AppendLine("ON  M_CONSTRUCTION.CONST_NO = VALT.CONST_NO")
                    sbSQL.AppendLine("WHERE  VALT.UPD_FLG <> 0")
                    sbSQL.AppendLine("AND VALT.ACTIVITY = '" & CsvImportProcDiv.Update & "'")  '処理区分が「U:更新」

                    db.Database.ExecuteSqlCommand(sbSQL.ToString)

                    '予約更新
                    Dim UpdateConstList As New List(Of ConstructionListCsvModel)

                    sbSQL.Clear()

                    sbSQL.AppendLine("SELECT ")
                    sbSQL.AppendLine("  M_CONSTRUCTION.CONST_NO")
                    sbSQL.AppendLine("  ,VALT.CSV_ROW_NUM")
                    sbSQL.AppendLine("FROM M_CONSTRUCTION")
                    sbSQL.AppendLine("INNER JOIN " & sTmpTblNm & " VALT")
                    sbSQL.AppendLine("ON  M_CONSTRUCTION.CONST_NO = VALT.CONST_NO")
                    sbSQL.AppendLine("WHERE  VALT.UPD_FLG <> 0")
                    sbSQL.AppendLine("AND VALT.ACTIVITY = '" & CsvImportProcDiv.Update & "'")  '処理区分が「U:更新」

                    UpdateConstList = CType(db.Database.SqlQuery(Of ConstructionListCsvModel)(sbSQL.ToString).ToList, List(Of ConstructionListCsvModel))

                    Dim lstRow As DataInportResult = New DataInportResult
                    Dim ViewModel As ConstructionListViewModel = New ConstructionListViewModel
                    For Each UpdateConst As ConstructionListCsvModel In UpdateConstList
                        '工事情報を取得
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("SELECT ")
                        sbSQL.AppendLine("  CONST.CONST_NO ")
                        sbSQL.AppendLine("  , CONST.CONST_START_DT ")
                        sbSQL.AppendLine("  , CONST.CONST_END_DT ")
                        sbSQL.AppendLine("  , CONST.CONST_LENT_START_DT ")
                        sbSQL.AppendLine("  , CONST.CONST_LENT_END_DT ")
                        sbSQL.AppendLine("  , CONST.CONST_STATUS ")
                        sbSQL.AppendLine("  , CONST.CONST_CONTENTS ")
                        sbSQL.AppendLine("  , CONST.CONST_EMPLOYEE_CD ")
                        sbSQL.AppendLine("  , CONST.CONST_EMPLOYEE_NAME ")
                        sbSQL.AppendLine("  , CONST.MNG_LOC_L_CD ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO1 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO2 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO3 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO4 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO5 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO6 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO7 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO8 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO9 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO10 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO11 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO12 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO13 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO14 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO15 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO16 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO17 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO18 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO19 ")
                        sbSQL.AppendLine("  , CONST.CONST_INFO20 ")
                        sbSQL.AppendLine("  , CONST.PREV_TEMP_NUM ")
                        sbSQL.AppendLine("  , CONST.CONST_IN_NUM ")
                        sbSQL.AppendLine("  , CONST.NOW_TEMP_NUM ")
                        sbSQL.AppendLine("  , CONST.CONST_OUT_NUM ")
                        sbSQL.AppendLine("  , CONST.CUR_LENT_NUM ")
                        sbSQL.AppendLine("  , CONST.CUR_TEMP_NUM ")
                        sbSQL.AppendLine("  , CONST.SITE_EMPLOYEE_CD ")
                        sbSQL.AppendLine("  , CONST.SITE_EMPLOYEE_NAME ")
                        sbSQL.AppendLine("  , CONST.CONST_CONFIRM_TS ")
                        sbSQL.AppendLine("  , CONST.CONST_CONFIRM_EMPLOYEE_CD ")
                        sbSQL.AppendLine("  , CONST.CONST_CONFIRM_EMPLOYEE_NAME ")
                        sbSQL.AppendLine("  , CONST.CONST_CONFIRM_REMARK ")
                        sbSQL.AppendLine("  , CONST.TASK_TS ")
                        sbSQL.AppendLine("  , CONST.INS_DATETIME ")
                        sbSQL.AppendLine("  , CONST.INS_EMPLOYEE_CD ")
                        sbSQL.AppendLine("  , CONST.INS_EMPLOYEE_NAME ")
                        sbSQL.AppendLine("  , CONST.UPD_DATETIME ")
                        sbSQL.AppendLine("  , CONST.UPD_EMPLOYEE_CD ")
                        sbSQL.AppendLine("  , CONST.UPD_EMPLOYEE_NAME ")
                        sbSQL.AppendLine("  , CONST.UPD_TASK ")
                        sbSQL.AppendLine("  , MNGLOCL.LOC_NAME AS MngLocLName ")
                        sbSQL.AppendLine("  , AUTH.UPD_FLG AS UPD_FLG ")
                        sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
                        sbSQL.AppendLine("FROM ")
                        sbSQL.AppendLine("  M_CONSTRUCTION CONST ")
                        sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
                        sbSQL.AppendLine("ON CONST.MNG_LOC_L_CD = AUTH.LOC_L_CD")
                        sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL ")
                        sbSQL.AppendLine("ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
                        sbSQL.AppendLine("AND CONST.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")
                        sbSQL.AppendLine("WHERE  CONST_NO = @CONST_NO")

                        params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@ReadAuthFlg", AuthInfo.ReadAuthFlg))
                        params.Add(New SqlParameter($"@UpdAuthFlg", AuthInfo.UpdAuthFlg))
                        params.Add(New SqlParameter($"@ReadHubAuthFlg", AuthInfo.ReadHubAuthFlg))
                        params.Add(New SqlParameter($"@UpdHubAuthFlg", AuthInfo.UpdHubAuthFlg))
                        params.Add(New SqlParameter($"@UpdAuthType", Misc.UpdAuthType.UPD_AUTH))
                        params.Add(New SqlParameter($"@CONST_NO", UpdateConst.CONST_NO))

                        'SQLを実行
                        ViewModel = db.ConstructionListView.SqlQuery(sbSQL.ToString, params.ToArray).FirstOrDefault
                        ViewModel.ConstStartDtString = CommonRfid.ToStringIntYMD(ViewModel.CONST_START_DT)
                        ViewModel.ConstEndDtString = CommonRfid.ToStringIntYMD(ViewModel.CONST_END_DT)
                        ViewModel.ConstLentStartDtString = CommonRfid.ToStringIntYMD(ViewModel.CONST_LENT_START_DT)
                        ViewModel.ConstLentEndDtString = CommonRfid.ToStringIntYMD(ViewModel.CONST_LENT_END_DT)

                        '予約を更新
                        Dim updReseult = Dao.ToolConstruction.AddAssetReserve(db, ViewModel, _LoginInfo, Me._Message)
                        If Not updReseult = "ok" Then
                            'ロールバック
                            tran.Rollback()

                            Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                            Me._lstErr(0).ErrRowIdx = UpdateConst.CSV_ROW_NUM
                            Me._lstErr(0).ErrGaiyou = updReseult
                            Return Json(Me._lstErr)
                        End If
                    Next

                    ''●●●
                    'logger.Info("STEP10 " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

                    '挿入・更新処理
                    sbSQL.Clear()

                    sbSQL.AppendLine("  INSERT INTO M_CONSTRUCTION")
                    sbSQL.AppendLine("  (")

                    sbSQL.AppendLine("     CONST_NO")
                    sbSQL.AppendLine("    ,CONST_START_DT")
                    sbSQL.AppendLine("    ,CONST_END_DT")
                    sbSQL.AppendLine("    ,CONST_LENT_START_DT")
                    sbSQL.AppendLine("    ,CONST_LENT_END_DT")
                    sbSQL.AppendLine("    ,CONST_STATUS")
                    sbSQL.AppendLine("    ,CONST_CONTENTS")
                    sbSQL.AppendLine("    ,CONST_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,CONST_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,MNG_LOC_L_CD")
                    sbSQL.AppendLine("    ,CONST_INFO1")
                    sbSQL.AppendLine("    ,CONST_INFO2")
                    sbSQL.AppendLine("    ,CONST_INFO3")
                    sbSQL.AppendLine("    ,CONST_INFO4")
                    sbSQL.AppendLine("    ,CONST_INFO5")
                    sbSQL.AppendLine("    ,CONST_INFO6")
                    sbSQL.AppendLine("    ,CONST_INFO7")
                    sbSQL.AppendLine("    ,CONST_INFO8")
                    sbSQL.AppendLine("    ,CONST_INFO9")
                    sbSQL.AppendLine("    ,CONST_INFO10")
                    sbSQL.AppendLine("    ,CONST_INFO11")
                    sbSQL.AppendLine("    ,CONST_INFO12")
                    sbSQL.AppendLine("    ,CONST_INFO13")
                    sbSQL.AppendLine("    ,CONST_INFO14")
                    sbSQL.AppendLine("    ,CONST_INFO15")
                    sbSQL.AppendLine("    ,CONST_INFO16")
                    sbSQL.AppendLine("    ,CONST_INFO17")
                    sbSQL.AppendLine("    ,CONST_INFO18")
                    sbSQL.AppendLine("    ,CONST_INFO19")
                    sbSQL.AppendLine("    ,CONST_INFO20")
                    sbSQL.AppendLine("    ,PREV_TEMP_NUM")
                    sbSQL.AppendLine("    ,CONST_IN_NUM")
                    sbSQL.AppendLine("    ,NOW_TEMP_NUM")
                    sbSQL.AppendLine("    ,CONST_OUT_NUM")
                    sbSQL.AppendLine("    ,CUR_LENT_NUM")
                    sbSQL.AppendLine("    ,CUR_TEMP_NUM")
                    sbSQL.AppendLine("    ,SITE_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,SITE_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,CONST_CONFIRM_TS")
                    sbSQL.AppendLine("    ,CONST_CONFIRM_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,CONST_CONFIRM_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,CONST_CONFIRM_REMARK")
                    sbSQL.AppendLine("    ,TASK_TS")
                    sbSQL.AppendLine("    ,INS_DATETIME")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_DATETIME")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK")
                    sbSQL.AppendLine("  )")
                    sbSQL.AppendLine("SELECT")
                    sbSQL.AppendLine("     VALT.CONST_NO")
                    sbSQL.AppendLine("    ,VALT.CONST_START_DT")
                    sbSQL.AppendLine("    ,VALT.CONST_END_DT")
                    sbSQL.AppendLine("    ,VALT.CONST_LENT_START_DT")
                    sbSQL.AppendLine("    ,VALT.CONST_LENT_END_DT")
                    sbSQL.AppendLine("    ,VALT.CONST_STATUS")
                    sbSQL.AppendLine("    ,VALT.CONST_CONTENTS")
                    sbSQL.AppendLine("    ,VALT.CONST_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,VALT.CONST_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,VALT.MNG_LOC_L_CD")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO1")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO2")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO3")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO4")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO5")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO6")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO7")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO8")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO9")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO10")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO11")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO12")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO13")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO14")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO15")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO16")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO17")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO18")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO19")
                    sbSQL.AppendLine("    ,VALT.CONST_INFO20")
                    sbSQL.AppendLine("    ,VALT.PREV_TEMP_NUM")
                    sbSQL.AppendLine("    ,VALT.CONST_IN_NUM")
                    sbSQL.AppendLine("    ,VALT.NOW_TEMP_NUM")
                    sbSQL.AppendLine("    ,VALT.CONST_OUT_NUM")
                    sbSQL.AppendLine("    ,VALT.CUR_LENT_NUM")
                    sbSQL.AppendLine("    ,VALT.CUR_TEMP_NUM")
                    sbSQL.AppendLine("    ,VALT.SITE_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,VALT.SITE_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,VALT.CONST_CONFIRM_TS")
                    sbSQL.AppendLine("    ,VALT.CONST_CONFIRM_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,VALT.CONST_CONFIRM_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,VALT.CONST_CONFIRM_REMARK")
                    sbSQL.AppendLine("    ,VALT.TASK_TS")
                    sbSQL.AppendLine("    ,VALT.INS_DATETIME")
                    sbSQL.AppendLine("    ,VALT.INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,VALT.INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,VALT.UPD_DATETIME")
                    sbSQL.AppendLine("    ,VALT.UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,VALT.UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,VALT.UPD_TASK")
                    sbSQL.AppendLine("FROM " & sTmpTblNm & " VALT")

                    sbSQL.AppendLine("WHERE NOT EXISTS")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine("  SELECT 1 FROM M_CONSTRUCTION")
                    sbSQL.AppendLine("  WHERE M_CONSTRUCTION.CONST_NO = VALT.CONST_NO")
                    sbSQL.AppendLine(")")
                    sbSQL.AppendLine("AND VALT.ACTIVITY = '" & CsvImportProcDiv.Insert & "'")  '処理区分が「I:追加」

                    db.Database.ExecuteSqlCommand(sbSQL.ToString)

                    tran.Commit()

                    ''●●●
                    'logger.Info("STEP END " & Now.ToString("yyyy/MM/dd HH:mm:ss"))

                    '操作ログ出力
                    sProcessContents = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                    _iCsvImportRowCount.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE))

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)

                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using

            Return Json(Me._lstErr)
        End Function
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function IchiranCsvExport(ViewModel As ConstructionListViewModel) As ActionResult
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

            Dim ConstData As List(Of ConstructionListViewModel)

            '工事マスタデータ取得
            ConstData = GetConstructionListData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.ConstructionList.KeyDiv, CommonSettingKey.ConstructionList.Key1, CommonSettingKey.ConstructionList.Key2, CommonSettingKey.ConstructionList.Key3, True)
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
            'CSVヘッダー取得
            Dim sHeader As String
            'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_CONSTRUCTION_LIST)
            Dim singleQuotation As String = "'"

            sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_CONSTRUCTION_LIST)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As ConstructionListViewModel In ConstData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(String.Empty)

                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CONST_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.CONST_START_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.CONST_END_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.CONST_LENT_START_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.CONST_LENT_END_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_CONTENTS)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CONST_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.MNG_LOC_L_CD.Trim)
                Select Case row.CONST_STATUS
                    Case CONST_STATUS.Incomplete
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(CONST_STATUS_CSV.Incomplete)
                    Case CONST_STATUS.Complete
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(CONST_STATUS_CSV.Complete)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO8)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO9)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO10)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO11)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO12)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO13)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO14)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO15)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO16)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO17)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO18)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO19)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_INFO20)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PREV_TEMP_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_IN_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.NOW_TEMP_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_OUT_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CUR_LENT_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CUR_TEMP_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.SITE_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.SITE_EMPLOYEE_NAME)
                If row.CONST_CONFIRM_TS.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringLongYMDHMS(row.CONST_CONFIRM_TS.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CONST_CONFIRM_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_CONFIRM_EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_CONFIRM_REMARK)

                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                'ファイルをダウンロードさせる
                Call FileDownload(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)

                'ダウンロードさせた後、ファイルを削除。
                System.IO.File.Delete(sFullPath + "\" + sUrlFileName)

            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     ConstData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})


        End Function
#End Region

#Region "CSVヘッダー取得"
        ''' <summary>
        ''' CSVヘッダー取得
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

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetConstructionListData() As List(Of ConstructionListViewModel)
            Dim lstConstructionList As List(Of ConstructionListViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstConstructionList = db.ConstructionListView.SqlQuery(CreateSQL(), _paramSqlList.ToArray).ToList

            'データを返却
            Return lstConstructionList
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

#Region "CSVファイルのエラーチェック"
        ''' <summary>
        ''' CSVファイルのエラーチェック
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
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstructionList, _LoginInfo)

            '一覧画面用
            Dim tblShokai As DataTable = Dao.ToolLocation.GetMngLocationLData(_LoginInfo.EmployeeCd, AuthInfo, False, Misc.UpdAuthType.UPD_AUTH)
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

            'モーダル画面の管理ロケーションコンボ設定
            Dim SelectedCd As String = _ViewModel.MNG_LOC_L_CD
            If AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                '編集権限がある社員の場合（編集可能なモーダルダイアログ用）
                Dim tblEdit As DataTable
                Dim lstLoc_Edit As New List(Of LocationDropdownModel)     'ロケーションマスタリスト
                '編集可能なロケーションを取得
                tblEdit = Dao.ToolLocation.GetMngLocationLData(_LoginInfo.EmployeeCd, AuthInfo, True, Misc.UpdAuthType.UPD_AUTH)
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

#Region "作業責任者ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 作業責任者ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetConstEmployeeData() As JsonResult
            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstructionList, _LoginInfo)
            Dim tbl As DataTable = Dao.ToolEmployee.GetEmployeeDataHubLocLCd(_LoginInfo.EmployeeCd, AuthInfo)
            Dim lstEmployee As New List(Of DropdownModel)     '社員マスタリスト

            For Each row As DataRow In tbl.Rows
                lstEmployee.Add(
                    New DropdownModel With {
                        .Cd = row("EMPLOYEE_CD").ToString,
                        .Name = row("EMPLOYEE_NAME").ToString
                    }
                )
            Next

            'モーダル画面のロケーションコンボ設定
            Dim SelectedCd As String = _ViewModel.CONST_EMPLOYEE_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelEmployee = New SelectList(lstEmployee, "Cd", "Name", "")
            Else
                ViewBag.modalmodelEmployee = New SelectList(lstEmployee, "Cd", "Name", SelectedCd)
            End If

            Return Json(lstEmployee)
        End Function
#End Region

#Region "工事状態ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 工事状態ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetConstStatusData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = CONST_STATUS.Incomplete,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_CONSTRUCTION_STATUS_INCOMPLETE).ToString
                },
                New DropdownModel With {
                    .Cd = CONST_STATUS.Complete,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_CONSTRUCTION_STATUS_COMPLETE).ToString
                }}

            '一覧画面の工事状態コンボ設定
            ViewBag.ConstStatusList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchConstStatus)

            Return Json(lst)
        End Function
#End Region

#Region "工事完了ダイアログ表示チェック"
        ''' <summary>
        ''' 工事完了ダイアログ表示チェック
        ''' </summary>
        ''' <returns></returns>
        Public Function ShowDeleteDialog(ByVal constNo As String) As ActionResult

            Dim db As New SystemDBContext

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim sbChkSQL As StringBuilder = New StringBuilder
            Dim lstParam As New List(Of SqlParameter)

            '登録工事番号チェック
            sbChkSQL = New StringBuilder

            sbChkSQL.AppendLine("SELECT *")
            sbChkSQL.AppendLine("FROM T_TEMPORARY")
            sbChkSQL.AppendLine("WHERE REG_CONST_NO ='" & constNo & "'")

            'SQLを実行する
            Dim chkModel As List(Of TemporaryListModel) = db.TemporaryList.SqlQuery(sbChkSQL.ToString).ToList
            If chkModel.Count > 0 Then
                Return Json("error")
            End If

            Return Json("ok")
        End Function
#End Region

#Region "工事完了ダイアログ表示チェック"
        ''' <summary>
        ''' 工事完了ダイアログ表示チェック
        ''' </summary>
        ''' <returns></returns>
        Public Function ShowConstComplete(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As ActionResult

            '最新のチェック状態でセッション情報を更新する(追加および削除)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(SelectKeys, NoneSelectKeys)

            Dim db As New SystemDBContext

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstructionList, _LoginInfo)

            Dim sbChkSQL As StringBuilder = New StringBuilder
            Dim lstParam As New List(Of SqlParameter)

            Dim sSelCONST_NO As String = "'" & String.Join("','", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(ConstructionListKey.CONST_NO))) & "'"

            sbChkSQL.AppendLine("SELECT TOP 1 *")
            sbChkSQL.AppendLine("FROM M_CONSTRUCTION")
            sbChkSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
            sbChkSQL.AppendLine("    ON AUTH.LOC_L_CD = M_CONSTRUCTION.MNG_LOC_L_CD ")
            lstParam.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
            lstParam.Add(New SqlParameter("@ReadAuthFlg", AuthInfo.ReadAuthFlg))
            lstParam.Add(New SqlParameter("@UpdAuthFlg", AuthInfo.UpdAuthFlg))
            lstParam.Add(New SqlParameter("@ReadHubAuthFlg", AuthInfo.ReadHubAuthFlg))
            lstParam.Add(New SqlParameter("@UpdHubAuthFlg", AuthInfo.UpdHubAuthFlg))
            lstParam.Add(New SqlParameter("@UpdAuthType", Misc.UpdAuthType.UPD_AUTH))
            sbChkSQL.AppendLine("WHERE M_CONSTRUCTION.CONST_NO IN (" & sSelCONST_NO & ") ")
            sbChkSQL.AppendLine("AND   AUTH.UPD_FLG = '" & Misc.UpdFlg.No & "'")

            'SQLを実行する
            Dim chkModel As List(Of ConstructionListModel) = db.ConstructionList.SqlQuery(sbChkSQL.ToString, lstParam.ToArray).ToList
            If chkModel.Count > 0 Then
                Return Json("auth-error")
            End If

            '完了済みチェック
            sbChkSQL = New StringBuilder

            sbChkSQL.AppendLine("SELECT *")
            sbChkSQL.AppendLine("FROM M_CONSTRUCTION")
            sbChkSQL.AppendLine("WHERE CONST_NO IN (" & sSelCONST_NO & ") ")

            'SQLを実行する
            Dim chkStatusModel As List(Of ConstructionListModel) = db.ConstructionList.SqlQuery(sbChkSQL.ToString).ToList
            If chkStatusModel.Any(Function(x) x.CONST_STATUS = CONST_STATUS.Complete) Then
                Return Json("error")
            End If

            '返却状況チェック
            sbChkSQL = New StringBuilder
            lstParam = New List(Of SqlParameter)

            sbChkSQL.AppendLine("SELECT RESVM.*")
            sbChkSQL.AppendLine("FROM T_RESERVE_DETAIL RESVM")
            sbChkSQL.AppendLine("INNER JOIN (SELECT RESERVE_ID FROM T_RESERVE WHERE REMARK IN (" & sSelCONST_NO & ")) RESVH")
            sbChkSQL.AppendLine("ON RESVM.RESERVE_ID = RESVH.RESERVE_ID")
            sbChkSQL.AppendLine("WHERE RESVM.RESERVE_STATUS = @RESERVE_STATUS")
            lstParam.Add(New SqlParameter("@RESERVE_STATUS", ReserveStatus.Lending))

            'SQLを実行する
            Dim chkReserveModel As List(Of ReserveDetailModel) = db.ReserveDetail.SqlQuery(sbChkSQL.ToString, lstParam.ToArray).ToList
            If chkReserveModel.Count > 0 Then
                Return Json("unreturned")
            End If

            Return Json("ok")
        End Function
#End Region

#Region "帳票出力ダイアログ表示チェック"
        ''' <summary>
        ''' 帳票出力ダイアログ表示チェック
        ''' </summary>
        ''' <returns></returns>
        Public Function ShowReportOutput(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As ActionResult

            '最新のチェック状態でセッション情報を更新する(追加および削除)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(SelectKeys, NoneSelectKeys)

            Dim db As New SystemDBContext

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '未完了チェック
            Dim sbChkSQL As StringBuilder = New StringBuilder
            Dim lstParam As New List(Of SqlParameter)

            Dim sSelCONST_NO As String = "'" & String.Join("','", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(ConstructionListKey.CONST_NO))) & "'"

            sbChkSQL.AppendLine("SELECT *")
            sbChkSQL.AppendLine("FROM M_CONSTRUCTION")
            sbChkSQL.AppendLine("WHERE CONST_NO IN (" & sSelCONST_NO & ") ")

            'SQLを実行する
            Dim chkStatusModel As List(Of ConstructionListModel) = db.ConstructionList.SqlQuery(sbChkSQL.ToString).ToList
            If chkStatusModel.Any(Function(x) x.CONST_STATUS = CONST_STATUS.Incomplete) Then
                Return Json("error")
            End If

            Return Json("ok")
        End Function
#End Region

#Region "仮置解除"
        ''' <summary>
        ''' 仮置解除
        ''' </summary>
        ''' <returns></returns>
        <HttpPost()>
        Public Function ReleaseTemp(ByVal SelectKeys As String, ByVal ConstNo As String) As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim db As New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)
            Dim ret As Integer

            Dim sKeyStr As String = "'" & String.Join("','", SelectKeys.Split(",")) & "'"

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try
                    '紐付け元 紐付け先のデータを消される前に先に修正


                    'コンマ区切りで分割する
                    Dim tempIDs As String()
                    tempIDs = sKeyStr.Split(","c)
                    '選択した件数ループ
                    For Each tempID As String In tempIDs

                        '削除する全情報取得(紐付け元で使用)
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("SELECT ")
                        sbSQL.AppendLine(" CONST_NO ")
                        sbSQL.AppendLine(" ,ASSET_NO ")
                        sbSQL.AppendLine(" FROM T_CONST_ASSET  ")

                        sbSQL.AppendLine(" WHERE ")
                        sbSQL.AppendLine($" CONST_NO = '{ConstNo}' ")
                        sbSQL.AppendLine(" AND EXISTS ")
                        sbSQL.AppendLine("  (")
                        sbSQL.AppendLine("      SELECT 1 ")
                        sbSQL.AppendLine("      FROM T_TEMP_ASSET TASSET")
                        sbSQL.AppendLine("      WHERE TEMP_ID = " & tempID)
                        sbSQL.AppendLine("      AND T_CONST_ASSET.ASSET_NO = TASSET.ASSET_NO")
                        sbSQL.AppendLine("  )")

                        Dim deleteData As DbRawSqlQuery(Of ConstAssetModel) = _db.Database.SqlQuery(Of ConstAssetModel)(sbSQL.ToString)

                        ''仮置きマスタから登録工事番号取得
                        'sbSQL.Clear()
                        'params = New List(Of SqlParameter)

                        'sbSQL.AppendLine("SELECT ")
                        'sbSQL.AppendLine(" REG_CONST_NO ")
                        'sbSQL.AppendLine(" FROM T_TEMPORARY ")
                        'sbSQL.AppendLine(" WHERE ")
                        'sbSQL.AppendLine($" TEMP_ID = {tempID} ")

                        ''一件のみ取得
                        'Dim regCountNo As String = db.Database.SqlQuery(Of String)(sbSQL.ToString).First()

                        'tempIdごとの解除数取得
                        Dim tempIdCount As Integer = deleteData.Count()
                        'Dim filteredData As List(Of ConstAssetModel) = deleteData.Where(Function(item) item.CONST_NO = regCountNo).ToList()


                        '工事資産更新
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("UPDATE T_CONST_ASSET SET")
                        sbSQL.AppendLine("     CONST_ASSET_STATUS = '1' ")
                        sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                        sbSQL.AppendLine(" WHERE CONST_NO = (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ")")
                        sbSQL.AppendLine(" AND ASSET_NO IN (SELECT ASSET_NO FROM T_TEMP_ASSET WHERE TEMP_ID = " + tempID + ")")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))
                        params.Add(New SqlParameter($"@CONST_NO", ConstNo))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                        '工事マスタ更新
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("UPDATE M_CONSTRUCTION SET")
                        'sbSQL.AppendLine("     PREV_TEMP_NUM = CONST_IN_NUM - (SELECT COUNT(*) FROM T_TEMP_ASSET WHERE TEMP_ID = " + tempID + ")")
                        'sbSQL.AppendLine("    ,CONST_IN_NUM = CONST_IN_NUM - (SELECT COUNT(*) FROM T_TEMP_ASSET WHERE TEMP_ID = " + tempID + ")")

                        sbSQL.AppendLine($"   PREV_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND PREV_TEMP_FLAG = 'Y')")                                                        '前回仮置き
                        sbSQL.AppendLine($"  ,CONST_IN_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1')")                                                    '持込数
                        sbSQL.AppendLine($"  ,NOW_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND PREV_TEMP_FLAG = 'N' AND TEMP_ID IS NOT NULL )")   '今回仮置き数
                        sbSQL.AppendLine($"  ,CONST_OUT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NULL AND CONST_ASSET_STATUS = '1')")                               '持帰数
                        sbSQL.AppendLine($"  ,CUR_LENT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND TEMP_ID IS NULL )")                               '現在貸出数
                        sbSQL.AppendLine($"  ,CUR_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                             '現在仮置数

                        sbSQL.AppendLine("    ,TASK_TS = CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                        sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                        sbSQL.AppendLine(" FROM M_CONSTRUCTION CON ")
                        sbSQL.AppendLine(" WHERE CONST_NO = (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ")")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))
                        params.Add(New SqlParameter($"@CONST_NO", ConstNo))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                        '工事履歴登録
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("INSERT INTO T_CONST_LOG")
                        sbSQL.AppendLine("(")
                        sbSQL.AppendLine("     CONST_NO")
                        sbSQL.AppendLine("    ,CONST_WORK_TS")
                        sbSQL.AppendLine("    ,CONST_WORK_DIV")
                        sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                        sbSQL.AppendLine("    ,LENT_NUM")
                        sbSQL.AppendLine("    ,LEASE_NUM")
                        sbSQL.AppendLine("    ,TEMP_REG_NUM")
                        sbSQL.AppendLine("    ,CONST_IN_MATCH_NUM")
                        sbSQL.AppendLine("    ,CONST_OUT_MATCH_NUM")
                        sbSQL.AppendLine("    ,CONST_IN_NUM")
                        sbSQL.AppendLine("    ,TEMP_NUM")
                        sbSQL.AppendLine("    ,CONST_OUT_NUM")
                        sbSQL.AppendLine("    ,INS_DATETIME")
                        sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_DATETIME")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_TASK")
                        sbSQL.AppendLine(") VALUES (")
                        sbSQL.AppendLine("     (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ")")
                        sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                        sbSQL.AppendLine("    ,'" + ConstWorkDiv.TempRelease + "'")
                        sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                        sbSQL.AppendLine("    ,(SELECT LENT_NUM FROM T_CONST_LOG WHERE CONST_NO = ( SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + " ) AND CONST_WORK_TS = ( SELECT MAX(CONST_WORK_TS) FROM T_CONST_LOG WHERE CONST_NO = ( SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + $" ) )) + {tempIdCount}")
                        sbSQL.AppendLine("    ,0")
                        sbSQL.AppendLine($"    , {tempIdCount}")
                        sbSQL.AppendLine("    ,0")
                        sbSQL.AppendLine("    ,0")
                        sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ") AND CONST_ASSET_STATUS = " + "'" + CONST_ASSET_STATUS.Lending + "')")
                        sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ") AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = " + "'" + CONST_ASSET_STATUS.Lending + "')")
                        sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ") AND TEMP_ID IS NULL AND CONST_ASSET_STATUS = " + "'" + CONST_ASSET_STATUS.Lending + "')")
                        sbSQL.AppendLine("    ,@INS_DATETIME")
                        sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,@UPD_DATETIME")
                        sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,@UPD_TASK")
                        sbSQL.AppendLine(")")

                        params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                        params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                        '工事資産履歴登録
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("INSERT INTO T_CONST_ASSET_LOG")
                        sbSQL.AppendLine("(")
                        sbSQL.AppendLine("     CONST_NO")
                        sbSQL.AppendLine("    ,CONST_WORK_TS")
                        sbSQL.AppendLine("    ,ASSET_NO")
                        sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                        sbSQL.AppendLine("    ,CONST_MATCH_STATUS")
                        sbSQL.AppendLine("    ,TEMP_FLAG")
                        sbSQL.AppendLine("    ,TEMP_ID")
                        sbSQL.AppendLine("    ,INS_DATETIME")
                        sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_DATETIME")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,UPD_TASK")
                        sbSQL.AppendLine(")")
                        sbSQL.AppendLine("SELECT")
                        sbSQL.AppendLine("     (SELECT REG_CONST_NO FROM T_TEMPORARY WHERE TEMP_ID = " + tempID + ")")
                        sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                        sbSQL.AppendLine("    ,TASSET.ASSET_NO")
                        sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                        sbSQL.AppendLine("    ,'" + CONST_MATCH_STATUS.Lending + "'")
                        sbSQL.AppendLine("    ,'" + TempFlg.Cd.Yes + "'")
                        sbSQL.AppendLine("    ,TASSET.TEMP_ID")
                        sbSQL.AppendLine("    ,@INS_DATETIME")
                        sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,@UPD_DATETIME")
                        sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,@UPD_TASK")
                        sbSQL.AppendLine("FROM T_TEMP_ASSET TASSET")

                        sbSQL.AppendLine("WHERE  TASSET.TEMP_ID = " + tempID + "")

                        params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                        params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelTemporaryList.TITLE)))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)
                    Next


                    '仮置きテーブル更新
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("UPDATE T_TEMPORARY SET")
                    sbSQL.AppendLine("    REL_CONST_NO = NULL")
                    sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                    sbSQL.AppendLine("WHERE TEMP_ID IN (" & sKeyStr & ")")

                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '工事資産削除処理
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("DELETE FROM T_CONST_ASSET")
                    sbSQL.AppendLine("WHERE CONST_NO = @CONST_NO")
                    sbSQL.AppendLine("  AND EXISTS")
                    sbSQL.AppendLine("  (")
                    sbSQL.AppendLine("      SELECT 1 ")
                    sbSQL.AppendLine("      FROM T_TEMP_ASSET TASSET")
                    sbSQL.AppendLine("      WHERE TEMP_ID IN (" & sKeyStr & ")")
                    sbSQL.AppendLine("      AND T_CONST_ASSET.ASSET_NO = TASSET.ASSET_NO")
                    sbSQL.AppendLine("  )")

                    params.Add(New SqlParameter($"@CONST_NO", ConstNo))

                    ret = db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                    '工事マスタ更新
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("UPDATE M_CONSTRUCTION SET")
                    'sbSQL.AppendLine("     PREV_TEMP_NUM = (SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND PREV_TEMP_FLAG = '" + PrevTempFlg.Cd.Yes + "')")
                    'sbSQL.AppendLine("    ,CONST_IN_NUM = (SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO)")
                    'sbSQL.AppendLine("    ,CONST_OUT_NUM = (SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND TEMP_ID IS NULL)")
                    'sbSQL.AppendLine("    ,CUR_TEMP_NUM = (SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND CONST_ASSET_STATUS = '" + CONST_ASSET_STATUS.Lending + "')")

                    sbSQL.AppendLine($"   PREV_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND PREV_TEMP_FLAG = 'Y')")                                                        '前回仮置き
                    sbSQL.AppendLine($"  ,CONST_IN_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1')")                                                    '持込数
                    sbSQL.AppendLine($"  ,NOW_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND PREV_TEMP_FLAG = 'N' AND TEMP_ID IS NOT NULL )")   '今回仮置き数
                    sbSQL.AppendLine($"  ,CONST_OUT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NULL AND CONST_ASSET_STATUS = '1')")                               '持帰数
                    sbSQL.AppendLine($"  ,CUR_LENT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND TEMP_ID IS NULL )")                               '現在貸出数
                    sbSQL.AppendLine($"  ,CUR_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                             '現在仮置数

                    sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                    sbSQL.AppendLine(" FROM M_CONSTRUCTION CON ")
                    sbSQL.AppendLine(" WHERE CONST_NO = @CONST_NO")

                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))
                    params.Add(New SqlParameter($"@CONST_NO", ConstNo))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '工事履歴登録
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("INSERT INTO T_CONST_LOG")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine("     CONST_NO")
                    sbSQL.AppendLine("    ,CONST_WORK_TS")
                    sbSQL.AppendLine("    ,CONST_WORK_DIV")
                    sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                    sbSQL.AppendLine("    ,LENT_NUM")
                    sbSQL.AppendLine("    ,LEASE_NUM")
                    sbSQL.AppendLine("    ,TEMP_REG_NUM")
                    sbSQL.AppendLine("    ,CONST_IN_MATCH_NUM")
                    sbSQL.AppendLine("    ,CONST_OUT_MATCH_NUM")
                    sbSQL.AppendLine("    ,CONST_IN_NUM")
                    sbSQL.AppendLine("    ,TEMP_NUM")
                    sbSQL.AppendLine("    ,CONST_OUT_NUM")
                    sbSQL.AppendLine("    ,INS_DATETIME")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_DATETIME")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK")
                    sbSQL.AppendLine(") VALUES (")
                    sbSQL.AppendLine("     @CONST_NO")
                    sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                    sbSQL.AppendLine("    ,'" + ConstWorkDiv.TempRelease + "'")
                    sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,@TEMP_REG_NUM")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,0")
                    sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO)")
                    sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND TEMP_ID IS NOT NULL)")
                    sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM T_CONST_ASSET WHERE CONST_NO = @CONST_NO AND TEMP_ID IS NULL)")
                    sbSQL.AppendLine("    ,@INS_DATETIME")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_DATETIME")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_TASK")
                    sbSQL.AppendLine(")")

                    params.Add(New SqlParameter($"@CONST_NO", ConstNo))
                    params.Add(New SqlParameter($"@TEMP_REG_NUM", ret))
                    params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '工事資産履歴登録
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("INSERT INTO T_CONST_ASSET_LOG")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine("     CONST_NO")
                    sbSQL.AppendLine("    ,CONST_WORK_TS")
                    sbSQL.AppendLine("    ,ASSET_NO")
                    sbSQL.AppendLine("    ,CONST_MATCH_DIV")
                    sbSQL.AppendLine("    ,CONST_MATCH_STATUS")
                    sbSQL.AppendLine("    ,TEMP_FLAG")
                    sbSQL.AppendLine("    ,TEMP_ID")
                    sbSQL.AppendLine("    ,INS_DATETIME")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_DATETIME")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,UPD_TASK")
                    sbSQL.AppendLine(")")
                    sbSQL.AppendLine("SELECT")
                    sbSQL.AppendLine("     @CONST_NO")
                    sbSQL.AppendLine("    ,CONVERT(bigint, FORMAT(GetDate(), 'yyyyMMddHHmmss'))")
                    sbSQL.AppendLine("    ,TASSET.ASSET_NO")
                    sbSQL.AppendLine("    ,'" + ConstMatchDiv.None + "'")
                    sbSQL.AppendLine("    ,'" + CONST_MATCH_STATUS.TempRelease + "'")
                    sbSQL.AppendLine("    ,'" + TempFlg.Cd.No + "'")
                    sbSQL.AppendLine("    ,NULL")
                    sbSQL.AppendLine("    ,@INS_DATETIME")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_DATETIME")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("    ,@UPD_TASK")
                    sbSQL.AppendLine("FROM T_TEMP_ASSET TASSET")

                    sbSQL.AppendLine("WHERE  TASSET.TEMP_ID IN (" & sKeyStr & ")")

                    params.Add(New SqlParameter($"@CONST_NO", ConstNo))
                    params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)


                    '紐付け元
                    '選択した件数ループ

                    'tempIDごとに処理
                    For Each tempID As String In tempIDs

                    Next

                    tran.Commit()

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelConstructionList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabelConstructionList.BUTTON_RELEASE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     SelectKeys.Split(",").Count.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionList.TITLE))

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)

                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using

            Return Json("ok")

        End Function
#End Region

#Region "資産予約登録処理"
        ''' <summary>
        ''' 資産予約登録処理
        ''' </summary>
        ''' <returns></returns>
        Public Function AddAssetReserve(ByVal ViewModel As ConstructionListViewModel, ByVal ReserveNum As Integer, ByVal SelectedProductNo As String) As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim db As New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)
            Dim returnValue As Integer = 0

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try

                    '予約明細登録
                    '照合機能の予約実施設定を取得
                    Dim MatchingReserve As SettingInfo = _Message.getSettingInfo(CommonSettingKey.MatchingReserveFunction.KeyDiv,
                                                                  CommonSettingKey.MatchingReserveFunction.Key1,
                                                                  CommonSettingKey.MatchingReserveFunction.Key2,
                                                                  CommonSettingKey.MatchingReserveFunction.Key3, True)

                    Dim sWorkDayFlg As String = MatchingReserve.DATA2


                    '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                    Dim sWorkFlg As String = WorkFlg.Jitsubi
                    If sWorkDayFlg = WorkDayFlg.Yes Then
                        sWorkFlg = WorkFlg.Kadobi
                    End If

                    '予約明細データ取得(警告フラグも取得)
                    Dim resDetail = Dao.ToolConstruction.GetReserveDetailDataWarning(ViewModel.CONST_NO, CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd"), CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd"), sWorkFlg)

                    '品番リスト取得
                    Dim SelectedProductNoList = SelectedProductNo.Split(",")

                    For Each ProductNo In SelectedProductNoList
                        If Not resDetail.Rows.Count = 0 Then
                            '予約明細が存在する場合
                            '予約明細を品番で絞り込み
                            Dim resSelDetail = If(resDetail.Select("PRODUCT_NO ='" + ProductNo + "'").Count = 0, resDetail.Clone, resDetail.Select("PRODUCT_NO ='" + ProductNo + "'").CopyToDataTable)
                            '貸出・返却済み数チェック
                            Dim lendHenkyakuCnt = resSelDetail.Select("RESERVE_STATUS IN ('" + ReserveStatus.Lending + "','" + ReserveStatus.HenkyakuSumi + "')").Count()
                            If lendHenkyakuCnt > ReserveNum Then
                                Return Json(String.Format(_Message.getMessage(CommonLabelConstructionList.MSG_LOWER_LEND_RETURNED_NUM), lendHenkyakuCnt.ToString))
                            End If

                            '予約テーブル更新
                            sbSQL.Clear()
                            params = New List(Of SqlParameter)

                            sbSQL.AppendLine("UPDATE T_RESERVE SET")
                            sbSQL.AppendLine("    REQ_EMP_CD = @REQ_EMP_CD")
                            sbSQL.AppendLine("    ,CHARGE_EMP_CD = @CHARGE_EMP_CD")
                            sbSQL.AppendLine("    ,REAL_START_DT = @REAL_START_DT")
                            sbSQL.AppendLine("    ,REAL_END_DT = @REAL_END_DT")
                            sbSQL.AppendLine("    ,USER_START_DT = @USER_START_DT")
                            sbSQL.AppendLine("    ,USER_END_DT = @USER_END_DT")
                            sbSQL.AppendLine("    ,MNG_LOC_L_CD = @MNG_LOC_L_CD")
                            sbSQL.AppendLine("    ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                            sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,UPD_TASK = @UPD_TASK")
                            sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                            params.Add(New SqlParameter($"@REQ_EMP_CD", ViewModel.CONST_EMPLOYEE_CD))
                            params.Add(New SqlParameter($"@CHARGE_EMP_CD", ViewModel.CONST_EMPLOYEE_CD))
                            params.Add(New SqlParameter($"@REAL_START_DT", If(String.IsNullOrEmpty(ViewModel.ConstStartDtString), 0, CInt(CDate(ViewModel.ConstStartDtString).ToString("yyyyMMdd")))))
                            params.Add(New SqlParameter($"@REAL_END_DT", If(String.IsNullOrEmpty(ViewModel.ConstEndDtString), 0, CInt(CDate(ViewModel.ConstEndDtString).ToString("yyyyMMdd")))))
                            params.Add(New SqlParameter($"@USER_START_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentStartDtString), 0, CInt(CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd")))))
                            params.Add(New SqlParameter($"@USER_END_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentEndDtString), 0, CInt(CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd")))))
                            params.Add(New SqlParameter($"@MNG_LOC_L_CD", ViewModel.MNG_LOC_L_CD))
                            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                            params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))
                            params.Add(New SqlParameter($"@RESERVE_ID", resDetail.Rows(0)("RESERVE_ID")))

                            db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)


                            '貸出中データ更新
                            Dim lendingDataList = resSelDetail.Select("RESERVE_STATUS ='" + ReserveStatus.Lending + "'")
                            For Each lendingData As DataRow In lendingDataList
                                sbSQL.Clear()
                                params = New List(Of SqlParameter)
                                returnValue = 0

                                sbSQL.AppendLine("DECLARE @nRet INT;")
                                sbSQL.AppendLine("EXEC @nRet =dbo.[ASSET_REPLACE_RESERVE_POSSIBLE_SP]")
                                sbSQL.AppendLine("            @IN_PRODUCT_NO")
                                sbSQL.AppendLine("            ,@ASSET_NO")
                                sbSQL.AppendLine("            ,@IN_FROM_DT")
                                sbSQL.AppendLine("            ,@IN_TO_DT")
                                sbSQL.AppendLine("            ,@IN_KADO_JITU")
                                sbSQL.AppendLine("            ,@IN_CONST_NO")
                                sbSQL.AppendLine("            ,@IN_CONST_RESERVE_ID")
                                sbSQL.AppendLine("            ,@IN_REG_EMP_CD")
                                sbSQL.AppendLine("            ,@IN_REG_EMP_NAME")
                                sbSQL.AppendLine("            ,@IN_REG_TASK")
                                sbSQL.AppendLine(";")
                                sbSQL.AppendLine("SELECT @RETURN_VALUE = @nRet")

                                params.Add(New SqlParameter($"@IN_PRODUCT_NO", ProductNo))
                                params.Add(New SqlParameter($"@ASSET_NO", lendingData("ASSET_NO").ToString))
                                params.Add(New SqlParameter($"@IN_FROM_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentStartDtString), 0, CInt(CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd")))))
                                params.Add(New SqlParameter($"@IN_TO_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentEndDtString), 0, CInt(CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd")))))
                                params.Add(New SqlParameter($"@IN_KADO_JITU", sWorkDayFlg))
                                params.Add(New SqlParameter($"@IN_CONST_NO", ViewModel.CONST_NO))
                                params.Add(New SqlParameter($"@IN_CONST_RESERVE_ID", lendingData("RESERVE_ID").ToString))
                                params.Add(New SqlParameter($"@IN_REG_EMP_CD", _LoginInfo.EmployeeCd))
                                params.Add(New SqlParameter($"@IN_REG_EMP_NAME", _LoginInfo.EmployeeNm))
                                params.Add(New SqlParameter($"@IN_REG_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

                                Dim returnValueParam = New SqlParameter()
                                returnValueParam.ParameterName = $"@RETURN_VALUE"
                                returnValueParam.SqlDbType = SqlDbType.Int
                                returnValueParam.Direction = ParameterDirection.Output
                                params.Add(returnValueParam)

                                db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                                returnValue = params.Where(Function(x) x.ParameterName = "@RETURN_VALUE").FirstOrDefault.Value

                                '代替品番予約更新ストアドプロシージャエラー
                                If returnValue = AssetReplaceReservePossibleSpResult.TermError Then
                                    tran.Rollback()
                                    Return Json(String.Format(_Message.getMessage(CommonLabelConstructionList.MSG_CANNOT_CHANGE_LENDING_ASSET), lendingData("ASSET_NO").ToString))
                                End If
                            Next

                            '予約中データ更新
                            Dim procCnt = 1
                            Dim reserveDataList = resSelDetail.Select("RESERVE_STATUS ='" + ReserveStatus.Mi + "'")
                            For Each reserveData As DataRow In reserveDataList

                                '予約数を超過している場合は更新せずに明細、メンテナンスデータを削除
                                If lendHenkyakuCnt + procCnt > ReserveNum Then
                                    '予約明細削除
                                    sbSQL.Clear()
                                    params = New List(Of SqlParameter)

                                    '削除処理
                                    sbSQL.AppendLine("DELETE FROM T_RESERVE_DETAIL")
                                    sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")
                                    sbSQL.AppendLine("AND ASSET_NO = @ASSET_NO")
                                    sbSQL.AppendLine("AND REAL_START_DT = @REAL_START_DT")

                                    params.Add(New SqlParameter($"@RESERVE_ID", reserveData("RESERVE_ID")))
                                    params.Add(New SqlParameter($"@ASSET_NO", reserveData("ASSET_NO")))
                                    params.Add(New SqlParameter($"@REAL_START_DT", reserveData("REAL_START_DT")))

                                    db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                                    '予約メンテナンス削除
                                    sbSQL.Clear()
                                    params = New List(Of SqlParameter)

                                    '削除処理
                                    sbSQL.AppendLine("DELETE FROM T_RESERVE_MAINTE")
                                    sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")
                                    sbSQL.AppendLine("AND ASSET_NO = @ASSET_NO")

                                    params.Add(New SqlParameter($"@RESERVE_ID", reserveData("RESERVE_ID")))
                                    params.Add(New SqlParameter($"@ASSET_NO", reserveData("ASSET_NO")))

                                    db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                                    procCnt += 1

                                    Continue For
                                End If
                                sbSQL.Clear()
                                params = New List(Of SqlParameter)
                                returnValue = 0

                                sbSQL.AppendLine("DECLARE @nRet INT;")
                                sbSQL.AppendLine("EXEC @nRet =dbo.[ASSET_REPLACE_RESERVE_POSSIBLE_SP]")
                                sbSQL.AppendLine("            @IN_PRODUCT_NO")
                                sbSQL.AppendLine("            ,@ASSET_NO")
                                sbSQL.AppendLine("            ,@IN_FROM_DT")
                                sbSQL.AppendLine("            ,@IN_TO_DT")
                                sbSQL.AppendLine("            ,@IN_KADO_JITU")
                                sbSQL.AppendLine("            ,@IN_CONST_NO")
                                sbSQL.AppendLine("            ,@IN_CONST_RESERVE_ID")
                                sbSQL.AppendLine("            ,@IN_REG_EMP_CD")
                                sbSQL.AppendLine("            ,@IN_REG_EMP_NAME")
                                sbSQL.AppendLine("            ,@IN_REG_TASK")
                                sbSQL.AppendLine(";")
                                sbSQL.AppendLine("SELECT @RETURN_VALUE = @nRet")

                                params.Add(New SqlParameter($"@IN_PRODUCT_NO", ProductNo))
                                params.Add(New SqlParameter($"@ASSET_NO", reserveData("ASSET_NO").ToString))
                                params.Add(New SqlParameter($"@IN_FROM_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentStartDtString), 0, CInt(CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd")))))
                                params.Add(New SqlParameter($"@IN_TO_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentEndDtString), 0, CInt(CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd")))))
                                params.Add(New SqlParameter($"@IN_KADO_JITU", sWorkDayFlg))
                                params.Add(New SqlParameter($"@IN_CONST_NO", ViewModel.CONST_NO))
                                params.Add(New SqlParameter($"@IN_CONST_RESERVE_ID", reserveData("RESERVE_ID").ToString))
                                params.Add(New SqlParameter($"@IN_REG_EMP_CD", _LoginInfo.EmployeeCd))
                                params.Add(New SqlParameter($"@IN_REG_EMP_NAME", _LoginInfo.EmployeeNm))
                                params.Add(New SqlParameter($"@IN_REG_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

                                Dim returnValueParam = New SqlParameter()
                                returnValueParam.ParameterName = $"@RETURN_VALUE"
                                returnValueParam.SqlDbType = SqlDbType.Int
                                returnValueParam.Direction = ParameterDirection.Output
                                params.Add(returnValueParam)

                                db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                                returnValue = params.Where(Function(x) x.ParameterName = "@RETURN_VALUE").FirstOrDefault.Value

                                '代替品番予約更新ストアドプロシージャエラー
                                If returnValue = AssetReplaceReservePossibleSpResult.ChangeError Then
                                    tran.Rollback()
                                    Return Json(String.Format(_Message.getMessage(CommonLabelConstructionList.MSG_CANNOT_CHANGE_RESERVE), reserveData("ASSET_NO").ToString))
                                ElseIf returnValue = AssetReplaceReservePossibleSpResult.TermError Then
                                    tran.Rollback()
                                    Return Json(String.Format(_Message.getMessage(CommonLabelConstructionList.MSG_CANNOT_CHANGE_TERM), reserveData("ASSET_NO").ToString))
                                End If

                                procCnt += 1
                            Next

                            '不足分の予約を登録
                            For icnt As Integer = 0 To ReserveNum - resSelDetail.Rows.Count - 1
                                sbSQL.Clear()
                                params = New List(Of SqlParameter)
                                returnValue = 0

                                sbSQL.AppendLine("DECLARE @nRet INT;")
                                sbSQL.AppendLine("EXEC @nRet =dbo.[ASSET_REPLACE_RESERVE_POSSIBLE_SP]")
                                sbSQL.AppendLine("            @IN_PRODUCT_NO")
                                sbSQL.AppendLine("            ,NULL")
                                sbSQL.AppendLine("            ,@IN_FROM_DT")
                                sbSQL.AppendLine("            ,@IN_TO_DT")
                                sbSQL.AppendLine("            ,@IN_KADO_JITU")
                                sbSQL.AppendLine("            ,@IN_CONST_NO")
                                sbSQL.AppendLine("            ,@IN_CONST_RESERVE_ID")
                                sbSQL.AppendLine("            ,@IN_REG_EMP_CD")
                                sbSQL.AppendLine("            ,@IN_REG_EMP_NAME")
                                sbSQL.AppendLine("            ,@IN_REG_TASK")
                                sbSQL.AppendLine(";")
                                sbSQL.AppendLine("SELECT @RETURN_VALUE = @nRet")

                                params.Add(New SqlParameter($"@IN_PRODUCT_NO", ProductNo))
                                params.Add(New SqlParameter($"@IN_FROM_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentStartDtString), 0, CInt(CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd")))))
                                params.Add(New SqlParameter($"@IN_TO_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentEndDtString), 0, CInt(CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd")))))
                                params.Add(New SqlParameter($"@IN_KADO_JITU", sWorkDayFlg))
                                params.Add(New SqlParameter($"@IN_CONST_NO", ViewModel.CONST_NO))
                                params.Add(New SqlParameter($"@IN_CONST_RESERVE_ID", resDetail.Rows(0)("RESERVE_ID")))
                                params.Add(New SqlParameter($"@IN_REG_EMP_CD", _LoginInfo.EmployeeCd))
                                params.Add(New SqlParameter($"@IN_REG_EMP_NAME", _LoginInfo.EmployeeNm))
                                params.Add(New SqlParameter($"@IN_REG_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

                                Dim returnValueParam = New SqlParameter()
                                returnValueParam.ParameterName = $"@RETURN_VALUE"
                                returnValueParam.SqlDbType = SqlDbType.Int
                                returnValueParam.Direction = ParameterDirection.Output
                                params.Add(returnValueParam)

                                db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                                returnValue = params.Where(Function(x) x.ParameterName = "@RETURN_VALUE").FirstOrDefault.Value

                                '代替品番予約更新ストアドプロシージャエラー
                                If returnValue = AssetReplaceReservePossibleSpResult.ChangeError Then
                                    tran.Rollback()
                                    Return Json(_Message.getMessage(CommonLabelConstructionList.MSG_CANNOT_RESERVE))
                                End If
                            Next

                            '明細が0件の場合は、当該予約データと予約明細データと予約メンテナンスデータをすべて削除する
                            '予約明細データ取得
                            sbSQL.Clear()
                            params = New List(Of SqlParameter)

                            sbSQL.AppendLine("SELECT * ")
                            sbSQL.AppendLine("FROM T_RESERVE_DETAIL")
                            sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                            params.Add(New SqlParameter($"@RESERVE_ID", resDetail.Rows(0)("RESERVE_ID")))
                            Dim chkModel As List(Of ReserveDetailModel) = db.ReserveDetail.SqlQuery(sbSQL.ToString, params.ToArray).ToList

                            If chkModel.Count = 0 Then
                                '予約削除
                                sbSQL.Clear()
                                params = New List(Of SqlParameter)

                                '削除処理
                                sbSQL.AppendLine("DELETE FROM T_RESERVE")
                                sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                                params.Add(New SqlParameter($"@RESERVE_ID", resDetail.Rows(0)("RESERVE_ID")))

                                db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                                '予約明細削除
                                sbSQL.Clear()
                                params = New List(Of SqlParameter)

                                '削除処理
                                sbSQL.AppendLine("DELETE FROM T_RESERVE_DETAIL")
                                sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                                params.Add(New SqlParameter($"@RESERVE_ID", resDetail.Rows(0)("RESERVE_ID")))

                                db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                                '予約メンテナンス削除
                                sbSQL.Clear()
                                params = New List(Of SqlParameter)

                                '削除処理
                                sbSQL.AppendLine("DELETE FROM T_RESERVE_MAINTE")
                                sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")

                                params.Add(New SqlParameter($"@RESERVE_ID", resDetail.Rows(0)("RESERVE_ID")))

                                db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

                            End If
                        Else

                            '予約明細が存在しない場合
                            '予約数0件の場合は登録しない
                            If ReserveNum = 0 Then
                                Return Json("ok")
                            End If

                            '予約ID取得
                            Dim intReserveId = Dao.ToolReserve.GetNextReserveId()

                            '予約テーブル登録
                            sbSQL.Clear()
                            params = New List(Of SqlParameter)

                            sbSQL.AppendLine("INSERT INTO T_RESERVE")
                            sbSQL.AppendLine("(")
                            sbSQL.AppendLine("    RESERVE_ID")
                            sbSQL.AppendLine("    ,REQ_EMP_CD")
                            sbSQL.AppendLine("    ,CHARGE_EMP_CD")
                            sbSQL.AppendLine("    ,REQUEST_DT")
                            sbSQL.AppendLine("    ,REAL_START_DT")
                            sbSQL.AppendLine("    ,REAL_END_DT")
                            sbSQL.AppendLine("    ,USER_START_DT")
                            sbSQL.AppendLine("    ,USER_END_DT")
                            sbSQL.AppendLine("    ,LEASE_PLACE_CD")
                            sbSQL.AppendLine("    ,LEASE_USE")
                            sbSQL.AppendLine("    ,MNG_LOC_L_CD")
                            sbSQL.AppendLine("    ,DEST_NAME1")
                            sbSQL.AppendLine("    ,REMARK")
                            sbSQL.AppendLine("    ,RESERVE_STATUS")
                            sbSQL.AppendLine("    ,INS_DATETIME")
                            sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,UPD_DATETIME")
                            sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,UPD_TASK")
                            sbSQL.AppendLine(") VALUES (")
                            sbSQL.AppendLine("     @RESERVE_ID")
                            sbSQL.AppendLine("    ,@REQ_EMP_CD")
                            sbSQL.AppendLine("    ,@CHARGE_EMP_CD")
                            sbSQL.AppendLine("    ,CONVERT(int, FORMAT(GetDate(), 'yyyyMMdd'))")
                            sbSQL.AppendLine("    ,@REAL_START_DT")
                            sbSQL.AppendLine("    ,@REAL_END_DT")
                            sbSQL.AppendLine("    ,@USER_START_DT")
                            sbSQL.AppendLine("    ,@USER_END_DT")
                            sbSQL.AppendLine("    ,' '")
                            sbSQL.AppendLine("    ,' '")
                            sbSQL.AppendLine("    ,@MNG_LOC_L_CD")
                            sbSQL.AppendLine("    ,' '")
                            sbSQL.AppendLine("    ,@REMARK")
                            sbSQL.AppendLine("    ,@RESERVE_STATUS")
                            sbSQL.AppendLine("    ,@INS_DATETIME")
                            sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,@UPD_DATETIME")
                            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                            sbSQL.AppendLine("    ,@UPD_TASK")
                            sbSQL.AppendLine(")")

                            params.Add(New SqlParameter($"@RESERVE_ID", intReserveId))
                            params.Add(New SqlParameter($"@REQ_EMP_CD", ViewModel.CONST_EMPLOYEE_CD))
                            params.Add(New SqlParameter($"@CHARGE_EMP_CD", ViewModel.CONST_EMPLOYEE_CD))
                            params.Add(New SqlParameter($"@REAL_START_DT", If(String.IsNullOrEmpty(ViewModel.ConstStartDtString), 0, CInt(CDate(ViewModel.ConstStartDtString).ToString("yyyyMMdd")))))
                            params.Add(New SqlParameter($"@REAL_END_DT", If(String.IsNullOrEmpty(ViewModel.ConstEndDtString), 0, CInt(CDate(ViewModel.ConstEndDtString).ToString("yyyyMMdd")))))
                            params.Add(New SqlParameter($"@USER_START_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentStartDtString), 0, CInt(CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd")))))
                            params.Add(New SqlParameter($"@USER_END_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentEndDtString), 0, CInt(CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd")))))
                            params.Add(New SqlParameter($"@MNG_LOC_L_CD", ViewModel.MNG_LOC_L_CD))
                            params.Add(New SqlParameter($"@REMARK", ViewModel.CONST_NO))
                            params.Add(New SqlParameter($"@RESERVE_STATUS", ReserveStatus.Mi))
                            params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                            params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                            params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))
                            Dim paramMaxId = New SqlParameter()
                            paramMaxId.ParameterName = $"@MAX_RESERVE_ID"
                            paramMaxId.SqlDbType = SqlDbType.Int
                            paramMaxId.Direction = ParameterDirection.Output
                            params.Add(paramMaxId)

                            db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                            For icnt As Integer = 0 To ReserveNum - 1
                                sbSQL.Clear()
                                params = New List(Of SqlParameter)
                                returnValue = 0

                                sbSQL.AppendLine("DECLARE @nRet INT;")
                                sbSQL.AppendLine("EXEC @nRet =dbo.[ASSET_REPLACE_RESERVE_POSSIBLE_SP]")
                                sbSQL.AppendLine("            @IN_PRODUCT_NO")
                                sbSQL.AppendLine("            ,NULL")
                                sbSQL.AppendLine("            ,@IN_FROM_DT")
                                sbSQL.AppendLine("            ,@IN_TO_DT")
                                sbSQL.AppendLine("            ,@IN_KADO_JITU")
                                sbSQL.AppendLine("            ,@IN_CONST_NO")
                                sbSQL.AppendLine("            ,@IN_CONST_RESERVE_ID")
                                sbSQL.AppendLine("            ,@IN_REG_EMP_CD")
                                sbSQL.AppendLine("            ,@IN_REG_EMP_NAME")
                                sbSQL.AppendLine("            ,@IN_REG_TASK")
                                sbSQL.AppendLine(";")
                                sbSQL.AppendLine("SELECT @RETURN_VALUE = @nRet")

                                params.Add(New SqlParameter($"@IN_PRODUCT_NO", ProductNo))
                                params.Add(New SqlParameter($"@IN_FROM_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentStartDtString), 0, CInt(CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd")))))
                                params.Add(New SqlParameter($"@IN_TO_DT", If(String.IsNullOrEmpty(ViewModel.ConstLentEndDtString), 0, CInt(CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd")))))
                                params.Add(New SqlParameter($"@IN_KADO_JITU", sWorkDayFlg))
                                params.Add(New SqlParameter($"@IN_CONST_NO", ViewModel.CONST_NO))
                                params.Add(New SqlParameter($"@IN_CONST_RESERVE_ID", intReserveId))
                                params.Add(New SqlParameter($"@IN_REG_EMP_CD", _LoginInfo.EmployeeCd))
                                params.Add(New SqlParameter($"@IN_REG_EMP_NAME", _LoginInfo.EmployeeNm))
                                params.Add(New SqlParameter($"@IN_REG_TASK", Me._Message.getMessage(CommonLabelConstructionList.TITLE)))

                                Dim returnValueParam = New SqlParameter()
                                returnValueParam.ParameterName = $"@RETURN_VALUE"
                                returnValueParam.SqlDbType = SqlDbType.Int
                                returnValueParam.Direction = ParameterDirection.Output
                                params.Add(returnValueParam)

                                db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                                returnValue = params.Where(Function(x) x.ParameterName = "@RETURN_VALUE").FirstOrDefault.Value

                                '代替品番予約更新ストアドプロシージャエラー
                                If returnValue = AssetReplaceReservePossibleSpResult.ChangeError Then
                                    tran.Rollback()
                                    Return Json(_Message.getMessage(CommonLabelConstructionList.MSG_CANNOT_RESERVE))
                                End If
                            Next
                        End If
                    Next

                    tran.Commit()

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)

                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using

            Return Json("ok")
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

#Region "入力設定情報取得"
        ''' <summary>
        ''' 入力設定情報取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetInputItem() As List(Of SearchInputItemModel)
            Dim tbl As DataTable = Dao.ToolSetting.GetConstructionInputItemInfo()
            Dim lst As New List(Of SearchInputItemModel)

            For Each row As DataRow In tbl.Rows
                'ドロップダウンデータ取得
                Dim dropTbl As DataTable = Dao.ToolSetting.GetItemDropDownInfo(row("DATA3").ToString)
                Dim dropLst As New List(Of DropdownModel)
                For Each rowDrop As DataRow In dropTbl.Rows
                    dropLst.Add(
                    New DropdownModel With {
                        .Cd = rowDrop("DATA2").ToString,
                        .Name = rowDrop("DATA1").ToString
                    }
                )
                Next

                lst.Add(
                    New SearchInputItemModel With {
                        .KEY1 = row("KEY1").ToString,
                        .DATA1 = row("DATA1").ToString,
                        .DATA2 = row("DATA2").ToString,
                        .DATA3 = row("DATA3").ToString,
                        .DropdownModel = dropLst,
                        .DropdownList = New SelectList(dropLst, "Cd", "Name", row("DATA2").ToString)
                    }
                )
            Next
            ViewBag.inputItemList = lst
            Return lst
        End Function
#End Region

#Region "検索条件情報取得"
        ''' <summary>
        ''' 検索条件情報取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetSearchItem() As List(Of SearchInputItemModel)
            Dim tbl As DataTable = Dao.ToolSetting.GetConstructionSearchItemInfo()
            Dim lst As New List(Of SearchInputItemModel)

            For Each row As DataRow In tbl.Rows
                'ドロップダウンデータ取得
                Dim dropTbl As DataTable = Dao.ToolSetting.GetItemDropDownInfo(row("DATA3").ToString)
                Dim dropLst As New List(Of DropdownModel)
                For Each rowDrop As DataRow In dropTbl.Rows
                    dropLst.Add(
                    New DropdownModel With {
                        .Cd = rowDrop("DATA2").ToString,
                        .Name = rowDrop("DATA1").ToString
                    }
                )
                Next

                lst.Add(
                    New SearchInputItemModel With {
                        .KEY1 = row("KEY1").ToString,
                        .DATA1 = row("DATA1").ToString,
                        .DATA2 = row("DATA2").ToString,
                        .DATA3 = row("DATA3").ToString,
                        .DropdownModel = dropLst,
                        .DropdownList = New SelectList(dropLst, "Cd", "Name", row("DATA2").ToString)
                    }
                )
            Next
            ViewBag.searchItemList = lst
            Return lst
        End Function
#End Region

#Region "仮置一覧画面表示処理"
        ''' <summary>
        ''' 仮置一覧画面表示処理
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function ShowTemporaryList(ByVal ViewModel As ConstructionListViewModel, ByVal SelectedConstNo As String) As ActionResult

            '検索条件をセッションに保存
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST_SEARCH_KEY) = ViewModel

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '入力内容をセッションに保存
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST) = _ViewModel

            '登録状態をセッションに保持
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_EDIT) = True

            'セッションクリア
            If Session(RedirectConstructionListInfo.ID) IsNot Nothing Then
                Session.Remove(RedirectConstructionListInfo.ID)
            End If

            'リダイレクト情報作成
            Dim info As New RedirectConstructionListInfo
            info.RedirectPage = RedirectConstructionListInfo.RedirectPageType.ConstructionList
            info.KeyValue = SelectedConstNo

            Session(RedirectConstructionListInfo.ID) = info

            Return RedirectToAction("Index", "TemporaryList")
        End Function
#End Region

#Region "仮置資産一覧画面表示処理"
        ''' <summary>
        ''' 仮置資産一覧画面表示処理
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function ShowTempAssetList(ByVal ViewModel As ConstructionListViewModel, ByVal SelectedConstNo As String, ByVal SelectedTempId As String) As ActionResult

            '検索条件をセッションに保存
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST_SEARCH_KEY) = ViewModel

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '入力内容をセッションに保存
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST) = _ViewModel

            '登録状態をセッションに保持
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_EDIT) = True

            'セッションクリア
            If Session(RedirectConstructionListInfo.ID) IsNot Nothing Then
                Session.Remove(RedirectConstructionListInfo.ID)
            End If

            Session.Remove(CommonSessionKey.CREAGE_TEMP_ASSET_LIST_KEY)
            Session(CommonSessionKey.CREAGE_TEMP_ASSET_LIST_KEY) = SelectedTempId

            'リダイレクト情報作成
            Dim info As New RedirectConstructionListInfo
            info.RedirectPage = RedirectConstructionListInfo.RedirectPageType.ConstructionList
            info.KeyValue = SelectedConstNo

            Session(RedirectConstructionListInfo.ID) = info

            Return RedirectToAction("Index", "TempAssetList")
        End Function
#End Region

#Region "資産一覧画面表示処理"
        ''' <summary>
        ''' 資産一覧画面表示処理
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function ShowConstAssetList(ByVal ViewModel As ConstructionListViewModel, ByVal SelectedConstNo As String) As ActionResult
            '検索条件をセッションに保存
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST_SEARCH_KEY) = ViewModel

            'セッションクリア
            If Session(RedirectConstructionListInfo.ID) IsNot Nothing Then
                Session.Remove(RedirectConstructionListInfo.ID)
            End If

            'リダイレクト情報作成
            Dim info As New RedirectConstructionListInfo
            info.RedirectPage = RedirectConstructionListInfo.RedirectPageType.ConstructionList
            info.KeyValue = SelectedConstNo

            Session(RedirectConstructionListInfo.ID) = info

            Return RedirectToAction("Index", "ConstAssetList")
        End Function
#End Region

#Region "工事履歴画面表示処理"
        ''' <summary>
        ''' 工事履歴画面表示処理
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function ShowConstructionHist(ByVal ViewModel As ConstructionListViewModel, ByVal SelectedConstNo As String) As ActionResult
            '検索条件をセッションに保存
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_LIST_SEARCH_KEY) = ViewModel

            'セッションクリア
            If Session(RedirectConstructionListInfo.ID) IsNot Nothing Then
                Session.Remove(RedirectConstructionListInfo.ID)
            End If

            'リダイレクト情報作成
            Dim info As New RedirectConstructionListInfo
            info.RedirectPage = RedirectConstructionListInfo.RedirectPageType.ConstructionList
            info.KeyValue = SelectedConstNo

            Session(RedirectConstructionListInfo.ID) = info

            Return RedirectToAction("Index", "ConstructionHist")
        End Function
#End Region
#Region "CSVマッピングマスタを取得する"
        ''' <summary>
        ''' CSVマッピングマスタを取得する
        ''' </summary>
        Private Function GetCsvMap() As String
            Dim tbl As New DataTable
            Dim sCharCd As String = String.Empty

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CSVMAP_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT CHAR_CD")
                sbSQL.AppendLine("FROM")
                sbSQL.AppendLine("M_CSVMAP")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("CSV_DIV = '" & CsvDiv.Construction & "'")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                If Not IsNothing(tbl) Then
                    sCharCd = tbl.Rows(0).Item("CHAR_CD")
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return sCharCd
        End Function
#End Region

#Region "CSVマッピング明細マスタを取得する"
        ''' <summary>
        ''' CSVマッピング明細マスタを取得する
        ''' </summary>
        Private Function GetCsvMapDetail() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CSVMAP_DETAILテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("  CSV_DIV ")
                sbSQL.Append("  , CSV_NO ")
                sbSQL.Append("  , TOOL_DIV ")
                sbSQL.Append("  , WORD_DIV ")
                sbSQL.Append("  , WORD_CD ")
                sbSQL.Append("  , REQUIRED_ITEM_FLAG ")
                sbSQL.Append("  , SPECIFIED_DIV ")
                sbSQL.Append("  , SPEC_CAN_DIV ")
                sbSQL.Append("  , CSV_COL_NO ")
                sbSQL.Append("  , CSV_FIX_VAL ")
                sbSQL.Append("  , INS_DATETIME ")
                sbSQL.Append("  , INS_EMPLOYEE_CD ")
                sbSQL.Append("  , INS_EMPLOYEE_NAME ")
                sbSQL.Append("  , UPD_DATETIME ")
                sbSQL.Append("  , UPD_EMPLOYEE_CD ")
                sbSQL.Append("  , UPD_EMPLOYEE_NAME ")
                sbSQL.Append("  , UPD_TASK ")
                sbSQL.Append("   ,TOOL_DIV + WORD_DIV + WORD_CD AS RenketsuWordCd ")
                sbSQL.Append("FROM ")
                sbSQL.Append("  M_CSVMAP_DETAIL ")
                sbSQL.Append("WHERE ")
                sbSQL.Append("CSV_DIV = '" & CsvDiv.Construction & "'")

                sbSQL.Append(" ORDER BY CSV_DIV,CSV_NO")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                Return tbl

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

#Region "CSVマッピング明細マスタの項目チェック"
        ''' <summary>
        ''' CSVマッピング明細マスタの項目チェック
        ''' </summary>
        ''' <param name="tblCsvMapMei">CSVマッピング明細マスタのデータテーブル</param>
        Private Function IsCsvMapMeiKoumokuOK(tblCsvMapMei As DataTable) As Boolean
            _lstCsvHed = New List(Of String)
            '工事マスタのCSVマッピング明細の項目定義を取得
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.Activity)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstNo)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstStartDt)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstEndDt)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstLentStartDt)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstLentEndDt)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstContents)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstEmployeeCd)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.MngLocLCd)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo1)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo2)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo3)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo4)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo5)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo6)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo7)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo8)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo9)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo10)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo11)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo12)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo13)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo14)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo15)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo16)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo17)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo18)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo19)
            _lstCsvHed.Add(CsvMapMei_Construction.CharCd.ConstInfo20)


            '項目数チェック
            If _lstCsvHed.Count <> tblCsvMapMei.Rows.Count Then
                Return False
            End If

            '項目値チェック
            For icnt As Integer = 0 To _lstCsvHed.Count - 1
                Dim row As DataRow()
                row = tblCsvMapMei.Select("RenketsuWordCd = '" & _lstCsvHed(icnt).ToString & "'")
                If row.Count = 0 Then
                    '該当項目がない場合、エラー
                    Return False
                End If
            Next

            Return True
        End Function
#End Region

#Region "指定したCSVファイルからデータを読み込み、登録する"
        ''' <summary>
        ''' 指定したCSVファイルからデータを読み込み、登録する
        ''' </summary>
        ''' <param name="path">読み取るファイルのパス</param>
        ''' <param name="enc">エンコード</param>
        ''' <param name="isKomokusuuErr">項目数エラーかどうか</param>
        ''' <param name="tblCsvMapMei">CSVマッピング明細マスタのデータテーブル</param>
        ''' <param name="sTmpTblNm">登録するデータベースのテーブル名</param>
        ''' <param name="db">db</param>
        ''' <returns>true:正常終了 false:エラー：</returns>
        ''' <remarks></remarks>
        Public Function ReadCsvFile(ByVal path As String, ByVal enc As System.Text.Encoding, ByRef isKomokusuuErr As Boolean, tblCsvMapMei As DataTable, sTmpTblNm As String, db As SystemDBContext) As Boolean

            Dim blErr As Boolean = False 'エラーがあるかどうか
            Dim sline As String = ""    'CSVファイル読込行データ格納用

            'CSV登録用データテーブル作成
            CreatetblImportCsv()

            'ロケーションマスタデータ取得
            Dim LocaData As DataTable
            LocaData = Dao.ToolLocation.GetLocationDataAll()

            '工事マスタデータ取得
            Dim tblConst As DataTable
            tblConst = Dao.ToolConstruction.GetConstructionDataAll()

            '社員マスタデータ取得
            Dim tblEmployee As DataTable
            tblEmployee = Dao.ToolEmployee.GetEmployeeDataAll()

            'CSVマッピング明細マスタで列番指定の項目数取得
            Dim Colrows() As DataRow = tblCsvMapMei.Select("CSV_COL_NO = MAX(CSV_COL_NO) ")
            Dim iColNoCnt As Integer = CInt(Colrows(0).Item("CSV_COL_NO"))

            '登録/更新可能な拠点情報を取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstructionList, _LoginInfo)
            Dim lstAuthLocation As List(Of String) = Dao.ToolCommon.GetAuthLocation(_LoginInfo, AuthInfo)


            '工事番号の自動生成設定を取得
            Dim ConstNoSelfReg As SettingInfo = _Message.getSettingInfo(CommonSettingKey.ConstNo_Self_Regist.KeyDiv,
                                                                  CommonSettingKey.ConstNo_Self_Regist.Key1,
                                                                  CommonSettingKey.ConstNo_Self_Regist.Key2,
                                                                  CommonSettingKey.ConstNo_Self_Regist.Key3, True)

            Dim constNoSeq As Integer = 0                               '工事番号 自動生成連番
            Dim constNoAutoRegFlg As Boolean = False                    '工事新規登録フラグ

            'ファイルから読み込み
            Using sr As StreamReader = New StreamReader(path, enc)
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

                    ''●●●
                    'If (iRowCnt Mod 10000) = 0 Then
                    '    logger.Info("STEP3 " & (iRowCnt + 1).ToString & "  " & Now.ToString("yyyy/MM/dd HH:mm:ss"))
                    'End If

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
                    If fields.Length < iColNoCnt Then
                        isKomokusuuErr = True
                        Return blErr
                    End If

                    _dtCsvImport.Clear()    'データテーブルクリア
                    'データ行追加
                    Dim newrow As DataRow = _dtCsvImport.NewRow
                    For iCol As Integer = 0 To _lstCsvHed.Count - 1
                        Dim sStr As String = String.Empty
                        Dim rows As DataRow() = tblCsvMapMei.Select("RenketsuWordCd = '" & _lstCsvHed(iCol).ToString & "'")
                        If rows(0).Item("SPECIFIED_DIV") = CsvMapping_SpecifiedDiv.Index Then
                            If CommonRfid.ToStringNvl(rows(0).Item("CSV_COL_NO")).Trim = String.Empty OrElse CInt(CommonRfid.ToStringNvl(rows(0).Item("CSV_COL_NO")).Trim) = 0 Then
                                sStr = String.Empty
                            Else
                                sStr = fields(CInt(rows(0).Item("CSV_COL_NO")) - 1).Trim.TrimStart("'"c)
                            End If
                        ElseIf rows(0).Item("SPECIFIED_DIV") = CsvMapping_SpecifiedDiv.Kotei Then
                            sStr = tblCsvMapMei(iCol).Item("CSV_FIX_VAL").ToString
                        End If
                        newrow(iCol) = sStr
                    Next
                    _dtCsvImport.Rows.Add(newrow)

                    '工事番号の自動生成フラグを初期化
                    constNoAutoRegFlg = False

                    '項目値のエラーチェック
                    '※constNoAutoRegFlg, constNoSeqはIsDataErrorLine内で値を設定
                    If Not IsDataErrorLine(iRowCnt, tblConst, tblEmployee, LocaData, lstAuthLocation, ConstNoSelfReg, constNoAutoRegFlg, constNoSeq) Then
                        'エラーの場合、登録処理は行わない
                        sline = sr.ReadLine()
                        blErr = True
                        Continue Do
                    End If

                    '登録処理
                    If Not blErr Then
                        Try
                            InsertWork(iRowCnt, sTmpTblNm, db, constNoAutoRegFlg, constNoSeq)
                        Catch ex As Exception
                            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                        End Try
                    End If

                    sline = sr.ReadLine()
                Loop

                _iCsvImportRowCount = iRowCnt - 1

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

#Region "項目値のエラーチェック"
        ''' <summary>
        ''' 項目値のエラーチェック
        ''' </summary>
        ''' <param name="iRowCnt">データ行</param>
        ''' <param name="tblConst">工事マスタのデータテーブル</param>
        ''' <param name="tblEmployee">社員マスタのデータテーブル</param>
        ''' <param name="LocaData">ロケーションマスタのデータテーブル</param>
        ''' <param name="lstAuthLocation">登録/更新可能な拠点リスト</param>
        ''' <param name="ConstNoSelfReg">設定M資産番号 自動生成設定</param>
        ''' <param name="constNoAutoRegFlg">工事番号自動生成フラグ</param>
        ''' <param name="constNoSeq">工事番号 自動生成連番値</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function IsDataErrorLine(ByVal iRowCnt As Integer,
                                         ByVal tblConst As DataTable,
                                         ByVal tblEmployee As DataTable,
                                         ByVal LocaData As DataTable,
                                         ByVal lstAuthLocation As List(Of String),
                                         ByVal ConstNoSelfReg As SettingInfo,
                                         ByRef constNoAutoRegFlg As Boolean,
                                         ByRef constNoSeq As Integer
                                         ) As Boolean

            Dim blRet As Boolean = True

            Dim db As New SystemDBContext

            'オブジェクトの編集開始
            Me._dtCsvImport.Rows(0).BeginEdit()

            '--データチェック
            'Datatable より、検索する
            Dim rowsConst() As DataRow = tblConst.Select("CONST_NO = '" & Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstNo) & "'")

            '既存DBデータ区分をセット
            Dim enumDbDataKbn As DbDataKbn
            If rowsConst.Count = 0 Then
                enumDbDataKbn = DbDataKbn.Non
            Else
                enumDbDataKbn = DbDataKbn.Exist
            End If

            '該当の管理ロケーションマスタデータを取得する
            Dim MngLocLrow As DataRow() = Nothing  '大区分
            If Not Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.MngLocLCd).ToString.Trim = String.Empty Then
                MngLocLrow = LocaData.Select("LOC_L_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.MngLocLCd).ToString.Trim + "' AND LOC_DIV = '" + LOC_DIV.LocationL + "'")
            End If

            '該当の社員ロケーションマスタデータを取得する
            Dim Employeerow As DataRow() = Nothing
            If Not Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstEmployeeCd).ToString.Trim = String.Empty Then
                Employeerow = tblEmployee.Select("EMPLOYEE_CD = '" + Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstEmployeeCd).ToString.Trim + "'")
            End If

            '入力リストデータ取得
            Dim inputItemList = GetInputItem()
            Dim inputItem = New SearchInputItemModel

            'チェック用に工事開始日、工事終了日、工事貸出開始日、工事貸出終了日を保存
            Dim constStartDt As DateTime
            Dim constEndDt As DateTime
            Dim constLentStartDt As DateTime
            Dim constLentEndDt As DateTime

            Dim ConstNo As String = Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstNo).ToString.Trim
            Dim shoriKbn As String = Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.Activity).ToString.Trim            '処理区分を事前に取得
            For iCalCnt As Integer = 0 To _lstCsvHed.Count - 1

                '新規行がなければ追加する
                If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                    Dim lstRow As DataInportResult = New DataInportResult
                    Me._lstErr.Add(lstRow)
                End If

                '処理区分が削除の場合
                If shoriKbn = CsvImportProcDiv.Delete Then

                    '項目が処理区分と工事番号と、管理ロケ大区分コード以外
                    If Not (iCalCnt = CsvMapMei_Construction.ColIdx.Activity OrElse iCalCnt = CsvMapMei_Construction.ColIdx.ConstNo OrElse iCalCnt = CsvMapMei_Construction.ColIdx.MngLocLCd) Then
                        'チェックをしない
                        Continue For
                    End If
                End If


                '列によってチェック条件・設定項目値を分ける
                Dim sStr As String = Me._dtCsvImport.Rows(0).Item(iCalCnt).ToString.Trim
                Select Case iCalCnt
                    Case CsvMapMei_Construction.ColIdx.Activity

                        '処理区分
                        '空白チェック
                        If String.IsNullOrEmpty(sStr) Then
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_DIV)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                            blRet = False
                            '区分値チェック
                        ElseIf sStr <> CsvImportProcDiv.Insert AndAlso sStr <> CsvImportProcDiv.Update AndAlso sStr <> CsvImportProcDiv.Delete Then
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_DIV)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DIV_VAL)
                            blRet = False
                        End If

                    Case CsvMapMei_Construction.ColIdx.ConstNo

                        '工事番号
                        If ConstNoSelfReg.DATA1 = ValidFlg.Cd.Valid Then
                            If sStr <> "" AndAlso _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.Activity).ToString.Trim = CsvImportProcDiv.Insert Then
                                '工事番号入力済み　かつ　処理区分：I(追加)
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_NO)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_CONST_NO_AUTO_GENERATE_NOT_INPUT)
                                blRet = False
                            ElseIf sStr = "" Then
                                '自動生成が必要かチェック
                                '※以下の場合に自動生成ありとなる
                                '1. 処理区分：I(追加)　かつ　資産番号が未入力
                                constNoAutoRegFlg = _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.Activity).ToString.Trim = CsvImportProcDiv.Insert

                                If constNoAutoRegFlg Then
                                    '自動生成あり
                                    If constNoSeq <= 0 Then
                                        '連番未採番なら採番する
                                        constNoSeq = ConstNoAutoNumberingSeq()
                                    Else
                                        '連番を加算
                                        constNoSeq += 1
                                    End If

                                    '自動生成される場合は以降のチェックは実施なし
                                    Continue For
                                End If
                            End If
                        End If

                        If String.IsNullOrEmpty(sStr) Then
                            '空白チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_NO)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                            blRet = False
                        ElseIf sStr.Length > itemlength.ConstNo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_NO)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                            '半角英数字チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_NO)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                            blRet = False
                        Else
                            '工事番号存在チェック
                            Dim model As ConstructionListModel = db.ConstructionList.Find(sStr)

                            Select Case Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.Activity).ToString.Trim
                                '処理区分：I(追加)の場合
                                Case CsvImportProcDiv.Insert
                                    '工事番号が存在している場合エラー
                                    If Not IsNothing(model) Then
                                        If ConstNoSelfReg.DATA1 <> ValidFlg.Cd.Valid Then       ' 工事番号が自動生成ではない場合、エラー判定する
                                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_NO)
                                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_CONST_NO) & "[{0}]", sStr)
                                            blRet = False
                                        End If
                                    End If
                                '処理区分：U(更新)
                                Case CsvImportProcDiv.Update
                                    '工事番号が存在しない場合エラー
                                    If IsNothing(model) Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_NO)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXISTS_CONST_NO)
                                        blRet = False
                                    End If
                                '処理区分：D(削除)
                                Case CsvImportProcDiv.Delete
                                    '工事番号が存在しない場合エラー
                                    If IsNothing(model) Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_NO)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXISTS_CONST_NO)
                                        blRet = False
                                    End If

                                    '仮置き登録されている場合はエラー
                                    Dim sbChkSQL As StringBuilder = New StringBuilder

                                    sbChkSQL.AppendLine("SELECT *")
                                    sbChkSQL.AppendLine("FROM T_TEMPORARY")
                                    sbChkSQL.AppendLine("WHERE REG_CONST_NO ='" & ConstNo & "'")

                                    'SQLを実行する
                                    Dim chkModel As List(Of TemporaryListModel) = db.TemporaryList.SqlQuery(sbChkSQL.ToString).ToList

                                    If chkModel.Count > 0 Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_NO)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelConstructionList.MSG_EXIST_REG_CONST_NO)
                                        blRet = False
                                    End If
                            End Select
                        End If

                    Case CsvMapMei_Construction.ColIdx.ConstStartDt
                        '工事開始日
                        If String.IsNullOrEmpty(sStr) Then
                            '空白チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_START_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                            blRet = False
                        ElseIf sStr.Length > itemlength.LeaseLimit + 2 Then '/が入るのでその分プラスする。
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_START_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                            '半角英数字記号チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_START_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                            blRet = False
                        Else
                            '日付チェック
                            'DateTimeに変換できるか確かめる
                            If Not DateTime.TryParse(sStr, constStartDt) Then
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_START_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If

                            '既存がある時、既存と値が違えば変更フラグをtrueにする
                            If enumDbDataKbn = DbDataKbn.Exist Then
                                If Not CommonRfid.ToStringNvl(rowsConst(0).Item("CONST_START_DT")) = constStartDt.ToString("yyyyMMdd") Then
                                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
                                End If
                            End If
                        End If
                        '1000年未満の場合エラー
                        ' / を取り除いて数字だけにする
                        Dim sCleanStr As String = System.Text.RegularExpressions.Regex.Replace(sStr, "[^0-9]", "")
                        Dim num As Integer

                        ' TryParseで日付チェック ＆ 年が1000未満ならエラー
                        If Integer.TryParse(sCleanStr, num) Then
                            If num < MIN_DATE Then
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_START_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_INCORRECT_DATE)
                                blRet = False
                            End If

                        End If


                    Case CsvMapMei_Construction.ColIdx.ConstEndDt
                        '工事終了日
                        If String.IsNullOrEmpty(sStr) Then
                            '空白チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_END_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                            blRet = False
                        ElseIf sStr.Length > itemlength.LeaseLimit + 2 Then '/が入るのでその分プラスする。
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_END_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                            '半角英数字記号チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_END_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                            blRet = False
                        Else
                            '日付チェック
                            'DateTimeに変換できるか確かめる
                            If Not DateTime.TryParse(sStr, constEndDt) Then
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_END_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf constStartDt > constEndDt Then
                                '工事開始日＞工事終了日の場合エラー
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_END_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_CONST_END_DT_KAKOBI)
                                blRet = False
                            End If

                            '既存がある時、既存と値が違えば変更フラグをtrueにする
                            If enumDbDataKbn = DbDataKbn.Exist Then
                                If Not CommonRfid.ToStringNvl(rowsConst(0).Item("CONST_END_DT")) = constEndDt.ToString("yyyyMMdd") Then
                                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
                                End If
                            End If
                        End If

                        '1000年未満の場合エラー
                        ' / を取り除いて数字だけにする
                        Dim sCleanStr As String = System.Text.RegularExpressions.Regex.Replace(sStr, "[^0-9]", "")
                        Dim num As Integer

                        ' TryParseで日付チェック ＆ 年が1000未満ならエラー
                        If Integer.TryParse(sCleanStr, num) Then
                            If num < MIN_DATE Then
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_END_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_INCORRECT_DATE)
                                blRet = False
                            End If

                        End If

                    Case CsvMapMei_Construction.ColIdx.ConstLentStartDt
                        '工事貸出開始日
                        If String.IsNullOrEmpty(sStr) Then
                            '空白チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                            blRet = False
                        ElseIf sStr.Length > itemlength.LeaseLimit + 2 Then '/が入るのでその分プラスする。
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                            '半角英数字記号チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                            blRet = False
                        Else
                            '日付チェック
                            'DateTimeに変換できるか確かめる
                            If Not DateTime.TryParse(sStr, constLentStartDt) Then
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf constStartDt < constLentStartDt Then
                                '工事開始日＜工事貸出開始日の場合エラー
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_CONST_LENT_START_DT_MIRAIBI)
                                blRet = False
                            End If

                            '既存がある時、既存と値が違えば変更フラグをtrueにする
                            If enumDbDataKbn = DbDataKbn.Exist Then
                                If Not CommonRfid.ToStringNvl(rowsConst(0).Item("CONST_LENT_START_DT")) = constLentStartDt.ToString("yyyyMMdd") Then
                                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
                                End If
                            End If
                        End If

                        '1000年未満の場合エラー
                        ' / を取り除いて数字だけにする
                        Dim sCleanStr As String = System.Text.RegularExpressions.Regex.Replace(sStr, "[^0-9]", "")
                        Dim num As Integer

                        ' TryParseで日付チェック ＆ 年が1000未満ならエラー
                        If Integer.TryParse(sCleanStr, num) Then
                            If num < MIN_DATE Then
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_START_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_INCORRECT_DATE)
                                blRet = False
                            End If

                        End If

                    Case CsvMapMei_Construction.ColIdx.ConstLentEndDt
                        '工事貸出終了日
                        If String.IsNullOrEmpty(sStr) Then
                            '空白チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                            blRet = False
                        ElseIf sStr.Length > itemlength.LeaseLimit + 2 Then '/が入るのでその分プラスする。
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                            '半角英数字記号チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                            blRet = False
                        Else
                            '日付チェック
                            'DateTimeに変換できるか確かめる
                            If Not DateTime.TryParse(sStr, constLentEndDt) Then
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf constLentStartDt > constLentEndDt Then
                                '工事貸出開始日＞工事貸出終了日の場合エラー
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_CONST_LENT_END_DT_KAKOBI)
                                blRet = False
                            ElseIf constEndDt > constLentEndDt Then
                                '工事終了日＞工事貸出終了日の場合エラー
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_CONST_LENT_END_DT_KAKOBI_CONST_END_DT)
                                blRet = False
                            End If

                            '既存がある時、既存と値が違えば変更フラグをtrueにする
                            If enumDbDataKbn = DbDataKbn.Exist Then
                                If Not CommonRfid.ToStringNvl(rowsConst(0).Item("CONST_END_DT")) = constLentEndDt.ToString("yyyyMMdd") Then
                                    Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
                                End If
                            End If
                        End If

                        '1000年未満の場合エラー
                        ' / を取り除いて数字だけにする
                        Dim sCleanStr As String = System.Text.RegularExpressions.Regex.Replace(sStr, "[^0-9]", "")
                        Dim num As Integer

                        ' TryParseで日付チェック ＆ 年が1000未満ならエラー
                        If Integer.TryParse(sCleanStr, num) Then
                            If num < MIN_DATE Then
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_LENT_END_DT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_INCORRECT_DATE)
                                blRet = False
                            End If

                        End If

                    Case CsvMapMei_Construction.ColIdx.ConstContents
                        '工事内容
                        '既存がある時、既存と値が違えば変更フラグをtrueにする
                        If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsConst(0).Item("CONST_CONTENTS")).Trim = CommonRfid.ToStringNvl(sStr) Then
                            Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
                        End If

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstContents Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_CONTENTS)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                    Case CsvMapMei_Construction.ColIdx.ConstEmployeeCd
                        '工事責任者コード
                        '既存がある時、既存と値が違えば変更フラグをtrueにする
                        If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsConst(0).Item("CONST_EMPLOYEE_CD")).Trim = CommonRfid.ToStringNvl(sStr) Then
                            Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
                        End If

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstEmployeeCd Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelConstructionList.LABEL_CONST_EMPLOYEE_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        ElseIf Employeerow Is Nothing OrElse Employeerow.Length = 0 Then
                            '社員マスタに登録されているロケーションコードかチェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelConstructionList.LABEL_CONST_EMPLOYEE_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                            blRet = False
                        Else
                            'エラーがない場合は工事責任者氏名を設定
                            Me._dtCsvImport.Rows(0).Item("CONST_EMPLOYEE_NAME") = Employeerow(0)("EMPLOYEE_NAME").ToString
                        End If

                    Case CsvMapMei_Construction.ColIdx.MngLocLCd
                        '管理ロケ区分コード
                        '既存がある時、既存と値が違えば変更フラグをtrueにする
                        If enumDbDataKbn = DbDataKbn.Exist AndAlso Not CommonRfid.ToStringNvl(rowsConst(0).Item("MNG_LOC_L_CD")).Trim = CommonRfid.ToStringNvl(sStr) Then    '2019/12/19 nv miyaji
                            Me._dtCsvImport.Rows(0).Item("UPD_FLG") = True
                        End If

                        If String.IsNullOrEmpty(sStr) Then
                            '空白チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelConstructionList.LABEL_MNG_LOC_L_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                            blRet = False
                        ElseIf sStr.Length > itemlength.MngLocLCd Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelConstructionList.LABEL_MNG_LOC_L_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        ElseIf Not CommonRfid.IsHankaku(sStr) Then
                            '半角英数字チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelConstructionList.LABEL_MNG_LOC_L_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                            blRet = False
                        ElseIf MngLocLrow Is Nothing OrElse MngLocLrow.Length = 0 Then
                            'ロケーションマスタに登録されているロケーションコードかチェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelConstructionList.LABEL_MNG_LOC_L_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                            blRet = False
                        ElseIf Not lstAuthLocation.Contains(sStr) Then
                            '登録/更新権限のあるロケーションコードかチェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelConstructionList.LABEL_MNG_LOC_L_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_AUTH_LOCATION)
                            blRet = False
                        Else
                            Select Case Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.Activity).ToString.Trim
                                '処理区分：U(更新)、D(削除)
                                Case CsvImportProcDiv.Update, CsvImportProcDiv.Delete
                                    '工事情報を取得
                                    Dim model As ConstructionListModel = db.ConstructionList.Find(Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstNo).ToString.Trim)
                                    '登録/更新権限のあるロケーションコードかチェック
                                    If Not IsNothing(model) AndAlso Not lstAuthLocation.Contains(model.MNG_LOC_L_CD) Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelConstructionList.LABEL_MNG_LOC_L_CD)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_AUTH_LOCATION)
                                        blRet = False
                                    End If
                            End Select
                        End If

                    Case CsvMapMei_Construction.ColIdx.ConstInfo1
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容3にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO1)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO1)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo1) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo2
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO2)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO2)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo2) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo2
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO2)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO2)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo2) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo3
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO3)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO3)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo3) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo4
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO4)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO4)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo4) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo5
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO5)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO5)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo5) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo6
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO6)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO6)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo6) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo7
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO7)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO7)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo7) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo8
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "8").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO8)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO8)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo8) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo9
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "9").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO9)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO9)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo9) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo10
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "10").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO10)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO10)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo10) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo11
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "11").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO11)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO11)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo11) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo12
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "12").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO12)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO12)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo12) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo13
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "13").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO13)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO13)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo13) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo14
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "14").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO14)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO14)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo14) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo15
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "15").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO15)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO15)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo15) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo16
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "16").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO16)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO16)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo16) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo17
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "17").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO17)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO17)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo17) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo18
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "18").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO18)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO18)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo18) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo19
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "19").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO19)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO19)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo19) = sStr

                    Case CsvMapMei_Construction.ColIdx.ConstInfo20
                        '設定マスタの入力内容を取得
                        inputItem = inputItemList.Where(Function(x) x.KEY1 = "20").FirstOrDefault

                        Select Case inputItem.DATA1
                            Case InputType.TEXT, InputType.LABEL
                                '設定マスタに固定値が設定されている場合変換
                                If Not String.IsNullOrEmpty(inputItem.DATA2) Then
                                    sStr = inputItem.DATA2
                                End If
                            Case InputType.AREA
                                'データに「$$$」がない場合は、データの最後に「$$$」を付与する
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not sStr.Contains(DELIMITER) Then
                                    sStr += DELIMITER
                                End If
                            Case InputType.LIST
                                '設定M.内容4にセットされているKEY種別の内容1に設定されたデータがなれけばエラー
                                If Not String.IsNullOrEmpty(sStr) AndAlso Not inputItem.DropdownModel.Any(Function(x) x.Cd = sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO20)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                                    blRet = False
                                End If
                        End Select

                        If String.IsNullOrEmpty(sStr) Then
                            '空白ならチェックしない
                            Continue For
                        End If
                        If sStr.Length > itemlength.ConstInfo Then
                            '項目長チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CONST_INFO20)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                            blRet = False
                        End If

                        'エラーがなければCSVデータを更新
                        Me._dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo20) = sStr

                End Select
            Next

            'CSVにない項目の設定
            'データ項目にエラーがない場合
            If blRet Then
                If enumDbDataKbn = DbDataKbn.Non Then
                    '新規
                    Me._dtCsvImport.Rows(0).Item("CONST_STATUS") = CONST_STATUS.Incomplete
                    Me._dtCsvImport.Rows(0).Item("PREV_TEMP_NUM") = 0
                    Me._dtCsvImport.Rows(0).Item("CONST_IN_NUM") = 0
                    Me._dtCsvImport.Rows(0).Item("NOW_TEMP_NUM") = 0
                    Me._dtCsvImport.Rows(0).Item("CONST_OUT_NUM") = 0
                    Me._dtCsvImport.Rows(0).Item("CUR_LENT_NUM") = 0
                    Me._dtCsvImport.Rows(0).Item("CUR_TEMP_NUM") = 0
                    Me._dtCsvImport.Rows(0).Item("SITE_EMPLOYEE_CD") = DBNull.Value
                    Me._dtCsvImport.Rows(0).Item("SITE_EMPLOYEE_NAME") = DBNull.Value
                    Me._dtCsvImport.Rows(0).Item("CONST_CONFIRM_TS") = DBNull.Value
                    Me._dtCsvImport.Rows(0).Item("CONST_CONFIRM_EMPLOYEE_CD") = DBNull.Value
                    Me._dtCsvImport.Rows(0).Item("CONST_CONFIRM_EMPLOYEE_NAME") = DBNull.Value
                    Me._dtCsvImport.Rows(0).Item("CONST_CONFIRM_REMARK") = DBNull.Value

                    Me._dtCsvImport.Rows(0).Item("TASK_TS") = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))
                    Me._dtCsvImport.Rows(0).Item("INS_DATETIME") = CommonRfid.getNowDate
                    Me._dtCsvImport.Rows(0).Item("INS_EMPLOYEE_CD") = _LoginInfo.EmployeeCd
                    Me._dtCsvImport.Rows(0).Item("INS_EMPLOYEE_NAME") = _LoginInfo.EmployeeNm
                    Me._dtCsvImport.Rows(0).Item("UPD_DATETIME") = CommonRfid.getNowDate
                    Me._dtCsvImport.Rows(0).Item("UPD_EMPLOYEE_CD") = _LoginInfo.EmployeeCd
                    Me._dtCsvImport.Rows(0).Item("UPD_EMPLOYEE_NAME") = _LoginInfo.EmployeeNm
                    Me._dtCsvImport.Rows(0).Item("UPD_TASK") = Me._Message.getMessage(CommonLabelConstructionList.TITLE)
                Else
                    '新規以外
                    Me._dtCsvImport.Rows(0).Item("CONST_STATUS") = rowsConst(0).Item("CONST_STATUS")
                    Me._dtCsvImport.Rows(0).Item("PREV_TEMP_NUM") = rowsConst(0).Item("PREV_TEMP_NUM")
                    Me._dtCsvImport.Rows(0).Item("CONST_IN_NUM") = rowsConst(0).Item("CONST_IN_NUM")
                    Me._dtCsvImport.Rows(0).Item("NOW_TEMP_NUM") = rowsConst(0).Item("NOW_TEMP_NUM")
                    Me._dtCsvImport.Rows(0).Item("CONST_OUT_NUM") = rowsConst(0).Item("CONST_OUT_NUM")
                    Me._dtCsvImport.Rows(0).Item("CUR_LENT_NUM") = rowsConst(0).Item("CUR_LENT_NUM")
                    Me._dtCsvImport.Rows(0).Item("CUR_TEMP_NUM") = rowsConst(0).Item("CUR_TEMP_NUM")
                    Me._dtCsvImport.Rows(0).Item("SITE_EMPLOYEE_CD") = rowsConst(0).Item("SITE_EMPLOYEE_CD")
                    Me._dtCsvImport.Rows(0).Item("SITE_EMPLOYEE_NAME") = rowsConst(0).Item("SITE_EMPLOYEE_NAME")
                    Me._dtCsvImport.Rows(0).Item("CONST_CONFIRM_TS") = rowsConst(0).Item("CONST_CONFIRM_TS")
                    Me._dtCsvImport.Rows(0).Item("CONST_CONFIRM_EMPLOYEE_CD") = rowsConst(0).Item("CONST_CONFIRM_EMPLOYEE_CD")
                    Me._dtCsvImport.Rows(0).Item("CONST_CONFIRM_EMPLOYEE_NAME") = rowsConst(0).Item("CONST_CONFIRM_EMPLOYEE_NAME")
                    Me._dtCsvImport.Rows(0).Item("CONST_CONFIRM_REMARK") = rowsConst(0).Item("CONST_CONFIRM_REMARK")
                    Me._dtCsvImport.Rows(0).Item("TASK_TS") = Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))
                    Me._dtCsvImport.Rows(0).Item("INS_DATETIME") = rowsConst(0).Item("INS_DATETIME")
                    Me._dtCsvImport.Rows(0).Item("INS_EMPLOYEE_CD") = rowsConst(0).Item("INS_EMPLOYEE_CD")
                    Me._dtCsvImport.Rows(0).Item("INS_EMPLOYEE_NAME") = rowsConst(0).Item("INS_EMPLOYEE_NAME")

                    Me._dtCsvImport.Rows(0).Item("UPD_DATETIME") = CommonRfid.getNowDate
                    Me._dtCsvImport.Rows(0).Item("UPD_EMPLOYEE_CD") = _LoginInfo.EmployeeCd
                    Me._dtCsvImport.Rows(0).Item("UPD_EMPLOYEE_NAME") = _LoginInfo.EmployeeNm
                    Me._dtCsvImport.Rows(0).Item("UPD_TASK") = Me._Message.getMessage(CommonLabelConstructionList.TITLE)
                End If
            End If

            'オブジェクトの編集終了
            Me._dtCsvImport.Rows(0).EndEdit()

            Return blRet

        End Function
#End Region

#Region "一時表に登録"
        ''' <summary>
        ''' 一時表に登録
        ''' </summary>
        ''' <param name="icnt">登録する行</param>
        ''' <param name="sTmpTblNm">登録するデータテーブルのテーブル名</param>
        ''' <param name="db">db</param>
        Public Sub InsertWork(icnt As Integer, sTmpTblNm As String, db As SystemDBContext, constNoAutoRegFlg As Boolean, constNoSeq As Integer)
            '--登録処理開始
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)

            Try
                'Insert SQL 作成
                sbSQL.AppendLine("INSERT INTO " & sTmpTblNm)
                sbSQL.AppendLine("(")

                sbSQL.AppendLine("     ACTIVITY")
                sbSQL.AppendLine("    ,CONST_NO")
                sbSQL.AppendLine("    ,CONST_START_DT")
                sbSQL.AppendLine("    ,CONST_END_DT")
                sbSQL.AppendLine("    ,CONST_LENT_START_DT")
                sbSQL.AppendLine("    ,CONST_LENT_END_DT")
                sbSQL.AppendLine("    ,CONST_STATUS")
                sbSQL.AppendLine("    ,CONST_CONTENTS")
                sbSQL.AppendLine("    ,CONST_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,CONST_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,MNG_LOC_L_CD")
                sbSQL.AppendLine("    ,CONST_INFO1")
                sbSQL.AppendLine("    ,CONST_INFO2")
                sbSQL.AppendLine("    ,CONST_INFO3")
                sbSQL.AppendLine("    ,CONST_INFO4")
                sbSQL.AppendLine("    ,CONST_INFO5")
                sbSQL.AppendLine("    ,CONST_INFO6")
                sbSQL.AppendLine("    ,CONST_INFO7")
                sbSQL.AppendLine("    ,CONST_INFO8")
                sbSQL.AppendLine("    ,CONST_INFO9")
                sbSQL.AppendLine("    ,CONST_INFO10")
                sbSQL.AppendLine("    ,CONST_INFO11")
                sbSQL.AppendLine("    ,CONST_INFO12")
                sbSQL.AppendLine("    ,CONST_INFO13")
                sbSQL.AppendLine("    ,CONST_INFO14")
                sbSQL.AppendLine("    ,CONST_INFO15")
                sbSQL.AppendLine("    ,CONST_INFO16")
                sbSQL.AppendLine("    ,CONST_INFO17")
                sbSQL.AppendLine("    ,CONST_INFO18")
                sbSQL.AppendLine("    ,CONST_INFO19")
                sbSQL.AppendLine("    ,CONST_INFO20")
                sbSQL.AppendLine("    ,PREV_TEMP_NUM")
                sbSQL.AppendLine("    ,CONST_IN_NUM")
                sbSQL.AppendLine("    ,NOW_TEMP_NUM")
                sbSQL.AppendLine("    ,CONST_OUT_NUM")
                sbSQL.AppendLine("    ,CUR_LENT_NUM")
                sbSQL.AppendLine("    ,CUR_TEMP_NUM")
                sbSQL.AppendLine("    ,SITE_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,SITE_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,CONST_CONFIRM_TS")
                sbSQL.AppendLine("    ,CONST_CONFIRM_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,CONST_CONFIRM_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,CONST_CONFIRM_REMARK")
                sbSQL.AppendLine("    ,TASK_TS")
                sbSQL.AppendLine("    ,INS_DATETIME")
                sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,UPD_DATETIME")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,UPD_TASK")
                sbSQL.AppendLine("    ,UPD_FLG")
                sbSQL.AppendLine("    ,CSV_ROW_NUM")
                sbSQL.AppendLine(") VALUES (")

                sbSQL.AppendLine("     @ACTIVITY")
                sbSQL.AppendLine("    ,@CONST_NO")
                sbSQL.AppendLine("    ,@CONST_START_DT")
                sbSQL.AppendLine("    ,@CONST_END_DT")
                sbSQL.AppendLine("    ,@CONST_LENT_START_DT")
                sbSQL.AppendLine("    ,@CONST_LENT_END_DT")
                sbSQL.AppendLine("    ,@CONST_STATUS")
                sbSQL.AppendLine("    ,@CONST_CONTENTS")
                sbSQL.AppendLine("    ,@CONST_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@CONST_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,@MNG_LOC_L_CD")
                sbSQL.AppendLine("    ,@CONST_INFO1")
                sbSQL.AppendLine("    ,@CONST_INFO2")
                sbSQL.AppendLine("    ,@CONST_INFO3")
                sbSQL.AppendLine("    ,@CONST_INFO4")
                sbSQL.AppendLine("    ,@CONST_INFO5")
                sbSQL.AppendLine("    ,@CONST_INFO6")
                sbSQL.AppendLine("    ,@CONST_INFO7")
                sbSQL.AppendLine("    ,@CONST_INFO8")
                sbSQL.AppendLine("    ,@CONST_INFO9")
                sbSQL.AppendLine("    ,@CONST_INFO10")
                sbSQL.AppendLine("    ,@CONST_INFO11")
                sbSQL.AppendLine("    ,@CONST_INFO12")
                sbSQL.AppendLine("    ,@CONST_INFO13")
                sbSQL.AppendLine("    ,@CONST_INFO14")
                sbSQL.AppendLine("    ,@CONST_INFO15")
                sbSQL.AppendLine("    ,@CONST_INFO16")
                sbSQL.AppendLine("    ,@CONST_INFO17")
                sbSQL.AppendLine("    ,@CONST_INFO18")
                sbSQL.AppendLine("    ,@CONST_INFO19")
                sbSQL.AppendLine("    ,@CONST_INFO20")
                sbSQL.AppendLine("    ,@PREV_TEMP_NUM")
                sbSQL.AppendLine("    ,@CONST_IN_NUM")
                sbSQL.AppendLine("    ,@NOW_TEMP_NUM")
                sbSQL.AppendLine("    ,@CONST_OUT_NUM")
                sbSQL.AppendLine("    ,@CUR_LENT_NUM")
                sbSQL.AppendLine("    ,@CUR_TEMP_NUM")
                sbSQL.AppendLine("    ,@SITE_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@SITE_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,@CONST_CONFIRM_TS")
                sbSQL.AppendLine("    ,@CONST_CONFIRM_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@CONST_CONFIRM_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,@CONST_CONFIRM_REMARK")
                sbSQL.AppendLine("    ,@TASK_TS")
                sbSQL.AppendLine("    ,@INS_DATETIME")
                sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,@UPD_DATETIME")
                sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,@UPD_TASK")
                sbSQL.AppendLine("    ,@UPD_FLG")
                sbSQL.AppendLine("    ,@CSV_ROW_NUM")
                sbSQL.AppendLine(")")

                'パラメーターにデータを入れる

                params = New List(Of SqlParameter)

                params.Add(New SqlParameter($"@ACTIVITY", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.Activity).ToString.Trim))

                If constNoAutoRegFlg Then
                    '資産番号自動生成あり
                    params.Add(New SqlParameter($"@CONST_NO", ConstNoAutoGenelate(constNoSeq)))
                Else
                    '工事番号自動生成なし
                    params.Add(New SqlParameter($"@CONST_NO", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstNo).ToString.Trim))
                End If

                Dim constStartDt As DateTime
                If DateTime.TryParse(_dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstStartDt).ToString.Trim, constStartDt) Then
                    params.Add(New SqlParameter($"@CONST_START_DT", CInt(constStartDt.ToString("yyyyMMdd"))))
                Else
                    params.Add(New SqlParameter($"@CONST_START_DT", DBNull.Value))
                End If
                Dim constEndDt As DateTime
                If DateTime.TryParse(_dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstEndDt).ToString.Trim, constEndDt) Then
                    params.Add(New SqlParameter($"@CONST_END_DT", CInt(constEndDt.ToString("yyyyMMdd"))))
                Else
                    params.Add(New SqlParameter($"@CONST_END_DT", DBNull.Value))
                End If
                Dim constLentStartDt As DateTime
                If DateTime.TryParse(_dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstLentStartDt).ToString.Trim, constLentStartDt) Then
                    params.Add(New SqlParameter($"@CONST_LENT_START_DT", CInt(constLentStartDt.ToString("yyyyMMdd"))))
                Else
                    params.Add(New SqlParameter($"@CONST_LENT_START_DT", DBNull.Value))
                End If
                Dim constLentEndDt As DateTime
                If DateTime.TryParse(_dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstLentEndDt).ToString.Trim, constLentEndDt) Then
                    params.Add(New SqlParameter($"@CONST_LENT_END_DT", CInt(constLentEndDt.ToString("yyyyMMdd"))))
                Else
                    params.Add(New SqlParameter($"@CONST_LENT_END_DT", DBNull.Value))
                End If
                params.Add(New SqlParameter($"@CONST_STATUS", _dtCsvImport.Rows(0).Item("CONST_STATUS").ToString.Trim))
                params.Add(New SqlParameter($"@CONST_CONTENTS", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstContents).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_EMPLOYEE_CD", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstEmployeeCd).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_EMPLOYEE_NAME", _dtCsvImport.Rows(0).Item("CONST_EMPLOYEE_NAME").ToString.Trim))
                params.Add(New SqlParameter($"@MNG_LOC_L_CD", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.MngLocLCd).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO1", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo1).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO2", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo2).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO3", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo3).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO4", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo4).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO5", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo5).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO6", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo6).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO7", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo7).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO8", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo8).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO9", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo9).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO10", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo10).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO11", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo11).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO12", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo12).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO13", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo13).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO14", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo14).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO15", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo15).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO16", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo16).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO17", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo17).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO18", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo18).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO19", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo19).ToString.Trim))
                params.Add(New SqlParameter($"@CONST_INFO20", _dtCsvImport.Rows(0).Item(CsvMapMei_Construction.ColIdx.ConstInfo20).ToString.Trim))
                If IsNumeric(_dtCsvImport.Rows(0).Item("PREV_TEMP_NUM")) Then
                    params.Add(New SqlParameter($"@PREV_TEMP_NUM", CInt(_dtCsvImport.Rows(0).Item("PREV_TEMP_NUM"))))
                Else
                    params.Add(New SqlParameter($"@PREV_TEMP_NUM", 0))
                End If
                If IsNumeric(_dtCsvImport.Rows(0).Item("CONST_IN_NUM")) Then
                    params.Add(New SqlParameter($"@CONST_IN_NUM", CInt(_dtCsvImport.Rows(0).Item("CONST_IN_NUM"))))
                Else
                    params.Add(New SqlParameter($"@CONST_IN_NUM", 0))
                End If
                If IsNumeric(_dtCsvImport.Rows(0).Item("NOW_TEMP_NUM")) Then
                    params.Add(New SqlParameter($"@NOW_TEMP_NUM", CInt(_dtCsvImport.Rows(0).Item("NOW_TEMP_NUM"))))
                Else
                    params.Add(New SqlParameter($"@NOW_TEMP_NUM", 0))
                End If
                If IsNumeric(_dtCsvImport.Rows(0).Item("CONST_OUT_NUM")) Then
                    params.Add(New SqlParameter($"@CONST_OUT_NUM", CInt(_dtCsvImport.Rows(0).Item("CONST_OUT_NUM"))))
                Else
                    params.Add(New SqlParameter($"@CONST_OUT_NUM", 0))
                End If
                If IsNumeric(_dtCsvImport.Rows(0).Item("CUR_LENT_NUM")) Then
                    params.Add(New SqlParameter($"@CUR_LENT_NUM", CInt(_dtCsvImport.Rows(0).Item("CUR_LENT_NUM"))))
                Else
                    params.Add(New SqlParameter($"@CUR_LENT_NUM", 0))
                End If
                If IsNumeric(_dtCsvImport.Rows(0).Item("CUR_TEMP_NUM")) Then
                    params.Add(New SqlParameter($"@CUR_TEMP_NUM", CInt(_dtCsvImport.Rows(0).Item("CUR_TEMP_NUM"))))
                Else
                    params.Add(New SqlParameter($"@CUR_TEMP_NUM", 0))
                End If
                params.Add(New SqlParameter($"@SITE_EMPLOYEE_CD", _dtCsvImport.Rows(0).Item("SITE_EMPLOYEE_CD").ToString.Trim))
                params.Add(New SqlParameter($"@SITE_EMPLOYEE_NAME", _dtCsvImport.Rows(0).Item("SITE_EMPLOYEE_NAME").ToString.Trim))
                Dim constConfirmTs As DateTime
                If DateTime.TryParseExact(_dtCsvImport.Rows(0).Item("CONST_CONFIRM_TS").ToString.Trim, "yyyyMMddHHmmss", Nothing, Globalization.DateTimeStyles.None, constConfirmTs) Then
                    params.Add(New SqlParameter($"@CONST_CONFIRM_TS", _dtCsvImport.Rows(0).Item("CONST_CONFIRM_TS").ToString.Trim))
                Else
                    params.Add(New SqlParameter($"@CONST_CONFIRM_TS", DBNull.Value))
                End If
                params.Add(New SqlParameter($"@CONST_CONFIRM_EMPLOYEE_CD", _dtCsvImport.Rows(0).Item("CONST_CONFIRM_EMPLOYEE_CD").ToString.Trim))
                params.Add(New SqlParameter($"@CONST_CONFIRM_EMPLOYEE_NAME", _dtCsvImport.Rows(0).Item("CONST_CONFIRM_EMPLOYEE_NAME").ToString.Trim))
                params.Add(New SqlParameter($"@CONST_CONFIRM_REMARK", _dtCsvImport.Rows(0).Item("CONST_CONFIRM_REMARK").ToString.Trim))

                If Not String.IsNullOrEmpty(_dtCsvImport.Rows(0).Item("TASK_TS").ToString.Trim) Then
                    params.Add(New SqlParameter($"@TASK_TS", _dtCsvImport.Rows(0).Item("TASK_TS").ToString.Trim))
                Else
                    params.Add(New SqlParameter($"@TASK_TS", DBNull.Value))
                End If
                params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
                params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _dtCsvImport.Rows(0).Item("INS_EMPLOYEE_CD").ToString.Trim))
                params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _dtCsvImport.Rows(0).Item("INS_EMPLOYEE_NAME").ToString.Trim))
                params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
                params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT)))

                params.Add(New SqlParameter($"@UPD_FLG", _dtCsvImport.Rows(0).Item("UPD_FLG")))
                params.Add(New SqlParameter($"@CSV_ROW_NUM", icnt))

                db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)

            Catch ex As Exception
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            End Try

        End Sub

#End Region

#Region "CSV登録用データテーブルの作成"
        ''' <summary>
        ''' CSV登録用データテーブルの作成
        ''' </summary>
        Public Sub CreatetblImportCsv()
            'CSVマッピング明細マスタで定義されている項目の列を追加
            _dtCsvImport = New DataTable("ConstructionList")
            For icnt As Integer = 0 To _lstCsvHed.Count - 1
                _dtCsvImport.Columns.Add(_lstCsvHed(icnt).ToString, GetType(String))
            Next

            '工事マスタ登録に足りない列を追加する
            Dim dcConstEmployeeNm = New DataColumn
            Dim dcConstStatus = New DataColumn
            Dim dcPrevTempNum = New DataColumn
            Dim dcConstInNum = New DataColumn
            Dim dcNowTempNum = New DataColumn
            Dim dcConstOutNum = New DataColumn
            Dim dcCurLentNum = New DataColumn
            Dim dcCurTempNum = New DataColumn
            Dim dcSiteEmployeeCd = New DataColumn
            Dim dcSiteEmployeeName = New DataColumn
            Dim dcConstConfirmTs = New DataColumn
            Dim dcConstConfirmEmployeeCd = New DataColumn
            Dim dcConstConfirmEmployeeName = New DataColumn
            Dim dcConstConfirmRemark = New DataColumn
            Dim dcTaskTs = New DataColumn
            Dim dcInsDate = New DataColumn
            Dim dcInsEmployeeCd = New DataColumn
            Dim dcInsEmployeeNm = New DataColumn
            Dim dcUpdDate = New DataColumn
            Dim dcUpdEmployeeCd = New DataColumn
            Dim dcUpdEmployeeNm = New DataColumn
            Dim dcUpdTask = New DataColumn
            Dim dcUpdFlg = New DataColumn

            ' 列名
            dcConstEmployeeNm.ColumnName = "CONST_EMPLOYEE_NAME"
            dcConstStatus.ColumnName = "CONST_STATUS"
            dcPrevTempNum.ColumnName = "PREV_TEMP_NUM"
            dcConstInNum.ColumnName = "CONST_IN_NUM"
            dcNowTempNum.ColumnName = "NOW_TEMP_NUM"
            dcConstOutNum.ColumnName = "CONST_OUT_NUM"
            dcCurLentNum.ColumnName = "CUR_LENT_NUM"
            dcCurTempNum.ColumnName = "CUR_TEMP_NUM"
            dcSiteEmployeeCd.ColumnName = "SITE_EMPLOYEE_CD"
            dcSiteEmployeeName.ColumnName = "SITE_EMPLOYEE_NAME"
            dcConstConfirmTs.ColumnName = "CONST_CONFIRM_TS"
            dcConstConfirmEmployeeCd.ColumnName = "CONST_CONFIRM_EMPLOYEE_CD"
            dcConstConfirmEmployeeName.ColumnName = "CONST_CONFIRM_EMPLOYEE_NAME"
            dcConstConfirmRemark.ColumnName = "CONST_CONFIRM_REMARK"
            dcTaskTs.ColumnName = "TASK_TS"
            dcInsDate.ColumnName = "INS_DATETIME"
            dcInsEmployeeCd.ColumnName = "INS_EMPLOYEE_CD"
            dcInsEmployeeNm.ColumnName = "INS_EMPLOYEE_NAME"
            dcUpdDate.ColumnName = "UPD_DATETIME"
            dcUpdEmployeeCd.ColumnName = "UPD_EMPLOYEE_CD"
            dcUpdEmployeeNm.ColumnName = "UPD_EMPLOYEE_NAME"
            dcUpdTask.ColumnName = "UPD_TASK"
            dcUpdFlg.ColumnName = "UPD_FLG"
            ' 初期値
            dcConstEmployeeNm.DefaultValue = DBNull.Value
            dcConstStatus.DefaultValue = CONST_STATUS.Incomplete
            dcPrevTempNum.DefaultValue = 0
            dcConstInNum.DefaultValue = 0
            dcNowTempNum.DefaultValue = 0
            dcConstOutNum.DefaultValue = 0
            dcCurLentNum.DefaultValue = 0
            dcCurTempNum.DefaultValue = 0
            dcSiteEmployeeCd.DefaultValue = DBNull.Value
            dcSiteEmployeeName.DefaultValue = DBNull.Value
            dcConstConfirmTs.DefaultValue = DBNull.Value
            dcConstConfirmEmployeeCd.DefaultValue = DBNull.Value
            dcConstConfirmEmployeeName.DefaultValue = DBNull.Value
            dcConstConfirmRemark.DefaultValue = DBNull.Value
            dcTaskTs.DefaultValue = DBNull.Value
            dcInsDate.DefaultValue = CommonRfid.getNowDate
            dcInsEmployeeCd.DefaultValue = _LoginInfo.EmployeeCd
            dcInsEmployeeNm.DefaultValue = _LoginInfo.EmployeeNm
            dcUpdDate.DefaultValue = CommonRfid.getNowDate
            dcUpdEmployeeCd.DefaultValue = _LoginInfo.EmployeeCd
            dcUpdEmployeeNm.DefaultValue = _LoginInfo.EmployeeNm
            dcUpdTask.DefaultValue = Me._Message.getMessage(CommonLabelConstructionList.TITLE)
            dcUpdFlg.DefaultValue = False
            ' データ型
            dcConstEmployeeNm.DataType = Type.GetType("System.String")
            dcConstStatus.DataType = Type.GetType("System.String")
            dcPrevTempNum.DataType = Type.GetType("System.Int32")
            dcConstInNum.DataType = Type.GetType("System.Int32")
            dcNowTempNum.DataType = Type.GetType("System.Int32")
            dcConstOutNum.DataType = Type.GetType("System.Int32")
            dcCurLentNum.DataType = Type.GetType("System.Int32")
            dcCurTempNum.DataType = Type.GetType("System.Int32")
            dcSiteEmployeeCd.DataType = Type.GetType("System.String")
            dcSiteEmployeeName.DataType = Type.GetType("System.String")
            dcConstConfirmTs.DataType = Type.GetType("System.Int64")
            dcConstConfirmEmployeeCd.DataType = Type.GetType("System.String")
            dcConstConfirmEmployeeName.DataType = Type.GetType("System.String")
            dcConstConfirmRemark.DataType = Type.GetType("System.String")
            dcTaskTs.DataType = Type.GetType("System.Int64")
            dcInsDate.DataType = Type.GetType("System.DateTime")
            dcInsEmployeeCd.DataType = Type.GetType("System.String")
            dcInsEmployeeNm.DataType = Type.GetType("System.String")
            dcUpdDate.DataType = Type.GetType("System.DateTime")
            dcUpdEmployeeCd.DataType = Type.GetType("System.String")
            dcUpdEmployeeNm.DataType = Type.GetType("System.String")
            dcUpdTask.DataType = Type.GetType("System.String")
            dcUpdFlg.DataType = Type.GetType("System.Boolean")

            ' 列を追加
            Me._dtCsvImport.Columns.Add(dcConstEmployeeNm)
            Me._dtCsvImport.Columns.Add(dcConstStatus)
            Me._dtCsvImport.Columns.Add(dcPrevTempNum)
            Me._dtCsvImport.Columns.Add(dcConstInNum)
            Me._dtCsvImport.Columns.Add(dcNowTempNum)
            Me._dtCsvImport.Columns.Add(dcConstOutNum)
            Me._dtCsvImport.Columns.Add(dcCurLentNum)
            Me._dtCsvImport.Columns.Add(dcCurTempNum)
            Me._dtCsvImport.Columns.Add(dcSiteEmployeeCd)
            Me._dtCsvImport.Columns.Add(dcSiteEmployeeName)
            Me._dtCsvImport.Columns.Add(dcConstConfirmTs)
            Me._dtCsvImport.Columns.Add(dcConstConfirmEmployeeCd)
            Me._dtCsvImport.Columns.Add(dcConstConfirmEmployeeName)
            Me._dtCsvImport.Columns.Add(dcConstConfirmRemark)
            Me._dtCsvImport.Columns.Add(dcTaskTs)
            Me._dtCsvImport.Columns.Add(dcInsDate)
            Me._dtCsvImport.Columns.Add(dcInsEmployeeCd)
            Me._dtCsvImport.Columns.Add(dcInsEmployeeNm)
            Me._dtCsvImport.Columns.Add(dcUpdDate)
            Me._dtCsvImport.Columns.Add(dcUpdEmployeeCd)
            Me._dtCsvImport.Columns.Add(dcUpdEmployeeNm)
            Me._dtCsvImport.Columns.Add(dcUpdTask)
            Me._dtCsvImport.Columns.Add(dcUpdFlg)  '既存が更新されたか。新規or既存(除却済)はFalseのまま。

        End Sub
#End Region

#Region "帳票出力処理"
        ''' <summary>
        ''' 帳票出力処理
        ''' </summary>
        ''' <param name="ViewModel">ビューモデル</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function ReportOutput(ViewModel As ConstructionListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            '最新のチェック状態でセッション情報を更新する(追加および削除) 　　別ページのチェックも取るため、このコードで、チェック内容を取得する
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)
            Dim sKeyStr As String = "'" & String.Join("','", lstSelectKeys) & "'"

            Dim messageRfid As MessageRfid      'メッセージ兼設定リスト
            messageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)


            'CSVファイル出力先フォルダに作成する
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'Excelテンプレートパスの設定
            Dim sTempFullPath As String = Server.MapPath(CommonConst.TEMPLATE_FOLDERNM)

            'PDFファイル出力名取得
            Dim setting As SettingInfo = messageRfid.getSettingInfo(CommonSettingKey.ConstPrint.KeyDiv, CommonSettingKey.ConstPrint.Key1, CommonSettingKey.ConstPrint.Key2, CommonSettingKey.Lending_Confirm_Output.Key3, True)
            Dim sDspFileName As String = String.Format(setting.DATA1 + "_{0}.pdf", Now.ToString("yyyyMMddHHmmss"))


            '工事帳票PDF作成
            Dim sTmpPdfFileNm As String = Dao.ToolConstruction.MakeConstPrint(sKeyStr, sTempFullPath, sFullPath)

            'ファイルをダウンロードさせる
            Call FileDownload(sDspFileName, "application/pdf", sTmpPdfFileNm)

            'ダウンロードさせた後、ファイルを削除。
            System.IO.File.Delete(sTmpPdfFileNm)

            '結果表示
            Return New EmptyResult()

        End Function
#End Region

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
            Dim lstConstructionList As New List(Of ConstructionListViewModel)

            '選択解除の場合、初期化する
            If Not blnSelectAll Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                Return Json("0")
            Else
                '全選択ボタン押下フラグをセットする
                Session(CommonSessionKey.ALL_SELECT_FLG) = True
            End If

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
                    lstConstructionList = _db.ConstructionListView.SqlQuery(strSQL).ToList
                Else

                    lstConstructionList = _db.ConstructionListView.SqlQuery(strSQL, lstParamCopy.ToArray).ToList
                End If
            End If

            'チェックボックスlstConstructionListの選択状態のキー(工事番号)を更新する
            If lstConstructionList.Count > 0 Then
                Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstConstructionList.Select(Function(d) $"{d.CONST_NO}").ToList
            End If

            '件数を返却する
            Return Json(lstConstructionList.Count)

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
            '一覧にない資産は未選択キーとする
            If Not IsNothing(_ViewModel.CONST_NO) Then
                Dim results = From data In _viewResult Where data.CONST_NO = _ViewModel.CONST_NO
                If results.Count <= 0 Then
                    NoneSelectKeys = String.Join("/", New String() {NoneSelectKeys, $"{_ViewModel.CONST_NO}"})
                End If
            End If

            'セッションの選択キー情報を取得する
            Dim lstSelectKeys As New List(Of String)
            If Not IsNothing(Session(CommonSessionKey.LIST_CHECKED_KEYS)) Then
                lstSelectKeys = Session(CommonSessionKey.LIST_CHECKED_KEYS)
            End If

            '選択キーを追加する
            If Not IsNothing(SelectKeys) AndAlso SelectKeys <> String.Empty Then
                lstSelectKeys.AddRange(SelectKeys.Split("/").ToList)
            End If

            '重複を排除する
            lstSelectKeys = lstSelectKeys.Distinct.ToList

            '未選択キーを削除する
            If Not IsNothing(NoneSelectKeys) AndAlso NoneSelectKeys <> String.Empty Then
                For Each deleteKey In NoneSelectKeys.Split("/").ToList
                    lstSelectKeys.Remove(deleteKey)
                Next
            End If

            'セッションを更新する
            Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstSelectKeys

            'チェックされた行のキーリストを返却する
            Return lstSelectKeys
        End Function
#End Region

#Region "引数に該当する仮置きデータを取得します(工事番号)"
        ''' <summary>
        ''' 引数に該当する仮置きデータを取得します(工事番号)
        ''' </summary>
        ''' <param name="sConstNo">工事番号</param>
        ''' <returns>仮置きデータ</returns>
        Public Function GetTemporaryDataByConstNo(ByVal sConstNo As String) As JsonResult
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As New List(Of SqlParameter)

            '新規作成時は空のオブジェクトを返却
            If sConstNo = Nothing Then
                Return Json(New List(Of ConstructionListTempListViewModel))
            End If

            'T_TEMPORARYテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  TEMP.TEMP_ID")
            sbSQL.AppendLine("  ,TEMP.TEMP_DT")
            sbSQL.AppendLine("  ,TEMP.WORK_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,CONST.CONST_CONTENTS")
            sbSQL.AppendLine("  ,TEMP.TEMP_INFO1")
            sbSQL.AppendLine("  ,TEMP.TEMP_INFO2")
            sbSQL.AppendLine("  ,TEMP.TEMP_INFO3")
            sbSQL.AppendLine("  ,TEMP.TEMP_INFO4")
            sbSQL.AppendLine("  ,TEMP.TEMP_INFO5")
            sbSQL.AppendLine("  ,TEMP.TEMP_INFO6")
            sbSQL.AppendLine("  ,TEMP.TEMP_INFO7")
            sbSQL.AppendLine("  ,(SELECT COUNT(*) FROM T_TEMP_ASSET WHERE TEMP_ID = TEMP.TEMP_ID ) AS TEMP_NUM ")
            sbSQL.AppendLine("FROM  T_TEMPORARY TEMP")
            sbSQL.AppendLine("LEFT JOIN M_CONSTRUCTION CONST")
            sbSQL.AppendLine("ON TEMP.REG_CONST_NO = CONST.CONST_NO")
            sbSQL.AppendLine("WHERE TEMP.REL_CONST_NO = @REL_CONST_NO")
            sbSQL.AppendLine("ORDER BY TEMP.TEMP_ID")

            params.Add(New SqlParameter($"@REL_CONST_NO", sConstNo.Trim))

            Dim lstTemp As List(Of ConstructionListTempListViewModel)   '仮置きリスト
            Dim db As SystemDBContext = New SystemDBContext
            lstTemp = db.ConstructionListTempListView.SqlQuery(sbSQL.ToString, params.ToArray).ToList
            If lstTemp Is Nothing Then
                lstTemp = New List(Of ConstructionListTempListViewModel)
            End If

            Return Json(lstTemp)
        End Function
#End Region

#Region "引数に該当する予約データを取得します(工事番号)"
        ''' <summary>
        ''' 引数に該当する予約データを取得します(工事番号)
        ''' </summary>
        ''' <param name="sConstNo">工事番号</param>
        ''' <returns>予約データ</returns>
        Public Function GetReserveDataByConstNo(ByVal sConstNo As String) As JsonResult
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As New List(Of SqlParameter)

            '新規作成時は空のオブジェクトを返却
            If sConstNo = Nothing Then
                Return Json(New List(Of ConstructionListReserveListViewModel))
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '照合機能の予約実施設定を取得
            Dim MatchingReserve As SettingInfo = _Message.getSettingInfo(CommonSettingKey.MatchingReserveFunction.KeyDiv,
                                                                  CommonSettingKey.MatchingReserveFunction.Key1,
                                                                  CommonSettingKey.MatchingReserveFunction.Key2,
                                                                  CommonSettingKey.MatchingReserveFunction.Key3, True)

            Dim sWorkDayFlg As String = MatchingReserve.DATA2

            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If

            '工事情報を取得する
            Dim db As SystemDBContext = New SystemDBContext
            sbSQL.AppendLine("SELECT CONST_NO, CONST_LENT_START_DT, CONST_LENT_END_DT FROM M_CONSTRUCTION")
            sbSQL.AppendLine($"WHERE CONST_NO = '{sConstNo}'")
            Dim constInfo As List(Of ConstructionListTermModel)
            constInfo = db.ConstructionTermList.SqlQuery(sbSQL.ToString).ToList

            '予約情報を取得する
            sbSQL.Clear()
            Try
                sbSQL.AppendLine("SELECT RESERVE_ID FROM T_RESERVE")
                sbSQL.AppendLine($"WHERE REMARK = '{sConstNo}'")
                Dim resInfo As List(Of ReserveListModel)
                Dim reserveId As String = ""
                resInfo = db.ReserveList.SqlQuery(sbSQL.ToString).ToList
                If resInfo IsNot Nothing OrElse resInfo.Count > 0 Then
                    reserveId = resInfo(0).RESERVE_ID.ToString
                End If
            Catch ex As Exception
            End Try

            'T_CONST_ASSETテーブルから取得するSQLを作成
            sbSQL.Clear()
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine($"    MAX('{sConstNo}') AS CONST_NO ")
            sbSQL.AppendLine("    ,MAX(MA.ASSET_NO) AS ASSET_NO ")
            sbSQL.AppendLine("    ,MA.PRODUCT_NO ")
            sbSQL.AppendLine("    ,MAX(MA.ASSET_NAME) AS ASSET_NAME ")
            sbSQL.AppendLine("    ,ISNULL(MAX(RSV.RESERVATION_NUM), 0) AS RESERVATION_NUM ")
            sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM M_ASSET WHERE PRODUCT_NO = MA.PRODUCT_NO AND LEASE_STATUS = '1') AS ASSET_NUM ")
            sbSQL.AppendLine($"    ,dbo.ASSET_MODEL_RESERVE_POSSIBLE_SF(MA.PRODUCT_NO, MAX({constInfo(0).CONST_LENT_START_DT}), MAX({constInfo(0).CONST_LENT_END_DT}), @WORKDAYFLG, MAX(RSV.RESERVE_ID)) AS WARN_MAINTE_NUM ")
            sbSQL.AppendLine($"    ,0 AS AVAILABLE_RESERVATION_NUM")
            sbSQL.AppendLine("FROM M_ASSET MA ")
            sbSQL.AppendLine("INNER JOIN ( ")
            sbSQL.AppendLine("    SELECT ")                                      '品番ごとの予約数取得
            sbSQL.AppendLine("         MAX(RSV.RESERVE_ID) AS RESERVE_ID ")
            sbSQL.AppendLine("        ,MA.PRODUCT_NO ")
            sbSQL.AppendLine("        ,COUNT(*) AS RESERVATION_NUM ")
            sbSQL.AppendLine("    FROM T_RESERVE RSV ")
            sbSQL.AppendLine("    INNER JOIN T_RESERVE_DETAIL RESVM ")
            sbSQL.AppendLine("    ON RSV.RESERVE_ID = RESVM.RESERVE_ID ")
            sbSQL.AppendLine("    INNER JOIN M_ASSET MA ")
            sbSQL.AppendLine("    ON RESVM.ASSET_NO = MA.ASSET_NO ")
            sbSQL.AppendLine("    WHERE RSV.REMARK = @CONST_NO ")
            sbSQL.AppendLine("    GROUP BY MA.PRODUCT_NO) RSV ")
            sbSQL.AppendLine("ON MA.PRODUCT_NO = RSV.PRODUCT_NO ")
            'sbSQL.AppendLine("WHERE CA.CONST_NO = @CONST_NO ")
            'sbSQL.AppendLine("AND CA.CONST_ASSET_STATUS = @CONST_ASSET_STATUS ")
            sbSQL.AppendLine("WHERE MA.ASSET_STATUS IN ('1', '2') AND MA.LEASE_STATUS = '1' ")
            'sbSQL.AppendLine("AND CA.CONST_ASSET_STATUS = @CONST_ASSET_STATUS ")
            sbSQL.AppendLine("GROUP BY MA.PRODUCT_NO ")
            sbSQL.AppendLine("ORDER BY MA.PRODUCT_NO ")

            ''T_CONST_ASSETテーブルから取得するSQLを作成
            'sbSQL.AppendLine("SELECT ")
            'sbSQL.AppendLine("FROM T_CONST_ASSET CA ")
            'sbSQL.AppendLine("INNER JOIN M_CONSTRUCTION MC ")
            'sbSQL.AppendLine("ON CA.CONST_NO = MC.CONST_NO ")
            'sbSQL.AppendLine("INNER JOIN M_ASSET MA ")
            'sbSQL.AppendLine("ON CA.ASSET_NO = MA.ASSET_NO ")
            'sbSQL.AppendLine("LEFT JOIN ( ")
            'sbSQL.AppendLine("    SELECT ")                                      '品番ごとの予約数取得
            'sbSQL.AppendLine("        MAX(RSV.RESERVE_ID) AS RESERVE_ID ")
            'sbSQL.AppendLine("        ,MA.PRODUCT_NO ")
            'sbSQL.AppendLine("        ,COUNT(*) AS RESERVATION_NUM ")
            'sbSQL.AppendLine("    FROM T_RESERVE RSV ")
            'sbSQL.AppendLine("    INNER JOIN T_RESERVE_DETAIL RESVM ")
            'sbSQL.AppendLine("    ON RSV.RESERVE_ID = RESVM.RESERVE_ID ")
            'sbSQL.AppendLine("    INNER JOIN M_ASSET MA ")
            'sbSQL.AppendLine("    ON RESVM.ASSET_NO = MA.ASSET_NO ")
            'sbSQL.AppendLine("    WHERE RSV.REMARK = @CONST_NO ")
            'sbSQL.AppendLine("    GROUP BY MA.PRODUCT_NO) RSV ")
            'sbSQL.AppendLine("ON MA.PRODUCT_NO = RSV.PRODUCT_NO ")
            'sbSQL.AppendLine("WHERE CA.CONST_NO = @CONST_NO ")
            'sbSQL.AppendLine("AND CA.CONST_ASSET_STATUS = @CONST_ASSET_STATUS ")
            'sbSQL.AppendLine("GROUP BY MA.PRODUCT_NO ")
            'sbSQL.AppendLine("ORDER BY MA.PRODUCT_NO ")

            params.Add(New SqlParameter($"@WORKDAYFLG", sWorkFlg))
            params.Add(New SqlParameter($"@CONST_NO", sConstNo.Trim))
            'params.Add(New SqlParameter($"@CONST_ASSET_STATUS", CONST_ASSET_STATUS.Reserve))

            Dim lstTemp As List(Of ConstructionListReserveListViewModel)   '予約リスト
            lstTemp = db.ConstructionListReserveListView.SqlQuery(sbSQL.ToString, params.ToArray).ToList
            If lstTemp Is Nothing Then
                lstTemp = New List(Of ConstructionListReserveListViewModel)
            Else
                For Each constData As ConstructionListReserveListViewModel In lstTemp
                    If Not String.IsNullOrWhiteSpace(constData.WARN_MAINTE_NUM) Then
                        Dim parts() As String = constData.WARN_MAINTE_NUM.Split(","c)

                        If parts.Length = 2 Then
                            ' 1個目の値を元のプロパティに上書き
                            constData.WARN_MAINTE_NUM = parts(1)

                            ' 2個目の値を別のプロパティに格納（INT変換）
                            Dim val As Integer
                            If Integer.TryParse(parts(0), val) Then
                                constData.AVAILABLE_RESERVATION_NUM = val
                            Else
                                ' 変換できないときは 0 にする（必要に応じてエラーログなども可）
                                constData.AVAILABLE_RESERVATION_NUM = 0
                            End If
                        End If
                    End If
                Next
            End If

            Return Json(lstTemp)
        End Function
#End Region

#Region "資産予約画面検索処理"
        ''' <summary>
        ''' 資産予約画面検索処理
        ''' </summary>
        ''' <param name="sConstNo">工事番号</param>
        ''' <param name="sProductNo">品番</param>
        ''' <param name="sAssetName">品名</param>
        ''' <returns>予約データ</returns>
        Public Function SearchReserveData(ByVal sConstNo As String, ByVal sProductNo As String, ByVal sAssetName As String) As JsonResult
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As New List(Of SqlParameter)

            '新規作成時は空のオブジェクトを返却
            If sConstNo = Nothing Then
                Return Json(New List(Of ConstructionListReserveListViewModel))
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '照合機能の予約実施設定を取得
            Dim MatchingReserve As SettingInfo = _Message.getSettingInfo(CommonSettingKey.MatchingReserveFunction.KeyDiv,
                                                                  CommonSettingKey.MatchingReserveFunction.Key1,
                                                                  CommonSettingKey.MatchingReserveFunction.Key2,
                                                                  CommonSettingKey.MatchingReserveFunction.Key3, True)

            Dim sWorkDayFlg As String = MatchingReserve.DATA2

            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If

            '工事情報を取得する
            Dim db As SystemDBContext = New SystemDBContext
            sbSQL.AppendLine("SELECT CONST_NO, CONST_LENT_START_DT, CONST_LENT_END_DT FROM M_CONSTRUCTION")
            sbSQL.AppendLine($"WHERE CONST_NO = '{sConstNo}'")
            Dim constInfo As List(Of ConstructionListTermModel)
            constInfo = db.ConstructionTermList.SqlQuery(sbSQL.ToString).ToList

            '予約情報を取得する
            sbSQL.Clear()
            Try
                sbSQL.AppendLine("SELECT RESERVE_ID FROM T_RESERVE")
                sbSQL.AppendLine($"WHERE REMARK = '{sConstNo}'")
                Dim resInfo As List(Of ReserveListModel)
                Dim reserveId As String = ""
                resInfo = db.ReserveList.SqlQuery(sbSQL.ToString).ToList
                If resInfo IsNot Nothing OrElse resInfo.Count > 0 Then
                    reserveId = resInfo(0).RESERVE_ID.ToString
                End If
            Catch ex As Exception
            End Try

            'T_CONST_ASSETテーブルから取得するSQLを作成
            sbSQL.Clear()
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine($"    MAX('{sConstNo}') AS CONST_NO ")
            sbSQL.AppendLine("    ,MAX(MA.ASSET_NO) AS ASSET_NO ")
            sbSQL.AppendLine("    ,MA.PRODUCT_NO ")
            sbSQL.AppendLine("    ,MAX(MA.ASSET_NAME) AS ASSET_NAME ")
            sbSQL.AppendLine("    ,ISNULL(MAX(RSV.RESERVATION_NUM), 0) AS RESERVATION_NUM ")
            sbSQL.AppendLine("    ,(SELECT COUNT(*) FROM M_ASSET WHERE PRODUCT_NO = MA.PRODUCT_NO AND LEASE_STATUS = @LEASE_STATUS) AS ASSET_NUM ")
            sbSQL.AppendLine($"    ,dbo.ASSET_MODEL_RESERVE_POSSIBLE_SF(MA.PRODUCT_NO, MAX({constInfo(0).CONST_LENT_START_DT}), MAX({constInfo(0).CONST_LENT_END_DT}), @WORKDAYFLG, MAX(RSV.RESERVE_ID)) AS WARN_MAINTE_NUM ")
            sbSQL.AppendLine($"    ,0 AS AVAILABLE_RESERVATION_NUM")
            sbSQL.AppendLine("FROM M_ASSET MA ")
            sbSQL.AppendLine("LEFT JOIN ( ")
            sbSQL.AppendLine("    SELECT ")                                      '品番ごとの予約数取得
            sbSQL.AppendLine("         MAX(RSV.RESERVE_ID) AS RESERVE_ID ")
            sbSQL.AppendLine("        ,MA.PRODUCT_NO ")
            sbSQL.AppendLine("        ,COUNT(*) AS RESERVATION_NUM ")
            sbSQL.AppendLine("    FROM T_RESERVE RSV ")
            sbSQL.AppendLine("    INNER JOIN T_RESERVE_DETAIL RESVM ")
            sbSQL.AppendLine("    ON RSV.RESERVE_ID = RESVM.RESERVE_ID ")
            sbSQL.AppendLine("    INNER JOIN M_ASSET MA ")
            sbSQL.AppendLine("    ON RESVM.ASSET_NO = MA.ASSET_NO ")
            sbSQL.AppendLine("    WHERE RSV.REMARK = @CONST_NO ")
            sbSQL.AppendLine("    GROUP BY MA.PRODUCT_NO) RSV ")
            sbSQL.AppendLine("ON MA.PRODUCT_NO = RSV.PRODUCT_NO ")
            'sbSQL.AppendLine("WHERE CA.CONST_NO = @CONST_NO ")
            'sbSQL.AppendLine("AND CA.CONST_ASSET_STATUS = @CONST_ASSET_STATUS ")
            sbSQL.AppendLine("WHERE MA.ASSET_STATUS IN ('1', '2') AND MA.LEASE_STATUS = '1' ")
            'sbSQL.AppendLine("AND CA.CONST_ASSET_STATUS = @CONST_ASSET_STATUS ")

            '品番 sProductNo
            If (Not IsNothing(sProductNo)) AndAlso
                    (Not String.IsNullOrEmpty(sProductNo.Trim)) Then
                sbSQL.AppendLine("  AND MA.PRODUCT_NO like " & "@sProductNo")
                params.Add(New SqlParameter("@sProductNo", $"{"%" & sProductNo.Trim & "%"}"))
            End If

            '品名 sAssetName
            If (Not IsNothing(sAssetName)) AndAlso
                    (Not String.IsNullOrEmpty(sAssetName.Trim)) Then
                sbSQL.AppendLine("  AND MA.ASSET_NAME like " & "@sAssetName")
                params.Add(New SqlParameter("@sAssetName", $"{"%" & sAssetName.Trim & "%"}"))
            End If

            sbSQL.AppendLine("GROUP BY MA.PRODUCT_NO ")
            sbSQL.AppendLine("ORDER BY MA.PRODUCT_NO ")

            params.Add(New SqlParameter($"@LEASE_STATUS", AssetLeaseStatus.Ok))
            params.Add(New SqlParameter($"@WORKDAYFLG", sWorkFlg))
            params.Add(New SqlParameter($"@CONST_NO", sConstNo.Trim))
            params.Add(New SqlParameter($"@CONST_ASSET_STATUS", CONST_ASSET_STATUS.Reserve))

            Dim lstTemp As List(Of ConstructionListReserveListViewModel)   '予約リスト
            lstTemp = db.ConstructionListReserveListView.SqlQuery(sbSQL.ToString, params.ToArray).ToList
            If lstTemp Is Nothing Then
                lstTemp = New List(Of ConstructionListReserveListViewModel)
            Else
                For Each constData As ConstructionListReserveListViewModel In lstTemp
                    If Not String.IsNullOrWhiteSpace(constData.WARN_MAINTE_NUM) Then
                        Dim parts() As String = constData.WARN_MAINTE_NUM.Split(","c)

                        If parts.Length = 2 Then
                            ' 1個目の値を元のプロパティに上書き
                            constData.WARN_MAINTE_NUM = parts(1)

                            ' 2個目の値を別のプロパティに格納（INT変換）
                            Dim val As Integer
                            If Integer.TryParse(parts(0), val) Then
                                constData.AVAILABLE_RESERVATION_NUM = val
                            Else
                                ' 変換できないときは 0 にする（必要に応じてエラーログなども可）
                                constData.AVAILABLE_RESERVATION_NUM = 0
                            End If
                        End If
                    End If
                Next
            End If

            Return Json(lstTemp)
        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String
            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstructionList, _LoginInfo)
            If AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                '自拠点の更新・照会権限有り
                If AuthInfo.UpdHubAuthFlg = AUTH_FLG.Yes Then
                    '他拠点の更新・照会権限有り
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                Else
                    '他拠点の更新・照会権限無し
                    ViewBag.AuthProcFlg = AuthProcFlg.JiAll
                End If
            Else
                '照会のみ
                ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
            End If

#Region "検索条件SQLを作成"
            Dim sbWhr As New StringBuilder

            '工事番号 SearchConstNo
            If (Not IsNothing(_ViewModel.SearchConstNo)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstNo.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_NO like " & "@SearchConstNo")
                addSqlParam("SearchConstNo", $"{"%" & _ViewModel.SearchConstNo.Trim & "%"}")
            End If

            '工事開始日 SearchConstStartDt
            If (Not IsNothing(_ViewModel.SearchConstStartDt)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstStartDt.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_START_DT >= " & "@SearchConstStartDt")
                addSqlParam("SearchConstStartDt", $"{_ViewModel.SearchConstStartDt.Replace("-", "")}")
            End If

            '工事終了日 SearchConstEndDt
            If (Not IsNothing(_ViewModel.SearchConstEndDt)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstEndDt.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_END_DT <= " & "@SearchConstEndDt")
                addSqlParam("SearchConstEndDt", $"{_ViewModel.SearchConstEndDt.Replace("-", "")}")
            End If

            '工事貸出開始日 SearchConstLentStartDt
            If (Not IsNothing(_ViewModel.SearchConstLentStartDt)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstLentStartDt.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_LENT_START_DT >= " & "@SearchConstLentStartDt")
                addSqlParam("SearchConstLentStartDt", $"{_ViewModel.SearchConstLentStartDt.Replace("-", "")}")
            End If

            '工事貸出終了日 SearchConstLentEndDt
            If (Not IsNothing(_ViewModel.SearchConstLentEndDt)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstLentEndDt.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_LENT_END_DT <= " & "@SearchConstLentEndDt")
                addSqlParam("SearchConstLentEndDt", $"{_ViewModel.SearchConstLentEndDt.Replace("-", "")}")
            End If

            '工事状態 SearchConstStatus
            If (Not IsNothing(_ViewModel.SearchConstStatus)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstStatus.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_STATUS like " & "@SearchConstStatus")
                addSqlParam("SearchConstStatus", $"{"%" & _ViewModel.SearchConstStatus.Trim & "%"}")
            End If

            '工事内容 SearchConstContents
            If (Not IsNothing(_ViewModel.SearchConstContents)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstContents.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_CONTENTS like " & "@SearchConstContents")
                addSqlParam("SearchConstContents", $"{"%" & _ViewModel.SearchConstContents.Trim & "%"}")
            End If

            '工事責任者氏名 SearchConstEmployeeName
            If (Not IsNothing(_ViewModel.SearchConstEmployeeName)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstEmployeeName.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_EMPLOYEE_NAME like " & "@SearchConstEmployeeName")
                addSqlParam("SearchConstEmployeeName", $"{"%" & _ViewModel.SearchConstEmployeeName.Trim & "%"}")
            End If

            '拠点ロケ SearchMngLocL
            If (Not IsNothing(_ViewModel.SearchMngLocL)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchMngLocL.Trim)) Then
                sbWhr.AppendLine("  AND CONST.MNG_LOC_L_CD like " & "@SearchMngLocLCd")
                addSqlParam("SearchMngLocLCd", $"{"%" & _ViewModel.SearchMngLocL.Trim & "%"}")
            End If

            '工事情報1 SearchConstInfo1
            If (Not IsNothing(_ViewModel.SearchConstInfo1)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo1.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO1 like " & "@SearchConstInfo1")
                addSqlParam("SearchConstInfo1", $"{"%" & _ViewModel.SearchConstInfo1.Trim & "%"}")
            End If

            '工事情報2 SearchConstInfo2
            If (Not IsNothing(_ViewModel.SearchConstInfo2)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo2.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO2 like " & "@SearchConstInfo2")
                addSqlParam("SearchConstInfo2", $"{"%" & _ViewModel.SearchConstInfo2.Trim & "%"}")
            End If

            '工事情報3 SearchConstInfo3
            If (Not IsNothing(_ViewModel.SearchConstInfo3)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo3.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO3 like " & "@SearchConstInfo3")
                addSqlParam("SearchConstInfo3", $"{"%" & _ViewModel.SearchConstInfo3.Trim & "%"}")
            End If

            '工事情報4 SearchConstInfo4
            If (Not IsNothing(_ViewModel.SearchConstInfo4)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo4.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO4 like " & "@SearchConstInfo4")
                addSqlParam("SearchConstInfo4", $"{"%" & _ViewModel.SearchConstInfo4.Trim & "%"}")
            End If

            '工事情報5 SearchConstInfo5
            If (Not IsNothing(_ViewModel.SearchConstInfo5)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo5.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO5 like " & "@SearchConstInfo5")
                addSqlParam("SearchConstInfo5", $"{"%" & _ViewModel.SearchConstInfo5.Trim & "%"}")
            End If

            '工事情報6 SearchConstInfo6
            If (Not IsNothing(_ViewModel.SearchConstInfo6)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo6.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO6 like " & "@SearchConstInfo6")
                addSqlParam("SearchConstInfo6", $"{"%" & _ViewModel.SearchConstInfo6.Trim & "%"}")
            End If

            '工事情報7 SearchConstInfo7
            If (Not IsNothing(_ViewModel.SearchConstInfo7)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo7.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO7 like " & "@SearchConstInfo7")
                addSqlParam("SearchConstInfo7", $"{"%" & _ViewModel.SearchConstInfo7.Trim & "%"}")
            End If

            '工事情報8 SearchConstInfo8
            If (Not IsNothing(_ViewModel.SearchConstInfo8)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo8.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO8 like " & "@SearchConstInfo8")
                addSqlParam("SearchConstInfo8", $"{"%" & _ViewModel.SearchConstInfo8.Trim & "%"}")
            End If

            '工事情報9 SearchConstInfo9
            If (Not IsNothing(_ViewModel.SearchConstInfo9)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo9.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO9 like " & "@SearchConstInfo9")
                addSqlParam("SearchConstInfo9", $"{"%" & _ViewModel.SearchConstInfo9.Trim & "%"}")
            End If

            '工事情報10 SearchConstInfo10
            If (Not IsNothing(_ViewModel.SearchConstInfo10)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo10.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO10 like " & "@SearchConstInfo10")
                addSqlParam("SearchConstInfo10", $"{"%" & _ViewModel.SearchConstInfo10.Trim & "%"}")
            End If

            '工事情報11 SearchConstInfo11
            If (Not IsNothing(_ViewModel.SearchConstInfo11)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo11.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO11 like " & "@SearchConstInfo11")
                addSqlParam("SearchConstInfo11", $"{"%" & _ViewModel.SearchConstInfo11.Trim & "%"}")
            End If

            '工事情報12 SearchConstInfo12
            If (Not IsNothing(_ViewModel.SearchConstInfo12)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo12.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO12 like " & "@SearchConstInfo12")
                addSqlParam("SearchConstInfo12", $"{"%" & _ViewModel.SearchConstInfo12.Trim & "%"}")
            End If

            '工事情報13 SearchConstInfo13
            If (Not IsNothing(_ViewModel.SearchConstInfo13)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo13.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO13 like " & "@SearchConstInfo13")
                addSqlParam("SearchConstInfo13", $"{"%" & _ViewModel.SearchConstInfo13.Trim & "%"}")
            End If

            '工事情報14 SearchConstInfo14
            If (Not IsNothing(_ViewModel.SearchConstInfo14)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo14.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO14 like " & "@SearchConstInfo14")
                addSqlParam("SearchConstInfo14", $"{"%" & _ViewModel.SearchConstInfo14.Trim & "%"}")
            End If

            '工事情報15 SearchConstInfo15
            If (Not IsNothing(_ViewModel.SearchConstInfo15)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo15.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO15 like " & "@SearchConstInfo15")
                addSqlParam("SearchConstInfo15", $"{"%" & _ViewModel.SearchConstInfo15.Trim & "%"}")
            End If

            '工事情報16 SearchConstInfo16
            If (Not IsNothing(_ViewModel.SearchConstInfo16)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo16.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO16 like " & "@SearchConstInfo16")
                addSqlParam("SearchConstInfo16", $"{"%" & _ViewModel.SearchConstInfo16.Trim & "%"}")
            End If

            '工事情報17 SearchConstInfo17
            If (Not IsNothing(_ViewModel.SearchConstInfo17)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo17.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO17 like " & "@SearchConstInfo17")
                addSqlParam("SearchConstInfo17", $"{"%" & _ViewModel.SearchConstInfo17.Trim & "%"}")
            End If

            '工事情報18 SearchConstInfo18
            If (Not IsNothing(_ViewModel.SearchConstInfo18)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo18.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO18 like " & "@SearchConstInfo18")
                addSqlParam("SearchConstInfo18", $"{"%" & _ViewModel.SearchConstInfo18.Trim & "%"}")
            End If

            '工事情報19 SearchConstInfo19
            If (Not IsNothing(_ViewModel.SearchConstInfo19)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo19.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO19 like " & "@SearchConstInfo19")
                addSqlParam("SearchConstInfo19", $"{"%" & _ViewModel.SearchConstInfo19.Trim & "%"}")
            End If

            '工事情報20 SearchConstInfo20
            If (Not IsNothing(_ViewModel.SearchConstInfo20)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstInfo20.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_INFO20 like " & "@SearchConstInfo20")
                addSqlParam("SearchConstInfo20", $"{"%" & _ViewModel.SearchConstInfo20.Trim & "%"}")
            End If

#End Region

            Dim sbSQL As New StringBuilder

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  CONST.CONST_NO ")
            sbSQL.AppendLine("  , CONST.CONST_START_DT ")
            sbSQL.AppendLine("  , CONST.CONST_END_DT ")
            sbSQL.AppendLine("  , CONST.CONST_LENT_START_DT ")
            sbSQL.AppendLine("  , CONST.CONST_LENT_END_DT ")
            sbSQL.AppendLine("  , CONST.CONST_STATUS ")
            sbSQL.AppendLine("  , CONST.CONST_CONTENTS ")
            sbSQL.AppendLine("  , CONST.CONST_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.CONST_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.MNG_LOC_L_CD ")
            sbSQL.AppendLine("  , CONST.CONST_INFO1 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO2 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO3 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO4 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO5 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO6 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO7 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO8 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO9 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO10 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO11 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO12 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO13 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO14 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO15 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO16 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO17 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO18 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO19 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO20 ")
            sbSQL.AppendLine("  , CONST.PREV_TEMP_NUM ")
            sbSQL.AppendLine("  , CONST.CONST_IN_NUM ")
            sbSQL.AppendLine("  , CONST.NOW_TEMP_NUM ")
            sbSQL.AppendLine("  , CONST.CONST_OUT_NUM ")
            sbSQL.AppendLine("  , CONST.CUR_LENT_NUM ")
            sbSQL.AppendLine("  , CONST.CUR_TEMP_NUM ")
            sbSQL.AppendLine("  , CONST.SITE_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.SITE_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_TS ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_REMARK ")
            sbSQL.AppendLine("  , CONST.TASK_TS ")
            sbSQL.AppendLine("  , CONST.INS_DATETIME ")
            sbSQL.AppendLine("  , CONST.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.UPD_DATETIME ")
            sbSQL.AppendLine("  , CONST.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.UPD_TASK ")
            sbSQL.AppendLine("  , MNGLOCL.LOC_NAME AS MngLocLName ")
            sbSQL.AppendLine("  , AUTH.UPD_FLG AS UPD_FLG ")
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  M_CONSTRUCTION CONST ")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
            sbSQL.AppendLine("ON CONST.MNG_LOC_L_CD = AUTH.LOC_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL ")
            sbSQL.AppendLine("ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND CONST.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")

            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{AuthInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{AuthInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{AuthInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{AuthInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthType", $"{Misc.UpdAuthType.UPD_AUTH}")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            sbSQL.AppendLine(sbWhr.ToString)

            'ORDER句の設定
            sbSQL.AppendLine(" ORDER BY CONST_NO")

            'クエリを返却
            Return sbSQL.ToString
        End Function

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New ConstructionListViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchConstNo = _ViewModel.SearchConstNo
            _viewResult.ElementAt(0).SearchConstStartDt = _ViewModel.SearchConstStartDt
            _viewResult.ElementAt(0).SearchConstEndDt = _ViewModel.SearchConstEndDt
            _viewResult.ElementAt(0).SearchConstLentStartDt = _ViewModel.SearchConstLentStartDt
            _viewResult.ElementAt(0).SearchConstLentEndDt = _ViewModel.SearchConstLentEndDt
            _viewResult.ElementAt(0).SearchConstStatus = _ViewModel.SearchConstStatus
            _viewResult.ElementAt(0).SearchConstContents = _ViewModel.SearchConstContents
            _viewResult.ElementAt(0).SearchConstEmployeeName = _ViewModel.SearchConstEmployeeName
            _viewResult.ElementAt(0).SearchMngLocL = _ViewModel.SearchMngLocL
            _viewResult.ElementAt(0).SearchConstInfo1 = _ViewModel.SearchConstInfo1
            _viewResult.ElementAt(0).SearchConstInfo2 = _ViewModel.SearchConstInfo2
            _viewResult.ElementAt(0).SearchConstInfo3 = _ViewModel.SearchConstInfo3
            _viewResult.ElementAt(0).SearchConstInfo4 = _ViewModel.SearchConstInfo4
            _viewResult.ElementAt(0).SearchConstInfo5 = _ViewModel.SearchConstInfo5
            _viewResult.ElementAt(0).SearchConstInfo6 = _ViewModel.SearchConstInfo6
            _viewResult.ElementAt(0).SearchConstInfo7 = _ViewModel.SearchConstInfo7
            _viewResult.ElementAt(0).SearchConstInfo8 = _ViewModel.SearchConstInfo8
            _viewResult.ElementAt(0).SearchConstInfo9 = _ViewModel.SearchConstInfo9
            _viewResult.ElementAt(0).SearchConstInfo10 = _ViewModel.SearchConstInfo10
            _viewResult.ElementAt(0).SearchConstInfo11 = _ViewModel.SearchConstInfo11
            _viewResult.ElementAt(0).SearchConstInfo12 = _ViewModel.SearchConstInfo12
            _viewResult.ElementAt(0).SearchConstInfo13 = _ViewModel.SearchConstInfo13
            _viewResult.ElementAt(0).SearchConstInfo14 = _ViewModel.SearchConstInfo14
            _viewResult.ElementAt(0).SearchConstInfo15 = _ViewModel.SearchConstInfo15
            _viewResult.ElementAt(0).SearchConstInfo16 = _ViewModel.SearchConstInfo16
            _viewResult.ElementAt(0).SearchConstInfo17 = _ViewModel.SearchConstInfo17
            _viewResult.ElementAt(0).SearchConstInfo18 = _ViewModel.SearchConstInfo18
            _viewResult.ElementAt(0).SearchConstInfo19 = _ViewModel.SearchConstInfo19
            _viewResult.ElementAt(0).SearchConstInfo20 = _ViewModel.SearchConstInfo20
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.CONSTRUCTION_LIST).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfo = GetGridInfo()
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As ConstructionListViewModel)
            _Model.CONST_NO = model.CONST_NO
            _Model.CONST_START_DT = model.CONST_START_DT
            _Model.CONST_END_DT = model.CONST_END_DT
            _Model.CONST_LENT_START_DT = model.CONST_LENT_START_DT
            _Model.CONST_LENT_END_DT = model.CONST_LENT_END_DT
            _Model.CONST_STATUS = model.CONST_STATUS
            _Model.CONST_CONTENTS = model.CONST_CONTENTS
            _Model.CONST_EMPLOYEE_CD = model.CONST_EMPLOYEE_CD
            _Model.CONST_EMPLOYEE_NAME = model.CONST_EMPLOYEE_NAME
            _Model.MNG_LOC_L_CD = model.MNG_LOC_L_CD
            _Model.CONST_INFO1 = model.CONST_INFO1
            _Model.CONST_INFO2 = model.CONST_INFO2
            _Model.CONST_INFO3 = model.CONST_INFO3
            _Model.CONST_INFO4 = model.CONST_INFO4
            _Model.CONST_INFO5 = model.CONST_INFO5
            _Model.CONST_INFO6 = model.CONST_INFO6
            _Model.CONST_INFO7 = model.CONST_INFO7
            _Model.CONST_INFO8 = model.CONST_INFO8
            _Model.CONST_INFO9 = model.CONST_INFO9
            _Model.CONST_INFO10 = model.CONST_INFO10
            _Model.CONST_INFO11 = model.CONST_INFO11
            _Model.CONST_INFO12 = model.CONST_INFO12
            _Model.CONST_INFO13 = model.CONST_INFO13
            _Model.CONST_INFO14 = model.CONST_INFO14
            _Model.CONST_INFO15 = model.CONST_INFO15
            _Model.CONST_INFO16 = model.CONST_INFO16
            _Model.CONST_INFO17 = model.CONST_INFO17
            _Model.CONST_INFO18 = model.CONST_INFO18
            _Model.CONST_INFO19 = model.CONST_INFO19
            _Model.CONST_INFO20 = model.CONST_INFO20
            _Model.PREV_TEMP_NUM = model.PREV_TEMP_NUM
            _Model.CONST_IN_NUM = model.CONST_IN_NUM
            _Model.NOW_TEMP_NUM = model.NOW_TEMP_NUM
            _Model.CONST_OUT_NUM = model.CONST_OUT_NUM
            _Model.CUR_LENT_NUM = model.CUR_LENT_NUM
            _Model.CUR_TEMP_NUM = model.CUR_TEMP_NUM
            _Model.SITE_EMPLOYEE_CD = model.SITE_EMPLOYEE_CD
            _Model.SITE_EMPLOYEE_NAME = model.SITE_EMPLOYEE_NAME
            _Model.CONST_CONFIRM_TS = model.CONST_CONFIRM_TS
            _Model.CONST_CONFIRM_EMPLOYEE_CD = model.CONST_CONFIRM_EMPLOYEE_CD
            _Model.CONST_CONFIRM_EMPLOYEE_NAME = model.CONST_CONFIRM_EMPLOYEE_NAME
            _Model.CONST_CONFIRM_REMARK = model.CONST_CONFIRM_REMARK
            _Model.TASK_TS = model.TASK_TS
            _Model.INS_DATETIME = model.INS_DATETIME
            _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub
#End Region

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

#Region "工事番号の自動生成要求"
        ''' <summary>
        ''' 工事番号の自動生成要求
        ''' </summary>
        ''' <returns></returns>
        Public Function RequestConstNo() As ActionResult
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '工事番号の自動生成確認
            If Not IsAutoConstNo() Then
                '工事番号の自動生成なし
                Return Content("")
            End If

            '工事番号を生成
            Dim constNo As String = ConstNoAutoGenelate()

            Return Content(constNo)
        End Function

#End Region

#Region "工事番号の自動生成有無確認"
        Private Function IsAutoConstNo() As Boolean
            '工事番号の自動生成設定を取得
            Dim ConstNoSelfReg As SettingInfo = _Message.getSettingInfo(CommonSettingKey.ConstNo_Self_Regist.KeyDiv,
                                                                  CommonSettingKey.ConstNo_Self_Regist.Key1,
                                                                  CommonSettingKey.ConstNo_Self_Regist.Key2,
                                                                  CommonSettingKey.ConstNo_Self_Regist.Key3, True)

            If Not IsNothing(ConstNoSelfReg) AndAlso ConstNoSelfReg.DATA1 <> ValidFlg.Cd.Valid Then
                Return False
            Else
                Return True
            End If
        End Function
#End Region

#Region "工事番号の自動生成"
        ''' <summary>
        ''' 工事番号の自動生成
        ''' </summary>
        ''' <returns></returns>
        Private Function ConstNoAutoGenelate(Optional constNoSeq As Integer = 0) As String
            Dim sToDayYMD As String = Now.ToString("yyyyMMdd")
            Dim iNextConstSeq As Integer

            If constNoSeq > 0 Then
                '指定の連番を使用
                iNextConstSeq = constNoSeq
            Else
                '連番を採番
                iNextConstSeq = ConstNoAutoNumberingSeq()
            End If

            '工事番号を生成
            Dim constNo As String = sToDayYMD & iNextConstSeq.ToString.PadLeft(itemlength.ConstNoAutoSeq, "0")

            Return constNo
        End Function

        ''' <summary>
        ''' 工事番号の自動採番 連番を取得
        ''' </summary>
        ''' <returns></returns>
        Private Function ConstNoAutoNumberingSeq() As Integer
            Dim sToDayYMD As String = Now.ToString("yyyyMMdd")
            Dim iNextConstSeq As Integer = 0


            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT TOP 1")
            sbSQL.AppendLine("  CONST_NO")
            sbSQL.AppendLine("FROM M_CONSTRUCTION")
            sbSQL.AppendLine("WHERE CONST_NO LIKE '" & sToDayYMD & "%'")
            sbSQL.AppendLine("ORDER BY CONST_NO DESC")

            'SQLを実行する
            Dim selectData As DbRawSqlQuery(Of MstConstructionModel) = _db.Database.SqlQuery(Of MstConstructionModel)(sbSQL.ToString)
            If selectData.Count > 0 Then
                Dim constModel As MstConstructionModel = selectData.ToList(0)

                '資産番号から連番を切り出し
                Dim sTmpConstSeq As String = constModel.CONST_NO.Substring(constModel.CONST_NO.Length - itemlength.ConstNoAutoSeq)

                '数値ではない場合は連番値を1にする
                Integer.TryParse(sTmpConstSeq, iNextConstSeq)
            End If

            iNextConstSeq += 1

            Return iNextConstSeq
        End Function
#End Region

#Region "項目配置情報を取得"
        Private Function GetGridInfo() As Dictionary(Of String, SlickGridInfo)
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

            params.Add(New SqlParameter($"@DISP_ID", DispId.CONSTRUCTION_LIST))

            Dim colSetDetail As List(Of ColSetDetailModel) = _db.ColSetDetail.SqlQuery(sbSQL.ToString, params.ToArray).ToList

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

#Region "検索条件の初期値設定"
        Private Sub SetInitCondition(ByVal searchItemList As List(Of SearchInputItemModel), ByVal dictGridInfo As Dictionary(Of String, SlickGridInfo))

            Dim searchItem = New SearchInputItemModel

            'viewModelに初期値を設定(工事情報1～20)
            '設定マスタが存在、項目配置明細マスタ.表示フラグ:表示、初期表示の場合
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "1").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO1").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo1 IsNot Nothing AndAlso _ViewModel.SearchConstInfo1 = "" Then
                '入力属性:リストでドロップダウン項目に含まれない場合は初期値を設定しない(画面は選択なしのため)
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo1 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "2").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO2").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo2 IsNot Nothing AndAlso _ViewModel.SearchConstInfo2 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo2 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "3").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO3").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo3 IsNot Nothing AndAlso _ViewModel.SearchConstInfo3 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo3 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "4").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO4").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo4 IsNot Nothing AndAlso _ViewModel.SearchConstInfo4 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo4 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "5").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO5").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo5 IsNot Nothing AndAlso _ViewModel.SearchConstInfo5 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo5 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "6").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO6").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo6 IsNot Nothing AndAlso _ViewModel.SearchConstInfo6 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo6 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "7").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO7").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo7 IsNot Nothing AndAlso _ViewModel.SearchConstInfo7 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo7 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "8").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO8").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo8 IsNot Nothing AndAlso _ViewModel.SearchConstInfo8 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo8 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "9").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO9").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo9 IsNot Nothing AndAlso _ViewModel.SearchConstInfo9 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo9 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "10").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO10").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo10 IsNot Nothing AndAlso _ViewModel.SearchConstInfo10 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo10 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "11").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO11").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo11 IsNot Nothing AndAlso _ViewModel.SearchConstInfo11 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo11 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "12").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO12").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo12 IsNot Nothing AndAlso _ViewModel.SearchConstInfo12 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo12 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "13").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO13").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo13 IsNot Nothing AndAlso _ViewModel.SearchConstInfo13 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo13 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "14").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO14").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo14 IsNot Nothing AndAlso _ViewModel.SearchConstInfo14 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo14 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "15").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO15").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo15 IsNot Nothing AndAlso _ViewModel.SearchConstInfo15 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo15 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "16").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO16").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo16 IsNot Nothing AndAlso _ViewModel.SearchConstInfo16 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo16 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "17").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO17").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo17 IsNot Nothing AndAlso _ViewModel.SearchConstInfo17 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo17 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "18").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO18").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo18 IsNot Nothing AndAlso _ViewModel.SearchConstInfo18 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo18 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "19").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO19").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo19 IsNot Nothing AndAlso _ViewModel.SearchConstInfo19 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo19 = searchItem.DATA2
                End If
            End If
            searchItem = searchItemList.Where(Function(x) x.KEY1 = "20").FirstOrDefault
            If searchItem IsNot Nothing AndAlso dictGridInfo("CONST_INFO20").DispFlag = "1" AndAlso _ViewModel.SearchConstInfo20 IsNot Nothing AndAlso _ViewModel.SearchConstInfo20 = "" Then
                If Not searchItem.DATA1 = InputType.LIST OrElse (searchItem.DATA1 = InputType.LIST AndAlso searchItem.DropdownModel.Any(Function(x) x.Cd = searchItem.DATA2)) Then
                    _ViewModel.SearchConstInfo20 = searchItem.DATA2
                End If
            End If

        End Sub
#End Region

#Region "変更用警告データ取得処理"
        ''' <summary>
        ''' 変更用警告データ取得処理
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function GetWarningData(ByVal ViewModel As ConstructionListViewModel) As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '予約明細登録
            '照合機能の予約実施設定を取得
            Dim MatchingReserve As SettingInfo = _Message.getSettingInfo(CommonSettingKey.MatchingReserveFunction.KeyDiv,
                                                                  CommonSettingKey.MatchingReserveFunction.Key1,
                                                                  CommonSettingKey.MatchingReserveFunction.Key2,
                                                                  CommonSettingKey.MatchingReserveFunction.Key3, True)

            Dim sWorkDayFlg As String = MatchingReserve.DATA2


            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If

            '予約明細データ取得(警告フラグも取得)
            Dim resDetail = Dao.ToolConstruction.GetReserveDetailDataWarning(ViewModel.CONST_NO, CDate(ViewModel.ConstLentStartDtString).ToString("yyyyMMdd"), CDate(ViewModel.ConstLentEndDtString).ToString("yyyyMMdd"), sWorkFlg)
            Dim cntWarnMainte = 0
            '取得した出来た場合
            If Not resDetail.Rows.Count = 0 Then
                For Each rowData As DataRow In resDetail.Rows
                    'WARN_MAINTE_KBNがある場合
                    If rowData("WARN_MAINTE_KBN").ToString() IsNot String.Empty Then
                        'カウントアップ
                        cntWarnMainte += 1
                    End If
                Next
            End If
            ViewBag.cntWarnMainte = cntWarnMainte
            '警告件数リターン
            Return Json(cntWarnMainte)
        End Function
#End Region

    End Class
End Namespace