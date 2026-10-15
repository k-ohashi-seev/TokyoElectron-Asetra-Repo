Imports Asetra.Models
Imports Asetra.Misc
Imports AsetraLibWebApi
Imports System.Data.SqlClient

Namespace Controllers

    ''' <summary>
    ''' 予約申請登録/更新画面表示用のクラス
    ''' </summary>
    Public Class LendingEntryController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)


#Region "クラス定数"
        Public Const MIN_DATE As Integer = 10000101
#End Region

#Region "クラス変数"
        Private _Model As LendingEntryModel = New LendingEntryModel                                 'LendingEntryモデル
        Private _ViewModel As LendingEntryViewModel = New LendingEntryViewModel                     'LendingEntry一覧表示用モデル
        Protected _viewResult As List(Of LendingEntryViewModel) = New List(Of LendingEntryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報

        '2021/11/10 add nv Inoue
        'DB関数 資産メンテナンス登録情報取得　の戻り値のDataset  CreateChcek() でNew 
        Private _dsMentInfo As DataSet

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
        Public Function Index(ByVal ViewModel As LendingEntryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_LENDING_ENTRY)

                'チェックボックス状態の選択状態をクリアする
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
            End If

            'メッセージ一覧取得  2022/7/25 sv sato add
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel = ViewModel

                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '一覧表示処理

                '権限区分
                ViewBag.AuthDiv = _LoginInfo.KengenKbn

                '予約申請登録/変更画面への引渡し情報をセッションから取得
                Dim SeniInfo As LendingEntryInfo = Session(CommonSessionKey.LENDING_ENTRY_INFO)

                'モデルを設定(セッションにある場合はセッションから設定)
                Dim model As LendingEntryViewModel = Session(CommonSessionKey.CREAGE_LENDING_ENTRY)
                If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                    _ViewModel = model

                    'グリッドの情報を取得
                    Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)
                    _ViewModel.ReserveDetailData = lstGrid

                    '貸出・返却が正常終了した時の場合
                    If _ViewModel.States = StatusType.SuccessLending OrElse _ViewModel.States = StatusType.SuccessReturned Then
                        '画面を再表示する。
                        '一覧表示のSQLを呼ぶ。
                        MyBase.BaseIndex(_ViewModel)

                        ''ステータスを再設定する。
                        '_viewResult(0).States = _ViewModel.States
                    Else
                        '画面情報を設定
                        _viewResult.Add(_ViewModel)
                    End If

                Else
                    _ViewModel = ViewModel
                    _ViewModel.SenimotoKbn = SeniInfo.SenimotoKbn

                    If SeniInfo.DispKbn = DispKbn.Create Then
                        _ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK
                        _ViewModel.RESERVE_ID = 0
                    Else
                        _ViewModel.OnCreteButton = CommonConst.KEY_ON_UPDATE_CLICK
                        _ViewModel.RESERVE_ID = SeniInfo.ReserveId
                    End If

                    '初期表示時のみ、一覧表示のSQLを呼ぶ。
                    MyBase.BaseIndex(_ViewModel)
                End If

                ''2021/12/20 nv miyaji AuthProcFlgは初期表示時のみ設定するように修正。ViewExecQuery関数に移動。
                ''画面更新権限設定
                'If SeniInfo.DispKbn = DispKbn.Create Then
                '    '全て更新可能
                '    _viewResult(0).AuthProcFlg = AuthProcFlg.All

                'ElseIf SeniInfo.DispKbn = DispKbn.Edit Then
                '    Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '    '予約変更可能日数を取得する。
                '    Dim Setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Update_Enabled.KeyDiv, CommonSettingKey.Lending_Update_Enabled.Key1, CommonSettingKey.Lending_Update_Enabled.Key2, CommonSettingKey.Lending_Update_Enabled.Key3, True)
                '    Dim sUpdateOkDays As Integer = Setting.DATA1
                '    Dim sWorkflg As String = Setting.DATA2

                '    '対象日付取得(予約変更可能日付)
                '    Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, sUpdateOkDays * -1, sWorkflg)   '2021/11/22 nv miyaji 稼働日フラグを設定マスタから取るように修正
                '    'Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, sUpdateOkDays * -1, WorkFlg.Jitsubi)   '2021/11/22 nv miyaji 稼働日フラグを設定マスタから取るように修正

                '    If _viewResult(0).REAL_START_DT < iTargetDate Then
                '        '--2021/12/01 nv miyaji update start
                '        '一部更新可能を設定
                '        _viewResult(0).AuthProcFlg = AuthProcFlg.JiAll

                '        '権限情報の取得
                '        Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)
                '        '特殊権限が存在するかどうかを取得
                '        Dim isExistAuthOption_Self As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_SELF)
                '        Dim isExistAuthOption_Hub As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_HUB)
                '        Dim isExistAuthOption_Admin As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_ADMIN)
                '        'ログイン社員の所属拠点データ取得
                '        Dim tblEmpHub As DataTable = Dao.ToolEmployee.GetHubLocData(_LoginInfo.EmployeeCd)
                '        If isExistAuthOption_Admin Then
                '            'U_ADMINの場合
                '            '全て更新可能
                '            _viewResult(0).AuthProcFlg = AuthProcFlg.All
                '        ElseIf isExistAuthOption_Hub OrElse isExistAuthOption_Self Then
                '            'U_HUB、U_SELFの場合
                '            '担当者以外で自拠点の場合、全て更新可能。
                '            If _LoginInfo.KengenKbn <> AUTH_DIV.Nomal Then
                '                Dim IsJiKyoten As Boolean = False        '自拠点か否か
                '                For Each rowEmpHub As DataRow In tblEmpHub.Rows
                '                    If _viewResult(0).MNG_LOC_L_CD = rowEmpHub("HUB_LOC_L_CD").ToString Then
                '                        IsJiKyoten = True
                '                        Exit For
                '                    End If
                '                Next
                '                '自拠点だった場合
                '                If IsJiKyoten Then
                '                    '全て更新可能
                '                    _viewResult(0).AuthProcFlg = AuthProcFlg.All
                '                End If
                '            End If
                '        End If
                '        ''権限区分が担当者の場合、削除ボタン・一覧削除ボタン・一覧追加ボタンを非表示にする。
                '        'If _LoginInfo.KengenKbn = AUTH_DIV.Nomal Then
                '        '    ViewBag.CantEdit = True
                '        'End If
                '        ''権限情報の取得
                '        ''更新制限有り
                '        'If _LoginInfo.ShosaiAuthDiv = SHOSAI_AUTH_DIV.Tsujo_Nomal _
                '        '            OrElse _LoginInfo.ShosaiAuthDiv = SHOSAI_AUTH_DIV.Kyoten_Nomal _
                '        '            OrElse _LoginInfo.ShosaiAuthDiv = SHOSAI_AUTH_DIV.Honbu_Nomal Then
                '        '    '担当者、拠点担当者、本部担当者の場合
                '        '    '一部更新制限有り
                '        '    _viewResult(0).AuthProcFlg = AuthProcFlg.JiAll
                '        'Else
                '        '    '全て更新可能
                '        '    _viewResult(0).AuthProcFlg = AuthProcFlg.All
                '        'End If
                '        '--2021/12/01 nv miyaji update end
                '    Else
                '        '全て更新可能
                '        _viewResult(0).AuthProcFlg = AuthProcFlg.All
                '    End If

                '    '2021/12/20 nv miyaji 下に移動
                '    ''全て更新可能の場合、明細に未貸出以外があれば実貸出開始日を非活性にする。（照会のみ、一部更新可能の場合、ビュー側で非活性にしている）
                '    'If _viewResult(0).AuthProcFlg = AuthProcFlg.All Then
                '    '    '実貸出開始日を非活性にするフラグ
                '    '    '既存のデータベースから該当資産情報を取得
                '    '    Dim lstModelSelDetail As ReserveDetailViewModel() = (From motoOld In _ViewModel.ReserveDetailData
                '    '                                                         Where (motoOld.RESERVE_STATUS <> ReserveStatus.Mi)
                '    '                                                         Order By motoOld.ASSET_NO).ToArray
                '    '    If lstModelSelDetail.Length > 0 Then
                '    '        _ViewModel.isNotEnableRealStartDt = True
                '    '    End If
                '    'Else
                '    '    '削除ボタン・一覧削除・資産追加ボタンを非表示にする。
                '    '    ViewBag.CantEdit = True
                '    'End If

                'Else
                '    '照会のみ
                '    _viewResult(0).AuthProcFlg = AuthProcFlg.ShokaiNomi
                'End If

                '削除ボタン・一覧削除・資産追加ボタンを非表示にするフラグ
                ViewBag.CantEdit = False
                '全て更新可能の場合、明細に未貸出以外があれば実貸出開始日を非活性にする。（照会のみ、一部更新可能の場合、ビュー側で非活性にしている）
                If _viewResult(0).AuthProcFlg = AuthProcFlg.All Then
                    '実貸出開始日を非活性にするフラグ
                    '既存のデータベースから該当資産情報を取得
                    Dim lstModelSelDetail As ReserveDetailViewModel() = (From motoOld In _ViewModel.ReserveDetailData
                                                                         Where (motoOld.RESERVE_STATUS <> ReserveStatus.Mi)
                                                                         Order By motoOld.ASSET_NO).ToArray
                    If lstModelSelDetail.Length > 0 Then
                        _ViewModel.isNotEnableRealStartDt = True
                    End If
                Else
                    '削除ボタン・一覧削除・資産追加ボタンを非表示にする。
                    ViewBag.CantEdit = True
                End If

                '一覧画面からの遷移の場合
                ViewBag.KashidashiBtnEnable = True  '貸出ボタンを活性化
                ViewBag.HenkyakuBtnEnable = True  '返却ボタンを活性化
                ViewBag.UpdateBtnEnable = True  '変更ボタンを活性化
                If SeniInfo.SenimotoKbn = SenimotoKbn.LendingList Then
                    If SeniInfo.IsDispKashiHenBtn = False Then
                        '貸出・返却権限がない場合
                        '貸出ボタンを非活性
                        ViewBag.KashidashiBtnEnable = False
                        '返却ボタンを非活性
                        ViewBag.HenkyakuBtnEnable = False
                        '変更ボタンを非活性
                        ViewBag.UpdateBtnEnable = False
                    Else
                        '既存のデータベースから該当予約明細情報を取得
                        Dim lstModelMi As ReserveDetailViewModel() = (From modelMi In _ViewModel.ReserveDetailData
                                                                      Where (modelMi.RESERVE_STATUS = ReserveStatus.Mi)
                                                                      Order By modelMi.ASSET_NO).ToArray
                        '明細のステータスが「未貸出」のデータがない場合
                        If lstModelMi.Length = 0 Then
                            '貸出ボタンを非活性
                            ViewBag.KashidashiBtnEnable = False
                        End If
                        '既存のデータベースから該当予約明細情報を取得
                        Dim lstModelLending As ReserveDetailViewModel() = (From modelLending In _ViewModel.ReserveDetailData
                                                                           Where (modelLending.RESERVE_STATUS = ReserveStatus.Lending)
                                                                           Order By modelLending.ASSET_NO).ToArray
                        '明細のステータスが「貸出中」のデータがない場合
                        If lstModelLending.Length = 0 Then
                            '返却ボタンを非活性
                            ViewBag.HenkyakuBtnEnable = False
                        End If
                    End If

                End If

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo

                '2022/7/25 sv sato add start
                '設定マスタから予約番号の入力可否フラグを取得
                If GetInputEnableReserveId() AndAlso _ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK Then
                    ViewBag.CanDspOptionData1 = True
                Else
                    ViewBag.CanDspOptionData1 = False
                End If
                '2022/7/25 sv sato add end
            End If

            '貸出担当者
            GetEmployeeData()

            '送り先(貸出先)
            GetLeasePlaceData()

            '返送先
            GetRetPlaceData()    '2023/03/22 Kawamoto ADD

            '貸出用途
            GetLeaseStatusData()

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.LENDING_ENTRY)
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

            ''エラーでない場合、セッションクリア
            'If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
            '    Session.Remove(CommonSessionKey.CREAGE_LENDING_ENTRY)
            'End If

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
            'sbSQL.Append("  M_DEPARTMENT ")
            'sbSQL.Append("WHERE ")
            'sbSQL.Append("  1 = 1 ")

            ''検索条件を設定する
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

            '画面に表示するViewを設定
            If _ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK Then
                '新規登録時
                'セッションのカゴ情報を取得する
                Dim lstCartInfo As New List(Of CartInfo)
                If Not IsNothing(Session(CommonSessionKey.CREAGE_CART_INFO)) Then
                    lstCartInfo = Session(CommonSessionKey.CREAGE_CART_INFO)
                End If

                'グリッド情報
                Dim lstReserveDetail As List(Of ReserveDetailViewModel) = New List(Of ReserveDetailViewModel)
                For Each row As CartInfo In lstCartInfo
                    'IDをキーに資産モデルを取得する
                    Dim modelAsset As AssetListModel = db.AssetList.Find(row.AssetNo)

                    Dim rowData As New ReserveDetailViewModel
                    rowData.RESERVE_ID = 0
                    rowData.ASSET_NO = row.AssetNo
                    If row.AssetName Is Nothing Then
                        rowData.AssetName = String.Empty
                    Else
                        rowData.AssetName = row.AssetName
                    End If
                    rowData.REAL_START_DT = 0
                    rowData.REAL_END_DT = 0
                    rowData.CHARGE_EMP_CD = String.Empty
                    rowData.LENDING_DT = Nothing
                    rowData.RETURN_DT = Nothing
                    rowData.PR_ST_DT = 0
                    rowData.PR_END_DT = 0
                    rowData.LEASE_CHG_TIMES = 0
                    rowData.HANDOVER_DIV = HandOverDiv.Mi
                    rowData.HANDOVER_RESERVE_ID = Nothing
                    rowData.RESERVE_STATUS = ReserveStatus.Mi
                    If modelAsset.FREE_ITEM10 Is Nothing Then
                        rowData.SerialNo = String.Empty
                    Else
                        rowData.SerialNo = modelAsset.FREE_ITEM10
                    End If
                    rowData.LeaseKigen = String.Empty
                    rowData.EmployeeName = String.Empty
                    lstReserveDetail.Add(rowData)

                Next

                'ビューモデルにグリッド情報を設定
                _ViewModel.ReserveDetailData = lstReserveDetail

                'グリッド情報をセッションに保持
                Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO) = lstReserveDetail

                'ビューモデルに管理ロケーションを設定
                _ViewModel.MNG_LOC_L_CD = lstCartInfo(0).MngLocLCd
                _ViewModel.LocLName = lstCartInfo(0).MngLocLName

                '_viewResult.Add(_ViewModel)    '2021/12/20 nv miyaji 関数の最後に移動

            Else
                '変更時
                'IDをキーに予約モデルを取得する
                Dim modelEntry As LendingEntryModel = db.LendingEntry.Find(_ViewModel.RESERVE_ID)
                'モデルをビューモデルにコピー
                CopyModeltoViewModel(modelEntry)

                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationL, modelEntry.MNG_LOC_L_CD, " ", " "}
                Dim modelLoc As LocationModel = db.Location.Find(findKeys)
                '管理ロケーション名をセット
                _ViewModel.LocLName = modelLoc.LOC_NAME

                '予約明細テーブルを取得
                Dim sbSQL As StringBuilder = New StringBuilder
                Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2021/12/07 nv miyaji add
                'T_RESERVE_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT RESERVE.*")
                sbSQL.AppendLine("       ,ISNULL(ASSET.ASSET_NAME,'') As AssetName")
                sbSQL.AppendLine("       ,ISNULL(ASSET.FREE_ITEM10,'') As SerialNo")
                sbSQL.AppendLine("       ,ISNULL(EMP.EMPLOYEE_NAME,'') As EmployeeName")
                sbSQL.AppendLine("FROM T_RESERVE_DETAIL RESERVE")
                sbSQL.AppendLine("INNER JOIN M_ASSET ASSET")
                sbSQL.AppendLine("ON ASSET.ASSET_NO = RESERVE.ASSET_NO ")
                sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE EMP")
                sbSQL.AppendLine("ON EMP.EMPLOYEE_CD = RESERVE.CHARGE_EMP_CD ")
                sbSQL.AppendLine("WHERE")
                '--2021/12/07 nv miyaji update start
                sbSQL.AppendLine("RESERVE.RESERVE_ID = @ReserveId")
                params.Add(New SqlParameter("@ReserveId", _ViewModel.RESERVE_ID))
                'sbSQL.AppendLine("RESERVE.RESERVE_ID = '" & _ViewModel.RESERVE_ID & "'")
                '--2021/12/07 nv miyaji update end
                sbSQL.AppendLine("ORDER BY RESERVE.ASSET_NO")
                'SQLを実行する
                Dim lstReserveDetail As List(Of ReserveDetailViewModel) = db.ReserveDetailView.SqlQuery(sbSQL.ToString, params.ToArray).ToList  '2021/12/07 nv miyaji
                'Dim lstReserveDetail As List(Of ReserveDetailViewModel) = db.ReserveDetailView.SqlQuery(sbSQL.ToString).ToList  '2021/12/07 nv miyaji

                'メッセージ一覧取得
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '2021/11/11 upd nv miyaji start
                ''期間延長回数取得
                'Dim setting_KikanEnchoSuu As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_ChangedTimes.KeyDiv, CommonSettingKey.Lending_ChangedTimes.Key1, CommonSettingKey.Lending_ChangedTimes.Key2, CommonSettingKey.Lending_ChangedTimes.Key3, True)
                'Dim sKikanEnchoSuu As String = setting_KikanEnchoSuu.DATA1
                ''返却リードタイム
                'Dim setting_HenkyakuLT As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Return_LeadTime.KeyDiv, CommonSettingKey.Return_LeadTime.Key1, CommonSettingKey.Return_LeadTime.Key2, CommonSettingKey.Return_LeadTime.Key3, True)
                '稼働日考慮フラグ
                Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
                Dim sWorkDayFlg As String = reservationFunction.DATA2
                '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                Dim sWorkFlg As String = WorkFlg.Jitsubi
                If sWorkDayFlg = WorkDayFlg.Yes Then
                    sWorkFlg = WorkFlg.Kadobi
                End If
                'Dim iHenkyakuLT As Integer = CInt(setting_KikanEnchoSuu.DATA1)
                ''稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                'Dim sWorkFlg As String = WorkFlg.Jitsubi
                'If setting_KikanEnchoSuu.DATA2 = WorkDayFlg.Yes Then
                '    sWorkFlg = WorkFlg.Kadobi
                'End If
                '2021/11/11 upd nv miyaji end 


                '実貸出開始日文字列、実貸出終了日文字列、期限日を設定する。
                For Each row As ReserveDetailViewModel In lstReserveDetail
                    '期限日
                    row.LeaseKigen = CommonConst.MAX_KIGENBI_STR
                    If row.REAL_START_DT <> 0 AndAlso row.REAL_END_DT <> 0 Then
                        '開始日と終了日に両方値がある場合、資産メンテナンス登録情報取得
                        Dim tblInfo As DataTable = Dao.ToolReserve.GetReserveMainteInfo(row.ASSET_NO, row.RESERVE_ID, row.REAL_START_DT, row.REAL_END_DT, sWorkFlg)
                        If tblInfo IsNot Nothing AndAlso tblInfo.Rows.Count > 0 AndAlso tblInfo.Rows(0).Item("LIMIT_DT") <> CommonConst.MAX_KIGENBI AndAlso tblInfo.Rows(0).Item("LIMIT_DT") >= row.REAL_START_DT Then
                            row.LeaseKigen = CommonRfid.ToStringDateYMD(tblInfo.Rows(0).Item("LIMIT_DT"), "yyyyMMdd")
                        End If
                    End If
                    '実貸出開始日文字列
                    If row.REAL_START_DT <> 0 Then
                        row.RealStartDtString = CommonRfid.ToStringDateYMD(row.REAL_START_DT, "yyyyMMdd")
                    End If
                    '実貸出終了日文字列
                    If row.REAL_END_DT <> 0 Then
                        row.RealEndDtString = CommonRfid.ToStringDateYMD(row.REAL_END_DT, "yyyyMMdd")
                    End If
                Next

                'ビューモデルにグリッド情報を設定
                _ViewModel.ReserveDetailData = lstReserveDetail

                'グリッド情報をセッションに保持
                Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO) = _ViewModel.ReserveDetailData

                '_viewResult.Add(_ViewModel)    '2021/12/20 nv miyaji 関数の最後に移動
            End If

            '2021/12/20 nv miyaji AuthProcFlgは初期表示時のみ設定する。
            '予約申請登録/変更画面への引渡し情報をセッションから取得
            Dim SeniInfo As LendingEntryInfo = Session(CommonSessionKey.LENDING_ENTRY_INFO)
            If SeniInfo.DispKbn = DispKbn.Create Then
                '全て更新可能
                _ViewModel.AuthProcFlg = AuthProcFlg.All

            ElseIf SeniInfo.DispKbn = DispKbn.Edit Then
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

                '予約変更可能日数を取得する。
                Dim Setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Update_Enabled.KeyDiv, CommonSettingKey.Lending_Update_Enabled.Key1, CommonSettingKey.Lending_Update_Enabled.Key2, CommonSettingKey.Lending_Update_Enabled.Key3, True)
                Dim sUpdateOkDays As Integer = Setting.DATA1
                Dim sWorkflg As String = Setting.DATA2

                '対象日付取得(予約変更可能日付)
                Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, sUpdateOkDays * -1, sWorkflg)   '2021/11/22 nv miyaji 稼働日フラグを設定マスタから取るように修正
                'Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, sUpdateOkDays * -1, WorkFlg.Jitsubi)   '2021/11/22 nv miyaji 稼働日フラグを設定マスタから取るように修正

                If _ViewModel.REAL_START_DT < iTargetDate Then
                    '--2021/12/01 nv miyaji update start
                    '一部更新可能を設定
                    _ViewModel.AuthProcFlg = AuthProcFlg.JiAll

                    '権限情報の取得
                    Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)
                    '特殊権限が存在するかどうかを取得
                    Dim isExistAuthOption_Self As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_SELF)
                    Dim isExistAuthOption_Hub As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_HUB)
                    Dim isExistAuthOption_Admin As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_ADMIN)
                    'ログイン社員の所属拠点データ取得
                    Dim tblEmpHub As DataTable = Dao.ToolEmployee.GetHubLocData(_LoginInfo.EmployeeCd)
                    If isExistAuthOption_Admin Then
                        'U_ADMINの場合
                        '全て更新可能
                        _ViewModel.AuthProcFlg = AuthProcFlg.All
                    ElseIf isExistAuthOption_Hub OrElse isExistAuthOption_Self Then
                        'U_HUB、U_SELFの場合
                        '担当者以外で自拠点の場合、全て更新可能。
                        If _LoginInfo.KengenKbn <> AUTH_DIV.Nomal Then
                            Dim IsJiKyoten As Boolean = False        '自拠点か否か
                            For Each rowEmpHub As DataRow In tblEmpHub.Rows
                                If _ViewModel.MNG_LOC_L_CD = rowEmpHub("HUB_LOC_L_CD").ToString Then
                                    IsJiKyoten = True
                                    Exit For
                                End If
                            Next
                            '自拠点だった場合
                            If IsJiKyoten Then
                                '全て更新可能
                                _ViewModel.AuthProcFlg = AuthProcFlg.All
                            End If
                        End If
                    End If
                    ''権限区分が担当者の場合、削除ボタン・一覧削除ボタン・一覧追加ボタンを非表示にする。
                    'If _LoginInfo.KengenKbn = AUTH_DIV.Nomal Then
                    '    ViewBag.CantEdit = True
                    'End If
                    ''権限情報の取得
                    ''更新制限有り
                    'If _LoginInfo.ShosaiAuthDiv = SHOSAI_AUTH_DIV.Tsujo_Nomal _
                    '            OrElse _LoginInfo.ShosaiAuthDiv = SHOSAI_AUTH_DIV.Kyoten_Nomal _
                    '            OrElse _LoginInfo.ShosaiAuthDiv = SHOSAI_AUTH_DIV.Honbu_Nomal Then
                    '    '担当者、拠点担当者、本部担当者の場合
                    '    '一部更新制限有り
                    '    _viewResult(0).AuthProcFlg = AuthProcFlg.JiAll
                    'Else
                    '    '全て更新可能
                    '    _viewResult(0).AuthProcFlg = AuthProcFlg.All
                    'End If
                    '--2021/12/01 nv miyaji update end
                Else
                    '全て更新可能
                    _ViewModel.AuthProcFlg = AuthProcFlg.All
                End If

                '2021/12/20 nv miyaji このIF文の外でチェックするように修正。
                ''全て更新可能の場合、明細に未貸出以外があれば実貸出開始日を非活性にする。（照会のみ、一部更新可能の場合、ビュー側で非活性にしている）
                'If _viewResult(0).AuthProcFlg = AuthProcFlg.All Then
                '    '実貸出開始日を非活性にするフラグ
                '    '既存のデータベースから該当資産情報を取得
                '    Dim lstModelSelDetail As ReserveDetailViewModel() = (From motoOld In _ViewModel.ReserveDetailData
                '                                                         Where (motoOld.RESERVE_STATUS <> ReserveStatus.Mi)
                '                                                         Order By motoOld.ASSET_NO).ToArray
                '    If lstModelSelDetail.Length > 0 Then
                '        _ViewModel.isNotEnableRealStartDt = True
                '    End If
                'Else
                '    '削除ボタン・一覧削除・資産追加ボタンを非表示にする。
                '    ViewBag.CantEdit = True
                'End If

            Else
                '照会のみ
                _ViewModel.AuthProcFlg = AuthProcFlg.ShokaiNomi
            End If

            _viewResult.Add(_ViewModel)

            'SQLを実行
            'Dim query As DbSqlQuery(Of LendingEntryViewModel) = _db.LendingEntryView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function Create(ByVal ViewModel As LendingEntryViewModel, ByVal OnCreteButton As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '変更処理の場合、データベースのモデルを取得
            Dim modelOld As New LendingEntryModel
            Dim lstReserveDetailOld As New List(Of ReserveDetailViewModel)
            Dim db As New SystemDBContext
            If _ViewModel.OnCreteButton = CommonConst.KEY_ON_UPDATE_CLICK Then
                '予約テーブルを取得
                modelOld = db.LendingEntry.Find(_ViewModel.RESERVE_ID)

                '予約明細テーブルを取得
                Dim sbSQL As StringBuilder = New StringBuilder
                Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2021/12/07 nv miyaji add
                'T_RESERVE_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT RESERVE.*")
                sbSQL.AppendLine("       ,ASSET.ASSET_NAME As AssetName")
                sbSQL.AppendLine("       ,ASSET.FREE_ITEM10 As SerialNo")
                sbSQL.AppendLine("       ,EMP.EMPLOYEE_NAME As EmployeeName")
                sbSQL.AppendLine("FROM T_RESERVE_DETAIL RESERVE")
                sbSQL.AppendLine("INNER JOIN M_ASSET ASSET")
                sbSQL.AppendLine("ON ASSET.ASSET_NO = RESERVE.ASSET_NO ")
                sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE EMP")
                sbSQL.AppendLine("ON EMP.EMPLOYEE_CD = RESERVE.CHARGE_EMP_CD ")
                sbSQL.AppendLine("WHERE")
                '--2021/12/07 nv miyaji update start
                sbSQL.AppendLine("RESERVE.RESERVE_ID = @ReserveId")
                params.Add(New SqlParameter("@ReserveId", _ViewModel.RESERVE_ID))
                'sbSQL.AppendLine("RESERVE.RESERVE_ID = '" & _ViewModel.RESERVE_ID & "'")
                '--2021/12/07 nv miyaji update end
                sbSQL.AppendLine("ORDER BY RESERVE.ASSET_NO")
                'SQLを実行する
                lstReserveDetailOld = db.ReserveDetailView.SqlQuery(sbSQL.ToString, params.ToArray).ToList  '2021/12/07 nv miyaji
                'lstReserveDetailOld = db.ReserveDetailView.SqlQuery(sbSQL.ToString).ToList '2021/12/07 nv miyaji
            End If


            '実貸出開始日・実貸出終了日、ユーザー貸出開始日・ユーザー貸出貸出終了日、○○期限、申請日を設定する
            If String.IsNullOrEmpty(_ViewModel.RealStartDtString) Then
                _ViewModel.REAL_START_DT = 0
            Else
                _ViewModel.REAL_START_DT = CInt(CDate(_ViewModel.RealStartDtString).ToString("yyyyMMdd"))
            End If
            If String.IsNullOrEmpty(_ViewModel.RealEndDtString) Then
                _ViewModel.REAL_END_DT = 0
            Else
                _ViewModel.REAL_END_DT = CInt(CDate(_ViewModel.RealEndDtString).ToString("yyyyMMdd"))
            End If
            If String.IsNullOrEmpty(_ViewModel.UserStartDtString) Then
                _ViewModel.USER_START_DT = 0
            Else
                _ViewModel.USER_START_DT = CInt(CDate(_ViewModel.UserStartDtString).ToString("yyyyMMdd"))
            End If
            If String.IsNullOrEmpty(_ViewModel.UserEndDtString) Then
                _ViewModel.USER_END_DT = 0
            Else
                _ViewModel.USER_END_DT = CInt(CDate(_ViewModel.UserEndDtString).ToString("yyyyMMdd"))
            End If
            _ViewModel.REQUEST_DT = CInt(CDate(CommonRfid.getNowDate).ToString("yyyyMMdd"))

            '登録・更新内容のログ出力  '2022/05/25 nv miyaji add
            Dim sEntryType As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU)
            If Not ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK Then
                sEntryType = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU)
            End If
            For icnt As Integer = 0 To lstGrid.Count - 1
                Dim sLogEntry As String = sEntryType & " " &
                             "申請者コード：" & _ViewModel.REQ_EMP_CD & " " &
                             "ヘッダ実貸出開始日：" & _ViewModel.REAL_START_DT & " " &
                             "ヘッダ実貸出終了日：" & _ViewModel.REAL_END_DT & " " &
                             "資産番号：" & lstGrid(icnt).ASSET_NO & " " &
                             "資産名：" & lstGrid(icnt).AssetName & " " &
                             "実貸出開始日：" & lstGrid(icnt).REAL_START_DT & " " &
                             "実貸出終了日：" & lstGrid(icnt).REAL_END_DT & " " &
                             Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
                             CommonRfid.ToStringNvl(_ViewModel.RESERVE_ID).Trim
                logger.Info(sLogEntry)
            Next

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理
                MyBase.BaseCreate(_ViewModel)
            Else
                '更新処理
                MyBase.BaseEdit(_ViewModel)
            End If

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                Dim isMailEncho As Boolean = False              '期間延長メール送信チェックのためのフラグ 2021/11/06 add nv Inoue
                Dim isMailChangeEmp As Boolean = False          '引継メール送信チェックのためのフラグ 2021/11/07 add nv miyaji
                Dim sTaskNm As String

                '更新した予約IDを取得
                Dim lReserveId As Long = _Model.RESERVE_ID


                If _ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK Then
                    '新規登録時
                    sTaskNm = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU)
                Else
                    '変更時
                    sTaskNm = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU)
                End If


                '予約明細テーブルの削除
                '--2021/12/07 nv miyaji update start
                Dim sbSQL_DelDetail As StringBuilder = New StringBuilder
                Dim params_DelDetail As List(Of SqlParameter) = New List(Of SqlParameter)
                sbSQL_DelDetail.AppendLine("DELETE FROM T_RESERVE_DETAIL WHERE RESERVE_ID = @ReserveId")
                params_DelDetail.Add(New SqlParameter("@ReserveId", lReserveId))
                _db.Database.ExecuteSqlCommand(sbSQL_DelDetail.ToString, params_DelDetail.ToArray)
                '_db.Database.ExecuteSqlCommand("DELETE FROM T_RESERVE_DETAIL WHERE RESERVE_ID = '" & lReserveId & "'")
                '--2021/12/07 nv miyaji update end

                '処理済引継先予約Noのリスト（予約を複数回更新しようとするとエラーになるため）
                Dim lstFinReserveId As New List(Of Long)

                ''グリッドの情報を取得    '2022/05/25 nv miyaji ログ出しに使うので、上に移動
                'Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)
                Dim model As List(Of ReserveDetailModel) = New List(Of ReserveDetailModel)    '登録用モデル

                '登録用モデルにデータを入れる
                For icnt As Integer = 0 To lstGrid.Count - 1
                    model.Add(New ReserveDetailModel())
                    model(icnt).RESERVE_ID = lReserveId
                    model(icnt).ASSET_NO = lstGrid(icnt).ASSET_NO
                    model(icnt).REAL_START_DT = lstGrid(icnt).REAL_START_DT
                    model(icnt).REAL_END_DT = lstGrid(icnt).REAL_END_DT
                    model(icnt).HANDOVER_DIV = lstGrid(icnt).HANDOVER_DIV
                    model(icnt).HANDOVER_RESERVE_ID = lstGrid(icnt).HANDOVER_RESERVE_ID
                    '引継元明細の場合
                    If lstGrid(icnt).HANDOVER_DIV = HandOverDiv.HikitsugiMoto Then
                        '前回登録時、該当資産番号が引継元明細だったかどうかをチェック
                        Dim sAssetNo As String = lstGrid(icnt).ASSET_NO
                        '既存のデータベースから該当資産情報を取得
                        Dim lstModelSelDetail As ReserveDetailViewModel() = (From motoOld In lstReserveDetailOld
                                                                             Where (motoOld.ASSET_NO = sAssetNo)
                                                                             Order By motoOld.ASSET_NO).ToArray
                        If lstModelSelDetail(0).HANDOVER_RESERVE_ID Is Nothing Then
                            '前回登録時、該当資産番号が引継明細でなかった場合
                            '予約モデルを取得。
                            'Dim dbNext As New SystemDBContext
                            Dim dbNext As SystemDBContext = _db

                            '2021/12/07 upd nv Inoue start 更新予約をチェックし、実行するように変更
                            If lstFinReserveId.IndexOf(lstGrid(icnt).HANDOVER_RESERVE_ID.Value) = -1 Then
                                Dim modelNextReserve As LendingEntryModel = dbNext.LendingEntry.Find(lstGrid(icnt).HANDOVER_RESERVE_ID.Value)
                                'modelNextReserve.RESERVE_STATUS = ReserveStatus.Lending
                                'アタッチする(予約)　　2021/12/07 upd nv Inoue 予約と予約明細の更新タイミングを分ける
                                _db.Entry(modelNextReserve).State = Entity.EntityState.Modified

                                lstFinReserveId.Add(lstGrid(icnt).HANDOVER_RESERVE_ID.Value)
                            End If
                            '2021/12/07 upd nv Inoue end 更新予約をチェックし、実行するように変更

                            '予約明細モデルを取得。
                            Dim modelNextReserveDetail As ReserveDetailModel = dbNext.ReserveDetail.Find(lstGrid(icnt).HANDOVER_RESERVE_ID, lstGrid(icnt).ASSET_NO, lstGrid(icnt).NextRealStartDt)
                            Dim iNextRealStartDt As Integer = 0
                            If lstGrid.Count - 1 >= icnt + 1 AndAlso lstGrid(icnt + 1).HANDOVER_DIV = HandOverDiv.HikitsugiSaki Then
                                '引継先明細がある場合、引継先明細の実貸出終了日＋１の日付をセット。
                                iNextRealStartDt = CInt(CDate(CommonRfid.ToStringDateYMD(lstGrid(icnt + 1).REAL_END_DT, "yyyyMMdd")).AddDays(1).ToString("yyyyMMdd"))
                            Else
                                '引継先明細がない場合、引継元明細の実貸出終了日＋１の日付をセット。
                                iNextRealStartDt = CInt(CDate(CommonRfid.ToStringDateYMD(lstGrid(icnt).REAL_END_DT, "yyyyMMdd")).AddDays(1).ToString("yyyyMMdd"))
                            End If
                            modelNextReserveDetail.LENDING_DT = iNextRealStartDt
                            modelNextReserveDetail.PR_ST_DT = iNextRealStartDt
                            modelNextReserveDetail.RESERVE_STATUS = ReserveStatus.Lending

                            'アタッチする(予約明細)   2021/12/07 upd nv Inoue 予約と予約明細の更新タイミングを分ける
                            _db.Entry(modelNextReserveDetail).State = Entity.EntityState.Modified

                            '引継元の資産の実貸出終了日から、次の予約の実貸出開始日までの間にあるメンテナンスデータは削除する。
                            '--2021/12/07 nv miyaji update start
                            Dim sbSQL_DelMainte As StringBuilder = New StringBuilder
                            Dim params_DelMainte As List(Of SqlParameter) = New List(Of SqlParameter)
                            sbSQL_DelMainte.AppendLine("DELETE FROM T_RESERVE_MAINTE WHERE ASSET_NO = @AssetNo AND MAINTE_START_DT < @NextRealStartDt AND @RealEndDt < MAINTE_END_DT")
                            params_DelMainte.Add(New SqlParameter("@AssetNo", model(icnt).ASSET_NO))
                            params_DelMainte.Add(New SqlParameter("@NextRealStartDt", lstGrid(icnt).NextRealStartDt))
                            params_DelMainte.Add(New SqlParameter("@RealEndDt", lstGrid(icnt).REAL_END_DT))
                            _db.Database.ExecuteSqlCommand(sbSQL_DelMainte.ToString, params_DelMainte.ToArray)
                            '_db.Database.ExecuteSqlCommand("DELETE FROM T_RESERVE_MAINTE WHERE ASSET_NO = '" & model(icnt).ASSET_NO & "' AND   MAINTE_START_DT < " & lstGrid(icnt).NextRealStartDt & " AND " & lstGrid(icnt).REAL_END_DT & " < MAINTE_END_DT ")

                            '--2021/12/14 nv miyaji add start
                            '引継明細作成時、前予約から引継された明細であれば、前予約の引継先明細の予約ステータスを「返却済」に変更する処理
                            UpdBeforeReserve(Me._db, _Model.RESERVE_ID, lstGrid(icnt).ASSET_NO)
                            '--2021/12/14 nv miyaji add end

                            isMailChangeEmp = True
                        End If
                        'For Each modelDatail As ReserveDetailViewModel In lstReserveDetailOld  '2021/11/08 nv miyaji コメントアウト
                        '    If modelDatail.ASSET_NO = lstGrid(icnt).ASSET_NO Then
                        '        If modelDatail.HANDOVER_RESERVE_ID Is Nothing Then
                        '            '前回登録時、該当資産番号が引継明細でなかった場合
                        '            '予約モデルを取得。
                        '            Dim dbNext As New SystemDBContext
                        '            Dim modelNextReserve As LendingEntryModel = dbNext.LendingEntry.Find(lstGrid(icnt).HANDOVER_RESERVE_ID)
                        '            modelNextReserve.RESERVE_STATUS = ReserveStatus.Lending

                        '            '予約明細モデルを取得。
                        '            Dim modelNextReserveDetail As ReserveDetailModel = dbNext.ReserveDetail.Find(lstGrid(icnt).HANDOVER_RESERVE_ID, lstGrid(icnt).ASSET_NO, lstGrid(icnt).NextRealStartDt)
                        '            Dim iNextRealStartDt As Integer = CInt(CDate(CommonRfid.ToStringDateYMD(lstGrid(icnt).REAL_END_DT, "yyyyMMdd")).AddDays(1).ToString("yyyyMMdd"))
                        '            modelNextReserveDetail.LENDING_DT = iNextRealStartDt
                        '            modelNextReserveDetail.PR_ST_DT = iNextRealStartDt
                        '            modelNextReserveDetail.RESERVE_STATUS = ReserveStatus.Lending

                        '            'アタッチする
                        '            _db.Entry(modelNextReserve).State = Entity.EntityState.Modified
                        '            _db.Entry(modelNextReserveDetail).State = Entity.EntityState.Modified

                        '            '引継元の資産の実貸出終了日から、次の予約の実貸出開始日までの間にあるメンテナンスデータは削除する。
                        '            _db.Database.ExecuteSqlCommand("DELETE FROM T_RESERVE_MAINTE WHERE ASSET_NO = '" & model(icnt).ASSET_NO & "' AND   MAINTE_START_DT < " & lstGrid(icnt).NextRealStartDt & " AND " & lstGrid(icnt - 1).REAL_END_DT & " < MAINTE_END_DT ")

                        '            isMailChangeEmp = True

                        '        End If

                        '        Exit For
                        '    End If
                        'Next

                    End If

                    '引継先明細の場合
                    If lstGrid(icnt).HANDOVER_DIV = HandOverDiv.HikitsugiSaki Then
                        'グリッドの担当者をセットする。
                        model(icnt).CHARGE_EMP_CD = lstGrid(icnt).CHARGE_EMP_CD
                    Else
                        '担当者コンボの担当者をセットする。
                        model(icnt).CHARGE_EMP_CD = _Model.CHARGE_EMP_CD
                    End If
                    model(icnt).UPD_TASK = sTaskNm

                    model(icnt).LENDING_DT = lstGrid(icnt).LENDING_DT
                    model(icnt).RETURN_DT = lstGrid(icnt).RETURN_DT
                    If lstGrid(icnt).RESERVE_STATUS = ReserveStatus.Mi Then
                        '未貸出の場合、実質貸出開始日をセット
                        model(icnt).PR_ST_DT = model(icnt).REAL_START_DT
                    ElseIf lstGrid(icnt).RESERVE_STATUS = ReserveStatus.Lending Then
                        '貸出中の場合、貸出日をセット
                        model(icnt).PR_ST_DT = model(icnt).LENDING_DT
                    Else
                        'それ以外は、そのままセット
                        model(icnt).PR_ST_DT = lstGrid(icnt).PR_ST_DT
                    End If
                    If lstGrid(icnt).RESERVE_STATUS = ReserveStatus.HenkyakuSumi Then
                        '返却済みの場合、返却日をセット
                        model(icnt).PR_END_DT = model(icnt).RETURN_DT
                    Else
                        'それ以外の場合、実質貸出終了日をセット
                        model(icnt).PR_END_DT = model(icnt).REAL_END_DT
                    End If

                    ''期間延長回数取得
                    'Dim setting_KikanEnchoSuu As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_ChangedTimes.KeyDiv, CommonSettingKey.Lending_ChangedTimes.Key1, CommonSettingKey.Lending_ChangedTimes.Key2, CommonSettingKey.Lending_ChangedTimes.Key3, True)
                    'Dim sKikanEnchoSuu As String = setting_KikanEnchoSuu.DATA1

                    ''返却リードタイム
                    'Dim setting_HenkyakuLT As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Return_LeadTime.KeyDiv, CommonSettingKey.Return_LeadTime.Key1, CommonSettingKey.Return_LeadTime.Key2, CommonSettingKey.Return_LeadTime.Key3, True)
                    'Dim iHenkyakuLT As Integer = CInt(setting_KikanEnchoSuu.DATA1)
                    ''稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                    'Dim sWorkFlg As String = WorkFlg.Jitsubi
                    'If setting_KikanEnchoSuu.DATA2 = WorkDayFlg.Yes Then
                    '    sWorkFlg = WorkFlg.Kadobi
                    'End If

                    '期間延長回数
                    model(icnt).LEASE_CHG_TIMES = lstGrid(icnt).LEASE_CHG_TIMES
                    '予約変更可能日数を取得する。
                    Dim Setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Update_Enabled.KeyDiv, CommonSettingKey.Lending_Update_Enabled.Key1, CommonSettingKey.Lending_Update_Enabled.Key2, CommonSettingKey.Lending_Update_Enabled.Key3, True)
                    Dim sUpdateOkDays As Integer = Setting.DATA1
                    Dim sWorkflg As String = Setting.DATA2
                    '対象日付取得(予約変更可能日付)
                    Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, sUpdateOkDays * -1, sWorkflg)   '2021/11/22 nv miyaji 稼働日フラグを設定マスタから取るように修正
                    'Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, sUpdateOkDays * -1, WorkFlg.Jitsubi)   '2021/11/22 nv miyaji 稼働日フラグを設定マスタから取るように修正

                    '----- 2021/11/29 nv Inoue 符号がおかしいので修正
                    '予約変更可能日付 ＞ 実質貸出期間Fromで、かつ実質貸出期間Toがデータベースの値より大きい値に変更されていた場合にカウントアップ。（引継データは除く）
                    'If iTargetDate <= model(icnt).REAL_START_DT Then
                    'If iTargetDate > model(icnt).REAL_START_DT Then    '2021/12/09 nv miyaji
                    If iTargetDate > model(icnt).REAL_START_DT AndAlso model(icnt).HANDOVER_DIV = HandOverDiv.Mi Then    '2021/12/09 nv miyaji
                        For Each row As ReserveDetailViewModel In lstReserveDetailOld
                            If row.ASSET_NO = model(icnt).ASSET_NO Then
                                '実貸出終了日がデータベースの値より大きい値に変更されていた場合にカウントアップする。
                                If row.REAL_END_DT < model(icnt).REAL_END_DT Then
                                    model(icnt).LEASE_CHG_TIMES = lstGrid(icnt).LEASE_CHG_TIMES + 1
                                    isMailEncho = True          '2021/11/06 add nv Inoue
                                End If
                                Exit For
                            End If
                        Next
                    End If

                    model(icnt).RESERVE_STATUS = lstGrid(icnt).RESERVE_STATUS
                    model(icnt).INS_DATETIME = CommonRfid.getNowDate
                    model(icnt).INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                    model(icnt).INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                    model(icnt).UPD_DATETIME = CommonRfid.getNowDate
                    model(icnt).UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                    model(icnt).UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm

                Next

                'システムコンテキストにモデルを追加
                _db.ReserveDetail.AddRange(model)

                If _ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK Then
                    '新規登録時
                    'システムコンテキストに予約履歴モデルを追加。
                    InsertReserveLog(_Model, ProcessDiv.MI)
                Else
                    '変更時
                    'システムコンテキストに予約履歴モデルを追加。
                    InsertReserveLog(_Model, ProcessDiv.MU)
                End If

                '--2021/12/07 nv miyaji update start
                '今回削除された明細の資産のメンテナンスデータは削除する。
                For Each row As ReserveDetailViewModel In lstReserveDetailOld
                    'グリッドデータから該当資産情報を取得
                    Dim lstModelSelDetail As ReserveDetailViewModel() = (From selModel In lstGrid
                                                                         Where (selModel.ASSET_NO = row.ASSET_NO)
                                                                         Order By selModel.ASSET_NO).ToArray
                    If lstModelSelDetail.Length = 0 Then
                        '明細が削除されていた場合
                        Dim sbSQL_DelMei As StringBuilder = New StringBuilder
                        Dim params_DelMei As List(Of SqlParameter) = New List(Of SqlParameter)
                        sbSQL_DelMei.AppendLine("DELETE FROM T_RESERVE_MAINTE WHERE ASSET_NO = @AssetNo AND RESERVE_ID = @ReserveId")
                        params_DelMei.Add(New SqlParameter("@AssetNo", row.ASSET_NO))
                        params_DelMei.Add(New SqlParameter("@ReserveId", row.RESERVE_ID))
                        _db.Database.ExecuteSqlCommand(sbSQL_DelMei.ToString, params_DelMei.ToArray)

                    End If

                Next
                '--2021/12/07 nv miyaji update end

                '登録処理を実行
                _db.SaveChanges()

                '引継先の予約のステータスを貸出中にする。　※以下の更新処理をこの位置より下で実施すると更新結果が正しくコミットできなかったため、この位置にした
                Dim sbSQL As StringBuilder = New StringBuilder
                For Each item In lstFinReserveId
                    sbSQL.Clear()
                    sbSQL.AppendLine($"UPDATE T_RESERVE SET RESERVE_STATUS = CASE (SELECT COUNT(1) FROM T_RESERVE_DETAIL WHERE RESERVE_ID = {item} AND RESERVE_STATUS = 0) WHEN 0 THEN 1 ELSE 0 END, UPD_DATETIME = GETDATE() WHERE  RESERVE_ID = {item};")
                    _db.Database.ExecuteSqlCommand(sbSQL.ToString)
                Next

                '資産メンテナンス登録処理　2021/11/10 add  nv Inoue start
                Call Dao.ToolReserve.UpdAssetMainte(Me._db, _Model.RESERVE_ID, lstGrid, Me._dsMentInfo, Me._LoginInfo, sTaskNm)
                Me._dsMentInfo.Dispose()
                '資産メンテナンス登録処理　2021/11/10 add  nv Inoue end

                'メール送信用のコード  nv Inoue start
                If (ViewModel.OnCreteButton = CommonConst.KEY_ON_UPDATE_CLICK) Then
                    '期間延長メールの送信
                    Dim settingMailPeriodExtension As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Mail_Period_Extension.KeyDiv, CommonSettingKey.Mail_Period_Extension.Key1, CommonSettingKey.Mail_Period_Extension.Key2, CommonSettingKey.Mail_Period_Extension.Key3, True)
                    If settingMailPeriodExtension.DATA1 = Misc.MailSendFlg.Yes Then
                        '資産の管理ロケ（設定M(KEY種別=Mail_Period_Extension_Loc)）が存在しており、実質貸出期間Toが非活性時に期間延長されていた場合
                        Dim clsToolMail As New Dao.ToolMail(Me._LoginInfo, Me._Message)
                        If isMailEncho And clsToolMail.ChkMngLoc(CommonSettingKey.Mail_Period_Extension_Loc.KeyDiv, Me._Model.MNG_LOC_L_CD) Then
                            Call clsToolMail.MakeMailEncho(Me._db, Me._Model, model, lstReserveDetailOld)
                        End If
                    End If

                    '担当者変更メールの送信
                    Dim settingMail_Change_Person As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Mail_Change_Person.KeyDiv, CommonSettingKey.Mail_Change_Person.Key1, CommonSettingKey.Mail_Change_Person.Key2, CommonSettingKey.Mail_Change_Person.Key3, True)
                    If settingMail_Change_Person.DATA1 = Misc.MailSendFlg.Yes Then
                        '資産の管理ロケ（設定M(KEY種別=Mail_Period_Extension_Loc)）が存在しており、実質貸出期間Toが非活性時に期間延長されていた場合
                        Dim clsToolMail As New Dao.ToolMail(Me._LoginInfo, Me._Message)
                        If isMailChangeEmp And clsToolMail.ChkMngLoc(CommonSettingKey.Mail_Change_Person_Loc.KeyDiv, Me._Model.MNG_LOC_L_CD) Then
                            Call clsToolMail.MakeMailHikitsugi(Me._db, Me._Model, model, lstReserveDetailOld)
                        End If
                    End If

                    '出荷通知メールの送信
                    Dim settingMailShipping As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Mail_Shipping.KeyDiv, CommonSettingKey.Mail_Shipping.Key1, CommonSettingKey.Mail_Shipping.Key2, CommonSettingKey.Mail_Shipping.Key3, True)
                    If settingMailShipping.DATA1 = Misc.MailSendFlg.Yes Then
                        '送り状番号が空以外で変更されていた場合
                        If Not String.IsNullOrEmpty(Me._Model.DEST_NO) AndAlso Me._Model.DEST_NO <> modelOld.DEST_NO Then
                            Dim clsToolMail As New Dao.ToolMail(Me._LoginInfo, Me._Message)
                            Call clsToolMail.MakeMailShukkaTsuchi(Me._db, Me._Model, model)
                        End If
                    End If

                End If
                'メール送信用のコード  end

                If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU) & " " &
                                                         Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                         Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
                                                         CommonRfid.ToStringNvl(_Model.RESERVE_ID).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU))

                    _ViewModel.States = StatusType.SuccessCreate

                Else

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU) & " " &
                                                         Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                         Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
                                                         CommonRfid.ToStringNvl(_Model.RESERVE_ID).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU))

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
            Session(CommonSessionKey.CREAGE_LENDING_ENTRY) = _ViewModel

            'エラーでない場合、セッションクリア
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

            'If _LoginInfo.ShosaiAuthDiv = SHOSAI_AUTH_DIV.Tsujo_Nomal _
            '                    OrElse _LoginInfo.ShosaiAuthDiv = SHOSAI_AUTH_DIV.Kyoten_Nomal _
            '                    OrElse _LoginInfo.ShosaiAuthDiv = SHOSAI_AUTH_DIV.Honbu_Nomal Then

            '    '担当者・拠点担当者・本部担当者は社内用備考・ユーザー貸出終了日・実貸出終了日しか変更できないので、データベースから元のデータを取得する。
            '    _Model = _db.LendingEntry.Find(_ViewModel.RESERVE_ID)

            '    'ビューモデルの実貸出終了日・ユーザー貸出終了日・社内用備考を登録用モデルにコピーする。
            '    _Model.REAL_END_DT = _ViewModel.REAL_END_DT
            '    _Model.USER_END_DT = _ViewModel.USER_END_DT
            '    _Model.REMARK_INTERNAL = _ViewModel.REMARK_INTERNAL
            'Else
            'モデルを登録用モデルにコピー
            CopyViewModeltoModel(_ViewModel)
            'End If


        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeEdit()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_ENTRY) = _ViewModel

            'エラーでない場合、セッションクリア
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
            Dim blnReturn As Boolean = True

            '予約番号　2022/7/25 sv sato add
            If GetInputEnableReserveId() Then
                '入力可能時のみチェック
                '--入力されていない場合、エラー
                If _Model.RESERVE_ID = 0 Then
                    ModelState.AddModelError("ReserveIdString", CommonLabel.MSG_ERR_NOT_INPUT_RESERVE_ID)
                    Return False
                End If

                '--重複している場合、エラー
                Dim existModel As LendingEntryModel = _db.LendingEntry.Find(_Model.RESERVE_ID)
                If Not IsNothing(existModel) Then
                    ModelState.AddModelError("ReserveIdString", CommonLabel.MSG_ERR_EXITS_RESERVE_ID)
                    Return False
                End If
            End If

            '実貸出開始日
            '--入力されていない場合、エラー
            If _Model.REAL_START_DT = 0 Then
                ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_REAL_START_DT_EMPTY)
                Return False
            End If
            '--システム日付より過去の場合、エラー
            If _Model.REAL_START_DT < CInt(Now.Date.ToString("yyyyMMdd")) Then
                ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_REAL_START_DT_KAKOBI)
                Return False
            End If
            '--システム設定の貸出開始日のリードタイムから取得した対象日付＜システム日付　の場合、エラー
            '--2021/12/16 nv miyaji update start
            '一般者か又は管理者でもU_HUB権限があり自拠点でない場合のチェックが必要かを取得
            Dim isNeedCheck As Boolean = GetIsNeedCheck(_LoginInfo, _Model.MNG_LOC_L_CD)
            If isNeedCheck Then
                'If _LoginInfo.KengenKbn = AUTH_DIV.Nomal Then   '2021/12/13 nv miyaji　一般者のみチェックする。
                '--2021/12/16 nv miyaji update end
                Dim setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_StartDt_LT.KeyDiv, CommonSettingKey.Lending_StartDt_LT.Key1, CommonSettingKey.Lending_StartDt_LT.Key2, CommonSettingKey.Lending_StartDt_LT.Key3, True)
                '対象日付取得(貸出可能日)
                Dim iLendingStartDate As Integer = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(_Model.REAL_START_DT, "yyyyMMdd")), CInt(setting.DATA1) * -1, setting.DATA2)
                '--2021/12/02 nv miyaji add start
                If iLendingStartDate = 0 Then
                    '0が帰ってきた場合、エラーメッセージを出す。
                    ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_DATE_SETTEIGAI)
                    Return False
                End If
                '--2021/12/02 nv miyaji add end
                If iLendingStartDate < CInt(Now.Date.ToString("yyyyMMdd")) Then
                    ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_REAL_START_DATE_LT)
                    Return False
                End If
            End If

            '実貸出終了日
            '入力されていない場合、エラー
            If _Model.REAL_END_DT = 0 Then
                ModelState.AddModelError("RealEndDtString", CommonLabel.MSG_ERR_REAL_END_DT_EMPTY)
                Return False
            End If

            '実貸出開始日＞実貸出終了日の場合、エラー
            If _Model.REAL_START_DT > _Model.REAL_END_DT Then
                ModelState.AddModelError("RealEndDtString", CommonLabel.MSG_ERR_REAL_END_DT_KAKOBI)
                Return False
            End If

            '実貸出開始日、実貸出終了日 1000年以上チェック
            If _Model.REAL_START_DT < MIN_DATE Then
                ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If _Model.REAL_END_DT < MIN_DATE Then
                ModelState.AddModelError("RealEndDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            'ユーザー貸出期間開始日、ユーザー貸出期間終了日 1000年以上チェック
            If _Model.USER_START_DT < MIN_DATE Then
                ModelState.AddModelError("UserStartDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If _Model.USER_END_DT < MIN_DATE Then
                ModelState.AddModelError("UserEndDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '貸出期間チェック
            '--2021/12/16 nv miyaji update start
            '一般者か又は管理者でもU_HUB権限があり自拠点でない場合のチェックが必要かを取得
            If isNeedCheck Then
                ''一般者の場合のみ、チェックする。
                'If _LoginInfo.KengenKbn = AUTH_DIV.Nomal Then
                Dim iTargetDate As Integer = 0

                '--2021/11/22 nv miyaji update start Reservation_Methodの数だけチェックするように変更
                Dim iCnt As Integer = 1
                While iCnt > 0
                    Dim Youto As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method.KeyDiv, iCnt, CommonSettingKey.Reservation_Method.Key2, CommonSettingKey.Reservation_Method.Key3)
                    If Youto Is Nothing Then
                        iCnt = 0
                    Else
                        If _Model.LEASE_USE = Youto.DATA1 Then
                            '対象日付取得(貸出可能期間)
                            iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(_Model.REAL_START_DT, "yyyyMMdd")), Youto.DATA2 - 1, Youto.DATA3)
                            '--2021/12/02 nv miyaji add start
                            If iTargetDate = 0 Then
                                '0が帰ってきた場合、エラーメッセージを出す。
                                ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_DATE_SETTEIGAI)
                                Return False
                            End If
                            '--2021/12/02 nv miyaji add end
                            Exit While
                        End If
                        iCnt += 1
                    End If
                End While
                'Dim Youto1 As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method.KeyDiv, CommonSettingKey.Reservation_Method.Key1, CommonSettingKey.Reservation_Method.Key2, CommonSettingKey.Reservation_Method.Key3)
                'Dim Youto2 As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method_Development.KeyDiv, CommonSettingKey.Reservation_Method_Development.Key1, CommonSettingKey.CsvMapShiftJis.Key2, CommonSettingKey.Reservation_Method_Development.Key3)
                'If _Model.LEASE_USE = Youto1.DATA1 Then
                '    '対象日付取得(貸出可能期間)
                '    iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(_Model.REAL_START_DT, "yyyyMMdd")), Youto1.DATA2, Youto1.DATA3)
                'Else
                '    '対象日付取得(貸出可能期間)
                '    iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(_Model.REAL_START_DT, "yyyyMMdd")), Youto2.DATA2, Youto2.DATA3)
                'End If
                '--2021/11/22 nv miyaji update end

                If _Model.REAL_END_DT > iTargetDate Then
                    ModelState.AddModelError("LEASE_USE", String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_REAL_DT_TOO_LONG), CommonRfid.ToStringDateYMD(iTargetDate, "yyyyMMdd")))
                    Return False
                End If
            End If

            '非稼働日の場合、エラー
            If Dao.ToolReserve.GetNotWorkFlg(_Model.REAL_START_DT) = NotWorkFlg.Yes Then
                ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_NOT_WORK_FRG_YES)
                Return False
            End If
            If Dao.ToolReserve.GetNotWorkFlg(_Model.REAL_END_DT) = NotWorkFlg.Yes Then
                ModelState.AddModelError("RealEndDtString", CommonLabel.MSG_ERR_NOT_WORK_FRG_YES)
                Return False
            End If

            'ADD Start sv sato 2022/10/03
            '送り先名と送り先住所の必須チェック
            If ChkDestNameAndDestAddressRequired() = False Then
                Return False
            End If
            'ADD End sv sato 2022/10/03

            '予約期間の重複確認
            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)
            For Each row As ReserveDetailViewModel In lstGrid
                If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, 0, 0, row.REAL_START_DT, row.REAL_END_DT) > 0 Then
                    ModelState.AddModelError("RealEndDtString", CommonLabel.MSG_ERR_MAINTE_DT_JUUFUKU)
                    Return False
                End If
            Next

            '2021/11/10 add nv Inoue
            '各資産の　資産メンテナンス登録情報 セット　&　チェック
            Dim sNgAssetNo As String = CheckAssetMainteInfo(lstGrid)
            If Not String.IsNullOrEmpty(sNgAssetNo) Then
                ModelState.AddModelError("RealEndDtString", String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_MUST_MAINTE), sNgAssetNo))
                Return False
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
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU)

            _db.LendingEntry.Add(_Model)

            '資産履歴テーブルを更新する。
            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)
            For Each row As ReserveDetailViewModel In lstGrid
                Dim db As New SystemDBContext
                Dim modelAsset As AssetListModel = db.AssetList.Find(row.ASSET_NO)
                InsertAssetHistory(modelAsset, ProcessDiv.RI)
            Next

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'DBのデータを取得する
            Dim model As LendingEntryModel = db.LendingEntry.Find(_Model.RESERVE_ID)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            '実貸出開始日
            '--入力されていない場合、エラー
            If _Model.REAL_START_DT = 0 Then
                ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_REAL_START_DT_EMPTY)
                Return False
            End If

            '一般者か又は管理者でもU_HUB権限があり自拠点でない場合のチェックが必要かを取得
            Dim isNeedCheck As Boolean = GetIsNeedCheck(_LoginInfo, _Model.MNG_LOC_L_CD)
            ''--データベースと値が違う場合
            If model.REAL_START_DT <> _Model.REAL_START_DT Then
                '--システム日付より過去の場合、エラー
                If _Model.REAL_START_DT < CInt(Now.Date.ToString("yyyyMMdd")) Then
                    ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_REAL_START_DT_KAKOBI)
                    Return False
                End If
                '--システム設定の貸出開始日のリードタイムから取得した対象日付＜システム日付　の場合、エラー
                If isNeedCheck Then   '2021/12/16 nv miyaji　一般者と、管理者でもU_HUB権限があり自拠点でない場合にチェックする。
                    Dim setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_StartDt_LT.KeyDiv, CommonSettingKey.Lending_StartDt_LT.Key1, CommonSettingKey.Lending_StartDt_LT.Key2, CommonSettingKey.Lending_StartDt_LT.Key3, True)
                    '対象日付取得(貸出可能日)
                    Dim iLendingStartDate As Integer = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(_Model.REAL_START_DT, "yyyyMMdd")), CInt(setting.DATA1) * -1, setting.DATA2)
                    '--2021/12/02 nv miyaji add start
                    If iLendingStartDate = 0 Then
                        '0が帰ってきた場合、エラーメッセージを出す。
                        ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_DATE_SETTEIGAI)
                        Return False
                    End If
                    '--2021/12/02 nv miyaji add end
                    If iLendingStartDate < CInt(Now.Date.ToString("yyyyMMdd")) Then
                        ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_REAL_START_DATE_LT)
                        Return False
                    End If
                End If
            End If

            '実貸出終了日
            '入力されていない場合、エラー
            If _Model.REAL_END_DT = 0 Then
                ModelState.AddModelError("RealEndDtString", CommonLabel.MSG_ERR_REAL_END_DT_EMPTY)
                Return False
            End If

            '実貸出開始日＞実貸出終了日の場合、エラー
            If _Model.REAL_START_DT > _Model.REAL_END_DT Then
                ModelState.AddModelError("RealEndDtString", CommonLabel.MSG_ERR_REAL_END_DT_KAKOBI)
                Return False
            End If

            '実貸出開始日、実貸出終了日 1000年以上チェック
            If _Model.REAL_START_DT < MIN_DATE Then
                ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If _Model.REAL_END_DT < MIN_DATE Then
                ModelState.AddModelError("RealEndDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            'ユーザー貸出期間開始日、ユーザー貸出期間終了日 1000年以上チェック
            If _Model.USER_START_DT < MIN_DATE Then
                ModelState.AddModelError("UserStartDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            If _Model.USER_END_DT < MIN_DATE Then
                ModelState.AddModelError("UserEndDtString", CommonLabel.MSG_ERR_INCORRECT_DATE)
                blnReturn = False
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '貸出期間チェック
            '--2021/12/16 nv miyaji upd start　一般者と、管理者でもU_HUB権限があり自拠点でない場合にチェックする。
            If isNeedCheck Then
                ''一般者の場合のみ、チェックする。
                'If _LoginInfo.KengenKbn = AUTH_DIV.Nomal Then
                '--2021/12/16 nv miyaji upd end
                Dim iTargetDate As Integer = 0

                '--2021/11/22 nv miyaji update start Reservation_Methodの数だけチェックするように変更
                Dim iCnt As Integer = 1
                While iCnt > 0
                    Dim Youto As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method.KeyDiv, iCnt, CommonSettingKey.Reservation_Method.Key2, CommonSettingKey.Reservation_Method.Key3)
                    If Youto Is Nothing Then
                        iCnt = 0
                    Else
                        If _Model.LEASE_USE = Youto.DATA1 Then
                            '対象日付取得(貸出可能期間)
                            '--2021/12/13 nv miyaji update start
                            If model.REAL_END_DT - model.REAL_START_DT >= _Model.REAL_END_DT - _Model.REAL_START_DT Then
                                '期間が変わらないまたは縮んだ場合、貸出用途の内容２-1と3で実質貸出開始日～終了日チェック
                                iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(model.REAL_START_DT, "yyyyMMdd")), Youto.DATA2 - 1, Youto.DATA3)  '2021/12/07 nv miyaji 仕様変更
                            Else
                                '期間が延びた場合、現状の内容5と6でチェック
                                iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(model.REAL_END_DT, "yyyyMMdd")), Youto.DATA5, Youto.DATA6)  '2021/12/07 nv miyaji 仕様変更
                            End If
                            If iTargetDate = 0 Then
                                '0が帰ってきた場合、エラーメッセージを出す。
                                ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_DATE_SETTEIGAI)
                                Return False
                            End If
                            'iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(model.REAL_END_DT, "yyyyMMdd")), Youto.DATA5, Youto.DATA6)  '2021/12/07 nv miyaji 仕様変更
                            ''iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(_Model.REAL_START_DT, "yyyyMMdd")), Youto.DATA2, Youto.DATA3)  '2021/12/07 nv miyaji 仕様変更
                            ''--2021/12/02 nv miyaji add start
                            'If iTargetDate = 0 Then
                            '    '0が帰ってきた場合、エラーメッセージを出す。
                            '    ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_DATE_SETTEIGAI)
                            '    Return False
                            'End If
                            '--2021/12/02 nv miyaji add end
                            '--2021/12/13 nv miyaji update end
                            Exit While
                        End If
                        iCnt += 1
                    End If
                End While
                'Dim Youto1 As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method.KeyDiv, CommonSettingKey.Reservation_Method.Key1, CommonSettingKey.Reservation_Method.Key2, CommonSettingKey.Reservation_Method.Key3)
                'Dim Youto2 As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method_Development.KeyDiv, CommonSettingKey.Reservation_Method_Development.Key1, CommonSettingKey.CsvMapShiftJis.Key2, CommonSettingKey.Reservation_Method_Development.Key3)
                'If _Model.LEASE_USE = Youto1.DATA1 Then
                '    '対象日付取得(貸出可能期間)
                '    iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(_Model.REAL_START_DT, "yyyyMMdd")), Youto1.DATA2, Youto1.DATA3)
                'Else
                '    '対象日付取得(貸出可能期間)
                '    iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(_Model.REAL_START_DT, "yyyyMMdd")), Youto2.DATA2, Youto2.DATA3)
                'End If
                '--2021/11/22 nv miyaji update end

                If _Model.REAL_END_DT > iTargetDate Then
                    ModelState.AddModelError("LEASE_USE", String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_REAL_DT_TOO_LONG), CommonRfid.ToStringDateYMD(iTargetDate, "yyyyMMdd")))
                    Return False
                End If
            End If

            '非稼働日の場合、エラー
            If Dao.ToolReserve.GetNotWorkFlg(_Model.REAL_START_DT) = NotWorkFlg.Yes Then
                ModelState.AddModelError("RealStartDtString", CommonLabel.MSG_ERR_NOT_WORK_FRG_YES)
                Return False
            End If
            If Dao.ToolReserve.GetNotWorkFlg(_Model.REAL_END_DT) = NotWorkFlg.Yes Then
                ModelState.AddModelError("RealEndDtString", CommonLabel.MSG_ERR_NOT_WORK_FRG_YES)
                Return False
            End If

            'ADD Start sv sato 2022/10/03
            '送り先名と送り先住所の必須チェック
            If ChkDestNameAndDestAddressRequired() = False Then
                Return False
            End If
            'ADD End sv sato 2022/10/03

            '予約期間の重複確認
            For Each row As ReserveDetailViewModel In lstGrid
                '明細が引継元でない　かつ　返却済みでない時にチェック
                If row.HANDOVER_DIV <> HandOverDiv.HikitsugiMoto AndAlso row.RESERVE_STATUS <> ReserveStatus.HenkyakuSumi Then  '2022/03/14 nv miyaji 条件に返却済みでない場合を追加。
                    'If row.HANDOVER_DIV <> HandOverDiv.HikitsugiMoto Then  '2022/03/14 nv miyaji
                    If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_START_DT, row.REAL_END_DT) > 0 Then
                        ModelState.AddModelError("RealEndDtString", CommonLabel.MSG_ERR_MAINTE_DT_JUUFUKU)
                        Return False
                    End If
                End If
            Next

            '2021/11/10 add nv Inoue
            '各資産の　資産メンテナンス登録情報 セット　&　チェック
            Dim sNgAssetNo As String = CheckAssetMainteInfo(lstGrid)
            If Not String.IsNullOrEmpty(sNgAssetNo) Then
                ModelState.AddModelError("", String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_MUST_MAINTE), sNgAssetNo))
                Return False
            End If

            '他端末で更新されているかチェックする
            If _Model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") <> model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") Then
                'モデルとDBの更新日付が異なっている場合エラー
                ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                Return False
            End If

            Return blnReturn
        End Function

        ''' <summary>
        ''' 更新処理のコンテキストを変更します
        ''' </summary>
        Protected Overrides Sub EditQuery()
            Dim db As New SystemDBContext
            'データベースのモデルを取得
            Dim modelOld As New LendingEntryModel
            modelOld = db.LendingEntry.Find(_ViewModel.RESERVE_ID)
            '予約変更可能日数を取得する。
            Dim Setting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Update_Enabled.KeyDiv, CommonSettingKey.Lending_Update_Enabled.Key1, CommonSettingKey.Lending_Update_Enabled.Key2, CommonSettingKey.Lending_Update_Enabled.Key3, True)
            Dim sUpdateOkDays As Integer = Setting.DATA1
            Dim sWorkflg As String = Setting.DATA2
            '対象日付取得(予約変更可能日付)
            Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, sUpdateOkDays * -1, sWorkflg)   '2021/11/22 nv miyaji 稼働日フラグを設定マスタから取るように修正
            'Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(Now.Date, sUpdateOkDays * -1, WorkFlg.Jitsubi)   '2021/11/22 nv miyaji 稼働日フラグを設定マスタから取るように修正
            '予約変更可能日付 > 実質貸出期間Fromで、かつ実貸出終了日がデータベースの値より大きい値に変更されていた場合にカウントアップ。
            If iTargetDate > _Model.REAL_START_DT AndAlso _Model.REAL_END_DT > modelOld.REAL_END_DT Then
                _Model.LEASE_CHG_TIMES += 1
            End If

            '予約ステータス
            Dim lendingAri As Integer = 0
            Dim henkyariAri As Integer = 0
            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)
            For Each row As ReserveDetailViewModel In lstGrid
                '貸出中のデータがある場合
                If row.RESERVE_STATUS = ReserveStatus.Lending Then
                    lendingAri += 1
                End If
                '返却済みのデータがある場合
                If row.RESERVE_STATUS = ReserveStatus.HenkyakuSumi Then
                    henkyariAri += 1
                End If
            Next

            'If isLendingAri Then
            '    '引継先以外の貸出中のデータがある場合
            '    _Model.RESERVE_STATUS = ReserveStatus.Lending
            'ElseIf Not isMiAri Then
            '    '引継先以外の貸出中のデータ・未貸出のデータがない場合（全て返却済み）
            '    _Model.RESERVE_STATUS = ReserveStatus.HenkyakuSumi
            'Else
            '    'それ以外
            '    _Model.RESERVE_STATUS = ReserveStatus.Mi
            'End If
            If lstGrid.Count = henkyariAri Then
                '返却済み
                _Model.RESERVE_STATUS = ReserveStatus.HenkyakuSumi
            ElseIf (lstGrid.Count - henkyariAri) = lendingAri Then
                '未貸出がない場合は貸出中
                _Model.RESERVE_STATUS = ReserveStatus.Lending
            Else
                'それ以外
                _Model.RESERVE_STATUS = ReserveStatus.Mi
            End If

            '更新日時・更新社員コード・更新社員名・更新プロセスを登録
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU)

            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified

            '資産履歴テーブルを更新する。
            ''グリッドの情報を取得
            'Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)
            For Each row As ReserveDetailViewModel In lstGrid
                Dim db1 As New SystemDBContext
                Dim modelAsset As AssetListModel = db1.AssetList.Find(row.ASSET_NO)
                InsertAssetHistory(modelAsset, ProcessDiv.RU)
            Next

        End Sub

        '今回、不使用
        '''' <summary>
        '''' 変更内容を非同期通信で取得します（AJax)
        '''' </summary>
        '''' <param name="ID">ID</param>
        '''' <returns>View表示内容</returns>
        'Public Function GetEditData(ByVal ID As String) As JsonResult
        '    Dim db As New SystemDBContext

        '    'IDをキーにモデルを取得する
        '    Dim model As LendingEntryModel = db.LendingEntry.Find(ID)
        '    model.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
        '    model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")

        '    Return Json(model)

        'End Function
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
        Public Overridable Function Delete(ByVal ViewModel As LendingEntryViewModel) As ActionResult
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
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.RESERVE_ID).Trim
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU))

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
            Session(CommonSessionKey.CREAGE_LENDING_ENTRY) = _ViewModel

        End Sub

        ''' <summary>
        ''' 削除処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function DeleteChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            '予約ステータスが未貸出以外のデータを取得
            Dim lstModelSelDetail As ReserveDetailViewModel() = (From model In lstGrid
                                                                 Where (model.RESERVE_STATUS <> ReserveStatus.Mi)
                                                                 Order By model.ASSET_NO).ToArray
            '未貸出以外の明細があればエラー
            If lstModelSelDetail.Count > 0 Then
                ModelState.AddModelError("", CommonLabel.MSG_ERR_EXIST_NOT_RESERVE_STATUS_MI)
                blnReturn = False
            End If

            Return blnReturn
        End Function

        ''' <summary>
        ''' 削除処理のクエリを作成します
        ''' </summary>
        Protected Overrides Sub DeleteQuery()
            Dim entry As LendingEntryModel = _db.LendingEntry.Find(_Model.RESERVE_ID)

            '削除を実行する
            _db.LendingEntry.Remove(entry)

            '更新した予約IDを取得
            Dim lReserveId As Long = _Model.RESERVE_ID

            '資産履歴テーブルを更新する。
            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)
            For Each row As ReserveDetailViewModel In lstGrid
                Dim db As New SystemDBContext
                Dim modelAsset As AssetListModel = db.AssetList.Find(row.ASSET_NO)
                InsertAssetHistory(modelAsset, ProcessDiv.RD)
            Next

            '予約履歴テーブルを更新する。
            InsertReserveLog(_Model, ProcessDiv.MD)

            '予約明細テーブルの削除
            '--2021/12/07 nv miyaji update start
            Dim sbSQL_DelDetail As StringBuilder = New StringBuilder
            Dim params_DelDetail As List(Of SqlParameter) = New List(Of SqlParameter)
            sbSQL_DelDetail.AppendLine("DELETE FROM T_RESERVE_DETAIL WHERE RESERVE_ID = @ReserveId")
            params_DelDetail.Add(New SqlParameter("@ReserveId", lReserveId))
            _db.Database.ExecuteSqlCommand(sbSQL_DelDetail.ToString, params_DelDetail.ToArray)
            '_db.Database.ExecuteSqlCommand("DELETE FROM T_RESERVE_DETAIL WHERE RESERVE_ID = '" & lReserveId & "'")
            '--2021/12/07 nv miyaji update end

            'メンテナンステーブルの削除
            '--2021/12/07 nv miyaji update start
            Dim sbSQL_DelMainte As StringBuilder = New StringBuilder
            Dim params_DelMainte As List(Of SqlParameter) = New List(Of SqlParameter)
            sbSQL_DelMainte.AppendLine("DELETE FROM T_RESERVE_MAINTE WHERE RESERVE_ID = @ReserveId")
            params_DelMainte.Add(New SqlParameter("@ReserveId", lReserveId))
            _db.Database.ExecuteSqlCommand(sbSQL_DelMainte.ToString, params_DelMainte.ToArray)
            '_db.Database.ExecuteSqlCommand("DELETE FROM T_RESERVE_MAINTE WHERE RESERVE_ID = '" & lReserveId & "'")


        End Sub
