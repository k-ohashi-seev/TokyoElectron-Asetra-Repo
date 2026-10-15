Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' ロケーションマスタメンテナンス画面表示用のクラス
    ''' </summary>
    Public Class LocationController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)


#Region "クラス定数"
        ''' <summary>
        ''' データ取込画面　結果表示用クラス
        ''' </summary>
        Private Class CsvLocation
            Public Enum ColIdx
                ''' <summary>ロケ大区分コード</summary>
                LocLCd
                ''' <summary>ロケ中区分コード</summary>
                LocMCd
                ''' <summary>ロケ小区分コード</summary>
                LocSCd
                ''' <summary>ロケ大区分名称</summary>
                LocLName
                ''' <summary>ロケ中区分名称</summary>
                LocMName
                ''' <summary>ロケ小区分名称</summary>
                LocSName
                ''' <summary>RFIDタグ拠点</summary>   
                LocRfidFlg
                ''' <summary>管理資産棚卸区分</summary>
                OwnInvFlg
                ''' <summary>メール送信先</summary>
                MailSendAddress
                ''' <summary>メンテ発生貸出回数</summary>
                MainteLeaseNum
                ''' <summary>メンテ発生貸出日数</summary>
                MainteLeaseDay
                ''' <summary>メンテ発生貸出日数稼働日フラグ</summary>
                MainteLeaseDayWorkFlag
                ''' <summary>メンテ日数</summary>
                MainteDay
                ''' <summary>メンテ日数稼働日フラグ</summary>
                MainteDayWorkFlag
                ''' <summary>指定日数</summary>    '2023/02/08 ADD Shirakawa
                SpecifiedQty
            End Enum
        End Class
#End Region

#Region "クラス変数"
        Private _Model As LocationModel = New LocationModel                                 'Locationモデル
        Private _ViewModel As LocationViewModel = New LocationViewModel                     'Location一覧表示用モデル
        Protected _viewResult As List(Of LocationViewModel) = New List(Of LocationViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _AuthInfo As AuthInfo = Nothing                 '権限情報
        '2019/08/14 DELETE Inoue
        'Private _LocationLList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        'Private _LocationMList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        'Private _LocationSList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns>2020/02/12 nv miyaji add</returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As LocationViewModel, ByVal actionname As String) As ActionResult
            'チェックボックス状態の選択状態をクリアする　
            Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
            '全選択ボタン押下フラグをクリアする
            Session.Remove(CommonSessionKey.ALL_SELECT_FLG)

            'セッションクリア 検索時にロケ大区分の検索条件を保存
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
        Public Function Index(ByVal ViewModel As LocationViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_LOCATION)
            End If

            '--2020/02/12 nv miyaji update start
            'メッセージ一覧取得
            Dim Message As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '検索ボタンがクリックされた場合、選択状態のセッションをクリアする
            If Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする　'2020/02/12 nv miyaji　
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
            End If
            '--2020/02/12 nv miyaji update end

            '検索ボタンがクリックされた場合 検索時にロケ大区分の検索条件を保存
            If Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session(CommonSessionKey.SEARCH_FLG) = True
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As LocationViewModel = Session(CommonSessionKey.CREAGE_LOCATION)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            '設定情報(ロケ使用区分)取得     2019/08/14 ADD Inoue
            Dim settingInfo As MessageRfid      'メッセージ兼設定リスト
            Dim modelLocationUsage As New LocationUsageModel       'ロケーション使用区分
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim setting As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingInfo.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            ViewBag.LocUsage_L = setting.DATA1
            ViewBag.LocUsage_M = setting.DATA2
            ViewBag.LocUsage_S = setting.DATA3

            '表示指定初期値設定
            If String.IsNullOrEmpty(_ViewModel.SearchDispSelect) Then
                Dim defaultDispSelect As String = DISP_SELECT.LMS
                If ViewBag.LocUsage_S <> Misc.ItemUse.Yes Then
                    defaultDispSelect = DISP_SELECT.LM
                End If
                If ViewBag.LocUsage_M <> Misc.ItemUse.Yes Then
                    defaultDispSelect = DISP_SELECT.L
                End If
                _ViewModel.SearchDispSelect = defaultDispSelect
            End If

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)

            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
                ViewBag.modelLocL = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                ViewBag.modelLocM = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                ViewBag.modelLocS = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                ViewBag.modalmodelLocL = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                ViewBag.modalmodelLocM = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                ViewBag.modelPrinter = New SelectList(New List(Of PrinterDropdownModel), "KEY1", "DATA1")
                CopyModelData()
                Return View(_viewResult)
            Else
                '権限情報の取得 
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
                _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Location, _LoginInfo)

                'セッションSEARCH_FLGがNothingの場合
                If Session(CommonSessionKey.SEARCH_FLG) Is Nothing Then
                    '初回表示フラグ True 検索時にロケ大区分の検索条件を保存
                    _ViewModel.FirstSearch = True
                Else
                    '検索ボタンから表示
                    '初回表示フラグ False
                    _ViewModel.FirstSearch = False
                    '一覧表示処理
                    MyBase.BaseIndex(_ViewModel)
                End If

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo

                '検索条件の復元有無 検索時にロケ大区分の検索条件を保存
                _ViewModel.IsRestoreOwnSearchConditions = True

                '--2021/09/27 nv iwamoto add start
                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '権限情報の取得
                Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Location, _LoginInfo)
                If _AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    '自拠点の更新・照会権限有り
                    If _AuthInfo.UpdHubAuthFlg = AUTH_FLG.Yes Then
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
                '--2021/09/27 nv iwamoto add end
            End If

            '--2020/02/12 miyaji add start
            '最新のチェック状態でセッション情報を更新する(追加および削除)
            UpdateChecedInfo(_ViewModel.SelectKeys, _ViewModel.NoneSelectKeys)
            '--2020/02/12 miyaji add end

            '直近登録情報の設定
            Dim tbl As DataTable = Dao.ToolCommon.GetMstInsInfo("M_LOCATION")
            If tbl IsNot Nothing AndAlso tbl.Rows.Count <> 0 Then
                ViewBag.LastInsDateTime = CommonRfid.ToStringDateYMDHM(Date.Parse(tbl.Rows(0).Item("INS_DATETIME").ToString))
                ViewBag.LastInsEmployeeNm = tbl.Rows(0).Item("INS_EMPLOYEE_NAME").ToString
            End If

            'ログインセッション切れ以外のエラーが設定されていたらモーダルのコンボボックスを設定   '2019/11/19 nv miyaji
            If _ViewModel.ErrCode = String.Empty OrElse _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY Then
                ' 大区分ドロップダウンリスト作成
                GetLocationLData()
                ' 中区分
                GetLocationMData(_ViewModel.SearchLocL)
                ' 小区分
                GetLocationSData(_ViewModel.SearchLocL, _ViewModel.SearchLocM)
            Else
                ' 大区分ドロップダウンリスト作成
                GetLocationLData()
                ' 中区分
                GetLocationMData(_ViewModel.ModalLocL)
                ' 小区分
                GetLocationSData(_ViewModel.ModalLocL, _ViewModel.ModalLocM)
            End If

            ' プリンタ情報取得
            GetPrinter()

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.LOCATION, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL))
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
                Session.Remove(CommonSessionKey.CREAGE_LOCATION)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Location, _LoginInfo)
            If _AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                '自拠点の更新・照会権限有り
                If _AuthInfo.UpdHubAuthFlg = AUTH_FLG.Yes Then
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

            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            'sbSQL.Append("SELECT ")
            'sbSQL.Append("  LOC_DIV ")
            'sbSQL.Append("  , LOC_L_CD ")
            'sbSQL.Append("  , LOC_M_CD ")
            'sbSQL.Append("  , LOC_S_CD ")
            'sbSQL.Append("  , LOC_NAME ")
            'sbSQL.Append("  , INS_DATETIME ")
            'sbSQL.Append("  , INS_EMPLOYEE_CD ")
            'sbSQL.Append("  , INS_EMPLOYEE_NAME ")
            'sbSQL.Append("  , UPD_DATETIME ")
            'sbSQL.Append("  , UPD_EMPLOYEE_CD ")
            'sbSQL.Append("  , UPD_EMPLOYEE_NAME ")
            'sbSQL.Append("  , UPD_TASK ")
            'sbSQL.Append("   ,COUNT(*) OVER () AS TotalCount ")
            'sbSQL.Append("FROM ")
            'sbSQL.Append("  M_LOCATION ")
            'sbSQL.Append("WHERE ")
            'sbSQL.Append("  1 = 1 ")

            sbSQL.Append("SELECT ")
            sbSQL.Append("  LOC_BASE.LOC_DIV ")
            sbSQL.Append("  , LOC_BASE.LOC_L_CD ")
            sbSQL.Append("  , LOC_BASE.LOC_M_CD ")
            sbSQL.Append("  , LOC_BASE.LOC_S_CD ")
            sbSQL.Append("  , LOC_BASE.LOC_NAME ")
            sbSQL.Append("  , LOC_L.LOC_NAME AS LOC_L_NAME ")
            sbSQL.Append("  , LOC_M.LOC_NAME AS LOC_M_NAME ")
            sbSQL.Append("  , LOC_BASE.LOC_NAME AS LOC_S_NAME ")
            sbSQL.Append("  , LOC_BASE.LOC_SHARE_FLG ")                 'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.LOC_RFID_FLG ")                 ' RFIDタグ拠点
            sbSQL.Append("  , LOC_BASE.OWN_INV_FLG ")
            sbSQL.Append("  , LOC_BASE.MAIL_SEND_ADDRESS ")             'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.MAINTE_LEASE_NUM ")              'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.MAINTE_LEASE_DAY ")              'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.MAINTE_LEASE_DAY_WORK_FLAG ")    'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.MAINTE_DAY ")                    'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.MAINTE_DAY_WORK_FLAG ")          'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.SPECIFIED_QTY ")                 '2023/02/08 ADD Shirakawa

            sbSQL.Append("  , LOC_BASE.INS_DATETIME ")
            sbSQL.Append("  , LOC_BASE.INS_EMPLOYEE_CD ")
            sbSQL.Append("  , LOC_BASE.INS_EMPLOYEE_NAME ")
            sbSQL.Append("  , LOC_BASE.UPD_DATETIME ")
            sbSQL.Append("  , LOC_BASE.UPD_EMPLOYEE_CD ")
            sbSQL.Append("  , LOC_BASE.UPD_EMPLOYEE_NAME ")
            sbSQL.Append("  , LOC_BASE.UPD_TASK ")
            sbSQL.Append("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_LOCATION LOC_BASE ")
            sbSQL.Append("  LEFT JOIN M_LOCATION LOC_L ")
            sbSQL.Append("  ON  LOC_L.LOC_L_CD = LOC_BASE.LOC_L_CD ")
            sbSQL.Append("  AND LOC_L.LOC_M_CD = ' ' ")
            sbSQL.Append("  AND LOC_L.LOC_S_CD = ' ' ")
            sbSQL.Append("  LEFT JOIN M_LOCATION LOC_M ")
            sbSQL.Append("  ON  LOC_M.LOC_L_CD = LOC_BASE.LOC_L_CD ")
            sbSQL.Append("  AND LOC_M.LOC_M_CD = LOC_BASE.LOC_M_CD ")
            sbSQL.Append("  AND LOC_M.LOC_S_CD = ' ' ")

            '検索時、自分の権限のあるデータのみを表示する 
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
            sbSQL.AppendLine("ON LOC_BASE.LOC_L_CD = ULOC.LOC_L_CD ")
            sbSQL.AppendLine($"AND ((ULOC.LOC_M_FLG='{AUTH_LOC_M_FLG.Yes}' AND ULOC.LOC_M_CD = LOC_BASE.LOC_M_CD) OR (ULOC.LOC_M_FLG = '{AUTH_LOC_M_FLG.No}')) ")
            _paramSqlList.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
            _paramSqlList.Add(New SqlParameter("@ReadAuthFlg", _AuthInfo.ReadAuthFlg))
            _paramSqlList.Add(New SqlParameter("@UpdAuthFlg", _AuthInfo.UpdAuthFlg))
            _paramSqlList.Add(New SqlParameter("@ReadHubAuthFlg", _AuthInfo.ReadHubAuthFlg))
            _paramSqlList.Add(New SqlParameter("@UpdHubAuthFlg", _AuthInfo.UpdHubAuthFlg))
            _paramSqlList.Add(New SqlParameter("@UpdAuthOption", _AuthInfo.AuthOption))


            sbSQL.Append("WHERE ")
            sbSQL.Append("  1 = 1 ")

            '検索条件を設定する
            'ロケ大区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                sbSQL.Append("  AND LOC_BASE.LOC_L_CD = @SearchLocL")
                addSqlParam("SearchLocL", $"{_ViewModel.SearchLocL}")
            End If

            'ロケ中区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocM) Then
                sbSQL.Append("  AND LOC_BASE.LOC_M_CD = @SearchLocM")
                addSqlParam("SearchLocM", $"{_ViewModel.SearchLocM}")
            End If

            'ロケ小区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocS) Then
                sbSQL.Append("  AND LOC_BASE.LOC_S_CD = @SearchLocS")
                addSqlParam("SearchLocS", $"{_ViewModel.SearchLocS}")
            End If

            '表示指定
            If Not String.IsNullOrEmpty(_ViewModel.SearchDispSelect) Then
                Select Case _ViewModel.SearchDispSelect
                    Case DISP_SELECT.L
                        sbSQL.Append("  AND (LOC_BASE.LOC_DIV = @LocDivL)")
                        addSqlParam("LocDivL", $"{LOC_DIV.LocationL}")

                    Case DISP_SELECT.M
                        sbSQL.Append("  AND (LOC_BASE.LOC_DIV = @LocDivM)")
                        addSqlParam("LocDivM", $"{LOC_DIV.LocationM}")

                    Case DISP_SELECT.S
                        sbSQL.Append("  AND (LOC_BASE.LOC_DIV = @LocDivS)")
                        addSqlParam("LocDivS", $"{LOC_DIV.LocationS}")

                    Case DISP_SELECT.LM
                        sbSQL.Append("  AND (LOC_BASE.LOC_DIV = @LocDivL OR LOC_BASE.LOC_DIV = @LocDivM)")
                        addSqlParam("LocDivL", $"{LOC_DIV.LocationL}")
                        addSqlParam("LocDivM", $"{LOC_DIV.LocationM}")
                End Select
            End If

            sbSQL.Append(" ORDER BY LOC_BASE.LOC_L_CD, LOC_BASE.LOC_M_CD, LOC_BASE.LOC_S_CD, LOC_BASE.LOC_DIV")

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

            '--2020/02/12 nv miyaji add start
            '検索条件(全件)を指定したSQLとパラメーターをセッションに保持する
            Session(CommonSessionKey.LIST_VIEW_SQL) = MyBase._SQL
            Session(CommonSessionKey.LIST_VIEW_PARAM) = MyBase._paramSqlList
            '--2020/02/12 nv miyaji add end

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of LocationViewModel) = _db.LocationView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function Create(ByVal ViewModel As LocationViewModel, ByVal OnCreteButton As String) As ActionResult
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

            '入力不可のキー項目に半角スペースを設定する
            Select Case _ViewModel.LOC_DIV
                Case LOC_DIV.LocationL
                    _ViewModel.LOC_M_CD = " "
                    _ViewModel.LOC_S_CD = " "

                Case LOC_DIV.LocationM
                    _ViewModel.LOC_S_CD = " "
            End Select

            ''分類区分が大区分以外の場合は値をクリアする      '--2021/11/16 add nv iwamoto
            'If _ViewModel.LOC_DIV <> LOC_DIV.LocationL Then
            '    _ViewModel.LOC_SHARE_FLG = String.Empty
            '    _ViewModel.MAIL_SEND_ADDRESS = Nothing
            '    _ViewModel.MAINTE_LEASE_NUM = Nothing
            '    _ViewModel.MAINTE_LEASE_DAY = Nothing
            '    _ViewModel.MAINTE_LEASE_DAY_WORK_FLAG = "A"
            '    _ViewModel.MAINTE_DAY = Nothing
            '    _ViewModel.MAINTE_DAY_WORK_FLAG = "A"
            'End If

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理
                MyBase.BaseCreate(_ViewModel)

                '--2019/11/19 nv miyaji add start
                If _ViewModel.ErrCode <> String.Empty Then
                    If _ViewModel.LOC_DIV = LOC_DIV.LocationM AndAlso _ViewModel.ErrTarget = "LOC_L_CD" Then
                        ' 分類区分が中区分の時、大区分エラーはコンボボックスをエラーターゲットとする。
                        _ViewModel.ErrTarget = "ModalLocL"
                    ElseIf _ViewModel.LOC_DIV = LOC_DIV.LocationS Then
                        ' 分類区分が小区分の時、大区分・中区分エラーはコンボボックスをエラーターゲットとする。
                        If _ViewModel.ErrTarget = "LOC_L_CD" Then
                            _ViewModel.ErrTarget = "ModalLocL"
                        ElseIf _ViewModel.ErrTarget = "LOC_M_CD" Then
                            _ViewModel.ErrTarget = "ModalLocM"
                        End If
                    End If
                End If
                '--2019/11/19 nv miyaji add end
            Else
                '更新処理
                MyBase.BaseEdit(_ViewModel)
            End If

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_DIV) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_DIV).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_L_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_M_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_S_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))

                    _ViewModel.States = StatusType.SuccessCreate
                Else
                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_DIV) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_DIV).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_L_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_M_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_S_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))

                    If _ViewModel.LOC_DIV = LOC_DIV.LocationL AndAlso
                        _ViewModel.LocRfidFlg AndAlso
                        _ViewModel.LOC_RFID_FLG = LocRfidFlg.No Then
                        _ViewModel.States = StatusType.SuccessLocEdit
                        TempData("LocLCd") = _ViewModel.LOC_L_CD
                    Else
                        _ViewModel.States = StatusType.SuccessEdit
                    End If
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
            Session(CommonSessionKey.CREAGE_LOCATION) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_LOCATION) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            '必須チェック
            Select Case _Model.LOC_DIV
                Case LOC_DIV.LocationL
                    'View側でチェック

                Case LOC_DIV.LocationM
                    'ロケ中区分コード必須入力チェック
                    If _Model.LOC_M_CD Is Nothing Then
                        ModelState.AddModelError("LOC_M_CD", CommonLabel.MSG_ERR_NOT_ENTERD)
                        blnReturn = False
                    End If

                    'ロケ大区分コード存在チェック
                    If _Model.LOC_L_CD IsNot Nothing Then
                        If Dao.ToolLocation.IsExistLocLCd(_Model.LOC_L_CD) = False Then
                            ModelState.AddModelError("LOC_L_CD", CommonLabel.MSG_ERR_NOT_EXIST_LOC_L_CD)
                            blnReturn = False
                        End If
                    End If

                Case LOC_DIV.LocationS
                    'ロケ中区分コード必須入力チェック
                    If _Model.LOC_M_CD Is Nothing Then
                        ModelState.AddModelError("LOC_M_CD", CommonLabel.MSG_ERR_NOT_ENTERD)
                        blnReturn = False
                    End If
                    'ロケ小区分コード必須入力チェック
                    If _Model.LOC_S_CD Is Nothing Then
                        ModelState.AddModelError("LOC_S_CD", CommonLabel.MSG_ERR_NOT_ENTERD)
                        blnReturn = False
                    End If

                    'ロケ大区分コード存在チェック
                    If _Model.LOC_L_CD IsNot Nothing Then
                        If Dao.ToolLocation.IsExistLocLCd(_Model.LOC_L_CD) = False Then
                            ModelState.AddModelError("LOC_L_CD", CommonLabel.MSG_ERR_NOT_EXIST_LOC_L_CD)
                            blnReturn = False
                        End If
                    End If
                    'ロケ中区分コード存在チェック
                    If _Model.LOC_M_CD IsNot Nothing Then
                        If Dao.ToolLocation.IsExistLocMCd(_Model.LOC_L_CD, _Model.LOC_M_CD) = False Then
                            ModelState.AddModelError("LOC_M_CD", CommonLabel.MSG_ERR_NOT_EXIST_LOC_M_CD)
                            blnReturn = False
                        End If
                    End If
            End Select

            '登録済みかチェックする
            '重複エラーの場合、エラー情報を保持する
            Select Case _Model.LOC_DIV
                Case LOC_DIV.LocationL
                    '拠点責任者がログインしている場合(権限マスタの更新権限（他拠点）がNの場合)
                    'ロケーション(会社)を追加する権限がありません。
                    _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Location, _LoginInfo)
                    If _AuthInfo.UpdHubAuthFlg = AUTH_FLG.No Then
                        ModelState.AddModelError("LOC_L_CD", CommonLabel.MSG_ERR_ADDKENGEN)
                        blnReturn = False
                    End If

                    Dim model As LocationModel = db.Location.Find(_Model.LOC_DIV, _Model.LOC_L_CD, " ", " ")
                    If model IsNot Nothing Then
                        ModelState.AddModelError("LOC_L_CD", CommonLabel.MSG_ERR_DATAJUUFUKU)
                        blnReturn = False
                    End If

                Case LOC_DIV.LocationM
                    Dim model As LocationModel = db.Location.Find(_Model.LOC_DIV, _Model.LOC_L_CD, _Model.LOC_M_CD, " ")
                    If model IsNot Nothing Then
                        ModelState.AddModelError("LOC_M_CD", CommonLabel.MSG_ERR_DATAJUUFUKU)
                        blnReturn = False
                    End If

                Case LOC_DIV.LocationS
                    Dim model As LocationModel = db.Location.Find(_Model.LOC_DIV, _Model.LOC_L_CD, _Model.LOC_M_CD, _Model.LOC_S_CD)
                    If model IsNot Nothing Then
                        ModelState.AddModelError("LOC_S_CD", CommonLabel.MSG_ERR_DATAJUUFUKU)
                        blnReturn = False
                    End If
            End Select

            '貸出日数が未入力の場合に、稼働日考慮がチェックされていればエラー               '2021/11/08 add nv iwamoto
            If _Model.MAINTE_LEASE_DAY Is Nothing AndAlso _Model.MAINTE_LEASE_DAY_WORK_FLAG = WorkDayFlg.Yes Then
                ModelState.AddModelError("MAINTE_LEASE_DAY", CommonLabelCategory.MSG_ERR_CANNOT_CHECK_MAINTE_LEASE_DAY_WORK_FLAG)
                blnReturn = False
            End If

            'メンテナンス日数が未入力の場合に、稼働日考慮がチェックされていればエラー       '2021/11/08 add nv iwamoto
            If _Model.MAINTE_DAY Is Nothing AndAlso _Model.MAINTE_DAY_WORK_FLAG = WorkDayFlg.Yes Then
                ModelState.AddModelError("MAINTE_DAY", CommonLabelCategory.MSG_ERR_CANNOT_CHECK_MAINTE_DAY_WORK_FLAG)
                blnReturn = False
            End If

            '指定数量
            If _Model.SPECIFIED_QTY IsNot Nothing Then
                Dim srtSpecified As String = _Model.SPECIFIED_QTY
                Dim sStrcnt As String() = srtSpecified.ToString().Split("."c)
                If sStrcnt(0).Length > itemlength.SpecifiedQty Then
                    '項目長チェック
                    ModelState.AddModelError("SPECIFIED_QTY", CommonLabel.MSG_ERR_DATAOVER)
                    blnReturn = False
                End If
                '小数点部があったなら小数点部のチェックを行う
                If sStrcnt.Length > 1 Then
                    If sStrcnt(1).Length > itemlength.SpecifiedQtyDecimal Then
                        '項目長チェック
                        ModelState.AddModelError("SPECIFIED_QTY", CommonLabel.MSG_ERR_DATAOVER)
                        blnReturn = False
                    End If
                End If
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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelLocation.TITLE)

            'ロケ区分が大区分以外の場合は値をクリアする      '--2021/11/16 add nv iwamoto
            If _Model.LOC_DIV <> LOC_DIV.LocationL Then
                _Model.LOC_SHARE_FLG = String.Empty
                _Model.LOC_RFID_FLG = String.Empty   'RFIDタグ拠点
                _Model.OWN_INV_FLG = OwnInvFlg.No
                _Model.MAIL_SEND_ADDRESS = Nothing
                _Model.MAINTE_LEASE_NUM = Nothing
                _Model.MAINTE_LEASE_DAY = Nothing
                _Model.MAINTE_LEASE_DAY_WORK_FLAG = String.Empty
                _Model.MAINTE_DAY = Nothing
                _Model.MAINTE_DAY_WORK_FLAG = String.Empty
                _Model.SPECIFIED_QTY = Nothing          '2023/02/08 ADD Shirakawa
            End If
            _db.Location.Add(_Model)

            'ロケ区分が大区分の場合は項目配置マスタ,項目配置明細マスタを登録する
            If _Model.LOC_DIV = LOC_DIV.LocationL Then
                Try
                    '項目配置マスタ　項目配置明細マスタ　削除
                    MColSet_DetailDelete()
                    '項目配置マスタ登録
                    MColSetInsert()
                    '項目配置明細マスタ登録
                    MColSetDetailInsert()

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                             Me._Message.getMessage(CommonLabelLocation.MSG_INFO_MSTCOL_INS) & " " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_LOC_L) & ": " &
                                                             CommonRfid.ToStringNvl(_ViewModel.LOC_L_CD).Trim & "  "
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))
                Catch ex As Exception
                    ModelState.AddModelError("", CommonLabel.MSG_ERR_SYSTEM)
                End Try
            End If
        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As LocationModel = db.Location.Find(_Model.LOC_DIV, _Model.LOC_L_CD, _Model.LOC_M_CD, _Model.LOC_S_CD)

            '他端末で更新されているかチェックする
            If model Is Nothing Then
                '該当データが存在しない場合エラー
                ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                blnReturn = False
            End If
            If model IsNot Nothing Then
                If _Model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") <> model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") Then
                    'モデルとDBの更新日付が異なっている場合エラー
                    ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                    blnReturn = False
                End If
            End If

            '貸出日数が未入力の場合に、稼働日考慮がチェックされていればエラー               '2021/11/08 add nv iwamoto
            If _Model.MAINTE_LEASE_DAY Is Nothing AndAlso _Model.MAINTE_LEASE_DAY_WORK_FLAG = WorkDayFlg.Yes Then
                ModelState.AddModelError("MAINTE_LEASE_DAY", CommonLabelCategory.MSG_ERR_CANNOT_CHECK_MAINTE_LEASE_DAY_WORK_FLAG)
                blnReturn = False
            End If

            'メンテナンス日数が未入力の場合に、稼働日考慮がチェックされていればエラー       '2021/11/08 add nv iwamoto
            If _Model.MAINTE_DAY Is Nothing AndAlso _Model.MAINTE_DAY_WORK_FLAG = WorkDayFlg.Yes Then
                ModelState.AddModelError("MAINTE_DAY", CommonLabelCategory.MSG_ERR_CANNOT_CHECK_MAINTE_DAY_WORK_FLAG)
                blnReturn = False
            End If

            '指定数量
            If _Model.SPECIFIED_QTY IsNot Nothing Then
                Dim srtSpecified As String = _Model.SPECIFIED_QTY
                Dim sStrcnt As String() = srtSpecified.ToString().Split("."c)
                If sStrcnt(0).Length > itemlength.SpecifiedQty Then
                    '項目長チェック
                    ModelState.AddModelError("SPECIFIED_QTY", CommonLabel.MSG_ERR_DATAOVER)
                    blnReturn = False
                End If
                '小数点部があったなら小数点部のチェックを行う
                If sStrcnt.Length > 1 Then
                    If sStrcnt(1).Length > itemlength.SpecifiedQtyDecimal Then
                        '項目長チェック
                        ModelState.AddModelError("SPECIFIED_QTY", CommonLabel.MSG_ERR_DATAOVER)
                        blnReturn = False
                    End If
                End If
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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelLocation.TITLE)

            'ロケ区分が大区分以外の場合は値をクリアする      '--2021/11/16 add nv iwamoto
            If _Model.LOC_DIV <> LOC_DIV.LocationL Then
                _Model.LOC_SHARE_FLG = String.Empty
                _Model.LOC_RFID_FLG = String.Empty   'RFIDタグ拠点
                _Model.OWN_INV_FLG = OwnInvFlg.No
                _Model.MAIL_SEND_ADDRESS = Nothing
                _Model.MAINTE_LEASE_NUM = Nothing
                _Model.MAINTE_LEASE_DAY = Nothing
                _Model.MAINTE_LEASE_DAY_WORK_FLAG = String.Empty
                _Model.MAINTE_DAY = Nothing
                _Model.MAINTE_DAY_WORK_FLAG = String.Empty
                _Model.SPECIFIED_QTY = Nothing         '2023/02/08 ADD Shirakawa
            End If

            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified

        End Sub

#Region "項目配置マスタ登録処理"
        ''' <summary>
        ''' 項目配置マスタ登録処理
        ''' </summary>
        Public Sub MColSetInsert()

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.Clear()
            sbSQL.AppendLine("INSERT INTO M_COL_SET")
            sbSQL.AppendLine("(")
            sbSQL.AppendLine("   DISP_ID")
            sbSQL.AppendLine("  ,LOC_L_CD")
            sbSQL.AppendLine("  ,DISP_ORDER")
            sbSQL.AppendLine("  ,TOOL_DIV")
            sbSQL.AppendLine("  ,WORD_DIV")
            sbSQL.AppendLine("  ,WORD_CD")
            sbSQL.AppendLine("  ,DISP_FLG")
            sbSQL.AppendLine("  ,FIX_COL_FLG")
            sbSQL.AppendLine("  ,FIX_COL_CNT")
            sbSQL.AppendLine("  ,INS_DATETIME")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_DATETIME")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_TASK")
            sbSQL.AppendLine(")  ")
            sbSQL.AppendLine(" SELECT ")
            sbSQL.AppendLine(" DISP_ID")
            sbSQL.AppendLine(",@LOC_L_CD")
            sbSQL.AppendLine("  ,DISP_ORDER")
            sbSQL.AppendLine("  ,TOOL_DIV")
            sbSQL.AppendLine("  ,WORD_DIV")
            sbSQL.AppendLine("  ,WORD_CD")
            sbSQL.AppendLine("  ,DISP_FLG")
            sbSQL.AppendLine("  ,FIX_COL_FLG")
            sbSQL.AppendLine("  ,FIX_COL_CNT")
            sbSQL.AppendLine(",@INS_DATETIME")
            sbSQL.AppendLine(",@INS_EMPLOYEE_CD")
            sbSQL.AppendLine(",@INS_EMPLOYEE_NAME")
            sbSQL.AppendLine(",@UPD_DATETIME")
            sbSQL.AppendLine(",@UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(",@UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(",@UPD_TASK")
            sbSQL.AppendLine(" FROM M_COL_SET")
            sbSQL.AppendLine(" WHERE  LOC_L_CD = '" & COLSET_LOC.TEMPLATE & "'")

            params.Add(New SqlParameter($"@LOC_L_CD", _Model.LOC_L_CD))
            params.Add(New SqlParameter($"@INS_DATETIME", _Model.INS_DATETIME))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _Model.INS_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _Model.INS_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_DATETIME", _Model.UPD_DATETIME))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _Model.UPD_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _Model.UPD_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_TASK", _Model.UPD_TASK))

            _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
        End Sub
