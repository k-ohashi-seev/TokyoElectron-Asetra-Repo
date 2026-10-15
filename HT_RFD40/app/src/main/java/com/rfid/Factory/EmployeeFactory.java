package com.rfid.Factory;

import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;

import org.json.JSONArray;
import org.json.JSONException;

import java.util.ArrayList;
import java.util.Arrays;

public class EmployeeFactory {

    /**
     * 社員マスタにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json) throws JSONException {
        Log.d("EmployeeInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.MASTER_EMPLOYEE);
            sql.append(" (");
            sql.append("  EMPLOYEE_CD,");         //社員コード
            sql.append("  EMPLOYEE_NAME,");       //社員名
            sql.append("  AUTH_DIV,");            //権限区分     20210918 ADD Takase V2.0 
            sql.append("  HEAD_OFFICE_FLG");      //本部フラグ   20210918 ADD Takase V2.0 
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                    //社員コード
            sql.append(" ?,");                    //社員名
            sql.append(" ?,");                    //権限区分       20210918 ADD Takase V2.0 
            sql.append(" ?");                     //本部フラグ     20210918 ADD Takase V2.0 
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("employeeCd"),    //社員コード
                    json.getJSONObject(i).getString("employeeName"),  //社員名
                    json.getJSONObject(i).getString("authDiv"),       //社員名		   20210918 ADD Takase V2.0
                    json.getJSONObject(i).getString("headOfficeFlg")  //本部フラグ     20210918 ADD Takase V2.0
            });

            Log.d("EmployeeInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 社員マスタを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("EmployeeDelete", "START");

        //社員マスタの削除
        db.delete(CommonConst.MASTER_EMPLOYEE, null, null);

    }

    /**
     * 社員マスタ検索SQL
     * @return SQL
     */
    public static String Select(){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT");
        sql.append(" EMPLOYEE_CD");
        sql.append(" ,EMPLOYEE_NAME");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_EMPLOYEE);
        sql.append(" WHERE ");
        sql.append(" EMPLOYEE_CD Like ? ");
        sql.append(" ORDER BY EMPLOYEE_CD");
        return sql.toString();
    }

    /**
     * 社員マスタ検索SQL(並べ替え、ロケコード有)
     * @param locList
     * @return
     */
    public static String SelectSort(ArrayList<String> locList){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT DISTINCT");
        sql.append(" EMPLOYEE_CD");
        sql.append(" ,EMPLOYEE_NAME");
        sql.append(" FROM ( ");
        sql.append(" SELECT ");
        sql.append(" E.EMPLOYEE_CD ");
        sql.append(" ,E.EMPLOYEE_NAME ");
        if(locList.size()!=0){  //ログインユーザの所属ロケが存在する場合
            sql.append(", CASE ");
            for(String locCode:locList){
                sql.append(" WHEN EH.HUB_LOC_L_CD = '"+locCode+"' ");
                sql.append(" THEN 1");
            }
            sql.append(" ELSE 2");
            sql.append(" END AS Priority ");
        }
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_EMPLOYEE).append(" E ");
        sql.append(" LEFT JOIN ");
        sql.append(CommonConst.MASTER_EMPLOYEE_HUB).append(" EH ");
        sql.append(" ON E.EMPLOYEE_CD = EH.EMPLOYEE_CD ");
        sql.append(" AND (");
        if(locList.size()!=0){
            int i=1;
            for(String locCode:locList){
                if(i>=2){   //ロケコードが2つ以上ある場合
                    sql.append(" OR EH.HUB_LOC_L_CD = '"+locCode+"'");
                }
                else{
                    sql.append(" EH.HUB_LOC_L_CD = '"+locCode+"'");
                }
                i++;
            }
            sql.append(" )");
        }
        else{
            sql.append(" 1=1 )");
        }
        sql.append(" ) AS SubQuery ");
        sql.append(" WHERE ");
        sql.append(" EMPLOYEE_CD Like ? ");
        sql.append(" ORDER BY ");
        if(locList.size()!=0){
            sql.append(" Priority, ");
        }
        sql.append(" EMPLOYEE_NAME ASC ");
        return sql.toString();
    }

    /**
     * 社員マスタ検索SQL(並べ替え、ロケコード無)
     * @param
     * @return
     */
    public static String SelectSort(){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT DISTINCT");
        sql.append(" E.EMPLOYEE_CD");
        sql.append(" ,E.EMPLOYEE_NAME");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_EMPLOYEE).append(" E ");
        sql.append(" LEFT JOIN ");
        sql.append(CommonConst.MASTER_EMPLOYEE_HUB).append(" EH ");
        sql.append(" ON E.EMPLOYEE_CD = EH.EMPLOYEE_CD");
        //sql.append(" WHERE ");
        //sql.append(" E.EMPLOYEE_CD Like ? ");
        sql.append(" ORDER BY ");
        sql.append(" E.EMPLOYEE_NAME ASC ");
        return sql.toString();
    }

    /**
     *
     * @return
     */
    public static ArrayList<String> GetEmployeeLocCode(String locCode,SQLiteDatabase db){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT");
        sql.append(" HUB_LOC_L_CD ");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_EMPLOYEE_HUB);
        sql.append(" WHERE EMPLOYEE_CD = '"+locCode+"' ");
        Cursor cursor = db.rawQuery(sql.toString(),null);
        ArrayList<String>locCodeList = new ArrayList<>();
        while(cursor.moveToNext()){
            locCodeList.add(cursor.getString(cursor.getColumnIndex("HUB_LOC_L_CD")));
        }
        cursor.close();
        return locCodeList;
    }

    //20210918 ADD Takase V2.0 START
    /**
     * 社員マスタ検索SQL(HT.対象拠点設定時)
     * @return
     */
    //TODO:更新権限(他拠点)を持っているが、HT.対象拠点に所属していない社員は含まれない
    public static String SelectHubEmployee(){
        StringBuffer sql = new StringBuffer();
        sql.append(" SELECT");
        sql.append("  E.EMPLOYEE_CD");
        sql.append(" ,E.EMPLOYEE_NAME");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_EMPLOYEE).append(" E ");
        sql.append(" INNER JOIN ");
        sql.append(CommonConst.MASTER_EMPLOYEE_HUB).append(" EH ");
        sql.append(" ON");
        sql.append("  E.EMPLOYEE_CD = EH.EMPLOYEE_CD");
        sql.append(" WHERE");
        sql.append("  E.EMPLOYEE_CD Like ? ");
        sql.append("  AND EH.HUB_LOC_L_CD = ?");
        //sql.append(" ORDER BY E.EMPLOYEE_CD");
        sql.append(" ORDER BY E.EMPLOYEE_NAME");    //ADD 20241127 shimomura
        return sql.toString();
    }
    //20210918 ADD Takase V2.0 END

    /**
     * 社員名を取得する
     * @param db
     * @param strEmployeeCode
     * @return
     */
    public static String GetEmployeeName(SQLiteDatabase db, String strEmployeeCode){
        boolean blnReturn = true;
        String strName = "";

        if (strEmployeeCode == "") {
            return strName;
        }

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  EMPLOYEE_NAME");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_EMPLOYEE);
        sql.append(" WHERE EMPLOYEE_CD = ?");

        //貸出先検索
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strEmployeeCode});

        cursor.moveToFirst();
        if (cursor.getCount() > 0 ) {
            strName = cursor.getString(cursor.getColumnIndex("EMPLOYEE_NAME"));
        }

        return strName;
    }

    /**
     * 社員名を取得する
     * @param strEmployeeCode
     * @return
     */
    public static String GetEmployeeName(String strEmployeeCode, Context context) {
        String strEmployeeName = "";
        SQLiteDatabase db = null;

        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(context);
            db = dbHelper.getWritableDatabase();

            strEmployeeName = GetEmployeeName(db, strEmployeeCode);

        } finally {
            if (db != null) {
                db.close();
            }
            return strEmployeeName;
        }
    }
}
