package com.rfid.Factory;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.MatchAssetListData;
import com.rfid.common.RetAssetListData;
import com.rfid.common.bean.EntryProcessDataDetail;

import org.json.JSONArray;
import org.json.JSONException;

import java.util.ArrayList;

public class MatchAseetListFactory {

    /**
     * 照合資産リストテーブルにデータを登録する
     * @param db
     */
    public static void Insert(SQLiteDatabase db, EntryProcessDataDetail value,String ConstRuctionNo)  {
        Log.d("MatchAssetListInsert", "START");



            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.TABLE_MATCH_ASSET_LIST);
            sql.append(" (");
            sql.append("  PROCESS_LOG_ID,");           //処理履歴
            sql.append("  ASSET_NO,");                 //資産番号
            sql.append("  CONST_NO,");                 //工事番号
            sql.append("  CONT_MATCH_DIV");            //照合結果
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                         //処理履歴
            sql.append(" ?,");                         //資産番号
            sql.append(" ?,");                         //工事番号
            sql.append(" ?");                          //照合結果
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    value.getProcessLogId(),            //処理履歴
                    value.getAssetNo(),                 //資産番号
                    ConstRuctionNo,                     //工事番号
                    value.getContMatchDiv()             //照合結果
            });

        }


    /**
     * 照合資産リストテーブルを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("MattchAssetListDelete", "START");

        //工事資産テーブルの削除
        db.delete(CommonConst.TABLE_MATCH_ASSET_LIST, null, null);

    }

    /**
     * 照合資産リストデータを取得する
     * @param db
     */
    public static ArrayList<MatchAssetListData> SelectData(SQLiteDatabase db, String ProcessLogId){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO");                 //資産番号
        sql.append("  ,CONT_MATCH_DIV");          //照合結果
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_MATCH_ASSET_LIST);
        sql.append(" WHERE ");
        sql.append(" PROCESS_LOG_ID = " + ProcessLogId );

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        //取得したデータを処理履歴クラスに格納する
        ArrayList<MatchAssetListData> map = new ArrayList<MatchAssetListData>();
        while (cursor.moveToNext()) {
            MatchAssetListData data = new MatchAssetListData();

            //各項目を設定する
            data.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));                   //処理履歴ID
            data.setContMatchDiv(cursor.getString(cursor.getColumnIndex("CONT_MATCH_DIV")));        //照合結果

            map.add(data);
        }

        return map;
    }

}


