package com.rfid.common;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.IBinder;
import android.os.RemoteException;
import android.util.Log;

import com.cipherlab.barcode.GeneralString;
import com.cipherlab.barcode.ReaderManager;
import com.cipherlab.barcode.decoder.KeyboardEmulationType;
import com.cipherlab.barcode.decoder.OutputEnterChar;
import com.cipherlab.barcode.decoder.OutputEnterWay;
import com.cipherlab.barcode.decoderparams.ReaderOutputConfiguration;
import com.cipherlab.barcodebase.ReaderCallback;

/**
 * サイファーラボ製 RS35のSDKを扱うクラスです。
 * <ui>
 *     <li>バーコード読取に関する処理を定義します</li>
 * </ui>
 */
public class WrapBcdSdk {
    /** クラスインスタンス */
    private static WrapBcdSdk _instance;

    /** Android読取制御クラス変数 */
    private ReaderManager _readerManager;

    /** Android端末コールバック */
    private BarcodeInterface _bcdCallback;

    /** 読取開始済みフラグ */
    private boolean _isReading = false;

    /** コールバック */

    /**
     * コンストラクタ
     * ※直接インスタンス化できないようにPrivateで宣言
     */
    private WrapBcdSdk() {}

    /**
     * クラスインスタンス返却
     * @return クラスインスタンス
     */
    public static synchronized WrapBcdSdk getInstance() {
        if (_instance == null) {
            _instance = new WrapBcdSdk();
        }
        return _instance;
    }

    /**
     * リソース生成
     * @param context コンテキスト
     *※引数には getApplicationContext() で取得したアプリケーションコンテキストを指定する。
     */
    public void createScanReader(Context context) throws Exception {
        if (_readerManager == null) {
            _readerManager = ReaderManager.InitInstance(context);

            //各通知を受信できるように登録
            IntentFilter filter = new IntentFilter();
            filter.addAction(GeneralString.Intent_SOFTTRIGGER_DATA);
            filter.addAction(GeneralString.Intent_PASS_TO_APP);
            filter.addAction(GeneralString.Intent_READERSERVICE_CONNECTED);         //バーコードリーダが使用可能になったタイミングで通知
            context.registerReceiver(dataReceiver, filter);
        }
    }

    /** リソース解放 */
    public void releaseScanReader(Context context) {
        if (_readerManager != null) {
            _readerManager.Release();
            _readerManager = null;
            context.unregisterReceiver(dataReceiver);
        }
    }

    /** 読取開始処理 */
    public void startRead() throws Exception {
        try {
            _readerManager.SoftScanTrigger();
            _isReading = true;
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            throw ex;
        }
    }

    /** 読取停止処理 */
    public void stopRead() {
        try {
            _isReading = false;
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            throw ex;
        }
    }

    /** 読取機能非活性処理(Android端末のトリガーに反応しなくなる) */
    public void lockScanner() {
        _readerManager.SetActive(false);
    }

    /** 読取機能活性処理(Android端末のトリガーに反応するようになる) */
    public void unLockScanner() {
        _readerManager.SetActive(true);
    }

    /**
     * 読取イベントのCallBack先を設定
     * @param listener 参照
     */
    public void setCallBackListener(BarcodeInterface listener) {
        _bcdCallback = listener;
    }

    /** 読取イベントのCallBack先設定を削除 */
    public void releaseCallBackListener() {
        _bcdCallback = null;
    }

    /**
     * 読取実行中かどうか確認します。
     * @return True:読取実行中　False:読取未開始
     */
    public boolean isReading() {
        return _isReading;
    }

    /**
     * 通知を受信するためのレシーバー
     */
    private final BroadcastReceiver dataReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            switch (intent.getAction()) {
                case GeneralString.Intent_SOFTTRIGGER_DATA:     //ソフトウェアトリガーでの読取
                case GeneralString.Intent_PASS_TO_APP:          //ハードウェアトリガーでの読取
                    //◆読取イベント
                    //読取データを通知
                    String readData = intent.getStringExtra(GeneralString.BcReaderData);
                    if (_bcdCallback != null) {
                        _bcdCallback.onBcdDataReceived(readData);
                    }
                    break;

                case GeneralString.Intent_READERSERVICE_CONNECTED:
                    //◆バーコードリーダが使用可能になると通知
                    //バーコード読取データアウトプット設定
                    ReaderOutputConfiguration settings = new ReaderOutputConfiguration();
                    settings.enableKeyboardEmulation = KeyboardEmulationType.None;              //読取データをIntent経由で受信する
                    settings.autoEnterWay = OutputEnterWay.Disable;                             //自動でEnterを付与しない
                    settings.autoEnterChar = OutputEnterChar.None;                              //自動Enterなし
                    _readerManager.Set_ReaderOutputConfiguration(settings);
                    break;
            }
        }
    };
}
