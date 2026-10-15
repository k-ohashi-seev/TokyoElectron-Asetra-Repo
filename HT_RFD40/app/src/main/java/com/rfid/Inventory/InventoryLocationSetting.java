package com.rfid.Inventory;

import android.annotation.SuppressLint;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.view.MenuItem;

import com.rfid.Factory.InventoryStartFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseLocationSetting;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.SystemSetting;
import com.rfid.main.LoginMn;
import com.rfid.main.MainActivity;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;

/**
 * 棚卸ロケーション選択画面のクラス
 * ロケーションを選択します。
 * 設定マスタの値で、どの区分まで表示するか制御します。
 *
 */
public class InventoryLocationSetting extends BaseLocationSetting implements CommonConfirmDialog.Callback {

    private final static int ACTION_LOGOUT = 1;
    private static final int BUTTON_SETTEI = 9;         //設定画面
    CommonConfirmDialog.Builder  _confirmDialog = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        _confirmDialog = new CommonConfirmDialog.Builder(this);

    }

    //ポーズイベント
    @Override
    public void onPause() {
        super.onPause();

        //タイマー停止
        StopTimer();

    }

    /**
     * 次ページ遷移の指定
     * @return
     */
    @Override
    protected Intent onNextPage(){
        return new Intent(getApplication(), InventoryLocation.class);
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
        return C_Setting.KEY_EACH_INVENTORY;
    }

    /**
     * 設定マスタよりロケ区分値を取得する
     * @param db
     * @return
     */
    @Override
    protected String[] GetLocationUsage(SQLiteDatabase db) {
        return SettingFactory.GetLocationUsageInv(db);
    }

    /**
     * 文言を表示します。
     */
    @Override
    protected void InitializeWord() {
        super.InitializeWord();
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_TITLE, "", this));                      //実棚ロケ指定　タイトル
    }

    //20231011 Add Start Shibata
    /**
     * 拠点設定が定義されていないときに棚卸開始情報に画面のロケ大区分コードが存在するか判定する
     * @param locLCode
     * @param db
     */
    @Override
    protected boolean InventoryStartCheck (String locLCode, SQLiteDatabase db) {

        Cursor cursor;
        String strSql = InventoryStartFactory.Select();
        cursor = db.rawQuery(strSql, new String[]{locLCode});

        while (cursor.moveToNext()) {
            if (cursor.getString(0).equals(locLCode)) {
                //棚卸開始情報テーブルに画面の大ロケコードがある場合trueを返す
                return true;
            }
        }
        //拠点設定が定義されていない場合かつ、棚卸開始情報テーブルに画面の大ロケコードがない場合falseを返す
        return false;
    }

    /**
     * メニュー選択イベント
     * @param item
     * @return
     */
    @SuppressLint("NonConstantResourceId")
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        try {
            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            int id = item.getItemId();
            switch(id){
                case android.R.id.home:     //戻るボタン
                    //ボタン非活性
                    AllResetButton(false);

                    //Back ボタン 実棚サブメニュー画面を表示
                    Intent intentreturn = new Intent(getApplication(), MainActivity.class);
                    startActivity(intentreturn);

                    return true;

                case R.id.action_setting:
                    //ボタン非活性
                    AllResetButton(false);

                    //設定 →　システム設定画面開く
                    Intent intent = new Intent(getApplication(), SystemSett.class);
                    startActivityForResult(intent, BUTTON_SETTEI);

                    return true;

                case R.id.action_logout:
                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, ACTION_LOGOUT);
                    _confirmDialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGOUT, "", this));  //ログアウト
                    _confirmDialog.message(SystemSetting.getString(
                            CommonConst.VIEW_WORD_LOGOUT, "", this));                //ログアウトします。よろしいですか？
                    _confirmDialog.params(bundle);
                    _confirmDialog.show();
                    return true;

            }

            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            return false;
        }
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }

    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        if (params == null) {
            return;
        }

        //呼出元で分岐する
        switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
            case ACTION_LOGOUT:      //終了ボタン
                //ログイン画面を開く
                Intent intent = new Intent(getApplication(), LoginMn.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                startActivity(intent);
        }
    }

    //20231011 Add End Shibata
}
