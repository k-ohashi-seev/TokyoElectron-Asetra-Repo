Imports Asetra.Models
Imports Asetra.Misc
Imports System.IO
Imports System.Data.Entity.Infrastructure
Imports System.Web.WebPages
Imports System.Data.SqlClient
Imports System.Net
Imports Asetra.Dao

Namespace Controllers

    ''' <summary>
    ''' ゲートマスタメンテナンス画面表示用のクラス
    ''' </summary>
    Public Class GateController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)


#Region "クラス定数"
        '電波強度 最大(SpeedWay)
        Private Const SW_ANTENNA_MAX_POWER As Integer = 30
        '電波強度 最小(SpeedWay)
        Private Const SW_ANTENNA_MIN_POWER As Integer = 10
        '感度 最大(SpeedWay)
        Private Const SW_SENSITIVITY_MAX_POWER As Integer = -30
        '感度 最小(SpeedWay)
        Private Const SW_SENSITIVITY_MIN_POWER As Integer = -80
#End Region

#Region "クラス変数"
        Private _Model As GateModel = New GateModel                                 'Gateモデル
        Private _ViewModel As GateViewModel = New GateViewModel                     'Gate一覧表示用モデル
        Protected _viewResult As List(Of GateViewModel) = New List(Of GateViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
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
        Public Function Index(ByVal ViewModel As GateViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_GATE)
            End If

            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As GateViewModel = Session(CommonSessionKey.CREAGE_GATE)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            '20220328 ADD Start Kawamoto v2.1
            '使用ゲート取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim settingGateDiv As String = Dao.ToolSetting.GetUseGate(Setting)
            _ViewModel.GATE_DIV = settingGateDiv
            '20220328 ADD End Kawamoto v2.1

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

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            'チャンネルコンボボックスを設定
            ChannelDropDownListCreate()

            '有効無効コンボボックスを設定
            ValidDropDownListCreate()

            '20220328 ADD Start Kawamoto v2.1
            'Searchモードコンボボックスを設定
            SearchModeDropDownListCreate()

            'Readerモードコンボボックスを設定
            ReaderModeDropDownListCreate()
            '20220328 ADD End Kawamoto v2.1

            '20220511 ADD Start Kawamoto
            '機種コンボボックスを設定
            DeviceTypeDropDownListCreate()

            'GPIO機器コンボボックスを設定
            GpioDeviceDropDownListCreate()

            'GPOポートコンボボックスを設定
            GpoPortDropDownListCreate()
            '20220511 ADD End Kawamoto

            Dim gateId As String = DispId.GATE_WAVE_GATE
            If settingGateDiv = GateDiv.Cd.SW Then
                gateId = DispId.GATE_SPEED_WAY
            End If

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(gateId)

            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If

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
                Session.Remove(CommonSessionKey.CREAGE_GATE)
            End If

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            Dim sbSQL As StringBuilder = New StringBuilder

            '一覧表示表用SQL
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  GATE.GATE_CD ")
            sbSQL.AppendLine("  , GATE.GATE_NAME ")
            sbSQL.AppendLine("  , GATE.GATE_IPADDRESS ")
            sbSQL.AppendLine("  , GATE.GATE_CHANNEL ")
            sbSQL.AppendLine("  , GATE.PATLITE_VALID_FLAG ")
            sbSQL.AppendLine("  , GATE.GATE_VALID_FLAG ")
            sbSQL.AppendLine("  , THERMO1.THERMO_VALID_FLAG As THERMO1_VALID_FLAG")
            sbSQL.AppendLine("  , THERMO2.THERMO_VALID_FLAG As THERMO2_VALID_FLAG")
            sbSQL.AppendLine("  , GATE.INS_DATETIME ")
            sbSQL.AppendLine("  , GATE.INS_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , GATE.INS_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , GATE.UPD_DATETIME ")
            sbSQL.AppendLine("  , GATE.UPD_EMPLOYEE_CD ")
            sbSQL.AppendLine("  , GATE.UPD_EMPLOYEE_NAME ")
            sbSQL.AppendLine("  , GATE.UPD_TASK ")
            sbSQL.AppendLine("   ,COUNT(*) OVER () AS TotalCount ")
            '20220328 ADD Start Kawamoto v2.1
            sbSQL.AppendLine("  , GATE.GATE_DIV ")
            sbSQL.AppendLine("  , GATE.CAMERA_VALID_FLAG ")
            sbSQL.AppendLine("  , GATE.DEVICE_TYPE ")               '20220511 ADD Kawamoto
            sbSQL.AppendLine("  , GATE.SENSITIVITY ")
            sbSQL.AppendLine("  , GATE.SESSION ")
            sbSQL.AppendLine("  , FORMAT(GATE.FREQUENCY1, '0.0###') AS FREQUENCY1 ")
            sbSQL.AppendLine("  , FORMAT(GATE.FREQUENCY2, '0.0###') AS FREQUENCY2 ")
            sbSQL.AppendLine("  , FORMAT(GATE.FREQUENCY3, '0.0###') AS FREQUENCY3 ")
            sbSQL.AppendLine("  , FORMAT(GATE.FREQUENCY4, '0.0###') AS FREQUENCY4 ")
            sbSQL.AppendLine("  , GATE.SEARCH_MODE ")
            sbSQL.AppendLine("  , GATE.READER_MODE ")
            sbSQL.AppendLine("  , GATE.TAG_POPULATION ")
            sbSQL.AppendLine("  , GATE.GPIO_DEVICE ")               '20220511 ADD Kawamoto
            sbSQL.AppendLine("  , GATE.DEVICE_NAME_C ")
            sbSQL.AppendLine("  , GATE.BIT_POS_C ")
            sbSQL.AppendLine("  , GATE.GPO_PORT_C ")                '20220511 ADD Kawamoto
            sbSQL.AppendLine("  , GATE.DEVICE_NAME_P ")
            sbSQL.AppendLine("  , GATE.BIT_POS_P ")
            sbSQL.AppendLine("  , GATE.GPO_PORT_P ")                '20220511 ADD Kawamoto
            sbSQL.AppendLine("  , GATE.UNCONDITIONAL_C ")
            sbSQL.AppendLine("  , GATE.UNCONDITIONAL_P ")
            sbSQL.AppendLine("  , SUBSTRING(")
            sbSQL.AppendLine($"    CASE WHEN GATE.FREQUENCY1 IS NOT NULL AND GATE.FREQUENCY1 <> 0 THEN ',' + FORMAT(GATE.FREQUENCY1, '0.0###') + 'MHz' ELSE '' END + ")
            sbSQL.AppendLine($"    CASE WHEN GATE.FREQUENCY2 IS NOT NULL AND GATE.FREQUENCY2 <> 0 THEN ',' + FORMAT(GATE.FREQUENCY2, '0.0###') + 'MHz' ELSE '' END + ")
            sbSQL.AppendLine($"    CASE WHEN GATE.FREQUENCY3 IS NOT NULL AND GATE.FREQUENCY3 <> 0 THEN ',' + FORMAT(GATE.FREQUENCY3, '0.0###') + 'MHz' ELSE '' END + ")
            sbSQL.AppendLine($"    CASE WHEN GATE.FREQUENCY4 IS NOT NULL AND GATE.FREQUENCY4 <> 0 THEN ',' + FORMAT(GATE.FREQUENCY4, '0.0###') + 'MHz' ELSE '' END ")
            sbSQL.AppendLine("  , 2, 100) AS DISP_FREQUENCY")       'substring(X,2,100)は先頭のカンマを除去する処理(100に意味はない。Xより長ければ良い)
            sbSQL.AppendLine("  , ANTENNA.ANTENNA_RF_POWER AS DISP_ANTENNA_POWER")
            '20220328 ADD End Kawamoto v2.1
            sbSQL.AppendLine("FROM M_GATE GATE")
            sbSQL.AppendLine("LEFT JOIN M_GATE_THERMO THERMO1")
            sbSQL.AppendLine("ON THERMO1.GATE_CD = GATE.GATE_CD")
            sbSQL.AppendLine("AND THERMO1.THERMO_NO = 1")
            sbSQL.AppendLine($"AND GATE.GATE_DIV = {_ViewModel.GATE_DIV} ")
            sbSQL.AppendLine("LEFT JOIN M_GATE_THERMO THERMO2")
            sbSQL.AppendLine("ON THERMO2.GATE_CD = GATE.GATE_CD")
            sbSQL.AppendLine("AND THERMO2.THERMO_NO = 2")
            sbSQL.AppendLine($"AND GATE.GATE_DIV = {_ViewModel.GATE_DIV} ")
            '20220328 ADD Start Kawamoto v2.1
            sbSQL.AppendLine("LEFT JOIN M_GATE_ANTENNA ANTENNA")
            sbSQL.AppendLine("ON ANTENNA.GATE_DIV = GATE.GATE_DIV")
            sbSQL.AppendLine("AND ANTENNA.GATE_CD = GATE.GATE_CD")
            sbSQL.AppendLine("AND ANTENNA.ANTENNA_NO = 1")
            '20220328 ADD End Kawamoto v2.1
            sbSQL.AppendLine("WHERE ")
            '20220328 UPD Start Kawamoto v2.1
            'sbSQL.AppendLine("  1 = 1 ")
            sbSQL.AppendLine($"  GATE.GATE_DIV = {_ViewModel.GATE_DIV} ")
            '20220328 UPD End Kawamoto v2.1

            sbSQL.AppendLine(" ORDER BY GATE.GATE_CD")

            'クエリを返却
            MyBase._SQL = sbSQL.ToString

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()

            'SQLを実行
            Dim query As DbSqlQuery(Of GateViewModel) = _db.GateView.SqlQuery(_SQL, _paramSqlList.ToArray)

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
        <ValidateInput(False)>
        Public Function Create(ByVal ViewModel As GateViewModel, ByVal OnCreteButton As String) As ActionResult
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

            ''アンテナ有効フラグが無効の時、アンテナ電波強度はクリアする
            'If _ViewModel.ANTENNA1_VALID_FLAG = ValidFlg.Cd.Invalid Then
            '    _ViewModel.ANTENNA1_RF_POWER = String.Empty
            'End If
            'If _ViewModel.ANTENNA2_VALID_FLAG = ValidFlg.Cd.Invalid Then
            '    _ViewModel.ANTENNA2_RF_POWER = String.Empty
            'End If
            'If _ViewModel.ANTENNA3_VALID_FLAG = ValidFlg.Cd.Invalid Then
            '    _ViewModel.ANTENNA3_RF_POWER = String.Empty
            'End If
            'If _ViewModel.ANTENNA4_VALID_FLAG = ValidFlg.Cd.Invalid Then
            '    _ViewModel.ANTENNA4_RF_POWER = String.Empty
            'End If

            If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then
                '登録処理
                MyBase.BaseCreate(_ViewModel)
            Else
                '更新処理
                MyBase.BaseEdit(_ViewModel)
            End If

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
                If (ViewModel.OnCreteButton = CommonConst.KEY_ON_CREATE_CLICK) Then

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelGate.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_CREATE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_GATE_CD) & ": " &
                                                     _ViewModel.GATE_CD & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_GATE_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.GATE_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelGate.TITLE))

                    _ViewModel.States = StatusType.SuccessCreate
                Else

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelGate.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_GATE_CD) & ": " &
                                                     _ViewModel.GATE_CD & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_GATE_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.GATE_NAME).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelGate.TITLE))

                    _ViewModel.States = StatusType.SuccessEdit
                End If
            Else
                CopyModelData()
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

            'モデルを登録用モデルにコピー
            CopyViewModeltoModel(_ViewModel)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeCreate()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_GATE) = _ViewModel

        End Sub

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeEdit()

            '親の初期処理
            MyBase.InitializeCreate()

            'モデルを登録用モデルにコピー
            CopyViewModeltoModel(_ViewModel)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeEdit()

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_GATE) = _ViewModel

        End Sub

        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
            Dim db As New SystemDBContext
            Dim blnReturn As Boolean = True

            'ゲートコードが登録済みかチェックする
            '20220328 UPD Start Kawamoto v2.1
            'Dim Gate As GateModel = _db.Gate.Find(_Model.GATE_CD)
            Dim Gate As GateModel = _db.Gate.Find(_Model.GATE_DIV, _Model.GATE_CD)
            '20220328 UPD End Kawamoto v2.1

            '重複エラーの場合、エラー情報を保持する
            If Not IsNothing(Gate) Then
                ModelState.AddModelError("GATE_CD", CommonLabel.MSG_ERR_DATAJUUFUKU)
                Return False
            End If

            '登録・更新時のエラーチェック
            If Not ErrCheck() Then
                Return False
            End If

            Return True

        End Function

        ''' <summary>
        ''' 登録処理をコンテキストに追加します
        ''' </summary>
        Protected Overrides Sub CreateQuery()
            '登録日時・登録社員コード・登録社員名・登録プロセスを登録
            _Model.INS_DATETIME = CommonRfid.getNowDate
            _Model.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelGate.TITLE)

            'ゲートマスタ登録
            _db.Gate.Add(_Model)

            Try
                If _ViewModel.GATE_DIV = GateDiv.Cd.WG Then
                    'WaveGate用
                    'ゲートアンテナマスタ登録
                    'アンテナ１
                    GateAntennaInsert(1, _ViewModel.ANTENNA1_VALID_FLAG, _ViewModel.ANTENNA1_RF_POWER)
                    'アンテナ２
                    GateAntennaInsert(2, _ViewModel.ANTENNA2_VALID_FLAG, _ViewModel.ANTENNA2_RF_POWER)
                    'アンテナ３
                    GateAntennaInsert(3, _ViewModel.ANTENNA3_VALID_FLAG, _ViewModel.ANTENNA3_RF_POWER)
                    'アンテナ４
                    GateAntennaInsert(4, _ViewModel.ANTENNA4_VALID_FLAG, _ViewModel.ANTENNA4_RF_POWER)
                    'サーモ１
                    GateThermoInsert(1, _ViewModel.THERMO1_VALID_FLAG)
                    'サーモ２
                    GateThermoInsert(2, _ViewModel.THERMO2_VALID_FLAG)
                Else
                    '20220328 ADD Start Kawamoto v2.1
                    'SpeedWay用
                    'ゲートアンテナマスタ登録
                    'アンテナ１
                    GateAntennaInsert(1, _ViewModel.ANTENNA1_VALID_FLAG, _ViewModel.ANTENNA1_RF_POWER)
                    'アンテナ２
                    GateAntennaInsert(2, _ViewModel.ANTENNA2_VALID_FLAG, _ViewModel.ANTENNA1_RF_POWER)
                    'アンテナ３
                    GateAntennaInsert(3, _ViewModel.ANTENNA3_VALID_FLAG, _ViewModel.ANTENNA1_RF_POWER)
                    'アンテナ４
                    GateAntennaInsert(4, _ViewModel.ANTENNA4_VALID_FLAG, _ViewModel.ANTENNA1_RF_POWER)
                    '20220328 ADD End Kawamoto v2.1
                End If

            Catch ex As Exception
                ModelState.AddModelError("", CommonLabel.MSG_ERR_SYSTEM)
            End Try

        End Sub

        ''' <summary>
        ''' 更新処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function EditChcek() As Boolean
            Dim db As New SystemDBContext

            'DBのデータを取得する
            '20220328 UPD Start Kawamoto v2.1
            'Dim model As GateModel = db.Gate.Find(_Model.GATE_CD)
            Dim model As GateModel = db.Gate.Find(_Model.GATE_DIV, _Model.GATE_CD)
            '20220328 UPD End Kawamoto v2.1

            '他端末で更新されているかチェックする
            If _Model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") <> model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss") Then
                'モデルとDBの更新日付が異なっている場合エラー
                ModelState.AddModelError("UPD_DATETIME", CommonErrorMsg.OTHER_MACHINE_UPDATE)
                Return False
            End If

            '登録・更新時のエラーチェック
            If Not ErrCheck() Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' 更新処理のコンテキストを変更します
        ''' </summary>
        Protected Overrides Sub EditQuery()
            '更新日時・更新社員コード・更新社員名・更新プロセスを登録
            _Model.UPD_DATETIME = CommonRfid.getNowDate
            _Model.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            _Model.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            _Model.UPD_TASK = Me._Message.getMessage(CommonLabelGate.TITLE)

            'アタッチする
            _db.Entry(_Model).State = Entity.EntityState.Modified

            'ゲートアンテナマスタ・サーモセンサマスタ更新
            If ModelState.IsValid Then
                Try
                    If _ViewModel.GATE_DIV = GateDiv.Cd.WG Then
                        'WaveGate用
                        'アンテナ１
                        GateAntennaUpdate(1, _ViewModel.ANTENNA1_VALID_FLAG, _ViewModel.ANTENNA1_RF_POWER)
                        'アンテナ２
                        GateAntennaUpdate(2, _ViewModel.ANTENNA2_VALID_FLAG, _ViewModel.ANTENNA2_RF_POWER)
                        'アンテナ３
                        GateAntennaUpdate(3, _ViewModel.ANTENNA3_VALID_FLAG, _ViewModel.ANTENNA3_RF_POWER)
                        'アンテナ４
                        GateAntennaUpdate(4, _ViewModel.ANTENNA4_VALID_FLAG, _ViewModel.ANTENNA4_RF_POWER)
                        'サーモ１
                        GateThermoUpdate(1, _ViewModel.THERMO1_VALID_FLAG)
                        'サーモ２
                        GateThermoUpdate(2, _ViewModel.THERMO2_VALID_FLAG)
                    Else
                        '20220328 ADD Start Kawamoto v2.1
                        'SpeedWay用
                        'アンテナ１
                        GateAntennaUpdate(1, _ViewModel.ANTENNA1_VALID_FLAG, _ViewModel.ANTENNA1_RF_POWER)
                        'アンテナ２
                        GateAntennaUpdate(2, _ViewModel.ANTENNA2_VALID_FLAG, _ViewModel.ANTENNA1_RF_POWER)
                        'アンテナ３
                        GateAntennaUpdate(3, _ViewModel.ANTENNA3_VALID_FLAG, _ViewModel.ANTENNA1_RF_POWER)
                        'アンテナ４
                        GateAntennaUpdate(4, _ViewModel.ANTENNA4_VALID_FLAG, _ViewModel.ANTENNA1_RF_POWER)
                        '20220328 ADD End Kawamoto v2.1
                    End If

                Catch ex As Exception
                    ModelState.AddModelError("", CommonLabel.MSG_ERR_SYSTEM)
                End Try
            End If
        End Sub

        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="ID">ID</param>
        ''' <returns>View表示内容</returns>
        Public Function GetEditData(ByVal ID As String, ByVal Div As String) As JsonResult
            Dim db As New SystemDBContext

            'IDをキーにデータを取得する
            '20220328 UPD Start Kawamoto v2.1
            'Dim tbl As DataTable = Dao.ToolGate.GetGateData(ID)
            Dim tbl As DataTable = Dao.ToolGate.GetGateData(Div, ID)
            '20220328 UPD End Kawamoto v2.1

            '取得したデータをモデルにコピーする
            Dim model As GateViewModel = New GateViewModel
            model.GATE_CD = tbl.Rows(0).Item("GATE_CD").ToString
            model.GATE_NAME = tbl.Rows(0).Item("GATE_NAME").ToString
            model.GATE_IPADDRESS = tbl.Rows(0).Item("GATE_IPADDRESS").ToString
            model.PATLITE_VALID_FLAG = tbl.Rows(0).Item("PATLITE_VALID_FLAG").ToString
            If Not CommonRfid.ToStringNvl(tbl.Rows(0).Item("GATE_CHANNEL")) = String.Empty Then
                model.GATE_CHANNEL = CInt(tbl.Rows(0).Item("GATE_CHANNEL"))
            End If
            model.GATE_VALID_FLAG = tbl.Rows(0).Item("GATE_VALID_FLAG").ToString
            model.ANTENNA1_VALID_FLAG = tbl.Rows(0).Item("ANTENNA1_VALID_FLAG").ToString
            model.ANTENNA2_VALID_FLAG = tbl.Rows(0).Item("ANTENNA2_VALID_FLAG").ToString
            model.ANTENNA3_VALID_FLAG = tbl.Rows(0).Item("ANTENNA3_VALID_FLAG").ToString
            model.ANTENNA4_VALID_FLAG = tbl.Rows(0).Item("ANTENNA4_VALID_FLAG").ToString
            model.ANTENNA1_RF_POWER = tbl.Rows(0).Item("ANTENNA1_RF_POWER").ToString
            model.ANTENNA2_RF_POWER = tbl.Rows(0).Item("ANTENNA2_RF_POWER").ToString
            model.ANTENNA3_RF_POWER = tbl.Rows(0).Item("ANTENNA3_RF_POWER").ToString
            model.ANTENNA4_RF_POWER = tbl.Rows(0).Item("ANTENNA4_RF_POWER").ToString
            model.THERMO1_VALID_FLAG = tbl.Rows(0).Item("THERMO1_VALID_FLAG").ToString
            model.THERMO2_VALID_FLAG = tbl.Rows(0).Item("THERMO2_VALID_FLAG").ToString
            model.INS_DATETIME = CDate(tbl.Rows(0).Item("INS_DATETIME"))
            model.INS_EMPLOYEE_CD = tbl.Rows(0).Item("INS_EMPLOYEE_CD").ToString
            model.INS_EMPLOYEE_NAME = tbl.Rows(0).Item("INS_EMPLOYEE_NAME").ToString
            model.UPD_DATETIME = CDate(tbl.Rows(0).Item("UPD_DATETIME"))
            model.UPD_EMPLOYEE_CD = tbl.Rows(0).Item("UPD_EMPLOYEE_CD").ToString
            model.UPD_EMPLOYEE_NAME = tbl.Rows(0).Item("UPD_EMPLOYEE_NAME").ToString
            model.UPD_TASK = tbl.Rows(0).Item("UPD_TASK").ToString
            model.InsDatetimeString = model.INS_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")
            model.UpdDatetimeString = model.UPD_DATETIME.Value.ToString("yyyy/MM/dd HH:mm:ss")

            '20220328 ADD Start Kawamoto v2.1
            model.GATE_DIV = CommonRfid.ToStringNvl(tbl.Rows(0).Item("GATE_DIV"))
            model.CAMERA_VALID_FLAG = CommonRfid.ToStringNvl(tbl.Rows(0).Item("CAMERA_VALID_FLAG"))
            model.SENSITIVITY = CommonRfid.ToStringNvl(tbl.Rows(0).Item("SENSITIVITY"))
            model.SESSION = CommonRfid.ToStringNvl(tbl.Rows(0).Item("SESSION"))
            model.FREQUENCY1 = CommonRfid.FormatFrequency(CommonRfid.ToStringNvl(tbl.Rows(0).Item("FREQUENCY1")))
            model.FREQUENCY2 = CommonRfid.FormatFrequency(CommonRfid.ToStringNvl(tbl.Rows(0).Item("FREQUENCY2")))
            model.FREQUENCY3 = CommonRfid.FormatFrequency(CommonRfid.ToStringNvl(tbl.Rows(0).Item("FREQUENCY3")))
            model.FREQUENCY4 = CommonRfid.FormatFrequency(CommonRfid.ToStringNvl(tbl.Rows(0).Item("FREQUENCY4")))
            If Not CommonRfid.ToStringNvl(tbl.Rows(0).Item("SEARCH_MODE")) = String.Empty Then
                model.SEARCH_MODE = Convert.ToInt32(CommonRfid.ToStringNvl(tbl.Rows(0).Item("SEARCH_MODE")))
            End If
            If Not CommonRfid.ToStringNvl(tbl.Rows(0).Item("READER_MODE")) = String.Empty Then
                model.READER_MODE = Convert.ToInt32(CommonRfid.ToStringNvl(tbl.Rows(0).Item("READER_MODE")))
            End If
            model.TAG_POPULATION = CommonRfid.ToStringNvl(tbl.Rows(0).Item("TAG_POPULATION"))
            model.DEVICE_NAME_C = CommonRfid.ToStringNvl(tbl.Rows(0).Item("DEVICE_NAME_C"))
            model.BIT_POS_C = CommonRfid.ToStringNvl(tbl.Rows(0).Item("BIT_POS_C"))
            model.DEVICE_NAME_P = CommonRfid.ToStringNvl(tbl.Rows(0).Item("DEVICE_NAME_P"))
            model.BIT_POS_P = CommonRfid.ToStringNvl(tbl.Rows(0).Item("BIT_POS_P"))
            model.UNCONDITIONAL_P = CommonRfid.ToStringNvl(tbl.Rows(0).Item("UNCONDITIONAL_P"))
            model.UNCONDITIONAL_C = CommonRfid.ToStringNvl(tbl.Rows(0).Item("UNCONDITIONAL_C"))
            '20220328 ADD End Kawamoto v2.1
            '20220511 ADD Start Kawamoto
            model.DEVICE_TYPE = CommonRfid.ToStringNvl(tbl.Rows(0).Item("DEVICE_TYPE"))
            model.GPIO_DEVICE = CommonRfid.ToStringNvl(tbl.Rows(0).Item("GPIO_DEVICE"))
            If Not CommonRfid.ToStringNvl(tbl.Rows(0).Item("GPO_PORT_C")) = String.Empty Then
                model.GPO_PORT_C = Convert.ToInt32(CommonRfid.ToStringNvl(tbl.Rows(0).Item("GPO_PORT_C")))
            End If
            If Not CommonRfid.ToStringNvl(tbl.Rows(0).Item("GPO_PORT_P")) = String.Empty Then
                model.GPO_PORT_P = Convert.ToInt32(CommonRfid.ToStringNvl(tbl.Rows(0).Item("GPO_PORT_P")))
            End If
            '20220511 ADD End Kawamoto

            Return Json(model)

        End Function

