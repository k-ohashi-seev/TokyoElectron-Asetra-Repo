Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' カテゴリーマスタメンテナンス画面表示用のクラス
    ''' </summary>
    Public Class CategoryController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"
        ''' <summary>
        ''' データ取込画面　結果表示用クラス
        ''' </summary>
        Private Class CsvCategory
            Public Enum ColIdx
                ''' <summary>大分類コード</summary>
                CatLCd
                ''' <summary>中分類コード</summary>
                CatMCd
                ''' <summary>小分類コード</summary>
                CatSCd
                ''' <summary>大分類名称</summary>
                CatLName
                ''' <summary>中分類名称</summary>
                CatMName
                ''' <summary>小分類名称</summary>
                CatSName
                ''' <summary>リース期限アラート区分</summary>
                LeaseLimitAlertDiv
                ''' <summary>リース期限何日前</summary>
                LeaseLimitBefDays
                ''' <summary>○○期限アラート区分</summary>
                GenericLimitAlertDiv
                ''' <summary>○○期限何日前</summary>
                GenericLimitBefDays
                ''' <summary>貸出アラート区分</summary>
                LeaseAlertDiv
                ''' <summary>貸出アラート回数</summary>
                LeaseAlertCnt
                ''' <summary>貸出上限エラー区分</summary>
                LeaseUpperErrDiv
                ''' <summary>貸出上限数</summary>
                LeaseUpperLimit
                ''' <summary>資産ID(EPC)登録方法</summary>
                AssetIDRegDiv
                ''' <summary>タグEPC領域サイズ</summary>
                EPCSize
                ''' <summary>タグEPC事業所キー</summary>
                EPCCompanyKey
                ''' <summary>タグEPC資産タイプ</summary>
                EPCAssetType
            End Enum
        End Class
#End Region

