Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports System.Web.WebPages
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' 履歴照会(資産)画面表示用のクラス
    ''' </summary>
    Public Class AssetDetailController
        Inherits PageBaseController

#Region "クラス定数"

#End Region

#Region "クラス変数"
        Private _Model As AssetDetailModel = New AssetDetailModel                                 'AssetDetailモデル
        Private _ViewModel As AssetDetailViewModel = New AssetDetailViewModel                     'AssetDetail一覧表示用モデル
        Protected _viewResult As List(Of AssetDetailViewModel) = New List(Of AssetDetailViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _params As List(Of SqlParameter)                'SQLパラメータ
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
        Public Function Index(ByVal ViewModel As AssetDetailViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_ASSET_DETAIL)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As AssetDetailViewModel = Session(CommonSessionKey.CREAGE_ASSET_DETAIL)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'メッセージ一覧取得              2021/10/03 ADD nv Inoue
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
                ViewBag.MstAssetData = New List(Of MstAssetModel)
                CopyModelData()
                Return View(_viewResult)
            Else
                '一覧表示処理
                MyBase.BaseIndex(_ViewModel)

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            '2023/05/10 Kawamoto ADD START
            '設定マスタ（薬品管理機能）の取得
            Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
                                                                      CommonSettingKey.Drugs_Function.Key1,
                                                                      CommonSettingKey.Drugs_Function.Key2,
                                                                      CommonSettingKey.Drugs_Function.Key3, True)
            '薬品機能がONの場合
            If DrugsFunc.DATA1 = DrugsFunction.Yes Then
                ViewBag.DrugsFunc = True
            End If
            '2023/05/10 Kawamoto ADD END

            '資産マスタデータ取得
            GetMstAssetData()

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
                Session.Remove(CommonSessionKey.CREAGE_ASSET_DETAIL)
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
            'Dim query As DbSqlQuery(Of AssetDetailViewModel) = _db.AssetDetailView.SqlQuery(_SQL, _paramSqlList.ToArray)
            Dim query As DbSqlQuery(Of AssetDetailViewModel) = _db.AssetDetailView.SqlQuery(_SQL, _params.ToArray)

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
        Public Function Create(ByVal ViewModel As AssetDetailViewModel, ByVal OnCreteButton As String) As ActionResult
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
            Session(CommonSessionKey.CREAGE_ASSET_DETAIL) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_ASSET_DETAIL) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            '処理履歴IDが登録済みかチェックする
            Dim AssetDetail As AssetDetailModel = _db.AssetDetail.Find(_Model.PROCESS_LOG_ID)

            '重複エラーの場合、エラー情報を保持する
            If Not IsNothing(AssetDetail) Then
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

            _db.AssetDetail.Add(_Model)

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As AssetDetailModel = db.AssetDetail.Find(_Model.PROCESS_LOG_ID)

            '他端末で更新されているかチェックする
            If _Model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") <> model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") Then
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

            'IDをキーにモデルを取得する
            Dim model As AssetDetailModel = db.AssetDetail.Find(ID)

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
        Public Overridable Function Delete(ByVal ViewModel As AssetDetailViewModel) As ActionResult
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
            Session(CommonSessionKey.CREAGE_ASSET_DETAIL) = _ViewModel

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
            '特に無し
        End Sub
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ViewModel As AssetDetailViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            Dim AssetDetailData As List(Of AssetDetailViewModel)

            '資産処理履歴データ取得
            AssetDetailData = GetAssetDetailData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      '設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.AssetDetail.KeyDiv, CommonSettingKey.AssetDetail.Key1, CommonSettingKey.AssetDetail.Key2, CommonSettingKey.AssetDetail.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_DETAIL)
            Dim sHeader As String = Nothing

            '設定マスタ（薬品管理機能）の取得
            Dim DrugsFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
                                                                      CommonSettingKey.Drugs_Function.Key1,
                                                                      CommonSettingKey.Drugs_Function.Key2,
                                                                      CommonSettingKey.Drugs_Function.Key3, True)
            '薬品機能がOFFの場合
            If DrugsFunc.DATA1 = DrugsFunction.No Then
                'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_DETAIL)
                sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_DETAIL)
                '薬品機能がONの場合
            Else
                'sHeader = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_DETAIL_DRUG_FUNC)
                sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_DETAIL_DRUG_FUNC)
            End If
            '2023/05/10 Kawamoto UPD END

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As AssetDetailViewModel In AssetDetailData
                '資産状態
                Dim sAssetStatus As String = String.Empty
                Select Case row.ASSET_STATUS
                    Case ASSET_STATUS.Preparation
                        sAssetStatus = Me._Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_JUNBI)
                    Case ASSET_STATUS.Storage
                        sAssetStatus = Me._Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_HOKAN)
                    Case ASSET_STATUS.Lending
                        sAssetStatus = Me._Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_LEASE)
                    Case ASSET_STATUS.Removed
                        sAssetStatus = Me._Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_RETIRE)
                End Select

                '資産ID状態
                Dim sEpcEncStatus As String = String.Empty
                Select Case row.EPC_ENC_STATUS
                    Case EpcEncStatus.Mi
                        sEpcEncStatus = Me._Message.getMessage(CommonLabel.VALUE_MI)
                    Case EpcEncStatus.Sumi
                        sEpcEncStatus = Me._Message.getMessage(CommonLabel.VALUE_SUMI)
                End Select

                'タグ紐付状態
                Dim sTagLinkStatus As String = String.Empty
                Select Case row.TAG_LINK_STATUS
                    Case EpcEncStatus.Mi
                        sTagLinkStatus = Me._Message.getMessage(CommonLabel.VALUE_MI)
                    Case EpcEncStatus.Sumi
                        sTagLinkStatus = Me._Message.getMessage(CommonLabel.VALUE_SUMI)
                End Select

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
                    Case ProcessDiv.LC
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_LC)
                    Case ProcessDiv.CD
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CD)
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
                    Case ProcessToolDiv.CS
                        sProcessToolDiv = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_CS)
                    Case ProcessToolDiv.HT
                        sProcessToolDiv = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)
                End Select

                '2021/10/09 nv Inoue add start
                '貸出状態
                Dim sLeaseStatus As String = String.Empty
                Select Case row.LEASE_STATUS
                    Case LeaseStatusAsset.Fuka
                        sLeaseStatus = Me._Message.getMessage(CommonLabel.VALUE_FUKA)
                    Case LeaseStatusAsset.Kanou
                        sLeaseStatus = Me._Message.getMessage(CommonLabel.VALUE_KANOU)
                End Select
                '2021/10/09 nv Inoue add end

                col = ""
                '--資産マスタ
                col += SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.SUB_ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sAssetStatus)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sEpcEncStatus)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sTagLinkStatus)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sLeaseStatus)     '2021/10/09 nv Inoue add
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PRODUCT_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_CNT)
                '--資産処理履歴
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sProcessDt)
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

                '2021/10/09 nv Inoue add start
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_NAME)
                '2021/10/09 nv Inoue add end

                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_NAME)
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
                                                     AssetDetailData.Count.ToString
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
        Private Function GetAssetDetailData() As List(Of AssetDetailViewModel)
            Dim lstAssetDetail As List(Of AssetDetailViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstAssetDetail = db.AssetDetailView.SqlQuery(CreateSQL(), _params.ToArray).ToList

            'データを返却
            Return lstAssetDetail
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

#Region "資産マスタデータデータ取得"
        ''' <summary>
        ''' 資産マスタデータデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetMstAssetData() As JsonResult
            Dim tbl As DataTable = Dao.ToolAsset.GetMstAsset(Session(CommonSessionKey.CREAGE_ASSET_DETAIL_KEY))
            Dim lst As New List(Of MstAssetModel)

            If tbl Is Nothing OrElse tbl.Rows.Count = 0 Then
                Return Json(Nothing)
            End If

            Dim row As DataRow = tbl.Rows(0)
            lst.Add(
                    New MstAssetModel With {
                    .ASSET_NO = row("ASSET_NO").ToString,
                    .SUB_ASSET_NO = row("SUB_ASSET_NO").ToString,
                    .ASSET_ID = row("ASSET_ID").ToString,
                    .ASSET_DIV = row("ASSET_DIV").ToString,
                    .ASSET_NAME = row("ASSET_NAME").ToString,
                    .CAT_L_CD = row("CAT_L_CD").ToString.Trim,
                    .CAT_M_CD = row("CAT_M_CD").ToString.Trim,
                    .CAT_S_CD = row("CAT_S_CD").ToString.Trim,
                    .MAKER = row("MAKER").ToString,
                    .PRODUCT_NO = row("PRODUCT_NO").ToString,
                    .MODEL_NO = row("MODEL_NO").ToString,
                    .LEASE_NO = row("LEASE_NO").ToString,
                    .LEASE_CNT = Integer.Parse(row("LEASE_CNT").ToString),
                    .ASSET_STATUS = Byte.Parse(row("ASSET_STATUS").ToString),
                    .EPC_ENC_STATUS = Byte.Parse(row("EPC_ENC_STATUS").ToString),
                    .TAG_LINK_STATUS = Byte.Parse(row("TAG_LINK_STATUS").ToString),
                    .LEASE_STATUS = Byte.Parse(row("LEASE_STATUS").ToString),   '2021/10/06 nv miyaji add
                    .MNG_LOC_L_CD = row("MNG_LOC_L_CD").ToString.Trim,   '2021/10/06 nv miyaji add
                    .MNG_LOC_L_NAME = row("MNG_LOC_L_NAME").ToString,
                    .LEASE_PLACE_CD = row("LEASE_PLACE_CD").ToString,
                    .LEASE_PLACE_NAME = row("LEASE_PLACE_NAME").ToString,
                    .REMARK = row("REMARK").ToString,
                    .LEASE_EMPLOYEE_CD = row("LEASE_EMPLOYEE_CD").ToString,
                    .LEASE_EMPLOYEE_NAME = row("LEASE_EMPLOYEE_NAME").ToString,
                    .CAT_L_NAME = row("CAT_L_NAME").ToString,
                    .CAT_M_NAME = row("CAT_M_NAME").ToString,
                    .CAT_S_NAME = row("CAT_S_NAME").ToString
                    }
                )

            ViewBag.MstAssetData = lst
            _ViewModel.mstAssetData = lst

            Return Json(lst)
        End Function
#End Region

#Region "一覧データ取得用SQLを作成する"
        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String

            '2021/10/02 ADD nv Inoue start
            If IsNothing(Me._Message) Then
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            End If
            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)
            '2021/10/02 ADD nv Inoue end

            Dim sbSQL As StringBuilder = New StringBuilder
            _params = New List(Of SqlParameter)

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  PLOG.PROCESS_LOG_ID ")
            sbSQL.AppendLine("  , PLOG.PROCESS_DT ")
            sbSQL.AppendLine("  , PLOG.PROCESS_TM ")
            sbSQL.AppendLine("  , PLOG.ASSET_NO ")
            sbSQL.AppendLine("  , PLOG.SUB_ASSET_NO ")
            sbSQL.AppendLine("  , PLOG.ASSET_ID ")
            sbSQL.AppendLine("  , PLOG.PROCESS_TOOL_DIV ")
            sbSQL.AppendLine("  , PLOG.PROCESS_DIV ")
            sbSQL.AppendLine("  , PLOG.LEASE_PLACE_CD ")
            sbSQL.AppendLine("  , PLOG.LEASE_PLACE_NAME ")
            sbSQL.AppendLine("  , PLOG.REMARK ")    '2019/11/19 nv miyaji add
            sbSQL.AppendLine("  , PLOG.LEASE_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , PLOG.LEASE_EMPLOYEE_NAME ")


            '貸出データの時は、処理日を表示する 2020/03/13 Update  Inoue
            'sbSQL.Append("  , PLOG.LEASE_DT ")   '2020/03/06 nv miyaji add
            sbSQL.AppendLine("  , CASE WHEN PLOG.PROCESS_DIV = '" & Misc.ProcessDiv.BO & "' THEN PLOG.PROCESS_DT")
            sbSQL.AppendLine("         ELSE PLOG.LEASE_DT")
            sbSQL.AppendLine("    END AS LEASE_DT")

            '返却データの時は、処理日を表示する 2020/03/13 Update  Inoue
            'sbSQL.Append("  , PLOG.LEASE_DUE_DT ")
            sbSQL.AppendLine("  , CASE WHEN PLOG.PROCESS_DIV = '" & Misc.ProcessDiv.BI & "' THEN PLOG.PROCESS_DT")
            sbSQL.AppendLine("         ELSE PLOG.LEASE_DUE_DT")
            sbSQL.AppendLine("    END AS LEASE_DUE_DT")

            '2021/10/06 nv miyaji add start
            sbSQL.AppendLine("  , PLOG.LEASE_STATUS ")
            sbSQL.AppendLine("  , IsNull(PLOG.MNG_LOC_L_CD, '') AS MNG_LOC_L_CD ")
            sbSQL.AppendLine("  , IsNull(LOCL.LOC_NAME, '') AS MNG_LOC_L_NAME ")
            '2021/10/06 nv miyaji add end

            sbSQL.AppendLine("  , IsNull(PLOG.LOC_L_CD, '') AS LOC_L_CD")
            sbSQL.AppendLine("  , IsNull(PLOG.LOC_M_CD, '') AS LOC_M_CD")
            sbSQL.AppendLine("  , IsNull(PLOG.LOC_S_CD, '') AS LOC_S_CD")
            sbSQL.AppendLine("  , PLOG.LOC_L_NAME ")
            sbSQL.AppendLine("  , PLOG.LOC_M_NAME ")
            sbSQL.AppendLine("  , PLOG.LOC_S_NAME ")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("  , ASSET.MAKER ")
            sbSQL.AppendLine("  , ASSET.PRODUCT_NO ")
            sbSQL.AppendLine("  , ASSET.MODEL_NO ")
            '2023/03/22 Kawamoto ADD START
            sbSQL.AppendLine("  , PLOG.DUE_QTY ")
            sbSQL.AppendLine("  , PLOG.WORKER_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , PLOG.WORKER_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , PLOG.WORKER_MINUTE ")
            sbSQL.AppendLine("  , PLOG.WORKER_OVERVIEW ")
            sbSQL.AppendLine("  , PLOG.WORKER_REMARK1 ")
            sbSQL.AppendLine("  , PLOG.WORKER_REMARK2 ")
            '2023/03/22 Kawamoto ADD END
            sbSQL.AppendLine("  , PLOG.REMARK1 ")
            sbSQL.AppendLine("  , PLOG.REMARK2 ")
            sbSQL.AppendLine("  , PLOG.REMARK3 ")
            sbSQL.AppendLine("  , PLOG.REMARK4 ")
            sbSQL.AppendLine("  , PLOG.REMARK5 ")
            sbSQL.AppendLine("  , PLOG.REMARK6 ")
            sbSQL.AppendLine("  , PLOG.REMARK7 ")
            sbSQL.AppendLine("  , PLOG.REMARK8 ")
            sbSQL.AppendLine("  , PLOG.REMARK9 ")
            sbSQL.AppendLine("  , PLOG.REMARK10 ")
            sbSQL.AppendLine("  , PLOG.TASK_RESULT ")
            sbSQL.AppendLine("  , PLOG.UPD_DATETIME ")
            sbSQL.AppendLine("  , PLOG.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , PLOG.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , PLOG.UPD_TASK ")
            sbSQL.AppendLine("  , ASSET.ASSET_STATUS ")
            sbSQL.AppendLine("  , ASSET.EPC_ENC_STATUS ")
            sbSQL.AppendLine("  , ASSET.TAG_LINK_STATUS ")
            sbSQL.AppendLine("  , IsNull(ASSET.CAT_L_CD, '') AS CAT_L_CD ")
            sbSQL.AppendLine("  , IsNull(ASSET.CAT_M_CD, '') AS CAT_M_CD ")
            sbSQL.AppendLine("  , IsNull(ASSET.CAT_S_CD, '') AS CAT_S_CD ")
            sbSQL.AppendLine("  , CAT_L.CAT_NAME AS CAT_L_NAME ")
            sbSQL.AppendLine("  , CAT_M.CAT_NAME AS CAT_M_NAME ")
            sbSQL.AppendLine("  , CAT_S.CAT_NAME AS CAT_S_NAME ")
            sbSQL.AppendLine("  , ASSET.LEASE_CNT ")
            sbSQL.AppendLine("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  T_PROCESS_LOG PLOG ")
            '2021/10/02 ADD nv Inoue start
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
            sbSQL.AppendLine("  ON AUTH.LOC_L_CD = PLOG.MNG_LOC_L_CD ")
            _params.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
            _params.Add(New SqlParameter("@ReadAuthFlg", AuthInfo.ReadAuthFlg))
            _params.Add(New SqlParameter("@UpdAuthFlg", AuthInfo.UpdAuthFlg))
            _params.Add(New SqlParameter("@ReadHubAuthFlg", AuthInfo.ReadHubAuthFlg))
            _params.Add(New SqlParameter("@UpdHubAuthFlg", AuthInfo.UpdHubAuthFlg))
            _params.Add(New SqlParameter("@UpdAuthType", Misc.UpdAuthType.UPD_AUTH))
            '2021/10/02 ADD nv Inoue end

            sbSQL.AppendLine("LEFT JOIN M_ASSET ASSET")
            sbSQL.AppendLine("  ON ASSET.ASSET_NO = PLOG.ASSET_NO ")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_L")
            sbSQL.AppendLine("  ON  CAT_L.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
            sbSQL.AppendLine("  AND CAT_L.CAT_L_CD = ASSET.CAT_L_CD ")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_M")
            sbSQL.AppendLine("  ON  CAT_M.CAT_DIV = '" + CAT_DIV.CategoryM + "'")
            sbSQL.AppendLine("  AND CAT_M.CAT_L_CD = ASSET.CAT_L_CD ")
            sbSQL.AppendLine("  AND CAT_M.CAT_M_CD = ASSET.CAT_M_CD ")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_S")
            sbSQL.AppendLine("  ON  CAT_S.CAT_DIV = '" + CAT_DIV.CategoryS + "'")
            sbSQL.AppendLine("  AND CAT_S.CAT_L_CD = ASSET.CAT_L_CD ")
            sbSQL.AppendLine("  AND CAT_S.CAT_M_CD = ASSET.CAT_M_CD ")
            sbSQL.AppendLine("  AND CAT_S.CAT_S_CD = ASSET.CAT_S_CD ")
            '2021/10/06 nv miyaji add start
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND PLOG.MNG_LOC_L_CD = LOCL.LOC_L_CD")
            '2021/10/06 nv miyaji add end
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '検索条件を設定する
            Dim selectAssetNo As String = Session(CommonSessionKey.CREAGE_ASSET_DETAIL_KEY)
            '資産番号
            If Not String.IsNullOrEmpty(selectAssetNo) Then
                sbSQL.AppendLine("  AND PLOG.ASSET_NO = @ASSET_NO")
                _params.Add(New SqlParameter($"@ASSET_NO", selectAssetNo))
            End If

            sbSQL.AppendLine(" ORDER BY PROCESS_DT DESC, PROCESS_TM DESC")

            Return sbSQL.ToString()
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
                _viewResult.Add(New AssetDetailViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            If String.IsNullOrEmpty(_ViewModel.LOC_L_CD) Then
                _viewResult.ElementAt(0).GridInfoTop = ToolColSet.GetGridInfo(_db, DispId.ASSET_DETAIL_TOP, COLSET_LOC.TEMPLATE)
                _viewResult.ElementAt(0).FixColCntTop = _db.ColSet.Find(DispId.ASSET_DETAIL_TOP, COLSET_LOC.TEMPLATE).FIX_COL_CNT
                _viewResult.ElementAt(0).GridInfoBottom = ToolColSet.GetGridInfo(_db, DispId.ASSET_DETAIL_BOTTOM, COLSET_LOC.TEMPLATE)
                _viewResult.ElementAt(0).FixColCntBottom = _db.ColSet.Find(DispId.ASSET_DETAIL_BOTTOM, COLSET_LOC.TEMPLATE).FIX_COL_CNT
            Else
                If ToolColSet.GetGridInfo(_db, DispId.ASSET_DETAIL_TOP, _ViewModel.LOC_L_CD).Count > 0 Then
                    _viewResult.ElementAt(0).GridInfoTop = ToolColSet.GetGridInfo(_db, DispId.ASSET_DETAIL_TOP, _ViewModel.LOC_L_CD)
                    _viewResult.ElementAt(0).FixColCntTop = _db.ColSet.Find(DispId.ASSET_DETAIL_TOP, _ViewModel.LOC_L_CD).FIX_COL_CNT
                Else
                    _viewResult.ElementAt(0).GridInfoTop = ToolColSet.GetGridInfo(_db, DispId.ASSET_DETAIL_TOP, COLSET_LOC.TEMPLATE)
                    _viewResult.ElementAt(0).FixColCntTop = _db.ColSet.Find(DispId.ASSET_DETAIL_TOP, COLSET_LOC.TEMPLATE).FIX_COL_CNT
                End If
                If ToolColSet.GetGridInfo(_db, DispId.ASSET_DETAIL_BOTTOM, _ViewModel.LOC_L_CD).Count > 0 Then
                    _viewResult.ElementAt(0).GridInfoBottom = ToolColSet.GetGridInfo(_db, DispId.ASSET_DETAIL_BOTTOM, _ViewModel.LOC_L_CD)
                    _viewResult.ElementAt(0).FixColCntBottom = _db.ColSet.Find(DispId.ASSET_DETAIL_BOTTOM, _ViewModel.LOC_L_CD).FIX_COL_CNT
                Else
                    _viewResult.ElementAt(0).GridInfoBottom = ToolColSet.GetGridInfo(_db, DispId.ASSET_DETAIL_BOTTOM, COLSET_LOC.TEMPLATE)
                    _viewResult.ElementAt(0).FixColCntBottom = _db.ColSet.Find(DispId.ASSET_DETAIL_BOTTOM, COLSET_LOC.TEMPLATE).FIX_COL_CNT
                End If
            End If

        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As AssetDetailViewModel)
            _Model.PROCESS_LOG_ID = model.PROCESS_LOG_ID
            _Model.PROCESS_DT = model.PROCESS_DT
            _Model.PROCESS_TM = model.PROCESS_TM
            _Model.ASSET_NO = model.ASSET_NO
            _Model.SUB_ASSET_NO = model.SUB_ASSET_NO
            _Model.ASSET_ID = model.ASSET_ID
            _Model.PROCESS_TOOL_DIV = model.PROCESS_DT
            _Model.PROCESS_DIV = model.PROCESS_DT
            _Model.LEASE_PLACE_CD = model.PROCESS_DT
            _Model.LEASE_PLACE_NAME = model.PROCESS_DT
            _Model.REMARK = model.REMARK    '2019/11/19 nv miyaji add
            _Model.LEASE_EMPLOYEE_CD = model.PROCESS_DT
            _Model.LEASE_EMPLOYEE_NAME = model.PROCESS_DT
            _Model.LEASE_DT = model.PROCESS_DT    '2019/11/19 nv miyaji add 使ってないみたいだけど一応
            _Model.LEASE_DUE_DT = model.PROCESS_DT
            _Model.LEASE_STATUS = model.LEASE_STATUS        '2021/10/06 nv miyaji add
            _Model.MNG_LOC_L_CD = model.MNG_LOC_L_CD        '2021/10/06 nv miyaji add
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
            _Model.REMARK1 = model.REMARK1
            _Model.REMARK2 = model.REMARK2
            _Model.REMARK3 = model.REMARK3
            _Model.REMARK4 = model.REMARK4
            _Model.REMARK5 = model.REMARK5
            _Model.REMARK6 = model.REMARK6
            _Model.REMARK7 = model.REMARK7
            _Model.REMARK8 = model.REMARK8
            _Model.REMARK9 = model.REMARK9
            _Model.REMARK10 = model.REMARK10
            _Model.TASK_RESULT = model.TASK_RESULT
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub
#End Region

    End Class
End Namespace