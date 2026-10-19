Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient
Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports GrapeCity.Documents.Excel
Imports GrapeCity.Documents.Excel.Drawing
Imports Asetra.Dao

Namespace Controllers
    Public Class AssetLabelPrintController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"

        ''' <summary>
        ''' ページ間のチェックボックス対応
        ''' </summary>
        Private Enum AssetLabelPrintKey
            ASSET_NO = 0
            CATEGORY_L = 1
        End Enum

#End Region

#Region "クラス変数"
        Private _Model As AssetLabelPrintModel = New AssetLabelPrintModel                                 'Assetモデル
        Private _ViewModel As AssetLabelPrintViewModel = New AssetLabelPrintViewModel                     'Asset一覧表示用モデル
        Protected _viewResult As List(Of AssetLabelPrintViewModel) = New List(Of AssetLabelPrintViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報

        Private _isSort As Boolean = True                       'ソートフラグ
        Private _AuthInfo As New AuthInfo                 '権限情報
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As AssetLabelPrintViewModel, ByVal actionname As String) As ActionResult
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
        Public Function Index(ByVal ViewModel As AssetLabelPrintViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LABEL_PRINT)
            End If

            '上限確認フラグ 初期化
            ViewBag.isLimitFlg = False
            '一覧件数　初期化
            ViewBag.cntList = 0
            '設定マスタ上限件数　初期化
            ViewBag.cntLimit = 0
            '一覧表示フラグ
            Dim isDisp As Boolean = True

            'メッセージ一覧取得
            Dim Message As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Asset_Label_Print.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Label_Print.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Label_Print.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Label_Print.Key3)

            '検索ボタンがクリックされた場合、選択状態のセッションをクリアする
            If Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                'セッションSEARCH_FLGを設定
                Session(CommonSessionKey.SEARCH_FLG) = True
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As AssetLabelPrintViewModel = Session(CommonSessionKey.CREAGE_ASSET_LABEL_PRINT)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
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
                ViewBag.modelLocL = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                ViewBag.modelLocM = New MultiSelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME")
                ViewBag.modelLocS = New MultiSelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME")
                ViewBag.modelCatL = New MultiSelectList(New List(Of CategoryDropdownModel), "CAT_CD", "CAT_NAME")
                ViewBag.modelPrinter = New SelectList(New List(Of PrinterDropdownModel), "KEY1", "DATA1")
                CopyModelData()
                Return View(_viewResult)
            Else
                '項目配置情報取得
                Dim dictGridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_LABEL_PRINT, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL))
                If dictGridInfo.Count = 0 Then
                    dictGridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_LABEL_PRINT, COLSET_LOC.TEMPLATE)
                End If

                '未タグ紐づけ資産 存在チェック用
                ViewQuery()

                '検索条件の復元有無
                _ViewModel.IsRestoreOwnSearchConditions = True

                '件数確認用データのためソートしない
                _isSort = False
                'セッションSEARCH_FLGがNothingの場合
                If Session(CommonSessionKey.SEARCH_FLG) Is Nothing Then
                    '初期表示
                    _ViewModel.FirstSearch = True
                    isDisp = False

                    '設定マスタから取得できた場合 かつ 初期表示フラグ(Data1)がある場合 かつ 設定マスタ(web読取データ)の初期表示Nの場合
                    If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA1) = False AndAlso WebReadData.DATA1 = RedeDataFlg.No Then
                        _ViewModel.FirstSearch = False
                    End If
                Else
                    '検索ボタンから表示
                    '初回表示フラグ False
                    _ViewModel.FirstSearch = False
                    '表示する
                    MyBase.BaseIndex(_ViewModel, False)

                End If

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm

                ViewBag.LoginInfo = _LoginInfo

            End If

            '設定マスタから取得できた場合 かつ 上限値(Data2)が存在している場合 かつ 上限を超えていた場合
            If _viewResult.Count > 0 AndAlso WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA2) = False AndAlso WebReadData.DATA2 < _viewResult.ElementAt(0).TotalCount Then

                '上限確認フラグ
                ViewBag.isLimitFlg = True
                '一覧データ取得件数
                ViewBag.cntList = _viewResult.ElementAt(0).TotalCount
                '設定マスタ上限件数　初期化
                ViewBag.cntLimit = WebReadData.DATA2
                '未タグ紐付けの資産　文言クリア
                ViewBag.MiTagHimoduke = ""

                '読取データをクリア
                _viewResult.Clear()
            ElseIf isDisp Then
                'ソートする
                _isSort = True
                '一覧データ取得
                MyBase.BaseIndex(_ViewModel)
            End If

            'チェックボックスのON/OFF設定
            If _viewResult.Count = 0 Then
                _ViewModel.SelectKeys = String.Empty
            End If
            If _viewResult.Count > 0 AndAlso _ViewModel.needToInitCheckbox Then
                'チェック状態のセッション情報を更新する
                UpdateChecedTangible()
            Else
                '最新のチェック状態でセッション情報を更新する(追加および削除)
                UpdateChecedInfo(_ViewModel.SelectKeys, _ViewModel.NoneSelectKeys)
            End If

            'ログインセッション切れ以外のエラーが設定されていたらモーダルのコンボボックスを設定
            If _ViewModel.ErrCode = String.Empty OrElse _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY Then
                'ロケーション
                ' 大区分ドロップダウンリスト作成
                GetLocationLData()
                '[CC431] UPD Start 20261007
                ' 中区分・小区分（入庫予定Tに存在するもの。場所から選ぶの ON／OFF で連動）
                SetSearchLocationOptions(Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLabelPrint, _LoginInfo))
                '[CC431] UPD End 20261007

                'カテゴリー
                ' 大分類ドロップダウンリスト作成
                GetCategoryLData()

            Else
                'ロケーション
                ' 大区分ドロップダウンリスト作成
                GetLocationLData()
                ' 中区分
                GetLocationMData(_ViewModel.LOC_L_CD)
                ' 小区分
                GetLocationSData(_ViewModel.LOC_L_CD, New String() {_ViewModel.LOC_M_CD})
                '[CC431] ADD Start 20261007
                ' 検索条件の中区分・小区分は、検索条件の会社・選択値で作り直す
                ' ※上の GetLocationMData／GetLocationSData は LOC_L_CD／LOC_M_CD で検索条件のコンボも作るため、検索条件の選択値が外れる
                If Not IsNothing(_LoginInfo) Then
                    SetSearchLocationOptions(Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLabelPrint, _LoginInfo))
                End If
                '[CC431] ADD End 20261007

                'カテゴリー
                ' 大分類ドロップダウンリスト作成
                GetCategoryLData()
            End If

            '[CC431] ADD Start 20261007
            '設定情報(ロケーション使用区分)取得（場所から選ぶの表示に使う）
            Dim settingLocUsage As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            If IsNothing(settingLocUsage) Then
                Throw New Exception(String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            ViewBag.LocUsage_L = settingLocUsage.DATA1
            ViewBag.LocUsage_M = settingLocUsage.DATA2
            ViewBag.LocUsage_S = settingLocUsage.DATA3
            '[CC431] ADD End 20261007

            ' プリンタ情報取得
            GetPrinter()

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.ASSET_LABEL_PRINT, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL))
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
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()

            'クエリを返却
            MyBase._SQL = CreateSQL(_isSort)

            '検索条件(全件)を指定したSQLとパラメーターをセッションに保持する
            Session(CommonSessionKey.LIST_VIEW_SQL) = MyBase._SQL
            Session(CommonSessionKey.LIST_VIEW_PARAM) = MyBase._paramSqlList

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()

            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LABEL_PRINT)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of AssetLabelPrintViewModel) = _db.AssetLabelPrintView.SqlQuery(_SQL, _paramSqlList.ToArray)

            '画面に表示するViewを設定
            _viewResult = query.ToList

        End Sub

#End Region

#Region "Excelデータ取込処理（資産台帳取込）"
        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="ExcelFile">登録するExcelファイル</param>
        ''' <returns>View表示内容</returns>
        Public Function ImportExcelFile(ByVal ExcelFile As HttpPostedFileBase, ByVal LocLCd As String) As ActionResult

            Me._lstErr = New List(Of DataInportResult)
            Dim errRow As DataInportResult = New DataInportResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr.Add(errRow)
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.LoginSession
                '結果表示
                Return Json(Me._lstErr)
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim sProcessContents As String          '操作ログ出力
            Dim sTitle As String = Me._Message.getMessage(CommonLabelAssetLabelPrint.TITLE)
            Dim sAction As String = Me._Message.getMessage(CommonLabel.BUTTON_ASSET_LEDGER_IMPORT)

            '操作ログ出力(取込開始)
            sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.BUTTON_INPORT)
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle, LocLCd)

            '読込むExcelファイルがあるかどうか
            If Not IsImportFileError(ExcelFile) Then
                Dim sErrGaiyou As String = String.Empty
                Select Case Me._lstErr(0).ResultCd
                    Case DataInport.DataInportResultCd.NoSelect
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECT)
                    Case DataInport.DataInportResultCd.Empty
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_EMPTY)
                    Case DataInport.DataInportResultCd.NotExcel
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXCEL)
                End Select

                '操作ログ出力
                sProcessContents = sTitle & " " & sAction & " " & sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)

                Return Json(Me._lstErr)
            End If

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVIMPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            Dim sFilePath As String = Path.Combine(sFullPath, ExcelFile.FileName)
            Dim excelRowCount As Integer = 0
            Dim blRet As Boolean
            Try
                'Excelファイルをアップロード
                ExcelFile.SaveAs(sFilePath)

                'Excelファイル読込＆DB登録処理
                blRet = Dao.ToolAssetExcelImport.Execute(Misc.Auth.FunctionId.AssetLabelPrint, sFilePath, LocLCd, _LoginInfo, _lstErr, excelRowCount)
            Catch
                InsImportExceptionLog(sTitle, sAction, Path.GetFileName(ExcelFile.FileName))
                Throw
            End Try

            'エラーがあった場合
            If Not blRet Then
                Dim sFileName As String = Path.GetFileName(ExcelFile.FileName)

                If Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportDeleteConfirm Then
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_CONF_ASSET_IMPORT_DELETE).Replace("{0}", Me._lstErr(0).DeleteCount.ToString) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr Then
                    '項目数エラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportSpSystemErr Then
                    'ストアドで予期しないエラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_UNEXPECTED_ERR) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportSpParamErr Then
                    'ストアドで引数エラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_PARAM_ERR) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportSpLocMaterErr Then
                    'ストアドでロケマスタなしエラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_LOC_ERR) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportSpEmployeeErr Then
                    'ストアドで登録担当者なしエラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_EMPLOYEE_ERR) & " " & sFileName
                ElseIf Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.ImportNotUpdatableComErr Then
                    'Excel取込エラー（更新可能拠点以外を含む）
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_EXIST_ASSET_COMPANY_NOT_UPDATABLE) & " " & sFileName
                Else
                    '入力エラー
                    sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " & sFileName & " " &
                                                                      Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                                                      Me._lstErr.Count.ToString
                End If

                '操作ログ出力
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)

                Return Json(Me._lstErr)
            End If

            '操作ログ出力
            sProcessContents = sTitle & " " & sAction & " " &
                                Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                excelRowCount.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)

            Me._lstErr.Add(errRow)
            Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.OK

            Return Json(Me._lstErr)
        End Function

        ''' <summary>
        ''' 資産台帳取込の削除確認で「はい」が選ばれた場合の取込処理（AJax）
        ''' </summary>
        ''' <param name="ImportKey">取込ごとのキー（初回の取込で返したもの）</param>
        ''' <param name="LocLCd">ロケ大コード（画面の検索条件-ロケーション会社）</param>
        ''' <returns>取込結果</returns>
        ''' <remarks>Excelファイルは送られない。初回の取込で入庫予定ワークに保存したデータで取込を行う。</remarks>
        Public Function ImportExcelFileConfirmed(ByVal ImportKey As String, ByVal LocLCd As String) As ActionResult

            Me._lstErr = New List(Of DataInportResult)

            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.LoginSession})
                Return Json(Me._lstErr)
            End If

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim sTitle As String = Me._Message.getMessage(CommonLabelAssetLabelPrint.TITLE)
            Dim sAction As String = Me._Message.getMessage(CommonLabel.BUTTON_ASSET_LEDGER_IMPORT)

            Dim rowCount As Integer = 0
            Dim sFileName As String = String.Empty
            Dim blRet As Boolean
            Try
                blRet = Dao.ToolAssetExcelImport.ExecuteConfirmed(Misc.Auth.FunctionId.AssetLabelPrint, ImportKey, LocLCd, _LoginInfo, _lstErr, rowCount, sFileName)
            Catch
                InsImportExceptionLog(sTitle, sAction, sFileName)
                Throw
            End Try

            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, GetImportResultLog(blRet, sTitle, sAction, sFileName, rowCount), _LoginInfo, sTitle)

            If blRet Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.OK})
            End If

            Return Json(Me._lstErr)
        End Function

        ''' <summary>
        ''' 資産台帳取込の削除確認で「いいえ」が選ばれた場合の処理（AJax）
        ''' </summary>
        ''' <param name="ImportKey">取込ごとのキー（初回の取込で返したもの）</param>
        ''' <returns>処理結果</returns>
        ''' <remarks>取込ごとのキーを条件に入庫予定ワークのデータを削除して中断する。</remarks>
        Public Function ImportExcelFileCancel(ByVal ImportKey As String) As ActionResult

            Me._lstErr = New List(Of DataInportResult)

            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.LoginSession})
                Return Json(Me._lstErr)
            End If

            Dao.ToolAssetExcelImport.CancelImport(ImportKey, _LoginInfo)

            Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.OK})
            Return Json(Me._lstErr)
        End Function

        ''' <summary>
        ''' 資産台帳取込の結果から、操作ログに出力する内容を作成する（「はい」の取込用）
        ''' </summary>
        ''' <param name="blRet">取込結果</param>
        ''' <param name="sTitle">画面タイトル</param>
        ''' <param name="sAction">ボタン名</param>
        ''' <param name="sFileName">取込ファイル名</param>
        ''' <param name="rowCount">取込件数</param>
        ''' <returns>操作ログの内容</returns>
        ''' <remarks>出力内容は ImportExcelFile と同じ。ワークにデータが無い場合は取込失敗（ImportExcelErr）。</remarks>
        Private Function GetImportResultLog(ByVal blRet As Boolean,
                                            ByVal sTitle As String,
                                            ByVal sAction As String,
                                            ByVal sFileName As String,
                                            ByVal rowCount As Integer) As String
            If blRet Then
                Return sTitle & " " & sAction & " " &
                       Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                       Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                       rowCount.ToString
            End If

            Select Case Me._lstErr(0).ResultCd
                Case DataInport.DataInportResultCd.ImportExcelErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_ERR) & " " & sFileName
                Case DataInport.DataInportResultCd.ImportSpSystemErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_UNEXPECTED_ERR) & " " & sFileName
                Case DataInport.DataInportResultCd.ImportSpParamErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_PARAM_ERR) & " " & sFileName
                Case DataInport.DataInportResultCd.ImportSpLocMaterErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_LOC_ERR) & " " & sFileName
                Case DataInport.DataInportResultCd.ImportSpEmployeeErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_IMPORT_SP_EMPLOYEE_ERR) & " " & sFileName
                Case DataInport.DataInportResultCd.ImportNotUpdatableComErr
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_EXIST_ASSET_COMPANY_NOT_UPDATABLE) & " " & sFileName
                Case Else
                    Return sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " & sFileName & " " &
                           Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                           Me._lstErr.Count.ToString
            End Select
        End Function

        ''' <summary>
        ''' 資産台帳取込で例外が発生した場合の操作ログを出力する
        ''' </summary>
        ''' <param name="sTitle">画面タイトル</param>
        ''' <param name="sAction">ボタン名</param>
        ''' <param name="sFileName">取込ファイル名</param>
        ''' <remarks>操作ログの出力に失敗しても、呼び出し元の例外を優先するため例外は投げない。</remarks>
        Private Sub InsImportExceptionLog(ByVal sTitle As String,
                                          ByVal sAction As String,
                                          ByVal sFileName As String)
            Try
                Dim sProcessContents As String = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_SYSTEM) & " " & sFileName
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)
            Catch ex As Exception
                logger.Error(ex.Message)
            End Try
        End Sub

