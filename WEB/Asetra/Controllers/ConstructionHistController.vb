Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports System.Web.WebPages
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' 工事履歴一覧画面表示用のクラス
    ''' </summary>
    Public Class ConstructionHistController
        Inherits PageBaseController

#Region "クラス定数"
        ''' <summary>
        ''' ページ間のチェックボックス対応
        ''' </summary>
        Private Enum ConstructionHistKey
            CONST_NO = 0
            CONST_WORK_TS = 1
            CONST_WORK_DIV = 2
        End Enum

#End Region

#Region "クラス変数"
        Private _Model As ConstructionHistModel = New ConstructionHistModel                                 'ConstructionHistモデル
        Private _ViewModel As ConstructionHistViewModel = New ConstructionHistViewModel                     'ConstructionHist一覧表示用モデル
        Protected _viewResult As List(Of ConstructionHistViewModel) = New List(Of ConstructionHistViewModel)    '結果View
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
        Public Function Init(ByVal ViewModel As ConstructionHistViewModel, ByVal actionname As String) As ActionResult
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
        Public Function Index(ByVal ViewModel As ConstructionHistViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_CONSTRUCTION_HIST)
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
            Dim model As ConstructionHistViewModel = Session(CommonSessionKey.CREAGE_CONSTRUCTION_HIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            Dim info As RedirectConstructionListInfo = Session(RedirectConstructionListInfo.ID)
            If info IsNot Nothing Then
                If Session(CommonSessionKey.CREAGE_CONSTRUCTION_HIST_SEARCH_KEY) IsNot Nothing Then
                    Dim senimaeModel As ConstructionHistViewModel = Session(CommonSessionKey.CREAGE_CONSTRUCTION_HIST_SEARCH_KEY)
                    _ViewModel = senimaeModel
                    Session.Remove(CommonSessionKey.CREAGE_CONSTRUCTION_HIST_SEARCH_KEY)
                End If

                If info.RedirectPage = RedirectConstructionListInfo.RedirectPageType.ConstructionHist Then
                    '工事資産履歴一覧画面から戻ったときの処理
                    'リダイレクト情報作成(メニューから呼び出した時と同じ状態にする)
                    Dim info2 As New RedirectConstructionListInfo
                    info2.RedirectPage = RedirectConstructionListInfo.RedirectPageType.Menu
                    info2.KeyValue = String.Empty

                    Session(RedirectConstructionListInfo.ID) = info2
                Else
                    ViewBag.ConstructionListKey = info.KeyValue
                End If
            End If
            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '初期表示
                '項目配置情報取得
                Dim dictGridInfo = ToolColSet.GetGridInfo(_db, DispId.CONSTRUCTION_HIST_TOP)

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

            '最新のチェック状態でセッション情報を更新する(追加および削除)
            UpdateChecedInfo(_ViewModel.SelectKeys, _ViewModel.NoneSelectKeys)

            '工事状態ドロップダウンリストデータ取得
            GetConstStatusData()
            '管理ロケーションドロップダウンリスト作成
            GetMngLocationLData()
            '作業内容ドロップダウンリスト作成
            GetConstWorkDivData()
            '照合結果ドロップダウンリスト作成
            GetConstMatchDivData()

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.CONSTRUCTION_HIST_BOTTOM)
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
                Session.Remove(CommonSessionKey.CREAGE_CONSTRUCTION_HIST)
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
            Dim query As DbSqlQuery(Of ConstructionHistViewModel) = _db.ConstructionHistView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function Create(ByVal ViewModel As ConstructionHistViewModel, ByVal OnCreteButton As String) As ActionResult
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
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_HIST) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_HIST) = _ViewModel

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelConstructionHist.TITLE)

            _db.ConstructionHist.Add(_Model)

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelConstructionHist.TITLE)

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
            Dim model As ConstructionHistModel = db.ConstructionHist.Find(ID)

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
        Public Overridable Function Delete(ByVal ViewModel As ConstructionHistViewModel) As ActionResult
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
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_HIST) = _ViewModel

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
        Public Function IchiranCsvExport(ViewModel As ConstructionHistViewModel) As ActionResult
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

            Dim ConstructionHistData As List(Of ConstructionHistViewModel)

            '工事マスタデータ取得
            ConstructionHistData = GetConstructionHistData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.ConstructionHist.KeyDiv, CommonSettingKey.ConstructionHist.Key1, CommonSettingKey.ConstructionHist.Key2, CommonSettingKey.ConstructionHist.Key3, True)
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
            'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_CONSTRUCTION_HIST)

            sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_CONSTRUCTION_HISTORY_LIST)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As ConstructionHistViewModel In ConstructionHistData
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
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringLongYMDHMS(row.CONST_WORK_TS))
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.ConstRegist
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_CONST_REGIST))
                    Case ConstWorkDiv.TempLink
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_TEMP_LINK))
                    Case ConstWorkDiv.Lease
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_LEASE))
                    Case ConstWorkDiv.InMatch
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_IN_MATCH))
                    Case ConstWorkDiv.TempRegist
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_TEMP_REGIST))
                    Case ConstWorkDiv.OutMatch
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_OUT_MATCH))
                    Case ConstWorkDiv.MatchReturn
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_MATCH_RETURN))
                    Case ConstWorkDiv.Returned
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_RETURNED))
                    Case ConstWorkDiv.ConstComplete
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_CONST_COMPLETE))
                    Case ConstWorkDiv.AnotherConstLink
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ANOTHER_CONST_LINK))
                    Case ConstWorkDiv.TempRelease
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_TEMP_RELEASE))
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.InMatch, ConstWorkDiv.OutMatch, ConstWorkDiv.MatchReturn
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_MATCH_DIV)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.TempLink, ConstWorkDiv.Lease
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LENT_NUM)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.MatchReturn, ConstWorkDiv.Returned
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_NUM)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.TempRegist, ConstWorkDiv.AnotherConstLink, ConstWorkDiv.TempRelease
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_REG_NUM)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.InMatch
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_IN_MATCH_NUM.ToString + "/" + row.CONST_MATCH_TTLNUM.ToString)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.OutMatch, ConstWorkDiv.MatchReturn
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_OUT_MATCH_NUM.ToString + "/" + row.CONST_MATCH_TTLNUM.ToString)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_IN_NUM_HIST)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_OUT_NUM_HIST)

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelConstructionHist.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ":  " &
                                                     ConstructionHistData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelConstructionHist.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetConstructionHistData() As List(Of ConstructionHistViewModel)
            Dim lstConstructionHist As List(Of ConstructionHistViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstConstructionHist = db.ConstructionHistView.SqlQuery(CreateSQL(), _paramSqlList.ToArray).ToList

            'データを返却
            Return lstConstructionHist
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
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstructionHist, _LoginInfo)

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

