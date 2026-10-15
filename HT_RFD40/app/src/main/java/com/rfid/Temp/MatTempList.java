package com.rfid.Temp;

import android.app.Activity;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.Resources;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.util.Log;
import android.util.TypedValue;
import android.view.Menu;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.SimpleAdapter;
import android.widget.TextView;

import com.rfid.Factory.ConstRuctionFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Factory.TempListFactory;
import com.rfid.Factory.TemporaryFactory;
import com.rfid.common.BaseActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonCalender;
import com.rfid.common.CommonConst;
import com.rfid.common.ConstRuctionDetail;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.common.TemporaryRegister;
import com.rfidpk.R;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
//20241004 ADD Shirakawa V4.0

/**
 * 仮置きデータの一覧表示画面のクラスです。
 * 仮置きマスタからデータを取得し、一覧表示します。
 */
public class MatTempList extends BaseActivity {

    /* 一覧表示 */
    ListView listView = null;
    // 前回選択されたアイテムのビュー
    private View previousSelectedView = null;
    // 前回選択されたアイテムのposition
    private int previousSelectedViewPosition = -1;

    //工事番号
    private String _ConstRuctionNo = "";

    //メニュー選択画面
    private String _MenuClick = "";

    //仮置きコンポーネントの設定
    private String[][] _strTemporaryInputItem;

    //工事貸出終了日
    private String _LentEndDt = "";

    private final static int BUTTON_SUMMARY = 0;

    private static final int RESIST_OK = 5;

