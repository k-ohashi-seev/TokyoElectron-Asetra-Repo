Imports System.Data.SqlClient
Imports Asetra.Models

Namespace Misc
    ''' <summary>
    ''' 権限情報クラス
    ''' </summary>
    Public Class AuthInfo
        '-----インスタンス変数-----
        ''' <summary>メニューID</summary>
        Private _sMenuId As String
        ''' <summary>機能ID</summary>
        Private _sFunctionId As String
        ''' <summary>権限区分</summary>
        Private _sAuthDiv As String
        ''' <summary>権限タイプ</summary>
        Private _sAuthType As String
        ''' <summary>本部フラグ</summary>
        Private _sHeadOfficeFlg As String
        ''' <summary>参照権限</summary>
        Private _sReadAuthFlg As String
        ''' <summary>更新権限</summary>
        Private _sUpdAuthFlg As String
        ''' <summary>参照権限（他拠点)</summary>
        Private _sReadHubAuthFlg As String
        ''' <summary>更新権限（他拠点）</summary>
        Private _sUpdHubAuthFlg As String
        ''' <summary>特殊権限</summary>
        Private _sAuthOption As String

        '-----プロパティ-----
        ''' <summary>メニューID</summary>
        Public Property MenuId() As String
            Get
                Return _sMenuId
            End Get
            Set(ByVal value As String)
                _sMenuId = value
            End Set
        End Property

        ''' <summary>機能ID</summary>
        Public Property FunctionId() As String
            Get
                Return _sFunctionId
            End Get
            Set(ByVal value As String)
                _sFunctionId = value
            End Set
        End Property
        ''' <summary>権限区分</summary>
        Public Property AuthDiv() As String
            Get
                Return _sAuthDiv
            End Get
            Set(ByVal value As String)
                _sAuthDiv = value
            End Set
        End Property
        ''' <summary>権限タイプ</summary>
        Public Property AuthType() As String
            Get
                Return _sAuthType
            End Get
            Set(ByVal value As String)
                _sAuthType = value
            End Set
        End Property
        ''' <summary>本部フラグ</summary>
        Public Property HeadOfficeFlg() As String
            Get
                Return _sHeadOfficeFlg
            End Get
            Set(ByVal value As String)
                _sHeadOfficeFlg = value
            End Set
        End Property
        ''' <summary>参照権限</summary>
        Public Property ReadAuthFlg() As String
            Get
                Return _sReadAuthFlg
            End Get
            Set(ByVal value As String)
                _sReadAuthFlg = value
            End Set
        End Property
        ''' <summary>更新権限</summary>
        Public Property UpdAuthFlg() As String
            Get
                Return _sUpdAuthFlg
            End Get
            Set(ByVal value As String)
                _sUpdAuthFlg = value
            End Set
        End Property
        ''' <summary>参照権限（他拠点)</summary>
        Public Property ReadHubAuthFlg() As String
            Get
                Return _sReadHubAuthFlg
            End Get
            Set(ByVal value As String)
                _sReadHubAuthFlg = value
            End Set
        End Property
        ''' <summary>更新権限（他拠点）</summary>
        Public Property UpdHubAuthFlg() As String
            Get
                Return _sUpdHubAuthFlg
            End Get
            Set(ByVal value As String)
                _sUpdHubAuthFlg = value
            End Set
        End Property
        ''' <summary>特殊権限</summary>
        Public Property AuthOption() As String
            Get
                Return _sAuthOption
            End Get
            Set(ByVal value As String)
                _sAuthOption = value
            End Set
        End Property

    End Class

End Namespace
