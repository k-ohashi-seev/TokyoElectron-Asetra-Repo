Imports System.Data.SqlClient
Imports System.Net
Imports System.Web.Http
Imports System.Web.Http.Cors

Namespace Controllers
    ''' <summary>
    ''' マスタデータを返すコントローラー
    ''' </summary>
    Public Class HtGetMasterDataCmdController
        Inherits ApiController

#Region "固定値定義"
        ''' <summary>
        ''' 処理名
        ''' </summary>
        Private TASK_NAME As String = Me.GetType.Name
#End Region

#Region "変数定義"

        ''' <summary>
        ''' ログ出力クラス
        ''' </summary>
        Private logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        Private Class HubLoc
            Public Property L As String
            Public Property M As String
        End Class
#End Region

#Region "Public メソッド"
        ''' <summary>
        ''' マスタデータ送信処理
        ''' </summary>
        ''' <returns>戻り値クラス</returns>
        ''' <remarks>api/HtAssetRecv呼び出し</remarks>
        <HttpPost>
        <EnableCors("*", "*", "*")>
        Public Function Post(<FromBody> ByVal value As HtGetMasterDataCmdReqParameter) As HtGetMasterDataCmdResParameter
            Dim ret As HtGetMasterDataCmdResParameter = New HtGetMasterDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.OK)}
            logger.Info($"Input：{Newtonsoft.Json.JsonConvert.SerializeObject(value)}")
            If Not ModelState.IsValid Then
                '送信情報が不正
                Return New CommonResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .wordCd = CommonWord.MSG_EXCEPTION}
            End If

            Dim db As New SystemDBContext

            If db.p_DataBaseOpenFlag = False Then
                'DB接続異常
                Return New CommonResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .wordCd = CommonWord.MSG_ERR_DB}
            End If

            Try

                '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 START
                Dim assetImportFlg As Boolean = True        '資産取込フラグ

                '設定マスタ（ロケごとの資産登録上限数）の取得
                Dim setting As M_SETTING = db.M_SETTING.Find(CommonConst.ASSET_REGISTRATION_LIMIT, "**", "**", "**")

                Dim employee As M_EMPLOYEE = db.M_EMPLOYEE _
                                                            .Where(Function(x) x.EMPLOYEE_CD = value.employeeCd) _
                                                            .FirstOrDefault
                Dim employeeAuth As String = employee.AUTH_DIV

                Dim headOffice As String = db.M_DEPARTMENT _
                                                            .Where(Function(x) x.DEPARTMENT_CD = employee.DEPARTMENT_CD) _
                                                            .Select(Function(x) x.HEAD_OFFICE_FLG) _
                                                            .FirstOrDefault


                Dim locList As List(Of HubLoc)
                Dim isAuth As Boolean
                If employeeAuth = "N" AndAlso headOffice = "N" Then
                    isAuth = False
                    locList = db.M_EMPLOYEE_HUB _
                                .Where(Function(x) x.EMPLOYEE_CD = value.employeeCd) _
                                .Select(Function(x) New HubLoc With {.L = x.HUB_LOC_L_CD, .M = x.HUB_LOC_M_CD}) _
                                .ToList()
                Else
                    isAuth = True
                    locList = db.M_EMPLOYEE_HUB _
                                .Where(Function(x) x.EMPLOYEE_CD = value.employeeCd) _
                                .Select(Function(x) New HubLoc With {.L = x.HUB_LOC_L_CD, .M = ""}) _
                                .ToList()
                End If

                If locList.Count <= 0 Then
                    '所属拠点が1件もない場合
                    ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                    ret.wordCd = CommonWord.MSG_ERR_NO_ASSIGNED_BASE
                    logger.Info("RETURN " + ret.returnCode.ToString)
                    Return ret
                End If

                Dim assetLocSearchFlg As Boolean = True    '資産ロケ絞り込みフラグ

                'Dim tableName As String = CommonConst.T_INV_ASSET
                'Dim sqlSearch As String = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName"
                'Dim count As Integer = db.Database.SqlQuery(Of Integer)(sqlSearch, New SqlClient.SqlParameter("@tableName", tableName)).FirstOrDefault()
                'Dim isExistTInvAsset As Boolean = count > 0


                '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 END

                '設定マスタ取得(照合機能)
                Dim MatchingFunction As M_SETTING = db.M_SETTING.Find(CommonConst.KEY_MATCHING_FUNCTION, "**", "**", "**")

                For Each item In value.masterDataList

                    Select Case item.masterName
                        Case CommonConst.MASTER_ASSET
                            '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 UPD START
                            ''資産マスタ情報を全件取得
                            'ret.adl = CType(db.Database.SqlQuery(Of AssetData)(GetAllAssetData()).ToArray(), AssetData())       '2012/02/13 戸祭 v1.1 変数名を変更
                            '資産取込フラグがONの場合
                            If assetImportFlg Then
                                '資産マスタ情報を取得
                                '20260925 Y.Ono ver4.3.1要求事項16 Start
                                'ret.adl = CType(db.Database.SqlQuery(Of AssetData)(GetAssetData(assetLocSearchFlg, isAuth, locList)).ToArray(), AssetData())       '2012/02/13 戸祭 v1.1 変数名を変更
                                ret.adl = CType(db.Database.SqlQuery(Of AssetData)(GetAssetData(assetLocSearchFlg, isAuth, locList, value.employeeCd)).ToArray(), AssetData())
                                '20260925 Y.Ono ver4.3.1要求事項16 End
                            End If
                        '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 UPD END
                        Case CommonConst.MASTER_LOCATION
                            'ロケーションマスタ情報を取得
                            ret.locationDataList = CType(db.Database.SqlQuery(Of LocationData)(GetLocationData(assetLocSearchFlg, isAuth, locList)).ToArray(), LocationData())
                        Case CommonConst.MASTER_EMPLOYEE
                            '社員マスタ情報を取得
                            Dim employeeArray(0) As EmployeeData
                            employeeArray(0) = New EmployeeData()
                            employeeArray(0).employeeCd = employee.EMPLOYEE_CD
                            employeeArray(0).employeeName = employee.EMPLOYEE_NAME
                            employeeArray(0).authDiv = employee.AUTH_DIV
                            employeeArray(0).headOfficeFlg = headOffice
                            ret.employeeDataList = employeeArray
                        Case CommonConst.MASTER_WORD
                            '文言マスタ情報を取得
                            ret.wordDataList = CType(db.Database.SqlQuery(Of WordData)(GetAllWordData()).ToArray(), WordData())
                        Case CommonConst.MASTER_SETTING
                            '設定マスタを全件取得
                            ret.settingDataList = CType(db.Database.SqlQuery(Of SettingData)(GetAllSettingData()).ToArray(), SettingData())
                        '2021/09/15 高瀬 v2.0 予約機能 START
                        Case CommonConst.MASTER_AUTH
                            '権限マスタを全件取得
                            ret.authDataList = CType(db.Database.SqlQuery(Of AuthData)(GetAllAuthData()).ToArray(), AuthData())
                        Case CommonConst.TABLE_INVENTORY
                            '棚卸開始情報を取得
                            If assetLocSearchFlg Then
                                ret.invList = CType(db.Database.SqlQuery(Of InvStartData)(GetInvStartDataByLoc(locList)).ToArray(), InvStartData())
                            Else
                                ret.invList = CType(db.Database.SqlQuery(Of InvStartData)(GetAllInvStartData()).ToArray(), InvStartData())
                            End If



                    End Select
                Next

                Using tx = db.Database.BeginTransaction((System.Data.IsolationLevel.Serializable))
                    db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_SEND_SUCCSE))
                    db.SaveChanges()
                    tx.Commit()
                End Using

                '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 UPD START
                '資産取込フラグがOFFの場合
                If Not assetImportFlg Then
                    logger.Error("ASSET NO SEND ERROR : LocCode Is Null")
                    ret.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN_ASSET_NO_SEND)
                End If
                '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 UPD END

            Catch ex As System.Data.Common.DbException
                'DB系の例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.wordCd = CommonWord.MSG_ERR_DB
            Catch ex As Exception
                '予期せぬ例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.wordCd = CommonWord.MSG_EXCEPTION
            End Try

            'DB切断
            db.subTerminate()
            Return ret
        End Function
