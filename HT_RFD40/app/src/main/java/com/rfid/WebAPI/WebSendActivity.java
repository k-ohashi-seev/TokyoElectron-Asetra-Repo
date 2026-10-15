package com.rfid.WebAPI;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.TextView;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.InventoryFactory;
import com.rfid.Factory.LentAssetListFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.ProcessFactory;
import com.rfid.Factory.RetAseetListFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Factory.TempAssetFactory;
import com.rfid.Factory.TemporaryFactory;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.EntryInventData;
import com.rfid.common.bean.EntryLogData;
import com.rfid.common.bean.EntryMatchProcessData;
import com.rfid.common.bean.EntryProcessData;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

/**
 * 実績データを送信するクラス
 * 非同期で送信します。
 */
public class WebSendActivity extends BaseRFIDActivity implements CommonOKDialog.Callback , CommonConfirmDialog.Callback {

    private String _URL = null;
    private int _timeout = 30000;

    final Context context = this;
    final Activity activity = this;

    private int BUTTON_INV_CONFIRM = 1;

    //送信用データ
    private EntryInventData InventData = null;
    private EntryProcessData ProcessData = null;
    private EntryLogData LogDataInv = null;
    private EntryLogData LogDataProcess = null;
    private EntryLogData LogDataError = null;
    private EntryMatchProcessData MatchProcessData = null;

    //貸出照合から直接送信用のコード
    //送信成功
    private int MAT_LEASE_SEND_SUCCESS = 1001;
    //送信失敗
    private int MAT_LEASE_SEND_FAILURE = 1002;

    private enum RETUEN_CODE {
        Error(1),
        Worning(2),
        Confirm(3),
        TagError(4),
        LeaseUpperErr(5),
        LeaseUpperWarn(6),
        LeaseReservationErr(7);                  //20210918 ADD Takase V2.0

