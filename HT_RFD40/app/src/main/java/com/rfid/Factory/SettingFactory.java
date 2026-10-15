package com.rfid.Factory;

import android.app.Activity;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.text.TextUtils;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.bean.EntryProcessData;
import com.rfid.common.bean.EntryProcessDataDetail;
import com.rfid.setting.SettingValue;

import org.json.JSONArray;
import org.json.JSONException;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;

public class SettingFactory {

    /**
     * 設定マスタにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json) throws JSONException {
        Log.d("SettingInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.MASTER_SETTING);
            sql.append(" (");
            sql.append("  KEY_DIV,");           //KEY種別
            sql.append("  KEY1,");              //KEY1
            sql.append("  KEY2,");              //KEY2
            sql.append("  KEY3,");              //KEY3
            sql.append("  REMARK,");            //備考
            sql.append("  DATA1,");             //DATA1
            sql.append("  DATA2,");             //DATA2
            sql.append("  DATA3,");             //DATA3
            sql.append("  DATA4,");             //DATA4
            sql.append("  DATA5,");             //DATA5
            sql.append("  DATA6,");             //DATA6
            sql.append("  DATA7,");             //DATA7
            sql.append("  DATA8,");             //DATA8
            sql.append("  DATA9,");             //DATA9
            sql.append("  DATA10,");            //DATA10
            sql.append("  SPECIAL_DATA_DIV"); //特定データ区分
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                    //KEY種別
            sql.append(" ?,");                    //KEY1
            sql.append(" ?,");                    //KEY2
            sql.append(" ?,");                    //KEY3
            sql.append(" ?,");                    //備考
            sql.append(" ?,");                    //DATA1
            sql.append(" ?,");                    //DATA2
            sql.append(" ?,");                    //DATA3
            sql.append(" ?,");                    //DATA4
            sql.append(" ?,");                    //DATA5
            sql.append(" ?,");                    //DATA6
            sql.append(" ?,");                    //DATA7
            sql.append(" ?,");                    //DATA8
            sql.append(" ?,");                    //DATA9
            sql.append(" ?,");                    //DATA10
            sql.append(" ?");                     //特定データ区分
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("keyDiv"),          //KEY種別
                    json.getJSONObject(i).getString("key1"),            //KEY1
                    json.getJSONObject(i).getString("key2"),            //KEY2
                    json.getJSONObject(i).getString("key3"),            //KEY3
                    json.getJSONObject(i).getString("remark"),          //備考
                    json.getJSONObject(i).getString("data1"),           //DATA1
                    json.getJSONObject(i).getString("data2"),           //DATA2
                    json.getJSONObject(i).getString("data3"),           //DATA3
                    json.getJSONObject(i).getString("data4"),           //DATA4
                    json.getJSONObject(i).getString("data5"),           //DATA5
                    json.getJSONObject(i).getString("data6"),           //DATA6
                    json.getJSONObject(i).getString("data7"),           //DATA7
                    json.getJSONObject(i).getString("data8"),           //DATA8
                    json.getJSONObject(i).getString("data9"),           //DATA9
                    json.getJSONObject(i).getString("data10"),          //DATA10
                    json.getJSONObject(i).getString("specialDataDiv")  //特定データ区分
            });

            Log.d("SettingInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 設定マスタを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("SettingDelete", "START");

        //設定マスタの削除
        db.delete(CommonConst.MASTER_SETTING, null, null);

    }

    /**
     * 棚卸方法を取得する
     * @param db
     * @return
     */
    public static String GetInventoryMethod(SQLiteDatabase db) {
        Log.d("GetInventoryMethod", "START");

        String strInventoryMethod = CommonConst.INVENTORY_METHOD.NoneLocation.toString();

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                 //棚卸方法
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_INVENTORY_METHOD});

        while (cursor.moveToNext()) {
            //取得する
            strInventoryMethod = cursor.getString(cursor.getColumnIndex("DATA1"));        //棚卸方法
        }

        return strInventoryMethod;

    }

    /**
     * 資産読取媒体を取得する　
     * @param db
     * @return
     */
    //※もともとsqliteから"TG"もしくは"QR"を取得してその値をreturnしていたが、仕様変更によりRFIDリーダの使用有無の設定から"TG"もしくは"QR"をreturnする
    public static String GetReadingMedium(SQLiteDatabase db, Activity act) {
        Log.d("GetReadingMedium", "START");

        SettingValue C_Setting = new SettingValue();
        String strReadingMedium = CommonConst.READING_MEDIUM.QR.getString();
        boolean isRFIDReaderUse = Objects.equals(C_Setting.getRfidReaderUse(act), "1");

        if (isRFIDReaderUse) {
            strReadingMedium = CommonConst.READING_MEDIUM.TAG.getString();
        }

        return strReadingMedium;
    }

    /**
     * ロケ最小区分を取得する
     * @param db
     * @return
     */
    public static String[] GetLocationUsage(SQLiteDatabase db) {
        Log.d("GetLocationUsage", "START");

        String strInventoryMethod[] = new String[3];

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //ロケ大区分
        sql.append("  ,DATA2");                 //ロケ中区分
        sql.append("  ,DATA3");                 //ロケ小区分
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_LOCATION_USAGE});

        while (cursor.moveToNext()) {
            //取得する
            strInventoryMethod[0] = cursor.getString(cursor.getColumnIndex("DATA1"));        //ロケ大区分
            strInventoryMethod[1] = cursor.getString(cursor.getColumnIndex("DATA2"));        //ロケ中区分
            strInventoryMethod[2] = cursor.getString(cursor.getColumnIndex("DATA3"));        //ロケ大小区分
        }

        return strInventoryMethod;

    }

    /**
     * 棚卸用ロケ最小区分を取得する
     * @param db
     * @return
     */
    public static String[] GetLocationUsageInv(SQLiteDatabase db) {
        Log.d("GetLocationUsageInv", "START");

        String strInventoryMethod[] = new String[3];

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //ロケ大区分
        sql.append("  ,DATA2");                 //ロケ中区分
        sql.append("  ,DATA3");                 //ロケ小区分
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_LOCATION_USAGE_INV});

        while (cursor.moveToNext()) {
            //取得する
            strInventoryMethod[0] = cursor.getString(cursor.getColumnIndex("DATA1"));        //ロケ大区分
            strInventoryMethod[1] = cursor.getString(cursor.getColumnIndex("DATA2"));        //ロケ中区分
            strInventoryMethod[2] = cursor.getString(cursor.getColumnIndex("DATA3"));        //ロケ大小区分
        }

        return strInventoryMethod;

    }

    /**
     * タグ生成方法を取得する
     * @param db
     * @return
     */
    public static String GetAssetNoGeneration(SQLiteDatabase db) {
        Log.d("GetAssetNoGeneration", "START");

        String strAssetGeneration = CommonConst.ASSET_GENERATION.NoWrite.getString();

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //タグ生成方法
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_ASSET_ID_GENERATION});

        while (cursor.moveToNext()) {
            //取得する
            strAssetGeneration = cursor.getString(cursor.getColumnIndex("DATA1"));        //タグ生成方法
        }

        return strAssetGeneration;

    }

    /**
     * 資産ID発番を取得する
     * @param db
     * @return
     */
    public static String[] GetAutomaticGeneration(SQLiteDatabase db) {
        Log.d("GetAutomaticGeneration", "START");

        String strAutomatic[] = new String[3];

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //識別コード
        sql.append("  ,DATA2");                 //生成年
        sql.append("  ,DATA3");                 //連番
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_AUTOMATIC_GENERATION});

        while (cursor.moveToNext()) {
            //取得する
            strAutomatic[0] = cursor.getString(cursor.getColumnIndex("DATA1"));        //識別コード
            strAutomatic[1] = cursor.getString(cursor.getColumnIndex("DATA2"));        //生成年
            strAutomatic[2] = cursor.getString(cursor.getColumnIndex("DATA3"));        //連番
        }

        return strAutomatic;

    }

    /**
     * 資産ID発番を更新する
     * @param db
     * @param strYear
     * @param strNumber
     * @return
     */
    public static void UpdateAutomaticGeneration(SQLiteDatabase db, String strYear, String strNumber) {
        Log.d("UpdateAutomaticGeneration", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" UPDATE ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" SET DATA2 = ?");
        sql.append(" ,DATA3 = ?");
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        db.execSQL(sql.toString(), new String[]{strYear, strNumber, CommonConst.SETTEI_KEY_AUTOMATIC_GENERATION});
    }

    /**
     * 棚卸開始宣言を取得する
     * @param db
     * @return
     */
    public static String GetInventoryStart(SQLiteDatabase db) {
        Log.d("GetInventoryStart", "START");

        String strInventory = "";

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                 //棚卸方法
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_INVENTORY_START});

        while (cursor.moveToNext()) {
            //取得する
            strInventory = cursor.getString(cursor.getColumnIndex("DATA1"));        //棚卸方法
        }

        return strInventory;

    }

