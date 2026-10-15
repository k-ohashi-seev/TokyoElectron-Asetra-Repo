package com.rfid.common;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;
import android.view.Menu;
import android.widget.ArrayAdapter;
import android.widget.ListView;

import com.rfidpk.R;

public class EmployeeList extends AppCompatActivity {

    ListView listView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_employee_list);
        //Toolbar toolbar = findViewById(R.id.toolbar);
        //setSupportActionBar(toolbar);
        SQLiteDatabase db = null;

        try {

            listView = (ListView) findViewById(R.id.employeeListView);

            ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(this,android.R.layout.simple_list_item_1);

            //データベースヘルパーのインスタンスを作成する（まだデータベースはできない）
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            //データベースオブジェクトを取得する（データベースにアクセスすると作成される。）
            db = dbHelper.getWritableDatabase();

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" SELECT");
            sql.append(" EMPLOYEE_CD");
            sql.append(" ,EMPLOYEE_NAME");
            sql.append(" FROM ");
            sql.append(CommonConst.MASTER_EMPLOYEE);

            //社員コード検索
            Cursor cursor = db.rawQuery(sql.toString(), null);

            StringBuilder text = new StringBuilder();
            while (cursor.moveToNext()){

                arrayAdapter.add(cursor.getString(0));
                arrayAdapter.add(cursor.getString(1));
                arrayAdapter.add(cursor.getString(2));
                listView.setAdapter(arrayAdapter);
            }

        } catch (Exception ex){
            ex.printStackTrace();
        } finally {
            db.close();
        }
    }

    /**
     * メニューの設定処理
     * @param menu
     * @return
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_nosetting, menu);
        return true;
    }

    /**
     * メニュー変更イベント処理
     * @param menu
     * @return
     */
    @Override
    public boolean onPrepareOptionsMenu(Menu menu) {
        return true;
    }

}
