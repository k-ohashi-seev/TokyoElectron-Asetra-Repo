Imports Asetra.Models
Imports System.Web.Mvc
Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient
Imports System.Data.Entity.Infrastructure
Imports Asetra.Dao

Namespace Controllers
    Public Class DrugManageListController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"

#End Region

#Region "クラス変数"
        Private _ViewModel As DrugManageListViewModel = New DrugManageListViewModel                 '管理簿出力用表示モデル
        Protected _viewResult As New List(Of DrugManageListViewModel)                               '結果View
        Private _LoginInfo As RfidEntity.LoginUser = Nothing                                        'ログイン情報
        Private _Message As MessageRfid = Nothing
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As DrugSpecListViewModel, ByVal actionname As String) As ActionResult
            'チェックボックス状態の選択状態をクリアする　
            Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
            '全選択ボタン押下フラグをクリアする
            Session.Remove(CommonSessionKey.ALL_SELECT_FLG)

            If Not IsNothing(Session(CommonSessionKey.CREAGE_DRUGMANAGELIST)) Then
                Session.Remove(CommonSessionKey.CREAGE_DRUGMANAGELIST)
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
        Function Index(ByVal ViewModel As DrugManageListViewModel, ByVal actionname As String) As ActionResult

            If IsNothing(actionname) OrElse (actionname <> CommonConst.KEY_ON_CREATE_CLICK AndAlso actionname <> CommonLabel.BUTTON_SEARCH) Then
                Session.Remove(CommonSessionKey.CREAGE_DRUGMANAGELIST)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As DrugManageListViewModel = Session(CommonSessionKey.CREAGE_DRUGMANAGELIST)
            If Not IsNothing(model) AndAlso (actionname = CommonConst.KEY_ON_CREATE_CLICK OrElse actionname = CommonLabel.BUTTON_SEARCH) Then
                _ViewModel = model
                Session.Remove(CommonSessionKey.CREAGE_DRUGMANAGELIST)
            Else
                If String.IsNullOrWhiteSpace(actionname) Then
                    '先月の月末/月初の日付をセット
                    ViewModel.SearchDateFrom = Date.Now.AddMonths(-1).ToString("yyyy-MM-") + "01"
                    ViewModel.SearchDateTo = Date.Parse(Date.Now.ToString("yyyy-MM-") + "01").AddDays(-1).ToString("yyyy-MM-dd")
                End If
                _ViewModel = ViewModel
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
                ViewBag.AuthUpdFlg = False
            Else
                If actionname = Me._Message.getMessage(CommonLabel.BUTTON_SEARCH) OrElse
                    actionname = CommonLabel.BUTTON_SEARCH Then

                    '任意のタイミングで一覧表示処理
                    MyBase.BaseIndex(_ViewModel)
                End If

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm

                ViewBag.LoginInfo = _LoginInfo

                '権限情報の取得
                Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.DrugManageList, _LoginInfo)

                '更新権限の確認
                If AuthInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthUpdFlg = AUTH_FLG.Yes
                Else
                    ViewBag.AuthUpdFlg = AUTH_FLG.No
                End If
            End If

            'ロケーション
            ' 大区分ドロップダウンリスト作成
            GetLocationLData()
            ' 中区分
            GetLocationMData(_ViewModel.SearchLocL)
            ' 小区分
            GetLocationSData(_ViewModel.SearchLocL, _ViewModel.SearchLocM)

            '一覧の選択状態を保持する
            UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            'カテゴリー
            ' 大分類ドロップダウンリスト作成
            GetCategoryLData()
            ' 中分類
            GetCategoryMData(_ViewModel.SearchCatL)
            ' 小分類
            GetCategorySData(_ViewModel.SearchCatL, _ViewModel.SearchCatM)

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.DRUG_MANAGE_LIST)
            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If

            '一覧情報にページ情報を設定
            CopyModelData()

            Return View(_viewResult)
        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()

            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_ASSET_LIST)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()

            'クエリを返却
            MyBase._SQL = CreateSQL()

            '検索条件(全件)を指定したSQLとパラメーターをセッションに保持する
            Session(CommonSessionKey.LIST_VIEW_SQL) = MyBase._SQL
            Session(CommonSessionKey.LIST_VIEW_PARAM) = MyBase._paramSqlList

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of DrugManageListViewModel) = _db.DrugManageListView.SqlQuery(_SQL, _paramSqlList.ToArray)

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

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "ロケ大区分（保管場所）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 保管場所ドロップダウンリストデータ取得
        ''' </summary>
        Private Sub GetLocationLData()

            '必要な情報がない場合は空白をセット（タイムアウトの場合など）
            If IsNothing(_Message) OrElse IsNothing(_LoginInfo) Then
                ViewBag.modelLocL = New SelectList(New List(Of LocationDropdownModel), "LOC_CD", "LOC_NAME", "")
                Return
            End If

            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetList, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            'SQL文
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  AUTH.LOC_L_CD AS LOC_CD")
            sbSQL.AppendLine("  , CONCAT(AUTH.LOC_L_CD, '" & CommonConst.CodeNameDelimiter & "', AUTH.LOC_NAME) AS LOC_NAME ")
            sbSQL.AppendLine("  , AUTH.SHARED_FLG")
            sbSQL.AppendLine("  , AUTH.UPD_FLG")
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH ")
            sbSQL.AppendLine("ORDER BY LOC_CD ")

            '条件設定
            params.Add(New SqlParameter("EmployeeCd", $"{_LoginInfo.EmployeeCd}"))
            params.Add(New SqlParameter("ReadAuthFlg", $"{AuthInfo.ReadAuthFlg}"))
            params.Add(New SqlParameter("UpdAuthFlg", $"{AuthInfo.UpdAuthFlg}"))
            params.Add(New SqlParameter("ReadHubAuthFlg", $"{AuthInfo.ReadHubAuthFlg}"))
            params.Add(New SqlParameter("UpdHubAuthFlg", $"{AuthInfo.UpdHubAuthFlg}"))
            params.Add(New SqlParameter("UpdAuthType", $"{Misc.UpdAuthType.UPD_AUTH}"))

            '拠点情報を取得
            Dim lstAuthLocSF As List(Of AuthLocationSFModel) = _db.AuthLocationSF.SqlQuery(sbSQL.ToString, params.ToArray).ToList
            ViewBag.lstAuthLocSF = lstAuthLocSF

            'ロケ大区分データ取得
            Dim lstLocation As List(Of LocationDropdownModel) = (From row In lstAuthLocSF
                                                                 Select New LocationDropdownModel() With {.LOC_CD = row.LOC_CD, .LOC_NAME = row.LOC_NAME}).ToList

            '画面のロケーションコンボ設定
            ViewBag.modelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchLocL)
            If Not IsNothing(_ViewModel) Then
                _ViewModel.locLDropdownData = lstLocation
            End If

        End Sub
#End Region

#Region "ロケ中区分（階）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 中区分ドロップダウンリストデータ取得
        ''' </summary>
        Private Function GetLocationMData(ByVal selectLCode As String) As ActionResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationMData(selectLCode)
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

            '一覧画面のロケーションコンボ設定
            ViewBag.modelLocM = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchLocM)
            _ViewModel.locMDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "ロケ小区分（棚）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 小区分ドロップダウンリストデータ取得
        ''' </summary>
        Private Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCode As String) As ActionResult
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

            '一覧画面のロケーションコンボ設定
            ViewBag.modelLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _ViewModel.SearchLocS)
            _ViewModel.locSDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "カテゴリー　分類区分ドロップダウン変更処理"
        ''' <summary>
        ''' カテゴリー　分類区分ドロップダウン変更処理
        ''' </summary>
        ''' <param name="categoryLevel"></param>
        ''' <param name="selectLCode"></param>
        ''' <param name="selectMCode"></param>
        ''' <returns></returns>
        Public Function ChangeCategory(ByVal categoryLevel As String, ByVal selectLCode As String, ByVal selectMCode As String) As ActionResult
            If categoryLevel = CAT_DIV.CategoryM Then
                Return Json(GetCategoryMData(selectLCode))
            Else
                Return Json(GetCategorySData(selectLCode, selectMCode))
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
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
            ViewBag.modelCatL = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatL)
            _ViewModel.catLDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "カテゴリー　中分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' カテゴリー　中分類ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCategoryMData(selectLCode As String) As JsonResult
            Dim tbl As DataTable = Dao.ToolCategory.GetCategoryMData(selectLCode)
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
            ViewBag.modelCatM = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatM)
            _ViewModel.catMDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "カテゴリー　小分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' カテゴリー　小分類ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCategorySData(selectLCode As String, selectMCode As String) As JsonResult
            Dim tbl As DataTable = Dao.ToolCategory.GetCategorySData(selectLCode, selectMCode)
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
            ViewBag.modelCatS = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatS)
            _ViewModel.catSDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "一覧データ取得用SQLを作成する"
        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  ASSET.ASSET_NO ")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("  , ASSET.MODEL_NO AS LOT_NO ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM9 AS PURCHASER ")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM13 AS DUE_DATE ")
            sbSQL.AppendLine("  , CASE DRUG_LOG.DRUG_WORK_DIV ")
            sbSQL.AppendLine("  WHEN '3' THEN FORMAT(CONVERT(DATE, CONVERT(NVARCHAR, DRUG_LOG.PROCESS_DT)), 'yyyy/MM/dd') ")
            sbSQL.AppendLine("  ELSE '' ")
            sbSQL.AppendLine("  END AS LATEST_CHECK_DATE ")
            sbSQL.AppendLine("  , LOCM.LOC_NAME AS LOC_M_NAME ")
            sbSQL.AppendLine("  ,COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM M_ASSET ASSET ")
            sbSQL.AppendLine("  LEFT JOIN ( ")
            sbSQL.AppendLine("      SELECT ")
            sbSQL.AppendLine("          DRUG_WORK_DIV ")
            sbSQL.AppendLine("          , PROCESS_DT ")
            sbSQL.AppendLine("          , ASSET_NO ")
            sbSQL.AppendLine("          , RANK() OVER (PARTITION BY ASSET_NO ORDER BY PROCESS_DT DESC, PROCESS_TM DESC) AS RANK ")
            sbSQL.AppendLine("      FROM T_DRUG_DUE_LOG ")
            sbSQL.AppendLine("      ) AS DRUG_LOG ")
            sbSQL.AppendLine("      ON ASSET.ASSET_NO = DRUG_LOG.ASSET_NO ")
            sbSQL.AppendLine("      AND DRUG_LOG.RANK = 1 ")
            sbSQL.AppendLine("  LEFT JOIN M_LOCATION LOCM ")
            sbSQL.AppendLine($"      ON LOCM.LOC_DIV = '{LOC_DIV.LocationM}' ")
            sbSQL.AppendLine("      AND ASSET.LOC_L_CD = LOCM.LOC_L_CD ")
            sbSQL.AppendLine("      AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '検索条件を設定する
            ' 期間(From)
            If Not String.IsNullOrEmpty(_ViewModel.SearchDateFrom) Then
                sbSQL.AppendLine("  AND DRUG_LOG.PROCESS_DT >= @PROCESS_DT_FROM")
                addSqlParam("PROCESS_DT_FROM", $"{_ViewModel.SearchDateFrom.Replace("-", "")}")
            End If
            ' 期間(To)
            If Not String.IsNullOrEmpty(_ViewModel.SearchDateTo) Then
                sbSQL.AppendLine("  AND DRUG_LOG.PROCESS_DT <= @PROCESS_DT_TO")
                addSqlParam("PROCESS_DT_TO", $"{_ViewModel.SearchDateTo.Replace("-", "")}")
            End If
            ' 保管場所
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                sbSQL.AppendLine("  AND ASSET.LOC_L_CD = @LOC_L_CD")
                addSqlParam("LOC_L_CD", $"{_ViewModel.SearchLocL}")
            End If
            ' 階
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocM) Then
                sbSQL.AppendLine("  AND ASSET.LOC_M_CD = @LOC_M_CD")
                addSqlParam("LOC_M_CD", $"{_ViewModel.SearchLocM}")
            End If
            ' 棚
            If Not String.IsNullOrEmpty(_ViewModel.SearchLocS) Then
                sbSQL.AppendLine("  AND ASSET.LOC_S_CD = @LOC_S_CD")
                addSqlParam("LOC_S_CD", $"{_ViewModel.SearchLocS}")
            End If
            ' 大分類
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatL) Then
                sbSQL.AppendLine("  AND ASSET.CAT_L_CD = @CAT_L_CD")
                addSqlParam("CAT_L_CD", $"{_ViewModel.SearchCatL}")
            End If
            ' 中分類
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatM) Then
                sbSQL.AppendLine("  AND ASSET.CAT_M_CD = @CAT_M_CD")
                addSqlParam("CAT_M_CD", $"{_ViewModel.SearchCatM}")
            End If
            ' 小分類
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatS) Then
                sbSQL.AppendLine("  AND ASSET.CAT_S_CD = @CAT_S_CD")
                addSqlParam("CAT_S_CD", $"{_ViewModel.SearchCatS}")
            End If
            ' 薬品番号（前方一致）
            If Not String.IsNullOrEmpty(_ViewModel.SearchDrugNo) Then
                sbSQL.AppendLine("  AND ASSET.ASSET_NO LIKE @ASSET_NO")
                addSqlParam("ASSET_NO", $"{_ViewModel.SearchDrugNo}%")
            End If
            ' 薬品名称（部分一致）
            If Not String.IsNullOrEmpty(_ViewModel.SearchDrugName) Then
                sbSQL.AppendLine("  AND ASSET.ASSET_NAME LIKE @ASSET_NAME")
                addSqlParam("ASSET_NAME", $"%{_ViewModel.SearchDrugName}%")
            End If

            sbSQL.AppendLine(" ORDER BY CONVERT(DATE, ASSET.FREE_ITEM13), ASSET.ASSET_NO ")

            Return sbSQL.ToString()
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
            Dim lstDrugManage As New List(Of DrugManageListViewModel)

            '選択解除の場合、初期化する
            If Not blnSelectAll Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                Return Json("0")
            Else
                '全選択ボタン押下フラグをセットする　
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
                    lstDrugManage = _db.DrugManageListView.SqlQuery(strSQL).ToList
                Else

                    lstDrugManage = _db.DrugManageListView.SqlQuery(strSQL, lstParamCopy.ToArray).ToList
                End If
            End If

            'チェックボックスlstAssetListの選択状態のキー(資産番号)を更新する
            If lstDrugManage.Count > 0 Then
                Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstDrugManage.Select(Function(d) $"{d.ASSET_NO}").ToList
            End If

            '件数を返却する
            Return Json(lstDrugManage.Count)

        End Function