#End Region

#Region "貸出処理"
        ''' <summary>
        ''' 貸出処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function Lending(ViewModel As LendingEntryViewModel, SelectKeys As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If


            'モデルを設定
            _ViewModel = ViewModel

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            Dim lstSelectKeys As List(Of String) = SelectKeys.Split(",").ToList

            'チェックボックスlstSelectKeysの選択状態のキー(資産番号)を更新する
            Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstSelectKeys

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '--2021/12/14 nv miyaji add start
            'エラーチェック
            '貸出する際、「同じ資産番号」かつ「未貸出」かつ「画面に表示している予約の実質貸出開始日＞別の予約の貸出予実開始日の予約が存在する」かつ「つシステム日付≦別の予約の貸出予実終了日」の場合
            Dim db As New SystemDBContext
            For Each model As ReserveDetailViewModel In lstGrid
                If lstSelectKeys.IndexOf(model.ASSET_NO) > -1 Then
                    Dim lstSelected As New List(Of ReserveDetailModel)
                    Dim sbSQL As StringBuilder = New StringBuilder
                    Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2021/12/07 nv miyaji add
                    'T_RESERVE_DETAILテーブルから取得するSQLを作成
                    sbSQL.AppendLine("SELECT *")
                    sbSQL.AppendLine("FROM T_RESERVE_DETAIL")
                    sbSQL.AppendLine("WHERE")
                    sbSQL.AppendLine("ASSET_NO = @AssetNo")
                    sbSQL.AppendLine("AND RESERVE_STATUS = '" & ReserveStatus.Mi & "'")
                    sbSQL.AppendLine("AND @RealStartDt > PR_ST_DT")
                    sbSQL.AppendLine("AND @NowDt <= PR_END_DT")
                    params.Add(New SqlParameter("@AssetNo", model.ASSET_NO))
                    params.Add(New SqlParameter("@RealStartDt", model.REAL_START_DT))
                    params.Add(New SqlParameter("@NowDt", CInt(Now.Date.ToString("yyyyMMdd"))))
                    'SQLを実行する
                    lstSelected = db.ReserveDetail.SqlQuery(sbSQL.ToString, params.ToArray).ToList
                    If lstSelected IsNot Nothing AndAlso lstSelected.Count > 0 Then
                        '条件に合致したデータが有る場合、エラーメッセージ
                        _ViewModel.ErrCode = CommonLabel.MSG_ERR_EXITS_BEFORE_RESERVE_STATUS_MI

                        'モデルの内容をセッションに保持
                        Session(CommonSessionKey.CREAGE_LENDING_ENTRY) = _ViewModel

                        '結果表示
                        Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
                    End If
                End If
            Next
            '--2021/12/14 nv miyaji add start

            '貸出WebApi呼出処理

            'WebApiUrl取得
            Dim setting_WebApiUrl As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.WebAPI_Url.KeyDiv, CommonSettingKey.WebAPI_Url.Key1, CommonSettingKey.WebAPI_Url.Key2, CommonSettingKey.WebAPI_Url.Key3, True)
            Dim sUrl As String = setting_WebApiUrl.DATA1

            Dim wsAssetLending As New WsAssetLending(sUrl)          'Webservice呼出クラス宣言
            Dim DtoWsLending As List(Of WsAssetLending.DtoAssetLending) = New List(Of AsetraLibWebApi.WsAssetLending.DtoAssetLending)
            For Each model As ReserveDetailViewModel In lstGrid
                If lstSelectKeys.IndexOf(model.ASSET_NO) > -1 Then
                    Dim modelAsset As AssetListModel = db.AssetList.Find(model.ASSET_NO)
                    Dim param As WsAssetLending.DtoAssetLending = New WsAssetLending.DtoAssetLending
                    param.AssetNo = model.ASSET_NO
                    param.AssetID = modelAsset.ASSET_ID
                    param.ProcessToolDiv = ProcessToolDiv.Web
                    param.ProcessDiv = ProcessDiv.BO
                    param.LeasePlaceCd = _ViewModel.LEASE_PLACE_CD
                    param.LeaseEmployeeCd = _ViewModel.CHARGE_EMP_CD
                    param.LeaseDueDt = model.REAL_END_DT
                    param.EmployeeCd = _LoginInfo.EmployeeCd

                    DtoWsLending.Add(param)
                End If
            Next

            Dim Result As ClsWebApiClient.EnResultAPI = wsAssetLending.WebApiAssetLending(DtoWsLending)
            Select Case Result
                Case ClsWebApiClient.EnResultAPI.OK_Result
                    '正常応答
                    _ViewModel.States = StatusType.SuccessLending

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_SHOSAI) & " " &
                                                             Me._Message.getMessage(CommonLabel.MSG_SUCCESS_LENDING) & " " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
                                                             CommonRfid.ToStringNvl(_ViewModel.RESERVE_ID).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_SHOSAI))

                Case ClsWebApiClient.EnResultAPI.NoAnswer   '応答なし
                    'WebAPIの応答がありません。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_NO_RESPONSE
                    'C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990017", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)

            'Case ClsWebApiClient.EnResultAPI.DB_Error   'DB異常応答
            '    '8000,WRN,YELLOW,BLACK,%1
            '    Call clsSV_MSG.SVSubMsgDisp(prmlblMsg, 8000, "WebAPIからデータベース異常を受信しました。")

                Case ClsWebApiClient.EnResultAPI.Old_Naster 'マスタ古い応答
                    'マスタが最新ではありません。マスタを再取り込みしてください。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_MASTER_OLD
                    'C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990018", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
            '20200309 Add Start Sasaki V1.2 貸出上限
                Case ClsWebApiClient.EnResultAPI.LEASE_UPPER_ERR    '貸出上限エラー
                    '正常応答
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_JOGEN
                    '_ConfirmMessage = "990020"
                    '_LendLimitError = True
                    'Return True
                Case ClsWebApiClient.EnResultAPI.LEASE_UPPER_WARN    '貸出上限警告
                    '正常応答
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_JOGEN_KEIKOKU
                    '_ConfirmMessage = "990021"
                    'Return True
                '20200309 Add End Sasaki V1.2 貸出上限
                '20211005 Add Start v2.0 予約該当なし
                Case ClsWebApiClient.EnResultAPI.ReserveErr
                    '実質貸出期間外のため、貸出できなかった資産があります。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_DATE_OUT_RESERVED
                    'C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "030009", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
                    '20211005 Add End v2.0 予約該当なし
                Case Else 'ClsWebApiClient.EnResultAPI.Other_Error  'その他異常
                    'WebAPIから異常応答がありました。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_IJOU
                    'C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990019", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
            End Select

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_ENTRY) = _ViewModel

            ''操作ログ出力
            'Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_SHOSAI) & " " &
            '                                         Me._Message.getMessage(CommonLabel.MSG_SUCCESS_LENDING) & " " &
            '                                         Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
            '                                         CommonRfid.ToStringNvl(_ViewModel.RESERVE_ID).Trim
            'Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_SHOSAI))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "返却処理"
        ''' <summary>
        ''' 返却処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function Returned(ViewModel As LendingEntryViewModel, SelectKeys As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If


            'モデルを設定
            _ViewModel = ViewModel

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            Dim lstSelectKeys As List(Of String) = SelectKeys.Split(",").ToList

            'チェックボックスlstSelectKeysの選択状態のキー(資産番号)を更新する
            Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstSelectKeys

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '返却WebApi呼出処理

            'WebApiUrl取得
            Dim setting_WebApiUrl As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.WebAPI_Url.KeyDiv, CommonSettingKey.WebAPI_Url.Key1, CommonSettingKey.WebAPI_Url.Key2, CommonSettingKey.WebAPI_Url.Key3, True)
            Dim sUrl As String = setting_WebApiUrl.DATA1

            Dim wsAssetReturn As New WsAssetReturn(sUrl)          'Webservice呼出クラス宣言
            Dim DtoWsReturn As List(Of WsAssetReturn.DtoAssetReturn) = New List(Of AsetraLibWebApi.WsAssetReturn.DtoAssetReturn)
            For Each model As ReserveDetailViewModel In lstGrid
                If lstSelectKeys.IndexOf(model.ASSET_NO) > -1 Then
                    Dim db As New SystemDBContext
                    Dim modelAsset As AssetListModel = db.AssetList.Find(model.ASSET_NO)
                    Dim param As WsAssetReturn.DtoAssetReturn = New WsAssetReturn.DtoAssetReturn
                    param.AssetNo = model.ASSET_NO
                    param.AssetID = modelAsset.ASSET_ID
                    param.ProcessToolDiv = ProcessToolDiv.Web
                    param.ProcessDiv = ProcessDiv.BI
                    'param.LeasePlaceCd = _ViewModel.LEASE_PLACE_CD
                    'param.LeaseEmployeeCd = _ViewModel.CHARGE_EMP_CD
                    param.LeaseDueDt = model.REAL_END_DT
                    param.EmployeeCd = _LoginInfo.EmployeeCd

                    DtoWsReturn.Add(param)
                End If
            Next

            Dim Result As ClsWebApiClient.EnResultAPI = wsAssetReturn.WebApiAssetReturn(DtoWsReturn)
            Select Case Result
                Case ClsWebApiClient.EnResultAPI.OK_Result
                    '正常応答
                    _ViewModel.States = StatusType.SuccessReturned

                Case ClsWebApiClient.EnResultAPI.NoAnswer   '応答なし
                    'WebAPIの応答がありません。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_NO_RESPONSE
                    'C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990017", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)

            'Case ClsWebApiClient.EnResultAPI.DB_Error   'DB異常応答
            '    '8000,WRN,YELLOW,BLACK,%1
            '    Call clsSV_MSG.SVSubMsgDisp(prmlblMsg, 8000, "WebAPIからデータベース異常を受信しました。")

                Case ClsWebApiClient.EnResultAPI.Old_Naster 'マスタ古い応答
                    'マスタが最新ではありません。マスタを再取り込みしてください。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_MASTER_OLD
                    'C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990018", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
            '20200309 Add Start Sasaki V1.2 貸出上限
                Case ClsWebApiClient.EnResultAPI.LEASE_UPPER_ERR    '貸出上限エラー
                    '正常応答
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_JOGEN
                    '_ConfirmMessage = "990020"
                    '_LendLimitError = True
                    'Return True
                Case ClsWebApiClient.EnResultAPI.LEASE_UPPER_WARN    '貸出上限警告
                    '正常応答
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_JOGEN_KEIKOKU
                    '_ConfirmMessage = "990021"
                    'Return True
                '20200309 Add End Sasaki V1.2 貸出上限
                '20211005 Add Start v2.0 予約該当なし
                Case ClsWebApiClient.EnResultAPI.ReserveErr
                    '実質貸出期間外のため、貸出できなかった資産があります。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_DATE_OUT_RESERVED
                    'C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "030009", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
                    '20211005 Add End v2.0 予約該当なし
                Case Else 'ClsWebApiClient.EnResultAPI.Other_Error  'その他異常
                    'WebAPIから異常応答がありました。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_IJOU
                    'C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990019", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
            End Select

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_LENDING_ENTRY) = _ViewModel

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_SHOSAI) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_RETURNED) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_LEASE_NUMBER) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.RESERVE_ID).Trim
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_SHOSAI))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function
#End Region

