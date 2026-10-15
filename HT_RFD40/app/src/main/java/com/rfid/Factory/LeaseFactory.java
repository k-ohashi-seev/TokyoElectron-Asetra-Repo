package com.rfid.Factory;

import android.app.Activity;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.bean.EntryProcessDataDetail;

import org.json.JSONArray;
import org.json.JSONException;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

public class LeaseFactory implements Serializable {

    private HashMap<String, EntryProcessDataDetail> _mapOK = new HashMap<String, EntryProcessDataDetail>();          //貸出OKデータ
    private List<String> _listAssetIDRead = new ArrayList<String>();                       //資産ID読込データ
    private List<String> _listAssetNoRead = new ArrayList<String>();                        //資産No読込データ
    private String _LeaseDestination = "";                                             //貸出先コード
    private String _LeaseDestinationName = "";                                         //貸出先名
    private String _LeaseEmployee = "";                                                 //貸出担当コード
    private String _LeaseReturn = "";                                                   //貸出返却予定日


    /**
     * 貸出先マスタにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json) throws JSONException {
        Log.d("LeaseInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.MASTER_LEASE_PLACE);
            sql.append(" (");
            sql.append("  LEASE_PLACE_CD,");      //貸出先コード
            sql.append("  LEASE_PLACE_NAME");     //貸出先名
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                      //貸出先コード
            sql.append(" ?");                       //貸出先名
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("leasePlaceCd"),    //貸出先コード
                    json.getJSONObject(i).getString("leasePlaceName")   //貸出先名
            });

            Log.d("LeaseInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 貸出先マスタを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("LeaseDelete", "START");

        //貸出先マスタの削除
        db.delete(CommonConst.MASTER_LEASE_PLACE, null, null);

    }

    /**
     * 貸出先マスタ検索SQL
     * @return SQL
     */
    public static String Select(){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT");
        sql.append(" LEASE_PLACE_CD");
        sql.append(" ,LEASE_PLACE_NAME");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_LEASE_PLACE);
        sql.append(" WHERE ");
        sql.append(" LEASE_PLACE_CD Like ? ");
        sql.append(" ORDER BY LEASE_PLACE_CD");
        return sql.toString();
    }

    /**
     * 貸出先名称を取得する
     * @param db
     * @param strLeaseCode
     * @return
     */
    public static String GetLeaseDestinationName(SQLiteDatabase db, String strLeaseCode){
        boolean blnReturn = true;
        String strName = "";

        if (strLeaseCode == "") {
            return strName;
        }

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  LEASE_PLACE_NAME");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_LEASE_PLACE);
        sql.append(" WHERE LEASE_PLACE_CD = ?");

        //貸出先検索
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strLeaseCode});

        cursor.moveToFirst();
        if (cursor.getCount() > 0 ) {
            strName = cursor.getString(cursor.getColumnIndex("LEASE_PLACE_NAME"));
        }

        return strName;
    }

    /**
     * 貸出先名称を取得する
     * @param activity
     * @param strLeaseCode
     * @return
     */
    public static String GetLeaseDestinationName(Activity activity, String strLeaseCode){
        SQLiteDatabase db = null;
        try {

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(activity);
            db = dbHelper.getWritableDatabase();

            return GetLeaseDestinationName(db, strLeaseCode);

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }
}
