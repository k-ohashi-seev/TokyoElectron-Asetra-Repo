Imports System.Data.Entity.Infrastructure
Imports System.Data.Entity.Migrations
Imports System.Data.Entity.SqlServer
Imports System.Data.SqlClient
Imports System.IO
Imports Asetra.Dao
Imports Asetra.Misc
Imports Asetra.Misc.ColKey
Imports Asetra.Misc.CommonSettingKey
Imports Asetra.Models
Imports AsetraLibWebApi
Imports Newtonsoft.Json

Namespace Controllers
    ''' <summary>
    ''' 実棚照会画面表示用のクラス
    ''' </summary>
    Public Class InventoryInquiryController
        Inherits PageBaseController

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "クラス定数"
        'Private Enum InventoryInquiryListKey
        '    ASSET_NO
        '    PHYINV_STATUS
        'End Enum

        ''' <summary>
        ''' データ取込画面　結果表示用クラス
        ''' </summary>
        Private Class CsvInv
            Public Enum ColIdx
                AssetStatusName
                LockEmployeeCd
                LockEmployeeName
                PhyinvStatusName
                BookLocationLCode
                BookLocationLCodeName
                BookLocationMCode
                BookLocationMCodeName
                BookLocationSCode
                BookLocationSCodeName
                PhyinvLocationLCode
                PhyinvLocationLCodeName
                PhyinvLocationMCode
                PhyinvLocationMCodeName
                PhyinvLocationSCode
                PhyinvLocationSCodeName
                CatLCd
                CatLCdName
                IsTangible
                AssetNo
                SubAssetNo
                AssetName
                PhyinvDate
                PhyinvPersonCd
                PhyinvPersonName
                FreeItem6
                FreeItem6Name
                FreeItem1
                FreeItem2
                FreeItem3
                FreeItem4
                FreeItem5
                FreeItem7
                FreeItem8
                FreeItem9
                FreeItem10
                FreeItem11
                FreeItem12
                FreeItem13
                FreeItem14
                FreeItem15
                FreeItem16
                FreeItem17
                FreeItem18
                FreeItem19
                FreeItem20
                FreeItem21
                FreeItem22
                FreeItem23
                FreeItem24
                FreeItem25
                FreeItem26
                FreeItem27
                FreeItem28
                FreeItem29
                FreeItem30
                FreeItem31
                FreeItem32
                FreeItem33
                FreeItem34
                FreeItem35
                FreeItem36
                FreeItem37
                FreeItem38
                FreeItem39
                FreeItem40
                FreeItem41
                FreeItem42
                FreeItem43
                FreeItem44
                FreeItem45
                FreeItem46
                FreeItem47
                FreeItem48
                FreeItem49
                FreeItem50
                FreeItem51
                FreeItem52
                FreeItem53
                FreeItem54
                FreeItem55
                FreeItem56
                FreeItem57
                FreeItem58
                FreeItem59
                FreeItem60
                TagLinlStatus
                OpeStatus
                OpeStatusName
                DiversionRemove
                RemoveDate
                RemoveReason
                RemoveReasonName
                RemoveContent
                ChangeLocMCd
                ChangeLocMName
                ChangeLocSCd
                ChangeLocSName
                ChangeEmployee
                ChangeEmployeeName
                Remark1
                Remark2
                Remark3
                Remark4
                Remark5
                Remark6
                Remark7
                Remark8
                Remark9
                Remark10
            End Enum
        End Class

        '[CC431] ADD Start 20261005
        ''' <summary>
        ''' データ出力・データ取込（Excel）の列定義
        ''' </summary>
        ''' <remarks>列の並びは M_CSVHEADER_ORDER（機能ID:実棚照会）の RANK で決まる。列の追加時は定義・M_CSVHEADER_ORDER・M_WORD を追加する</remarks>
        Private Class ExcelInv
            ''' <summary>
            ''' 列定義
            ''' </summary>
            Public Class ColDef
                ''' <summary>ヘッダの文言コード（M_CSVHEADER_ORDER.WORD_CD）</summary>
                Public Property WordCd As String
                ''' <summary>出力する値の取得</summary>
                ''' <remarks>型=InventoryInquiryViewModelを1つ受け取り、String を返す関数を入れるデリゲート</remarks>
                Public Property GetValue As Func(Of InventoryInquiryViewModel, String)
                ''' <summary>文字列書式で出力するか</summary>
                Public Property IsText As Boolean = False
                '[CC431] ADD Start 20261008
                ''' <summary>取込で更新する棚卸資産T の列名（一時テーブルの列名も同じ）。Nothing なら取込対象外</summary>
                Public Property ImportColumn As String = Nothing
                ''' <summary>取込の項目長（入出力ファイル一覧の桁数）。0 ならチェックしない</summary>
                Public Property MaxLength As Integer = 0
                ''' <summary>取込で必須か（条件付きの必須は個別にチェックする）</summary>
                Public Property IsRequired As Boolean = False
                '[CC431] ADD End 20261008
            End Class

            '[CC431] ADD Start 20261008
            ''' <summary>データ取込のチェックで使う列の文言コード</summary>
            Public Const WC_ASSET_NO As String = "WC110001"             '資産番号
            Public Const WC_OPE_STATUS As String = "WC110082"           '変更後稼働区分
            Public Const WC_DIVERSION_REMOVE As String = "WC110084"     '転用or除却
            Public Const WC_REMOVE_DATE As String = "WC110085"          '除却時期(予定)
            Public Const WC_REMOVE_REASON As String = "WC110086"        '除却理由
            Public Const WC_REMOVE_CONTENT As String = "WC110088"       '除却内容
            Public Const WC_CHANGE_LOC_M_CD As String = "WC110089"      '変更後課コード
            Public Const WC_CHANGE_LOC_S_CD As String = "WC110091"      '新変更後設置場所コード
            Public Const WC_CHANGE_EMPLOYEE As String = "WC110093"      '変更後管理者社員番号

            ''' <summary>データ取込のデータ開始行（Excelの行番号。1行目はヘッダ）</summary>
            Public Const IMPORT_DATA_START_ROW As Integer = 2
            '[CC431] ADD End 20261008

            ''' <summary>
            ''' 列定義の一覧（文言コードで M_CSVHEADER_ORDER と紐付ける）
            ''' </summary>
            '[CC431] UPD Start 20261008 取込対象の列に ImportColumn を追加
            Public Shared ReadOnly ColDefs As List(Of ColDef) = New List(Of ColDef) From {
                New ColDef With {.WordCd = "WC110011", .GetValue = Function(r) r.PHYINV_LOC_L_CD, .IsText = True},       '実棚会社コード
                New ColDef With {.WordCd = "WC110012", .GetValue = Function(r) r.PHYINV_LOC_L_NAME},                     '実棚会社名
                New ColDef With {.WordCd = "WC110013", .GetValue = Function(r) r.PHYINV_LOC_M_CD, .IsText = True},       '実棚原価センタ
                New ColDef With {.WordCd = "WC110014", .GetValue = Function(r) r.PHYINV_LOC_M_NAME},                     '実棚原価センタ名
                New ColDef With {.WordCd = "WC110015", .GetValue = Function(r) r.PHYINV_LOC_S_CD, .IsText = True},       '実棚場所コード
                New ColDef With {.WordCd = "WC110016", .GetValue = Function(r) r.PHYINV_LOC_S_NAME},                     '実棚場所名
                New ColDef With {.WordCd = "WC110025", .GetValue = Function(r) r.CAT_L_CD, .IsText = True},              '資産クラス
                New ColDef With {.WordCd = "WC110026", .GetValue = Function(r) r.CAT_L_NAME},                            '資産クラス名
                New ColDef With {.WordCd = "WC110001", .GetValue = Function(r) r.ASSET_NO, .IsText = True},              '資産番号
                New ColDef With {.WordCd = "WC110027", .GetValue = Function(r) r.SUB_ASSET_NO, .IsText = True},          '旧資産番号
                New ColDef With {.WordCd = "WC110002", .GetValue = Function(r) r.ASSET_NAME},                            '資産名称
                New ColDef With {.WordCd = "WC110031", .GetValue = Function(r) r.FREE_ITEM2, .IsText = True},            '資本化日付
                New ColDef With {.WordCd = "WC110082", .GetValue = Function(r) r.OPE_STATUS, .IsText = True, .ImportColumn = "OPE_STATUS", .MaxLength = 4, .IsRequired = True},               '変更後稼働区分
                New ColDef With {.WordCd = "WC110084", .GetValue = Function(r) r.DIVERSION_REMOVE, .IsText = True, .ImportColumn = "DIVERSION_REMOVE", .MaxLength = 2},     '転用or除却
                New ColDef With {.WordCd = "WC110085", .GetValue = Function(r) r.REMOVE_DATE, .IsText = True, .ImportColumn = "REMOVE_DATE"},               '除却時期(予定)
                New ColDef With {.WordCd = "WC110086", .GetValue = Function(r) r.REMOVE_REASON, .IsText = True, .ImportColumn = "REMOVE_REASON"},           '除却理由
                New ColDef With {.WordCd = "WC110087", .GetValue = Function(r) r.REMOVE_REASON_NAME},                                                       '除却理由名称
                New ColDef With {.WordCd = "WC110088", .GetValue = Function(r) r.REMOVE_CONTENT, .ImportColumn = "REMOVE_CONTENT", .MaxLength = 255},                         '除却内容
                New ColDef With {.WordCd = "WC110089", .GetValue = Function(r) r.CHANGE_LOC_M_CD, .IsText = True, .ImportColumn = "CHANGE_LOC_M_CD", .MaxLength = 10},       '変更後課コード
                New ColDef With {.WordCd = "WC110091", .GetValue = Function(r) r.CHANGE_LOC_S_CD, .IsText = True, .ImportColumn = "CHANGE_LOC_S_CD", .MaxLength = 10},       '新変更後設置場所コード
                New ColDef With {.WordCd = "WC110093", .GetValue = Function(r) r.CHANGE_EMPLOYEE, .IsText = True, .ImportColumn = "CHANGE_EMPLOYEE", .MaxLength = 15},       '変更後管理者社員番号
                New ColDef With {.WordCd = "WC110095", .GetValue = Function(r) r.REMARK1, .ImportColumn = "REMARK1", .MaxLength = 255},                                       '備考1
                New ColDef With {.WordCd = "WC110096", .GetValue = Function(r) r.REMARK2, .ImportColumn = "REMARK2", .MaxLength = 255},                                       '備考2
                New ColDef With {.WordCd = "WC110097", .GetValue = Function(r) r.REMARK3, .ImportColumn = "REMARK3", .MaxLength = 255},                                       '備考3
                New ColDef With {.WordCd = "WC110098", .GetValue = Function(r) r.REMARK4, .ImportColumn = "REMARK4", .MaxLength = 255},                                       '備考4
                New ColDef With {.WordCd = "WC110099", .GetValue = Function(r) r.REMARK5, .ImportColumn = "REMARK5", .MaxLength = 255},                                       '備考5
                New ColDef With {.WordCd = "WC110100", .GetValue = Function(r) r.REMARK6, .ImportColumn = "REMARK6", .MaxLength = 255},                                       '備考6
                New ColDef With {.WordCd = "WC110101", .GetValue = Function(r) r.REMARK7, .ImportColumn = "REMARK7", .MaxLength = 255},                                       '備考7
                New ColDef With {.WordCd = "WC110102", .GetValue = Function(r) r.REMARK8, .ImportColumn = "REMARK8", .MaxLength = 255},                                       '備考8
                New ColDef With {.WordCd = "WC110103", .GetValue = Function(r) r.REMARK9, .ImportColumn = "REMARK9", .MaxLength = 255},                                       '備考9
                New ColDef With {.WordCd = "WC110104", .GetValue = Function(r) r.REMARK10, .ImportColumn = "REMARK10", .MaxLength = 255}                                      '備考10
            }
            '[CC431] UPD End 20261008
        End Class
        '[CC431] ADD End 20261005

        '[CC431] ADD Start 20261007
        ''' <summary>
        ''' 検索条件のロケーションの選択肢の対象（画面の ajax とコントローラの間だけで使う値）
        ''' Loc：ロケーション 原価センタ（中区分）・場所（小区分）、ChangeLoc：変更後課コード・新変更後設置場所コード
        ''' </summary>
        Private Const SEARCH_LOC_TARGET_LOC As String = "Loc"
        Private Const SEARCH_LOC_TARGET_CHANGE_LOC As String = "ChangeLoc"
        '[CC431] ADD End 20261007

#End Region

#Region "クラス変数"
        Private _LoginInfo As RfidEntity.LoginUser = Nothing    'ログイン情報
        Private _AuthInfo As AuthInfo = Nothing                 '権限情報
        Private _ViewModel As InventoryInquiryViewModel = New InventoryInquiryViewModel                         'InventoryInquiry一覧表示用モデル
        Protected _viewResult As List(Of InventoryInquiryViewModel) = New List(Of InventoryInquiryViewModel)    '結果View
        Private _Message As MessageRfid = Nothing
        Private _lstErr As List(Of DataInportResult)
        Private _LocationLList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationMList As List(Of LocationDropdownModel)     'ロケーションマスタリスト
        Private _LocationSList As List(Of LocationDropdownModel)     'ロケーションマスタリスト

        Private _viewInventory As List(Of InventoryViewModel)
        Private _invLocLCd As String = String.Empty
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
        Public Function Index(ByVal ViewModel As InventoryInquiryViewModel, ByVal actionname As String) As ActionResult

            '登録・変更画面からのリダイレクト以外、初期化する
            If IsNothing(actionname) OrElse actionname <> CommonConst.KEY_ON_CREATE_CLICK Then
                Session.Remove(CommonSessionKey.CREAGE_INVENTORY_INQUIRY)
            End If

            '20231005 ADD Start Shirakawa
            '棚卸進捗状況からのキー(棚卸ID/棚卸ロケ大区分コード)を取得
            Dim key As String = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY)
            If String.IsNullOrEmpty(key) Then
                Return RedirectToAction("Index", "InventorySummary")
            Else
                _invLocLCd = key.Split("/")(1)
            End If
            '20231005 ADD End Shirakawa

            'セッションに大ロケをいれる
            Session(CommonSessionKey.INV_RETURN_LOC) = _invLocLCd

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '検索ボタンがクリックされた場合、選択状態のセッションをクリアする
            If _Message.getMessage(CommonLabel.BUTTON_SEARCH).Equals(actionname) Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
            End If


            'モデルを設定(セッションにある場合はセッションから設定)
            Dim model As InventoryInquiryViewModel = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY)
            If Not IsNothing(model) AndAlso actionname = CommonConst.KEY_ON_CREATE_CLICK Then
                _ViewModel = model
            Else
                _ViewModel = ViewModel
            End If

            '最新のチェック状態でセッション情報を更新する(追加および削除)
            'UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys) '2020/01/07 nv miyaji コメントアウト

            'ログインセッションが切れていたらエラーコードを設定し、一覧表示しない
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOGININFO_EMPTY
                _ViewModel.ErrTarget = CommonConst.KEY_FROM_LOGIN
            Else
                '--2021/11/08 add start nv iwamoto
                '権限情報の取得
                Dim authInfo As AuthInfo = _Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryInquiry, _LoginInfo)
                If authInfo.UpdAuthFlg = AUTH_FLG.Yes Then
                    ViewBag.AuthProcFlg = AuthProcFlg.All
                Else
                    ViewBag.AuthProcFlg = AuthProcFlg.ShokaiNomi
                End If
                '--2021/11/08 add end nv iwamoto

                '一覧表示処理
                MyBase.BaseIndex(_ViewModel)
                '画面右上に社員名を表示
                ViewBag.LoginUserNm = _LoginInfo.EmployeeNm
                ViewBag.LoginInfo = _LoginInfo
            End If

            '設定情報(実棚ロケ使用区分)取得     2020/03/17 ADD nv.miyaji
            Dim settingInfo As MessageRfid      'メッセージ兼設定リスト
            Dim modelLocationUsage As New LocationUsageModel       'ロケーション使用区分
            settingInfo = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim setting As SettingInfo = settingInfo.getSettingInfo(CommonSettingKey.ActualShelfLocation.KeyDiv, CommonSettingKey.ActualShelfLocation.Key1, CommonSettingKey.ActualShelfLocation.Key2, CommonSettingKey.ActualShelfLocation.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingInfo.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.LocationUsage.KeyDiv))
            End If
            ViewBag.LocUsage_L = setting.DATA1
            ViewBag.LocUsage_M = setting.DATA2
            ViewBag.LocUsage_S = setting.DATA3

            '最新のチェック状態でセッション情報を更新する(追加および削除)
            UpdateChecedInfo(_ViewModel.SelectKeys, _ViewModel.NoneSelectKeys)

            '編集内容リストを更新する
            Dim editedList As List(Of EditedContainModel) = Nothing
            If (_ViewModel.EditedContainListJson IsNot Nothing) Then
                editedList = JsonConvert.DeserializeObject(Of List(Of EditedContainModel))(_ViewModel.EditedContainListJson)
            End If
            UpdateEditedContains(editedList)

            ' (ロケーション)大区分ドロップダウンリスト作成
            GetLocationLData()
            ' (ロケーション)中区分
            Dim auth As AuthInfo = _Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryInquiry, _LoginInfo)
            '[CC431] UPD Start 20261007
            ' (ロケーション)中区分・小区分、変更後課・変更後設置場所（当該棚卸の棚卸資産Tに存在するもの。場所から選ぶの ON／OFF で連動）
            Dim invIdForLoc As Long
            Long.TryParse(key.Split("/")(0), invIdForLoc)       'キー(棚卸ID/棚卸ロケ大区分コード)から棚卸ロケ大区分コードを取得
            SetSearchLocationOptions(invIdForLoc, _invLocLCd, auth)
            '[CC431] UPD End 20261007


            ' (カテゴリー)大区分ドロップダウンリスト作成
            GetCategoryLData()

            'フリー項目10（貸与資産状態区分）ドロップダウンリスト作成
            GetFreeItem10Data()

            'タグ紐づけ状態ドロップダウンリストデータ作成
            GetTagLinkStatusData()

            '[CC431] ADD Start 20261002
            ' 棚卸実施端末（実棚処理ツール区分。C:CS は使用しない）
            Dim phyinvToolDivList As New List(Of DropdownModel) From {
                New DropdownModel With {.Cd = ProcessToolDiv.HT, .Name = _Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)},
                New DropdownModel With {.Cd = ProcessToolDiv.Web, .Name = _Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)}
            }
            ViewBag.PhyinvToolDivList = New SelectList(phyinvToolDivList, "Cd", "Name")

            ' ロック資産ステータス（未：ロック担当者なし、済：ロック担当者あり）
            Dim lockStatusList As New List(Of DropdownModel) From {
                New DropdownModel With {.Cd = InvLockStatus.Unlocked, .Name = _Message.getMessage(CommonLabel.VALUE_MI)},
                New DropdownModel With {.Cd = InvLockStatus.Locked, .Name = _Message.getMessage(CommonLabel.VALUE_SUMI)}
            }
            ViewBag.LockStatusList = New SelectList(lockStatusList, "Cd", "Name")
            '[CC431] ADD End 20261002

            'ﾄﾞﾛｯﾌﾟﾀﾞｳﾝ選択肢作成
            ' 稼働区分
            Dim opeStatusItemList = GetSettingMDropListData("Ope_Status_Item")
            ViewBag.OpeOptions = Newtonsoft.Json.JsonConvert.SerializeObject(opeStatusItemList.ToDictionary(Function(x) x.Cd, Function(x) x.Name))
            '  検索条件の変更後稼働区分リストの先頭に「-」を追加
            opeStatusItemList.Insert(0,
                    New DropdownModel With {
                        .Cd = CommonConst.VALUE_BLANK,
                        .Name = CommonConst.VALUE_BLANK
                    }
            )
            ViewBag.OpeOptionsModel = New SelectList(opeStatusItemList, "Cd", "Name")

            ' 転用or除却
            Dim diversionOrRemoveList = GetSettingMDropListData("Diversion_or_Remove", "DATA1", "DATA1")
            ' 文字列順の問題で降順にする。
            diversionOrRemoveList.Sort(Function(a, b) b.Cd.CompareTo(a.Cd))
            ViewBag.DiversionRemoveOptions = Newtonsoft.Json.JsonConvert.SerializeObject(diversionOrRemoveList.ToDictionary(Function(x) x.Cd, Function(x) x.Name))
            ViewBag.DiversionRemoveOptionsModel = New SelectList(diversionOrRemoveList, "Cd", "Name")

            Dim diversionOrRemoveDataList = ToolSetting.GetSettingList("Diversion_or_Remove")
            ViewBag.DiversionRemoveDataList = Newtonsoft.Json.JsonConvert.SerializeObject(diversionOrRemoveDataList)

            ' 除却理由
            Dim removeReasonList = GetSettingMDropListData("Remove_Reason_Item")
            ViewBag.RemoveReasonOptions = Newtonsoft.Json.JsonConvert.SerializeObject(removeReasonList.ToDictionary(Function(x) x.Cd, Function(x) x.Name))
            ViewBag.RemoveReasonOptionsModel = New SelectList(removeReasonList, "Cd", "Name")

            Dim removeReasonDataList = ToolSetting.GetSettingList("Remove_Reason_Item")
            ViewBag.RemoveReasonDataList = Newtonsoft.Json.JsonConvert.SerializeObject(removeReasonDataList)

            '画面表示時に社員コード、ロケーションマスタ突き合せ用の一覧を取得
            _ViewModel.EmployeeList = GetEmployeeList()
            _ViewModel.LocationList = GetLocationList()
            _ViewModel.LocationMList = GetLocationMList(_ViewModel.SearchLocL, _LoginInfo.EmployeeCd, auth)
            _ViewModel.LocationSList = GetLocationSList(_ViewModel.SearchLocL)

            '一覧表示フラグ取得
            Dim colSetModel As ColSetModel = _db.ColSet.Find(DispId.INVENTORY_INQUIRY, If(String.IsNullOrEmpty(_ViewModel.SearchLocL), "TEMPLATE", _ViewModel.SearchLocL))
            If Not IsNothing(colSetModel) AndAlso colSetModel.DISP_FLG.ToString = DispFlag.VISIBLE Then
                ViewBag.GridDisp = ValidFlg.Cd.Valid
            Else
                ViewBag.GridDisp = ValidFlg.Cd.Invalid
            End If

            'Session(CommonSessionKey.INV_ID) =
            'Session(CommonSessionKey.LOC_L_CD) = _ViewModel.LOC_L_CD

            '一覧情報にページ情報を設定
            CopyModelData()

            ' 棚卸管理テーブルを取得
            GetInventry()

            '結果表示
            Return View(_viewResult)

        End Function

        ''' <summary>
        ''' ロケーション変更
        ''' </summary>
        ''' <param name="locationLevel"></param>
        ''' <param name="selectLCode"></param>
        ''' <param name="selectMCode"></param>
        ''' <returns></returns>
        ''' <remarks>使用していない</remarks>
        Public Function ChangeLocation(ByVal locationLevel As String, ByVal selectLCode As String, ByVal selectMCode As String,
                                       sEmployeeCd As String, authInfo As AuthInfo) As ActionResult

            '2020/03/13 実棚ロケ使用区分　の判断を追加 ADD Inoue
            'If locationLevel = "M" Then
            '    Return Json(GetLocationMData(selectLCode))
            'Else
            '    Return Json(GetLocationSData(selectLCode, selectMCode))
            'End If

            ''結果表示
            'Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

            Dim settingRfid As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)               'メッセージ兼設定リスト
            Dim modelLocationUsage As New LocationUsageModel        'ロケーション使用区分

            '棚卸処理では、ActualShelfLocation　を使用する
            Dim setting As SettingInfo = settingRfid.getSettingInfo(CommonSettingKey.ActualShelfLocation.KeyDiv, CommonSettingKey.ActualShelfLocation.Key1, CommonSettingKey.ActualShelfLocation.Key2, CommonSettingKey.ActualShelfLocation.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingRfid.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.ActualShelfLocation.KeyDiv))
            End If
            modelLocationUsage.LocUsage_L = setting.DATA1
            modelLocationUsage.LocUsage_M = setting.DATA2
            modelLocationUsage.LocUsage_S = setting.DATA3

            If locationLevel = LOC_DIV.LocationM Then
                If modelLocationUsage.LocUsage_M = Misc.ItemUse.Yes Then
                    Return Json(GetLocationMData(selectLCode, authInfo))
                Else
                    '中区分 使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            Else

                If modelLocationUsage.LocUsage_S = Misc.ItemUse.Yes Then
                    Return Json(GetLocationSData(selectLCode, selectMCode))
                Else
                    '小区分　使用をしない場合、Nothing を返す
                    Return Nothing
                End If
            End If

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

        '[CC431] ADD Start 20261007
        ''' <summary>
        ''' 検索条件のロケーションの選択肢を返す（画面の ajax から呼ぶ）
        ''' </summary>
        ''' <param name="target">Loc：原価センタ・場所、ChangeLoc：変更後課・変更後設置場所</param>
        ''' <param name="level">返す側（M：中区分、S：小区分）</param>
        ''' <param name="reverse">場所から選ぶ（True：場所で原価センタを絞り込む、False：原価センタで場所を絞り込む）</param>
        ''' <param name="codes">絞り込みに使う、反対側の選択中のコード（「,」区切り）</param>
        ''' <returns>選択肢（LOC_CD／LOC_NAME のリスト）</returns>
        Public Function GetSearchLocationOptions(ByVal target As String, ByVal level As String, ByVal reverse As Boolean, ByVal codes As String) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            Dim key As String = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY)
            If IsNothing(_LoginInfo) OrElse String.IsNullOrEmpty(key) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '棚卸進捗状況からのキー(棚卸ID/棚卸ロケ大区分コード)から棚卸IDを取得
            Dim keys As String() = key.Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            '棚卸処理では、ActualShelfLocation　を使用する
            Dim settingRfid As MessageRfid = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)               'メッセージ兼設定リスト
            Dim setting As SettingInfo = settingRfid.getSettingInfo(CommonSettingKey.ActualShelfLocation.KeyDiv, CommonSettingKey.ActualShelfLocation.Key1, CommonSettingKey.ActualShelfLocation.Key2, CommonSettingKey.ActualShelfLocation.Key3)
            If IsNothing(setting) Then
                Throw New Exception(String.Format(settingRfid.getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), CommonSettingKey.ActualShelfLocation.KeyDiv))
            End If
            '中区分・小区分を使用しない場合、Nothing を返す（ChangeLocation と同じ）
            If (level = LOC_DIV.LocationM AndAlso setting.DATA2 <> Misc.ItemUse.Yes) OrElse
               (level = LOC_DIV.LocationS AndAlso setting.DATA3 <> Misc.ItemUse.Yes) Then
                Return Nothing
            End If

            '権限はサーバー側で取得する
            Me._Message = settingRfid
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryInquiry, _LoginInfo)

            Return Json(GetSearchLocationList(target, level, reverse, CommonRfid.ToStringNvl(codes).Split(","c), invId, invLocL, authInfo))

        End Function
        '[CC431] ADD End 20261007

        ''' <summary>
        ''' 後処理
        ''' </summary>
        Protected Overrides Sub DisposeIndex()

            'エラーでない場合、セッションクリア
            If ModelState.IsValid AndAlso _ViewModel.ErrCode = String.Empty Then
                Session.Remove(CommonSessionKey.CREAGE_INVENTORY_INQUIRY)
            End If
        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを作成します
        ''' </summary>
        Protected Overrides Sub ViewQuery()
            'クエリを返却
            MyBase._SQL = CreateSQL()

            '検索条件(全件)を指定したSQLとパラメーターをセッションに保持する
            Session(CommonSessionKey.LIST_VIEW_SQL) = MyBase._SQL
            Session(CommonSessionKey.LIST_VIEW_PARAM) = MyBase._paramSqlList

        End Sub

        ''' <summary>
        ''' 一覧表示のクエリを実行します
        ''' </summary>
        Protected Overrides Sub ViewExecQuery()
            'SQLを実行
            Dim query As DbSqlQuery(Of InventoryInquiryViewModel) = _db.InventoryInquiryView.SqlQuery(_SQL, _paramSqlList.ToArray)
            '画面に表示するViewを設定
            _viewResult = query.ToList
        End Sub

