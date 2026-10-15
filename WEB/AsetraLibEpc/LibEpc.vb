''' <summary>
''' システム名    ：ＲＦＩＤ資産管理パッケージ
''' サブシステム名：Epc関連DLL
''' クラス名      ：AsetraLibRegist LibRegist クラス
''' 作成日        ：2020/02/18  Inoue
''' 修正日        ：0000/00/00
''' </summary>
''' <remarks></remarks>
Public Class LibEpc

    Public Const ALL_LEN As Integer = 96

    Public Const GRAI_Max_SerialNo As Long = 274877906943
    Public Const GIAI_Max_SerialNo As Long = 4611686018427387903

    Public Const GRAI_PREFIX2 As String = "00110011000"     'GRAIヘッダ部＋フィルター値
    Public Const GIAI_PREFIX2 As String = "00110100000"     'GIAIヘッダ部＋フィルター値

    Public Const GRAI_SerialNoDigit2 As Integer = 38        'GRAI シリアルNo部２進数桁数





    ''' <summary>パーティション値ハッシュテーブル（事業所コード桁数,パーティション値）</summary>
    Public htPartitionVal As Hashtable = New Hashtable From {
                                                                  {12, 0},
                                                                  {11, 1},
                                                                  {10, 2},
                                                                  {9, 3},
                                                                  {8, 4},
                                                                  {7, 5},
                                                                  {6, 6}
                                                             }
    ''' <summary>事業所コード２進数桁数ハッシュテーブル（事業所コード桁数,事業所コード２進数桁数）</summary>
    Public htCompanyPrefix As Hashtable = New Hashtable From {
                                                                  {12, 40},
                                                                  {11, 37},
                                                                  {10, 34},
                                                                  {9, 30},
                                                                  {8, 27},
                                                                  {7, 24},
                                                                  {6, 20}
                                                             }

    ''' <summary>資産タイプ２進数桁数ハッシュテーブル（資産タイプ桁数,資産タイプ２進数桁数）</summary>
    Public htAssetType As Hashtable = New Hashtable From {
                                                                  {0, 4},
                                                                  {1, 7},
                                                                  {2, 10},
                                                                  {3, 14},
                                                                  {4, 17},
                                                                  {5, 20},
                                                                  {6, 24}
                                                                }

    ''' <summary>GIAIシリアル番号２進数桁数ハッシュテーブル（パーティション値,GIAIシリアル番号２進数）</summary>
    Public hGiaiIar2Digits As Hashtable = New Hashtable From {
                                                                  {0, 42},
                                                                  {1, 45},
                                                                  {2, 48},
                                                                  {3, 52},
                                                                  {4, 55},
                                                                  {5, 58},
                                                                  {6, 62}
                                                                }

    '-----private変数-----
    Private _Stat As Integer              'ステータス

    '----- プロパティ -----
    Public ReadOnly Property Stat As Integer
        Get
            Return Me._Stat
        End Get
    End Property

    '----- ステータスクラス -----

    ''' <summary>
    ''' ステータスクラス
    ''' </summary>
    Public Class EpcStatus
        ''' <summary>正常終了</summary>
        Public Shared Ok As Integer = 0
        ''' <summary>事業所コード異常</summary>
        Public Shared ErrCompanyPrefix As Integer = -1
        ''' <summary>資産タイプ異常</summary>
        Public Shared ErrAssetType As Integer = -2
        ''' <summary>シリアルNo異常</summary>
        Public Shared ErrSerialNo As Integer = -3
        ''' <summary>その他のエラー</summary>
        Public Shared ErrOther As Integer = -99
    End Class

    '----- パブリック関数 -----

    ''' <summary>
    ''' GRAI-96 識別コード作成
    ''' </summary>
    ''' <param name="sCompanyPrefix10">事業所コード</param>
    ''' <param name="sAssetType10">資産タイプ</param>
    ''' <param name="lSerialNo">シリアルNo</param>
    ''' <returns>
    ''' GRAI-96コード
    ''' </returns>
    Public Function MakeCodeGRAI96(sCompanyPrefix10 As String, sAssetType10 As String, lSerialNo As Long) As String
        Dim sRet As String = String.Empty
        Me._Stat = EpcStatus.ErrOther

        Try

            Dim iPartitionVal As Integer

            Dim sPartition2 As String = String.Empty

            Dim sCompanyPrefix2 As String = String.Empty
            Dim sAssetType2 As String = String.Empty
            Dim sSerialNo2 As String = String.Empty

            Dim iSerialNoDigit As Integer

            Dim sEpc2 As String = String.Empty
            Dim sEpc16 As String = String.Empty

            If (Not IsNumeric(sCompanyPrefix10)) Or sCompanyPrefix10.Length < 6 Or sCompanyPrefix10.Length > 12 Then
                Me._Stat = EpcStatus.ErrCompanyPrefix
                Return sRet
            End If

            If (Not String.IsNullOrEmpty(sAssetType10)) And (Not IsNumeric(sAssetType10)) Then
                Me._Stat = EpcStatus.ErrAssetType
                Return sRet
            End If

            If (sCompanyPrefix10.Length + sAssetType10.Length) <> 12 Then
                Me._Stat = EpcStatus.ErrAssetType
                Return sRet
            End If

            If lSerialNo < 1 Or lSerialNo > GRAI_Max_SerialNo Then
                Me._Stat = EpcStatus.ErrSerialNo
                Return sRet
            End If

            iPartitionVal = CInt(htPartitionVal(sCompanyPrefix10.Length))
            sPartition2 = Convert.ToString(iPartitionVal, 2).ToString
            sPartition2 = New String("0"c, (3 - sPartition2.Length)) & sPartition2

            sCompanyPrefix2 = Convert.ToString(CLng(sCompanyPrefix10), 2).ToString
            sCompanyPrefix2 = New String("0"c, CInt(htCompanyPrefix(CInt(sCompanyPrefix10.Length))) - sCompanyPrefix2.Length) & sCompanyPrefix2

            If String.IsNullOrEmpty(sAssetType10) Then
                sAssetType2 = "0"
                sAssetType2 = New String("0"c, CInt(htAssetType(CInt(sAssetType10.Length))) - sAssetType2.Length) & sAssetType2
            Else
                sAssetType2 = Convert.ToString(CLng(sAssetType10), 2).ToString
                sAssetType2 = New String("0"c, CInt(htAssetType(CInt(sAssetType10.Length))) - sAssetType2.Length) & sAssetType2
            End If

            sSerialNo2 = Convert.ToString(lSerialNo, 2).ToString
            iSerialNoDigit = GRAI_SerialNoDigit2
            sSerialNo2 = New String("0"c, iSerialNoDigit - sSerialNo2.Length) & sSerialNo2

            sEpc2 = GRAI_PREFIX2 & sPartition2 & sCompanyPrefix2 & sAssetType2 & sSerialNo2

            If sEpc2.Length <> ALL_LEN Then
                Me._Stat = EpcStatus.ErrSerialNo
                Return sRet
            End If

            sEpc16 = Cnv2to16(sEpc2)

            sRet = sEpc16
            Me._Stat = EpcStatus.Ok


        Catch ex As Exception
            Throw ex
        End Try

        Return sRet

    End Function

    ''' <summary>
    ''' GIAI-96 識別コード作成
    ''' </summary>
    ''' <param name="sCompanyPrefix10">事業所コード</param>
    ''' <param name="lSerialNo">シリアルNo</param>
    ''' <returns>
    ''' GRAI-96コード
    ''' </returns>
    Public Function MakeCodeGIAI96(sCompanyPrefix10 As String, lSerialNo As Long) As String
        Dim sRet As String = String.Empty
        Me._Stat = EpcStatus.ErrOther

        Try

            Dim iPartitionVal As Integer

            Dim sPartition2 As String = String.Empty

            Dim sCompanyPrefix2 As String = String.Empty
            Dim sAssetType2 As String = String.Empty
            Dim sSerialNo2 As String = String.Empty

            Dim iSerialNoDigit As Integer

            Dim sEpc2 As String = String.Empty
            Dim sEpc16 As String = String.Empty

            If (Not IsNumeric(sCompanyPrefix10)) Or sCompanyPrefix10.Length < 6 Or sCompanyPrefix10.Length > 12 Then
                Me._Stat = EpcStatus.ErrCompanyPrefix
                Return sRet
            End If

            If lSerialNo < 1 Or lSerialNo > GIAI_Max_SerialNo Then
                Me._Stat = EpcStatus.ErrSerialNo
                Return sRet
            End If

            iPartitionVal = CInt(htPartitionVal(sCompanyPrefix10.Length))
            sPartition2 = Convert.ToString(iPartitionVal, 2).ToString
            sPartition2 = New String("0"c, (3 - sPartition2.Length)) & sPartition2

            sCompanyPrefix2 = Convert.ToString(CLng(sCompanyPrefix10), 2).ToString
            sCompanyPrefix2 = New String("0"c, CInt(htCompanyPrefix(CInt(sCompanyPrefix10.Length))) - sCompanyPrefix2.Length) & sCompanyPrefix2

            sSerialNo2 = Convert.ToString(lSerialNo, 2).ToString
            iSerialNoDigit = CInt(hGiaiIar2Digits(iPartitionVal))
            sSerialNo2 = New String("0"c, iSerialNoDigit - sSerialNo2.Length) & sSerialNo2

            sEpc2 = GIAI_PREFIX2 & sPartition2 & sCompanyPrefix2 & sSerialNo2

            If sEpc2.Length <> ALL_LEN Then
                Me._Stat = EpcStatus.ErrSerialNo
                Return sRet
            End If

            sEpc16 = Cnv2to16(sEpc2)

            sRet = sEpc16
            Me._Stat = EpcStatus.Ok

        Catch ex As Exception
            Throw ex
        End Try

        Return sRet

    End Function

    '----- プライベート関数 -----

    ''' <summary>
    ''' 2進数文字列を16進数文字列に変換する（先頭から4文字単位）
    ''' </summary>
    ''' <param name="sStr2">2進数文字列</param>
    ''' <returns>
    ''' 16進数文字列
    ''' </returns>
    Private Function Cnv2to16(sStr2 As String) As String

        Dim sWk As String
        Dim sStr16 As String = String.Empty

        For ii As Integer = 0 To sStr2.Length - 1 Step 4

            sWk = sStr2.Substring(ii, 4)

            sStr16 &= Convert.ToUInt16(sWk, 2).ToString("X")

        Next ii

        Return sStr16

    End Function

End Class
