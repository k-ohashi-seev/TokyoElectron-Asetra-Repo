Imports Asetra.Misc
Imports System.IO
Imports Asetra.Models
Imports System.Data.SqlClient

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：資産関連ツール系 Data Access クラス
    ''' 作成日        ：2019/08/24      Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolAsset

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "資産番号より資産ID取得"
        '2020/03/05 nv miyaji 使ってないのでコメントアウト
        '''' <summary>
        '''' 資産番号より資産ID取得
        '''' </summary>
        '''' <param name="sAssetNo">資産番号</param>
        '''' <returns>資産ID</returns>
        'Public Shared Function GetAssetId(ByVal sAssetNo As String) As String
        '    '値が空の場合は String.Empty を戻す
        '    If String.IsNullOrEmpty(sAssetNo) Then
        '        Return String.Empty
        '    End If

        '    Dim tbl As New DataTable

        '    Dim sbSQL As StringBuilder = New StringBuilder
        '    Dim clsDB As clsSV_DB = New clsSV_DB
        '    If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
        '        '接続失敗
        '        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
        '    End If
        '    Try
        '        'M_LOCATIONテーブルから取得するSQLを作成
        '        sbSQL.Append("SELECT ASSET_ID ")
        '        sbSQL.Append("FROM M_ASSET ")
        '        sbSQL.Append("WHERE  ASSET_NO = '" & sAssetNo.Trim & "'")

        '        tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

        '        If tbl.Rows.Count = 0 Then
        '            Return String.Empty
        '        Else
        '            Return tbl.Rows(0).Item("ASSET_ID").ToString
        '        End If

        '    Catch ex As Exception
        '        'SQL ERR
        '        logger.Error(ex.Message)
        '        logger.Error(sbSQL)
        '        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
        '    Finally
        '        'DB CLOSE
        '        clsDB.SVSubCloseConnection()
        '    End Try
        'End Function
#End Region

