package com.rfid.common;

public class CommonConst {

    public static final String FIXED_VALUE_WEBAPI_DIRECTORY = "asetraapi";   //WebAPI接続時のディレクトリ名
    //public static final String FIXED_VALUE_WEBAPI_DIRECTORY = "asetraapitest";            //WebAPI接続時のディレクトリ名

    public static final String VERSION_NAME = "Version ";       //Version名称

    public static final String FIXED_VALUE_AES_KEY = "asetra_system_key"; //AES暗号化キー

    /**
     * 資産マスタ：ステータス
     */

    public enum ASSET_STATUS{
        prepare("0"),      //準備中
        Storage("1"),      //保管
        Lease("2"),        //貸出
        Dispose("9");      //除却

        private final String text;
        private ASSET_STATUS(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 資産マスタ：実棚ステータス
     */
    public enum PHYINV_STATUS{
        NotConvert("O"),      //対象外
        Error("E"),            //現物なし
        Normal("N"),           //正常
        Warning("W");          //警告

        private final String text;
        private PHYINV_STATUS(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 資産マスタ：ステータス
     */
    public enum ASSET_LEASE_STATUS{
        Impossible("0"),    //不可
        Possible ("1");     //可能

        private final String text;
        private ASSET_LEASE_STATUS(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

//20191209 Add Start Sasaki
    /**
     * 資産マスタ：実棚ステータス済フラグ
     */
    public enum PHYINV_FLAG{
        Yes("Y"),      //実棚済み
        No("N");        //未実施

        private final String text;
        private PHYINV_FLAG(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }


//20191209 Add End Sasaki

    //20241107 Add Start shimomura
    /**
     * 仮置きテーブル：仮置き情報有無
     */
    public enum TEMPINFO_FLAG{
        Yes("Y"),      //情報あり
        No("N");        //情報なし

        private final String text;
        private TEMPINFO_FLAG(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }
    //20241107 Add End shimomura

    /**
     * ロケーションマスタ：区分
     */
    public enum LOCATION_DIV{
        L("L"),         //L
        M("M"),         //M
        S("S");         //S

        private final String text;
        private LOCATION_DIV(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 設定マスタ：ロケ区分
     */
    public enum LOCATION_USAGE{
        NoneUse("0"),      //使用しない
        USE("1");           //使用する

        private final String text;
        private LOCATION_USAGE(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 文言マスタ：利用ツール区分
     */
    public enum WORD_TOOL_DIV{
        H("H");         //H

        private final String text;
        private WORD_TOOL_DIV(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 処理履歴：処理区分
     */
    public enum PROCESS_DIV{
        Storage("BS"),     //保管
        Lease("BO"),       //貸出
        Return("BI"),      //返却
        Dispose("BR"),     //除却
        TagLink("T2"),     //タグ紐付
        TagLinkWrite("T1"),     //タグ紐付


        MatchIn("MO"),          //持出照合
        MatchOut("MI"),         //持帰照合

        MatchReturn("BM"),      //照合返却

        MatchTemp("TM");      //仮置き

        private final String text;
        private PROCESS_DIV(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 操作ログ：処理種別
     */
    public enum PROCESS_CAT{
        Event("EV"),        //イベント
        Operation("OP"),   //操作
        Error("ER");        //エラー

        private final String text;
        private PROCESS_CAT(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 操作ログ：ログ種別
     */
    public enum LOG_KIND{
        Inventory("T"),        //棚卸
        Process("H");          //処理履歴

        private final String text;
        private LOG_KIND(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 設定：棚卸方法
     */
    public enum INVENTORY_METHOD{
        NoneLocation("1"),  //ロケーションなし
        Location("2");      //ロケーションあり

        private final String text;
        private INVENTORY_METHOD(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 設定：資産読取媒体
     */
    public enum READING_MEDIUM{
        TAG("TG"),     //RFIDタグ、QR、Barcode
        QR("QR");       //QR、Barcode

        private final String text;
        private READING_MEDIUM(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 設定：資産ID生成方法
     */
    public enum ASSET_GENERATION{
        NoWrite("1"),      //書込みしない
        AssetNo("2"),      //資産番号を書込み
        Automatic("3"),    //採番して書込み
        AssetID("4");      //資産番IDを書込み

        private final String text;
        private ASSET_GENERATION(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * 設定：棚卸開始有無
     */
    public enum INVENTORY_START{
        Start("1");        //開始
        private final String text;
        private INVENTORY_START(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

//20191209 Add Start Sasaki
    /**
     * 設定：棚卸継続有無
     */
    public enum INVENTORY_CONTINUE{
        NO("N"),        //継続なし
        YES("Y");       //継続あり
        private final String text;
        private INVENTORY_CONTINUE(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }
//20191209 Add End Sasaki

    //20210918 ADD Takase V2.0 START
    /**
     * 設定：権限タイプ
     */
    public enum AUTHORITY_TYPE {
        NORMAL("N"),    //通常権限
        HUB("H");       //拠点権限
        private final String text;
        private AUTHORITY_TYPE(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 権限マスタ：権限有無
     */
    public enum AUTH_FLG {
        YES("Y"),    //あり
        NO("N");     //なし
        private final String text;
        private AUTH_FLG(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 資産マスタ：共通拠点フラグ
     */
    public enum LOC_SHARE_FLG {
        SHARED("Y"),    //共通
        NORMAL("N");    //一般拠点
        private final String text;
        private LOC_SHARE_FLG(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：予約機能有無
     */
    public enum RESERVATION_FUNCTION {
        YES("Y"),    //あり
        NO("N");     //なし
        private final String text;
        private RESERVATION_FUNCTION(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：通信方法
     */
    public enum TRANSFER_METHOD {
        WebAPI("WebAPI"),
        USB("USB");
        private final String text;
        private TRANSFER_METHOD(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }
    //20210918 ADD Takase V2.0 END

    /**
     * 一覧表示メイン、サブキー
     */
    public enum List_VIEW_KEY{
        Main("Main"),      //メイン
        Sub("Sub"),        //サブ

        //仮置き一覧で使用
        Sub2("Sub2"),        //サブ2
        Sub3("Sub3"),        //サブ3
        Sub4("Sub4"),        //サブ4
        Sub5("Sub5"),        //サブ5
        Sub6("Sub6"),        //サブ6
        Sub7("Sub7"),        //サブ7

        Inv("Inv"),        //電波
        Hidden("hidden"),  //隠し項目
        Aux("Aux"),        //補助情報 　 //20230907 ADD okubo v2.4.1 Start
        Mat("Mat");        //照合結果 持込照合/持帰照合で使用

        private final String text;
        private List_VIEW_KEY(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    /**
     * メニュー画面選択ボタン
     */
    public enum MENU_CLICK {
        Inventory("Inventory"),
        Search("Search"),
        Lease("Lease"),
        Return("Return"),
        Storage("Storage"),
        Dispose("Dispose"),
        Linktag("Linktag");
        private final String text;
        private MENU_CLICK(final String text) {
            this.text = text;
        }
        public final String getString() {
            return this.text;
        }
    }

    /**
     * メニュー画面モード
     */
    public enum MENU_MODE {
        Normal("Normal"),                   //通常
        MatControl("MatControl"),           //照合(管理)
        MatGeneral("MatGeneral"),           //照合(一般、仮置きなし)
        MatGeneralTmpo("MatGeneralTmpo");   //照合(一般、仮置きあり)
        private final String text;

        private MENU_MODE(final String text) {
            this.text = text;
        }
        public final String getString() {
            return this.text;
        }
    }

    /**
     * 管理⇔一般切替
     */
    public enum COLLATION_OPERATION {
        Control("管理"),
        General("一般");
        private final String text;
        private COLLATION_OPERATION(final String text) {
            this.text = text;
        }
        public final String getString() {
            return this.text;
        }
    }

    /**
     * Reader⇔Android切替
     */
    public enum READER_SETTING {
        Reader("RFIDReader"),
        Android("Android");
        private final String text;
        private READER_SETTING(final String text) {
            this.text = text;
        }
        public final String getString() {
            return this.text;
        }
    }

    /**
     * ZEBRA/DENSO/IDATA/KEYENCE_DX/CIPHER_LAB_RS35
     */
    public enum CONNECT_RFIDNAME {
        DENSO("DENSO"),
        ZEBRA("ZEBRA"),
        IDATA("IDATA"),
        KEYENCE_DX("KEYENCE_DX"),
        CIPHER_LAB_RS35("CIPHER_LAB_RS35");
        private final String text;
        private CONNECT_RFIDNAME(final String text) {
            this.text = text;
        }
        public final String getString() {
            return this.text;
        }
    }

    public enum INVENTORY_KBN {
        L(0),
        M(1),
        S(2);
        private final int text;
        private INVENTORY_KBN(final int text) {
            this.text = text;
        }
        public int getInt() {
            return this.text;
        }
    }

    public enum LEASE_UPPER_ERR_DIV {
        ERROR("Y"),     //貸出チェックする
        ALERT("N"),     //警告する
        NONE("");       //何もしない
        private final String text;
        private LEASE_UPPER_ERR_DIV(final String text) {
            this.text = text;
        }
        public String getString() {
            return this.text;
        }
    }

    public enum RETURN_STARTUP_VALUE {
        YES("Y"),    //初期起動の場合
        NO("N");     //初期起動ではない場合
        private final String text;
        private RETURN_STARTUP_VALUE(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：HT貸出時のアラート表示有無　20230830 ADD Kawamoto v2.4.1
     */
    public enum LEASE_ALERT_DISPLAY {
        YES("Y"),    //表示する
        NO("N");     //表示しない
        private final String text;
        private LEASE_ALERT_DISPLAY(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：HT貸出時の期限日判定項目　20230830 ADD Kawamoto v2.4.1
     */
    public enum LEASE_DEADLONE {
        SYSTEM_DATE("SYSTEM_DATE"),     //貸出実施日
        RETURN_DATE("RETURN_DATE");     //返却予定日
        private final String text;
        private LEASE_DEADLONE(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：HT返却時のアラート表示有無　20230830 ADD Kawamoto v2.4.1
     */
    public enum RETURN_ALERT_DISPLAY {
        YES("Y"),    //表示する
        NO("N");     //表示しない
        private final String text;
        private RETURN_ALERT_DISPLAY(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：連続貸出(HTアプリ)実施有無　20230830 ADD Kawamoto v2.4.1
     */
    public enum LENDING_CONTINUOUS {
        YES("Y"),    //実施する
        NO("N");     //実施しない
        private final String text;
        private LENDING_CONTINUOUS(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 照合結果
     */
    public enum CONT_MATCH_DIV {
        MAT_OK("1"),            //照合OK
        MAT_LIST_OTHER("8"),    //リスト外NG
        MAT_NO("9"),            //照合NG
        NONE("");               //何もしない
        private final String text;
        private CONT_MATCH_DIV(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 一覧画面で表示するための警告メッセージ
     */
    public enum WARNING_DISPLAY {
        LEASE_UPPER_LIMIT_WARN(1),
        LEASE_UPPER_LIMIT_ERR(2),
        LEASE_LIMIT(3),
        GENERIC_LIMIT(4),
        RETURN_LIMIT(5);

        private final int sortRank;
        WARNING_DISPLAY(final int sortRank) {this.sortRank = sortRank;}
        public int getSortRank() {return this.sortRank;}
    }

    /**
     * 設定：返却予定日を自動でセット
     */
    public enum LEASE_SET_SYSDATE_HT {
        YES("Y"),    //する
        NO("N");     //しない
        private final String text;
        private LEASE_SET_SYSDATE_HT(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：貸出情報表示
     */
    public enum LEASE_DSP_INFO {
        INVISIBLE("0"),
        LOGIN_INFO("1"),
        LENDING_INFO("2");
        private final String text;
        private LEASE_DSP_INFO(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：返却情報表示
     */
    public enum RETURN_DSP_INFO {
        INVISIBLE("0"),
        LOGIN_INFO("1");
        private final String text;
        private RETURN_DSP_INFO(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 照合結果区分
     */
    public enum CONST_MATCH_DIV {
        OK("1"),
        NG("9"),
        OTHER("8");
        private final String text;
        private CONST_MATCH_DIV(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：照合機能
     */
    public enum MATCHING_FUNCTION {
        YES("Y"),    //する
        NO("N");     //しない
        private final String text;
        private MATCHING_FUNCTION(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：照合機能
     */
    public enum MATCHING_RESERVE_FUNCTION {
        YES("Y"),    //する
        NO("N");     //しない
        private final String text;
        private MATCHING_RESERVE_FUNCTION(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：仮置機能の設定
     */
    public enum TEMPORARYLEAVE_FUNCTION {
        YES("Y"),    //する
        NO("N");     //しない
        private final String text;
        private TEMPORARYLEAVE_FUNCTION(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }

    /**
     * 設定：仮置のコンポーネント設定　Data2
     */
    public enum TEMPORARY_INPUT_ITEM_DATA {
        YES("Y"),    //する
        NO("N");     //しない
        private final String text;
        private TEMPORARY_INPUT_ITEM_DATA(final String text) { this.text = text; }
        public String getString() { return this.text; }
    }



//20210918 ADD Takase V2.0 START
/************************************************************************************
 ** 固定値(システムパスワード)
 ************************************************************************************/
    public static final String SYSTEM_PASSWORD = "03100310";
//20210918 ADD Takase V2.0 END

/************************************************************************************
 ** 固定値
 ************************************************************************************/
    /**
     * 設定マスタキー名称
     */
    public static final String SETTEI_KEY_INVENTORY_METHOD = "Inventory_method";
    public static final String SETTEI_KEY_READING_MEDIUM = "Reading_medium";
    public static final String SETTEI_KEY_LOCATION_USAGE = "Location_usage_classification";
    public static final String SETTEI_KEY_LOCATION_USAGE_INV = "Actual_shelf_location";
    public static final String SETTEI_KEY_ASSET_ID_GENERATION = "Asset_ID_generation_method";
    public static final String SETTEI_KEY_AUTOMATIC_GENERATION = "Asset_ID_automatic_generation";
    public static final String SETTEI_KEY_INVENTORY_START = "Inventory_start";
    public static final String SETTEI_KEY_INVENTORY_CONTINUE = "Inventory_Continuous";		                //20191210 Add Sasaki
    public static final String SETTEI_KEY_AUTHORITY_TYPE = "Authority_Type";                                //20210918 ADD Takase V2.0
    public static final String SETTEI_KEY_RESERVATION_FUNCTION = "Reservation_Function";                    //20210918 ADD Takase V2.0
    public static final String SETTEI_KEY_ASSET_REGISTRATION_LIMIT = "Asset_Registration_Limit";            //20230221 ADD Sato v2.3
    public static final String SETTEI_KEY_LEASE_ALERT_DISPLAY = "Lease_Alert_Display";                      //20230830 ADD Kawamoto v2.4.1
    public static final String SETTEI_KEY_RETURN_ALERT_DISPLAY = "Return_Alert_Display";                    //20230830 ADD Kawamoto v2.4.1
    public static final String SETTEI_KEY_LENDING_CONTINUOUS = "Lending_Continuous";                        //20230830 ADD Kawamoto v2.4.1
    public static final String SETTEI_KEY_LEASE_RETURN_ASSET_COLOR_HT = "Lease_Return_Asset_Color_HT";      //20231024 ADD Sato v2.4.1
    public static final String SETTEI_KEY_LEASE_SET_SYSDATE_HT = "Lease_Set_SysDate_HT";                    //20231024 ADD Sato v2.4.1
    public static final String SETTEI_KEY_LEASE_DISPLAY_INFO_HT = "Lease_Diplay_Info_HT";                   //20231024 ADD Sato v2.4.1
    public static final String SETTEI_KEY_RETURN_DISPLAY_INFO_HT = "Return_Diplay_Info_HT";                  //20231024 ADD Sato v2.4.1
    public static final String SETTEI_KEY_SUMMARY_BACKGROUND_COLOR_HT = "Summary_Background_Color_HT";
    public static final String SETTEI_KEY_MATCHING_FUNCTION = "Matching_Function";                          //照合機能の設定 v4.0
    public static final String SETTEI_KEY_MATCHING_RESERVE_FUNCTION = "Matching_Reserve_Function";          //照合予約機能の設定 v4.0
    public static final String SETTEI_KEY_TEMPORARYLEAVE_FUNCTION = "TemporaryLeave_Function";              //仮置き機能の設定 v4.0
    public static final String SETTEI_KEY_TEMPORARY_INPUT_ITEM = "Temporary_Input_Item";            //仮置き登録時項目設定 v4.0
    public static final String SETTEI_KEY_CONSTRUCTION_INPUT_ITEM = "Construction_Input_Item";            //工事情報項目設定 v4.0

    /**
     * 設定マスタ　Key1名称         ※20231024 ADD Sato v2.4.1
     */
    public static final String SETTEI_KEY1_ASSET_NO_REGIST = "ASSET_NO_REGIST";
    public static final String SETTEI_KEY1_ASSET_DATA_ILLEGAL = "ASSET_DATA_ILLEGAL";
    public static final String SETTEI_KEY1_ASSET_STATUS_ILLEGAL = "ASSET_STATUS_ILLEGAL";
    public static final String SETTEI_KEY1_AUTHROTY = "AUTHROTY";
    public static final String SETTEI_KEY1_LEASE_STATUS_ILLEGAL = "LEASE_STATUS_ILLEGAL";
    public static final String SETTEI_KEY1_LEASE_ALERT = "LEASE_ALERT";
    public static final String SETTEI_KEY1_LEASE_ERR = "LEASE_ERR";
    public static final String SETTEI_KEY1_LEASE_DATE_LIMIT = "LEASE_DATE_LIMIT";
    public static final String SETTEI_KEY1_GENERAL_DATE_LIMIT = "GENERAL_DATE_LIMIT";
    public static final String SETTEI_KEY1_RETURN_DATE_LIMIT = "RETURN_DATE_LIMIT";
    public static final String SETTEI_KEY1_NORMAL = "NORMAL";
    public static final String SETTEI_KEY1_WARN = "WARN";
    public static final String SETTEI_KEY1_ERROR = "ERROR";

    /**
     * ファイル書込みキー
     */
    public static final String SYSTEM_KEY_MASTER_RECEIVE_DATE = "SYSTEM_KEY_MASTER_RECEIVE_DATE";
    public static final String SYSTEM_KEY_INITIALIZE = "SYSTEM_KEY_INITIALIZE";
    public static final String SYSTEM_KEY_STARTUP = "SYSTEM_KEY_STARTUP";

    /**
     * 画面間で引き渡すロケコードキー
     */
    public static final String VIEW_KEY_MENU_SELECT = "VIEW_KEY_MENU_SELECT";               //メニュー選択遷移先
    public static final String VIEW_KEY_TIMEOUT = "VIEW_KEY_MENU_SELECT";                   //時間切れ画面
    public static final String VIEW_KEY_LOCATION_CD = "VIEW_KEY_LOCATION_CD";               //ロケーション選択
    public static final String VIEW_KEY_LOCATION = "VIEW_KEY_LOCATION";                      //ロケーション選択
    public static final String VIEW_KEY_LOCATION_L_DATA = "VIEW_KEY_LOCATION_L_DATA";
    public static final String VIEW_KEY_LOCATION_M_DATA = "VIEW_KEY_LOCATION_M_DATA";
    public static final String VIEW_KEY_LOCATION_S_DATA = "VIEW_KEY_LOCATION_S_DATA";
    public static final String VIEW_KEY_LOCATION_L_TEXT= "VIEW_KEY_LOCATION_L_TEXT";
    public static final String VIEW_KEY_LOCATION_M_TEXT = "VIEW_KEY_LOCATION_M_TEXT";
    public static final String VIEW_KEY_LOCATION_S_TEXT = "VIEW_KEY_LOCATION_S_TEXT";
    public static final String VIEW_KEY_LOCATION_SELECT = "VIEW_KEY_LOCATION_SELECT";

    public static final String VIEW_KEY_LEASE_CD = "VIEW_KEY_LEASE_CD";                    //貸出先選択
    public static final String VIEW_KEY_LEASE_NAME = "VIEW_KEY_LEASE_NAME";               //貸出先選択
    public static final String VIEW_KEY_LEASE_REMARK = "VIEW_KEY_LEASE_REMARK";          //備考
    public static final String VIEW_KEY_EMPLOYEE_CD = "VIEW_KEY_EMPLOYEE_CD";             //社員選択
    public static final String VIEW_KEY_EMPLOYEE_NAME = "VIEW_KEY_EMPLOYEE_NAME";        //社員選択
    public static final String VIEW_KEY_EMPLOYEE_PASS = "VIEW_KEY_EMPLOYEE_PASS";        //社員パスワード
    public static final String VIEW_KEY_DATE = "VIEW_KEY_DATE";                             //日付
    public static final String VIEW_KEY_SEARCH_ASSET = "VIEW_KEY_SEARCH_ASSET";          //探索

    public static final String VIEW_CONFIRM_KEY = "VIEW_CONFIRM_KEY";

    public static final String VIEW_KEY_HT_HUB = "VIEW_HT_HUB_KEY";    //HT.対象拠点      //20210918 ADD Takase V2.0

    public static final String VIEW_KEY_HT_CONSTRUCTION_NO = "VIEW_KEY_HT_CONSTRUCTION_NO";    //工事番号         //20241004 ADD Shirakawa v.4.0
    public static final String VIEW_KEY_HT_CONST_LENT_END_DT = "VIEW_KEY_HT_CONST_LENT_END_DT";    //工事貸出終了日         //20241211 ADD Shirakawa v.4.0
    public static final String VIEW_KEY_HT_TEMP_ID = "VIEW_KEY_HT_TEMP_ID";    //仮置きID         //20241004 ADD Shirakawa v.4.0

    /**
     * 画面間で引き渡す棚卸データキー
     */
    public static final String VIEW_KEY_INVENTORY_DATA = "VIEW_KEY_INVENTORY_DATA";
    public static final String VIEW_KEY_LOCATION_TITLE = "VIEW_KEY_LOCATION_TITLE";
    public static final String VIEW_KEY_PROCESS_DATA = "VIEW_KEY_PROCESS_DATA";
    public static final String VIEW_KEY_SEARCH_DATA = "VIEW_KEY_SEARCH_DATA";
    public static final String VIEW_KEY_TAGLINK_DATA = "VIEW_KEY_TAGLINK_DATA";
    public static final String VIEW_KEY_LOGIN_TITLE = "VIEW_KEY_LOGIN_TITLE";
    public static final String VIEW_KEY_LOGIN_EMPLOYEE_CD = "VIEW_KEY_LOGIN_EMPLOYEE_CD";

    /**
     * ダイアログ
     */
    public static final String DIALOG_ERROR_TITLE = "ERROR";
    public static final String DIALOG_INFO_TITLE = "INFORMATION";
    public static final String DIALOG_PASSWORD_TITLE = "PASSWORD";                       //20210918 ADD Takase V2.0
    public static final String DIALOG_RESTART_TITLE = "SHUT DOWN";

    /**
     * ダイアログキー
     *    VIEW_CONFIRM_KEY
     */
    public final static int BUTTON_INV_CONFIRM = 90;

    /**
     * 設定マスタ
     */
    public static final String SETTING_INVENTORY_METHOD_NONELOC = "1";
    public static final String SETTING_INVENTORY_METHOD_LOC = "2";

    /**
     * 作業者継続確認の遷移元キー
     */
    public static final String KEY_READ_INVENTORY_LOC = "keyReadInventoryLoc";      //棚卸読取(ロケ指定あり)

    /**
     * 画面表示：全体
     */
    public static final String VIEW_WORD_ALL_SEND_DATA = "HB990001";                //蓄積データ送信
    public static final String VIEW_WORD_ALL_RECIVE_DATA = "HB990002";              //マスタ取込受信
    public static final String VIEW_WORD_ALL_CLEAR = "HB990003";                    //クリア
    public static final String VIEW_WORD_ALL_REGISTER = "HB990004";                 //登録
    public static final String VIEW_WORD_ALL_DESCRIPTION = "HB990005";              //詳細表示
    public static final String VIEW_WORD_ALL_LIST = "HB990007";                      //一覧
    public static final String VIEW_WORD_ALL_DATA_UMU = "HI990001";                 //データ有
    public static final String VIEW_WORD_ALL_LOC_L_KBN = "HI990009";                //ロケ大区分
    public static final String VIEW_WORD_ALL_LOC_M_KBN = "HI990010";                //ロケ中区分
    public static final String VIEW_WORD_ALL_LOC_S_KBN = "HI990011";                //ロケ小区分
    public static final String VIEW_WORD_ALL_SELECT = "HB990006";                   //選択
    public static final String VIEW_WORD_ALL_NEXT = "HB990008";                     //次へボタン
    public static final String VIEW_WORD_ALL_DENPA = "HI990002";                    //電波強度
    public static final String VIEW_WORD_ALL_COUNT = "HI990004";                    //件数
    public static final String VIEW_WORD_ALL_SEND_TITLE = "HI990012";               //蓄積データ送信
    public static final String VIEW_WORD_ALL_SEND_MSG = "HI990013";                 //実績送信中
    public static final String VIEW_WORD_ALL_RECIVE_TITLE = "HI990014";             //マスタ取込処理
    public static final String VIEW_WORD_ALL_RECIVE_MSG = "HI990015";               //マスタ取込処理中
    public static final String VIEW_WORD_ALL_TAG_READ = "HI990005";                 //タグ読取
    public static final String VIEW_WORD_ALL_BUTTON_INPUT = "HI990024";             //ボタン押下
    public static final String VIEW_WORD_ALL_MASTER_NONE = "HI990028";              //マスター未登録件数
    public static final String VIEW_WORD_ALL_ASSETNAME_TITLE = "HI990029";         //資産管理・名称
    public static final String VIEW_WORD_ALL_DENPA_L = "HI990030";                  //強
    public static final String VIEW_WORD_ALL_DENPA_M = "HI990031";                  //中
    public static final String VIEW_WORD_ALL_DENPA_S = "HI990032";                  //弱
    //20230221 ADD Sato v2.3 START
    public static final String VIEW_WORD_ALL_DATE_SETTING = "HI990035";             //日時設定
    public static final String VIEW_WORD_ALL_SERVER_DATE = "HI990036";              //サーバー日時
    public static final String VIEW_WORD_ALL_HT_DATE = "HI990037";                  //HT日時
    //20230221 ADD Sato v2.3 END

    public static final String VIEW_WORD_ALL_HT_DETAILS_BUTTON = "HB990052";        //詳細     //20241011 ADD Shirakawa v4.0

    public static final String VIEW_WORD_ALL_CARRY_COUNT = "HI990038";              //持出数
    public static final String VIEW_WORD_ALL_READ_COUNT = "HI990039";               //読取済数
    public static final String VIEW_WORD_ALL_UNREAD_COUNT = "HI990040";             //未読取数
    public static final String VIEW_WORD_ALL_MISMATCH_COUNT = "HI990041";           //リスト不一致数

    public static final String VIEW_WORD_ALL_LOGIN_EMPLOYEE_CONTINUE = "HI990042";           //ログイン保持



    /**
     * 画面表示：起動画面
     */
    public static final String VIEW_WORD_KIDO_MODE_BUTTON = "HI120001";             //モードボタン
    public static final String VIEW_WORD_KIDO_BLUETOOTH_BUTTON = "HI120002";        //Bluetoothボタン
    public static final String VIEW_WORD_KIDO_POWER_BUTTON = "HI120003";            //電源ボタン
    public static final String VIEW_WORD_KIDO_SCAN = "HI120004";                    //スキャントリガー
    public static final String VIEW_WORD_KIDO_PAIR_BUTTON = "HI120005";             //ペアリングボタン
    public static final String VIEW_WORD_KIDO_INDICATION = "HI120006";              //表示LED
    public static final String VIEW_WORD_KIDO_CHARGE = "HI120007";                  //充電端子
    public static final String VIEW_WORD_KIDO_PAIR_LED = "HI120008";                 //ペアリングLED
    public static final String VIEW_WORD_KIDO_POWER_LED = "HI120009";                //電源LED
    public static final String VIEW_WORD_KIDO_ZEBRA_MESSAGE1 = "HI120010";           //メッセージ1(ZEBRA)
    public static final String VIEW_WORD_KIDO_ZEBRA_MESSAGE2 = "HI120011";           //メッセージ2(ZEBRA)
    public static final String VIEW_WORD_KIDO_ZEBRA_MESSAGE3 = "HI120012";           //メッセージ3(ZEBRA)
    public static final String VIEW_WORD_KIDO_ZEBRA_MESSAGE4 = "HI120018";           //メッセージ4(ZEBRA)
    public static final String VIEW_WORD_KIDO_DENSO_MESSAGE1 = "HI120013";           //メッセージ1(DENSO)
    public static final String VIEW_WORD_KIDO_DENSO_MESSAGE2 = "HI120014";           //メッセージ2(DENSO)
    public static final String VIEW_WORD_KIDO_DENSO_MESSAGE3 = "HI120015";           //メッセージ3(DENSO)
    public static final String VIEW_WORD_KIDO_DENSO_MESSAGE4 = "HI120016";           //メッセージ4(DENSO)
    public static final String VIEW_WORD_KIDO_DENSO_MESSAGE7 = "HI120017";           //メッセージ7(DENSO)

    /////////////////////////////////////
    ////////   画面表示：蓄積データ送信   ////
    /////////////////////////////////////
    /** 蓄積データ送信 */
    public static final String VIEW_WORD_SEND_DATA_TITLE = "HT800001";
    /** 送信処理に失敗しました。 */
    public static final String VIEW_WORD_ERROR_FAILED_SENDING_DATA = "HM800001";
    /** マスタが最新でない恐れがあります。マスタを再取り込みしてください。 */
    public static final String VIEW_WORD_PLEASE_UPDATE_MASTER_DATABASE  = "HM800002";
    /** 受信処理に失敗しました。 */
    public static final String VIEW_WORD_ERROR_FAILED_RECEIVING_DATA = "HM800003";
    /** 棚卸処理が終了しています。棚卸データを削除しますか？ */
    public static final String VIEW_WORD_DELETE_STOCKTAKING_DATA = "HM800002";
    /** 送信データがありません。 */
    public static final String VIEW_WORD_ERROR_NO_DATA_TO_BE_SENT = "HM800007";
    /** 棚卸データを削除しました。 */
    public static final String VIEW_WORD_ERROR_STOCKTAKING_DATA_DELETED = "HM800008";
    /** マスタにロケ大区分コードが存在しません。 */
    public static final String VIEW_WORD_ERROR_NOT_EXIST_IN_LOCATION = "HM800009";
    /** 蓄積データの送信が完了しました。 */
    public static final String VIEW_WORD_STORED_DATA_TRANSFER_COMPLETED = "HM800010";
    /** マスタ取込中 */
    public static final String VIEW_WORD_MASTER_DATABASE_RECEIVING = "HM800013";
    /** データ送信中 */
    public static final String VIEW_WORD_DATA_SENDING = "HM800014";
    /** HTとUSBを接続してから開始ボタンを押してください。 */
    public static final String VIEW_WORD_USB_CONNECTED = "HM800016";
    /** DB接続エラーです。 */
    public static final String VIEW_WORD_ERROR_DB_NOT_CONNECTED = "HM800098";
    /** WebAPI通信中です。 */
    public static final String VIEW_WORD_COMMUNICATING = "HM990022";
    /** WebAPIの応答がありません。 */
    public static final String VIEW_WORD_ERROR_NO_RESPONSE_FROM_WEBAPI = "HM990060";

    /**
     * 画面表示：メニュー
     */
    public static final String VIEW_WORD_MENU_TITLE = "HT010001";                   //メニュー
    public static final String VIEW_WORD_MENU_INVENTORY = "HB010001";               //棚卸
    public static final String VIEW_WORD_MENU_SEARCH = "HB010002";                  //探索
    public static final String VIEW_WORD_MENU_LENDING = "HB010003";                 //貸出
    public static final String VIEW_WORD_MENU_RETURN = "HB010004";                  //返却
    public static final String VIEW_WORD_MENU_STORE = "HB010005";                   //保管
    public static final String VIEW_WORD_MENU_DELETE = "HB010006";                  //除却
    public static final String VIEW_WORD_MENU_TAG = "HB010007";                     //タグ紐付
    public static final String VIEW_WORD_MENU_MATCHIN = "HB010008";                //持込照合
    public static final String VIEW_WORD_MENU_MATCHOUT = "HB010009";               //持帰照合
    public static final String VIEW_WORD_MENU_MATCHTEMPREGIST = "HB010010";        //仮置登録
    public static final String VIEW_WORD_MENU_MATCHRETURN = "HB010011";            //照合返却
    public static final String VIEW_WORD_MENU_SETTEI= "HT100001";                  //システム設定
    public static final String VIEW_WORD_RFID_BATTERY= "HI010001";                  //電池残
    public static final String VIEW_WORD_MASTER_RECEIVE_DATE= "HI010002";          //マスタ受信日時
    public static final String VIEW_WORD_LOCATION_CHOICE = "HI990276";             //ロケーション選択

    /**
     * 画面表示：ログイン
     */
    public static final String VIEW_WORD_LOGIN_EMPLOYEE = "HI030003";                 //社員コード
    public static final String VIEW_WORD_LOGIN_GAID = "HI030004";                 //社員コード、パスワードを入力してください。
    public static final String VIEW_WORD_LOGIN_LOGIN = "HB030001";                     //ログイン
    public static final String VIEW_WORD_LOGIN_EXIT = "HB030002";                     //終了
    public static final String VIEW_WORD_LOGIN_CONTINE = "HB030004";                  //継続
    public static final String VIEW_WORD_LOGIN_LOGOUT = "HB030003";                     //ログアウト
    public static final String VIEW_WORD_LOGIN_PASS = "HI030006";                     //パスワード
    public static final String VIEW_WORD_LOGIN_SUB_TITLE = "HI030002";                 //コード読取、選択
    public static final String VIEW_WORD_LOGIN_ERR_EMPLOYEE_CODE_NONE = "HM030001";  //社員コードを入力してください。
    public static final String VIEW_WORD_LOGIN_ERR_EMPLOYEE_PASS_NONE = "HM030002";  //パスワードを入力してください。
    public static final String VIEW_WORD_LOGIN_READ_EMPLOYEE = "HI990019";            //社員バーコード読取
    public static final String VIEW_WORD_CHECK_EMPLOYEE_WORKER = "HI010005";            //作業者
    public static final String VIEW_WORD_CHECK_EMPLOYEE = "HT030002";            //作業者継続確認

    /**
     * 画面表示：貸出
     */
    public static final String VIEW_WORD_LEASE_DESTINATION = "HI040001";            //貸出先
    public static final String VIEW_WORD_LEASE_BIKO = "HI040005";                    //備考
    public static final String VIEW_WORD_LEASE_RESPONSIBLE = "HI040002";            //貸出担当
    public static final String VIEW_WORD_LEASE_RETURN = "HI040003";                 //返却予定日
    public static final String VIEW_WORD_LEASE_DESTINATION_SUB_TITLE = "HI040004";  //貸出先、返却日入力
    public static final String VIEW_WORD_LEASE_ERR_INPUT = "HM040002";              //貸出先または貸出担当を入力してください。
    public static final String VIEW_WORD_LEASE_ERR_RETURN_INPUT = "HM040003";       //返却予定日を入力してください。
    public static final String VIEW_WORD_LEASE_ERR_MASTER_NONE = "HM040004";        //貸出先マスタに登録されていません。
    public static final String VIEW_WORD_LEASE_READ_DESTINATION = "HI990020";        //貸出先バーコード読取
    public static final String VIEW_WORD_LEASE_READ_RESPONSIBLE = "HI990021";        //貸出担当バーコード読取

    /**
     * 画面表示：貸出一覧
     */
    public static final String VIEW_WORD_LEASE_LIST_ASSET_NUMBER = "HI990006";            //資産番号
    public static final String VIEW_WORD_LEASE_LIST_ASSET_NAME = "HI990007";              //資産名称
    public static final String VIEW_WORD_LEASE_LIST_MSG_REGISTER = "HM040001";            //貸出登録は完了しました。
    public static final String VIEW_WORD_LEASE_ERROR_RETURN = "HM040005";                 //返却予定日に過去日は指定できません。
    public static final String VIEW_WORD_LEASE_LIST_LIMIT_WARNING = "HI040007";             //貸出上限警告件数       20200226 Add Sasaki V1.2
    public static final String VIEW_WORD_LEASE_LIST_LIMIT_COUNT = "HI040006";             //貸出上限エラー件数       20200226 Add Sasaki V1.2
    public static final String VIEW_WORD_LEASE_ERR_NO_AUTHORITY = "HI040008";              //貸出不可                20210918 ADD Takase V2.0 
    public static final String VIEW_WORD_LEASE_LIST_LEASE_LIMIT_ERROR = "HI040009";       //リース期限エラー        20230830 ADD Kawamoto V2.4.1
    public static final String VIEW_WORD_LEASE_LIST_GENERIC_LIMIT_ERROR = "HI040010";     //○○期限エラー          20230830 ADD Kawamoto V2.4.1


    /**
     * 画面表示：返却
     */
    public static final String VIEW_WORD_RETURN_LIST_MSG_REGISTER = "HM050001";            //返却登録は完了しました。
    public static final String VIEW_WORD_RETURN_ERR_NO_AUTHORITY = "HI050001";             //返却不可                20210918 ADD Takase V2.0
    public static final String VIEW_WORD_RETURN_LIST_DUE_LIMIT_ERROR = "HI050002";        //返却期限エラー          20230906 ADD okubo V2.4.1

    public static final String VIEW_WORD_RETURN_MESS_DUE_LIMIT = "HM050002";        //返却期限        20230907 ADD okubo V2.4.1
    public static final String VIEW_WORD_RETURN_MESS_LEASE_LIMIT = "HM050003";      //リース期限      20230907 ADD okubo V2.4.1
    public static final String VIEW_WORD_RETURN_MESS_GENERIC_LIMIT = "HM050004";    //○○期限        20230907 ADD okubo V2.4.1
    public static final String VIEW_WORD_RETURN_MESS_LIMIT_WARNING = "HM050005";    //貸出上限警告    20230907 ADD okubo V2.4.1
    public static final String VIEW_WORD_RETURN_MESS_LIMIT_COUNT = "HM050006";      //貸出上限エラー  20230907 ADD okubo V2.4.1

    /**
     * 画面表示：照合返却
     */
    public static final String VIEW_WORD_MATRETURN_COUNT = "HI160001";      //返却数

    /**
     * 画面表示：保管先入力
     */
    public static final String VIEW_WORD_STORAGE_LOCATION_INPUT_MSG_ITEM = "HI060002";             //保管先の読取又は選択
    public static final String VIEW_WORD_STORAGE_LOCATION_INPUT_ERR_LABEL_QR = "HM990032";         //ロケーションラベルQRが不正です。
    public static final String VIEW_WORD_STORAGE_LOCATION_INPUT_ERR_LOCATION_MASTER = "HM990033";  //WEBAPIのログを参照してください。

    /**
     * 画面表示：保管一覧
     */
    public static final String VIEW_WORD_STORAGE_MSG_REGISTER = "HM060001";    //保管登録は完了しました。
    public static final String VIEW_WORD_STORAGE_ERR_NO_AUTHORITY = "HI060003";    //保管不可                        20210918 ADD Takase V2.0 

    /**
     * 画面表示：実棚
     */
    public static final String VIEW_WORD_INV_MENU_TITLE = "HT020001";          //実棚サブメニュー
    public static final String VIEW_WORD_INV_MENU_BUTTON = "HB020001";         //実棚ボタン
    public static final String VIEW_WORD_INV_LOC_SUB_TITLE = "HI020009";       //保管先の読取又は選択
    public static final String VIEW_WORD_INV_TITLE = "HT020002";                //実棚
    public static final String VIEW_WORD_INV_LOC_TITLE = "HT020003";           //実棚　ロケ指定
    public static final String VIEW_WORD_INV_READ_COUNT = "HI020001";           //実棚収集件数
    public static final String VIEW_WORD_INV_SUMI_COUNT = "HI020014";      //棚卸済数     20191209 Add Sasaki
    public static final String VIEW_WORD_INV_ACCUMULATION_COUNT = "HI020002";  //HT蓄積件数
    public static final String VIEW_WORD_INV_STOCK_COUNT = "HI020003";         //在庫件数
    public static final String VIEW_WORD_INV_LOC_COUNT = "HI020004";           //ロケ在庫件数
    public static final String VIEW_WORD_INV_LOC_OK_COUNT = "HI020005";        //正常数⇒実施数/残
    public static final String VIEW_WORD_INV_LOC_ERR_COUNT = "HI020006";       //ロケ違い
    public static final String VIEW_WORD_INV_LOC_NONE_COUNT = "HI020007";      //在庫なし数
    public static final String VIEW_WORD_INV_LOC_SUMI_COUNT = "HI020015";      //実施済/総数         20200305 Add Sasaki V1.2
    public static final String VIEW_WORD_INV_LOC_LEASE_COUNT = "HI020016";     //棚卸時の貸出数      20200305 Add Sasaki V1.2
    public static final String VIEW_WORD_INV_LOC_ZAN = "HI020017";              //残      20200305 Add Sasaki V1.2
    public static final String VIEW_WORD_INV_TAG_MODE = "HI020011";            //タグ読取モード
    public static final String VIEW_WORD_INV_READ_SUB_TITLE = "HI020008";      //読取開始してください
    public static final String VIEW_WORD_INV_READ_START = "HB020002";          //タグ読取開始
    public static final String VIEW_WORD_INV_READ_END = "HB020003";            //タグ読取終了
    public static final String VIEW_WORD_INV_MSG_REGISTER = "HM020001";        //実棚登録は完了しました
    public static final String VIEW_WORD_INV_MSG_LOC_L = "HM020007";        //ロケーション大を入力してください
    public static final String VIEW_WORD_INV_MSG_LOC_M = "HM020008";        //ロケーション中を入力してください
    public static final String VIEW_WORD_INV_LOC_ERR_BUTTON = "HB020004";      //ロケ違いボタン
    public static final String VIEW_WORD_INV_STOCK_NONE_BUTTON = "HB020005";   //在庫なしボタン
    public static final String VIEW_WORD_INV_CONFIRM_REGISTER = "HM020002";   //実棚データを登録します。よろしいですか？
    public static final String VIEW_WORD_INV_CONFIRM_ZERO_REGISTER = "HM020003";   //実棚収集件数は0件です。実棚データを登録してもよろしいですか？
    public static final String VIEW_WORD_INV_SUB_ASSETNO = "HI990046";   //サブ資産番号

    /**
     * 画面表示：探索資産読取
     */
    public static final String VIEW_WORD_SEARCH_ASSET_NUMBER_QR = "HI080001";         //資産番号QR読取
    public static final String VIEW_WORD_SEARCH_MSG_MAX_READ_COUNT = "HM080002";      //読み取り最大件数に達しました。
    public static final String VIEW_WORD_SEARCH_MSG_NO_ASSET_ID = "HM990068";         //EPCコードがない資産のため、処理対象外です。

    /**
     * 画面表示：探索一覧
     */
    public static final String VIEW_WORD_SEARCH_SEARCH_BUTTON = "HB080001";    //探索
    public static final String VIEW_WORD_SEARCH_SEARCH_RSSI = "HI080002";       //RSSI
    public static final String VIEW_WORD_SEARCH_SEARCH_S = "HI080003";       //遠
    public static final String VIEW_WORD_SEARCH_SEARCH_L = "HI080004";       //近

    /**
     * 画面表示：探索
     */
    public static final String VIEW_WORD_SEARCH_DISCOVERY_BUTTON = "HB080002";        //発見
    public static final String VIEW_WORD_SEARCH_MSG_DISCOVERY_NONE = "HM080001";      //タグの探索ができていません。


    /**
     * 画面表示：除却
     */
    public static final String VIEW_WORD_REMOVE_MSG_REGISTER = "HM070001";            //除却登録は完了しました。
    public static final String VIEW_WORD_REMOVE_ERR_NO_AUTHORITY = "HI070001";        //除却不可                       20210918 ADD Takase V2.0

    /**
     * 画面表示：タグ紐付け タグ読取
     */
    public static final String VIEW_WORD_LINK_ERR_REGISTER_NONE = "HM090002";         //資産マスタに登録されていません。
    public static final String VIEW_WORD_LINK_ERR_WRITE = "HM090006";                   //タグ書込みに失敗しました。
    public static final String VIEW_WORD_LINK_ERR_EPCSIZE= "HM090007";                  //EPC領域サイズ指定と異なるEPCです。
    public static final String VIEW_WORD_LINK_ERR_NOT_HUB = "HM090008";               //所属拠点外の資産はタグ紐付できません。     20210918 ADD Takase V2.0
    public static final String VIEW_WORD_LINK_ERR_NOT_HT_HUB= "HM090009";             //対象拠点外の資産はタグ紐付できません。     20210918 ADD Takase V2.0

    /**
     * 画面表示：資産確認
     */
    public static final String VIEW_WORD_LINK_ASSET_ID = "HI990008";                    //資産ID
    public static final String VIEW_WORD_LINK_EPC_ENCODE = "HI090003";                  //読取EPCｴﾝｺｰﾄﾞ値
    public static final String VIEW_WORD_LINK_LOC_INFO = "HI090004";                    //ロケ情報
    public static final String VIEW_WORD_LINK_LOCL_TITLE = "HI090005";                  //大区分
    public static final String VIEW_WORD_LINK_LOCM_TITLE = "HI090006";                  //中区分
    public static final String VIEW_WORD_LINK_LOCS_TITLE = "HI090007";                  //小区分
    public static final String VIEW_WORD_LINK_MSG_ITEM = "HI090002";                    //タグ読取　書込と紐付
    public static final String VIEW_WORD_LINK_REGISTER_BUTTON = "HB090001";             //紐付登録
    public static final String VIEW_WORD_LINK_MSG_SET = "HM090003";                     //既に紐付済です。
    public static final String VIEW_WORD_LINK_MSG_SET_OTHER_NUMBER = "HM090004";        //既に他の資産番号で紐付られています。
    public static final String VIEW_WORD_LINK_MSG_SET_REGISTER = "HM090001";            //タグ書込&紐付登録は完了しました。
    public static final String VIEW_WORD_LINK_MSG_SET_TAG_REGISTER = "HM090005";        //タグ紐付登録は完了しました。

    /**
     * 画面表示：貸出(照合)
     */
    public static final String VIEW_WORD_SEND_BUTTON = "HB180001";        //送信
    public static final String VIEW_WORD_MATCH_LEASE_MSG_EXSIT_TEMP_ASEET = "HM180001";        //仮置き対象の資産を読みました。クリアして再度貸出対象の資産だけを読取してください。

    /**
     * 画面表示：工事選択　　2024/10/11 ADD Shirakawa v4.0
     */
    public static final String VIEW_WORD_CONSTRUCTION_TITLE = "HT170001";        //工事選択
    public static final String VIEW_WORD_CONSTRUCTION_SET_WORK_CARRY_DAY = "HI170001";        //作業実施日

    public static final String VIEW_WORD_CONSTRUCTION_SET_WORK_DAY = "HI170002";        //作業日

    public static final String VIEW_WORD_CONSTRUCTION_SET_LIABILITY = "HI170003";        //責任者

    public static final String VIEW_WORD_CONSTRUCTION_SET_WORK_CONTENT = "HI170004";        //作業内容

    public static final String VIEW_WORD_CONSTRUCTION_SEARCH_BUTTON = "HB170001";        //検索

    public static final String VIEW_WORD_CONSTRUCTION_CLEAR_BUTTON = "HB170002";        //クリア

    public static final String VIEW_WORD_CONSTRUCTION_INPUT_MSG_ITEM = "HI170005";        //貸出する工事を選択してください

    /**
     * 画面表示：工事情報確認　　2024/10/11 ADD Shimomura v4.0
     */
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_TITLE = "HT170002";                //工事情報確認
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_LOCATION="HI170006";              //管理拠点
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_UP_AND_DOWN="HI170008";              //上り/下り
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_LINE_TYPE="HI170009";              //線路種
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_STATION="HI170010";              //駅区間
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_DISTANCE="HI170011";              //距離区間
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO6="HI170012";              //工事情報6
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO7="HI170013";              //工事情報7
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO8="HI170014";              //工事情報8
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO9="HI170015";              //工事情報9
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO10="HI170016";              //工事情報10
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO11="HI170017";              //工事情報11
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO12="HI170018";              //工事情報12
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO13="HI170019";              //工事情報13
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO14="HI170020";              //工事情報14
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO15="HI170021";              //工事情報15
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO16="HI170022";              //工事情報16
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO17="HI170023";              //工事情報17
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO18="HI170024";              //工事情報18
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO19="HI170025";              //工事情報19
    public static final String VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO20="HI170026";              //工事情報20
    public static final String VIEW_WORD_CONST_ITEM = "HI170027";        //工事
    public static final String VIEW_WORD_NEXT_PUSH = "HI170028";         //次へタップ
    /**
     * 画面表示：仮置き登録　　2024/10/11 ADD Shirakawa v4.0
     */
    public static final String VIEW_WORD_TEMP_LIST_TITLE = "HT140001";        //仮置一覧
    public static final String VIEW_WORD_TEMP_REGISTER_TITLE = "HT140002";        //仮置登録
    public static final String VIEW_WORD_TEMP_REGIST_BUTTON = "HB140001";        //新規登録
    public static final String VIEW_WORD_TEMP_CHENGE_BUTTON = "HB140002";        //変更

    public static final String VIEW_WORD_OTHER_TEMP_COUNT = "HI140001";        //他仮置き
    public static final String VIEW_WORD_TEMP_DAY = "HI140002";        //仮置き日
    public static final String VIEW_WORD_TEMP_WORK_LOC = "HI140003";        //作業場所
    public static final String VIEW_WORD_TEMP_READ_MSG_ITEM = "HI140004";        //読取する仮置を選択してください
    public static final String VIEW_WORD_TEMP_INFO_REGISTER = "HI140005";        //仮置き情報を登録してください
    public static final String VIEW_WORD_TEMPDATA_REGISTER = "HM140001";        //仮置き登録は完了しました。
    public static final String VIEW_WORD_OTHER_TEMPDATA = "HM140002";        //ほかに仮置き登録した資産があります。よろしいですか？
    public static final String VIEW_KEY_STATE_SELECT = "VIEW_KEY_STATE_SELECT";               //項目選択ボタン状態
    public static final String VIEW_KEY_STATE_REGISTER = "REGISTER";               //新規登録
    public static final String VIEW_KEY_STATE_UPDATE = "UPDATE";               //変更
    public static final String VIEW_KEY_TEMPDATA_REGISTER_CHECK = "HM140003";   //仮置きデータを登録します。よろしいですか？
    public static final String VIEW_KEY_TEMPDATA_INPUT_CHECK = "HM140004";      //を入力してください。
    public static final String VIEW_KEY_TEMP_REMARK = "HI140006";      //備考

    /**
     * 画面表示：持出照合　　2024/10/11 ADD Shirakawa v4.0
     */
    public static final String VIEW_WORD_MATCHINDATA_REGISTER = "HM130001";        //持出照合を登録は完了しました。

    /**
     * 画面表示：持帰照合　　2024/10/11 ADD Shirakawa v4.0
     */
    public static final String VIEW_WORD_MATCHOUTDATA_REGISTER = "HM150001";        //持帰照合を登録は完了しました。

    /**
     * 画面表示：照合返却　　2024/10/11 ADD Shirakawa v4.0
     */
    public static final String VIEW_WORD_MATCHRETURNDATA_REGISTER = "HM160001";        //照合返却を登録は完了しました。

    /**
     * 画面表示：システム設定
     */
    public static final String VIEW_WORD_SYSTEM_HTID = "HI100008";                //HTID
    public static final String VIEW_WORD_SYSTEM_VOLUME = "HI100001";              //音量
    public static final String VIEW_WORD_SYSTEM_DENPA = "HI100004";               //電波設定
    public static final String VIEW_WORD_SYSTEM_SESSION_FLG = "HI100009";         //SessionFlg
    public static final String VIEW_WORD_SYSTEM_Q_VALUE = "HI100010";             //Qvalue
    public static final String VIEW_WORD_SYSTEM_SERVER_CONNECT = "HI100002";      //サーバー接続
    public static final String VIEW_WORD_SYSTEM_TIME_OUT = "HI100003";            //タイムアウト
    public static final String VIEW_WORD_SYSTEM_DESCRIPTION = "HI100011";         //詳細設定
    public static final String VIEW_WORD_ALL_ERR_INTENCITY_VALUE = "HM100001";    //電波強度の設定に誤りがあります。
    //20210918 ADD Takase V2.0 START
    public static final String VIEW_WORD_SYSTEM_HUB = "HI100012";                 //対象拠点
    public static final String VIEW_WORD_SYSTEM_TRANSFER = "HI100013";            //HTデータ送受信
    public static final String VIEW_WORD_SYSTEM_SET_HUB = "HI100014";             //拠点設定
    //20210918 ADD Takase V2.0 END
    public static final String VIEW_WORD_SYSTEM_VALID = "HI100015";               //有効
    public static final String VIEW_WORD_SYSTEM_RFIDREADER_USE_VALUE = "HI100017";//使用
    public static final String VIEW_WORD_SYSTEM_QRREADER = "HI100018";            //QR読取機器
    public static final String VIEW_WORD_SYSTEM_RFIDREADER = "HI100019";          //RFIDReader
    public static final String VIEW_WORD_SYSTEM_LOGIN_TIMEOUT = "HI990044";       //操作タイムアウト(分)
    public static final String VIEW_WORD_SYSTEM_LOGIN_TIMEOUT_MESSAGE = "HI990045";       //操作タイムアウト(分)
    public static final String VIEW_WORD_SYSTEM_COLLATIONWORK = "HI100020";          //照合作業

    /**
     * WebAPI
     */
    public static final String VIEW_WORD_WEBAPI_ERR_MASTER = "HM800001";        //マスタが最新ではない
    public static final String VIEW_WORD_WEBAPI_CONNECT_START = "HI110001";    //通信接続開始
    public static final String VIEW_WORD_WEBAPI_CONNECT_WAIT = "HI110002";      //通信接続待ち
    public static final String VIEW_WORD_WEBAPI_DATA_SEND = "HI110003";         //データ送信中
    public static final String VIEW_WORD_WEBAPI_PROCESS_WAIT = "HI110004";      //処理終了待ち
    public static final String VIEW_WORD_WEBAPI_DELETE_DATA = "HI110005";        //データ削除中
    public static final String VIEW_WORD_WEBAPI_MASTER_WAIT = "HI110006";       //マスタ要求中
    public static final String VIEW_WORD_WEBAPI_MASTER_RECIVE = "HI110007";       //マスタ取込中
    public static final String VIEW_WORD_WEBAPI_MASTER_RECIVE_END = "HI110008";   //マスタ取込終了
    public static final String VIEW_WORD_WEBAPI_ERROR_SEND = "HM110001";           //送信処理に失敗しました。
    public static final String VIEW_WORD_WEBAPI_ERROR_RECEIVE = "HM110002";       //受信処理に失敗しました。
    public static final String VIEW_WORD_WEBAPI_ERROR_CONNECT = "HM110003";       //Webサーバーの接続に失敗しました。
    public static final String VIEW_WORD_WEBAPI_ERROR_SETTING = "HM110004";       //サーバー情報が設定されていません。
    public static final String VIEW_WORD_WEBAPI_ERROR_SEND_DATA = "HM110005";       //送信データがありません。
    public static final String VIEW_WORD_WEBAPI_DELETE_INVDATA = "HM110006";       //棚卸データを削除しました。

    /**
     * USB通信
     */
    public static final String VIEW_WORD_ALL_USB_SEND_TITLE = "HI990033";               //実績送信中
    public static final String VIEW_WORD_ALL_USB_SEND_GUIDE = "HM990043";               //HTとUSBを接続してから蓄積データ送信ボタンを押してください
    public static final String VIEW_WORD_ALL_USB_SEND_DATA = "HB990011";                //蓄積データ送信
    public static final String VIEW_WORD_ALL_USB_RECEIVE_TITLE = "HI990034";            //実績送信中
    public static final String VIEW_WORD_ALL_USB_RECEIVE_GUIDE = "HM990044";            //HTとUSBを接続、PC側のデータ送信を行ってから、マスタ取込受信ボタンを押してください
    public static final String VIEW_WORD_COMMUNICATION_TIMEOUT = "HM800012";            //通信タイムアウトが発生しました
    public static final String VIEW_WORD_ALL_USD_RECIVE_DATA = "HB990012";              //マスタ取込受信
    public static final String VIEW_WORD_USB_DISCONNECTED = "HM800013";                 //USBが接続されていません。接続状態を確認してください。

    /**
     * エラーメッセージ
     */
    public static final String VIEW_WORD_ALL_ERR_LOC_INPUT = "HM990011";        //未入力のロケがあります。
    public static final String VIEW_WORD_ALL_ERR_LOC_NONE = "HM990010";         //指定されたロケが存在しない
    public static final String VIEW_WORD_ALL_ERR_EMPLOYEE_NONE = "HM990031";    //指定された社員が存在しない
    public static final String VIEW_WORD_ALL_ERR_TAG_READ_END = "HM990039";        //タグ読み取りを終了してください
    public static final String VIEW_WORD_ALL_ERR_INPUT_MAX_LENGTH = "HM990040";   //最大桁数を超えています
    public static final String VIEW_WORD_ALL_BATTERY_CHARGE = "HM990041";        //電池残量が少なくなってきました。充電することをお勧めします。
    public static final String VIEW_WORD_ALL_ERR_CONNECT = "HM990001";          //RFIDリーダライタとのペアリングを行っています。%1接続に失敗する場合は、ペアリングボタンを押して、ペアリング状態にしてください。
    public static final String VIEW_WORD_ALL_ERR_RFID_CONNECT = "HM990003";     //RFIDリーダライタが接続されていません。
    //20210918 ADD Takase V2.0 START
    public static final String VIEW_WORD_ALL_ERR_NO_AUTHORITY = "HM990042";             //権限がありません。
    public static final String VIEW_WORD_ALL_CONFIRM_USB_RECEIVE_ERR = "HM990047";      //受信データが存在しません。PC側のデータ送信を実施してください。
    public static final String VIEW_WORD_ALL_ERR_PASSWORD = "HM990049";               //パスワードが正しくありません。
    public static final String VIEW_WORD_ALL_ERR_TARGET_HUB = "HM990050";             //対象拠点に所属していないため、ログインできません。
    public static final String VIEW_WORD_ALL_ERR_TARGET_HUB_LOC = "HM990051";         //対象拠点のみ選択可能です。
    //20210918 ADD Takase V2.0 END
    public static final String VIEW_WORD_ALL_ERR_FW_VER = "HM990052";           //AutoLinkProfile機能が利用できないF/WのSP1です。設定をOFFにしてください。          20220317 ADD Suzuki V2.1
    public static final String VIEW_WORD_ALL_ERR_ASSET_NO_SEND = "HM990053";        //マスタ取込受信が完了しました。対象拠点が未指定のため資産データを取り込みませんでした。対象拠点を指定してください。 20230206 ADD Kawamoto V2.3
    public static final String VIEW_WORD_ALL_ERR_HT_DATE = "HM990055";         //マスタ受信処理を中断して、Androidの時刻設定機能で修正してください。 20230221 ADD Sato V2.3
    public static final String VIEW_WORD_ALL_ERR_UPD_DATE = "HM990056";        //時刻合わせに失敗しました。、Androidの時刻設定機能で修正してください。 20230221 ADD Sato V2.3
    public static final String VIEW_WORD_ALL_ERR_INV_NO_START_LOCL = "HM020005";       //棚卸を開始していないロケ大です。 20231011 ADD Shibata v3.0

    public static final String VIEW_WORD_ERROR_UNEXPECTED_ERROR = "HM809999";           //予期せぬエラーが発生しました
    public static final String VIEW_WORD_ERROR_INVALID_RESPONSE_F8ROM_WEBAPI = "CM990019";          //WebAPIから異常応答がありました
    public static final String VIEW_WORD_ERROR_RETRY_LOGIN = "HM030002";     //一定時間操作がないため、作業者確認をします。社員コードを入力し、継続してください。
    public static final String VIEW_WORD_ERROR_EMPLOYEE_CODE = "HM030003";     //社員コードが違います。社員コードを入力しなおしてください。
    public static final String VIEW_WORD_ERROR_ISNOT_TANGIBLE = "HM080016";     //無形資産です。
    public static final String VIEW_WORD_ERROR_ISNOT_USE_RFID_LOC = "HM080017";     //バーコード運用拠点の資産です。



    /**
     * 確認メッセージ
     */
    public static final String VIEW_WORD_ALL_CONFIRM_DATA_CLEAR = "HM990020";       //読取データをクリアします。よろしいですか？
    public static final String VIEW_WORD_ALL_CONFIRM_DATA_REGISTER = "HM990021";    //読取データを登録します。よろしいですか？
    public static final String VIEW_WORD_ALL_CONFIRM_DATA_RETURN= "HM990022";       //登録されていない読取データは破棄されます。よろしいですか？
    public static final String VIEW_WORD_ALL_CONFIRM_SEND_DATA = "HM990023";        //蓄積データの送信が完了しました。
    public static final String VIEW_WORD_ALL_CONFIRM_RECIVE_DATA = "HM990024";      //マスタ取込受信が完了しました。
    public static final String VIEW_WORD_ALL_CONFIRM_REGISTER_DATA = "HM990036";    //マスタが最新でない恐れがありますが、読取データを登録しますか？
    public static final String VIEW_WORD_ALL_CONFIRM_LEASE_DATA = "HM990037";       //貸出中のデータがあります。登録してもよろしいですか？
    public static final String VIEW_WORD_ALL_CONFIRM_RETURN_DATA = "HM990038";      //返却中のデータがあります。登録してもよろしいですか？
    //20210918 ADD Takase V2.0 START
    public static final String VIEW_WORD_ALL_CONFIRM_USB_RECEIVE = "HM990045";          //データ送信が完了しました。PC側のデータ受信を行ってください。
    public static final String VIEW_WORD_ALL_INPUT_PASSWORD = "HM990048";           //パスワードを入力してください。
    //20210918 ADD Takase V2.0 END
    public static final String VIEW_WORD_ALL_CONFIRM_SET_HUB = "HM990054";          //対象拠点を変更した場合はマスタ受信を行ってください。 20230206 ADD Kawamoto V2.3
    public static final String VIEW_WORD_ALL_CONFIRM_CHECK_ERR_ASSET = "HM990057";  //チェックエラーの資産は返却登録ができません。それ以外の資産を登録します。よろしいですか？ 20231024 ADD Sato V2.4.1
    public static final String VIEW_WORD_ALL_RESTART_APPLICATION = "HM990058";      //RFIDReader使用の設定が変更されました。アプリを終了します。よろしいですか？
    public static final String VIEW_WORD_EXIST_NOMATCH = "HM990059";      //照合できていない資産があります。よろしいですか？

    public static final String VIEW_WORD_NOTHING_UPDATE_AUTH = "HM990061";      //更新権限がありません
    public static final String VIEW_WORD_APPLICATION_EXIT = "HM030004";     //アプリを終了します。よろしいですか？
    public static final String VIEW_WORD_LOGOUT = "HM030005";     //ログアウトします。よろしいですか？

    public static final String VIEW_WORD_UNSENT_DATA_LOGOUT = "HM030006";    //実績データが未送信です。ログアウトすると実績データが全て削除されますが、よろしいでしょうか？



 /************************************************************************************
 ** サイズ
 ************************************************************************************/
    public static final int VIEW_LOCATION_NAME_SIZE = 29;
    public static final int VIEW_LEASE_NAME_SIZE = 29;
    public static final int VIEW_LEASE_SETTING_NAME_SIZE = 34;
    public static final int VIEW_LOCATION_SETTING_NAME_SIZE = 32;
    public static final int VIEW_TAGLINK_CONFIRM_SIZE = 15;

    //操作ログ コンテントカットサイズ
    public static final int CONTENT_SIZE = 1024;

    //入力文字数
    public static final int VIEW_MAXLENGTH_LOC_L_CD = 10;       //20210510 UPD Suzuki v1.3
    public static final int VIEW_MAXLENGTH_LOC_M_CD = 10;       //20210510 UPD Suzuki v1.3
    public static final int VIEW_MAXLENGTH_LOC_S_CD = 10;       //20210510 UPD Suzuki v1.3
    public static final int VIEW_MAXLENGTH_LEASE_CD = 15;
    public static final int VIEW_MAXLENGTH_EMPLOYEE_CD = 15;
    public static final int VIEW_MAXLENGTH_BIOS = 255;
    public static final int PC_LENGTH_BIT_SIZE = 5;             // PCのLength部分のビット長
    public static final int PC_HEX_SIZE = 4;                    // EPC領域のPC部分のサイズ

/************************************************************************************
 ** 文字コード
 ************************************************************************************/
    public static final String DEFUALT_CHAR = "MS932";

 /************************************************************************************
  ** テーブル名称
  ************************************************************************************/
    /** 資産マスタ */
    public static final String MASTER_ASSET  = "M_ASSET";

    /** 部署マスタ */
    public static final String MASTER_DEPARTMENT = "M_DEPARTMENT";

    /** 貸出先マスタ */
    public static final String MASTER_LEASE_PLACE  = "M_LEASE_PLACE";

   /** ロケーションマスタ */
    public static final String MASTER_LOCATION  = "M_LOCATION";

    /** 社員マスタ */
    public static final String MASTER_EMPLOYEE  = "M_EMPLOYEE";

    /** 文言マスタ */
    public static final String MASTER_WORD  = "M_WORD";

    /** 設定マスタ */
    public static final String MASTER_SETTING  = "M_SETTING";

    //20210918 ADD Takase V2.0 START
    /** 権限マスタ */
    public static final String MASTER_AUTH = "M_AUTH";

    /** 社員所属拠点マスタ */
    public static final String MASTER_EMPLOYEE_HUB = "M_EMPLOYEE_HUB";
    //20210918 ADD Takase V2.0 END

    /** 資産貸出情報テーブル */
    public static final String TABLE_ASSET_LEASE  = "T_ASSET_LEASE";

    /** 資産処理履歴テーブル */
    public static final String TABLE_PROCESS_LOG  = "T_PROCESS_LOG";

    /** 操作ログテーブル */
    public static final String TABLE_OPERATION_LOG  = "T_OPERATION_LOG";

    /** 一時表テーブル */
    public static final String TEMP_PROCESS_LOG  = "TEMP_PROCESS_LOG";

    /** 一時表テーブル */
    public static final String TEMP_SEARCH  = "TEMP_SEARCH";

    //20231004 ADD Shibata V3.0 START
    /** 棚卸開始情報テーブル */
    public static final String TABLE_INVENTORY  = "T_INVENTORY";

    /** 棚卸開始情報テーブル */
    public static final String TABLE_INVENTORY_START = "T_INV_START";
    //20231004 ADD Shibata V3.0 END

    /** 工事マスタ */
    public static final String MASTER_CONSTRUCTION  = "M_CONSTRUCTION";

	/** 仮置きテーブル */
    public static final String TABLE_TEMPORARY  = "T_TEMPORARY";

    /** 仮置き資産テーブル */
    public static final String TABLE_TEMP_ASSET  = "T_TEMP_ASSET";

    /** 工事資産テーブル */
    public static final String TABLE_CONST_ASSET  = "T_CONST_ASSET";

    /** 照合資産リストテーブル */
    public static final String TABLE_MATCH_ASSET_LIST  = "T_MATCH_ASSET_LIST";

    /** 返却リストテーブル */
    public static final String TABLE_RET_ASSET_LIST  = "T_RET_ASSET_LIST";

    /** 貸出リストテーブル */
    public static final String TABLE_LENT_ASSET_LIST  = "T_LENT_ASSET_LIST";

    /** 仮置きリストテーブル */
    public static final String TABLE_TEMP_LIST  = "T_TEMP_LIST";

    /** 仮置き資産リストテーブル */
    public static final String TABLE_TEMP_ASSET_LIST  = "T_TEMP_ASSET_LIST";

    /** 照合リストテーブル */
    public static final String TABLE_MATCH_LIST  = "T_MATCH_LIST";

    /** 予約情報テーブル */
    public static final String TABLE_RESERVE  = "T_RESERVE";
/************************************************************************************
 ** RFID関連
 ************************************************************************************/

    /** EPC デフォルトアクセスパスワード */
    public static final String TAG_DEFAULT_ACCESS_PWD = "00000000";

    /*
     * RFIDタグにおける各単位の考え方について
     * ※1Word = 2Byte = 16bit
     */

    /** EPC 96bit 16進数桁数 */
    public static final int EPC_96BIT_HEX_SIZE = 24;

    /** EPC 128bit 16進数桁数 */
    public static final int EPC_128BIT_HEX_SIZE = 32;

    /** EPC 96bit Word型桁数 */
    public static final int EPC_96BIT_WORD_SIZE = 6;

    /** EPC 128bit Word型桁数 */
    public static final int EPC_128BIT_WORD_SIZE = 8;

    /** EPC領域内のEPC開始位置(bit型) */
    public static final int EPC_BIT_TYPE_START_POS = 32;

    /** EPC領域内のPC開始位置(bit型) */
    public static final int PC_BIT_TYPE_START_POS = 16;

    /** EPC領域内のEPC開始位置(byte型) */
    public static final int EPC_BYTE_TYPE_START_POS = 4;

    /** EPC領域内のPC開始位置(byte型) */
    public static final int PC_BYTE_TYPE_START_POS = 2;

    /** EPC領域内のEPC開始位置(Word型) */
    public static final int EPC_WORD_TYPE_START_POS = 2;

    /** EPC領域内のPC開始位置(Word型) */
    public static final int PC_WORD_TYPE_START_POS = 1;

/************************************************************************************
 ** フォルダ・ファイル名
 ************************************************************************************/
    public static final String FOLDER_TEST = "test";
}

