package com.rfid.Inventory;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import androidx.appcompat.app.AlertDialog;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.NumberPicker;
import android.widget.Spinner;
import android.widget.Switch;
import android.widget.TextView;

import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.InventoryFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseRead;
import com.rfid.common.BeepSound;
import com.rfid.common.Common;
import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.main.LoginMn;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.text.ParseException;

/**
 * 理論在庫比較あり 実棚収集画面
 *
 */
public class InventoryLocation extends BaseRead implements CommonConfirmDialog.Callback, CommonOKDialog.Callback{

    //システム設定値
    private SettingValue C_Setting = new SettingValue();

    //メニュー遷移定数
    private static final int Inventory_Sumary = 2;    //棚卸集計画面

    //戻り値用オブジェクト
    private Intent ResIntent = new Intent();

    //集計処理フラグ
    private boolean _SummaryProcesFlag = false;

    //初期化済みフラグ
    private boolean _InitializedFlag = false;

    //通信中フラグ(連続クリック防止)
    private Boolean _TransMesuarFlag = false;

    // 棚卸し保持データ
    //private static InventoryFactory _InventoryData = new InventoryFactory();  //20200211 Del Sasaki

    // ビルダー
    private AlertDialog.Builder builder;
    // 表示したダイアログ本体
    private AlertDialog dialog;
    // numberPicker
    private NumberPicker numberPicker;

    //ロケーションコード
    private String _LocLCode = "";
    private String _LocMCode = "";
    private String _LocSCode = "";

    //ロケーション名
    private String _LocLName = "";
    private String _LocMName = "";
    private String _LocSName = "";

    private TextView text = null;

    private final static int BUTTON_CLEAR = 3;
    private final static int BUTTON_RETURN = 2;

    private final static int BUTTON_LOGOUT = 1;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {

        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_inventory_location);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            // プログレスダイアログの表示
            C_Progres.StartProgressDialog(this, "Loading...");

            //RFIDリーダーをOFFにする
            RFID_Read_Stop();

            //コンボボックスのデータ設定
            SetComboDataIntencity();

            //文言表示
            InitializeWord();

            //画面設定
            InitialSetting();

            //ボタン非活性
            //SetButtonEnabled(false, false);   20200213 Del Sasaki
            AllResetButton(false);  //20200213 Add Sasaki

            //データ読み込み
            InsertData();

            //操作時間のタイマーをセット
            StartTimeoutTimer(CommonConst.KEY_READ_INVENTORY_LOC);

