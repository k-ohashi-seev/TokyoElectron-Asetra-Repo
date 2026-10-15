Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Data.SqlClient
Imports System.Web.WebPages
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' 工事資産履歴一覧画面表示用のクラス
    ''' </summary>
    Public Class ConstAssetHistController
        Inherits PageBaseController

#Region "クラス定数"
        ''' <summary>
        ''' 工事履歴一覧チェックボックスキー
        ''' </summary>
        Private Enum ConstructionHistKey
            CONST_NO = 0
            CONST_WORK_TS = 1
            CONST_WORK_DIV = 2
        End Enum

#End Region

#Region "クラス変数"
        Private _ViewModel As ConstAssetHistViewModel = New ConstAssetHistViewModel                     'ConstAssetHist一覧表示用モデル
        Protected _viewResult As List(Of ConstAssetHistViewModel) = New List(Of ConstAssetHistViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _params As List(Of SqlParameter)                'SQLパラメータ
#End Region

#Region "一覧表示処理"
        ''' <summary>
        ''' アクションメソッド(Index)
        ''' スーパークラスの一覧表示処理を呼出します
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <param name="actionname">アクション名</param>
        ''' <returns>View表示内容</returns>
        <ValidateInput(False)>
        Public Function Index(ByVal ViewModel As ConstAssetHistViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_CONST_ASSET_HIST)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As ConstAssetHistViewModel = Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            'メッセージ一覧取得              2021/10/03 ADD nv Inoue
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '一覧表示処理
                MyBase.BaseIndex(_ViewModel)

                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            '工事データ取得
            GetTblConstructionData()

            '工事履歴データ取得
            GetTblConstructionHistData()

            '一覧情報にページ情報を設定
            CopyModelData()

            '結果表示
            Return View(_viewResult)

        End Function

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()

            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_CONST_ASSET_HIST)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()

            'クエリを返却
            MyBase._SQL = CreateSQL()

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of ConstAssetHistViewModel) = _db.ConstAssetHistView.SqlQuery(_SQL, _params.ToArray)

            '画面に表示するViewを設定
            _viewResult = query.ToList

        End Sub

#End Region

#Region "登録・更新処理"
        ''' <summary>
        ''' アクションメソッド(Create)
        ''' 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        ''' 詳細については、http://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <param name="OnCreteButton">クリックされたボタン</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        Public Function Create(ByVal ViewModel As ConstAssetHistViewModel, ByVal OnCreteButton As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理
                MyBase.BaseCreate(_ViewModel)
            Else
                '更新処理
                MyBase.BaseEdit(_ViewModel)
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeCreate()

            '親の初期処理
            MyBase.InitializeCreate()

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeCreate()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeEdit()

            '親の初期処理
            MyBase.InitializeCreate()

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeEdit()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            Return blnReturn
        End Function

        ''' <summary>
        ''' 登録処理をコンテキストに追加します
        ''' </summary>
        Protected Overrides Sub CreateQuery()
        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            Return blnReturn
        End Function

        ''' <summary>
        ''' 更新処理のコンテキストを変更します
        ''' </summary>
        Protected Overrides Sub EditQuery()
        End Sub
#End Region

