package com.rfid.common;

import com.keyence.autoid.sdk.scan.DecodeResult;

public interface DxBarcodeInterface {
    /** 読取結果通知イベント */
    void onBcdDataReceived(DecodeResult.Result result, String codeType, String readData, byte[] bytReadData);
}
