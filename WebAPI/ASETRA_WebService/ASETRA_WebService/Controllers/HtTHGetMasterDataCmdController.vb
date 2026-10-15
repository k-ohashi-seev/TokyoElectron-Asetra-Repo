Imports System.Data.SqlClient
Imports System.Net
Imports System.Web.Http
Imports System.Web.Http.Cors

Namespace Controllers
    ''' <summary>
    ''' マスタデータを返すコントローラー
    ''' </summary>
    Public Class HtTHGetMasterDataCmdController
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
        ''' <remarks>api/HtTHGetMasterDataCmd呼び出し</remarks>
        <HttpPost>
        <EnableCors("*", "*", "*")>
        Public Function Post(<FromBody> ByVal value As HtTHGetMasterDataCmdReqParameter) As HtTHGetMasterDataCmdResParameter
            Dim ret As HtTHGetMasterDataCmdResParameter = New HtTHGetMasterDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.OK)}
            logger.Info($"Input：{Newtonsoft.Json.JsonConvert.SerializeObject(value)}")
            If Not ModelState.IsValid Then
                '送信情報が不正
                Return New HtTHGetMasterDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .message = CommonConst.MSG_INV_ARG}
            End If

            Dim db As New SystemDBContext

            If db.p_DataBaseOpenFlag = False Then
                'DB接続異常
                Return New HtTHGetMasterDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .message = CommonConst.MSG_ERR_DB}
            End If

            Try
                'マスタデータ受信呼び出し
                Dim param = New HtGetMasterDataCmdReqParameter() With {
                    .masterDataList = {
                        New GetMasterDataDetail With {.masterName = "M_ASSET"},
                        New GetMasterDataDetail With {.masterName = "M_EMPLOYEE"},
                        New GetMasterDataDetail With {.masterName = "M_EMPLOYEE_COMMON_HUB"},
                        New GetMasterDataDetail With {.masterName = "M_SETTING"},
                        New GetMasterDataDetail With {.masterName = "M_AUTH"}
                    }
                }
                Dim mstData = New HtGetMasterDataCmdController()
                Dim retMstData = mstData.Post(param)

                If Not retMstData.returnCode = ReturnCodeEnum.OK Then
                    ret.returnCode = retMstData.returnCode
                    ret.message = CommonFunction.GetMessage(db, retMstData.wordCd)
                Else
                    '値をセット
                    ret.adl = CopyAssetData(retMstData.adl)
                    ret.employeeDataList = retMstData.employeeDataList
                    ret.settingDataList = retMstData.settingDataList
                    ret.authDataList = retMstData.authDataList
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
        ''' 資産マスタ情報の返却データを作成
        ''' </summary>
        ''' <param name="adlList">マスタデータ受信の取得データ</param>
        Private Function CopyAssetData(ByVal adlList As AssetData()) As HtTHGetMasterDataCmdResParameter.AssetData()

            Dim tempAdlList = New List(Of HtTHGetMasterDataCmdResParameter.AssetData)
            Dim retAdl As HtTHGetMasterDataCmdResParameter.AssetData

            For Each adl As AssetData In adlList
                retAdl = New HtTHGetMasterDataCmdResParameter.AssetData With {
                    .no = adl.no,
                    .nm = adl.nm,
                    .mn = adl.mn,
                    .llcd = adl.llcd,
                    .st = adl.st,
                    .ls = adl.ls,
                    .mll = adl.mll
                }

                tempAdlList.Add(retAdl)
            Next

            Return tempAdlList.ToArray()
        End Function
#End Region

    End Class
End Namespace