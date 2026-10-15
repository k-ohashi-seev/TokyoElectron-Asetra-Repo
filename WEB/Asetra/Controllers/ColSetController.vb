Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports System.IO
Imports System.Web.Mvc
Imports Asetra.Misc
Imports Asetra.Models
Imports Newtonsoft.Json

Namespace Controllers
    ''' <summary>
    ''' 項目配置マスタ画面用クラス
    ''' </summary>
    Public Class ColSetController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス変数"
        Private _Model As ColSetDetailModel = New ColSetDetailModel                                 '項目配置マスタ詳細モデル
        Private _ViewModel As ColSetDetailViewModel = New ColSetDetailViewModel                     '項目配置マスタ一覧表示用モデル
        Protected _viewResult As List(Of ColSetDetailViewModel) = New List(Of ColSetDetailViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _AuthInfo As AuthInfo = Nothing                 '権限情報
#End Region

#Region "クラス定数"
        Private ReadOnly noDesignatedLocDispIdList As New List(Of String) From {
                DispId.CATEGORY,
                DispId.DEPARTMENT,
                DispId.SETTING,
                DispId.WORD,
                DispId.CSV_MAPPING,
                DispId.GOODS,
                DispId.OPERATION_LOG
            }
#End Region

#Region "構造体"
        ''' <summary>
        ''' エラー時の画面再表示用
        ''' </summary>
        Private Class DisplayData
            ''' <summary>画面名</summary>
            Public DispDiv As String
            ''' <summary>固定列数</summary>
            Public FixColCnt As String
            ''' <summary>一覧表示用データ</summary>
            Public GridList As GridData
        End Class

        ''' <summary>
        ''' 一覧表示データ
        ''' </summary>
        Private Class GridData
            ''' <summary>項目名</summary>
            Public ColName As String
            ''' <summary>並び順</summary>
            Public SortOrder As String
            ''' <summary>表示フラグ</summary>
            Public DispFlg As String
        End Class

        ''' <summary>
        ''' 更新用リクエストデータ
        ''' </summary>
        Private Class ColSetUpdateRequest
            Public Property viewModels As List(Of ColSetDetailViewModel)
            Public Property fixColCnt As String
            Public Property dispDiv As String
            Public Property homeBaseLoc As String
            Public Property btnUpdateAllDisabled As Boolean
        End Class

        ''' <summary>
        ''' 検索用リクエストデータ
        ''' </summary>
        Private Class ColSetSearchRequest
            Public Property dispDiv As String
            Public Property homeBaseLoc As String
            Public Property isDisabled As Boolean
        End Class
#End Region

#Region "一覧表示処理"
        ''' <summary>
        ''' アクションメソッド(Index)
        ''' スーパークラスの一覧表示処理を呼出します
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <ValidateInput(False)>
        Public Function Index(ByVal ViewModel As ColSetDetailViewModel) As ActionResult

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As ColSetDetailViewModel = Session(CommonSessionKey.CREAGE_COLSET)
            If Not IsNothing(model) Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
                ViewBag.DispNameList = New SelectList(New List(Of SelectListItem), "Value", "Text", "")
                ViewBag.HomeBaseList = New SelectList(New List(Of SelectListItem), "Value", "Text", "")
                CopyModelData()
                Return View(_viewResult)
            Else
                '初期化処理
                InitializeIndex()

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo

                '権限情報の取得 
                Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ColSet, _LoginInfo)
                If authInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                    ViewBag.IsDropDownEnabled = True
                Else
                    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                    ViewBag.IsDropDownEnabled = False
                End If

                If authInfo.UpdHubAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthProcFlgAll = AuthProcFlg.All
                Else
                    ViewBag.AuthProcFlgAll = AuthProcFlg.ShokaiNomi
                End If

                Dim isSuperAdmin As Boolean = False
                If (authInfo.AuthDiv = AUTH_DIV.SuperAdmin) Then
                    isSuperAdmin = True
                End If

                '画面名コンボボックス設定
                DispNameDropDownListCreate(isSuperAdmin)

                '所属拠点(会社)コンボボックス設定
                DesignatedBaseDropDownListCreate()

                '一覧表示のクエリを作成
                ViewQuery()

                'SQLの実行
                Try
                    ViewExecQuery()
                Catch ex As Exception
                    logger.Error(ex.Message)
                    logger.Error(_SQL)

                    Throw
                End Try

                '後処理
                DisposeIndex()

            End If

            'エラー有の場合、表示するデータに画面データをマージする
            If _ViewModel.ErrCode <> String.Empty AndAlso _ViewModel.ErrCode <> CommonLabel.MSG_ERR_LOGININFO_EMPTY Then
                'セッションから取得
                Dim sessionData As List(Of ColSetDetailViewModel) = Session(CommonSessionKey.CREAGE_COLSET_DISPLAYDATA)
                _ViewModel.SearchDispDiv = sessionData(0).SearchDispDiv
                _ViewModel.SearchLocCd = sessionData(0).SearchLocCd
                _ViewModel.FixColCnt = sessionData(0).FixColCnt

                For i = 0 To sessionData.Count - 1
                    For Each detailModel As ColSetDetailViewModel In _viewResult
                        If sessionData(i).DB_COLUMN_NAME = detailModel.DB_COLUMN_NAME Then
                            detailModel.DISP_ORDER = sessionData(i).DISP_ORDER
                            detailModel.DISP_FLAG = sessionData(i).DISP_FLAG
                        End If
                    Next
                Next

                '並び順でソート
                _viewResult.Sort(Function(a, b) a.DISP_ORDER - b.DISP_ORDER)

                'CopyModelDataで固定列数をセットするため
                If (CommonRfid.IsNumber(sessionData(0).FixColCnt)) Then
                    _viewResult(0).FIX_COL_CNT = CommonRfid.ToNumber(sessionData(0).FixColCnt)
                Else
                    _viewResult(0).FIX_COL_CNT = Nothing
                End If
            End If

            '一覧情報にページ情報を設定
            CopyModelData()

            '画面データの内容セッションを初期化
            Session.Remove(CommonSessionKey.CREAGE_COLSET_DISPLAYDATA)

            ' ページ表示
            Return View(_viewResult)

        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()
            'ビュー側でセッションを使わないので一覧表示時にクリアする
            Session.Remove(CommonSessionKey.CREAGE_COLSET)

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.Append("SELECT ")
            sbSQL.Append("  COL_DETAIL.COLUMN_NAME AS DB_COLUMN_NAME ")
            sbSQL.Append("  , COL_DETAIL.DISP_ORDER AS DISP_ORDER ")
            sbSQL.Append("  , COL_DETAIL.DISP_FLAG AS DISP_FLAG ")
            sbSQL.Append("  , WORD.WORDING AS COL_NAME ")
            sbSQL.Append("  , COL.FIX_COL_FLG AS FIX_COL_FLG ")
            sbSQL.Append("  , COL.FIX_COL_CNT AS FIX_COL_CNT ")
            sbSQL.Append("  ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.Append("FROM")
            sbSQL.Append("  M_COL_SET_DETAIL COL_DETAIL ")
            sbSQL.Append("  INNER JOIN M_COL_SET COL ")
            sbSQL.Append("    ON COL.DISP_ID = COL_DETAIL.DISP_ID ")
            sbSQL.Append("    AND COL.LOC_L_CD = COL_DETAIL.LOC_L_CD  ")
            sbSQL.Append("  LEFT JOIN M_WORD WORD ")
            sbSQL.Append($"   ON WORD.LANG_DIV = '{LangDiv.LU}' ")
            sbSQL.Append("        AND COL_DETAIL.TOOL_DIV = WORD.TOOL_DIV ")
            sbSQL.Append("    AND COL_DETAIL.WORD_DIV = WORD.WORD_DIV ")
            sbSQL.Append("    AND COL_DETAIL.WORD_CD = WORD.WORD_CD ")
            sbSQL.Append("WHERE")
            sbSQL.Append("  1 = 1 ")

            '検索条件を設定する
            'CSV区分
            sbSQL.Append("AND COL.DISP_ID = @DispDiv ")
            If String.IsNullOrEmpty(_ViewModel.SearchDispDiv) Then
                addSqlParam("DispDiv", $"{DispId.ASSET_LIST}")
            Else
                addSqlParam("DispDiv", $"{_ViewModel.SearchDispDiv}")
            End If

            '所属拠点
            sbSQL.Append("AND COL_DETAIL.LOC_L_CD = @LocCd ")
            addSqlParam("LocCd", $"{_ViewModel.SearchLocCd}")

            sbSQL.Append("ORDER BY COL_DETAIL.DISP_ORDER")

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of ColSetDetailViewModel) = _db.ColSetDetailView.SqlQuery(_SQL, _paramSqlList.ToArray)

            '画面に表示するViewを設定
            _viewResult = query.ToList

        End Sub