#Region "工事作業区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 工事作業区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetConstWorkDivData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = ConstWorkDiv.ConstRegist,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_CONST_REGIST).ToString
                },
                New DropdownModel With {
                    .Cd = ConstWorkDiv.TempLink,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_TEMP_LINK).ToString
                },
                New DropdownModel With {
                    .Cd = ConstWorkDiv.Lease,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_LEASE).ToString
                },
                New DropdownModel With {
                    .Cd = ConstWorkDiv.InMatch,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_IN_MATCH).ToString
                },
                New DropdownModel With {
                    .Cd = ConstWorkDiv.TempRegist,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_TEMP_REGIST).ToString
                },
                New DropdownModel With {
                    .Cd = ConstWorkDiv.OutMatch,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_OUT_MATCH).ToString
                },
                New DropdownModel With {
                    .Cd = ConstWorkDiv.MatchReturn,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_MATCH_RETURN).ToString
                },
                New DropdownModel With {
                    .Cd = ConstWorkDiv.Returned,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_RETURNED).ToString
                },
                New DropdownModel With {
                    .Cd = ConstWorkDiv.ConstComplete,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_CONST_COMPLETE).ToString
                },
                New DropdownModel With {
                    .Cd = ConstWorkDiv.AnotherConstLink,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_ANOTHER_CONST_LINK).ToString
                },
                New DropdownModel With {
                    .Cd = ConstWorkDiv.TempRelease,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_TEMP_RELEASE).ToString
                }}

            '一覧画面の工事作業区分コンボ設定
            ViewBag.ConstWorkDivList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchConstWorkDiv)

            Return Json(lst)
        End Function
