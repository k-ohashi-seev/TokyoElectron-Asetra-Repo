Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports System.Web.WebPages
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' 工事資産一覧画面表示用のクラス
    ''' </summary>
    Public Class ConstAssetListController
        Inherits PageBaseController

#Region "クラス定数"

#End Region

#Region "クラス変数"
        Private _Model As ConstAssetListModel = New ConstAssetListModel                                 'ConstAssetListモデル
        Private _ViewModel As ConstAssetListViewModel = New ConstAssetListViewModel                     'ConstAssetList一覧表示用モデル
        Protected _viewResult As List(Of ConstAssetListViewModel) = New List(Of ConstAssetListViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As ConstAssetListViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(RedirectConstructionListInfo.ID) IsNot Nothing Then
                Session.Remove(RedirectConstructionListInfo.ID)
                'チェックボックス状態の選択状態をクリアする
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
            End If

            If Session(CommonSessionKey.SEARCH_FLG) IsNot Nothing Then
                Session.Remove(CommonSessionKey.SEARCH_FLG)
            End If

            'リダイレクト情報作成
            Dim info As New RedirectConstructionListInfo
            info.RedirectPage = RedirectConstructionListInfo.RedirectPageType.Menu
            info.KeyValue = String.Empty

            Session(RedirectConstructionListInfo.ID) = info
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
        Public Function Index(ByVal ViewModel As ConstAssetListViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_CONST_ASSET_LIST)
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            ViewBag.DrugsFunc = False

            '検索ボタンがクリックされた場合、選択状態のセッションをクリアする
            If Me._Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                'セッションSEARCH_FLGを設定
                Session(CommonSessionKey.SEARCH_FLG) = True
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As ConstAssetListViewModel = Session(CommonSessionKey.CREAGE_CONST_ASSET_LIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            Dim info As RedirectConstructionListInfo = Session(RedirectConstructionListInfo.ID)
            If info IsNot Nothing Then
                ViewBag.ConstructionListKey = info.KeyValue
            End If

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '初期表示
                '項目配置情報取得
                Dim dictGridInfo = ToolColSet.GetGridInfo(_db, DispId.CONST_ASSET_LIST_TOP)

                '検索条件リストデータ取得
                Dim searchItemList = GetSearchItem()

                'セッションSEARCH_FLGがNothingの場合
                If Session(CommonSessionKey.SEARCH_FLG) Is Nothing Then
                    '検索条件の初期値設定
                    SetInitCondition(searchItemList, dictGridInfo)
                End If

                '表示する
                MyBase.BaseIndex(_ViewModel)

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            '工事状態ドロップダウンリストデータ取得
            GetConstStatusData()
            '管理ロケーションドロップダウンリスト作成
            GetMngLocationLData()
            '工事資産状態ドロップダウンリスト作成
            GetConstAssetStatusData()
            '仮置有無ドロップダウンリスト作成
            GetTempYesOrNoData()

            'ロケーション
            ' 大区分ドロップダウンリスト作成
            GetLocationLData()
            ' 中区分
            GetLocationMData(_ViewModel.SearchLocL)
            ' 小区分
            GetLocationSData(_ViewModel.SearchLocL, _ViewModel.SearchLocM)

            'カテゴリー
            ' 大分類ドロップダウンリスト作成
            GetCategoryLData()
            ' 中分類
            GetCategoryMData(_ViewModel.SearchCatL)
            ' 小分類
            GetCategorySData(_ViewModel.SearchCatL, _ViewModel.SearchCatM)

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.CONST_ASSET_LIST_BOTTOM)
            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If

            '工事データ取得
            GetTblConstructionData()

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
                Session.Remove(CommonSessionKey.CREAGE_CONST_ASSET_LIST)
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
            Dim query As DbSqlQuery(Of ConstAssetListViewModel) = _db.ConstAssetListView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function Create(ByVal ViewModel As ConstAssetListViewModel, ByVal OnCreteButton As String) As ActionResult
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

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理
                MyBase.BaseCreate(_ViewModel)
            Else
                '更新処理
                MyBase.BaseEdit(_ViewModel)
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
            Session(CommonSessionKey.CREAGE_CONST_ASSET_LIST) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_CONST_ASSET_LIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelConstAssetList.TITLE)

            _db.ConstAssetList.Add(_Model)

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelConstAssetList.TITLE)

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
            Dim model As ConstAssetListModel = db.ConstAssetList.Find(ID)

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
        Public Overridable Function Delete(ByVal ViewModel As ConstAssetListViewModel) As ActionResult
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
            Session(CommonSessionKey.CREAGE_CONST_ASSET_LIST) = _ViewModel

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
        End Sub
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function IchiranCsvExport(ViewModel As ConstAssetListViewModel) As ActionResult
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

            Dim ConstData As List(Of ConstAssetListViewModel)

            '工事マスタデータ取得
            ConstData = GetConstAssetListData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.ConstAssetList.KeyDiv, CommonSettingKey.ConstAssetList.Key1, CommonSettingKey.ConstAssetList.Key2, CommonSettingKey.ConstAssetList.Key3, True)
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
            'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_CONST_ASSET_LIST)

            sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_CONSTRUCTION_ASSET_LIST)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As ConstAssetListViewModel In ConstData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(row.CONST_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.CONST_START_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.CONST_END_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.CONST_LENT_START_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.CONST_LENT_END_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_CONTENTS)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD.Trim)
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
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.SITE_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.SITE_EMPLOYEE_NAME)
                If row.CONST_CONFIRM_TS.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringLongYMDHMS(row.CONST_CONFIRM_TS.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_CONFIRM_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_CONFIRM_EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_CONFIRM_REMARK)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD_ASSET.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MngLocLNameAsset)
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
                Dim sLeaseStatus As String = LeaseStatusAsset_Csv.Kanou
                If row.LEASE_STATUS <> LeaseStatusAsset.Kanou Then
                    sLeaseStatus = LeaseStatusAsset_Csv.Fuka
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sLeaseStatus)
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
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_DAY)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_LEASE_DAY_WORK_FLAG)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_DAY)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_DAY_WORK_FLAG)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocLName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocMName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocSName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CatLName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CatMName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CatSName)
                If row.GENERIC_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.GENERIC_LIMIT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PRODUCT_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_CNT)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_DIV)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_NO)
                If row.LEASE_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_LIMIT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6)
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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelConstAssetList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     ConstData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstAssetList.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetConstAssetListData() As List(Of ConstAssetListViewModel)
            Dim lstConstAssetList As List(Of ConstAssetListViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstConstAssetList = db.ConstAssetListView.SqlQuery(CreateSQL(), _paramSqlList.ToArray).ToList

            'データを返却
            Return lstConstAssetList
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
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstAssetList, _LoginInfo)

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

            Return Json(lstLoc_Shokai)
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

#Region "工事資産状態ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 工事資産状態ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetConstAssetStatusData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = CONST_ASSET_STATUS.Lending,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_LEASESTATUS_LENDING).ToString
                },
                New DropdownModel With {
                    .Cd = CONST_ASSET_STATUS.Reserve,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RESERVE).ToString
                },
                New DropdownModel With {
                    .Cd = CONST_ASSET_STATUS.HenkyakuSumi,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_LEASESTATUS_RETURNED).ToString
                }}

            '一覧画面の工事資産状態コンボ設定
            ViewBag.ConstAssetStatusList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchConstAssetStatus)

            Return Json(lst)
        End Function
#End Region

#Region "仮置有無ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 仮置有無ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetTempYesOrNoData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = TempFlg.Cd.Yes,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_TEMP_EXISTS).ToString
                },
                New DropdownModel With {
                    .Cd = TempFlg.Cd.No,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_TEMP_NOT_EXISTS).ToString
                }}

            '一覧画面の仮置有無コンボ設定
            ViewBag.TempYesOrNoList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchTempYesOrNo)

            Return Json(lst)
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
#End Region

#Region "大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 大区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationLData() As JsonResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationLData()
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

            Return Json(lstLocation)
        End Function
#End Region

#Region "中区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 中区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLocationMData(ByVal selectLCode As String) As JsonResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationMData(selectLCode)
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

            '一覧画面のロケーションコンボ設定
            ViewBag.modelLocM = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchLocM)
            _ViewModel.locMDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "小区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCode As String) As JsonResult
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

            '一覧画面のロケーションコンボ設定
            ViewBag.modelLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchLocS)
            _ViewModel.locSDropdownData = lstLocation

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

            '一覧画面のカテゴリーコンボ設定
            ViewBag.modelCatL = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatL)
            _ViewModel.catLDropdownData = lst

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

            Return Json(lst)
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

#Region "工事データ取得"
        ''' <summary>
        ''' 工事データ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetTblConstructionData() As List(Of ConstructionListViewModel)
            Dim lst As New List(Of ConstructionListViewModel)

            Dim sSelKey As String = String.Empty
            Dim info As RedirectConstructionListInfo = Session(RedirectConstructionListInfo.ID)
            If info IsNot Nothing Then
                sSelKey = info.KeyValue
            End If

            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstAssetList, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

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
            sbSQL.AppendLine("  , CONST.MNG_LOC_L_CD AS MNG_LOC_L_CD")
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
            sbSQL.AppendLine("  ON AUTH.LOC_L_CD = CONST.MNG_LOC_L_CD ")
            params.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter("@ReadAuthFlg", AuthInfo.ReadAuthFlg))
            params.Add(New SqlParameter("@UpdAuthFlg", AuthInfo.UpdAuthFlg))
            params.Add(New SqlParameter("@ReadHubAuthFlg", AuthInfo.ReadHubAuthFlg))
            params.Add(New SqlParameter("@UpdHubAuthFlg", AuthInfo.UpdHubAuthFlg))
            params.Add(New SqlParameter("@UpdAuthType", Misc.UpdAuthType.UPD_AUTH))
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL ")
            sbSQL.AppendLine("ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND CONST.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '検索条件を設定する
            '工事番号
            sbSQL.AppendLine("  AND CONST.CONST_NO = @CONST_NO")
            params.Add(New SqlParameter($"@CONST_NO", sSelKey))

            'ORDER句の設定
            sbSQL.AppendLine(" ORDER BY CONST.CONST_NO")

            lst = _db.ConstructionListView.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            ViewBag.tblConstructionListData = lst
            _ViewModel.tblConstructionListData = lst

            Return lst
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

#Region "一覧データ取得用SQLを作成する"
        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String
            '工事一覧から遷移の場合は工事番号取得
            Dim sSelKey As String = String.Empty
            Dim info As RedirectConstructionListInfo = Session(RedirectConstructionListInfo.ID)
            If info IsNot Nothing Then
                sSelKey = info.KeyValue
            End If

            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstAssetList, _LoginInfo)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

#Region "検索条件SQLを作成"
            Dim sbWhr As New StringBuilder
            If Not String.IsNullOrEmpty(sSelKey) Then
                '----- 工事一覧から遷移時の検索条件
                '工事番号 SearchConstNo
                sbWhr.AppendLine("  AND CONST.CONST_NO = @SearchConstNo")
                addSqlParam("SearchConstNo", $"{sSelKey}")
            Else
                '----- メニューから遷移時の検索条件
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
            End If

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

            '状態 SearchConstAssetStatus
            If Not String.IsNullOrEmpty(_ViewModel.SearchConstAssetStatus) Then
                sbWhr.AppendLine("  AND CASSET.CONST_ASSET_STATUS = @SearchConstAssetStatus")
                addSqlParam("SearchConstAssetStatus", $"{_ViewModel.SearchConstAssetStatus}")
            End If

            '仮置有無 SearchTempYesOrNo
            If Not String.IsNullOrEmpty(_ViewModel.SearchTempYesOrNo) Then
                Select Case _ViewModel.SearchTempYesOrNo
                    Case TempFlg.Cd.Yes
                        sbWhr.AppendLine("  AND NOT (CASSET.TEMP_ID IS NULL OR CASSET.TEMP_ID = '') ")
                    Case TempFlg.Cd.No
                        sbWhr.AppendLine("  AND (CASSET.TEMP_ID IS NULL OR CASSET.TEMP_ID = '') ")
                End Select
            End If

            '仮置き場所 SearchTempInfo
            If (Not IsNothing(_ViewModel.SearchTempInfo)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempInfo.Trim)) Then
                sbWhr.AppendLine("  AND CONCAT(TEMP.TEMP_INFO1, TEMP.TEMP_INFO2, TEMP.TEMP_INFO3, TEMP.TEMP_INFO4, TEMP.TEMP_INFO5, TEMP.TEMP_INFO6, TEMP.TEMP_INFO7) like " & "@SearchTempInfo")
                addSqlParam("SearchTempInfo", $"{"%" & _ViewModel.SearchTempInfo.Trim & "%"}")
            End If

            '仮置き備考 SearchAssetName
            If (Not IsNothing(_ViewModel.SearchTempRemark)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTempRemark.Trim)) Then
                sbWhr.AppendLine("  AND TEMP.TEMP_REMARK like " & "@SearchTempRemark")
                addSqlParam("SearchTempRemark", $"{"%" & _ViewModel.SearchTempRemark.Trim & "%"}")
            End If

            'ロケ大区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                sbWhr.AppendLine("  AND ASSET.LOC_L_CD = @SearchLocL")
                addSqlParam("SearchLocL", $"{_ViewModel.SearchLocL}")
            End If

            'ロケ中区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocM) Then
                sbWhr.AppendLine("  AND ASSET.LOC_M_CD = @SearchLocM")
                addSqlParam("SearchLocM", $"{_ViewModel.SearchLocM}")
            End If

            'ロケ小区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocS) Then
                sbWhr.AppendLine("  AND ASSET.LOC_S_CD = @SearchLocS")
                addSqlParam("SearchLocS", $"{_ViewModel.SearchLocS}")
            End If

            'カテゴリ大分類 SearchCatL
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatL) Then
                sbWhr.AppendLine("  AND ASSET.CAT_L_CD = @SearchCatL")
                addSqlParam("SearchCatL", $"{_ViewModel.SearchCatL}")
            End If

            'カテゴリ中分類 SearchCatM
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatM) Then
                sbWhr.AppendLine("  AND ASSET.CAT_M_CD = @SearchCatM")
                addSqlParam("SearchCatM", $"{_ViewModel.SearchCatM}")
            End If

            'カテゴリ小分類 SearchCatS
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatS) Then
                sbWhr.AppendLine("  AND ASSET.CAT_S_CD = @SearchCatS")
                addSqlParam("SearchCatS", $"{_ViewModel.SearchCatS}")
            End If

            'メーカー SearchMaker
            If (Not IsNothing(_ViewModel.SearchMaker)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchMaker.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.MAKER like " & "@SearchMaker")
                addSqlParam("SearchMaker", $"{"%" & _ViewModel.SearchMaker.Trim & "%"}")
            End If

            '品番 SearchProductNo
            If (Not IsNothing(_ViewModel.SearchProductNo)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchProductNo.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.PRODUCT_NO like " & "@SearchProductNo")
                addSqlParam("SearchProductNo", $"{"%" & _ViewModel.SearchProductNo.Trim & "%"}")
            End If

            '型番 SearchModelNo
            If (Not IsNothing(_ViewModel.SearchModelNo)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchModelNo.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.MODEL_NO like " & "@SearchModelNo")
                addSqlParam("SearchModelNo", $"{"%" & _ViewModel.SearchModelNo.Trim & "%"}")
            End If

            'リース管理番号 SearchLeaseNo
            If (Not IsNothing(_ViewModel.SearchLeaseNo)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchLeaseNo.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.LEASE_NO like " & "@SearchLeaseNo")
                addSqlParam("SearchLeaseNo", $"{"%" & _ViewModel.SearchLeaseNo.Trim & "%"}")
            End If

            'リース期限 SearchLeaseLimit
            If (Not IsNothing(_ViewModel.SearchLeaseLimit)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchLeaseLimit.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.LEASE_LIMIT <= " & "@SearchLeaseLimit")
                addSqlParam("SearchLeaseLimit", $"{_ViewModel.SearchLeaseLimit.Replace("-", "")}")
            End If

            'フリー項目1 SearchFreeItem1 → 資産購入日
            If (Not IsNothing(_ViewModel.SearchFreeItem1)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem1.Trim)) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem1).ToString("yyyy/MM/dd")
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM1 like " & "@SearchFreeItem1")
                addSqlParam("SearchFreeItem1", $"{"%" & sDate & "%"}")
            End If

            'フリー項目2 SearchFreeItem2
            If (Not IsNothing(_ViewModel.SearchFreeItem2)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem2.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM2 like " & "@SearchFreeItem2")
                addSqlParam("SearchFreeItem2", $"{"%" & _ViewModel.SearchFreeItem2.Trim & "%"}")
            End If

            'フリー項目3 SearchFreeItem3
            If (Not IsNothing(_ViewModel.SearchFreeItem3)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem3.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM3 like " & "@SearchFreeItem3")
                addSqlParam("SearchFreeItem3", $"{"%" & _ViewModel.SearchFreeItem3.Trim & "%"}")
            End If

            'フリー項目4 SearchFreeItem4
            If (Not IsNothing(_ViewModel.SearchFreeItem4)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem4.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM4 like " & "@SearchFreeItem4")
                addSqlParam("SearchFreeItem4", $"{"%" & _ViewModel.SearchFreeItem4.Trim & "%"}")
            End If

            'フリー項目5 SearchFreeItem5
            If (Not IsNothing(_ViewModel.SearchFreeItem5)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem5.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM5 like " & "@SearchFreeItem5")
                addSqlParam("SearchFreeItem5", $"{"%" & _ViewModel.SearchFreeItem5.Trim & "%"}")
            End If

            'フリー項目6 SearchFreeItem6
            If (Not IsNothing(_ViewModel.SearchFreeItem6)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem6.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM6 like " & "@SearchFreeItem6")
                addSqlParam("SearchFreeItem6", $"{"%" & _ViewModel.SearchFreeItem6.Trim & "%"}")
            End If

            'フリー項目7 SearchFreeItem7
            If (Not IsNothing(_ViewModel.SearchFreeItem7)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem7.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM7 like " & "@SearchFreeItem7")
                addSqlParam("SearchFreeItem7", $"{"%" & _ViewModel.SearchFreeItem7.Trim & "%"}")
            End If

            'フリー項目8 SearchFreeItem8
            If (Not IsNothing(_ViewModel.SearchFreeItem8)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem8.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM8 like " & "@SearchFreeItem8")
                addSqlParam("SearchFreeItem8", $"{"%" & _ViewModel.SearchFreeItem8.Trim & "%"}")
            End If

            'フリー項目9 SearchFreeItem9
            If (Not IsNothing(_ViewModel.SearchFreeItem9)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem9.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM9 like " & "@SearchFreeItem9")
                addSqlParam("SearchFreeItem9", $"{"%" & _ViewModel.SearchFreeItem9.Trim & "%"}")
            End If

            'フリー項目10 SearchFreeItem10
            If (Not IsNothing(_ViewModel.SearchFreeItem10)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem10.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM10 like " & "@SearchFreeItem10")
                addSqlParam("SearchFreeItem10", $"{"%" & _ViewModel.SearchFreeItem10.Trim & "%"}")
            End If

            'フリー項目11 SearchFreeItem11
            If (Not IsNothing(_ViewModel.SearchFreeItem11)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem11.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM11 like " & "@SearchFreeItem11")
                addSqlParam("SearchFreeItem11", $"{"%" & _ViewModel.SearchFreeItem11.Trim & "%"}")
            End If

            'フリー項目12 SearchFreeItem12
            If (Not IsNothing(_ViewModel.SearchFreeItem12)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem12.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM12 like " & "@SearchFreeItem12")
                addSqlParam("SearchFreeItem12", $"{"%" & _ViewModel.SearchFreeItem12.Trim & "%"}")
            End If

            'フリー項目13 SearchFreeItem13
            If (Not IsNothing(_ViewModel.SearchFreeItem13)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem13.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM13 like " & "@SearchFreeItem13")
                addSqlParam("SearchFreeItem13", $"{"%" & _ViewModel.SearchFreeItem13.Trim & "%"}")
            End If

            'フリー項目14 SearchFreeItem14
            If (Not IsNothing(_ViewModel.SearchFreeItem14)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem14.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM14 like " & "@SearchFreeItem14")
                addSqlParam("SearchFreeItem14", $"{"%" & _ViewModel.SearchFreeItem14.Trim & "%"}")
            End If

            'フリー項目15 SearchFreeItem15
            If (Not IsNothing(_ViewModel.SearchFreeItem15)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem15.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM15 like " & "@SearchFreeItem15")
                addSqlParam("SearchFreeItem15", $"{"%" & _ViewModel.SearchFreeItem15.Trim & "%"}")
            End If

            'フリー項目16 SearchFreeItem16
            If (Not IsNothing(_ViewModel.SearchFreeItem16)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem16.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM16 like " & "@SearchFreeItem16")
                addSqlParam("SearchFreeItem16", $"{"%" & _ViewModel.SearchFreeItem16.Trim & "%"}")
            End If

            'フリー項目17 SearchFreeItem17
            If (Not IsNothing(_ViewModel.SearchFreeItem17)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem17.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM17 like " & "@SearchFreeItem17")
                addSqlParam("SearchFreeItem17", $"{"%" & _ViewModel.SearchFreeItem17.Trim & "%"}")
            End If

            'フリー項目18 SearchFreeItem18
            If (Not IsNothing(_ViewModel.SearchFreeItem18)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem18.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM18 like " & "@SearchFreeItem18")
                addSqlParam("SearchFreeItem18", $"{"%" & _ViewModel.SearchFreeItem18.Trim & "%"}")
            End If

            'フリー項目19 SearchFreeItem19
            If (Not IsNothing(_ViewModel.SearchFreeItem19)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem19.Trim)) Then
                sbWhr.AppendLine("  AND ASSET.FREE_ITEM19 like " & "@SearchFreeItem19")
                addSqlParam("SearchFreeItem19", $"{"%" & _ViewModel.SearchFreeItem19.Trim & "%"}")
            End If
#End Region

            Dim sbSQL As StringBuilder = New StringBuilder

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
            sbSQL.AppendLine("  , CONST.MNG_LOC_L_CD")
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
            sbSQL.AppendLine("  , ASSET.ASSET_NO ")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("  , ASSET.ASSET_DIV ")
            sbSQL.AppendLine("  , CASSET.CONST_ASSET_STATUS ")
            sbSQL.AppendLine("  , CASSET.TEMP_ID ")
            sbSQL.AppendLine("  , CASSET.PREV_TEMP_FLAG ")
            sbSQL.AppendLine("  , CASSET.TASK_TS ")
            sbSQL.AppendLine("  , TEMP.TEMP_INFO1 ")
            sbSQL.AppendLine("  , TEMP.TEMP_INFO2 ")
            sbSQL.AppendLine("  , TEMP.TEMP_INFO3 ")
            sbSQL.AppendLine("  , TEMP.TEMP_INFO4 ")
            sbSQL.AppendLine("  , TEMP.TEMP_INFO5 ")
            sbSQL.AppendLine("  , TEMP.TEMP_INFO6 ")
            sbSQL.AppendLine("  , TEMP.TEMP_INFO7 ")
            sbSQL.AppendLine("  , TEMP.TEMP_REMARK ")
            sbSQL.AppendLine("  , ASSET.MNG_LOC_L_CD AS MNG_LOC_L_CD_ASSET")
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
            sbSQL.AppendLine("  , ASSET.FREE_ITEM11 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM12 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM13 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM14 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM15 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM16 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM17 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM18 ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM19 ")
            sbSQL.AppendLine("  , ASSET.ASSET_STATUS ")
            sbSQL.AppendLine("  , ASSET.LEASE_STATUS ")
            sbSQL.AppendLine("  , ASSET.LEASE_PLACE_CD ")
            sbSQL.AppendLine("  , ASSET.LEASE_PLACE_NAME ")
            sbSQL.AppendLine("  , ASSET.REMARK ")
            sbSQL.AppendLine("  , ASSET.LEASE_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , ASSET.LEASE_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , ASSET.LEASE_DT ")
            sbSQL.AppendLine("  , ASSET.LEASE_DUE_DT ")
            sbSQL.AppendLine("  , ASSET.MAINTE_LEASE_NUM ")
            sbSQL.AppendLine("  , ASSET.MAINTE_LEASE_DAY ")
            sbSQL.AppendLine("  , ASSET.MAINTE_LEASE_DAY_WORK_FLAG ")
            sbSQL.AppendLine("  , ASSET.MAINTE_DAY ")
            sbSQL.AppendLine("  , ASSET.MAINTE_DAY_WORK_FLAG ")
            sbSQL.AppendLine("  , ASSET.GENERIC_LIMIT ")
            sbSQL.AppendLine("  , ASSET.LEASE_CNT ")
            sbSQL.AppendLine("  , CASSET.INS_DATETIME ")
            sbSQL.AppendLine("  , CASSET.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CASSET.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CASSET.UPD_DATETIME ")
            sbSQL.AppendLine("  , CASSET.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CASSET.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CASSET.UPD_TASK")
            sbSQL.AppendLine("  , LOCL.LOC_NAME AS LocLName ")
            sbSQL.AppendLine("  , LOCM.LOC_NAME AS LocMName ")
            sbSQL.AppendLine("  , LOCS.LOC_NAME AS LocSName ")
            sbSQL.AppendLine("  , CATL.CAT_NAME AS CatLName ")
            sbSQL.AppendLine("  , CATM.CAT_NAME AS CatMName ")
            sbSQL.AppendLine("  , CATS.CAT_NAME AS CatSName ")
            sbSQL.AppendLine("  , MNGLOCL_CONST.LOC_NAME AS MngLocLName ")
            sbSQL.AppendLine("  , MNGLOCL_ASSET.LOC_NAME AS MngLocLNameAsset ")
            sbSQL.AppendLine("  , CONCAT(CONST.CONST_NO,',',ASSET.ASSET_NO) AS KEY_VALUE ")
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM M_CONSTRUCTION CONST ")
            sbSQL.AppendLine("INNER JOIN T_CONST_ASSET CASSET ")
            sbSQL.AppendLine("ON CONST.CONST_NO = CASSET.CONST_NO ")
            sbSQL.AppendLine("INNER JOIN M_ASSET ASSET ")
            sbSQL.AppendLine("ON CASSET.ASSET_NO = ASSET.ASSET_NO ")
            sbSQL.AppendLine("LEFT JOIN T_TEMPORARY TEMP ")
            sbSQL.AppendLine("ON CASSET.TEMP_ID = TEMP.TEMP_ID ")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
            sbSQL.AppendLine("  ON AUTH.LOC_L_CD = CONST.MNG_LOC_L_CD ")
            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{AuthInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{AuthInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{AuthInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{AuthInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthType", $"{Misc.UpdAuthType.UPD_AUTH}")

            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL_CONST ")
            sbSQL.AppendLine("ON MNGLOCL_CONST.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND CONST.MNG_LOC_L_CD = MNGLOCL_CONST.LOC_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL_ASSET ")
            sbSQL.AppendLine("ON MNGLOCL_ASSET.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.MNG_LOC_L_CD = MNGLOCL_ASSET.LOC_L_CD")
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

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            sbSQL.AppendLine(sbWhr.ToString)

            If String.IsNullOrEmpty(sSelKey) Then
                '----- 通常時の検索条件
                sbSQL.AppendLine(" ORDER BY CONST.CONST_NO,")
                sbSQL.AppendLine("          ASSET.ASSET_NO")

            Else
                '----- 工事一覧から遷移時の検索条件
                sbSQL.AppendLine(" ORDER BY ASSET.ASSET_NO")
            End If

            Return sbSQL.ToString()
        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New ConstAssetListViewModel())
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
            _viewResult.ElementAt(0).SearchAssetNo = _ViewModel.SearchAssetNo
            _viewResult.ElementAt(0).SearchAssetName = _ViewModel.SearchAssetName
            _viewResult.ElementAt(0).SearchTempInfo = _ViewModel.SearchTempInfo
            _viewResult.ElementAt(0).SearchTempRemark = _ViewModel.SearchTempRemark
            _viewResult.ElementAt(0).SearchLocL = _ViewModel.SearchLocL
            _viewResult.ElementAt(0).SearchLocM = _ViewModel.SearchLocM
            _viewResult.ElementAt(0).SearchLocS = _ViewModel.SearchLocS
            _viewResult.ElementAt(0).SearchCatL = _ViewModel.SearchCatL
            _viewResult.ElementAt(0).SearchCatM = _ViewModel.SearchCatM
            _viewResult.ElementAt(0).SearchCatS = _ViewModel.SearchCatS
            _viewResult.ElementAt(0).SearchMaker = _ViewModel.SearchMaker
            _viewResult.ElementAt(0).SearchProductNo = _ViewModel.SearchProductNo
            _viewResult.ElementAt(0).SearchModelNo = _ViewModel.SearchModelNo
            _viewResult.ElementAt(0).SearchLeaseNo = _ViewModel.SearchLeaseNo
            _viewResult.ElementAt(0).SearchLeaseLimit = _ViewModel.SearchLeaseLimit
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
            _viewResult.ElementAt(0).SearchFreeItem12 = _ViewModel.SearchFreeItem12
            _viewResult.ElementAt(0).SearchFreeItem13 = _ViewModel.SearchFreeItem13
            _viewResult.ElementAt(0).SearchFreeItem14 = _ViewModel.SearchFreeItem14
            _viewResult.ElementAt(0).SearchFreeItem15 = _ViewModel.SearchFreeItem15
            _viewResult.ElementAt(0).SearchFreeItem16 = _ViewModel.SearchFreeItem16
            _viewResult.ElementAt(0).SearchFreeItem17 = _ViewModel.SearchFreeItem17
            _viewResult.ElementAt(0).SearchFreeItem18 = _ViewModel.SearchFreeItem18
            _viewResult.ElementAt(0).SearchFreeItem19 = _ViewModel.SearchFreeItem19
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
            _viewResult.ElementAt(0).GridInfoTop = ToolColSet.GetGridInfo(_db, DispId.CONST_ASSET_LIST_TOP)
            _viewResult.ElementAt(0).FixColCntTop = _db.ColSet.Find(DispId.CONST_ASSET_LIST_TOP).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfoBottom = ToolColSet.GetGridInfo(_db, DispId.CONST_ASSET_LIST_BOTTOM)
            _viewResult.ElementAt(0).FixColCntBottom = _db.ColSet.Find(DispId.CONST_ASSET_LIST_BOTTOM).FIX_COL_CNT
        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As ConstAssetListViewModel)
            _Model.CONST_NO = model.CONST_NO
            _Model.ASSET_NO = model.ASSET_NO
            _Model.CONST_ASSET_STATUS = model.CONST_ASSET_STATUS
            _Model.TEMP_ID = model.TEMP_ID
            _Model.PREV_TEMP_FLAG = model.PREV_TEMP_FLAG
            _Model.TASK_TS = model.TASK_TS
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub
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
    End Class
End Namespace