Namespace Misc
    ''' <summary>
    ''' 
    ''' </summary>
    Public Class SlickGridInfo
        '-----インスタンス変数-----
        ''' <summary>並び順</summary>
        Private _sDispOrder As Integer
        ''' <summary>表示フラグ</summary>
        Private _sDispFlag As String

        '-----プロパティ-----
        ''' <summary>並び順</summary>
        Public Property DispOrder() As Integer
            Get
                Return _sDispOrder
            End Get
            Set(ByVal value As Integer)
                _sDispOrder = value
            End Set
        End Property

        ''' <summary>表示フラグ</summary>
        Public Property DispFlag() As String
            Get
                Return _sDispFlag
            End Get
            Set(ByVal value As String)
                _sDispFlag = value
            End Set
        End Property
    End Class
End Namespace
