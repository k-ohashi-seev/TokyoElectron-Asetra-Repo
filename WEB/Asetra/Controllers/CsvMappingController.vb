Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Threading
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers
    ''' <summary>
    ''' CSVマッピング画面表示用のクラス
    ''' </summary>
    Public Class CsvMappingController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス変数"
        Private _Model As CsvMapDetailModel = New CsvMapDetailModel                                 'CsvMapDetailモデル
        Private _ViewModel As CsvMapDetailViewModel = New CsvMapDetailViewModel                     'CsvMapDetail一覧表示用モデル
        Protected _viewResult As List(Of CsvMapDetailViewModel) = New List(Of CsvMapDetailViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
#End Region

#Region "構造体"
        ''' <summary>
        ''' エラー時の画面再表示用
        ''' </summary>
        Private Class DisplayData
            ''' <summary>CSV連番</summary>
            Public CsvNo As String
            ''' <summary>指定方法</summary>
            Public SpecifiedDiv As String
            ''' <summary>列番号</summary>
            Public CsvColNo As String
            ''' <summary>固定値</summary>
            Public CsvFixVal As String
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
        Public Function Index(ByVal ViewModel As CsvMapDetailViewModel) As ActionResult

            ''登録・変更画面からのリダイレクト以外、初期化する
            'If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
            '    Session.Remove(CommonSessionKey.CREAGE_CSVMAPPING)
            'End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As CsvMapDetailViewModel = Session(CommonSessionKey.CREAGE_CSVMAPPING)
            If Not IsNothing(model) Then
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
                'MyBase.BaseIndex(_ViewModel)   'ページングされるので基底クラスを使わないようにする
                '初期化処理
                InitializeIndex()

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


                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            'CSV区分コンボボックス設定
            CsvDivDropDownListCreate()

            'エラー有の場合、表示するデータに画面データをマージする
            Dim isError As Boolean = False
            If _ViewModel.ErrCode <> String.Empty AndAlso _ViewModel.ErrCode <> CommonLabel.MSG_ERR_LOGININFO_EMPTY Then
                'CSVNoの内容をセッションから取得
                Dim lstData As List(Of DisplayData) = Session(CommonSessionKey.CREAGE_CSVMAPPING_DISPLAYDATA)
                For icnt = 0 To lstData.Count - 1
                    For Each modelcsv As CsvMapDetailViewModel In _viewResult
                        If modelcsv.CSV_NO = lstData(icnt).CsvNo Then
                            modelcsv.SPECIFIED_DIV = lstData(icnt).SpecifiedDiv
                            If lstData(icnt).CsvColNo = String.Empty Then
                                modelcsv.CSV_COL_NO = Nothing
                            Else
                                modelcsv.CSV_COL_NO = lstData(icnt).CsvColNo
                            End If
                            modelcsv.CSV_FIX_VAL = lstData(icnt).CsvFixVal
                        End If
                    Next
                Next

                isError = True
            End If

            '文字コードコンボボックス設定
            DropDownListCreate(isError)

            '一覧情報にページ情報を設定
            CopyModelData()

            '画面データの内容セッションを初期化
            Session.Remove(CommonSessionKey.CREAGE_CSVMAPPING_DISPLAYDATA)

            ' ページ表示
            Return View(_viewResult)

            ''結果表示
            'Return View(_viewResult)

        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()
            'エラーでない場合、セッションクリア
            'If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
            'Session.Remove(CommonSessionKey.CREAGE_CSVMAPPING)
            'End If
            'ビュー側でセッションを使わないので一覧表示時にクリアする
            Session.Remove(CommonSessionKey.CREAGE_CSVMAPPING)

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.Append("SELECT ")
            sbSQL.Append("  CSV_DETAIL.CSV_DIV ")
            sbSQL.Append("  , CSV_DETAIL.CSV_NO ")
            sbSQL.Append("  , CSV_DETAIL.TOOL_DIV ")
            sbSQL.Append("  , CSV_DETAIL.WORD_DIV ")
            sbSQL.Append("  , CSV_DETAIL.WORD_CD ")
            sbSQL.Append("  , CSV_DETAIL.REQUIRED_ITEM_FLAG ")
            sbSQL.Append("  , CSV_DETAIL.SPECIFIED_DIV ")
            sbSQL.Append("  , CSV_DETAIL.SPEC_CAN_DIV ")        '20230203 ADD Shirakawa
            sbSQL.Append("  , CSV_DETAIL.CSV_COL_NO ")
            sbSQL.Append("  , CSV_DETAIL.CSV_FIX_VAL ")
            sbSQL.Append("  , CSV_DETAIL.INS_DATETIME ")
            sbSQL.Append("  , CSV_DETAIL.INS_EMPLOYEE_CD ")
            sbSQL.Append("  , CSV_DETAIL.INS_EMPLOYEE_NAME ")
            sbSQL.Append("  , CSV_DETAIL.UPD_DATETIME ")
            sbSQL.Append("  , CSV_DETAIL.UPD_EMPLOYEE_CD ")
            sbSQL.Append("  , CSV_DETAIL.UPD_EMPLOYEE_NAME ")
            sbSQL.Append("  , CSV_DETAIL.UPD_TASK ")
            sbSQL.Append("  , WORD.WORDING AS CSV_KOUMOKU ")
            sbSQL.Append("  ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_CSVMAP_DETAIL CSV_DETAIL ")
            sbSQL.Append("  LEFT JOIN M_WORD WORD ")
            sbSQL.Append("    ON WORD.LANG_DIV = 'LU' ")
            sbSQL.Append("        AND CSV_DETAIL.TOOL_DIV = WORD.TOOL_DIV ")
            sbSQL.Append("    AND CSV_DETAIL.WORD_DIV = WORD.WORD_DIV ")
            sbSQL.Append("    AND CSV_DETAIL.WORD_CD = WORD.WORD_CD ")
            sbSQL.Append("WHERE ")
            sbSQL.Append("  1 = 1 ")

            '検索条件を設定する
            'CSV区分
            sbSQL.Append("AND CSV_DIV = @CsvDiv")
            If String.IsNullOrEmpty(_ViewModel.SearchCsvDiv) Then
                addSqlParam("CsvDiv", $"{CsvDiv.Asset}")
            Else
                addSqlParam("CsvDiv", $"{_ViewModel.SearchCsvDiv}")
            End If

            sbSQL.Append(" ORDER BY CSV_DIV,CSV_NO")

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of CsvMapDetailViewModel) = _db.CsvMapDetailView.SqlQuery(_SQL, _paramSqlList.ToArray)

            '画面に表示するViewを設定
            _viewResult = query.ToList

        End Sub

