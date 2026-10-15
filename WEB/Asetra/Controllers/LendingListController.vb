Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' 予約一覧画面表示用のクラス
    ''' </summary>
    Public Class LendingListController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"
#End Region

#Region "クラス変数"
        'Private _Model As LendingListModel = New LendingListModel                                   'LendingListモデル
        Private _ViewModel As LendingListViewModel = New LendingListViewModel                       'LendingList一覧表示用モデル
        Protected _viewResult As List(Of LendingListViewModel) = New List(Of LendingListViewModel)  '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As LendingListViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(CommonSessionKey.CREAGE_LENDING_LIST_REDIRECT) Is Nothing Then
                'チェックボックス状態の選択状態をクリアする
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG_SECOND)
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
        Public Function Index(ByVal ViewModel As LendingListViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_LENDING_LIST)
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '検索ボタンがクリックされた場合はセッションをクリアする
            If Me._Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                'チェックボックス状態の選択状態をクリアする
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG_SECOND)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As LendingListViewModel = Session(CommonSessionKey.CREAGE_LENDING_LIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'リダイレクト用モデルの設定
            Dim modelRedirect As LendingListViewModel = Session(CommonSessionKey.CREAGE_LENDING_LIST_REDIRECT)
            If Not IsNothing(modelRedirect) Then
                _ViewModel = modelRedirect
                Session.Remove(CommonSessionKey.CREAGE_LENDING_LIST_REDIRECT)
            End If

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
                    _ViewModel.SearchLeaseStatus_NotRented = True
                    _ViewModel.SearchLeaseStatus_Lending = True
                    _ViewModel.SearchDateExtract = LendingListDateExtract.RealLendingPeriod
                    _ViewModel.SearchRealLendingPeriodFrom = Now.ToString("yyyy-MM-dd")
                    '管理ロケーションの選択キーリスト
                    _ViewModel.SelectMngLocKeys = GetDefaultSelectMngLocKeys()
                End If

                '権限情報の取得
                Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingList, _LoginInfo)
                '特殊権限が存在するかどうかを取得
                ViewBag.IsExistAuthOption_Self = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_SELF)
                ViewBag.IsExistAuthOption_Hub = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_HUB)

                '一覧表示処理（管理ロケーションチェックボックスリストが存在しない場合は、初期表示と判断し、グリッド表示しない）
                If Not String.IsNullOrEmpty(_ViewModel.SelectMngLocKeys) Then
                    MyBase.BaseIndex(_ViewModel)
                End If

                '--2021/12/06 nv miyaji add start
                '予約確認書出力フラグ取得
                Dim setting_Kaku As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Confirm_Output.KeyDiv, CommonSettingKey.Lending_Confirm_Output.Key1, CommonSettingKey.Lending_Confirm_Output.Key2, CommonSettingKey.Lending_Confirm_Output.Key3, True)
                If setting_Kaku IsNot Nothing AndAlso setting_Kaku.DATA1 = LendingConfirmOutputFlg.Yes Then
                    ViewBag.LendingConfirmOutputFlg = True
                Else
                    ViewBag.LendingConfirmOutputFlg = False
                End If
                '予約 確認ラベル出力フラグ取得
                Dim setting_KakuLabel As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Label_Function.KeyDiv, CommonSettingKey.Lending_Label_Function.Key1, CommonSettingKey.Lending_Label_Function.Key2, CommonSettingKey.Lending_Label_Function.Key3, True)
                If setting_KakuLabel IsNot Nothing AndAlso setting_KakuLabel.DATA1 = LendingLabelFunctionFlg.Yes Then
                    ViewBag.LendingLabelFunctionFlg = True
                Else
                    ViewBag.LendingLabelFunctionFlg = False
                End If
                '送り状出力フラグ取得
                Dim setting_Okurijo As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Invoice_Function.KeyDiv, CommonSettingKey.Lending_Invoice_Function.Key1, CommonSettingKey.Lending_Invoice_Function.Key2, CommonSettingKey.Lending_Invoice_Function.Key3, True)
                If setting_Okurijo IsNot Nothing AndAlso setting_Okurijo.DATA1 = LendingInvoiceFunctionFlg.Yes Then
                    ViewBag.LendingInvoiceFunctionFlg = True
                Else
                    ViewBag.LendingInvoiceFunctionFlg = False
                End If
                '--2021/12/06 nv miyaji add end

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            Call UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            '送り状I/F情報取得
            GetInvoiceType()
            'プリンタ情報取得
            GetPrinter()

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
                Session.Remove(CommonSessionKey.CREAGE_LENDING_LIST)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingList, _LoginInfo)

#Region "検索条件を設定する"
            Dim sbWhr As New StringBuilder
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
                sbWhr.AppendLine("  AND RSV.MNG_LOC_L_CD IN (" & sbSQLMngLocKey.ToString & ")")
            End If

            '担当者
            If Not String.IsNullOrEmpty(_ViewModel.SearchEmpName) Then
                sbWhr.AppendLine("  AND CHARGE_EMP.EMPLOYEE_NAME LIKE @ChargeEmpName")
                addSqlParam("ChargeEmpName", $"%{_ViewModel.SearchEmpName}%")
            End If

            '申請者
            If Not String.IsNullOrEmpty(_ViewModel.SearchReqEmpName) Then
                sbWhr.AppendLine("  AND REQ_EMP.EMPLOYEE_NAME LIKE @ReqEmpName")
                addSqlParam("ReqEmpName", $"%{_ViewModel.SearchReqEmpName}%")
            End If

            '予約番号
            If Not String.IsNullOrEmpty(_ViewModel.SearchReserveId) Then
                sbWhr.AppendLine("  AND RSV.RESERVE_ID = @ReserveId")
                addSqlParam("ReserveId", $"{_ViewModel.SearchReserveId}")
            End If

            '貸出状態
            Dim sbSQLLeaseStatus As StringBuilder = New StringBuilder
            '--未貸出
            If _ViewModel.SearchLeaseStatus_NotRented Then
                If sbSQLLeaseStatus.Length <> 0 Then
                    sbSQLLeaseStatus.AppendLine("OR")
                End If
                sbSQLLeaseStatus.AppendLine("RSV.RESERVE_STATUS = '" & ReserveStatus.Mi & "'")
            End If
            '--返却済
            If _ViewModel.SearchLeaseStatus_Returned Then
                If sbSQLLeaseStatus.Length <> 0 Then
                    sbSQLLeaseStatus.AppendLine("OR")
                End If
                sbSQLLeaseStatus.AppendLine("RSV.RESERVE_STATUS = '" & ReserveStatus.HenkyakuSumi & "'")
            End If
            '--貸出中
            If _ViewModel.SearchLeaseStatus_Lending Then
                If sbSQLLeaseStatus.Length <> 0 Then
                    sbSQLLeaseStatus.AppendLine("OR")
                End If
                sbSQLLeaseStatus.AppendLine("(")
                sbSQLLeaseStatus.AppendLine("  RSV.RESERVE_STATUS = '" & ReserveStatus.Lending & "'")
                sbSQLLeaseStatus.AppendLine("  AND RSV.REAL_END_DT >= " & Now.ToString("yyyyMMdd"))
                sbSQLLeaseStatus.AppendLine(")")
            End If
            '--返却遅延
            If _ViewModel.SearchLeaseStatus_ReturnDelay Then
                If sbSQLLeaseStatus.Length <> 0 Then
                    sbSQLLeaseStatus.AppendLine("OR")
                End If
                sbSQLLeaseStatus.AppendLine("(")
                sbSQLLeaseStatus.AppendLine("  RSV.RESERVE_STATUS = '" & ReserveStatus.Lending & "'")
                sbSQLLeaseStatus.AppendLine("  AND RSV.REAL_END_DT < " & Now.ToString("yyyyMMdd"))
                sbSQLLeaseStatus.AppendLine(")")
            End If
            '貸出状態SQL連結
            sbWhr.AppendLine("  AND (" & sbSQLLeaseStatus.ToString & ")")

            '日付抽出
            Select Case _ViewModel.SearchDateExtract
                Case LendingListDateExtract.RealLendingPeriod
                    '--実質貸出期間
                    '実質貸出期間To
                    If Not String.IsNullOrEmpty(_ViewModel.SearchRealLendingPeriodTo) Then
                        sbWhr.AppendLine("  AND RSV.REAL_START_DT <= @RealLendingPeriodTo")
                        addSqlParam("RealLendingPeriodTo", $"{_ViewModel.SearchRealLendingPeriodTo.Replace("-", "")}")
                    End If
                    '実質貸出期間From
                    If Not String.IsNullOrEmpty(_ViewModel.SearchRealLendingPeriodFrom) Then
                        sbWhr.AppendLine("  AND RSV.REAL_END_DT >= @RealLendingPeriodFrom")
                        addSqlParam("RealLendingPeriodFrom", $"{_ViewModel.SearchRealLendingPeriodFrom.Replace("-", "")}")
                    End If

                Case LendingListDateExtract.SameDayShipping
                    '--当日出荷
                    sbWhr.AppendLine("  AND RSV.REAL_START_DT = " & Now.ToString("yyyyMMdd"))

                Case LendingListDateExtract.NextDayShipping
                    '--翌日出荷
                    '稼働日考慮フラグ
                    Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
                    Dim sWorkDayFlg As String = reservationFunction.DATA2
                    '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                    Dim sWorkFlg As String = WorkFlg.Jitsubi
                    If sWorkDayFlg = WorkDayFlg.Yes Then
                        sWorkFlg = WorkFlg.Kadobi
                    End If
                    '出荷日
                    Dim iBeforeShippingDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, 1, sWorkFlg)
                    sbWhr.AppendLine("  AND RSV.REAL_START_DT = @BeforeShippingDate")
                    addSqlParam("BeforeShippingDate", $"{iBeforeShippingDate.ToString}")

                Case LendingListDateExtract.RealLendingStartDate
                    '--実質貸出開始日
                    sbWhr.AppendLine("  AND RSV.REAL_START_DT = @RealLendingStartDate")
                    addSqlParam("RealLendingStartDate", $"{_ViewModel.SearchRealLendingStartDate.Replace("-", "")}")
            End Select
#End Region

            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("  RSV.RESERVE_ID")
            sbSQL.AppendLine("  , RSV.RESERVE_STATUS")
            sbSQL.AppendLine("  , RSV.REQ_EMP_CD")
            sbSQL.AppendLine("  , RSV.CHARGE_EMP_CD")
            sbSQL.AppendLine("  , CHARGE_EMP.EMPLOYEE_NAME AS CHARGE_EMP_NAME")
            sbSQL.AppendLine("  , RSV.MNG_LOC_L_CD")
            sbSQL.AppendLine("  , MNG_LOC.LOC_NAME AS MNG_LOC_L_NAME")
            sbSQL.AppendLine("  , RSV.LEASE_PLACE_CD")
            sbSQL.AppendLine("  , LEASE_PLACE.LEASE_PLACE_NAME")
            sbSQL.AppendLine("  , ISNULL(RSV.DEST_NAME1, '') + ISNULL(RSV.DEST_NAME2, '') AS DEST_NAME")
            sbSQL.AppendLine("  , RSV.REAL_START_DT")
            sbSQL.AppendLine("  , RSV.REAL_END_DT")
            sbSQL.AppendLine("  , RSV.DEST_NO")                 '2023/03/22 Kawamoto ADD
            sbSQL.AppendLine("  , RSV.LEASE_USE")               '2024/04/25 Tomatsuri ADD
            sbSQL.AppendLine("  , TotalCount AS TotalCount ")
            sbSQL.AppendLine("FROM")
            sbSQL.AppendLine("  T_RESERVE RSV ")
            sbSQL.AppendLine("  INNER JOIN M_LOCATION MNG_LOC ")
            sbSQL.AppendLine("  ON  MNG_LOC.LOC_DIV = @LocDivL")
            sbSQL.AppendLine("  AND MNG_LOC.LOC_L_CD = RSV.MNG_LOC_L_CD ")
            addSqlParam("LocDivL", $"{LOC_DIV.LocationL}")
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuthHub) AUTH")
            sbSQL.AppendLine("  ON RSV.MNG_LOC_L_CD = AUTH.LOC_L_CD")
            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{authInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{authInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{authInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{authInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthType_UpdAuthHub", $"{UpdAuthType.UPD_AUTH_HUB}")
            sbSQL.AppendLine("  LEFT JOIN M_EMPLOYEE CHARGE_EMP ")
            sbSQL.AppendLine("  ON  CHARGE_EMP.EMPLOYEE_CD = RSV.CHARGE_EMP_CD ")
            sbSQL.AppendLine("  LEFT JOIN M_EMPLOYEE REQ_EMP ")
            sbSQL.AppendLine("  ON  REQ_EMP.EMPLOYEE_CD = RSV.REQ_EMP_CD ")
            sbSQL.AppendLine("  LEFT JOIN M_LEASE_PLACE LEASE_PLACE ")
            sbSQL.AppendLine("  ON  LEASE_PLACE.LEASE_PLACE_CD = RSV.LEASE_PLACE_CD ")

            sbSQL.AppendLine("  LEFT JOIN (")
            sbSQL.AppendLine("    SELECT COUNT(*) TotalCount")
            sbSQL.AppendLine("    FROM T_RESERVE RSV ")
            sbSQL.AppendLine("    INNER JOIN M_LOCATION MNG_LOC ")
            sbSQL.AppendLine("    ON  MNG_LOC.LOC_DIV = @LocDivL")
            sbSQL.AppendLine("    AND MNG_LOC.LOC_L_CD = RSV.MNG_LOC_L_CD ")
            sbSQL.AppendLine("    INNER JOIN dbo.AUTH_LOCATION_SF(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuthHub) AUTH")
            sbSQL.AppendLine("    ON RSV.MNG_LOC_L_CD = AUTH.LOC_L_CD")
            sbSQL.AppendLine("    LEFT JOIN M_EMPLOYEE CHARGE_EMP ")
            sbSQL.AppendLine("    ON  CHARGE_EMP.EMPLOYEE_CD = RSV.CHARGE_EMP_CD ")
            sbSQL.AppendLine("    LEFT JOIN M_EMPLOYEE REQ_EMP ")
            sbSQL.AppendLine("    ON  REQ_EMP.EMPLOYEE_CD = RSV.REQ_EMP_CD ")
            sbSQL.AppendLine("    LEFT JOIN M_LEASE_PLACE LEASE_PLACE ")
            sbSQL.AppendLine("    ON  LEASE_PLACE.LEASE_PLACE_CD = RSV.LEASE_PLACE_CD ")

            sbSQL.AppendLine("    WHERE 1 = 1 ")
            sbSQL.AppendLine("    " & sbWhr.ToString)
            sbSQL.AppendLine("  ) B")
            sbSQL.AppendLine("    ON  1 = 1 ")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")
            sbSQL.AppendLine(sbWhr.ToString)

            sbSQL.AppendLine(" ORDER BY RSV.REAL_START_DT DESC, RESERVE_ID")

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

            Session(CommonSessionKey.LIST_VIEW_SQL) = MyBase._SQL
            Session(CommonSessionKey.LIST_VIEW_PARAM) = MyBase._paramSqlList
        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of LendingListViewModel) = _db.LendingListView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        ''' 後処理
        ''' </summary>
        ''' <param name="model">基底モデル</param>
        Protected Overrides Sub DisposeDelete(ByRef model As BaseModel)
        End Sub

        ''' <summary>
        ''' 削除処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function DeleteChcek() As Boolean
            Dim blnReturn As Boolean = True
            Return blnReturn
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
        Public Function CsvExport(ViewModel As LendingListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            Dim LendingListData As List(Of LendingListViewModel)

            'CSV出力データ取得
            LendingListData = GetLendingListData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LendingList.KeyDiv, CommonSettingKey.LendingList.Key1, CommonSettingKey.LendingList.Key2, CommonSettingKey.LendingList.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_LENDING_LIST)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_LENDING_LIST)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As LendingListViewModel In LendingListData
                col = ""

                '--予約番号
                col += SVClsCSVExp.SVFncCSVAddDquot(row.RESERVE_ID)
                '--貸出状態
                Dim sReserveStatus As String = String.Empty
                Select Case row.RESERVE_STATUS
                    Case ReserveStatus.Mi
                        sReserveStatus = Me._Message.getMessage(CommonLabel.VALUE_NOT_RENTED)
                    Case ReserveStatus.Lending
                        sReserveStatus = Me._Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING)
                    Case ReserveStatus.HenkyakuSumi
                        sReserveStatus = Me._Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sReserveStatus)
                '--管理ロケーションコード
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD)
                '--管理ロケーション名
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_NAME)
                '--担当者ID
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHARGE_EMP_CD)
                '--担当者名
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHARGE_EMP_NAME)
                '--貸出先
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_NAME)
                '--送り先名
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DEST_NAME)
                '--実質貸出開始日
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.REAL_START_DT))
                '--実質貸出終了日
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.REAL_END_DT))
                '--送り状番号
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DEST_NO)      '2023/03/22 Kawamoto ADD
                '--貸出用途
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_USE)    '2023/03/22 Kawamoto ADD

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     LendingListData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingList.TITLE))


            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()
        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetLendingListData() As List(Of LendingListViewModel)
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingList, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("  RSV.RESERVE_ID")
            sbSQL.AppendLine("  , RSV.RESERVE_STATUS")
            sbSQL.AppendLine("  , RSV.REQ_EMP_CD")
            sbSQL.AppendLine("  , RSV.CHARGE_EMP_CD")
            sbSQL.AppendLine("  , CHARGE_EMP.EMPLOYEE_NAME AS CHARGE_EMP_NAME")
            sbSQL.AppendLine("  , RSV.MNG_LOC_L_CD")
            sbSQL.AppendLine("  , MNG_LOC.LOC_NAME AS MNG_LOC_L_NAME")
            sbSQL.AppendLine("  , RSV.LEASE_PLACE_CD")
            sbSQL.AppendLine("  , LEASE_PLACE.LEASE_PLACE_NAME")
            sbSQL.AppendLine("  , ISNULL(RSV.DEST_NAME1, '') + ISNULL(RSV.DEST_NAME2, '') AS DEST_NAME")
            sbSQL.AppendLine("  , RSV.REAL_START_DT")
            sbSQL.AppendLine("  , RSV.REAL_END_DT")
            sbSQL.AppendLine("  , RSV.DEST_NO")                 '2023/03/22 Kawamoto ADD
            sbSQL.AppendLine("  , RSV.LEASE_USE")               '2024/04/25 Tomatsuri ADD
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM")
            sbSQL.AppendLine("  T_RESERVE RSV ")
            sbSQL.AppendLine("  INNER JOIN M_LOCATION MNG_LOC ")
            sbSQL.AppendLine("  ON  MNG_LOC.LOC_DIV = @LocDivL")
            sbSQL.AppendLine("  AND MNG_LOC.LOC_L_CD = RSV.MNG_LOC_L_CD ")
            params.Add(New SqlParameter($"@LocDivL", LOC_DIV.LocationL))
            sbSQL.AppendLine("  LEFT JOIN M_EMPLOYEE CHARGE_EMP ")
            sbSQL.AppendLine("  ON  CHARGE_EMP.EMPLOYEE_CD = RSV.CHARGE_EMP_CD ")
            sbSQL.AppendLine("  LEFT JOIN M_EMPLOYEE REQ_EMP ")
            sbSQL.AppendLine("  ON  REQ_EMP.EMPLOYEE_CD = RSV.REQ_EMP_CD ")
            sbSQL.AppendLine("  LEFT JOIN M_LEASE_PLACE LEASE_PLACE ")
            sbSQL.AppendLine("  ON  LEASE_PLACE.LEASE_PLACE_CD = RSV.LEASE_PLACE_CD ")
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuthHub) AUTH")
            sbSQL.AppendLine("  ON  RSV.MNG_LOC_L_CD = AUTH.LOC_L_CD")
            params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@ReadAuthFlg", authInfo.ReadAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthFlg", authInfo.UpdAuthFlg))
            params.Add(New SqlParameter($"@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
            params.Add(New SqlParameter($"@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthType_UpdAuthHub", UpdAuthType.UPD_AUTH_HUB))
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '検索条件を設定する
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
                sbSQL.AppendLine("  AND RSV.MNG_LOC_L_CD IN (" & sbSQLMngLocKey.ToString & ")")
            End If

            '担当者
            If Not String.IsNullOrEmpty(_ViewModel.SearchEmpName) Then
                sbSQL.AppendLine("  AND CHARGE_EMP.EMPLOYEE_NAME LIKE @ChargeEmpName")
                params.Add(New SqlParameter($"@ChargeEmpName", "%" & _ViewModel.SearchEmpName & "%"))
            End If

            '申請者
            If Not String.IsNullOrEmpty(_ViewModel.SearchReqEmpName) Then
                sbSQL.AppendLine("  AND REQ_EMP.EMPLOYEE_NAME LIKE @ReqEmpName")
                params.Add(New SqlParameter($"@ReqEmpName", "%" & _ViewModel.SearchReqEmpName & "%"))
            End If

            '予約番号
            If Not String.IsNullOrEmpty(_ViewModel.SearchReserveId) Then
                sbSQL.AppendLine("  AND RSV.RESERVE_ID = @ReserveId")
                params.Add(New SqlParameter($"@ReserveId", _ViewModel.SearchReserveId))
            End If

            '貸出状態
            Dim sbSQLLeaseStatus As StringBuilder = New StringBuilder
            '--未貸出
            If _ViewModel.SearchLeaseStatus_NotRented Then
                If sbSQLLeaseStatus.Length <> 0 Then
                    sbSQLLeaseStatus.AppendLine("OR")
                End If
                sbSQLLeaseStatus.AppendLine("RSV.RESERVE_STATUS = '" & ReserveStatus.Mi & "'")
            End If
            '--返却済
            If _ViewModel.SearchLeaseStatus_Returned Then
                If sbSQLLeaseStatus.Length <> 0 Then
                    sbSQLLeaseStatus.AppendLine("OR")
                End If
                sbSQLLeaseStatus.AppendLine("RSV.RESERVE_STATUS = '" & ReserveStatus.HenkyakuSumi & "'")
            End If
            '--貸出中
            If _ViewModel.SearchLeaseStatus_Lending Then
                If sbSQLLeaseStatus.Length <> 0 Then
                    sbSQLLeaseStatus.AppendLine("OR")
                End If
                sbSQLLeaseStatus.AppendLine("(")
                sbSQLLeaseStatus.AppendLine("  RSV.RESERVE_STATUS = '" & ReserveStatus.Lending & "'")
                sbSQLLeaseStatus.AppendLine("  AND RSV.REAL_END_DT >= " & Now.ToString("yyyyMMdd"))
                sbSQLLeaseStatus.AppendLine(")")
            End If
            '--返却遅延
            If _ViewModel.SearchLeaseStatus_ReturnDelay Then
                If sbSQLLeaseStatus.Length <> 0 Then
                    sbSQLLeaseStatus.AppendLine("OR")
                End If
                sbSQLLeaseStatus.AppendLine("(")
                sbSQLLeaseStatus.AppendLine("  RSV.RESERVE_STATUS = '" & ReserveStatus.Lending & "'")
                sbSQLLeaseStatus.AppendLine("  AND RSV.REAL_END_DT < " & Now.ToString("yyyyMMdd"))
                sbSQLLeaseStatus.AppendLine(")")
            End If
            '貸出状態SQL連結
            sbSQL.AppendLine("  AND (" & sbSQLLeaseStatus.ToString & ")")

            '日付抽出
            Select Case _ViewModel.SearchDateExtract
                Case LendingListDateExtract.RealLendingPeriod
                    '--実質貸出期間
                    '実質貸出期間To
                    If Not String.IsNullOrEmpty(_ViewModel.SearchRealLendingPeriodTo) Then
                        sbSQL.AppendLine("  AND RSV.REAL_START_DT <= @RealLendingPeriodTo")
                        params.Add(New SqlParameter($"@RealLendingPeriodTo", _ViewModel.SearchRealLendingPeriodTo.Replace("-", "")))
                    End If
                    '実質貸出期間From
                    If Not String.IsNullOrEmpty(_ViewModel.SearchRealLendingPeriodFrom) Then
                        sbSQL.AppendLine("  AND RSV.REAL_END_DT >= @RealLendingPeriodFrom")
                        params.Add(New SqlParameter($"@RealLendingPeriodFrom", _ViewModel.SearchRealLendingPeriodFrom.Replace("-", "")))
                    End If

                Case LendingListDateExtract.SameDayShipping
                    '--当日出荷
                    sbSQL.AppendLine("  AND RSV.REAL_START_DT = " & Now.ToString("yyyyMMdd"))    '2022/01/11  upd nv Inoue end

                Case LendingListDateExtract.NextDayShipping
                    '--翌日出荷
                    '稼働日考慮フラグ
                    Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
                    Dim sWorkDayFlg As String = reservationFunction.DATA2
                    '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                    Dim sWorkFlg As String = WorkFlg.Jitsubi
                    If sWorkDayFlg = WorkDayFlg.Yes Then
                        sWorkFlg = WorkFlg.Kadobi
                    End If
                    '出荷日
                    Dim iBeforeShippingDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, 1, sWorkFlg)
                    sbSQL.AppendLine("  AND RSV.REAL_START_DT = @BeforeShippingDate")    '2022/01/11  upd nv Inoue end
                    params.Add(New SqlParameter($"@BeforeShippingDate", iBeforeShippingDate))

                Case LendingListDateExtract.RealLendingStartDate
                    '--実質貸出開始日
                    sbSQL.AppendLine("  AND RSV.REAL_START_DT = @RealLendingStartDate")
                    params.Add(New SqlParameter($"@RealLendingStartDate", _ViewModel.SearchRealLendingStartDate.Replace("-", "")))
            End Select

            sbSQL.AppendLine(" ORDER BY RSV.REAL_START_DT DESC, RESERVE_ID")

            Dim lstLendingList As List(Of LendingListViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstLendingList = db.LendingListView.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            'データを返却
            Return lstLendingList
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

#Region "送り状作成処理"
        ''' <summary>
        ''' 送り状作成処理
        ''' </summary>
        ''' <param name="ViewModel">ビューモデル</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function OutputLendingInvoice(ViewModel As LendingListViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Dim sProcessContents As String          '操作ログ出力
            Dim sInvoiceKey As String = ViewModel.InvoiceKey

            'モデルを設定
            _ViewModel = ViewModel

            '最新のチェック状態でセッション情報を更新する(追加および削除) 　　別ページのチェックも取るため、このコードで、チェック内容を取得する
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)
            Dim sKeyStr As String = "'" & String.Join("','", lstSelectKeys) & "'"

            Dim messageRfid As MessageRfid      'メッセージ兼設定リスト
            messageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '送り状情報の取得
            Dim settingInfo As SettingInfo = messageRfid.getSettingInfo(CommonSettingKey.Lending_Invoice_Output.KeyDiv, sInvoiceKey, CommonSettingKey.LabelPrinter.Key2, CommonSettingKey.LabelPrinter.Key3, True)

            '送り状マッピングデータ取得
            Dim tblInvoiceMap As DataTable = Dao.ToolLendingInvoice.GetInvoiceMapData(settingInfo.DATA1)

            '送り状出力用予約データ取得
            Dim tblInvoiceResv As DataTable = Dao.ToolLendingInvoice.GetInvoiceReserveData(sKeyStr)

            '送り状出力用予約資産データ取得
            Dim tblInvoiceRsvDtl As DataTable = Dao.ToolLendingInvoice.GetInvoiceReserveAssetData(sKeyStr)

            If tblInvoiceMap.Rows.Count = 0 Then
                '★★★エラー処理を書く

                'Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.CsvMapMei
                ''操作ログ出力
                'sProcessContents = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
                '                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                '                                     Me._Message.getMessage(CommonLabel.MSG_ERR_CSVMAPMEI) & " " &
                '                                     Path.GetFileName(CsvFile.FileName)
                'Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelEmployee.LABEL_TITLE))

                'Return Json(Me._lstErr)
            End If

            '出力ファイル名取得
            Dim sFileNm As String = settingInfo.DATA3

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = Encoding.GetEncoding(settingInfo.DATA5)

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆CSVファイル出力先設定
            Dim sUrlFileName As String = Path.GetRandomFileName()
            Dim sDspFileName As String = String.Format(sFileNm + "_{0}.csv", Now.ToString("yyyyMMddHHmmss"))
            SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sUrlFileName)

            '◆CSV出力用リスト作成

            'データ行挿入
            Dim sLine As String = String.Empty
            Dim sWord As String = String.Empty

            'ヘッダ部出力
            For Each rowMap As DataRow In tblInvoiceMap.Rows
                If Not String.IsNullOrEmpty(sLine) Then
                    sLine &= ","
                End If
                'sWord = messageRfid.getMessage(rowMap.Item("TOOL_DIV").ToString & rowMap.Item("WORD_DIV").ToString & rowMap.Item("WORD_CD").ToString)
                sWord = rowMap.Item("INVOICE_NAME").ToString
                sLine &= SVClsCSVExp.SVFncCSVAddDquot(sWord)
            Next
            SVClsCSVExp.p_csvDataList.Add(sLine)

            'データ行挿入
            Dim sAssetNameDelimt As String = "M_ASSET.ASSET_NAME_"      '資産名取得時のデリミタ
            For Each rowResv As DataRow In tblInvoiceResv.Rows
                sLine = String.Empty

                For Each rowMap As DataRow In tblInvoiceMap.Rows
                    If Not String.IsNullOrEmpty(sLine) Then
                        sLine &= ","
                    End If

                    sWord = String.Empty
                    Select Case rowMap.Item("INVOICE_OUTPUT_DIV").ToString
                        Case Misc.InvoiceOutputDiv.Fix
                            '固定値の場合
                            sLine &= SVClsCSVExp.SVFncCSVAddDquot(rowMap.Item("INVOICE_FIX_VAL").ToString)

                        Case Misc.InvoiceOutputDiv.Replace, Misc.InvoiceOutputDiv.Cut
                            'データ置換 関連の場合、（以下、本処理のみの値なので、直値で分岐する）
                            If rowMap.Item("REPLACE_STRING").ToString = "T_RESERVE.DEST_PHONE_NUMBER" Then
                                sWord = rowResv.Item("DEST_PHONE_NUMBER").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString = "T_RESERVE.DEST_ZIP_CODE" Then
                                sWord = rowResv.Item("DEST_ZIP_CODE").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString = "T_RESERVE.DEST_ADDRESS1" Then
                                sWord = rowResv.Item("DEST_ADDRESS1").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString = "T_RESERVE.DEST_ADDRESS2" Then
                                sWord = rowResv.Item("DEST_ADDRESS2").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString = "T_RESERVE.DEST_ADDRESS3" Then
                                sWord = rowResv.Item("DEST_ADDRESS3").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString = "T_RESERVE.DEST_NAME1" Then
                                sWord = rowResv.Item("DEST_NAME1").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString = "T_RESERVE.DEST_NAME2" Then
                                sWord = rowResv.Item("DEST_NAME2").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString = "M_EMPLOYEE.EMPLOYEE_NAME" Then
                                sWord = rowResv.Item("EMPLOYEE_NAME").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString = "M_DEPARTMENT.PHONE_NUMBER" Then
                                sWord = rowResv.Item("PHONE_NUMBER").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString = "M_DEPARTMENT.ZIP_CODE" Then
                                sWord = rowResv.Item("ZIP_CODE").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString = "M_DEPARTMENT.ADDRESS" Then
                                sWord = rowResv.Item("ADDRESS").ToString.Trim
                                If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                    sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                End If

                            ElseIf rowMap.Item("REPLACE_STRING").ToString.Substring(0, (sAssetNameDelimt).Length) = sAssetNameDelimt Then
                                Dim slstWk() As String = Split(rowMap.Item("REPLACE_STRING"), sAssetNameDelimt)
                                Dim idx As Integer = CInt(slstWk(1)) - 1

                                Dim rowsDtl As DataRow() = tblInvoiceRsvDtl.Select("RESERVE_ID = '" & rowResv.Item("RESERVE_ID").ToString & "'", "ASSET_NO")

                                If idx < rowsDtl.Count Then
                                    sWord = rowsDtl(idx).Item("ASSET_NAME").ToString.Trim
                                    If rowMap.Item("INVOICE_OUTPUT_DIV").ToString = Misc.InvoiceOutputDiv.Cut Then
                                        sWord = Dao.ToolLendingInvoice.InvoiceSubString(sWord, CInt(rowMap.Item("CUT_START_INDEX")), CInt(rowMap.Item("CUT_END_INDEX")))
                                    End If

                                    If Not String.IsNullOrEmpty(rowMap.Item("INVOICE_SUFFIX_VAL").ToString) Then
                                        If idx < rowsDtl.Count Then
                                            sWord &= rowMap.Item("INVOICE_SUFFIX_VAL").ToString
                                        End If
                                    End If
                                End If

                            End If

                            sLine &= SVClsCSVExp.SVFncCSVAddDquot(sWord)

                        Case Else
                            sLine &= SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                    End Select

                Next

                SVClsCSVExp.p_csvDataList.Add(sLine)

            Next


            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then
                '★
                '_ViewModel.States = StatusType.SuccessHakko

                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_LENDING_LIST) = _ViewModel
            End If

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
            sProcessContents = messageRfid.getMessage(CommonLabelLendingList.TITLE) & " " &
                                                     messageRfid.getMessage(CommonLabel.BUTTON_OUTPUT_INVOICE) & " " &
                                                     messageRfid.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     messageRfid.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     tblInvoiceResv.Rows.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, messageRfid.getMessage(CommonLabelLendingList.TITLE))

            '結果表示
            Return New EmptyResult()
            'Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "貸出確認書作成処理"
        ''' <summary>
        ''' 貸出確認書作成処理
        ''' </summary>
        ''' <param name="ViewModel">ビューモデル</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function OutputLendingConfirm(ViewModel As LendingListViewModel) As ActionResult
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
            Dim setting As SettingInfo = messageRfid.getSettingInfo(CommonSettingKey.Lending_Confirm_Output.KeyDiv, CommonSettingKey.Lending_Confirm_Output.Key1, CommonSettingKey.Lending_Confirm_Output.Key2, CommonSettingKey.Lending_Confirm_Output.Key3, True)
            Dim sDspFileName As String = String.Format(setting.DATA3 + "_{0}.pdf", Now.ToString("yyyyMMddHHmmss"))


            '予約確認書PDF作成
            Dim sTmpPdfFileNm As String = Dao.ToolLendingConfirm.MakeLendingConfirm(sKeyStr, sTempFullPath, sFullPath)
            'Dim sTmpPdfFileNm As String = Dao.ToolLendingConfirm.MakeLendingConfirm(sTempFullPath, sFullPath)


            'ファイルをダウンロードさせる
            Call FileDownload(sDspFileName, "application/pdf", sTmpPdfFileNm)

            'ダウンロードさせた後、ファイルを削除。
            System.IO.File.Delete(sTmpPdfFileNm)

            ''操作ログ出力
            'Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetList.TITLE) & " " &
            '                                         Me._Message.getMessage(CommonLabel.BUTTON_PRINT_QR_DAICHO) & " " &
            '                                         Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
            '                                         Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
            '                                         AssetData.Count.ToString
            'Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetList.TITLE))


            '結果表示
            Return New EmptyResult()
            'Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "貸出ラベル発行処理"
        ''' <summary>
        ''' 貸出ラベル発行処理
        ''' </summary>
        ''' <param name="ViewModel">ビューモデル</param>
        ''' <param name="SelectKeys">画面で選択された項目のキー値(カンマ区切り)</param>
        ''' <param name="PrinterKey">選択したラベルプリンターのKEY1</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function OutputLendingLabel(ViewModel As LendingListViewModel, SelectKeys As String, PrinterKey As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Dim sProcessContents As String          '操作ログ出力
            Dim sPrinterKey As String = ViewModel.PrinterKey

            'モデルを設定
            _ViewModel = ViewModel

            '最新のチェック状態でセッション情報を更新する(追加および削除) 　　別ページのチェックも取るため、このコードで、チェック内容を取得する
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)
            Dim sKeyStr As String = "'" & String.Join("','", lstSelectKeys) & "'"

            Dim messageRfid As MessageRfid      'メッセージ兼設定リスト
            messageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            'ラベルプリンター情報の取得
            Dim labelPrinter As SettingInfo = messageRfid.getSettingInfo(CommonSettingKey.Lending_Label_Output.KeyDiv, PrinterKey, CommonSettingKey.Lending_Label_Output.Key2, CommonSettingKey.Lending_Label_Output.Key3, True)

            '文字コードセット
            SVClsCSVExp.p_encoding = Encoding.GetEncoding("Shift_JIS")

            '出力ファイル名取得
            Dim sFileNm As String = "LEASE_LABEL_"

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = System.IO.Path.GetFullPath(labelPrinter.DATA3)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆CSVファイル出力先設定
            Dim sUrlFileName As String = Path.GetRandomFileName().Substring(0, 8)
            Dim sPrnFileName As String = String.Format(sFileNm & sUrlFileName & "{0}.csv", Now.ToString("HHmmss"))
            SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sPrnFileName)

            '貸出ラベルデータ取得
            Dim tblLeaseLabel As DataTable = Dao.ToolLendingLabel.GetLeaseLabelData(sKeyStr)


            '◆CSV作成

            'データ行挿入
            Dim sLine As String = String.Empty

            For Each row As DataRow In tblLeaseLabel.Rows
                Dim dtKigenDate As Date = New Date(row.Item("USER_END_DT").ToString.Substring(0, 4), row.Item("USER_END_DT").ToString.Substring(4, 2), row.Item("USER_END_DT").ToString.Substring(6, 2))

                sLine = String.Empty

                sLine &= SVClsCSVExp.SVFncCSVAddDquot(row.Item("ASSET_NAME").ToString)
                sLine &= "," & SVClsCSVExp.SVFncCSVAddDquot(row.Item("FREE_ITEM10").ToString)
                sLine &= "," & SVClsCSVExp.SVFncCSVAddDquot(dtKigenDate.ToString("yyyy/MM/dd"))
                sLine &= "," & SVClsCSVExp.SVFncCSVAddDquot(row.Item("DEPARTMENT_NAME").ToString)
                sLine &= "," & SVClsCSVExp.SVFncCSVAddDquot(row.Item("EMPLOYEE_NAME").ToString)
                '2024/04/09 戸祭 項目追加 START
                sLine &= "," & SVClsCSVExp.SVFncCSVAddDquot(row.Item("RESERVE_ID").ToString)        '予約番号
                sLine &= "," & SVClsCSVExp.SVFncCSVAddDquot(row.Item("ASSET_NO").ToString)          '資産番号
                '2024/04/09 戸祭 項目追加 END

                SVClsCSVExp.p_csvDataList.Add(sLine)

            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then
                '★
                '_ViewModel.States = StatusType.SuccessHakko

                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_LENDING_LIST) = _ViewModel
            Else
                ''30001,CRT,,,%1に失敗しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
            End If

            '操作ログ出力
            sProcessContents = messageRfid.getMessage(CommonLabelLendingList.TITLE) & " " &
                                                     messageRfid.getMessage(CommonLabel.BUTTON_OUTPUT_LENDING_LABEL) & " " &
                                                     messageRfid.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     messageRfid.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     tblLeaseLabel.Rows.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, messageRfid.getMessage(CommonLabelLendingList.TITLE))

            '結果表示
            'Return New EmptyResult()
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "予約詳細画面に遷移します"
        ''' <summary>
        ''' 予約詳細画面に遷移します
        ''' </summary>
        ''' <remarks>'2021/10/25 nv miyaji create</remarks>
        <ValidateInput(False)>
        Public Function SetReserveId(ByVal ViewModel As LendingListViewModel, ByVal SelectReserveInfo As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingList, _LoginInfo)
            '特殊権限が存在するかどうかを取得
            Dim isExistAuthOption_Self As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_SELF)
            Dim isExistAuthOption_Hub As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_HUB)
            Dim isExistAuthOption_Admin As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_ADMIN)

            'ログイン社員の所属拠点データ取得
            Dim tblEmpHub As DataTable = Dao.ToolEmployee.GetHubLocData(_LoginInfo.EmployeeCd)

            '予約番号関連情報
            '0:予約番号 1:管理ロケーションコード 2:申請者コード
            Dim sReserveInfo As String() = SelectReserveInfo.Split(",")

            '貸出/返却ボタンが表示可能かどうか
            Dim isDispKashiHenBtn As Boolean = False

            'U_ADMINが存在する場合は表示可能。
            If isExistAuthOption_Admin Then
                isDispKashiHenBtn = True
            End If

            '自分の所属拠点の予約だけが編集対象な権限の場合、管理ロケーションと所属拠点が合致すれば表示可能。
            If isExistAuthOption_Hub Then
                For Each row As DataRow In tblEmpHub.Rows
                    If sReserveInfo(1) = row("HUB_LOC_L_CD").ToString Then
                        isDispKashiHenBtn = True
                        Exit For
                    End If
                Next
            End If

            '自分が登録した予約だけが編集対象の権限の場合、ログイン社員と申請者が合致すれば表示可能。
            If isExistAuthOption_Self Then
                If sReserveInfo(2) = _LoginInfo.EmployeeCd Then
                    isDispKashiHenBtn = True
                End If
            End If

            '予約申請登録/変更画面への引渡し情報クラス作成
            '照会の場合、「newInfo.DispKbn」に「DispKbn.Disp」を入れ、「ReserveId」をセットする。
            Dim newInfo As New LendingEntryInfo
            newInfo.SenimotoKbn = SenimotoKbn.LendingList
            newInfo.DispKbn = DispKbn.Disp
            newInfo.IsDispKashiHenBtn = isDispKashiHenBtn
            newInfo.ReserveId = sReserveInfo(0)
            'セッションに保存
            Session(CommonSessionKey.LENDING_ENTRY_INFO) = newInfo

            'モデルの内容をリダイレクト用のセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_LIST_REDIRECT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", "LendingEntry")
        End Function