//20191206 Add Start Sasaki
    /**
     * 連続棚卸有無を取得する
     * @param db
     * @return
     */
    public static String GetInventoryContinue(SQLiteDatabase db) {
        Log.d("GetInventoryContinue", "START");

        String strInventory = CommonConst.INVENTORY_CONTINUE.NO.getString();

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                 //棚卸継続有無
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_INVENTORY_CONTINUE});

        while (cursor.moveToNext()) {
            //取得する
            strInventory = cursor.getString(cursor.getColumnIndex("DATA1"));        //棚卸継続有無
        }

        return strInventory;

    }
//20191206 Add End Sasaki

    //20210918 ADD Takase V2.0 START
    /**
     * 権限タイプを取得する
     * @param db
     * @return
     */
    public static String GetAuthorityType(SQLiteDatabase db) {
        Log.d("GetAuthorityType", "START");

        String strAuthType = CommonConst.AUTHORITY_TYPE.NORMAL.getString();

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                 //権限タイプ
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_AUTHORITY_TYPE});

        while (cursor.moveToNext()) {
            //取得する
            strAuthType = cursor.getString(cursor.getColumnIndex("DATA1"));        //権限タイプ
        }

        return strAuthType;
    }

    /**
     * 予約機能有無を取得する
     * @param db
     * @return
     */
    public static String GetReservationFunction(SQLiteDatabase db) {
        Log.d("GetReservationFunction", "START");

        String strInventory = CommonConst.INVENTORY_CONTINUE.NO.getString();

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                 //予約機能有無
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_RESERVATION_FUNCTION});

        while (cursor.moveToNext()) {
            //取得する
            strInventory = cursor.getString(cursor.getColumnIndex("DATA1"));        //予約機能有無
        }

        return strInventory;
    }
    //20210918 ADD Takase V2.0 END

    /**
     * 予約機能有無を取得する  20230221 ADD Sato v2.3
     * @param db
     * @return
     */
    public static String GetAssetRegistrationLimit(SQLiteDatabase db) {
        Log.d("GetAssetRegistrationLimit", "START");

        String strAssetRegLimit = "0";          //0は上限指定なし

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //資産登録上限数
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_ASSET_REGISTRATION_LIMIT});

        while (cursor.moveToNext()) {
            //取得する
            strAssetRegLimit = cursor.getString(cursor.getColumnIndex("DATA1"));        //資産登録上限数
        }

        return strAssetRegLimit;
    }

    /**
     * HT貸出時のアラート表示有無を取得する  20230830 ADD Kawamoto v2.4.1
     * @param db
     * @return
     */
    public static String[] GetLeaseAlertDisplay(SQLiteDatabase db) {
        Log.d("GetLeaseAlertDisplay", "START");

        String strLeaseAlert[]= {CommonConst.LEASE_ALERT_DISPLAY.NO.getString(),
                                 CommonConst.LEASE_DEADLONE.SYSTEM_DATE.getString()};

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //HT貸出時のアラート表示有無
        sql.append(" ,DATA2");                  //期限日判定項目
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_LEASE_ALERT_DISPLAY});

        if (cursor.moveToNext()) {
            //取得する
            strLeaseAlert[0] = cursor.getString(cursor.getColumnIndex("DATA1"));        //HT貸出時のアラート表示有無
            strLeaseAlert[1] = cursor.getString(cursor.getColumnIndex("DATA2"));        //期限日判定項目
        }
        cursor.close();

        return strLeaseAlert;
    }

    /**
     * HT返却時のアラート表示有無を取得する  20230830 ADD Kawamoto v2.4.1
     * @param db
     * @return
     */
    public static String GetReturnAlertDisplay(SQLiteDatabase db) {
        Log.d("GetReturnAlertDisplay", "START");

        String strReturnAlertDisplay = CommonConst.RETURN_ALERT_DISPLAY.NO.getString();

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //HT返却時のアラート表示有無
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_RETURN_ALERT_DISPLAY});

        if (cursor.moveToNext()) {
            //取得する
            strReturnAlertDisplay = cursor.getString(cursor.getColumnIndex("DATA1"));        //HT返却時のアラート表示有無
        }
        cursor.close();

        return strReturnAlertDisplay;
    }

    /**
     * 連続貸出（HTアプリ）実施有無を取得する  20230830 ADD Kawamoto v2.4.1
     * @param db
     * @return
     */
    public static String GetLendingContinuous(SQLiteDatabase db) {
        Log.d("GetLendingContinuous", "START");

        String strLendingContinuous = CommonConst.LENDING_CONTINUOUS.NO.getString();

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA2");                  //連続貸出（HTアプリ）実施有無
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_LENDING_CONTINUOUS});

        if (cursor.moveToNext()) {
            //取得する
            strLendingContinuous = cursor.getString(cursor.getColumnIndex("DATA2"));        //連続貸出（HTアプリ）実施有無
        }
        cursor.close();

        return strLendingContinuous;
    }

    /**
     * 資産表示用の色を取得する
     * @param db DBを操作するためのインスタンス
     * @param key1 キー1
     */
    public static String GetAssetColor(SQLiteDatabase db, String key1) {
        Log.d("GetAssetColor", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");
        sql.append(" AND KEY1 = ? ");
        sql.append(" AND KEY2 = '**' ");
        sql.append(" AND KEY3 = '**' ");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_LEASE_RETURN_ASSET_COLOR_HT, key1});

        String strRGBHex = "";
        if (cursor.moveToNext()) {
            //取得する
            strRGBHex = cursor.getString(cursor.getColumnIndex("DATA1"));
        }
        cursor.close();

        if (TextUtils.isEmpty(strRGBHex)) {
            //データが存在しない場合は赤色を返却
            return "#FF0000";
        }

        if (!strRGBHex.startsWith("#")) {
            return "#" + strRGBHex;
        }

        return strRGBHex;
    }

    /**
     * 返却予定日を自動でセットするかどうかの設定を取得
     * @param db
     * @return
     */
    public static String[] GetLeaseSetSysdateHT(SQLiteDatabase db) {
        Log.d("GetLeaseSetSysdateHT", "START");

        String retArray[]= {CommonConst.LEASE_SET_SYSDATE_HT.NO.getString(),"3650"};

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //HT貸出時のアラート表示有無
        sql.append(" ,DATA2");                  //期限日判定項目
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");
        sql.append(" AND KEY1 = '**' ");
        sql.append(" AND KEY2 = '**' ");
        sql.append(" AND KEY3 = '**' ");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_LEASE_SET_SYSDATE_HT});

        if (cursor.moveToNext()) {
            //取得する
            retArray[0] = cursor.getString(cursor.getColumnIndex("DATA1"));        // する/しない
            retArray[1] = cursor.getString(cursor.getColumnIndex("DATA2"));        //　日数
        }
        cursor.close();

        return retArray;
    }

    /**
     * 貸出情報表示設定を取得
     * @param db
     * @return
     */
    public static String GetLendingDspInfo(SQLiteDatabase db) {
        Log.d("GetLendingDspInfo", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");
        sql.append(" AND KEY1 = '**' ");
        sql.append(" AND KEY2 = '**' ");
        sql.append(" AND KEY3 = '**' ");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_LEASE_DISPLAY_INFO_HT});

        String lendingDspInfo = "0";
        if (cursor.moveToNext()) {
            //取得する
            lendingDspInfo = cursor.getString(cursor.getColumnIndex("DATA1"));
        }
        cursor.close();

        return lendingDspInfo;
    }

    /**
     * 返却情報表示設定を取得
     * @param db
     * @return
     */
    public static String GetLReturnDspInfo(SQLiteDatabase db) {
        Log.d("GetLReturnDspInfo", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");
        sql.append(" AND KEY1 = '**' ");
        sql.append(" AND KEY2 = '**' ");
        sql.append(" AND KEY3 = '**' ");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_RETURN_DISPLAY_INFO_HT});

        String returnDspInfo = "0";
        if (cursor.moveToNext()) {
            //取得する
            returnDspInfo = cursor.getString(cursor.getColumnIndex("DATA1"));
        }
        cursor.close();

        return returnDspInfo;
    }

    /**
     * 一覧画面の背景色を取得
     * @param db SQLite操作インスタンス
     * @param key1 キー1
     * @return
     */
    public static String GetSummaryBackgroundColor(SQLiteDatabase db, String key1) {
        Log.d("GetSummaryBackgroundColor", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");
        sql.append(" AND KEY1 = ? ");
        sql.append(" AND KEY2 = '**' ");
        sql.append(" AND KEY3 = '**' ");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_SUMMARY_BACKGROUND_COLOR_HT, key1});

        String strRGBHex = "";
        if (cursor.moveToNext()) {
            //取得する
            strRGBHex = cursor.getString(cursor.getColumnIndex("DATA1"));
        }
        cursor.close();

        if (TextUtils.isEmpty(strRGBHex)) {
            //データが存在しない場合は通常色(緑)を返却
            return "#6AB3B3";
        }

        if (!strRGBHex.startsWith("#")) {
            return "#" + strRGBHex;
        }

        return strRGBHex;
    }

    /**
     * 照合機能の設定を取得
     * @param db
     * @return
     */
    public static String GetMatchingFunction(SQLiteDatabase db) {
        Log.d("GetMatchingFunction", "START");

        String strMatchingFunction = CommonConst.MATCHING_FUNCTION.NO.getString();

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //HT返却時のアラート表示有無
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_MATCHING_FUNCTION});

        if (cursor.moveToNext()) {
            //取得する
            strMatchingFunction = cursor.getString(cursor.getColumnIndex("DATA1"));        //HT返却時のアラート表示有無
        }
        cursor.close();

        return strMatchingFunction;
    }

    /**
     * 照合予約機能の設定を取得
     * @param db
     * @return
     */
    public static String GetMatchingReserveFunction(SQLiteDatabase db) {
        Log.d("GetMatchingReserveFunction", "START");

        String strMatchingReserveFunction = CommonConst.MATCHING_FUNCTION.NO.getString();

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //HT返却時のアラート表示有無
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_MATCHING_RESERVE_FUNCTION});

        if (cursor.moveToNext()) {
            //取得する
            strMatchingReserveFunction = cursor.getString(cursor.getColumnIndex("DATA1"));        //HT返却時のアラート表示有無
        }
        cursor.close();

        return strMatchingReserveFunction;
    }

    /**
     * 仮置き機能の設定を取得
     * @param db
     * @return
     */
    public static String GetTemporaryLeaveFunction(SQLiteDatabase db) {
        Log.d("GetTemporaryLeaveFunction", "START");

        String strTemporaryLeaveFunction = CommonConst.TEMPORARYLEAVE_FUNCTION.NO.getString();

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1");                  //HT返却時のアラート表示有無
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_TEMPORARYLEAVE_FUNCTION});

        if (cursor.moveToNext()) {
            //取得する
            strTemporaryLeaveFunction = cursor.getString(cursor.getColumnIndex("DATA1"));        //HT返却時のアラート表示有無
        }
        cursor.close();

        return strTemporaryLeaveFunction;
    }


    /**
     * 仮置き機能の設定を取得@[
     */
    public static String GetTemporaryInputItem() {
        Log.d("GetTemporaryInputItem", "START");

        //String strTemporaryInputItem = CommonConst.SETTEI_KEY_TEMPORARYINPUTITEM_FUNCTION;

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  *");                  //HT返却時のアラート表示有無
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");

        return sql.toString();
    }

    /**
     * 仮置きのコンポーネント設定を取得
     * @param db
     * @return
     */
    public static String[][] GetTemporaryInputItem(SQLiteDatabase db) {
        Log.d("GetTemporaryInputItem", "START");

        String[][] strTemporaryInputItem = new String[7][6];

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1 ");
        sql.append("  ,DATA2 ");
        sql.append("  ,DATA3 ");
        sql.append("  ,DATA4 ");
        sql.append("  ,DATA5 ");
        sql.append("  ,DATA6 ");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");
        sql.append(" ORDER BY KEY_DIV ");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_TEMPORARY_INPUT_ITEM});
        int i = 0;
        while (cursor.moveToNext()) {
            //取得する
            strTemporaryInputItem[i][0] = cursor.getString(cursor.getColumnIndex("DATA1"));        //表示、非表示
            strTemporaryInputItem[i][1] = cursor.getString(cursor.getColumnIndex("DATA2"));        //テキスト、コンボボックス
            strTemporaryInputItem[i][2] = cursor.getString(cursor.getColumnIndex("DATA3"));        //固定初期値
            strTemporaryInputItem[i][3] = cursor.getString(cursor.getColumnIndex("DATA4"));        //単位
            strTemporaryInputItem[i][4] = cursor.getString(cursor.getColumnIndex("DATA5"));        //コンボボックス設定値
            strTemporaryInputItem[i][5] = cursor.getString(cursor.getColumnIndex("DATA6"));        //タイトル

            ++i;
        }
        cursor.close();

        return strTemporaryInputItem;
    }

    /**
     * 仮置きのリスト項目を取得
     * @param db
     * @return
     */
    public static HashMap GetTemporarySelectItem(SQLiteDatabase db,String keyDiv,String initial) {
        Log.d("GetTemporaryInputItem", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1 ");
        sql.append("  ,DATA2 ");
        sql.append("  ,DATA3 ");
        sql.append("  ,DATA4 ");
        sql.append("  ,DATA5 ");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");
        sql.append(" ORDER BY KEY1 ");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{keyDiv});
        HashMap<String,String> item = new HashMap<>();
        HashMap<String,String> subItem = new HashMap<>();
        //固定初期値データを加えるか
        boolean isInitial = true;

        while (cursor.moveToNext()) {
            if (cursor.getString(cursor.getColumnIndex("DATA1")).equals(initial)){
                isInitial = false;
            }
            subItem.put(cursor.getString(cursor.getColumnIndex("DATA2")),cursor.getString(cursor.getColumnIndex("DATA1")));
        }
        //初期値がある場合セット
        if(isInitial == true){
            if(!initial.equals("") ){
                item.put("",initial);
            }
        }
        //保存しておいたデータを追加
        for (Map.Entry<String, String> entry : subItem.entrySet()) {
            item.put(entry.getKey(),entry.getValue());
        }
        cursor.close();

        return item;
    }

    public static HashMap GetConstRuctionSelectItem(SQLiteDatabase db,String keyDiv) {
        Log.d("GetConstRuctionInputItem", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1 ");
        sql.append("  ,DATA2 ");
        sql.append("  ,DATA3 ");
        sql.append("  ,DATA4 ");
        sql.append("  ,DATA5 ");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");
        sql.append(" ORDER BY KEY1 + 0");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{keyDiv});
        HashMap<String,String> item = new HashMap<>();
        while (cursor.moveToNext()) {
            item.put(cursor.getString(cursor.getColumnIndex("DATA2")),cursor.getString(cursor.getColumnIndex("DATA1")));               //DATA2をキー、DATA1を値に設定
        }
        cursor.close();

        return item;
    }

    /**
     * 工事情報のコンポーネント設定を取得
     * @param db
     * @return
     */
    public static String[][] GetConstructionInputItem(SQLiteDatabase db) {
        Log.d("GetConstructionInputItem", "START");

        String[][] strConstRuctionInputItem = new String[20][3];

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  DATA1 ");
        sql.append("  ,DATA2 ");
        sql.append("  ,DATA3 ");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_SETTING);
        sql.append(" WHERE KEY_DIV = ?");
        sql.append(" ORDER BY KEY1 + 0");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.SETTEI_KEY_CONSTRUCTION_INPUT_ITEM});
        int i = 0;
        while (cursor.moveToNext()) {
            //取得する
            strConstRuctionInputItem[i][0] = cursor.getString(cursor.getColumnIndex("DATA1"));        //テキスト、コンボボックス
            strConstRuctionInputItem[i][1] = cursor.getString(cursor.getColumnIndex("DATA2"));        //固定初期値
            strConstRuctionInputItem[i][2] = cursor.getString(cursor.getColumnIndex("DATA3"));        //リスト設定値

            ++i;
        }
        cursor.close();

        return strConstRuctionInputItem;
    }

}