#End Region

#Region "項目配置明細マスタ登録処理"
        ''' <summary>
        ''' 項目配置明細マスタ登録処理
        ''' </summary>
        Public Sub MColSetDetailInsert()

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.Clear()
            sbSQL.AppendLine("INSERT INTO M_COL_SET_DETAIL")
            sbSQL.AppendLine("(")
            sbSQL.AppendLine("   DISP_ID")
            sbSQL.AppendLine("  ,LOC_L_CD")
            sbSQL.AppendLine("  ,COLUMN_NAME")
            sbSQL.AppendLine("  ,DISP_ORDER")
            sbSQL.AppendLine("  ,TOOL_DIV")
            sbSQL.AppendLine("  ,WORD_DIV")
            sbSQL.AppendLine("  ,WORD_CD")
            sbSQL.AppendLine("  ,DISP_FLAG")
            sbSQL.AppendLine("  ,INS_DATETIME")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_DATETIME")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_TASK")
            sbSQL.AppendLine(")  ")
            sbSQL.AppendLine(" SELECT ")
            sbSQL.AppendLine(" DISP_ID")
            sbSQL.AppendLine(",@LOC_L_CD")
            sbSQL.AppendLine("  ,COLUMN_NAME")
            sbSQL.AppendLine("  ,DISP_ORDER")
            sbSQL.AppendLine("  ,TOOL_DIV")
            sbSQL.AppendLine("  ,WORD_DIV")
            sbSQL.AppendLine("  ,WORD_CD")
            sbSQL.AppendLine("  ,DISP_FLAG")
            sbSQL.AppendLine(",@INS_DATETIME")
            sbSQL.AppendLine(",@INS_EMPLOYEE_CD")
            sbSQL.AppendLine(",@INS_EMPLOYEE_NAME")
            sbSQL.AppendLine(",@UPD_DATETIME")
            sbSQL.AppendLine(",@UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(",@UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(",@UPD_TASK")
            sbSQL.AppendLine(" FROM M_COL_SET_DETAIL")
            sbSQL.AppendLine(" WHERE  LOC_L_CD = '" & COLSET_LOC.TEMPLATE & "'")

            params.Add(New SqlParameter($"@LOC_L_CD", _Model.LOC_L_CD))
            params.Add(New SqlParameter($"@INS_DATETIME", _Model.INS_DATETIME))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _Model.INS_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _Model.INS_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_DATETIME", _Model.UPD_DATETIME))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _Model.UPD_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _Model.UPD_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_TASK", _Model.UPD_TASK))

            _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
        End Sub
#End Region
#Region "項目配置マスタ 項目配置明細マスタ削除処理"
        ''' <summary>
        ''' 項目配置マスタ 項目配置明細マスタ 削除処理
        ''' </summary>
        Public Sub MColSet_DetailDelete()
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            '項目配置マスタ
            sbSQL.Clear()
            sbSQL.AppendLine("DELETE FROM M_COL_SET")
            sbSQL.AppendLine(" WHERE  LOC_L_CD = '" & _Model.LOC_L_CD & "'")
            'sbSQL.AppendLine(" WHERE DISP_ID = '" & DispId.LOCATION & "'")
            'sbSQL.AppendLine(" AND  LOC_L_CD = '" & _Model.LOC_L_CD & "'")
            _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray())

            '項目配置明細マスタ
            sbSQL.Clear()
            sbSQL.AppendLine("DELETE FROM M_COL_SET_DETAIL")
            sbSQL.AppendLine(" WHERE  LOC_L_CD = '" & _Model.LOC_L_CD & "'")
            'sbSQL.AppendLine(" WHERE DISP_ID = '" & DispId.LOCATION & "'")
            'sbSQL.AppendLine(" AND  LOC_L_CD = '" & _Model.LOC_L_CD & "'")
            _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray())

            _ViewModel.SearchLocL = String.Empty
        End Sub
