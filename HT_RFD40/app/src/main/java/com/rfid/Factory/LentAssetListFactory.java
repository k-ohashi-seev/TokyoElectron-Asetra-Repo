package com.rfid.Factory;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.LentAssetListData;
import com.rfid.common.bean.EntryMatchProcessData;
import com.rfid.common.bean.EntryMatchProcessDataDetail;
import com.rfid.common.bean.EntryProcessDataDetail;

import java.util.ArrayList;

public class LentAssetListFactory {

    /**
     * 照合資産リストテーブルにデータを登録する
     * @param db
     */
    public static void Insert(SQLiteDatabase db, EntryProcessDataDetail value,String ConstRuctionNo)  {
        Log.d("LentAssetListInsert", "START");



            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.TABLE_LENT_ASSET_LIST);
            sql.append(" (");
            sql.append("  PROCESS_LOG_ID,");           //処理履歴
            sql.append("  ASSET_NO,");                 //資産番号
            sql.append("  CONST_NO,");                 //工事番号
            sql.append("  LEASE_DT,");                 //持出日
            sql.append("  REMARK ");                   //備考
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");
            sql.append(" ?,");
            sql.append(" ?,");
            sql.append(" strftime('%Y%m%d', datetime('now', 'localtime')),");
            sql.append(" ?");
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    value.getProcessLogId(),            //処理履歴
                    value.getAssetNo(),                 //資産番号
                    ConstRuctionNo,                     //工事番号
                    value.getRemark(),                  //備考
            });

        }


    /**
     * 照合資産リストテーブルを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("LentAssetListDelete", "START");

        //照合資産テーブルの削除
        db.delete(CommonConst.TABLE_LENT_ASSET_LIST, null, null);

    }

    /**
     * 工事番号で照合貸出リストデータを削除する
     * @param db
     */
    public static void ConstNoDelete(SQLiteDatabase db, String ConstNo){
        Log.d("LentAssetListDelete", "START");

//        //SQL作成
//        StringBuilder sql = new StringBuilder();
//        sql.append(" DELETE");
//        sql.append(" FROM ");
//        sql.append(CommonConst.TABLE_LENT_ASSET_LIST);
//        sql.append(" WHERE ");
//        sql.append(" CONST_NO = '" + ConstNo + "'" );
//
//        //SQL発行
//        Cursor cursor = db.rawQuery(sql.toString(), null);

        //照合資産テーブルの削除
        db.delete(CommonConst.TABLE_LENT_ASSET_LIST, "CONST_NO = ?", new String[] {ConstNo});

        Log.d("ConstNo", "貸出リストデータ削除");
    }

    /**
     * 照合資産リストデータを取得する
     * @param db
     */
    public static ArrayList<LentAssetListData> SelectData(SQLiteDatabase db,String ProcessLogId){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO");                 //資産番号
        sql.append("  ,LEASE_DT");                //持出日
        sql.append("  ,REMARK");                  //備考
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_LENT_ASSET_LIST);
        sql.append(" WHERE ");
        sql.append(" PROCESS_LOG_ID = " + ProcessLogId );

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        //取得したデータを処理履歴クラスに格納する
        ArrayList<LentAssetListData> map = new ArrayList<LentAssetListData>();
        while (cursor.moveToNext()) {
            LentAssetListData data = new LentAssetListData();

            //各項目を設定する
            data.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));                   //処理履歴ID
            data.setLeaseDt(cursor.getString(cursor.getColumnIndex("LEASE_DT")));                   //処理日
            data.setRemark(cursor.getString(cursor.getColumnIndex("REMARK")));                      //備考

            map.add(data);
        }

        return map;
    }

    /**
     * 資産番号を取得する
     * @param ConstRuctionNo
     * @return
     */
    public static String GetAssetNoData(String ConstRuctionNo){
        StringBuffer sql = new StringBuffer();

        sql.append(" SELECT");
        sql.append("  ASSET_NO AS ASSET_NO");

        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_LENT_ASSET_LIST);

        sql.append(" WHERE ");
        sql.append(" CONST_NO = '" + ConstRuctionNo + "'");

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
        sql.append(CommonConst.TABLE_LENT_ASSET_LIST);

        sql.append(" WHERE ");
        sql.append(" CONST_NO = '" + ConstRuctionNo + "'");

        return sql.toString();
    }

}

