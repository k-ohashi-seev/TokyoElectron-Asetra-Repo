Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 予約状況照会用予約情報表示用モデル
    ''' </summary>
    Partial Public Class LendingStatusReserveInfoModel
        Inherits BaseModel

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        ''' <returns>資産番号</returns>
        Public Property ASSET_NO As String

        ''' <summary>
        ''' 予約ID
        ''' </summary>
        ''' <returns>予約ID</returns>
        Public Property RESERVE_ID As String               '2022/09/22 sv tomatsuri 型変更

        ''' <summary>
        ''' メンテナンスID
        ''' </summary>
        ''' <returns>メンテナンスID</returns>
        Public Property MAINTE_ID As String               '2022/09/22 sv tomatsuri 型変更

        ''' <summary>
        ''' 予約ステータス
        ''' </summary>
        ''' <returns>予約ステータス</returns>
        Public Property RESERVE_STATUS As String = String.Empty

        ''' <summary>
        ''' 資産貸出状態
        ''' </summary>
        ''' <returns>資産貸出状態</returns>
        Public Property RSVFLG As String = String.Empty

        ''' <summary>
        ''' 担当者名
        ''' </summary>
        ''' <returns></returns>
        Public Property CHARGE_EMP_NAME As String = String.Empty

        ''' <summary>
        ''' 申請者名
        ''' </summary>
        ''' <returns></returns>
        Public Property REQ_EMP_NAME As String = String.Empty

        ''' <summary>
        ''' 工事番号
        ''' </summary>
        ''' <returns></returns>
        Public Property CONST_NO As String = String.Empty

        ''' <summary>
        ''' 工事日
        ''' </summary>
        ''' <returns></returns>
        Public Property CONST_DT As String = String.Empty

        ''' <summary>
        ''' 工事内容
        ''' </summary>
        ''' <returns></returns>
        Public Property CONST_CONTENTS As String = String.Empty

        ''' <summary>
        ''' 工事責任者氏名
        ''' </summary>
        ''' <returns></returns>
        Public Property CONST_EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' 更新可か否か
        ''' </summary>
        ''' <returns>更新可か否か</returns>
        Public Property IS_UPD As Boolean = False

    End Class
End Namespace