#End Region

        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="ID">ID</param>
        ''' <returns>View表示内容</returns>
        Public Function GetEditData(ByVal ID As String) As JsonResult
            Dim db As New SystemDBContext

            'ロケ大、中の選択肢を自分が権限をもっているロケにする
            'ログイン情報を取得。
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'IDをキーにモデルを取得する
            Dim findKeys As String() = ID.Split(",")
            Dim model As LocationModel = db.Location.Find(findKeys)
            model.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            model.LocLName = Dao.ToolLocation.GetLocationName(LOC_DIV.LocationL, model.LOC_L_CD, " ", " ")
            model.LocMName = Dao.ToolLocation.GetLocationName(LOC_DIV.LocationM, model.LOC_L_CD, model.LOC_M_CD, " ")
            model.LocSName = Dao.ToolLocation.GetLocationName(LOC_DIV.LocationS, model.LOC_L_CD, model.LOC_M_CD, model.LOC_S_CD)

            Return Json(model)
        End Function

#End Region

#Region "資産マスタ削除処理"
        ''' <summary>
        ''' 資産マスタ削除処理
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function DeleteAsset(ByVal ViewModel As LocationViewModel, ByVal OnCreteButton As String) As ActionResult
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

            Try
                '資産マスタ削除処理
                Dao.ToolAsset.DeleteAssetByLocLCd(_ViewModel.EditLocLCd)
                _ViewModel.States = StatusType.SuccessEdit
            Catch ex As Exception
                _ViewModel.States = ""
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_DB_SQL_ERR
            End Try

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabelLocation.MSG_INFO_ASSET_DEL) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_MNG_LOC) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.EditLocLCd).Trim & "  "
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_LOCATION) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
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
        Public Overridable Function Delete(ByVal ViewModel As LocationViewModel) As ActionResult
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

            '削除処理
            MyBase.BaseDelete(_ViewModel)

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                '操作ログ出力
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_DIV) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_DIV).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_L_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_M_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_S_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LOC_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LOC_NAME).Trim
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))

                _ViewModel.States = StatusType.SuccessDelete
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
            Session(CommonSessionKey.CREAGE_LOCATION) = _ViewModel

        End Sub

        ''' <summary>
        ''' 削除処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function DeleteChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As LocationModel = db.Location.Find(_Model.LOC_DIV, _Model.LOC_L_CD, _Model.LOC_M_CD, _Model.LOC_S_CD)

            '他の箇所で使用されている場合エラー
            If Dao.ToolLocation.IsUseLoc(_Model.LOC_DIV, _Model.LOC_L_CD, _Model.LOC_M_CD, _Model.LOC_S_CD) Then    '2021/12/16 nv miyaji 引数を追加したため修正
                'If Dao.ToolLocation.IsUseLoc(_Model.LOC_L_CD, _Model.LOC_M_CD, _Model.LOC_S_CD) Then   '2021/12/16 nv miyaji 引数を追加したため修正
                ModelState.AddModelError("LOC_DIV", CommonErrorMsg.MSG_ERR_USED_LOCATION)
                blnReturn = False
            End If

            '他端末で更新されているかチェックする
            If model Is Nothing Then
                '該当データが存在しない場合エラー
                ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                blnReturn = False
            End If
            If model IsNot Nothing Then
                If _Model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") <> model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") Then
                    'モデルとDBの更新日付が異なっている場合エラー
                    ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                    blnReturn = False
                End If
            End If

            Return blnReturn
        End Function

        ''' <summary>
        ''' 削除処理のクエリを作成します
        ''' </summary>
        Protected Overrides Sub DeleteQuery()
            Dim model As LocationModel = _db.Location.Find(_Model.LOC_DIV, _Model.LOC_L_CD, _Model.LOC_M_CD, _Model.LOC_S_CD)

            '削除を実行する
            _db.Location.Remove(model)

            'ロケ区分が大区分の場合は項目配置マスタ,項目配置明細マスタを削除する
            If _Model.LOC_DIV = LOC_DIV.LocationL Then
                Try
                    '項目配置マスタ　項目配置明細マスタ　削除
                    MColSet_DetailDelete()
                Catch ex As Exception
                    ModelState.AddModelError("", CommonLabel.MSG_ERR_SYSTEM)
                End Try
            End If
        End Sub
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
                sProcessContents = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))

                Return Json(Me._lstErr)
            End If

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVIMPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'CSVファイルをアップロード
            sCsvPath = Path.Combine(sFullPath, CsvFile.FileName)
            Dim sCsvUrl As String = Path.Combine(sFullPath, CsvFile.FileName)
            CsvFile.SaveAs(sCsvPath)

            'CSVヘッダー取得
            Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_LOCATION)
            Dim stArrayData As String() = sHeader.Split(","c)

            'CSVデータ取得 & 項目数チェック
            Dim isKomokusuuErr As Boolean = False
            Dim tbl As DataTable = clsSV_CsvFile.ReadCsvFileToDataTable(sCsvPath, Encoding.UTF8, CChar(","), True, "Location", stArrayData.Length, isKomokusuuErr)
            '項目数エラーの場合
            If isKomokusuuErr Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " &
                                                     Path.GetFileName(CsvFile.FileName)
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))

                Return Json(Me._lstErr)
            End If

            'エラーチェック
            If Not IsDataError(tbl, stArrayData) Then
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " &
                                                     Path.GetFileName(CsvFile.FileName) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                                     Me._lstErr.Count.ToString
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))

                Return Json(Me._lstErr)
            End If

            '--登録処理開始
            Dim db As New SystemDBContext
            Dim model As List(Of LocationModel) = New List(Of LocationModel)    '登録用モデル
            Dim colSetmodel As List(Of ColSetModel) = New List(Of ColSetModel)    '項目配置マスタ	登録用モデル 
            Dim colSetDetailModel As List(Of ColSetDetailModel) = New List(Of ColSetDetailModel)    '項目配置明細マスタ登録用モデル 
            Dim colSetIndex As Integer = 0
            Dim colSetDetailIndex As Integer = 0
            Dim subdb As New SystemDBContext
            ''トランザクションスタート 　データ編集後に移動
            'Using tran = db.Database.BeginTransaction()
            '    Try

            '        'ロケーションマスタデータを全削除
            '        db.Database.ExecuteSqlCommand("DELETE FROM M_LOCATION")

            Try
                '項目配置マスタデータテンプレート取得
                Dim sbSQL As New StringBuilder
                Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
                params.Clear()
                sbSQL.Clear()
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("  DISP_ID")
                sbSQL.AppendLine(", LOC_L_CD")
                sbSQL.AppendLine(", DISP_ORDER")
                sbSQL.AppendLine(", TOOL_DIV")
                sbSQL.AppendLine(", WORD_DIV")
                sbSQL.AppendLine(", WORD_CD")
                sbSQL.AppendLine(", FIX_COL_FLG")
                sbSQL.AppendLine(", FIX_COL_CNT")
                sbSQL.AppendLine(", DISP_FLG")
                sbSQL.AppendLine(", INS_DATETIME")
                sbSQL.AppendLine(", INS_EMPLOYEE_CD")
                sbSQL.AppendLine(", INS_EMPLOYEE_NAME")
                sbSQL.AppendLine(", UPD_DATETIME")
                sbSQL.AppendLine(", UPD_EMPLOYEE_CD")
                sbSQL.AppendLine(", UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine(", UPD_TASK")
                sbSQL.AppendLine(" FROM M_COL_SET")
                sbSQL.AppendLine(" WHERE LOC_L_CD = '" & COLSET_LOC.TEMPLATE & "'")
                Dim colSetDataModel As List(Of ColSetModel) = subdb.ColSet.SqlQuery(sbSQL.ToString, params.ToArray).ToList

                '項目配置明細マスタデータテンプレート取得
                params.Clear()
                sbSQL.Clear()
                sbSQL.AppendLine("SELECT")
                sbSQL.AppendLine("  DISP_ID")
                sbSQL.AppendLine(", LOC_L_CD")
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
                sbSQL.AppendLine(" FROM M_COL_SET_DETAIL")
                sbSQL.AppendLine(" WHERE LOC_L_CD = '" & COLSET_LOC.TEMPLATE & "'")
                Dim colSetDetailmodelData As List(Of ColSetDetailModel) = subdb.ColSetDetail.SqlQuery(sbSQL.ToString, params.ToArray).ToList

                '登録用モデルにデータを入れる
                For icnt As Integer = 0 To tbl.Rows.Count - 1
                    Dim locLCd As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.LocLCd).ToString.Trim.TrimStart("'"c)
                    Dim locMCd As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.LocMCd).ToString.Trim.TrimStart("'"c)
                    Dim locSCd As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.LocSCd).ToString.Trim.TrimStart("'"c)

                    'ロケ区分を取得します
                    Dim locDiv As String = Dao.ToolLocation.GetLocDiv(locLCd, locMCd, locSCd)
                    '該当するロケ区分がなかった場合は次行に遷移
                    If String.IsNullOrEmpty(locDiv) Then
                        Continue For
                    End If

                    model.Add(New LocationModel())
                    model(icnt).LOC_DIV = locDiv
                    Select Case model(icnt).LOC_DIV
                        Case LOC_DIV.LocationL
                            model(icnt).LOC_L_CD = locLCd
                            model(icnt).LOC_M_CD = " "
                            model(icnt).LOC_S_CD = " "
                            model(icnt).LOC_NAME = tbl.Rows(icnt).Item(CsvLocation.ColIdx.LocLName).ToString.Trim

                        Case LOC_DIV.LocationM
                            model(icnt).LOC_L_CD = locLCd
                            model(icnt).LOC_M_CD = locMCd
                            model(icnt).LOC_S_CD = " "
                            model(icnt).LOC_NAME = tbl.Rows(icnt).Item(CsvLocation.ColIdx.LocMName).ToString.Trim

                        Case LOC_DIV.LocationS
                            model(icnt).LOC_L_CD = locLCd
                            model(icnt).LOC_M_CD = locMCd
                            model(icnt).LOC_S_CD = locSCd
                            model(icnt).LOC_NAME = tbl.Rows(icnt).Item(CsvLocation.ColIdx.LocSName).ToString.Trim
                    End Select
                    'Add Start 2021.08.23 Iwamoto

                    If model(icnt).LOC_DIV = LOC_DIV.LocationL Then
                        'model(icnt).LOC_SHARE_FLG = tbl.Rows(icnt).Item(CsvLocation.ColIdx.LocShareFlg).ToString.Trim
                        '--共通拠点フラグ 初期値Nをいれておく
                        model(icnt).LOC_SHARE_FLG = LocShareFlg.No
                    Else
                        model(icnt).LOC_SHARE_FLG = String.Empty
                    End If

                    'RFIDタグ拠点
                    Dim sLocRfidFlg As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.LocRfidFlg).ToString.Trim
                    If model(icnt).LOC_DIV = LOC_DIV.LocationL Then
                        model(icnt).LOC_RFID_FLG = sLocRfidFlg
                    Else
                        model(icnt).LOC_RFID_FLG = String.Empty
                    End If
                    Dim sOwnInvFlg As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.OwnInvFlg).ToString.Trim
                    If model(icnt).LOC_DIV = LOC_DIV.LocationL Then
                        model(icnt).OWN_INV_FLG = sOwnInvFlg
                    Else
                        model(icnt).OWN_INV_FLG = OwnInvFlg.No
                    End If
                    '--メール送信先
                    If model(icnt).LOC_DIV = LOC_DIV.LocationL Then
                        model(icnt).MAIL_SEND_ADDRESS = tbl.Rows(icnt).Item(CsvLocation.ColIdx.MailSendAddress).ToString.Trim
                    Else
                        model(icnt).MAIL_SEND_ADDRESS = Nothing
                    End If
                    '--メンテ発生貸出回数
                    Dim sMainteLeaseNum As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.MainteLeaseNum).ToString.Trim
                    If model(icnt).LOC_DIV = LOC_DIV.LocationL AndAlso Not String.IsNullOrEmpty(sMainteLeaseNum) Then
                        model(icnt).MAINTE_LEASE_NUM = Integer.Parse(sMainteLeaseNum)
                    Else
                        model(icnt).MAINTE_LEASE_NUM = Nothing
                    End If
                    '--メンテ発生貸出日数
                    Dim sMainteLeaseDay As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.MainteLeaseDay).ToString.Trim
                    If model(icnt).LOC_DIV = LOC_DIV.LocationL AndAlso Not String.IsNullOrEmpty(sMainteLeaseDay) Then
                        model(icnt).MAINTE_LEASE_DAY = Integer.Parse(sMainteLeaseDay)
                    Else
                        model(icnt).MAINTE_LEASE_DAY = Nothing
                    End If
                    '--メンテ発生貸出日数稼働日フラグ
                    '--2021/11/08 upd start nv iwamoto
                    'model(icnt).MAINTE_LEASE_DAY_WORK_FLAG = tbl.Rows(icnt).Item(CsvLocation.ColIdx.MainteLeaseDayWorkFlag).ToString.Trim
                    Dim sMainteLeaseDayWorkFlag As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.MainteLeaseDayWorkFlag).ToString.Trim
                    If model(icnt).LOC_DIV = LOC_DIV.LocationL Then
                        If model(icnt).MAINTE_LEASE_DAY.HasValue Then
                            model(icnt).MAINTE_LEASE_DAY_WORK_FLAG = sMainteLeaseDayWorkFlag
                        Else
                            model(icnt).MAINTE_LEASE_DAY_WORK_FLAG = WorkDayFlg.No
                        End If
                    Else
                        model(icnt).MAINTE_LEASE_DAY_WORK_FLAG = String.Empty
                    End If
                    '--2021/11/08 upd end nv iwamoto
                    '--メンテ日数
                    Dim sMainteDay As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.MainteDay).ToString.Trim
                    If model(icnt).LOC_DIV = LOC_DIV.LocationL AndAlso Not String.IsNullOrEmpty(sMainteDay) Then
                        model(icnt).MAINTE_DAY = Integer.Parse(sMainteDay)
                    Else
                        model(icnt).MAINTE_DAY = Nothing
                    End If
                    '--メンテ日数稼働日フラグ
                    '--2021/11/08 upd start nv iwamoto
                    'model(icnt).MAINTE_DAY_WORK_FLAG = tbl.Rows(icnt).Item(CsvLocation.ColIdx.MainteDayWorkFlag).ToString.Trim
                    Dim sMainteDayWorkFlag As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.MainteDayWorkFlag).ToString.Trim
                    If model(icnt).LOC_DIV = LOC_DIV.LocationL Then
                        If model(icnt).MAINTE_DAY.HasValue Then
                            model(icnt).MAINTE_DAY_WORK_FLAG = sMainteDayWorkFlag
                        Else
                            model(icnt).MAINTE_DAY_WORK_FLAG = WorkDayFlg.No
                        End If
                    Else
                        model(icnt).MAINTE_DAY_WORK_FLAG = String.Empty
                    End If
                    '--2021/11/08 upd end nv iwamoto
                    'Add End 2021.08.23 Iwamoto
                    '--2023/02/08 ADD Start Shirakawa
                    '--指定数量
                    Dim sSpecifiedQty As String = tbl.Rows(icnt).Item(CsvLocation.ColIdx.SpecifiedQty).ToString.Trim
                    If model(icnt).LOC_DIV = LOC_DIV.LocationL AndAlso Not String.IsNullOrEmpty(sSpecifiedQty) Then
                        model(icnt).SPECIFIED_QTY = Decimal.Parse(sSpecifiedQty)
                    Else
                        model(icnt).SPECIFIED_QTY = Nothing
                    End If
                    '--2023/02/08 ADD End Shirakawa

                    model(icnt).INS_DATETIME = CommonRfid.getNowDate
                    model(icnt).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                    model(icnt).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                    model(icnt).UPD_DATETIME = CommonRfid.getNowDate
                    model(icnt).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                    model(icnt).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                    model(icnt).UPD_TASK = Me._Message.getMessage(CommonLabelLocation.TITLE)

                    '項目配置マスタ 項目配置明細マスタ	編集
                    If model(icnt).LOC_DIV = LOC_DIV.LocationL Then
                        If GetColSetDataCount(locLCd) = 0 Then
                            For Each row In colSetDataModel
                                colSetmodel.Add(New ColSetModel())
                                colSetmodel(colSetIndex).DISP_ID = row.DISP_ID
                                colSetmodel(colSetIndex).LOC_L_CD = locLCd
                                colSetmodel(colSetIndex).DISP_ORDER = row.DISP_ORDER
                                colSetmodel(colSetIndex).TOOL_DIV = row.TOOL_DIV
                                colSetmodel(colSetIndex).WORD_DIV = row.WORD_DIV
                                colSetmodel(colSetIndex).WORD_CD = row.WORD_CD
                                colSetmodel(colSetIndex).DISP_FLG = row.DISP_FLG
                                colSetmodel(colSetIndex).FIX_COL_FLG = row.FIX_COL_FLG
                                colSetmodel(colSetIndex).FIX_COL_CNT = row.FIX_COL_CNT
                                colSetmodel(colSetIndex).INS_DATETIME = CommonRfid.getNowDate
                                colSetmodel(colSetIndex).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                                colSetmodel(colSetIndex).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                                colSetmodel(colSetIndex).UPD_DATETIME = CommonRfid.getNowDate
                                colSetmodel(colSetIndex).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                                colSetmodel(colSetIndex).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                                colSetmodel(colSetIndex).UPD_TASK = Me._Message.getMessage(CommonLabelLocation.TITLE)
                                colSetIndex += 1
                            Next
                        End If

                        If GetColSetDetailDataCount(locLCd) = 0 Then
                            '項目配置詳細マスタを編集
                            For Each row In colSetDetailmodelData
                                colSetDetailModel.Add(New ColSetDetailModel())
                                colSetDetailModel(colSetDetailIndex).DISP_ID = row.DISP_ID
                                colSetDetailModel(colSetDetailIndex).LOC_L_CD = locLCd
                                colSetDetailModel(colSetDetailIndex).COLUMN_NAME = row.COLUMN_NAME
                                colSetDetailModel(colSetDetailIndex).DISP_ORDER = row.DISP_ORDER
                                colSetDetailModel(colSetDetailIndex).TOOL_DIV = row.TOOL_DIV
                                colSetDetailModel(colSetDetailIndex).WORD_DIV = row.WORD_DIV
                                colSetDetailModel(colSetDetailIndex).WORD_CD = row.WORD_CD
                                colSetDetailModel(colSetDetailIndex).DISP_FLAG = row.DISP_FLAG
                                colSetDetailModel(colSetDetailIndex).INS_DATETIME = CommonRfid.getNowDate
                                colSetDetailModel(colSetDetailIndex).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                                colSetDetailModel(colSetDetailIndex).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                                colSetDetailModel(colSetDetailIndex).UPD_DATETIME = CommonRfid.getNowDate
                                colSetDetailModel(colSetDetailIndex).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                                colSetDetailModel(colSetDetailIndex).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                                colSetDetailModel(colSetDetailIndex).UPD_TASK = Me._Message.getMessage(CommonLabelLocation.TITLE)
                                colSetDetailIndex += 1
                            Next
                        End If
                    End If
                Next
            Catch ex As Exception
                logger.Error(ex.Message)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            End Try

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try

                    'ロケーションマスタデータを全削除
                    db.Database.ExecuteSqlCommand("DELETE FROM M_LOCATION")

                    'システムコンテキストにモデルを追加
                    db.Location.AddRange(model)

                    '項目配置マスタ  
                    If colSetmodel.Count > 0 Then
                        db.ColSet.AddRange(colSetmodel)
                    End If
                    '項目配置明細マスタ
                    If colSetDetailModel.Count > 0 Then
                        db.ColSetDetail.AddRange(colSetDetailModel)
                    End If

                    '登録処理を実行
                    db.SaveChanges()

                    tran.Commit()

                    '操作ログ出力
                    sProcessContents = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                             Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                             tbl.Rows.Count.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    'logger.Error(sbSQL)    'マスタ系はSQLでの登録ではないのではずしておく
                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using

            Return Json(Me._lstErr)
        End Function
