package com.rfid.Inventory;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.Spinner;
import android.widget.Switch;
import android.widget.TextView;

import com.rfid.Factory.InventoryFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseRead;
import com.rfid.common.BeepSound;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.RFIDPkgException;
import com.rfid.common.SystemSetting;
import com.rfid.main.MainActivity;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;


public class InventoryTagIn extends BaseRead {

    //システム設定値
    private SettingValue C_Setting = new SettingValue();

    // 棚卸し保持データ
    //public InventoryFactory _InventoryData = new InventoryFactory();      //20200211 Del Sasaki

    private final static int BUTTON_CLEAR = 1;
    private final static int BUTTON_RETURN = 2;
    private final static int BUTTON_REGISTER = 3;
    private final static int BUTTON_MENU = 4;
    private final String TEST_FOLDER = "test";

    private final Activity activity = this;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {

        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_inventory_tag_in);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            // プログレスダイアログの表示
            C_Progres.StartProgressDialog(this, "Loading...");

            //RFIDリーダーをOFFにする
            RFID_Read_Stop();

            //電波強度コンボボックスのデータ設定
            SetComboDataIntencity();

            //ボタン非活性
            SetButtonEnabled(false, false);

            //文言表示
            InitializeWord();

            //データ読み込み
            InsertData();