#End Region

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


        ''' <summary>
        ''' 全選択ボタン押下時処理
        ''' 表示されている検索条件で全件検索し
        ''' チェック項目としてセッションにセットする
        ''' </summary>
        ''' <param name="blnSelectListAll"></param>
        ''' <returns></returns>
        ''' <remarks>2020/02/12 nv miyaji add</remarks>
        Public Function SelectListAll(ByVal blnSelectListAll As Boolean) As ActionResult
            Dim sbSQL As New StringBuilder
            Dim lstLendingList As New List(Of LendingListViewModel)

            '--2020/02/12 nv miyaji upd start
            '選択解除の場合、初期化する
            If Not blnSelectListAll Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする　'2020/02/12 nv miyaji　
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG_SECOND)
                Return Json("0")
            Else
                '全選択ボタン押下フラグをセットする　'2020/02/12 nv miyaji　
                Session(CommonSessionKey.ALL_SELECT_FLG_SECOND) = True
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
                    lstLendingList = _db.LendingListView.SqlQuery(strSQL).ToList
                Else

                    lstLendingList = _db.LendingListView.SqlQuery(strSQL, lstParamCopy.ToArray).ToList
                End If
            End If

            If lstLendingList.Count > 0 Then
                Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstLendingList.Select(Function(d) $"{d.RESERVE_ID}").ToList
            End If

            '件数を返却する
            Return Json(lstLendingList.Count)

        End Function
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
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingList, _LoginInfo)

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
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingList, _LoginInfo)

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

