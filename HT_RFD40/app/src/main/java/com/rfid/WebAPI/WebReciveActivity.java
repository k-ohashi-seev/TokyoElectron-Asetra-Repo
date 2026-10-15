package com.rfid.WebAPI;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.TextView;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.AuthFactory;
import com.rfid.Factory.ConstAssetFactory;
import com.rfid.Factory.EmployeeFactory;
import com.rfid.Factory.EmployeeHubFactory;
import com.rfid.Factory.InventoryStartFactory;
import com.rfid.Factory.LeaseFactory;
import com.rfid.Factory.LentAssetListFactory;
import com.rfid.Factory.LocationFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.MatchAseetListFactory;
import com.rfid.Factory.ProcessFactory;
import com.rfid.Factory.ReserveFactory;
import com.rfid.Factory.RetAseetListFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Factory.TempAssetFactory;
import com.rfid.Factory.TempAssetListFactory;
import com.rfid.Factory.TempListFactory;
import com.rfid.Factory.TemporaryFactory;
import com.rfid.Factory.WordFactory;
import com.rfid.Factory.InventoryStartFactory;
import com.rfid.Factory.ConstRuctionFactory;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonInputPasswordDialog;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.GetMasterData;
import com.rfid.common.bean.GetMasterDataDetail;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfid.setting.SystemSettHub;
import com.rfid.setting.SystemSettSub;
import com.rfidpk.R;

import org.json.JSONArray;
import org.json.JSONObject;
import org.xmlpull.v1.XmlPullParser;

/**
 * マスタ受信を送信するクラス
 * 非同期で送信します。
 */
public class WebReciveActivity extends BaseRFIDActivity implements CommonOKDialog.Callback, CommonInputPasswordDialog.Callback {

    //DB接続
    private SQLiteDatabase _db = null;

    private final Context context = this;
    private final WebReciveActivity activity = this;

    //Declare a variable to store ProfileManager object
    //private ProfileManager profileManager = null;
    private String profileName = "ClockProfile-1";

    private String parmName = "";
    private String errorType = "";

    // Provides error description
    private String errorDescription = "";

    // Provides error string with type/name + description
    private String errorString = "";

    private String _Server = "";
    private int _timeout = 30000;

    private HttpAsyncReciveLoader ReciveAsset = null;

    private String _MasterList = "";

    //20230206 ADD Kawamoto V2.3 START
    public enum RETUEN_CODE {
        OK(0),
        Error(1),
        Worning(2),
        Confirm(3),
        WargningTag(4),
        AssetNoSend(5);

