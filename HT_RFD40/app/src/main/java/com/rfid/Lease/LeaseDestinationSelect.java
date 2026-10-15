package com.rfid.Lease;

import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.util.TypedValue;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ListView;
import android.widget.SimpleAdapter;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.common.Common;
import com.rfidpk.R;
import com.rfid.Factory.LeaseFactory;
import com.rfid.Factory.ProcessFactory;
import com.rfid.common.BaseActivity;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.EntryProcessDataDetail;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * 貸出先の一覧表示画面のクラスです。
 * 貸出先マスタからデータを取得し、一覧表示します。
 * 一覧のデータを選択すると呼出元画面に戻ります。
 */
public class LeaseDestinationSelect extends BaseActivity {

    /* 一覧表示 */
    ListView listView = null;

    /**
     * 画面表示処理
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        setContentView(R.layout.activity_lease_list);

        //タイトルバー 戻るボタン表示
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        //DB接続用変数
        SQLiteDatabase db = null;

        try {

            //Vieweを初期化する
            listView = (ListView) findViewById(R.id.leaseListView);

            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //受渡データを取得する
            Intent intent = getIntent();
            String strLeaseCode = intent.getStringExtra(CommonConst.VIEW_KEY_LEASE_CD);

            //貸出先マスタからデータを取得し一覧表示する
            ViewLeaseData(db, strLeaseCode);

            //文言表示
            InitializeWord();

            //一覧表示のクリック処理
            listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                @Override
                public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                    //選択データを取得する
                    GetSelectListData(parent, position);

                    // このアクティビティを終了する
                    finish();
                }
            });

        } catch (Exception ex){
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING,"",this));
    }

    /**
     * 貸出先マスタからデータを取得し、一覧表示する
     * @param db DB接続
     * @param strLeaseCode 貸出先コード
     */
    private void ViewLeaseData(SQLiteDatabase db, String strLeaseCode) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //SQL作成
        String strSql = LeaseFactory.Select();

        //貸出先マスタからデータを取得
        Cursor cursor = db.rawQuery(strSql, new String[]{strLeaseCode + "%"});

        //取得したデータを一覧表示に設定
        List<Map<String, String>> listview = new ArrayList<Map<String, String>>();
        while (cursor.moveToNext()){
            //表示データを設定する
            Map<String, String> map = new HashMap<String, String>();
            map.put(CommonConst.List_VIEW_KEY.Main.getString(), cursor.getString(0));   //貸出先コード
            map.put(CommonConst.List_VIEW_KEY.Sub.getString(), cursor.getString(1));    //貸出先名称
            listview.add(map);
        }

        //初期化
        SimpleAdapter arrayAdapter = InitializeAdapter(listview);
        ListView listView = (ListView) findViewById(R.id.leaseListView);

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
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

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
     * リストビューの選択されたデータを取得する
     * @param parent
     * @param position
     */
    private void GetSelectListData(AdapterView<?> parent, int position) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        ListView listView = (ListView)parent;
        Map<String, String> map = (HashMap)listView.getItemAtPosition(position);

        //呼出元に返却するデータを設定する
        Intent intent = new Intent();
        intent.putExtra(CommonConst.VIEW_KEY_LEASE_CD, map.get(CommonConst.List_VIEW_KEY.Main.getString()));
        intent.putExtra(CommonConst.VIEW_KEY_LEASE_NAME, map.get(CommonConst.List_VIEW_KEY.Sub.getString()));
        setResult(RESULT_OK, intent);
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