#End Region

#Region "照合結果区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 照合結果区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetConstMatchDivData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = ConstMatchDiv.None,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_NONE_MATCH).ToString
                },
                New DropdownModel With {
                    .Cd = ConstMatchDiv.Ok,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_MATCH_OK).ToString
                },
                New DropdownModel With {
                    .Cd = ConstMatchDiv.Ng,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_MATCH_NG).ToString
                }}

            '一覧画面の仮置有無コンボ設定
            ViewBag.ConstMatchDivList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchConstMatchDiv)

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
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstructionHist, _LoginInfo)

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

            '作業日 SearchConstWorkTs
            If (Not IsNothing(_ViewModel.SearchConstWorkTs)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstWorkTs.Trim)) Then
                sbWhr.AppendLine("  AND CONST.CONST_START_DT <= " & "@SearchConstWorkTs")
                sbWhr.AppendLine("  AND CONST.CONST_END_DT >= " & "@SearchConstWorkTs")
                addSqlParam("SearchConstWorkTs", $"{_ViewModel.SearchConstWorkTs.Replace("-", "")}")
            End If

            '作業内容 SearchConstWorkDiv
            If (Not IsNothing(_ViewModel.SearchConstWorkDiv)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchConstWorkDiv.Trim)) Then
                sbWhr.AppendLine("  AND CLOG.CONST_WORK_DIV like " & "@SearchConstWorkDiv")
                addSqlParam("SearchConstWorkDiv", $"{_ViewModel.SearchConstWorkDiv}")
            End If

            '照合結果 SearchConstMatchDiv
            If Not String.IsNullOrEmpty(_ViewModel.SearchConstMatchDiv) Then
                sbWhr.AppendLine("  AND CLOG.CONST_MATCH_DIV = @SearchConstMatchDiv")
                addSqlParam("SearchConstMatchDiv", $"{_ViewModel.SearchConstMatchDiv}")
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
            sbSQL.AppendLine("  , CLOG.CONST_WORK_TS ")
            sbSQL.AppendLine("  , CLOG.CONST_WORK_DIV ")
            sbSQL.AppendLine("  , CLOG.CONST_MATCH_DIV ")
            sbSQL.AppendLine("  , CLOG.LENT_NUM ")
            sbSQL.AppendLine("  , CLOG.LEASE_NUM ")
            sbSQL.AppendLine("  , CLOG.TEMP_REG_NUM ")
            sbSQL.AppendLine("  , CLOG.CONST_IN_MATCH_NUM ")
            sbSQL.AppendLine("  , CLOG.CONST_OUT_MATCH_NUM ")
            sbSQL.AppendLine("  , CLOG.CONST_IN_NUM AS CONST_IN_NUM_HIST ")
            sbSQL.AppendLine("  , CLOG.CONST_MATCH_TTLNUM")
            sbSQL.AppendLine("  , CLOG.TEMP_NUM ")
            sbSQL.AppendLine("  , CLOG.CONST_OUT_NUM AS CONST_OUT_NUM_HIST ")
            sbSQL.AppendLine("  , CLOG.INS_DATETIME ")
            sbSQL.AppendLine("  , CLOG.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CLOG.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CLOG.UPD_DATETIME ")
            sbSQL.AppendLine("  , CLOG.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CLOG.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CLOG.UPD_TASK")
            sbSQL.AppendLine("  , MNGLOCL.LOC_NAME As MngLocLName ")
            sbSQL.AppendLine("  , CONCAT(CONST.CONST_NO,',',CLOG.CONST_WORK_TS,',',CLOG.CONST_WORK_DIV) AS KEY_VALUE ")
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM M_CONSTRUCTION CONST ")
            sbSQL.AppendLine("INNER JOIN T_CONST_LOG CLOG ")
            sbSQL.AppendLine("ON CONST.CONST_NO = CLOG.CONST_NO ")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
            sbSQL.AppendLine("  ON AUTH.LOC_L_CD = CONST.MNG_LOC_L_CD ")
            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{AuthInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{AuthInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{AuthInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{AuthInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthType", $"{Misc.UpdAuthType.UPD_AUTH}")

            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL ")
            sbSQL.AppendLine("ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND CONST.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            sbSQL.AppendLine(sbWhr.ToString)

            If String.IsNullOrEmpty(sSelKey) Then
                '----- 通常時の検索条件
                sbSQL.AppendLine(" ORDER BY CONST.CONST_NO,")
                sbSQL.AppendLine("          CLOG.CONST_WORK_TS")

            Else
                '----- 工事一覧から遷移時の検索条件
                sbSQL.AppendLine(" ORDER BY CLOG.CONST_WORK_TS")
            End If

            Return sbSQL.ToString()
        End Function
#End Region

#Region "元画面の検索条件をセッションに保存し、工事資産履歴一覧画面に遷移します"
        ''' <summary>
        ''' 元画面の検索条件をセッションに保存し、工事資産履歴一覧画面に遷移します
        ''' </summary>
        <ValidateInput(False)>
        Public Function SetKeys(ByVal ViewModel As ConstructionHistViewModel, ByVal SelectedKeys As String) As ActionResult
            Session.Remove(CommonSessionKey.CREAGE_CONST_ASSET_HIST_KEY)
            Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST_KEY) = SelectedKeys

            '検索条件をセッションに保存
            Session(CommonSessionKey.CREAGE_CONSTRUCTION_HIST_SEARCH_KEY) = ViewModel

            'リダイレクト情報作成
            Dim info As RedirectConstructionListInfo = Session(RedirectConstructionListInfo.ID)
            If info Is Nothing OrElse info.RedirectPage <> RedirectConstructionListInfo.RedirectPageType.ConstructionList Then
                '遷移情報セッションクリア
                If Session(RedirectConstructionListInfo.ID) IsNot Nothing Then
                    Session.Remove(RedirectConstructionListInfo.ID)
                End If
                Dim info2 As New RedirectConstructionListInfo
                info2.RedirectPage = RedirectConstructionListInfo.RedirectPageType.ConstructionHist
                info2.KeyValue = String.Empty

                Session(RedirectConstructionListInfo.ID) = info2
            End If

            '結果表示
            Return RedirectToAction("Index", "ConstAssetHist")
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
                    NoneSelectKeys = String.Join("/", New String() {NoneSelectKeys, $"{_ViewModel.CONST_NO},{_ViewModel.CONST_WORK_TS},{_ViewModel.CONST_WORK_DIV}"})
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

#Region "共通"

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New ConstructionHistViewModel())
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
            _viewResult.ElementAt(0).SearchConstWorkTs = _ViewModel.SearchConstWorkTs
            _viewResult.ElementAt(0).SearchConstWorkDiv = _ViewModel.SearchConstWorkDiv
            _viewResult.ElementAt(0).SearchConstMatchDiv = _ViewModel.SearchConstMatchDiv
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
            _viewResult.ElementAt(0).GridInfoTop = ToolColSet.GetGridInfo(_db, DispId.CONSTRUCTION_HIST_TOP)
            _viewResult.ElementAt(0).FixColCntTop = _db.ColSet.Find(DispId.CONSTRUCTION_HIST_TOP).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfoBottom = ToolColSet.GetGridInfo(_db, DispId.CONSTRUCTION_HIST_BOTTOM)
            _viewResult.ElementAt(0).FixColCntBottom = _db.ColSet.Find(DispId.CONSTRUCTION_HIST_BOTTOM).FIX_COL_CNT
        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As ConstructionHistViewModel)
            _Model.CONST_NO = model.CONST_NO
            _Model.CONST_WORK_TS = model.CONST_WORK_TS
            _Model.CONST_WORK_DIV = model.CONST_WORK_DIV
            _Model.CONST_MATCH_DIV = model.CONST_MATCH_DIV
            _Model.LENT_NUM = model.LENT_NUM
            _Model.LEASE_NUM = model.LEASE_NUM
            _Model.TEMP_REG_NUM = model.TEMP_REG_NUM
            _Model.CONST_IN_MATCH_NUM = model.CONST_IN_MATCH_NUM
            _Model.CONST_OUT_MATCH_NUM = model.CONST_OUT_MATCH_NUM
            _Model.CONST_IN_NUM = model.CONST_IN_NUM
            _Model.TEMP_NUM = model.TEMP_NUM
            _Model.CONST_OUT_NUM = model.CONST_OUT_NUM
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