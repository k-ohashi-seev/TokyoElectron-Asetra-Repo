Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Data.SqlClient
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' 予約履歴画面表示用のクラス
    ''' </summary>
    Public Class LendingHistoryController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"
#End Region

#Region "クラス変数"
        'Private _Model As LendingHistoryModel = New LendingHistoryModel                                     'LendingHistoryモデル
        Private _ViewModel As LendingHistoryViewModel = New LendingHistoryViewModel                         'LendingHistory一覧表示用モデル
        Protected _viewResult As List(Of LendingHistoryViewModel) = New List(Of LendingHistoryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
#End Region

#Region "初期処理"
        ''' <summary>
        ''' アクションメソッド(Init)
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function Init(ByVal ViewModel As LendingHistoryViewModel, ByVal actionname As String) As ActionResult
            'セッションクリア
            If Session(RedirectAssetListInfo.ID) IsNot Nothing Then
                'チェックボックス状態の選択状態をクリアする
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
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
        Public Function Index(ByVal ViewModel As LendingHistoryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_LENDING_HISTORY)
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As LendingHistoryViewModel = Session(CommonSessionKey.CREAGE_LENDING_HISTORY)
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
                Session.Remove(CommonSessionKey.CREAGE_LENDING_HISTORY)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            Dim sbSQL As StringBuilder = New StringBuilder

            '予約ID
            Dim lReserveId As Long = 0
            If Session(CommonSessionKey.LENDING_HISTORY_INFO) IsNot Nothing Then
                lReserveId = Long.Parse(Session(CommonSessionKey.LENDING_HISTORY_INFO))
            End If

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("  RSV.RESERVE_ID")
            sbSQL.AppendLine("  , RSV.MNG_LOC_L_CD")
            sbSQL.AppendLine("  , MNG_LOC.LOC_NAME AS MNG_LOC_L_NAME")
            sbSQL.AppendLine("  , RSV.REQ_EMP_CD")
            sbSQL.AppendLine("  , REQ_EMP.EMPLOYEE_NAME AS REQ_EMP_NAME")
            sbSQL.AppendLine("  , RSV_LOG.RESERVE_LOG_ID")
            sbSQL.AppendLine("  , RSV_LOG.PROCESS_DT")
            sbSQL.AppendLine("  , RSV_LOG.PROCESS_TM")
            sbSQL.AppendLine("  , RSV_LOG.PROCESS_TOOL_DIV")
            sbSQL.AppendLine("  , RSV_LOG.PROCESS_DIV")
            sbSQL.AppendLine("  , RSV_LOG.REAL_START_DT")
            sbSQL.AppendLine("  , RSV_LOG.REAL_END_DT")
            sbSQL.AppendLine("  , RSV_LOG.USER_START_DT")
            sbSQL.AppendLine("  , RSV_LOG.USER_END_DT")
            sbSQL.AppendLine("  , RSV_LOG.CHARGE_EMP_CD")
            sbSQL.AppendLine("  , CHARGE_EMP.EMPLOYEE_NAME AS CHARGE_EMP_NAME")
            sbSQL.AppendLine("  , RSV_LOG.LEASE_PLACE_CD")
            sbSQL.AppendLine("  , LEASE_PLACE.LEASE_PLACE_NAME")
            sbSQL.AppendLine("  , RSV_LOG.LEASE_USE")
            sbSQL.AppendLine("  , ISNULL(RSV_LOG.DEST_NAME1, '') + ISNULL(RSV_LOG.DEST_NAME2, '') AS DEST_NAME")
            sbSQL.AppendLine("  , ISNULL(RSV_LOG.DEST_ADDRESS1, '') + ISNULL(RSV_LOG.DEST_ADDRESS2, '') + ISNULL(RSV_LOG.DEST_ADDRESS3, '') AS DEST_ADDRESS")
            sbSQL.AppendLine("  , RSV_LOG.REMARK")
            sbSQL.AppendLine("  , RSV_LOG.REMARK_INTERNAL")
            sbSQL.AppendLine("  , RSV_LOG.DEST_NO")
            sbSQL.AppendLine("  , RSV_LOG.ASSET_NO")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM10 AS SERIAL_NO")
            sbSQL.AppendLine("  , RSV_LOG.DTL_REAL_START_DT")
            sbSQL.AppendLine("  , RSV_LOG.DTL_REAL_END_DT")
            sbSQL.AppendLine("  , RSV_LOG.DTL_LENDING_DT")
            sbSQL.AppendLine("  , RSV_LOG.DTL_RETURN_DT")
            sbSQL.AppendLine("  , RSV_LOG.DTL_CHARGE_EMP_CD")
            sbSQL.AppendLine("  , DTL_CHARGE_EMP.EMPLOYEE_NAME AS DTL_CHARGE_EMP_NAME")
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM")
            sbSQL.AppendLine("  T_RESERVE_LOG RSV_LOG")
            sbSQL.AppendLine("  INNER JOIN T_RESERVE RSV")
            sbSQL.AppendLine("  ON  RSV_LOG.RESERVE_ID = RSV.RESERVE_ID")
            sbSQL.AppendLine("  INNER JOIN M_ASSET ASSET")
            sbSQL.AppendLine("  ON  RSV_LOG.ASSET_NO = ASSET.ASSET_NO")
            sbSQL.AppendLine("  INNER JOIN M_LOCATION MNG_LOC")
            sbSQL.AppendLine("  ON  MNG_LOC.LOC_DIV = @LocDivL")
            sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD = MNG_LOC.LOC_L_CD")
            addSqlParam("LocDivL", $"{LOC_DIV.LocationL}")
            sbSQL.AppendLine("  LEFT JOIN M_EMPLOYEE REQ_EMP")
            sbSQL.AppendLine("  ON  RSV.REQ_EMP_CD = REQ_EMP.EMPLOYEE_CD")
            sbSQL.AppendLine("  LEFT JOIN M_EMPLOYEE CHARGE_EMP")
            sbSQL.AppendLine("  ON  RSV_LOG.CHARGE_EMP_CD = CHARGE_EMP.EMPLOYEE_CD")
            sbSQL.AppendLine("  LEFT JOIN M_EMPLOYEE DTL_CHARGE_EMP")
            sbSQL.AppendLine("  ON  RSV_LOG.DTL_CHARGE_EMP_CD = DTL_CHARGE_EMP.EMPLOYEE_CD")
            sbSQL.AppendLine("  LEFT JOIN M_LEASE_PLACE LEASE_PLACE")
            sbSQL.AppendLine("  ON  RSV_LOG.LEASE_PLACE_CD = LEASE_PLACE.LEASE_PLACE_CD")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")
            sbSQL.AppendLine("  AND RSV_LOG.RESERVE_ID = @ReserveId")
            addSqlParam("ReserveId", $"{lReserveId}")

            sbSQL.AppendLine("ORDER BY RSV_LOG.PROCESS_DT, RSV_LOG.PROCESS_TM")

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of LendingHistoryViewModel) = _db.LendingHistoryView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        Public Function CsvExport(ViewModel As LendingHistoryViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel

            Dim LendingHistoryData As List(Of LendingHistoryViewModel)

            'CSV出力データ取得
            LendingHistoryData = GetLendingHistoryData()

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
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_LENDING_HISTORY)
            Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_LENDING_HISTORY)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As LendingHistoryViewModel In LendingHistoryData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(row.RESERVE_ID)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.MNG_LOC_L_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REQ_EMP_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REQ_EMP_NAME)
                '--処理日時
                Dim sProcessDtTm As String = CommonRfid.ToStringLongYMDHM(Long.Parse(row.PROCESS_DT.ToString & row.PROCESS_TM))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sProcessDtTm)
                '--処理区分
                Dim sProcessDiv As String = String.Empty
                Select Case row.PROCESS_DIV
                    Case ProcessDiv.MI
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MI)
                    Case ProcessDiv.CI
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CI)
                    Case ProcessDiv.MU
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MU)
                    Case ProcessDiv.CU
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CU)
                    Case ProcessDiv.MR
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MR)
                    Case ProcessDiv.CR
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CR)
                    Case ProcessDiv.MD
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_MD)
                    Case ProcessDiv.T1
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_T1)
                    Case ProcessDiv.T2
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_T2)
                    Case ProcessDiv.BS
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_BS)
                    Case ProcessDiv.BO
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_BO)
                    Case ProcessDiv.BI
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_BI)
                    Case ProcessDiv.BR
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_RETIRE)
                    Case ProcessDiv.LC
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_LC)
                    Case ProcessDiv.CD
                        sProcessDiv = Me._Message.getMessage(CommonLabel.VALUE_PROCESS_DIV_CD)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sProcessDiv)
                '--処理分類
                Dim sProcessToolDiv As String = String.Empty
                Select Case row.PROCESS_TOOL_DIV
                    Case ProcessToolDiv.Web
                        sProcessToolDiv = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)
                    Case ProcessToolDiv.CS
                        sProcessToolDiv = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_CS)
                    Case ProcessToolDiv.HT
                        sProcessToolDiv = Me._Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sProcessToolDiv)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHARGE_EMP_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHARGE_EMP_NAME)
                '--実質貸出開始日
                Dim sRealStartDt As String = String.Empty
                If row.REAL_START_DT.HasValue Then
                    sRealStartDt = CommonRfid.ToStringIntYMD(row.REAL_START_DT.Value)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sRealStartDt)
                '--実質貸出終了日
                Dim sRealEndDt As String = String.Empty
                If row.REAL_END_DT.HasValue Then
                    sRealEndDt = CommonRfid.ToStringIntYMD(row.REAL_END_DT.Value)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sRealEndDt)
                '--ユーザー貸出開始日
                Dim sUserStartDt As String = String.Empty
                If row.USER_START_DT.HasValue Then
                    sUserStartDt = CommonRfid.ToStringIntYMD(row.USER_START_DT.Value)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sUserStartDt)
                '--ユーザー貸出終了日
                Dim sUserEndDt As String = String.Empty
                If row.USER_END_DT.HasValue Then
                    sUserEndDt = CommonRfid.ToStringIntYMD(row.USER_END_DT.Value)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sUserEndDt)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_PLACE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LEASE_USE)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DEST_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DEST_ADDRESS)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK_INTERNAL)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DEST_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.SERIAL_NO)
                '--資産実質貸出開始日
                Dim sDtlRealStartDt As String = String.Empty
                If row.DTL_REAL_START_DT.HasValue Then
                    sDtlRealStartDt = CommonRfid.ToStringIntYMD(row.DTL_REAL_START_DT.Value)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sDtlRealStartDt)
                '--資産実質貸出終了日
                Dim sDtlRealEndDt As String = String.Empty
                If row.DTL_REAL_END_DT.HasValue Then
                    sDtlRealEndDt = CommonRfid.ToStringIntYMD(row.DTL_REAL_END_DT.Value)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sDtlRealEndDt)
                '--実貸出日
                Dim sDtlLendingDt As String = String.Empty
                If row.DTL_LENDING_DT.HasValue Then
                    sDtlLendingDt = CommonRfid.ToStringIntYMD(row.DTL_LENDING_DT.Value)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sDtlLendingDt)
                '--実返却日
                Dim sDtlReturnDt As String = String.Empty
                If row.DTL_RETURN_DT.HasValue Then
                    sDtlReturnDt = CommonRfid.ToStringIntYMD(row.DTL_RETURN_DT.Value)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(sDtlReturnDt)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DTL_CHARGE_EMP_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DTL_CHARGE_EMP_NAME)

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
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HISTORY) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT_ALL) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     LendingHistoryData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelLendingStatus.TITLE_LENDING_HISTORY))


            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()
        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetLendingHistoryData() As List(Of LendingHistoryViewModel)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            '予約ID
            Dim lReserveId As Long = 0
            If Session(CommonSessionKey.LENDING_HISTORY_INFO) IsNot Nothing Then
                lReserveId = Long.Parse(Session(CommonSessionKey.LENDING_HISTORY_INFO))
            End If

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("  RSV.RESERVE_ID")
            sbSQL.AppendLine("  , RSV.MNG_LOC_L_CD")
            sbSQL.AppendLine("  , MNG_LOC.LOC_NAME AS MNG_LOC_L_NAME")
            sbSQL.AppendLine("  , RSV.REQ_EMP_CD")
            sbSQL.AppendLine("  , REQ_EMP.EMPLOYEE_NAME AS REQ_EMP_NAME")
            sbSQL.AppendLine("  , RSV_LOG.RESERVE_LOG_ID")
            sbSQL.AppendLine("  , RSV_LOG.PROCESS_DT")
            sbSQL.AppendLine("  , RSV_LOG.PROCESS_TM")
            sbSQL.AppendLine("  , RSV_LOG.PROCESS_TOOL_DIV")
            sbSQL.AppendLine("  , RSV_LOG.PROCESS_DIV")
            sbSQL.AppendLine("  , RSV_LOG.REAL_START_DT")
            sbSQL.AppendLine("  , RSV_LOG.REAL_END_DT")
            sbSQL.AppendLine("  , RSV_LOG.USER_START_DT")
            sbSQL.AppendLine("  , RSV_LOG.USER_END_DT")
            sbSQL.AppendLine("  , RSV_LOG.CHARGE_EMP_CD")
            sbSQL.AppendLine("  , CHARGE_EMP.EMPLOYEE_NAME AS CHARGE_EMP_NAME")
            sbSQL.AppendLine("  , RSV_LOG.LEASE_PLACE_CD")
            sbSQL.AppendLine("  , LEASE_PLACE.LEASE_PLACE_NAME")
            sbSQL.AppendLine("  , RSV_LOG.LEASE_USE")
            sbSQL.AppendLine("  , ISNULL(RSV_LOG.DEST_NAME1, '') + ISNULL(RSV_LOG.DEST_NAME2, '') AS DEST_NAME")
            sbSQL.AppendLine("  , ISNULL(RSV_LOG.DEST_ADDRESS1, '') + ISNULL(RSV_LOG.DEST_ADDRESS2, '') + ISNULL(RSV_LOG.DEST_ADDRESS3, '') AS DEST_ADDRESS")
            sbSQL.AppendLine("  , RSV_LOG.REMARK")
            sbSQL.AppendLine("  , RSV_LOG.REMARK_INTERNAL")
            sbSQL.AppendLine("  , RSV_LOG.DEST_NO")
            sbSQL.AppendLine("  , RSV_LOG.ASSET_NO")
            sbSQL.AppendLine("  , ASSET.ASSET_NAME")
            sbSQL.AppendLine("  , ASSET.FREE_ITEM10 AS SERIAL_NO")
            sbSQL.AppendLine("  , RSV_LOG.DTL_REAL_START_DT")
            sbSQL.AppendLine("  , RSV_LOG.DTL_REAL_END_DT")
            sbSQL.AppendLine("  , RSV_LOG.DTL_LENDING_DT")
            sbSQL.AppendLine("  , RSV_LOG.DTL_RETURN_DT")
            sbSQL.AppendLine("  , RSV_LOG.DTL_CHARGE_EMP_CD")
            sbSQL.AppendLine("  , DTL_CHARGE_EMP.EMPLOYEE_NAME AS DTL_CHARGE_EMP_NAME")
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM")
            sbSQL.AppendLine("  T_RESERVE_LOG RSV_LOG")
            sbSQL.AppendLine("  INNER JOIN T_RESERVE RSV")
            sbSQL.AppendLine("  ON  RSV_LOG.RESERVE_ID = RSV.RESERVE_ID")
            sbSQL.AppendLine("  INNER JOIN M_ASSET ASSET")
            sbSQL.AppendLine("  ON  RSV_LOG.ASSET_NO = ASSET.ASSET_NO")
            sbSQL.AppendLine("  INNER JOIN M_LOCATION MNG_LOC")
            sbSQL.AppendLine("  ON  MNG_LOC.LOC_DIV = @LocDivL")
            sbSQL.AppendLine("  AND ASSET.MNG_LOC_L_CD = MNG_LOC.LOC_L_CD")
            params.Add(New SqlParameter($"@LocDivL", LOC_DIV.LocationL))
            sbSQL.AppendLine("  LEFT JOIN M_EMPLOYEE REQ_EMP")
            sbSQL.AppendLine("  ON  RSV.REQ_EMP_CD = REQ_EMP.EMPLOYEE_CD")
            sbSQL.AppendLine("  LEFT JOIN M_EMPLOYEE CHARGE_EMP")
            sbSQL.AppendLine("  ON  RSV_LOG.CHARGE_EMP_CD = CHARGE_EMP.EMPLOYEE_CD")
            sbSQL.AppendLine("  LEFT JOIN M_EMPLOYEE DTL_CHARGE_EMP")
            sbSQL.AppendLine("  ON  RSV_LOG.DTL_CHARGE_EMP_CD = DTL_CHARGE_EMP.EMPLOYEE_CD")
            sbSQL.AppendLine("  LEFT JOIN M_LEASE_PLACE LEASE_PLACE")
            sbSQL.AppendLine("  ON  RSV_LOG.LEASE_PLACE_CD = LEASE_PLACE.LEASE_PLACE_CD")
            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")
            sbSQL.AppendLine("  AND RSV_LOG.RESERVE_ID = @ReserveId")
            params.Add(New SqlParameter($"@ReserveId", lReserveId))
            sbSQL.AppendLine("ORDER BY RSV_LOG.PROCESS_DT, RSV_LOG.PROCESS_TM")

            Dim lstLendingHistory As List(Of LendingHistoryViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstLendingHistory = db.LendingHistoryView.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            'データを返却
            Return lstLendingHistory
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
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New LendingHistoryViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).GridInfoTop = ToolColSet.GetGridInfo(_db, DispId.LENDING_HISTORY_TOP)
            _viewResult.ElementAt(0).FixColCntTop = _db.ColSet.Find(DispId.LENDING_HISTORY_TOP).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfoBottom = ToolColSet.GetGridInfo(_db, DispId.LENDING_HISTORY_BOTTOM)
            _viewResult.ElementAt(0).FixColCntBottom = _db.ColSet.Find(DispId.LENDING_HISTORY_BOTTOM).FIX_COL_CNT
        End Sub
#End Region

    End Class
End Namespace
