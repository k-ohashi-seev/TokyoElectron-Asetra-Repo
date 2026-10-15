package com.rfid.Factory;

import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.bean.EntryProcessDataDetail;

import org.json.JSONArray;
import org.json.JSONException;

public class ReserveFactory {

    /**
     * 予約テーブルにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json) throws JSONException {
        Log.d("ReserveInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.TABLE_RESERVE);
            sql.append(" (");
            sql.append("  RESERVE_ID,");         //予約番号
            sql.append("  CONST_NO,");           //工事番号
            sql.append("  CONST_CONTENTS,");     //工事内容
            sql.append("  EMP,");                //担当者社員コード
            sql.append("  RES_ST_DT,");          //実質貸出開始日
            sql.append("  RES_END_DT,");         //実質貸出終了日
            sql.append("  ASEET_NO");            //資産番号
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                  //予約番号
            sql.append(" ?,");                  //工事番号
            sql.append(" ?,");                  //工事内容
            sql.append(" ?,");                  //担当者社員コード
            sql.append(" ?,");                  //実質貸出開始日
            sql.append(" ?,");                  //実質貸出終了日
            sql.append(" ?");                   //資産番号
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("reserveId"),       //予約番号
                    json.getJSONObject(i).getString("constNo"),         //工事番号
                    json.getJSONObject(i).getString("contents"),        //工事内容
                    json.getJSONObject(i).getString("emp"),             //担当者社員コード
                    json.getJSONObject(i).getString("resStDt"),         //実質貸出開始日
                    json.getJSONObject(i).getString("resEndDt"),        //実質貸出終了日
                    json.getJSONObject(i).getString("no")               //資産番号

            });


            Log.d("ReserveInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 仮置きテーブルを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("ReserveDelete", "START");

        //権限マスタの削除
        db.delete(CommonConst.TABLE_RESERVE, null, null);

    }

}

