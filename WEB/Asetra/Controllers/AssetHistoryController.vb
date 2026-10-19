Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports System.IO
Imports System.Web.WebPages
Imports Asetra.Dao
Imports Asetra.Misc
Imports Asetra.Models
Imports Org.BouncyCastle.Ocsp

Namespace Controllers

    ''' <summary>
    ''' 資産履歴照会画面表示用のクラス
    ''' </summary>
    Public Class AssetHistoryController
        Inherits PageBaseController

#Region "クラス定数"

#End Region

#Region "クラス変数"
        Private _Model As AssetHistoryModel = New AssetHistoryModel                                 'AssetHistoryモデル
        Private _ViewModel As AssetHistoryViewModel = New AssetHistoryViewModel                     'AssetHistory一覧表示用モデル
        Protected _viewResult As List(Of AssetHistoryViewModel) = New List(Of AssetHistoryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _params As List(Of SqlParameter)                'SQLパラメータ
        Private _isSort As Boolean = True                       'ソートフラグ
        Private _AuthInfo As AuthInfo = Nothing                 '権限情報
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As AssetHistoryViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
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
        Public Function Index(ByVal ViewModel As AssetHistoryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_ASSET_HISTORY)
            End If

            '上限確認フラグ 初期化
            ViewBag.isLimitFlg = False
            '一覧件数　初期化
            ViewBag.cntList = 0
            '設定マスタ上限件数　初期化
            ViewBag.cntLimit = 0
            '一覧表示フラグ
            Dim isDisp As Boolean = True

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As AssetHistoryViewModel = Session(CommonSessionKey.CREAGE_ASSET_HISTORY)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            '2023/5/29 Sato ADD START
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            ViewBag.DrugsFunc = False
            '2023/5/29 Sato ADD END

            '検索ボタンがクリックされた場合
            If Me._Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session(CommonSessionKey.SEARCH_FLG) = True
            End If

            '20230901 ADD Start Shirakawa
            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid                      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Asset_Hist.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Hist.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Hist.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Hist.Key3)
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

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '2023/05/10 Kawamoto ADD START
                '設定マスタ（薬品管理機能）の取得
                Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
                                                                      CommonSettingKey.Drugs_Function.Key1,
                                                                      CommonSettingKey.Drugs_Function.Key2,
                                                                      CommonSettingKey.Drugs_Function.Key3, True)
                '薬品機能がONの場合
                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                    ViewBag.DrugsFunc = True
                Else
                    _ViewModel.SearchProcessDivDC = False
                End If
                '2023/05/10 Kawamoto ADD END

                '権限情報の取得
                _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetHistory, _LoginInfo)

                '検索条件の復元有無
                _ViewModel.IsRestoreOwnSearchConditions = True

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

            'ADD Start 2022/8/1
            'UPD 2023/04/30 ローカルではなくフィールド変数のViewModelを渡すように変更
            'If ChkSelectAllStatus(ViewModel) Then
            If ChkSelectAllStatus(_ViewModel, ViewBag.DrugsFunc) Then
                '処理区分が全選択状態の場合、全選択ボタン押下フラグをセットする
                Session(CommonSessionKey.ALL_SELECT_FLG) = True
            End If
            'ADD End 2022/8/1

            'ロケーション
            ' 大区分ドロップダウンリスト作成
            GetLocationLData()
            '[CC431] UPD Start 20261007
            ' 中区分・小区分（ロケーションマスタにあり、権限があるもの。場所から選ぶの ON／OFF で連動）
            Dim authForLoc As AuthInfo = Nothing
            If Not IsNothing(_LoginInfo) Then
                authForLoc = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetHistory, _LoginInfo)
            End If
            SetSearchLocationOptions(authForLoc)
            '[CC431] UPD End 20261007

            'カテゴリー
            ' 大分類ドロップダウンリスト作成
            GetCategoryLData()

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
                Session.Remove(CommonSessionKey.CREAGE_ASSET_HISTORY)
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
            'Dim query As DbSqlQuery(Of AssetHistoryViewModel) = _db.AssetHistoryView.SqlQuery(_SQL, _paramSqlList.ToArray)
            Dim query As DbSqlQuery(Of AssetHistoryViewModel) = _db.AssetHistoryView.SqlQuery(_SQL, _params.ToArray)

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
        Public Function Create(ByVal ViewModel As AssetHistoryViewModel, ByVal OnCreteButton As String) As ActionResult
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
            Session(CommonSessionKey.CREAGE_ASSET_HISTORY) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_ASSET_HISTORY) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            '処理履歴IDが登録済みかチェックする
            Dim AssetHistory As AssetHistoryModel = _db.AssetHistory.Find(_Model.PROCESS_LOG_ID)

            '重複エラーの場合、エラー情報を保持する
            If Not IsNothing(AssetHistory) Then
                ModelState.AddModelError("PROCESS_LOG_ID", CommonErrorMsgMember.DUPLICATION_ID)
                blnReturn = False
            End If

            Return blnReturn

        End Function

        ''' <summary>
        ''' 登録処理をコンテキストに追加します
        ''' </summary>
        Protected Overrides Sub CreateQuery()
            '登録日時・登録社員コード・登録社員名・登録プロセスを登録
            '_Model.INS_DATETIME = CommonRfid.getNowDate
            '_Model.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            '_Model.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelAssetHistory.TITLE)

            _db.AssetHistory.Add(_Model)

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As AssetHistoryModel = db.AssetHistory.Find(_Model.PROCESS_LOG_ID)

            '他端末で更新されているかチェックする
            If _Model.UPD_DATETIME.ToString("yyyy/MM/dd HH:mm:ss") <> model.UPD_DATETIME.ToString("yyyy/MM/dd HH:mm:ss") Then
                'モデルとDBの更新日付が異なっている場合エラー
                ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelAssetHistory.TITLE)

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

            'ログイン情報を取得。
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'IDをキーにモデルを取得する
            Dim model As AssetHistoryModel = db.AssetHistory.Find(ID)

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
        Public Overridable Function Delete(ByVal ViewModel As AssetHistoryViewModel) As ActionResult
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
            Session(CommonSessionKey.CREAGE_ASSET_HISTORY) = _ViewModel

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
            Dim assetHistory As AssetHistoryModel = _db.AssetHistory.Find(_Model.PROCESS_LOG_ID)

            '削除を実行する
            _db.AssetHistory.Remove(assetHistory)

        End Sub
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ViewModel As AssetHistoryViewModel) As ActionResult
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
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Asset_Hist.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Hist.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Hist.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Hist.Key3)
            '20230901 ADD End Shirakawa

            'モデルを設定
            _ViewModel = ViewModel

            Dim AssetHistoryData As List(Of AssetHistoryViewModel)

            '資産処理履歴データ取得
            AssetHistoryData = GetAssetHistoryData()

            '20230901 ADD Start Shirakawa
            '設定マスタから取得できた場合
            '設定マスタから取得できた場合 かつ 上限値(Data2)が存在している場合 かつ 上限を超えていた場合
            If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA2) = False AndAlso WebReadData.DATA2 < AssetHistoryData.Count Then
                '上限を超えていた場合　CSV出力しない

                '上限確認フラグ
                ViewBag.isLimitFlg = True
                '一覧データ取得件数
                ViewBag.cntList = AssetHistoryData.Count
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
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.AssetHistory.KeyDiv, CommonSettingKey.AssetHistory.Key1, CommonSettingKey.AssetHistory.Key2, CommonSettingKey.AssetHistory.Key3, True)
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


            '2023/05/10 Kawamoto UPD START
            'CSVヘッダー取得
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_HISTORY)
            Dim sHeader As String = Nothing

            '設定マスタ（薬品管理機能）の取得
            Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
                                                                      CommonSettingKey.Drugs_Function.Key1,
                                                                      CommonSettingKey.Drugs_Function.Key2,
                                                                      CommonSettingKey.Drugs_Function.Key3, True)
            '薬品機能がOFFの場合
            If DrugsFunc.DATA1 = DrugsFunction.No Then
                'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_HISTORY)
                sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_HISTORY)
                '薬品機能がONの場合
            Else
                'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_HISTORY_DRUG_FUNC)
                sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_HISTORY_DRUG_FUNC)
            End If
            '2023/05/10 Kawamoto UPD END

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As AssetHistoryViewModel In AssetHistoryData
                '処理日時
                Dim times As String = row.PROCESS_TM
                If times.Length > 4 Then
                    times = times.Substring(0, 4)
                End If
                Dim sProcessDt As String = String.Empty
                Dim dtProcessDt As Date? = CommonRfid.ToDate(row.PROCESS_DT.ToString & times, "yyyyMMddHHmm", "yyyyMMdd")
                If dtProcessDt.HasValue Then
                    sProcessDt = CommonRfid.ToStringDateYMDHM(dtProcessDt.Value)
                Else
                    sProcessDt = CommonRfid.ToStringStringYMDHM(Long.Parse(row.PROCESS_DT.ToString & times))
                End If

                '処理区分
                Dim sProcessDiv As String = String.Empty
                Select Case row.PROCESS_DIV
                    Case ProcessDiv.MI
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MI)
                    Case ProcessDiv.CI
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CI)
                    Case ProcessDiv.MU
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MU)
                    Case ProcessDiv.CU
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CU)
                    Case ProcessDiv.MR
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MR)
                    Case ProcessDiv.CR
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CR)
                    Case ProcessDiv.MD
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MD)
                    Case ProcessDiv.T1
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_T1)
                    Case ProcessDiv.T2
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_T2)
                    Case ProcessDiv.BS
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_BS)
                    Case ProcessDiv.BO
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_BO)
                    Case ProcessDiv.BI
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_BI)
                    Case ProcessDiv.BR
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_RETIRE)
                    Case ProcessDiv.CD
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CD)
                    Case ProcessDiv.LC
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_LC)
                    Case ProcessDiv.LP
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_LP)
                    Case ProcessDiv.LI
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_LI)
                    Case ProcessDiv.UI
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_UI)
                    Case ProcessDiv.IC
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_IC)
                End Select

                '処理ツール
                Dim sProcessToolDiv As String = String.Empty
                Select Case row.PROCESS_TOOL_DIV
                    Case ProcessToolDiv.Web
                        sProcessToolDiv = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)
                    'Case ProcessToolDiv.CS
                    '    sProcessToolDiv = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_CS)
                    Case ProcessToolDiv.HT
                        sProcessToolDiv = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)
                End Select


                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(sProcessDt)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.SUB_ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sProcessDiv)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sProcessToolDiv)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK) '2019/12/23 nv miyaji add
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_EMPLOYEE_NAME)
                '--2020/03/06 nv miyaji add start
                If row.LEASE_DT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_DT.Value))    '2020/03/16 nv miyaji
                    'col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToDate(row.LEASE_DT.Value, "yyyyMMdd"))    '2020/03/16 nv miyaji
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                '--2020/03/06 nv miyaji add end
                If row.LEASE_DUE_DT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_DUE_DT.Value))    '2020/03/16 nv miyaji
                    'col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToDate(row.LEASE_DUE_DT.Value, "yyyyMMdd"))    '2020/03/16 nv miyaji
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If

                '2023/03/22 Kawamoto ADD START
                '薬品機能がONの場合
                If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DUE_QTY)
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.WORKER_EMPLOYEE_CD)
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.WORKER_EMPLOYEE_NAME)
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.WORKER_MINUTE)
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.WORKER_OVERVIEW)
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.WORKER_REMARK1)
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.WORKER_REMARK2)
                End If
                '2023/03/22 Kawamoto ADD END

                '2021/10/06 nv Inoue add start
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_NAME)
                '2021/10/06 nv Inoue add end

                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PRODUCT_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_ID)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.UPD_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.UPD_EMPLOYEE_NAME)
                If DrugsFunc.DATA1 = DrugsFunction.No Then
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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetHistory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     AssetHistoryData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetHistory.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetAssetHistoryData() As List(Of AssetHistoryViewModel)
            Dim lstAssetHistory As List(Of AssetHistoryViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstAssetHistory = db.AssetHistoryView.SqlQuery(CreateSQL(), _params.ToArray).ToList

            'データを返却
            Return lstAssetHistory
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
#Region "検索条件のロケーション（原価センタ・場所）の選択肢"
        ''' <summary>
        ''' 検索条件のロケーション（原価センタ・場所）の選択肢を返す（画面の ajax から呼ぶ）
        ''' </summary>
        ''' <param name="selectLCode">会社（大区分コード）</param>
        ''' <param name="level">返す側（M：中区分、S：小区分）</param>
        ''' <param name="reverse">場所から選ぶ（True：場所で原価センタを絞り込む、False：原価センタで場所を絞り込む）</param>
        ''' <param name="codes">絞り込みに使う、反対側の選択中のコード（「,」区切り）</param>
        ''' <returns>選択肢（LOC_CD／LOC_NAME のリスト）</returns>
        Public Function GetSearchLocationOptions(ByVal selectLCode As String, ByVal level As String, ByVal reverse As Boolean, ByVal codes As String) As ActionResult
            'ログインセッションが切れていたらログイン画面に遷移する（選択肢の取得にはログイン者の権限が必要）
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
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetHistory, _LoginInfo)

            Return Json(GetSearchLocationList(level, reverse, CommonRfid.ToStringNvl(codes).Split(","c), selectLCode, authInfo))

        End Function

        ''' <summary>
        ''' 検索条件のロケーション（原価センタ・場所）の選択肢を、場所から選ぶの ON／OFF と選択値に合わせて作る
        ''' </summary>
        ''' <param name="authInfo">権限情報（ログインセッションが切れている場合は Nothing）</param>
        Private Sub SetSearchLocationOptions(authInfo As AuthInfo)
            Dim selLocM As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocM)
            Dim selLocS As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocS)

            Dim lstLocM As New List(Of LocationDropdownModel)
            Dim lstLocS As New List(Of LocationDropdownModel)
            'ログインセッションが切れている場合は空（選択肢の取得にはログイン者の権限が必要）
            If Not IsNothing(_LoginInfo) Then
                lstLocM = GetSearchLocationList(LOC_DIV.LocationM, _ViewModel.SearchLocReverse, selLocS, _ViewModel.SearchLocL, authInfo)
                lstLocS = GetSearchLocationList(LOC_DIV.LocationS, _ViewModel.SearchLocReverse, selLocM, _ViewModel.SearchLocL, authInfo)
            End If

            '一覧画面のロケーションコンボ設定（複数選択のため MultiSelectList）
            ViewBag.modelLocM = New MultiSelectList(lstLocM, "LOC_CD", "LOC_NAME", selLocM)
            ViewBag.modelLocS = New MultiSelectList(lstLocS, "LOC_CD", "LOC_NAME", selLocS)
        End Sub

        ''' <summary>
        ''' 検索条件のロケーションの選択肢を取得する
        ''' 選択肢は、ロケーションマスタにあり、権限があるもの
        ''' ※T_PROCESS_LOG に存在するかは見ない（履歴が増えると応答時間が遅くなるため。2026/10/07 回答）
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
            Dim exTarget As Dao.ToolLocation.LocExistTarget = Dao.ToolLocation.LocExistTarget.Location()
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

            Return Json(lstLocation)
        End Function