#End Region

#Region "並び順のソートカラム取得"
        '[CC431] DEL 20261015 GetSortColumn（1列だけ返す）を削除し、GetOrderBy（キーの並び全体を返す）に置き換え
        '[CC431] ADD Start 20261015
        ''' <summary>
        ''' 並び順に対応する ORDER BY の中身（キーの並び）を取得する
        ''' </summary>
        ''' <param name="sortOrder">検索条件 並び順(AssetLabelPrintSort)</param>
        ''' <returns>ORDER BY に続けて指定する文字列（想定外の値は資産クラス）</returns>
        ''' <remarks>
        ''' 原価センタ・場所はコード順（仕様変更2）。第2キー以降は並び順ごとに定義し、資産番号を最後に付ける。
        ''' 資本化日付・管理者社員番号・実棚者コード・備考は、空白（NULL・空文字）を後ろにする。
        ''' </remarks>
        Private Function GetOrderBy(ByVal sortOrder As Integer) As String
            Select Case sortOrder
                Case AssetLabelPrintSort.CatLAsc
                    Return "INA.CAT_L_CD,INA.ASSET_NO"
                Case AssetLabelPrintSort.AssetNoAsc
                    Return "INA.ASSET_NO"
                Case AssetLabelPrintSort.LocMAsc
                    Return "INA.LOC_M_CD,INA.LOC_S_CD,INA.ASSET_NO"
                Case AssetLabelPrintSort.LocSAsc
                    Return "INA.LOC_S_CD,INA.LOC_M_CD,INA.ASSET_NO"
                Case AssetLabelPrintSort.FreeItem2Asc
                    Return GetBlankLastOrder("INA.FREE_ITEM2") & ",INA.ASSET_NO"
                Case AssetLabelPrintSort.FreeItem6Asc
                    Return GetBlankLastOrder("INA.FREE_ITEM6") & ",INA.ASSET_NO"
                Case Else
                    Return "INA.CAT_L_CD,INA.ASSET_NO"
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

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function IchiranCsvExport(ViewModel As AssetLabelPrintViewModel) As ActionResult
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

            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid                      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Asset_Label_Print.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Label_Print.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Label_Print.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Asset_Label_Print.Key3)
            'モデルを設定
            _ViewModel = ViewModel

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Me._AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLabelPrint, _LoginInfo)

            Dim AssetData As List(Of AssetLabelPrintViewModel)

            '資産マスタデータ取得
            AssetData = GetAssetLabelPrintData()

            '設定マスタから取得できた場合
            '設定マスタから取得できた場合 かつ 上限値(Data2)が存在している場合 かつ 上限を超えていた場合
            If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA2) = False AndAlso WebReadData.DATA2 < AssetData.Count Then
                '上限を超えていた場合　CSV出力しない

                '上限確認フラグ
                ViewBag.isLimitFlg = True
                '一覧データ取得件数
                ViewBag.cntList = AssetData.Count
                '設定マスタ上限件数　初期化
                ViewBag.cntLimit = WebReadData.DATA2

                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.AssetLabelPrint.KeyDiv, CommonSettingKey.AssetLabelPrint.Key1, CommonSettingKey.AssetLabelPrint.Key2, CommonSettingKey.AssetLabelPrint.Key3, True)
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
            '設定マスタ（資産CSVファイル取込全件対象）の取得
            Dim CsvRegistAll As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Asset_CSV_Regist_ALL.KeyDiv,
                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key1,
                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key2,
                                                                         CommonSettingKey.Asset_CSV_Regist_ALL.Key3, True)
            'CSVヘッダー取得
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_LABEL_PRINT)
            Dim singleQuotation As String = "'"

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As AssetLabelPrintViewModel In AssetData
                col = ""
                col += SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.SUB_ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.MNG_LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MngLocLName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocLName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocMName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LocSName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CAT_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CatLName)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6_NAME)
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
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM20)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM21)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM22)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM23)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM24)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM25)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM26)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM27)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM28)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM29)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM30)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM31)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM32)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM33)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM34)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM35)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM36)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM37)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM38)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM39)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM40)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM41)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM42)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM43)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM44)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM45)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM46)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM47)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM48)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM49)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM50)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM51)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM52)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM53)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM54)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM55)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM56)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM57)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM58)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM59)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM60)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.IP_DATETIME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.IP_FILE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.IP_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.IpEmployeeName)
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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetLabelPrint.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     AssetData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetLabelPrint.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetAssetLabelPrintData() As List(Of AssetLabelPrintViewModel)
            Dim lstAssetLabelPrint As List(Of AssetLabelPrintViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstAssetLabelPrint = db.AssetLabelPrintView.SqlQuery(CreateSQL(), _paramSqlList.ToArray).ToList

            'データを返却
            Return lstAssetLabelPrint
        End Function
#End Region

#Region "ラベル不要処理"
        ''' <summary>
        ''' ラベル不要処理
        ''' </summary>
        ''' <param name="ViewModel">ビューモデル</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function processLabelNotRequired(ViewModel As AssetLabelPrintViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '最新のチェック状態でセッション情報を更新する(追加および削除) (キー：資産番号, カテゴリーL)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            'マスタデータ取得
            Dim AssetListData As List(Of AssetListLabelModel)
            AssetListData = GetSelectAssetData("'" & String.Join("','", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetLabelPrintKey.ASSET_NO))) & "'")

            'DB処理
            Dim db As SystemDBContext = New SystemDBContext
            Dim sUpdTask As String = Me._Message.getMessage(CommonLabel.BUTTON_LABEL_NOT_REQUIRED)

            Using tran = db.Database.BeginTransaction()
                Try
                    '資産マスタ、入庫予定T、履歴TのDB更新
                    Dim errMessage = AssetDbUpdateProcess(db, lstSelectKeys, AssetListData, sUpdTask, Misc.ProcessDiv.UI, _LoginInfo)

                    If Not String.IsNullOrEmpty(errMessage) Then
                        ViewModel.ErrCode = errMessage
                    ElseIf Not ModelState.IsValid Then
                        ViewModel.ErrCode = CommonLabel.MSG_ERR_UPDATE_FAILURE
                    Else
                        '操作ログ出力
                        Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetLabelPrint.TITLE) & " " &
                                                    sUpdTask & " " &
                                                    Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                    Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                    lstSelectKeys.Count.ToString
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetLabelPrint.TITLE))

                        tran.Commit()

                        '選択状態をクリアにする
                        Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                        ViewModel.SelectKeys = String.Empty

                        'モデルの内容をセッションに保持
                        ViewModel.States = StatusType.SuccessNoHakko
                    End If

                Catch ex As Exception
                    ViewModel.ErrCode = CommonLabel.MSG_ERR_UPDATE_FAILURE
                    tran.Rollback()
                    logger.Error(ex.Message)
                End Try
            End Using

            Session(CommonSessionKey.CREAGE_ASSET_LABEL_PRINT) = ViewModel

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
#End Region

#Region "ラベル発行用CSV出力処理"
        ''' <summary>
        ''' ラベル発行用CSV出力処理
        ''' </summary>
        ''' <param name="SelectKeys">画面で選択された項目のキー値(カンマ区切り)</param>
        ''' <param name="PrinterKey">選択したラベルプリンターのKEY1</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function DbUpdateForLabel(ViewModel As AssetLabelPrintViewModel, SelectKeys As String, PrinterKey As String) As ActionResult
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

            Dim AssetListData As List(Of AssetListLabelModel)

            '最新のチェック状態でセッション情報を更新する(追加および削除) (キー：資産番号, カテゴリーL)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            'マスタデータ取得
            AssetListData = GetSelectAssetData("'" & String.Join("','", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetLabelPrintKey.ASSET_NO))) & "'")

            'DB処理
            Dim db As SystemDBContext = New SystemDBContext
            Dim sUpdTask As String = Me._Message.getMessage(CommonLabel.BUTTON_LABEL_PRINT)

            Using tran = db.Database.BeginTransaction()
                Try
                    '資産マスタ、入庫予定T、履歴TのDB更新
                    Dim errMessage = AssetDbUpdateProcess(db, lstSelectKeys, AssetListData, sUpdTask, Misc.ProcessDiv.LI, _LoginInfo)

                    If Not String.IsNullOrEmpty(errMessage) Then
                        ViewModel.ErrCode = errMessage
                    ElseIf Not ModelState.IsValid Then
                        ViewModel.ErrCode = CommonLabel.MSG_ERR_UPDATE_FAILURE
                    Else
                        tran.Commit()

                        '選択状態をセッションに保持（CSV発行用）
                        Session(CommonSessionKey.CREAGE_ASSET_LABEL_PRINT_FOR_LABEL) = lstSelectKeys

                        '選択状態をクリアにする
                        Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                        ViewModel.SelectKeys = String.Empty

                        'モデルの内容をセッションに保持
                        _ViewModel.States = StatusType.SuccessEdit

                    End If

                Catch ex As Exception
                    ViewModel.ErrCode = CommonLabel.MSG_ERR_UPDATE_FAILURE
                    tran.Rollback()
                    logger.Error(ex.Message)
                End Try
            End Using

            Session(CommonSessionKey.CREAGE_ASSET_LABEL_PRINT) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "ラベル発行用CSV出力処理"
        ''' <summary>
        ''' ラベル発行用CSV出力処理
        ''' </summary>
        ''' <param name="SelectKeys">画面で選択された項目のキー値(カンマ区切り)</param>
        ''' <param name="PrinterKey">選択したラベルプリンターのKEY1</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExportForLabel(ViewModel As AssetLabelPrintViewModel, SelectKeys As String, PrinterKey As String) As ActionResult
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

            'チェック状態を取得する
            Dim lstSelectKeys As List(Of String) = Session(CommonSessionKey.CREAGE_ASSET_LABEL_PRINT_FOR_LABEL)

            'マスタデータ取得
            Dim AssetListData As List(Of AssetListLabelModel)
            AssetListData = GetSelectAssetData("'" & String.Join("','", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetLabelPrintKey.ASSET_NO))) & "'")

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ラベルプリンター情報の取得
            Dim labelPrinter As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LabelPrinter.KeyDiv, PrinterKey, CommonSettingKey.LabelPrinter.Key2, CommonSettingKey.LabelPrinter.Key3, True)

            '出力ファイル名取得
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.AssetLabel.KeyDiv, CommonSettingKey.AssetLabel.Key1, CommonSettingKey.AssetLabel.Key2, CommonSettingKey.AssetLabel.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = System.IO.Path.GetFullPath(labelPrinter.DATA3)

            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆CSVファイル出力先設定
            Dim sUrlFileName As String = Path.GetRandomFileName().Substring(0, 8)
            Dim sPrnFileName As String = String.Format(sFileNm & sUrlFileName & "{0}.csv", Now.ToString("HHmmss"))
            SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sPrnFileName)

            '◆CSV出力用リスト作成

            'データ行挿入
            Dim col As String = ""
            For Each row As AssetListLabelModel In AssetListData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(LabelType.AS)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(labelPrinter.DATA1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(labelPrinter.DATA2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_ID)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.EPC_SIZE)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.GEN_ASSET_ID)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_DIV)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAKER)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PRODUCT_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MODEL_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_NO)
                If row.LEASE_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.LEASE_LIMIT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                If row.GENERIC_LIMIT.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(row.GENERIC_LIMIT.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM1)
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
                col += "," & SVClsCSVExp.SVFncCSVAddDquot("1")                          '発行枚数の1 を追加
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD.Trim)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_NAME)
                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                Dim labelPrintMethodSelect As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LabelPrntMethodSelect.KeyDiv, CommonSettingKey.LabelPrntMethodSelect.Key1, CommonSettingKey.LabelPrntMethodSelect.Key2, CommonSettingKey.LabelPrntMethodSelect.Key3, False)

                If labelPrintMethodSelect IsNot Nothing AndAlso labelPrintMethodSelect.DATA1 = "Y" Then
                    'ファイルをダウンロードさせる
                    Call FileDownload(sPrnFileName, "text/comma-separated-values", sFullPath + "\" + sPrnFileName)
                    'ダウンロードさせた後、ファイルを削除。
                    System.IO.File.Delete(sFullPath + "\" + sPrnFileName)
                End If

                'ファイル更新
                Dao.ToolAsset.UpdateAssetStat(AssetListData, _LoginInfo)

                _ViewModel.States = StatusType.SuccessHakko

                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_ASSET_LABEL_PRINT) = _ViewModel
            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetLabelPrint.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_LABEL_PRINT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     lstSelectKeys.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetLabelPrint.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "ラベル発行用CSV出力のデータを取得"
        ''' <summary>
        ''' ラベル発行用CSV出力のデータを取得します
        ''' </summary>
        Private Function GetSelectAssetData(SelectKeys As String) As List(Of AssetListLabelModel)
            Dim db As SystemDBContext = New SystemDBContext

            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  ASSET.ASSET_NO ")
            sbSQL.AppendLine("  , ASSET.ASSET_ID ")
            sbSQL.AppendLine("  , ASSET.EPC_SIZE ")
            sbSQL.AppendLine("  , ASSET.GEN_ASSET_ID ")
            sbSQL.AppendLine("  , ASSET.ASSET_DIV ")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("  , IsNull(ASSET.LOC_L_CD, '') AS LOC_L_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.LOC_M_CD, '') AS LOC_M_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.LOC_S_CD, '') AS LOC_S_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.CAT_L_CD, '') AS CAT_L_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.CAT_M_CD, '') AS CAT_M_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.CAT_S_CD, '') AS CAT_S_CD")
            sbSQL.AppendLine("  , IsNull(ASSET.MNG_LOC_L_CD, '') AS MNG_LOC_L_CD")
            sbSQL.AppendLine("  , ASSET.MAKER ")
            sbSQL.AppendLine("  , ASSET.PRODUCT_NO ")
            sbSQL.AppendLine("  , ASSET.MODEL_NO ")
            sbSQL.AppendLine("  , ASSET.LEASE_NO ")
            sbSQL.AppendLine("  , ASSET.LEASE_LIMIT ")
            sbSQL.AppendLine("  , ASSET.GENERIC_LIMIT ")
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
            sbSQL.AppendLine("  , ASSET.TAG_LINK_STATUS ")
            sbSQL.AppendLine("  , LOCL.LOC_NAME AS LOC_L_NAME ")
            sbSQL.AppendLine("  , LOCM.LOC_NAME AS LOC_M_NAME ")
            sbSQL.AppendLine("  , LOCS.LOC_NAME AS LOC_S_NAME ")
            sbSQL.AppendLine("  , CATL.CAT_NAME AS CAT_L_NAME ")
            sbSQL.AppendLine("  , CATM.CAT_NAME AS CAT_M_NAME ")
            sbSQL.AppendLine("  , CATS.CAT_NAME AS CAT_S_NAME ")
            sbSQL.AppendLine("  , MNG_LOCL.LOC_NAME AS MNG_LOC_L_NAME ")
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  M_ASSET ASSET ")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
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
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNG_LOCL ")
            sbSQL.AppendLine("ON MNG_LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.MNG_LOC_L_CD = MNG_LOCL.LOC_L_CD")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  ASSET.ASSET_NO in (" & SelectKeys & ") ")
            sbSQL.AppendLine(" ORDER BY ASSET_NO")

            'SQLを実行する
            Dim lst As List(Of AssetListLabelModel)
            lst = db.AssetListLabel.SqlQuery(sbSQL.ToString).ToList

            'データを返却
            Return lst

        End Function
