Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports Asetra.Dao
Imports System.Data.SqlClient

Namespace Controllers
    ''' <summary>
    ''' 設定マスタメンテナンス画面表示用のクラス
    ''' </summary>
    Public Class SettingController
        Inherits PageBaseController

#Region "クラス変数"
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _Model As SettingModel = New SettingModel                                   'Settingモデル
        Private _ViewModel As SettingViewModel = New SettingViewModel                       'Setting一覧表示用モデル
        Protected _viewResult As List(Of SettingViewModel) = New List(Of SettingViewModel)  '結果View
        Private _Message As MessageRfid = Nothing
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
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
        Public Function Index(ByVal ViewModel As SettingViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_SETTING)
            Else
                '本処理のみ、基本情報、再ロード
                Dim multi As MessageRfid = New MessageRfid()
                multi.Load()
                HttpContext.Application(CommonConst.KEY_APP_LANGUAGE) = multi
                Misc.CommonRfid.MessageRfid = multi
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As SettingViewModel = Session(CommonSessionKey.CREAGE_SETTING)
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
                MyBase.BaseIndex(_ViewModel, False)
                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm

                ViewBag.LoginInfo = _LoginInfo
            End If

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.SETTING, "ALL")
            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If

            '機能分類名コンボボックス設定
            DropDownListCreate()

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
                Session.Remove(CommonSessionKey.CREAGE_SETTING)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT TOP " & Misc.CommonConst.SettingListMaxRowCount.ToString)
            sbSQL.Append("  A.KEY_DIV ")
            sbSQL.Append("  , A.KEY1 ")
            sbSQL.Append("  , A.KEY2 ")
            sbSQL.Append("  , A.KEY3 ")
            sbSQL.Append("  , A.REMARK ")
            sbSQL.Append("  , A.DATA1 ")
            sbSQL.Append("  , A.DATA2 ")
            sbSQL.Append("  , A.DATA3 ")
            sbSQL.Append("  , A.DATA4 ")
            sbSQL.Append("  , A.DATA5 ")
            sbSQL.Append("  , A.DATA6 ")
            sbSQL.Append("  , A.DATA7 ")
            sbSQL.Append("  , A.DATA8 ")
            sbSQL.Append("  , A.DATA9 ")
            sbSQL.Append("  , A.DATA10 ")
            sbSQL.Append("  , A.SPECIAL_DATA_DIV ")
            sbSQL.Append("  , A.FUNC_CAT_NAME1 ")
            sbSQL.Append("  , A.FUNC_CAT_NAME2 ")
            sbSQL.Append("  , A.FUNC_CAT_NAME3 ")
            sbSQL.Append("  , A.SETTING_OBJECT_DIV ")
            sbSQL.Append("  , A.FUNC_CAT_SORT ")
            sbSQL.Append("  , A.UPD_AUTHORITY_FLG ")
            sbSQL.Append("  , A.DEL_AUTHORITY_FLG ")
            sbSQL.Append("  , A.INS_DATETIME ")
            sbSQL.Append("  , A.INS_EMPLOYEE_CD ")
            sbSQL.Append("  , A.INS_EMPLOYEE_NAME ")
            sbSQL.Append("  , A.UPD_DATETIME ")
            sbSQL.Append("  , A.UPD_EMPLOYEE_CD ")
            sbSQL.Append("  , A.UPD_EMPLOYEE_NAME ")
            sbSQL.Append("  , A.UPD_TASK ")

            sbSQL.Append("  ,CASE ")
            'sbSQL.Append("  WHEN A.FUNC_CAT_NAME2 = '' OR A.FUNC_CAT_NAME2 IS NULL AND A.FUNC_CAT_NAME3 = '' OR A.FUNC_CAT_NAME3 IS NULL THEN A.FUNC_CAT_NAME1")
            'sbSQL.Append("  WHEN A.FUNC_CAT_NAME2 != '' OR A.FUNC_CAT_NAME2 IS NOT NULL AND A.FUNC_CAT_NAME3 = '' OR A.FUNC_CAT_NAME3 IS NULL THEN A.FUNC_CAT_NAME1 + ',' + A.FUNC_CAT_NAME2")
            'sbSQL.Append("  WHEN A.FUNC_CAT_NAME3 != '' OR A.FUNC_CAT_NAME3 IS NOT NULL AND A.FUNC_CAT_NAME3 != '' OR A.FUNC_CAT_NAME3 IS NOT NULL THEN A.FUNC_CAT_NAME1 + ',' + A.FUNC_CAT_NAME2 + ',' + A.FUNC_CAT_NAME3")

            sbSQL.Append("  WHEN (A.FUNC_CAT_NAME2 = '' OR A.FUNC_CAT_NAME2 IS NULL) AND (A.FUNC_CAT_NAME3 = '' OR A.FUNC_CAT_NAME3 IS NULL) THEN B.DATA1")
            sbSQL.Append("  WHEN (A.FUNC_CAT_NAME2 != '' OR A.FUNC_CAT_NAME2 IS NOT NULL) AND (A.FUNC_CAT_NAME3 = '' OR A.FUNC_CAT_NAME3 IS NULL) THEN B.DATA1 + ' , ' + C.DATA1")
            sbSQL.Append("  WHEN (A.FUNC_CAT_NAME2 != '' OR A.FUNC_CAT_NAME2 IS NOT NULL) AND (A.FUNC_CAT_NAME3 != '' OR A.FUNC_CAT_NAME3 IS NOT NULL) THEN B.DATA1 + ' , ' + C.DATA1 + ' , ' + D.DATA1")
            sbSQL.Append("  WHEN (A.FUNC_CAT_NAME2 = '' OR A.FUNC_CAT_NAME2 IS NULL) AND (A.FUNC_CAT_NAME3 != '' OR A.FUNC_CAT_NAME3 IS NOT NULL) THEN B.DATA1 + ' , ' + D.DATA1")

            sbSQL.Append("  END AS FUNC_CAT_NAME")

            sbSQL.Append("   ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_SETTING AS A ")
            sbSQL.Append("  LEFT JOIN M_SETTING AS B ")
            sbSQL.Append("  ON A.FUNC_CAT_NAME1 = B.KEY1 ")
            sbSQL.Append("  AND B.KEY_DIV = 'FUNCTION_CATEGORY' ")

            sbSQL.Append("  LEFT JOIN M_SETTING AS C ")
            sbSQL.Append("  ON A.FUNC_CAT_NAME2 = C.KEY1 ")
            sbSQL.Append("  AND C.KEY_DIV = 'FUNCTION_CATEGORY' ")

            sbSQL.Append("  LEFT JOIN M_SETTING AS D ")
            sbSQL.Append("  ON A.FUNC_CAT_NAME3 = D.KEY1 ")
            sbSQL.Append("  AND D.KEY_DIV = 'FUNCTION_CATEGORY' ")


            sbSQL.Append("WHERE ")
            sbSQL.Append("  1 = 1 ")

            '検索条件を設定する
            '種別
            If Not IsNothing(_ViewModel.SearchKeyDiv) AndAlso _ViewModel.SearchKeyDiv <> "" Then
                sbSQL.Append("  AND A.KEY_DIV Like @keyDiv")
                addSqlParam("keyDiv", $"{_ViewModel.SearchKeyDiv}%")
            End If

            '機能分類名
            If Not IsNothing(_ViewModel.SearchFuncCatName) AndAlso _ViewModel.SearchFuncCatName <> "" Then
                sbSQL.AppendLine("  AND ( A.FUNC_CAT_NAME1 = @funcCatName")
                sbSQL.AppendLine("  OR A.FUNC_CAT_NAME2 = @funcCatName")
                sbSQL.AppendLine("  OR A.FUNC_CAT_NAME3 = @funcCatName )")
                addSqlParam("funcCatName", $"{_ViewModel.SearchFuncCatName}")
            End If

            Dim sbSQLCheck As New StringBuilder
            'HT
            If _ViewModel.SearchSettingObjectDivHT Then
                If sbSQLCheck.Length <> 0 Then
                    sbSQLCheck.AppendLine("OR")
                End If
                sbSQLCheck.AppendLine("A.SETTING_OBJECT_DIV = '" & SETTING_OBJECT_DIV.HT & "'")
            End If
            'PC
            If _ViewModel.SearchSettingObjectDivPC Then
                If sbSQLCheck.Length <> 0 Then
                    sbSQLCheck.AppendLine("OR")
                End If
                sbSQLCheck.AppendLine("A.SETTING_OBJECT_DIV = '" & SETTING_OBJECT_DIV.PC & "'")
            End If
            'WEB
            If _ViewModel.SearchSettingObjectDivWEB Then
                If sbSQLCheck.Length <> 0 Then
                    sbSQLCheck.AppendLine("OR")
                End If
                sbSQLCheck.AppendLine("A.SETTING_OBJECT_DIV = '" & SETTING_OBJECT_DIV.WEB & "'")
            End If
            'ALL
            If _ViewModel.SearchSettingObjectDivALL Then
                If sbSQLCheck.Length <> 0 Then
                    sbSQLCheck.AppendLine("OR")
                End If
                sbSQLCheck.AppendLine("A.SETTING_OBJECT_DIV = '" & SETTING_OBJECT_DIV.ALL & "'")
            End If
            sbSQL.AppendLine("  AND (" & sbSQLCheck.ToString & ")")


            sbSQL.Append(" ORDER BY B.FUNC_CAT_SORT, A.FUNC_CAT_SORT, A.KEY_DIV, A.KEY1, A.KEY2, A.KEY3")

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of SettingViewModel) = _db.SettingView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function Create(ByVal ViewModel As SettingViewModel, ByVal OnCreteButton As String) As ActionResult
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

            If (OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
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
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelSetting.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY_DIV) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.KEY_DIV).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY1) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.KEY1).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY2) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.KEY2).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY3) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.KEY3).Trim & "  "
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelSetting.TITLE))

                    _ViewModel.States = StatusType.SuccessCreate
                Else
                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelSetting.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY_DIV) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.KEY_DIV).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY1) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.KEY1).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY2) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.KEY2).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY3) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.KEY3).Trim & "  "
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelSetting.TITLE))

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
            Session(CommonSessionKey.CREAGE_SETTING) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_SETTING) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            'IDが登録済みかチェックする
            Dim Setting As SettingModel = _db.Setting.Find(_Model.KEY_DIV, _Model.KEY1, _Model.KEY2, _Model.KEY3)

            '重複エラーの場合、エラー情報を保持する
            If Not IsNothing(Setting) Then
                ModelState.AddModelError("KEY_DIV", CommonLabel.MSG_ERR_DATAJUUFUKU)
                blnReturn = False
            End If

            '機能分類_並び順が重複している場合、エラー
            'Dim InsDataCnt As Integer = GetInsUpdCheckData(_Model.FUNC_CAT_NAME1, _Model.FUNC_CAT_SORT, _Model.KEY_DIV, _Model.KEY1, _Model.KEY2, _Model.KEY3)
            'If InsDataCnt > 0 Then
            '    ModelState.AddModelError("FUNC_CAT_SORT", CommonLabelSetting.MSG_ERR_ID_FNC_CAT_ORDER_DUPLICATION)
            '    blnReturn = False
            'End If


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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelSetting.TITLE)

            _db.Setting.Add(_Model)

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As SettingModel = db.Setting.Find(_Model.KEY_DIV, _Model.KEY1, _Model.KEY2, _Model.KEY3)

            '他端末で更新されているかチェックする
            If _Model.UPD_DATETIME.ToString("yyyy/MM/dd HH:mm:ss") <> model.UPD_DATETIME.ToString("yyyy/MM/dd HH:mm:ss") Then
                'モデルとDBの更新日付が異なっている場合エラー
                ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                blnReturn = False
            End If

            ''機能分類_並び順が重複している場合、エラー
            'Dim UpdDataCnt As Integer = GetInsUpdCheckData(_Model.FUNC_CAT_NAME1, _Model.FUNC_CAT_SORT, _Model.KEY_DIV, _Model.KEY1, _Model.KEY2, _Model.KEY3)
            'If UpdDataCnt > 0 Then
            '    ModelState.AddModelError("FUNC_CAT_SORT", CommonLabelSetting.MSG_ERR_ID_FNC_CAT_ORDER_DUPLICATION)
            '    blnReturn = False
            'End If

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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelSetting.TITLE)
            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified
        End Sub

        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="KEYS">KEYS</param>
        ''' <returns>View表示内容</returns>
        Public Function GetEditData(ByVal KEYS) As JsonResult
            Dim db As New SystemDBContext

            Dim findKeys As String() = KEYS(0).Split(",")
            Dim model As SettingModel = db.Setting.Find(findKeys)

            model.EDIT_SPECIAL_DATA_DIV = model.SPECIAL_DATA_DIV
            model.EDIT_INS_DATETIME = model.INS_DATETIME.ToString()
            model.EDIT_INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            model.EDIT_INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            model.EDIT_UPD_DATETIME = model.UPD_DATETIME.ToString()
            model.EDIT_UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            model.EDIT_UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            model.EDIT_UPD_TASK = model.UPD_TASK.ToString()

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
        Public Overridable Function Delete(ByVal ViewModel As SettingViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim checkFlg As Boolean = False

            '削除処理
            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim deleteKeys As String() = ViewModel.DeleteKeys.Split(":")
            For Each s As String In deleteKeys
                Dim deleteKey As String() = s.Split(",")

                sbSQL.Clear()
                sbSQL.AppendLine("DELETE FROM M_SETTING")
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("KEY_DIV = '" & deleteKey(0) & "'")
                sbSQL.AppendLine("AND KEY1 = '" & deleteKey(1) & "'")
                sbSQL.AppendLine("AND KEY2 = '" & deleteKey(2) & "'")
                sbSQL.AppendLine("AND KEY3 = '" & deleteKey(3) & "'")
                db.Database.ExecuteSqlCommand(sbSQL.ToString())

                '操作ログ出力
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelSetting.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY_DIV) & ": " &
                                                     deleteKey(0).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY1) & ": " &
                                                     deleteKey(1).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY2) & ": " &
                                                     deleteKey(2).Trim & "  " &
                                                     Me._Message.getMessage(CommonLabelSetting.LABEL_KEY3) & ": " &
                                                     deleteKey(3).Trim & "  "
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelSetting.TITLE))
            Next

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                    _ViewModel.States = StatusType.SuccessDelete
                End If

            '検索項目の値を保存
            _ViewModel.SearchKeyDiv = ViewModel.SearchKeyDiv
            _ViewModel.SearchFuncCatName = ViewModel.SearchFuncCatName
            _ViewModel.SearchSettingObjectDivHT = ViewModel.SearchSettingObjectDivHT
            _ViewModel.SearchSettingObjectDivPC = ViewModel.SearchSettingObjectDivPC
            _ViewModel.SearchSettingObjectDivWEB = ViewModel.SearchSettingObjectDivWEB
            _ViewModel.SearchSettingObjectDivALL = ViewModel.SearchSettingObjectDivALL


            'モデルの内容をセッションに保持
            '※MyBase.BaseDeleteを使用しない場合、DisposeDeleteが実行されないためここで設定する。
            Session(CommonSessionKey.CREAGE_SETTING) = _ViewModel

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
            Session(CommonSessionKey.CREAGE_SETTING) = _ViewModel

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
            Dim setting As SettingModel = _db.Setting.Find(_Model.KEY_DIV, _Model.KEY1, _Model.KEY2, _Model.KEY3)
            '削除を実行する
            _db.Setting.Remove(setting)
        End Sub
