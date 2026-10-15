Imports GrapeCity.Documents.Excel

Public Class QRImageInfo
    '-----インスタンス変数-----
    ''' <summary>左端の x 座標</summary>
    Private _dLeft As Double
    Private _dRight As Double
    Private _dTop As Double
    Private _dBottom As Double
    Private _dWidth As Double
    Private _dHeight As Double

    '-----プロパティ-----
    ''' <summary>左端の x 座標</summary>
    Public Property Left() As Double
        Get
            Return _dLeft
        End Get
        Set(ByVal value As Double)
            _dLeft = value
        End Set
    End Property

    ''' <summary>右端の x 座標</summary>
    Public Property Right() As Double
        Get
            Return _dRight
        End Get
        Set(ByVal value As Double)
            _dRight = value
        End Set
    End Property

    ''' <summary>上端の y 座標</summary>
    Public Property Top() As Double
        Get
            Return _dTop
        End Get
        Set(ByVal value As Double)
            _dTop = value
        End Set
    End Property

    ''' <summary>下端の y 座標</summary>
    Public Property Bottom() As Double
        Get
            Return _dBottom
        End Get
        Set(ByVal value As Double)
            _dBottom = value
        End Set
    End Property

    ''' <summary>幅</summary>
    Public Property Width() As Double
        Get
            Return _dWidth
        End Get
        Set(ByVal value As Double)
            _dWidth = value
        End Set
    End Property

    ''' <summary>高さ</summary>
    Public Property Height() As Double
        Get
            Return _dHeight
        End Get
        Set(ByVal value As Double)
            _dHeight = value
        End Set
    End Property

    '-----メソッド-----
#Region "指定のセル範囲からQR画像の位置情報を取得"
    ''' <summary>
    ''' 指定のセル範囲からQR画像の位置情報を取得
    ''' </summary>
    ''' <param name="ws">シート情報</param>
    ''' <param name="range">セル範囲</param>
    ''' <returns></returns>
    Public Shared Function GetQRPosition(ByVal ws As IWorksheet, ByVal range As IRange) As QRImageInfo
        Dim qrInfo As New QRImageInfo

        '左端の情報を取得
        Dim left As Double = 0.0
        For j As Integer = 0 To range.Column - 1
            left += ws.Columns(j).WidthInPixel
        Next
        qrInfo.Left = left

        '上端の情報を取得
        Dim top As Double = 0.0
        For j As Integer = 0 To range.Row - 1
            top += ws.Rows(j).HeightInPixel
        Next
        qrInfo.Top = top

        '右端の情報を取得
        Dim right As Double = left
        For j As Integer = range.Column To (range.Column - 1 + range.ColumnCount)
            right += ws.Columns(j).WidthInPixel
        Next
        qrInfo.Right = right

        '下端の情報を取得
        Dim bottom As Double = top
        For j As Integer = range.Row To (range.Row - 1 + range.RowCount)
            bottom += ws.Rows(j).HeightInPixel
        Next
        qrInfo.Bottom = bottom

        '幅の情報を取得
        qrInfo.Width = right - left

        '高さの情報を取得
        qrInfo.Height = bottom - top

        Return qrInfo
    End Function
#End Region

End Class
