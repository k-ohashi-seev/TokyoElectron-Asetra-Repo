Namespace Misc
    ''' <summary>
    ''' 定数クラス
    ''' </summary>
    Public Class CommonConst

        ''' <summary>
        ''' アプリケーションキー(メッセージ・設定)
        ''' </summary>
        Public Const KEY_APP_LANGUAGE As String = "KEY_APP_LANGUAGE"

        '2019/08/25 MessageRfid に組み込む（メッセージ取得時に設定情報の読込が必要なため）
        '''' <summary>
        '''' アプリケーションキー(設定)
        '''' </summary>
        'Public Const KEY_APP_SETTING As String = "KEY_APP_SETTING"

        ''' <summary>
        ''' 検索ボタン押下
        ''' </summary>
        Public Const KEY_SEARCH_CLICK As String = "Search"

        ''' <summary>
        ''' DB接続文字列
        ''' </summary>
        Public Const KEY_DB_CONNECTION As String = "DefaultConnection"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const KEY_ON_CREATE_CLICK As String = "Create"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const KEY_ON_UPDATE_CLICK As String = "Update"

        ''' <summary>
        ''' ページ表示用のタイトル
        ''' </summary>
        Public Const KEY_PAGE_TITLE As String = "TITLE"

        ''' <summary>
        ''' APPルートフォルダ
        ''' </summary>
        Public Const ROOT_FOLDERNM As String = "~/"

        ''' <summary>
        ''' CSVダウンロードフォルダ
        ''' </summary>
        Public Const CSVEXPORT_FOLDERNM As String = ROOT_FOLDERNM & "App_Data/CsvExport"

        ''' <summary>
        ''' CSVアップロードフォルダ
        ''' </summary>
        Public Const CSVIMPORT_FOLDERNM As String = ROOT_FOLDERNM & "App_Data/CsvImport"

        ''' <summary>
        ''' QR画像保存フォルダ
        ''' </summary>
        Public Const QRIMAGE_FOLDERNM As String = ROOT_FOLDERNM & "QRimages"

        ''' <summary>
        ''' イメージ画像一時保存フォルダ
        ''' </summary>
        Public Const IMAGETMP_FOLDERNM As String = "ImageTmp"

        ''' <summary>
        ''' QR作成EXE パス
        ''' </summary>
        Public Const QREXE＿PATH As String = "\EXE\ExecQRMaker.exe"

        ''' <summary>
        ''' ロケーション大区分変換文字列
        ''' </summary>
        Public Const WORD_LOC_L_CD As String = "%%LOC_L_CD%%"

        ''' <summary>
        ''' ロケーション中区分変換文字列
        ''' </summary>
        Public Const WORD_LOC_M_CD As String = "%%LOC_M_CD%%"

        ''' <summary>
        ''' ロケーション小区分変換文字列
        ''' </summary>
        Public Const WORD_LOC_S_CD As String = "%%LOC_S_CD%%"


        ''' <summary>
        ''' ログイン画面呼出用（ログイン）
        ''' </summary>
        Public Const KEY_FROM_LOGIN As String = "Login"

        ''' <summary>
        ''' ログイン画面呼出用（ログアウト）
        ''' </summary>
        Public Const KEY_FROM_LOGOUT As String = "Logout"

        ''' <summary>
        ''' 区切り文字
        ''' </summary>
        Public Const CodeNameDelimiter As String = " "

        ''' <summary>
        ''' 資産ID採番時の識別コード
        ''' </summary>
        Public Const ASSET_SEQNO_ID As String = "*AS"

        ''' <summary>
        ''' 資産ID採番時桁数
        ''' </summary>
        Public Const ASSET_SEQNO_KETA As Integer = 5

        ''' <summary>
        ''' QR台帳テンプレートフォルダ
        ''' </summary>
        Public Const TEMPLATE_FOLDERNM As String = ROOT_FOLDERNM & "Template"

        ''' <summary>
        ''' QR台帳テンプレート名(標準)
        ''' </summary>
        Public Const DAICHO_TEMPLATE_FILENM As String = "QRDaicho.xlsx"

        ''' <summary>
        ''' QR台帳テンプレート名(カスタム1)  2023/10/17 Sato ADD v2.5
        ''' </summary>
        Public Const DAICHO_CUSTOM1_TEMPLATE_FILENM As String = "QRDaicho_Custom1.xlsx"

        ''' <summary>
        ''' INVENTRY_RESULTテンプレート名
        ''' </summary>
        Public Const INVENTRY_RESULT_FILENM As String = "INVENTRY_RESULT.xlsx"


        ''' <summary>
        ''' 予約確認書テンプレート名
        ''' </summary>
        Public Const LEASECONFIRM_TEMPLATE_FILENM As String = "LEASE_PRINT.xlsx"

        ''' <summary>
        ''' 指定数量管理表テンプレート名  20230430 ADD Sato v2.4
        ''' </summary>
        Public Const DRUG_SPEC_LIST_TEMPLATE_FILENM As String = "DRUG_SPEC_LIST.xlsx"

        ''' <summary>
        ''' 管理簿テンプレート名  20230430 ADD Sato v2.4
        ''' </summary>
        Public Const DRUG_MANAGE_LIST_TEMPLATE_FILENM As String = "DRUG_MANAGE_LIST.xlsx"

        ''' <summary>
        ''' 工事帳票テンプレート名
        ''' </summary>
        Public Const CONST_PRINT_TEMPLATE_FILENM As String = "CONST_PRINT.xlsx"

        ''' <summary>
        ''' 設定マスタ 一覧表示件数上限値
        ''' </summary>
        Public Const SettingListMaxRowCount As Integer = 10000

        ''' <summary>
        ''' 操作ログマスタ 処理内容項目長
        ''' </summary>
        Public Const OpeLogContentsLen As Integer = 255

        ''' <summary>
        ''' QR台帳テンプレートフォルダ
        ''' </summary>
        Public Const LOG_FOLDERNM As String = ROOT_FOLDERNM & "App_Data/log4net"

        ''' <summary>
        ''' CSV取込時、エラー出力最大件数
        ''' </summary>
        Public Const CSV_ERROR_MAXCOUNT As Integer = 1000

        ''' <summary>
        ''' 商品マスタ内容反映時、更新しない事を示す文字列
        ''' </summary>
        Public Const NOT_UPD_STR As String = "@@@@@@@@@@"

        ''' <summary>
        ''' 商品マスタ内容反映時、更新しない事を示す数字列
        ''' </summary>
        Public Const NOT_UPD_INT As Integer = -2147483648

        ''' <summary>
        ''' 資産メンテナンス登録情報取得関数で、返還される最大期限日
        ''' </summary>
        Public Const MAX_KIGENBI As Integer = 29991231

        ''' <summary>
        ''' 資産メンテナンス登録情報取得関数で、期限日「29991231」が帰ってきたときに表示する文字列
        ''' </summary>
        Public Const MAX_KIGENBI_STR As String = "-"

        ''' <summary>
        ''' 社員所属拠点登録最大数
        ''' </summary>
        Public Const MAX_HUBLOC_CNT As String = "50"

        ''' <summary>
        ''' 有形/無形区分を判断する桁
        ''' </summary>
        Public Const IS_TANGIBLE_DIGIT As Integer = 5

        ''' <summary>
        ''' 無形資産判定文字
        ''' </summary>
        Public Const NOT_TANGIBLE_CHAR As String = "2"

        ''' <summary>
        ''' 検索条件で空白を指定するために表示する文字列
        ''' </summary>
        Public Const VALUE_BLANK As String = "-"

        '複数選択テキストボックスの区切り文字　※Scripts\multi-input-dialog.js と合わせる
        Public Const MULTI_INPUT_DELIMITER As Char = "|"c
    End Class

    ''' <summary>
    ''' 権限定義クラス
    ''' </summary>
    Public Class Auth
        Public Class MenuId
            ''' <summary>資産</summary>
            Public Shared Asset As String = "Asset"
            ''' <summary>工事</summary>
            Public Shared Construction As String = "Construction"
            ''' <summary>貸出</summary>
            Public Shared Lending As String = "Lending"
            ''' <summary>棚卸</summary>
            Public Shared Inventory As String = "Inventory"
            ''' <summary>マスタメンテ</summary>
            Public Shared Master As String = "Master"
            ''' <summary>ゲート</summary>
            Public Shared Gate As String = "Gate"
            ''' <summary>ログ照会</summary>
            Public Shared Operation As String = "Operation"
        End Class
        Public Class FunctionId
            ''' <summary>資産ラベル発行</summary>
            Public Shared AssetLabelPrint As String = "AssetLabelPrint"
            ''' <summary>資産一覧</summary>
            Public Shared AssetList As String = "AssetList"
            ''' <summary>資産在庫照会</summary>
            Public Shared StockList As String = "StockList"
            ''' <summary>資産履歴照会</summary>
            Public Shared AssetHistory As String = "AssetHistory"
            '2023/03/22 Kawamoto ADD START
            ''' <summary>指定数量管理表出力</summary>
            Public Shared DrugSpecList As String = "DrugSpecList"
            ''' <summary>管理簿出力</summary>
            Public Shared DrugManageList As String = "DrugManageList"
            '2023/03/22 Kawamoto ADD END

            ''' <summary>工事一覧</summary>
            Public Shared ConstructionList As String = "ConstructionList"
            ''' <summary>工事資産一覧</summary>
            Public Shared ConstAssetList As String = "ConstAssetList"
            ''' <summary>工事履歴一覧</summary>
            Public Shared ConstructionHist As String = "ConstructionHist"
            ''' <summary>工事資産履歴一覧</summary>
            Public Shared ConstAssetHist As String = "ConstAssetHist"
            ''' <summary>仮置一覧</summary>
            Public Shared TemporaryList As String = "TemporaryList"
            ''' <summary>仮置資産一覧</summary>
            Public Shared TempAssetList As String = "TempAssetList"

            ''' <summary>予約状況照会</summary>
            Public Shared LendingStatus As String = "LendingStatus"
            ''' <summary>予約一覧</summary>
            Public Shared LendingList As String = "LendingList"
            ''' <summary>貸出先一覧</summary>
            Public Shared AssetLeasePlaceList As String = "AssetLeasePlaceList"
            ''' <summary>貸出先資産照会</summary>
            Public Shared AssetLeasePlaceDetail As String = "AssetLeasePlaceDetail"
            ''' <summary>貸出履歴照会</summary>
            Public Shared AssetLeaseHitory As String = "AssetLeaseHitory"

            ''' <summary>棚卸進捗状況</summary>
            Public Shared InventorySummary As String = "InventorySummary"
            ''' <summary>棚卸実績一覧</summary>
            Public Shared InventoryHistory As String = "InventoryHistory"
            ''' <summary>実棚照会</summary>
            Public Shared InventoryInquiry As String = "InventoryInquiry"
            ''' <summary>棚卸実績照会</summary>
            Public Shared InventoryAssetHistory As String = "InventoryAssetHistory"
            '20231024 ADD Start Shirakawa
            ''' <summary>棚卸実績一覧</summary>
            Public Shared InventoryLocHistory As String = "InventoryLocHistory"
            '20231024 ADD End Shirakawa

            ''' <summary>カテゴリーマスタメンテ</summary>
            Public Shared Category As String = "Category"
            ''' <summary>ロケーションマスタメンテ</summary>
            Public Shared Location As String = "Location"
            ''' <summary>部署マスタメンテ</summary>
            Public Shared Department As String = "Department"
            ''' <summary>社員マスタメンテ</summary>
            Public Shared Employee As String = "Employee"
            ''' <summary>貸出先マスタメンテ</summary>
            Public Shared LeasePlace As String = "LeasePlace"
            ''' <summary>設定マスタメンテ</summary>
            Public Shared Setting As String = "Setting"
            ''' <summary>文言マスタメンテ</summary>
            Public Shared Word As String = "Word"
            ''' <summary>CSVマッピング</summary>
            Public Shared CsvMapping As String = "CsvMapping"
            ''' <summary>カレンダーマスタメンテ</summary>
            Public Shared Calender As String = "Calender"
            ''' <summary>商品マスタメンテ</summary>
            Public Shared Goods As String = "Goods"
            ''' <summary>項目配置マスタメンテ</summary>
            Public Shared ColSet As String = "ColSet"

            ''' <summary>ゲートマスタメンテ</summary>
            Public Shared Gate As String = "Gate"
            ''' <summary>ゲート持ち出し履歴</summary>
            Public Shared GatePassLog As String = "GatePassLog"

            ''' <summary>ログ照会</summary>
            Public Shared OperationLog As String = "OperationLog"

        End Class
    End Class

    ''' <summary>
    ''' AUTH_LOCATION_SF DB関数 共有拠点更新権限対象 選択肢　定義クラス
    ''' </summary>
    Public Class UpdAuthType
        Public Shared UPD_AUTH As String = "UPD_AUTH"
        Public Shared UPD_AUTH_HUB As String = "UPD_AUTH_HUB"
    End Class

    ''' <summary>
    ''' 権限の特殊設定名
    ''' </summary>
    Public Class AuthOptionName
        Public Shared U_SELF As String = "U_SELF"
        Public Shared U_HUB As String = "U_HUB"
        Public Shared U_HUB_AUTH As String = "U_HUB_AUTH"
        Public Shared U_ADMIN As String = "U_ADMIN"
        Public Shared U_EMPHUB As String = "U_EMPHUB"
        Public Shared U_CHGALL As String = "U_CHGALL"
        Public Shared U_CHGHUB As String = "U_CHGHUB"
        Public Shared U_MAINTE As String = "U_MAINTE"
        Public Shared U_MAINTEHUB As String = "U_MAINTEHUB"
        Public Shared U_LOC_M As String = "U_LOC_M"
        Public Shared U_LOCK_CANCEL As String = "U_LOCK_CANCEL"
        Public Shared U_NOEDIT_DEP As String = "U_NOEDIT_DEP"
    End Class

    ''' <summary>
    ''' 部署コード固定値
    ''' </summary>
    Public Class DepartmentCD
        Public Shared DEP_HEAD As String = "001"
        Public Shared DEP_HUB As String = "002"
    End Class


    Public Class CommonSessionKey

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const SYSTEM_ERR_MESSAGE As String = "SYSTEM_ERR_MESSAGE"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const SYSTEM_ERR_DATE As String = "SYSTEM_ERR_DATE"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_MEMBER As String = "CREAGE_MEMBER"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_EMPLOYEE As String = "CREAGE_EMPLOYEE"

        ''' <summary>
        ''' 社員情報(ログイン画面で使用)
        ''' </summary>
        Public Const CREAGE_EMPLOYEE_INFO As String = "CREAGE_EMPLOYEE_INFO"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_SETTING As String = "CREAGE_SETTING"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_INVENTORY_INQUIRY As String = "CREAGE_INVENTORY_INQUIRY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_DEPARTMENT As String = "CREAGE_DEPARTMENT"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_LEASE_PLACE As String = "CREAGE_LEASE_PLACE"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_LOCATION As String = "CREAGE_LOCATION"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_CATEGORY As String = "CREAGE_CATEGORY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_INVENTORY_SUMMARY As String = "CREAGE_INVENTORY_SUMMARY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_INVENTORY_HISTORY As String = "CREAGE_INVENTORY_HISTORY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_OPERATION_LOG As String = "CREAGE_OPERATION_LOG"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_STOCK_LIST As String = "CREAGE_STOCK_LIST"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_LOGIN As String = "CREAGE_LOGIN"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_ASSET_LIST As String = "CREAGE_ASSET_LIST"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const SEARCH_FLG As String = "SEARCH_FLG"

        ''' <summary>
        ''' 資産一覧画面表示時のキー
        ''' </summary>
        Public Const CREAGE_ASSET_LIST_KEY As String = "CREAGE_ASSET_LIST_KET"

        ''' <summary>
        ''' 資産ラベル発行画面表示時のキー
        ''' </summary>
        Public Const CREAGE_ASSET_LABEL_PRINT As String = "CREAGE_ASSET_LABEL_PRINT"

        ''' <summary>
        ''' 資産ラベル発行ラベル発行CSV用のキー
        ''' </summary>
        Public Const CREAGE_ASSET_LABEL_PRINT_FOR_LABEL As String = "CREAGE_ASSET_LABEL_PRINT_FOR_LABEL"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_ASSET_HISTORY As String = "CREAGE_ASSET_HISTORY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_ASSET_DETAIL As String = "CREAGE_ASSET_DETAIL"

        ''' <summary>
        ''' 履歴照会(資産)画面表示時のキー
        ''' </summary>
        Public Const CREAGE_ASSET_DETAIL_KEY As String = "CREAGE_ASSET_DETAIL_KEY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_CONSTRUCTION_LIST As String = "CREAGE_CONSTRUCTION_LIST"

        ''' <summary>
        ''' 工事登録状態
        ''' </summary>
        Public Const CREAGE_CONSTRUCTION_EDIT As String = "CREAGE_CONSTRUCTION_EDIT"

        ''' <summary>
        ''' 工事一覧の検索条件
        ''' </summary>
        Public Const CREAGE_CONSTRUCTION_LIST_SEARCH_KEY As String = "CREAGE_CONSTRUCTION_LIST_SEARCH_KEY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_CONST_ASSET_LIST As String = "CREAGE_CONST_ASSET_LIST"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_CONSTRUCTION_HIST As String = "CREAGE_CONSTRUCTION_HIST"

        ''' <summary>
        ''' 工事履歴一覧の検索条件
        ''' </summary>
        Public Const CREAGE_CONSTRUCTION_HIST_SEARCH_KEY As String = "CREAGE_CONSTRUCTION_HIST_SEARCH_KEY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_CONST_ASSET_HIST As String = "CREAGE_CONST_ASSET_HIST"

        ''' <summary>
        ''' 工事資産履歴一覧画面表示時のキー
        ''' </summary>
        Public Const CREAGE_CONST_ASSET_HIST_KEY As String = "CREAGE_CONST_ASSET_HIST_KEY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_TEMPORARY_LIST As String = "CREAGE_TEMPORARY_LIST"

        ''' <summary>
        ''' 仮置一覧の検索条件
        ''' </summary>
        Public Const CREAGE_TEMPORARY_LIST_SEARCH_KEY As String = "CREAGE_TEMPORARY_LIST_SEARCH_KEY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_TEMP_ASSET_LIST As String = "CREAGE_TEMP_ASSET_LIST"

        ''' <summary>
        ''' 仮置資産一覧画面表示時のキー
        ''' </summary>
        Public Const CREAGE_TEMP_ASSET_LIST_KEY As String = "CREAGE_TEMP_ASSET_LIST_KEY"

        ''' <summary>
        ''' 実棚照会画面表示時のキー
        ''' </summary>
        Public Const CREAGE_INVENTORY_DETAIL_KEY As String = "CREAGE_INVENTORY_DETAIL_KEY"

        ''' <summary>
        ''' 貸出先資産照会画面表示時のキー
        ''' </summary>
        Public Const CREAGE_ASSET_LEASEPLACE_DETAIL_KEY As String = "CREAGE_ASSET_LEASEPLACE_DETAIL_KEY"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_WORD As String = "CREAGE_WORD"

        ''' <summary>
        ''' ゲートマスタ　データ登録用　'2020/02/18 nv miyaji add
        ''' </summary>
        Public Const CREAGE_GATE As String = "CREAGE_GATE"

        ''' <summary>
        ''' 資産一覧画像イメージファイル名のキー（このキー値がない場合は、画像がない。もしくは削除された。）
        ''' </summary>
        Public Const ASSET_IMAGEFILE_KEY As String = "ASSET_IMAGEFILE_KEY"

        ''' <summary>
        ''' 資産一覧画面の検索条件保存用のキー
        ''' </summary>
        Public Const CREAGE_ASSET_SERCH_KEY As String = "CREAGE_ASSET_SERCH_KEY"

        ''' <summary>
        ''' 在庫照会画面の検索条件保存用のキー
        ''' </summary>
        Public Const CREAGE_STOCK_SERCH_KEY As String = "CREAGE_STOCK_SERCH_KEY"

        ''' <summary>
        ''' 貸出先一覧画面の検索条件保存用のキー
        ''' </summary>
        Public Const CREAGE_ASSET_LEASEPLACE_DETAIL_SERCH_KEY As String = "CREAGE_ASSET_LEASEPLACE_DETAIL_SERCH_KEY"

        ''' <summary>
        ''' 貸出資産照会画面　貸出先一覧画面からの遷移
        ''' </summary>
        Public Const CREAGE_ASSET_LEASEPLACE_DETAIL_FROM_LIST As String = "CREAGE_ASSET_LEASEPLACE_DETAIL_FROM_LIST"

        ''' <summary>
        ''' CSVマッピング画面
        ''' </summary>
        Public Const CREAGE_CSVMAPPING As String = "CREAGE_CSVMAPPING"

        ''' <summary>
        ''' CSVマッピング画面 No保存用
        ''' </summary>
        Public Const CREAGE_CSVMAPPING_DISPLAYDATA As String = "CREAGE_CSVMAPPING_DISPLAYDATA"

        ''' <summary>
        ''' プロダクトキー入力画面
        ''' </summary>
        Public Const CREAGE_ENTRYPRODUCTKEY As String = "CREAGE_ENTRYPRODUCTKEY"

        ''' <summary>
        ''' 指定数量管理表出力画面  20230430 ADD Sato
        ''' </summary>
        Public Const CREAGE_DRUGSPECLIST As String = "CREAGE_DRUGSPECLIST"

        ''' <summary>
        ''' 管理簿出力画面  20230430 ADD Sato
        ''' </summary>
        Public Const CREAGE_DRUGMANAGELIST As String = "CREAGE_DRUGMANAGELIST"

        ''' <summary>
        ''' 管理簿出力画面メッセージ  20230430 ADD Sato
        ''' </summary>
        Public Const CREAGE_DRUGMANAGELIST_MSG As String = "CREAGE_DRUGMANAGELIST_MSG"

        ''' <summary>
        ''' 配置項目マスタ画面
        ''' </summary>
        Public Const CREAGE_COLSET As String = "CREAGE_COLSET"

        ''' <summary>
        ''' 配置項目マスタ画面 No保存用 
        ''' </summary>
        Public Const CREAGE_COLSET_DISPLAYDATA As String = "CREAGE_COLSET_DISPLAYDATA"

        ''' <summary>
        ''' 通過コントローラー名のキー
        ''' </summary>
        Public Const CONTROLLER_NAME As String = "CONTROLLER_NAME"

        ''' <summary>
        ''' 選択されたキーを保存用
        ''' </summary>
        Public Const LIST_CHECKED_KEYS As String = "LIST_CHECKED_KEYS"

        ''' <summary>
        ''' 保存されていない編集内容
        ''' </summary>
        Public Const LIST_EDITED_CONTAINS As String = "LIST_EDITED_CONTAINS"

        ''' <summary>
        ''' 全選択ボタン押下フラグ状態保存用    '2020/02/12 nv miyaji
        ''' </summary>
        Public Const ALL_SELECT_FLG As String = "ALL_SELECT_FLG"

        ''' <summary>
        ''' 全選択ボタン押下フラグ状態保存用_2つ目
        ''' </summary>
        Public Const ALL_SELECT_FLG_SECOND As String = "ALL_SELECT_FLG_SECOND"

        ''' <summary>
        ''' 最新の一覧表示SQL
        ''' </summary>
        Public Const LIST_VIEW_SQL As String = "LIST_VIEW_SQL"

        ''' <summary>
        ''' 最新の一覧表示SQLパラメーター
        ''' </summary>
        Public Const LIST_VIEW_PARAM As String = "LIST_VIEW_PARAM"

        ''' <summary>
        ''' 資産一覧画面　最新の一覧表示SQL　Where文
        ''' </summary>
        Public Const LIST_VIEW_SQL_WHERE As String = "LIST_VIEW_SQL_WHERE"

        '--2019/12/24 miyaji update start
        ''' <summary>
        ''' ライセンス認証　認証OK
        ''' </summary>
        Public Const REGIST_OK As String = "REGIST_OK"

        ''' <summary>
        ''' ライセンス認証　プロダクト番号
        ''' </summary>
        Public Const REGIST_PRODUCT_NO As String = "REGIST_PRODUCT_NO"

        ''' <summary>
        ''' ライセンス認証　プロダクトキー
        ''' </summary>
        Public Const REGIST_PRODUCT_KEY As String = "REGIST_PRODUCT_KEY"

        ''' <summary>
        ''' ライセンス認証　試用期限
        ''' </summary>
        Public Const REGIST_TRIAL_LIMIT As String = "REGIST_TRIAL_LIMIT"
        '--2019/12/24 miyaji update end

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_CALENDAR As String = "CREAGE_CALENDAR"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_LENDING_STATUS As String = "CREAGE_LENDING_STATUS"

        ''' <summary>
        ''' 予約状況照会画面で別画面からリダイレクトされた際に復元するモデル保存用のキー
        ''' </summary>
        Public Const CREAGE_LENDING_STATUS_REDIRECT As String = "CREAGE_LENDING_STATUS_REDIRECT"

        ''' <summary>
        ''' カート情報のキー
        ''' </summary>
        Public Const CREAGE_CART_INFO As String = "CREAGE_CART_INFO"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_LENDING_ENTRY As String = "CREAGE_LENDING_ENTRY"

        ''' <summary>
        ''' 予約申請登録画面 グリッド情報
        ''' </summary>
        Public Const LENDING_ENTRY_GRID_INFO As String = "LENDING_ENTRY_GRID_INFO"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_LENDING_MAINTE_ENTRY As String = "CREAGE_LENDING_MAINTE_ENTRY"

        ''' <summary>
        ''' 予約申請登録/変更画面への引渡し情報
        ''' </summary>

        Public Const LENDING_ENTRY_INFO As String = "LENDING_ENTRY_INFO"

        ''' <summary>
        ''' メンテナンス登録/変更画面への引渡し情報
        ''' </summary>

        Public Const LENDING_MAINTE_ENTRY_INFO As String = "LENDING_MAINTE_ENTRY_INFO"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_LENDING_LIST As String = "CREAGE_LENDING_LIST"

        ''' <summary>
        ''' 予約一覧画面で別画面からリダイレクトされた際に復元するモデル保存用のキー
        ''' </summary>
        Public Const CREAGE_LENDING_LIST_REDIRECT As String = "CREAGE_LENDING_LIST_REDIRECT"

        ''' <summary>
        ''' 
        ''' </summary>
        Public Const CREAGE_LENDING_HISTORY As String = "CREAGE_LENDING_HISTORY"

        ''' <summary>
        ''' 予約履歴画面への引渡し情報
        ''' </summary>
        Public Const LENDING_HISTORY_INFO As String = "LENDING_HISTORY_INFO"

        ''' <summary>
        '''  商品マスタ
        ''' </summary>
        Public Const CREAGE_GOODS As String = "CREAGE_GOODS"

        '20231005 ADD Start Shirakawa
        ''' <summary>
        ''' 実棚照会画面表示時のキー
        ''' </summary>
        Public Const CREAGE_INVENTORY_INQUIRY_KEY As String = "CREAGE_INVENTORY_INQUIRY_KEY"
        '20231005 ADD End Shirakawa

        ''' <summary>
        ''' 棚卸進捗状況で使用　棚卸IDを保存
        ''' </summary>
        Public Const INV_ID As String = "INV_ID"

        ''' <summary>
        ''' 棚卸実績一覧、棚卸状況照会に戻った時に使用　棚卸ロケを保存
        ''' </summary>
        Public Const INV_RETURN_LOC As String = "INV_RETURN_LOC"

    End Class

    ''' <summary>
    ''' ステータス定義クラス
    ''' </summary>
    Public Class StatusType
        ''' <summary>登録正常終了</summary>
        Public Shared SuccessCreate As String = "SuccessCreate"
        ''' <summary>変更正常終了</summary>
        Public Shared SuccessEdit As String = "SuccessEdit"
        ''' <summary>削除正常終了</summary>
        Public Shared SuccessDelete As String = "SuccessDelete"
        ''' <summary>除却正常終了</summary>
        Public Shared SuccessRetire As String = "SuccessRetire"
        ''' <summary>除却解除正常終了</summary>
        Public Shared SuccessRetireOff As String = "SuccessRetireOff"
        ''' <summary>棚卸確定正常終了</summary>
        Public Shared SuccessInvKakutei As String = "SuccessInvKakutei"
        ''' <summary>棚卸確定エラー</summary>
        Public Shared ErrorInvKakutei As String = "ErrorInvKakutei"
        ''' <summary>発行正常終了</summary>
        Public Shared SuccessHakko As String = "SuccessHakko"
        ''' <summary>発行不要正常終了</summary>
        Public Shared SuccessNoHakko As String = "SuccessNoHakko"
        ''' <summary>貸出正常終了</summary>
        Public Shared SuccessLending As String = "SuccessLending"
        ''' <summary>返却正常終了</summary>
        Public Shared SuccessReturned As String = "SuccessReturned"
        ''' <summary>工事完了正常終了</summary>
        Public Shared SuccessComplete As String = "SuccessComplete"
        ''' <summary>仮置き追加正常終了</summary>
        Public Shared SuccessAddTemp As String = "SuccessAddTemp"
        ''' <summary>ロケ更新正常終了</summary>
        Public Shared SuccessLocEdit As String = "SuccessLocEdit"
    End Class

    ''' <summary>
    ''' 使用区分定義クラス
    ''' </summary>
    Public Class ItemUse
        Public Shared Yes As String = "1"
        Public Shared No As String = "0"
    End Class

    ''' <summary>
    ''' 権限区分定義クラス
    ''' </summary>
    Public Class AUTH_DIV
        Public Shared Nomal As String = "N"
        Public Shared Admin As String = "A"
        Public Shared SuperAdmin As String = "S"
    End Class

    ''' <summary>
    ''' 権限(あり／なし)定義クラス
    ''' </summary>
    Public Class AUTH_FLG
        Public Shared Yes As String = "Y"
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' ロケ中区分返却フラグ定義クラス(AUTH_LOCATION_ULOC)
    ''' </summary>
    Public Class AUTH_LOC_M_FLG
        Public Shared Yes As String = "Y"
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' ロケ区分定義クラス
    ''' </summary>
    Public Class LOC_DIV
        Public Shared LocationL As String = "L"
        Public Shared LocationM As String = "M"
        Public Shared LocationS As String = "S"
    End Class

    ''' <summary>
    ''' 分類区分定義クラス
    ''' </summary>
    Public Class CAT_DIV
        Public Shared CategoryL As String = "L"
        Public Shared CategoryM As String = "M"
        Public Shared CategoryS As String = "S"
    End Class

    ''' <summary>
    ''' リース期限アラート区分定義クラス
    ''' </summary>
    Public Class LeaseLimitAlertDiv
        ''' <summary>上位分類に準ずる</summary>
        Public Shared Front As String = "F"
        ''' <summary>アラート表示しない</summary>
        Public Shared No As String = "N"
        ''' <summary>アラート表示する</summary>
        Public Shared Yes As String = "Y"
    End Class

    ''' <summary>
    ''' ○○期限アラート区分定義クラス
    ''' </summary>
    Public Class GenericLimitAlertDiv
        ''' <summary>上位分類に準ずる</summary>
        Public Shared Front As String = "F"
        ''' <summary>アラート表示しない</summary>
        Public Shared No As String = "N"
        ''' <summary>アラート表示する</summary>
        Public Shared Yes As String = "Y"
    End Class

    ''' <summary>
    ''' 貸出アラート区分
    ''' </summary>
    Public Class LeaseAlertDiv
        ''' <summary>上位分類に準ずる</summary>
        Public Shared Front As String = "F"
        ''' <summary>アラート表示しない</summary>
        Public Shared No As String = "N"
        ''' <summary>アラート表示する</summary>
        Public Shared Yes As String = "Y"
    End Class

    ''' <summary>
    ''' 貸出上限エラー区分
    ''' </summary>
    Public Class LeaseUpperErrDiv
        ''' <summary>上限を超える貸出の場合アラートにする。</summary>
        Public Shared No As String = "N"
        ''' <summary>上限を超える貸出の場合エラーにする。</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>しない</summary>
        Public Shared None As String = String.Empty
    End Class

    ''' <summary>
    ''' 表示指定区分定義クラス
    ''' </summary>
    Public Class DISP_SELECT
        Public Shared L As String = "L"
        Public Shared M As String = "M"
        Public Shared S As String = "S"
        Public Shared LM As String = "LM"
        Public Shared LMS As String = "LMS"
    End Class

    ''' <summary>
    ''' 処理ツール区分定義クラス
    ''' </summary>
    Public Class ProcessToolDiv
        Public Shared Web As String = "W"
        Public Shared CS As String = "C"
        Public Shared HT As String = "H"
        '[CC431] ADD Start 20261015
        ''' <summary>検索条件専用：棚卸実施端末が未設定（NULL・空文字）。DB には保存しない値</summary>
        Public Shared NoValue As String = "-"
        '[CC431] ADD End 20261015
    End Class

    ''' <summary>
    ''' 文言区分区分定義クラス
    ''' </summary>
    Public Class WordDiv
        Public Shared ItemName As String = "I"
        Public Shared Title As String = "T"
        Public Shared Button As String = "B"
        Public Shared Value As String = "V"
        Public Shared Message As String = "M"
        Public Shared Remark As String = "R"
        Public Shared Csv As String = "C"
        Public Shared Setting As String = "D"
    End Class

    ''' <summary>
    ''' 言語区分定義クラス
    ''' </summary>
    Public Class LangDiv
        Public Shared JA As String = "JA"
        Public Shared EN As String = "EN"
        Public Shared LU As String = "LU"
    End Class

    ''' <summary>
    ''' 処理ツール区分定義クラス
    ''' </summary>
    Public Class ProcessToolDivCSV
        Public Shared Web As String = "Web"
        Public Shared CS As String = "CS"
        Public Shared HT As String = "HT"
    End Class

    ''' <summary>
    ''' 文言区分区分定義クラス
    ''' </summary>
    Public Class WordDivCSV
        Public Shared ItemName As String = "item"
        Public Shared Title As String = "title"
        Public Shared Button As String = "button"
        Public Shared Value As String = "value"
        Public Shared Message As String = "message"
        Public Shared Remark As String = "remark"
        Public Shared Csv As String = "csvitem"
        Public Shared Setting As String = "setting"
    End Class

    ''' <summary>
    ''' 処理種別定義クラス
    ''' </summary>
    Public Class ProcessCat
        Public Shared [Event] As String = "EV"
        Public Shared Operation As String = "OP"
        Public Shared [Error] As String = "ER"
    End Class

    ''' <summary>
    ''' 実棚ステータス定義クラス
    ''' </summary>
    Public Class PHYINV_STATUS
        ''' <summary>0:対象外</summary>
        Public Shared Excluded As String = "0"
        ''' <summary>E:現物なし(未処理を含む)</summary>
        Public Shared NoSpot As String = "E"
        ''' <summary>N:正常 (現物あり、現物なしで貸出中)</summary>
        Public Shared Normal As String = "N"
        ''' <summary>W:警告(現物あり、ロケ違い)</summary>
        Public Shared Warning As String = "W"
    End Class

    ''' <summary>
    ''' 資産状態定義クラス
    ''' </summary>
    Public Class ASSET_STATUS
        ''' <summary>0:準備中</summary>
        Public Shared Preparation As String = "0"
        ''' <summary>1:保管中</summary>
        Public Shared Storage As String = "1"
        ''' <summary>2:貸出中</summary>
        Public Shared Lending As String = "2"
        ''' <summary>9:除却済</summary>
        Public Shared Removed As String = "9"
    End Class

    ''' <summary>
    ''' 有形・無形
    ''' </summary>
    Public Class TANGIBLE
        ''' <summary>Y:有形資産</summary>
        Public Shared TANGIBLE As String = "Y"
        ''' <summary>N:無形資産</summary>
        Public Shared NOT_TANGIBLE As String = "N"
    End Class

    ''' <summary>
    ''' 貸与資産状態区分
    ''' </summary>
    Public Class LOAN_ASSET_STATUS
        ''' <summary>(空白):-</summary>
        Public Shared BLANK As String = "-"
        ''' <summary>1:貸与中</summary>
        Public Shared ON_LOAN As String = "1"
        ''' <summary>2:返却済み</summary>
        Public Shared RETURNED As String = "2"
    End Class

    ''' <summary>
    ''' 貸与資産状態区分(名称)
    ''' </summary>
    Public Class LOAN_ASSET_STATUS_VALUE
        ''' <summary>(空白):-</summary>
        Public Shared BLANK As String = "-"
        ''' <summary>1:貸与中</summary>
        Public Shared ON_LOAN As String = "貸与中"
        ''' <summary>2:返却済み</summary>
        Public Shared RETURNED As String = "返却済み"
    End Class

    ''' <summary>
    ''' 工事状態定義クラス
    ''' </summary>
    Public Class CONST_STATUS
        ''' <summary>0:未完了</summary>
        Public Shared Incomplete As String = "0"
        ''' <summary>1:完了</summary>
        Public Shared Complete As String = "1"
    End Class

    ''' <summary>
    ''' 工事状態 CSV出力・取込用
    ''' </summary>
    Public Class CONST_STATUS_CSV
        ''' <summary>N:未完了</summary>
        Public Shared Incomplete As String = "N"
        ''' <summary>Y:完了</summary>
        Public Shared Complete As String = "Y"
    End Class

    ''' <summary>
    ''' 工事資産状態定義クラス
    ''' </summary>
    Public Class CONST_ASSET_STATUS
        ''' <summary>1:貸出中</summary>
        Public Shared Lending As String = "1"
        ''' <summary>2:予約中</summary>
        Public Shared Reserve As String = "2"
        ''' <summary>3:返却済み</summary>
        Public Shared HenkyakuSumi As String = "3"
    End Class

    ''' <summary>
    ''' 資産状態定義クラス
    ''' </summary>
    Public Class CONST_MATCH_STATUS
        ''' <summary>1:保管中(返却済み)</summary>
        Public Shared Storage As String = "1"
        ''' <summary>2:貸出中</summary>
        Public Shared Lending As String = "2"
        ''' <summary>9:仮置き解除</summary>
        Public Shared TempRelease As String = "9"
    End Class

    ''' <summary>
    ''' 仮置き紐付状態定義クラス
    ''' </summary>
    Public Class REL_CONST_STATUS
        ''' <summary>0:未紐付け</summary>
        Public Shared Unlinked As String = "0"
        ''' <summary>1:他工事に紐づき済み</summary>
        Public Shared Linked As String = "1"
    End Class

    ''' <summary>
    ''' 仮置き紐付状態 CSV出力・取込用
    ''' </summary>
    Public Class REL_CONST_STATUS_CSV
        ''' <summary>N:未紐付け</summary>
        Public Shared Unlinked As String = "N"
        ''' <summary>Y:他工事に紐づき済み</summary>
        Public Shared Linked As String = "Y"
    End Class

    ''' <summary>
    ''' ラベル種類区分定義クラス
    ''' </summary>
    Public Class LabelType
        Public Shared [AS] As String = "*AS"
        Public Shared LC As String = "*LC"
        Public Shared EP As String = "*EP"
    End Class

    ''' <summary>
    ''' 処理区分定義クラス
    ''' </summary>
    Public Class ProcessDiv
        ''' <summary>登録(メンテ)</summary>
        Public Shared MI As String = "MI"
        ''' <summary>登録(取込)</summary>
        Public Shared CI As String = "CI"
        ''' <summary>更新(メンテ)</summary>
        Public Shared MU As String = "MU"
        ''' <summary>更新(取込)</summary>
        Public Shared CU As String = "CU"
        ''' <summary>除却(メンテ)</summary>
        Public Shared MR As String = "MR"
        ''' <summary>除却(取込) </summary>
        Public Shared CR As String = "CR"
        ''' <summary>削除(メンテ)</summary>
        Public Shared MD As String = "MD"
        ''' <summary>削除(取込)</summary>
        Public Shared CD As String = "CD"
        ''' <summary>タグラベル発行＆紐付</summary>
        Public Shared T1 As String = "T1"
        ''' <summary>タグ紐付</summary>
        Public Shared T2 As String = "T2"
        ''' <summary>保管</summary>
        Public Shared BS As String = "BS"
        ''' <summary>貸出</summary>
        Public Shared BO As String = "BO"
        ''' <summary>返却</summary>
        Public Shared BI As String = "BI"
        ''' <summary>除却</summary>
        Public Shared BR As String = "BR"
        ''' <summary>現ロケ変更</summary>
        Public Shared LC As String = "LC"
        ''' <summary>予約登録</summary>
        Public Shared RI As String = "RI"
        ''' <summary>予約変更</summary>
        Public Shared RU As String = "RU"
        ''' <summary>予約削除</summary>
        Public Shared RD As String = "RD"
        '2022/7/25 sv sato add start
        ''' <summary>予約登録(取込)</summary>
        Public Shared XI As String = "XI"
        ''' <summary>予約変更(取込)</summary>
        Public Shared XU As String = "XU"
        ''' <summary>予約削除(取込)</summary>
        Public Shared XD As String = "XD"
        '2022/7/25 sv sato add end
        ''' <summary>残量確認  2023/04/30 sv sato add</summary>
        Public Shared DC As String = "DC"
        ''' <summary>入庫予定</summary>
        Public Shared LP As String = "LP"
        ''' <summary>登録(ラベル発行)</summary>
        Public Shared LI As String = "LI"
        ''' <summary>登録(ラベル不要)</summary>
        Public Shared UI As String = "UI"
        '[CC431] ADD Start 20261002
        ''' <summary>棚卸確定</summary>
        Public Shared IC As String = "IC"
        '[CC431] ADD End 20261002
    End Class

    ''' <summary>
    ''' 設定M 資産ID生成方法定義クラス
    ''' </summary>
    Public Class AssetIDGenerationMethod
        ''' <summary>1:生成しない(タグ紐付で行う)</summary>
        Public Shared None As Integer = 1
        ''' <summary>2:資産番号より生成</summary>
        Public Shared AssetNo As Integer = 2
        ''' <summary>3:自動採番で生成</summary>
        Public Shared Auto As Integer = 3
        ''' <summary>4:CSV取込時の資産IDをそのまま使用する。</summary>
        Public Shared Inp As Integer = 4
        ''' <summary>5:GIAI</summary>
        Public Shared GIAI As Integer = 5
        ''' <summary>6:GRAI</summary>
        Public Shared GRAI As Integer = 6
    End Class

    ''' <summary>
    ''' 資産ID生成方法 利用有無定義クラス
    ''' </summary>
    Public Class AssetIDGenerationUsageType
        ''' <summary>Y:利用する</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>N:利用しない</summary>
        Public Shared No As String = "N"
    End Class
    ''' <summary>
    ''' 設定対象区分定義クラス
    ''' </summary>
    Public Class SETTING_OBJECT_DIV
        Public Shared HT As String = "HT"
        Public Shared PC As String = "PC"
        Public Shared WEB As String = "WEB"
        Public Shared ALL As String = "ALL"
    End Class

    ''' <summary>
    ''' 更新可否定義クラス
    ''' </summary>
    Public Class UPD_AUTHORITY_FLG
        ''' <summary>Y:更新可能</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>N:更新不可</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' 削除可否定義クラス
    ''' </summary>
    Public Class DEL_AUTHORITY_FLG
        ''' <summary>Y:削除可能</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>N:削除不可</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' 設定M 書込みタグEPC領域サイズ
    ''' </summary>
    Public Class TagSize
        Public Shared Size064 As Integer = 64
        Public Shared Size096 As Integer = 96
        Public Shared Size128 As Integer = 128
        Public Shared Size256 As Integer = 256
        Public Shared Size512 As Integer = 512
    End Class

    ''' <summary>
    ''' CSV取込データ削除設定
    ''' </summary>
    Public Class AssetDelete
        Public Shared Remove As String = "DP"
        Public Shared Delete As String = "DA"
    End Class

    ''' <summary>
    ''' 入力属性定義クラス
    ''' </summary>
    Public Class InputType
        Public Shared TEXT As String = "TEXT"
        Public Shared LIST As String = "LIST"
        Public Shared AREA As String = "AREA"
        Public Shared LABEL As String = "LABEL"
    End Class

    ''' <summary>
    ''' データ取込画面　定数定義クラス
    ''' </summary>
    Public Class DataInport
        ''' <summary>
        ''' データ取込画面　画面に表示する最大件数
        ''' </summary>
        Public Const DISP_ROW_COUNT As Integer = 5

        ''' <summary>資産番号の自動生成上限</summary>
        Public Const ASSET_NO_AUTO_REG_MAX_SIZE = 100000

        ''' <summary>
        ''' データ取込画面　結果
        ''' </summary>
        Public Class color
            ''' <summary>デフォルト</summary>
            Public Const Nomal As String = "white"
            ''' <summary>データ取込中・正常終了</summary>
            Public Const OK As String = "yellow"
            ''' <summary>データ取込中・正常終了</summary>
            Public Const NG As String = "red"
        End Class

        ''' <summary>
        ''' データ取込画面　結果
        ''' </summary>
        Public Class DataInportResultCd
            ''' <summary>正常終了</summary>
            Public Const OK As Integer = 0
            ''' <summary>選択データ無し</summary>
            Public Const NoSelect As Integer = 1
            ''' <summary>データが空</summary>
            Public Const Empty As Integer = 2
            ''' <summary>拡張子違い</summary>
            Public Const NotCsv As Integer = 3
            ''' <summary>項目数が不正</summary>
            Public Const KoumokuSuuErr As Integer = 4
            ''' <summary>データが不正</summary>
            Public Const DataNG As Integer = 5
            ''' <summary>ログインセッション切れ</summary>
            Public Const LoginSession As Integer = 6
            ''' <summary>CSVマッピング明細マスタが不正</summary>
            Public Const CsvMapMei As Integer = 7
            ''' <summary>CSVマッピング明細マスタが不正</summary>
            Public Const CsvMapCharCd As Integer = 8
            ''' <summary>設定マスタにCSV取込用文字コードのデータ無し</summary>
            Public Const SettingInfo As Integer = 9
            ''' <summary>ロケごとの資産登録上限数オーバー</summary>
            Public Const AssetRegistrationLimit As Integer = 10
            ''' <summary>拡張子違い（Excel用）</summary>
            Public Const NotExcel As Integer = 11
            ''' <summary>Excel取込エラー（予期しないエラー）</summary>
            Public Const ImportExcelErr As Integer = 12
            ''' <summary>拠点責任者がログインしている場合、自拠点以外が含まれている場合はエラー。</summary>
            Public Const ImportKengenErr As Integer = 13
            ''' <summary>Excel取込エラー（ストアドで予期しないエラー）</summary>
            Public Const ImportSpSystemErr As Integer = 14
            ''' <summary>Excel取込エラー（ストアドで引数エラー）</summary>
            Public Const ImportSpParamErr As Integer = 15
            ''' <summary>Excel取込エラー（ストアドでロケマスタなしエラー）</summary>
            Public Const ImportSpLocMaterErr As Integer = 16
            ''' <summary>Excel取込エラー（ストアドで登録担当者なしエラー）</summary>
            Public Const ImportSpEmployeeErr As Integer = 17
            ''' <summary>Excel取込エラー（更新可能拠点以外を含む）</summary>
            Public Const ImportNotUpdatableComErr As Integer = 18
            ''' <summary>削除件数が警告件数を超えたため、実行前に確認が必要</summary>
            ''' <remarks>
            ''' 設定M(KEY種別=Asset_Delete_AlertCount)のDATA1を超える削除が発生する場合に返す。
            ''' 画面で確認して「はい」が選ばれた場合は、入庫予定ワークに保存済みの内容で取込を続行する（ExecuteConfirmed。Excelは読み直さない）。
            ''' </remarks>
            Public Const ImportDeleteConfirm As Integer = 19
        End Class
    End Class


    ''' <summary>
    ''' 項目長
    ''' </summary>
    Public Class itemlength
        ''' <summary>社員コード</summary>
        Public Const EmployeeCd As Integer = 15
        ''' <summary>社員名</summary>
        Public Const EmployeeNm As Integer = 60
        ''' <summary>メールアドレス</summary>
        Public Const MailAddress As Integer = 50
        ''' <summary>部署コード</summary>
        Public Const DepartmentCd As Integer = 15
        ''' <summary>部署名</summary>
        Public Const DepartmentNm As Integer = 60
        ''' <summary>郵便番号</summary>
        Public Const ZipCode As Integer = 8
        ''' <summary>住所</summary>
        Public Const Address As Integer = 48
        ''' <summary>送付部署名</summary>
        Public Const ReqDepName As Integer = 150
        ''' <summary>電話番号</summary>
        Public Const PhoneNumber As Integer = 14
        ''' <summary>FAX</summary>
        Public Const Fax As Integer = 14
        ''' <summary>権限</summary>
        Public Const KengenKbn As Integer = 1
        ''' <summary>貸出先コード</summary>
        Public Const LeasePlaceCd As Integer = 15
        ''' <summary>貸出先名</summary>
        Public Const LeasePlaceNm As Integer = 60
        ''' <summary>ロケ区分</summary>
        Public Const LocDiv As Integer = 1
        ''' <summary>ロケ大区分コード</summary>
        Public Const LocLCd As Integer = 10     '20210511 UPD Suzuki
        ''' <summary>ロケ中区分コード</summary>
        Public Const LocMCd As Integer = 10     '20210511 UPD Suzuki
        ''' <summary>ロケ小区分コード</summary>
        Public Const LocSCd As Integer = 10     '20210511 UPD Suzuki
        ''' <summary>ロケQR</summary>
        Public Const LocQr As Integer = 30
        ''' <summary>ロケ名称</summary>
        Public Const LocNm As Integer = 60
        ''' <summary>カテゴリ大分類コード</summary>
        Public Const CatLCd As Integer = 8
        ''' <summary>カテゴリ中分類コード</summary>
        Public Const CatMCd As Integer = 8
        ''' <summary>カテゴリ小分類コード</summary>
        Public Const CatSCd As Integer = 8
        ''' <summary>分類名称</summary>
        Public Const CatNm As Integer = 60
        ''' <summary>リース期限アラート区分</summary>
        Public Const LeaseLimitAlertDiv As Integer = 1
        ''' <summary>リース期限何日前</summary>
        Public Const LeaseLimitBefDays As Integer = 3
        ''' <summary>○○期限アラート区分</summary>
        Public Const GenericLimitAlertDiv As Integer = 1
        ''' <summary>○○期限何日前</summary>
        Public Const GenericLimitBefDays As Integer = 3
        ''' <summary>貸出アラート区分</summary>
        Public Const LeaseAlertDiv As Integer = 1
        ''' <summary>貸出アラート回数</summary>
        Public Const LeaseAlertCnt As Integer = 5
        ''' <summary>貸出上限数</summary>
        Public Const LeaseUpperLimit As Integer = 5
        ''' <summary>貸出上限エラー区分</summary>
        Public Const LeaseUpperErrDiv As Integer = 1
        ''' <summary>EPC事業所キー</summary>
        Public Const EPCCompanyKey As Integer = 30
        ''' <summary>EPC資産タイプ</summary>
        Public Const EPCAssetType As Integer = 60
        ''' <summary>資産番号</summary>
        Public Const AssetNo As Integer = 24
        ''' <summary>資産ID</summary>
        Public Const AssetID As Integer = 128
        ''' <summary>資産区分</summary>
        Public Const AssetDiv As Integer = 60
        ''' <summary>資産名称</summary>
        Public Const AssetNm As Integer = 60
        ''' <summary>メーカ</summary>
        Public Const Maker As Integer = 60
        ''' <summary>品番</summary>
        Public Const ProductNo As Integer = 30
        ''' <summary>型番</summary>
        Public Const ModelNo As Integer = 30
        ''' <summary>リース管理番号</summary>
        Public Const LeaseNo As Integer = 30
        ''' <summary>リース期限</summary>
        Public Const LeaseLimit As Integer = 8
        ''' <summary>○○期限</summary>
        Public Const GenLimit As Integer = 8
        ''' <summary>フリー項目</summary>
        Public Const FreeItem As Integer = 255
        ''' <summary>資産画像ファイル名</summary>
        Public Const AssetPicFileNm As Integer = 255
        ''' <summary>資産購入日</summary>
        Public Const AssetKounyuDate As Integer = 10
        ''' <summary>利用ツール区分</summary>
        Public Const ToolDiv As Integer = 1
        ''' <summary>文言区分</summary>
        Public Const WordDiv As Integer = 1
        ''' <summary>文言コード</summary>
        Public Const WordCd As Integer = 6
        ''' <summary>CSVマッピング画面　列番号</summary>
        Public Const ColNo As Integer = 3
        ''' <summary>CSVマッピング画面　固定値</summary>
        Public Const FixVal As Integer = 255
        ''' <summary>非稼働日フラグ</summary>
        Public Const NotWorkFlg As Integer = 1
        '--2021/10/04 nv miyaji add start
        ''' <summary>貸出状態</summary>
        Public Const LeaseStatus As Integer = 1
        ''' <summary>管理ロケ区分コード</summary>
        Public Const MngLocLCd As Integer = 10
        ''' <summary>メンテ稼働日フラグ</summary>
        Public Const MainteWorkFlg As Integer = 1
        ''' <summary>メンテ発生貸出回数</summary>
        Public Const MainteLeaseNum As Integer = 4
        ''' <summary>メンテ発生貸出日数</summary>
        Public Const MainteLeaseDay As Integer = 4
        ''' <summary>メンテ日数</summary>
        Public Const MainteDay As Integer = 4
        '--2021/10/04 nv miyaji add end
        ''' <summary>メール送信先</summary>
        Public Const MailSendAddress As Integer = 1000      '2021/11/08 add nv iwamoto

        '2022/07/25 add start sv sato
        ''' <summary>処理区分</summary>
        Public Const ProcDiv As Integer = 1
        ''' <summary>予約番号</summary>
        Public Const ReserveNum As Integer = 18
        ''' <summary>申請日</summary>
        Public Const ReqestDate As Integer = 8
        ''' <summary>実貸出開始日</summary>
        Public Const RealStartDt As Integer = 8
        ''' <summary>実貸出終了日</summary>
        Public Const RealEndDt As Integer = 8
        ''' <summary>貸出用途</summary>
        Public Const LeaseUse As Integer = 30
        ''' <summary>送り先名</summary>
        Public Const DestName As Integer = 16
        ''' <summary>送り先住所</summary>
        Public Const DestAddress As Integer = 16
        ''' <summary>備考</summary>
        Public Const Remark As Integer = 255
        ''' <summary>送り状番号</summary>
        Public Const DestNo As Integer = 100
        '2022/07/25 add end sv sato
        '2023/02/09 ADD　Start Shirakawa
        ''' <summary>指定数量整数</summary>
        Public Const SpecifiedQty As Integer = 5
        ''' <summary>指定数量小数点以下</summary>
        Public Const SpecifiedQtyDecimal As Integer = 3
        '2023/02/09 ADD　Start Shirakawa
        '2023/02/08 Kawamoto ADD START
        ''' <summary>購入日</summary>
        Public Const KounyuDate As Integer = 10
        '2023/02/08 Kawamoto ADD END
        '2023/03/22 Kawamoto ADD START
        ''' <summary>貸出確認書宛名</summary>
        Public Const ToDestName As Integer = 16
        ''' <summary>送り先住所</summary>
        Public Const RetAddress As Integer = 16
        '2023/03/22 Kawamoto ADD START
        ''' <summary>資産番号の自動生成桁数</summary>
        Public Const AssetNoAuto As Integer = 12
        ''' <summary>資産番号の自動生成連番桁数</summary>
        Public Const AssetNoAutoSeq As Integer = 5
        ''' <summary>並び順</summary>
        Public Const DispOrder As Integer = 4
        ''' <summary>工事開始日</summary>
        Public Const ConstStartDt As Integer = 8
        ''' <summary>工事終了日</summary>
        Public Const ConstEndDt As Integer = 8
        ''' <summary>工事貸出開始日</summary>
        Public Const ConstLentStartDt As Integer = 8
        ''' <summary>工事貸出終了日</summary>
        Public Const ConstLentEndDt As Integer = 8
        ''' <summary>工事番号</summary>
        Public Const ConstNo As Integer = 20
        ''' <summary>工事内容</summary>
        Public Const ConstContents As Integer = 255
        ''' <summary>工事責任者コード</summary>
        Public Const ConstEmployeeCd As Integer = 15
        ''' <summary>工事情報</summary>
        Public Const ConstInfo As Integer = 255
        ''' <summary>工事完了日時</summary>
        Public Const ConstConfirmTs As Integer = 14
        ''' <summary>工事番号の自動生成桁数</summary>
        Public Const ConstNoAuto As Integer = 14
        ''' <summary>工事番号の自動生成連番桁数</summary>
        Public Const ConstNoAutoSeq As Integer = 6
    End Class

    ''' <summary>
    ''' 最大値
    ''' </summary>
    Public Class MaxValue
        ''' <summary>何日前</summary>
        Public Const BefDays As Integer = 999
        ''' <summary>貸出アラート回数</summary>
        Public Const LeaseAlertCnt As Integer = 99999
        ''' <summary>貸出上限数</summary>
        Public Const LeaseUpperLimit As Integer = 99999
    End Class
    ''' <summary>
    ''' 最小値
    ''' </summary>
    Public Class MinValue
        ''' <summary>何日前</summary>
        Public Const BefDays As Integer = 0
        ''' <summary>貸出アラート回数</summary>
        Public Const LeaseAlertCnt As Integer = 1
        ''' <summary>貸出上限数</summary>
        Public Const LeaseUpperLimit As Integer = 1
    End Class

    ''' <summary>
    ''' EPCエンコード状態定義クラス
    ''' </summary>
    Public Class EpcEncStatus
        Public Shared Mi As String = "0"
        Public Shared Sumi As String = "1"
    End Class

    ''' <summary>
    ''' タグ紐付状態区分定義クラス
    ''' </summary>
    Public Class TagLinkStatus
        Public Shared Mi As String = "0"
        Public Shared Sumi As String = "1"
    End Class

    ''' <summary>
    ''' 資産一覧画面　表示順定義クラス
    ''' </summary>
    ''' <summary>
    ''' 資産ラベル発行画面　並び順定義クラス
    ''' </summary>
    ''' <remarks>
    ''' 設計書の並び順（資産クラス／資産番号／原価センタ／場所／資本化日付／管理者社員番号の各昇順）に対応する。
    ''' 第2ソートキーの資産番号昇順は、ORDER句を組み立てる側で付与する。
    ''' </remarks>
    Public Class AssetLabelPrintSort
        ''' <summary>資産クラス昇順</summary>
        Public Shared CatLAsc As Integer = 0
        ''' <summary>資産番号昇順</summary>
        Public Shared AssetNoAsc As Integer = 1
        ''' <summary>原価センタ昇順</summary>
        Public Shared LocMAsc As Integer = 2
        ''' <summary>場所昇順</summary>
        Public Shared LocSAsc As Integer = 3
        ''' <summary>資本化日付昇順</summary>
        Public Shared FreeItem2Asc As Integer = 4
        ''' <summary>管理者社員番号昇順</summary>
        Public Shared FreeItem6Asc As Integer = 5
    End Class

    '[CC431] ADD Start 20261002
    ''' <summary>
    ''' 実棚照会画面　並び順定義クラス
    ''' </summary>
    ''' <remarks>
    ''' 第2ソートキーの資産番号昇順は、ORDER句を組み立てる側で付与する。
    ''' </remarks>
    Public Class InventoryInquirySort
        ''' <summary>資産クラス昇順</summary>
        Public Shared CatLAsc As Integer = 0
        ''' <summary>資産番号昇順</summary>
        Public Shared AssetNoAsc As Integer = 1
        ''' <summary>原価センタ昇順</summary>
        Public Shared LocMAsc As Integer = 2
        ''' <summary>場所昇順</summary>
        Public Shared LocSAsc As Integer = 3
        ''' <summary>資本化日付昇順</summary>
        Public Shared FreeItem2Asc As Integer = 4
        ''' <summary>管理者社員番号昇順</summary>
        Public Shared FreeItem6Asc As Integer = 5
        ''' <summary>実棚者コード昇順</summary>
        Public Shared PhyinvEmployeeAsc As Integer = 6
        ''' <summary>備考1昇順</summary>
        Public Shared Remark1Asc As Integer = 7
        ''' <summary>備考2昇順</summary>
        Public Shared Remark2Asc As Integer = 8
        ''' <summary>備考3昇順</summary>
        Public Shared Remark3Asc As Integer = 9
        ''' <summary>備考4昇順</summary>
        Public Shared Remark4Asc As Integer = 10
        ''' <summary>備考5昇順</summary>
        Public Shared Remark5Asc As Integer = 11
        ''' <summary>備考6昇順</summary>
        Public Shared Remark6Asc As Integer = 12
        ''' <summary>備考7昇順</summary>
        Public Shared Remark7Asc As Integer = 13
        ''' <summary>備考8昇順</summary>
        Public Shared Remark8Asc As Integer = 14
        ''' <summary>備考9昇順</summary>
        Public Shared Remark9Asc As Integer = 15
        ''' <summary>備考10昇順</summary>
        Public Shared Remark10Asc As Integer = 16
    End Class

    ''' <summary>
    ''' 実棚照会画面　ロック資産ステータス定義クラス（検索条件の値。DBには保存しない）
    ''' </summary>
    Public Class InvLockStatus
        ''' <summary>未：ロック担当者なし</summary>
        Public Shared Unlocked As String = "0"
        ''' <summary>済：ロック担当者あり</summary>
        Public Shared Locked As String = "1"
    End Class
    '[CC431] ADD End 20261002

    '[CC431] ADD Start 20261015
    ''' <summary>
    ''' 資産履歴照会画面　並び順定義クラス（仕様変更2）
    ''' </summary>
    ''' <remarks>各並び順の ORDER BY は AssetHistoryController.GetOrderBy で組み立てる。第2キー以降は処理日付降順、最後に資産番号昇順。</remarks>
    Public Class AssetHistorySort
        ''' <summary>処理日時降順</summary>
        Public Shared ProcessDtDesc As Integer = 0
        ''' <summary>資産クラス昇順</summary>
        Public Shared CatLAsc As Integer = 1
        ''' <summary>資産番号昇順</summary>
        Public Shared AssetNoAsc As Integer = 2
        ''' <summary>原価センタ昇順</summary>
        Public Shared LocMAsc As Integer = 3
        ''' <summary>場所昇順</summary>
        Public Shared LocSAsc As Integer = 4
        ''' <summary>備考1昇順</summary>
        Public Shared Remark1Asc As Integer = 5
        ''' <summary>備考2昇順</summary>
        Public Shared Remark2Asc As Integer = 6
        ''' <summary>備考3昇順</summary>
        Public Shared Remark3Asc As Integer = 7
        ''' <summary>備考4昇順</summary>
        Public Shared Remark4Asc As Integer = 8
        ''' <summary>備考5昇順</summary>
        Public Shared Remark5Asc As Integer = 9
        ''' <summary>備考6昇順</summary>
        Public Shared Remark6Asc As Integer = 10
        ''' <summary>備考7昇順</summary>
        Public Shared Remark7Asc As Integer = 11
        ''' <summary>備考8昇順</summary>
        Public Shared Remark8Asc As Integer = 12
        ''' <summary>備考9昇順</summary>
        Public Shared Remark9Asc As Integer = 13
        ''' <summary>備考10昇順</summary>
        Public Shared Remark10Asc As Integer = 14
    End Class

    ''' <summary>
    ''' 資産在庫照会画面　並び順定義クラス（仕様変更2）
    ''' </summary>
    ''' <remarks>一覧は集約表示（ロケーション・分類）ごとの集計のため、資産番号・備考などは持たない。会社（ロケ大）は並び順に含めない。</remarks>
    Public Class StockListSort
        ''' <summary>原価センタ昇順（原価センタ、場所、資産クラス）。初期値</summary>
        Public Shared LocMAsc As Integer = 0
        ''' <summary>場所昇順（場所、原価センタ、資産クラス）</summary>
        Public Shared LocSAsc As Integer = 1
        ''' <summary>資産クラス昇順（資産クラス、原価センタ、場所）</summary>
        Public Shared CatLAsc As Integer = 2
    End Class
    '[CC431] ADD End 20261015

    Public Class AssetDispSelect
        Public Shared Loca As Integer = 0
        Public Shared Cat As Integer = 1
        Public Shared Asset As Integer = 2
        Public Shared Rate As Integer = 3  '2023/02/06 Kawamoto ADD
        ''' <summary>資産クラス昇順</summary>
        Public Shared CatLAsc As Integer = 10
        ''' <summary>資産番号昇順</summary>
        Public Shared AssetNoAsc As Integer = 11
        ''' <summary>原価センタ昇順</summary>
        Public Shared LocMAsc As Integer = 12
        ''' <summary>場所昇順</summary>
        Public Shared LocSAsc As Integer = 13
        ''' <summary>資本化日付昇順</summary>
        Public Shared FreeItem2Asc As Integer = 14
        ''' <summary>管理者社員番号昇順</summary>
        Public Shared FreeItem6Asc As Integer = 15
        ''' <summary>備考1昇順</summary>
        Public Shared Remark1Asc As Integer = 16
        ''' <summary>備考2昇順</summary>
        Public Shared Remark2Asc As Integer = 17
        ''' <summary>備考3昇順</summary>
        Public Shared Remark3Asc As Integer = 18
        ''' <summary>備考4昇順</summary>
        Public Shared Remark4Asc As Integer = 19
        ''' <summary>備考5昇順</summary>
        Public Shared Remark5Asc As Integer = 20
        ''' <summary>備考6昇順</summary>
        Public Shared Remark6Asc As Integer = 21
        ''' <summary>備考7昇順</summary>
        Public Shared Remark7Asc As Integer = 22
        ''' <summary>備考8昇順</summary>
        Public Shared Remark8Asc As Integer = 23
        ''' <summary>備考9昇順</summary>
        Public Shared Remark9Asc As Integer = 24
        ''' <summary>備考10昇順</summary>
        Public Shared Remark10Asc As Integer = 25
    End Class

    ''' <summary>
    ''' 資産履歴テーブル　処理結果定義クラス
    ''' </summary>
    Public Class TaskResult
        ''' <summary>正常</summary>
        Public Shared OK As String = "N"
        ''' <summary>エラー</summary>
        Public Shared NG As String = "E"
    End Class

    ''' <summary>
    ''' 工事履歴テーブル　工事作業区分定義クラス
    ''' </summary>
    Public Class ConstWorkDiv
        ''' <summary>工事登録</summary>
        Public Shared ConstRegist As String = "1 "
        ''' <summary>仮置き紐付</summary>
        Public Shared TempLink As String = "2 "
        ''' <summary>貸出</summary>
        Public Shared Lease As String = "3 "
        ''' <summary>持込照合</summary>
        Public Shared InMatch As String = "4 "
        ''' <summary>仮置き登録</summary>
        Public Shared TempRegist As String = "5 "
        ''' <summary>持ち帰照合</summary>
        Public Shared OutMatch As String = "6 "
        ''' <summary>照合返却</summary>
        Public Shared MatchReturn As String = "7 "
        ''' <summary>返却</summary>
        Public Shared Returned As String = "8 "
        ''' <summary>工事完了</summary>
        Public Shared ConstComplete As String = "9 "
        ''' <summary>別工事紐付</summary>
        Public Shared AnotherConstLink As String = "10"
        ''' <summary>仮置き解除</summary>
        Public Shared TempRelease As String = "11"
    End Class

    ''' <summary>
    ''' 工事履歴テーブル　照合結果区分定義クラス
    ''' </summary>
    Public Class ConstMatchDiv
        ''' <summary>照合なし</summary>
        Public Shared None As String = "0"
        ''' <summary>照合OK</summary>
        Public Shared Ok As String = "1"
        ''' <summary>照合リスト外</summary>
        Public Shared OutOfMatchList As String = "8"
        ''' <summary>照合NG</summary>
        Public Shared Ng As String = "9"
    End Class

    ''' <summary>
    ''' QR台帳　エクセル出力行・列定義クラス
    ''' </summary>
    Public Class LabelQRDaicho
        Public Class Row
            ''' <summary>資産番号</summary>
            Public Shared AssetNo As Integer = 1
            ''' <summary>資産名称</summary>
            Public Shared AssetNm As Integer = 2
            ''' <summary>ロケーションコード</summary>
            Public Shared LocaCd As Integer = 3
            ''' <summary>ロケーション名</summary>
            Public Shared LocaNm As Integer = 4
            ''' <summary>カテゴリーコード</summary>
            Public Shared CatCd As Integer = 5
            ''' <summary>カテゴリー名</summary>
            Public Shared CatNm As Integer = 6
            ''' <summary>QR画像</summary>
            Public Shared Pic As Integer = 2
        End Class
        Public Class Col
            ''' <summary>画像以外の項目</summary>
            Public Shared NotPic As Integer = 1
            ''' <summary>QR画像</summary>
            Public Shared Pic As Integer = 4
            ''' <summary>QR画像 左の列の貼付ポイント位置</summary>
            Public Shared PointX_Left As Integer = 170
            ''' <summary>QR画像 右の列の貼付ポイント位置</summary>
            Public Shared PointX_Right As Integer = 410
        End Class

        ''' <summary>文字の表示文字数</summary>
        Public Shared StrLength As Integer = 19

        ''' <summary>左のラベルの項目から右のラベルの同項目までの列数</summary>
        Public Shared ColCount As Integer = 7

        ''' <summary>上のラベルの項目から下のラベルの同項目までの行数</summary>
        Public Shared RowCount As Integer = 9

        ''' <summary>1ページに表示するラベル数</summary>
        Public Shared PageLabelSuu As Integer = 16

        ''' <summary>1列のラベル数</summary>
        Public Shared ColLabelSuu As Integer = 8

        ''' <summary>罫線削除行(上)</summary>
        Public Shared DelLineTop As Integer = 0

        ''' <summary>罫線削除列(下)</summary>
        Public Shared DelLineBottom As Integer = 7

        ''' <summary>罫線削除列(左)</summary>
        Public Shared DelLineLeft As Integer = 0

        ''' <summary>罫線削除列(右)</summary>
        Public Shared DelLineRight As Integer = 5

        ''' <summary>1ラベルの列数</summary>
        Public Shared LabelColSuu As Integer = 6

        ''' <summary>1ラベルの行数</summary>
        Public Shared LabelRowSuu As Integer = 8

        ''' <summary>QR画像のサイズ縦横</summary>
        Public Shared ImageSize As Integer = 55

    End Class

    ''' <summary>
    ''' QR台帳(カスタム1)　エクセル出力行・列定義クラス
    ''' </summary>
    Public Class QRDaichoCumstom1
        Public Class TruncLength
            ''' <summary>資産番号</summary>
            Public Const AssetNo As Integer = 24
            ''' <summary>資産名称</summary>
            Public Const AssetNm As Integer = 16
            ''' <summary>備考1</summary>
            Public Const FreeItem1 As Integer = 16
            ''' <summary>備考2</summary>
            Public Const FreeItem2 As Integer = 12

        End Class
    End Class

    ''' <summary>
    ''' CSVマッピング画面　指定方法定義クラス
    ''' </summary>
    Public Class CsvMapping_SpecifiedDiv
        ''' <summary>列番指定</summary>
        Public Shared Index As String = "C"
        ''' <summary>固定値</summary>
        Public Shared Kotei As String = "F"
        ''' <summary>未指定</summary>
        Public Shared MiShitei As String = "Y"
    End Class

    ''' <summary>
    ''' CSVマッピング画面　指定方法定義クラス
    ''' </summary>
    Public Class CsvMapping_ReqItemFlag
        ''' <summary>必須</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>必須でない</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' CSVマッピング画面　グリッド列定義クラス
    ''' </summary>
    Public Class CsvMapping_Col
        Public Shared No As Integer = 0
        Public Shared Koumoku As Integer = 1
        Public Shared ShiteiHouhou As Integer = 2
        Public Shared ColIndex As Integer = 3
        Public Shared FixedVal As Integer = 4
    End Class

    ''' <summary>
    ''' CSVマッピング画面　ラジオボタン並び順定義クラス
    ''' </summary>
    Public Class CsvMapping_Radio
        Public Shared Retsu As Integer = 0
        Public Shared Kotei As Integer = 1
        Public Shared Mishitei As Integer = 2
    End Class

    ''' <summary>
    ''' CSVマッピングマスタ　CSV区分
    ''' </summary>
    Public Class CsvDiv
        ''' <summary>資産</summary>
        Public Shared Asset As String = "ASSET"
        ''' <summary>社員</summary>
        Public Shared Employee As String = "EMPLOYEE"
        ''' <summary>商品</summary>
        Public Shared Goods As String = "GOODS"
        ''' <summary>工事</summary>
        Public Shared Construction As String = "CONSTRUCTION"
    End Class

    ''' <summary>
    ''' CSVマッピング明細マスタ　資産マスタ項目文言コード
    ''' </summary>
    ''' <remarks>
    ''' 資産マスタのCSV取り込み時に作成するデータテーブルの並び順
    ''' ColIdxとCharCdはセットで管理すること。
    ''' </remarks>
    Public Class CsvMapMei_Asset
        Public Class ColIdx
            '20230203 ADD Start Shirakawa
            ''' <summary>処理区分</summary>
            Public Shared Activity As Integer = 0
            '20230203 ADD End Shirakawa
            Public Shared AssetNo As Integer = 1
            ''' <summary>資産ID</summary>
            Public Shared AssetID As Integer = 2
            ''' <summary>資産区分</summary>
            Public Shared AssetDiv As Integer = 3
            ''' <summary>資産名称</summary>
            Public Shared AssetNm As Integer = 4
            ''' <summary>ロケ大区分コード</summary>
            Public Shared LocLCd As Integer = 5
            ''' <summary>ロケ中区分コード</summary>
            Public Shared LocMCd As Integer = 6
            ''' <summary>ロケ小区分コード</summary>
            Public Shared LocSCd As Integer = 7
            ''' <summary>カテゴリ大分類コード</summary>
            Public Shared CatLCd As Integer = 8
            ''' <summary>カテゴリ中分類コード</summary>
            Public Shared CatMCd As Integer = 9
            ''' <summary>カテゴリ小分類コード</summary>
            Public Shared CatSCd As Integer = 10
            ''' <summary>メーカ</summary>
            Public Shared Maker As Integer = 11
            ''' <summary>品番</summary>
            Public Shared ProductNo As Integer = 12
            ''' <summary>型番</summary>
            Public Shared ModelNo As Integer = 13
            ''' <summary>リース管理番号</summary>
            Public Shared LeaseNo As Integer = 14
            ''' <summary>リース期限</summary>
            Public Shared LeaseLimit As Integer = 15
            ''' <summary>○○期限</summary>
            Public Shared GenLimit As Integer = 16
            ''' <summary>フリー項目1</summary>
            Public Shared FreeItem1 As Integer = 17
            ''' <summary>フリー項目2</summary>
            Public Shared FreeItem2 As Integer = 18
            ''' <summary>フリー項目3</summary>
            Public Shared FreeItem3 As Integer = 19
            ''' <summary>フリー項目4</summary>
            Public Shared FreeItem4 As Integer = 20
            ''' <summary>フリー項目5</summary>
            Public Shared FreeItem5 As Integer = 21
            ''' <summary>フリー項目6</summary>
            Public Shared FreeItem6 As Integer = 22
            ''' <summary>フリー項目7</summary>
            Public Shared FreeItem7 As Integer = 23
            ''' <summary>フリー項目8</summary>
            Public Shared FreeItem8 As Integer = 24
            ''' <summary>フリー項目9</summary>
            Public Shared FreeItem9 As Integer = 25
            ''' <summary>フリー項目10</summary>
            Public Shared FreeItem10 As Integer = 26

            '20230203 ADD Start Shirakawa
            ''' <summary>フリー項目11</summary>
            Public Shared FreeItem11 As Integer = 27
            ''' <summary>フリー項目12</summary>
            Public Shared FreeItem12 As Integer = 28
            ''' <summary>フリー項目13</summary>
            Public Shared FreeItem13 As Integer = 29
            ''' <summary>フリー項目14</summary>
            Public Shared FreeItem14 As Integer = 30
            ''' <summary>フリー項目15</summary>
            Public Shared FreeItem15 As Integer = 31
            ''' <summary>フリー項目16</summary>
            Public Shared FreeItem16 As Integer = 32
            ''' <summary>フリー項目17</summary>
            Public Shared FreeItem17 As Integer = 33
            ''' <summary>フリー項目18</summary>
            Public Shared FreeItem18 As Integer = 34
            ''' <summary>フリー項目19</summary>
            Public Shared FreeItem19 As Integer = 35
            '20230203 ADD End Shirakawa

            ''' <summary>資産画像ファイル名</summary>
            Public Shared AssetPicFileNm As Integer = 36
            '--2021/10/04 nv miyaji add start
            ''' <summary>管理ロケー区分コード</summary>
            Public Shared MngLocLCd As Integer = 37
            ''' <summary>貸出状態</summary>
            Public Shared LeaseStatus As Integer = 38
            ''' <summary>メンテ発生貸出回数</summary>
            Public Shared MainteLeaseNum As Integer = 39
            ''' <summary>メンテ発生貸出日数</summary>
            Public Shared MainteLeaseDay As Integer = 40
            ''' <summary>メンテ発生貸出日数稼働日フラグ</summary>
            Public Shared MainteLeaseDayWorkFlg As Integer = 41
            ''' <summary>メンテ日数</summary>
            Public Shared MainteDay As Integer = 42
            ''' <summary>メンテ日数稼働日フラグ</summary>
            Public Shared MainteDayWorkFlg As Integer = 43
            '--2021/10/04 nv miyaji add end

        End Class
        '文字コード
        Public Class CharCd
            '20230203 ADD Start Shirakawa
            ''' <summary>処理区分</summary>
            Public Shared Activity As String = CommonLabel.LABEL_PROCESS_DIV
            '20230203 ADD End Shirakawa
            ''' <summary>資産番号</summary>
            Public Shared AssetNo As String = CommonLabel.LABEL_ASSET_NO
            ''' <summary>資産ID</summary>
            Public Shared AssetID As String = CommonLabel.LABEL_ASSET_ID
            ''' <summary>資産区分</summary>
            Public Shared AssetDiv As String = CommonLabel.LABEL_ASSET_DIV
            ''' <summary>資産名称</summary>
            Public Shared AssetNm As String = CommonLabel.LABEL_ASSET_NAME
            ''' <summary>ロケ大区分コード</summary>
            Public Shared LocLCd As String = CommonLabel.LABEL_CSV_LOC_L_CD
            ''' <summary>ロケ中区分コード</summary>
            Public Shared LocMCd As String = CommonLabel.LABEL_CSV_LOC_M_CD
            ''' <summary>ロケ小区分コード</summary>
            Public Shared LocSCd As String = CommonLabel.LABEL_CSV_LOC_S_CD
            ''' <summary>カテゴリ大分類コード</summary>
            Public Shared CatLCd As String = CommonLabel.LABEL_CSV_CAT_L_CD
            ''' <summary>カテゴリ中分類コード</summary>
            Public Shared CatMCd As String = CommonLabel.LABEL_CSV_CAT_M_CD
            ''' <summary>カテゴリ小分類コード</summary>
            Public Shared CatSCd As String = CommonLabel.LABEL_CSV_CAT_S_CD
            ''' <summary>メーカ</summary>
            Public Shared Maker As String = CommonLabel.LABEL_MAKER
            ''' <summary>品番</summary>
            Public Shared ProductNo As String = CommonLabel.LABEL_PRODUCT_NO
            ''' <summary>型番</summary>
            Public Shared ModelNo As String = CommonLabel.LABEL_MODEL_NO
            ''' <summary>リース管理番号</summary>
            Public Shared LeaseNo As String = CommonLabel.LABEL_LEASE_NO
            ''' <summary>リース期限</summary>
            Public Shared LeaseLimit As String = CommonLabel.LABEL_LEASE_LIMIT
            ''' <summary>○○期限</summary>
            Public Shared GenLimit As String = CommonLabel.LABEL_GENERIC_LIMIT
            ''' <summary>資産購入日</summary>
            Public Shared FreeItem1 As String = CommonLabel.LABEL_ASSET_KOUNYU_DATE
            ''' <summary>フリー項目2</summary>
            Public Shared FreeItem2 As String = CommonLabel.LABEL_FREE_ITEM2
            ''' <summary>フリー項目3</summary>
            Public Shared FreeItem3 As String = CommonLabel.LABEL_FREE_ITEM3
            ''' <summary>フリー項目4</summary>
            Public Shared FreeItem4 As String = CommonLabel.LABEL_FREE_ITEM4
            ''' <summary>フリー項目5</summary>
            Public Shared FreeItem5 As String = CommonLabel.LABEL_FREE_ITEM5
            ''' <summary>フリー項目6</summary>
            Public Shared FreeItem6 As String = CommonLabel.LABEL_FREE_ITEM6
            ''' <summary>フリー項目7</summary>
            Public Shared FreeItem7 As String = CommonLabel.LABEL_FREE_ITEM7
            ''' <summary>フリー項目8</summary>
            Public Shared FreeItem8 As String = CommonLabel.LABEL_FREE_ITEM8
            ''' <summary>フリー項目9</summary>
            Public Shared FreeItem9 As String = CommonLabel.LABEL_FREE_ITEM9
            ''' <summary>フリー項目10</summary>
            Public Shared FreeItem10 As String = CommonLabel.LABEL_FREE_ITEM10

            '20230203 ADD Start Shirakawa
            ''' <summary>フリー項目11</summary>
            Public Shared FreeItem11 As String = CommonLabel.LABEL_FREE_ITEM11
            ''' <summary>フリー項目12</summary>
            Public Shared FreeItem12 As String = CommonLabel.LABEL_FREE_ITEM12
            ''' <summary>フリー項目13</summary>
            Public Shared FreeItem13 As String = CommonLabel.LABEL_FREE_ITEM13
            ''' <summary>フリー項目14</summary>
            Public Shared FreeItem14 As String = CommonLabel.LABEL_FREE_ITEM14
            ''' <summary>フリー項目15</summary>
            Public Shared FreeItem15 As String = CommonLabel.LABEL_FREE_ITEM15
            ''' <summary>フリー項目16</summary>
            Public Shared FreeItem16 As String = CommonLabel.LABEL_FREE_ITEM16
            ''' <summary>フリー項目17</summary>
            Public Shared FreeItem17 As String = CommonLabel.LABEL_FREE_ITEM17
            ''' <summary>フリー項目18</summary>
            Public Shared FreeItem18 As String = CommonLabel.LABEL_FREE_ITEM18
            ''' <summary>フリー項目19</summary>
            Public Shared FreeItem19 As String = CommonLabel.LABEL_FREE_ITEM19
            '20230203 ADD End Shirakawa

            ''' <summary>資産画像ファイル名</summary>
            Public Shared AssetPicFileNm As String = CommonLabel.LABEL_ASSET_PIC_FILENAME
            '--2021/10/04 nv miyaji add start
            ''' <summary>管理ロケ区分コード</summary>
            Public Shared MngLocLCd As String = CommonLabel.LABEL_MNG_LOC_KBN_CD
            ''' <summary>貸出状態</summary>
            Public Shared LeaseStatus As String = CommonLabel.LABEL_LEASE_STATUS
            ''' <summary>メンテ発生貸出回数</summary>
            Public Shared MainteLeaseNum As String = CommonLabel.LABEL_MAINTE_LEASE_NUM
            ''' <summary>メンテ発生貸出日数</summary>
            Public Shared MainteLeaseDay As String = CommonLabel.LABEL_MAINTE_LEASE_DAY
            ''' <summary>メンテ発生貸出日数稼働日フラグ</summary>
            Public Shared MainteLeaseDayWorkFlg As String = CommonLabel.LABEL_MAINTE_LEASE_DAY_WORK_FLG
            ''' <summary>メンテ日数</summary>
            Public Shared MainteDay As String = CommonLabel.LABEL_MAINTE_DAY
            ''' <summary>メンテ日数稼働日フラグ</summary>
            Public Shared MainteDayWorkFlg As String = CommonLabel.LABEL_MAINTE_DAY_WORK_FLG
            '--2021/10/04 nv miyaji add end
        End Class


    End Class
    ''' <summary>
    ''' CSVマッピング明細マスタ　社員マスタ項目文言コード
    ''' </summary>
    ''' <remarks>
    ''' 社員マスタのCSV取り込み時に作成するデータテーブルの並び順
    ''' ColIdxとCharCdはセットで管理すること。
    ''' </remarks>
    Public Class CsvMapMei_Employee
        Public Class ColIdx
            ''' <summary>社員コード</summary>
            Public Shared EmployeeCd As Integer = 0
            ''' <summary>氏名</summary>
            Public Shared EmployeeNm As Integer = 1
            ''' <summary>退職フラグ</summary>
            Public Shared RetireFlg As Integer = 2
            ''' <summary>部署コード</summary>
            Public Shared DepartmentCd As Integer = 3
            ''' <summary>権限区分</summary>
            Public Shared AuthDiv As Integer = 4
            ''' <summary>メールアドレス</summary>
            Public Shared MailAddress As Integer = 5
            ''' <summary>所属拠点1</summary>
            Public Shared HubLocLCd1 As Integer = 6
            ''' <summary>所属拠点2</summary>
            Public Shared HubLocLCd2 As Integer = 7
            ''' <summary>所属拠点3</summary>
            Public Shared HubLocLCd3 As Integer = 8
            ''' <summary>所属拠点4</summary>
            Public Shared HubLocLCd4 As Integer = 9
            ''' <summary>所属拠点5</summary>
            Public Shared HubLocLCd5 As Integer = 10
            ''' <summary>所属拠点6</summary>
            Public Shared HubLocLCd6 As Integer = 11
            ''' <summary>所属拠点7</summary>
            Public Shared HubLocLCd7 As Integer = 12
            ''' <summary>所属拠点8</summary>
            Public Shared HubLocLCd8 As Integer = 13
            ''' <summary>所属拠点9</summary>
            Public Shared HubLocLCd9 As Integer = 14
            ''' <summary>所属拠点10</summary>
            Public Shared HubLocLCd10 As Integer = 15
            ''' <summary>所属拠点11</summary>
            Public Shared HubLocLCd11 As Integer = 16
            ''' <summary>所属拠点12</summary>
            Public Shared HubLocLCd12 As Integer = 17
            ''' <summary>所属拠点13</summary>
            Public Shared HubLocLCd13 As Integer = 18
            ''' <summary>所属拠点14</summary>
            Public Shared HubLocLCd14 As Integer = 19
            ''' <summary>所属拠点15</summary>
            Public Shared HubLocLCd15 As Integer = 20
            ''' <summary>所属拠点16</summary>
            Public Shared HubLocLCd16 As Integer = 21
            ''' <summary>所属拠点17</summary>
            Public Shared HubLocLCd17 As Integer = 22
            ''' <summary>所属拠点18</summary>
            Public Shared HubLocLCd18 As Integer = 23
            ''' <summary>所属拠点19</summary>
            Public Shared HubLocLCd19 As Integer = 24
            ''' <summary>所属拠点20</summary>
            Public Shared HubLocLCd20 As Integer = 25
            ''' <summary>所属拠点21</summary>
            Public Shared HubLocLCd21 As Integer = 26
            ''' <summary>所属拠点22</summary>
            Public Shared HubLocLCd22 As Integer = 27
            ''' <summary>所属拠点23</summary>
            Public Shared HubLocLCd23 As Integer = 28
            ''' <summary>所属拠点24</summary>
            Public Shared HubLocLCd24 As Integer = 29
            ''' <summary>所属拠点25</summary>
            Public Shared HubLocLCd25 As Integer = 30
            ''' <summary>所属拠点26</summary>
            Public Shared HubLocLCd26 As Integer = 31
            ''' <summary>所属拠点27</summary>
            Public Shared HubLocLCd27 As Integer = 32
            ''' <summary>所属拠点28</summary>
            Public Shared HubLocLCd28 As Integer = 33
            ''' <summary>所属拠点29</summary>
            Public Shared HubLocLCd29 As Integer = 34
            ''' <summary>所属拠点30</summary>
            Public Shared HubLocLCd30 As Integer = 35
        End Class
        '文字コード
        Public Class CharCd
            ''' <summary>社員コード</summary>
            Public Shared EmployeeCd As String = CommonLabel.LABEL_EMPLOYEEID
            ''' <summary>氏名</summary>
            Public Shared EmployeeNm As String = CommonLabel.LABEL_EMPLOYEENM
            ''' <summary>退職フラグ</summary>
            Public Shared RetireFlg As String = CommonLabel.LABEL_RETIRE_FLG
            ''' <summary>部署コード</summary>
            Public Shared DepartmentCd As String = CommonLabel.LABEL_BUSHOCODE
            ''' <summary>権限区分</summary>
            Public Shared AuthDiv As String = CommonLabel.LABEL_KENGEN_KBN
            ''' <summary>メールアドレス</summary>
            Public Shared MailAddress As String = CommonLabel.LABEL_MAIL_ADDRESS
            ''' <summary>所属拠点1</summary>
            Public Shared HubLocLCd1 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN1
            ''' <summary>所属拠点2</summary>
            Public Shared HubLocLCd2 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN2
            ''' <summary>所属拠点3</summary>
            Public Shared HubLocLCd3 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN3
            ''' <summary>所属拠点4</summary>
            Public Shared HubLocLCd4 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN4
            ''' <summary>所属拠点5</summary>
            Public Shared HubLocLCd5 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN5
            ''' <summary>所属拠点6</summary>
            Public Shared HubLocLCd6 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN6
            ''' <summary>所属拠点7</summary>
            Public Shared HubLocLCd7 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN7
            ''' <summary>所属拠点8</summary>
            Public Shared HubLocLCd8 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN8
            ''' <summary>所属拠点9</summary>
            Public Shared HubLocLCd9 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN9
            ''' <summary>所属拠点10</summary>
            Public Shared HubLocLCd10 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN10
            ''' <summary>所属拠点11</summary>
            Public Shared HubLocLCd11 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN11
            ''' <summary>所属拠点12</summary>
            Public Shared HubLocLCd12 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN12
            ''' <summary>所属拠点13</summary>
            Public Shared HubLocLCd13 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN13
            ''' <summary>所属拠点14</summary>
            Public Shared HubLocLCd14 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN14
            ''' <summary>所属拠点15</summary>
            Public Shared HubLocLCd15 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN15
            ''' <summary>所属拠点16</summary>
            Public Shared HubLocLCd16 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN16
            ''' <summary>所属拠点17</summary>
            Public Shared HubLocLCd17 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN17
            ''' <summary>所属拠点18</summary>
            Public Shared HubLocLCd18 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN18
            ''' <summary>所属拠点19</summary>
            Public Shared HubLocLCd19 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN19
            ''' <summary>所属拠点20</summary>
            Public Shared HubLocLCd20 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN20
            ''' <summary>所属拠点21</summary>
            Public Shared HubLocLCd21 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN21
            ''' <summary>所属拠点22</summary>
            Public Shared HubLocLCd22 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN22
            ''' <summary>所属拠点23</summary>
            Public Shared HubLocLCd23 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN23
            ''' <summary>所属拠点24</summary>
            Public Shared HubLocLCd24 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN24
            ''' <summary>所属拠点25</summary>
            Public Shared HubLocLCd25 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN25
            ''' <summary>所属拠点26</summary>
            Public Shared HubLocLCd26 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN26
            ''' <summary>所属拠点27</summary>
            Public Shared HubLocLCd27 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN27
            ''' <summary>所属拠点28</summary>
            Public Shared HubLocLCd28 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN28
            ''' <summary>所属拠点29</summary>
            Public Shared HubLocLCd29 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN29
            ''' <summary>所属拠点30</summary>
            Public Shared HubLocLCd30 As String = CommonLabel.LABEL_SHOZOKU_KYOTEN30

        End Class


    End Class

    ''' <summary>
    ''' CSVマッピング明細マスタ　商品マスタ項目文言コード
    ''' </summary>
    ''' <remarks>
    ''' 商品マスタのCSV取り込み時に作成するデータテーブルの並び順
    ''' ColIdxとCharCdはセットで管理すること。
    ''' </remarks>
    Public Class CsvMapMei_Goods
        Public Class ColIdx
            ''' <summary>品番</summary>
            Public Shared ProductNo As Integer = 0
            ''' <summary>資産区分</summary>
            Public Shared AssetDiv As Integer = 1
            ''' <summary>資産名称</summary>
            Public Shared AssetNm As Integer = 2
            ''' <summary>管理ロケ大区分コード</summary>
            Public Shared MngLocLCd As Integer = 3
            ''' <summary>ロケ大区分コード</summary>
            Public Shared LocLCd As Integer = 4
            ''' <summary>ロケ中区分コード</summary>
            Public Shared LocMCd As Integer = 5
            ''' <summary>ロケ小区分コード</summary>
            Public Shared LocSCd As Integer = 6
            ''' <summary>カテゴリ大分類コード</summary>
            Public Shared CatLCd As Integer = 7
            ''' <summary>カテゴリ中分類コード</summary>
            Public Shared CatMCd As Integer = 8
            ''' <summary>カテゴリ小分類コード</summary>
            Public Shared CatSCd As Integer = 9
            ''' <summary>メーカ</summary>
            Public Shared Maker As Integer = 10
            ''' <summary>型番</summary>
            Public Shared ModelNo As Integer = 11
            ''' <summary>リース管理番号</summary>
            Public Shared LeaseNo As Integer = 12
            ''' <summary>リース期限</summary>
            Public Shared LeaseLimit As Integer = 13
            ''' <summary>○○期限</summary>
            Public Shared GenLimit As Integer = 14
            ''' <summary>フリー項目1</summary>
            Public Shared FreeItem1 As Integer = 15
            ''' <summary>フリー項目2</summary>
            Public Shared FreeItem2 As Integer = 16
            ''' <summary>フリー項目3</summary>
            Public Shared FreeItem3 As Integer = 17
            ''' <summary>フリー項目4</summary>
            Public Shared FreeItem4 As Integer = 18
            ''' <summary>フリー項目5</summary>
            Public Shared FreeItem5 As Integer = 19
            ''' <summary>フリー項目6</summary>
            Public Shared FreeItem6 As Integer = 20
            ''' <summary>フリー項目7</summary>
            Public Shared FreeItem7 As Integer = 21
            ''' <summary>フリー項目8</summary>
            Public Shared FreeItem8 As Integer = 22
            ''' <summary>フリー項目9</summary>
            Public Shared FreeItem9 As Integer = 23
            ''' <summary>フリー項目10</summary>
            Public Shared FreeItem10 As Integer = 24
            '2023/03/22 Kawamoto ADD START
            ''' <summary>フリー項目11</summary>
            Public Shared FreeItem11 As Integer = 25
            ''' <summary>フリー項目12</summary>
            Public Shared FreeItem12 As Integer = 26
            ''' <summary>フリー項目13</summary>
            Public Shared FreeItem13 As Integer = 27
            ''' <summary>フリー項目14</summary>
            Public Shared FreeItem14 As Integer = 28
            ''' <summary>フリー項目15</summary>
            Public Shared FreeItem15 As Integer = 29
            ''' <summary>フリー項目16</summary>
            Public Shared FreeItem16 As Integer = 30
            ''' <summary>フリー項目17</summary>
            Public Shared FreeItem17 As Integer = 31
            ''' <summary>フリー項目18</summary>
            Public Shared FreeItem18 As Integer = 32
            ''' <summary>フリー項目19</summary>
            Public Shared FreeItem19 As Integer = 33
            '2023/03/22 Kawamoto ADD END
            ''' <summary>資産画像ファイル名</summary>
            Public Shared AssetPicFileNm As Integer = 34

        End Class
        '文字コード
        Public Class CharCd
            ''' <summary>品番</summary>
            Public Shared ProductNo As String = CommonLabel.LABEL_PRODUCT_NO
            ''' <summary>資産区分</summary>
            Public Shared AssetDiv As String = CommonLabel.LABEL_ASSET_DIV
            ''' <summary>資産名称</summary>
            Public Shared AssetNm As String = CommonLabel.LABEL_ASSET_NAME
            ''' <summary>管理ロケ大区分コード</summary>
            Public Shared MngLocLCd As String = CommonLabel.LABEL_MNG_LOC_KBN_CD
            ''' <summary>ロケ大区分コード</summary>
            Public Shared LocLCd As String = CommonLabel.LABEL_CSV_LOC_L_CD
            ''' <summary>ロケ中区分コード</summary>
            Public Shared LocMCd As String = CommonLabel.LABEL_CSV_LOC_M_CD
            ''' <summary>ロケ小区分コード</summary>
            Public Shared LocSCd As String = CommonLabel.LABEL_CSV_LOC_S_CD
            ''' <summary>カテゴリ大分類コード</summary>
            Public Shared CatLCd As String = CommonLabel.LABEL_CSV_CAT_L_CD
            ''' <summary>カテゴリ中分類コード</summary>
            Public Shared CatMCd As String = CommonLabel.LABEL_CSV_CAT_M_CD
            ''' <summary>カテゴリ小分類コード</summary>
            Public Shared CatSCd As String = CommonLabel.LABEL_CSV_CAT_S_CD
            ''' <summary>メーカ</summary>
            Public Shared Maker As String = CommonLabel.LABEL_MAKER
            ''' <summary>型番</summary>
            Public Shared ModelNo As String = CommonLabel.LABEL_MODEL_NO
            ''' <summary>リース管理番号</summary>
            Public Shared LeaseNo As String = CommonLabel.LABEL_LEASE_NO
            ''' <summary>リース期限</summary>
            Public Shared LeaseLimit As String = CommonLabel.LABEL_LEASE_LIMIT
            ''' <summary>○○期限</summary>
            Public Shared GenLimit As String = CommonLabel.LABEL_GENERIC_LIMIT
            ''' <summary>フリー項目1</summary>
            Public Shared FreeItem1 As String = CommonLabel.LABEL_ASSET_KOUNYU_DATE
            ''' <summary>フリー項目2</summary>
            Public Shared FreeItem2 As String = CommonLabel.LABEL_FREE_ITEM2
            ''' <summary>フリー項目3</summary>
            Public Shared FreeItem3 As String = CommonLabel.LABEL_FREE_ITEM3
            ''' <summary>フリー項目4</summary>
            Public Shared FreeItem4 As String = CommonLabel.LABEL_FREE_ITEM4
            ''' <summary>フリー項目5</summary>
            Public Shared FreeItem5 As String = CommonLabel.LABEL_FREE_ITEM5
            ''' <summary>フリー項目6</summary>
            Public Shared FreeItem6 As String = CommonLabel.LABEL_FREE_ITEM6
            ''' <summary>フリー項目7</summary>
            Public Shared FreeItem7 As String = CommonLabel.LABEL_FREE_ITEM7
            ''' <summary>フリー項目8</summary>
            Public Shared FreeItem8 As String = CommonLabel.LABEL_FREE_ITEM8
            ''' <summary>フリー項目9</summary>
            Public Shared FreeItem9 As String = CommonLabel.LABEL_FREE_ITEM9
            ''' <summary>フリー項目10</summary>
            Public Shared FreeItem10 As String = CommonLabel.LABEL_FREE_ITEM10
            '2023/03/22 Kawamoto ADD START
            ''' <summary>フリー項目11</summary>
            Public Shared FreeItem11 As String = CommonLabel.LABEL_FREE_ITEM11
            ''' <summary>フリー項目12</summary>
            Public Shared FreeItem12 As String = CommonLabel.LABEL_FREE_ITEM12
            ''' <summary>フリー項目13</summary>
            Public Shared FreeItem13 As String = CommonLabel.LABEL_FREE_ITEM13
            ''' <summary>フリー項目14</summary>
            Public Shared FreeItem14 As String = CommonLabel.LABEL_FREE_ITEM14
            ''' <summary>フリー項目15</summary>
            Public Shared FreeItem15 As String = CommonLabel.LABEL_FREE_ITEM15
            ''' <summary>フリー項目16</summary>
            Public Shared FreeItem16 As String = CommonLabel.LABEL_FREE_ITEM16
            ''' <summary>フリー項目17</summary>
            Public Shared FreeItem17 As String = CommonLabel.LABEL_FREE_ITEM17
            ''' <summary>フリー項目18</summary>
            Public Shared FreeItem18 As String = CommonLabel.LABEL_FREE_ITEM18
            ''' <summary>フリー項目19</summary>
            Public Shared FreeItem19 As String = CommonLabel.LABEL_FREE_ITEM19
            '2023/03/22 Kawamoto ADD END
            ''' <summary>資産画像ファイル名</summary>
            Public Shared AssetPicFileNm As String = CommonLabel.LABEL_ASSET_PIC_FILENAME
        End Class


    End Class

    ''' <summary>
    ''' CSVマッピング明細マスタ　工事マスタ項目文言コード
    ''' </summary>
    ''' <remarks>
    ''' 工事マスタのCSV取り込み時に作成するデータテーブルの並び順
    ''' ColIdxとCharCdはセットで管理すること。
    ''' </remarks>
    Public Class CsvMapMei_Construction
        Public Class ColIdx
            ''' <summary>処理区分</summary>
            Public Shared Activity As Integer = 0
            ''' <summary>工事番号</summary>
            Public Shared ConstNo As Integer = 1
            ''' <summary>工事開始日</summary>
            Public Shared ConstStartDt As Integer = 2
            ''' <summary>工事終了日</summary>
            Public Shared ConstEndDt As Integer = 3
            ''' <summary>工事貸出開始日</summary>
            Public Shared ConstLentStartDt As Integer = 4
            ''' <summary>工事貸出終了日</summary>
            Public Shared ConstLentEndDt As Integer = 5
            ''' <summary>工事内容</summary>
            Public Shared ConstContents As Integer = 6
            ''' <summary>工事責任者コード</summary>
            Public Shared ConstEmployeeCd As Integer = 7
            ''' <summary>管理ロケ大区分コード</summary>
            Public Shared MngLocLCd As Integer = 8
            ''' <summary>工事情報1</summary>
            Public Shared ConstInfo1 As Integer = 9
            ''' <summary>工事情報2</summary>
            Public Shared ConstInfo2 As Integer = 10
            ''' <summary>工事情報3</summary>
            Public Shared ConstInfo3 As Integer = 11
            ''' <summary>工事情報4</summary>
            Public Shared ConstInfo4 As Integer = 12
            ''' <summary>工事情報5</summary>
            Public Shared ConstInfo5 As Integer = 13
            ''' <summary>工事情報6</summary>
            Public Shared ConstInfo6 As Integer = 14
            ''' <summary>工事情報7</summary>
            Public Shared ConstInfo7 As Integer = 15
            ''' <summary>工事情報8</summary>
            Public Shared ConstInfo8 As Integer = 16
            ''' <summary>工事情報9</summary>
            Public Shared ConstInfo9 As Integer = 17
            ''' <summary>工事情報10</summary>
            Public Shared ConstInfo10 As Integer = 18
            ''' <summary>工事情報11</summary>
            Public Shared ConstInfo11 As Integer = 19
            ''' <summary>工事情報12</summary>
            Public Shared ConstInfo12 As Integer = 20
            ''' <summary>工事情報13</summary>
            Public Shared ConstInfo13 As Integer = 21
            ''' <summary>工事情報14</summary>
            Public Shared ConstInfo14 As Integer = 22
            ''' <summary>工事情報15</summary>
            Public Shared ConstInfo15 As Integer = 23
            ''' <summary>工事情報16</summary>
            Public Shared ConstInfo16 As Integer = 24
            ''' <summary>工事情報17</summary>
            Public Shared ConstInfo17 As Integer = 25
            ''' <summary>工事情報18</summary>
            Public Shared ConstInfo18 As Integer = 26
            ''' <summary>工事情報19</summary>
            Public Shared ConstInfo19 As Integer = 27
            ''' <summary>工事情報20</summary>
            Public Shared ConstInfo20 As Integer = 28
        End Class
        '文字コード
        Public Class CharCd
            ''' <summary>処理区分</summary>
            Public Shared Activity As String = CommonLabel.LABEL_PROCESS_DIV
            ''' <summary>工事番号</summary>
            Public Shared ConstNo As String = CommonLabel.LABEL_CONST_NO
            ''' <summary>工事開始日</summary>
            Public Shared ConstStartDt As String = CommonLabel.LABEL_CONST_START_DT
            ''' <summary>工事終了日</summary>
            Public Shared ConstEndDt As String = CommonLabel.LABEL_CONST_END_DT
            ''' <summary>工事貸出開始日</summary>
            Public Shared ConstLentStartDt As String = CommonLabel.LABEL_CONST_LENT_START_DT
            ''' <summary>工事貸出終了日</summary>
            Public Shared ConstLentEndDt As String = CommonLabel.LABEL_CONST_LENT_END_DT
            ''' <summary>工事内容</summary>
            Public Shared ConstContents As String = CommonLabel.LABEL_CONST_CONTENTS
            ''' <summary>工事責任者コード</summary>
            Public Shared ConstEmployeeCd As String = CommonLabelConstructionList.LABEL_CONST_EMPLOYEE_CD
            ''' <summary>管理ロケ大区分コード</summary>
            Public Shared MngLocLCd As String = CommonLabelConstructionList.LABEL_MNG_LOC_L_CD
            ''' <summary>工事情報1</summary>
            Public Shared ConstInfo1 As String = CommonLabel.LABEL_CONST_INFO1
            ''' <summary>工事情報2</summary>
            Public Shared ConstInfo2 As String = CommonLabel.LABEL_CONST_INFO2
            ''' <summary>工事情報3</summary>
            Public Shared ConstInfo3 As String = CommonLabel.LABEL_CONST_INFO3
            ''' <summary>工事情報4</summary>
            Public Shared ConstInfo4 As String = CommonLabel.LABEL_CONST_INFO4
            ''' <summary>工事情報5</summary>
            Public Shared ConstInfo5 As String = CommonLabel.LABEL_CONST_INFO5
            ''' <summary>工事情報6</summary>
            Public Shared ConstInfo6 As String = CommonLabel.LABEL_CONST_INFO6
            ''' <summary>工事情報7</summary>
            Public Shared ConstInfo7 As String = CommonLabel.LABEL_CONST_INFO7
            ''' <summary>工事情報8</summary>
            Public Shared ConstInfo8 As String = CommonLabel.LABEL_CONST_INFO8
            ''' <summary>工事情報9</summary>
            Public Shared ConstInfo9 As String = CommonLabel.LABEL_CONST_INFO9
            ''' <summary>工事情報10</summary>
            Public Shared ConstInfo10 As String = CommonLabel.LABEL_CONST_INFO10
            ''' <summary>工事情報11</summary>
            Public Shared ConstInfo11 As String = CommonLabel.LABEL_CONST_INFO11
            ''' <summary>工事情報12</summary>
            Public Shared ConstInfo12 As String = CommonLabel.LABEL_CONST_INFO12
            ''' <summary>工事情報13</summary>
            Public Shared ConstInfo13 As String = CommonLabel.LABEL_CONST_INFO13
            ''' <summary>工事情報14</summary>
            Public Shared ConstInfo14 As String = CommonLabel.LABEL_CONST_INFO14
            ''' <summary>工事情報15</summary>
            Public Shared ConstInfo15 As String = CommonLabel.LABEL_CONST_INFO15
            ''' <summary>工事情報16</summary>
            Public Shared ConstInfo16 As String = CommonLabel.LABEL_CONST_INFO16
            ''' <summary>工事情報17</summary>
            Public Shared ConstInfo17 As String = CommonLabel.LABEL_CONST_INFO17
            ''' <summary>工事情報18</summary>
            Public Shared ConstInfo18 As String = CommonLabel.LABEL_CONST_INFO18
            ''' <summary>工事情報19</summary>
            Public Shared ConstInfo19 As String = CommonLabel.LABEL_CONST_INFO19
            ''' <summary>工事情報20</summary>
            Public Shared ConstInfo20 As String = CommonLabel.LABEL_CONST_INFO20
        End Class
    End Class

    ''' <summary>
    ''' 貸出状態
    ''' </summary>
    Public Class LeaseStatus
        Public Class Cd
            ''' <summary>返却</summary>
            Public Shared Returned As Integer = 1
            ''' <summary>貸出中</summary>
            Public Shared Lending As Integer = 2
            ''' <summary>期限切れ</summary>
            Public Shared LimitExpired As Integer = 3
        End Class
    End Class

    ''' <summary>
    ''' 有効無効フラグ
    ''' </summary>
    Public Class ValidFlg
        Public Class Cd
            ''' <summary>有効</summary>
            Public Shared Valid As String = "Y"
            ''' <summary>無効</summary>
            Public Shared Invalid As String = "N"
        End Class
    End Class

    ''' <summary>
    ''' 本部フラグ
    ''' </summary>
    Public Class HeadOfficeFlg
        Public Class Cd
            ''' <summary>本部</summary>
            Public Shared Honbu As String = "Y"
            ''' <summary>拠点</summary>
            Public Shared Kyoten As String = "N"
        End Class
    End Class

    ''' <summary>
    ''' ゲート通過区分
    ''' </summary>
    Public Class GatePassDiv
        Public Class Cd
            ''' <summary>持出/返却エラー</summary>
            Public Shared ER As String = "ER"
            ''' <summary>入退室</summary>
            Public Shared IO As String = "IO"
            ''' <summary>持出/返却</summary>
            Public Shared LR As String = "LR"
            ''' <summary>システムエラー</summary>
            Public Shared SE As String = "SE"
        End Class
    End Class


    ''' <summary>
    ''' ゲート区分
    ''' </summary>
    Public Class GateDiv
        Public Class Cd
            ''' <summary>WaveGate(ゲートマスタのゲート区分)</summary>
            Public Shared WG As String = "0"
            ''' <summary>SpeedWay(ゲートマスタのゲート区分)</summary>
            Public Shared SW As String = "1"
        End Class
        Public Class Name
            ''' <summary>WaveGate(設定マスタのデータ2)</summary>
            Public Shared WG As String = "Wavegate"
            ''' <summary>SpeedWay(設定マスタのデータ2)</summary>
            Public Shared SW As String = "Speedway"
        End Class
    End Class

    ''' <summary>
    ''' 周波数
    ''' </summary>
    Public Class Frequency
        Public Class Cd
            ''' <summary>916.8Mhz</summary>
            Public Shared FIRST As String = "916.8Mhz"
            ''' <summary>918.0Mhz</summary>
            Public Shared SECOND As String = "918.0Mhz"
            ''' <summary>919.2Mhz</summary>
            Public Shared THIRD As String = "919.2Mhz"
            ''' <summary>920.4Mhz</summary>
            Public Shared FOURTH As String = "920.4Mhz"
        End Class
    End Class

    ''' <summary>
    ''' Searchモード
    ''' </summary>
    Public Class SearchMode
        Public Class Cd
            ''' <summary>ReaderSelected</summary>
            Public Shared ReaderSelected As String = "0"
            ''' <summary>SingleTarget</summary>
            Public Shared SingleTarget As String = "1"
            ''' <summary>DualTarget</summary>
            Public Shared DualTarget As String = "2"
            ''' <summary>TagFocus</summary>
            Public Shared TagFocus As String = "3"
            ''' <summary>SingleTargetReset</summary>
            Public Shared SingleTargetReset As String = "5"
            ''' <summary>DualTargetBtoASelect</summary>
            Public Shared DualTargetBtoASelect As String = "6"
        End Class
        Public Class Name
            ''' <summary>ReaderSelected</summary>
            Public Shared ReaderSelected As String = "ReaderSelected"
            ''' <summary>SingleTarget</summary>
            Public Shared SingleTarget As String = "SingleTarget"
            ''' <summary>DualTarget</summary>
            Public Shared DualTarget As String = "DualTarget"
            ''' <summary>TagFocus</summary>
            Public Shared TagFocus As String = "TagFocus"
            ''' <summary>SingleTargetReset</summary>
            Public Shared SingleTargetReset As String = "SingleTargetReset"
            ''' <summary>DualTargetBtoASelect</summary>
            Public Shared DualTargetBtoASelect As String = "DualTargetBtoASelect"
        End Class
    End Class

    ''' <summary>
    ''' Readerモード
    ''' </summary>
    Public Class ReaderMode
        Public Class Cd
            ''' <summary>MaxThroughput</summary>
            Public Shared MaxThroughput As String = "0"
            ''' <summary>Hybrid</summary>
            Public Shared Hybrid As String = "1"
            ''' <summary>DenseReaderM4</summary>
            Public Shared DenseReaderM4 As String = "2"
            ''' <summary>DenseReaderM8</summary>
            Public Shared DenseReaderM8 As String = "3"
            ''' <summary>MaxMiller</summary>
            Public Shared MaxMiller As String = "4"
            ''' <summary>DenseReaderM4Two</summary>
            Public Shared DenseReaderM4Two As String = "5"
            ''' <summary>AutoSetDenseReader</summary>
            Public Shared AutoSetDenseReader As String = "1000"
            ''' <summary>AutoSetDenseReaderDeepScan</summary>
            Public Shared AutoSetDenseReaderDeepScan As String = "1002"
            ''' <summary>AutoSetStaticFast</summary>
            Public Shared AutoSetStaticFast As String = "1003"
            ''' <summary>AutoSetStaticDenseReader</summary>
            Public Shared AutoSetStaticDenseReader As String = "1004"
        End Class
        Public Class Name
            ''' <summary>MaxThroughput</summary>
            Public Shared MaxThroughput As String = "MaxThroughput"
            ''' <summary>Hybrid</summary>
            Public Shared Hybrid As String = "Hybrid"
            ''' <summary>DenseReaderM4</summary>
            Public Shared DenseReaderM4 As String = "DenseReaderM4"
            ''' <summary>DenseReaderM8</summary>
            Public Shared DenseReaderM8 As String = "DenseReaderM8"
            ''' <summary>MaxMiller</summary>
            Public Shared MaxMiller As String = "MaxMiller"
            ''' <summary>DenseReaderM4Two</summary>
            Public Shared DenseReaderM4Two As String = "DenseReaderM4Two"
            ''' <summary>AutoSetDenseReader</summary>
            Public Shared AutoSetDenseReader As String = "AutoSetDenseReader"
            ''' <summary>AutoSetDenseReaderDeepScan</summary>
            Public Shared AutoSetDenseReaderDeepScan As String = "AutoSetDenseReaderDeepScan"
            ''' <summary>AutoSetStaticFast</summary>
            Public Shared AutoSetStaticFast As String = "AutoSetStaticFast"
            ''' <summary>AutoSetStaticDenseReader</summary>
            Public Shared AutoSetStaticDenseReader As String = "AutoSetStaticDenseReader"
        End Class
    End Class

    ''' <summary>
    ''' 機種
    ''' </summary>
    Public Class DeviceType
        Public Class Cd
            ''' <summary>R420</summary>
            Public Shared R420 As String = "0"
            ''' <summary>R700</summary>
            Public Shared R700 As String = "1"
        End Class
        Public Class Name
            ''' <summary>R420</summary>
            Public Shared R420 As String = "R420"
            ''' <summary>R700</summary>
            Public Shared R700 As String = "R700"
        End Class
    End Class

    ''' <summary>
    ''' GPIO機器
    ''' </summary>
    Public Class GpioDevice
        Public Class Cd
            ''' <summary>Speed_Way</summary>
            Public Shared Speed_Way As String = "0"
            ''' <summary>Dio_Board</summary>
            Public Shared Dio_Board As String = "1"
        End Class
        Public Class Name
            ''' <summary>Speed_Way</summary>
            Public Shared Speed_Way As String = "Speedway"
            ''' <summary>Dio_Board</summary>
            Public Shared Dio_Board As String = "CONTEC DIOボード"
        End Class
    End Class

    ''' <summary>
    ''' 共通拠点フラグ
    ''' </summary>
    Public Class LocShareFlg
        ''' <summary>共通</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>一般拠点</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' 稼働日フラグ
    ''' </summary>
    Public Class WorkDayFlg
        ''' <summary>稼働日考慮</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>稼働日考慮しない</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' 非稼働日フラグ
    ''' </summary>
    Public Class NotWorkFlg
        ''' <summary>非稼働日</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>稼働日</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' 稼働フラグ
    ''' </summary>
    Public Class WorkFlg
        ''' <summary>稼働日</summary>
        Public Shared Kadobi As String = "K"
        ''' <summary>実日</summary>
        Public Shared Jitsubi As String = "J"
    End Class

    ''' <summary>
    ''' 退職フラグ
    ''' </summary>
    Public Class RetireFlg
        Public Class Cd
            ''' <summary>退職済</summary>
            Public Shared Yes As String = "Y"
            ''' <summary>就業中</summary>
            Public Shared No As String = "N"
        End Class
    End Class

    ''' <summary>
    ''' 権限タイプ
    ''' </summary>
    Public Class Authority_Type
        Public Class Cd
            ''' <summary>通常権限</summary>
            Public Shared Nomal As String = "N"
            ''' <summary>拠点権限</summary>
            Public Shared Kyoten As String = "H"
        End Class
    End Class

    ''' <summary>
    ''' 拠点利用区分ごとの詳細な権限区分定義クラス
    ''' </summary>
    Public Class SHOSAI_AUTH_DIV
        ''' <summary>一般者(通常権限)</summary>
        Public Shared Tsujo_Nomal As String = "N"
        ''' <summary>管理者(通常権限)</summary>
        Public Shared Tsujo_Admin As String = "A"
        ''' <summary>拠点担当者(拠点権限)</summary>
        Public Shared Kyoten_Nomal As String = "KN"
        ''' <summary>拠点責任者(拠点権限)</summary>
        Public Shared Kyoten_Admin As String = "KA"
        ''' <summary>本部担当者(拠点権限)</summary>
        Public Shared Honbu_Nomal As String = "HN"
        ''' <summary>本部責任者(拠点権限)</summary>
        Public Shared Honbu_Admin As String = "HA"
        ''' <summary>スーパー管理者</summary>
        Public Shared SuperAdmin As String = "S"
    End Class

    ''' <summary>
    ''' メンテ稼働日フラグ
    ''' </summary>
    Public Class MainteWorkFlg
        ''' <summary>稼働日考慮</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>稼働日考慮しない（=実日数計算)</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' メンテ稼働日フラグ
    ''' </summary>
    Public Class AssetLeaseStatus
        ''' <summary>不可</summary>
        Public Shared Ng As String = "0"
        ''' <summary>可能</summary>
        Public Shared Ok As String = "1"
    End Class

    ''' <summary>
    ''' 予約状況照会画面用並び替え定義クラス
    ''' </summary>
    Public Class LendingStatusSort
        ''' <summary>予約観点</summary>
        Public Shared Reserve As String = "R"
        ''' <summary>メンテナンス観点</summary>
        Public Shared Mainte As String = "M"
    End Class

    ''' <summary>
    ''' 予約一覧画面用日付抽出定義クラス
    ''' </summary>
    Public Class LendingListDateExtract
        ''' <summary>実質貸出期間</summary>
        Public Shared RealLendingPeriod As String = "RP"
        ''' <summary>当日出荷</summary>
        Public Shared SameDayShipping As String = "SS"
        ''' <summary>翌日出荷</summary>
        Public Shared NextDayShipping As String = "NS"
        ''' <summary>実質貸出開始日</summary>
        Public Shared RealLendingStartDate As String = "RS"
    End Class

    ''' <summary>
    ''' 権限による処理区分
    ''' </summary>
    Public Class AuthProcFlg
        ''' <summary>照会・編集可能</summary>
        Public Shared All As String = "照会・編集可能"
        ''' <summary>照会のみ</summary>
        Public Shared ShokaiNomi As String = "照会のみ"
        ''' <summary>照会・編集可能(自拠点)</summary>
        Public Shared JiAll As String = "照会・編集可能(自拠点)"
        ''' <summary>照会のみ(自拠点)</summary>
        Public Shared JiShokaiNomi As String = "照会のみ(自拠点)"
    End Class

    ''' <summary>
    ''' 共有拠点フラグ
    ''' </summary>
    Public Class SharedFlg
        ''' <summary>共有拠点</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>専用拠点</summary>
        Public Shared No As String = "N"
        ''' <summary>他拠点</summary>
        Public Shared Other As String = "A"
    End Class

    ''' <summary>
    ''' 更新権限フラグ
    ''' </summary>
    Public Class UpdFlg
        ''' <summary>更新可能</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>更新不可</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' 貸出状態(貸出可能かどうか)
    ''' </summary>
    Public Class LeaseStatusAsset
        ''' <summary>不可</summary>
        Public Shared Fuka As Integer = 0
        ''' <summary>可能</summary>
        Public Shared Kanou As Integer = 1
    End Class

    ''' <summary>
    ''' 貸出状態(貸出可能かどうか) CSV出力・取込用
    ''' </summary>
    Public Class LeaseStatusAsset_Csv
        ''' <summary>不可</summary>
        Public Shared Fuka As String = "N"
        ''' <summary>可能</summary>
        Public Shared Kanou As String = "Y"
    End Class

    ''' <summary>
    ''' 予約ステータス
    ''' </summary>
    Public Class ReserveStatus
        ''' <summary>未貸出</summary>
        Public Shared Mi As String = "0"
        ''' <summary>貸出中</summary>
        Public Shared Lending As String = "1"
        ''' <summary>返却済み</summary>
        Public Shared HenkyakuSumi As String = "2"
    End Class

    ''' <summary>
    ''' 予約ステータス（予約状況照会表示用）
    ''' </summary>
    Public Class ReserveDspStat
        ''' <summary>未貸出</summary>
        Public Shared Mi As String = "0"
        ''' <summary>貸出中</summary>
        Public Shared Lending As String = "1"
        ''' <summary>貸出中(遅延)</summary>
        Public Shared Delay As String = "9"
        ''' <summary>返却済み</summary>
        Public Shared HenkyakuSumi As String = "2"
    End Class


    ''' <summary>
    ''' 資産貸出状態定義クラス
    ''' </summary>
    Public Class AssetReserveInfo
        ''' <summary>なし</summary>
        Public Shared None As String = " "
        ''' <summary>休日</summary>
        Public Shared Holiday As String = "H"
        ''' <summary>予約中</summary>
        Public Shared Reserve As String = "R"
        ''' <summary>貸出中</summary>
        Public Shared Lease As String = "L"
        ''' <summary>メンテナンス</summary>
        Public Shared Mainte As String = "1"
        ''' <summary>返却済み</summary>
        Public Shared Henkyaku As String = "F"
        ''' <summary>〇〇登録</summary>
        Public Shared blaRegist As String = "2"
        ''' <summary>予備登録１</summary>
        Public Shared preRegist1 As String = "3"
        ''' <summary>予備登録２</summary>
        Public Shared preRegist2 As String = "4"
    End Class

    ''' <summary>
    ''' 引継区分
    ''' </summary>
    Public Class HandOverDiv
        ''' <summary>引継元</summary>
        Public Shared HikitsugiMoto As String = "D"
        ''' <summary>引継先</summary>
        Public Shared HikitsugiSaki As String = "S"
        ''' <summary>未引継</summary>
        Public Shared Mi As String = "N"
    End Class

    ''' <summary>
    ''' メール区分定義クラス
    ''' </summary>
    Public Class MAIL_DIV
        ''' <summary>1:事前返却</summary>
        Public Shared JizenHenkyaku As Integer = 1
        ''' <summary>2:返却遅延</summary>
        Public Shared HenkyakuChien As Integer = 2
        ''' <summary>3:延長通知</summary>
        Public Shared Encho As Integer = 3
        ''' <summary>4:横持通知</summary>
        Public Shared Yokomochi As Integer = 4
        ''' <summary>5:出荷通知</summary>
        Public Shared Shukka As Integer = 5
        ''' <summary>99：システムエラー</summary>
        Public Shared SysErr As Integer = 99
    End Class

    ''' <summary>
    ''' メール区分名定義クラス
    ''' </summary>
    Public Class MAIL_DIV_NM
        ''' <summary>1:事前返却</summary>
        Public Shared JizenHenkyaku As String = "事前返却"
        ''' <summary>2:返却遅延</summary>
        Public Shared HenkyakuChien As String = "返却遅延"
        ''' <summary>3:延長通知</summary>
        Public Shared Encho As String = "延長通知"
        ''' <summary>4:横持通知</summary>
        Public Shared Yokomochi As String = "横持通知"
        ''' <summary>5:出荷通知</summary>
        Public Shared Shukka As String = "出荷通知"
        ''' <summary>99：システムエラー</summary>
        Public Shared SysErr As String = "システムエラー"
    End Class

    ''' <summary>
    ''' メール送信ステータス
    ''' </summary>
    Public Class MAIL_STATUS
        ''' <summary>0:未送信</summary>
        Public Shared Mi As String = "0"
        ''' <summary>1:送信済み</summary>
        Public Shared Sumi As String = "1"
    End Class

    ''' <summary>
    ''' メール送信区分
    ''' </summary>
    Public Class MAIL_SEND_DIV
        ''' <summary>TO</summary>
        Public Shared MailTO As String = "TO"
        ''' <summary>CC</summary>
        Public Shared CC As String = "CC"
        ''' <summary>BCC</summary>
        Public Shared BCC As String = "BCC"
    End Class

    ''' <summary>
    ''' 画面表示区分
    ''' </summary>
    Public Class DispKbn
        ''' <summary>新規登録</summary>
        Public Shared Create As Integer = 1
        ''' <summary>変更</summary>
        Public Shared Edit As Integer = 2
        ''' <summary>照会</summary>
        Public Shared Disp As Integer = 3
    End Class

    ''' <summary>
    ''' メンテナンス分
    ''' </summary>
    Public Class MainteKbn
        ''' <summary>メンテナンス</summary>
        Public Shared Mainte As String = "1"
        ''' <summary>〇〇登録</summary>
        Public Shared BlaRegist As String = "2"
        ''' <summary>予備登録１</summary>
        Public Shared PreRegist1 As String = "3"
        ''' <summary>予備登録２</summary>
        Public Shared PreRegist2 As String = "4"
    End Class


