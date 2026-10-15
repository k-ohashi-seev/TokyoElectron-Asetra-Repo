package com.rfid.common;

import android.annotation.SuppressLint;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.util.TypedValue;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Adapter;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.SimpleAdapter;
import android.widget.TextView;

import com.rfid.Factory.LocationFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.main.LoginMn;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;
import com.rfid.common.BaseActivity;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * ロケデータの一覧表示画面のクラスです。
 * ロケマスタからデータを取得し、一覧表示します。
 * 一覧のデータを選択すると呼出元画面に戻ります。
 */
public class LocationSelect extends BaseActivity implements CommonConfirmDialog.Callback {

    /* 一覧表示 */
    ListView listView = null;

    private final static int ACTION_LOGOUT = 1;

    CommonConfirmDialog.Builder  _confirmDialog = null;

    /**
     * 画面表示処理
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_location_list);

        //タイトルバー 戻るボタン表示
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        //DB接続用変数
        SQLiteDatabase db = null;

        _confirmDialog = new CommonConfirmDialog.Builder(this);

        try {

            //呼出元画面からロケ区分を取得する
            Intent intent = getIntent();
            String strLocLKbn = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_L_DATA);
            String strLocMKbn = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_M_DATA);
            String strLocSKbn = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_S_DATA);
            String strTitle = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_TITLE);
            String strLocSelect = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_SELECT);

            //呼出元画面からタイトルを取得し表示する
            ((TextView)findViewById(R.id.lblListViewTitle)).setText(intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_TITLE));

            //Vieweを初期化する
            listView = (ListView) findViewById(R.id.locationListView);

            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //ロケマスタからデータを取得し一覧表示する
            ViewLocationData(db, strLocLKbn, strLocMKbn, strLocSKbn, strLocSelect);

            //文言表示
            InitializeWord();

            //一覧表示のクリック処理
            listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                @Override
                public void onItemClick(AdapterView<?> parent, View view, int position, long id) {

                    if (checkTimeout()) {return;}

                    //選択データを取得する
                    ListView listView = (ListView)parent;
                    Map<String, String> map = (HashMap)listView.getItemAtPosition(position);

                    //呼出元に返却するデータを設定する
                    Intent intent = new Intent();
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_CD, map.get(CommonConst.List_VIEW_KEY.Main.getString()));
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION, map.get(CommonConst.List_VIEW_KEY.Sub.getString()));
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

    // レジュームイベント
    @Override
    public void onResume() {
        super.onResume();
    }

    /**
     *  ポーズイベント
     */
    @Override
    public void onPause() {
        try {
            super.onPause();

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" + Common.toString(ex), this);
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_LOCATION_CHOICE,"",this));
    }

    /**
     * ロケーションマスタからデータを取得し、一覧表示する
     * @param db DB接続
     * @param strLocLKbn ロケ大区分
     * @param strLocMKbn ロケ中区分
     * @param strLocSKbn ロケ小区分
     * @param strLocSelect 選択切替
     */
    private void ViewLocationData(SQLiteDatabase db, String strLocLKbn, String strLocMKbn, String strLocSKbn, String strLocSelect) {

        //SQL作成
        String strSql = new String();
        List<String> list = new ArrayList<>();

        if (CommonConst.LOCATION_DIV.L.getString().equals(strLocSelect)) {
            //ロケ大区分の指定がない場合
            //20210918 CHG Takase V2.0
            if (CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_STORAGE.getString())) {
                //保管の場合
                strSql = LocationFactory.createSQLData();
            } else {
                if (CommonAuthority.Auth.updHubAuthFlg.equals(CommonConst.AUTH_FLG.NO.getString())) {
                    //更新権限(他拠点)を持っていない場合
                    strSql = LocationFactory.createSQLDataHub(CommonAuthority.Auth.updHubList);
                } else {
                    strSql = LocationFactory.createSQLData();
                }
            }
            //20210918 CHG Takase V2.0 END
            list.add(CommonConst.LOCATION_DIV.L.getString());
            list.add(strLocLKbn + "%");
        } else if (CommonConst.LOCATION_DIV.M.getString().equals(strLocSelect)) {
            //ロケ中区分の指定がない場合
            strSql = LocationFactory.createSQLDataL();
            list.add(CommonConst.LOCATION_DIV.M.getString());
            list.add(strLocLKbn);
            list.add(strLocMKbn + "%");
        } else if (CommonConst.LOCATION_DIV.S.getString().equals(strLocSelect)) {
            //ロケ小区分の指定がない場合
            strSql = LocationFactory.createSQLDataM();
            list.add(CommonConst.LOCATION_DIV.S.getString());
            list.add(strLocLKbn);
            list.add(strLocMKbn);
            list.add(strLocSKbn + "%");
        }

        //検索条件がない場合、nullを設定
        String[] strWhere = list.toArray(new String[list.size()]);
        if (strWhere.length == 0){
            strWhere = null;
        }

        //ロケマスタからデータを取得
        Cursor cursor = db.rawQuery(strSql, strWhere);

        //取得したデータを一覧表示に設定
        List<Map<String, String>> listview = new ArrayList<Map<String, String>>();
        while (cursor.moveToNext()){
            //表示データを設定する
            Map<String, String> map = new HashMap<String, String>();
            map.put(CommonConst.List_VIEW_KEY.Main.getString(), cursor.getString(0));   //ロケコード
            map.put(CommonConst.List_VIEW_KEY.Sub.getString(), cursor.getString(1));    //ロケ名称
            listview.add(map);
        }

        //初期化
        SimpleAdapter arrayAdapter = InitializeAdapter(listview);
        ListView listView = (ListView) findViewById(R.id.locationListView);

        //リストに追加
        listView.setAdapter(arrayAdapter);

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
                } else {
                    view.setBackgroundResource(R.color.ListViewBack2);
                }

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
     * メニュー選択イベント
     * @param item
     * @return
     */
    @SuppressLint("NonConstantResourceId")
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        try {
            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            int id = item.getItemId();
            switch(id){
                case R.id.action_logout:
                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, ACTION_LOGOUT);
                    _confirmDialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGOUT, "", this));  //ログアウト
                    _confirmDialog.message(SystemSetting.getString(
                            CommonConst.VIEW_WORD_LOGOUT, "", this));                //ログアウトします。よろしいですか？
                    _confirmDialog.params(bundle);
                    _confirmDialog.show();

            }

            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }

    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        if (params == null) {
            return;
        }

        //呼出元で分岐する
        switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
            case ACTION_LOGOUT:      //終了ボタン
                //ログイン画面を開く
                Intent intent = new Intent(getApplication(), LoginMn.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                startActivity(intent);
        }
    }
}
