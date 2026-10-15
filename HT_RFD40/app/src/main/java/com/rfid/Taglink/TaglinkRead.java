package com.rfid.Taglink;

import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MenuItem;
import android.widget.TextView;

import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.InventoryFactory;
import com.rfid.Factory.LocationFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Inventory.InventoryLocation;
import com.rfid.common.AssetModel;
import com.rfid.common.Common;
import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.main.LoginMn;
import com.rfid.main.MainActivity;
import com.rfidpk.R;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;

/**
 * 貸出資産読取画面
 *
 */
public class TaglinkRead extends BaseRFIDActivity  implements CommonConfirmDialog.Callback {

    //システム設定値
    private SettingValue C_Setting = new SettingValue();
    CommonConfirmDialog.Builder  _confirmDialog = null;
    private final static int ACTION_LOGOUT = 1;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_taglink_read);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //文言表示
            InitializeWord();

            //操作時間のタイマーをセット
            StartTimeoutTimer(CommonConst.VIEW_WORD_MENU_TITLE);

            _confirmDialog = new CommonConfirmDialog.Builder(this);

            //サービス開始
            startService();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG,"",this));             //タグ紐付　タイトル
        ((TextView)findViewById(R.id.labTagLinkRead))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NUMBER, "", this));  //資産番号
        ((TextView)findViewById(R.id.labReadTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_ASSET_NUMBER_QR, "", this));  //資産番号QR読取
    }

    // レジュームイベント
    @Override
    public void onResume() {
        try {
            super.onResume();

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //読取設定
            ReaderOn();

            //タイマー開始
            StartTimer();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    //ポーズイベント
    @Override
    public void onPause() {
        try {
            super.onPause();

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //読取OFF
            ReaderOff();

            //タイマー停止
            StopTimer();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * Barcode/QR読取りイベント
     * 親クラスから呼ばれます。
     * @param strData
     */
    protected void BarcodeDataReceived(String strData) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        if (checkTimeout()) {return;}

        SQLiteDatabase db = null;
        try {

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(TaglinkRead.this);
            db = dbHelper.getWritableDatabase();
            String assetNo = _InventoryData.GetAssetNoByProcessedData(db, strData);
            if (assetNo.equals("")) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, " AssetNo is not found");
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_REGISTER_NONE, "", this)
                        ,SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG, "", this));
                return;
            }

            Log.d("資産番号", assetNo);

            //資産マスタに登録があるかチェックする
            AssetModel asset = AssetFactory.Select(db, assetNo);
            if ("".equals(asset.getAssetNo())) {
                //資産マスタに存在しない場合
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_REGISTER_NONE, "", this)
                        ,SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG, "", this));
                return ;
            }

            //20210918 ADD Takase V2.0 START
            //参照権限がない資産の場合
            if(!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, assetNo), CommonAuthority.Auth.readHubList)) {
                //所属拠点外エラー(エラーメッセージ)
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_NO_AUTHORITY, "", this), "");
                return;
            }
            //更新権限がない資産の場合
            if (!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, assetNo), CommonAuthority.Auth.updHubList)) {
                //更新できない旨を伝えるメッセージ(画面遷移可能)
                SettingValue settingValue = new SettingValue();
                if (settingValue.getTargetHub(this).equals("")) {
                    showToastMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_NOT_HUB, "", this));
                } else {
                    showToastMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_NOT_HT_HUB, "", this));
                }
            }

            if ("".equals(asset.getAssetID())) {
                //資産IDが未設定の場合
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_MSG_NO_ASSET_ID, "", this)
                        ,SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", this));
                return ;
            }

            //20210918 ADD Takase V2.0 START

            //資産マスタにデータがある場合、資産確認ページに渡す
            Intent intent = new Intent(getApplication(), TaglinkConfirm.class);
            intent.putExtra(CommonConst.VIEW_KEY_TAGLINK_DATA, asset);
            startActivity(intent);

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * リーダーをONにします。
     * 親クラスから呼ばれます。
     */
    protected void ReaderOn(){
        //バーコード ON
        Barcode_Read_Start();
    }

    /**
     * リーダーをOFFにします。
     * 親クラスから呼ばれます。
     */
    protected void ReaderOff(){
        // バーコードOFF
        Barcode_Read_Stop();
    }

    //クリア処理
    private void ClearRfidTagList() {

        //二度読みバッファクリア
        subClearDuplexBuffer();

    }

    /**
     * メニュー選択イベント
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            switch (item.getItemId()) {
                case android.R.id.home:     //戻るボタン
                    //メニュー画面へ
                    Intent intentMenu = new Intent(getApplication(), MainActivity.class);
                    startActivity(intentMenu);
                    return true;

                case R.id.action_setting:
                    //設定 →　システム設定画面開く
                    Intent intent = new Intent(getApplication(), SystemSett.class);
                    startActivity(intent);
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
        } catch (Exception ex) {
            return false;
        }
    }

    /**
     * キー入力イベント 20200316 Add Sasaki
     * @param keyCode
     * @param event
     * @return
     */
    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            switch (keyCode)  {
                case KeyEvent.KEYCODE_BACK:
                    //Back ボタン
                    //メニュー画面へ
                    Intent intentMenu = new Intent(getApplication(), MainActivity.class);
                    startActivity(intentMenu);
            }

            return super.onKeyDown(keyCode, event);
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return C_Setting.KEY_EACH_TAGLINK;
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    protected void OpenMainActivity(){ }
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) {}

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
}
