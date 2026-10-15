Imports System.Data.SqlClient
Imports Asetra.Models

Namespace Misc

    ''' <summary>
    ''' カゴ情報クラス
    ''' </summary>
    Public Class CartInfo

        '-----インスタンス変数-----

        ''' <summary>管理ロケ大区分コード</summary>
        Private _sMngLocLCd As String
        ''' <summary>管理ロケ大区分名称</summary>
        Private _sMngLocLName As String
        ''' <summary>資産番号</summary>
        Private _sAssetNo As String
        ''' <summary>資産名称</summary>
        Private _sAssetName As String

        '-----プロパティ-----

        ''' <summary>管理ロケ大区分コード</summary>
        Public Property MngLocLCd() As String
            Get
                Return _sMngLocLCd
            End Get
            Set(ByVal value As String)
                _sMngLocLCd = value
            End Set
        End Property
        ''' <summary>管理ロケ大区分名称</summary>
        Public Property MngLocLName() As String
            Get
                Return _sMngLocLName
            End Get
            Set(ByVal value As String)
                _sMngLocLName = value
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
        ''' <summary>資産名称</summary>
        Public Property AssetName() As String
            Get
                Return _sAssetName
            End Get
            Set(ByVal value As String)
                _sAssetName = value
            End Set
        End Property
    End Class
End Namespace
