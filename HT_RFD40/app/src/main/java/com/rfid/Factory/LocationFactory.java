package com.rfid.Factory;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;

import org.json.JSONArray;
import org.json.JSONException;

public class LocationFactory {

    /**
     * ロケーションマスタにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json) throws JSONException {
        Log.d("LocationInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.MASTER_LOCATION);
            sql.append(" (");
            sql.append("  LOC_DIV,");           //ロケ区分
            sql.append("  LOC_L_CD,");          //ロケ大区分コード
            sql.append("  LOC_M_CD,");          //ロケ中区分コード
            sql.append("  LOC_S_CD,");          //ロケ小区分コード
            sql.append("  LOC_NAME,");          //ロケ名称
            sql.append("  LOC_RFID_FLG");       //RFIDタグ拠点
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                    //ロケ区分
            sql.append(" ?,");                    //ロケ大区分コード
            sql.append(" ?,");                    //ロケ中区分コード
            sql.append(" ?,");                    //ロケ小区分コード
            sql.append(" ?,");                    //ロケ名称
            sql.append(" ?");                     //RFIDタグ拠点
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("locDiv"),     //ロケ区分
                    json.getJSONObject(i).getString("locLCd"),     //ロケ大区分コード
                    json.getJSONObject(i).getString("locMCd"),     //ロケ中区分コード
                    json.getJSONObject(i).getString("locSCd"),     //ロケ小区分コード
                    json.getJSONObject(i).getString("locName"),    //ロケ名称
                    json.getJSONObject(i).getString("locRFIDFlg")  //RFIDタグ拠点
            });

            Log.d("LocationInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * ロケーションマスタを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("LocationDelete", "START");

        //ロケーションマスタの削除
        db.delete(CommonConst.MASTER_LOCATION, null, null);

    }

    @SuppressLint("Range")
    public static String GetLocRfidFlg(SQLiteDatabase db, String strLLoc){
        if (strLLoc.isEmpty()){return strLLoc;}
        String locRfidFlg = "";

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  LOC_RFID_FLG");
        sql.append(" FROM M_LOCATION");
        sql.append(" WHERE LOC_L_CD = ?");
        sql.append("  AND LOC_DIV = ?");

        //ロケーション照合
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strLLoc, CommonConst.LOCATION_DIV.L.toString()});

        cursor.moveToFirst();
        if (cursor.getCount() > 0 ) {
            locRfidFlg = cursor.getString(cursor.getColumnIndex("LOC_RFID_FLG"));
        }

        return locRfidFlg;
    }

    /**
     * ロケコードが存在するかチェックする
     * @param db
     * @param strLLoc
     * @param strMLoc
     * @param strSLoc
     * @return
     */
    public static boolean ExistLocation(SQLiteDatabase db, String strLLoc, String strMLoc, String strSLoc){
        boolean blnReturn = true;

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  count(*) AS COUNT");
        sql.append(" FROM M_LOCATION");
        sql.append(" WHERE 1=1 ");

        String[] strLoc = null;

        if (!"".equals(strLLoc) && !"".equals(strMLoc) &&  !"".equals(strSLoc)) {
            //大・中・小入力あり
            sql.append("  AND LOC_L_CD = ?");
            sql.append("  AND LOC_M_CD = ?");
            sql.append("  AND LOC_S_CD = ?");
            sql.append("  AND LOC_DIV = ?");
            strLoc = new String[]{strLLoc, strMLoc, strSLoc, CommonConst.LOCATION_DIV.S.getString()};
        } else if (!"".equals(strLLoc) && !"".equals(strMLoc)) {
            //大・中入力あり
            sql.append("  AND LOC_L_CD = ?");
            sql.append("  AND LOC_M_CD = ?");
            sql.append("  AND LOC_DIV = ?");
            strLoc = new String[]{strLLoc, strMLoc, CommonConst.LOCATION_DIV.M.getString()};
        } else if (!"".equals(strLLoc)) {
            //大入力あり
            //20220316 ADD Suzuki START
            if (!"".equals(strSLoc)){
                //小入力あり
                return false;
            }
            //20220316 ADD Suzuki END
            sql.append("  AND LOC_L_CD = ?");
            sql.append("  AND LOC_DIV = ?");
            strLoc = new String[]{strLLoc, CommonConst.LOCATION_DIV.L.getString()};
        }

        //ロケーション照合
        Cursor cursor = db.rawQuery(sql.toString(), strLoc);

        cursor.moveToFirst();
        if (((Cursor) cursor).getInt(0) <= 0) {
            blnReturn = false;
        }

        return blnReturn;
    }

