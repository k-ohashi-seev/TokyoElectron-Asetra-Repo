package com.rfid.common;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.media.AudioManager;
import android.util.Log;
import android.widget.Toast;

import com.cipherlab.rfid.BeepType;
import com.cipherlab.rfid.DeviceVoltageInfo;
import com.cipherlab.rfid.GeneralString;
import com.cipherlab.rfid.ClResult;
import com.cipherlab.rfid.DeviceResponse;
import com.cipherlab.rfid.Gen2Settings;
import com.cipherlab.rfid.InventoryType;
import com.cipherlab.rfid.NotificationParams;
import com.cipherlab.rfid.QValue;
import com.cipherlab.rfid.RFIDMemoryBank;
import com.cipherlab.rfid.RFIDMode;
import com.cipherlab.rfid.RfidEpcFilter;
import com.cipherlab.rfid.RfidOutputConfiguration;
import com.cipherlab.rfid.ScanMode;
import com.cipherlab.rfid.SessionSettings;
import com.cipherlab.rfid.SwitchMode;
import com.cipherlab.rfidapi.RfidManager;

/**
 * サイファーラボ製 RS35 UHF RFIDリーダのSDKを扱うクラスです。
 * <ui>
 *     <li>タグ読取に関する処理を定義します</li>
 * </ui>
 */
public class WrapRfidSdk {
    /** クラスインスタンス */
    private static WrapRfidSdk _instance;

    /** 処理ロックオブジェクト */
    private final Object _readerLock = new Object();

    /** RFIDリーダー操作変数 */
    private RfidManager _RFIDReader;

    /** RFIDサービス開始フラグ */
    private boolean _isServiceConnected = false;

    /** 読取中判別フラグ(True:読取中　False:読取未開始) */
    private boolean _isReading = false;

    /** タグ書込み成否フラグ */
    private Boolean _isWriteSuccess = null;

    /** コールバック */
    private RFIDInterface _rfidCallback;

    /** デバイスバッテリー情報 */
    private final DeviceVoltageInfo _deviceVoltageInfo = new DeviceVoltageInfo();

    /** タグ読取状態 */
    private TagReadMode _readMode = TagReadMode.NONE;

    /** タグ読取状態定義 */
    private enum TagReadMode {
        NONE,
        TRIGGER_READ,
        AUTO_READ
    }

    /**
     * コンストラクタ
     * ※直接インスタンス化できないようにPrivateで宣言
     */
    private WrapRfidSdk() {}

    /**
     * クラスインスタンス返却
     * @return クラスインスタンス
     */
    public static synchronized WrapRfidSdk getInstance() {
        if (_instance == null) {
            _instance = new WrapRfidSdk();
        }
        return _instance;
    }

