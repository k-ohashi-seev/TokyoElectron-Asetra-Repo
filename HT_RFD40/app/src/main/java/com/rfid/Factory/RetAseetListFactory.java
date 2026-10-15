package com.rfid.Factory;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.LentAssetListData;
import com.rfid.common.RetAssetListData;
import com.rfid.common.bean.EntryProcessDataDetail;

import java.util.ArrayList;

public class RetAseetListFactory {

    /**
     * 返却リストテーブルにデータを登録する
     * @param db
     */
    public static void Insert(SQLiteDatabase db, EntryProcessDataDetail value,String ConstRuctionNo)  {
        Log.d("RetAssetListInsert", "START");

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.TABLE_RET_ASSET_LIST);
            sql.append(" (");
            sql.append("  PROCESS_LOG_ID,");           //処理履歴
            sql.append("  ASSET_NO,");                 //資産番号
            sql.append("  CONST_NO," );                //工事番号
            sql.append("  CONT_MATCH_DIV" );           //照合結果
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
     * 返却リストテーブルを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("RetAssetListDelete", "START");

        //工事資産テーブルの削除
        db.delete(CommonConst.TABLE_RET_ASSET_LIST, null, null);

    }

    /**
     * 返却リストデータを取得する
     * @param db
     */
    public static ArrayList<RetAssetListData> SelectData(SQLiteDatabase db, String ProcessLogId){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO,");                 //資産番号
        sql.append("  CONT_MATCH_DIV");            //照合結果
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_RET_ASSET_LIST);
        sql.append(" WHERE ");
        sql.append(" PROCESS_LOG_ID = " + ProcessLogId );

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        //取得したデータを処理履歴クラスに格納する
        ArrayList<RetAssetListData> map = new ArrayList<RetAssetListData>();
        while (cursor.moveToNext()) {
            RetAssetListData data = new RetAssetListData();

            //各項目を設定する
            data.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));                   //処理履歴ID
            data.setContMatchDiv(cursor.getString(cursor.getColumnIndex("CONT_MATCH_DIV")));

            map.add(data);
        }

        return map;
    }

    /**
     * 返却リスト資産マスタ取得SQL
     * @return SQL
     */
    public static String GetAssetNoData(String ConstRuctionNo){
        StringBuffer sql = new StringBuffer();

        sql.append(" SELECT");
        sql.append("  ASSET_NO AS ASSET_NO");
        sql.append("  ,CONT_MATCH_DIV AS CONT_MATCH_DIV");

        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_RET_ASSET_LIST);

        sql.append(" WHERE ");
        sql.append(" CONST_NO = '" + ConstRuctionNo + "'");

        return sql.toString();
    }

    /**
     * 返却リスト資産マスタ全て取得SQL
     * @return SQL
     */
    public static String GetAssetNoConstNoData(){
        StringBuffer sql = new StringBuffer();

        sql.append(" SELECT");
        sql.append("  ASSET_NO AS ASSET_NO");
        sql.append("  ,CONST_NO AS CONST_NO");
        sql.append("  ,CONST_NO AS CONST_NO");
        sql.append("  ,CONT_MATCH_DIV AS CONT_MATCH_DIV");

        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_RET_ASSET_LIST);

        return sql.toString();
    }

    /**
     * 資産番号、処理履歴IDを取得する
     * @param ConstRuctionNo
     * @return
     */
    public static String GetAseetNoProcessIdData(String ConstRuctionNo){
        StringBuffer sql = new StringBuffer();

        sql.append(" SELECT");

        sql.append("  PROCESS_LOG_ID AS PROCESS_LOG_ID,");
        sql.append("  ASSET_NO AS ASSET_NO");

        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_RET_ASSET_LIST);

        sql.append(" WHERE ");
        sql.append(" CONST_NO = '" + ConstRuctionNo + "'");

        return sql.toString();
    }

    /**
     * 工事番号で照合貸出リストデータを削除する
     * @param db
     */
    public static void ConstNoDelete(SQLiteDatabase db, String ConstNo){
        Log.d("RetAssetListDelete", "START");

        //返却リストテーブルの削除
        db.delete(CommonConst.TABLE_RET_ASSET_LIST, "CONST_NO = ?", new String[] {ConstNo});

        Log.d("ConstNo", "返却リストデータ削除");
    }

}

