package com.rfid.common;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.os.Handler;
import androidx.annotation.Nullable;

import android.provider.Settings;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.ImageView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;
import com.zebra.rfid.api3.BEEPER_VOLUME;
import com.zebra.rfid.api3.ENUM_NEW_KEYLAYOUT_TYPE;
import com.zebra.rfid.api3.ENUM_TRIGGER_MODE;
import com.zebra.rfid.api3.Events;
import com.zebra.rfid.api3.InvalidUsageException;
import com.zebra.rfid.api3.OperationFailureException;
import com.zebra.rfid.api3.TagData;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.Timer;
import java.util.TimerTask;

/**
 * Zebraへの接続用クラス
 * RFIDリーダのRFIDタグ、QR/Barcodeの読取
 * Android端末からのQR/Barcode読取を行います。
 *
 */
public abstract class BaseRFIDActivity extends BaseActivity implements RFIDHandler.ResponseHandlerInterface, BarcodeHandler.ResponseHandlerInterface, CommonHtDateCheckDialog.Callback {

    protected boolean disconnectFlag = false;

    //スキャナ再接続フラグ
    public static boolean p_ReconnectFlag = false;

    private boolean _finalSuccessFlag = false;

    //RSSI 積算配列
    protected ArrayList<Short> _ArrayRssi = new ArrayList<Short>();

    //RFIDライブラリ
    public static  RFIDHandler rfidHandler;

    //プログレスダイアログ クラス
    private CommonProgres C_Progres = new CommonProgres();

    //タイマークラス
    private static Timer _timer = null;
    private static CountUpTimerTaskMain _timerTask = null;
    private static Handler handler_timer = new Handler();

    protected boolean InitializeActivity = false;

    //探索ハンドル
    private Handler messageHandler = new Handler();

    //探索レベル
    private Float readPowerLevelOnSearch = null;

    //探索ステージ
    private ReadPowerStage readPowerStageOnSearch = null;

    //システム設定値
    protected SettingValue C_Setting = new SettingValue();

    private Events.BatteryData _Battery = null;

    private String _IntensityKey = "";

    protected BeepSound beepSound = null;
    protected BeepSoundSearch beepSoundSearch = null;

    private String _TagID = null;
    private int _ScannerID = 0;
    private boolean blnTagSearch = false;

    private boolean ConnectMessageDisable = false;
    private boolean DisconnectBarcodeFlag = false;

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

    public enum TagPopulation {
        MIN("30"),
        MAX("600");
        private final String text;
        private TagPopulation(final String text) {
            this.text = text;
        }
        public final String getString() {
            return this.text;
        }
    }

    /**
     * TOP-Activity
     */
    final static String TAG = "RFID_AM";

    public static BarcodeHandler barcode;

    public static BarcodeZebraHandler barcodezebra;

    protected abstract void OpenMainActivity();

    protected boolean blnConnectOpenFlag = true;

    private int REFRESH = 1;
    private int EVENT = 2;
    private int CREATE = 3;

    private static IntentFilter intentFilter = null;


    //DENSO用(ZEBRAでは使用せず)
    protected HashMap<String, Short> LinkTagList = new HashMap<String, Short>();

    //ダイアログコード
    private static final int DIALOG_HT_DATE = 2001;
    private static final int DIALOG_ERR = 2002;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        SQLiteDatabase db = null;

        //機器名称を設定する
        Common.ConnectRFIDName = CommonConst.CONNECT_RFIDNAME.ZEBRA.getString();

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

