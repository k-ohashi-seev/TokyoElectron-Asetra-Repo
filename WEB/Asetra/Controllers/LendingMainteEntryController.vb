Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure

Namespace Controllers
    ''' <summary>
    ''' メンテナンス登録画面表示用のクラス
    ''' </summary>
    Public Class LendingMainteEntryController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        Public Const MIN_DATE As Integer = 10000101

#Region "クラス定数"

#End Region

#Region "クラス変数"
        Private _Model As LendingMainteEntryModel = New LendingMainteEntryModel                                 'LendingMainteEntryモデル
        Private _ViewModel As LendingMainteEntryViewModel = New LendingMainteEntryViewModel                     'LendingMainteEntry一覧表示用モデル
        Protected _viewResult As List(Of LendingMainteEntryViewModel) = New List(Of LendingMainteEntryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
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
        Public Function Index(ByVal ViewModel As LendingMainteEntryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_LENDING_MAINTE_ENTRY)
            End If

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '一覧表示処理

                'モデルを設定(セッションにある場合はセッションから設定)
                Dim model As LendingMainteEntryViewModel = Session(CommonSessionKey.CREAGE_LENDING_MAINTE_ENTRY)
                If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                    _ViewModel = model

                    _viewResult.Add(_ViewModel)

                    '全て更新可能
                    _ViewModel.AuthProcFlg = AuthProcFlg.All
                Else
                    _ViewModel = ViewModel

                    'セッション情報を取得する
                    Dim SeniInfo As LendingMainteEntryInfo = Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO)
                    If SeniInfo.DispKbn = DispKbn.Create Then
                        _ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK
                        _ViewModel.ASSET_NO = SeniInfo.AssetNo
                    Else
                        If SeniInfo.DispKbn = DispKbn.Edit Then
                            '全て更新可能
                            _ViewModel.AuthProcFlg = AuthProcFlg.All
                        Else
                            '照会のみ
                            _ViewModel.AuthProcFlg = AuthProcFlg.ShokaiNomi
                        End If
                        _ViewModel.OnCreteButton = CommonConst.KEY_ON_UPDATE_CLICK
                        _ViewModel.MAINTE_ID = SeniInfo.MainteId
                    End If
                    _ViewModel.MAINTE_KBN = SeniInfo.MainteKbn
                End If

                '初期表示時のみ、一覧表示のSQLを呼ぶ。
                If Not (Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK) Then
                    MyBase.BaseIndex(_ViewModel)
                End If

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo

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
                Session.Remove(CommonSessionKey.CREAGE_LENDING_MAINTE_ENTRY)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            'Dim sbSQL As StringBuilder = New StringBuilder

            ''一覧表示表用SQL
            'sbSQL.Append("SELECT ")
            'sbSQL.Append("  DEPARTMENT_CD ")
            'sbSQL.Append("  , DEPARTMENT_NAME ")
            'sbSQL.Append("  , ZIP_CODE")    '2021/08/16 nv miyaji add
            'sbSQL.Append("  , ADDRESS ")    '2021/08/16 nv miyaji add
            'sbSQL.Append("  , REQ_DEP_NAME ")    '2021/08/16 nv miyaji add
            'sbSQL.Append("  , PHONE_NUMBER ")    '2021/08/16 nv miyaji add
            'sbSQL.Append("  , FAX ")    '2021/08/16 nv miyaji add
            'sbSQL.Append("  , HEAD_OFFICE_FLG ")    '2021/08/16 nv miyaji add
            'sbSQL.Append("  , INS_DATETIME ")
            'sbSQL.Append("  , INS_EMPLOYEE_CD ")
            'sbSQL.Append("  , INS_EMPLOYEE_NAME ")
            'sbSQL.Append("  , UPD_DATETIME ")
            'sbSQL.Append("  , UPD_EMPLOYEE_CD ")
            'sbSQL.Append("  , UPD_EMPLOYEE_NAME ")
            'sbSQL.Append("  , UPD_TASK ")
            'sbSQL.Append("   ,COUNT(*) OVER () AS TotalCount ")
            'sbSQL.Append("FROM ")
            'sbSQL.Append("  T_RESERVE_MAITE ")
            'sbSQL.Append("WHERE ")
            'sbSQL.Append("  1 = 1 ")

            '検索条件を設定する
            ''部署コード
            'If Not String.IsNullOrEmpty(_ViewModel.SearchCode) Then
            '    sbSQL.Append("  AND DEPARTMENT_CD Like @departmentCd")
            '    addSqlParam("departmentCd", $"{_ViewModel.SearchCode}%")
            'End If

            ''部署名
            'If Not String.IsNullOrEmpty(_ViewModel.SearchName) Then
            '    sbSQL.Append("  AND DEPARTMENT_NAME Like @name")
            '    addSqlParam("name", $"%{_ViewModel.SearchName}%")
            'End If

            'sbSQL.Append(" ORDER BY DEPARTMENT_CD")

            ''クエリを返却
            'MyBase._SQL = sbSQL.ToString

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()
            Dim db As New SystemDBContext

            If _ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK Then
                '登録画面

                'IDをキーに資産モデルを取得する
                Dim modelAsset As AssetListModel = db.AssetList.Find(_ViewModel.ASSET_NO)
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationL, modelAsset.MNG_LOC_L_CD, " ", " "}
                Dim modelLoc As LocationModel = db.Location.Find(findKeys)

                _ViewModel.RESERVE_ID = 0   '予約番号
                _ViewModel.AssetName = modelAsset.ASSET_NAME   '資産名
                _ViewModel.SerialNo = modelAsset.FREE_ITEM10   'シリアルNo
                _ViewModel.MngLocLName = modelLoc.LOC_NAME   '管理ロケーション名
            Else
                '変更画面

                'IDをキーに予約メンテナンスモデルを取得する
                Dim modelMainte As LendingMainteEntryModel = db.LendingMainteEntry.Find(_ViewModel.MAINTE_ID)
                'IDをキーに資産モデルを取得する
                Dim modelAsset As AssetListModel = db.AssetList.Find(modelMainte.ASSET_NO)
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationL, modelAsset.MNG_LOC_L_CD, " ", " "}
                Dim modelLoc As LocationModel = db.Location.Find(findKeys)

                _ViewModel.RESERVE_ID = modelMainte.RESERVE_ID   '予約番号
                _ViewModel.ASSET_NO = modelMainte.ASSET_NO   '予約番号
                _ViewModel.AssetName = modelAsset.ASSET_NAME   '資産名
                _ViewModel.SerialNo = modelAsset.FREE_ITEM10   'シリアルNo
                _ViewModel.MngLocLName = modelLoc.LOC_NAME   '管理ロケーション名
                _ViewModel.MAINTE_START_DT = modelMainte.MAINTE_START_DT   'メンテナンス開始日
                _ViewModel.MAINTE_END_DT = modelMainte.MAINTE_END_DT   'メンテナンス終了日
                _ViewModel.REMARK = modelMainte.REMARK   '備考
                _ViewModel.MainteStartDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(modelMainte.MAINTE_START_DT))   'メンテナンス開始日表示文字列
                _ViewModel.MainteEndDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(modelMainte.MAINTE_END_DT))    'メンテナンス終了日表示文字列
                _ViewModel.INS_DATETIME = modelMainte.INS_DATETIME
                _ViewModel.INS_EMPLOYEE_CD = modelMainte.INS_EMPLOYEE_CD
                _ViewModel.INS_EMPLOYEE_NAME = modelMainte.INS_EMPLOYEE_NAME
                _ViewModel.UPD_DATETIME = modelMainte.UPD_DATETIME
                _ViewModel.UPD_EMPLOYEE_CD = modelMainte.UPD_EMPLOYEE_CD
                _ViewModel.UPD_EMPLOYEE_NAME = modelMainte.UPD_EMPLOYEE_NAME
                _ViewModel.UPD_TASK = modelMainte.UPD_TASK
            End If

            '画面に表示するViewを設定
            _viewResult.Add(_ViewModel)

            ''SQLを実行
            'Dim query As DbSqlQuery(Of LendingMainteEntryViewModel) = _db.LendingMainteEntryView.SqlQuery(_SQL, _paramSqlList.ToArray)

            ''画面に表示するViewを設定
            '_viewResult = query.ToList

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
        Public Function Create(ByVal ViewModel As LendingMainteEntryViewModel, ByVal OnCreteButton As String) As ActionResult
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

            'メンテナンス開始日・メンテナンス終了日を設定する
            If String.IsNullOrEmpty(_ViewModel.MainteStartDtString) Then
                _ViewModel.MAINTE_START_DT = Nothing
            Else
                _ViewModel.MAINTE_START_DT = CInt(CDate(_ViewModel.MainteStartDtString).ToString("yyyyMMdd"))
            End If
            If String.IsNullOrEmpty(_ViewModel.MainteEndDtString) Then
                _ViewModel.MAINTE_END_DT = Nothing
            Else
                _ViewModel.MAINTE_END_DT = CInt(CDate(_ViewModel.MainteEndDtString).ToString("yyyyMMdd"))
            End If

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
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_MAINTE_TOUROKU) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_MAINTE_ID) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.MAINTE_ID).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_MAINTE_TOUROKU))

                    _ViewModel.States = StatusType.SuccessCreate
                Else

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_MAINTE_HENKOU) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_MAINTE_ID) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.MAINTE_ID)
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_MAINTE_HENKOU))

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
            Session(CommonSessionKey.CREAGE_LENDING_MAINTE_ENTRY) = _ViewModel

            'エラーでない場合、カゴのセッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_CART_INFO)
            End If

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
            Session(CommonSessionKey.CREAGE_LENDING_MAINTE_ENTRY) = _ViewModel

            'エラーでない場合、カゴのセッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_CART_INFO)
            End If

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext

            '照合機能設定を取得
            Dim MatchingFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.MatchingFunction.KeyDiv,
                                                                         CommonSettingKey.MatchingFunction.Key1,
                                                                         CommonSettingKey.MatchingFunction.Key2,
                                                                         CommonSettingKey.MatchingFunction.Key3, True)

            If Not IsNothing(MatchingFunc) AndAlso MatchingFunc.DATA1 = ValidFlg.Cd.Invalid Then
                '照合機能無し
                'メンテナンス開始日
                'システム日付より過去の場合、エラー
                If _Model.MAINTE_START_DT < CInt(Now.Date.ToString("yyyyMMdd")) Then
                    ModelState.AddModelError("MainteStartDtString", CommonLabel.MSG_ERR_MAINTE_START_DT_KAKOBI)
                    Return False
                End If
            End If

            'メンテナンス開始日＞メンテナンス終了日の場合、エラー
            If _Model.MAINTE_START_DT > _Model.MAINTE_END_DT Then
                ModelState.AddModelError("MainteStartDtString", CommonLabel.MSG_ERR_MAINTE_END_DT_KAKOBI)
                Return False
            End If

            'メンテナンス開始日,メンテナンス終了日 1000年以上チェック
            If _Model.MAINTE_START_DT < MIN_DATE Then
                ModelState.AddModelError("MainteStartDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                Return False
            End If

            If _Model.MAINTE_END_DT < MIN_DATE Then
                ModelState.AddModelError("MainteEndDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                Return False
            End If

            'メンテナンス期間の重複確認
            If Dao.ToolReserve.GetReserveMainteJufukuSuu(_Model.ASSET_NO, 0, 0, _Model.MAINTE_START_DT, _Model.MAINTE_END_DT, "Y") > 0 Then
                ModelState.AddModelError("MainteStartDtString", CommonLabel.MSG_ERR_MAINTE_DT_JUUFUKU)
                Return False
            End If

            Return True

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_MAINTE_TOUROKU)

            _db.LendingMainteEntry.Add(_Model)


        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim db As New SystemDBContext

            '照合機能設定を取得
            Dim MatchingFunc As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.MatchingFunction.KeyDiv,
                                                                         CommonSettingKey.MatchingFunction.Key1,
                                                                         CommonSettingKey.MatchingFunction.Key2,
                                                                         CommonSettingKey.MatchingFunction.Key3, True)

            If Not IsNothing(MatchingFunc) AndAlso MatchingFunc.DATA1 = ValidFlg.Cd.Invalid Then
                '照合機能無し
                'メンテナンス開始日
                'システム日付より過去の場合、エラー
                If _Model.MAINTE_START_DT < CInt(Now.Date.ToString("yyyyMMdd")) Then
                    ModelState.AddModelError("MainteStartDtString", CommonLabel.MSG_ERR_MAINTE_START_DT_KAKOBI)
                    Return False
                End If
            End If

            'メンテナンス開始日＞メンテナンス終了日の場合、エラー
            If _Model.MAINTE_START_DT > _Model.MAINTE_END_DT Then
                ModelState.AddModelError("MainteStartDtString", CommonLabel.MSG_ERR_MAINTE_END_DT_KAKOBI)
                Return False
            End If

            'メンテナンス開始日,メンテナンス終了日 1000年以上チェック
            If _Model.MAINTE_START_DT < MIN_DATE Then
                ModelState.AddModelError("MainteStartDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                Return False
            End If

            If _Model.MAINTE_END_DT < MIN_DATE Then
                ModelState.AddModelError("MainteEndDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                Return False
            End If

            'メンテナンス期間の重複確認
            If Dao.ToolReserve.GetReserveMainteJufukuSuu(_Model.ASSET_NO, 0, _Model.MAINTE_ID, _Model.MAINTE_START_DT, _Model.MAINTE_END_DT, "Y") > 0 Then
                ModelState.AddModelError("MainteStartDtString", CommonLabel.MSG_ERR_MAINTE_DT_JUUFUKU)
                Return False
            End If

            'DBのデータを取得する
            Dim model As LendingMainteEntryModel = db.LendingMainteEntry.Find(_Model.MAINTE_ID)

            '他端末で更新されているかチェックする
            If _Model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") <> model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") Then
                'モデルとDBの更新日付が異なっている場合エラー
                ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' 更新処理のコンテキストを変更します
        ''' </summary>
        Protected Overrides Sub EditQuery()
            '更新日時・更新社員コード・更新社員名・更新プロセスを登録
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_MAINTE_HENKOU)

            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified

        End Sub

        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="ID">ID</param>
        ''' <returns>View表示内容</returns>
        Public Function GetEditData(ByVal ID As String) As JsonResult
            'Dim db As New SystemDBContext

            ''IDをキーにモデルを取得する
            'Dim model As LendingMainteEntryModel = db.LendingMainteEntry.Find(ID)
            ''model.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            ''model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")

            'Return Json(model)

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
        Public Overridable Function Delete(ByVal ViewModel As LendingMainteEntryViewModel) As ActionResult
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
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_MAINTE_HENKOU) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_MAINTE_ID) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.MAINTE_ID).Trim
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_MAINTE_HENKOU))

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
            Session(CommonSessionKey.CREAGE_LENDING_MAINTE_ENTRY) = _ViewModel

            'エラーでない場合、カゴのセッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_CART_INFO)
            End If

        End Sub

        ''' <summary>
        ''' 削除処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function DeleteChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            ''DBのデータを取得する
            'Dim model As LendingMainteEntryModel = db.LendingMainteEntry.Find(_Model.DEPARTMENT_CD)

            ''他の箇所で使用されている場合エラー
            'If Dao.ToolLendingMainteEntry.IsUseLendingMainteEntry(_Model.DEPARTMENT_CD) Then
            '    ModelState.AddModelError("DEPARTMENT_CD", CommonErrorMsg.MSG_ERR_USED_DEPARTMENT)
            '    blnReturn = False
            'End If

            Return blnReturn
        End Function

        ''' <summary>
        ''' 削除処理のクエリを作成します
        ''' </summary>
        Protected Overrides Sub DeleteQuery()
            Dim LendingMainte As LendingMainteEntryModel = _db.LendingMainteEntry.Find(_Model.MAINTE_ID)

            '削除を実行する
            _db.LendingMainteEntry.Remove(LendingMainte)

        End Sub
#End Region


#Region "共通"

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New LendingMainteEntryViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton

            _viewResult.ElementAt(0).MAINTE_KBN = _ViewModel.MAINTE_KBN

        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As LendingMainteEntryViewModel)
            _Model.MAINTE_ID = model.MAINTE_ID
            _Model.RESERVE_ID = model.RESERVE_ID
            _Model.ASSET_NO = model.ASSET_NO
            _Model.MAINTE_START_DT = model.MAINTE_START_DT
            _Model.MAINTE_END_DT = model.MAINTE_END_DT
            _Model.REMARK = model.REMARK

            Dim SeniInfo As LendingMainteEntryInfo = Session(CommonSessionKey.LENDING_MAINTE_ENTRY_INFO)
            _Model.MAINTE_KBN = model.MAINTE_KBN

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