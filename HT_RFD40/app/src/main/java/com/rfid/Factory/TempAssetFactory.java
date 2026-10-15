package com.rfid.Factory;

import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.bean.EntryProcessDataDetail;

import org.json.JSONArray;
import org.json.JSONException;

public class TempAssetFactory {

    /**
     * 仮置き資産テーブルにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json,String prmConstId,String prmTempId) throws JSONException {
        Log.d("TempAssetInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.TABLE_TEMP_ASSET);
            sql.append(" (");
            sql.append("  ASSET_NO,");           //資産番号
            sql.append("  CONST_NO,");          //工事番号
            sql.append("  TEMP_ID");            //仮置きID
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");              //資産番号
            sql.append(" ?,");                   //工事番号
            sql.append(" ?");                   //仮置きID
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("no"),        //資産番号
                    prmConstId,                                         //工事番号
                    prmTempId                                         //仮置きID
            });

            Log.d("TempAssetInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 仮置き資産テーブルにデータを登録する
     * @param db
     */
    public static void Insert(SQLiteDatabase db, EntryProcessDataDetail value, String ConstRuctionNo, String TempId)  {
        Log.d("TempAssetInsert", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" INSERT");
        sql.append(" INTO ");
        sql.append(CommonConst.TABLE_TEMP_ASSET);
        sql.append(" (");
        sql.append("  ASSET_NO,");                 //資産番号
        sql.append("  CONST_NO,");                 //工事番号
        sql.append("  TEMP_ID");                   //仮置きID
        sql.append("  ) ");
        sql.append(" VALUES (");
        sql.append(" ?,");                         //資産番号
        sql.append(" ?,");                         //工事番号
        sql.append(" ?");                          //仮置きID
        sql.append(" )");

        //SQL発行
        db.execSQL(sql.toString(), new String[]{
                value.getAssetNo(),                 //資産番号
                ConstRuctionNo,                     //工事番号
                TempId                              //仮置きID
        });

    }

    /**
     * 仮置き資産テーブルにデータを更新する
     * @param db
     */
    public static void UpdateTempId(SQLiteDatabase db, String BeforeTempId, String NweTempId,String constNo)  {
        Log.d("TempAssetUpdateTempId", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" UPDATE ");
        sql.append(CommonConst.TABLE_TEMP_ASSET);
        sql.append(" SET");
        sql.append("  TEMP_ID = ? ");                 //仮置きID
        sql.append("  WHERE TEMP_ID = ?");
        sql.append("  AND CONST_NO = ?");

        //SQL発行
        //インデックス指定必要
        db.execSQL(sql.toString(), new Object[]{NweTempId, BeforeTempId, constNo});

    }

    /**
     * 仮置き資産テーブルを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("TempAssetDelete", "START");

        //権限マスタの削除
        db.delete(CommonConst.TABLE_TEMP_ASSET, null, null);

    }

    /**
     * 仮置き資産テーブルを削除する
     * @param db
     */
    public static void ConstNoTempIdDelete(SQLiteDatabase db,String tempId, String constNo) {
        Log.d("TempAssetDelete", "START");

        String whereCleause = "TEMP_ID = ? AND CONST_NO = ?";
        String[] whereArgs = { tempId, constNo };
        //仮置き資産マスタの削除
        db.delete(CommonConst.TABLE_TEMP_ASSET, whereCleause, whereArgs);

    }

}