#End Region

#Region "ボタン処理"
        ''' <summary>
        ''' ボタン処理
        ''' </summary>
        ''' <param name="LIST_CSV_NO">CSV連番</param>
        ''' <param name="LIST_CSV_DIV">CSV区分</param>
        ''' <param name="LIST_KOUMOKU">CSV項目</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Function Buttom(ByVal ViewModel As CsvMapDetailViewModel, ByVal LIST_CSV_NO As String(), ByVal LIST_CSV_DIV As String(), ByVal LIST_KOUMOKU As String()) As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index")
            End If

            'モデルを設定
            _ViewModel = ViewModel

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '画面の値を取得
            Dim lstDisplayData As List(Of DisplayData) = New List(Of DisplayData)
            Dim lstColNo = New List(Of String)  '登録時の列番号重複チェック用
            Dim isError As Boolean = False
            For i = 0 To LIST_CSV_NO.Length - 1
                Dim dataLine As DisplayData = New DisplayData
                dataLine.CsvNo = LIST_CSV_NO(i)
                dataLine.SpecifiedDiv = Request.Params($"SPECIFIED_DIV{LIST_CSV_NO(i)}")
                dataLine.CsvColNo = Request.Params($"CSV_COL_NO{LIST_CSV_NO(i)}")
                dataLine.CsvFixVal = Request.Params($"CSV_FIX_VAL{LIST_CSV_NO(i)}")
                lstDisplayData.Add(dataLine)

                'エラーチェック
                If isError = False Then
                    'エラーフラグがオフの時にチェック
                    If CheckError(dataLine.CsvNo, dataLine.SpecifiedDiv, dataLine.CsvColNo, dataLine.CsvFixVal, lstColNo) Then
                        'エラーがあればエラーフラグをオンにする
                        isError = True
                    End If
                End If

            Next

            ''文字コードコンボボックスチェック  '2019/12/20 miyaji エラーメッセージをコントローラーで設定するなら活かす
            'If String.IsNullOrEmpty(_ViewModel.CHAR_CD) Then
            '    _ViewModel.ErrCode = CommonLabel.MSG_ERR_DATAEMPTY
            '    _ViewModel.ErrTarget = "CHAR_CD"
            '    isError = True
            'End If

            'モデルの検索項目(CsvDiv)に更新するCsv区分を設定。(コンボボックスが変更されている場合があるので)
            _ViewModel.SearchCsvDiv = LIST_CSV_DIV(0)

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_CSVMAPPING) = _ViewModel

            '画面データの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_CSVMAPPING_DISPLAYDATA) = lstDisplayData

            If isError Then
                Return RedirectToAction("Index")
            End If

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)
            Dim sTaskNm As String = "CsvMappingController.Buttom"

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try
                    'CSVマッピングマスタ更新
                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine("UPDATE M_CSVMAP SET")
                    sbSQL.AppendLine("   CHAR_CD = '" & _ViewModel.CHAR_CD & "'")
                    sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

                    sbSQL.AppendLine("WHERE CSV_DIV = @CSV_DIV")

                    '--2021/09/10 nv miyaji update start
                    params.Add(New SqlParameter($"@CSV_DIV", _ViewModel.SearchCsvDiv))
                    'params.Add(New SqlParameter($"@CSV_DIV", _ViewModel.CSV_DIV))
                    '--2021/09/10 nv miyaji update end

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)


                    'CSVマッピング明細マスタ更新
                    For i = 0 To LIST_CSV_NO.Length - 1

                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("UPDATE M_CSVMAP_DETAIL SET")
                        sbSQL.AppendLine("   SPECIFIED_DIV = '" & lstDisplayData(i).SpecifiedDiv & "'")
                        sbSQL.AppendLine("  ,CSV_COL_NO = '" & lstDisplayData(i).CsvColNo & "'")
                        sbSQL.AppendLine("  ,CSV_FIX_VAL = '" & lstDisplayData(i).CsvFixVal & "'")

                        sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

                        sbSQL.AppendLine("WHERE CSV_DIV = @CSV_DIV")
                        sbSQL.AppendLine("AND CSV_NO = @CSV_NO")

                        params.Add(New SqlParameter($"@CSV_DIV", LIST_CSV_DIV(i)))
                        params.Add(New SqlParameter($"@CSV_NO", LIST_CSV_NO(i)))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    Next

                    tran.Commit()

                    '正常終了ダイアログ
                    _ViewModel.States = StatusType.SuccessEdit

                    'モデルの内容をセッションに保持
                    Session(CommonSessionKey.CREAGE_CSVMAPPING) = _ViewModel

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelCsvMap.TITLE) & " " &
                                         Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                         Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                         LIST_CSV_NO.Length.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelCsvMap.TITLE))

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)

                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using

            Return RedirectToAction("Index")
            'Return RedirectToAction("Index")


        End Function
