package com.rfid.Taglink;

import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.ProcessFactory;
import com.rfid.common.AssetModel;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.main.LoginMn;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;

import java.nio.charset.Charset;

/**
 * タグ紐付登録画面
 *
 */
public class TaglinkConfirm extends BaseRFIDActivity implements CommonConfirmDialog.Callback, CommonOKDialog.Callback{

    //システム設定値
    private SettingValue C_Setting = new SettingValue();

    //タグ紐付用データ
    private AssetModel _TagData = null;

    private String ReadTagID = "";
    CommonConfirmDialog.Builder  _confirmDialog = null;
    private final static int ACTION_LOGOUT = 1;
    protected static final int BUTTON_COMPLETE = 2;
    private final static int BUTTON_REGIST = 3;

    // 読取タグのPC + EPCを保持
    private String _pcEpcHex = "";

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        //DB接続
        SQLiteDatabase db = null;

        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_taglink_confirm);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //前画面からデータを取得する
            _TagData = (AssetModel)getIntent().getSerializableExtra(CommonConst.VIEW_KEY_TAGLINK_DATA);

            //文言表示
            InitializeWord();

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //資産マスタから情報を設定する
            ViewAssetInfo();

            //ボタン非活性
            SetButtonEnabled(false);

            //操作時間のタイマーをセット
            StartTimeoutTimer(CommonConst.VIEW_WORD_MENU_SEARCH);

            _confirmDialog = new CommonConfirmDialog.Builder(this);

            //クリアボタン Clickイベント
            findViewById(R.id.btnClear).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    if (checkTimeout()) {return;}
                    onClearButton();
                }
            });

            //紐付登録ボタン Clickイベント
            findViewById(R.id.btnRegist).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    if (checkTimeout()) {return;}
                    onRegistButton();
                }
            });

            //サービス開始
            startService();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        } finally {
            if (db != null ){
                db.close();
            }
        }
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG,"",this));             //タグ紐付　タイトル
        ((TextView)findViewById(R.id.lblAssetNo))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NUMBER, "", this));  //資産番号
        ((TextView)findViewById(R.id.lblAssetName))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NAME, "", this));    //資産名称
        ((TextView)findViewById(R.id.lblLocTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_LOC_INFO, "", this));              //ロケ情報
        ((TextView)findViewById(R.id.lblLocLKbn))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_LOCL_TITLE, "", this));           //大区分
        ((TextView)findViewById(R.id.lblLocMKbn))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_LOCM_TITLE, "", this));           //中区分
        ((TextView)findViewById(R.id.lblLocSKbn))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_LOCS_TITLE, "", this));           //小区分
        ((TextView)findViewById(R.id.lblAssetId))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ASSET_ID, "", this));              //資産ID
        ((TextView)findViewById(R.id.lblTagLinkRead))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_EPC_ENCODE, "", this));           //読取EPCエンコード値
        ((TextView)findViewById(R.id.btnClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));                   //クリア
        ((TextView)findViewById(R.id.btnRegist))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_REGISTER_BUTTON, "", this));      //紐付登録
        ((TextView)findViewById(R.id.labReadTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_MSG_ITEM, "", this));  //タグ読取　書込と紐付
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

            //画面初期化
            InitializeActivity();

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

            //ReaderOff
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
     * 資産情報を表示します
     */
    private void ViewAssetInfo() {
        ((TextView)findViewById(R.id.txtAssetNo)).setText(_TagData.getAssetNo());       //資産番号
        ((TextView)findViewById(R.id.txtSubAssetNo)).setText(_TagData.getSubAssetNo());    //資産番号
        ((TextView)findViewById(R.id.txtAssetName)).setText(Common.substring(_TagData.getAssetName(), CommonConst.VIEW_TAGLINK_CONFIRM_SIZE));   //資産名称
        ((TextView)findViewById(R.id.txtAssetId)).setText(_TagData.getAssetID());                                                                    //資産ID
        ((TextView)findViewById(R.id.txtLocLKbn)).setText(Common.substring(_TagData.gettLocLCd(), CommonConst.VIEW_TAGLINK_CONFIRM_SIZE));       //ロケ大区分
        ((TextView)findViewById(R.id.txtLocMKbn)).setText(Common.substring(_TagData.gettLocMCd(), CommonConst.VIEW_TAGLINK_CONFIRM_SIZE));       //ロケ中区分
        ((TextView)findViewById(R.id.txtLocSKbn)).setText(Common.substring(_TagData.gettLocSCd(), CommonConst.VIEW_TAGLINK_CONFIRM_SIZE));       //ロケ小区分
    }

    /**
     * RFID読取りイベント
     * 親クラスから呼ばれます。
     * @param strData
     * @param tempRSSI
     */
    @Override
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        SQLiteDatabase db = null;
        try {
            Log.d("資産ID", strData);

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //既に読み込まれていた場合、終了
            if (!"".equals(ReadTagID)) {
                return;
            } else {
                ReadTagID = strData;
            }

            //20210918 ADD Takase V2.0 START
            //更新権限がない資産の場合
            if (!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, _TagData.getAssetNo()), CommonAuthority.Auth.updHubList)) {
                Button button = findViewById(R.id.btnClear);
                button.setEnabled(true);
                button.setTextColor(Color.parseColor("#474747"));
            //20210918 ADD Takase V2.0 START

            //資産IDが既に存在する場合、エラーとする
            } else if (AssetFactory.existAssetID(db, strData)) {
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_MSG_SET_OTHER_NUMBER, "", this)
                        , SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG, "", this));
                Button button = findViewById(R.id.btnClear);
                button.setEnabled(true);
                button.setTextColor(Color.parseColor("#474747"));
            //EPC_SIZEが異なる場合エラーとする　
            } else if (CommonConst.ASSET_GENERATION.NoWrite.getString().equals(_TagData.gettAssetIdRegDiv()) &&
                       !checkEpcSize(strData, _TagData.getEpcSize())) {
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_ERR_EPCSIZE, "", this)
                        , SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG, "", this));
                Button button = findViewById(R.id.btnClear);
                button.setEnabled(true);
                button.setTextColor(Color.parseColor("#474747"));
            } else {
                //ボタン活性
                SetButtonEnabled(true);
            }

            //表示する
            ((TextView) findViewById(R.id.txtTagLinkRead)).setText(strData);

            //PC + EPCを保持
            _pcEpcHex = strPcHex + strData;

            //二度読み防止バッファクリア
            //subClearDuplexBuffer();

            //読取停止
            RFID_Read_Stop();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * クリア処理
     */
    private void onClearButton(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        ReadTagID = "";
        _pcEpcHex = "";

        //読取を消去
        ((TextView)findViewById(R.id.txtTagLinkRead)).setText("");

        //ボタン非活性
        SetButtonEnabled(false);

        //読取開始
        ReaderOn();
    }

    /**
     * 紐付登録処理
     */
    private void onRegistButton(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //確認画面を表示し、OKの場合true、キャンセルの場合false
        final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_REGIST);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //クリア
        dialog.message(SystemSetting.getString(
                CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_REGISTER, "", this));                //読取データを登録します。よろしいですか？
        dialog.params(bundle);
        dialog.show();
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
     * 確認ダイアログでOKがクリックされたイベント処理
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //呼出元で分岐する
        switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
            case BUTTON_REGIST:      //登録ボタン
                //ボタン非活性
                AllResetButton(false);
                SetButtonEnabled(false);
                new Thread() {
                    @Override
                    public void run() {
                        TagLinkRegist();
                    }
                }.start();

                break;
            case BUTTON_COMPLETE:      //登録完了ボタン
                //タグ読取画面へ
                finish();
                break;

            case ACTION_LOGOUT:      //ログアウトボタン
                //ログイン画面を開く
                Intent intent = new Intent(getApplication(), LoginMn.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                startActivity(intent);
                break;
        }

    }

    /**
     * 指定サイズより大きい場合エラー
     * @param strAssetID
     * @param strEpcSize
     * @return
     */
    private boolean checkEpcSize(String strAssetID, String strEpcSize) {
        boolean blnReturn = false;

        //指定がない場合正常終了
        if (strEpcSize == null || "".equals(strEpcSize)) {
            return true;
        }

        //ビット変換し、指定サイズと異なる場合エラーとする
        int intByte = strAssetID.getBytes(Charset.forName("MS932")).length;
        if (intByte * 4 == Integer.valueOf(strEpcSize)) {
            blnReturn = true;
        }

        return blnReturn;
    }

    /**
     * タグ書込み処理
     */
    private void TagLinkRegist() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        SQLiteDatabase db = null;
        try {

            String strMessage = "";
            String strProcessDiv = "";

            //読取資産ID
            String strAssetID = ((TextView) findViewById(R.id.txtTagLinkRead)).getText().toString();

            if (CommonConst.ASSET_GENERATION.NoWrite.getString().equals(_TagData.gettAssetIdRegDiv())) {
                //タグ書込みを行わない場合、読込した資産IDを登録する
                _TagData.setAssetID(strAssetID);
                strMessage = CommonConst.VIEW_WORD_LINK_MSG_SET_TAG_REGISTER;
                strProcessDiv = CommonConst.PROCESS_DIV.TagLink.getString();
            } else {
                //タグ書込み実施に必要なパラメータを準備
                String oldPc = "";
                if (_pcEpcHex.length() >= CommonConst.PC_HEX_SIZE) {
                    oldPc = _pcEpcHex.substring(0, CommonConst.PC_HEX_SIZE);
                }

                int newEpcSize = Common.toInt(_TagData.getEpcSize());

                //資産Mの資産IDを書込みする
                WriteTag(strAssetID, _TagData.getAssetID(), oldPc, newEpcSize);
                strMessage = CommonConst.VIEW_WORD_LINK_MSG_SET_REGISTER;
                strProcessDiv = CommonConst.PROCESS_DIV.TagLinkWrite.getString();
            }

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            db.beginTransaction();

            //登録処理
            ProcessFactory.InsertProcesTaglink(db, _TagData, strProcessDiv, this);

            //資産状態を更新
            AssetFactory.Update(db, _TagData, CommonConst.ASSET_STATUS.Storage);

            //イベントログ
            LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                    GetWord(strMessage)
                            + " 【" + GetWord(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NUMBER) + "】" + _TagData.getAssetNo()
                            + " 【" + GetWord(CommonConst.VIEW_WORD_LINK_ASSET_ID) + "】" + _TagData.getAssetID()
                    , this);

            //コミット
            db.setTransactionSuccessful();

            //メッセージBOX
            Bundle bundle = new Bundle();
            bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_COMPLETE);
            final CommonOKDialog.Builder dialog = new CommonOKDialog.Builder(this, false);
            dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
            dialog.message(SystemSetting.getString(strMessage, "", this));                //タグ書込み＆紐付は完了しました
            dialog.params(bundle);
            dialog.show();
        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            //ボタン活性
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    SetButtonEnabled(true);
                    AllResetButton(true);
                }
            });
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (db != null) {
                db.endTransaction();
                db.close();
                db = null;
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        } finally {
            ReaderOff();
            if (db != null) {
                db.endTransaction();
            }
        }

    }

    //画面初期化
    private void InitializeActivity() {
    }

    /**
     * リーダーをONにします。
     * 親クラスから呼ばれます。
     */
    protected void ReaderOn(){
        //RFID Write
        RFID_Write_Start(true, C_Setting.KEY_EACH_TAGLINK);
    }

    /**
     * リーダーをOFFにします。
     * 親クラスから呼ばれます。
     */
    protected void ReaderOff(){
        // RFID OFF
        RFID_Read_Stop();
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
                    //タグ読取画面へ
                    finish();
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

    /** ボタン有効/無効設定
     * */
    protected void SetButtonEnabled(boolean prmEnabled) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //紐付登録ボタン
        ((Button)findViewById(R.id.btnRegist)).setEnabled(prmEnabled);

        //クリアボタン
        ((Button)findViewById(R.id.btnClear)).setEnabled(prmEnabled);

        if (prmEnabled == false) {
            ((Button)findViewById(R.id.btnRegist)).setBackground(getDrawable(R.drawable.execute_button_diable));
            ((Button)findViewById(R.id.btnRegist)).setTextColor(Color.parseColor("#a9a9a9"));
            ((Button)findViewById(R.id.btnClear)).setTextColor(Color.parseColor("#a9a9a9"));
        } else {
            ((Button)findViewById(R.id.btnRegist)).setBackground(getDrawable(R.drawable.execute_button));
            ((Button)findViewById(R.id.btnRegist)).setTextColor(Color.parseColor("#ffffff"));
            ((Button)findViewById(R.id.btnClear)).setTextColor(Color.parseColor("#474747"));
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

//20191022 Add Start Sasaki
    /**
     * RFIDリーダー切断イベント
     */
    @Override
    public void onDisconnectEvent() {
        //親クラスの切断処理
        super.onDisconnectEvent();

        //読取データをクリアする
        onClearButton();

    }
//20191022 Add End Sasaki

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {
//20191022 Add Start Sasaki
        //未入力の場合、非活性
        if ("".equals(ReadTagID)) {
            blnEnabled = false;
        }
//20191022 Add End Sasaki

        //紐付登録ボタン
        ((Button)findViewById(R.id.btnRegist)).setEnabled(blnEnabled);

        if (blnEnabled == false) {
            ((Button)findViewById(R.id.btnRegist)).setBackground(getDrawable(R.drawable.execute_button_diable));
            ((Button)findViewById(R.id.btnRegist)).setTextColor(Color.parseColor("#a9a9a9"));
        } else {
            ((Button)findViewById(R.id.btnRegist)).setBackground(getDrawable(R.drawable.execute_button));
            ((Button)findViewById(R.id.btnRegist)).setTextColor(Color.parseColor("#ffffff"));
        }
    }

    protected void ReadTimeStart() {}
    protected void OpenMainActivity(){ }
    protected void BarcodeDataReceived(String strData) {}
    public void onMyDialogCancelled(int requestCode, Bundle params) {}
}