#End Region
#Region "項目配置マスタデータ(ロケ大区分が一致するデータの登録件数)を取得する"
        ''' <summary>
        ''' 項目配置マスタデータ(ロケ大区分が一致するデータの登録件数)を取得する
        ''' <param name="locLCd">ロケ大区分</param>
        ''' <returns>件数</returns>
        ''' </summary>
        Private Function GetColSetDataCount(ByVal locLCd As String) As Integer
            Dim total As Integer = 0

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_ASSETテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("  COUNT(*) AS CNT ")
                sbSQL.Append("FROM ")
                sbSQL.Append("  M_COL_SET ")
                sbSQL.Append("WHERE ")
                sbSQL.Append("  LOC_L_CD = '" & locLCd & "'")

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                total = Integer.Parse(tbl.Rows(0).Item("CNT"))
                Return total

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return total
        End Function
#End Region
#Region "項目配置明細マスタデータ(ロケ大区分が一致するデータの登録件数)を取得する"
        ''' <summary>
        ''' 項目配置明細マスタデータ(ロケ大区分が一致するデータの登録件数)を取得する
        ''' <param name="locLCd">ロケ大区分</param>
        ''' <returns>件数</returns>
        ''' </summary>
        Private Function GetColSetDetailDataCount(ByVal locLCd As String) As Integer
            Dim total As Integer = 0

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_ASSETテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("  COUNT(*) AS CNT ")
                sbSQL.Append("FROM ")
                sbSQL.Append("  M_COL_SET_DETAIL ")
                sbSQL.Append("WHERE ")
                sbSQL.Append("  LOC_L_CD = '" & locLCd & "'")

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                total = Integer.Parse(tbl.Rows(0).Item("CNT"))
                Return total

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return total
        End Function
