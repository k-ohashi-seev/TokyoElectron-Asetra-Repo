Imports System.IO
Imports System.Text
Imports System.Net
Imports System.Runtime.Serialization.Json
Imports System.Reflection.MethodBase

''' <summary>
''' ユーザ名      ：ＳＡＴＯ
''' システム名    ：ＲＦＩＤ資産管理システム
''' サブシステム名：Ｃ／Ｓ画面
''' プログラム名  ：Ｃ／Ｓ画面
''' クラス名      ：WEB API 通信クラス
''' 作成日        ：2019/08/19  Ver.1.0.0.0
''' 修正日        ：0000/00/00  Ver.0.0.0.0
''' </summary>
''' <remarks></remarks>
Public Class ClsWebApiClient

#Region "定数定義"
    ''' <summary>クラス名</summary>
    Private Const CLASS_NAME As String = "ClsWebApiClient"

    ''' <summary>処理データ登録コマンド</summary>
    Private Const WEB_CMD_ENTRY_PROC As String = "HtEntryProcessDataCmd"

    ''' <summary>処理データ登録コマンド</summary>  '2021/11/18 nv miyaji add
    Private Const WEB_CMD_INVENT_PROC As String = "HtEntryInventDataCmd"

    ''' <summary>応答定義</summary>
    Public Enum EnResultAPI
        ''' <summary>正常応答</summary>
        OK_Result = 0
        ''' <summary>応答なし</summary>
        NoAnswer = -1
        '''' <summary>データベース異常応答</summary>  'マスタ古い以外は 全て異常応答
        'DB_Error = -2
        ''' <summary>マスタ古い応答</summary>
        Old_Naster = -3
        ''' <summary>タグ紐付エラー</summary>
        WARN_TAG = -4
        ''' <summary>貸出上限エラー</summary>
        LEASE_UPPER_ERR = -5                    '20200309 Add Sasaki V1.2 貸出上限
        ''' <summary>貸出上限警告</summary>
        LEASE_UPPER_WARN = -6                   '20200309 Add Sasaki V1.2 貸出上限
        ''' <summary>その他異常応答</summary>
        Other_Error = -7                        '20200309 Upd Sasaki V1.2 貸出上限
        'Other_Error = -5                       '20200309 Upd Sasaki V1.2 貸出上限
        '''<summary>予約該当なし</summary>
        ReserveErr = -8

    End Enum
#End Region

#Region "変数宣言"
    ''' <summary>送信データ 処理データ登録用</summary>
    Private _sendEntryProc As New HtEntryProcessDataCmdReqParameter
    ''' <summary>送信データ 実棚情報登録登録用</summary>  '2021/11/18 nv miyaji add
    Private _sendInventProc As New HtEntryInventDataCmdReqParameter
#End Region

#Region "プロパティ"
    ''' <summary>接続ＵＲＬ</summary>
    Private _WebServerURL As String = ""
    ''' <summary>接続ＵＲＬ</summary>
    Public WriteOnly Property p_WebServerURL As String
        Set(value As String)
            If value.EndsWith("/") = False Then
                value &= "/"    '末尾区切り なければ付加
            End If

            _WebServerURL = value
        End Set
    End Property

    ''' <summary>処理データ登録 代入バッファ</summary>
    Private _tempEntryProcessData As New EntryProcessDataDetail
    ''' <summary>処理データ登録 代入バッファ</summary>
    Public ReadOnly Property p_tempEntryProcessData As EntryProcessDataDetail
        Get
            Return _tempEntryProcessData
        End Get
    End Property

    ''' <summary>実棚情報登録 代入バッファ</summary>
    Private _tempEntryInventData As New EntryInventDataDetail
    ''' <summary>実棚情報登録 代入バッファ</summary>
    Public ReadOnly Property p_tempEntryInventData As EntryInventDataDetail
        Get
            Return _tempEntryInventData
        End Get
    End Property

    ''' <summary>ワーニング応答 重複資産番号</summary>
    Private _WarnDuplicateAssetNo As String = ""
    ''' <summary>ワーニング応答 重複資産番号</summary>
    Public ReadOnly Property p_WarnDuplicateAssetNo As String
        Get
            Return _WarnDuplicateAssetNo
        End Get
    End Property
#End Region

#Region "処理データ登録 データ代入"
    ''' <summary>
    ''' 処理データ登録 データ初期化
    ''' </summary>
    ''' <remarks>初期化後、p_tempEntryProcessData プロパティに個別データを代入する</remarks>
    Public Sub InitEntryProcessData()
        On Error Resume Next
        _tempEntryProcessData = New EntryProcessDataDetail
    End Sub

    ''' <summary>
    ''' 処理データ登録 データ配列追加
    ''' </summary>
    ''' <remarks>初期化・個別データ代入後、追加を実行する</remarks>
    Public Sub AddEntryProcessData()
        On Error Resume Next

        Dim NewLength As Integer = If(_sendEntryProc.processDataList Is Nothing, 0, _sendEntryProc.processDataList.Length)
        ReDim Preserve _sendEntryProc.processDataList(NewLength)
        _sendEntryProc.processDataList(NewLength) = _tempEntryProcessData
    End Sub
#End Region

#Region "処理データ登録 コマンド送信"
    ''' <summary>
    ''' 処理データ登録 コマンド送信
    ''' </summary>
    ''' <param name="prmProcesDiv">処理区分</param>
    ''' <param name="prmDataCount">データ件数</param>
    ''' <param name="prmArayAssetNo">資産番号配列</param>
    ''' <param name="prmAuxLogval">補助情報</param>
    ''' <param name="prmAssetList">資産番号リスト</param>
    ''' <returns></returns>
    Public Function fncEntryProcesSend(ByVal prmProcesDiv As String,
                                       ByVal prmDataCount As Integer,
                                       ByVal prmArayAssetNo() As String,
                                       Optional ByVal prmAuxLogval As String = "",
                                       Optional ByRef prmAssetReserveList() As AssetReserveError = Nothing,
                                       Optional ByRef prmAssetList() As AssetLeaseError = Nothing) As EnResultAPI '20200310 Add Sasaki V1.2 貸出上限
        'Public Function fncEntryProcesSend(ByVal prmProcesDiv As String, ByVal prmDataCount As Integer, ByVal prmArayAssetNo() As String, Optional ByVal prmAuxLogval As String = "") As EnResultAPI       '20200310 Del Sasaki V1.2 貸出上限
        Dim funcName As String = GetCurrentMethod().Name
        fncEntryProcesSend = EnResultAPI.NoAnswer
        Try
            _WarnDuplicateAssetNo = ""

            '一旦、コメントアウト　nv Inoue  2021/11/03
            'Call SVSubWriteLog(EN_LogType.Evt, CLASS_NAME, funcName, "処理データ登録 コマンド送信", "処理区分:" & prmProcesDiv & " データ件数=" & prmDataCount.ToString & " 対象資産番号(" & Join(prmArayAssetNo, ",") & ") " & prmAuxLogval)

            'WebAPI 通信共通クラス 接続ＵＲＬ＋コマンド
            Dim C_WebClientCom As New ClsWebClientCom(_WebServerURL & WEB_CMD_ENTRY_PROC)

            'JSONデータシリアライズ用
            Dim jsonSerializer As DataContractJsonSerializer = Nothing
            jsonSerializer = New DataContractJsonSerializer(GetType(HtEntryProcessDataCmdReqParameter))

            '要求データ書き込み シリアライズ化(JSON)
            Using _ReqStream As Stream = C_WebClientCom.p_WebRequest.GetRequestStream
                jsonSerializer.WriteObject(_ReqStream, _sendEntryProc)
            End Using

            'リソースからの応答取得
            If C_WebClientCom.fncWabApiSendAnswer() = False Then
                '応答なし
                'C_WebClientCom.p_ErrorMessage
                Return EnResultAPI.NoAnswer
            End If

            '受信データ(JSON)をデシリアライズ
            Dim ReciveValue As HtEntryProcessDataCmdResParameter
            jsonSerializer = New DataContractJsonSerializer(GetType(HtEntryProcessDataCmdResParameter))
            Using _DecStream As New MemoryStream(Encoding.UTF8.GetBytes(C_WebClientCom.p_ResultBody))
                ReciveValue = DirectCast(jsonSerializer.ReadObject(_DecStream), HtEntryProcessDataCmdResParameter)
            End Using

            '一旦、コメントアウト　nv Inoue  2021/11/03
            'Call SVSubWriteLog(EN_LogType.Evt, CLASS_NAME, funcName, "処理データ登録 コマンド応答", "戻り値:" & ReciveValue.returnCode.ToString & " ワーニング:" & ReciveValue.wordCd)

            Select Case ReciveValue.returnCode
                Case ReturnCodeEnum.OK
                    Return EnResultAPI.OK_Result    '正常応答

                Case ReturnCodeEnum.NG
                    '貸出対象エラーリスト
                    If ReciveValue.reserveList IsNot Nothing AndAlso ReciveValue.reserveList.Length > 0 Then
                        prmAssetReserveList = ReciveValue.reserveList
                        Return EnResultAPI.ReserveErr
                    End If
                    '貸出エラーリスト
                    If ReciveValue.assetLeaseList IsNot Nothing AndAlso ReciveValue.assetLeaseList.Length > 0 Then
                        '貸出上限警告有(未処理)
                        prmAssetList = ReciveValue.assetLeaseList
                        Return EnResultAPI.LEASE_UPPER_ERR
                    End If

                    Return EnResultAPI.ReserveErr    '予約エラー

                Case ReturnCodeEnum.WARN
                    '対象登録タグ紐付エラー
                    If ReciveValue.assetTagList IsNot Nothing AndAlso ReciveValue.assetTagList.Length > 0 Then
                        'タグ紐付エラー 重複データ保存
                        _WarnDuplicateAssetNo = ReciveValue.assetTagList(0).dupAssetNo
                        Return EnResultAPI.WARN_TAG
                    End If
                    '貸出エラーリスト
                    If ReciveValue.assetLeaseList IsNot Nothing AndAlso ReciveValue.assetLeaseList.Length > 0 Then
                        '貸出上限警告有(未処理)
                        prmAssetList = ReciveValue.assetLeaseList
                        Return EnResultAPI.LEASE_UPPER_WARN
                    End If

                    Return EnResultAPI.OK_Result    '正常応答

                Case Else
                    '異常応答

                    '一旦、コメントアウト　nv Inoue  2021/11/03 start
                    'Select Case ReciveValue.wordCd
                    '    Case CommonWord.MSG_OLD_MST
                    '        Return EnResultAPI.Old_Naster   'マスタ古い応答
                    '    Case Else
                    '        Return EnResultAPI.Other_Error  'その他異常応答
                    'End Select
                    Return EnResultAPI.Other_Error  'その他異常応答
                    '一旦、コメントアウト　nv Inoue  2021/11/03 end

            End Select

            'Case ReturnCodeEnum.WARN_TAG        '対象登録タグ紐付エラー
            '    If ReciveValue.assetTagList.Length > 0 Then
            '        'タグ紐付エラー 重複データ保存
            '        _WarnDuplicateAssetNo = ReciveValue.assetTagList(0).dupAssetNo
            '    End If
            '    Return EnResultAPI.WARN_TAG

            ''20200309 Add Start Sasaki V1.2 貸出上限
            'Case ReturnCodeEnum.LEASE_UPPER_ERR
            '    prmAssetList = ReciveValue.assetLeaseList
            '    Return EnResultAPI.LEASE_UPPER_ERR

            'Case ReturnCodeEnum.LEASE_UPPER_WARN
            '    prmAssetList = ReciveValue.assetLeaseList
            '    Return EnResultAPI.LEASE_UPPER_WARN
            '    '20200309 Add End Sasaki V1.2 貸出上限

            'Case Else 'ReturnCodeEnum.NG, ReturnCodeEnum.WARN
            '    '異常応答
            '    Select Case ReciveValue.wordCd
            '        'Case CommonWord.MSG_ERR_DB
            '        '    Return EnResultAPI.DB_Error     'データベース異常応答
            '        Case CommonWord.MSG_OLD_MST
            '            Return EnResultAPI.Old_Naster   'マスタ古い応答
            '        Case Else
            '            Return EnResultAPI.Other_Error  'その他異常応答
            '    End Select

        Catch ex As Exception
            '一旦、コメントアウト　nv Inoue  2021/11/03
            'Call SVSubWriteLog(EN_LogType.Err, CLASS_NAME, funcName, LOG_ERR_CATHCH_TITLE, ex.Message)
        End Try
    End Function
