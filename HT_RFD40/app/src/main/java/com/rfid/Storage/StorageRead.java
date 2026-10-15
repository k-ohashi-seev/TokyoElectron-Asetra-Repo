package com.rfid.Storage;

import android.app.Activity;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.CompoundButton;
import android.widget.Switch;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.common.BaseRead;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;

/**
 * 保管資産読取画面
 *
 */
public class StorageRead extends BaseRead implements CommonConfirmDialog.Callback, CommonOKDialog.Callback{

    //システム設定値
    private SettingValue C_Setting = new SettingValue();


    //ロケーションコード
    private String _LocLCode = "";
    private String _LocMCode = "";
    private String _LocSCode = "";

    //ロケーション名称
    private String _LocLName = "";
    private String _LocMName = "";
    private String _LocSName = "";

    private TextView text = null;

    private final static int BUTTON_CLEAR = 1;
    private final static int BUTTON_RETURN = 2;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {

        //DB接続
        SQLiteDatabase db = null;

        try {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.activity_storage_read);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            // プログレスダイアログの表示
            C_Progres.StartProgressDialog(this, "Loading...");

            //RFIDリーダーをOFFにする
            RFID_Read_Stop();

            //文言表示
            InitializeWord();

            //初期表示
            InitializeViewData();

            //ボタン非活性
            //SetButtonEnabled(false, false);   20200213 Del Sasaki
            AllResetButton(false);  //20200213 Add Sasaki

            //処理履歴登録用データ設定
            SetProcessData();

            //データ読み込み
            InsertData();

            //タグ読取モード Clickイベント
            final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
            final Activity activity = this;
            ((Switch)findViewById(R.id.switchTagMode)).setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener(){
                @Override
                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                    //タグ読取モード変更処理
                    onSwitchChanged(isChecked);
                }
            });

            //クリアボタン Clickイベント
            findViewById(R.id.btnClear).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
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
                    //ボタン非活性
                    AllResetButton(false);
                    SetButtonEnabled(false, false);

                    ReaderOff();
                    //詳細表示画面へ
                    Intent intent = new Intent(getApplication(), StorageSummary.class);
                    //intent.putExtra(CommonConst.VIEW_KEY_PROCESS_DATA, _ProcessData);
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
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_STORE,"",this));                 //保管タイトル
        ((TextView)findViewById(R.id.labStorageBReadCountTtl))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_COUNT, "", this));        //件数
        ((TextView)findViewById(R.id.labStorageIntencity))
            .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this));           //電波強度
        ((TextView)findViewById(R.id.lblTagModeTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_TAG_MODE, "", this));    //タグモード
        ((TextView)findViewById(R.id.labStorageReadTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this));    //タグ読取
        ((TextView)findViewById(R.id.btnClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));       //クリア
        ((TextView)findViewById(R.id.btnDetail))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LIST, "", this));        //一覧
        ((TextView)findViewById(R.id.btnTagReadStart))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", this));    //タグ読取開始
    }

    /**
     * 初期表示
     */
    private void InitializeViewData() {
        //電波強度を表示
        ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_STORAGE));

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
        ((TextView)findViewById(R.id.txtStorageLocL)).setText(strLocL);
        ((TextView)findViewById(R.id.txtStorageLocM)).setText(strLocM);
        ((TextView)findViewById(R.id.txtStorageLocS)).setText(strLocS);
    }

    /**
     * 処理履歴データを設定する
     */
    private void SetProcessData() {

        _ProcessData.setLocLKbnCd(_LocLCode);       //ロケ大区分
        _ProcessData.setLocLKbnName(_LocLName);     //ロケ大区分名称
        _ProcessData.setLocMKbnCd(_LocMCode);       //ロケ中区分
        _ProcessData.setLocMKbnName(_LocMName);     //ロケ中区分名称
        _ProcessData.setLocSKbnCd(_LocSCode);       //ロケ小区分
        _ProcessData.setLocSKbnName(_LocSName);     //ロケ小区分名称

    }

    /**
     * 戻り先の画面
     */
    @Override
    public void onReadReturn(){
        //クリア
        _ProcessData.AllClear();    //20200217 Add Sasaki

        //ロケ選択画面へ
        Intent intent = new Intent(getApplication(), StorageLocationSetting.class);
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_L_DATA, _LocLCode); //大区分の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_M_DATA, _LocMCode); //中区分の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_S_DATA, _LocSCode); //小区分の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_L_TEXT, _LocLName); //大区分名称の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_M_TEXT, _LocMName); //中区分名称の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_S_TEXT, _LocSName); //小区分名称の値を渡す
        setResult(RESULT_OK, intent);

        finish();
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return C_Setting.KEY_EACH_STORAGE;
    }

}
