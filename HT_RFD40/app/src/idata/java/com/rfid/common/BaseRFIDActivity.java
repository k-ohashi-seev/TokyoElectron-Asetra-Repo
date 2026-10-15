package com.rfid.common;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.database.sqlite.SQLiteDatabase;
import android.os.BatteryManager;
import android.os.Bundle;
import android.os.Handler;
import android.provider.Settings;
import androidx.annotation.Nullable;
import android.text.TextUtils;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MenuItem;
import android.widget.ImageView;
import android.widget.Spinner;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.idata.IdataApplication;
import com.rfid.common.idata.ScannerInterface;
import com.rfid.common.idata.event.BackResult;
import com.rfid.common.idata.event.GetRFIDThread;
import com.rfid.common.idata.util.MUtil;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;

import java.util.Date;
import java.util.Calendar;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Timer;
import java.util.TimerTask;

import realid.rfidlib.BuildConfig;
import realid.rfidlib.EmshConstant;

import static realid.rfidlib.EmshConstant.EmshBatteryPowerMode.EMSH_PWR_MODE_BATTERY_ERROR;
import static realid.rfidlib.EmshConstant.EmshBatteryPowerMode.EMSH_PWR_MODE_CHG_FULL;
import static realid.rfidlib.EmshConstant.EmshBatteryPowerMode.EMSH_PWR_MODE_CHG_GENERAL;
import static realid.rfidlib.EmshConstant.EmshBatteryPowerMode.EMSH_PWR_MODE_CHG_QUICK;
import static realid.rfidlib.EmshConstant.EmshBatteryPowerMode.EMSH_PWR_MODE_DSG_UHF;
import static realid.rfidlib.EmshConstant.EmshBatteryPowerMode.EMSH_PWR_MODE_STANDBY;

/**
 * iDATAへの接続用クラス
 * RFIDリーダのRFIDタグ、QR/Barcodeの読取
 * Android端末からのQR/Barcode読取を行います。
 *
 */
public abstract class BaseRFIDActivity extends BaseActivity implements BackResult, CommonHtDateCheckDialog.Callback {
    protected ScannerInterface scannerInterface;
    protected Boolean isScsan=false;
    protected Boolean isRFIDRead =false;

    protected boolean disconnectFlag = false;

    //スキャナ再接続フラグ
    public static boolean p_ReconnectFlag = false;

    private boolean _finalSuccessFlag = false;

    //RSSI 積算配列
    protected ArrayList<Short> _ArrayRssi = new ArrayList<Short>();


    //プログレスダイアログ クラス
    private CommonProgres C_Progres = new CommonProgres();

    protected boolean InitializeActivity = false;

    //探索ハンドル
    private Handler messageHandler = new Handler();

    //探索レベル
    private Float readPowerLevelOnSearch = null;

    protected boolean blnConnectOpenFlag = true;

    //探索ステージ
    private ReadPowerStage readPowerStageOnSearch = null;

    //システム設定値
    protected SettingValue C_Setting = new SettingValue();


    private String _IntensityKey = "";

    protected BeepSound beepSound = null;
    protected BeepSoundSearch beepSoundSearch = null;


    private Timer mTimer = null;
    private TimerTask mTimerTask = null;

    private int _ScannerID = 0; //2022/8/1 ADD S.Arai

    private boolean IsSearchTag=false;

    //RFIDリーダーのバッテリー残量レベル
    private int _scannerBatteryLevel = -1;

    //Android端末の充電状況
    private int _androidBatteryStatus = -1;

    /** ダイアログコード */
    private static final int DIALOG_HT_DATE = 2001;
    private static final int DIALOG_ERR = 2002;

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

    protected abstract void OpenMainActivity();

    private int REFRESH = 1;
    private int EVENT = 2;
    private int CREATE = 3;

    //DENSO用(ZEBRAでは使用せず)
    protected HashMap<String, Short> LinkTagList = new HashMap<String, Short>();