#End Region

#Region "CSVデータ取込処理"
        ''' <summary>
        ''' アクションメソッド(CsvImport)
        ''' 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        ''' 詳細については、http://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvImport(ByVal ViewModel As SettingViewModel) As ActionResult
            'If ModelState.IsValid Then
            Dim File As HttpPostedFileBase = Request.Files("CsvImport")
            If IsDBNull(File) And File.ContentLength > 0 Then
                Dim uploadDir As String = "~/uploads"
                Dim sCsvPath As String = Path.Combine(Server.MapPath(uploadDir), File.FileName)
                Dim sCsvUrl As String = Path.Combine(uploadDir, File.FileName)
                File.SaveAs(sCsvPath)
                'ViewModel.CsvPath = sCsvUrl
            End If

            'End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "全データCSV出力処理"
        ''' <summary>
        ''' アクションメソッド(CsvExport)
        ''' 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        ''' 詳細については、http://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ByVal ViewModel As SettingViewModel) As ActionResult
            'If ModelState.IsValid Then
            '    Dim File As HttpPostedFileBase = Request.Files("CsvImport")
            '    If IsDBNull(File) And File.ContentLength > 0 Then
            '        Dim uploadDir As String = "~/uploads"
            '        Dim sCsvPath As String = Path.Combine(Server.MapPath(uploadDir), File.FileName)
            '        Dim sCsvUrl As String = Path.Combine(uploadDir, File.FileName)
            '        File.SaveAs(sCsvPath)
            '        EmployeeViewModel.CsvPath = sCsvUrl
            '    End If

            'End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "コンボボックス用設定マスタ取得処理"
        ''' <summary>
        ''' コンボボックス用に設定マスタを取得します
        ''' </summary>
        Public Sub DropDownListCreate()

            Dim lstFuncCatName As List(Of SettingModel)     '機能分類名リスト
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext
            'M_SETTINGテーブルから取得するSQLを作成
            sbSQL.Append("SELECT ")
            sbSQL.Append(" * ")
            sbSQL.Append(" FROM  M_SETTING ")
            sbSQL.Append(" WHERE  KEY_DIV = 'FUNCTION_CATEGORY' ")
            sbSQL.Append(" ORDER BY FUNC_CAT_SORT")
            'SQLを実行する
            lstFuncCatName = db.Setting.SqlQuery(sbSQL.ToString).ToList
            '一覧画面の機能分類名コンボ設定
            If String.IsNullOrEmpty(_ViewModel.SearchFuncCatName) Then
                ViewBag.FuncCatNameList = New SelectList(lstFuncCatName, "KEY1", "DATA1", String.Empty)
            Else
                ViewBag.FuncCatNameList = New SelectList(lstFuncCatName, "KEY1", "DATA1", _ViewModel.SearchFuncCatName)
            End If

            'モーダル画面の機能分類名コンボ設定
            '機能分類名1
            Dim SelectedCd1 As String = _ViewModel.FUNC_CAT_NAME1
            If String.IsNullOrEmpty(SelectedCd1) Then
                ViewBag.funcCatNameList1 = New SelectList(lstFuncCatName, "KEY1", "DATA1", String.Empty)
            Else
                ViewBag.funcCatNameList1 = New SelectList(lstFuncCatName, "KEY1", "DATA1", SelectedCd1)
            End If

            '機能分類名2
            Dim SelectedCd2 As String = _ViewModel.FUNC_CAT_NAME2
            If String.IsNullOrEmpty(SelectedCd2) Then
                ViewBag.funcCatNameList2 = New SelectList(lstFuncCatName, "KEY1", "DATA1", String.Empty)
            Else
                ViewBag.funcCatNameList2 = New SelectList(lstFuncCatName, "KEY1", "DATA1", SelectedCd2)
            End If

            '機能分類名3
            Dim SelectedCd3 As String = _ViewModel.FUNC_CAT_NAME3
            If String.IsNullOrEmpty(SelectedCd3) Then
                ViewBag.funcCatNameList3 = New SelectList(lstFuncCatName, "KEY1", "DATA1", String.Empty)
            Else
                ViewBag.funcCatNameList3 = New SelectList(lstFuncCatName, "KEY1", "DATA1", SelectedCd3)
            End If

        End Sub
#End Region

#Region "共通"

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New SettingViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            '_viewResult.ElementAt(0).KEY_DIV = _ViewModel.KEY_DIV
            '_viewResult.ElementAt(0).KEY1 = _ViewModel.KEY1
            '_viewResult.ElementAt(0).KEY2 = _ViewModel.KEY2
            '_viewResult.ElementAt(0).KEY3 = _ViewModel.KEY3
            '_viewResult.ElementAt(0).CsvPath = _ViewModel.CsvPath
            _viewResult.ElementAt(0).SearchKeyDiv = _ViewModel.SearchKeyDiv
            _viewResult.ElementAt(0).SearchFuncCatName = _ViewModel.SearchFuncCatName
            _viewResult.ElementAt(0).SearchSettingObjectDivHT = _ViewModel.SearchSettingObjectDivHT
            _viewResult.ElementAt(0).SearchSettingObjectDivPC = _ViewModel.SearchSettingObjectDivPC
            _viewResult.ElementAt(0).SearchSettingObjectDivWEB = _ViewModel.SearchSettingObjectDivWEB
            _viewResult.ElementAt(0).SearchSettingObjectDivALL = _ViewModel.SearchSettingObjectDivALL
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).CopyFlag = _ViewModel.CopyFlag
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.SETTING, "ALL").FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.SETTING, "ALL")

            If Not String.IsNullOrEmpty(_ViewModel.INS_EMPLOYEE_CD) Then
                _viewResult.ElementAt(0).INS_DATETIME = _ViewModel.INS_DATETIME
                _viewResult.ElementAt(0).INS_EMPLOYEE_CD = _ViewModel.INS_EMPLOYEE_CD
                _viewResult.ElementAt(0).INS_EMPLOYEE_NAME = _ViewModel.INS_EMPLOYEE_NAME
                _viewResult.ElementAt(0).UPD_DATETIME = _ViewModel.UPD_DATETIME
                _viewResult.ElementAt(0).UPD_EMPLOYEE_CD = _ViewModel.UPD_EMPLOYEE_CD
                _viewResult.ElementAt(0).UPD_EMPLOYEE_NAME = _ViewModel.UPD_EMPLOYEE_NAME
                _viewResult.ElementAt(0).UPD_TASK = _ViewModel.UPD_TASK
            End If


        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As SettingViewModel)
            _Model.KEY_DIV = model.KEY_DIV
            _Model.KEY1 = model.KEY1
            _Model.KEY2 = model.KEY2
            _Model.KEY3 = model.KEY3


            _Model.REMARK = model.REMARK

            _Model.DATA1 = model.DATA1
            _Model.DATA2 = model.DATA2
            _Model.DATA3 = model.DATA3
            _Model.DATA4 = model.DATA4
            _Model.DATA5 = model.DATA5
            _Model.DATA6 = model.DATA6
            _Model.DATA7 = model.DATA7
            _Model.DATA8 = model.DATA8
            _Model.DATA9 = model.DATA9
            _Model.DATA10 = model.DATA10

            _Model.FUNC_CAT_NAME1 = model.FUNC_CAT_NAME1
            _Model.FUNC_CAT_NAME2 = model.FUNC_CAT_NAME2
            _Model.FUNC_CAT_NAME3 = model.FUNC_CAT_NAME3
            _Model.SETTING_OBJECT_DIV = model.SETTING_OBJECT_DIV
            _Model.FUNC_CAT_SORT = model.FUNC_CAT_SORT
            _Model.UPD_AUTHORITY_FLG = model.UPD_AUTHORITY_FLG
            _Model.DEL_AUTHORITY_FLG = model.DEL_AUTHORITY_FLG

            _Model.INS_DATETIME = model.INS_DATETIME
            _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub
#End Region

#Region "削除チェック用検索処理"
        ''' <summary>
        ''' 削除チェック用検索処理
        ''' </summary>
        Private Function GetDelCheckData(ByVal delKeyDiv As String) As Integer
            Dim tbl As New DataTable
            Dim cnt As Integer = 0

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CSVMAP_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine(" SELECT ")
                sbSQL.AppendLine(" COUNT(*) AS CNT ")
                sbSQL.AppendLine(" FROM ")
                sbSQL.AppendLine(" M_SETTING ")
                sbSQL.AppendLine(" WHERE ")
                sbSQL.AppendLine(" KEY_DIV = '" & delKeyDiv & "' ")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                If Not IsNothing(tbl) Then
                    Dim intCheck As Integer
                    If Integer.TryParse(tbl.Rows(0).Item("CNT"), intCheck) = True Then
                        cnt = intCheck
                    End If
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return cnt
        End Function
