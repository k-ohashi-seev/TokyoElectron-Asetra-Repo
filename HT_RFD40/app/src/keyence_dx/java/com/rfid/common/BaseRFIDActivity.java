package com.rfid.common;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.provider.Settings;
import androidx.annotation.Nullable;
import android.text.TextUtils;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.keyence.autoid.sdk.rfid.Filter;
import com.keyence.autoid.sdk.rfid.Status;
import com.keyence.autoid.sdk.rfid.params.CommonParams;
import com.keyence.autoid.sdk.rfid.params.IDType;
import com.keyence.autoid.sdk.rfid.params.InventoryParams;
import com.keyence.autoid.sdk.rfid.params.MemoryBank;
import com.keyence.autoid.sdk.rfid.params.TriggerMode;
import com.keyence.autoid.sdk.rfid.params.WriteIDParams;
import com.keyence.autoid.sdk.rfid.params.WriteParams;
import com.keyence.autoid.sdk.scan.DecodeResult;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.parameter.CommonParamsBuilder;
import com.rfid.common.parameter.InventoryParamsBuilder;
import com.rfid.common.parameter.WriteParamsBuilder;
import com.rfid.main.LoginMn;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.Locale;
import java.util.Timer;
import java.util.TimerTask;

/**
 * キーエンス製 DX-RH1及びDX-A600/DX-A400への接続用クラス
 * <ui>
 *     <li>タグ読取結果を受信します</li>
 *     <li>QR/バーコード読取結果を受信します</li>
 * </ui>
 */
public abstract class BaseRFIDActivity extends BaseActivity implements DxBarcodeInterface, DxRFIDInterface, DxDeviceInterface, CommonHtDateCheckDialog.Callback {
    /** RFIDリーダ操作クラス変数 */
    private WrapDxRFID _dxRfid;

    /** Android端末操作クラス変数 */
    private WrapDxBcd _dxBcd;

    /** 端末情報レシーバー */
    protected final DeviceBroadcastReceiver _deviceReceiver = DeviceBroadcastReceiver.getInstance();

    /** トリガーキー押下時のキーコード */
    protected static final int KEY_CODE_TRIGGER = 507;
    protected static final int KEY_CODE_TRIGGER_SIDE = 305;

    /** キー入力フラグ */
    private boolean _isKeyDown = false;

    /** 読取許可フラグ */
    private boolean _isScanPermission = false;

    /** バッテリー残量値 */
    private BatteryInfo _batteryInfo;

    /** バーコード 読取りハンドラー
     *  */
    private Handler handler_bar;

    /** RFID 読取りハンドラー */
    private Handler handler_rfid;

    /** 初期化済みフラグ */
    protected boolean _InitializedFlag = false;

    //※このクラスでは使用していないが、既存との整合性を保つため残しておく
    protected boolean InitializeActivity = false;

    /** バッテリー取得タイマー */
    private static Timer _timer;
    private static CountUpTimerTaskMain _timerTask;
    private static Handler handler_timer;

    /** 探索ハンドラー */
    private Handler messageHandler;

    /** 探索ステージ */
    private ReadPowerStage readPowerStageOnSearch = null;

    /** RSSI 積算配列 */
    protected ArrayList<Short> _ArrayRssi = new ArrayList<Short>();

    /** Beep音量 */
    protected BeepSound beepSound = null;
    protected BeepSoundSearch beepSoundSearch = null;

    /** RFIDリーダ接続可否フラグ */
    protected boolean blnConnectOpenFlag = true;

    /** 読取中のフリーズ回避用変数 */
    private Date readStartDate = null;
    private Runnable readTimeViewRefreshAction = null;
    private long readTimeMilliseconds = 0;
    private Handler handler = new Handler();

    /** 探索一覧用データ */
    protected HashMap<String, Short> LinkTagList = new HashMap<String, Short>();

    /** ダイアログコード */
    private static final int DIALOG_HT_DATE = 2001;
    private static final int DIALOG_ERR = 2002;

    private static class BatteryInfo{
        public int level = 0;
        public boolean isCharging = false;
    }

    /**
     * Stage of Read output value (1 to 5 stage)
     */
    private enum ReadPowerStage {
        STAGE_1
        , STAGE_2
        , STAGE_3
        , STAGE_4
        , STAGE_5
    }

    /** 時刻取得用フィルター */
    private static IntentFilter intentFilter = null;

    /**
     * Createイベント
     * @param savedInstanceState
     */
    @Override
    protected  void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        SQLiteDatabase db = null;

        //機器名称を設定する
        Common.ConnectRFIDName = CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString();

        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //RFIDリーダを使用しない設定の場合、RFIDリーダーに接続しない
            if (CommonConst.READING_MEDIUM.QR.getString().equals(SettingFactory.GetReadingMedium(db, this))) {
                blnConnectOpenFlag = false;
            }

            try {
                if (intentFilter == null) {
                    //時刻合わせ受信
                    //  ※シミュレータを立ち上げて、以下のコマンドを実行
                    //    adb shell am broadcast -a TIME_SETTING --es "value" "20211231235931"
                    BroadcastReceiver br = new BroadcastReceiver() {
                        @Override
                        public void onReceive(Context context, Intent intent) {
                            String value = intent.getStringExtra("value");
                            if (value == null || value.length() != 14) {
                                return;
                            }
                            String date = value.substring(0, 8);
                            String time = value.substring(8, 14);

                            UpdateAndroidTimeToast(date, time);
                        }
                    };
                    intentFilter = new IntentFilter();
                    intentFilter.addAction("TIME_SETTING");
                    registerReceiver(br, intentFilter);
                }
            } catch (Exception ex) {
            }

