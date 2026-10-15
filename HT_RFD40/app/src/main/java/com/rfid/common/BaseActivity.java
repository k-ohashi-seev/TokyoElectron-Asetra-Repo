package com.rfid.common;

import android.annotation.SuppressLint;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;

import android.os.Handler;
import android.util.Log;
import android.view.Gravity;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.Toast;

import com.rfid.Dispose.DisposeRead;
import com.rfid.Factory.InventoryFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.ProcessFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Inventory.InventoryLocation;
import com.rfid.Inventory.InventoryLocationSetting;
import com.rfid.Inventory.InventoryTagIn;
import com.rfid.Lease.LeaseRead;
import com.rfid.Lease.LeaseSetting;
import com.rfid.Return.ReturnRead;
import com.rfid.Search.SearchRead;
import com.rfid.Storage.StorageLocationSetting;
import com.rfid.Taglink.TaglinkRead;
import com.rfid.main.CheckEmployee;
import com.rfid.main.MainActivity;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;

import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

/**
 * Common control class
 */
public abstract class BaseActivity extends AppCompatActivity implements CommonOKDialog.Callback, CommonConfirmDialog.Callback {


    private Toast ts = null;

    //システム設定値
    protected SettingValue C_Setting = new SettingValue();

    //RFIDリーダーへの接続状態
    protected static boolean connectReader = false;

    //エラーメッセージ用ダイアログ作成クラス ADD 2019/12/12
    private static CommonDialog _ErrDialog = new CommonDialog();

    // 棚卸し保持データ
    protected static InventoryFactory _InventoryData = new InventoryFactory();

    // 貸出など保持データ
    protected static ProcessFactory _ProcessData = new ProcessFactory();

    // スレッド間処理ロック用オブジェクト　20230705 ADD Sato
    protected final Object _LockObj = new Object();

    //操作放置時間設定
    private long _timeoutSetting = 0;
    //時間切れ画面
    private String _timeoutScreen = "";

    //確認ダイアログ用
    private final static int ACTION_LOGOUT = 1;

    /** バッテリー取得タイマー */
    private static Timer _timeoutTimer;
    private static CountUpTimeoutTimerTaskMain _timeoutTimerTask;
    private static Handler handler_timeout_timer;

    CommonConfirmDialog.Builder  _confirmDialog = null;

    /**
     * Activity stack management
     */
    private static List<BaseActivity> activityStack = new ArrayList<>();

    //戻り値用オブジェクト
    protected Intent ResIntent = new Intent();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        _confirmDialog = new CommonConfirmDialog.Builder(this);