#End Region

        ''' <summary>
        ''' 現ロケ変更処理
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="OnCreteButton"></param>
        ''' <returns></returns>
        ''' <remarks>現バージョンでは使用しない</remarks>
        <ValidateInput(False)>
        Public Function UpdateInventory(ByVal ViewModel As InventoryInquiryViewModel, ByVal OnCreteButton As String) As ActionResult
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

            '最新のチェック状態でセッション情報を更新する(追加および削除) (キー：資産番号, 実棚ステータス)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            '棚卸進捗状況からのキー(棚卸ID/棚卸ロケ大区分コード)を取得
            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            '選択されたデータの実棚ステータスが「W」でなければエラー
            For Each str As String In lstSelectKeys
                Dim dt = ToolInvAsset.GetInvAsset(invId, invLocL, str)

                If dt.Rows(0).Field(Of String)("PHYINV_STATUS") <> PHYINV_STATUS.Warning Then
                    'エラー
                    _ViewModel.ErrCode = CommonErrorMsg.PHYINV_STATUS_CHECKBOX_NOTSELWARNING
                    'モデルの内容をセッションに保持
                    Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY) = _ViewModel
                    Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
                End If
            Next

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)
            Dim sTaskNm As String = "InventoryInquiryController.UpdateInventory"

            '2023/02/06 ADD Start Shirakawa
            '設定ファイルからロケごとの資産登録上限数を取得

            Dim Setting As MessageRfid      '設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim AssetRegistrationLimit As SettingInfo = Setting.getSettingInfo(CommonSettingKey.Asset_Registration_Limit.KeyDiv, CommonSettingKey.Asset_Registration_Limit.Key1, CommonSettingKey.Asset_Registration_Limit.Key2, CommonSettingKey.Asset_Registration_Limit.Key3, True)


            '2023/02/06 ADD End Shirakawa

            '2019/10/14 Update Inoue
            'sbSQL.Clear()
            'params = New List(Of SqlParameter)

            'sbSQL.AppendLine("UPDATE M_ASSET SET")
            'sbSQL.AppendLine("   LOC_L_CD = PHYINV_LOC_L_CD")
            'sbSQL.AppendLine("  ,LOC_M_CD = PHYINV_LOC_M_CD")
            'sbSQL.AppendLine("  ,LOC_S_CD = PHYINV_LOC_S_CD")
            'sbSQL.AppendLine("  ,PHYINV_STATUS = '" & PHYINV_STATUS.Normal & "'")

            'sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
            'sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
            'sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
            'sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

            'params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            'params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            'params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

            'sbSQL.AppendLine("WHERE")

            'Dim assetNoCondition As String()
            'assetNoCondition = _ViewModel.SelectAssetNo.Split(",")
            'If assetNoCondition.Length = 1 Then
            '    sbSQL.AppendLine("ASSET_NO = '" & assetNoCondition(0) & "'")
            'Else
            '    sbSQL.Append("ASSET_NO IN (")
            '    For i As Integer = 0 To assetNoCondition.Count - 1
            '        If i > 0 Then
            '            sbSQL.Append("," & assetNoCondition(i))
            '        Else
            '            sbSQL.Append(assetNoCondition(i))
            '        End If
            '    Next
            '    sbSQL.AppendLine(")")
            'End If
            'db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

            ''正常時、正常終了のステータスを返す
            'If String.IsNullOrEmpty(_ViewModel.ErrCode) Then
            '    '操作ログ出力
            '    'Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE) & " " &
            '    '                                     Me._Message.getMessage(CommonLabelInventorySummary.BUTTON_CHANGE_CURRENT_LOC) & " " &
            '    '                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
            '    '                                     Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " &
            '    '                                     _ViewModel.ASSET_NO.ToString.Trim & "  " &
            '    '                                     Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
            '    '                                     _ViewModel.ASSET_NAME.ToString.Trim
            '    'Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE),
            '    '                               _ViewModel.PHYINV_LOC_L_CD.ToString.Trim, _ViewModel.PHYINV_LOC_M_CD.ToString.Trim, _ViewModel.PHYINV_LOC_S_CD.ToString.Trim,
            '    '                               _ViewModel.PHYINV_LOC_L_NAME.ToString.Trim, _ViewModel.PHYINV_LOC_M_NAME.ToString.Trim, _ViewModel.PHYINV_LOC_S_NAME.ToString.Trim)
            '    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE) & " " &
            '                                         Me._Message.getMessage(CommonLabelInventorySummary.BUTTON_CHANGE_CURRENT_LOC) & " " &
            '                                         Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT)
            '    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE))

            '    _ViewModel.States = StatusType.SuccessEdit
            'End If

            'トランザクションスタート
            Using tran = db.Database.BeginTransaction()
                Try

                    Dim aseetNoList As New List(Of String)   '2023/02/09 ADD Shirakawa
                    '資産M更新
                    Dim assetNoCondition As String()
                    '--2019/12/17 miyaji update start
                    'assetNoCondition = String.Join(",", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(InventoryInquiryListKey.ASSET_NO))).Split(",")
                    assetNoCondition = lstSelectKeys.ToArray
                    '--2019/12/17 miyaji update end
                    For ii As Integer = 0 To assetNoCondition.Count - 1
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("UPDATE M_ASSET SET")
                        sbSQL.AppendLine("   LOC_L_CD = PHYINV_LOC_L_CD")
                        sbSQL.AppendLine("  ,LOC_M_CD = PHYINV_LOC_M_CD")
                        sbSQL.AppendLine("  ,LOC_S_CD = PHYINV_LOC_S_CD")
                        sbSQL.AppendLine("  ,PHYINV_STATUS = '" & PHYINV_STATUS.Normal & "'")

                        sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")

                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                        params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                        params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

                        sbSQL.AppendLine("WHERE ASSET_NO = @ASSET_NO")

                        params.Add(New SqlParameter($"@ASSET_NO", assetNoCondition(ii)))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                        '資産処理履歴ファイル　更新
                        sbSQL.Clear()
                        params = New List(Of SqlParameter)

                        sbSQL.AppendLine("INSERT INTO T_PROCESS_LOG")
                        sbSQL.AppendLine("(")
                        sbSQL.AppendLine("   PROCESS_DT")
                        sbSQL.AppendLine("  ,PROCESS_TM")
                        sbSQL.AppendLine("  ,ASSET_NO")
                        sbSQL.AppendLine("  ,ASSET_ID")
                        sbSQL.AppendLine("  ,PROCESS_TOOL_DIV")
                        sbSQL.AppendLine("  ,PROCESS_DIV")
                        sbSQL.AppendLine("  ,LEASE_PLACE_CD")
                        sbSQL.AppendLine("  ,LEASE_PLACE_NAME")
                        sbSQL.AppendLine("  ,REMARK")   '2019/11/19 nv miyaji add
                        sbSQL.AppendLine("  ,LEASE_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,LEASE_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,LEASE_DUE_DT")
                        sbSQL.AppendLine("  ,LOC_L_CD")
                        sbSQL.AppendLine("  ,LOC_M_CD")
                        sbSQL.AppendLine("  ,LOC_S_CD")
                        sbSQL.AppendLine("  ,LOC_L_NAME")
                        sbSQL.AppendLine("  ,LOC_M_NAME")
                        sbSQL.AppendLine("  ,LOC_S_NAME")
                        sbSQL.AppendLine("  ,ASSET_NAME")
                        sbSQL.AppendLine("  ,MAKER")
                        sbSQL.AppendLine("  ,PRODUCT_NO")
                        sbSQL.AppendLine("  ,MODEL_NO")
                        sbSQL.AppendLine("  ,TASK_RESULT")
                        sbSQL.AppendLine("  ,UPD_DATETIME")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,UPD_TASK")
                        sbSQL.AppendLine(")")
                        sbSQL.AppendLine("SELECT")
                        sbSQL.AppendLine("   CONVERT(int, FORMAT(GetDate(), 'yyyyMMdd'))")
                        sbSQL.AppendLine("  ,FORMAT(GetDate(), 'HHmmssfff')")
                        sbSQL.AppendLine("  ,ASSET.ASSET_NO")
                        sbSQL.AppendLine("  ,ASSET.ASSET_ID")
                        sbSQL.AppendLine("  ,'" & Misc.ProcessToolDiv.Web & "'")
                        sbSQL.AppendLine("  ,'" & Misc.ProcessDiv.LC & "'")                 '2019/10/15 nv miyaji upd
                        sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_CD")
                        sbSQL.AppendLine("  ,ASSET.LEASE_PLACE_NAME")
                        sbSQL.AppendLine("  ,ASSET.REMARK")   '2019/11/19 nv miyaji add
                        sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,ASSET.LEASE_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,ASSET.LEASE_DUE_DT")
                        sbSQL.AppendLine("  ,ASSET.LOC_L_CD")
                        sbSQL.AppendLine("  ,ASSET.LOC_M_CD")
                        sbSQL.AppendLine("  ,ASSET.LOC_S_CD")
                        sbSQL.AppendLine("  ,LOCL.LOC_NAME AS LocLName ")
                        sbSQL.AppendLine("  ,LOCM.LOC_NAME AS LocMName ")
                        sbSQL.AppendLine("  ,LOCS.LOC_NAME AS LocSName ")
                        sbSQL.AppendLine("  ,ASSET.ASSET_NAME")
                        sbSQL.AppendLine("  ,ASSET.MAKER")
                        sbSQL.AppendLine("  ,ASSET.PRODUCT_NO")
                        sbSQL.AppendLine("  ,ASSET.MODEL_NO")
                        sbSQL.AppendLine("  ,'" & Misc.TaskResult.OK & "'")
                        sbSQL.AppendLine("  ,ASSET.UPD_DATETIME")
                        sbSQL.AppendLine("  ,ASSET.UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("  ,ASSET.UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("  ,ASSET.UPD_TASK")
                        sbSQL.AppendLine("FROM M_ASSET ASSET")

                        sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
                        sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
                        sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
                        sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM ")
                        sbSQL.AppendLine("ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
                        sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCM.LOC_L_CD")
                        sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
                        sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS ")
                        sbSQL.AppendLine("ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
                        sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCS.LOC_L_CD")
                        sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCS.LOC_M_CD")
                        sbSQL.AppendLine("AND ASSET.LOC_S_CD = LOCS.LOC_S_CD")

                        sbSQL.AppendLine("WHERE  ASSET.ASSET_NO = @ASSET_NO")
                        params.Add(New SqlParameter($"@ASSET_NO", assetNoCondition(ii)))

                        db.Database.ExecuteSqlCommand(sbSQL.ToString, params.ToArray)


                        aseetNoList.Add(assetNoCondition(ii)) '2023/02/09 ADD Shirakawa

                    Next

                    '2023/02/06 ADD Start Shirakawa
                    'コミットを行うか判定
                    Dim isCommit As Boolean = False
                    Dim sLimit As Integer = 0
                    '設定マスタのAssetRegistrationLimit.DATA1が数値である場合
                    If Integer.TryParse(AssetRegistrationLimit.DATA1, sLimit) Then
                        'リミットが0より大きい場合
                        If 0 < sLimit Then
                            '総数がリミットを超えてないチェックする
                            Dim lstAseetDate As New List(Of AssetListLOC_L_CDModel)

                            For i As Integer = 0 To aseetNoList.Count - 1
                                '変更後の大区分コード取得
                                Dim tbl As New DataTable
                                Dim clsDB As clsSV_DB = New clsSV_DB
                                If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                                    '接続失敗
                                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
                                End If
                                sbSQL.Clear()
                                sbSQL.AppendLine("SELECT ")
                                sbSQL.AppendLine(" LOC_L_CD")
                                sbSQL.AppendLine(" FROM")
                                sbSQL.AppendLine(" M_ASSET ")
                                sbSQL.AppendLine(" WHERE ")
                                sbSQL.AppendLine($" ASSET_NO = '{aseetNoList(i)}' ")

                                lstAseetDate = db.AssetListLOC_L_CDModel.SqlQuery(sbSQL.ToString).ToList


                                '変更後の大区分ロケ
                                Dim LOCL As String = lstAseetDate(0).LOC_L_CD.ToString


                                '資産マスタからロケ大区分コードと管理ロケ大区分コードの総数取得
                                Dim lstAseetMngLocLCNTDate As New List(Of AssetListMNG_LOC_L_CDCNTModel)
                                sbSQL.Clear()
                                sbSQL.AppendLine("SELECT ")
                                sbSQL.AppendLine(" COUNT(MNG_LOC_L_CD) As SUM_LOC_CD_CNT")
                                sbSQL.AppendLine(" FROM")
                                sbSQL.AppendLine(" M_ASSET ")
                                sbSQL.AppendLine(" WHERE ")
                                sbSQL.AppendLine($" MNG_LOC_L_CD = '{LOCL}' OR LOC_L_CD = '{LOCL}' ")
                                sbSQL.AppendLine("AND ASSET_STATUS <> 9 ")


                                lstAseetMngLocLCNTDate = db.AssetListMNG_LOC_L_CDCNTModel.SqlQuery(sbSQL.ToString).ToList

                                '総数
                                Dim intSumLocCnt As Integer = lstAseetMngLocLCNTDate(0).SUM_LOC_CD_CNT


                                'リミットを超えていた場合はロールバックFor文を抜ける
                                If sLimit < intSumLocCnt Then
                                    'ロールバック
                                    tran.Rollback()
                                    isCommit = False
                                    Exit For
                                End If

                                isCommit = True
                            Next

                            'Forを抜け、isCommitがTrueの場合コミット
                            If isCommit = True Then
                                tran.Commit()
                            End If
                        Else
                            'リミットが0の場合コミットする。
                            isCommit = True
                            tran.Commit()
                        End If
                    Else
                        'リミットが数値でない場合コミットする。
                        isCommit = True
                        tran.Commit()
                    End If


                    '2023/02/06 ADD End Shirakawa

                    '--2019/10/15 nv miyaji add start

                    '2023/02/13 ADD Start Shirakawa
                    If isCommit = True Then
                        '正常終了ダイアログ
                        _ViewModel.States = StatusType.SuccessEdit
                    Else
                        'ロケの登録上限数を超えたためロケを変更できません。
                        _ViewModel.ErrCode = CommonLabel.MSG_ERR_LOC_LIMIT_OVER
                    End If
                    '2023/02/13 ADD End Shirakawa

                    'チェック状態のクリア     '--2021/12/01 add nv iwamoto
                    _ViewModel.SelectKeys = String.Empty
                    _ViewModel.NoneSelectKeys = String.Empty
                    Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)

                    'モデルの内容をセッションに保持
                    Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY) = _ViewModel
                    '--2019/10/15 nv miyaji add end

                    '操作ログ出力
                    Dim tblAsset As DataTable
                    '2023/02/13 ADD Start Shirakawa
                    If isCommit = True Then
                        For ii As Integer = 0 To assetNoCondition.Count - 1
                            '操作ログ情報取得のため、更新前の資産Mデータを取得する

                            tblAsset = ToolInvAsset.GetInvAsset(invId, invLocL, assetNoCondition(ii))

                            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventorySummary.BUTTON_CHANGE_CURRENT_LOC) & " " &
                                                         Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                         Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " &
                                                         assetNoCondition(ii).Trim & "  " &
                                                         Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
                                                         CommonRfid.ToStringNvl(tblAsset.Rows(0)("ASSET_NAME").ToString).Trim
                            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo,
                                                           Me._Message.getMessage(CommonLabelInventorySummary.BUTTON_CHANGE_CURRENT_LOC),
                                                           tblAsset.Rows(0)("LOC_L_CD").ToString,
                                                           tblAsset.Rows(0)("LOC_M_CD").ToString,
                                                           tblAsset.Rows(0)("LOC_S_CD").ToString)
                        Next
                    Else

                        Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventorySummary.BUTTON_CHANGE_CURRENT_LOC) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_ERR_LOC_LIMIT_OVER)
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo,
                               Me._Message.getMessage(CommonLabelInventorySummary.BUTTON_CHANGE_CURRENT_LOC))
                    End If

                    '2023/02/13 ADD Start Shirakawa
                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)

                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

                End Try
            End Using


            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function


        ''' <summary>
        ''' 編集内容チェック
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <returns></returns>
        Public Function ValidateEditContain(ByVal ViewModel As InventoryInquiryViewModel) As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '棚卸進捗状況からのキー(棚卸ID/棚卸ロケ大区分コード)を取得
            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder

            Dim ret = New ApiResponse With {
                .ok = False
            }

            Try
                Dim errs As New List(Of String)
                Dim assetList = New List(Of String)
                Dim editedContain = JsonConvert.DeserializeObject(Of EditedContainModel)(ViewModel.EditJson)
                assetList.Add(editedContain.ASSET_NO)

                'ロック状態チェック
                Dim lockUsers = ToolInvAsset.GetLockUsers(invId, invLocL, assetList)
                If lockUsers.Count > 0 AndAlso lockUsers(0) <> _LoginInfo.EmployeeCd Then
                    'ログイン担当者以外がロックしている場合はエラー
                    ret.err = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_LOCK_OTHER_USER_DISABLE)
                    Return Json(ret)
                End If

                '[CC431] DEL 20261005 権限情報の取得（権限のチェックを削除したため）
                '[CC431] UPD Start 20261009 値のチェックを共通関数にまとめた。エラーの項目も返す（詳細モーダルでエラー項目を赤く表示するため）
                '値チェック
                Dim errColumn As String = Nothing
                Dim errCode As String = GetEditContainErrCode(invId, invLocL, editedContain, db, errColumn)
                If errCode IsNot Nothing Then
                    ret.err = Me._Message.getMessage(errCode)
                    ret.errColumn = errColumn
                    Return Json(ret)
                End If
                '[CC431] UPD End 20261009

                Return Json(New ApiResponse With {
                    .ok = True,
                    .message = "OK"
                })

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)

                ret.err = Me._Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)
                Return Json(ret)
            End Try

        End Function

        ''' <summary>
        ''' 編集処理
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <returns></returns>
        Public Function EditAsset(ByVal ViewModel As InventoryInquiryViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '棚卸進捗状況からのキー(棚卸ID/棚卸ロケ大区分コード)を取得
            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            '[CC431] DEL 20261009 params の宣言（棚卸資産Tの更新を共通関数にまとめたため）

            Dim ret = New ApiResponse With {
                .ok = False
            }

            Dim editedContain =
                JsonConvert.DeserializeObject(Of EditedContainModel)(ViewModel.EditJson)

            Using tran = db.Database.BeginTransaction()
                Try
                    Dim assetList = New List(Of String)
                    assetList.Add(editedContain.ASSET_NO)
                    Dim lockUsers = ToolInvAsset.GetLockUsers(invId, invLocL, assetList)
                    If lockUsers.Count > 0 AndAlso lockUsers(0) <> _LoginInfo.EmployeeCd Then
                        'ログイン担当者以外がロックしている場合はエラー
                        ret.err = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_LOCK_OTHER_USER_DISABLE)
                        Return Json(ret)
                    End If

                    '[CC431] ADD Start 20261009
                    '確認ダイアログの前（ValidateEditContain）と同じ値のチェックを、更新の直前にも行う。エラーの項目も返す
                    Dim errColumn As String = Nothing
                    Dim errCode As String = GetEditContainErrCode(invId, invLocL, editedContain, db, errColumn)
                    If errCode IsNot Nothing Then
                        ret.err = Me._Message.getMessage(errCode)
                        ret.errColumn = errColumn
                        Return Json(ret)
                    End If
                    '[CC431] ADD End 20261009

                    '先にＬＯＧ用のレコードを取得する
                    Dim tblAsset As DataTable = ToolInvAsset.GetInvAsset(invId, invLocL, editedContain.ASSET_NO)

                    '[CC431] UPD Start 20261009 棚卸資産Tの更新を共通関数にまとめた（WHERE に棚卸ID・棚卸ロケ大区分コードが加わる）
                    '棚卸資産T更新処理実行
                    UpdateInvAsset(db, sbSQL, invId, invLocL, editedContain, "InventoryInquiryController.EditAsset")
                    '[CC431] UPD End 20261009

                    'チェック状態のクリア
                    _ViewModel.SelectKeys = String.Empty
                    _ViewModel.NoneSelectKeys = String.Empty
                    Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)

                    '編集内容を持ち越さないように削除する
                    Session.Remove(CommonSessionKey.LIST_EDITED_CONTAINS)

                    '操作ログ出力
                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_INVENTORY_EDIT) & " " &
                                                    Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                    Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " & editedContain.ASSET_NO.Trim & "  " &
                                                    Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " & CommonRfid.ToStringNvl(tblAsset.Rows(0)("ASSET_NAME").ToString).Trim
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo,
                                                    Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_INVENTORY_EDIT),
                                                    tblAsset.Rows(0)("LOC_L_CD").ToString,
                                                    tblAsset.Rows(0)("LOC_M_CD").ToString,
                                                    tblAsset.Rows(0)("LOC_S_CD").ToString)

                    tran.Commit()

                    Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY) = ViewModel

                    Return Json(New ApiResponse With {
                        .ok = True,
                        .message = "OK"
                    })

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)

                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                End Try
            End Using

        End Function

        Private Sub SetErrMessage(ByRef errMsg As String, ByVal setMessage As String)
            If (String.IsNullOrEmpty(errMsg)) Then
                errMsg = setMessage
            End If
        End Sub

        '[CC431] ADD Start 20261005
        ''' <summary>
        ''' 新変更後設置場所コードの存在チェック（権限は見ない）
        ''' 変更後課コードが空の場合は、棚卸資産T.ロケ中区分コードとの組み合わせで判定する
        ''' </summary>
        ''' <param name="invId">棚卸ID</param>
        ''' <param name="invLocL">棚卸ロケ大区分コード</param>
        ''' <param name="editedContain">編集内容</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Private Function IsExistChangeLocS(invId As Long, invLocL As String, editedContain As EditedContainModel) As Boolean
            '変更後課コードがあれば変更後課コードを使用
            Dim locMCd As String = editedContain.CHANGE_LOC_M_CD
            If String.IsNullOrWhiteSpace(locMCd) Then
                '変更後課コードがなければ棚卸資産T.ロケ中区分コードを使用
                '※editedContain..LOC_M_CDは画面から送られないため空。棚卸資産Tから取得する
                Dim tblAsset As DataTable = ToolInvAsset.GetInvAsset(invId, invLocL, editedContain.ASSET_NO)
                If tblAsset.Rows.Count = 0 OrElse IsDBNull(tblAsset.Rows(0)("LOC_M_CD")) Then
                    Return False
                End If
                locMCd = tblAsset.Rows(0)("LOC_M_CD").ToString
            End If
            'ロケーションマスタに該当するロケ中区分コード＋ロケ小区分コードが存在するか(権限は見ない)
            Return ToolLocation.IsExistLocSCd(editedContain.LOC_L_CD, locMCd, editedContain.CHANGE_LOC_S_CD)
        End Function
        '[CC431] ADD End 20261005

        '[CC431] ADD Start 20261009
        ''' <summary>
        ''' 変更前の値のチェック（一覧の変更・詳細モーダルの変更で共通）
        ''' </summary>
        ''' <param name="invId">棚卸ID</param>
        ''' <param name="invLocL">棚卸ロケ大区分コード</param>
        ''' <param name="editedContain">編集内容</param>
        ''' <param name="db">DBコンテキスト</param>
        ''' <param name="errColumn">エラーの列（ChangeAssetResponse.errColumn に返す値）</param>
        ''' <returns>エラーがあれば文言コード、なければ Nothing</returns>
        Private Function GetEditContainErrCode(invId As Long, invLocL As String, editedContain As EditedContainModel,
                                               db As SystemDBContext, ByRef errColumn As String) As String
            '適切なロケであるかチェック
            If Not String.IsNullOrWhiteSpace(editedContain.CHANGE_LOC_M_CD) Then
                '変更後課コードが入っている場合のみ確認する
                'ロケーションマスタに該当する中区分コードが存在しているか(権限は見ない。会社内に存在するかをチェックする)
                Dim isExistM = ToolLocation.IsExistLocMCd(editedContain.LOC_L_CD, editedContain.CHANGE_LOC_M_CD)
                If (Not isExistM) Then
                    errColumn = "CHANGE_LOC_M_CD"
                    Return CommonLabelInventoryInquiry.MSG_LOC_M_ERROR
                End If
            End If

            If Not String.IsNullOrWhiteSpace(editedContain.CHANGE_LOC_S_CD) Then
                '変更後設置場所コードが入っている場合のみ確認する
                'ロケーションマスタに該当する小区分コードが存在しているか(権限は見ない。変更後課コードが空なら棚卸資産T.ロケ中区分コードで判定する)
                Dim isExistS = IsExistChangeLocS(invId, invLocL, editedContain)
                If (Not isExistS) Then
                    errColumn = "CHANGE_LOC_S_CD"
                    Return CommonLabelInventoryInquiry.MSG_LOC_S_ERROR
                End If
            End If

            If Not (String.IsNullOrWhiteSpace(editedContain.CHANGE_EMPLOYEE)) Then
                '変更後管理者社員番号が入っている場合のみ確認する
                '社員マスタに該当する社員コードが存在しているか
                Dim isExistEmployee = ToolEmployee.ExistEmployeeCd(db, editedContain.CHANGE_EMPLOYEE)
                If (Not isExistEmployee) Then
                    errColumn = "CHANGE_EMPLOYEE"
                    Return CommonLabelInventoryInquiry.MSG_EMPLOYEE_ERROR
                End If
            End If

            '稼働区分必須チェック
            If (String.IsNullOrWhiteSpace(editedContain.OPE_STATUS)) Then
                errColumn = "OPE_STATUS"
                Return CommonLabelInventoryInquiry.MSG_OPE_STATUS_ERROR
            End If

            If Not (String.IsNullOrWhiteSpace(editedContain.DIVERSION_REMOVE)) Then
                '転用or除却が入っている場合
                Dim diversionOrRemove = ToolSetting.GetSetting("Diversion_or_Remove", editedContain.DIVERSION_REMOVE, "**", "**")
                If diversionOrRemove IsNot Nothing AndAlso diversionOrRemove.DATA2 = "Y" Then
                    '転用or除却が「除却」の場合(設定マスタ.DATA2がY)
                    If String.IsNullOrWhiteSpace(editedContain.REMOVE_DATE) Then
                        '除却時期は必須
                        errColumn = "REMOVE_DATE"
                        Return CommonLabelInventoryInquiry.MSG_REMOVE_DATE_ERROR
                    Else
                        '除却時期が入っている場合は形式(yyyy/mm)をチェック
                        '詳細モーダルの入力UIでフォーマットされる形がyyyy-MMなので(一覧はyyyy/MMで送られる)
                        Dim relaceDate = editedContain.REMOVE_DATE.Replace("-", "/")
                        Dim valid = IsValidYearMonthByDate(relaceDate)
                        If (Not valid) Then
                            errColumn = "REMOVE_DATE"
                            Return CommonLabelInventorySummary.MSG_REMOVE_DATE_FORMAT
                        End If
                    End If
                End If
            End If

            If Not (String.IsNullOrWhiteSpace(editedContain.REMOVE_REASON)) Then
                '除却理由が入っている場合
                Dim removeReason = ToolSetting.GetSetting("Remove_Reason_Item", editedContain.REMOVE_REASON, "**", "**")
                '除却内容が必須の設定の場合(設定マスタ.DATA3がY)
                If (removeReason IsNot Nothing AndAlso removeReason.DATA3 = "Y" AndAlso String.IsNullOrWhiteSpace(editedContain.REMOVE_CONTENT)) Then
                    '除却内容は必須
                    errColumn = "REMOVE_CONTENT"
                    Return CommonLabelInventoryInquiry.MSG_REMOVE_CONTENT_ERROR
                End If
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 棚卸資産Tの更新（一覧の変更・詳細モーダルの変更で共通）
        ''' </summary>
        ''' <param name="db">DBコンテキスト（呼び出し元のトランザクション内で実行する）</param>
        ''' <param name="sbSQL">SQL（エラー時に呼び出し元でログに出力するため、呼び出し元の StringBuilder に組み立てる）</param>
        ''' <param name="invId">棚卸ID</param>
        ''' <param name="invLocL">棚卸ロケ大区分コード</param>
        ''' <param name="editedContain">編集内容</param>
        ''' <param name="updTask">最終更新タスク</param>
        Private Sub UpdateInvAsset(db As SystemDBContext, sbSQL As StringBuilder, invId As Long, invLocL As String,
                                   editedContain As EditedContainModel, updTask As String)
            Dim params As New List(Of SqlParameter)

            '除却時期は yyyy/MM で保存する
            Dim replaceDate As String = ""
            If Not String.IsNullOrEmpty(editedContain.REMOVE_DATE) Then
                replaceDate = editedContain.REMOVE_DATE.Replace("-", "/").ToString.Trim.TrimStart("'"c)
            End If

            sbSQL.Clear()
            sbSQL.AppendLine(" UPDATE T_INV_ASSET SET")
            sbSQL.AppendLine(" OPE_STATUS = @OPE_STATUS")
            sbSQL.AppendLine(" ,DIVERSION_REMOVE = @DIVERSION_REMOVE")
            sbSQL.AppendLine(" ,REMOVE_DATE = @REMOVE_DATE")
            sbSQL.AppendLine(" ,REMOVE_REASON = @REMOVE_REASON")
            sbSQL.AppendLine(" ,REMOVE_CONTENT = @REMOVE_CONTENT")
            sbSQL.AppendLine(" ,CHANGE_LOC_M_CD = @CHANGE_LOC_M_CD")
            sbSQL.AppendLine(" ,CHANGE_LOC_S_CD = @CHANGE_LOC_S_CD")
            sbSQL.AppendLine(" ,CHANGE_EMPLOYEE = @CHANGE_EMPLOYEE")
            sbSQL.AppendLine(" ,REMARK1 = @REMARK1")
            sbSQL.AppendLine(" ,REMARK2 = @REMARK2")
            sbSQL.AppendLine(" ,REMARK3 = @REMARK3")
            sbSQL.AppendLine(" ,REMARK4 = @REMARK4")
            sbSQL.AppendLine(" ,REMARK5 = @REMARK5")
            sbSQL.AppendLine(" ,REMARK6 = @REMARK6")
            sbSQL.AppendLine(" ,REMARK7 = @REMARK7")
            sbSQL.AppendLine(" ,REMARK8 = @REMARK8")
            sbSQL.AppendLine(" ,REMARK9 = @REMARK9")
            sbSQL.AppendLine(" ,REMARK10 = @REMARK10")
            sbSQL.AppendLine(" ,ENT_EMPLOYEE_CD = @ENT_EMPLOYEE_CD")
            sbSQL.AppendLine(" ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
            sbSQL.AppendLine(" ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
            sbSQL.AppendLine(" ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
            sbSQL.AppendLine(" ,UPD_TASK = @UPD_TASK")
            sbSQL.AppendLine(" WHERE ASSET_NO = @ASSET_NO")
            sbSQL.AppendLine(" AND INV_ID = @INV_ID")
            sbSQL.AppendLine(" AND INV_LOC_L_CD = @INV_LOC_L_CD")

            '値が Nothing の場合は、以前の文字列連結と同じく空文字で更新する
            params.Add(New SqlParameter("@OPE_STATUS", If(editedContain.OPE_STATUS, "")))
            params.Add(New SqlParameter("@DIVERSION_REMOVE", If(editedContain.DIVERSION_REMOVE, "")))
            params.Add(New SqlParameter("@REMOVE_DATE", replaceDate))
            params.Add(New SqlParameter("@REMOVE_REASON", If(editedContain.REMOVE_REASON, "")))
            params.Add(New SqlParameter("@REMOVE_CONTENT", If(editedContain.REMOVE_CONTENT, "")))
            params.Add(New SqlParameter("@CHANGE_LOC_M_CD", If(editedContain.CHANGE_LOC_M_CD, "")))
            params.Add(New SqlParameter("@CHANGE_LOC_S_CD", If(editedContain.CHANGE_LOC_S_CD, "")))
            params.Add(New SqlParameter("@CHANGE_EMPLOYEE", If(editedContain.CHANGE_EMPLOYEE, "")))
            params.Add(New SqlParameter("@REMARK1", If(editedContain.REMARK1, "")))
            params.Add(New SqlParameter("@REMARK2", If(editedContain.REMARK2, "")))
            params.Add(New SqlParameter("@REMARK3", If(editedContain.REMARK3, "")))
            params.Add(New SqlParameter("@REMARK4", If(editedContain.REMARK4, "")))
            params.Add(New SqlParameter("@REMARK5", If(editedContain.REMARK5, "")))
            params.Add(New SqlParameter("@REMARK6", If(editedContain.REMARK6, "")))
            params.Add(New SqlParameter("@REMARK7", If(editedContain.REMARK7, "")))
            params.Add(New SqlParameter("@REMARK8", If(editedContain.REMARK8, "")))
            params.Add(New SqlParameter("@REMARK9", If(editedContain.REMARK9, "")))
            params.Add(New SqlParameter("@REMARK10", If(editedContain.REMARK10, "")))
            params.Add(New SqlParameter("@ENT_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter("@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter("@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
            params.Add(New SqlParameter("@UPD_TASK", updTask))
            params.Add(New SqlParameter("@ASSET_NO", editedContain.ASSET_NO))
            params.Add(New SqlParameter("@INV_ID", invId))
            params.Add(New SqlParameter("@INV_LOC_L_CD", invLocL))

            db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)
        End Sub
        '[CC431] ADD End 20261009

        '[CC431] UPD Start 20261007 現物確認から呼ぶときにチェック状態を消さないよう引数 KeepChecked を追加
        ''' <summary>
        ''' 変更処理
        ''' </summary>
        ''' <param name="KeepChecked">True：チェック状態を消さない（現物確認から呼ぶ場合）。省略時は False</param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function ChangeAsset(ViewModel As InventoryInquiryViewModel, Optional KeepChecked As Boolean = False) As ActionResult
            '[CC431] UPD End 20261007
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            '棚卸進捗状況からのキー(棚卸ID/棚卸ロケ大区分コード)を取得
            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder

            Dim editedContains As List(Of EditedContainModel) =
                JsonConvert.DeserializeObject(Of List(Of EditedContainModel))(ViewModel.EditJson)

            '[CC431] DEL 20261005 権限情報の取得（権限のチェックを削除したため）

            For Each editedContain In editedContains
                Dim response = New ChangeAssetResponse() With {
                    .ok = False,
                    .errAsset = editedContain.ASSET_NO
                }

                'ロックされていないか確認
                Dim isEditable = ToolInvAsset.IsEditable(invId, invLocL, editedContain.ASSET_NO, _LoginInfo.EmployeeCd)
                If Not isEditable Then
                    'ログイン担当者以外がロックしている場合はエラー
                    response.errColumn = "LOCK"
                    response.errMsg = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_LOCK_OTHER_USER_DISABLE)
                    Return Json(response)
                End If

                '[CC431] UPD Start 20261009 値のチェックを共通関数にまとめた
                '値チェック
                Dim errColumn As String = Nothing
                Dim errCode As String = GetEditContainErrCode(invId, invLocL, editedContain, db, errColumn)
                If errCode IsNot Nothing Then
                    response.errColumn = errColumn
                    response.errMsg = Me._Message.getMessage(errCode)
                    Return Json(response)
                End If
                '[CC431] UPD End 20261009
            Next

            '変更処理
            Using tran = db.Database.BeginTransaction()
                Try

                    For Each editedContain In editedContains
                        Dim tblAsset As DataTable = ToolInvAsset.GetInvAsset(invId, invLocL, editedContain.ASSET_NO)

                        editedContain.ASSET_NAME = If(IsDBNull(tblAsset.Rows(0)("ASSET_NAME")), Nothing, tblAsset.Rows(0)("ASSET_NAME"))
                        editedContain.LOC_L_CD = If(IsDBNull(tblAsset.Rows(0)("LOC_L_CD")), Nothing, tblAsset.Rows(0)("LOC_L_CD"))
                        editedContain.LOC_M_CD = If(IsDBNull(tblAsset.Rows(0)("LOC_M_CD")), Nothing, tblAsset.Rows(0)("LOC_M_CD"))
                        editedContain.LOC_S_CD = If(IsDBNull(tblAsset.Rows(0)("LOC_S_CD")), Nothing, tblAsset.Rows(0)("LOC_S_CD"))

                        '[CC431] UPD Start 20261009 棚卸資産Tの更新を共通関数にまとめた。最終更新タスクは ChangeAsset にする（以前は EditAsset のままだった）
                        '棚卸資産T更新処理実行
                        UpdateInvAsset(db, sbSQL, invId, invLocL, editedContain, "InventoryInquiryController.ChangeAsset")
                        '[CC431] UPD End 20261009
                    Next

                    For Each editedContain In editedContains
                        '操作ログ出力

                        Dim sProcessContents As String = Me._Message.getMessage(CommonLabel.BUTTON_CHANGE) & " " &
                                                        Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                        Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " & editedContain.ASSET_NO.Trim & "  " &
                                                        Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
                                                        CommonRfid.ToStringNvl(editedContain.ASSET_NAME).Trim
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo,
                                                        Me._Message.getMessage(CommonLabel.BUTTON_CHANGE),
                                                        invLocL,
                                                        editedContain.LOC_M_CD,
                                                        editedContain.LOC_S_CD)
                    Next

                    'チェック状態のクリア
                    '[CC431] UPD Start 20261007 現物確認から呼ばれた場合は、続けて現物確認を行うためチェック状態を消さない
                    If Not KeepChecked Then
                        _ViewModel.SelectKeys = String.Empty
                        _ViewModel.NoneSelectKeys = String.Empty
                        Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                    End If
                    '[CC431] UPD End 20261007

                    '編集内容を持ち越さないように削除する
                    Session.Remove(CommonSessionKey.LIST_EDITED_CONTAINS)

                    tran.Commit()

                    Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY) = ViewModel

                    Dim ret = New ChangeAssetResponse() With {
                        .ok = True
                    }

                    Return Json(ret)
                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)

                    Dim ret = New ChangeAssetResponse() With {
                        .ok = False,
                        .errMsg = String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_SYSTEM))
                    }

                    'Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                    Return Json(ret)
                End Try
            End Using

            '結果表示
            'Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})

        End Function

        Private Function IsValidYearMonthByDate(value As String) As Boolean
            Dim pattern As String = "^\d{4}/(0[1-9]|1[0-2])$"
            Return Regex.IsMatch(value, pattern)
        End Function

        '[CC431] ADD Start 20261007
        ''' <summary>
        ''' 現物確認の事前チェック
        ''' </summary>
        ''' <param name="dt">選択された資産の棚卸資産T</param>
        ''' <returns>エラーがあれば文言コード、なければ Nothing</returns>
        Private Function GetGenbutsuKakuninErrCode(dt As DataTable) As String
            For Each row As DataRow In dt.Rows
                '選択された資産が棚卸中でない場合はエラー
                If row.Field(Of String)("PHYINV_STATUS").Equals(PHYINV_STATUS.Excluded) Then
                    Return CommonErrorMsg.PHYINV_STATUS_CHECKBOX_NOTINVENTORY
                End If

                '正常が混じっていたらNG
                If row.Field(Of String)("PHYINV_STATUS").Equals(PHYINV_STATUS.Normal) Then
                    Return CommonLabelInventoryInquiry.MSG_CONTAIN_NORMAL
                End If

                '他担当者がロックした資産の場合NG
                Dim lockEmployee = row.Field(Of String)("LOCK_EMPLOYEE_CD")
                If Not String.IsNullOrEmpty(lockEmployee) AndAlso Not lockEmployee.Equals(_LoginInfo.EmployeeCd) Then
                    Return CommonLabelInventoryInquiry.MSG_LOCK_OTHER_USER_DISABLE
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' 現物確認の事前チェック（画面から ajax で呼ぶ）
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function ValidateGenbutsuKakunin(ByVal ViewModel As InventoryInquiryViewModel) As ActionResult
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

            '最新のチェック状態でセッション情報を更新する(別ページのチェックも取るため)
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            '棚卸進捗状況からのキー(棚卸ID/棚卸ロケ大区分コード)を取得
            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            Dim dt = ToolInvAsset.GetInvAssets(invId, invLocL, lstSelectKeys)

            '現物確認事前チェック
            Dim errCode As String = GetGenbutsuKakuninErrCode(dt)
            Dim response = New ChangeAssetResponse() With {
                .ok = errCode Is Nothing
            }
            If errCode IsNot Nothing Then
                'エラーがある場合はエラーメッセージを設定
                response.errMsg = Me._Message.getMessage(errCode)
            End If
            Return Json(response)
        End Function
        '[CC431] ADD End 20261007

        ''' <summary>
        ''' 現物確認処理
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="OnCreteButton"></param>
        ''' <returns></returns>
        <ValidateInput(False)>
        Public Function UpdateGenbutsuKakunin(ByVal ViewModel As InventoryInquiryViewModel, ByVal OnCreteButton As String) As ActionResult
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

            '最新のチェック状態でセッション情報を更新する(追加および削除) 　　別ページのチェックも取るため、このコードで、チェック内容を取得する
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)
            'Dim sKeyStr As String() = String.Join(",", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(InventoryInquiryListKey.ASSET_NO))).Split(",")
            Dim sKeyStr As String() = lstSelectKeys.ToArray
            Dim sqlKey As String = String.Empty
            sqlKey = "'"
            For Each str As String In sKeyStr
                If sqlKey.Length > 1 Then
                    sqlKey = sqlKey & "','"
                End If
                sqlKey = sqlKey & str
            Next
            sqlKey = sqlKey & "'"

            '棚卸進捗状況からのキー(棚卸ID/棚卸ロケ大区分コード)を取得
            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            Dim dt = ToolInvAsset.GetInvAssets(invId, invLocL, lstSelectKeys)

            '選択された資産が棚卸中でない場合はエラー
            Dim db As SystemDBContext = New SystemDBContext
            '[CC431] UPD Start 20261007 チェックを現物確認の事前チェックの共通処理で行う（確認用アクションと同じチェック）
            Dim errCode As String = GetGenbutsuKakuninErrCode(dt)
            If errCode IsNot Nothing Then
                'エラー
                _ViewModel.ErrCode = errCode
                'モデルの内容をセッションに保持
                Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY) = _ViewModel
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If
            '[CC431] UPD End 20261007

            'WebApiUrl取得
            Dim setting_WebApiUrl As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.WebAPI_Url.KeyDiv, CommonSettingKey.WebAPI_Url.Key1, CommonSettingKey.WebAPI_Url.Key2, CommonSettingKey.WebAPI_Url.Key3, True)
            Dim sUrl As String = setting_WebApiUrl.DATA1

            Dim wsAssetInventory As New WsAssetInventory(sUrl)          'Webservice呼出クラス宣言
            Dim DtoWsInventory As List(Of WsAssetInventory.DtoAssetInventory) = New List(Of AsetraLibWebApi.WsAssetInventory.DtoAssetInventory)
            For Each row As DataRow In dt.Rows
                Dim param As WsAssetInventory.DtoAssetInventory = New WsAssetInventory.DtoAssetInventory
                param.an = row.Field(Of String)("ASSET_NO")
                param.pt = Now.ToString("yyyyMMddHHmmss")
                param.ps = Misc.PHYINV_STATUS.Normal
                param.lc = row.Field(Of String)("LOC_L_CD")
                param.mc = row.Field(Of String)("LOC_M_CD")
                param.sc = row.Field(Of String)("LOC_S_CD")
                param.ec = _LoginInfo.EmployeeCd

                DtoWsInventory.Add(param)
            Next

            '実棚情報更新(WebAPI)　※処理ツール区分はコマンド送信メソッド内でセットしている
            Dim Result As ClsWebApiClient.EnResultAPI = wsAssetInventory.WebApiAssetInventory(DtoWsInventory)
            Select Case Result
                Case ClsWebApiClient.EnResultAPI.OK_Result
                    '正常応答
                    _ViewModel.States = StatusType.SuccessLending
                    'チェック状態のクリア
                    _ViewModel.SelectKeys = String.Empty
                    _ViewModel.NoneSelectKeys = String.Empty
                    Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)

                    '操作ログ出力
                    For Each row As DataRow In dt.Rows
                        Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryInquiry.BUTTON_GENBUTSU_KAKUNIN) & " " &
                                                             Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " &
                                                             row.Field(Of String)("ASSET_NO") & "  " &
                                                             Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
                                                             row.Field(Of String)("ASSET_NAME")
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.BUTTON_GENBUTSU_KAKUNIN))
                    Next

                Case ClsWebApiClient.EnResultAPI.NoAnswer   '応答なし
                    'WebAPIの応答がありません。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_NO_RESPONSE

                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabel.MSG_ERR_API_NO_RESPONSE) & " " &
                                                    sUrl
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.BUTTON_GENBUTSU_KAKUNIN))

                Case Else 'ClsWebApiClient.EnResultAPI.Other_Error  'その他異常
                    'WebAPIから異常応答がありました。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_IJOU

                    Dim sProcessContents As String = Me._Message.getMessage(CommonLabel.MSG_ERR_API_IJOU) & " " &
                                                    sUrl
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.BUTTON_GENBUTSU_KAKUNIN))

            End Select

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY) = _ViewModel

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function

        ''' <summary>
        ''' 現物未確認処理
        ''' </summary>
        ''' <param name="ViewModel"></param>
        ''' <param name="OnCreteButton"></param>
        ''' <returns></returns>
        ''' <remarks>使用していない</remarks>
        <ValidateInput(False)>
        Public Function UpdateGenbutsuMikakunin(ByVal ViewModel As InventoryInquiryViewModel, ByVal OnCreteButton As String) As ActionResult
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

            '最新のチェック状態でセッション情報を更新する(追加および削除) 　　別ページのチェックも取るため、このコードで、チェック内容を取得する
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)
            'Dim sKeyStr As String() = String.Join(",", lstSelectKeys.Select(Function(x) x.Split(",")).Select(Function(X) X(InventoryInquiryListKey.ASSET_NO))).Split(",")
            Dim sKeyStr As String() = lstSelectKeys.ToArray
            Dim sqlKey As String = String.Empty
            sqlKey = "'"
            For Each str As String In sKeyStr
                If sqlKey.Length > 1 Then
                    sqlKey = sqlKey & "','"
                End If
                sqlKey = sqlKey & str
            Next
            sqlKey = sqlKey & "'"

            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            Dim dt = ToolInvAsset.GetInvAssets(invId, invLocL, lstSelectKeys)

            'WebApiUrl取得
            Dim setting_WebApiUrl As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.WebAPI_Url.KeyDiv, CommonSettingKey.WebAPI_Url.Key1, CommonSettingKey.WebAPI_Url.Key2, CommonSettingKey.WebAPI_Url.Key3, True)
            Dim sUrl As String = setting_WebApiUrl.DATA1

            Dim wsAssetInventory As New WsAssetInventory(sUrl)          'Webservice呼出クラス宣言
            Dim DtoWsInventory As List(Of WsAssetInventory.DtoAssetInventory) = New List(Of AsetraLibWebApi.WsAssetInventory.DtoAssetInventory)
            For Each row As DataRow In dt.Rows
                Dim param As WsAssetInventory.DtoAssetInventory = New WsAssetInventory.DtoAssetInventory
                param.an = row.Field(Of String)("ASSET_NO")
                param.pt = Now.ToString("yyyyMMddHHmmss")
                param.ps = Misc.PHYINV_STATUS.NoSpot
                param.lc = row.Field(Of String)("PHYINV_LOC_L_CD")
                param.mc = row.Field(Of String)("PHYINV_LOC_M_CD")
                param.sc = row.Field(Of String)("PHYINV_LOC_S_CD")
                param.ec = _LoginInfo.EmployeeCd

                DtoWsInventory.Add(param)
            Next

            Dim Result As ClsWebApiClient.EnResultAPI = wsAssetInventory.WebApiAssetInventory(DtoWsInventory)
            Select Case Result
                Case ClsWebApiClient.EnResultAPI.OK_Result
                    '正常応答
                    _ViewModel.States = StatusType.SuccessLending
                    'チェック状態のクリア
                    _ViewModel.SelectKeys = String.Empty
                    _ViewModel.NoneSelectKeys = String.Empty
                    Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)

                Case ClsWebApiClient.EnResultAPI.NoAnswer   '応答なし
                    'WebAPIの応答がありません。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_NO_RESPONSE

                Case Else 'ClsWebApiClient.EnResultAPI.Other_Error  'その他異常
                    'WebAPIから異常応答がありました。
                    _ViewModel.ErrCode = CommonLabel.MSG_ERR_API_IJOU
            End Select

            'モデルの内容をセッションに保持
            Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY) = _ViewModel

            '操作ログ出力
            For Each row As DataRow In dt.Rows

                Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryInquiry.BUTTON_GENBUTSU_MIKAKUNIN) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " &
                                                     row.Field(Of String)("ASSET_NO") & "  " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
                                                     row.Field(Of String)("ASSET_NAME")
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.BUTTON_GENBUTSU_MIKAKUNIN))
            Next

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function

        ''' <summary>
        ''' 全選択ボタン押下時処理
        ''' 表示されている検索条件で全件検索し
        ''' チェック項目としてセッションにセットする
        ''' </summary>
        ''' <param name="blnSelectAll"></param>
        ''' <returns></returns>
        Public Function SelectAll(ByVal blnSelectAll As Boolean) As ActionResult
            Dim sbSQL As New StringBuilder
            Dim lstList As New List(Of InventoryInquiryViewModel)

            '選択解除の場合、初期化する
            If Not blnSelectAll Then
                Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)
                '全選択ボタン押下フラグをクリアする
                Session.Remove(CommonSessionKey.ALL_SELECT_FLG)
                Return Json("0")
            Else
                '全選択ボタン押下フラグをセットする
                Session(CommonSessionKey.ALL_SELECT_FLG) = True
            End If

            '現在表示検索時のSQLを取得する
            Dim strSQL As String = Session(CommonSessionKey.LIST_VIEW_SQL)
            Dim lstParam As List(Of SqlParameter) = Session(CommonSessionKey.LIST_VIEW_PARAM)

            'パラメーターをコピーする(SqlParameterがSqlParameterCollectionに含まれるエラー回避のため)
            Dim lstParamCopy As New List(Of SqlParameter)
            Dim param As New SqlParameter
            For Each param In lstParam
                lstParamCopy.Add(New SqlParameter(param.ParameterName, param.Value))
            Next

            'SQLを実行する
            If Not IsNothing(strSQL) Then
                If lstParam.Count <= 0 Then
                    lstList = _db.InventoryInquiryView.SqlQuery(strSQL).ToList
                Else

                    lstList = _db.InventoryInquiryView.SqlQuery(strSQL, lstParamCopy.ToArray).ToList
                End If
            End If

            'チェックボックスlstAssetListの選択状態のキー(資産番号)を更新する
            If lstList.Count > 0 Then
                Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstList.Select(Function(i) $"{i.ASSET_NO}").ToList
            End If

            '件数を返却する
            Return Json(lstList.Count)

        End Function


#Region "登録・更新処理"
        ''' <summary>
        ''' 登録処理のチェック処理をします
        ''' <returns>正常：True、エラー：False</returns>
        ''' </summary>
        Protected Overrides Function CreateChcek() As Boolean
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
        End Sub
#End Region

#Region "(ロケーション)大区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)大区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLocationLData() As JsonResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationLData()
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            ViewBag.modelLocL = New SelectList(lstLocation, "LOC_CD", "LOC_NAME", _invLocLCd) '初期表示時に選択したテキスト表示 20231012 ADD Shirakawa
            _ViewModel.locLDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "(ロケーション)中区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)中区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>使用していない</remarks>
        Private Function GetLocationMData(ByVal selectLCode As String, authInfo As AuthInfo) As JsonResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationMData(selectLCode, _LoginInfo, authInfo)
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            '検索条件の中区分・変更後中区分コンボ設定（複数選択のため MultiSelectList。選択状態はそれぞれ別に持つ）
            ViewBag.modelLocM = New MultiSelectList(lstLocation, "LOC_CD", "LOC_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchLocM))
            ViewBag.modelChangeLocM = New MultiSelectList(lstLocation, "LOC_CD", "LOC_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchChangeLocMCd))
            _ViewModel.locLDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

#Region "(ロケーション)小区分ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (ロケーション)小区分ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>使用していない</remarks>
        Private Function GetLocationSData(ByVal selectLCode As String, ByVal selectMCode As String, Optional ByVal isChangeColumn As Boolean = False) As JsonResult
            Dim tbl As DataTable = Dao.ToolLocation.GetLocationSData(selectLCode, selectMCode)
            Dim lstLocation As New List(Of LocationDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            If Not isChangeColumn Then
                ViewBag.modelLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME")
            Else
                ViewBag.modelChangeLocS = New SelectList(lstLocation, "LOC_CD", "LOC_NAME")
            End If

            _ViewModel.locLDropdownData = lstLocation

            Return Json(lstLocation)
        End Function
#End Region

        '[CC431] ADD Start 20261007
#Region "検索条件 ロケーション・変更後ロケーションの選択肢（場所から選ぶの連動）"
        ''' <summary>
        ''' 検索条件の 4 つのコンボ（原価センタ・場所・変更後課・変更後設置場所）の選択肢を、場所から選ぶの ON／OFF と選択値に合わせて作る
        ''' </summary>
        ''' <param name="invId">棚卸ID</param>
        ''' <param name="invLocL">棚卸ロケ大区分コード</param>
        ''' <param name="authInfo">権限情報</param>
        Private Sub SetSearchLocationOptions(invId As Long, invLocL As String, authInfo As AuthInfo)
            '検索条件(複数指定)
            Dim selLocM As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocM)
            Dim selLocS As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchLocS)
            Dim selChangeLocM As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchChangeLocMCd)
            Dim selChangeLocS As String() = CommonRfid.GetSelectedValues(_ViewModel.SearchChangeLocSCd)

            'ロケーション 原価センタ・場所
            ViewBag.modelLocM = New MultiSelectList(
                GetSearchLocationList(SEARCH_LOC_TARGET_LOC, LOC_DIV.LocationM, _ViewModel.SearchLocReverse, selLocS, invId, invLocL, authInfo),
                "LOC_CD", "LOC_NAME", selLocM)
            ViewBag.modelLocS = New MultiSelectList(
                GetSearchLocationList(SEARCH_LOC_TARGET_LOC, LOC_DIV.LocationS, _ViewModel.SearchLocReverse, selLocM, invId, invLocL, authInfo),
                "LOC_CD", "LOC_NAME", selLocS)

            '変更後課コード・新変更後設置場所コード
            ViewBag.modelChangeLocM = New MultiSelectList(
                GetSearchLocationList(SEARCH_LOC_TARGET_CHANGE_LOC, LOC_DIV.LocationM, _ViewModel.SearchChangeLocReverse, selChangeLocS, invId, invLocL, authInfo),
                "LOC_CD", "LOC_NAME", selChangeLocM)
            ViewBag.modelChangeLocS = New MultiSelectList(
                GetSearchLocationList(SEARCH_LOC_TARGET_CHANGE_LOC, LOC_DIV.LocationS, _ViewModel.SearchChangeLocReverse, selChangeLocM, invId, invLocL, authInfo),
                "LOC_CD", "LOC_NAME", selChangeLocS)
        End Sub

        ''' <summary>
        ''' 検索条件のロケーションの選択肢を取得する
        ''' 場所から選ぶ OFF：中区分は絞り込みなし、小区分は選択中の中区分で絞り込む
        ''' 場所から選ぶ ON ：小区分は絞り込みなし、中区分は選択中の小区分で絞り込む
        ''' 絞り込まれる側で、絞り込む側が未選択の場合は空
        ''' </summary>
        ''' <param name="target">Loc：原価センタ・場所、ChangeLoc：変更後課・変更後設置場所</param>
        ''' <param name="level">取得する側（M：中区分、S：小区分）</param>
        ''' <param name="reverse">場所から選ぶ</param>
        ''' <param name="filterCodes">反対側の選択中のコード</param>
        ''' <param name="invId">棚卸ID</param>
        ''' <param name="invLocL">棚卸ロケ大区分コード</param>
        ''' <param name="authInfo">権限情報</param>
        ''' <returns>選択肢</returns>
        Private Function GetSearchLocationList(target As String, level As String, reverse As Boolean, filterCodes As String(),
                                               invId As Long, invLocL As String, authInfo As AuthInfo) As List(Of LocationDropdownModel)
            Dim lstLocation As New List(Of LocationDropdownModel)

            '絞り込まれる側か（OFF なら小区分、ON なら中区分）
            Dim isFiltered As Boolean = If(reverse, level = LOC_DIV.LocationM, level = LOC_DIV.LocationS)
            Dim codes As String() = Nothing
            If isFiltered Then
                codes = CommonRfid.GetSelectedValues(filterCodes)
                If codes.Length = 0 Then
                    '絞り込む側が未選択
                    Return lstLocation
                End If
            End If

            Dim tbl As DataTable
            If target = SEARCH_LOC_TARGET_CHANGE_LOC Then
                '変更後課・変更後設置場所（棚卸資産Tの変更後のコード。権限は見ない）
                If level = LOC_DIV.LocationM Then
                    tbl = ToolInvAsset.GetChangeLocMData(invId, invLocL, codes)
                Else
                    tbl = ToolInvAsset.GetChangeLocSData(invId, invLocL, codes)
                End If
            Else
                '原価センタ・場所（権限あり かつ 棚卸資産Tに存在するもの）
                Dim exTarget As Dao.ToolLocation.LocExistTarget = Dao.ToolLocation.LocExistTarget.InvAsset(invId, invLocL)
                If level = LOC_DIV.LocationM Then
                    tbl = Dao.ToolLocation.GetLocationMDataExists(invLocL, _LoginInfo, authInfo, exTarget, codes)
                Else
                    tbl = Dao.ToolLocation.GetLocationSDataExists(invLocL, _LoginInfo, authInfo, exTarget, codes)
                End If
            End If

            For Each row As DataRow In tbl.Rows
                lstLocation.Add(
                    New LocationDropdownModel With {
                        .LOC_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next

            Return lstLocation
        End Function
#End Region
        '[CC431] ADD End 20261007

#Region "(カテゴリー)大分類ドロップダウンリストデータ取得"
        ''' <summary>
        ''' (カテゴリー)大分類ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetCategoryLData() As JsonResult
            Dim tbl As DataTable = Dao.ToolCategory.GetCategoryLData()
            Dim lstCategory As New List(Of CategoryDropdownModel)     'ロケーションマスタリスト

            For Each row As DataRow In tbl.Rows
                lstCategory.Add(
                    New CategoryDropdownModel With {
                        .CAT_CD = row("CAT_CD").ToString,
                        .CAT_NAME = row("CAT_NAME").ToString
                    }
                )
            Next

            '検索条件のカテゴリー大分類コンボ設定（複数選択のため MultiSelectList）
            ViewBag.modelCatL = New MultiSelectList(lstCategory, "CAT_CD", "CAT_NAME", CommonRfid.GetSelectedValues(_ViewModel.SearchCatL))
            _ViewModel.catLDropdownData = lstCategory

            Return Json(lstCategory)
        End Function
#End Region

#Region "フリー項目10（資産貸与状態区分）ドロップダウンリストデータ取得"
        ''' <summary>
        ''' フリー項目10（資産貸与状態区分）ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetFreeItem10Data() As JsonResult
            Dim lst As New List(Of DropdownModel)
            lst.Add(
                    New DropdownModel With {
                        .Cd = LOAN_ASSET_STATUS.BLANK,
                        .Name = LOAN_ASSET_STATUS_VALUE.BLANK
                    }
                )
            lst.Add(
                    New DropdownModel With {
                        .Cd = LOAN_ASSET_STATUS.ON_LOAN,
                        .Name = LOAN_ASSET_STATUS_VALUE.ON_LOAN
                    }
                )
            lst.Add(
                    New DropdownModel With {
                        .Cd = LOAN_ASSET_STATUS.RETURNED,
                        .Name = LOAN_ASSET_STATUS_VALUE.RETURNED
                    }
                )

            '一覧画面のフリー項目10（資産貸与状態区分）コンボ設定
            ViewBag.FreeItem10List = New SelectList(lst, "Cd", "Name", _ViewModel.SearchFreeItem10)

            Return Json(lst)
        End Function
#End Region

#Region "タグ紐づけ状態ドロップダウンリストデータ取得"
        ''' <summary>
        ''' タグ紐づけ状態ドロップダウンリストデータ取得
        ''' </summary>
        ''' <returns></returns>
        Private Function GetTagLinkStatusData() As JsonResult
            Dim lst As List(Of DropdownModel) = New List(Of DropdownModel) From {
                New DropdownModel With {
                    .Cd = "",
                    .Name = Me._Message.getMessage(CommonLabel.LABEL_SELECT_ALL).ToString
                },
                New DropdownModel With {
                    .Cd = TagLinkStatus.Sumi,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_SUMI).ToString
                },
                New DropdownModel With {
                    .Cd = TagLinkStatus.Mi,
                    .Name = Me._Message.getMessage(CommonLabel.VALUE_MI).ToString
                }
            }

            '一覧画面のタグ紐づけ状態コンボ設定
            ViewBag.TagLinkStatusList = New SelectList(lst, "Cd", "Name", _ViewModel.SearchTagLinkStatus)

            Return Json(lst)
        End Function
#End Region

#Region "設定Mからドロップダウンリスト取得"
        ''' <summary>
        ''' 設定Mからドロップダウンリスト取得
        ''' </summary>
        Private Function GetSettingMDropListData(key As String, Optional keyColumn As String = "DATA1", Optional valueColumn As String = "DATA2") As List(Of DropdownModel)
            Dim dropTbl As DataTable = Dao.ToolSetting.GetItemDropDownInfo(key)
            Dim dropLst As New List(Of DropdownModel)
            For Each rowDrop As DataRow In dropTbl.Rows
                dropLst.Add(
                    New DropdownModel With {
                        .Cd = rowDrop(keyColumn).ToString,
                        .Name = rowDrop(valueColumn).ToString
                    }
                )
            Next
            Return dropLst
        End Function
