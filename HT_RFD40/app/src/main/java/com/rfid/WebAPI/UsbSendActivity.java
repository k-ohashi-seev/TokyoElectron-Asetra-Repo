package com.rfid.WebAPI;

import android.app.Activity;
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
import android.os.BatteryManager;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.rfid.Factory.InventoryFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.MatchProcessFactory;
import com.rfid.Factory.ProcessFactory;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.EntryInventData;
import com.rfid.common.bean.EntryLogData;
import com.rfid.common.bean.EntryProcessData;
import com.rfidpk.R;
import com.rfid.setting.SettingValue;

import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Calendar;

public class UsbSendActivity extends BaseRFIDActivity implements CommonOKDialog.Callback , CommonConfirmDialog.Callback {

    final Context context = this;
    final Activity activity = this;
    private final ObjectMapper mapper = new ObjectMapper();
    private SQLiteDatabase db;

    //送信用データ
    private EntryInventData inventData = null;
    private EntryProcessData processData = null;
    private EntryLogData logDataInv = null;
    private EntryLogData logDataProcess = null;
    private EntryLogData logDataError = null;

    // フォルダ名
    private final String OUTPUT_FOLDER = "testOutput";
    private final String VERIFY_FOLDER = "test";
    private String folderPath = "";

    // ファイル名
    private String fileName = "";
    private File file;
    private final String TRIGGER_FILE_NAME = "HT_TRIGGER_USB_SEND.JSON";
    private final String RESPONSE_FILE_NAME = "HT_RESPONSE_USB_SEND.JSON";
    private final String DISCONNECTED_STRING = "DISCONNECTED";
    private String data;

    //タイムアウト設定
    private long startTime = 0;
    private long timeOut = 0;
    private final String TIMEOUT_STRING = "TIMEOUT";
    private final String ERROR_STRING = "ERROR";

    //データクリア設定
    private final boolean dataClearOK = true;
    private final boolean dataClearNG = false;