#Region "直接引継ボタン押下のエラーチェック(AJax)"
        ''' <summary>
        ''' 直接引継ボタン押下のエラーチェック(AJax)
        ''' </summary>
        ''' <param name="lReserveId">予約番号</param>
        ''' <param name="lstAsset">更新する資産番号のリスト</param>
        ''' <returns>View表示内容</returns>
        Public Function HikitsugiCheck(lReserveId As Long, lstAsset As List(Of String)) As JsonResult
            Dim db As New SystemDBContext
            Dim model As New LendingEntryViewModel  '返却用モデル

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            'エラーチェック
            'IDをキーにモデルを取得する
            Dim modelHed As LendingEntryModel = db.LendingEntry.Find(lstGrid(0).RESERVE_ID)
            Dim sNextEmployeeCd As String = String.Empty  '次予約の担当者格納用
            For Each row As ReserveDetailViewModel In lstGrid
                If lstAsset.IndexOf(row.ASSET_NO) > -1 Then
                    '引継のデータの場合、エラー
                    If row.HANDOVER_RESERVE_ID IsNot Nothing Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECTED_STATUS_HIKITSUGI)
                        Return Json(model)
                    End If
                    '貸出中以外のデータの場合、エラー
                    If row.RESERVE_STATUS <> ReserveStatus.Lending Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECTED_NOT_STATUS_LENDING)
                        Return Json(model)
                    End If

                    '--2021/12/14 nv miyaji add start
                    '--次予約取得
                    Dim tblNext As DataTable = Dao.ToolReserve.GetNextReserve(row.ASSET_NO, lReserveId, row.REAL_END_DT, WorkFlg.Jitsubi)

                    '次の予約がない場合、エラー
                    If tblNext Is Nothing OrElse tblNext.Rows.Count = 0 Then
                        model.ErrCode = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NO_NEXT_RESERVE), row.ASSET_NO)
                        Return Json(model)
                    End If

                    '２明細以上ある場合、最初の明細の次の予約の担当者が他の明細の次予約担当者と違う場合、エラー
                    If sNextEmployeeCd = String.Empty Then
                        sNextEmployeeCd = tblNext.Rows(0).Item("EMPLOYEE_CD")
                    Else
                        If tblNext.Rows(0).Item("EMPLOYEE_CD") <> sNextEmployeeCd Then
                            model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECTED_DEF_HIKITSUGI_EMPLOYEE)
                            Return Json(model)
                        End If
                    End If

                    '次予約の担当者コードをセット
                    model.HikiEmployeeCd = tblNext.Rows(0).Item("EMPLOYEE_CD")
                    '--2021/12/14 nv miyaji add end
                End If
            Next

            'IDをキーにモデルを取得する
            Dim modelEmployee As EmployeeModel = db.Employee.Find(model.HikiEmployeeCd)
            '次予約の担当者名をセット
            model.HikiEmployeeNm = modelEmployee.EMPLOYEE_NAME

            Return Json(model)

        End Function
