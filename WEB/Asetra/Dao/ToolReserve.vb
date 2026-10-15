Imports Asetra.Misc
Imports System.IO
Imports Asetra.Models
Imports System.Data.SqlClient
Imports System.Data.Entity.Infrastructure

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：予約関連ツール系 Data Access クラス
    ''' 作成日        ：2021/10/18      miyaji
    ''' 修正日        ：2022/08/31      Kawamoto
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolReserve
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "メンテナンス・予約期間の重複チェック"
        ''' <summary>
        ''' メンテナンス・予約期間の重複チェック
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <param name="lReserveId">予約番号</param>
        ''' <param name="lMainteId">メンテナンス番号</param>
        ''' <param name="iTargetFrom">対象日From</param>
        ''' <param name="iTargetTo">対象日To</param>
        ''' <param name="iMenteReg">メンテナンス登録か否か</param> メンテナンス登録,○○登録,予備登録1,予備登録2の際は重複可能でも登録できないようにするため
        ''' <returns>
        ''' 重複件数
        ''' </returns>
        Public Shared Function GetReserveMainteJufukuSuu(sAssetNo As String, lReserveId As Long, lMainteId As Long, iTargetFrom As Integer, iTargetTo As Integer, Optional iMenteReg As String = "N") As Integer
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                '予約重複確認
                '予約テーブルと予約メンテナンステーブルから取得したレコードの合計数を返却する。
                sbSQL.Append("SELECT dbo.CHECK_RESERVE_SF(@AssetNo, @ReserveId, @MainteId, @TargetFrom, @TargetTo, @MenteReg) AS Cnt")
                params.Add(New SqlParameter($"@AssetNo", sAssetNo))
                If lReserveId = 0 Then
                    params.Add(New SqlParameter($"@ReserveId", DBNull.Value))
                Else
                    params.Add(New SqlParameter($"@ReserveId", lReserveId))
                End If
                If lMainteId = 0 Then
                    params.Add(New SqlParameter($"@MainteId", DBNull.Value))
                Else
                    params.Add(New SqlParameter($"@MainteId", lMainteId))
                End If
                params.Add(New SqlParameter($"@TargetFrom", iTargetFrom))
                params.Add(New SqlParameter($"@TargetTo", iTargetTo))
                params.Add(New SqlParameter($"@MenteReg", iMenteReg))

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                Return CInt(tbl.Rows(0)(0))

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

        End Function
#End Region

#Region "メンテナンス・予約期間の重複チェック（Entity Frameworkで実施）"
        ''' <summary>
        ''' メンテナンス・予約期間の重複チェック（Entity Frameworkで実施）
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <param name="lReserveId">予約番号</param>
        ''' <param name="lMainteId">メンテナンス番号</param>
        ''' <param name="iTargetFrom">対象日From</param>
        ''' <param name="iTargetTo">対象日To</param>
        ''' <param name="iMenteReg">メンテナンス登録か否か</param> メンテナンス登録の際は重複可能でも登録できないようにするため
        ''' <returns>
        ''' 重複件数
        ''' </returns>
        Public Shared Function GetReserveMainteJufukuSuuEF(ByRef db As SystemDBContext, sAssetNo As String, lReserveId As Long, lMainteId As Long, iTargetFrom As Integer, iTargetTo As Integer, Optional iMenteReg As String = "N") As Integer
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Try
                '予約重複確認
                '予約テーブルと予約メンテナンステーブルから取得したレコードの合計数を返却する。
                sbSQL.Append("SELECT dbo.CHECK_RESERVE_SF(@AssetNo, @ReserveId, @MainteId, @TargetFrom, @TargetTo, @MenteReg) AS Cnt")
                params.Add(New SqlParameter($"@AssetNo", sAssetNo))
                If lReserveId = 0 Then
                    params.Add(New SqlParameter($"@ReserveId", DBNull.Value))
                Else
                    params.Add(New SqlParameter($"@ReserveId", lReserveId))
                End If
                If lMainteId = 0 Then
                    params.Add(New SqlParameter($"@MainteId", DBNull.Value))
                Else
                    params.Add(New SqlParameter($"@MainteId", lMainteId))
                End If
                params.Add(New SqlParameter($"@TargetFrom", iTargetFrom))
                params.Add(New SqlParameter($"@TargetTo", iTargetTo))
                params.Add(New SqlParameter($"@MenteReg", iMenteReg))

                Dim checkReserve As List(Of CheckReserveSF) = db.CheckReserveSF.SqlQuery(sbSQL.ToString, params.ToArray).ToList
                Return checkReserve(0).CNT

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            End Try
        End Function