#End Region

#Region "画面名コンボボックス設定処理"
        ''' <summary>
        ''' 画面名コンボボックス設定処理
        ''' </summary>
        Public Sub DispNameDropDownListCreate(isSuperAdmin As Boolean)

            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'コンボボックスの値設定
            Dim lstCsvDiv As List(Of SelectListItem) = GetDispNameDropSrc(isSuperAdmin)

            Dim selectDispDiv As String = _ViewModel.SearchDispDiv
            If (selectDispDiv Is Nothing) Then
                selectDispDiv = DispId.ASSET_LIST
            End If
            '一覧画面の画面名コンボ設定
            ViewBag.DispNameList = New SelectList(lstCsvDiv, "Value", "Text", selectDispDiv)

        End Sub

        ''' <summary>
        ''' 画面名コンボボックス設定用のデータソースを取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetDispNameDropSrc(isSuperAdmin As Boolean) As List(Of SelectListItem)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As New List(Of SqlParameter)

            '一覧表示表用SQL
            sbSQL.Append("SELECT")
            sbSQL.Append("  COL.DISP_ID AS DISP_ID ")
            sbSQL.Append("  , MIN(WORD.WORDING) AS COL_NAME ")
            sbSQL.Append("FROM")
            sbSQL.Append("  M_COL_SET COL ")
            sbSQL.Append("  LEFT JOIN M_WORD WORD ")
            sbSQL.Append($"   ON WORD.LANG_DIV = '{LangDiv.LU}' ")
            sbSQL.Append("    AND COL.TOOL_DIV = WORD.TOOL_DIV ")
            sbSQL.Append("    AND COL.WORD_DIV = WORD.WORD_DIV ")
            sbSQL.Append("    AND COL.WORD_CD = WORD.WORD_CD ")
            If (Not isSuperAdmin) Then
                Dim line As String = String.Empty
                For Each noDesignatedLocDispId In noDesignatedLocDispIdList
                    If line <> String.Empty Then
                        line &= ", "
                    End If
                    line &= $"'{noDesignatedLocDispId}'"
                Next

                sbSQL.Append($"  WHERE COL.DISP_ID NOT IN ({line}) ")
            End If
            sbSQL.Append("GROUP BY COL.DISP_ID ")
            sbSQL.Append("ORDER BY MIN(COL.DISP_ORDER) ")

            'SQL実行
            Dim lstColSet As List(Of ColSetDropdownModel) = _db.ColSetDropdown.SqlQuery(sbSQL.ToString).ToList

            '取得内容をドロップダウン用の形式に変換
            Dim lstSelItems As New List(Of SelectListItem)
            For Each row In lstColSet
                lstSelItems.Add(New SelectListItem() With {.Value = row.DISP_ID, .Text = row.COL_NAME})
            Next

            Return lstSelItems
        End Function
#End Region

#Region "所属拠点(会社)コンボボックス設定処理"

        <HttpPost>
        Public Function GetHomeBaseList(searchDispDiv As String) As JsonResult
            _ViewModel.SearchDispDiv = searchDispDiv

            DesignatedBaseDropDownListCreate()

            Dim list As IEnumerable(Of SelectListItem) = DirectCast(ViewBag.HomeBaseList, IEnumerable(Of SelectListItem))
            Return Json(list, JsonRequestBehavior.AllowGet)
        End Function

        ''' <summary>
        ''' 所属拠点(会社)コンボボックス設定処理
        ''' </summary>
        Public Sub DesignatedBaseDropDownListCreate()

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Exit Sub
            End If

            '権限情報の取得
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ColSet, _LoginInfo)

            '所属拠点(会社)コンボボックス設定
            Dim designatedLoc As New List(Of SelectListItem)
            Dim divData = New SelectListItem()

            If (_ViewModel Is Nothing) Then
                Exit Sub
            End If

            If (noDesignatedLocDispIdList.Contains(_ViewModel.SearchDispDiv)) Then
                '拠点指定なし画面の場合
                divData.Value = COLSET_LOC.ALL
                divData.Text = Me._Message.getMessage(CommonLabelColSet.LABEL_HOME_BASE_ALL)
                designatedLoc.Add(divData)
                _ViewModel.SearchLocCd = COLSET_LOC.ALL
                ViewBag.SearchLocCd = COLSET_LOC.ALL
            Else
                '拠点指定あり画面の場合

                'ログイン社員の所属拠点データ取得
                Dim tblEmpHub As DataTable = Dao.ToolLocation.GetLocationLData(_LoginInfo, _AuthInfo)
                For Each row As DataRow In tblEmpHub.Rows
                    divData = New SelectListItem()
                    divData.Value = row("LOC_CD").ToString()
                    divData.Text = row("LOC_NAME").ToString()
                    designatedLoc.Add(divData)
                Next

                If (_AuthInfo.UpdHubAuthFlg = AUTH_FLG.Yes OrElse _AuthInfo.ReadHubAuthFlg = AUTH_FLG.Yes) Then
                    '権限がある場合は「テンプレート」も追加
                    divData = New SelectListItem()
                    divData.Value = COLSET_LOC.TEMPLATE
                    divData.Text = Me._Message.getMessage(CommonLabelColSet.LABEL_TEMPLATE)
                    designatedLoc.Insert(0, divData)
                End If

                If (_ViewModel.SearchLocCd Is Nothing) Then
                    _ViewModel.SearchLocCd = designatedLoc(0).Value
                    ViewBag.SearchLocCd = designatedLoc(0).Value
                End If

            End If

            ViewBag.HomeBaseList = New SelectList(designatedLoc, "Value", "Text", _ViewModel.SearchLocCd)
            Exit Sub
        End Sub