#End Region

#Region "削除処理"
        ''' <summary>
        ''' アクションメソッド(Delete)
        ''' </summary>
        ''' <param name="ViewModel">モデル</param>
        ''' <returns>View表示内容</returns>
        <HttpPost()>
        <ValidateAntiForgeryToken()>
        <ValidateInput(False)>
        Public Overridable Function Delete(ByVal ViewModel As GateViewModel) As ActionResult
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

            '削除処理
            MyBase.BaseDelete(_ViewModel)

            '正常時、正常終了のステータスを返す
            If String.IsNullOrEmpty(_ViewModel.ErrCode) Then

                '操作ログ出力
                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelGate.TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_DELETE) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_GATE_CD) & ": " &
                                                     _ViewModel.GATE_CD & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_GATE_NAME) & ": " &
                                                     CommonRfid.ToStringNvl(_ViewModel.GATE_NAME).Trim
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelGate.TITLE))

                _ViewModel.States = StatusType.SuccessDelete
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

        ''' <summary>
        ''' 初期処理
        ''' </summary>
        Protected Overrides Sub InitializeDelete()

            '親の初期処理
            MyBase.InitializeDelete()

            'モデルを削除用モデルにコピー
            CopyViewModeltoModel(_ViewModel)

        End Sub

        ''' <summary>
        ''' 後処理
        ''' </summary>
        ''' <param name="model">基底モデル</param>
        Protected Overrides Sub DisposeDelete(ByRef model As BaseModel)

            MyBase.DisposeDelete(model)

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_GATE) = _ViewModel

        End Sub

        ''' <summary>
        ''' 削除処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function DeleteChcek() As Boolean
            Dim blnReturn As Boolean = True
            Dim db As New SystemDBContext

            ''DBのデータを取得する
            'Dim model As GateModel = db.Gate.Find(_Model.GATE_CD)

            ''他の箇所で使用されている場合エラー
            'If Dao.ToolGate.IsUseGate(_Model.GATE_CD) Then
            '    ModelState.AddModelError("GATE_CD", CommonErrorMsg.MSG_ERR_USED_DEPARTMENT)
            '    blnReturn = False
            'End If

            Return blnReturn
        End Function

        ''' <summary>
        ''' 削除処理のクエリを作成します
        ''' </summary>
        Protected Overrides Sub DeleteQuery()
            '20220328 UPD Start Kawamoto v2.1
            'Dim gate As GateModel = _db.Gate.Find(_Model.GATE_CD)
            Dim gate As GateModel = _db.Gate.Find(_Model.GATE_DIV, _Model.GATE_CD)
            '20220328 UPD End Kawamoto v2.1

            '削除を実行する
            _db.Gate.Remove(gate)

            'ゲートアンテナマスタ・ゲートサーモマスタ削除
            If ModelState.IsValid Then
                Try
                    GateAntennaDelete()
                    If _ViewModel.GATE_DIV = GateDiv.Cd.WG Then              '20220328 ADD Kawamoto v2.1
                        'WaveGate用
                        GateThermoDelete()
                    End If
                Catch ex As Exception
                    ModelState.AddModelError("", CommonLabel.MSG_ERR_SYSTEM)
                End Try
            End If

        End Sub
#End Region

#Region "有効無効コンボボックス設定処理"
        ''' <summary>
        ''' 有効無効コンボボックス設定処理
        ''' </summary>
        Public Sub ValidDropDownListCreate()

            Dim selListItem1 As SelectListItem = New SelectListItem() With {.Value = ValidFlg.Cd.Valid, .Text = Me._Message.getMessage(CommonLabel.VALUE_VALID)}
            Dim selListItem2 As SelectListItem = New SelectListItem() With {.Value = ValidFlg.Cd.Invalid, .Text = Me._Message.getMessage(CommonLabel.VALUE_INVALID)}
            Dim newList As List(Of SelectListItem) = New List(Of SelectListItem)
            newList.Add(selListItem1)
            newList.Add(selListItem2)

            'モーダル画面の有効/無効コンボ設定
            'ゲート
            Dim SelectedCd As String = _ViewModel.GATE_VALID_FLAG
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalValidGate = New SelectList(newList, "Value", "Text", 0)
            Else
                ViewBag.modalValidGate = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            'パトライト
            SelectedCd = _ViewModel.PATLITE_VALID_FLAG
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalValidPat = New SelectList(newList, "Value", "Text", 0)
            Else
                ViewBag.modalValidPat = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            'アンテナ１
            SelectedCd = _ViewModel.ANTENNA1_VALID_FLAG
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalValidAntenna1 = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalValidAntenna1 = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            'アンテナ２
            SelectedCd = _ViewModel.ANTENNA2_VALID_FLAG
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalValidAntenna2 = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalValidAntenna2 = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            'アンテナ３
            SelectedCd = _ViewModel.ANTENNA3_VALID_FLAG
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalValidAntenna3 = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalValidAntenna3 = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            'アンテナ４
            SelectedCd = _ViewModel.ANTENNA4_VALID_FLAG
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalValidAntenna4 = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalValidAntenna4 = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            'サーモセンサ１
            SelectedCd = _ViewModel.THERMO1_VALID_FLAG
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalValidThermo1 = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalValidThermo1 = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            'サーモセンサ２
            SelectedCd = _ViewModel.THERMO2_VALID_FLAG
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalValidThermo2 = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalValidThermo2 = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            '20220328 ADD Start Kawamoto v2.1
            'カメラ
            SelectedCd = _ViewModel.CAMERA_VALID_FLAG
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalValidCamera = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalValidCamera = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            '無条件で発報(カメラ)
            SelectedCd = _ViewModel.UNCONDITIONAL_C
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalUnconditionalCamera = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalUnconditionalCamera = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            '無条件で発報(パトライト)
            SelectedCd = _ViewModel.UNCONDITIONAL_P
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalUnconditionalPatlite = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalUnconditionalPatlite = New SelectList(newList, "Value", "Text", SelectedCd)
            End If
            '20220328 ADD End Kawamoto v2.1
        End Sub