#End Region

        Private Function GetEmployeeList() As List(Of EmployeeModel)
            Dim tblEmployee = Dao.ToolEmployee.GetEmployeeDataAll()
            Dim ret = New List(Of EmployeeModel)
            For Each row In tblEmployee.Rows
                Dim e = New EmployeeModel()
                e.EMPLOYEE_CD = row("EMPLOYEE_CD")
                e.EMPLOYEE_NAME = row("EMPLOYEE_NAME")
                ret.Add(e)
            Next
            Return ret
        End Function

        Private Function GetLocationList() As List(Of LocationModel)
            Dim tbl = Dao.ToolLocation.GetLocationLData()
            Dim ret = New List(Of LocationModel)
            For Each row As DataRow In tbl.Rows
                ret.Add(
                    New LocationModel With {
                        .LOC_L_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next
            Return ret
        End Function

        Private Function GetLocationMList(locLCd As String, sEmployeeCd As String, authInfo As AuthInfo) As List(Of LocationModel)
            '[CC431] UPD Start 20261002
            ' 一覧・詳細モーダルの変更後課の選択肢は、検索条件とは別に、画面の会社の全原価センタ（権限なし）
            Dim tbl = Dao.ToolLocation.GetLocationMData(locLCd)
            '[CC431] UPD End 20261002
            Dim ret = New List(Of LocationModel)
            For Each row As DataRow In tbl.Rows
                ret.Add(
                    New LocationModel With {
                        .LOC_L_CD = locLCd.ToString,
                        .LOC_M_CD = row("LOC_CD").ToString,
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next
            Return ret
        End Function

        Private Function GetLocationSList(locLCd As String) As List(Of LocationModel)
            Dim tbl = Dao.ToolLocation.GetLocationSData(locLCd)
            Dim ret = New List(Of LocationModel)
            For Each row As DataRow In tbl.Rows
                ret.Add(
                    New LocationModel With {
                        .LOC_L_CD = locLCd.ToString,
                        .LOC_M_CD = row("LOC_M_CD"),
                        .LOC_S_CD = row("LOC_CD"),
                        .LOC_NAME = row("LOC_NAME").ToString
                    }
                )
            Next
            Return ret
        End Function


#Region "CSVデータ取込処理　使用しないため、コメント化"
        '''' <summary>
        '''' 変更内容を非同期通信で取得します（AJax)
        '''' </summary>
        '''' <param name="CsvFile">登録するCSVファイル</param>
        '''' <returns>View表示内容</returns>
        'Public Function ImportCsvFile(ByVal CsvFile As HttpPostedFileBase) As ActionResult
        '    Me._lstErr = New List(Of DataInportResult)
        '    Dim errRow As DataInportResult = New DataInportResult
        '    Me._lstErr.Add(errRow)
        '    Dim sCsvPath As String = String.Empty   'CSVアップロードフルパス

        '    Dim sProcessContents As String          '操作ログ出力

        '    'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
        '    _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
        '    If IsNothing(_LoginInfo) Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.LoginSession
        '        '結果表示
        '        Return Json(Me._lstErr)
        '    End If

        '    Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
        '    Dim invId As Long
        '    Long.TryParse(keys(0), invId)
        '    Dim invLocL = keys(1)

        '    'メッセージ一覧取得
        '    Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
        '    _AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryInquiry, _LoginInfo)

        '    '読込むCSVファイルがあるかどうか
        '    If Not IsCsvFileError(CsvFile) Then
        '        Dim sErrGaiyou As String = String.Empty
        '        Select Case Me._lstErr(0).ResultCd
        '            Case DataInport.DataInportResultCd.NoSelect
        '                sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECT)
        '            Case DataInport.DataInportResultCd.Empty
        '                sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_EMPTY)
        '            Case DataInport.DataInportResultCd.NotCsv
        '                sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTCSV)
        '        End Select

        '        '操作ログ出力
        '        sProcessContents = Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE) & " " &
        '                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
        '                                             sErrGaiyou
        '        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE))
        '        Return Json(Me._lstErr)
        '    End If

        '    '◆CSVファイル出力先フォルダ存在チェック
        '    Dim sFullPath As String = Server.MapPath(CommonConst.CSVIMPORT_FOLDERNM)
        '    If Not Directory.Exists(sFullPath) Then
        '        Directory.CreateDirectory(sFullPath)
        '    End If

        '    'CSVファイルをアップロード
        '    sCsvPath = Path.Combine(sFullPath, CsvFile.FileName)
        '    Dim sCsvUrl As String = Path.Combine(sFullPath, CsvFile.FileName)
        '    CsvFile.SaveAs(sCsvPath)

        '    'CSVヘッダー取得
        '    Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_INVENTORY_INQUIRY)
        '    Dim stArrayData As String() = sHeader.Split(","c)

        '    'CSVデータ取得 & 項目数チェック
        '    Dim isKomokusuuErr As Boolean = False
        '    Dim tbl As DataTable = clsSV_CsvFile.ReadCsvFileToDataTable(sCsvPath, Encoding.UTF8, CChar(","), True, "InventoryInquiry", stArrayData.Length, isKomokusuuErr)
        '    '項目数エラーの場合
        '    If isKomokusuuErr Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
        '        '操作ログ出力
        '        sProcessContents = Me._Message.getMessage(CommonLabelCategory.TITLE) & " " &
        '                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
        '                                             Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " &
        '                                             Path.GetFileName(CsvFile.FileName)
        '        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE))

        '        Return Json(Me._lstErr)
        '    End If

        '    'エラーチェック
        '    If Not IsDataError(tbl, stArrayData) Then
        '        '操作ログ出力
        '        sProcessContents = Me._Message.getMessage(CommonLabelCategory.TITLE) & " " &
        '                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
        '                                             Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " &
        '                                             Path.GetFileName(CsvFile.FileName) & " " &
        '                                             Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
        '                                             Me._lstErr.Count.ToString
        '        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE))

        '        Return Json(Me._lstErr)
        '    End If

        '    '--登録処理開始
        '    Dim db As New SystemDBContext
        '    Dim insertModels As List(Of InventoryInquiryModel) = New List(Of InventoryInquiryModel)    '登録用モデル

        '    'トランザクションスタート
        '    Using tran = db.Database.BeginTransaction()
        '        Try

        '            '更新する
        '            For icnt As Integer = 0 To tbl.Rows.Count - 1

        '                Dim assetNo = tbl.Rows(icnt).Item(CsvInv.ColIdx.AssetNo).ToString.Trim.TrimStart("'"c)
        '                Dim invAssetList = db.InventoryInquiry.Where(Function(x) x.INV_ID = invId AndAlso x.INV_LOC_L_CD = invLocL AndAlso
        '                                                                 x.ASSET_NO = assetNo).ToList
        '                Dim invAsset = invAssetList(0)

        '                invAsset.OPE_STATUS = tbl.Rows(icnt).Item(CsvInv.ColIdx.OpeStatus).ToString.Trim.TrimStart("'"c)
        '                invAsset.DIVERSION_REMOVE = tbl.Rows(icnt).Item(CsvInv.ColIdx.DiversionRemove).ToString.Trim
        '                invAsset.REMOVE_DATE = tbl.Rows(icnt).Item(CsvInv.ColIdx.RemoveDate).ToString.Trim.TrimStart("'"c)
        '                invAsset.REMOVE_REASON = tbl.Rows(icnt).Item(CsvInv.ColIdx.RemoveReason).ToString.Trim.TrimStart("'"c)
        '                invAsset.REMOVE_CONTENT = tbl.Rows(icnt).Item(CsvInv.ColIdx.RemoveContent).ToString.Trim
        '                invAsset.CHANGE_LOC_M_CD = tbl.Rows(icnt).Item(CsvInv.ColIdx.ChangeLocMCd).ToString.Trim.TrimStart("'"c)
        '                invAsset.CHANGE_LOC_S_CD = tbl.Rows(icnt).Item(CsvInv.ColIdx.ChangeLocSCd).ToString.Trim.TrimStart("'"c)
        '                invAsset.CHANGE_EMPLOYEE = tbl.Rows(icnt).Item(CsvInv.ColIdx.ChangeEmployee).ToString.Trim.TrimStart("'"c)

        '                invAsset.REMARK1 = tbl.Rows(icnt).Item(CsvInv.ColIdx.Remark1).ToString.Trim
        '                invAsset.REMARK2 = tbl.Rows(icnt).Item(CsvInv.ColIdx.Remark2).ToString.Trim
        '                invAsset.REMARK3 = tbl.Rows(icnt).Item(CsvInv.ColIdx.Remark3).ToString.Trim
        '                invAsset.REMARK4 = tbl.Rows(icnt).Item(CsvInv.ColIdx.Remark4).ToString.Trim
        '                invAsset.REMARK5 = tbl.Rows(icnt).Item(CsvInv.ColIdx.Remark5).ToString.Trim
        '                invAsset.REMARK6 = tbl.Rows(icnt).Item(CsvInv.ColIdx.Remark6).ToString.Trim
        '                invAsset.REMARK7 = tbl.Rows(icnt).Item(CsvInv.ColIdx.Remark7).ToString.Trim
        '                invAsset.REMARK8 = tbl.Rows(icnt).Item(CsvInv.ColIdx.Remark8).ToString.Trim
        '                invAsset.REMARK9 = tbl.Rows(icnt).Item(CsvInv.ColIdx.Remark9).ToString.Trim
        '                invAsset.REMARK10 = tbl.Rows(icnt).Item(CsvInv.ColIdx.Remark10).ToString.Trim

        '                invAsset.UPD_DATETIME = CommonRfid.getNowDate
        '                invAsset.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
        '                invAsset.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
        '                invAsset.UPD_TASK = Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE)

        '            Next

        '            '登録処理を実行
        '            db.SaveChanges()

        '            tran.Commit()

        '            '操作ログ出力


        '            For icnt As Integer = 0 To tbl.Rows.Count - 1
        '                '操作ログ出力
        '                Dim assetNo = tbl.Rows(icnt).Item(CsvInv.ColIdx.AssetNo).ToString.Trim.TrimStart("'"c)
        '                Dim tblAsset As DataTable = ToolInvAsset.GetInvAsset(invId, invLocL, assetNo)
        '                sProcessContents = Me._Message.getMessage(CommonLabel.BUTTON_CSVIMPORT) & " " &
        '                                                Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & ": " & assetNo & "  " &
        '                                                Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
        '                                                CommonRfid.ToStringNvl(tblAsset.Rows(0)("ASSET_NAME").ToString).Trim
        '                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo,
        '                                               Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE))

        '            Next

        '        Catch ex As Exception
        '            tran.Rollback()

        '            logger.Error(ex.Message)
        '            'logger.Error(sbSQL)    'マスタ系はSQLでの登録ではないのではずしておく

        '            Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))

        '        End Try
        '    End Using
        '    Return Json(Me._lstErr)
        'End Function

        '''' <summary>
        '''' CSVファイルのエラーチェック
        '''' </summary>
        '''' <param name="file">チェック対象のファイル</param>
        '''' <returns>True:エラー無し　False:エラー有り</returns>
        'Public Function IsCsvFileError(file As HttpPostedFileBase) As Boolean
        '    Dim blRet As Boolean = False

        '    'ファイルがあるかどうか
        '    If IsNothing(file) Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NoSelect
        '        Return blRet
        '    End If

        '    'ファイルサイズ
        '    If file.ContentLength = 0 Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.Empty
        '        Return blRet
        '    End If

        '    '拡張子を検証
        '    Dim extention As String = Path.GetExtension(file.FileName).Replace(".", "")
        '    If String.IsNullOrEmpty(extention) OrElse extention.ToLower <> "csv" Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NotCsv
        '        Return blRet
        '    End If

        '    blRet = True
        '    Return blRet
        'End Function

        '''' <summary>
        '''' 読込んだCSVファイルデータのエラーチェック
        '''' </summary>
        '''' <param name="tbl">チェック対象のデータテーブル</param>
        '''' <param name="stArrayData">ヘッダの項目配列</param>
        '''' <returns>True:エラー無し　False:エラー有り</returns>
        'Public Function IsDataError(tbl As DataTable, stArrayData As String()) As Boolean
        '    Dim blRet As Boolean = False

        '    Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
        '    Dim invId As Long
        '    Long.TryParse(keys(0), invId)
        '    Dim invLocL = keys(1)

        '    '--項目数チェック
        '    If IsError(tbl) OrElse tbl.Columns.Count <> stArrayData.Length Then
        '        'エラー
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr
        '        Return blRet
        '    End If

        '    ''--マスタデータ取得
        '    Dim opeStatusItem = GetSettingMDropListData("Ope_Status_Item")
        '    Dim divOrRemoveItem = GetSettingMDropListData("Diversion_or_Remove", "DATA1", "DATA1")
        '    Dim removeReasonItem = GetSettingMDropListData("Remove_Reason_Item")

        '    Dim locLCd = invLocL        'LOC_Lは変わらない前提
        '    Dim locMList = ToolLocation.GetHubLocationFromDiv(LOC_DIV.LocationM, _LoginInfo, _AuthInfo)
        '    Dim locSList = ToolLocation.GetHubLocationFromDiv(LOC_DIV.LocationS, _LoginInfo, _AuthInfo)
        '    Dim invAssetTbl = ToolInvAsset.GetMyAsset(invId, invLocL)
        '    Dim assetNoList = New List(Of String)
        '    For Each row As DataRow In invAssetTbl.Rows
        '        assetNoList.Add(row.Field(Of String)("ASSET_NO"))
        '    Next

        '    Dim employeeList = ToolEmployee.GetEmployeeDataAll()

        '    '--データチェック
        '    blRet = True
        '    Dim lstSumiCd As List(Of String) = New List(Of String)
        '    For iRowCnt As Integer = 0 To tbl.Rows.Count - 1

        '        Dim divOrRemoveValue = tbl.Rows(iRowCnt).Item(CsvInv.ColIdx.DiversionRemove).ToString.Trim.TrimStart("'"c)
        '        Dim divOrRemoveRecord = Nothing
        '        If (Not String.IsNullOrEmpty(divOrRemoveValue) AndAlso divOrRemoveItem.Exists(Function(o) o.Cd = divOrRemoveValue)) Then
        '            divOrRemoveRecord = ToolSetting.GetSetting("Diversion_or_Remove", divOrRemoveValue, "**", "**")
        '        End If

        '        For iColCnt As Integer = 0 To stArrayData.Length - 1

        '            '新規行がなければ追加する
        '            If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
        '                Dim lstRow As DataInportResult = New DataInportResult
        '                Me._lstErr.Add(lstRow)
        '            End If

        '            Dim isErr = False
        '            '列によってチェック条件・設定項目値を分ける
        '            Dim sStr As String = tbl.Rows(iRowCnt).Item(iColCnt).ToString.Trim.TrimStart("'"c)
        '            Select Case iColCnt
        '                '資産番号
        '                Case CsvInv.ColIdx.AssetNo
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                    ElseIf Not assetNoList.Contains(sStr) Then
        '                        'T_INV_ASSETに登録されているコードか
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_NOT_INVENTORY_NOW)
        '                    End If

        '                    If Not (isErr) Then
        '                        For Each row As DataRow In invAssetTbl.Rows
        '                            If (row.Field(Of String)("ASSET_NO") = sStr) Then
        '                                Dim lockEmployee = row.Field(Of String)("LOCK_EMPLOYEE_CD")
        '                                If (Not String.IsNullOrWhiteSpace(lockEmployee) AndAlso lockEmployee <> _LoginInfo.EmployeeCd) Then
        '                                    isErr = True
        '                                    Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_LOCK_OTHER_USER_DISABLE)
        '                                End If
        '                                Exit For
        '                            End If
        '                        Next
        '                    End If

        '                    If (isErr) Then
        '                        blRet = False
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO)
        '                    End If
        '                    '稼働区分
        '                Case CsvInv.ColIdx.OpeStatus
        '                    If String.IsNullOrEmpty(sStr) Then
        '                        '空白チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                    ElseIf sStr.Length > 4 Then
        '                        '項目長チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_MAXLENGTH)
        '                    ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                        '半角英数字チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    ElseIf Not opeStatusItem.Exists(Function(o) o.Cd = sStr) Then
        '                        '設定Mに登録されているコードか
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                    End If
        '                    If (isErr) Then
        '                        blRet = False
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_OPE_STATUS)
        '                    End If

        '                    '転用or除却
        '                Case CsvInv.ColIdx.DiversionRemove
        '                    If sStr.Length = 0 Then
        '                        '空白の場合はチェック無し
        '                    ElseIf sStr.Length > 2 Then
        '                        '項目長チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_MAXLENGTH)
        '                    ElseIf Not divOrRemoveItem.Exists(Function(o) o.Cd = sStr) Then
        '                        '設定Mに登録されているコードか
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                    End If
        '                    If (isErr) Then
        '                        blRet = False
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_DIVERSION_OR_REMOVE)
        '                    End If

        '                    '除却時期(予定)
        '                Case CsvInv.ColIdx.RemoveDate
        '                    If (divOrRemoveRecord IsNot Nothing AndAlso divOrRemoveRecord.DATA2 = "Y") Then
        '                        '転用or除却の値がYの場合、必須チェック
        '                        If String.IsNullOrEmpty(sStr) Then
        '                            isErr = True
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        End If

        '                        Dim valid = IsValidYearMonthByDate(sStr)
        '                        If (Not valid) Then
        '                            isErr = True
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelInventorySummary.MSG_REMOVE_DATE_FORMAT)
        '                        End If

        '                    Else
        '                        '転用or除却の値がNの場合、ブランクチェック
        '                        If Not String.IsNullOrEmpty(sStr) Then
        '                            isErr = True
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_DONT_DIVERSION_OR_REMOVE_ERROR)
        '                        End If
        '                    End If

        '                    If sStr.Length > 30 Then
        '                        '項目長チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_MAXLENGTH)
        '                    End If
        '                    If (isErr) Then
        '                        blRet = False
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_REMOVE_DATE)
        '                    End If

        '                    '除却理由
        '                Case CsvInv.ColIdx.RemoveReason
        '                    If sStr.Length = 0 Then

        '                    ElseIf sStr.Length > 1 Then
        '                        '項目長チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_MAXLENGTH)
        '                    ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                        '半角英数字チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    ElseIf Not removeReasonItem.Exists(Function(o) o.Cd = sStr) Then
        '                        '設定Mに登録されているコードか
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                    End If
        '                    If (isErr) Then
        '                        blRet = False
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_REMOVE_REASON)
        '                    End If

        '                    '除却内容
        '                Case CsvInv.ColIdx.RemoveContent
        '                    Dim removeReasonValue = tbl.Rows(iRowCnt).Item(CsvInv.ColIdx.RemoveReason).ToString.Trim.TrimStart("'"c)
        '                    Dim removeReasonRecord = Nothing
        '                    If (Not String.IsNullOrEmpty(removeReasonValue) AndAlso removeReasonItem.Exists(Function(o) o.Cd = removeReasonValue)) Then
        '                        removeReasonRecord = ToolSetting.GetSetting("Remove_Reason_Item", removeReasonValue, "**", "**")
        '                    End If

        '                    If (removeReasonRecord IsNot Nothing AndAlso removeReasonRecord.DATA3 = "Y") Then
        '                        '除却理由の値がYの場合、必須チェック
        '                        If String.IsNullOrEmpty(sStr) Then
        '                            isErr = True
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATAEMPTY)
        '                        End If
        '                    Else
        '                        '除却理由の値がNの場合、ブランクチェック
        '                        If Not String.IsNullOrEmpty(sStr) Then
        '                            isErr = True
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_DONT_REMOVE_CONTENT_ERROR)
        '                        End If
        '                    End If

        '                    If sStr.Length > 30 Then
        '                        '項目長チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_MAXLENGTH)
        '                    End If
        '                    If (isErr) Then
        '                        blRet = False
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_REMOVE_CONTENT)
        '                    End If

        '                    '変更後課コード
        '                Case CsvInv.ColIdx.ChangeLocMCd

        '                    If sStr.Length = 0 Then
        '                        'ブランクの場合はスルー
        '                    ElseIf sStr.Length > 10 Then
        '                        '項目長チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_MAXLENGTH)
        '                    ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                        '半角英数字チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    Else
        '                        '所属拠点に存在していない場合はエラー
        '                        Dim LocRow As DataRow() = Nothing
        '                        LocRow = locMList.Select("LOC_L_CD = '" + locLCd + "' AND LOC_M_CD = '" + sStr + "'")
        '                        If LocRow Is Nothing OrElse LocRow.Length = 0 Then
        '                            isErr = True
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERRNO_EMPLOYEE_HUB_NOT_EXISTS)
        '                        End If
        '                    End If
        '                    If (isErr) Then
        '                        blRet = False
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CHANGE_LOC_M_CD)
        '                    End If
        '                    '変更後設置場所コード
        '                Case CsvInv.ColIdx.ChangeLocSCd
        '                    If sStr.Length = 0 Then

        '                    ElseIf sStr.Length > 10 Then
        '                        '項目長チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_MAXLENGTH)
        '                    ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                        '半角英数字チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    Else
        '                        '所属拠点に存在していない場合はエラー
        '                        Dim LocRow As DataRow() = Nothing
        '                        Dim locMCd = tbl.Rows(iRowCnt).Item(CsvInv.ColIdx.ChangeLocMCd).ToString.Trim.TrimStart("'"c)
        '                        LocRow = locSList.Select("LOC_L_CD = '" + locLCd + "' AND LOC_M_CD = '" + locMCd + "' AND LOC_S_CD = '" + sStr + "'")
        '                        If LocRow Is Nothing OrElse LocRow.Length = 0 Then
        '                            isErr = True
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonErrorMsg.MSG_ERRNO_NO_LOC_S)
        '                        End If
        '                    End If
        '                    If (isErr) Then
        '                        blRet = False
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CHANGE_LOC_S_CD)
        '                    End If
        '                    '変更後仕様社員番号
        '                Case CsvInv.ColIdx.ChangeEmployee
        '                    If sStr.Length = 0 Then

        '                    ElseIf sStr.Length > 15 Then
        '                        '項目長チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_MAXLENGTH)
        '                    ElseIf Not CommonRfid.IsHankaku(sStr) Then
        '                        '半角英数字チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_DATASEIYAKU)
        '                    Else
        '                        Dim employeeRow = employeeList.Select("EMPLOYEE_CD = '" + sStr + "'")
        '                        If employeeRow Is Nothing OrElse employeeRow.Length = 0 Then
        '                            isErr = True
        '                            Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXIST_DB)
        '                        End If
        '                    End If
        '                    If (isErr) Then
        '                        blRet = False
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_CHANGE_EMPLOYEE_CD)
        '                    End If
        '                Case CsvInv.ColIdx.Remark1,
        '                    CsvInv.ColIdx.Remark2,
        '                    CsvInv.ColIdx.Remark3,
        '                    CsvInv.ColIdx.Remark4,
        '                    CsvInv.ColIdx.Remark5,
        '                    CsvInv.ColIdx.Remark6,
        '                    CsvInv.ColIdx.Remark7,
        '                    CsvInv.ColIdx.Remark8,
        '                    CsvInv.ColIdx.Remark9,
        '                    CsvInv.ColIdx.Remark10

        '                    Dim i = iColCnt - CsvInv.ColIdx.Remark1 + 1
        '                    '備考1～10
        '                    If sStr.Length > 255 Then
        '                        '項目長チェック
        '                        isErr = True
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_MAXLENGTH)
        '                    End If
        '                    If (isErr) Then
        '                        blRet = False
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = iRowCnt + 2
        '                        Me._lstErr(Me._lstErr.Count - 1).ErrKoumoku = Me._Message.getMessage(CommonLabel.LABEL_REMARK) + i.ToString()
        '                    End If
        '            End Select
        '        Next
        '    Next


        '    'データ項目にエラーがあった場合
        '    If blRet = False Then
        '        Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG

        '        '最終行が空白行であれば削除する。
        '        If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx = 0 Then
        '            Dim lstRow As DataInportResult = New DataInportResult
        '            Me._lstErr.RemoveAt(Me._lstErr.Count - 1)
        '        End If
        '    End If

        '    Return blRet
        'End Function

