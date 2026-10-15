Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
''' <summary>
''' システム名    ：ＲＦＩＤ資産管理パッケージ
''' サブシステム名：ライセンス・トライアルDLL
''' クラス名      ：AsetraLibLicense LibLicense クラス
''' 作成日        ：2019/12/16  Inoue
''' 修正日        ：0000/00/00
''' </summary>
''' <remarks></remarks>
Public Class LibLicense

    '----- パブリック関数 -----

    ''' <summary>
    ''' プロダクト番号を元に、プロダクトキーを取得する
    ''' </summary>
    ''' <param name="sProductNo">プロダクト番号</param>
    Public Shared Function GetProductkey(sProductNo As String) As String
        Dim sRet As String

        Try
            sRet = SubRegist.SubGetProductkey(sProductNo)
        Catch ex As Exception
            Return String.Empty
        End Try

        Return sRet

    End Function

End Class

