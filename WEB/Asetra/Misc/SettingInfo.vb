Imports System.Data.SqlClient
Imports Asetra.Models

Namespace Misc
    ''' <summary>
    ''' 設定情報クラス
    ''' </summary>
    Public Class SettingInfo

        '-----インスタンス変数-----
        ''' <summary>KEY種別</summary>
        Private _sKeyDiv As String
        ''' <summary>KEY1</summary>
        Private _sKey1 As String
        ''' <summary>KEY2</summary>
        Private _sKey2 As String
        ''' <summary>KEY3</summary>
        Private _sKey3 As String
        ''' <summary>備考</summary>
        Private _sReMark As String
        ''' <summary>DATA1</summary>
        Private _sData1 As String
        ''' <summary>DATA2</summary>
        Private _sData2 As String
        ''' <summary>DATA3</summary>
        Private _sData3 As String
        ''' <summary>DATA4</summary>
        Private _sData4 As String
        ''' <summary>DATA5</summary>
        Private _sData5 As String
        ''' <summary>DATA6</summary>
        Private _sData6 As String
        ''' <summary>DATA7</summary>
        Private _sData7 As String
        ''' <summary>DATA8</summary>
        Private _sData8 As String
        ''' <summary>DATA9</summary>
        Private _sData9 As String
        ''' <summary>DATA10</summary>
        Private _sData10 As String


        '-----プロパティ-----
        ''' <summary>KEY種別を取得および設定します。</summary>
        Public Property KeyDiv() As String
            Get
                Return _sKeyDiv
            End Get
            Set(ByVal value As String)
                _sKeyDiv = value
            End Set
        End Property
        ''' <summary>KEY1を取得および設定します。</summary>
        Public Property Key1() As String
            Get
                Return _sKey1
            End Get
            Set(ByVal value As String)
                _sKey1 = value
            End Set
        End Property
        ''' <summary>KEY2を取得および設定します。</summary>
        Public Property Key2() As String
            Get
                Return _sKey2
            End Get
            Set(ByVal value As String)
                _sKey2 = value
            End Set
        End Property
        ''' <summary>KEY3を取得および設定します。</summary>
        Public Property Key3() As String
            Get
                Return _sKey3
            End Get
            Set(ByVal value As String)
                _sKey3 = value
            End Set
        End Property
        ''' <summary>DATA1を取得および設定します。</summary>
        Public Property DATA1() As String
            Get
                Return _sData1
            End Get
            Set(ByVal value As String)
                _sData1 = value
            End Set
        End Property
        ''' <summary>DATA2を取得および設定します。</summary>
        Public Property DATA2() As String
            Get
                Return _sData2
            End Get
            Set(ByVal value As String)
                _sData2 = value
            End Set
        End Property
        ''' <summary>DATA3を取得および設定します。</summary>
        Public Property DATA3() As String
            Get
                Return _sData3
            End Get
            Set(ByVal value As String)
                _sData3 = value
            End Set
        End Property
        ''' <summary>DATA4を取得および設定します。</summary>
        Public Property DATA4() As String
            Get
                Return _sData4
            End Get
            Set(ByVal value As String)
                _sData4 = value
            End Set
        End Property
        ''' <summary>DATA5を取得および設定します。</summary>
        Public Property DATA5() As String
            Get
                Return _sData5
            End Get
            Set(ByVal value As String)
                _sData5 = value
            End Set
        End Property
        ''' <summary>DATA6を取得および設定します。</summary>
        Public Property DATA6() As String
            Get
                Return _sData6
            End Get
            Set(ByVal value As String)
                _sData6 = value
            End Set
        End Property
        ''' <summary>DATA7を取得および設定します。</summary>
        Public Property DATA7() As String
            Get
                Return _sData7
            End Get
            Set(ByVal value As String)
                _sData7 = value
            End Set
        End Property
        ''' <summary>DATA8を取得および設定します。</summary>
        Public Property DATA8() As String
            Get
                Return _sData8
            End Get
            Set(ByVal value As String)
                _sData8 = value
            End Set
        End Property
        ''' <summary>DATA9を取得および設定します。</summary>
        Public Property DATA9() As String
            Get
                Return _sData9
            End Get
            Set(ByVal value As String)
                _sData9 = value
            End Set
        End Property
        ''' <summary>DATA10を取得および設定します。</summary>
        Public Property DATA10() As String
            Get
                Return _sData10
            End Get
            Set(ByVal value As String)
                _sData10 = value
            End Set
        End Property
    End Class
End Namespace