#End Region

#Region "カレンダー指定日の予約・メンテナンス情報取得"
        ''' <summary>
        ''' カレンダー指定日の予約・メンテナンス情報取得
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <param name="iTargetDate">対象日</param>
        ''' <returns>
        ''' 予約情報
        ''' </returns>
        Public Shared Function GetAssetCalDateReserveInfo(sAssetNo As String, iTargetDate As Integer) As DataTable
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.Append("SELECT * FROM dbo.ASSET_CalDate_RESERVE_INFO_SF(@AssetNo, @TargetDate)")
                params.Add(New SqlParameter($"@AssetNo", sAssetNo))
                params.Add(New SqlParameter($"@TargetDate", iTargetDate))

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                Return tbl
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "次予約取得"
        ''' <summary>
        ''' 次予約取得
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <param name="lReserveId">予約ID</param>
        ''' <param name="iTargetDate">対象日</param>
        ''' <param name="sWorkFlg">稼働日実日区分</param>
        ''' <returns>
        ''' 次予約情報
        ''' </returns>
        Public Shared Function GetNextReserve(sAssetNo As String, lReserveId As Long, iTargetDate As Integer, sWorkFlg As String) As DataTable
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.Append("SELECT * FROM dbo.NEXT_RESERVE_SF(@AssetNo, @ReserveId, @TargetDate, @WorkFlg)")
                params.Add(New SqlParameter($"@AssetNo", sAssetNo))
                params.Add(New SqlParameter($"@ReserveId", lReserveId))
                params.Add(New SqlParameter($"@TargetDate", iTargetDate))
                params.Add(New SqlParameter($"@WorkFlg", sWorkFlg))

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                Return tbl
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "指定期間の営業日数取得"
        ''' <summary>
        ''' 指定期間の営業日数取得
        ''' </summary>
        ''' <param name="iStartDate">開始日</param>
        ''' <param name="iEndDate">終了日</param>
        ''' <returns>営業日数</returns>
        Public Shared Function GetEigyoDay(iStartDate As Integer, iEndDate As Integer) As Integer
            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT COUNT(*) AS EigyoDay FROM dbo.RESERVE_DATE_SF(@StartDate, @EndDate)")
            sbSQL.AppendLine("WHERE NOT_WORK_FLG = @NotWorkFlg_No")

            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            params.Add(New SqlParameter($"@StartDate", iStartDate))
            params.Add(New SqlParameter($"@EndDate", iEndDate))
            params.Add(New SqlParameter($"@NotWorkFlg_No", NotWorkFlg.No))

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Dim ret As Integer = 0
            Try
                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                If tbl.Rows.Count <> 0 Then
                    ret = Integer.Parse(tbl.Rows(0).Item("EigyoDay"))
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return ret
        End Function
#End Region

'20220831 Add Start Kawamoto
#Region "指定期間の稼働日or非稼働日を取得"
        ''' <summary>
        ''' 指定期間の稼働日or非稼働日を取得
        ''' </summary>
        ''' <param name="iStartDate">開始日</param>
        ''' <param name="iEndDate">終了日</param>
        ''' <param name="iNotWorkFlg">非稼働日フラグ（Y：非稼働日、N：稼働日）</param>
        ''' <returns>営業日数</returns>
        Public Shared Function GetReserveDate(iStartDate As Integer, iEndDate As Integer, iNotWorkFlg As String) As DataTable
            Dim clsDB As clsSV_DB = New clsSV_DB
            Dim sbSQL As StringBuilder = New StringBuilder
            Try
                If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                    '接続失敗
                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
                End If

                sbSQL.AppendLine("SELECT DT FROM dbo.RESERVE_DATE_SF(@StartDate, @EndDate)")
                sbSQL.AppendLine("WHERE NOT_WORK_FLG = @NotWorkFlg_No")

                Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
                params.Add(New SqlParameter($"@StartDate", iStartDate))
                params.Add(New SqlParameter($"@EndDate", iEndDate))
                params.Add(New SqlParameter($"@NotWorkFlg_No", iNotWorkFlg))

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                Return tbl

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region
'20220831 Add End Kawamoto

#Region "指定日の非稼働日フラグ取得"
        ''' <summary>
        ''' 指定日の非稼働日フラグ取得
        ''' </summary>
        ''' <param name="iTargetDate">対象日</param>
        ''' <returns>非稼働日フラグ</returns>
        Public Shared Function GetNotWorkFlg(iTargetDate As Integer) As String
            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT * FROM dbo.RESERVE_DATE_SF(@StartDate, @EndDate)")

            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            params.Add(New SqlParameter($"@StartDate", iTargetDate))
            params.Add(New SqlParameter($"@EndDate", iTargetDate))

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Dim ret As String = String.Empty
            Try
                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                If tbl.Rows.Count <> 0 Then
                    ret = tbl.Rows(0).Item("NOT_WORK_FLG").ToString
                End If

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return ret
        End Function
