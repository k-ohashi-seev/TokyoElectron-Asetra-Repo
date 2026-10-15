package com.rfid.common;

import android.app.Activity;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;
import android.util.Log;

public class SystemSetting {

    /**
     * SharedPreferencesから文字列値を取得する
     * @param key    SharedPreferencesキー
     * @param defVal 未設定時等の初期値
     * @param act    アクティビティ
     * @return 設定値
     */
    public static String getString(String key, String defVal, Activity act) {
        String val = PreferenceManager.getDefaultSharedPreferences(act).getString(key, defVal);
        Log.d("設定値取得", String.format("%s：%s", key, val));
        return val;
    }

    /**
     * SharedPreferencesへ文字列値を保存する
     * @param key SharedPreferencesキー
     * @param val 設定値
     * @param act アクティビティ
     */
    public static void setString(String key, String val, Activity act) {
        SharedPreferences.Editor editor = PreferenceManager.getDefaultSharedPreferences(act).edit();
        editor.putString(key, val);
        editor.apply();
        Log.i("設定値保存", String.format("%s：%s→%s", key, getString(key, val, act), val));
    }
}