#End Region

#Region "エラーチェック"
        ''' <summary>
        ''' エラーチェック
        ''' </summary>
        ''' <remarks>エラー有:true エラー無:false</remarks>
        Private Function CheckError(sCsvNo As String, sShiteihouhou As String, sColIndex As String, sFixVal As String, lstColNo As List(Of String)) As Boolean
            '列番号
            If sShiteihouhou = CsvMapping_SpecifiedDiv.Index Then
                '空白チェック・0チェック
                If String.IsNullOrEmpty(sColIndex) OrElse CInt(sColIndex) = 0 Then
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_DATAEMPTY
                    _ViewModel.ErrTarget = "CSV_COL_NO" + sCsvNo
                    'ModelState.AddModelError("CSV_COL_NO", CommonLabel.MSG_ERR_DATAEMPTY)
                    Return True
                ElseIf sColIndex.Length > itemlength.ColNo Then
                    '項目長チェック
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_DATAOVER
                    _ViewModel.ErrTarget = "CSV_COL_NO" + sCsvNo
                    'ModelState.AddModelError("CSV_COL_NO", CommonLabel.MSG_ERR_DATAOVER)
                    Return True
                ElseIf Not CommonRfid.IsNumber(sColIndex) Then
                    '数字チェック
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_DATASEIYAKU
                    _ViewModel.ErrTarget = "CSV_COL_NO" + sCsvNo
                    'ModelState.AddModelError("CSV_COL_NO", CommonLabel.MSG_ERR_DATASEIYAKU)
                    Return True
                ElseIf lstColNo.IndexOf(sColIndex) > -1 Then
                    '重複チェック
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_DATAJUUFUKU
                    _ViewModel.ErrTarget = "CSV_COL_NO" + sCsvNo
                    'ModelState.AddModelError("CSV_COL_NO", CommonLabel.MSG_ERR_DATAJUUFUKU & String.Format(Me._Message.getMessage(CommonLabel.LABEL_EMPLOYEEID) & "[{0}]", sColIndex))
                    Return True
                End If
                lstColNo.Add(sColIndex)
            End If

            '固定値
            If sShiteihouhou = CsvMapping_SpecifiedDiv.Kotei Then
                '空白チェック
                If String.IsNullOrEmpty(sFixVal) Then
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_DATAEMPTY
                    _ViewModel.ErrTarget = "CSV_FIX_VAL" + sCsvNo
                    'ModelState.AddModelError("CSV_FIX_VAL", CommonLabel.MSG_ERR_DATAEMPTY)
                    Return True
                ElseIf sFixVal.Length > itemlength.FixVal Then
                    '項目長チェック
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_DATAOVER
                    _ViewModel.ErrTarget = "CSV_FIX_VAL" + sCsvNo
                    'ModelState.AddModelError("CSV_FIX_VAL", CommonLabel.MSG_ERR_DATAOVER)
                    Return True
                End If
            End If

            Return False
        End Function
