package com.rfid.Factory;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;
import android.widget.EditText;
import android.widget.TextView;

import com.rfid.common.CommonConst;
import com.rfid.common.MatchAssetListData;
import com.rfid.common.TempAssetListData;
import com.rfid.common.TempListData;
import com.rfid.common.bean.EntryProcessDataDetail;
import com.rfidpk.R;

import java.util.ArrayList;

public class TempListFactory {

    /**
     * 仮置きリストテーブルにデータを登録する
     * @param db
     */
    public static void Insert(SQLiteDatabase db, EntryProcessDataDetail value,String ConstRuctionNo,String TempId)  {
        Log.d("TempListInsert", "START");

        ProcessFactory process = new ProcessFactory();

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.TABLE_TEMP_LIST);
            sql.append(" (");
            sql.append("  PROCESS_LOG_ID,");          //処理履歴
            sql.append("  TEMP_ID,");                 //仮置きID
            sql.append("  TEMP_DT,");                 //仮置き日
//            sql.append("  TEMP_EMP_CD,");             //作業責任者コード
//            sql.append("  TEMP_EMP_NAME,");           //作業責任者名
            sql.append("  TEMP_INFO1,");              //仮置き情報1
            sql.append("  TEMP_INFO2,");              //仮置き情報2
            sql.append("  TEMP_INFO3,");              //仮置き情報3
            sql.append("  TEMP_INFO4,");              //仮置き情報4
            sql.append("  TEMP_INFO5,");              //仮置き情報5
            sql.append("  TEMP_INFO6,");              //仮置き情報6
            sql.append("  TEMP_INFO7,");              //仮置き情報7
            sql.append("  TEMP_REMARK,");             //備考
            sql.append("  CONST_NO");                 //工事番号
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                         //処理履歴
            sql.append(" ?,");                         //仮置きID
            sql.append(" ?,");                         //仮置き日
//            sql.append(" ?,");                         //作業責任者コード
//            sql.append(" ?,");                         //作業責任者名
            sql.append(" ?,");                         //仮置き情報1
            sql.append(" ?,");                         //仮置き情報2
            sql.append(" ?,");                         //仮置き情報3
            sql.append(" ?,");                         //仮置き情報4
            sql.append(" ?,");                         //仮置き情報5
            sql.append(" ?,");                         //仮置き情報6
            sql.append(" ?,");                         //仮置き情報7
            sql.append(" ?,");                         //備考
            sql.append(" ?");                          //工事番号
            sql.append(" )");

        //仮置きテーブルから情報取得
        String getTempSql = TemporaryFactory.GetTemporaryInfoData(ConstRuctionNo,TempId);
        Cursor cursor = db.rawQuery(getTempSql, null);

        if (cursor.moveToNext()) {
            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    value.getProcessLogId(),                                            //処理履歴
                    TempId,                                                             //仮置きID
                    cursor.getString(cursor.getColumnIndex("TEMP_DT")),              //仮置き日
//                    cursor.getString(cursor.getColumnIndex("TEMP_EMP_CD")),          //作業責任者コード
//                    cursor.getString(cursor.getColumnIndex("TEMP_EMP_NAME")),        //作業責任者名
                    cursor.getString(cursor.getColumnIndex("INFO1")),                //仮置き情報1
                    cursor.getString(cursor.getColumnIndex("INFO2")),                //仮置き情報2
                    cursor.getString(cursor.getColumnIndex("INFO3")),                //仮置き情報3
                    cursor.getString(cursor.getColumnIndex("INFO4")),                //仮置き情報4
                    cursor.getString(cursor.getColumnIndex("INFO5")),                //仮置き情報5
                    cursor.getString(cursor.getColumnIndex("INFO6")),                //仮置き情報6
                    cursor.getString(cursor.getColumnIndex("INFO7")),                //仮置き情報7
                    cursor.getString(cursor.getColumnIndex("REMARK")),               //備考
                    ConstRuctionNo                                                      //工事番号
            });

            //ログメッセージ表示用に保存
            process.setTempDt(cursor.getString(cursor.getColumnIndex("TEMP_DT")));
