Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models

    ''' <summary>
    ''' 指定数量管理表出力用表示用モデル
    ''' </summary>
    Public Class DrugSpecListViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 検索条件 期間(From)
        ''' </summary>
        ''' <returns>検索条件 期間(From)</returns>
        <NotMapped>
        Public Property SearchDateFrom As String = String.Empty

        ''' <summary>
        ''' 検索条件 期間(To)
        ''' </summary>
        ''' <returns>検索条件 期間(To)</returns>
        <NotMapped>
        Public Property SearchDateTo As String = String.Empty

        ''' <summary>
        ''' 出力条件 保管場所
        ''' </summary>
        ''' <returns>出力条件 保管場所</returns>
        <NotMapped>
        Public Property SelectStorageLoc As String = String.Empty

        ''' <summary>
        '''  保管場所ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>保管場所ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property StorageLocDropdownData As List(Of LocationDropdownModel)
    End Class
End Namespace