#End Region

#Region "チャンネルコンボボックス設定処理"
        ''' <summary>
        ''' チャンネルコンボボックス設定処理
        ''' </summary>
        Public Sub ChannelDropDownListCreate()

            Dim selListItem1 As SelectListItem = New SelectListItem() With {.Value = "1", .Text = "1"}
            Dim selListItem2 As SelectListItem = New SelectListItem() With {.Value = "2", .Text = "2"}
            Dim selListItem3 As SelectListItem = New SelectListItem() With {.Value = "3", .Text = "3"}
            Dim selListItem4 As SelectListItem = New SelectListItem() With {.Value = "4", .Text = "4"}
            Dim newList As List(Of SelectListItem) = New List(Of SelectListItem)
            newList.Add(selListItem1)
            newList.Add(selListItem2)
            newList.Add(selListItem3)
            newList.Add(selListItem4)

            'モーダル画面のチャンネルコンボ設定
            Dim SelectedCd As String = CommonRfid.ToStringNvl(_ViewModel.GATE_CHANNEL)
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalChannel = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalChannel = New SelectList(newList, "Value", "Text", SelectedCd)
            End If

        End Sub
#End Region

        '20220328 ADD Start Kawamoto v2.1
#Region "Searchモードコンボボックス設定処理"
        ''' <summary>
        ''' Searchモードコンボボックス設定処理
        ''' </summary>
        Public Sub SearchModeDropDownListCreate()

            Dim selListItem1 As SelectListItem = New SelectListItem() With {.Value = SearchMode.Cd.ReaderSelected, .Text = SearchMode.Name.ReaderSelected}
            Dim selListItem2 As SelectListItem = New SelectListItem() With {.Value = SearchMode.Cd.SingleTarget, .Text = SearchMode.Name.SingleTarget}
            Dim selListItem3 As SelectListItem = New SelectListItem() With {.Value = SearchMode.Cd.DualTarget, .Text = SearchMode.Name.DualTarget}
            Dim selListItem4 As SelectListItem = New SelectListItem() With {.Value = SearchMode.Cd.TagFocus, .Text = SearchMode.Name.TagFocus}
            Dim selListItem5 As SelectListItem = New SelectListItem() With {.Value = SearchMode.Cd.SingleTargetReset, .Text = SearchMode.Name.SingleTargetReset}
            Dim selListItem6 As SelectListItem = New SelectListItem() With {.Value = SearchMode.Cd.DualTargetBtoASelect, .Text = SearchMode.Name.DualTargetBtoASelect}
            Dim newList As List(Of SelectListItem) = New List(Of SelectListItem)
            newList.Add(selListItem1)
            newList.Add(selListItem2)
            newList.Add(selListItem3)
            newList.Add(selListItem4)
            newList.Add(selListItem5)
            newList.Add(selListItem6)

            'モーダル画面のSearchモードコンボ設定
            Dim SelectedCd As String = CommonRfid.ToStringNvl(_ViewModel.SEARCH_MODE)
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalSearch = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalSearch = New SelectList(newList, "Value", "Text", SelectedCd)
            End If

        End Sub
