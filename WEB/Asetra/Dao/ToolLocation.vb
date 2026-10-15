Imports System.Data.SqlClient
Imports System.IO
Imports System.Net.Sockets
Imports Asetra.Misc

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：ローケーション関連ツール系 Data Access クラス
    ''' 作成日        ：2019/08/14      Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolLocation

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 大区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationLData() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    LOC_L_CD LOC_CD")
                'sbSQL.Append("  , LOC_NAME ")  '2019/09/12 upd nv iwamoto
                sbSQL.Append("  , CONCAT(LOC_L_CD, '" & CommonConst.CodeNameDelimiter & "', LOC_NAME) AS LOC_NAME ")
                sbSQL.Append("FROM  M_LOCATION ")
                sbSQL.Append("WHERE  LOC_DIV = '" & LOC_DIV.LocationL & "'")
                sbSQL.Append("ORDER BY LOC_L_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region

        '20231031 ADD Start Shirakawa
#Region "大区分ドロップダウンリストデータ取得 棚卸実績照会用"
        ''' <summary>
        ''' 大区分ドロップダウンリストデータ取得 棚卸実績照会用
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationLDataInvAseetHistory(ByVal invId As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    M_LOCATION.LOC_L_CD LOC_CD")
                sbSQL.Append("  , CONCAT(M_LOCATION.LOC_L_CD, '" & CommonConst.CodeNameDelimiter & "', M_LOCATION.LOC_NAME) AS LOC_NAME ")
                sbSQL.Append("FROM  M_LOCATION ")
                sbSQL.Append("LEFT JOIN T_INV_DETAIL ")
                sbSQL.Append("ON M_LOCATION.LOC_L_CD = T_INV_DETAIL.INV_LOC_L_CD ")
                sbSQL.Append("WHERE  LOC_DIV = '" & LOC_DIV.LocationL & "'")
                sbSQL.Append("AND  T_INV_DETAIL.INV_ID = '" & invId & "'")         '棚卸ID
                sbSQL.Append("ORDER BY M_LOCATION.LOC_L_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region
        '20231031 ADD End Shirakawa

#Region "中区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 中区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <param name="selectLCode">大区分コード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationMData(ByVal selectLCode As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    LOC_M_CD LOC_CD")
                'sbSQL.Append("  , LOC_NAME ")  '2019/09/12 upd nv iwamoto
                sbSQL.Append("  , CONCAT(LOC_M_CD, '" & CommonConst.CodeNameDelimiter & "', LOC_NAME) AS LOC_NAME ")
                sbSQL.Append("FROM  M_LOCATION ")
                sbSQL.Append("WHERE  LOC_DIV = '" & LOC_DIV.LocationM & "' ")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("  AND LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(selectLCode)))
                'sbSQL.Append("  AND LOC_L_CD = '" & selectLCode & "' ")
                sbSQL.Append("ORDER BY LOC_M_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region

#Region "小区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <param name="selectLCode">大区分コード</param>
        ''' <param name="selectMCode">中区分コード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCode As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    LOC_S_CD LOC_CD")
                'sbSQL.Append("  , LOC_NAME ")  '2019/09/12 upd nv iwamoto
                sbSQL.Append("  , CONCAT(LOC_S_CD, '" & CommonConst.CodeNameDelimiter & "', LOC_NAME) AS LOC_NAME ")
                sbSQL.Append("FROM M_LOCATION ")
                sbSQL.Append("WHERE LOC_DIV = '" & LOC_DIV.LocationS & "' ")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("  AND LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(selectLCode)))
                sbSQL.Append("  AND LOC_M_CD = @LOC_M_CD ")
                params.Add(New SqlParameter($"@LOC_M_CD", CommonRfid.ToStringNvl(selectMCode)))
                'sbSQL.Append("  AND LOC_L_CD = '" & selectLCode & "' ")
                'sbSQL.Append("  AND LOC_M_CD = '" & selectMCode & "' ")
                sbSQL.Append("ORDER BY LOC_S_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

            Catch ex As Exception
                'SQL ERR
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
        ''' 小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <param name="selectLCode">大区分コード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationSData(ByVal selectLCode As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    LOC_S_CD AS LOC_CD")
                sbSQL.Append("  , LOC_M_CD AS LOC_M_CD")
                'sbSQL.Append("  , LOC_NAME ")  '2019/09/12 upd nv iwamoto
                sbSQL.Append("  , CONCAT(LOC_S_CD, '" & CommonConst.CodeNameDelimiter & "', LOC_NAME) AS LOC_NAME ")
                sbSQL.Append("FROM M_LOCATION ")
                sbSQL.Append("WHERE LOC_DIV = '" & LOC_DIV.LocationS & "' ")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("  AND LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(selectLCode)))
                'sbSQL.Append("  AND LOC_L_CD = '" & selectLCode & "' ")
                'sbSQL.Append("  AND LOC_M_CD = '" & selectMCode & "' ")
                sbSQL.Append("ORDER BY LOC_S_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function

#End Region

#Region "大区分ドロップダウンリストデータ取得（所属ロケのみを取得）"
        ''' <summary>
        ''' 大区分ドロップダウンリストデータ取得（所属ロケのみを取得）
        ''' </summary>
        ''' <param name="loginInfo">ログイン者情報</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <param name="isEditMode">True=更新可能ロケを返す,False=照会可能ロケを返す</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationLData(loginInfo As RfidEntity.LoginUser, authInfo As AuthInfo, Optional ByVal isEditMode As Boolean = False) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try

                'SQLパラメータ
                Dim _params As New List(Of SqlParameter)

                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    MLOC.LOC_L_CD LOC_CD")
                sbSQL.Append("  , CONCAT(MLOC.LOC_L_CD, '" & CommonConst.CodeNameDelimiter & "', MLOC.LOC_NAME) AS LOC_NAME ")
                sbSQL.Append("FROM  M_LOCATION AS MLOC ")
                sbSQL.Append("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.Append("ON MLOC.LOC_L_CD = ULOC.LOC_L_CD ")
                _params.Add(New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd))
                _params.Add(New SqlParameter("@ReadAuthFlg", authInfo.ReadAuthFlg))
                _params.Add(New SqlParameter("@UpdAuthFlg", authInfo.UpdAuthFlg))
                _params.Add(New SqlParameter("@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                _params.Add(New SqlParameter("@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                _params.Add(New SqlParameter("@UpdAuthOption", authInfo.AuthOption))

                sbSQL.Append(" WHERE  MLOC.LOC_DIV = '" & LOC_DIV.LocationL & "'")
                If isEditMode Then
                    '更新モードの場合、更新できる管理ロケーションコードのみ取得する。
                    sbSQL.AppendLine("AND ULOC.UPD_FLG = '" & UpdFlg.Yes & "'")
                End If
                sbSQL.Append(" GROUP BY MLOC.LOC_L_CD,MLOC.LOC_NAME")
                sbSQL.Append(" ORDER BY MLOC.LOC_L_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, _params)
            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region

#Region "中区分ドロップダウンリストデータ取得（所属ロケのみを取得）"
        ''' <summary>
        ''' 中区分ドロップダウンリストデータ取得（所属ロケのみを取得）
        ''' </summary>
        ''' <param name="selectLCode">大区分コード</param>
        ''' <param name="loginInfo">ログイン者情報</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <param name="isEditMode">True=更新可能ロケを返す,False=照会可能ロケを返す</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationMData(ByVal selectLCode As String, loginInfo As RfidEntity.LoginUser, authInfo As AuthInfo, Optional ByVal isEditMode As Boolean = False) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    MLOC.LOC_M_CD LOC_CD")
                sbSQL.Append("  , CONCAT(MLOC.LOC_M_CD, '" & CommonConst.CodeNameDelimiter & "', MLOC.LOC_NAME) AS LOC_NAME ")
                sbSQL.Append("FROM  M_LOCATION AS MLOC ")
                sbSQL.Append("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.Append("ON MLOC.LOC_L_CD = ULOC.LOC_L_CD ")
                sbSQL.Append("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = MLOC.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
                params.Add(New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd))
                params.Add(New SqlParameter("@ReadAuthFlg", authInfo.ReadAuthFlg))
                params.Add(New SqlParameter("@UpdAuthFlg", authInfo.UpdAuthFlg))
                params.Add(New SqlParameter("@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                params.Add(New SqlParameter("@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                params.Add(New SqlParameter("@UpdAuthOption", authInfo.AuthOption))

                sbSQL.Append("WHERE  MLOC.LOC_DIV = '" & LOC_DIV.LocationM & "' ")
                sbSQL.Append("  AND MLOC.LOC_L_CD = @LOC_L_CD ")
                If isEditMode Then
                    '更新モードの場合、更新できる管理ロケーションコードのみ取得する。
                    sbSQL.AppendLine("AND ULOC.UPD_FLG = '" & UpdFlg.Yes & "'")
                End If
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(selectLCode)))
                sbSQL.Append("ORDER BY MLOC.LOC_M_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region

#Region "小区分ドロップダウンリストデータ取得（所属ロケのみを取得）"
        ''' <summary>
        ''' 小区分ドロップダウンリストデータ取得（所属ロケのみを取得）
        ''' </summary>
        ''' <param name="selectLCode">大区分コード</param>
        ''' <param name="selectMCode">中区分コード</param>
        ''' <param name="loginInfo">ログイン者情報</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCode As String, loginInfo As RfidEntity.LoginUser, authInfo As AuthInfo) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    MLOC.LOC_S_CD LOC_CD")
                sbSQL.Append("  , CONCAT(MLOC.LOC_S_CD, '" & CommonConst.CodeNameDelimiter & "', MLOC.LOC_NAME) AS LOC_NAME ")
                sbSQL.Append("FROM M_LOCATION AS MLOC ")
                sbSQL.Append("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.Append("ON MLOC.LOC_L_CD = ULOC.LOC_L_CD ")
                sbSQL.Append("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = MLOC.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")

                params.Add(New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd))
                params.Add(New SqlParameter("@ReadAuthFlg", authInfo.ReadAuthFlg))
                params.Add(New SqlParameter("@UpdAuthFlg", authInfo.UpdAuthFlg))
                params.Add(New SqlParameter("@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                params.Add(New SqlParameter("@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                params.Add(New SqlParameter("@UpdAuthOption", authInfo.AuthOption))

                sbSQL.Append("WHERE MLOC.LOC_DIV = '" & LOC_DIV.LocationS & "' ")
                sbSQL.Append("  AND MLOC.LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(selectLCode)))
                sbSQL.Append("  AND MLOC.LOC_M_CD = @LOC_M_CD ")
                params.Add(New SqlParameter($"@LOC_M_CD", CommonRfid.ToStringNvl(selectMCode)))
                sbSQL.Append("ORDER BY MLOC.LOC_S_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function

#End Region

        '[CC431] ADD Start 20261007
#Region "検索条件の選択肢（指定したテーブルに存在するロケーションのみを取得）"
        ''' <summary>
        ''' 選択肢を「このテーブルに存在するもの」に絞るための条件
        ''' 画面ごとの違いは作成メソッド（Shared）にまとめ、選択肢を取得する SQL は共通にする
        ''' ※テーブル名・列名は作成メソッドの中で固定の文字列にし、画面の入力は使わない
        ''' </summary>
        Public Class LocExistTarget
            ''' <summary>対象テーブル（SQL では別名 EX で参照する）</summary>
            Public Property TableName As String
            ''' <summary>ロケ大区分コードの列名</summary>
            Public Property ColLocL As String
            ''' <summary>ロケ中区分コードの列名</summary>
            Public Property ColLocM As String
            ''' <summary>ロケ小区分コードの列名</summary>
            Public Property ColLocS As String
            ''' <summary>対象テーブルの絞り込み条件（別名 EX。値はパラメータで渡す）</summary>
            Public Property Where As String
            ''' <summary>絞り込み条件のパラメータ（名前, 値）</summary>
            Public Property ParamValues As New Dictionary(Of String, Object)

            ''' <summary>
            ''' 絞り込み条件のパラメータを追加する
            ''' ※SqlParameter は複数のコマンドで使い回せないため、呼び出しごとに作成する
            ''' </summary>
            ''' <param name="params">追加先のパラメータ</param>
            Public Sub AddParams(params As List(Of SqlParameter))
                For Each p As KeyValuePair(Of String, Object) In ParamValues
                    params.Add(New SqlParameter(p.Key, p.Value))
                Next
            End Sub

            ''' <summary>
            ''' 棚卸資産T（実棚照会）
            ''' </summary>
            ''' <param name="invId">棚卸ID</param>
            ''' <param name="invLocL">棚卸ロケ大区分コード</param>
            Public Shared Function InvAsset(invId As Long, invLocL As String) As LocExistTarget
                Dim target As New LocExistTarget With {
                    .TableName = "T_INV_ASSET",
                    .ColLocL = "LOC_L_CD",
                    .ColLocM = "LOC_M_CD",
                    .ColLocS = "LOC_S_CD",
                    .Where = "EX.INV_ID = @EX_INV_ID AND EX.INV_LOC_L_CD = @EX_INV_LOC_L_CD"
                }
                target.ParamValues.Add("@EX_INV_ID", invId)
                target.ParamValues.Add("@EX_INV_LOC_L_CD", CommonRfid.ToStringNvl(invLocL))
                Return target
            End Function

            ''' <summary>
            ''' 資産M（資産一覧）。除却済以外
            ''' </summary>
            Public Shared Function Asset() As LocExistTarget
                Dim target As New LocExistTarget With {
                    .TableName = "M_ASSET",
                    .ColLocL = "LOC_L_CD",
                    .ColLocM = "LOC_M_CD",
                    .ColLocS = "LOC_S_CD",
                    .Where = "EX.ASSET_STATUS <> @EX_ASSET_STATUS"
                }
                target.ParamValues.Add("@EX_ASSET_STATUS", ASSET_STATUS.Removed)
                Return target
            End Function

            ''' <summary>
            ''' 資産M（資産在庫照会）。一覧の集計と同じ 1:保管中・2:貸出中
            ''' </summary>
            Public Shared Function StockAsset() As LocExistTarget
                Dim target As New LocExistTarget With {
                    .TableName = "M_ASSET",
                    .ColLocL = "LOC_L_CD",
                    .ColLocM = "LOC_M_CD",
                    .ColLocS = "LOC_S_CD",
                    .Where = "EX.ASSET_STATUS IN (@EX_ASSET_STATUS_STORAGE, @EX_ASSET_STATUS_LENDING)"
                }
                target.ParamValues.Add("@EX_ASSET_STATUS_STORAGE", ASSET_STATUS.Storage)
                target.ParamValues.Add("@EX_ASSET_STATUS_LENDING", ASSET_STATUS.Lending)
                Return target
            End Function

            ''' <summary>
            ''' 入庫予定T（資産ラベル発行）
            ''' </summary>
            Public Shared Function InAsset() As LocExistTarget
                Return New LocExistTarget With {
                    .TableName = "T_IN_ASSET",
                    .ColLocL = "LOC_L_CD",
                    .ColLocM = "LOC_M_CD",
                    .ColLocS = "LOC_S_CD",
                    .Where = "1 = 1"
                }
            End Function

            ''' <summary>
            ''' ロケーションM（資産履歴照会）
            ''' ロケーションマスタ自身を対象にするため、「ロケーションマスタにあり、権限があるもの」になる
            ''' </summary>
            Public Shared Function Location() As LocExistTarget
                Return New LocExistTarget With {
                    .TableName = "M_LOCATION",
                    .ColLocL = "LOC_L_CD",
                    .ColLocM = "LOC_M_CD",
                    .ColLocS = "LOC_S_CD",
                    .Where = "1 = 1"
                }
            End Function

            ''' <summary>
            ''' 過去棚卸実績T（棚卸実績照会）
            ''' </summary>
            ''' <param name="invId">棚卸ID</param>
            Public Shared Function InvResult(invId As Long) As LocExistTarget
                Dim target As New LocExistTarget With {
                    .TableName = "T_INV_RESULT",
                    .ColLocL = "LOC_L_CD",
                    .ColLocM = "LOC_M_CD",
                    .ColLocS = "LOC_S_CD",
                    .Where = "EX.INV_ID = @EX_INV_ID"
                }
                target.ParamValues.Add("@EX_INV_ID", invId)
                Return target
            End Function
        End Class

        ''' <summary>
        ''' 中区分ドロップダウンリストデータ取得（所属ロケのみ、かつ対象テーブルに存在するもののみを取得）
        ''' </summary>
        ''' <param name="selectLCode">大区分コード</param>
        ''' <param name="loginInfo">ログイン者情報</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <param name="target">対象テーブルの条件</param>
        ''' <param name="selectSCodes">小区分コード（場所から選ぶ ON のとき、この小区分が存在する中区分に絞り込む。Nothing・空は絞り込まない）</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationMDataExists(ByVal selectLCode As String, loginInfo As RfidEntity.LoginUser, authInfo As AuthInfo,
                                                      target As LocExistTarget, Optional ByVal selectSCodes As String() = Nothing) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成（権限の結合で同じ中区分が重複しないよう DISTINCT）
                sbSQL.Append("SELECT DISTINCT ")
                sbSQL.Append("    MLOC.LOC_M_CD LOC_CD")
                sbSQL.Append("  , CONCAT(MLOC.LOC_M_CD, '" & CommonConst.CodeNameDelimiter & "', MLOC.LOC_NAME) AS LOC_NAME ")
                sbSQL.Append("FROM  M_LOCATION AS MLOC ")
                sbSQL.Append("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.Append("ON MLOC.LOC_L_CD = ULOC.LOC_L_CD ")
                sbSQL.Append("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = MLOC.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
                params.Add(New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd))
                params.Add(New SqlParameter("@ReadAuthFlg", authInfo.ReadAuthFlg))
                params.Add(New SqlParameter("@UpdAuthFlg", authInfo.UpdAuthFlg))
                params.Add(New SqlParameter("@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                params.Add(New SqlParameter("@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                params.Add(New SqlParameter("@UpdAuthOption", authInfo.AuthOption))

                sbSQL.Append("WHERE  MLOC.LOC_DIV = '" & LOC_DIV.LocationM & "' ")
                sbSQL.Append("  AND MLOC.LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(selectLCode)))

                '対象テーブルに存在するもの
                sbSQL.Append("  AND EXISTS (SELECT 1 FROM " & target.TableName & " AS EX ")
                sbSQL.Append("              WHERE " & target.Where & " ")
                sbSQL.Append("                AND EX." & target.ColLocL & " = MLOC.LOC_L_CD ")
                sbSQL.Append("                AND EX." & target.ColLocM & " = MLOC.LOC_M_CD ")
                target.AddParams(params)
                '場所から選ぶ ON：選択中の小区分で絞り込む
                Dim sCodes As String() = CommonRfid.GetSelectedValues(selectSCodes)
                If sCodes.Length > 0 Then
                    Dim names As New List(Of String)
                    For i As Integer = 0 To sCodes.Length - 1
                        names.Add($"@SEL_S_CD{i}")
                        params.Add(New SqlParameter($"@SEL_S_CD{i}", sCodes(i)))
                    Next
                    sbSQL.Append("                AND EX." & target.ColLocS & " IN (" & String.Join(", ", names) & ") ")
                End If
                sbSQL.Append("             ) ")
                sbSQL.Append("ORDER BY LOC_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

            Catch ex As Exception
                'SQL ERR
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
        ''' 小区分ドロップダウンリストデータ取得（所属ロケのみ、かつ対象テーブルに存在するもののみを取得）
        ''' 小区分コードで 1 件にまとめ、名称は代表の 1 件（中区分コード順で最初のもの）
        ''' </summary>
        ''' <param name="selectLCode">大区分コード</param>
        ''' <param name="loginInfo">ログイン者情報</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <param name="target">対象テーブルの条件</param>
        ''' <param name="selectMCodes">中区分コード（場所から選ぶ OFF のとき、この中区分の小区分に絞り込む。Nothing・空は絞り込まない）</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationSDataExists(ByVal selectLCode As String, loginInfo As RfidEntity.LoginUser, authInfo As AuthInfo,
                                                      target As LocExistTarget, Optional ByVal selectMCodes As String() = Nothing) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT X.LOC_CD, X.LOC_NAME FROM ( ")
                sbSQL.Append("  SELECT ")
                sbSQL.Append("      MLOC.LOC_S_CD LOC_CD")
                sbSQL.Append("    , CONCAT(MLOC.LOC_S_CD, '" & CommonConst.CodeNameDelimiter & "', MLOC.LOC_NAME) AS LOC_NAME ")
                '小区分コードごとに代表の 1 件（中区分コード順で最初のもの）
                sbSQL.Append("    , ROW_NUMBER() OVER (PARTITION BY MLOC.LOC_S_CD ORDER BY MLOC.LOC_M_CD) AS RN ")
                sbSQL.Append("  FROM M_LOCATION AS MLOC ")
                sbSQL.Append("  INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.Append("  ON MLOC.LOC_L_CD = ULOC.LOC_L_CD ")
                sbSQL.Append("  AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = MLOC.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
                params.Add(New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd))
                params.Add(New SqlParameter("@ReadAuthFlg", authInfo.ReadAuthFlg))
                params.Add(New SqlParameter("@UpdAuthFlg", authInfo.UpdAuthFlg))
                params.Add(New SqlParameter("@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                params.Add(New SqlParameter("@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                params.Add(New SqlParameter("@UpdAuthOption", authInfo.AuthOption))

                sbSQL.Append("  WHERE MLOC.LOC_DIV = '" & LOC_DIV.LocationS & "' ")
                sbSQL.Append("    AND MLOC.LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(selectLCode)))
                '場所から選ぶ OFF：選択中の中区分で絞り込む
                Dim mCodes As String() = CommonRfid.GetSelectedValues(selectMCodes)
                If mCodes.Length > 0 Then
                    Dim names As New List(Of String)
                    For i As Integer = 0 To mCodes.Length - 1
                        names.Add($"@SEL_M_CD{i}")
                        params.Add(New SqlParameter($"@SEL_M_CD{i}", mCodes(i)))
                    Next
                    sbSQL.Append("    AND MLOC.LOC_M_CD IN (" & String.Join(", ", names) & ") ")
                End If

                '対象テーブルに存在するもの（中区分・小区分の組）
                sbSQL.Append("    AND EXISTS (SELECT 1 FROM " & target.TableName & " AS EX ")
                sbSQL.Append("                WHERE " & target.Where & " ")
                sbSQL.Append("                  AND EX." & target.ColLocL & " = MLOC.LOC_L_CD ")
                sbSQL.Append("                  AND EX." & target.ColLocM & " = MLOC.LOC_M_CD ")
                sbSQL.Append("                  AND EX." & target.ColLocS & " = MLOC.LOC_S_CD) ")
                target.AddParams(params)
                sbSQL.Append(") X ")
                sbSQL.Append("WHERE X.RN = 1 ")
                sbSQL.Append("ORDER BY X.LOC_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region
        '[CC431] ADD End 20261007

#Region "所属ロケデータ取得(ロケ区分指定)"
        ''' <summary>
        ''' 所属ロケデータ取得(ロケ区分指定)
        ''' </summary>
        ''' <param name="locDiv">ロケ区分</param>
        ''' <param name="loginInfo">ログイン者情報</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        ''' <remarks>指定したロケ区分のロケーションマスタのうち、所属拠点のデータを取得する。コードのみで名称は取得しない。</remarks>
        Public Shared Function GetHubLocationFromDiv(ByVal locDiv As String, loginInfo As RfidEntity.LoginUser, authInfo As AuthInfo) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    LOC.LOC_L_CD ")
                sbSQL.Append(" ,  LOC.LOC_M_CD ")
                sbSQL.Append(" ,  LOC.LOC_S_CD ")
                sbSQL.Append("FROM  M_LOCATION AS LOC ")
                sbSQL.Append("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.Append("ON LOC.LOC_L_CD = ULOC.LOC_L_CD ")
                sbSQL.Append("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = LOC.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
                params.Add(New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd))
                params.Add(New SqlParameter("@ReadAuthFlg", authInfo.ReadAuthFlg))
                params.Add(New SqlParameter("@UpdAuthFlg", authInfo.UpdAuthFlg))
                params.Add(New SqlParameter("@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                params.Add(New SqlParameter("@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                params.Add(New SqlParameter("@UpdAuthOption", authInfo.AuthOption))

                sbSQL.Append("WHERE  LOC.LOC_DIV = '" & locDiv & "' ")
                sbSQL.Append("ORDER BY LOC.LOC_L_CD,LOC.LOC_M_CD,LOC.LOC_S_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region

#Region "管理ロケーションドロップダウンリストデータ取得"
        ''' <summary>
        ''' 管理ロケーションドロップダウンリストデータ取得
        ''' </summary>
        ''' <param name="sEmployeeCd">社員コード</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <param name="isEditMode">照会モードか、更新モードか</param>
        ''' <param name="sUpdAuthType">予約系処理時:Misc.UpdAuthType.UPD_AUTH_HUB 他:Misc.UpdAuthType.UPD_AUTH</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetMngLocationLData(sEmployeeCd As String, authInfo As AuthInfo, isEditMode As Boolean, sUpdAuthType As String) As DataTable
            Dim tbl As New DataTable
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("    LOC.LOC_L_CD LOC_CD")
                sbSQL.AppendLine("  , CONCAT(LOC.LOC_L_CD, '" & CommonConst.CodeNameDelimiter & "', LOC.LOC_NAME) AS LOC_NAME ")
                sbSQL.AppendLine("  , LOC_SHARE_FLG SHARED_FLG")
                sbSQL.AppendLine("FROM  M_LOCATION LOC")
                sbSQL.Append("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS AUTH ")
                sbSQL.Append("ON LOC.LOC_L_CD = AUTH.LOC_L_CD ")

                If isEditMode Then
                    '更新モードの場合、更新できる管理ロケーションコードのみ取得する。
                    sbSQL.AppendLine("AND AUTH.UPD_FLG = '" & UpdFlg.Yes & "'")
                End If
                sbSQL.AppendLine("WHERE  LOC.LOC_DIV = '" & LOC_DIV.LocationL & "'")
                sbSQL.AppendLine("GROUP BY LOC.LOC_L_CD,LOC.LOC_NAME,LOC_SHARE_FLG, AUTH.UPD_FLG  ")
                sbSQL.AppendLine("ORDER BY AUTH.UPD_FLG DESC, LOC.LOC_L_CD")

                Dim paramSqlList As List(Of SqlParameter) = New List(Of SqlParameter) 'SQLパラメータ
                paramSqlList.Add(New SqlParameter($"@EmployeeCd", sEmployeeCd))
                paramSqlList.Add(New SqlParameter($"@ReadAuthFlg", authInfo.ReadAuthFlg))
                paramSqlList.Add(New SqlParameter($"@UpdAuthFlg", authInfo.UpdAuthFlg))
                paramSqlList.Add(New SqlParameter($"@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                paramSqlList.Add(New SqlParameter($"@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                paramSqlList.Add(New SqlParameter($"@UpdAuthOption", authInfo.AuthOption))

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, paramSqlList)

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region

#Region "上位の大区分コード存在チェック"
        ''' <summary>
        ''' 上位の大区分コード存在チェック
        ''' </summary>
        ''' <param name="locLCd">ロケ大区分コード</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Shared Function IsExistLocLCd(locLCd As String) As Boolean
            '値が空の場合は半角スペースを設定
            If String.IsNullOrEmpty(locLCd) Then
                locLCd = " "
            End If

            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT LOC_L_CD ")
                sbSQL.Append("FROM M_LOCATION ")
                sbSQL.Append("WHERE  LOC_DIV = '" & LOC_DIV.LocationL & "'")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("  AND LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                'sbSQL.Append("  AND  LOC_L_CD = '" & locLCd & "'")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                If tbl.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "上位の中区分コード存在チェック"
        ''' <summary>
        ''' 上位の中区分コード存在チェック
        ''' </summary>
        ''' <param name="locLCd">ロケ大区分コード</param>
        ''' <param name="locMCd">ロケ中区分コード</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Shared Function IsExistLocMCd(locLCd As String, locMCd As String) As Boolean
            '値が空の場合は半角スペースを設定
            If String.IsNullOrEmpty(locMCd) Then
                locMCd = " "
            End If

            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT LOC_M_CD ")
                sbSQL.Append("FROM M_LOCATION ")
                sbSQL.Append("WHERE  LOC_DIV = '" & LOC_DIV.LocationM & "'")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("  AND LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                sbSQL.Append("  AND LOC_M_CD = @LOC_M_CD ")
                params.Add(New SqlParameter($"@LOC_M_CD", CommonRfid.ToStringNvl(locMCd)))
                'sbSQL.Append("  AND  LOC_L_CD = '" & locLCd & "'")
                'sbSQL.Append("  AND  LOC_M_CD = '" & locMCd & "'")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                If tbl.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

        '[CC431] ADD Start 20261005
#Region "小区分コード存在チェック"
        ''' <summary>
        ''' 小区分コード存在チェック（権限は見ない）
        ''' </summary>
        ''' <param name="locLCd">ロケ大区分コード</param>
        ''' <param name="locMCd">ロケ中区分コード</param>
        ''' <param name="locSCd">ロケ小区分コード</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Shared Function IsExistLocSCd(locLCd As String, locMCd As String, locSCd As String) As Boolean
            '値が空の場合は半角スペースを設定
            If String.IsNullOrEmpty(locMCd) Then
                locMCd = " "
            End If
            If String.IsNullOrEmpty(locSCd) Then
                locSCd = " "
            End If

            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT LOC_S_CD ")
                sbSQL.Append("FROM M_LOCATION ")
                sbSQL.Append("WHERE  LOC_DIV = '" & LOC_DIV.LocationS & "'")
                sbSQL.Append("  AND LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                sbSQL.Append("  AND LOC_M_CD = @LOC_M_CD ")
                params.Add(New SqlParameter($"@LOC_M_CD", CommonRfid.ToStringNvl(locMCd)))
                sbSQL.Append("  AND LOC_S_CD = @LOC_S_CD ")
                params.Add(New SqlParameter($"@LOC_S_CD", CommonRfid.ToStringNvl(locSCd)))

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                If tbl.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region
        '[CC431] ADD End 20261005

#Region "ロケ名称取得"
        ''' <summary>
        ''' ロケ名称取得
        ''' </summary>
        ''' <param name="locDiv">ロケ区分</param>
        ''' <param name="locLCd">ロケ大区分コード</param>
        ''' <param name="locMCd">ロケ中区分コード</param>
        ''' <param name="locSCd">ロケ小区分コード</param>
        ''' <returns>ロケ名称</returns>
        Public Shared Function GetLocationName(locDiv As String, locLCd As String, locMCd As String, locSCd As String) As String
            '値が空の場合は半角スペースを設定
            If String.IsNullOrEmpty(locLCd) Then
                locLCd = " "
            End If
            If String.IsNullOrEmpty(locMCd) Then
                locMCd = " "
            End If
            If String.IsNullOrEmpty(locSCd) Then
                locSCd = " "
            End If

            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT LOC_NAME ")
                sbSQL.Append("FROM M_LOCATION ")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("WHERE  LOC_DIV = @LOC_DIV ")
                params.Add(New SqlParameter($"@LOC_DIV", CommonRfid.ToStringNvl(locDiv)))
                sbSQL.Append("  AND LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                sbSQL.Append("  AND LOC_M_CD = @LOC_M_CD ")
                params.Add(New SqlParameter($"@LOC_M_CD", CommonRfid.ToStringNvl(locMCd)))
                sbSQL.Append("  AND LOC_S_CD = @LOC_S_CD ")
                params.Add(New SqlParameter($"@LOC_S_CD", CommonRfid.ToStringNvl(locSCd)))
                'sbSQL.Append("WHERE  LOC_DIV = '" & locDiv & "'")
                'sbSQL.Append("  AND  LOC_L_CD = '" & locLCd & "'")
                'sbSQL.Append("  AND  LOC_M_CD = '" & locMCd & "'")
                'sbSQL.Append("  AND  LOC_S_CD = '" & locSCd & "'")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                If tbl.Rows.Count = 0 Then
                    Return String.Empty
                Else
                    Return tbl.Rows(0).Item("LOC_NAME").ToString
                End If

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "更新可能拠点RFID運用拠点存在チェック"
        ''' <summary>
        ''' 更新可能拠点RFID運用拠点存在チェック
        ''' </summary>
        ''' <param name="loginInfo">ログイン者情報</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <returns>True:存在する False:存在しない</returns>
        ''' <remarks>ログイン担当者の更新可能拠点にRFID運用拠点が含まれているかをチェックする</remarks>
        Public Shared Function IsExistRFIDLoc(loginInfo As RfidEntity.LoginUser, authInfo As AuthInfo) As Boolean

            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.Append("SELECT ")
                sbSQL.Append(" LOC.LOC_L_CD LOC_CD ")
                sbSQL.Append("FROM  M_LOCATION AS LOC ")
                sbSQL.Append("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.Append("ON LOC.LOC_L_CD = ULOC.LOC_L_CD ")
                params.Add(New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd))
                params.Add(New SqlParameter("@ReadAuthFlg", authInfo.ReadAuthFlg))
                params.Add(New SqlParameter("@UpdAuthFlg", authInfo.UpdAuthFlg))
                params.Add(New SqlParameter("@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                params.Add(New SqlParameter("@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                params.Add(New SqlParameter("@UpdAuthOption", authInfo.AuthOption))
                sbSQL.Append(" WHERE  LOC.LOC_RFID_FLG = '" & LocRfidFlg.Yes & "' ")
                sbSQL.AppendLine(" AND ULOC.UPD_FLG = '" & UpdFlg.Yes & "' ")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                If tbl.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "RFID運用拠点チェック"
        ''' <summary>
        ''' RFID運用拠点チェック
        ''' </summary>
        ''' <param name="locLcd">ロケ大区分コード</param>
        ''' <returns>True:存在する False:存在しない</returns>
        ''' <remarks>引数のロケ大区分コードがRFID運用拠点かをチェックする</remarks>
        Public Shared Function IsRFIDLoc(locLcd As String) As Boolean
            If String.IsNullOrEmpty(locLcd) Then
                'ロケ大区分コードが指定されていない場合はFalse
                Return False
            End If

            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.Append("SELECT ")
                sbSQL.Append(" LOC.LOC_L_CD LOC_CD ")
                sbSQL.Append("FROM  M_LOCATION AS LOC ")
                sbSQL.Append(" WHERE  LOC.LOC_RFID_FLG = '" & LocRfidFlg.Yes & "' ")
                sbSQL.Append(" AND  LOC.LOC_L_CD = '" & locLcd & "' ")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                If tbl.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "更新可能なロケ大取得"
        ''' <summary>
        ''' 更新可能拠点取得
        ''' </summary>
        ''' <param name="loginInfo">ログイン者情報</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <returns>True:存在する False:存在しない</returns>
        ''' <remarks>更新可能なロケ大リストを取得する</remarks>
        Public Shared Function GetUpdatableLocLList(loginInfo As RfidEntity.LoginUser, authInfo As AuthInfo) As DataTable

            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.Append("SELECT ")
                sbSQL.Append(" DISTINCT LOC.LOC_L_CD LOC_CD ")
                sbSQL.Append("FROM  M_LOCATION AS LOC ")
                sbSQL.Append("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.Append("ON LOC.LOC_L_CD = ULOC.LOC_L_CD ")
                params.Add(New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd))
                params.Add(New SqlParameter("@ReadAuthFlg", authInfo.ReadAuthFlg))
                params.Add(New SqlParameter("@UpdAuthFlg", authInfo.UpdAuthFlg))
                params.Add(New SqlParameter("@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                params.Add(New SqlParameter("@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                params.Add(New SqlParameter("@UpdAuthOption", authInfo.AuthOption))
                sbSQL.Append(" WHERE ULOC.UPD_FLG = '" & UpdFlg.Yes & "' ")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                Return tbl

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "引数の各区分コードからロケ区分を取得します"
        ''' <summary>
        ''' 引数の各区分コードからロケ区分を取得します
        ''' </summary>
        ''' <param name="locLCd">ロケ大区分コード</param>
        ''' <param name="locMCd">ロケ中区分コード</param>
        ''' <param name="locSCd">ロケ小区分コード</param>
        ''' <returns></returns>
        Public Shared Function GetLocDiv(locLCd As String, locMCd As String, locSCd As String) As String
            Dim ret As String = String.Empty

            If (locLCd.Trim.Length <> 0 AndAlso locMCd.Trim.Length = 0 AndAlso locSCd.Trim.Length = 0) Then
                ret = LOC_DIV.LocationL
            ElseIf (locLCd.Trim.Length <> 0 AndAlso locMCd.Trim.Length <> 0 AndAlso locSCd.Trim.Length = 0) Then
                ret = LOC_DIV.LocationM
            ElseIf (locLCd.Trim.Length <> 0 AndAlso locMCd.Trim.Length <> 0 AndAlso locSCd.Trim.Length <> 0) Then
                ret = LOC_DIV.LocationS
            End If

            Return ret
        End Function
#End Region

#Region "データ取得(全項目)"
        ''' <summary>
        ''' データ取得(全項目)
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetLocationDataAll() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT *")
                sbSQL.Append("FROM  M_LOCATION ")
                'sbSQL.Append("WHERE  LOC_DIV = '" & LOC_DIV.LocationL & "'")
                sbSQL.Append("ORDER BY LOC_L_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                tbl.DefaultView.Sort = "LOC_DIV ASC, LOC_L_CD ASC, LOC_M_CD ASC, LOC_S_CD ASC"      '2019/12/18 ADD Inoue

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region

#Region "ロケーション使用チェック"
        ''' <summary>
        ''' ロケーション使用チェック(指定コードが、引数のもの以外のロケーションＭ内、資産Ｍに存在するか、否か)
        ''' </summary>
        ''' <param name="locDiv">ロケーション区分</param>
        ''' <param name="locLCd">ロケ大区分コード</param>
        ''' <param name="locMCd">ロケ中区分コード</param>
        ''' <param name="locSCd">ロケ小区分コード</param>
        ''' <param name="masterCheckSkip">マスタチェックスキップ</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Shared Function IsUseLoc(locDiv As String, ByVal locLCd As String, ByVal locMCd As String, ByVal locSCd As String, Optional ByVal masterCheckSkip As Boolean = False) As Boolean  '2021/12/16 nv miyaji
            'Public Shared Function IsUseLoc(ByVal locLCd As String, ByVal locMCd As String, ByVal locSCd As String) As Boolean  '2021/12/16 nv miyaji
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                If String.IsNullOrEmpty(locSCd.Trim) Then
                    '小区分以外の場合、ロケーションＭ内をチェックする

                    If Not masterCheckSkip Then                                 '2022/03/08 ファイル取込時のマスタチェックを廃止

                        'M_LOCATIONテーブルから取得するSQLを作成
                        sbSQL.AppendLine("SELECT DISTINCT LOC_L_CD ")
                        sbSQL.AppendLine("FROM M_LOCATION ")
                        '--2020/03/05 nv miyaji update start
                        sbSQL.AppendLine("WHERE (LOC_L_CD <> @LOC_L_CD ")
                        sbSQL.AppendLine("   OR  LOC_M_CD <> @LOC_M_CD ")
                        sbSQL.AppendLine("   OR  LOC_S_CD <> @LOC_S_CD )")
                        params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                        params.Add(New SqlParameter($"@LOC_M_CD", CommonRfid.ToStringNvl(locMCd)))
                        params.Add(New SqlParameter($"@LOC_S_CD", CommonRfid.ToStringNvl(locSCd)))
                        'sbSQL.AppendLine("WHERE (LOC_L_CD <> '" & locLCd & "'")
                        'sbSQL.AppendLine("   OR  LOC_M_CD <> '" & locMCd & "'")
                        'sbSQL.AppendLine("   OR  LOC_S_CD <> '" & locSCd & "')")

                        sbSQL.AppendLine("AND   LOC_L_CD = @LOC_L_CD ")
                        'sbSQL.AppendLine("AND   LOC_L_CD = '" & locLCd & "'")
                        If Not String.IsNullOrEmpty(locMCd.Trim) Then
                            sbSQL.AppendLine("AND   LOC_M_CD = @LOC_M_CD ")
                            'sbSQL.AppendLine("AND   LOC_M_CD = '" & locMCd & "'")
                        End If
                        tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                        'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                        '--2020/03/05 nv miyaji update end

                        If tbl.Rows.Count > 0 Then
                            Return True
                        End If
                    End If
                End If

                '資産Ｍ使用チェック
                sbSQL.Clear()
                params = New List(Of SqlParameter) '2020/03/05 nv miyaji add
                sbSQL.AppendLine("SELECT DISTINCT ASSET_NO ")
                sbSQL.AppendLine("FROM M_ASSET ")
                '--2020/03/05 nv miyaji update start
                sbSQL.AppendLine("WHERE ( LOC_L_CD = @LOC_L_CD ") '2021/12/16 nv miyaji
                'sbSQL.AppendLine("WHERE LOC_L_CD = @LOC_L_CD ") '2021/12/16 nv miyaji
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                'sbSQL.AppendLine("WHERE LOC_L_CD = '" & locLCd & "'")
                If Not String.IsNullOrEmpty(locMCd.Trim) Then
                    sbSQL.AppendLine("AND   LOC_M_CD = @LOC_M_CD ")
                    params.Add(New SqlParameter($"@LOC_M_CD", CommonRfid.ToStringNvl(locMCd)))
                    'sbSQL.AppendLine("AND   LOC_M_CD = '" & locMCd & "'")
                End If
                If Not String.IsNullOrEmpty(locSCd.Trim) Then
                    sbSQL.AppendLine("AND   LOC_S_CD = @LOC_S_CD ")
                    params.Add(New SqlParameter($"@LOC_S_CD", CommonRfid.ToStringNvl(locSCd)))
                    'sbSQL.AppendLine("AND   LOC_S_CD = '" & locSCd & "'")
                End If
                '--2021/12/16 nv miyaji add start
                sbSQL.AppendLine(")")
                If locDiv = LOC_DIV.LocationL Then
                    sbSQL.AppendLine("OR ( MNG_LOC_L_CD = @MNG_LOC_L_CD )")
                    params.Add(New SqlParameter($"@MNG_LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                End If
                '--2021/12/16 nv miyaji add end
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                If tbl.Rows.Count > 0 Then
                    Return True
                End If

                '入庫予定情報使用チェック
                sbSQL.Clear()
                params = New List(Of SqlParameter)
                sbSQL.AppendLine("SELECT DISTINCT ASSET_NO ")
                sbSQL.AppendLine("FROM T_IN_ASSET ")
                sbSQL.AppendLine("WHERE ( LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                If Not String.IsNullOrEmpty(locMCd.Trim) Then
                    sbSQL.AppendLine("AND   LOC_M_CD = @LOC_M_CD ")
                    params.Add(New SqlParameter($"@LOC_M_CD", CommonRfid.ToStringNvl(locMCd)))
                End If
                If Not String.IsNullOrEmpty(locSCd.Trim) Then
                    sbSQL.AppendLine("AND   LOC_S_CD = @LOC_S_CD ")
                    params.Add(New SqlParameter($"@LOC_S_CD", CommonRfid.ToStringNvl(locSCd)))
                End If
                sbSQL.AppendLine(")")
                If locDiv = LOC_DIV.LocationL Then
                    sbSQL.AppendLine("OR ( MNG_LOC_L_CD = @MNG_LOC_L_CD )")
                    params.Add(New SqlParameter($"@MNG_LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                End If
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                If tbl.Rows.Count > 0 Then
                    Return True
                End If

                'ロケ大区分、中区分の場合、所属拠点をチェック
                If locDiv = LOC_DIV.LocationL OrElse locDiv = LOC_DIV.LocationM Then                      '2023/08/08 Tomatsuri ADD
                    '--2021/12/16 nv miyaji add start
                    '所属拠点Ｍ使用チェック
                    sbSQL.Clear()
                    params = New List(Of SqlParameter) '2020/03/05 nv miyaji add
                    sbSQL.AppendLine("SELECT DISTINCT EMPLOYEE_CD ")
                    sbSQL.AppendLine("FROM M_EMPLOYEE_HUB ")
                    sbSQL.AppendLine("WHERE HUB_LOC_L_CD = @LOC_L_CD ")
                    params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                    If locDiv = LOC_DIV.LocationM Then
                        sbSQL.AppendLine("AND   HUB_LOC_M_CD = @LOC_M_CD ")
                        params.Add(New SqlParameter($"@LOC_M_CD", CommonRfid.ToStringNvl(locMCd)))
                    End If
                    tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                    '--2020/03/05 nv miyaji update end

                    If tbl.Rows.Count > 0 Then
                        Return True
                    End If
                    '--2021/12/16 nv miyaji add end
                End If                                                   '2023/08/08 Tomatsuri ADD

                '未使用の場合
                Return False

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "大ロケデータ取得"
        ''' <summary>
        ''' ロケ名称取得
        ''' </summary>
        ''' <param name="locLCd">ロケ大区分コード</param>
        ''' <returns>DataTable</returns>
        Public Shared Function GetLargeLocationData(locLCd As String) As DataTable

            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT * ")
                sbSQL.Append("FROM M_LOCATION ")
                sbSQL.Append("WHERE  LOC_DIV = @LOC_DIV ")
                params.Add(New SqlParameter($"@LOC_DIV", LOC_DIV.LocationL))
                sbSQL.Append("  AND LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function
#End Region


#Region "適切なロケーションであるかをチェックする"

        Public Shared Function IsAppropriateLocation(locLCd As String, locMCd As String, locSCd As String,
                                                     sEmployeeCd As String, authInfo As AuthInfo) As Boolean

            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    MLOC.LOC_M_CD LOC_CD")
                sbSQL.Append("  , CONCAT(MLOC.LOC_M_CD, '" & CommonConst.CodeNameDelimiter & "', MLOC.LOC_NAME) AS LOC_NAME ")
                sbSQL.Append("FROM  M_LOCATION AS MLOC ")
                sbSQL.Append("INNER JOIN dbo.AUTH_LOCATION_ULOC ( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AS ULOC ")
                sbSQL.Append("ON MLOC.LOC_L_CD = ULOC.LOC_L_CD ")
                sbSQL.Append("AND ((ULOC.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND ULOC.LOC_M_CD = MLOC.LOC_M_CD) OR (ULOC.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
                params.Add(New SqlParameter("@EmployeeCd", sEmployeeCd))
                params.Add(New SqlParameter("@ReadAuthFlg", authInfo.ReadAuthFlg))
                params.Add(New SqlParameter("@UpdAuthFlg", authInfo.UpdAuthFlg))
                params.Add(New SqlParameter("@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
                params.Add(New SqlParameter("@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
                params.Add(New SqlParameter("@UpdAuthOption", authInfo.AuthOption))

                sbSQL.Append("WHERE MLOC.LOC_L_CD = @LOC_L_CD ")
                params.Add(New SqlParameter($"@LOC_L_CD", CommonRfid.ToStringNvl(locLCd)))
                If (locMCd IsNot Nothing) Then
                    sbSQL.Append(" AND MLOC.LOC_M_CD = @LOC_M_CD ")
                    params.Add(New SqlParameter($"@LOC_M_CD ", CommonRfid.ToStringNvl(locMCd)))
                End If
                If (locSCd IsNot Nothing) Then
                    sbSQL.Append(" AND MLOC.LOC_S_CD = @LOC_S_CD ")
                    params.Add(New SqlParameter($"@LOC_S_CD ", CommonRfid.ToStringNvl(locSCd)))
                End If

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

            Catch ex As Exception
                'SQL ERR
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            'レコードが取得できた場合、OK
            Return tbl.Rows.Count() > 0

        End Function

#End Region


    End Class
End Namespace