#End Region
        '[CC431] ADD Start 20261008
#Region "データ取込処理"
        ''' <summary>
        ''' データ取込（Excel）を非同期通信で行います（AJax)
        ''' </summary>
        ''' <param name="ExcelFile">取り込むExcelファイル</param>
        ''' <returns>取込結果</returns>
        ''' <remarks>
        ''' 列の並びは M_CSVHEADER_ORDER と列定義（ExcelInv.ColDefs）で決まる（データ出力と同じ）。
        ''' 更新は、チェック済みのデータを一時テーブル（#INV_IMPORT）に一括登録してから、1 回の UPDATE で行う。
        ''' </remarks>
        Public Function ImportExcelFile(ByVal ExcelFile As HttpPostedFileBase) As ActionResult
            Me._lstErr = New List(Of DataInportResult)
            Me._lstErr.Add(New DataInportResult)

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.LoginSession
                '結果表示
                Return Json(Me._lstErr)
            End If

            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim sTitle As String = Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE)
            Dim sAction As String = Me._Message.getMessage(CommonLabel.BUTTON_DATA_IMPORT)
            Dim sProcessContents As String          '操作ログ出力

            '①読込むファイルの基本チェック
            If Not IsImportFileError(ExcelFile) Then
                Dim sErrGaiyou As String = String.Empty
                Select Case Me._lstErr(0).ResultCd
                    Case DataInport.DataInportResultCd.NoSelect
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_SELECT)
                    Case DataInport.DataInportResultCd.Empty
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_EMPTY)
                    Case DataInport.DataInportResultCd.NotExcel
                        sErrGaiyou = Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXCEL)
                End Select

                '操作ログ出力
                sProcessContents = sTitle & " " & sAction & " " & sErrGaiyou
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)
                Return Json(Me._lstErr)
            End If

            '②取込用一時フォルダが存在しなければ作成し、ファイルをアップロードする
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVIMPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If
            Dim sFileName As String = Path.GetFileName(ExcelFile.FileName)
            Dim sFilePath As String = Path.Combine(sFullPath, sFileName)
            ExcelFile.SaveAs(sFilePath)

            '③ファイル読込
            Dim ws As GrapeCity.Documents.Excel.IWorksheet = Nothing
            Try
                'ライセンスキー
                GrapeCity.Documents.Excel.Workbook.SetLicenseKey("株式会社シービジョン,496378668853486#A03NJgzNzYTO4IiOiQWSisnOiQkIsISP3cWe6QTZ95WWvEmMYdTStZ5MltGdv26dWlWazcmYBREamhXNyUEVRdGbOl6SuREarg4L6dVRwZnUz4EZFZzKzYmZv2Ee7gje9tWb5IzZ6x6arQGUVl5UiojITJCL7YzM8cTM8gDO0IicfJye#4Xfd5nI9g4NyIiOiMkIsIiMWBCdl9kLgwWZjhXRgI7bmBCduVWb5N6bEByQHJiOi8kI1tlOiQmcQJCLiEDNzIDNwAiNwITM9EDMyIiOiQncDJCLiM1gjf0gjjrgjP9gjz1gjfrgj1LpnrJvk/IvlrKomLiOiEmTDJCLiYDO4M5Ntg")
                Dim wb As GrapeCity.Documents.Excel.IWorkbook = New GrapeCity.Documents.Excel.Workbook
                Using fs As New FileStream(sFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    wb.Open(fs)
                End Using
                ws = wb.Worksheets(0)
            Catch ex As Exception
                'Excelファイルとして読めない場合（拡張子だけ xlsx のファイルなど）
                logger.Error("ImportExcelFile: " & ex.Message)
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NotExcel

                '操作ログ出力
                sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_NOTEXCEL) & " " & sFileName
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)
                Return Json(Me._lstErr)
            End Try

            '列の並び（M_CSVHEADER_ORDER の順。データ出力と同じ）
            Dim headerWordList As List(Of String) = Dao.ToolCSVHeader.GetCSVHeaderOrder(CommonCsvHedderFundtionId.FUNCID_INVENTORY_INQUIRY)
            Dim colIdxMap As New Dictionary(Of String, Integer)
            For colIndex As Integer = 0 To headerWordList.Count - 1
                If Not colIdxMap.ContainsKey(headerWordList(colIndex)) Then
                    colIdxMap.Add(headerWordList(colIndex), colIndex)
                End If
            Next

            '取込対象の列（資産番号はキー、取込対象列名が入っているもの）
            Dim importDefs As List(Of ExcelInv.ColDef) = ExcelInv.ColDefs.Where(Function(x) x.ImportColumn IsNot Nothing).ToList
            Dim missingWordCds As List(Of String) = importDefs.Select(Function(x) x.WordCd).
                                                    Concat({ExcelInv.WC_ASSET_NO}).
                                                    Where(Function(x) Not colIdxMap.ContainsKey(x)).ToList
            If missingWordCds.Count > 0 Then
                '列定義と M_CSVHEADER_ORDER が合っていない
                logger.Error("ImportExcelFile: M_CSVHEADER_ORDER に取込対象の文言コードがない " & String.Join(",", missingWordCds))
            End If

            '③項目数チェック
            Dim lastDataCol As Integer = ws.UsedRange.Column + ws.UsedRange.Columns.ColumnCount     'A列からの列数
            Dim lastDataRow As Integer = ws.UsedRange.Row + ws.UsedRange.Rows.RowCount              '最終行（Excelの行番号）DioDocs が「シートで使われている範囲」とみなす範囲の最終行
            If missingWordCds.Count > 0 OrElse lastDataCol <> headerWordList.Count Then
                '列定義と M_CSVHEADER_ORDER が合っていない、または項目数が違う場合、エラーを返す
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr

                '操作ログ出力
                sProcessContents = sTitle & " " & sAction & " " & Me._Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU) & " " & sFileName
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)
                Return Json(Me._lstErr)
            End If

            '最終行判定：使われている範囲の最終行から遡って資産番号が入っている行を最終行とする
            Dim assetNoColIdx As Integer = colIdxMap(ExcelInv.WC_ASSET_NO)
            Do While lastDataRow >= ExcelInv.IMPORT_DATA_START_ROW
                If GetImportCellValue(ws, lastDataRow, assetNoColIdx) <> String.Empty Then
                    Exit Do
                End If
                lastDataRow -= 1
            Loop

            '③④データのエラーチェック
            Dim importTbl As DataTable = Nothing
            If Not IsExcelDataError(ws, colIdxMap, importDefs, lastDataRow, invId, invLocL, importTbl) Then
                '操作ログ出力
                sProcessContents = sTitle & " " & sAction & " " &
                                   Me._Message.getMessage(CommonLabel.MSG_ERR_ELSE) & " " & sFileName & " " &
                                   Me._Message.getMessage(CommonLabel.LABEL_ERR_KENSUU) & ": " &
                                   Me._lstErr.Count.ToString
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)
                Return Json(Me._lstErr)
            End If

            '⑤棚卸資産Tの該当データを更新する（データ行がなければ更新しない）
            '[CC431] UPD Start 20261009 更新件数が取込件数と一致しない場合（チェック後に棚卸が完了したなど）は、取込エラーとして返す
            If importTbl.Rows.Count > 0 Then
                Dim updCount As Integer
                If Not UpdateInvAssetByImport(importTbl, importDefs, invId, invLocL, updCount) Then
                    '画面には、棚卸中でないことを示す文言を行番号・項目名なしで返す
                    Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                    Me._lstErr(0).ErrRowIdx = Nothing
                    Me._lstErr(0).ErrKoumoku = String.Empty
                    Me._lstErr(0).ErrGaiyou = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_NOT_INVENTORY_NOW)

                    '操作ログには、実際に起きたこと（件数の不一致）を出力する
                    sProcessContents = sTitle & " " & sAction & " " &
                                       Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_ERR_UPDATEDATA_COUNT).
                                           Replace("{0}", importTbl.Rows.Count.ToString).Replace("{1}", updCount.ToString) & " " & sFileName
                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)
                    Return Json(Me._lstErr)
                End If
            End If
            '[CC431] UPD End 20261009

            '⑦操作ログ出力（取込 1 回につき 1 件）
            sProcessContents = sTitle & " " & sAction & " " &
                               Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                               Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                               importTbl.Rows.Count.ToString & " " & sFileName
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, sTitle)

            Return Json(Me._lstErr)
        End Function

        ''' <summary>
        ''' データ取込（Excel）ファイルの基本チェック
        ''' </summary>
        ''' <param name="file">チェック対象のファイル</param>
        ''' <returns>True:エラー無し　False:エラー有り</returns>
        Private Function IsImportFileError(file As HttpPostedFileBase) As Boolean
            'ファイルがあるかどうか
            If IsNothing(file) Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NoSelect
                Return False
            End If

            'ファイルサイズ
            If file.ContentLength = 0 Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.Empty
                Return False
            End If

            '拡張子を検証（xlsx のみ）
            Dim extention As String = Path.GetExtension(file.FileName).Replace(".", "")
            If String.IsNullOrEmpty(extention) OrElse extention.ToLower <> "xlsx" Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.NotExcel
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' データ取込（Excel）のデータのエラーチェック
        ''' </summary>
        ''' <param name="ws">取込ファイルのシート</param>
        ''' <param name="colIdxMap">文言コード→列番号（0始まり）</param>
        ''' <param name="importDefs">取込対象の列定義</param>
        ''' <param name="lastDataRow">最終行（Excelの行番号）</param>
        ''' <param name="invId">棚卸ID</param>
        ''' <param name="invLocL">棚卸ロケ大区分コード</param>
        ''' <param name="importTbl">（戻り値）一時テーブルに登録するデータ（資産番号＋取込対象の列）</param>
        ''' <returns>True:エラー無し　False:エラー有り</returns>
        ''' <remarks>行ごとに DB を読まないよう、マスタ・棚卸資産T は最初にまとめて取得し、メモリで判定する</remarks>
        Private Function IsExcelDataError(ws As GrapeCity.Documents.Excel.IWorksheet,
                                          colIdxMap As Dictionary(Of String, Integer),
                                          importDefs As List(Of ExcelInv.ColDef),
                                          lastDataRow As Integer,
                                          invId As Long,
                                          invLocL As String,
                                          ByRef importTbl As DataTable) As Boolean
            Dim blRet As Boolean = True

            '--マスタデータ取得
            Dim opeStatusItem = GetSettingMDropListData("Ope_Status_Item")
            Dim divOrRemoveItem = GetSettingMDropListData("Diversion_or_Remove", "DATA1", "DATA1")
            Dim removeReasonItem = GetSettingMDropListData("Remove_Reason_Item")
            Dim divOrRemoveSettings = GetImportSettingMap("Diversion_or_Remove")
            Dim removeReasonSettings = GetImportSettingMap("Remove_Reason_Item")

            '画面の会社のロケーション中区分・小区分（権限は見ない）
            Dim locMSet As New HashSet(Of String)
            For Each row As DataRow In ToolLocation.GetLocationMData(invLocL).Rows
                locMSet.Add(row("LOC_CD").ToString)
            Next
            Dim locSSet As New HashSet(Of String)       '中区分コード＋タブ＋小区分コード
            For Each row As DataRow In ToolLocation.GetLocationSData(invLocL).Rows
                locSSet.Add(row("LOC_M_CD").ToString & vbTab & row("LOC_CD").ToString)
            Next

            Dim employeeSet As New HashSet(Of String)
            For Each row As DataRow In ToolEmployee.GetEmployeeDataAll().Rows
                employeeSet.Add(row("EMPLOYEE_CD").ToString)
            Next

            '当該棚卸ID・会社の棚卸資産T
            Dim invAssetMap As New Dictionary(Of String, DataRow)
            For Each row As DataRow In ToolInvAsset.GetMyAsset(invId, invLocL).Rows
                Dim key As String = row("ASSET_NO").ToString.Trim
                If Not invAssetMap.ContainsKey(key) Then
                    invAssetMap.Add(key, row)
                End If
            Next

            '個別にチェックする列（これ以外の取込対象は備考1～10）
            Dim specificWordCds As String() = {ExcelInv.WC_OPE_STATUS, ExcelInv.WC_DIVERSION_REMOVE, ExcelInv.WC_REMOVE_DATE,
                                               ExcelInv.WC_REMOVE_REASON, ExcelInv.WC_REMOVE_CONTENT, ExcelInv.WC_CHANGE_LOC_M_CD,
                                               ExcelInv.WC_CHANGE_LOC_S_CD, ExcelInv.WC_CHANGE_EMPLOYEE}

            '文言コード→列定義（必須・項目長のチェックに使う）
            Dim defMap As Dictionary(Of String, ExcelInv.ColDef) = importDefs.ToDictionary(Function(x) x.WordCd)

            '一時テーブルに登録するデータ
            importTbl = New DataTable
            importTbl.Columns.Add("ASSET_NO", GetType(String))
            For Each def As ExcelInv.ColDef In importDefs
                importTbl.Columns.Add(def.ImportColumn, GetType(String))
            Next

            Dim assetNoSet As New HashSet(Of String)    '重複チェック用

            '--データチェック
            For rowNo As Integer = ExcelInv.IMPORT_DATA_START_ROW To lastDataRow
                '行の値（文言コード→値）※資産番号と取込対象列の値のみ追加する
                Dim vals As New Dictionary(Of String, String)
                vals.Add(ExcelInv.WC_ASSET_NO, GetImportCellValue(ws, rowNo, colIdxMap(ExcelInv.WC_ASSET_NO)))
                For Each def As ExcelInv.ColDef In importDefs
                    vals(def.WordCd) = GetImportCellValue(ws, rowNo, colIdxMap(def.WordCd))
                Next

                '各列のチェックは、①空なら必須のチェックだけ ②形式（項目長・文字種）のエラーはすべて出す
                '③形式にエラーがなければ、マスタなどの個別チェック（最初の 1 件）を行う
                Dim errs As List(Of String)
                Dim chkDef As ExcelInv.ColDef

                '資産番号（取込対象の列ではないため、項目長などは列定義に持たない）
                Dim assetNo As String = vals(ExcelInv.WC_ASSET_NO)
                Dim invAssetRow As DataRow = Nothing
                invAssetMap.TryGetValue(assetNo, invAssetRow)
                '文字種6：半角英数字記号(#$*+-.:;=<>_)
                errs = GetImportFormatErrs(assetNo, True, 24, Function(x) CommonRfid.IsAssetNo(x))
                If errs.Count > 0 OrElse String.IsNullOrEmpty(assetNo) Then
                    '必須・形式のエラー
                ElseIf assetNoSet.Contains(assetNo) Then
                    'ファイル内で重複（2件目以降をエラー）
                    errs.Add(CommonLabel.MSG_ERR_DATAJUUFUKU)
                ElseIf invAssetRow Is Nothing Then
                    '当該棚卸の棚卸資産Tにない（拠点以外・棚卸中でない資産）
                    errs.Add(CommonLabelInventoryInquiry.MSG_NOT_INVENTORY_NOW)
                Else
                    'ログイン担当者以外がロックした資産
                    Dim lockEmployee As String = invAssetRow("LOCK_EMPLOYEE_CD").ToString
                    If Not String.IsNullOrWhiteSpace(lockEmployee) AndAlso lockEmployee <> _LoginInfo.EmployeeCd Then
                        errs.Add(CommonLabelInventoryInquiry.MSG_LOCK_OTHER_USER_DISABLE)
                    End If
                End If
                If Not String.IsNullOrEmpty(assetNo) Then
                    assetNoSet.Add(assetNo)
                End If
                If AddImportErrs(rowNo, ExcelInv.WC_ASSET_NO, errs) Then blRet = False

                '変更後稼働区分
                Dim opeStatus As String = vals(ExcelInv.WC_OPE_STATUS)
                chkDef = defMap(ExcelInv.WC_OPE_STATUS)
                errs = GetImportFormatErrs(opeStatus, chkDef.IsRequired, chkDef.MaxLength, Function(x) CommonRfid.IsHankaku(x))
                If errs.Count > 0 OrElse String.IsNullOrEmpty(opeStatus) Then
                    '必須・形式のエラー
                ElseIf Not opeStatusItem.Exists(Function(o) o.Cd = opeStatus) Then
                    '設定Mに登録されているコードか
                    errs.Add(CommonLabel.MSG_ERR_NOTEXIST_DB)
                End If
                If AddImportErrs(rowNo, ExcelInv.WC_OPE_STATUS, errs) Then blRet = False

                '転用or除却
                Dim divOrRemove As String = vals(ExcelInv.WC_DIVERSION_REMOVE)
                chkDef = defMap(ExcelInv.WC_DIVERSION_REMOVE)
                errs = GetImportFormatErrs(divOrRemove, chkDef.IsRequired, chkDef.MaxLength)
                If errs.Count > 0 OrElse String.IsNullOrEmpty(divOrRemove) Then
                    '必須・形式のエラー、または空白（チェック無し）
                ElseIf Not divOrRemoveItem.Exists(Function(o) o.Cd = divOrRemove) Then
                    '設定Mに登録されているコードか
                    errs.Add(CommonLabel.MSG_ERR_NOTEXIST_DB)
                End If
                If AddImportErrs(rowNo, ExcelInv.WC_DIVERSION_REMOVE, errs) Then blRet = False

                '転用or除却の設定M.DATA2がYか（除却）
                Dim divOrRemoveRecord As SettingInfo = Nothing
                If Not String.IsNullOrEmpty(divOrRemove) AndAlso divOrRemoveItem.Exists(Function(o) o.Cd = divOrRemove) Then
                    divOrRemoveSettings.TryGetValue(divOrRemove, divOrRemoveRecord)
                End If
                Dim isRemove As Boolean = (divOrRemoveRecord IsNot Nothing AndAlso divOrRemoveRecord.DATA2 = "Y")

                '除却時期(予定)
                Dim removeDate As String = vals(ExcelInv.WC_REMOVE_DATE)
                chkDef = defMap(ExcelInv.WC_REMOVE_DATE)
                errs = GetImportFormatErrs(removeDate, chkDef.IsRequired, chkDef.MaxLength)
                If errs.Count > 0 Then
                    '必須・形式のエラー
                ElseIf isRemove Then
                    'DATA2がYの場合は必須、YYYY/MM形式（日付型のセルも形式エラー）
                    If String.IsNullOrEmpty(removeDate) Then
                        errs.Add(CommonLabel.MSG_ERR_DATAEMPTY)
                    ElseIf Not IsValidYearMonthByDate(removeDate) Then
                        errs.Add(CommonLabelInventorySummary.MSG_REMOVE_DATE_FORMAT)
                    End If
                ElseIf Not String.IsNullOrEmpty(removeDate) Then
                    'DATA2がY以外の場合は空白
                    errs.Add(CommonLabelInventoryInquiry.MSG_DONT_DIVERSION_OR_REMOVE_ERROR)
                End If
                If AddImportErrs(rowNo, ExcelInv.WC_REMOVE_DATE, errs) Then blRet = False

                '除却理由
                Dim removeReason As String = vals(ExcelInv.WC_REMOVE_REASON)
                chkDef = defMap(ExcelInv.WC_REMOVE_REASON)
                errs = GetImportFormatErrs(removeReason, chkDef.IsRequired, chkDef.MaxLength, Function(x) CommonRfid.IsHankaku(x))
                If errs.Count > 0 OrElse String.IsNullOrEmpty(removeReason) Then
                    '必須・形式のエラー、または空白（チェック無し）
                ElseIf Not removeReasonItem.Exists(Function(o) o.Cd = removeReason) Then
                    '設定Mに登録されているコードか
                    errs.Add(CommonLabel.MSG_ERR_NOTEXIST_DB)
                ElseIf Not isRemove Then
                    '転用or除却のDATA2がY以外の場合は空白
                    errs.Add(CommonLabelInventoryInquiry.MSG_DONT_REMOVE_REASON_ERROR)
                End If
                If AddImportErrs(rowNo, ExcelInv.WC_REMOVE_REASON, errs) Then blRet = False

                '除却内容
                Dim removeContent As String = vals(ExcelInv.WC_REMOVE_CONTENT)
                Dim removeReasonRecord As SettingInfo = Nothing
                If Not String.IsNullOrEmpty(removeReason) AndAlso removeReasonItem.Exists(Function(o) o.Cd = removeReason) Then
                    removeReasonSettings.TryGetValue(removeReason, removeReasonRecord)
                End If
                chkDef = defMap(ExcelInv.WC_REMOVE_CONTENT)
                errs = GetImportFormatErrs(removeContent, chkDef.IsRequired, chkDef.MaxLength)
                If errs.Count > 0 Then
                    '必須・形式のエラー
                ElseIf removeReasonRecord IsNot Nothing AndAlso removeReasonRecord.DATA3 = "Y" AndAlso String.IsNullOrEmpty(removeContent) Then
                    '除却理由のDATA3がYの場合は必須
                    errs.Add(CommonLabel.MSG_ERR_DATAEMPTY)
                ElseIf String.IsNullOrEmpty(removeReason) AndAlso Not String.IsNullOrEmpty(removeContent) Then
                    '除却理由が空白なら除却内容も空白
                    errs.Add(CommonLabelInventoryInquiry.MSG_DONT_REMOVE_CONTENT_ERROR)
                End If
                If AddImportErrs(rowNo, ExcelInv.WC_REMOVE_CONTENT, errs) Then blRet = False

                '変更後課コード
                Dim changeLocM As String = vals(ExcelInv.WC_CHANGE_LOC_M_CD)
                chkDef = defMap(ExcelInv.WC_CHANGE_LOC_M_CD)
                errs = GetImportFormatErrs(changeLocM, chkDef.IsRequired, chkDef.MaxLength, Function(x) CommonRfid.IsHankaku(x))
                If errs.Count > 0 OrElse String.IsNullOrEmpty(changeLocM) Then
                    '必須・形式のエラー、または空白（チェック無し）
                ElseIf Not locMSet.Contains(changeLocM) Then
                    '画面の会社のロケーション中区分コードか（権限は見ない）
                    errs.Add(CommonLabelInventoryInquiry.MSG_LOC_M_ERROR)
                End If
                If AddImportErrs(rowNo, ExcelInv.WC_CHANGE_LOC_M_CD, errs) Then blRet = False

                '新変更後設置場所コード
                Dim changeLocS As String = vals(ExcelInv.WC_CHANGE_LOC_S_CD)
                chkDef = defMap(ExcelInv.WC_CHANGE_LOC_S_CD)
                errs = GetImportFormatErrs(changeLocS, chkDef.IsRequired, chkDef.MaxLength, Function(x) CommonRfid.IsHankaku(x))
                If errs.Count > 0 OrElse String.IsNullOrEmpty(changeLocS) Then
                    '必須・形式のエラー、または空白（チェック無し）
                Else
                    '画面の会社のロケーション小区分コードか（権限は見ない）
                    '変更後課コードが空白の場合は、棚卸資産T.ロケ中区分コードとの組み合わせで判定する
                    Dim locMCd As String = changeLocM
                    If String.IsNullOrEmpty(locMCd) AndAlso invAssetRow IsNot Nothing Then
                        locMCd = invAssetRow("LOC_M_CD").ToString
                    End If
                    If Not locSSet.Contains(locMCd & vbTab & changeLocS) Then
                        errs.Add(CommonLabelInventoryInquiry.MSG_LOC_S_ERROR)
                    End If
                End If
                If AddImportErrs(rowNo, ExcelInv.WC_CHANGE_LOC_S_CD, errs) Then blRet = False

                '変更後管理者社員番号
                Dim changeEmployee As String = vals(ExcelInv.WC_CHANGE_EMPLOYEE)
                chkDef = defMap(ExcelInv.WC_CHANGE_EMPLOYEE)
                errs = GetImportFormatErrs(changeEmployee, chkDef.IsRequired, chkDef.MaxLength, Function(x) CommonRfid.IsHankaku(x))
                If errs.Count > 0 OrElse String.IsNullOrEmpty(changeEmployee) Then
                    '必須・形式のエラー、または空白（チェック無し）
                ElseIf Not employeeSet.Contains(changeEmployee) Then
                    '社員Mに登録されているコードか
                    errs.Add(CommonLabel.MSG_ERR_NOTEXIST_DB)
                End If
                If AddImportErrs(rowNo, ExcelInv.WC_CHANGE_EMPLOYEE, errs) Then blRet = False

                '備考1～10
                For Each def As ExcelInv.ColDef In importDefs.Where(Function(x) Not specificWordCds.Contains(x.WordCd))
                    errs = GetImportFormatErrs(vals(def.WordCd), def.IsRequired, def.MaxLength)
                    If AddImportErrs(rowNo, def.WordCd, errs) Then blRet = False
                Next

                '一時テーブルに登録するデータ
                Dim dr As DataRow = importTbl.NewRow()
                dr("ASSET_NO") = assetNo
                For Each def As ExcelInv.ColDef In importDefs
                    dr(def.ImportColumn) = vals(def.WordCd)
                Next
                importTbl.Rows.Add(dr)
            Next

            'データ項目にエラーがあった場合
            If Not blRet Then
                Me._lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
            End If

            Return blRet
        End Function

        ''' <summary>
        ''' データ取込の必須・形式（項目長・文字種）のチェック
        ''' </summary>
        ''' <param name="value">セルの値</param>
        ''' <param name="isRequired">必須か（列定義の IsRequired）</param>
        ''' <param name="maxLength">項目長（列定義の MaxLength）。0 ならチェックしない</param>
        ''' <param name="isValidChar">文字種のチェック。省略時はチェックしない</param>
        ''' <returns>エラー概要の文言コードの一覧。空なら必須のエラーだけ、空でなければ形式のエラーをすべて返す</returns>
        Private Function GetImportFormatErrs(value As String, isRequired As Boolean, maxLength As Integer,
                                             Optional isValidChar As Func(Of String, Boolean) = Nothing) As List(Of String)
            Dim errs As New List(Of String)

            '空の場合は必須のチェックのみ
            If String.IsNullOrEmpty(value) Then
                If isRequired Then
                    errs.Add(CommonLabel.MSG_ERR_DATAEMPTY)
                End If
                Return errs
            End If

            '項目長
            If maxLength > 0 AndAlso value.Length > maxLength Then
                errs.Add(CommonLabel.MSG_ERR_MAXLENGTH)
            End If

            '文字種
            If isValidChar IsNot Nothing AndAlso Not isValidChar(value) Then
                errs.Add(CommonLabel.MSG_ERR_DATASEIYAKU)
            End If

            Return errs
        End Function

        ''' <summary>
        ''' データ取込のエラーを一覧に追加する（1 つのセルの複数のエラー）
        ''' </summary>
        ''' <param name="rowNo">行（Excelの行番号）</param>
        ''' <param name="wordCd">データ項目（列の文言コード）</param>
        ''' <param name="errCds">エラー概要の文言コードの一覧</param>
        ''' <returns>True:エラーを追加した　False:エラーなし</returns>
        Private Function AddImportErrs(rowNo As Integer, wordCd As String, errCds As List(Of String)) As Boolean
            For Each errCd As String In errCds
                AddImportErr(rowNo, wordCd, errCd)
            Next
            Return errCds.Count > 0
        End Function

        ''' <summary>
        ''' データ取込のエラーを一覧に追加する
        ''' </summary>
        ''' <param name="rowNo">行（Excelの行番号）</param>
        ''' <param name="wordCd">データ項目（列の文言コード）</param>
        ''' <param name="errCd">エラー概要の文言コード。Nothing ならエラーなし</param>
        ''' <returns>True:エラーを追加した　False:エラーなし</returns>
        Private Function AddImportErr(rowNo As Integer, wordCd As String, errCd As String) As Boolean
            If errCd Is Nothing Then
                Return False
            End If

            '先頭行は結果コードと 1 件目のエラーを持つ。2 件目以降は行を追加する
            If Me._lstErr(Me._lstErr.Count - 1).ErrRowIdx > 0 Then
                Me._lstErr.Add(New DataInportResult)
            End If
            Dim errRow As DataInportResult = Me._lstErr(Me._lstErr.Count - 1)
            errRow.ErrRowIdx = rowNo
            errRow.ErrKoumoku = Me._Message.getMessage(wordCd)
            errRow.ErrGaiyou = Me._Message.getMessage(errCd)
            Return True
        End Function

        ''' <summary>
        ''' データ取込のセルの値を文字列で取得する
        ''' </summary>
        ''' <param name="ws">シート</param>
        ''' <param name="rowNo">行（Excelの行番号）</param>
        ''' <param name="colIdx">列番号（0始まり）</param>
        ''' <returns>前後の空白を除いた値。空のセルは空文字</returns>
        Private Function GetImportCellValue(ws As GrapeCity.Documents.Excel.IWorksheet, rowNo As Integer, colIdx As Integer) As String
            Dim cv As Object = ws.Cells(rowNo - 1, colIdx).Value
            If cv Is Nothing Then
                Return String.Empty
            End If
            Return cv.ToString.Trim
        End Function

        ''' <summary>
        ''' 設定Mのコード（KEY1）→設定情報の対応表を取得する（KEY2・KEY3 が "**" の行）
        ''' </summary>
        ''' <param name="keyDiv">キー区分</param>
        ''' <returns>KEY1→設定情報</returns>
        Private Function GetImportSettingMap(keyDiv As String) As Dictionary(Of String, SettingInfo)
            Dim ret As New Dictionary(Of String, SettingInfo)
            Dim settingList As List(Of SettingInfo) = ToolSetting.GetSettingList(keyDiv)
            If settingList Is Nothing Then
                Return ret
            End If
            For Each setting As SettingInfo In settingList.Where(Function(x) x.Key2 = "**" AndAlso x.Key3 = "**")
                If Not ret.ContainsKey(setting.Key1) Then
                    ret.Add(setting.Key1, setting)
                End If
            Next
            Return ret
        End Function

        ''' <summary>
        ''' データ取込の内容で棚卸資産Tを更新する
        ''' </summary>
        ''' <param name="importTbl">取込データ（資産番号＋取込対象の列）</param>
        ''' <param name="importDefs">取込対象の列定義</param>
        ''' <param name="invId">棚卸ID</param>
        ''' <param name="invLocL">棚卸ロケ大区分コード</param>
        ''' <param name="updCount">更新件数（件数が合わない場合に、呼び出し元で操作ログに出力するため返す）</param>
        ''' <returns>True:更新した　False:更新件数が取込件数と一致せず、全件取り消した</returns>
        ''' <remarks>
        ''' 一時テーブル（#INV_IMPORT。接続を閉じると自動で削除される）に SqlBulkCopy で一括登録し、
        ''' 棚卸資産Tと結合して 1 回の UPDATE で更新する。棚卸実施端末（PHYINV_TOOL_DIV）は更新しない。
        ''' </remarks>
        '[CC431] UPD Start 20261009 件数が合わない場合は例外にせず、False を返す（呼び出し元で取込エラーとして表示する）
        Private Function UpdateInvAssetByImport(importTbl As DataTable, importDefs As List(Of ExcelInv.ColDef), invId As Long, invLocL As String,
                                                ByRef updCount As Integer) As Boolean
            '[CC431] UPD End 20261009
            '列名は列定義の固定値（入力値ではない）
            Dim importColumns As List(Of String) = importDefs.Select(Function(x) x.ImportColumn).ToList
            Dim sbSQL As New StringBuilder

            Using db As New SystemDBContext
                Dim conn As SqlConnection = CType(db.Database.Connection, SqlConnection)
                If conn.State = ConnectionState.Closed Then conn.Open()

                Using tran As SqlTransaction = conn.BeginTransaction()
                    Try
                        '一時テーブルを作成（列の型・照合順序は棚卸資産Tと同じ）
                        sbSQL.AppendLine("SELECT TOP 0 ASSET_NO, " & String.Join(", ", importColumns))
                        sbSQL.AppendLine("INTO #INV_IMPORT")
                        sbSQL.AppendLine("FROM T_INV_ASSET")
                        Using cmd As New SqlCommand(sbSQL.ToString, conn, tran)
                            cmd.CommandTimeout = 600
                            cmd.ExecuteNonQuery()
                        End Using

                        '取込データを一括登録
                        Using bulk As New SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran)
                            bulk.DestinationTableName = "#INV_IMPORT"
                            bulk.BulkCopyTimeout = 600
                            For Each col As DataColumn In importTbl.Columns
                                bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName)
                            Next
                            bulk.WriteToServer(importTbl)
                        End Using

                        '棚卸資産Tを更新
                        sbSQL.Clear()
                        sbSQL.AppendLine("UPDATE T")
                        sbSQL.AppendLine("SET")
                        For Each colName As String In importColumns
                            sbSQL.AppendLine($"     T.{colName} = W.{colName}")
                            sbSQL.Append("    ,")
                        Next
                        sbSQL.AppendLine("T.ENT_EMPLOYEE_CD = @ENT_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,T.UPD_DATETIME = @UPD_DATETIME")
                        sbSQL.AppendLine("    ,T.UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                        sbSQL.AppendLine("    ,T.UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                        sbSQL.AppendLine("    ,T.UPD_TASK = @UPD_TASK")
                        sbSQL.AppendLine("FROM T_INV_ASSET T")
                        sbSQL.AppendLine("INNER JOIN #INV_IMPORT W")
                        sbSQL.AppendLine("    ON T.ASSET_NO = W.ASSET_NO")
                        sbSQL.AppendLine("WHERE T.INV_ID = @INV_ID")
                        sbSQL.AppendLine("  AND T.INV_LOC_L_CD = @INV_LOC_L_CD")

                        '[CC431] DEL 20261009 updCount の宣言（呼び出し元へ返すため引数にした）
                        Using cmd As New SqlCommand(sbSQL.ToString, conn, tran)
                            cmd.CommandTimeout = 600
                            cmd.Parameters.Add(New SqlParameter("@ENT_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            cmd.Parameters.Add(New SqlParameter("@UPD_DATETIME", CDate(CommonRfid.getNowDate)))
                            cmd.Parameters.Add(New SqlParameter("@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                            cmd.Parameters.Add(New SqlParameter("@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                            cmd.Parameters.Add(New SqlParameter("@UPD_TASK", Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE)))
                            cmd.Parameters.Add(New SqlParameter("@INV_ID", invId))
                            cmd.Parameters.Add(New SqlParameter("@INV_LOC_L_CD", invLocL))
                            updCount = cmd.ExecuteNonQuery()
                        End Using

                        'チェック後に棚卸資産Tの行がなくなった場合（棚卸確定など）は、全件取り消す
                        '[CC431] UPD Start 20261009 例外にせず、ロールバックして False を返す
                        If updCount <> importTbl.Rows.Count Then
                            tran.Rollback()
                            logger.Error($"UpdateInvAssetByImport: 更新件数が取込件数と一致しない（取込:{importTbl.Rows.Count} 更新:{updCount}）")
                            Return False
                        End If

                        tran.Commit()
                        Return True
                        '[CC431] UPD End 20261009

                    Catch ex As Exception
                        tran.Rollback()

                        logger.Error("UpdateInvAssetByImport: " & ex.Message)
                        logger.Error(sbSQL)

                        Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                    End Try
                End Using
            End Using
        End Function
#End Region
        '[CC431] ADD End 20261008

#Region "CSV出力処理　使用しないため、コメント化"
        '''' <summary>
        '''' CSV出力処理
        ''''         属性から外した→<ValidateAntiForgeryToken()>
        '''' </summary>
        '''' <remarks>
        '''' 使用しないように変更のため、コメント化
        '''' M_CSVHEADER_ORDERが出力順とあわなくてヘッダーとずれるため、
        '''' コメントを解除して使用する場合には他の方法でヘッダーを作成する必要あり。
        '''' </remarks>
        '<HttpPost()>
        '<ValidateInput(False)>
        'Public Function CsvExport(ByVal ViewModel As InventoryInquiryViewModel) As ActionResult
        '    'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
        '    _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
        '    If IsNothing(_LoginInfo) Then
        '        '結果表示
        '        Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        '    End If

        '    _ViewModel = ViewModel

        '    Dim InventoryInquiryData As List(Of InventoryInquiryViewModel)

        '    '実棚照会取得
        '    InventoryInquiryData = GetInventoryInquiryData()

        '    'CSV出力クラス作成
        '    Dim SVClsCSVExp As New clsSV_CsvExport

        '    '文字コードセット
        '    SVClsCSVExp.p_encoding = System.Text.Encoding.UTF8

        '    '出力ファイル名取得
        '    Dim Setting As MessageRfid      'メッセージ兼設定リスト
        '    Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
        '    Dim CsvHedderNm As SettingInfo = Setting.getSettingInfo(CommonSettingKey.InventoryInquiry.KeyDiv, CommonSettingKey.InventoryInquiry.Key1, CommonSettingKey.InventoryInquiry.Key2, CommonSettingKey.InventoryInquiry.Key3, True)
        '    Dim sFileNm As String = CsvHedderNm.DATA1

        '    '◆CSVファイル出力先フォルダ存在チェック
        '    Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
        '    If Not Directory.Exists(sFullPath) Then
        '        Directory.CreateDirectory(sFullPath)
        '    End If

        '    '◆CSVファイル出力先設定
        '    Dim sUrlFileName As String = Path.GetRandomFileName()
        '    Dim sDspFileName As String = String.Format(sFileNm + "_{0}.csv", Now.ToString("yyyyMMdd"))
        '    SVClsCSVExp.SVFncGetSaveFileName(sFullPath, sUrlFileName)

        '    '◆CSV出力用リスト作成
        '    'メッセージ一覧取得
        '    Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

        '    'CSVヘッダー取得
        '    'Dim sHeader As String = Me._Message.getMessage(CommonLabelCsvHedder.LABEL_INVENTORY_INQUIRY)
        '    Dim sHeader As String = GetCSVHeader(CommonCsvHedderFundtionId.FUNCID_INVENTORY_INQUIRY)
        '    Dim singleQuotation As String = "'"

        '    'ヘッダ行挿入
        '    SVClsCSVExp.p_csvDataList.Add(SVClsCSVExp.SVFncCSVAddDquotHed(sHeader))

        '    'データ行挿入
        '    Dim col As String = ""
        '    For Each row As InventoryInquiryViewModel In InventoryInquiryData
        '        col = ""
        '        col += SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_STATUS)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOCK_EMPLOYEE_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOCK_EMPLOYEE_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_STATUS)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_L_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_L_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_M_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_M_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.LOC_S_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.LOC_S_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.PHYINV_LOC_L_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_LOC_L_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.PHYINV_LOC_M_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_LOC_M_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.PHYINV_LOC_S_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_LOC_S_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CAT_L_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CAT_L_NAME)

        '        Dim tangibleValue = If(row.IS_TANGIBLE = "Y", Me._Message.getMessage(CommonLabel.VALUE_TANGIBLE_ASSET), Me._Message.getMessage(CommonLabel.VALUE_INTANGIBLE_ASSET))
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(tangibleValue)

        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.ASSET_NO)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.SUB_ASSET_NO)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.ASSET_NAME)

        '        Dim tsStr = If(row.PHYINV_TS IsNot Nothing, row.PHYINV_TS.ToString(), "")
        '        If row.PHYINV_TS IsNot Nothing Then
        '            Dim s As String = row.PHYINV_TS.ToString()
        '            If s.Length = 14 Then
        '                tsStr = $"{s.Substring(0, 4)}/{s.Substring(4, 2)}/{s.Substring(6, 2)} {s.Substring(8, 2)}:{s.Substring(10, 2)}:{s.Substring(12, 2)}"
        '            End If
        '        End If

        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(tsStr)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.PHYINV_EMPLOYEE_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.PHYINV_EMPLOYEE_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM6_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM1)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM2)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.FREE_ITEM3)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM4)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM5)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM7)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM8)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM9)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM10)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM11)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM12)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM13)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM14)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM15)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM16)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM17)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM18)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM19)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM20)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM21)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM22)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM23)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM24)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM25)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM26)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM27)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM28)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM29)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM30)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM31)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM32)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM33)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM34)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM35)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM36)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM37)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM38)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM39)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM40)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM41)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM42)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM43)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM44)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM45)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM46)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM47)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM48)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM49)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM50)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM51)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM52)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM53)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM54)

        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM55)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM56)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM57)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM58)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM59)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.FREE_ITEM60)

        '        Select Case row.TAG_LINK_STATUS
        '            Case Byte.Parse(TagLinkStatus.Mi)
        '                col += "," & SVClsCSVExp.SVFncCSVAddDquot(Me._Message.getMessage(CommonLabel.VALUE_MI).ToString())
        '            Case Byte.Parse(TagLinkStatus.Sumi)
        '                col += "," & SVClsCSVExp.SVFncCSVAddDquot(Me._Message.getMessage(CommonLabel.VALUE_SUMI).ToString())
        '            Case Else
        '                col += "," & SVClsCSVExp.SVFncCSVAddDquot(String.Empty)
        '        End Select

        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.OPE_STATUS)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.OPE_STATUS_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.DIVERSION_REMOVE)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.REMOVE_DATE)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.REMOVE_REASON)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMOVE_REASON_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMOVE_CONTENT)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CHANGE_LOC_M_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHANGE_LOC_M_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CHANGE_LOC_S_CD)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHANGE_LOC_S_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(singleQuotation + row.CHANGE_EMPLOYEE)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.CHANGE_EMPLOYEE_NAME)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK1)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK2)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK3)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK4)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK5)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK6)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK7)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK8)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK9)
        '        col += "," & SVClsCSVExp.SVFncCSVAddDquot(row.REMARK10)


        '        SVClsCSVExp.p_csvDataList.Add(col)
        '    Next

        '    'CSVファイル出力
        '    If SVClsCSVExp.SVFncCSVFileOutPut(False) Then

        '        'ファイルをダウンロードさせる
        '        Call FileDownload(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)
        '        'Return File(sDspFileName, "text/comma-separated-values", sFullPath + "\" + sUrlFileName)

        '        'ダウンロードさせた後、ファイルを削除。
        '        System.IO.File.Delete(sFullPath + "\" + sUrlFileName)
        '        'Response.Redirect("index")

        '        ''10025,INF,,,%1が完了しました。,OKOnly,1
        '        'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 10025, "CSVファイルの出力")
        '    Else
        '        ''30001,CRT,,,%1に失敗しました。,OKOnly,1
        '        'Call clsSV_MSG.SVFncPopUpMsg(FORM_NAME, 30001, "CSVファイルの出力")
        '    End If

        '    '操作ログ出力
        '    Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE) & " " &
        '                                             Me._Message.getMessage(CommonLabel.BUTTON_CSVEXPORT) & " " &
        '                                             Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
        '                                             Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
        '                                             InventoryInquiryData.Count.ToString
        '    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE))

        '    '結果表示
        '    Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        'End Function