#Region "送り状I/Fの種類選択プルダウン用データ取得"
        ''' <summary>
        ''' 送り状I/Fの種類選択プルダウン用データ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function ShowInvoiceList(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As ActionResult
            '最新のチェック状態でセッション情報を更新する(追加および削除)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(SelectKeys, NoneSelectKeys)

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Dim lstSetting As List(Of PrinterDropdownModel)   '送り状I/Fの種類リスト
            lstSetting = GetInvoiceType()
            Return Json(lstSetting)
        End Function
#End Region

#Region "プリンター選択プルダウン用データ取得"
        ''' <summary>
        ''' プリンター選択プルダウン用データ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function ShowPrinterList(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As ActionResult
            '最新のチェック状態でセッション情報を更新する(追加および削除)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(SelectKeys, NoneSelectKeys)

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Dim lstSetting As List(Of PrinterDropdownModel)   'プリンタリスト
            lstSetting = GetPrinter()
            Return Json(lstSetting)
        End Function
#End Region

#Region "送り状I/F情報取得"
        ''' <summary>
        ''' 送り状I/F情報取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetInvoiceType() As List(Of PrinterDropdownModel)
            Dim tbl As DataTable = Dao.ToolSetting.GetLendingInvoiceInfo()
            Dim lst As New List(Of PrinterDropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New PrinterDropdownModel With {
                        .KEY1 = row("KEY1").ToString,
                        .DATA1 = row("DATA1").ToString
                    }
                )
            Next
            ViewBag.modelInvoice = New SelectList(lst, "KEY1", "DATA1")
            Return lst
        End Function
#End Region

#Region "プリンタ情報取得"
        ''' <summary>
        ''' プリンタ情報取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetPrinter() As List(Of PrinterDropdownModel)
            Dim tbl As DataTable = Dao.ToolSetting.GetLendingLabelInfo()
            Dim lst As New List(Of PrinterDropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New PrinterDropdownModel With {
                        .KEY1 = row("KEY1").ToString,
                        .DATA2 = row("DATA2").ToString
                    }
                )
            Next
            ViewBag.modelPrinter = New SelectList(lst, "KEY1", "DATA2")
            Return lst
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
            If Not IsNothing(_ViewModel.RESERVE_ID) Then
                Dim results = From data In _viewResult Where data.RESERVE_ID = _ViewModel.RESERVE_ID
                If results.Count <= 0 Then
                    NoneSelectKeys = String.Join("/", New String() {NoneSelectKeys, $"{_ViewModel.RESERVE_ID}"})
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
                _viewResult.Add(New LendingListViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).mngLocCheckBoxListData = _ViewModel.mngLocCheckBoxListData
            _viewResult.ElementAt(0).SelectMngLocKeys = _ViewModel.SelectMngLocKeys
            _viewResult.ElementAt(0).SearchEmpName = _ViewModel.SearchEmpName
            _viewResult.ElementAt(0).SearchReqEmpName = _ViewModel.SearchReqEmpName
            _viewResult.ElementAt(0).SearchLeaseStatus_NotRented = _ViewModel.SearchLeaseStatus_NotRented
            _viewResult.ElementAt(0).SearchLeaseStatus_Lending = _ViewModel.SearchLeaseStatus_Lending
            _viewResult.ElementAt(0).SearchLeaseStatus_Returned = _ViewModel.SearchLeaseStatus_Returned
            _viewResult.ElementAt(0).SearchLeaseStatus_ReturnDelay = _ViewModel.SearchLeaseStatus_ReturnDelay
            _viewResult.ElementAt(0).SearchDateExtract = _ViewModel.SearchDateExtract
            _viewResult.ElementAt(0).SearchRealLendingPeriodFrom = _ViewModel.SearchRealLendingPeriodFrom
            _viewResult.ElementAt(0).SearchRealLendingPeriodTo = _ViewModel.SearchRealLendingPeriodTo
            _viewResult.ElementAt(0).SearchRealLendingStartDate = _ViewModel.SearchRealLendingStartDate
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.LENDING_LIST)
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.LENDING_LIST).FIX_COL_CNT
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
        End Sub
#End Region

    End Class
End Namespace