#End Region
#Region "全データCSV出力処理"
        ''' <summary>
        ''' 全データCSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        Public Function CsvExport() As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            Dim LocationData As List(Of LocationCsvModel)

            'ロケーションマスタデータ取得
            LocationData = GetAllLocationData()




            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.Location.KeyDiv, CommonSettingKey.Location.Key1, CommonSettingKey.Location.Key2, CommonSettingKey.Location.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_LOCATION)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_LOCATION)
            Dim singleQuotation As String = "'"

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As LocationCsvModel In LocationData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_NAME)

                'Add Start 2021.08.23 Iwamoto
                ' col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_SHARE_FLG)
                'RFIDタグ拠点
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_RFID_FLG)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.OWN_INV_FLG)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAIL_SEND_ADDRESS)
                If row.MAINTE_LEASE_NUM.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_NUM)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                If row.MAINTE_LEASE_DAY.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_DAY)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                '--2021/11/08 upd start nv iwamoto
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_DAY_WORK_FLAG)
                If row.MAINTE_LEASE_DAY.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_DAY_WORK_FLAG)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                '--2021/11/08 upd end nv iwamoto
                If row.MAINTE_DAY.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_DAY)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                '--2021/11/08 upd start nv iwamoto
                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_DAY_WORK_FLAG)
                If row.MAINTE_DAY.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_DAY_WORK_FLAG)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                '--2021/11/08 upd end nv iwamoto
                'Add End 2021.08.23 Iwamoto
                '--2023/02/08 ADD Start Shirakawa
                If row.SPECIFIED_QTY.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.SPECIFIED_QTY)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                '--2023/02/08 ADD End Shirakawa
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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     LocationData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetAllLocationData() As List(Of LocationCsvModel)

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Return Nothing
            End If

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Location, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.Append("SELECT ")
            sbSQL.Append("  LOC_BASE.LOC_DIV ")
            sbSQL.Append("  , LOC_BASE.LOC_L_CD ")
            sbSQL.Append("  , LOC_BASE.LOC_M_CD ")
            sbSQL.Append("  , LOC_BASE.LOC_S_CD ")
            sbSQL.Append("  , LOC_L.LOC_NAME AS LOC_L_NAME ")
            sbSQL.Append("  , LOC_M.LOC_NAME AS LOC_M_NAME ")
            sbSQL.Append("  , LOC_BASE.LOC_NAME AS LOC_S_NAME ")
            sbSQL.Append("  , LOC_BASE.LOC_SHARE_FLG ")                 'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.LOC_RFID_FLG ")                  ' RFIDタグ拠点
            sbSQL.Append("  , LOC_BASE.OWN_INV_FLG")
            sbSQL.Append("  , LOC_BASE.MAIL_SEND_ADDRESS ")             'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.MAINTE_LEASE_NUM ")              'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.MAINTE_LEASE_DAY ")              'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.MAINTE_LEASE_DAY_WORK_FLAG ")    'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.MAINTE_DAY ")                    'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.MAINTE_DAY_WORK_FLAG ")          'Add 2021.08.23 Iwamoto
            sbSQL.Append("  , LOC_BASE.SPECIFIED_QTY ")                 '2023/02/08 ADD Shirakawa

            sbSQL.Append("FROM ")
            sbSQL.Append("  M_LOCATION LOC_BASE ")
            sbSQL.Append("  LEFT JOIN M_LOCATION LOC_L ")
            sbSQL.Append("  ON  LOC_L.LOC_L_CD = LOC_BASE.LOC_L_CD ")
            sbSQL.Append("  AND LOC_L.LOC_M_CD = ' ' ")
            sbSQL.Append("  AND LOC_L.LOC_S_CD = ' ' ")
            sbSQL.Append("  LEFT JOIN M_LOCATION LOC_M ")
            sbSQL.Append("  ON  LOC_M.LOC_L_CD = LOC_BASE.LOC_L_CD ")
            sbSQL.Append("  AND LOC_M.LOC_M_CD = LOC_BASE.LOC_M_CD ")
            sbSQL.Append("  AND LOC_M.LOC_S_CD = ' ' ")

            '参照権限のあるデータのみ出力するようにしてほしい。
            '検索時、自分の権限のあるデータのみを表示する
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
            sbSQL.AppendLine("ON LOC_BASE.LOC_L_CD = ULOC.LOC_L_CD ")
            sbSQL.AppendLine($"AND ((ULOC.LOC_M_FLG='{AUTH_LOC_M_FLG.Yes}' AND ULOC.LOC_M_CD = LOC_BASE.LOC_M_CD) OR (ULOC.LOC_M_FLG = '{AUTH_LOC_M_FLG.No}')) ")
            sbSQL.Append(" ORDER BY LOC_BASE.LOC_L_CD, LOC_BASE.LOC_M_CD, LOC_BASE.LOC_S_CD, LOC_BASE.LOC_DIV")

            _paramSqlList.Clear()           'SQLパラメータのクリア
            _paramSqlList.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
            _paramSqlList.Add(New SqlParameter("@ReadAuthFlg", _AuthInfo.ReadAuthFlg))
            _paramSqlList.Add(New SqlParameter("@UpdAuthFlg", _AuthInfo.UpdAuthFlg))
            _paramSqlList.Add(New SqlParameter("@ReadHubAuthFlg", _AuthInfo.ReadHubAuthFlg))
            _paramSqlList.Add(New SqlParameter("@UpdHubAuthFlg", _AuthInfo.UpdHubAuthFlg))
            _paramSqlList.Add(New SqlParameter("@UpdAuthOption", _AuthInfo.AuthOption))

            Dim lstLocation As List(Of LocationCsvModel)   'ロケーションリスト
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstLocation = db.LocationCsv.SqlQuery(sbSQL.ToString, _paramSqlList.ToArray).ToList

            For Each item As LocationCsvModel In lstLocation
                Select Case item.LOC_DIV
                    Case LOC_DIV.LocationL
                        item.LOC_M_NAME = String.Empty
                        item.LOC_S_NAME = String.Empty
                    Case LOC_DIV.LocationM
                        item.LOC_S_NAME = String.Empty
                    Case LOC_DIV.LocationS
                        '特に無し
                End Select
            Next

            'データを返却
            Return lstLocation
        End Function
#End Region

