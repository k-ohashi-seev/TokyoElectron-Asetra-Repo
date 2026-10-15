package com.rfid.common;

public interface RFIDInterface {
    /** 読取結果通知イベント */
    void onReadResultReceived(String readEpc, double rssi, String readPc);
}