#End Region

#Region "Readerモードコンボボックス設定処理"
        ''' <summary>
        ''' Readerモードコンボボックス設定処理
        ''' </summary>
        Public Sub ReaderModeDropDownListCreate()

            Dim selListItem1 As SelectListItem = New SelectListItem() With {.Value = ReaderMode.Cd.MaxThroughput, .Text = ReaderMode.Name.MaxThroughput}
            Dim selListItem2 As SelectListItem = New SelectListItem() With {.Value = ReaderMode.Cd.Hybrid, .Text = ReaderMode.Name.Hybrid}
            Dim selListItem3 As SelectListItem = New SelectListItem() With {.Value = ReaderMode.Cd.DenseReaderM4, .Text = ReaderMode.Name.DenseReaderM4}
            Dim selListItem4 As SelectListItem = New SelectListItem() With {.Value = ReaderMode.Cd.DenseReaderM8, .Text = ReaderMode.Name.DenseReaderM8}
            Dim selListItem5 As SelectListItem = New SelectListItem() With {.Value = ReaderMode.Cd.MaxMiller, .Text = ReaderMode.Name.MaxMiller}
            Dim selListItem6 As SelectListItem = New SelectListItem() With {.Value = ReaderMode.Cd.DenseReaderM4Two, .Text = ReaderMode.Name.DenseReaderM4Two}
            Dim selListItem7 As SelectListItem = New SelectListItem() With {.Value = ReaderMode.Cd.AutoSetDenseReader, .Text = ReaderMode.Name.AutoSetDenseReader}
            Dim selListItem8 As SelectListItem = New SelectListItem() With {.Value = ReaderMode.Cd.AutoSetDenseReaderDeepScan, .Text = ReaderMode.Name.AutoSetDenseReaderDeepScan}
            Dim selListItem9 As SelectListItem = New SelectListItem() With {.Value = ReaderMode.Cd.AutoSetStaticFast, .Text = ReaderMode.Name.AutoSetStaticFast}
            Dim selListItem10 As SelectListItem = New SelectListItem() With {.Value = ReaderMode.Cd.AutoSetStaticDenseReader, .Text = ReaderMode.Name.AutoSetStaticDenseReader}

            Dim newList As List(Of SelectListItem) = New List(Of SelectListItem)
            newList.Add(selListItem1)
            newList.Add(selListItem2)
            newList.Add(selListItem3)
            newList.Add(selListItem4)
            newList.Add(selListItem5)
            newList.Add(selListItem6)
            newList.Add(selListItem7)
            newList.Add(selListItem8)
            newList.Add(selListItem9)
            newList.Add(selListItem10)

            'モーダル画面のSearchモードコンボ設定
            Dim SelectedCd As String = CommonRfid.ToStringNvl(_ViewModel.READER_MODE)
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalReader = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalReader = New SelectList(newList, "Value", "Text", SelectedCd)
            End If

        End Sub
#End Region
        '20220328 ADD End Kawamoto v2.1

        '20220511 ADD Start Kawamoto
#Region "機種コンボボックス設定処理"
        ''' <summary>
        ''' 機種コンボボックス設定処理
        ''' </summary>
        Public Sub DeviceTypeDropDownListCreate()

            Dim selListItem1 As SelectListItem = New SelectListItem() With {.Value = DeviceType.Cd.R420, .Text = DeviceType.Name.R420}
            Dim selListItem2 As SelectListItem = New SelectListItem() With {.Value = DeviceType.Cd.R700, .Text = DeviceType.Name.R700}
            Dim newList As List(Of SelectListItem) = New List(Of SelectListItem)
            newList.Add(selListItem1)
            newList.Add(selListItem2)

            'モーダル画面の機種コンボ設定
            Dim SelectedCd As String = CommonRfid.ToStringNvl(_ViewModel.DEVICE_TYPE)
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalDeviceType = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalDeviceType = New SelectList(newList, "Value", "Text", SelectedCd)
            End If

        End Sub