    /**
     * 画面表示処理
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_temp_list);

        Intent intent = getIntent();
        //工事選択No取得
        _ConstRuctionNo = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO);

        //メニューで選択したメニュー取得
        _MenuClick = intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT);

        _LentEndDt = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT);

        //タイトルバー 戻るボタン表示
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        //DB接続用変数
        SQLiteDatabase db = null;

        try {

            //呼出元画面からタイトルを取得し表示する
            //((TextView)findViewById(R.id.lblListViewTitle)).setText(intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_TITLE));
            listView = (ListView) findViewById(R.id.constRuctionListView);

            //次へボタン非活性
            setButtonEnabled(false);

            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //設定マスタから表示非表示を確認
            _strTemporaryInputItem = SettingFactory.GetTemporaryInputItem(db);

            //仮置きマスタからデータを取得し一覧表示する
            ViewTempData(db, _ConstRuctionNo);

            //文言表示
            InitializeWord();

            //一覧表示のクリック処理
            listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                @Override
                public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                    //選択データを取得する
                    ListView listView = (ListView)parent;
                    Map<String, String> map = (HashMap)listView.getItemAtPosition(position);

                    // 前回選択されたアイテムがあれば、その背景色を元に戻す
                    if (previousSelectedView != null) {
                        //元に戻す
                        if (previousSelectedViewPosition % 2 == 0){
                            previousSelectedView.setBackgroundResource(R.color.ListViewBack);
                        } else {
                            previousSelectedView.setBackgroundResource(R.color.ListViewBack2);
                        }
                    }

                    // 現在選択されたアイテムの背景色を変更
                    view.setBackgroundResource(R.color.ButtonChange); // 直接背景色を変更

                    //呼出元に返却するデータを設定する
                    intent.putExtra(CommonConst.VIEW_KEY_HT_TEMP_ID, map.get(CommonConst.List_VIEW_KEY.Hidden.getString()));
                    setResult(RESULT_OK, intent);

                    // 今回の選択アイテムを保存
                    previousSelectedView = view;
                    previousSelectedViewPosition = position;

                    //次へボタン非活性
                    setButtonEnabled(true);

//                    // このアクティビティを終了する
//                    finish();
                }
            });

            //新規登録ボタン Clickイベント
            findViewById(R.id.btnRegist).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {

//                    //前回の選択がなければ何もしない
//                    if (previousSelectedView == null) {
//                        return;
//                    }

                    //仮置き詳細画面へ 新規のため仮置きIDを送らない
                    Intent intent = new Intent(getApplication(), TemporaryRegister.class);
                    intent.putExtra(CommonConst.VIEW_KEY_STATE_SELECT,CommonConst.VIEW_KEY_STATE_REGISTER);
                    intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                    intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, _ConstRuctionNo);
                    intent.putExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT, _LentEndDt);
                    startActivityForResult(intent, BUTTON_SUMMARY);
                }
            });

            //変更ボタン Clickイベント
            findViewById(R.id.btnChenge).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    //前回の選択がなければ何もしない
                        if (previousSelectedView == null) {
                            return;
                        }

                    //仮置きID取得
                    String id = intent.getStringExtra(CommonConst.VIEW_KEY_HT_TEMP_ID);

                    //仮置き登録画面へ 変更のため仮置きIDを送る
                    Intent intent = new Intent(getApplication(), TemporaryRegister.class);
                    intent.putExtra(CommonConst.VIEW_KEY_STATE_SELECT,CommonConst.VIEW_KEY_STATE_UPDATE);
                    intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                    intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, _ConstRuctionNo);
                    intent.putExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT, _LentEndDt);
                    intent.putExtra(CommonConst.VIEW_KEY_HT_TEMP_ID, id);
                    startActivityForResult(intent, BUTTON_SUMMARY);

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
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_LIST_TITLE, "", this));

        ((TextView)findViewById(R.id.lbHederTempDay))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_DAY, "", this));  //仮置き日
        ((TextView)findViewById(R.id.lbHederCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_COUNT, "", this));  //件数
//        ((TextView)findViewById(R.id.lbHederLiability))
//                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SET_LIABILITY, "", this));  //責任者
        ((TextView)findViewById(R.id.lbHederWorkLoc))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_WORK_LOC, "", this));  //作業場所
        ((TextView) findViewById(R.id.labSubTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_READ_MSG_ITEM, "", this));     //読取する仮置を選択してください


        ((TextView) findViewById(R.id.btnRegist))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_REGIST_BUTTON, "", this));                 //新規登録
        ((TextView) findViewById(R.id.btnChenge))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_CHENGE_BUTTON, "", this));                 //変更
    }

    /**
     * 仮置きマスタからデータを取得し、一覧表示する
     * @param db DB接続
     */
    private void ViewTempData(SQLiteDatabase db,String no) {

        //SQL作成
        String strSql = new String();
        List<String> list = new ArrayList<>();


        //SQL作成
        strSql = TemporaryFactory.createSQLData(no);
        //仮置きマスタからデータを取得
        Cursor cursor = db.rawQuery(strSql, null);

        //取得したデータを一覧表示に設定
        List<Map<String, String>> listview = new ArrayList<Map<String, String>>();
        while (cursor.moveToNext()){

            //表示データを設定する
            Map<String, String> map = new HashMap<String, String>();

            map.put(CommonConst.List_VIEW_KEY.Main.getString(), cursor.getString(cursor.getColumnIndex("TEMP_DT")) + " " + cursor.getString(cursor.getColumnIndex("COUNT")) + "件");   //仮置き日　件数　
            map.put(CommonConst.List_VIEW_KEY.Sub.getString(), cursor.getString(cursor.getColumnIndex("INFO1")).replace("$$$", ""));    //情報1
            map.put(CommonConst.List_VIEW_KEY.Sub2.getString(), cursor.getString(cursor.getColumnIndex("INFO2")).replace("$$$", ""));    //情報2
            map.put(CommonConst.List_VIEW_KEY.Sub3.getString(), cursor.getString(cursor.getColumnIndex("INFO3")).replace("$$$", ""));    //情報3　デリミタがあれば""
            map.put(CommonConst.List_VIEW_KEY.Sub4.getString(), cursor.getString(cursor.getColumnIndex("INFO4")).replace("$$$", ""));    //情報4  デリミタがあれば""
            map.put(CommonConst.List_VIEW_KEY.Sub5.getString(), cursor.getString(cursor.getColumnIndex("INFO5")).replace("$$$", ""));    //情報5
            map.put(CommonConst.List_VIEW_KEY.Sub6.getString(), cursor.getString(cursor.getColumnIndex("INFO6")).replace("$$$", ""));    //情報6
            map.put(CommonConst.List_VIEW_KEY.Sub7.getString(), cursor.getString(cursor.getColumnIndex("INFO7")).replace("$$$", ""));    //情報7
            map.put(CommonConst.List_VIEW_KEY.Hidden.getString(), cursor.getString(cursor.getColumnIndex("TEMP_ID")));    //隠れ項目　工事番号

            listview.add(map);
        }

        cursor.close();

        //今回仮置き登録したデータ取得
        strSql = TempListFactory.getTempListData(no);
        //仮置きマスタからデータを取得
         cursor = db.rawQuery(strSql, null);

        while (cursor.moveToNext()){

            //同じ仮置きIDが既に存在する場合
            removeItemsContaining(listview,cursor.getString(cursor.getColumnIndex("TEMP_ID")));

            //表示データを設定する
            Map<String, String> map = new HashMap<String, String>();

            map.put(CommonConst.List_VIEW_KEY.Main.getString(), cursor.getString(cursor.getColumnIndex("TEMP_DT")) + " " + cursor.getString(cursor.getColumnIndex("COUNT")) + "件");   //仮置き日　件数　
            map.put(CommonConst.List_VIEW_KEY.Sub.getString(), cursor.getString(cursor.getColumnIndex("INFO1")).replace("$$$", ""));    //情報1
            map.put(CommonConst.List_VIEW_KEY.Sub2.getString(), cursor.getString(cursor.getColumnIndex("INFO2")).replace("$$$", ""));    //情報2
            map.put(CommonConst.List_VIEW_KEY.Sub3.getString(), cursor.getString(cursor.getColumnIndex("INFO3")).replace("$$$", ""));    //情報3　デリミタがあれば""
            map.put(CommonConst.List_VIEW_KEY.Sub4.getString(), cursor.getString(cursor.getColumnIndex("INFO4")).replace("$$$", ""));    //情報4  デリミタがあれば""
            map.put(CommonConst.List_VIEW_KEY.Sub5.getString(), cursor.getString(cursor.getColumnIndex("INFO5")).replace("$$$", ""));    //情報5
            map.put(CommonConst.List_VIEW_KEY.Sub6.getString(), cursor.getString(cursor.getColumnIndex("INFO6")).replace("$$$", ""));    //情報6
            map.put(CommonConst.List_VIEW_KEY.Sub7.getString(), cursor.getString(cursor.getColumnIndex("INFO7")).replace("$$$", ""));    //情報7
            map.put(CommonConst.List_VIEW_KEY.Hidden.getString(), cursor.getString(cursor.getColumnIndex("TEMP_ID")));    //隠れ項目　工事番号

            listview.add(map);
        }

        //初期化
        SimpleAdapter arrayAdapter = InitializeAdapter(listview);

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
                R.layout.custom_mat_temp_listview,
                new String[] {CommonConst.List_VIEW_KEY.Main.getString(), CommonConst.List_VIEW_KEY.Sub.getString(), CommonConst.List_VIEW_KEY.Sub2.getString(), CommonConst.List_VIEW_KEY.Sub3.getString(), CommonConst.List_VIEW_KEY.Sub4.getString(), CommonConst.List_VIEW_KEY.Sub5.getString(), CommonConst.List_VIEW_KEY.Sub6.getString(), CommonConst.List_VIEW_KEY.Sub7.getString()},
                new int[] {android.R.id.text1, android.R.id.text2,R.id.text4,R.id.text5,R.id.text6,R.id.text7,R.id.text8,R.id.text9}
        ){
            @Override
            public View getView(int position, View convertView, ViewGroup parent){
                View view = super.getView(position, convertView, parent);

                //一覧に表示するか非表示にするか設定
                VisibleListSetting(_strTemporaryInputItem,view);

                if (position % 2 == 0){
                    view.setBackgroundResource(R.color.ListViewBack);
                } else {
                    view.setBackgroundResource(R.color.ListViewBack2);
                }

                if (Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString())) {
                    TextView first = view.findViewById(android.R.id.text1);
                    TextView second = view.findViewById(android.R.id.text2);
                    TextView fourth = view.findViewById(R.id.text4);
                    TextView fifth = view.findViewById(R.id.text5);
                    TextView sixth = view.findViewById(R.id.text6);
                    TextView seventh = view.findViewById(R.id.text7);
                    TextView eighth = view.findViewById(R.id.text8);
                    TextView ninth = view.findViewById(R.id.text9);
                    first.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_first_line_text));
                    second.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
                    fourth.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
                    fifth.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
                    sixth.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
                    seventh.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
                    eighth.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
                    ninth.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
                }

                //前回の選択があれば色を変える
                if (position == previousSelectedViewPosition){
                    // 現在選択されたアイテムの背景色を変更
                    view.setBackgroundResource(R.color.ButtonChange); // 直接背景色を変更
                }

                return view;
            }
        };

        return adapter;
    }

    private void VisibleListSetting(String prmTemporaryInputItem[][],View prmView){
        //情報1
        //非表示
        if(prmTemporaryInputItem[0][0].equals(CommonConst.MATCHING_FUNCTION.NO.getString())){
            //情報1非表示
            prmView.findViewById(android.R.id.text2).setVisibility(View.GONE);

        }else{
            //情報1表示
            prmView.findViewById(android.R.id.text2).setVisibility(View.VISIBLE);
        }

        //情報2
        //非表示
        if(prmTemporaryInputItem[1][0].equals(CommonConst.MATCHING_FUNCTION.NO.getString())){
            //情報2非表示
            prmView.findViewById(R.id.text4).setVisibility(View.GONE);

        }else{
            //情報2表示
            prmView.findViewById(R.id.text4).setVisibility(View.VISIBLE);
        }

        //情報3
        //非表示
        if(prmTemporaryInputItem[2][0].equals(CommonConst.MATCHING_FUNCTION.NO.getString())){
            //情報3非表示
            prmView.findViewById(R.id.text5).setVisibility(View.GONE);

        }else{
            //情報3表示
            prmView.findViewById(R.id.text5).setVisibility(View.VISIBLE);
        }

        //情報4
        //非表示
        if(prmTemporaryInputItem[3][0].equals(CommonConst.MATCHING_FUNCTION.NO.getString())){
            //情報4非表示
            prmView.findViewById(R.id.text6).setVisibility(View.GONE);

        }else{
            //情報4表示
            prmView.findViewById(R.id.text6).setVisibility(View.VISIBLE);
        }

        //情報5
        //非表示
        if(prmTemporaryInputItem[4][0].equals(CommonConst.MATCHING_FUNCTION.NO.getString())){
            //情報5非表示
            prmView.findViewById(R.id.text7).setVisibility(View.GONE);
        }else{
            //情報5表示
            prmView.findViewById(R.id.text7).setVisibility(View.VISIBLE);
        }

        //情報6
        //非表示
        if(prmTemporaryInputItem[5][0].equals(CommonConst.MATCHING_FUNCTION.NO.getString())){
            //情報6非表示
            prmView.findViewById(R.id.text8).setVisibility(View.GONE);
        }else{
            //情報6表示
            prmView.findViewById(R.id.text8).setVisibility(View.VISIBLE);
        }

        //情報7
        //非表示
        if(prmTemporaryInputItem[6][0].equals(CommonConst.MATCHING_FUNCTION.NO.getString())){
            //情報7非表示
            prmView.findViewById(R.id.text9).setVisibility(View.GONE);
        }else{
            //情報7表示
            prmView.findViewById(R.id.text9).setVisibility(View.VISIBLE);
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

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    /**
     * 一覧画面から戻るイベント処理をします。
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data){
        try {
            switch(requestCode) {
                case BUTTON_SUMMARY:        //一覧画面から戻る

                    if(resultCode == RESIST_OK){
                        setResult(RESIST_OK);

                        //画面を終了する
                        this.finish();
                    }

                default:
                    break;
            }
        } catch (Exception ex) {
            //ShowErrorMessage(ex.getMessage(), "");
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * ボタンの非活性、活性をします
     * @param blnEnable
     */
    private void setButtonEnabled(boolean blnEnable) {
        if (blnEnable) {
            findViewById(R.id.btnChenge).setEnabled(true);
            findViewById(R.id.btnChenge).setBackground(getDrawable(R.drawable.execute_button));
            ((Button)findViewById(R.id.btnChenge)).setTextColor(Color.parseColor("#ffffff"));
        } else {
            findViewById(R.id.btnChenge).setEnabled(false);
            findViewById(R.id.btnChenge).setBackground(getDrawable(R.drawable.execute_button_diable));
            ((Button)findViewById(R.id.btnChenge)).setTextColor(Color.parseColor("#a9a9a9"));
        }
    }

    // 指定の文字列を含むデータを削除
    private static void removeItemsContaining(List<Map<String, String>> list, String keyword) {
        Iterator<Map<String, String>> iterator = list.iterator();
        while (iterator.hasNext()) {
            Map<String, String> item = iterator.next();
            if (item.get(CommonConst.List_VIEW_KEY.Hidden.getString()).toLowerCase().contains(keyword.toLowerCase())) {
                iterator.remove(); // 安全な削除方法
            }
        }
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }
}
