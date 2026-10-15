package com.rfid.setting;

import android.app.Activity;
import android.content.SharedPreferences;
import android.content.res.Resources;
import android.database.sqlite.SQLiteDatabase;
import android.preference.PreferenceManager;
import android.util.Log;
import android.widget.Spinner;

import com.rfid.common.Common;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfidpk.R;

import java.util.Objects;


// 各種設定値を管理する クラス
public class SettingValue {

    //SharedPreferencesキー：RFIDReader使用
    public final String KEY_RFIDREADER_USE = "rfidreader_use";

    //SharedPreferencesキー：照合作業
    private final String KEY_COLLATION_OPERATION = "collation_operation";

    //SharedPreferencesキー：EmployeePrevSet使用
    public final String KEY_EMPLOYEE_PREV_SET = "employee_prev_set";

    //SharedPreferencesキー：切替
    private final String KEY_READER_CHANGE = "reader_change";

    //SharedPreferencesキー：ＰＣ接続ＩＰアドレス
    private final String KEY_CONNECT_IPADDRESS = "connect_server";

    //SharedPreferencesキー：ＰＣ接続ポート番号
    private final String KEY_CONNECT_PORTNO = "connect_portno";

    //SharedPreferencesキー：ＰＣ接続タイムアウト
    private final String KEY_CONNECT_TIMEOUT = "connect_timeout";

    //SharedPreferencesキー：操作放置時間
    private final String KEY_NO_CONTROL_TIME = "connect_timeout";

    //SharedPreferencesキー(共通部)：タグ読み込み時の電波強度
    private final String KEY_BT_READ_INTENSITY = "bt_read_intensity";

    //SharedPreferencesキー(共通部)：タグ読み込み時のセッション値
    private final String KEY_BT_READ_SESSION = "bt_read_session";

    //SharedPreferencesキー(共通部)：タグ読み込み時のＱバリュー値
    private final String KEY_BT_READ_QVALUE = "bt_read_Q_value";

    //SharedPreferencesキー(個別機能)：棚卸
    public final String KEY_EACH_INVENTORY = "_inv";

    //20191120 Add Start Sasaki
    //SharedPreferencesキー(個別機能)：棚卸(強)
    public final String KEY_EACH_INVENTORY_L = "_invL";

    //SharedPreferencesキー(個別機能)：棚卸(中)
    public final String KEY_EACH_INVENTORY_M = "_invM";

    //SharedPreferencesキー(個別機能)：棚卸(弱)
    public final String KEY_EACH_INVENTORY_S = "_invS";

    //SharedPreferencesキー(個別機能)：棚卸画面で設定した区分(強中弱)
    public final String KEY_EACH_INVENTORY_KBN = "_invKbn";

    //棚卸画面で設定した区分(強中弱)に設定する値 強
    public final String INVENTORY_VALUE_L = "L";

    //棚卸画面で設定した区分(強中弱)に設定する値 中
    public final String INVENTORY_VALUE_M = "M";

    //棚卸画面で設定した区分(強中弱)に設定する値 弱
    public final String INVENTORY_VALUE_S = "S";
    //20191120 Add End Sasaki

    //SharedPreferencesキー(個別機能)：探索
    public final String KEY_EACH_SEARCH = "_sch";

    //SharedPreferencesキー(個別機能)：貸出
    public final String KEY_EACH_LEASE = "_les";

    //SharedPreferencesキー(個別機能)：返却
    public final String KEY_EACH_RETURN = "_ret";

    //SharedPreferencesキー(個別機能)：保管
    public final String KEY_EACH_STORAGE = "_str";

    //SharedPreferencesキー(個別機能)：除却
    public final String KEY_EACH_DISPOSE = "_dsp";

    //SharedPreferencesキー(個別機能)：タグ紐付
    public final String KEY_EACH_TAGLINK = "_lik";

    //SharedPreferencesキー(個別機能)：持込照合
    public final String KEY_EACH_MATIN = "_min";

    //SharedPreferencesキー(個別機能)：持帰照合
    public final String KEY_EACH_MATOUT = "_mout";

    //SharedPreferencesキー(個別機能)：仮置登録
    public final String KEY_EACH_MATTEMP = "_mtemp";

    //SharedPreferencesキー(個別機能)：照合返却
    public final String KEY_EACH_MATRET = "_mret";

    //20210918 ADD Takase V2.0 START
    //SharedPreferencesキー：対象拠点
    private final String KEY_TARGET_HUB = "target_hub";

