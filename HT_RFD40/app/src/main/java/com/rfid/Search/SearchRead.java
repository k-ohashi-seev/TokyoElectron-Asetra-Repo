package com.rfid.Search;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.Switch;
import android.widget.TextView;

import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.LocationFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Taglink.TaglinkRead;
import com.rfid.common.AssetModel;
import com.rfid.common.Common;
import com.rfid.main.LoginMn;
import com.rfidpk.R;
import com.rfid.Factory.SearchFactory;
import com.rfid.common.BaseRead;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.CommonProgres;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.main.MainActivity;
import com.rfid.setting.SettingValue;

/**
 * 探索資産読取画面
 *
 */
public class SearchRead extends BaseRead implements CommonConfirmDialog.Callback, CommonOKDialog.Callback{

    //システム設定値
    private SettingValue C_Setting = new SettingValue();

    //戻り値用オブジェクト
    private Intent ResIntent = new Intent();

    //プログレスダイアログ クラス
    private CommonProgres C_Progres = new CommonProgres();

    //集計処理フラグ
    private boolean _SummaryProcesFlag = false;

    //初期化済みフラグ
    private boolean _InitializedFlag = false;

    //通信中フラグ(連続クリック防止)
    private Boolean _TransMesuarFlag = false;

    // 探索保持データ
    private SearchFactory _SearchData = new SearchFactory();

    //タグ開始フラグ
    private boolean _TagReadStartFlag = false;

    //タグモードフラグ
    private boolean _TagMode = true;

    //ボタン制御フラグ
    private boolean _ButtonClickFlag = false;

    //DB接続
    private SQLiteDatabase _db;

    private TextView text = null;