#Region "列表示フラグ"
    Public Class DispFlag
        ''' <summary>表示</summary>
        Public Const VISIBLE As String = "1"
        ''' <summary>非表示</summary>
        Public Const INVISIBLE As String = "0"
    End Class
#End Region

    ''' <summary>
    ''' 反映ボタン表示フラグ
    ''' </summary>
    Public Class ReflectFlg
        ''' <summary>反映ボタン表示</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>反映ボタン非表示</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' 送り状出力データ区分
    ''' </summary>
    Public Class InvoiceOutputDiv
        ''' <summary>データ置換</summary>
        Public Shared Replace As String = "R"
        ''' <summary>データ置換（文字列カットあり）</summary>
        Public Shared Cut As String = "C"
        ''' <summary>固定値</summary>
        Public Shared Fix As String = "F"
        ''' <summary>未指定</summary>
        Public Shared Yet As String = "Y"
    End Class

    ''' <summary>
    ''' 遷移元画面区分
    ''' </summary>
    Public Class SenimotoKbn
        ''' <summary>予約状況照会</summary>
        Public Shared LendingStatus As Integer = 1
        ''' <summary>予約一覧</summary>
        Public Shared LendingList As Integer = 2
    End Class

    ''' <summary>
    ''' メール送信可否
    ''' </summary>
    Public Class MailSendFlg
        ''' <summary>実施する</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>実施しない</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' メンテナンス実施フラグ
    ''' </summary>
    Public Class MainteExecFlg
        ''' <summary>メンテナンス要</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>メンテナンス不要</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' メンテナンス区分
    ''' </summary>
    Public Class MainteDiv
        ''' <summary>そのまま登録可能</summary>
        Public Shared Ok As String = "N"
        ''' <summary>メンテ期間短縮</summary>
        Public Shared Shrt As String = "S"
        ''' <summary>メンテ期間削除</summary>
        Public Shared Del As String = "D"
        ''' <summary>期限日変更</summary>
        Public Shared Chg As String = "C"
        ''' <summary>期間内の予約不可</summary>
        Public Shared Ng As String = "E"
    End Class

    ''' <summary>
    ''' 予約確認書出力フラグ
    ''' </summary>
    Public Class LendingConfirmOutputFlg
        ''' <summary>実施する</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>実施しない</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' 予約確認ラベル出力フラグ
    ''' </summary>
    Public Class LendingLabelFunctionFlg
        ''' <summary>実施する</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>実施しない</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' 送り状出力フラグ
    ''' </summary>
    Public Class LendingInvoiceFunctionFlg
        ''' <summary>実施する</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>実施しない</summary>
        Public Shared No As String = "N"
    End Class

    ''' <summary>
    ''' RFIDタグ拠点フラグ　2025/10/14 ADD Kudo
    ''' </summary>
    Public Class LocRfidFlg
        ''' <summary>Y：RFIDタグ拠点</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>N：バーコード運用拠点</summary>
        Public Shared No As String = "N"
    End Class
    Public Class OwnInvFlg
        ''' <summary>Y：担当者が管理している資産</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>N：権限のある資産　大区分の場合のみ有効</summary>
        Public Shared No As String = "N"
    End Class

#Region "予約状況照会カレンダー色定義"
    ''' <summary>
    ''' 予約状況照会カレンダー色定義
    ''' </summary>
    Public Class CalendarColor
        ''' <summary>なし</summary>
        Public Shared None As String = "background-color:#FFFFFF;"
        ''' <summary>予約中</summary>
        Public Shared Reserve As String = "background-color:#0070c0;"
        ''' <summary>貸出中</summary>
        Public Shared Lease As String = "background-color:#ffc000;"
        ''' <summary>メンテナンス</summary>
        Public Shared Mainte As String = "background-color:#ff0000;"
        ''' <summary>返却済</summary>
        Public Shared Henkyaku As String = "background-color:#a6a6a6;"
        ''' <summary>休日</summary>
        Public Shared Holiday As String = "background-color:#ffe3e3;"
    End Class
