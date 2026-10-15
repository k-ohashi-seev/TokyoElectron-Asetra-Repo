package com.rfid.Search;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.util.TypedValue;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.SimpleAdapter;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.main.LoginMn;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;
import com.rfid.Factory.SearchFactory;
import com.rfid.common.AssetModel;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.CommonProgres;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

/**
 * 探索詳細表示のクラス
 */
public class SearchSummary extends BaseRFIDActivity implements CommonOKDialog.Callback, CommonConfirmDialog.Callback {

    //保持データ
    private SearchFactory _SearchData = null;

    //探索画面
    private static final int ACTION_LOGOUT = 1;
    private static final int BUTTON_SEARCH = 2;

    //探索データ
    HashMap<String, AssetModel> _SearchTag = null;

    CommonConfirmDialog.Builder  _confirmDialog = null;

    //プログレスダイアログ クラス
    private CommonProgres C_Progres = new CommonProgres();

    //キー
    private final static int BUTTON_SETTEI = 9;     //20191213 Add Sasaki


    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        SQLiteDatabase db = null;

        try {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.activity_search_summary);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //前画面からデータを取得する
            _SearchData = (SearchFactory)getIntent().getSerializableExtra(CommonConst.VIEW_KEY_SEARCH_DATA);

            //文言表示
            InitializeWord();

            //操作時間のタイマーをセット
            StartTimeoutTimer(CommonConst.VIEW_WORD_MENU_SEARCH);

            _confirmDialog = new CommonConfirmDialog.Builder(this);

            //探索ボタン Clickイベント
            final Activity activity = this;
            final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
            findViewById(R.id.btnSearch).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {

                    if (checkTimeout()) {return;}

                    //ボタン非活性
                    SetButtonEnabled(false);

                    // プログレスダイアログの表示
                    C_Progres.StartProgressDialog(activity, "Searching...");
                    final Handler mainHandler = new Handler(Looper.getMainLooper());
                    new Thread() {
                        @Override
                        public void run() {
                            //DB接続
                            DatabaseHelper dbHelper = new DatabaseHelper(activity);
                            final SQLiteDatabase db = dbHelper.getWritableDatabase();
                            try {
                                //一時表に登録する
                                CreateTempTable(db);

                                //探索する
                                SearchData(db);

                                //データを検索
                                ReaderOn();

                                mainHandler.post(new Runnable() {
                                    @Override
                                    public void run() {
                                        try {
                                            //一覧表示を再表示する
                                            ViewLeaseDataDenpa(db);
                                        } finally {
                                            if (db != null) {
                                                db.close();
                                            }

                                            //ボタン活性
                                            SetButtonEnabled(true);

                                            //ReaderOff
                                            ReaderOff();
                                            C_Progres.StopProgressDialog();
                                        }
                                    }
                                });

                            } catch (Exception ex){
                                //ボタン活性
                                SetButtonEnabled(true);
                                showErrorMessage(ex.getMessage());
                                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                                //エラーログ
                                LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), activity);
                            }
                        }
                    }.start();
                }
            });

            //Vieweを初期化する
            final ListView listView = (ListView) findViewById(R.id.listview_Summary_Search);

            //一覧表示のクリック処理
            listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                @Override
                public void onItemClick(AdapterView<?> parent, View view, int position, long id) {

                    //未接続の場合、終了
                    if (!connectReader) {
                        ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_RFID_CONNECT, "", activity),
                                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", activity));
                        return;
                    }

                    //一覧表示のデータを取得する
                    Map<String, String> map = (HashMap)listView.getItemAtPosition(position);
                    String strAssetNo = map.get(CommonConst.List_VIEW_KEY.Main.getString());

                    //資産マスタに存在しない場合、処理終了
                    if (map.get(CommonConst.List_VIEW_KEY.Hidden.getString()) == null) {
                        return;
                    }

                    //探索画面へ
                    Intent intent = new Intent(getApplication(), SearchTag.class);
                    intent.putExtra(CommonConst.VIEW_KEY_SEARCH_ASSET, strAssetNo);    //資産番号
                    intent.putExtra(CommonConst.VIEW_KEY_SEARCH_DATA, _SearchData);
                    startActivityForResult(intent, BUTTON_SEARCH);
                }
            });

        } catch (Exception ex) {
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
     * 探索開始
     */
    protected void ReaderOn(){

        LinkTagList.clear();

        //Densoの場合開始する
        //20230221 UPD Sato v2.3 iDataとキーエンス製DXシリーズの場合も開始する
        //20230410 UPD Sato v2.3 RS35の場合も開始する
        if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName) ||
            CommonConst.CONNECT_RFIDNAME.IDATA.getString().equals(Common.ConnectRFIDName) ||
            CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString().equals(Common.ConnectRFIDName) ||
                CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString().equals(Common.ConnectRFIDName)) {

            for(Iterator<AssetModel> iterator = _SearchTag.values().iterator(); iterator.hasNext(); ) {

                //探索する
                AssetModel model = iterator.next();
                Rfid_Search_List_Start(model.getAssetID(), GetSettingKey());

                //ReaderOFFをすぐに呼ぶと次の電波強度が取得できないためスリープを入れる
                try {
                    Thread.sleep(1500);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                ReaderOff();
            }
        }
    }

    /**
     * 探索終了
     */
    protected void ReaderOff(){
        //探索OFF
        Rfid_Search_Stop();
    }

    //メニュー選択イベント
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        try {
            switch(item.getItemId()) {
                case android.R.id.home:     //戻るボタン
                    this.finish();
                    return true;
//20191213 Add Start Sasaki
                case R.id.action_setting:
                    //設定 →　システム設定画面開く
                    ReaderOff();
                    Intent intent = new Intent(getApplication(), SystemSett.class);
                    startActivityForResult(intent, BUTTON_SETTEI);
                    return true;

                case R.id.action_logout:  //ログアウトボタン
                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, ACTION_LOGOUT);
                    _confirmDialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGOUT, "", this));  //ログアウト
                    _confirmDialog.message(SystemSetting.getString(
                            CommonConst.VIEW_WORD_LOGOUT, "", this));                //ログアウトします。よろしいですか？
                    _confirmDialog.params(bundle);
                    _confirmDialog.show();
                    return true;
