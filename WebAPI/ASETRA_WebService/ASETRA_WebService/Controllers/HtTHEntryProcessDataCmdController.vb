Imports System.Data.SqlClient
Imports System.Net
Imports System.Web.Http
Imports System.Web.Http.Cors

Namespace Controllers
    ''' <summary>
    ''' 処理データを登録するコントローラー
    ''' </summary>
    Public Class HtTHEntryProcessDataCmdController
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
        Public Function Post(<FromBody> ByVal value As HtTHEntryProcessDataCmdReqParameter) As CommonResParameterTH
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
                '照合機能データ登録呼び出し
                Dim param = GetEntryMatchProcDataParam(value)
                Dim entryMatchProcData = New HtEntryMatchProcessDataCmdController()
                Dim retEntryMatchProcData = entryMatchProcData.Post(param)

                If Not retEntryMatchProcData.returnCode = ReturnCodeEnum.OK Then
                    ret.returnCode = retEntryMatchProcData.returnCode
                    ret.message = String.Format(CommonFunction.GetMessage(db, retEntryMatchProcData.wordCd), "", retEntryMatchProcData.outErrorList(0).rep1, retEntryMatchProcData.outErrorList(0).rep2)
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

#Region "Private メソッド"
        ''' <summary>
        ''' 照合機能データ登録の呼び出し用パラメータを作成
        ''' </summary>
        ''' <param name="thMatchList">マスタデータ受信の取得データ</param>
        Private Function GetEntryMatchProcDataParam(ByVal thMatchList As HtTHEntryProcessDataCmdReqParameter) As HtEntryMatchProcessDataCmdReqParameter

            Dim tempMatchList = New List(Of EntryMatchProcessDataDetail)
            Dim retMatchProcessDataDetail As EntryMatchProcessDataDetail
            Dim retLentAssetDetailList As List(Of EntryLentAssetListDetail)
            Dim retRetAssetDetailList As List(Of EntryRetAssetListDetail)
            Dim retLentAssetDetail As EntryLentAssetListDetail
            Dim retRetAssetDetail As EntryRetAssetListDetail

            '照合リスト
            For Each match As THEntryProcessDataDetail In thMatchList.matchList
                retLentAssetDetailList = New List(Of EntryLentAssetListDetail)
                retRetAssetDetailList = New List(Of EntryRetAssetListDetail)

                retMatchProcessDataDetail = New EntryMatchProcessDataDetail With {
                    .processLogId = match.processLogId,
                    .processDt = match.processDt,
                    .processTm = match.processTm,
                    .employeeCd = match.employeeCd,
                    .processDiv = match.processDiv,
                    .constNo = match.contNo,
                    .leaseEmployeeCd = match.leaseEmployeeCd,
                    .leaseDueDt = match.leaseDueDt
                }

                '貸出リスト
                If match.lentAssetList IsNot Nothing Then
                    For Each lentAsset As THEntryLentAssetListDetail In match.lentAssetList
                        retLentAssetDetail = New EntryLentAssetListDetail With {
                        .assetNo = lentAsset.assetNo,
                        .leaseDt = match.leaseDt,
                        .remark = match.remark
                    }
                        retLentAssetDetailList.Add(retLentAssetDetail)
                    Next
                    retMatchProcessDataDetail.lentAssetList = retLentAssetDetailList.ToArray()
                End If


                '返却リスト
                If match.retAssetList IsNot Nothing Then
                    For Each retAsset As THEntryRetAssetListDetail In match.retAssetList
                        retRetAssetDetail = New EntryRetAssetListDetail With {
                            .assetNo = retAsset.assetNo,
                            .contMatchDiv = CommonConst.CONST_MATCH_DIV.MI,
                            .inspeFlag = retAsset.InspeFlg
                        }
                        retRetAssetDetailList.Add(retRetAssetDetail)
                    Next
                    retMatchProcessDataDetail.retAssetList = retRetAssetDetailList.ToArray()
                End If

                tempMatchList.Add(retMatchProcessDataDetail)
            Next


            Return New HtEntryMatchProcessDataCmdReqParameter() With {
                    .matchProcessDataList = tempMatchList.ToArray()
                }
        End Function
#End Region

    End Class
End Namespace