#End Region

#Region "CSV区分コンボボックス設定処理"
        ''' <summary>
        ''' CSV区分コンボボックス設定処理
        ''' </summary>
        Public Sub CsvDivDropDownListCreate()

            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'コンボボックスの値設定
            Dim lstCsvDiv As List(Of SelectListItem) = New List(Of SelectListItem)
            lstCsvDiv.Add(New SelectListItem() With {.Value = CsvDiv.Asset, .Text = Setting.getMessage(CommonLabel.VALUE_CSV_DIV_ASSET)})
            lstCsvDiv.Add(New SelectListItem() With {.Value = CsvDiv.Employee, .Text = Setting.getMessage(CommonLabel.VALUE_CSV_DIV_EMPLOYEE)})

            Dim settingInfo As SettingInfo = Setting.getSettingInfo(CommonSettingKey.GoodsReflect.KeyDiv, CommonSettingKey.GoodsReflect.Key1, CommonSettingKey.GoodsReflect.Key2, CommonSettingKey.GoodsReflect.Key3)
            If Not settingInfo Is Nothing Then
                Dim sValidFlg As String = settingInfo.DATA1.Trim
                If sValidFlg = ValidFlg.Cd.Valid Then
                    lstCsvDiv.Add(New SelectListItem() With {.Value = CsvDiv.Goods, .Text = Setting.getMessage(CommonLabel.VALUE_CSV_DIV_GOODS)})
                End If
            End If

            lstCsvDiv.Add(New SelectListItem() With {.Value = CsvDiv.Construction, .Text = Setting.getMessage(CommonLabel.VALUE_CSV_DIV_CONSTRUCTION)})

            '一覧画面のCSVコンボ設定
            ViewBag.CsvDivMapList = New SelectList(lstCsvDiv, "Value", "Text", _ViewModel.SearchCsvDiv)

        End Sub
#End Region

#Region "文字コードコンボボックス設定処理"
        ''' <summary>
        ''' 文字コードコンボボックス設定処理
        ''' </summary>
        ''' <param name="isError">エラーフラグ</param>
        Public Sub DropDownListCreate(isError As Boolean)

            '出力ファイル名取得
            Dim list As List(Of SettingInfo) = New List(Of SettingInfo) '並び替えのため
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim UTFData As SettingInfo = Setting.getSettingInfo(CommonSettingKey.CsvMapUtf.KeyDiv, CommonSettingKey.CsvMapUtf.Key1, CommonSettingKey.CsvMapUtf.Key2, CommonSettingKey.CsvMapUtf.Key3)
            Dim ShiftJisData As SettingInfo = Setting.getSettingInfo(CommonSettingKey.CsvMapShiftJis.KeyDiv, CommonSettingKey.CsvMapShiftJis.Key1, CommonSettingKey.CsvMapShiftJis.Key2, CommonSettingKey.CsvMapShiftJis.Key3)
            If Not UTFData Is Nothing Then
                list.Add(UTFData)
            End If
            If Not ShiftJisData Is Nothing Then
                list.Add(ShiftJisData)
            End If

            ' 並べ替えた結果を別のコレクションに書き出す（LINQのOrderBy拡張メソッド）
            Dim sorted As IOrderedEnumerable(Of SettingInfo) = list.OrderBy(Function(x) x.DATA10)
            'list.Sort()


            Dim db As New SystemDBContext
            '--2021/09/10 nv miyaji upd start
            'エラーの時にはコンボボックスの文字コードの値を変えないようにする。
            If isError = False Then
                'IDをキーにモデルを取得する
                Dim model As CsvMapModel = db.CsvMap.Find(_ViewModel.SearchCsvDiv)
                'Dim model As CsvMapModel = db.CsvMap.Find("ASSET")
                _ViewModel.CHAR_CD = model.CHAR_CD
            End If
            '--2021/09/10 nv miyaji upd end

            '一覧画面の文字コードコンボ設定
            ViewBag.CsvMapList = New SelectList(sorted, "KEY1", "DATA1", _ViewModel.CHAR_CD)
            'ViewBag.CsvMapList = New SelectListItem() {
            '        New SelectListItem() With {.Value = sorted(0).DATA10, .Text = sorted(0).DATA1},
            '        New SelectListItem() With {.Value = sorted(1).DATA10, .Text = sorted(1).DATA1}
            '     }

            '設定マスタから文字コードデータを取得できなかった時、エラーメッセージを設定する。
            If list.Count = 0 Then
                ViewBag.ErrorMessage = String.Format(Setting.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.CsvMapShiftJis.KeyDiv)
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
                _viewResult.Add(New CsvMapDetailViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.CSV_MAPPING, COLSET_LOC.ALL)
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).CHAR_CD = _ViewModel.CHAR_CD
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
        End Sub

        '2021/09/13 nv miyaji 使ってないのでコメントアウト
        '''' <summary>
        '''' Viewモデルを更新用のモデルにコピーする
        '''' </summary>
        'Private Sub CopyViewModeltoModel(ByVal model As CsvMapDetailViewModel)
        '    _Model.CSV_DIV = model.CSV_DIV
        '    _Model.CSV_NO = model.CSV_NO
        '    _Model.TOOL_DIV = model.TOOL_DIV
        '    _Model.WORD_DIV = model.WORD_DIV
        '    _Model.WORD_CD = model.WORD_CD
        '    _Model.REQUIRED_ITEM_FLAG = model.REQUIRED_ITEM_FLAG
        '    _Model.SPECIFIED_DIV = model.SPECIFIED_DIV
        '    _Model.CSV_COL_NO = model.CSV_COL_NO
        '    _Model.CSV_FIX_VAL = model.CSV_FIX_VAL
        '    _Model.INS_DATETIME = model.INS_DATETIME
        '    _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
        '    _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
        '    _Model.UPD_DATETIME = model.UPD_DATETIME
        '    _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
        '    _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
        '    _Model.UPD_TASK = model.UPD_TASK
        'End Sub
#End Region

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
    End Class
End Namespace