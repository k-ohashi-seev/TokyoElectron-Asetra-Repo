package com.rfid.common;

import android.app.Activity;
import android.content.Intent;
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
import com.rfid.Factory.EmployeeFactory;
import com.rfid.Factory.LocationFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.main.LoginMn;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import android.content.SharedPreferences;
//20241004 ADD Shirakawa V4.0
/**
 * 工事データの一覧表示画面のクラスです。
 * 工事マスタからデータを取得し、一覧表示します。
 */
public class ConstRuction extends BaseActivity implements CommonCalender.Callback {

    /* 一覧表示 */
    ListView listView = null;
    // 前回選択されたアイテムのビュー
    private View previousSelectedView = null;
    // 前回選択されたアイテムのposition
    private int previousSelectedViewPosition = -1;

    String[] _auseLoc = null;

    String[] _auseShareLoc = null;

    //作業実施日
    private String _WorkDay = "";

    //メニュー選択画面
    private String _MenuClick = CommonConst.MENU_CLICK.Inventory.toString();

    //SharedPreferencesキー：ConstRuctionPrevSet使用
    public final String KEY_CONSTRUCTION_PREV_SET = "construction_prev_set";

    private String ConstRuctionNo = "";

    private String MngLocLCd = "";

    /**
     * 画面表示処理
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_construction);

        //タイトルバー 戻るボタン表示
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        //DB接続用変数
        SQLiteDatabase db = null;

        ConstRuctionNo = getConnectConstRuction(this);

        try {
            Intent intent = getIntent();
            //メニューで選択したメニュー取得
            _MenuClick = intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT);
            //呼出元画面からタイトルを取得し表示する
            //((TextView)findViewById(R.id.lblListViewTitle)).setText(intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_TITLE));
            listView = (ListView) findViewById(R.id.constRuctionListView);

            //次へボタン非活性
            setButtonEnabled(false);

            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //現日付取得
            final DateFormat df = new SimpleDateFormat("yyyy/MM/dd");
            final Date date = new Date(System.currentTimeMillis());

            //作業日を取得し表示する
            ((TextView) findViewById(R.id.txtWorkDay)).setText(df.format(date));

            //工事マスタからデータを取得し一覧表示する
            ViewConstRuctionData(db,ConstRuctionNo);

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
                    intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, map.get(CommonConst.List_VIEW_KEY.Hidden.getString()));

                    //選択した工事情報の管理ロケ取得
                    MngLocLCd = map.get(CommonConst.List_VIEW_KEY.Aux.getString());

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

            //次へボタン Clickイベント
            findViewById(R.id.btnNext).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {

                    //前回の選択がなければ何もしない
                    if (previousSelectedView == null) {
                        return;
                    }

                    //更新可能権限かチェック
                    if(!AuthCheck()){
                        return;
                    }

                    //工事番号取得
                    String no = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO);

                    //工事番号保存
                    setConnectConstRuction(ConstRuction.this, no);

//                    //機能ID取得
//                    functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_SEARCH);
                    //詳細画面へ
                    Intent intent = new Intent(getApplication(), ConstRuctionDetail.class);
                    intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                    intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, no);
                    startActivity(intent);
                }
            });

            //カレンダー選択ボタン Clickイベント
            final CommonCalender.Builder calender = new CommonCalender.Builder(this);
            //作業実施日ボタン Clickイベント
            findViewById(R.id.txtWorkDay).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    //カレンダー表示
                    calender.show();

                }
            });

            //検索ボタン押下
            //検索ボタン Clickイベント
            findViewById(R.id.btnSearch).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {

                    //DB接続用変数
                    SQLiteDatabase db = null;

                    try {
                        Intent intent = getIntent();
                        //呼出元画面からタイトルを取得し表示する
                        listView = (ListView) findViewById(R.id.constRuctionListView);

                        //前回の選択データ削除
                        previousSelectedView = null;
                        previousSelectedViewPosition = -1;

                        //次へボタン非活性
                        setButtonEnabled(false);

                        //データベースヘルパーのインスタンスを作成する
                        db = dbHelper.getWritableDatabase();

                        //ロケマスタからデータを取得し一覧表示する
                        ViewConstRuctionData(db,ConstRuctionNo);

                    } catch (Exception ex){
                        showErrorMessage(ex.getMessage());
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    } finally {
                        db.close();
                    }

                }
            });

            //クリアボタン Clickイベント
            findViewById(R.id.btnClear).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {

                    try {

                        //検索条件をクリアする
                        ((TextView) findViewById(R.id.txtWorkDay)).setText("");
                        ((TextView) findViewById(R.id.txtLiability)).setText("");

                    } catch (Exception ex){
                        showErrorMessage(ex.getMessage());
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    }
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
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_TITLE, "", this));

        ((TextView)findViewById(R.id.lbWorkDay))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SET_WORK_CARRY_DAY, "", this));  //作業実施日
        ((TextView)findViewById(R.id.lbLiability))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SET_LIABILITY, "", this));  //責任者
        ((TextView)findViewById(R.id.btnSearch))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SEARCH_BUTTON, "", this));  //検索
        ((TextView)findViewById(R.id.btnClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_CLEAR_BUTTON, "", this));  //クリア
        ((TextView)findViewById(R.id.lbHederWorkDay))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SET_WORK_DAY, "", this));  //作業日
        ((TextView)findViewById(R.id.lbHederLiability))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SET_LIABILITY, "", this));  //責任者
        ((TextView)findViewById(R.id.lbHederWorkContent))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SET_WORK_CONTENT, "", this));  //作業内容
        ((TextView) findViewById(R.id.labSubTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_INPUT_MSG_ITEM, "", this));     //保管先の読取又は選択


        ((TextView) findViewById(R.id.btnNext))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_NEXT, "", this));                 //次へ
    }

    /**
     * 工事マスタからデータを取得し、一覧表示する
     * @param db DB接続
     */
    private void ViewConstRuctionData(SQLiteDatabase db,String no) {

        SettingValue settingValue = new SettingValue();

        //SQL作成
        String strSql = new String();
        List<String> list = new ArrayList<>();


        String loginuserCd = SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD,"",this);