#End Region

#Region "検索処理"
        <HttpPost>
        Public Function Search() As JsonResult

            Dim jsonString As String
            Using reader As New StreamReader(Request.InputStream)
                jsonString = reader.ReadToEnd()
            End Using

            Dim data As ColSetSearchRequest = JsonConvert.DeserializeObject(Of ColSetSearchRequest)(jsonString)

            _ViewModel.SearchDispDiv = data.dispDiv
            _ViewModel.SearchLocCd = data.homeBaseLoc

            Try
                ViewQuery()
                ViewExecQuery()
                Return Json("")
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(_SQL)
                Return Json("")
            End Try
        End Function
#End Region

#Region "更新処理"
        ''' <summary>
        ''' 更新処理
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Function Update() As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return Json("")     'View側のSubmitでリダイレクト
            End If

            Dim jsonString As String
            Using reader As New StreamReader(Request.InputStream)
                jsonString = reader.ReadToEnd()
            End Using

            ' JSON → ColSetUpdateRequest に変換
            Dim data As ColSetUpdateRequest = JsonConvert.DeserializeObject(Of ColSetUpdateRequest)(jsonString)
            'モデルを設定
            data.viewModels(0).FixColCnt = data.fixColCnt
            data.viewModels(0).SearchDispDiv = data.dispDiv
            data.viewModels(0).SearchLocCd = data.homeBaseLoc
            _ViewModel = data.viewModels(0)
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '入力チェック
            If updInputCheck(data.viewModels, data.fixColCnt) = False Then
                'モデルの内容をセッションに保持 
                Session(CommonSessionKey.CREAGE_COLSET) = _ViewModel
                Session(CommonSessionKey.CREAGE_COLSET_DISPLAYDATA) = data.viewModels
                Return Json("")     'View側のSubmitでリダイレクト
            End If

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)
            Dim sTaskNm As String = "ColSetController.Update"

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try
                    '項目配置マスタ更新
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("UPDATE M_COL_SET SET")
                    sbSQL.AppendLine($"  FIX_COL_CNT = {data.fixColCnt}")
                    sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

                    sbSQL.AppendLine("WHERE")
                    sbSQL.AppendLine(" DISP_ID = @DISP_ID")
                    params.Add(New SqlParameter($"@DISP_ID", data.dispDiv))

                    sbSQL.AppendLine(" AND LOC_L_CD = @LOC_L_CD")
                    params.Add(New SqlParameter($"@LOC_L_CD", data.homeBaseLoc))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '項目配置詳細マスタ
                    For i As Integer = 0 To data.viewModels.Count - 1
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("UPDATE M_COL_SET_DETAIL SET")
                        sbSQL.AppendLine($"  DISP_ORDER = {data.viewModels(i).DISP_ORDER}")
                        sbSQL.AppendLine($" ,DISP_FLAG = '{data.viewModels(i).DISP_FLAG}'")
                        sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

                        sbSQL.AppendLine("WHERE")
                        sbSQL.AppendLine(" DISP_ID = @DISP_ID")
                        sbSQL.AppendLine(" AND COLUMN_NAME = @COLUMN_NAME")
                        sbSQL.AppendLine(" AND LOC_L_CD = @LOC_L_CD")

                        params.Add(New SqlParameter($"@DISP_ID", data.dispDiv))
                        params.Add(New SqlParameter($"@COLUMN_NAME", data.viewModels(i).DB_COLUMN_NAME))
                        params.Add(New SqlParameter($"@LOC_L_CD", data.homeBaseLoc))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)
                    Next

                    tran.Commit()

                    '正常終了ダイアログ
                    _ViewModel.States = StatusType.SuccessEdit

                    _ViewModel.ErrCode = ""
                    _ViewModel.ErrTarget = ""

                    'モデルの内容をセッションに保持
                    Session(CommonSessionKey.CREAGE_COLSET) = _ViewModel
                    Session(CommonSessionKey.CREAGE_COLSET_DISPLAYDATA) = data.viewModels

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelColSet.TITLE) & " " &
                                         Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                         Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                         data.viewModels.Count.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelColSet.TITLE))

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)

                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using

            Return Json("")     'View側のSubmitでリダイレクト
        End Function

        ''' <summary>
        ''' 更新処理(全ての所属拠点)
        ''' </summary>
        ''' <returns></returns>
        <ValidateInput(False)>
        Function UpdateAllLoc() As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return Json("")     'View側のSubmitでリダイレクト
            End If

            Dim jsonString As String
            Using reader As New StreamReader(Request.InputStream)
                jsonString = reader.ReadToEnd()
            End Using

            ' JSON → ColSetUpdateRequest に変換
            Dim data As ColSetUpdateRequest = JsonConvert.DeserializeObject(Of ColSetUpdateRequest)(jsonString)
            'モデルを設定
            data.viewModels(0).FixColCnt = data.fixColCnt
            data.viewModels(0).SearchDispDiv = data.dispDiv
            data.viewModels(0).SearchLocCd = data.homeBaseLoc
            _ViewModel = data.viewModels(0)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '入力チェック
            If updInputCheck(data.viewModels, data.fixColCnt) = False Then
                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_COLSET) = _ViewModel
                Session(CommonSessionKey.CREAGE_COLSET_DISPLAYDATA) = data.viewModels
                Return Json("")     'View側のSubmitでリダイレクト
            End If

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)
            Dim sTaskNm As String = "ColSetController.Update"

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try
                    '項目配置マスタ更新
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("UPDATE M_COL_SET SET")
                    sbSQL.AppendLine($"  FIX_COL_CNT = {data.fixColCnt}")
                    sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

                    sbSQL.AppendLine("FROM M_COL_SET CS")
                    sbSQL.AppendLine("WHERE")
                    sbSQL.AppendLine(" DISP_ID = @DISP_ID")
                    params.Add(New SqlParameter($"@DISP_ID", data.dispDiv))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    '項目配置詳細マスタ
                    For i As Integer = 0 To data.viewModels.Count - 1
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("UPDATE M_COL_SET_DETAIL SET")
                        sbSQL.AppendLine($"  DISP_ORDER = {data.viewModels(i).DISP_ORDER}")
                        sbSQL.AppendLine($" ,DISP_FLAG = '{data.viewModels(i).DISP_FLAG}'")
                        sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

                        sbSQL.AppendLine("FROM M_COL_SET_DETAIL CSD")
                        sbSQL.AppendLine("WHERE")
                        sbSQL.AppendLine(" DISP_ID = @DISP_ID")
                        sbSQL.AppendLine(" AND COLUMN_NAME = @COLUMN_NAME")

                        params.Add(New SqlParameter($"@DISP_ID", data.dispDiv))
                        params.Add(New SqlParameter($"@COLUMN_NAME", data.viewModels(i).DB_COLUMN_NAME))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)
                    Next

                    tran.Commit()

                    '正常終了ダイアログ
                    _ViewModel.States = StatusType.SuccessEdit

                    _ViewModel.ErrCode = ""
                    _ViewModel.ErrTarget = ""

                    'モデルの内容をセッションに保持
                    Session(CommonSessionKey.CREAGE_COLSET) = _ViewModel
                    Session(CommonSessionKey.CREAGE_COLSET_DISPLAYDATA) = data.viewModels

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelColSet.TITLE) & " " &
                                         Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                         Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                         data.viewModels.Count.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelColSet.TITLE))

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)

                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using

            Return Json("")     'View側のSubmitでリダイレクト
        End Function