#Region "資産ID作成"
        ''' <summary>
        ''' 資産ID作成
        ''' </summary>
        ''' <param name="iAssetIdRegDiv">資産ID登録方法</param>
        ''' <param name="sAssetStr">資産番号 ただし、「入力・CSV取込時の資産IDをそのまま使用する」場合は、チェックする資産ID</param>
        ''' <param name="iEpcSize">EPC領域サイズ</param>
        ''' <param name="loginUser">ログイン情報</param>
        ''' <returns>資産ID エラー時、String.Empty</returns>
        Public Shared Function MakeAssetId(ByVal iAssetIdRegDiv As Integer,
                                          ByVal sAssetStr As String,
                                          ByVal iEpcSize As Integer,
                                          ByVal loginUser As RfidEntity.LoginUser) As String

            Select Case iAssetIdRegDiv
                Case Misc.AssetIDGenerationMethod.None
                    '生成しない(タグ紐付で行う)
                    Return String.Empty

                Case Misc.AssetIDGenerationMethod.AssetNo
                    '資産番号より生成

                    '2019/10/24 UPDATE
                    '16進変換
                    Dim aryByte As Byte() = Encoding.UTF8.GetBytes(sAssetStr)
                    sAssetStr = BitConverter.ToString(aryByte).Replace("-", String.Empty)

                    sAssetStr = sAssetStr.PadRight(iEpcSize / 4, "0"c)

                    Return sAssetStr

                Case Misc.AssetIDGenerationMethod.Auto
                    '自動採番で生成
                    Dim lSeqNo As Long       '採番する番号

                    '設定マスタの発番データ取得
                    Dim tbl As New DataTable

                    Dim sbSQL As StringBuilder = New StringBuilder
                    Dim clsDB As clsSV_DB = New clsSV_DB
                    If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                        '接続失敗
                        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
                    End If
                    Try
                        'M_LOCATIONテーブルから取得するSQLを作成
                        sbSQL.AppendLine("SELECT DATA1, DATA2, DATA3 ")
                        sbSQL.AppendLine("FROM M_SETTING ")
                        sbSQL.AppendLine("WHERE  KEY_DIV = '" & Misc.CommonSettingKey.AssetIdAutomatic.KeyDiv & "'")
                        sbSQL.AppendLine("AND    KEY1 = '" & Misc.CommonSettingKey.AssetIdAutomatic.Key1 & "'")
                        sbSQL.AppendLine("AND    KEY2 = '" & Misc.CommonSettingKey.AssetIdAutomatic.Key2 & "'")
                        sbSQL.AppendLine("AND    KEY3 = '" & Misc.CommonSettingKey.AssetIdAutomatic.Key3 & "'")
                        tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)

                        If tbl.Rows.Count = 0 Then
                            lSeqNo = 1
                        Else
                            Dim row As DataRow = tbl.Rows(0)
                            If Int(row.Item("DATA2")) = Now.Year Then
                                lSeqNo = CLng(row.Item("DATA3")) + 1
                            Else
                                lSeqNo = 1
                            End If
                        End If

                        '採番データ更新
                        Dim sYYYY As String = Now.Year.ToString

                        sbSQL.Clear()
                        sbSQL.AppendLine("MERGE INTO M_SETTING AS TARGET")
                        sbSQL.AppendLine("USING")
                        sbSQL.AppendLine("(")
                        sbSQL.AppendLine("  SELECT")
                        sbSQL.AppendLine("       " & clsDB.SVFncCreateSQLValueString(Misc.CommonSettingKey.AssetIdAutomatic.KeyDiv) & " AS KEY_DIV")
                        sbSQL.AppendLine("      ," & clsDB.SVFncCreateSQLValueString(Misc.CommonSettingKey.AssetIdAutomatic.Key1) & " AS KEY1")
                        sbSQL.AppendLine("      ," & clsDB.SVFncCreateSQLValueString(Misc.CommonSettingKey.AssetIdAutomatic.Key2) & " AS KEY2")
                        sbSQL.AppendLine("      ," & clsDB.SVFncCreateSQLValueString(Misc.CommonSettingKey.AssetIdAutomatic.Key3) & " AS KEY3")
                        sbSQL.AppendLine("      ," & clsDB.SVFncCreateSQLValueString(Misc.CommonConst.ASSET_SEQNO_ID) & " AS DATA1")
                        sbSQL.AppendLine("      ," & clsDB.SVFncCreateSQLValueString(sYYYY) & " AS DATA2")
                        sbSQL.AppendLine("      ," & clsDB.SVFncCreateSQLValueString(lSeqNo.ToString(New String("0"c, Misc.CommonConst.ASSET_SEQNO_KETA))) & " AS DATA3")
                        sbSQL.AppendLine("      ," & clsDB.SVFncCreateSQLValueString(clsSV_DB.SQL_GET_DATE2) & " AS UPD_DATETIME")
                        sbSQL.AppendLine("      ," & clsDB.SVFncCreateSQLValueString(loginUser.EmployeeCd) & " AS UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("      ," & clsDB.SVFncCreateSQLValueString(loginUser.EmployeeNm) & " AS UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("      ," & clsDB.SVFncCreateSQLValueString("Dao.MakeAssetId") & " AS UPD_TASK")
                        sbSQL.AppendLine(") VALT")
                        sbSQL.AppendLine("ON")
                        sbSQL.AppendLine("(")
                        sbSQL.AppendLine("      TARGET.KEY_DIV = VALT.KEY_DIV")
                        sbSQL.AppendLine("  AND TARGET.KEY1    = VALT.KEY1")
                        sbSQL.AppendLine("  AND TARGET.KEY2    = VALT.KEY2")
                        sbSQL.AppendLine("  AND TARGET.KEY3    = VALT.KEY3")
                        sbSQL.AppendLine(")")
                        sbSQL.AppendLine("WHEN MATCHED THEN")
                        sbSQL.AppendLine("  UPDATE SET")
                        sbSQL.AppendLine("       TARGET.DATA2 = VALT.DATA2")
                        sbSQL.AppendLine("      ,TARGET.DATA3 = VALT.DATA3")
                        sbSQL.AppendLine("      ,TARGET.UPD_DATETIME      = VALT.UPD_DATETIME")
                        sbSQL.AppendLine("      ,TARGET.UPD_EMPLOYEE_CD   = VALT.UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("      ,TARGET.UPD_EMPLOYEE_NAME = VALT.UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("      ,TARGET.UPD_TASK          = VALT.UPD_TASK")
                        sbSQL.AppendLine("WHEN NOT MATCHED THEN")
                        sbSQL.AppendLine("  INSERT")
                        sbSQL.AppendLine("  (")
                        sbSQL.AppendLine("       KEY_DIV")
                        sbSQL.AppendLine("      ,KEY1")
                        sbSQL.AppendLine("      ,KEY2")
                        sbSQL.AppendLine("      ,KEY3")
                        sbSQL.AppendLine("      ,DATA1")
                        sbSQL.AppendLine("      ,DATA2")
                        sbSQL.AppendLine("      ,DATA3")
                        sbSQL.AppendLine("      ,INS_DATETIME")
                        sbSQL.AppendLine("      ,INS_EMPLOYEE_CD")
                        sbSQL.AppendLine("      ,INS_EMPLOYEE_NAME")
                        sbSQL.AppendLine("      ,UPD_DATETIME")
                        sbSQL.AppendLine("      ,UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("      ,UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("      ,UPD_TASK")
                        sbSQL.AppendLine("  )")
                        sbSQL.AppendLine("  VALUES")
                        sbSQL.AppendLine("  (")
                        sbSQL.AppendLine("       VALT.KEY_DIV")
                        sbSQL.AppendLine("      ,VALT.KEY1")
                        sbSQL.AppendLine("      ,VALT.KEY2")
                        sbSQL.AppendLine("      ,VALT.KEY3")
                        sbSQL.AppendLine("      ,VALT.DATA1")
                        sbSQL.AppendLine("      ,VALT.DATA2")
                        sbSQL.AppendLine("      ,VALT.DATA3")
                        sbSQL.AppendLine("      ,VALT.UPD_DATETIME")
                        sbSQL.AppendLine("      ,VALT.UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("      ,VALT.UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("      ,VALT.UPD_DATETIME")
                        sbSQL.AppendLine("      ,VALT.UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("      ,VALT.UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("      ,VALT.UPD_TASK")
                        sbSQL.AppendLine("  );")
                        Dim ret As Integer = clsDB.SVFncExecuteNonQuery(sbSQL.ToString)

                        Dim sRetAsset As String

                        sRetAsset = Misc.CommonConst.ASSET_SEQNO_ID & sYYYY & lSeqNo.ToString(New String("0"c, Misc.CommonConst.ASSET_SEQNO_KETA))

                        '2019/10/24 UPDATE
                        '16進変換
                        Dim aryByte As Byte() = Encoding.UTF8.GetBytes(sRetAsset)
                        sRetAsset = BitConverter.ToString(aryByte).Replace("-", String.Empty)

                        sRetAsset = sRetAsset.PadRight(iEpcSize / 4, "0"c)

                        Return sRetAsset

                    Catch ex As Exception
                        logger.Error(ex.Message)
                        logger.Error(sbSQL)
                        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                    Finally
                        'DB CLOSE
                        clsDB.SVSubCloseConnection()
                    End Try

                Case Misc.AssetIDGenerationMethod.Inp
                    '入力・CSV取込時の資産IDをそのまま使用する

                    '呼び出し元で行なう
                    ''16進文字列チェック
                    'For Each Chr As Char In sAssetStr
                    '    If Not Uri.IsHexDigit(Chr) Then
                    '        Return String.Empty
                    '    End If
                    'Next

                    ''2019/09/30 nv inoue　upd start
                    ''16進文字列に"30"を付与する場合
                    ''Dim sAddZeroStr As String = String.Empty
                    ''If sAssetStr.Length < (iEpcSize / 4) Then
                    ''    Dim iLen As Integer = ((iEpcSize / 4) - sAssetStr.Length) \ 2
                    ''    Dim aryByte As Byte() = Encoding.UTF8.GetBytes(New String("0"c, iLen))
                    ''    sAddZeroStr = BitConverter.ToString(aryByte).Replace("-", String.Empty)
                    ''End If

                    ''通常文字列に"0"を付与する場合　
                    'sAssetStr = sAssetStr.PadRight(iEpcSize / 8, "0"c)
                    ''2019/09/30 nv inoue　upd end

                    '2020/02/10 nv miyaji コメントアウト start 0埋めしないようにする
                    ''2019/10/24 UPDATE
                    'sAssetStr = sAssetStr.PadRight(iEpcSize / 4, "0"c)
                    '2020/02/10 nv miyaji コメントアウト end

                    Return sAssetStr
                Case Else
                    Return String.Empty

            End Select

        End Function
#End Region

#Region "資産ID 存在チェック"
        ''' <summary>
        ''' 資産IDが他の資産で使用されているかどうかを返す
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <param name="sAssetId">資産ID</param>
        ''' <returns>True:使用されている  False:使用されていない</returns>
        Public Shared Function IsExistsAssetId(ByVal sAssetNo As String, ByVal sAssetId As String) As Boolean
            '値が空の場合は String.Empty を戻す
            If String.IsNullOrEmpty(sAssetNo.Trim) Or String.IsNullOrEmpty(sAssetId.Trim) Then
                Return False
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
                'M_ASSETテーブルから取得するSQLを作成
                sbSQL.Append("SELECT Distinct ASSET_NO ")
                sbSQL.Append("FROM M_ASSET ")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("WHERE  ASSET_NO <> @ASSET_NO ")
                params.Add(New SqlParameter($"@ASSET_NO", CommonRfid.ToStringNvl(sAssetNo.Trim)))
                sbSQL.Append("AND    ASSET_ID = @ASSET_ID ")
                params.Add(New SqlParameter($"@ASSET_ID", CommonRfid.ToStringNvl(sAssetId.Trim)))
                'sbSQL.Append("WHERE  ASSET_NO <> '" & sAssetNo.Trim & "'")
                'sbSQL.Append("AND    ASSET_ID = '" & sAssetId.Trim & "'")

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

#Region "資産番号、サブ資産番号 存在チェック"
        ''' <summary>
        ''' 資産番号、サブ資産番号が他の資産で使用されているかどうかを返す
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <param name="sSubAssetNo">サブ資産番号</param>
        ''' <returns>True:使用されている  False:使用されていない</returns>
        Public Shared Function IsExistsSubAssetNo(ByVal sAssetNo As String, ByVal sSubAssetNo As String) As Boolean

            If Not String.IsNullOrEmpty(sSubAssetNo) Then
                If sAssetNo = sSubAssetNo Then
                    '資産番号とサブ資産番号が同じ場合はエラー
                    Return True
                End If
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
                'M_ASSETテーブルから取得するSQLを作成
                sbSQL.Append("SELECT Distinct ASSET_NO ")
                sbSQL.Append("FROM M_ASSET ")
                sbSQL.Append("WHERE  ASSET_NO <> @ASSET_NO ")
                params.Add(New SqlParameter($"@ASSET_NO", CommonRfid.ToStringNvl(sAssetNo.Trim)))
                If Not String.IsNullOrEmpty(sSubAssetNo) Then
                    'サブ資産番号がある場合
                    sbSQL.Append("AND (")
                    sbSQL.Append(" ASSET_NO = @SUB_ASSET_NO ")              '資産番号が引数.サブ資産番号と同じ
                    sbSQL.Append(" OR SUB_ASSET_NO = @SUB_ASSET_NO ")       'サブ資産番号が引数.サブ資産番号と同じ
                    sbSQL.Append(" OR SUB_ASSET_NO = @ASSET_NO ")           'サブ資産番号が引数.資産番号と同じ
                    sbSQL.Append(")")
                    params.Add(New SqlParameter($"@SUB_ASSET_NO", CommonRfid.ToStringNvl(sSubAssetNo.Trim)))
                Else
                    'サブ資産番号がない場合
                    sbSQL.Append("AND SUB_ASSET_NO = @ASSET_NO ")           'サブ資産番号が引数.資産番号と同じ
                End If

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

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

#Region "資産番号の画像存在チェック"
        ''' <summary>
        ''' 資産番号と画像ファイル名より、その画像ファイルが他の資産で使用されているかどうかを返す
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <param name="sImgFileNm">画像ファイル名</param>
        ''' <returns>True:使用されている  False:使用されていない</returns>
        Public Shared Function IsUseImg(ByVal sAssetNo As String, ByVal sImgFileNm As String) As Boolean
            '値が空の場合は String.Empty を戻す
            If String.IsNullOrEmpty(sAssetNo.Trim) Or String.IsNullOrEmpty(sImgFileNm.Trim) Then
                Return False
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
                'M_ASSETテーブルから取得するSQLを作成
                sbSQL.Append("SELECT Distinct ASSET_NO ")
                sbSQL.Append("FROM M_ASSET ")
                '--2020/03/05 nv miyaji update start
                sbSQL.Append("WHERE  ASSET_NO <> @ASSET_NO ")
                params.Add(New SqlParameter($"@ASSET_NO", CommonRfid.ToStringNvl(sAssetNo.Trim)))
                sbSQL.Append("AND    ASSET_PIC_FILENAME = @ASSET_PIC_FILENAME ")
                params.Add(New SqlParameter($"@ASSET_PIC_FILENAME", CommonRfid.ToStringNvl(sImgFileNm.Trim)))
                'sbSQL.Append("WHERE  ASSET_NO <> '" & sAssetNo.Trim & "'")
                'sbSQL.Append("AND    ASSET_PIC_FILENAME = '" & sImgFileNm.Trim & "'")

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

#Region "引数に該当する資産マスタデータを取得します"
        ''' <summary>
        ''' 引数に該当する資産マスタデータを取得します
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <returns>資産マスタデータ</returns>
        Public Shared Function GetMstAsset(sAssetNo As String) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As New List(Of SqlParameter)


            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_ASSETテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  ASSET.* ")
                sbSQL.AppendLine("  , CAT_L.CAT_NAME AS CAT_L_NAME ")
                sbSQL.AppendLine("  , CAT_M.CAT_NAME AS CAT_M_NAME ")
                sbSQL.AppendLine("  , CAT_S.CAT_NAME AS CAT_S_NAME ")
                '2021/10/06 nv miyaji add start
                sbSQL.AppendLine("  , LOCL.LOC_NAME AS MNG_LOC_L_NAME ")
                '2021/10/06 nv miyaji add end
                sbSQL.AppendLine("FROM  M_ASSET ASSET ")
                sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_L ")
                sbSQL.AppendLine("  ON  CAT_L.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
                sbSQL.AppendLine("  AND CAT_L.CAT_L_CD = ASSET.CAT_L_CD ")
                sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_M ")
                sbSQL.AppendLine("  ON  CAT_M.CAT_DIV = '" + CAT_DIV.CategoryM + "'")
                sbSQL.AppendLine("  AND CAT_M.CAT_L_CD = ASSET.CAT_L_CD ")
                sbSQL.AppendLine("  AND CAT_M.CAT_M_CD = ASSET.CAT_M_CD ")
                sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_S ")
                sbSQL.AppendLine("  ON  CAT_S.CAT_DIV = '" + CAT_DIV.CategoryS + "'")
                sbSQL.AppendLine("  AND CAT_S.CAT_L_CD = ASSET.CAT_L_CD ")
                sbSQL.AppendLine("  AND CAT_S.CAT_M_CD = ASSET.CAT_M_CD ")
                sbSQL.AppendLine("  AND CAT_S.CAT_S_CD = ASSET.CAT_S_CD ")
                '2021/10/06 nv miyaji add start
                sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
                sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
                sbSQL.AppendLine("AND ASSET.MNG_LOC_L_CD = LOCL.LOC_L_CD")
                '2021/10/06 nv miyaji add end
                sbSQL.AppendLine("WHERE ASSET.ASSET_NO = @ASSET_NO")
                sbSQL.AppendLine("ORDER BY ASSET.ASSET_NO")

                params.Add(New SqlParameter($"@ASSET_NO", sAssetNo.Trim))

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
#End Region

#Region "複数の資産番号に該当する資産マスタデータを取得します"
        ''' <summary>
        ''' 複数の資産番号に該当する資産マスタデータを取得します
        ''' </summary>
        ''' <param name="lstAssetNo">資産番号を格納したリスト</param>
        ''' <returns>資産マスタデータ</returns>
        Public Shared Function GetMstAssetByList(lstAssetNo As List(Of String)) As DataTable
            Dim tbl As New DataTable

            If lstAssetNo.Count = 0 Then
                Return tbl
            End If

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As New List(Of SqlParameter)

            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_ASSETテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  ASSET.* ")
                sbSQL.AppendLine("  , CAT_L.CAT_NAME AS CAT_L_NAME ")
                sbSQL.AppendLine("  , CAT_M.CAT_NAME AS CAT_M_NAME ")
                sbSQL.AppendLine("  , CAT_S.CAT_NAME AS CAT_S_NAME ")
                sbSQL.AppendLine("  , LOCL.LOC_NAME AS MNG_LOC_L_NAME ")
                sbSQL.AppendLine("FROM  M_ASSET ASSET ")
                sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_L ")
                sbSQL.AppendLine("  ON  CAT_L.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
                sbSQL.AppendLine("  AND CAT_L.CAT_L_CD = ASSET.CAT_L_CD ")
                sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_M ")
                sbSQL.AppendLine("  ON  CAT_M.CAT_DIV = '" + CAT_DIV.CategoryM + "'")
                sbSQL.AppendLine("  AND CAT_M.CAT_L_CD = ASSET.CAT_L_CD ")
                sbSQL.AppendLine("  AND CAT_M.CAT_M_CD = ASSET.CAT_M_CD ")
                sbSQL.AppendLine("LEFT JOIN M_CATEGORY CAT_S ")
                sbSQL.AppendLine("  ON  CAT_S.CAT_DIV = '" + CAT_DIV.CategoryS + "'")
                sbSQL.AppendLine("  AND CAT_S.CAT_L_CD = ASSET.CAT_L_CD ")
                sbSQL.AppendLine("  AND CAT_S.CAT_M_CD = ASSET.CAT_M_CD ")
                sbSQL.AppendLine("  AND CAT_S.CAT_S_CD = ASSET.CAT_S_CD ")
                sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
                sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
                sbSQL.AppendLine("AND ASSET.MNG_LOC_L_CD = LOCL.LOC_L_CD")
                Dim sbSQLAssetNo As StringBuilder = New StringBuilder
                For idx As Integer = 0 To lstAssetNo.Count - 1
                    If sbSQLAssetNo.Length <> 0 Then
                        sbSQLAssetNo.Append(", ")
                    End If
                    sbSQLAssetNo.Append("@ASSET_NO" & idx)
                    params.Add(New SqlParameter($"@ASSET_NO" & idx, lstAssetNo(idx)))
                Next
                sbSQL.AppendLine("WHERE ASSET.ASSET_NO IN (" & sbSQLAssetNo.ToString & ")")
                sbSQL.AppendLine("ORDER BY ASSET.ASSET_NO")

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
#End Region

#Region "資産状態 更新(タグ発行時)"
        ''' <summary>
        ''' 指定資産番号のデータを資産状態を更新する(タグ発行時)
        ''' </summary>
        ''' <param name="AssetListData">資産情報モデルリスト</param>
        ''' <param name="loginUser">ログイン情報</param>
        ''' <returns>True:せい  False:使用されていない</returns>
        Public Shared Function UpdateAssetStat(ByVal AssetListData As List(Of AssetListLabelModel), ByVal loginUser As RfidEntity.LoginUser) As String

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            clsDB.SVFncStartTransaction()

            Try
                Dim params As List(Of SqlParameter)
                Dim ret As Integer

                For Each item As AssetListLabelModel In AssetListData
                    params = New List(Of SqlParameter)
                    sbSQL.Clear()

                    If IsNothing(item.ASSET_STATUS) Then
                        item.ASSET_STATUS = Misc.ASSET_STATUS.Preparation
                    End If
                    If IsNothing(item.TAG_LINK_STATUS) Then
                        item.TAG_LINK_STATUS = Misc.TagLinkStatus.Mi
                    End If
                    If item.ASSET_STATUS.ToString = Misc.ASSET_STATUS.Preparation Then
                        sbSQL.AppendLine("UPDATE M_ASSET SET ")
                        sbSQL.AppendLine("   ASSET_STATUS  =  " & Misc.ASSET_STATUS.Storage)
                        sbSQL.AppendLine("  ,TAG_LINK_STATUS  =  " & Misc.TagLinkStatus.Sumi)

                        '--2020/02/12 nv inoue add start  棚卸開始チェック
                        sbSQL.AppendLine("  ,PHYINV_STATUS = CASE WHEN (" & Dao.ToolCommon.GetSqlStrChkInvt() & ") = 0 Then PHYINV_STATUS ELSE '" & Misc.PHYINV_STATUS.NoSpot & "' End")
                        '--2020/02/12 nv inoue add end

                        sbSQL.AppendLine("  ,UPD_DATETIME = " & clsDB.SVFncCreateSQLValueString(clsSV_DB.SQL_GET_DATE2))
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", loginUser.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", loginUser.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", "Dao.GetAsset"))

                        sbSQL.AppendLine("WHERE  ASSET_NO = @ASSET_NO")
                        params.Add(New SqlParameter($"@ASSET_NO", item.ASSET_NO))
                        ret = clsDB.SVFncExecuteNonQuery(sbSQL.ToString, params)


                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

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
                        sbSQL.AppendLine("  ,REMARK")   '2019/11/19 nv miyaji add
                        sbSQL.AppendLine("  ,LEASE_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,LEASE_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,LEASE_DUE_DT")
                        sbSQL.AppendLine("  ,MNG_LOC_L_CD")
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
                        sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.T1 & "'")
                        sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_CD")
                        sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_NAME")
                        sbSQL.AppendLine("  ,ASSET.REMARK")   '2019/11/19 nv miyaji add
                        sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,ASSET.LEASE_DUE_DT")
                        sbSQL.AppendLine("  ,ASSET.MNG_LOC_L_CD")
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
                        sbSQL.AppendLine("  ,ASSET.CAT_L_CD")
                        sbSQL.AppendLine("  ,CATL.CAT_NAME ")
                        sbSQL.AppendLine("  ,ASSET.SUB_ASSET_NO")
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

                        sbSQL.AppendLine("WHERE  ASSET.ASSET_NO = @ASSET_NO")

                        params.Add(New SqlParameter($"@ASSET_NO", item.ASSET_NO))
                        ret = clsDB.SVFncExecuteNonQuery(sbSQL.ToString, params)

                    ElseIf item.ASSET_STATUS.ToString = Misc.ASSET_STATUS.Storage And item.TAG_LINK_STATUS.ToString <> Misc.TagLinkStatus.Sumi Then
                        sbSQL.AppendLine("UPDATE M_ASSET SET ")
                        sbSQL.AppendLine("   TAG_LINK_STATUS  =  " & Misc.TagLinkStatus.Sumi)

                        sbSQL.AppendLine("  ,UPD_DATETIME = " & clsDB.SVFncCreateSQLValueString(clsSV_DB.SQL_GET_DATE2))
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", loginUser.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", loginUser.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", "Dao.GetAsset"))


                        sbSQL.AppendLine("WHERE  ASSET_NO = @ASSET_NO")
                        params.Add(New SqlParameter($"@ASSET_NO", item.ASSET_NO))
                        ret = clsDB.SVFncExecuteNonQuery(sbSQL.ToString, params)

                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

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
                        sbSQL.AppendLine("  ,REMARK")   '2019/11/19 nv miyaji add
                        sbSQL.AppendLine("  ,LEASE_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,LEASE_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,LEASE_DUE_DT")
                        sbSQL.AppendLine("  ,MNG_LOC_L_CD")
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
                        sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.T1 & "'")
                        sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_CD")
                        sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_NAME")
                        sbSQL.AppendLine("  ,ASSET.REMARK")   '2019/11/19 nv miyaji add
                        sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,ASSET.LEASE_DUE_DT")
                        sbSQL.AppendLine("  ,ASSET.MNG_LOC_L_CD")
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
                        sbSQL.AppendLine("  ,ASSET.CAT_L_CD")
                        sbSQL.AppendLine("  ,CATL.CAT_NAME ")
                        sbSQL.AppendLine("  ,ASSET.SUB_ASSET_NO")
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

                        sbSQL.AppendLine("WHERE  ASSET.ASSET_NO = @ASSET_NO")

                        params.Add(New SqlParameter($"@ASSET_NO", item.ASSET_NO))
                        ret = clsDB.SVFncExecuteNonQuery(sbSQL.ToString, params)

                    End If

                Next

                clsDB.SVFncFinishTransaction(True)

                Return True

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                clsDB.SVFncFinishTransaction(False)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

#Region "16進文字列を通常文字列に戻す（ASCII化）"
        ''' <summary>
        ''' 16進文字列を通常文字列に戻す（ASCII化）
        ''' </summary>
        ''' <param name="sHex">16進文字列</param>
        ''' <returns>資産ID</returns>
        Public Shared Function HexToStr(ByVal sHex As String) As String
            '値が空の場合は String.Empty を戻す
            If String.IsNullOrEmpty(sHex) Then
                Return String.Empty
            End If

            Dim aryStr((sHex.Length \ 2) - 1) As String
            Dim jj As Integer = 0
            For ii As Integer = 0 To sHex.Length - 2 Step 2
                aryStr(jj) = sHex.Substring(ii, 2)
                jj += 1
            Next

            Dim aryByte((sHex.Length \ 2) - 1) As Byte
            For ii As Integer = 0 To aryStr.Length - 1
                aryByte(ii) = Convert.ToByte(aryStr(ii), 16)
            Next

            'Dim retStr As String = Encoding.ASCII.GetString(aryByte)
            Dim retStr As String = Encoding.UTF8.GetString(aryByte)

            Return retStr

        End Function
#End Region

#Region "未タグ紐づけ資産 存在チェック"
        ''' <summary>
        ''' 未タグ紐づけ資産があるかどうかを返す
        ''' </summary>
        ''' <param name="sWhereSQL">SQLWhere文</param>
        ''' <param name="sParam">SQLパラメーター</param>
        ''' <returns>True:使用されている  False:使用されていない</returns>
        Public Shared Function IsExistsMitagHimoduke(ByVal sWhereSQL As String, ByVal sParam As List(Of SqlParameter)) As Boolean
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_ASSETテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT TOP 1 ASSET.ASSET_NO ")
                sbSQL.AppendLine("FROM M_ASSET ASSET")
                '2020/02/28 nv miyaji add start アラートにチェックがある時落ちたので修正
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
                '2020/02/28 nv miyaji add end
                sbSQL.AppendLine("WHERE")
                sbSQL.AppendLine("  1 = 1 ")
                sbSQL.AppendLine(sWhereSQL)
                sbSQL.AppendLine("  And ASSET.TAG_LINK_STATUS = '" & TagLinkStatus.Mi & "'")
                sbSQL.AppendLine("  And ASSET.ASSET_STATUS <> '" & ASSET_STATUS.Removed & "'")

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, sParam)

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

#Region "データ取得(全項目)"
        ''' <summary>
        ''' データ取得(全項目)
        ''' </summary>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetAssetDataAll() As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_CATEGORYテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT *")
                sbSQL.AppendLine("FROM  M_ASSET ")
                sbSQL.AppendLine("ORDER BY ASSET_NO")


                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                tbl.PrimaryKey = New DataColumn() {tbl.Columns("ASSET_NO")}

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

#Region "連番テーブルからの連番取得＆更新"
        ''' <summary>
        ''' 連番テーブルからの連番取得＆更新
        ''' </summary>
        ''' <param name="iAssetIdRegDiv">資産ID登録方法</param>
        ''' <param name="sComKey">EPC事業所キー</param>
        ''' <param name="sEpcAssetType">EPC資産タイプ</param>
        ''' <param name="loginUser">ログイン情報</param>
        ''' <returns>True:せい  False:使用されていない</returns>
        Public Shared Function GetSerialNo(ByVal iAssetIdRegDiv As Integer, ByVal sComKey As String, ByVal sEpcAssetType As String, ByVal loginUser As RfidEntity.LoginUser) As String

            '自動採番で生成
            Dim lSeqNo As Long       '採番する番号

            '設定マスタの発番データ取得
            Dim tbl As New DataTable


            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            clsDB.SVFncStartTransaction()

            Try
                'T_EPC_SERIALNOテーブルから取得するSQLを作成
                sbSQL.AppendLine("SELECT EPC_SERIALNO ")
                sbSQL.AppendLine("FROM T_EPC_SERIALNO ")
                '--2020/03/05 nv miyaji update start
                sbSQL.AppendLine("WHERE  ASSET_ID_REG_DIV = @ASSET_ID_REG_DIV ")
                params.Add(New SqlParameter($"@ASSET_ID_REG_DIV", iAssetIdRegDiv))
                sbSQL.AppendLine("AND  EPC_COMPANY_KEY = @EPC_COMPANY_KEY ")
                params.Add(New SqlParameter($"@EPC_COMPANY_KEY", CommonRfid.ToStringNvl(sComKey)))
                sbSQL.AppendLine("AND  EPC_ASSET_TYPE = @EPC_ASSET_TYPE ")
                params.Add(New SqlParameter($"@EPC_ASSET_TYPE", CommonRfid.ToStringNvl(sEpcAssetType)))
                'sbSQL.AppendLine("WHERE  ASSET_ID_REG_DIV = " & iAssetIdRegDiv)
                'sbSQL.AppendLine("AND    EPC_COMPANY_KEY = '" & sComKey & "'")
                'sbSQL.AppendLine("AND    EPC_ASSET_TYPE = '" & sEpcAssetType & "'")
                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)
                'tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                '--2020/03/05 nv miyaji update end

                sbSQL.Clear()
                params = New List(Of SqlParameter)  '2020/03/05 nv miyaji add
                If tbl.Rows.Count = 0 Then
                    lSeqNo = 1

                    sbSQL.AppendLine("INSERT INTO T_EPC_SERIALNO")
                    sbSQL.AppendLine("(")
                    sbSQL.AppendLine("   ASSET_ID_REG_DIV")
                    sbSQL.AppendLine("  ,EPC_COMPANY_KEY")
                    sbSQL.AppendLine("  ,EPC_ASSET_TYPE")
                    sbSQL.AppendLine("  ,EPC_SERIALNO")
                    sbSQL.AppendLine("  ,INS_DATETIME")
                    sbSQL.AppendLine("  ,INS_EMPLOYEE_CD")
                    sbSQL.AppendLine("  ,INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("  ,UPD_DATETIME")
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("  ,UPD_TASK")
                    sbSQL.AppendLine(") VALUES (")
                    sbSQL.AppendLine(" @AssetIdRegDiv")
                    sbSQL.AppendLine(",@ComKey")
                    sbSQL.AppendLine(",@EpcAssetType")
                    sbSQL.AppendLine(",@SerialNo")
                    sbSQL.AppendLine("," & clsDB.SVFncCreateSQLValueString(clsSV_DB.SQL_GET_DATE2))
                    sbSQL.AppendLine(",@INS_EMPLOYEE_CD")
                    sbSQL.AppendLine(",@INS_EMPLOYEE_NAME")
                    sbSQL.AppendLine("," & clsDB.SVFncCreateSQLValueString(clsSV_DB.SQL_GET_DATE2))
                    sbSQL.AppendLine(",@UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine(",@UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine(",@UPD_TASK")
                    sbSQL.AppendLine(")")

                    params.Add(New SqlParameter($"@AssetIdRegDiv", iAssetIdRegDiv))
                    params.Add(New SqlParameter($"@ComKey", sComKey))
                    params.Add(New SqlParameter($"@EpcAssetType", sEpcAssetType))
                    params.Add(New SqlParameter($"@SerialNo", lSeqNo))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", loginUser.EmployeeCd))
                    params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", loginUser.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", loginUser.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", loginUser.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", "Dao.GetSerialNo"))
                Else
                    Dim row As DataRow = tbl.Rows(0)
                    lSeqNo = CLng(row.Item("EPC_SERIALNO")) + 1

                    sbSQL.AppendLine("UPDATE T_EPC_SERIALNO SET ")
                    sbSQL.AppendLine("   EPC_SERIALNO  =  " & lSeqNo)
                    sbSQL.AppendLine("  ,UPD_DATETIME = " & clsDB.SVFncCreateSQLValueString(clsSV_DB.SQL_GET_DATE2))
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", loginUser.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", loginUser.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", "Dao.GetSerialNo"))

                    sbSQL.AppendLine("WHERE  ASSET_ID_REG_DIV = @AssetIdRegDiv")
                    params.Add(New SqlParameter($"@AssetIdRegDiv", iAssetIdRegDiv))
                    sbSQL.AppendLine("AND  EPC_COMPANY_KEY = @ComKey")
                    params.Add(New SqlParameter($"@ComKey", CommonRfid.ToStringNvl(sComKey)))
                    sbSQL.AppendLine("AND  EPC_ASSET_TYPE = @EpcAssetType")
                    params.Add(New SqlParameter($"@EpcAssetType", CommonRfid.ToStringNvl(sEpcAssetType)))
                End If

                clsDB.SVFncExecuteNonQuery(sbSQL.ToString, params)

                clsDB.SVFncFinishTransaction(True)

                Return lSeqNo.ToString

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                clsDB.SVFncFinishTransaction(False)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

'2023/02/08 Kawamoto ADD START
#Region "資産マスタ(ロケ大区分、または管理ロケ大区分が一致するデータの登録件数)を取得する"
        ''' <summary>
        ''' 資産マスタ(ロケ大区分、または管理ロケ大区分が一致するデータの登録件数)を取得する
        ''' <param name="AssetNo">資産番号</param>
        ''' <param name="locLCd">ロケ大区分</param>
        ''' <returns>件数</returns>
        ''' </summary>
        Public Shared Function GetAssetLocLCodeTotal(ByVal AssetNo As String, 
                                                     ByVal locLCd As String) As Integer
            Dim total As Integer = 0

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_ASSETテーブルから取得するSQLを作成
                sbSQL.Append("SELECT ")
                sbSQL.Append("  COUNT(*) AS CNT ")
                sbSQL.Append("FROM ")
                sbSQL.Append("  M_ASSET ")
                sbSQL.Append("WHERE ")
                sbSQL.Append("  ASSET_STATUS <> '" & ASSET_STATUS.Removed & "'")
                sbSQL.Append(" AND ")
                sbSQL.Append("  ASSET_NO <> '" & AssetNo & "'")
                sbSQL.Append(" AND ")
                sbSQL.Append("( ")
                sbSQL.Append("  MNG_LOC_L_CD = '" & LocLCd & "'")
                sbSQL.Append(" OR ")
                sbSQL.Append("  LOC_L_CD = '" & locLCd & "'")
                sbSQL.Append(") ")

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)
                total = Integer.Parse(tbl.Rows(0).Item("CNT"))
                Return total

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return total
        End Function
#End Region

#Region "資産マスタ(ロケ大区分、または管理ロケ大区分毎の登録件数)を取得する"
        ''' <summary>
        ''' 資産マスタ(ロケ大区分、または管理ロケ大区分毎の登録件数)を取得する
        ''' <returns>件数</returns>
        ''' </summary>
        Public Shared Function GetAssetLocLCodeTotal() As DataTable
            Dim total As Integer = 0

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                'M_ASSETテーブルから取得するSQLを作成
                sbSQL.Append("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ")
                sbSQL.Append("SELECT ")
                sbSQL.Append("  MNG_LOC_L_CD AS LOC_L_CD")
                sbSQL.Append(" ,COUNT(*) AS CNT ")
                sbSQL.Append("FROM ")
                sbSQL.Append("  M_ASSET ")
                sbSQL.Append("WHERE ")
                sbSQL.Append("  ASSET_STATUS <> '" & ASSET_STATUS.Removed & "' ")
                sbSQL.Append("GROUP BY ")
                sbSQL.Append("  MNG_LOC_L_CD ")
                sbSQL.Append("UNION ")
                sbSQL.Append("SELECT ")
                sbSQL.Append("  LOC_L_CD")
                sbSQL.Append(" ,COUNT(*) AS CNT ")
                sbSQL.Append("FROM ")
                sbSQL.Append("  M_ASSET ")
                sbSQL.Append("WHERE ")
                sbSQL.Append("  ASSET_STATUS <> '" & ASSET_STATUS.Removed & "' ")
                sbSQL.Append("GROUP BY ")
                sbSQL.Append("  LOC_L_CD ")

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString)
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
        '2023/02/08 Kawamoto ADD END

#Region "指定ロケの資産を削除"
        ''' <summary>
        ''' 指定ロケの資産を削除＆更新
        ''' </summary>
        ''' <param name="locLCd">ロケ大区分コード</param>
        ''' <returns>True:成功  False:失敗</returns>
        Public Shared Function DeleteAssetByLocLCd(ByVal locLCd As String) As Boolean

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If

            clsDB.SVFncStartTransaction()

            Try
                '資産処理履歴登録
                sbSQL.Clear()
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
                sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.CD & "'")
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

                sbSQL.AppendLine(" WHERE MNG_LOC_L_CD = '" & locLCd & "'")
                clsDB.SVFncExecuteNonQuery(sbSQL.ToString)

                '資産マスタ削除
                sbSQL.Clear()
                sbSQL.AppendLine("DELETE FROM M_ASSET")
                sbSQL.AppendLine(" WHERE MNG_LOC_L_CD = '" & locLCd & "'")
                clsDB.SVFncExecuteNonQuery(sbSQL.ToString)

                clsDB.SVFncFinishTransaction(True)

                Return True

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                clsDB.SVFncFinishTransaction(False)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region
    End Class
End Namespace