#Region "削除処理"
        ''' <summary>
        ''' アクションメソッド(Delete)
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        Public Overridable Function Delete(ByVal ViewModel As ConstAssetHistViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'ViewModelをクラス変数に保持
            _ViewModel = ViewModel

            '削除処理
            MyBase.BaseDelete(_ViewModel)

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeDelete()

            '親の初期処理
            MyBase.InitializeDelete()

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        ''' <param name="model">基底モデル</param>
        Protected Overrides Sub DisposeDelete(ByRef model As BaseModel)

            MyBase.DisposeDelete(model)

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST) = _ViewModel

        End Sub

        ''' <summary>
        ''' 削除処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function DeleteChcek() As Boolean
            Return True
        End Function

        ''' <summary>
        ''' 削除処理のクエリを作成します
        ''' </summary>
        Protected Overrides Sub DeleteQuery()
            '特に無し
        End Sub
#End Region

#Region "CSV出力処理"
        ''' <summary>
        ''' CSV出力処理
        ''' </summary>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Function CsvExport(ViewModel As ConstAssetHistViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'モデルを設定
            _ViewModel = ViewModel


            '工事データ取得
            Dim ConstructionListData As ConstructionListViewModel
            ConstructionListData = GetTblConstructionData().FirstOrDefault()

            '工事履歴データ取得
            Dim ConstructionHistData As ConstructionHistViewModel
            ConstructionHistData = GetTblConstructionHistData().FirstOrDefault()

            '工事資産履歴一覧データ取得
            Dim ConstAssetHistData As List(Of ConstAssetHistViewModel)
            ConstAssetHistData = GetConstAssetHistData()

            'CSV出力クラス作成
            Dim SVClsCSVExp As New clsSV_CsvExport

            '文字コードセット
            SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

            '出力ファイル名取得
            Dim Setting As MessageRfid      '設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.ConstAssetHist.KeyDiv, CommonSettingKey.ConstAssetHist.Key1, CommonSettingKey.ConstAssetHist.Key2, CommonSettingKey.ConstAssetHist.Key3, True)
            Dim sFileNm As String = CsvHedderNm.DATA1

            '◆CSVファイル出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆CSVファイル出力先設定
            Dim sUrlFileName As String = Path.GetRandomFileName()
            Dim sDspFileName As String = String.Format(sFileNm + "_{0}.csv", Now.ToString("yyyyMMdd"))
            SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sUrlFileName)

            '◆CSV出力用リスト作成
            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'CSVヘッダー取得
            Dim sHeader As String
            'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_CONST_ASSET_HIST)
            sHeader = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_CONSTRUCTION_HISTORY_ASSET_LIST)

            'ヘッダ行挿入
            SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))
            'データ行挿入
            Dim col As String = ""
            For Each row As ConstAssetHistViewModel In ConstAssetHistData
                col = ""

                col += SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(ConstructionListData.CONST_START_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(ConstructionListData.CONST_END_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(ConstructionListData.CONST_LENT_START_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringIntYMD(ConstructionListData.CONST_LENT_END_DT))
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_CONTENTS)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.MNG_LOC_L_CD.Trim)
                Select Case ConstructionListData.CONST_STATUS
                    Case CONST_STATUS.Incomplete
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(CONST_STATUS_CSV.Incomplete)
                    Case CONST_STATUS.Complete
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(CONST_STATUS_CSV.Complete)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO8)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO9)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO10)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO11)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO12)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO13)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO14)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO15)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO16)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO17)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO18)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO19)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_INFO20)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.PREV_TEMP_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_IN_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.NOW_TEMP_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_OUT_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CUR_LENT_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CUR_TEMP_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.SITE_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.SITE_EMPLOYEE_NAME)
                If ConstructionListData.CONST_CONFIRM_TS.HasValue Then
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringLongYMDHMS(ConstructionListData.CONST_CONFIRM_TS.Value))
                Else
                    col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End If
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_CONFIRM_EMPLOYEE_CD)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_CONFIRM_EMPLOYEE_NAME)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionListData.CONST_CONFIRM_REMARK)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(CommonRfid.ToStringLongYMDHMS(ConstructionHistData.CONST_WORK_TS))
                Select Case ConstructionHistData.CONST_WORK_DIV
                    Case ConstWorkDiv.ConstRegist
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_CONST_REGIST))
                    Case ConstWorkDiv.TempLink
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_TEMP_LINK))
                    Case ConstWorkDiv.Lease
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_LEASE))
                    Case ConstWorkDiv.InMatch
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_IN_MATCH))
                    Case ConstWorkDiv.TempRegist
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_TEMP_REGIST))
                    Case ConstWorkDiv.OutMatch
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_OUT_MATCH))
                    Case ConstWorkDiv.MatchReturn
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_MATCH_RETURN))
                    Case ConstWorkDiv.Returned
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_RETURNED))
                    Case ConstWorkDiv.ConstComplete
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_CONST_COMPLETE))
                    Case ConstWorkDiv.AnotherConstLink
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ANOTHER_CONST_LINK))
                    Case ConstWorkDiv.TempRelease
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_TEMP_RELEASE))
                End Select
                Select Case ConstructionHistData.CONST_WORK_DIV
                    Case ConstWorkDiv.InMatch, ConstWorkDiv.OutMatch, ConstWorkDiv.MatchReturn
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionHistData.CONST_MATCH_DIV)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                Select Case ConstructionHistData.CONST_WORK_DIV
                    Case ConstWorkDiv.TempLink, ConstWorkDiv.Lease
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionHistData.LENT_NUM)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.MatchReturn, ConstWorkDiv.Returned
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionHistData.LEASE_NUM)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.TempRegist, ConstWorkDiv.AnotherConstLink, ConstWorkDiv.TempRelease
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionHistData.TEMP_REG_NUM)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.InMatch
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionHistData.TEMP_REG_NUM.ToString + "/" + ConstructionHistData.CONST_MATCH_TTLNUM.ToString)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.OutMatch, ConstWorkDiv.MatchReturn
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionHistData.CONST_OUT_MATCH_NUM.ToString + "/" + ConstructionHistData.CONST_MATCH_TTLNUM.ToString)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionHistData.CONST_IN_NUM_HIST)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionHistData.TEMP_NUM)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(ConstructionHistData.CONST_OUT_NUM_HIST)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NO)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.ConstRegist
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_CONST_REGIST))
                    Case ConstWorkDiv.TempLink
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_TEMP_LINK))
                    Case ConstWorkDiv.Lease
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_LEASE))
                    Case ConstWorkDiv.InMatch
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_IN_MATCH))
                    Case ConstWorkDiv.TempRegist
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_TEMP_REGIST))
                    Case ConstWorkDiv.OutMatch
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_OUT_MATCH))
                    Case ConstWorkDiv.MatchReturn
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_MATCH_RETURN))
                    Case ConstWorkDiv.Returned
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_RETURNED))
                    Case ConstWorkDiv.ConstComplete
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_CONST_COMPLETE))
                    Case ConstWorkDiv.AnotherConstLink
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_ANOTHER_CONST_LINK))
                    Case ConstWorkDiv.TempRelease
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_TEMP_RELEASE))
                End Select
                Select Case row.CONST_WORK_DIV
                    Case ConstWorkDiv.InMatch, ConstWorkDiv.OutMatch, ConstWorkDiv.MatchReturn
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_MATCH_DIV)
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CONST_MATCH_STATUS)
                Select Case row.TEMP_FLAG
                    Case TempFlg.Cd.Yes
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_CIRCLE))
                    Case TempFlg.Cd.No
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(_Message.getMessage(CommonLabel.VALUE_CROSS_MARK))
                    Case Else
                        col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
                End Select
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO1)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO2)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO3)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO4)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO5)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO6)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_INFO7)
                col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.TEMP_REMARK)

                SVClsCSVExp.p_csvDataList.Add(col)
            Next

            'CSVファイル出力
            If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

                'ファイルをダウンロードさせる
                Call FileDownload(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)

                'ダウンロードさせた後、ファイルを削除。
                System.IO.File.Delete(sFullPath + "\" + sUrlFileName)

            End If

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelAssetHistory.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     ConstAssetHistData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelAssetHistory.TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            'Return New EmptyResult()


        End Function
#End Region

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetConstAssetHistData() As List(Of ConstAssetHistViewModel)
            Dim lstConstAssetHist As List(Of ConstAssetHistViewModel)
            Dim db As SystemDBContext = New SystemDBContext
            'SQLを実行する
            lstConstAssetHist = db.ConstAssetHistView.SqlQuery(CreateSQL(), _params.ToArray).ToList

            'データを返却
            Return lstConstAssetHist
        End Function
#End Region

#Region "ファイルをダウンロードさせます。"
        ''' <summary>
        ''' ファイルをダウンロードさせます。
        ''' </summary>
        ''' <param name="FileName">ダウンロードするファイル名を指定します。</param>
        ''' <param name="MimeType">ダウンロードするファイルのMIMEタイプを指定します。</param>
        ''' <param name="sFileFullPath">ダウンロードするファイルのフルパス</param>
        Private Sub FileDownload(FileName As String, MimeType As String, ByVal sFileFullPath As String)
            'ダウンロードするファイル名
            Dim dlFileName As String

            'ファイル名が日本語の場合を考慮したダウンロードファイル名を作成
            If Request.Browser.Browser = "IE" Then
                'IEの場合はファイル名をURLエンコード
                dlFileName = HttpUtility.UrlEncode(FileName)
            Else
                'IE以外の場合はそのままでOK
                dlFileName = FileName
            End If

            'ダウンロード処理
            'Response情報クリア
            Response.ClearContent()

            'バッファリング
            Response.Buffer = True

            'HTTPヘッダー情報・MIMEタイプ設定
            Response.AddHeader("Content-Disposition", String.Format("attachment;filename={0}", dlFileName))
            Response.ContentType = MimeType

            'ファイルを書き出し
            Response.WriteFile(sFileFullPath)

            Response.Flush()
            Response.End()
        End Sub
#End Region

#Region "工事データ取得"
        ''' <summary>
        ''' 工事データ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetTblConstructionData() As List(Of ConstructionListViewModel)
            Dim lst As New List(Of ConstructionListViewModel)

            If IsNothing(Me._Message) Then
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            End If
            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstAssetHist, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  CONST.CONST_NO ")
            sbSQL.AppendLine("  , CONST.CONST_START_DT ")
            sbSQL.AppendLine("  , CONST.CONST_END_DT ")
            sbSQL.AppendLine("  , CONST.CONST_LENT_START_DT ")
            sbSQL.AppendLine("  , CONST.CONST_LENT_END_DT ")
            sbSQL.AppendLine("  , CONST.CONST_STATUS ")
            sbSQL.AppendLine("  , CONST.CONST_CONTENTS ")
            sbSQL.AppendLine("  , CONST.CONST_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.CONST_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.MNG_LOC_L_CD AS MNG_LOC_L_CD")
            sbSQL.AppendLine("  , CONST.CONST_INFO1 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO2 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO3 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO4 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO5 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO6 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO7 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO8 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO9 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO10 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO11 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO12 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO13 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO14 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO15 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO16 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO17 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO18 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO19 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO20 ")
            sbSQL.AppendLine("  , CONST.PREV_TEMP_NUM ")
            sbSQL.AppendLine("  , CONST.CONST_IN_NUM ")
            sbSQL.AppendLine("  , CONST.NOW_TEMP_NUM ")
            sbSQL.AppendLine("  , CONST.CONST_OUT_NUM ")
            sbSQL.AppendLine("  , CONST.CUR_LENT_NUM ")
            sbSQL.AppendLine("  , CONST.CUR_TEMP_NUM ")
            sbSQL.AppendLine("  , CONST.SITE_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.SITE_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_TS ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_REMARK ")
            sbSQL.AppendLine("  , CONST.TASK_TS ")
            sbSQL.AppendLine("  , CONST.INS_DATETIME ")
            sbSQL.AppendLine("  , CONST.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.UPD_DATETIME ")
            sbSQL.AppendLine("  , CONST.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.UPD_TASK ")
            sbSQL.AppendLine("  , MNGLOCL.LOC_NAME AS MngLocLName ")
            sbSQL.AppendLine("  , AUTH.UPD_FLG AS UPD_FLG ")
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM ")
            sbSQL.AppendLine("  M_CONSTRUCTION CONST ")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_SF( @EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthType) AS AUTH")
            sbSQL.AppendLine("  ON AUTH.LOC_L_CD = CONST.MNG_LOC_L_CD ")
            params.Add(New SqlParameter("@EmployeeCd", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter("@ReadAuthFlg", AuthInfo.ReadAuthFlg))
            params.Add(New SqlParameter("@UpdAuthFlg", AuthInfo.UpdAuthFlg))
            params.Add(New SqlParameter("@ReadHubAuthFlg", AuthInfo.ReadHubAuthFlg))
            params.Add(New SqlParameter("@UpdHubAuthFlg", AuthInfo.UpdHubAuthFlg))
            params.Add(New SqlParameter("@UpdAuthType", Misc.UpdAuthType.UPD_AUTH))
            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL ")
            sbSQL.AppendLine("ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND CONST.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '検索条件を設定する
            Dim selectConstNo As String = Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST_KEY).Split(",")(ConstructionHistKey.CONST_NO)

            '工事番号
            sbSQL.AppendLine("  AND CONST.CONST_NO = @CONST_NO")
            params.Add(New SqlParameter($"@CONST_NO", selectConstNo))

            'ORDER句の設定
            sbSQL.AppendLine(" ORDER BY CONST.CONST_NO")

            lst = _db.ConstructionListView.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            ViewBag.tblConstructionListData = lst
            _ViewModel.tblConstructionListData = lst

            Return lst
        End Function
#End Region

#Region "工事履歴データ取得"
        ''' <summary>
        ''' 工事履歴データ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetTblConstructionHistData() As List(Of ConstructionHistViewModel)
            Dim lst As New List(Of ConstructionHistViewModel)

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  CONST.CONST_NO ")
            sbSQL.AppendLine("  , CONST.CONST_START_DT ")
            sbSQL.AppendLine("  , CONST.CONST_END_DT ")
            sbSQL.AppendLine("  , CONST.CONST_LENT_START_DT ")
            sbSQL.AppendLine("  , CONST.CONST_LENT_END_DT ")
            sbSQL.AppendLine("  , CONST.CONST_STATUS ")
            sbSQL.AppendLine("  , CONST.CONST_CONTENTS ")
            sbSQL.AppendLine("  , CONST.CONST_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.CONST_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.MNG_LOC_L_CD")
            sbSQL.AppendLine("  , CONST.CONST_INFO1 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO2 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO3 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO4 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO5 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO6 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO7 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO8 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO9 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO10 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO11 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO12 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO13 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO14 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO15 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO16 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO17 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO18 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO19 ")
            sbSQL.AppendLine("  , CONST.CONST_INFO20 ")
            sbSQL.AppendLine("  , CONST.PREV_TEMP_NUM ")
            sbSQL.AppendLine("  , CONST.CONST_IN_NUM ")
            sbSQL.AppendLine("  , CONST.NOW_TEMP_NUM ")
            sbSQL.AppendLine("  , CONST.CONST_OUT_NUM ")
            sbSQL.AppendLine("  , CONST.CUR_LENT_NUM ")
            sbSQL.AppendLine("  , CONST.CUR_TEMP_NUM ")
            sbSQL.AppendLine("  , CONST.SITE_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.SITE_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_TS ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CONST.CONST_CONFIRM_REMARK ")
            sbSQL.AppendLine("  , CLOG.CONST_WORK_TS ")
            sbSQL.AppendLine("  , CLOG.CONST_WORK_DIV ")
            sbSQL.AppendLine("  , CLOG.CONST_MATCH_DIV ")
            sbSQL.AppendLine("  , CLOG.LENT_NUM ")
            sbSQL.AppendLine("  , CLOG.LEASE_NUM ")
            sbSQL.AppendLine("  , CLOG.TEMP_REG_NUM ")
            sbSQL.AppendLine("  , CLOG.CONST_IN_MATCH_NUM ")
            sbSQL.AppendLine("  , CLOG.CONST_OUT_MATCH_NUM ")
            sbSQL.AppendLine("  , CLOG.CONST_IN_NUM AS CONST_IN_NUM_HIST ")
            sbSQL.AppendLine("  , CLOG.CONST_MATCH_TTLNUM")
            sbSQL.AppendLine("  , CLOG.TEMP_NUM ")
            sbSQL.AppendLine("  , CLOG.CONST_OUT_NUM AS CONST_OUT_NUM_HIST ")
            sbSQL.AppendLine("  , CLOG.INS_DATETIME ")
            sbSQL.AppendLine("  , CLOG.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CLOG.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CLOG.UPD_DATETIME ")
            sbSQL.AppendLine("  , CLOG.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , CLOG.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , CLOG.UPD_TASK")
            sbSQL.AppendLine("  , MNGLOCL.LOC_NAME As MngLocLName ")
            sbSQL.AppendLine("  , CONCAT(CONST.CONST_NO,',',CLOG.CONST_WORK_TS,',',CLOG.CONST_WORK_DIV) AS KEY_VALUE ")
            sbSQL.AppendLine("  , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("FROM M_CONSTRUCTION CONST ")
            sbSQL.AppendLine("INNER JOIN T_CONST_LOG CLOG ")
            sbSQL.AppendLine("ON CONST.CONST_NO = CLOG.CONST_NO ")

            sbSQL.AppendLine("LEFT JOIN M_LOCATION MNGLOCL ")
            sbSQL.AppendLine("ON MNGLOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
            sbSQL.AppendLine("AND CONST.MNG_LOC_L_CD = MNGLOCL.LOC_L_CD")

            sbSQL.AppendLine("WHERE ")
            sbSQL.AppendLine("  1 = 1 ")

            '検索条件を設定する
            Dim selectConstNo As String = Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST_KEY).Split(",")(ConstructionHistKey.CONST_NO)
            Dim selectConstWorkTs As String = Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST_KEY).Split(",")(ConstructionHistKey.CONST_WORK_TS)
            Dim selectConstWorkDiv As String = Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST_KEY).Split(",")(ConstructionHistKey.CONST_WORK_DIV)

            sbSQL.AppendLine("  AND CLOG.CONST_NO = @CONST_NO")
            sbSQL.AppendLine("  AND CLOG.CONST_WORK_TS = @CONST_WORK_TS")
            sbSQL.AppendLine("  AND CLOG.CONST_WORK_DIV = @CONST_WORK_DIV")
            params.Add(New SqlParameter($"@CONST_NO", selectConstNo))
            params.Add(New SqlParameter($"@CONST_WORK_TS", selectConstWorkTs))
            params.Add(New SqlParameter($"@CONST_WORK_DIV", selectConstWorkDiv))

            lst = _db.ConstructionHistView.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            ViewBag.tblConstructionHistData = lst
            _ViewModel.tblConstructionHistData = lst

            Return lst
        End Function
#End Region

#Region "一覧データ取得用SQLを作成する"
        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String

            If IsNothing(Me._Message) Then
                Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            End If
            '権限情報の取得
            Dim AuthInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.ConstAssetHist, _LoginInfo)

            Dim sbSQL As StringBuilder = New StringBuilder
            _params = New List(Of SqlParameter)

            '一覧表示表用SQL
            sbSQL.AppendLine("  SELECT ")
            sbSQL.AppendLine("    CALOG.ASSET_NO ")
            sbSQL.AppendLine("    , ASSET.ASSET_NAME ")
            sbSQL.AppendLine("    , CLOG.CONST_WORK_DIV ")
            sbSQL.AppendLine("    , CALOG.CONST_MATCH_DIV ")
            sbSQL.AppendLine("    , CALOG.CONST_MATCH_STATUS ")
            sbSQL.AppendLine("    , CALOG.TEMP_FLAG ")
            sbSQL.AppendLine("    , TEMP.TEMP_INFO1 ")
            sbSQL.AppendLine("    , TEMP.TEMP_INFO2 ")
            sbSQL.AppendLine("    , TEMP.TEMP_INFO3 ")
            sbSQL.AppendLine("    , TEMP.TEMP_INFO4 ")
            sbSQL.AppendLine("    , TEMP.TEMP_INFO5 ")
            sbSQL.AppendLine("    , TEMP.TEMP_INFO6 ")
            sbSQL.AppendLine("    , TEMP.TEMP_INFO7 ")
            sbSQL.AppendLine("    , TEMP.TEMP_REMARK ")
            sbSQL.AppendLine("    , CALOG.INS_DATETIME ")
            sbSQL.AppendLine("    , CALOG.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("    , CALOG.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("    , CALOG.UPD_DATETIME ")
            sbSQL.AppendLine("    , CALOG.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("    , CALOG.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("    , CALOG.UPD_TASK ")
            sbSQL.AppendLine("    , COUNT(*) OVER () AS TotalCount ")
            sbSQL.AppendLine("  FROM ")
            sbSQL.AppendLine("  (SELECT * FROM ")
            sbSQL.AppendLine("     ( ")
            sbSQL.AppendLine("        SELECT ")
            sbSQL.AppendLine("          * ")
            sbSQL.AppendLine("          , row_number() over (PARTITION BY ASSET_NO ORDER BY CONST_WORK_TS DESC) AS number ")
            sbSQL.AppendLine("        FROM T_CONST_ASSET_LOG ")
            sbSQL.AppendLine("        WHERE CONST_NO = @CONST_NO ")
            sbSQL.AppendLine("        AND CONST_WORK_TS <= @CONST_WORK_TS ")
            sbSQL.AppendLine("     ) AS CALOG ")
            sbSQL.AppendLine("     WHERE number = 1) AS CALOG ")
            sbSQL.AppendLine("  INNER JOIN M_CONSTRUCTION CONST ")
            sbSQL.AppendLine("  ON CONST.CONST_NO = CALOG.CONST_NO ")
            sbSQL.AppendLine("  INNER JOIN T_CONST_LOG CLOG ")
            sbSQL.AppendLine("  ON CLOG.CONST_NO = CALOG.CONST_NO ")
            sbSQL.AppendLine("  AND CLOG.CONST_WORK_TS = CALOG.CONST_WORK_TS ")
            sbSQL.AppendLine("  LEFT JOIN M_ASSET ASSET ")
            sbSQL.AppendLine("  ON ASSET.ASSET_NO = CALOG.ASSET_NO ")
            sbSQL.AppendLine("  LEFT JOIN T_TEMPORARY TEMP ")
            sbSQL.AppendLine("  ON TEMP.TEMP_ID = CALOG.TEMP_ID ")

            '検索条件を設定する
            Dim selectConstNo As String = Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST_KEY).Split(",")(ConstructionHistKey.CONST_NO)
            Dim selectConstWorkTs As String = Session(CommonSessionKey.CREAGE_CONST_ASSET_HIST_KEY).Split(",")(ConstructionHistKey.CONST_WORK_TS)

            _params.Add(New SqlParameter($"@CONST_NO", selectConstNo))
            _params.Add(New SqlParameter($"@CONST_WORK_TS", selectConstWorkTs))

            sbSQL.AppendLine(" ORDER BY CALOG.ASSET_NO")

            Return sbSQL.ToString()
        End Function
#End Region

#Region "CSVヘッダー取得"
        ''' <summary>
        ''' CSVヘッダー取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCSVHeader(prmFunctionId As String) As String
            Dim csvHeader As String = String.Empty
            Dim wordList As New List(Of String)
            Dim csvHeaderWord As List(Of String) = Dao.ToolCSVHeader.GetCSVHeaderOrder(prmFunctionId)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            For Each word As String In csvHeaderWord
                wordList.Add(Me._Message.getMessage(word))
            Next

            csvHeader = String.Join(",", wordList)

            Return csvHeader
        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New ConstAssetHistViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).GridInfoTop = ToolColSet.GetGridInfo(_db, DispId.CONST_ASSET_HIST_TOP)
            _viewResult.ElementAt(0).FixColCntTop = _db.ColSet.Find(DispId.CONST_ASSET_HIST_TOP).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfoMiddle = ToolColSet.GetGridInfo(_db, DispId.CONST_ASSET_HIST_MIDDLE)
            _viewResult.ElementAt(0).FixColCntMiddle = _db.ColSet.Find(DispId.CONST_ASSET_HIST_MIDDLE).FIX_COL_CNT
            _viewResult.ElementAt(0).GridInfoBottom = ToolColSet.GetGridInfo(_db, DispId.CONST_ASSET_HIST_BOTTOM)
            _viewResult.ElementAt(0).FixColCntBottom = _db.ColSet.Find(DispId.CONST_ASSET_HIST_BOTTOM).FIX_COL_CNT
        End Sub
#End Region

    End Class
End Namespace