Imports System.Net
Imports System.Web.Http
Imports System.Web.Http.Cors

Namespace Controllers
    Public Class HtGetDateTimeCmdController
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
        ''' システム日付・時刻送信処理
        ''' </summary>
        ''' <returns>戻り値クラス</returns>
        <HttpGet>
        <HttpPost>
        <EnableCors("*", "*", "*")>
        Public Function Post() As HtGetDateTimeCmdResParameter
            Dim ret As HtGetDateTimeCmdResParameter = New HtGetDateTimeCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.OK)}

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

                ret.getSystemDateTime = CType(db.Database.SqlQuery(Of GetDateTime)(GetSystemDateTime()).ToArray(), GetDateTime())

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
        ''' <summary>
        ''' システム日付・時刻を返す
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetSystemDateTime() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine(" CONVERT(VARCHAR,GETDATE(),112) AS 'systemDt'")
            sb.AppendLine(",FORMAT(GETDATE(),'HHmmssfff') AS 'systemTm'")

            logger.Info(vbCrLf & sb.ToString())

            Return sb.ToString
        End Function

#End Region

    End Class
End Namespace