#End Region

#Region "ラベル発行用のDB処理"
        ''' <summary>
        ''' ラベル発行用CSV出力のデータを取得します
        ''' </summary>
        ''' <param name="db">DBシステムコンテキスト</param>
        ''' <param name="lstSelectKeys">チェック状態一覧</param>
        ''' <param name="AssetListData">既存の資産マスタデータ</param>
        ''' <param name="sUpdTask">更新処理</param>
        ''' <param name="sProcessDiv">処理区分（更新値）</param>
        ''' <returns>エラーメッセージ</returns>
        Private Function AssetDbUpdateProcess(ByVal db As SystemDBContext,
                                                 ByVal lstSelectKeys As List(Of String),
                                                 ByVal AssetListData As List(Of AssetListLabelModel),
                                                 ByVal sUpdTask As String,
                                                 ByVal sProcessDiv As String,
                                                 ByVal loginInfo As RfidEntity.LoginUser) As String
            'チェック状態の資産番号のみ抽出
            Dim lstAssetNo As New List(Of String)
            For Each selectKey In lstSelectKeys
                Dim parts = selectKey.Split(","c)
                If parts.Length > 0 Then
                    lstAssetNo.Add(parts(0).Trim())
                End If
            Next

            'カテゴリマスタ、入庫予定テーブルデータ取得
            Dim categoryList As List(Of CategoryModel) = db.Category.ToList()
            Dim tInAssetList As List(Of AssetLabelPrintModel) = db.AssetLabelPrint.Where(Function(c) lstAssetNo.Contains(c.ASSET_NO)).ToList()

            If categoryList.Count = 0 OrElse tInAssetList.Count = 0 Then
                Return CommonLabel.MSG_ERR_UPDATE_FAILURE
            End If

            '更新可能拠点かチェック
            Dim AuthInfo As AuthInfo = Misc.CommonRfid.MessageRfid.GetFunctionAuth(Misc.Auth.FunctionId.AssetLabelPrint, loginInfo)
            Dim updatableLocL = Dao.ToolLocation.GetUpdatableLocLList(loginInfo, AuthInfo)
            Dim locLCodes As New HashSet(Of String)(From r As DataRow In updatableLocL.Rows
                                                    Select Convert.ToString(r("LOC_CD")))
            Dim hasInvalid As Boolean = tInAssetList.Any(Function(a) Not locLCodes.Contains(a.MNG_LOC_L_CD))

            If hasInvalid Then
                Return CommonLabel.MSG_ERR_EXIST_ASSET_COMPANY_NOT_UPDATABLE
            End If

            '資産マスタデータ更新
            UpdateMAssetFromTInAsset(db, lstAssetNo, sUpdTask)

            '資産マスタデータ登録
            For Each sAssetNo As String In lstAssetNo
                Dim asset As AssetListLabelModel = AssetListData.FirstOrDefault(Function(d) d.ASSET_NO = sAssetNo)

                If asset Is Nothing Then
                    Dim inAsset As AssetLabelPrintModel = tInAssetList.FirstOrDefault(Function(d) d.ASSET_NO = sAssetNo)
                    Dim cat = categoryList.Where(Function(c) c.CAT_L_CD = inAsset.CAT_L_CD)
                    InsertMAssetFromTInAsset(db, sAssetNo, cat(0), sUpdTask)
                End If
            Next

            '入庫予定テーブル削除
            DeleteTInAsset(db, lstAssetNo)

            '履歴データ登録
            InsertTProcessLogFromMAsset(db, lstAssetNo, sProcessDiv, sUpdTask)

            Return String.Empty

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

#Region "EXCELファイルのエラーチェック"
        ''' <summary>
        ''' EXCELファイルのエラーチェック
        ''' </summary>
        ''' <param name="file">チェック対象のファイル</param>
        ''' <returns>True:エラー無し　False:エラー有り</returns>
        Public Function IsImportFileError(file As HttpPostedFileBase) As Boolean
            Dim blRet As Boolean = False

            'ファイルがあるかどうか
            If IsNothing(file) Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.NoSelect})
                Return blRet
            End If

            'ファイルサイズ
            If file.ContentLength = 0 Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.Empty})
                Return blRet
            End If

            '拡張子を検証
            Dim extention As String = Path.GetExtension(file.FileName).Replace(".", "")
            If String.IsNullOrEmpty(extention) OrElse Not {"xls", "xlsx"}.Contains(extention.ToLower()) Then
                Me._lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.NotExcel})
                Return blRet
            End If

            blRet = True
            Return blRet
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

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLabelPrint, _LoginInfo)

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
#Region "検索条件のロケーションの選択肢（場所から選ぶ）"
        ''' <summary>
        ''' 検索条件のロケーション（原価センタ・場所）の選択肢を返す（画面の ajax から呼ぶ）
        ''' </summary>
        ''' <param name="selectLCode">会社（大区分コード）</param>
        ''' <param name="level">返す側（M：中区分、S：小区分）</param>
        ''' <param name="reverse">場所から選ぶ（True：場所で原価センタを絞り込む、False：原価センタで場所を絞り込む）</param>
        ''' <param name="codes">絞り込みに使う、反対側の選択中のコード（「,」区切り）</param>
        ''' <returns>選択肢（LOC_CD／LOC_NAME のリスト）</returns>
        Public Function GetSearchLocationOptions(ByVal selectLCode As String, ByVal level As String, ByVal reverse As Boolean, ByVal codes As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
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
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLabelPrint, _LoginInfo)

            Return Json(GetSearchLocationList(level, reverse, CommonRfid.ToStringNvl(codes).Split(","c), selectLCode, authInfo))

        End Function

        ''' <summary>
        ''' 検索条件の原価センタ・場所の選択肢を、場所から選ぶの ON／OFF と選択値に合わせて作る
        ''' </summary>
        ''' <param name="authInfo">権限情報</param>
        Private Sub SetSearchLocationOptions(authInfo As AuthInfo)
            Dim selLocM As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocM)
            Dim selLocS As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocS)

            ViewBag.modelLocM = New MultiSelectList(
                GetSearchLocationList(LOC_DIV.LocationM, _ViewModel.SearchLocReverse, selLocS, _ViewModel.SearchLocL, authInfo),
                "LOC_CD", "LOC_NAME", selLocM)
            ViewBag.modelLocS = New MultiSelectList(
                GetSearchLocationList(LOC_DIV.LocationS, _ViewModel.SearchLocReverse, selLocM, _ViewModel.SearchLocL, authInfo),
                "LOC_CD", "LOC_NAME", selLocS)
        End Sub

        ''' <summary>
        ''' 検索条件のロケーションの選択肢を取得する（権限あり かつ 入庫予定Tに存在するもの）
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
            Dim exTarget As Dao.ToolLocation.LocExistTarget = Dao.ToolLocation.LocExistTarget.InAsset()
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

            'モーダル画面のロケーションコンボ設定
            Dim SelectedCd As String = _ViewModel.LOC_L_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", "")
            Else
                ViewBag.modalmodelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", SelectedCd)
            End If

            Return Json(lstLocation)
        End Function