    /**
     * onCreateイベント
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        //SQLiteDatabase db = null;
        db=null;
        final Activity activity = this;

        try {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.webapi_main);

            //progressを表示
            ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.VISIBLE);

            //文言表示
            InitializeWord();

            //処理開始時間を取得
            startTime = System.currentTimeMillis();

            new Thread() {
                @Override
                public void run() {
                    //DB接続
                    DatabaseHelper dbHelper = new DatabaseHelper(getApplicationContext());
                    db = dbHelper.getWritableDatabase();

                    try {
                        if (!GetUSBConnectionStatus()) {
                            data = DISCONNECTED_STRING;
                        }else{
                            //棚卸処理送信データをチェックする
                            InventoryFactory inventory = new InventoryFactory();
                            inventData =inventory.CollectInventoryData(db);

                            //処理履歴データをチェックする
                            ProcessFactory process = new ProcessFactory();
                            processData =process.SelectData(db);

                            //照合データをチェックする
                            MatchProcessFactory matchProcess = new MatchProcessFactory();

                            //操作ログデータをチェックする
                            LogFactory log = new LogFactory();
                            //logDataInv =log.Select(db);

                            //送信データをjson型にする
                            ObjectMapper mapper = new ObjectMapper();
                            String inventorySendData = mapper.writeValueAsString(inventData);
                            String processSendData = mapper.writeValueAsString(processData);
                            //String matchProcessSendData = mapper.writeValueAsString(matchProcessData)
                            String logSendDataInv = mapper.writeValueAsString(logDataInv);
                            //String logSendDataProcess = mapper.writeValueAsString(logDataProcess);
                            //String logSendDataError = mapper.writeValueAsString(logDataError);

                            //jsonの中にjson
                            SendData sendData = new SendData();
                            sendData.InventData =inventorySendData;
                            sendData.ProcessData =processSendData;
                            //sendData.MatchProcessData =
                            sendData.LogDataInv =logSendDataInv;
                            //sendData.LogDataProcess =logSendDataProcess;
                            //sendData.LogDataError =logSendDataError;

                            String sendAllData = mapper.writeValueAsString(sendData);

                            //ファイル作成
                            WriteFile(sendAllData);

                            //トリガーファイルを作成する
                            WriteTriggerFile();

                            //レスポンスファイル監視
                            data = WatchFile(startTime);
                        }
                        runOnUiThread(new Runnable() {
                                          @Override
                                          public void run() {
                                              try {
                                                  if (data == ERROR_STRING) {
                                                      ClearData(dataClearOK);
                                                      LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                                              SystemSetting.getString(CommonConst.VIEW_WORD_SEND_DATA_TITLE, "", activity) + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ERROR_UNEXPECTED_ERROR, "", activity), activity);
                                                      //progressを非表示
                                                      ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                                      ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ERROR_UNEXPECTED_ERROR, "", activity));
                                                      Log.e(CommonConst.DIALOG_ERROR_TITLE, SystemSetting.getString(CommonConst.VIEW_WORD_ERROR_UNEXPECTED_ERROR, "", activity));

                                                      return;
                                                  } else if (data == TIMEOUT_STRING) {
                                                      ClearData(dataClearOK);
                                                      LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                                              SystemSetting.getString(CommonConst.VIEW_WORD_SEND_DATA_TITLE, "", activity) + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_COMMUNICATION_TIMEOUT, "", activity), activity);
                                                      //progressを非表示
                                                      ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                                      ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_COMMUNICATION_TIMEOUT, "", activity));
                                                      Log.e(CommonConst.DIALOG_ERROR_TITLE, SystemSetting.getString(CommonConst.VIEW_WORD_COMMUNICATION_TIMEOUT, "", activity));

                                                      return;
                                                  } else if (data == null || data.trim() == "") {
                                                      ClearData(dataClearOK);
                                                      LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                                              SystemSetting.getString(CommonConst.VIEW_WORD_SEND_DATA_TITLE, "", activity) + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ERROR_NO_RESPONSE_FROM_WEBAPI, "", activity), activity);
                                                      //progressを非表示
                                                      ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                                      ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ERROR_NO_RESPONSE_FROM_WEBAPI, "", activity));
                                                      Log.e(CommonConst.DIALOG_ERROR_TITLE, SystemSetting.getString(CommonConst.VIEW_WORD_ERROR_NO_RESPONSE_FROM_WEBAPI, "", activity));

                                                      return;
                                                  } else if (data == DISCONNECTED_STRING) {
                                                      LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                                              SystemSetting.getString(CommonConst.VIEW_WORD_SEND_DATA_TITLE, "", activity) + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ERROR_UNEXPECTED_ERROR, "", activity), activity);
                                                      //progressを非表示
                                                      ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                                      ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_USB_DISCONNECTED, "", activity));
                                                      Log.e(CommonConst.DIALOG_ERROR_TITLE, SystemSetting.getString(CommonConst.VIEW_WORD_USB_DISCONNECTED, "", activity));
                                                      return;
                                                  }

                                                  JSONObject json = new JSONObject(data);
                                                  if (json.getString("returnCode").equals("0")) {
                                                      //WebAPIの応答結果が正常の場合
                                                      ClearData(dataClearOK);
                                                      //progressを非表示
                                                      ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                                      //完了メッセージ
                                                      ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_STORED_DATA_TRANSFER_COMPLETED, "", activity));   //蓄積データの送信が完了しました。
                                                      setResult(RESULT_OK, new Intent());
                                                  } else if (json.getString("returnCode").equals("1")) {
                                                      //WebAPIの応答結果がエラーの場合
                                                      ClearData(dataClearOK);
                                                      LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                                              SystemSetting.getString(CommonConst.VIEW_WORD_SEND_DATA_TITLE, "", activity) + ":" + SystemSetting.getString("HM" + json.getString("wordCd"), "", activity), activity);
                                                      //progressを非表示
                                                      ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                                      ShowErrorMessage(SystemSetting.getString("HM" + json.getString("wordCd"), "", activity));
                                                      Log.e(CommonConst.DIALOG_ERROR_TITLE, SystemSetting.getString("HM" + json.getString("wordCd"), "", activity));

                                                      return;
                                                  } else if (json.getString("returnCode").equals("3")) {
                                                      //WebAPIの応答結果が確認の場合
                                                      LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                                              SystemSetting.getString(CommonConst.VIEW_WORD_SEND_DATA_TITLE, "", activity) + ":" + SystemSetting.getString("HM" + json.getString("wordCd"), "", activity), activity);
                                                      //progressを非表示
                                                      Log.e(CommonConst.DIALOG_ERROR_TITLE, SystemSetting.getString("HM" + json.getString("wordCd"), "", activity));
                                                      ShowInvConfirmMessage(SystemSetting.getString(CommonConst.VIEW_WORD_DELETE_STOCKTAKING_DATA, "", activity));
                                                  }
                                              } catch (Exception ex) {
                                                  if (db != null) {
                                                      db.close();
                                                      db = null;
                                                  }
                                                  LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                                          SystemSetting.getString(CommonConst.VIEW_WORD_SEND_DATA_TITLE, "", activity) + ":" + Common.toString(ex), activity);
                                                  //progressを非表示
                                                  ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                                  ShowErrorMessage(ex.getMessage());
                                                  Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                                              } finally {
                                                  if (db != null) {
                                                      db.close();
                                                      db = null;
                                                  }
                                                  //トリガーファイル削除
                                                  DeleteFile(TRIGGER_FILE_NAME);

                                                  //レスポンスファイル削除
                                                  DeleteFile(RESPONSE_FILE_NAME);
                                              }
                                          }
                                      });
                    }catch(Exception ex) {
                        //エラーログ
                        if (db != null) {
                            db.close();
                            db = null;
                        }
                        LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                SystemSetting.getString(CommonConst.VIEW_WORD_ALL_USB_SEND_DATA, "", activity) + ":" + Common.toString(ex), activity);
                        //progressを非表示
                        ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                        ShowErrorMessage(ex.getMessage());
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    }
                }
                //String sendAllData = mapper.writeValueAsString(sendData);

                //ファイル作成
                //WriteFile(sendAllData);

                //ファイル作成完了ダイアログ
                //ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CONFIRM_USB_RECEIVE,"",this));

                //蓄積データクリア
                //ClearData();
            }.start();
        } catch (Exception ex) {
            //エラーログ
            if (db != null) {
                db.close();
                db = null;
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_USB_SEND_DATA, "", activity) + ":" + Common.toString(ex), activity);
            //progressを非表示
            ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
            ShowErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    public class SendData {
        public String InventData;
        public String ProcessData;
        public String MatchProcessData;
        public String LogDataInv;
        public String LogDataProcess;
        public String LogDataError;
    }

    /**
     * ファイル作成
     * @param sendData
     * @throws IOException
     */
    public void WriteFile(String sendData) throws IOException {
        //日時取得
        Calendar calendar = Calendar.getInstance();
        SimpleDateFormat sdf = new SimpleDateFormat("yyyyMMddHHmmss");
        String strDate = sdf.format(calendar.getTime());

        //フォルダ名
        final String OUTPUT_FOLDER = "testOutput";

        // 出力フォルダ
        String folderPath = String.valueOf(activity.getExternalFilesDir(OUTPUT_FOLDER));
        File rootFile = new File(folderPath);

        if (!rootFile.exists()) {
            //ディレクトリ作成
            rootFile.mkdirs();
        }

        // ファイル名
        String fileName = folderPath + "/HT_SEND_" + Common.getPdaId(Common.getIpAddress()) + "_" + strDate + ".JSON";
        File file = new File(fileName);
        if (file.exists()) {
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
        MediaScannerConnection.scanFile(activity.getApplicationContext(), path, type, null);

    }

    /**
     * ファイル作成完了処理
     */
    private void ClearData(boolean deleteFlg) {
        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(context);
            db = dbHelper.getWritableDatabase();
            db.beginTransaction();

            if(deleteFlg == true) {
                //棚卸データのクリア
                //InventoryFactory.Delete(db);
                //_SpareData.Clear();
            }

            //処理履歴データのクリア
            LogFactory process = new LogFactory();
//           process.ClearTable(db);  あとで必要かも

            //処理履歴のログを削除
            LogFactory log = new LogFactory();
            log.ClearProcess(db);

            // エラーログを削除
//            log.Clear(db);  あとで必要かも

            //コミット
            db.setTransactionSuccessful();

        }catch(Exception e){
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_USB_SEND_DATA, "", activity) + ":" + Common.toString(e), activity);
        }finally {
            if (db != null) {
                db.endTransaction();
            }
        }
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
        //TODO:
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_TITLE, "", this));
        ((TextView) findViewById(R.id.lblWebApiTitle)).setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_MSG, "", this));
    }

    /**
     * トリガーファイル作成
     * @throws IOException
     */
    public void WriteTriggerFile() throws IOException {

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
        file.createNewFile();

        //作成したファイルをAndroidに認識させる
        String[] path = new String[]{file.getPath()};
        String[] type = new String[]{"application/json"};
        MediaScannerConnection.scanFile(this.getApplicationContext(), path, type, null);
    }

    /**
     * レスポンスファイル監視
     * @param startTime
     * @return
     * @throws IOException
     */
    public String WatchFile(long startTime) throws IOException {

        //フォルダ名
        String folderPath = String.valueOf(this.getExternalFilesDir(VERIFY_FOLDER));
        File rootFile = new File(folderPath);
        if (!rootFile.exists()) {
            //ディレクトリ作成
            rootFile.mkdirs();
        }

        //ファイル名（パス）
        fileName = folderPath + "/" + RESPONSE_FILE_NAME;
        file = new File(fileName);

        //設定値取得
        SettingValue setting = new SettingValue();
        timeOut = Integer.parseInt(setting.getConnectTimeOut(this)) * 1000;

        while(true) {
            //給電状態が充電中・満充電以外の場合は処理終了
            if(!GetUSBConnectionStatus()) {
                return DISCONNECTED_STRING;
            }

            //ファイル展開
            if (file.exists()) {
                return CheckFile(file,startTime);
            }

            //タイムアウトの場合は処理終了
            if(System.currentTimeMillis() - startTime > timeOut) {
                return TIMEOUT_STRING;
            }
        }
    }

    /**
     * ファイル確認
     * @param file
     * @return
     * @throws IOException
     */
    public String CheckFile(File file,long startTime) throws IOException {

        int offset = -1;
        String content = "";
        String line;

        try(BufferedReader reader = new BufferedReader(new FileReader(file))) {
            //正常にファイルを読み込めるまで繰り返す
            while(offset == -1) {
                offset = reader.read();

                line = reader.readLine();
                if(line!= null && line.trim().length() > 0) {
                    //content.append(line).append(System.lineSeparator());
                    content = line;
                }

                //タイムアウトの場合は処理終了
                if(System.currentTimeMillis() - startTime > timeOut) {
                    return TIMEOUT_STRING;
                }

                try {
                    Thread.sleep(100);
                } catch(InterruptedException e) {
                    return ERROR_STRING;
                }
            }
        }

        return content;
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
     * 棚卸確認ダイアログを表示する
     * @param strMessage 棚卸処理が終了しています。棚卸データを削除しますか？
     */
    private void ShowInvConfirmMessage(String strMessage) {
        CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
        dialog.message(strMessage);
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, CommonConst.BUTTON_INV_CONFIRM);
        dialog.params(bundle);
        dialog.show();
    }

    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     * @param requestCode CustomDialg 実行時 requestCode
     * @param params CustomDialg に受渡した引数
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        super.onMyDialogSucceeded(requestCode, params);
        setResult(RESULT_OK);
        finish();
    }

    /**
     * 確認ダイアログでキャンセルがクリックされたイベント処理
     * @param requestCode CustomDialg 実行時 requestCode
     * @param params CustomDialg に受渡した引数
     */
    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {
        setResult(RESULT_OK);
        finish();
    }

    /**
     * エラーメッセージを表示する
     * @param strErrMessage
     */
    private void ShowErrorMessage(String strErrMessage) {
        CommonOKDialog.Builder  errDialog = new CommonOKDialog.Builder(this, true);
        errDialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity));
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
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity));
        dialog.message(strMessage);
        dialog.params(new Bundle());
        dialog.show();
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
