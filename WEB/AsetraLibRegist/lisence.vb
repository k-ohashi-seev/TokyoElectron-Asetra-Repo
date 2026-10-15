Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
''' <summary>
''' システム名    ：ＲＦＩＤ資産管理パッケージ
''' サブシステム名：
''' クラス名      ：AsetraLibRegist lisence クラス
''' 作成日        ：2019/12/16
''' 修正日        ：0000/00/00
''' </summary>
''' <remarks></remarks>
Public Class lisence

    '----- パブリック関数 -----

    ''' <summary>
    ''' WebサーバーMACアドレスを元に、プロダクト番号を取得する。
    ''' </summary>
    Public Shared Function GetProductNo() As String
        Dim sRet As String = "11111-22222-33333-44444-55555"


        Return sRet

    End Function

    ''' <summary>
    ''' プロダクト番号を元に、プロダクトキーを取得する
    ''' </summary>
    ''' <param name="sProductNo">プロダクト番号</param>
    Public Shared Function GetProductkey(sProductNo As String) As String
        Dim sRet As String = "AAAAA-BBBBB-CCCCC-DDDDD-EEEEE"


        Return sRet

    End Function

    ''' <summary>
    ''' プロダクト番号とプロダクトキーを元に、レジストリの取得有無、試用残日数を取得する。
    ''' </summary>
    ''' <param name="sProductkey">プロダクトキー</param>
    Public Shared Function CheckLicense(sProductkey As String) As Integer
        Dim iRet As Integer = 0


        Return iRet

    End Function

    ''' <summary>
    ''' 指定のプロダクトキーをレジストリに登録する。
    ''' </summary>
    ''' <param name="sProductkey">プロダクトキー</param>
    Public Shared Sub registLicense(sProductkey As String)


    End Sub



    '----- プライベート関数 -----

    ''' <summary>
    ''' 文字列をAESで暗号化
    ''' </summary>
    Private Function Encrypt(text As String) As String

        ' 参考サイト
        ' http://programmers.high-way.info/vb/aes.html

        Return String.Empty

    End Function




End Class