#End Region

#Region "引継ぎモーダルの変更ボタン押下処理(AJax)"
        ''' <summary>
        ''' 引継ぎモーダルの変更ボタン押下処理(AJax)
        ''' </summary>
        ''' <param name="lReserveId">予約番号</param>
        ''' <param name="sHikiDt">受渡日</param>
        ''' <param name="sEmployeeCd">担当者コード</param>
        ''' <param name="lstAsset">引継する資産番号のリスト</param>
        ''' <returns>View表示内容</returns>
        Public Function SetHikitsugi(lReserveId As Long, sHikiDt As String, ByVal sEmployeeCd As String, lstAsset As List(Of String)) As JsonResult
            Dim db As New SystemDBContext
            Dim model As New LendingEntryViewModel  '返却用モデル

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            '稼働日考慮フラグ
            Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
            Dim sWorkDayFlg As String = reservationFunction.DATA2
            ''稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            'Dim sWorkFlg As String = WorkFlg.Jitsubi
            'If sWorkDayFlg = WorkDayFlg.Yes Then
            '    sWorkFlg = WorkFlg.Kadobi
            'End If

            '次予約情報格納用
            Dim tblNextReserve As New DataTable

            '受渡日
            Dim iHikiDate As Integer = CInt(CDate(sHikiDt).ToString("yyyyMMdd"))

            'エラーチェック
            Dim isFirst As Boolean = True   '最初フラグ
            For Each row As ReserveDetailViewModel In lstGrid
                If lstAsset.IndexOf(row.ASSET_NO) > -1 Then
                    'エラーチェック

                    '--次予約取得
                    Dim tblNext As DataTable = Dao.ToolReserve.GetNextReserve(row.ASSET_NO, lReserveId, row.REAL_END_DT, WorkFlg.Jitsubi)   '2021/12/06 nv miyaji
                    'Dim tblNext As DataTable = Dao.ToolReserve.GetNextReserve(row.ASSET_NO, lReserveId, row.REAL_END_DT, sWorkFlg)   '2021/12/06 nv miyaji

                    '次の予約がない場合、エラー
                    If tblNext Is Nothing OrElse tblNext.Rows.Count = 0 Then
                        model.ErrCode = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NO_NEXT_RESERVE), row.ASSET_NO)
                        Return Json(model)
                    End If

                    '次の予約の担当者が違う場合、エラー
                    If tblNext.Rows(0).Item("EMPLOYEE_CD") <> sEmployeeCd Then
                        model.ErrCode = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_SAME_TANTO_NEXT_RESERVE), row.ASSET_NO)
                        Return Json(model)
                    End If

                    '実質貸出期間From≦受渡日≦戻り値.終了日でなければエラー
                    If Not (row.REAL_START_DT <= iHikiDate AndAlso iHikiDate < tblNext(0)("END_DT")) Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_UKEWATASHI_DAY)
                        Return Json(model)
                    End If

                    '次予約情報を追加
                    '資産番号を入れる列を追加する。
                    Dim dc = New DataColumn
                    dc.ColumnName = "AssetNo"
                    dc.DefaultValue = row.ASSET_NO
                    dc.DataType = Type.GetType("System.String")
                    tblNext.Columns.Add(dc)
                    If isFirst Then
                        tblNextReserve = tblNext.Clone()
                        isFirst = False
                    End If
                    tblNextReserve.ImportRow(tblNext(0))

                End If
            Next

            'リストの値を更新
            Dim lstNew As New List(Of ReserveDetailViewModel)
            For Each row As ReserveDetailViewModel In lstGrid
                If lstAsset.IndexOf(row.ASSET_NO) > -1 Then
                    '次予約情報の該当行を取得
                    Dim rowNext As DataRow() = tblNextReserve.Select("AssetNo = '" & row.ASSET_NO & "'")

                    '引継元データ
                    row.RETURN_DT = iHikiDate
                    'row.RETURN_DT = row.REAL_END_DT
                    row.REAL_END_DT = iHikiDate
                    row.PR_END_DT = iHikiDate
                    row.HANDOVER_DIV = HandOverDiv.HikitsugiMoto
                    row.HANDOVER_RESERVE_ID = rowNext(0)("RESERVE_ID")
                    row.RESERVE_STATUS = ReserveStatus.HenkyakuSumi
                    row.LeaseKigen = CommonConst.MAX_KIGENBI_STR
                    row.RealEndDtString = CommonRfid.ToStringDateYMD(row.REAL_END_DT, "yyyyMMdd")
                    row.NextRealStartDt = rowNext(0)("ST_DT")

                    lstNew.Add(row)

                    '受渡日＜戻り値.開始日-1の場合に登録する。
                    If row.REAL_END_DT < CInt(CDate(CommonRfid.ToStringDateYMD(rowNext(0)("ST_DT"), "yyyyMMdd")).AddDays(-1).ToString("yyyyMMdd")) Then
                        'IDをキーに社員モデルを取得する
                        Dim modelEmployee As EmployeeModel = db.Employee.Find(rowNext(0)("EMPLOYEE_CD"))

                        '引継ぎ先データ作成
                        Dim rowNew As New ReserveDetailViewModel
                        rowNew.RESERVE_ID = row.RESERVE_ID
                        rowNew.ASSET_NO = row.ASSET_NO
                        rowNew.AssetName = row.AssetName
                        rowNew.REAL_START_DT = CInt(CDate(sHikiDt).AddDays(1).ToString("yyyyMMdd"))
                        rowNew.REAL_END_DT = CInt(rowNext(0)("LIMIT_DT"))
                        rowNew.CHARGE_EMP_CD = modelEmployee.EMPLOYEE_CD
                        rowNew.LENDING_DT = rowNew.REAL_START_DT
                        rowNew.RETURN_DT = Nothing
                        rowNew.PR_ST_DT = rowNew.REAL_START_DT
                        rowNew.PR_END_DT = rowNew.REAL_END_DT
                        rowNew.LEASE_CHG_TIMES = 0
                        rowNew.HANDOVER_DIV = HandOverDiv.HikitsugiSaki
                        rowNew.HANDOVER_RESERVE_ID = rowNext(0)("RESERVE_ID")
                        rowNew.RESERVE_STATUS = ReserveStatus.Lending
                        rowNew.SerialNo = row.SerialNo
                        rowNew.LeaseKigen = CommonConst.MAX_KIGENBI_STR
                        rowNew.EmployeeName = modelEmployee.EMPLOYEE_NAME
                        rowNew.RealStartDtString = CommonRfid.ToStringDateYMD(rowNew.REAL_START_DT, "yyyyMMdd")
                        rowNew.RealEndDtString = CommonRfid.ToStringDateYMD(rowNew.REAL_END_DT, "yyyyMMdd")
                        rowNew.NextRealStartDt = rowNext(0)("ST_DT")

                        lstNew.Add(rowNew)
                    End If
                Else
                    lstNew.Add(row)
                End If

            Next

            'グリッド情報をセッションに保持
            Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO) = lstNew

            'ビューモデルにグリッド情報を設定
            model.ReserveDetailData = lstNew

            Return Json(model)

        End Function