#End Region

#Region "実棚情報登録 データ代入"  '2021/11/18 nv miyaji add
    ''' <summary>
    ''' 実棚情報登録 データ初期化
    ''' </summary>
    ''' <remarks>初期化後、p_tempEntryProcessData プロパティに個別データを代入する</remarks>
    Public Sub InitEntryInventData()
        On Error Resume Next
        _tempEntryInventData = New EntryInventDataDetail
    End Sub

    ''' <summary>
    ''' 実棚情報登録 データ配列追加
    ''' </summary>
    ''' <remarks>初期化・個別データ代入後、追加を実行する</remarks>
    Public Sub AddEntryInventData()
        On Error Resume Next

        Dim NewLength As Integer = If(_sendInventProc.ie Is Nothing, 0, _sendInventProc.ie.Length)
        ReDim Preserve _sendInventProc.ie(NewLength)
        _sendInventProc.ie(NewLength) = _tempEntryInventData
    End Sub
#End Region

#Region "実棚情報登録 コマンド送信"  '2021/11/18 nv miyaji add
    ''' <summary>
    ''' 実棚情報登録 コマンド送信
    ''' </summary>
    ''' <returns></returns>
    Public Function fncEntryInventSend() As EnResultAPI
        Dim funcName As String = GetCurrentMethod().Name
        fncEntryInventSend = EnResultAPI.NoAnswer
        Try
            '一旦、コメントアウト　nv Inoue  2021/11/03
            'Call SVSubWriteLog(EN_LogType.Evt, CLASS_NAME, funcName, "処理データ登録 コマンド送信", "処理区分:" & prmProcesDiv & " データ件数=" & prmDataCount.ToString & " 対象資産番号(" & Join(prmArayAssetNo, ",") & ") " & prmAuxLogval)

            'WebAPI 通信共通クラス 接続ＵＲＬ＋コマンド
            Dim C_WebClientCom As New ClsWebClientCom(_WebServerURL & WEB_CMD_INVENT_PROC)

            'JSONデータシリアライズ用
            Dim jsonSerializer As DataContractJsonSerializer = Nothing
            jsonSerializer = New DataContractJsonSerializer(GetType(HtEntryInventDataCmdReqParameter))

            '要求データ書き込み シリアライズ化(JSON)
            Using _ReqStream As Stream = C_WebClientCom.p_WebRequest.GetRequestStream
                _sendInventProc.td = "W"    '2021/11/18 nv miyaji add 「W」固定
                jsonSerializer.WriteObject(_ReqStream, _sendInventProc)
            End Using

            'リソースからの応答取得
            If C_WebClientCom.fncWabApiSendAnswer() = False Then
                '応答なし
                'C_WebClientCom.p_ErrorMessage
                Return EnResultAPI.NoAnswer
            End If

            '受信データ(JSON)をデシリアライズ
            Dim ReciveValue As CommonResParameter
            jsonSerializer = New DataContractJsonSerializer(GetType(CommonResParameter))
            Using _DecStream As New MemoryStream(Encoding.UTF8.GetBytes(C_WebClientCom.p_ResultBody))
                ReciveValue = DirectCast(jsonSerializer.ReadObject(_DecStream), CommonResParameter)
            End Using

            '一旦、コメントアウト　nv Inoue  2021/11/03
            'Call SVSubWriteLog(EN_LogType.Evt, CLASS_NAME, funcName, "処理データ登録 コマンド応答", "戻り値:" & ReciveValue.returnCode.ToString & " ワーニング:" & ReciveValue.wordCd)

            Select Case ReciveValue.returnCode
                Case ReturnCodeEnum.OK, ReturnCodeEnum.WARN
                    Return EnResultAPI.OK_Result    '正常応答
                Case Else
                    '異常応答
                    Return EnResultAPI.Other_Error  'その他異常応答

            End Select

        Catch ex As Exception
            '一旦、コメントアウト　nv Inoue  2021/11/03
            'Call SVSubWriteLog(EN_LogType.Err, CLASS_NAME, funcName, LOG_ERR_CATHCH_TITLE, ex.Message)
        End Try
    End Function
