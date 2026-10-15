package com.rfid.Search;

import android.app.Activity;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.main.LoginMn;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;
import com.rfid.Factory.SearchFactory;
import com.rfid.Lease.LeaseSetting;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.CommonProgres;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;

/**
 * 探索クラス
 */
public class SearchTag extends BaseRFIDActivity implements CommonOKDialog.Callback, CommonConfirmDialog.Callback{

    //戻り値用オブジェクト
    private Intent ResIntent = new Intent();

    //プログレスダイアログ クラス
    private CommonProgres C_Progres = new CommonProgres();

    //通信中フラグ
    private Boolean _TransMesuarFlag = false;

    //完了フラグ
    private boolean _CompleteFlag = false;

    //保持データ
    private SearchFactory _SearchData = null;

    //資産番号
    private String _AssetNo = "";
    private String _SubAssetNo = "";

    CommonConfirmDialog.Builder  _confirmDialog = null;

    //確認画面
    private static final int ACTION_LOGOUT = 1;
    private static final int BUTTON_REGISTER = 2;
    private static final int BUTTON_SETTEI = 9;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        SQLiteDatabase db = null;
        try {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.activity_search_tag_in);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //前画面からデータを取得する
            _SearchData = (SearchFactory)getIntent().getSerializableExtra(CommonConst.VIEW_KEY_SEARCH_DATA);
            _AssetNo = getIntent().getStringExtra(CommonConst.VIEW_KEY_SEARCH_ASSET);
            _SubAssetNo = _SearchData.GetSubAssetNo(_AssetNo);

            //文言表示
            InitializeWord();

            //画面初期化
            InitializeActivity();

            //タイマー停止
            StopTimer();

            //操作時間のタイマーをセット
            StartTimeoutTimer(CommonConst.VIEW_WORD_MENU_SEARCH);

            _confirmDialog = new CommonConfirmDialog.Builder(this);

            //発見ボタン Clickイベント
            final Activity activity = this;
            final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
            findViewById(R.id.btnStop).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    try {

                        if (checkTimeout()) {return;}

                        _SearchData.DeleteAssetNo(_AssetNo);

                        //遷移先にデータを渡す
                        Intent intent = new Intent();
                        intent.putExtra(CommonConst.VIEW_KEY_SEARCH_DATA, _SearchData);
                        setResult(RESULT_OK, intent);

                        finish();

                    } catch (Exception ex) {
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    }
                }
            });

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


    //メニュー選択イベント
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        try {
            switch (item.getItemId()) {
                case R.id.action_setting:
                    //設定 →　システム設定画面開く
                    ReaderOff();
                    Intent intent = new Intent(getApplication(), SystemSett.class);
                    startActivityForResult(intent, BUTTON_SETTEI);
                    return true;
                case R.id.action_logout:  //ログアウトボタン
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
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
            return false;
        }
    }

    // レジュームイベント
    @Override
    public void onResume() {
        super.onResume();

        //探索開始移行処理
        SearchTagStartProc();

    }

    /**
     *  ポーズイベント
     */
    @Override
    public void onPause() {

        super.onPause();
        //探索終了
        ReaderOff();
    }

    //画面初期化
    private void InitializeActivity() {

        //資産番号
        ((TextView)findViewById(R.id.txtAssetNo)).setText(_AssetNo);

        //サブ資産番号
        ((TextView)findViewById(R.id.txtSubAssetNo)).setText(_SubAssetNo);

        //電波強度
        ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_SEARCH));

    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH,"",this));                               //タイトル
        ((TextView)findViewById(R.id.btnStop))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_DISCOVERY_BUTTON, "", this));        //発見

        if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
            //DENSOの場合
            ((TextView) findViewById(R.id.labSchRssi))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_SEARCH_RSSI, "", this));              //RSSI
        } else {
            //ZEBRAの場合
            ((TextView) findViewById(R.id.labSchRssi)).setText("");              //RSSI
        }
        ((TextView)findViewById(R.id.labAssetNo))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NUMBER, "", this));        //資産番号
        ((TextView)findViewById(R.id.labSubAssetNo))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_SUB_ASSETNO, "", this));                //サブ資産番号
        ((TextView)findViewById(R.id.labSchIntencityTtl))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this));                      //電波強度
        Button buttonStop = findViewById(R.id.btnStop);
        if (buttonStop.getText().length() >= 10) {
            buttonStop.setTextSize(18);
        }
    }

    /** ボタン有効/無効設定
     * @param prmEnabled  有効/無効SystemSetting.getString(CommonConst.VIEW_WORD_SEARCH_SEARCH_RSSI, "", this)
     * */
    private void SetButtonEnabled(boolean prmEnabled) {
        Button _ButonConfirm = findViewById(R.id.btnInvCConfirm);
        _ButonConfirm.setEnabled(prmEnabled);
    }


    //探索開始移行処理
    private void SearchTagStartProc() {
        try {

            if (!super.isCommScanner()) {   //切断中除外
                return;
            }

            //DENSOの場合ONにする ZEBRAはOnScannerAppearedでONにするため
            //キーエンス製DXシリーズ,iDATAもONにする　20230221 ADD Sato v2.3
            //RS35もONにする　20230410 Sato v2.3.1
            if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName) ||
                    CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString().equals(Common.ConnectRFIDName) ||
                    CommonConst.CONNECT_RFIDNAME.IDATA.getString().equals(Common.ConnectRFIDName) ||
                    CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString().equals(Common.ConnectRFIDName)) {
                //RFID リーダーＯＮ
                ReaderOn();
            }

        }
        catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    protected void ReaderOn(){
        try {
            //探索ON
            Rfid_Search_Start(_SearchData.GetAssetID(_AssetNo), GetSettingKey());
        } catch(Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(e), this);
        }
    }

    protected void ReaderOff(){
        //探索OFF
        Rfid_Search_Stop();
    }

    /**
     * 接続処理
     */
    @Override
    public void OnScannerAppeared() {
        super.OnScannerAppeared();
        //読取開始する
        ReaderOn();
    }

    /**
     * RFID読取りイベント
     * 親クラスから呼ばれます。
     * @param strData
     * @param tempRSSI
     */
    @Override
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) {
        //該当の資産IDの場合
        if (_SearchData.GetAssetID(_AssetNo).equals(strData)) {
            _ArrayRssi.add(tempRSSI);
        }
    }

    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        if (params == null) {
            return;
        }

        //呼出元で分岐する
        switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
            case BUTTON_REGISTER:      //登録ボタン

                //貸出先入力画面へ
                Intent intent = new Intent(getApplication(), LeaseSetting.class);
                startActivity(intent);

                break;
            case ACTION_LOGOUT:      //ログアウトボタン
                //ログイン画面を開く
                intent = new Intent(getApplication(), LoginMn.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                startActivity(intent);
                break;

        }

    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }

    /**
     * サブ画面から戻るイベント処理をします。
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data){
        try {
            switch(requestCode) {
                case BUTTON_SETTEI:
                    //電波強度再設定
                    ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, GetSettingKey()));
                    ReaderOn();
                    break;
                default:
                    break;
            }


        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * メニューの設定処理
     * @param menu
     * @return
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_noconnect, menu);
        return true;
    }

    /**
     * メニュー変更イベント処理
     * @param menu
     * @return
     */
    @Override
    public boolean onPrepareOptionsMenu(Menu menu) {
        return true;
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return C_Setting.KEY_EACH_SEARCH;
    }
    
    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    @Override
    protected void BarcodeDataReceived(String strData) {}

    @Override
    protected void OpenMainActivity(){ }

}
