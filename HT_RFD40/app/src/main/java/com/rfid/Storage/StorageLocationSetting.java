package com.rfid.Storage;

import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;

import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseLocationSetting;
import com.rfid.main.MainActivity;

/**
 * 保管ロケーション選択画面のクラス
 * ロケーションを選択します。
 * 設定マスタの値で、どの区分まで表示するか制御します。
 *
 */
public class StorageLocationSetting extends BaseLocationSetting {

    /**
     * 次ページ遷移の指定
     * @return
     */
    @Override
    protected Intent onNextPage(){
        return new Intent(getApplication(), StorageRead.class);
    }

    /**
     * 戻るページの遷移
     * @return
     */
    @Override
    protected Intent onReturnPage(){
        return new Intent(getApplication(), MainActivity.class);
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return C_Setting.KEY_EACH_STORAGE;
    }

    /**
     * 設定マスタよりロケ区分値を取得する
     * @param db
     * @return
     */
    @Override
    protected String[] GetLocationUsage(SQLiteDatabase db) {
        return SettingFactory.GetLocationUsage(db);
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }
}
