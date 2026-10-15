Namespace Misc
    ''' <summary>
    ''' 予約申請登録/変更画面への引渡し情報クラス
    ''' </summary>
    Public Class LendingEntryInfo

        '-----インスタンス変数-----

        ''' <summary>元画面区分</summary>
        Private _sSenimotoKbn As Integer
        ''' <summary>画面表示区分</summary>
        Private _sDispKbn As Integer
        ''' <summary>貸出/返却ボタンが表示可能かどうか</summary>
        Private _isDispKashiHenBtn As Boolean
        ''' <summary>予約ID</summary>
        Private _lReserveId As Long

        '-----プロパティ-----

        ''' <summary>元画面区分</summary>
        Public Property SenimotoKbn() As Integer
            Get
                Return _sSenimotoKbn
            End Get
            Set(ByVal value As Integer)
                _sSenimotoKbn = value
            End Set
        End Property
        ''' <summary>画面表示区分</summary>
        Public Property DispKbn() As Integer
            Get
                Return _sDispKbn
            End Get
            Set(ByVal value As Integer)
                _sDispKbn = value
            End Set
        End Property
        ''' <summary>貸出/返却ボタンが表示可能かどうか</summary>
        Public Property IsDispKashiHenBtn() As Boolean
            Get
                Return _isDispKashiHenBtn
            End Get
            Set(ByVal value As Boolean)
                _isDispKashiHenBtn = value
            End Set
        End Property
        ''' <summary>予約ID</summary>
        Public Property ReserveId() As Long
            Get
                Return _lReserveId
            End Get
            Set(ByVal value As Long)
                _lReserveId = value
            End Set
        End Property

    End Class
End Namespace