#End Region

#Region "CSV取込　処理区分"
    ''' <summary>処理区分 2022/07/25 add sv sato </summary>
    Public Class CsvImportProcDiv
        ''' <summary>追加</summary>
        Public Shared Insert As String = "I"
        ''' <summary>更新</summary>
        Public Shared Update As String = "U"
        ''' <summary>削除</summary>
        Public Shared Delete As String = "D"
        ''' <summary>除却</summary>
        Public Shared Retirement As String = "R"   '2023/02/08 Kawamoto ADD
    End Class
#End Region

#Region "共通構造体定義"

    ''' <summary>
    ''' データ取込画面　結果表示用クラス
    ''' </summary>
    Public Class DataInportResult
        ''' <summary>取込結果</summary>
        Public ResultCd As Integer = 0
        ''' <summary>データ行</summary>
        Public ErrRowIdx As Integer? = 0
        ''' <summary>データ項目</summary>
        Public ErrKoumoku As String = String.Empty
        ''' <summary>エラー概要</summary>
        Public ErrGaiyou As String = String.Empty
        ''' <summary>削除対象件数</summary>
        ''' <remarks>ResultCd が ImportDeleteConfirm のとき、確認メッセージに埋め込む件数を返す。</remarks>
        Public DeleteCount As Integer = 0
        ''' <summary>取込ごとのキー（GUID）</summary>
        ''' <remarks>ResultCd が ImportDeleteConfirm のとき、確認後の「はい」「いいえ」で画面から送り返すキーを返す。</remarks>
        Public ImportKey As String = String.Empty
    End Class