#End Region

        '[CC431] ADD Start 20261005
#Region "データ出力処理"
        ''' <summary>
        ''' データ出力処理（Excel）
        ''' </summary>
        ''' <remarks>出力する列と並びは M_CSVHEADER_ORDER と列定義（ExcelInv.ColDefs）で決まる</remarks>
        <HttpPost()>
        <ValidateInput(False)>
        Public Function DataExport(ByVal ViewModel As InventoryInquiryViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            _ViewModel = ViewModel

            Dim InventoryInquiryData As List(Of InventoryInquiryViewModel)

            '実棚照会取得（検索条件の並び順を反映）
            InventoryInquiryData = GetInventoryInquiryData()

            '出力ファイル名取得
            Dim Setting As MessageRfid      'メッセージ兼設定リスト
            Setting = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim FileNmSetting As SettingInfo = Setting.getSettingInfo(CommonSettingKey.InventoryInquiry.KeyDiv, CommonSettingKey.InventoryInquiry.Key1, CommonSettingKey.InventoryInquiry.Key2, CommonSettingKey.InventoryInquiry.Key3, True)
            Dim sFileNm As String = FileNmSetting.DATA1

            '◆出力先フォルダ存在チェック
            Dim sFullPath As String = Server.MapPath(CommonConst.CSVEXPORT_FOLDERNM)
            If Not Directory.Exists(sFullPath) Then
                Directory.CreateDirectory(sFullPath)
            End If

            '◆出力ファイル名設定（一時ファイル名：ランダム文字列＋_yyyyMMdd.xlsx）
            Dim sYmd As String = Now.ToString("yyyyMMdd")
            Dim sUrlFileName As String = String.Format("{0}_{1}.xlsx", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()), sYmd)
            Dim sDspFileName As String = String.Format("{0}_{1}.xlsx", sFileNm, sYmd)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            '出力列（M_CSVHEADER_ORDER の並び順）
            Dim headerWordList As List(Of String) = Dao.ToolCSVHeader.GetCSVHeaderOrder(CommonCsvHedderFundtionId.FUNCID_INVENTORY_INQUIRY)
            Dim colDefList As New List(Of ExcelInv.ColDef)
            For Each wordCd As String In headerWordList
                Dim colDef As ExcelInv.ColDef = ExcelInv.ColDefs.FirstOrDefault(Function(x) x.WordCd = wordCd)
                If colDef Is Nothing Then
                    '列定義がない場合は、ヘッダのみ出力し値は空にする
                    logger.Warn("DataExport: 列定義がない文言コード " & wordCd)
                End If
                colDefList.Add(colDef)
            Next

            'ライセンスキー
            GrapeCity.Documents.Excel.Workbook.SetLicenseKey("株式会社シービジョン,496378668853486#A03NJgzNzYTO4IiOiQWSisnOiQkIsISP3cWe6QTZ95WWvEmMYdTStZ5MltGdv26dWlWazcmYBREamhXNyUEVRdGbOl6SuREarg4L6dVRwZnUz4EZFZzKzYmZv2Ee7gje9tWb5IzZ6x6arQGUVl5UiojITJCL7YzM8cTM8gDO0IicfJye#4Xfd5nI9g4NyIiOiMkIsIiMWBCdl9kLgwWZjhXRgI7bmBCduVWb5N6bEByQHJiOi8kI1tlOiQmcQJCLiEDNzIDNwAiNwITM9EDMyIiOiQncDJCLiM1gjf0gjjrgjP9gjz1gjfrgj1LpnrJvk/IvlrKomLiOiEmTDJCLiYDO4M5Ntg")
            Dim WB As GrapeCity.Documents.Excel.IWorkbook = New GrapeCity.Documents.Excel.Workbook
            Dim WS As GrapeCity.Documents.Excel.IWorksheet = WB.Worksheets(0)

            For colIndex As Integer = 0 To colDefList.Count - 1
                '文字列書式の列は、値の設定前に列全体の書式を設定する
                If colDefList(colIndex) IsNot Nothing AndAlso colDefList(colIndex).IsText Then
                    WS.Columns(colIndex).NumberFormat = "@"
                End If

                'ヘッダ行（1行目）
                WS.Cells(0, colIndex).Value = Me._Message.getMessage(headerWordList(colIndex))
            Next

            'データ行（2行目から）
            Dim rowIndex As Integer = 1
            For Each row As InventoryInquiryViewModel In InventoryInquiryData
                For colIndex As Integer = 0 To colDefList.Count - 1
                    If colDefList(colIndex) IsNot Nothing Then
                        WS.Cells(rowIndex, colIndex).Value = colDefList(colIndex).GetValue(row)
                    End If
                Next
                rowIndex += 1
            Next

            '.xlsxドキュメントをエクスポートします
            WB.Save(sFullPath + "\" + sUrlFileName, GrapeCity.Documents.Excel.SaveFileFormat.Xlsx)

            'ファイルをダウンロードさせる
            Call FileDownload(sDspFileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFullPath + "\" + sUrlFileName)

            'ダウンロードさせた後、ファイルを削除。
            System.IO.File.Delete(sFullPath + "\" + sUrlFileName)

            '操作ログ出力
            Dim sProcessContents As String = Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE) & " " &
                                                     Me._Message.getMessage(CommonLabel.BUTTON_DATA_EXPORT) & " " &
                                                     Me._Message.getMessage(CommonLabel.MSG_INPORTOK) & " " &
                                                     Me._Message.getMessage(CommonLabel.LABEL_KENSUU) & ": " &
                                                     InventoryInquiryData.Count.ToString
            Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo, Me._Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE))

            '結果表示
            Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
        End Function