#End Region

#Region "GPIO機器コンボボックス設定処理"
        ''' <summary>
        ''' GPIO機器コンボボックス設定処理
        ''' </summary>
        Public Sub GpioDeviceDropDownListCreate()

            Dim selListItem1 As SelectListItem = New SelectListItem() With {.Value = GpioDevice.Cd.Speed_Way, .Text = Me._Message.getMessage(CommonLabel.SPEED_WAY)}
            Dim selListItem2 As SelectListItem = New SelectListItem() With {.Value = GpioDevice.Cd.Dio_Board, .Text = Me._Message.getMessage(CommonLabel.CONTEC_DIO_BOARD)}
            Dim newList As List(Of SelectListItem) = New List(Of SelectListItem)
            newList.Add(selListItem1)
            newList.Add(selListItem2)

            'モーダル画面の機種コンボ設定
            Dim SelectedCd As String = CommonRfid.ToStringNvl(_ViewModel.GPIO_DEVICE)
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalGpioDevice = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalGpioDevice = New SelectList(newList, "Value", "Text", SelectedCd)
            End If

        End Sub
#End Region

#Region "GPOポートコンボボックス設定処理"
        ''' <summary>
        ''' GPOポートコンボボックス設定処理
        ''' </summary>
        Public Sub GpoPortDropDownListCreate()

            Dim selListItem1 As SelectListItem = New SelectListItem() With {.Value = "1", .Text = "1"}
            Dim selListItem2 As SelectListItem = New SelectListItem() With {.Value = "2", .Text = "2"}
            Dim selListItem3 As SelectListItem = New SelectListItem() With {.Value = "3", .Text = "3"}
            Dim newList As List(Of SelectListItem) = New List(Of SelectListItem)
            newList.Add(selListItem1)
            newList.Add(selListItem2)
            newList.Add(selListItem3)

            'モーダル画面のGPOポート（カメラ）コンボ設定
            Dim SelectedCd As String = CommonRfid.ToStringNvl(_ViewModel.GPO_PORT_C)
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalGpoPortCamera = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalGpoPortCamera = New SelectList(newList, "Value", "Text", SelectedCd)
            End If

            'モーダル画面のGPOポート（パトライト）コンボ設定
            SelectedCd = CommonRfid.ToStringNvl(_ViewModel.GPO_PORT_P)
            If String.IsNullOrEmpty(SelectedCd) Then
                ViewBag.modalGpoPortPatlite = New SelectList(newList, "Value", "Text", String.Empty)
            Else
                ViewBag.modalGpoPortPatlite = New SelectList(newList, "Value", "Text", SelectedCd)
            End If

        End Sub
#End Region
        '20220511 ADD End Kawamoto