#End Region

    '2023/02/08 Kawamoto ADD START
#Region "資産CSVファイル取込全件対象"
    ''' <summary>資産CSVファイル取込全件対象 </summary>
    Public Class AssetCSVRegistAll
        ''' <summary>全件対象</summary>
        Public Shared All As String = "Y"
        ''' <summary>差分取込</summary>
        Public Shared Difference As String = "N"
    End Class
#End Region

#Region "薬品管理機能"
    ''' <summary>薬品管理機能 </summary>
    Public Class DrugsFunction
        ''' <summary>薬品管理機能あり</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>機能なし</summary>
        Public Shared No As String = "N"
    End Class
#End Region

#Region "計量機"
    ''' <summary>計量機 </summary>
    Public Class DrugsWeighingMachine
        Public Class Data2
            ''' <summary>表示する</summary>
            Public Shared Yes As String = "Y"
            ''' <summary>表示しない</summary>
            Public Shared No As String = "N"
        End Class
    End Class
#End Region

#Region "有無"
    ''' <summary>有無 </summary>
    Public Class Exist
        ''' <summary>Y</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>N</summary>
        Public Shared No As String = "N"
        ''' <summary>-</summary>
        Public Shared None As String = "-"
    End Class
#End Region

#Region "危険物の指定数量"
    ''' <summary>危険物の指定数量 </summary>
    Public Class DrugsFSActNumber
        Public Class Data3
            ''' <summary>単位：L</summary>
            Public Shared L As String = "L"
            ''' <summary>単位：KG</summary>
            Public Shared KG As String = "KG"
        End Class
    End Class
#End Region

#Region "薬品作業区分"
    ''' <summary>薬品作業区分 </summary>
    Public Class DrugsWorkDiv
        ''' <summary>受入</summary>
        Public Shared Accept As String = "1"
        ''' <summary>返却</summary>
        Public Shared Returned As String = "2"
        ''' <summary>確認</summary>
        Public Shared Confirm As String = "3"
    End Class
#End Region

#Region "毒物及び劇薬取扱法"
    ''' <summary>毒物及び劇薬取扱法</summary>
    Public Class PoisonAndDeleterious
        ''' <summary>毒物</summary>
        Public Shared Poison As String = "1"
        ''' <summary>劇物</summary>
        Public Shared Deleterious As String = "2"
        ''' <summary>-</summary>
        Public Shared None As String = "-"
    End Class
#End Region
    '2023/02/08 Kawamoto ADD END

    '2023/03/22 Kawamoto ADD START
#Region "添付ファイル管理画面　定数定義クラス"
    ''' <summary>
    ''' 添付ファイル管理画面　定数定義クラス
    ''' </summary>
    Public Class AttachmentManagement
        ''' <summary>
        ''' 画面に表示する最大件数
        ''' </summary>
        Public Const DISP_ROW_COUNT As Integer = 10

    End Class

    ''' <summary>
    ''' 添付ファイル管理画面 一覧表示用クラス
    ''' </summary>
    Public Class AttachmentInfo
        ''' <summary>
        ''' 表示項目：ファイル名
        ''' </summary>
        ''' <returns>ファイル名</returns>
        Public Property FileNm As String = String.Empty

        ''' <summary>
        ''' 表示項目：バイト数
        ''' </summary>
        ''' <returns>バイト数</returns>
        Public Property ByteCount As String = String.Empty

        ''' <summary>
        ''' 表示項目：ファイル更新日時
        ''' </summary>
        ''' <returns>ファイル更新日時</returns>
        Public Property FileUpdateTime As String = String.Empty

        ''' <summary>
        ''' ファイルダウンロードリンク
        ''' </summary>
        ''' <returns>ファイルダウンロードリンク</returns>
        Public Property FileDownloadLink As String = String.Empty
    End Class

#End Region

#Region "お知らせダウンロード　定数定義クラス"
    ''' <summary>
    ''' お知らせダウンロード　定数定義クラス
    ''' </summary>
    Public Class NewsDownload
        ''' <summary>
        ''' 画面に表示する最大件数
        ''' </summary>
        Public Const DISP_ROW_COUNT As Integer = 20

    End Class
#End Region

#Region "二次貸出先フラグ"
    ''' <summary>
    ''' 二次貸出先フラグ
    ''' </summary>
    Public Class SecLeasePlaceFlg
        Public Class Cd
            ''' <summary>二次貸出先</summary>
            Public Shared Yes As String = "Y"
            ''' <summary>二次貸出先ではない</summary>
            Public Shared No As String = "N"
        End Class
    End Class
#End Region

    '2023/03/22 Kawamoto ADD END

#Region "QR台帳種別"
    Public Class QrLedgerType
        ''' <summary>標準</summary>
        Public Shared Standard As String = "1"
        ''' <summary>カスタム1</summary>
        Public Shared Cunstom1 As String = "2"
    End Class
