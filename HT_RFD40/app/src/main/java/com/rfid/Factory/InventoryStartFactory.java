package com.rfid.Factory;

import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.CommonConst;
import com.rfid.setting.SettingValue;

import org.json.JSONArray;
import org.json.JSONException;

public class InventoryStartFactory {
        /**
         * 棚卸開始情報テーブルを読み込みます。
         */
        public static String Select() {
                StringBuffer sql = new StringBuffer();
                sql.append(" SELECT");
                sql.append("  INV_LOC_L_CD");
                sql.append(" FROM ");
                sql.append(CommonConst.TABLE_INVENTORY_START);
                sql.append(" WHERE ");
                sql.append("  INV_LOC_L_CD = ? ");
                return sql.toString();
        }


        /**
         * 棚卸開始情報テーブルにデータを登録する
         * @param db
         * @param json　リクエストして帰ってきたテーブル
         * @throws JSONException
         */
        public static void Insert(SQLiteDatabase db, JSONArray json) throws JSONException {
                Log.d("InventoryStartInsert", "START");

                //件数分ループして登録する
                for (int i = 0; i < json.length(); i++) {

                        //SQL作成
                        StringBuilder sql = new StringBuilder();
                        sql.append(" INSERT");
                        sql.append(" INTO ");
                        sql.append(CommonConst.TABLE_INVENTORY_START);
                        sql.append(" (");
                        sql.append("  INV_LOC_L_CD");         //棚卸開始大ロケコード
                        sql.append("  ) ");
                        sql.append(" VALUES (");
                        sql.append(" ?");                     //棚卸開始大ロケコード
                        sql.append(" )");

                        //SQL発行
                        db.execSQL(sql.toString(), new String[]{
                                json.getJSONObject(i).getString("invLocLCd"),    //棚卸開始大ロケコード
                        });

                        Log.d("InventoryStartInsert:" + i, json.getJSONObject(i).toString());
                }
        }

        /**
         * 棚卸開始情報テーブルを削除する
         * @param db sqliteを操作するためのインスタンス
         */
        public static void Delete(SQLiteDatabase db) {
                Log.d("InventoryStartDelete", "START");

                //棚卸開始情報テーブルの削除
                db.delete(CommonConst.TABLE_INVENTORY_START, null, null);
        }
}


