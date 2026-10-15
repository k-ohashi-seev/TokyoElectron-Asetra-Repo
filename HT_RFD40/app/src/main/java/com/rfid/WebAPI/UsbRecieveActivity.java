package com.rfid.WebAPI;

import android.app.Activity;
import android.os.BatteryManager;
import android.content.Context;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.media.MediaScannerConnection;
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
import com.rfid.Factory.ConstRuctionFactory;
import com.rfid.Factory.EmployeeFactory;
import com.rfid.Factory.EmployeeHubFactory;
import com.rfid.Factory.InventoryStartFactory;
import com.rfid.Factory.LeaseFactory;
import com.rfid.Factory.LocationFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Factory.TempAssetFactory;
import com.rfid.Factory.TemporaryFactory;
import com.rfid.Factory.WordFactory;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonInputPasswordDialog;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.GetMasterData;
import com.rfid.common.bean.GetMasterDataDetail;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSettHub;
import com.rfidpk.R;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpCookie;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.text.SimpleDateFormat;
import java.util.Calendar;

public class UsbRecieveActivity extends BaseRFIDActivity implements CommonOKDialog.Callback , CommonConfirmDialog.Callback ,CommonInputPasswordDialog.Callback {

    final Context context = this;
    final Activity activity = this;

    //20230206 ADD Kawamoto V2.3 START
    // フォルダ名
    private final String INPUT_FOLDER = "testInput";
    private final String VERIFY_FOLDER = "test";
    private String folderPath = "";
    private String responseFolderPath = "";

    // ファイル名
    private String fileName = "";
    private String responseFileName = "";
    private File file;
    private  File responseFile;
    private String data;
    private final String TRIGGER_FILE_NAME = "HT_TRIGGER_USB_RECEIVE.JSON";
    private final String MASTER_FILE_NAME = "HT_RECEIVE.JSON";
    private final String RESPONSE_FILE_NAME = "HT_RESPONSE_USB_RECEIVE.JSON";

    // 受信マスタ名
    private String _MasterList = "";

    //タイムアウト設定
    private long startTime = 0;
    private long timeOut = 0;
    private final String TIMEOUT_STRING = "TIMEOUT";
    private final String ERROR_STRING = "ERROR";
    private final String DISCONNECTED_STRING = "DISCONNECTED";

    //メッセージ
    private String returnMsg = "";

    private enum RETUEN_CODE {
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

