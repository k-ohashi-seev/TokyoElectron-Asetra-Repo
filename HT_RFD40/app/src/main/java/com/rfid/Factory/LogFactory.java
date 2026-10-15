package com.rfid.Factory;

import android.app.Activity;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;
import android.widget.TextView;

import com.rfid.common.Common;
import com.rfidpk.R;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.EntryLogData;
import com.rfid.common.bean.EntryLogDataDetail;
import com.rfid.common.bean.EntryProcessData;
import com.rfid.common.bean.EntryProcessDataDetail;

import java.nio.charset.Charset;

public class LogFactory {

//    /** ログファイル */
//    private static LogFile _log = null;

    /**
     * 操作ログデータを取得する
     * @param db
     * @param kind
     */
    public EntryLogData SelectData(SQLiteDatabase db, CommonConst.LOG_KIND kind){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  OPE_LOG_ID");                     //操作ログID
        sql.append("  ,PROCESS_DT");                    //処理日
        sql.append("  ,PROCESS_TM");                    //処理時刻
        sql.append("  ,PROCESS_TOOL_DIV");             //処理ツール区分
        sql.append("  ,PROCESS_CAT");                   //処理種別
        sql.append("  ,PROCESS_CONTENTS");             //処理内容
        sql.append("  ,OPE_EMPLOYEE_CD");              //操作者社員コード
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_OPERATION_LOG);
        sql.append(" WHERE LOG_KIND = ? ");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{kind.getString()});
        try {

            //取得したデータをログクラスに格納する
            EntryLogData map = new EntryLogData();
            while (cursor.moveToNext()) {
                EntryLogDataDetail data = new EntryLogDataDetail();

                //各項目を設定する
                data.setOpeLogId(cursor.getString(cursor.getColumnIndex("OPE_LOG_ID")));                //操作ログID
                data.setProcessDt(cursor.getString(cursor.getColumnIndex("PROCESS_DT")));               //処理日
                data.setProcessTm(cursor.getString(cursor.getColumnIndex("PROCESS_TM")));               //処理時刻
                data.setProcessToolDiv(cursor.getString(cursor.getColumnIndex("PROCESS_TOOL_DIV")));   //処理ツール区分
                data.setProcessCat(cursor.getString(cursor.getColumnIndex("PROCESS_CAT")));             //処理種別
                data.setProcessContent(cursor.getString(cursor.getColumnIndex("PROCESS_CONTENTS")));   //処理内容
                data.setOpeEmployeeCd(cursor.getString(cursor.getColumnIndex("OPE_EMPLOYEE_CD")));      //操作者社員コード

                //溜め込む
                map.setlogDataDetailList(data);
            }

            return map;
        } finally {
            cursor.close();
        }
    }

    /**
     * 操作エラーログデータを取得する
     * @param db
     */
    public EntryLogData SelectErrorData(SQLiteDatabase db){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  OPE_LOG_ID");                     //操作ログID
        sql.append("  ,PROCESS_DT");                    //処理日
        sql.append("  ,PROCESS_TM");                    //処理時刻
        sql.append("  ,PROCESS_TOOL_DIV");             //処理ツール区分
        sql.append("  ,PROCESS_CAT");                   //処理種別
        sql.append("  ,PROCESS_CONTENTS");             //処理内容
        sql.append("  ,OPE_EMPLOYEE_CD");              //操作者社員コード
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_OPERATION_LOG);
        sql.append(" WHERE PROCESS_CAT = ? ");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{CommonConst.PROCESS_CAT.Error.getString()});
        try {

            //取得したデータをログクラスに格納する
            EntryLogData map = new EntryLogData();
            while (cursor.moveToNext()) {
                EntryLogDataDetail data = new EntryLogDataDetail();

                //各項目を設定する
                data.setOpeLogId(cursor.getString(cursor.getColumnIndex("OPE_LOG_ID")));                //操作ログID
                data.setProcessDt(cursor.getString(cursor.getColumnIndex("PROCESS_DT")));               //処理日
                data.setProcessTm(cursor.getString(cursor.getColumnIndex("PROCESS_TM")));               //処理時刻
                data.setProcessToolDiv(cursor.getString(cursor.getColumnIndex("PROCESS_TOOL_DIV")));   //処理ツール区分
                data.setProcessCat(cursor.getString(cursor.getColumnIndex("PROCESS_CAT")));             //処理種別
                data.setProcessContent(cursor.getString(cursor.getColumnIndex("PROCESS_CONTENTS")));   //処理内容
                data.setOpeEmployeeCd(cursor.getString(cursor.getColumnIndex("OPE_EMPLOYEE_CD")));      //操作者社員コード

                //溜め込む
                map.setlogDataDetailList(data);
            }

            return map;
        } finally {
            cursor.close();
        }
    }

    /**
     * 操作ログテーブル登録
     * @param db
     */
    public static void Insert(SQLiteDatabase db, EntryLogDataDetail value){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" INSERT");
        sql.append(" INTO ");
        sql.append(CommonConst.TABLE_OPERATION_LOG);
        sql.append(" (");
        sql.append("  PROCESS_DT");                     //処理日
        sql.append("  ,PROCESS_TM");                    //処理時刻
        sql.append("  ,PROCESS_TOOL_DIV");             //処理ツール区分
        sql.append("  ,PROCESS_CAT");                   //処理種別
        sql.append("  ,PROCESS_CONTENTS");             //処理内容
        sql.append("  ,OPE_EMPLOYEE_CD");              //操作者社員コード
        sql.append("  ,LOG_KIND");                      //ログ種別
        sql.append("  ) ");
        sql.append(" VALUES (");
        //2021/11/08 UPD Start Suzuki
        //sql.append(" strftime('%Y%m%d', 'now'),");
        sql.append(" strftime('%Y%m%d', datetime('now', 'localtime')),");
        //2021/11/08 UPD End Suzuki
        sql.append(" replace(strftime('%H%M%f', datetime('now', 'localtime')), '.', ''),");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?");
        sql.append(" )");

        //SQL発行
        String strContent = Common.substring(value.getProcessContents(),  CommonConst.CONTENT_SIZE, Charset.forName("UTF-8"));
        db.execSQL(sql.toString(), new String[]{
                CommonConst.WORD_TOOL_DIV.H.getString(),    //処理ツール区分
                value.getProcessCat(),              //処理種別
                strContent,                         //処理内容
                value.getOpeEmployeeCd(),           //操作者社員コード
                value.getLogKind()                  //ログ種別
        });

    }

    /**
     * 操作ログテーブル登録
     * @param db
     * @param Category
     * @param strContent
     * @param activity
     */
    public static void Insert(SQLiteDatabase db, CommonConst.PROCESS_CAT Category, String strContent, Activity activity){
        WriteFileLog(Category, strContent, activity);

        EntryLogDataDetail value = new EntryLogDataDetail();
        value.setProcessCat(Category.getString());      //処理種別
        value.setProcessContent(strContent);            //処理内容
        value.setOpeEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD , "", activity));            //操作者社員コード
        value.setLogKind(CommonConst.LOG_KIND.Process.getString());     //ログ種別
        Insert(db, value);
    }

    /**
     * 操作ログテーブル登録
     * @param db
     * @param Category
     * @param strContent
     * @param logKind
     * @param activity
     */
    public static void Insert(SQLiteDatabase db, CommonConst.PROCESS_CAT Category, String strContent, CommonConst.LOG_KIND logKind, Activity activity){
        WriteFileLog(Category, strContent, activity);

        EntryLogDataDetail value = new EntryLogDataDetail();
        value.setProcessCat(Category.getString());      //処理種別
        value.setProcessContent(strContent);            //処理内容
        value.setOpeEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD , "", activity));            //操作者社員コード
        value.setLogKind(logKind.getString());     //ログ種別
        Insert(db, value);
    }

    /**
     * 操作ログテーブル登録
     * @param Category
     * @param strContent
     * @param activity
     */
    public static void Insert(CommonConst.PROCESS_CAT Category, String strContent, Activity activity){
        WriteFileLog(Category, strContent, activity);

        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(activity);
            db = dbHelper.getWritableDatabase();

            db.beginTransaction();

            EntryLogDataDetail value = new EntryLogDataDetail();
            value.setProcessCat(Category.getString());                      //処理種別
            value.setProcessContent(strContent);                            //処理内容
            value.setOpeEmployeeCd(SystemSetting.getString(
                    CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD , "", activity));            //操作者社員コード
            value.setLogKind(CommonConst.LOG_KIND.Process.getString());     //ログ種別
            Insert(db, value);

            db.setTransactionSuccessful();
            db.endTransaction();

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 操作ログテーブル登録
     * @param Category
     * @param strContent
     * @param logKind
     * @param activity
     */
    public static void Insert(CommonConst.PROCESS_CAT Category, String strContent, CommonConst.LOG_KIND logKind, Activity activity){
        WriteFileLog(Category, strContent, activity);

        SQLiteDatabase db = null;
        try {

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(activity);
            db = dbHelper.getWritableDatabase();

            EntryLogDataDetail value = new EntryLogDataDetail();
            value.setProcessCat(Category.getString());                      //処理種別
            value.setProcessContent(strContent);                            //処理内容
            value.setOpeEmployeeCd(SystemSetting.getString(
                    CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD , "", activity));            //操作者社員コード
            value.setLogKind(logKind.getString());                         //ログ種別
            Insert(db, value);

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 棚卸の操作ログデータをクリアする
     */
    public void ClearInv(SQLiteDatabase db){

        //操作ログデータテーブルの削除
        db.delete(CommonConst.TABLE_OPERATION_LOG, "LOG_KIND = ?", new String[]{CommonConst.LOG_KIND.Inventory.getString()});

    }

    /**
     * 処理履歴の操作ログデータをクリアする
     */
    public void ClearProcess(SQLiteDatabase db){

        //操作ログデータテーブルの削除
        db.delete(CommonConst.TABLE_OPERATION_LOG, "LOG_KIND = ?", new String[]{CommonConst.LOG_KIND.Process.getString()});

    }

    /**
     * エラー操作ログデータをクリアする
     */
    public void ClearErrorLog(SQLiteDatabase db){

        //操作ログデータテーブルの削除
        db.delete(CommonConst.TABLE_OPERATION_LOG, "PROCESS_CAT = ?", new String[]{CommonConst.PROCESS_CAT.Error.getString()});

    }

    /**
     * 件数を取得する
     */
    public static long Count(Activity activity) {
        SQLiteDatabase db = null;
        try {
            long longCount = 0;

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(activity);
            db = dbHelper.getWritableDatabase();

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" SELECT ");
            sql.append("  COUNT(*) AS COUNT ");
            sql.append(" FROM ");
            sql.append(CommonConst.TABLE_OPERATION_LOG);

            //SQL発行
            Cursor cursor = db.rawQuery(sql.toString(), null);
            try {

                //件数を取得する
                while (cursor.moveToNext()) {
                    longCount = cursor.getLong(cursor.getColumnIndex("COUNT"));
                }

                return longCount;
            } finally {
                cursor.close();
            }

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    public static void WriteFileLog(CommonConst.PROCESS_CAT Category, String strContent, Activity activity) {
        try {
            //ログファイル作成
//            _log = LogFile.getInstance(activity.getClass());
//            _log.logConfigure(activity);
//            switch (Category){
//                case Error:
//                    _log.error(strContent);
//                    break;
//                case Operation:
//                case Event:
//                    _log.info(strContent);
//                    break;
//            }
        } catch (Exception ex) {
            // 無視
        }
    }
}
