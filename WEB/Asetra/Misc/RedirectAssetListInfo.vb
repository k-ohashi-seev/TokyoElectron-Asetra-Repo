Namespace Misc

    ''' <summary>
    ''' 資産一覧画面リダイレクト情報クラス
    ''' </summary>
    Public Class RedirectAssetListInfo

        '-----定数-----

        ''' <summary>セッションID</summary>
        Public Const ID As String = "RedirectAssetListInfo"

        ''' <summary>
        ''' 呼出元区分
        ''' </summary>
        Public Enum RedirectPageType
            ''' <summary>メニュー</summary>
            Menu = 0
            ''' <summary>資産在庫照会</summary>
            StockList = 1
            ''' <summary>資産一覧</summary>
            AssetList = 2
        End Enum

        '-----プロパティ-----

        ''' <summary>
        ''' 呼出元画面
        ''' </summary>
        Public Property RedirectPage As Integer

        ''' <summary>
        ''' 画面表示用キー項目
        ''' </summary>
        Public Property KeyValue As String

    End Class
End Namespace