    /**
     * onCreateイベント
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.webapi_main);

            //progressを表示
            ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.VISIBLE);

            //文言表示
            InitializeWord();

            //処理開始時間を取得
            startTime = System.currentTimeMillis();

            //マスタデータ格納フォルダの設定・初期化
            folderPath = String.valueOf(activity.getExternalFilesDir(INPUT_FOLDER));
            fileName = folderPath + "/" + MASTER_FILE_NAME;
            File rootFile = new File(folderPath);
            File targetFile = new File(fileName);
            if(!rootFile.exists()) {
                //ディレクトリ作成
                rootFile.mkdirs();
            }
            if(targetFile.exists()) {
                //マスタデータの削除
                targetFile.delete();
            }

            //レスポンスファイル削除
            DeleteFile(RESPONSE_FILE_NAME);

            //受信するマスタ名称を設定する
            _MasterList =  GetReceiveMasterList();

            //トリガーファイルを作成する
            WriteTriggerFile(_MasterList);

            new Thread() {
                @Override
                public void run() {

                    try {
                        data = ReadFile(startTime);
                    }catch(Exception ex) {
                        //トリガーファイル削除
                        DeleteFile(TRIGGER_FILE_NAME);

                        //レスポンスファイル削除
                        DeleteFile(RESPONSE_FILE_NAME);

                        //progressを非表示
                        ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                        ShowErrorMessage(ex.getMessage());
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    }

                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {

                            SQLiteDatabase db = null;
                            try {
                                //ファイル読取
                                //String data = ReadFile(startTime);
                                if (data == null || data.trim() == "") {
                                    //データ無しの場合
                                    //progressを非表示
                                    ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                    ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CONFIRM_USB_RECEIVE_ERR, "", activity));
                                    setResult(RESULT_CANCELED);
                                    return;
                                }else if (data == TIMEOUT_STRING) {
                                    //タイムアウトの場合
                                    //progressを非表示
                                    ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                    ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_COMMUNICATION_TIMEOUT, "", activity));
                                    setResult(RESULT_CANCELED);
                                    return;
                                } else if(data == ERROR_STRING) {
                                    //例外エラーの場合
                                    //progressを非表示
                                    ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                    ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ERROR_UNEXPECTED_ERROR, "", activity));
                                    setResult(RESULT_CANCELED);
                                    return;
                                } else if(data == DISCONNECTED_STRING) {
                                    //USB未接続の場合
                                    //progressを非表示
                                    ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                    ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_USB_DISCONNECTED, "", activity));
                                    setResult(RESULT_CANCELED);
                                    return;
                                } else if(!data.contains("returnCode")) {
                                    //WebAPIが不通の場合
                                    //progressを非表示
                                    ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                    ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ERROR_INVALID_RESPONSE_F8ROM_WEBAPI, "", activity));
                                    setResult(RESULT_CANCELED);
                                    return;
                                }

                                //DB接続
                                DatabaseHelper dbHelper = new DatabaseHelper(context);
                                db = dbHelper.getWritableDatabase();
                                db.beginTransaction();

                                JSONObject json = new JSONObject(data);

                                if(json.getString("returnCode").equals("1")) {
                                    //progressを非表示
                                    ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                    ShowErrorMessage(SystemSetting.getString("HM" + json.getString("wordCd"),"",activity));
                                    Log.e(CommonConst.DIALOG_ERROR_TITLE, SystemSetting.getString("HM" + json.getString("wordCd"),"",activity));
                                    setResult(RESULT_CANCELED);
                                    return;
                                }

                                //ファイル読取
//                                String data = ReadFile();
//                                if (data == null || data.trim() == "") {
//                                    ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CONFIRM_USB_RECEIVE_ERR, "", activity));
//                                    return;
//                                }



                                //20230206 UPD Kawamoto V2.3 START
                                ////資産マスタのデータを削除する
                                //AssetFactory.deleteAssetMaster(db);

                                ////資産マスタにデータを登録する
                                //AssetFactory.insertAssetMaster(db, json.getJSONArray("adl"));
                                int intReturnCd = json.getInt("returnCode");
                                //処理結果が5(資産情報未送信)以外の場合
                                if (intReturnCd != RETUEN_CODE.AssetNoSend.getInt()) {
                                    //資産マスタのデータを削除する
                                    AssetFactory.deleteAssetMaster(db);

                                    //資産マスタにデータを登録する
                                    if (json.optJSONArray("adl") != null) {
                                        AssetFactory.insertAssetMaster(db, json.getJSONArray("adl"));
                                    }
                                }
                                //20230206 UPD Kawamoto V2.3 END

                                //工事マスタのデータを削除する
                                ConstRuctionFactory.Delete(db);

                                //仮置きテーブルのデータを削除する
                                TemporaryFactory.Delete(db);

                                //仮置きテーブルのデータを削除する
                                TemporaryFactory.Delete(db);

                                //工事資産テーブルのデータを削除する
                                ConstAssetFactory.Delete(db);

                                //仮置き資産テーブルのデータを削除する
                                TempAssetFactory.Delete(db);

                                if (json.optJSONArray("constList") != null) {
                                    //工事マスタにデータを登録する
                                    ConstRuctionFactory.Insert(db,json.getJSONArray("constList"));
                                }

                                //社員マスタのデータを削除する
                                EmployeeFactory.Delete(db);

                                //社員マスタにデータを登録する
                                EmployeeFactory.Insert(db, json.getJSONArray("employeeDataList"));

                                //ロケーションマスタのデータを削除する
                                LocationFactory.Delete(db);

                                //ロケーションマスタにデータを登録する
                                LocationFactory.Insert(db, json.getJSONArray("locationDataList"));

                                //貸出先マスタのデータを削除する
                                LeaseFactory.Delete(db);

                                //貸出先マスタにデータを登録する
                                LeaseFactory.Insert(db, json.getJSONArray("leasePlaceDataList"));

                                //設定マスタのデータを削除する
                                SettingFactory.Delete(db);

                                //設定マスタにデータを登録する
                                SettingFactory.Insert(db, json.getJSONArray("settingDataList"));

                                //権限マスタのデータを削除する
                                AuthFactory.Delete(db);

                                //権限マスタにデータを登録する
                                AuthFactory.Insert(db, json.getJSONArray("authDataList"));

                                //社員所属拠点マスタのデータを削除する
                                EmployeeHubFactory.Delete(db);

                                //社員所属拠点マスタにデータを登録する
                                EmployeeHubFactory.Insert(db, json.getJSONArray("employeeHubDataList"));

                                //文言マスタのデータを削除する
                                WordFactory.Delete(db);

                                //文言マスタにデータを登録する
                                WordFactory.Insert(db, json.getJSONArray("wordDataList"));

                                //文言マスタからデータを取得し設定する
                                WordFactory.CreateWordData(db, activity);

                                //20231013 ADD Shibata V3.0 START
                                //棚卸開始情報テーブルのデータを削除する
                                InventoryStartFactory.Delete(db);

                                if (json.optJSONArray("invList") != null) {
                                    //棚卸開始情報テーブルにデータを登録する
                                    InventoryStartFactory.Insert(db, json.getJSONArray("invList"));
                                }
                                //20231013 ADD Shibata V3.0 END

                                //commit
                                db.setTransactionSuccessful();

                                //ファイル削除
                                if (file.exists()) {
                                    file.delete();
                                }

                                //トリガーファイル削除
                                DeleteFile(TRIGGER_FILE_NAME);

                                //レスポンスファイル削除
                                DeleteFile(RESPONSE_FILE_NAME);

                                //progressを非表示
                                ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                    //完了メッセージ
                                    ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CONFIRM_RECIVE_DATA, "", activity));   //マスタ取込受信が完了しました。
                                //20230206 UPD Kawamoto V2.3 END

                                setResult(RESULT_OK, new Intent());

                            } catch (Exception ex) {
                                //エラーログ
                                if (db != null) {
                                    db.endTransaction();
                                    db.close();
                                    db = null;
                                }

                                //トリガーファイル削除
                                DeleteFile(TRIGGER_FILE_NAME);

                                //レスポンスファイル削除
                                DeleteFile(RESPONSE_FILE_NAME);

                                LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                        SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity) + ":" + Common.toString(ex), activity);
                                //progressを非表示
                                ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                ShowErrorMessage(ex.getMessage());
                                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));

                                setResult(RESULT_CANCELED);
                            } finally {
                                if (db != null) {
                                    db.endTransaction();
                                }

                                //トリガーファイル削除
                                DeleteFile(TRIGGER_FILE_NAME);

                                //レスポンスファイル削除
                                DeleteFile(RESPONSE_FILE_NAME);

                            }
                        }
                    });
                }
            }.start();

        } catch (Exception ex) {
            //トリガーファイル削除
            DeleteFile(TRIGGER_FILE_NAME);

            //レスポンスファイル削除
            DeleteFile(RESPONSE_FILE_NAME);

            //progressを非表示
            ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
            ShowErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));

            setResult(RESULT_CANCELED);
        }
    }

    /**
     * ファイル読込
     * @param startTime
     * @return
     * @throws IOException
     */
    public String ReadFile(long startTime) throws IOException {

        // フォルダ名
        folderPath = String.valueOf(activity.getExternalFilesDir(INPUT_FOLDER));

        // ファイル名
        fileName = folderPath + "/" + MASTER_FILE_NAME;
        file = new File(fileName);

        //設定値取得
        SettingValue setting = new SettingValue();
        timeOut = Integer.parseInt(setting.getConnectTimeOut(this)) * 1000;

        while(true) {
            //USB接続状態を判別
            if(!GetUSBConnectionStatus()) {
                return DISCONNECTED_STRING;
            }

            //ファイル展開
            if (file.exists()) {
                return ExpandFile(file);
            }

            //タイムアウトの場合は処理終了
            if(System.currentTimeMillis() - startTime > timeOut) {
                return TIMEOUT_STRING;
            }
        }
    }