#End Region

#Region "個別期間延長ボタン押下のエラーチェック(AJax)"
        ''' <summary>
        ''' 個別期間延長ボタン押下のエラーチェック(AJax)
        ''' </summary>
        ''' <param name="lstAsset">更新する資産番号のリスト</param>
        ''' <returns>View表示内容</returns>
        Public Function KobetsuKikanEnchoCHeck(lstAsset As List(Of String)) As JsonResult
            Dim db As New SystemDBContext
            Dim model As New LendingEntryViewModel  '返却用モデル

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            'エラーチェック
            'IDをキーにモデルを取得する
            Dim modelHed As LendingEntryModel = db.LendingEntry.Find(lstGrid(0).RESERVE_ID)
            For Each row As ReserveDetailViewModel In lstGrid
                If lstAsset.IndexOf(row.ASSET_NO) > -1 Then
                    '引継のデータの場合、エラー
                    If row.HANDOVER_RESERVE_ID IsNot Nothing Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECTED_STATUS_HIKITSUGI)
                        Return Json(model)
                    End If
                    '返却済のデータの場合、エラー
                    If row.RESERVE_STATUS = ReserveStatus.HenkyakuSumi Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECTED_STATUS_RETURNED)
                        Return Json(model)
                    End If
                End If
            Next

            Return Json(model)

        End Function
#End Region

#Region "個別期間延長モーダルの変更ボタン押下処理(AJax)"
        ''' <summary>
        ''' 個別期間延長モーダルの変更ボタン押下処理(AJax)
        ''' </summary>
        ''' <param name="sFromDt">貸出開始日</param>
        ''' <param name="sToDt">貸出終了日</param>
        ''' <param name="lstAsset">更新する資産番号のリスト</param>
        ''' <param name="sAuthProcFlg">画面更新権限</param>
        ''' <param name="sLeaseUse">貸出用途</param>
        ''' <param name="sMngLocLCd">管理ロケーションコード</param>
        ''' <returns>View表示内容</returns>
        Public Function SetKobetsuRealEndDt(sFromDt As String, sToDt As String, lstAsset As List(Of String), sAuthProcFlg As String, sLeaseUse As String, sMngLocLCd As String) As ActionResult
            Dim db As New SystemDBContext
            Dim model As New LendingEntryViewModel  '返却用モデル

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            'エラーチェック
            'IDをキーにモデルを取得する
            Dim modelHed As LendingEntryModel = db.LendingEntry.Find(lstGrid(0).RESERVE_ID)
            'データベースの明細のモデルを取得
            Dim lstReserveDetailOld As New List(Of ReserveDetailViewModel)
            '予約明細テーブルを取得
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2021/12/07 nv miyaji add
            'T_RESERVE_DETAILテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT RESERVE.*")
            sbSQL.AppendLine("       ,ASSET.ASSET_NAME As AssetName")
            sbSQL.AppendLine("       ,ASSET.FREE_ITEM10 As SerialNo")
            sbSQL.AppendLine("       ,EMP.EMPLOYEE_NAME As EmployeeName")
            sbSQL.AppendLine("FROM T_RESERVE_DETAIL RESERVE")
            sbSQL.AppendLine("INNER JOIN M_ASSET ASSET")
            sbSQL.AppendLine("ON ASSET.ASSET_NO = RESERVE.ASSET_NO ")
            sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE EMP")
            sbSQL.AppendLine("ON EMP.EMPLOYEE_CD = RESERVE.CHARGE_EMP_CD ")
            sbSQL.AppendLine("WHERE")
            '--2021/12/07 nv miyaji update start
            sbSQL.AppendLine("RESERVE.RESERVE_ID = @ReserveId")
            params.Add(New SqlParameter("@ReserveId", lstGrid(0).RESERVE_ID))
            'sbSQL.AppendLine("RESERVE.RESERVE_ID = '" & lstGrid(0).RESERVE_ID & "'")
            '--2021/12/07 nv miyaji update end
            sbSQL.AppendLine("ORDER BY RESERVE.ASSET_NO")
            'SQLを実行する
            lstReserveDetailOld = db.ReserveDetailView.SqlQuery(sbSQL.ToString, params.ToArray).ToList  '2021/12/07 nv miyaji
            'lstReserveDetailOld = db.ReserveDetailView.SqlQuery(sbSQL.ToString).ToList  '2021/12/07 nv miyaji

            For Each row As ReserveDetailViewModel In lstGrid
                If lstAsset.IndexOf(row.ASSET_NO) > -1 Then
                    '既存のデータベースから該当資産情報を取得
                    Dim lstModelSelDetail As ReserveDetailViewModel() = (From motoOld In lstReserveDetailOld
                                                                         Where (motoOld.ASSET_NO = row.ASSET_NO)
                                                                         Order By motoOld.ASSET_NO).ToArray
                    '画面項目がロックされている場合のみ、最大期間延長回数チェックする。
                    If sAuthProcFlg = AuthProcFlg.JiAll Then
                        '設定Mの期間延長回数　＜　明細の期間延長変更回数の時、エラー(データベースの貸出期間To＜貸出終了日の時のみチェックする)
                        If lstModelSelDetail.Count > 0 AndAlso lstModelSelDetail(0).REAL_END_DT < CInt(CDate(sToDt).ToString("yyyyMMdd")) Then
                            '期間延長回数取得
                            Dim setting_KikanEnchoSuu As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_ChangedTimes.KeyDiv, CommonSettingKey.Lending_ChangedTimes.Key1, CommonSettingKey.Lending_ChangedTimes.Key2, CommonSettingKey.Lending_ChangedTimes.Key3, True)
                            Dim sKikanEnchoSuu As String = setting_KikanEnchoSuu.DATA1
                            '期間延長回数チェック
                            If sKikanEnchoSuu <> 0 AndAlso sKikanEnchoSuu < row.LEASE_CHG_TIMES + 1 Then
                                model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_KIKAN_ENCHO_OVER)
                                Return Json(model)
                            End If
                        End If
                    End If
                    '実貸出開始日　＞　実貸出終了日の時、エラー
                    If CInt(CDate(sFromDt).ToString("yyyyMMdd")) > CInt(CDate(sToDt).ToString("yyyyMMdd")) Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_REAL_END_DT_KAKOBI)
                        Return Json(model)
                    End If
                    '予約期間の重複確認
                    If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_START_DT, CInt(CDate(sToDt).ToString("yyyyMMdd"))) > 0 Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_MAINTE_DT_JUUFUKU)
                        Return Json(model)
                    End If
                    '非稼働日の場合、エラー
                    If Dao.ToolReserve.GetNotWorkFlg(CInt(CDate(sToDt).ToString("yyyyMMdd"))) = NotWorkFlg.Yes Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_NOT_WORK_FRG_YES)
                        Return Json(model)
                    End If
                    '貸出期間チェック
                    '一般者の場合のみ、チェックする。
                    'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
                    _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
                    If IsNothing(_LoginInfo) Then
                        '結果表示
                        Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
                    End If
                    '--2021/12/16 nv miyaji update start
                    '一般者か又は管理者でもU_HUB権限があり自拠点でない場合のチェックが必要かを取得
                    Dim isNeedCheck As Boolean = GetIsNeedCheck(_LoginInfo, sMngLocLCd)
                    If isNeedCheck Then
                        'If _LoginInfo.KengenKbn = AUTH_DIV.Nomal Then
                        '--2021/12/16 nv miyaji update end
                        Dim iTargetDate As Integer = 0

                        Dim iCnt As Integer = 1
                        While iCnt > 0
                            Dim Youto As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method.KeyDiv, iCnt, CommonSettingKey.Reservation_Method.Key2, CommonSettingKey.Reservation_Method.Key3)
                            If Youto Is Nothing Then
                                iCnt = 0
                            Else
                                If sLeaseUse = Youto.DATA1 Then
                                    '対象日付取得(貸出可能期間)
                                    If lstModelSelDetail(0).REAL_END_DT - lstModelSelDetail(0).REAL_START_DT >= CInt(CDate(sToDt).ToString("yyyyMMdd")) - CInt(CDate(sFromDt).ToString("yyyyMMdd")) Then
                                        '期間が変わらないまたは縮んだ場合、貸出用途の内容２-1と3で実質貸出開始日～終了日チェック
                                        iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(lstModelSelDetail(0).REAL_START_DT, "yyyyMMdd")), Youto.DATA2 - 1, Youto.DATA3)  '2021/12/07 nv miyaji 仕様変更
                                    Else
                                        '期間が延びた場合、現状の内容5と6でチェック
                                        iTargetDate = Dao.ToolCommon.GetTargetDate(CDate(CommonRfid.ToStringDateYMD(lstModelSelDetail(0).REAL_END_DT, "yyyyMMdd")), Youto.DATA5, Youto.DATA6)  '2021/12/07 nv miyaji 仕様変更
                                    End If
                                    If iTargetDate = 0 Then
                                        '0が帰ってきた場合、エラーメッセージを出す。
                                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_DATE_SETTEIGAI)
                                        Return Json(model)
                                    End If
                                    Exit While
                                End If
                                iCnt += 1
                            End If
                        End While

                        If CInt(CDate(sToDt).ToString("yyyyMMdd")) > iTargetDate Then
                            model.ErrCode = String.Format(Me._Message.getMessage(CommonLabel.MSG_ERR_REAL_DT_TOO_LONG), CommonRfid.ToStringDateYMD(iTargetDate, "yyyyMMdd"))
                            Return Json(model)
                        End If
                    End If
                End If
            Next

            '2021/11/11 upd nv miyaji start
            ''返却リードタイム
            'Dim setting_HenkyakuLT As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Return_LeadTime.KeyDiv, CommonSettingKey.Return_LeadTime.Key1, CommonSettingKey.Return_LeadTime.Key2, CommonSettingKey.Return_LeadTime.Key3, True)
            '稼働日考慮フラグ
            Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
            Dim sWorkDayFlg As String = reservationFunction.DATA2
            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If
            'Dim iHenkyakuLT As Integer = CInt(setting_KikanEnchoSuu.DATA1)
            ''稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            'Dim sWorkFlg As String = WorkFlg.Jitsubi
            'If setting_KikanEnchoSuu.DATA2 = WorkDayFlg.Yes Then
            '    sWorkFlg = WorkFlg.Kadobi
            'End If
            '2021/11/11 upd nv miyaji end 

            'リストの値を更新
            For Each row As ReserveDetailViewModel In lstGrid
                If lstAsset.IndexOf(row.ASSET_NO) > -1 Then
                    '期限日
                    row.LeaseKigen = CommonConst.MAX_KIGENBI_STR
                    If sFromDt <> String.Empty AndAlso sToDt <> String.Empty Then
                        '開始日と終了日に両方値がある場合、資産メンテナンス登録情報取得
                        Dim tblInfo As DataTable = Dao.ToolReserve.GetReserveMainteInfo(row.ASSET_NO, row.RESERVE_ID, CInt(CDate(sFromDt).ToString("yyyyMMdd")), CInt(CDate(sToDt).ToString("yyyyMMdd")), sWorkFlg)
                        If tblInfo IsNot Nothing AndAlso tblInfo.Rows.Count > 0 AndAlso tblInfo.Rows(0).Item("LIMIT_DT") <> CommonConst.MAX_KIGENBI AndAlso tblInfo.Rows(0).Item("LIMIT_DT") >= CInt(CDate(sFromDt).ToString("yyyyMMdd")) Then
                            row.LeaseKigen = CommonRfid.ToStringDateYMD(tblInfo.Rows(0).Item("LIMIT_DT"), "yyyyMMdd")
                        End If
                    End If

                    row.REAL_END_DT = CInt(CDate(sToDt).ToString("yyyyMMdd"))
                    row.PR_END_DT = CInt(CDate(sToDt).ToString("yyyyMMdd"))
                    row.RealEndDtString = CommonRfid.ToStringDateYMD(row.REAL_END_DT, "yyyyMMdd")

                    '実質貸出開始日と実質貸出終了日に値が入っている時
                    If row.REAL_START_DT <> 0 AndAlso row.REAL_END_DT <> 0 AndAlso row.REAL_START_DT <= row.REAL_END_DT Then
                        '予約期間の重複確認(From)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_START_DT, row.REAL_START_DT) > 0 Then
                            row.JuufukuAriFrom = True
                        Else
                            row.JuufukuAriFrom = False
                        End If
                        '予約期間の重複確認(From+1～To-1)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, CInt(CDate(sFromDt).AddDays(1).ToString("yyyyMMdd")), CInt(CDate(sToDt).AddDays(-1).ToString("yyyyMMdd"))) > 0 Then
                            row.JuufukuAriNaka = True
                        Else
                            row.JuufukuAriNaka = False
                        End If
                        '予約期間の重複確認(To)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_END_DT, row.REAL_END_DT) > 0 Then
                            row.JuufukuAriTo = True
                        Else
                            row.JuufukuAriTo = False
                        End If
                    End If
                End If
            Next

            'グリッド情報をセッションに保持
            Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO) = lstGrid

            'ビューモデルにグリッド情報を設定
            model.ReserveDetailData = lstGrid

            Return Json(model)

        End Function
#End Region

#Region "一覧削除ボタン押下処理(AJax)"
        ''' <summary>
        ''' 一覧削除ボタン押下処理(AJax)
        ''' </summary>
        ''' <param name="lstAsset">削除する資産番号のリスト</param>
        ''' <returns>View表示内容</returns>
        Public Function DeleteMeisai(lstAsset As List(Of String)) As JsonResult
            Dim db As New SystemDBContext
            Dim model As New LendingEntryViewModel  '返却用モデル

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'エラーチェック
            'IDをキーにモデルを取得する
            For Each row As ReserveDetailViewModel In lstGrid
                If lstAsset.IndexOf(row.ASSET_NO) > -1 Then
                    '未貸出以外のデータがある場合、エラー
                    If row.RESERVE_STATUS <> ReserveStatus.Mi Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECTED_NOT_STATUS_MI)
                        Return Json(model)
                    End If
                End If
            Next

            '引数の資産番号を削除した新しいリストを作成する。
            Dim lstNew As New List(Of ReserveDetailViewModel)
            For Each row As ReserveDetailViewModel In lstGrid
                If lstAsset.IndexOf(row.ASSET_NO) = -1 Then
                    lstNew.Add(row)
                End If
            Next

            'グリッド情報をセッションに保持
            Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO) = lstNew

            'ビューモデルにグリッド情報を設定
            model.ReserveDetailData = lstNew

            Return Json(model)

        End Function
#End Region

