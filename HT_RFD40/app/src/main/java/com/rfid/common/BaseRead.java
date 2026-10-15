package com.rfid.common;

import android.content.Context;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MenuItem;
import android.widget.Button;
import android.widget.Spinner;
import android.widget.Switch;
import android.widget.TextView;

import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.ProcessFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;

import java.util.Objects;

/**
 * 資産読取スーパークラス
 *
 */
public abstract class BaseRead extends BaseRFIDActivity implements CommonConfirmDialog.Callback, CommonOKDialog.Callback{

    //プログレスダイアログ クラス
    protected CommonProgres C_Progres = new CommonProgres();

    //システム設定値
    private SettingValue C_Setting = new SettingValue();

    //戻り値用オブジェクト
    protected Intent ResIntent = new Intent();

    //初期化済みフラグ
    protected boolean _InitializedFlag = false;

    //通信中フラグ(連続クリック防止)
    protected Boolean _TransMesuarFlag = false;

    // 保持データ
    //protected ProcessFactory _ProcessData = new ProcessFactory();     //20200211 Del Sasaki

    //タグ開始フラグ
    protected boolean _TagReadStartFlag = false;

    //タグモードフラグ
    protected boolean _TagMode = true;

    //設定マスタのタグモード
    protected String _SettingTagMode = "";

    protected boolean _blnReading = false;

    protected final static int BUTTON_CLEAR = 1;
    protected final static int BUTTON_RETURN = 2;
    protected final static int BUTTON_MENU = 3;
    protected final static int BUTTON_SUMMARY = 0;
    protected final static int BUTTON_SETTEI = 9;




    // レジュームイベント
    @Override
    public void onResume() {

        try {
            super.onResume();

            //画面初期化
            InitializeActivity();

            //読取設定
            ReaderOn();

            //読取モードの設定
            SettingReadMode();

            //タイマー開始
            StartTimer();

            //ボタン活性
            if (_blnReading) {
                SetButtonEnabled(false, false);
                AllResetButton(false);
            } else {
                if (connectReader) {
                    SetButtonEnabled(true, true);
                } else {
                    SetButtonEnabled(true, true);
                    AllResetButton(false);
                }
            }

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
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

            //ボタン設定の変更
            Button _button = findViewById(R.id.btnTagReadStart);
            if (_button != null) {
                _button.setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", this));
            }

            //タグ読取フラグをOFF
            resetTagStart();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * RFIDリーダー接続イベント
     */
    @Override
    public void OnScannerAppeared() {
        //親クラスの接続処理
        super.OnScannerAppeared();

        //タグモードの場合活性
        if(_TagMode) {
            AllResetButton(true);
        }
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
        AllResetButton(false);
    }

    /**
     * RFID読取りイベント
     * 親クラスから呼ばれます。
     * @param strData
     * @param tempRSSI
     */
    @Override
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) {
        SQLiteDatabase db = null;
        try {

            //既に読込済みの場合終了
            if (_ProcessData.ExistAssetID(strData)) {
                return;
            }

            //20230221 ADD Sato v2.3 START  DBインスタンスの生成タイミングを変更
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //資産番号を取得
            String strAssetNo = AssetFactory.Convert(db, strData);

            //資産マスタに存在する資産の場合、QR読取済みかどうかをチェック
            if (_ProcessData.ExistMasterAssetID(strData)) {
                //既に読込済みの場合終了
                if (_ProcessData.ExistAssetNo(strAssetNo)) {
                    return;
                }
            }
            //20230221 ADD Sato v2.3 END

            //資産ID情報を溜め込む
            _ProcessData.StockAssetID(strData);
            Log.d("資産ID", strData);

            //20210918 ADD Takase V2.0 START
            //20230221 DEL Sato v2.3 START
//            DatabaseHelper dbHelper = new DatabaseHelper(this);
//            db = dbHelper.getWritableDatabase();
//            String strAssetNo = AssetFactory.Convert(db, strData);
            //20230221 DEL Sato v2.3 END

            //参照権限チェック
            //参照権限が無い資産の場合
            if(!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, strAssetNo), CommonAuthority.Auth.readHubList)) {
                //読み捨てる（一覧表示は行う）
                return;
            }
            //20210918 ADD Takase V2.0 END

//20191126 Add Start Sasaki
            //マスタに存在する場合、資産IDを溜め込む
            if (_ProcessData.ExistMasterAssetID(strData)) {
                //20210918 ADD Takase V2.0 START
                //更新権限チェック
                AssetModel asset = AssetFactory.Select(db, strAssetNo);
                // (貸出の時貸出状態チェック）　&& 更新権限チェック
                if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_LEASE.getString())
                        || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                            && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {
                    //カウント音を出す
                    if (beepSound == null) {
                        beepSound = new BeepSound(this);
                    }
                    beepSound.BeepCountUp();
                    //資産IDを溜め込む
                    _ProcessData.StockViewAssetData(strData);
                }
                //20210918 ADD Takase V2.0 END
            }
//20191126 Add End Sasaki

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
    }

    /**
     * Barcode/QR読取りイベント
     * 親クラスから呼ばれます。
     * @param strData
     */
    protected void BarcodeDataReceived(String strData) {
        SQLiteDatabase db = null;
        try {
            //既に読込済みの場合終了
            if (_ProcessData.ExistAssetNo(strData)) {
                return;
            }

            //20230221 ADD Sato v2.3 START  DBインスタンスの生成タイミングを変更
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //資産マスタに存在する資産の場合、タグ読取済みかどうかをチェック
            if (_ProcessData.ExistMasterAssetNo(strData)) {
                AssetModel asset = AssetFactory.Select(db, strData);

                //既に読込済みの場合終了
                if (_ProcessData.ExistAssetID(asset.getAssetID())) {
                    return;
                }
            }
            //20230221 ADD Sato v2.3 END

            //資産番号情報を溜め込む
            _ProcessData.StockAssetNo(strData);
            Log.d("資産番号", strData);

            //20210918 ADD Takase V2.0 START
            //20230221 DEL Sato v2.3 START
//            DatabaseHelper dbHelper = new DatabaseHelper(this);
//            db = dbHelper.getWritableDatabase();
            //20230221 DEL Sato v2.3 END

            //参照権限チェック
            //参照権限の無い資産の場合
            if(!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, strData), CommonAuthority.Auth.readHubList)) {
                //読み捨てる（一覧表示は行う）
                return;
            }
            //20210918 ADD Takase V2.0 END

