package com.rfid.Factory;

import android.app.Activity;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.SystemSetting;

import org.json.JSONArray;
import org.json.JSONException;

public class WordFactory {

    /**
     * 文言マスタにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json) throws JSONException {
        Log.d("WordInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.MASTER_WORD);
            sql.append(" (");
            sql.append("  TOOL_DIV,");           //利用ツール区分
            sql.append("  WORD_DIV,");           //文言区分
            sql.append("  WORD_CD,");            //文言コード
            sql.append("  WORDING");             //文言
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                    //利用ツール区分
            sql.append(" ?,");                    //文言区分
            sql.append(" ?,");                    //文言コード
            sql.append(" ?");                     //文言
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    CommonConst.WORD_TOOL_DIV.H.toString(),                //利用ツール区分
                    json.getJSONObject(i).getString("wordDiv"),    //文言区分
                    json.getJSONObject(i).getString("wordCd"),     //文言コード
                    json.getJSONObject(i).getString("wording"),    //文言
            });

            Log.d("WordInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 文言マスタを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("WordDelete", "START");

        //文言マスタの削除
        db.delete(CommonConst.MASTER_WORD, null, null);

    }

    public static void CreateWordData(SQLiteDatabase db, Activity activity){
        boolean blnReturn = true;

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  TOOL_DIV");
        sql.append("  ,WORD_DIV");
        sql.append("  ,WORD_CD");
        sql.append("  ,WORDING");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_WORD);
        sql.append(" WHERE ");
        sql.append(" TOOL_DIV = ?");

        //ロケーション照合
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{"H"});

        while (cursor.moveToNext()) {
            String strToolDiv = cursor.getString(cursor.getColumnIndex("TOOL_DIV"));
            String strWordDiv = cursor.getString(cursor.getColumnIndex("WORD_DIV"));
            String strWordCd = cursor.getString(cursor.getColumnIndex("WORD_CD"));
            String strWording = cursor.getString(cursor.getColumnIndex("WORDING"));
            SystemSetting.setString(strToolDiv + strWordDiv + strWordCd, strWording, activity);
        }
    }
}
