Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers
    ''' <summary>
    ''' 文言設定マスタ画面表示用のクラス
    ''' </summary>
    Public Class WordController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)


#Region "クラス定数"
        ''' <summary>
        ''' データ取込画面　結果表示用クラス
        ''' </summary>
        Private Class CsvWord
            Public Enum ColIdx
                ''' <summary>利用ツール区分</summary>
                ToolDiv
                ''' <summary>項目区分</summary>
                WordDiv
                ''' <summary>文言コード</summary>
                WordCd
                ''' <summary>ディフォルト英語</summary>
                WordingEN
                ''' <summary>ディフォルト日本語</summary>
                WordingJA
                ''' <summary>表示内容</summary>
                WordingLU
            End Enum
        End Class
#End Region

#Region "クラス変数"
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _ViewModel As WordViewModel = New WordViewModel                         'Word一覧表示用モデル
        Protected _viewResult As List(Of WordViewModel) = New List(Of WordViewModel)    '結果View
        Private _lstErr As List(Of DataInportResult)
        Private _Message As MessageRfid = Nothing
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
        Public Function Index(ByVal ViewModel As WordViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_WORD)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As WordViewModel = Session(CommonSessionKey.CREAGE_WORD)
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
                ' 利用ツールの初期値
                If String.IsNullOrEmpty(ViewModel.SearchToolDiv) Then
                    ViewModel.SearchToolDiv = ProcessToolDiv.Web
                End If
                ' 文言区分の初期値
                If String.IsNullOrEmpty(ViewModel.SearchWordDiv) Then
                    ViewModel.SearchWordDiv = WordDiv.ItemName
                End If
                '一覧表示処理
                MyBase.BaseIndex(_ViewModel)
                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.WORD, "ALL")
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
                Session.Remove(CommonSessionKey.CREAGE_WORD)
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
            Dim query As DbSqlQuery(Of WordViewModel) = _db.WordView.SqlQuery(_SQL, _paramSqlList.ToArray)
            '画面に表示するViewを設定
            _viewResult = query.ToList
        End Sub

#End Region

#Region "登録・更新処理"
        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim blnReturn As Boolean = True
            Return blnReturn
        End Function

        ''' <summary>
        ''' 登録処理をコンテキストに追加します
        ''' </summary>
        Protected Overrides Sub CreateQuery()
        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Return blnReturn
        End Function

        ''' <summary>
        ''' 更新処理のコンテキストを変更します
        ''' </summary>
        Protected Overrides Sub EditQuery()
        End Sub

#End Region