            //RFIDリーダとAndroid端末を操作可能にする
            if (blnConnectOpenFlag) {
                _dxRfid = WrapDxRFID.getInstance();
                _dxRfid.createRFIDReader(getApplicationContext());
            }
            _dxBcd = WrapDxBcd.getInstance();
            _dxBcd.createScanReader(getApplicationContext());
            connectReader = isCommScanner();

            _isScanPermission = false;

            //Handler初期化
            handler_bar = new Handler(getMainLooper());
            handler_rfid = new Handler(getMainLooper());
            handler_timer = new Handler(getMainLooper());
            messageHandler = new Handler(getMainLooper());
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        } finally {
            if(db != null){
                db.close();
            }
        }
    }

    /**
     * 破棄イベント
     */
    @Override
    protected void onDestroy() {
        try {
            super.onDestroy();
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (_dxRfid != null) {
            _dxRfid.setCallBackListener(this);
        }
        if (_dxBcd != null) {
            _dxBcd.setCallBackListener(this);
        }
        _deviceReceiver.setCallBackDeviceListener(this, this);

        //メニューを再表示
        invalidateOptionsMenu();
    }

    @Override
    protected void onPause() {
        super.onPause();
        if (_dxRfid != null) {
            _dxRfid.releaseCallBackListener();
        }
        if (_dxBcd != null) {
            _dxBcd.releaseCallBackListener();
        }
        _deviceReceiver.releaseCallBackDeviceListener(this);
    }

    /**
     * RFIDスキャナに接続された際に呼ばれるイベント
     */
    public void OnScannerAppeared() {}

    protected abstract void OpenMainActivity();


    /**
     * RFIDリーダーの接続状態を返却
     */
    public boolean isCommScanner() {
        if (_dxBcd == null) return false;
        return _dxBcd.isReaderConnect();
    }

    /**
     * リーダー接続
     */
    protected void Reader_Connect() {
        //メニュー画面を開く
        //OpenMainActivity();
    }

    /**
     * メニュー変更イベント処理
     * @param menu
     * @return
     */
    @Override
    public boolean onPrepareOptionsMenu(Menu menu) {
        //super.onPrepareOptionsMenu(menu);

        //MenuInflater inflater = getMenuInflater();
        //inflater.inflate(R.menu.menu_main, menu);

        MenuItem menuConnect = menu.findItem(R.id.action_connect);
        if(connectReader) {
            menuConnect.setIcon(R.drawable.connect_48);
        } else {
            menuConnect.setIcon(R.drawable.connect_disable_48);
        }
        return true;
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
            case DIALOG_HT_DATE: //HT日時エラー
                //システム日時設定
                Intent intent = new Intent();
                intent.setAction(Settings.ACTION_DATE_SETTINGS);
                startActivityForResult(intent, DIALOG_HT_DATE);
                break;
            case DIALOG_ERR:    //エラー
                finish();
                break;
        }
    }

    /**
     * 遷移先から戻った場合のイベント
     * @param requestCode リクエストコード
     * @param resultCode　結果
     * @param data　Intent
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        switch (requestCode) {
            case DIALOG_HT_DATE: //HT日時エラー
                finish();
                break;
        }
    }

    /**
     * Start service in the background
     * ※既存の動作との整合性を取るため、処理は実装しないが関数は残しておく
     */
    public void startService() {}

    /**
     * バッテリー残量変化時イベント
     * @param batteryLevel バッテリ残量(パーセント)
     */
    @Override
    public void onBatteryModify(boolean isCharging, int batteryLevel) {
        try {
            if (_batteryInfo == null) {
                _batteryInfo = new BatteryInfo();
            }

            //バッテリー情報を保持
            _batteryInfo.level = batteryLevel;
            _batteryInfo.isCharging = isCharging;
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     *  タイマー開始
     */
    protected void StartTimer() {
        //Readerを使用しない場合、終了
        if (!blnConnectOpenFlag){
            return;
        }

        if (_timer == null) {
            // Timer インスタンスを生成
            _timer = new Timer();
            // TimerTask インスタンスを生成
            _timerTask = new CountUpTimerTaskMain();

            // スケジュールを設定 500msec
            _timer.schedule(_timerTask, 0, 500);
        }

    }

    /**
     * タイマー停止
     */
    protected void StopTimer() {
        //タイマー停止
        if (_timer != null) {
            _timer.cancel();
        }
        _timerTask = null;
        _timer = null;

        //リーダーOFF
        ReaderOff();

    }

    /**
     * タイマータスク
     */
    private class CountUpTimerTaskMain extends TimerTask {
        @Override
        public void run() {
            // handlerを使って処理をキューイングする
            handler_timer.post(new Runnable() {
                public void run() {
                    try {
                        //バッテリー残量を取得する
                        BatteryRemainDisp();
                    } catch (Exception ex) {
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    }
                }
            });
        }
    }

   /**
     * 入力キーコード取得イベント
     * @param event　キーイベント
     * @return True/False
     */
    @Override
    public boolean dispatchKeyEvent(KeyEvent event) {
        try {
            if(event.getKeyCode() == KEY_CODE_TRIGGER && event.getAction() == KeyEvent.ACTION_DOWN && _isScanPermission) {
                if(!_isKeyDown){
                    //トリガーキー押下中は読取を行う
                    _dxBcd.startRead();
                    _isKeyDown = true;
                }
            }else if(event.getKeyCode() == KEY_CODE_TRIGGER && event.getAction() == KeyEvent.ACTION_UP && _isScanPermission){
                //トリガーキーを離すと読取を停止する
                if(_isKeyDown){
                    _dxBcd.stopRead();
                }
                _isKeyDown = false;
            }
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_SCAN, "", this) + ":" + Common.toString(ex), this);
        }
        return super.dispatchKeyEvent(event);
    }

    /**
     * トリガー押下中かを取得する
     * @return True:トリガー押下中 False:トリガー押下外
     */
    protected boolean isKeyDown() {
        return _isKeyDown;
    }