    /**
     * RFIDリーダー リソース生成
     * @param context コンテキスト
     *※引数には getApplicationContext() で取得したアプリケーションコンテキストを指定する。
     */
    public void createRFIDReader(Context context) {
        synchronized (_readerLock) {
            try {
                if (_RFIDReader == null) {
                    _RFIDReader = RfidManager.InitInstance(context);

                    //各通知を受信できるように登録
                    IntentFilter filter = new IntentFilter();
                    filter.addAction(GeneralString.Intent_RFIDSERVICE_CONNECTED);
                    filter.addAction(GeneralString.Intent_RFIDSERVICE_TAG_DATA);
                    filter.addAction(GeneralString.Intent_RFIDSERVICE_EVENT);
                    context.registerReceiver(rfidReceiver, filter);
                }
            } catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                throw ex;
            }
        }
    }

    /** RFIDリーダー リソース解放 */
    public void releaseRFIDReader(Context context) {
        synchronized (_readerLock) {
            try {
                if (_RFIDReader != null) {
                    _RFIDReader.Release();
                    _RFIDReader = null;
                    context.unregisterReceiver(rfidReceiver);
                }
            } catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                throw ex;
            }
        }
    }

    /**
     * RFIDリーダのバッテリー情報を取得
     * @return デバイスバッテリー情報
     */
    public DeviceVoltageInfo getBatteryPercent() {
        try {
            //バッテリー情報を取得
            if (_RFIDReader.GetBatteryLifePercent(_deviceVoltageInfo) != ClResult.S_OK.ordinal()) {
                return null;
            }
            return _deviceVoltageInfo;
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            return null;
        }
    }

    /**
     * RFIDリーダの装着状態を取得
     * @return True:装着済み　False:未装着
     */
    public boolean getRFIDReaderAttached() {
        if (_isServiceConnected) {
            //RFIDサービスと接続されている場合
            return _RFIDReader != null &&_RFIDReader.GetConnectionStatus();
        } else {
            //RFIDサービスと接続されていない場合
            return _RFIDReader != null;
        }
    }

    /**
     * トリガーの反応を制御します。
     * @param enableTrigger True:活性 False:非活性
     */
    public void setEnableTrigger(boolean enableTrigger) {
        if (_RFIDReader == null) return;

        //トリガー非活性
        _RFIDReader.EnableDeviceTrigger(enableTrigger);
    }

    /**
     * RFIDリーダの読取モードを変更します。
     * @param isRFIDMode True:タグ読取 False:バーコード/QR読取(トリガーを引くとAndroid端末からレーザーが照射される)
     */
    public void setReaderReadModeSwitch(boolean isRFIDMode) {
        if (_RFIDReader == null) return;

        //RFIDリーダが準備中の場合
        if (!_isServiceConnected) {
            return;
        }

        //現在適用中の読取モードを取得
        //※タグ読取モードの場合は1、バーコード/QR読取の場合は0を返す。それ以外の場合は-1
        int status = _RFIDReader.GetRFIDSwitchStatus();
        if (status == -1) return;

        //変更対象のモードが現在適用中の場合、何もしない
        if ((status == 1) == isRFIDMode) {
            return;
        }

        //RFIDリーダの読取モードを切替
        _RFIDReader.SetRFIDSwitchStatus(isRFIDMode);
    }

    /**
     * RFIDリーダのスキャン操作を終了する
     * ※RS35はスキャンモードを変更すると、スキャンを停止する
     */
    private void scanPowerOff() {
        if (_RFIDReader == null) return;

        //現在適用中のスキャンモードを取得
        ScanMode scanMode = _RFIDReader.GetScanMode();

        //スキャンモードを変更して、元に戻す
        _RFIDReader.SetScanMode(ScanMode.Single);
        _RFIDReader.SetScanMode(scanMode);
    }

    /**
     * 二度読み許可
     * @param isDupAllow True:二度読み可能　False:二度読み不可
     */
    public void setFilterDuplicate(boolean isDupAllow) throws Exception {
        //Trueの場合は1をセット、それ以外は0をセット
        if (_RFIDReader.SetFilterDuplicate((isDupAllow ? 1 : 0)) != ClResult.S_OK.ordinal()) {
            throw new Exception(_RFIDReader.GetLastError());
        }
    }

    /**
     * 電波強度を設定
     * @param power 電波強度
     */
    public void setTxPower(int power) throws Exception {
        if (_RFIDReader.SetTxPower(power) != ClResult.S_OK.ordinal()) {
            throw new Exception(_RFIDReader.GetLastError());
        }
    }

    /**
     * タグに関する設定
     * @param prmSession セッション
     */
    public void setGetGen2(String prmSession) throws Exception {
        Gen2Settings settings = new Gen2Settings();

        //セッション設定
        SessionSettings session = SessionSettings.S1;
        switch (prmSession) {
            case "0":
                session = SessionSettings.S0;
                break;
            case "1":
                session = SessionSettings.S1;
                break;
            case "2":
                session = SessionSettings.S2;
                break;
            case "3":
                session = SessionSettings.S3;
                break;
        }
        settings.Session = session;

        //設定適用
        if (_RFIDReader.SetGen2(settings) != ClResult.S_OK.ordinal()) {
            throw new Exception(_RFIDReader.GetLastError());
        }
    }

    /**
     * Q値を設定します。
     * @param qValue Q値
     */
    public void setQValue(short qValue) throws Exception {
        //設定用QValueを準備
        QValue settingQVal = new QValue();
        settingQVal.Dynamic = false;
        settingQVal.value = qValue;
        settingQVal.Min = qValue;
        settingQVal.Max = qValue;

        if (_RFIDReader.SetQValue(settingQVal) != ClResult.S_OK.ordinal()) {
            throw new Exception(_RFIDReader.GetLastError());
        }
    }

    /**
     * EPCの先頭からを対象とするフィルターを設定します。
     * ※フィルターに一致するEPCのみ読取するようになる
     * @param hexMask フィルタリングする値(16進数文字列)
     */
    public void setIncludeEpcTopFilter(String hexMask) throws Exception {
        RfidEpcFilter filter = new RfidEpcFilter();
        filter.Enable = 1;                                          //フィルター有効
        filter.Startbit_MSB = 0;                                    //EPCの先頭を開始位置とする
        filter.Startbit_LSB = 0;                                    //EPCの先頭を開始位置とする
        filter.PatternLength_MSB = 0;                               //セットする値の長さ
        filter.PatternLength_LSB = (byte)(hexMask.length() * 4);    //セットする値の長さ(96bitや128bitなど)
        filter.EPCPattern1 = hexMask;                               //フィルタリング値

        //フィルター適用
        if (_RFIDReader.SetIncludedEPCFilter(filter) != ClResult.S_OK.ordinal()) {
            throw new Exception(_RFIDReader.GetLastError());
        }
    }

    /**
     * フィルターを設定をクリアします。
     */
    public void clearIncludeEpcFilter() throws Exception {
        RfidEpcFilter filter = new RfidEpcFilter();
        filter.Enable = 0;                              //フィルター無効

        //フィルター適用
        if (_RFIDReader.SetIncludedEPCFilter(filter) != ClResult.S_OK.ordinal()) {
            throw new Exception(_RFIDReader.GetLastError());
        }
    }

    /**
     * 読取バッファをクリア
     */
    public void clearFilterDuplicate() throws Exception {
        if (_RFIDReader == null) return;
        if (_RFIDReader.ClearFilterDuplicate() != ClResult.S_OK.ordinal()) {
            throw new Exception(_RFIDReader.GetLastError());
        }
    }

    /**
     * トリガー押下でのインベントリ開始(読取)
     */
    public void triggerInventoryStart() throws Exception {
        synchronized (_readerLock) {
            try {
                if (!isReading()) {
                    //RFIDモードをインベントリに設定
                    if (_RFIDReader.SetRFIDMode(RFIDMode.Inventory) != ClResult.S_OK.ordinal()) {
                        throw new Exception(_RFIDReader.GetLastError());
                    }

                    //トリガー押下時、タグを読み取る
                    if (_RFIDReader.SetScanMode(ScanMode.Continuous) != ClResult.S_OK.ordinal()) {
                        throw new Exception(_RFIDReader.GetLastError());
                    }

                    //トリガー活性
                    _RFIDReader.EnableDeviceTrigger(true);

                    //読取実行中フラグON
                    _isReading = true;
                    _readMode = TagReadMode.TRIGGER_READ;
                }
            } catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                throw ex;
            }
        }
    }

    /**
     * トリガー押下でのインベントリ終了
     */
    public void triggerInventoryStop() throws Exception {
        synchronized (_readerLock) {
            try {
                if (isReading()) {
                    //モードが異なる場合
                    if (_readMode != TagReadMode.TRIGGER_READ) return;

                    //トリガー非活性
                    _RFIDReader.EnableDeviceTrigger(false);

                    //読取実行中フラグOFF
                    _isReading = false;
                    _readMode = TagReadMode.NONE;
                }
            } catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                throw ex;
            }
        }
    }

    /**
     * トリガー押下なしのインベントリ開始(読取)
     */
    public void autoInventoryStart() throws Exception {
        // 20230919 UPD Sato Start
        //254は読取最大数
//        if (_RFIDReader.RFIDDirectStartInventoryRound(InventoryType.EPC, 254) != ClResult.S_OK.ordinal()) {
//            throw new Exception("RFIDDirectStartInventoryRound Error:" + _RFIDReader.GetLastError());
//        }

        // トリガー押下をエミュレートして読取開始
        if (_RFIDReader.SoftScanTrigger(true) != ClResult.S_OK.ordinal()) {
            throw new Exception("SoftScanTrigger Start Error:" + _RFIDReader.GetLastError());
        }
        // 20230919 UPD Sato End

        //読取実行中フラグON
        _isReading = true;
        _readMode = TagReadMode.AUTO_READ;
    }

    /**
     * トリガー押下なしのインベントリ終了
     */
    public void autoInventoryStop() throws Exception {
        //モードが異なる場合
        if (_readMode != TagReadMode.AUTO_READ) return;

        // 20230919 UPD Sato Start
//        _RFIDReader.RFIDDirectCancelInventoryRound();

        // トリガー押下をエミュレートして読取停止
        if (_RFIDReader.SoftScanTrigger(false) != ClResult.S_OK.ordinal()) {
            throw new Exception("SoftScanTrigger Stop Error:" + _RFIDReader.GetLastError());
        }
        // 20230919 UPD Sato End

        //読取実行中フラグOFF
        _isReading = false;
        _readMode = TagReadMode.NONE;
    }

    /**
     * タグ書込み
     * @param password アクセスパスワード
     * @param epc 書込み対象のEPC
     * @param bank メモリ領域
     * @param start 書込み開始位置
     * @param retry リトライ回数
     * @param data 書込みデータ
     */
    public void writeTagEpc(byte[] password, byte[] epc, RFIDMemoryBank bank, int start, int retry, byte[] data) throws Exception {
        synchronized (_readerLock) {
            if (!isReading()) {
                //書込み実施
                DeviceResponse response = _RFIDReader.RFIDDirectWriteTagByEPC(password, epc, bank, start, retry, data);
                if (response != DeviceResponse.OperationSuccess) {
                    throw new Exception("Tag Write Failure：" + response);
                }

                try {
                    _isWriteSuccess = null;
                    long timeout = 5000L;
                    long elapsed = 0L;

                    //書込み結果が通知されるまで待機
                    while(_isWriteSuccess == null) {
                        //5秒経過すればタイムアウトとみなす
                        if (elapsed >= timeout) {
                            throw new Exception("Tag Write Failure：Timeout");
                        }

                        //一定間隔でスリープ
                        Thread.sleep(100L);
                        elapsed += 100L;
                    }

                    //書込みに失敗していれば例外を発生させる
                    if (!_isWriteSuccess) {
                        throw new Exception("Tag Write Failure");
                    }
                } catch (Exception e) {
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                    throw e;
                } finally {
                    //書込み失敗時、RFIDリーダが熱を持ち続けないようにRFIDリーダのスキャンモードを変更して強制的に動作を止める
                    scanPowerOff();
                }
            }
        }
    }

    /**
     * 読取実行中かどうか確認します。
     * @return True:読取実行中　False:読取未開始
     */
    public boolean isReading() {
        return _isReading;
    }

    /**
     * 読取イベントのCallBack先を設定
     * @param listener 参照
     */
    public void setCallBackListener(RFIDInterface listener) {
        synchronized (_readerLock) {
            _rfidCallback = listener;
        }
    }

    /** 読取イベントのCallBack先設定を削除 */
    public void releaseCallBackListener() {
        synchronized (_readerLock) {
            _rfidCallback = null;
        }
    }

    /**
     * 通知を受信するためのレシーバー
     */
    private final BroadcastReceiver rfidReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            try {
                switch (intent.getAction()) {
                    //◆RFID関連の操作が可能になった段階で通知
                    case GeneralString.Intent_RFIDSERVICE_CONNECTED:
                        _isServiceConnected = true;

                        //RFIDリーダの初期設定
                        setEnableTrigger(false);                          //トリガー非活性
                        setInitNotifyConfig();                            //ブザー鳴動・LED設定
                        setInitOutputConfig();                            //読取データアウトプット設定
                        setInitTriggerSwitchMode();                       //トリガー高速スイッチングモード設定
                        break;

                    //◆RFIDタグ処理結果受信
                    case GeneralString.Intent_RFIDSERVICE_TAG_DATA:
                        int response = intent.getIntExtra(GeneralString.EXTRA_RESPONSE, -1);
                        int type = intent.getIntExtra(GeneralString.EXTRA_DATA_TYPE, -1);

                        //応答結果ごとに処理
                        switch (response) {
                            case GeneralString.RESPONSE_OPERATION_SUCCESS:
                                //処理成功
                                if (type == GeneralString.TYPE_NORMAL_SCAN || type == GeneralString.TYPE_INVENTORY_EPC_SCAN) {
                                    //読取データの場合
                                    String pc = intent.getStringExtra(GeneralString.EXTRA_PC);
                                    String epc = intent.getStringExtra(GeneralString.EXTRA_EPC);
                                    double rssi = intent.getDoubleExtra(GeneralString.EXTRA_DATA_RSSI, 0);

                                    //EPCとRSSIを渡して、読取イベントを発生させる
                                    if (_rfidCallback != null) {
                                        _rfidCallback.onReadResultReceived(epc.toUpperCase(), rssi, pc);
                                    }
                                }
                                break;

                            case GeneralString.RESPONSE_OPERATION_FINISH:
                                if (type == GeneralString.TYPE_WRITE_TAG) {
                                    //タグ書込みの場合
                                    _isWriteSuccess = true;
                                }
                                break;

                            case GeneralString.RESPONSE_OPERATION_TIMEOUT_FAIL:
                            case GeneralString.RESPONSE_PASSWORD_FAIL:
                            case GeneralString.RESPONSE_OPERATION_FAIL:
                            case GeneralString.RESPONSE_DEVICE_BUSY:
                                if (type == GeneralString.TYPE_WRITE_TAG) {
                                    //タグ書込みの場合
                                    _isWriteSuccess = false;
                                }
                        }
                        break;

                    //◆各種RFID関連のイベントを通知
                    case GeneralString.Intent_RFIDSERVICE_EVENT:
                        break;
                }
            } catch (Exception e) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
            }
        }
    };

    /**
     * 読取データアウトプット設定を行います。
     */
    private void setInitOutputConfig() {
        //端末に適用されている設定を取得
        RfidOutputConfiguration outputConfig = new RfidOutputConfiguration();
        if (_RFIDReader.GetDataOutputSettings(outputConfig) != ClResult.S_OK.ordinal()) {
            Log.e("読取データアウトプット設定", "読取データアウトプット設定に失敗");
            return;
        }

        //アプリで設定しておきたい項目のみ変更
        outputConfig.szEPCPrefixCode = "";              //前置文字(読取データの先頭に何も付与しない)
        outputConfig.szEPCSuffixCode = "";              //後置文字(読取データの末尾に何も付与しない)

        //設定適用
        _RFIDReader.SetDataOutputSettings(outputConfig);
    }

    /**
     * ブザー鳴動・LEDに関する設定を行います。
     */
    private void setInitNotifyConfig() {
        //端末に適用されている設定を取得
        NotificationParams notifyConfig = new NotificationParams();
        if (_RFIDReader.GetNotification(notifyConfig) != ClResult.S_OK.ordinal()) {
            Log.e("ブザー鳴動・LED設定", "ブザー鳴動・LED設定に失敗");
            return;
        }

        //アプリで設定しておきたい項目のみ変更
        notifyConfig.ReaderBeep = BeepType.Mute;        //RFIDリーダの読取音をミュート

        //設定適用
        _RFIDReader.SetNotification(notifyConfig);
    }

    /**
     * トリガーの高速スイッチングモードを設定します。
     */
    private void setInitTriggerSwitchMode() {
        //高速スイッチングモードがONの場合は、OFF変更
        if (_RFIDReader.GetTriggerSwitchMode().TriggerSwitchStatus) {
            _RFIDReader.SetTriggerSwitchMode(false);
        }
    }
}