    //SharedPreferencesキー：送信方法
    private final String KEY_TRANSFER = "transfer";
    //20210918 ADD Takase V2.0 END

    //20220316 ADD Suzuki V2.1 START
    //SharedPreferencesキー(共通部)：タグ読み込み時のAutoLinkProfile
    public final String KEY_BT_READ_AUTO_LINK = "bt_read_auto_link";
    //20220316 ADD Suzki V2.1 END

    //SharedPreferencesキー：ログイン担当者コード
    public final String KEY_LOGIN_EMPLOYEE_CODE = "login_employee_code";

    //SharedPreferencesキー：ログイン担当者名
    public final String KEY_LOGIN_EMPLOYEE_NAME = "login_employee_name";

    //SharedPreferencesキー：ログイン担当者本部フラグ
    public final String KEY_LOGIN_HEAD_OFFICE = "login_head_office";

    //SharedPreferencesキー：ログイン担当者権限区分
    public final String KEY_LOGIN_AUTH = "login_auth";

    //SharedPreferencesキー：ログイン担当者RFIDタグ拠点フラグ
    public final String KEY_LOGIN_LOC_RFID_FLG = "login_loc_rfid_flg";

    //SharedPreferencesキー：操作タイムアウト時間
    public final String LOGIN_TIMEOUT = "login_timeout";

    //SharedPreferencesキー：最終操作日時
    public final String LAST_OPERATION_TIME = "last_operation_time";

    //SharedPreferencesキー：RFID使用区分[設定画面用]
    public final String RFID_READER_USE_SETTING = "RfidReaderUseSetting";

    /** SharedPreferencesキー：言語 */
    private final String KEY_LANGUAGE = "Language";

    //電波強度設定範囲
    //public final int INTENCITY_MIN = 4;
    //public final int INTENCITY_MAX = 30;
    public final int INTENCITY_MIN = 120;
    public final int INTENCITY_MAX = 280;

    //個別機能指定の設定値を返す
    public int p_fncIntencity = 0;      //電波強度
    public String p_fncSession = "";    //セッション
    public short p_fncQValue = 0;       //Ｑバリュー

    //ログの設定値
    public static final String SETTING_LOG_OUTPUT = "SETTING_LOG_OUTPUT";
    public static final String SETTING_LOG_LEVEL = "SETTING_LOG_LEVEL";
    public static final String SETTING_LOG_ROLLING = "SETTING_LOG_ROLLING";
    public static final String SETTING_LOG_MAXSIZE = "SETTING_LOG_MAXSIZE";
    public enum LOG_OUTPUT_FLAG {
        NO,     //ログ出力しない
        YES;    //ログ出力します。
    }

    /**
     * 対象言語を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getTargetLang(Activity act) {
        Resources res = act.getResources();
        return getString(KEY_LANGUAGE, res.getString(R.string.default_language), act);
    }

    /**
     * 対象言語を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setTargetLang(Activity act,  String value) {
        setString(KEY_LANGUAGE, value, act);
    }

    //エラーメッセージ
    public String p_ErrorMessage = "";

    /**
     * RFIDReader使用有無の設定を取得する
     * @param act    アクティビティ
     * @return       設定値
     */

    public String getRfidReaderUse(Activity act) {
        return getString(KEY_RFIDREADER_USE, act.getString(R.string.default_bt_RFID_reader_use), act);
    }

    /**
     * RFIDReader使用有無の設定を保存する
     * @param act
     * @param value
     */

    public void setRfidReaderUse(Activity act, String value) {
        setString(KEY_RFIDREADER_USE, value, act);
    }

    /**
     * EmployeePrevSet使用有無の設定を取得する
     * @param act    アクティビティ
     * @return       設定値
     */

    public String getEmployeePrevSet(Activity act) {
        return getString(KEY_EMPLOYEE_PREV_SET, act.getString(R.string.default_bt_employee_prev_set), act);
    }

    /**
     * EmployeePrevSet使用有無の設定を保存する
     * @param act
     * @param value
     */

    public void setEmployeePrevSet(Activity act, String value) {
        setString(KEY_EMPLOYEE_PREV_SET, value, act);
    }