#End Region

#Region "入力チェック処理"
        ''' <summary>
        ''' 入力チェック処理
        ''' </summary>
        ''' <param name="updModel">更新対象データ</param>
        ''' <param name="fixColCnt">固定列数</param>
        ''' <returns></returns>
        Private Function updInputCheck(ByVal updModel As List(Of ColSetDetailViewModel), ByVal fixColCnt As String) As Boolean

            '並び順重複チェック用Hash
            Dim hashUniqueOrders As New HashSet(Of Integer)()

            '◆並び順
            For Each model In updModel
                If String.IsNullOrEmpty(model.DISP_ORDER.ToString) OrElse model.DISP_ORDER = 0 Then
                    '未入力チェック
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_DATAEMPTY
                    _ViewModel.ErrTarget = "DISP_ORDER"
                    Return False
                ElseIf model.DISP_ORDER.ToString.Length > itemlength.DispOrder Then
                    '項目長チェック
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_DATAOVER
                    _ViewModel.ErrTarget = "DISP_ORDER"
                    Return False
                ElseIf CommonRfid.ContainsFullWidth(model.DISP_ORDER.ToString) Then
                    '全角文字チェック
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_INPUT_HANKAKU
                    _ViewModel.ErrTarget = "DISP_ORDER"
                    Return False
                ElseIf Not CommonRfid.IsHankakuNumber(model.DISP_ORDER.ToString) Then
                    '半角数字チェック
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_INPUT_INTEGER
                    _ViewModel.ErrTarget = "DISP_ORDER"
                    Return False
                ElseIf Not hashUniqueOrders.Add(model.DISP_ORDER) Then
                    '重複チェック
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_DUPLICATE_DISP_ORDER
                    _ViewModel.ErrTarget = "DISP_ORDER"
                    Return False
                End If
            Next

            '◆表示
            '表示フラグONのレコード数を取得
            Dim iVisibleColCnt As Integer = updModel.Where(Function(x) x.DISP_FLAG = DispFlag.VISIBLE).Count

            If iVisibleColCnt <= 0 Then
                '表示列なし
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_NOT_EXISTS_DISP_ITEM
                _ViewModel.ErrTarget = "DISP_FLAG"
                Return False
            End If

            '◆固定列数
            If String.IsNullOrEmpty(fixColCnt) Then
                '未入力チェック
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_DATAEMPTY
                _ViewModel.ErrTarget = "txtFilColCnt"
                Return False
            ElseIf CommonRfid.ContainsFullWidth(fixColCnt) Then
                '全角文字チェック
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_INPUT_HANKAKU
                _ViewModel.ErrTarget = "txtFilColCnt"
                Return False
            ElseIf Not CommonRfid.IsMinusHankakuNumber(fixColCnt) Then
                '半角数字チェック
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_FIX_COL_CNT
                _ViewModel.ErrTarget = "txtFilColCnt"
                Return False
            ElseIf CInt(fixColCnt) < 0 Then
                '下限値チェック
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_FIX_COL_CNT
                _ViewModel.ErrTarget = "txtFilColCnt"
                Return False
            ElseIf iVisibleColCnt < CInt(fixColCnt) Then
                '固定可能チェック
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_DISP_OVER_FIX_COL_CNT
                _ViewModel.ErrTarget = "txtFilColCnt"
                Return False
            End If

            Return True
        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New ColSetDetailViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).FixColCnt = _viewResult.ElementAt(0).FIX_COL_CNT.ToString
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
        End Sub
#End Region

#Region "不要Overridesメソッド"
        Protected Overrides Sub CreateQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub DeleteQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub EditQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Function CreateChcek() As Boolean
            Throw New NotImplementedException()
        End Function

        Protected Overrides Function DeleteChcek() As Boolean
            Throw New NotImplementedException()
        End Function

        Protected Overrides Function EditChcek() As Boolean
            Throw New NotImplementedException()
        End Function
#End Region

    End Class
End Namespace