#End Region

#Region "中区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 中区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLocationMData(ByVal selectLCode As String) As JsonResult
            Dim tbl As DataTable
            If Not _LoginInfo Is Nothing Then
                tbl = Dao.ToolLocation.GetLocationMData(selectLCode, _LoginInfo, _AuthInfo)
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

            'モーダル画面のロケーションコンボ設定
            Dim SelectedCd As String = _ViewModel.LOC_M_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelLocM = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", "")
            Else
                ViewBag.modalmodelLocM = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", SelectedCd)
            End If

            Return Json(lstLocation)
        End Function
#End Region

#Region "小区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCodes As String()) As JsonResult
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            '中区分は複数選択のため、選択された中区分ごとに小区分を取得してマージする
            If Not _LoginInfo Is Nothing Then
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
            End If

            'コード順に並べ替える
            lstLocation = lstLocation.OrderBy(Function(loc) loc.LOC_CD).ToList

            '一覧画面のロケーションコンボ設定（複数選択のため MultiSelectList）
            ViewBag.modelLocS = New MultiSelectList(lstLocation, "LOC_CD", "LOC_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchLocS))
            _ViewModel.locSDropdownData = lstLocation

            'モーダル画面のロケーションコンボ設定
            Dim SelectedCd As String = _ViewModel.LOC_S_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", "")
            Else
                ViewBag.modalmodelLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", SelectedCd)
            End If

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

            'モーダル画面のカテゴリーコンボ設定
            Dim SelectedCd As String = _ViewModel.CAT_L_CD
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalmodelCatL = New SelectList(lst, "CAT_CD", "CAT_NAME", "")
            Else
                ViewBag.modalmodelCatL = New SelectList(lst, "CAT_CD", "CAT_NAME", SelectedCd)
            End If

            Return Json(lst)
        End Function
#End Region

#Region "プリンター選択プルダウン用データ取得"
        ''' <summary>
        ''' プリンター選択プルダウン用データ取得
        ''' </summary>
        ''' <returns></returns>
        Public Function ShowPrinterList(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As ActionResult

            '最新のチェック状態でセッション情報を更新する(追加および削除)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(SelectKeys, NoneSelectKeys)

            Dim db As New SystemDBContext

            '権限による実行可能チェック
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLabelPrint, _LoginInfo)

            Dim sbAuthChkSQL As StringBuilder = New StringBuilder
            Dim lstParam As New List(Of SqlParameter)

            Dim sSelASSET_NO As String = "'" & String.Join("','", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetLabelPrintKey.ASSET_NO))) & "'"

            sbAuthChkSQL.AppendLine("SELECT TOP 1 *")
            sbAuthChkSQL.AppendLine("FROM T_IN_ASSET")
            sbAuthChkSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC")
            sbAuthChkSQL.AppendLine("    ON T_IN_ASSET.MNG_LOC_L_CD = ULOC.LOC_L_CD  ")
            sbAuthChkSQL.AppendLine("    AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = T_IN_ASSET.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
            lstParam.Add(New SqlParameter("@EmployeeCd", $"{_LoginInfo.EmployeeCd}"))
            lstParam.Add(New SqlParameter("@ReadAuthFlg", $"{_AuthInfo.ReadAuthFlg}"))
            lstParam.Add(New SqlParameter("@UpdAuthFlg", $"{_AuthInfo.UpdAuthFlg}"))
            lstParam.Add(New SqlParameter("@ReadHubAuthFlg", $"{_AuthInfo.ReadHubAuthFlg}"))
            lstParam.Add(New SqlParameter("@UpdHubAuthFlg", $"{_AuthInfo.UpdHubAuthFlg}"))
            lstParam.Add(New SqlParameter("@UpdAuthOption", $"{_AuthInfo.AuthOption}"))
            sbAuthChkSQL.AppendLine("WHERE T_IN_ASSET.ASSET_NO in (" & sSelASSET_NO & ") ")
            sbAuthChkSQL.AppendLine("AND   ULOC.UPD_FLG = '" & Misc.UpdFlg.No & "'")

            'SQLを実行する
            Dim chkModel As List(Of AssetLabelPrintModel) = db.AssetLabelPrint.SqlQuery(sbAuthChkSQL.ToString, lstParam.ToArray).ToList
            If chkModel.Count > 0 Then
                Return Json("auth-error")
            End If

            'カテゴリーを重複なしで取得する(資産番号, カテゴリーL)
            Dim Cals As String() = lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(AssetLabelPrintKey.CATEGORY_L)).Distinct.ToArray
            Dim sCalsStr As String = "'" & String.Join("','", Cals) & "'"

            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT TOP 1 *")
            sbSQL.AppendLine("FROM M_CATEGORY")
            sbSQL.AppendLine("WHERE CAT_DIV = '" & Misc.CAT_DIV.CategoryL & "'")
            sbSQL.AppendLine("AND   CAT_L_CD in (" & sCalsStr & ") ")
            sbSQL.AppendLine("AND   ASSET_ID_REG_DIV = " & Misc.AssetIDGenerationMethod.None.ToString)

            'SQLを実行する
            Dim catmodel As List(Of CategoryModel) = db.Category.SqlQuery(sbSQL.ToString).ToList
            If catmodel.Count > 0 Then
                Return Json("error")
            End If


            Dim lstSetting As List(Of PrinterDropdownModel)   'プリンタリスト
            lstSetting = GetPrinter()
            Return Json(lstSetting)
        End Function
#End Region

#Region "プリンタ情報取得"
        ''' <summary>
        ''' プリンタ情報取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetPrinter() As List(Of PrinterDropdownModel)
            Dim tbl As DataTable = Dao.ToolSetting.GetMstPrinterInfo()
            Dim lst As New List(Of PrinterDropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New PrinterDropdownModel With {
                        .KEY1 = row("KEY1").ToString,
                        .DATA1 = row("DATA1").ToString
                    }
                )
            Next
            ViewBag.modelPrinter = New SelectList(lst, "KEY1", "DATA1")
            Return lst
        End Function
#End Region

#Region "データを基にチェック状態でセッション情報を更新する"
        ''' <summary>
        ''' データを基にチェック状態でセッション情報を更新する
        ''' </summary>
        ''' <remarks>設定マスタに該当カテゴリー大分類コードが登録されている場合にチェック</remarks>
        Private Function UpdateChecedTangible() As List(Of String)
            'チェック状態をセットする
            Dim lstAssetLabelPrint As New List(Of AssetLabelPrintViewModel)

            '現在表示検索時の検索条件、パラメーターを取得する
            Dim sbWhereSQL As String = Session(CommonSessionKey.LIST_VIEW_SQL_WHERE)
            Dim lstParam As List(Of SqlParameter) = Session(CommonSessionKey.LIST_VIEW_PARAM)

            Dim lst As List(Of String) = Dao.ToolInAsset.GetInAssetTangible(sbWhereSQL, lstParam)

            'セッションの選択キー情報を取得する
            'セッションを更新する
            Session(CommonSessionKey.LIST_CHECKED_KEYS) = lst

            'チェックされた行のキーリストを返却する
            Return lst

        End Function
#End Region