#End Region

    '対応コマンドが増える場合は、この部分に記述する


#Region "WebAPI 通信共通クラス"
    ''' <summary>WebAPI 通信共通クラス</summary>
    Private Class ClsWebClientCom

#Region "定数宣言"

#End Region

#Region "プロパティ"
        ''' <summary>Webリクエスト</summary>
        Private _WebRequest As HttpWebRequest
        ''' <summary>Webリクエスト</summary>
        Public ReadOnly Property p_WebRequest As HttpWebRequest
            Get
                Return _WebRequest
            End Get
        End Property

        ''' <summary>応答本文</summary>
        Private _ResultBody As String = ""
        ''' <summary>応答本文</summary>
        Public ReadOnly Property p_ResultBody As String
            Get
                Return _ResultBody
            End Get
        End Property

        ''' <summary>エラーメッセージ</summary>
        Private _ErrorMessage As String = ""
        ''' <summary>エラーメッセージ</summary>
        Public ReadOnly Property p_ErrorMessage As String
            Get
                Return _ErrorMessage
            End Get
        End Property
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="prmSendURL">送信URL</param>
        Public Sub New(ByVal prmSendURL As String)
            On Error Resume Next

            'HttpWebRequestインスタンス初期化
            _WebRequest = CType(WebRequest.Create(prmSendURL), HttpWebRequest)
            'POST送信
            _WebRequest.Method = "POST"
            '認証情報
            _WebRequest.Credentials = CredentialCache.DefaultCredentials
            'Accept HTTPヘッダー
            _WebRequest.Accept = "application/json"
            'ContentType HTTPヘッダー
            _WebRequest.ContentType = "application/json"
        End Sub
#End Region

#Region "ＷＥＢ ＡＰＩ通信実行"
        ''' <summary>
        ''' ＷＥＢ ＡＰＩ通信実行
        ''' </summary>
        ''' <returns></returns>
        Public Function fncWabApiSendAnswer() As Boolean
            Dim funcName As String = GetCurrentMethod().Name
            fncWabApiSendAnswer = False
            Try
                '一旦、コメントアウト　nv Inoue  2021/11/03
                'Call SVSubWriteLog(EN_LogType.Evt, CLASS_NAME, funcName, "WEB API 通信開始", "URL:" & _WebRequest.RequestUri.ToString)

                'リソースからの応答取得
                Dim _WebResp As HttpWebResponse = CType(_WebRequest.GetResponse, HttpWebResponse)

                '応答本文読み取り
                _ResultBody = ""
                Using _ResStream As Stream = _WebResp.GetResponseStream
                    Using _reader As New StreamReader(_ResStream)
                        _ResultBody = _reader.ReadToEnd
                    End Using
                End Using

                '一旦、コメントアウト　nv Inoue  2021/11/03
                'Call SVSubWriteLog(EN_LogType.Evt, CLASS_NAME, funcName, "WEB API 通信完了", "レスポンス:" & _ResultBody)
                Return True

            Catch ex As Exception
                _ErrorMessage = ex.Message

                '一旦、コメントアウト　nv Inoue  2021/11/03
                'Call SVSubWriteLog(EN_LogType.Err, CLASS_NAME, funcName, "WEB API 通信失敗", ex.Message)
            End Try
        End Function
#End Region

    End Class
#End Region

End Class
