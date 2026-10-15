Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Threading

Namespace Controllers
    ''' <summary>
    ''' プロダクトキー入力画面表示用のクラス
    ''' </summary>
    Public Class EntryProductKeyController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス変数"
        Private _ViewModel As EntryProductKeyViewModel = New EntryProductKeyViewModel                    'Employee一覧表示用モデル
        Protected _viewResult As List(Of EntryProductKeyViewModel) = New List(Of EntryProductKeyViewModel)     '結果View
        Private _Message As MessageRfid = Nothing
#End Region

#Region "一覧表示処理"
        ''' <summary>
        ''' アクションメソッド(Index)
        ''' スーパークラスの一覧表示処理を呼出します
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns>View表示内容</returns>
        Function Index(ByVal ViewModel As EntryProductKeyViewModel, ByVal actionname As String) As ActionResult

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As EntryProductKeyViewModel = Session(CommonSessionKey.CREAGE_ENTRYPRODUCTKEY)
            If Not IsNothing(model) Then
                _ViewModel = model
                '登録ボタン押下後の場合、ここに入る。セッションをクリアする。
                Session.Remove(CommonSessionKey.CREAGE_ENTRYPRODUCTKEY)
            Else
                _ViewModel = ViewModel
            End If

            'ライセンスセッションが切れていたらライセンスコントローラーを呼び出す
            If IsNothing(Session(CommonSessionKey.REGIST_PRODUCT_NO)) Then
                _ViewModel.ErrCode = CommonLabelLicense.MSG_REDISPLAY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                'プロダクトNoを設定
                _ViewModel.PRODUCT_CD = Session(CommonSessionKey.REGIST_PRODUCT_NO)

                '試用期間を設定
                _ViewModel.TRIAL_LIMIT = Session(CommonSessionKey.REGIST_TRIAL_LIMIT)
            End If

            '一覧情報にページ情報を設定
            CopyModelData()

            Return View(_viewResult)
        End Function
#End Region

#Region "登録処理"
        ''' <summary>
        ''' アクションメソッド(Entry)
        ''' 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        ''' 詳細については、http://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function Entry(ByVal ViewModel As EntryProductKeyViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'プロダクトキーのセッションを初期化する
            Session.Remove(CommonSessionKey.CREAGE_ENTRYPRODUCTKEY)

            'セッションからプロダクトキーを取得する
            Dim sProductKey As String = Session(CommonSessionKey.REGIST_PRODUCT_KEY)

            '--プロダクトキーチェック処理
            '入力されたプロダクトキーをハイフンで連結する
            Dim sKey As String = String.Join("-", _ViewModel.PRODUCT_KEY1, _ViewModel.PRODUCT_KEY2, _ViewModel.PRODUCT_KEY3, _ViewModel.PRODUCT_KEY4, _ViewModel.PRODUCT_KEY5)
            If sKey = sProductKey Then
                'プロダクトキーが一致した場合、登録処理をする
                AsetraLibRegist.LibRegist.registLicense(sProductKey)

                'ライセンス認証OKセッションにTrueをセット
                Session(CommonSessionKey.REGIST_OK) = True

                '登録成功ダイアログを設定
                _ViewModel.States = StatusType.SuccessCreate
            Else
                'プロダクトキーが一致しなかった場合、エラー
                _ViewModel.ErrCode = CommonLabelLicense.MSG_ERR_PRODUCT_KEY
                '_ViewModel.ErrTarget = "PRODUCT_KEY1"
            End If

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_ENTRYPRODUCTKEY) = _ViewModel

            '結果表示
            Return RedirectToAction("Index")
        End Function
#End Region

#Region "ログイン画面に遷移"
        ''' <summary>
        ''' アクションメソッド(ToLogin)
        ''' 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        ''' 詳細については、http://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function ToLogin(ByVal ViewModel As EntryProductKeyViewModel) As ActionResult
            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            'ライセンスセッションが切れていたらライセンスコントローラーを呼び出す
            If IsNothing(Session(CommonSessionKey.REGIST_PRODUCT_NO)) Then

                '結果表示
                Return RedirectToAction("Index")
            Else

                'ログイン画面に遷移
                Return RedirectToAction("Index", "Login")
            End If

        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New EntryProductKeyViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).PRODUCT_CD = _ViewModel.PRODUCT_CD
            _viewResult.ElementAt(0).PRODUCT_KEY1 = _ViewModel.PRODUCT_KEY1
            _viewResult.ElementAt(0).PRODUCT_KEY2 = _ViewModel.PRODUCT_KEY2
            _viewResult.ElementAt(0).PRODUCT_KEY3 = _ViewModel.PRODUCT_KEY3
            _viewResult.ElementAt(0).PRODUCT_KEY4 = _ViewModel.PRODUCT_KEY4
            _viewResult.ElementAt(0).PRODUCT_KEY5 = _ViewModel.PRODUCT_KEY5
            _viewResult.ElementAt(0).TRIAL_LIMIT = _ViewModel.TRIAL_LIMIT
            '_viewResult.ElementAt(0).PRODUCT_CD = "11111-22222-33333-44444-55555"
            '_viewResult.ElementAt(0).PRODUCT_KEY1 = "AAAAA"
            '_viewResult.ElementAt(0).PRODUCT_KEY2 = "BBBBB"
            '_viewResult.ElementAt(0).PRODUCT_KEY3 = "CCCCC"
            '_viewResult.ElementAt(0).PRODUCT_KEY4 = "DDDDD"
            '_viewResult.ElementAt(0).PRODUCT_KEY5 = "EEEEE"
            '_viewResult.ElementAt(0).TRIAL_LIMIT = 30
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            '_viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            '_viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).States = _ViewModel.States
        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As EntryProductKeyViewModel)
            '_Model.EMPLOYEE_CD = model.EMPLOYEE_CD
            '_Model.EMPLOYEE_NAME = model.EMPLOYEE_NAME
            '_Model.DEPARTMENT_CD = model.DEPARTMENT_CD
            '_Model.PASSWORD = model.PASSWORD
            '_Model.AUTH_DIV = model.AUTH_DIV
            '_Model.INS_DATETIME = model.INS_DATETIME
            '_Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            '_Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            '_Model.UPD_DATETIME = model.UPD_DATETIME
            '_Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            '_Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            '_Model.UPD_TASK = model.UPD_TASK
        End Sub
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