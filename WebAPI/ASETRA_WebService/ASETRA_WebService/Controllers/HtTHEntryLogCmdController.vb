Imports System.Web.Http
Imports System.Web.Http.Cors

Namespace Controllers
    ''' <summary>
    ''' 処理データを登録するコントローラー
    ''' </summary>
    Public Class HtTHEntryLogCmdController
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
#End Region

#Region "Public メソッド"
        ''' <summary>
        ''' マスタデータ送信処理
        ''' </summary>
        ''' <returns>戻り値クラス</returns>
        ''' <remarks>api/HtAssetRecv呼び出し</remarks>
        <HttpPost>
        <EnableCors("*", "*", "*")>
        Public Function Post(<FromBody> ByVal value As HtTHEntryLogCmdReqParameter) As CommonResParameterTH
            Dim ret As CommonResParameterTH = New CommonResParameterTH() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.OK)}
            logger.Info($"Input：{Newtonsoft.Json.JsonConvert.SerializeObject(value)}")
            If Not ModelState.IsValid Then
                '送信情報が不正
                Return New CommonResParameterTH() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .message = CommonConst.MSG_INV_ARG}
            End If

            Dim db As New SystemDBContext

            If db.p_DataBaseOpenFlag = False Then
                'DB接続異常
                Return New CommonResParameterTH() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .message = CommonConst.MSG_ERR_DB}
            End If

            Try
                '操作ログ登録呼び出し
                Dim param = New HtEntryLogCmdReqParameter With {
                    .logDataDetailList = value.logDataDetailList
                }
                Dim entryLog = New HtEntryLogCmdController()
                Dim retEntryLog = entryLog.Post(param)

                If Not retEntryLog.returnCode = ReturnCodeEnum.OK Then
                    ret.returnCode = retEntryLog.returnCode
                    ret.message = CommonFunction.GetMessage(db, retEntryLog.wordCd)
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

            'DB切断
            db.subTerminate()
            Return ret
        End Function
#End Region
    End Class
End Namespace