//            process.setTempWorker(cursor.getString(cursor.getColumnIndex("TEMP_EMP_NAME")));
            process.setTempInfo1(cursor.getString(cursor.getColumnIndex("INFO1")));
            process.setTempInfo2(cursor.getString(cursor.getColumnIndex("INFO2")));
            process.setTempInfo3(cursor.getString(cursor.getColumnIndex("INFO3")));
            process.setTempInfo4(cursor.getString(cursor.getColumnIndex("INFO4")));
            process.setTempInfo5(cursor.getString(cursor.getColumnIndex("INFO5")));
            process.setTempInfo6(cursor.getString(cursor.getColumnIndex("INFO6")));
            process.setTempInfo7(cursor.getString(cursor.getColumnIndex("INFO7")));
        }

    }

    public static String GetMaxTempId(SQLiteDatabase db) {
        Log.d("GetMaxTempId", "START");

        String strTempId = "";

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  TEMP_ID ");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_LIST);
        sql.append(" WHERE TEMP_ID LIKE 'z%'");  //z+数値のもののみ取得
        sql.append(" AND CAST(SUBSTR(TEMP_ID, 2) AS INTEGER) = ");
        sql.append(" (SELECT MAX(CAST(SUBSTR(TEMP_ID, 2) AS INTEGER)) ");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_LIST);
        sql.append(" WHERE TEMP_ID LIKE 'z%') ");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        if (cursor.moveToNext()) {
            //取得する
            strTempId = cursor.getString(cursor.getColumnIndex("TEMP_ID"));        //表示、非表示

        }
        cursor.close();

        return strTempId;
    }

    /**
     * 仮置きリストテーブルを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("TempListDelete", "START");

        //工事資産テーブルの削除
        db.delete(CommonConst.TABLE_TEMP_LIST, null, null);

    }

    /**
     * 照合資産リストデータを取得する
     * @param db
     */
    public static ArrayList<TempListData> SelectData(SQLiteDatabase db, String ProcessLogId){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  TEMP_ID");              //仮置きID
        sql.append("  ,TEMP_DT");             //仮置き日
//        sql.append("  ,TEMP_EMP_CD");         //作業責任者コード
//        sql.append("  ,TEMP_EMP_NAME");       //作業責任者名
        sql.append("  ,TEMP_INFO1");          //仮置き情報1
        sql.append("  ,TEMP_INFO2");          //仮置き情報2
        sql.append("  ,TEMP_INFO3");          //仮置き情報3
        sql.append("  ,TEMP_INFO4");          //仮置き情報4
        sql.append("  ,TEMP_INFO5");          //仮置き情報5
        sql.append("  ,TEMP_INFO6");          //仮置き情報6
        sql.append("  ,TEMP_INFO7");          //仮置き情報7
        sql.append("  ,TEMP_REMARK");         //備考
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_LIST);
        sql.append(" WHERE ");
        sql.append(" PROCESS_LOG_ID = " + ProcessLogId );

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        //取得したデータを処理履歴クラスに格納する
        ArrayList<TempListData> map = new ArrayList<TempListData>();
        while (cursor.moveToNext()) {
            TempListData data = new TempListData();

            //各項目を設定する
            data.setTempId(cursor.getString(cursor.getColumnIndex("TEMP_ID")));               //仮置きID
            data.setTempDt(cursor.getString(cursor.getColumnIndex("TEMP_DT")));               //仮置き日
//            data.setTempEmpCd(cursor.getString(cursor.getColumnIndex("TEMP_EMP_CD")));        //作業責任者コード
//            data.setTempEmpName(cursor.getString(cursor.getColumnIndex("TEMP_EMP_NAME")));    //作業責任者名
            data.setTempInfo1(cursor.getString(cursor.getColumnIndex("TEMP_INFO1")));         //仮置き情報1
            data.setTempInfo2(cursor.getString(cursor.getColumnIndex("TEMP_INFO2")));         //仮置き情報2
            data.setTempInfo3(cursor.getString(cursor.getColumnIndex("TEMP_INFO3")));         //仮置き情報3
            data.setTempInfo4(cursor.getString(cursor.getColumnIndex("TEMP_INFO4")));         //仮置き情報4
            data.setTempInfo5(cursor.getString(cursor.getColumnIndex("TEMP_INFO5")));         //仮置き情報5
            data.setTempInfo6(cursor.getString(cursor.getColumnIndex("TEMP_INFO6")));         //仮置き情報6
            data.setTempInfo7(cursor.getString(cursor.getColumnIndex("TEMP_INFO7")));         //仮置き情報7
            data.setTempRemark(cursor.getString(cursor.getColumnIndex("TEMP_REMARK")));       //備考

            //貸出資産リスト追加
            data.setTempAssetList(TempAssetListFactory.SelectData(db,ProcessLogId,cursor.getString(cursor.getColumnIndex("TEMP_ID"))));

            map.add(data);
        }

        return map;
    }

    /**
     * 仮置きリストテーブル取得SQL
     * @return SQL
     */
    public static String getTempListData(String ConstRuctionNo){
        StringBuffer sql = new StringBuffer();


        sql.append(" SELECT");
        sql.append("  TEMP.TEMP_ID AS TEMP_ID");
        sql.append("  , TEMP.TEMP_DT AS TEMP_DT");
//        sql.append("  , TEMP.TEMP_EMP_CD AS TEMP_EMP_CD");
//        sql.append("  , TEMP.TEMP_EMP_NAME AS TEMP_EMP_NAME");
        sql.append("  , TEMP.TEMP_INFO1 AS INFO1");
        sql.append("  , TEMP.TEMP_INFO2 AS INFO2");
        sql.append("  , TEMP.TEMP_INFO3 AS INFO3");
        sql.append("  , TEMP.TEMP_INFO4 AS INFO4");
        sql.append("  , TEMP.TEMP_INFO5 AS INFO5");
        sql.append("  , TEMP.TEMP_INFO6 AS INFO6");
        sql.append("  , TEMP.TEMP_INFO7 AS INFO7");
        sql.append("  , COUNT(TEMPASSET.TEMP_ID) AS COUNT");

        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_LIST + " AS TEMP");
        sql.append(" LEFT JOIN ");
        sql.append(CommonConst.TABLE_TEMP_ASSET_LIST + " AS TEMPASSET");
        sql.append(" ON ");
        sql.append(" TEMP.TEMP_ID = TEMPASSET.TEMP_ID ");

        sql.append(" WHERE ");
        sql.append(" TEMP.CONST_NO = '" + ConstRuctionNo + "'");

        sql.append(" GROUP BY TEMP.TEMP_ID ");

        return sql.toString();
    }

    /**
     * 仮置きリストマスタ取得SQL(資産情報なし)
     * @return SQL
     */
    public static String GetTempListDataTempId(String ConstRuctionNo,String tempId){
        StringBuffer sql = new StringBuffer();

        sql.append(" SELECT");
        sql.append("  TEMP.TEMP_ID AS TEMP_ID");
        sql.append("  , TEMP.TEMP_DT AS TEMP_DT");
//        sql.append("  , TEMP.TEMP_EMP_CD AS TEMP_EMP_CD");
//        sql.append("  , TEMP.TEMP_EMP_NAME AS TEMP_EMP_NAME");
        sql.append("  , TEMP.TEMP_INFO1 AS INFO1");
        sql.append("  , TEMP.TEMP_INFO2 AS INFO2");
        sql.append("  , TEMP.TEMP_INFO3 AS INFO3");
        sql.append("  , TEMP.TEMP_INFO4 AS INFO4");
        sql.append("  , TEMP.TEMP_INFO5 AS INFO5");
        sql.append("  , TEMP.TEMP_INFO6 AS INFO6");
        sql.append("  , TEMP.TEMP_INFO7 AS INFO7");
        sql.append("  , TEMP.TEMP_REMARK AS REMARK");

        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_LIST + " AS TEMP");

        sql.append(" WHERE ");
        sql.append(" TEMP.CONST_NO = '" + ConstRuctionNo + "'");
        sql.append(" AND TEMP.TEMP_ID = '" + tempId +"'");

        return sql.toString();
    }

    /**
     * 仮置き資産マスタ取得SQL
     * @return SQL
     */
    public static String GetAssetNoData(String ConstRuctionNo,String TempId){
        StringBuffer sql = new StringBuffer();

        sql.append(" SELECT");
        sql.append("  ASSET_NO AS ASSET_NO");

        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_ASSET);

        sql.append(" WHERE ");
        sql.append(" CONST_NO = '" + ConstRuctionNo + "'");
        sql.append(" AND TEMP_ID = '" + TempId +"'");

        return sql.toString();
    }

}

