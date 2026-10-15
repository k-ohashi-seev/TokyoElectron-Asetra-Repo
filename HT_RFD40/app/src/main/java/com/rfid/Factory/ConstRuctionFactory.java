package com.rfid.Factory;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConst;

import org.json.JSONArray;
import org.json.JSONException;

public class ConstRuctionFactory {

    /**
     * 工事マスタにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json) throws JSONException {
        Log.d("ConstRuctionInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.MASTER_CONSTRUCTION);
            sql.append(" (");
            sql.append("  CONST_NO,");             //工事番号
            sql.append("  CONST_START_DT,");       //工事開始日
            sql.append("  CONST_END_DT,");         //工事終了日
            sql.append("  CONST_LENT_START_DT,");  //工事貸出開始日
            sql.append("  CONST_LENT_END_DT,");    //工事貸出終了日
            sql.append("  CONST_CONTENTS,");       //工事内容
            sql.append("  CONST_EMPLOYEE_CD,");    //工事責任者コード
            sql.append("  CONST_EMPLOYEE_NAME,");  //工事責任者氏名
            sql.append("  MNG_LOC_L_CD,");         //管理ロケ大区分コード
            sql.append("  CONST_INFO1,");          //工事情報1
            sql.append("  CONST_INFO2,");          //工事情報2
            sql.append("  CONST_INFO3,");          //工事情報3
            sql.append("  CONST_INFO4,");          //工事情報4
            sql.append("  CONST_INFO5,");          //工事情報5
            sql.append("  CONST_INFO6,");          //工事情報6
            sql.append("  CONST_INFO7,");          //工事情報7
            sql.append("  CONST_INFO8,");          //工事情報8
            sql.append("  CONST_INFO9,");          //工事情報9
            sql.append("  CONST_INFO10,");         //工事情報10
            sql.append("  CONST_INFO11,");         //工事情報11
            sql.append("  CONST_INFO12,");         //工事情報12
            sql.append("  CONST_INFO13,");         //工事情報13
            sql.append("  CONST_INFO14,");         //工事情報14
            sql.append("  CONST_INFO15,");         //工事情報15
            sql.append("  CONST_INFO16,");         //工事情報16
            sql.append("  CONST_INFO17,");         //工事情報17
            sql.append("  CONST_INFO18,");         //工事情報18
            sql.append("  CONST_INFO19,");         //工事情報19
            sql.append("  CONST_INFO20");          //工事情報20
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                     //工事番号
            sql.append(" ?,");                     //工事開始日
            sql.append(" ?,");                     //工事終了日
            sql.append(" ?,");                     //工事貸出開始日
            sql.append(" ?,");                     //工事貸出終了日
            sql.append(" ?,");                     //工事内容
            sql.append(" ?,");                     //工事責任者コード
            sql.append(" ?,");                     //工事責任者氏名
            sql.append(" ?,");                     //管理ロケ大区分コード
            sql.append(" ?,");                     //工事情報1
            sql.append(" ?,");                     //工事情報2
            sql.append(" ?,");                     //工事情報3
            sql.append(" ?,");                     //工事情報4
            sql.append(" ?,");                     //工事情報5
            sql.append(" ?,");                     //工事情報6
            sql.append(" ?,");                     //工事情報7
            sql.append(" ?,");                     //工事情報8
            sql.append(" ?,");                     //工事情報9
            sql.append(" ?,");                     //工事情報10
            sql.append(" ?,");                     //工事情報11
            sql.append(" ?,");                     //工事情報12
            sql.append(" ?,");                     //工事情報13
            sql.append(" ?,");                     //工事情報14
            sql.append(" ?,");                     //工事情報15
            sql.append(" ?,");                     //工事情報16
            sql.append(" ?,");                     //工事情報17
            sql.append(" ?,");                     //工事情報18
            sql.append(" ?,");                     //工事情報19
            sql.append(" ?");                      //工事情報20

            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("no"),          //工事番号
                    json.getJSONObject(i).getString("stDt"),        //工事開始日
                    json.getJSONObject(i).getString("endDt"),       //工事終了日
                    json.getJSONObject(i).getString("lentStDt"),    //工事貸出開始日
                    json.getJSONObject(i).getString("lentEndDt"),   //工事貸出終了日
                    json.getJSONObject(i).getString("contents"),    //工事内容
                    json.getJSONObject(i).getString("empCd"),       //工事責任者コード
                    json.getJSONObject(i).getString("empName"),     //工事責任者氏名
                    json.getJSONObject(i).getString("mngLocLCd"),   //管理ロケ大区分コード
                    json.getJSONObject(i).getString("info1"),       //工事情報1
                    json.getJSONObject(i).getString("info2"),       //工事情報2
                    json.getJSONObject(i).getString("info3"),       //工事情報3
                    json.getJSONObject(i).getString("info4"),       //工事情報4
                    json.getJSONObject(i).getString("info5"),       //工事情報5
                    json.getJSONObject(i).getString("info6"),       //工事情報6
                    json.getJSONObject(i).getString("info7"),       //工事情報7
                    json.getJSONObject(i).getString("info8"),       //工事情報8
                    json.getJSONObject(i).getString("info9"),       //工事情報9
                    json.getJSONObject(i).getString("info10"),      //工事情報10
                    json.getJSONObject(i).getString("info11"),      //工事情報11
                    json.getJSONObject(i).getString("info12"),      //工事情報12
                    json.getJSONObject(i).getString("info13"),      //工事情報13
                    json.getJSONObject(i).getString("info14"),      //工事情報14
                    json.getJSONObject(i).getString("info15"),      //工事情報15
                    json.getJSONObject(i).getString("info16"),      //工事情報16
                    json.getJSONObject(i).getString("info17"),      //工事情報17
                    json.getJSONObject(i).getString("info18"),      //工事情報18
                    json.getJSONObject(i).getString("info19"),      //工事情報19
                    json.getJSONObject(i).getString("info20"),      //工事情報20

            });
            //仮置きテーブルにデータを登録する
            TemporaryFactory.Insert(db, json.getJSONObject(i).getJSONArray("temporaryList"),json.getJSONObject(i).getString("no"));

            //工事資産テーブルにデータを登録する
            ConstAssetFactory.Insert(db, json.getJSONObject(i).getJSONArray("matchList"),json.getJSONObject(i).getString("no"));

            Log.d("ConstRuctionInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 工事マスタを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("ConstRuctionDelete", "START");

        //権限マスタの削除
        db.delete(CommonConst.MASTER_CONSTRUCTION, null, null);

    }

    /**
     * 工事マスタ取得SQL
     * @return SQL
     */
    public static String getConstRuctionData(String Day ,String Liability,String no,String[] target){
        StringBuffer sql = new StringBuffer();
        int i = 0;

        sql.append(" SELECT");
        sql.append("  CONST_START_DT AS START_DT");
        sql.append("  , CONST_END_DT AS END_DT");
        sql.append("  , CONST_EMPLOYEE_NAME AS EMPLOYEE_NAME");
        sql.append("  , CONST_CONTENTS AS CONTENTS");
        sql.append("  , CONST_NO AS NO");
        sql.append("  , MNG_LOC_L_CD AS MngLocLCd");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_CONSTRUCTION);

