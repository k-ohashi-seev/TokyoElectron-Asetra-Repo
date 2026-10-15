Imports System.IO
Imports System.Data.SqlClient
''' <summary>
''' 棚卸 APIクラス
''' </summary>
Public Class WsAssetInventory
    Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "変数宣言"
    ''' <summary>WEB API 接続URL</summary>
    Private _sWebApiUrl As String

#End Region

#Region "コンストラクタ"
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="sWebApiUrl">WEB API 接続URL</param>
    ''' <remarks></remarks>
    Public Sub New(sWebApiUrl As String)
        Me._sWebApiUrl = sWebApiUrl
    End Sub
#End Region

    Public Class DtoAssetInventory

        ''' <summary>資産番号</summary>
        Public an As String
        ''' <summary>実棚日時</summary>
        Public pt As String
        ''' <summary>実棚ステータス</summary>
        Public ps As String
        ''' <summary>実棚ロケーション大区分コード</summary>
        Public lc As String
        ''' <summary>実棚ロケーション中区分コード</summary>
        Public mc As String
        ''' <summary>実棚ロケーション小区分コード</summary>
        Public sc As String
        ''' <summary>実棚実施者社員コード</summary>
        Public ec As String

    End Class

    ''' <summary>
    ''' 実棚情報更新
    ''' </summary>
    ''' <param name="params">セットするパラメーターリスト</param>
    ''' <returns>True:表示する  False:表示しない</returns>
    Public Function WebApiAssetInventory(params As List(Of DtoAssetInventory)) As ClsWebApiClient.EnResultAPI
        Try
            Dim ProcesCount As Integer = 0
            Dim ReturnDate = Now
            Dim C_WebApiClient As New ClsWebApiClient
            C_WebApiClient.p_WebServerURL = Me._sWebApiUrl     'WEB API 接続URL
            Dim ArayAssetNo() As String = Nothing

            For LopCnt As Integer = 0 To params.Count - 1

                'パラメータセット
                C_WebApiClient.InitEntryInventData()                   'バッファ初期化
                With C_WebApiClient.p_tempEntryInventData
                    ReDim Preserve ArayAssetNo(ProcesCount)
                    ArayAssetNo(ProcesCount) = params(LopCnt).an
                    ProcesCount += 1

                    .an = params(LopCnt).an                   '資産番号
                    .pt = params(LopCnt).pt                   '実棚日時
                    .ps = params(LopCnt).ps                   '実棚ステータス
                    .lc = params(LopCnt).lc                   '実棚ロケーション大区分コード
                    .mc = params(LopCnt).mc                   '実棚ロケーション中区分コード
                    .sc = params(LopCnt).sc                   '実棚ロケーション小区分コード
                    .ec = params(LopCnt).ec                   '実棚実施者社員コード
                End With

                C_WebApiClient.AddEntryInventData()    'バッファ→配列追加
            Next LopCnt



            '登録送信
            Dim Result As ClsWebApiClient.EnResultAPI = C_WebApiClient.fncEntryInventSend()
            Return Result

        Catch ex As Exception
            logger.Error(ex.Message)
            Return Nothing

            'Call SVSubWriteLog(EN_LogType.Err, CLASS_NAME, funcName, LOG_ERR_CATHCH_TITLE, ex.Message)

            ''WebAPIから異常応答がありました。
            'C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990019", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
        End Try
    End Function

End Class
