package com.rfid.common;

import android.content.Context;
import android.util.Log;

import com.keyence.autoid.sdk.SdkStatus;
import com.keyence.autoid.sdk.rfid.Filter;
import com.keyence.autoid.sdk.rfid.RFIDReader;
import com.keyence.autoid.sdk.rfid.ReadResult;
import com.keyence.autoid.sdk.rfid.Status;
import com.keyence.autoid.sdk.rfid.params.CommonParams;
import com.keyence.autoid.sdk.rfid.params.InventoryParams;
import com.keyence.autoid.sdk.rfid.params.MemoryBank;
import com.keyence.autoid.sdk.rfid.params.SessionFlag;
import com.keyence.autoid.sdk.rfid.params.SingleReadParams;
import com.keyence.autoid.sdk.rfid.params.WriteIDParams;
import com.keyence.autoid.sdk.rfid.params.WriteParams;
import com.rfid.common.enumparam.EnSession;

/**
 * キーエンス製 DX-RH1及びDX-RM1(RFIDリーダ)のSDKを扱うクラスです。
 * <ui>
 *     <li>RFタグ読取に関する処理を定義します</li>
 * </ui>
 */
public class WrapDxRFID implements RFIDReader.ReadResultListener{
    /** クラスインスタンス */
    private static WrapDxRFID _instance;

    /** 処理ロックオブジェクト */
    private final Object _readerLock = new Object();

    /** RFIDリーダー操作変数 */
    private RFIDReader _RFIDReader;

    /** 読取中判別フラグ(True:読取中　False:読取未開始) */
    private boolean _isReading = false;

    /** リスタート用フィルター */
    private Filter _filter;

    /** リスタート用インベントリパラメータ */
    private InventoryParams _inventParams;

    /** コールバック */
    private DxRFIDInterface _dxRfid;

    /**
     * コンストラクタ
     * ※直接インスタンス化できないようにPrivateで宣言
     */
    private WrapDxRFID() {}

