package com.rfid.Factory;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.Common;
import com.rfid.common.CommonConst;
import com.rfid.common.bean.EntryProcessDataDetail;

import org.json.JSONArray;
import org.json.JSONException;

public class TemporaryFactory {

    /**
     * 仮置きテーブルにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json,String prmConstId) throws JSONException {
        Log.d("TemporaryInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.TABLE_TEMPORARY);
            sql.append(" (");
            sql.append("  TEMP_ID,");           //仮置きID
            sql.append("  TEMP_DT,");           //仮置き日
//            sql.append("  TEMP_EMP_CD,");       //作業責任者コード
//            sql.append("  TEMP_EMP_NAME,");     //作業責任者名
            sql.append("  TEMP_INFO1,");        //仮置き情報1
            sql.append("  TEMP_INFO2,");        //仮置き情報2
            sql.append("  TEMP_INFO3,");        //仮置き情報3
            sql.append("  TEMP_INFO4,");        //仮置き情報4
            sql.append("  TEMP_INFO5,");        //仮置き情報5
            sql.append("  TEMP_INFO6,");        //仮置き情報6
            sql.append("  TEMP_INFO7,");        //仮置き情報7
            sql.append("  TEMP_REMARK,");       //備考
            sql.append("  CONST_NO");           //工事番号
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                  //仮置きID
            sql.append(" ?,");                  //仮置き日
//            sql.append(" ?,");                  //作業責任者コード
//            sql.append(" ?,");                  //作業責任者名
            sql.append(" ?,");                  //仮置き情報1
            sql.append(" ?,");                  //仮置き情報2
            sql.append(" ?,");                  //仮置き情報3
            sql.append(" ?,");                  //仮置き情報4
            sql.append(" ?,");                  //仮置き情報5
            sql.append(" ?,");                  //仮置き情報6
            sql.append(" ?,");                  //仮置き情報7
            sql.append(" ?,");                  //備考
            sql.append(" ?");                   //工事番号
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("tempId"),          //仮置きID
                    json.getJSONObject(i).getString("tempDt"),          //仮置き日
//                    json.getJSONObject(i).getString("tempEmpCd"),       //作業責任者コード
//                    json.getJSONObject(i).getString("tempEmpName"),     //作業責任者名
                    json.getJSONObject(i).getString("tempInfo1"),       //仮置き情報1
                    json.getJSONObject(i).getString("tempInfo2"),       //仮置き情報2
                    json.getJSONObject(i).getString("tempInfo3"),       //仮置き情報3
                    json.getJSONObject(i).getString("tempInfo4"),       //仮置き情報4
                    json.getJSONObject(i).getString("tempInfo5"),       //仮置き情報5
                    json.getJSONObject(i).getString("tempInfo6"),       //仮置き情報4
                    json.getJSONObject(i).getString("tempInfo7"),       //仮置き情報5
                    json.getJSONObject(i).getString("tempRemark"),      //備考
                    prmConstId                                                  //工事番号
            });

            //仮置き資産テーブルにデータを登録する
            TempAssetFactory.Insert(db, json.getJSONObject(i).getJSONArray("tempAssetList"),prmConstId,json.getJSONObject(i).getString("tempId"));

            Log.d("TemporaryInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 仮置きリストテーブルにデータを登録する
     * @param db
     */
    public static void Insert(SQLiteDatabase db, EntryProcessDataDetail value, String[] data,String tempId)  {
        Log.d("TempListInsert", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" INSERT");
        sql.append(" INTO ");
        sql.append(CommonConst.TABLE_TEMPORARY);
        sql.append(" (");
        sql.append("  TEMP_ID,");                 //仮置きID
        sql.append("  TEMP_DT,");                 //仮置き日
//        sql.append("  TEMP_EMP_CD,");             //作業責任者コード
//        sql.append("  TEMP_EMP_NAME,");           //作業責任者名
        sql.append("  TEMP_INFO1,");              //仮置き情報1
        sql.append("  TEMP_INFO2,");              //仮置き情報2
        sql.append("  TEMP_INFO3,");              //仮置き情報3
        sql.append("  TEMP_INFO4,");              //仮置き情報4
        sql.append("  TEMP_INFO5,");              //仮置き情報5
        sql.append("  TEMP_INFO6,");              //仮置き情報6
        sql.append("  TEMP_INFO7,");              //仮置き情報7
        sql.append("  TEMP_REMARK,")  ;            //備考
        sql.append("  CONST_NO");                 //工事番号
        sql.append("  ) ");
        sql.append(" VALUES (");
        sql.append(" ?,");                         //仮置きID
        sql.append(" strftime('%Y%m%d', datetime('now', 'localtime')),");  //仮置き日
//        sql.append(" ?,");                         //作業責任者コード
//        sql.append(" ?,");                         //作業責任者名
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

        //SQL発行
            db.execSQL(sql.toString(), new Object[]{tempId,  data[0], data[1], data[2], data[3], data[4], data[5], data[6],data[7], data[8]});
        }

