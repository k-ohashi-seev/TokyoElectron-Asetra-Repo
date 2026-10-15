package com.rfid.Factory;

import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.bean.EntryProcessDataDetail;

import org.json.JSONArray;
import org.json.JSONException;

public class ConstAssetFactory {

    /**
     * 工事資産テーブルにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json,String prmConstId) throws JSONException {
        Log.d("ConstAssetInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.TABLE_CONST_ASSET);
            sql.append(" (");
            sql.append("  ASSET_NO,");           //資産番号
            sql.append("  CONST_NO");          //工事番号
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                   //資産番号
            sql.append(" ?");                   //工事番号
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("no"),        //資産番号
                    prmConstId                                         //工事番号
            });

            Log.d("ConstAssetInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 貸出(照合)登録を行った際に工事資産テーブルに重複データ以外を登録する
     * @param db

     */
    public static void AseetInsert(SQLiteDatabase db, EntryProcessDataDetail value, String ConstRuctionNo) {
        Log.d("ConstAssetIgnoreInsert", "START");

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT ");
            sql.append(" INTO ");
            sql.append(CommonConst.TABLE_CONST_ASSET);
            sql.append(" (");
            sql.append("  ASSET_NO,");           //資産番号
            sql.append("  CONST_NO");          //工事番号
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                   //資産番号
            sql.append(" ?");                   //工事番号
            sql.append(" )");


        //SQL発行
        db.execSQL(sql.toString(), new String[]{
                value.getAssetNo(),                 //資産番号
                ConstRuctionNo,                     //工事番号
        });

    }


    /**
     * 工事資産テーブルを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("ConstAssetDelete", "START");

        //工事資産テーブルの削除
        db.delete(CommonConst.TABLE_CONST_ASSET, null, null);

    }

}