#Region "クラス変数"
        Private _Model As CategoryModel = New CategoryModel                                 'Categoryモデル
        Private _ViewModel As CategoryViewModel = New CategoryViewModel                     'Category一覧表示用モデル
        Protected _viewResult As List(Of CategoryViewModel) = New List(Of CategoryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報

        Private _CategoryLList As List(Of CategoryDropdownModel)     'ロケーションマスタリスト
        Private _CategoryMList As List(Of CategoryDropdownModel)     'ロケーションマスタリスト
        Private _CategorySList As List(Of CategoryDropdownModel)     'ロケーションマスタリスト
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
        Public Function Index(ByVal ViewModel As CategoryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_CATEGORY)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As CategoryViewModel = Session(CommonSessionKey.CREAGE_CATEGORY)
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
                '一覧表示処理
                MyBase.BaseIndex(_ViewModel)

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo

                '--2021/09/27 nv iwamoto add start
                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '権限情報の取得
                Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.Category, _LoginInfo)
                If authInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                Else
                    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                End If
                '--2021/09/27 nv iwamoto add end
            End If

            '直近登録情報の設定
            Dim tbl As DataTable = Dao.ToolCommon.GetMstInsInfo("M_CATEGORY")
            If tbl IsNot Nothing AndAlso tbl.Rows.Count <> 0 Then
                ViewBag.LastInsDateTime = CommonRfid.ToStringDateYMDHM(Date.Parse(tbl.Rows(0).Item("INS_DATETIME").ToString))
                ViewBag.LastInsEmployeeNm = tbl.Rows(0).Item("INS_EMPLOYEE_NAME").ToString
            End If

            'ログインセッション切れ以外のエラーが設定されていたらモーダルのコンボボックスを設定   '2019/11/19 nv miyaji
            If _ViewModel.ErrCode = String.Empty OrElse _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY Then
                ' 大分類ドロップダウンリスト作成
                GetCategoryLData()
                ' 中分類
                GetCategoryMData(_ViewModel.SearchCatL)
                ' 小分類
                GetCategorySData(_ViewModel.SearchCatL, _ViewModel.SearchCatM)
            Else
                ' 大分類ドロップダウンリスト作成
                GetCategoryLData()
                ' 中分類
                GetCategoryMData(_ViewModel.ModalCatL)
                ' 小分類
                GetCategorySData(_ViewModel.ModalCatL, _ViewModel.ModalCatM)
            End If

            '資産ID登録方法の初期値取得     'Add 2020.02.13 Iwamoto
            Dim tblAssetIDRegDiv As DataTable = Dao.ToolSetting.GetAssetIDGenerationMethodData
            If tblAssetIDRegDiv.Rows.Count <> 0 Then
                ViewBag.AssetIDRegDivDef = Byte.Parse(tblAssetIDRegDiv.Rows(0).Item("DATA1"))
            End If
            'EPC領域サイズの初期値取得     'Add 2020.02.13 Iwamoto
            Dim tblTagSize As DataTable = Dao.ToolSetting.GetTagSizeData
            If tblTagSize.Rows.Count <> 0 Then
                ViewBag.EpcSizeDef = Integer.Parse(tblTagSize.Rows(0).Item("DATA1"))
            End If

            '資産ID登録方法
            GetAssetIdRegDivData()
            'EPC領域サイズ
            GetEpcSizeData()
            'EPC事業所キー   'Add 2020.02.10 Iwamoto
            If _ViewModel.ErrCode = String.Empty OrElse _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY Then
                GetEpcCompanyKeyData(ViewBag.AssetIDRegDivDef)
            Else
                GetEpcCompanyKeyData(_ViewModel.ASSET_ID_REG_DIV)
            End If

            '資産ID登録方法入力可不可設定 '2019/09/26 nv miyaji add
            ViewBag.AssetIdDivReg = _ViewModel.AssetIdDivReg

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.CATEGORY, COLSET_LOC.ALL)
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
                Session.Remove(CommonSessionKey.CREAGE_CATEGORY)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            'sbSQL.Append("SELECT ")
            'sbSQL.Append("  CAT_DIV ")
            'sbSQL.Append("  , CAT_L_CD ")
            'sbSQL.Append("  , CAT_M_CD ")
            'sbSQL.Append("  , CAT_S_CD ")
            'sbSQL.Append("  , CAT_NAME ")
            'sbSQL.Append("  , LEASE_LIMIT_ALERT_DIV ")
            'sbSQL.Append("  , LEASE_LIMIT_BEF_DAYS ")
            'sbSQL.Append("  , GENERIC_LIMIT_ALERT_DIV ")
            'sbSQL.Append("  , GENERIC_LIMIT_BEF_DAYS ")
            'sbSQL.Append("  , ASSET_ID_REG_DIV ")
            'sbSQL.Append("  , EPC_SIZE ")
            'sbSQL.Append("  , INS_DATETIME ")
            'sbSQL.Append("  , INS_EMPLOYEE_CD ")
            'sbSQL.Append("  , INS_EMPLOYEE_NAME ")
            'sbSQL.Append("  , UPD_DATETIME ")
            'sbSQL.Append("  , UPD_EMPLOYEE_CD ")
            'sbSQL.Append("  , UPD_EMPLOYEE_NAME ")
            'sbSQL.Append("  , UPD_TASK ")
            'sbSQL.Append("   ,COUNT(*) OVER () AS TotalCount ")
            'sbSQL.Append("FROM ")
            'sbSQL.Append("  M_CATEGORY ")
            'sbSQL.Append("WHERE ")
            'sbSQL.Append("  1 = 1 ")

            sbSQL.Append("SELECT ")
            sbSQL.Append("  CAT_BASE.CAT_DIV ")
            sbSQL.Append("  , CAT_BASE.CAT_L_CD ")
            sbSQL.Append("  , CAT_BASE.CAT_M_CD ")
            sbSQL.Append("  , CAT_BASE.CAT_S_CD ")
            sbSQL.Append("  , CAT_BASE.CAT_NAME ")
            sbSQL.Append("  , CAT_L.CAT_NAME AS CAT_L_NAME ")
            sbSQL.Append("  , CAT_M.CAT_NAME AS CAT_M_NAME ")
            sbSQL.Append("  , CAT_BASE.CAT_NAME AS CAT_S_NAME ")
            sbSQL.Append("  , CAT_BASE.LEASE_LIMIT_ALERT_DIV ")
            sbSQL.Append("  , CAT_BASE.LEASE_LIMIT_BEF_DAYS ")
            sbSQL.Append("  , CAT_BASE.GENERIC_LIMIT_ALERT_DIV ")
            sbSQL.Append("  , CAT_BASE.GENERIC_LIMIT_BEF_DAYS ")
            sbSQL.Append("  , CAT_BASE.LEASE_ALERT_DIV ")           'Add 2020.02.04 Iwamoto
            sbSQL.Append("  , CAT_BASE.LEASE_ALERT_CNT ")           'Add 2020.02.04 Iwamoto
            sbSQL.Append("  , CAT_BASE.LEASE_UPPER_LIMIT ")         'Add 2020.02.04 Iwamoto
            sbSQL.Append("  , CAT_BASE.LEASE_UPPER_ERR_DIV ")       'Add 2020.02.04 Iwamoto
            sbSQL.Append("  , CAT_BASE.ASSET_ID_REG_DIV ")
            sbSQL.Append("  , CAT_BASE.EPC_SIZE ")
            sbSQL.Append("  , CAT_BASE.EPC_COMPANY_KEY ")           'Add 2020.02.10 Iwamoto
            sbSQL.Append("  , STT.DATA2 AS EPC_COMPANY_KEY_NAME ")  'Add 2020.02.14 Iwamoto
            sbSQL.Append("  , CAT_BASE.EPC_ASSET_TYPE ")            'Add 2020.02.04 Iwamoto
            sbSQL.Append("  , CAT_BASE.INS_DATETIME ")
            sbSQL.Append("  , CAT_BASE.INS_EMPLOYEE_CD ")
            sbSQL.Append("  , CAT_BASE.INS_EMPLOYEE_NAME ")
            sbSQL.Append("  , CAT_BASE.UPD_DATETIME ")
            sbSQL.Append("  , CAT_BASE.UPD_EMPLOYEE_CD ")
            sbSQL.Append("  , CAT_BASE.UPD_EMPLOYEE_NAME ")
            sbSQL.Append("  , CAT_BASE.UPD_TASK ")
            sbSQL.Append("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_CATEGORY CAT_BASE ")
            sbSQL.Append("  LEFT JOIN M_CATEGORY CAT_L ")
            sbSQL.Append("  ON  CAT_L.CAT_L_CD = CAT_BASE.CAT_L_CD ")
            sbSQL.Append("  AND CAT_L.CAT_M_CD = ' ' ")
            sbSQL.Append("  AND CAT_L.CAT_S_CD = ' ' ")
            sbSQL.Append("  LEFT JOIN M_CATEGORY CAT_M ")
            sbSQL.Append("  ON  CAT_M.CAT_L_CD = CAT_BASE.CAT_L_CD ")
            sbSQL.Append("  AND CAT_M.CAT_M_CD = CAT_BASE.CAT_M_CD ")
            sbSQL.Append("  AND CAT_M.CAT_S_CD = ' ' ")
            'Add Start 2020.02.14 Iwamoto
            sbSQL.Append("  LEFT JOIN M_SETTING STT ")
            sbSQL.Append("  ON  STT.KEY_DIV = @KeyDiv ")
            sbSQL.Append("  AND STT.KEY1 = CAT_BASE.ASSET_ID_REG_DIV ")
            sbSQL.Append("  AND STT.KEY2 = CAT_BASE.EPC_COMPANY_KEY ")
            'Add End 2020.02.14 Iwamoto
            sbSQL.Append("WHERE ")
            sbSQL.Append("  1 = 1 ")

            addSqlParam("KeyDiv", $"{CommonSettingKey.AssetIDGenerationCoPrefix.KeyDiv}")

            '検索条件を設定する
            'カテゴリ大分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatL) Then
                sbSQL.Append("  AND CAT_BASE.CAT_L_CD = @SearchCatL")
                addSqlParam("SearchCatL", $"{_ViewModel.SearchCatL}")
            End If

            'カテゴリ中分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatM) Then
                sbSQL.Append("  AND CAT_BASE.CAT_M_CD = @SearchCatM")
                addSqlParam("SearchCatM", $"{_ViewModel.SearchCatM}")
            End If

            'カテゴリ小分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatS) Then
                sbSQL.Append("  AND CAT_BASE.CAT_S_CD = @SearchCatS")
                addSqlParam("SearchCatS", $"{_ViewModel.SearchCatS}")
            End If

            '表示指定
            If Not String.IsNullOrEmpty(_ViewModel.SearchDispSelect) Then
                Select Case _ViewModel.SearchDispSelect
                    Case DISP_SELECT.L
                        sbSQL.Append("  AND (CAT_BASE.CAT_DIV = @CatDivL)")
                        addSqlParam("CatDivL", $"{CAT_DIV.CategoryL}")

                    Case DISP_SELECT.M
                        sbSQL.Append("  AND (CAT_BASE.CAT_DIV = @CatDivM)")
                        addSqlParam("CatDivM", $"{CAT_DIV.CategoryM}")

                    Case DISP_SELECT.S
                        sbSQL.Append("  AND (CAT_BASE.CAT_DIV = @CatDivS)")
                        addSqlParam("CatDivS", $"{CAT_DIV.CategoryS}")

                    Case DISP_SELECT.LM
                        sbSQL.Append("  AND (CAT_BASE.CAT_DIV = @CatDivL OR CAT_BASE.CAT_DIV = @CatDivM)")
                        addSqlParam("CatDivL", $"{CAT_DIV.CategoryL}")
                        addSqlParam("CatDivM", $"{CAT_DIV.CategoryM}")
                End Select
            End If

            sbSQL.Append(" ORDER BY CAT_BASE.CAT_L_CD, CAT_BASE.CAT_M_CD, CAT_BASE.CAT_S_CD, CAT_BASE.CAT_DIV")

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of CategoryViewModel) = _db.CategoryView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function Create(ByVal ViewModel As CategoryViewModel, ByVal OnCreteButton As String) As ActionResult
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
            Select Case _ViewModel.CAT_DIV
                Case CAT_DIV.CategoryL
                    _ViewModel.CAT_M_CD = " "
                    _ViewModel.CAT_S_CD = " "

                Case CAT_DIV.CategoryM
                    _ViewModel.CAT_S_CD = " "
            End Select

            ''リース期限アラート区分が「する」以外の場合はNothingを設定する
            'If _ViewModel.LEASE_LIMIT_ALERT_DIV <> LeaseLimitAlertDiv.Yes Then
            '    _ViewModel.LEASE_LIMIT_BEF_DAYS = Nothing
            'End If

            ''○○期限アラート区分が「する」以外の場合はNothingを設定する
            'If _ViewModel.GENERIC_LIMIT_ALERT_DIV <> GenericLimitAlertDiv.Yes Then
            '    _ViewModel.GENERIC_LIMIT_BEF_DAYS = Nothing
            'End If

            '分類区分が大区分でない場合はNothingを設定する
            If _ViewModel.CAT_DIV <> CAT_DIV.CategoryL Then
                _ViewModel.ASSET_ID_REG_DIV = Nothing
                _ViewModel.EPC_SIZE = Nothing
            End If

            '分類区分が大区分かつ、資産ID登録方法がGIAI、GRAIの場合はEPC領域サイズを96に設定。
            If _ViewModel.CAT_DIV = CAT_DIV.CategoryL AndAlso
                (_ViewModel.ASSET_ID_REG_DIV = AssetIDGenerationMethod.GIAI OrElse _ViewModel.ASSET_ID_REG_DIV = AssetIDGenerationMethod.GRAI) Then
                _ViewModel.EPC_SIZE = TagSize.Size096
            End If

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理
                MyBase.BaseCreate(_ViewModel)

                '--2019/11/19 nv miyaji add start
                If _ViewModel.ErrCode <> String.Empty Then
                    If _ViewModel.CAT_DIV = CAT_DIV.CategoryM AndAlso _ViewModel.ErrTarget = "CAT_L_CD" Then
                        ' 分類区分が中区分の時、大区分エラーはコンボボックスをエラーターゲットとする。
                        _ViewModel.ErrTarget = "ModalCatL"
                    ElseIf _ViewModel.CAT_DIV = CAT_DIV.CategoryS Then
                        ' 分類区分が小区分の時、大区分・中区分エラーはコンボボックスをエラーターゲットとする。
                        If _ViewModel.ErrTarget = "CAT_L_CD" Then
                            _ViewModel.ErrTarget = "ModalCatL"
                        ElseIf _ViewModel.ErrTarget = "CAT_M_CD" Then
                            _ViewModel.ErrTarget = "ModalCatM"
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
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelCategory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_DIV) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_DIV).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_L_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_M_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_S_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelCategory.TITLE))

                    _ViewModel.States = StatusType.SuccessCreate
                Else
                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelCategory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_DIV) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_DIV).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_L_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_M_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_S_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelCategory.TITLE))

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
            CopyViewModeltoModel(_ViewModel)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeCreate()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_CATEGORY) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_CATEGORY) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            '必須チェック
            Select Case _Model.CAT_DIV
                Case CAT_DIV.CategoryL
                    'View側でチェック

                Case CAT_DIV.CategoryM
                    'カテゴリ中分類コード必須入力チェック
                    If _Model.CAT_M_CD Is Nothing Then
                        ModelState.AddModelError("CAT_M_CD", CommonLabel.MSG_ERR_NOT_ENTERD)
                        blnReturn = False
                    End If

                    'カテゴリ大分類コード存在チェック
                    If _Model.CAT_L_CD IsNot Nothing Then
                        If Dao.ToolCategory.IsExistCatLCd(_Model.CAT_L_CD) = False Then
                            ModelState.AddModelError("CAT_L_CD", CommonLabel.MSG_ERR_NOT_EXIST_CAT_L_CD)
                            blnReturn = False
                        End If
                    End If

                Case CAT_DIV.CategoryS
                    'カテゴリ中分類コード必須入力チェック
                    If _Model.CAT_M_CD Is Nothing Then
                        ModelState.AddModelError("CAT_M_CD", CommonLabel.MSG_ERR_NOT_ENTERD)
                        blnReturn = False
                    End If
                    'カテゴリ小分類コード必須入力チェック
                    If _Model.CAT_S_CD Is Nothing Then
                        ModelState.AddModelError("CAT_S_CD", CommonLabel.MSG_ERR_NOT_ENTERD)
                        blnReturn = False
                    End If

                    'カテゴリ大分類コード存在チェック
                    If _Model.CAT_L_CD IsNot Nothing Then
                        If Dao.ToolCategory.IsExistCatLCd(_Model.CAT_L_CD) = False Then
                            ModelState.AddModelError("CAT_L_CD", CommonLabel.MSG_ERR_NOT_EXIST_CAT_L_CD)
                            blnReturn = False
                        End If
                    End If
                    'カテゴリ中分類コード存在チェック
                    If _Model.CAT_M_CD IsNot Nothing Then
                        If Dao.ToolCategory.IsExistCatMCd(_Model.CAT_L_CD, _Model.CAT_M_CD) = False Then
                            ModelState.AddModelError("CAT_M_CD", CommonLabel.MSG_ERR_NOT_EXIST_CAT_M_CD)
                            blnReturn = False
                        End If
                    End If
            End Select

            'リース期限アラート区分が「する」場合のチェック
            If _Model.LEASE_LIMIT_ALERT_DIV = LeaseLimitAlertDiv.Yes Then
                '--日付入力必須
                If _Model.LEASE_LIMIT_BEF_DAYS.HasValue = False Then
                    ModelState.AddModelError("LEASE_LIMIT_BEF_DAYS", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If
                '--日付範囲
                If _Model.LEASE_LIMIT_BEF_DAYS.HasValue Then
                    If _Model.LEASE_LIMIT_BEF_DAYS.Value < MinValue.BefDays OrElse _Model.LEASE_LIMIT_BEF_DAYS.Value > MaxValue.BefDays Then
                        ModelState.AddModelError("LEASE_LIMIT_BEF_DAYS", CommonLabel.MSG_ERR_INPUT_RANGE)
                        blnReturn = False
                    End If
                End If
            End If

            '○○期限アラート区分が「する」場合のチェック
            If _Model.GENERIC_LIMIT_ALERT_DIV = GenericLimitAlertDiv.Yes Then
                '--日付入力必須
                If _Model.GENERIC_LIMIT_BEF_DAYS.HasValue = False Then
                    ModelState.AddModelError("GENERIC_LIMIT_BEF_DAYS", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If
                '--日付範囲
                If _Model.GENERIC_LIMIT_BEF_DAYS.HasValue Then
                    If _Model.GENERIC_LIMIT_BEF_DAYS.Value < MinValue.BefDays OrElse _Model.GENERIC_LIMIT_BEF_DAYS.Value > MaxValue.BefDays Then
                        ModelState.AddModelError("GENERIC_LIMIT_BEF_DAYS", CommonLabel.MSG_ERR_INPUT_RANGE)
                        blnReturn = False
                    End If
                End If
            End If

            '貸出アラート区分が「する」場合のチェック   'Add 2020.03.03 Iwamoto
            If _Model.LEASE_ALERT_DIV = LeaseAlertDiv.Yes Then
                '--貸出アラート回数
                '--範囲チェック
                If _Model.LEASE_ALERT_CNT.HasValue Then
                    If _Model.LEASE_ALERT_CNT.Value < MinValue.LeaseAlertCnt OrElse _Model.LEASE_ALERT_CNT.Value > MaxValue.LeaseAlertCnt Then
                        ModelState.AddModelError("LEASE_ALERT_CNT", CommonLabel.MSG_ERR_INPUT_RANGE)
                        blnReturn = False
                    End If
                End If
                '--貸出上限数
                '--範囲チェック
                If _Model.LEASE_UPPER_LIMIT.HasValue Then
                    If _Model.LEASE_UPPER_LIMIT.Value < MinValue.LeaseUpperLimit OrElse _Model.LEASE_UPPER_LIMIT.Value > MaxValue.LeaseUpperLimit Then
                        ModelState.AddModelError("LEASE_UPPER_LIMIT", CommonLabel.MSG_ERR_INPUT_RANGE)
                        blnReturn = False
                    End If
                End If
                '--貸出上限エラー区分が「しない」以外の場合にチェック
                If CommonRfid.ToStringNvl(_Model.LEASE_UPPER_ERR_DIV) <> LeaseUpperErrDiv.None Then
                    '----貸出上限数入力必須
                    If _Model.LEASE_UPPER_LIMIT.HasValue = False Then
                        ModelState.AddModelError("LEASE_UPPER_LIMIT", CommonLabel.MSG_ERR_NOT_ENTERD)
                        blnReturn = False
                    End If
                    '----貸出上限数大小チェック
                    If _Model.LEASE_ALERT_CNT.HasValue AndAlso _Model.LEASE_UPPER_LIMIT.HasValue Then
                        If _Model.LEASE_ALERT_CNT.Value > _Model.LEASE_UPPER_LIMIT.Value Then
                            ModelState.AddModelError("LEASE_UPPER_LIMIT", CommonLabelCategory.MSG_ERR_LEASE_UPPER_LIMIT_INPUT)
                            blnReturn = False
                        End If
                    End If
                End If
            End If

            '大区分選択時にチェック
            If _Model.CAT_DIV = CAT_DIV.CategoryL Then
                '--選択必須
                If _Model.ASSET_ID_REG_DIV.HasValue = False Then
                    ModelState.AddModelError("ASSET_ID_REG_DIV", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If
                '--選択必須
                If _Model.EPC_SIZE.HasValue = False Then
                    ModelState.AddModelError("EPC_SIZE", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If
            End If

            '資産ID登録方法が5:GIAIの場合にチェック
            If _Model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.GIAI Then
                '--選択必須
                If String.IsNullOrEmpty(_Model.EPC_COMPANY_KEY) Then
                    ModelState.AddModelError("EPC_COMPANY_KEY", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If
            End If

            '資産ID登録方法が6:GRAIの場合にチェック
            If _Model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.GRAI Then
                '--選択必須
                If String.IsNullOrEmpty(_Model.EPC_COMPANY_KEY) Then
                    ModelState.AddModelError("EPC_COMPANY_KEY", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If

                '2020/10/27 START 資産タイプの桁数チェックを追加
                'EPC事業所ID+資産タイプの総桁数は12でなければいけない(ISO規定)
                Dim epcAssetType As String = If(String.IsNullOrEmpty(_Model.EPC_ASSET_TYPE), "", _Model.EPC_ASSET_TYPE)
                Dim epcCompanyId As String = Dao.ToolSetting.GetEpcCompanyId(_Model.EPC_COMPANY_KEY)
                If epcAssetType.Length + epcCompanyId.Length <> 12 Then
                    ModelState.AddModelError("EPC_ASSET_TYPE", CommonLabel.MSG_ERR_ASSET_TYPE_DIGITS)
                    blnReturn = False
                End If
                '2020/10/27 END 資産タイプの桁数チェックを追加
            End If

            '登録済みかチェックする
            '重複エラーの場合、エラー情報を保持する
            Select Case _Model.CAT_DIV
                Case CAT_DIV.CategoryL
                    Dim model As CategoryModel = db.Category.Find(_Model.CAT_DIV, _Model.CAT_L_CD, " ", " ")
                    If model IsNot Nothing Then
                        ModelState.AddModelError("CAT_L_CD", CommonLabel.MSG_ERR_DATAJUUFUKU)
                        blnReturn = False
                    End If

                Case CAT_DIV.CategoryM
                    Dim model As CategoryModel = db.Category.Find(_Model.CAT_DIV, _Model.CAT_L_CD, _Model.CAT_M_CD, " ")
                    If model IsNot Nothing Then
                        ModelState.AddModelError("CAT_M_CD", CommonLabel.MSG_ERR_DATAJUUFUKU)
                        blnReturn = False
                    End If

                Case CAT_DIV.CategoryS
                    Dim model As CategoryModel = db.Category.Find(_Model.CAT_DIV, _Model.CAT_L_CD, _Model.CAT_M_CD, _Model.CAT_S_CD)
                    If model IsNot Nothing Then
                        ModelState.AddModelError("CAT_S_CD", CommonLabel.MSG_ERR_DATAJUUFUKU)
                        blnReturn = False
                    End If
            End Select

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelCategory.TITLE)

            _db.Category.Add(_Model)

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As CategoryModel = db.Category.Find(_Model.CAT_DIV, _Model.CAT_L_CD, _Model.CAT_M_CD, _Model.CAT_S_CD)

            'リース期限アラート区分が「する」場合のチェック
            If _Model.LEASE_LIMIT_ALERT_DIV = LeaseLimitAlertDiv.Yes Then
                '--日付入力必須
                If _Model.LEASE_LIMIT_BEF_DAYS.HasValue = False Then
                    ModelState.AddModelError("LEASE_LIMIT_BEF_DAYS", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If
                '--日付範囲
                If _Model.LEASE_LIMIT_BEF_DAYS.HasValue Then
                    If _Model.LEASE_LIMIT_BEF_DAYS.Value < MinValue.BefDays OrElse _Model.LEASE_LIMIT_BEF_DAYS.Value > MaxValue.BefDays Then
                        ModelState.AddModelError("LEASE_LIMIT_BEF_DAYS", CommonLabel.MSG_ERR_INPUT_RANGE)
                        blnReturn = False
                    End If
                End If
            End If

            '○○期限アラート区分が「する」場合のチェック
            If _Model.GENERIC_LIMIT_ALERT_DIV = GenericLimitAlertDiv.Yes Then
                '--日付入力必須
                If _Model.GENERIC_LIMIT_BEF_DAYS.HasValue = False Then
                    ModelState.AddModelError("GENERIC_LIMIT_BEF_DAYS", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If
                '--日付範囲
                If _Model.GENERIC_LIMIT_BEF_DAYS.HasValue Then
                    If _Model.GENERIC_LIMIT_BEF_DAYS.Value < MinValue.BefDays OrElse _Model.GENERIC_LIMIT_BEF_DAYS.Value > MaxValue.BefDays Then
                        ModelState.AddModelError("GENERIC_LIMIT_BEF_DAYS", CommonLabel.MSG_ERR_INPUT_RANGE)
                        blnReturn = False
                    End If
                End If
            End If

            '貸出アラート区分が「する」場合のチェック   'Add 2020.03.03 Iwamoto
            If _Model.LEASE_ALERT_DIV = LeaseAlertDiv.Yes Then
                '--貸出アラート回数
                '--範囲チェック
                If _Model.LEASE_ALERT_CNT.HasValue Then
                    If _Model.LEASE_ALERT_CNT.Value < MinValue.LeaseAlertCnt OrElse _Model.LEASE_ALERT_CNT.Value > MaxValue.LeaseAlertCnt Then
                        ModelState.AddModelError("LEASE_ALERT_CNT", CommonLabel.MSG_ERR_INPUT_RANGE)
                        blnReturn = False
                    End If
                End If
                '--貸出上限数
                '--範囲チェック
                If _Model.LEASE_UPPER_LIMIT.HasValue Then
                    If _Model.LEASE_UPPER_LIMIT.Value < MinValue.LeaseUpperLimit OrElse _Model.LEASE_UPPER_LIMIT.Value > MaxValue.LeaseUpperLimit Then
                        ModelState.AddModelError("LEASE_UPPER_LIMIT", CommonLabel.MSG_ERR_INPUT_RANGE)
                        blnReturn = False
                    End If
                End If
                '--貸出上限エラー区分が「しない」以外の場合にチェック
                If CommonRfid.ToStringNvl(_Model.LEASE_UPPER_ERR_DIV) <> LeaseUpperErrDiv.None Then
                    '----貸出上限数入力必須
                    If _Model.LEASE_UPPER_LIMIT.HasValue = False Then
                        ModelState.AddModelError("LEASE_UPPER_LIMIT", CommonLabel.MSG_ERR_NOT_ENTERD)
                        blnReturn = False
                    End If
                    '----貸出上限数大小チェック
                    If _Model.LEASE_ALERT_CNT.HasValue AndAlso _Model.LEASE_UPPER_LIMIT.HasValue Then
                        If _Model.LEASE_ALERT_CNT.Value > _Model.LEASE_UPPER_LIMIT.Value Then
                            ModelState.AddModelError("LEASE_UPPER_LIMIT", CommonLabelCategory.MSG_ERR_LEASE_UPPER_LIMIT_INPUT)
                            blnReturn = False
                        End If
                    End If
                End If
            End If

            '大区分選択時にチェック
            If _Model.CAT_DIV = CAT_DIV.CategoryL Then
                '--選択必須
                If _Model.ASSET_ID_REG_DIV.HasValue = False Then
                    ModelState.AddModelError("ASSET_ID_REG_DIV", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If
                '--選択必須
                If _Model.EPC_SIZE.HasValue = False Then
                    ModelState.AddModelError("EPC_SIZE", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If
            End If

            '資産ID登録方法がGIAIの場合にチェック
            If _Model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.GIAI Then
                '--選択必須
                If String.IsNullOrEmpty(_Model.EPC_COMPANY_KEY) Then
                    ModelState.AddModelError("EPC_COMPANY_KEY", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If
            End If

            '資産ID登録方法がGRAIの場合にチェック
            If _Model.ASSET_ID_REG_DIV = AssetIDGenerationMethod.GRAI Then
                '--選択必須
                If String.IsNullOrEmpty(_Model.EPC_COMPANY_KEY) Then
                    ModelState.AddModelError("EPC_COMPANY_KEY", CommonLabel.MSG_ERR_NOT_ENTERD)
                    blnReturn = False
                End If

                '2020/10/27 START 資産タイプの桁数チェックを追加
                'EPC事業所ID+資産タイプの総桁数は12でなければいけない(ISO規定)
                Dim epcAssetType As String = If(String.IsNullOrEmpty(_Model.EPC_ASSET_TYPE), "", _Model.EPC_ASSET_TYPE)
                Dim epcCompanyId As String = Dao.ToolSetting.GetEpcCompanyId(_Model.EPC_COMPANY_KEY)
                If epcAssetType.Length + epcCompanyId.Length <> 12 Then
                    ModelState.AddModelError("EPC_ASSET_TYPE", CommonLabel.MSG_ERR_ASSET_TYPE_DIGITS)
                    blnReturn = False
                End If
                '2020/10/27 END 資産タイプの桁数チェックを追加
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
        ''' 更新処理のコンテキストを変更します
        ''' </summary>
        Protected Overrides Sub EditQuery()

            '更新日時・更新社員コード・更新社員名・更新プロセスを登録
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelCategory.TITLE)

            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified

        End Sub

        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="ID">ID</param>
        ''' <returns>View表示内容</returns>
        Public Function GetEditData(ByVal ID As String) As JsonResult
            Dim db As New SystemDBContext

            'IDをキーにモデルを取得する
            Dim findKeys As String() = ID.Split(",")
            Dim model As CategoryModel = db.Category.Find(findKeys)
            model.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            model.CatLName = Dao.ToolCategory.GetCategoryName(CAT_DIV.CategoryL, model.CAT_L_CD, " ", " ")
            model.CatMName = Dao.ToolCategory.GetCategoryName(CAT_DIV.CategoryM, model.CAT_L_CD, model.CAT_M_CD, " ")
            model.CatSName = Dao.ToolCategory.GetCategoryName(CAT_DIV.CategoryS, model.CAT_L_CD, model.CAT_M_CD, model.CAT_S_CD)

            'EPC事業所キードロップダウンリスト作成   'Add 2020.02.13 Iwamoto
            GetModalEpcCompanyKeyData(model)

            '資産ID登録方法入力可不可設定    '2019/09/26 nv miyaji add
            '資産一覧で既に資産ID登録を行っているカテゴリーは変更できない
            If model.CAT_DIV = CAT_DIV.CategoryL Then
                If Dao.ToolCategory.IsUseCatAsset(model.CAT_L_CD, " ", " ") Then
                    model.AssetIdDivReg = False
                Else
                    model.AssetIdDivReg = True
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
        Public Overridable Function Delete(ByVal ViewModel As CategoryViewModel) As ActionResult
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
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelCategory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_DIV) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_DIV).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_L_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_M_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_S_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_CAT_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.CAT_NAME).Trim
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelCategory.TITLE))

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
            Session(CommonSessionKey.CREAGE_CATEGORY) = _ViewModel

        End Sub

        ''' <summary>
        ''' 削除処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function DeleteChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As CategoryModel = db.Category.Find(_Model.CAT_DIV, _Model.CAT_L_CD, _Model.CAT_M_CD, _Model.CAT_S_CD)

            '他の箇所で使用されている場合エラー
            If Dao.ToolCategory.IsUseCat(_Model.CAT_L_CD, _Model.CAT_M_CD, _Model.CAT_S_CD) Then
                ModelState.AddModelError("CAT_DIV", CommonErrorMsg.MSG_ERR_USED_CATEGORY)
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
            Dim Category As CategoryModel = _db.Category.Find(_Model.CAT_DIV, _Model.CAT_L_CD, _Model.CAT_M_CD, _Model.CAT_S_CD)

            '削除を実行する
            _db.Category.Remove(Category)

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
                sProcessContents = Me._Message.getMessage(CommonLabelCategory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelCategory.TITLE))
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
            Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_CATEGORY)
            Dim stArrayData As String() = sHeader.Split(","c)

            'CSVデータ取得 & 項目数チェック
            Dim isKomokusuuErr As Boolean = False
            Dim tbl As DataTable = clsSV_CsvFile.ReadCsvFileToDataTable(sCsvPath, Encoding.UTF8, CChar(","), True, "Category", stArrayData.Length, isKomokusuuErr)
            '項目数エラーの場合
            If isKomokusuuErr Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelCategory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " &
                                                     Path.GetFileName(CsvFile.FileName)
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelCategory.TITLE))

                Return Json(Me._lstErr)
            End If

            'エラーチェック
            If Not IsDataError(tbl, stArrayData) Then
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelCategory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " &
                                                     Path.GetFileName(CsvFile.FileName) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                                     Me._lstErr.Count.ToString
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelCategory.TITLE))

                Return Json(Me._lstErr)
            End If

            '--登録処理開始
            Dim db As New SystemDBContext
            Dim model As List(Of CategoryModel) = New List(Of CategoryModel)    '登録用モデル

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try

                    'カテゴリーマスタデータを全削除
                    db.Database.ExecuteSqlCommand("DELETE FROM M_CATEGORY")

                    '登録用モデルにデータを入れる
                    For icnt As Integer = 0 To tbl.Rows.Count - 1
                        Dim catLCd As String = tbl.Rows(icnt).Item(CsvCategory.ColIdx.CatLCd).ToString.Trim.TrimStart("'"c)
                        Dim catMCd As String = tbl.Rows(icnt).Item(CsvCategory.ColIdx.CatMCd).ToString.Trim.TrimStart("'"c)
                        Dim catSCd As String = tbl.Rows(icnt).Item(CsvCategory.ColIdx.CatSCd).ToString.Trim.TrimStart("'"c)

                        '分類区分を取得します
                        Dim catDiv As String = Dao.ToolCategory.GetCatDiv(catLCd, catMCd, catSCd)
                        '該当する分類区分がなかった場合は次行に遷移
                        If String.IsNullOrEmpty(catDiv) Then
                            Continue For
                        End If

                        model.Add(New CategoryModel())
                        '--分類区分
                        model(icnt).CAT_DIV = catDiv
                        '--分類コード
                        Select Case model(icnt).CAT_DIV
                            Case CAT_DIV.CategoryL
                                model(icnt).CAT_L_CD = catLCd
                                model(icnt).CAT_M_CD = " "
                                model(icnt).CAT_S_CD = " "
                                model(icnt).CAT_NAME = tbl.Rows(icnt).Item(CsvCategory.ColIdx.CatLName).ToString.Trim

                            Case CAT_DIV.CategoryM
                                model(icnt).CAT_L_CD = catLCd
                                model(icnt).CAT_M_CD = catMCd
                                model(icnt).CAT_S_CD = " "
                                model(icnt).CAT_NAME = tbl.Rows(icnt).Item(CsvCategory.ColIdx.CatMName).ToString.Trim

                            Case CAT_DIV.CategoryS
                                model(icnt).CAT_L_CD = catLCd
                                model(icnt).CAT_M_CD = catMCd
                                model(icnt).CAT_S_CD = catSCd
                                model(icnt).CAT_NAME = tbl.Rows(icnt).Item(CsvCategory.ColIdx.CatSName).ToString.Trim
                        End Select
                        '--リース期限アラート区分
                        model(icnt).LEASE_LIMIT_ALERT_DIV = tbl.Rows(icnt).Item(CsvCategory.ColIdx.LeaseLimitAlertDiv).ToString.Trim
                        '--リース期限何日前
                        Dim sLeaseLimitBefDays As String = tbl.Rows(icnt).Item(CsvCategory.ColIdx.LeaseLimitBefDays).ToString.Trim
                        If String.IsNullOrEmpty(sLeaseLimitBefDays) Then
                            model(icnt).LEASE_LIMIT_BEF_DAYS = Nothing
                        Else
                            model(icnt).LEASE_LIMIT_BEF_DAYS = Integer.Parse(sLeaseLimitBefDays)
                        End If
                        '--○○期限アラート区分
                        model(icnt).GENERIC_LIMIT_ALERT_DIV = tbl.Rows(icnt).Item(CsvCategory.ColIdx.GenericLimitAlertDiv).ToString.Trim
                        '--○○期限何日前
                        Dim sGenericLimitBefDays As String = tbl.Rows(icnt).Item(CsvCategory.ColIdx.GenericLimitBefDays).ToString.Trim
                        If String.IsNullOrEmpty(sGenericLimitBefDays) Then
                            model(icnt).GENERIC_LIMIT_BEF_DAYS = Nothing
                        Else
                            model(icnt).GENERIC_LIMIT_BEF_DAYS = Integer.Parse(sGenericLimitBefDays)
                        End If
                        '--貸出アラート区分     'Add 2020.02.13 Iwamoto
                        model(icnt).LEASE_ALERT_DIV = tbl.Rows(icnt).Item(CsvCategory.ColIdx.LeaseAlertDiv).ToString.Trim
                        '--貸出アラート回数     'Add 2020.02.13 Iwamoto
                        Dim sLeaseAlertCnt As String = tbl.Rows(icnt).Item(CsvCategory.ColIdx.LeaseAlertCnt).ToString.Trim
                        If String.IsNullOrEmpty(sLeaseAlertCnt) Then
                            model(icnt).LEASE_ALERT_CNT = Nothing
                        Else
                            model(icnt).LEASE_ALERT_CNT = Integer.Parse(sLeaseAlertCnt)
                        End If
                        '--貸出上限数            'Add 2020.02.13 Iwamoto
                        Dim sLeaseUpperLimit As String = tbl.Rows(icnt).Item(CsvCategory.ColIdx.LeaseUpperLimit).ToString.Trim
                        If String.IsNullOrEmpty(sLeaseUpperLimit) Then
                            model(icnt).LEASE_UPPER_LIMIT = Nothing
                        Else
                            model(icnt).LEASE_UPPER_LIMIT = Integer.Parse(sLeaseUpperLimit)
                        End If
                        '--貸出上限エラー区分    'Add 2020.02.13 Iwamoto
                        Dim sLeaseUpperErrDiv As String = tbl.Rows(icnt).Item(CsvCategory.ColIdx.LeaseUpperErrDiv).ToString.Trim
                        If String.IsNullOrEmpty(sLeaseUpperErrDiv) Then
                            model(icnt).LEASE_UPPER_ERR_DIV = Nothing
                        Else
                            model(icnt).LEASE_UPPER_ERR_DIV = sLeaseUpperErrDiv
                        End If
                        '--資産ID登録方法
                        Dim sAssetIDRegDiv As String = tbl.Rows(icnt).Item(CsvCategory.ColIdx.AssetIDRegDiv).ToString.Trim
                        If String.IsNullOrEmpty(sAssetIDRegDiv) Then
                            model(icnt).ASSET_ID_REG_DIV = Nothing
                        Else
                            model(icnt).ASSET_ID_REG_DIV = Byte.Parse(sAssetIDRegDiv)
                        End If
                        '--EPC領域サイズ
                        Dim sEPCSize As String = tbl.Rows(icnt).Item(CsvCategory.ColIdx.EPCSize).ToString.Trim
                        If String.IsNullOrEmpty(sEPCSize) Then
                            model(icnt).EPC_SIZE = Nothing
                        Else
                            model(icnt).EPC_SIZE = Integer.Parse(sEPCSize)
                        End If
                        '--EPC事業所キー         'Add 2020.02.13 Iwamoto
                        model(icnt).EPC_COMPANY_KEY = tbl.Rows(icnt).Item(CsvCategory.ColIdx.EPCCompanyKey).ToString.Trim
                        '--EPC資産タイプ         'Add 2020.02.13 Iwamoto
                        model(icnt).EPC_ASSET_TYPE = tbl.Rows(icnt).Item(CsvCategory.ColIdx.EPCAssetType).ToString.Trim
                        model(icnt).INS_DATETIME = CommonRfid.getNowDate
                        model(icnt).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(icnt).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(icnt).UPD_DATETIME = CommonRfid.getNowDate
                        model(icnt).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(icnt).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(icnt).UPD_TASK = Me._Message.getMessage(CommonLabelCategory.TITLE)
                    Next

                    'システムコンテキストにモデルを追加
                    db.Category.AddRange(model)

                    '登録処理を実行
                    db.SaveChanges()

                    tran.Commit()

                    '操作ログ出力
                    sProcessContents = Me._Message.getMessage(CommonLabelCategory.TITLE) & " " &
                                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                             Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                             tbl.Rows.Count.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelCategory.TITLE))

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

            Dim CategoryData As List(Of CategoryCsvModel)

            'ロケーションマスタデータ取得
            CategoryData = GetAllCategoryData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.Category.KeyDiv, CommonSettingKey.Category.Key1, CommonSettingKey.Category.Key2, CommonSettingKey.Category.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_CATEGORY)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_CATEGORY)
            Dim singleQuotation As String = "'"

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As CategoryCsvModel In CategoryData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CAT_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CAT_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CAT_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_LIMIT_ALERT_DIV)
                If row.LEASE_LIMIT_BEF_DAYS.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_LIMIT_BEF_DAYS)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.GENERIC_LIMIT_ALERT_DIV)
                If row.GENERIC_LIMIT_BEF_DAYS.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.GENERIC_LIMIT_BEF_DAYS)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                'Add Start 2020.02.13 Iwamoto
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_ALERT_DIV)
                If row.LEASE_ALERT_CNT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_ALERT_CNT)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_UPPER_ERR_DIV)
                If row.LEASE_UPPER_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_UPPER_LIMIT)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                'Add End 2020.02.13 Iwamoto
                If row.ASSET_ID_REG_DIV.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_ID_REG_DIV)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                If row.EPC_SIZE.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.EPC_SIZE)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.EPC_COMPANY_KEY)  'Add 2020.02.13 Iwamoto
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.EPC_ASSET_TYPE)   'Add 2020.02.13 Iwamoto

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelCategory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     CategoryData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelCategory.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetAllCategoryData() As List(Of CategoryCsvModel)
            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.Append("SELECT ")
            sbSQL.Append("  CAT_BASE.CAT_DIV ")
            sbSQL.Append("  , CAT_BASE.CAT_L_CD ")
            sbSQL.Append("  , CAT_BASE.CAT_M_CD ")
            sbSQL.Append("  , CAT_BASE.CAT_S_CD ")
            sbSQL.Append("  , CAT_L.CAT_NAME AS CAT_L_NAME ")
            sbSQL.Append("  , CAT_M.CAT_NAME AS CAT_M_NAME ")
            sbSQL.Append("  , CAT_BASE.CAT_NAME AS CAT_S_NAME ")
            sbSQL.Append("  , CAT_BASE.LEASE_LIMIT_ALERT_DIV ")
            sbSQL.Append("  , CAT_BASE.LEASE_LIMIT_BEF_DAYS ")
            sbSQL.Append("  , CAT_BASE.GENERIC_LIMIT_ALERT_DIV ")
            sbSQL.Append("  , CAT_BASE.GENERIC_LIMIT_BEF_DAYS ")
            sbSQL.Append("  , CAT_BASE.LEASE_ALERT_DIV ")           'Add 2020.02.13 Iwamoto
            sbSQL.Append("  , CAT_BASE.LEASE_ALERT_CNT ")           'Add 2020.02.13 Iwamoto
            sbSQL.Append("  , CAT_BASE.LEASE_UPPER_ERR_DIV ")       'Add 2020.02.13 Iwamoto
            sbSQL.Append("  , CAT_BASE.LEASE_UPPER_LIMIT ")         'Add 2020.02.13 Iwamoto
            sbSQL.Append("  , CAT_BASE.ASSET_ID_REG_DIV ")
            sbSQL.Append("  , CAT_BASE.EPC_SIZE ")
            sbSQL.Append("  , CAT_BASE.EPC_COMPANY_KEY ")           'Add 2020.02.13 Iwamoto
            sbSQL.Append("  , CAT_BASE.EPC_ASSET_TYPE ")            'Add 2020.02.13 Iwamoto
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_CATEGORY CAT_BASE ")
            sbSQL.Append("  LEFT JOIN M_CATEGORY CAT_L ")
            sbSQL.Append("  ON  CAT_L.CAT_L_CD = CAT_BASE.CAT_L_CD ")
            sbSQL.Append("  AND CAT_L.CAT_M_CD = ' ' ")
            sbSQL.Append("  AND CAT_L.CAT_S_CD = ' ' ")
            sbSQL.Append("  LEFT JOIN M_CATEGORY CAT_M ")
            sbSQL.Append("  ON  CAT_M.CAT_L_CD = CAT_BASE.CAT_L_CD ")
            sbSQL.Append("  AND CAT_M.CAT_M_CD = CAT_BASE.CAT_M_CD ")
            sbSQL.Append("  AND CAT_M.CAT_S_CD = ' ' ")
            sbSQL.Append(" ORDER BY CAT_BASE.CAT_L_CD, CAT_BASE.CAT_M_CD, CAT_BASE.CAT_S_CD, CAT_BASE.CAT_DIV")

            Dim lstCategory As List(Of CategoryCsvModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstCategory = db.CategoryCsv.SqlQuery(sbSQL.ToString).ToList

            For Each item As CategoryCsvModel In lstCategory
                Select Case item.CAT_DIV
                    Case CAT_DIV.CategoryL
                        item.CAT_M_NAME = String.Empty
                        item.CAT_S_NAME = String.Empty
                    Case CAT_DIV.CategoryM
                        item.CAT_S_NAME = String.Empty
                    Case CAT_DIV.CategoryS
                        '特に無し
                End Select
            Next

            'データを返却
            Return lstCategory
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

            '--マスタデータ取得
            Dim tblMst As DataTable = Dao.ToolCategory.GetCategoryDataAll
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

                Dim sCatLCd As String = tbl.Rows(iRowCnt).Item(CsvCategory.ColIdx.CatLCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.CatLCd)
                Dim sCatMCd As String = tbl.Rows(iRowCnt).Item(CsvCategory.ColIdx.CatMCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.CatMCd)
                Dim sCatSCd As String = tbl.Rows(iRowCnt).Item(CsvCategory.ColIdx.CatSCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.CatSCd)

                '分類区分を取得します
                Dim sCatDiv As String = Dao.ToolCategory.GetCatDiv(sCatLCd, sCatMCd, sCatSCd)
                '該当する分類区分がなかった場合はエラー
                If String.IsNullOrEmpty(sCatDiv) Then
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_DIV)
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERR_CAT_DIV)
                    blRet = False
                End If

                '重複チェック
                If lstSumiCd.IndexOf(sCatLCd & sCatMCd & sCatSCd) > -1 Then
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_KEY_ITEM)
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_KEY_ITEM) & "[{0}]", sCatLCd & sCatMCd & sCatSCd)
                    blRet = False
                Else
                    If Not String.IsNullOrEmpty(sCatDiv) Then
                        '読取データに既存データが存在するかどうかチェック
                        For Each rowMst As DataRow In tblMst.Rows
                            Dim sMstCatLCd As String = rowMst("CAT_L_CD").ToString.Trim.PadRight(itemlength.CatLCd)
                            Dim sMstCatMCd As String = rowMst("CAT_M_CD").ToString.Trim.PadRight(itemlength.CatMCd)
                            Dim sMstCatSCd As String = rowMst("CAT_S_CD").ToString.Trim.PadRight(itemlength.CatSCd)
                            If (sMstCatLCd & sMstCatMCd & sMstCatSCd) = (sCatLCd & sCatMCd & sCatSCd) Then
                                rowMst("IsExist") = True
                                Exit For
                            End If
                        Next

                        lstSumiCd.Add(sCatLCd & sCatMCd & sCatSCd)
                    End If
                End If

                'リース期限アラート区分
                Dim sLeaseLimitAlertDiv As String = String.Empty
                '○○期限アラート区分
                Dim sGenericLimitAlertDiv As String = String.Empty
                '貸出アラート区分
                Dim sLeaseAlertDiv As String = String.Empty
                '貸出アラート回数
                Dim sLeaseAlertCnt As String = String.Empty
                '貸出上限エラー区分
                Dim sLeaseUpperErrDiv As String = String.Empty

                For iCalCnt As Integer = 0 To stArrayData.Length - 1
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If

                    '列によってチェック条件・設定項目値を分ける
                    Dim sStr As String = tbl.Rows(iRowCnt).Item(iCalCnt).ToString.Trim.TrimStart("'"c)
                    Select Case iCalCnt
                        Case CsvCategory.ColIdx.CatLCd
                            'カテゴリ大分類コード
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.CatLCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.CatMCd
                            'カテゴリ中分類コード
                            If (sCatDiv = CAT_DIV.CategoryM OrElse sCatDiv = CAT_DIV.CategoryS) AndAlso String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.CatMCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.CatSCd
                            'カテゴリ小分類コード
                            If sCatDiv = CAT_DIV.CategoryS AndAlso String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.CatSCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.CatLName
                            'カテゴリ大分類名称
                            If sStr.Length > itemlength.CatNm Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_L_NAME)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.CatMName
                            'カテゴリ中分類名称
                            If sStr.Length > itemlength.CatNm Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_M_NAME)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.CatSName
                            'カテゴリ小分類名称
                            If sStr.Length > itemlength.CatNm Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_S_NAME)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.LeaseLimitAlertDiv
                            'リース期限アラート区分
                            sLeaseLimitAlertDiv = sStr
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.LeaseLimitAlertDiv Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso
                                Not (sStr = LeaseLimitAlertDiv.Front OrElse sStr = LeaseLimitAlertDiv.No OrElse sStr = LeaseLimitAlertDiv.Yes) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso sCatDiv = CAT_DIV.CategoryL AndAlso sStr = LeaseLimitAlertDiv.Front Then
                                '大分類のデータは「上位に準ずる」の設定不可
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERR_ALERT_DIV)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.LeaseLimitBefDays
                            'リース期限何日前
                            Dim sKoumoku As String = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT) & "?" & Me._Message.getMessage(CommonLabel.LABEL_BEF_DAYS)
                            If sLeaseLimitAlertDiv = LeaseLimitAlertDiv.Yes AndAlso String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = sKoumoku
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sLeaseLimitAlertDiv = LeaseLimitAlertDiv.Yes AndAlso sStr.Length > itemlength.LeaseLimitBefDays Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = sKoumoku
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf sLeaseLimitAlertDiv = LeaseLimitAlertDiv.Yes AndAlso Not CommonRfid.IsNumber(sStr) Then
                                '数値変換チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = sKoumoku
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf sLeaseLimitAlertDiv = LeaseLimitAlertDiv.Yes AndAlso Not String.IsNullOrEmpty(sStr) AndAlso
                                (sStr < MinValue.BefDays OrElse sStr > MaxValue.BefDays) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_INPUT_RANGE)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.GenericLimitAlertDiv
                            '○○期限アラート区分
                            sGenericLimitAlertDiv = sStr
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.GenericLimitAlertDiv Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso
                                Not (sStr = GenericLimitAlertDiv.Front OrElse sStr = GenericLimitAlertDiv.No OrElse sStr = GenericLimitAlertDiv.Yes) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso sCatDiv = CAT_DIV.CategoryL AndAlso sStr = GenericLimitAlertDiv.Front Then
                                '大分類のデータは「上位に準ずる」の設定不可
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERR_ALERT_DIV)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.GenericLimitBefDays
                            '○○期限何日前
                            Dim sKoumoku As String = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT) & "?" & Me._Message.getMessage(CommonLabel.LABEL_BEF_DAYS)
                            If sGenericLimitAlertDiv = GenericLimitAlertDiv.Yes AndAlso String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = sKoumoku
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sGenericLimitAlertDiv = GenericLimitAlertDiv.Yes AndAlso sStr.Length > itemlength.GenericLimitBefDays Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = sKoumoku
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf sGenericLimitAlertDiv = GenericLimitAlertDiv.Yes AndAlso Not CommonRfid.IsNumber(sStr) Then
                                '数値変換チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = sKoumoku
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf sGenericLimitAlertDiv = GenericLimitAlertDiv.Yes AndAlso Not String.IsNullOrEmpty(sStr) AndAlso
                                (sStr < MinValue.BefDays OrElse sStr > MaxValue.BefDays) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_GENERIC_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_INPUT_RANGE)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.LeaseAlertDiv
                            '貸出アラート区分       'Add 2020.02.13 Iwamoto
                            sLeaseAlertDiv = sStr
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.LeaseAlertDiv Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso
                                Not (sStr = LeaseAlertDiv.Front OrElse sStr = LeaseAlertDiv.No OrElse sStr = LeaseAlertDiv.Yes) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso sCatDiv = CAT_DIV.CategoryL AndAlso sStr = LeaseAlertDiv.Front Then
                                '大分類のデータは「上位に準ずる」の設定不可
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERR_ALERT_DIV)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.LeaseAlertCnt
                            '貸出アラート回数       'Add 2020.02.13 Iwamoto
                            sLeaseAlertCnt = sStr
                            If Not String.IsNullOrEmpty(sStr) AndAlso sStr.Length > itemlength.LeaseAlertCnt Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_ALERT_CNT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso Not CommonRfid.IsNumber(sStr) Then
                                '数値変換チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_ALERT_CNT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf sLeaseAlertDiv = LeaseAlertDiv.Yes AndAlso Not String.IsNullOrEmpty(sStr) AndAlso
                                (sStr < MinValue.LeaseAlertCnt OrElse sStr > MaxValue.LeaseAlertCnt) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_ALERT_CNT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_INPUT_RANGE)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.LeaseUpperErrDiv
                            '貸出上限エラー区分      'Add 2020.02.13 Iwamoto
                            sLeaseUpperErrDiv = sStr
                            If Not String.IsNullOrEmpty(sStr) AndAlso sStr.Length > itemlength.LeaseUpperErrDiv Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso
                                Not (sStr = LeaseUpperErrDiv.No OrElse sStr = LeaseUpperErrDiv.Yes) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT_ALERT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.LeaseUpperLimit
                            '貸出上限数              'Add 2020.02.13 Iwamoto
                            If Not String.IsNullOrEmpty(sStr) AndAlso sStr.Length > itemlength.LeaseUpperLimit Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not String.IsNullOrEmpty(sStr) AndAlso Not CommonRfid.IsNumber(sStr) Then
                                '数値変換チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf sLeaseAlertDiv = LeaseAlertDiv.Yes AndAlso sLeaseUpperErrDiv <> LeaseUpperErrDiv.None AndAlso
                                String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sLeaseAlertDiv = LeaseAlertDiv.Yes AndAlso sLeaseUpperErrDiv <> LeaseUpperErrDiv.None AndAlso
                                Not String.IsNullOrEmpty(sStr) AndAlso
                                (sStr < MinValue.LeaseUpperLimit OrElse sStr > MaxValue.LeaseUpperLimit) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_INPUT_RANGE)
                                blRet = False
                            ElseIf sLeaseAlertDiv = LeaseAlertDiv.Yes AndAlso sLeaseUpperErrDiv <> LeaseUpperErrDiv.None AndAlso
                                Not String.IsNullOrEmpty(sLeaseAlertCnt) AndAlso Not String.IsNullOrEmpty(sStr) Then
                                '貸出アラート回数
                                Dim dLeaseAlertCnt As Decimal = 0
                                If CommonRfid.IsNumber(sLeaseAlertCnt) Then
                                    dLeaseAlertCnt = Decimal.Parse(sLeaseAlertCnt)
                                End If
                                '貸出上限数
                                Dim dLeaseUpperLimit As Decimal = Decimal.Parse(sStr)
                                '大小チェック
                                If dLeaseAlertCnt > dLeaseUpperLimit Then
                                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_UPPER_LIMIT)
                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelCategory.MSG_ERR_LEASE_UPPER_LIMIT_INPUT)
                                    blRet = False
                                End If
                            End If
                        Case CsvCategory.ColIdx.AssetIDRegDiv
                            '資産ID登録方法
                            If sCatDiv = CAT_DIV.CategoryL AndAlso String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelCategory.LABEL_ASSET_ID_REG_DIV)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sCatDiv = CAT_DIV.CategoryL AndAlso Not CommonRfid.IsNumber(sStr) Then
                                '数値変換チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelCategory.LABEL_ASSET_ID_REG_DIV)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf sCatDiv = CAT_DIV.CategoryL AndAlso Not String.IsNullOrEmpty(sStr) AndAlso
                                Not (sStr = AssetIDGenerationMethod.None.ToString OrElse
                                sStr = AssetIDGenerationMethod.AssetNo.ToString OrElse
                                sStr = AssetIDGenerationMethod.Auto.ToString OrElse
                                sStr = AssetIDGenerationMethod.Inp.ToString OrElse
                                sStr = AssetIDGenerationMethod.GIAI.ToString OrElse
                                sStr = AssetIDGenerationMethod.GRAI.ToString) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelCategory.LABEL_ASSET_ID_REG_DIV)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf (sCatDiv = CAT_DIV.CategoryM OrElse sCatDiv = CAT_DIV.CategoryS) AndAlso Not String.IsNullOrEmpty(sStr) Then
                                '値チェック
                                '--中分類、小分類のデータに値が入力されている場合はエラー
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelCategory.LABEL_ASSET_ID_REG_DIV)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelCategory.MSG_ERR_ASSET_ID_REG_DIV_INPUT)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.EPCSize
                            'EPC領域サイズ
                            If sCatDiv = CAT_DIV.CategoryL AndAlso String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelCategory.LABEL_EPC_SIZE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sCatDiv = CAT_DIV.CategoryL AndAlso Not CommonRfid.IsNumber(sStr) Then
                                '数値変換チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelCategory.LABEL_EPC_SIZE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf sCatDiv = CAT_DIV.CategoryL AndAlso Not String.IsNullOrEmpty(sStr) AndAlso
                                Not (Integer.Parse(sStr) = TagSize.Size064 OrElse
                                Integer.Parse(sStr) = TagSize.Size096 OrElse
                                Integer.Parse(sStr) = TagSize.Size128 OrElse
                                Integer.Parse(sStr) = TagSize.Size256 OrElse
                                Integer.Parse(sStr) = TagSize.Size512) Then
                                '値チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelCategory.LABEL_EPC_SIZE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf (sCatDiv = CAT_DIV.CategoryM OrElse sCatDiv = CAT_DIV.CategoryS) AndAlso Not String.IsNullOrEmpty(sStr) Then
                                '値チェック
                                '--中分類、小分類のデータに値が入力されている場合はエラー
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelCategory.LABEL_EPC_SIZE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelCategory.MSG_ERR_EPC_SIZE_INPUT)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.EPCCompanyKey
                            'EPC事業所キー           'Add 2020.02.13 Iwamoto
                            If Not String.IsNullOrEmpty(sStr) AndAlso sStr.Length > itemlength.EPCCompanyKey Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EPC_COMPANY)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf (sCatDiv = CAT_DIV.CategoryM OrElse sCatDiv = CAT_DIV.CategoryS) AndAlso Not String.IsNullOrEmpty(sStr) Then
                                '値チェック
                                '--中分類、小分類のデータに値が入力されている場合はエラー
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EPC_COMPANY)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelCategory.MSG_ERR_EPC_COMPANY_KEY_INPUT)
                                blRet = False
                            End If
                        Case CsvCategory.ColIdx.EPCAssetType
                            'EPC資産タイプ           'Add 2020.02.13 Iwamoto
                            If Not String.IsNullOrEmpty(sStr) AndAlso sStr.Length > itemlength.EPCAssetType Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EPC_ASSET_TYPE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf (sCatDiv = CAT_DIV.CategoryM OrElse sCatDiv = CAT_DIV.CategoryS) AndAlso Not String.IsNullOrEmpty(sStr) Then
                                '値チェック
                                '--中分類、小分類のデータに値が入力されている場合はエラー
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_EPC_ASSET_TYPE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelCategory.MSG_ERR_EPC_ASSET_TYPE_INPUT)
                                blRet = False
                            End If
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

                Dim sCatLCd As String = tbl.Rows(iRowCnt).Item(CsvCategory.ColIdx.CatLCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.CatLCd)
                Dim sCatMCd As String = tbl.Rows(iRowCnt).Item(CsvCategory.ColIdx.CatMCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.CatMCd)
                Dim sCatSCd As String = tbl.Rows(iRowCnt).Item(CsvCategory.ColIdx.CatSCd).ToString.Trim.TrimStart("'"c).PadRight(itemlength.CatSCd)

                '分類区分を取得します
                Dim sCatDiv As String = Dao.ToolCategory.GetCatDiv(sCatLCd, sCatMCd, sCatSCd)
                '該当する分類区分がなかった場合は次行に遷移
                If String.IsNullOrEmpty(sCatDiv) Then
                    Continue For
                End If

                Select Case sCatDiv
                    Case CAT_DIV.CategoryM
                        If lstSumiCd.IndexOf(sCatLCd & Space(itemlength.CatMCd) & Space(itemlength.CatSCd)) = -1 Then
                            '大分類存在チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_CAT_L_CD) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD) & "[{0}]", sCatLCd)
                            blRet = False
                        End If

                    Case CAT_DIV.CategoryS
                        Dim hasError As Boolean = False
                        If lstSumiCd.IndexOf(sCatLCd & Space(itemlength.CatMCd) & Space(itemlength.CatSCd)) = -1 Then
                            '大分類存在チェック
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_CAT_L_CD) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD) & "[{0}]", sCatLCd)
                            blRet = False
                            hasError = True
                        End If
                        If lstSumiCd.IndexOf(sCatLCd & sCatMCd & Space(itemlength.CatSCd)) = -1 Then
                            '中分類存在チェック
                            If hasError Then
                                Dim lstRow As DataInportResult = New DataInportResult
                                Me._lstErr.Add(lstRow)
                            End If
                            Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                            Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD)
                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_CAT_M_CD) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD) & "[{0}]", sCatMCd)
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
                    Dim sMstCatLCd As String = rowMst("CAT_L_CD").ToString
                    Dim sMstCatMCd As String = rowMst("CAT_M_CD").ToString
                    Dim sMstCatSCd As String = rowMst("CAT_S_CD").ToString
                    If Dao.ToolCategory.IsUseCat(sMstCatLCd, sMstCatMCd, sMstCatSCd, True) Then             ' 2022/03/08 戸祭 DBチェックをSKIP
                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = Nothing
                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CATEGORY)
                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_NOT_EXIST_USED_DATA) &
                            String.Format(Me._Message.getMessage(CommonLabel.LABEL_CAT_L_CD) & "[{0}]", sMstCatLCd.Trim) & " " &
                            String.Format(Me._Message.getMessage(CommonLabel.LABEL_CAT_M_CD) & "[{0}]", sMstCatMCd.Trim) & " " &
                            String.Format(Me._Message.getMessage(CommonLabel.LABEL_CAT_S_CD) & "[{0}]", sMstCatSCd.Trim) & " " &
                            String.Format(Me._Message.getMessage(CommonLabel.LABEL_CAT_NAME) & "[{0}]", rowMst("CAT_NAME").ToString)
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

#Region "資産ID登録方法ドロップダウン変更処理"
        ''' <summary>
        ''' 資産ID登録方法ドロップダウン変更処理
        ''' </summary>
        ''' <param name="selectAssetIdRegDiv">資産ID登録方法</param>
        ''' <returns></returns>
        Public Function ChangeAssetIdRegDiv(selectAssetIdRegDiv As Byte?) As ActionResult
            Return Json(GetEpcCompanyKeyData(selectAssetIdRegDiv))

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

            'モーダル画面のカテゴリーコンボ設定 '2019/11/19 nv miyaji add
            Dim SelectedCd As String = _ViewModel.CAT_L_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelCatL = New SelectList(lst, "CAT_CD", "CAT_NAME", "")
            Else
                ViewBag.modalmodelCatL = New SelectList(lst, "CAT_CD", "CAT_NAME", SelectedCd)
            End If

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

            'モーダル画面のカテゴリーコンボ設定  '2019/11/19 nv miyaji add
            Dim SelectedCd As String = _ViewModel.CAT_M_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelCatM = New SelectList(lst, "CAT_CD", "CAT_NAME", "")
            Else
                ViewBag.modalmodelCatM = New SelectList(lst, "CAT_CD", "CAT_NAME", SelectedCd)
            End If

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

#Region "資産ID登録方法ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 資産ID登録方法ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetAssetIdRegDivData() As JsonResult
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            'Upd Start 2020.02.06 Iwamoto
            'Dim lst As New List(Of AssetIdRegDivDropdownModel)
            'lst.Add(New AssetIdRegDivDropdownModel With {.Cd = AssetIDGenerationMethod.None, .Name = Me._Message.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_NONE)})
            'lst.Add(New AssetIdRegDivDropdownModel With {.Cd = AssetIDGenerationMethod.AssetNo, .Name = Me._Message.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_ASSET_NO)})
            'lst.Add(New AssetIdRegDivDropdownModel With {.Cd = AssetIDGenerationMethod.Auto, .Name = Me._Message.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_AUTO)})
            'lst.Add(New AssetIdRegDivDropdownModel With {.Cd = AssetIDGenerationMethod.Inp, .Name = Me._Message.getMessage(CommonLabel.VALUE_ASSET_ID_REG_DIV_INP)})
            'データ取得
            Dim tblDropDown As DataTable = Dao.ToolSetting.GetAssetIDGenerationDropDownData
            'リスト作成
            Dim lst As New List(Of AssetIdRegDivDropdownModel)
            For Each row As DataRow In tblDropDown.Rows
                lst.Add(New AssetIdRegDivDropdownModel With {.Cd = Integer.Parse(row("Cd")), .Name = row("Name").ToString})
            Next
            'Upd End 2020.02.06 Iwamoto

            '初期値設定
            Dim assetIDRegDiv As Byte? = Nothing
            'Upd Start 2020.02.13 Iwamoto
            'If String.IsNullOrEmpty(_ViewModel.ErrCode) = False Then
            '    assetIDRegDiv = _ViewModel.ASSET_ID_REG_DIV
            'ElseIf _ViewModel.ASSET_ID_REG_DIV.HasValue Then
            '    assetIDRegDiv = _ViewModel.ASSET_ID_REG_DIV
            'Else
            '    Dim tbl As DataTable = Dao.ToolSetting.GetAssetIDGenerationMethodData
            '    If tbl.Rows.Count <> 0 Then
            '        assetIDRegDiv = Byte.Parse(tbl.Rows(0).Item("DATA1"))
            '    End If
            'End If
            If String.IsNullOrEmpty(_ViewModel.ErrCode) = False Then
                assetIDRegDiv = _ViewModel.ASSET_ID_REG_DIV
            ElseIf _ViewModel.ASSET_ID_REG_DIV.HasValue Then
                assetIDRegDiv = _ViewModel.ASSET_ID_REG_DIV
            End If
            'Upd End 2020.02.13 Iwamoto

            ViewBag.AssetIdRegDiv = New SelectList(lst, "Cd", "Name", assetIDRegDiv)
            _ViewModel.assetIdRegDivDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "EPC領域サイズドロップダウンリストデータ取得"
        ''' <summary>
        ''' EPC領域サイズドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetEpcSizeData() As JsonResult
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim lst As New List(Of EpcSizeDropdownModel)
            lst.Add(New EpcSizeDropdownModel With {.Cd = TagSize.Size064, .Name = Me._Message.getMessage(CommonLabel.VALUE_EPC_SIZE_064)})
            lst.Add(New EpcSizeDropdownModel With {.Cd = TagSize.Size096, .Name = Me._Message.getMessage(CommonLabel.VALUE_EPC_SIZE_096)})
            lst.Add(New EpcSizeDropdownModel With {.Cd = TagSize.Size128, .Name = Me._Message.getMessage(CommonLabel.VALUE_EPC_SIZE_128)})
            lst.Add(New EpcSizeDropdownModel With {.Cd = TagSize.Size256, .Name = Me._Message.getMessage(CommonLabel.VALUE_EPC_SIZE_256)})
            lst.Add(New EpcSizeDropdownModel With {.Cd = TagSize.Size512, .Name = Me._Message.getMessage(CommonLabel.VALUE_EPC_SIZE_512)})

            '初期値設定
            Dim epcSize As Integer? = Nothing
            'Upd Start 2020.02.13 Iwamoto
            'If String.IsNullOrEmpty(_ViewModel.ErrCode) = False Then
            '    epcSize = _ViewModel.EPC_SIZE
            'ElseIf _ViewModel.EPC_SIZE.HasValue Then
            '    epcSize = _ViewModel.EPC_SIZE
            'Else
            '    Dim tbl As DataTable = Dao.ToolSetting.GetTagSizeData
            '    If tbl.Rows.Count <> 0 Then
            '        epcSize = Integer.Parse(tbl.Rows(0).Item("DATA1"))
            '    End If
            'End If
            If String.IsNullOrEmpty(_ViewModel.ErrCode) = False Then
                epcSize = _ViewModel.EPC_SIZE
            ElseIf _ViewModel.EPC_SIZE.HasValue Then
                epcSize = _ViewModel.EPC_SIZE
            End If
            'Upd End 2020.02.13 Iwamoto

            ViewBag.EpcSize = New SelectList(lst, "Cd", "Name", epcSize)
            _ViewModel.epcSizeDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "EPC事業所キードロップダウンリストデータ取得"
        ''' <summary>
        ''' EPC事業所キードロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetEpcCompanyKeyData(selectAssetIdRegDiv As Byte?) As JsonResult
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            'データ取得
            Dim tblDropDown As DataTable = Dao.ToolSetting.GetEpcCompanyKeyDropDownData(selectAssetIdRegDiv)
            'リスト作成
            Dim lst As New List(Of EpcCompanyKeyDropdownModel)
            For Each row As DataRow In tblDropDown.Rows
                lst.Add(New EpcCompanyKeyDropdownModel With {.Cd = Integer.Parse(row("Cd")), .Name = row("Name").ToString})
            Next

            '初期値設定
            Dim value As String = String.Empty
            If String.IsNullOrEmpty(_ViewModel.ErrCode) = False Then
                value = _ViewModel.EPC_COMPANY_KEY
            ElseIf Not String.IsNullOrEmpty(_ViewModel.EPC_COMPANY_KEY) Then
                value = _ViewModel.EPC_COMPANY_KEY
            End If

            ViewBag.EpcCompanyKey = New SelectList(lst, "Cd", "Name", value)
            _ViewModel.epcCompanyKeyDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "編集ボタンクリック時、モーダル表示のEPC事業所キードロップダウンリストデータを取得する"
        ''' <summary>
        ''' 編集ボタンクリック時、モーダル表示のEPC事業所キードロップダウンリストデータを取得する
        ''' </summary>
        Private Sub GetModalEpcCompanyKeyData(ByRef model As CategoryModel)
            'データ取得
            Dim tblDropDown As DataTable = Dao.ToolSetting.GetEpcCompanyKeyDropDownData(model.ASSET_ID_REG_DIV)
            'リスト作成
            Dim lst As New List(Of EpcCompanyKeyDropdownModel)
            For Each row As DataRow In tblDropDown.Rows
                lst.Add(New EpcCompanyKeyDropdownModel With {.Cd = Integer.Parse(row("Cd")), .Name = row("Name").ToString})
            Next

            ViewBag.EpcCompanyKey = New SelectList(lst, "Cd", "Name", model.EPC_COMPANY_KEY)
            model.epcCompanyKeyDropdownData = lst
        End Sub
#End Region

#Region "分類名取得処理"
        ''' <summary>
        ''' 分類名取得処理
        ''' </summary>
        ''' <param name="inputLCode">カテゴリ大分類コード</param>
        ''' <param name="inputMCode">カテゴリ中分類コード</param>
        ''' <param name="inputSCode">カテゴリ小分類コード</param>
        ''' <returns></returns>
        Public Function GetCatName(locDiv As String, inputLCode As String, inputMCode As String, inputSCode As String) As JsonResult
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
            'M_CATEGORYテーブルから取得するSQLを作成
            sbSQL.Append("SELECT ")
            sbSQL.Append("  CAT_BASE.CAT_DIV ")
            sbSQL.Append("  , CAT_BASE.CAT_L_CD ")
            sbSQL.Append("  , CAT_BASE.CAT_M_CD ")
            sbSQL.Append("  , CAT_BASE.CAT_S_CD ")
            sbSQL.Append("  , CAT_L.CAT_NAME AS CAT_L_NAME ")
            sbSQL.Append("  , CAT_M.CAT_NAME AS CAT_M_NAME ")
            sbSQL.Append("  , CAT_BASE.CAT_NAME AS CAT_S_NAME ")
            sbSQL.Append("  , CAT_BASE.LEASE_LIMIT_ALERT_DIV ")
            sbSQL.Append("  , CAT_BASE.LEASE_LIMIT_BEF_DAYS ")
            sbSQL.Append("  , CAT_BASE.GENERIC_LIMIT_ALERT_DIV ")
            sbSQL.Append("  , CAT_BASE.GENERIC_LIMIT_BEF_DAYS ")

            sbSQL.Append("  , CAT_BASE.LEASE_ALERT_DIV ")
            sbSQL.Append("  , CAT_BASE.LEASE_ALERT_CNT ")
            sbSQL.Append("  , CAT_BASE.LEASE_UPPER_LIMIT ")
            sbSQL.Append("  , CAT_BASE.LEASE_UPPER_ERR_DIV ")

            sbSQL.Append("  , CAT_BASE.ASSET_ID_REG_DIV ")
            sbSQL.Append("  , CAT_BASE.EPC_SIZE ")

            sbSQL.Append("  , CAT_BASE.EPC_COMPANY_KEY ")
            sbSQL.Append("  , CAT_BASE.EPC_ASSET_TYPE ")


            sbSQL.Append("FROM ")
            sbSQL.Append("  M_CATEGORY CAT_BASE ")
            sbSQL.Append("  LEFT JOIN M_CATEGORY CAT_L ")
            sbSQL.Append("  ON  CAT_L.CAT_L_CD = CAT_BASE.CAT_L_CD ")
            sbSQL.Append("  AND CAT_L.CAT_M_CD = ' ' ")
            sbSQL.Append("  AND CAT_L.CAT_S_CD = ' ' ")
            sbSQL.Append("  LEFT JOIN M_CATEGORY CAT_M ")
            sbSQL.Append("  ON  CAT_M.CAT_L_CD = CAT_BASE.CAT_L_CD ")
            sbSQL.Append("  AND CAT_M.CAT_M_CD = CAT_BASE.CAT_M_CD ")
            sbSQL.Append("  AND CAT_M.CAT_S_CD = ' ' ")
            sbSQL.Append("WHERE ")
            sbSQL.Append("  CAT_BASE.CAT_L_CD = '" & inputLCode & "'")
            sbSQL.Append("  AND CAT_BASE.CAT_M_CD = '" & inputMCode & "'")
            sbSQL.Append("  AND CAT_BASE.CAT_S_CD = '" & inputSCode & "'")

            'SQLを実行する
            Dim lst As New List(Of CategoryCsvModel)
            lst = db.CategoryCsv.SqlQuery(sbSQL.ToString).ToList()

            For Each item As CategoryCsvModel In lst
                Select Case item.CAT_DIV
                    Case CAT_DIV.CategoryL
                        item.CAT_M_NAME = String.Empty
                        item.CAT_S_NAME = String.Empty
                    Case CAT_DIV.CategoryM
                        item.CAT_S_NAME = String.Empty
                    Case CAT_DIV.CategoryS
                        '特に無し
                End Select
            Next

            Return Json(lst)
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
                _viewResult.Add(New CategoryViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchCatL = _ViewModel.SearchCatL
            _viewResult.ElementAt(0).SearchCatM = _ViewModel.SearchCatM
            _viewResult.ElementAt(0).SearchCatS = _ViewModel.SearchCatS
            If String.IsNullOrEmpty(_ViewModel.SearchDispSelect) Then
                _viewResult.ElementAt(0).SearchDispSelect = DISP_SELECT.LMS
            Else
                _viewResult.ElementAt(0).SearchDispSelect = _ViewModel.SearchDispSelect
            End If
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.CATEGORY, COLSET_LOC.ALL)
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.CATEGORY, COLSET_LOC.ALL).FIX_COL_CNT
        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As CategoryViewModel)
            _Model.CAT_DIV = model.CAT_DIV
            _Model.CAT_L_CD = model.CAT_L_CD
            _Model.CAT_M_CD = model.CAT_M_CD
            _Model.CAT_S_CD = model.CAT_S_CD
            _Model.CAT_NAME = model.CAT_NAME
            _Model.LEASE_LIMIT_ALERT_DIV = model.LEASE_LIMIT_ALERT_DIV
            _Model.LEASE_LIMIT_BEF_DAYS = model.LEASE_LIMIT_BEF_DAYS
            _Model.GENERIC_LIMIT_ALERT_DIV = model.GENERIC_LIMIT_ALERT_DIV
            _Model.GENERIC_LIMIT_BEF_DAYS = model.GENERIC_LIMIT_BEF_DAYS
            'Add Start 2020.02.04 Iwamoto
            _Model.LEASE_ALERT_DIV = model.LEASE_ALERT_DIV
            _Model.LEASE_ALERT_CNT = model.LEASE_ALERT_CNT
            _Model.LEASE_UPPER_LIMIT = model.LEASE_UPPER_LIMIT
            _Model.LEASE_UPPER_ERR_DIV = model.LEASE_UPPER_ERR_DIV
            'Add End 2020.02.04 Iwamoto
            _Model.ASSET_ID_REG_DIV = model.ASSET_ID_REG_DIV
            _Model.EPC_SIZE = model.EPC_SIZE
            _Model.EPC_COMPANY_KEY = model.EPC_COMPANY_KEY  'Add 2020.02.10 Iwamoto
            _Model.EPC_ASSET_TYPE = model.EPC_ASSET_TYPE    'Add 2020.02.04 Iwamoto
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