#Region "ゲートアンテナマスタ登録処理"
        ''' <summary>
        ''' ゲートアンテナマスタ登録処理
        ''' </summary>
        Public Sub GateAntennaInsert(iAntenaNo As Integer, sValidFlg As String, sRfPow As String)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.Clear()
            sbSQL.AppendLine("INSERT INTO M_GATE_ANTENNA")
            sbSQL.AppendLine("(")
            sbSQL.AppendLine("   GATE_DIV")                 '20220328 ADD Kawamoto v2.1
            sbSQL.AppendLine("  ,GATE_CD")
            sbSQL.AppendLine("  ,ANTENNA_NO")
            sbSQL.AppendLine("  ,ANTENNA_VALID_FLAG")
            sbSQL.AppendLine("  ,ANTENNA_RF_POWER")
            sbSQL.AppendLine("  ,INS_DATETIME")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_DATETIME")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_TASK")
            sbSQL.AppendLine(") VALUES (")
            sbSQL.AppendLine(" @GateDiv")                   '20220328 ADD Kawamoto v2.1
            sbSQL.AppendLine(",@GateCd")
            sbSQL.AppendLine(",@ANTENNA_NO")
            sbSQL.AppendLine(",@ANTENNA_VALID_FLAG")
            Dim iRfPow As Integer = Nothing
            If Int32.TryParse(sRfPow, iRfPow) Then
                sbSQL.AppendLine("," & iRfPow)
            Else
                sbSQL.AppendLine(",NULL")
            End If
            sbSQL.AppendLine(",@INS_DATETIME")
            sbSQL.AppendLine(",@INS_EMPLOYEE_CD")
            sbSQL.AppendLine(",@INS_EMPLOYEE_NAME")
            sbSQL.AppendLine(",@UPD_DATETIME")
            sbSQL.AppendLine(",@UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(",@UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(",@UPD_TASK")
            sbSQL.AppendLine(")")

            params.Add(New SqlParameter($"@GateDiv", _ViewModel.GATE_DIV))  '20220328 ADD Kawamoto v2.1
            params.Add(New SqlParameter($"@GateCd", _ViewModel.GATE_CD))
            params.Add(New SqlParameter($"@ANTENNA_NO", iAntenaNo))
            params.Add(New SqlParameter($"@ANTENNA_VALID_FLAG", sValidFlg))
            params.Add(New SqlParameter($"@INS_DATETIME", _Model.INS_DATETIME))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _Model.INS_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _Model.INS_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_DATETIME", _Model.UPD_DATETIME))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _Model.UPD_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _Model.UPD_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_TASK", _Model.UPD_TASK))

            _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
        End Sub
#End Region

#Region "ゲートアンテナマスタ更新処理"
        ''' <summary>
        ''' ゲートアンテナマスタ更新処理
        ''' </summary>
        Public Sub GateAntennaUpdate(iAntenaNo As Integer, sValidFlg As String, sRfPow As String)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.AppendLine("UPDATE M_GATE_ANTENNA SET")
            sbSQL.AppendLine("  ANTENNA_VALID_FLAG = '" & sValidFlg & "'")
            Dim iRfPow As Integer = Nothing
            If Int32.TryParse(sRfPow, iRfPow) Then
                sbSQL.AppendLine("  ,ANTENNA_RF_POWER = '" & sRfPow & "'")
            Else
                sbSQL.AppendLine("  ,ANTENNA_RF_POWER = NULL")
            End If
            sbSQL.AppendLine("  ,UPD_DATETIME = @UPD_DATETIME")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")
            params.Add(New SqlParameter($"@UPD_DATETIME", _Model.UPD_DATETIME))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _Model.UPD_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _Model.UPD_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_TASK", _Model.UPD_TASK))
            sbSQL.AppendLine("WHERE GATE_CD = '" & _ViewModel.GATE_CD & "'")
            sbSQL.AppendLine("AND ANTENNA_NO = " & iAntenaNo)
            sbSQL.AppendLine("AND GATE_DIV = '" & _ViewModel.GATE_DIV & "'")        '20220328 ADD Kawamoto v2.1

            _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
        End Sub
#End Region

#Region "ゲートアンテナマスタ削除処理"
        ''' <summary>
        ''' ゲートアンテナマスタ削除処理
        ''' </summary>
        Public Sub GateAntennaDelete()
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.Clear()
            sbSQL.AppendLine("DELETE FROM M_GATE_ANTENNA")
            sbSQL.AppendLine("WHERE GATE_CD = '" & _Model.GATE_CD & "'")
            sbSQL.AppendLine("AND GATE_DIV = '" & _Model.GATE_DIV & "'")            '20220328 ADD Kawamoto v2.1
            _db.Database.ExecuteSqlCommand(sbSQL.ToString)
        End Sub
#End Region

#Region "ゲートサーモマスタ登録処理"
        ''' <summary>
        ''' ゲートサーモマスタ登録処理
        ''' </summary>
        Public Sub GateThermoInsert(iThermoNo As Integer, sValidFlg As String)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.Clear()
            sbSQL.AppendLine("INSERT INTO M_GATE_THERMO")
            sbSQL.AppendLine("(")
            sbSQL.AppendLine("   GATE_CD")
            sbSQL.AppendLine("  ,THERMO_NO")
            sbSQL.AppendLine("  ,THERMO_VALID_FLAG")
            sbSQL.AppendLine("  ,INS_DATETIME")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,INS_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_DATETIME")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_TASK")
            sbSQL.AppendLine(") VALUES (")
            sbSQL.AppendLine(" @GateCd")
            sbSQL.AppendLine(",@THERMO_NO")
            sbSQL.AppendLine(",@THERMO_VALID_FLAG")
            sbSQL.AppendLine(",@INS_DATETIME")
            sbSQL.AppendLine(",@INS_EMPLOYEE_CD")
            sbSQL.AppendLine(",@INS_EMPLOYEE_NAME")
            sbSQL.AppendLine(",@UPD_DATETIME")
            sbSQL.AppendLine(",@UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(",@UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(",@UPD_TASK")
            sbSQL.AppendLine(")")

            params.Add(New SqlParameter($"@GateCd", _ViewModel.GATE_CD))
            params.Add(New SqlParameter($"@THERMO_NO", iThermoNo))
            params.Add(New SqlParameter($"@THERMO_VALID_FLAG", sValidFlg))
            params.Add(New SqlParameter($"@INS_DATETIME", _Model.INS_DATETIME))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_CD", _Model.INS_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@INS_EMPLOYEE_NAME", _Model.INS_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_DATETIME", _Model.UPD_DATETIME))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _Model.UPD_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _Model.UPD_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_TASK", _Model.UPD_TASK))

            _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)
        End Sub
#End Region

#Region "ゲートサーモマスタ更新処理"
        ''' <summary>
        ''' ゲートサーモマスタ更新処理
        ''' </summary>
        Public Sub GateThermoUpdate(iThermoNo As Integer, sValidFlg As String)
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.AppendLine("UPDATE M_GATE_THERMO SET")
            sbSQL.AppendLine("  THERMO_VALID_FLAG = '" & sValidFlg & "'")
            sbSQL.AppendLine("  ,UPD_DATETIME = @UPD_DATETIME")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
            sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")
            params.Add(New SqlParameter($"@UPD_DATETIME", _Model.UPD_DATETIME))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _Model.UPD_EMPLOYEE_CD))
            params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _Model.UPD_EMPLOYEE_NAME))
            params.Add(New SqlParameter($"@UPD_TASK", _Model.UPD_TASK))
            sbSQL.AppendLine("WHERE GATE_CD = '" & _ViewModel.GATE_CD & "'")
            sbSQL.AppendLine("AND THERMO_NO = " & iThermoNo)

            _db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)


        End Sub
#End Region

#Region "ゲートサーモマスタ削除処理"
        ''' <summary>
        ''' ゲートサーモマスタ削除処理
        ''' </summary>
        Public Sub GateThermoDelete()
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.Clear()
            sbSQL.AppendLine("DELETE FROM M_GATE_THERMO")
            sbSQL.AppendLine("WHERE GATE_CD = '" & _Model.GATE_CD & "'")
            _db.Database.ExecuteSqlCommand(sbSQL.ToString)
        End Sub
#End Region

