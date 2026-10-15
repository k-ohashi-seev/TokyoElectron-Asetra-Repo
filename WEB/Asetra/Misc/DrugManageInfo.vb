Public Class DrugManageInfo
    '-----インスタンス変数-----
    ''' <summary>保管場所</summary>
    Private _sStorageLoc As String
    ''' <summary>関係法令</summary>
    Private _sRelateLaw As String
    ''' <summary>内容量</summary>
    Private _sInternalQty As String
    ''' <summary>内容量単位(kg or L)</summary>
    Private _sInternalQtyUnit As String
    ''' <summary>資産番号</summary>
    Private _sAssetNo As String
    ''' <summary>薬品名</summary>
    Private _sAssetName As String
    ''' <summary>日付</summary>
    Private _sProcessDate As String
    ''' <summary>ロット番号</summary>
    Private _sLotNo As String
    ''' <summary>使用量</summary>
    Private _sUseQty As String
    ''' <summary>残量</summary>
    Private _sRemainQty As String
    ''' <summary>特記事項</summary>
    Private _sSpecialReport As String
    ''' <summary>作業者</summary>
    Private _sWorkerEmp As String
    ''' <summary>作業概要</summary>
    Private _sWorkOutline As String
    ''' <summary>作業時間</summary>
    Private _sWorkTime As String
    ''' <summary>取扱責任者</summary>
    Private _sResposeEmp As String

    '-----プロパティ-----
    ''' <summary>保管場所</summary>
    Public Property StorageLoc() As String
        Get
            Return _sStorageLoc
        End Get
        Set(ByVal value As String)
            _sStorageLoc = value
        End Set
    End Property
    ''' <summary>関係法令</summary>
    Public Property RelateLaw() As String
        Get
            Return _sRelateLaw
        End Get
        Set(ByVal value As String)
            _sRelateLaw = value
        End Set
    End Property
    ''' <summary>内容量</summary>
    Public Property InternalQty() As Decimal
        Get
            Return _sInternalQty
        End Get
        Set(ByVal value As Decimal)
            _sInternalQty = value
        End Set
    End Property
    ''' <summary>内容量単位(kg or L)</summary>
    Public Property InternalQtyUnit() As String
        Get
            Return _sInternalQtyUnit
        End Get
        Set(ByVal value As String)
            _sInternalQtyUnit = value
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
    ''' <summary>薬品名</summary>
    Public Property AssetName() As String
        Get
            Return _sAssetName
        End Get
        Set(ByVal value As String)
            _sAssetName = value
        End Set
    End Property
    ''' <summary>日付</summary>
    Public Property ProcessDt() As String
        Get
            Return _sProcessDate
        End Get
        Set(ByVal value As String)
            _sProcessDate = value
        End Set
    End Property
    ''' <summary>ロット番号</summary>
    Public Property LotNo() As String
        Get
            Return _sLotNo
        End Get
        Set(ByVal value As String)
            _sLotNo = value
        End Set
    End Property
    ''' <summary>使用量</summary>
    Public Property UseQty() As String
        Get
            Return _sUseQty
        End Get
        Set(ByVal value As String)
            _sUseQty = value
        End Set
    End Property
    ''' <summary>残量</summary>
    Public Property RemainQty() As String
        Get
            Return _sRemainQty
        End Get
        Set(ByVal value As String)
            _sRemainQty = value
        End Set
    End Property
    ''' <summary>特記事項</summary>
    Public Property SpecialReport() As String
        Get
            Return _sSpecialReport
        End Get
        Set(ByVal value As String)
            _sSpecialReport = value
        End Set
    End Property
    ''' <summary>作業者</summary>
    Public Property WorkerEmp() As String
        Get
            Return _sWorkerEmp
        End Get
        Set(ByVal value As String)
            _sWorkerEmp = value
        End Set
    End Property
    ''' <summary>作業概要</summary>
    Public Property WorkOutline() As String
        Get
            Return _sWorkOutline
        End Get
        Set(ByVal value As String)
            _sWorkOutline = value
        End Set
    End Property
    ''' <summary>作業時間</summary>
    Public Property WorkTime() As String
        Get
            Return _sWorkTime
        End Get
        Set(ByVal value As String)
            _sWorkTime = value
        End Set
    End Property
    ''' <summary>取扱責任者</summary>
    Public Property ResposeEmp() As String
        Get
            Return _sResposeEmp
        End Get
        Set(ByVal value As String)
            _sResposeEmp = value
        End Set
    End Property

End Class
