Imports Asetra.Models
Imports Asetra.Misc
Imports System.Web.Mvc

Namespace Controllers
    ''' <summary>
    ''' ライセンス認証のクラス
    ''' </summary>
    Public Class LicenseController
        Inherits Controller

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"
        ''' <summary>
        ''' ライセンスチェック結果
        ''' </summary>
        Private Enum licenseCheckResult
            OK = 0
            NG = -1
        End Enum
#End Region

        ''' <summary>
        ''' アクションメソッド(Index)
        ''' スーパークラスの一覧表示処理を呼出します
        ''' </summary>
        ''' <returns>View表示内容</returns>
        Function Index() As ActionResult

            '結果表示

            'Return RedirectToAction("Index", "Login")

            'ASETRAバージョンテーブルデータ取得
            Dim tbl As DataTable = Dao.ToolAsetraVer.GetAsetraVerInfo()

            'DB作成日取得
            Dim dtMkDbDate As Date? = AsetraLibRegist.LibRegist.GetMkDbDateDecrypt(tbl.Rows(0).Item("SERIAL_NO").ToString)

            'ライセンスキー取得
            Dim sLicenseKey As String = tbl.Rows(0).Item("LICENSE_KEY").ToString

            If String.IsNullOrEmpty(sLicenseKey) AndAlso dtMkDbDate.HasValue Then
                'ライセンスキーを生成し、DBにセット
                sLicenseKey = AsetraLibRegist.LibRegist.GetLicenseKey(dtMkDbDate.Value)
                Call Dao.ToolAsetraVer.UpdLicenseKey(sLicenseKey)
            End If

            Dim sNo As String = String.Empty            'プロダクト番号
            Dim sKey As String = String.Empty            'プロダクトキー

            '認証OKセッションを初期化する
            Session.Remove(CommonSessionKey.REGIST_OK)

            'WebサーバーMACアドレスを元に、プロダクト番号を取得
            sNo = AsetraLibRegist.LibRegist.GetProductNo(sLicenseKey)

            'logger.Info("●●● ProductNo=[" & sNo & "]")


            'プロダクト番号を元に、プロダクトキーを取得する
            sKey = AsetraLibRegist.LibRegist.GetProductkey(sNo)

            logger.Info("●●● Productkey=[" & sKey & "]")

            'ライセンスチェック          ■■■　一時的にコメントアウト  ver2.0
            'Dim iRet As Integer = AsetraLibRegist.LibRegist.CheckLicense(sKey)

            '■■■　開発用に、全てOKにする  ver2.0
            Dim iRet As Integer = licenseCheckResult.OK

            '●●●　テスト用
            'dtMkDbDate = Now.Date.AddDays(-1)

            Select Case iRet
                Case licenseCheckResult.OK
                    '認証OK
                    Session(CommonSessionKey.REGIST_OK) = True

                    '結果表示
                    Return RedirectToAction("Index", "Login")
                Case licenseCheckResult.NG
                    '試用期限切れ
                    Session(CommonSessionKey.REGIST_PRODUCT_NO) = sNo
                    Session(CommonSessionKey.REGIST_PRODUCT_KEY) = sKey
                    Session(CommonSessionKey.REGIST_TRIAL_LIMIT) = 0

                    '結果表示
                    Return RedirectToAction("Index", "EntryProductKey")
                Case Else
                    '試用期限内
                    If dtMkDbDate.HasValue AndAlso dtMkDbDate.Value.AddMonths(1) >= Now.Date() Then
                        '試用OK

                        '試用期限は、DB作成日から算出した日数と比べ、小さい方
                        'Dim iDbKigen As Integer = (dtMkDbDate.Value.AddMonths(1) - Now.Date()).Days + 1
                        Dim iDbKigen As Integer = (dtMkDbDate.Value.AddMonths(1) - Now.Date()).Days
                        If iRet > iDbKigen Then
                            iRet = iDbKigen
                        End If

                        Session(CommonSessionKey.REGIST_OK) = True
                        Session(CommonSessionKey.REGIST_PRODUCT_NO) = sNo
                        Session(CommonSessionKey.REGIST_PRODUCT_KEY) = sKey
                        Session(CommonSessionKey.REGIST_TRIAL_LIMIT) = iRet
                    Else
                        'DB作成日が取得できない場合、もしくは、DB作成日から1か月を超える場合、試用期限切れ
                        Session(CommonSessionKey.REGIST_PRODUCT_NO) = sNo
                        Session(CommonSessionKey.REGIST_PRODUCT_KEY) = sKey
                        Session(CommonSessionKey.REGIST_TRIAL_LIMIT) = 0
                    End If

                    '結果表示
                    Return RedirectToAction("Index", "EntryProductKey")
            End Select

        End Function

    End Class
End Namespace