Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Data.SqlClient

Namespace Controllers

    ''' <summary>
    ''' 貸出履歴照会画面表示用のクラス
    ''' </summary>
    Public Class AssetLeaseHitoryController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"
#End Region

#Region "クラス変数"
        'Private _Model As AssetLeaseHitoryModel = New AssetLeaseHitoryModel                                     'AssetLeaseHitoryモデル
        Private _ViewModel As AssetLeaseHitoryViewModel = New AssetLeaseHitoryViewModel                         'AssetLeaseHitory一覧表示用モデル
        Protected _viewResult As List(Of AssetLeaseHitoryViewModel) = New List(Of AssetLeaseHitoryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
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
        Public Function Init(ByVal ViewModel As AssetLeaseHitoryViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(RedirectAssetListInfo.ID) IsNot Nothing Then
                'チェックボックス状態の選択状態をクリアする
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
            End If

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
        Public Function Index(ByVal ViewModel As AssetLeaseHitoryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_LENDING_LIST)
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
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '検索ボタンがクリックされた場合はセッションをクリアする
            If Me._Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                'チェックボックス状態の選択状態をクリアする
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As AssetLeaseHitoryViewModel = Session(CommonSessionKey.CREAGE_LENDING_LIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            '20230901 ADD Start Shirakawa
            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid                      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_LEASE_HISTORY.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_LEASE_HISTORY.Key1,
                                                                      CommonSettingKey.Web_Read_Data_LEASE_HISTORY.Key2,
                                                                      CommonSettingKey.Web_Read_Data_LEASE_HISTORY.Key3)
            '20230901 ADD End Shirakawa

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else

                '管理ロケーションチェックボックスリスト作成
                _ViewModel.mngLocCheckBoxListData = GetMngLocData()

                '検索条件の初期値設定
                If String.IsNullOrEmpty(_ViewModel.SelectMngLocKeys) Then
                    _ViewModel.SearchTargetPeriodFrom = New Date(Now.Year, Now.Month, 1).ToString("yyyy-MM-dd")
                    _ViewModel.SearchTargetPeriodTo = New Date(Now.Year, Now.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd")
                    '管理ロケーションの選択キーリスト
                    _ViewModel.SelectMngLocKeys = GetDefaultSelectMngLocKeys()
                End If

                '一覧表示処理（管理ロケーションチェックボックスリストが存在しない場合は、初期表示と判断し、グリッド表示しない）
                If Not String.IsNullOrEmpty(_ViewModel.SelectMngLocKeys) Then
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
                End If

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

            ' 大分類ドロップダウンリスト作成
            GetCategoryLData()
            ' 中分類
            GetCategoryMData(_ViewModel.SearchCatL)
            ' 小分類
            GetCategorySData(_ViewModel.SearchCatL, _ViewModel.SearchCatM)

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.ASSET_LEASE_HITORY)
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
                Session.Remove(CommonSessionKey.CREAGE_LENDING_LIST)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLeaseHitory, _LoginInfo)
            '抽出期間
            Dim iTargetPeriodFrom As Integer = Integer.Parse(Date.Parse(_ViewModel.SearchTargetPeriodFrom).ToString("yyyyMMdd"))
            Dim iTargetPeriodTo As Integer = Integer.Parse(Date.Parse(_ViewModel.SearchTargetPeriodTo).ToString("yyyyMMdd"))
            '営業日数の取得
            Dim iEigyoDay As Integer = Dao.ToolReserve.GetEigyoDay(iTargetPeriodFrom, iTargetPeriodTo)

            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            If _isSort Then
                sbSQL.AppendLine("SELECT ")
            Else
                sbSQL.AppendLine("SELECT TOP 1")
            End If
            sbSQL.AppendLine("  ASSET.MNG_LOC_L_CD")
            sbSQL.AppendLine("  , MNG_LOC.LOC_NAME AS MNG_LOC_L_NAME")
            sbSQL.AppendLine("  , ASSET.CAT_L_CD")
            sbSQL.AppendLine("  , ASSET.CAT_M_CD")
            sbSQL.AppendLine("  , ASSET.CAT_S_CD")
            sbSQL.AppendLine("  , CATL.CAT_NAME AS CAT_L_NAME ")
            sbSQL.AppendLine("  , CATM.CAT_NAME AS CAT_M_NAME ")
            sbSQL.AppendLine("  , CATS.CAT_NAME AS CAT_S_NAME ")
            sbSQL.AppendLine("  , ASSET.ASSET_NO")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME")
            sbSQL.AppendLine($"  , CASE ASSET.ASSET_STATUS WHEN '{ASSET_STATUS.Removed}' THEN '○' ELSE '' END DISPOSE ")                '2024/04/17 Add Tomatsuri
            sbSQL.AppendLine("  , dbo.ASSET_RESERVE_COUNT_SF(ASSET.ASSET_NO, @TargetPeriodFrom, @TargetPeriodTo) AS LEASE_CNT")
            sbSQL.AppendLine("  , dbo.ASSET_RESERVE_DAYCOUNT_SF(ASSET.ASSET_NO, @TargetPeriodFrom, @TargetPeriodTo) AS LEASE_DAY")
            sbSQL.AppendLine("  , dbo.ASSET_RESERVE_MAINTEDAYCOUNT_SF(ASSET.ASSET_NO, @TargetPeriodFrom, @TargetPeriodTo) AS MAINTE_DAY") '2022/8/31 Add Kawamoto 
            sbSQL.AppendLine("  , CONVERT(int, @EigyoDay) AS EIGYO_DAY")
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            addSqlParam("TargetPeriodFrom", $"{iTargetPeriodFrom}")
            addSqlParam("TargetPeriodTo", $"{iTargetPeriodTo}")
            addSqlParam("EigyoDay", $"{iEigyoDay}")
            sbSQL.AppendLine("FROM")
            sbSQL.AppendLine("  M_ASSET ASSET ")
            sbSQL.AppendLine("  LEFT JOIN M_LOCATION MNG_LOC ")
            sbSQL.AppendLine("  ON  MNG_LOC.LOC_DIV = @LocDivL")
            sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD = MNG_LOC.LOC_L_CD")
            addSqlParam("LocDivL", $"{LOC_DIV.LocationL}")
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATL ")
            sbSQL.AppendLine("  ON  CATL.CAT_DIV = @CatDivL")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATL.CAT_L_CD")
            addSqlParam("CatDivL", $"{CAT_DIV.CategoryL}")
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATM ")
            sbSQL.AppendLine("  ON  CATM.CAT_DIV = @CatDivM")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATM.CAT_L_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_M_CD = CATM.CAT_M_CD")
            addSqlParam("CatDivM", $"{CAT_DIV.CategoryM}")
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATS ")
            sbSQL.AppendLine("  ON  CATS.CAT_DIV = @CatDivS")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATS.CAT_L_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_M_CD = CATS.CAT_M_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_S_CD = CATS.CAT_S_CD")
            addSqlParam("CatDivS", $"{CAT_DIV.CategoryS}")
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuthHub) AUTH")
            sbSQL.AppendLine("  ON  ASSET.MNG_LOC_L_CD = AUTH.LOC_L_CD")
            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{authInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{authInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{authInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{authInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthType_UpdAuthHub", $"{UpdAuthType.UPD_AUTH_HUB}")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '20240417 Tomatsuri ADD Start
            '検索条件を設定する
            '除却対象
            If Not _ViewModel.SearchDispose Then
                sbSQL.AppendLine($"  AND ASSET.ASSET_STATUS <> '{ASSET_STATUS.Removed}'")           '除却以外を対象
            End If
            '20240417 Tomatsuri ADD End

            'カテゴリ大分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatL) Then
                sbSQL.AppendLine("  AND ASSET.CAT_L_CD = @SearchCatL")
                addSqlParam("SearchCatL", $"{_ViewModel.SearchCatL}")
            End If

            'カテゴリ中分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatM) Then
                sbSQL.AppendLine("  AND ASSET.CAT_M_CD = @SearchCatM")
                addSqlParam("SearchCatM", $"{_ViewModel.SearchCatM}")
            End If

            'カテゴリ小分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatS) Then
                sbSQL.AppendLine("  AND ASSET.CAT_S_CD = @SearchCatS")
                addSqlParam("SearchCatS", $"{_ViewModel.SearchCatS}")
            End If

            '管理ロケ大区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SelectMngLocKeys) Then
                Dim sbSQLMngLocKey As StringBuilder = New StringBuilder
                Dim selectMngLocKeys As String() = _ViewModel.SelectMngLocKeys.Split(", ")
                For idx As Integer = 0 To selectMngLocKeys.Length - 1
                    If sbSQLMngLocKey.Length <> 0 Then
                        sbSQLMngLocKey.Append(", ")
                    End If
                    sbSQLMngLocKey.Append("@MngLocLCd" & idx)
                    addSqlParam("MngLocLCd" & idx, $"{selectMngLocKeys(idx)}")
                Next
                sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD IN (" & sbSQLMngLocKey.ToString & ")")
            End If

            If _isSort Then
                sbSQL.AppendLine("ORDER BY")
                sbSQL.AppendLine("  MNG_LOC.LOC_SHARE_FLG DESC")
                sbSQL.AppendLine("  , ASSET.MNG_LOC_L_CD")
                sbSQL.AppendLine("  , ASSET.CAT_L_CD")
                sbSQL.AppendLine("  , ASSET.CAT_M_CD")
                sbSQL.AppendLine("  , ASSET.CAT_S_CD")
                sbSQL.AppendLine("  , ASSET.ASSET_NO")
            End If

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of AssetLeaseHitoryViewModel) = _db.AssetLeaseHitoryView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
            Dim blnReturn As Boolean = True
            Return blnReturn
        End Function

        ''' <summary>
        ''' 削除処理のクエリを作成します
        ''' </summary>
        Protected Overrides Sub DeleteQuery()
        End Sub
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ViewModel As AssetLeaseHitoryViewModel) As ActionResult
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

            '20230901 ADD Start Shirakawa
            '設定マスタ(web読取データ)取得
            Dim settingInfo As MessageRfid                      'メッセージ兼設定リスト
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim WebReadData As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.Web_Read_Data_LEASE_HISTORY.KeyDiv,
                                                                      CommonSettingKey.Web_Read_Data_LEASE_HISTORY.Key1,
                                                                      CommonSettingKey.Web_Read_Data_LEASE_HISTORY.Key2,
                                                                      CommonSettingKey.Web_Read_Data_LEASE_HISTORY.Key3)
            '20230901 ADD End Shirakawa

            'モデルを設定
            _ViewModel = ViewModel

            Dim AssetLeaseHitoryData As List(Of AssetLeaseHitoryViewModel)

            'CSV出力データ取得
            AssetLeaseHitoryData = GetAssetLeaseHitoryData()

            '20230901 ADD Start Shirakawa
            '設定マスタから取得できた場合
            '設定マスタから取得できた場合 かつ 上限値(Data2)が存在している場合 かつ 上限を超えていた場合
            If WebReadData IsNot Nothing AndAlso String.IsNullOrEmpty(WebReadData.DATA2) = False AndAlso WebReadData.DATA2 < AssetLeaseHitoryData.Count Then
                '上限を超えていた場合　CSV出力しない

                '上限確認フラグ
                ViewBag.isLimitFlg = True
                '一覧データ取得件数
                ViewBag.cntList = AssetLeaseHitoryData.Count
                '設定マスタ上限件数　初期化
                ViewBag.cntLimit = WebReadData.DATA2

                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '20230901 ADD End Shirakawa

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.LendingHist.KeyDiv, CommonSettingKey.LendingHist.Key1, CommonSettingKey.LendingHist.Key2, CommonSettingKey.LendingHist.Key3, True)
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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_ASSET_LEASE_HISTORY)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_ASSET_LEASE_HISTORY)


            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As AssetLeaseHitoryViewModel In AssetLeaseHitoryData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_M_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_S_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DISPOSE)      '2024/4/17 Add Tomatsuri
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_CNT)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_DAY)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MAINTE_DAY)   '2022/8/31 Add Kawamoto
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.EIGYO_DAY)
                Dim dLendingRate As Decimal = 0
                If row.EIGYO_DAY <> 0 Then
                    dLendingRate = CommonRfid.ToRound((row.LEASE_DAY + row.MAINTE_DAY) / row.EIGYO_DAY, 5)   '2022/8/31 Upd Kawamoto
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(dLendingRate)

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetLeaseHitory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     AssetLeaseHitoryData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetLeaseHitory.TITLE))


            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()
        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetAssetLeaseHitoryData() As List(Of AssetLeaseHitoryViewModel)
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLeaseHitory, _LoginInfo)
            '抽出期間
            Dim iTargetPeriodFrom As Integer = Integer.Parse(Date.Parse(_ViewModel.SearchTargetPeriodFrom).ToString("yyyyMMdd"))
            Dim iTargetPeriodTo As Integer = Integer.Parse(Date.Parse(_ViewModel.SearchTargetPeriodTo).ToString("yyyyMMdd"))
            '営業日数の取得
            Dim iEigyoDay As Integer = Dao.ToolReserve.GetEigyoDay(iTargetPeriodFrom, iTargetPeriodTo)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("  ASSET.MNG_LOC_L_CD")
            sbSQL.AppendLine("  , MNG_LOC.LOC_NAME AS MNG_LOC_L_NAME")
            sbSQL.AppendLine("  , ASSET.CAT_L_CD")
            sbSQL.AppendLine("  , ASSET.CAT_M_CD")
            sbSQL.AppendLine("  , ASSET.CAT_S_CD")
            sbSQL.AppendLine("  , CATL.CAT_NAME AS CAT_L_NAME ")
            sbSQL.AppendLine("  , CATM.CAT_NAME AS CAT_M_NAME ")
            sbSQL.AppendLine("  , CATS.CAT_NAME AS CAT_S_NAME ")
            sbSQL.AppendLine("  , ASSET.ASSET_NO")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME")
            sbSQL.AppendLine($"  , CASE ASSET.ASSET_STATUS WHEN '{ASSET_STATUS.Removed}' THEN 'Y' ELSE 'N' END DISPOSE ")                 '2024/04/17 Add Tomatsuri
            sbSQL.AppendLine("  , dbo.ASSET_RESERVE_COUNT_SF(ASSET.ASSET_NO, @TargetPeriodFrom, @TargetPeriodTo) AS LEASE_CNT")
            sbSQL.AppendLine("  , dbo.ASSET_RESERVE_DAYCOUNT_SF(ASSET.ASSET_NO, @TargetPeriodFrom, @TargetPeriodTo) AS LEASE_DAY")
            sbSQL.AppendLine("  , dbo.ASSET_RESERVE_MAINTEDAYCOUNT_SF(ASSET.ASSET_NO, @TargetPeriodFrom, @TargetPeriodTo) AS MAINTE_DAY") '2022/8/31 Add Kawamoto 
            sbSQL.AppendLine("  , CONVERT(int, @EigyoDay) AS EIGYO_DAY")
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            params.Add(New SqlParameter($"@TargetPeriodFrom", iTargetPeriodFrom))
            params.Add(New SqlParameter($"@TargetPeriodTo", iTargetPeriodTo))
            params.Add(New SqlParameter($"@EigyoDay", iEigyoDay))
            sbSQL.AppendLine("FROM")
            sbSQL.AppendLine("  M_ASSET ASSET ")
            sbSQL.AppendLine("  LEFT JOIN M_LOCATION MNG_LOC ")
            sbSQL.AppendLine("  ON  MNG_LOC.LOC_DIV = @LocDivL")
            sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD = MNG_LOC.LOC_L_CD")
            params.Add(New SqlParameter($"@LocDivL", LOC_DIV.LocationL))
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATL ")
            sbSQL.AppendLine("  ON  CATL.CAT_DIV = @CatDivL")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATL.CAT_L_CD")
            params.Add(New SqlParameter($"@CatDivL", CAT_DIV.CategoryL))
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATM ")
            sbSQL.AppendLine("  ON  CATM.CAT_DIV = @CatDivM")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATM.CAT_L_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_M_CD = CATM.CAT_M_CD")
            params.Add(New SqlParameter($"@CatDivM", CAT_DIV.CategoryM))
            sbSQL.AppendLine("  LEFT JOIN M_CATEGORY CATS ")
            sbSQL.AppendLine("  ON  CATS.CAT_DIV = @CatDivS")
            sbSQL.AppendLine("  AND ASSET.CAT_L_CD = CATS.CAT_L_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_M_CD = CATS.CAT_M_CD")
            sbSQL.AppendLine("  AND ASSET.CAT_S_CD = CATS.CAT_S_CD")
            params.Add(New SqlParameter($"@CatDivS", CAT_DIV.CategoryS))
            sbSQL.AppendLine("  INNER JOIN dbo.AUTH_LOCATION_SF(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuthHub) AUTH")
            sbSQL.AppendLine("  ON  ASSET.MNG_LOC_L_CD = AUTH.LOC_L_CD")
            params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@ReadAuthFlg", authInfo.ReadAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthFlg", authInfo.UpdAuthFlg))
            params.Add(New SqlParameter($"@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
            params.Add(New SqlParameter($"@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthType_UpdAuthHub", UpdAuthType.UPD_AUTH_HUB))
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '20240417 Tomatsuri ADD Start
            '検索条件を設定する
            '除却対象
            If Not _ViewModel.SearchDispose Then
                sbSQL.AppendLine($"  AND ASSET.ASSET_STATUS <> '{ASSET_STATUS.Removed}'")           '除却以外を対象
            End If
            '20240417 Tomatsuri ADD End

            '検索条件を設定する
            'カテゴリ大分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatL) Then
                sbSQL.AppendLine("  AND ASSET.CAT_L_CD = @SearchCatL")
                params.Add(New SqlParameter($"@SearchCatL", _ViewModel.SearchCatL))
            End If

            'カテゴリ中分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatM) Then
                sbSQL.AppendLine("  AND ASSET.CAT_M_CD = @SearchCatM")
                params.Add(New SqlParameter($"@SearchCatM", _ViewModel.SearchCatM))
            End If

            'カテゴリ小分類コード
            If Not String.IsNullOrEmpty(_ViewModel.SearchCatS) Then
                sbSQL.AppendLine("  AND ASSET.CAT_S_CD = @SearchCatS")
                params.Add(New SqlParameter($"@SearchCatS", _ViewModel.SearchCatS))
            End If

            '管理ロケ大区分コード
            If Not String.IsNullOrEmpty(_ViewModel.SelectMngLocKeys) Then
                Dim sbSQLMngLocKey As StringBuilder = New StringBuilder
                Dim selectMngLocKeys As String() = _ViewModel.SelectMngLocKeys.Split(", ")
                For idx As Integer = 0 To selectMngLocKeys.Length - 1
                    If sbSQLMngLocKey.Length <> 0 Then
                        sbSQLMngLocKey.Append(", ")
                    End If
                    sbSQLMngLocKey.Append("@MngLocLCd" & idx)
                    params.Add(New SqlParameter($"MngLocLCd" & idx, selectMngLocKeys(idx)))
                Next
                sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD IN (" & sbSQLMngLocKey.ToString & ")")
            End If

            sbSQL.AppendLine("ORDER BY")
            sbSQL.AppendLine("  MNG_LOC.LOC_SHARE_FLG DESC")
            sbSQL.AppendLine("  , ASSET.MNG_LOC_L_CD")
            sbSQL.AppendLine("  , ASSET.CAT_L_CD")
            sbSQL.AppendLine("  , ASSET.CAT_M_CD")
            sbSQL.AppendLine("  , ASSET.CAT_S_CD")
            sbSQL.AppendLine("  , ASSET.ASSET_NO")

            Dim lstAssetLeaseHitory As List(Of AssetLeaseHitoryViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstAssetLeaseHitory = db.AssetLeaseHitoryView.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            'データを返却
            Return lstAssetLeaseHitory
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

#Region "全選択ボタン押下時処理"
        ''' <summary>
        ''' 全選択ボタン押下時処理
        ''' </summary>
        ''' <param name="isSelectAll"></param>
        ''' <returns></returns>
        Public Function SelectAll(ByVal isSelectAll As Boolean) As ActionResult
            If Not isSelectAll Then
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                Return Json("")
            Else
                '全選択ボタン押下フラグをセットする
                Session(CommonSessionKey.ALL_SELECT_FLG) = True
                Return Json("")
            End If
        End Function
#End Region

#Region "管理ロケーションリストデータ取得"
        ''' <summary>
        ''' 管理ロケーションリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetMngLocData() As List(Of LocationDropdownModel)
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLeaseHitory, _LoginInfo)

            Dim tbl As DataTable = Dao.ToolLocation.GetMngLocationLData(_LoginInfo.EmployeeCd, authInfo, False, Misc.UpdAuthType.UPD_AUTH_HUB)
            Dim lst As New List(Of LocationDropdownModel)

            For Each row As DataRow In tbl.Rows
                lst.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            Return lst
        End Function
#End Region

#Region "管理ロケーションの選択キーリストの初期値を取得します"
        ''' <summary>
        ''' 管理ロケーションの選択キーリストの初期値を取得します
        ''' </summary>
        Private Function GetDefaultSelectMngLocKeys() As String
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.AssetLeaseHitory, _LoginInfo)

            Dim tbl As DataTable = Dao.ToolLocation.GetMngLocationLData(_LoginInfo.EmployeeCd, authInfo, False, Misc.UpdAuthType.UPD_AUTH_HUB)
            Dim lst As New List(Of String)
            For Each row As DataRow In tbl.Rows
                If row("SHARED_FLG").ToString <> SharedFlg.Other Then
                    lst.Add(row("LOC_CD").ToString)
                End If
            Next

            'カンマ区切りのデータを設定
            Return String.Join(",", lst.ToArray())
        End Function
#End Region

#Region "分類区分ドロップダウン変更処理"
        ''' <summary>
        ''' 分類区分ドロップダウン変更処理
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

#Region "大分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 大分類ドロップダウンリストデータ取得
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

            ViewBag.modelCatL = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatL)
            _ViewModel.catLDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "中分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 中分類ドロップダウンリストデータ取得
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

            ViewBag.modelCatM = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatM)
            _ViewModel.catMDropdownData = lst

            Return Json(lst)
        End Function
