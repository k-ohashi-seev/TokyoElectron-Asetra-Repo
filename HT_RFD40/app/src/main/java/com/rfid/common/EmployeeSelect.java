package com.rfid.common;

import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.util.TypedValue;
import android.view.Menu;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ListView;
import android.widget.SimpleAdapter;
import android.widget.TextView;

import com.rfid.main.LoginMn;
import com.rfidpk.R;
import com.rfid.Factory.EmployeeFactory;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * 社員一覧表示画面のクラスです。
 * 社員マスタからデータを取得し、一覧表示します。
 * 一覧のデータを選択すると呼出元画面に戻ります。
 */
public class EmployeeSelect extends BaseActivity {

    /* 一覧表示 */
    ListView listView = null;

    /**
     * 画面表示処理
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_employee_list);

        //タイトルバー 戻るボタン表示
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        //DB接続用変数
        SQLiteDatabase db = null;

        try {

            //Vieweを初期化する
            listView = (ListView) findViewById(R.id.employeeListView);

            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //社員マスタからデータを取得し一覧表示する
            ViewEmployeeData(db);

            //文言表示
            InitializeWord();

            //一覧表示のクリック処理
            listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                @Override
                public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                    //選択データを取得する
                    ListView listView = (ListView)parent;
                    Map<String, String> map = (HashMap)listView.getItemAtPosition(position);

                    //呼出元に返却するデータを設定する
                    Intent intent = new Intent();
                    intent.putExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD, map.get(CommonConst.List_VIEW_KEY.Main.getString()));
                    intent.putExtra(CommonConst.VIEW_KEY_EMPLOYEE_NAME, map.get(CommonConst.List_VIEW_KEY.Sub.getString()));
                    setResult(RESULT_OK, intent);

                    // このアクティビティを終了する
                    finish();
                }
            });

        } catch (Exception ex){
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            db.close();
        }
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_EMPLOYEE,"",this));
    }

    /**
     * 社員マスタからデータを取得し、一覧表示する
     * @param db DB接続
     */
    private void ViewEmployeeData(SQLiteDatabase db) {

        //SQL作成
        String strSql = new String();
        List<String> list = new ArrayList<>();
        //strSql = EmployeeFactory.Select();   //20210918 DEL Takase V2.0

        //社員マスタからデータを取得
        //20210918 DEL Takase V2.0 START
        //String strEmploryyCd = getIntent().getStringExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD);
        //Cursor cursor = db.rawQuery(strSql, new String[]{strEmploryyCd + "%"});
        //20210918 DEL Takase V2.0 END
        
        //20210918 ADD Takase V2.0 START
        String strEmployeeCd = getIntent().getStringExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD);
        String loginuserCd = SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD,"",this);

        //HT.対象拠点を取得
        Intent intent = getIntent();
        String strHtHub = intent.getStringExtra(CommonConst.VIEW_KEY_HT_HUB);
        Cursor cursor;
        String empCodeCheck = C_Setting.getEmployeePrevSet(this);
        ArrayList locCodeList;
//        if(empCodeCheck.equals("true")){
//            locCodeList = EmployeeFactory.GetEmployeeLocCode(loginuserCd,db);
//        }
//        else{
//            locCodeList = EmployeeFactory.GetEmployeeLocCode(LoginMn.LOGIN_USER_CD,db);
//        }
        //全開選択していたロケコード取得
        locCodeList = EmployeeFactory.GetEmployeeLocCode(loginuserCd,db);

        if (strHtHub.equals("")) {
            //HT.対象拠点が設定されていない場合
            //strSql = EmployeeFactory.Select();
            strSql = EmployeeFactory.SelectSort(locCodeList);
            cursor = db.rawQuery(strSql, new String[]{strEmployeeCd + "%"});
        } else {
            //HT.対象拠点が設定されている場合
            strSql = EmployeeFactory.SelectHubEmployee();
            cursor = db.rawQuery(strSql, new String[]{strEmployeeCd + "%", strHtHub});
        }
        //20210918 ADD Takase V2.0 END

        //取得したデータを一覧表示に設定
        List<Map<String, String>> listview = new ArrayList<Map<String, String>>();
        while (cursor.moveToNext()){
            //表示データを設定する
            Map<String, String> map = new HashMap<String, String>();
            map.put(CommonConst.List_VIEW_KEY.Main.getString(), cursor.getString(0));   //社員コード
            map.put(CommonConst.List_VIEW_KEY.Sub.getString(), cursor.getString(1));    //社員名称
            listview.add(map);
        }

        //初期化
        SimpleAdapter arrayAdapter = InitializeAdapter(listview);
        ListView listView = (ListView) findViewById(R.id.employeeListView);

        //リストに追加
        if (listView != null) {
            listView.setAdapter(arrayAdapter);
        }

        //件数を表示
        TextView txtView = (TextView)findViewById(R.id.lblListViewCount);
        txtView.setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_COUNT, "", this) + ": " + cursor.getCount() );
    }

    /**
     * 一覧表示の初期化処理
     * @return 一覧表示用Adapter
     */
    private SimpleAdapter InitializeAdapter(List list){
        SimpleAdapter adapter = new SimpleAdapter(
                this,
                list,
                android.R.layout.simple_list_item_2,
                new String[] {CommonConst.List_VIEW_KEY.Main.getString(), CommonConst.List_VIEW_KEY.Sub.getString()},
                new int[] {android.R.id.text1, android.R.id.text2}
        ){
            @Override
            public View getView(int position, View convertView, ViewGroup parent){
                View view = super.getView(position, convertView, parent);

                if (position % 2 == 0){
                    view.setBackgroundResource(R.color.ListViewBack);
                    
                //20210918 ADD Takase V2.0 START
                } else {
                    view.setBackgroundResource(R.color.ListViewBack2);
                }
                //20210918 ADD Takase V2.0 END

                //20230221 ADD Sato V2.3 Start
                if (Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString())) {
                    TextView first = view.findViewById(android.R.id.text1);
                    TextView second = view.findViewById(android.R.id.text2);
                    first.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_first_line_text));
                    second.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
                }
                //20230221 ADD Sato V2.3 End
                return view;
            }
        };

        return adapter;
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

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }
}