#End Region

#Region "管理簿確認"
        ''' <summary>
        ''' 管理簿確認
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <param name="actionname">アクション名</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function ManageBookCheck(ByVal ViewModel As DrugManageListViewModel, ByVal actionname As String) As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            _ViewModel = ViewModel

            Dim wkAssetNoList = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)
            Dim arrayAssetNo As String() = wkAssetNoList.ToArray
            Dim assetModel As AssetListModel = Nothing

            For Each assetNo As String In arrayAssetNo

                '薬品重量履歴の登録
                Dao.ToolDrugDueLog.InsDrugDueLogDivCheck(assetNo,
                                                         _LoginInfo,
                                                         Me._Message.getMessage(CommonLabelDrugManageList.TITLE))

                '資産番号で資産マスタを検索
                assetModel = _db.AssetList.Find(assetNo)
                If IsNothing(assetModel) Then
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_NOT_EXITS_ASSET_NO

                    'モデルの内容をセッションに保持
                    Session(CommonSessionKey.CREAGE_DRUGMANAGELIST) = _ViewModel

                    Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
                End If

                '資産処理履歴の登録
                InsertTProcessLog(_db, assetModel)

            Next

            _ViewModel.States = StatusType.SuccessCreate
            Session(CommonSessionKey.CREAGE_DRUGMANAGELIST) = _ViewModel

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelDrugManageList.TITLE) & " " &
                                                Me._Message.getMessage(CommonLabel.BUTTON_MANAGEBOOK_CHECK) & " " &
                                                Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                arrayAssetNo.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelDrugManageList.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonLabel.BUTTON_SEARCH})
        End Function
