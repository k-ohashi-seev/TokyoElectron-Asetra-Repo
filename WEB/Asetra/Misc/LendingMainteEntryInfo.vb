Namespace Misc
    ''' <summary>
    ''' メンテナンス登録/変更画面への引渡し情報クラス
    ''' </summary>
    Public Class LendingMainteEntryInfo

        '-----インスタンス変数-----

        ''' <summary>画面表示区分</summary>
        Private _sDispKbn As String
        ''' <summary>メンテナンスID</summary>
        Private _lMainteId As Long
        ''' <summary>資産番号</summary>
        Private _sAssetNo As String
        ''' <summary>メンテナンス区分</summary>
        Private _sMainteKbn As String

        '-----プロパティ-----

        ''' <summary>画面表示区分</summary>
        Public Property DispKbn() As String
            Get
                Return _sDispKbn
            End Get
            Set(ByVal value As String)
                _sDispKbn = value
            End Set
        End Property
        ''' <summary>メンテナンスID</summary>
        Public Property MainteId() As Long
            Get
                Return _lMainteId
            End Get
            Set(ByVal value As Long)
                _lMainteId = value
            End Set
        End Property
        ''' <summary>資産番号</summary>
        Public Property AssetNo() As String
            Get
                Return _sAssetNo
            End Get
            Set(ByVal value As String)
                _sAssetNo = value
            End Set
        End Property

        ''' <summary>メンテナンス区分</summary>
        Public Property MainteKbn() As String
            Get
                Return _sMainteKbn
            End Get
            Set(ByVal value As String)
                _sMainteKbn = value
            End Set
        End Property
    End Class
End Namespace

