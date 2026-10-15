Imports System.IO
Imports System.Data.SqlClient
''' <summary>
''' 返却 APIクラス
''' </summary>
Public Class WsAssetReturn
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

    Public Class DtoAssetReturn

        ''' <summary>資産番号</summary>
        Public AssetNo As String
        ''' <summary>資産ID</summary>
        Public AssetID As String
        ''' <summary>処理ツール区分</summary>
        Public ProcessToolDiv As String
        ''' <summary>処理区分</summary>
        Public ProcessDiv As String
        Public LeaseDueDt As String
        ''' <summary>ログイン担当者コード</summary>
        Public EmployeeCd As String

    End Class

    ''' <summary>
    ''' 資産番号 返却登録
    ''' </summary>
    ''' <param name="params">セットするパラメーターリスト</param>
    ''' <returns>True:表示する  False:表示しない</returns>
    Public Function WebApiAssetReturn(params As List(Of DtoAssetReturn)) As ClsWebApiClient.EnResultAPI
        Try

            Dim ProcesCount As Integer = 0
            Dim ReturnDate = Now
            Dim C_WebApiClient As New ClsWebApiClient
            C_WebApiClient.p_WebServerURL = Me._sWebApiUrl     'WEB API 接続URL
            Dim ArayAssetNo() As String = Nothing

            For LopCnt As Integer = 0 To params.Count - 1

                'パラメータセット
                C_WebApiClient.InitEntryProcessData()                   'バッファ初期化
                With C_WebApiClient.p_tempEntryProcessData
                    ReDim Preserve ArayAssetNo(ProcesCount)
                    ArayAssetNo(ProcesCount) = params(LopCnt).AssetNo
                    ProcesCount += 1

                    .processLogId = ProcesCount.ToString                '処理履歴ID
                    .processDt = ReturnDate.ToString("yyyyMMdd")        '処理日
                    .processTm = ReturnDate.ToString("HHmmss") & "000"  '処理時刻
                    .assetNo = params(LopCnt).AssetNo                   '資産番号
                    .assetId = params(LopCnt).AssetID                   '資産ID
                    .processToolDiv = params(LopCnt).ProcessToolDiv     '処理ツール区分
                    .processDiv = params(LopCnt).ProcessDiv             '処理区分
                    .leasePlaceCd = ""                                  '貸出先場所コード
                    .leaseEmployeeCd = ""                               '貸出先担当者社員コード
                    .leaseDueDt = ""                                    '貸出返却予定日
                    .locLCd = ""                                        'ロケ大区分コード
                    .locMCd = ""                                        'ロケ中区分コード
                    .locSCd = ""                                        'ロケ小区分コード
                    .employeeCd = params(LopCnt).EmployeeCd             'ログイン担当者コード                                                                           
                    .remark = ""                '備考
                End With

                C_WebApiClient.AddEntryProcessData()    'バッファ→配列追加
            Next LopCnt

            '登録送信
            Dim Result As ClsWebApiClient.EnResultAPI = C_WebApiClient.fncEntryProcesSend(params(0).ProcessDiv, ProcesCount, ArayAssetNo)
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
