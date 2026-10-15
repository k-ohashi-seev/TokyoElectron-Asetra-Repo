package com.rfid.common;

public interface BarcodeInterface {
    /** 読取結果通知イベント */
    void onBcdDataReceived(String readData);
}