#Region "削除処理"
        ''' <summary>
        ''' 後処理
        ''' </summary>
        ''' <param name="model">基底モデル</param>
        Protected Overrides Sub DisposeDelete(ByRef model As BaseModel)
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
                sProcessContents = Me._Message.getMessage(CommonLabelWord.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelWord.TITLE))

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
            Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_WORD)
            Dim stArrayData As String() = sHeader.Split(","c)

            'CSVデータ取得 & 項目数チェック
            Dim isKomokusuuErr As Boolean = False
            Dim tbl As DataTable = clsSV_CsvFile.ReadCsvFileToDataTable(sCsvPath, Encoding.UTF8, CChar(","), True, "Word", stArrayData.Length, isKomokusuuErr)
            '項目数エラーの場合
            If isKomokusuuErr Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelWord.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " &
                                                     Path.GetFileName(CsvFile.FileName)
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelWord.TITLE))

                Return Json(Me._lstErr)
            End If

            'エラーチェック
            If Not IsDataError(tbl, stArrayData) Then
                '操作ログ出力
                sProcessContents = Me._Message.getMessage(CommonLabelWord.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " &
                                                     Path.GetFileName(CsvFile.FileName) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                                     Me._lstErr.Count.ToString
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelWord.TITLE))

                Return Json(Me._lstErr)
            End If

            '--登録処理開始
            Dim db As New SystemDBContext
            Dim model As List(Of WordModel) = New List(Of WordModel)    '登録用モデル
            Dim addCount As Integer = -1

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try

                    'ロケーションマスタデータを全削除
                    db.Database.ExecuteSqlCommand("DELETE FROM M_WORD")

                    '登録用モデルにデータを入れる
                    For icnt As Integer = 0 To tbl.Rows.Count - 1
                        ' 日本語
                        addCount += 1
                        model.Add(New WordModel())
                        model(addCount).LANG_DIV = LangDiv.JA
                        model(addCount).TOOL_DIV = ConvertToolDiv(tbl.Rows(icnt).Item(CsvWord.ColIdx.ToolDiv).ToString.Trim)
                        model(addCount).WORD_DIV = ConvertWordDiv(tbl.Rows(icnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim)
                        model(addCount).WORD_CD = tbl.Rows(icnt).Item(CsvWord.ColIdx.WordCd).ToString.Trim.TrimStart("'"c)
                        model(addCount).WORDING = tbl.Rows(icnt).Item(CsvWord.ColIdx.WordingJA).ToString.Trim
                        model(addCount).INS_DATETIME = CommonRfid.getNowDate
                        model(addCount).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(addCount).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(addCount).UPD_DATETIME = CommonRfid.getNowDate
                        model(addCount).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(addCount).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(addCount).UPD_TASK = Me._Message.getMessage(CommonLabelWord.TITLE)
                        ' 英語
                        addCount += 1
                        model.Add(New WordModel())
                        model(addCount).LANG_DIV = LangDiv.EN
                        model(addCount).TOOL_DIV = ConvertToolDiv(tbl.Rows(icnt).Item(CsvWord.ColIdx.ToolDiv).ToString.Trim)
                        model(addCount).WORD_DIV = ConvertWordDiv(tbl.Rows(icnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim)
                        model(addCount).WORD_CD = tbl.Rows(icnt).Item(CsvWord.ColIdx.WordCd).ToString.Trim.TrimStart("'"c)
                        model(addCount).WORDING = tbl.Rows(icnt).Item(CsvWord.ColIdx.WordingEN).ToString.Trim
                        model(addCount).INS_DATETIME = CommonRfid.getNowDate
                        model(addCount).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(addCount).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(addCount).UPD_DATETIME = CommonRfid.getNowDate
                        model(addCount).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(addCount).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(addCount).UPD_TASK = Me._Message.getMessage(CommonLabelWord.TITLE)
                        ' 見出し
                        addCount += 1
                        model.Add(New WordModel())
                        model(addCount).LANG_DIV = LangDiv.LU
                        model(addCount).TOOL_DIV = ConvertToolDiv(tbl.Rows(icnt).Item(CsvWord.ColIdx.ToolDiv).ToString.Trim)
                        model(addCount).WORD_DIV = ConvertWordDiv(tbl.Rows(icnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim)
                        model(addCount).WORD_CD = tbl.Rows(icnt).Item(CsvWord.ColIdx.WordCd).ToString.Trim.TrimStart("'"c)
                        model(addCount).WORDING = tbl.Rows(icnt).Item(CsvWord.ColIdx.WordingLU).ToString.Trim
                        model(addCount).INS_DATETIME = CommonRfid.getNowDate
                        model(addCount).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(addCount).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(addCount).UPD_DATETIME = CommonRfid.getNowDate
                        model(addCount).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                        model(addCount).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                        model(addCount).UPD_TASK = Me._Message.getMessage(CommonLabelWord.TITLE)
                    Next

                    'システムコンテキストにモデルを追加
                    db.Word.AddRange(model)

                    'エラーメッセージ取得のためTryCatchを記述
                    'db.SaveChanges()
                    Try
                        '登録処理を実行
                        db.SaveChanges()

                        tran.Commit()

                    Catch ex As System.Data.Entity.Validation.DbEntityValidationException
                        For Each errors In ex.EntityValidationErrors
                            For Each er In errors.ValidationErrors
                                System.Diagnostics.Trace.WriteLine(er.ErrorMessage)
                            Next
                        Next
                        tran.Rollback()

                        logger.Error(ex.Message)
                    End Try

                    '基本情報、再ロード
                    Dim multi As MessageRfid = New MessageRfid()
                    multi.Load()
                    HttpContext.Application(CommonConst.KEY_APP_LANGUAGE) = multi
                    Misc.CommonRfid.MessageRfid = multi

                    '操作ログ出力
                    sProcessContents = Me._Message.getMessage(CommonLabelWord.TITLE) & " " &
                                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
                                                             Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                             tbl.Rows.Count.ToString
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelWord.TITLE))

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    'logger.Error(sbSQL)    'マスタ系はSQLでの登録ではないのではずしておく
                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using
            Return Json(Me._lstErr)
        End Function

        ''' <summary>
        ''' 利用ツール区分変換
        ''' </summary>
        ''' <param name="targetValue"></param>
        ''' <returns></returns>
        Private Function ConvertToolDiv(targetValue As String) As String
            Dim result As String
            '文言マスタから取ると不具合が起きる可能性があるので、プログラムから取るように変更する 2019/12/17 miyaji
            If targetValue = ProcessToolDivCSV.Web Then
                'If targetValue = _Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB) Then
                result = ProcessToolDiv.Web
                '文言マスタから取ると不具合が起きる可能性があるので、プログラムから取るように変更する 2019/12/17 miyaji
            ElseIf targetValue = ProcessToolDivCSV.CS Then
                'ElseIf targetValue = _Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_CS) Then 
                result = ProcessToolDiv.CS
            Else
                result = ProcessToolDiv.HT
            End If
            Return result
        End Function

        ''' <summary>
        ''' 項目区分変換
        ''' </summary>
        ''' <param name="targetValue"></param>
        ''' <returns></returns>
        Private Function ConvertWordDiv(targetValue As String) As String
            Dim result As String
            If targetValue = WordDivCSV.ItemName Then
                result = WordDiv.ItemName
            ElseIf targetValue = WordDivCSV.Title Then
                result = WordDiv.Title
            ElseIf targetValue = WordDivCSV.Button Then
                result = WordDiv.Button
            ElseIf targetValue = WordDivCSV.Value Then
                result = WordDiv.Value
            ElseIf targetValue = WordDivCSV.Remark Then
                result = WordDiv.Remark
            ElseIf targetValue = WordDivCSV.Csv Then
                result = WordDiv.Csv
            ElseIf targetValue = WordDivCSV.Setting Then
                result = WordDiv.Setting
            Else
                result = WordDiv.Message
            End If
            Return result
        End Function

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
            If String.IsNullOrEmpty(extention) OrElse extention <> "csv" Then
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
                ''新規行がなければ追加する  '2020/03/16 nv miyaji　コメントアウト
                'If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                '    Dim lstRow As DataInportResult = New DataInportResult
                '    Me._lstErr.Add(lstRow)
                'End If

                Dim toolDiv As String = tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.ToolDiv).ToString.Trim.PadRight(itemlength.ToolDiv)
                Dim wordDiv As String = tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim.PadRight(itemlength.WordDiv)
                Dim wordCd As String = tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordCd).ToString.Trim.PadRight(itemlength.WordCd).TrimStart("'"c)

                '重複チェック     '--2019/10/21 add nv iwamoto
                If lstSumiCd.IndexOf(toolDiv & wordDiv & wordCd) > -1 Then
                    '--2020/03/16 nv miyaji add start
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If
                    '--2020/03/16 nv miyaji add end
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_KEY_ITEM)
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAJUUFUKU) & String.Format(Me._Message.getMessage(CommonLabel.LABEL_KEY_ITEM) & "[{0}]", toolDiv.Trim & "," & wordDiv.Trim & "," & wordCd.Trim)
                    blRet = False
                Else
                    lstSumiCd.Add(toolDiv & wordDiv & wordCd)
                End If

                ' 利用区分チェック
                '文言マスタから取ると不具合が起きる可能性があるので、プログラムから取るように変更する 2019/12/17 miyaji
                'If tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.ToolDiv).ToString.Trim <> _Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB) _
                '    AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.ToolDiv).ToString.Trim <> _Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_CS) _
                '    AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.ToolDiv).ToString.Trim <> _Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT) Then
                If tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.ToolDiv).ToString.Trim <> ProcessToolDivCSV.Web _
                        AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.ToolDiv).ToString.Trim <> ProcessToolDivCSV.CS _
                        AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.ToolDiv).ToString.Trim <> ProcessToolDivCSV.HT Then
                    '--2020/03/16 nv miyaji add start
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If
                    '--2020/03/16 nv miyaji add end

                    ' 利用区分エラー
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_TOOL_DIV)
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                    blRet = False
                End If
                ' 項目区分チェック
                If tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim <> WordDivCSV.ItemName _
                    AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim <> WordDivCSV.Title _
                    AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim <> WordDivCSV.Button _
                    AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim <> WordDivCSV.Value _
                    AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim <> WordDivCSV.Message _
                    AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim <> WordDivCSV.Remark _
                    AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim <> WordDivCSV.Csv _
                    AndAlso tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordDiv).ToString.Trim <> WordDivCSV.Setting Then
                    '--2020/03/16 nv miyaji add start
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If
                    '--2020/03/16 nv miyaji add end

                    ' 項目区分エラー
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelWord.LABEL_WORD_DIV)
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                    blRet = False
                End If
                ' 文言コードチェック
                If (tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordCd).ToString.Trim.TrimStart("'"c)).Length <> 6 _
                    OrElse CommonRfid.IsNumber(tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordCd).ToString.Trim.TrimStart("'"c)) = False Then
                    '--2020/03/16 nv miyaji add start
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If
                    '--2020/03/16 nv miyaji add end

                    ' 文言コードエラー
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelWord.LABEL_WORD_CODE)
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                    blRet = False
                End If
                ' 日本語チェック
                ' If (tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordingJA).ToString.Trim).Length > 1024 Then    '--2021/11/26 upd nv iwamoto
                If (tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordingJA).ToString.Trim).Length > 2048 Then
                    '--2020/03/16 nv miyaji add start
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If
                    '--2020/03/16 nv miyaji add end

                    ' ディフォルト日本語エラー
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelWord.LABEL_DEFAULT_JA)
                    '--2020/03/13 nv miyaji update start
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                    'Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                    '--2020/03/13 nv miyaji update end
                    blRet = False
                End If
                ' 英語チェック
                'If (tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordingEN).ToString.Trim).Length > 1024 Then     '--2021/11/26 upd nv iwamoto
                If (tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordingEN).ToString.Trim).Length > 2048 Then
                    '--2020/03/16 nv miyaji add start
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If
                    '--2020/03/16 nv miyaji add end

                    ' ディフォルト英語エラー
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelWord.LABEL_DEFAULT_EN)
                    '--2020/03/13 nv miyaji update start
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                    'Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                    '--2020/03/13 nv miyaji update end
                    blRet = False
                End If
                ' 表示内容チェック
                'If (tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordingLU).ToString.Trim).Length > 1024 Then     '--2021/11/26 upd nv iwamoto
                If (tbl.Rows(iRowCnt).Item(CsvWord.ColIdx.WordingLU).ToString.Trim).Length > 2048 Then
                    '--2020/03/16 nv miyaji add start
                    '新規行がなければ追加する
                    If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                        Dim lstRow As DataInportResult = New DataInportResult
                        Me._lstErr.Add(lstRow)
                    End If
                    '--2020/03/16 nv miyaji add end

                    ' 表示内容エラー
                    Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
                    Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabelWord.LABEL_DISPLAY_NAME)
                    '--2020/03/13 nv miyaji update start
                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAOVER)
                    'Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
                    '--2020/03/13 nv miyaji update end
                    blRet = False
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

            Dim WordData As List(Of WordViewModel)

            '文言設定マスタデータ取得
            WordData = GetAllWordData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.Word.KeyDiv, CommonSettingKey.Word.Key1, CommonSettingKey.Word.Key2, CommonSettingKey.Word.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_WORD)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_WORD)
            Dim singleQuotation As String = "'"

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As WordViewModel In WordData
                col = ""
                If row.TOOL_DIV.Trim = ProcessToolDiv.Web Then
                    '文言マスタから取ると不具合が起きる可能性があるので、プログラムから取るように変更する 2019/12/17 miyaji
                    'col += SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB))
                    col += SVClsCSVExp.SVFncCSVAddDquot(ProcessToolDivCSV.Web)
                ElseIf row.TOOL_DIV.Trim = ProcessToolDiv.CS Then
                    '文言マスタから取ると不具合が起きる可能性があるので、プログラムから取るように変更する 2019/12/17 miyaji
                    'col += SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_CS))
                    col += SVClsCSVExp.SVFncCSVAddDquot(ProcessToolDivCSV.CS)
                ElseIf row.TOOL_DIV.Trim = ProcessToolDiv.HT Then
                    '文言マスタから取ると不具合が起きる可能性があるので、プログラムから取るように変更する 2019/12/17 miyaji
                    'col += SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)) 
                    col += SVClsCSVExp.SVFncCSVAddDquot(ProcessToolDivCSV.HT)
                Else
                    col += SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If

                If row.WORD_DIV.Trim = WordDiv.ItemName Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(WordDivCSV.ItemName)
                ElseIf row.WORD_DIV.Trim = WordDiv.Title Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(WordDivCSV.Title)
                ElseIf row.WORD_DIV.Trim = WordDiv.Button Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(WordDivCSV.Button)
                ElseIf row.WORD_DIV.Trim = WordDiv.Value Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(WordDivCSV.Value)
                ElseIf row.WORD_DIV.Trim = WordDiv.Message Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(WordDivCSV.Message)
                ElseIf row.WORD_DIV.Trim = WordDiv.Remark Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(WordDivCSV.Remark)
                ElseIf row.WORD_DIV.Trim = WordDiv.Csv Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(WordDivCSV.Csv)
                ElseIf row.WORD_DIV.Trim = WordDiv.Setting Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(WordDivCSV.Setting)
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If

                'col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.WORD_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.WORD_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.WORDING_EN)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.WORDING_JA)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.WORDING_LU)

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelWord.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     WordData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelWord.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetAllWordData() As List(Of WordViewModel)
            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.Clear()
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("ja.LANG_DIV")
            sbSQL.AppendLine(",ja.TOOL_DIV")
            sbSQL.AppendLine(",ja.WORD_DIV")
            sbSQL.AppendLine(",ja.WORD_CD")
            sbSQL.AppendLine(",en.WORDING WORDING_EN")
            sbSQL.AppendLine(",ja.WORDING WORDING_JA")
            sbSQL.AppendLine(",lu.WORDING WORDING_LU")
            sbSQL.AppendLine(",(SELECT INS_DATETIME FROM M_WORD WHERE")
            sbSQL.AppendLine("LANG_DIV = (Select TOP 1 LANG_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND TOOL_DIV = (SELECT TOP 1 TOOL_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND WORD_DIV = (SELECT TOP 1 WORD_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND WORD_CD = (SELECT TOP 1 WORD_CD FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine(") LAST_INS_DATETIME")
            sbSQL.AppendLine(",(SELECT INS_EMPLOYEE_NAME FROM M_WORD WHERE")
            sbSQL.AppendLine("LANG_DIV = (Select TOP 1 LANG_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND TOOL_DIV = (SELECT TOP 1 TOOL_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND WORD_DIV = (SELECT TOP 1 WORD_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND WORD_CD = (SELECT TOP 1 WORD_CD FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine(") LAST_INS_EMPLOYEE_NAME")
            sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount")
            sbSQL.AppendLine("FROM M_WORD ja")
            sbSQL.AppendLine("LEFT JOIN M_WORD en")
            sbSQL.AppendLine("ON")
            sbSQL.AppendLine("en.LANG_DIV = '" & LangDiv.EN & "'")
            sbSQL.AppendLine("AND en.TOOL_DIV = ja.TOOL_DIV")
            sbSQL.AppendLine("AND en.WORD_DIV = ja.WORD_DIV")
            sbSQL.AppendLine("AND en.WORD_CD = ja.WORD_CD")
            sbSQL.AppendLine("LEFT JOIN M_WORD lu")
            sbSQL.AppendLine("ON")
            sbSQL.AppendLine("lu.LANG_DIV = '" & LangDiv.LU & "'")
            sbSQL.AppendLine("AND lu.TOOL_DIV = ja.TOOL_DIV")
            sbSQL.AppendLine("AND lu.WORD_DIV = ja.WORD_DIV")
            sbSQL.AppendLine("AND lu.WORD_CD = ja.WORD_CD")
            sbSQL.AppendLine("WHERE")
            sbSQL.AppendLine("ja.LANG_DIV = '" & LangDiv.JA & "'")
            sbSQL.AppendLine("ORDER BY")
            sbSQL.AppendLine("ja.TOOL_DIV, ja.WORD_DIV, ja.WORD_CD")

            Dim lstWord As List(Of WordViewModel)   'ロケーションリスト
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstWord = db.WordView.SqlQuery(sbSQL.ToString).ToList
            'データを返却
            Return lstWord
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
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim m = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '一覧表示表用SQL
            sbSQL.Clear()
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("ja.LANG_DIV")
            sbSQL.AppendLine(",ja.TOOL_DIV")
            sbSQL.AppendLine(",ja.WORD_DIV")
            sbSQL.AppendLine(",ja.WORD_CD")
            sbSQL.AppendLine(",en.WORDING WORDING_EN")
            sbSQL.AppendLine(",ja.WORDING WORDING_JA")
            sbSQL.AppendLine(",lu.WORDING WORDING_LU")

            sbSQL.AppendLine(",(SELECT INS_DATETIME FROM M_WORD WHERE")
            sbSQL.AppendLine("LANG_DIV = (Select TOP 1 LANG_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND TOOL_DIV = (SELECT TOP 1 TOOL_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND WORD_DIV = (SELECT TOP 1 WORD_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND WORD_CD = (SELECT TOP 1 WORD_CD FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine(") LAST_INS_DATETIME")
            sbSQL.AppendLine(",(SELECT INS_EMPLOYEE_NAME FROM M_WORD WHERE")
            sbSQL.AppendLine("LANG_DIV = (Select TOP 1 LANG_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND TOOL_DIV = (SELECT TOP 1 TOOL_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND WORD_DIV = (SELECT TOP 1 WORD_DIV FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine("AND WORD_CD = (SELECT TOP 1 WORD_CD FROM M_WORD ORDER BY UPD_DATETIME DESC)")
            sbSQL.AppendLine(") LAST_INS_EMPLOYEE_NAME")

            sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount")

            sbSQL.AppendLine("FROM M_WORD ja")

            sbSQL.AppendLine("LEFT JOIN M_WORD en")
            sbSQL.AppendLine("ON")
            sbSQL.AppendLine("en.LANG_DIV = '" & LangDiv.EN & "'")
            sbSQL.AppendLine("AND en.TOOL_DIV = ja.TOOL_DIV")
            sbSQL.AppendLine("AND en.WORD_DIV = ja.WORD_DIV")
            sbSQL.AppendLine("AND en.WORD_CD = ja.WORD_CD")

            sbSQL.AppendLine("LEFT JOIN M_WORD lu")
            sbSQL.AppendLine("ON")
            sbSQL.AppendLine("lu.LANG_DIV = '" & LangDiv.LU & "'")
            sbSQL.AppendLine("AND lu.TOOL_DIV = ja.TOOL_DIV")
            sbSQL.AppendLine("AND lu.WORD_DIV = ja.WORD_DIV")
            sbSQL.AppendLine("AND lu.WORD_CD = ja.WORD_CD")

            sbSQL.AppendLine("WHERE")
            sbSQL.AppendLine("ja.LANG_DIV = '" & LangDiv.JA & "'")

            '検索条件を設定する
            ' 処理ツール
            sbSQL.AppendLine("AND ja.TOOL_DIV = '" & _ViewModel.SearchToolDiv & "'")
            ' 処理ツール
            sbSQL.AppendLine("AND ja.WORD_DIV = '" & _ViewModel.SearchWordDiv & "'")

            sbSQL.AppendLine("ORDER BY")
            sbSQL.AppendLine("ja.TOOL_DIV, ja.WORD_DIV, ja.WORD_CD")

            Return sbSQL.ToString()

        End Function

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New WordViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchToolDiv = _ViewModel.SearchToolDiv
            _viewResult.ElementAt(0).SearchWordDiv = _ViewModel.SearchWordDiv
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).LAST_INS_DATETIME = _viewResult.ElementAt(0).LAST_INS_DATETIME
            _viewResult.ElementAt(0).LAST_INS_EMPLOYEE_NAME = _viewResult.ElementAt(0).LAST_INS_EMPLOYEE_NAME
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.WORD, "ALL").FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.WORD, "ALL")

        End Sub

#End Region

    End Class
End Namespace