#End Region

#Region "小分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 小分類ドロップダウンリストデータ取得
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

            ViewBag.modelCatS = New SelectList(lst, "CAT_CD", "CAT_NAME", _ViewModel.SearchCatS)
            _ViewModel.catSDropdownData = lst

            Return Json(lst)
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

            params.Add(New SqlParameter($"@DISP_ID", DispId.ASSET_LEASE_HITORY))

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
                _viewResult.Add(New AssetLeaseHitoryViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchTargetPeriodFrom = _ViewModel.SearchTargetPeriodFrom
            _viewResult.ElementAt(0).SearchTargetPeriodTo = _ViewModel.SearchTargetPeriodTo
            _viewResult.ElementAt(0).SearchCatL = _ViewModel.SearchCatL
            _viewResult.ElementAt(0).SearchCatM = _ViewModel.SearchCatM
            _viewResult.ElementAt(0).SearchCatS = _ViewModel.SearchCatS
            _viewResult.ElementAt(0).mngLocCheckBoxListData = _ViewModel.mngLocCheckBoxListData
            _viewResult.ElementAt(0).SelectMngLocKeys = _ViewModel.SelectMngLocKeys
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.ASSET_LEASE_HITORY).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfo = GetGridInfo()
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
        End Sub
#End Region

    End Class
End Namespace