#End Region

#Region "資産メンテナンス登録情報取得"
        ''' <summary>
        ''' 資産メンテナンス登録情報取得
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <param name="lReserveId">予約ID</param>
        ''' <param name="iTargetDateFrom">貸出開始日</param>
        ''' <param name="iTargetDateTo">貸出終了日</param>
        ''' <param name="sWorkFlg">稼働日実日区分</param>
        ''' <returns>
        ''' 資産メンテナンス登録情報
        ''' </returns>
        Public Shared Function GetReserveMainteInfo(sAssetNo As String, lReserveId As Long, iTargetDateFrom As Integer, iTargetDateTo As Integer, sWorkFlg As String) As DataTable
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.Append("SELECT * FROM dbo.WARN_MAINTE_GET_SF(@AssetNo, @ReserveId, @TargetDateFrom, @TargetDateTo, @WorkFlg)")
                params.Add(New SqlParameter($"@AssetNo", sAssetNo))
                If lReserveId = 0 Then
                    params.Add(New SqlParameter($"@ReserveId", DBNull.Value))
                Else
                    params.Add(New SqlParameter($"@ReserveId", lReserveId))
                End If
                params.Add(New SqlParameter($"@TargetDateFrom", iTargetDateFrom))
                params.Add(New SqlParameter($"@TargetDateTo", iTargetDateTo))
                params.Add(New SqlParameter($"@WorkFlg", sWorkFlg))

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                Return tbl
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "資産メンテナンス登録情報取得（Entity Frameworkで実施）"
        ''' <summary>
        ''' 資産メンテナンス登録情報取得（Entity Frameworkで実施） 2022/07/25 add sv sato
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <param name="lReserveId">予約ID</param>
        ''' <param name="iTargetDateFrom">貸出開始日</param>
        ''' <param name="iTargetDateTo">貸出終了日</param>
        ''' <param name="sWorkFlg">稼働日実日区分</param>
        ''' <returns>
        ''' 資産メンテナンス登録情報
        ''' </returns>
        Public Shared Function GetReserveMainteInfoEF(ByRef db As SystemDBContext, sAssetNo As String, lReserveId As Long, iTargetDateFrom As Integer, iTargetDateTo As Integer, sWorkFlg As String) As List(Of ReserveMainteInfoSF)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Try
                sbSQL.Append("SELECT * FROM dbo.RESERVE_MAINTE_INFO_SF(@AssetNo, @ReserveId, @TargetDateFrom, @TargetDateTo, @WorkFlg)")
                params.Add(New SqlParameter($"@AssetNo", sAssetNo))
                If lReserveId = 0 Then
                    params.Add(New SqlParameter($"@ReserveId", DBNull.Value))
                Else
                    params.Add(New SqlParameter($"@ReserveId", lReserveId))
                End If
                params.Add(New SqlParameter($"@TargetDateFrom", iTargetDateFrom))
                params.Add(New SqlParameter($"@TargetDateTo", iTargetDateTo))
                params.Add(New SqlParameter($"@WorkFlg", sWorkFlg))

                Dim lstReserveMainteInfo As List(Of ReserveMainteInfoSF) = db.ReserveMainteInfo.SqlQuery(sbSQL.ToString, params.ToArray).ToList

                Return lstReserveMainteInfo
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            End Try
        End Function
#End Region