        //テキストボックスの値取得
        String strDay = ((EditText)findViewById(R.id.txtWorkDay)).getText().toString();
        String strLiability = ((EditText)findViewById(R.id.txtLiability)).getText().toString();

        //HT.対象拠点を取得
        Intent intent = getIntent();
        String strHtHub = intent.getStringExtra(CommonConst.VIEW_KEY_HT_HUB);
        Cursor cursor;
        String empCodeCheck = C_Setting.getEmployeePrevSet(this);
        ArrayList locCodeList;

//        //参照拠点取得
//        if (CommonAuthority.Auth.readHubList != null) {
//
//        }

        //拠点権限の場合(通常の場合は全表示)
        if (SettingFactory.GetAuthorityType(db).equals(CommonConst.AUTHORITY_TYPE.HUB.getString())){

            String[] target = null;
            target = CommonAuthority.Auth.readHubList;
            //共通拠点マージした参照権限ロケ
            List<String> shareMargeList = new ArrayList<>();
            //共通拠点のロケ
            List<String> shareList = new ArrayList<>();

            //共通拠点以外の拠点がある場合
            if(target != null){
                //共通拠点をリストに追加するための一時リスト
                shareMargeList = new ArrayList<>(Arrays.asList(target));
            }

            //参照権限がYの場合
            if (CommonAuthority.Auth.readAuthFlg.equals(CommonConst.AUTH_FLG.YES.getString())){

                //共通ロケのロケデータ取得
                strSql = LocationFactory.GetShareLocationLCode();

                cursor = db.rawQuery(strSql,null);

//                //共通拠点をリストに追加するための一時リスト
//                List<String> shareMargeList = new ArrayList<>(Arrays.asList(target));

                while (cursor.moveToNext()){
                    String locLCode = cursor.getString(cursor.getColumnIndex("LOC_L_CD"));
                    //共通ロケ
                    shareList.add(locLCode);
                    if (!shareMargeList.contains(locLCode)) {  // 重複チェック
                        shareMargeList.add(locLCode);
                    }
                }

                //一件以上ある場合
                if(shareMargeList.size() > 0){
                    //共通拠点と対象拠点の情報
                    _auseLoc = shareMargeList.toArray(new String[0]);
                }

                //一件以上ある場合
                if(shareList.size() > 0){
                    //共通拠点の情報
                    _auseShareLoc = shareList.toArray(new String[0]);
                }

                if (cursor != null) {
                    cursor.close(); // Cursor を閉じる
                }
            }else{
                //一件以上ある場合
                if(shareMargeList.size() > 0){
                    //共通拠点と対象拠点の情報
                    _auseLoc = shareMargeList.toArray(new String[0]);
                }
            }
        }

        //SQL作成
        strSql = ConstRuctionFactory.getConstRuctionData(strDay,strLiability,no,_auseLoc);

        //検索条件がない場合、nullを設定
        String[] strWhere = list.toArray(new String[list.size()]);
        if (strWhere.length == 0){
            strWhere = null;
        }

        //工事マスタからデータを取得
        cursor = db.rawQuery(strSql, strWhere);

        String strStartDay = "";
        String strEndDay = "";