#End Region

#Region "帳票出力"
        ''' <summary>
        ''' 帳票出力
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <param name="actionname">アクション名</param>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Function ReportOutput(ByVal ViewModel As DrugManageListViewModel, ByVal actionname As String) As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            _ViewModel = ViewModel

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '期限（From）が空白もしくは数値ではない場合、-1をセット
            Dim iSearhDtFrom As Integer
            If String.IsNullOrEmpty(ViewModel.SearchDateFrom) OrElse Integer.TryParse(ViewModel.SearchDateFrom.Replace("-", ""), iSearhDtFrom) = False Then
                iSearhDtFrom = -1
            End If

            '期限（To）が空白もしくは数値ではない場合、-1をセット
            Dim iSearhDtTo As Integer
            If String.IsNullOrEmpty(ViewModel.SearchDateTo) OrElse Integer.TryParse(ViewModel.SearchDateTo.Replace("-", ""), iSearhDtTo) = False Then
                iSearhDtTo = -1
            End If

            '出力用データ取得
            Dim lstDrugManage As List(Of DrugManageInfo) = Dao.ToolDrugManageList.GetDrugManageListData(iSearhDtFrom,
                                                                                                        iSearhDtTo,
                                                                                                        ViewModel.SearchLocL,
                                                                                                        ViewModel.SearchLocM,
                                                                                                        ViewModel.SearchLocS,
                                                                                                        ViewModel.SearchCatL,
                                                                                                        ViewModel.SearchCatM,
                                                                                                        ViewModel.SearchCatS,
                                                                                                        ViewModel.SearchDrugNo,
                                                                                                        ViewModel.SearchDrugName)

            '画面でチェックしている資産だけ抽出とする
            If lstDrugManage Is Nothing Then
                lstDrugManage = New List(Of DrugManageInfo)
            End If
            Dim wkAssetNoList = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)
            Dim wkList = New List(Of DrugManageInfo)
            For Each drug In lstDrugManage
                For Each delAsset In wkAssetNoList
                    If delAsset.Equals(drug.AssetNo) Then
                        wkList.Add(drug)
                        Exit For
                    End If
                Next
            Next
            lstDrugManage = wkList

            If IsNothing(lstDrugManage) OrElse lstDrugManage.Count <= 0 Then
                '出力データが存在しない場合
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_NOT_EXISTS_OUT_PUT_DATA

                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_DRUGMANAGELIST) = _ViewModel

                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'CSVファイル出力先フォルダに作成する
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            'Excelテンプレートパスの設定
            Dim sTempFullPath As String = Server.MapPath(CommonConst.TEMPLATE_FOLDERNM)

            'PDFファイル出力名取得
            Dim setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ManagePrint.KeyDiv, CommonSettingKey.ManagePrint.Key1, CommonSettingKey.ManagePrint.Key2, CommonSettingKey.ManagePrint.Key3, True)
            Dim sDspFileName As String = String.Format(setting.DATA1 + "_{0}.pdf", Now.ToString("yyyyMMddHHmmss"))

            '期間Fromと期間Toの区切り文字を変換
            Dim sSearchDateFrom As String = ""
            If Not IsNothing(ViewModel.SearchDateFrom) Then
                sSearchDateFrom = ViewModel.SearchDateFrom.Replace("-", "/")
            End If
            Dim sSearchDateTo As String = ""
            If Not IsNothing(ViewModel.SearchDateTo) Then
                sSearchDateTo = ViewModel.SearchDateTo.Replace("-", "/")
            End If

            '指定数量管理表PDF一時ファイル名取得
            Dim sTmpPdfFileNm As String = Dao.ToolDrugManageList.MakeDrugManageList(sTempFullPath, sFullPath, lstDrugManage, sSearchDateFrom, sSearchDateTo)

            'ファイルをダウンロードさせる
            Call FileDownload(sDspFileName, "application/pdf", sTmpPdfFileNm)

            'ダウンロードさせた後、ファイルを削除。
            System.IO.File.Delete(sTmpPdfFileNm)

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelDrugManageList.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_REPORT_OUTPUT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     lstDrugManage.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelDrugManageList.TITLE))

            '結果表示
            Return New EmptyResult()
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

