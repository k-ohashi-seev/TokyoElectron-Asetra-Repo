Imports System.Web.Http
Imports System.Web.Http.Cors

Namespace Controllers
    ''' <summary>
    ''' 予約確認を行うコントローラー
    ''' </summary>
    Public Class HtTHReserveCheckCmdController
        Inherits ApiController

#Region "固定値定義"
        ''' <summary>
        ''' 処理名
        ''' </summary>
        Private TASK_NAME As String = Me.GetType().Name
#End Region

#Region "変数定義"

        ''' <summary>
        ''' ログ出力クラス
        ''' </summary>
        Private logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
#End Region

#Region "Public メソッド"
        ''' <summary>
        ''' 予約確認処理
        ''' </summary>
        ''' <param name="value">予約確認情報</param>
        ''' <returns>戻り値クラス</returns>
        ''' <remarks>api/HtTHReserveCheckCmd呼び出し</remarks>
        <HttpPost>
        <EnableCors("*", "*", "*")>
        Public Function Post(<FromBody> ByVal value As HtTHReserveCheckCmdReqParameter) As HtTHReserveCheckCmdResParameter
            Dim ret As HtTHReserveCheckCmdResParameter = New HtTHReserveCheckCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.OK)}
            logger.Info($"Input：{Newtonsoft.Json.JsonConvert.SerializeObject(value)}")
            If Not ModelState.IsValid Then
                '送信情報が不正
                logger.Info($"SEND INFO ERROR")
                Return New HtTHReserveCheckCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .message = CommonConst.MSG_INV_ARG}
            End If

            Dim db As New SystemDBContext

            If db.p_DataBaseOpenFlag = False Then
                'DB接続異常
                logger.Info($"DB ERROR")
                Return New HtTHReserveCheckCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .message = CommonConst.MSG_ERR_DB}
            End If

            Try
                '貸出返却区分が返却以外の場合
                If value.lentRetFlg <> CommonConst.LENT_RET_FLG.RET Then
                    '予約データ取得
                    Dim reserveData As ReserveInfo() = db.Database.SqlQuery(Of ReserveInfo)(GetReserveData(value.assetNo)).ToArray()

                    If reserveData.Count > 0 Then
                        '存在する場合
                        If reserveData(0).changeEmpCd = value.employeeCd Then
                            '予約明細.担当社員コード = 引数.ログイン担当者コード
                            ret.returnCode = Convert.ToInt32(ReturnCodeEnum.RESERVED_SELF)
                            ret.startDt = If(IsNothing(reserveData(0).lendingDt), Nothing, reserveData(0).lendingDt.ToString)
                            ret.endDt = If(IsNothing(reserveData(0).returnDt), Nothing, reserveData(0).returnDt.ToString)
                            ret.constNo = reserveData(0).remark
                        Else
                            '.予約明細.担当社員コードに該当する社員データを取得する
                            Dim emp As M_EMPLOYEE = db.M_EMPLOYEE.Find(value.employeeCd)

                            ret.returnCode = Convert.ToInt32(ReturnCodeEnum.RESERVED_OTHER)
                            ret.startDt = If(IsNothing(reserveData(0).lendingDt), Nothing, reserveData(0).lendingDt.ToString)
                            ret.endDt = If(IsNothing(reserveData(0).returnDt), Nothing, reserveData(0).returnDt.ToString)
                            ret.employeeName = emp.EMPLOYEE_NAME
                            ret.constNo = reserveData(0).remark
                        End If

                        '処理を終了
                        logger.Info("RETURN " + ret.returnCode.ToString)
                        Return ret
                    End If
                End If


                '予約メンテナンスデータ取得
                Dim reserveMainteData As ReserveMainteInfo() = CType(db.Database.SqlQuery(Of ReserveMainteInfo)(GetReserveMainteData(value.assetNo, value.systemDt)).ToArray(), ReserveMainteInfo())

                If reserveMainteData.Count > 0 Then
                    '存在する場合
                    ret.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN_ASSET_NO_SEND)
                    ret.startDt = If(IsNothing(reserveMainteData(0).mainteStartDt), Nothing, reserveMainteData(0).mainteStartDt.ToString)
                    ret.endDt = If(IsNothing(reserveMainteData(0).mainteEndDt), Nothing, reserveMainteData(0).mainteEndDt.ToString)
                End If

            Catch ex As System.Data.Common.DbException
                'DB系の例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.message = CommonConst.MSG_ERR_DB
            Catch ex As Exception
                '予期せぬ例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.message = CommonConst.MSG_EXCEPTION
            End Try

            logger.Info("RETURN " + ret.returnCode.ToString)
            Return ret
        End Function
#End Region

#Region "Private メソッド"

#Region "予約データ取得(貸出中)"
        ''' <summary>
        ''' 予約データ取得(貸出中)
        ''' </summary>
        Private Function GetReserveData(ByVal assetNo As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT ")
            sb.AppendLine(" RSV.REMARK AS remark")
            sb.AppendLine(" ,RESVD.CHARGE_EMP_CD AS changeEmpCd")
            sb.AppendLine(" ,RESVD.LENDING_DT AS lendingDt")
            sb.AppendLine(" ,RESVD.RETURN_DT AS returnDt")
            sb.AppendLine("FROM T_RESERVE RSV ")
            sb.AppendLine("INNER JOIN T_RESERVE_DETAIL RESVD ")
            sb.AppendLine("ON RSV.RESERVE_ID = RESVD.RESERVE_ID ")
            sb.AppendLine($"AND RESVD.RESERVE_STATUS = '{CommonConst.RESERVE_STATUS.LENDING}' ")
            sb.AppendLine($"AND RESVD.ASSET_NO = '{assetNo}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function
#End Region

#Region "予約メンテナンスデータ取得(メンテナンス期間)"
        ''' <summary>
        ''' 予約メンテナンスデータ取得(メンテナンス期間)
        ''' </summary>
        Private Function GetReserveMainteData(ByVal assetNo As String, ByVal systemDt As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT ")
            sb.AppendLine(" RESVM.MAINTE_START_DT AS mainteStartDt")
            sb.AppendLine(" ,RESVM.MAINTE_END_DT AS mainteEndDt")
            sb.AppendLine("FROM T_RESERVE_MAINTE RESVM ")
            sb.AppendLine("WHERE ")
            sb.AppendLine($"RESVM.ASSET_NO = '{assetNo}' ")
            sb.AppendLine($"AND RESVM.MAINTE_START_DT <= '{systemDt}' ")
            sb.AppendLine($"AND RESVM.MAINTE_END_DT >= '{systemDt}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function
#End Region

#End Region

    End Class

    ''' <summary>
    ''' 予約情報
    ''' </summary>
    Public Class ReserveInfo
        ''' <summary>
        ''' 備考
        ''' </summary>
        ''' <returns></returns>
        Property remark As String
        ''' <summary>
        ''' 担当者員コード
        ''' </summary>
        ''' <returns></returns>
        Property changeEmpCd As String
        ''' <summary>
        ''' 貸出日
        ''' </summary>
        ''' <returns></returns>
        Property lendingDt As Integer? = Nothing
        ''' <summary>
        ''' 返却日
        ''' </summary>
        ''' <returns></returns>
        Property returnDt As Integer? = Nothing

    End Class

    ''' <summary>
    ''' 予約メンテナンス情報
    ''' </summary>
    Public Class ReserveMainteInfo
        ''' <summary>
        ''' メンテナンス開始日
        ''' </summary>
        ''' <returns></returns>
        Property mainteStartDt As Integer
        ''' <summary>
        ''' メンテナンス終了日
        ''' </summary>
        ''' <returns></returns>
        Property mainteEndDt As Integer

    End Class

End Namespace