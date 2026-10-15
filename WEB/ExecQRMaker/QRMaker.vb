Imports System.Runtime.InteropServices

''' <summary>
''' システム名    ：ＲＦＩＤ資産管理パッケージ
''' サブシステム名：
''' クラス名      ：QRMaker クラス
''' 作成日        ：2019/08/17      Inoue
''' 修正日        ：0000/00/00
''' </summary>
''' <remarks></remarks>
Public Class QRMaker

#Region "変数宣言"
    ''' <summary>AxQRMAKERADLib</summary>
    Private _QRmakerAd As AxQRMAKERADLib.AxQRmakerAd

#End Region

#Region "コンストラクタ"
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="QRmakerAd">QRMakerAD コントロール</param>
    ''' <remarks></remarks>
    Public Sub New(QRmakerAd As AxQRMAKERADLib.AxQRmakerAd)
        Me._QRmakerAd = QRmakerAd

        'コンストラクタで、初期設定値を設定します
        Me._QRmakerAd.CodeType = QRMAKERADLib.enumCodeType.QR       'コード種類              ：   QRコード
        Me._QRmakerAd.ModelNo = QRMAKERADLib.enumModelNo.Model2     'QRコードのモデルNo      ：   Model2
        Me._QRmakerAd.QRversion = 0                                 'QRコードバージョンNo    ：   Auto
        Me._QRmakerAd.EccLevel = QRMAKERADLib.enumEccLevel.M        'QRコード誤り訂正レベル  ：   M(15%)
        Me._QRmakerAd.MbcsID = 932                                  'QRコードの漢字セット    ：   シフトJIS

        Me._QRmakerAd.CellAdjust = 0
        Me._QRmakerAd.CellUnit = 300
        Me._QRmakerAd.CellPitch = 6
        Me._QRmakerAd.QuietZone = 5         'マージン                ：   5セル

    End Sub
#End Region

#Region "QRデータ画像ファイルを作成する"
    ''' <summary>
    ''' QRデータ画像ファイルを作成する
    ''' </summary>
    ''' <param name="sQrStr">出力文字列</param>
    ''' <param name="sFileNm">出力ファイル名（フルパス）</param>
    ''' <returns>
    ''' DataTable
    ''' </returns>
    Public Function MakeQrData(sQrStr As String, sFileNm As String) As Integer

        Me._QRmakerAd.InputData = sQrStr

        '表示テスト用
        'Me._QRmakerAd.AutoRedraw = QRMAKERADLib.enumOnOff.On
        'Me._QRmakerAd.Refresh()
        'System.Windows.Forms.Application.DoEvents()

        Dim iCellCnt As Integer = Me._QRmakerAd.CreateQrMetaFile(vbNull, sFileNm, 3)

        Return Me._QRmakerAd.MakeStatus
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
    Public Function GetQRMakerMsg(ByVal status As Integer) As String

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
            Case Else
                GetQRMakerMsg = "予期せぬエラーが発生しました (" & status.ToString & ")"
        End Select

    End Function
#End Region

End Class
