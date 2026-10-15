package com.rfid.common.idata;

import android.app.Application;

import com.rfid.common.idata.util.MConstant;
import com.rfid.common.idata.util.MUtil;

import realid.rfidlib.MyLib;

public class IdataApplication extends Application {
    private MyLib idataLib;
    private static IdataApplication idataApp;
    public static String currentDeviceName = "";
    public static boolean poweronStatus; //上电状态
    public static int oldStatue = -1;
    @Override
    public void onCreate() {
        super.onCreate();
        idataApp = this;
        idataLib = new MyLib(this);
        currentDeviceName = (String) MUtil.getInstance().getSystemProp(MConstant.DeviceCode);//获取设备编号
        initOperate();
    }

    public static IdataApplication getIdataApp() {
        return idataApp;
    }

    public MyLib getIdataLib() {
        return idataLib;
    }

    private void initOperate() {
        poweronStatus = idataLib.powerOn();
    }

}


