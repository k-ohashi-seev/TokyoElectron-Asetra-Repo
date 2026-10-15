package com.rfid.common;

public interface DxDeviceInterface {
    /** バッテリー残量変化イベント */
    void onBatteryModify(boolean isCharging, int batteryLevel);
}
