Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 予約照会一覧CSV出力用モデル
    ''' </summary>
    Public Class LendingRefListCsvModel

        ''' <summary>
        ''' カテゴリー大分類コード
        ''' </summary>
        ''' <returns>カテゴリー大分類コード</returns>
        Public Property CAT_L_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリー中分類コード
        ''' </summary>
        ''' <returns>カテゴリー中分類コード</returns>
        Public Property CAT_M_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリー小分類コード
        ''' </summary>
        ''' <returns>カテゴリー小分類コード</returns>
        Public Property CAT_S_CD As String = String.Empty

        ''' <summary>
        ''' カテゴリー大分類名称
        ''' </summary>
        ''' <returns>カテゴリー大分類名称</returns>
        Public Property CAT_L_NAME As String = String.Empty

        ''' <summary>
        ''' カテゴリー中分類名称
        ''' </summary>
        ''' <returns>カテゴリー中分類名称</returns>
        Public Property CAT_M_NAME As String = String.Empty

        ''' <summary>
        ''' カテゴリー小分類名称
        ''' </summary>
        ''' <returns>カテゴリー小分類名称</returns>
        Public Property CAT_S_NAME As String = String.Empty

        ''' <summary>
        ''' 管理ロケーションコード
        ''' </summary>
        ''' <returns>管理ロケーションコード</returns>
        Public Property MNG_LOC_L_CD As String = String.Empty

        ''' <summary>
        ''' 管理ロケーション名
        ''' </summary>
        ''' <returns>管理ロケーション名</returns>
        Public Property MNG_LOC_L_NAME As String = String.Empty

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        ''' <returns></returns>
        <Key>
        Public Property ASSET_NO As String

        ''' <summary>
        ''' 資産名称
        ''' </summary>
        ''' <returns></returns>
        Public Property ASSET_NAME As String = String.Empty

        '2022/08/31 Add Start Kawamoto
        ''' <summary>
        ''' 型番
        ''' </summary>
        ''' <returns></returns>
        Public Property MODEL_NO As String = String.Empty
        '2022/08/31 Add End Kawamoto

        ''' <summary>
        ''' 貸出状態
        ''' </summary>
        ''' <returns>貸出状態</returns>
        Public Property RESERVE_DSPSTAT As String = String.Empty

        ''' <summary>
        ''' 予約ID
        ''' </summary>
        ''' <returns>予約ID</returns>
        Public Property RESERVE_ID As Long

        ''' <summary>
        ''' 送り状番号
        ''' </summary>
        ''' <returns>送り状番号</returns>
        Public Property DEST_NO As String

        ''' <summary>
        ''' 担当者部署コード
        ''' </summary>
        ''' <returns>担当者部署コード</returns>
        Public Property CHARGE_DEP_CD As String = String.Empty

        ''' <summary>
        ''' 担当者部署名
        ''' </summary>
        ''' <returns>担当者部署名</returns>
        Public Property CHARGE_DEP_NM As String = String.Empty

        ''' <summary>
        ''' 担当者コード
        ''' </summary>
        ''' <returns>担当者コード</returns>
        Public Property CHARGE_EMP_CD As String = String.Empty

        ''' <summary>
        ''' 担当者名
        ''' </summary>
        ''' <returns>担当者名</returns>
        Public Property CHARGE_EMP_NM As String = String.Empty

        ''' <summary>
        ''' 申請者部署コード
        ''' </summary>
        ''' <returns>申請者部署コード</returns>
        Public Property REQ_DEP_CD As String = String.Empty

        ''' <summary>
        ''' 申請者部署名
        ''' </summary>
        ''' <returns>申請者部署名</returns>
        Public Property REQ_DEP_NM As String = String.Empty

        ''' <summary>
        ''' 申請者コード
        ''' </summary>
        ''' <returns>申請者コード</returns>
        Public Property REQ_EMP_CD As String = String.Empty

        ''' <summary>
        ''' 申請者名
        ''' </summary>
        ''' <returns>申請者名</returns>
        Public Property REQ_EMP_NM As String = String.Empty

        ''' <summary>
        ''' 貸出予実開始日  2021/11/30 upd nv Inoue
        ''' </summary>
        ''' <returns>貸出予実開始日</returns>
        Public Property MIN_PR_ST_DT As Integer = 0

        ''' <summary>
        ''' 貸出予実終了日  2021/11/30 upd nv Inoue
        ''' </summary>
        ''' <returns>貸出予実終了日</returns>
        Public Property PR_END_DT As Integer = 0

        ''' <summary>
        ''' ユーザー貸出開始日
        ''' </summary>
        ''' <returns>ユーザー貸出開始日</returns>
        Public Property USER_START_DT As Integer = 0

        ''' <summary>
        ''' ユーザー貸出終了日
        ''' </summary>
        ''' <returns>ユーザー貸出終了日</returns>
        Public Property USER_END_DT As Integer = 0

        ''' <summary>
        ''' 工事番号
        ''' </summary>
        ''' <returns>工事番号</returns>
        Public Property CONST_NO As String

        ''' <summary>
        ''' 工事開始日
        ''' </summary>
        ''' <returns>工事開始日</returns>
        Public Property CONST_START_DT As Integer? = 0

        ''' <summary>
        ''' 工事終了日
        ''' </summary>
        ''' <returns>工事終了日</returns>
        Public Property CONST_END_DT As Integer? = 0

        ''' <summary>
        ''' 工事内容
        ''' </summary>
        ''' <returns>工事内容</returns>
        Public Property CONST_CONTENTS As String

        ''' <summary>
        ''' 工事責任者コード
        ''' </summary>
        ''' <returns>工事責任者コード</returns>
        Public Property CONST_EMPLOYEE_CD As String

        ''' <summary>
        ''' 工事責任者名
        ''' </summary>
        ''' <returns>工事責任者名</returns>
        Public Property CONST_EMPLOYEE_NAME As String
    End Class
End Namespace