#Region "資産メンテナンス登録処理"
        ''' <summary>
        ''' 資産メンテナンス登録処理
        ''' </summary>
        ''' <param name="db">SystemDBContext</param>
        ''' <param name="lReserveId">予約ID</param>
        ''' <param name="lstDltViewModel">予約詳細 viewmodel</param>
        ''' <param name="dsMentInfo">貸出開始日</param>
        ''' <param name="loginInfo">ログイン情報</param>
        ''' <param name="sTitle">タスク名</param>
        Public Shared Sub UpdAssetMainte(ByVal db As SystemDBContext,
                                              ByVal lReserveId As Long,
                                              ByVal lstDltViewModel As List(Of ReserveDetailViewModel),
                                              ByVal dsMentInfo As DataSet,
                                              ByVal loginInfo As RfidEntity.LoginUser,
                                              ByVal sTitle As String
                                              )

            Dim modelMainte As LendingMainteEntryModel
            For ii As Integer = 0 To lstDltViewModel.Count - 1


                Dim rowMentInfo As DataRow = dsMentInfo.Tables(ii).Rows(0)
                Dim modelDtl As ReserveDetailViewModel = lstDltViewModel(ii)
                Dim viewSearch As List(Of SearchMainteViewModel) = Nothing

                '--2021/12/07 nv Inoue add 引継区分 <> 未引継ぎ はチェック対象外にする
                If modelDtl.HANDOVER_DIV <> Misc.HandOverDiv.Mi Then
                    Continue For
                End If

                '該当メンテナンスID取得
                If modelDtl.RESERVE_ID <> 0 Then
                    Dim sbSQL As StringBuilder = New StringBuilder
                    Dim params As List(Of SqlParameter) = New List(Of SqlParameter) 'SQLパラメータ
                    sbSQL.AppendLine("SELECT")
                    sbSQL.AppendLine("   MAINTE_ID")
                    sbSQL.AppendLine("  ,RESERVE_ID")
                    sbSQL.AppendLine("  ,ASSET_NO")
                    sbSQL.AppendLine("  ,COUNT(*) OVER () AS TotalCount ")
                    sbSQL.AppendLine("FROM T_RESERVE_MAINTE")
                    sbSQL.AppendLine("WHERE RESERVE_ID = @RESERVE_ID")
                    sbSQL.AppendLine("AND   ASSET_NO = @ASSET_NO")
                    params.Add(New SqlParameter($"@RESERVE_ID", lReserveId))
                    params.Add(New SqlParameter($"@ASSET_NO", modelDtl.ASSET_NO))
                    Dim modelSearch As DbSqlQuery(Of SearchMainteViewModel) = db.SearchMainteView.SqlQuery(sbSQL.ToString, params.ToArray)
                    viewSearch = New List(Of SearchMainteViewModel)  '結果View
                    viewSearch = modelSearch.ToList
                End If

                If rowMentInfo("MAINTE_EXEC_FLG").ToString = Misc.MainteExecFlg.No Then
                    '①その予約のメンテナンス区分=N
                    '  予約IDと資産番号が合致するメンテナンスデータを削除　※空振りでもよい
                    If modelDtl.RESERVE_ID <> 0 AndAlso viewSearch.Count > 0 Then
                        modelMainte = db.LendingMainteEntry.Find(viewSearch(0).MAINTE_ID)

                        '削除を実行する
                        db.LendingMainteEntry.Remove(modelMainte)
                    End If
                Else
                    '②その予約のメンテナンス区分=Y
                    If modelDtl.RESERVE_ID <> 0 AndAlso viewSearch.Count > 0 Then
                        '  予約IDと資産番号でメンテナンスデータを検索
                        '  ②-1　あった場合
                        '    そのメンテナンスデータの期間をストアドが返却する期間に変更
                        modelMainte = db.LendingMainteEntry.Find(viewSearch(0).MAINTE_ID)

                        If rowMentInfo("MAINTE_DIV").ToString = Misc.MainteDiv.Del Then
                            '削除を実行する
                            db.LendingMainteEntry.Remove(modelMainte)
                        Else
                            modelMainte.RESERVE_ID = lReserveId
                            modelMainte.ASSET_NO = modelDtl.ASSET_NO
                            modelMainte.MAINTE_START_DT = rowMentInfo("ST_DT")
                            modelMainte.MAINTE_END_DT = rowMentInfo("END_DT")
                            modelMainte.MAINTE_KBN = MainteKbn.Mainte

                            modelMainte.UPD_DATETIME = CommonRfid.getNowDate
                            modelMainte.UPD_EMPLOYEE_CD = loginInfo.EmployeeCd
                            modelMainte.UPD_EMPLOYEE_NAME = loginInfo.EmployeeNm
                            modelMainte.UPD_TASK = sTitle

                            '更新を実行する
                            db.Entry(modelMainte).State = Entity.EntityState.Modified
                        End If

                    Else
                        '  ②-2　なかった場合
                        '    新規登録
                        If rowMentInfo("MAINTE_DIV").ToString <> Misc.MainteDiv.Del Then
                            modelMainte = New LendingMainteEntryModel

                            modelMainte.RESERVE_ID = lReserveId
                            modelMainte.ASSET_NO = modelDtl.ASSET_NO
                            modelMainte.MAINTE_START_DT = rowMentInfo("ST_DT")
                            modelMainte.MAINTE_END_DT = rowMentInfo("END_DT")
                            modelMainte.MAINTE_KBN = MainteKbn.Mainte
                            modelMainte.REMARK = String.Empty

                            modelMainte.INS_DATETIME = CommonRfid.getNowDate
                            modelMainte.INS_EMPLOYEE_CD = loginInfo.EmployeeCd
                            modelMainte.INS_EMPLOYEE_NAME = loginInfo.EmployeeNm
                            modelMainte.UPD_DATETIME = CommonRfid.getNowDate
                            modelMainte.UPD_EMPLOYEE_CD = loginInfo.EmployeeCd
                            modelMainte.UPD_EMPLOYEE_NAME = loginInfo.EmployeeNm
                            modelMainte.UPD_TASK = sTitle

                            db.LendingMainteEntry.Add(modelMainte)
                        End If
                    End If

                    db.SaveChanges()

                End If
            Next

        End Sub
#End Region

#Region "予約番号存在チェック処理"
        ''' <summary>
        ''' 予約番号存在チェック処理 2022/07/25 add sv sato
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function ExistReserveId(ByVal reserveId As Long) As Boolean
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.Append("SELECT * FROM T_RESERVE WHERE RESERVE_ID = " & reserveId)
                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                '存在しない
                If IsNothing(tbl) OrElse tbl.Rows.Count <= 0 Then
                    Return False
                End If

                Return True
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "予約番号シーケンス 採番処理"
        ''' <summary>
        ''' 予約番号シーケンス　採番処理 2022/07/25 add sv sato
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetNextReserveId() As Integer
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.Append("select next value for RESERVE_ID_SEQUENCE")
                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                '存在しない
                If IsNothing(tbl) OrElse tbl.Rows.Count <= 0 Then
                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                End If

                Return CInt(tbl.Rows(0)(0))
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "棚卸IDシーケンス 採番処理"
        ''' <summary>
        ''' 棚卸IDシーケンス　採番処理 20231013 ADD Shirakawa
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetNextReserveInvId() As Integer
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.Append("select next value for INV_ID_SEQUENCE")
                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                '存在しない
                If IsNothing(tbl) OrElse tbl.Rows.Count <= 0 Then
                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                End If

                Return CInt(tbl.Rows(0)(0))
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "貸出中(遅延)資産の表示期間取得"
        ''' <summary>
        ''' 貸出中(遅延)資産の表示期間取得
        ''' </summary>
        ''' <param name="assetNolist">対象資産番号</param>
        ''' <returns>
        ''' 貸出開始、終了日情報
        ''' </returns>
        Public Shared Function GetDelayReserveInfo(assetNolist As String) As DataTable
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("MIN(PR_ST_DT) AS PR_ST_DT ")
                sbSQL.AppendLine(",MAX(PR_END_DT) AS PR_END_DT ")
                sbSQL.AppendLine("FROM T_RESERVE RSV ")
                sbSQL.AppendLine("LEFT JOIN T_RESERVE_DETAIL RESD ")
                sbSQL.AppendLine("ON RSV.RESERVE_ID = RESD.RESERVE_ID ")
                sbSQL.AppendLine("LEFT JOIN M_ASSET ASSET ")
                sbSQL.AppendLine("ON RESD.ASSET_NO = ASSET.ASSET_NO ")
                sbSQL.AppendLine("WHERE RSV.RESERVE_STATUS = '" & Misc.ReserveStatus.Lending & "' ")
                sbSQL.AppendLine("AND RSV.REAL_END_DT < CONVERT(int, CONVERT(VARCHAR, GETDATE(), 112)) ")
                sbSQL.AppendLine("AND ASSET.ASSET_NO IN (" & assetNolist & ") ")

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                Return tbl
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

    End Class
End Namespace