#Region "ロケーションラベル発行用CSV出力処理"
        ''' <summary>
        ''' ロケーションラベル発行用CSV出力処理
        ''' </summary>
        ''' <param name="SelectKeys">画面で選択された項目のキー値(スラッシュ区切り)</param>
        ''' <param name="PrinterKey">選択したラベルプリンターのKEY1</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExportForLabel(ByVal ViewModel As LocationViewModel, SelectKeys As String, PrinterKey As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            Dim LocationData As List(Of LocationCsvModel)

            '--2020/02/12 miyaji upd start
            '最新のチェック状態でセッション情報を更新する(追加および削除) (キー：ロケ区分, ロケ大区分コード, ロケ中区分コード, ロケ小区分コード)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            'ロケーションマスタデータ取得
            LocationData = GetSelectLocationData(lstSelectKeys)
            'LocationData = GetSelectLocationData(SelectKeys)
            '--2020/02/12 miyaji upd end

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ラベルプリンター情報の取得
            Dim labelPrinter As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LabelPrinter.KeyDiv, PrinterKey, CommonSettingKey.LabelPrinter.Key2, CommonSettingKey.LabelPrinter.Key3, True)

            '出力ファイル名取得
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LocationLabel.KeyDiv, CommonSettingKey.LocationLabel.Key1, CommonSettingKey.LocationLabel.Key2, CommonSettingKey.LocationLabel.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

            '◆CSVファイル出力先フォルダ存在チェック
            'Dim sFullPath As String = Server.MapPath(CommonConst.ROOT_FOLDERNM & labelPrinter.DATA4)

            'Dim sFullPath As String = Server.MapPath(labelPrinter.DATA4)
            Dim sFullPath As String = System.IO.Path.GetFullPath(labelPrinter.DATA4)

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
            For Each row As LocationCsvModel In LocationData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(LabelType.LC)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(labelPrinter.DATA1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(labelPrinter.DATA2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot("1")          '発行枚数の1 を追加  Inoue 2019/09/03

                col += ""
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

                ''10025,INF,,,%1が完了しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 10025, "CSVファイルの出力")
            Else
                ''30001,CRT,,,%1に失敗しました。,OKOnly,1
                'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLocation.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_LOC_LABEL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     LocationData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLocation.TITLE))

            '正常時、正常終了のステータスを返す
            _ViewModel.States = StatusType.SuccessHakko

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_LOCATION) = _ViewModel

            '再表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
#End Region

#Region "ロケーションラベル発行用CSV出力のデータを取得"
        ''' <summary>
        ''' ロケーションラベル発行用CSV出力のデータを取得します
        ''' </summary>
        Private Function GetSelectLocationData(SelectKeys As List(Of String)) As List(Of LocationCsvModel)  '--2020/02/12 nv miyaji
            'Private Function GetSelectLocationData(SelectKeys As String) As List(Of LocationCsvModel)  '--2020/02/12 nv miyaji
            Dim lstLocation As New List(Of LocationCsvModel)   'ロケーションリスト
            Dim db As SystemDBContext = New SystemDBContext

            'Dim keys As String() = SelectKeys.Split("/")  '--2020/02/12 nv miyaji

            'For Each key As String In keys '--2020/02/12 nv miyaji
            For Each key As String In SelectKeys '--2020/02/12 nv miyaji
                Dim item As String() = key.Split(",")

                Dim sbSQL As StringBuilder = New StringBuilder
                sbSQL.Append("SELECT ")
                sbSQL.Append("  LOC_BASE.LOC_DIV ")
                sbSQL.Append("  , LOC_BASE.LOC_L_CD ")
                sbSQL.Append("  , LOC_BASE.LOC_M_CD ")
                sbSQL.Append("  , LOC_BASE.LOC_S_CD ")
                sbSQL.Append("  , LOC_L.LOC_NAME AS LOC_L_NAME ")
                sbSQL.Append("  , LOC_M.LOC_NAME AS LOC_M_NAME ")
                sbSQL.Append("  , LOC_BASE.LOC_NAME AS LOC_S_NAME ")
                sbSQL.Append("  , LOC_BASE.LOC_SHARE_FLG ")                 '2021/11/05 add nv iwamoto
                sbSQL.Append("  , LOC_BASE.LOC_RFID_FLG ")                  'RFIDタグ拠点
                sbSQL.Append("  , LOC_BASE.OWN_INV_FLG ")
                sbSQL.Append("  , LOC_BASE.MAIL_SEND_ADDRESS ")             '2021/11/05 add nv iwamoto
                sbSQL.Append("  , LOC_BASE.MAINTE_LEASE_NUM ")              '2021/11/05 add nv iwamoto
                sbSQL.Append("  , LOC_BASE.MAINTE_LEASE_DAY ")              '2021/11/05 add nv iwamoto
                sbSQL.Append("  , LOC_BASE.MAINTE_LEASE_DAY_WORK_FLAG ")    '2021/11/05 add nv iwamoto
                sbSQL.Append("  , LOC_BASE.MAINTE_DAY ")                    '2021/11/05 add nv iwamoto
                sbSQL.Append("  , LOC_BASE.MAINTE_DAY_WORK_FLAG ")          '2021/11/05 add nv iwamoto
                sbSQL.Append("  , LOC_BASE.SPECIFIED_QTY ")                 '2023/02/08 ADD Shirakawa
                sbSQL.Append("FROM ")
                sbSQL.Append("  M_LOCATION LOC_BASE ")
                sbSQL.Append("  LEFT JOIN M_LOCATION LOC_L ")
                sbSQL.Append("  ON  LOC_L.LOC_L_CD = LOC_BASE.LOC_L_CD ")
                sbSQL.Append("  AND LOC_L.LOC_M_CD = ' ' ")
                sbSQL.Append("  AND LOC_L.LOC_S_CD = ' ' ")
                sbSQL.Append("  LEFT JOIN M_LOCATION LOC_M ")
                sbSQL.Append("  ON  LOC_M.LOC_L_CD = LOC_BASE.LOC_L_CD ")
                sbSQL.Append("  AND LOC_M.LOC_M_CD = LOC_BASE.LOC_M_CD ")
                sbSQL.Append("  AND LOC_M.LOC_S_CD = ' ' ")
                sbSQL.Append("WHERE ")
                sbSQL.Append("  LOC_BASE.LOC_DIV = '" & item(0) & "' ")
                sbSQL.Append("  AND LOC_BASE.LOC_L_CD = '" & item(1) & "' ")
                sbSQL.Append("  AND LOC_BASE.LOC_M_CD = '" & item(2) & "' ")
                sbSQL.Append("  AND LOC_BASE.LOC_S_CD = '" & item(3) & "' ")
                sbSQL.Append(" ORDER BY LOC_BASE.LOC_L_CD, LOC_BASE.LOC_M_CD, LOC_BASE.LOC_S_CD, LOC_BASE.LOC_DIV")

                'SQLを実行する
                Dim lst As List(Of LocationCsvModel)
                lst = db.LocationCsv.SqlQuery(sbSQL.ToString).ToList
                If lst.Count > 0 Then
                    lstLocation.Add(lst(0))
                End If
            Next

            For Each item As LocationCsvModel In lstLocation
                Select Case item.LOC_DIV
                    Case LOC_DIV.LocationL
                        item.LOC_M_NAME = String.Empty
                        item.LOC_S_NAME = String.Empty
                    Case LOC_DIV.LocationM
                        item.LOC_S_NAME = String.Empty
                    Case LOC_DIV.LocationS
                        '特に無し
                End Select
            Next

            'データを返却
            Return lstLocation
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

