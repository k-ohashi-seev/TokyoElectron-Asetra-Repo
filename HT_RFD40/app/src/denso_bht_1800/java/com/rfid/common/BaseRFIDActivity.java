package com.rfid.common;

import android.app.Activity;
import android.app.ActivityManager;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.os.Handler;
import androidx.annotation.Nullable;
import android.text.TextUtils;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.CheckBox;
import android.widget.ImageView;
import android.widget.Spinner;
import android.widget.TextView;

import com.densowave.bhtsdk.barcode.BarcodeManager;
import com.densowave.bhtsdk.barcode.BarcodeScanner;
import com.densowave.bhtsdk.systemsettings.SystemSettingsLibrary;
import com.densowave.scannersdk.Barcode.BarcodeData;
import com.densowave.scannersdk.Barcode.BarcodeDataReceivedEvent;
import com.densowave.scannersdk.Common.CommException;
import com.densowave.scannersdk.Common.CommManager;
import com.densowave.scannersdk.Common.CommScanner;
import com.densowave.scannersdk.Common.CommStatusChangedEvent;
import com.densowave.scannersdk.Const.CommConst;
import com.densowave.scannersdk.Dto.BarcodeScannerSettings;
import com.densowave.scannersdk.Dto.CommScannerParams;
import com.densowave.scannersdk.Dto.RFIDScannerSettings;
import com.densowave.scannersdk.Exception.ErrorCode;
import com.densowave.scannersdk.Listener.BarcodeDataDelegate;
import com.densowave.scannersdk.Listener.RFIDDataDelegate;
import com.densowave.scannersdk.Listener.ScannerAcceptStatusListener;
import com.densowave.scannersdk.Listener.ScannerStatusListener;
import com.densowave.scannersdk.RFID.RFIDData;
import com.densowave.scannersdk.RFID.RFIDDataReceivedEvent;
import com.densowave.scannersdk.RFID.RFIDException;
import com.rfid.Factory.SettingFactory;
import com.rfid.main.InitialActivity;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

import static com.densowave.bhtsdk.barcode.BarcodeScannerInfo.BarcodeScannerType;

/**
 * Densoへの接続用クラス
 * RFIDリーダのRFIDタグ、QR/Barcodeの読取
 * Android端末からのQR/Barcode読取を行います。
 *
 */