        //取得したデータを一覧表示に設定
        List<Map<String, String>> listview = new ArrayList<Map<String, String>>();
        while (cursor.moveToNext()){

            strStartDay = getMonthDay(cursor.getString(0));
            strEndDay = getMonthDay(cursor.getString(1));

            //表示データを設定する
            Map<String, String> map = new HashMap<String, String>();
            map.put(CommonConst.List_VIEW_KEY.Main.getString(), strStartDay + "～" + strEndDay + "       " + cursor.getString(2));   //作業日　責任者
            map.put(CommonConst.List_VIEW_KEY.Sub.getString(), cursor.getString(3));    //作業内容
            map.put(CommonConst.List_VIEW_KEY.Hidden.getString(), cursor.getString(4));    //隠れ項目　工事番号
            map.put(CommonConst.List_VIEW_KEY.Aux.getString(), cursor.getString(5));    //補足情報　管理ロケ

            listview.add(map);
        }

        if (cursor != null) {
            cursor.close(); // Cursor を閉じる
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


                if (Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString())) {
                    TextView first = view.findViewById(android.R.id.text1);
                    TextView second = view.findViewById(android.R.id.text2);
                    first.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_first_line_text));
                    second.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
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

    private String getMonthDay(String dateStr) {

        // 月と日の部分を抽出
        //String month = String.valueOf(Integer.parseInt(dateStr.substring(4, 6))); // 08 -> 8
        String month = String.valueOf(dateStr.substring(4, 6));

        String day = dateStr.substring(6, 8); // 24

        // フォーマットして出力
        String formattedDate = month + "/" + day;

        return formattedDate;
    }

    /**
     * カレンダークリック時のイベント
     * @param strDate 日付
     */
    @Override
    public void onCalenderClick(String strDate) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        if (strDate != null && !"".equals(strDate)){
            _WorkDay = strDate;

            ((TextView) findViewById(R.id.txtWorkDay)).setText(strDate);
        }

    }

    /**
     * ボタンの非活性、活性をします
     * @param blnEnable
     */
    private void setButtonEnabled(boolean blnEnable) {
        if (blnEnable) {
            findViewById(R.id.btnNext).setEnabled(true);
            findViewById(R.id.btnNext).setBackground(getDrawable(R.drawable.execute_button));
            ((Button)findViewById(R.id.btnNext)).setTextColor(Color.parseColor("#ffffff"));
        } else {
            findViewById(R.id.btnNext).setEnabled(false);
            findViewById(R.id.btnNext).setBackground(getDrawable(R.drawable.execute_button_diable));
            ((Button)findViewById(R.id.btnNext)).setTextColor(Color.parseColor("#a9a9a9"));
        }
    }

    /**
     * 工事IDを取得する
     * @param act    アクティビティ
     * @return        設定値
     */
    public String getConnectConstRuction(Activity act) {
        Resources res = act.getResources();
        return getString(KEY_CONSTRUCTION_PREV_SET, "", act);
    }

    /**
     * 工事IDを保存する
     * @param act    アクティビティ
     * @param value  設定値
     */
    public void setConnectConstRuction(Activity act,  String value) {
        setString(KEY_CONSTRUCTION_PREV_SET, value, act);
    }

    /**
     * SharedPreferencesへ文字列値を保存する
     * @param key SharedPreferencesキー
     * @param val 設定値
     * @param act アクティビティ
     */
    private static void setString(String key, String val, Activity act) {
        SharedPreferences.Editor editor = PreferenceManager.getDefaultSharedPreferences(act).edit();
        editor.putString(key, val);
        editor.apply();
        Log.i("設定値保存", String.format("%s：%s→%s", key, getString(key, val, act), val));
    }

    /**
     * SharedPreferencesから文字列値を取得する
     * @param key    SharedPreferencesキー
     * @param defVal 未設定時等の初期値
     * @param act    アクティビティ
     * @return 設定値
     */
    private static String getString(String key, String defVal, Activity act) {
        String val = PreferenceManager.getDefaultSharedPreferences(act).getString(key, defVal);
        Log.d("設定値取得", String.format("%s：%s", key, val));
        return val;
    }

    private boolean AuthCheck(){
        Intent intent = getIntent();
        try {
            //更新可能の権限かチェック
            if(!CommonAuthority.CheckConstRactionAuth(MngLocLCd, CommonAuthority.Auth.updHubList,_auseShareLoc)){
                //エラーメッセージを表示
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_NOTHING_UPDATE_AUTH , "", this),
                        SystemSetting.getString(intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT), "", this));
                return false;
            }

            return true;

        } catch (Exception ex){
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" +  Common.toString(ex), this);
            return false;
        }
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }
}
