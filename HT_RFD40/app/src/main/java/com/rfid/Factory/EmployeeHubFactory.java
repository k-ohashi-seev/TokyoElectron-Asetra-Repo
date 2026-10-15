package com.rfid.Factory;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConst;

import org.json.JSONArray;
import org.json.JSONException;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class EmployeeHubFactory {
    /**
     * 社員所属拠点マスタにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json) throws JSONException {
        Log.d("EmployeeHubInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.MASTER_EMPLOYEE_HUB);
            sql.append(" (");
            sql.append("  EMPLOYEE_CD,");         //社員コード
            sql.append("  HUB_LOC_L_CD");         //拠点ロケ大コード
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                    //社員コード
            sql.append(" ?");                     //拠点ロケ大コード
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("employeeCd"),    //社員コード
                    json.getJSONObject(i).getString("hubLocLCd")      //拠点ロケ大コード
            });

            Log.d("EmployeeHubInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 社員所属拠点マスタを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("EmployeeHubDelete", "START");

        //社員マスタの削除
        db.delete(CommonConst.MASTER_EMPLOYEE_HUB, null, null);

    }

    /**
     * 社員の拠点にHT.対象拠点が含まれているか
     * @param db
     * @param strEmployeeCode
     * @param HtHub
     * @return
     */
    public static boolean CheckHub(SQLiteDatabase db, String strEmployeeCode, String HtHub){
        int count = 0;

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  count(*) AS COUNT");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_EMPLOYEE_HUB);
        sql.append(" WHERE ");
        sql.append("  EMPLOYEE_CD = ? ");
        sql.append("  AND HUB_LOC_L_CD = ? ");

        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strEmployeeCode, HtHub});

        cursor.moveToFirst();
        if (cursor.getCount() > 0 ) {
            count = cursor.getInt(cursor.getColumnIndex("COUNT"));
        }

        return (count > 0);
    }

    /**
     * 拠点リストを取得する
     * @param db
     * @param strEmployeeCode
     * @return
     */
    public static String[] GetHubList(SQLiteDatabase db, String strEmployeeCode) {

        //拠点リスト
        List<String> hubList = new ArrayList<String>();
        
        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  HUB_LOC_L_CD");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_EMPLOYEE_HUB);
        sql.append(" WHERE ");
        sql.append("  EMPLOYEE_CD = ? ");

        //拠点検索
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strEmployeeCode});
        
        while (cursor.moveToNext()) {
        	hubList.add(cursor.getString(cursor.getColumnIndex("HUB_LOC_L_CD")));
        }
        return hubList.toArray(new String[hubList.size()]);
    }
}

