Imports System
Imports System.Collections.Generic
Imports System.Text
'[CC431] ADD Start 20261007
Imports System.Data.SqlClient
'[CC431] ADD End 20261007
Imports Asetra.Misc

Public Class ToolInvAsset

    Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    ''' <summary>
    ''' 引数にとったアセットが誰にロックされているかを取得する
    ''' </summary>
    ''' <param name="sAssetNoList">アセットNoリスト</param>
    ''' <returns>
    ''' DataTable
    ''' </returns>
    Public Shared Function GetLockUsers(ByVal invId As Long, ByVal invLocL As String, ByVal sAssetNoList As List(Of String)) As List(Of String)
        Dim tbl As New DataTable
        Dim ret = New List(Of String)
        Dim sbSQL As StringBuilder = New StringBuilder
        Dim clsDB As clsSV_DB = New clsSV_DB
        If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
            '接続失敗
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        End If
        Try
            'M_CATEGORYテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT LOCK_EMPLOYEE_CD ")
            sbSQL.AppendLine("FROM T_INV_ASSET ")
            sbSQL.AppendLine($"WHERE ASSET_NO IN ('{String.Join("','", sAssetNoList)}') ")
            sbSQL.AppendLine($" AND INV_ID = {invId}")
            sbSQL.AppendLine($" AND INV_LOC_L_CD = '{invLocL}'")
            sbSQL.AppendLine(" GROUP BY LOCK_EMPLOYEE_CD")
            tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
            For Each r As DataRow In tbl.Rows
                If (Not IsDBNull(r("LOCK_EMPLOYEE_CD"))) Then
                    Dim lockEmployeeCd = r.Field(Of String)("LOCK_EMPLOYEE_CD")
                    ret.Add(lockEmployeeCd)
                End If

            Next

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

    ''' <summary>
    ''' 引数にとったアセットが編集可能であるか（ロックされていないか）を取得する
    ''' </summary>
    ''' <param name="assetNo">アセットNo</param>
    ''' <returns>true: 編集可能</returns>
    Public Shared Function IsEditable(invId As Long, invLocL As String, assetNo As String, userCd As String) As Boolean
        Dim tbl As New DataTable
        Dim ret = New List(Of String)
        Dim sbSQL As StringBuilder = New StringBuilder
        Dim clsDB As clsSV_DB = New clsSV_DB
        If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
            '接続失敗
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        End If
        Try
            'M_CATEGORYテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT LOCK_EMPLOYEE_CD ")
            sbSQL.AppendLine("FROM T_INV_ASSET ")
            sbSQL.AppendLine($"WHERE ASSET_NO = '{assetNo}' ")
            sbSQL.AppendLine($" AND INV_ID = {invId}")
            sbSQL.AppendLine($" AND INV_LOC_L_CD = '{invLocL}'")

            tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

            If tbl.Rows.Count = 1 AndAlso
                Not String.IsNullOrEmpty(tbl.Rows(0).Field(Of String)("LOCK_EMPLOYEE_CD")) AndAlso
                tbl.Rows(0).Field(Of String)("LOCK_EMPLOYEE_CD") <> userCd Then
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

        Return False

    End Function

    ''' <summary>
    ''' 引数にとったアセットが編集可能であるか（ロックされていないか）を取得する
    ''' </summary>
    ''' <param name="assetNo">アセットNo</param>
    ''' <returns>true: 編集可能</returns>
    Public Shared Function GetInvAsset(invId As Long, invLocL As String, assetNo As String, userCd As String) As Boolean
        Dim tbl As New DataTable
        Dim ret = New List(Of String)
        Dim sbSQL As StringBuilder = New StringBuilder
        Dim clsDB As clsSV_DB = New clsSV_DB
        If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
            '接続失敗
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        End If
        Try
            'M_CATEGORYテーブルから取得するSQLを作成
            sbSQL.AppendLine("SELECT LOCK_EMPLOYEE_CD ")
            sbSQL.AppendLine("FROM T_INV_ASSET ")
            sbSQL.AppendLine($"WHERE ASSET_NO = '{assetNo}' ")
            sbSQL.AppendLine($" AND INV_ID = {invId}")
            sbSQL.AppendLine($" AND INV_LOC_L_CD = '{invLocL}'")

            tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

            If tbl.Rows.Count = 1 AndAlso
                Not String.IsNullOrEmpty(tbl.Rows(0).Field(Of String)("LOCK_EMPLOYEE_CD")) AndAlso
                tbl.Rows(0).Field(Of String)("LOCK_EMPLOYEE_CD") <> userCd Then
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

        Return False

    End Function


    ''' <summary>
    ''' 引数にとったassetNoのレコードを取得する
    ''' </summary>
    ''' <param name="assetNo">アセットNo</param>
    Public Shared Function GetInvAsset(invId As Long, invLocL As String, assetNo As String) As DataTable
        Dim tbl As New DataTable
        Dim sbSQL As StringBuilder = New StringBuilder
        Dim clsDB As clsSV_DB = New clsSV_DB
        If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
            '接続失敗
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        End If
        Try
            sbSQL.AppendLine("SELECT * ")
            sbSQL.AppendLine("FROM T_INV_ASSET ")
            sbSQL.AppendLine($"WHERE ASSET_NO = '{assetNo}' ")
            sbSQL.AppendLine($" AND INV_ID = {invId}")
            sbSQL.AppendLine($" AND INV_LOC_L_CD = '{invLocL}'")

            tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

            Return tbl

        Catch ex As Exception
            logger.Error(ex.Message)
            logger.Error(sbSQL)
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        Finally
            'DB CLOSE
            clsDB.SVSubCloseConnection()
        End Try

        Return Nothing

    End Function

    ''' <summary>
    ''' 引数にとったassetNoのレコードを取得する
    ''' </summary>
    Public Shared Function GetInvAssets(invId As Long, invLocL As String, assetNos As List(Of String)) As DataTable
        Dim tbl As New DataTable
        Dim sbSQL As StringBuilder = New StringBuilder
        Dim clsDB As clsSV_DB = New clsSV_DB
        If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
            '接続失敗
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        End If
        Try
            sbSQL.AppendLine("SELECT * ")
            sbSQL.AppendLine("FROM T_INV_ASSET ")
            sbSQL.AppendLine($"WHERE ASSET_NO IN ('{String.Join("','", assetNos)}') ")
            sbSQL.AppendLine($" AND INV_ID = {invId}")
            sbSQL.AppendLine($" AND INV_LOC_L_CD = '{invLocL}'")

            tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

            Return tbl

        Catch ex As Exception
            logger.Error(ex.Message)
            logger.Error(sbSQL)
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        Finally
            'DB CLOSE
            clsDB.SVSubCloseConnection()
        End Try

        Return Nothing

    End Function


    ''' <summary>
    ''' TInvAsset前レコード取得する
    ''' </summary>
    Public Shared Function GetMyAsset(invId As Long, invLocL As String) As DataTable
        Dim tbl As New DataTable
        Dim sbSQL As StringBuilder = New StringBuilder
        Dim clsDB As clsSV_DB = New clsSV_DB
        If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
            '接続失敗
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        End If
        Try
            sbSQL.AppendLine("SELECT * ")
            sbSQL.AppendLine("FROM T_INV_ASSET ")
            sbSQL.AppendLine("WHERE INV_LOC_L_CD = " + invLocL)
            sbSQL.AppendLine($" AND INV_ID = {invId}")

            tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
            Return tbl

        Catch ex As Exception
            logger.Error(ex.Message)
            logger.Error(sbSQL)
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        Finally
            'DB CLOSE
            clsDB.SVSubCloseConnection()
        End Try

        Return Nothing

    End Function

    ''' <summary>
    ''' 引数にとったロケーションの棚卸資産の変更後稼働区分が入っていないものがあるかを取得する
    ''' </summary>
    ''' <param name="invId">棚卸ID</param>
    ''' <param name="invLocL">ロケーション大区分コード</param>
    ''' <returns>true: 変更後稼働区分が入っている,false: 変更後稼働区分が入っていないものがある</returns>
    Public Shared Function CheckOpeStatus(invId As String, invLocL As List(Of String)) As Boolean
        Dim tbl As New DataTable
        Dim ret = New List(Of String)
        Dim sbSQL As StringBuilder = New StringBuilder
        Dim clsDB As clsSV_DB = New clsSV_DB
        If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
            '接続失敗
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        End If
        Try
            Dim wloc As String = String.Empty
            For Each loc As String In invLocL
                If Not String.IsNullOrEmpty(wloc) Then
                    wloc += ","
                End If
                wloc += "'" & loc & "'"
            Next

            'T_INV_ASSETテーブルから変更後稼働区分が入っていないものを取得するSQLを作成
            sbSQL.AppendLine("SELECT ASSET_NO ")
            sbSQL.AppendLine("FROM T_INV_ASSET ")
            sbSQL.AppendLine($"WHERE TRIM(ISNULL(OPE_STATUS,''))='' ")
            sbSQL.AppendLine($" AND INV_ID = {invId}")
            sbSQL.AppendLine($" AND INV_LOC_L_CD IN ({wloc})")

            tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

            If tbl.Rows.Count > 0 Then
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

        Return False

    End Function

    '[CC431] ADD Start 20261007
    ''' <summary>
    ''' 検索条件 変更後課コードの選択肢を取得する（棚卸資産Tの変更後課コードに存在するもの。権限は見ない）
    ''' </summary>
    ''' <param name="invId">棚卸ID</param>
    ''' <param name="invLocL">棚卸ロケ大区分コード</param>
    ''' <param name="selectSCodes">変更後設置場所コード（場所から選ぶ ON のとき、この変更後設置場所を持つ資産の変更後課コードに絞り込む。Nothing・空は絞り込まない）</param>
    ''' <returns>DataTable（LOC_CD, LOC_NAME）</returns>
    Public Shared Function GetChangeLocMData(invId As Long, invLocL As String, Optional selectSCodes As String() = Nothing) As DataTable
        Dim tbl As New DataTable
        Dim sbSQL As StringBuilder = New StringBuilder
        Dim params As New List(Of SqlParameter)
        Dim clsDB As clsSV_DB = New clsSV_DB
        If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
            '接続失敗
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        End If
        Try
            'M_LOCATION にないコードも選択肢に出す（名称なしでコードだけ表示）
            sbSQL.AppendLine("SELECT DISTINCT EX.CHANGE_LOC_M_CD AS LOC_CD")
            sbSQL.AppendLine("     , CASE WHEN M.LOC_NAME IS NULL THEN EX.CHANGE_LOC_M_CD")
            sbSQL.AppendLine("            ELSE CONCAT(EX.CHANGE_LOC_M_CD, '" & CommonConst.CodeNameDelimiter & "', M.LOC_NAME) END AS LOC_NAME")
            sbSQL.AppendLine("FROM T_INV_ASSET EX")
            sbSQL.AppendLine("LEFT JOIN M_LOCATION M")
            sbSQL.AppendLine("  ON M.LOC_DIV = '" & LOC_DIV.LocationM & "' AND M.LOC_L_CD = EX.INV_LOC_L_CD AND M.LOC_M_CD = EX.CHANGE_LOC_M_CD")
            sbSQL.AppendLine("WHERE EX.INV_ID = @INV_ID AND EX.INV_LOC_L_CD = @INV_LOC_L_CD")
            params.Add(New SqlParameter("@INV_ID", invId))
            params.Add(New SqlParameter("@INV_LOC_L_CD", CommonRfid.ToStringNvl(invLocL)))
            sbSQL.AppendLine("  AND ISNULL(EX.CHANGE_LOC_M_CD, '') <> ''")
            '場所から選ぶ ON：選択中の変更後設置場所で絞り込む
            Dim sCodes As String() = CommonRfid.GetSelectedValues(selectSCodes)
            If sCodes.Length > 0 Then
                Dim names As New List(Of String)
                For i As Integer = 0 To sCodes.Length - 1
                    names.Add($"@SEL_S_CD{i}")
                    params.Add(New SqlParameter($"@SEL_S_CD{i}", sCodes(i)))
                Next
                sbSQL.AppendLine("  AND EX.CHANGE_LOC_S_CD IN (" & String.Join(", ", names) & ")")
            End If
            sbSQL.AppendLine("ORDER BY LOC_CD")

            tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

        Catch ex As Exception
            logger.Error(ex.Message)
            logger.Error(sbSQL)
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        Finally
            'DB CLOSE
            clsDB.SVSubCloseConnection()
        End Try

        Return tbl

    End Function

    ''' <summary>
    ''' 検索条件 新変更後設置場所コードの選択肢を取得する（棚卸資産Tの変更後設置場所コードに存在するもの。権限は見ない）
    ''' 場所コードで 1 件にまとめ、名称は代表の 1 件（中区分コード順で最初のもの）。
    ''' 名称は M_LOCATION から取得し、変更後課コードが空の行はロケ中区分コードで結び付ける
    ''' </summary>
    ''' <param name="invId">棚卸ID</param>
    ''' <param name="invLocL">棚卸ロケ大区分コード</param>
    ''' <param name="selectMCodes">変更後課コード（場所から選ぶ OFF のとき、この変更後課コードの資産に絞り込む。Nothing・空は絞り込まない）</param>
    ''' <returns>DataTable（LOC_CD, LOC_NAME）</returns>
    Public Shared Function GetChangeLocSData(invId As Long, invLocL As String, Optional selectMCodes As String() = Nothing) As DataTable
        Dim tbl As New DataTable
        Dim sbSQL As StringBuilder = New StringBuilder
        Dim params As New List(Of SqlParameter)
        Dim clsDB As clsSV_DB = New clsSV_DB
        If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
            '接続失敗
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        End If
        Try
            sbSQL.AppendLine("SELECT X.LOC_CD, X.LOC_NAME FROM (")
            sbSQL.AppendLine("  SELECT P.CHANGE_LOC_S_CD AS LOC_CD")
            sbSQL.AppendLine("       , CASE WHEN S.LOC_NAME IS NULL THEN P.CHANGE_LOC_S_CD")
            sbSQL.AppendLine("              ELSE CONCAT(P.CHANGE_LOC_S_CD, '" & CommonConst.CodeNameDelimiter & "', S.LOC_NAME) END AS LOC_NAME")
            '場所コードごとに代表の 1 件（中区分コード順で最初のもの）
            sbSQL.AppendLine("       , ROW_NUMBER() OVER (PARTITION BY P.CHANGE_LOC_S_CD ORDER BY P.M_CD) AS RN")
            '先に（場所コード、中区分）の組にしてから M_LOCATION と結び付ける。変更後課コードが空ならロケ中区分コード
            sbSQL.AppendLine("  FROM (SELECT DISTINCT EX.CHANGE_LOC_S_CD")
            sbSQL.AppendLine("             , COALESCE(NULLIF(EX.CHANGE_LOC_M_CD, ''), EX.LOC_M_CD) AS M_CD")
            sbSQL.AppendLine("        FROM T_INV_ASSET EX")
            sbSQL.AppendLine("        WHERE EX.INV_ID = @INV_ID AND EX.INV_LOC_L_CD = @INV_LOC_L_CD")
            params.Add(New SqlParameter("@INV_ID", invId))
            params.Add(New SqlParameter("@INV_LOC_L_CD", CommonRfid.ToStringNvl(invLocL)))
            sbSQL.AppendLine("          AND ISNULL(EX.CHANGE_LOC_S_CD, '') <> ''")
            '場所から選ぶ OFF：選択中の変更後課コードで絞り込む
            Dim mCodes As String() = CommonRfid.GetSelectedValues(selectMCodes)
            If mCodes.Length > 0 Then
                Dim names As New List(Of String)
                For i As Integer = 0 To mCodes.Length - 1
                    names.Add($"@SEL_M_CD{i}")
                    params.Add(New SqlParameter($"@SEL_M_CD{i}", mCodes(i)))
                Next
                sbSQL.AppendLine("          AND EX.CHANGE_LOC_M_CD IN (" & String.Join(", ", names) & ")")
            End If
            sbSQL.AppendLine("       ) P")
            sbSQL.AppendLine("  LEFT JOIN M_LOCATION S")
            sbSQL.AppendLine("    ON S.LOC_DIV = '" & LOC_DIV.LocationS & "' AND S.LOC_L_CD = @INV_LOC_L_CD AND S.LOC_M_CD = P.M_CD AND S.LOC_S_CD = P.CHANGE_LOC_S_CD")
            sbSQL.AppendLine(") X")
            sbSQL.AppendLine("WHERE X.RN = 1")
            sbSQL.AppendLine("ORDER BY X.LOC_CD")

            tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

        Catch ex As Exception
            logger.Error(ex.Message)
            logger.Error(sbSQL)
            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        Finally
            'DB CLOSE
            clsDB.SVSubCloseConnection()
        End Try

        Return tbl

    End Function
    '[CC431] ADD End 20261007


End Class