#Region "読込んだCSVファイルデータのエラーチェック"
        ''' <summary>
        ''' 読込んだCSVファイルデータのエラーチェック
        ''' </summary>
        ''' <param name="tbl">チェック対象のデータテーブル</param>
        ''' <param name="stArrayData">ヘッダの項目配列</param>
        ''' <returns>True:エラー無し　False:エラー有り</returns>
        Public Function IsDataError(tbl As DataTable, stArrayData As String()) As Boolean
            Dim blRet As Boolean = False

            '--項目数チェック
            If IsError(tbl) OrElse tbl.Columns.Count <> stArrayData.Length Then
                'エラー
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
                Return blRet
            End If

            '設定情報(ロケ使用区分)取得
            Dim settingInfo As MessageRfid      'メッセージ兼設定リスト
            Dim modelLocationUsage As New LocationUsageModel       'ロケーション使用区分
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim setting As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingInfo.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            Dim sLocUsage_L As String = setting.DATA1
            Dim sLocUsage_M As String = setting.DATA2
            Dim sLocUsage_S As String = setting.DATA3

            '--マスタデータ取得
            Dim tblMst As DataTable = Dao.ToolLocation.GetLocationDataAll
            tblMst.Columns.Add("IsExist", Type.GetType("System.Boolean"))

            '--データチェック
            blRet = True
            Dim lstSumiCd As List(Of String) = New List(Of String)
            For iRowCnt As Integer = 0 To tbl.Rows.Count - 1
                '新規行がなければ追加する
                If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                    Dim lstRow As DataInportResult = New DataInportResult
                    Me._lstErr.Add(lstRow)
                End If

                Dim sLocLCd As String = tbl.Rows(iRowCnt).Item(CsvLocation.ColIdx.LocLCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.LocLCd)
                Dim sLocMCd As String = tbl.Rows(iRowCnt).Item(CsvLocation.ColIdx.LocMCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.LocMCd)
                Dim sLocSCd As String = tbl.Rows(iRowCnt).Item(CsvLocation.ColIdx.LocSCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.LocSCd)

                'ロケ区分を取得します
                Dim sLocDiv As String = Dao.ToolLocation.GetLocDiv(sLocLCd, sLocMCd, sLocSCd)
                '該当するロケ区分がなかった場合はエラー
                If String.IsNullOrEmpty(sLocDiv) Then
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_DIV)
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERR_LOC_DIV)
                    blRet = False
                End If

                '重複チェック
                If lstSumiCd.IndexOf(sLocLCd & sLocMCd & sLocSCd) > -1 Then
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_KEY_ITEM)
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_KEY_ITEM) & "[{0}]", sLocLCd & sLocMCd & sLocSCd)
                    blRet = False
                Else
                    If Not String.IsNullOrEmpty(sLocDiv) Then
                        '読取データに既存データが存在するかどうかチェック
                        For Each rowMst As DataRow In tblMst.Rows
                            Dim sMstLocLCd As String = rowMst("LOC_L_CD").ToString.Trim.PadRight(itemlength.LocLCd)
                            Dim sMstLocMCd As String = rowMst("LOC_M_CD").ToString.Trim.PadRight(itemlength.LocMCd)
                            Dim sMstLocSCd As String = rowMst("LOC_S_CD").ToString.Trim.PadRight(itemlength.LocSCd)
                            If (sMstLocLCd & sMstLocMCd & sMstLocSCd) = (sLocLCd & sLocMCd & sLocSCd) Then
                                rowMst("IsExist") = True
                                Exit For
                            End If
                        Next

                        lstSumiCd.Add(sLocLCd & sLocMCd & sLocSCd)
                    End If
                End If

                'メンテ発生貸出日数
                Dim dMainteLeaseDay As Decimal? = Nothing
                'メンテ日数
                Dim dMainteDay As Decimal? = Nothing
                '指定数量
                Dim dSpecifiedQty As Decimal? = Nothing

                For iCalCnt As Integer = 0 To stArrayData.Length - 1
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If

                    '列によってチェック条件・設定項目値を分ける
                    Dim sStr As String = tbl.Rows(iRowCnt).Item(iCalCnt).ToString.Trim.TrimStart("'"c)
                    Select Case iCalCnt
                        Case CsvLocation.ColIdx.LocLCd
                            'ロケ大区分コード
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.LocLCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If
                        Case CsvLocation.ColIdx.LocMCd
                            'ロケ中区分コード
                            If (sLocDiv = LOC_DIV.LocationM OrElse sLocDiv = LOC_DIV.LocationS) AndAlso String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.LocMCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso sLocUsage_M <> Misc.ItemUse.Yes Then
                                '設定マスタによる使用チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_CANNOT_USE_BY_SETTING)
                                blRet = False
                            End If
                        Case CsvLocation.ColIdx.LocSCd
                            'ロケ小区分コード
                            If sLocDiv = LOC_DIV.LocationS AndAlso String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.LocSCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso sLocUsage_S <> Misc.ItemUse.Yes Then
                                '設定マスタによる使用チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_CANNOT_USE_BY_SETTING)
                                blRet = False
                            End If
                        Case CsvLocation.ColIdx.LocLName
                            'ロケ大区分名称
                            If sStr.Length > itemlength.LocNm Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_NAME)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case CsvLocation.ColIdx.LocMName
                            'ロケ中区分名称
                            If sStr.Length > itemlength.LocNm Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_M_NAME)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case CsvLocation.ColIdx.LocSName
                            'ロケ小区分名称
                            If sStr.Length > itemlength.LocNm Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_S_NAME)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case CsvLocation.ColIdx.LocRfidFlg
                            If sLocDiv = LOC_DIV.LocationL Then
                                If String.IsNullOrEmpty(sStr) Then
                                    '空白チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_RFID_FLG)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                    blRet = False
                                ElseIf Not (sStr = LocRfidFlg.Yes OrElse sStr = LocRfidFlg.No) Then
                                    '値チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_RFID_FLG)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                End If

                                '既存データチェック
                                Dim LocRow As DataRow() = Nothing
                                LocRow = tblMst.Select("LOC_DIV = '" + sLocDiv + "' And LOC_L_CD = '" + sLocLCd + "'")
                                If Not LocRow Is Nothing AndAlso LocRow.Length > 0 Then
                                    'データがある場合
                                    'バーコード運用拠点からRFIDタグ拠点に変更された場合はエラーとする
                                    If LocRow(0).Item("LOC_RFID_FLG").ToString.Trim = LocRfidFlg.No AndAlso
                                        sStr = LocRfidFlg.Yes Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_RFID_FLG)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelLocation.MSG_ERR_RFID_CHANGE)
                                        blRet = False
                                    End If
                                End If
                            End If
                        Case CsvLocation.ColIdx.OwnInvFlg
                            If sLocDiv = LOC_DIV.LocationL Then
                                If String.IsNullOrEmpty(sStr) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelLocation.LABEL_OWN_INV_FLG)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                    blRet = False
                                ElseIf Not (sStr = OwnInvFlg.Yes OrElse sStr = OwnInvFlg.No) Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelLocation.LABEL_OWN_INV_FLG)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                End If
                            End If
                        Case CsvLocation.ColIdx.MailSendAddress
                            'メール送信先         '2021/11/08 add nv iwamoto
                            If sLocDiv = LOC_DIV.LocationL AndAlso Not String.IsNullOrEmpty(sStr) Then
                                '項目長チェック
                                If sStr.Length > itemlength.MailSendAddress Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAIL_SEND_ADDRESS)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                    blRet = False
                                ElseIf Not CommonRfid.IsHanEiKigou(sStr) Then
                                    '半角英数字記号チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAIL_SEND_ADDRESS)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                End If
                            End If
                        Case CsvLocation.ColIdx.MainteLeaseNum
                            'メンテ発生貸出回数              '2021/11/15 add nv iwamoto
                            If sLocDiv = LOC_DIV.LocationL AndAlso Not String.IsNullOrEmpty(sStr) Then
                                Dim dMainteLeaseNum As Decimal? = CommonRfid.ToNumber(sStr)
                                If dMainteLeaseNum.HasValue = False Then
                                    '数字チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_NUM)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                ElseIf dMainteLeaseNum < 0 Then
                                    '値チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_NUM)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NEGATIVE_VALUE)
                                    blRet = False
                                End If
                            End If
                        Case CsvLocation.ColIdx.MainteLeaseDay
                            'メンテ発生貸出日数              '2021/11/15 add nv iwamoto
                            If sLocDiv = LOC_DIV.LocationL AndAlso Not String.IsNullOrEmpty(sStr) Then
                                dMainteLeaseDay = CommonRfid.ToNumber(sStr)
                                If dMainteLeaseDay.HasValue = False Then
                                    '数字チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                ElseIf dMainteLeaseDay < 0 Then
                                    '値チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NEGATIVE_VALUE)
                                    blRet = False
                                End If
                            End If
                        Case CsvLocation.ColIdx.MainteLeaseDayWorkFlag
                            'メンテ発生貸出日数稼働日フラグ  '2021/11/15 add nv iwamoto
                            If sLocDiv = LOC_DIV.LocationL Then
                                If String.IsNullOrEmpty(sStr) Then
                                    '空白チェック
                                    'メンテ発生貸出日数が空でない時
                                    If dMainteLeaseDay.HasValue Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY_WORK_FLG)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                        blRet = False
                                    End If
                                ElseIf sStr <> MainteWorkFlg.Yes AndAlso sStr <> MainteWorkFlg.No Then
                                    '「Y」or「N」チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY_WORK_FLG)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                Else
                                    'メンテ発生貸出日数稼働日フラグにYorNが入っている時
                                    'メンテ発生貸出日数が空の時
                                    If dMainteLeaseDay.HasValue = False Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_LEASE_DAY)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                        blRet = False
                                    End If
                                End If
                            End If
                        Case CsvLocation.ColIdx.MainteDay
                            'メンテ日数                      '2021/11/15 add nv iwamoto
                            If sLocDiv = LOC_DIV.LocationL AndAlso Not String.IsNullOrEmpty(sStr) Then
                                dMainteDay = CommonRfid.ToNumber(sStr)
                                If dMainteDay.HasValue = False Then
                                    '数字チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                ElseIf dMainteDay < 0 Then
                                    '値チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NEGATIVE_VALUE)
                                    blRet = False
                                End If
                            End If
                        Case CsvLocation.ColIdx.MainteDayWorkFlag
                            'メンテ日数稼働日フラグ          '2021/11/15 add nv iwamoto
                            If sLocDiv = LOC_DIV.LocationL Then
                                If String.IsNullOrEmpty(sStr) Then
                                    '空白チェック
                                    'メンテ日数が空でない時
                                    If dMainteDay.HasValue Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY_WORK_FLG)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                        blRet = False
                                    End If
                                ElseIf sStr <> MainteWorkFlg.Yes AndAlso sStr <> MainteWorkFlg.No Then
                                    '「Y」or「N」チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY_WORK_FLG)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                Else
                                    'メンテ日数稼働日フラグにYorNが入っている時
                                    'メンテ日数が空の時
                                    If dMainteDay.HasValue = False Then
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_MAINTE_DAY)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                        blRet = False
                                    End If
                                End If
                            End If
                            '2023/02/08 ADD Start Shirakwa
                        Case CsvLocation.ColIdx.SpecifiedQty
                            '小数点の桁数と整数の桁数を取得
                            Dim sStrcnt As String() = sStr.ToString().Split("."c)
                            '指定数量                     
                            If sLocDiv = LOC_DIV.LocationL AndAlso Not String.IsNullOrEmpty(sStr) Then
                                dSpecifiedQty = CommonRfid.ToNumber(sStr)
                                If dSpecifiedQty.HasValue = False Then
                                    '数字チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_SPECIFIED_QTY)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                    blRet = False
                                ElseIf dSpecifiedQty < 0 Then
                                    '値チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_SPECIFIED_QTY)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NEGATIVE_VALUE)
                                    blRet = False
                                ElseIf sStrcnt(0).Length > itemlength.SpecifiedQty Then
                                    '項目長チェック
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_SPECIFIED_QTY)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                    blRet = False
                                End If
                                '小数点部があったなら小数点部のチェックを行う
                                If sStrcnt.Length > 1 Then
                                    If sStrcnt(1).Length > itemlength.SpecifiedQtyDecimal Then
                                        '項目長チェック
                                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_SPECIFIED_QTY)
                                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                        blRet = False
                                    End If
                                End If
                            End If
                            '2023/02/08 ADD End Shirakwa
                    End Select
                Next
            Next

            '上位チェック
            For iRowCnt As Integer = 0 To tbl.Rows.Count - 1
                '新規行がなければ追加する
                If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                    Dim lstRow As DataInportResult = New DataInportResult
                    Me._lstErr.Add(lstRow)
                End If

                Dim sLocLCd As String = tbl.Rows(iRowCnt).Item(CsvLocation.ColIdx.LocLCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.LocLCd)
                Dim sLocMCd As String = tbl.Rows(iRowCnt).Item(CsvLocation.ColIdx.LocMCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.LocMCd)
                Dim sLocSCd As String = tbl.Rows(iRowCnt).Item(CsvLocation.ColIdx.LocSCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.LocSCd)

                'ロケ区分を取得します
                Dim sLocDiv As String = Dao.ToolLocation.GetLocDiv(sLocLCd, sLocMCd, sLocSCd)
                '該当するロケ区分がなかった場合は次行に遷移
                If String.IsNullOrEmpty(sLocDiv) Then
                    Continue For
                End If

                Select Case sLocDiv
                    Case LOC_DIV.LocationM
                        '20210510 UPD Start Suzuki v1.3
                        'If lstSumiCd.IndexOf(sLocLCd & "     " & "     ") = -1 Then
                        If lstSumiCd.IndexOf(sLocLCd & "          " & "          ") = -1 Then
                            '20210510 UPD End Suzuki v1.3                            '大区分存在チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_LOC_L_CD) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD) & "[{0}]", sLocLCd)
                            blRet = False
                        End If

                    Case LOC_DIV.LocationS
                        Dim hasError As Boolean = False
                        '20210510 UPD Start Suzuki v1.3
                        'If lstSumiCd.IndexOf(sLocLCd & "     " & "     ") = -1 Then
                        If lstSumiCd.IndexOf(sLocLCd & "          " & "          ") = -1 Then
                            '20210510 UPD End Suzuki v1.3
                            '大区分存在チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_LOC_L_CD) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD) & "[{0}]", sLocLCd)
                            blRet = False
                            hasError = True
                        End If
                        '20210510 UPD Start Suzuki v1.3
                        'If lstSumiCd.IndexOf(sLocLCd & sLocMCd & "     ") = -1 Then
                        If lstSumiCd.IndexOf(sLocLCd & sLocMCd & "          ") = -1 Then
                            '20210510 UPD End Suzuki v1.3
                            '中区分存在チェック
                            If hasError Then
                                Dim lstRow As DataInportResult = New DataInportResult
                                Me._lstErr.Add(lstRow)
                            End If
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_LOC_M_CD) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD) & "[{0}]", sLocMCd)
                            blRet = False
                        End If
                End Select
            Next

            '使用チェック
            For Each rowMst As DataRow In tblMst.Rows
                '新規行がなければ追加する
                If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 OrElse Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx.HasValue = False Then
                    Dim lstRow As DataInportResult = New DataInportResult
                    Me._lstErr.Add(lstRow)
                End If

                If IsDBNull(rowMst("IsExist")) Then
                    Dim sMstLocDiv As String = rowMst("LOC_DIV").ToString
                    Dim sMstLocLCd As String = rowMst("LOC_L_CD").ToString
                    Dim sMstLocMCd As String = rowMst("LOC_M_CD").ToString
                    Dim sMstLocSCd As String = rowMst("LOC_S_CD").ToString
                    If Dao.ToolLocation.IsUseLoc(sMstLocDiv, sMstLocLCd, sMstLocMCd, sMstLocSCd, True) Then    ' 2022/03/08 戸祭 DBチェックをSKIP '2021/12/16 nv miyaji 引数を追加したため修正
                        'If Dao.ToolLocation.IsUseLoc(sMstLocLCd, sMstLocMCd, sMstLocSCd) Then    '2021/12/16 nv miyaji 引数を追加したため修正
                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = Nothing
                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GEN_LOCATION)
                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_NOT_EXIST_USED_DATA) &
                            String.Format(Me._Message.getMessage(CommonLabel.LABEL_LOC_L_CD) & "[{0}]", sMstLocLCd.Trim) & " " &
                            String.Format(Me._Message.getMessage(CommonLabel.LABEL_LOC_M_CD) & "[{0}]", sMstLocMCd.Trim) & " " &
                            String.Format(Me._Message.getMessage(CommonLabel.LABEL_LOC_S_CD) & "[{0}]", sMstLocSCd.Trim) & " " &
                            String.Format(Me._Message.getMessage(CommonLabel.LABEL_LOC_NAME) & "[{0}]", rowMst("LOC_NAME").ToString)
                        blRet = False
                    End If
                End If
            Next

            'データ項目にエラーがあった場合
            If blRet = False Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG

                '最終行が空白行であれば削除する。
                If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = 0 Then
                    Dim lstRow As DataInportResult = New DataInportResult
                    Me._lstErr.RemoveAt(Me._lstErr.Count - 1)
                End If
            End If

            Return blRet
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

            'ロケ大、中の選択肢を自分が権限をもっているロケにする
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            End If
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetHistory, _LoginInfo)

            If locationLevel = LOC_DIV.LocationM Then
                If modelLocationUsage.LocUsage_M = Misc.ItemUse.Yes Then
                    Return GetLocationMData(selectLCode)
                Else
                    '中区分 使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            Else

                If modelLocationUsage.LocUsage_S = Misc.ItemUse.Yes Then
                    Return GetLocationSData(selectLCode, selectMCode)
                Else
                    '小区分　使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
#End Region

