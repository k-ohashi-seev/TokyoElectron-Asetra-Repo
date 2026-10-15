Imports System.IO
Imports System.Data.SqlClient

''' <summary>
''' 貸出APIクラス
''' </summary>
Public Class WsAssetLending

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

    Public Class DtoAssetLending

        ''' <summary>資産番号</summary>
        Public AssetNo As String
        ''' <summary>資産ID</summary>
        Public AssetID As String
        ''' <summary>処理ツール区分</summary>
        Public ProcessToolDiv As String
        ''' <summary>処理区分</summary>
        Public ProcessDiv As String
        ''' <summary>貸出先場所コード</summary>
        Public LeasePlaceCd As String
        ''' <summary>貸出先担当者社員コード</summary>
        Public LeaseEmployeeCd As String
        ''' <summary>貸出返却予定日</summary>
        Public LeaseDueDt As String
        ''' <summary>ログイン担当者コード</summary>
        Public EmployeeCd As String

    End Class

    ''' <summary>
    ''' 資産番号 貸出登録
    ''' </summary>
    ''' <param name="params">セットするパラメーターリスト</param>
    ''' <returns>True:表示する  False:表示しない</returns>
    Public Function WebApiAssetLending(params As List(Of DtoAssetLending)) As ClsWebApiClient.EnResultAPI
        Try
            Dim ProcesCount As Integer = 0
            Dim ProcDate As Date = Now
            Dim C_WebApiClient As New ClsWebApiClient
            C_WebApiClient.p_WebServerURL = Me._sWebApiUrl     'WEB API 接続URL
            Dim ArayAssetNo() As String = Nothing


            For LopCnt As Integer = 0 To params.Count - 1
                'パラメータセット
                C_WebApiClient.InitEntryProcessData()   'バッファ初期化
                With C_WebApiClient.p_tempEntryProcessData
                    ReDim Preserve ArayAssetNo(ProcesCount)
                    ArayAssetNo(ProcesCount) = params(LopCnt).AssetNo
                    ProcesCount += 1

                    .processLogId = ProcesCount.ToString                    '処理履歴ID
                    .processDt = ProcDate.ToString("yyyyMMdd")              '処理日
                    '2021/11/05 upd nv Inoue start
                    .processTm = ProcDate.ToString("HH:mm:ss") & "000"      '処理時刻
                    .processTm = ProcDate.ToString("HHmmss") & "000"        '処理時刻
                    '2021/11/05 upd nv Inoue end

                    .assetNo = params(LopCnt).AssetNo                       '資産番号
                    .assetId = params(LopCnt).AssetID                       '資産ID
                    .processToolDiv = params(LopCnt).ProcessToolDiv         '処理ツール区分
                    .processDiv = params(LopCnt).ProcessDiv                 '処理区分
                    .leasePlaceCd = params(LopCnt).LeasePlaceCd             '貸出先場所コード
                    .leaseEmployeeCd = params(LopCnt).LeaseEmployeeCd       '貸出先担当者社員コード
                    .leaseDueDt = params(LopCnt).LeaseDueDt                 '貸出返却予定日
                    .locLCd = ""                                            'ロケ大区分コード
                    .locMCd = ""                                            'ロケ中区分コード
                    .locSCd = ""                                            'ロケ小区分コード
                    .employeeCd = params(LopCnt).EmployeeCd                 'ログイン担当者コード                                                                           
                    .remark = ""
                End With

                C_WebApiClient.AddEntryProcessData()    'バッファ→配列追加
            Next LopCnt

            'If ProcesCount <= 0 Then   '一件もない場合はありえないのでコメント
            '    clsSV_MSG.SVSubMsgClear(prmlblMsg)
            '    Return False
            'End If


            '登録送信
            'Dim Result As ClsWebApiClient.EnResultAPI = C_WebApiClient.fncEntryProcesSend(CommonConst.PROCESS_DIV_BO, ProcesCount, ArayAssetNo, _SelectedParamator.p_EventLogVal)                      '20200310 Del Sasaki V1.2 貸出上限
            Dim Result As ClsWebApiClient.EnResultAPI = C_WebApiClient.fncEntryProcesSend(params(0).ProcessDiv, ProcesCount, ArayAssetNo)                '20200310 Add Sasaki V1.2 貸出上限
            Return Result

            'Select Case Result
            '    Case ClsWebApiClient.EnResultAPI.OK_Result
            '        '正常応答
            '        clsSV_MSG.SVSubMsgClear(prmlblMsg)
            '        Return True

            '    Case ClsWebApiClient.EnResultAPI.NoAnswer   '応答なし
            '        'WebAPIの応答がありません。
            '        C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990017", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)

            ''Case ClsWebApiClient.EnResultAPI.DB_Error   'DB異常応答
            ''    '8000,WRN,YELLOW,BLACK,%1
            ''    Call clsSV_MSG.SVSubMsgDisp(prmlblMsg, 8000, "WebAPIからデータベース異常を受信しました。")

            '    Case ClsWebApiClient.EnResultAPI.Old_Naster 'マスタ古い応答
            '        'マスタが最新ではありません。マスタを再取り込みしてください。
            '        C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990018", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
            ''20200309 Add Start Sasaki V1.2 貸出上限
            '    Case ClsWebApiClient.EnResultAPI.LEASE_UPPER_ERR    '貸出上限エラー
            '        '正常応答
            '        _ConfirmMessage = "990020"
            '        _LendLimitError = True
            '        Return True
            '    Case ClsWebApiClient.EnResultAPI.LEASE_UPPER_WARN    '貸出上限警告
            '        '正常応答
            '        _ConfirmMessage = "990021"
            '        Return True
            '    '20200309 Add End Sasaki V1.2 貸出上限
            '    '20211005 Add Start v2.0 予約該当なし
            '    Case ClsWebApiClient.EnResultAPI.ReserveErr
            '        '予約されていないため貸出できなかった資産があります。
            '        C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "030009", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
            '        '20211005 Add End v2.0 予約該当なし
            '    Case Else 'ClsWebApiClient.EnResultAPI.Other_Error  'その他異常
            '        'WebAPIから異常応答がありました。
            '        C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990019", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
            'End Select

            'Return False    '異常応答 応答なし

        Catch ex As Exception
            logger.Error(ex.Message)
            Return Nothing

            'Call SVSubWriteLog(EN_LogType.Err, CLASS_NAME, funcName, LOG_ERR_CATHCH_TITLE, ex.Message)

            ''WebAPIから異常応答がありました。
            'C_LanguageWord.subMsgDisp(prmlblMsg, CommonConst.WORD_DIV_M, "990019", ClsLanguageWord.EnLabelColor.YELLOW, ClsLanguageWord.EnLabelColor.BLACK)
        End Try
    End Function
End Class