            //タグ読取モード Clickイベント
            final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
            final CommonOKDialog.Builder  errdialog = new CommonOKDialog.Builder(this, true);
            final Activity activity = this;
            //final BarcodeManager.BarcodeManagerListener listner = this;       //Android用
            ((Switch)findViewById(R.id.switchTagMode)).setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener(){
                @Override
                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                    if (checkTimeout()) {return;}
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_INV_TAG_MODE, "", activity), activity);

                    //タグ読取モード変更処理
                    onSwitchChanged(isChecked);
                }
            });

            //クリアボタン Clickイベント
            findViewById(R.id.btnClear).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", activity), activity);

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

            //タグ読取開始ボタン Clickイベント
            findViewById(R.id.btnTagReadStart).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", activity), activity);

                    if (checkTimeout()) {return;}
                    //ボタン切替
                    if (_TagReadStartFlag){
                        //タグ読取開始している場合、収集終了処理
                        InventoryEndProcess();
                    } else {
                        //タグ読取を開始していない場合、収集開始処理
                        InventoryStartProcess();
                    }

                }
            });

            //詳細表示ボタン Clickイベント
            findViewById(R.id.btnDetail).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LIST, "", activity), activity);

                    if (checkTimeout()) {return;}
                    //ボタン非活性
                    AllResetButton(false);
                    //SetButtonEnabled(false, false);       20191017 Del Sasaki

                    ReaderOff();
                    //詳細表示画面へ
                    Intent intent = new Intent(getApplication(), InventorySummary.class);
                    //intent.putExtra(CommonConst.VIEW_KEY_INVENTORY_DATA, _InventoryData);
                    //startActivity(intent); 20191210 Del Sasaki
                    startActivityForResult(intent, BUTTON_SUMMARY); //20191210 Add Sasaki

                }
            });

            //電波強度変更リスナー
            Spinner _spinner = findViewById(R.id.cmbIntencity);
            _spinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                //何も選択されなかった時の動作
                @Override
                public void onNothingSelected(AdapterView adapterView) {}
                @Override
                public void onItemSelected(AdapterView parent, View view, int position, long id) {
                    if (checkTimeout()) {return;}
                    //電波強度変更処理
                    ChangeIntencity();
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
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_TITLE,"",this));             //実棚　ロケ指定　タイトル
        ((TextView)findViewById(R.id.labInvZaikoTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_COUNT, "", this));   //ロケ在庫件数
        ((TextView)findViewById(R.id.labInvIntencity))
            .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this));           //電波強度
        ((TextView)findViewById(R.id.lblTagModeTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_TAG_MODE, "", this));    //タグモード
        ((TextView)findViewById(R.id.labInvBReadCountTtl))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_COUNT, "", this));  //実棚収集件数
        ((TextView)findViewById(R.id.labReadTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_SUB_TITLE, "", this));  //読取開始してください
        ((TextView)findViewById(R.id.btnClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));       //クリア
        ((TextView)findViewById(R.id.btnDetail))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LIST, "", this));    //一覧
        ((TextView)findViewById(R.id.btnTagReadStart))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", this));    //タグ読取開始
        Spinner intencity = (Spinner)findViewById(R.id.cmbIntencity);
        if(intencity.getSelectedItem().toString().length() > 2) {
            ViewGroup.LayoutParams param = intencity.getLayoutParams();
            param.width = 250;
            intencity.setLayoutParams(param);
        }
    }

    /**
     * 棚卸対象データを取得しデータを作成する
     */
    protected void InsertData() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        _blnReading = true;
        final Context context = this;
        new Thread() {
            @Override
            public void run() {
                //DB接続
                SQLiteDatabase db = null;

                try {
                    //DB接続
                    DatabaseHelper dbHelper = new DatabaseHelper(context);
                    db = dbHelper.getWritableDatabase();

                    //棚卸対象データを作成する
                    Intent intent = getIntent();
                    _InventoryData = null;
                    _InventoryData = new InventoryFactory();
                    _InventoryData.CreateInventoryData(db, intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_L_DATA),
                            intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_M_DATA),
                            intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_S_DATA));
                    //棚卸継続方法を取得し設定する
                    _InventoryData.setInventoryContinue(SettingFactory.GetInventoryContinue(db));
                    runOnUiThread(new Runnable() { //20200121 Add Thread Sasaki
                        @Override
                        public void run() {
                            //件数表示を再表示する
                            ResetCountDisp();
                            //ボタンを活性にする
                            SetButtonEnabled(true, true);
                            //棚卸検証用読取EPCデータファイル読み込み
                            testReadFileEPC();
                        }
                    });
                } catch (Exception ex) {
                    showErrorMessage(ex.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                } finally {
                    if (db != null) {
                        db.close();
                    }
                    C_Progres.StopProgressDialog();
                    _blnReading = false;
                }
            }
        }.start();
    }

    /**
     * 電波強度のコンボボックス設定
     */
    private void SetComboDataIntencity() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //コンボボックスの数値を設定する
        String[] spinnerItems = SetIntenciryComboData();

        // ArrayAdapter
        ArrayAdapter<String> adapter
                = new ArrayAdapter<>(this,
                android.R.layout.simple_spinner_item, spinnerItems);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);

        //電波強度コンボ(棚卸)
        Spinner spinner = findViewById(R.id.cmbIntencity);
        spinner.setAdapter(adapter);

        //初期値を設定する
        //String _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY); 20191220 Del Sasaki
        String _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_KBN);   //20191220 Add Sasaki
        Spinner SpinnerData = findViewById(R.id.cmbIntencity);
        //20191220 UPD Start Sasaki
        int intPosition = 0;
        if (C_Setting.INVENTORY_VALUE_L.equals(_Intencity)) {
            //強
            intPosition = CommonConst.INVENTORY_KBN.L.getInt();
        } else if (C_Setting.INVENTORY_VALUE_M.equals(_Intencity)) {
            //中
            intPosition =  CommonConst.INVENTORY_KBN.M.getInt();
        } else if (C_Setting.INVENTORY_VALUE_S.equals(_Intencity)) {
            //弱
            intPosition =  CommonConst.INVENTORY_KBN.S.getInt();
        }
        //20191220 UPD End Sasaki
        SpinnerData.setSelection(intPosition);
    }

    /**
     * 電波強度コンボボックス設定
     * @return
     */
    private String[] SetIntenciryComboData() {
        String[] comboItems = null;

        //20191125 Add Start Sasaki
        //強・中・弱
        comboItems = new String[3];
        comboItems[0] = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA_L, "", this);  //強
        comboItems[1] = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA_M, "", this);  //中
        comboItems[2] = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA_S, "", this);  //弱
        //20191125 Add End Sasaki

        //20191125 Del Start Sasaki
        /*if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
            //DENSOの場合
            comboItems = new String[27];
            for (int i = 0; i < 27; i++) {
                comboItems[i] = String.valueOf(i + 4);
            }
        } else {
            //ZEBRAの場合
            comboItems = new String[17];
            for (int i = 0; i < 17; i++) {
                comboItems[i] = String.valueOf((i + 12) * 10);
            }
        }*/
        //20191125 Del End Sasaki
        return comboItems;
    }

    private void InitialSetting() {

        //全画面のロケを取得し表示
        Intent intent = getIntent();
        _LocLCode = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_L_DATA);
        _LocMCode = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_M_DATA);
        _LocSCode = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_S_DATA);
        _LocLName = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_L_TEXT);
        _LocMName = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_M_TEXT);
        _LocSName = intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_S_TEXT);

        String strLocL = Common.substring(_LocLCode + " " + _LocLName, CommonConst.VIEW_LOCATION_NAME_SIZE);
        String strLocM = Common.substring(_LocMCode + " " + _LocMName, CommonConst.VIEW_LOCATION_NAME_SIZE);
        String strLocS = Common.substring(_LocSCode + " " + _LocSName, CommonConst.VIEW_LOCATION_NAME_SIZE);
        ((TextView)findViewById(R.id.txtInvLocL)).setText(strLocL);
        ((TextView)findViewById(R.id.txtInvLocM)).setText(strLocM);
        ((TextView)findViewById(R.id.txtInvLocS)).setText(strLocS);

        _InventoryData.setLocLKbnCd(_LocLCode);
        _InventoryData.setLocMKbnCd(_LocMCode);
        _InventoryData.setLocSKbnCd(_LocSCode);
        _InventoryData.setLocLKbnName(_LocLName);
        _InventoryData.setLocMKbnName(_LocMName);
        _InventoryData.setLocSKbnName(_LocSName);
    }

    /**
     * レジュームイベント
     */
    @Override
    public void onResume() {
        try {
            super.onResume();

            //画面初期化
            InitializeActivity();

            //ボタン活性
            if (_blnReading) {
                SetButtonEnabled(false, false);
                AllResetButton(false);
            } else {
                if (connectReader) {
                    SetButtonEnabled(true, true);
                } else {
                    SetButtonEnabled(true, true);
                    super.AllResetButton(false);
                }
            }

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /** ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * @param blnTagMode  タグモードの有効/無効
     * */
    protected void SetButtonEnabled(boolean prmEnabled, boolean blnTagMode) {
        if (!connectReader) {
            prmEnabled = false;
        }
        //20191016 Add Start Sasaki
        //読取開始の場合、一覧ボタン活性
        ((Button) findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button));
        ((Button) findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#ffffff"));
        ((Button) findViewById(R.id.btnDetail)).setEnabled(true);                           //20220316 ADD Suzuki V2.1
        if (_TagReadStartFlag) {
            //読取開始の場合、一覧ボタン非活性
            ((Button) findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button_diable));
            ((Button) findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#a9a9a9"));
            ((Button) findViewById(R.id.btnDetail)).setEnabled(false);                       //20220316 ADD Suzuki V2.1
        }
        //20191016 Add End Sasaki
        super.AllResetButton(true);   //20191119 Add Sasaki

        //スイッチ
        ((Switch) findViewById(R.id.switchTagMode)).setEnabled(blnTagMode);

        runOnUiThread(new Runnable() {	//20200121 Add Thread Sasaki
            @Override
            public void run() {
                //件数が0件の場合
                if (_InventoryData.InventoryCount() <= 0) {
                    //20191016 Del Start Sasaki
                    //((Button)findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button_diable));
                    //((Button)findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#a9a9a9"));
                    //20191016 Del End Sasaki

                    //クリアボタン
                    ((Button)findViewById(R.id.btnClear)).setEnabled(false);
                    Drawable leftDrawable = getResources().getDrawable(R.drawable.trash_48_disable);
                    leftDrawable.setBounds(0, 0, leftDrawable.getMinimumWidth(), leftDrawable.getMinimumHeight());
                    Button clear = findViewById(R.id.btnClear);
                    clear.setBackground(getDrawable(R.drawable.execute_button_clear_disable));
                    clear.setCompoundDrawables(null, leftDrawable, null, null);
                    clear.setTextColor(Color.parseColor("#a9a9a9"));
                //件数が1件以上の場合
                } else {
                    //20191016 Del Start Sasaki
                    //((Button)findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button));
                    //((Button)findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#ffffff"));
                    //20191016 Del Start Sasaki

                    //クリアボタン
                    ((Button)findViewById(R.id.btnClear)).setEnabled(true);
                    Drawable leftDrawable = getResources().getDrawable(R.drawable.trash_48);
                    leftDrawable.setBounds(0, 0, leftDrawable.getMinimumWidth(), leftDrawable.getMinimumHeight());
                    Button clear = findViewById(R.id.btnClear);
                    clear.setBackground(getDrawable(R.drawable.execute_button_clear));
                    clear.setCompoundDrawables(null, leftDrawable, null, null);
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
        } else {
            ((Switch) findViewById(R.id.switchTagMode)).setEnabled(false);      //20200123 Add Sasaki
        }
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(final boolean blnEnabled) {
        super.AllResetButton(blnEnabled);
        runOnUiThread(new Runnable() { //20200121 Add Thread Sasaki
            @Override
            public void run() {
                if (blnEnabled) {
                    ((Button)findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button));
                    ((Button)findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#ffffff"));
                    ((Button)findViewById(R.id.btnClear)).setBackground(getDrawable(R.drawable.execute_button_clear));
                    ((Button)findViewById(R.id.btnClear)).setTextColor(Color.parseColor("#ffffff"));
                } else {
                    ((Button)findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button_diable));
                    ((Button)findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#a9a9a9"));
                    ((Button)findViewById(R.id.btnClear)).setBackground(getDrawable(R.drawable.execute_button_clear_disable));
                    ((Button)findViewById(R.id.btnClear)).setTextColor(Color.parseColor("#a9a9a9"));
                }
            }
        });
    }

    /**
     * RFIDリーダー接続イベント
     */
    @Override
    public void OnScannerAppeared() {
        //親クラスの接続処理
        super.OnScannerAppeared();

        SetButtonEnabled(true, true);
    }

    /**
     * RFIDリーダー切断イベント
     */
    @Override
    public void onDisconnectEvent() {
        //親クラスの切断処理
        super.onDisconnectEvent();

        //タグ読取終了処理を呼び出す
        InventoryEndProcess();

        //ボタンを非活性
        super.AllResetButton(false);
    }

    //ポーズイベント
    @Override
    public void onPause() {
        try {
            super.onPause();

            //タイマー停止
            StopTimer();

            //ボタン設定の変更
            Button _button = findViewById(R.id.btnTagReadStart);
            _button.setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", this));

            //タグ読取フラグをOFF
            _TagReadStartFlag = false;

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * RFID読取りイベント
     * 親クラスから呼ばれます。
     * @param strData
     * @param tempRSSI
     */
    @Override
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) {
        try {
            //資産ID⇒資産番号に変換する
            //Log.d("資産ID", strData); 不要なログのためコメントアウト
            String AssetNo = _InventoryData.GetAssetNoFromID(strData);

            if (AssetNo == null) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, "AssetID⇒AssetNo Error");
                return;
            }

            //カウント音を出す
            if (beepSound == null) {
                beepSound = new BeepSound(this);
            }
            beepSound.BeepCountUp();

            //データチェック
            CountRFidGaranteValue(AssetNo);
        } catch (Exception ex) {
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", this) + ":" + Common.toString(ex), this);
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * Barcode/QR読取りイベント
     * 親クラスから呼ばれます。
     * @param strData
     */
    protected void BarcodeDataReceived(String strData) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        //LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this); 不要なログのためコメントアウト
        SQLiteDatabase db = null;
        try {

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(InventoryLocation.this);
            db = dbHelper.getWritableDatabase();
            String assetNo = _InventoryData.GetAssetNoByProcessedData(db, strData);
            if (assetNo.equals("")) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, " AssetNo is not found");
                return;
            }

            //データチェック
            CountRFidGaranteValue(assetNo);

            //ボタン活性
            SetButtonEnabled(true, true);
        } catch (Exception ex) {
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", this) + ":" + Common.toString(ex), this);
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 読取りデータカウント処理
     * @param AssetNo 読取りデータ
     */
    private void CountRFidGaranteValue(String AssetNo) throws ParseException {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        //LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this); 不要なログのためコメントアウト

        //詳細表示中除外
        if(_SummaryProcesFlag) {
            return;
        }

        if (AssetNo != null) {
            Log.d("資産番号", AssetNo);
        }

        //20210918 ADD Takase V2.0 START
        SQLiteDatabase db = null;
        try {
            //権限ロケチェック
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            if (!CommonAuthority.CheckAssetInventoryAuth(db, AssetFactory.Select(db, AssetNo), CommonAuthority.Auth.updHubList)) {
                return;
            }

            // 棚卸対象データに存在しない場合
            if (!_InventoryData.ExistInventory(AssetNo)) {
                //資産データに存在するかチェックする
                if (_InventoryData.ExistAsset(AssetNo)) {

                    //20191209 Add Start Sasaki
                    //連続棚卸モードで既に正常で棚卸済みの場合(正常⇒ロケ違い)、飛ばす
                    if (CommonConst.INVENTORY_CONTINUE.YES.getString().equals(_InventoryData.getInventoryContinue())
                            && _InventoryData.isJittanaZumi(AssetNo)) {
                        return;
                    }
                    //20191209 Add End Sasaki

                    //資産データありの場合、ロケ照合エラーデータを追加する
                    _InventoryData.StockDataLocErr(this, AssetNo, _LocLCode, _LocMCode, _LocSCode, db);
                    //棚卸データをためる
                    _InventoryData.StockData(this, AssetNo, _LocLCode, _LocMCode, _LocSCode, CommonConst.PHYINV_STATUS.Warning.getString(), db);
                    //在庫なしデータから削除する
                    _InventoryData.DeleteDataNone(AssetNo, db);
                    //正常データから削除する
                    _InventoryData.DeleteDataOK(AssetNo, db);
                } else {
                    //資産データなしの場合、処理終了
                    return;
                }
            } else {
                //20200217 Add Start Sasaki
                //連続棚卸モードで既に正常で棚卸済みの場合(正常⇒正常)、飛ばす
                if (CommonConst.INVENTORY_CONTINUE.YES.getString().equals(_InventoryData.getInventoryContinue())
                        && _InventoryData.isJittanaZumi(AssetNo)) {
                    return;
                }
                //20200217 Add End Sasaki
                //20200305 Add Start Sasaki V1.2
                //貸出中で未棚卸のデータの場合、貸出中から削除する。
                if (_InventoryData.isLeaseNoInv(AssetNo, db)) {
                    //貸出中(未棚卸)データから削除する
                    _InventoryData.DeleteDataLease(AssetNo, db);
                }
                //20200305 Add End Sasaki V1.2

                //棚卸OKデータをためる
                _InventoryData.StockDataOK(this, AssetNo, _LocLCode, _LocMCode, _LocSCode);
                //棚卸データをためる
                _InventoryData.StockData(this, AssetNo, _LocLCode, _LocMCode, _LocSCode, CommonConst.PHYINV_STATUS.Normal.getString(), db);
                //在庫なしデータから削除する
                _InventoryData.DeleteDataNone(AssetNo, db);
                //ロケ違いデータから削除する
                _InventoryData.DeleteDataLocErr(AssetNo, db);
            }

            // 読取りデータ件数表示
            UpdateGranteCountDisp();

        } catch (Exception ex) {
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
        //20210918 ADD Takase V2.0 END


    }

    //クリア処理
    protected void ClearRfidTagList() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //二度読みバッファクリア
        subClearDuplexBuffer();

        //読込データのクリア
        _InventoryData.Clear();

        // 読取りデータ件数表示
        UpdateGranteCountDisp();

        //件数表示を再表示する
        ResetCountDisp();

        //ボタンを非活性にする
        SetButtonEnabled(false, false);
    }

    //  読取りデータ件数表示
    protected void UpdateGranteCountDisp() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //件数表示
        TextView _LabReadCoun =  findViewById(R.id.labReadCountVal);
        _LabReadCoun.setText(Long.toString(_InventoryData.InventoryCount()));

        //予定数を超えた場合、ブザー
        //if(_InventoryData.TotalCount() < _InventoryData.InventoryCount()) {
        //    //ブザーON
        //    //buzzer_B2();
        //}
    }

    /**
     * 件数表示のリセット
     */
    protected void ResetCountDisp(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //実績収集件数のクリア
        ((TextView)findViewById(R.id.labReadCountVal)).setText("0");

        //在庫件数の取得
        Long lngSoreCount = _InventoryData.TotalCount();
        Long lngCount = _InventoryData.InventoryCountOKOriginal();        //20200305 Add Sasaki V1.2

        //在庫件数の設定
        //20200305 Add Start Sasaki V1.2
        if (CommonConst.INVENTORY_CONTINUE.YES.getString().equals(_InventoryData.getInventoryContinue())) {
            //連続棚卸の場合
            ((TextView) findViewById(R.id.labInvZaikoCount)).setText(lngCount.toString() + "/" + lngSoreCount.toString());    //20200305 Add Sasaki V1.2
        } else {
        //20200305 Add End Sasaki V1.2
            //連続棚卸でない場合
            ((TextView)findViewById(R.id.labInvZaikoCount)).setText(lngSoreCount.toString());
        }
    }

    /**
     * メニュー選択イベント
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            switch (item.getItemId()) {
                case android.R.id.home:     //戻るボタン
                    return returnButton();

                case R.id.action_setting:
                    //設定 →　システム設定画面開く
                    if (_TagReadStartFlag) {
                        //タグ読取開始中の場合、処理しない
                        ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_TAG_READ_END, "", this), "");
                        return false;
                    }
                    Intent intent = new Intent(getApplication(), SystemSett.class);
                    startActivity(intent);
                    return true;
                case R.id.action_menu:  //メニューボタン
                    return returnMenu();
            }
            return super.onOptionsItemSelected(item);
        } catch (Exception ex) {
            return false;
        }
    }

//20191210 Add Start Sasaki
    /**
     * サブ画面から戻るイベント処理をします。
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            switch(requestCode) {
                case BUTTON_SUMMARY:        //一覧画面から戻る
                    //バーコードの場合、読取開始
                    if (!_TagMode) {
                        ReaderOn();
                    }

                    //ボタンを活性
                    SetButtonEnabled(true, true);
                    AllResetButton(true);
                    break;

                default:
                    break;
            }

        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }
//20191210 Add End Sasaki

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

/************************************************************************/
/*  画面遷移
/************************************************************************/

    //キー入力イベント
    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            switch (keyCode)  {
                case KeyEvent.KEYCODE_BACK:
                    //Back ボタン
                    return returnButton();
            }

            return super.onKeyDown(keyCode, event);
        }
        catch (Exception ex) {
            return false;
        }
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
                onReadReturn();
                break;
            case BUTTON_LOGOUT:       //ログアウトボタン
                //ログイン画面を開く
                Intent intent = new Intent(getApplication(), LoginMn.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                startActivity(intent);
                break;
        }
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return C_Setting.KEY_EACH_INVENTORY;
    }

    /**
     * 戻り先の画面
     */
    @Override
    protected void onReadReturn(){
        //ロケ選択画面へ
        Intent intent = new Intent(getApplication(), InventoryLocationSetting.class);
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_L_DATA, _LocLCode); //大区分の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_M_DATA, _LocMCode); //中区分の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_S_DATA, _LocSCode); //小区分の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_L_TEXT, _LocLName); //大区分名称の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_M_TEXT, _LocMName); //中区分名称の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_S_TEXT, _LocSName); //小区分名称の値を渡す
        setResult(RESULT_OK, intent);

        //クリア
        _InventoryData.AllClear();       //20200214 Add Sasaki

        finish();

    }

    /**
     * メニュー画面に遷移する
     */
    public void onMenuReturn() {
        //クリア処理
        _InventoryData.AllClear();      //20200214 Add Sasaki
        super.onMenuReturn();
    }

    /**
     * 戻るボタン処理
     * @return
     */
    private boolean returnButton() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        if (checkTimeout()) {return true;}
        if (_InventoryData.InventoryCount() > 0) {
            onReturnConfirm();
            return true;
        } else {
            onReadReturn();
            return true;
        }
    }

    /**
     * メニューへ戻る処理
     * @return
     */
    private boolean returnMenu() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        if (_InventoryData.InventoryCount() > 0) {
            onReturnConfirm(BUTTON_MENU);
            return true;
        } else {
            onMenuReturn();
            return true;
        }
    }

    /**
     * 棚卸検証用読取EPCデータファイルを読み込む
     */
    private  void testReadFileEPC() {
        String folderPath = String.valueOf(getExternalFilesDir(CommonConst.FOLDER_TEST));
        File rootFile = new File(folderPath);

        if (rootFile.exists()) {
            String fileName = folderPath + "/" + "INV_TEST.txt";
            File file = new File(fileName);

            try (BufferedReader br = new BufferedReader(new FileReader(file)))
            {
                String line;
                while ((line = br.readLine()) != null) {
                    //読み込んだEPCで順番に読取イベントを発生させる
                    RFIDDataReceived(line, (short) 0, "");
                }
            } catch (IOException e) {
                e.printStackTrace();
            }

        }
    }

/************************************************************************/
/*  定義のみ
/************************************************************************/
    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {}
    protected void OpenMainActivity(){ }
}
