package com.rfid.common;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.BatteryManager;

/**
 * Android端末に関する通知を受信するクラスです。
 * <ui>
 *     <li>バッテリー残量を取得します</li>
 * </ui>
 */
public class DeviceBroadcastReceiver extends BroadcastReceiver {
    /** クラスインスタンス */
    private static DeviceBroadcastReceiver _instance;

    /** デバイス通知コールバック */
    private DxDeviceInterface _dxDevice;

    /** バッテリーフィルタ登録フラグ */
    private boolean _isRegBatteryFilter = false;

    /**
     * コンストラクタ
     * ※直接インスタンス化できないようにPrivateで宣言
     */
    private DeviceBroadcastReceiver() {}

    /**
     * クラスインスタンス返却
     * @return クラスインスタンス
     */
    public static synchronized DeviceBroadcastReceiver getInstance() {
        if (_instance == null) {
            _instance = new DeviceBroadcastReceiver();
        }
        return _instance;
    }

    /**
     * 通知受信イベント
     * @param context コンテキスト
     * @param intent インテント
     */
    @Override
    public void onReceive(Context context, Intent intent) {
        if (intent.getAction().equals(Intent.ACTION_BATTERY_CHANGED)) {
            //電池残量変化時
            if (_dxDevice != null) {

                //充電中かどうかを取得
                int status = intent.getIntExtra(BatteryManager.EXTRA_STATUS, -1);
                boolean isCharging = status == BatteryManager.BATTERY_STATUS_CHARGING ||
                        status == BatteryManager.BATTERY_STATUS_FULL;

                //バッテリー残量を取得
                int batteryLevel = intent.getIntExtra(BatteryManager.EXTRA_LEVEL, -1);

                _dxDevice.onBatteryModify(isCharging, batteryLevel);
            }
        }
    }

    /**
     * 端末の電池残量変化イベントの受信設定
     * @param context コンテキスト
     */
    private void setBatteryInfo(Context context) {
        //未登録時のみ設定
        if (!_isRegBatteryFilter) {
            if (_instance == null) return;

            //バッテリー残量を受信するように設定
            IntentFilter batteryFilter = new IntentFilter(Intent.ACTION_BATTERY_CHANGED);
            Intent intent = context.registerReceiver(_instance, batteryFilter);

            //充電中かどうかを取得
            int status = intent.getIntExtra(BatteryManager.EXTRA_STATUS, -1);
            boolean isCharging = status == BatteryManager.BATTERY_STATUS_CHARGING ||
                    status == BatteryManager.BATTERY_STATUS_FULL;

            //バッテリー残量を取得
            int batteryLevel = intent.getIntExtra(BatteryManager.EXTRA_LEVEL, -1);

            if (_dxDevice != null) {
                _dxDevice.onBatteryModify(isCharging, batteryLevel);
            }
            _isRegBatteryFilter = true;
        }
    }

    /**
     * デバイス通知のCallBack先を設定
     * @param context コンテキスト
     * @param listener 参照
     */
    public void setCallBackDeviceListener(Context context, DxDeviceInterface listener) {
        _dxDevice = listener;
        setBatteryInfo(context);
    }

    /** デバイス通知のCallBack先設定を削除 */
    public void releaseCallBackDeviceListener(Context context) {
        _dxDevice = null;
        context.unregisterReceiver(_instance);
        _isRegBatteryFilter = false;
    }
}
