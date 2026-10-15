Namespace Misc

    ''' <summary>
    ''' 工事一覧画面リダイレクト情報クラス
    ''' </summary>
    Public Class RedirectConstructionListInfo

        '-----定数-----

        ''' <summary>セッションID</summary>
        Public Const ID As String = "RedirectConstructionListInfo"

        ''' <summary>
        ''' 呼出元区分
        ''' </summary>
        Public Enum RedirectPageType
            ''' <summary>メニュー</summary>
            Menu = 0
            ''' <summary>工事一覧</summary>
            ConstructionList = 1
            ''' <summary>工事資産一覧</summary>
            ConstAssetList = 2
            ''' <summary>工事履歴一覧</summary>
            ConstructionHist = 3
            ''' <summary>仮置一覧</summary>
            TemporaryList = 4
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