#Region "資産追加ボタン押下処理(AJax)"
        ''' <summary>
        ''' 資産追加ボタン押下処理(AJax)
        ''' </summary>
        ''' <param name="sAddAssetNo">追加する資産番号</param>
        ''' <param name="sFromDt">画面表示の実貸出開始日</param>
        ''' <param name="sToDt">画面表示の実貸出終了日</param>
        ''' <returns>View表示内容</returns>
        Public Function AddMeisai(lReserveId As Long, sAddAssetNo As String, sFromDt As String, sToDt As String, sMngLocLCd As String) As JsonResult
            Dim db As New SystemDBContext
            Dim model As New LendingEntryViewModel  '返却用モデル

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            '空白をトリムする
            sAddAssetNo = sAddAssetNo.Trim

            'IDをキーに資産モデルを取得する
            Dim modelAsset As AssetListModel = db.AssetList.Find(sAddAssetNo)

            'モデルがない場合、エラーメッセージ
            If modelAsset Is Nothing Then
                model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
                Return Json(model)
            End If

            '既に同じ資産番号が追加されている場合、エラーメッセージ
            For Each row As ReserveDetailViewModel In lstGrid
                If row.ASSET_NO = sAddAssetNo Then
                    model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECTED_SAME_ASSET_NO)
                    Return Json(model)
                End If
            Next

            '入力した資産の拠点が違う場合、エラーメッセージ
            If modelAsset.MNG_LOC_L_CD <> sMngLocLCd Then
                model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_ASSET_MNG_LOC_L_CD)
                Return Json(model)
            End If

            '入力した資産が貸出不可状態の場合、エラーメッセージ
            If modelAsset.LEASE_STATUS = AssetLeaseStatus.Ng Then
                model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_STATUS_FUKA)
                Return Json(model)
            End If

            '入力した資産の資産状態が「保管中」「貸出中」以外の場合、エラーメッセージ
            If modelAsset.ASSET_STATUS <> ASSET_STATUS.Storage AndAlso modelAsset.ASSET_STATUS <> ASSET_STATUS.Lending Then
                model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_INPUT_ASSET_STATUS)
                Return Json(model)
            End If

            '予約上限数
            Dim ReserveJougenSuu As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_Number_Items.KeyDiv, CommonSettingKey.Lending_Number_Items.Key1, CommonSettingKey.Lending_Number_Items.Key2, CommonSettingKey.Lending_Number_Items.Key3, True)
            Dim iJogenSuu As Integer = ReserveJougenSuu.DATA1
            '明細数が設定Mの予約上限数に達しているの場合、エラーメッセージ
            If iJogenSuu = lstGrid.Count Then
                model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_OVER_MAX_RESERVE_SUU)
                Return Json(model)
            End If

            'IDをキーに予約モデルを取得する
            Dim modelEntry As LendingEntryModel = db.LendingEntry.Find(lReserveId)

            '追加する明細情報を作成し、グリッドリストに追加する。
            Dim rowData As New ReserveDetailViewModel
            If modelEntry Is Nothing Then
                '新規登録時
                rowData.RESERVE_ID = 0
            Else
                '変更時
                rowData.RESERVE_ID = modelEntry.RESERVE_ID
            End If
            If String.IsNullOrEmpty(sFromDt) Then
                rowData.REAL_START_DT = 0
                rowData.PR_ST_DT = 0
            Else
                rowData.REAL_START_DT = CInt(CDate(sFromDt).ToString("yyyyMMdd"))
                rowData.PR_ST_DT = CInt(CDate(sFromDt).ToString("yyyyMMdd"))
            End If
            'If String.IsNullOrEmpty(sFromDt) Then  '2022/05/17 nv miyaji
            If String.IsNullOrEmpty(sToDt) Then  '2022/05/17 nv miyaji
                rowData.REAL_END_DT = 0
                rowData.PR_END_DT = 0
            Else
                rowData.REAL_END_DT = CInt(CDate(sToDt).ToString("yyyyMMdd"))
                rowData.PR_END_DT = CInt(CDate(sToDt).ToString("yyyyMMdd"))
            End If
            rowData.ASSET_NO = sAddAssetNo
            '--2021/12/24 nv miyaji update start
            If modelAsset.ASSET_NAME Is Nothing Then
                rowData.AssetName = String.Empty
            Else
                rowData.AssetName = modelAsset.ASSET_NAME
            End If
            'rowData.AssetName = modelAsset.ASSET_NAME
            '--2021/12/24 nv miyaji update end
            rowData.CHARGE_EMP_CD = String.Empty
            rowData.LENDING_DT = Nothing
            rowData.RETURN_DT = Nothing
            rowData.LEASE_CHG_TIMES = 0
            rowData.HANDOVER_DIV = HandOverDiv.Mi
            rowData.HANDOVER_RESERVE_ID = Nothing
            rowData.RESERVE_STATUS = ReserveStatus.Mi
            '--2021/12/24 nv miyaji update start
            If modelAsset.FREE_ITEM10 Is Nothing Then
                rowData.SerialNo = String.Empty
            Else
                rowData.SerialNo = modelAsset.FREE_ITEM10
            End If
            'rowData.SerialNo = modelAsset.FREE_ITEM10
            '--2021/12/24 nv miyaji update end
            rowData.LeaseKigen = String.Empty
            rowData.EmployeeName = String.Empty
            rowData.RealStartDtString = CommonRfid.ToStringDateYMD(rowData.REAL_START_DT, "yyyyMMdd")
            rowData.RealEndDtString = CommonRfid.ToStringDateYMD(rowData.REAL_END_DT, "yyyyMMdd")
            '実質貸出開始日と実質貸出終了日に値が入っている時
            rowData.JuufukuAriFrom = False  '初期値設定
            rowData.JuufukuAriNaka = False  '初期値設定
            rowData.JuufukuAriTo = False  '初期値設定
            If rowData.REAL_START_DT <> 0 AndAlso rowData.REAL_END_DT <> 0 AndAlso rowData.REAL_START_DT <= rowData.REAL_END_DT Then
                '予約期間の重複確認(From)
                If Dao.ToolReserve.GetReserveMainteJufukuSuu(rowData.ASSET_NO, rowData.RESERVE_ID, 0, rowData.REAL_START_DT, rowData.REAL_START_DT) > 0 Then
                    rowData.JuufukuAriFrom = True
                End If
                '予約期間の重複確認(From+1～To-1)
                If Dao.ToolReserve.GetReserveMainteJufukuSuu(rowData.ASSET_NO, rowData.RESERVE_ID, 0, CInt(CDate(sFromDt).AddDays(1).ToString("yyyyMMdd")), CInt(CDate(sToDt).AddDays(-1).ToString("yyyyMMdd"))) > 0 Then
                    rowData.JuufukuAriNaka = True
                End If
                '予約期間の重複確認(To)
                If Dao.ToolReserve.GetReserveMainteJufukuSuu(rowData.ASSET_NO, rowData.RESERVE_ID, 0, rowData.REAL_END_DT, rowData.REAL_END_DT) > 0 Then
                    rowData.JuufukuAriTo = True
                End If
                '期間From＝期間Toで、重複有りの時、「～」も赤にする。
                If sFromDt = sToDt AndAlso rowData.JuufukuAriFrom = True Then
                    rowData.JuufukuAriNaka = True
                End If
            End If
            lstGrid.Add(rowData)

            'グリッド情報をセッションに保持
            Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO) = lstGrid

            'ビューモデルにグリッド情報を設定
            model.ReserveDetailData = lstGrid

            Return Json(model)

        End Function
#End Region

#Region "実貸出開始日の変更時処理(AJax)"
        ''' <summary>
        ''' 実貸出開始日の変更時処理(AJax)
        ''' </summary>
        ''' <param name="sFromDt">貸出開始日</param>
        ''' <param name="sToDt">貸出終了日</param>
        ''' <param name="sUserToDt">ユーザー貸出終了日</param>
        ''' <returns>View表示内容</returns>
        Public Function SetRealStartDt(ByVal sFromDt As String, ByVal sToDt As String, ByVal sUserToDt As String) As JsonResult
            Dim db As New SystemDBContext
            Dim model As New LendingEntryViewModel  '返却用モデル

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            'グリッドの情報がない場合、グリッドの更新はしない。
            If lstGrid Is Nothing OrElse lstGrid.Count = 0 Then
                Return Json(model)
            End If

            '稼働日考慮フラグ
            Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
            Dim sWorkDayFlg As String = reservationFunction.DATA2
            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If

            'ユーザー貸出終了日を更新
            If sFromDt <> String.Empty AndAlso sUserToDt <> String.Empty AndAlso CInt(CDate(sUserToDt).ToString("yyyyMMdd")) < CInt(CDate(sFromDt).ToString("yyyyMMdd")) Then
                'ユーザー貸出終了日が貸出開始日より前になった場合、ユーザー貸出終了日に貸出開始日を入れる。
                model.UserEndDtString = sFromDt
            Else
                '--2021/12/02 nv miyaji add start
                'それ以外の場合、ユーザー貸出終了日に正しい値を入れる。
                '返却リードタイム
                Dim setting_HenkyakuLT As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Return_LeadTime.KeyDiv, CommonSettingKey.Return_LeadTime.Key1, CommonSettingKey.Return_LeadTime.Key2, CommonSettingKey.Return_LeadTime.Key3, True)
                Dim iHenkyakuLT As Integer = CInt(setting_HenkyakuLT.DATA1)
                '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                Dim sHenkyakuWorkFlg As String = WorkFlg.Jitsubi
                If setting_HenkyakuLT.DATA2 = WorkFlg.Kadobi Then
                    sHenkyakuWorkFlg = WorkFlg.Kadobi
                End If

                '1000年未満の場合エラー
                ' / を取り除いて数字だけにする
                Dim sCleanStr As String = System.Text.RegularExpressions.Regex.Replace(sToDt, "[^0-9]", "")
                Dim num As Integer

                If Integer.TryParse(sCleanStr, num) Then
                    If num < MIN_DATE Then
                        sToDt = String.Empty
                    End If
                End If

                '対象日付取得
                If sToDt = String.Empty Then
                    model.UserEndDtString = String.Empty
                Else
                    Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(CDate(sToDt), iHenkyakuLT, sHenkyakuWorkFlg)  '0が返ってくるはずがないので、0チェックはしない。
                    model.UserEndDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(iTargetDate))
                End If
                '--2021/12/02 nv miyaji add end
            End If

            'エラーチェック
            For Each row As ReserveDetailViewModel In lstGrid
                'エラーメッセージを返すようなエラーチェックは特になし。
            Next

            'リストの値を更新
            For Each row As ReserveDetailViewModel In lstGrid
                row.JuufukuAriFrom = False  '初期値設定
                row.JuufukuAriNaka = False  '初期値設定
                row.JuufukuAriTo = False  '初期値設定
                If row.RESERVE_STATUS <> ReserveStatus.HenkyakuSumi Then
                    '期限日
                    row.LeaseKigen = CommonConst.MAX_KIGENBI_STR
                    If sFromDt <> String.Empty AndAlso sToDt <> String.Empty Then
                        '開始日と終了日に両方値がある場合、資産メンテナンス登録情報取得
                        Dim tblInfo As DataTable = Dao.ToolReserve.GetReserveMainteInfo(row.ASSET_NO, row.RESERVE_ID, CInt(CDate(sFromDt).ToString("yyyyMMdd")), CInt(CDate(sToDt).ToString("yyyyMMdd")), sWorkFlg)
                        If tblInfo IsNot Nothing AndAlso tblInfo.Rows.Count > 0 AndAlso tblInfo.Rows(0).Item("LIMIT_DT") <> CommonConst.MAX_KIGENBI AndAlso tblInfo.Rows(0).Item("LIMIT_DT") >= CInt(CDate(sFromDt).ToString("yyyyMMdd")) Then
                            row.LeaseKigen = CommonRfid.ToStringDateYMD(tblInfo.Rows(0).Item("LIMIT_DT"), "yyyyMMdd")
                        End If
                    End If

                    '実質貸出開始日と表示文字列をセット
                    If sFromDt = String.Empty Then
                        '実質貸出開始日
                        row.REAL_START_DT = 0
                        '貸出予実開始日
                        row.PR_ST_DT = 0
                        '実貸出開始日表示文字列
                        row.RealStartDtString = String.Empty
                    Else
                        '実質貸出開始日
                        row.REAL_START_DT = CInt(CDate(sFromDt).ToString("yyyyMMdd"))
                        '貸出予実開始日
                        row.PR_ST_DT = CInt(CDate(sFromDt).ToString("yyyyMMdd"))
                        '実貸出開始日表示文字列
                        row.RealStartDtString = CDate(sFromDt).ToString("yyyy/MM/dd")
                    End If

                    '実質貸出開始日と実質貸出終了日に値が入っている時
                    If row.REAL_START_DT <> 0 AndAlso row.REAL_END_DT <> 0 AndAlso row.REAL_START_DT <= row.REAL_END_DT Then
                        '予約期間の重複確認(From)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_START_DT, row.REAL_START_DT) > 0 Then
                            row.JuufukuAriFrom = True
                        End If
                        '予約期間の重複確認(From+1～To-1)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, CInt(CDate(sFromDt).AddDays(1).ToString("yyyyMMdd")), CInt(CDate(sToDt).AddDays(-1).ToString("yyyyMMdd"))) > 0 Then
                            row.JuufukuAriNaka = True
                        End If
                        '予約期間の重複確認(To)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_END_DT, row.REAL_END_DT) > 0 Then
                            row.JuufukuAriTo = True
                        End If
                        '期間From＝期間Toで、重複有りの時、「～」も赤にする。
                        If sFromDt = sToDt AndAlso row.JuufukuAriFrom = True Then
                            row.JuufukuAriNaka = True
                        End If
                    End If
                End If
            Next

            'グリッド情報をセッションに保持
            Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO) = lstGrid

            'ビューモデルにグリッド情報を設定
            model.ReserveDetailData = lstGrid

            Return Json(model)

        End Function
#End Region

#Region "実貸出終了日の変更時処理(AJax)"
        ''' <summary>
        ''' 実貸出終了日の変更時処理(AJax)
        ''' </summary>
        ''' <param name="sFromDt">貸出開始日</param>
        ''' <param name="sToDt">貸出終了日</param>
        ''' <param name="sAuthProcFlg">画面更新権限</param>
        ''' <returns>View表示内容</returns>
        Public Function SetRealEndDt(ByVal sFromDt As String, ByVal sToDt As String, sAuthProcFlg As String) As JsonResult
            Dim db As New SystemDBContext
            Dim model As New LendingEntryViewModel  '返却用モデル

            'グリッドにセットする実貸出終了日
            Dim sUpdEndDt As String = sToDt

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)

            ''グリッドの情報がない場合、グリッドの更新はしない。
            'If lstGrid Is Nothing OrElse lstGrid.Count = 0 Then
            '    Return Json(model)
            'End If

            Dim isReturn As Boolean = False 'グリッドを更新せずに戻るエラーがあるフラグ
            Dim iOldRealEndDt As Integer = 0
            Dim iOldUserEndDt As Integer = 0

            'エラーチェック
            If lstGrid.Count > 0 AndAlso lstGrid(0).RESERVE_ID <> 0 Then
                '変更処理の時だけエラーチェックする。

                'データベースの明細のモデルを取得
                Dim lstReserveDetailOld As New List(Of ReserveDetailViewModel)
                '予約明細テーブルを取得
                Dim sbSQL As StringBuilder = New StringBuilder
                Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2021/12/07 nv miyaji add
                'T_RESERVE_DETAILテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT RESERVE.*")
                sbSQL.AppendLine("       ,ASSET.ASSET_NAME As AssetName")
                sbSQL.AppendLine("       ,ASSET.FREE_ITEM10 As SerialNo")
                sbSQL.AppendLine("       ,EMP.EMPLOYEE_NAME As EmployeeName")
                sbSQL.AppendLine("FROM T_RESERVE_DETAIL RESERVE")
                sbSQL.AppendLine("INNER JOIN M_ASSET ASSET")
                sbSQL.AppendLine("ON ASSET.ASSET_NO = RESERVE.ASSET_NO ")
                sbSQL.AppendLine("LEFT JOIN M_EMPLOYEE EMP")
                sbSQL.AppendLine("ON EMP.EMPLOYEE_CD = RESERVE.CHARGE_EMP_CD ")
                sbSQL.AppendLine("WHERE")
                '--2021/12/07 nv miyaji update start
                sbSQL.AppendLine("RESERVE.RESERVE_ID = @ReserveId")
                params.Add(New SqlParameter("@ReserveId", lstGrid(0).RESERVE_ID))
                'sbSQL.AppendLine("RESERVE.RESERVE_ID = '" & lstGrid(0).RESERVE_ID & "'")
                '--2021/12/07 nv miyaji update start
                sbSQL.AppendLine("ORDER BY RESERVE.ASSET_NO")
                'SQLを実行する
                lstReserveDetailOld = db.ReserveDetailView.SqlQuery(sbSQL.ToString, params.ToArray).ToList  '2021/12/07 nv miyaji
                'lstReserveDetailOld = db.ReserveDetailView.SqlQuery(sbSQL.ToString).ToList '2021/12/07 nv miyaji

                'IDをキーにモデルを取得する
                Dim modelHed As LendingEntryModel = db.LendingEntry.Find(lstGrid(0).RESERVE_ID)
                iOldRealEndDt = modelHed.REAL_END_DT
                iOldUserEndDt = modelHed.USER_END_DT
                For Each row As ReserveDetailViewModel In lstGrid
                    '引継データがある場合、エラー
                    If row.HANDOVER_DIV <> HandOverDiv.Mi Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_KIKAN_ENCHO_HIKI)
                        sUpdEndDt = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(iOldRealEndDt))
                        isReturn = True
                        'Return Json(model)
                    End If

                    '返却データがある場合、エラー
                    If Not isReturn AndAlso row.RESERVE_STATUS = ReserveStatus.HenkyakuSumi Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_KIKAN_ENCHO_HENKYAKU)
                        sUpdEndDt = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(iOldRealEndDt))
                        isReturn = True
                        'Return Json(model)
                    End If

                    '予約ヘッダの期間延長変更回数　＜　明細の期間延長変更回数の時、エラー
                    If Not isReturn AndAlso modelHed.LEASE_CHG_TIMES < row.LEASE_CHG_TIMES Then
                        model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_KIKAN_ENCHO_KOBETSU)
                        sUpdEndDt = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(iOldRealEndDt))
                        isReturn = True
                        'Return Json(model)
                    End If

                    '画面項目がロックされている場合のみ、最大期間延長回数チェックする。
                    If sAuthProcFlg = AuthProcFlg.JiAll Then
                        '設定Mの期間延長回数　＜　明細の期間延長変更回数の時、エラー(データベースの貸出期間To＜貸出終了日の時のみチェックする)
                        '既存のデータベースから該当資産情報を取得
                        Dim lstModelSelDetail As ReserveDetailViewModel() = (From motoOld In lstReserveDetailOld
                                                                             Where (motoOld.ASSET_NO = row.ASSET_NO)
                                                                             Order By motoOld.ASSET_NO).ToArray
                        If Not isReturn AndAlso lstModelSelDetail.Count > 0 AndAlso sUpdEndDt <> String.Empty AndAlso lstModelSelDetail(0).REAL_END_DT < CInt(CDate(sUpdEndDt).ToString("yyyyMMdd")) Then
                            '期間延長回数取得
                            Dim setting_KikanEnchoSuu As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Lending_ChangedTimes.KeyDiv, CommonSettingKey.Lending_ChangedTimes.Key1, CommonSettingKey.Lending_ChangedTimes.Key2, CommonSettingKey.Lending_ChangedTimes.Key3, True)
                            Dim sKikanEnchoSuu As String = setting_KikanEnchoSuu.DATA1
                            '期間延長回数チェック
                            If sKikanEnchoSuu <> 0 AndAlso sKikanEnchoSuu < row.LEASE_CHG_TIMES + 1 Then
                                model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_KIKAN_ENCHO_OVER)
                                model.RealEndDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(iOldRealEndDt))
                                sUpdEndDt = model.RealEndDtString
                                'Return Json(model)
                            End If
                        End If
                    End If

                Next

            End If

            '1000年未満の場合エラー
            ' / を取り除いて数字だけにする
            Dim sCleanStr As String = System.Text.RegularExpressions.Regex.Replace(sUpdEndDt, "[^0-9]", "")
            Dim num As Integer

            If Integer.TryParse(sCleanStr, num) Then
                If num < MIN_DATE Then
                    sUpdEndDt = String.Empty
                End If
            End If

            'ユーザー貸出終了日を設定
            If String.IsNullOrEmpty(sUpdEndDt) Then
                'ユーザー貸出終了日を更新
                model.UserEndDtString = String.Empty
            Else
                '返却リードタイム
                Dim setting_HenkyakuLT As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Return_LeadTime.KeyDiv, CommonSettingKey.Return_LeadTime.Key1, CommonSettingKey.Return_LeadTime.Key2, CommonSettingKey.Return_LeadTime.Key3, True)
                Dim iHenkyakuLT As Integer = CInt(setting_HenkyakuLT.DATA1)
                '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
                Dim sHenkyakuWorkFlg As String = WorkFlg.Jitsubi
                If setting_HenkyakuLT.DATA2 = WorkFlg.Kadobi Then
                    sHenkyakuWorkFlg = WorkFlg.Kadobi
                End If

                '対象日付取得
                Dim iTargetDate As Integer = Dao.ToolCommon.GetTargetDate(CDate(sUpdEndDt), iHenkyakuLT, sHenkyakuWorkFlg)
                '--2021/12/02 nv miyaji update start
                If iTargetDate = 0 Then
                    '0が帰ってきた場合、エラーメッセージを出す。
                    'isReturn = True    '2022/06/16 nv miyaji グリッドの貸し出し終了日も更新したいので、コメントアウト。
                    If iOldRealEndDt = 0 Then
                        '新規登録画面の場合
                        sUpdEndDt = String.Empty
                        model.UserEndDtString = String.Empty
                    Else
                        '変更画面の場合
                        sUpdEndDt = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(iOldRealEndDt))
                        model.UserEndDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(iOldUserEndDt))
                    End If
                    model.RealEndDtString = sUpdEndDt   '2022/06/16 nv miyaji add  グリッドの貸し出し終了日をデータベースの値に戻す。
                    model.ErrCode = Me._Message.getMessage(CommonLabel.MSG_ERR_DATE_SETTEIGAI)
                Else
                    'ユーザー貸出終了日を更新
                    '対象日付が貸出開始日より前になった場合、ユーザー貸出終了日に貸出開始日を入れる。
                    If sFromDt <> String.Empty AndAlso iTargetDate < CInt(CDate(sFromDt).ToString("yyyyMMdd")) Then
                        iTargetDate = CInt(CDate(sFromDt).ToString("yyyyMMdd"))
                    End If
                    model.UserEndDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(iTargetDate))
                End If
                ''ユーザー貸出終了日を更新
                ''対象日付が貸出開始日より前になった場合、ユーザー貸出終了日に貸出開始日を入れる。
                'If sFromDt <> String.Empty AndAlso iTargetDate < CInt(CDate(sFromDt).ToString("yyyyMMdd")) Then
                '    iTargetDate = CInt(CDate(sFromDt).ToString("yyyyMMdd"))
                'End If
                'model.UserEndDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(iTargetDate))
                '--2021/12/02 nv miyaji update end

            End If

            'グリッドの明細がないか、グリッドを更新せずに戻るエラーがある場合
            If lstGrid.Count = 0 OrElse isReturn Then
                '実貸出終了日をデータベースの値に戻す。
                model.RealEndDtString = sUpdEndDt
                'グリッドを更新しない
                Return Json(model)
            End If

            '稼働日考慮フラグ
            Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
            Dim sWorkDayFlg As String = reservationFunction.DATA2
            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If

            'リストの値を更新
            For Each row As ReserveDetailViewModel In lstGrid
                row.JuufukuAriFrom = False  '初期値設定
                row.JuufukuAriNaka = False  '初期値設定
                row.JuufukuAriTo = False  '初期値設定
                If row.RESERVE_STATUS <> ReserveStatus.HenkyakuSumi Then
                    '期限日
                    row.LeaseKigen = CommonConst.MAX_KIGENBI_STR
                    If sFromDt <> String.Empty AndAlso sUpdEndDt <> String.Empty Then
                        '開始日と終了日に両方値がある場合、資産メンテナンス登録情報取得
                        Dim tblInfo As DataTable = Dao.ToolReserve.GetReserveMainteInfo(row.ASSET_NO, row.RESERVE_ID, CInt(CDate(sFromDt).ToString("yyyyMMdd")), CInt(CDate(sUpdEndDt).ToString("yyyyMMdd")), sWorkFlg)
                        If tblInfo IsNot Nothing AndAlso tblInfo.Rows.Count > 0 AndAlso tblInfo.Rows(0).Item("LIMIT_DT") <> CommonConst.MAX_KIGENBI AndAlso tblInfo.Rows(0).Item("LIMIT_DT") >= CInt(CDate(sFromDt).ToString("yyyyMMdd")) Then
                            row.LeaseKigen = CommonRfid.ToStringDateYMD(tblInfo.Rows(0).Item("LIMIT_DT"), "yyyyMMdd")
                        End If

                        If IsDBNull(tblInfo.Rows(0).Item("WARN_MAINTE_KBN")) Then
                            model.WarnMainteKbn = ""
                        ElseIf String.IsNullOrEmpty(tblInfo.Rows(0).Item("WARN_MAINTE_KBN")) Then
                            model.WarnMainteKbn = ""
                        Else
                            model.WarnMainteKbn = tblInfo.Rows(0).Item("WARN_MAINTE_KBN")
                        End If

                    End If

                    '実質貸出開始日と表示文字列をセット
                    If sUpdEndDt = String.Empty Then
                        '実貸出終了日
                        row.REAL_END_DT = 0
                        '貸出予実終了日
                        row.PR_END_DT = 0
                        '実貸出終了日表示文字列
                        row.RealEndDtString = String.Empty
                    Else
                        '実貸出終了日
                        row.REAL_END_DT = CInt(CDate(sUpdEndDt).ToString("yyyyMMdd"))
                        '貸出予実終了日
                        row.PR_END_DT = CInt(CDate(sUpdEndDt).ToString("yyyyMMdd"))
                        '実貸出終了日表示文字列
                        row.RealEndDtString = CDate(sUpdEndDt).ToString("yyyy/MM/dd")
                    End If


                    '実質貸出開始日と実質貸出終了日に値が入っている時
                    If row.REAL_START_DT <> 0 AndAlso row.REAL_END_DT <> 0 AndAlso row.REAL_START_DT <= row.REAL_END_DT Then
                        '予約期間の重複確認(From)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_START_DT, row.REAL_START_DT) > 0 Then
                            row.JuufukuAriFrom = True
                        End If
                        '予約期間の重複確認(From+1～To-1)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, CInt(CDate(sFromDt).AddDays(1).ToString("yyyyMMdd")), CInt(CDate(sUpdEndDt).AddDays(-1).ToString("yyyyMMdd"))) > 0 Then
                            row.JuufukuAriNaka = True
                        End If
                        '予約期間の重複確認(To)
                        If Dao.ToolReserve.GetReserveMainteJufukuSuu(row.ASSET_NO, row.RESERVE_ID, 0, row.REAL_END_DT, row.REAL_END_DT) > 0 Then
                            row.JuufukuAriTo = True
                        End If

                        '期間From＝期間Toで、重複有りの時、「～」も赤にする。
                        If sFromDt = sUpdEndDt AndAlso row.JuufukuAriFrom = True Then
                            row.JuufukuAriNaka = True
                        End If
                    End If

                End If
            Next

            'グリッド情報をセッションに保持
            Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO) = lstGrid

            'ビューモデルにグリッド情報を設定
            model.ReserveDetailData = lstGrid

            Return Json(model)

        End Function
#End Region

#Region "送り先(貸出先)、返送先プルダウン変更時処理(AJax)"
        ''' <summary>
        ''' 送り先(貸出先)、返送先プルダウン変更時処理(AJax)
        ''' </summary>
        ''' <param name="sLeasePlaseCd">貸出先コード</param>
        ''' <returns>View表示内容</returns>
        Public Function GetLeasePlaceInfo(ByVal sLeasePlaseCd As String) As JsonResult
            Dim db As New SystemDBContext
            Dim model As New LendingEntryViewModel  '返却用モデル

            '設定マスタに登録された貸出先コードの場合は、住所などすべて""で返却する。
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim settings As SettingInfo() = Me._Message.getSettingList(CommonSettingKey.Destination_Manual_Rrgist.KeyDiv)
            If settings IsNot Nothing Then
                For Each s In settings
                    If sLeasePlaseCd.Equals(s.DATA1.Trim) Then
                        Return Json(model)
                    End If
                Next
            End If

            'IDをキーにモデルを取得する
            Dim modelLease As LeasePlaceModel = db.LeasePlace.Find(sLeasePlaseCd)
            '貸出先名
            If modelLease.LEASE_PLACE_NAME.Length < 17 Then
                model.DEST_NAME1 = modelLease.LEASE_PLACE_NAME
            ElseIf modelLease.LEASE_PLACE_NAME.Length < 33 Then
                '17桁より大きく33桁より小さい場合は２分割する。
                model.DEST_NAME1 = modelLease.LEASE_PLACE_NAME.Substring(0, 16)
                model.DEST_NAME2 = modelLease.LEASE_PLACE_NAME.Substring(16)
            Else
                '33桁以上の場合は32桁目までを表示する。
                model.DEST_NAME1 = modelLease.LEASE_PLACE_NAME.Substring(0, 16)
                model.DEST_NAME2 = modelLease.LEASE_PLACE_NAME.Substring(16, 16)
            End If
            '住所
            If modelLease.ADDRESS IsNot Nothing Then
                If modelLease.ADDRESS.Length < 17 Then
                    model.DEST_ADDRESS1 = modelLease.ADDRESS
                ElseIf modelLease.ADDRESS.Length < 33 Then
                    '17桁より大きく33桁より小さい場合は２分割する。
                    model.DEST_ADDRESS1 = modelLease.ADDRESS.Substring(0, 16)
                    model.DEST_ADDRESS2 = modelLease.ADDRESS.Substring(16)
                Else
                    '33桁以上の場合は３分割する。
                    model.DEST_ADDRESS1 = modelLease.ADDRESS.Substring(0, 16)
                    model.DEST_ADDRESS2 = modelLease.ADDRESS.Substring(16, 16)
                    model.DEST_ADDRESS3 = modelLease.ADDRESS.Substring(32)
                End If
            End If
            model.DEST_ZIP_CODE = modelLease.ZIP_CODE
            model.DEST_PHONE_NUMBER = modelLease.PHONE_NUMBER

            '2023/03/22 Kawamoto ADD START
            '貸出確認書宛名
            If modelLease.SEC_LEASE_PLACE_FLG = SecLeasePlaceFlg.Cd.No Then
                '二次貸出先フラグが"N"(二次貸出先ではない)の場合、貸出先名をセット
                model.TO_DEST_NAME = modelLease.LEASE_PLACE_NAME
            End If
            '2023/03/22 Kawamoto ADD END

            Return Json(model)

        End Function
