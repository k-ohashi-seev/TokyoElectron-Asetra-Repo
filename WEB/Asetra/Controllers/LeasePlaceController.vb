Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' 貸出先マスタメンテナンス画面表示用のクラス
    ''' </summary>
    Public Class LeasePlaceController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)


#Region "クラス定数"
        ''' <summary>
        ''' データ取込画面　結果表示用クラス
        ''' </summary>
        Private Class CsvLeasePlace
            Public Enum ColIdx
                ''' <summary>貸出先コード</summary>
                Cd = 0
                ''' <summary>貸出先名</summary>
                Name = 1
                ''' <summary>電話番号</summary>
                PhoneNumber = 2
                ''' <summary>郵便番号</summary>
                ZipCode = 3
                ''' <summary>住所</summary>
                Address = 4
                ''' <summary>二次貸出先</summary>
                SecLeasePlace = 5
            End Enum
        End Class
#End Region

#Region "クラス変数"
        Private _Model As LeasePlaceModel = New LeasePlaceModel                                 'LeasePlaceモデル
        Private _ViewModel As LeasePlaceViewModel = New LeasePlaceViewModel                     'LeasePlace一覧表示用モデル
        Protected _viewResult As List(Of LeasePlaceViewModel) = New List(Of LeasePlaceViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
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
        Public Function Index(ByVal ViewModel As LeasePlaceViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_LEASE_PLACE)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As LeasePlaceViewModel = Session(CommonSessionKey.CREAGE_LEASE_PLACE)
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

                '--2021/09/27 nv miyaji add start
                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '権限情報の取得
                Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LeasePlace, _LoginInfo)
                If AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                Else
                    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                End If
                '--2021/09/27 nv miyaji add end
            End If

            '直近登録情報の設定
            Dim tbl As DataTable = Dao.ToolCommon.GetMstInsInfo("M_LEASE_PLACE")
            If tbl IsNot Nothing AndAlso tbl.Rows.Count <> 0 Then
                ViewBag.LastInsDateTime = CommonRfid.ToStringDateYMDHM(Date.Parse(tbl.Rows(0).Item("INS_DATETIME").ToString))
                ViewBag.LastInsEmployeeNm = tbl.Rows(0).Item("INS_EMPLOYEE_NAME").ToString
            End If

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.LEASE_PLACE)
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
                Session.Remove(CommonSessionKey.CREAGE_LEASE_PLACE)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.Append("SELECT ")
            sbSQL.Append("  LEASE_PLACE_CD ")
            sbSQL.Append("  , LEASE_PLACE_NAME ")
            sbSQL.Append("  , ZIP_CODE")    '2021/08/26 nv miyaji add
            sbSQL.Append("  , ADDRESS ")    '2021/08/26 nv miyaji add
            sbSQL.Append("  , PHONE_NUMBER ")    '2021/08/26 nv miyaji add
            sbSQL.Append("  , SEC_LEASE_PLACE_FLG") '2023/03/22 Kawamoto ADD
            sbSQL.Append("  , INS_DATETIME ")
            sbSQL.Append("  , INS_EMPLOYEE_CD ")
            sbSQL.Append("  , INS_EMPLOYEE_NAME ")
            sbSQL.Append("  , UPD_DATETIME ")
            sbSQL.Append("  , UPD_EMPLOYEE_CD ")
            sbSQL.Append("  , UPD_EMPLOYEE_NAME ")
            sbSQL.Append("  , UPD_TASK ")
            sbSQL.Append("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_LEASE_PLACE ")
            sbSQL.Append("WHERE ")
            sbSQL.Append("  1 = 1 ")

            '検索条件を設定する
            '貸出先コード
            If Not IsNothing(_ViewModel.SearchCode) AndAlso _ViewModel.SearchCode <> "" Then
                sbSQL.Append("  AND LEASE_PLACE_CD Like @leasePlaceCd")
                addSqlParam("leasePlaceCd", $"{_ViewModel.SearchCode}%")
            End If

            '貸出先名
            If Not IsNothing(_ViewModel.SearchName) AndAlso _ViewModel.SearchName <> "" Then
                sbSQL.Append("  AND LEASE_PLACE_NAME Like @name")
                addSqlParam("name", $"%{_ViewModel.SearchName}%")
            End If

            sbSQL.Append(" ORDER BY LEASE_PLACE_CD")

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of LeasePlaceViewModel) = _db.LeasePlaceView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function Create(ByVal ViewModel As LeasePlaceViewModel, ByVal OnCreteButton As String) As ActionResult
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

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLeasePlace.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LEASE_PLACE_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LEASE_PLACE_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLeasePlace.TITLE))

                    _ViewModel.States = StatusType.SuccessCreate
                Else
                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLeasePlace.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LEASE_PLACE_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LEASE_PLACE_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLeasePlace.TITLE))

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
            Session(CommonSessionKey.CREAGE_LEASE_PLACE) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_LEASE_PLACE) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            '貸出先コードが登録済みかチェックする
            Dim LeasePlace As LeasePlaceModel = _db.LeasePlace.Find(_Model.LEASE_PLACE_CD)

            '重複エラーの場合、エラー情報を保持する
            If Not IsNothing(LeasePlace) Then
                ModelState.AddModelError("LEASE_PLACE_CD", CommonLabel.MSG_ERR_DATAJUUFUKU)
                blnReturn = False
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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelLeasePlace.TITLE)

            _db.LeasePlace.Add(_Model)

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As LeasePlaceModel = db.LeasePlace.Find(_Model.LEASE_PLACE_CD)

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelLeasePlace.TITLE)

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
            Dim model As LeasePlaceModel = db.LeasePlace.Find(ID)
            model.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")

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
        Public Overridable Function Delete(ByVal ViewModel As LeasePlaceViewModel) As ActionResult
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
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLeasePlace.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LEASE_PLACE_CD).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.LEASE_PLACE_NAME).Trim
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLeasePlace.TITLE))

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
            Session(CommonSessionKey.CREAGE_LEASE_PLACE) = _ViewModel

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
            Dim leasePlace As LeasePlaceModel = _db.LeasePlace.Find(_Model.LEASE_PLACE_CD)

            '削除を実行する
            _db.LeasePlace.Remove(leasePlace)

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
                sProcessContents = Me._Message.getMessage(CommonLabelLeasePlace.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLeasePlace.TITLE))

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
            Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_LEASE_PLACE)
            Dim stArrayData As String() = sHeader.Split(","c)

            'CSVデータ取得 & 項目数チェック
            Dim isKomokusuuErr As Boolean = False
            Dim tbl As DataTable = clsSV_CsvFile.ReadCsvFileToDataTable(sCsvPath, Encoding.UTF8, CChar(","), True, "LeasePlace", stArrayData.Length, isKomokusuuErr)
            '項目数エラーの場合
            If isKomokusuuErr Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelLeasePlace.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " &
                                                     Path.GetFileName(CsvFile.FileName)
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLeasePlace.TITLE))

                Return Json(Me._lstErr)
            End If

            'エラーチェック
            If Not IsDataError(tbl, stArrayData) Then
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelLeasePlace.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " &
                                                     Path.GetFileName(CsvFile.FileName) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                                     Me._lstErr.Count.ToString
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLeasePlace.TITLE))

                Return Json(Me._lstErr)
            End If

            '--登録処理開始
            Dim db As New SystemDBContext
            Dim model As List(Of LeasePlaceModel) = New List(Of LeasePlaceModel)    '登録用モデル

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try

                    '貸出先マスタデータを全削除
                    db.Database.ExecuteSqlCommand("DELETE FROM M_LEASE_PLACE")

                    '登録用モデルにデータを入れる
                    For icnt As Integer = 0 To tbl.Rows.Count - 1
                        model.Add(New LeasePlaceModel())
                        '--2019/10/01 nv miyaji upd start
                        model(icnt).LEASE_PLACE_CD = tbl.Rows(icnt).Item(CsvLeasePlace.ColIdx.Cd).ToString.Trim.TrimStart("'"c)
                        model(icnt).LEASE_PLACE_NAME = tbl.Rows(icnt).Item(CsvLeasePlace.ColIdx.Name).ToString.Trim
                        'model(icnt).LEASE_PLACE_CD = tbl.Rows(icnt).Item(stArrayData(0)).ToString.Trim
                        'model(icnt).LEASE_PLACE_NAME = tbl.Rows(icnt).Item(stArrayData(1)).ToString.Trim
                        '--2019/10/01 nv miyaji upd end
                        '--2021/08/26 nv miyaji add start
                        model(icnt).PHONE_NUMBER = tbl.Rows(icnt).Item(CsvLeasePlace.ColIdx.PhoneNumber).ToString.Trim.TrimStart("'"c)
                        model(icnt).ZIP_CODE = tbl.Rows(icnt).Item(CsvLeasePlace.ColIdx.ZipCode).ToString.Trim.TrimStart("'"c)
                        model(icnt).ADDRESS = tbl.Rows(icnt).Item(CsvLeasePlace.ColIdx.Address).ToString.Trim
                        '--2021/08/26 nv miyaji add end
                        model(icnt).SEC_LEASE_PLACE_FLG = tbl.Rows(icnt).Item(CsvLeasePlace.ColIdx.SecLeasePlace).ToString.Trim     '2023/03/22 Kawamoto ADD 
                        model(icnt).INS_DATETIME = CommonRfid.getNowDate
                        model(icnt).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(icnt).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(icnt).UPD_DATETIME = CommonRfid.getNowDate
                        model(icnt).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(icnt).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(icnt).UPD_TASK = Me._Message.getMessage(CommonLabelLeasePlace.TITLE)
                    Next

                    'システムコンテキストにモデルを追加
                    db.LeasePlace.AddRange(model)

                    '登録処理を実行
                    db.SaveChanges()

                    tran.Commit()

                    '操作ログ出力
                    sProcessContents = Me._Message.getMessage(CommonLabelLeasePlace.TITLE) & " " &
                                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                             Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                             tbl.Rows.Count.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLeasePlace.TITLE))

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

            Dim LeasePlaceData As List(Of LeasePlaceViewModel)

            '貸出先マスタデータ取得
            LeasePlaceData = GetAllLeasePlaceData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LeasePlace.KeyDiv, CommonSettingKey.LeasePlace.Key1, CommonSettingKey.LeasePlace.Key2, CommonSettingKey.LeasePlace.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_LEASE_PLACE)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_LEASE_PLACE)
            Dim singleQuotation As String = "'"

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As LeasePlaceViewModel In LeasePlaceData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LEASE_PLACE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_NAME)
                '--2021/08/26 nv miyaji add start
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.PHONE_NUMBER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.ZIP_CODE)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ADDRESS)
                '--2021/08/26 nv miyaji add end
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.SEC_LEASE_PLACE_FLG)      '2023/03/22 Kawamoto ADD

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLeasePlace.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     LeasePlaceData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLeasePlace.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function