#End Region

#Region "登録・更新時チェック用検索処理"
        ''' <summary>
        ''' 登録・更新時チェック用検索処理
        ''' </summary>
        Private Function GetInsUpdCheckData(ByVal funcCatName As String,
                                            ByVal funcCatSort As String,
                                            ByVal prmKeyDiv As String,
                                            ByVal prmKey1 As String,
                                            ByVal prmKey2 As String,
                                            ByVal prmKey3 As String) As Integer
            Dim tbl As New DataTable
            Dim cnt As Integer = 0

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CSVMAP_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine(" SELECT ")
                sbSQL.AppendLine(" COUNT(*) AS CNT ")
                sbSQL.AppendLine(" FROM ")
                sbSQL.AppendLine(" M_SETTING ")
                sbSQL.AppendLine(" WHERE ")
                sbSQL.AppendLine(" FUNC_CAT_NAME1 = '" & funcCatName & "' ")
                sbSQL.AppendLine(" AND ")
                sbSQL.AppendLine(" FUNC_CAT_SORT = '" & funcCatSort & "' ")
                sbSQL.AppendLine(" AND (")
                sbSQL.AppendLine(" KEY_DIV <> '" & prmKeyDiv & "' ")
                sbSQL.AppendLine(" OR KEY1 <> '" & prmKey1 & "' ")
                sbSQL.AppendLine(" OR KEY2 <> '" & prmKey2 & "' ")
                sbSQL.AppendLine(" OR KEY3 <> '" & prmKey3 & "' ")
                sbSQL.AppendLine(" )")


                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                If Not IsNothing(tbl) Then
                    Dim intCheck As Integer
                    If Integer.TryParse(tbl.Rows(0).Item("CNT"), intCheck) = True Then
                        cnt = intCheck
                    End If
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return cnt
        End Function
#End Region

#Region "削除エラーチェック処理"
        ''' <summary>
        ''' 削除エラーチェック処理
        ''' </summary>
        Public Function DelErrorCheck(ByVal deleteData As String()) As JsonResult
            Dim db As New SystemDBContext
            Dim viewModel As New SettingViewModel

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim groupValues = deleteData.GroupBy(Function(value) value).Select(Function(groupValue) New With {.Num = groupValue.Key, .Count = groupValue.Count()})

            For Each s In groupValues
                Dim settingDataCnt As Integer = GetDelCheckData(s.Num)
                '同じキー種別内で残り1つのデータの場合はエラーメッセージ表示
                If settingDataCnt <= s.Count Then
                    viewModel.ErrCode = CommonLabelSetting.MSG_ERR_ID_DATA_ONLY_ONE
                    Return Json(viewModel)
                End If
            Next

            '結果表示
            Return Json(viewModel)

        End Function
#End Region

    End Class
End Namespace