#End Region
        '[CC431] ADD End 20261005

#Region "CSV出力用のデータを取得"
        ''' <summary>
        ''' CSV出力用のデータを取得します
        ''' </summary>
        Private Function GetInventoryInquiryData() As List(Of InventoryInquiryViewModel)
            Dim lstInventoryInquiry As List(Of InventoryInquiryViewModel)
            Dim db As SystemDBContext = New SystemDBContext

            'SQLを実行する
            'lstInventoryInquiry = db.InventoryInquiryView.SqlQuery(CreateSQL()).ToList

            Dim query As DbSqlQuery(Of InventoryInquiryViewModel) = db.InventoryInquiryView.SqlQuery(CreateSQL(), _paramSqlList.ToArray)
            lstInventoryInquiry = query.ToList

            'データを返却
            Return lstInventoryInquiry
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

            'ダウンロード完了検知用にクッキーを設定
            Response.Cookies.Add(New HttpCookie("fileDownloadToken", "done") With {.Path = "/"})

            'HTTPヘッダー情報・MIMEタイプ設定
            Response.AddHeader("Content-Disposition", String.Format("attachment;filename={0}", dlFileName))
            Response.ContentType = MimeType

            'ファイルを書き出し
            Response.WriteFile(sFileFullPath)

            Response.Flush()
            Response.End()
        End Sub
#End Region

#Region "QRコードデータを取得する"

        ''' <summary>
        ''' QRコードデータを取得する（AJax)
        '''   参考　https://blog.ch3cooh.jp/entry/20140722/1405990800
        ''' </summary>
        ''' <param name="sAssetNo">資産番号</param>
        Public Function GetQRData(ByVal sAssetNo As String) As String
            'QRコードを作成する

            'Dim sAssetId As String = Dao.ToolAsset.GetAssetId(sAssetNo)
            If String.IsNullOrEmpty(sAssetNo) Then
                Return String.Empty
            End If

            'QRコードを作成する
            Dim sQrFileName As String = Now.ToString("yyyyMMdd_") & Path.GetRandomFileName() & ".png"
            Dim sCallPath As String = System.IO.Path.GetFullPath(HttpContext.Server.MapPath(Misc.CommonConst.QRIMAGE_FOLDERNM) & "\\" & sQrFileName)
            Dim ret = Dao.MakeQr.GetQRData(sAssetNo, sCallPath)
            If ret <> 0 Then
                'TODO エラーメッセージを表示する
                Dim errmsg As String = Dao.MakeQr.GetQRMakerMsg(ret)
                Return String.Empty
            End If

            Return sQrFileName


        End Function
#End Region