    private static final int ACTION_LOGOUT = 1;
    private final static int BUTTON_CLEAR = 2;
    private final static int BUTTON_RETURN = 3;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.activity_search_read);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //RFIDリーダーをOFFにする
            RFID_Read_Stop();

            //文言表示
            InitializeWord();

            //ボタン非活性
            SetButtonEnabled(false, false);

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            _db = dbHelper.getWritableDatabase();

            final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
            final Activity activity = this;
            final Context context = this;

            //操作時間のタイマーをセット
            StartTimeoutTimer(CommonConst.VIEW_WORD_MENU_SEARCH);

            //クリアボタン Clickイベント
            findViewById(R.id.btnClear).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    if (checkTimeout()) {return;}
                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_CLEAR);
                    dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", activity));  //クリア
                    dialog.message(SystemSetting.getString(
                            CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_CLEAR, "", activity));                //読取データをクリアします。よろしいですか？
                    dialog.params(bundle);
                    dialog.show();
                }
            });

            //一覧ボタン Clickイベント
            findViewById(R.id.btnDetail).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    if (checkTimeout()) {return;}
                    //詳細表示画面へ
                    Intent intent = new Intent(getApplication(), SearchSummary.class);
                    intent.putExtra(CommonConst.VIEW_KEY_SEARCH_DATA, _SearchData);
                    startActivityForResult(intent, BUTTON_SUMMARY);
                }
            });

            _InitializedFlag = false;

            //サービス開始
            startService();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH,"",this));                        //タイトル
        ((TextView)findViewById(R.id.labReadCount))
            .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_COUNT, "", this));                    //件数
        ((TextView)findViewById(R.id.labReturnReadTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_ASSET_NUMBER_QR, "", this));  //資産番号QR読取
        ((TextView)findViewById(R.id.btnClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));                //クリア
        ((TextView)findViewById(R.id.btnDetail))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LIST, "", this));                //一覧
    }

    // レジュームイベント
    @Override
    public void onResume() {
        try {
            super.onResume();

            //画面初期化
            InitializeActivity();

            //タイマー開始
            StartTimer();

            ReaderOn();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    //ポーズイベント
    @Override
    public void onPause() {
        try {
            super.onPause();

            //ReaderOff
            ReaderOff();

            //タイマー停止
            StopTimer();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * Barcode/QR読取りイベント
     * 親クラスから呼ばれます。
     * @param strData
     */
    protected void BarcodeDataReceived(String strData) {
        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(SearchRead.this);
            db = dbHelper.getWritableDatabase();
            String assetNo = _InventoryData.GetAssetNoByProcessedData(db, strData);
            if (assetNo.equals("")) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, " AssetNo is not found");
                //資産マスタに存在しない場合
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_REGISTER_NONE, "", this)
                        ,SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", this));
                return;
            }

            //資産マスタに存在しない場合、エラー
            AssetModel asset = AssetFactory.Select(db, assetNo);
            if ("".equals(asset.getAssetNo())) {
                //資産マスタに存在しない場合
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_REGISTER_NONE, "", this)
                        ,SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", this));
                return ;
            }

            if ("".equals(asset.getAssetID())) {
                //資産IDが未設定の場合
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_MSG_NO_ASSET_ID, "", this)
                        ,SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", this));
                return ;
            }

            //既に読込済みの場合、読み取らない
            if (_SearchData.ExistAssetNo(asset.getAssetNo())) {
                return;
            }

            //資産番号情報を溜め込む
            _SearchData.StockAssetNo(asset.getAssetNo());
            Log.d("資産番号", asset.getAssetNo());

            // 読取りデータ件数表示
            UpdateGranteCountDisp();

            //ボタン活性
            SetButtonEnabled(true, true);

            //5件以上になった場合、以降読取OFFする
            if (_SearchData.CountAll() >= 5) {
                //エラーメッセージを表示する
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_MSG_MAX_READ_COUNT,"",this),
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH,"",this));
                //ReaderOff
                ReaderOff();
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

    //  読取りデータ件数表示
    protected void UpdateGranteCountDisp() {
        //件数表示
        TextView _LabReadCoun =  findViewById(R.id.labReadCountVal);
        _LabReadCoun.setText(Integer.toString(_SearchData.CountAll()));
    }

    //画面初期化
    protected void InitializeActivity() {
        // 読取りデータ件数表示
        UpdateGranteCountDisp();
    }

    /**
     * リーダーをONにします。
     * 親クラスから呼ばれます。
     */
    protected void ReaderOn(){
        // バーコードOn
        Barcode_Read_Start();
    }

    /**
     * リーダーをOFFにします。
     * 親クラスから呼ばれます。
     */
    protected void ReaderOff(){
        // バーコードOFF
        Barcode_Read_Stop();
    }

    /**
     * 確認画面を表示する
     */
    protected void onReturnConfirm() {
        //確認画面を表示し、OKの場合true、キャンセルの場合false
        final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_RETURN);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));  //クリア
        dialog.message(SystemSetting.getString(
                CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_RETURN, "", this));               //登録されていない読取データは破棄されます。よろしいですか？
        dialog.params(bundle);
        dialog.show();
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
            case BUTTON_CLEAR:      //クリアボタン
                //クリア処理
                ClearRfidTagList();
                break;
            case BUTTON_RETURN:     //戻るボタン
            case ACTION_LOGOUT:      //ログアウトボタン
                //ログイン画面を開く
                Intent intent = new Intent(getApplication(), LoginMn.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                startActivity(intent);
                break;

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
                case BUTTON_SETTEI:
                    break;

                case BUTTON_SUMMARY:        //探索画面から戻る
                    //前画面からデータを取得する
                    _SearchData.Clear();
                    break;

                default:
                    //前画面からデータを取得する
                    _SearchData.Clear();
                    break;
            }

        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * クリア処理
     */
    protected void ClearRfidTagList() {

        //読込データのクリア
        _SearchData.Clear();

        // 読取りデータ件数表示
        UpdateGranteCountDisp();

        //件数表示を再表示する
        ResetCountDisp();

        //ボタン非活性
        SetButtonEnabled(false, false);

        //ReaderOn
        ReaderOn();
    }

    /**
     * 戻り先の画面
     */
    @Override
    protected void onReadReturn(){
        //メニュー画面へ
        Intent intent = new Intent(getApplication(), MainActivity.class);
        startActivity(intent);
    }

    /** ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * @param blnTagMode  タグモードの有効/無効
     * */
    protected void SetButtonEnabled(boolean prmEnabled, boolean blnTagMode) {

        //0件の場合非活性
        if (_SearchData.CountAll() <= 0) {
            prmEnabled = false;
        }

        //詳細表示ボタン
        ((Button)findViewById(R.id.btnDetail)).setEnabled(prmEnabled);

        //クリアボタン
        ((Button)findViewById(R.id.btnClear)).setEnabled(prmEnabled);

        final boolean blnEnabled = prmEnabled;
        runOnUiThread(new Runnable() {    //20200121 Add Thread Sasaki
            @Override
            public void run() {
                if (blnEnabled == false) {
                    ((Button) findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button_diable));
                    ((Button) findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#a9a9a9"));

                    //クリアボタン
                    Drawable leftDrawable = getResources().getDrawable(R.drawable.trash_32_disable);
                    leftDrawable.setBounds(0, 0, leftDrawable.getMinimumWidth(), leftDrawable.getMinimumHeight());
                    Button clear = findViewById(R.id.btnClear);
                    clear.setBackground(getDrawable(R.drawable.execute_button_clear_disable));
                    clear.setCompoundDrawables(leftDrawable, null, null, null);
                    clear.setTextColor(Color.parseColor("#a9a9a9"));
                } else {
                    ((Button) findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button));
                    ((Button) findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#ffffff"));

                    //クリアボタン
                    Drawable leftDrawable = getResources().getDrawable(R.drawable.trash_32);
                    leftDrawable.setBounds(0, 0, leftDrawable.getMinimumWidth(), leftDrawable.getMinimumHeight());
                    Button clear = findViewById(R.id.btnClear);
                    clear.setBackground(getDrawable(R.drawable.execute_button_clear));
                    clear.setCompoundDrawables(leftDrawable, null, null, null);
                    clear.setTextColor(Color.parseColor("#ffffff"));
                }
            }
        });

        //タグモードボタン(TAGモードの場合)
        if (CommonConst.READING_MEDIUM.TAG.getString().equals(_SettingTagMode)) {
            if (findViewById(R.id.switchTagMode) != null) {
                if (_TagReadStartFlag) {
                    ((Switch) findViewById(R.id.switchTagMode)).setEnabled(false);
                } else {
                    ((Switch) findViewById(R.id.switchTagMode)).setEnabled(true);
                }
            }
        }
    }

    /**
     * RFIDリーダー切断イベント
     */
    @Override
    public void onDisconnectEvent() {
        super.onDisconnectEvent();
        SetButtonEnabled(true, true);
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return C_Setting.KEY_EACH_SEARCH;
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {
    }

    protected void OpenMainActivity(){ }

    @Override
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) { }

}