#Region "登録・更新時のエラーチェック"
        ''' <summary>
        ''' 登録・更新時のエラーチェック
        ''' </summary>
        ''' <returns>True:エラー無　False:エラー有</returns>
        Public Function ErrCheck() As Boolean

            'ゲート有効フラグ有効時のチェック
            If _ViewModel.GATE_VALID_FLAG = ValidFlg.Cd.Valid Then
                'ゲートIPアドレス空白チェック
                If _ViewModel.GATE_IPADDRESS = String.Empty Then
                    ModelState.AddModelError("GATE_IPADDRESS", CommonLabel.MSG_ERR_NOT_ENTERD)
                    Return False
                End If

                'IPアドレスチェック
                If Not _ViewModel.GATE_IPADDRESS = String.Empty Then
                    Dim returnValue As IPAddress
                    Try
                        returnValue = IPAddress.Parse(_ViewModel.GATE_IPADDRESS.Trim)
                    Catch ex As Exception
                        '20015,EXC,,,%1が不正です。%2,OKOnly,1
                        ModelState.AddModelError("GATE_IPADDRESS", CommonLabel.MSG_ERR_DATASEIYAKU)
                        Return False
                    End Try
                End If

                If _ViewModel.GATE_DIV = GateDiv.Cd.WG Then
                    'WaveGate用
                    'アンテナ有効フラグが有効の時、アンテナ電波強度がなければエラー
                    If _ViewModel.ANTENNA1_VALID_FLAG = ValidFlg.Cd.Valid AndAlso _ViewModel.ANTENNA1_RF_POWER = String.Empty Then
                        ModelState.AddModelError("ANTENNA1_RF_POWER", CommonLabel.MSG_ERR_NOT_ENTERD)
                        Return False
                    End If
                    If _ViewModel.ANTENNA2_VALID_FLAG = ValidFlg.Cd.Valid AndAlso _ViewModel.ANTENNA2_RF_POWER = String.Empty Then
                        ModelState.AddModelError("ANTENNA2_RF_POWER", CommonLabel.MSG_ERR_NOT_ENTERD)
                        Return False
                    End If
                    If _ViewModel.ANTENNA3_VALID_FLAG = ValidFlg.Cd.Valid AndAlso _ViewModel.ANTENNA3_RF_POWER = String.Empty Then
                        ModelState.AddModelError("ANTENNA3_RF_POWER", CommonLabel.MSG_ERR_NOT_ENTERD)
                        Return False
                    End If
                    If _ViewModel.ANTENNA4_VALID_FLAG = ValidFlg.Cd.Valid AndAlso _ViewModel.ANTENNA4_RF_POWER = String.Empty Then
                        ModelState.AddModelError("ANTENNA4_RF_POWER", CommonLabel.MSG_ERR_NOT_ENTERD)
                        Return False
                    End If
                Else
                    '20220328 ADD Start Kawamoto v2.1
                    'SpeedWay用
                    'アンテナ有効フラグが有効の時
                    If (_ViewModel.ANTENNA1_VALID_FLAG = ValidFlg.Cd.Valid OrElse _ViewModel.ANTENNA2_VALID_FLAG = ValidFlg.Cd.Valid OrElse
                        _ViewModel.ANTENNA3_VALID_FLAG = ValidFlg.Cd.Valid OrElse _ViewModel.ANTENNA4_VALID_FLAG = ValidFlg.Cd.Valid) Then
                        Dim intPowerCast As Integer
                        If _ViewModel.ANTENNA1_RF_POWER = String.Empty Then
                            'アンテナ電波強度がなければエラー
                            ModelState.AddModelError("ANTENNA1_RF_POWER", CommonLabel.MSG_ERR_NOT_ENTERD)
                            Return False
                        ElseIf Not Integer.TryParse(_ViewModel.ANTENNA1_RF_POWER, intPowerCast) OrElse intPowerCast > SW_ANTENNA_MAX_POWER OrElse intPowerCast < SW_ANTENNA_MIN_POWER Then
                            '範囲外ならエラー(ViewModelの範囲はWaveGateに準拠しているため使えない)
                            ModelState.AddModelError("ANTENNA1_RF_POWER", CommonLabel.MSG_ERR_INPUT_RANGE)
                            Return False
                        End If

                        If _ViewModel.SENSITIVITY = String.Empty Then
                            '感度がなければエラー
                            ModelState.AddModelError("SENSITIVITY", CommonLabel.MSG_ERR_NOT_ENTERD)
                            Return False
                        End If

                        If _ViewModel.SESSION = String.Empty Then
                            'セッションがなければエラー
                            ModelState.AddModelError("SESSION", CommonLabel.MSG_ERR_NOT_ENTERD)
                            Return False
                        End If

                        If _ViewModel.TAG_POPULATION = String.Empty Then
                            'Tag Populationがなければエラー
                            ModelState.AddModelError("TAG_POPULATION", CommonLabel.MSG_ERR_NOT_ENTERD)
                            Return False
                        End If

                        If _ViewModel.SENSITIVITY.IndexOf("-") > 0 Then
                            '-が先頭以外にあればエラー
                            ModelState.AddModelError("SENSITIVITY", CommonLabel.MSG_ERR_DATASEIYAKU)
                            Return False
                        End If

                        If _ViewModel.SENSITIVITY.Length - _ViewModel.SENSITIVITY.Replace("-", "").Length > 1 Then
                            '-が複数個含まれる場合はエラー
                            ModelState.AddModelError("SENSITIVITY", CommonLabel.MSG_ERR_DATASEIYAKU)
                            Return False
                        End If
                        Dim intSensitivityCast As Integer
                        If Not Integer.TryParse(_ViewModel.SENSITIVITY, intSensitivityCast) OrElse intSensitivityCast > SW_SENSITIVITY_MAX_POWER OrElse intSensitivityCast < SW_SENSITIVITY_MIN_POWER Then
                            '範囲外ならエラー(ViewModelの範囲はWaveGateに準拠しているため使えない)
                            ModelState.AddModelError("SENSITIVITY", CommonLabel.MSG_ERR_INPUT_RANGE)
                            Return False
                        End If
                        If String.IsNullOrEmpty(_ViewModel.FREQUENCY1) AndAlso String.IsNullOrEmpty(_ViewModel.FREQUENCY2) AndAlso String.IsNullOrEmpty(_ViewModel.FREQUENCY3) AndAlso String.IsNullOrEmpty(_ViewModel.FREQUENCY4) Then
                            '周波数は最低一つ入力しないとエラー
                            ModelState.AddModelError("FREQUENCY1", CommonLabelGate.MSG_ERR_FREQUENCY_NO_INPUT)
                            Return False
                        End If

                    End If

                    'GPIO機器が「DIOボード」の場合
                    If _ViewModel.GPIO_DEVICE = GpioDevice.Cd.Dio_Board Then
                        'カメラ有効フラグが有効の時、機器名(カメラ用)、ポート番号(カメラ用)がなければエラー
                        If _ViewModel.CAMERA_VALID_FLAG = ValidFlg.Cd.Valid AndAlso _ViewModel.DEVICE_NAME_C = String.Empty Then
                            ModelState.AddModelError("DEVICE_NAME_C", CommonLabel.MSG_ERR_NOT_ENTERD)
                            Return False
                        End If
                        If _ViewModel.CAMERA_VALID_FLAG = ValidFlg.Cd.Valid AndAlso _ViewModel.BIT_POS_C = String.Empty Then
                            ModelState.AddModelError("BIT_POS_C", CommonLabel.MSG_ERR_NOT_ENTERD)
                            Return False
                        End If

                        'パトライト有効フラグが有効の時、機器名(パトライト用)、ポート番号(パトライト用)がなければエラー
                        If _ViewModel.PATLITE_VALID_FLAG = ValidFlg.Cd.Valid AndAlso _ViewModel.DEVICE_NAME_P = String.Empty Then
                            ModelState.AddModelError("DEVICE_NAME_P", CommonLabel.MSG_ERR_NOT_ENTERD)
                            Return False
                        End If
                        If _ViewModel.PATLITE_VALID_FLAG = ValidFlg.Cd.Valid AndAlso _ViewModel.BIT_POS_P = String.Empty Then
                            ModelState.AddModelError("BIT_POS_P", CommonLabel.MSG_ERR_NOT_ENTERD)
                            Return False
                        End If

                        '20220511 ADD Start Kawamoto
                        'カメラ有効フラグとパトライト有効フラグが両方有効の時
                        If _ViewModel.CAMERA_VALID_FLAG = ValidFlg.Cd.Valid AndAlso _ViewModel.PATLITE_VALID_FLAG = ValidFlg.Cd.Valid Then
                            'ポート番号(カメラ用)とポート番号(パトライト用)が同じポートの場合エラー
                            If _ViewModel.BIT_POS_C = _ViewModel.BIT_POS_P Then
                                ModelState.AddModelError("BIT_POS_C", CommonLabelGate.MSG_ERR_SELECT_DIFFERENT_BIT_POS)
                                Return False
                            End If
                        End If

                        'GPIO機器が「SpeedWay」の場合
                    ElseIf _ViewModel.GPIO_DEVICE = GpioDevice.Cd.Speed_Way Then
                        'カメラ有効フラグとパトライト有効フラグが両方有効の時
                        If _ViewModel.CAMERA_VALID_FLAG = ValidFlg.Cd.Valid AndAlso _ViewModel.PATLITE_VALID_FLAG = ValidFlg.Cd.Valid Then
                            'GPOポート(カメラ)とGPOポート(パトライト)が同じポートを選択した場合エラー
                            If _ViewModel.GPO_PORT_C = _ViewModel.GPO_PORT_P Then
                                ModelState.AddModelError("GPO_PORT_C", CommonLabelGate.MSG_ERR_SELECT_DIFFERENT_GPO_PORT)
                                Return False
                            End If
                        End If
                    End If
                    '20220511 ADD End Kawamoto
                End If
                '20220328 ADD End Kawamoto v2.1
            End If
            Return True
        End Function