    /**
     * 共通拠点のロケ大コードを取得する
     * @return
     */
    public static String GetShareLocationLCode (){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  LOC_L_CD");
        sql.append(" FROM M_LOCATION");
        sql.append(" WHERE LOC_SHARE_FLG = '" + CommonConst.LOC_SHARE_FLG.SHARED.getString() + "'");

        return sql.toString();
    }

    /**
     * ロケ大区分名称を取得する
     * @param db
     * @param strLLoc
     * @return
     */
    @SuppressLint("Range")
    public static String GetLocationLName (SQLiteDatabase db, String strLLoc){
        boolean blnReturn = true;
        String strName = "";

        if ("".equals(strLLoc)) {
            return strName;
        }

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  LOC_NAME");
        sql.append(" FROM M_LOCATION");
        sql.append(" WHERE LOC_L_CD = ?");
        sql.append("  AND LOC_DIV = ?");

        //ロケーション照合
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strLLoc, CommonConst.LOCATION_DIV.L.toString()});

        cursor.moveToFirst();
        if (cursor.getCount() > 0 ) {
            strName = cursor.getString(cursor.getColumnIndex("LOC_NAME"));
        }

        return strName;
    }

    /**
     * ロケ大区分名称を取得する
     * @param activity
     * @param strLLoc
     * @return
     */
    public static String GetLocationLName (Activity activity, String strLLoc){
        SQLiteDatabase db = null;
        try {

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(activity);
            db = dbHelper.getWritableDatabase();

            return GetLocationLName(db, strLLoc);

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * ロケ中区分名称を取得する
     * @param db
     * @param strLLoc
     * @param strMLoc
     * @return
     */
    @SuppressLint("Range")
    public static String GetLocationMName (SQLiteDatabase db, String strLLoc, String strMLoc){
        boolean blnReturn = true;
        String strName = "";

        if ("".equals(strMLoc)) {
            return strName;
        }

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  LOC_NAME");
        sql.append(" FROM M_LOCATION");
        sql.append(" WHERE LOC_L_CD = ?");
        sql.append("  AND LOC_M_CD = ?");
        sql.append("  AND LOC_DIV = ?");

        //ロケーション照合
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strLLoc, strMLoc, CommonConst.LOCATION_DIV.M.toString()});

        cursor.moveToFirst();
        if (cursor.getCount() > 0 ) {
            strName = cursor.getString(cursor.getColumnIndex("LOC_NAME"));
        }

        return strName;
    }

    /**
     * ロケ中区分名称を取得する
     * @param activity
     * @param strLLoc
     * @param strMLoc
     * @return
     */
    public static String GetLocationMName (Activity activity, String strLLoc, String strMLoc){
        SQLiteDatabase db = null;
        try {

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(activity);
            db = dbHelper.getWritableDatabase();

            return GetLocationMName(db, strLLoc, strMLoc);

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * ロケ小区分名称を取得する
     * @param db
     * @param strLLoc
     * @param strMLoc
     * @param strSLoc
     * @return
     */
    @SuppressLint("Range")
    public static String GetLocationSName (SQLiteDatabase db, String strLLoc, String strMLoc, String strSLoc){
        boolean blnReturn = true;
        String strName = "";

        if ("".equals(strSLoc)) {
            return strName;
        }

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  LOC_NAME");
        sql.append(" FROM M_LOCATION");
        sql.append(" WHERE LOC_L_CD = ?");
        sql.append("  AND LOC_M_CD = ?");
        sql.append("  AND LOC_S_CD = ?");
        sql.append("  AND LOC_DIV = ?");

        //ロケーション照合
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strLLoc, strMLoc, strSLoc, CommonConst.LOCATION_DIV.S.toString()});

        cursor.moveToFirst();
        if (cursor.getCount() > 0 ) {
            strName = cursor.getString(cursor.getColumnIndex("LOC_NAME"));
        }

        return strName;
    }

    /**
     * ロケ小区分名称を取得する
     * @param activity
     * @param strLLoc
     * @param strMLoc
     * @param strSLoc
     * @return
     */
    public static String GetLocationSName (Activity activity, String strLLoc, String strMLoc, String strSLoc){
        SQLiteDatabase db = null;
        try {

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(activity);
            db = dbHelper.getWritableDatabase();

            return GetLocationSName(db, strLLoc, strMLoc, strSLoc);

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * ロケ大区分SQL
     * @return SQL
     */
    public static String createSQLData(){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT");
        sql.append("  LOC_L_CD AS LOC_CD");
        sql.append("  , LOC_NAME AS LOC_NAME");
        sql.append(" FROM M_LOCATION");
        sql.append("  WHERE LOC_DIV = ?");
        sql.append("  AND LOC_L_CD LIKE ?");
        sql.append(" ORDER BY LOC_L_CD");
        return sql.toString();
    }

    //20210918 ADD Takase V2.0 START
    /**
     * ロケ大区分SQL(自拠点のみ)
     * @return
     */
    public static String createSQLDataHub(String[] hubList){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT");
        sql.append("  LOC_L_CD AS LOC_CD");
        sql.append("  , LOC_NAME AS LOC_NAME");
        sql.append(" FROM");
        sql.append("  (SELECT ");
        sql.append("    * ");
        sql.append("   FROM ").append(CommonConst.MASTER_LOCATION);
        sql.append("   WHERE");
        sql.append("    1 <> 1");
        for (int i = 0; i < hubList.length; i++) {
            sql.append(" OR LOC_L_CD = '").append(hubList[i]).append("'");
        }
        sql.append("  )");
        sql.append(" WHERE LOC_DIV = ?");
        sql.append(" AND LOC_L_CD LIKE ?");
        sql.append(" ORDER BY LOC_L_CD");
        return sql.toString();
    }
    //20210918 ADD Takase V2.0 END

    /**
     * ロケ中区分SQL
     * @return SQL
     */
    public static String createSQLDataL(){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT");
        sql.append("  LOC_M_CD AS LOC_CD");
        sql.append("  , LOC_NAME AS LOC_NAME");
        sql.append(" FROM M_LOCATION");
        sql.append("  WHERE LOC_DIV = ?");
        sql.append("  AND LOC_L_CD = ?");
        sql.append("  AND LOC_M_CD LIKE ?");
        sql.append(" ORDER BY LOC_M_CD");
        return sql.toString();
    }

    /**
     * ロケ小区分SQL
     * @return SQL
     */
    public static String createSQLDataM(){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT");
        sql.append("  LOC_S_CD AS LOC_CD");
        sql.append("  , LOC_NAME AS LOC_NAME");
        sql.append(" FROM M_LOCATION");
        sql.append("  WHERE LOC_DIV = ?");
        sql.append("  AND LOC_L_CD = ?");
        sql.append("  AND LOC_M_CD = ?");
        sql.append("  AND LOC_S_CD LIKE ?");
        sql.append(" ORDER BY LOC_S_CD");
        return sql.toString();
    }

}
