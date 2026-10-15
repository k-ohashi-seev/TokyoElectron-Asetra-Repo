Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient

Namespace Controllers
    ''' <summary>
    ''' 操作ログ照会画面表示用のクラス
    ''' </summary>
    Public Class OperationLogController
        Inherits PageBaseController

#Region "クラス変数"
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _ViewModel As OperationLogViewModel = New OperationLogViewModel                         'OperationLog一覧表示用モデル
        Protected _viewResult As List(Of OperationLogViewModel) = New List(Of OperationLogViewModel)    '結果View
        Private _LocationLList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationMList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationSList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _Message As MessageRfid = Nothing
        Private _isSort As Boolean = True                       'ソートフラグ
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As OperationLogViewModel, ByVal actionname As String) As ActionResult
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
        Public Function Index(ByVal ViewModel As OperationLogViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_OPERATION_LOG)
            End If

            '上限確認フラグ 初期化
            ViewBag.isLimitFlg = False
            '一覧件数　初期化
            ViewBag.cntList = 0
            '設定マスタ上限件数　初期化
            ViewBag.cntLimit = 0
            '一覧表示フラグ
            Dim isDisp As Boolean = True

            '20230901 ADD Start Shirakawa
            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_Operation_Log.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_Operation_Log.Key1,
                                                                      CommonSettingKey.Web_Read_Data_Operation_Log.Key2,
                                                                      CommonSettingKey.Web_Read_Data_Operation_Log.Key3)
            '20230901 ADD End Shirakawa

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As OperationLogViewModel = Session(CommonSessionKey.CREAGE_OPERATION_LOG)
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
                ' 期間の初期値(本日)
                If String.IsNullOrEmpty(ViewModel.SearchDateFrom) Then
                    ViewModel.SearchDateFrom = System.DateTime.Now.ToString("yyyy-MM-dd")
                End If
                If String.IsNullOrEmpty(ViewModel.SearchDateTo) Then
                    ViewModel.SearchDateTo = System.DateTime.Now.ToString("yyyy-MM-dd")
                End If
                ' 処理ツールの初期値
                If String.IsNullOrEmpty(ViewModel.SearchProcessTool) Then
                    ViewModel.SearchProcessTool = "ALL"
                End If

                '20230904 UPD Start Shirakawa
                '件数確認用データのためソートしない
                _isSort = False
                'セッションSEARCH_FLGがNothingの場合
                If Session(CommonSessionKey.SEARCH_FLG) Is Nothing Then
                    '初期表示
                    '設定マスタから取得できた場合 かつ 初期表示フラグ(Data1)がある場合 かつ 設定マスタ(web読取データ)の初期表示Nの場合
                    If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA1) = False AndAlso WebReadData.DATA1 = RedeDataFlg.No Then
                        '表示しない
                        isDisp = False
                    Else
                        '表示する
                        MyBase.BaseIndex(_ViewModel, False)
                    End If
                Else
                    '検索ボタンから表示
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

            ' (ロケーション)大区分ドロップダウンリスト作成
            GetLocationLData()
            ' (ロケーション)中区分
            GetLocationMData(_ViewModel.SearchLocL)
            ' (ロケーション)小区分
            GetLocationSData(_ViewModel.SearchLocL, _ViewModel.SearchLocM)

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.OPERATION_LOG, COLSET_LOC.ALL)
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
        ''' ロケーション変更
        ''' </summary>
        ''' <param name="locationLevel"></param>
        ''' <param name="selectLCode"></param>
        ''' <param name="selectMCode"></param>
        ''' <returns></returns>
        Public Function ChangeLocation(ByVal locationLevel As String, ByVal selectLCode As String, ByVal selectMCode As String) As ActionResult
            '--2020/03/17 nv miyaji update start
            Dim settingRfid As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)               'メッセージ兼設定リスト
            Dim modelLocationUsage As New LocationUsageModel        'ロケーション使用区分
            Dim setting As SettingInfo = settingRfid.getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingRfid.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            modelLocationUsage.LocUsage_L = setting.DATA1
            modelLocationUsage.LocUsage_M = setting.DATA2
            modelLocationUsage.LocUsage_S = setting.DATA3

            If locationLevel = LOC_DIV.LocationM Then
                If modelLocationUsage.LocUsage_M = Misc.ItemUse.Yes Then
                    Return Json(GetLocationMData(selectLCode))
                Else
                    '中区分 使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            Else
                If modelLocationUsage.LocUsage_S = Misc.ItemUse.Yes Then
                    Return Json(GetLocationSData(selectLCode, selectMCode))
                Else
                    '小区分　使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            End If
            'If locationLevel = "M" Then
            '    Return Json(GetLocationMData(selectLCode))
            'Else
            '    Return Json(GetLocationSData(selectLCode, selectMCode))
            'End If
            '--2020/03/17 nv miyaji update end

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()
            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_OPERATION_LOG)
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
            Dim query As DbSqlQuery(Of OperationLogViewModel) = _db.OperationLogView.SqlQuery(_SQL, _paramSqlList.ToArray)
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

#Region "(ロケーション)大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)大区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationLData() As JsonResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationLData()
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.modelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME")
            _ViewModel.locLDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "(ロケーション)中区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)中区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationMData(ByVal selectLCode As String) As JsonResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationMData(selectLCode)
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.modelLocM = New SelectList(lstLocation, "LOC_CD", "LOC_NAME")
            _ViewModel.locMDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "(ロケーション)小区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCode As String) As JsonResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationSData(selectLCode, selectMCode)
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.modelLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME")
            _ViewModel.locSDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

        '20230904 ADD Start Shirakawa
#Region "検索ボタン押下"
        ''' <summary>
        ''' 検索ボタン押下
        ''' </summary>
        ''' <returns></returns>
        Public Function SearchClick() As ActionResult
            Session(CommonSessionKey.SEARCH_FLG) = True

            '結果表示
            Return RedirectToAction("Index")
        End Function
#End Region
        '20230904 ADD End Shirakawa

#Region "項目配置情報を取得"
        Private Function GetGridInfo() As Dictionary(Of String, SlickGridInfo)
            '項目配置詳細マスタを取得
            Dim sbSQL As New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("  DISP_ID")
            sbSQL.AppendLine(", LOC_L_CD")
            sbSQL.AppendLine(", COLUMN_NAME")
            sbSQL.AppendLine(", DISP_ORDER")
            sbSQL.AppendLine(", TOOL_DIV")
            sbSQL.AppendLine(", WORD_DIV")
            sbSQL.AppendLine(", WORD_CD")
            sbSQL.AppendLine(", DISP_FLAG")
            sbSQL.AppendLine(", INS_DATETIME")
            sbSQL.AppendLine(", INS_EMPLOYEE_CD")
            sbSQL.AppendLine(", INS_EMPLOYEE_NAME")
            sbSQL.AppendLine(", UPD_DATETIME")
            sbSQL.AppendLine(", UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(", UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(", UPD_TASK")
            sbSQL.AppendLine("FROM M_COL_SET_DETAIL")
            sbSQL.AppendLine("WHERE DISP_ID = @DISP_ID")
            'sbSQL.AppendLine("AND LOC_LCD = @LOC_L_CD")


            params.Add(New SqlParameter($"@DISP_ID", DispId.OPERATION_LOG))

            Dim colSetDetail As List(Of ColSetDetailModel) = _db.ColSetDetail.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            'Viewに渡す用に変換
            Dim dictGridInfo As New Dictionary(Of String, SlickGridInfo)
            For Each row In colSetDetail
                If Not dictGridInfo.ContainsKey(row.COLUMN_NAME) Then
                    'Key:DB項目名 Value:Grid表示情報
                    dictGridInfo.Add(row.COLUMN_NAME, New SlickGridInfo With {.DispOrder = row.DISP_ORDER, .DispFlag = row.DISP_FLAG})
                End If
            Next

            Return dictGridInfo
        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL(Optional ByVal isSort As Boolean = True) As String
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim m = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '一覧表示表用SQL
            sbSQL.Clear()
            If isSort Then
                sbSQL.AppendLine("SELECT ")
            Else
                sbSQL.AppendLine("SELECT TOP 1 ")
            End If
            sbSQL.AppendLine("OPE_LOG_ID")
            sbSQL.AppendLine(",PROCESS_DT")
            sbSQL.AppendLine(",PROCESS_TM")
            sbSQL.AppendLine(",CAST(PROCESS_DT AS VARCHAR(8)) + PROCESS_TM PROCESS_DATETIME")
            sbSQL.AppendLine(",PROCESS_TOOL_DIV")
            sbSQL.AppendLine(",PROCESS_CAT")
            sbSQL.AppendLine(",LOC_L_CD")
            sbSQL.AppendLine(",LOC_M_CD")
            sbSQL.AppendLine(",LOC_S_CD")
            sbSQL.AppendLine(",LOC_L_NAME")
            sbSQL.AppendLine(",LOC_M_NAME")
            sbSQL.AppendLine(",LOC_S_NAME")
            sbSQL.AppendLine(",PROCESS_CONTENTS")
            sbSQL.AppendLine(",OPE_EMPLOYEE_CD")
            sbSQL.AppendLine(",OPE_EMPLOYEE_NAME")
            sbSQL.AppendLine(",UPD_DATETIME")
            sbSQL.AppendLine(",UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(",UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(",UPD_TASK")
            sbSQL.AppendLine(",COUNT(*) OVER () AS TotalCount")
            sbSQL.AppendLine("FROM T_OPERATION_LOG")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("1 = 1")

            '検索条件を設定する
            ' 期間(From)
            If Not IsNothing(_ViewModel.SearchDateFrom) AndAlso _ViewModel.SearchDateFrom <> "" Then
                sbSQL.AppendLine("AND PROCESS_DT >= @dateFrom")
                addSqlParam("dateFrom", Replace(_ViewModel.SearchDateFrom, "-", ""))
            End If
            ' 期間(To)
            If Not IsNothing(_ViewModel.SearchDateTo) AndAlso _ViewModel.SearchDateTo <> "" Then
                sbSQL.AppendLine("AND PROCESS_DT <= @dateTo")
                addSqlParam("dateTo", Replace(_ViewModel.SearchDateTo, "-", ""))
            End If
            'ロケーション 大区分
            If Not IsNothing(_ViewModel.SearchLocL) AndAlso _ViewModel.SearchLocL <> "" Then
                sbSQL.AppendLine("AND LOC_L_CD = @locLCd")
                addSqlParam("locLCd", _ViewModel.SearchLocL)
            End If
            'ロケーション 中区分
            If Not IsNothing(_ViewModel.SearchLocM) AndAlso _ViewModel.SearchLocM <> "" Then
                sbSQL.AppendLine("AND LOC_M_CD = @locMCd")
                addSqlParam("locMCd", _ViewModel.SearchLocM)
            End If
            'ロケーション 小区分
            If Not IsNothing(_ViewModel.SearchLocS) AndAlso _ViewModel.SearchLocS <> "" Then
                sbSQL.AppendLine("AND LOC_S_CD = @locSCd")
                addSqlParam("locSCd", _ViewModel.SearchLocS)
            End If
            ' 処理ツール
            If _ViewModel.SearchProcessTool = ProcessToolDiv.Web _
                Or _ViewModel.SearchProcessTool = ProcessToolDiv.CS _
                Or _ViewModel.SearchProcessTool = ProcessToolDiv.HT Then
                sbSQL.AppendLine("AND PROCESS_TOOL_DIV = '" & _ViewModel.SearchProcessTool & "'")
            End If
            ' ログイン者名
            If Not IsNothing(_ViewModel.SearchEmployeeName) AndAlso _ViewModel.SearchEmployeeName <> "" Then
                sbSQL.AppendLine("AND OPE_EMPLOYEE_NAME Like @employeeName")
                addSqlParam("employeeName", $"%{_ViewModel.SearchEmployeeName}%")
            End If
            ' ログ種別
            Dim processCategoryCondition As List(Of String) = New List(Of String)
            'If _ViewModel.SearchProcessCategoryOP Then processCategoryCondition.Add("'" & ProcessCat.Operation & "'")  '2020/03/16 nv miyaji コメントアウト
            If _ViewModel.SearchProcessCategoryEV Then processCategoryCondition.Add("'" & ProcessCat.Event & "'")
            If _ViewModel.SearchProcessCategoryER Then processCategoryCondition.Add("'" & ProcessCat.Error & "'")
            If processCategoryCondition.Count = 1 Then
                sbSQL.AppendLine("AND PROCESS_CAT = " & processCategoryCondition(0))
            ElseIf processCategoryCondition.Count > 1 Then
                sbSQL.Append("AND PROCESS_CAT IN (")
                For i As Integer = 0 To processCategoryCondition.Count - 1
                    If i > 0 Then
                        sbSQL.Append("," & processCategoryCondition(i))
                    Else
                        sbSQL.Append(processCategoryCondition(i))
                    End If
                Next
                sbSQL.AppendLine(")")
            End If
            If isSort Then
                sbSQL.AppendLine("ORDER BY")
                sbSQL.AppendLine("PROCESS_DT DESC")
                sbSQL.AppendLine(",PROCESS_TM DESC")
            End If

            Return sbSQL.ToString()

        End Function

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New OperationLogViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchDateFrom = _ViewModel.SearchDateFrom
            _viewResult.ElementAt(0).SearchDateTo = _ViewModel.SearchDateTo
            _viewResult.ElementAt(0).SearchLocL = _ViewModel.SearchLocL
            _viewResult.ElementAt(0).SearchLocM = _ViewModel.SearchLocM
            _viewResult.ElementAt(0).SearchLocS = _ViewModel.SearchLocS
            _viewResult.ElementAt(0).SearchProcessTool = _ViewModel.SearchProcessTool
            _viewResult.ElementAt(0).SearchEmployeeName = _ViewModel.SearchEmployeeName
            _viewResult.ElementAt(0).SearchProcessCategoryOP = _ViewModel.SearchProcessCategoryOP
            _viewResult.ElementAt(0).SearchProcessCategoryEV = _ViewModel.SearchProcessCategoryEV
            _viewResult.ElementAt(0).SearchProcessCategoryER = _ViewModel.SearchProcessCategoryER
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.OPERATION_LOG, COLSET_LOC.ALL).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfo = GetGridInfo()
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
        End Sub

#End Region

    End Class
End Namespace