#End Region

#Region "予約履歴画面に遷移します"
        ''' <summary>
        ''' 予約履歴画面に遷移します
        ''' </summary>
        <ValidateInput(False)>
        Public Function ShowLendingRireki(ByVal ViewModel As LendingEntryViewModel) As ActionResult
            'メンテナンスID
            Dim lReserveId As Long = Long.Parse(ViewModel.RESERVE_ID)

            'セッションに保存
            Session(CommonSessionKey.LENDING_HISTORY_INFO) = lReserveId

            '結果表示
            Return RedirectToAction("Index", "LendingHistory")
        End Function
#End Region

#Region "社員マスタドロップダウンリストデータ取得処理"
        ''' <summary>
        ''' 社員マスタドロップダウンリストデータ取得処理
        ''' </summary>
        Public Sub GetEmployeeData()

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '--2021/12/06 nv miyaji update start ログイン情報がない場合、落ちるので修正
            '権限情報の取得
            Dim authInfo As AuthInfo
            '特殊権限が存在するかどうかを取得
            Dim isExistAuthOption_ChgHub As Boolean = False
            If _LoginInfo IsNot Nothing Then
                authInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)
                isExistAuthOption_ChgHub = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_CHGHUB)
            End If
            ''権限情報の取得
            'Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)
            ''特殊権限が存在するかどうかを取得
            'Dim isExistAuthOption_ChgHub As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_CHGHUB)
            '--2021/12/06 nv miyaji update end

            Dim lstAll As List(Of EmployeeModel)
            Dim lstHub As List(Of EmployeeModel) = Nothing

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim db As SystemDBContext = New SystemDBContext

            'M_EMPLOYEEテーブルから取得するSQLを作成
            'sbSQL.AppendLine("SELECT *")
            'sbSQL.AppendLine(" FROM ")
            'sbSQL.AppendLine("  M_EMPLOYEE ")
            'sbSQL.AppendLine("WHERE ")
            'sbSQL.AppendLine("AUTH_DIV <> '" & Misc.AUTH_DIV.SuperAdmin & "'")
            'sbSQL.AppendLine("AND RETIRE_FLG = '" & RetireFlg.Cd.No & "'")
            'sbSQL.AppendLine("ORDER BY EMPLOYEE_CD")
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  EMPLOYEE_CD ")
            sbSQL.AppendLine("  ,EMPLOYEE_NAME ")
            sbSQL.AppendLine("  ,PASSWORD ")
            sbSQL.AppendLine("  ,DEPARTMENT_CD ")
            sbSQL.AppendLine("  ,AUTH_DIV ")
            sbSQL.AppendLine("  ,RETIRE_FLG ")
            sbSQL.AppendLine("  ,MAIL_ADDRESS ")
            sbSQL.AppendLine("  ,INS_DATETIME ")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  ,UPD_DATETIME ")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  ,UPD_TASK ")
            sbSQL.AppendLine("FROM( ")
            sbSQL.AppendLine("    SELECT M.* ")
            sbSQL.AppendLine("      ,ROW_NUMBER() OVER(PARTITION BY M.EMPLOYEE_CD ORDER BY ")
            sbSQL.AppendLine("        CASE WHEN MH.HUB_LOC_L_CD IN (SELECT HUB_LOC_L_CD FROM M_EMPLOYEE_HUB WHERE EMPLOYEE_CD = @EmployeeCd) THEN 0 ELSE 1 END ")
            sbSQL.AppendLine("        ,M.EMPLOYEE_CD) AS ROW_NUM")
            sbSQL.AppendLine("    FROM M_EMPLOYEE M")
            sbSQL.AppendLine("    LEFT JOIN M_EMPLOYEE_HUB MH")
            sbSQL.AppendLine("    ON M.EMPLOYEE_CD = MH.EMPLOYEE_CD")
            params.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("AUTH_DIV <> '" & Misc.AUTH_DIV.SuperAdmin & "'")
            sbSQL.AppendLine("AND RETIRE_FLG = '" & Misc.RetireFlg.Cd.No & "'")

            sbSQL.AppendLine("ORDER BY ")
            sbSQL.AppendLine("  CASE WHEN MH.HUB_LOC_L_CD IN (SELECT HUB_LOC_L_CD FROM M_EMPLOYEE_HUB WHERE EMPLOYEE_CD = @EmployeeCd) THEN 0 ELSE 1 END ")
            sbSQL.AppendLine("  ,M.EMPLOYEE_CD ")
            sbSQL.AppendLine("OFFSET 0 ROWS ")
            sbSQL.AppendLine(") AS T ")
            sbSQL.AppendLine("WHERE ROW_NUM = 1 ")
            'SQLを実行する
            'lstAll = db.Employee.SqlQuery(sbSQL.ToString).ToList
            lstAll = db.Employee.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            '-- 2021/12/05 upd nv Inoue start  全て、isExistAuthOption_ChgHub 用リストの作成
            If isExistAuthOption_ChgHub Then
                'M_EMPLOYEEテーブルから取得するSQLを作成
                sbSQL.Clear()
                params = New List(Of SqlParameter)

                'ChgHub時の社員一覧取得SQLを作成
                sbSQL.AppendLine("SELECT * FROM M_EMPLOYEE ")
                sbSQL.AppendLine("WHERE AUTH_DIV <> '" & Misc.AUTH_DIV.SuperAdmin & "'")
                sbSQL.AppendLine("AND RETIRE_FLG = '" & RetireFlg.Cd.No & "'")
                sbSQL.AppendLine("AND (")       ' start
                If _ViewModel.OnCreteButton = CommonConst.KEY_ON_UPDATE_CLICK Then
                    sbSQL.AppendLine("     EMPLOYEE_CD IN (@EmpCd1, @EmpCd2) or ")
                    params.Add(New SqlParameter($"@EmpCd1", _ViewModel.REQ_EMP_CD))
                    params.Add(New SqlParameter($"@EmpCd2", _ViewModel.CHARGE_EMP_CD))
                End If
                sbSQL.AppendLine("     EXISTS")
                sbSQL.AppendLine("     (")
                sbSQL.AppendLine("       SELECT 1 FROM M_EMPLOYEE_HUB")
                sbSQL.AppendLine("       WHERE M_EMPLOYEE_HUB.EMPLOYEE_CD = M_EMPLOYEE.EMPLOYEE_CD")
                sbSQL.AppendLine("       AND EXISTS")
                sbSQL.AppendLine("       (")
                sbSQL.AppendLine("           SELECT 1 FROM M_EMPLOYEE_HUB TMPHUB")
                sbSQL.AppendLine("           WHERE TMPHUB.HUB_LOC_L_CD = M_EMPLOYEE_HUB.HUB_LOC_L_CD")
                sbSQL.AppendLine("           AND   TMPHUB.EMPLOYEE_CD = @EmployeeCd")
                sbSQL.AppendLine("       )")
                sbSQL.AppendLine("     )")
                sbSQL.AppendLine("   )")       ' end
                params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))

                sbSQL.AppendLine("ORDER BY EMPLOYEE_CD")
                'SQLを実行する
                lstHub = db.Employee.SqlQuery(sbSQL.ToString, params.ToArray).ToList
            End If
            '-- 2021/12/05 upd nv Inoue start


            '2021/11/09 nv miyaji退職以外の全担当者でいいとのことなので、不要。何かの参考になりそうなのでコードは残しておく。

            'Dim lstTanto As List(Of EmployeeModel)
            'sbSQL.Clear()
            ''M_EMPLOYEEテーブルから取得するSQLを作成(担当社員コンボボックス)
            'sbSQL.AppendLine("SELECT *")
            'sbSQL.AppendLine(" FROM ")
            'sbSQL.AppendLine("  M_EMPLOYEE EMP")
            'sbSQL.AppendLine("WHERE ")
            'sbSQL.AppendLine("EMP.AUTH_DIV <> '" & Misc.AUTH_DIV.SuperAdmin & "'")
            'sbSQL.AppendLine("AND EXISTS")
            'sbSQL.AppendLine("(")
            'sbSQL.AppendLine("SELECT 1 FROM M_EMPLOYEE_HUB EHUB")
            'sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_SF(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType_UpdAuthHub) AUTH")
            'sbSQL.AppendLine("ON AUTH.LOC_L_CD = EHUB.HUB_LOC_L_CD")
            'params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))
            'params.Add(New SqlParameter($"@ReadAuthFlg", authInfo.ReadAuthFlg))
            'params.Add(New SqlParameter($"@UpdAuthFlg", authInfo.UpdAuthFlg))
            'params.Add(New SqlParameter($"@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
            'params.Add(New SqlParameter($"@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
            'params.Add(New SqlParameter($"@UpdAuthType_UpdAuthHub", UpdAuthType.UPD_AUTH_HUB))
            'sbSQL.AppendLine("WHERE EHUB.EMPLOYEE_CD = EMP.EMPLOYEE_CD")
            'sbSQL.AppendLine(")")
            'sbSQL.AppendLine("ORDER BY EMP.EMPLOYEE_CD")

            ''SQLを実行する
            'lstTanto = db.Employee.SqlQuery(sbSQL.ToString, params.ToArray).ToList


            '社員コンボ設定
            Dim sFirstIndex As String = String.Empty
            If _LoginInfo IsNot Nothing Then
                sFirstIndex = _LoginInfo.EmployeeCd
            End If

            '-- 2021/12/05 upd nv Inoue start  全て、lstAll　にしていたのをisExistAuthOption_ChgHubで分岐した
            Dim lstEmp As List(Of EmployeeModel)
            If isExistAuthOption_ChgHub Then
                lstEmp = lstHub
            Else
                lstEmp = lstAll
            End If
            '申請社員コンボボックス
            If String.IsNullOrEmpty(_ViewModel.REQ_EMP_CD) Then
                ViewBag.ReqEmployeeList = New SelectList(lstEmp, "EMPLOYEE_CD", "EMPLOYEE_NAME", sFirstIndex)
            Else
                ViewBag.ReqEmployeeList = New SelectList(lstEmp, "EMPLOYEE_CD", "EMPLOYEE_NAME", _ViewModel.REQ_EMP_CD)
            End If
            '担当社員コンボボックス
            If String.IsNullOrEmpty(_ViewModel.CHARGE_EMP_CD) Then
                ViewBag.ChargeEmployeeList = New SelectList(lstEmp, "EMPLOYEE_CD", "EMPLOYEE_NAME", sFirstIndex)
            Else
                ViewBag.ChargeEmployeeList = New SelectList(lstEmp, "EMPLOYEE_CD", "EMPLOYEE_NAME", _ViewModel.CHARGE_EMP_CD)
            End If
            '-- 2021/12/05 upd nv Inoue start

            '引継ぎ社員コンボボックス
            If String.IsNullOrEmpty(_ViewModel.CHARGE_EMP_CD) Then
                ViewBag.HikiEmployeeList = New SelectList(lstAll, "EMPLOYEE_CD", "EMPLOYEE_NAME", "")
            Else
                ViewBag.HikiEmployeeList = New SelectList(lstAll, "EMPLOYEE_CD", "EMPLOYEE_NAME", _ViewModel.HikiEmployeeCd)
            End If

        End Sub
#End Region

#Region "送り先(貸出先)ドロップダウンリストデータ取得処理"
        ''' <summary>
        ''' 送り先(貸出先)マスタドロップダウンリストデータ取得処理
        ''' </summary>
        Public Sub GetLeasePlaceData()

            Dim lst As List(Of LeasePlaceModel)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext
            'M_LEASE_PLACEテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT *")
            sbSQL.AppendLine(" FROM ")
            sbSQL.AppendLine("  M_LEASE_PLACE ")
            sbSQL.AppendLine("ORDER BY LEASE_PLACE_CD") '2020/03/10 nv miyaji add
            'SQLを実行する
            lst = db.LeasePlace.SqlQuery(sbSQL.ToString).ToList
            '貸出先コンボ設定
            If String.IsNullOrEmpty(_ViewModel.LEASE_PLACE_CD) Then
                ViewBag.LeasePlaceList = New SelectList(lst, "LEASE_PLACE_CD", "LEASE_PLACE_NAME", "")
            Else
                ViewBag.LeasePlaceList = New SelectList(lst, "LEASE_PLACE_CD", "LEASE_PLACE_NAME", _ViewModel.LEASE_PLACE_CD)
            End If

        End Sub
#End Region

'2023/03/22 Kawamoto ADD START
#Region "返送先ドロップダウンリストデータ取得処理"
        ''' <summary>
        ''' 返送先ドロップダウンリストデータ取得処理
        ''' </summary>
        Public Sub GetRetPlaceData()

            Dim lst As List(Of LeasePlaceModel)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext

            'M_LEASE_PLACEテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT *")
            sbSQL.AppendLine(" FROM ")
            sbSQL.AppendLine("  M_LEASE_PLACE ")
            sbSQL.AppendLine(" WHERE ")
            sbSQL.AppendLine("  SEC_LEASE_PLACE_FLG = '" + SecLeasePlaceFlg.Cd.Yes + "'")
            sbSQL.AppendLine("ORDER BY LEASE_PLACE_CD")
            'SQLを実行する
            lst = db.LeasePlace.SqlQuery(sbSQL.ToString).ToList

            '返送先コンボ設定
            If String.IsNullOrEmpty(_ViewModel.RET_LEASE_PLACE_CD) Then
                ViewBag.RetLeasePlaceList = New SelectList(lst, "LEASE_PLACE_CD", "LEASE_PLACE_NAME", "")
            Else
                ViewBag.RetLeasePlaceList = New SelectList(lst, "LEASE_PLACE_CD", "LEASE_PLACE_NAME", _ViewModel.RET_LEASE_PLACE_CD)
            End If

        End Sub
#End Region
'2023/03/22 Kawamoto ADD END

#Region "貸出用途ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 貸出用途ドロップダウンリストデータ取得
        ''' </summary>
        ''' <remarks>2021/09/17 nv miyaji create</remarks>
        Private Sub GetLeaseStatusData()
            Dim list As List(Of SettingInfo) = New List(Of SettingInfo)

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '--2021/11/22 nv miyaji update start Reservation_Methodの数だけ取得するように変更
            Dim iCnt As Integer = 1
            While iCnt > 0
                Dim Youto As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method.KeyDiv, iCnt, CommonSettingKey.Reservation_Method.Key2, CommonSettingKey.Reservation_Method.Key3)
                If Youto Is Nothing Then
                    iCnt = 0
                Else
                    list.Add(Youto)
                    iCnt += 1
                End If
            End While
            'Dim Youto1 As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method.KeyDiv, CommonSettingKey.Reservation_Method.Key1, CommonSettingKey.Reservation_Method.Key2, CommonSettingKey.Reservation_Method.Key3)
            'Dim Youto2 As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reservation_Method_Development.KeyDiv, CommonSettingKey.Reservation_Method_Development.Key1, CommonSettingKey.CsvMapShiftJis.Key2, CommonSettingKey.Reservation_Method_Development.Key3)
            'If Not Youto1 Is Nothing Then
            '    list.Add(Youto1)
            'End If
            'If Not Youto2 Is Nothing Then
            '    list.Add(Youto2)
            'End If
            '--2021/11/22 nv miyaji update end

            ' 並べ替えた結果を別のコレクションに書き出す（LINQのOrderBy拡張メソッド）
            Dim sorted As IOrderedEnumerable(Of SettingInfo) = list.OrderBy(Function(x) x.DATA10)

            '初期値設定
            Dim sLeaseUse As String = AssetLeaseStatus.Ng
            If Not String.IsNullOrEmpty(_ViewModel.LEASE_USE) Then
                sLeaseUse = _ViewModel.LEASE_USE
            End If

            ViewBag.LeaseUse = New SelectList(sorted, "DATA1", "DATA1", sLeaseUse)
            '_ViewModel.epcSizeDropdownData = lst

        End Sub
#End Region

