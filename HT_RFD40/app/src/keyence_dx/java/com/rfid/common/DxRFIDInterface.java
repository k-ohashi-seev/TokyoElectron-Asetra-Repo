package com.rfid.common;

import com.keyence.autoid.sdk.rfid.Status;
import com.keyence.autoid.sdk.rfid.params.MemoryBank;

public interface DxRFIDInterface {
    /** 読取結果通知イベント */
    void onReadResultReceived(Status status, String readEpc, float rssi, MemoryBank memoryBank, byte[] bankData, String readPc);
}