#Region "アセットのロック/アンロックする"

        ''' <summary>
        ''' アセットをロックする
        ''' </summary>
        Public Function Lock(ViewModel As InventoryInquiryViewModel) As ActionResult

            Dim ret = New ApiResponse With {
                .ok = False
            }

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            Dim isInventory = ToolInventory.IsIventory(invId, invLocL, lstSelectKeys)
            If (isInventory) Then
                ret.err = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_NOT_INVENTORY_NOW)
                Return Json(ret)
            End If

            Dim lockUsers = ToolInvAsset.GetLockUsers(invId, invLocL, lstSelectKeys)
            If (lockUsers.Count = 0 OrElse (lockUsers.Count = 1 AndAlso lockUsers(0) = _LoginInfo.EmployeeCd)) Then
                '自身がロックもしくはロックされていないので、まったく問題ない
            Else
                '自分以外の誰かがロックしている。
                ret.err = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_LOCK_OTHER_USER_DISABLE)
                Return Json(ret)
            End If

            Dim sTaskNm As String = "InventoryInquiryController.Lock"
            Dim result = SetLockUnLock(lstSelectKeys, True, sTaskNm)
            If (result) Then
                ret.ok = True
                Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY) = ViewModel
            Else
                ret.err = Me._Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)
            End If

            Return Json(ret)
        End Function

        Public Function UnlockCheck(ViewModel As InventoryInquiryViewModel) As ActionResult

            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            Dim ret = New ApiResponse With {
                .ok = False
            }

            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            Dim lockUsers = ToolInvAsset.GetLockUsers(invId, invLocL, lstSelectKeys)
            If (lockUsers.Count = 0 OrElse (lockUsers.Count = 1 AndAlso lockUsers(0) = _LoginInfo.EmployeeCd)) Then
                '自身がロックもしくはロックされていないので、まったく問題ない
            Else
                '自分以外の誰かがロックしている。

                Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryInquiry, _LoginInfo)
                If (authInfo.AuthOption = "U_LOCK_CANCEL") Then
                    ret.ok = True
                    ret.message = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_LOCK_OTHER_USER_ENABLE)
                    Return Json(ret)
                Else
                    ret.err = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_LOCK_OTHER_USER_DISABLE)
                    Return Json(ret)
                End If
            End If

            ret.ok = True
            Return Json(ret)

        End Function

        ''' <summary>
        ''' アセットをアンロックする
        ''' </summary>
        ''' <param name="sAssetNos">資産番号をカンマで結合させた文字列</param>
        Public Function Unlock(ViewModel As InventoryInquiryViewModel) As ActionResult
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return RedirectToAction("Index", New With {.actionname = CommonConst.KEY_ON_CREATE_CLICK})
            End If

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            Dim ret = New ApiResponse With {
                .ok = False
            }
            Dim lstSelectKeys As List(Of String) = UpdateChecedInfo(ViewModel.SelectKeys, ViewModel.NoneSelectKeys)

            Dim lockUsers = ToolInvAsset.GetLockUsers(invId, invLocL, lstSelectKeys)
            If (lockUsers.Count = 0 OrElse (lockUsers.Count = 1 AndAlso lockUsers(0) = _LoginInfo.EmployeeCd)) Then
                '自身がロックもしくはロックされていないので、まったく問題ない
            Else
                '自分以外の誰かがロックしている。
                Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryInquiry, _LoginInfo)
                If (authInfo.AuthOption = "U_LOCK_CANCEL") Then
                    '無処理
                Else
                    ret.err = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_LOCK_OTHER_USER_DISABLE)
                    Return Json(ret)
                End If
            End If

            Dim isInventory = ToolInventory.IsIventory(invId, invLocL, lstSelectKeys)
            If (isInventory) Then
                ret.err = Me._Message.getMessage(CommonLabelInventoryInquiry.MSG_NOT_INVENTORY_NOW)
                Return Json(ret)
            End If

            Dim sTaskNm As String = "InventoryInquiryController.Unlock"

            Dim result = SetLockUnLock(lstSelectKeys, False, sTaskNm)
            If (result) Then
                ret.ok = True
                Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY) = ViewModel
            Else
                ret.err = Me._Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)
            End If

            Return Json(ret)
        End Function

        Private Function SetLockUnLock(sAssetNoList As List(Of String), isLock As Boolean, sTaskNm As String) As Boolean
            'ログインセッションが切れていたらエラーメッセージを表示しログイン画面に遷移する
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)
            If IsNothing(_LoginInfo) Then
                '結果表示
                Return False
            End If

            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)

            Using tran = db.Database.BeginTransaction()
                Try

                    Dim dataForLog = New List(Of EditedContainModel)

                    For Each assetNo In sAssetNoList
                        Dim tblAsset As DataTable = ToolInvAsset.GetInvAsset(invId, invLocL, assetNo)
                        Dim data As New EditedContainModel()
                        data.ASSET_NO = assetNo
                        data.ASSET_NAME = tblAsset.Rows(0)("ASSET_NAME").ToString
                        data.LOC_L_CD = If(IsDBNull(tblAsset.Rows(0)("LOC_L_CD")), "", tblAsset.Rows(0)("LOC_L_CD").ToString)
                        data.LOC_M_CD = If(IsDBNull(tblAsset.Rows(0)("LOC_M_CD")), "", tblAsset.Rows(0)("LOC_M_CD").ToString)
                        data.LOC_S_CD = If(IsDBNull(tblAsset.Rows(0)("LOC_S_CD")), "", tblAsset.Rows(0)("LOC_S_CD").ToString)
                        dataForLog.Add(data)
                    Next

                    sbSQL.Clear()
                    params = New List(Of SqlParameter)

                    sbSQL.AppendLine(" UPDATE T_INV_ASSET SET")
                    If isLock Then
                        sbSQL.AppendLine("  LOCK_EMPLOYEE_CD = '" & _LoginInfo.EmployeeCd & "'")
                    Else
                        sbSQL.AppendLine("  LOCK_EMPLOYEE_CD = NULL ")
                    End If

                    sbSQL.AppendLine("  ,UPD_DATETIME = " & clsSV_DB.SQL_GET_DATE2)
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_CD = @UPD_EMPLOYEE_CD")
                    sbSQL.AppendLine("  ,UPD_EMPLOYEE_NAME = @UPD_EMPLOYEE_NAME")
                    sbSQL.AppendLine("  ,UPD_TASK = @UPD_TASK")
                    sbSQL.AppendLine($" WHERE ASSET_NO IN ({"'" + String.Join("','", sAssetNoList) + "'"})")
                    sbSQL.AppendLine($" AND INV_ID = {invId}")
                    sbSQL.AppendLine($" AND INV_LOC_L_CD = '{invLocL}'")

                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_CD", _LoginInfo.EmployeeCd))
                    params.Add(New SqlParameter($"@UPD_EMPLOYEE_NAME", _LoginInfo.EmployeeNm))
                    params.Add(New SqlParameter($"@UPD_TASK", sTaskNm))

                    db.Database.ExecuteSqlCommand(sbSQL.ToString(), params.ToArray)

                    'チェック状態のクリア
                    _ViewModel.SelectKeys = String.Empty
                    _ViewModel.NoneSelectKeys = String.Empty
                    Session.Remove(CommonSessionKey.LIST_CHECKED_KEYS)

                    'モデルの内容をセッションに保持
                    Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY) = _ViewModel

                    For Each assetNo In sAssetNoList
                        '操作ログ出力
                        Dim sProcessContents As String = If(isLock, "ロック", "アンロック") & " " &
                                                        Me._Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT) & " " &
                                                        Me._Message.getMessage(CommonLabel.LABEL_ASSET_NO) & ": " & assetNo.Trim & "  " &
                                                        Me._Message.getMessage(CommonLabel.LABEL_ASSET_NAME) & ": " &
                                                        dataForLog.Find(Function(d) d.ASSET_NO = assetNo).ASSET_NAME
                        Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, _LoginInfo,
                                                        If(isLock, "ロック", "アンロック"),
                                                        dataForLog.Find(Function(d) d.ASSET_NO = assetNo).LOC_L_CD,
                                                        dataForLog.Find(Function(d) d.ASSET_NO = assetNo).LOC_M_CD,
                                                        dataForLog.Find(Function(d) d.ASSET_NO = assetNo).LOC_S_CD)
                    Next

                    tran.Commit()

                    Return True

                Catch ex As Exception
                    tran.Rollback()

                    logger.Error(ex.Message)
                    logger.Error(sbSQL)

                    Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
                End Try
            End Using

            Return False
        End Function

#End Region

#Region "棚卸管理テーブルデータ取得"
        ''' <summary>
        ''' 棚卸管理テーブルデータ取得
        ''' </summary>
        Private Sub GetInventry()
            Dim db As SystemDBContext = New SystemDBContext
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter)

            sbSQL.Clear()
            params = New List(Of SqlParameter)

            sbSQL.AppendLine("SELECT 0 AS START_DEC_TS")        'ダミーのキー値

            '帳簿在庫数 SUM(保管、貸出)
            sbSQL.AppendLine("       ,COUNT_BIG(")
            sbSQL.AppendLine("           CASE WHEN ia.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ") THEN ia.ASSET_NO")
            sbSQL.AppendLine("           ELSE NULL END")
            sbSQL.AppendLine("        ) AS LOGIC_INV_NUM")

            '実棚数 SUM(正常、警告)
            sbSQL.AppendLine("       ,COUNT_BIG(")
            sbSQL.AppendLine("           CASE WHEN ia.PHYINV_STATUS IN ('" & PHYINV_STATUS.Normal & "', '" & PHYINV_STATUS.Warning & "') THEN ia.ASSET_NO")
            sbSQL.AppendLine("           ELSE NULL END")
            sbSQL.AppendLine("        ) AS ACTUAL_NUMBER_OF_SHELVES")

            '正常数 SUM(正常)
            sbSQL.AppendLine("       ,COUNT_BIG(")
            sbSQL.AppendLine("           CASE WHEN ia.PHYINV_STATUS IN ('" & PHYINV_STATUS.Normal & "') THEN ia.ASSET_NO")
            sbSQL.AppendLine("           ELSE NULL END")
            sbSQL.AppendLine("        ) AS PHYINV_NUM")

            'ロケ違い数 SUM(警告)
            sbSQL.AppendLine("       ,COUNT_BIG(")
            sbSQL.AppendLine("           CASE WHEN ia.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ") ")
            sbSQL.AppendLine("                       AND   ia.PHYINV_STATUS = '" & PHYINV_STATUS.Warning & "' THEN ia.ASSET_NO")
            sbSQL.AppendLine("           ELSE NULL END")
            sbSQL.AppendLine("        ) AS LOC_DIFF_NUM")

            '在庫なし数 SUM(現物なし(未処理を含む))
            sbSQL.AppendLine("       ,COUNT_BIG(")
            sbSQL.AppendLine("           CASE WHEN ia.ASSET_STATUS In (" & ASSET_STATUS.Storage & "," & ASSET_STATUS.Lending & ") ")
            sbSQL.AppendLine("                       AND   ia.PHYINV_STATUS = '" & PHYINV_STATUS.NoSpot & "' THEN ia.ASSET_NO")
            sbSQL.AppendLine("           ELSE NULL END")
            sbSQL.AppendLine("        ) AS NO_ITEM_NUM")

            sbSQL.AppendLine("      ,COUNT(*) OVER () AS TotalCount ")

            sbSQL.AppendLine("FROM T_INV_ASSET ia")
            sbSQL.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AUTH")
            sbSQL.AppendLine("ON  AUTH.LOC_L_CD = ia.LOC_L_CD")
            sbSQL.AppendLine("AND ((AUTH.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND AUTH.LOC_M_CD = ia.LOC_M_CD) OR (AUTH.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")

            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            Dim authInfo As AuthInfo = Me._Message.GetFunctionAuth(Misc.Auth.FunctionId.InventoryInquiry, _LoginInfo)
            params.Add(New SqlParameter($"@EmployeeCd", _LoginInfo.EmployeeCd))
            params.Add(New SqlParameter($"@ReadAuthFlg", authInfo.ReadAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthFlg", authInfo.UpdAuthFlg))
            params.Add(New SqlParameter($"@ReadHubAuthFlg", authInfo.ReadHubAuthFlg))
            params.Add(New SqlParameter($"@UpdHubAuthFlg", authInfo.UpdHubAuthFlg))
            params.Add(New SqlParameter($"@UpdAuthOption", authInfo.AuthOption))

            sbSQL.AppendLine("WHERE 1 = 1")

            '検索条件を設定する
            'ロケーション 大区分
            If Not IsNothing(_ViewModel.SearchLocL) AndAlso _ViewModel.SearchLocL <> "" Then
                sbSQL.AppendLine("AND ia.LOC_L_CD = @locLCd")
                params.Add(New SqlParameter($"@locLCd", _ViewModel.SearchLocL))
            End If
            'ロケーション 中区分（複数選択）
            sbSQL.AppendLine(CreateInConditionTo("ia.LOC_M_CD", "locMCd", _ViewModel.SearchLocM, params))
            'ロケーション 小区分（複数選択）
            sbSQL.AppendLine(CreateInConditionTo("ia.LOC_S_CD", "locSCd", _ViewModel.SearchLocS, params))

            'カテゴリー 大分類（複数選択）
            sbSQL.AppendLine(CreateInConditionTo("ia.CAT_L_CD", "catLCd", _ViewModel.SearchCatL, params))

            ' 実棚ステータス 全て
            If _ViewModel.SearchPhyinvStatusAll = False Then
                Dim statusCondition As List(Of String) = New List(Of String)
                If _ViewModel.SearchPhyinvStatusN Then statusCondition.Add("'" & PHYINV_STATUS.Normal & "'")
                If _ViewModel.SearchPhyinvStatusW Then statusCondition.Add("'" & PHYINV_STATUS.Warning & "'")
                If _ViewModel.SearchPhyinvStatusE Then statusCondition.Add("'" & PHYINV_STATUS.NoSpot & "'")
                If statusCondition.Count = 1 Then
                    sbSQL.AppendLine("AND ia.PHYINV_STATUS = " & statusCondition(0))
                Else
                    sbSQL.Append("AND ia.PHYINV_STATUS IN (")
                    For i As Integer = 0 To statusCondition.Count - 1
                        If i > 0 Then
                            sbSQL.Append("," & statusCondition(i))
                        Else
                            sbSQL.Append(statusCondition(i))
                        End If
                    Next
                    sbSQL.AppendLine(")")
                End If
            End If
            ' 資産番号（「|」区切りで複数指定。資産番号・子番号のいずれかに前方一致）
            sbSQL.AppendLine(CreateLikeConditionTo({"ia.ASSET_NO", "ia.SUB_ASSET_NO"}, "assetNo", CommonRfid.GetMultiInputValues(_ViewModel.SearchAssetNo), params))
            ' 資産名称
            If Not IsNothing(_ViewModel.SearchAssetName) AndAlso _ViewModel.SearchAssetName <> "" Then
                sbSQL.AppendLine("  AND ia.ASSET_NAME Like @assetName")
                params.Add(New SqlParameter($"@assetName", $"%{_ViewModel.SearchAssetName}%"))
            End If
            ' メーカ
            If Not IsNothing(_ViewModel.SearchMaker) AndAlso _ViewModel.SearchMaker <> "" Then
                sbSQL.AppendLine("  AND ia.MAKER Like @maker")
                params.Add(New SqlParameter($"@maker", $"%{_ViewModel.SearchMaker}%"))
            End If
            ' 品番
            If Not IsNothing(_ViewModel.SearchProductNo) AndAlso _ViewModel.SearchProductNo <> "" Then
                sbSQL.AppendLine("  AND ia.PRODUCT_NO Like @productNo")
                params.Add(New SqlParameter($"@productNo", $"%{_ViewModel.SearchProductNo}%"))
            End If
            ' 型番
            If Not IsNothing(_ViewModel.SearchModelNo) AndAlso _ViewModel.SearchModelNo <> "" Then
                sbSQL.AppendLine("  AND ia.MODEL_NO Like @modelNo")
                params.Add(New SqlParameter($"@modelNo", $"%{_ViewModel.SearchModelNo}%"))
            End If
            '有形・無形
            Dim tangibleCondition As List(Of String) = New List(Of String)
            If _ViewModel.SearchTangibleCheck Then tangibleCondition.Add(TANGIBLE.TANGIBLE)
            If _ViewModel.SearchNotTangibleCheck Then tangibleCondition.Add(TANGIBLE.NOT_TANGIBLE)
            If tangibleCondition.Count = 1 Then
                sbSQL.AppendLine("AND ia.IS_TANGIBLE = '" & tangibleCondition(0) & "'")
            End If

            sbSQL = SetSearchCondition(sbSQL, params)

            _viewInventory = db.InventoryView.SqlQuery(sbSQL.ToString, params.ToArray).ToList

            ViewBag.LogicInvNum = 0
            ViewBag.PhyinvNum = 0
            ViewBag.LocDiffNum = 0
            ViewBag.NoItemNum = 0
            If _viewInventory.Count > 0 Then
                ViewBag.LogicInvNum = _viewInventory(0).LOGIC_INV_NUM
                ViewBag.PhyinvNum = _viewInventory(0).PHYINV_NUM
                ViewBag.LocDiffNum = _viewInventory(0).LOC_DIFF_NUM
                ViewBag.NoItemNum = _viewInventory(0).NO_ITEM_NUM
            End If

        End Sub
#End Region

#Region "最新のチェック状態でセッション情報を更新する"
        ''' <summary>
        ''' 最新のチェック状態でセッション情報を更新する
        ''' 重複を排除したチェックされたキーを返却する
        ''' </summary>
        ''' <param name="SelectKeys">選択キー</param>
        ''' <param name="NoneSelectKeys">未選択キー</param>
        ''' <returns>チェックされた行のキーリスト</returns>
        Private Function UpdateChecedInfo(ByVal SelectKeys As String, ByVal NoneSelectKeys As String) As List(Of String)
            '--2020/01/07 miyaji add start
            '一覧にない資産は未選択キーとする
            If Not IsNothing(_ViewModel.ASSET_NO) Then
                Dim results = From data In _viewResult Where data.ASSET_NO = _ViewModel.ASSET_NO
                If results.Count <= 0 Then
                    NoneSelectKeys = String.Join("/", New String() {NoneSelectKeys, $"{_ViewModel.ASSET_NO},{_ViewModel.PHYINV_STATUS}"})
                End If
            End If
            '--2020/01/07 miyaji add end

            'セッションの選択キー情報を取得する
            Dim lstSelectKeys As New List(Of String)
            If Not IsNothing(Session(CommonSessionKey.LIST_CHECKED_KEYS)) Then
                lstSelectKeys = Session(CommonSessionKey.LIST_CHECKED_KEYS)
            End If

            '選択キーを追加する
            If Not IsNothing(SelectKeys) AndAlso SelectKeys <> String.Empty Then
                lstSelectKeys.AddRange(SelectKeys.Split("/").ToList)
            End If

            '--2020/01/07 miyaji add start
            '重複を排除する
            lstSelectKeys = lstSelectKeys.Distinct.ToList
            '--2020/01/07 miyaji add end

            '未選択キーを削除する
            If Not IsNothing(NoneSelectKeys) AndAlso NoneSelectKeys <> String.Empty Then
                For Each deleteKey In NoneSelectKeys.Split("/").ToList
                    lstSelectKeys.Remove(deleteKey)
                Next
            End If

            ''重複を排除する   '2020/01/07 nv miyaji　コメントアウト
            'lstSelectKeys = lstSelectKeys.Distinct.ToList

            'セッションを更新する
            Session(CommonSessionKey.LIST_CHECKED_KEYS) = lstSelectKeys

            'チェックされた行のキーリストを返却する
            Return lstSelectKeys
        End Function
#End Region


#Region "現在の編集状態でセッション情報を更新する"
        ''' <summary>
        ''' 現在の編集状態でセッション情報を更新する
        ''' </summary>
        ''' <param name="editedContains">画面の編集情報</param>
        ''' <returns>チェックされた行のキーリスト</returns>
        Private Function UpdateEditedContains(editedContains As List(Of EditedContainModel))

            'セッションの編集情報を取得する
            Dim sess As New List(Of EditedContainModel)
            If Not IsNothing(Session(CommonSessionKey.LIST_EDITED_CONTAINS)) Then
                sess = Session(CommonSessionKey.LIST_EDITED_CONTAINS)
            End If

            If (editedContains IsNot Nothing) Then
                For Each e In editedContains
                    '既にセッションに存在する場合は削除する
                    Dim exists = From s In sess Where s.ASSET_NO = e.ASSET_NO
                    If exists.Count > 0 Then
                        sess.Remove(exists(0))
                    End If
                    'セッションに追加する
                    sess.Add(e)
                Next
            End If

            'セッションを更新する
            Session(CommonSessionKey.LIST_EDITED_CONTAINS) = sess

            Return sess
        End Function
#End Region

#Region "タグ発行用CSV出力のデータを取得"
        ''' <summary>
        ''' タグ発行用CSV出力のデータを取得します
        ''' </summary>
        Private Function GetSelectedData(SelectKeys As String) As List(Of AssetListLabelModel)
            Dim lstAsset As New List(Of AssetListLabelModel)
            Dim db As SystemDBContext = New SystemDBContext

            Dim keys As String() = SelectKeys.Split(",")

            For Each key As String In keys
                Dim sbSQL As StringBuilder = New StringBuilder
                sbSQL.AppendLine("SELECT ")
                sbSQL.AppendLine("  ASSET.ASSET_NO ")
                sbSQL.AppendLine("  , ASSET.ASSET_ID ")
                sbSQL.AppendLine("  , ASSET.EPC_SIZE ")
                sbSQL.AppendLine("  , ASSET.GEN_ASSET_ID ")
                sbSQL.AppendLine("  , ASSET.ASSET_DIV ")
                sbSQL.AppendLine("  , ASSET.ASSET_NAME ")
                sbSQL.AppendLine("  , IsNull(ASSET.LOC_L_CD, '') AS LOC_L_CD")
                sbSQL.AppendLine("  , IsNull(ASSET.LOC_M_CD, '') AS LOC_M_CD")
                sbSQL.AppendLine("  , IsNull(ASSET.LOC_S_CD, '') AS LOC_S_CD")
                sbSQL.AppendLine("  , IsNull(ASSET.CAT_L_CD, '') AS CAT_L_CD")
                sbSQL.AppendLine("  , IsNull(ASSET.CAT_M_CD, '') AS CAT_M_CD")
                sbSQL.AppendLine("  , IsNull(ASSET.CAT_S_CD, '') AS CAT_S_CD")
                sbSQL.AppendLine("  , ASSET.MAKER ")
                sbSQL.AppendLine("  , ASSET.PRODUCT_NO ")
                sbSQL.AppendLine("  , ASSET.MODEL_NO ")
                sbSQL.AppendLine("  , ASSET.LEASE_NO ")
                sbSQL.AppendLine("  , ASSET.LEASE_LIMIT ")
                sbSQL.AppendLine("  , ASSET.GENERIC_LIMIT ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM1 ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM2 ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM3 ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM4 ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM5 ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM6 ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM7 ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM8 ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM9 ")
                sbSQL.AppendLine("  , ASSET.FREE_ITEM10 ")
                sbSQL.AppendLine("  , ASSET.ASSET_STATUS ")
                sbSQL.AppendLine("  , ASSET.TAG_LINK_STATUS ")
                sbSQL.AppendLine("  , LOCL.LOC_NAME AS LOC_L_NAME ")
                sbSQL.AppendLine("  , LOCM.LOC_NAME AS LOC_M_NAME ")
                sbSQL.AppendLine("  , LOCS.LOC_NAME AS LOC_S_NAME ")
                sbSQL.AppendLine("  , CATL.CAT_NAME AS CAT_L_NAME ")
                sbSQL.AppendLine("  , CATM.CAT_NAME AS CAT_M_NAME ")
                sbSQL.AppendLine("  , CATS.CAT_NAME AS CAT_S_NAME ")
                sbSQL.AppendLine("FROM ")
                sbSQL.AppendLine("  M_ASSET ASSET ")
                sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCL ")
                sbSQL.AppendLine("ON LOCL.LOC_DIV = '" + LOC_DIV.LocationL + "'")
                sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCL.LOC_L_CD")
                sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCM ")
                sbSQL.AppendLine("ON LOCM.LOC_DIV = '" + LOC_DIV.LocationM + "'")
                sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCM.LOC_L_CD")
                sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCM.LOC_M_CD")
                sbSQL.AppendLine("LEFT JOIN M_LOCATION LOCS ")
                sbSQL.AppendLine("ON LOCS.LOC_DIV = '" + LOC_DIV.LocationS + "'")
                sbSQL.AppendLine("AND ASSET.LOC_L_CD = LOCS.LOC_L_CD")
                sbSQL.AppendLine("AND ASSET.LOC_M_CD = LOCS.LOC_M_CD")
                sbSQL.AppendLine("AND ASSET.LOC_S_CD = LOCS.LOC_S_CD")
                sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATL ")
                sbSQL.AppendLine("ON CATL.CAT_DIV = '" + CAT_DIV.CategoryL + "'")
                sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATL.CAT_L_CD")
                sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATM ")
                sbSQL.AppendLine("ON CATM.CAT_DIV = '" + CAT_DIV.CategoryM + "'")
                sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATM.CAT_L_CD")
                sbSQL.AppendLine("AND ASSET.CAT_M_CD = CATM.CAT_M_CD")
                sbSQL.AppendLine("LEFT JOIN M_CATEGORY CATS ")
                sbSQL.AppendLine("ON CATS.CAT_DIV = '" + CAT_DIV.CategoryS + "'")
                sbSQL.AppendLine("AND ASSET.CAT_L_CD = CATS.CAT_L_CD")
                sbSQL.AppendLine("AND ASSET.CAT_M_CD = CATS.CAT_M_CD")
                sbSQL.AppendLine("AND ASSET.CAT_S_CD = CATS.CAT_S_CD")
                sbSQL.AppendLine("WHERE ")
                sbSQL.AppendLine("  ASSET.ASSET_NO = '" & key & "' ")
                sbSQL.AppendLine(" ORDER BY ASSET_NO")

                'SQLを実行する
                Dim lst As List(Of AssetListLabelModel)
                lst = db.AssetListLabel.SqlQuery(sbSQL.ToString).ToList
                If lst.Count > 0 Then
                    lstAsset.Add(lst(0))
                End If
            Next

            'データを返却
            Return lstAsset
        End Function
#End Region

#Region "CSVヘッダー取得"
        ''' <summary>
        ''' プリンタ情報取得
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

        ''' <summary>
        ''' 変更内容を非同期通信で取得します（AJax)
        ''' </summary>
        ''' <param name="assetNo">assetNo</param>
        ''' <returns>View表示内容</returns>
        Public Function GetEditData(ByVal assetNo As String) As JsonResult
            Dim db As New SystemDBContext

            'ログイン情報を取得。
            _LoginInfo = Session(CommonSessionKey.CREAGE_LOGIN)

            'メッセージ一覧取得
            Me._Message = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)

            Dim keys As String() = Session(CommonSessionKey.CREAGE_INVENTORY_INQUIRY_KEY).Split("/")
            Dim invId As Long
            Long.TryParse(keys(0), invId)
            Dim invLocL = keys(1)

            Dim whereSql = "WHERE ia.ASSET_NO = '" & assetNo & "'"
            whereSql += $" AND INV_ID = {invId}"
            whereSql += $" AND INV_LOC_L_CD = '{invLocL}'"

            Dim sql = GetBaseSql(whereSql)

            'IDをキーにモデルを取得する
            Dim model As InventoryInquiryViewModel = db.InventoryInquiryView.SqlQuery(sql, _paramSqlList.ToArray).First()

            Return Json(model)

        End Function

#Region "共通"

        ''' <summary>
        ''' 一覧データ取得用SQLを作成する
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSQL() As String

#Region "検索条件の作成"
            Dim sbWhr As New StringBuilder
            '必須条件
            sbWhr.AppendLine("WHERE ia.ASSET_STATUS IN ('" & ASSET_STATUS.Storage & "','" & ASSET_STATUS.Lending & "')")

            '20231012 ADD Start Shirakawa
            '初回(_ViewModel.SearchLocL = "")の時に遷移時に受け取った値をロケ大CDを使う
            'コンボボックスの空白(全ロケCD取得)を選んだ場合Nothingが返ってくる
            If _ViewModel.SearchLocL Is String.Empty Then
                _ViewModel.SearchLocL = _invLocLCd
            End If
            '20231012 ADD End Shirakawa

            '検索条件を設定する
            'ロケーション 大区分
            If Not IsNothing(_ViewModel.SearchLocL) AndAlso _ViewModel.SearchLocL <> "" Then
                sbWhr.AppendLine("AND ia.LOC_L_CD = @locLCd")
                addSqlParam("locLCd", _ViewModel.SearchLocL)
            End If
            'ロケーション 中区分（複数選択）
            sbWhr.AppendLine(CreateInCondition("ia.LOC_M_CD", "locMCd", _ViewModel.SearchLocM))
            'ロケーション 小区分（複数選択）
            sbWhr.AppendLine(CreateInCondition("ia.LOC_S_CD", "locSCd", _ViewModel.SearchLocS))

            'カテゴリー 大分類（複数選択）
            sbWhr.AppendLine(CreateInCondition("ia.CAT_L_CD", "catLCd", _ViewModel.SearchCatL))

            ' 実棚ステータス 全て
            'If _ViewModel.SearchPhyinvStatusAll = False Then
            Dim statusCondition As List(Of String) = New List(Of String)
            If _ViewModel.SearchPhyinvStatusN Then statusCondition.Add("'" & PHYINV_STATUS.Normal & "'")
            If _ViewModel.SearchPhyinvStatusW Then statusCondition.Add("'" & PHYINV_STATUS.Warning & "'")
            If _ViewModel.SearchPhyinvStatusE Then statusCondition.Add("'" & PHYINV_STATUS.NoSpot & "'")
            If statusCondition.Count = 0 Then
                '全てチェックなしの場合は、バイパス      2019/10/06 ADD
            ElseIf statusCondition.Count = 1 Then
                sbWhr.AppendLine("AND ia.PHYINV_STATUS = " & statusCondition(0))
            Else
                sbWhr.Append("AND ia.PHYINV_STATUS IN (")
                For i As Integer = 0 To statusCondition.Count - 1
                    If i > 0 Then
                        sbWhr.Append("," & statusCondition(i))
                    Else
                        sbWhr.Append(statusCondition(i))
                    End If
                Next
                sbWhr.AppendLine(")")
            End If
            'End If
            ' 資産番号（「|」区切りで複数指定。資産番号・子番号のいずれかに前方一致）
            sbWhr.AppendLine(CreateLikeCondition({"ia.ASSET_NO", "ia.SUB_ASSET_NO"}, "assetNo", CommonRfid.GetMultiInputValues(_ViewModel.SearchAssetNo)))
            ' 資産名称
            If Not IsNothing(_ViewModel.SearchAssetName) AndAlso _ViewModel.SearchAssetName <> "" Then
                sbWhr.AppendLine("  AND ia.ASSET_NAME Like @assetName")
                addSqlParam("assetName", $"%{_ViewModel.SearchAssetName}%")
            End If
            ' メーカ
            If Not IsNothing(_ViewModel.SearchMaker) AndAlso _ViewModel.SearchMaker <> "" Then
                sbWhr.AppendLine("  AND ia.MAKER Like @maker")
                addSqlParam("maker", $"%{_ViewModel.SearchMaker}%")
            End If
            ' 品番
            If Not IsNothing(_ViewModel.SearchProductNo) AndAlso _ViewModel.SearchProductNo <> "" Then
                sbWhr.AppendLine("  AND ia.PRODUCT_NO Like @productNo")
                addSqlParam("productNo", $"%{_ViewModel.SearchProductNo}%")
            End If
            ' 型番
            If Not IsNothing(_ViewModel.SearchModelNo) AndAlso _ViewModel.SearchModelNo <> "" Then
                sbWhr.AppendLine("  AND ia.MODEL_NO Like @modelNo")
                addSqlParam("modelNo", $"%{_ViewModel.SearchModelNo}%")
            End If

            '有形・無形区分
            If _ViewModel.SearchTangibleCheck <> _ViewModel.SearchNotTangibleCheck Then
                '有形資産、無形資産の値が違う場合は条件に追加
                sbWhr.AppendLine("  AND ia.IS_TANGIBLE = " & "@SearchIsTangible")
                If _ViewModel.SearchTangibleCheck Then
                    addSqlParam("SearchIsTangible", $"{ TANGIBLE.TANGIBLE }")
                Else
                    addSqlParam("SearchIsTangible", $"{ TANGIBLE.NOT_TANGIBLE }")
                End If
            End If

            sbWhr = SetSearchCondition(sbWhr)

#End Region

            Dim sbSQL As New StringBuilder
            '一覧表示表用SQL
            sbSQL.Clear()
            sbSQL.AppendLine(GetBaseSql(sbWhr.ToString))


            '検索条件を設定する
            sbSQL.AppendLine("ORDER BY")
            '[CC431] UPD Start 20261002
            ' 検索条件で選択された並び順で並べる。第2キーは資産番号昇順（一覧・データ出力の両方で使用）
            '[CC431] UPD Start 20261005
            ' 並び順が資産番号のときは、第2キーを付けない（同じ列を ORDER BY に2回指定するとエラーになるため）
            Dim sortColumn As String = GetSortColumn(_ViewModel.SearchSortOrder)
            sbSQL.AppendLine(sortColumn)
            If sortColumn <> "ia.ASSET_NO" Then
                sbSQL.AppendLine(",ia.ASSET_NO")
            End If
            '[CC431] UPD End 20261005
            '[CC431] UPD End 20261002

            Return sbSQL.ToString()

        End Function

        '[CC431] ADD Start 20261002
        ''' <summary>
        ''' 並び順に対応するソートカラムを取得する
        ''' </summary>
        ''' <param name="sortOrder">検索条件 並び順(InventoryInquirySort)</param>
        ''' <returns>ORDER BY に指定するカラム名（想定外の値は資産クラス）</returns>
        ''' <remarks>原価センタ・場所はコード順。第2ソートキーの資産番号は呼び出し元で付与する</remarks>
        Private Function GetSortColumn(ByVal sortOrder As Integer) As String
            Select Case sortOrder
                Case InventoryInquirySort.CatLAsc
                    Return "ia.CAT_L_CD"
                Case InventoryInquirySort.AssetNoAsc
                    Return "ia.ASSET_NO"
                Case InventoryInquirySort.LocMAsc
                    Return "ia.LOC_M_CD"
                Case InventoryInquirySort.LocSAsc
                    Return "ia.LOC_S_CD"
                Case InventoryInquirySort.FreeItem2Asc
                    Return "ia.FREE_ITEM2"
                Case InventoryInquirySort.FreeItem6Asc
                    Return "ia.FREE_ITEM6"
                Case InventoryInquirySort.PhyinvEmployeeAsc
                    Return "ia.PHYINV_EMPLOYEE_CD"
                Case InventoryInquirySort.Remark1Asc
                    Return "ia.REMARK1"
                Case InventoryInquirySort.Remark2Asc
                    Return "ia.REMARK2"
                Case InventoryInquirySort.Remark3Asc
                    Return "ia.REMARK3"
                Case InventoryInquirySort.Remark4Asc
                    Return "ia.REMARK4"
                Case InventoryInquirySort.Remark5Asc
                    Return "ia.REMARK5"
                Case InventoryInquirySort.Remark6Asc
                    Return "ia.REMARK6"
                Case InventoryInquirySort.Remark7Asc
                    Return "ia.REMARK7"
                Case InventoryInquirySort.Remark8Asc
                    Return "ia.REMARK8"
                Case InventoryInquirySort.Remark9Asc
                    Return "ia.REMARK9"
                Case InventoryInquirySort.Remark10Asc
                    Return "ia.REMARK10"
                Case Else
                    Return "ia.CAT_L_CD"
            End Select
        End Function
        '[CC431] ADD End 20261002

        Private Sub MyAddSqlParam(ByVal strKey As String, ByVal strValue As String, ByRef params As List(Of SqlParameter))
            If (params Is Nothing) Then
                addSqlParam(strKey, strValue)
            Else
                params.Add(New SqlParameter($"@" + strKey, strValue))
            End If
        End Sub

        ''' <summary>
        ''' 複数選択された値のIN句を作成します（SQLパラメータの設定先を指定できる版）
        ''' </summary>
        ''' <param name="column">項目名</param>
        ''' <param name="paramName">パラメータ名(@なし)</param>
        ''' <param name="values">複数選択された値</param>
        ''' <param name="params">SQLパラメータの設定先（Nothing の場合は BaseController の _paramSqlList）</param>
        ''' <returns>IN句（未選択時は空文字）</returns>
        ''' <remarks>MyAddSqlParam と同じく、独自のパラメータリストを使うSQL（件数集計など）でも使えるようにする</remarks>
        Private Function CreateInConditionTo(ByVal column As String, ByVal paramName As String, ByVal values As String(), ByRef params As List(Of SqlParameter)) As String
            Dim startIndex As Integer = _paramSqlList.Count
            Dim sCond As String = CreateInCondition(column, paramName, values)
            MoveSqlParams(startIndex, params)
            Return sCond
        End Function

        '[CC431] UPD Start 20261002 部分一致を指定できるよう引数 isPartialMatch を追加
        ''' <summary>
        ''' 複数入力された値のLIKE条件（OR）を作成します（SQLパラメータの設定先を指定できる版）
        ''' </summary>
        ''' <param name="columns">項目名</param>
        ''' <param name="paramName">パラメータ名(@なし)</param>
        ''' <param name="values">複数入力された値</param>
        ''' <param name="params">SQLパラメータの設定先（Nothing の場合は BaseController の _paramSqlList）</param>
        ''' <param name="isPartialMatch">True：部分一致（%値%）、False：前方一致（値%）。省略時は前方一致</param>
        ''' <returns>LIKE条件（未入力時は空文字）</returns>
        Private Function CreateLikeConditionTo(ByVal columns As String(), ByVal paramName As String, ByVal values As String(), ByRef params As List(Of SqlParameter), Optional ByVal isPartialMatch As Boolean = False) As String
            Dim startIndex As Integer = _paramSqlList.Count
            Dim sCond As String = CreateLikeCondition(columns, paramName, values, isPartialMatch)
            '[CC431] UPD End 20261002
            MoveSqlParams(startIndex, params)
            Return sCond
        End Function

        ''' <summary>
        ''' _paramSqlList の startIndex 以降に追加されたSQLパラメータを params へ移します
        ''' </summary>
        ''' <remarks>params が Nothing の場合は移さない（_paramSqlList のまま使用する）</remarks>
        Private Sub MoveSqlParams(ByVal startIndex As Integer, ByRef params As List(Of SqlParameter))
            If params Is Nothing Then
                Return
            End If

            For i As Integer = startIndex To _paramSqlList.Count - 1
                params.Add(_paramSqlList(i))
            Next
            _paramSqlList.RemoveRange(startIndex, _paramSqlList.Count - startIndex)
        End Sub

        Private Function SetSearchCondition(ByRef sbWhr As StringBuilder, Optional ByRef params As List(Of SqlParameter) = Nothing) As StringBuilder

            'ロック担当者コード
            If (Not IsNothing(_ViewModel.SearchLockEmployee)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchLockEmployee.Trim)) Then
                sbWhr.AppendLine("  AND ia.LOCK_EMPLOYEE_CD like " & "@SearchLockEmployee")
                MyAddSqlParam("SearchLockEmployee", $"{_ViewModel.SearchLockEmployee.Trim & "%"}", params)
            End If

            '[CC431] ADD Start 20261002
            'ロック資産ステータス（未：ロック担当者なし、済：ロック担当者あり）
            If Not String.IsNullOrEmpty(_ViewModel.SearchLockStatus) Then
                If _ViewModel.SearchLockStatus = InvLockStatus.Unlocked Then
                    sbWhr.AppendLine("  AND ISNULL(ia.LOCK_EMPLOYEE_CD, '') = ''")
                ElseIf _ViewModel.SearchLockStatus = InvLockStatus.Locked Then
                    sbWhr.AppendLine("  AND ISNULL(ia.LOCK_EMPLOYEE_CD, '') <> ''")
                End If
            End If
            '[CC431] ADD End 20261002

            '転用or除却
            If (Not IsNothing(_ViewModel.SearchDiversionRemove)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchDiversionRemove.Trim)) Then
                sbWhr.AppendLine("  AND ia.DIVERSION_REMOVE = " & "@SearchDiversionRemove")
                MyAddSqlParam("SearchDiversionRemove", $"{_ViewModel.SearchDiversionRemove.Trim}", params)
            End If

            '稼働区分
            If (Not IsNothing(_ViewModel.SearchOpeStatus)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchOpeStatus.Trim)) Then
                sbWhr.AppendLine("  AND ISNULL(ia.OPE_STATUS,'') = " & "@SearchOpeStatus")
                If _ViewModel.SearchOpeStatus.Trim = CommonConst.VALUE_BLANK Then
                    MyAddSqlParam("SearchOpeStatus", "", params)
                Else
                    MyAddSqlParam("SearchOpeStatus", $"{_ViewModel.SearchOpeStatus.Trim}", params)
                End If
            End If

            '除却時期
            If (Not IsNothing(_ViewModel.SearchRemoveDate)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchRemoveDate.Trim)) Then
                Dim sDate As String = CDate(_ViewModel.SearchRemoveDate & "-01").ToString("yyyy/MM")
                sbWhr.AppendLine("  AND ia.REMOVE_DATE like " & "@SearchRemoveDate")
                MyAddSqlParam("SearchRemoveDate", $"{sDate & "%"}", params)
            End If

            '除却理由
            If (Not IsNothing(_ViewModel.SearchRemoveReason)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchRemoveReason.Trim)) Then
                sbWhr.AppendLine("  AND ia.REMOVE_REASON = " & "@SearchRemoveReason")
                MyAddSqlParam("SearchRemoveReason", $"{_ViewModel.SearchRemoveReason.Trim}", params)
            End If

            '除却内容
            If (Not IsNothing(_ViewModel.SearchRemoveContent)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchRemoveContent.Trim)) Then
                sbWhr.AppendLine("  AND ia.REMOVE_CONTENT like " & "@SearchRemoveContent")
                MyAddSqlParam("SearchRemoveContent", $"{"%" & _ViewModel.SearchRemoveContent.Trim & "%"}", params)
            End If

            '変更後部課コード（複数選択。コンボで選択するためコードの完全一致）
            sbWhr.AppendLine(CreateInConditionTo("ia.CHANGE_LOC_M_CD", "SearchChangeLocMCd", _ViewModel.SearchChangeLocMCd, params))

            '変更後設置場所コード（複数選択。コンボで選択するためコードの完全一致）
            sbWhr.AppendLine(CreateInConditionTo("ia.CHANGE_LOC_S_CD", "SearchChangeLocSCd", _ViewModel.SearchChangeLocSCd, params))

            '変更後設置社員番号
            If (Not IsNothing(_ViewModel.SearchChangeEmployee)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchChangeEmployee.Trim)) Then
                sbWhr.AppendLine("  AND ia.CHANGE_EMPLOYEE like " & "@SearchChangeEmployee")
                MyAddSqlParam("SearchChangeEmployee", $"{_ViewModel.SearchChangeEmployee.Trim & "%"}", params)
            End If

            'フリー項目10 SearchFreeItem10 貸与資産状態区分
            If (Not IsNothing(_ViewModel.SearchFreeItem10)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem10.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM10 IN (" & "@SearchFreeItem10" & "," & "@SearchFreeItem10Name" & ")")
                Select Case _ViewModel.SearchFreeItem10.Trim
                    Case LOAN_ASSET_STATUS.BLANK
                        MyAddSqlParam("SearchFreeItem10", "", params)
                        MyAddSqlParam("SearchFreeItem10Name", LOAN_ASSET_STATUS_VALUE.BLANK, params)
                    Case LOAN_ASSET_STATUS.ON_LOAN
                        MyAddSqlParam("SearchFreeItem10", LOAN_ASSET_STATUS.ON_LOAN, params)
                        MyAddSqlParam("SearchFreeItem10Name", LOAN_ASSET_STATUS_VALUE.ON_LOAN, params)
                    Case LOAN_ASSET_STATUS.RETURNED
                        MyAddSqlParam("SearchFreeItem10", LOAN_ASSET_STATUS.RETURNED, params)
                        MyAddSqlParam("SearchFreeItem10Name", LOAN_ASSET_STATUS_VALUE.RETURNED, params)
                End Select
            End If

            '[CC431] DEL 20261002 資本化日付の年月（前方一致）の条件を削除し、日付範囲に変更
            '[CC431] ADD Start 20261002
            '日付範囲の検索は "yyyy/MM/dd" の文字列の大小比較で行うため、書式外の値は検索対象にしない（ヒットさせない）
            Const sDateFormatCond As String = " LIKE '[0-9][0-9][0-9][0-9]/[0-9][0-9]/[0-9][0-9]'"

            'フリー項目2From SearchFreeItem2From 資本化日付
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem2From) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem2From).ToString("yyyy/MM/dd")
                sbWhr.AppendLine("  AND ia.FREE_ITEM2" & sDateFormatCond)
                sbWhr.AppendLine("  AND ia.FREE_ITEM2 >= " & "@SearchFreeItem2From")
                MyAddSqlParam("SearchFreeItem2From", sDate, params)
            End If

            'フリー項目2To SearchFreeItem2To 資本化日付
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem2To) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem2To).ToString("yyyy/MM/dd")
                sbWhr.AppendLine("  AND ia.FREE_ITEM2" & sDateFormatCond)
                sbWhr.AppendLine("  AND ia.FREE_ITEM2 <= " & "@SearchFreeItem2To")
                MyAddSqlParam("SearchFreeItem2To", sDate, params)
            End If
            '[CC431] ADD End 20261002

            'フリー項目4 SearchFreeItem4 稼働区分名
            If (Not IsNothing(_ViewModel.SearchFreeItem4)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem4.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM4 like " & "@SearchFreeItem4")
                MyAddSqlParam("SearchFreeItem4", $"{"%" & _ViewModel.SearchFreeItem4.Trim & "%"}", params)
            End If

            'フリー項目5 SearchFreeItem5 型式
            If (Not IsNothing(_ViewModel.SearchFreeItem5)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem5.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM5 like " & "@SearchFreeItem5")
                MyAddSqlParam("SearchFreeItem5", $"{"%" & _ViewModel.SearchFreeItem5.Trim & "%"}", params)
            End If

            'フリー項目6 SearchFreeItem6 管理社員番号
            If (Not IsNothing(_ViewModel.SearchFreeItem6)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem6.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM6 like " & "@SearchFreeItem6")
                MyAddSqlParam("SearchFreeItem6", $"{"%" & _ViewModel.SearchFreeItem6.Trim & "%"}", params)
            End If

            'フリー項目7 SearchFreeItem7 シリアルNo
            If (Not IsNothing(_ViewModel.SearchFreeItem7)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem7.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM7 like " & "@SearchFreeItem7")
                MyAddSqlParam("SearchFreeItem7", $"{"%" & _ViewModel.SearchFreeItem7.Trim & "%"}", params)
            End If

            'フリー項目8 SearchFreeItem8 WBS番号
            If (Not IsNothing(_ViewModel.SearchFreeItem8)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem8.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM8 like " & "@SearchFreeItem8")
                MyAddSqlParam("SearchFreeItem8", $"{"%" & _ViewModel.SearchFreeItem8.Trim & "%"}", params)
            End If

            'フリー項目9 SearchFreeItem9 備考
            If (Not IsNothing(_ViewModel.SearchFreeItem9)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem9.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM9 like " & "@SearchFreeItem9")
                MyAddSqlParam("SearchFreeItem9", $"{"%" & _ViewModel.SearchFreeItem9.Trim & "%"}", params)
            End If

            'フリー項目11From SearchFreeItem11From 取得金額
            If (Not IsNothing(_ViewModel.SearchFreeItem11From)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem11From.Trim)) Then
                sbWhr.AppendLine("  AND TRY_CAST(ia.FREE_ITEM11 AS decimal(23, 2)) >= " & "@SearchFreeItem11From")
                MyAddSqlParam("SearchFreeItem11From", $"{Decimal.Parse(_ViewModel.SearchFreeItem11From.Trim)}", params)
            End If

            'フリー項目11To SearchFreeItem11To 取得金額
            If (Not IsNothing(_ViewModel.SearchFreeItem11To)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem11To.Trim)) Then
                sbWhr.AppendLine("  AND TRY_CAST(ia.FREE_ITEM11 AS decimal(23, 2)) <= " & "@SearchFreeItem11To")
                MyAddSqlParam("SearchFreeItem11To", $"{Decimal.Parse(_ViewModel.SearchFreeItem11To.Trim)}", params)
            End If

            'フリー項目12From SearchFreeItem12From 当月末簿価
            If (Not IsNothing(_ViewModel.SearchFreeItem12From)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem12From.Trim)) Then
                sbWhr.AppendLine("  AND TRY_CAST(ia.FREE_ITEM12 AS decimal(23, 2)) >= " & "@SearchFreeItem12From")
                MyAddSqlParam("SearchFreeItem12From", $"{Decimal.Parse(_ViewModel.SearchFreeItem12From.Trim)}", params)
            End If

            'フリー項目12To SearchFreeItem10To 当月末簿価
            If (Not IsNothing(_ViewModel.SearchFreeItem12To)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem12To.Trim)) Then
                sbWhr.AppendLine("  AND TRY_CAST(ia.FREE_ITEM12 AS decimal(23, 2)) <= " & "@SearchFreeItem12To")
                MyAddSqlParam("SearchFreeItem12To", $"{Decimal.Parse(_ViewModel.SearchFreeItem12To.Trim)}", params)
            End If

            'フリー項目14 SearchFreeItem14 シリアル番号
            If (Not IsNothing(_ViewModel.SearchFreeItem14)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem14.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM14 like " & "@SearchFreeItem14")
                MyAddSqlParam("SearchFreeItem14", $"{"%" & _ViewModel.SearchFreeItem14.Trim & "%"}", params)
            End If

            'フリー項目15 SearchFreeItem15 資産グループコード
            If (Not IsNothing(_ViewModel.SearchFreeItem15)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem15.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM15 like " & "@SearchFreeItem15")
                MyAddSqlParam("SearchFreeItem15", $"{"%" & _ViewModel.SearchFreeItem15.Trim & "%"}", params)
            End If

            'フリー項目16 SearchFreeItem16 貸与先
            If (Not IsNothing(_ViewModel.SearchFreeItem16)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem16.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM16 like " & "@SearchFreeItem16")
                MyAddSqlParam("SearchFreeItem16", $"{"%" & _ViewModel.SearchFreeItem16.Trim & "%"}", params)
            End If

            'フリー項目17 SearchFreeItem17 貸与先名
            If (Not IsNothing(_ViewModel.SearchFreeItem17)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem17.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM17 like " & "@SearchFreeItem17")
                MyAddSqlParam("SearchFreeItem17", $"{"%" & _ViewModel.SearchFreeItem17.Trim & "%"}", params)
            End If

            '[CC431] DEL 20261002 開始日(貸与)・終了日(貸与)の完全一致の条件を削除し、日付範囲に変更
            '[CC431] ADD Start 20261002
            'フリー項目18From SearchFreeItem18From 開始日(貸与)
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem18From) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem18From).ToString("yyyy/MM/dd")
                sbWhr.AppendLine("  AND ia.FREE_ITEM18" & sDateFormatCond)
                sbWhr.AppendLine("  AND ia.FREE_ITEM18 >= " & "@SearchFreeItem18From")
                MyAddSqlParam("SearchFreeItem18From", sDate, params)
            End If

            'フリー項目18To SearchFreeItem18To 開始日(貸与)
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem18To) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem18To).ToString("yyyy/MM/dd")
                sbWhr.AppendLine("  AND ia.FREE_ITEM18" & sDateFormatCond)
                sbWhr.AppendLine("  AND ia.FREE_ITEM18 <= " & "@SearchFreeItem18To")
                MyAddSqlParam("SearchFreeItem18To", sDate, params)
            End If

            'フリー項目19From SearchFreeItem19From 終了日(貸与)
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem19From) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem19From).ToString("yyyy/MM/dd")
                sbWhr.AppendLine("  AND ia.FREE_ITEM19" & sDateFormatCond)
                sbWhr.AppendLine("  AND ia.FREE_ITEM19 >= " & "@SearchFreeItem19From")
                MyAddSqlParam("SearchFreeItem19From", sDate, params)
            End If

            'フリー項目19To SearchFreeItem19To 終了日(貸与)
            If Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem19To) Then
                Dim sDate As String = CDate(_ViewModel.SearchFreeItem19To).ToString("yyyy/MM/dd")
                sbWhr.AppendLine("  AND ia.FREE_ITEM19" & sDateFormatCond)
                sbWhr.AppendLine("  AND ia.FREE_ITEM19 <= " & "@SearchFreeItem19To")
                MyAddSqlParam("SearchFreeItem19To", sDate, params)
            End If
            '[CC431] ADD End 20261002

            'フリー項目20From SearchFreeItem20From 貸与回数
            If (Not IsNothing(_ViewModel.SearchFreeItem20From)) AndAlso
                    (IsNumeric(_ViewModel.SearchFreeItem20From.Trim)) Then
                sbWhr.AppendLine("  AND TRY_CAST(ia.FREE_ITEM20 AS decimal(21, 0)) >= " & "@SearchFreeItem20From")
                MyAddSqlParam("SearchFreeItem20From", $"{CInt(_ViewModel.SearchFreeItem20From.Trim)}", params)
            End If

            'フリー項目20To SearchFreeItem20To 貸与回数
            If (Not IsNothing(_ViewModel.SearchFreeItem20To)) AndAlso
                    (IsNumeric(_ViewModel.SearchFreeItem20To.Trim)) Then
                sbWhr.AppendLine("  AND TRY_CAST(ia.FREE_ITEM20 AS decimal(21, 0)) <= " & "@SearchFreeItem20To")
                MyAddSqlParam("SearchFreeItem20To", $"{CInt(_ViewModel.SearchFreeItem20To.Trim)}", params)
            End If

            'フリー項目21 SearchFreeItem21 備考欄
            If (Not IsNothing(_ViewModel.SearchFreeItem21)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem21.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM21 like " & "@SearchFreeItem21")
                MyAddSqlParam("SearchFreeItem21", $"{"%" & _ViewModel.SearchFreeItem21.Trim & "%"}", params)
            End If

            'フリー項目39 SearchFreeItem39 市区町村コード名称
            If (Not IsNothing(_ViewModel.SearchFreeItem39)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem39.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM39 like " & "@SearchFreeItem39")
                MyAddSqlParam("SearchFreeItem39", $"{"%" & _ViewModel.SearchFreeItem39.Trim & "%"}", params)
            End If

            'フリー項目42 SearchFreeItem42 CAPS番号
            If (Not IsNothing(_ViewModel.SearchFreeItem42)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem42.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM42 like " & "@SearchFreeItem42")
                MyAddSqlParam("SearchFreeItem42", $"{"%" & _ViewModel.SearchFreeItem42.Trim & "%"}", params)
            End If

            'フリー項目37 SearchFreeItem37 資産リース区分
            If (Not IsNothing(_ViewModel.SearchFreeItem37)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem37.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM37 like " & "@SearchFreeItem37")
                MyAddSqlParam("SearchFreeItem37", $"{"%" & _ViewModel.SearchFreeItem37.Trim & "%"}", params)
            End If

            'タグ紐付状態 SearchTagLinkStatus
            If (Not IsNothing(_ViewModel.SearchTagLinkStatus)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchTagLinkStatus)) Then
                sbWhr.AppendLine("  AND ia.TAG_LINK_STATUS = " & "@SearchTagLinkStatus")
                MyAddSqlParam("SearchTagLinkStatus", $"{_ViewModel.SearchTagLinkStatus}", params)
            End If

            '[CC431] ADD Start 20261002
            '実棚日From（実棚日時は YYYYMMDDHHMMSS の数値。From はその日の始まりから）
            If Not String.IsNullOrEmpty(_ViewModel.SearchPhyinvDateFrom) Then
                sbWhr.AppendLine("  AND ia.PHYINV_TS >= " & "@SearchPhyinvDateFrom")
                MyAddSqlParam("SearchPhyinvDateFrom", CDate(_ViewModel.SearchPhyinvDateFrom).ToString("yyyyMMdd") & "000000", params)
            End If

            '実棚日To（To はその日の終わりまで含める）
            If Not String.IsNullOrEmpty(_ViewModel.SearchPhyinvDateTo) Then
                sbWhr.AppendLine("  AND ia.PHYINV_TS <= " & "@SearchPhyinvDateTo")
                MyAddSqlParam("SearchPhyinvDateTo", CDate(_ViewModel.SearchPhyinvDateTo).ToString("yyyyMMdd") & "235959", params)
            End If

            '実棚者コード（前方一致）
            If (Not IsNothing(_ViewModel.SearchPhyinvEmployee)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchPhyinvEmployee.Trim)) Then
                sbWhr.AppendLine("  AND ia.PHYINV_EMPLOYEE_CD like " & "@SearchPhyinvEmployee")
                MyAddSqlParam("SearchPhyinvEmployee", $"{_ViewModel.SearchPhyinvEmployee.Trim & "%"}", params)
            End If

            '棚卸実施端末（実棚処理ツール区分）
            If Not String.IsNullOrEmpty(_ViewModel.SearchPhyinvToolDiv) Then
                sbWhr.AppendLine("  AND ia.PHYINV_TOOL_DIV = " & "@SearchPhyinvToolDiv")
                MyAddSqlParam("SearchPhyinvToolDiv", _ViewModel.SearchPhyinvToolDiv, params)
            End If

            'フリー項目27 SearchFreeItem27 棚卸ノート（部分一致）
            If (Not IsNothing(_ViewModel.SearchFreeItem27)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchFreeItem27.Trim)) Then
                sbWhr.AppendLine("  AND ia.FREE_ITEM27 like " & "@SearchFreeItem27")
                MyAddSqlParam("SearchFreeItem27", $"{"%" & _ViewModel.SearchFreeItem27.Trim & "%"}", params)
            End If

            '入力作業者（前方一致）
            If (Not IsNothing(_ViewModel.SearchEntEmployee)) AndAlso
                    (Not String.IsNullOrEmpty(_ViewModel.SearchEntEmployee.Trim)) Then
                sbWhr.AppendLine("  AND ia.ENT_EMPLOYEE_CD like " & "@SearchEntEmployee")
                MyAddSqlParam("SearchEntEmployee", $"{_ViewModel.SearchEntEmployee.Trim & "%"}", params)
            End If

            '詳細検索 備考1～備考10（「|」区切りで複数指定。同じ項目内は OR、項目同士は AND。部分一致）
            Dim dicRemark As New Dictionary(Of String, String) From {
                {"REMARK1", _ViewModel.SearchRemark1},
                {"REMARK2", _ViewModel.SearchRemark2},
                {"REMARK3", _ViewModel.SearchRemark3},
                {"REMARK4", _ViewModel.SearchRemark4},
                {"REMARK5", _ViewModel.SearchRemark5},
                {"REMARK6", _ViewModel.SearchRemark6},
                {"REMARK7", _ViewModel.SearchRemark7},
                {"REMARK8", _ViewModel.SearchRemark8},
                {"REMARK9", _ViewModel.SearchRemark9},
                {"REMARK10", _ViewModel.SearchRemark10}
            }
            For Each kvRemark As KeyValuePair(Of String, String) In dicRemark
                Dim sRemarkCond As String = CreateLikeConditionTo({$"ia.{kvRemark.Key}"}, $"Search{kvRemark.Key}", CommonRfid.GetMultiInputValues(kvRemark.Value), params, True)
                If Not String.IsNullOrEmpty(sRemarkCond) Then
                    sbWhr.AppendLine(sRemarkCond)
                End If
            Next
            '[CC431] ADD End 20261002

            Return sbWhr
        End Function

        Private Function GetBaseSql(whereSql As String) As String
            Dim m = HttpContext.Application(CommonConst.KEY_APP_LANGUAGE)
            '権限情報の取得        '--2021/11/10 add nv iwamoto
            Dim authInfo As AuthInfo = m.GetFunctionAuth(Misc.Auth.FunctionId.InventoryInquiry, _LoginInfo)

            Dim ret = New StringBuilder

            ret.AppendLine("SELECT")
            ret.AppendLine("ia.ASSET_NO")
            ret.AppendLine(",ia.ASSET_STATUS")
            ret.AppendLine(",CASE")
            ret.AppendLine("WHEN ia.ASSET_STATUS = " & ASSET_STATUS.Preparation & " THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_UNLINKED) & "'")  '未紐付け
            ret.AppendLine("WHEN ia.ASSET_STATUS = " & ASSET_STATUS.Storage & " THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_STOCK) & "'")     '在庫
            ret.AppendLine("WHEN ia.ASSET_STATUS = " & ASSET_STATUS.Lending & " THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_LENDING) & "'")   '貸出
            ret.AppendLine("WHEN ia.ASSET_STATUS = " & ASSET_STATUS.Removed & " THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_REMOVALE) & "'")  '除却
            ret.AppendLine("ELSE ''")
            ret.AppendLine("END ASSET_STATUS_NAME ")
            ret.AppendLine(",ia.LOCK_EMPLOYEE_CD + ' ' + le.EMPLOYEE_NAME AS LOCK_EMPLOYEE_CD_NAME")
            ret.AppendLine(",ia.LOCK_EMPLOYEE_CD")
            ret.AppendLine(",le.EMPLOYEE_NAME As LOCK_EMPLOYEE_NAME")
            ret.AppendLine(",ia.PHYINV_STATUS")
            ret.AppendLine(",CASE")
            ret.AppendLine("WHEN ia.PHYINV_STATUS = '" & PHYINV_STATUS.Excluded & "' THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_EXCLUDED) & "'")   '対象外
            ret.AppendLine("WHEN ia.PHYINV_STATUS = '" & PHYINV_STATUS.NoSpot & "' THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_NOSPOT) & "'")     '現物なし(未処理を含む)
            ret.AppendLine("WHEN ia.PHYINV_STATUS = '" & PHYINV_STATUS.Normal & "' THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_NORMAL) & "'")     '正常
            ret.AppendLine("WHEN ia.PHYINV_STATUS = '" & PHYINV_STATUS.Warning & "' THEN '" & m.getMessage(CommonLabelInventoryInquiry.TITLE_INV_INKIND) & "'")     '現物あり(ロケ違い)
            ret.AppendLine("ELSE ''")
            ret.AppendLine("END PHYINV_STATUS_NAME")

            ret.AppendLine(",ia.LOC_L_CD + ' ' + ml.LOC_NAME AS LOC_L_CD_NAME")
            ret.AppendLine(",ia.LOC_L_CD")
            ret.AppendLine(",ml.LOC_NAME LOC_L_NAME")
            ret.AppendLine(",ia.LOC_M_CD + ' ' + mm.LOC_NAME AS LOC_M_CD_NAME ")
            ret.AppendLine(",ia.LOC_M_CD")
            ret.AppendLine(",mm.LOC_NAME LOC_M_NAME")
            ret.AppendLine(",ia.LOC_S_CD + ' ' + ms.LOC_NAME AS LOC_S_CD_NAME ")
            ret.AppendLine(",ia.LOC_S_CD")
            ret.AppendLine(", ms.LOC_NAME LOC_S_NAME")

            ret.AppendLine(",ia.PHYINV_LOC_L_CD + ' ' + ia.PHYINV_LOC_L_NAME AS PHYINV_LOC_L_CD_NAME")
            ret.AppendLine(",ia.PHYINV_LOC_L_CD")
            ret.AppendLine(",ia.PHYINV_LOC_L_NAME")
            ret.AppendLine(",ia.PHYINV_LOC_M_CD + ' ' + ia.PHYINV_LOC_M_NAME AS PHYINV_LOC_M_CD_NAME")
            ret.AppendLine(",ia.PHYINV_LOC_M_CD")
            ret.AppendLine(",ia.PHYINV_LOC_M_NAME")
            ret.AppendLine(",ia.PHYINV_LOC_S_CD + ' ' + ia.PHYINV_LOC_S_NAME AS PHYINV_LOC_S_CD_NAME")
            ret.AppendLine(",ia.PHYINV_LOC_S_CD")
            ret.AppendLine(",ia.PHYINV_LOC_S_NAME")
            ret.AppendLine(",ia.ASSET_NAME")

            ret.AppendLine(",ia.CAT_L_CD + ' ' + mc.CAT_NAME AS CAT_L_CD_NAME")
            ret.AppendLine(",ia.CAT_L_CD")
            ret.AppendLine(",mc.CAT_NAME AS CAT_L_NAME")

            ret.AppendLine(",ia.IS_TANGIBLE")
            ret.AppendLine(",ia.SUB_ASSET_NO")
            ret.AppendLine(",ia.PHYINV_TS")
            ret.AppendLine(",ia.PHYINV_EMPLOYEE_CD")
            ret.AppendLine(",ia.PHYINV_EMPLOYEE_NAME")

            ret.AppendLine(",ia.MAKER")
            ret.AppendLine(",ia.PRODUCT_NO")
            ret.AppendLine(",ia.MODEL_NO")

            ret.AppendLine(",ia.FREE_ITEM6")
            ret.AppendLine(",me1.EMPLOYEE_NAME AS FREE_ITEM6_NAME")

            ret.AppendLine(",ia.FREE_ITEM1")
            ret.AppendLine(",ia.FREE_ITEM2")
            ret.AppendLine(",ia.FREE_ITEM3")
            ret.AppendLine(",ia.FREE_ITEM4")
            ret.AppendLine(",ia.FREE_ITEM5")
            ret.AppendLine(",ia.FREE_ITEM7")
            ret.AppendLine(",ia.FREE_ITEM8")
            ret.AppendLine(",ia.FREE_ITEM9")
            ret.AppendLine(",ia.FREE_ITEM10")
            ret.AppendLine(",ia.FREE_ITEM11")
            ret.AppendLine(",ia.FREE_ITEM12")
            ret.AppendLine(",ia.FREE_ITEM13")
            ret.AppendLine(",ia.FREE_ITEM14")
            ret.AppendLine(",ia.FREE_ITEM15")
            ret.AppendLine(",ia.FREE_ITEM16")
            ret.AppendLine(",ia.FREE_ITEM17")
            ret.AppendLine(",ia.FREE_ITEM18")
            ret.AppendLine(",ia.FREE_ITEM19")
            ret.AppendLine(",ia.FREE_ITEM20")
            ret.AppendLine(",ia.FREE_ITEM21")
            ret.AppendLine(",ia.FREE_ITEM22")
            ret.AppendLine(",ia.FREE_ITEM23")
            ret.AppendLine(",ia.FREE_ITEM24")
            ret.AppendLine(",ia.FREE_ITEM25")
            ret.AppendLine(",ia.FREE_ITEM26")
            ret.AppendLine(",ia.FREE_ITEM27")
            ret.AppendLine(",ia.FREE_ITEM28")
            ret.AppendLine(",ia.FREE_ITEM29")
            ret.AppendLine(",ia.FREE_ITEM30")
            ret.AppendLine(",ia.FREE_ITEM31")
            ret.AppendLine(",ia.FREE_ITEM32")
            ret.AppendLine(",ia.FREE_ITEM33")
            ret.AppendLine(",ia.FREE_ITEM34")
            ret.AppendLine(",ia.FREE_ITEM35")
            ret.AppendLine(",ia.FREE_ITEM36")
            ret.AppendLine(",ia.FREE_ITEM37")
            ret.AppendLine(",ia.FREE_ITEM38")
            ret.AppendLine(",ia.FREE_ITEM39")
            ret.AppendLine(",ia.FREE_ITEM40")
            ret.AppendLine(",ia.FREE_ITEM41")
            ret.AppendLine(",ia.FREE_ITEM42")
            ret.AppendLine(",ia.FREE_ITEM43")
            ret.AppendLine(",ia.FREE_ITEM44")
            ret.AppendLine(",ia.FREE_ITEM45")
            ret.AppendLine(",ia.FREE_ITEM46")
            ret.AppendLine(",ia.FREE_ITEM47")
            ret.AppendLine(",ia.FREE_ITEM48")
            ret.AppendLine(",ia.FREE_ITEM49")
            ret.AppendLine(",ia.FREE_ITEM50")
            ret.AppendLine(",ia.FREE_ITEM51")
            ret.AppendLine(",ia.FREE_ITEM52")
            ret.AppendLine(",ia.FREE_ITEM53")
            ret.AppendLine(",ia.FREE_ITEM54")
            ret.AppendLine(",ia.FREE_ITEM55")
            ret.AppendLine(",ia.FREE_ITEM56")
            ret.AppendLine(",ia.FREE_ITEM57")
            ret.AppendLine(",ia.FREE_ITEM58")
            ret.AppendLine(",ia.FREE_ITEM59")
            ret.AppendLine(",ia.FREE_ITEM60")
            ret.AppendLine(",ia.OPE_STATUS")
            ret.AppendLine(",ia.TAG_LINK_STATUS ")
            ret.AppendLine(",ope.DATA2 AS OPE_STATUS_NAME")
            ret.AppendLine(",ia.DIVERSION_REMOVE")
            ret.AppendLine(",ia.REMOVE_DATE")
            ret.AppendLine(",ia.REMOVE_REASON")
            ret.AppendLine(",rr.DATA2 AS REMOVE_REASON_NAME")
            ret.AppendLine(",ia.REMOVE_CONTENT")
            ret.AppendLine(",ia.CHANGE_LOC_M_CD")
            ret.AppendLine(",clm.LOC_NAME AS CHANGE_LOC_M_NAME")
            ret.AppendLine(",ia.CHANGE_LOC_S_CD")
            ret.AppendLine(",cls.LOC_NAME AS CHANGE_LOC_S_NAME")
            ret.AppendLine(",ia.CHANGE_EMPLOYEE")
            ret.AppendLine(",ce.EMPLOYEE_NAME AS CHANGE_EMPLOYEE_NAME")
            ret.AppendLine(",ia.REMARK1")
            ret.AppendLine(",ia.REMARK2")
            ret.AppendLine(",ia.REMARK3")
            ret.AppendLine(",ia.REMARK4")
            ret.AppendLine(",ia.REMARK5")
            ret.AppendLine(",ia.REMARK6")
            ret.AppendLine(",ia.REMARK7")
            ret.AppendLine(",ia.REMARK8")
            ret.AppendLine(",ia.REMARK9")
            ret.AppendLine(",ia.REMARK10")
            '[CC431] ADD Start 20261002
            ' 棚卸実施端末、入力作業者コード／名（名称は社員M）
            ret.AppendLine(",ia.PHYINV_TOOL_DIV")
            ret.AppendLine(",ia.ENT_EMPLOYEE_CD")
            ret.AppendLine(",ee.EMPLOYEE_NAME AS ENT_EMPLOYEE_NAME")
            '[CC431] ADD End 20261002

            ret.AppendLine(",AUTH.UPD_FLG")       '--2021/11/22 add nv iwamoto
            ret.AppendLine(",totalCount AS TotalCount ")

            ret.AppendLine("FROM T_INV_ASSET ia")
            ret.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AUTH")
            ret.AppendLine("ON  AUTH.LOC_L_CD = ia.LOC_L_CD")
            ret.AppendLine("AND ((AUTH.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND AUTH.LOC_M_CD = ia.LOC_M_CD) OR (AUTH.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")

            addSqlParam("EmployeeCd", $"{_LoginInfo.EmployeeCd}")
            addSqlParam("ReadAuthFlg", $"{authInfo.ReadAuthFlg}")
            addSqlParam("UpdAuthFlg", $"{authInfo.UpdAuthFlg}")
            addSqlParam("ReadHubAuthFlg", $"{authInfo.ReadHubAuthFlg}")
            addSqlParam("UpdHubAuthFlg", $"{authInfo.UpdHubAuthFlg}")
            addSqlParam("UpdAuthOption", $"{authInfo.AuthOption}")

            ret.AppendLine(" LEFT JOIN M_EMPLOYEE me1 ON me1.EMPLOYEE_CD = ia.FREE_ITEM6 ")
            ret.AppendLine(" LEFT JOIN M_EMPLOYEE ce ON ce.EMPLOYEE_CD = ia.CHANGE_EMPLOYEE ")
            ret.AppendLine(" LEFT JOIN M_EMPLOYEE le ON le.EMPLOYEE_CD = ia.LOCK_EMPLOYEE_CD ")
            ret.AppendLine(" LEFT JOIN M_SETTING ope ON ope.KEY_DIV = 'Ope_Status_Item' AND ope.KEY1 = ia.OPE_STATUS ")
            ret.AppendLine(" LEFT JOIN M_SETTING rr ON rr.KEY_DIV = 'Remove_Reason_Item' AND rr.KEY1 = ia.REMOVE_REASON ")
            ret.AppendLine(" LEFT JOIN M_LOCATION clm ON clm.LOC_DIV = 'M' AND clm.LOC_L_CD = ia.LOC_L_CD AND clm.LOC_M_CD = ia.CHANGE_LOC_M_CD ")
            '[CC431] UPD Start 20261002
            ' 新変更後設置場所名称：変更後課コードが空の場合は、ロケ中区分コード配下の場所で名称を取得する
            ret.AppendLine(" LEFT JOIN M_LOCATION cls ON cls.LOC_DIV = 'S' AND cls.LOC_L_CD = ia.LOC_L_CD AND cls.LOC_M_CD = COALESCE(NULLIF(ia.CHANGE_LOC_M_CD, ''), ia.LOC_M_CD) AND cls.LOC_S_CD = ia.CHANGE_LOC_S_CD")
            '[CC431] UPD End 20261002
            '[CC431] ADD Start 20261002
            ' 入力作業者名
            ret.AppendLine(" LEFT JOIN M_EMPLOYEE ee ON ee.EMPLOYEE_CD = ia.ENT_EMPLOYEE_CD ")
            '[CC431] ADD End 20261002

            ret.AppendLine("INNER JOIN (")
            ret.AppendLine("  SELECT COUNT(*) AS totalCount")
            ret.AppendLine("  FROM T_INV_ASSET ia")
            ret.AppendLine("INNER JOIN dbo.AUTH_LOCATION_ULOC(@EmployeeCd, @ReadAuthFlg, @UpdAuthFlg, @ReadHubAuthFlg, @UpdHubAuthFlg, @UpdAuthOption) AUTH")
            ret.AppendLine("ON  AUTH.LOC_L_CD = ia.LOC_L_CD")
            ret.AppendLine("AND ((AUTH.LOC_M_FLG='" & Misc.AUTH_LOC_M_FLG.Yes & "' AND AUTH.LOC_M_CD = ia.LOC_M_CD) OR (AUTH.LOC_M_FLG = '" & Misc.AUTH_LOC_M_FLG.No & "')) ")
            ret.AppendLine(" " & whereSql)
            ret.AppendLine(") C")
            ret.AppendLine("ON  1 = 1")

            'ロケーション
            ret.AppendLine("LEFT JOIN (")
            ret.AppendLine("  SELECT LOC_NAME,LOC_L_CD")
            ret.AppendLine("  FROM M_LOCATION")
            ret.AppendLine("  WHERE LOC_DIV = '" & LOC_DIV.LocationL & "'")
            ret.AppendLine(") ml")
            ret.AppendLine("ON  ml.LOC_L_CD = ia.LOC_L_CD")
            ret.AppendLine("LEFT JOIN (")
            ret.AppendLine("  SELECT LOC_NAME,LOC_L_CD,LOC_M_CD")
            ret.AppendLine("  FROM M_LOCATION")
            ret.AppendLine("  WHERE LOC_DIV = '" & LOC_DIV.LocationM & "'")
            ret.AppendLine(") mm")
            ret.AppendLine("ON  mm.LOC_L_CD = ia.LOC_L_CD")
            ret.AppendLine("AND mm.LOC_M_CD = ia.LOC_M_CD")
            ret.AppendLine("LEFT JOIN (")
            ret.AppendLine("  SELECT LOC_NAME,LOC_L_CD,LOC_M_CD,LOC_S_CD")
            ret.AppendLine("  FROM M_LOCATION")
            ret.AppendLine("  WHERE LOC_DIV = '" & LOC_DIV.LocationS & "'")
            ret.AppendLine(") ms")
            ret.AppendLine("ON  ms.LOC_L_CD = ia.LOC_L_CD")
            ret.AppendLine("AND ms.LOC_M_CD = ia.LOC_M_CD")
            ret.AppendLine("AND ms.LOC_S_CD = ia.LOC_S_CD")

            'カテゴリ
            ret.AppendLine("LEFT JOIN M_CATEGORY mc")
            ret.AppendLine("  ON mc.CAT_DIV = 'L' AND ia.CAT_L_CD = mc.CAT_L_CD")
            ret.AppendLine(whereSql)

            Return ret.ToString()
        End Function


        ''' <summary>
        ''' ページ情報のモデルをコピーします
        ''' </summary>
        Private Sub CopyModelData()

            '一覧表示のデータが0件の場合、モデルを作成
            If _viewResult.Count = 0 Then
                _viewResult.Add(New InventoryInquiryViewModel())
            End If

            '先頭にコピー
            _viewResult.ElementAt(0).Pager.PageNumber = _ViewModel.PageNum
            _viewResult.ElementAt(0).Pager.TotalCount = _viewResult.ElementAt(0).TotalCount
            _viewResult.ElementAt(0).SearchLocL = _ViewModel.SearchLocL
            _viewResult.ElementAt(0).SearchLocM = _ViewModel.SearchLocM
            _viewResult.ElementAt(0).SearchLocS = _ViewModel.SearchLocS
            _viewResult.ElementAt(0).SearchCatL = _ViewModel.SearchCatL
            _viewResult.ElementAt(0).SearchPhyinvStatusAll = _ViewModel.SearchPhyinvStatusAll
            _viewResult.ElementAt(0).SearchPhyinvStatusN = _ViewModel.SearchPhyinvStatusN
            _viewResult.ElementAt(0).SearchPhyinvStatusW = _ViewModel.SearchPhyinvStatusW
            _viewResult.ElementAt(0).SearchPhyinvStatusE = _ViewModel.SearchPhyinvStatusE
            _viewResult.ElementAt(0).SearchAssetNo = _ViewModel.SearchAssetNo
            _viewResult.ElementAt(0).SearchAssetName = _ViewModel.SearchAssetName
            _viewResult.ElementAt(0).SearchMaker = _ViewModel.SearchMaker
            _viewResult.ElementAt(0).SearchProductNo = _ViewModel.SearchProductNo
            _viewResult.ElementAt(0).SearchModelNo = _ViewModel.SearchModelNo
            _viewResult.ElementAt(0).SearchTangibleCheck = _ViewModel.SearchTangibleCheck
            _viewResult.ElementAt(0).SearchNotTangibleCheck = _ViewModel.SearchNotTangibleCheck
            _viewResult.ElementAt(0).SearchAssetAdmin = _ViewModel.SearchAssetAdmin

            _viewResult.ElementAt(0).SearchLockEmployee = _ViewModel.SearchLockEmployee
            _viewResult.ElementAt(0).SearchDiversionRemove = _ViewModel.SearchDiversionRemove
            _viewResult.ElementAt(0).SearchOpeStatus = _ViewModel.SearchOpeStatus
            _viewResult.ElementAt(0).SearchRemoveDate = _ViewModel.SearchRemoveDate
            _viewResult.ElementAt(0).SearchRemoveReason = _ViewModel.SearchRemoveReason
            _viewResult.ElementAt(0).SearchRemoveContent = _ViewModel.SearchRemoveContent

            _viewResult.ElementAt(0).SearchChangeLocMCd = _ViewModel.SearchChangeLocMCd
            _viewResult.ElementAt(0).SearchChangeLocSCd = _ViewModel.SearchChangeLocSCd
            '[CC431] ADD Start 20261007
            _viewResult.ElementAt(0).SearchLocReverse = _ViewModel.SearchLocReverse
            _viewResult.ElementAt(0).SearchChangeLocReverse = _ViewModel.SearchChangeLocReverse
            '[CC431] ADD End 20261007
            _viewResult.ElementAt(0).SearchChangeEmployee = _ViewModel.SearchChangeEmployee

            '[CC431] UPD Start 20261002
            _viewResult.ElementAt(0).SearchFreeItem2From = _ViewModel.SearchFreeItem2From
            _viewResult.ElementAt(0).SearchFreeItem2To = _ViewModel.SearchFreeItem2To
            '[CC431] UPD End 20261002
            _viewResult.ElementAt(0).SearchFreeItem4 = _ViewModel.SearchFreeItem4
            _viewResult.ElementAt(0).SearchFreeItem5 = _ViewModel.SearchFreeItem5
            _viewResult.ElementAt(0).SearchFreeItem6 = _ViewModel.SearchFreeItem6
            _viewResult.ElementAt(0).SearchFreeItem7 = _ViewModel.SearchFreeItem7
            _viewResult.ElementAt(0).SearchFreeItem8 = _ViewModel.SearchFreeItem8
            _viewResult.ElementAt(0).SearchFreeItem9 = _ViewModel.SearchFreeItem9
            _viewResult.ElementAt(0).SearchFreeItem10 = _ViewModel.SearchFreeItem10
            _viewResult.ElementAt(0).SearchFreeItem11From = _ViewModel.SearchFreeItem11From
            _viewResult.ElementAt(0).SearchFreeItem11To = _ViewModel.SearchFreeItem11To
            _viewResult.ElementAt(0).SearchFreeItem12From = _ViewModel.SearchFreeItem12From
            _viewResult.ElementAt(0).SearchFreeItem12To = _ViewModel.SearchFreeItem12To
            _viewResult.ElementAt(0).SearchFreeItem14 = _ViewModel.SearchFreeItem14
            _viewResult.ElementAt(0).SearchFreeItem15 = _ViewModel.SearchFreeItem15
            _viewResult.ElementAt(0).SearchFreeItem16 = _ViewModel.SearchFreeItem16
            _viewResult.ElementAt(0).SearchFreeItem17 = _ViewModel.SearchFreeItem17
            '[CC431] UPD Start 20261002
            _viewResult.ElementAt(0).SearchFreeItem18From = _ViewModel.SearchFreeItem18From
            _viewResult.ElementAt(0).SearchFreeItem18To = _ViewModel.SearchFreeItem18To
            _viewResult.ElementAt(0).SearchFreeItem19From = _ViewModel.SearchFreeItem19From
            _viewResult.ElementAt(0).SearchFreeItem19To = _ViewModel.SearchFreeItem19To
            '[CC431] UPD End 20261002
            _viewResult.ElementAt(0).SearchFreeItem20From = _ViewModel.SearchFreeItem20From
            _viewResult.ElementAt(0).SearchFreeItem20To = _ViewModel.SearchFreeItem20To
            _viewResult.ElementAt(0).SearchFreeItem21 = _ViewModel.SearchFreeItem21
            _viewResult.ElementAt(0).SearchFreeItem37 = _ViewModel.SearchFreeItem37
            _viewResult.ElementAt(0).SearchFreeItem39 = _ViewModel.SearchFreeItem39
            _viewResult.ElementAt(0).SearchFreeItem42 = _ViewModel.SearchFreeItem42
            _viewResult.ElementAt(0).SearchTagLinkStatus = CommonRfid.ToStringNvl(_ViewModel.SearchTagLinkStatus)
            '[CC431] ADD Start 20261002
            _viewResult.ElementAt(0).SearchPhyinvDateFrom = _ViewModel.SearchPhyinvDateFrom
            _viewResult.ElementAt(0).SearchPhyinvDateTo = _ViewModel.SearchPhyinvDateTo
            _viewResult.ElementAt(0).SearchPhyinvEmployee = _ViewModel.SearchPhyinvEmployee
            _viewResult.ElementAt(0).SearchPhyinvToolDiv = _ViewModel.SearchPhyinvToolDiv
            _viewResult.ElementAt(0).SearchLockStatus = _ViewModel.SearchLockStatus
            _viewResult.ElementAt(0).SearchFreeItem27 = _ViewModel.SearchFreeItem27
            _viewResult.ElementAt(0).SearchEntEmployee = _ViewModel.SearchEntEmployee
            _viewResult.ElementAt(0).SearchSortOrder = _ViewModel.SearchSortOrder
            _viewResult.ElementAt(0).SearchRemark1 = _ViewModel.SearchRemark1
            _viewResult.ElementAt(0).SearchRemark2 = _ViewModel.SearchRemark2
            _viewResult.ElementAt(0).SearchRemark3 = _ViewModel.SearchRemark3
            _viewResult.ElementAt(0).SearchRemark4 = _ViewModel.SearchRemark4
            _viewResult.ElementAt(0).SearchRemark5 = _ViewModel.SearchRemark5
            _viewResult.ElementAt(0).SearchRemark6 = _ViewModel.SearchRemark6
            _viewResult.ElementAt(0).SearchRemark7 = _ViewModel.SearchRemark7
            _viewResult.ElementAt(0).SearchRemark8 = _ViewModel.SearchRemark8
            _viewResult.ElementAt(0).SearchRemark9 = _ViewModel.SearchRemark9
            _viewResult.ElementAt(0).SearchRemark10 = _ViewModel.SearchRemark10
            _viewResult.ElementAt(0).DetailOpenFlg = _ViewModel.DetailOpenFlg
            '[CC431] ADD End 20261002
            '[CC431] ADD Start 20261005
            _viewResult.ElementAt(0).OpenFlg = _ViewModel.OpenFlg
            '[CC431] ADD End 20261005

            _viewResult.ElementAt(0).CsvPath = _ViewModel.CsvPath
            _viewResult.ElementAt(0).ErrCode = _ViewModel.ErrCode
            _viewResult.ElementAt(0).ErrTarget = _ViewModel.ErrTarget
            _viewResult.ElementAt(0).PageNum = _ViewModel.PageNum
            _viewResult.ElementAt(0).OnCreteButton = _ViewModel.OnCreteButton
            _viewResult.ElementAt(0).States = _ViewModel.States '--2019/10/15 nv miyaji add

            _viewResult.ElementAt(0).LocationMList = _ViewModel.LocationMList
            _viewResult.ElementAt(0).LocationSList = _ViewModel.LocationSList
            _viewResult.ElementAt(0).EmployeeList = _ViewModel.EmployeeList

            If String.IsNullOrEmpty(_ViewModel.SearchLocL) Then
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_INQUIRY, "TEMPLATE")
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_INQUIRY, "TEMPLATE").FIX_COL_CNT
            Else
                _viewResult.ElementAt(0).GridInfo = ToolColSet.GetGridInfo(_db, DispId.INVENTORY_INQUIRY, _ViewModel.SearchLocL)
                _viewResult.ElementAt(0).FixColCnt = _db.ColSet.Find(DispId.INVENTORY_INQUIRY, _ViewModel.SearchLocL).FIX_COL_CNT
            End If
        End Sub

#End Region

    End Class
End Namespace