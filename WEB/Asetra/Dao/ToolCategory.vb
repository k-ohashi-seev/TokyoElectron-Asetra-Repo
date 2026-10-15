Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：ローケーション関連ツール系 Data Access クラス
    ''' 作成日        ：2019/08/14      Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolCategory

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "大分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 大分類ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetCategoryLData() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    CAT_L_CD CAT_CD")
                'sbSQL.Append("  , CAT_NAME ")  '2019/09/12 upd nv iwamoto
                sbSQL.Append("  , CONCAT(CAT_L_CD, '" & CommonConst.CodeNameDelimiter & "', CAT_NAME) AS CAT_NAME ")
                sbSQL.Append("FROM  M_CATEGORY ")
                sbSQL.Append("WHERE  CAT_DIV = '" & CAT_DIV.CategoryL & "'")
                sbSQL.Append("ORDER BY CAT_L_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

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
#End Region

#Region "中分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 中分類ドロップダウンリストデータ取得
        ''' </summary>
        ''' <param name="selectLCode">大分類コード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetCategoryMData(ByVal selectLCode As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    CAT_M_CD CAT_CD")
                'sbSQL.Append("  , CAT_NAME ")  '2019/09/12 upd nv iwamoto
                sbSQL.Append("  , CONCAT(CAT_M_CD, '" & CommonConst.CodeNameDelimiter & "', CAT_NAME) AS CAT_NAME ")
                sbSQL.Append("FROM  M_CATEGORY ")
                sbSQL.Append("WHERE  CAT_DIV = '" & CAT_DIV.CategoryM & "' ")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("  AND CAT_L_CD = @CAT_L_CD ")
                params.Add(New SqlParameter($"@CAT_L_CD", CommonRfid.ToStringNvl(selectLCode)))
                'sbSQL.Append("  AND CAT_L_CD = '" & selectLCode & "' ")
                sbSQL.Append("ORDER BY CAT_M_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

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
#End Region

#Region "小分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' 小分類ドロップダウンリストデータ取得
        ''' </summary>
        ''' <param name="selectLCode">大分類コード</param>
        ''' <param name="selectMCode">中分類コード</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetCategorySData(ByVal selectLCode As String, ByVal selectMCode As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("    CAT_S_CD CAT_CD")
                'sbSQL.Append("  , CAT_NAME ")  '2019/09/12 upd nv iwamoto
                sbSQL.Append("  , CONCAT(CAT_S_CD, '" & CommonConst.CodeNameDelimiter & "', CAT_NAME) AS CAT_NAME ")
                sbSQL.Append("FROM M_CATEGORY ")
                sbSQL.Append("WHERE CAT_DIV = '" & CAT_DIV.CategoryS & "' ")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("  AND CAT_L_CD = @CAT_L_CD ")
                params.Add(New SqlParameter($"@CAT_L_CD", CommonRfid.ToStringNvl(selectLCode)))
                sbSQL.Append("  AND CAT_M_CD = @CAT_M_CD ")
                params.Add(New SqlParameter($"@CAT_M_CD", CommonRfid.ToStringNvl(selectMCode)))
                'sbSQL.Append("  AND CAT_L_CD = '" & selectLCode & "' ")
                'sbSQL.Append("  AND CAT_M_CD = '" & selectMCode & "' ")
                sbSQL.Append("ORDER BY CAT_S_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

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

#End Region

#Region "上位の大分類コード存在チェック"
        ''' <summary>
        ''' 上位の大分類コード存在チェック
        ''' </summary>
        ''' <param name="catLCd">カテゴリ大分類コード</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Shared Function IsExistCatLCd(catLCd As String) As Boolean
            '値が空の場合は半角スペースを設定
            If String.IsNullOrEmpty(catLCd) Then
                catLCd = " "
            End If

            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                'TODO:ExceptionをThrowするよう変更する
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_LOCATIONテーブルから取得するSQLを作成
                sbSQL.Append("SELECT CAT_L_CD ")
                sbSQL.Append("FROM M_CATEGORY ")
                sbSQL.Append("WHERE  CAT_DIV = '" & CAT_DIV.CategoryL & "'")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("  AND CAT_L_CD = @CAT_L_CD ")
                params.Add(New SqlParameter($"@CAT_L_CD", CommonRfid.ToStringNvl(catLCd)))
                'sbSQL.Append("  AND  CAT_L_CD = '" & catLCd & "'")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                If tbl.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

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

#Region "上位の中分類コード存在チェック"
        ''' <summary>
        ''' 上位の中分類コード存在チェック
        ''' </summary>
        ''' <param name="catLCd">カテゴリ大分類コード</param>
        ''' <param name="catMCd">カテゴリ中分類コード</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Shared Function IsExistCatMCd(catLCd As String, catMCd As String) As Boolean
            '値が空の場合は半角スペースを設定
            If String.IsNullOrEmpty(catMCd) Then
                catMCd = " "
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
                sbSQL.Append("SELECT CAT_M_CD ")
                sbSQL.Append("FROM M_CATEGORY ")
                sbSQL.Append("WHERE  CAT_DIV = '" & CAT_DIV.CategoryM & "'")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("  AND CAT_L_CD = @CAT_L_CD ")
                params.Add(New SqlParameter($"@CAT_L_CD", CommonRfid.ToStringNvl(catLCd)))
                sbSQL.Append("  AND CAT_M_CD = @CAT_M_CD ")
                params.Add(New SqlParameter($"@CAT_M_CD", CommonRfid.ToStringNvl(catMCd)))
                'sbSQL.Append("  AND  CAT_L_CD = '" & catLCd & "'")
                'sbSQL.Append("  AND  CAT_M_CD = '" & catMCd & "'")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                If tbl.Rows.Count = 0 Then
                    Return False
                Else
                    Return True
                End If

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

#Region "分類名取得"
        ''' <summary>
        ''' 分類名取得
        ''' </summary>
        ''' <param name="catDiv">分類区分</param>
        ''' <param name="catLCd">カテゴリ大分類コード</param>
        ''' <param name="catMCd">カテゴリ中分類コード</param>
        ''' <param name="catSCd">カテゴリ小分類コード</param>
        ''' <returns>分類名</returns>
        Public Shared Function GetCategoryName(catDiv As String, catLCd As String, catMCd As String, catSCd As String) As String
            '値が空の場合は半角スペースを設定
            If String.IsNullOrEmpty(catLCd) Then
                catLCd = " "
            End If
            If String.IsNullOrEmpty(catMCd) Then
                catMCd = " "
            End If
            If String.IsNullOrEmpty(catSCd) Then
                catSCd = " "
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
                sbSQL.Append("SELECT CAT_NAME ")
                sbSQL.Append("FROM M_CATEGORY ")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("WHERE  CAT_DIV = @CAT_DIV ")
                params.Add(New SqlParameter($"@CAT_DIV", CommonRfid.ToStringNvl(catDiv)))
                sbSQL.Append("  AND CAT_L_CD = @CAT_L_CD ")
                params.Add(New SqlParameter($"@CAT_L_CD", CommonRfid.ToStringNvl(catLCd)))
                sbSQL.Append("  AND CAT_M_CD = @CAT_M_CD ")
                params.Add(New SqlParameter($"@CAT_M_CD", CommonRfid.ToStringNvl(catMCd)))
                sbSQL.Append("  AND CAT_S_CD = @CAT_S_CD ")
                params.Add(New SqlParameter($"@CAT_S_CD", CommonRfid.ToStringNvl(catSCd)))
                'sbSQL.Append("WHERE  CAT_DIV = '" & catDiv & "'")
                'sbSQL.Append("  AND  CAT_L_CD = '" & catLCd & "'")
                'sbSQL.Append("  AND  CAT_M_CD = '" & catMCd & "'")
                'sbSQL.Append("  AND  CAT_S_CD = '" & catSCd & "'")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                If tbl.Rows.Count = 0 Then
                    Return String.Empty
                Else
                    Return tbl.Rows(0).Item("CAT_NAME").ToString
                End If

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

#Region "引数の各分類コードから分類区分を取得します"
        ''' <summary>
        ''' 引数の各分類コードから分類区分を取得します
        ''' </summary>
        ''' <param name="catLCd">カテゴリ大分類コード</param>
        ''' <param name="catMCd">カテゴリ中分類コード</param>
        ''' <param name="catSCd">カテゴリ小分類コード</param>
        ''' <returns></returns>
        Public Shared Function GetCatDiv(catLCd As String, catMCd As String, catSCd As String) As String
            Dim ret As String = String.Empty

            If (catLCd.Trim.Length <> 0 AndAlso catMCd.Trim.Length = 0 AndAlso catSCd.Trim.Length = 0) Then
                ret = CAT_DIV.CategoryL
            ElseIf (catLCd.Trim.Length <> 0 AndAlso catMCd.Trim.Length <> 0 AndAlso catSCd.Trim.Length = 0) Then
                ret = CAT_DIV.CategoryM
            ElseIf (catLCd.Trim.Length <> 0 AndAlso catMCd.Trim.Length <> 0 AndAlso catSCd.Trim.Length <> 0) Then
                ret = CAT_DIV.CategoryS
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
        Public Shared Function GetCategoryDataAll() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.Append("SELECT *")
                sbSQL.Append("FROM  M_CATEGORY ")
                sbSQL.Append("ORDER BY CAT_L_CD")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                tbl.DefaultView.Sort = "CAT_DIV ASC, CAT_L_CD ASC, CAT_M_CD ASC, CAT_S_CD ASC"      '2019/12/18 ADD Inoue


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
#End Region

#Region "分類使用チェック"
        ''' <summary>
        ''' 分類使用チェック(指定コードが、引数のもの以外のロケーションＭ内、資産Ｍに存在するか、否か)
        ''' </summary>
        ''' <param name="catLCd">カテゴリ大分類コード</param>
        ''' <param name="catMCd">カテゴリ中分類コード</param>
        ''' <param name="catSCd">カテゴリ小分類コード</param>
        ''' <param name="masterCheckSkip">マスタチェックスキップ</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Shared Function IsUseCat(ByVal catLCd As String, ByVal catMCd As String, ByVal catSCd As String, Optional ByVal masterCheckSkip As Boolean = False) As Boolean
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                If String.IsNullOrEmpty(catSCd.Trim) Then
                    '小区分以外の場合、ロケーションＭ内をチェックする

                    If Not masterCheckSkip Then                                 '2022/03/08 ファイル取込時のマスタチェックを廃止

                        'M_LOCATIONテーブルから取得するSQLを作成
                        sbSQL.AppendLine("SELECT DISTINCT CAT_L_CD ")
                        sbSQL.AppendLine("FROM M_CATEGORY ")
                        '--2020/03/05 nv miyaji update start
                        sbSQL.AppendLine("WHERE (CAT_L_CD <> @CAT_L_CD ")
                        sbSQL.AppendLine("   OR  CAT_M_CD <> @CAT_M_CD ")
                        sbSQL.AppendLine("   OR  CAT_S_CD <> @CAT_S_CD )")
                        params.Add(New SqlParameter($"@CAT_L_CD", CommonRfid.ToStringNvl(catLCd)))
                        params.Add(New SqlParameter($"@CAT_M_CD", CommonRfid.ToStringNvl(catMCd)))
                        params.Add(New SqlParameter($"@CAT_S_CD", CommonRfid.ToStringNvl(catSCd)))
                        'sbSQL.AppendLine("WHERE (CAT_L_CD <> '" & catLCd & "'")
                        'sbSQL.AppendLine("   OR  CAT_M_CD <> '" & catMCd & "'")
                        'sbSQL.AppendLine("   OR  CAT_S_CD <> '" & catSCd & "')")

                        sbSQL.AppendLine("AND   CAT_L_CD = @CAT_L_CD ")
                        'sbSQL.AppendLine("AND   CAT_L_CD = '" & catLCd & "'")
                        If Not String.IsNullOrEmpty(catMCd.Trim) Then
                            sbSQL.AppendLine("AND   CAT_M_CD = @CAT_M_CD ")
                            'sbSQL.AppendLine("AND   CAT_M_CD = '" & catMCd & "'")
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
                sbSQL.AppendLine("WHERE CAT_L_CD = @CAT_L_CD1 ")
                params.Add(New SqlParameter($"@CAT_L_CD1", CommonRfid.ToStringNvl(catLCd)))
                'sbSQL.AppendLine("WHERE CAT_L_CD = '" & catLCd & "'")
                If Not String.IsNullOrEmpty(catMCd.Trim) Then
                    sbSQL.AppendLine("AND   CAT_M_CD = @CAT_M_CD1 ")
                    params.Add(New SqlParameter($"@CAT_M_CD1", CommonRfid.ToStringNvl(catMCd)))
                    'sbSQL.AppendLine("AND   CAT_M_CD = '" & catMCd & "'")
                End If
                If Not String.IsNullOrEmpty(catSCd.Trim) Then
                    sbSQL.AppendLine("AND   CAT_S_CD = @CAT_S_CD1 ")
                    params.Add(New SqlParameter($"@CAT_S_CD1", CommonRfid.ToStringNvl(catSCd)))
                    'sbSQL.AppendLine("AND   CAT_S_CD = '" & catSCd & "'")
                End If
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                If tbl.Rows.Count > 0 Then
                    Return True
                End If

                '入庫予定データ使用チェック
                sbSQL.Clear()
                params = New List(Of SqlParameter)
                sbSQL.AppendLine("SELECT DISTINCT ASSET_NO ")
                sbSQL.AppendLine("FROM T_IN_ASSET ")
                sbSQL.AppendLine("WHERE CAT_L_CD = @CAT_L_CD1 ")
                params.Add(New SqlParameter($"@CAT_L_CD1", CommonRfid.ToStringNvl(catLCd)))
                If Not String.IsNullOrEmpty(catMCd.Trim) Then
                    sbSQL.AppendLine("AND   CAT_M_CD = @CAT_M_CD1 ")
                    params.Add(New SqlParameter($"@CAT_M_CD1", CommonRfid.ToStringNvl(catMCd)))
                End If
                If Not String.IsNullOrEmpty(catSCd.Trim) Then
                    sbSQL.AppendLine("AND   CAT_S_CD = @CAT_S_CD1 ")
                    params.Add(New SqlParameter($"@CAT_S_CD1", CommonRfid.ToStringNvl(catSCd)))
                End If
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                If tbl.Rows.Count > 0 Then
                    Return True
                End If

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

#Region "分類使用チェック(資産ID登録済データ)"
        ''' <summary>
        ''' 分類使用チェック(既に資産ID登録を行っている資産Mデータが存在するかどうか)
        ''' </summary>
        ''' <param name="catLCd">カテゴリ大分類コード</param>
        ''' <param name="catMCd">カテゴリ中分類コード</param>
        ''' <param name="catSCd">カテゴリ小分類コード</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Shared Function IsUseCatAsset(ByVal catLCd As String, ByVal catMCd As String, ByVal catSCd As String) As Boolean
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter) '2020/03/05 nv miyaji add
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try

                '資産Ｍ使用チェック
                sbSQL.Clear()
                sbSQL.AppendLine("SELECT DISTINCT ASSET_NO ")
                sbSQL.AppendLine("FROM M_ASSET ")
                '--2020/03/05 nv miyaji update start
                sbSQL.AppendLine("WHERE CAT_L_CD = @CAT_L_CD ")
                params.Add(New SqlParameter($"@CAT_L_CD", CommonRfid.ToStringNvl(catLCd)))
                'sbSQL.AppendLine("WHERE CAT_L_CD = '" & catLCd & "'")
                If Not String.IsNullOrEmpty(catMCd.Trim) Then
                    sbSQL.AppendLine("AND   CAT_M_CD = @CAT_M_CD ")
                    params.Add(New SqlParameter($"@CAT_M_CD", CommonRfid.ToStringNvl(catMCd)))
                    'sbSQL.AppendLine("AND   CAT_M_CD = '" & catMCd & "'")
                End If
                If Not String.IsNullOrEmpty(catSCd.Trim) Then
                    sbSQL.AppendLine("AND   CAT_S_CD = @CAT_S_CD ")
                    params.Add(New SqlParameter($"@CAT_S_CD", CommonRfid.ToStringNvl(catSCd)))
                    'sbSQL.AppendLine("AND   CAT_S_CD = '" & catSCd & "'")
                End If
                sbSQL.AppendLine("AND   ISNULL(ASSET_ID, '') <> ''")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                If tbl.Rows.Count > 0 Then
                    Return True
                End If

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

    End Class
End Namespace