        //日付と責任者が指定されている場合
        if(!Day.equals("") && !Liability.equals("")){
            sql.append(" WHERE ");
            sql.append(" CONST_START_DT <= '" + Day.replace("/", "") + "'");
            sql.append(" AND CONST_END_DT >= '" + Day.replace("/", "") + "'");
            sql.append(" AND CONST_EMPLOYEE_NAME LIKE '%" + Liability + "%'");

                if(target != null){
                    sql.append(" AND MNG_LOC_L_CD IN ( ");
                    for (String hub : target){
                        sql.append("'" + hub + "'");

                        i++;
                        if(target.length > i ){
                            //コンマ追加
                            sql.append(",");
                        }
                    }
                    sql.append(" ) ");

                }
        }else{
            //日付のみ指定されている場合
            if(!Day.equals("")){
                sql.append(" WHERE ");
                sql.append(" CONST_START_DT <= '" + Day.replace("/", "") + "'");
                sql.append(" AND CONST_END_DT >= '" + Day.replace("/", "") + "'");

                if(target != null){
                    sql.append(" AND MNG_LOC_L_CD IN ( ");
                    for (String hub : target){
                        sql.append("'" + hub + "'");

                        i++;
                        if(target.length > i ){
                            //コンマ追加
                            sql.append(",");
                        }
                    }
                    sql.append(" ) ");

                }
            }else if(!Liability.equals("")){
                sql.append(" WHERE ");
                sql.append(" CONST_EMPLOYEE_NAME LIKE '%" + Liability + "%'");

                if(target != null){
                    sql.append(" AND MNG_LOC_L_CD IN ( ");
                    for (String hub : target){
                        sql.append("'" + hub + "'");

                        i++;
                        if(target.length > i ){
                            //コンマ追加
                            sql.append(",");
                        }
                    }
                    sql.append(" ) ");

                }
            }else{
                if(target != null){
                    sql.append(" WHERE ");
                    sql.append("  MNG_LOC_L_CD IN ( ");
                    for (String hub : target){
                        sql.append("'" + hub + "'");

                        i++;
                        if(target.length > i ){
                            //コンマ追加
                            sql.append(",");
                        }
                    }
                    sql.append(" ) ");

                }
            }

        }