    /**
     * クラスインスタンス返却
     * @return クラスインスタンス
     */
    public static synchronized WrapDxRFID getInstance() {
        if (_instance == null) {
            _instance = new WrapDxRFID();
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
                    _RFIDReader = RFIDReader.createRFIDReader(context);

                    //読取結果が通知されるようにリスナー登録
                    _RFIDReader.addReadResultListener(this);

                    //デフォルト設定
                    CommonParams params = new CommonParams();
                    params.setDefault();
                    _RFIDReader.setConfig(params);

                    //個別読取無効設定
                    setDisableSingleRead();
                }
            } catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                throw ex;
            }
        }
    }

    /** RFIDリーダー リソース解放 */
    public void releaseRFIDReader() {
        synchronized (_readerLock) {
            try {
                if (_RFIDReader != null) {
                    _RFIDReader.removeReadResultListener(this);         //読取結果通知リスナー削除
                    _RFIDReader.releaseRFIDReader();
                    _RFIDReader = null;
                }
            } catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                throw ex;
            }
        }
    }

    /**
     * RFIDリーダに現在適用されている設定を取得
     * @param params RFIDリーダパラメータ
     */
    public CommonParams getConfig(CommonParams params) {
        _RFIDReader.getConfig(params);
        return params;
    }

    /** RFIDリーダー共通設定 */
    public void setReaderCommConfig(CommonParams params) throws Exception {
        if (_RFIDReader == null) {throw new Exception("Reader Config Error");}

        //設定適用
        _RFIDReader.setConfig(params);
    }

    /**
     * 個別読取無効設定
     */
    private void setDisableSingleRead() {
        if (_RFIDReader == null) {return;}

        // 既存の個別読み取り設定を取得
        SingleReadParams singleReadParams = new SingleReadParams();
        Status status = _RFIDReader.getConfig(singleReadParams);
        if (status != Status.STATUS_OK){
            throw new RuntimeException("Get SingleReadParams Failure");
        }

        // 個別読み取りを無効に設定
        singleReadParams.enabled = false;
        status = _RFIDReader.setConfig(singleReadParams);
        if (status != Status.STATUS_OK){
            throw new RuntimeException("Set SingleReadParams Disable Failure");
        }
    }

    /**
     * セッションフラグ取得処理
     * @param session セッション値
     * @return セッションフラグ
     */
    public static SessionFlag getSessionFlg(int session) {
        SessionFlag sessionFlag = null;
        EnSession enSession = EnSession.getEnSession(session);
        if (enSession == null) {
            return null;
        }

        switch (enSession) {
            case SESSION_0:
                return SessionFlag.SESSION0;
            case SESSION_1:
                return SessionFlag.SESSION1;
            case SESSION_2:
                return SessionFlag.SESSION2;
            case SESSION_3:
                return SessionFlag.SESSION3;
        }

        return sessionFlag;
    }

    /**
     * フィルターを作成します。
     * @param memBank 対象のメモリ領域
     * @param offset 開始位置(ビット値)　※EPCの先頭を対象にする場合は32を指定する
     * @param size 有効ビット長
     * @param maskData フィルタリング設定値
     * @return フィルター
     */
    public Filter createDataFilter(MemoryBank memBank, long offset, int size, byte[] maskData) {
        try {
            return Filter.createDataFilter(memBank, offset, size, maskData);
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            throw ex;
        }
    }

    /**
     * 特定EPCのみを対象とするフィルターを作成します。
     * @param tagId EPC
     * @return フィルター
     */
    public Filter createTagIdFilter(String tagId) {
        try {
            return Filter.createTagIDFilter(tagId);
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            throw ex;
        }
    }

    /**
     * インベントリ開始(読取)
     * @param filter フィルター
     * @param params インベントリパラメータ
     */
    public void inventoryStart(Filter filter, InventoryParams params)throws Exception {
        synchronized (_readerLock) {
            try {
                if (!isReading()) {
                    //※ openInventory()でエラーが発生する場合があるため、開始前にClose処理実施
                    _RFIDReader.closeInventory();

                    //フィルター設定保持
                    _filter = filter;

                    //引数のパラメータが空の場合は、前回設定を適用する
                    if (params == null) {
                        params = _inventParams;
                    }

                    Status status = _RFIDReader.openInventory(filter, params);
                    if (status != Status.STATUS_OK) {
                        //インベントリ開始失敗
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, "エラーコード：" + status.toString() + " " + status.name());
                        throw new Exception("タグ読取開始エラー" + "\n" + " エラーコード：" + status.toString() + "\n" + status.name());
                    }

                    //読取実行中フラグON
                    _isReading = true;
                    _inventParams = params;
                }
            } catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                throw ex;
            }
        }
    }

    /** インベントリ終了*/
    public void inventoryStop() throws Exception {
        synchronized (_readerLock) {
            try {
                if (isReading()) {
                    Status status = _RFIDReader.closeInventory();
                    if (status != Status.STATUS_OK) {
                        //インベントリ終了失敗
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, "エラーコード：" + status.toString() + " " + status.name());
                        throw new Exception("タグ読取終了エラー" + "\n" + " エラーコード：" + status.toString() + "\n" + status.name());
                    }
                    //読取実行中フラグOFF
                    _isReading = false;
                }
            } catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                throw ex;
            }
        }
    }

    /**
     * タグ書込み
     * @param filter フィルター
     * @param params 書込みパラメータ
     */
    public void writeTag(Filter filter, WriteParams params) throws Exception {
        synchronized (_readerLock) {
            try {
                if (!isReading()) {
                    Status status = _RFIDReader.writeTag(filter, params);
                    if (status != Status.STATUS_OK) {
                        //タグ書込み開始失敗
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, "エラーコード：" + status.toString() + " " + status.name());
                        throw new Exception("タグ書込みエラー" + "\n" + " エラーコード：" + status.toString() + "\n" + status.name());
                    }
                }
            } catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                throw ex;
            }
        }
    }

    /**
     * タグ書込み
     * @param filter フィルター
     * @param params 書込みパラメータ
     */
    public void writeTagID(Filter filter, WriteIDParams params) throws Exception {
        synchronized (_readerLock) {
            try {
                if (!isReading()) {
                    Status status = _RFIDReader.writeID(filter, params);
                    if (status != Status.STATUS_OK) {
                        //タグ書込み開始失敗
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, "エラーコード：" + status.toString() + " " + status.name());
                        throw new Exception("タグ書込みエラー" + "\n" + " エラーコード：" + status.toString() + "\n" + status.name());
                    }

                    //読取実行中フラグON
                    _isReading = true;
                }
            } catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                throw ex;
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
    public void setCallBackListener(DxRFIDInterface listener) {
        synchronized (_readerLock) {
            _dxRfid = listener;
        }
    }

    /** 読取イベントのCallBack先設定を削除 */
    public void releaseCallBackListener() {
        synchronized (_readerLock) {
            _dxRfid = null;
        }
    }

    /**
     * RFID読取イベント処理
     * @param readResult 読取結果パラメータ
     */
    @Override
    public void onReadResultReceived(ReadResult readResult) {
        try {
            if (readResult.getStatus() == Status.STATUS_ERR_RFID_TIMEOUT) {
                //タイムアウトの場合、読取を再開する
                inventoryStop();
                inventoryStart(_filter, _inventParams);
                return;
            }

            if (_dxRfid != null) {
                //読取イベントの発生を通知する
                _dxRfid.onReadResultReceived(readResult.getStatus(),
                        readResult.getTagID(),
                        readResult.getRSSI(),
                        readResult.getMemoryBank(),
                        readResult.getBankData(),
                        readResult.getPC());
            }
        }catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }
}