#End Region

#Region "中区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 中区分ドロップダウンリストデータ取得
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

            Return Json(lstLocation)

        End Function
#End Region

#Region "小区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCodes As String()) As JsonResult
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            '中区分は複数選択のため、選択された中区分ごとに小区分を取得してマージする
            For Each selectMCode As String In CommonRfid.GetSelectedValues(selectMCodes)
                Dim tbl As DataTable = Dao.ToolLocation.GetLocationSData(selectLCode, selectMCode, _LoginInfo, _AuthInfo)

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

            Return Json(lstLocation)
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
            ViewBag.modelCatL = New MultiSelectList(lst, "CAT_CD", "CAT_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchCatL))
            _ViewModel.catLDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "一覧データ取得用SQLを作成する"
        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL(Optional ByVal isSort As Boolean = True) As String
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '権限情報の取得
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetHistory, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            _params = New List(Of SqlParameter)
            _paramSqlList.Clear()   'CreateInCondition / CreateLikeCondition（BaseController）用

            '一覧表示表用SQL
            If isSort Then
                sbSQL.AppendLine("SELECT DISTINCT")
            Else
                sbSQL.AppendLine("SELECT TOP 1")
            End If
            sbSQL.AppendLine("    PROLOG.PROCESS_LOG_ID ")
            sbSQL.AppendLine("  , PROLOG.PROCESS_DT ")
            sbSQL.AppendLine("  , PROLOG.PROCESS_TM ")
            sbSQL.AppendLine("  , PROLOG.ASSET_NO ")
            sbSQL.AppendLine("  , PROLOG.ASSET_ID ")
            sbSQL.AppendLine("  , PROLOG.PROCESS_TOOL_DIV ")
            sbSQL.AppendLine("  , PROLOG.PROCESS_DIV ")
            sbSQL.AppendLine("  , PROLOG.LEASE_PLACE_CD ")
            sbSQL.AppendLine("  , PROLOG.LEASE_PLACE_NAME ")
            sbSQL.AppendLine("  , PROLOG.REMARK ")    '2019/11/19 nv miyaji add
            sbSQL.AppendLine("  , PROLOG.LEASE_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , PROLOG.LEASE_EMPLOYEE_NAME ")

            '貸出データの時は、処理日を表示する 2020/03/13 Update  Inoue
            'sbSQL.AppendLine("  , LEASE_DT ")   '2020/03/06 nv miyaji add
            sbSQL.AppendLine("  , CASE WHEN PROLOG.PROCESS_DIV = '" & Misc.ProcessDiv.BO & "' THEN PROLOG.PROCESS_DT")
            sbSQL.AppendLine("         ELSE PROLOG.LEASE_DT")
            sbSQL.AppendLine("    END AS LEASE_DT")

            '返却データの時は、処理日を表示する 2020/03/13 Update  Inoue
            'sbSQL.AppendLine("  , LEASE_DUE_DT ")
            sbSQL.AppendLine("  , CASE WHEN PROLOG.PROCESS_DIV = '" & Misc.ProcessDiv.BI & "' THEN PROLOG.PROCESS_DT")
            sbSQL.AppendLine("         ELSE PROLOG.LEASE_DUE_DT")
            sbSQL.AppendLine("    END AS LEASE_DUE_DT")

            '2021/10/05 nv Inoue add start
            sbSQL.AppendLine("  , PROLOG.LEASE_STATUS ")
            sbSQL.AppendLine("  , IsNull(PROLOG.MNG_LOC_L_CD, '') AS MNG_LOC_L_CD ")
            sbSQL.AppendLine("  , IsNull(LOCL.LOC_NAME, '') AS MNG_LOC_L_NAME ")
            '2021/10/05 nv Inoue add end

            sbSQL.AppendLine("  , IsNull(PROLOG.LOC_L_CD, '') AS LOC_L_CD ")
            sbSQL.AppendLine("  , IsNull(PROLOG.LOC_M_CD, '') AS LOC_M_CD ")
            sbSQL.AppendLine("  , IsNull(PROLOG.LOC_S_CD, '') AS LOC_S_CD ")
            sbSQL.AppendLine("  , IsNull(PROLOG.CAT_L_CD, '') AS CAT_L_CD ")
            sbSQL.AppendLine("  , PROLOG.LOC_L_NAME ")
            sbSQL.AppendLine("  , PROLOG.LOC_M_NAME ")
            sbSQL.AppendLine("  , PROLOG.LOC_S_NAME ")
            sbSQL.AppendLine("  , PROLOG.ASSET_NAME ")
            sbSQL.AppendLine("  , PROLOG.MAKER ")
            sbSQL.AppendLine("  , PROLOG.PRODUCT_NO ")
            sbSQL.AppendLine("  , PROLOG.MODEL_NO ")
            sbSQL.AppendLine("  , PROLOG.SUB_ASSET_NO ")
            sbSQL.AppendLine("  , CATL.CAT_NAME AS CAT_NAME ")
            '2023/03/22 Kawamoto ADD START
            sbSQL.AppendLine("  , PROLOG.DUE_QTY ")
            sbSQL.AppendLine("  , PROLOG.WORKER_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , PROLOG.WORKER_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , PROLOG.WORKER_MINUTE ")
            sbSQL.AppendLine("  , PROLOG.WORKER_OVERVIEW ")
            sbSQL.AppendLine("  , PROLOG.WORKER_REMARK1 ")
            sbSQL.AppendLine("  , PROLOG.WORKER_REMARK2 ")
            '2023/03/22 Kawamoto ADD END
            sbSQL.AppendLine("  , PROLOG.REMARK1 ")
            sbSQL.AppendLine("  , PROLOG.REMARK2 ")
            sbSQL.AppendLine("  , PROLOG.REMARK3 ")
            sbSQL.AppendLine("  , PROLOG.REMARK4 ")
            sbSQL.AppendLine("  , PROLOG.REMARK5 ")
            sbSQL.AppendLine("  , PROLOG.REMARK6 ")
            sbSQL.AppendLine("  , PROLOG.REMARK7 ")
            sbSQL.AppendLine("  , PROLOG.REMARK8 ")
            sbSQL.AppendLine("  , PROLOG.REMARK9 ")
            sbSQL.AppendLine("  , PROLOG.REMARK10 ")
            sbSQL.AppendLine("  , PROLOG.TASK_RESULT ")
            sbSQL.AppendLine("  , PROLOG.UPD_DATETIME ")
            sbSQL.AppendLine("  , PROLOG.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , PROLOG.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , PROLOG.UPD_TASK ")
            sbSQL.AppendLine("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  T_PROCESS_LOG PROLOG ")

            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND PROLOG.MNG_LOC_L_CD = LOCL.LOC_L_CD ")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATL ")
            sbSQL.AppendLine("ON CATL.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
            sbSQL.AppendLine("AND PROLOG.CAT_L_CD = CATL.CAT_L_CD ")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine(" (PROLOG.MNG_LOC_L_CD IS NULL OR ")
            sbSQL.AppendLine(" EXISTS(SELECT 1 FROM dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
            sbSQL.AppendLine(" WHERE ULOC.LOC_L_CD = PROLOG.MNG_LOC_L_CD ")
            sbSQL.AppendLine("AND ((ULOC.LOC_M_FLG='" & AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = PROLOG.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & AUTH_LOC_M_FLG.No & "')))) ")

            If Not _LoginInfo Is Nothing Then
                _params.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
                _params.Add(New SqlParameter("@ReadAuthFlg", _AuthInfo.ReadAuthFlg))
                _params.Add(New SqlParameter("@UpdAuthFlg", _AuthInfo.UpdAuthFlg))
                _params.Add(New SqlParameter("@ReadHubAuthFlg", _AuthInfo.ReadHubAuthFlg))
                _params.Add(New SqlParameter("@UpdHubAuthFlg", _AuthInfo.UpdHubAuthFlg))
                _params.Add(New SqlParameter("@UpdAuthOption", _AuthInfo.AuthOption))
            Else
                'デバッグ時、_LoginInfoがNULLの場合があるため
                _params.Add(New SqlParameter("@EmployeeCd", ""))
                _params.Add(New SqlParameter("@ReadAuthFlg", $"{AUTH_FLG.No}"))
                _params.Add(New SqlParameter("@UpdAuthFlg", $"{AUTH_FLG.No}"))
                _params.Add(New SqlParameter("@ReadHubAuthFlg", $"{AUTH_FLG.No}"))
                _params.Add(New SqlParameter("@UpdHubAuthFlg", $"{AUTH_FLG.No}"))
                _params.Add(New SqlParameter("@UpdAuthOption", ""))
            End If

            '検索条件を設定する
            'ロケ大区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                sbSQL.Append("  AND PROLOG.LOC_L_CD = @SearchLocL")
                _params.Add(New SqlParameter($"@SearchLocL", _ViewModel.SearchLocL))
            End If

            'ロケ中区分コード（複数選択）
            sbSQL.Append(CreateInCondition("PROLOG.LOC_M_CD", "SearchLocM", _ViewModel.SearchLocM))

            'ロケ小区分コード（複数選択）
            sbSQL.Append(CreateInCondition("PROLOG.LOC_S_CD", "SearchLocS", _ViewModel.SearchLocS))

            'カテゴリ大分類 SearchCatL（複数選択。SQLパラメータは後で _params へ移す）
            sbSQL.Append(CreateInCondition("PROLOG.CAT_L_CD", "SearchCatL", _ViewModel.SearchCatL))

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
                sbSQL.Append(CreateLikeCondition({$"PROLOG.{kvRemark.Key}"}, $"Search{kvRemark.Key}", CommonRfid.GetMultiInputValues(kvRemark.Value), True))
            Next

            ' 期間(From)
            If Not String.IsNullOrEmpty(_ViewModel.SearchDateFrom) Then
                sbSQL.AppendLine("  AND PROLOG.PROCESS_DT >= @PROCESS_DT_FROM")
                _params.Add(New SqlParameter($"@PROCESS_DT_FROM", _ViewModel.SearchDateFrom.Replace("-", "")))
            End If
            ' 期間(To)
            If Not String.IsNullOrEmpty(_ViewModel.SearchDateTo) Then
                sbSQL.AppendLine("  AND PROLOG.PROCESS_DT <= @PROCESS_DT_TO")
                _params.Add(New SqlParameter($"@PROCESS_DT_TO", _ViewModel.SearchDateTo.Replace("-", "")))
            End If

            Dim sbSQLCheck As New StringBuilder
            '入庫予定
            If _ViewModel.SearchProcessDivLP Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.LP & "'")
            End If
            '登録(ラベル発行)
            If _ViewModel.SearchProcessDivLI Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.LI & "'")
            End If
            '登録(ラベル不要)
            If _ViewModel.SearchProcessDivUI Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.UI & "'")
            End If
            '登録(メンテ)
            If _ViewModel.SearchProcessDivMI Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.MI & "'")
            End If
            '更新(メンテ)
            If _ViewModel.SearchProcessDivMU Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.MU & "'")
            End If
            '更新(取込)
            If _ViewModel.SearchProcessDivCU Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.CU & "'")
            End If
            '除却(メンテ)
            If _ViewModel.SearchProcessDivMR Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.MR & "'")
            End If
            '削除(メンテ)  '2019/12/20 miyaji add
            If _ViewModel.SearchProcessDivMD Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.MD & "'")
            End If
            '削除(取込)  '2019/12/20 miyaji add
            If _ViewModel.SearchProcessDivCD Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.CD & "'")
            End If
            'タグラベル発行＆紐付
            If _ViewModel.SearchProcessDivT1 Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.T1 & "'")
            End If
            'タグ紐付
            If _ViewModel.SearchProcessDivT2 Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.T2 & "'")
            End If
            '除却
            If _ViewModel.SearchProcessDivBR Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.BR & "'")
            End If
            '棚卸確定
            If _ViewModel.SearchProcessDivIC Then
                If sbSQLCheck.Length <> 0 Then sbSQLCheck.Append(",")
                sbSQLCheck.Append("'" & ProcessDiv.IC & "'")
            End If
            'SQL連結
            If sbSQLCheck.Length <> 0 Then
                sbSQL.Append(" AND PROLOG.PROCESS_DIV IN(" & sbSQLCheck.ToString & ") ")
            End If

            '処理ツール
            If Not String.IsNullOrEmpty(_ViewModel.SearchProcessToolDiv) Then
                sbSQL.Append("  AND PROLOG.PROCESS_TOOL_DIV = @PROCESS_TOOL_DIV")
                _params.Add(New SqlParameter($"@PROCESS_TOOL_DIV", _ViewModel.SearchProcessToolDiv))
            End If

            '資産番号（「|」区切りで複数指定。資産番号・子番号のいずれかに前方一致）
            sbSQL.Append(CreateLikeCondition({"PROLOG.ASSET_NO", "PROLOG.SUB_ASSET_NO"}, "ASSET_NO", CommonRfid.GetMultiInputValues(_ViewModel.SearchAssetNo)))

            'CreateInCondition / CreateLikeCondition（BaseController）で設定したSQLパラメータを、本画面のSQLパラメータへ移す
            '（本画面は BaseController の _paramSqlList ではなく _params を使用しているため）
            _params.AddRange(_paramSqlList)
            _paramSqlList.Clear()

            '資産名称
            If Not String.IsNullOrEmpty(_ViewModel.SearchAssetName) Then
                sbSQL.Append("  AND PROLOG.ASSET_NAME Like @ASSET_NAME")
                _params.Add(New SqlParameter($"@ASSET_NAME", "%" & _ViewModel.SearchAssetName & "%"))
            End If

            ''メーカ
            'If Not String.IsNullOrEmpty(_ViewModel.SearchMaker) Then
            '    sbSQL.Append("  AND PROLOG.MAKER Like @MAKER")
            '    _params.Add(New SqlParameter($"@MAKER", "%" & _ViewModel.SearchMaker & "%"))
            'End If

            ''品番
            'If Not String.IsNullOrEmpty(_ViewModel.SearchProductNo) Then
            '    sbSQL.Append("  AND PROLOG.PRODUCT_NO Like @PRODUCT_NO")
            '    _params.Add(New SqlParameter($"@PRODUCT_NO", "%" & _ViewModel.SearchProductNo & "%"))
            'End If

            ''型番
            'If Not String.IsNullOrEmpty(_ViewModel.SearchModelNo) Then
            '    sbSQL.Append("  AND PROLOG.MODEL_NO Like @MODEL_NO")
            '    _params.Add(New SqlParameter($"@MODEL_NO", "%" & _ViewModel.SearchModelNo & "%"))
            'End If

            'ログイン者名
            If Not String.IsNullOrEmpty(_ViewModel.SearchEmployeeNm) Then
                sbSQL.Append("  AND PROLOG.UPD_EMPLOYEE_NAME Like @UPD_EMPLOYEE_NAME")
                _params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", "%" & _ViewModel.SearchEmployeeNm & "%"))
            End If

            '2020/03/15 UPDATE Inoue
            'sbSQL.Append(" ORDER BY PROCESS_DT DESC, PROCESS_TM DESC")
            If isSort Then
                '[CC431] UPD Start 20261015
                ' 仕様変更2：検索条件の並び順で並べる（第3キーは資産番号昇順）
                sbSQL.Append(" ORDER BY " & GetOrderBy(_ViewModel.SearchSortOrder))
                '[CC431] UPD End 20261015
            End If

            Return sbSQL.ToString()
        End Function

        '[CC431] ADD Start 20261015
        ''' <summary>
        ''' 並び順に対応する ORDER BY の中身（キーの並び）を取得する
        ''' </summary>
        ''' <param name="sortOrder">検索条件 並び順(AssetHistorySort)</param>
        ''' <returns>ORDER BY に続けて指定する文字列（想定外の値は処理日付降順）</returns>
        ''' <remarks>備考は空白（NULL・空文字）を後ろにする。原価センタ・場所はコード順。末尾は処理日付降順、資産番号昇順。</remarks>
        Private Function GetOrderBy(ByVal sortOrder As Integer) As String
            Select Case sortOrder
                Case AssetHistorySort.CatLAsc
                    Return "PROLOG.CAT_L_CD,PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.AssetNoAsc
                    Return "PROLOG.ASSET_NO,PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC"
                Case AssetHistorySort.LocMAsc
                    Return "PROLOG.LOC_M_CD,PROLOG.LOC_S_CD,PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.LocSAsc
                    Return "PROLOG.LOC_S_CD,PROLOG.LOC_M_CD,PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.Remark1Asc
                    Return GetBlankLastOrder("PROLOG.REMARK1") & ",PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.Remark2Asc
                    Return GetBlankLastOrder("PROLOG.REMARK2") & ",PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.Remark3Asc
                    Return GetBlankLastOrder("PROLOG.REMARK3") & ",PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.Remark4Asc
                    Return GetBlankLastOrder("PROLOG.REMARK4") & ",PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.Remark5Asc
                    Return GetBlankLastOrder("PROLOG.REMARK5") & ",PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.Remark6Asc
                    Return GetBlankLastOrder("PROLOG.REMARK6") & ",PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.Remark7Asc
                    Return GetBlankLastOrder("PROLOG.REMARK7") & ",PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.Remark8Asc
                    Return GetBlankLastOrder("PROLOG.REMARK8") & ",PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.Remark9Asc
                    Return GetBlankLastOrder("PROLOG.REMARK9") & ",PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case AssetHistorySort.Remark10Asc
                    Return GetBlankLastOrder("PROLOG.REMARK10") & ",PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
                Case Else
                    Return "PROLOG.PROCESS_DT DESC,PROLOG.PROCESS_TM DESC,PROLOG.ASSET_NO"
            End Select
        End Function

        ''' <summary>
        ''' 空白（NULL・空文字）を後ろにする並びを返す。スペースのみは値ありの扱い
        ''' </summary>
        Private Function GetBlankLastOrder(ByVal col As String) As String
            Return "CASE WHEN " & col & " IS NULL OR " & col & " = '' THEN 1 ELSE 0 END," & col
        End Function
        '[CC431] ADD End 20261015
#End Region

#Region "全選択ボタン押下時処理"
        ''' <summary>
        ''' 全選択ボタン押下時処理 2022/7/25 sv sato add
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

#Region "処理区分チェックボックス状態チェック処理"
        ''' <summary>
        ''' 処理区分チェックボックス状態チェック処理
        ''' </summary>
        ''' <param name="ViewModel">ビューモデル</param>
        ''' <returns></returns>
        Private Function ChkSelectAllStatus(ByVal ViewModel As AssetHistoryViewModel, Optional DrugFuncFlg As Boolean = False) As Boolean
            With ViewModel
                '検索条件の処理区分選択状態チェック
                If .SearchProcessDivMI AndAlso
                    .SearchProcessDivCI AndAlso
                    .SearchProcessDivMU AndAlso
                    .SearchProcessDivCU AndAlso
                    .SearchProcessDivMR AndAlso
                    .SearchProcessDivCR AndAlso
                    .SearchProcessDivT1 AndAlso
                    .SearchProcessDivT2 AndAlso
                    .SearchProcessDivBS AndAlso
                    .SearchProcessDivBO AndAlso
                    .SearchProcessDivBI AndAlso
                    .SearchProcessDivBR AndAlso
                    .SearchProcessDivLC AndAlso
                    .SearchProcessDivRI AndAlso
                    .SearchProcessDivRU AndAlso
                    .SearchProcessDivRD AndAlso
                    .SearchProcessDivXI AndAlso
                    .SearchProcessDivXU AndAlso
                    .SearchProcessDivXD AndAlso
                    .SearchProcessDivMD AndAlso
                    .SearchProcessDivCD AndAlso
                    .SearchProcessDivIC Then
                    '処理区分のうち、残量確認以外が選択状態の場合

                    '薬品機能OFFの場合は、全処理区分が選択状態とみなす
                    '薬品機能ONの場合は、残量確認が選択されていれば全選択状態とみなす
                    If DrugFuncFlg = False OrElse (DrugFuncFlg AndAlso .SearchProcessDivDC = True) Then
                        Return True
                    End If
                End If
            End With

            '1つでも選択されていない場合
            Return False
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
                _viewResult.Add(New AssetHistoryViewModel())
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
            _viewResult.ElementAt(0).SearchDateFrom = _ViewModel.SearchDateFrom
            _viewResult.ElementAt(0).SearchDateTo = _ViewModel.SearchDateTo
            _viewResult.ElementAt(0).SearchProcessDivLP = _ViewModel.SearchProcessDivLP
            _viewResult.ElementAt(0).SearchProcessDivLI = _ViewModel.SearchProcessDivLI
            _viewResult.ElementAt(0).SearchProcessDivUI = _ViewModel.SearchProcessDivUI
            _viewResult.ElementAt(0).SearchProcessDivMI = _ViewModel.SearchProcessDivMI
            _viewResult.ElementAt(0).SearchProcessDivMU = _ViewModel.SearchProcessDivMU
            _viewResult.ElementAt(0).SearchProcessDivCU = _ViewModel.SearchProcessDivCU
            _viewResult.ElementAt(0).SearchProcessDivMR = _ViewModel.SearchProcessDivMR
            _viewResult.ElementAt(0).SearchProcessDivMD = _ViewModel.SearchProcessDivMD
            _viewResult.ElementAt(0).SearchProcessDivCD = _ViewModel.SearchProcessDivCD
            _viewResult.ElementAt(0).SearchProcessDivT1 = _ViewModel.SearchProcessDivT1
            _viewResult.ElementAt(0).SearchProcessDivT2 = _ViewModel.SearchProcessDivT2
            _viewResult.ElementAt(0).SearchProcessDivBR = _ViewModel.SearchProcessDivBR
            _viewResult.ElementAt(0).SearchProcessDivIC = _ViewModel.SearchProcessDivIC
            '_viewResult.ElementAt(0).SearchProcessDivLC = _ViewModel.SearchProcessDivLC
            _viewResult.ElementAt(0).SearchProcessToolDiv = _ViewModel.SearchProcessToolDiv
            _viewResult.ElementAt(0).SearchAssetNo = _ViewModel.SearchAssetNo
            _viewResult.ElementAt(0).SearchAssetName = _ViewModel.SearchAssetName
            _viewResult.ElementAt(0).SearchMaker = _ViewModel.SearchMaker
            _viewResult.ElementAt(0).SearchProductNo = _ViewModel.SearchProductNo
            _viewResult.ElementAt(0).SearchModelNo = _ViewModel.SearchModelNo
            _viewResult.ElementAt(0).SearchEmployeeNm = _ViewModel.SearchEmployeeNm
            _viewResult.ElementAt(0).DetailOpenFlg = _ViewModel.DetailOpenFlg
            '[CC431] ADD Start 20261015
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
            '[CC431] ADD End 20261015
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
            '[CC431] ADD Start 20261015
            _viewResult.ElementAt(0).SearchSortOrder = _ViewModel.SearchSortOrder
            '[CC431] ADD End 20261015
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            If String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_HISTORY, COLSET_LOC.TEMPLATE)
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.ASSET_HISTORY, COLSET_LOC.TEMPLATE).FIX_COL_CNT
            ElseIf ToolColSet.GetGridInfo(_db, DispId.ASSET_HISTORY, _ViewModel.SearchLocL).Count > 0 Then
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_HISTORY, _ViewModel.SearchLocL)
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.ASSET_HISTORY, _ViewModel.SearchLocL).FIX_COL_CNT
            Else
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_HISTORY, COLSET_LOC.TEMPLATE)
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.ASSET_HISTORY, COLSET_LOC.TEMPLATE).FIX_COL_CNT
            End If
            _viewResult.ElementAt(0).IsRestoreOwnSearchConditions = _ViewModel.IsRestoreOwnSearchConditions
            _viewResult.ElementAt(0).FirstSearch = _ViewModel.FirstSearch

        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As AssetHistoryViewModel)
            _Model.PROCESS_LOG_ID = model.PROCESS_LOG_ID
            _Model.PROCESS_DT = model.PROCESS_DT
            _Model.PROCESS_TM = model.PROCESS_TM
            _Model.ASSET_NO = model.ASSET_NO
            _Model.ASSET_ID = model.ASSET_ID
            _Model.PROCESS_TOOL_DIV = model.PROCESS_DT
            _Model.PROCESS_DIV = model.PROCESS_DT
            _Model.LEASE_PLACE_CD = model.PROCESS_DT
            _Model.LEASE_PLACE_NAME = model.PROCESS_DT
            _Model.REMARK = model.REMARK
            _Model.LEASE_EMPLOYEE_CD = model.PROCESS_DT
            _Model.LEASE_EMPLOYEE_NAME = model.PROCESS_DT
            _Model.LEASE_DT = model.PROCESS_DT    '2019/11/19 nv miyaji add 使ってないみたいだけど一応
            _Model.LEASE_DUE_DT = model.PROCESS_DT
            _Model.LEASE_STATUS = model.LEASE_STATUS        '2021/10/05 nv Inoue add
            _Model.MNG_LOC_L_CD = model.MNG_LOC_L_CD        '2021/10/05 nv Inoue add
            _Model.LOC_L_CD = model.LOC_L_CD
            _Model.LOC_M_CD = model.LOC_M_CD
            _Model.LOC_S_CD = model.LOC_S_CD
            _Model.LOC_L_NAME = model.LOC_L_NAME
            _Model.LOC_M_NAME = model.LOC_M_NAME
            _Model.LOC_S_NAME = model.LOC_S_NAME
            _Model.ASSET_NAME = model.ASSET_NAME
            _Model.MAKER = model.MAKER
            _Model.PRODUCT_NO = model.PRODUCT_NO
            _Model.MODEL_NO = model.MODEL_NO
            '2023/03/22 Kawamoto ADD START
            _Model.DUE_QTY = model.DUE_QTY
            _Model.WORKER_EMPLOYEE_CD = model.WORKER_EMPLOYEE_CD
            _Model.WORKER_EMPLOYEE_NAME = model.WORKER_EMPLOYEE_NAME
            _Model.WORKER_MINUTE = model.WORKER_MINUTE
            _Model.WORKER_OVERVIEW = model.WORKER_OVERVIEW
            _Model.WORKER_REMARK1 = model.WORKER_REMARK1
            _Model.WORKER_REMARK2 = model.WORKER_REMARK2
            '2023/03/22 Kawamoto ADD START
            _Model.CAT_L_CD = model.CAT_L_CD
            _Model.CAT_NAME = model.CAT_NAME
            '_Model.SUB_ASSET_NO = model.SUB_ASSET_NO
            _Model.TASK_RESULT = model.TASK_RESULT
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub
#End Region

    End Class
End Namespace