public abstract class BaseRFIDActivity extends BaseActivity implements ScannerStatusListener, RFIDDataDelegate, BarcodeDataDelegate, ScannerAcceptStatusListener
        ,BarcodeManager.BarcodeManagerListener, BarcodeScanner.BarcodeDataListener, SystemSettingsLibrary.SystemSettingsListener {

    private boolean scannerConnectedOnCreate = false;

    //RFID オープンフラグ
    public Boolean p_RfidOpenFlag = false;

    //バーコード オープンフラグ
    public Boolean p_BarcodeOpenFlag = false;

    public static CommScanner commScanner;

    //バーコード 読取りハンドラー
    private Handler handler_bar = new Handler();

    //RFID 読取りハンドラー
    private Handler handler_rfid = new Handler();

    //初期化済みフラグ
    protected boolean _InitializedFlag = false;

    //スキャナ再接続フラグ
    public static boolean p_ReconnectFlag = false;

    //Bluetoothローカルネーム
    public static String p_btLocalName = "";

    private boolean _finalSuccessFlag = false;

    protected boolean InitializeActivity = false;   //zebraで使用 20200122 Add Sasaki

    //タイマークラス
    private static Timer _timer;
    private static CountUpTimerTaskMain _timerTask;
    private static Handler handler_timer = new Handler();

    //プログレスダイアログ クラス
    private CommonProgres C_Progres = new CommonProgres();

    //スキャナ接続フラグ
    public static boolean scannerConnected = false;

    //探索ハンドル
    private Handler messageHandler = new Handler();

    //探索ステージ
    private ReadPowerStage readPowerStageOnSearch = null;

    //RSSI 積算配列
    protected ArrayList<Short> _ArrayRssi = new ArrayList<Short>();

    private BarcodeManager mBarcodeManager = null;
    private BarcodeScanner mBarcodeScanner = null;
    private BarcodeScannerType mScannerType = null;

    protected BeepSound beepSound = null;
    protected BeepSoundSearch beepSoundSearch = null;

    protected boolean blnConnectOpenFlag = true;

    private SystemSettingsLibrary mSystemsettingslib = null;

    //カウンタ用
    private Date readStartDate = null;
    private Runnable readTimeViewRefreshAction = null;
    private long readTimeMilliseconds = 0;
    private Handler handler = new Handler();

    private static IntentFilter intentFilter = null;

    //探索一覧用データ
    protected HashMap<String, Short> LinkTagList = new HashMap<String, Short>();

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

    private enum ReadPowerWidth {
        WIDTH_1
        , WIDTH_2
        , WIDTH_3
        , WIDTH_4
        , WIDTH_5
    }
    /**
     * Set CommScanner which is connected
     *
     * @param connectedCommScanner Set CommScanner In case of CommScanner null which is connected, set the CommScanner which is being held to null.
     */
    public void setConnectedCommScanner(CommScanner connectedCommScanner) {
        if (connectedCommScanner != null) {
            scannerConnected = true;
            connectedCommScanner.addStatusListener(this);
        } else {
            scannerConnected = false;
            if (commScanner != null) {
                commScanner.removeStatusListener(this);
            }
        }
        commScanner = connectedCommScanner;
    }

    /**
     * Disconnect SP1
     */
    public void disconnectCommScanner(){
        try {
            if (commScanner != null) {
                //commScanner.close();
                commScanner.removeStatusListener(this);
                scannerConnected = false;
                commScanner = null;
            }
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    /**
     * Get CommScanner
     * Since it is not always connected even if the acquired CommScanner is not null,
     * Use isCommScanner in order to check whether the scanner is connected
     *
     * @return
     */
    public CommScanner getCommScanner() {
        return commScanner;
    }

    /**
     * Createイベント
     * @param savedInstanceState
     */
    @Override
    protected  void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        SQLiteDatabase db = null;

        //機器名称を設定する
        Common.ConnectRFIDName = CommonConst.CONNECT_RFIDNAME.DENSO.getString();

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

                            UpdateAndroidTime(date, time);
                        }
                    };
                    intentFilter = new IntentFilter();
                    intentFilter.addAction("TIME_SETTING");
                    registerReceiver(br, intentFilter);
                }
            } catch (Exception ex) {
            }

            //Android端末の時刻設定
            mSystemsettingslib.create(this, this);

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
            if (blnConnectOpenFlag) {
                //スキャナを切断する
                if (isCommScanner()) {
                    //disconnectCommScanner();
                }
                //CommManager.endAccept();
                //CommManager.removeAcceptStatusListener(this);     //リスナー登録解除
            }

            super.onDestroy();
            if(mSystemsettingslib != null) {
                mSystemsettingslib.destroy();
            }
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    @Override
    protected void onResume() {
        super.onResume();

        //メニューを再表示
        invalidateOptionsMenu();
    }

    @Override
    protected void onPause() {
        super.onPause();
    }

    /**
     * RFIDスキャナに接続された際に呼ばれるイベント
     */
    public void OnScannerAppeared() {}

    /**
     * RFIDスキャナに接続された際に呼ばれるイベント
     * @param commScanner
     */
    @Override
    public void OnScannerAppeared(CommScanner commScanner) {
        boolean successFlag = false;

        Log.i("CONNECT","onScannerAppeared");

        try {
            // Abort the connection request
            commScanner.claim();        //セッション開始

            CommManager.endAccept();    //スキャナからの待ち受け停止
            CommManager.removeAcceptStatusListener(this);     //リスナー登録解除
            setConnectedCommScanner(commScanner);

            successFlag = true;

            _finalSuccessFlag = successFlag;
            p_btLocalName = commScanner.getBTLocalName();

        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }

        if (p_ReconnectFlag) {
            //再接続 メイン画面は表示しない
            return;
        }

        connectReader = true;
        invalidateOptionsMenu();

        this.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                showToastMessage("connect");
                TextView _LabConect = (TextView) findViewById(R.id.labIniConect);
                if (_finalSuccessFlag) {
                    if (_LabConect != null) {
                        _LabConect.setText(p_btLocalName);
                    }

                    //メニュー画面を開く
                    OpenMainActivity();
                }
            }
        });
    }

    protected abstract void OpenMainActivity();

    /**
     * RFIDスキャナとのステータス変更イベント
     * 接続OFF時にイベントを取得する
     *
     * @param scanner Scanner
     * @param state   Status
     */
    public void onScannerStatusChanged(CommScanner scanner, CommStatusChangedEvent state) {

        CommConst.ScannerStatus scannerStatus = state.getStatus();
        if (scanner == commScanner && scannerStatus.equals(CommConst.ScannerStatus.CLOSE_WAIT)) {
            //切断された場合、接続状態のアイコンを切断状態にする
            this.runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    if (scannerConnected) {
                        // Disconnection message display
                        scannerConnected = false;
                        connectReader = false;
                        disconnectCommScanner();
                    }
                }
            });
        }
    }

    /**
     * RFIDリーダーの接続状態
     */
    public boolean isCommScanner() {
        return scannerConnected;
    }

    /**
     * ＳＰ１リーダー接続
     */
    protected void Reader_Connect() {
        try {
            if (!blnConnectOpenFlag) {
                return;
            }
            // Connect it not connected when displaying the screen
            //TextView connectionStatusTextView = (TextView) findViewById(R.id.connection_status);
            TextView _LabConect = findViewById(R.id.labIniConect);
            if (!isCommScanner()) {
                CommManager.addAcceptStatusListener(this);  //リスナーを登録
                CommManager.startAccept();                  //スキャナ待ち受け開始
                // Draw connection status if not connected
                String InfoDisp = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_CONNECT, "", this);
                InfoDisp = InfoDisp.replace("%1", "\n");
                _LabConect.setText(InfoDisp);
            } else {
                if (commScanner != null) {
                    _LabConect.setText(commScanner.getBTLocalName());
                    connectReader = true;
                    invalidateOptionsMenu();
                    showToastMessage("connect");
                    if (beepSound == null ) {
                        beepSound = new BeepSound(this);
                    }
                    beepSound.BeepConnect();
                } else {
                    _LabConect.setText("");
                    connectReader = false;
                    invalidateOptionsMenu();
                    showToastMessage("disconnect");
                }
            }
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * ＳＰ１リーダー再接続
     */
    protected  void Reader_ReConnect() {
        try {
            Log.d("DISCONNECT", "Reader_ReConnect");

            if (!blnConnectOpenFlag) {
                return;
            }
            String strWord = SystemSetting.getString(CommonConst.SYSTEM_KEY_STARTUP, "", this);
            if (strWord.equals(CommonConst.RETURN_STARTUP_VALUE.YES.getString())) {
                return;
            }
            p_ReconnectFlag = true;
            connectReader = false;      //接続状態
            showToastMessage("disconnect");  //初期起動でない場合表示する
            invalidateOptionsMenu();    //メニュー表示
            BatteryRemainDisp();        //バッテリー表示
            onDisconnectEvent();        //各画面の切断処理

            CommManager.addAcceptStatusListener(this);
            CommManager.startAccept();  //スキャナ待ち受け開始

        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
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
     * Start service in the background
     */
    public void startService() {
        if (isCommScanner()) {

            // Check if the service is already started or not
            ActivityManager am = (ActivityManager) this.getSystemService(ACTIVITY_SERVICE);
            List<ActivityManager.RunningServiceInfo> listServiceInfo = am.getRunningServices(Integer.MAX_VALUE);

            for (ActivityManager.RunningServiceInfo curr : listServiceInfo) {
                // If it is up running, do not start the service again
                if (curr.service.getClassName().equals(TaskService.class.getName())) {
                    return;
                }
            }

            // Start service
            Intent intent = new Intent(getApplication(), TaskService.class);
            startService(intent);
            ServiceParam serviceParam = new ServiceParam();
            serviceParam.commScanner = getCommScanner();
            intent.putExtra(InitialActivity.serviceKey, serviceParam);
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
                        if (!scannerConnected && !p_ReconnectFlag ) {
                            //BlueTooth切断 → 再接続
                            Log.i("DISCONNECT", "BlueTooth切断" + this);

                            //再接続
                            Reader_ReConnect();
                        }
                        else if (scannerConnected && p_ReconnectFlag) {
                            Log.i("CONNECT", "BlueTooth再接続" + this);
                            //BlueTooth再接続
                            p_ReconnectFlag = false;

                            //リーダーON
                            ReaderOn();

                            //接続状態をOFF⇒ONに
                            connectReader = true;
                            showToastMessage("connect");
                            invalidateOptionsMenu();

                            OnScannerAppeared();        //各画面個別の接続処理

                            BatteryRemainDisp();        //バッテリー表示
                        }
                    } catch (Exception ex) {
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    }
                }
            });
        }
    }

    /**
     * バーコード取得イベント
     * @param commScanner
     * @param barcodeDataReceivedEvent
     */
    @Override
    public void onBarcodeDataReceived(final CommScanner commScanner, BarcodeDataReceivedEvent barcodeDataReceivedEvent) {
        if (barcodeDataReceivedEvent != null) {
            final List<BarcodeData> barcodeDataList = barcodeDataReceivedEvent.getBarcodeData();
            if (barcodeDataList != null && barcodeDataList.size() > 0) {
                handler_bar.post(new Runnable() {
                    @Override
                    public void run() {
                        for (BarcodeData _barcodeData : barcodeDataList) {
                            //String _text = _barcodeData.getSymbologyDenso() + "(" + _barcodeData.getSymbologyAim() + ")\n";  //バーコード種別
                            String _text = "";
                            try {
                                _text += new String(_barcodeData.getData(), "Shift-JIS");    //バーコードデータ
                            } catch (Exception e) {
                                e.printStackTrace();
                            }

                            //音を出す
                            try {
                                commScanner.buzzer(CommConst.CommBuzzerType.B2);
                            } catch (Exception e) {
                                e.printStackTrace();
                            }

                            //サブクラスで実装する
                            BarcodeDataReceived(_text);

                        }
                    }
                });
            }
        }
    }

    /**
     * Androidバーコード受信イベント
     * バーコードマネージャ作成イベント
     * @param barcodeManager
     */
    @Override
    public void onBarcodeManagerCreated(BarcodeManager barcodeManager) {
        // When barcode scanner manager created.
        mBarcodeManager = barcodeManager;
        try {
            List<BarcodeScanner> listScanner = mBarcodeManager.getBarcodeScanners();
            if (listScanner.size() > 0) {
                // Get BarcodeScanner instance
                mBarcodeScanner = listScanner.get(0);

                if (mScannerType == null) {
                    mScannerType = mBarcodeScanner.getInfo().getType();
                }

                if (mBarcodeScanner != null) {
                    // Register Data Received event
                    mBarcodeScanner.addDataListener(this);

                    // Setting for Scanner
                    CommonBarCode C_ComBarCode = new CommonBarCode();
                    C_ComBarCode.p_qrCode_enabled = true;   //ＱＲコード読取り
                    C_ComBarCode.p_codabar_enabled = true;
                    C_ComBarCode.p_code39_enabled = true;
                    C_ComBarCode.p_code93_enabled = true;
                    C_ComBarCode.p_code128_enabled = true;
                    C_ComBarCode.p_dataMatrix_enabled = true;
                    C_ComBarCode.p_ean8_enabled = true;
                    C_ComBarCode.p_ean13UpcA_enabled = true;
                    C_ComBarCode.p_gs1DataBar_enabled = true;
                    C_ComBarCode.p_itf_enabled = true;
                    C_ComBarCode.setScanner(mBarcodeScanner, mScannerType);

                    // Enable Scanner
                    mBarcodeScanner.claim();
                }
            }
        }
        catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * Androidバーコード受信イベント
     * @param event
     */
    @Override
    public void onBarcodeDataReceived(com.densowave.bhtsdk.barcode.BarcodeDataReceivedEvent event) {
        try {

            List<com.densowave.bhtsdk.barcode.BarcodeDataReceivedEvent.BarcodeData> listBarcodeData = event.getBarcodeData();
            for (com.densowave.bhtsdk.barcode.BarcodeDataReceivedEvent.BarcodeData barcodeData : listBarcodeData) {
                String aimSymbology = barcodeData.getSymbologyAim();        //コード種類(AIM準拠)
                String densoSymbology = barcodeData.getSymbologyDenso();    //コード種類(DENSO規定))
                String ReadData = barcodeData.getData();                    //エンコード済取得データ

                runOnUiThread(new Runnable() {
                    String _Aim;
                    String _Denso;
                    String _ReadData;
                    Runnable setBarcodeData(String pAim, String pDenso, String pReadData) {
                        this._Aim = pAim;
                        this._Denso = pDenso;
                        this._ReadData = pReadData;
                        return this;
                    }
                    @Override
                    public void run() {
                        if(this._ReadData.length() > 0) {
                            //サブクラスで実装する
                            BarcodeDataReceived(_ReadData);
                        }
                    }
                }.setBarcodeData(aimSymbology, densoSymbology, ReadData));
            };
        }
        catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * RFIDタグスキャナ読取りイベント
     * @param scanner
     * @param rfidDataReceivedEvent
     */
    @Override
    public void onRFIDDataReceived(CommScanner scanner, final RFIDDataReceivedEvent rfidDataReceivedEvent) {
        handler_rfid.post(new Runnable() {
            @Override
            public void run() {
                //RFID情報 件数分ループ
                for (int i = 0; i < rfidDataReceivedEvent.getRFIDData().size(); i++) {
                    RFIDData eachRFID = rfidDataReceivedEvent.getRFIDData().get(i);
                    byte[] _uii = rfidDataReceivedEvent.getRFIDData().get(i).getUII();   //UIIを取得

                    String _data = "";
                    for (int loop = 0; loop < _uii.length; loop++) {
                        _data += String.format("%02X ", _uii[loop]).trim();     //16進化 結合
                    }

                    short tempRSSI = (short) eachRFID.getRSSI();                //RSSI値を取得
                    String strPcHex = Integer.toHexString(rfidDataReceivedEvent.getRFIDData().get(i).getPC());

                    //サブクラスで実装する
                    RFIDDataReceived(_data, tempRSSI, strPcHex);
                }
            }
        });
    }

    // RFID リーダー初期化
    private void RFID_Read_Initialize() {
        //p_RfidOpenFlag = false;
        scannerConnectedOnCreate = isCommScanner();

        if (scannerConnectedOnCreate) {
            //RFID読み取り完了 delegate登録
            getCommScanner().getRFIDScanner().setDataDelegate(this);

            //カウンタスタート
            ReadTimeStart();
        }

        // Service is started in the back ground.
        startService();

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
        if (!p_RfidOpenFlag) {
            try {
                String ReadPass = "00000000";
                short ReadStart = 0;
                byte[] Checkbytes = hexStringToBytes(prmCheckUII);

                //RFID 設定 読取り通知音-OFF
                setScannerSettings(false, prmIntencity, prmSession, prmQValue, true, prmAroundWave, false, false,true);

                //UII指定 openRead
                if (isCommScanner()) {
                    getCommScanner().getRFIDScanner().openRead(RFIDScannerSettings.RFIDBank.UII, ReadStart, (short) Checkbytes.length, stringToByte(ReadPass), Checkbytes);
                }
                p_RfidOpenFlag = true;

                //ブザーON
                //commScanner.buzzer(CommConst.CommBuzzerType.B2);
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
        if (!p_RfidOpenFlag) {
            try {
                String ReadPass = "00000000";
                short ReadStart = 0;
                byte[] Checkbytes = hexStringToBytes(prmCheckUII);

                //RFID 設定 読取り通知音-OFF
                setScannerSettings(false, prmIntencity, prmSession, prmQValue, true, prmAroundWave, false, false,true);

                //トリガー設定
                setScannerSettingsNoTrigger(false);

                //UII指定 openRead
                if (isCommScanner()) {
                    getCommScanner().getRFIDScanner().openRead(RFIDScannerSettings.RFIDBank.UII, ReadStart, (short) Checkbytes.length, stringToByte(ReadPass), Checkbytes);
                }
                p_RfidOpenFlag = true;

                //ブザーON
                //commScanner.buzzer(CommConst.CommBuzzerType.B2);
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
        if (!p_RfidOpenFlag) {
            try {
                if (prmSettingNeed) {   //ADD 2019/03/26
                    //RFID 設定 読取り通知音ON
                    setScannerSettings(false, prmIntencity, prmSession, prmQValue, prmMultchRd, prmAroundWave, prmDoubleInhibit, false,false);
                }

                p_RfidOpenFlag = true;

                if (isCommScanner()) {
                    getCommScanner().getRFIDScanner().openInventory();
                }
                //ブザーON
                //commScanner.buzzer(CommConst.CommBuzzerType.B2);
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
            strRead = String.valueOf(LinkTagList.get(strTagID) / 10.0f);
        }

        return strRead;
    }

    /**
     * Set up the scanner
     *
     * @param sessionInit      セッション初期化
     * @param prmIntencity     電波強度
     * @param prmSession       セッション
     * @param prmQValue        Ｑバリュー
     * @param prmMultchRd      マルチ読取り(=true)/シングル読取り(=false)
     * @param prmAroundWave    垂直・水平読取り(=true)/水平読取り(=false)  ADD 2019/03/12
     * @param prmDoubleInhibit 二度読み防止ON(=true)/OFF(=false)  ADD 2019/03/12
     * @param prmReadSound     読取り通知音(=true)/OFF(=false)  ADD 2019/03/14
     * @param searchMode       探索時の呼び出しか
     * @throws CommException Exception concerning CommScanner
     * @throws RFIDException Exception concerning RFIDScanner
     */
    private void setScannerSettings(@Nullable Boolean sessionInit, int prmIntencity, String prmSession, short prmQValue,
                                    boolean prmMultchRd, boolean prmAroundWave, boolean prmDoubleInhibit, boolean prmReadSound, boolean searchMode) throws CommException, RFIDException {
        try {
            if (!isCommScanner()) {
                return;
            }

            //セッション値
            @Nullable RFIDScannerSettings.Scan.SessionFlag sessionFlag = null;
            switch (prmSession) {
                case "0":
                    sessionFlag = RFIDScannerSettings.Scan.SessionFlag.S0;
                    break;
                case "1":
                    sessionFlag = RFIDScannerSettings.Scan.SessionFlag.S1;
                    break;
                case "2":
                    sessionFlag = RFIDScannerSettings.Scan.SessionFlag.S2;
                    break;
                case "3":
                    sessionFlag = RFIDScannerSettings.Scan.SessionFlag.S3;
                    break;
            }

            // Acquire only the configuration values from the existing ones
            RFIDScannerSettings _RFIDsettings = getCommScanner().getRFIDScanner().getSettings();

            if (prmDoubleInhibit) {
                //二度読み防止あり ADD 2019/03/12
                _RFIDsettings.scan.doubleReading = RFIDScannerSettings.Scan.DoubleReading.PREVENT2;
            } else {
                //二度読み防止 なし
                _RFIDsettings.scan.doubleReading = RFIDScannerSettings.Scan.DoubleReading.Free;
            }

            if (prmMultchRd) {
                //トリガモード モメンタリモード
                _RFIDsettings.scan.triggerMode = RFIDScannerSettings.Scan.TriggerMode.MOMENTARY;
                //_RFIDsettings.scan.triggerMode = RFIDScannerSettings.Scan.TriggerMode.CONTINUOUS1;
            } else {
                //トリガモード  AUTO　OFF
                _RFIDsettings.scan.triggerMode = RFIDScannerSettings.Scan.TriggerMode.AUTO_OFF;
            }

            // Set 電波強度
            _RFIDsettings.scan.powerLevelRead = prmIntencity;

            if (prmAroundWave) {
                // Set Polarization to Horizon & Vertical   ADD 2019/03/12
                _RFIDsettings.scan.polarization = RFIDScannerSettings.Scan.Polarization.Both;
            } else {
                // Set Polarization to Horizon
                _RFIDsettings.scan.polarization = RFIDScannerSettings.Scan.Polarization.H;
            }

            if (sessionFlag != null) {
                _RFIDsettings.scan.sessionFlag = sessionFlag;    //セッション値
            }

            if (sessionInit != null) {
                _RFIDsettings.scan.sessionInit = sessionInit;
            }

            _RFIDsettings.scan.qParam = prmQValue;     //Ｑバリュー値

            getCommScanner().getRFIDScanner().setSettings(_RFIDsettings);

            //CommScannerParamsクラス
            CommScannerParams _commParams = getCommScanner().getParams();

            //読み取り通知音 ADD 2019/03/14
            if (prmReadSound) {
                _commParams.notification.sound.buzzer = CommScannerParams.Notification.Sound.Buzzer.ENABLE;
            } else {
                _commParams.notification.sound.buzzer = CommScannerParams.Notification.Sound.Buzzer.DISABLE;
            }
            //20220316 ADD Suzuki V2.1 START
            //読取り精度向上(SP1のファームバージョンが1.14より低い場合、例外が発生)
            if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
                //DENSOの場合
                Boolean blnAutoLink = Boolean.valueOf(C_Setting.getBTAutoLinkProfile( this, C_Setting.KEY_BT_READ_AUTO_LINK));
                getCommScanner().getRFIDScanner().setAutoLinkProfile(blnAutoLink);
            }
            //20220316 ADD Suzuki V2.1 END

            getCommScanner().setParams(_commParams);
        }
        //20220316 ADD Suzuki V2.1 START
        catch (RFIDException ex) {
            if(ex.getErrorCode().equals(ErrorCode.NOT_SUPPORT_COMMAND) &&
                Boolean.valueOf(C_Setting.getBTAutoLinkProfile( this, C_Setting.KEY_BT_READ_AUTO_LINK)) &&
                !searchMode){
                //「例外が発生⇒ファームバージョンが1.14より低い」かつ「AutoLinkProfileの設定が有効」かつ
                //「AutoLinkProfileを利用するopenの仕方(openInventory)である⇒探索以外の処理」
                ReaderOff();
                showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_FW_VER , "", this));
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            }
        }
        //20220316 ADD Suzuki V2.1 END
        catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * トリガー設定
     * @param blnTrigger
     */
    private void setScannerSettingsNoTrigger(boolean blnTrigger) {
         try {
             if (!isCommScanner()) {
                 return;
             }

             RFIDScannerSettings _RFIDsettings = getCommScanner().getRFIDScanner().getSettings();

            if (blnTrigger) {
                //トリガモードを引く
                _RFIDsettings.scan.triggerMode = RFIDScannerSettings.Scan.TriggerMode.MOMENTARY;
            } else {
                //トリガモードを引かず読取
                _RFIDsettings.scan.triggerMode = RFIDScannerSettings.Scan.TriggerMode.CONTINUOUS1;
            }
            getCommScanner().getRFIDScanner().setSettings(_RFIDsettings);
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    // RFID リーダーＯＦＦ
    public void subRFID_Read_Stop() {
        if (p_RfidOpenFlag) {
            if (!isCommScanner()) {
                //既に切断
                p_RfidOpenFlag = false;
            } else {
                try {
                    if (isCommScanner()) {
                        getCommScanner().getRFIDScanner().close();
                    }
                    p_RfidOpenFlag = false;

                    //ブザーON
                    //buzzer_B3();
                } catch (RFIDException re) {
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(re));
                }
            }
        }
    }

    //二度読みバッファをクリア
    public void subClearDuplexBuffer() {
        try {
            if (isCommScanner()) {
                getCommScanner().getRFIDScanner().clearDoubleReadingBuffer();
            }
        } catch (RFIDException re) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(re));
        }
    }

    /**
     * バーコード リーダーＯＮ
     *
     * @param prmMultchRd マルチ読取り(=true)/シングル読取り(=false)
     */
    public boolean fncBarcode_Read_Start(boolean prmMultchRd) {
        if (!p_BarcodeOpenFlag) {
            try {
                if (!isCommScanner()) {
                    return false;
                }
                // For barcode settings, the default value excluding the barcode type (value set in the device) = not set anything
                BarcodeScannerSettings _barcodeSettings = getCommScanner().getBarcodeScanner().getSettings();

                if (prmMultchRd) {
                    //トリガモード モメンタリモード
                    _barcodeSettings.scan.triggerMode = BarcodeScannerSettings.Scan.TriggerMode.MOMENTARY;
                } else {
                    //トリガモード  AUTO　OFF
                    _barcodeSettings.scan.triggerMode = BarcodeScannerSettings.Scan.TriggerMode.AUTO_OFF;
                }

                //バーコード種類設定
                _barcodeSettings.decode.symbologies.ean13upcA.enabled = true;           // EAN-13 UPC-A
                _barcodeSettings.decode.symbologies.ean8.enabled = true;                 // EAN-8
                _barcodeSettings.decode.symbologies.upcE.enabled = true;                 // UPC-E
                _barcodeSettings.decode.symbologies.itf.enabled = true;                  // ITF
                _barcodeSettings.decode.symbologies.stf.enabled = true;                  // STF
                _barcodeSettings.decode.symbologies.codabar.enabled = true;              // Codabar
                _barcodeSettings.decode.symbologies.code39.enabled = true;               // Code39
                _barcodeSettings.decode.symbologies.code93.enabled = true;               // Code93
                _barcodeSettings.decode.symbologies.code128.enabled = true;              // Code128
                _barcodeSettings.decode.symbologies.msi.enabled = true;                   // MSI
                _barcodeSettings.decode.symbologies.gs1DataBar.enabled = true;           // GS1 Databar
                _barcodeSettings.decode.symbologies.gs1DataBarLimited.enabled = true;   // GS1 Databar Limited
                _barcodeSettings.decode.symbologies.gs1DataBarExpanded.enabled = true;  // GS1 Databar Expanded

                _barcodeSettings.decode.symbologies.qrCode.enabled = true;           // QR Code
                _barcodeSettings.decode.symbologies.qrCode.model1.enabled = true;   // QR Code.Model1
                _barcodeSettings.decode.symbologies.qrCode.model2.enabled = true;   // QR Code.Model2
                _barcodeSettings.decode.symbologies.microQr.enabled = true;          // QR Code.Micro QR

                _barcodeSettings.decode.symbologies.iqrCode.enabled = false;                 // iQR Code
                _barcodeSettings.decode.symbologies.iqrCode.square.enabled = false;         // iQR Code.Square
                _barcodeSettings.decode.symbologies.iqrCode.rectangle.enabled = false;     // iQR Code.Rectangle
                _barcodeSettings.decode.symbologies.dataMatrix.enabled = false;             // Data Matrix
                _barcodeSettings.decode.symbologies.dataMatrix.square.enabled = false;      // Data Matrix.Square
                _barcodeSettings.decode.symbologies.dataMatrix.rectangle.enabled = false;  // Data Matrix.Rectangle
                _barcodeSettings.decode.symbologies.pdf417.enabled = false;                  // PDF417
                _barcodeSettings.decode.symbologies.microPdf417.enabled = false;            // Micro PDF417
                _barcodeSettings.decode.symbologies.maxiCode.enabled = false;               // Maxi Code
                _barcodeSettings.decode.symbologies.gs1Composite.enabled = false;          // GS1 Composite
                _barcodeSettings.decode.symbologies.plessey.enabled = false;                // Plessey
                _barcodeSettings.decode.symbologies.aztec.enabled = false;                  // Aztec

                //バーコード設定保存
                getCommScanner().getBarcodeScanner().setSettings(_barcodeSettings);

                // Start scanning
                getCommScanner().getBarcodeScanner().openReader();
                p_BarcodeOpenFlag = true;

                //ブザーON
                RFIDSoundBuzzer(true);
                //commScanner.buzzer(CommConst.CommBuzzerType.B2);
                return true;

            } catch (Exception ex) {
                return false;
            }
        } else {
            return true;
        }
    }

    // バーコード リーダーＯＦＦ
    public void subBarcode_Read_Stop() {
        try {
            if (p_BarcodeOpenFlag) {
                if (!isCommScanner()) {
                    //既に切断
                    p_BarcodeOpenFlag = false;
                } else {
                    if (isCommScanner()) {
                        getCommScanner().getBarcodeScanner().closeReader();
                    }
                    p_BarcodeOpenFlag = false;

                    //ブザーOFF
                    RFIDSoundBuzzer(false);
                    //buzzer_B3();
                }
            }
        } catch (Exception be) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(be));
        }
    }

    //バーコード リーダー初期化
    private void Barcode_Read_Initialize() {

        p_BarcodeOpenFlag = false;
        scannerConnectedOnCreate = isCommScanner();

        if (scannerConnectedOnCreate) {
            //バーコード読み取り完了 delegate登録
            if (isCommScanner()) {
                getCommScanner().getBarcodeScanner().setDataDelegate(this);
            }
        }

        // Service is started in the back ground.
        startService();

    }

    //バーコード リーダーＯＮ
    protected void Barcode_Read_Start() {

        //対象リーダー取得する
        SettingValue setting = new SettingValue();
        if (CommonConst.READER_SETTING.Reader.getString().equals(setting.getReaderChange(this))) {
            //RFIDリーダを使用する場合
            Barcode_Read_Initialize();
            if (scannerConnectedOnCreate) {
                fncBarcode_Read_Start(false);
            }

            //Androidを停止する
            try {
                if (mBarcodeManager != null) {
                    // Disable Scanner
                    mBarcodeScanner.close();
                    // Remove Listener Event
                    mBarcodeScanner.removeDataListener(this);

                    // Remove Scanner Manager
                    mBarcodeManager.destroy();
                    mBarcodeManager = null;
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        } else {
            //Androidを使用する場合
            try {
                // Create BarcodeManager
                BarcodeManager.create(this, this);

                //RFIDリーダーを停止する
                if (scannerConnectedOnCreate) {
                    subBarcode_Read_Stop();
                }
            } catch (Exception ex) {
                // Set to default if Wrong Parameters has been reloaded.
                mScannerType = null;
            }
        }

        p_BarcodeOpenFlag = true;
    }

    // バーコード リーダーＯＦＦ
    protected void Barcode_Read_Stop() {

        //対象リーダー取得する
        SettingValue setting = new SettingValue();
        if (CommonConst.READER_SETTING.Reader.getString().equals(setting.getReaderChange(this))) {
            //RFIDリーダを使用する場合
            if (scannerConnectedOnCreate) {
                subBarcode_Read_Stop();
            }
        } else {
            //Androidを使用する場合
            try {
                if (mBarcodeManager != null) {
                    // Disable Scanner
                    mBarcodeScanner.close();
                    // Remove Listener Event
                    mBarcodeScanner.removeDataListener(this);

                    // Remove Scanner Manager
                    mBarcodeManager.destroy();
                    mBarcodeManager = null;
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        }

        p_BarcodeOpenFlag = false;
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
                //RFID リーダー初期化
                RFID_Read_Initialize();

                if (scannerConnectedOnCreate) {
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
                            , true, true, true, parmChange);
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
                //RFID リーダー初期化
                RFID_Read_Initialize();

                if (scannerConnectedOnCreate) {
                    fncRFID_Read_Start(Integer.valueOf(C_Setting.getBTReadIntensity(activity, strKey)),
                            C_Setting.getBTReadSession(activity, strKey),
                            Short.valueOf(C_Setting.getBTReadQValue(activity, strKey))
                            , false, true, true, parmChange);
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
        if (blnConnectOpenFlag) {
            if (scannerConnectedOnCreate) {
                subClearDuplexBuffer();  //二度読みバッファクリア
                subRFID_Read_Stop();

                //タイマーカウントストップ
                ReadTimeStop();

                //音を停止
                if (beepSound != null) {
                    beepSound.Destory();
                    beepSound = null;
                }

                if (isCommScanner()) {
                    getCommScanner().getRFIDScanner().setDataDelegate(null);
                }
            }
        } else {
            disconnectCommScanner();
        }
    }

    /**
     * 探索開始
     * @param strAssetID
     * @param strKey
     */
    protected void Rfid_Search_Start(String strAssetID, String strKey){
        try {
            RFID_Read_Initialize();

            if (scannerConnectedOnCreate) {
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
            RFID_Read_Initialize();

            if (scannerConnectedOnCreate) {
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
            if (scannerConnectedOnCreate) {
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
                    readPowerLevel = readPowerLevel / _ArrayRssi.size() / 10.0f;
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
    protected void WriteTag(String strTag, String strNewTag, String strOldPc, int newEpcBitSize) throws RFIDException, CommException{
        String ReadPass = CommonConst.TAG_DEFAULT_ACCESS_PWD;
        short ReadStart = CommonConst.EPC_BYTE_TYPE_START_POS;

        String strNewPc = "";
        //PCの書換が必要かどうかをチェック
        if (Common.needOverWritePc(newEpcBitSize, strOldPc)) {
            //PC書換あり
            strNewPc = Common.calcTagPcHex(newEpcBitSize, strOldPc);
            ReadStart = CommonConst.PC_BYTE_TYPE_START_POS;
        }

        byte[] SearchData = hexStringToBytes(strTag);
        byte[] WriteData = hexStringToBytes(strNewPc + strNewTag);
        RFID_Read_Initialize();

        //RFID 設定 読取り通知音 ON、トリガーモード AutoOff
        setScannerSettings(false, Integer.valueOf(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_TAGLINK)),
                C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_TAGLINK),
                Short.valueOf(C_Setting.getBTReadQValue(this, C_Setting.KEY_EACH_TAGLINK)),
                false, true, false, false,false);
        if (isCommScanner()) {
            getCommScanner().getRFIDScanner().writeOneTag(RFIDScannerSettings.RFIDBank.UII, ReadStart, (short) WriteData.length, stringToByte(ReadPass), WriteData, SearchData, 30000);
        }

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

            int stage2MaxReadPowerLevel = getResources().getInteger(R.integer.stage2_max_read_power_level_on_search);
            int stage3MaxReadPowerLevel = getResources().getInteger(R.integer.stage3_max_read_power_level_on_search);
            int stage4MaxReadPowerLevel = getResources().getInteger(R.integer.stage4_max_read_power_level_on_search);
            int stage5MaxReadPowerLevel = getResources().getInteger(R.integer.stage5_max_read_power_level_on_search);
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
     * Set display of circle according to the stage of Read output value
     * @param readPowerStage The stage of the Read output value that will be the basis of the circular display  Do not display if null is specified
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

    //電池残量表示
    protected void BatteryRemainDisp() {
        try {
            int _BatteryRemain = 0;
            if (connectReader) {
                _BatteryRemain = GetBatteryRemain();
            }

            int batteryResId = 0;
            switch(_BatteryRemain) {
                case 1:
                    batteryResId = R.mipmap.battery_1;
                    break;
                case 2:
                    batteryResId = R.mipmap.battery_2;
                    break;
                case 3:
                    batteryResId = R.mipmap.battery_full;
                    break;
                default:
                    batteryResId = R.mipmap.battery_0;
                    break;
            }

            //電池残量が少ない場合、警告メッセージを表示する
            if (_BatteryRemain == 1) {
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
     * SP1 電池残量取得
     *
     * @return 0:残なし、1:10%未満、2:40%未満、3:40%以上
     */
    public int GetBatteryRemain() {
        try {
            CommConst.CommBattery _battery = getCommScanner().getRemainingBattery();
            int _AnsValue = 0;

            switch (_battery) {
                case UNDER10:
                    _AnsValue = 1;
                    break;

                case UNDER40:
                    _AnsValue = 2;
                    break;

                case OVER40:
                    _AnsValue = 3;
                    break;
            }
            return _AnsValue;
        } catch (Exception e) {
            return 0;
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

            // 日付を作成する
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
            Date dateTo = null;
            try {
                //現在日時
                String strDate = strGetDate.substring(0, 4) + "/" + strGetDate.substring(4, 6) + "/" + strGetDate.substring(6);
                String strTime = strGetTime.substring(0, 2) + ":" + strGetTime.substring(2, 4) + ":" + strGetTime.substring(4, 6);
                dateTo = sdf.parse(strDate + " " + strTime);
            } catch (ParseException e) {
                e.printStackTrace();
            }

            // 日付をlong値に変換する
            long dateTimeTo = dateTo.getTime();

            //日時を設定する
            mSystemsettingslib.setCurrentTimeMillis(dateTimeTo);
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
        return true;
    }


    /**
     * 時刻設定Create
     * @param systemsettinglib
     */
    public void onSystemSettingsCreated(SystemSettingsLibrary systemsettinglib) {
        mSystemsettingslib = systemsettinglib;
    };

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

    /**
     * Convert from hexadecimal string to byte list
     *
     * @param hexString The hexadecimal string
     * @return The byte list based on the hexadecimal string
     */
    private static byte[] hexStringToBytes(String hexString) {
        // Element 0 in case of null character
        if (hexString.length() == 0) {
            return new byte[0];
        }

        // Cut out hexadecimal character string in byte unit and store it in the list
        // 1 byte is equivalent to 2 hexadecimal characters, so cut out 2 characters at a time
        // In order to cut out 2 characters at a time irrespective of the length of the character string, add 0 to the beginning if the character string is odd length
        String workHexString = hexString.length() % 2 == 0 ? hexString : "0" + hexString;
        byte[] bytes = new byte[workHexString.length() / 2];
        for (int i = 0; i < bytes.length; i++) {
            // If leave Byte.parseByte as it is, overflow will occur when the value becomes larger than 0x80
            // By parsing it to a larger type then casting it to byte, a value larger than 0x80 can be input as a negative value
            String hex2Characters = workHexString.substring(i * 2, i * 2 + 2);

            short number = Short.parseShort(String.format("%s", hex2Characters), 16);
            bytes[i] = (byte) number;
        }
        return bytes;
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

    protected void buzzer_B3() {
        try {
            commScanner.buzzer(CommConst.CommBuzzerType.B3);
        } catch (Exception ex) {
            Log.d(CommonConst.DIALOG_ERROR_TITLE, ex.getMessage());
        }
    }

    protected boolean ConnectStatus(){
        return _finalSuccessFlag;
    }

    /**
     * RFIDの読取音のON/OFFを制御する
     * @param blnBuzzerOn
     */
    private void RFIDSoundBuzzer(boolean blnBuzzerOn) {

        try {
            //CommScannerParamsクラス
            CommScannerParams _commParams = getCommScanner().getParams();

            //読み取り通知音 ON/OFF
            if (blnBuzzerOn) {
                _commParams.notification.sound.buzzer = CommScannerParams.Notification.Sound.Buzzer.ENABLE;
            } else {
                _commParams.notification.sound.buzzer = CommScannerParams.Notification.Sound.Buzzer.DISABLE;
            }

            getCommScanner().setParams(_commParams);
        } catch (Exception e) {
            Log.d(CommonConst.DIALOG_ERROR_TITLE, e.getMessage());
        }
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
    protected abstract void RFIDDataReceived(String strReadData, short tempRSSI, final String strPcHex);

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
