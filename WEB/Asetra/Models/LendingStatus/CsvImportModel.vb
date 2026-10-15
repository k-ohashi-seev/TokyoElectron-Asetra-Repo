Namespace Models
    ''' <summary>
    ''' 予約照会一覧のCSV取込用モデル
    ''' </summary>
    Public Class ReserveCsvImportModel

        ''' <summary>
        ''' 資産情報クラス
        ''' </summary>
        Public Class AssetInfo
            Public ASSET_NO As String = String.Empty
            Public MNG_LOC_L_CD As String = String.Empty
        End Class

        ''' <summary>
        ''' 処理区分
        ''' </summary>
        ''' <returns></returns>
        Public Property PROCESS_DIV As String = String.Empty

        ''' <summary>
        ''' 予約番号
        ''' </summary>
        ''' <returns></returns>
        Public Property RESERVE_ID As String = String.Empty

        ''' <summary>
        ''' 申請日
        ''' </summary>
        ''' <returns></returns>
        Public Property REQUEST_DT As String = String.Empty

        ''' <summary>
        ''' 申請社員コード
        ''' </summary>
        ''' <returns></returns>
        Public Property REQ_EMP_CD As String = String.Empty

        ''' <summary>
        ''' 担当社員コード
        ''' </summary>
        ''' <returns></returns>
        Public Property CHARGE_EMP_CD As String = String.Empty

        ''' <summary>
        ''' 実貸出開始日
        ''' </summary>
        ''' <returns></returns>
        Public Property REAL_START_DT As String = String.Empty

        ''' <summary>
        ''' 実貸出終了日
        ''' </summary>
        ''' <returns></returns>
        Public Property REAL_END_DT As String = String.Empty

        ''' <summary>
        ''' 貸出先コード
        ''' </summary>
        ''' <returns></returns>
        Public Property LEASE_PLACE_CD As String = String.Empty

        ''' <summary>
        ''' 貸出用途
        ''' </summary>
        ''' <returns></returns>
        Public Property LEASE_USE As String = String.Empty

        ''' <summary>
        ''' 送り先名1
        ''' </summary>
        ''' <returns></returns>
        Public Property DEST_NAME1 As String = String.Empty

        ''' <summary>
        ''' 送り先名2
        ''' </summary>
        ''' <returns></returns>
        Public Property DEST_NAME2 As String = String.Empty

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' 貸出確認書宛名
        ''' </summary>
        ''' <returns></returns>
        Public Property TO_DEST_NAME As String = String.Empty
        '2023/03/22 Kawamoto ADD END

        ''' <summary>
        ''' 送り先住所1
        ''' </summary>
        ''' <returns></returns>
        Public Property DEST_ADDRESS1 As String = String.Empty

        ''' <summary>
        ''' 送り先住所2
        ''' </summary>
        ''' <returns></returns>
        Public Property DEST_ADDRESS2 As String = String.Empty

        ''' <summary>
        ''' 送り先住所3
        ''' </summary>
        ''' <returns></returns>
        Public Property DEST_ADDRESS3 As String = String.Empty

        ''' <summary>
        ''' 送り先電話番号
        ''' </summary>
        ''' <returns></returns>
        Public Property DEST_PHONE_NUM As String = String.Empty

        ''' <summary>
        ''' 送り先郵便番号
        ''' </summary>
        ''' <returns></returns>
        Public Property DEST_ZIP_CODE As String = String.Empty

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' 返送先(貸出先コード)
        ''' </summary>
        ''' <returns></returns>
        Public Property RET_LEASE_PLACE_CD As String = String.Empty

        ''' <summary>
        ''' 返送先住所1
        ''' </summary>
        ''' <returns></returns>
        Public Property RET_ADDRESS1 As String = String.Empty

        ''' <summary>
        ''' 返送先住所2
        ''' </summary>
        ''' <returns></returns>
        Public Property RET_ADDRESS2 As String = String.Empty

        ''' <summary>
        ''' 返送先住所3
        ''' </summary>
        ''' <returns></returns>
        Public Property RET_ADDRESS3 As String = String.Empty

        ''' <summary>
        ''' 返送先郵便番号
        ''' </summary>
        ''' <returns></returns>
        Public Property RET_ZIP_CODE As String = String.Empty
        '2023/03/22 Kawamoto ADD END

        ''' <summary>
        ''' 備考
        ''' </summary>
        ''' <returns></returns>
        Public Property REMARK As String = String.Empty

        ''' <summary>
        ''' 社内備考
        ''' </summary>
        ''' <returns></returns>
        Public Property REMARK_INTERNAL As String = String.Empty

        ''' <summary>
        ''' 送り状番号
        ''' </summary>
        ''' <returns></returns>
        Public Property DEST_NO As String = String.Empty

        ''' <summary>
        ''' 資産情報リスト
        ''' </summary>
        ''' <returns></returns>
        Public Property ASSET_INFO_LIST As New List(Of AssetInfo)

        ''' <summary>
        ''' 予約明細情報
        ''' </summary>
        ''' <returns></returns>
        Public Property RESERVE_DETAIL As New List(Of ReserveDetailViewModel)

        ''' <summary>
        ''' ユーザー貸出開始日
        ''' </summary>
        ''' <returns></returns>
        Public Property USER_START_DT As String = String.Empty

        ''' <summary>
        ''' ユーザー貸出終了日
        ''' </summary>
        ''' <returns></returns>
        Public Property USER_END_DT As String = String.Empty
    End Class
End Namespace
