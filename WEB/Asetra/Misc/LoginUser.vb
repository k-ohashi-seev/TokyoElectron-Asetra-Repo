Imports Asetra.Models


Namespace RfidEntity
    ''' <summary>
    ''' ログインユーザークラス
    ''' </summary>
    ''' <remarks>
    ''' 2019/08/05 nv miyaji
    ''' </remarks>
    Public Class LoginUser

        '-----インスタンス変数-----
        ''' <summary>社員コード</summary>
        Private _sEmployeeCd As String
        ''' <summary>社員名</summary>
        Private _sEmployeeNm As String
        ''' <summary>部署コード</summary>
        Private _sDepartmentCd As String
        ''' <summary>部署名</summary>
        Private _sDepartmentNm As String
        ''' <summary>権限区分（社員マスタ）</summary>
        Private _sKengenKbn As String
        ''' <summary>権限タイプ</summary>
        Private _sAuthType As String
        ''' <summary>本部フラグ</summary>
        Private _sHeadOfficeFlg As String
        ''' <summary>詳細権限区分（WEB内使用）</summary>
        Private _sShosaiAuthDiv As String


        '-----プロパティ-----
        ''' <summary>社員コードを取得および設定します。</summary>
        Public Property EmployeeCd() As String
            Get
                Return _sEmployeeCd
            End Get
            Set(ByVal value As String)
                _sEmployeeCd = value
            End Set
        End Property
        ''' <summary>社員名を取得および設定します。</summary>
        Public Property EmployeeNm() As String
            Get
                Return _sEmployeeNm
            End Get
            Set(ByVal value As String)
                _sEmployeeNm = value
            End Set
        End Property
        ''' <summary>部署コードを取得および設定します</summary>
        Public Property DepartmentCd() As String
            Get
                Return _sDepartmentCd
            End Get
            Set(ByVal value As String)
                _sDepartmentCd = value
            End Set
        End Property
        ''' <summary>部署名を取得および設定します</summary>
        Public Property DepartmentNm() As String
            Get
                Return _sDepartmentNm
            End Get
            Set(ByVal value As String)
                _sDepartmentNm = value
            End Set
        End Property
        ''' <summary>権限区分を取得および設定します</summary>
        Public Property KengenKbn() As String
            Get
                Return _sKengenKbn
            End Get
            Set(ByVal value As String)
                _sKengenKbn = value
            End Set
        End Property
        ''' <summary>権限タイプを取得および設定します</summary>
        Public Property AuthType() As String
            Get
                Return _sAuthType
            End Get
            Set(ByVal value As String)
                _sAuthType = value
            End Set
        End Property
        ''' <summary>本部フラグを取得および設定します</summary>
        Public Property HeadOfficeFlg() As String
            Get
                Return _sHeadOfficeFlg
            End Get
            Set(ByVal value As String)
                _sHeadOfficeFlg = value
            End Set
        End Property
        ''' <summary>詳細権限区分を取得および設定します</summary>
        Public Property ShosaiAuthDiv() As String
            Get
                Return _sShosaiAuthDiv
            End Get
            Set(ByVal value As String)
                _sShosaiAuthDiv = value
            End Set
        End Property

        ''-----コンストラクタ-----

        '''' <summary>
        '''' コンストラクタ
        '''' 外部からのインスタンス作成を禁止。
        '''' </summary>
        '''' <remarks></remarks>
        'Private Sub New()
        'End Sub

    End Class

End Namespace