#End Region

    '20230904 ADD Start Shirakawa
    ''' <summary>
    ''' 一覧表示確認フラグ
    ''' </summary>
    Public Class RedeDataFlg
        ''' <summary>表示する</summary>
        Public Shared Yes As String = "Y"
        ''' <summary>表示しない</summary>
        Public Shared No As String = "N"
    End Class
    '20230904 ADD End Shirakawa

    ''' <summary>
    ''' 仮置きフラグ
    ''' </summary>
    Public Class TempFlg
        Public Class Cd
            ''' <summary>仮置き中</summary>
            Public Shared Yes As String = "Y"
            ''' <summary>仮置きしていない</summary>
            Public Shared No As String = "N"
        End Class
    End Class

    ''' <summary>
    ''' 前回仮置きフラグ
    ''' </summary>
    Public Class PrevTempFlg
        Public Class Cd
            ''' <summary>前回仮置きした</summary>
            Public Shared Yes As String = "Y"
            ''' <summary>前回仮置きしていない</summary>
            Public Shared No As String = "N"
        End Class
    End Class

    ''' <summary>
    ''' 指定品番予約可能数取得ストアドファンクション　処理結果定義クラス
    ''' </summary>
    Public Class AssetReplaceReservePossibleSpResult
        ''' <summary>成功</summary>
        Public Shared Success As String = 1
        ''' <summary>エラー[予約変更NG]</summary>
        Public Shared ChangeError As String = 8
        ''' <summary>エラー[予約期間NG]</summary>
        Public Shared TermError As String = 9
    End Class

    ''' <summary>
    ''' 資産台帳取込ストアドファンクション　処理結果定義クラス
    ''' </summary>
    Public Class ImportAssetFileResult
        ''' <summary>成功</summary>
        Public Shared Success As String = 1
        ''' <summary>登録担当者なしエラー</summary>
        Public Shared SpEmployeeErr As String = 6
        ''' <summary>ロケマスタなしエラー </summary>
        Public Shared SpLocMaterErr As String = 7
        ''' <summary>引数エラー</summary>
        Public Shared ParamErr As String = 8
        ''' <summary>予期しない</summary>
        Public Shared SpSystemErr As String = 9
    End Class


#Region "項目配置マスタメンテ　画面ID"
    ''' <summary>
    ''' 項目配置マスタメンテ　画面ID
    ''' </summary>
    Public Class DispId
        ''' <summary>資産一覧</summary>
        Public Const ASSET_LIST As String = "ASSET_LIST"
        ''' <summary>資産ラベル発行</summary>
        Public Const ASSET_LABEL_PRINT As String = "ASSET_LABEL_PRINT"
        ''' <summary>部署マスタ一覧</summary>
        Public Const DEPARTMENT As String = "DEPARTMENT"
        ''' <summary>資産履歴照会(番号指定)上部</summary>
        Public Const ASSET_DETAIL_TOP As String = "ASSET_DETAIL_TOP"
        ''' <summary>資産履歴照会(番号指定)下部</summary>
        Public Const ASSET_DETAIL_BOTTOM As String = "ASSET_DETAIL_BOTTOM"
        ''' <summary>資産在庫照会</summary>
        Public Const STOCK_LIST As String = "STOCK_LIST"
        ''' <summary>資産履歴照会</summary>
        Public Const ASSET_HISTORY As String = "ASSET_HISTORY"
        ''' <summary>管理簿出力</summary>
        Public Const DRUG_MANAGE_LIST As String = "DRUG_MANAGE_LIST"
        ''' <summary>工事一覧</summary>
        Public Const CONSTRUCTION_LIST As String = "CONSTRUCTION_LIST"
        ''' <summary>工事資産一覧上部</summary>
        Public Const CONST_ASSET_LIST_TOP As String = "CONST_ASSET_LIST_TOP"
        ''' <summary>工事資産一覧下部</summary>
        Public Const CONST_ASSET_LIST_BOTTOM As String = "CONST_ASSET_LIST_BOTTOM"
        ''' <summary>工事履歴一覧上部</summary>
        Public Const CONSTRUCTION_HIST_TOP As String = "CONSTRUCTION_HIST_TOP"
        ''' <summary>工事履歴一覧下部</summary>
        Public Const CONSTRUCTION_HIST_BOTTOM As String = "CONSTRUCTION_HIST_BOTTOM"
        ''' <summary>工事資産履歴一覧上部</summary>
        Public Const CONST_ASSET_HIST_TOP As String = "CONST_ASSET_HIST_TOP"
        ''' <summary>工事資産履歴一覧中部</summary>
        Public Const CONST_ASSET_HIST_MIDDLE As String = "CONST_ASSET_HIST_MIDDLE"
        ''' <summary>工事資産履歴一覧下部</summary>
        Public Const CONST_ASSET_HIST_BOTTOM As String = "CONST_ASSET_HIST_BOTTOM"

        ''' <summary>仮置一覧</summary>
        Public Const TEMPORARY_LIST As String = "TEMPORARY_LIST"
        ''' <summary>仮置資産一覧上部</summary>
        Public Const TEMP_ASSET_LIST_TOP As String = "TEMP_ASSET_LIST_TOP"
        ''' <summary>仮置資産一覧下部</summary>
        Public Const TEMP_ASSET_LIST_BOTTOM As String = "TEMP_ASSET_LIST_BOTTOM"
        ''' <summary>貸出先一覧</summary>
        Public Const ASSET_LEASE_PLACE_LIST As String = "ASSET_LEASE_PLACE_LIST"
        ''' <summary>貸出先資産照会</summary>
        Public Const ASSET_LEASE_PLACE_DETAIL As String = "ASSET_LEASE_PLACE_DETAIL"
        ''' <summary>予約状況照会</summary>
        Public Const LENDING_STATUS As String = "LENDING_STATUS"
        ''' <summary>予約申請</summary>
        Public Const LENDING_ENTRY As String = "LENDING_ENTRY"
        ''' <summary>予約履歴(上部)</summary>
        Public Const LENDING_HISTORY_TOP As String = "LENDING_HISTORY_TOP"
        ''' <summary>予約履歴(下部)</summary>
        Public Const LENDING_HISTORY_BOTTOM As String = "LENDING_HISTORY_BOTTOM"
        ''' <summary>予約一覧</summary>
        Public Const LENDING_LIST As String = "LENDING_LIST"
        ''' <summary>貸出履歴照会</summary>
        Public Const ASSET_LEASE_HITORY As String = "ASSET_LEASE_HITORY"
        ''' <summary>棚卸進捗状況</summary>
        Public Const INVENTORY_SUMMARY As String = "INVENTORY_SUMMARY"
        ''' <summary>実棚照会</summary>
        Public Const INVENTORY_INQUIRY As String = "INVENTORY_INQUIRY"
        ''' <summary>棚卸実績一覧</summary>
        Public Const INVENTORY_HISTORY As String = "INVENTORY_HISTORY"
        ''' <summary>棚卸ロケ別実績一覧(上部)</summary>
        Public Const INVENTORY_LOC_HISTORY_TOP As String = "INVENTORY_LOC_HISTORY_TOP"
        ''' <summary>棚卸ロケ別実績一覧(下部)</summary>
        Public Const INVENTORY_LOC_HISTORY_BOTTOM As String = "INVENTORY_LOC_HISTORY_BOTTOM"
        ''' <summary>棚卸実績照会</summary>
        Public Const INVENTORY_ASSET_HISTORY As String = "INVENTORY_ASSET_HISTORY"
        ''' <summary>カテゴリーマスタメンテ</summary>
        Public Const CATEGORY As String = "CATEGORY"
        ''' <summary>ロケーションマスタメンテ</summary>
        Public Const LOCATION As String = "LOCATION"
        ''' <summary>社員マスタメンテ</summary>
        Public Const EMPLOYEE As String = "EMPLOYEE"
        ''' <summary>貸出先マスタメンテ</summary>
        Public Const LEASE_PLACE As String = "LEASE_PLACE"
        ''' <summary>設定マスタメンテ</summary>
        Public Const SETTING As String = "SETTING"
        ''' <summary>文言マスタメンテ</summary>
        Public Const WORD As String = "WORD"
        ''' <summary>CSVマッピング</summary>
        Public Const CSV_MAPPING As String = "CSV_MAPPING"
        ''' <summary>カレンダー</summary>
        Public Const CALENDAR As String = "CALENDAR"
        ''' <summary>ゲートマスタメンテ(WaveGate)</summary>
        Public Const GATE_WAVE_GATE As String = "GATE_WAVE_GATE"
        ''' <summary>ゲートマスタメンテ(SpeedWay)</summary>
        Public Const GATE_SPEED_WAY As String = "GATE_SPEED_WAY"
        ''' <summary>ゲート持出履歴(WaveGate)</summary>
        Public Const GATE_PASS_LOG_WAVE_GATE As String = "GATE_PASS_LOG_WAVE_GATE"
        ''' <summary>ゲート持出履歴(SpeedWay)</summary>
        Public Const GATE_PASS_LOG_SPEED_WAY As String = "GATE_PASS_LOG_SPEED_WAY"
        ''' <summary>操作履歴</summary>
        Public Const OPERATION_LOG As String = "OPERATION_LOG"
        ''' <summary>商品マスタメンテ</summary>
        Public Const GOODS As String = "GOODS"
    End Class
#End Region

#Region "項目配置マスタメンテ　ロケ大区分(TEMPLATE,ALL)"
    ''' <summary>
    ''' 項目配置マスタメンテ　ロケ大区分(TEMPLATE,ALL)
    ''' </summary>
    Public Class COLSET_LOC
        ''' <summary>全て</summary>
        Public Const ALL As String = "ALL"
        ''' <summary>全て</summary>
        Public Const TEMPLATE As String = "TEMPLATE"
    End Class
#End Region

