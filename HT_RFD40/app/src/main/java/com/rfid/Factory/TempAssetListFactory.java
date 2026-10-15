package com.rfid.Factory;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.TempAssetListData;
import com.rfid.common.TempListData;
import com.rfid.common.bean.EntryProcessDataDetail;

import java.util.ArrayList;

public class TempAssetListFactory {

    /**
     * 仮置き資産リストテーブルにデータを登録する
     * @param db
     */
    public static void Insert(SQLiteDatabase db, EntryProcessDataDetail value,String ConstRuctionNo,String TempId)  {
        Log.d("TempAssetListInsert", "START");

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.TABLE_TEMP_ASSET_LIST);
            sql.append(" (");
            sql.append("  PROCESS_LOG_ID,");           //処理履歴
            sql.append("  ASSET_NO,");                 //資産番号
            sql.append("  CONST_NO,");                 //工事番号
            sql.append("  TEMP_ID");                   //仮置きID
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                         //処理履歴
            sql.append(" ?,");                         //資産番号
            sql.append(" ?,");                         //工事番号
            sql.append(" ?");                          //仮置きID
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    value.getProcessLogId(),            //処理履歴
                    value.getAssetNo(),                 //資産番号
                    ConstRuctionNo,                     //工事番号
                    TempId                              //仮置きID
            });

        }


    /**
     * 照合資産リストテーブルを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("TempAssetListDelete", "START");

        //工事資産テーブルの削除
        db.delete(CommonConst.TABLE_TEMP_ASSET_LIST, null, null);

    }

    /**
     * 照合資産リストデータを取得する
     * @param db
     */
    public static ArrayList<TempAssetListData> SelectData(SQLiteDatabase db, String ProcessLogId, String TempId){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO");              //資産番号
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_ASSET_LIST);
        sql.append(" WHERE ");
        sql.append(" PROCESS_LOG_ID = " + ProcessLogId );
        sql.append(" AND TEMP_ID = '" + TempId + "'" );

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        //取得したデータを処理履歴クラスに格納する
        ArrayList<TempAssetListData> map = new ArrayList<TempAssetListData>();
        while (cursor.moveToNext()) {
            TempAssetListData data = new TempAssetListData();

            //各項目を設定する
            data.setNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));               //資産番号
            map.add(data);
        }

        return map;
    }

    /**
     * 資産番号、処理履歴IDを取得する
     * @param ConstRuctionNo
     * @return
     */
    public static String GetAseetNoProcessIdTempIdData(String ConstRuctionNo,String TempId){
        StringBuffer sql = new StringBuffer();

        sql.append(" SELECT");

        sql.append("  PROCESS_LOG_ID AS PROCESS_LOG_ID,");
        sql.append("  ASSET_NO AS ASSET_NO");

        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_ASSET_LIST);

        sql.append(" WHERE ");
        sql.append(" CONST_NO = '" + ConstRuctionNo + "'");
        sql.append(" AND TEMP_ID = '" + TempId + "'");

        return sql.toString();
    }

    /**
     * 工事番号、仮置きIDで仮置き資産リストデータを削除する
     * @param db
     */
    public static void ConstNoTempIdDelete(SQLiteDatabase db, String ConstNo,String TempId){
        Log.d("TempAssetListDelete", "START");

        //照合資産テーブルの削除
        db.delete(CommonConst.TABLE_TEMP_ASSET_LIST, "CONST_NO = ? AND TEMP_ID = ?", new String[] {ConstNo,TempId});

        Log.d("ConstNo,TempId", "仮置き資産リストデータ削除");
    }

}