#End Region

#Region "共通"

        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New GateViewModel())
            End If

            '使用ゲート取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim settingGateDiv As String = Dao.ToolSetting.GetUseGate(Setting)

            Dim gateId As String = DispId.GATE_WAVE_GATE
            If settingGateDiv = GateDiv.Cd.SW Then
                gateId = DispId.GATE_SPEED_WAY
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).States = _ViewModel.States
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            '20220328 ADD Start Kawamoto v2.1
            _viewResult.ElementAt(0).GATE_DIV = _ViewModel.GATE_DIV
            _viewResult.ElementAt(0).FREQUENCY1 = _ViewModel.FREQUENCY1
            _viewResult.ElementAt(0).FREQUENCY2 = _ViewModel.FREQUENCY2
            _viewResult.ElementAt(0).FREQUENCY3 = _ViewModel.FREQUENCY3
            _viewResult.ElementAt(0).FREQUENCY4 = _ViewModel.FREQUENCY4
            '20220328 ADD End Kawamoto v2.1
            _viewResult.ElementAt(0).GridInfo = GetGridInfo(_db)
            _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(gateId).FIX_COL_CNT

        End Sub

        ''' <summary>
        ''' Viewモデルを更新用のモデルにコピーする
        ''' </summary>
        Private Sub CopyViewModeltoModel(ByVal model As GateViewModel)
            _Model.GATE_CD = model.GATE_CD
            _Model.GATE_NAME = model.GATE_NAME
            _Model.GATE_IPADDRESS = model.GATE_IPADDRESS
            _Model.GATE_CHANNEL = model.GATE_CHANNEL
            _Model.PATLITE_VALID_FLAG = model.PATLITE_VALID_FLAG
            _Model.GATE_VALID_FLAG = model.GATE_VALID_FLAG
            _Model.INS_DATETIME = model.INS_DATETIME
            _Model.INS_EMPLOYEE_CD = model.INS_EMPLOYEE_CD
            _Model.INS_EMPLOYEE_NAME = model.INS_EMPLOYEE_NAME
            _Model.UPD_DATETIME = model.UPD_DATETIME
            _Model.UPD_EMPLOYEE_CD = model.UPD_EMPLOYEE_CD
            _Model.UPD_EMPLOYEE_NAME = model.UPD_EMPLOYEE_NAME
            _Model.UPD_TASK = model.UPD_TASK
            '20220328 ADD Start Kawamoto v2.1
            _Model.GATE_DIV = model.GATE_DIV
            _Model.SENSITIVITY = model.SENSITIVITY
            _Model.SESSION = model.SESSION
            _Model.FREQUENCY1 = model.FREQUENCY1
            _Model.FREQUENCY2 = model.FREQUENCY2
            _Model.FREQUENCY3 = model.FREQUENCY3
            _Model.FREQUENCY4 = model.FREQUENCY4
            _Model.SEARCH_MODE = model.SEARCH_MODE
            _Model.READER_MODE = model.READER_MODE
            If model.TAG_POPULATION <> "" Then
                _Model.TAG_POPULATION = model.TAG_POPULATION.TrimStart("0")
            End If
            _Model.DEVICE_NAME_C = model.DEVICE_NAME_C
            If Not CommonRfid.ToStringNvl(model.BIT_POS_C) = String.Empty Then
                _Model.BIT_POS_C = Convert.ToInt32(model.BIT_POS_C)
            End If
            _Model.DEVICE_NAME_P = model.DEVICE_NAME_P
            If Not CommonRfid.ToStringNvl(model.BIT_POS_P) = String.Empty Then
                _Model.BIT_POS_P = model.BIT_POS_P
            End If
            If _ViewModel.GATE_DIV = GateDiv.Cd.WG Then
                _Model.CAMERA_VALID_FLAG = ValidFlg.Cd.Invalid
                _Model.UNCONDITIONAL_P = ValidFlg.Cd.Invalid
                _Model.UNCONDITIONAL_C = ValidFlg.Cd.Invalid
            Else
                _Model.CAMERA_VALID_FLAG = model.CAMERA_VALID_FLAG
                _Model.UNCONDITIONAL_P = model.UNCONDITIONAL_P
                _Model.UNCONDITIONAL_C = model.UNCONDITIONAL_C
            End If
            '20220328 ADD End Kawamoto v2.1
            '20220511 ADD Start Kawamoto
            _Model.DEVICE_TYPE = model.DEVICE_TYPE
            _Model.GPIO_DEVICE = model.GPIO_DEVICE
            If Not CommonRfid.ToStringNvl(model.GPO_PORT_C) = String.Empty Then
                _Model.GPO_PORT_C = Convert.ToInt32(model.GPO_PORT_C)
            End If
            If Not CommonRfid.ToStringNvl(model.GPO_PORT_P) = String.Empty Then
                _Model.GPO_PORT_P = model.GPO_PORT_P
            End If
            '20220511 ADD End Kawamoto

        End Sub
#End Region

#Region "項目配置情報を取得"
        ''' <summary>
        ''' 項目配置情報を取得
        ''' </summary>
        ''' <param name="db">DBコンテキスト</param>
        ''' <returns></returns>
        Public Shared Function GetGridInfo(ByRef db As SystemDBContext) As Dictionary(Of String, SlickGridInfo)
            '項目配置詳細マスタを取得
            Dim sbSQL As New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)

            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("  DISP_ID")
            sbSQL.AppendLine(", COLUMN_NAME")
            sbSQL.AppendLine(", DISP_ORDER")
            sbSQL.AppendLine(", TOOL_DIV")
            sbSQL.AppendLine(", WORD_DIV")
            sbSQL.AppendLine(", WORD_CD")
            sbSQL.AppendLine(", DISP_FLAG")
            sbSQL.AppendLine(", INS_DATETIME")
            sbSQL.AppendLine(", INS_EMPLOYEE_CD")
            sbSQL.AppendLine(", INS_EMPLOYEE_NAME")
            sbSQL.AppendLine(", UPD_DATETIME")
            sbSQL.AppendLine(", UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(", UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(", UPD_TASK")
            sbSQL.AppendLine("FROM M_COL_SET_DETAIL")
            sbSQL.AppendLine("WHERE DISP_ID = @DISP_ID")
            sbSQL.AppendLine("OR DISP_ID = @DISP_ID2")

            params.Add(New SqlParameter($"@DISP_ID", DispId.GATE_SPEED_WAY))
            params.Add(New SqlParameter($"@DISP_ID2", DispId.GATE_WAVE_GATE))

            Dim colSetDetail As List(Of ColSetDetailModel) = db.ColSetDetail.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            'Viewに渡す用に変換
            Dim dictGridInfo As New Dictionary(Of String, SlickGridInfo)
            For Each row In colSetDetail
                If Not dictGridInfo.ContainsKey(row.COLUMN_NAME) Then
                    'Key:DB項目名 Value:Grid表示情報
                    dictGridInfo.Add(row.COLUMN_NAME, New SlickGridInfo With {.DispOrder = row.DISP_ORDER, .DispFlag = row.DISP_FLAG})
                End If
            Next

            Return dictGridInfo
        End Function
#End Region

    End Class
End Namespace