#End Region

#Region "Private メソッド"
        '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 UPD START
        '20260925 Y.Ono ver4.3.1要求事項16 Start

        '''' <summary>
        '''' 資産マスタ情報を返す
        '''' </summary>
        '''' <returns>クエリ</returns>
        'Private Function GetAllAssetData() As String
        ''' <summary>
        ''' 資産マスタ情報を返す
        ''' </summary>
        ''' <param name="plmAssetLocSearchFlg">資産ロケ絞り込みフラグ(ON：ロケコードで絞り込み、OFF：全件)</param>
        ''' <param name="plmIsAuth">本部管理者、本部担当者、拠点管理者かどうか</param>
        ''' <param name="plmLocCodeList">ロケコードリスト</param>
        ''' <returns>クエリ</returns>


        Private Function GetAssetData(ByVal plmAssetLocSearchFlg As Boolean,
                                         ByVal plmIsAuth As Boolean,
                                         ByVal plmLocCodeList As List(Of HubLoc),
                                         ByVal plmEmployeeCd As String) As String

            'Private Function GetAssetData(ByVal plmAssetLocSearchFlg As Boolean,
            '                                 ByVal plmIsAuth As Boolean,
            '                                 ByVal plmLocCodeList As List(Of HubLoc)) As String
            '20260925 Y.Ono ver4.3.1要求事項16 End

            '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 UPD END
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            '2012/02/13 戸祭 v1.1 エイリアス名を変更 START
            sb.AppendLine("	 ASSET.ASSET_NO         AS no")
            sb.AppendLine("	,ISNULL(ASSET.ASSET_ID ,'')         AS id")
            sb.AppendLine("	,ISNULL(ASSET.ASSET_NAME ,'')       AS nm")
            sb.AppendLine("	,ISNULL(ASSET.MODEL_NO ,'')         AS mn")
            sb.AppendLine("	,ISNULL(ASSET.LOC_L_CD ,'')         AS llcd")
            sb.AppendLine("	,ISNULL(ASSET.LOC_M_CD ,'')         AS lmcd")
            sb.AppendLine("	,ISNULL(ASSET.LOC_S_CD ,'')         AS lscd")
            sb.AppendLine("	,CONVERT(INT,CAT.ASSET_ID_REG_DIV)   AS reg")
            sb.AppendLine("	,CONVERT(INT,CAT.EPC_SIZE)           AS sz")
            sb.AppendLine("	,CONVERT(VARCHAR,ASSET.ASSET_STATUS)  AS st")          'v1.1 戸祭 連続棚卸対応
            sb.AppendLine($",CASE WHEN CS.CAT_DIV IS NOT NULL THEN CASE WHEN CS.LEASE_ALERT_DIV = '{CommonConst.LEASE_ALERT_DIV_Y}' THEN ISNULL(LTRIM(STR(CS.LEASE_ALERT_CNT)), '') ELSE '' END ELSE CASE WHEN CM.CAT_DIV IS NOT NULL THEN CASE WHEN CM.LEASE_ALERT_DIV = '{CommonConst.LEASE_ALERT_DIV_Y}' THEN ISNULL(LTRIM(STR(CM.LEASE_ALERT_CNT)), '') ELSE '' END  ELSE ISNULL(LTRIM(STR(CL.LEASE_ALERT_CNT)), '') END END AS ual")
            '2012/02/13 戸祭 v1.1 エイリアス名を変更 END
            '-- 2020/02/12 v1.2 戸祭 貸出上限機能 START --
            sb.AppendLine($"	,CASE WHEN CS.CAT_DIV IS NOT NULL THEN CASE WHEN CS.LEASE_ALERT_DIV = '{CommonConst.LEASE_ALERT_DIV_Y}' THEN ISNULL(CS.LEASE_UPPER_ERR_DIV, '') ELSE '' END ELSE CASE WHEN CM.CAT_DIV IS NOT NULL THEN CASE WHEN CM.LEASE_ALERT_DIV = '{CommonConst.LEASE_ALERT_DIV_Y}' THEN ISNULL(CM.LEASE_UPPER_ERR_DIV, '') ELSE '' END  ELSE ISNULL(CL.LEASE_UPPER_ERR_DIV, '') END END AS ule")
            sb.AppendLine($"	,CASE WHEN CS.CAT_DIV IS NOT NULL THEN CASE WHEN CS.LEASE_ALERT_DIV = '{CommonConst.LEASE_ALERT_DIV_Y}' THEN ISNULL(LTRIM(STR(CS.LEASE_UPPER_LIMIT)), '') ELSE '' END ELSE CASE WHEN CM.CAT_DIV IS NOT NULL THEN CASE WHEN CM.LEASE_ALERT_DIV = '{CommonConst.LEASE_ALERT_DIV_Y}' THEN ISNULL(LTRIM(STR(CM.LEASE_UPPER_LIMIT)), '') ELSE '' END  ELSE ISNULL(LTRIM(STR(CL.LEASE_UPPER_LIMIT)), '') END END AS ul")
            sb.AppendLine("	,ISNULL(LTRIM(STR(ASSET.LEASE_CNT)), '') AS ln")
            '-- 2020/02/12 v1.2 戸祭 貸出上限機能 END --
            sb.AppendLine("	,CASE WHEN ASSET.PHYINV_TS IS NULL THEN 'N' ELSE 'Y' END  AS pif")    'v1.1 戸祭 連続棚卸対応
            sb.AppendLine("	,ASSET.PHYINV_STATUS                        AS pis")
            sb.AppendLine(" ,ASSET.MNG_LOC_L_CD                   AS mll")
            sb.AppendLine(" ,ISNULL(CONVERT(VARCHAR,ASSET.LEASE_STATUS), '')  AS ls")       '2021/09/15 高瀬 v2.0 予約機能
            sb.AppendLine(" ,ISNULL(LTRIM(STR(ASSET.LEASE_DUE_DT)), '') AS rdt ")           '2023/08/25 戸祭 v2.4.1 追加
            sb.AppendLine(" ,ISNULL(LTRIM(STR(ASSET.LEASE_LIMIT)), '') AS llt ")            '2023/08/25 戸祭 v2.4.1 追加
            sb.AppendLine(" ,ISNULL(LTRIM(STR(ASSET.GENERIC_LIMIT)), '') AS qlt ")          '2023/08/25 戸祭 v2.4.1 追加
            sb.AppendLine("	,ISNULL(ASSET.SUB_ASSET_NO ,'')       AS subno ")
            sb.AppendLine("	,ISNULL(ASSET.IS_TANGIBLE,' ')        AS istangible ")
            '20260925 Y.Ono ver4.3.1要求事項16 Start
            ' 棚卸資産.資産番号がnullの場合は、棚卸対象区分=N:棚卸対象外とする。
            ' 上記以外の場合
            '   資産マスタ.管理ロケ大区分 = ロケM.ロケ大区分が一致し、かつ、ロケM.管理資産棚卸区分=Yかつ、パラメータ.社員コード=棚卸資産.フリー項目6が一致の場合、棚卸対象区分=Y
            '   上記以外の場合、棚卸対象区分=N:棚卸対象外とする。

            'sb.AppendLine("	,CASE WHEN INV.ASSET_NO IS NULL THEN 'N' ELSE 'Y' END AS invflg ")              '棚卸区分　棚卸資産テーブルにデータがある場合はY、ない場合はN

            '20261029 ver4.3.1要求事項16 修正 Start
            ' ロケM.管理資産棚卸区分<>Yの場合は、従来どおり棚卸対象区分=Y:棚卸対象とする。
            'sb.AppendLine(",  CASE WHEN INV.ASSET_NO IS NULL THEN 'N' ")
            'sb.AppendLine("   ELSE (")
            'sb.Append("       CASE WHEN (OWN_INV_FLG = 'Y' AND RTRIM(LTRIM(FREE_ITEM6)) = '").Append(plmEmployeeCd).Append("' THEN 'Y' ").AppendLine("")
            'sb.AppendLine("       ELSE 'N' END ")
            'sb.AppendLine("   ) End As invflg ")
            sb.AppendLine(",  CASE WHEN INV.ASSET_NO IS NULL THEN 'N' ")
            sb.AppendLine("        WHEN LOC.OWN_INV_FLG = 'Y' THEN ")
            sb.Append("            CASE WHEN RTRIM(LTRIM(INV.FREE_ITEM6)) = '").Append(plmEmployeeCd.Replace("'", "''")).AppendLine("' THEN 'Y' ELSE 'N' END ")
            sb.AppendLine("        ELSE 'Y' END AS invflg ")
            '20261029 ver4.3.1要求事項16 修正 End
            '20260925 Y.Ono ver4.3.1要求事項16 End
            sb.AppendLine("FROM")
            sb.AppendLine($" {CommonConst.MASTER_ASSET} As ASSET")
            sb.AppendLine($" LEFT JOIN {CommonConst.T_INV_ASSET} As INV")
            sb.AppendLine(" On ASSET.ASSET_NO=INV.ASSET_NO")
            sb.AppendLine($" JOIN {CommonConst.MASTER_CATEGORY} As CAT")
            sb.AppendLine($" On ASSET.CAT_L_CD = CAT.CAT_L_CD")
            sb.AppendLine($" And CAT.CAT_DIV = '{CommonConst.CAT_L_DIV}'")
            '-- 2020/02/12 v1.2 戸祭 貸出上限機能 START --
            sb.AppendLine($"LEFT JOIN {CommonConst.MASTER_CATEGORY} AS CL")
            sb.AppendLine("	ON")
            sb.AppendLine($"	    CL.CAT_DIV = '{CommonConst.CAT_L_DIV }'")
            sb.AppendLine($"	AND CL.CAT_L_CD = ASSET.CAT_L_CD")
            sb.AppendLine($"	AND CL.LEASE_ALERT_DIV = '{CommonConst.LEASE_ALERT_DIV_Y}'")    'アラート区分=Y
            sb.AppendLine("	LEFT JOIN M_CATEGORY AS CM")
            sb.AppendLine("	ON")
            sb.AppendLine($"	    CM.CAT_DIV = '{CommonConst.CAT_M_DIV }'")
            sb.AppendLine($"	AND CM.CAT_L_CD = ASSET.CAT_L_CD")
            sb.AppendLine($"	AND CM.CAT_M_CD = ASSET.CAT_M_CD")
            sb.AppendLine($"	AND CM.LEASE_ALERT_DIV <> '{CommonConst.LEASE_ALERT_DIV_F}'")   'アラート区分<>F
            sb.AppendLine("	LEFT JOIN M_CATEGORY AS CS")
            sb.AppendLine("	ON")
            sb.AppendLine($"	    CS.CAT_DIV = '{CommonConst.CAT_S_DIV }'")
            sb.AppendLine($"	AND CS.CAT_L_CD = ASSET.CAT_L_CD")
            sb.AppendLine($"	AND CS.CAT_M_CD = ASSET.CAT_M_CD")
            sb.AppendLine($"	AND CS.CAT_S_CD = ASSET.CAT_S_CD")
            sb.AppendLine($"	AND CS.LEASE_ALERT_DIV <> '{CommonConst.LEASE_ALERT_DIV_F}'")   'アラート区分<>F
            '20260925 Y.Ono ver4.3.1要求事項16 Start
            sb.AppendLine(" LEFT JOIN M_LOCATION AS LOC ")
            sb.AppendLine(" ON ASSET.MNG_LOC_L_CD = LOC.LOC_L_CD ")
            '20261029 ver4.3.1要求事項16 修正 Start
            sb.AppendLine(" AND LOC.LOC_DIV = 'L' ")
            '20261029 ver4.3.1要求事項16 修正 End
            '20260925 Y.Ono ver4.3.1要求事項16 End
            '-- 2020/02/12 v1.2 戸祭 貸出上限機能 END --
            sb.AppendLine($" WHERE ASSET.ASSET_STATUS <> {CommonConst.ASSET_STATUS_RETIRE}")
            '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 START
            If plmAssetLocSearchFlg Then
                Dim loclist As String = ""
                Dim stringSql As String = ""
                For Each loc As HubLoc In plmLocCodeList
                    If plmIsAuth Then
                        loclist += " " + loc.L + ","
                    Else
                        stringSql += $"	((ASSET.LOC_L_CD = '{loc.L}' OR ASSET.MNG_LOC_L_CD = '{loc.L}') AND ASSET.LOC_M_CD = '{loc.M}') OR"
                    End If
                Next

                If plmIsAuth Then
                    loclist = loclist.TrimEnd(","c)
                    sb.AppendLine($" AND ASSET.LOC_L_CD IN ({loclist})")
                    sb.AppendLine($" OR ASSET.MNG_LOC_L_CD IN ({loclist})")
                ElseIf Not plmIsAuth AndAlso plmLocCodeList.Count > 0 Then
                    stringSql = stringSql.Substring(0, stringSql.Length - 2)
                    sb.AppendLine($"	AND ({stringSql})")
                End If

            End If
            '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 END

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 貸出先マスタ情報をすべて返す
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetAllLeasePlaceData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 LEASE_PLACE_CD    AS LeasePlaceCd")
            sb.AppendLine("	,ISNULL(LEASE_PLACE_NAME , '')  AS LeasePlaceName")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.MASTER_LEASE_PLACE}")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ロケーションマスタ情報をすべて返す
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetLocationData(ByVal plmAssetLocSearchFlg As Boolean,
                                            ByVal plmIsAuth As Boolean,
                                         ByVal plmLocCodeList As List(Of HubLoc)) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 LOC_DIV    AS LocDiv")
            sb.AppendLine("	,LOC_L_CD   AS LocLCd")
            sb.AppendLine("	,LOC_M_CD   AS LocMCd")
            sb.AppendLine("	,LOC_S_CD   AS LocSCd")
            sb.AppendLine("	,ISNULL(LOC_NAME , '')      AS LocName")                '2021/09/15 高瀬 v2.0 予約機能
            sb.AppendLine(" ,ISNULL(LOC_RFID_FLG ,'')  AS locRFIDFlg")            '2021/09/15 高瀬 v2.0 予約機能
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.MASTER_LOCATION}")
            If plmAssetLocSearchFlg Then
                sb.AppendLine(" WHERE ")
                Dim loclist As String = ""
                Dim stringSql As String = ""
                For Each loc As HubLoc In plmLocCodeList
                    If plmIsAuth Then
                        loclist += " " + loc.L + ","
                    Else
                        stringSql += $"	(LOC_L_CD = '{loc.L}' AND LOC_M_CD = '{loc.M}') OR"

                    End If
                Next

                If plmIsAuth Then
                    loclist = loclist.TrimEnd(","c)
                    sb.AppendLine($" LOC_L_CD IN ({loclist})")
                Else
                    sb.AppendLine($" {stringSql} ")
                    sb.AppendLine($" (LOC_DIV = 'L' AND LOC_L_CD = '{plmLocCodeList(0).L}') ")
                End If

            End If


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 社員マスタ情報(と部署マスタの本部フラグ)をすべて返す
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetAllEmployeeData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 EMPLOYEE_CD    AS EmployeeCd")
            sb.AppendLine("	,EMPLOYEE_NAME  AS EmployeeName")
            sb.AppendLine("	,AUTH_DIV       AS AuthDiv")                            '2021/09/15 高瀬 v2.0 予約機能
            sb.AppendLine("	,ISNULL(HEAD_OFFICE_FLG , '')  AS HeadOfficeFlg")       '2021/09/15 高瀬 v2.0 予約機能
            sb.AppendLine("FROM")
            '2021/09/15 高瀬 v2.0 予約機能 START
            sb.AppendLine($"{CommonConst.MASTER_EMPLOYEE} AS EMP")
            sb.AppendLine("LEFT OUTER JOIN")
            sb.AppendLine($"{CommonConst.MASTER_DEPARTMENT} AS DEP")
            sb.AppendLine("ON")
            sb.AppendLine("  EMP.DEPARTMENT_CD = DEP.DEPARTMENT_CD")
            '2021/09/15 高瀬 v2.0 予約機能 END
            sb.AppendLine("WHERE")
            sb.AppendLine("      AUTH_DIV <> 'S'")  'スーパー管理者以外を取得
            sb.AppendLine("  AND RETIRE_FLG <> 'Y'")  '退職者以外を取得             '2021/09/15 高瀬 v2.0 予約機能

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 文言マスタ情報をすべて返す
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetAllWordData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 WORD_DIV    AS WordDiv")
            sb.AppendLine("	,WORD_CD     AS WordCd")
            sb.AppendLine("	,WORDING     AS Wording")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.MASTER_WORD}")
            sb.AppendLine("WHERE")
            sb.AppendLine($"     LANG_DIV = '{CommonConst.LANG_DIV_LU}'")
            sb.AppendLine($" AND TOOL_DIV = '{CommonConst.WORD_TOOL_H}'")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 設定マスタ情報をすべて返す
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetAllSettingData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  KEY_DIV            AS KeyDiv")
            sb.AppendLine(" ,KEY1               AS Key1")
            sb.AppendLine(" ,KEY2               AS Key2")
            sb.AppendLine(" ,KEY3               AS Key3")
            sb.AppendLine(" ,ISNULL(REMARK , '')             AS Remark")
            sb.AppendLine(" ,ISNULL(DATA1 , '')              AS Data1")
            sb.AppendLine(" ,ISNULL(DATA2 , '')              AS Data2")
            sb.AppendLine(" ,ISNULL(DATA3 , '')              AS Data3")
            sb.AppendLine(" ,ISNULL(DATA4 , '')              AS Data4")
            sb.AppendLine(" ,ISNULL(DATA5 , '')              AS Data5")
            sb.AppendLine(" ,ISNULL(DATA6 , '')              AS Data6")
            sb.AppendLine(" ,ISNULL(DATA7 , '')              AS Data7")
            sb.AppendLine(" ,ISNULL(DATA8 , '')              AS Data8")
            sb.AppendLine(" ,ISNULL(DATA9 , '')              AS Data9")
            sb.AppendLine(" ,ISNULL(DATA10 , '')             AS Data10")
            sb.AppendLine(" ,ISNULL(SPECIAL_DATA_DIV , '')   AS SpecialDataDiv")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.MASTER_SETTING}")

            '2023/10/3 柴田v3.0 START
            'sb.AppendLine("UNION")
            'sb.AppendLine("SELECT")
            'sb.AppendLine("  'Inventory_start'  AS KeyDiv")
            'sb.AppendLine(" ,'**'               AS Key1")
            'sb.AppendLine(" ,'**'               AS Key2")
            'sb.AppendLine(" ,'**'               AS Key3")
            'sb.AppendLine(" ,''                 AS Remark")
            'sb.AppendLine(" ,ISNULL((SELECT '1' FROM T_INVENTORY WHERE INV_CONFIRM_TS IS NULL), '0')  AS Data1")
            'sb.AppendLine(" ,''                              AS Data2")
            'sb.AppendLine(" ,''                              AS Data3")
            'sb.AppendLine(" ,''                              AS Data4")
            'sb.AppendLine(" ,''                              AS Data5")
            'sb.AppendLine(" ,''                              AS Data6")
            'sb.AppendLine(" ,''                              AS Data7")
            'sb.AppendLine(" ,''                              AS Data8")
            'sb.AppendLine(" ,''                              AS Data9")
            'sb.AppendLine(" ,''                              AS Data10")
            'sb.AppendLine(" ,''                              AS SpecialDataDiv")
            '2023/10/3 柴田v3.0 END

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        '2021/09/15 高瀬 v2.0 予約機能 START
        ''' <summary>
        ''' 権限マスタ情報をすべて返す
        ''' </summary>
        ''' <returns></returns>
        Private Function GetAllAuthData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 FUNCTION_ID                    AS FunctionId")
            sb.AppendLine("	,AUTH_DIV                       AS AuthDiv")
            sb.AppendLine("	,AUTH_TYPE                      AS AuthType")
            sb.AppendLine("	,ISNULL(HEAD_OFFICE_FLG , '')   AS HeadOfficeFlg")
            sb.AppendLine("	,ISNULL(READ_AUTH_FLG , '')     AS ReadAuthFlg")
            sb.AppendLine("	,ISNULL(UPD_AUTH_FLG , '')      AS UpdAuthFlg")
            sb.AppendLine("	,ISNULL(READ_HUB_AUTH_FLG , '') AS ReadHubAuthFlg")
            sb.AppendLine("	,ISNULL(UPD_HUB_AUTH_FLG , '')  AS UpdHubAuthFlg")
            sb.AppendLine("	,ISNULL(AUTH_OPTION , '')       AS AuthOption")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.MASTER_AUTH}")
            sb.AppendLine("WHERE")
            sb.AppendLine($" PROCESS_TOOL_DIV = '{CommonConst.WORD_TOOL_H}'")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 社員所属拠点マスタをすべて返す
        ''' </summary>
        ''' <returns></returns>
        Private Function GetAllEmployeeHubData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 HUB.EMPLOYEE_CD                  AS EmployeeCd")
            sb.AppendLine("	,ISNULL(HUB.HUB_LOC_L_CD , '')    AS HubLocLCd")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.MASTER_EMPLOYEE_HUB} HUB")
            sb.AppendLine("INNER JOIN")
            sb.AppendLine($"{CommonConst.MASTER_EMPLOYEE} EMP")
            sb.AppendLine("ON")
            sb.AppendLine(" HUB.EMPLOYEE_CD = EMP.EMPLOYEE_CD")
            sb.AppendLine("WHERE")
            sb.AppendLine("      EMP.AUTH_DIV <> 'S'")
            sb.AppendLine("  AND EMP.RETIRE_FLG <> 'Y'")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 社員所属拠点マスタと共通拠点を併せて返す
        ''' </summary>
        ''' <returns></returns>
        Private Function GetAllEmployeeCommonHubData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 HUB.EMPLOYEE_CD                  AS EmployeeCd")
            sb.AppendLine("	,ISNULL(HUB.HUB_LOC_L_CD , '')    AS HubLocLCd")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.MASTER_EMPLOYEE_HUB} HUB")
            sb.AppendLine("INNER JOIN")
            sb.AppendLine($"{CommonConst.MASTER_EMPLOYEE} EMP")
            sb.AppendLine("ON")
            sb.AppendLine(" HUB.EMPLOYEE_CD = EMP.EMPLOYEE_CD")
            sb.AppendLine("WHERE")
            sb.AppendLine("      EMP.AUTH_DIV <> 'S'")
            sb.AppendLine("  AND EMP.RETIRE_FLG <> 'Y'")

            sb.AppendLine("UNION")
            sb.AppendLine("SELECT")
            sb.AppendLine("	 EMP.EMPLOYEE_CD                  AS EmployeeCd")
            sb.AppendLine("	,ISNULL(LOC.LOC_L_CD, '')         AS HubLocLCd")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.MASTER_EMPLOYEE} EMP")
            sb.AppendLine("CROSS JOIN")
            sb.AppendLine($"{CommonConst.MASTER_LOCATION} LOC")
            sb.AppendLine("WHERE")
            sb.AppendLine("      EMP.AUTH_DIV <> 'S'")
            sb.AppendLine("  AND EMP.RETIRE_FLG <> 'Y'")
            sb.AppendLine("  AND LOC.LOC_DIV = 'L'")
            sb.AppendLine("  AND LOC.LOC_SHARE_FLG = 'Y'")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        '2021/09/15 高瀬 v2.0 予約機能 END
        ''' <summary>
        ''' 棚卸開始情報をすべて返す
        ''' </summary>
        ''' <returns></returns>
        Private Function GetAllInvStartData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 TID.INV_LOC_L_CD                  AS InvLocLCd")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.TABLE_INVENTORY} TI")
            sb.AppendLine("INNER JOIN")
            sb.AppendLine($"{CommonConst.TABLE_INVENTORY_DETAIL} TID")
            sb.AppendLine("ON")
            sb.AppendLine(" TI.INV_ID = TID.INV_ID")
            sb.AppendLine("WHERE")
            sb.AppendLine("TI.INV_CONFIRM_TS IS NULL")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 棚卸開始情報のロケ大区分コードに合致するデータ返す
        ''' </summary>
        ''' <returns></returns>
        Private Function GetInvStartDataByLoc(ByVal plmLocCodeList As List(Of HubLoc)) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 TID.INV_LOC_L_CD                  AS InvLocLCd")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.TABLE_INVENTORY} TI")
            sb.AppendLine("INNER JOIN")
            sb.AppendLine($"{CommonConst.TABLE_INVENTORY_DETAIL} TID")
            sb.AppendLine("ON")
            sb.AppendLine(" TI.INV_ID = TID.INV_ID")
            sb.AppendLine("WHERE")
            sb.AppendLine("TI.INV_CONFIRM_TS IS NULL")
            Dim loclist As String = ""
            Dim stringSql As String = ""
            For Each loc As HubLoc In plmLocCodeList
                loclist += " " + loc.L + ","
            Next

            loclist = loclist.TrimEnd(","c)
            sb.AppendLine($" AND TID.INV_LOC_L_CD IN ({loclist})")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        '''' <summary>
        '''' 工場マスタ情報を返す
        '''' </summary>
        '''' <returns>クエリ</returns>
        'Private Function GetAllAssetData() As String
        ''' <summary>
        ''' 工場マスタ情報を返す
        ''' </summary>
        ''' <param name="plmLocCode">資産ロケ取得コード</param>
        ''' <returns>クエリ</returns>
        Private Function GetAllConstRuctionData(ByVal plmLocCode As String) As String

            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 CONST_NO               AS no")
            sb.AppendLine("	,CONST_START_DT         AS stDt")
            sb.AppendLine("	,CONST_END_DT           AS endDt")
            sb.AppendLine("	,CONST_LENT_START_DT    AS lentStDt")
            sb.AppendLine("	,CONST_LENT_END_DT      AS lentEndDt")
            sb.AppendLine("	,CONST_CONTENTS         AS contents")
            sb.AppendLine("	,CONST_EMPLOYEE_CD      AS empCd")
            sb.AppendLine("	,CONST_EMPLOYEE_NAME    AS empName")
            sb.AppendLine("	,MNG_LOC_L_CD           AS mngLocLCd")
            sb.AppendLine("	,CONST_INFO1            AS info1")
            sb.AppendLine("	,CONST_INFO2            AS info2")
            sb.AppendLine("	,CONST_INFO3            AS info3")
            sb.AppendLine("	,CONST_INFO4            AS info4")
            sb.AppendLine("	,CONST_INFO5            AS info5")
            sb.AppendLine("	,CONST_INFO6            AS info6")
            sb.AppendLine("	,CONST_INFO7            AS info7")
            sb.AppendLine("	,CONST_INFO8            AS info8")
            sb.AppendLine("	,CONST_INFO9            AS info9")
            sb.AppendLine("	,CONST_INFO10           AS info10")
            sb.AppendLine("	,CONST_INFO11           AS info11")
            sb.AppendLine("	,CONST_INFO12           AS info12")
            sb.AppendLine("	,CONST_INFO13           AS info13")
            sb.AppendLine("	,CONST_INFO14           AS info14")
            sb.AppendLine("	,CONST_INFO15           AS info15")
            sb.AppendLine("	,CONST_INFO16           AS info16")
            sb.AppendLine("	,CONST_INFO17           AS info17")
            sb.AppendLine("	,CONST_INFO18           AS info18")
            sb.AppendLine("	,CONST_INFO19           AS info19")
            sb.AppendLine("	,CONST_INFO20           AS info20")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.MASTER_CONSTRUCTION}")
            sb.AppendLine("WHERE")
            sb.AppendLine($" CONST_CONFIRM_TS IS NULL")            '工事完了日時がNULL

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 仮置きテーブル情報を返す
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetAllTemporaryData(ByVal plmConstId As String) As String

            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine(" TEMP_ID             AS tempId")
            sb.AppendLine(",TEMP_DT             AS tempDt")
            'sb.AppendLine(",WORK_EMPLOYEE_CD    AS tempEmpCd")
            'sb.AppendLine(",WORK_EMPLOYEE_NAME  AS tempEmpName")
            sb.AppendLine(",TEMP_INFO1          AS tempInfo1")
            sb.AppendLine(",TEMP_INFO2          AS tempInfo2")
            sb.AppendLine(",TEMP_INFO3          AS tempInfo3")
            sb.AppendLine(",TEMP_INFO4          AS tempInfo4")
            sb.AppendLine(",TEMP_INFO5          AS tempInfo5")
            sb.AppendLine(",TEMP_INFO6          AS tempInfo6")
            sb.AppendLine(",TEMP_INFO7          AS tempInfo7")
            sb.AppendLine(",TEMP_REMARK         AS tempRemark")
            sb.AppendLine(" FROM")
            sb.AppendLine($"{CommonConst.MASTER_CONSTRUCTION}")
            sb.AppendLine(" CON INNER JOIN ")
            sb.AppendLine($"{CommonConst.TABLE_TEMPORARY}")
            sb.AppendLine(" TMP ON CON.CONST_NO = TMP.REG_CONST_NO")
            sb.AppendLine(" WHERE TMP.REL_CONST_NO IS NULL")
            sb.AppendLine($" AND CON.CONST_NO = '{plmConstId}'")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 仮置き資産テーブル情報を返す
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetAllTempAssetData(ByVal plmTempId As String) As String

            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine(" ASSET_NO          AS no")
            sb.AppendLine(" FROM")
            sb.AppendLine($"{CommonConst.MASTER_CONSTRUCTION}")
            sb.AppendLine(" INNER JOIN ")
            sb.AppendLine($"{CommonConst.TABLE_TEMPORARY}")
            sb.AppendLine(" TMPO ON CONST_NO = TMPO.REG_CONST_NO")
            sb.AppendLine(" INNER JOIN ")
            sb.AppendLine($"{CommonConst.TABLE_TEMP_ASSET}")
            sb.AppendLine(" TMPA ON TMPA.TEMP_ID = TMPO.TEMP_ID")
            sb.AppendLine(" WHERE TMPO.REL_CONST_NO IS NULL")
            sb.AppendLine($" AND TMPO.TEMP_ID = '{plmTempId}'")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産テーブル情報を返す
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetAllConstAssetData(ByVal plmConstId As String) As String

            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine(" ASSET.ASSET_NO          AS no")
            sb.AppendLine(" FROM")
            sb.AppendLine($"{CommonConst.MASTER_CONSTRUCTION}")
            sb.AppendLine(" CON INNER JOIN ")
            sb.AppendLine($"{CommonConst.TABLE_CONST_ASSET}")
            sb.AppendLine(" ASSET ON ASSET.CONST_NO = CON.CONST_NO")
            sb.AppendLine(" WHERE ASSET.CONST_ASSET_STATUS = '1'")
            sb.AppendLine(" AND ASSET.TEMP_ID IS NULL")
            sb.AppendLine($" AND CON.CONST_NO = '{plmConstId}'")
            sb.AppendLine(" UNION ")
            sb.AppendLine(" SELECT ")
            sb.AppendLine(" TMPA.ASSET_NO AS no")
            sb.AppendLine(" FROM")
            sb.AppendLine($" {CommonConst.MASTER_CONSTRUCTION} CON")
            sb.AppendLine(" INNER JOIN")
            sb.AppendLine($" {CommonConst.TABLE_TEMPORARY} TMPO")
            sb.AppendLine("     ON CON.CONST_NO = TMPO.REL_CONST_NO")
            sb.AppendLine(" INNER JOIN ")
            sb.AppendLine($" {CommonConst.TABLE_TEMP_ASSET} TMPA")
            sb.AppendLine("     ON TMPO.TEMP_ID = TMPA.TEMP_ID")
            sb.AppendLine(" WHERE")
            sb.AppendLine($"     CON.CONST_NO = '{plmConstId}'")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約テーブル情報を返す
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetAllReserveData() As String

            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine(" R.RESERVE_ID AS reserveId,")
            sb.AppendLine(" CON.CONST_NO AS constNo,")
            sb.AppendLine(" CON.CONST_CONTENTS AS contents,")
            sb.AppendLine(" R.CHARGE_EMP_CD AS emp,")
            sb.AppendLine(" R.REAL_START_DT AS resStDt,")
            sb.AppendLine(" R.REAL_END_DT AS resEndDt,")
            sb.AppendLine(" RD.ASSET_NO AS no")
            sb.AppendLine(" FROM ")
            sb.AppendLine(" ( SELECT")
            sb.AppendLine(" ASSET_NO,")
            sb.AppendLine(" RESERVE_ID")
            sb.AppendLine(" FROM ")
            sb.AppendLine($"{CommonConst.TABLE_RESERVE_DETAIL}")
            sb.AppendLine(" WHERE ")
            sb.AppendLine(" RESERVE_STATUS = '0' OR RESERVE_STATUS = '1'")
            sb.AppendLine(" ) AS RD")
            sb.AppendLine(" INNER JOIN ")
            sb.AppendLine($"{CommonConst.MASTER_ASSET} ASSET")
            sb.AppendLine(" ON ASSET.ASSET_NO = RD.ASSET_NO ")
            sb.AppendLine(" INNER JOIN ")
            sb.AppendLine($"{CommonConst.TABLE_RESERVE} R")
            sb.AppendLine(" ON R.RESERVE_ID = RD.RESERVE_ID")
            sb.AppendLine(" INNER JOIN ")
            sb.AppendLine($"{CommonConst.MASTER_CONSTRUCTION} CON")
            sb.AppendLine(" ON CON.CONST_NO = R.REMARK_INTERNAL")
            sb.AppendLine(" WHERE ")
            sb.AppendLine(" ASSET.LEASE_STATUS = '1'")



            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

#End Region


    End Class
End Namespace