        private final int text;
        private RETUEN_CODE(final int text) {
            this.text = text;
        }
        public int getInt() {
            return this.text;
        }
    }

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
            ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.VISIBLE);

            //文言表示
            InitializeWord();

            //設定値を取得する
            //URL
            SettingValue setting = new SettingValue();
            final String strServer = setting.getConnectServer(this);
            if (strServer == null || strServer == "") {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                //20220316 UPD Suzuki START
                //ShowErrorMessage(CommonConst.VIEW_WORD_WEBAPI_ERROR_SETTING);
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_SETTING, "", activity));
                //20220316 UPD Suzuki END
                return;
            }

            //タイムアウト
            _timeout = Integer.parseInt(setting.getConnectTimeOut(this)) * 1000;

            final Activity activity = this;

            new Thread() {
                @Override
                public void run() {
                    SQLiteDatabase db = null;
                    try {
                        //DB接続
                        DatabaseHelper dbHelper = new DatabaseHelper(getApplicationContext());
                        db = dbHelper.getWritableDatabase();

                        //棚卸処理送信データをチェックする
                        InventoryFactory inventory = new InventoryFactory();
                        InventData = inventory.CollectInventoryData(db);

                        //処理履歴データをチェックする
                        ProcessFactory process = new ProcessFactory();
                        ProcessData = process.SelectData(db);

                        //照合データをチェックする
                        MatchProcessData = process.MatSelectData(db);

                        //操作ログデータをチェックする
                        LogFactory log = new LogFactory();
                        LogDataInv = log.SelectData(db, CommonConst.LOG_KIND.Inventory);
                        LogDataProcess = log.SelectData(db, CommonConst.LOG_KIND.Process);
                        LogDataError = log.SelectErrorData(db);
                        //送信データが存在しないまたはエラーログが存在しない場合、エラー
                        if ((InventData == null || InventData.count() <= 0) &&
                                (ProcessData == null || ProcessData.count() <= 0) &&
                                        (MatchProcessData == null || MatchProcessData.count() <= 0))                        {
                            if (LogDataError == null || LogDataError.count() <= 0) {
                                //progressを非表示
                                ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_SEND_DATA, "", activity));
                                return;
                            }
                        }

                        //送信データをjson型にする
                        ObjectMapper mapper = new ObjectMapper();
                        String SendInventoryData = mapper.writeValueAsString(InventData);
                        String SendProcessData = mapper.writeValueAsString(ProcessData);
                        //照合データ
                        String SendMatchProcessData = mapper.writeValueAsString(MatchProcessData);

                        //WebAPI接続先
                        _URL = "http://" + strServer + "/" + CommonConst.FIXED_VALUE_WEBAPI_DIRECTORY + "/api/";
                        //_URL = "http://" + strServer + "/api/";

                        //時刻合わせ受信
                        HttpAsyncSendLoader SendAsset = new HttpAsyncSendLoader(activity, null, _timeout);
                        SendAsset.setListener(timeListener());
                        SendAsset.execute("http://" + strServer + "/" + CommonConst.FIXED_VALUE_WEBAPI_DIRECTORY + "/api/HtGetDateTimeCmd");
                        //SendAsset.execute("http://" + strServer + "/api/HtGetDateTimeCmd");

                        if (InventData != null && InventData.count() > 0) {
                            //棚卸データがある場合、棚卸処理データを送信する
                            HttpAsyncSendLoader SendInventory = new HttpAsyncSendLoader(activity, SendInventoryData, _timeout);
                            SendInventory.setListener(createInventoryListener());
                            SendInventory.execute(_URL + "HtEntryInventDataCmd");
                        } else {
                            //棚卸データなし、処理履歴データがある場合
                            if (ProcessData != null && ProcessData.count() > 0) {
                                //処理履歴データを送信する
                                HttpAsyncSendLoader SendProcess = new HttpAsyncSendLoader(activity, SendProcessData, _timeout);
                                SendProcess.setListener(createProcessListener());
                                SendProcess.execute(_URL + "HtEntryProcessDataCmd");
                                //棚卸データなし、処理履歴データなし、エラーログが存在する場合
                            } else if(MatchProcessData != null && MatchProcessData.count() > 0){
                                //照合データががある場合、照合データを送信する
                                HttpAsyncSendLoader SendMatchProcess = new HttpAsyncSendLoader(activity, SendMatchProcessData, _timeout);
                                SendMatchProcess.setListener(createMatchProcessListener());
                                SendMatchProcess.execute(_URL + "HtEntryMatchProcessDataCmd");
                            }else if (LogDataError != null && LogDataError.count() > 0) {
                                //操作エラーログを送信する
                                String SendErrorLogData = mapper.writeValueAsString(LogDataError);
                                HttpAsyncSendLoader SendErrorLog = new HttpAsyncSendLoader(activity, SendErrorLogData, _timeout);
                                SendErrorLog.setListener(createLogListenerError());
                                SendErrorLog.execute(_URL + "HtEntryLogCmd");
                            }
                        }

                    } catch (Exception ex) {
                        //エラーログ
                        if (db != null) {
                            db.close();
                        }
                        LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity) + ":" + Common.toString(ex), activity);
                        //progressを非表示
                        ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                        ShowErrorMessage(ex.getMessage());
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    } finally {
                        if (db != null) {
                            db.close();
                        }
                    }
                }
            }.start();

        } catch (Exception ex) {
            //progressを非表示
            ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
            ShowErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    //クリエイトイベント
    @Override
    protected void onStart() {
        super.onStart();
    }

    /**
     * 棚卸データを送信する
     * @return
     */
    private HttpAsyncSendLoader.Listener createInventoryListener() {

        return new HttpAsyncSendLoader.Listener() {

            /**
             * 棚卸データ送信処理に成功した場合
             */
            @Override
            public void onSuccess(JSONObject _item) {
                //棚卸用操作ログを送信する
                SendLogInv();
            }

            /**
             * タグ紐付けエラー
             * @param _item
             */
            @Override
            public void onTagError(JSONObject _item){}

            /**
             * 貸出回数上限エラー 20200225 Add Sasaki V1.2
             * @param _item
             */
            @Override
            public void onLeaseError(JSONObject _item){}

            //20210918 ADD Takase V2.0 START
            /**
             * 予約エラー
             * @param _item
             */
            @Override
            public void onReservationError(JSONObject _item){}
            //20210918 ADD Takase V2.0 END

            @Override
            public void onMatchError(JSONObject _item){}

            @Override
            public void onLockError(JSONObject _item) throws JSONException {
                String strErrCd = _item.getString("wordCd");
                // lockAssetListをJSONArrayとして取得
                JSONArray lockArray = _item.getJSONArray("lockAssetList");

                // assetNoをすべてカンマ区切りで結合
                StringBuilder sb = new StringBuilder();
                int count = 0;
                for (int i = 0; i < lockArray.length(); i++) {
                    JSONObject asset = lockArray.getJSONObject(i);
                    if (i > 0) sb.append(", ");
                    sb.append(asset.getString("assetNo"));
                    count++;
                }

                String lockAssetList = sb.toString();
                ShowOKMessage( SystemSetting.getString(strErrCd, "", activity)
                        + System.lineSeparator() + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_COUNT, "", activity) + ":" + count
                        + System.lineSeparator() + lockAssetList);
                //送信データを削除する
                InventoryComplete(context);
            }

            /**
             * 棚卸データ送信処理に失敗した場合
             */
            @Override
            public void onError(String strErrMessage, int intReturnCd) {
                if (intReturnCd == RETUEN_CODE.Error.getInt()) {
                    //エラーの場合終了
                    ShowErrorMessage(strErrMessage);
                } else if ( intReturnCd == RETUEN_CODE.Worning.getInt()) {
                    //棚卸用操作ログを送信する
                    //SendLogInv();         //20191122 Del Sasaki
                    //棚卸を削除する
                    InventoryComplete(context);    //20191122 Add Sasaki
                    //警告メッセージを表示する
                    ShowOKMessage(strErrMessage);
                } else if ( intReturnCd == RETUEN_CODE.Confirm.getInt()) {
                    //確認メッセージの場合
                    ShowConfirmMessage(strErrMessage);
                }
            }

            @Override
            public void onConnectError(String strErrMessage) {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                //エラーの場合終了
                ShowErrorMessage(strErrMessage);
            }
        };
    }

    /**
     * 処理履歴データを送信する
     * @return
     */
    private HttpAsyncSendLoader.Listener createProcessListener() {

        final Context context = this;
        final Activity activity = this;

        return new HttpAsyncSendLoader.Listener() {
            @Override
            public void onSuccess(JSONObject json) {
                //処理履歴用操作ログを送信する
                SendLogProcess();
            }

            @Override
            public void onTagError(JSONObject json){
                try {
                    //データを解析する
                    JSONArray array = json.getJSONArray("assetTagList");

                    StringBuffer sb = new StringBuffer();
                    sb.append(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NUMBER, "", activity));    //資産番号
                    sb.append(":");
                    for (int i = 0; i < array.length(); i++) {

                        //登録済み資産番号、資産IDを作成する
                        if (i > 0) {
                            sb.append(", ");
                        }
                        sb.append(array.getJSONObject(i).getString("dupAssetNo"));
                        Log.d("TagError:" + i, array.getJSONObject(i).toString());
                    }

                    //処理履歴を削除する
                    ProcessComplete();

                    //メッセージを表示する
                    ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LINK_MSG_SET_OTHER_NUMBER, "", activity) + " " + sb.toString()
                            ,SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_TITLE, "", activity));

                } catch (Exception e) {
                    ShowErrorMessage(e.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                }
            }

            /**
             * 貸出回数上限エラー 20200225 Add Sasaki V1.2
             * @param json
             */
            @Override
            public void onLeaseError(JSONObject json){
                try {
                    //データを解析する
                    JSONArray array = json.getJSONArray("assetLeaseList");

                    StringBuffer sb = new StringBuffer();

                    //20231024 UPD Sato v2.4.1 START
//                    sb.append(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NUMBER, "", activity));    //資産番号
//                    sb.append(":");
//                    for (int i = 0; i < array.length(); i++) {
//
//                        //登録済み資産番号、資産IDを作成する
//                        if (i > 0) {
//                            sb.append(", ");
//                        }
//                        sb.append(array.getJSONObject(i).getString("assetNo"));
//                        Log.d("LeaseError:" + i, array.getJSONObject(i).toString());
//                    }

                    sb.append(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_COUNT, "", activity));    //件数
                    sb.append(":");
                    sb.append(array.length());
                    //20231024 UPD Sato v2.4.1 END

                    //処理履歴を削除する
                    ProcessComplete();

                    String wordCd = json.getString("wordCd");
                    if (!json.getString("wordCdLeaseErr").equals("")) {
                        wordCd = json.getString("wordCdLeaseErr");
                    } else if (!json.getString("wordCdLeaseWarn").equals("")) {
                        wordCd = json.getString("wordCdLeaseWarn");
                    }

                    Log.d("LeaseErrorCd:", wordCd);

                    //メッセージを表示する
                    ShowErrorMessage(SystemSetting.getString(wordCd, "", activity) + " " + sb.toString()
                            ,SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_TITLE, "", activity));

                } catch (Exception e) {
                    ShowErrorMessage(e.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                }
            }

            //20210918 ADD Takase V2.0 START

            @Override
            public void onMatchError(JSONObject _item){}

            @Override
            public void onLockError(JSONObject _item) {

            }

            /**
             * 予約エラー
             * @param json
             */
            @Override
            public void onReservationError(JSONObject json){
                try {
                    //データを解析する
                    JSONArray array = json.getJSONArray("reserveList");

                    StringBuffer sb = new StringBuffer();
                    sb.append(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NUMBER, "", activity));    //資産番号
                    sb.append(":");
                    for (int i = 0; i < array.length(); i++) {

                        //登録済み資産番号、資産IDを作成する
                        if (i > 0) {
                            sb.append(", ");
                        }
                        sb.append(array.getJSONObject(i).getString("assetNo"));
                        Log.d("ReservationError:" + i, array.getJSONObject(i).toString());
                    }

                    //処理履歴を削除する
                    ProcessComplete();


                    //メッセージを表示する
                    ShowErrorMessage(SystemSetting.getString(json.getString("wordCdReservation"), "", activity) + " " + sb.toString()
                            ,SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_TITLE, "", activity));

                } catch (Exception e) {
                    ShowErrorMessage(e.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                }
            }
            //20210918 ADD Takase V2.0 END

            @Override
            public void onError(String strErrMessage, int intReturnCd) {
                //20230221 UPD Sato v2.3 DB操作のためtry～catchで囲むように修正
                SQLiteDatabase db = null;
                try {
                    //正常以外の処理
                    if (intReturnCd == RETUEN_CODE.Error.getInt()) {
                        //エラーの場合終了
                        ShowErrorMessage(strErrMessage);
                    } else if ( intReturnCd == RETUEN_CODE.Worning.getInt()) {
                        //※資産上限件数エラーの場合
                        //処理履歴を削除する
                        ProcessComplete();

                        //データベースヘルパーのインスタンスを作成する
                        DatabaseHelper dbHelper = new DatabaseHelper(context);
                        db = dbHelper.getWritableDatabase();

                        //警告メッセージを表示する
                        ShowOKMessage(strErrMessage.replace("{1}", SettingFactory.GetAssetRegistrationLimit(db)));
                    } else {
                        //警告メッセージを表示する
                        ShowOKMessage(strErrMessage); // ここで止まったら問題！！（Confirm）
                    }
                } catch (Exception ex) {
                    ShowErrorMessage(ex.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                } finally {
                    if (db != null) {db.close();}
                }
            }

            @Override
            public void onConnectError(String strErrMessage) {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                //エラーの場合終了
                ShowErrorMessage(strErrMessage);
            }
        };
    }

    /**
     * 照合処理履歴データを送信する
     * @return
     */
    private HttpAsyncSendLoader.Listener createMatchProcessListener() {

        final Context context = this;
        final Activity activity = this;

        return new HttpAsyncSendLoader.Listener() {
            @Override
            public void onSuccess(JSONObject json) {
                //処理履歴用操作ログを送信する
                SendMatLogProcess();

                SQLiteDatabase db = null;
                try {
                    //データベースヘルパーのインスタンスを作成する
                    DatabaseHelper dbHelper = new DatabaseHelper(context);
                    db = dbHelper.getWritableDatabase();

                    //tempIdListがある場合
                    if (!json.isNull("tempIdList")) {
                        //仮置きIDをパラメータから取得
                        JSONArray array = json.getJSONArray("tempIdList");

                        // 件数分繰り返し処理
                        for (int i = 0; i < array.length(); i++) {
                            //仮置きテーブル更新
                            TemporaryFactory.UpdateTempId(db,array.getJSONObject(i).getString("beforeTempId"),array.getJSONObject(i).getString("afterTempId"),array.getJSONObject(i).getString("constId"));
                            //仮置き資産テーブル更新
                            TempAssetFactory.UpdateTempId(db,array.getJSONObject(i).getString("beforeTempId"),array.getJSONObject(i).getString("afterTempId"),array.getJSONObject(i).getString("constId"));
                        }

                    }
                }catch (Exception e) {
                    ShowErrorMessage(e.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                }finally {
                    if (db != null) {db.close();}
                    setResult(MAT_LEASE_SEND_SUCCESS);
                }

            }

            /**
             * タグ紐付けエラー
             * @param _item
             */
            @Override
            public void onTagError(JSONObject _item){}

            /**
             * 貸出回数上限エラー
             * @param _item
             */
            @Override
            public void onLeaseError(JSONObject _item){}

            /**
             * 予約エラー
             * @param _item
             */
            @Override
            public void onReservationError(JSONObject _item){}

            /**
             * 照合処理用エラー
             * @param json
             */
            @Override
            public void onMatchError(JSONObject json){
                try {
                    //データを解析する
                    JSONArray array = json.getJSONArray("outErrorList");

                    //処理履歴を削除する
                    MatchProcessComplete();

                    String msg = SystemSetting.getString(json.getString("wordCd"), "", activity);
                    //{1}の文字列に値追加
                    msg = msg.replace("{1}", array.getJSONObject(0).getString("rep1"));
                    //{2}の文字列に値追加
                    msg = msg.replace("{2}", array.getJSONObject(0).getString("rep2"));
                    //メッセージを表示する
                    ShowErrorMessage(msg,SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_TITLE, "", activity));

                    setResult(MAT_LEASE_SEND_FAILURE);

                } catch (Exception e) {
                    ShowErrorMessage(e.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                }
            }

            @Override
            public void onLockError(JSONObject _item) {

            }

            @Override
            public void onError(String strErrMessage, int intReturnCd) {
                SQLiteDatabase db = null;
                try {
                    //正常以外の処理
                    if (intReturnCd == RETUEN_CODE.Error.getInt()) {
                        //エラーの場合終了
                        ShowErrorMessage(strErrMessage);
                    } else if ( intReturnCd == RETUEN_CODE.Worning.getInt()) {

                        //照合処理履歴を削除する
                        MatchProcessComplete();

                        //データベースヘルパーのインスタンスを作成する
                        DatabaseHelper dbHelper = new DatabaseHelper(context);
                        db = dbHelper.getWritableDatabase();

                        //警告メッセージを表示する
                        ShowOKMessage(strErrMessage.replace("{1}", SettingFactory.GetAssetRegistrationLimit(db)));
                    } else {
                        //警告メッセージを表示する
                        ShowOKMessage(strErrMessage); // ここで止まったら問題！！（Confirm）
                    }
                } catch (Exception ex) {
                    ShowErrorMessage(ex.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                } finally {
                    if (db != null) {db.close();}
                    setResult(MAT_LEASE_SEND_FAILURE);
                }
            }

            @Override
            public void onConnectError(String strErrMessage) {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                //エラーの場合終了
                ShowErrorMessage(strErrMessage);

                setResult(MAT_LEASE_SEND_FAILURE);
            }
        };
    }

    /**
     * 棚卸用操作ログデータを送信する
     * @return
     */
    private HttpAsyncSendLoader.Listener createLogListenerInv() {

        return new HttpAsyncSendLoader.Listener() {
            @Override
            public void onSuccess(JSONObject _item) {

                //棚卸を削除する
                InventoryComplete(context);

                if (ProcessData != null && ProcessData.count() > 0) {
                    try {
                        //処理履歴データを送信する
                        SendProcess();
                    } catch (Exception e) {
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                    }
                } else {
                    //20191016 Upd Start Sasaki
                    //処理履歴がなく、エラーログがある場合
                    if (LogDataError != null && LogDataError.count() > 0){
                        //エラーログを送信する
                        SendLogError();
                    } else {
                        //完了メッセージ
                        ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CONFIRM_SEND_DATA, "", activity));     //送信が完了しました。
                    }
                    //20191016 Upd End Sasaki
                }
            }

            /**
             * タグ紐付けエラー
             * @param _item
             */
            @Override
            public void onTagError(JSONObject _item){ }

            /**
             * 貸出回数上限エラー 20200225 Add Sasaki V1.2
             * @param _item
             */
            @Override
            public void onLeaseError(JSONObject _item){}

            //20210918 ADD Takase V2.0 START
            /**
             * 予約エラー
             * @param _item
             */
            @Override
            public void onReservationError(JSONObject _item){}
            //20210918 ADD Takase V2.0 END

            /**
             * 照合処理用エラー
             * @param _item
             */
            @Override
            public void onMatchError(JSONObject _item){}

            @Override
            public void onLockError(JSONObject _item) {

            }

            @Override
            public void onError(String strErrMessage, int intReturnCd) {
                //操作ログでエラーでも棚卸を削除する
                InventoryComplete(context);

                if (ProcessData != null && ProcessData.count() > 0) {
                    try {
                        //処理履歴データを送信する
                        SendProcess();
                    } catch (Exception e) {
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                    }
                } else {
                    //完了メッセージ
                    ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CONFIRM_SEND_DATA, "", activity));     //送信が完了しました。
                }
            }

            @Override
            public void onConnectError(String strErrMessage) {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                //エラーの場合終了
                ShowErrorMessage(strErrMessage);
            }
        };
    }

    /**
     * 処理履歴ログデータを送信する
     * @return
     */
    private HttpAsyncSendLoader.Listener createLogListenerProcess() {

        return new HttpAsyncSendLoader.Listener() {
            @Override
            public void onSuccess(JSONObject _item) {
                //処理履歴を削除する
                ProcessComplete();
                //完了メッセージ
                ShowOKMessage(SystemSetting.getString(
                        CommonConst.VIEW_WORD_ALL_CONFIRM_SEND_DATA, "", activity));     //送信が完了しました。
            }

            @Override
            public void onError(String strErrMessage, int intReturnCd) {
                //操作ログでも処理履歴を削除する
                ProcessComplete();
                //完了メッセージ
                ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CONFIRM_SEND_DATA, "", activity));     //送信が完了しました。
            }

            @Override
            public void onConnectError(String strErrMessage) {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                //エラーの場合終了
                ShowErrorMessage(strErrMessage);
            }

            /**
             * タグ紐付けエラー
             * @param json
             */
            @Override
            public void onTagError(JSONObject json) { }

            /**
             * 貸出回数上限エラー 20200225 Add Sasaki V1.2
             * @param _item
             */
            @Override
            public void onLeaseError(JSONObject _item){}

            //20210918 ADD Takase V2.0 START
            /**
             * 予約エラー
             * @param _item
             */
            @Override
            public void onReservationError(JSONObject _item){}
            //20210918 ADD Takase V2.0 END


            /**
             * 照合処理用エラー
             * @param _item
             */
            @Override
            public void onMatchError(JSONObject _item){}

            @Override
            public void onLockError(JSONObject _item) {

            }
        };
    }

    /**
     * 処理履歴ログデータを送信する
     * @return
     */
    private HttpAsyncSendLoader.Listener createLogListenerProcessMat() {

        return new HttpAsyncSendLoader.Listener() {
            @Override
            public void onSuccess(JSONObject _item) {
                //処理履歴を削除する
                MatchProcessComplete();
                //完了メッセージ
                ShowOKMessage(SystemSetting.getString(
                        CommonConst.VIEW_WORD_ALL_CONFIRM_SEND_DATA, "", activity));     //送信が完了しました。
            }

            @Override
            public void onError(String strErrMessage, int intReturnCd) {
                //操作ログでも処理履歴を削除する
                MatchProcessComplete();
                //完了メッセージ
                ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CONFIRM_SEND_DATA, "", activity));     //送信が完了しました。
            }

            @Override
            public void onConnectError(String strErrMessage) {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                //エラーの場合終了
                ShowErrorMessage(strErrMessage);
            }

            /**
             * タグ紐付けエラー
             * @param json
             */
            @Override
            public void onTagError(JSONObject json) { }

            /**
             * 貸出回数上限エラー 20200225 Add Sasaki V1.2
             * @param _item
             */
            @Override
            public void onLeaseError(JSONObject _item){}

            //20210918 ADD Takase V2.0 START
            /**
             * 予約エラー
             * @param _item
             */
            @Override
            public void onReservationError(JSONObject _item){}
            //20210918 ADD Takase V2.0 END

            /**
             * 照合処理用エラー
             * @param _item
             */
            @Override
            public void onMatchError(JSONObject _item){}

            @Override
            public void onLockError(JSONObject _item) {

            }
        };
    }

    /**
     * エラーログデータを送信する
     * @return
     */
    private HttpAsyncSendLoader.Listener createLogListenerError() {

        return new HttpAsyncSendLoader.Listener() {
            @Override
            public void onSuccess(JSONObject _item) {
                //エラーログを削除する
                ErrorLogComplete();
                //完了メッセージ
                ShowOKMessage(SystemSetting.getString(
                        CommonConst.VIEW_WORD_ALL_CONFIRM_SEND_DATA, "", activity));     //送信が完了しました。
            }

            @Override
            public void onError(String strErrMessage, int intReturnCd) {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                //エラーの場合終了
                ShowErrorMessage(strErrMessage);
            }

            @Override
            public void onConnectError(String strErrMessage) {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);
                //エラーの場合終了
                ShowErrorMessage(strErrMessage);
            }

            /**
             * タグ紐付けエラー
             * @param json
             */
            @Override
            public void onTagError(JSONObject json) { }

            /**
             * 貸出回数上限エラー 20200225 Add Sasaki V1.2
             * @param _item
             */
            @Override
            public void onLeaseError(JSONObject _item){}

            //20210918 ADD Takase V2.0 START
            /**
             * 予約エラー
             * @param _item
             */
            @Override
            public void onReservationError(JSONObject _item){}
            //20210918 ADD Takase V2.0 END

            /**
             * 照合処理用エラー
             * @param _item
             */
            @Override
            public void onMatchError(JSONObject _item){}

            @Override
            public void onLockError(JSONObject _item) {

            }
        };
    }

    /**
     * 棚卸用操作ログの送信
     */
    private void SendLogInv() {
        try {
            //操作ログデータを送信する
            if (LogDataInv != null && LogDataInv.count() > 0) {
                ObjectMapper mapper = new ObjectMapper();
                String SendLogData = mapper.writeValueAsString(LogDataInv);
                HttpAsyncSendLoader SendLog = new HttpAsyncSendLoader(activity, SendLogData, _timeout);
                SendLog.setListener(createLogListenerInv());
                SendLog.execute(_URL + "HtEntryLogCmd");
            } else {
                //送信データを削除する
                InventoryComplete(context);

                //完了メッセージ
                InventoryCompleteMessage(CommonConst.VIEW_WORD_WEBAPI_DELETE_INVDATA);
            }
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    /**
     * 処理履歴用の操作ログの送信
     */
    private void SendLogProcess() {
        try {
            //操作ログデータを送信する
            if (LogDataProcess != null && LogDataProcess.count() > 0) {
                ObjectMapper mapper = new ObjectMapper();
                String SendLogData = mapper.writeValueAsString(LogDataProcess);
                HttpAsyncSendLoader SendLog = new HttpAsyncSendLoader(activity, SendLogData, _timeout);
                SendLog.setListener(createLogListenerProcess());
                SendLog.execute(_URL + "HtEntryLogCmd");
            } else {
                //送信データを削除する
                ProcessComplete();
                //完了メッセージ
                ShowOKMessage(SystemSetting.getString(
                        CommonConst.VIEW_WORD_ALL_CONFIRM_SEND_DATA, "", activity));     //送信が完了しました。
            }
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    /**
     * 照合処理履歴用の操作ログの送信
     */
    private void SendMatLogProcess() {
        try {
            //操作ログデータを送信する
            if (LogDataProcess != null && LogDataProcess.count() > 0) {
                ObjectMapper mapper = new ObjectMapper();
                String SendLogData = mapper.writeValueAsString(LogDataProcess);
                HttpAsyncSendLoader SendLog = new HttpAsyncSendLoader(activity, SendLogData, _timeout);
                SendLog.setListener(createLogListenerProcessMat());
                SendLog.execute(_URL + "HtEntryLogCmd");
            } else {
                //送信データを削除する
                MatchProcessComplete();
                //完了メッセージ
                ShowOKMessage(SystemSetting.getString(
                        CommonConst.VIEW_WORD_ALL_CONFIRM_SEND_DATA, "", activity));     //送信が完了しました。
            }
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    /**
     * エラー操作ログの送信
     */
    private void SendLogError() {
        try {
            //操作ログデータを送信する
            if (LogDataError != null && LogDataError.count() > 0) {
                ObjectMapper mapper = new ObjectMapper();
                String SendLogData = mapper.writeValueAsString(LogDataError);
                HttpAsyncSendLoader SendLog = new HttpAsyncSendLoader(activity, SendLogData, _timeout);
                //SendLog.setListener(createLogListenerInv());      //20191016 Del Sasaki
                SendLog.setListener(createLogListenerError());      //20191016 Add Sasaki
                SendLog.execute(_URL + "HtEntryLogCmd");
            } else {
                //送信データを削除する
                //InventoryComplete();      //20191016 Del Sasaki
                ErrorLogComplete();         //20191016 Add Sasaki

                //完了メッセージ
                InventoryCompleteMessage(CommonConst.VIEW_WORD_WEBAPI_DELETE_INVDATA);
            }
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }

    /**
     * 処理履歴の送信
     */
    private void SendProcess() {
        try {
            ObjectMapper mapper = new ObjectMapper();
            String SendProcessData = mapper.writeValueAsString(ProcessData);
            HttpAsyncSendLoader SendProcess = new HttpAsyncSendLoader(activity, SendProcessData, _timeout);
            SendProcess.setListener(createProcessListener());
            SendProcess.execute(_URL + "HtEntryProcessDataCmd");
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
    }
    /**
     * 時刻を受信する
     * @return
     */
    private HttpAsyncSendLoader.Listener timeListener() {

        return new HttpAsyncSendLoader.Listener() {
            @Override
            public void onSuccess(final JSONObject json) {
                //20230211 DEL Sato v2.3 送信での時刻合わせは不要のため削除
//                try {
//                    //受信した時刻で設定する
//                    JSONArray data = json.getJSONArray("getSystemDateTime");
//
//                    //日付のフォーマット変換
//                    String strGetDate = data.getJSONObject(0).getString("systemDt");
//                    String strGetTime = data.getJSONObject(0).getString("systemTm");
//                    UpdateAndroidTime(strGetDate, strGetTime);
//
//                }catch (Exception ex){
//                    //エラーログ
//                    LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
//                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity) + ":" + Common.toString(ex), activity);
//                }
            }

            @Override
            public void onError(String strErrMessage, int intValue) { }
            @Override
            public void onTagError(JSONObject json) { }
            /**
             * 貸出回数上限エラー 20200225 Add Sasaki V1.2
             * @param _item
             */
            @Override
            public void onLeaseError(JSONObject _item){}
            @Override
            public void onConnectError(String strErrMessage) {
            }
            
            //20210918 ADD Takase V2.0 START
            /**
             * 予約エラー
             * @param _item
             */
            @Override
            public void onReservationError(JSONObject _item){}
            //20210918 ADD Takase V2.0 END

            /**
             * 照合処理用エラー
             * @param _item
             */
            @Override
            public void onMatchError(JSONObject _item){}

            @Override
            public void onLockError(JSONObject _item) {

            }
        };
    }

    /**
     * 棚卸の完了処理
     */
    public static void InventoryComplete(Context context ) {
        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(context);
            db = dbHelper.getWritableDatabase();
            db.beginTransaction();

            //HT蓄積データのクリア
            InventoryFactory inventory = new InventoryFactory();
            inventory.ClearHTSoreData(db);

            //棚卸のログを削除
            LogFactory log = new LogFactory();
            log.ClearInv(db);

            //コミット
            db.setTransactionSuccessful();

        } finally {
            if (db != null) {
                db.endTransaction();
            }
        }
    }

    private void InventoryCompleteMessage(String strMessage) {
        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(context);
            db = dbHelper.getWritableDatabase();

            //処理履歴がない場合、完了
            ProcessFactory process = new ProcessFactory();
            EntryProcessData ProcessData = process.SelectData(db);

            if (ProcessData == null || ProcessData.count() <= 0) {
                //progressを非表示
                ((ProgressBar)findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);

                //完了メッセージ
                ShowOKMessage(SystemSetting.getString(strMessage, "", activity));     //送信が完了しました。
            }
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 処理履歴の完了処理
     */
    private void ProcessComplete() {
        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(context);
            db = dbHelper.getWritableDatabase();
            db.beginTransaction();

            //処理履歴データのクリア
            ProcessFactory process = new ProcessFactory();
            process.ClearTable(db);

            //処理履歴のログを削除
            LogFactory log = new LogFactory();
            log.ClearProcess(db);

            //コミット
            db.setTransactionSuccessful();

            //progressを非表示
            ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);

        } catch (Exception e) {
            //エラーログ
            if (db != null) {
                db.endTransaction();
                db.close();
                db = null;
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity) + ":" + Common.toString(e), activity);
            ShowErrorMessage(e.getMessage());
        } finally {
            if (db != null) {
                db.endTransaction();
            }
        }
    }

    /**
     * 照合処理履歴の完了処理
     */
    private void MatchProcessComplete() {
        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(context);
            db = dbHelper.getWritableDatabase();
            db.beginTransaction();

            //送信したデータを削除する前に返却リストのデータを更新する
            //照合返却で送ったデータを取得
            String sql;
            Cursor cursor;

            sql = RetAseetListFactory.GetAssetNoConstNoData();
            cursor = db.rawQuery(sql, null);

            while (cursor.moveToNext()){
                //取得する
                @SuppressLint("Range") String AssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));
                @SuppressLint("Range") String ConstNo = cursor.getString(cursor.getColumnIndex("CONST_NO"));
                @SuppressLint("Range") String ContMatchDiv = cursor.getString(cursor.getColumnIndex("CONT_MATCH_DIV"));

                //照合OKの場合
                if(ContMatchDiv.equals(CommonConst.CONST_MATCH_DIV.OK.getString())){
                    //処理履歴IDで照合リストテーブルを削除
                    db.delete(CommonConst.TABLE_CONST_ASSET, "ASSET_NO = ? AND CONST_NO = ?", new String[] {AssetNo,ConstNo});
                }

            }

            //照合処理履歴データのクリア
            ProcessFactory process = new ProcessFactory();
            process.ClearMatTable(db);


            //照合処理履歴のログを削除
            LogFactory log = new LogFactory();
            log.ClearProcess(db);

            //コミット
            db.setTransactionSuccessful();

            //progressを非表示
            ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);

        } catch (Exception e) {
            //エラーログ
            if (db != null) {
                db.endTransaction();
                db.close();
                db = null;
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity) + ":" + Common.toString(e), activity);
            ShowErrorMessage(e.getMessage());
        } finally {
            if (db != null) {
                db.endTransaction();
            }
        }
    }

    /**
     * エラーログの完了処理
     */
    private void ErrorLogComplete() {
        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(context);
            db = dbHelper.getWritableDatabase();
            db.beginTransaction();

            //エラーログを削除
            LogFactory log = new LogFactory();
            log.ClearErrorLog(db);

            //コミット
            db.setTransactionSuccessful();

            //progressを非表示
            ((ProgressBar) findViewById(R.id.progressBar)).setVisibility(View.INVISIBLE);

        } catch (Exception e) {
            //エラーログ
            if (db != null) {
                db.endTransaction();
                db.close();
                db = null;
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity) + ":" + Common.toString(e), activity);
            ShowErrorMessage(e.getMessage());
        } finally {
            if (db != null) {
                db.endTransaction();
            }
        }
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
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_TITLE,"",this));                 //実績データ送信
        ((TextView)findViewById(R.id.lblWebApiTitle)).setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_MSG,"",this));                //実績送信中
    }

    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     * @param requestCode CustomDialg 実行時 requestCode
     * @param params CustomDialg に受渡した引数
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        if (params.getInt(CommonConst.VIEW_CONFIRM_KEY) == BUTTON_INV_CONFIRM) {

            //棚卸用操作ログを送信する
            SendLogInv();

            //完了メッセージ
//            InventoryCompleteMessage(CommonConst.VIEW_WORD_WEBAPI_DELETE_INVDATA);
        } else {
            finish();
        }
    }

    /**
     * 確認ダイアログでキャンセルがクリックされたイベント処理
     * @param requestCode CustomDialg 実行時 requestCode
     * @param params CustomDialg に受渡した引数
     */
    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {
        finish();
    }

    /**
     * エラーメッセージを表示する
     * @param strErrMessage
     */
    private void ShowErrorMessage(String strErrMessage) {
        CommonOKDialog.Builder  errdialog = new CommonOKDialog.Builder(this, true);
        errdialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity));
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
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity));
        dialog.message(strMessage);
        dialog.params(new Bundle());
        dialog.show();
    }

    /**
     * 確認メッセージを表示する
     * @param strMessage
     */
    private void ShowConfirmMessage(String strMessage) {
        //完了メッセージ
        CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity));
        dialog.message(strMessage);
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_INV_CONFIRM);
        dialog.params(bundle);
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