        sql.append(" ORDER BY ");

        //工事番号指定されている場合
        if(!no.equals(""))
        {
            sql.append(" CASE ");
            sql.append(" WHEN CONST_NO LIKE '" + no + "' THEN 1 ");
            sql.append(" ELSE 0 ");
            sql.append(" END DESC,  ");
            sql.append(" START_DT, ");
            sql.append(" CONST_NO ");
        }
        else
        {
            sql.append(" START_DT, ");
            sql.append(" CONST_NO ");
        }

        return sql.toString();
    }
    /**
     * 工事情報取得SQL
     * @return SQL
     */
    public static String createSQLData(String no){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT");
        sql.append("  CONST_START_DT AS START_DT");
        sql.append("  , CONST_END_DT AS END_DT");
        sql.append("  , CONST_EMPLOYEE_NAME AS EMPLOYEE_NAME");
        sql.append("  , CONST_CONTENTS AS CONTENTS");
        sql.append("  , MNG_LOC_L_CD AS LOCATION");
        sql.append("  , CONST_INFO1 AS INFO1");
        sql.append("  , CONST_INFO2 AS INFO2");
        sql.append("  , CONST_INFO3 AS INFO3");
        sql.append("  , CONST_INFO4 AS INFO4");
        sql.append("  , CONST_INFO5 AS INFO5");
        sql.append("  , CONST_INFO6 AS INFO6");
        sql.append("  , CONST_INFO7 AS INFO7");
        sql.append("  , CONST_INFO8 AS INFO8");
        sql.append("  , CONST_INFO9 AS INFO9");
        sql.append("  , CONST_INFO10 AS INFO10");
        sql.append("  , CONST_INFO11 AS INFO11");
        sql.append("  , CONST_INFO12 AS INFO12");
        sql.append("  , CONST_INFO13 AS INFO13");
        sql.append("  , CONST_INFO14 AS INFO14");
        sql.append("  , CONST_INFO15 AS INFO15");
        sql.append("  , CONST_INFO16 AS INFO16");
        sql.append("  , CONST_INFO17 AS INFO17");
        sql.append("  , CONST_INFO18 AS INFO18");
        sql.append("  , CONST_INFO19 AS INFO19");
        sql.append("  , CONST_INFO20 AS INFO20");
        sql.append("  , CONST_LENT_END_DT AS LENT_END_DT");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_CONSTRUCTION);
        sql.append(" WHERE Const_NO = '"+ no + "'");
        return sql.toString();
    }

    public static String GetTemporaryInfo(SQLiteDatabase db,String no){
        String strTemporaryInfo = CommonConst.TEMPINFO_FLAG.No.getString();
        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  *");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMPORARY);
        sql.append(" WHERE CONST_NO = ?");

        //SQL発行
            Cursor cursor = db.rawQuery(sql.toString(), new String[]{no});
        if (cursor.moveToNext()) {
            //取得する
            strTemporaryInfo = CommonConst.TEMPINFO_FLAG.Yes.getString();      //仮置き情報あり
        }
        cursor.close();

        //Yの場合
        if (strTemporaryInfo == CommonConst.TEMPINFO_FLAG.Yes.getString()){
            return strTemporaryInfo;
        }

        //今回仮置きしたデータ
        //SQL作成 初期化
        sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  *");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_LIST);
        sql.append(" WHERE CONST_NO = ?");

        //SQL発行
        cursor = db.rawQuery(sql.toString(), new String[]{no});
        if (cursor.moveToNext()) {
            //取得する
            strTemporaryInfo = CommonConst.TEMPINFO_FLAG.Yes.getString();      //仮置き情報あり
        }
        cursor.close();

        return strTemporaryInfo;
    }

    public static String GetConstContents(SQLiteDatabase db,String no){
        String strTemporaryInfo = CommonConst.TEMPINFO_FLAG.No.getString();
        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT ");
        sql.append(" CONST_CONTENTS ");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_CONSTRUCTION);
        sql.append(" WHERE CONST_NO = ?");

        //SQL発行


        Cursor cursor = db.rawQuery(sql.toString(), new String[]{no});
        if (cursor.moveToNext()) {
            //取得する
            strTemporaryInfo = CommonConst.TEMPINFO_FLAG.Yes.getString();      //仮置き情報あり
        }
        cursor.close();

        return strTemporaryInfo;
    }
}