    /**
     * ファイル展開
     * @param file
     * @return
     * @throws IOException
     */
    public String ExpandFile(File file) throws IOException {

        //ファイル展開
        StringBuffer sb = new StringBuffer();
        int offset = -1;
        char[] buf = new char[1024];

        // フォルダ名
        responseFolderPath = String.valueOf(activity.getExternalFilesDir(VERIFY_FOLDER));
        // ファイル名
        responseFileName = responseFolderPath + "/" + RESPONSE_FILE_NAME;
        responseFile = new File(responseFileName);

        //レスポンスファイルが取得できたら次の処理へ進む
        while(true) {
            if (responseFile.exists()) {
                break;
            }

            //給電状態が充電中・満充電以外の場合は処理終了
            if(!GetUSBConnectionStatus()) {
                return DISCONNECTED_STRING;
            }
        }

        FileInputStream fileInputStream = new FileInputStream(file);
        try (BufferedReader bufferedReader = new BufferedReader(new InputStreamReader(fileInputStream, "UTF-8"))) {
            while (true) {
                buf = new char[1024];
                offset = bufferedReader.read(buf);
                if (offset < 0) break;

                String lineData = String.valueOf(buf);
                if (lineData != null && lineData.trim().length() > 0) {
                    sb.append(lineData);
                } else {
                    break;
                }
            }
        }
        return sb.toString();
    }