            //20200122 Add Start Sasaki
            //リーダー接続の場合、再接続する
            if (blnConnectOpenFlag) {
                if (rfidHandler == null) {
                    rfidHandler = new RFIDHandler();
                    rfidHandler.onCreate(this);
                } else {
                    Thread.sleep(1000);
                    if (!rfidHandler.isReaderConnected()) {
                        rfidHandler.removeEventsListener();
                        rfidHandler.onPause();
                        rfidHandler = null;
                        DisconnectBarcode();
                        barcode = null;
                        rfidHandler = new RFIDHandler();
                        rfidHandler.onCreate(this);
                    }
                }
            }
            //20200122 Add End Sasaki

        } catch (Exception e){
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        } finally {
            if(db != null){
                db.close();
            }
        }

    }

    protected void onResume() {
        super.onResume();

        //タイマーストップ
        //StopTimer();      20191016 Del Sasaki

        //接続状態を再更新する
        invalidateOptionsMenu();    //20191031 Add Sasaki

        initializeReader();

        String strConnect = "";
        if (blnConnectOpenFlag) {
            final Activity activity = this;

            try {
                //if (isCommScanner()) {
                if (isCommScanner()) {
                    //接続に成功した場合
                    OnScannerAppeared();
                } else {
                    //20191016 Del Start Sasaki
                    /*//再接続開始された場合、メニューを更新する
                    if("".equals(rfidHandler.connect())){
                        //接続に成功した場合
                        OnScannerAppeared();
                        if (beepSound == null ) {
                            beepSound = new BeepSound(activity);
                        }
                        beepSound.BeepConnect();
                    }*/
                    //20191016 Del Start Sasaki
                }
            } catch (Exception e) {
                e.printStackTrace();
            }

        }
    }

    @Override
    protected void onPause() {
        try {
            super.onPause();
            if (barcode != null) {
                barcode.onPause();
            }
            new Thread() {
                @Override
                public void run() {
                    if (blnConnectOpenFlag) {
                        if (connectReader && rfidHandler != null) {
                            rfidHandler.stopInventory();
                        }
                    }
                }
            }.start();
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
    }

    /**
     * リーダー接続
     */
    protected void Reader_Connect() {
        //メニュー画面を開く
        OpenMainActivity();
    }

    /**
     * リーダー再接続
     */
    public void Reader_ReConnect() {
        try {
            p_ReconnectFlag = true;
            disconnectFlag = false;

            if (connectReader) {
                return;
            }

            if (rfidHandler != null) {
                //rfidHandler.connect(); 20200116 Del Sasaki
            }

            //接続状態メニューを更新する
            connectReader = true;
            disconnectFlag = false;
            invalidateOptionsMenu();
            showToastMessage("connect");

            //ボタン表示
            AllResetButton(true);

            //20191016 Add Start Sasaki
            //接続に成功した場合
            OnScannerAppeared();
            if (beepSound == null ) {
                beepSound = new BeepSound(activity);
            }
            beepSound.BeepConnect();

            //起動画面の場合、メニュー画面に遷移する
            _finalSuccessFlag = true;
            OpenMainActivity();
            //20191016 Add End Sasaki
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    /**
     * 再接続時の処理 //20200121 Add Sasaki
     */
    public void ReConnect() {
        try {
            //リーダー接続時の場合
            if (blnConnectOpenFlag) {
                //起動画面の場合、再接続する
                if (InitializeActivity) {
                    if (rfidHandler != null) {
                        ConnectMessageDisable = true;
                        DisconnectBarcodeFlag = true;
                        rfidHandler.reConnect();
                    }
                }

                //Barcodeを初期化した後、リーダーをCreateする(QRが正常に読めないため)
                if (rfidHandler == null) {
                    ConnectMessageDisable = true;
                    DisconnectBarcode();
                    barcode = null;
                    rfidHandler = new RFIDHandler();
                    rfidHandler.onCreate(this);
                }
            }
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
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
     * サービス開始(Zebraでは使用しない)
     */
    public void startService() {
    }

    /**
     * タイマー開始
     */
    protected void StartTimer() {
        //Readerを使用しない場合、終了
        if (!blnConnectOpenFlag){
            return;
        }

        //20191016 Add Start Sasaki
        //切断されている場合、タイマーは起動しない
        if (!connectReader) {
            return;
        }
        //20191016 Add End Sasaki

        // Timer インスタンスを生成
        if (_timer == null) {
            _timer = new Timer();
            _timerTask = new CountUpTimerTaskMain();

            // スケジュールを設定 1秒
            _timer.schedule(_timerTask, 0, 1000);
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
    }

    /**
     * タイマータスク
     */
    final Activity activity = this;
    private class CountUpTimerTaskMain extends TimerTask {
        @Override
        public void run() {
            // handlerを使って処理をキューイングする
            handler_timer.post(new Runnable() {
                public void run() {
                    //ここで処理時間の長い処理を実行する
                    try {
                        //既に切断済みの場合終了
                        if (!connectReader && disconnectFlag) {
                            return;
                        }
                        if (rfidHandler != null) {
							/* 20200121 Del Sasaki
                            //接続状態を取得する
                            boolean blnRet = rfidHandler.BluetoothConnected();

                            //切断されていた場合、切断処理を行う
                            if (!blnRet) {
                                //onDisconnectEvent(); 20200116 Del sasaaki
                            } */
                        }
                    } catch (Exception ex) {
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    }
                }
            });
        }
    }

    private void initializeReader() {

        try {
            // RFID Handler
            if (blnConnectOpenFlag) {
                //RFIDリーダーの接続
                if (rfidHandler == null) {
                    rfidHandler = new RFIDHandler();
                    rfidHandler.onCreate(this);
                } else {
                    rfidHandler.setActivity(this);
                }
                //RFIDリーダーQRの接続 20200108 20191216時にコメントしたものを解除
                //RFIDリーダーとの接続状態を確認するために必要
                if (barcode == null) {
                    Thread.sleep(500);
                    barcode = new BarcodeHandler();
                    barcode.onCreate(this);
                    barcode.onResumeBarcode();
                    barcode.UpdateScannerListView(EVENT);
                } else {
                    barcode.setActivity(this);
//                    barcode.onResumeBarcode();
                    barcode.UpdateScannerListView(EVENT);
                }
            } else {
                connectReader = false;
                invalidateOptionsMenu();
            }
            //AndroidBarcode
            if (barcodezebra == null) {
                barcodezebra = new BarcodeZebraHandler();
                barcodezebra.onCreate(this);
            } else {
                barcodezebra.setActivity(this);
            }
        } catch (Exception e){
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    /**
     * バーコード受信時の処理をサブクラスで実装する
     *
     * @param strReadData
     */
    protected abstract void BarcodeDataReceived(String strReadData);

    /**
     * RFID受信時の処理をサブクラスで実装する
     *
     * @param strReadData
     */
    protected abstract void RFIDDataReceived(String strReadData, short tempRSSI, final String strPcHex);

    /**
     * 接続のON/OFFをサブクラスで実装する
     */
    protected abstract void ReaderOn();
    protected abstract void ReaderOff();

    /**
     * 二度読みバッファをクリア
     */
    public void subClearDuplexBuffer() {
        ReaderOn();
    }

    /**
     * バーコード リーダーＯＮ
     */
    protected void Barcode_Read_Start() {
        try {
//20200108 Add Start Sasaki
            //前画面のバーコードON/OFF処理とかぶらないようにスリープで回避
            try {
                Thread.sleep(100);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
//20200108 Add End Sasaki

            final BaseRFIDActivity activity = this;

            SettingValue setting = new SettingValue();
            final String strReaderMode = setting.getReaderChange(activity);

            //対象リーダー取得する
            if (CommonConst.READER_SETTING.Reader.getString().equals(strReaderMode)) {
                //AndroidのQR/BCを使用不可にする
                if (barcodezebra != null) {
                    barcodezebra.disable();
                }
                //RFIDリーダーのQR/BCに接続する
                if (barcode == null) {
                    //リーダーに接続する
                    barcode = new BarcodeHandler();
                    barcode.onCreate(activity);
                    barcode.onResumeBarcode();
                    barcode.UpdateScannerListView(EVENT) ;
                } else {
                    barcode.setActivity(activity);
                    //barcode.onResumeBarcode();
                    barcode.UpdateScannerListView(EVENT) ;
                }
            } else {
                //AndroidのQR/BCに接続する
                if (barcodezebra == null) {
                    barcodezebra = new BarcodeZebraHandler();
                    barcodezebra.onCreate(activity);
                } else {
                    barcodezebra.setActivity(activity);
                    barcodezebra.enable();
                }
            }
            new Thread() {
                @Override
                public void run() {
                    try {
                        //RFIDリーダーのモードをバーコードに設定する
                        if (rfidHandler != null && connectReader) {
                            //reader接続の場合
                            if (CommonConst.READER_SETTING.Reader.getString().equals(strReaderMode)) {
                                if (rfidHandler.reader.getHostName().startsWith("RFD8500")) {
                                    Thread.sleep(1100);      //ConfigでのLockエラー回避
                                    rfidHandler.reader.Config.setTriggerMode(ENUM_TRIGGER_MODE.BARCODE_MODE, true);
                                    rfidHandler.reader.Config.setBeeperVolume(BEEPER_VOLUME.QUIET_BEEP);
                                }
                            }

                            // RFD40のトリガー割り当て
                            setTriggerLayoutRfd40(strReaderMode, false);
                        }
                    } catch (Exception e) {
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                    }
                }
            }.start();
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    /**
     * バーコード リーダーＯＦＦ
     */
    protected void Barcode_Read_Stop() {
//20200108 Add Start Sasaki
        //前画面のバーコードON/OFF処理とかぶらないようにスリープで回避
        try {
            Thread.sleep(200);
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
//20200108 Add End Sasaki
        new Thread() {
            @Override
            public void run() {
                try {
                    //RFIDリーダーのモードのバーコードに解除する
                    if (rfidHandler != null) {
                        if (rfidHandler.isReaderConnected()) {
                            try {
                                rfidHandler.reader.Config.setTriggerMode(ENUM_TRIGGER_MODE.RFID_MODE, true);
                                setTriggerNoActionRfd40();
                            } catch (Exception e) {
                                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                            }
                        }
                    }

                    //AndroidのQR/BCを使用不可にする
                    if (barcodezebra != null) {
                        barcodezebra.disable();
                    }

                } catch (Exception e) {
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                }
            }
        }.start();
    }

//20200108 Add Start Sasaki
    /**
     * RFIDのバーコードリーダーを切断する
     */
    protected void DisconnectBarcode() {
        if (barcode != null) {
            barcode.dcssdkEventScannerDisappeared(_ScannerID);
            barcode.disconnect(_ScannerID);
            barcode.scannerHasDisconnected(_ScannerID);
        }
    }
//20200108 Add End Sasaki

    /**
     * RFID リーダーＯＮ
     *
     * @param prmParmChange パラメータ変更(=true)/変更なし(=false)
     * @param strKey
     */
    protected void RFID_Read_Start(boolean prmParmChange, final String strKey) {

        _IntensityKey = strKey;

        try {
            if (rfidHandler == null || !rfidHandler.isReaderConnected()) {
                initializeReader();
            }

            //Beep音Create
            try {
                if (beepSound == null) {
                    beepSound = new BeepSound(this);
                }
            } catch (Exception e) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
            }

            // プログレスダイアログの表示
            C_Progres.StartProgressDialog(this, "Connect...");

            final BaseRFIDActivity activity = this;
            new Thread() {
                @Override
                public void run() {

                    try {
                        SettingValue setting = new SettingValue();
                        final String strReaderMode = setting.getReaderChange(activity);

                        // RFD40のトリガー割り当て
                        setTriggerLayoutRfd40(strReaderMode, true);

                        //棚卸の場合
                        Short srtTagPopulation = Short.valueOf(TagPopulation.MIN.getString());
                        if(C_Setting.KEY_EACH_INVENTORY.equals(strKey)) {
                            srtTagPopulation = Short.valueOf(TagPopulation.MAX.getString());
                        }

                        //読取設定
                        rfidHandler.ConfigureReader(Integer.valueOf(C_Setting.getBTReadIntensity(activity, strKey))
                                                    , C_Setting.getBTReadSession(activity, strKey)
                                                    ,srtTagPopulation);
                    } catch (Exception e) {
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                    } finally {
                        C_Progres.StopProgressDialog();
                    }
                }
            }.start();

        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
            C_Progres.StopProgressDialog();
        }
    }

    /**
     * RFID リーダーＯＦＦ
     */
    protected void RFID_Read_Stop() {
        new Thread() {
            @Override
            public void run() {
                try {
                    if (connectReader) {
                        rfidHandler.stopInventory();
                        rfidHandler.removeEventsListener();
                    }

                    //音を停止
                    if (beepSound != null) {
                        beepSound.Destory();
                        beepSound = null;
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        }.start();
    }

    protected void RFID_Setting_Start() {
        if (rfidHandler == null) {
            return ;
        }

        try {
            Thread.sleep(300);	//20200122 Add Sasaki
            rfidHandler.ConfigureBattery();
        } catch (InvalidUsageException e) {
            e.printStackTrace();
        } catch (OperationFailureException e) {
            e.printStackTrace();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * 探索開始
     * @param strAssetID
     * @param strKey
     * @throws OperationFailureException
     * @throws InvalidUsageException
     */
    protected void Rfid_Search_Start(String strAssetID, final String strKey) throws OperationFailureException, InvalidUsageException {

        //Beep音Create
        try {
            if (beepSoundSearch == null){
                beepSoundSearch = new BeepSoundSearch(this);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }

        rfidHandler.onResume();
        blnTagSearch = true;
        _TagID = strAssetID;
        final BaseRFIDActivity activity = this;
        new Thread() {
            @Override
            public void run() {

                try {
                    SettingValue setting = new SettingValue();
                    final String strReaderMode = setting.getReaderChange(activity);

                    // RFD40のトリガー割り当て
                    setTriggerLayoutRfd40(strReaderMode, true);

                    rfidHandler.ConfigureReaderSearch(Integer.valueOf(C_Setting.getBTReadIntensity(activity, strKey))
                                                        , C_Setting.getBTReadSession(activity, strKey)
                                                        ,Short.valueOf(TagPopulation.MAX.getString()));
                    messageHandler.post(r);
                } catch (Exception e) {
                    showToastMessage(e.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                }
            }
        }.start();
    }

    /**
     * 探索終了
     */
    protected void Rfid_Search_Stop(){
        try {
            //rfidHandler.stopInventory();
            rfidHandler.removeEventsListener();
            messageHandler.removeCallbacks(r);

            blnTagSearch = false;

            //音を停止
            if (beepSoundSearch != null) {
                beepSoundSearch.Destory();
                beepSoundSearch = null;
            }
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    //タイマータスク
    Runnable r = new Runnable() {
        @Override
        public void run() {
            try {
                float readPowerLevel = 0;
/*                if(_ArrayRssi.size() <= 0) {
                    setReadPowerLevelOnSearch(null);
                }
               else {*/
                for (int i = 0; i < _ArrayRssi.size(); i++) {
                    readPowerLevel = readPowerLevel + _ArrayRssi.get(i);
                }
                //readPowerLevel = readPowerLevel / _ArrayRssi.size() / 10.0f;
                if (readPowerLevel > 0 ) {
                    readPowerLevel = readPowerLevel / _ArrayRssi.size();
                }
                Log.i("SearchPowerLevel", String.valueOf(readPowerLevel));
                setReadPowerLevelOnSearch(readPowerLevel);
                _ArrayRssi.clear();
 //               }
                messageHandler.postDelayed(this,250);
            }
            catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            }
        }
    };

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
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * 探索の電波強度の画像と音を設定する
     * @param readPowerLevel  Read output value when searching
     */
    private void setReadPowerLevelOnSearch(@Nullable Float readPowerLevel) {
        try {

            readPowerLevelOnSearch = readPowerLevel;

            //電波強度を表示する
            String text = readPowerLevel != null ? String.format("%.2f", readPowerLevel) : "";
            TextView textView = (TextView) findViewById(R.id.labSchRssiVal);
            textView.setText(text);

            //電波強度のステージを取得する
            ReadPowerStage readPowerStage = getReadPowerStage(readPowerLevel);

            //音を設定する
            setSearchSound(readPowerStage);

            //変更がない場合、再表示せず終了
            if (readPowerStageOnSearch == readPowerStage) {
                return;
            }
            readPowerStageOnSearch = readPowerStage;
            setSearchCircle(readPowerStage);

        }
        catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * Seting the sound according to the stage of Read output value
     * @param readPowerStage  The stage of Read output value  Do not playback if null is specified
     */
    private void setSearchSound(@Nullable ReadPowerStage readPowerStage) {
        try {
            if (readPowerStage == null) {
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
     * 電波レベルを取得する
     * @param readPowerLevel  Read output value
     * @return  The stage corresponding to the Read output value Return null if null is specified to argument
     */
    private ReadPowerStage getReadPowerStage(@Nullable Float readPowerLevel) {
        try {
            if (readPowerLevel == null || readPowerLevel == 0) {
                return null;
            }

            int stage2MaxReadPowerLevel = getResources().getInteger(R.integer.zebra_stage2_max_read_power_level_on_search);
            int stage3MaxReadPowerLevel = getResources().getInteger(R.integer.zebra_stage3_max_read_power_level_on_search);
            int stage4MaxReadPowerLevel = getResources().getInteger(R.integer.zebra_stage4_max_read_power_level_on_search);
            int stage5MaxReadPowerLevel = getResources().getInteger(R.integer.zebra_stage5_max_read_power_level_on_search);
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
     * 電波レベルを取得する(探索一覧用)
     * @param readPowerLevel  Read output value
     * @return  The stage corresponding to the Read output value Return null if null is specified to argument
     */
    private ReadPowerStage getReadPowerStageSearchList(@Nullable Float readPowerLevel) {
        try {
            if (readPowerLevel == null || readPowerLevel == 0) {
                return null;
            }

            int stage2MaxReadPowerLevel = getResources().getInteger(R.integer.zebra_list_stage2_max_read_power_level_on_search);
            int stage3MaxReadPowerLevel = getResources().getInteger(R.integer.zebra_list_stage3_max_read_power_level_on_search);
            int stage4MaxReadPowerLevel = getResources().getInteger(R.integer.zebra_list_stage4_max_read_power_level_on_search);
            int stage5MaxReadPowerLevel = getResources().getInteger(R.integer.zebra_list_stage5_max_read_power_level_on_search);
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
     * RFID 書込みＯＮ
     * @param prmParmChange
     * @param strKey
     */
    protected void RFID_Write_Start(boolean prmParmChange, final String strKey) {
        try {
            final Activity activity = this;
            new Thread() {
                @Override
                public void run() {
                    try {
                        SettingValue setting = new SettingValue();
                        final String strReaderMode = setting.getReaderChange(activity);

                        // RFD40のトリガー割り当て
                        setTriggerLayoutRfd40(strReaderMode, true);

                        rfidHandler.ConfigureReaderSingleTrigger(Integer.valueOf(C_Setting.getBTReadIntensity(activity, strKey))
                                , C_Setting.getBTReadSession(activity, strKey)
                                ,Short.valueOf(TagPopulation.MIN.getString()));
                        rfidHandler.reader.Config.setTriggerMode(ENUM_TRIGGER_MODE.RFID_MODE, true);
                        //rfidHandler.performInventory();
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                }
            }.start();

        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * タグ書込み
     * @param strTag 既存EPC
     * @param strNewTag 新規EPC
     * @param strOldPc 既存PC
     * @param newEpcBitSize 新規EPCサイズ
     */
    protected void WriteTag(String strTag, String strNewTag, String strOldPc, int newEpcBitSize) throws InvalidUsageException, OperationFailureException, RFIDPkgException{

        try {
            //書込み設定
            rfidHandler.ConfigureReaderWrite();

            String strNewPc = "";
            boolean isPcOverWrite = false;

            //PCの書換が必要かどうかをチェック
            if (Common.needOverWritePc(newEpcBitSize, strOldPc)) {
                //PC書換あり
                strNewPc = Common.calcTagPcHex(newEpcBitSize, strOldPc);
                isPcOverWrite = true;
            }

            //書込み処理
            rfidHandler.WriteTag(strTag, strNewPc + strNewTag, isPcOverWrite);
        } catch (OperationFailureException e) {
            //書込みに失敗した場合
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_LINK_REGISTER_BUTTON, "", this) + ":" + e.getResults() + ":" +e.getVendorMessage() + ":" + Common.toString(e), this);
            e.printStackTrace();
            throw new RFIDPkgException(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_WRITE, "", this));
        } catch (Exception e ) {
            //書込みに失敗した場合
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_LINK_REGISTER_BUTTON, "", this) + ":" + Common.toString(e), this);
            e.printStackTrace();
            throw new RFIDPkgException(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_WRITE, "", this));
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

        //書込みしたTagを読取してチェックする
        TagData tagRead = rfidHandler.ReadTag(strNewTag);
        if (tagRead == null) {
            //読取したTAGを読めなかった場合、エラー
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_LINK_REGISTER_BUTTON, "", this) + ": 書込タグの読取 null", this);
            throw new RFIDPkgException(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_WRITE, "", this));
        } else if (!strNewTag.equals(tagRead.getTagID())) {
            //読取した資産IDが一致しなかった場合、エラー
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_LINK_REGISTER_BUTTON, "", this) + ": 書込タグの資産IDチェックエラー", this);
            throw new RFIDPkgException(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_WRITE, "", this));
        }
    }

    /**
     * 読取した探索データを取得する
     * @param strTagID
     * @return
     */
    protected String ReadSearchTag(String strTagID, String strKey){
        String strRead = "";
        TagData tagRead = null;

        try {
            rfidHandler.ConfigureReaderSearchSummary(Integer.valueOf(C_Setting.getBTReadIntensity(this, strKey))
                                                    , C_Setting.getBTReadSession(this, strKey)
                                                    , Short.valueOf(TagPopulation.MAX.getString()));
            tagRead = rfidHandler.ReadTag(strTagID);
        } catch (Exception e) {
            e.printStackTrace();
        }

        if (tagRead != null) {
            strRead = String.valueOf(tagRead.getPeakRSSI());
        }

        return strRead;
    }

    // 電波強度変更処理
    protected void ChangeIntencity() {

        String strKey = GetSettingKey();

        //電波強度有効チェック
        Spinner _Intencity = findViewById(R.id.cmbIntencity);
        int Result = C_Setting.ValidCheckNewIntencity(this, _Intencity, strKey);
        if (Result < 0) {   //入力エラー
            showErrorMessage(C_Setting.p_ErrorMessage);
            return;
        } else if (Result == 0) {     //変更なし
            return;
        }

        // リーダーOFF
        ReaderOff();

        //リーダーON
        ReaderOn();
    }

    //電池残量表示
    protected void BatteryRemainDisp() {

    }

    protected void buzzer_B2() {
        try {
             //commScanner.buzzer(CommConst.CommBuzzerType.B2);
        } catch (Exception ex) {
            Log.d(CommonConst.DIALOG_ERROR_TITLE, ex.getMessage());
        }
    }

    protected void buzzer_B3() {
        try {
            //commScanner.buzzer(CommConst.CommBuzzerType.B3);
        } catch (Exception ex) {
            Log.d(CommonConst.DIALOG_ERROR_TITLE, ex.getMessage());
        }
    }

    protected boolean ConnectStatus() {
        return _finalSuccessFlag;
    }

    /**
     *  RFIDリーダー電池残量取得
     *
     * @return 0:残なし、1:30%未満、2:70%未満、3:70%以上
     */
    private int batteryResId = 0;
    public void setBatteryData(final Events.BatteryData strBattery) {

        try {
            //電池残量取得
            int _AnsValue = 0;
            if (strBattery == null) {
                _AnsValue = 0;
            } else if (strBattery.getLevel() >=  70) {
                _AnsValue = 3;
            } else if (strBattery.getLevel() >=  30) {
                _AnsValue = 2;
            } else if (strBattery.getLevel() >=  1) {
                _AnsValue = 1;
            } else {
                _AnsValue = 0;
            }

            //残量によってイメージを切り替える
            batteryResId = 0;
            switch(_AnsValue) {
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
            if (strBattery != null && _AnsValue == 1) {
                ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_BATTERY_CHARGE, "", this), "");
            }

            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    try {
                        ImageView _imageView = findViewById(R.id.ImgViewMnuBattery);
                        if (_imageView != null) {
                            _imageView.setImageResource(batteryResId);
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                }
            });

        } catch (Exception e) {
            showToastMessage(e.getMessage());
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
        ReadPowerStage readPowerStage = getReadPowerStageSearchList(Float.valueOf(strIntencity));

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
     * Determine CommScanner
     * If @return CommScanner is connected or disconnected, return true or false.
     */
    public boolean isCommScanner() {
        if (rfidHandler == null) {
            return false;
        }
        return rfidHandler.isReaderConnected();
    }

    @Override
    public void handleTagdata(TagData[] tagData) {
        final StringBuilder sb = new StringBuilder();
        for (int index = 0; index < tagData.length; index++) {
            sb.append(tagData[index].getTagID() + "\n");
        }
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.i("handleTagdata", sb.toString());
            }
        });
    }

    @Override
    public void handleTriggerPress(boolean pressed) {
        try {
            if (pressed) {
                if (blnTagSearch) {
                    rfidHandler.performSearchInventory(_TagID);
                } else {
                    rfidHandler.performInventory();
                }
            } else if (blnTagSearch) {
                rfidHandler.stopSearchInventory();
            } else {
                rfidHandler.stopInventory();
            }
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    @Override
    public void OnScannerAppeared() {
        boolean successFlag = false;

        try {
            //既に接続済みの場合飛ばす
            if (connectReader) {
                return;
            }

            if (rfidHandler != null) {
                if (rfidHandler.isReaderConnected()) {
                    ReaderOn();
                    _finalSuccessFlag = true;
                    //接続状態メニューを更新する
                    connectReader = true;
                    disconnectFlag = false;
                    invalidateOptionsMenu();
                    runOnUiThread(new Runnable() {
                        public void run() {
                            showToastMessage("connect");
                        }
                    });
                } else {
                    _finalSuccessFlag = false;
                }
            } else {
                onResume();
            }

            if (p_ReconnectFlag) {
                //再接続 メイン画面は表示しない
                return;
            }

            final Activity activity = this;
            this.runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    if (_finalSuccessFlag) {

                        p_ReconnectFlag=true;

                        //接続音を出す
                        BeepSound beepoConnectSound = new BeepSound(activity);
                        beepoConnectSound.BeepConnect();

                        try {
                            Thread.sleep(500);
                        } catch (Exception e) {
                            e.printStackTrace();
                        }

                        //メニュー画面を開く
                        OpenMainActivity();

                    }
                }
            });
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    /**
     * RFIDリーダー切断イベント
     */
    @Override
    public void onDisconnectEvent() {
        try {
            //既に切断済みの場合終了
            if (!connectReader && disconnectFlag) {
                return;
            }

            //接続状態メニューを更新する
            connectReader = false;      //接続状態
            disconnectFlag = true;      //Disconnectイベント
            if (rfidHandler != null) {
                rfidHandler.removeEventsListener();
                rfidHandler.onPause();
                rfidHandler = null;
            }

            //再接続時の強制終了時は処理しない 20200121 Upd Start Sasaki
            if (!ConnectMessageDisable) {
                invalidateOptionsMenu();
                setBatteryData(null);
                DisconnectBarcode();
            }

            if (DisconnectBarcodeFlag) {
                DisconnectBarcode();
            }
            //20200121 Upd End Sasaki

            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    //再接続時の強制終了時はメッセージを表示しない
                    if (!ConnectMessageDisable) { //20200121 Add Sasaki
                        showToastMessage("disconnect");
                    }

                    ConnectMessageDisable = false; //20200121 Add Sasaki
                    DisconnectBarcodeFlag = false; //20200121 Add Sasaki
                }
            });
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * RFIDタグ受信イベント
     * @param strData EPC
     * @param tempRSSI RSSI
     * @param strPcHex EPC領域内のPC
     */
    @Override
    public void handleRfidReceived(String strData, final short tempRSSI, final String strPcHex) {
        final String strDatasend = strData;
        final BaseRFIDActivity activity = this;
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                try {
                    RFIDDataReceived(strDatasend, tempRSSI, strPcHex);
                } catch (Exception e) {
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                }
            }
        });
    }

    /**
     * バーコード/QR受信イベント
     * @param strData
     */
    @Override
    public void handleBarcodeReceived(String strData) {
        final String strDatasend = strData;
        final BaseRFIDActivity activity = this;
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                try {
                    //beepSound.BeepCountUp();
                    BarcodeDataReceived(strDatasend);
                } catch (Exception e) {
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                }
            }
        });
    }

    @Override
    public void doConnectResult(int intResultCode, String strMessage) {
        Intent returnIntent = new Intent();
        setResult(intResultCode, returnIntent);
        //Toast.makeText(getApplicationContext(), strMessage, Toast.LENGTH_SHORT).show(); //20200116 Add Sasaki
    }

    /**
     * ブザー音量設定
     *
     * @param prmNewVal 設定値 0:小、1:中、2:大
     */
    public void SetBuzzerVolume(int prmNewVal) {
        try {
            switch (prmNewVal) {
                case 0:     //音量 小
                    rfidHandler.reader.Config.setBeeperVolume(BEEPER_VOLUME.LOW_BEEP);
                    break;

                case 1:     //音量 中
                    rfidHandler.reader.Config.setBeeperVolume(BEEPER_VOLUME.MEDIUM_BEEP);
                    break;

                case 2:     //音量 大
                    rfidHandler.reader.Config.setBeeperVolume(BEEPER_VOLUME.HIGH_BEEP);
                    break;
                default:    //音量 ミュート
                    rfidHandler.reader.Config.setBeeperVolume(BEEPER_VOLUME.QUIET_BEEP);
            }

            rfidHandler.reader.Config.saveConfig();
            //save
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
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
            int id = item.getItemId();
            switch (id) {
                case R.id.action_setting:
                    //ボタン非活性
                    AllResetButton(false);
                    //設定 →　システム設定画面開く
                    Intent intent2 = new Intent(getApplication(), SystemSett.class);
                    startActivity(intent2);
                    return true;
                case R.id.action_connect:
                    //接続
                    if (!connectReader) {
                        //開始
                        onResume();
                    }
                    return true;
                case R.id.action_menu:
                    //ボタン非活性
                    AllResetButton(false);
                    ReaderOff();
                    onMenuReturn();

                    return true;
            }

            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            return false;
        }
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
                String msg = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_HT_DATE, "マスタ受信処理を中断して、Androidの時刻設定機能で修正してください。", this);
                String serverDtTitle = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SERVER_DATE, "サーバー日時", this) + ": ";
                String htDtTitle = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_HT_DATE, "HT日時", this) + ": ";
                showHtDateToast(msg, serverDtTitle, sdf.format(serverDate), htDtTitle, sdf.format(htDate.getTime()));
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
     * サーバーとHTの時刻を表示するトースト
     * @param message メッセージ
     * @param serverDtTitle サーバー日時タイトル
     * @param serverDt サーバー日時
     * @param htDtTitle HT日時タイトル
     * @param htDt HT日時
     */
    private void showHtDateToast(String message, String serverDtTitle, String serverDt, String htDtTitle, String htDt) {
        // LayoutInflaterを使用してカスタムレイアウトをインフレート
        LayoutInflater inflater = getLayoutInflater();
        View layout = inflater.inflate(R.layout.custom_htdate_toast, null);

        // トーストの表示内容を設定
        //　サーバー日時タイトル
        TextView labTitleServerDt = layout.findViewById(R.id.title_toast_server_date);
        labTitleServerDt.setText(serverDtTitle);

        // HT日時タイトル
        TextView labTitleHtDt = layout.findViewById(R.id.title_toast_ht_date);
        labTitleHtDt.setText(htDtTitle);

        // サーバー日時
        TextView labServerDt = layout.findViewById(R.id.toast_server_date);
        labServerDt.setText(serverDt);

        // HT日時
        TextView labHtDt = layout.findViewById(R.id.toast_ht_date);
        labHtDt.setText(htDt);

        //　メッセージ
        TextView labMsg = layout.findViewById(R.id.toast_message);
        labMsg.setText(message);

        // トーストを作成して表示
        Toast toast = new Toast(this);
        toast.setDuration(Toast.LENGTH_LONG);
        toast.setView(layout);
        toast.show();
    }

//20200108 Add Start Sasaki
    /**
     * スキャナIDをセットする
     * @param intScannerID スキャナID
     */
    protected void setScannerID(int intScannerID) {
        _ScannerID = intScannerID;
    }
//20200108 Add End Sasaki

    protected abstract String GetSettingKey();

    //DENSO用(ZEBRAでは使用せず)
    protected void Rfid_Search_List_Start(String strAssetID, String strKey){}

    /**
     * RFD40のトリガー割り当て
     */
    private void setTriggerLayoutRfd40(String readerMode, boolean isRFID) {
        if (rfidHandler == null || !connectReader) {
            //RFIDリーダが接続されていない場合はトリガー割り当てなし
            return;
        }

        if (!rfidHandler.reader.getHostName().startsWith("RFD40")) {
            //RFD40以外なら処理なし
            return;
        }

        String[] splitStr = rfidHandler.reader.getHostName().split("-");

        try {
            if (isRFID) {
                // 上下トリガーがタグ読取
                rfidHandler.reader.Config.setKeylayoutType(ENUM_NEW_KEYLAYOUT_TYPE.RFID, ENUM_NEW_KEYLAYOUT_TYPE.RFID);
                SetBuzzerVolume(9);
                return;
            }

            if (CommonConst.READER_SETTING.Android.getString().equals(readerMode)) {
                // QR読取端末がAndroidになっている場合、上下トリガーがAndroid端末でのレーザー照射によるQR読取
                rfidHandler.reader.Config.setKeylayoutType(ENUM_NEW_KEYLAYOUT_TYPE.TERMINAL_SCAN, ENUM_NEW_KEYLAYOUT_TYPE.TERMINAL_SCAN);
                return;
            }

            boolean isPremiumPlus = false;

            //RFD40のモデルによって分岐
            // ※Premium Plus以外は同じトリガー設定を適用する
            switch (splitStr[0]) {
                case "RFD4030": // Standard
                    break;
                case "RFD4031": // Premium or Premium Plus
                    if (splitStr[1].contains("G1")) {
                        // Premium Plus (WiFi & Scan)
                        isPremiumPlus = true;
                    }
                    break;
                default:
                    if (splitStr[0].startsWith("RFD40+")) {
                        // Premium Plus (WiFi & Scan)
                        isPremiumPlus = true;
                    }
                    break;
            }

            //読取時の音量設定
            SetBuzzerVolume(1);

            if (isPremiumPlus) {
                // Premium Plusの場合は、上下トリガーがRFIDリーダでのレーザー照射によるQR読取
                rfidHandler.reader.Config.setKeylayoutType(ENUM_NEW_KEYLAYOUT_TYPE.SLED_SCAN, ENUM_NEW_KEYLAYOUT_TYPE.SLED_SCAN);
            } else {
                // StandardとPremiumの場合は、上下トリガーがAndroid端末でのレーザー照射によるQR読取
                rfidHandler.reader.Config.setKeylayoutType(ENUM_NEW_KEYLAYOUT_TYPE.TERMINAL_SCAN, ENUM_NEW_KEYLAYOUT_TYPE.TERMINAL_SCAN);
            }

        } catch (Exception e) {
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_SCAN, "ScanTrigger", this) + ":" + e.getMessage() + ":" + Common.toString(e), this);
        }
    }

    /**
     * RFD40のプレミアムプラスの場合はトリガー割り当てを解除する
     */
    private void setTriggerNoActionRfd40() {
        try {
            if (rfidHandler == null || !connectReader) {
                //RFIDリーダが接続されていない場合はトリガー割り当てなし
                return;
            }

            if (!rfidHandler.reader.getHostName().startsWith("RFD40")) {
                //RFD40以外なら処理なし
                return;
            }

            //トリガー割り当て解除
            rfidHandler.reader.Config.setKeylayoutType(ENUM_NEW_KEYLAYOUT_TYPE.NO_ACTION, ENUM_NEW_KEYLAYOUT_TYPE.NO_ACTION);

        } catch (Exception e) {
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_SCAN, "ScanTrigger", this) + ":" + e.getMessage() + ":" + Common.toString(e), this);
        }
    }

    /**
     * RFD40がプレミアムプラスかどうかを判断する
     * TODO ※タイミング的に共通化できなかったので、今後可能なタイミングで実施する
     * @param deviceInfo 機器情報
     * @return True:Premium Plus  False:Standard or Premium
     */
    private boolean isRfd40PremiumPlus(String[] deviceInfo) {
        boolean isPremiumPlus = false;

        //RFD40のモデルによって分岐
        // ※Premium Plus以外は同じトリガー設定を適用する
        switch (deviceInfo[0]) {
            case "RFD4030": // Standard
                break;
            case "RFD4031": // Premium or Premium Plus
                if (deviceInfo[1].contains("G1")) {
                    // Premium Plus (WiFi & Scan)
                    isPremiumPlus = true;
                }
                break;
            default:
                if (deviceInfo[0].startsWith("RFD40+")) {
                    // Premium Plus (WiFi & Scan)
                    isPremiumPlus = true;
                }
                break;
        }

        return isPremiumPlus;
    }

    /**
     * 電波強度の設定可能範囲値を作成
     * @return 設定可能範囲
     */
    protected String[] SetIntensityRange() {
        String[] comboItems;

        if (rfidHandler.reader != null && rfidHandler.reader.getHostName().startsWith("RFD40")) {
            //RFD40の場合、0～300が範囲
            comboItems = new String[31];
            for (int i = 0; i < 31; i++) {
                comboItems[i] = String.valueOf(i * 10);
            }
        } else {
            //それ以外の場合、120～280が範囲
            comboItems = new String[17];
            //String[] comboItems = new String[27];
            for (int i = 0; i < 17; i++) {
                comboItems[i] = String.valueOf((i + 12) * 10);
            }
        }

        return comboItems;
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
}