/***********************************
 * QR/バーコード関連
 ***********************************/
    /**
     * QR/バーコード読取結果通知イベント
     * @param result 実行結果
     * @param codeType コード種別
     * @param readData 読取データ
     * @param bytReadData 読取データ(バイトデータ)
     */
    @Override
    public void onBcdDataReceived(final DecodeResult.Result result, String codeType, final String readData, byte[] bytReadData) {
        handler_bar.post(new Runnable() {
            @Override
            public void run() {
                try {
                    if (result == DecodeResult.Result.SUCCESS) {
                        //読取成功時
                        //サブクラスで処理を実装
                        BarcodeDataReceived(readData);
                    } else {
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, "DecodeResult：" + result.toString());
                    }
                } catch (Exception ex) {
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    showErrorMessage(ex.getMessage());
                }
            }
        });
    }

/***********************************
 * RFID関連
 ***********************************/
    /**
     * RFタグ読取結果通知イベント
     * @param status 実行結果
     * @param readEpc 読取EPC
     * @param rssi 応答電波強度
     * @param memoryBank 対象メモリバンク
     * @param bankData メモリバンクからの読取データ
     * @param readPc 読取PC値
     */
    @Override
    public void onReadResultReceived(final Status status, final String readEpc, final float rssi, MemoryBank memoryBank, byte[] bankData, String readPc) {
        handler_rfid.post(new Runnable() {
            @Override
            public void run() {
                try {
                    if (status == Status.STATUS_OK) {
                        //読取成功時
                        //サブクラスで処理を実装
                        RFIDDataReceived(readEpc, (short)rssi, readPc);
                    }
                } catch (Exception ex) {
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    showErrorMessage(ex.getMessage());
                }
            }
        });
    }

    /**
     * EPCの先頭からの値を対象としたフィルタを作成します。
     * @param hexMaskData フィルター設定する16進数文字列
     * @return フィルター
     * @throws Exception 例外
     */
    protected Filter createFilterFromTop(String hexMaskData) throws Exception {
        if (_dxRfid == null) {return null;}
        byte[] maskData = stringToByte(hexMaskData);
        return _dxRfid.createDataFilter(MemoryBank.EPC, 32, (maskData.length * 8) -1, maskData);
    }

    /**
     * RFID リーダーＯＮ(探索動作)
     *
     * @param prmIntencity  電波強度
     * @param prmSession    セッション
     * @param prmQValue     Ｑバリュー
     * @param prmAroundWave 垂直・水平読取り(=true)/水平読取り(=false)
     * @param prmCheckUII   探索UII(HEX 24文字)
     */
    public boolean fncRFID_Search_Start(int prmIntencity, String prmSession, short prmQValue, boolean prmAroundWave, String prmCheckUII) {
        if (_dxRfid == null) return false;

        if (!_dxRfid.isReading()) {
            try {
                //インベントリ設定
                InventoryParams invParams = setScannerSettings(false,
                                                               prmIntencity,
                                                               prmSession,
                                                               prmQValue,
                                                               true,
                                                               prmAroundWave,
                                                               true,
                                                               false,
                                                               true);

                //対象UIIのフィルターを作成
                Filter filter = createFilterFromTop(prmCheckUII);

                //UII指定で読取開始
                _dxRfid.inventoryStart(filter, invParams);
                return true;

            } catch (Exception e) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                return false;
            }
        } else {
            return true;
        }
    }

    /**
     * RFID リーダーＯＮ(探索一覧動作)
     *
     * @param prmIntencity  電波強度
     * @param prmSession    セッション
     * @param prmQValue     Ｑバリュー
     * @param prmAroundWave 垂直・水平読取り(=true)/水平読取り(=false)
     * @param prmCheckUII   探索UII(HEX 24文字)
     */
    public boolean fncRFID_Search_List_Start(int prmIntencity, String prmSession, short prmQValue, boolean prmAroundWave, String prmCheckUII) {
        if (_dxRfid == null) return false;

        if (!_dxRfid.isReading()) {
            try {
                //インベントリ設定
                InventoryParams invParams = setScannerSettings(false,
                                                                prmIntencity,
                                                                prmSession,
                                                                prmQValue,
                                                                false,
                                                                prmAroundWave,
                                                                true,
                                                                false,
                                                                true);

                //対象UIIのフィルターを作成
                Filter filter = createFilterFromTop(prmCheckUII);

                //UII指定で読取開始
                _dxRfid.inventoryStart(filter, invParams);
                return true;

            } catch (Exception e) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                return false;
            }
        } else {
            return true;
        }
    }

    /**
     * RFID リーダーＯＮ(通常動作)
     *
     * @param prmIntencity     電波強度
     * @param prmSession       セッション
     * @param prmQValue        Ｑバリュー
     * @param prmMultchRd      マルチ読取り(=true)/シングル読取り(=false)
     * @param prmAroundWave    垂直・水平読取り(=true)/水平読取り(=false)  ADD 2019/03/12
     * @param prmDoubleInhibit 二度読み防止ON(=true)/OFF(=false)  ADD 2019/03/12
     * @param prmSettingNeed   設定必要(=true)/設定不要(=false)  ADD 2019/03/26
     */
    private boolean fncRFID_Read_Start(int prmIntencity, String prmSession, short prmQValue, boolean prmMultchRd, boolean prmAroundWave, boolean prmDoubleInhibit, boolean prmSettingNeed) {
        if (_dxRfid == null) return false;

        if (!_dxRfid.isReading()) {
            try {
                InventoryParams invParams = null;
                if (prmSettingNeed) {
                    //設定が必要な場合
                    //インベントリ設定
                    invParams = setScannerSettings(false,
                                                    prmIntencity,
                                                    prmSession,
                                                    prmQValue,
                                                    prmMultchRd,
                                                    prmAroundWave,
                                                    prmDoubleInhibit,
                                                    false,
                                                    false);
                }

                //インベントリ開始
                _dxRfid.inventoryStart(null, invParams);
                return true;
            } catch (Exception e) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                return false;
            }
        } else {
            return true;
        }
    }

    /**
     * 読取した探索データを取得する
     * @param strTagID
     * @return 電波強度
     */
    protected String ReadSearchTag(String strTagID, String strKey){
        String strRead = "";
        if (LinkTagList.containsKey(strTagID)) {
            strRead = String.valueOf(LinkTagList.get(strTagID));
        }

        return strRead;
    }

    /**
     * インベントリ用の設定作成
     *
     * @param sessionInit      セッション初期化
     * @param prmIntencity     電波強度
     * @param prmSession       セッション
     * @param prmQValue        Ｑバリュー
     * @param prmMultchRd      マルチ読取り(=true)/シングル読取り(=false)
     * @param prmAroundWave    垂直・水平読取り(=true)/水平読取り(=false)
     * @param prmDoubleInhibit 二度読み防止ON(=true)/OFF(=false)
     * @param prmReadSound     読取り通知音(=true)/OFF(=false)
     * @param searchMode       探索時の呼び出しか
     */
    private InventoryParams setScannerSettings(@Nullable Boolean sessionInit, int prmIntencity, String prmSession, short prmQValue,
                                               boolean prmMultchRd, boolean prmAroundWave, boolean prmDoubleInhibit, boolean prmReadSound, boolean searchMode) {
        try {
            //リーダ共通設定作成
            CommonParamsBuilder commPrmBuilder = new CommonParamsBuilder();
            commPrmBuilder.transmitPower(prmIntencity);
            commPrmBuilder.sessionFlag(WrapDxRFID.getSessionFlg(Integer.parseInt(prmSession)));

            //現在リーダに適用されている設定を取得して、パラメータを上書き適用
            CommonParams params = commPrmBuilder.build(_dxRfid.getConfig(new CommonParams()));
            _dxRfid.setReaderCommConfig(params);

            //インベントリ設定作成
            InventoryParamsBuilder invPrmBuilder = new InventoryParamsBuilder();

            //二度読み許可設定  True:二度読みあり False:二度読みなし
            invPrmBuilder.allowDoubleReading(prmDoubleInhibit);

            if (prmMultchRd) {
                //トリガモード モメンタリモード
                invPrmBuilder.triggerMode(TriggerMode.CONTINUOUS);
            } else {
                //トリガモード  AUTO　OFF
                invPrmBuilder.triggerMode(TriggerMode.ALWAYS_ON);
            }

            return invPrmBuilder.build();
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            return null;
        }
    }

    /**
     * 書込み用の設定作成
     *
     * @param prmIntencity     電波強度
     * @param prmSession       セッション
     * @param prmData          書込みデータ
     * @param isPcOverWrite    PC上書きフラグ
     */
    private WriteParams setScannerWriteSettings(int prmIntencity, String prmSession, byte[] prmData, boolean isPcOverWrite) {
        try {
            //リーダ共通設定作成
            CommonParamsBuilder commPrmBuilder = new CommonParamsBuilder();
            commPrmBuilder.transmitPower(prmIntencity);
            commPrmBuilder.sessionFlag(WrapDxRFID.getSessionFlg(Integer.parseInt(prmSession)));

            //現在リーダに適用されている設定を取得して、パラメータを上書き適用
            CommonParams params = commPrmBuilder.build(_dxRfid.getConfig(new CommonParams()));
            _dxRfid.setReaderCommConfig(params);

            //書込み設定作成
            WriteParamsBuilder writePrmBuilder = new WriteParamsBuilder();
            writePrmBuilder.data(prmData);

            //PCを上書きする場合
            if (isPcOverWrite) {
                writePrmBuilder.offset(CommonConst.PC_WORD_TYPE_START_POS);
            }

            return writePrmBuilder.build();
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            return null;
        }
    }

    // RFID リーダーＯＦＦ
    public void subRFID_Read_Stop() throws Exception {
        if (_dxRfid == null) return;

        //インベントリ停止
        if (_dxRfid.isReading()) {
            _dxRfid.inventoryStop();
        }
    }

    /**
     * 二度読みバッファをクリア
     */
    public void subClearDuplexBuffer() {

    }

    /**
     * バーコード リーダーＯＮ
     */
    protected void Barcode_Read_Start() {
        _dxBcd.unLockScanner();
        _isScanPermission = true;
    }

    /**
     * バーコード リーダーＯＦＦ
     */
    protected void Barcode_Read_Stop() {
        _dxBcd.lockScanner();
        _isScanPermission = false;
    }

    /**
     * RFID リーダーＯＮ
     *
     * @param prmParmChange パラメータ変更(=true)/変更なし(=false)
     * @param strKey 電波強度で使用する画面ごとのキー
     */
    protected void RFID_Read_Start(boolean prmParmChange, final String strKey) {

        final Activity activity = this;
        final boolean parmChange = prmParmChange;

        new Thread() {
            @Override
            public void run() {
                if (isCommScanner()) {
                    try {
                        if (beepSound == null){
                            beepSound = new BeepSound(activity);
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                    fncRFID_Read_Start(Integer.valueOf(C_Setting.getBTReadIntensity(activity, strKey)),
                            C_Setting.getBTReadSession(activity, strKey),
                            Short.valueOf(C_Setting.getBTReadQValue(activity, strKey))
                            , true, true, false, parmChange);
                    if (!_InitializedFlag) {
                        subClearDuplexBuffer();  //二度読みバッファクリア
                        _InitializedFlag = true;     //初期化済み
                    }
                }

            }
        }.start();
    }

    /**
     * RFID 書込み ＯＮ
     *
     * @param prmParmChange パラメータ変更(=trWriteue)/変更なし(=false)
     * @param strKey 電波強度で使用する画面ごとのキー
     */
    protected void RFID_Write_Start(boolean prmParmChange, final String strKey) {

        final Activity activity = this;
        final boolean parmChange = prmParmChange;

        new Thread() {
            @Override
            public void run() {
                if (isCommScanner()) {
                    fncRFID_Read_Start(Integer.valueOf(C_Setting.getBTReadIntensity(activity, strKey)),
                            C_Setting.getBTReadSession(activity, strKey),
                            Short.valueOf(C_Setting.getBTReadQValue(activity, strKey))
                            , true, true, false, parmChange);
                    if (!_InitializedFlag) {
                        subClearDuplexBuffer();  //二度読みバッファクリア
                        _InitializedFlag = true;     //初期化済み
                    }
                }

            }
        }.start();
    }

    /**
     * RFID リーダーＯＦＦ
     */
    protected void RFID_Read_Stop() {
        try {
            if (blnConnectOpenFlag) {
                if (isCommScanner()) {
                    subClearDuplexBuffer();  //二度読みバッファクリア
                    subRFID_Read_Stop();

                    //タイマーカウントストップ
                    ReadTimeStop();

                    //音を停止
                    if (beepSound != null) {
                        beepSound.Destory();
                        beepSound = null;
                    }
                }
            }
        } catch (Exception ex) {
            ex.printStackTrace();
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * 探索開始
     * @param strAssetID
     * @param strKey
     */
    protected void Rfid_Search_Start(String strAssetID, String strKey){
        try {
            if (isCommScanner()) {
                //Beep音Create
                try {
                    if (beepSoundSearch == null){
                        beepSoundSearch = new BeepSoundSearch(this);
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }

                _ArrayRssi.clear();
                fncRFID_Search_Start(Integer.valueOf(C_Setting.getBTReadIntensity(this, strKey)),
                        C_Setting.getBTReadSession(this, strKey),
                        Short.valueOf(C_Setting.getBTReadQValue(this, strKey)), true, strAssetID);
                messageHandler.post(r);
            }
        } catch (Exception ex) {
            ex.printStackTrace();
        }
    }

    /**
     * 探索一覧開始
     * @param strAssetID
     * @param strKey
     */
    protected void Rfid_Search_List_Start(String strAssetID, String strKey){
        try {
            if (isCommScanner()) {
                fncRFID_Search_List_Start(Integer.valueOf(C_Setting.getBTReadIntensity(this, strKey)),
                        C_Setting.getBTReadSession(this, strKey),
                        Short.valueOf(C_Setting.getBTReadQValue(this, strKey)), true, strAssetID);
            }
        } catch (Exception ex) {
            ex.printStackTrace();
        }
    }

    /**
     * 探索終了
     */
    protected void Rfid_Search_Stop(){
        try {
            if (isCommScanner()) {
                subRFID_Read_Stop();
                messageHandler.removeCallbacks(r);

                //音を停止
                if (beepSoundSearch != null) {
                    beepSoundSearch.Destory();
                    beepSoundSearch = null;
                }
            }
        } catch (Exception ex) {
            ex.printStackTrace();
        }
    }

    //タイマータスク
    Runnable r = new Runnable() {
        @Override
        public void run() {
            try {
                float readPowerLevel = 0;
                if(_ArrayRssi.size() <= 0) {
                    setReadPowerLevelOnSearch(null);
                }
                else {
                    for (int i = 0; i < _ArrayRssi.size(); i++) {
                        readPowerLevel = readPowerLevel + _ArrayRssi.get(i);
                    }
                    readPowerLevel = readPowerLevel / _ArrayRssi.size();
                    setReadPowerLevelOnSearch(readPowerLevel);
                    _ArrayRssi.clear();
                }
                messageHandler.postDelayed(this,375);
            }
            catch (Exception ex) {

            }
        }
    };

    /**
     * タグ書込み
     * @param strTag 既存EPC
     * @param strNewTag 新規EPC
     * @param strOldPc 既存PC
     * @param newEpcBitSize 新規EPCサイズ
     */
    protected void WriteTag(String strTag, String strNewTag, String strOldPc, int newEpcBitSize) throws Exception {
        String strNewPc = "";
        boolean isPcOverWrite = false;

        //PCの書換が必要かどうかをチェック
        if (Common.needOverWritePc(newEpcBitSize, strOldPc)) {
            //PC書換あり
            strNewPc = Common.calcTagPcHex(newEpcBitSize, strOldPc);
            isPcOverWrite = true;
        }

        //書込み設定
        WriteParams writeParams = setScannerWriteSettings(Integer.valueOf(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_TAGLINK)),
                                                        C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_TAGLINK),
                                                        convertHexStrToHexBytes(strNewPc + strNewTag),
                                                        isPcOverWrite);

        //UII指定用のフィルター作成
        Filter filter = createFilterFromTop(strTag);

        //書込み実施
        _dxRfid.writeTag(filter, writeParams);

        //音を出す
        //Beep音Create
        try {
            if (beepSound == null) {
                beepSound = new BeepSound(this);
            }
            beepSound.BeepWrite();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * Set the display and the sound of the Read output value when searching
     * @param readPowerLevel  Read output value when searching
     */
    private void setReadPowerLevelOnSearch(@Nullable Float readPowerLevel) {
        try {
            // If the Read output value is also null last time, do nothing
            //if (readPowerLevelOnSearch == null && readPowerLevel == null) {
            //    return;
            //}
            //readPowerLevelOnSearch = readPowerLevel;

            // Reflect the changed Read output value on the display
            String text = readPowerLevel != null ? String.format("%.2f", readPowerLevel) : "";
            TextView textView = (TextView) findViewById(R.id.labSchRssiVal);
            textView.setText(text);

            //電波のステージを取得する
            ReadPowerStage readPowerStage = getReadPowerStage(readPowerLevel);

            //音を出す
            setSearchSound(readPowerStage);
            if (readPowerStageOnSearch == readPowerStage) {
                return;
            }
            readPowerStageOnSearch = readPowerStage;

            // Reflects the stage of Read output value after change in Yen and sound
            setSearchCircle(readPowerStage);

        }
        catch (Exception ex) {

        }
    }

    /**
     * Seting the sound according to the stage of Read output value
     * @param readPowerStage  The stage of Read output value  Do not playback if null is specified
     */
    private void setSearchSound(@Nullable ReadPowerStage readPowerStage) {
        try {
            if (readPowerStage == null) {
                BeepAudioTracks.stopAudioTracks();
                return;
            }

            switch (readPowerStage) {
                case STAGE_1:
                    beepSoundSearch.BeepSearch1();
                    break;
                case STAGE_2:
                    beepSoundSearch.BeepSearch2();
                    break;
                case STAGE_3:
                    beepSoundSearch.BeepSearch3();
                    break;
                case STAGE_4:
                    beepSoundSearch.BeepSearch4();
                    break;
                case STAGE_5:
                    beepSoundSearch.BeepSearch5();
                    break;
                default:
                    return;
            }
        }
        catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * Returns the stage corresponding to the Read output value
     * @param readPowerLevel  Read output value
     * @return  The stage corresponding to the Read output value Return null if null is specified to argument
     */
    private ReadPowerStage getReadPowerStage(@Nullable Float readPowerLevel) {
        try {
            if (readPowerLevel == null) {
                return null;
            }

            int stage2MaxReadPowerLevel = getResources().getInteger(R.integer.keyence_dx_stage2_max_read_power_level_on_search);
            int stage3MaxReadPowerLevel = getResources().getInteger(R.integer.keyence_dx_stage3_max_read_power_level_on_search);
            int stage4MaxReadPowerLevel = getResources().getInteger(R.integer.keyence_dx_stage4_max_read_power_level_on_search);
            int stage5MaxReadPowerLevel = getResources().getInteger(R.integer.keyence_dx_stage5_max_read_power_level_on_search);
            if (readPowerLevel <= stage5MaxReadPowerLevel) {
                return ReadPowerStage.STAGE_5;
            }
            else if (readPowerLevel <= stage4MaxReadPowerLevel) {
                return ReadPowerStage.STAGE_4;
            }
            else if (readPowerLevel <= stage3MaxReadPowerLevel) {
                return ReadPowerStage.STAGE_3;
            }
            else if (readPowerLevel <= stage2MaxReadPowerLevel) {
                return ReadPowerStage.STAGE_2;
            }
            else {
                return ReadPowerStage.STAGE_1;
            }
        }
        catch (Exception ex) {
            return null;
        }
    }

    /**
     * 探索時に読取値に応じて画面に表示する画像を設定
     * @param readPowerStage 読取ステージ値
     */
    private void setSearchCircle(@Nullable ReadPowerStage readPowerStage) {
        try {
            // Do not display if the stage of Read output value is null
            ImageView imageView = (ImageView) findViewById(R.id.image_search_circle);
            if (readPowerStage != null) {
                imageView.setVisibility(ImageView.VISIBLE);
            }
            else {
                imageView.setVisibility(ImageView.INVISIBLE);
                return;
            }

            // Configure the display of Yen according to the stage of Read output value
            int circleResId;
            switch (readPowerStage) {
                case STAGE_1:
                    circleResId = R.mipmap.locate_tag_circle_over_35;
                    break;
                case STAGE_2:
                    circleResId = R.mipmap.locate_tag_circle_48_to_36;
                    break;
                case STAGE_3:
                    circleResId = R.mipmap.locate_tag_circle_62_to_49;
                    break;
                case STAGE_4:
                    circleResId = R.mipmap.locate_tag_circle_74_to_63;
                    break;
                case STAGE_5:
                    circleResId = R.mipmap.locate_tag_circle_under_75;
                    break;
                default:
                    return;
            }
            imageView.setImageResource(circleResId);
        }
        catch (Exception ex) {

        }
    }

    /**
     * 電池残量表示
     */
    protected void BatteryRemainDisp() {
        try {
            //バッテリー残量未取得は無視
            if (_batteryInfo == null) return;

            //RFIDリーダが装着されていない場合は無視
            if (_dxRfid == null || !isCommScanner()) return;

            int batteryResId = R.mipmap.battery_0;
            if (_batteryInfo.level < 30) {
                //30未満
                batteryResId = R.mipmap.battery_1;
            } else if (_batteryInfo.level < 70) {
                //30以上70未満
                batteryResId = R.mipmap.battery_2;
            } else {
                //70以上
                batteryResId = R.mipmap.battery_full;
            }

            //電池残量が少ない場合、警告メッセージを表示する
            //※充電中は表示しない
            if (_batteryInfo.level <= 10 && !_batteryInfo.isCharging) {
                ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_BATTERY_CHARGE, "", this), "");
            }

            final int _battery = batteryResId;
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    ImageView _imageView = (ImageView) findViewById(R.id.ImgViewMnuBattery);
                    if (_imageView != null) {
                        _imageView.setImageResource(_battery);
                    }
                }
            });

        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * 電波強度の表示幅を算出して返す
     * @param strIntencity
     * @return
     */
    public int getReadPowerWidth(String strIntencity) {
        int returnWidth = 0;

        if (strIntencity == null || "".equals(strIntencity)) {
            return 0;
        }
        //電波強度のステージを取得する
        ReadPowerStage readPowerStage = getReadPowerStage(Float.valueOf(strIntencity));

        switch (readPowerStage) {
            case STAGE_1:
                returnWidth = 5;
                break;
            case STAGE_2:
                returnWidth = 4;
                break;
            case STAGE_3:
                returnWidth = 3;
                break;
            case STAGE_4:
                returnWidth = 2;
                break;
            case STAGE_5:
                returnWidth = 1;
                break;
        }

        return returnWidth;
    }

    /**
     * Android端末の時刻を更新する
     * @param strGetDate
     * @param strGetTime
     */
    protected boolean UpdateAndroidTime(String strGetDate, String strGetTime) {
        try {
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm");

            //サーバー日時を変換
            String strDate = strGetDate.substring(0, 4) + "/" + strGetDate.substring(4, 6) + "/" + strGetDate.substring(6);
            String strTime = strGetTime.substring(0, 2) + ":" + strGetTime.substring(2, 4);
            Date serverDate = sdf.parse(strDate + " " + strTime);
            try {
                serverDate = sdf.parse(strDate + " " + strTime);
            } catch (Exception e) {
                e.printStackTrace();
            }

            //サーバー日時を比較用に変換
            Calendar serverDateFrom = Calendar.getInstance();
            serverDateFrom.setTime(serverDate);
            serverDateFrom.add(Calendar.MINUTE, -1);

            Calendar serverDateTo = Calendar.getInstance();
            serverDateTo.setTime(serverDate);
            serverDateTo.add(Calendar.MINUTE, 1);

            //HTのシステム日時を取得
            Calendar htDate = Calendar.getInstance();

            //サーバー日時とHT日時のズレをチェック
            if (serverDateFrom.getTime().compareTo(htDate.getTime()) > 0 ||
                    serverDateTo.getTime().compareTo(htDate.getTime()) < 0) {
                //1分以上ズレている場合はダイアログを表示
                final CommonHtDateCheckDialog.Builder dialog = new CommonHtDateCheckDialog.Builder();
                Bundle bundle = new Bundle();
                bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, DIALOG_HT_DATE);
                dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DATE_SETTING, "日時設定", this));
                dialog.message(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_HT_DATE, "マスタ受信処理を中断して、Androidの時刻設定機能で修正してください。", this));
                dialog.titleServerDate(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SERVER_DATE, "サーバー日時", this));
                dialog.titleHtDate(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_HT_DATE, "HT日時", this));
                dialog.serverDate(sdf.format(serverDate));
                dialog.htDate(sdf.format(htDate.getTime()));
                dialog.params(bundle);
                dialog.show(getSupportFragmentManager(), "Activity");
                return false;
            }

            return true;
        } catch (Exception e) {
            e.printStackTrace();
            showErrDialog(DIALOG_ERR, "", SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_UPD_DATE, "", this));
            return false;
        }
    }

    /**
     * Android端末の時刻を更新する
     * @param strGetDate
     * @param strGetTime
     */
    protected boolean UpdateAndroidTimeToast(String strGetDate, String strGetTime) {
        try {
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm");

            //サーバー日時を変換
            String strDate = strGetDate.substring(0, 4) + "/" + strGetDate.substring(4, 6) + "/" + strGetDate.substring(6);
            String strTime = strGetTime.substring(0, 2) + ":" + strGetTime.substring(2, 4);
            Date serverDate = sdf.parse(strDate + " " + strTime);
            try {
                serverDate = sdf.parse(strDate + " " + strTime);
            } catch (Exception e) {
                e.printStackTrace();
            }

            //サーバー日時を比較用に変換
            Calendar serverDateFrom = Calendar.getInstance();
            serverDateFrom.setTime(serverDate);
            serverDateFrom.add(Calendar.MINUTE, -1);

            Calendar serverDateTo = Calendar.getInstance();
            serverDateTo.setTime(serverDate);
            serverDateTo.add(Calendar.MINUTE, 1);

            //HTのシステム日時を取得
            Calendar htDate = Calendar.getInstance();

            //サーバー日時とHT日時のズレをチェック
            if (serverDateFrom.getTime().compareTo(htDate.getTime()) > 0 ||
                    serverDateTo.getTime().compareTo(htDate.getTime()) < 0) {
                //1分以上ズレている場合はトーストを表示
                String serverTimeMsg = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SERVER_DATE, "サーバー日時", this) + ":" + sdf.format(serverDate);
                String htTimeMsg = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_HT_DATE, "HT日時", this) + ":" + sdf.format(htDate.getTime());
                String msg = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_HT_DATE, "マスタ受信処理を中断して、Androidの時刻設定機能で修正してください。", this);
                String br = System.getProperty("line.separator");
                Toast.makeText(this, msg + br + serverTimeMsg + br + htTimeMsg, Toast.LENGTH_LONG).show();
                return false;
            }

            return true;
        } catch (Exception e) {
            e.printStackTrace();
            showToastMessage(e.getMessage());
            return false;
        }
    }

    /**
     * エラーダイアログを表示する
     * @param intButton
     */
    private void showErrDialog(int intButton, String title, String message) {
        //エラーダイアログを表示
        final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, true);
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, intButton);
        dialog.title(title);
        dialog.message(message);
        dialog.params(bundle);
        dialog.show();
    }

    /**
     * 文字列→BYTE 配列変換
     *
     * @param prmHex 16進 文字列
     * @return
     */
    private byte[] stringToByte(String prmHex) {
        byte[] ansBytes = new byte[prmHex.length() / 2];
        for (int index = 0; index < ansBytes.length; index++) {
            ansBytes[index] = (byte) Integer.parseInt(prmHex.substring(index * 2, (index + 1) * 2), 16);
        }
        return ansBytes;
    }

    protected void buzzer_B2() {
        try {
            if (beepSound == null ) {
                beepSound = new BeepSound(this);
            }
            beepSound.BeepConnect();
        } catch (Exception ex) {
            Log.d(CommonConst.DIALOG_ERROR_TITLE, ex.getMessage());
        }
    }

    protected boolean ConnectStatus(){
        //RFIDリーダが装着されていればTrue
        return isCommScanner();
    }

    /**
     * 読取途中でフリーズする回避対応
     */
    protected void ReadTimeStart(){
        readStartDate = new Date();
        readStartDate.setTime(readStartDate.getTime() - readTimeMilliseconds);

        readTimeViewRefreshAction = new Runnable() {
            @Override
            public void run() {
                // Keep the difference from the measurement start time

                Date nowDate = new Date();
                readTimeMilliseconds = nowDate.getTime() - readStartDate.getTime();

                // Update the display
                ((TextView)findViewById(R.id.textView_time_counter)).setText(String.valueOf(readTimeMilliseconds));
                // Update again after ten milliseconds

                handler.postDelayed(this, 10);
            }
        };
        handler.post(readTimeViewRefreshAction);
    }

    /**
     * 読取途中でフリーズする回避対応
     */
    private void ReadTimeStop() {
        handler.removeCallbacks(readTimeViewRefreshAction);
        readTimeMilliseconds = 0;
        readTimeViewRefreshAction = null;
        readStartDate = null;
    }

    /**
     * 16進数文字列を16進数バイト配列に変換します。
     * @param hexStr ASCII文字列
     * @return 16進数バイト配列
     */
    public static byte[] convertHexStrToHexBytes(String hexStr) {
        if (hexStr.equals("")) {return null;}

        byte[] maskData = new byte[hexStr.length() / 2];

        for (int i = 0; i < maskData.length; i++) {
            //2文字ずつ切り出し、16進数に変換
            maskData[i] = (byte)Integer.decode("0x" + hexStr.substring(i * 2, (i + 1) * 2)).intValue();
        }

        return maskData;
    }

    /**
     * 電波強度の設定可能範囲値を作成
     * @return 設定可能範囲
     */
    protected String[] SetIntensityRange() {
        // ※この機種は設定画面で電波強度範囲を設定しているため、このメソッドは使用しない。
        return new String[1];
    }

    /**
     * 読取開始をサブクラスで実装する
     */
    protected abstract void ReaderOn();

    /**
     * 読取終了をサブクラスで実装する
     */
    protected abstract void ReaderOff();

    /**
     * RFIDタグ受信時の処理をサブクラスで実装する
     * @param strReadData EPC
     * @param tempRSSI RSSI
     * @param strPcHex PC
     */
    protected abstract void RFIDDataReceived(String strReadData, short tempRSSI, String strPcHex);

    /**
     * バーコード受信時の処理をサブクラスで実装する
     * @param strReadData
     */
    protected abstract void BarcodeDataReceived(String strReadData);

    /**
     * RFIDセッティング用の各画面固有のキーをサブクラスで実装する
     * @return
     */
    protected abstract String GetSettingKey();

    /**
     * Zebraで使用(Interfaceのため)
     */
    protected void RFID_Setting_Start(){}
    protected void onDisconnectEvent(){}
}
