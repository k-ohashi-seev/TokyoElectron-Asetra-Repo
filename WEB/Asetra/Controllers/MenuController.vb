Imports Asetra.Misc
Imports System.Web.Mvc

Namespace Controllers
    ''' <summary>
    ''' メニュー画面表示用のクラス
    ''' </summary>
    Public  Class MenuController
        Inherits PageBaseController

#Region "クラス変数"
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
#End Region

#Region "一覧表示処理"
        ''' <summary>
        ''' アクションメソッド(Index)
        ''' スーパークラスの一覧表示処理を呼出します
        ''' </summary>
        ''' <returns>View表示内容</returns>
        <ValidateInput(False)>
        Function Index() As ActionResult
            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '画面右上の社員名を非表示
                ViewBag.LoginUserNm = ""
            Else
                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ''権限取得
                'ViewBag.LoginKengenKbn = _LoginInfo.KengenKbn
                ViewBag.LoginInfo = _LoginInfo
            End If

            Return View()
        End Function
#End Region

        Protected Overrides Sub CreateQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub DeleteQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub EditQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub ViewExecQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub ViewQuery()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Function CreateChcek() As Boolean
            Throw New NotImplementedException()
        End Function

        Protected Overrides Function DeleteChcek() As Boolean
            Throw New NotImplementedException()
        End Function

        Protected Overrides Function EditChcek() As Boolean
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace