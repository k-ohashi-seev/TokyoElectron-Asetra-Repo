package com.rfid.common;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.SimpleAdapter;
import android.widget.TextView;

import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.LentAssetListFactory;
import com.rfid.Factory.RetAseetListFactory;
import com.rfid.Factory.SearchFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Factory.TempAssetFactory;
import com.rfid.Factory.TempAssetListFactory;
import com.rfid.Search.SearchTag;
import com.rfid.WebAPI.UsbConfirmActivity;
import com.rfid.WebAPI.WebSendActivity;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;
import com.rfid.Factory.LogFactory;
import com.rfid.common.bean.EntryProcessDataDetail;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/**
 * 詳細表示の親クラス
 */
public abstract class BaseSummary extends BaseActivity implements CommonOKDialog.Callback, CommonConfirmDialog.Callback{

    // 保持データ
    //protected ProcessFactory _ProcessData = null;     //20200211 Del Sasaki

    /* 一覧表示 */
    ListView listView = null;

    //プログレスダイアログ クラス
    protected CommonProgres C_Progres = new CommonProgres();

    private View previousSelectedView = null;
    private int previousSelectedColor;
    // 前回選択されたアイテムのposition
    protected int previousSelectedViewPosition = -1;

    //登録時の警告フラグ
    protected Boolean _blnAlertFlag = false;

    //チェックエラー警告フラグ
    protected boolean _blnCheckErrAlertFlag = false;

    //送信画面遷移フラグ (仮置き照合で使用)
    protected boolean _blnSendFlag = false;
    //仮置き登録済みチェックフラグ (仮置き照合で使用)
    boolean _existTempAseetFlg = false;

    //確認画面
    protected static final int BUTTON_REGISTER = 1;
    protected static final int BUTTON_COMPLETE = 2;
    protected static final int BUTTON_CLEAR = 3;
    protected static final int BUTTON_MENU = 4;
    protected static final int BUTTON_NOMATCH = 5;
    //探索画面
    private static final int BUTTON_SEARCH = 6;

    //送信画面
    private static final int BUTTON_MAT_LEASE_SEND = 7;

    protected static final int BUTTON_RETURN = 2;

    //貸出照合から直接送信用のコード
    //送信成功
    private int MAT_LEASE_SEND_SUCCESS = 1001;
    //送信失敗
    private int MAT_LEASE_SEND_FAILURE = 1002;

    protected SearchFactory _SearchData = new SearchFactory();

    private String _AssetNo = "";

    //アダプター保存
    private SimpleAdapter _arrayAdapter;

    //HT貸出時のアラート表示有無 20230830 ADD Kawamoto v2.4.1
    protected boolean _LeaseAlertDisplay = false;

    //HT返却時のアラート表示有無 20230830 ADD Kawamoto v2.4.1
    protected boolean _ReturnAlertDisplay = false;

    //メニュー選択画面
    private static String _MenuClick = "";

    //工事選択No
    private String _ConstRuctionNo = "";

    //仮置きID
    private String _TempId = "";

    //表示リスト保存
    private List<Map<String, String>> _list = new ArrayList<Map<String, String>>();

    //貸出(照合)仮置き資産あるかチェック用
    LinkedHashMap<String, EntryProcessDataDetail> _data;

    /**
     * ヘッダー背景色区分定義
     */
    protected enum ListHeaderBkgColorDiv {
        NORMAL,
        WARN,
        ERROR
    }
    protected ListHeaderBkgColorDiv _headerColorDiv = ListHeaderBkgColorDiv.NORMAL;

    /**
     * Createイベント
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        SQLiteDatabase db = null;
        try {

            Intent intent = getIntent();
            //メニューで選択したメニュー取得
            _MenuClick = intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT);

            //工事選択No取得
            _ConstRuctionNo = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO);

            //仮置きの場合
            _TempId = intent.getStringExtra(CommonConst.VIEW_KEY_HT_TEMP_ID);

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            super.onCreate(savedInstanceState);
            //貸出の場合
            if (_ProcessData.isLease()) {
                //HT貸出時のアラート表示あり
                if (SettingFactory.GetLeaseAlertDisplay(db)[0].equals(CommonConst.LEASE_ALERT_DISPLAY.YES.getString())) {
                    setContentView(R.layout.activity_summary_lease_alert);
                    _LeaseAlertDisplay = true;

                }
                //HT貸出時のアラート表示なし
                else {
                    setContentView(R.layout.activity_summary_lease);
                    _LeaseAlertDisplay = false;
                }

                //貸出(照合)の場合
                if(_ProcessData.isMatLease()){
                    _LeaseAlertDisplay = false;
//                    // 「貸出(照合)」設定
//                    _ProcessData.setIsMatLease(false);
                }
            }
            //返却の場合
            else if(_ProcessData.isReturn()){
                if(SettingFactory.GetReturnAlertDisplay(db).equals(CommonConst.RETURN_ALERT_DISPLAY.YES.getString())) {
                    //HT返却時のアラート表示あり
                    setContentView(R.layout.activity_summary_alert);
                    _ReturnAlertDisplay = true;
                }
                else{
                    //HT返却時のアラート表示なし
                    setContentView(R.layout.activity_summary);
                    _ReturnAlertDisplay = false;
                }
            }
            else if(_MenuClick != null &&(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN) ||
                    _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT))){
                    setContentView(R.layout.activity_summary_match_inout);

                    //探索ボタン非活性
                    setButtonEnabled(false);
            }
            //その他
            else {
                setContentView(R.layout.activity_summary);
            }

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            // プログレスダイアログの表示
            C_Progres.StartProgressDialog(this, "Loading...");

            db.beginTransaction();

            //前画面からデータを取得する
            //_ProcessData = (ProcessFactory)getIntent().getSerializableExtra(CommonConst.VIEW_KEY_PROCESS_DATA);

            //文言表示
            InitializeWord();

            //画面初期化
            //InitializeActivity();     20200226 Del Sasaki V1.2
            //_MenuClickがnullでない場合
            if(_MenuClick != null){
                //持出照合/持帰照合/照合返却の場合
                if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN) ||
                        _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT) ||
                        _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)) {
                    //一時表に登録する
                    _ProcessData.CreateMatTempTable(db);
                }else {
                    //一時表に登録する
                    _ProcessData.CreateTempTable(db);
                }
            }else {
                //一時表に登録する
                _ProcessData.CreateTempTable(db);
            }

            listView = (ListView) findViewById(R.id.listview_Summary);

            //貸出照合の場合
            if(_ProcessData.isMatLease()){
                //照合予約機能の場合
                if(SettingFactory.GetMatchingReserveFunction(db).equals(CommonConst.MATCHING_RESERVE_FUNCTION.YES.getString())){
                    //送信処置
                    _blnSendFlag = true;
                    //送信ボタン
                    ((TextView)findViewById(R.id.btnSummaryConfirm))
                            .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SEND_BUTTON, "", this));        //送信
                }else{
                    //登録処置
                    _blnSendFlag = false;
                    //登録ボタン
                    ((TextView)findViewById(R.id.btnSummaryConfirm))
                            .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));        //登録
                }
            }

            //一覧表示する
            ViewLeaseData(db);

            //20200228 Del Start Sasaki V1.2
            //登録ボタン押下不可
            //if (_ProcessData.CountAll() <= 0 ) {
            //SetButtonEnabled(false);
            //}
            //20200228 Del End Sasaki V1.2
            //ボタン押下不可
            AllResetButton(false);

            //クリアボタン押下
            final Activity activity = this;
            findViewById(R.id.btnSummarClear).setOnClickListener(new View.OnClickListener() {
                 @Override
                 public void onClick(View view) {
                     //確認メッセージを表示する
                     ConfirmMessage(BUTTON_CLEAR, CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_CLEAR);
                 }
            });

            if(_MenuClick != null &&(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN) ||
                    _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT))){
                //一覧表示のクリック処理
                listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                    @Override
                    public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                        Map<String, String> map = (HashMap)listView.getItemAtPosition(position);
                        String contMatchDiv = map.get(CommonConst.List_VIEW_KEY.Mat.getString());

                        //未読み取りデータのみ選択可能
                        if(contMatchDiv.equals(CommonConst.CONT_MATCH_DIV.MAT_NO.getString())){
                            //前回と違うところを選択した場合
                            if (position != previousSelectedViewPosition){
                                //選択データを取得する
                                ListView listView = (ListView)parent;

                                Drawable background = view.getBackground();
                                int color = 0;
                                if (background instanceof ColorDrawable) {
                                    // ColorDrawable にキャストして色を取得
                                    color = ((ColorDrawable) background).getColor();
                                }

                                //_SearchDataを初期化
                                _SearchData.Clear();

                                // 前回選択されたアイテムがあれば、その背景色を元に戻す
                                if (previousSelectedView != null) {
                                    //元に戻す
                                    if(previousSelectedColor != 0){
                                        //前回の色に戻す
                                        previousSelectedView.setBackgroundColor(previousSelectedColor);
                                    }else{
                                        previousSelectedView.setBackgroundColor(Color.TRANSPARENT);
                                    }
                                }
                                _AssetNo = map.get(CommonConst.List_VIEW_KEY.Main.getString());
                                _SearchData.StockAssetNo(_AssetNo);

                                CreateTempTable();

                                // 現在選択されたアイテムの背景色を変更
                                view.setBackgroundResource(R.color.ButtonChange); // 直接背景色を変更

                                // 今回の選択アイテムを保存
                                previousSelectedView = view;
                                previousSelectedViewPosition = position;
                                previousSelectedColor = color;
                                //探索ボタン活性
                                setButtonEnabled(true);
                            }
                        }

                    }
                });

                //探索ボタン押下
                findViewById(R.id.btnSearch).setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View view) {
                        //探索画面へ
                        Intent intent = new Intent(getApplication(), SearchTag.class);
                        intent.putExtra(CommonConst.VIEW_KEY_SEARCH_ASSET, _AssetNo);    //資産番号
                        intent.putExtra(CommonConst.VIEW_KEY_SEARCH_DATA, _SearchData);
                        startActivityForResult(intent, BUTTON_SEARCH);
                    }
                });
            }

            //登録ボタン Clickイベント
            findViewById(R.id.btnSummaryConfirm).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {

                    //_MenuClickがnullでない場合
                    if(_MenuClick != null){
                        //持出照合/持帰照合/照合返却の場合
                        if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN) ||
                                _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT) ||
                                _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)){
                            //未読み取り件数が1件以上の場合
                            if (_ProcessData.CountMatAsset() > 0 ){
                                //未読取済数が0ではない場合の警告メッセージ
                                //照合できていない資産があります。よろしいですか？
                                ConfirmMessage(BUTTON_NOMATCH, CommonConst.VIEW_WORD_EXIST_NOMATCH);
                                return;
                            }
                        }

                        //仮置の場合
                        if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST)){
                            //他仮置きが1件以上の場合
                            if (_ProcessData.getAlreadyTempCnt() > 0 ){
                                //未読取済数が0ではない場合の警告メッセージ
                                //他の仮置き登録した資産があります。よろしいですか？
                                ConfirmMessage(BUTTON_NOMATCH, CommonConst.VIEW_WORD_OTHER_TEMPDATA);
                                return;
                            }
                        }
                    }

                    //20231024 ADD Sato v2.4.1 START
                    if (_blnCheckErrAlertFlag) {
                        //上限期限詳細表示が有効な場合の警告メッセージ
                        ConfirmMessage(BUTTON_REGISTER, CommonConst.VIEW_WORD_ALL_CONFIRM_CHECK_ERR_ASSET);
                        return;
                    }
                    //20231024 ADD Sato v2.4.1 END

                    if (_blnAlertFlag) {
                        //警告の確認メッセージを表示する
                        ConfirmMessage(BUTTON_REGISTER, GetAlertMessage());
                    } else {
                        //正常の確認メッセージを表示する
                        ConfirmMessage(BUTTON_REGISTER, CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_REGISTER);
                    }
                }
            });

            db.setTransactionSuccessful();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            //エラーログ
            if (db != null) {
                db.endTransaction();
                db.close();
                db = null;
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, Common.toString(ex), this);
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.endTransaction();
            }
            C_Progres.StopProgressDialog();
        }
    }

    /**
     * メニュー選択イベント
     * @param item
     * @return
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        try {

            switch(item.getItemId()) {
                case android.R.id.home:     //戻るボタン
                    //戻る処理
                    onReturn();
                    return true;

                case R.id.action_menu:
                    return returnMenu();
            }
            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, Common.toString(ex), this);
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            return false;
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
     * レジュームイベント
     */
    @Override
    public void onResume() {
        super.onResume();
    }

    /**
     *  ポーズイベント
     */
    @Override
    public void onPause() {
        super.onPause();
    }

    //画面初期化
    private void InitializeActivity() {

        //件数表示
        ClearCountView();

    }

    /**
     * 件数表示
     */
    protected void ClearCountView() {
        //件数表示
        ((TextView) findViewById(R.id.txtSummaryListCount)).setText(
                Long.toString(_ProcessData.CountAll()));                                        //件数
        //20210918 CHG Takase V2.0 START
        ((TextView) findViewById(R.id.txtSummaryListNoAuthority)).setText(Long.toString(_ProcessData.CountNoAuth()));        //権限無しエラー件数

        //20231116 UPD Sato v2.4.1 STRAT
//        ((TextView) findViewById(R.id.txtSummaryListCountErr)).setText(
//                Long.toString(_ProcessData.CountAll() - _ProcessData.CountOKAll() - _ProcessData.CountLeaseLimitErr() - _ProcessData.CountNoAuth()));   //エラー件数
        ((TextView) findViewById(R.id.txtSummaryListCountErr)).setText(String.valueOf(_ProcessData.CountMstNoData()));   //マスタ未登録件数
        //20231116 UPD Sato v2.4.1 END

        //20210918 CHG Takase V2.0 END
        if (_ProcessData.isLease()) {
            ((TextView) findViewById(R.id.txtSummaryLeaseLimitWarning)).setText(
                    Long.toString(_ProcessData.CountLeaseLimitWarning()));                                 //貸出上限警告件数
            ((TextView) findViewById(R.id.txtSummaryLeaseLimitErr)).setText(
                    Long.toString(_ProcessData.CountLeaseLimitErr()));                                     //貸出上限エラー件数
        }

        //貸出(照合)の場合
        if(_ProcessData.isMatLease()){
            //貸出不可件数非表示
            findViewById(R.id.lblSummaryListNoAuthority).setVisibility(View.GONE);
            findViewById(R.id.txtSummaryListNoAuthority).setVisibility(View.GONE);
            //リース期限エラー件数非表示
            findViewById(R.id.lblSummaryLeaseLimit).setVisibility(View.GONE);
            findViewById(R.id.txtSummaryLeaseLimit).setVisibility(View.GONE);
            //○○期限エラー件数非表示
            findViewById(R.id.lblSummaryGenericLimit).setVisibility(View.GONE);
            findViewById(R.id.txtSummaryGenericLimit).setVisibility(View.GONE);
//            // 「貸出(照合)」設定
//            _ProcessData.setIsMatLease(false);
        }

        //20230830 ADD Kawamoto v2.4.1 Start
        //HT貸出時のアラート表示ありの場合
        if (_LeaseAlertDisplay) {
            ((TextView) findViewById(R.id.txtSummaryLeaseLimit)).setText(
                    Long.toString(_ProcessData.CountLeaseUntilErr()));        //リース期限エラー件数
            ((TextView) findViewById(R.id.txtSummaryGenericLimit)).setText(
                    Long.toString(_ProcessData.CountGenericUntilErr()));      //○○期限エラー件数
        }
        //20230830 ADD Kawamoto v2.4.1 End



        //HT返却時のアラート表示有  20230906 ADD okubo V2.4.1 Start
        else if (_ReturnAlertDisplay) {
            ((TextView)findViewById(R.id.txtSummaryReturnLimit)).setText(
                    Long.toString(_ProcessData.CountReturnUntilErr()));       //返却期限エラー件数
            ((TextView) findViewById(R.id.txtSummaryLeaseLimit)).setText(
                    Long.toString(_ProcessData.CountLeaseUntilErr()));        //リース期限エラー件数
            ((TextView) findViewById(R.id.txtSummaryGenericLimit)).setText(
                    Long.toString(_ProcessData.CountGenericUntilErr()));      //○○期限エラー件数
            ((TextView) findViewById(R.id.txtSummaryLeaseLimitWarning)).setText(
                    Long.toString(_ProcessData.CountLeaseLimitWarning()));                                 //貸出上限警告件数
            ((TextView) findViewById(R.id.txtSummaryLeaseLimitErr)).setText(
                    Long.toString(_ProcessData.CountLeaseLimitErr()));                                     //貸出上限エラー件数
        }
        //20230906 ADD okubo V2.4.1 end
    }

    /** ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * */
    protected void SetButtonEnabled(boolean prmEnabled) {
        //登録ボタン
        ((Button)findViewById(R.id.btnSummaryConfirm)).setEnabled(prmEnabled);

        if (prmEnabled == false) {
            ((Button)findViewById(R.id.btnSummaryConfirm)).setBackground(getDrawable(R.drawable.execute_button_diable));
            ((Button)findViewById(R.id.btnSummaryConfirm)).setTextColor(Color.parseColor("#a9a9a9"));
        } else {
            ((Button)findViewById(R.id.btnSummaryConfirm)).setBackground(getDrawable(R.drawable.execute_button));
            ((Button)findViewById(R.id.btnSummaryConfirm)).setTextColor(Color.parseColor("#ffffff"));
        }
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {

        SetButtonEnabled(blnEnabled);

        //クリアボタン
        if (blnEnabled) {
            ((Button)findViewById(R.id.btnSummarClear)).setBackground(getDrawable(R.drawable.execute_button_clear));
            ((Button)findViewById(R.id.btnSummarClear)).setTextColor(Color.parseColor("#ffffff"));
        } else {
            ((Button)findViewById(R.id.btnSummarClear)).setBackground(getDrawable(R.drawable.execute_button_clear_disable));
            ((Button)findViewById(R.id.btnSummarClear)).setTextColor(Color.parseColor("#a9a9a9"));
        }
    }

    /**
     * 一覧表示する
     * @param db データベース接続
     */
    private void ViewLeaseData(final SQLiteDatabase db) {
        final Activity activity = this;     //20230907 ADD okubo v2.4.1
        //一覧の背景色のカラーコードを取得
        int bkgNormalColor = Color.parseColor(SettingFactory.GetSummaryBackgroundColor(db, CommonConst.SETTEI_KEY1_NORMAL));
        int bkgWarnColor = Color.parseColor(SettingFactory.GetSummaryBackgroundColor(db, CommonConst.SETTEI_KEY1_WARN));
        int bkgErrColor = Color.parseColor(SettingFactory.GetSummaryBackgroundColor(db, CommonConst.SETTEI_KEY1_ERROR));

        new Thread() {
            @Override
            public void run() {
                try {
                    Intent intent = getIntent();

                    LinkedHashMap<String, EntryProcessDataDetail> data;
                    //_MenuClickがnullでない場合
                    if(_MenuClick != null){
                        //持出照合/持帰照合/照合返却/仮置きの場合
                        if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN) ||
                                _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT) ||
                                _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN) ||
                                _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST)) {
                            data = _ProcessData.SelectMatAssetData(activity, db, intent.getStringExtra(CommonConst.VIEW_KEY_DATE), _LeaseAlertDisplay,  _ReturnAlertDisplay);
                        }else {
                            //読込データを取得する        //貸出・返却のアラート表示有無を渡す  20230907 ADD okubo v2.4.1
                            data = _ProcessData.SelectAssetData(activity, db, intent.getStringExtra(CommonConst.VIEW_KEY_DATE), _LeaseAlertDisplay,  _ReturnAlertDisplay);
                        }
                    }else {
                        //読込データを取得する        //貸出・返却のアラート表示有無を渡す  20230907 ADD okubo v2.4.1
                        data = _ProcessData.SelectAssetData(activity, db, intent.getStringExtra(CommonConst.VIEW_KEY_DATE), _LeaseAlertDisplay,  _ReturnAlertDisplay);
                    }

                    //読み取りデータ保存
                    _data = data;

                    //表示リスト
                    final List<Map<String, String>> list = new ArrayList<Map<String, String>>();

                    //取得したデータを一覧表示に設定
                    for  (Map.Entry<String, EntryProcessDataDetail>entry : data.entrySet()){
                        //表示データを設定する
                        Map<String, String> map = new HashMap<String, String>();

                        //map.put(CommonConst.List_VIEW_KEY.Main.getString(), entry.getKey());                        //資産番号
                        //20230907 ADD okubo v2.4.1 Start
                        String tempKey = entry.getKey();                        //資産番号
                        String Warning = entry.getValue().getWarningDisplay();  //ワーニグ表示
                        if(Warning.length() > 0) {
                            tempKey = tempKey + "(" + Warning + ")";    //資産番号の後ろにワーニング
                        }
                        map.put(CommonConst.List_VIEW_KEY.Main.getString(), tempKey);
                        map.put(CommonConst.List_VIEW_KEY.Aux.getString(), Warning);    //ワーニング
                        //20230907 ADD okubo v2.4.1 End

                        map.put(CommonConst.List_VIEW_KEY.Sub.getString(), entry.getValue().getAssetName());        //資産名称
                        map.put(CommonConst.List_VIEW_KEY.Hidden.getString(), entry.getValue().getAssetStatus());   //資産状態
                        list.add(map);

                        //ヘッダー背景色区分を設定
                        setHeaderBkgColorDiv(entry.getValue().getAssetStatus(), entry.getValue().getAssetNo(), Warning);
                    }

                    //リスト並び替え
                    sortList(list);

                    //初期化
                    final SimpleAdapter arrayAdapter = InitializeAdapter(list);
                    final ListView listView = (ListView) findViewById(R.id.listview_Summary);
                    final LinearLayout listHeader = findViewById(R.id.ListViewHeader);

                    _list = list;

                    //一覧の背景色のカラーコードを取得
                    final int backgroundColor;
                    switch (_headerColorDiv) {
                        case WARN:
                            backgroundColor = bkgWarnColor;
                            break;
                        case ERROR:
                            backgroundColor = bkgErrColor;
                            break;
                        default:
                            backgroundColor = bkgNormalColor;
                            break;
                    }

                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            //リストに追加
                            listView.setAdapter(arrayAdapter);
                            //adapter保存
                            _arrayAdapter = arrayAdapter;

                            //一覧の背景色を設定
                            listHeader.setBackgroundColor(backgroundColor);

                            //件数表示
                            ClearCountView();

                            //ボタン活性 20200408 Add Sasaki V1.2
                            if (_ProcessData.CountOKAll() <= 0 ) {
                                AllResetButton(true);
                                SetButtonEnabled(false);
                            }
                            else {
                                AllResetButton(true);
                            }
                        }
                    });
                }
                finally {
                    C_Progres.StopProgressDialog();
                }
            }
        }.start();
    }


    /**
     * 一覧表示の初期化処理
     * @return 一覧表示用Adapter
     */
    protected SimpleAdapter InitializeAdapter(List list){

        //Adapterの設定   //ワーニングキーとワーニング表示Text(非表示)追加     //20230907 ADD okubo v2.4.1
        SimpleAdapter adapter = new SimpleAdapter(
                this,
                list,
                //android.R.layout.simple_list_item_2,
                R.layout.custom_listview,
                new String[] {CommonConst.List_VIEW_KEY.Main.getString(), CommonConst.List_VIEW_KEY.Sub.getString(), CommonConst.List_VIEW_KEY.Hidden.getString(), CommonConst.List_VIEW_KEY.Aux.getString()},
                new int[] {android.R.id.text1, android.R.id.text2, R.id.text3, R.id.textAux}
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

                //背景色の色を設定する(1行ごとに色を変える)
                if (position % 2 == 0){
                    view.setBackgroundResource(R.color.ListViewBack);
                }
                else {
                    view.setBackgroundResource(R.color.ListViewBack2);
                }

                //資産状態を取得する
                Map<String, String> item = (HashMap)this.getItem(position);
                String strAssetStatus = item.get(CommonConst.List_VIEW_KEY.Hidden.getString());
                String strAssetNo = item.get(CommonConst.List_VIEW_KEY.Main.getString());       //20200225 Add Sasaki V1.2
                //20230908 ADD okubo v2.4.1 Start
                String strWarning = item.get(CommonConst.List_VIEW_KEY.Aux.getString());        //ワーニング表示(非表示)追加
                strAssetNo = strAssetNo.replace(strWarning, "");
                strAssetNo = strAssetNo.replace("()", "");  //ワーニング除去

                //資産状態によって文字色を変更する(サブクラスで実装する)
                //int intColor = SettingListColor(strAssetStatus);                               //20200225 Del Sasaki V1.2
                //int intColor = SettingListColor(strAssetStatus, strAssetNo);                    //20200225 Add Sasaki V1.2
                int intColor = SettingListColor(strAssetStatus, strAssetNo, strWarning);          //ワーニング表示(非表示)追加
                //20230907 ADD okubo v2.4.1 End

                //色を設定
                ((TextView) view.findViewById(android.R.id.text1)).setTextColor(intColor);      //資産番号
                ((TextView) view.findViewById(android.R.id.text2)).setTextColor(intColor);      //資産名称

                return view;
            }
        };

        return adapter;
    }

    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     * @param requestCode CustomDialg 実行時 requestCode
     * @param params CustomDialg に受渡した引数
     */
    final Activity nowActivity = this;
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        if (params == null) {
            return;
        }

        //ボタン非活性
        AllResetButton(false);
        try {
            //呼出元で分岐する
            switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
                case BUTTON_REGISTER:      //登録ボタン
                    C_Progres.StartProgressDialog(this, "Proceccing...");
                    final Context context = this;
                    final Activity activity = this;

                    new Thread() {
                        @Override
                        public void run() {
                            SQLiteDatabase db = null;
                            try {
                                //DB接続
                                DatabaseHelper dbHelper = new DatabaseHelper(context);
                                db = dbHelper.getWritableDatabase();
                                db.beginTransaction();

                                //仮置き登録済みチェックフラグ 初期化
                                _existTempAseetFlg = false;

                                //貸出(照合)の場合
                                if(_ProcessData.isMatLease()){
                                    for  (Map.Entry<String, EntryProcessDataDetail>entry : _data.entrySet()){
                                        //資産番号
                                        String assetNo = entry.getKey();
                                        if(_ProcessData.ExistTempListAssetNo(assetNo)){
                                            AssetModel asset = AssetFactory.Select(db, assetNo);
                                            //貸出中でない場合
                                            if(!CommonConst.ASSET_STATUS.Lease.getString().equals(asset.getAssetStatus())){
                                                //仮置き中のデータ有
                                                _existTempAseetFlg = true;
                                                return;
                                            }

                                        }
                                    }
                                }

                                //登録処理を行う
                                RegisterProcess(db);

                                if (_ProcessData.isLease()
                                  && SettingFactory.GetLendingContinuous(db).equals(CommonConst.LENDING_CONTINUOUS.YES.getString())) {
                                    //連続貸出 クリアしない  //20230911 ADD okubo v2.4.1
                                }else {
                                    //クリア
                                    _ProcessData.AllClear();    //20200217 Add Sasaki
                                }

                                //commit
                                db.setTransactionSuccessful();


                            } catch (Exception e){
                                //エラーログ
                                if (db != null) {
                                    db.endTransaction();
                                    db.close();
                                    db = null;
                                }
                                LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                        SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LIST, "", activity) + ":" + Common.toString(e), activity);
                                runOnUiThread(new Runnable() {
                                    @Override
                                    public void run() {
                                        //ボタン活性
                                        AllResetButton(true);
                                    }
                                });
                            } finally {
                                if (db != null) {
                                    db.endTransaction();
                                }
                                C_Progres.StopProgressDialog();

                                //仮置きした資産が存在しているか
                                if(_existTempAseetFlg == true){
                                    //エラーメッセージ表示
                                    showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_MATCH_LEASE_MSG_EXSIT_TEMP_ASEET, "", activity));
                                    runOnUiThread(new Runnable() {
                                        @Override
                                        public void run() {
                                            //ボタン活性
                                            AllResetButton(true);
                                        }
                                    });
                                }else{
                                    //登録or送信処理
                                    //ボタンが送信の場合
                                    if (_blnSendFlag == true){
                                        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                                        LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity), activity);

                                        SettingValue setting = new SettingValue();
                                        if (setting.getTransfer(nowActivity).equals(CommonConst.TRANSFER_METHOD.USB.getString())) {
                                            //ＵＳＢ接続確認画面へ
                                            Intent intent = new Intent(getApplication(), UsbConfirmActivity.class);
                                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_ALL_SEND_DATA);
                                            startActivityForResult(intent, BUTTON_MAT_LEASE_SEND);
                                        } else {
                                            //棚卸データ送信画面へ
                                            Intent intent = new Intent(getApplication(), WebSendActivity.class);
                                            startActivityForResult(intent, BUTTON_MAT_LEASE_SEND);
                                        }
                                    }
                                }
                            }
                        }
                    }.start();

                    break;

                case BUTTON_CLEAR:      //クリアボタン

                    //クリア処理を行う
                    ClearRfidTagList();

                    //仮置き、照合返却、貸出データ削除
                    DataClear();

                    //ソートデータクリア
                    _ProcessData.ClearSortData();
                        //前画面へ戻る
                        onReturn();

                    break;

                case BUTTON_COMPLETE:   //登録完了後の確認画面でのOKボタン

                    //前画面へ戻る
                    onReturnComplete();

                    break;

                case BUTTON_MENU:
                    //クリア
                    _ProcessData.AllClear();    //20200217 Add Sasaki

                    //仮置き、照合返却、貸出データ削除
                    DataClear();

                    //メニュー画面へ戻る
                    onMenuReturn();

                    break;

                case BUTTON_NOMATCH:
                    //読み取り件数0
                    //20231024 ADD Sato v2.4.1 START
                    if (_blnCheckErrAlertFlag) {
                        //上限期限詳細表示が有効な場合の警告メッセージ
                        ConfirmMessage(BUTTON_REGISTER, CommonConst.VIEW_WORD_ALL_CONFIRM_CHECK_ERR_ASSET);
                        return;
                    }
                    //20231024 ADD Sato v2.4.1 END

                    if (_blnAlertFlag) {
                        //警告の確認メッセージを表示する
                        ConfirmMessage(BUTTON_REGISTER, GetAlertMessage());
                    } else {
                        //正常の確認メッセージを表示する
                        ConfirmMessage(BUTTON_REGISTER, CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_REGISTER);
                    }

                    break;
            }

        } catch (Exception e){
            //ボタン活性
            AllResetButton(true);
        }
    }

    /**
     * キー入力イベント
     * @param keyCode
     * @param event
     * @return
     */
    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        try {
            switch (keyCode)  {
                case KeyEvent.KEYCODE_BACK:
                    //Back ボタン
                    finish();
                    return true;
                case R.id.action_menu:
                    return returnMenu();
            }
            return super.onKeyDown(keyCode, event);
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * クリア処理
     */
    private void ClearRfidTagList() {
        //照合一致数クリア
        _ProcessData.ClearListCnt();
        //リスト不一致数クリア
        _ProcessData.ClearNoListCnt();
        //読込データのクリア
        _ProcessData.Clear();

    }

    /**
     * 確認メッセージを表示する
     * @param intButton
     * @param strMessage
     */
    private void ConfirmMessage(int intButton, String strMessage) {
        //メッセージBOX
        final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, intButton);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
        dialog.message(SystemSetting.getString(strMessage, "", this));
        dialog.params(bundle);
        dialog.show();
    }


    /**
     * 前画面に戻る処理
     */
    protected void onReturn() {
        //資産読取画面へ
        Intent intent = new Intent();
        //intent.putExtra(CommonConst.VIEW_KEY_PROCESS_DATA, _ProcessData);
        setResult(RESULT_OK, intent);

        this.finish();
    }

    /**
     * 登録ボタンOK後の画面遷移処理
     */
    protected void onReturnComplete() {
        onReturn();
    }

    /**
     * メニューへ戻る処理
     * @return
     */
    private boolean returnMenu() {
        if (_ProcessData.CountAll() > 0) {
            onReturnConfirm(BUTTON_MENU);
            return true;
        } else {
            //クリア
            _ProcessData.AllClear();    //20200217 Add Sasaki
            onMenuReturn();
            return true;
        }
    }

    /**
     * 確認画面を表示する
     * @param intButton
     */
    protected void onReturnConfirm(int intButton) {
        //確認画面を表示し、OKの場合true、キャンセルの場合false
        final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, intButton);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
        dialog.message(SystemSetting.getString(
                CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_RETURN, "", this));               //登録されていない読取データは破棄されます。よろしいですか？
        dialog.params(bundle);
        dialog.show();
    }

    /**
     * 一覧のヘッダー背景色区分を設定します
     * ※背景色を変更したい場合は、各画面ごとにオーバーライドしてください
     * @param strAssetStatus　資産状態
     * @param strAssetNo　資産番号
     * @param strWarning　警告・エラーメッセージ
     */
    protected void setHeaderBkgColorDiv(String strAssetStatus, String strAssetNo, String strWarning) {}

    /**
     * 文言表示
     * サブクラスで実装する
     */
    protected abstract void InitializeWord();

    protected void CreateTempTable(){};

    /**
     * 登録処理
     * サブクラスで実装する
     */
    protected abstract void RegisterProcess(SQLiteDatabase db);

    /**
     * 資産状態によって文字色を設定する
     * サブクラスで実装する
     * @param strAssetStatus
     * @return 文字色
     */
    //protected abstract int SettingListColor(String strAssetStatus);                               //20200225 Del Sasaki V1.2
    //protected abstract int SettingListColor(String strAssetStatus, String strAssetNo);          //20200225 Add Sasaki V1.2
    protected abstract int SettingListColor(String strAssetStatus, String strAssetNo, String strWarning);   //ワーニング表示(非表示)追加   20230907 ADD okubo v2.4.1

    protected abstract String GetAlertMessage();

    /**
     * ボタンの非活性、活性をします
     * @param blnEnable
     */
    private void setButtonEnabled(boolean blnEnable) {
        if (blnEnable) {
            findViewById(R.id.btnSearch).setEnabled(true);
            findViewById(R.id.btnSearch).setBackground(getDrawable(R.drawable.execute_button_clear));
            ((Button)findViewById(R.id.btnSearch)).setTextColor(Color.parseColor("#ffffff"));
        } else {
            findViewById(R.id.btnSearch).setEnabled(false);
            findViewById(R.id.btnSearch).setBackground(getDrawable(R.drawable.execute_button_clear_disable));
            ((Button)findViewById(R.id.btnSearch)).setTextColor(Color.parseColor("#a9a9a9"));
        }
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
                    //resultCodeが-1であるとき
                    if(resultCode == RESULT_OK){
                        //選択したpositionのものを変更
                        Object item = _arrayAdapter.getItem(previousSelectedViewPosition);

                        //選択したデータの資産番号取得
                        String assetNo = ((Map<String, String>) item).get(CommonConst.List_VIEW_KEY.Main.getString());

                        //発見した資産が未読取にある場合
                        if(_ProcessData.ExistCollationViewAssetNo(assetNo)){
                            //該当する資産番号のデータ削除
                            _ProcessData.RemoveCollationViewAssetNo(assetNo);
                        }
                        //再描写
                        //_arrayAdapter.notifyDataSetChanged();
                        //選択したデータを一番後ろに並び替え
                        sortList(_list);
                        //新しいアダプター作成
                        SimpleAdapter arrayAdapter = InitializeAdapter(_list);
                        //新しいアダプターに更新
                        _arrayAdapter = arrayAdapter;

                        //前回の選択初期化
                        previousSelectedViewPosition = -1;

                        //探索ボタン非活性
                        setButtonEnabled(false);

                        TextView txtSummaryListCountErr = findViewById(R.id.txtSummaryListCountErr);
                        int currentValue = Integer.parseInt(txtSummaryListCountErr.getText().toString());
                        // 1を引く
                        int newValue = currentValue - 1;
                        // 新しい値をTextViewに設定
                        txtSummaryListCountErr.setText(String.valueOf(newValue));

                        //選択した資産番号の照合結果を更新
                        //送信用のデータ取得
                        HashMap<String, EntryProcessDataDetail>map = _ProcessData.GetRegisterAsset();
                        EntryProcessDataDetail ProcessData = map.get(assetNo);
                        ProcessData.SetContMatchDiv(CommonConst.CONST_MATCH_DIV.OK.getString());

                        //照合一致数カウントアップ
                        _ProcessData.cntListCnt();

                        //再表示
                        listView.setAdapter(arrayAdapter);
                    }
                    break;

                case BUTTON_MAT_LEASE_SEND:  //送信画面から戻る
                    //成功した場合
                    if(resultCode == MAT_LEASE_SEND_SUCCESS){
                        onReturnComplete();
                    }else if(resultCode == MAT_LEASE_SEND_FAILURE){
                        //失敗した場合
                        SetButtonEnabled(true);
                        AllResetButton(true);
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

    public static <T> void sortList(List<Map<String, String>> list) {
        //ソート1
        List<Map<String, String>> sort1List = new ArrayList<>();
        //ソート2
        List<Map<String, String>> sort2List = new ArrayList<>();
        //ソート3
        List<Map<String, String>> sort3List = new ArrayList<>();
        //ソート4
        List<Map<String, String>> sort4List = new ArrayList<>();

        //_MenuClickがnullでない場合
        if(_MenuClick != null){
            if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN) ||
                    _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT) ||
                    _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)) {
                //照合系
                //照合結果順　入っているデータ並び替え
                for (Map<String, String> item : list) {
                    System.out.println(item);
                    String assetNo = ((Map<String, String>) item).get(CommonConst.List_VIEW_KEY.Main.getString());

                    //ソート再計算
                    if (_ProcessData.ExistMasterAssetNo(assetNo)){
                        if(_ProcessData.ExistCollationAssetNo(assetNo)){
                            if(!_ProcessData.ExistCollationViewAssetNo(assetNo)){
                                //ソート4
                                sort4List.add(item);
                            }else{
                                //ソート1
                                sort1List.add(item);
                            }
                        }else{
                            //ソート2
                            sort2List.add(item);
                        }
                    }else{
                        //ソート3
                        sort3List.add(item);
                    }
                }

                //照合結果ごとに資産番号順を並び替え
                sort1List.sort(Comparator.comparing(map -> map.get(CommonConst.List_VIEW_KEY.Main.getString())));
                sort2List.sort(Comparator.comparing(map -> map.get(CommonConst.List_VIEW_KEY.Main.getString())));
                sort3List.sort(Comparator.comparing(map -> map.get(CommonConst.List_VIEW_KEY.Main.getString())));
                sort4List.sort(Comparator.comparing(map -> map.get(CommonConst.List_VIEW_KEY.Main.getString())));

                // リストすべて削除
                list.clear();

                //ソート1から順に入れ込み
                //各データ入れ込み
                if (sort1List.size() > 0){
                    for (Map<String, String> item : sort1List) {
                        // ソートしたリストで置き換え
                        list.add(item);
                    }
                }

                if (sort2List.size() > 0){
                    for (Map<String, String> item : sort2List) {
                        // ソートしたリストで置き換え
                        list.add(item);
                    }
                }

                if (sort3List.size() > 0){
                    for (Map<String, String> item : sort3List) {
                        // ソートしたリストで置き換え
                        list.add(item);
                    }
                }

                if (sort4List.size() > 0){
                    for (Map<String, String> item : sort4List) {
                        // ソートしたリストで置き換え
                        list.add(item);
                    }
                }

            }else if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST)){
                //仮置きの場合

                //仮置き　入っているデータ並び替え
                for (Map<String, String> item : list) {
                    System.out.println(item);
                    String assetNo = ((Map<String, String>) item).get(CommonConst.List_VIEW_KEY.Main.getString());

                    //ソート再計算
                    if (_ProcessData.ExistMasterAssetNo(assetNo)){
                        //照合データに含まれていない場合チェック
                        if(!_ProcessData.ExistCollationAssetNo(assetNo)){
                            //ソート2
                            sort2List.add(item);
                        }else{
                            //ほかで仮置き登録済みである場合
                            if(_ProcessData.ExistTempListAssetNo(assetNo)){
                                //ソート4
                                sort4List.add(item);
                            }else{
                                //ソート1
                                sort1List.add(item);
                            }
                        }
                    }else{
                        //ソート3
                        sort3List.add(item);
                    }

                }

                //照合結果ごとに資産番号順を並び替え
                sort1List.sort(Comparator.comparing(map -> map.get(CommonConst.List_VIEW_KEY.Main.getString())));
                sort2List.sort(Comparator.comparing(map -> map.get(CommonConst.List_VIEW_KEY.Main.getString())));
                sort3List.sort(Comparator.comparing(map -> map.get(CommonConst.List_VIEW_KEY.Main.getString())));
                sort4List.sort(Comparator.comparing(map -> map.get(CommonConst.List_VIEW_KEY.Main.getString())));

                // リストすべて削除
                list.clear();

                //ソート1から順に入れ込み
                //各データ入れ込み
                if (sort1List.size() > 0){
                    for (Map<String, String> item : sort1List) {
                        // ソートしたリストで置き換え
                        list.add(item);
                    }
                }

                if (sort2List.size() > 0){
                    for (Map<String, String> item : sort2List) {
                        // ソートしたリストで置き換え
                        list.add(item);
                    }
                }

                if (sort3List.size() > 0){
                    for (Map<String, String> item : sort3List) {
                        // ソートしたリストで置き換え
                        list.add(item);
                    }
                }

                if (sort4List.size() > 0){
                    for (Map<String, String> item : sort4List) {
                        // ソートしたリストで置き換え
                        list.add(item);
                    }
                }
            }
        }
    }

    /** 照合返却/貸出照合/仮置きの場合データ削除
     */
    private void DataClear(){
        //貸出(照合)の場合
        if(_ProcessData.isMatLease()){
            SQLiteDatabase db = null;

            try {
                DatabaseHelper dbHelper = new DatabaseHelper(this);
                db = dbHelper.getWritableDatabase();
                String sql;
                Cursor cursor;

                //工事番号で貸出リストテーブルのデータ取得
                //HT蓄積データの取得
                sql = LentAssetListFactory.GetAseetNoProcessIdData(_ConstRuctionNo);

                cursor = db.rawQuery(sql, null);

                while (cursor.moveToNext()){
                    //取得する
                    String AssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));
                    String ProcessLogId = cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID"));

                    //処理履歴IDで照合リストテーブルを削除
                    db.delete(CommonConst.TABLE_MATCH_LIST, "PROCESS_LOG_ID = ?", new String[] {ProcessLogId});

                    //資産番号で資産マスタテーブルを更新
                    AssetFactory.ClearUpdate(db, AssetNo);

                }

                //工事番号で貸出リストテーブルのデータ削除
                LentAssetListFactory.ConstNoDelete(db,_ConstRuctionNo);


            }catch (Exception ex) {
                //エラーログ
                if (db != null) {
                    db.close();
                }
                LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                        SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this) + ":" + Common.toString(ex), this);
                showErrorMessage(ex.getMessage());
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            } finally {
                if (db != null) {
                    db.close();
                }
            }
        }
        if(_MenuClick != null){
            //仮置きの場合
            if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST)){
                SQLiteDatabase db = null;

                try {
                    DatabaseHelper dbHelper = new DatabaseHelper(this);
                    db = dbHelper.getWritableDatabase();
                    String sql;
                    Cursor cursor;

                    //工事番号で仮置き資産リストテーブルのデータ取得
                    //HT蓄積データの取得
                    sql = TempAssetListFactory.GetAseetNoProcessIdTempIdData(_ConstRuctionNo,_TempId);

                    cursor = db.rawQuery(sql, null);

                    while (cursor.moveToNext()){
                        //取得する
                        String AssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));
                        String ProcessLogId = cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID"));

                        //処理履歴IDで照合リストテーブルを削除
                        db.delete(CommonConst.TABLE_MATCH_LIST, "PROCESS_LOG_ID = ?", new String[] {ProcessLogId});
                        //処理履歴IDで仮置きリストテーブルを削除
                        db.delete(CommonConst.TABLE_TEMP_LIST, "PROCESS_LOG_ID = ?", new String[] {ProcessLogId});
                    }
                    if (cursor != null) {
                        cursor.close(); // Cursor を閉じる
                    }

                    //工事番号、仮置きIDで仮置き資産テーブルのデータ削除
                    TempAssetFactory.ConstNoTempIdDelete(db,_TempId,_ConstRuctionNo);
                    //工事番号、仮置きIDで仮置き資産リストテーブルのデータ削除
                    TempAssetListFactory.ConstNoTempIdDelete(db,_ConstRuctionNo,_TempId);

                }catch (Exception ex) {
                    //エラーログ
                    if (db != null) {
                        db.close();
                    }
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this) + ":" + Common.toString(ex), this);
                    showErrorMessage(ex.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                } finally {
                    if (db != null) {
                        db.close();
                    }
                }
            }

            //照合返却の場合
            if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)){
                SQLiteDatabase db = null;

                try {
                    DatabaseHelper dbHelper = new DatabaseHelper(this);
                    db = dbHelper.getWritableDatabase();
                    String sql;
                    Cursor cursor;

                    //工事番号で返却リストテーブルのデータ取得
                    //HT蓄積データの取得
                    sql = RetAseetListFactory.GetAseetNoProcessIdData(_ConstRuctionNo);

                    cursor = db.rawQuery(sql, null);

                    while (cursor.moveToNext()){
                        //取得する
                        String AssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));
                        String ProcessLogId = cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID"));

                        //処理履歴IDで照合リストテーブルを削除
                        db.delete(CommonConst.TABLE_MATCH_LIST, "PROCESS_LOG_ID = ?", new String[] {ProcessLogId});

                    }
                    if (cursor != null) {
                        cursor.close(); // Cursor を閉じる
                    }

                    //工事番号で返却リストテーブルのデータ削除
                    RetAseetListFactory.ConstNoDelete(db,_ConstRuctionNo);

                }catch (Exception ex) {
                    //エラーログ
                    if (db != null) {
                        db.close();
                    }
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this) + ":" + Common.toString(ex), this);
                    showErrorMessage(ex.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                } finally {
                    if (db != null) {
                        db.close();
                    }
                }
            }
        }
    }
}