#Region "全選択ボタン押下時処理"
        ''' <summary>
        ''' 全選択ボタン押下時処理
        ''' 表示されている検索条件で全件検索し
        ''' チェック項目としてセッションにセットする
        ''' </summary>
        ''' <param name="blnSelectAll"></param>
        ''' <returns></returns>
        Public Function SelectAll(ByVal blnSelectAll As Boolean) As ActionResult
            Dim sbSQL As New StringBuilder
            Dim lstAssetLabelPrint As New List(Of AssetLabelPrintViewModel)

            '選択解除の場合、初期化する
            If Not blnSelectAll Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                Return Json("0")
            Else
                '全選択ボタン押下フラグをセットする　'2020/02/12 nv miyaji　
                Session(CommonSessionKey.ALL_SELECT_FLG) = True
            End If

            '現在表示検索時のSQLを取得する
            Dim strSQL As String = Session(CommonSessionKey.LIST_VIEW_SQL)
            Dim lstParam As List(Of SqlParameter) = Session(CommonSessionKey.LIST_VIEW_PARAM)

            'パラメーターをコピーする(SqlParameterがSqlParameterCollectionに含まれるエラー回避のため)
            Dim lstParamCopy As New List(Of SqlParameter)
            Dim param As New SqlParameter
            For Each param In lstParam
                lstParamCopy.Add(New SqlParameter(param.ParameterName, param.Value))
            Next

            'SQLを実行する
            If Not IsNothing(strSQL) Then
                If lstParam.Count <= 0 Then
                    lstAssetLabelPrint = _db.AssetLabelPrintView.SqlQuery(strSQL).ToList
                Else

                    lstAssetLabelPrint = _db.AssetLabelPrintView.SqlQuery(strSQL, lstParamCopy.ToArray).ToList
                End If
            End If

            'チェックボックスlstAssetLabelPrintの選択状態のキー(資産番号)を更新する
            If lstAssetLabelPrint.Count > 0 Then
                Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstAssetLabelPrint.Select(Function(d) $"{d.ASSET_NO},{d.CAT_L_CD}").ToList
            End If

            '件数を返却する
            Return Json(lstAssetLabelPrint.Count)

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
            If Not IsNothing(_ViewModel.ASSET_NO) Then
                Dim results = From data In _viewResult Where data.ASSET_NO = _ViewModel.ASSET_NO
                If results.Count <= 0 Then
                    NoneSelectKeys = String.Join("/", New String() {NoneSelectKeys, $"{_ViewModel.ASSET_NO},{_ViewModel.CAT_L_CD}"})
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

#Region "共通"

        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL(Optional ByVal isSort As Boolean = True) As String
            Dim sSelKey As String = String.Empty

            '権限情報の取得
            If Not _LoginInfo Is Nothing Then
                _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLabelPrint, _LoginInfo)
                '更新可能な権限か
                If _AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.hasUpdAuthFlg = AUTH_FLG.Yes
                Else
                    ViewBag.hasUpdAuthFlg = AUTH_FLG.No
                End If
            Else
                ViewBag.hasUpdAuthFlg = AUTH_FLG.No
            End If

            '検索条件
            Dim sbWhr As New StringBuilder
            If String.IsNullOrEmpty(sSelKey) Then
                'ロケ大区分コード
                If Not String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                    sbWhr.AppendLine("  AND INA.LOC_L_CD = @SearchLocL")
                    addSqlParam("SearchLocL", $"{_ViewModel.SearchLocL}")
                End If

                'ロケ中区分コード（複数選択）
                Dim sLocMCond As String = CreateInCondition("INA.LOC_M_CD", "SearchLocM", _ViewModel.SearchLocM)
                If Not String.IsNullOrEmpty(sLocMCond) Then
                    sbWhr.AppendLine(sLocMCond)
                End If

                'ロケ小区分コード（複数選択）
                Dim sLocSCond As String = CreateInCondition("INA.LOC_S_CD", "SearchLocS", _ViewModel.SearchLocS)
                If Not String.IsNullOrEmpty(sLocSCond) Then
                    sbWhr.AppendLine(sLocSCond)
                End If

                'カテゴリ大分類 SearchCatL（複数選択）
                Dim sCatLCond As String = CreateInCondition("INA.CAT_L_CD", "SearchCatL", _ViewModel.SearchCatL)
                If Not String.IsNullOrEmpty(sCatLCond) Then
                    sbWhr.AppendLine(sCatLCond)
                End If

                '有形・無形区分
                If _ViewModel.SearchIsTangibleY <> _ViewModel.SearchIsTangibleN Then
                    '有形資産、無形資産の値が違う場合は条件に追加
                    sbWhr.AppendLine("  AND INA.IS_TANGIBLE = " & "@SearchIsTangible")
                    If _ViewModel.SearchIsTangibleY Then
                        addSqlParam("SearchIsTangible", $"{ TANGIBLE.TANGIBLE }")
                    Else
                        addSqlParam("SearchIsTangible", $"{ TANGIBLE.NOT_TANGIBLE }")
                    End If
                End If

                '資産番号 SearchAssetNo（「|」区切りで複数指定。資産番号・子番号のいずれかに前方一致）
                Dim sAssetNoCond As String = CreateLikeCondition({"INA.ASSET_NO", "INA.SUB_ASSET_NO"}, "SearchAssetNo", CommonRfid.GetMultiInputValues(_ViewModel.SearchAssetNo))
                If Not String.IsNullOrEmpty(sAssetNoCond) Then
                    sbWhr.AppendLine(sAssetNoCond)
                End If

                '資産名称 SearchAssetName
                If (Not IsNothing(_ViewModel.SearchAssetName)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchAssetName.Trim)) Then
                    sbWhr.AppendLine("  AND INA.ASSET_NAME like " & "@SearchAssetName")
                    addSqlParam("SearchAssetName", $"{"%" & _ViewModel.SearchAssetName.Trim & "%"}")
                End If

                'フリー項目6 SearchFreeItem6 管理社員番号
                If (Not IsNothing(_ViewModel.SearchFreeItem6)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem6.Trim)) Then
                    sbWhr.AppendLine("  AND INA.FREE_ITEM6 like " & "@SearchFreeItem6")
                    addSqlParam("SearchFreeItem6", $"{"%" & _ViewModel.SearchFreeItem6.Trim & "%"}")
                End If

                'フリー項目15 SearchFreeItem15 資産グループコード
                If (Not IsNothing(_ViewModel.SearchFreeItem15)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem15.Trim)) Then
                    sbWhr.AppendLine("  AND INA.FREE_ITEM15 like " & "@SearchFreeItem15")
                    addSqlParam("SearchFreeItem15", $"{"%" & _ViewModel.SearchFreeItem15.Trim & "%"}")
                End If

                '取込日FROM
                If (Not IsNothing(_ViewModel.SearchIpDateFrom)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchIpDateFrom.Trim)) Then
                    Dim fromDt As DateTime = DateTime.Parse(_ViewModel.SearchIpDateFrom).Date
                    Dim fromNum As String = Long.Parse(fromDt.ToString("yyyyMMddHHmmss"))
                    sbWhr.AppendLine("  AND INA.IP_DATETIME >= " & "@SearchIpDateFrom")
                    addSqlParam("SearchIpDateFrom", fromNum)
                End If

                '取込日TO
                If (Not IsNothing(_ViewModel.SearchIpDateTo)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchIpDateTo.Trim)) Then
                    Dim toDt As DateTime = DateTime.Parse(_ViewModel.SearchIpDateTo).Date.AddDays(1).AddSeconds(-1)
                    Dim toNum As String = Long.Parse(toDt.ToString("yyyyMMddHHmmss"))
                    sbWhr.AppendLine("  AND INA.IP_DATETIME <= " & "@SearchIpDateTo")
                    addSqlParam("SearchIpDateTo", toNum)
                End If

                '取込担当者コード
                If (Not IsNothing(_ViewModel.SearchInsEmployeeCd)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchInsEmployeeCd.Trim)) Then
                    sbWhr.AppendLine("  AND INA.IP_EMPLOYEE_CD like " & "@SearchInsEmployeeCd")
                    addSqlParam("SearchInsEmployeeCd", $"{"%" & _ViewModel.SearchInsEmployeeCd.Trim & "%"}")
                End If
            End If

            'Where文を有形/無形区分から選択状態表示のためのセッションに保存
            Session(CommonSessionKey.LIST_VIEW_SQL_WHERE) = sbWhr.ToString

            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            If isSort Then
                sbSQL.AppendLine("SELECT ")
            Else
                sbSQL.AppendLine("SELECT TOP 1")
            End If

            sbSQL.AppendLine("    INA.ASSET_NO ")
            sbSQL.AppendLine("  , INA.SUB_ASSET_NO ")
            sbSQL.AppendLine("  , INA.ASSET_NAME ")
            sbSQL.AppendLine("  , INA.MNG_LOC_L_CD ")
            sbSQL.AppendLine("  , INA.LOC_L_CD ")
            sbSQL.AppendLine("  , INA.LOC_M_CD ")
            sbSQL.AppendLine("  , INA.LOC_S_CD ")
            sbSQL.AppendLine("  , INA.CAT_L_CD ")
            sbSQL.AppendLine("  , INA.ASSET_STATUS ")
            sbSQL.AppendLine("  , INA.FREE_ITEM1 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM2 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM3 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM4 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM5 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM6 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM7 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM8 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM9 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM10 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM11 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM12 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM13 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM14 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM15 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM16 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM17 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM18 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM19 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM20 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM21 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM22 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM23 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM24 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM25 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM26 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM27 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM28 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM29 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM30 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM31 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM32 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM33 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM34 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM35 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM36 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM37 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM38 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM39 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM40 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM41 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM42 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM43 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM44 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM45 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM46 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM47 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM48 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM49 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM50 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM51 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM52 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM53 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM54 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM55 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM56 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM57 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM58 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM59 ")
            sbSQL.AppendLine("  , INA.FREE_ITEM60 ")
            sbSQL.AppendLine("  , INA.IS_TANGIBLE ")
            sbSQL.AppendLine("  , INA.IP_DATETIME ")
            sbSQL.AppendLine("  , INA.IP_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , INA.IP_FILE_NAME ")
            sbSQL.AppendLine("  , INA.INS_DATETIME ")
            sbSQL.AppendLine("  , INA.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , INA.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , INA.UPD_DATETIME ")
            sbSQL.AppendLine("  , INA.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , INA.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , INA.UPD_TASK ")
            sbSQL.AppendLine("  , LOCL.LOC_NAME AS LocLName ")
            sbSQL.AppendLine("  , LOCM.LOC_NAME AS LocMName ")
            sbSQL.AppendLine("  , LOCS.LOC_NAME AS LocSName ")
            sbSQL.AppendLine("  , CATL.CAT_NAME AS CatLName ")
            sbSQL.AppendLine("  , EMP.EMPLOYEE_NAME As IpEmployeeName")
            sbSQL.AppendLine("  , MNGEMP.EMPLOYEE_NAME AS FREE_ITEM6_NAME ")
            sbSQL.AppendLine("  , ULOC.UPD_FLG AS UPD_FLG ")
            sbSQL.AppendLine("  , MNGLOCL.LOC_NAME AS MngLocLName ")
            sbSQL.AppendLine("  , TotalCount ")
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  T_IN_ASSET INA")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC")
            sbSQL.AppendLine("ON INA.MNG_LOC_L_CD = ULOC.LOC_L_CD ")
            sbSQL.AppendLine("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = INA.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")

            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND INA.LOC_L_CD = LOCL.LOC_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL ")
            sbSQL.AppendLine("ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND INA.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM ")
            sbSQL.AppendLine("ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
            sbSQL.AppendLine("AND INA.LOC_L_CD = LOCM.LOC_L_CD")
            sbSQL.AppendLine("AND INA.LOC_M_CD = LOCM.LOC_M_CD")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS ")
            sbSQL.AppendLine("ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
            sbSQL.AppendLine("AND INA.LOC_L_CD = LOCS.LOC_L_CD")
            sbSQL.AppendLine("AND INA.LOC_M_CD = LOCS.LOC_M_CD")
            sbSQL.AppendLine("AND INA.LOC_S_CD = LOCS.LOC_S_CD")
            sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATL ")
            sbSQL.AppendLine("ON CATL.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
            sbSQL.AppendLine("AND INA.CAT_L_CD = CATL.CAT_L_CD")
            sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE EMP ")
            sbSQL.AppendLine("ON INA.IP_EMPLOYEE_CD = EMP.EMPLOYEE_CD")
            sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE MNGEMP ")
            sbSQL.AppendLine("ON INA.FREE_ITEM6 = MNGEMP.EMPLOYEE_CD")

            sbSQL.AppendLine("LEFT JOIN (")
            sbSQL.AppendLine("  SELECT COUNT(*) TotalCount")
            sbSQL.AppendLine("  FROM T_IN_ASSET INA ")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC")
            sbSQL.AppendLine("ON INA.MNG_LOC_L_CD = ULOC.LOC_L_CD ")
            sbSQL.AppendLine("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = INA.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
            sbSQL.AppendLine("  WHERE 1 = 1")
            sbSQL.AppendLine("  " & sbWhr.ToString)
            sbSQL.AppendLine(") A")
            sbSQL.AppendLine("ON 1=1")

            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{_AuthInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{_AuthInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{_AuthInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{_AuthInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthOption", $"{_AuthInfo.AuthOption}")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            sbSQL.AppendLine(sbWhr.ToString)

            'ORDER句の設定
            If isSort Then
                '[CC431] UPD Start 20261015
                sbSQL.AppendLine(" ORDER BY " & GetOrderBy(_ViewModel.SearchSortOrder))
                '[CC431] UPD End 20261015
            End If

            'クエリを返却
            Return sbSQL.ToString

        End Function

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New AssetLabelPrintViewModel())
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
            _viewResult.ElementAt(0).SearchIsTangibleY = _ViewModel.SearchIsTangibleY
            _viewResult.ElementAt(0).SearchIsTangibleN = _ViewModel.SearchIsTangibleN
            _viewResult.ElementAt(0).SearchAssetNo = _ViewModel.SearchAssetNo
            _viewResult.ElementAt(0).SearchAssetName = _ViewModel.SearchAssetName
            _viewResult.ElementAt(0).SearchFreeItem6 = _ViewModel.SearchFreeItem6
            _viewResult.ElementAt(0).SearchFreeItem15 = _ViewModel.SearchFreeItem15
            _viewResult.ElementAt(0).SearchIpDateFrom = _ViewModel.SearchIpDateFrom
            _viewResult.ElementAt(0).SearchIpDateTo = _ViewModel.SearchIpDateTo
            _viewResult.ElementAt(0).SearchInsEmployeeCd = _ViewModel.SearchInsEmployeeCd
            _viewResult.ElementAt(0).SearchSortOrder = _ViewModel.SearchSortOrder
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_LABEL_PRINT, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL))
            If _viewResult.ElementAt(0).GridInfo.Count > 0 Then
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.ASSET_LABEL_PRINT, IIf(String.IsNullOrEmpty(_ViewModel.SearchLocL), COLSET_LOC.TEMPLATE, _ViewModel.SearchLocL)).FIX_COL_CNT
            Else
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.ASSET_LABEL_PRINT, COLSET_LOC.TEMPLATE).FIX_COL_CNT
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.ASSET_LABEL_PRINT, COLSET_LOC.TEMPLATE)
            End If
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
            _viewResult.ElementAt(0).IsRestoreOwnSearchConditions = _ViewModel.IsRestoreOwnSearchConditions
            _viewResult.ElementAt(0).FirstSearch = _ViewModel.FirstSearch
            _viewResult.ElementAt(0).needToInitCheckbox = _ViewModel.needToInitCheckbox
            _viewResult.ElementAt(0).SelectKeys = _ViewModel.SelectKeys
            _viewResult.ElementAt(0).NoneSelectKeys = _ViewModel.NoneSelectKeys
            _viewResult.ElementAt(0).PrinterKey = _ViewModel.PrinterKey
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

#Region "資産マスタ/入庫予定T/履歴操作メソッド"

        ''' <summary>
        ''' 資産マスタにデータを登録する（入庫予定テーブルから値取得）
        ''' </summary>
        ''' <param name="db">DBシステムコンテキスト</param>
        ''' <param name="sAssetNo">資産番号（更新キー）</param>
        ''' <param name="cat">カテゴリデータ</param>
        ''' <param name="sUpdTask">更新処理</param>
        Private Sub InsertMAssetFromTInAsset(ByVal db As SystemDBContext, ByVal sAssetNo As String, ByVal cat As CategoryModel, ByVal sUpdTask As String)
            Dim sbSQL As StringBuilder = New StringBuilder

            sbSQL.AppendLine("  INSERT INTO M_ASSET")
            sbSQL.AppendLine("  (")
            sbSQL.AppendLine("     ASSET_NO")
            sbSQL.AppendLine("    ,ASSET_ID")
            sbSQL.AppendLine("    ,SUB_ASSET_NO")
            sbSQL.AppendLine("    ,EPC_SIZE")
            sbSQL.AppendLine("    ,GEN_ASSET_ID")
            sbSQL.AppendLine("    ,ASSET_DIV")
            sbSQL.AppendLine("    ,ASSET_NAME")
            sbSQL.AppendLine("    ,MNG_LOC_L_CD")
            sbSQL.AppendLine("    ,LOC_L_CD")
            sbSQL.AppendLine("    ,LOC_M_CD")
            sbSQL.AppendLine("    ,LOC_S_CD")
            sbSQL.AppendLine("    ,CAT_L_CD")
            sbSQL.AppendLine("    ,CAT_M_CD")
            sbSQL.AppendLine("    ,CAT_S_CD")
            sbSQL.AppendLine("    ,MAKER")
            sbSQL.AppendLine("    ,PRODUCT_NO")
            sbSQL.AppendLine("    ,MODEL_NO")
            sbSQL.AppendLine("    ,LEASE_NO")
            sbSQL.AppendLine("    ,LEASE_LIMIT")
            sbSQL.AppendLine("    ,GENERIC_LIMIT")
            sbSQL.AppendLine("    ,FREE_ITEM1")
            sbSQL.AppendLine("    ,FREE_ITEM2")
            sbSQL.AppendLine("    ,FREE_ITEM3")
            sbSQL.AppendLine("    ,FREE_ITEM4")
            sbSQL.AppendLine("    ,FREE_ITEM5")
            sbSQL.AppendLine("    ,FREE_ITEM6")
            sbSQL.AppendLine("    ,FREE_ITEM7")
            sbSQL.AppendLine("    ,FREE_ITEM8")
            sbSQL.AppendLine("    ,FREE_ITEM9")
            sbSQL.AppendLine("    ,FREE_ITEM10")
            sbSQL.AppendLine("    ,FREE_ITEM11")
            sbSQL.AppendLine("    ,FREE_ITEM12")
            sbSQL.AppendLine("    ,FREE_ITEM13")
            sbSQL.AppendLine("    ,FREE_ITEM14")
            sbSQL.AppendLine("    ,FREE_ITEM15")
            sbSQL.AppendLine("    ,FREE_ITEM16")
            sbSQL.AppendLine("    ,FREE_ITEM17")
            sbSQL.AppendLine("    ,FREE_ITEM18")
            sbSQL.AppendLine("    ,FREE_ITEM19")
            sbSQL.AppendLine("    ,FREE_ITEM20")
            sbSQL.AppendLine("    ,FREE_ITEM21")
            sbSQL.AppendLine("    ,FREE_ITEM22")
            sbSQL.AppendLine("    ,FREE_ITEM23")
            sbSQL.AppendLine("    ,FREE_ITEM24")
            sbSQL.AppendLine("    ,FREE_ITEM25")
            sbSQL.AppendLine("    ,FREE_ITEM26")
            sbSQL.AppendLine("    ,FREE_ITEM27")
            sbSQL.AppendLine("    ,FREE_ITEM28")
            sbSQL.AppendLine("    ,FREE_ITEM29")
            sbSQL.AppendLine("    ,FREE_ITEM30")
            sbSQL.AppendLine("    ,FREE_ITEM31")
            sbSQL.AppendLine("    ,FREE_ITEM32")
            sbSQL.AppendLine("    ,FREE_ITEM33")
            sbSQL.AppendLine("    ,FREE_ITEM34")
            sbSQL.AppendLine("    ,FREE_ITEM35")
            sbSQL.AppendLine("    ,FREE_ITEM36")
            sbSQL.AppendLine("    ,FREE_ITEM37")
            sbSQL.AppendLine("    ,FREE_ITEM38")
            sbSQL.AppendLine("    ,FREE_ITEM39")
            sbSQL.AppendLine("    ,FREE_ITEM40")
            sbSQL.AppendLine("    ,FREE_ITEM41")
            sbSQL.AppendLine("    ,FREE_ITEM42")
            sbSQL.AppendLine("    ,FREE_ITEM43")
            sbSQL.AppendLine("    ,FREE_ITEM44")
            sbSQL.AppendLine("    ,FREE_ITEM45")
            sbSQL.AppendLine("    ,FREE_ITEM46")
            sbSQL.AppendLine("    ,FREE_ITEM47")
            sbSQL.AppendLine("    ,FREE_ITEM48")
            sbSQL.AppendLine("    ,FREE_ITEM49")
            sbSQL.AppendLine("    ,FREE_ITEM50")
            sbSQL.AppendLine("    ,FREE_ITEM51")
            sbSQL.AppendLine("    ,FREE_ITEM52")
            sbSQL.AppendLine("    ,FREE_ITEM53")
            sbSQL.AppendLine("    ,FREE_ITEM54")
            sbSQL.AppendLine("    ,FREE_ITEM55")
            sbSQL.AppendLine("    ,FREE_ITEM56")
            sbSQL.AppendLine("    ,FREE_ITEM57")
            sbSQL.AppendLine("    ,FREE_ITEM58")
            sbSQL.AppendLine("    ,FREE_ITEM59")
            sbSQL.AppendLine("    ,FREE_ITEM60")
            sbSQL.AppendLine("    ,ASSET_PIC_FILENAME")
            sbSQL.AppendLine("    ,LEASE_CNT")
            sbSQL.AppendLine("    ,ASSET_STATUS")
            sbSQL.AppendLine("    ,EPC_ENC_STATUS")
            sbSQL.AppendLine("    ,TAG_LINK_STATUS")
            sbSQL.AppendLine("    ,LEASE_STATUS")
            sbSQL.AppendLine("    ,LEASE_PLACE_CD")
            sbSQL.AppendLine("    ,LEASE_PLACE_NAME")
            sbSQL.AppendLine("    ,REMARK")
            sbSQL.AppendLine("    ,LEASE_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,LEASE_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,LEASE_DT")
            sbSQL.AppendLine("    ,LEASE_DUE_DT")
            sbSQL.AppendLine("    ,MAINTE_LEASE_NUM ")
            sbSQL.AppendLine("    ,MAINTE_LEASE_DAY ")
            sbSQL.AppendLine("    ,MAINTE_LEASE_DAY_WORK_FLAG ")
            sbSQL.AppendLine("    ,MAINTE_DAY ")
            sbSQL.AppendLine("    ,MAINTE_DAY_WORK_FLAG ")
            sbSQL.AppendLine("    ,RETIRE_TS")
            sbSQL.AppendLine("    ,RETIRE_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,RETIRE_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,RETIRE_ASSET_ID")
            sbSQL.AppendLine("    ,PHYINV_TS")
            sbSQL.AppendLine("    ,PHYINV_STATUS")
            sbSQL.AppendLine("    ,PHYINV_LOC_L_CD")
            sbSQL.AppendLine("    ,PHYINV_LOC_M_CD")
            sbSQL.AppendLine("    ,PHYINV_LOC_S_CD")
            sbSQL.AppendLine("    ,PHYINV_LOC_L_NAME")
            sbSQL.AppendLine("    ,PHYINV_LOC_M_NAME")
            sbSQL.AppendLine("    ,PHYINV_LOC_S_NAME")
            sbSQL.AppendLine("    ,PHYINV_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,PHYINV_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,TASK_TS")
            sbSQL.AppendLine("    ,IS_TANGIBLE")
            sbSQL.AppendLine("    ,INS_DATETIME")
            sbSQL.AppendLine("    ,INS_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,UPD_DATETIME")
            sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,UPD_TASK")
            sbSQL.AppendLine("  )")
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("     INA.ASSET_NO")
            sbSQL.AppendLine("    ,@ASSET_ID")
            sbSQL.AppendLine("    ,INA.SUB_ASSET_NO")
            sbSQL.AppendLine("    ,@EPC_SIZE")
            sbSQL.AppendLine("    ,@ASSET_ID")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,INA.ASSET_NAME")
            sbSQL.AppendLine("    ,INA.MNG_LOC_L_CD")
            sbSQL.AppendLine("    ,INA.LOC_L_CD")
            sbSQL.AppendLine("    ,INA.LOC_M_CD")
            sbSQL.AppendLine("    ,INA.LOC_S_CD")
            sbSQL.AppendLine("    ,INA.CAT_L_CD")
            sbSQL.AppendLine("    ,' '")
            sbSQL.AppendLine("    ,' '")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,INA.FREE_ITEM1")
            sbSQL.AppendLine("    ,INA.FREE_ITEM2")
            sbSQL.AppendLine("    ,INA.FREE_ITEM3")
            sbSQL.AppendLine("    ,INA.FREE_ITEM4")
            sbSQL.AppendLine("    ,INA.FREE_ITEM5")
            sbSQL.AppendLine("    ,INA.FREE_ITEM6")
            sbSQL.AppendLine("    ,INA.FREE_ITEM7")
            sbSQL.AppendLine("    ,INA.FREE_ITEM8")
            sbSQL.AppendLine("    ,INA.FREE_ITEM9")
            sbSQL.AppendLine("    ,INA.FREE_ITEM10")
            sbSQL.AppendLine("    ,INA.FREE_ITEM11")
            sbSQL.AppendLine("    ,INA.FREE_ITEM12")
            sbSQL.AppendLine("    ,INA.FREE_ITEM13")
            sbSQL.AppendLine("    ,INA.FREE_ITEM14")
            sbSQL.AppendLine("    ,INA.FREE_ITEM15")
            sbSQL.AppendLine("    ,INA.FREE_ITEM16")
            sbSQL.AppendLine("    ,INA.FREE_ITEM17")
            sbSQL.AppendLine("    ,INA.FREE_ITEM18")
            sbSQL.AppendLine("    ,INA.FREE_ITEM19")
            sbSQL.AppendLine("    ,INA.FREE_ITEM20")
            sbSQL.AppendLine("    ,INA.FREE_ITEM21")
            sbSQL.AppendLine("    ,INA.FREE_ITEM22")
            sbSQL.AppendLine("    ,INA.FREE_ITEM23")
            sbSQL.AppendLine("    ,INA.FREE_ITEM24")
            sbSQL.AppendLine("    ,INA.FREE_ITEM25")
            sbSQL.AppendLine("    ,INA.FREE_ITEM26")
            sbSQL.AppendLine("    ,INA.FREE_ITEM27")
            sbSQL.AppendLine("    ,INA.FREE_ITEM28")
            sbSQL.AppendLine("    ,INA.FREE_ITEM29")
            sbSQL.AppendLine("    ,INA.FREE_ITEM30")
            sbSQL.AppendLine("    ,INA.FREE_ITEM31")
            sbSQL.AppendLine("    ,INA.FREE_ITEM32")
            sbSQL.AppendLine("    ,INA.FREE_ITEM33")
            sbSQL.AppendLine("    ,INA.FREE_ITEM34")
            sbSQL.AppendLine("    ,INA.FREE_ITEM35")
            sbSQL.AppendLine("    ,INA.FREE_ITEM36")
            sbSQL.AppendLine("    ,INA.FREE_ITEM37")
            sbSQL.AppendLine("    ,INA.FREE_ITEM38")
            sbSQL.AppendLine("    ,INA.FREE_ITEM39")
            sbSQL.AppendLine("    ,INA.FREE_ITEM40")
            sbSQL.AppendLine("    ,INA.FREE_ITEM41")
            sbSQL.AppendLine("    ,INA.FREE_ITEM42")
            sbSQL.AppendLine("    ,INA.FREE_ITEM43")
            sbSQL.AppendLine("    ,INA.FREE_ITEM44")
            sbSQL.AppendLine("    ,INA.FREE_ITEM45")
            sbSQL.AppendLine("    ,INA.FREE_ITEM46")
            sbSQL.AppendLine("    ,INA.FREE_ITEM47")
            sbSQL.AppendLine("    ,INA.FREE_ITEM48")
            sbSQL.AppendLine("    ,INA.FREE_ITEM49")
            sbSQL.AppendLine("    ,INA.FREE_ITEM50")
            sbSQL.AppendLine("    ,INA.FREE_ITEM51")
            sbSQL.AppendLine("    ,INA.FREE_ITEM52")
            sbSQL.AppendLine("    ,INA.FREE_ITEM53")
            sbSQL.AppendLine("    ,INA.FREE_ITEM54")
            sbSQL.AppendLine("    ,INA.FREE_ITEM55")
            sbSQL.AppendLine("    ,INA.FREE_ITEM56")
            sbSQL.AppendLine("    ,INA.FREE_ITEM57")
            sbSQL.AppendLine("    ,INA.FREE_ITEM58")
            sbSQL.AppendLine("    ,INA.FREE_ITEM59")
            sbSQL.AppendLine("    ,INA.FREE_ITEM60")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,INA.ASSET_STATUS")
            sbSQL.AppendLine("    ,1")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,0")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,NULL")
            sbSQL.AppendLine("    ,@TASK_TS")
            sbSQL.AppendLine("    ,INA.IS_TANGIBLE")
            sbSQL.AppendLine("    ,@INS_DATETIME")
            sbSQL.AppendLine("    ,@INS_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,@INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,@UPD_DATETIME")
            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,@UPD_TASK")
            sbSQL.AppendLine("FROM T_IN_ASSET INA")

            sbSQL.AppendLine("LEFT JOIN M_ASSET MA ON MA.ASSET_NO = INA.ASSET_NO")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("    MA.ASSET_NO IS NULL ")
            sbSQL.AppendLine($"AND INA.ASSET_NO = '{sAssetNo}'")

            Dim params = New List(Of SqlParameter)
            Dim sAssetId As String = GetAssetId(cat)
            params.Add(New SqlParameter($"@ASSET_ID", sAssetId))
            params.Add(New SqlParameter($"@EPC_SIZE", cat.EPC_SIZE))
            params.Add(New SqlParameter($"@TASK_TS", Long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))))
            params.Add(New SqlParameter($"@INS_DATETIME", CommonRfid.getNowDate))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            params.Add(New SqlParameter($"@UPD_TASK", sUpdTask))

            db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
        End Sub

        ''' <summary>
        ''' 資産マスタにデータを更新する（入庫予定テーブルから値取得）
        ''' </summary>
        ''' <param name="db">DBシステムコンテキスト</param>
        ''' <param name="lstAssetNo">資産番号一覧（更新キー）</param>
        ''' <param name="sUpdTask">更新処理</param>
        Private Sub UpdateMAssetFromTInAsset(ByVal db As SystemDBContext, ByVal lstAssetNo As List(Of String), ByVal sUpdTask As String)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim sAssetNos As String = String.Join(",", lstAssetNo.Select(Function(x) $"'{x}'"))

            sbSQL.AppendLine("UPDATE MA SET")
            sbSQL.AppendLine("     MA.ASSET_NO = INA.ASSET_NO")
            sbSQL.AppendLine("    ,MA.SUB_ASSET_NO = INA.SUB_ASSET_NO")
            sbSQL.AppendLine("    ,MA.ASSET_NAME = INA.ASSET_NAME")
            sbSQL.AppendLine("    ,MA.MNG_LOC_L_CD = INA.MNG_LOC_L_CD")
            sbSQL.AppendLine("    ,MA.LOC_L_CD = INA.LOC_L_CD")
            sbSQL.AppendLine("    ,MA.LOC_M_CD = INA.LOC_M_CD")
            sbSQL.AppendLine("    ,MA.LOC_S_CD = INA.LOC_S_CD")
            sbSQL.AppendLine("    ,MA.CAT_L_CD = INA.CAT_L_CD")
            sbSQL.AppendLine("    ,MA.CAT_M_CD = ' '")
            sbSQL.AppendLine("    ,MA.CAT_S_CD = ' '")
            sbSQL.AppendLine("    ,MA.FREE_ITEM1 = INA.FREE_ITEM1")
            sbSQL.AppendLine("    ,MA.FREE_ITEM2 = INA.FREE_ITEM2")
            sbSQL.AppendLine("    ,MA.FREE_ITEM3 = INA.FREE_ITEM3")
            sbSQL.AppendLine("    ,MA.FREE_ITEM4 = INA.FREE_ITEM4")
            sbSQL.AppendLine("    ,MA.FREE_ITEM5 = INA.FREE_ITEM5")
            sbSQL.AppendLine("    ,MA.FREE_ITEM6 = INA.FREE_ITEM6")
            sbSQL.AppendLine("    ,MA.FREE_ITEM7 = INA.FREE_ITEM7")
            sbSQL.AppendLine("    ,MA.FREE_ITEM8 = INA.FREE_ITEM8")
            sbSQL.AppendLine("    ,MA.FREE_ITEM9 = INA.FREE_ITEM9")
            sbSQL.AppendLine("    ,MA.FREE_ITEM10 = INA.FREE_ITEM10")
            sbSQL.AppendLine("    ,MA.FREE_ITEM11 = INA.FREE_ITEM11")
            sbSQL.AppendLine("    ,MA.FREE_ITEM12 = INA.FREE_ITEM12")
            sbSQL.AppendLine("    ,MA.FREE_ITEM13 = INA.FREE_ITEM13")
            sbSQL.AppendLine("    ,MA.FREE_ITEM14 = INA.FREE_ITEM14")
            sbSQL.AppendLine("    ,MA.FREE_ITEM15 = INA.FREE_ITEM15")
            sbSQL.AppendLine("    ,MA.FREE_ITEM16 = INA.FREE_ITEM16")
            sbSQL.AppendLine("    ,MA.FREE_ITEM17 = INA.FREE_ITEM17")
            sbSQL.AppendLine("    ,MA.FREE_ITEM18 = INA.FREE_ITEM18")
            sbSQL.AppendLine("    ,MA.FREE_ITEM19 = INA.FREE_ITEM19")
            sbSQL.AppendLine("    ,MA.FREE_ITEM20 = INA.FREE_ITEM20")
            sbSQL.AppendLine("    ,MA.FREE_ITEM21 = INA.FREE_ITEM21")
            sbSQL.AppendLine("    ,MA.FREE_ITEM22 = INA.FREE_ITEM22")
            sbSQL.AppendLine("    ,MA.FREE_ITEM23 = INA.FREE_ITEM23")
            sbSQL.AppendLine("    ,MA.FREE_ITEM24 = INA.FREE_ITEM24")
            sbSQL.AppendLine("    ,MA.FREE_ITEM25 = INA.FREE_ITEM25")
            sbSQL.AppendLine("    ,MA.FREE_ITEM26 = INA.FREE_ITEM26")
            sbSQL.AppendLine("    ,MA.FREE_ITEM27 = INA.FREE_ITEM27")
            sbSQL.AppendLine("    ,MA.FREE_ITEM28 = INA.FREE_ITEM28")
            sbSQL.AppendLine("    ,MA.FREE_ITEM29 = INA.FREE_ITEM29")
            sbSQL.AppendLine("    ,MA.FREE_ITEM30 = INA.FREE_ITEM30")
            sbSQL.AppendLine("    ,MA.FREE_ITEM31 = INA.FREE_ITEM31")
            sbSQL.AppendLine("    ,MA.FREE_ITEM32 = INA.FREE_ITEM32")
            sbSQL.AppendLine("    ,MA.FREE_ITEM33 = INA.FREE_ITEM33")
            sbSQL.AppendLine("    ,MA.FREE_ITEM34 = INA.FREE_ITEM34")
            sbSQL.AppendLine("    ,MA.FREE_ITEM35 = INA.FREE_ITEM35")
            sbSQL.AppendLine("    ,MA.FREE_ITEM36 = INA.FREE_ITEM36")
            sbSQL.AppendLine("    ,MA.FREE_ITEM37 = INA.FREE_ITEM37")
            sbSQL.AppendLine("    ,MA.FREE_ITEM38 = INA.FREE_ITEM38")
            sbSQL.AppendLine("    ,MA.FREE_ITEM39 = INA.FREE_ITEM39")
            sbSQL.AppendLine("    ,MA.FREE_ITEM40 = INA.FREE_ITEM40")
            sbSQL.AppendLine("    ,MA.FREE_ITEM41 = INA.FREE_ITEM41")
            sbSQL.AppendLine("    ,MA.FREE_ITEM42 = INA.FREE_ITEM42")
            sbSQL.AppendLine("    ,MA.FREE_ITEM43 = INA.FREE_ITEM43")
            sbSQL.AppendLine("    ,MA.FREE_ITEM44 = INA.FREE_ITEM44")
            sbSQL.AppendLine("    ,MA.FREE_ITEM45 = INA.FREE_ITEM45")
            sbSQL.AppendLine("    ,MA.FREE_ITEM46 = INA.FREE_ITEM46")
            sbSQL.AppendLine("    ,MA.FREE_ITEM47 = INA.FREE_ITEM47")
            sbSQL.AppendLine("    ,MA.FREE_ITEM48 = INA.FREE_ITEM48")
            sbSQL.AppendLine("    ,MA.FREE_ITEM49 = INA.FREE_ITEM49")
            sbSQL.AppendLine("    ,MA.FREE_ITEM50 = INA.FREE_ITEM50")
            sbSQL.AppendLine("    ,MA.FREE_ITEM51 = INA.FREE_ITEM51")
            sbSQL.AppendLine("    ,MA.FREE_ITEM52 = INA.FREE_ITEM52")
            sbSQL.AppendLine("    ,MA.FREE_ITEM53 = INA.FREE_ITEM53")
            sbSQL.AppendLine("    ,MA.FREE_ITEM54 = INA.FREE_ITEM54")
            sbSQL.AppendLine("    ,MA.FREE_ITEM55 = INA.FREE_ITEM55")
            sbSQL.AppendLine("    ,MA.FREE_ITEM56 = INA.FREE_ITEM56")
            sbSQL.AppendLine("    ,MA.FREE_ITEM57 = INA.FREE_ITEM57")
            sbSQL.AppendLine("    ,MA.FREE_ITEM58 = INA.FREE_ITEM58")
            sbSQL.AppendLine("    ,MA.FREE_ITEM59 = INA.FREE_ITEM59")
            sbSQL.AppendLine("    ,MA.FREE_ITEM60 = INA.FREE_ITEM60")
            sbSQL.AppendLine("    ,MA.IS_TANGIBLE = INA.IS_TANGIBLE")
            sbSQL.AppendLine("    ,MA.UPD_DATETIME = @UPD_DATETIME")
            sbSQL.AppendLine("    ,MA.UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("    ,MA.UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("    ,MA.UPD_TASK = @UPD_TASK")

            sbSQL.AppendLine(" FROM M_ASSET MA ")
            sbSQL.AppendLine(" INNER JOIN T_IN_ASSET INA ON MA.ASSET_NO = INA.ASSET_NO ")
            sbSQL.AppendLine(" WHERE ")
            sbSQL.AppendLine($"     MA.ASSET_NO IN ({sAssetNos}) ")

            Dim params = New List(Of SqlParameter)
            params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            params.Add(New SqlParameter($"@UPD_TASK", sUpdTask))
            params.Add(New SqlParameter($"@ASSET_NOS", sUpdTask))

            db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
        End Sub

        ''' <summary>
        ''' 入庫予定テーブルからデータを削除する
        ''' </summary>
        ''' <param name="db">DBシステムコンテキスト</param>
        ''' <param name="lstAssetNo">資産番号一覧（更新キー）</param>
        Private Sub DeleteTInAsset(ByVal db As SystemDBContext, ByVal lstAssetNo As List(Of String))
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim sAssetNos As String = String.Join(",", lstAssetNo.Select(Function(x) $"'{x}'"))

            sbSQL.AppendLine("DELETE FROM T_IN_ASSET")
            sbSQL.AppendLine("WHERE")
            sbSQL.AppendLine(" ASSET_NO IN (" & sAssetNos & ")")

            db.Database.ExecuteSqlCommand(sbSQL.ToString)
        End Sub

        ''' <summary>
        ''' 処理履歴テーブルにデータを追加する
        ''' </summary>
        ''' <param name="db">DBシステムコンテキスト</param>
        ''' <param name="lstAssetNo">資産番号一覧（更新キー）</param>
        ''' <param name="sProcessDiv">処理区分（更新値）</param>
        ''' <param name="sUpdTask">更新処理</param>
        Private Sub InsertTProcessLogFromMAsset(ByVal db As SystemDBContext, ByVal lstAssetNo As List(Of String), ByVal sProcessDiv As String, ByVal sUpdTask As String)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim sAssetNos As String = String.Join(",", lstAssetNo.Select(Function(x) $"'{x}'"))

            sbSQL.AppendLine("INSERT INTO T_PROCESS_LOG")
            sbSQL.AppendLine("(")
            sbSQL.AppendLine("   PROCESS_DT")
            sbSQL.AppendLine("  ,PROCESS_TM")
            sbSQL.AppendLine("  ,ASSET_NO")
            sbSQL.AppendLine("  ,ASSET_ID")
            sbSQL.AppendLine("  ,PROCESS_TOOL_DIV")
            sbSQL.AppendLine("  ,PROCESS_DIV")
            sbSQL.AppendLine("  ,LEASE_PLACE_CD")
            sbSQL.AppendLine("  ,LEASE_PLACE_NAME")
            sbSQL.AppendLine("  ,REMARK")
            sbSQL.AppendLine("  ,LEASE_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,LEASE_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,LEASE_DT")
            sbSQL.AppendLine("  ,LEASE_DUE_DT")
            sbSQL.AppendLine("  ,LOC_L_CD")
            sbSQL.AppendLine("  ,LOC_M_CD")
            sbSQL.AppendLine("  ,LOC_S_CD")
            sbSQL.AppendLine("  ,LOC_L_NAME")
            sbSQL.AppendLine("  ,LOC_M_NAME")
            sbSQL.AppendLine("  ,LOC_S_NAME")
            sbSQL.AppendLine("  ,ASSET_NAME")
            sbSQL.AppendLine("  ,MAKER")
            sbSQL.AppendLine("  ,PRODUCT_NO")
            sbSQL.AppendLine("  ,MODEL_NO")
            sbSQL.AppendLine("  ,REMARK1")
            sbSQL.AppendLine("  ,REMARK2")
            sbSQL.AppendLine("  ,REMARK3")
            sbSQL.AppendLine("  ,REMARK4")
            sbSQL.AppendLine("  ,REMARK5")
            sbSQL.AppendLine("  ,REMARK6")
            sbSQL.AppendLine("  ,REMARK7")
            sbSQL.AppendLine("  ,REMARK8")
            sbSQL.AppendLine("  ,REMARK9")
            sbSQL.AppendLine("  ,REMARK10")
            sbSQL.AppendLine("  ,MNG_LOC_L_CD ")
            sbSQL.AppendLine("  ,LEASE_STATUS ")
            sbSQL.AppendLine("  ,CAT_L_CD")
            sbSQL.AppendLine("  ,CAT_NAME")
            sbSQL.AppendLine("  ,SUB_ASSET_NO")
            sbSQL.AppendLine("  ,TASK_RESULT")
            sbSQL.AppendLine("  ,UPD_DATETIME")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_TASK")
            sbSQL.AppendLine(")")
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("   CONVERT(int, FORMAT(GetDate(), 'yyyyMMdd'))")
            sbSQL.AppendLine("  ,FORMAT(GetDate(), 'HHmmssfff')")
            sbSQL.AppendLine("  ,ASSET.ASSET_NO")
            sbSQL.AppendLine("  ,ASSET.ASSET_ID")
            sbSQL.AppendLine("  ,'" & Misc.ProcessToolDiv.Web & "'")
            sbSQL.AppendLine("  ,'" & sProcessDiv & "'")
            sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_CD")
            sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_NAME")
            sbSQL.AppendLine("  ,ASSET.REMARK")
            sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,ASSET.LEASE_DT")
            sbSQL.AppendLine("  ,ASSET.LEASE_DUE_DT")
            sbSQL.AppendLine("  ,ASSET.LOC_L_CD")
            sbSQL.AppendLine("  ,ASSET.LOC_M_CD")
            sbSQL.AppendLine("  ,ASSET.LOC_S_CD")
            sbSQL.AppendLine("  ,LOCL.LOC_NAME AS LocLName ")
            sbSQL.AppendLine("  ,LOCM.LOC_NAME AS LocMName ")
            sbSQL.AppendLine("  ,LOCS.LOC_NAME AS LocSName ")
            sbSQL.AppendLine("  ,ASSET.ASSET_NAME")
            sbSQL.AppendLine("  ,ASSET.MAKER")
            sbSQL.AppendLine("  ,ASSET.PRODUCT_NO")
            sbSQL.AppendLine("  ,ASSET.MODEL_NO")
            sbSQL.AppendLine("  ,ASSET.REMARK1")
            sbSQL.AppendLine("  ,ASSET.REMARK2")
            sbSQL.AppendLine("  ,ASSET.REMARK3")
            sbSQL.AppendLine("  ,ASSET.REMARK4")
            sbSQL.AppendLine("  ,ASSET.REMARK5")
            sbSQL.AppendLine("  ,ASSET.REMARK6")
            sbSQL.AppendLine("  ,ASSET.REMARK7")
            sbSQL.AppendLine("  ,ASSET.REMARK8")
            sbSQL.AppendLine("  ,ASSET.REMARK9")
            sbSQL.AppendLine("  ,ASSET.REMARK10")
            sbSQL.AppendLine("  ,ASSET.MNG_LOC_L_CD ")
            sbSQL.AppendLine("  ,ASSET.LEASE_STATUS ")
            sbSQL.AppendLine("  ,ASSET.CAT_L_CD ")
            sbSQL.AppendLine("  ,CATL.CAT_NAME ")
            sbSQL.AppendLine("  ,ASSET.SUB_ASSET_NO ")
            sbSQL.AppendLine("  ,'" & Misc.TaskResult.OK & "'")
            sbSQL.AppendLine("  ,ASSET.UPD_DATETIME")
            sbSQL.AppendLine("  ,ASSET.UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,ASSET.UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,ASSET.UPD_TASK")
            sbSQL.AppendLine("FROM M_ASSET ASSET")

            sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
            sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
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

            sbSQL.AppendLine("WHERE")
            sbSQL.AppendLine(" ASSET.ASSET_NO IN (" & sAssetNos & ")")

            Dim params = New List(Of SqlParameter)
            params.Add(New SqlParameter($"@UPD_DATETIME", CommonRfid.getNowDate))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            params.Add(New SqlParameter($"@UPD_TASK", sUpdTask))

            db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
        End Sub
#End Region

#Region "資産ID生成"
        ''' <summary>
        ''' 資産ID生成
        ''' </summary>
        ''' <returns></returns>
        Private Function GetAssetId(cat As CategoryModel) As String

            Dim IsError As Boolean = False
            Dim sAssetId As String = ""
            Dim sAssetIdRegDiv = cat.ASSET_ID_REG_DIV
            Dim sEpcSize = cat.EPC_SIZE
            Dim sEpcCompanyKey = cat.EPC_COMPANY_KEY
            Dim sEpcAssetType = cat.EPC_ASSET_TYPE

            Select Case sAssetIdRegDiv
                Case AssetIDGenerationMethod.None, AssetIDGenerationMethod.Auto
                    '生成資産IDには資産IDと同じ値を入れる
                    sAssetId = Dao.ToolAsset.MakeAssetId(sAssetIdRegDiv, String.Empty, sEpcSize, _LoginInfo)
                Case AssetIDGenerationMethod.AssetNo
                    sAssetId = Dao.ToolAsset.MakeAssetId(sAssetIdRegDiv, _ViewModel.ASSET_NO, sEpcSize, _LoginInfo)
                Case AssetIDGenerationMethod.GIAI
                    Dim Settinginfo As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.AssetIDGenerationCoPrefixGIAI.KeyDiv, CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key1, sEpcCompanyKey, CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key3)
                    '設定マスタからデータを取得できなかった場合、エラー
                    If Settinginfo Is Nothing Then
                        Throw New Exception(String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), (CommonSettingKey.AssetIDGenerationCoPrefixGIAI.KeyDiv & " " & CommonSettingKey.AssetIDGenerationCoPrefixGIAI.Key1 & " " & sEpcCompanyKey)))
                    Else

                        'シリアルNo取得
                        Dim iSerialNo As String = Dao.ToolAsset.GetSerialNo(AssetIDGenerationMethod.GIAI, Settinginfo.DATA1, "**", _LoginInfo)

                        '資産ID取得
                        Dim libEpc As AsetraLibEpc.LibEpc = New AsetraLibEpc.LibEpc
                        sAssetId = libEpc.MakeCodeGIAI96(Settinginfo.DATA1, CLng(iSerialNo))
                        Select Case libEpc.Stat
                            Case AsetraLibEpc.LibEpc.EpcStatus.ErrCompanyPrefix
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_COM_CD)
                                IsError = True
                            Case AsetraLibEpc.LibEpc.EpcStatus.ErrAssetType
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_ASSET_TYPE)
                                IsError = True
                            Case AsetraLibEpc.LibEpc.EpcStatus.ErrSerialNo
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_SERIAL_NO)
                                IsError = True
                            Case AsetraLibEpc.LibEpc.EpcStatus.ErrOther
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_SYSTEM)
                                IsError = True
                        End Select
                    End If
                Case AssetIDGenerationMethod.GRAI
                    Dim Settinginfo As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.AssetIDGenerationCoPrefixGRAI.KeyDiv, CommonSettingKey.AssetIDGenerationCoPrefixGRAI.Key1, sEpcCompanyKey, CommonSettingKey.AssetIDGenerationCoPrefixGRAI.Key3)
                    '設定マスタからデータを取得できなかった場合、エラー
                    If Settinginfo Is Nothing Then
                        Throw New Exception(String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), (CommonSettingKey.AssetIDGenerationCoPrefixGRAI.KeyDiv & " " & CommonSettingKey.AssetIDGenerationCoPrefixGRAI.Key1 & " " & sEpcCompanyKey)))
                    Else
                        'シリアルNo取得＆更新
                        Dim iSerialNo As String = Dao.ToolAsset.GetSerialNo(AssetIDGenerationMethod.GRAI, Settinginfo.DATA1, sEpcAssetType, _LoginInfo)

                        '資産ID取得
                        Dim libEpc As AsetraLibEpc.LibEpc = New AsetraLibEpc.LibEpc
                        sAssetId = libEpc.MakeCodeGRAI96(Settinginfo.DATA1, sEpcAssetType, CLng(iSerialNo))
                        Select Case libEpc.Stat
                            Case AsetraLibEpc.LibEpc.EpcStatus.ErrCompanyPrefix
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_COM_CD)
                                IsError = True
                            Case AsetraLibEpc.LibEpc.EpcStatus.ErrAssetType
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_ASSET_TYPE)
                                IsError = True
                            Case AsetraLibEpc.LibEpc.EpcStatus.ErrSerialNo
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_SERIAL_NO)
                                IsError = True
                            Case AsetraLibEpc.LibEpc.EpcStatus.ErrOther
                                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_SYSTEM)
                                IsError = True
                        End Select
                    End If
            End Select

            '資産IDの長さチェック
            If Not IsError AndAlso Not sAssetId = String.Empty AndAlso (sEpcSize / 4) <> sAssetId.Length Then
                ModelState.AddModelError("ASSET_ID", CommonLabel.MSG_ERR_ASSETID_LENGTH)
            End If

            Return sAssetId
        End Function
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