#Region "画面区分"
    ''' <summary>
    ''' 画面区分
    ''' </summary>
    Public Class DispDiv
        ''' <summary>資産一覧</summary>
        Public Const ASSET_LIST As String = "ASSET_LIST"
        ''' <summary>資産ラベル発行</summary>
        Public Const ASSET_LABEL_PRINT As String = "ASSET_LABEL_PRINT"
        ''' <summary>資産在庫照会</summary>
        Public Const STOCK_LIST As String = "STOCK_LIST"
        ''' <summary>部署マスタメンテ</summary>
        Public Const DEPARTMENT As String = "DEPARTMENT"
        ''' <summary>項目配置マスタメンテ</summary>
        Public Const COL_SET As String = "COL_SET"
        ''' <summary>カテゴリーマスタメンテ</summary>
        Public Const CATEGORY As String = "CATEGORY"
        ''' <summary>ロケーションマスタメンテ</summary>
        Public Const LOCATION As String = "LOCATION"
        ''' <summary>CSVマッピング</summary>
        Public Const CSV_MAPPING As String = "CSV_MAPPING"
        ''' <summary>社員マスタメンテ</summary>
        Public Const EMPLOYEE As String = "EMPLOYEE"
        ''' <summary>貸出先マスタメンテ</summary>
        Public Const LEASE_PLACE As String = "LEASE_PLACE"
        ''' <summary>貸出先マスタメンテ</summary>
        Public Const SETTING As String = "SETTING"
        ''' <summary>文言設定マスタメンテ</summary>
        Public Const WORD As String = "WORD"
    End Class
#End Region

#Region "SlickGrid列キー"
    ''' <summary>
    ''' SlickGrid列キー
    ''' </summary>
    Public Class ColKey
        ''' <summary>資産一覧</summary>
        Public Class AssetList
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>変更/参照</summary>
            Public Const CHANGE_OR_DISP As String = "_CHANGE_OR_DISP"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>サブ資産番号</summary>
            Public Const SUB_ASSET_NO As String = "_SUB_ASSET_NO"
            ''' <summary>資産名</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>資産購入日</summary>
            Public Const ASSET_KOUNYU_DATE As String = "_ASSET_KOUNYU_DATE"
            ''' <summary>管理ロケーションコード/名</summary>
            Public Const MNG_LOC_L_CD_NAMED As String = "_MNG_LOC_L_CD_NAME"
            ''' <summary>資産状態</summary>
            Public Const ASSET_STATUS As String = "_ASSET_STATUS"
            ''' <summary>貸出状態</summary>
            Public Const LEASE_STATUS As String = "_LEASE_STATUS"
            ''' <summary>貸出回数</summary>
            Public Const LEASE_COUNT As String = "_LEASE_COUNT"
            ''' <summary>貸出期間</summary>
            Public Const LEASE_KIKAN As String = "_LEASE_KIKAN"
            ''' <summary>貸出先コード/名</summary>
            Public Const LEASE_PLACE_CD_NAME As String = "_LEASE_PLACE_CD_NAME"
            ''' <summary>貸出先備考</summary>
            Public Const LEASE_REMARK As String = "_LEASE_REMARK"
            ''' <summary>貸出担当者コード/名</summary>
            Public Const LEASE_EMPLOYEE_CD_NAME As String = "_LEASE_EMPLOYEE_CD_NAME"
            ''' <summary>メンテ発生貸出回数</summary>
            Public Const MAINTE_LEASE_NUM As String = "MAINTE_LEASE_NUM"
            ''' <summary>メンテ発生貸出日数</summary>
            Public Const MAINTE_LEASE_DAY As String = "MAINTE_LEASE_DAY"
            ''' <summary>メンテ日数</summary>
            Public Const MAINTE_DAY As String = "MAINTE_DAY"
            ''' <summary>ロケーション大区分コード/名</summary>
            Public Const LOC_L_CD_NAME As String = "_LOC_L_CD_NAME"
            ''' <summary>ロケーション中区分コード/名</summary>
            Public Const LOC_M_CD_NAME As String = "_LOC_M_CD_NAME"
            ''' <summary>ロケーション小区分コード/名</summary>
            Public Const LOC_S_CD_NAME As String = "_LOC_S_CD_NAME"
            ''' <summary>カテゴリ大分類コード/名</summary>
            Public Const CAT_L_CD_NAME As String = "_CAT_L_CD_NAME"
            ''' <summary>カテゴリ中分類コード/名</summary>
            Public Const CAT_M_CD_NAME As String = "_CAT_M_CD_NAME"
            ''' <summary>カテゴリ小分類コード/名</summary>
            Public Const CAT_S_CD_NAME As String = "_CAT_S_CD_NAME"
            ''' <summary>○○期限</summary>
            Public Const GENERIC_LIMIT As String = "_GENERIC_LIMIT"
            ''' <summary>資産製造元</summary>
            Public Const MAKER As String = "_MAKER"
            ''' <summary>品番</summary>
            Public Const PRODUCT_NO As String = "_PRODUCT_NO"
            ''' <summary>型番</summary>
            Public Const MODEL_NO As String = "_MODEL_NO"
            ''' <summary>資産区分</summary>
            Public Const ASSET_DIV As String = "_ASSET_DIV"
            ''' <summary>リース管理番号</summary>
            Public Const LEASE_NO As String = "_LEASE_NO"
            ''' <summary>リース期限</summary>
            Public Const LEASE_LIMIT As String = "_LEASE_LIMIT"
            ''' <summary>フリー項目1</summary>
            Public Const FREE_ITEM1 As String = "_FREE_ITEM1"
            ''' <summary>フリー項目2</summary>
            Public Const FREE_ITEM2 As String = "_FREE_ITEM2"
            ''' <summary>フリー項目3</summary>
            Public Const FREE_ITEM3 As String = "_FREE_ITEM3"
            ''' <summary>フリー項目4</summary>
            Public Const FREE_ITEM4 As String = "_FREE_ITEM4"
            ''' <summary>フリー項目5</summary>
            Public Const FREE_ITEM5 As String = "_FREE_ITEM5"
            ''' <summary>フリー項目6</summary>
            Public Const FREE_ITEM6 As String = "_FREE_ITEM6"
            ''' <summary>フリー項目7</summary>
            Public Const FREE_ITEM7 As String = "_FREE_ITEM7"
            ''' <summary>フリー項目8</summary>
            Public Const FREE_ITEM8 As String = "_FREE_ITEM8"
            ''' <summary>フリー項目9</summary>
            Public Const FREE_ITEM9 As String = "_FREE_ITEM9"
            ''' <summary>フリー項目10</summary>
            Public Const FREE_ITEM10 As String = "_FREE_ITEM10"
            ''' <summary>フリー項目11</summary>
            Public Const FREE_ITEM11 As String = "_FREE_ITEM11"
            ''' <summary>フリー項目12</summary>
            Public Const FREE_ITEM12 As String = "_FREE_ITEM12"
            ''' <summary>フリー項目13</summary>
            Public Const FREE_ITEM13 As String = "_FREE_ITEM13"
            ''' <summary>フリー項目14</summary>
            Public Const FREE_ITEM14 As String = "_FREE_ITEM14"
            ''' <summary>フリー項目15</summary>
            Public Const FREE_ITEM15 As String = "_FREE_ITEM15"
            ''' <summary>フリー項目16</summary>
            Public Const FREE_ITEM16 As String = "_FREE_ITEM16"
            ''' <summary>フリー項目17</summary>
            Public Const FREE_ITEM17 As String = "_FREE_ITEM17"
            ''' <summary>フリー項目18</summary>
            Public Const FREE_ITEM18 As String = "_FREE_ITEM18"
            ''' <summary>フリー項目19</summary>
            Public Const FREE_ITEM19 As String = "_FREE_ITEM19"
            '' <summary>フリー項目20</summary>
            Public Const FREE_ITEM20 As String = "_FREE_ITEM20"
            '' <summary>フリー項目21</summary>
            Public Const FREE_ITEM21 As String = "_FREE_ITEM21"
            '' <summary>フリー項目22</summary>
            Public Const FREE_ITEM22 As String = "_FREE_ITEM22"
            '' <summary>フリー項目23</summary>
            Public Const FREE_ITEM23 As String = "_FREE_ITEM23"
            '' <summary>フリー項目24</summary>
            Public Const FREE_ITEM24 As String = "_FREE_ITEM24"
            '' <summary>フリー項目25</summary>
            Public Const FREE_ITEM25 As String = "_FREE_ITEM25"
            '' <summary>フリー項目26</summary>
            Public Const FREE_ITEM26 As String = "_FREE_ITEM26"
            '' <summary>フリー項目27</summary>
            Public Const FREE_ITEM27 As String = "_FREE_ITEM27"
            '' <summary>フリー項目28</summary>
            Public Const FREE_ITEM28 As String = "_FREE_ITEM28"
            '' <summary>フリー項目29</summary>
            Public Const FREE_ITEM29 As String = "_FREE_ITEM29"
            '' <summary>フリー項目30</summary>
            Public Const FREE_ITEM30 As String = "_FREE_ITEM30"
            '' <summary>フリー項目31</summary>
            Public Const FREE_ITEM31 As String = "_FREE_ITEM31"
            '' <summary>フリー項目32</summary>
            Public Const FREE_ITEM32 As String = "_FREE_ITEM32"
            '' <summary>フリー項目33</summary>
            Public Const FREE_ITEM33 As String = "_FREE_ITEM33"
            '' <summary>フリー項目34</summary>
            Public Const FREE_ITEM34 As String = "_FREE_ITEM34"
            '' <summary>フリー項目35</summary>
            Public Const FREE_ITEM35 As String = "_FREE_ITEM35"
            '' <summary>フリー項目36</summary>
            Public Const FREE_ITEM36 As String = "_FREE_ITEM36"
            '' <summary>フリー項目37</summary>
            Public Const FREE_ITEM37 As String = "_FREE_ITEM37"
            '' <summary>フリー項目38</summary>
            Public Const FREE_ITEM38 As String = "_FREE_ITEM38"
            '' <summary>フリー項目39</summary>
            Public Const FREE_ITEM39 As String = "_FREE_ITEM39"
            '' <summary>フリー項目40</summary>
            Public Const FREE_ITEM40 As String = "_FREE_ITEM40"
            '' <summary>フリー項目41</summary>
            Public Const FREE_ITEM41 As String = "_FREE_ITEM41"
            '' <summary>フリー項目42</summary>
            Public Const FREE_ITEM42 As String = "_FREE_ITEM42"
            '' <summary>フリー項目43</summary>
            Public Const FREE_ITEM43 As String = "_FREE_ITEM43"
            '' <summary>フリー項目44</summary>
            Public Const FREE_ITEM44 As String = "_FREE_ITEM44"
            '' <summary>フリー項目45</summary>
            Public Const FREE_ITEM45 As String = "_FREE_ITEM45"
            '' <summary>フリー項目46</summary>
            Public Const FREE_ITEM46 As String = "_FREE_ITEM46"
            '' <summary>フリー項目47</summary>
            Public Const FREE_ITEM47 As String = "_FREE_ITEM47"
            '' <summary>フリー項目48</summary>
            Public Const FREE_ITEM48 As String = "_FREE_ITEM48"
            '' <summary>フリー項目49</summary>
            Public Const FREE_ITEM49 As String = "_FREE_ITEM49"
            '' <summary>フリー項目50</summary>
            Public Const FREE_ITEM50 As String = "_FREE_ITEM50"
            '' <summary>フリー項目51</summary>
            Public Const FREE_ITEM51 As String = "_FREE_ITEM51"
            '' <summary>フリー項目52</summary>
            Public Const FREE_ITEM52 As String = "_FREE_ITEM52"
            '' <summary>フリー項目53</summary>
            Public Const FREE_ITEM53 As String = "_FREE_ITEM53"
            '' <summary>フリー項目54</summary>
            Public Const FREE_ITEM54 As String = "_FREE_ITEM54"
            '' <summary>フリー項目55</summary>
            Public Const FREE_ITEM55 As String = "_FREE_ITEM55"
            '' <summary>フリー項目56</summary>
            Public Const FREE_ITEM56 As String = "_FREE_ITEM56"
            '' <summary>フリー項目57</summary>
            Public Const FREE_ITEM57 As String = "_FREE_ITEM57"
            '' <summary>フリー項目58</summary>
            Public Const FREE_ITEM58 As String = "_FREE_ITEM58"
            '' <summary>フリー項目59</summary>
            Public Const FREE_ITEM59 As String = "_FREE_ITEM59"
            '' <summary>フリー項目60</summary>
            Public Const FREE_ITEM60 As String = "_FREE_ITEM60"
            ''' <summary>資産ID</summary>
            Public Const ASSET_ID As String = "_ASSET_ID"
            ''' <summary>資産ID状態</summary>
            Public Const ASSET_ID_STATUS As String = "_ASSET_ID_STATUS"
            ''' <summary>タグ紐付け状態</summary>
            Public Const TAG_LINK_STATUS As String = "_TAG_LINK_STATUS"
            ''' <summary>有形・無形区分</summary>
            Public Const IS_TANGIBLE As String = "_IS_TANGIBLE"
            ''' <summary>備考1</summary>
            Public Const REMARK1 As String = "_REMARK1"
            ''' <summary>備考2</summary>
            Public Const REMARK2 As String = "_REMARK2"
            ''' <summary>備考3</summary>
            Public Const REMARK3 As String = "_REMARK3"
            ''' <summary>備考4</summary>
            Public Const REMARK4 As String = "_REMARK4"
            ''' <summary>備考5</summary>
            Public Const REMARK5 As String = "_REMARK5"
            ''' <summary>備考6</summary>
            Public Const REMARK6 As String = "_REMARK6"
            ''' <summary>備考7</summary>
            Public Const REMARK7 As String = "_REMARK7"
            ''' <summary>備考8</summary>
            Public Const REMARK8 As String = "_REMARK8"
            ''' <summary>備考9</summary>
            Public Const REMARK9 As String = "_REMARK9"
            ''' <summary>備考10</summary>
            Public Const REMARK10 As String = "_REMARK10"
            ''' <summary>登録日時</summary>
            Public Const INS_DATETIME As String = "_INS_DATETIME"
            ''' <summary>登録者</summary>
            Public Const INS_EMPLOYEE_NAME As String = "_INS_EMPLOYEE_NAME"
            ''' <summary>最終更新日時</summary>
            Public Const UPD_DATETIME As String = "_UPD_DATETIME"
            ''' <summary>最終更新担当者</summary>
            Public Const UPD_EMPLOYEE_NAME As String = "_UPD_EMPLOYEE_NAME"
        End Class

        ''' <summary>資産ラベル発行</summary>
        Public Class AssetLabelPrint
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>変更/参照</summary>
            Public Const CHANGE_OR_DISP As String = "_CHANGE_OR_DISP"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>サブ資産番号</summary>
            Public Const SUB_ASSET_NO As String = "_SUB_ASSET_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>管理ロケーションコード/名</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>ロケーション大区分コード/名</summary>
            Public Const LOC_L_CD As String = "_LOC_L_CD"
            ''' <summary>ロケーション中区分コード/名</summary>
            Public Const LOC_M_CD As String = "_LOC_M_CD"
            ''' <summary>ロケーション小区分コード/名</summary>
            Public Const LOC_S_CD As String = "_LOC_S_CD"
            ''' <summary>カテゴリー大分類コード/名</summary>
            Public Const CAT_L_CD As String = "_CAT_L_CD"
            ''' <summary>有形/無形区分</summary>
            Public Const IS_TANGIBLE As String = "_IS_TANGIBLE"
            ''' <summary>資産取得日</summary>
            Public Const FREE_ITEM1 As String = "_FREE_ITEM1"
            ''' <summary>資本化日付</summary>
            Public Const FREE_ITEM2 As String = "_FREE_ITEM2"
            ''' <summary>稼働区分</summary>
            Public Const FREE_ITEM3 As String = "_FREE_ITEM3"
            ''' <summary>稼働区分名</summary>
            Public Const FREE_ITEM4 As String = "_FREE_ITEM4"
            ''' <summary>型式</summary>
            Public Const FREE_ITEM5 As String = "_FREE_ITEM5"
            ''' <summary>管理者社員コード/名</summary>
            Public Const FREE_ITEM6 As String = "_FREE_ITEM6"
            ''' <summary>シリアルNO</summary>
            Public Const FREE_ITEM7 As String = "_FREE_ITEM7"
            ''' <summary>WBS番号</summary>
            Public Const FREE_ITEM8 As String = "_FREE_ITEM8"
            ''' <summary>備考</summary>
            Public Const FREE_ITEM9 As String = "_FREE_ITEM9"
            ''' <summary>貸与資産状態区分</summary>
            Public Const FREE_ITEM10 As String = "_FREE_ITEM10"
            ''' <summary>取得価格</summary>
            Public Const FREE_ITEM11 As String = "_FREE_ITEM11"
            ''' <summary>当月末帳簿価格額</summary>
            Public Const FREE_ITEM12 As String = "_FREE_ITEM12"
            ''' <summary>補助番号</summary>
            Public Const FREE_ITEM13 As String = "_FREE_ITEM13"
            ''' <summary>取込ファイル名</summary>
            Public Const FREE_ITEM14 As String = "_FREE_ITEM14"
            ''' <summary>資産グループ</summary>
            Public Const FREE_ITEM15 As String = "_FREE_ITEM15"
            ''' <summary>貸与先</summary>
            Public Const FREE_ITEM16 As String = "_FREE_ITEM16"
            ''' <summary>貸与先名</summary>
            Public Const FREE_ITEM17 As String = "_FREE_ITEM17"
            ''' <summary>開始日</summary>
            Public Const FREE_ITEM18 As String = "_FREE_ITEM18"
            ''' <summary>終了日</summary>
            Public Const FREE_ITEM19 As String = "_FREE_ITEM19"
            ''' <summary>貸与回数</summary>
            Public Const FREE_ITEM20 As String = "_FREE_ITEM20"
            ''' <summary>備考</summary>
            Public Const FREE_ITEM21 As String = "_FREE_ITEM21"
            ''' <summary>会計年度</summary>
            Public Const FREE_ITEM22 As String = "_FREE_ITEM22"
            ''' <summary>会計期間</summary>
            Public Const FREE_ITEM23 As String = "_FREE_ITEM23"
            ''' <summary>テキスト2</summary>
            Public Const FREE_ITEM24 As String = "_FREE_ITEM24"
            ''' <summary>勘定項目</summary>
            Public Const FREE_ITEM25 As String = "_FREE_ITEM25"
            ''' <summary>勘定項目名称</summary>
            Public Const FREE_ITEM26 As String = "_FREE_ITEM26"
            ''' <summary>棚卸ノート</summary>
            Public Const FREE_ITEM27 As String = "_FREE_ITEM27"
            ''' <summary>初回所得日</summary>
            Public Const FREE_ITEM28 As String = "_FREE_ITEM28"
            ''' <summary>数量</summary>
            Public Const FREE_ITEM29 As String = "_FREE_ITEM29"
            ''' <summary>資産番号テキスト</summary>
            Public Const FREE_ITEM30 As String = "_FREE_ITEM30"
            ''' <summary>車両ナンバープレート</summary>
            Public Const FREE_ITEM31 As String = "_FREE_ITEM31"
            ''' <summary>資産休止フラグ</summary>
            Public Const FREE_ITEM32 As String = "_FREE_ITEM32"
            ''' <summary>取得区分</summary>
            Public Const FREE_ITEM33 As String = "_FREE_ITEM33"
            ''' <summary>取得区分名</summary>
            Public Const FREE_ITEM34 As String = "_FREE_ITEM34"
            ''' <summary>資産種類</summary>
            Public Const FREE_ITEM35 As String = "_FREE_ITEM35"
            ''' <summary>資産種類名</summary>
            Public Const FREE_ITEM36 As String = "_FREE_ITEM36"
            ''' <summary>資産リース区分</summary>
            Public Const FREE_ITEM37 As String = "_FREE_ITEM37"
            ''' <summary>市区町村コード</summary>
            Public Const FREE_ITEM38 As String = "_FREE_ITEM38"
            ''' <summary>市区町村コード名</summary>
            Public Const FREE_ITEM39 As String = "_FREE_ITEM39"
            ''' <summary>仕入先</summary>
            Public Const FREE_ITEM40 As String = "_FREE_ITEM40"
            ''' <summary>資産供給者名</summary>
            Public Const FREE_ITEM41 As String = "_FREE_ITEM41"
            ''' <summary>CAPS番号</summary>
            Public Const FREE_ITEM42 As String = "_FREE_ITEM42"
            ''' <summary>稟議#</summary>
            Public Const FREE_ITEM43 As String = "_FREE_ITEM43"
            ''' <summary>リース会社</summary>
            Public Const FREE_ITEM44 As String = "_FREE_ITEM44"
            ''' <summary>リース会社名</summary>
            Public Const FREE_ITEM45 As String = "_FREE_ITEM45"
            ''' <summary>契約番号</summary>
            Public Const FREE_ITEM46 As String = "_FREE_ITEM46"
            ''' <summary>契約日</summary>
            Public Const FREE_ITEM47 As String = "_FREE_ITEM47"
            ''' <summary>リース終了日付</summary>
            Public Const FREE_ITEM48 As String = "_FREE_ITEM48"
            ''' <summary>リース開始日</summary>
            Public Const FREE_ITEM49 As String = "_FREE_ITEM49"
            ''' <summary>リース期間（年）</summary>
            Public Const FREE_ITEM50 As String = "_FREE_ITEM50"
            ''' <summary>リース期間（期）</summary>
            Public Const FREE_ITEM51 As String = "_FREE_ITEM51"
            ''' <summary>リース支払回数</summary>
            Public Const FREE_ITEM52 As String = "_FREE_ITEM52"
            ''' <summary>リース支払</summary>
            Public Const FREE_ITEM53 As String = "_FREE_ITEM53"
            ''' <summary>年利</summary>
            Public Const FREE_ITEM54 As String = "_FREE_ITEM54"
            ''' <summary>フリー項目55</summary>
            Public Const FREE_ITEM55 As String = "_FREE_ITEM55"
            ''' <summary>フリー項目56</summary>
            Public Const FREE_ITEM56 As String = "_FREE_ITEM56"
            ''' <summary>フリー項目57</summary>
            Public Const FREE_ITEM57 As String = "_FREE_ITEM57"
            ''' <summary>フリー項目58</summary>
            Public Const FREE_ITEM58 As String = "_FREE_ITEM58"
            ''' <summary>フリー項目59</summary>
            Public Const FREE_ITEM59 As String = "_FREE_ITEM59"
            ''' <summary>フリー項目60</summary>
            Public Const FREE_ITEM60 As String = "_FREE_ITEM60"
            ''' <summary>取込日時</summary>
            Public Const IP_DATETIME As String = "_IP_DATETIME"
            ''' <summary>取込ファイル名</summary>
            Public Const IP_FILE_NAME As String = "_IP_FILE_NAME"
            ''' <summary>取込担当者コード/名</summary>
            Public Const IP_EMPLOYEE_CD As String = "_IP_EMPLOYEE_CD"
        End Class

        Public Class DepartmentList
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>変更/参照</summary>
            Public Const CHANGE_OR_DISP As String = "_CHANGE_OR_DISP"
            ''' <summary>部署コード</summary>
            Public Const DEPARTMENT_CD As String = "_DEPARTMENT_CD"
            ''' <summary>部署名</summary>
            Public Const DEPARTMENT_NAME As String = "_DEPARTMENT_NAME"
            ''' <summary>郵便番号</summary>
            Public Const ZIP_CODE As String = "_ZIP_CODE"
            ''' <summary>本部/拠点</summary>
            Public Const HEAD_OFFICE_FLG As String = "_HEAD_OFFICE_FLG"
            ''' <summary>電話番号</summary>
            Public Const PHONE_NUMBER As String = "_PHONE_NUMBER"
            ''' <summary>住所</summary>
            Public Const ADDRESS As String = "_ADDRESS"
            ''' <summary>FAX</summary>
            Public Const FAX As String = "_FAX"
            ''' <summary>送付部署名</summary>
            Public Const REQ_DEP_NAME As String = "_REQ_DEP_NAME"

        End Class

        ''' <summary>項目配置マスタメンテ</summary>
        Public Class ColSet
            ''' <summary>DB項目名</summary>
            Public Const DB_COLUMN_NAME As String = "_DB_COLUMN_NAME"
            ''' <summary>並び順</summary>
            Public Const DISP_ORDER As String = "_DISP_ORDER"
            ''' <summary>表示</summary>
            Public Const DISP_FLAG As String = "_DISP_FLAG"
            ''' <summary>項目名</summary>
            Public Const COL_NAME As String = "_COL_NAME"
            ''' <summary>固定列数</summary>
            Public Const FIX_COL_CNT As String = "_FIX_COL_CNT"
        End Class

        ''' <summary>カテゴリマスタメンテ</summary>
        Public Class Category
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>分類区分</summary>
            Public Const CAT_DIV As String = "_CAT_DIV"
            ''' <summary>分類コード</summary>
            Public Const CAT_CD As String = "_CAT_CD"
            ''' <summary>カテゴリ大分類コード</summary>
            Public Const CAT_L_CD As String = "_CAT_L_CD"
            ''' <summary>カテゴリ中分類コード</summary>
            Public Const CAT_M_CD As String = "_CAT_M_CD"
            ''' <summary>カカテゴリ小分類コード</summary>
            Public Const CAT_S_CD As String = "_CAT_S_CD"
            ''' <summary>分類名称</summary>
            Public Const CAT_NAME As String = "_CAT_NAME"
            ''' <summary>リース期限アラート区分</summary>
            Public Const LEASE_LIMIT_ALERT_DIV As String = "_LEASE_LIMIT_ALERT_DIV"
            ''' <summary>リース期限何日前</summary>
            Public Const LEASE_LIMIT_BEF_DAYS As String = "_LEASE_LIMIT_BEF_DAYS"
            ''' <summary>○○期限アラート区分</summary>
            Public Const GENERIC_LIMIT_ALERT_DIV As String = "_GENERIC_LIMIT_ALERT_DIV"
            ''' <summary>○○期限何日前</summary>
            Public Const GENERIC_LIMIT_BEF_DAYS As String = "_GENERIC_LIMIT_BEF_DAYS"
            ''' <summary>貸出アラート区分</summary>
            Public Const LEASE_ALERT_DIV As String = "_LEASE_ALERT_DIV"
            ''' <summary>貸出アラート回数</summary>
            Public Const LEASE_ALERT_CNT As String = "_LEASE_ALERT_CNT"
            ''' <summary>貸出上限数</summary>
            Public Const LEASE_UPPER_LIMIT As String = "_LEASE_UPPER_LIMIT"
            ''' <summary>貸出上限エラー区分</summary>
            Public Const LEASE_UPPER_ERR_DIV As String = "_LEASE_UPPER_ERR_DIV"
            ''' <summary>資産ID登録方法</summary>
            Public Const ASSET_ID_REG_DIV As String = "_ASSET_ID_REG_DIV"
            ''' <summary>EPC領域サイズ</summary>
            Public Const EPC_SIZE As String = "_EPC_SIZE"
            ''' <summary>EPC事業所キー</summary>
            Public Const EPC_COMPANY_KEY As String = "_EPC_COMPANY_KEY"
            ''' <summary>EPC資産タイプ</summary>
            Public Const EPC_ASSET_TYPE As String = "_EPC_ASSET_TYPE"

        End Class

        ''' <summary>ロケーションマスタメンテ</summary>
        Public Class Location
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>ロケーション区分/区分名</summary>
            Public Const LOC_DIV As String = "_LOC_DIV"
            ''' <summary>ロケーションコード</summary>
            Public Const LOC_CD As String = "_LOC_CD"
            ''' <summary>ロケーション名</summary>
            Public Const LOC_NAME As String = "_LOC_NAME"
            ''' <summary>共通/拠点</summary>   '2025/10/14 DEL Kudo
            Public Const LOC_SHARE_FLG As String = "_LOC_SHARE_FLG"
            ''' <summary>メンテ発生貸出回数</summary>
            Public Const MAINTE_LEASE_NUM As String = "_MAINTE_LEASE_NUM"
            ''' <summary>メンテ発生貸出日数</summary>
            Public Const MAINTE_LEASE_DAY As String = "_MAINTE_LEASE_DAY"
            ''' <summary>メンテ日数</summary>
            Public Const MAINTE_DAY As String = "_MAINTE_DAY"
            ''' <summary>数量</summary>
            Public Const SPECIFIED_QTY As String = "_SPECIFIED_QTY"
            ''' <summary>RFIDタグ拠点</summary>    '2025/10/14 ADD Kudo
            Public Const LOC_RFID_FLG As String = "LOC_RFID_FLG"
            ''' <summary>管理資産棚卸区分</summary>
            Public Const OWN_INV_FLG As String = "OWN_INV_FLG"
        End Class

        ''' <summary>CSVマッピング</summary>
        Public Class CsvMapping
            ''' <summary>No</summary>
            Public Const NO As String = "_NO"
            ''' <summary>CSV項目</summary>
            Public Const CSV_KOUMOKU As String = "_CSV_KOUMOKU"
            ''' <summary>必須</summary>
            Public Const REQUIRED As String = "_REQUIRED"
            ''' <summary>指定方法</summary>
            Public Const SPECIFIED As String = "_SPECIFIED"
            ''' <summary>列番号</summary>
            Public Const COL_NO As String = "_COL_NO"
            ''' <summary>固定値</summary>
            Public Const FIX_VAL As String = "_FIX_VAL"
        End Class

        ''' <summary>社員マスタメンテ</summary>
        Public Class Employee
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>社員コード</summary>
            Public Const EMPLOYEE_CD As String = "_EMPLOYEE_CD"
            ''' <summary>社員コード</summary>
            Public Const EMPLOYEE_NAME As String = "_EMPLOYEE_NAME"
            ''' <summary>退職フラグ</summary>
            Public Const RETIRE_FLG As String = "_RETIRE_FLG"
            ''' <summary>部署コード</summary>
            Public Const DEPARTMENT_CD As String = "_DEPARTMENT_CD"
            ''' <summary>部署名</summary>
            Public Const DEPARTMENT_NAME As String = "_DEPARTMENT_NAME"
            ''' <summary>パスワード</summary>
            Public Const PASSWORD As String = "_PASSWORD"
            ''' <summary>権限区分</summary>
            Public Const AUTH_DIV As String = "_AUTH_DIV"
            ''' <summary>所属拠点</summary>
            Public Const HUB_LOC_L_NAME As String = "_HUB_LOC_L_NAME"
            ''' <summary>登録日時</summary>
            Public Const INS_DATETIME As String = "_INS_DATETIME"
            ''' <summary>登録者</summary>
            Public Const INS_EMPLOYEE_NAME As String = "_INS_EMPLOYEE_NAME"
            ''' <summary>最終登録更新日時</summary>
            Public Const UPD_DATETIME As String = "_UPD_DATETIME"
            ''' <summary>最終登録更新者</summary>
            Public Const UPD_EMPLOYEE_NAME As String = "_UPD_EMPLOYEE_NAME"
        End Class

        ''' <summary>貸出先マスタメンテ</summary>
        Public Class LeasePlace
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>貸出先コード</summary>
            Public Const LEASE_PLACE_CD As String = "_LEASE_PLACE_CD"
            ''' <summary>貸出先名</summary>
            Public Const LEASE_PLACE_NAME As String = "_LEASE_PLACE_NAME"
            ''' <summary>電話番号</summary>
            Public Const PHONE_NUMBER As String = "_PHONE_NUMBER"
            ''' <summary>郵便番号</summary>
            Public Const ZIP_CODE As String = "_ZIP_CODE"
            ''' <summary>住所</summary>
            Public Const ADDRESS As String = "_ADDRESS"
            ''' <summary>二次貸出先フラグ</summary>
            Public Const SEC_LEASE_PLACE_FLG As String = "_SEC_LEASE_PLACE_FLG"
        End Class

        ''' <summary>設定マスタメンテ</summary>
        Public Class SETTING
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>機能分類名</summary>
            Public Const FUNC_CAT_NAME As String = "_FUNC_CAT_NAME"
            ''' <summary>KEY種別</summary>
            Public Const KEY_DIV As String = "_KEY_DIV"
            ''' <summary>KEY1</summary>
            Public Const KEY1 As String = "_KEY1"
            ''' <summary>KEY2</summary>
            Public Const KEY2 As String = "_KEY2"
            ''' <summary>KEY3</summary>
            Public Const KEY3 As String = "_KEY3"
            ''' <summary>備考</summary>REMARK
            Public Const REMARK As String = "_KEY3"
            ''' <summary>DATA1</summary>REMARK
            Public Const DATA1 As String = "_DATA1"
            ''' <summary>DATA2</summary>REMARK
            Public Const DATA2 As String = "_DATA2"
            ''' <summary>DATA3</summary>REMARK
            Public Const DATA3 As String = "_DATA3"
            ''' <summary>DATA4</summary>REMARK
            Public Const DATA4 As String = "_DATA4"
            ''' <summary>DATA5</summary>REMARK
            Public Const DATA5 As String = "_DATA5"
            ''' <summary>DATA6</summary>REMARK
            Public Const DATA6 As String = "_DATA6"
            ''' <summary>DATA7</summary>REMARK
            Public Const DATA7 As String = "_DATA7"
            ''' <summary>DATA8</summary>REMARK
            Public Const DATA8 As String = "_DATA8"
            ''' <summary>DATA9</summary>REMARK
            Public Const DATA9 As String = "_DATA9"
            ''' <summary>DATA10</summary>REMARK
            Public Const DATA10 As String = "_DATA10"
            ''' <summary>設定対象区分</summary>REMARK
            Public Const SETTING_OBJECT_DIV As String = "_SETTING_OBJECT_DIV"
            ''' <summary>更新可否</summary>REMARK
            Public Const UPD_AUTHORITY_FLG As String = "_UPD_AUTHORITY_FLG"
            ''' <summary>削除可否</summary>REMARK
            Public Const DEL_AUTHORITY_FLG As String = "_DEL_AUTHORITY_FLG"
            ''' <summary>登録日時</summary>REMARK
            Public Const INS_DATETIME As String = "_INS_DATETIME"
            ''' <summary>登録者氏名</summary>REMARK
            Public Const INS_EMPLOYEE_NAME As String = "_INS_EMPLOYEE_NAME"
            ''' <summary>最終登録更新日時</summary>REMARK
            Public Const UPD_DATETIME As String = "_UPD_DATETIME"
            ''' <summary>最終登録更新者氏名</summary>REMARK
            Public Const UPD_EMPLOYEE_NAME As String = "_UPD_EMPLOYEE_NAME"
        End Class

        ''' <summary>文言マスタメンテ</summary>
        Public Class WORD
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>文言コード</summary>
            Public Const WORD_CD As String = "_WORD_CD"
            ''' <summary>デフォルト英語</summary>
            Public Const WORDING_EN As String = "_WORDING_EN"
            ''' <summary>デフォルト日本語</summary>
            Public Const WORDING_JA As String = "_WORDING_JA"
            ''' <summary>表示名</summary>
            Public Const WORDING_LU As String = "_WORDING_LU"
        End Class

        ''' <summary>資産在庫照会</summary>
        Public Class StockList
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>管理ロケーションコード/名</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>ロケーション大区分コード/名</summary>
            Public Const LOC_L_CD As String = "_LOC_L_CD"
            ''' <summary>ロケーション中区分コード/名</summary>
            Public Const LOC_M_CD As String = "_LOC_M_CD"
            ''' <summary>ロケーション小区分コード/名</summary>
            Public Const LOC_S_CD As String = "_LOC_S_CD"
            ''' <summary>カテゴリー大分類コード/名</summary>
            Public Const CAT_L_CD As String = "_CAT_L_CD"
            ''' <summary>カテゴリー中分類コード/名</summary>
            Public Const CAT_M_CD As String = "_CAT_M_CD"
            ''' <summary>カテゴリー小分類コード/名</summary>
            Public Const CAT_S_CD As String = "_CAT_S_CD"
            ''' <summary>数量</summary>
            Public Const QTY As String = "_QTY"
        End Class

        ''' <summary>資産履歴照会(番号指定)上部</summary>
        Public Class AssetDetailTop
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>サブ資産番号</summary>
            Public Const SUB_ASSET_NO As String = "_SUB_ASSET_NO"
            ''' <summary>資産状態</summary>
            Public Const ASSET_STATUS As String = "_ASSET_STATUS"
            ''' <summary>資産ID状態</summary>
            Public Const ASSET_ID_STATUS As String = "_ASSET_ID_STATUS"
            ''' <summary>タグ紐付け状態</summary>
            Public Const TAG_LINK_STATUS As String = "_TAG_LINK_STATUS"
            ''' <summary>貸出状態</summary>
            Public Const LEASE_STATUS As String = "_LEASE_STATUS"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>カテゴリー大分類コード/名</summary>
            Public Const CAT_L_CD As String = "_CAT_L_CD"
            ''' <summary>カテゴリー中分類コード/名</summary>
            Public Const CAT_M_CD As String = "_CAT_M_CD"
            ''' <summary>カテゴリー小分類コード/名</summary>
            Public Const CAT_S_CD As String = "_CAT_S_CD"
            ''' <summary>資産製造元</summary>
            Public Const MAKER As String = "_MAKER"
            ''' <summary>品番</summary>
            Public Const PRODUCT_NO As String = "_PRODUCT_NO"
            ''' <summary>型番</summary>
            Public Const MODEL_NO As String = "_MODEL_NO"
            ''' <summary>貸出回数</summary>
            Public Const LEASE_CNT As String = "_LEASE_CNT"
        End Class

        ''' <summary>資産履歴照会(番号指定)下部</summary>
        Public Class AssetDetailBottom
            ''' <summary>処理日時</summary>
            Public Const PROCESS_DT As String = "_PROCESS_DT"
            ''' <summary>処理区分</summary>
            Public Const PROCESS_DIV As String = "_PROCESS_DIV"
            ''' <summary>処理分類</summary>
            Public Const PROCESS_TOOL As String = "_PROCESS_TOOL"
            ''' <summary>貸出先コード/名</summary>
            Public Const LEASE_PLACE_CD As String = "_LEASE_PLACE_CD"
            ''' <summary>貸出先備考</summary>
            Public Const REMARK As String = "_REMARK"
            ''' <summary>貸出先担当社員コード/名</summary>
            Public Const LEASE_EMPLOYEE_CD As String = "_LEASE_EMPLOYEE_CD"
            ''' <summary>貸出期間</summary>
            Public Const LEASE_KIKAN As String = "_LEASE_KIKAN"
            ''' <summary>重量</summary>
            Public Const WEIGHT_VAL As String = "_WEIGHT_VAL"
            ''' <summary>作業者コード/名</summary>
            Public Const WORKER_CD As String = "_WORKER_CD"
            ''' <summary>作業時間</summary>
            Public Const WORKING_HOURS As String = "_WORKING_HOURS"
            ''' <summary>作業概要</summary>
            Public Const WORK_SUMMARY As String = "_WORK_SUMMARY"
            ''' <summary>返却備考1</summary>
            Public Const RETURN_REMARK1 As String = "_RETURN_REMARK1"
            ''' <summary>返却備考2</summary>
            Public Const RETURN_REMARK2 As String = "_RETURN_REMARK2"
            ''' <summary>管理ロケーションコード/名</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>ロケーション大区分コード/名</summary>
            Public Const LOC_L_CD As String = "_LOC_L_CD"
            ''' <summary>ロケーション中区分コード/名</summary>
            Public Const LOC_M_CD As String = "_LOC_M_CD"
            ''' <summary>ロケーション小区分コード/名</summary>
            Public Const LOC_S_CD As String = "_LOC_S_CD"
            ''' <summary>資産ID（EPC）</summary>
            Public Const ASSET_ID As String = "_ASSET_ID"
            ''' <summary>ログイン社員コード/名</summary>
            Public Const UPD_EMPLOYEE_CD As String = "_UPD_EMPLOYEE_CD"
            ''' <summary>備考1</summary>
            Public Const REMARK1 As String = "_REMARK1"
            ''' <summary>備考2</summary>
            Public Const REMARK2 As String = "_REMARK2"
            ''' <summary>備考3</summary>
            Public Const REMARK3 As String = "_REMARK3"
            ''' <summary>備考4</summary>
            Public Const REMARK4 As String = "_REMARK4"
            ''' <summary>備考5</summary>
            Public Const REMARK5 As String = "_REMARK5"
            ''' <summary>備考6</summary>
            Public Const REMARK6 As String = "_REMARK6"
            ''' <summary>備考7</summary>
            Public Const REMARK7 As String = "_REMARK7"
            ''' <summary>備考8</summary>
            Public Const REMARK8 As String = "_REMARK8"
            ''' <summary>備考9</summary>
            Public Const REMARK9 As String = "_REMARK9"
            ''' <summary>備考10</summary>
            Public Const REMARK10 As String = "_REMARK10"
        End Class

        ''' <summary>資産履歴照会</summary>
        Public Class AssetHistory
            ''' <summary>処理日時</summary>
            Public Const PROCESS_DT As String = "_PROCESS_DT"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>サブ資産番号</summary>
            Public Const SUB_ASSET_NO As String = "_SUB_ASSET_NO"
            ''' <summary>処理区分</summary>
            Public Const PROCESS_DIV As String = "_PROCESS_DIV"
            ''' <summary>処理分類</summary>
            Public Const PROCESS_TOOL As String = "_PROCESS_TOOL"
            ''' <summary>貸出先コード/名</summary>
            Public Const LEASE_PLACE_CD As String = "_LEASE_PLACE_CD"
            ''' <summary>貸出先備考</summary>
            Public Const REMARK As String = "_REMARK"
            ''' <summary>貸出先担当社員コード/名</summary>
            Public Const LEASE_EMPLOYEE_CD As String = "_LEASE_EMPLOYEE_CD"
            ''' <summary>貸出期間</summary>
            Public Const LEASE_KIKAN As String = "_LEASE_KIKAN"
            ''' <summary>重量</summary>
            Public Const WEIGHT_VAL As String = "_WEIGHT_VAL"
            ''' <summary>作業者コード/名</summary>
            Public Const WORKER_CD As String = "_WORKER_CD"
            ''' <summary>作業時間</summary>
            Public Const WORKING_HOURS As String = "_WORKING_HOURS"
            ''' <summary>作業概要</summary>
            Public Const WORK_SUMMARY As String = "_WORK_SUMMARY"
            ''' <summary>返却備考1</summary>
            Public Const RETURN_REMARK1 As String = "_RETURN_REMARK1"
            ''' <summary>返却備考2</summary>
            Public Const RETURN_REMARK2 As String = "_RETURN_REMARK2"
            ''' <summary>管理ロケーションコード/名</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>ロケーション大区分コード/名</summary>
            Public Const LOC_L_CD As String = "_LOC_L_CD"
            ''' <summary>ロケーション中区分コード/名</summary>
            Public Const LOC_M_CD As String = "_LOC_M_CD"
            ''' <summary>ロケーション小区分コード/名</summary>
            Public Const LOC_S_CD As String = "_LOC_S_CD"
            ''' <summary>備考1</summary>
            Public Const REMARK1 As String = "_REMARK1"
            ''' <summary>備考2</summary>
            Public Const REMARK2 As String = "_REMARK2"
            ''' <summary>備考3</summary>
            Public Const REMARK3 As String = "_REMARK3"
            ''' <summary>備考4</summary>
            Public Const REMARK4 As String = "_REMARK4"
            ''' <summary>備考5</summary>
            Public Const REMARK5 As String = "_REMARK5"
            ''' <summary>備考6</summary>
            Public Const REMARK6 As String = "_REMARK6"
            ''' <summary>備考7</summary>
            Public Const REMARK7 As String = "_REMARK7"
            ''' <summary>備考8</summary>
            Public Const REMARK8 As String = "_REMARK8"
            ''' <summary>備考9</summary>
            Public Const REMARK9 As String = "_REMARK9"
            ''' <summary>備考10</summary>
            Public Const REMARK10 As String = "_REMARK10"
            ''' <summary>カテゴリー大区分コード/名</summary>
            Public Const CAT_L_CD As String = "_CAT_L_CD"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>資産製造元</summary>
            Public Const MAKER As String = "_MAKER"
            ''' <summary>品番</summary>
            Public Const PRODUCT_NO As String = "_PRODUCT_NO"
            ''' <summary>型番</summary>
            Public Const MODEL_NO As String = "_MODEL_NO"
            ''' <summary>資産ID（EPC）</summary>
            Public Const ASSET_ID As String = "_ASSET_ID"
            ''' <summary>ログイン社員コード/名</summary>
            Public Const UPD_EMPLOYEE_CD As String = "_UPD_EMPLOYEE_CD"
        End Class

        ''' <summary>カレンダーマスタメンテ</summary>
        Public Class CALENDAR
            ''' <summary>年月日</summary>
            Public Const DT As String = "_DT"
            ''' <summary>曜日</summary>
            Public Const DAY_OF_WEEK As String = "_DAY_OF_WEEK"
            ''' <summary>非稼働日フラグ</summary>
            Public Const NOT_WORK_FLG As String = "_NOT_WORK_FLG"
        End Class

        ''' <summary>商品マスタメンテ</summary>
        Public Class GOODS
            ''' <summary>品番</summary>
            Public Const PRODUCT_NO As String = "_PRODUCT_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>資産購入日</summary>
            Public Const FREE_ITEM1 As String = "FREE_ITEM1"
            ''' <summary>管理ロケーションコード/名</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>ロケーション大区分コード/名</summary>
            Public Const LOC_L_CD As String = "_LOC_L_CD"
            ''' <summary>ロケーション中区分コード/名</summary>
            Public Const LOC_M_CD As String = "_LOC_M_CD"
            ''' <summary>ロケーション小区分コード/名</summary>
            Public Const LOC_S_CD As String = "_LOC_S_CD"
            ''' <summary>カテゴリー大分類コード/名</summary>
            Public Const CAT_L_CD As String = "_CAT_L_CD"
            ''' <summary>カテゴリー中分類コード/名</summary>
            Public Const CAT_M_CD As String = "_CAT_M_CD"
            ''' <summary>カテゴリー小分類コード/名</summary>
            Public Const CAT_S_CD As String = "_CAT_S_CD"
            ''' <summary>○○期限</summary>
            Public Const GENERIC_LIMIT As String = "_GENERIC_LIMIT"
            ''' <summary>資産製造元</summary>
            Public Const MAKER As String = "_MAKER"
            ''' <summary>型番</summary>
            Public Const MODEL_NO As String = "_MODEL_NO"
            ''' <summary>資産区分</summary>
            Public Const ASSET_DIV As String = "_ASSET_DIV"
            ''' <summary>リース管理番号</summary>
            Public Const LEASE_NO As String = "_LEASE_NO"
            ''' <summary>リース期限</summary>
            Public Const LEASE_LIMIT As String = "_LEASE_LIMIT"
            ''' <summary>備考2</summary>
            Public Const FREE_ITEM2 As String = "_FREE_ITEM2"
            ''' <summary>備考3</summary>
            Public Const FREE_ITEM3 As String = "_FREE_ITEM3"
            ''' <summary>備考4</summary>
            Public Const FREE_ITEM4 As String = "_FREE_ITEM4"
            ''' <summary>備考5</summary>
            Public Const FREE_ITEM5 As String = "_FREE_ITEM5"
            ''' <summary>備考6</summary>
            Public Const FREE_ITEM6 As String = "_FREE_ITEM6"
            ''' <summary>備考7</summary>
            Public Const FREE_ITEM7 As String = "_FREE_ITEM7"
            ''' <summary>備考8</summary>
            Public Const FREE_ITEM8 As String = "_FREE_ITEM8"
            ''' <summary>備考9</summary>
            Public Const FREE_ITEM9 As String = "_FREE_ITEM9"
            ''' <summary>備考10</summary>
            Public Const FREE_ITEM10 As String = "_FREE_ITEM10"
            ''' <summary>備考11</summary>
            Public Const FREE_ITEM11 As String = "_FREE_ITEM11"
            ''' <summary>備考12</summary>
            Public Const FREE_ITEM12 As String = "_FREE_ITEM12"
            ''' <summary>備考13</summary>
            Public Const FREE_ITEM13 As String = "_FREE_ITEM13"
            ''' <summary>備考14</summary>
            Public Const FREE_ITEM14 As String = "_FREE_ITEM14"
            ''' <summary>備考15</summary>
            Public Const FREE_ITEM15 As String = "_FREE_ITEM15"
            ''' <summary>備考16</summary>
            Public Const FREE_ITEM16 As String = "_FREE_ITEM16"
            ''' <summary>備考17</summary>
            Public Const FREE_ITEM17 As String = "_FREE_ITEM17"
            ''' <summary>備考18</summary>
            Public Const FREE_ITEM18 As String = "_FREE_ITEM18"
            ''' <summary>備考19 </summary>
            Public Const FREE_ITEM19 As String = "_FREE_ITEM19"
            ''' <summary>資産画像</summary>
            Public Const ASSET_PIC_FILENAME As String = "_ASSET_PIC_FILENAME"
        End Class

        ''' <summary>ゲートマスタメンテ</summary>
        Public Class GATE
            ''' <summary>選択(WaveGate)</summary>
            Public Const CHECK_SELECT_WG As String = "_CHECK_SELECT_WG"
            ''' <summary>選択(SpeedWay)</summary>
            Public Const CHECK_SELECT_SW As String = "_CHECK_SELECT_SW"

            ''' <summary>ゲートコード(WaveGate)</summary>
            Public Const GATE_CD_WG As String = "_GATE_CD_WG"
            ''' <summary>ゲートコード(SpeedWay)</summary>
            Public Const GATE_CD_SW As String = "_GATE_CD_SW"
            ''' <summary>ゲート名(WaveGate)</summary>
            Public Const GATE_NAME_WG As String = "_GATE_NAME_WG"
            ''' <summary>ゲート名(SpeedWay)</summary>
            Public Const GATE_NAME_SW As String = "_GATE_NAME_SW"
            ''' <summary>IPアドレス(WaveGate)</summary>
            Public Const GATE_IPADDRESS_WG As String = "_GATE_IPADDRESS_WG"
            ''' <summary>IPアドレス(SpeedWay)</summary>
            Public Const GATE_IPADDRESS_SW As String = "_GATE_IPADDRESS_SW"
            ''' <summary>ゲート(WaveGate)</summary>
            Public Const GATE_VALID_FLAG_WG As String = "_GATE_VALID_FLAG_WG"
            ''' <summary>ゲート(SpeedWay)</summary>
            Public Const GATE_VALID_FLAG_SW As String = "_GATE_VALID_FLAG_SW"
            ''' <summary>パトライト</summary>
            Public Const PATLITE_VALID_FLAG As String = "_PATLITE_VALID_FLAG"
            ''' <summary>サーモセンサ１</summary>
            Public Const THERMO1_VALID_FLAG As String = "_THERMO1_VALID_FLAG"
            ''' <summary>サーモセンサ２</summary>
            Public Const THERMO2_VALID_FLAG As String = "_THERMO2_VALID_FLAG"
            ''' <summary>チャンネル</summary>
            Public Const GATE_CHANNEL As String = "_GATE_CHANNEL"
            ''' <summary>周波数</summary>
            Public Const FREQUENCY As String = "_FREQUENCY"
            ''' <summary>電波強度</summary>
            Public Const ANTENNA_POW As String = "_ANTENNA_POW"
            ''' <summary>機種</summary>
            Public Const DEVICE_TYPE As String = "_DEVICE_TYPE"
            ''' <summary>GPIO</summary>
            Public Const GPIO As String = "_GPIO"
            ''' <summary>IO設定</summary>
            Public Const IO_SETTING As String = "_IO_SETTING"
            ''' <summary>IO No.1</summary>
            Public Const IO_No1 As String = "_IO_No1"
            ''' <summary>IO No.2</summary>
            Public Const IO_No2 As String = "_IO_No2"
        End Class

        ''' <summary>ゲート持ち出し履歴</summary>
        Public Class GatePassLog
            ''' <summary>通過ゲート</summary>
            Public Const GATE_NAME As String = "_GATE_NAME"
            ''' <summary>通過日時</summary>
            Public Const GATE_PASS_DT As String = "_GATE_PASS_DT"
            ''' <summary>データ</summary>
            Public Const GATE_DATA As String = "_GATE_DATA"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>資産名称メーカ→資産製造元に変更</summary>
            Public Const MAKER As String = "_MAKER"
            ''' <summary>品番</summary>
            Public Const PRODUCT_NO As String = "_PRODUCT_NO"
            ''' <summary>型番</summary>
            Public Const MODEL_NO As String = "_MODEL_NO"
            ''' <summary>備考</summary>
            Public Const GATE_REMARK As String = "_GATE_REMARK"
        End Class

        ''' <summary>貸出先一覧</summary>
        Public Class AssetLeasePlaceList
            ''' <summary>貸出先コード</summary>
            Public Const LEASE_PLACE_CD As String = "_LEASE_PLACE_CD"
            ''' <summary>貸出回数</summary>
            Public Const LEASE_COUNT As String = "_LEASE_COUNT"
            ''' <summary>期限切れ</summary>
            Public Const LEASE_LIMIT_EXPIRE_COUNT As String = "_LEASE_LIMIT_EXPIRE_COUNT"
        End Class

        ''' <summary>貸出先資産照会</summary>
        Public Class AssetLeasePlaceDetailList
            ''' <summary>貸出先コード/名</summary>
            Public Const LEASE_PLACE_CD_NAME As String = "_LEASE_PLACE_CD_NAME"
            ''' <summary>貸出状態</summary>
            Public Const LEASE_STATUS As String = "_LEASE_STATUS"
            ''' <summary>貸出期間</summary>
            Public Const LEASE_KIKAN As String = "_LEASE_KIKAN"
            ''' <summary>貸出先備考</summary>
            Public Const LEASE_REMARK As String = "_LEASE_REMARK"
            ''' <summary>貸出担当者コード/名</summary>
            Public Const LEASE_EMPLOYEE_CD_NAME As String = "_LEASE_EMPLOYEE_CD_NAME"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>資産製造元</summary>
            Public Const MAKER As String = "_MAKER"
            ''' <summary>品番</summary>
            Public Const PRODUCT_NO As String = "_PRODUCT_NO"
            ''' <summary>型番</summary>
            Public Const MODEL_NO As String = "_MODEL_NO"
        End Class

        ''' <summary>予約履歴(上部)</summary>
        Public Class LendingHistoryTop
            ''' <summary>予約番号</summary>
            Public Const RESERVE_ID As String = "_RESERVE_ID"
            ''' <summary>管理ロケーション</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>申請者</summary>
            Public Const REQ_EMP_CD As String = "_REQ_EMP_CD"
        End Class

        ''' <summary>予約履歴(下部)</summary>
        Public Class LendingHistoryBottom
            ''' <summary>処理日時</summary>
            Public Const PROCESS_DT As String = "_PROCESS_DT"
            ''' <summary>処理区分</summary>
            Public Const PROCESS_DIV As String = "_PROCESS_DIV"
            ''' <summary>処理分類</summary>
            Public Const PROCESS_TOOL As String = "_PROCESS_TOOL"
            ''' <summary>担当者</summary>
            Public Const CHARGE_EMP_CD As String = "_CHARGE_EMP_CD"
            ''' <summary>実質貸出開始日</summary>
            Public Const REAL_START_DT As String = "_REAL_START_DT"
            ''' <summary>実質貸出終了日</summary>
            Public Const REAL_END_DT As String = "_REAL_END_DT"
            ''' <summary>申請貸出開始日</summary>
            Public Const USER_START_DT As String = "_USER_START_DT"
            ''' <summary>申請貸出終了日</summary>
            Public Const USER_END_DT As String = "_USER_END_DT"
            ''' <summary>貸出先</summary>
            Public Const LEASE_PLACE_CD As String = "_LEASE_PLACE_CD"
            ''' <summary>貸出用途</summary>
            Public Const LEASE_USE As String = "_LEASE_USE"
            ''' <summary>送り先名</summary>
            Public Const DEST_NAME As String = "_DEST_NAME"
            ''' <summary>送り先住所</summary>
            Public Const DEST_ADDRESS As String = "_DEST_ADDRESS"
            ''' <summary>送り先備考</summary>
            Public Const REMARK As String = "_REMARK"
            ''' <summary>社内用備考</summary>
            Public Const REMARK_INTERNAL As String = "_REMARK_INTERNAL"
            ''' <summary>送り状No</summary>
            Public Const DEST_NO As String = "_DEST_NO"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>シリアルNo</summary>
            Public Const SERIAL_NO As String = "_SERIAL_NO"
            ''' <summary>資産実質貸出開始日</summary>
            Public Const DTL_REAL_START_DT As String = "_DTL_REAL_START_DT"
            ''' <summary>資産実質貸出終了日</summary>
            Public Const DTL_REAL_END_DT As String = "_DTL_REAL_END_DT"
            ''' <summary>実貸出日</summary>
            Public Const DTL_LENDING_DT As String = "_DTL_LENDING_DT"
            ''' <summary>実返却日</summary>
            Public Const DTL_RETURN_DT As String = "_DTL_RETURN_DT"
            ''' <summary>資産担当者</summary>
            Public Const DTL_CHARGE_EMP_CD As String = "_DTL_CHARGE_EMP_CD"
        End Class

        ''' <summary>予約一覧</summary>
        Public Class LendingList
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>予約番号</summary>
            Public Const RESERVE_ID As String = "_RESERVE_ID"
            ''' <summary>貸出状態</summary>
            Public Const RESERVE_STATUS As String = "_RESERVE_STATUS"
            ''' <summary>担当者</summary>
            Public Const CHARGE_EMP_CD As String = "_CHARGE_EMP_CD"
            ''' <summary>管理ロケーション</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>貸出先名</summary>
            Public Const LEASE_PLACE_CD As String = "_LEASE_PLACE_CD"
            ''' <summary>送り先名</summary>
            Public Const DEST_NAME As String = "_DEST_NAME"
            ''' <summary>実質貸出開始日</summary>
            Public Const REAL_START_DT As String = "_REAL_START_DT"
            ''' <summary>実質貸出終了日</summary>
            Public Const REAL_END_DT As String = "_REAL_END_DT"
            ''' <summary>送り状番号</summary>
            Public Const DEST_NO As String = "_DEST_NO"
        End Class

        ''' <summary>棚卸実績照会</summary>
        Public Class InventoryAssetHistory
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>サブ資産番号</summary>
            Public Const SUB_ASSET_NO As String = "_SUB_ASSET_NO"
            ''' <summary>資産状態</summary>
            Public Const ASSET_STATUS_NAME As String = "_ASSET_STATUS_NAME"
            ''' <summary>実棚ステータス</summary>
            Public Const PHYINV_STATUS_NAME As String = "_PHYINV_STATUS_NAME"
            ''' <summary>帳簿ロケーション会社コード/名 </summary>
            Public Const BOOK_LOCATION_L_CODE_NAME As String = "_BOOK_LOCATION_L_CODE_NAME"
            ''' <summary>帳簿ロケーション部署コード/名 </summary>
            Public Const BOOK_LOCATION_M_CODE_NAME As String = "_BOOK_LOCATION_M_CODE_NAME"
            ''' <summary>帳簿ロケーション場所コード/名</summary>
            Public Const BOOK_LOCATION_S_CODE_NAME As String = "_BOOK_LOCATION_S_CODE_NAME"
            ''' <summary>実棚ロケーション会社コード/名</summary>
            Public Const PHYINV_LOCATION_L_CODE_NAME As String = "_PHYINV_LOCATION_L_CODE_NAME"
            ''' <summary>実棚ロケーション部署コード/名</summary>
            Public Const PHYINV_LOCATION_M_CODE_NAME As String = "_PHYINV_LOCATION_M_CODE_NAME"
            ''' <summary>実棚ロケーション場所コード/名</summary>
            Public Const PHYINV_LOCATION_S_CODE_NAME As String = "_PHYINV_LOCATION_S_CODE_NAME"
            ''' <summary>カテゴリー大分類コード/名</summary>
            Public Const CAT_L_CD As String = "_CAT_L_CD"
            ''' <summary>有形/無形区分</summary>
            Public Const IS_TANGIBLE As String = "_IS_TANGIBLE"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>実棚日時 </summary>
            Public Const PHYINV_DATE As String = "_PHYINV_DATE"
            ''' <summary>実棚者</summary>
            Public Const PHYINV_PERSON_NAME As String = "_PHYINV_PERSON_NAME"
            ''' <summary>管理者社員番号/名</summary>
            Public Const FREE_ITEM6 As String = "_FREE_ITEM6"
            ''' <summary>資産取得日</summary>
            Public Const FREE_ITEM1 As String = "_FREE_ITEM1"
            ''' <summary>資本化日付</summary>
            Public Const FREE_ITEM2 As String = "_FREE_ITEM2"
            ''' <summary>稼働区分</summary>
            Public Const FREE_ITEM3 As String = "_FREE_ITEM3"
            ''' <summary>稼働区分名</summary>
            Public Const FREE_ITEM4 As String = "_FREE_ITEM4"
            ''' <summary>型式</summary>
            Public Const FREE_ITEM5 As String = "_FREE_ITEM5"
            ''' <summary>シリアルNo</summary>
            Public Const FREE_ITEM7 As String = "_FREE_ITEM7"
            ''' <summary>WBS番号</summary>
            Public Const FREE_ITEM8 As String = "_FREE_ITEM8"
            ''' <summary>備考</summary>
            Public Const FREE_ITEM9 As String = "_FREE_ITEM9"
            ''' <summary>貸与資産状態区分</summary>
            Public Const FREE_ITEM10 As String = "_FREE_ITEM10"
            ''' <summary>取得金額</summary>
            Public Const FREE_ITEM11 As String = "_FREE_ITEM11"
            ''' <summary>当月末簿価</summary>
            Public Const FREE_ITEM12 As String = "_FREE_ITEM12"
            ''' <summary>補助番号</summary>
            Public Const FREE_ITEM13 As String = "_FREE_ITEM13"
            ''' <summary>装置管理番号</summary>
            Public Const FREE_ITEM14 As String = "_FREE_ITEM14"
            ''' <summary>資産グループコード</summary>
            Public Const FREE_ITEM15 As String = "_FREE_ITEM15"
            ''' <summary>貸与先</summary>
            Public Const FREE_ITEM16 As String = "_FREE_ITEM16"
            ''' <summary>貸与先名</summary>
            Public Const FREE_ITEM17 As String = "_FREE_ITEM17"
            ''' <summary>開始日(貸与)</summary>
            Public Const FREE_ITEM18 As String = "_FREE_ITEM18"
            ''' <summary>終了日（貸与）</summary>
            Public Const FREE_ITEM19 As String = "_FREE_ITEM19"
            ''' <summary>貸与回数</summary>
            Public Const FREE_ITEM20 As String = "_FREE_ITEM20"
            ''' <summary>備考欄</summary>
            Public Const FREE_ITEM21 As String = "_FREE_ITEM21"
            ''' <summary>会計年度</summary>
            Public Const FREE_ITEM22 As String = "_FREE_ITEM22"
            ''' <summary>会計期間</summary>
            Public Const FREE_ITEM23 As String = "_FREE_ITEM23"
            ''' <summary>テキスト2</summary>
            Public Const FREE_ITEM24 As String = "_FREE_ITEM24"
            ''' <summary>勘定項目</summary>
            Public Const FREE_ITEM25 As String = "_FREE_ITEM25"
            ''' <summary>勘定項目名称</summary>
            Public Const FREE_ITEM26 As String = "_FREE_ITEM26"
            ''' <summary>棚卸ノート</summary>
            Public Const FREE_ITEM27 As String = "_FREE_ITEM27"
            ''' <summary>初回所得日</summary>
            Public Const FREE_ITEM28 As String = "_FREE_ITEM28"
            ''' <summary>数量</summary>
            Public Const FREE_ITEM29 As String = "_FREE_ITEM29"
            ''' <summary>資産番号テキスト</summary>
            Public Const FREE_ITEM30 As String = "_FREE_ITEM30"
            ''' <summary>車両ナンバープレート</summary>
            Public Const FREE_ITEM31 As String = "_FREE_ITEM31"
            ''' <summary>資産休止フラグ</summary>
            Public Const FREE_ITEM32 As String = "_FREE_ITEM32"
            ''' <summary>取得区分</summary>
            Public Const FREE_ITEM33 As String = "_FREE_ITEM33"
            ''' <summary>取得区分名</summary>
            Public Const FREE_ITEM34 As String = "_FREE_ITEM34"
            ''' <summary>資産種類</summary>
            Public Const FREE_ITEM35 As String = "_FREE_ITEM35"
            ''' <summary>資産種類名</summary>
            Public Const FREE_ITEM36 As String = "_FREE_ITEM36"
            ''' <summary>資産リース区分</summary>
            Public Const FREE_ITEM37 As String = "_FREE_ITEM37"
            ''' <summary>市区町村コード</summary>
            Public Const FREE_ITEM38 As String = "_FREE_ITEM38"
            ''' <summary>市区町村コード名</summary>
            Public Const FREE_ITEM39 As String = "_FREE_ITEM39"
            ''' <summary>仕入先</summary>
            Public Const FREE_ITEM40 As String = "_FREE_ITEM40"
            ''' <summary>資産供給者名</summary>
            Public Const FREE_ITEM41 As String = "_FREE_ITEM41"
            ''' <summary>CAPS番号</summary>
            Public Const FREE_ITEM42 As String = "_FREE_ITEM42"
            ''' <summary>稟議#</summary>
            Public Const FREE_ITEM43 As String = "_FREE_ITEM43"
            ''' <summary>リース会社</summary>
            Public Const FREE_ITEM44 As String = "_FREE_ITEM44"
            ''' <summary>リース会社名</summary>
            Public Const FREE_ITEM45 As String = "_FREE_ITEM45"
            ''' <summary>契約番号</summary>
            Public Const FREE_ITEM46 As String = "_FREE_ITEM46"
            ''' <summary>契約日</summary>
            Public Const FREE_ITEM47 As String = "_FREE_ITEM47"
            ''' <summary>リース終了日付</summary>
            Public Const FREE_ITEM48 As String = "_FREE_ITEM48"
            ''' <summary>リース開始日</summary>
            Public Const FREE_ITEM49 As String = "_FREE_ITEM49"
            ''' <summary>リース期間（年）</summary>
            Public Const FREE_ITEM50 As String = "_FREE_ITEM50"
            ''' <summary>リース期間（期）</summary>
            Public Const FREE_ITEM51 As String = "_FREE_ITEM51"
            ''' <summary>リース支払回数</summary>
            Public Const FREE_ITEM52 As String = "_FREE_ITEM52"
            ''' <summary>リース支払</summary>
            Public Const FREE_ITEM53 As String = "_FREE_ITEM53"
            ''' <summary>年利</summary>
            Public Const FREE_ITEM54 As String = "_FREE_ITEM54"
            ''' <summary>フリー項目55</summary>
            Public Const FREE_ITEM55 As String = "_FREE_ITEM55"
            ''' <summary>フリー項目56</summary>
            Public Const FREE_ITEM56 As String = "_FREE_ITEM56"
            ''' <summary>フリー項目57</summary>
            Public Const FREE_ITEM57 As String = "_FREE_ITEM57"
            ''' <summary>フリー項目58</summary>
            Public Const FREE_ITEM58 As String = "_FREE_ITEM58"
            ''' <summary>フリー項目59</summary>
            Public Const FREE_ITEM59 As String = "_FREE_ITEM59"
            ''' <summary>フリー項目60</summary>
            Public Const FREE_ITEM60 As String = "_FREE_ITEM60"
            ''' <summary>タグ紐付け状態</summary>
            Public Const TAG_LINK_STATUS As String = "TAG_LINK_STATUS"
            ''' <summary>変更後稼働区分</summary>
            Public Const OPE_STATUS As String = "_OPE_STATUS"
            ''' <summary>転用or除却</summary>
            Public Const DIVERSION_REMOVE As String = "_DIVERSION_REMOVE"
            ''' <summary>除却時期(予定)</summary>
            Public Const REMOVE_DATE As String = "_REMOVE_DATE"
            ''' <summary>除却理由</summary>
            Public Const REMOVE_REASON As String = "_REMOVE_REASON"
            ''' <summary>除却内容</summary>
            Public Const REMOVE_CONTENT As String = "_REMOVE_CONTENT"
            ''' <summary>変更後課コード/名</summary>
            Public Const CHANGE_LOC_M_CD As String = "_CHANGE_LOC_M_CD"
            ''' <summary>新変更後設置場所コード/名</summary>
            Public Const CHANGE_LOC_S_CD As String = "_CHANGE_LOC_S_CD"
            ''' <summary>変更後使用社員番号/名</summary>
            Public Const CHANGE_EMPLOYEE As String = "_CHANGE_EMPLOYEE"
            ''' <summary>備考1</summary>
            Public Const REMARK1 As String = "_REMARK1"
            ''' <summary>備考2</summary>
            Public Const REMARK2 As String = "_REMARK2"
            ''' <summary>備考3</summary>
            Public Const REMARK3 As String = "_REMARK3"
            ''' <summary>備考4</summary>
            Public Const REMARK4 As String = "_REMARK4"
            ''' <summary>備考5</summary>
            Public Const REMARK5 As String = "_REMARK5"
            ''' <summary>備考6</summary>
            Public Const REMARK6 As String = "_REMARK6"
            ''' <summary>備考7</summary>
            Public Const REMARK7 As String = "_REMARK7"
            ''' <summary>備考8</summary>
            Public Const REMARK8 As String = "_REMARK8"
            ''' <summary>備考9</summary>
            Public Const REMARK9 As String = "_REMARK9"
            ''' <summary>備考10</summary>
            Public Const REMARK10 As String = "_REMARK10"
            '[CC431] ADD Start 20261009
            ''' <summary>棚卸実施端末</summary>
            Public Const PHYINV_TOOL_DIV As String = "_PHYINV_TOOL_DIV"
            ''' <summary>入力作業者コード/名</summary>
            Public Const ENT_EMPLOYEE As String = "_ENT_EMPLOYEE"
            '[CC431] ADD End 20261009
        End Class

        ''' <summary>棚卸実績一覧</summary>

        Public Class InventoryHistory
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>棚卸開始宣言日時</summary>
            Public Const START_DEC_TS As String = "_START_DEC_TS"
            ''' <summary>開始宣言者</summary>
            Public Const START_DEC_NAME As String = "_START_DEC_NAME"
            ''' <summary>棚卸名</summary>
            Public Const INV_NAME As String = "_INV_NAME"
            ''' <summary>棚卸確定日時</summary>
            Public Const INV_CONFIRM_TS As String = "_INV_CONFIRM_TS"
            ''' <summary>棚卸確定者</summary>
            Public Const INV_CONFIRM_NAME As String = "_INV_CONFIRM_NAME"
            ''' <summary>ロケ大区分</summary>
            Public Const INV_LOC_L_KUBUN As String = "_INV_LOC_L_KUBUN"
            ''' <summary>帳簿在庫数</summary>
            Public Const LOGIC_INV_NUM As String = "_LOGIC_INV_NUM"
            ''' <summary>実棚正常数</summary>
            Public Const PHYINV_NUM As String = "_PHYINV_NUM"
            ''' <summary>ロケ違い数</summary>
            Public Const LOC_DIFF_NUM As String = "_LOC_DIFF_NUM"
            ''' <summary>在庫なし数</summary>
            Public Const NO_ITEM_NUM As String = "_NO_ITEM_NUM"
        End Class
        ''' <summary>管理簿出力</summary>
        Public Class DrugManageList
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>ロット番号</summary>
            Public Const LOT_NO As String = "_LOT_NO"
            ''' <summary>購入者</summary>
            Public Const PURCHASER As String = "_PURCHASER"
            ''' <summary>納入日</summary>
            Public Const DUE_DATE As String = "_DUE_DATE"
            ''' <summary>直近確認日</summary>
            Public Const LATEST_CHECK_DATE As String = "_LATEST_CHECK_DATE"

        End Class

        ''' <summary>棚卸進捗状況</summary>
        Public Class InventorySummary
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>ロケ大区分</summary>
            Public Const INV_LOC_L_KUBUN As String = "_INV_LOC_L_KUBUN"
            ''' <summary>棚卸状態</summary>
            Public Const INVENTORY_STATUIS As String = "_INVENTORY_STATUIS"
            ''' <summary>棚卸開始日</summary>
            Public Const INVENTORY_STARTDAY As String = "_INVENTORY_STARTDAY"
            ''' <summary>帳簿在庫数</summary>
            Public Const BOOK_QUANTITY As String = "_BOOK_QUANTITY"
            ''' <summary>内貸出数</summary>
            Public Const NUMBER_OF_LOANS_WITHIN As String = "_NUMBER_OF_LOANS_WITHIN"
            ''' <summary>実棚数</summary>
            Public Const ACTUAL_NUMBER_OF_SHELVES As String = "_ACTUAL_NUMBER_OF_SHELVES"
            ''' <summary>正常数</summary>
            Public Const NORMAL_NUMBER As String = "_NORMAL_NUMBER"
            ''' <summary>ロケ違い数</summary>
            Public Const LOCATION_DIFFERENCE_NUMBER As String = "_LOCATION_DIFFERENCE_NUMBER"
            ''' <summary>在庫なし数</summary>
            Public Const OUT_OF_STOCK As String = "_OUT_OF_STOCK"
            ''' <summary>直近棚卸日</summary>
            Public Const NEAR_INVENTORY_DATE As String = "_NEAR_INVENTORY_DATE"
        End Class

        ''' <summary>実棚照会</summary>
        Public Class InventoryInquiry
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"

            ''' <summary>資産状態 </summary>
            Public Const ASSET_STATUS As String = "_ASSET_STATUS_NAME"
            ''' <summary>ロック担当者コード/名</summary>
            Public Const LOCK_EMPLOYEE_CD As String = "_LOCK_EMPLOYEE_CD"
            ''' <summary>実棚ステータス </summary>
            Public Const PHYINV_STATUS As String = "_PHYINV_STATUS"
            ''' <summary>会社コード/名 </summary>
            Public Const LOC_L_CD As String = "_LOC_L_CD"
            ''' <summary>部署コード/名 </summary>
            Public Const LOC_M_CD As String = "_LOC_M_CD"
            ''' <summary>場所コード/名 </summary>
            Public Const LOC_S_CD As String = "_LOC_S_CD"
            ''' <summary>実棚会社コード/名 </summary>
            Public Const PHYINV_LOC_L_CD As String = "_PHYINV_LOC_L_CD"
            ''' <summary>実棚部署コード/名 </summary>
            Public Const PHYINV_LOC_M_CD As String = "_PHYINV_LOC_M_CD"
            ''' <summary>実棚場所コード/名 </summary>
            Public Const PHYINV_LOC_S_CD As String = "_PHYINV_LOC_S_CD"
            ''' <summary>カテゴリー大分類コード/名</summary>
            Public Const CAT_L_CD As String = "_CAT_L_CD"
            ''' <summary>有形・無形区分</summary>
            Public Const IS_TANGIBLE As String = "_IS_TANGIBLE"
            ''' <summary>資産番号 </summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>サブ資産番号 </summary>
            Public Const SUB_ASSET_NO As String = "_SUB_ASSET_NO"
            ''' <summary>資産名称 </summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>実棚日時 </summary>
            Public Const PHYINV_TS As String = "_PHYINV_TS"
            ''' <summary>実棚者 </summary>
            Public Const PHYINV_EMPLOYEE_CD As String = "_PHYINV_EMPLOYEE_CD"
            ''' <summary>資産管理者社員番号/名</summary>
            Public Const FREE_ITEM6 As String = "_FREE_ITEM6"
            ''' <summary>資産取得日</summary>
            Public Const FREE_ITEM1 As String = "_FREE_ITEM1"
            ''' <summary>資本化日付</summary>
            Public Const FREE_ITEM2 As String = "_FREE_ITEM2"
            ''' <summary>稼働区分</summary>
            Public Const FREE_ITEM3 As String = "_FREE_ITEM3"
            ''' <summary>稼働区分名</summary>
            Public Const FREE_ITEM4 As String = "_FREE_ITEM4"
            ''' <summary>型式</summary>
            Public Const FREE_ITEM5 As String = "_FREE_ITEM5"
            ''' <summary>シリアルNo</summary>
            Public Const FREE_ITEM7 As String = "_FREE_ITEM7"
            ''' <summary>WBS番号</summary>
            Public Const FREE_ITEM8 As String = "_FREE_ITEM8"
            ''' <summary>備考</summary>
            Public Const FREE_ITEM9 As String = "_FREE_ITEM9"
            ''' <summary>貸与資産状態</summary>
            Public Const FREE_ITEM10 As String = "_FREE_ITEM10"
            ''' <summary>所得金額</summary>
            Public Const FREE_ITEM11 As String = "_FREE_ITEM11"
            ''' <summary>当月末簿価</summary>
            Public Const FREE_ITEM12 As String = "_FREE_ITEM12"
            ''' <summary>補助番号</summary>
            Public Const FREE_ITEM13 As String = "_FREE_ITEM13"
            ''' <summary>装置管理番号</summary>
            Public Const FREE_ITEM14 As String = "_FREE_ITEM14"
            ''' <summary>資産グループ</summary>
            Public Const FREE_ITEM15 As String = "_FREE_ITEM15"
            ''' <summary>貸与先</summary>
            Public Const FREE_ITEM16 As String = "_FREE_ITEM16"
            ''' <summary>貸与先名</summary>
            Public Const FREE_ITEM17 As String = "_FREE_ITEM17"
            ''' <summary>開始日</summary>
            Public Const FREE_ITEM18 As String = "_FREE_ITEM18"
            ''' <summary>終了日</summary>
            Public Const FREE_ITEM19 As String = "_FREE_ITEM19"
            ''' <summary>貸与回数</summary>
            Public Const FREE_ITEM20 As String = "_FREE_ITEM20"
            ''' <summary>備考</summary>
            Public Const FREE_ITEM21 As String = "_FREE_ITEM21"
            ''' <summary>会計年度</summary>
            Public Const FREE_ITEM22 As String = "_FREE_ITEM22"
            ''' <summary>会計期間</summary>
            Public Const FREE_ITEM23 As String = "_FREE_ITEM23"
            ''' <summary>テキスト2</summary>
            Public Const FREE_ITEM24 As String = "_FREE_ITEM24"
            ''' <summary>勘定項目</summary>
            Public Const FREE_ITEM25 As String = "_FREE_ITEM25"
            ''' <summary>勘定項目名称</summary>
            Public Const FREE_ITEM26 As String = "_FREE_ITEM26"
            ''' <summary>最終棚卸日</summary>
            Public Const LAST_INVENTORY_DT As String = "_LAST_INVENTORY_DT"
            ''' <summary>棚卸ノート</summary>
            Public Const FREE_ITEM27 As String = "_FREE_ITEM27"
            ''' <summary>初回所得日</summary>
            Public Const FREE_ITEM28 As String = "_FREE_ITEM28"
            ''' <summary>数量</summary>
            Public Const FREE_ITEM29 As String = "_FREE_ITEM29"
            ''' <summary>資産番号テキスト</summary>
            Public Const FREE_ITEM30 As String = "_FREE_ITEM30"
            ''' <summary>車両ナンバープレート</summary>
            Public Const FREE_ITEM31 As String = "_FREE_ITEM31"
            ''' <summary>資産休止フラグ</summary>
            Public Const FREE_ITEM32 As String = "_FREE_ITEM32"
            ''' <summary>取得区分</summary>
            Public Const FREE_ITEM33 As String = "_FREE_ITEM33"
            ''' <summary>取得区分名</summary>
            Public Const FREE_ITEM34 As String = "_FREE_ITEM34"
            ''' <summary>資産種類</summary>
            Public Const FREE_ITEM35 As String = "_FREE_ITEM35"
            ''' <summary>資産種類名</summary>
            Public Const FREE_ITEM36 As String = "_FREE_ITEM36"
            ''' <summary>資産リース区分</summary>
            Public Const FREE_ITEM37 As String = "_FREE_ITEM37"
            ''' <summary>市区町村コード</summary>
            Public Const FREE_ITEM38 As String = "_FREE_ITEM38"
            ''' <summary>市区町村コード名</summary>
            Public Const FREE_ITEM39 As String = "_FREE_ITEM39"
            ''' <summary>仕入先</summary>
            Public Const FREE_ITEM40 As String = "_FREE_ITEM40"
            ''' <summary>資産供給者名</summary>
            Public Const FREE_ITEM41 As String = "_FREE_ITEM41"
            ''' <summary>CAPS番号</summary>
            Public Const FREE_ITEM42 As String = "_FREE_ITEM42"
            ''' <summary>稟議#</summary>
            Public Const FREE_ITEM43 As String = "_FREE_ITEM43"
            ''' <summary>リース会社</summary>
            Public Const FREE_ITEM44 As String = "_FREE_ITEM44"
            ''' <summary>リース会社名</summary>
            Public Const FREE_ITEM45 As String = "_FREE_ITEM45"
            ''' <summary>契約番号</summary>
            Public Const FREE_ITEM46 As String = "_FREE_ITEM46"
            ''' <summary>契約日</summary>
            Public Const FREE_ITEM47 As String = "_FREE_ITEM47"
            ''' <summary>リース終了日付</summary>
            Public Const FREE_ITEM48 As String = "_FREE_ITEM48"
            ''' <summary>リース開始日</summary>
            Public Const FREE_ITEM49 As String = "_FREE_ITEM49"
            ''' <summary>リース期間（年）</summary>
            Public Const FREE_ITEM50 As String = "_FREE_ITEM50"
            ''' <summary>リース期間（期）</summary>
            Public Const FREE_ITEM51 As String = "_FREE_ITEM51"
            ''' <summary>リース支払回数</summary>
            Public Const FREE_ITEM52 As String = "_FREE_ITEM52"
            ''' <summary>リース支払</summary>
            Public Const FREE_ITEM53 As String = "_FREE_ITEM53"
            ''' <summary>年利</summary>
            Public Const FREE_ITEM54 As String = "_FREE_ITEM54"
            ''' <summary>フリー項目55</summary>
            Public Const FREE_ITEM55 As String = "_FREE_ITEM55"
            ''' <summary>フリー項目56</summary>
            Public Const FREE_ITEM56 As String = "_FREE_ITEM56"
            ''' <summary>フリー項目57</summary>
            Public Const FREE_ITEM57 As String = "_FREE_ITEM57"
            ''' <summary>フリー項目58</summary>
            Public Const FREE_ITEM58 As String = "_FREE_ITEM58"
            ''' <summary>フリー項目59</summary>
            Public Const FREE_ITEM59 As String = "_FREE_ITEM59"
            ''' <summary>フリー項目60</summary>
            Public Const FREE_ITEM60 As String = "_FREE_ITEM60"
            ''' <summary>タグ紐付け状態</summary>
            Public Const TAG_LINK_STATUS As String = "TAG_LINK_STATUS"
            ''' <summary>稼働区分</summary>
            Public Const OPE_STATUS As String = "_OPE_STATUS"
            ''' <summary>転用or除却</summary>
            Public Const DIVERSION_REMOVE As String = "_DIVERSION_REMOVE"
            ''' <summary>除却時期(予定)</summary>
            Public Const REMOVE_DATE As String = "_REMOVE_DATE"
            ''' <summary>除却理由</summary>
            Public Const REMOVE_REASON As String = "_REMOVE_REASON"
            ''' <summary>除却内容</summary>
            Public Const REMOVE_CONTENT As String = "_REMOVE_CONTENT"
            ''' <summary>変更後課コード</summary>
            Public Const CHANGE_LOC_M_CD As String = "_CHANGE_LOC_M_CD"
            ''' <summary>変更後課名称</summary>
            Public Const CHANGE_LOC_M_NAME As String = "_CHANGE_LOC_M_NAME"
            ''' <summary>新変更後設置場所コード</summary>
            Public Const CHANGE_LOC_S_CD As String = "_CHANGE_LOC_S_CD"
            ''' <summary>新変更後設置場所名称</summary>
            Public Const CHANGE_LOC_S_NAME As String = "_CHANGE_LOC_S_NAME"
            ''' <summary>変更後使用社員番号</summary>
            Public Const CHANGE_EMPLOYEE As String = "_CHANGE_EMPLOYEE"
            ''' <summary>変更後使用社員名</summary>
            Public Const CHANGE_EMPLOYEE_NAME As String = "_CHANGE_EMPLOYEE_NAME"
            ''' <summary>備考1</summary>
            Public Const REMARK1 As String = "_REMARK1"
            ''' <summary>備考2</summary>
            Public Const REMARK2 As String = "_REMARK2"
            ''' <summary>備考3</summary>
            Public Const REMARK3 As String = "_REMARK3"
            ''' <summary>備考4</summary>
            Public Const REMARK4 As String = "_REMARK4"
            ''' <summary>備考5</summary>
            Public Const REMARK5 As String = "_REMARK5"
            ''' <summary>備考6</summary>
            Public Const REMARK6 As String = "_REMARK6"
            ''' <summary>備考7</summary>
            Public Const REMARK7 As String = "_REMARK7"
            ''' <summary>備考8</summary>
            Public Const REMARK8 As String = "_REMARK8"
            ''' <summary>備考9</summary>
            Public Const REMARK9 As String = "_REMARK9"
            ''' <summary>備考10</summary>
            Public Const REMARK10 As String = "_REMARK10"
            '[CC431] ADD Start 20261002
            ''' <summary>棚卸実施端末</summary>
            Public Const PHYINV_TOOL_DIV As String = "_PHYINV_TOOL_DIV"
            ''' <summary>入力作業者コード/名</summary>
            Public Const ENT_EMPLOYEE As String = "_ENT_EMPLOYEE"
            '[CC431] ADD End 20261002

        End Class

        ''' <summary>棚卸ロケ別実績一覧(上部)</summary>
        Public Class InventoryLocHistoryTop
            ''' <summary>棚卸開始宣言日時</summary>
            Public Const START_DEC_TS As String = "_START_DEC_TS"
            ''' <summary>開始宣言者</summary>
            Public Const START_DEC_NAME As String = "_START_DEC_NAME"
            ''' <summary>棚卸名</summary>
            Public Const INV_NAME As String = "_INV_NAME"
            ''' <summary>棚卸確定日時</summary>
            Public Const INV_CONFIRM_TS As String = "_INV_CONFIRM_TS"
            ''' <summary>棚卸確定者</summary>
            Public Const INV_CONFIRM_NAME As String = "_INV_CONFIRM_NAME"
        End Class

        ''' <summary>棚卸ロケ別実績一覧(下部)</summary>
        Public Class InventoryLocHistoryBottom
            ''' <summary>ロケ大区分</summary>
            Public Const LOC_L_CD As String = "_LOC_L_CD"
            ''' <summary>帳簿在庫数</summary>
            Public Const BOOK_INVENTORY_NUMBER As String = "_BOOK_INVENTORY_NUMBER"
            ''' <summary>内貸出数</summary>
            Public Const NUMBER_OF_LOANS_WITHIN As String = "_NUMBER_OF_LOANS_WITHIN"
            ''' <summary>実棚数</summary>
            Public Const ACTUAL_NUMBER_OF_SHELVES As String = "_ACTUAL_NUMBER_OF_SHELVES"
            ''' <summary>正常数</summary>
            Public Const NORMAL_NUMBER As String = "_NORMAL_NUMBER"
            ''' <summary>ロケ違い数</summary>
            Public Const LOCATION_DIFFERENCE_NUMBER As String = "_LOCATION_DIFFERENCE_NUMBER"
            ''' <summary>在庫なし数</summary>
            Public Const OUT_OF_STOCK As String = "_OUT_OF_STOCK"
        End Class

        ''' <summary>
        ''' 貸出履歴照会
        ''' </summary>
        Public Class AssetLeaseHistory
            ''' <summary>管理ロケーションコード/名</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>カテゴリー大分類コード/名</summary>
            Public Const CAT_L_CD As String = "_CAT_L_CD"
            ''' <summary>カテゴリー中分類コード/名</summary>
            Public Const CAT_M_CD As String = "_CAT_M_CD"
            ''' <summary>カテゴリー小分類コード/名</summary>
            Public Const CAT_S_CD As String = "_CAT_S_CD"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>除却</summary>
            Public Const DISPOSE As String = "_DISPOSE"
            ''' <summary>貸出回数</summary>
            Public Const LEASE_CNT As String = "_LEASE_CNT"
            ''' <summary>貸出日数</summary>
            Public Const LEASE_DAY As String = "_LEASE_DAY"
            ''' <summary>メンテ日数</summary>
            Public Const MAINTE_DAY As String = "_MAINTE_DAY"
            ''' <summary>営業日数</summary>
            Public Const EIGYO_DAY As String = "_EIGYO_DAY"
            ''' <summary>貸出率</summary>
            Public Const LENDING_RATE As String = "_LENDING_RATE"
        End Class

        ''' <summary>
        ''' 操作履歴
        ''' </summary>
        Public Class OperationLog
            ''' <summary>処理日時</summary>
            Public Const PROCESS_DATETIME As String = "_PROCESS_DATETIME"
            ''' <summary>処理種別</summary>
            Public Const PROCESS_CAT As String = "_PROCESS_CAT"
            ''' <summary>処理分類</summary>
            Public Const PROCESS_TOOL_DIV As String = "_PROCESS_TOOL_DIV"
            ''' <summary>操作社員コード/名</summary>
            Public Const OPE_EMPLOYEE_CD As String = "_OPE_EMPLOYEE_CD"
            ''' <summary>処理内容</summary>
            Public Const PROCESS_CONTENTS As String = "_PROCESS_CONTENTS"
            ''' <summary>帳簿ﾛｹｰｼｮﾝ大区分ｺｰﾄﾞ/名</summary>
            Public Const BOOK_LOCATION_L_CODE_NAME As String = "_BOOK_LOCATION_L_CODE_NAME"
            ''' <summary>帳簿ﾛｹｰｼｮﾝ中区分ｺｰﾄﾞ/名</summary>
            Public Const BOOK_LOCATION_M_CODE_NAME As String = "_BOOK_LOCATION_M_CODE_NAME"
            ''' <summary>帳簿ﾛｹｰｼｮﾝ小区分ｺｰﾄﾞ/名</summary>
            Public Const BOOK_LOCATION_S_CODE_NAME As String = "_BOOK_LOCATION_S_CODE_NAME"
        End Class

        ''' <summary>
        ''' 予約申請
        ''' </summary>
        Public Class LendingEntry
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>シリアルNo</summary>
            Public Const SERIAL_NO As String = "_SERIAL_NO"
            ''' <summary>貸出状態</summary>
            Public Const LEASE_STATUS As String = "_LEASE_STATUS"
            ''' <summary>貸出期間</summary>
            Public Const LEASE_KIKAN As String = "_LEASE_KIKAN"
            ''' <summary>貸出期限</summary>
            Public Const ASSET_LEASE_LIMIT As String = "_ASSET_LEASE_LIMIT"
            ''' <summary>担当者</summary>
            Public Const EMPLOYEE_CD As String = "_EMPLOYEE_CD"
        End Class

        Public Class LendingStatus
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>カテゴリー大分類コード/名</summary>
            Public Const CAT_L As String = "_CAT_L"
            ''' <summary>カテゴリー中分類コード/名</summary>
            Public Const CAT_M As String = "_CAT_M"
            ''' <summary>カテゴリー小分類コード/名</summary>
            Public Const CAT_S As String = "_CAT_S"
            ''' <summary>管理ロケーション</summary>
            Public Const MANAGE_LOC As String = "_MANAGE_LOC"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>型番</summary>
            Public Const LOT_NO As String = "_LOT_NO"
            ''' <summary>資産状態</summary>
            Public Const ASSET_STATUS As String = "_ASSET_STATUS"
            ''' <summary>出荷通知</summary>
            Public Const SHIPPING_NOTIFY As String = "_SHIPPING_NOTIFY"
            ''' <summary>品番</summary>
            Public Const PRODUCT_NO As String = "_PRODUCT_NO"
        End Class

        ''' <summary>工事一覧</summary>
        Public Class ConstructionList
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>工事番号</summary>
            Public Const CONST_NO As String = "_CONST_NO"
            ''' <summary>工事開始日</summary>
            Public Const CONST_START_DT As String = "_CONST_START_DT"
            ''' <summary>工事終了日</summary>
            Public Const CONST_END_DT As String = "_CONST_END_DT"
            ''' <summary>工事貸出開始日</summary>
            Public Const CONST_LENT_START_DT As String = "_CONST_LENT_START_DT"
            ''' <summary>工事貸出終了日</summary>
            Public Const CONST_LENT_END_DT As String = "_CONST_LENT_END_DT"
            ''' <summary>工事状態</summary>
            Public Const CONST_STATUS As String = "_CONST_STATUS"
            ''' <summary>工事内容</summary>
            Public Const CONST_CONTENTS As String = "_CONST_CONTENTS"
            ''' <summary>工事責任者氏名</summary>
            Public Const CONST_EMPLOYEE_NAME As String = "_CONST_EMPLOYEE_NAME"
            ''' <summary>拠点ロケ</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>工事情報1</summary>
            Public Const CONST_INFO1 As String = "_CONST_INFO1"
            ''' <summary>工事情報2</summary>
            Public Const CONST_INFO2 As String = "_CONST_INFO2"
            ''' <summary>工事情報3</summary>
            Public Const CONST_INFO3 As String = "_CONST_INFO3"
            ''' <summary>工事情報4</summary>
            Public Const CONST_INFO4 As String = "_CONST_INFO4"
            ''' <summary>工事情報5</summary>
            Public Const CONST_INFO5 As String = "_CONST_INFO5"
            ''' <summary>工事情報6</summary>
            Public Const CONST_INFO6 As String = "_CONST_INFO6"
            ''' <summary>工事情報7</summary>
            Public Const CONST_INFO7 As String = "_CONST_INFO7"
            ''' <summary>工事情報8</summary>
            Public Const CONST_INFO8 As String = "_CONST_INFO8"
            ''' <summary>工事情報9</summary>
            Public Const CONST_INFO9 As String = "_CONST_INFO9"
            ''' <summary>工事情報10</summary>
            Public Const CONST_INFO10 As String = "_CONST_INFO10"
            ''' <summary>工事情報11</summary>
            Public Const CONST_INFO11 As String = "_CONST_INFO11"
            ''' <summary>工事情報12</summary>
            Public Const CONST_INFO12 As String = "_CONST_INFO12"
            ''' <summary>工事情報13</summary>
            Public Const CONST_INFO13 As String = "_CONST_INFO13"
            ''' <summary>工事情報14</summary>
            Public Const CONST_INFO14 As String = "_CONST_INFO14"
            ''' <summary>工事情報15</summary>
            Public Const CONST_INFO15 As String = "_CONST_INFO15"
            ''' <summary>工事情報16</summary>
            Public Const CONST_INFO16 As String = "_CONST_INFO16"
            ''' <summary>工事情報17</summary>
            Public Const CONST_INFO17 As String = "_CONST_INFO17"
            ''' <summary>工事情報18</summary>
            Public Const CONST_INFO18 As String = "_CONST_INFO18"
            ''' <summary>工事情報19</summary>
            Public Const CONST_INFO19 As String = "_CONST_INFO19"
            ''' <summary>工事情報20</summary>
            Public Const CONST_INFO20 As String = "_CONST_INFO20"
            ''' <summary>前回仮置数</summary>
            Public Const PREV_TEMP_NUM As String = "_PREV_TEMP_NUM"
            ''' <summary>持込数</summary>
            Public Const CONST_IN_NUM As String = "_CONST_IN_NUM"
            ''' <summary>今回仮置き数</summary>
            Public Const NOW_TEMP_NUM As String = "_NOW_TEMP_NUM"
            ''' <summary>持帰数</summary>
            Public Const CONST_OUT_NUM As String = "_CONST_OUT_NUM"
            ''' <summary>現在貸出数</summary>
            Public Const CUR_LENT_NUM As String = "_CUR_LENT_NUM"
            ''' <summary>現在仮置き数</summary>
            Public Const CUR_TEMP_NUM As String = "_CUR_TEMP_NUM"
            ''' <summary>現場確認者氏名</summary>
            Public Const SITE_EMPLOYEE_NAME As String = "_SITE_EMPLOYEE_NAME"
            ''' <summary>工事完了日時</summary>
            Public Const CONST_CONFIRM_TS As String = "_CONST_CONFIRM_TS"
            ''' <summary>工事完了者氏名</summary>
            Public Const CONST_CONFIRM_EMPLOYEE_NAME As String = "_CONST_CONFIRM_EMPLOYEE_NAME"
            ''' <summary>工事完了者備考</summary>
            Public Const CONST_CONFIRM_REMARK As String = "_CONST_CONFIRM_REMARK"
        End Class

        ''' <summary>仮置一覧</summary>
        Public Class TemporaryList
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>仮置き紐付状態</summary>
            Public Const REL_CONST_STATUS As String = "_REL_CONST_STATUS"
            ''' <summary>紐付工事番号</summary>
            Public Const REL_CONST_NO As String = "_REL_CONST_NO"
            ''' <summary>登録工事番号</summary>
            Public Const REG_CONST_NO As String = "_REG_CONST_NO"
            ''' <summary>仮置き日</summary>
            Public Const TEMP_DT As String = "_TEMP_DT"
            ''' <summary>作業内容</summary>
            Public Const CONST_CONTENTS As String = "_CONST_CONTENTS"
            ''' <summary>仮置き情報1</summary>
            Public Const TEMP_INFO1 As String = "_TEMP_INFO1"
            ''' <summary>仮置き情報2</summary>
            Public Const TEMP_INFO2 As String = "_TEMP_INFO2"
            ''' <summary>仮置き情報3</summary>
            Public Const TEMP_INFO3 As String = "_TEMP_INFO3"
            ''' <summary>仮置き情報4</summary>
            Public Const TEMP_INFO4 As String = "_TEMP_INFO4"
            ''' <summary>仮置き情報5</summary>
            Public Const TEMP_INFO5 As String = "_TEMP_INFO5"
            ''' <summary>仮置き情報6</summary>
            Public Const TEMP_INFO6 As String = "_TEMP_INFO6"
            ''' <summary>仮置き情報7</summary>
            Public Const TEMP_INFO7 As String = "_TEMP_INFO7"
            ''' <summary>仮置き備考</summary>
            Public Const TEMP_REMARK As String = "_TEMP_REMARK"
            ''' <summary>仮置き数</summary>
            Public Const TEMP_NUM As String = "_TEMP_NUM"
        End Class

        ''' <summary>仮置資産一覧(上部)</summary>
        Public Class TempAssetListTop
            ''' <summary>仮置き紐付状態</summary>
            Public Const REL_CONST_STATUS As String = "_REL_CONST_STATUS"
            ''' <summary>紐付工事番号</summary>
            Public Const REL_CONST_NO As String = "_REL_CONST_NO"
            ''' <summary>登録工事番号</summary>
            Public Const REG_CONST_NO As String = "_REG_CONST_NO"
            ''' <summary>仮置き日</summary>
            Public Const TEMP_DT As String = "_TEMP_DT"
            ''' <summary>作業内容</summary>
            Public Const CONST_CONTENTS As String = "_CONST_CONTENTS"
            ''' <summary>仮置き情報1</summary>
            Public Const TEMP_INFO1 As String = "_TEMP_INFO1"
            ''' <summary>仮置き情報2</summary>
            Public Const TEMP_INFO2 As String = "_TEMP_INFO2"
            ''' <summary>仮置き情報3</summary>
            Public Const TEMP_INFO3 As String = "_TEMP_INFO3"
            ''' <summary>仮置き情報4</summary>
            Public Const TEMP_INFO4 As String = "_TEMP_INFO4"
            ''' <summary>仮置き情報5</summary>
            Public Const TEMP_INFO5 As String = "_TEMP_INFO5"
            ''' <summary>仮置き情報6</summary>
            Public Const TEMP_INFO6 As String = "_TEMP_INFO6"
            ''' <summary>仮置き情報7</summary>
            Public Const TEMP_INFO7 As String = "_TEMP_INFO7"
            ''' <summary>仮置き備考</summary>
            Public Const TEMP_REMARK As String = "_TEMP_REMARK"
            ''' <summary>仮置き数</summary>
            Public Const TEMP_NUM As String = "_TEMP_NUM"
        End Class

        ''' <summary>仮置資産一覧(下部)</summary>
        Public Class TempAssetListBottom
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>資産名</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
        End Class

        ''' <summary>工事資産一覧(上部)</summary>
        Public Class ConstAssetListTop
            ''' <summary>工事番号</summary>
            Public Const CONST_NO As String = "_CONST_NO"
            ''' <summary>工事開始日</summary>
            Public Const CONST_START_DT As String = "_CONST_START_DT"
            ''' <summary>工事終了日</summary>
            Public Const CONST_END_DT As String = "_CONST_END_DT"
            ''' <summary>工事貸出開始日</summary>
            Public Const CONST_LENT_START_DT As String = "_CONST_LENT_START_DT"
            ''' <summary>工事貸出終了日</summary>
            Public Const CONST_LENT_END_DT As String = "_CONST_LENT_END_DT"
            ''' <summary>工事状態</summary>
            Public Const CONST_STATUS As String = "_CONST_STATUS"
            ''' <summary>工事内容</summary>
            Public Const CONST_CONTENTS As String = "_CONST_CONTENTS"
            ''' <summary>工事責任者氏名</summary>
            Public Const CONST_EMPLOYEE_NAME As String = "_CONST_EMPLOYEE_NAME"
            ''' <summary>拠点ロケ</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>工事情報1</summary>
            Public Const CONST_INFO1 As String = "_CONST_INFO1"
            ''' <summary>工事情報2</summary>
            Public Const CONST_INFO2 As String = "_CONST_INFO2"
            ''' <summary>工事情報3</summary>
            Public Const CONST_INFO3 As String = "_CONST_INFO3"
            ''' <summary>工事情報4</summary>
            Public Const CONST_INFO4 As String = "_CONST_INFO4"
            ''' <summary>工事情報5</summary>
            Public Const CONST_INFO5 As String = "_CONST_INFO5"
            ''' <summary>工事情報6</summary>
            Public Const CONST_INFO6 As String = "_CONST_INFO6"
            ''' <summary>工事情報7</summary>
            Public Const CONST_INFO7 As String = "_CONST_INFO7"
            ''' <summary>工事情報8</summary>
            Public Const CONST_INFO8 As String = "_CONST_INFO8"
            ''' <summary>工事情報9</summary>
            Public Const CONST_INFO9 As String = "_CONST_INFO9"
            ''' <summary>工事情報10</summary>
            Public Const CONST_INFO10 As String = "_CONST_INFO10"
            ''' <summary>工事情報11</summary>
            Public Const CONST_INFO11 As String = "_CONST_INFO11"
            ''' <summary>工事情報12</summary>
            Public Const CONST_INFO12 As String = "_CONST_INFO12"
            ''' <summary>工事情報13</summary>
            Public Const CONST_INFO13 As String = "_CONST_INFO13"
            ''' <summary>工事情報14</summary>
            Public Const CONST_INFO14 As String = "_CONST_INFO14"
            ''' <summary>工事情報15</summary>
            Public Const CONST_INFO15 As String = "_CONST_INFO15"
            ''' <summary>工事情報16</summary>
            Public Const CONST_INFO16 As String = "_CONST_INFO16"
            ''' <summary>工事情報17</summary>
            Public Const CONST_INFO17 As String = "_CONST_INFO17"
            ''' <summary>工事情報18</summary>
            Public Const CONST_INFO18 As String = "_CONST_INFO18"
            ''' <summary>工事情報19</summary>
            Public Const CONST_INFO19 As String = "_CONST_INFO19"
            ''' <summary>工事情報20</summary>
            Public Const CONST_INFO20 As String = "_CONST_INFO20"
            ''' <summary>前回仮置数</summary>
            Public Const PREV_TEMP_NUM As String = "_PREV_TEMP_NUM"
            ''' <summary>持込数</summary>
            Public Const CONST_IN_NUM As String = "_CONST_IN_NUM"
            ''' <summary>今回仮置き数</summary>
            Public Const NOW_TEMP_NUM As String = "_NOW_TEMP_NUM"
            ''' <summary>持帰数</summary>
            Public Const CONST_OUT_NUM As String = "_CONST_OUT_NUM"
            ''' <summary>現在貸出数</summary>
            Public Const CUR_LENT_NUM As String = "_CUR_LENT_NUM"
            ''' <summary>現在仮置き数</summary>
            Public Const CUR_TEMP_NUM As String = "_CUR_TEMP_NUM"
            ''' <summary>現場確認者氏名</summary>
            Public Const SITE_EMPLOYEE_NAME As String = "_SITE_EMPLOYEE_NAME"
            ''' <summary>工事完了日時</summary>
            Public Const CONST_CONFIRM_TS As String = "_CONST_CONFIRM_TS"
            ''' <summary>工事完了者氏名</summary>
            Public Const CONST_CONFIRM_EMPLOYEE_NAME As String = "_CONST_CONFIRM_EMPLOYEE_NAME"
            ''' <summary>工事完了者備考</summary>
            Public Const CONST_CONFIRM_REMARK As String = "_CONST_CONFIRM_REMARK"
        End Class

        ''' <summary>工事資産一覧(下部)</summary>
        Public Class ConstAssetListBottom
            ''' <summary>工事番号</summary>
            Public Const CONST_NO As String = "_CONST_NO"
            ''' <summary>工事開始日</summary>
            Public Const CONST_START_DT As String = "_CONST_START_DT"
            ''' <summary>工事終了日</summary>
            Public Const CONST_END_DT As String = "_CONST_END_DT"
            ''' <summary>工事貸出開始日</summary>
            Public Const CONST_LENT_START_DT As String = "_CONST_LENT_START_DT"
            ''' <summary>工事貸出終了日</summary>
            Public Const CONST_LENT_END_DT As String = "_CONST_LENT_END_DT"
            ''' <summary>工事状態</summary>
            Public Const CONST_STATUS As String = "_CONST_STATUS"
            ''' <summary>工事内容</summary>
            Public Const CONST_CONTENTS As String = "_CONST_CONTENTS"
            ''' <summary>工事責任者氏名</summary>
            Public Const CONST_EMPLOYEE_NAME As String = "_CONST_EMPLOYEE_NAME"
            ''' <summary>拠点ロケ</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>工事情報1</summary>
            Public Const CONST_INFO1 As String = "_CONST_INFO1"
            ''' <summary>工事情報2</summary>
            Public Const CONST_INFO2 As String = "_CONST_INFO2"
            ''' <summary>工事情報3</summary>
            Public Const CONST_INFO3 As String = "_CONST_INFO3"
            ''' <summary>工事情報4</summary>
            Public Const CONST_INFO4 As String = "_CONST_INFO4"
            ''' <summary>工事情報5</summary>
            Public Const CONST_INFO5 As String = "_CONST_INFO5"
            ''' <summary>工事情報6</summary>
            Public Const CONST_INFO6 As String = "_CONST_INFO6"
            ''' <summary>工事情報7</summary>
            Public Const CONST_INFO7 As String = "_CONST_INFO7"
            ''' <summary>工事情報8</summary>
            Public Const CONST_INFO8 As String = "_CONST_INFO8"
            ''' <summary>工事情報9</summary>
            Public Const CONST_INFO9 As String = "_CONST_INFO9"
            ''' <summary>工事情報10</summary>
            Public Const CONST_INFO10 As String = "_CONST_INFO10"
            ''' <summary>工事情報11</summary>
            Public Const CONST_INFO11 As String = "_CONST_INFO11"
            ''' <summary>工事情報12</summary>
            Public Const CONST_INFO12 As String = "_CONST_INFO12"
            ''' <summary>工事情報13</summary>
            Public Const CONST_INFO13 As String = "_CONST_INFO13"
            ''' <summary>工事情報14</summary>
            Public Const CONST_INFO14 As String = "_CONST_INFO14"
            ''' <summary>工事情報15</summary>
            Public Const CONST_INFO15 As String = "_CONST_INFO15"
            ''' <summary>工事情報16</summary>
            Public Const CONST_INFO16 As String = "_CONST_INFO16"
            ''' <summary>工事情報17</summary>
            Public Const CONST_INFO17 As String = "_CONST_INFO17"
            ''' <summary>工事情報18</summary>
            Public Const CONST_INFO18 As String = "_CONST_INFO18"
            ''' <summary>工事情報19</summary>
            Public Const CONST_INFO19 As String = "_CONST_INFO19"
            ''' <summary>工事情報20</summary>
            Public Const CONST_INFO20 As String = "_CONST_INFO20"
            ''' <summary>前回仮置数</summary>
            Public Const PREV_TEMP_NUM As String = "_PREV_TEMP_NUM"
            ''' <summary>持込数</summary>
            Public Const CONST_IN_NUM As String = "_CONST_IN_NUM"
            ''' <summary>今回仮置き数</summary>
            Public Const NOW_TEMP_NUM As String = "_NOW_TEMP_NUM"
            ''' <summary>持帰数</summary>
            Public Const CONST_OUT_NUM As String = "_CONST_OUT_NUM"
            ''' <summary>現在貸出数</summary>
            Public Const CUR_LENT_NUM As String = "_CUR_LENT_NUM"
            ''' <summary>現在仮置き数</summary>
            Public Const CUR_TEMP_NUM As String = "_CUR_TEMP_NUM"
            ''' <summary>現場確認者氏名</summary>
            Public Const SITE_EMPLOYEE_NAME As String = "_SITE_EMPLOYEE_NAME"
            ''' <summary>工事完了日時</summary>
            Public Const CONST_CONFIRM_TS As String = "_CONST_CONFIRM_TS"
            ''' <summary>工事完了者氏名</summary>
            Public Const CONST_CONFIRM_EMPLOYEE_NAME As String = "_CONST_CONFIRM_EMPLOYEE_NAME"
            ''' <summary>工事完了者備考</summary>
            Public Const CONST_CONFIRM_REMARK As String = "_CONST_CONFIRM_REMARK"
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>状態</summary>
            Public Const CONST_ASSET_STATUS As String = "_CONST_ASSET_STATUS"
            ''' <summary>仮置有無</summary>
            Public Const TEMP_YES_OR_NO As String = "_TEMP_YES_OR_NO"
            ''' <summary>仮置き場所</summary>
            Public Const TEMP_INFO As String = "_TEMP_INFO"
            ''' <summary>仮置き備考</summary>
            Public Const TEMP_REMARK As String = "_TEMP_REMARK"
            ''' <summary>ロケーション大区分コード/名</summary>
            Public Const LOC_L_CD_NAME As String = "_LOC_L_CD_NAME"
            ''' <summary>ロケーション中区分コード/名</summary>
            Public Const LOC_M_CD_NAME As String = "_LOC_M_CD_NAME"
            ''' <summary>ロケーション小区分コード/名</summary>
            Public Const LOC_S_CD_NAME As String = "_LOC_S_CD_NAME"
            ''' <summary>カテゴリ大分類コード/名</summary>
            Public Const CAT_L_CD_NAME As String = "_CAT_L_CD_NAME"
            ''' <summary>カテゴリ中分類コード/名</summary>
            Public Const CAT_M_CD_NAME As String = "_CAT_M_CD_NAME"
            ''' <summary>カテゴリ小分類コード/名</summary>
            Public Const CAT_S_CD_NAME As String = "_CAT_S_CD_NAME"
            ''' <summary>資産製造元</summary>
            Public Const MAKER As String = "_MAKER"
            ''' <summary>品番</summary>
            Public Const PRODUCT_NO As String = "_PRODUCT_NO"
            ''' <summary>型番</summary>
            Public Const MODEL_NO As String = "_MODEL_NO"
            ''' <summary>リース管理番号 </summary>
            Public Const LEASE_NO As String = "_LEASE_NO"
            ''' <summary>リース期限 </summary>
            Public Const LEASE_LIMIT As String = "_LEASE_LIMIT"
            ''' <summary>資産購入日</summary>
            Public Const FREE_ITEM1 As String = "_FREE_ITEM1"
            ''' <summary>備考1</summary>
            Public Const FREE_ITEM2 As String = "_FREE_ITEM2"
            ''' <summary>備考2</summary>
            Public Const FREE_ITEM3 As String = "_FREE_ITEM3"
            ''' <summary>備考3</summary>
            Public Const FREE_ITEM4 As String = "_FREE_ITEM4"
            ''' <summary>備考4</summary>
            Public Const FREE_ITEM5 As String = "_FREE_ITEM5"
            ''' <summary>備考5</summary>
            Public Const FREE_ITEM6 As String = "_FREE_ITEM6"
            ''' <summary>備考6</summary>
            Public Const FREE_ITEM7 As String = "_FREE_ITEM7"
            ''' <summary>備考7</summary>
            Public Const FREE_ITEM8 As String = "_FREE_ITEM8"
            ''' <summary>備考8</summary>
            Public Const FREE_ITEM9 As String = "_FREE_ITEM9"
            ''' <summary>備考9</summary>
            Public Const FREE_ITEM10 As String = "_FREE_ITEM10"
            ''' <summary>備考10</summary>
            Public Const FREE_ITEM11 As String = "_FREE_ITEM11"
            ''' <summary>備考11</summary>
            Public Const FREE_ITEM12 As String = "_FREE_ITEM12"
            ''' <summary>備考12</summary>
            Public Const FREE_ITEM13 As String = "_FREE_ITEM13"
            ''' <summary>備考13</summary>
            Public Const FREE_ITEM14 As String = "_FREE_ITEM14"
            ''' <summary>備考14</summary>
            Public Const FREE_ITEM15 As String = "_FREE_ITEM15"
            ''' <summary>備考15</summary>
            Public Const FREE_ITEM16 As String = "_FREE_ITEM16"
            ''' <summary>備考16</summary>
            Public Const FREE_ITEM17 As String = "_FREE_ITEM17"
            ''' <summary>備考17</summary>
            Public Const FREE_ITEM18 As String = "_FREE_ITEM18"
            ''' <summary>備考18</summary>
            Public Const FREE_ITEM19 As String = "_FREE_ITEM19"
        End Class

        ''' <summary>工事履歴一覧(上部)</summary>
        Public Class ConstructionHistTop
            ''' <summary>工事番号</summary>
            Public Const CONST_NO As String = "_CONST_NO"
            ''' <summary>工事開始日</summary>
            Public Const CONST_START_DT As String = "_CONST_START_DT"
            ''' <summary>工事終了日</summary>
            Public Const CONST_END_DT As String = "_CONST_END_DT"
            ''' <summary>工事貸出開始日</summary>
            Public Const CONST_LENT_START_DT As String = "_CONST_LENT_START_DT"
            ''' <summary>工事貸出終了日</summary>
            Public Const CONST_LENT_END_DT As String = "_CONST_LENT_END_DT"
            ''' <summary>工事状態</summary>
            Public Const CONST_STATUS As String = "_CONST_STATUS"
            ''' <summary>工事内容</summary>
            Public Const CONST_CONTENTS As String = "_CONST_CONTENTS"
            ''' <summary>工事責任者氏名</summary>
            Public Const CONST_EMPLOYEE_NAME As String = "_CONST_EMPLOYEE_NAME"
            ''' <summary>拠点ロケ</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>工事情報1</summary>
            Public Const CONST_INFO1 As String = "_CONST_INFO1"
            ''' <summary>工事情報2</summary>
            Public Const CONST_INFO2 As String = "_CONST_INFO2"
            ''' <summary>工事情報3</summary>
            Public Const CONST_INFO3 As String = "_CONST_INFO3"
            ''' <summary>工事情報4</summary>
            Public Const CONST_INFO4 As String = "_CONST_INFO4"
            ''' <summary>工事情報5</summary>
            Public Const CONST_INFO5 As String = "_CONST_INFO5"
            ''' <summary>工事情報6</summary>
            Public Const CONST_INFO6 As String = "_CONST_INFO6"
            ''' <summary>工事情報7</summary>
            Public Const CONST_INFO7 As String = "_CONST_INFO7"
            ''' <summary>工事情報8</summary>
            Public Const CONST_INFO8 As String = "_CONST_INFO8"
            ''' <summary>工事情報9</summary>
            Public Const CONST_INFO9 As String = "_CONST_INFO9"
            ''' <summary>工事情報10</summary>
            Public Const CONST_INFO10 As String = "_CONST_INFO10"
            ''' <summary>工事情報11</summary>
            Public Const CONST_INFO11 As String = "_CONST_INFO11"
            ''' <summary>工事情報12</summary>
            Public Const CONST_INFO12 As String = "_CONST_INFO12"
            ''' <summary>工事情報13</summary>
            Public Const CONST_INFO13 As String = "_CONST_INFO13"
            ''' <summary>工事情報14</summary>
            Public Const CONST_INFO14 As String = "_CONST_INFO14"
            ''' <summary>工事情報15</summary>
            Public Const CONST_INFO15 As String = "_CONST_INFO15"
            ''' <summary>工事情報16</summary>
            Public Const CONST_INFO16 As String = "_CONST_INFO16"
            ''' <summary>工事情報17</summary>
            Public Const CONST_INFO17 As String = "_CONST_INFO17"
            ''' <summary>工事情報18</summary>
            Public Const CONST_INFO18 As String = "_CONST_INFO18"
            ''' <summary>工事情報19</summary>
            Public Const CONST_INFO19 As String = "_CONST_INFO19"
            ''' <summary>工事情報20</summary>
            Public Const CONST_INFO20 As String = "_CONST_INFO20"
            ''' <summary>前回仮置数</summary>
            Public Const PREV_TEMP_NUM As String = "_PREV_TEMP_NUM"
            ''' <summary>持込数</summary>
            Public Const CONST_IN_NUM As String = "_CONST_IN_NUM"
            ''' <summary>今回仮置き数</summary>
            Public Const NOW_TEMP_NUM As String = "_NOW_TEMP_NUM"
            ''' <summary>持帰数</summary>
            Public Const CONST_OUT_NUM As String = "_CONST_OUT_NUM"
            ''' <summary>現在貸出数</summary>
            Public Const CUR_LENT_NUM As String = "_CUR_LENT_NUM"
            ''' <summary>現在仮置き数</summary>
            Public Const CUR_TEMP_NUM As String = "_CUR_TEMP_NUM"
            ''' <summary>現場確認者氏名</summary>
            Public Const SITE_EMPLOYEE_NAME As String = "_SITE_EMPLOYEE_NAME"
            ''' <summary>工事完了日時</summary>
            Public Const CONST_CONFIRM_TS As String = "_CONST_CONFIRM_TS"
            ''' <summary>工事完了者氏名</summary>
            Public Const CONST_CONFIRM_EMPLOYEE_NAME As String = "_CONST_CONFIRM_EMPLOYEE_NAME"
            ''' <summary>工事完了者備考</summary>
            Public Const CONST_CONFIRM_REMARK As String = "_CONST_CONFIRM_REMARK"
        End Class

        ''' <summary>工事履歴一覧(下部)</summary>
        Public Class ConstructionHistBottom
            ''' <summary>選択</summary>
            Public Const CHECK_SELECT As String = "_CHECK_SELECT"
            ''' <summary>工事番号</summary>
            Public Const CONST_NO As String = "_CONST_NO"
            ''' <summary>工事開始日</summary>
            Public Const CONST_START_DT As String = "_CONST_START_DT"
            ''' <summary>工事終了日</summary>
            Public Const CONST_END_DT As String = "_CONST_END_DT"
            ''' <summary>工事貸出開始日</summary>
            Public Const CONST_LENT_START_DT As String = "_CONST_LENT_START_DT"
            ''' <summary>工事貸出終了日</summary>
            Public Const CONST_LENT_END_DT As String = "_CONST_LENT_END_DT"
            ''' <summary>工事状態</summary>
            Public Const CONST_STATUS As String = "_CONST_STATUS"
            ''' <summary>工事内容</summary>
            Public Const CONST_CONTENTS As String = "_CONST_CONTENTS"
            ''' <summary>工事責任者氏名</summary>
            Public Const CONST_EMPLOYEE_NAME As String = "_CONST_EMPLOYEE_NAME"
            ''' <summary>拠点ロケ</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>工事情報1</summary>
            Public Const CONST_INFO1 As String = "_CONST_INFO1"
            ''' <summary>工事情報2</summary>
            Public Const CONST_INFO2 As String = "_CONST_INFO2"
            ''' <summary>工事情報3</summary>
            Public Const CONST_INFO3 As String = "_CONST_INFO3"
            ''' <summary>工事情報4</summary>
            Public Const CONST_INFO4 As String = "_CONST_INFO4"
            ''' <summary>工事情報5</summary>
            Public Const CONST_INFO5 As String = "_CONST_INFO5"
            ''' <summary>工事情報6</summary>
            Public Const CONST_INFO6 As String = "_CONST_INFO6"
            ''' <summary>工事情報7</summary>
            Public Const CONST_INFO7 As String = "_CONST_INFO7"
            ''' <summary>工事情報8</summary>
            Public Const CONST_INFO8 As String = "_CONST_INFO8"
            ''' <summary>工事情報9</summary>
            Public Const CONST_INFO9 As String = "_CONST_INFO9"
            ''' <summary>工事情報10</summary>
            Public Const CONST_INFO10 As String = "_CONST_INFO10"
            ''' <summary>工事情報11</summary>
            Public Const CONST_INFO11 As String = "_CONST_INFO11"
            ''' <summary>工事情報12</summary>
            Public Const CONST_INFO12 As String = "_CONST_INFO12"
            ''' <summary>工事情報13</summary>
            Public Const CONST_INFO13 As String = "_CONST_INFO13"
            ''' <summary>工事情報14</summary>
            Public Const CONST_INFO14 As String = "_CONST_INFO14"
            ''' <summary>工事情報15</summary>
            Public Const CONST_INFO15 As String = "_CONST_INFO15"
            ''' <summary>工事情報16</summary>
            Public Const CONST_INFO16 As String = "_CONST_INFO16"
            ''' <summary>工事情報17</summary>
            Public Const CONST_INFO17 As String = "_CONST_INFO17"
            ''' <summary>工事情報18</summary>
            Public Const CONST_INFO18 As String = "_CONST_INFO18"
            ''' <summary>工事情報19</summary>
            Public Const CONST_INFO19 As String = "_CONST_INFO19"
            ''' <summary>工事情報20</summary>
            Public Const CONST_INFO20 As String = "_CONST_INFO20"
            ''' <summary>前回仮置数</summary>
            Public Const PREV_TEMP_NUM As String = "_PREV_TEMP_NUM"
            ''' <summary>持込数</summary>
            Public Const CONST_IN_NUM As String = "_CONST_IN_NUM"
            ''' <summary>今回仮置き数</summary>
            Public Const NOW_TEMP_NUM As String = "_NOW_TEMP_NUM"
            ''' <summary>持帰数</summary>
            Public Const CONST_OUT_NUM As String = "_CONST_OUT_NUM"
            ''' <summary>現在貸出数</summary>
            Public Const CUR_LENT_NUM As String = "_CUR_LENT_NUM"
            ''' <summary>現在仮置き数</summary>
            Public Const CUR_TEMP_NUM As String = "_CUR_TEMP_NUM"
            ''' <summary>現場確認者氏名</summary>
            Public Const SITE_EMPLOYEE_NAME As String = "_SITE_EMPLOYEE_NAME"
            ''' <summary>工事完了日時</summary>
            Public Const CONST_CONFIRM_TS As String = "_CONST_CONFIRM_TS"
            ''' <summary>工事完了者氏名</summary>
            Public Const CONST_CONFIRM_EMPLOYEE_NAME As String = "_CONST_CONFIRM_EMPLOYEE_NAME"
            ''' <summary>工事完了者備考</summary>
            Public Const CONST_CONFIRM_REMARK As String = "_CONST_CONFIRM_REMARK"
            ''' <summary>作業日時</summary>
            Public Const CONST_WORK_TS As String = "_CONST_WORK_TS"
            ''' <summary>作業内容</summary>
            Public Const CONST_WORK_DIV As String = "_CONST_WORK_DIV"
            ''' <summary>照合結果</summary>
            Public Const CONST_MATCH_DIV As String = "_CONST_MATCH_DIV"
            ''' <summary>貸出数</summary>
            Public Const LENT_NUM As String = "_LENT_NUM"
            ''' <summary>返却数</summary>
            Public Const LEASE_NUM As String = "_LEASE_NUM"
            ''' <summary>仮置き登録数</summary>
            Public Const TEMP_REG_NUM As String = "_TEMP_REG_NUM"
            ''' <summary>持込照合数</summary>
            Public Const CONST_IN_MATCH_NUM As String = "_CONST_IN_MATCH_NUM"
            ''' <summary>持帰照合数</summary>
            Public Const CONST_OUT_MATCH_NUM As String = "_CONST_OUT_MATCH_NUM"
            ''' <summary>持込数</summary>
            Public Const CONST_IN_NUM_HIST As String = "_CONST_IN_NUM_HIST"
            ''' <summary>仮置き数</summary>
            Public Const TEMP_NUM As String = "_TEMP_NUM"
            ''' <summary>持帰数</summary>
            Public Const CONST_OUT_NUM_HIST As String = "_CONST_OUT_NUM_HIST"
        End Class

        ''' <summary>工事資産履歴一覧(上部)</summary>
        Public Class ConstAssetHistTop
            ''' <summary>工事番号</summary>
            Public Const CONST_NO As String = "_CONST_NO"
            ''' <summary>工事開始日</summary>
            Public Const CONST_START_DT As String = "_CONST_START_DT"
            ''' <summary>工事終了日</summary>
            Public Const CONST_END_DT As String = "_CONST_END_DT"
            ''' <summary>工事貸出開始日</summary>
            Public Const CONST_LENT_START_DT As String = "_CONST_LENT_START_DT"
            ''' <summary>工事貸出終了日</summary>
            Public Const CONST_LENT_END_DT As String = "_CONST_LENT_END_DT"
            ''' <summary>工事状態</summary>
            Public Const CONST_STATUS As String = "_CONST_STATUS"
            ''' <summary>工事内容</summary>
            Public Const CONST_CONTENTS As String = "_CONST_CONTENTS"
            ''' <summary>工事責任者氏名</summary>
            Public Const CONST_EMPLOYEE_NAME As String = "_CONST_EMPLOYEE_NAME"
            ''' <summary>拠点ロケ</summary>
            Public Const MNG_LOC_L_CD As String = "_MNG_LOC_L_CD"
            ''' <summary>工事情報1</summary>
            Public Const CONST_INFO1 As String = "_CONST_INFO1"
            ''' <summary>工事情報2</summary>
            Public Const CONST_INFO2 As String = "_CONST_INFO2"
            ''' <summary>工事情報3</summary>
            Public Const CONST_INFO3 As String = "_CONST_INFO3"
            ''' <summary>工事情報4</summary>
            Public Const CONST_INFO4 As String = "_CONST_INFO4"
            ''' <summary>工事情報5</summary>
            Public Const CONST_INFO5 As String = "_CONST_INFO5"
            ''' <summary>工事情報6</summary>
            Public Const CONST_INFO6 As String = "_CONST_INFO6"
            ''' <summary>工事情報7</summary>
            Public Const CONST_INFO7 As String = "_CONST_INFO7"
            ''' <summary>工事情報8</summary>
            Public Const CONST_INFO8 As String = "_CONST_INFO8"
            ''' <summary>工事情報9</summary>
            Public Const CONST_INFO9 As String = "_CONST_INFO9"
            ''' <summary>工事情報10</summary>
            Public Const CONST_INFO10 As String = "_CONST_INFO10"
            ''' <summary>工事情報11</summary>
            Public Const CONST_INFO11 As String = "_CONST_INFO11"
            ''' <summary>工事情報12</summary>
            Public Const CONST_INFO12 As String = "_CONST_INFO12"
            ''' <summary>工事情報13</summary>
            Public Const CONST_INFO13 As String = "_CONST_INFO13"
            ''' <summary>工事情報14</summary>
            Public Const CONST_INFO14 As String = "_CONST_INFO14"
            ''' <summary>工事情報15</summary>
            Public Const CONST_INFO15 As String = "_CONST_INFO15"
            ''' <summary>工事情報16</summary>
            Public Const CONST_INFO16 As String = "_CONST_INFO16"
            ''' <summary>工事情報17</summary>
            Public Const CONST_INFO17 As String = "_CONST_INFO17"
            ''' <summary>工事情報18</summary>
            Public Const CONST_INFO18 As String = "_CONST_INFO18"
            ''' <summary>工事情報19</summary>
            Public Const CONST_INFO19 As String = "_CONST_INFO19"
            ''' <summary>工事情報20</summary>
            Public Const CONST_INFO20 As String = "_CONST_INFO20"
            ''' <summary>前回仮置数</summary>
            Public Const PREV_TEMP_NUM As String = "_PREV_TEMP_NUM"
            ''' <summary>持込数</summary>
            Public Const CONST_IN_NUM As String = "_CONST_IN_NUM"
            ''' <summary>今回仮置き数</summary>
            Public Const NOW_TEMP_NUM As String = "_NOW_TEMP_NUM"
            ''' <summary>持帰数</summary>
            Public Const CONST_OUT_NUM As String = "_CONST_OUT_NUM"
            ''' <summary>現在貸出数</summary>
            Public Const CUR_LENT_NUM As String = "_CUR_LENT_NUM"
            ''' <summary>現在仮置き数</summary>
            Public Const CUR_TEMP_NUM As String = "_CUR_TEMP_NUM"
            ''' <summary>現場確認者氏名</summary>
            Public Const SITE_EMPLOYEE_NAME As String = "_SITE_EMPLOYEE_NAME"
            ''' <summary>工事完了日時</summary>
            Public Const CONST_CONFIRM_TS As String = "_CONST_CONFIRM_TS"
            ''' <summary>工事完了者氏名</summary>
            Public Const CONST_CONFIRM_EMPLOYEE_NAME As String = "_CONST_CONFIRM_EMPLOYEE_NAME"
            ''' <summary>工事完了者備考</summary>
            Public Const CONST_CONFIRM_REMARK As String = "_CONST_CONFIRM_REMARK"
        End Class

        ''' <summary>工事資産履歴一覧(中部)</summary>
        Public Class ConstAssetHistMiddle
            ''' <summary>作業日時</summary>
            Public Const CONST_WORK_TS As String = "_CONST_WORK_TS"
            ''' <summary>作業内容</summary>
            Public Const CONST_WORK_DIV As String = "_CONST_WORK_DIV"
            ''' <summary>照合結果</summary>
            Public Const CONST_MATCH_DIV As String = "_CONST_MATCH_DIV"
            ''' <summary>貸出数</summary>
            Public Const LENT_NUM As String = "_LENT_NUM"
            ''' <summary>返却数</summary>
            Public Const LEASE_NUM As String = "_LEASE_NUM"
            ''' <summary>仮置き登録数</summary>
            Public Const TEMP_REG_NUM As String = "_TEMP_REG_NUM"
            ''' <summary>持込照合数</summary>
            Public Const CONST_IN_MATCH_NUM As String = "_CONST_IN_MATCH_NUM"
            ''' <summary>持帰照合数</summary>
            Public Const CONST_OUT_MATCH_NUM As String = "_CONST_OUT_MATCH_NUM"
            ''' <summary>持込数</summary>
            Public Const CONST_IN_NUM_HIST As String = "_CONST_IN_NUM_HIST"
            ''' <summary>仮置き数</summary>
            Public Const TEMP_NUM As String = "_TEMP_NUM"
            ''' <summary>持帰数</summary>
            Public Const CONST_OUT_NUM_HIST As String = "_CONST_OUT_NUM_HIST"
        End Class

        ''' <summary>工事資産履歴一覧(下部)</summary>
        Public Class ConstAssetHistBottom
            ''' <summary>資産番号</summary>
            Public Const ASSET_NO As String = "_ASSET_NO"
            ''' <summary>資産名称</summary>
            Public Const ASSET_NAME As String = "_ASSET_NAME"
            ''' <summary>作業内容</summary>
            Public Const CONST_WORK_DIV As String = "_CONST_WORK_DIV"
            ''' <summary>照合結果</summary>
            Public Const CONST_MATCH_DIV As String = "_CONST_MATCH_DIV"
            ''' <summary>状態</summary>
            Public Const CONST_MATCH_STATUS As String = "_CONST_MATCH_STATUS"
            ''' <summary>仮置き</summary>
            Public Const TEMP_FLAG As String = "_TEMP_FLAG"
            ''' <summary>仮置情報1</summary>
            Public Const TEMP_INFO1 As String = "_TEMP_INFO1"
            ''' <summary>仮置情報2</summary>
            Public Const TEMP_INFO2 As String = "_TEMP_INFO2"
            ''' <summary>仮置情報3</summary>
            Public Const TEMP_INFO3 As String = "_TEMP_INFO3"
            ''' <summary>仮置情報4</summary>
            Public Const TEMP_INFO4 As String = "_TEMP_INFO4"
            ''' <summary>仮置情報5</summary>
            Public Const TEMP_INFO5 As String = "_TEMP_INFO5"
            ''' <summary>仮置情報6</summary>
            Public Const TEMP_INFO6 As String = "_TEMP_INFO6"
            ''' <summary>仮置情報7</summary>
            Public Const TEMP_INFO7 As String = "_TEMP_INFO7"
            ''' <summary>仮置き備考</summary>
            Public Const TEMP_REMARK As String = "_TEMP_REMARK"
        End Class
    End Class
#End Region

End Namespace