        private final int text;
        private RETUEN_CODE(final int text) {
            this.text = text;
        }
        public int getInt() {
            return this.text;
        }
    }
    private boolean flgPw = false;
    private final static int FORM_REQUEST_PASSWORD = 1;
    //20230206 ADD Kawamoto V2.3 END

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {

        SQLiteDatabase db = null;
        try {

            super.onCreate(savedInstanceState);
            setContentView(R.layout.webapi_main);

            //progressを表示
            ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.VISIBLE);

            //文言表示
            InitializeWord();

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(getApplicationContext());
            db = dbHelper.getWritableDatabase();

            //設定値を取得する
            SettingValue setting = new SettingValue();
            _Server = setting.getConnectServer(this);
            final CommonOKDialog.Builder  errdialog = new CommonOKDialog.Builder(this, true);
            if (_Server == null || _Server == "") {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_SETTING, "", this));
                return;
            }

            //受信するマスタ名称を設定する
            _MasterList =  GetReceiveMasterList(this);

            //タイムアウト
            _timeout = Integer.parseInt(setting.getConnectTimeOut(this)) * 1000;

            new Thread() {
                @Override
                public void run() {
                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {

                            //時刻合わせ受信
                            HttpAsyncReciveLoader ReciveAsset = new HttpAsyncReciveLoader(activity, null, _timeout);
                            ReciveAsset.setListener(timeListener());
                            ReciveAsset.execute("http://" + _Server + "/" + CommonConst.FIXED_VALUE_WEBAPI_DIRECTORY + "/api/HtGetDateTimeCmd");
                            //20230221 DEL Sato v2.3 START
//                            runOnUiThread(new Runnable() {
//                                @Override
//                                public void run() {
//
//                                    //マスタを受信
//                                    HttpAsyncReciveLoader ReciveAsset = new HttpAsyncReciveLoader(activity, _MasterList, _timeout);
//                                    ReciveAsset.setListener(createListener());
//                                    ReciveAsset.execute("http://" + _Server + "/" + CommonConst.FIXED_VALUE_WEBAPI_DIRECTORY + "/api/HtGetMasterDataCmd");
//
//                                }
//                            });
                            //20230221 DEL Sato v2.3 END
                        }
                    });
                }
            }.start();
        } catch (Exception ex) {
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", this) + ":" + Common.toString(ex), this);
            ShowErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * マスタのデータを取得する
     * @return
     */
    private HttpAsyncReciveLoader.Listener createListener() {

        return new HttpAsyncReciveLoader.Listener() {

            /**
             * 正常処理
             * @param json
             */
            @Override
            public void onSuccess(final JSONObject json) {
                SQLiteDatabase db = null;

                try {

                    int result = dbInsert(context, activity, json);

                    //progressを非表示
                    ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);

                    //20230206 UPD Kawamoto V2.3 START
                    ////完了メッセージ
                    //ShowOKMessage(SystemSetting.getString(
                    //CommonConst.VIEW_WORD_ALL_CONFIRM_RECIVE_DATA, "", activity));   //マスタ取込受信が完了しました。
                    //処理結果が5(資産情報未送信)の場合
                    if (result == RETUEN_CODE.AssetNoSend.getInt()) {
                        ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_ASSET_NO_SEND, "", activity));
                        flgPw = true;

                    } else {
                        //完了メッセージ
                        ShowOKMessage(SystemSetting.getString(
                                CommonConst.VIEW_WORD_ALL_CONFIRM_RECIVE_DATA, "", activity));   //マスタ取込受信が完了しました。
                    }
                    setResult(RESULT_OK, new Intent());
                    //20230206 UPD Kawamoto V2.3 END

                } catch (Exception ex){
                    //エラーログ
                    if (db != null) {
                        db.endTransaction();
                        db.close();
                        db = null;
                    }
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", activity) + ":" + Common.toString(ex), activity);
                    //progressを非表示
                    ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                    //エラーメッセージを表示
                    ShowErrorMessage(ex.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                } finally {
                    if (db != null) {
                        db.endTransaction();
                    }
                }
            }

            /**
             * エラー処理
             * @param strErrMessage
             */
            @Override
            public void onError(String strErrMessage) {
                ShowErrorMessage(strErrMessage);
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
            }

            /**
             * エラー処理
             * @param strErrMessage
             */
            @Override
            public void onConnectError(String strErrMessage) {
                ShowErrorMessage(strErrMessage);
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
            }
        };
    }

    /***
     * マスタデータ登録処理
     * @param context
     * @param activity
     * @param json
     * @return
     */
    public static int dbInsert (Context context, Activity activity, final JSONObject json) {
        SQLiteDatabase db = null;

        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(context);
            db = dbHelper.getWritableDatabase();
            final SQLiteDatabase dbfinal = db;
            db.beginTransaction();

            //20230206 UPD Kawamoto V2.3 START
            //資産マスタのデータを削除する
            //AssetFactory.deleteAssetMaster(db);

            ////資産マスタにデータを登録する
            //AssetFactory.insertAssetMaster(db, json.getJSONArray("adl"));
            int intReturnCd = json.getInt("returnCode");
            //処理結果が5(資産情報未送信)以外の場合
            if (intReturnCd != RETUEN_CODE.AssetNoSend.getInt()) {
                //資産マスタのデータを削除する
                AssetFactory.deleteAssetMaster(db);

                //資産マスタにデータを登録する
                AssetFactory.insertAssetMaster(db, json.getJSONArray("adl"));
            }
            //20230206 UPD Kawamoto V2.3 START

            //社員マスタのデータを削除する
            EmployeeFactory.Delete(db);

            //社員マスタにデータを登録する
            EmployeeFactory.Insert(db, json.getJSONArray("employeeDataList"));

            //ロケーションマスタのデータを削除する
            LocationFactory.Delete(db);

            //ロケーションマスタにデータを登録する
            LocationFactory.Insert(db, json.getJSONArray("locationDataList"));

            //設定マスタのデータを削除する
            SettingFactory.Delete(db);

            //設定マスタにデータを登録する
            SettingFactory.Insert(db, json.getJSONArray("settingDataList"));

            //20210918 ADD Takase V2.0 START
            //権限マスタのデータを削除する
            AuthFactory.Delete(db);

            //権限マスタにデータを登録する
            AuthFactory.Insert(db, json.getJSONArray("authDataList"));
            //20210918 ADD Takase V2.0 END

            //文言マスタのデータを削除する
            WordFactory.Delete(db);

            //文言マスタにデータを登録する
            WordFactory.Insert(db, json.getJSONArray("wordDataList"));

            //文言マスタからデータを取得し設定する
            WordFactory.CreateWordData(db, activity);

            //20231004 ADD Shibata V3.0 START
            //棚卸開始情報テーブルのデータを削除する
            InventoryStartFactory.Delete(db);

            if (json.optJSONArray("invList") != null) {
                //棚卸開始情報テーブルにデータを登録する
                InventoryStartFactory.Insert(db, json.getJSONArray("invList"));
            }
            //20231004 ADD Shibata V3.0 END

            //commit
            db.setTransactionSuccessful();

            return intReturnCd;

        } catch (Exception ex){
            //エラーログ
            if (db != null) {
                db.endTransaction();
                db.close();
                db = null;
            }
            return -1;
        } finally {
            if (db != null) {
                db.endTransaction();
            }
        }
    }

    //キー入力イベント
    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        try {
            switch (keyCode)  {
                case KeyEvent.KEYCODE_BACK:
                    //Back ボタン 無効
                    return false;
            }

            return super.onKeyDown(keyCode, event);
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * 時刻を受信する
     * @return
     */
    private HttpAsyncReciveLoader.Listener timeListener() {

        return new HttpAsyncReciveLoader.Listener() {
            @Override
            public void onSuccess(final JSONObject json) {

                try {
                    //受信した時刻で設定する
                    JSONArray data = json.getJSONArray("getSystemDateTime");

                    //日付のフォーマット変換
                    String strGetDate = data.getJSONObject(0).getString("systemDt");
                    String strGetTime = data.getJSONObject(0).getString("systemTm");

                    //UPD 20230221 Sato v2.3 START
//                    UpdateAndroidTime(strGetDate, strGetTime);

                    //時刻合わせ
                    if (!UpdateAndroidTime(strGetDate, strGetTime)) {
                        //失敗の場合、キーエンスとiDataは処理内でエラーメッセージを出す
                        //デンソーとZEBTAはエラーの場合、Catchに飛ぶ
                        return;
                    }

                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {

                            //マスタを受信
                            HttpAsyncReciveLoader ReciveAsset = new HttpAsyncReciveLoader(activity, _MasterList, _timeout);
                            ReciveAsset.setListener(createListener());
                            ReciveAsset.execute("http://" + _Server + "/" + CommonConst.FIXED_VALUE_WEBAPI_DIRECTORY + "/api/HtGetMasterDataCmd");
                        }
                    });
                    //UPD 20230221 Sato v2.3 END
                }catch (Exception ex){
                    //エラーログ
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", activity) + ":" + Common.toString(ex), activity);
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                }
            }

            @Override
            public void onError(String strErrMessage) {
                ShowErrorMessage(strErrMessage);
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
            }

            /**
             * エラー処理
             * @param strErrMessage
             */
            @Override
            public void onConnectError(String strErrMessage) {
                ShowErrorMessage(strErrMessage);
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
            }
        };
    }

    // Method to parse the XML response using XML Pull Parser
    public void parseXML(XmlPullParser myParser) {
        int event;
        try {
            // Retrieve error details if parm-error/characteristic-error in the response XML
            event = myParser.getEventType();
            while (event != XmlPullParser.END_DOCUMENT) {
                String name = myParser.getName();
                switch (event) {
                    case XmlPullParser.START_TAG:

                        if (name.equals("parm-error")) {
                            parmName = myParser.getAttributeValue(null, "name");
                            errorDescription = myParser.getAttributeValue(null, "desc");
                            errorString = " (Name: " + parmName + ", Error Description: " + errorDescription + ")";
                            return;
                        }
                        if (name.equals("characteristic-error")) {
                            errorType = myParser.getAttributeValue(null, "type");
                            errorDescription = myParser.getAttributeValue(null, "desc");
                            errorString = " (Type: " + errorType + ", Error Description: " + errorDescription + ")";
                            return;
                        }
                        break;
                    case XmlPullParser.END_TAG:

                        break;
                }
                event = myParser.next();
            }

        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    //20230206 UPD Kawamoto V2.3 START
    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        super.onMyDialogSucceeded(requestCode, params);
        if (flgPw) {
            flgPw = false;
            //パスワード入力画面
            CommonInputPasswordDialog.Builder dialog = new CommonInputPasswordDialog.Builder(activity);
            Bundle bundle = new Bundle();
            bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, FORM_REQUEST_PASSWORD);
            //dialog.onCreateDialog(bundle);
            dialog.title(CommonConst.DIALOG_PASSWORD_TITLE);  //パスワード
            dialog.message(SystemSetting.getString(
                    CommonConst.VIEW_WORD_ALL_INPUT_PASSWORD, "", activity));
            dialog.params(bundle);
            dialog.show();

        } else {
            //パスワード入力OKの場合
            if (params.getInt(CommonConst.VIEW_CONFIRM_KEY) == FORM_REQUEST_PASSWORD)  {
                //対象拠点設定画面
                Intent intent = new Intent(getApplication(), SystemSettHub.class);
                startActivity(intent);
            }
            finish();
        }
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {
        flgPw = false;
        finish();
    }
    //20230206 UPD Kawamoto V2.3 END

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_TITLE,"",this));                 //マスタ取込受信
        ((TextView)findViewById(R.id.lblWebApiTitle)).setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_MSG,"",this));                //マスタ取込処理中
    }

    public static String GetReceiveMasterList(Activity activity) throws JsonProcessingException {

        String[] strMaster = new String[]{
                CommonConst.MASTER_ASSET,           //資産マスタ
                CommonConst.MASTER_EMPLOYEE,        //社員マスタ
                CommonConst.MASTER_LOCATION,        //ロケーションマスタ
                CommonConst.MASTER_LEASE_PLACE,     //貸出先マスタ
                CommonConst.MASTER_WORD,            //文言マスタ
                CommonConst.MASTER_SETTING,         //設定マスタ
                CommonConst.MASTER_AUTH,            //権限マスタ          20210918 ADD Takase V2.0
                CommonConst.MASTER_EMPLOYEE_HUB,    //社員所属拠点マスタ  20210918 ADD Takase V2.0
                CommonConst.TABLE_INVENTORY,        //棚卸管理テーブル  　20231006 ADD Shibata v3.0
//                CommonConst.MASTER_CONSTRUCTION,    //工事マスタ 　20241002 ADD Shirakawa v4.0
//                CommonConst.TABLE_RESERVE           //予約マスタ 　20241002 ADD Shirakawa v4.0
//                CommonConst.TABLE_TEMPORARY,     //仮置きテーブル 　20241017 ADD Shimomura v4.0
//                CommonConst.TABLE_TEMP_ASSET,     //仮置き資産テーブル 　20241017 ADD Shimomura v4.0
//                CommonConst.TABLE_CONST_ASSET     //工事資産 　20241017 ADD Shimomura v4.0
        };
        GetMasterData data = new GetMasterData();
        for (String str : strMaster) {
            GetMasterDataDetail master = new GetMasterDataDetail();
            master.setMasterName(str);
            data.setmasterDataList(master);
        }

        //20230206 ADD Kawamoto V2.3 START
        //ログイン社員コードを取得する
        SettingValue C_Setting = new SettingValue();
        String employeeCode = C_Setting.getLoginEmployeeCode(activity);
        data.setEmployeeCd(employeeCode);
        //20230206 ADD Kawamoto V2.3 END

        //Json形式に変換する
        ObjectMapper mapper = new ObjectMapper();
        String MasterList =  mapper.writeValueAsString(data);

        return MasterList;
    }

    /**
     * メニューの設定処理
     * @param menu
     * @return
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_nosetting, menu);
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
     * エラーメッセージを表示する
     * @param strErrMessage
     */
    private void ShowErrorMessage(String strErrMessage) {
        CommonOKDialog.Builder  errdialog = new CommonOKDialog.Builder(this, true);
        errdialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", activity));  //マスタ受信
        errdialog.message(strErrMessage);
        errdialog.params(new Bundle());
        errdialog.show();
    }

    /**
     * メッセージを表示する
     * @param strMessage
     */
    private void ShowOKMessage(String strMessage) {
        //完了メッセージ
        CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", activity));  //マスタ受信
        dialog.message(strMessage);
        dialog.params(new Bundle());
        dialog.show();
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    protected void BarcodeDataReceived(String strData) {}
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) {}
    protected String GetSettingKey(){return "";}
    protected void OpenMainActivity(){ }
    protected void ReaderOn(){}
    protected void ReaderOff(){}
}
