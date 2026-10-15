package com.rfid.common;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;

import android.content.ContentValues;
import android.content.Context;
import android.content.res.AssetManager;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

/**
 * DB接続用のヘルパークラスです。
 */
public class DatabaseHelper extends SQLiteOpenHelper {

    /* データベース名 */
    private final static String DB_NAME = "RFIDDB";
    private final static int DB_VER = 6;    // ※DBに変更を加えた際は必ずこのバージョンを上げること
                                            // ※バージョンを上げないとアプリのアップデート時に
    /* コンテキスト */
    private Context mContext;

    /**
     * コンストラクタ
     */
    public DatabaseHelper(Context context) {
        super(context, DB_NAME, null, DB_VER);
        mContext = context;
    }

    /**
     * データベースが作成された時に呼ばれます。
     * assets/sql/create内に定義されているsqlを実行します。
     */
    @Override
    public void onCreate(SQLiteDatabase db) {
        try {
            execSql(db,"sql/create");
            execCSV(db, "sql/csv");
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    /**
     * データベースをバージョンアップした時に呼ばれます。
     * assets/sql/drop内に定義されているsqlを実行します。
     * その後onCreate()メソッドを呼び出します。
     */
    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        try {
            execSql(db,"sql/drop");
        } catch (IOException e) {
            e.printStackTrace();
        }
        onCreate(db);
    }

    /**
     * 引数に指定したassetsフォルダ内のsqlを実行します。
     * @param db データベース
     * @param assetsDir assetsフォルダ内のフォルダのパス
     * @throws IOException
     */
    private void execSql(SQLiteDatabase db,String assetsDir) throws IOException {
        AssetManager as = mContext.getResources().getAssets();
        try {
            String files[] = as.list(assetsDir);
            for (int i = 0; i < files.length; i++) {
                String str = readFile(as.open(assetsDir + "/" + files[i]));
                str = str.replace("\n", "");
                str = str.replace("\t", "");
                for (String sql: str.split(";")){
                    db.execSQL(sql);
                }
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    /**
     * 引数に指定したassetsフォルダ内のsqlを実行します。
     * @param db データベース
     * @param assetsDir assetsフォルダ内のフォルダのパス
     * @throws IOException
     */
    private void execCSV(SQLiteDatabase db,String assetsDir) throws IOException {
        AssetManager as = mContext.getResources().getAssets();
        try {
            ContentValues value = new ContentValues();
            String files[] = as.list(assetsDir);
            for (int i = 0; i < files.length; i++) {
                BufferedReader br = new BufferedReader(new InputStreamReader(as.open(assetsDir + "/" + files[i]), "UTF-8"));
                String line;
                while((line = br.readLine()) != null){
                    StringBuffer bfSQL = new StringBuffer();
                    bfSQL.append("INSERT INTO");
                    bfSQL.append(" " +  files[i].substring(0, files[i].length() - 4));
                    bfSQL.append(" VALUES(");
                    for (String strData: line.split(",")) {
                        bfSQL.append(strData);
                        bfSQL.append(", ");
                    }
                    String strSQL = bfSQL.toString();
                    strSQL = strSQL.substring(0, bfSQL.length() - 2);
                    strSQL = strSQL + ")";
                    db.execSQL(strSQL);
                }
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    /**
     * ファイルから文字列を読み込みます。
     * @param is
     * @return ファイルの文字列
     * @throws IOException
     */
    private String readFile(InputStream is) throws IOException{
        BufferedReader br = null;
        try {
            br = new BufferedReader(new InputStreamReader(is,"UTF-8"));

            StringBuilder sb = new StringBuilder();
            String str;
            while((str = br.readLine()) != null){
                sb.append(str +"\n");
            }
            return sb.toString();
        } finally {
            if (br != null) br.close();
        }
    }

}