    private EmshStatusBroadcastReceiver mEmshStatusReceiver;
    private GetRFIDThread rfidThread = GetRFIDThread.getInstance();//RFID标签信息获取线程

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        SQLiteDatabase db = null;

        //機器名称を設定する
        Common.ConnectRFIDName = CommonConst.CONNECT_RFIDNAME.IDATA.getString();
        try {
            //Beep音Create
            if (beepSound == null) {
                beepSound = new BeepSound(this);
            }
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            //IdataApplication.getIdataApp().getIdataLib().changeConfig(true);

            //RFIDリーダを使用しない設定の場合、RFIDリーダーに接続しない
            if (CommonConst.READING_MEDIUM.QR.getString().equals(SettingFactory.GetReadingMedium(db, this))) {
                blnConnectOpenFlag = false;
            }

            GetRFIDThread.getInstance().setBackResult(this);
            if(!rfidThread.getRunStatus())
                rfidThread.start();
            monitorEmsh();
            connectReader = IdataApplication.poweronStatus;
            invalidateOptionsMenu();
            scannerInterface=new ScannerInterface(this);
            scannerInterface.unlockScanKey();
            isScsan=false;
            isRFIDRead =false;
            showbeep=true;
            mIntentFilter = new IntentFilter();
            mIntentFilter.addAction(RES_ACTION);
        } catch (Exception e){
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        } finally {
            if(db != null){
                db.close();
            }
        }

    }


    //定时监听把枪状态
    private void monitorEmsh() {
        if(mEmshStatusReceiver==null) {
            mEmshStatusReceiver = new EmshStatusBroadcastReceiver();
            //RFIDリーダーの情報を取得するフィルター設定
            IntentFilter readerFilter = new IntentFilter(EmshConstant.Action.INTENT_EMSH_BROADCAST);
            registerReceiver(mEmshStatusReceiver, readerFilter);

            //Android端末の情報を取得するフィルター設定
            IntentFilter androidFilter = new IntentFilter(Intent.ACTION_BATTERY_CHANGED);
            registerReceiver(mEmshStatusReceiver, androidFilter);
        }
    }


    protected void onResume() {
        super.onResume();
        rfidThread.pauseThread();
        registerReceiver(BarcodeReceiver,mIntentFilter);
    }

    @Override
    protected void onPause() {
        super.onPause();
        try {
            recyleResoure();
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        rfidThread.interrupt();

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
            if(event.getRepeatCount()==0) {
                switch (keyCode) {
                    case KeyEvent.KEYCODE_F4:
                    case KeyEvent.KEYCODE_F8: //把枪按钮被按下,默认值为138
                        if (isRFIDRead) {
                            //タグ読取（このクラスで読取開始処理を実施する場合）
                            RFID_Read_Start(false, GetSettingKey());
                            return false;
                        }

                        if (isScsan) {
                            //バーコード読取
                            scannerInterface.scan_start();
                            return false;
                        } else {
                            //タグ読取（継承先で読取開始処理を実行する場合）
                            //継承先クラスでタグ読取を開始したかどうかを判別するため、タグ読取のみ戻り値をTrueで返す
                            //継承先クラスで読取開始を実行しているため、この中では読取開始は実行しない
                            //※iDataソースマージのため
                            return true;
                        }
                }
            }
            return super.onKeyDown(keyCode, event);
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * キーリリースイベント
     * @param keyCode
     * @param event
     * @return
     */
    @Override
    public boolean onKeyUp(int keyCode, KeyEvent event) {
        try {
            switch (keyCode)  {
                case KeyEvent.KEYCODE_F4:
                case KeyEvent.KEYCODE_F8: //把枪按钮被按下,默认值为138
                    if (isRFIDRead) {
                        //タグ読取（このクラスで読取終了処理を実施する場合）
                        RFID_Read_Stop();
                        return false;
                    }

                    if (isScsan) {
                        //バーコード読取
                        scannerInterface.scan_stop();
                        return false;
                    } else {
                        //タグ読取（継承先で読取終了処理を実行する場合）
                        //継承先クラスでタグ読取を終了したかどうかを判別するため、タグ読取のみ戻り値をTrueで返す
                        //継承先クラスで読取終了を実行しているため、この中では読取終了は実行しない
                        //※iDataソースマージのため
                        return true;
                    }
            }

            return super.onKeyDown(keyCode, event);
        }
        catch (Exception ex) {
            return false;
        }
    }

    private short maxValue = -29, minValue = -70; //RSSI的最大值和最小值
    public boolean showbeep=true;

    @Override
    public void postResult(String[] tagData) {
        try {
            final String epc = tagData[1]; //拿到EPC
            final String rssiStr = tagData[2]; //为负数的字符串

            int floatBit = Integer.parseInt(rssiStr.substring(rssiStr.length() - 1, rssiStr.length())); // 小数位的值
            int intgerBit = Integer.parseInt(rssiStr.substring(0, rssiStr.length() - 2)); // 整数位的值
            if (floatBit > 5) { //四舍五入操作
                --intgerBit;
            } else {
                ++intgerBit;
            }
            int rssi = intgerBit;
            // int length = maxValue - minValue; //取差值
            if (rssi >= maxValue) {
                rssi = maxValue;
            } else if (rssi <= minValue) {
                rssi = minValue;
            }
            rssi = rssi * 10;
            //カウント音を出す
            if (showbeep) {
                if (beepSound == null) {
                    beepSound = new BeepSound(this);
                }
                beepSound.BeepCountUp();
            }

            //メインスレッドで処理
            final int finalRssi = rssi;
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    RFIDDataReceived(epc, (short) finalRssi, "");
                }
            });
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    //下电，回收停止线程，退出应用
    private void recyleResoure() throws InterruptedException {
        MUtil.getInstance().rcyleDialog();
        //这里强制停止盘点，无论是否使用
        IdataApplication.getIdataApp().getIdataLib().stopInventory();
        if (mEmshStatusReceiver != null) {
            unregisterReceiver(mEmshStatusReceiver);
            mEmshStatusReceiver = null;
        }
        if (BarcodeReceiver != null) {
            unregisterReceiver(BarcodeReceiver);
//            BarcodeReceiver=null;
        }
        if (mTimer != null || mTimerTask != null) {
            mTimerTask.cancel();
            mTimer.cancel();
            mTimerTask = null;
            mTimer = null;
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
     * サービス開始(iDATAでは使用しない)
     */
    public void startService() {
    }


    /**
     * バーコード受信時の処理をサブクラスで実装する
     *
     * @param strReadData
     */
    protected abstract void BarcodeDataReceived(String strReadData);

    /**
     * RFID受信時の処理をサブクラスで実装する
     * @param strReadData EPC
     * @param tempRSSI RSSI
     * @param strPcHex PC
     */
    protected abstract void RFIDDataReceived(String strReadData, short tempRSSI, final String strPcHex);

    protected void RFIDSearchDataReceived(String strReadData, short tempRSSI){

    }

    /**
     * 接続のON/OFFをサブクラスで実装する
     */
    protected abstract void ReaderOn();
    protected abstract void ReaderOff();
    /**
     * タイマー開始
     */
    protected void StartTimer() {
        //Readerを使用しない場合、終了
        if (!blnConnectOpenFlag){
            return;
        }
        if (!connectReader) {
            return;
        }
        if(mTimer==null) {
            mTimer = new Timer();
            mTimerTask = new TimerTask() {
                @Override
                public void run() {
                    Intent intent = new Intent(EmshConstant.Action.INTENT_EMSH_REQUEST);
                    intent.putExtra(EmshConstant.IntentExtra.EXTRA_COMMAND, EmshConstant.Command.CMD_REFRESH_EMSH_STATUS);
                    sendBroadcast(intent);
                    batteryLevelCheck();
                }
            };
            mTimer.schedule(mTimerTask, 0, 1000);
        }
    }
    protected void StopTimer() {
        //タイマー停止
        if (mTimer != null) {
            mTimer.cancel();
        }
        mTimerTask = null;
        mTimer = null;
    }


    protected void Barcode_Read_Start() {
       //IdataApplication.getIdataApp().getIdataLib().changeConfig(false);
        scannerInterface.lockScanKey();
        isScsan=true;
        MUtil.getInstance().ChangeBarcodeType(true);
    }


    /**
     * バーコード リーダーＯＦＦ
     */
    protected void Barcode_Read_Stop() {
       //IdataApplication.getIdataApp().getIdataLib().changeConfig(true);
        scannerInterface.unlockScanKey();
        isScsan=false;
        MUtil.getInstance().ChangeBarcodeType(false);
    }



    /**
     * リーダー接続
     */
    protected void Reader_Connect() {
        //メニュー画面を開く
        OpenMainActivity();
    }

    public void OnScannerAppeared() {
    }


    /**
     * RFIDリーダー切断イベント
     */

    public void onDisconnectEvent() {

    }

    /**
     * 二度読みバッファをクリア
     */
    public void subClearDuplexBuffer() {

    }

    /**
     //     * 探索開始
     //     * @param strAssetID
     //     * @param strKey
     //     * @throws OperationFailureException
     //     * @throws InvalidUsageException
     //     */
    protected void Rfid_Search_Start(String strAssetID, final String strKey) throws Exception{
        //Beep音Create
        try {
            if (beepSoundSearch == null){
                beepSoundSearch = new BeepSoundSearch(this);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }

        _ArrayRssi.clear();
        messageHandler.post(r);
        isRFIDRead = true;
        showbeep = false;
    }

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
                    readPowerLevel =  readPowerLevel / _ArrayRssi.size() / 10.0f;
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
     * 探索終了
     */
    protected void Rfid_Search_Stop(){
        try {
            RFID_Read_Stop();
            messageHandler.removeCallbacks(r);

            //音を停止
            if (beepSoundSearch != null) {
                beepSoundSearch.Destory();
                beepSoundSearch = null;
            }
            isRFIDRead = false;
            showbeep = true;
        } catch (Exception ex) {
            ex.printStackTrace();
        }

    }

    public void startOrStopRFID(boolean isIfPostMsg) {
        if (isIfPostMsg) {
            if (!IdataApplication.getIdataApp().getIdataLib().startInventoryTag()) {
                return;
            }
        } else {
            if (! IdataApplication.getIdataApp().getIdataLib().stopInventory()) {
                return;
            }
        }
        GetRFIDThread.getInstance().setIfPostMsg(isIfPostMsg);
    }


    /**
     * RFID リーダーＯＮ
     *
     * @param prmParmChange パラメータ変更(=true)/変更なし(=false)
     * @param strKey
     */
    protected void RFID_Read_Start(boolean prmParmChange, final String strKey) {

        try {
           // IdataApplication.getIdataApp().getIdataLib().changeConfig(true);

            //フィルター解除
            IdataApplication.getIdataApp().getIdataLib().filterSet(IdataApplication.getIdataApp().getIdataLib().EPC,0,0,"",true);

            int power=  Integer.parseInt(C_Setting.getBTReadIntensity(this, strKey));  //设置功率
            int session=   Integer.parseInt( C_Setting.getBTReadSession(this,strKey)); //这个SESSION
          if(IdataApplication.getIdataApp().getIdataLib().powerSet(power) && IdataApplication.getIdataApp().getIdataLib().sessionModeSet(session)) {
              startOrStopRFID(true);
              rfidThread.resumeThread();
          }
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
            C_Progres.StopProgressDialog();
        }
    }

    /**
     * RFID リーダーＯＦＦ
     */
    protected void RFID_Read_Stop() {
        try {
            if (blnConnectOpenFlag) {
                if (isCommScanner()) {
                    rfidThread.pauseThread();
                    startOrStopRFID(false);
                    //音を停止
                    if (beepSound != null) {
                        beepSound.Destory();
                        beepSound = null;
                    }
                }
            }
//            isRFIDRead = false;
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    protected void RFID_Setting_Start() {

    }

    public boolean isCommScanner() {
            return true;
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

            int stage2MaxReadPowerLevel = getResources().getInteger(R.integer.idata_stage2_max_read_power_level_on_search);
            int stage3MaxReadPowerLevel = getResources().getInteger(R.integer.idata_stage3_max_read_power_level_on_search);
            int stage4MaxReadPowerLevel = getResources().getInteger(R.integer.idata_stage4_max_read_power_level_on_search);
            int stage5MaxReadPowerLevel = getResources().getInteger(R.integer.idata_stage5_max_read_power_level_on_search);
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

            int stage2MaxReadPowerLevel = getResources().getInteger(R.integer.idata_list_stage2_max_read_power_level_on_search);
            int stage3MaxReadPowerLevel = getResources().getInteger(R.integer.idata_list_stage3_max_read_power_level_on_search);
            int stage4MaxReadPowerLevel = getResources().getInteger(R.integer.idata_list_stage4_max_read_power_level_on_search);
            int stage5MaxReadPowerLevel = getResources().getInteger(R.integer.idata_list_stage5_max_read_power_level_on_search);
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
        isRFIDRead = true;
    }

    /**
     * タグ書込み
     * @param strOldEpc 既存EPC
     * @param strNewEpc 新規EPC
     * @param strOldPc 新規PC     ※iDataではこの値は使用できない
     * @param newEpcBitSize 新規EPCサイズ
     */
    protected void WriteTag(String strOldEpc, String strNewEpc, String strOldPc, int newEpcBitSize) throws RFIDPkgException {
        try {
            //PC値を取得するため、もう一度タグ読取
            //※iDataではインベントリ時にPCを取得できないので、他機種とは処理の流れが異なり、書込み時にPC値を取得する
            String oldPc = ReadPcFromEpcMem(strOldEpc);

            int oldTagStartPos = CommonConst.EPC_BIT_TYPE_START_POS;        //フィルタリング開始位置(ビット単位)	※EPCの先頭を指定
            int newTagStartPos = CommonConst.EPC_WORD_TYPE_START_POS;       //書込み開始位置(EPCの先頭を指定している)
            String newPC = "";                                              //新規PC値
            String tagOldData = strOldEpc;                                  //書込み対象タグ

            //PCの書換が必要かどうかをチェック
            if (Common.needOverWritePc(newEpcBitSize, oldPc)) {
                //書換あり
                newPC = Common.calcTagPcHex(newEpcBitSize, oldPc);          //新規のPC値を取得
                tagOldData = oldPc + tagOldData;                            //書込み対象タグにPCを付与
                oldTagStartPos = CommonConst.PC_BIT_TYPE_START_POS;         //PCの先頭を指定(ビット単位)
                newTagStartPos = CommonConst.PC_WORD_TYPE_START_POS;        //PCの先頭を指定(Word単位)  ※1Word = 2Byte = 16bit
            }

            int oldTagLen = tagOldData.length() * 4;                        //フィルタリングサイズ(ビット単位)
            String tagNewData = newPC + strNewEpc;                          //書込み用の値(PC + EPC)

            //対象とするデータの長さを指定
            //※96bitの場合は6（4 * 6 = Hex24桁）
            //※128bitの場合は8（4 * 8 = Hex32桁）
            int newTagLen = tagNewData.length() / 4;

            //対象タグを指定して、タグ書込みを実行
            boolean status = IdataApplication.getIdataApp().getIdataLib().writeTag(CommonConst.TAG_DEFAULT_ACCESS_PWD,
                                                                                   IdataApplication.getIdataApp().getIdataLib().EPC,
                                                                                   oldTagStartPos,
                                                                                   oldTagLen,
                                                                                   tagOldData,
                                                                                   IdataApplication.getIdataApp().getIdataLib().EPC,
                                                                                   newTagStartPos,
                                                                                   newTagLen,
                                                                                   tagNewData);

            try {
                if (beepSound == null) {
                    beepSound = new BeepSound(this);
                }
                beepSound.BeepWrite();                  //書込み音を鳴らす
            } catch (Exception e) {
                e.printStackTrace();
            }

            //書込みに失敗していれば、エラーログを作成してエラーを発生させる
            if(!status){
                LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                        SystemSetting.getString(CommonConst.VIEW_WORD_LINK_REGISTER_BUTTON, "", this) + ": 書込タグの資産IDチェックエラー", this);
                throw new RFIDPkgException(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_WRITE, "", this));
            }
        }  catch (Exception e ) {
            //書込みに失敗した場合
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_LINK_REGISTER_BUTTON, "", this) + ":" + Common.toString(e), this);
            e.printStackTrace();
            throw new RFIDPkgException(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_WRITE, "", this));
        }
    }

    /**
     * EPC領域からPC値を取得します
     * @param filterEpc 読取対象のEPC
     * @return PC
     */
    protected String ReadPcFromEpcMem(String filterEpc) {
        String accessPass = CommonConst.TAG_DEFAULT_ACCESS_PWD;         //アクセスパスワード
        int filterStartPos = CommonConst.EPC_BIT_TYPE_START_POS;        //フィルタリング開始位置(ビット単位)	※EPCの先頭を指定
        int filterSize = filterEpc.length() * 4;                        //フィルタリングサイズ(ビット単位)

        //EPC領域内のPC値のみ読取
        String pc = IdataApplication.getIdataApp().getIdataLib().readTag(
                accessPass,
                IdataApplication.getIdataApp().getIdataLib().EPC,
                filterStartPos,
                filterSize,
                filterEpc,
                IdataApplication.getIdataApp().getIdataLib().EPC,
                CommonConst.PC_WORD_TYPE_START_POS,
                CommonConst.PC_WORD_TYPE_START_POS);

        if (TextUtils.isEmpty(pc)) {
            //取得失敗の場合は空白で返す
            return "";
        }

        return pc;
    }

    /**
     * 読取した探索データを取得する
     * @param strTagID
     * @return
     */
    protected String ReadSearchTag(String strTagID, String strKey){
        String strRead = "";
        if (LinkTagList.containsKey(strTagID)) {
            strRead = String.valueOf(LinkTagList.get(strTagID) / 10.0f);
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
            //commScanner.buzzer(CommConst.CommBuzzerType.B3);
        } catch (Exception ex) {
            Log.d(CommonConst.DIALOG_ERROR_TITLE, ex.getMessage());
        }
    }

    protected boolean ConnectStatus() {
        return _finalSuccessFlag;
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

    public static final String RES_ACTION = "android.intent.action.SCANRESULT";
    private IntentFilter mIntentFilter;

    private BroadcastReceiver BarcodeReceiver = new BroadcastReceiver() {

        @Override
        public void onReceive(Context context, Intent intent) {
            // TODO Auto-generated method stub

            if (intent.getAction().equals(RES_ACTION)){
                try {
                    //获取扫描结果value
                    String valueName = "value";
                    final String scanResult = intent.getStringExtra(valueName);
                    if(!scanResult.isEmpty()) {
                        BarcodeDataReceived(scanResult);
                        scannerInterface.scan_stop();
                    }

                }catch (Exception ex){

                }
            }

        }
    };


    public class EmshStatusBroadcastReceiver extends BroadcastReceiver {

        @Override
        public void onReceive(Context context, Intent intent) {

            //BroadCast取得情報のログ出力
            if (BuildConfig.DEBUG) {
                Bundle extras = intent.getExtras();
                System.out.println("BroadCast取得情報");
                if (extras != null && !extras.isEmpty()) {
                    System.out.println("+ Bundle.toString: " + extras.toString() + "");

                    for (final String key : extras.keySet()) {
                        System.out.println("+ Key Name: " + key + "　　Value: " + extras.get(key).toString());
                    }
                }
            }

            //UPD Start Sato 2022/9/22
            if (intent.getAction().equals(Intent.ACTION_BATTERY_CHANGED)) {
                //◆Android端末の通知情報
                //充電状況保持
                _androidBatteryStatus = intent.getIntExtra(BatteryManager.EXTRA_STATUS, -1);

            } else if (intent.getAction().equals(EmshConstant.Action.INTENT_EMSH_BROADCAST)) {
                //◆RFIDリーダーの通知情報

                //バッテリー残量レベル保持
                _scannerBatteryLevel = intent.getIntExtra("BatteryCapacityLevel", -1);

                int sessionStatus = intent.getIntExtra("SessionStatus", 0);
                int batteryPowerMode = intent.getIntExtra("BatteryPowerMode", -1);
                //  MLog.e("sessionStatus = " + sessionStatus + "  batteryPowerMode  = " + batteryPowerMode);
                if ((sessionStatus & EmshConstant.EmshSessionStatus.EMSH_STATUS_POWER_STATUS) != 0) {
                    // 把枪电池当前状态
                    if (batteryPowerMode ==IdataApplication.oldStatue) { //相同状态不处理
                        MUtil.cancelWaringDialog();
                        return;
                    }
                    IdataApplication.oldStatue = batteryPowerMode;

                    switch (batteryPowerMode) {
                        case EMSH_PWR_MODE_CHG_GENERAL:
                        case EMSH_PWR_MODE_CHG_FULL:
                        case EMSH_PWR_MODE_CHG_QUICK:
                        case EMSH_PWR_MODE_STANDBY:
                            IdataApplication.getIdataApp().getIdataLib().powerOn();
                            break;
                        case EMSH_PWR_MODE_DSG_UHF:
                            connectReader=true;
                            invalidateOptionsMenu();
                            if (beepSound == null) {
                                beepSound = new BeepSound(BaseRFIDActivity.this);
                            }
                            beepSound.BeepConnect();
                            break;
                        default:
                            connectReader=false;
                            invalidateOptionsMenu();
                            if (beepSound == null) {
                                beepSound = new BeepSound(BaseRFIDActivity.this);
                            }
                            beepSound.BeepConnect();
                            break;
                    }
                } else {
                    IdataApplication.oldStatue = EMSH_PWR_MODE_BATTERY_ERROR;
                    connectReader=false;
                    invalidateOptionsMenu();
                }
            }
            //UPD End Sato 2022/9/22
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

    /**
     * RFID リーダーＯＮ(探索一覧動作)
     * @param strAssetID 資産ID
     * @param strKey　キー
     */
    protected void Rfid_Search_List_Start(String strAssetID, String strKey){
        try {
            showbeep = false;
            RFID_Read_Start(false,strKey);
        } catch (Exception ex) {
            ex.printStackTrace();
        }
    }

    /**
     * バッテリー残量が低下した際にエラーメッセージを表示する
     * ※ ADD Sato 2022/9/22
     */
    private void batteryLevelCheck() {
        try {
            //Android端末とRFIDリーダーから必要な情報が取得できていない場合はチェックなし
            //※_scannerBatteryLevelが0の場合はRFIDリーダと接続されていない場合の想定
            if (_scannerBatteryLevel <= 0) return;
            if (_androidBatteryStatus == -1) return;

            //Android端末が充電状態の場合チェックしない
            if (_androidBatteryStatus == BatteryManager.BATTERY_STATUS_CHARGING) return;

            if (_scannerBatteryLevel <= 1) {
                //バッテリー残量レベルが1以下
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_BATTERY_CHARGE, getString(R.string.battery_low), this),
                        getString(R.string.battery));
            }
        } catch (Exception ex) {
            ex.printStackTrace();
        }
    }

    /**
     * 電波強度の設定可能範囲値を作成
     * @return 設定可能範囲
     */
    protected String[] SetIntensityRange() {
        // ※この機種は設定画面で電波強度範囲を設定しているため、このメソッドは使用しない。
        return new String[1];
    }
}