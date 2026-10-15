package com.rfid.setting;

import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;

import com.rfid.common.DatabaseHelper;

public class SettingMaster {

    /**
     * 棚卸方法を取得します。
     * @param context コンテキスト
     * @return 棚卸方法 1:ロケ指定なし, 2:ロケ指定あり,　該当なし:""
     */
    public static String getInventoryMethod(Context context) {
        String strData = "";
        SQLiteDatabase db = null;

        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(context);
            db = dbHelper.getWritableDatabase();

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" SELECT");
            sql.append("  DATA1");
            sql.append(" FROM M_SETTING");
            sql.append(" WHERE");
            sql.append("  KEY_DIV = 'Inventory method'");

            //SQL発行
            Cursor cursor = db.rawQuery(sql.toString(), null);

            //取得した棚卸データを棚卸クラスに格納する
            if (cursor.getCount() > 0 ){
                cursor.moveToFirst();
                strData = cursor.getString(0);
            }

            return strData;

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }
}