#Region "管理簿確認 資産処理履歴登録"
        ''' <summary>
        ''' 管理簿確認 資産処理履歴登録
        ''' </summary>
        ''' <param name="mAssetRow"></param>
        ''' <returns></returns>
        Private Function InsertTProcessLog(ByRef db As SystemDBContext, ByVal mAssetRow As AssetListModel) As Boolean
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)

            Try
                sbSQL.Clear()
                params = New List(Of SqlParameter)

                sbSQL.AppendLine("INSERT INTO T_PROCESS_LOG")
                sbSQL.AppendLine("(")
                sbSQL.AppendLine("    PROCESS_DT")
                sbSQL.AppendLine("    ,PROCESS_TM")
                sbSQL.AppendLine("    ,ASSET_NO")
                sbSQL.AppendLine("    ,ASSET_ID")
                sbSQL.AppendLine("    ,PROCESS_TOOL_DIV")
                sbSQL.AppendLine("    ,PROCESS_DIV")
                sbSQL.AppendLine("    ,LEASE_STATUS")
                sbSQL.AppendLine("    ,MNG_LOC_L_CD")
                sbSQL.AppendLine("    ,LOC_L_CD")
                sbSQL.AppendLine("    ,LOC_M_CD")
                sbSQL.AppendLine("    ,LOC_S_CD")
                sbSQL.AppendLine("    ,LOC_L_NAME")
                sbSQL.AppendLine("    ,LOC_M_NAME")
                sbSQL.AppendLine("    ,LOC_S_NAME")
                sbSQL.AppendLine("    ,ASSET_NAME")
                sbSQL.AppendLine("    ,MAKER")
                sbSQL.AppendLine("    ,PRODUCT_NO")
                sbSQL.AppendLine("    ,MODEL_NO")
                sbSQL.AppendLine("    ,TASK_RESULT")
                sbSQL.AppendLine("    ,UPD_DATETIME")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,UPD_TASK")
                sbSQL.AppendLine(") VALUES (")
                sbSQL.AppendLine("    FORMAT(GetDate(), 'yyyyMMdd')")
                sbSQL.AppendLine("    ,FORMAT(GetDate(), 'HHmmssfff')")
                sbSQL.AppendLine("    ,@ASSET_NO")
                sbSQL.AppendLine("    ,@ASSET_ID")
                sbSQL.AppendLine("    ,@PROCESS_TOOL_DIV")
                sbSQL.AppendLine("    ,@PROCESS_DIV")
                sbSQL.AppendLine("    ,@LEASE_STATUS")
                sbSQL.AppendLine("    ,@MNG_LOC_L_CD")
                sbSQL.AppendLine("    ,@LOC_L_CD")
                sbSQL.AppendLine("    ,@LOC_M_CD")
                sbSQL.AppendLine("    ,@LOC_S_CD")
                sbSQL.AppendLine("    ,@LOC_L_NAME")
                sbSQL.AppendLine("    ,@LOC_M_NAME")
                sbSQL.AppendLine("    ,@LOC_S_NAME")
                sbSQL.AppendLine("    ,@ASSET_NAME")
                sbSQL.AppendLine("    ,@MAKER")
                sbSQL.AppendLine("    ,@PRODUCT_NO")
                sbSQL.AppendLine("    ,@MODEL_NO")
                sbSQL.AppendLine("    ,@TASK_RESULT")
                sbSQL.AppendLine("    ,GetDate()")
                sbSQL.AppendLine("    ,@UPD_EMPLOYEE_CD")
                sbSQL.AppendLine("    ,@UPD_EMPLOYEE_NAME")
                sbSQL.AppendLine("    ,@UPD_TASK")
                sbSQL.AppendLine(") ")

                params.Add(New SqlParameter($"@ASSET_NO", mAssetRow.ASSET_NO))
                params.Add(New SqlParameter($"@ASSET_ID", GetDBNullValueIfNothing(mAssetRow.ASSET_ID)))
                params.Add(New SqlParameter($"@PROCESS_TOOL_DIV", ProcessToolDiv.Web))
                params.Add(New SqlParameter($"@PROCESS_DIV", ProcessDiv.DC))
                params.Add(New SqlParameter($"@LEASE_STATUS", mAssetRow.LEASE_STATUS))
                params.Add(New SqlParameter($"@MNG_LOC_L_CD", mAssetRow.MNG_LOC_L_CD))
                params.Add(New SqlParameter($"@LOC_L_CD", mAssetRow.LOC_L_CD))
                params.Add(New SqlParameter($"@LOC_M_CD", mAssetRow.LOC_M_CD))
                params.Add(New SqlParameter($"@LOC_S_CD", mAssetRow.LOC_S_CD))

                Dim sLocLName As String = String.Empty
                Dim sLocMName As String = String.Empty
                Dim sLocSName As String = String.Empty

                If Not String.IsNullOrEmpty(mAssetRow.LOC_L_CD.Trim) Then
                    'IDをキーにロケーションモデルを取得する
                    Dim findKeys As String() = {LOC_DIV.LocationL, mAssetRow.LOC_L_CD, " ", " "}
                    Dim locamodel As LocationModel = db.Location.Find(findKeys)
                    sLocLName = locamodel.LOC_NAME
                End If
                If Not String.IsNullOrEmpty(mAssetRow.LOC_M_CD.Trim) Then
                    'IDをキーにロケーションモデルを取得する
                    Dim findKeys As String() = {LOC_DIV.LocationM, mAssetRow.LOC_L_CD, mAssetRow.LOC_M_CD, " "}
                    Dim locamodel As LocationModel = db.Location.Find(findKeys)
                    sLocMName = locamodel.LOC_NAME
                End If
                If Not String.IsNullOrEmpty(mAssetRow.LOC_S_CD.Trim) Then
                    'IDをキーにロケーションモデルを取得する
                    Dim findKeys As String() = {LOC_DIV.LocationS, mAssetRow.LOC_L_CD, mAssetRow.LOC_M_CD, mAssetRow.LOC_S_CD}
                    Dim locamodel As LocationModel = db.Location.Find(findKeys)
                    sLocSName = locamodel.LOC_NAME
                End If

                params.Add(New SqlParameter($"@LOC_L_NAME", sLocLName))
                params.Add(New SqlParameter($"@LOC_M_NAME", sLocMName))
                params.Add(New SqlParameter($"@LOC_S_NAME", sLocSName))
                params.Add(New SqlParameter($"@ASSET_NAME", GetDBNullValueIfNothing(mAssetRow.ASSET_NAME)))
                params.Add(New SqlParameter($"@MAKER", GetDBNullValueIfNothing(mAssetRow.MAKER)))
                params.Add(New SqlParameter($"@PRODUCT_NO", GetDBNullValueIfNothing(mAssetRow.PRODUCT_NO)))
                params.Add(New SqlParameter($"@MODEL_NO", GetDBNullValueIfNothing(mAssetRow.MODEL_NO)))
                params.Add(New SqlParameter($"@TASK_RESULT", TaskResult.OK))
                If IsNothing(_LoginInfo) Then
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", String.Empty))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", String.Empty))
                Else
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                End If
                params.Add(New SqlParameter($"@UPD_TASK", Me._Message.getMessage(CommonLabelDrugManageList.TITLE)))

                db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            End Try

            Return True
        End Function
#End Region

#Region "DB登録用のNULL値を返却"
        ''' <summary>
        ''' Nothingの場合、DBNull.Valueを返す
        ''' </summary>
        ''' <param name="obj">試す値</param>
        ''' <returns>Nothingの場合、DBNull.Value。Nothingではない場合、そのままの値を返す</returns>
        Public Function GetDBNullValueIfNothing(ByVal obj As Object) As Object

            If IsNothing(obj) Then
                Return DBNull.Value
            Else
                Return obj
            End If

        End Function
#End Region

#Region "共通"

#Region "ページ情報のモデルをコピーします"
        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New DrugManageListViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchDateFrom = _ViewModel.SearchDateFrom
            _viewResult.ElementAt(0).SearchDateTo = _ViewModel.SearchDateTo
            _viewResult.ElementAt(0).SearchLocL = _ViewModel.SearchLocL
            _viewResult.ElementAt(0).SearchLocM = _ViewModel.SearchLocM
            _viewResult.ElementAt(0).SearchLocS = _ViewModel.SearchLocS
            _viewResult.ElementAt(0).SearchCatL = _ViewModel.SearchCatL
            _viewResult.ElementAt(0).SearchCatM = _ViewModel.SearchCatM
            _viewResult.ElementAt(0).SearchCatS = _ViewModel.SearchCatS
            _viewResult.ElementAt(0).SearchDrugNo = _ViewModel.SearchDrugNo
            _viewResult.ElementAt(0).SearchDrugName = _ViewModel.SearchDrugName
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.DRUG_MANAGE_LIST)
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.DRUG_MANAGE_LIST).FIX_COL_CNT

        End Sub
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

#End Region

    End Class
End Namespace