#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetAllLeasePlaceData() As List(Of LeasePlaceViewModel)
            Dim sbSQL As StringBuilder = New StringBuilder

            sbSQL.Append("SELECT ")
            sbSQL.Append("  LEASE_PLACE_CD ")
            sbSQL.Append("  , LEASE_PLACE_NAME ")
            sbSQL.Append("  , PHONE_NUMBER ")    '2021/08/26 nv miyaji add
            sbSQL.Append("  , ZIP_CODE")    '2021/08/26 nv miyaji add
            sbSQL.Append("  , ADDRESS ")    '2021/08/26 nv miyaji add
            sbSQL.Append("  , SEC_LEASE_PLACE_FLG ")    '2023/03/22 Kawamoto ADD
            sbSQL.Append("  , INS_DATETIME ")
            sbSQL.Append("  , INS_EMPLOYEE_CD ")
            sbSQL.Append("  , INS_EMPLOYEE_NAME ")
            sbSQL.Append("  , UPD_DATETIME ")
            sbSQL.Append("  , UPD_EMPLOYEE_CD ")
            sbSQL.Append("  , UPD_EMPLOYEE_NAME ")
            sbSQL.Append("  , UPD_TASK ")
            sbSQL.Append("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_LEASE_PLACE ")
            sbSQL.Append(" ORDER BY LEASE_PLACE_CD")

            Dim lstLeasePlace As List(Of LeasePlaceViewModel)   '貸出先リスト
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstLeasePlace = db.LeasePlaceView.SqlQuery(sbSQL.ToString).ToList

            'データを返却
            Return lstLeasePlace
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

            '--データチェック
            blRet = True
            Dim lstSumiCd As List(Of String) = New List(Of String)
            For iRowCnt As Integer = 0 To tbl.Rows.Count - 1
                For iCalCnt As Integer = 0 To stArrayData.Length - 1
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If

                    '列によってチェック条件・設定項目値を分ける
                    Dim sStr As String = tbl.Rows(iRowCnt).Item(iCalCnt).ToString.Trim.TrimStart("'"c)
                    Select Case iCalCnt
                        '貸出先コード
                        Case CsvLeasePlace.ColIdx.Cd
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.LeasePlaceCd Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHankaku(sStr) Then
                                '半角英数字チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            ElseIf lstSumiCd.IndexOf(sStr) > -1 Then
                                '重複チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_CD) & "[{0}]", sStr)
                                blRet = False
                            Else
                                lstSumiCd.Add(sStr)
                            End If
                        '貸出先名
                        Case CsvLeasePlace.ColIdx.Name
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_NAME)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr.Length > itemlength.LeasePlaceNm Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_LEASE_PLACE_NAME)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                        '--2021/08/26 nv miyaji add start
                        '電話番号
                        Case CsvLeasePlace.ColIdx.PhoneNumber
                            If sStr.Length > itemlength.PhoneNumber Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PHONE_NUMBER)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHanSuuHaifun(sStr) Then
                                '半角数字+"-"チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_PHONE_NUMBER)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If
                        '郵便番号
                        Case CsvLeasePlace.ColIdx.ZipCode
                            If sStr.Length > itemlength.ZipCode Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ZIP_CODE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            ElseIf Not CommonRfid.IsHanSuuHaifun(sStr) Then
                                '半角数字+"-"チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ZIP_CODE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If
                        '住所
                        Case CsvLeasePlace.ColIdx.Address
                            If sStr.Length > itemlength.Address Then
                                '項目長チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ADDRESS)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                                blRet = False
                            End If
                            '--2021/08/26 nv miyaji add end
                        '2023/03/22 Kawamoto ADD START
                        '二次貸出先
                        Case CsvLeasePlace.ColIdx.SecLeasePlace
                            If String.IsNullOrEmpty(sStr) Then
                                '空白チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_SECOND_LEASE_PLACE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
                                blRet = False
                            ElseIf sStr <> SecLeasePlaceFlg.Cd.Yes AndAlso sStr <> SecLeasePlaceFlg.Cd.No Then
                                '文字制約チェック
                                Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                                Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_SECOND_LEASE_PLACE)
                                Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                                blRet = False
                            End If
                            '2023/03/22 Kawamoto ADD END
                    End Select
                Next
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
                _viewResult.Add(New LeasePlaceViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchCode = _ViewModel.SearchCode
            _viewResult.ElementAt(0).SearchName = _ViewModel.SearchName
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.LEASE_PLACE).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.LEASE_PLACE)

        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As LeasePlaceViewModel)
            _Model.LEASE_PLACE_CD = model.LEASE_PLACE_CD
            _Model.LEASE_PLACE_NAME = model.LEASE_PLACE_NAME
            '--2021/08/26 nv miyaji add start
            _Model.PHONE_NUMBER = model.PHONE_NUMBER
            _Model.ZIP_CODE = model.ZIP_CODE
            _Model.ADDRESS = model.ADDRESS
            '--2021/08/26 nv miyaji add end
            '2023/03/22 Kawamoto ADD START
            If model.DispSecLeasePlaceFlg Then
                _Model.SEC_LEASE_PLACE_FLG = SecLeasePlaceFlg.Cd.Yes
            Else
                _Model.SEC_LEASE_PLACE_FLG = SecLeasePlaceFlg.Cd.No
            End If
            '2023/03/22 Kawamoto ADD END
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