    /**
     * 管理⇔一般の切替を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getCollationOperation(Activity act) {
        Resources res = act.getResources();
        SQLiteDatabase db = null;
        SettingValue C_Setting = new SettingValue();

        try {

            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(act);
            db = dbHelper.getWritableDatabase();

            boolean isRFIDReaderUse = Objects.equals(C_Setting.getRfidReaderUse(act), "1");
            //読取媒体の設定により、初期値を変更する
            if (isRFIDReaderUse) {
                return getString(KEY_COLLATION_OPERATION, CommonConst.COLLATION_OPERATION.Control.getString(), act);
            } else {
                return getString(KEY_COLLATION_OPERATION, CommonConst.COLLATION_OPERATION.General.getString(), act);
            }

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 管理⇔一般の切替を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setCollationOperation(Activity act,  String value) {
        setString(KEY_COLLATION_OPERATION, value, act);
    }

    /**
     * RFIDReader⇔Androidの切替を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getReaderChange(Activity act) {
        Resources res = act.getResources();
        SQLiteDatabase db = null;
        SettingValue C_Setting = new SettingValue();

        try {
            //20230221 ADD Sato V2.3 Start
            //キーエンス製DXシリーズ、iData、RS35の場合はAndroidを返却  20230410 UPD Sato v2.3.1
            if (Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString()) ||
                    Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.IDATA.getString()) ||
                    Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString())) {
                return getString(KEY_READER_CHANGE, CommonConst.READER_SETTING.Android.getString(), act);
            }
            //20230221 ADD Sato V2.3 End

            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(act);
            db = dbHelper.getWritableDatabase();

            boolean isRFIDReaderUse = Objects.equals(C_Setting.getRfidReaderUse(act), "1");
            //読取媒体の設定により、初期値を変更する
            //if (CommonConst.READING_MEDIUM.TAG.getString().equals(SettingFactory.GetReadingMedium(db))) {
            if (isRFIDReaderUse) {
                return getString(KEY_READER_CHANGE, CommonConst.READER_SETTING.Reader.getString(), act);
            } else {
                return getString(KEY_READER_CHANGE, CommonConst.READER_SETTING.Android.getString(), act);
            }

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * RFIDReader⇔Androidの切替を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setReaderChange(Activity act,  String value) {
        setString(KEY_READER_CHANGE, value, act);
    }

    /**
     * ＰＣ接続ＩＰアドレスを取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getConnectServer(Activity act) {
        Resources res = act.getResources();
        return getString(KEY_CONNECT_IPADDRESS, "", act);
    }

    /**
     * ＰＣ接続ＩＰアドレスを保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setConnectServer(Activity act,  String value) {
        setString(KEY_CONNECT_IPADDRESS, value, act);
    }

    /**
     * ＰＣ接続ポート番号を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getConnectPortNo(Activity act) {
        Resources res = act.getResources();
        return getString(KEY_CONNECT_PORTNO, "", act);
    }

    /**
     * ＰＣ接続ポート番号を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setConnectPortNo(Activity act,  String value) {
        setString(KEY_CONNECT_PORTNO, value, act);
    }

    /**
     * ＰＣ接続タイムアウトを取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getConnectTimeOut(Activity act) {
        Resources res = act.getResources();
        return getString(KEY_CONNECT_TIMEOUT, res.getString(R.string.default_bt_connect_timeout), act);
    }

    /**
     * ＰＣ接続タイムアウトを保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setConnectTimeOut(Activity act,  String value) {
        setString(KEY_CONNECT_TIMEOUT, value, act);
    }

    /**
     * 操作放置時間を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getNoControlTime(Activity act) {
        Resources res = act.getResources();
        return getString(KEY_NO_CONTROL_TIME, res.getString(R.string.default_bt_no_control_time), act);
    }

    /**
     * 操作放置時間を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setNoControlTime(Activity act,  String value) {
        setString(KEY_NO_CONTROL_TIME, value, act);
    }

    /**
     * タグ読み込み時の電波強度を取得する
     * @param act    アクティビティ
     * @param pEach  個別機能
     * @return        設定値
     */
    public String getBTReadIntensity(Activity act, String pEach)  {
        Resources res = act.getResources();
        String strDefualtIntensity = "";

        //20230221 UPD Sato V2.3 キーエンス製DXシリーズ,iDataの場合も追加
        //20230410 UPD Sato v2.3.1 RS35の場合も追加
//        if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
        if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName) ||
            CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString().equals(Common.ConnectRFIDName) ||
            CommonConst.CONNECT_RFIDNAME.IDATA.getString().equals(Common.ConnectRFIDName) ||
            CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString().equals(Common.ConnectRFIDName)) {

            switch (pEach) {
                case KEY_EACH_INVENTORY:     //棚卸
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity);
                    break;
                case KEY_EACH_INVENTORY_L:
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_invL);
                    break;
                case KEY_EACH_INVENTORY_M:
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_invM);
                    break;
                case KEY_EACH_INVENTORY_S:
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_invS);
                    break;
                case KEY_EACH_SEARCH:       //探索
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_search);
                    break;
                case KEY_EACH_LEASE:        //貸出
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_lease);
                    break;
                case KEY_EACH_RETURN:       //返却
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_return);
                    break;
                case KEY_EACH_STORAGE:      //保管
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_storage);
                    break;
                case KEY_EACH_DISPOSE:      //除却
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_dispose);
                    break;
                case KEY_EACH_MATIN:      //持込照合
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_dispose);
                    break;
                case KEY_EACH_MATOUT:      //持帰照合
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_dispose);
                    break;
                case KEY_EACH_MATTEMP:      //仮置登録
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_dispose);
                    break;
                case KEY_EACH_MATRET:      //照合返却
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity_dispose);
                    break;
                default:
                    strDefualtIntensity = res.getString(R.string.denso_default_bt_read_intensity);
                    break;
            }
        } else {
            switch (pEach) {
                case KEY_EACH_INVENTORY:    //棚卸
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity);
                    break;
                case KEY_EACH_INVENTORY_L:
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_invL);
                    break;
                case KEY_EACH_INVENTORY_M:
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_invM);
                    break;
                case KEY_EACH_INVENTORY_S:
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_invS);
                    break;
                case KEY_EACH_SEARCH:       //探索
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_search);
                    break;
                case KEY_EACH_LEASE:        //貸出
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_lease);
                    break;
                case KEY_EACH_RETURN:       //返却
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_return);
                    break;
                case KEY_EACH_STORAGE:      //保管
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_storage);
                    break;
                case KEY_EACH_DISPOSE:      //除却
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_dispose);
                    break;
                case KEY_EACH_MATIN:      //持込照合
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_mat_in);
                    break;
                case KEY_EACH_MATOUT:      //持帰照合
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_mat_out);
                    break;
                case KEY_EACH_MATTEMP:      //仮置登録
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_mat_temp);
                    break;
                case KEY_EACH_MATRET:      //照合返却
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity_mat_ret);
                    break;
                default:
                    strDefualtIntensity = res.getString(R.string.default_bt_read_intensity);
                    break;
            }
        }

        return getString(KEY_BT_READ_INTENSITY + pEach, strDefualtIntensity, act);
    }

    /**
     * タグ読み込み時の電波強度を保存する
     * @param act    アクティビティ
     * @param pEach  個別機能
     * @param value  設定値
     */
    public void setBTReadIntensity(Activity act, String pEach, String value) {
        setString(KEY_BT_READ_INTENSITY + pEach, value, act);
    }

    /**
     * タグ読み込み時のセッション値を取得する
     * @param act    アクティビティ
     * @param pEach  個別機能
     * @return        設定値
     */
    public String getBTReadSession(Activity act, String pEach) {
        Resources res = act.getResources();
        String strInitValue = res.getString(R.string.default_bt_read_session);
        if (KEY_EACH_SEARCH.equals(pEach)) {
            strInitValue = "0";
        }
        return getString(KEY_BT_READ_SESSION + pEach, strInitValue, act);
    }

    /**
     * タグ読み込み時のセッション値を保存する
     * @param act    アクティビティ
     * @param pEach  個別機能
     * @param value  設定値
     */
    public void setBTReadSession(Activity act, String pEach, String value) {
        setString(KEY_BT_READ_SESSION + pEach, value, act);
    }

    /**
     * タグ読み込み時のＱバリュー値を取得する
     * @param act     アクティビティ
     * @param pEach   個別機能
     * @return         設定値
     */
    public String getBTReadQValue(Activity act, String pEach) {
        Resources res = act.getResources();
        return getString(KEY_BT_READ_QVALUE + pEach, res.getString(R.string.default_bt_read_Q_value), act);
    }

    /**
     * タグ読み込み時のＱバリュー値を保存する
     * @param act    アクティビティ
     * @param pEach  個別機能
     * @param value  設定値
     */
    public void setBTReadQValue( Activity act, String pEach, String value) {
        setString(KEY_BT_READ_QVALUE + pEach, value, act);
    }

    //20220316 ADD Suzuki V2.1 START
    /**
     * タグ読み込み時のAutoLinkProfileを保存する
     * @param act    アクティビティ
     * @param pEach  個別機能
     */
    public String getBTAutoLinkProfile(Activity act, String pEach) {
        Resources res = act.getResources();
        return getString(KEY_BT_READ_AUTO_LINK + pEach, res.getString(R.string.default_bt_auto_link), act);
    }

    /**
     * タグ読み込み時のAutoLinkProfileを保存する
     * @param act    アクティビティ
     * @param pEach  個別機能
     * @param value  設定値
     */
    public void setBTAutoLinkProfile(Activity act, String pEach, String value) {
        setString(KEY_BT_READ_AUTO_LINK + pEach, value, act);
    }
    //20220316 ADD Suzuki V2.1 END

    //20210918 ADD Takase V2.0 START
    /**
     * 対象拠点を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getTargetHub(Activity act) {
        return getString(KEY_TARGET_HUB, "", act);
    }

    /**
     * 対象拠点を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setTargetHub(Activity act,  String value) {
        setString(KEY_TARGET_HUB, value, act);
    }

    /**
     * 送信方法を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getTransfer(Activity act) {
        return getString(KEY_TRANSFER, CommonConst.TRANSFER_METHOD.WebAPI.getString(), act);
    }

    /**
     * 送信方法を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setTransfer(Activity act,  String value) {
        setString(KEY_TRANSFER, value, act);
    }
    //20210918 ADD Takase V2.0 END

    /**
     * ログイン担当者コードを取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getLoginEmployeeCode(Activity act) {
        return getString(KEY_LOGIN_EMPLOYEE_CODE, "", act);
    }

    /**
     * ログイン担当者コードを保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setLoginEmployeeCode(Activity act,  String value) {
        setString(KEY_LOGIN_EMPLOYEE_CODE, value, act);
    }

    /**
     * ログイン担当者名を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getLoginEmployeeName(Activity act) {
        return getString(KEY_LOGIN_EMPLOYEE_NAME, "", act);
    }

    /**
     * ログイン担当者名を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setLoginEmployeeName(Activity act,  String value) {
        setString(KEY_LOGIN_EMPLOYEE_NAME, value, act);
    }

    /**
     * ログイン担当者の本部フラグを取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getLoginHeadOffice(Activity act) {
        return getString(KEY_LOGIN_HEAD_OFFICE, "", act);
    }

    /**
     * ログイン担当者の本部フラグを保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setLoginHeadOffice(Activity act,  String value) {
        setString(KEY_LOGIN_HEAD_OFFICE, value, act);
    }

    /**
     * ログイン担当者権限区分を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getLoginAuth(Activity act) {
        return getString(KEY_LOGIN_AUTH, "", act);
    }

    /**
     * ログイン担当者権限区分を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setLoginAuth(Activity act,  String value) {
        setString(KEY_LOGIN_AUTH, value, act);
    }

    /**
     * ログイン担当者RFIDタグ拠点フラグを取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getLoginLocRfidFlg(Activity act) {
        return getString(KEY_LOGIN_LOC_RFID_FLG, "", act);
    }

    /**
     * ログイン担当者RFIDタグ拠点フラグを保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setLoginLocRfidFlg(Activity act,  String value) {
        setString(KEY_LOGIN_LOC_RFID_FLG, value, act);
    }

    /**
     * RFID使用区分[設定画面用]を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getRfidReaderuseSetting(Activity act) {
        return getString(RFID_READER_USE_SETTING, act.getString(R.string.default_bt_RFID_reader_use_setting), act);
    }

    /**
     * RFID使用区分[設定画面用]を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setRfidReaderuseSetting(Activity act,  String value) {
        setString(RFID_READER_USE_SETTING, value, act);
    }

    /**
     * 操作タイムアウト(分)を取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getLoginTimeoutSetting(Activity act) {
        return getString(LOGIN_TIMEOUT, "10", act);
    }

    /**
     * 操作タイムアウト(分)を保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setLoginTimeoutSetting(Activity act,  String value) {
        setString(LOGIN_TIMEOUT, value, act);
    }

    /**
     * SharedPreferencesから文字列値を取得する
     * @param key    SharedPreferencesキー
     * @param defVal 未設定時等の初期値
     * @param act    アクティビティ
     * @return 設定値
     */
    private static String getString(String key, String defVal, Activity act) {
        String val = PreferenceManager.getDefaultSharedPreferences(act).getString(key, defVal);
        Log.d("設定値取得", String.format("%s：%s", key, val));
        return val;
    }

    /**
     * SharedPreferencesへ文字列値を保存する
     * @param key SharedPreferencesキー
     * @param val 設定値
     * @param act アクティビティ
     */
    private static void setString(String key, String val, Activity act) {
        SharedPreferences.Editor editor = PreferenceManager.getDefaultSharedPreferences(act).edit();
        editor.putString(key, val);
        editor.apply();
        Log.i("設定値保存", String.format("%s：%s→%s", key, getString(key, val, act), val));
    }

    /**
     * 個別機能の３点設定を取得する
     * @param act    アクティビティ
     * @param pEach  個別機能
     */
    public void getBTReadSettingGet(Activity act, String pEach) {

        p_fncIntencity = Integer.parseInt(getBTReadIntensity(act, pEach));
        p_fncSession = getBTReadSession(act, pEach);
        p_fncQValue= (short) Integer.parseInt(getBTReadQValue(act, pEach));
    }

    /**
     * 電波強度を取得する
     * @return  電波強度
     */
    public String getIntencity() {
        return Integer.toString(p_fncIntencity);
    }

    /**
     * 電波強度変更チェック
     * @param act    アクティビティ
     * @param pTxtIntencity 電波強度表示View
     * @param pEach  個別機能
     * @return 変更あり(=1)/変更なし(=0)/無効(=-1)
     */
    public int ValidCheckNewIntencity(Activity act, Spinner pTxtIntencity, String pEach) {

        String _AfterIntencity = pTxtIntencity.getSelectedItem().toString();

        if(_AfterIntencity.length() <= 0) {
            //空白無効
            p_ErrorMessage = "電波強度を入力してください";
            return -1;
        }

        int tmpIntencity = Integer.parseInt(_AfterIntencity);
        if(p_fncIntencity == tmpIntencity) {
            //変更なし
            return 0;
        }

        //変更あり 電波強度保存
        setBTReadIntensity( act,  pEach, _AfterIntencity);
        p_fncIntencity = tmpIntencity;
        return 1;
    }


