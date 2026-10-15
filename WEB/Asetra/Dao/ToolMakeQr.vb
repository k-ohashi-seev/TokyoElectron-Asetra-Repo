Imports Asetra.Misc
Imports System.IO

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：QRコード作成関連クラス
    ''' 作成日        ：2019/08/23      Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class MakeQr


#Region "QRコードデータをQR画像保存フォルダに作成する"
        ''' <summary>
        ''' QRコードデータを作成する
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <param name="sQrFileName">QR画像で出力するファイル名(フルパス)</param>
        ''' <returns>0:正常終了  以外:エラーコード</returns>
        Public Shared Function GetQRData(ByVal sAssetNo As String, sQrFileName As String) As Integer
            '--QRコードを作成する

            'プログラム存在しない場合はエラーメッセージを表示し処理を中止します。
            Dim sExe As String = System.IO.Path.GetFullPath(HttpContext.Current.Server.MapPath("~/") & Misc.CommonConst.QREXE＿PATH)
            If Not System.IO.File.Exists(sExe) Then
                Return -100
            End If

            'プログラムを実行します。
            Dim sArgs As String = """" & sAssetNo & """ """ & sQrFileName & """"
            Using objProccess As Process = Process.Start(sExe, sArgs)
                objProccess.WaitForExit()
                Return objProccess.ExitCode
            End Using

            ''画像ファイルを読み込んで、Imageオブジェクトを作成する
            'Dim fs As FileStream = File.OpenRead("C:\test.jpg")

            ''画像を返す
            'Return fs

        End Function
#End Region

#Region "エラー番号→エラーメッセージの変換"
        ''' <summary>
        ''' statusコード→QRMakerメッセージの変換
        ''' </summary>
        ''' <param name="status">QRMaker 処理ステータス</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Shared Function GetQRMakerMsg(ByVal status As Integer) As String

            Select Case status
                Case 0
                    GetQRMakerMsg = ""
                Case -1
                    GetQRMakerMsg = "文字数オーバー。"
                Case -2
                    GetQRMakerMsg = "文字モードの変化数オーバー。"
                Case -3
                    GetQRMakerMsg = "不正な分割情報です。"
                Case -4
                    GetQRMakerMsg = "不正なコードモデル番号です。"
                Case -5
                    GetQRMakerMsg = "ファイル作成に失敗しました。"
                Case -6
                    GetQRMakerMsg = "バージョン番号指定エラー。"
                Case -7
                    GetQRMakerMsg = "不正なファイルフォーマット指定です。"
                Case -8
                    GetQRMakerMsg = "パラメーターが異常です。"
                Case -9
                    GetQRMakerMsg = "プリンタドライバの取得に失敗しました。"
                Case -10
                    GetQRMakerMsg = "描画バッファサイズが大きすぎます。"
                Case -98
                    GetQRMakerMsg = "指定ファイルの上位フォルダが存在しない、もしくは、アクセス権がありません。"
                Case -99
                    GetQRMakerMsg = "QR作成プログラム引数エラー。"
                Case -100
                    GetQRMakerMsg = "QR作成プログラムが存在しません。"
                Case Else
                    GetQRMakerMsg = "予期せぬエラーが発生しました (" & status.ToString & ")"
            End Select

        End Function
#End Region

    End Class

End Namespace