    /**
     * 仮置きリストテーブルのデータを更新する
     * @param db
     */
    public static void Update(SQLiteDatabase db, EntryProcessDataDetail value, String[] data,String tempId,Integer tempDate,String constNo)  {
        Log.d("TempListInsert", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" UPDATE ");
        sql.append(CommonConst.TABLE_TEMPORARY);
        sql.append(" SET");
        sql.append("  TEMP_ID = ?,");                 //仮置きID
        sql.append("  TEMP_DT = ?,");                 //仮置き日
//        sql.append("  TEMP_EMP_CD = ?,");             //作業責任者コード
//        sql.append("  TEMP_EMP_NAME = ?,");           //作業責任者名
        sql.append("  TEMP_INFO1 = ?,");              //仮置き情報1
        sql.append("  TEMP_INFO2 = ?,");              //仮置き情報2
        sql.append("  TEMP_INFO3 = ?,");              //仮置き情報3
        sql.append("  TEMP_INFO4 = ?,");              //仮置き情報4
        sql.append("  TEMP_INFO5 = ?,");              //仮置き情報5
        sql.append("  TEMP_INFO6 = ?,");              //仮置き情報6
        sql.append("  TEMP_INFO7 = ?,");              //仮置き情報7
        sql.append("  TEMP_REMARK = ?,");             //備考
        sql.append("  CONST_NO = ?");                 //工事番号
        sql.append("  WHERE TEMP_ID = ?");
        sql.append("  AND CONST_NO = ?");

        //SQL発行
        //インデックス指定必要
        db.execSQL(sql.toString(), new Object[]{tempId, tempDate,data[0],data[1],data[2],data[3],data[4],data[5],data[6],data[7], data[8], tempId, constNo});
    }

    public static String GetMaxTempId(SQLiteDatabase db) {
        Log.d("GetMaxTempId", "START");

        String strTempId = "";

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  TEMP_ID ");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMPORARY);
        sql.append(" WHERE TEMP_ID LIKE 'z%'");  //z+数値のもののみ取得
        sql.append(" AND CAST(SUBSTR(TEMP_ID, 2) AS INTEGER) = ");
        sql.append(" (SELECT MAX(CAST(SUBSTR(TEMP_ID, 2) AS INTEGER)) ");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMPORARY);
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
     * 仮置き資産テーブルにデータを更新する
     * @param db
     */
    public static void UpdateTempId(SQLiteDatabase db, String BeforeTempId, String NweTempId,String constNo)  {
        Log.d("TemporaryUpdateTempId", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" UPDATE ");
        sql.append(CommonConst.TABLE_TEMPORARY);
        sql.append(" SET");
        sql.append("  TEMP_ID = ? ");                 //仮置きID
        sql.append("  WHERE TEMP_ID = ?");
        sql.append("  AND CONST_NO = ?");

        //SQL発行
        //インデックス指定必要
        db.execSQL(sql.toString(), new Object[]{NweTempId, BeforeTempId, constNo});

    }

    /**
     * 仮置きマスタ取得SQL
     * @return SQL
     */
    public static String createSQLData(String ConstRuctionNo){
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
        sql.append(CommonConst.TABLE_TEMPORARY + " AS TEMP");
        sql.append(" LEFT JOIN ");
        sql.append(CommonConst.TABLE_TEMP_ASSET + " AS TEMPASSET");
        sql.append(" ON ");
        sql.append(" TEMP.TEMP_ID = TEMPASSET.TEMP_ID ");

        sql.append(" WHERE ");
        sql.append(" TEMP.CONST_NO = '" + ConstRuctionNo + "'");

        sql.append(" GROUP BY TEMP.TEMP_ID ");

        return sql.toString();
    }

    /**
     * 仮置きテーブルを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("TemporaryDelete", "START");

        //権限マスタの削除
        db.delete(CommonConst.TABLE_TEMPORARY, null, null);

    }

    /**
     * 仮置きマスタ取得SQL
     * @return SQL
     */
    public static String GetTemporaryData(String ConstRuctionNo,String tempId){
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
        sql.append(CommonConst.TABLE_TEMPORARY + " AS TEMP");

        sql.append(" WHERE ");
        sql.append(" TEMP.CONST_NO = '" + ConstRuctionNo + "'");
        sql.append(" AND TEMP.TEMP_ID = '" + tempId +"'");

        return sql.toString();
    }

    /**
     * 仮置きマスタ取得SQL
     * @return SQL
     */
    public static String GetTemporaryInfoData(String ConstRuctionNo,String TempId){
        StringBuffer sql = new StringBuffer();

        sql.append(" SELECT");
        sql.append("  TEMP_ID AS TEMP_ID");
        sql.append("  , TEMP_DT AS TEMP_DT");
//        sql.append("  , TEMP_EMP_CD AS TEMP_EMP_CD");
//        sql.append("  , TEMP_EMP_NAME AS TEMP_EMP_NAME");
        sql.append("  , TEMP_INFO1 AS INFO1");
        sql.append("  , TEMP_INFO2 AS INFO2");
        sql.append("  , TEMP_INFO3 AS INFO3");
        sql.append("  , TEMP_INFO4 AS INFO4");
        sql.append("  , TEMP_INFO5 AS INFO5");
        sql.append("  , TEMP_INFO6 AS INFO6");
        sql.append("  , TEMP_INFO7 AS INFO7");
        sql.append("  , TEMP_REMARK AS REMARK");

        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMPORARY);

        sql.append(" WHERE ");
        sql.append(" CONST_NO = '" + ConstRuctionNo + "'");
        sql.append(" AND TEMP_ID = '" + TempId +"'");

        return sql.toString();
    }

    /**
     * 仮置きマスタ取得SQL
     * @return SQL
     */
    public static String GetTempIdData(SQLiteDatabase db,String ConstRuctionNo,String TempId){
        StringBuffer sql = new StringBuffer();
        String assetSql = "";

        sql.append(" SELECT");
        sql.append("  TEMP_ID AS TEMP_ID");

        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMPORARY);

        sql.append(" WHERE ");
        sql.append(" CONST_NO = '" + ConstRuctionNo + "'");
        sql.append(" AND TEMP_ID = '" + TempId +"'");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        if (cursor.moveToNext()) {
            //取得する
            assetSql = TempListFactory.GetAssetNoData(ConstRuctionNo,cursor.getString(cursor.getColumnIndex("TEMP_ID")));        //表示、非表示

        }
        cursor.close();

        return assetSql;
    }

}