            //タグ読取モード Clickイベント
            ((Switch)findViewById(R.id.switchTagMode)).setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener(){
                @Override
                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_INV_TAG_MODE, "", activity), activity);

                    try {
                        //タグ読取モード変更処理
                        onSwitchChanged(isChecked);
                    } catch (Exception e) {
                        showErrorMessage(e.getMessage());
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                    }
                }
            });

            //クリアボタン Clickイベント
            findViewById(R.id.btnClear).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", activity), activity);

                    //確認メッセージを表示する
                    ConfirmMessage(BUTTON_CLEAR, CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_CLEAR);
                }
            });

            //タグ読取開始ボタン Clickイベント
            final Activity activity = this;
            findViewById(R.id.btnTagReadStart).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", activity), activity);

                    try {
                        //ボタン切替
                        if (_TagReadStartFlag) {
                            //タグ読取開始している場合、収集終了処理
                            InventoryEndProcess();
                        } else {
                            //タグ読取を開始していない場合、収集開始処理
                            InventoryStartProcess();
                        }
                    } catch(Exception ex) {
                        showErrorMessage(ex.getMessage());
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    }
                }
            });

            //登録ボタン Clickイベント
            findViewById(R.id.btnDetail).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", activity), activity);

                    //確認メッセージを表示する
                    ConfirmMessage(BUTTON_REGISTER, CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_REGISTER);
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
                    //電波強度変更処理
                    ChangeIntencity();
                }
            });

            //初期化済みフラグ
            _InitializedFlag = false;

            //サービススタート
            super.startService();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * レジュームイベント
     */
    @Override
    public void onResume() {
        try {
            super.onResume();

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //画面初期化
            InitializeActivity();

            //ボタン活性
            if (_blnReading) {
                SetButtonEnabled(false, true);
                //AllResetButton(false);
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

    /**
     * RFID受信イベント
     * @param strReadData
     * @param tempRSSI
     */
    protected void RFIDDataReceived(String strReadData, short tempRSSI, final String strPcHex) {
        try {
            //資産ID⇒資産番号に変換する
            //Log.d("資産ID", strReadData); 不要なログのためコメントアウト
            String AssetNo = _InventoryData.GetAssetNoFromID(strReadData);

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
        } catch (Exception ex){
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", this) + ":" + Common.toString(ex), this);
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }


    /**
     * 登録処理
     */
    private void InventoryRegister() throws RFIDPkgException {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            db.beginTransaction();

            //DBに登録する
            _InventoryData.Insert(db);

            //イベントログ
            LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                    GetWord(CommonConst.VIEW_WORD_INV_MSG_REGISTER)
                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + _InventoryData.InventoryCount()
                    , CommonConst.LOG_KIND.Inventory
                    , this);

            //実績収集件数をクリアする
            _InventoryData.Clear();

            //HT蓄積データを再取得する
            _InventoryData.CreateHTStoreData(db);

            //件数表示のリセット
            ResetCountDisp(db);

            //ボタンを非活性にする
            SetButtonEnabled(false, false);

            //commit
            db.setTransactionSuccessful();

            //メッセージBOX
            ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_INV_MSG_REGISTER, "", this)
            ,SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));
        } catch (Exception ex) {
            //エラーログ
            if (db != null) {
                db.endTransaction();
                db.close();
                db = null;
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", this) + ":" + Common.toString(ex), this);
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.endTransaction();
            }
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
                    _InventoryData = null;
                    _InventoryData = new InventoryFactory();
                    _InventoryData.CreateInventoryData(db,  "", "", "");

                    //HT蓄積データを取得し設定する
                    _InventoryData.CreateHTStoreData(db);

                    //棚卸継続方法を取得し設定する
                    _InventoryData.setInventoryContinue( SettingFactory.GetInventoryContinue(db));

                    runOnUiThread(new Runnable() { //20200121 Add Thread Sasaki
                        @Override
                        public void run() {
                            SQLiteDatabase db = null;

                            try {
                                //DB接続
                                DatabaseHelper dbHelper = new DatabaseHelper(context);
                                db = dbHelper.getWritableDatabase();

                                //件数表示を再表示する
                                ResetCountDisp(db);

                                //ボタンを活性にする
                                SetButtonEnabled(true, true);
                                AllResetButton(true);

                                //タグモードボタン(TAGモードの場合)
                                if (CommonConst.READING_MEDIUM.TAG.getString().equals(_SettingTagMode)) {
                                    ((Switch) findViewById(R.id.switchTagMode)).setEnabled(true);
                                }
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
                    });
                } catch (Exception ex) {
                    showErrorMessage(ex.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                } finally {
                    if (db != null) {
                        db.close();
                    }
                }
            }
        }.start();
    }

    /**
     *  バーコード取得イベント
     * @param strData
     */
    protected void BarcodeDataReceived(String strData) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        //LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this); 不要なログのためコメントアウト

        try {
            //データチェック
            CountRFidGaranteValue(strData);

            //ボタン制御
            SetButtonEnabled(true,true);
        } catch (Exception ex) {
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", this) + ":" + Common.toString(ex), this);
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }


    /**
     * 読取りデータカウント処理
     * @param AssetNo 読取りデータ
     */
    private void CountRFidGaranteValue(String AssetNo) {

        if (AssetNo != null) {
            Log.d("資産番号", AssetNo);
        }

        //棚卸データに存在する場合、蓄積する
        if (_InventoryData.ExistInventory(AssetNo)) {
            //連続棚卸モードの場合、既に読み込まれている場合は除外する
            if (CommonConst.INVENTORY_CONTINUE.YES.getString().equals(_InventoryData.getInventoryContinue())
                    && _InventoryData.isJittanaZumiNoLoc(AssetNo)) {
                return;
            }

            //棚卸データをためる
            _InventoryData.StockData(this, AssetNo);

            // 読取りデータ件数表示
            UpdateGranteCountDisp();
        }
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
        ResetCountDisp();

        //ボタン非活性
		//SetButtonEnabled(false, false); 20191220 Del Sasaki
        SetButtonEnabled(false, true); //20191220 Add Sasaki

    }

    //  読取りデータ件数表示
    protected void UpdateGranteCountDisp() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //件数表示
        TextView _LabReadCoun =  findViewById(R.id.labReadCountVal);
        _LabReadCoun.setText(Long.toString(_InventoryData.InventoryCount()));

        //予定数を超えた場合、ブザー
        if(_InventoryData.TotalCount() < _InventoryData.InventoryCount()) {
            //ブザーON
            buzzer_B2();
        }
    }

    /**
     * 件数表示のリセット
     */
    protected void ResetCountDisp(SQLiteDatabase db){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //実績収集件数のクリア
        ((TextView)findViewById(R.id.labReadCountVal)).setText("0");

        //HT蓄積件数の取得
        Long lngSoreCount = _InventoryData.HTStoreCount(db);

        //HT蓄積件数の更新
        ((TextView)findViewById(R.id.labInvStoreCount)).setText(lngSoreCount.toString());

//20191210 Add Start Sasaki
        //連続棚卸モードの場合
        if (CommonConst.INVENTORY_CONTINUE.YES.getString().equals(_InventoryData.getInventoryContinue())) {
            //棚卸済数の更新
            ((TextView) findViewById(R.id.labInvSumiCount)).setText(Long.toString(_InventoryData.InventorySumiCount(db)));
        } else {
            //棚卸済数の更新
            ((TextView) findViewById(R.id.labInvSumiCount)).setText("0");
        }
//20191210 Add End Sasaki

        //在庫件数の更新
        ((TextView)findViewById(R.id.labInvTargetCnt)).setText(Long.toString(_InventoryData.TotalCount()));
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_INV_TITLE,"",this));                     //実棚　タイトル
        ((TextView)findViewById(R.id.labInvBReadCountTtl))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_COUNT, "", this));     //実棚収集件数
        ((TextView)findViewById(R.id.labInvSumiValTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_SUMI_COUNT, "", this));     //棚卸済数
        ((TextView)findViewById(R.id.labInvStoreValTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_ACCUMULATION_COUNT, "", this));   //HT蓄積件数
        ((TextView)findViewById(R.id.labInvBTargetCntTtl))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_STOCK_COUNT, "", this));   //在庫件数
        ((TextView)findViewById(R.id.labIntencityTtl))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this));          //電波強度
        ((TextView)findViewById(R.id.lblTagModeTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_TAG_MODE, "", this));      //タグモード
        ((TextView)findViewById(R.id.labReadTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_COUNT, "", this));    //実棚収集件数
        ((TextView)findViewById(R.id.labReadTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_SUB_TITLE, "", this));  //読取開始してください
        ((TextView)findViewById(R.id.btnClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));       //クリア
        ((TextView)findViewById(R.id.btnDetail))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));    //一覧
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
     * 電波強度のコンボボックス設定
     */
    private void SetComboDataIntencity() {

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
         return comboItems;
    }

    /**
     * クリアボタン押下処理
     */
    @Override
    protected void onClearButton() {

        //if (_ProcessData.CountAll() > 0) { 20191220 Del Sasaki
        if (_InventoryData.InventoryCount() > 0) { //20191220 Add Sasaki
            //確認メッセージを表示する
            final CommonConfirmDialog.Builder dialog = new CommonConfirmDialog.Builder(this);
            Bundle bundle = new Bundle();
            bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_CLEAR);
            dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));  //クリア
            dialog.message(SystemSetting.getString(
                    CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_CLEAR, "", this));                //読取データをクリアします。よろしいですか？
            dialog.params(bundle);
            dialog.show();
        }
    }

    /** ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * @param blnTagMode  タグモードの有効/無効
     * */
    protected void SetButtonEnabled(boolean prmEnabled, boolean blnTagMode) {

        //0件の場合非活性
        if (_InventoryData.InventoryCount() <= 0) {
            prmEnabled = false;
        }

        //詳細表示ボタン
        ((Button)findViewById(R.id.btnDetail)).setEnabled(prmEnabled);

        //クリアボタン
        ((Button)findViewById(R.id.btnClear)).setEnabled(prmEnabled);

        //スイッチ
        ((Switch) findViewById(R.id.switchTagMode)).setEnabled(blnTagMode);

        super.AllResetButton(true);     //20200219 Add Sasaki

        final boolean blnEnabled = prmEnabled;
        runOnUiThread(new Runnable() { //20200121 Add Thread Sasaki
            @Override
            public void run() {
                if (blnEnabled == false) {
                    ((Button)findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button_diable));
                    ((Button)findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#a9a9a9"));

                    //クリアボタン
                    Drawable leftDrawable = getResources().getDrawable(R.drawable.trash_48_disable);
                    leftDrawable.setBounds(0, 0, leftDrawable.getMinimumWidth(), leftDrawable.getMinimumHeight());
                    Button clear = findViewById(R.id.btnClear);
                    clear.setBackground(getDrawable(R.drawable.execute_button_clear_disable));
                    clear.setCompoundDrawables(null, leftDrawable, null, null);
                    clear.setTextColor(Color.parseColor("#a9a9a9"));
                } else {
                    ((Button)findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button));
                    ((Button)findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#ffffff"));
                    //((Button)findViewById(R.id.btnClear)).setTextColor(Color.parseColor("#474747"));

                    //クリアボタン
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

    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        if (params == null) {
            return;
        }

        //呼出元で分岐する
        switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
            case BUTTON_REGISTER:      //登録ボタン
                try {
                    //登録処理
                    InventoryRegister();

                    //件数表示を再表示する
                    //ResetCountDisp(); 20191210 Del Sasaki

                    //ボタン無効
                    SetButtonEnabled(false,false);

                } catch (Exception ex) {
                    showErrorMessage(ex.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    //エラーログ
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
                }

            case BUTTON_CLEAR:      //クリアボタン
                //クリア処理
                ClearRfidTagList();
                break;
            case BUTTON_RETURN:     //戻るボタン
                onReadReturn();
                break;
            case BUTTON_MENU:       //メニューボタン
                onMenuReturn();
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
        //クリア処理
        _InventoryData.AllClear();      //20200214 Add Sasaki

        //メニュー画面へ
        Intent intent = new Intent(getApplication(), MainActivity.class);
        startActivity(intent);
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
        if (_InventoryData.InventoryCount() > 0) {
            onReturnConfirm(BUTTON_RETURN);
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
        if (_InventoryData.InventoryCount() > 0) {
            onReturnConfirm(BUTTON_MENU);
            return true;
        } else {
            onMenuReturn();
            return true;
        }
    }
}
