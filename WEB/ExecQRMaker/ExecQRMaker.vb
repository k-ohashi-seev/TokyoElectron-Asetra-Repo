Module ExecQRMaker

    ''' <summary>
    ''' スタートモジュール
    ''' </summary>
    ''' <param name="args">
    ''' args(0)=QRコードにする文字列
    ''' args(1)=QR画像で出力するファイル名(フルパス)
    ''' </param> 
    ''' <remarks>
    ''' 2019/08/20 Nicevision Inoue
    ''' </remarks>
    ''' 
    <STAThread()>
    Public Function Main(ByVal args() As String) As Integer

        '引数チェック
        If args.Length < 2 Then
            Return -99                      '引数エラー
        End If

        Dim sQrStr As String = args(0)          'QRコードの文字列
        Dim sFileNm As String = args(1)         'QR画像で出力するファイル名(フルパス)

        '指定ファイルのフォルダの存在チェック
        Dim sPath As String = System.IO.Path.GetDirectoryName(sFileNm)
        If Not System.IO.Directory.Exists(sPath) Then
            Return -98                      '指定ファイルの上位フォルダが存在しない、もしくは、アクセス権がありません。
        End If

        'QRMakerコントロール貼っているフォームの作成
        Dim frmQRMaker As New FormQRMaker()

        '表示テスト用
        'frmQRMaker.Show()

        Dim clsQRMaker As QRMaker = New QRMaker(frmQRMaker.QRmakerAdCtl)
        Dim ret As Integer = clsQRMaker.MakeQrData(sQrStr, sFileNm)

        Return ret
    End Function

End Module
