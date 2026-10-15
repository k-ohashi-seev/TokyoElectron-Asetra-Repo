package com.rfid.common;

import android.content.Context;
import android.util.Log;

import com.keyence.autoid.sdk.SdkStatus;
import com.keyence.autoid.sdk.deviceinfo.DeviceInfo;
import com.keyence.autoid.sdk.deviceinfo.UnitInfo;
import com.keyence.autoid.sdk.scan.DecodeResult;
import com.keyence.autoid.sdk.scan.ScanManager;

/**
 * キーエンス製 DX-A600/DX-A400(Android端末)のQR/Barcode関連のSDKを扱うクラスです。
 * <ui>
 *     <li>QR/バーコード読取に関する処理を定義します</li>
 * </ui>
 */
public class WrapDxBcd implements ScanManager.DataListener {
    /** クラスインスタンス */
    private static WrapDxBcd _instance;

    /** 処理ロックオブジェクト */
    private final Object _readerLock = new Object();

    /** Android読取制御クラス変数 */
    private ScanManager _scanManager;

    /** Android端末情報クラス変数 */
    private DeviceInfo _deviceInfo;

    /** RFIDリーダ装着フラグ */
    private boolean _isReaderConnected = false;

    /** コールバック */
    private DxBarcodeInterface _dxBarcode;

    /**
     * コンストラクタ
     * ※直接インスタンス化できないようにPrivateで宣言
     */
    private WrapDxBcd() {}

    /**
     * クラスインスタンス返却
     * @return クラスインスタンス
     */
    public static synchronized WrapDxBcd getInstance() {
        if (_instance == null) {
            _instance = new WrapDxBcd();
        }
        return _instance;
    }

    /**
     * リソース生成
     * @param context コンテキスト
     *※引数には getApplicationContext() で取得したアプリケーションコンテキストを指定する。
     */
    public void createScanReader(Context context) throws Exception {
        try {
            //読取制御クラスを使用可能にする
            if (_scanManager == null) {
                _scanManager = ScanManager.createScanManager(context);
                if (_scanManager == null) {
                    throw new Exception("createScanManager Failure");
                }

                //読取結果通知コールバック登録
                _scanManager.addDataListener(this);
            }

            //端末情報クラスを使用可能にする
            if (_deviceInfo == null) {
                _deviceInfo = DeviceInfo.createDeviceInfo(context);
            }

            //RFIDリーダとの装着状況を取得する
            UnitInfo unitInfo = new UnitInfo();
            SdkStatus status = _deviceInfo.getUnitInfo(unitInfo);
            if (status != SdkStatus.SUCCESS) {
                throw new Exception("Get UnitInfo Failure");
            }

            //RFIDリーダを装着していればTrue、未装着ならFalse
            _isReaderConnected = unitInfo.getUnitType().equals(UnitInfo.UnitType.DX_RH1) ||
                                 unitInfo.getUnitType().equals(UnitInfo.UnitType.DX_RM1);
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            throw ex;
        }
    }

    /** リソース解放 */
    public void releaseScanReader() {
        if (_scanManager != null) {
            _scanManager.removeDataListener(this);         //読取結果通知リスナー削除
            _scanManager.releaseScanManager();
            _scanManager = null;
        }
        if (_deviceInfo != null) {
            _deviceInfo.releaseDeviceInfo();
            _deviceInfo = null;
        }
    }

    /** RFIDリーダー装着有無チェック
     * ※ True:装着　False:未装着 */
    public boolean isReaderConnect() {
        return _isReaderConnected;
    }

    /** 読取開始処理 */
    public void startRead() throws Exception {
        try {
            SdkStatus status =  _scanManager.startRead();
            if (status != SdkStatus.SUCCESS) {
                //開始失敗
                Log.e(CommonConst.DIALOG_ERROR_TITLE, "エラーコード：" + status.toString() + " " + status.name());
                throw new Exception("読取開始エラー" + "\n" + " エラーコード：" + status.toString() + "\n" + status.name());
            }
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            throw ex;
        }
    }

    /** 読取停止処理 */
    public void stopRead() {
        try {
            if (_scanManager == null) return;

            if (_scanManager.isReading()) {
                //読取中であれば読取停止
                _scanManager.stopRead();
            }
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            throw ex;
        }
    }

    /** 読取機能停止処理(Android端末のトリガーに反応しなくなる) */
    public void lockScanner() {
        if (_scanManager != null) {
            _scanManager.lockScanner();
        }
    }

    /** 読取機能停止解除処理(Android端末のトリガーに反応するようになる) */
    public void unLockScanner() {
        if (_scanManager != null) {
            _scanManager.unlockScanner();
        }
    }

    /**
     * 読取イベントのCallBack先を設定
     * @param listener 参照
     */
    public void setCallBackListener(DxBarcodeInterface listener) {
        _dxBarcode = listener;
    }

    /** 読取イベントのCallBack先設定を削除 */
    public void releaseCallBackListener() {
        _dxBarcode = null;
    }

    /**
     * 読取実行中かどうか確認します。
     * @return True:読取実行中　False:読取未開始
     */
    public boolean isReading() {
        return _scanManager.isReading();
    }

    /**
     * バーコード読取イベント処理
     * @param decodeResult 読取結果パラメータ
     */
    @Override
    public void onDataReceived(DecodeResult decodeResult) {
        try {
            if (_dxBarcode != null) {
                _dxBarcode.onBcdDataReceived(decodeResult.getResult(),
                        decodeResult.getCodeType(),
                        decodeResult.getData(),
                        decodeResult.getRawData());
            }
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }
}
