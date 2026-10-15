Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace Models
    ''' <summary>
    ''' 項目配置マスタ詳細ビューモデル
    ''' </summary>
    Partial Public Class ColSetDetailViewModel
        Inherits BaseModel

        ''' <summary>
        ''' DB項目名
        ''' </summary>
        <Key>
        <StringLength(100)>
        Public Property DB_COLUMN_NAME As String

        ''' <summary>
        ''' 並び順
        ''' </summary>
        <Required>
        Public Property DISP_ORDER As Integer

        ''' <summary>
        ''' 表示フラグ
        ''' </summary>
        <Required>
        <StringLength(1)>
        Public Property DISP_FLAG As String

        ''' <summary>
        ''' 項目名
        ''' </summary>
        <Required>
        <StringLength(2048)>
        Public Property COL_NAME As String

        ''' <summary>
        ''' 固定列フラグ
        ''' </summary>
        <Required>
        <StringLength(1)>
        Public Property FIX_COL_FLG As String

        ''' <summary>
        ''' 拠点指定なし画面名 
        ''' </summary>
        <Required>
        <StringLength(1)>
        Public Property NO_DESIGNATED_LOC_NAME As List(Of String)

        ''' <summary>
        ''' 所属拠点名 
        ''' </summary>
        <Required>
        <StringLength(1)>
        Public Property HOME_BASE_NAME As Dictionary(Of String, String)

        ''' <summary>
        ''' 固定列数
        ''' </summary>
        <Required>
        Public Property FIX_COL_CNT As Integer?

        ''' <summary>
        ''' 検索項目：画面名
        ''' </summary>
        ''' <returns>検索項目：画面名</returns>
        <NotMapped>
        Public Property SearchDispDiv As String

        ''' <summary>
        ''' 検索項目：拠点名
        ''' </summary>
        ''' <returns>検索項目：拠点名</returns>
        <NotMapped>
        Public Property SearchLocCd As String

        ''' <summary>
        ''' 固定列数
        ''' </summary>
        ''' <returns>固定列数</returns>
        <NotMapped>
        Public Property FixColCnt As String
    End Class
End Namespace