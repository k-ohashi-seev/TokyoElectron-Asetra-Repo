package com.rfid.common.idata.util;

import android.content.Intent;
import androidx.appcompat.app.AlertDialog;
import android.text.TextUtils;
import android.widget.Toast;

import com.rfid.common.idata.IdataApplication;
import com.rfidpk.R;

import java.lang.reflect.Method;

import static com.rfid.common.idata.IdataApplication.currentDeviceName;

/**
 * author CYD
 * date 2018/11/26
 * email chengyd@idatachina.com
 */
public final class MUtil {

    //修改把枪按键的参数
    private static final String ACTION_KEYBD_REMAP = "android.intent.extend.KEYBD_REMAP";
    private static final String INTENT_EXTRA_COMMAND = "cmd";
    private static final String INTENT_EXTRA_PARAM_1 = "arg1";
    private static final String INTENT_EXTRA_PARAM_2 = "arg2";

    public static final String A5P_Device = "50S-V01-R01";//A5P
    public static final String A5P_ComBaseLin_Device = "50V400R001";//A5P共基线版本
    private Object camType = null;
    private boolean isHardwareDecode = false;


    private static class MySingleton {
        final static MUtil singleton = new MUtil();
    }


    public static MUtil getInstance() {
        return MySingleton.singleton;
    }

    public static void show(String text) {
        Toast.makeText(IdataApplication.getIdataApp(), text, Toast.LENGTH_SHORT).show();
    }

    public static void show(int rid) {
        Toast.makeText(IdataApplication.getIdataApp(), rid, Toast.LENGTH_SHORT).show();
    }

    private static AlertDialog atdialog;


    public static void warningDialog() {
        if (atdialog == null) {
            atdialog = new AlertDialog.Builder(IdataApplication.getIdataApp()).create();
            atdialog.setTitle(R.string.poweon_failed);
            atdialog.setMessage(IdataApplication.getIdataApp().getString(R.string.notice_power_failed));
            atdialog.setCancelable(false);
            atdialog.show();
        }
    }

    public static void cancelWaringDialog() {
        if (atdialog != null) {
            atdialog.cancel();
            atdialog = null;
        }
    }

    public void ChangeBarcodeType(boolean IsIntentSend) {
        Intent intent = new Intent("android.intent.action.BARCODEOUTPUT");
        intent.putExtra("android.intent.action.BARCODEOUTPUT", IsIntentSend ? 1 : 0);
        IdataApplication.getIdataApp().sendBroadcast(intent);
    }

    /**
     * 获取系统属性值
     *
     * @param prop 属性的key
     * @return 对应返回的值
     */
    public Object getSystemProp(String prop) {
        Object value = null;
        try {
            Class<?> cl = Class.forName("android.os.SystemProperties");
            Method md = cl.getMethod("get", String.class);
            value = md.invoke(cl, prop);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return value;
    }

    public void changCode(boolean status) {
        if (!currentDeviceName.equals(A5P_Device) && !currentDeviceName.equals(A5P_ComBaseLin_Device)) {
            return;
        }
        switchIScan(status);
        kpd_enableKeybd(status);
        if (status) {
            boolean flag = currentDeviceName.endsWith(A5P_Device);
            if (flag)
                kpd_rebindKeybd(260, 473);
            else
                kpd_rebindKeybd(62, 66);
        }
    }


    /**
     * 开关修改按键值的功能(关闭后先前修改的功能无效)
     *
     * @param bEnable true开启，false关闭
     */
    private void kpd_enableKeybd(boolean bEnable) {
        if (!currentDeviceName.equals(A5P_Device) && !currentDeviceName.equals(A5P_ComBaseLin_Device)) {
            return;
        }
        Intent intent = new Intent(ACTION_KEYBD_REMAP);
        intent.putExtra(INTENT_EXTRA_COMMAND, "enable");
        intent.putExtra(INTENT_EXTRA_PARAM_1, (bEnable ? 1 : 0)); // 0: disable / 1: enable
        IdataApplication.getIdataApp().sendBroadcast(intent);
    }

    /**
     * 修改指定的按键code替换为另外的code
     *
     * @param origKeyCode   当前按键的值
     * @param rebindKeyCode 要替换的值
     */
    private void kpd_rebindKeybd(int origKeyCode, int rebindKeyCode) {
        Intent intent = new Intent();
        intent.setAction(ACTION_KEYBD_REMAP);
        intent.putExtra(INTENT_EXTRA_COMMAND, "remap");
        //**** Reference: /kernel-4.9/include/uapi/linux/input-event-codes.h ****
        intent.putExtra(INTENT_EXTRA_PARAM_1, origKeyCode); // original key scancode
        intent.putExtra(INTENT_EXTRA_PARAM_2, rebindKeyCode); // remap key scancode
        IdataApplication.getIdataApp().sendBroadcast(intent);
    }

    public void rcyleDialog() {
        if (atdialog != null) {
            atdialog.cancel();
            atdialog = null;
        }
    }


    /**
     * 开关iscan，避免与UHF共用串口而冲突
     * (默认关闭iscan，除非获取到为软件扫描头，没有串口冲突)
     *
     * @param flag true开启iScan，false关闭
     */
    private void switchIScan(boolean flag) {
        if (!currentDeviceName.equals(A5P_Device) && !currentDeviceName.equals(A5P_ComBaseLin_Device)) {
            return;
        }
        if (camType == null) {
            camType = getSystemProp(MConstant.CameraType);//硬解扫描头
            //获取不到则默认有硬解扫描头
            isHardwareDecode = TextUtils.isEmpty((String) camType) ||
                    Integer.parseInt((String) camType) < 1 || Integer.parseInt((String) camType) > 9;
        }
        if (isHardwareDecode) {
            String KEY_BARCODE_ENABLESCANNER_ACTION = "android.intent.action.BARCODESCAN";
            Intent it = new Intent(KEY_BARCODE_ENABLESCANNER_ACTION);
            it.setPackage("com.android.auto.iscan");
            it.putExtra(KEY_BARCODE_ENABLESCANNER_ACTION, flag);
            IdataApplication.getIdataApp().sendBroadcast(it);
        }
    }


}
