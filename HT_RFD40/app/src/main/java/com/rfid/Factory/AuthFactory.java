package com.rfid.Factory;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConst;

import org.json.JSONArray;
import org.json.JSONException;

public class AuthFactory {

    /**
     * 権限マスタにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void Insert(SQLiteDatabase db, JSONArray json) throws JSONException {
        Log.d("AuthInsert", "START");

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO ");
            sql.append(CommonConst.MASTER_AUTH);
            sql.append(" (");
            sql.append("  FUNCTION_ID,");          //機能ID
            sql.append("  AUTH_DIV,");             //権限区分
            sql.append("  AUTH_TYPE,");            //権限タイプ
            sql.append("  HEAD_OFFICE_FLG,");      //本部フラグ
            sql.append("  READ_AUTH_FLG,");        //参照権限
            sql.append("  UPD_AUTH_FLG,");         //更新権限
            sql.append("  READ_HUB_AUTH_FLG,");    //参照権限(他拠点)
            sql.append("  UPD_HUB_AUTH_FLG,");      //更新権限(他拠点)
            sql.append("  AUTH_OPTION");           //特殊権限
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                     //機能ID
            sql.append(" ?,");                     //権限区分
            sql.append(" ?,");                     //権限タイプ
            sql.append(" ?,");                     //本部フラグ
            sql.append(" ?,");                     //参照権限
            sql.append(" ?,");                     //更新権限
            sql.append(" ?,");                     //参照権限(他拠点)
            sql.append(" ?,");                     //更新権限(他拠点)
            sql.append(" ?");                      //特殊権限
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("functionId"),      //機能ID
                    json.getJSONObject(i).getString("authDiv"),         //権限区分
                    json.getJSONObject(i).getString("authType"),        //権限タイプ
                    json.getJSONObject(i).getString("headOfficeFlg"),   //本部フラグ
                    json.getJSONObject(i).getString("readAuthFlg"),     //参照権限
                    json.getJSONObject(i).getString("updAuthFlg"),      //更新権限
                    json.getJSONObject(i).getString("readHubAuthFlg"),  //参照権限(他拠点)
                    json.getJSONObject(i).getString("updHubAuthFlg"),   //更新権限(他拠点)
                    json.getJSONObject(i).getString("authOption"),      //特殊権限
            });

            Log.d("AuthInsert:" + i, json.getJSONObject(i).toString());
        }
    }

    /**
     * 権限マスタを削除する
     * @param db
     */
    public static void Delete(SQLiteDatabase db) {
        Log.d("AuthDelete", "START");

        //権限マスタの削除
        db.delete(CommonConst.MASTER_AUTH, null, null);

    }

    /**
     * 権限を取得する
     * @param db
     * @param strEmployeeCode　社員コード
     * @param strFunctionId　機能ID
     */
    public static void GetAuth(SQLiteDatabase db, String strEmployeeCode, String strFunctionId) {

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("   AUTH.READ_AUTH_FLG");
        sql.append("  ,AUTH.UPD_AUTH_FLG");
        sql.append("  ,AUTH.READ_HUB_AUTH_FLG");
        sql.append("  ,AUTH.UPD_HUB_AUTH_FLG");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_AUTH).append(" AUTH ");
        sql.append(" INNER JOIN ");
        sql.append(CommonConst.MASTER_EMPLOYEE).append(" EMP ");
        sql.append(" ON ");
        sql.append("  AUTH.AUTH_DIV = EMP.AUTH_DIV ");
        sql.append(" WHERE ");
        sql.append("  EMP.EMPLOYEE_CD = ? ");
        sql.append("  AND AUTH.FUNCTION_ID = ? ");
        sql.append("  AND AUTH.AUTH_DIV = EMP.AUTH_DIV ");
        sql.append("  AND AUTH.HEAD_OFFICE_FLG = EMP.HEAD_OFFICE_FLG ");
        sql.append("  AND AUTH.AUTH_TYPE = 'H' ");     //拠点権限の場合

        //権限検索
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strEmployeeCode, strFunctionId});

        cursor.moveToFirst();
        if (cursor.getCount() > 0 ) {
            CommonAuthority.Auth auth = new CommonAuthority.Auth();
            auth.readAuthFlg = cursor.getString(cursor.getColumnIndex("READ_AUTH_FLG"));
            auth.updAuthFlg = cursor.getString(cursor.getColumnIndex("UPD_AUTH_FLG"));
            auth.readHubAuthFlg = cursor.getString(cursor.getColumnIndex("READ_HUB_AUTH_FLG"));
            auth.updHubAuthFlg = cursor.getString(cursor.getColumnIndex("UPD_HUB_AUTH_FLG"));
        }
    }
}