#Region "大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 大区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationLData() As JsonResult

            ' Dao モジュールを使用するよう変更  2019/08/14 Update Inoue
            'Dim sbSQL As StringBuilder = New StringBuilder
            'Dim db As SystemDBContext = New SystemDBContext
            ''M_LOCATIONテーブルから取得するSQLを作成
            'sbSQL.Append("SELECT ")
            'sbSQL.Append(" LOC_L_CD LOC_CD")
            'sbSQL.Append("  , LOC_NAME ")
            'sbSQL.Append("FROM ")
            'sbSQL.Append("  M_LOCATION ")
            'sbSQL.Append("WHERE")
            'sbSQL.Append("  LOC_DIV = '" & LOC_DIV.LocationL & "'")

            ''SQLを実行する
            '_LocationLList = _db.LocationDropdown.SqlQuery(sbSQL.ToString).ToList()
            'ViewBag.modelLocL = New SelectList(_LocationLList, "LOC_CD", "LOC_NAME")
            '_ViewModel.locLDropdownData = _LocationLList

            'Return Json(_LocationLList)

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

            'モーダル画面のコンボ設定
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
        Private Function GetLocationMData(ByVal selectLCode As String) As JsonResult

            ' Dao モジュールを使用するよう変更  2019/08/14 Update Inoue
            'Dim sbSQL As StringBuilder = New StringBuilder
            'Dim db As SystemDBContext = New SystemDBContext
            ''M_LOCATIONテーブルから取得するSQLを作成
            'sbSQL.Append("SELECT ")
            'sbSQL.Append(" LOC_M_CD LOC_CD")
            'sbSQL.Append("  , LOC_NAME ")
            'sbSQL.Append("FROM ")
            'sbSQL.Append("  M_LOCATION ")
            'sbSQL.Append("WHERE ")
            'sbSQL.Append("  LOC_DIV = '" & LOC_DIV.LocationM & "' ")

            'If Not String.IsNullOrEmpty(selectLCode) Then
            '    sbSQL.Append("  AND LOC_L_CD = '" & selectLCode & "'")
            'End If

            ''SQLを実行する
            '_LocationMList = _db.LocationDropdown.SqlQuery(sbSQL.ToString).ToList()
            'ViewBag.modelLocM = New SelectList(_LocationMList, "LOC_CD", "LOC_NAME")
            '_ViewModel.locMDropdownData = _LocationMList

            'Return Json(_LocationMList)
            'ロケ大、中の選択肢を自分が権限をもっているロケにする 
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationMData(selectLCode, _LoginInfo, _AuthInfo)
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.modelLocM = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchLocM)
            _ViewModel.locMDropdownData = lstLocation

            'モーダル画面のコンボ設定
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
        Private Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCode As String) As JsonResult

            ' Dao モジュールを使用するよう変更  2019/08/14 Update Inoue
            'Dim sbSQL As StringBuilder = New StringBuilder
            'Dim db As SystemDBContext = New SystemDBContext
            ''M_LOCATIONテーブルから取得するSQLを作成
            'sbSQL.Append("SELECT ")
            'sbSQL.Append(" LOC_S_CD LOC_CD")
            'sbSQL.Append("  , LOC_NAME ")
            'sbSQL.Append("FROM ")
            'sbSQL.Append("  M_LOCATION ")
            'sbSQL.Append("WHERE")
            'sbSQL.Append("  LOC_DIV = '" & LOC_DIV.LocationS & "' ")

            'If Not String.IsNullOrEmpty(selectLCode) Then
            '    sbSQL.Append("  AND LOC_L_CD = '" & selectLCode & "' ")
            'End If

            'If Not String.IsNullOrEmpty(selectMCode) Then
            '    sbSQL.Append("  AND LOC_M_CD = '" & selectMCode & "' ")
            'End If

            ''SQLを実行する
            '_LocationSList = _db.LocationDropdown.SqlQuery(sbSQL.ToString).ToList()
            'ViewBag.modelLocS = New SelectList(_LocationSList, "LOC_CD", "LOC_NAME")
            '_ViewModel.locSDropdownData = _LocationSList

            'Return Json(_LocationSList)

            'ロケ大、中の選択肢を自分が権限をもっているロケにする 
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationSData(selectLCode, selectMCode, _LoginInfo, _AuthInfo)

            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.modelLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchLocS)
            _ViewModel.locSDropdownData = lstLocation

            Return Json(lstLocation)

        End Function
#End Region

#Region "ロケ区分名称取得処理"
        ''' <summary>
        ''' ロケ区分名称取得処理
        ''' </summary>
        ''' <param name="inputLCode">ロケ大区分コード</param>
        ''' <param name="inputMCode">ロケ中区分コード</param>
        ''' <param name="inputSCode">ロケ小区分コード</param>
        ''' <returns></returns>
        Public Function GetLocName(locDiv As String, ByVal inputLCode As String, ByVal inputMCode As String, ByVal inputSCode As String) As JsonResult
            '値が空の場合は半角スペースを設定
            If String.IsNullOrEmpty(inputLCode) Then
                inputLCode = " "
            End If
            If String.IsNullOrEmpty(inputMCode) Then
                inputMCode = " "
            End If
            If String.IsNullOrEmpty(inputSCode) Then
                inputSCode = " "
            End If

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext
            'M_LOCATIONテーブルから取得するSQLを作成
            sbSQL.Append("SELECT ")
            sbSQL.Append("  LOC_BASE.LOC_DIV ")
            sbSQL.Append("  , LOC_BASE.LOC_L_CD ")
            sbSQL.Append("  , LOC_BASE.LOC_M_CD ")
            sbSQL.Append("  , LOC_BASE.LOC_S_CD ")
            sbSQL.Append("  , LOC_L.LOC_NAME AS LOC_L_NAME ")
            sbSQL.Append("  , LOC_M.LOC_NAME AS LOC_M_NAME ")
            sbSQL.Append("  , LOC_BASE.LOC_NAME AS LOC_S_NAME ")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_LOCATION LOC_BASE ")
            sbSQL.Append("  LEFT JOIN M_LOCATION LOC_L ")
            sbSQL.Append("  ON  LOC_L.LOC_L_CD = LOC_BASE.LOC_L_CD ")
            sbSQL.Append("  AND LOC_L.LOC_M_CD = ' ' ")
            sbSQL.Append("  AND LOC_L.LOC_S_CD = ' ' ")
            sbSQL.Append("  LEFT JOIN M_LOCATION LOC_M ")
            sbSQL.Append("  ON  LOC_M.LOC_L_CD = LOC_BASE.LOC_L_CD ")
            sbSQL.Append("  AND LOC_M.LOC_M_CD = LOC_BASE.LOC_M_CD ")
            sbSQL.Append("  AND LOC_M.LOC_S_CD = ' ' ")
            sbSQL.Append("WHERE ")
            sbSQL.Append("  LOC_BASE.LOC_L_CD = '" & inputLCode & "'")
            sbSQL.Append("  AND LOC_BASE.LOC_M_CD = '" & inputMCode & "'")
            sbSQL.Append("  AND LOC_BASE.LOC_S_CD = '" & inputSCode & "'")

            'SQLを実行する
            Dim lst As New List(Of LocationNameModel)
            lst = db.LocationName.SqlQuery(sbSQL.ToString).ToList()

            For Each item As LocationNameModel In lst
                Select Case item.LOC_DIV
                    Case LOC_DIV.LocationL
                        item.LOC_M_NAME = String.Empty
                        item.LOC_S_NAME = String.Empty
                    Case LOC_DIV.LocationM
                        item.LOC_S_NAME = String.Empty
                    Case LOC_DIV.LocationS
                        '特に無し
                End Select
            Next

            Return Json(lst)
        End Function
#End Region

#Region "プリンター選択プルダウン用データ取得"
        ''' <summary>
        ''' プリンター選択プルダウン用データ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function ShowPrinterList() As ActionResult
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

#Region "最新のチェック状態でセッション情報を更新する"
        ''' <summary>
        ''' 最新のチェック状態でセッション情報を更新する
        ''' 重複を排除したチェックされたキーを返却する
        ''' </summary>
        ''' <param name="SelectKeys">選択キー</param>
        ''' <param name="NoneSelectKeys">未選択キー</param>
        ''' <returns>チェックされた行のキーリスト</returns>
        ''' <remarks>2020/02/12 miyaji add</remarks>
        Private Function UpdateChecedInfo(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As List(Of String)
            '登録・変更・削除ボタン押下時の一覧画面呼び出しの場合
            If Session(CommonSessionKey.CREAGE_LOCATION) IsNot Nothing Then
                '一覧にないロケーションは未選択キーとする
                If Not IsNothing(_ViewModel.LOC_DIV) And Not IsNothing(_ViewModel.LOC_L_CD) And Not IsNothing(_ViewModel.LOC_M_CD) And Not IsNothing(_ViewModel.LOC_S_CD) Then
                    Dim results = From data In _viewResult Where data.LOC_DIV = _ViewModel.LOC_DIV And data.LOC_L_CD.Trim = _ViewModel.LOC_L_CD.Trim And data.LOC_M_CD.Trim = _ViewModel.LOC_M_CD.Trim And data.LOC_S_CD.Trim = _ViewModel.LOC_S_CD.Trim
                    If results.Count <= 0 Then
                        NoneSelectKeys = String.Join("/", New String() {NoneSelectKeys, $"{_ViewModel.LOC_DIV},{_ViewModel.LOC_L_CD},{_ViewModel.LOC_M_CD},{_ViewModel.LOC_S_CD}"})
                    End If
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

#Region "全選択ボタン押下時処理"
        ''' <summary>
        ''' 全選択ボタン押下時処理
        ''' 表示されている検索条件で全件検索し
        ''' チェック項目としてセッションにセットする
        ''' </summary>
        ''' <param name="blnSelectAll"></param>
        ''' <returns></returns>
        ''' <remarks>2020/02/12 nv miyaji add</remarks>
        Public Function SelectAll(ByVal blnSelectAll As Boolean) As ActionResult
            Dim sbSQL As New StringBuilder
            Dim lstLocatList As New List(Of LocationViewModel)

            '--2020/02/12 nv miyaji upd start
            '選択解除の場合、初期化する
            If Not blnSelectAll Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする　'2020/02/12 nv miyaji　
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
                    lstLocatList = _db.LocationView.SqlQuery(strSQL).ToList
                Else

                    lstLocatList = _db.LocationView.SqlQuery(strSQL, lstParamCopy.ToArray).ToList
                End If
            End If

            'チェックボックスlstLocatListの選択状態のキー(ロケ区分・ロケ大区分コード・ロケ中区分コード・ロケ小区分コード)を更新する
            If lstLocatList.Count > 0 Then
                Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstLocatList.Select(Function(d) $"{d.LOC_DIV},{d.LOC_L_CD},{d.LOC_M_CD},{d.LOC_S_CD}").ToList
            End If

            '件数を返却する
            Return Json(lstLocatList.Count)

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
                _viewResult.Add(New LocationViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchLocL = _ViewModel.SearchLocL
            _viewResult.ElementAt(0).SearchLocM = _ViewModel.SearchLocM
            _viewResult.ElementAt(0).SearchLocS = _ViewModel.SearchLocS
            _viewResult.ElementAt(0).SearchDispSelect = _ViewModel.SearchDispSelect
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.LOCATION, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL))
            If _viewResult.ElementAt(0).GridInfo.Count > 0 Then
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.LOCATION, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL)).FIX_COL_CNT
            Else
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.LOCATION, COLSET_LOC.TEMPLATE).FIX_COL_CNT
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.LOCATION, COLSET_LOC.TEMPLATE)
            End If
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            '検索時にロケ大区分の検索条件を保存
            _viewResult.ElementAt(0).IsRestoreOwnSearchConditions = _ViewModel.IsRestoreOwnSearchConditions
            _viewResult.ElementAt(0).FirstSearch = _ViewModel.FirstSearch
        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As LocationViewModel)
            _Model.LOC_DIV = model.LOC_DIV
            _Model.LOC_L_CD = model.LOC_L_CD
            _Model.LOC_M_CD = model.LOC_M_CD
            _Model.LOC_S_CD = model.LOC_S_CD
            _Model.LOC_NAME = model.LOC_NAME
            'Add Start 2021.08.23 Iwamoto 
            '--共通拠点フラグ 初期値Nをいれておく
            _Model.LOC_SHARE_FLG = LocShareFlg.No

            If model.LocRfidFlg Then
                _Model.LOC_RFID_FLG = LocRfidFlg.Yes
            Else
                _Model.LOC_RFID_FLG = LocRfidFlg.No
            End If
            If model.OwnInvFlg Then
                _Model.OWN_INV_FLG = OwnInvFlg.Yes
            Else
                _Model.OWN_INV_FLG = OwnInvFlg.No
            End If
            _Model.MAIL_SEND_ADDRESS = model.MAIL_SEND_ADDRESS
            _Model.MAINTE_LEASE_NUM = model.MAINTE_LEASE_NUM
            _Model.MAINTE_LEASE_DAY = model.MAINTE_LEASE_DAY
            If model.MainteLeaseDayWorkFlg Then
                _Model.MAINTE_LEASE_DAY_WORK_FLAG = WorkDayFlg.Yes
            Else
                _Model.MAINTE_LEASE_DAY_WORK_FLAG = WorkDayFlg.No
            End If
            _Model.MAINTE_DAY = model.MAINTE_DAY
            If model.MainteDayWorkFlg Then
                _Model.MAINTE_DAY_WORK_FLAG = WorkDayFlg.Yes
            Else
                _Model.MAINTE_DAY_WORK_FLAG = WorkDayFlg.No
            End If
            'Add End 2021.08.23 Iwamoto
            _Model.SPECIFIED_QTY = model.SPECIFIED_QTY      '2023/02/08 ADD Shirakawa



            _Model.INS_DATETIME = model.INS_DATETIME
            _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK

        End Sub
#End Region

    End Class
End Namespace