    //クリエイトイベント
    @Override
    protected void onStart() {
        super.onStart();
    }

    /**
     * キー入力イベント
     * @param keyCode
     * @param event
     * @return
     */
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
     * レジュームイベント
     */
    @Override
    public void onResume() {
        try {
            super.onResume();
        } catch (Exception ex) {
            ShowErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
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
        getMenuInflater().inflate(R.menu.menu_setting, menu);
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
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_TITLE, "", this));
        ((TextView) findViewById(R.id.lblWebApiTitle)).setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_MSG, "", this));
    }

    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     * @param requestCode CustomDialg 実行時 requestCode
     * @param params CustomDialg に受渡した引数
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        super.onMyDialogSucceeded(requestCode, params);
        //20230206 UPD Kawamoto V2.3 START
        //if (params.containsKey("ERROR")) {
        //setResult(RESULT_CANCELED);
        //} else {
        //setResult(RESULT_OK);
        //}
        if (flgPw) {
            flgPw = false;
            //パスワード入力画面
            CommonInputPasswordDialog.Builder dialog = new CommonInputPasswordDialog.Builder(this);
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
            if (params.getInt(CommonConst.VIEW_CONFIRM_KEY) == FORM_REQUEST_PASSWORD) {
                //対象拠点設定画面
                Intent intent = new Intent(getApplication(), SystemSettHub.class);
                startActivity(intent);
            } else if (params.containsKey("ERROR")) {
                setResult(RESULT_CANCELED);
            } else {
                setResult(RESULT_OK);
            }
            finish();
        }
        //20230206 UPD Kawamoto V2.3 END
    }

    /**
     * 確認ダイアログでキャンセルがクリックされたイベント処理
     * @param requestCode CustomDialg 実行時 requestCode
     * @param params CustomDialg に受渡した引数
     */
    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {
        flgPw = false;      //20230206 ADD Kawamoto V2.3
        setResult(RESULT_OK);
        finish();
    }

    /**
     * エラーメッセージを表示する
     * @param strErrMessage
     */
    private void ShowErrorMessage(String strErrMessage) {
        CommonOKDialog.Builder  errDialog = new CommonOKDialog.Builder(this, true);
        errDialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", activity));
        errDialog.message(strErrMessage);
        Bundle b = new Bundle();
        b.putString("ERROR", "ERROR");
        errDialog.params(b);
        errDialog.show();
    }

    /**
     * メッセージを表示する
     * @param strMessage
     */
    private void ShowOKMessage(String strMessage) {
        //完了メッセージ
        CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", activity));
        dialog.message(strMessage);
        dialog.params(new Bundle());
        dialog.show();
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    /**
     * マスタデータ取得
     * @return
     * @throws JsonProcessingException
     */
    private String GetReceiveMasterList() throws JsonProcessingException {

        String[] strMaster = new String[]{
                //CommonConst.MASTER_SPARE,           //設備予備品マスタ
                CommonConst.MASTER_LOCATION,        //ロケーションマスタ
                //CommonConst.MASTER_SHIP_PLACE,      //出庫先マスタ
                //CommonConst.MASTER_WORKER,          //社員マスタ
                CommonConst.MASTER_WORD,            //文言マスタ
                CommonConst.MASTER_SETTING,         //設定マスタ
                CommonConst.TABLE_INVENTORY,        //棚卸管理テーブル
                CommonConst.MASTER_AUTH,            //権限マスタ
        };
        GetMasterData data = new GetMasterData();
        for (String str : strMaster) {
            GetMasterDataDetail master = new GetMasterDataDetail();
            master.setMasterName(str);
            data.setmasterDataList(master);
        }

        //ログイン社員コードを取得する
        SettingValue C_Setting = new SettingValue();
        String employeeCode = C_Setting.getLoginEmployeeCode(this);
        data.setEmployeeCd(employeeCode);

//        //現在設定されている言語を取得する
//        String word = C_Setting.getTargetLang(this);
//        data.setword(word);
//
//        //要求端末区分
//        String processToolDiv = "H";
//        data.setprocessToolDiv(processToolDiv);

        //Json形式に変換する
        ObjectMapper mapper = new ObjectMapper();
        String MasterList =  mapper.writeValueAsString(data);

        return MasterList;
    }

    /**
     * トリガーファイル作成
     * @param sendData
     * @throws IOException
     */
    public void WriteTriggerFile(String sendData) throws IOException {

        try {
            //フォルダ名
            String folderPath = String.valueOf(this.getExternalFilesDir(VERIFY_FOLDER));
            File rootFile = new File(folderPath);
            if (!rootFile.exists()) {
                //ディレクトリ作成
                rootFile.mkdirs();
            }

            //ファイル名（パス）
            String fileName = folderPath + "/" + TRIGGER_FILE_NAME;
            File file = new File(fileName);
            if (file.exists()) {
                //ファイル再作成
                file.delete();
                file = new File(fileName);
            }

            // ファイル書込
            FileOutputStream fos = new FileOutputStream(fileName);
            byte[] sendJson = null;
            sendJson = sendData.getBytes("UTF-8");
            fos.write(sendJson);

            // 確定
            fos.flush();
            fos.close();

            //作成したファイルをAndroidに認識させる
            String[] path = new String[]{file.getPath()};
            String[] type = new String[]{"application/json"};
            MediaScannerConnection.scanFile(this.getApplicationContext(), path, type, null);
        } catch(IOException e) {
            ShowErrorMessage(e.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
            throw e;
        }
    }

    /**
     * トリガー/レスポンスファイル削除
     * @param targetName
     */
    public void DeleteFile(String targetName) {

        //フォルダ名
        String folderPath = String.valueOf(this.getExternalFilesDir(VERIFY_FOLDER));

        //ファイル名（パス）
        String fileName = folderPath + "/" + targetName;
        File file = new File(fileName);

        //ファイルが存在する場合は削除
        if (file.exists()) {
            file.delete();
        }
    }

    /**
     * 給電状態取得
     * @return
     */
    public boolean GetUSBConnectionStatus() {
        //給電状態（USB・クレードルの接続状態）を取得
        BatteryManager batteryManager = null;
        batteryManager = (BatteryManager) getSystemService(Context.BATTERY_SERVICE);
        int supplyStatus = batteryManager.getIntProperty(BatteryManager.BATTERY_PROPERTY_STATUS);

        //端末が「充電中」or「満充電」の場合はUSBが接続されていると判定
        if(supplyStatus == BatteryManager.BATTERY_STATUS_CHARGING || supplyStatus == BatteryManager.BATTERY_STATUS_FULL) {
            return true;
        }else {
            return false;
        }
    }

    protected void BarcodeDataReceived(String strData) {}
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) {}
    protected String GetSettingKey(){return "";}
    protected void OpenMainActivity(){ }
    protected void ReaderOn(){}
    protected void ReaderOff(){}

}

