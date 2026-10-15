Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
''' <summary>
''' システム名    ：ＲＦＩＤ資産管理パッケージ
''' サブシステム名：ライセンス・トライアルDLL
''' クラス名      ：AsetraLibRegist LibRegist クラス
''' 作成日        ：2019/12/16  Inoue
''' 修正日        ：0000/00/00
''' </summary>
''' <remarks></remarks>
Public Class LibRegist

    '----- パブリック関数 -----

    '2021/11/14 ランセンスキーより、プロダクトキーを求めるよう、変更 nv Inoue
    '''' <summary>
    '''' WebサーバーMACアドレスを元に、プロダクト番号を取得する。
    '''' </summary>
    'Public Shared Function GetProductNo() As String
    '    Dim sRet As String

    '    sRet = SubRegist.SubGetProductNo()
    '    'sRet = "11111-22222-33333-44444-55555"

    '    Return sRet

    'End Function
    ''' <summary>
    ''' ランセンスキーを元に、プロダクト番号を取得する。
    ''' </summary>
    ''' <param name="sLicenseKey">LICENSE_KEY</param>
    Public Shared Function GetProductNo(ByVal sLicenseKey As String) As String
        Dim sRet As String

        sRet = SubRegist.SubGetProductNo(sLicenseKey)

        Return sRet

    End Function


    ''' <summary>
    ''' プロダクト番号を元に、プロダクトキーを取得する
    ''' </summary>
    ''' <param name="sProductNo">プロダクト番号</param>
    Public Shared Function GetProductkey(sProductNo As String) As String
        Dim sRet As String

        sRet = SubRegist.SubGetProductkey(sProductNo)
        'sRet = "AAAAA-BBBBB-CCCCC-DDDDD-EEEEE"

        Return sRet

    End Function

    ''' <summary>
    ''' プロダクト番号とプロダクトキーを元に、レジストリの取得有無、試用残日数を取得する。
    ''' </summary>
    ''' <param name="sProductkey">プロダクトキー</param>
    Public Shared Function CheckLicense(sProductkey As String) As Integer
        Dim iRet As Integer

        'iRet = 0
        iRet = SubRegist.SubCheckLicense(sProductkey)

        Return iRet

    End Function

    ''' <summary>
    ''' 指定のプロダクトキーをレジストリに登録する。
    ''' </summary>
    ''' <param name="sProductkey">プロダクトキー</param>
    Public Shared Sub RegistLicense(sProductkey As String)

        Call SubRegist.SubRegistLicense(sProductkey)
    End Sub

    ''' <summary>
    ''' DB作成日より、LICENSE_KEY文字列を作成する
    ''' </summary>
    ''' <param name="dtDbMakeDate">DB作成日(ASETRAバージョンMのシリアルNoを複合化した日付)</param>
    ''' <returns>
    ''' LICENSE_KEY文字列 エラー時: String.Empty
    ''' </returns>
    ''' ''' <remarks>作成日 2021/11/14  Inoue</remarks>
    Public Shared Function GetLicenseKey(dtDbMakeDate As Date) As String

        Return SubRegist.GetLicenseKey(dtDbMakeDate)

    End Function

    ''' <summary>
    ''' DB作成日 複合化処理
    ''' </summary>
    ''' <param name="sSerialNo">シリアルNO(ASETRAバージョンM)</param>
    ''' <returns>
    ''' DB作成日 エラー時: nothing
    ''' </returns>
    Public Shared Function GetMkDbDateDecrypt(sSerialNo As String) As Date?

        Dim dtMkDbDate As Date?

        '空値チェック
        If String.IsNullOrEmpty(sSerialNo) Then
            Return Nothing
        End If

        'チェックデジット取得
        Dim sChkDigit As String = sSerialNo.Substring(sSerialNo.Length - 1, 1)

        Dim iDate As Integer
        Try
            iDate = CInt(sSerialNo.Substring(0, sSerialNo.Length - 1)) - 54321234
        Catch ex As Exception
            Return Nothing
        End Try

        '数値から得られるチェックデジットを求める
        Dim sMkDbDate As String = iDate.ToString
        If sMkDbDate.Length <> 8 Then
            Return Nothing
        End If
        Dim iCalcChkDigit As Integer = 0
        For ii As Integer = 0 To 7
            iCalcChkDigit += CInt(sMkDbDate.Substring(ii, 1))
        Next
        Dim sCalcChkDigit As String = (iCalcChkDigit Mod 10).ToString

        'チェックデジットが計算値と同じでない時、エラー
        If sChkDigit <> sCalcChkDigit Then
            Return Nothing
        End If

        Try
            dtMkDbDate = New Date(CInt(sMkDbDate.Substring(0, 4)), CInt(sMkDbDate.Substring(4, 2)), CInt(sMkDbDate.Substring(6, 2)))
        Catch ex As Exception
            Return Nothing
        End Try

        Return dtMkDbDate
    End Function

End Class