#Region "資産履歴を追加します"
        ''' <summary>
        ''' 資産履歴を追加します
        ''' </summary>
        ''' <param name="model">モデル</param>
        ''' <param name="sProcessDiv">処理区分</param>
        Private Sub InsertAssetHistory(model As AssetListModel, sProcessDiv As String)
            Dim HistoryModel As AssetHistoryModel = New AssetHistoryModel                    '資産履歴登録用モデル

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim db As New SystemDBContext
            Dim sLocLName As String = String.Empty
            Dim sLocMName As String = String.Empty
            Dim sLocSName As String = String.Empty
            'HistoryModel.PROCESS_LOG_ID = ""  '自動採番
            HistoryModel.PROCESS_DT = CInt(DateTime.Now.ToString("yyyyMMdd"))
            HistoryModel.PROCESS_TM = DateTime.Now.ToString("HHmmssfff")
            HistoryModel.ASSET_NO = model.ASSET_NO
            HistoryModel.PROCESS_TOOL_DIV = ProcessToolDiv.Web
            HistoryModel.ASSET_ID = model.ASSET_ID
            HistoryModel.PROCESS_DIV = sProcessDiv
            HistoryModel.LEASE_PLACE_CD = model.LEASE_PLACE_CD
            HistoryModel.LEASE_PLACE_NAME = model.LEASE_PLACE_NAME
            HistoryModel.REMARK = model.REMARK
            HistoryModel.LEASE_EMPLOYEE_CD = model.LEASE_EMPLOYEE_CD
            HistoryModel.LEASE_EMPLOYEE_NAME = model.LEASE_EMPLOYEE_NAME
            HistoryModel.LEASE_DT = model.LEASE_DT
            HistoryModel.LEASE_DUE_DT = model.LEASE_DUE_DT
            HistoryModel.LEASE_STATUS = model.LEASE_STATUS
            HistoryModel.MNG_LOC_L_CD = model.MNG_LOC_L_CD
            HistoryModel.LOC_L_CD = model.LOC_L_CD
            HistoryModel.LOC_M_CD = model.LOC_M_CD
            HistoryModel.LOC_S_CD = model.LOC_S_CD
            If Not String.IsNullOrEmpty(model.LOC_L_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationL, model.LOC_L_CD, " ", " "}
                Dim locamodel As LocationModel = db.Location.Find(findKeys)
                sLocLName = locamodel.LOC_NAME
            End If
            If Not String.IsNullOrEmpty(model.LOC_M_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationM, model.LOC_L_CD, model.LOC_M_CD, " "}
                Dim locamodel As LocationModel = db.Location.Find(findKeys)
                sLocMName = locamodel.LOC_NAME
            End If
            If Not String.IsNullOrEmpty(model.LOC_S_CD.Trim) Then
                'IDをキーにロケーションモデルを取得する
                Dim findKeys As String() = {LOC_DIV.LocationS, model.LOC_L_CD, model.LOC_M_CD, model.LOC_S_CD}
                Dim locamodel As LocationModel = db.Location.Find(findKeys)
                sLocSName = locamodel.LOC_NAME
            End If
            HistoryModel.LOC_L_NAME = sLocLName
            HistoryModel.LOC_M_NAME = sLocMName
            HistoryModel.LOC_S_NAME = sLocSName
            HistoryModel.ASSET_NAME = model.ASSET_NAME
            HistoryModel.MAKER = model.MAKER
            HistoryModel.PRODUCT_NO = model.PRODUCT_NO
            HistoryModel.MODEL_NO = model.MODEL_NO
            HistoryModel.TASK_RESULT = TaskResult.OK

            '履歴の更新日時・更新社員コード・更新社員名・更新プロセスを登録
            HistoryModel.UPD_DATETIME = CommonRfid.getNowDate
            HistoryModel.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            HistoryModel.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            If _ViewModel.SenimotoKbn = SenimotoKbn.LendingStatus Then
                '登録/変更画面
                If _ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK Then
                    '登録画面
                    HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU)
                Else
                    '変更画面
                    HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU)
                End If
            Else
                '詳細画面
                HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_SHOSAI)
            End If


            '資産履歴テーブルの更新
            _db.AssetHistory.Add(HistoryModel)
        End Sub
#End Region

#Region "予約履歴を追加します"
        ''' <summary>
        ''' 予約履歴を追加します
        ''' </summary>
        Public Sub InsertReserveLog(model As LendingEntryModel, sProcessDiv As String)
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'グリッドの情報を取得
            Dim lstGrid As List(Of ReserveDetailViewModel) = Session(CommonSessionKey.LENDING_ENTRY_GRID_INFO)
            Dim iCount As Integer = 1   '予約連番用
            For Each row As ReserveDetailViewModel In lstGrid
                Dim HistoryModel As LendingHistoryModel = New LendingHistoryModel                    '資産履歴登録用モデル
                HistoryModel.RESERVE_LOG_ID = 0 '自動採番
                HistoryModel.PROCESS_DT = CInt(DateTime.Now.ToString("yyyyMMdd"))
                HistoryModel.PROCESS_TM = DateTime.Now.ToString("HHmmssfff")
                HistoryModel.PROCESS_TOOL_DIV = ProcessToolDiv.Web
                HistoryModel.PROCESS_DIV = sProcessDiv
                HistoryModel.RESERVE_ID = model.RESERVE_ID
                HistoryModel.RESERVE_SUB_NO = iCount
                HistoryModel.REQ_EMP_CD = model.REQ_EMP_CD
                HistoryModel.CHARGE_EMP_CD = model.CHARGE_EMP_CD
                HistoryModel.REQUEST_DT = model.REQUEST_DT
                HistoryModel.REAL_START_DT = model.REAL_START_DT
                HistoryModel.REAL_END_DT = model.REAL_END_DT
                HistoryModel.USER_START_DT = model.USER_START_DT
                HistoryModel.USER_END_DT = model.USER_END_DT
                HistoryModel.LEASE_PLACE_CD = model.LEASE_PLACE_CD
                HistoryModel.LEASE_USE = model.LEASE_USE
                HistoryModel.MNG_LOC_L_CD = model.MNG_LOC_L_CD
                HistoryModel.DEST_NAME1 = model.DEST_NAME1
                HistoryModel.DEST_NAME2 = model.DEST_NAME2
                HistoryModel.DEST_ADDRESS1 = model.DEST_ADDRESS1
                HistoryModel.DEST_ADDRESS2 = model.DEST_ADDRESS2
                HistoryModel.DEST_ADDRESS3 = model.DEST_ADDRESS3
                HistoryModel.DEST_PHONE_NUMBER = model.DEST_PHONE_NUMBER
                HistoryModel.DEST_ZIP_CODE = model.DEST_ZIP_CODE
                HistoryModel.REMARK = model.REMARK
                HistoryModel.REMARK_INTERNAL = model.REMARK_INTERNAL
                HistoryModel.DEST_NO = model.DEST_NO
                HistoryModel.RESERVE_STATUS = model.RESERVE_STATUS

                HistoryModel.ASSET_NO = row.ASSET_NO
                HistoryModel.DTL_REAL_START_DT = row.REAL_START_DT
                HistoryModel.DTL_REAL_END_DT = row.REAL_END_DT
                If row.HANDOVER_DIV = HandOverDiv.HikitsugiSaki Then
                    HistoryModel.DTL_CHARGE_EMP_CD = row.CHARGE_EMP_CD
                Else
                    HistoryModel.DTL_CHARGE_EMP_CD = model.CHARGE_EMP_CD
                End If
                HistoryModel.DTL_LENDING_DT = row.LENDING_DT
                HistoryModel.DTL_RETURN_DT = row.RETURN_DT
                HistoryModel.DTL_HANDOVER_DIV = row.HANDOVER_DIV
                HistoryModel.DTL_RESERVE_STATUS = row.RESERVE_STATUS

                '履歴の登録日時・登録社員コード・登録社員名・更新日時・更新社員コード・更新社員名・更新プロセスを登録
                HistoryModel.INS_DATETIME = CommonRfid.getNowDate
                HistoryModel.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                HistoryModel.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                HistoryModel.UPD_DATETIME = CommonRfid.getNowDate
                HistoryModel.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                HistoryModel.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                If _ViewModel.SenimotoKbn = SenimotoKbn.LendingStatus Then
                    '登録/変更画面
                    If _ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK Then
                        '登録画面
                        HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_TOUROKU)
                    Else
                        '変更画面
                        HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HENKOU)
                    End If
                Else
                    '詳細画面
                    HistoryModel.UPD_TASK = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_SHOSAI)
                End If


                '予約履歴テーブルの更新
                _db.LendingHistory.Add(HistoryModel)

                '予約連番をカウントアップ
                iCount += 1
            Next

        End Sub
#End Region

#Region "各資産の　資産メンテナンス登録情報 セット　&　チェック"
        ''' <summary>
        ''' 各資産の　資産メンテナンス登録情報 セット　&　チェック
        ''' </summary>
        ''' <param name="lstGrid">List(Of ReserveDetailViewModel)</param>
        ''' <returns>
        ''' チェックOK時:String.Empty  チェックNG時:資産番号
        ''' </returns>
        ''' <remarks>
        '''     2021/10/16 nv Inoue
        ''' </remarks>
        Public Function CheckAssetMainteInfo(lstGrid As List(Of ReserveDetailViewModel)) As String
            '--2021/11/11 nv miyaji update start
            '稼働日考慮フラグ
            Dim reservationFunction As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3, True)
            Dim sWorkDayFlg As String = reservationFunction.DATA2
            '稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            Dim sWorkFlg As String = WorkFlg.Jitsubi
            If sWorkDayFlg = WorkDayFlg.Yes Then
                sWorkFlg = WorkFlg.Kadobi
            End If
            ''返却リードタイム
            'Dim setting_HenkyakuLT As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Return_LeadTime.KeyDiv, CommonSettingKey.Return_LeadTime.Key1, CommonSettingKey.Return_LeadTime.Key2, CommonSettingKey.Return_LeadTime.Key3, True)
            ''稼働日考慮フラグが"Y"時は"K"[稼働日]それ以外は"J"[実日]
            'Dim sWorkFlg As String = WorkFlg.Jitsubi
            'If setting_HenkyakuLT.DATA2 = WorkDayFlg.Yes Then
            '    sWorkFlg = WorkFlg.Kadobi
            'End If
            '--2021/11/11 nv miyaji update end

            Me._dsMentInfo = New DataSet
            Dim isMentInfoChk As Boolean = True
            Dim sNgAssetNo As String = String.Empty
            Dim iCnt As Integer = 0
            For Each row As ReserveDetailViewModel In lstGrid
                sNgAssetNo = row.ASSET_NO                  'NG用に最初にセットしておく


                '--2021/12/06 nv miyaji update start 仕様変更
                Me._dsMentInfo.Tables.Add(Dao.ToolReserve.GetReserveMainteInfo(row.ASSET_NO, row.RESERVE_ID, row.PR_ST_DT, row.PR_END_DT, sWorkFlg))
                'Me._dsMentInfo.Tables.Add(Dao.ToolReserve.GetReserveMainteInfo(row.ASSET_NO, row.RESERVE_ID, row.REAL_START_DT, row.REAL_END_DT, sWorkFlg))

                '--2021/12/08 nv Inoue add 引継区分 未引継ぎ　のみチェックする(配列順を考慮し、ここで行なう)
                If row.HANDOVER_DIV = Misc.HandOverDiv.Mi Then

                    '--2021/12/06 nv miyaji update end
                    If Me._dsMentInfo.Tables(iCnt).Rows(0).Item("MAINTE_EXEC_FLG").ToString = Misc.MainteExecFlg.Yes Then

                        '重複禁止データに予約のメンテナンス期間が重なっている場合
                        If Me._dsMentInfo.Tables(iCnt).Rows(0).Item("BAN_MAINTE").ToString = "Y" Then
                            isMentInfoChk = False
                            Exit For
                        End If

                        'メンテナンス必要時
                        If Me._dsMentInfo.Tables(iCnt).Rows(0).Item("MAINTE_DIV").ToString = Misc.MainteDiv.Ng Or
                           Me._dsMentInfo.Tables(iCnt).Rows(0).Item("MAINTE_DIV").ToString = Misc.MainteDiv.Chg Then
                            'この場合は、みんな不可
                            isMentInfoChk = False
                            Exit For
                        ElseIf Me._dsMentInfo.Tables(iCnt).Rows(0).Item("MAINTE_DIV").ToString = Misc.MainteDiv.Ok Then
                            'この場合は、みんな可
                        Else
                            If Me._LoginInfo.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.SuperAdmin Or
                               Me._LoginInfo.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Honbu_Admin Or
                               Me._LoginInfo.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Kyoten_Admin Or
                               Me._LoginInfo.ShosaiAuthDiv = Misc.SHOSAI_AUTH_DIV.Tsujo_Admin Then
                                'ログインが管理者以上の場合は、OKとする
                            Else
                                '以外は、不可
                                isMentInfoChk = False
                                Exit For
                            End If
                        End If
                    End If
                End If

                iCnt += 1
            Next

            If isMentInfoChk Then
                Return String.Empty
            Else
                Return sNgAssetNo
            End If

        End Function
#End Region

#Region "引継明細作成時、前予約から引継された明細であれば、前予約の引継先明細の予約ステータスを「返却済」に変更する処理"
        ''' <summary>
        ''' 引継明細作成時、前予約から引継された明細であれば、前予約の引継先明細の予約ステータスを「返却済」に変更する処理
        ''' </summary>
        ''' <param name="db">SystemDBContext</param>
        ''' <param name="lReserveId">予約ID</param>
        ''' <param name="sAssetNo">資産番号 viewmodel</param>
        Public Sub UpdBeforeReserve(ByVal db As SystemDBContext, ByVal lReserveId As Long, ByVal sAssetNo As String)
            '予約明細を取得
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2021/12/07 nv miyaji add
            'T_RESERVE_DETAILテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT RESERVE.*")
            sbSQL.AppendLine("FROM T_RESERVE_DETAIL RESERVE")
            sbSQL.AppendLine("WHERE")
            sbSQL.AppendLine("RESERVE.HANDOVER_DIV = '" & HandOverDiv.HikitsugiSaki & "'")
            sbSQL.AppendLine("AND HANDOVER_RESERVE_ID = @ReserveId")
            sbSQL.AppendLine("AND ASSET_NO = @AssetNo")
            params.Add(New SqlParameter("@ReserveId", lReserveId))
            params.Add(New SqlParameter("@AssetNo", sAssetNo))
            'SQLを実行する
            Dim BeforeReserve As List(Of ReserveDetailModel) = db.ReserveDetail.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            If BeforeReserve IsNot Nothing AndAlso BeforeReserve.Count > 0 Then
                BeforeReserve(0).RESERVE_STATUS = ReserveStatus.HenkyakuSumi

                'アタッチする(予約明細)   2021/12/07 upd nv Inoue 予約と予約明細の更新タイミングを分ける
                _db.Entry(BeforeReserve(0)).State = Entity.EntityState.Modified
            End If

        End Sub
#End Region

#Region "一般者か又は管理者でもU_HUB権限があり自拠点でない場合のチェックが必要かを取得"
        ''' <summary>
        ''' 一般者か又は管理者でもU_HUB権限があり自拠点でない場合のチェックが必要かを取得
        ''' </summary>
        ''' <remarks>2021/12/16 nv miyaji create</remarks>
        Private Function GetIsNeedCheck(LoginInfo As RfidEntity.LoginUser, sMngLocLCd As String) As Boolean

            '一般者の場合
            If _LoginInfo.KengenKbn = AUTH_DIV.Nomal Then
                Return True
            End If

            '管理者の場合
            'ログイン社員の所属拠点データ取得
            Dim tblEmpHub As DataTable = Dao.ToolEmployee.GetHubLocData(LoginInfo.EmployeeCd)
            '権限情報の取得
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, LoginInfo)
            '特殊権限が存在するかどうかを取得
            Dim isExistAuthOption_Hub As Boolean = Dao.ToolCommon.IsExistsAuthOption(authInfo.AuthOption, AuthOptionName.U_HUB)
            If isExistAuthOption_Hub Then
                Dim isJikyoten As Boolean = False    '自拠点か否か
                For Each rowEmpHub As DataRow In tblEmpHub.Rows
                    If sMngLocLCd = rowEmpHub("HUB_LOC_L_CD").ToString Then
                        isJikyoten = True
                        Exit For
                    End If
                Next
                If Not isJikyoten Then
                    Return True
                End If
            End If

            Return False

        End Function
#End Region

#Region "予約番号の入力可否を取得"
        ''' <summary>
        ''' 予約番号の入力可否を取得
        ''' </summary>
        ''' <returns>True:可能　False:不可</returns>
        Private Function GetInputEnableReserveId() As Boolean
            '設定マスタ(Reservation_Regist_Option)のData1が有効の場合、入力可能
            Dim reserveRegOption As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reserve_Regist_Option.KeyDiv, CommonSettingKey.Reserve_Regist_Option.Key1, CommonSettingKey.Reserve_Regist_Option.Key2, CommonSettingKey.Reserve_Regist_Option.Key3, True)
            Return reserveRegOption.DATA1 = ValidFlg.Cd.Valid
        End Function
#End Region

#Region "送り先名と送り先住所と貸出確認書宛名と返送先住所1の必須チェック"
        ''' <summary>
        ''' 送り先名と送り先住所の必須チェック ADD sv sato 2022/10/03
        ''' 貸出確認書宛名と返送先住所1も追加   2023/03/22 Kawamoto ADD
        ''' </summary>
        ''' <returns></returns>
        Private Function ChkDestNameAndDestAddressRequired() As Boolean
            'CSV取込が可能になっているかチェック 
            Dim reserveRegOption As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Reserve_Regist_Option.KeyDiv, CommonSettingKey.Reserve_Regist_Option.Key1, CommonSettingKey.Reserve_Regist_Option.Key2, CommonSettingKey.Reserve_Regist_Option.Key3, True)
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.LendingStatus, _LoginInfo)

            If authInfo.AuthOption.Contains(AuthOptionName.U_ADMIN) AndAlso reserveRegOption.DATA1 = ValidFlg.Cd.Valid Then
                'CSV取込可能な場合(管理者 かつ 設定Mの表示有無が有効の場合)、スペースのみの入力を許容する
                If IsNothing(_Model.DEST_NAME1) OrElse _Model.DEST_NAME1 = String.Empty Then
                    ModelState.AddModelError("DEST_NAME1", CommonLabel.MSG_ERR_NOT_ENTERD)
                    Return False
                End If
                If IsNothing(_Model.DEST_ADDRESS1) OrElse _Model.DEST_ADDRESS1 = String.Empty Then
                    ModelState.AddModelError("DEST_ADDRESS1", CommonLabel.MSG_ERR_NOT_ENTERD)
                    Return False
                End If
                '2023/03/22 Kawamoto ADD START
                If IsNothing(_Model.TO_DEST_NAME) OrElse _Model.TO_DEST_NAME = String.Empty Then
                    ModelState.AddModelError("TO_DEST_NAME", CommonLabel.MSG_ERR_NOT_ENTERD)
                    Return False
                End If
                If IsNothing(_Model.RET_ADDRESS1) OrElse _Model.RET_ADDRESS1 = String.Empty Then
                    ModelState.AddModelError("RET_ADDRESS1", CommonLabel.MSG_ERR_NOT_ENTERD)
                    Return False
                End If
                '2023/03/22 Kawamoto ADD END
            Else
                'CSV取込不可の場合、スペースのみの入力を許容しない
                If IsNothing(_Model.DEST_NAME1) OrElse _Model.DEST_NAME1.Trim = String.Empty Then
                    ModelState.AddModelError("DEST_NAME1", CommonLabel.MSG_ERR_NOT_ENTERD)
                    Return False
                End If
                If IsNothing(_Model.DEST_ADDRESS1) OrElse _Model.DEST_ADDRESS1.Trim = String.Empty Then
                    ModelState.AddModelError("DEST_ADDRESS1", CommonLabel.MSG_ERR_NOT_ENTERD)
                    Return False
                End If
                '2023/03/22 Kawamoto ADD START
                If IsNothing(_Model.TO_DEST_NAME) OrElse _Model.TO_DEST_NAME.Trim = String.Empty Then
                    ModelState.AddModelError("TO_DEST_NAME", CommonLabel.MSG_ERR_NOT_ENTERD)
                    Return False
                End If
                If IsNothing(_Model.RET_ADDRESS1) OrElse _Model.RET_ADDRESS1.Trim = String.Empty Then
                    ModelState.AddModelError("RET_ADDRESS1", CommonLabel.MSG_ERR_NOT_ENTERD)
                    Return False
                End If
                '2023/03/22 Kawamoto ADD END
            End If

            Return True
        End Function
#End Region

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

            params.Add(New SqlParameter($"@DISP_ID", DispId.LENDING_ENTRY))

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
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New LendingEntryViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.LENDING_ENTRY).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfo = GetGridInfo()
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton

            'グリッド情報をコピー
            _viewResult.ElementAt(0).ReserveDetailData = _ViewModel.ReserveDetailData
        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As LendingEntryViewModel)
            '2022/07/25 sv sato upd start
            '_Model.RESERVE_ID = model.RESERVE_ID
            If (model.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理の場合
                If GetInputEnableReserveId() Then
                    '入力した予約番号をセット
                    _Model.RESERVE_ID = model.ReserveIdString
                Else
                    '予約番号を採番
                    _Model.RESERVE_ID = Dao.ToolReserve.GetNextReserveId
                End If
            Else
                '更新・削除の場合
                _Model.RESERVE_ID = model.RESERVE_ID
            End If
            '2022/07/25 sv sato upd end

            _Model.REQ_EMP_CD = model.REQ_EMP_CD
            _Model.CHARGE_EMP_CD = model.CHARGE_EMP_CD
            _Model.REQUEST_DT = model.REQUEST_DT
            _Model.REAL_START_DT = model.REAL_START_DT
            _Model.REAL_END_DT = model.REAL_END_DT
            _Model.USER_START_DT = model.USER_START_DT
            _Model.USER_END_DT = model.USER_END_DT
            _Model.LEASE_PLACE_CD = model.LEASE_PLACE_CD
            _Model.LEASE_USE = model.LEASE_USE
            _Model.MNG_LOC_L_CD = model.MNG_LOC_L_CD
            _Model.DEST_NAME1 = model.DEST_NAME1
            _Model.DEST_NAME2 = model.DEST_NAME2
            '2023/03/22 Kawamoto ADD START
            _Model.TO_DEST_NAME = model.TO_DEST_NAME
            '2023/03/22 Kawamoto ADD END
            _Model.DEST_ADDRESS1 = model.DEST_ADDRESS1
            _Model.DEST_ADDRESS2 = model.DEST_ADDRESS2
            _Model.DEST_ADDRESS3 = model.DEST_ADDRESS3
            _Model.DEST_PHONE_NUMBER = model.DEST_PHONE_NUMBER
            _Model.DEST_ZIP_CODE = model.DEST_ZIP_CODE
            '2023/03/22 Kawamoto ADD START
            _Model.RET_LEASE_PLACE_CD = model.RET_LEASE_PLACE_CD
            _Model.RET_ADDRESS1 = model.RET_ADDRESS1
            _Model.RET_ADDRESS2 = model.RET_ADDRESS2
            _Model.RET_ADDRESS3 = model.RET_ADDRESS3
            _Model.RET_ZIP_CODE = model.RET_ZIP_CODE
            '2023/03/22 Kawamoto ADD END
            _Model.REMARK = model.REMARK
            _Model.REMARK_INTERNAL = model.REMARK_INTERNAL
            _Model.DEST_NO = model.DEST_NO
            _Model.LEASE_CHG_TIMES = model.LEASE_CHG_TIMES
            If String.IsNullOrEmpty(model.RESERVE_STATUS) Then
                _Model.RESERVE_STATUS = ReserveStatus.Mi
            Else
                _Model.RESERVE_STATUS = model.RESERVE_STATUS
            End If
            _Model.INS_DATETIME = model.INS_DATETIME
            _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
        End Sub

        ''' <summary>
        ''' 更新用のモデルをViewモデルにコピーする
        ''' </summary>
        Private Sub CopyModeltoViewModel(ByVal model As LendingEntryModel)
            _ViewModel.RESERVE_ID = model.RESERVE_ID
            _ViewModel.REQ_EMP_CD = model.REQ_EMP_CD
            _ViewModel.CHARGE_EMP_CD = model.CHARGE_EMP_CD
            _ViewModel.REQUEST_DT = model.REQUEST_DT
            _ViewModel.REAL_START_DT = model.REAL_START_DT
            _ViewModel.REAL_END_DT = model.REAL_END_DT
            _ViewModel.USER_START_DT = model.USER_START_DT
            _ViewModel.USER_END_DT = model.USER_END_DT
            _ViewModel.LEASE_PLACE_CD = model.LEASE_PLACE_CD
            _ViewModel.LEASE_USE = model.LEASE_USE
            _ViewModel.MNG_LOC_L_CD = model.MNG_LOC_L_CD
            _ViewModel.DEST_NAME1 = model.DEST_NAME1
            _ViewModel.DEST_NAME2 = model.DEST_NAME2
            '2023/03/22 Kawamoto ADD START
            _ViewModel.TO_DEST_NAME = model.TO_DEST_NAME
            '2023/03/22 Kawamoto ADD END
            _ViewModel.DEST_ADDRESS1 = model.DEST_ADDRESS1
            _ViewModel.DEST_ADDRESS2 = model.DEST_ADDRESS2
            _ViewModel.DEST_ADDRESS3 = model.DEST_ADDRESS3
            _ViewModel.DEST_PHONE_NUMBER = model.DEST_PHONE_NUMBER
            _ViewModel.DEST_ZIP_CODE = model.DEST_ZIP_CODE
            '2023/03/22 Kawamoto ADD START
            _ViewModel.RET_LEASE_PLACE_CD = model.RET_LEASE_PLACE_CD
            _ViewModel.RET_ADDRESS1 = model.RET_ADDRESS1
            _ViewModel.RET_ADDRESS2 = model.RET_ADDRESS2
            _ViewModel.RET_ADDRESS3 = model.RET_ADDRESS3
            _ViewModel.RET_ZIP_CODE = model.RET_ZIP_CODE
            '2023/03/22 Kawamoto ADD END
            _ViewModel.REMARK = model.REMARK
            _ViewModel.REMARK_INTERNAL = model.REMARK_INTERNAL
            _ViewModel.DEST_NO = model.DEST_NO
            _ViewModel.LEASE_CHG_TIMES = model.LEASE_CHG_TIMES
            _ViewModel.RESERVE_STATUS = model.RESERVE_STATUS
            _ViewModel.INS_DATETIME = model.INS_DATETIME
            _ViewModel.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            _ViewModel.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            _ViewModel.UPD_DATETIME = model.UPD_DATETIME
            _ViewModel.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _ViewModel.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _ViewModel.UPD_TASK = model.UPD_TASK
            _ViewModel.RealStartDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(model.REAL_START_DT))   '実貸出開始日表示文字列
            _ViewModel.RealEndDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(model.REAL_END_DT))    '実貸出終了日表示文字列
            _ViewModel.UserStartDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(model.USER_START_DT))   'ユーザー貸出開始日表示文字列
            _ViewModel.UserEndDtString = CommonRfid.ToHaifunYMD(CommonRfid.ToStringIntYMD(model.USER_END_DT))    'ユーザー貸出終了日表示文字列
            _ViewModel.ReserveIdString = model.RESERVE_ID
        End Sub
#End Region


    End Class
End Namespace