//20191126 Add Start Sasaki
            //マスタに存在する場合、資産IDを溜め込む
            if (_ProcessData.ExistMasterAssetNo(strData)) {
                //20210918 ADD Takase V2.0 START
                AssetModel asset = AssetFactory.Select(db, strData);
                // (貸出の時貸出状態チェック）　&& 更新権限チェック
                if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_LEASE.getString())
                        || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                            && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {
                    //資産IDを溜め込む
                    _ProcessData.StockViewAssetData(strData);
                }
                //20210918 ADD Takase V2.0 END
            }
//20191126 Add End Sasaki

            // 読取りデータ件数表示
            UpdateGranteCountDisp();

            //ボタン活性
            SetButtonEnabled(true, true);

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
    }

    /**
     * 収集開始処理
     *
     */
    protected void InventoryStartProcess() {

        //通信開始済み(連続クリック防止)
        if(_TransMesuarFlag) {
            return;
        }

        //タイマー停止
        StopTimer();

        _TagReadStartFlag = true;
        //リーダーOFF
        ReaderOff();

        //リーダーON
        ReaderOn();

        //通信開始済み(連続クリック防止)
        _TransMesuarFlag = true;

        //ボタン無効
        SetButtonEnabled(false, false);

        //ボタン設定の変更
        Button _button = findViewById(R.id.btnTagReadStart);
        _button.setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_END, "", this));    //タグ読取終了
    }

    /**
     * 収集終了処理
     */
    protected void InventoryEndProcess() {

        //タイマー開始
        StartTimer();

        //リーダーOFF
        ReaderOff();

        _TagReadStartFlag = false;

        //通信開始OFF(連続クリック防止)
        _TransMesuarFlag = false;

        //ボタン有効
        SetButtonEnabled(true, true);

        //ボタン設定の変更
        Button _button = findViewById(R.id.btnTagReadStart);
        if (_button != null) {
            _button.setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", this));    //タグ読取開始
        }

    }

    //画面初期化
    protected void InitializeActivity() {
        // 20230705 ADD Sato 処理タイミング制御のためsynchronizedを追加
        synchronized (_LockObj) {
            // 読取りデータ件数表示
            UpdateGranteCountDisp();

            //設定値取得
            C_Setting.getBTReadSettingGet(this, GetSettingKey());
        }
    }

    /**
     * リーダーをONにします。
     * 親クラスから呼ばれます。
     *
     */
    protected void ReaderOn(){

        String strKey = GetSettingKey();

        //読取開始の場合
        if (_TagMode) {
            //Barcode OFF
            Barcode_Read_Stop();

            if(_TagReadStartFlag) {
                //RFID リーダーON
                RFID_Read_Start(!_InitializedFlag, strKey);
            }
        } else {
            //Barcode ON
            Barcode_Read_Start();

            //RFID リーダーOFF
            RFID_Read_Stop();
        }
    }

    /**
     * リーダーをOFFにします。
     * 親クラスから呼ばれます。
     */
    protected void ReaderOff(){

        if (_TagMode) {
            if(_TagReadStartFlag) {
                //RFID リーダーOFF
                RFID_Read_Stop();
            }
        } else {
            // バーコードOFF
            Barcode_Read_Stop();
        }
    }

    /**
     * クリアボタン押下処理
     */
    protected void onClearButton() {

        if (_ProcessData.CountAll() > 0) {
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

    /**
     * QR/Barcode⇔Readerの切替スイッチ処理
     * @param isChecked
     */
    protected void onSwitchChanged(boolean isChecked) {

        try {
            //通信開始済み(連続クリック防止)
            findViewById(R.id.switchTagMode).setEnabled(false);

            if (isChecked == true) {
                // タグモードに設定
                _TagMode = true;

                //QRをOFFにする
                Barcode_Read_Stop();

                if (connectReader) {
                    //接続されている場合、タグ読取開始ボタンを活性
                    ((Button) findViewById(R.id.btnTagReadStart)).setEnabled(true);
                    ((Button) findViewById(R.id.btnTagReadStart)).setBackground(getDrawable(R.drawable.execute_button));
                    ((Button) findViewById(R.id.btnTagReadStart)).setTextColor(Color.parseColor("#ffffff"));
                }

                //電波強度を活性にする
                Spinner intencity = findViewById(R.id.cmbIntencity);
                if (intencity != null) {
                    intencity.setEnabled(true);
                }

            } else {
                // QRモードに設定
                _TagMode = false;

                //RFIDリーダーを切断する
                RFID_Read_Stop();

                //QRをONにする
                Barcode_Read_Start();

                //タグ読取開始ボタンを非活性
                ((Button) findViewById(R.id.btnTagReadStart)).setEnabled(false);
                ((Button)findViewById(R.id.btnTagReadStart)).setBackground(getDrawable(R.drawable.execute_button_diable));
                ((Button)findViewById(R.id.btnTagReadStart)).setTextColor(Color.parseColor("#a9a9a9"));

                //電波強度を非活性にする
                Spinner intencity = findViewById(R.id.cmbIntencity);
                if (intencity != null) {
                    intencity.setEnabled(false);
                }
            }

            //通信開始OFF(連続クリック防止)
            findViewById(R.id.switchTagMode).setEnabled(true);

        } catch (Exception e) {
            _TagReadStartFlag = false;      //タグ読込開始はOFF
            //ボタン設定の変更
            Button _button = findViewById(R.id.btnTagReadStart);
            _button.setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", this));    //タグ読取終了
            showErrorMessage(e.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    /**
     * クリア処理
     */
    protected void ClearRfidTagList() {

        //二度読みバッファクリア
        subClearDuplexBuffer();

        //読込データのクリア
        _ProcessData.Clear();

        // 読取りデータ件数表示
        UpdateGranteCountDisp();

        //ボタン非活性
        SetButtonEnabled(false, false);
    }

    //  読取りデータ件数表示
    protected void UpdateGranteCountDisp() {
        //件数表示(マスタにデータがあるタグの件数)
        TextView _LabReadCoun =  findViewById(R.id.labReadCountVal);
        //_LabReadCoun.setText(Integer.toString(_ProcessData.CountAll()));          //20191126 Del Sasaki
        _LabReadCoun.setText(Integer.toString(_ProcessData.CountViewAll()));      //20191126 Add Sasaki
    }

    /**
     * 件数表示のリセット
     */
    protected void ResetCountDisp(){

        //読取件数のクリア
        ((TextView)findViewById(R.id.labReadCountVal)).setText("0");

    }

    /** ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * @param blnTagMode  タグモードの有効/無効
     * */
    protected void SetButtonEnabled(boolean prmEnabled, boolean blnTagMode) {

        //0件の場合非活性
        if (_ProcessData.CountAll() <= 0) {
            prmEnabled = false;
        }

        if (findViewById(R.id.btnDetail) != null) {
            //詳細表示ボタン
            ((Button) findViewById(R.id.btnDetail)).setEnabled(prmEnabled);
        }

        if (findViewById(R.id.btnClear) != null) {
            //クリアボタン
            ((Button) findViewById(R.id.btnClear)).setEnabled(prmEnabled);
        }

        if (findViewById(R.id.switchTagMode) != null) {
            //スイッチ
            ((Switch) findViewById(R.id.switchTagMode)).setEnabled(blnTagMode);
        }

        final boolean blnEnabled = prmEnabled;
        runOnUiThread(new Runnable() {	//20200121 Add Thread Sasaki
              @Override
              public void run() {
                  if (blnEnabled == false) {
                      //詳細表示ボタン
                      ((Button) findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button_diable));
                      ((Button) findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#a9a9a9"));

                      //クリアボタン
                      Drawable leftDrawable = getResources().getDrawable(R.drawable.trash_48_disable);
                      leftDrawable.setBounds(0, 0, leftDrawable.getMinimumWidth(), leftDrawable.getMinimumHeight());
                      Button clear = findViewById(R.id.btnClear);
                      clear.setBackground(getDrawable(R.drawable.execute_button_clear_disable));
                      clear.setCompoundDrawables(null, leftDrawable, null, null);
                      clear.setTextColor(Color.parseColor("#a9a9a9"));

                  } else {
                      //詳細表示ボタン
                      ((Button) findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button));
                      ((Button) findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#ffffff"));

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
            if (findViewById(R.id.switchTagMode) != null) {
                ((Switch) findViewById(R.id.switchTagMode)).setEnabled(false);
            }
        }
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {

        boolean isRFIDReaderUse = Objects.equals(C_Setting.getRfidReaderUse(this), "1");
        if (findViewById(R.id.btnTagReadStart) == null) {
            return;
        }

        //未接続の場合、False
        if (!connectReader || !isRFIDReaderUse) {
            blnEnabled = false;
        }

        final boolean blnFEnabled = blnEnabled;
        runOnUiThread(new Runnable() { //20200121 Add Thread Sasaki
            @Override
            public void run() {
                if (blnFEnabled && _TagMode) {
                    //読取開始ボタンを活性
                    findViewById(R.id.btnTagReadStart).setEnabled(true);
                    ((Button) findViewById(R.id.btnTagReadStart)).setBackground(getDrawable(R.drawable.execute_button));
                    ((Button) findViewById(R.id.btnTagReadStart)).setTextColor(Color.parseColor("#ffffff"));
                } else {
                    //読取開始ボタンを非活性
                    findViewById(R.id.btnTagReadStart).setEnabled(false);
                    ((Button) findViewById(R.id.btnTagReadStart)).setBackground(getDrawable(R.drawable.execute_button_diable));
                    ((Button) findViewById(R.id.btnTagReadStart)).setTextColor(Color.parseColor("#a9a9a9"));
                }
            }
        });

    }

    /**
     * メニュー選択イベント
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
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
                    ReaderOff();
                    Intent intent = new Intent(getApplication(), SystemSett.class);
                    startActivityForResult(intent, BUTTON_SETTEI);
                    return true;

                case R.id.action_connect:
                    //接続
                    if (!connectReader) {
                        //開始
                        onResume();
                    }
                    return true;

                case R.id.action_menu:
                    return returnMenu();
            }
            return super.onOptionsItemSelected(item);
        } catch (Exception ex) {
            return false;
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
                case BUTTON_SUMMARY:        //一覧画面から戻る
                    if(resultCode == RESULT_OK) {
                        //前画面からデータを取得する
                        //_ProcessData = (ProcessFactory)data.getSerializableExtra(CommonConst.VIEW_KEY_PROCESS_DATA);

                        if (_ProcessData.CountAll() <= 0) {
                            //ボタンを制御する
                            SetButtonEnabled(false, false);
                        }

                        //バーコードの場合、読取開始
                        if (!_TagMode) {
                            ReaderOn();
                        }
                    }

                    //ボタンを活性
                    SetButtonEnabled(true, true);
                    AllResetButton(true);
                    break;

                case BUTTON_SETTEI:
                    //電波強度再表示
                    ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, GetSettingKey()));
                    break;

                default:
                    break;
            }

        } catch (Exception ex) {
            //ShowErrorMessage(ex.getMessage(), "");
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
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
                    return returnButton();

                    //20230221 ADD Sato v2.3 iDataソースマージ対応 START
                    //※既存動作を保ったままマージするため、BaseReadのキーイベントで処理する
                case KeyEvent.KEYCODE_F4:
                case KeyEvent.KEYCODE_F8:
                    //iDataのみ、トリガー押下でタグ読取を開始する
                    if (CommonConst.CONNECT_RFIDNAME.IDATA.getString().equals(Common.ConnectRFIDName)) {
                        if (super.onKeyDown(keyCode, event)) {
                            //タグ読取時のみ
                            InventoryStartProcess();
                        }
                        return false;
                    }
                    //20230221 ADD Sato v2.3 END
            }

            return super.onKeyDown(keyCode, event);
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * キー入力解除イベント  20230221 ADD Sato v2.3
     * @param keyCode
     * @param event
     * @return
     */
    @Override
    public boolean onKeyUp(int keyCode, KeyEvent event) {
        try {
            switch (keyCode)  {
                //20230221 ADD Sato v2.3 iDataソースマージ対応 START
                //※既存動作を保ったままマージするため、BaseReadのキーイベントで処理する
                case KeyEvent.KEYCODE_F4:
                case KeyEvent.KEYCODE_F8:
                    //iDataのみ、トリガーリリースでタグ読取を終了する
                    if (CommonConst.CONNECT_RFIDNAME.IDATA.getString().equals(Common.ConnectRFIDName)) {
                        if (super.onKeyUp(keyCode, event)) {
                            //タグ読取時のみ
                            InventoryEndProcess();
                        }
                        return false;
                    }
                    //20230221 ADD Sato v2.3 END

            }

            return super.onKeyUp(keyCode, event);
        }
        catch (Exception ex) {
            return false;
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
                setResult(RESULT_CANCELED, ResIntent);      //キャンセルを返す
                onReadReturn();
                break;
            case BUTTON_MENU:       //メニューボタン
                ReaderOff();
                onMenuReturn();
                break;
        }

    }

    /**
     * 読取モードを取得し、ボタンを制御する
     */
    protected void SettingReadMode(){
        SQLiteDatabase db = null;
        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            _SettingTagMode =SettingFactory.GetReadingMedium(db, this);

            //QRの場合
            if (CommonConst.READING_MEDIUM.QR.getString().equals(_SettingTagMode)) {

                if (findViewById(R.id.switchTagMode) != null) {
                    //QRモード
                    ((Switch) findViewById(R.id.switchTagMode)).setChecked(false);

                    //タグ切替を非活性
                    findViewById(R.id.switchTagMode).setEnabled(false);
                    ((Switch) findViewById(R.id.switchTagMode)).setTextColor(Color.parseColor("#a9a9a9"));
                }

                if (findViewById(R.id.btnTagReadStart) != null) {
                    //タグ読取開始を非活性
                    findViewById(R.id.btnTagReadStart).setEnabled(false);
                    findViewById(R.id.btnTagReadStart).setBackground(getDrawable(R.drawable.execute_button_diable));
                    ((Button) findViewById(R.id.btnTagReadStart)).setTextColor(Color.parseColor("#a9a9a9"));
                }
                _TagMode = false;       //20200123 Add Sasaki
            } else {

                if (findViewById(R.id.switchTagMode) != null) {
                    //タグ切替を非活性
                    findViewById(R.id.switchTagMode).setEnabled(true);
                    ((Switch) findViewById(R.id.switchTagMode)).setTextColor(Color.parseColor("#ffffff"));
                }

                if (findViewById(R.id.btnTagReadStart) != null) {
                    if (_TagMode) {
                        //タグ読取開始を活性
                        findViewById(R.id.btnTagReadStart).setEnabled(true);
                        findViewById(R.id.btnTagReadStart).setBackground(getDrawable(R.drawable.execute_button));
                        ((Button) findViewById(R.id.btnTagReadStart)).setTextColor(Color.parseColor("#ffffff"));
                    } else {
                        //タグ読取開始を非活性
                        findViewById(R.id.btnTagReadStart).setEnabled(false);
                        findViewById(R.id.btnTagReadStart).setBackground(getDrawable(R.drawable.execute_button_diable));
                        ((Button) findViewById(R.id.btnTagReadStart)).setTextColor(Color.parseColor("#a9a9a9"));
                    }
                }
            }


        } finally {
            if (db !=null) {
                db.close();
            }
        }
    }

    /**
     * 対象データを取得しデータを作成する
     */
    protected void InsertData() {
        _blnReading = true;
        final Context context = this;
        new Thread() {
            @Override
            public void run() {
                // 20230705 ADD Sato 処理タイミングを制御するためsynchronizedを追加
                synchronized (_LockObj) {
                    //DB接続
                    SQLiteDatabase db = null;

                    try {
                        //DB接続
                        DatabaseHelper dbHelper = new DatabaseHelper(context);
                        db = dbHelper.getWritableDatabase();

                        //対象データを作成する
//                    _ProcessData = null;         //202300705 DEL Sato タイミングによっては別箇所でNullPointerExceptionが発生するため削除
                        _ProcessData = new ProcessFactory();
                        _ProcessData.SetAssetAllData(db);

                        runOnUiThread(new Runnable() { //20200121 Add Thread Sasaki
                            @Override
                            public void run() {
                                //件数表示を再表示する
                                ResetCountDisp();
                                //ボタンを活性にする
                                SetButtonEnabled(true, true);
                                AllResetButton(true);
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
            }
        }.start();
    }

    /**
     * 確認メッセージを表示する
     * @param intButton
     * @param strMessage
     */
    protected void ConfirmMessage(int intButton, String strMessage) {
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
     * 戻るボタン処理
     * @return
     */
    private boolean returnButton() {
        if (_ProcessData.CountAll() > 0) {
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
        if (_ProcessData.CountAll() > 0) {
            onReturnConfirm(BUTTON_MENU);
            return true;
        } else {
            ReaderOff();
            onMenuReturn();
            return true;
        }
    }

    /**
     * 読取フラグをリセットする
     */
    protected void resetTagStart(){
        _TagReadStartFlag = false;
        _TransMesuarFlag = false;
    }

//20191125 Add Start Sasaki
    /**
     * 電波強度変更処理
     */
    protected void ChangeIntencity() {

        //選択した電波強度を設定する
        Spinner Intencity = findViewById(R.id.cmbIntencity);
        if (Intencity.getSelectedItemPosition() == CommonConst.INVENTORY_KBN.L.getInt()) {
            //強の場合
            C_Setting.setBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_KBN, C_Setting.INVENTORY_VALUE_L);
            C_Setting.setBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY, C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_L));
        } else if (Intencity.getSelectedItemPosition() == CommonConst.INVENTORY_KBN.M.getInt()) {
            //中の場合
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY_KBN, C_Setting.INVENTORY_VALUE_M);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY, C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_M));
        } else if (Intencity.getSelectedItemPosition() == CommonConst.INVENTORY_KBN.S.getInt()) {
            //弱の場合
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY_KBN, C_Setting.INVENTORY_VALUE_S);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY, C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_S));
        }

        // リーダーOFF
        ReaderOff();

        //リーダーON
        ReaderOn();
    }
//20191125 Add End Sasaki

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {}

    @Override
    protected void OpenMainActivity(){ }

    protected abstract void onReadReturn();
    protected abstract String GetSettingKey();

    //20210918 ADD Takase V2.0 START
    @Override
    public void onMenuReturn() {
        //読込データのクリア
        _ProcessData.Clear();
        super.onMenuReturn();
    }
    //20210918 ADD Takase V2.0 END
}