        // Add to Activity stack
        activityStack.add(this);
    }

    @Override
    protected void onDestroy() {
        // Delete from Activity stack
        activityStack.remove(this);
        super.onDestroy();
    }

    /**
     * Display toast
     * @param msg
     */
    public synchronized void showToastMessage(String msg) {
        if (ts != null) {
            ts.cancel();
        }
        ts = Toast.makeText(this, msg, Toast.LENGTH_SHORT);
        ts.setGravity(Gravity.CENTER, 0, 0);
        ts.show();
    }

    /**
     * エラーメッセージを表示する(導入関数)
     * @param msg
     */
    public void showErrorMessage(String msg) {
        if (msg == null) {
            return;
        }
        /*
        CommonDialog dialog = new CommonDialog();
        dialog.Message(msg);
        dialog.show(getSupportFragmentManager(), "ERROR");
        */

        if(_ErrDialog.p_ShowDialog) {    //ダイアログ表示中は表示しない ADD 2019/12/12
            return;
        }

        //排他制御でメッセージを表示する ADD 2019/12/12
        showErrorMessageSync(msg);
    }
    /**
     * エラーメッセージを表示する(排他制御) ADD 2019/12/12
     * @param msg
     * */
    private synchronized void showErrorMessageSync(String msg) {

        //モジュール変数で実行 2019/12/12
        _ErrDialog = new CommonDialog();
        _ErrDialog.Message(msg);
        //20220316 UPD Suzuki START
        //_ErrDialog.show(getSupportFragmentManager(), "ERROR");
        _ErrDialog.show(getSupportFragmentManager(), "");
        //20220316 UPD Suzuki END
    }

    /**
     * エラーメッセージを表示する
     * @param strErrMessage
     */
    public void ShowErrorMessage(String strErrMessage, String strTitle) {

        if(CommonOKDialog.p_ShowDialog) {    //ダイアログ表示中は表示しない ADD 2019/12/12
            return;
        }

        CommonOKDialog.Builder  errdialog = new CommonOKDialog.Builder(this, true);
        errdialog.title(strTitle);
        errdialog.message(strErrMessage);
        errdialog.params(new Bundle());
        errdialog.show();
    }

    /**
     * OKメッセージを表示する
     * @param strErrMessage
     */
    public void ShowOKMessage(String strErrMessage, String strTitle) {

        if(CommonOKDialog.p_ShowDialog) {    //ダイアログ表示中は表示しない ADD 2019/12/20 Sasaki
            return;
        }

        CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
        dialog.title(strTitle);
        dialog.message(strErrMessage);
        dialog.params(new Bundle());
        dialog.show();
    }

    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
    }

    /**
     * エラーメッセージを表示する
     * @param msg
     */
    public synchronized void showMessage(String title, String msg) {
        if (msg == null) {
            return;
        }
        CommonDialog dialog = new CommonDialog();
        dialog.Message(msg);
        dialog.show(getSupportFragmentManager(), title);
    }

    /**
     * メニューの設定処理
     * @param menu
     * @return
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    /**
     * メニュー変更イベント処理
     * @param menu
     * @return
     */
    @Override
    public boolean onPrepareOptionsMenu(Menu menu) {
        super.onPrepareOptionsMenu(menu);

        //MenuInflater inflater = getMenuInflater();
        //inflater.inflate(R.menu.menu_main, menu);

        MenuItem menuConnect = menu.findItem(R.id.action_connect);
        if(connectReader) {
            menuConnect.setIcon(R.drawable.connect_48);
        } else {
            menuConnect.setIcon(R.drawable.connect_disable_48);
        }
        return true;
    }

    /**
     * メニュー選択イベント
     */
    @SuppressLint("NonConstantResourceId")
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        try {
            switch(item.getItemId()) {
                case android.R.id.home:     //戻るボタン
                    if (checkTimeout()) {return true;}
                    //ボタン非活性
                    AllResetButton(false);

                    setResult(RESULT_CANCELED, ResIntent);      //キャンセルを返す
                    finish();

                    return true;
                case R.id.action_menu:
                    //ボタン非活性
                    AllResetButton(false);
                    onMenuReturn();
                    return true;
                case R.id.action_logout:
                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, ACTION_LOGOUT);
                    _confirmDialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGOUT, "", this));  //ログアウト
                    _confirmDialog.message(SystemSetting.getString(
                            CommonConst.VIEW_WORD_LOGOUT, "", this));                //ログアウトします。よろしいですか？
                    _confirmDialog.params(bundle);
                    _confirmDialog.show();
            }
            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * ログイン画面で使用。元の画面へ遷移する
     */
    public void PageTransition(String screenName) {

        Intent intent = null;
        SQLiteDatabase db = null;		//20210918 ADD Takase V2.0

        switch (screenName) {
            case CommonConst.VIEW_WORD_MENU_TITLE:
                intent = new Intent(getApplication(), MainActivity.class);
                startActivity(intent);
                break;
            case CommonConst.VIEW_WORD_MENU_INVENTORY:      //棚卸
                try {
                    //DB接続
                    DatabaseHelper dbHelper = new DatabaseHelper(this);
                    db = dbHelper.getWritableDatabase();

                    //実棚方法を取得
                    final String strInventoryMethod = SettingFactory.GetInventoryMethod(db);

                    //実棚ボタンイベント処理
                    EventButtonInventory(strInventoryMethod);

                } finally {
                    if (db != null) {
                        db.close();
                    }
                }
                break;
            case CommonConst.VIEW_WORD_MENU_SEARCH:        //探索
                intent = new Intent(getApplication(), SearchRead.class);
                startActivity(intent);
                break;
            case CommonConst.VIEW_WORD_MENU_LENDING:        //貸出
                //20210918 CHG Takase V2.0 START
                //intent = new Intent(getApplication(), LeaseSetting.class);
                //startActivity(intent);
                try {
                    //2024/10/03 ADD Shirakawa V.4.0 START
                    //DB接続
                    DatabaseHelper dbHelper = new DatabaseHelper(this);
                    db = dbHelper.getWritableDatabase();
                    //通常貸出か照合貸出チェック
                    if (CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())){
                        //照合貸出
                        intent = new Intent(getApplication(), ConstRuction.class);
                        intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, screenName);
                        startActivity(intent);
                    }else {
                        //通常貸出
                        if (SettingFactory.GetReservationFunction(db)
                                .equals((CommonConst.RESERVATION_FUNCTION.YES.getString()))) {
                            //予約機能あり
                            intent = new Intent(getApplication(), LeaseRead.class);
                        } else {
                            //予約機能なし
                            intent = new Intent(getApplication(), LeaseSetting.class);
                        }
                        startActivity(intent);
                    }
                    //2024/10/03 ADD Shirakawa V.4.0 END

                } finally {
                    if (db != null) {
                        db.close();
                    }
                }
                //20210918 CHG Takase V2.0 END
                break;
            case CommonConst.VIEW_WORD_INV_LOC_TITLE:         //実棚　ロケ指定
                intent = new Intent(getApplication(), InventoryLocationSetting.class);
                startActivity(intent);
                break;
            case CommonConst.VIEW_WORD_LOCATION_CHOICE:      //ロケーション選択
                intent = new Intent(getApplication(), LocationSelect.class);
                startActivity(intent);
                break;
            case CommonConst.KEY_READ_INVENTORY_LOC:         //棚卸読取(ロケ指定あり)
                intent = new Intent(getApplication(), InventoryLocation.class);
                startActivity(intent);
                break;
            case CommonConst.VIEW_WORD_MENU_RETURN:         //返却
                intent = new Intent(getApplication(), ReturnRead.class);
                startActivity(intent);
                break;
            case CommonConst.VIEW_WORD_MENU_STORE:          //保管
                intent = new Intent(getApplication(), StorageLocationSetting.class);
                startActivity(intent);
                break;
            case CommonConst.VIEW_WORD_MENU_DELETE:         //除却
                intent = new Intent(getApplication(), DisposeRead.class);
                startActivity(intent);
                break;
            case CommonConst.VIEW_WORD_MENU_TAG:            //タグ紐付
                intent = new Intent(getApplication(), TaglinkRead.class);
                startActivity(intent);
                break;

            case CommonConst.VIEW_WORD_MENU_MATCHIN:           //持込照合
            case CommonConst.VIEW_WORD_MENU_MATCHOUT:          //持帰照合
            case CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST:   //仮置登録
            case CommonConst.VIEW_WORD_MENU_MATCHRETURN:       //照合返却
                intent = new Intent(getApplication(), ConstRuction.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, screenName);
                startActivity(intent);
                break;
            default:
                break;
        }
    }

    /**
     * 実棚ボタンイベント
     * @param strInventoryMethod
     */
    public void EventButtonInventory(String strInventoryMethod){
        Intent intent = null;

        //設定マスタの実棚方法により画面遷移する
        switch (strInventoryMethod) {
            case CommonConst.SETTING_INVENTORY_METHOD_NONELOC:
                //実棚収集画面(ロケ指定なし)へ
                intent = new Intent(getApplication(), InventoryTagIn.class);
                break;
            case CommonConst.SETTING_INVENTORY_METHOD_LOC:
                //実棚収集画面(ロケ指定あり)へ
                intent = new Intent(getApplication(), InventoryLocationSetting.class);
                break;
            default:
                //実棚収集画面(ロケ指定なし)へ
                intent = new Intent(getApplication(), InventoryTagIn.class);
                break;
        }
        startActivity(intent);
    }

    public String GetWord(String strCode) {
        return SystemSetting.getString(strCode,"", this);
    }

    /**
     * メニュー画面に遷移する
     */
    public void onMenuReturn() {
        //this.moveTaskToBack(true);
        //メニュー画面へ
        Intent intentMenu = new Intent(getApplication(), MainActivity.class);
        startActivity(intentMenu);

        finish();
    }

    /**
     * 全てのアクティビティを終了することでアプリを終了させる
     */
    protected void allActivityFinish() {
        for (int i = 0; i < activityStack.size(); i++) {
            activityStack.get(i). finishAndRemoveTask ();
        }
        //アプリを終了させると同時にRFIDリーダの接続も切断するために必要
        android.os.Process.killProcess(android.os.Process.myPid());
    }
    protected abstract void AllResetButton(boolean blnEnabled);

    /**
     * 操作放置の時間を超えている場合時間切れの画面を指定して、作業者確認画面に遷移,以降の処理を行うかをきめるbooleanを返す
     * @return
     */
    public boolean checkTimeout() {
        if (_timeoutSetting == 0) {return false;}

        long noControlTime = Integer.parseInt(C_Setting.getNoControlTime(BaseActivity.this));
        if (noControlTime >= _timeoutSetting) {
            C_Setting.setNoControlTime(BaseActivity.this, "0");
            //作業者確認画面を開く
            Intent intent = new Intent(getApplication(), CheckEmployee.class);
            intent.putExtra(CommonConst.VIEW_KEY_TIMEOUT, _timeoutScreen);
            startActivity(intent);
            return true;
        } else {
            C_Setting.setNoControlTime(BaseActivity.this, "0");
            return false;
        }
    }

    /***
     * 時間切れの確認のための時間を足しこんでいく
     */
    protected class CountUpTimeoutTimerTaskMain extends TimerTask {
        @Override
        public void run() {
            long noControlTime = Integer.parseInt(C_Setting.getNoControlTime(BaseActivity.this));
            if (_timeoutSetting <= noControlTime) {
                // 時間切れ
                return;
            }
            String time = String.valueOf(noControlTime + 500);
            C_Setting.setNoControlTime(BaseActivity.this, time);
        }
    }

    /**
     *  タイマー開始
     */
    protected void StartTimeoutTimer(String screen) {
        _timeoutSetting = Integer.parseInt(C_Setting.getLoginTimeoutSetting(this))  * 60L * 1000L;
        _timeoutScreen = screen;

        if (_timeoutTimer == null) {
            // Timer インスタンスを生成
            _timeoutTimer = new Timer();
            // TimerTask インスタンスを生成
            _timeoutTimerTask = new CountUpTimeoutTimerTaskMain();

            // スケジュールを設定 500msec
            _timeoutTimer.schedule(_timeoutTimerTask, 0, 500);
        }

    }

    /**
     * タイマー停止
     */
    protected void StopTimeoutTimer() {
        //タイマー停止
        if (_timeoutTimer != null) {
            _timeoutTimer.cancel();
        }
        C_Setting.setNoControlTime(BaseActivity.this, "0");
        _timeoutTimerTask = null;
        _timeoutTimer = null;

    }
}