////////////////////////////// *** ログ設定
    /**
     * ログ出力有無を取得します。
     *
     * @param activity Activity
     * @return 設定値
     */
    public static String getLogOutput(Activity activity) {
        return getString(SETTING_LOG_OUTPUT, activity.getString(R.string.default_log_Output), activity);
    }

    /**
     * ログ出力有無を設定します。
     *
     * @param activity Activity
     * @param value　設定値
     */
    public static void setLogOutput(Activity activity, String value) {
        setString(SETTING_LOG_OUTPUT, value, activity);
    }

    /**
     * ログ出力レベルを取得します。
     *
     * @param activity Activity
     * @return 設定値
     */
    public static String getLogLevel(Activity activity) {
        return getString(SETTING_LOG_LEVEL, activity.getString(R.string.default_log_level), activity);
    }

    /**
     * ログ出力レベルを設定します。
     *
     * @param activity Activity
     * @param value　設定値
     */
    public static void setLogLevel(Activity activity, String value) {
        setString(SETTING_LOG_LEVEL, value, activity);
    }

    /**
     * ログローリング数を取得します。
     *
     * @param activity Activity
     * @return 設定値
     */
    public static String getLogRolling(Activity activity) {
        return getString(SETTING_LOG_ROLLING, activity.getString(R.string.default_log_rolling), activity);
    }

    /**
     * ログローリング数を設定します。
     *
     * @param activity Activity
     * @param value　設定値
     */
    public static void setLogRolling(Activity activity, String value) {
        setString(SETTING_LOG_ROLLING, value, activity);
    }

    /**
     * 最大ファイルサイズを取得します。
     *
     * @param activity Activity
     * @return 設定値
     */
    public static String getLogMaxSize(Activity activity) {
        return getString(SETTING_LOG_MAXSIZE, activity.getString(R.string.default_log_maxsize), activity);
    }

    /**
     * 最大ファイルサイズを設定します。
     *
     * @param activity Activity
     * @param value　設定値
     */
    public static void setLogMaxSize(Activity activity, String value) {
        setString(SETTING_LOG_MAXSIZE, value, activity);
    }
}