//20191213 Add End Sasaki
            }
            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
            return false;
        }
    }

    // レジュームイベント
    @Override
    public void onResume() {
        super.onResume();

        SQLiteDatabase db = null;
        try {

            //ReaderOff
            ReaderOff();

            //タイマー開始
            StartTimer();

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //一時表に登録する
            CreateTempTable(db);

            //探索する
            SearchData(db);

            //一覧表示を再表示
            ViewLeaseData(db);

            //ボタンを活性/非活性
            if(connectReader) {
                SetButtonEnabled(true);
            } else {
                SetButtonEnabled(false);
            }

        } catch (Exception ex) {
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

    //ポーズイベント
    @Override
    public void onPause() {
        super.onPause();

        //タイマー停止
        StopTimer();

        //ReaderOff
        //ReaderOff();

    }

    /**
     * 文言表示
     */
    @SuppressLint("SetTextI18n")
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH,"",this));                      //探索　タイトル
        ((TextView)findViewById(R.id.lblSummaryListTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NUMBER, "", this) + "\n" + SystemSetting.getString(CommonConst.VIEW_WORD_INV_SUB_ASSETNO, "", this)); //資産番号 サブ資産番号
        ((TextView)findViewById(R.id.lblSummaryInventoryS))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_SEARCH_S, "", this));       //遠
        ((TextView)findViewById(R.id.lblSummaryInventoryL))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_SEARCH_L, "", this));       //近
        ((TextView)findViewById(R.id.btnSearch))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_SEARCH_BUTTON, "", this));  //探索
    }

    /** ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * */
    private void SetButtonEnabled(boolean prmEnabled) {

        //0件の場合非活性
        if (_SearchData.CountAll() <= 0) {
            prmEnabled = false;
        }
        final boolean blnEnabled = prmEnabled;
        runOnUiThread(new Runnable() {	//20200121 Add Thread Sasaki
            @Override
            public void run() {
                Button ButonSearch = findViewById(R.id.btnSearch);
                ButonSearch.setEnabled(blnEnabled);

                if (blnEnabled == false) {
                    //探索ボタン
                    ((Button)findViewById(R.id.btnSearch)).setBackground(getDrawable(R.drawable.execute_button_diable));
                    ((Button)findViewById(R.id.btnSearch)).setTextColor(Color.parseColor("#a9a9a9"));
                } else {
                    //探索ボタン
                    ((Button)findViewById(R.id.btnSearch)).setBackground(getDrawable(R.drawable.execute_button));
                    ((Button)findViewById(R.id.btnSearch)).setTextColor(Color.parseColor("#ffffff"));
                }
            }
        });
    }

    /**
     * 一時表を作成し読取データを登録する
     * @param db
     */
    private void CreateTempTable(SQLiteDatabase db) {

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" CREATE TEMP TABLE ");
        sql.append(CommonConst.TEMP_SEARCH);
        sql.append(" (");
        sql.append("  ASSET_NO");                      //資産番号
        sql.append("  ) ");

        //SQL発行
        db.execSQL(sql.toString());

        for(String assetNo : _SearchData.GetAssetNo()){
            //SQL作成
            StringBuilder sqlInsert = new StringBuilder();
            sqlInsert.append(" INSERT");
            sqlInsert.append(" INTO ");
            sqlInsert.append(CommonConst.TEMP_SEARCH);
            sqlInsert.append(" (");
            sqlInsert.append("  ASSET_NO");                //資産番号
            sqlInsert.append("  ) ");
            sqlInsert.append(" VALUES (");
            sqlInsert.append(" ?");
            sqlInsert.append(" )");

            //SQL発行
            db.execSQL(sqlInsert.toString(), new String[]{
                    assetNo             //資産番号
            });
        }
    }

    private void SearchData(SQLiteDatabase db) {
        //読込データを取得する
        _SearchTag = _SearchData.SelectAssetData(db);
    }

    /**
     * RFID読取りイベント
     * 親クラスから呼ばれます。
     * @param strData
     * @param tempRSSI
     */
    @Override
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) {

        Log.d(strData, String.valueOf(tempRSSI));
        //資産IDをキーに、電波強度を溜め込む
        if (LinkTagList.containsKey(strData)) {
            LinkTagList.remove(strData);
        }
        LinkTagList.put(strData, tempRSSI);

    }

    /**
     * 一覧表示する
     * @param db
     */
    private void ViewLeaseData(SQLiteDatabase db) {

        //読込データを取得する
        HashMap<String, AssetModel> data = _SearchData.SelectAssetData(db);

        //表示リスト
        List<Map<String, String>> list = new ArrayList<Map<String, String>>();

        //取得したデータを一覧表示に設定
        for (Map.Entry<String, AssetModel> entry : data.entrySet()) {
            //表示データを設定する
            Map<String, String> map = new HashMap<String, String>();
            map.put(CommonConst.List_VIEW_KEY.Main.getString(), entry.getKey());                     //資産番号
            map.put(CommonConst.List_VIEW_KEY.Sub.getString(), entry.getValue().getSubAssetNo());    //サブ資産番号
            map.put(CommonConst.List_VIEW_KEY.Hidden.getString(), entry.getValue().getAssetID());  //資産ID
            map.put(CommonConst.List_VIEW_KEY.Inv.getString(), "");                               //電波
            list.add(map);

        }

        //初期化
        SimpleAdapter arrayAdapter = InitializeAdapter(list);
        ListView listView = (ListView) findViewById(R.id.listview_Summary_Search);

        //リストに追加
        listView.setAdapter(arrayAdapter);
    }

    /**
     * 一覧表示する
     * @param db
     */
    private void ViewLeaseDataDenpa(SQLiteDatabase db) {

        //読込データを取得する
        HashMap<String, AssetModel> data = _SearchData.SelectAssetData(db);

        //表示リスト
        List<Map<String, Object>> list = new ArrayList<Map<String, Object>>();

        //取得したデータを一覧表示に設定
        for (Map.Entry<String, AssetModel> entry : data.entrySet()) {
            //表示データを設定する
            Map<String, Object> map = new HashMap<String, Object>();
            map.put(CommonConst.List_VIEW_KEY.Main.getString(), entry.getKey());                     //資産番号
            map.put(CommonConst.List_VIEW_KEY.Sub.getString(), entry.getValue().getSubAssetNo());    //サブ資産名称
            map.put(CommonConst.List_VIEW_KEY.Hidden.getString(), entry.getValue().getAssetID());  //資産ID

            //電波状態を取得
            String strData = ReadSearchTag(entry.getValue().getAssetID(), GetSettingKey());
            map.put(CommonConst.List_VIEW_KEY.Inv.getString(), strData);                               //電波
            list.add(map);

        }

        //初期化
        SimpleAdapter arrayAdapter = InitializeAdapter(list);
        ListView listView = (ListView) findViewById(R.id.listview_Summary_Search);

        //リストに追加
        listView.setAdapter(arrayAdapter);
    }

    /**
     * 一覧表示の初期化処理
     * @return 一覧表示用Adapter
     */
    private SimpleAdapter InitializeAdapter(List list){

        //Adapterの設定
        SimpleAdapter adapter = new SimpleAdapter(
                this,
                list,
                R.layout.custom_listview_search,
                new String[] {CommonConst.List_VIEW_KEY.Main.getString(), CommonConst.List_VIEW_KEY.Sub.getString(),CommonConst.List_VIEW_KEY.Inv.getString(), CommonConst.List_VIEW_KEY.Hidden.getString()},
                new int[] {android.R.id.text1, android.R.id.text2, R.id.text4,  R.id.text3}
        ){
            /**
             * 一覧表示設定
             * @param position
             * @param convertView
             * @param parent
             * @return
             */
            @Override
            public View getView(int position, View convertView, ViewGroup parent){
                View view = super.getView(position, convertView, parent);

                if (position % 2 == 0){
                    view.setBackgroundResource(R.color.ListViewBack);
                } else {
                    view.setBackgroundResource(R.color.ListViewBack2);
                }

                //20230221 ADD Sato V2.3 Start キーエンス製DXシリーズの場合は文字サイズを変更
                if (Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString())) {
                    TextView first = view.findViewById(android.R.id.text1);
                    TextView second = view.findViewById(android.R.id.text2);
                    first.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_first_line_text));
                    second.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
                }
                //20230221 ADD Sato V2.3 End

                //資産IDを取得する
                Map<String, String> item = (HashMap)this.getItem(position);
                //String strAssetName = item.get(CommonConst.List_VIEW_KEY.Sub.getString());    //20200108 Del Sasaki
                //if (strAssetName == null || "".equals(strAssetName)) {                        //20200108 Del Sasaki
                String strAssetID = item.get(CommonConst.List_VIEW_KEY.Hidden.getString());    //20200108 Add Sasaki
                if(strAssetID == null || "".equals(strAssetID)) {                             //20200108 Add Sasaki
                    //資産IDなしの場合
                    ((TextView) view.findViewById(android.R.id.text1)).setTextColor(Color.RED);      //資産番号
                    ((TextView) view.findViewById(android.R.id.text2)).setTextColor(Color.RED);      //資産名称
                }

                //電波強度を数値から画像に設定する
                LinearLayout appInfoImage = view.findViewById(R.id.layoutIntencity);
                ViewGroup.LayoutParams params = appInfoImage.getLayoutParams();

                //電波強度を取得する
                String strIntencity = item.get(CommonConst.List_VIEW_KEY.Inv.getString());

                //params.width = params.width / 5 * getReadPowerWidth(strIntencity);    20200318 Del Sasaki
                params.width = 60 * getReadPowerWidth(strIntencity);                   //20200318 Add Sasaki
                appInfoImage.setLayoutParams(params);

                return view;
            }
        };

        return adapter;
    }

    /**
     * サブ画面から戻るイベント処理をします。
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data){
        try {
            switch(requestCode) {
                case BUTTON_SEARCH:        //探索画面から戻る
                    if(resultCode == RESULT_OK) {
                        //前画面からデータを取得する
                        _SearchData = (SearchFactory) data.getSerializableExtra(CommonConst.VIEW_KEY_SEARCH_DATA);
                    }
                    break;

                default:
                    break;
            }

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }


    /**
     * RFIDリーダー切断イベント
     */
    @Override
    public void onDisconnectEvent() {
        super.onDisconnectEvent();

        AllResetButton(false);
    }

    /**
     * RFIDリーダー接続イベント
     */
    @Override
    public void OnScannerAppeared() {
        //親クラスの接続処理
        super.OnScannerAppeared();

        //タグモードの場合活性
        AllResetButton(true);
    }

    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        if (params == null) {
            return;
        }

        //呼出元で分岐する
        switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
            case ACTION_LOGOUT:      //ログアウトボタン
                //ログイン画面を開く
                Intent intent = new Intent(getApplication(), LoginMn.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                startActivity(intent);
                break;

        }
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {
        SetButtonEnabled(blnEnabled);
    }

    @Override
    protected void OpenMainActivity(){ }
    @Override
    protected void BarcodeDataReceived(String strData) {}
    @Override
    protected String GetSettingKey(){return C_Setting.KEY_EACH_SEARCH;}

}
