package com.rfid.main;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import androidx.appcompat.widget.Toolbar;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import com.rfid.Factory.InventoryFactory;
import com.rfid.Factory.InventoryStartFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.ProcessFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Lease.LeaseRead;
import com.rfid.Lease.LeaseSetting;
import com.rfid.Search.SearchRead;
import com.rfid.Taglink.TaglinkRead;
import com.rfid.WebAPI.UsbConfirmActivity;
import com.rfid.WebAPI.WebReciveActivity;
import com.rfid.WebAPI.WebSendActivity;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonProgres;
import com.rfid.common.ConstRuction;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;

/**
 * メニュー画面処理クラス
 */
public class MainActivity extends BaseRFIDActivity implements CommonConfirmDialog.Callback {

    //プログレスダイアログ クラス
    private CommonProgres C_Progres = new CommonProgres();

    //DB接続
    private SQLiteDatabase _db = null;

    CommonConfirmDialog.Builder  _confirmDialog = null;

    //蓄積データ有無フラグ
    private boolean _isExistReciveData = false;

    private final static int ACTION_LOGOUT = 1;

    //送受信画面キー
    private final static int FORM_REQUEST_RECEIVE = 1;
    private final static int FORM_REQUEST_SEND = 2;
    private final static int BUTTON_SETTEI = 9;

    /** クリックイベント実行可否フラグ */
    private boolean mIsClickEvent;

    /**画面モード */
    private CommonConst.MENU_MODE _MenuMode = CommonConst.MENU_MODE.Normal;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {

        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_main);
            Toolbar toolbar = findViewById(R.id.toolbarMain);
            setSupportActionBar(toolbar);
            final CommonAuthority.FunctionId functionId = new CommonAuthority.FunctionId();  //20210918 ADD Takase V2.0

            _confirmDialog = new CommonConfirmDialog.Builder(this);

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            _db = dbHelper.getWritableDatabase();

            //メニューモード取得
            GetMenuMode();

            //文言表示
            InitializeWord();

            //操作時間のタイマーをセット
            StartTimeoutTimer(CommonConst.VIEW_WORD_MENU_TITLE);

            //20210918 ADD Takase V2.0 START
            //「貸出」初期化  レジュームイベントへ移動  20230906 DEL okubo V2.4.1
       //     _ProcessData.setIsLease(false);
            //20210918 ADD Takase V2.0 END

            //ボタン1 Clickイベント
            final Activity activity = this;
            findViewById(R.id.btnMnu1).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    // クリックイベント実行されていれば実行しない
                    if (mIsClickEvent) return;
                    // クリックイベントを禁止する
                    mIsClickEvent = true;
                    if (checkTimeout()) {return;}
                    String methodName = "";
                    Intent intent;
                    switch (_MenuMode) {
                        case Normal:      //通常
                        case MatControl:  //照合(管理)
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", activity), activity);

                            //20210918 ADD Takase V2.0 START
                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_INVENTORY);
                            //20210918 ADD Takase V2.0 END
                            //棚卸画面へ
                            try {
                                //DB接続
                                DatabaseHelper dbHelper = new DatabaseHelper(MainActivity.this);
                                _db = dbHelper.getWritableDatabase();

                                //実棚方法を取得
                                final String strInventoryMethod = SettingFactory.GetInventoryMethod(_db);

                                //実棚ボタンイベント処理
                                EventButtonInventory(strInventoryMethod);

                            } finally {
                                if (_db != null) {
                                    _db.close();
                                }
                            }

                            break;
                        case MatGeneral:  //照合(一般)仮置きなし
                        case MatGeneralTmpo:  //照合(一般)仮置きあり
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING, "", activity), activity);

                            //管理
                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE);

                            //貸出(照合)画面へ
                            if (CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())){
                                //照合貸出
                                intent = new Intent(getApplication(), ConstRuction.class);
                                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_LENDING);
                                startActivity(intent);
                            }else {
                                //通常貸出
                                if (SettingFactory.GetReservationFunction(_db)
                                        .equals((CommonConst.RESERVATION_FUNCTION.YES.getString()))) {
                                    //予約機能あり
                                    intent = new Intent(getApplication(), LeaseRead.class);
                                } else {
                                    //予約機能なし
                                    intent = new Intent(getApplication(), LeaseSetting.class);
                                }
                                startActivity(intent);
                            }

                            break;
                    }
                }
            });

            //ボタン2 Clickイベント
            findViewById(R.id.btnMnu2).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    // クリックイベント実行されていれば実行しない
                    if (mIsClickEvent) return;
                    // クリックイベントを禁止する
                    mIsClickEvent = true;
                    if (checkTimeout()) {return;}

                    String methodName = "";
                    Intent intent;
                    switch (_MenuMode) {
                        case Normal:      //通常
                        case MatControl:  //照合(管理)
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", activity), activity);

                            //20210918 ADD Takase V2.0 START
                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_SEARCH);
                            //20210918 ADD Takase V2.0 END
                            //探索画面へ
                            intent = new Intent(getApplication(), SearchRead.class);
                            startActivity(intent);
                            break;

                        case MatGeneral:  //照合(一般)仮置きなし
                        case MatGeneralTmpo:  //照合(一般)仮置きあり
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHRETURN, "", activity), activity);

                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHRETURN);

                            //照合返却画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_MATCHRETURN);
                            startActivity(intent);

                            break;
                    }
                }
            });

            //ボタン3 Clickイベント
            findViewById(R.id.btnMnu3).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    // クリックイベント実行されていれば実行しない
                    if (mIsClickEvent) return;
                    // クリックイベントを禁止する
                    mIsClickEvent = true;
                    if (checkTimeout()) {return;}

                    String methodName = "";
                    Intent intent;
                    switch (_MenuMode) {
                        case Normal:      //通常
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING, "", activity), activity);

                            //一般の場合
                            //20210918 ADD Takase V2.0 START
                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_LEASE);
                            //20210918 ADD Takase V2.0 END
                            //貸出画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_LENDING);
                            startActivity(intent);

                            break;
                        case MatControl:  //照合(管理)
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", activity), activity);

                            //管理
                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE);

                            //貸出(照合)画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_LENDING);
                            startActivity(intent);

                            break;
                        case MatGeneral:  //照合(一般)仮置きなし
                        case MatGeneralTmpo:  //照合(一般)仮置きあり
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHIN, "", activity), activity);

                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHIN);

                            //持込照合画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_MATCHIN);
                            startActivity(intent);

                            break;
                    }

                }
            });

            //ボタン4 Clickイベント
            findViewById(R.id.btnMnu4).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    // クリックイベント実行されていれば実行しない
                    if (mIsClickEvent) return;
                    // クリックイベントを禁止する
                    mIsClickEvent = true;
                    if (checkTimeout()) {return;}

                    String methodName = "";
                    Intent intent;
                    switch (_MenuMode) {
                        case Normal:      //通常
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_RETURN, "", activity), activity);

                            //20210918 ADD Takase V2.0 START
                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_RETURN);
                            //20210918 ADD Takase V2.0 END
                            //返却画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_RETURN);
                            startActivity(intent);

                            break;
                        case MatControl:  //照合(管理)
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHRETURN, "", activity), activity);

                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHRETURN);

                            //照合返却画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_MATCHRETURN);
                            startActivity(intent);

                            break;
                        case MatGeneral:  //照合(一般)仮置きなし
                        case MatGeneralTmpo:  //照合(一般)仮置きあり
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHIN, "", activity), activity);

                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHOUT);

                            //持帰照合画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_MATCHOUT);
                            startActivity(intent);

                            break;
                    }

                }
            });

            //ボタン5 Clickイベント
            findViewById(R.id.btnMnu5).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    // クリックイベント実行されていれば実行しない
                    if (mIsClickEvent) return;
                    // クリックイベントを禁止する
                    mIsClickEvent = true;
                    if (checkTimeout()) {return;}

                    String methodName = "";
                    Intent intent;

                    switch (_MenuMode) {
                        case Normal:      //通常
                        case MatControl:  //照合(管理)
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_STORE, "", activity), activity);

                            //20210918 ADD Takase V2.0 START
                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_STORAGE);
                            //20210918 ADD Takase V2.0 END
                            //保管画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_STORE);
                            startActivity(intent);

                            break;
                        case MatGeneral:  //照合(一般)仮置きなし
                        case MatGeneralTmpo:  //照合(一般)仮置きあり
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", activity), activity);

                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_RETURN);
                            //返却画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_RETURN);
                            startActivity(intent);

                            break;
                    }
                }
            });

            //ボタン6 Clickイベント
            findViewById(R.id.btnMnu6).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    // クリックイベント実行されていれば実行しない
                    if (mIsClickEvent) return;
                    // クリックイベントを禁止する
                    mIsClickEvent = true;
                    if (checkTimeout()) {return;}

                    String methodName = "";
                    Intent intent;
                    switch (_MenuMode) {
                        case Normal:      //通常
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_DELETE, "", activity), activity);

                            //20210918 ADD Takase V2.0 START
                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_DISPOSE);
                            //20210918 ADD Takase V2.0 END
                            //除却画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_DELETE);
                            startActivity(intent);

                            break;
                        case MatControl:  //照合(管理)
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", activity), activity);

                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_RETURN);
                            //返却画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_RETURN);
                            startActivity(intent);
                            break;
                        case MatGeneral:  //照合(一般)仮置きなし
                        case MatGeneralTmpo:  //照合(一般)仮置きあり
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", activity), activity);

                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_SEARCH);

                            //探索画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_SEARCH);
                            startActivity(intent);

                            break;
                    }


                }
            });

            //ボタン7 Clickイベント
            findViewById(R.id.btnMnu7).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    // クリックイベント実行されていれば実行しない
                    if (mIsClickEvent) return;
                    // クリックイベントを禁止する
                    mIsClickEvent = true;
                    if (checkTimeout()) {return;}

                    String methodName = "";
                    Intent intent;
                    switch (_MenuMode) {
                        case Normal:      //通常
                        case MatControl:  //照合(管理)
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG, "", activity), activity);

                            //20210918 ADD Takase V2.0 START
                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_TAG_LINK);
                            //20210918 ADD Takase V2.0 END
                            //タグ紐付画面へ
                            intent = new Intent(getApplication(), TaglinkRead.class);
                            startActivity(intent);
                            break;
                        case MatGeneral:  //照合(一般)仮置きなし

                            break;
                        case MatGeneralTmpo:  //照合(一般)仮置きあり
                            methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST, "", activity), activity);

                            //機能ID取得
                            functionId.setFunctionId(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHTEMP);

                            //持込照合画面へ
                            intent = new Intent(getApplication(), LoginMn.class);
                            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST);
                            startActivity(intent);
                            break;
                    }

                }
            });

            //マスタ取込受信ボタン Clickイベント
            final Activity nowActivity = this;
            findViewById(R.id.btnMnuReceiveData).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    // クリックイベント実行されていれば実行しない
                    if (mIsClickEvent) return;
                    // クリックイベントを禁止する
                    mIsClickEvent = true;
                    if (checkTimeout()) {return;}
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", activity), activity);

                    //20210918 CHG Takase V2.0 START
                    ////マスタ受信画面へ
                    //Intent intent = new Intent(getApplication(), WebReciveActivity.class);
                    //startActivityForResult(intent, FORM_REQUEST_RECEIVE);
                    SettingValue setting = new SettingValue();
                    if (setting.getTransfer(nowActivity).equals(CommonConst.TRANSFER_METHOD.USB.getString())) {
                        //ＵＳＢ接続確認画面へ
                        Intent intent = new Intent(getApplication(), UsbConfirmActivity.class);
                        intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_ALL_RECIVE_DATA);
                        startActivityForResult(intent, FORM_REQUEST_RECEIVE);
                    } else {
                        //マスタ受信画面へ
                        Intent intent = new Intent(getApplication(), WebReciveActivity.class);
                        startActivityForResult(intent, FORM_REQUEST_RECEIVE);
                    }
                    //20210918 CHG Takase V2.0 END
                }
            });

            //データ送信ボタン Clickイベント
            findViewById(R.id.btnMnuSendData).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    // クリックイベント実行されていれば実行しない
                    if (mIsClickEvent) return;
                    // クリックイベントを禁止する
                    mIsClickEvent = true;
                    if (checkTimeout()) {return;}
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", activity), activity);

                    //20210918 CHG Takase V2.0 START
                    ////棚卸データ送信画面へ
                    //Intent intent = new Intent(getApplication(), WebSendActivity.class);
                    //startActivityForResult(intent, FORM_REQUEST_SEND);
                    SettingValue setting = new SettingValue();
                    if (setting.getTransfer(nowActivity).equals(CommonConst.TRANSFER_METHOD.USB.getString())) {
                        //ＵＳＢ接続確認画面へ
                        Intent intent = new Intent(getApplication(), UsbConfirmActivity.class);
                        intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_ALL_SEND_DATA);
                        startActivityForResult(intent, FORM_REQUEST_SEND);
                    } else {
                        //棚卸データ送信画面へ
                        Intent intent = new Intent(getApplication(), WebSendActivity.class);
                        startActivityForResult(intent, FORM_REQUEST_SEND);
                    }
                    //20210918 CHG Takase V2.0 END
                }
            });

            //バージョン表示
            TextView _labVersion  = findViewById(R.id.LabMnuVersion);
            _labVersion.setText(CommonConst.VERSION_NAME + Common.getVersionCode(this));

            //マスタ受信日時表示
            ((TextView)findViewById(R.id.lblReceiveDate)).setText(SystemSetting.getString(CommonConst.SYSTEM_KEY_MASTER_RECEIVE_DATE, "",this));

            //20230221 ADD Sato v2.3 START
            //RFIDリーダのバッテリー残量を表示させない
            if (Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString()) ||
                    Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.IDATA.getString()) ||
                    Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString())){
                //キーエンス製DXシリーズ、iData、RS35の場合　20230410 UPD Sato v2.3.1
                findViewById(R.id.LabMnuSP1BatteryRemain).setVisibility(View.INVISIBLE);
                findViewById(R.id.ImgViewMnuBattery).setVisibility(View.INVISIBLE);
            }
            //20230221 ADD Sato v2.3 END

            //初期起動のフラグを設定しておく
            SystemSetting.setString(CommonConst.SYSTEM_KEY_INITIALIZE, "OK", this);

            //サービス開始
            super.startService();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (_db != null) {
                _db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);

        } finally {
            if (_db != null) {
                _db.close();
            }
        }
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
                case R.id.action_setting:
                    //設定 →　システム設定画面開く
                    ReaderOff();
                    Intent intent = new Intent(getApplication(), SystemSett.class);
                    startActivityForResult(intent, BUTTON_SETTEI);
                    return true;
                case R.id.action_connect:
                    //接続
                    if (!connectReader) {
                        //開始
                        onResume();
                    }
                    return true;

                case R.id.action_menu:
                    return true;

                case R.id.action_logout:
                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, ACTION_LOGOUT);
                    _confirmDialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGOUT, "", this));  //ログアウト

                    // 実績データがある場合
                    if (_isExistReciveData) {
                        _confirmDialog.message(SystemSetting.getString(
                                CommonConst.VIEW_WORD_UNSENT_DATA_LOGOUT, "", this)); //実績データが未送信です。ログアウトすると実績データが全て削除されますが、よろしいでしょうか？
                    } else {
                        _confirmDialog.message(SystemSetting.getString(
                                CommonConst.VIEW_WORD_LOGOUT, "", this));                //ログアウトします。よろしいですか？
                    }

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

    //キー入力イベント
    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

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

    // レジュームイベント
    @Override
    public void onResume() {
        try {
            super.onResume();

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //「貸出」(返却) 初期化  20230906 ADD okubo V2.4.1
            _ProcessData.setIsLease(false);

            //貸出(照合)初期化
            _ProcessData.setIsMatLease(false);

            //貸出(照合)初期化
            _ProcessData.setIsTemporary(false);

            SQLiteDatabase db = null;
            try {
                //DB接続
                DatabaseHelper dbHelper = new DatabaseHelper(this);
                db = dbHelper.getWritableDatabase();

                //データ有無表示
                ViewDataUmu(db);

                //メニューモード取得
                GetMenuMode();

                //文言設定
                InitializeWord();

            } finally {
                if(db != null){
                    db.close();
                }
            }
            //タイマー開始
            StartTimer();
            // クリックイベントを許可する
            mIsClickEvent = false;

            new Thread() {
                @Override
                public void run() {

                    try {
                        Thread.sleep(1800);

                        //読取開始
                        ReaderOn();

                        //RFIDリーダー電池残量表示
                        BatteryRemainDisp();

                    } catch (Exception e) {
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                    }
                }
            }.start();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     *  ポーズイベント
     */
    @Override
    public void onPause() {
        try {
            super.onPause();

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //タイマー終了
            StopTimer();
        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" + Common.toString(ex), this);
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }
    /**
     * サブ画面から戻るイベント
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        switch(requestCode) {
            case FORM_REQUEST_RECEIVE:        //マスタ受信画面から戻る
                if(resultCode == RESULT_OK) {
                    //表示データを再構築する
                    EventButtonMasterReceive( this);
                }

                break;
            case FORM_REQUEST_SEND:        //データ送信画面から戻る
                if(resultCode == RESULT_OK) {
                    SQLiteDatabase db = null;
                    try {
                        //DB接続
                        DatabaseHelper dbHelper = new DatabaseHelper(this);
                        db = dbHelper.getWritableDatabase();

                        //データ有無表示
                        ViewDataUmu(db);

                    } finally {
                        if(db != null){
                            db.close();
                        }
                    }
                }
                break;
            case BUTTON_SETTEI:
                //リーダー再起動
                ReaderOn();
            default:
                break;
        }
    }


    /**
     * マスタデータ受信イベント
     */
    private void EventButtonMasterReceive(Activity nowActivity) {
        //文言表示
        InitializeWord();

        //20200203 Add Start Sasaki
        //初回マスタ受信時は、読取をReaderにする
        String strReceiveDate = SystemSetting.getString(CommonConst.SYSTEM_KEY_MASTER_RECEIVE_DATE, "", this);
        if ("".equals(strReceiveDate)) {
            SQLiteDatabase db = null;
            try {
                //DB接続
                DatabaseHelper dbHelper = new DatabaseHelper(this);
                db = dbHelper.getWritableDatabase();

                //設定マスタの読取媒体を取得
                String strRead = SettingFactory.GetReadingMedium(db, this);

                //20230221 UPD Sato V2.3 Start
                //DENSOとZEBRAの場合のみ初期値を判断する  20230410 UPD Sato v2.3.1
                if (Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.DENSO.getString()) ||
                    Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.ZEBRA.getString())) {

                    //TAGの場合、初期値はReaderにする
                    if (CommonConst.READING_MEDIUM.TAG.getString().equals(strRead)) {
                        //設定をReaderにする
                        SettingValue value = new SettingValue();
                        value.setReaderChange(this, CommonConst.READER_SETTING.Reader.getString());
                    }
                }
                //20230221 UPD Sato V2.3 End
            } finally {
                if(db != null){
                    db.close();
                }
            }
        }
        //20200203 Add End Sasaki

        //マスタ取込受日時を保持する
        SystemSetting.setString(CommonConst.SYSTEM_KEY_MASTER_RECEIVE_DATE, Common.getNowTime("yyyy/MM/dd HH:mm"), nowActivity);

        //マスタ受信日時を再表示
        ((TextView)findViewById(R.id.lblReceiveDate)).setText(SystemSetting.getString(CommonConst.SYSTEM_KEY_MASTER_RECEIVE_DATE, "",nowActivity));

        //初期起動でない場合の値をセットする
        //※仕様変更に伴い以前の動きを再現するため
        SystemSetting.setString(CommonConst.SYSTEM_KEY_STARTUP, CommonConst.RETURN_STARTUP_VALUE.NO.getString(), nowActivity);

    }

    /**
     * 文言表示
     */
    @SuppressLint("SetTextI18n")
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TITLE,"",this));                 //メニュー
        Button btnMnu7;

        SettingValue settingValue = new SettingValue();
        ((TextView)findViewById(R.id.MenuLblLoginName))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CHECK_EMPLOYEE_WORKER, "", this)  + "：" + settingValue.getLoginEmployeeName(this));  //作業者

        switch (_MenuMode) {
            case Normal:      //通常
                //ボタン表示、非表示設定
                findViewById(R.id.btnMnu7).setVisibility(View.VISIBLE);

                //通常メニュー
                ((TextView)findViewById(R.id.btnMnu1))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", this));  //棚卸
                ((TextView)findViewById(R.id.btnMnu2))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", this));     //探索
                ((TextView)findViewById(R.id.btnMnu3))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING, "", this));    //貸出
                ((TextView)findViewById(R.id.btnMnu4))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_RETURN, "", this));     //返却
                ((TextView)findViewById(R.id.btnMnu5))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_STORE, "", this));      //保管
                ((TextView)findViewById(R.id.btnMnu6))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_DELETE, "", this));     //除却
                ((TextView)findViewById(R.id.btnMnu7))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG, "", this));        //タグ紐付

                //アイコンの位置が調整できないためコメントアウト
//                btnMnu7 = findViewById(R.id.btnMnu7);
//                if(btnMnu7.getText().length() >= 10) {
//                    btnMnu7.setPadding(0, 10, 0, 10);
//                }else{
//                    btnMnu7.setPadding(0, 20, 0, 10);
//                }

                //アイコンの設定
                ((TextView)findViewById(R.id.btnMnu1))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.inventory, 0, 0);  //棚卸
                ((TextView)findViewById(R.id.btnMnu1))
                        .setCompoundDrawablePadding(2);  //棚卸

                ((TextView)findViewById(R.id.btnMnu2))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.search, 0, 0);  //探索
                ((TextView)findViewById(R.id.btnMnu2))
                        .setCompoundDrawablePadding(0);  //探索

                ((TextView)findViewById(R.id.btnMnu3))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.rent, 0, 0);  //貸出
                ((TextView)findViewById(R.id.btnMnu3))
                        .setCompoundDrawablePadding(0);  //貸出

                ((TextView)findViewById(R.id.btnMnu4))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.returnable, 0, 0);  //返却
                ((TextView)findViewById(R.id.btnMnu4))
                        .setCompoundDrawablePadding(0);  //返却

                ((TextView)findViewById(R.id.btnMnu5))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.hokan, 0, 0);  //保管
                ((TextView)findViewById(R.id.btnMnu5))
                        .setCompoundDrawablePadding(0);  //保管

                ((TextView)findViewById(R.id.btnMnu6))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.trash, 0, 0);  //除却
                ((TextView)findViewById(R.id.btnMnu6))
                        .setCompoundDrawablePadding(22);  //除却

                ((TextView)findViewById(R.id.btnMnu7))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.tag, 0, 0);  //資産ID紐づけ
                ((TextView)findViewById(R.id.btnMnu7))
                        .setCompoundDrawablePadding(0);  //資産ID紐づけ

                break;
            case MatControl:  //照合(管理)
                //ボタン表示、非表示設定
                findViewById(R.id.btnMnu7).setVisibility(View.VISIBLE);

                //管理
                ((TextView)findViewById(R.id.btnMnu1))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", this));  //棚卸
                ((TextView)findViewById(R.id.btnMnu2))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", this));     //探索
                ((TextView)findViewById(R.id.btnMnu3))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING, "", this));    //貸出
                ((TextView)findViewById(R.id.btnMnu4))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHRETURN, "", this));     //照合返却
                ((TextView)findViewById(R.id.btnMnu5))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_STORE, "", this));      //保管
                ((TextView)findViewById(R.id.btnMnu6))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_RETURN, "", this));     //返却
                ((TextView)findViewById(R.id.btnMnu7))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG, "", this));        //タグ紐付

                //アイコンの設定
                ((TextView)findViewById(R.id.btnMnu1))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.inventory, 0, 0);  //棚卸
                ((TextView)findViewById(R.id.btnMnu1))
                        .setCompoundDrawablePadding(2);  //棚卸

                ((TextView)findViewById(R.id.btnMnu2))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.search, 0, 0);  //探索
                ((TextView)findViewById(R.id.btnMnu2))
                        .setCompoundDrawablePadding(0);  //探索

                ((TextView)findViewById(R.id.btnMnu3))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.rent, 0, 0);  //貸出
                ((TextView)findViewById(R.id.btnMnu3))
                        .setCompoundDrawablePadding(0);  //貸出

                ((TextView)findViewById(R.id.btnMnu4))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.mat_return_64, 0, 0);  //照合返却
                ((TextView)findViewById(R.id.btnMnu4))
                        .setCompoundDrawablePadding(0);  //照合返却

                ((TextView)findViewById(R.id.btnMnu5))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.hokan, 0, 0);  //保管
                ((TextView)findViewById(R.id.btnMnu5))
                        .setCompoundDrawablePadding(0);  //保管

                ((TextView)findViewById(R.id.btnMnu6))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.returnable, 0, 0);  //返却
                ((TextView)findViewById(R.id.btnMnu6))
                        .setCompoundDrawablePadding(-10);  //返却

                ((TextView)findViewById(R.id.btnMnu7))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.tag, 0, 0);  //資産ID紐づけ
                ((TextView)findViewById(R.id.btnMnu7))
                        .setCompoundDrawablePadding(0);  //資産ID紐づけ
                break;
            case MatGeneral:  //照合(一般)仮置きなし
                //ボタン表示、非表示設定
                findViewById(R.id.btnMnu7).setVisibility(View.GONE);

                ((TextView)findViewById(R.id.btnMnu1))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING, "", this));  //貸出
                ((TextView)findViewById(R.id.btnMnu2))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHRETURN, "", this));     //照合返却
                ((TextView)findViewById(R.id.btnMnu3))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHIN, "", this));    //持込照合
                ((TextView)findViewById(R.id.btnMnu4))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHOUT, "", this));     //持帰照合
                ((TextView)findViewById(R.id.btnMnu5))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_RETURN, "", this));      //返却
                ((TextView)findViewById(R.id.btnMnu6))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", this));     //探索

                //アイコンの設定
                ((TextView)findViewById(R.id.btnMnu1))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.rent, 0, 0);  //貸出
                ((TextView)findViewById(R.id.btnMnu1))
                        .setCompoundDrawablePadding(0);  //貸出

                ((TextView)findViewById(R.id.btnMnu2))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.mat_return_64, 0, 0);  //照合返却
                ((TextView)findViewById(R.id.btnMnu2))
                        .setCompoundDrawablePadding(0);  //照合返却

                ((TextView)findViewById(R.id.btnMnu3))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.matin_64, 0, 0);  //持込照合
                ((TextView)findViewById(R.id.btnMnu3))
                        .setCompoundDrawablePadding(0);  //持込照合

                ((TextView)findViewById(R.id.btnMnu4))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.matout_64, 0, 0);  //持出照合
                ((TextView)findViewById(R.id.btnMnu4))
                        .setCompoundDrawablePadding(0);  //持出照合

                ((TextView)findViewById(R.id.btnMnu5))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.returnable, 0, 0);  //返却
                ((TextView)findViewById(R.id.btnMnu5))
                        .setCompoundDrawablePadding(0);  //返却

                ((TextView)findViewById(R.id.btnMnu6))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.search, 0, 0);  //返却
                ((TextView)findViewById(R.id.btnMnu6))
                        .setCompoundDrawablePadding(22);  //探索

                break;
            case MatGeneralTmpo:  //照合(一般)仮置きあり
                //ボタン表示、非表示設定
                findViewById(R.id.btnMnu7).setVisibility(View.VISIBLE);

                ((TextView)findViewById(R.id.btnMnu1))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING, "", this));  //貸出
                ((TextView)findViewById(R.id.btnMnu2))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHRETURN, "", this));     //照合返却
                ((TextView)findViewById(R.id.btnMnu3))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHIN, "", this));    //持込照合
                ((TextView)findViewById(R.id.btnMnu4))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHOUT, "", this));     //持帰照合
                ((TextView)findViewById(R.id.btnMnu5))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_RETURN, "", this));      //返却
                ((TextView)findViewById(R.id.btnMnu6))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", this));     //探索
                ((TextView)findViewById(R.id.btnMnu7))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST, "", this));        //仮置登録

                btnMnu7 = findViewById(R.id.btnMnu7);
                if(btnMnu7.getText().length() >= 10) {
                    btnMnu7.setPadding(0, 10, 0, 10);
                }else{
                    btnMnu7.setPadding(0, 20, 0, 10);
                }

                //アイコンの設定 画像設定
                ((TextView)findViewById(R.id.btnMnu1))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.rent, 0, 0);  //貸出
                ((TextView)findViewById(R.id.btnMnu1))
                        .setCompoundDrawablePadding(0);  //貸出

                ((TextView)findViewById(R.id.btnMnu2))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.mat_return_64, 0, 0);  //棚卸
                ((TextView)findViewById(R.id.btnMnu2))
                        .setCompoundDrawablePadding(0);  //照合返却

                ((TextView)findViewById(R.id.btnMnu3))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.matin_64, 0, 0);  //持込照合
                ((TextView)findViewById(R.id.btnMnu3))
                        .setCompoundDrawablePadding(0);  //持込照合

                ((TextView)findViewById(R.id.btnMnu4))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.matout_64, 0, 0);  //持出照合
                ((TextView)findViewById(R.id.btnMnu4))
                        .setCompoundDrawablePadding(0);  //持出照合

                ((TextView)findViewById(R.id.btnMnu5))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.returnable, 0, 0);  //返却
                ((TextView)findViewById(R.id.btnMnu5))
                        .setCompoundDrawablePadding(0);  //返却

                ((TextView)findViewById(R.id.btnMnu6))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.search, 0, 0);  //返却
                ((TextView)findViewById(R.id.btnMnu6))
                        .setCompoundDrawablePadding(22);  //探索

                ((TextView)findViewById(R.id.btnMnu7))
                        .setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.temp_64, 0, 0);  //棚卸
                ((TextView)findViewById(R.id.btnMnu7))
                        .setCompoundDrawablePadding(0);  //仮置き登録


                break;
        }

            ((TextView)findViewById(R.id.btnMnuSendData))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", this));    //蓄積データ送信ボタン
            ((TextView)findViewById(R.id.btnMnuReceiveData))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", this));  //マスタ取込受信ボタン
            ((TextView)findViewById(R.id.lblReceiveDateTitle))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MASTER_RECEIVE_DATE, "", this) + ":");  //マスタ受信日時
            ((TextView)findViewById(R.id.LabMnuSP1BatteryRemain))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_RFID_BATTERY, "", this) + ":");  //電池残

            //ボタン制御
            InitializeButton();

    }

    /**
     * 初期状態でのボタン非活性を設定します
     */
    protected void InitializeButton(){
        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //マスタ受信日時が設定されていない場合、マスタ受信以外はボタン非活性
            String strReceiveDate = SystemSetting.getString(CommonConst.SYSTEM_KEY_MASTER_RECEIVE_DATE, "", this);
            if ("".equals(strReceiveDate)) {
                SetButtonEnabledAll(false);
                return;
            } else {
                SetButtonEnabledAll(true);
            }

            //マスタ受信の結果で棚卸ボタンの活性非活性を決定
            BtnInventorySwitchingEnabled(db);

            //設定マスタの読取媒体を取得
            String strRead = SettingFactory.GetReadingMedium(db, this);

            //QRの場合、非活性にする
            if (CommonConst.READING_MEDIUM.QR.getString().equals(strRead)) {
                SetButtonEnabled(false);
                //設定をAndroidにする
                //SettingValue value = new SettingValue();
                //value.setReaderChange(this, CommonConst.READER_SETTING.Android.getString());
                blnConnectOpenFlag = false;
                StopTimer();
            } else {
                //SetButtonEnabled(true);
//20200108 Add Start Sasaki
                //設定をReaderにする
                //SettingValue value = new SettingValue(); 20200123 Del Sasaki
                //value.setReaderChange(this, CommonConst.READER_SETTING.Reader.getString()); 20200123 Del Sasaki
//20200108 Add End Sasaki
                blnConnectOpenFlag = true;
                StartTimer();
            }

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /** ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * */
    protected void SetButtonEnabled(boolean prmEnabled) {

        //照合の場合
        switch (_MenuMode) {
            case Normal:      //通常
            case MatControl:  //照合(管理)
                //タグ探索
                ((Button)findViewById(R.id.btnMnu2)).setEnabled(prmEnabled);

                //タグ紐付け
                ((Button)findViewById(R.id.btnMnu7)).setEnabled(prmEnabled);

                if (prmEnabled == false) {
                    ((Button)findViewById(R.id.btnMnu2)).setBackground(getDrawable(R.color.ButtonDisable));
                    ((Button)findViewById(R.id.btnMnu2)).setTextColor(Color.parseColor("#a9a9a9"));
                    ((Button)findViewById(R.id.btnMnu7)).setBackground(getDrawable(R.color.ButtonDisable));
                    ((Button)findViewById(R.id.btnMnu7)).setTextColor(Color.parseColor("#a9a9a9"));

                } else {
                    ((Button)findViewById(R.id.btnMnu2)).setBackground(getDrawable(R.color.ButtonMenu));
                    ((Button)findViewById(R.id.btnMnu2)).setTextColor(Color.parseColor("#ffffff"));
                    ((Button)findViewById(R.id.btnMnu7)).setBackground(getDrawable(R.color.ButtonMenu));
                    ((Button)findViewById(R.id.btnMnu7)).setTextColor(Color.parseColor("#ffffff"));
                }


                break;
            case MatGeneral:  //照合(一般)仮置きなし
            case MatGeneralTmpo:  //照合(一般)仮置きあり
                //タグ探索
                ((Button)findViewById(R.id.btnMnu6)).setEnabled(prmEnabled);

                if (prmEnabled == false) {
                    ((Button)findViewById(R.id.btnMnu6)).setBackground(getDrawable(R.color.ButtonDisable));
                    ((Button)findViewById(R.id.btnMnu6)).setTextColor(Color.parseColor("#a9a9a9"));

                } else {
                    ((Button)findViewById(R.id.btnMnu6)).setBackground(getDrawable(R.color.ButtonMenu));
                    ((Button)findViewById(R.id.btnMnu6)).setTextColor(Color.parseColor("#ffffff"));
                }

                break;
        }

    }

    /** 棚卸ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * */
    protected void SetInventoryButtonEnabled(boolean prmEnabled) {
        //棚卸ボタン
        ((Button)findViewById(R.id.btnMnu1)).setEnabled(prmEnabled);

        SQLiteDatabase db = null;

        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //照合でない場合　or 管理である場合
            //照合機能の設定がYESの時
            if(SettingFactory.GetMatchingFunction(db).equals(CommonConst.MATCHING_FUNCTION.YES.getString())){
                SettingValue setting = new SettingValue();
                //照合管理の場合
                if (CommonConst.COLLATION_OPERATION.Control.getString().equals(setting.getCollationOperation(MainActivity.this))){
                    //管理
                    if (prmEnabled == false) {
                        ((Button)findViewById(R.id.btnMnu1)).setBackground(getDrawable(R.color.ButtonDisable));
                        ((Button)findViewById(R.id.btnMnu1)).setTextColor(Color.parseColor("#a9a9a9"));
                    } else {
                        ((Button)findViewById(R.id.btnMnu1)).setBackground(getDrawable(R.color.ButtonMenu));
                        ((Button)findViewById(R.id.btnMnu1)).setTextColor(Color.parseColor("#ffffff"));
                    }
                }
            } else {
                //通常メニュー
                if (prmEnabled == false) {
                    ((Button)findViewById(R.id.btnMnu1)).setBackground(getDrawable(R.color.ButtonDisable));
                    ((Button)findViewById(R.id.btnMnu1)).setTextColor(Color.parseColor("#a9a9a9"));
                } else {
                    ((Button)findViewById(R.id.btnMnu1)).setBackground(getDrawable(R.color.ButtonMenu));
                    ((Button)findViewById(R.id.btnMnu1)).setTextColor(Color.parseColor("#ffffff"));
                }
            }
        }finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /** メニューモード取得
     * */
    protected void GetMenuMode() {
        SQLiteDatabase db = null;

        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //データ有無表示
            ViewDataUmu(db);
            //画面モード取得
            //照合機能の設定がYESの時
            if(SettingFactory.GetMatchingFunction(db).equals(CommonConst.MATCHING_FUNCTION.YES.getString())){
                SettingValue setting = new SettingValue();
                //照合管理の場合
                if (CommonConst.COLLATION_OPERATION.Control.getString().equals(setting.getCollationOperation(MainActivity.this))){
                    //管理
                    _MenuMode = CommonConst.MENU_MODE.MatControl;
                }else{
                    //一般
                    //仮置きがありの場合
                    if(SettingFactory.GetTemporaryLeaveFunction(db).equals(CommonConst.TEMPORARYLEAVE_FUNCTION.YES.getString())){
                        _MenuMode = CommonConst.MENU_MODE.MatGeneralTmpo;
                    }else {
                        //仮置なし
                        _MenuMode = CommonConst.MENU_MODE.MatGeneral;
                    }
                }
            } else {
                //通常メニュー
                _MenuMode = CommonConst.MENU_MODE.Normal;
            }

        }finally {
            if(db != null){
                db.close();
            }
        }
    }


    /** 全ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * */
    protected void SetButtonEnabledAll(boolean prmEnabled) {
        //棚卸
        ((Button)findViewById(R.id.btnMnu1)).setEnabled(prmEnabled);

        //探索
        ((Button)findViewById(R.id.btnMnu2)).setEnabled(prmEnabled);

        //貸出
        ((Button)findViewById(R.id.btnMnu3)).setEnabled(prmEnabled);

        //返却
        ((Button)findViewById(R.id.btnMnu4)).setEnabled(prmEnabled);

        //保管
        ((Button)findViewById(R.id.btnMnu5)).setEnabled(prmEnabled);

        //除却
        ((Button)findViewById(R.id.btnMnu6)).setEnabled(prmEnabled);

        //タグ紐付け
        ((Button)findViewById(R.id.btnMnu7)).setEnabled(prmEnabled);

        //送信
        ((Button)findViewById(R.id.btnMnuSendData)).setEnabled(prmEnabled);

        if (prmEnabled == false) {
            ((Button)findViewById(R.id.btnMnu1)).setBackground(getDrawable(R.color.ButtonDisable));
            ((Button)findViewById(R.id.btnMnu1)).setTextColor(Color.parseColor("#a9a9a9"));
            ((Button)findViewById(R.id.btnMnu2)).setBackground(getDrawable(R.color.ButtonDisable));
            ((Button)findViewById(R.id.btnMnu2)).setTextColor(Color.parseColor("#a9a9a9"));
            ((Button)findViewById(R.id.btnMnu3)).setBackground(getDrawable(R.color.ButtonDisable));
            ((Button)findViewById(R.id.btnMnu3)).setTextColor(Color.parseColor("#a9a9a9"));
            ((Button)findViewById(R.id.btnMnu4)).setBackground(getDrawable(R.color.ButtonDisable));
            ((Button)findViewById(R.id.btnMnu4)).setTextColor(Color.parseColor("#a9a9a9"));
            ((Button)findViewById(R.id.btnMnu5)).setBackground(getDrawable(R.color.ButtonDisable));
            ((Button)findViewById(R.id.btnMnu5)).setTextColor(Color.parseColor("#a9a9a9"));
            ((Button)findViewById(R.id.btnMnu6)).setBackground(getDrawable(R.color.ButtonDisable));
            ((Button)findViewById(R.id.btnMnu6)).setTextColor(Color.parseColor("#a9a9a9"));
            ((Button)findViewById(R.id.btnMnu7)).setBackground(getDrawable(R.color.ButtonDisable));
            ((Button)findViewById(R.id.btnMnu7)).setTextColor(Color.parseColor("#a9a9a9"));
            ((Button)findViewById(R.id.btnMnuSendData)).setBackground(getDrawable(R.color.ButtonDisable));
            ((Button)findViewById(R.id.btnMnuSendData)).setTextColor(Color.parseColor("#a9a9a9"));

        } else {
            ((Button)findViewById(R.id.btnMnu1)).setBackground(getDrawable(R.color.ButtonMenu));
            ((Button)findViewById(R.id.btnMnu1)).setTextColor(Color.parseColor("#ffffff"));
            ((Button)findViewById(R.id.btnMnu2)).setBackground(getDrawable(R.color.ButtonMenu));
            ((Button)findViewById(R.id.btnMnu2)).setTextColor(Color.parseColor("#ffffff"));
            ((Button)findViewById(R.id.btnMnu3)).setBackground(getDrawable(R.color.ButtonMenu));
            ((Button)findViewById(R.id.btnMnu3)).setTextColor(Color.parseColor("#ffffff"));
            ((Button)findViewById(R.id.btnMnu4)).setBackground(getDrawable(R.color.ButtonMenu));
            ((Button)findViewById(R.id.btnMnu4)).setTextColor(Color.parseColor("#ffffff"));
            ((Button)findViewById(R.id.btnMnu5)).setBackground(getDrawable(R.color.ButtonMenu));
            ((Button)findViewById(R.id.btnMnu5)).setTextColor(Color.parseColor("#ffffff"));
            ((Button)findViewById(R.id.btnMnu6)).setBackground(getDrawable(R.color.ButtonMenu));
            ((Button)findViewById(R.id.btnMnu6)).setTextColor(Color.parseColor("#ffffff"));
            ((Button)findViewById(R.id.btnMnu7)).setBackground(getDrawable(R.color.ButtonMenu));
            ((Button)findViewById(R.id.btnMnu7)).setTextColor(Color.parseColor("#ffffff"));
            ((Button)findViewById(R.id.btnMnuSendData)).setBackground(getDrawable(R.color.ButtonUnder));
            ((Button)findViewById(R.id.btnMnuSendData)).setTextColor(Color.parseColor("#ffffff"));
        }
    }

    /**
     * メニュー変更イベント処理
     * @param menu
     * @return
     */
    @Override
    public boolean onPrepareOptionsMenu(Menu menu) {
        MenuItem menuConnect = menu.findItem(R.id.action_connect);
        if(connectReader) {
            menuConnect.setIcon(R.drawable.connect_48);
        } else {
            menuConnect.setIcon(R.drawable.connect_disable_48);
        }
        return true;
    }

    /**
     * データ有無を表示する
     * @param _db
     */
    private void ViewDataUmu(SQLiteDatabase _db) {
        InventoryFactory data = new InventoryFactory();
        LogFactory log = new LogFactory();
        if (data.HTStoreCount(_db) > 0  ||
            ProcessFactory.Count(this) > 0 ||
                ProcessFactory.MatCount(this) > 0 ||
            log.SelectErrorData(_db).count() > 0){
            _isExistReciveData = true;
            ((TextView) findViewById(R.id.labReciveFlag))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DATA_UMU, "", this));    //データ有
        } else {
            _isExistReciveData = false;
            ((TextView) findViewById(R.id.labReciveFlag)).setText("");
        }
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return "";
    }

    /**
     * 接続ON
     */
    protected void ReaderOn(){
        RFID_Setting_Start();
        ReaderOff();
    }

    /**
     * 接続OFF
     */
    protected void ReaderOff(){
        RFID_Read_Stop();
    }

    /**
     * @param db
     */
    private void BtnInventorySwitchingEnabled(SQLiteDatabase db) {
        Cursor cursor;
        boolean isDiscovered = false;
        SettingValue settingValue = new SettingValue();
        String target = settingValue.getTargetHub(this);

        //通常権限の場合
        if (!SettingFactory.GetAuthorityType(db).equals(CommonConst.AUTHORITY_TYPE.HUB.getString())) {
            SetInventoryButtonEnabled(true);
            return;
        }

        if (target.equals("")) {
            //拠点設定が定義されていない場合、活性にする
            SetInventoryButtonEnabled(true);
        } else {
            //拠点設定と一致する棚卸ロケ大区分コードが存在する場合、活性にする
            String strSql = InventoryStartFactory.Select();
            cursor = db.rawQuery(strSql, new String[]{target});

            while (cursor.moveToNext()) {
                if (cursor.getString(0).equals(target)) {
                    isDiscovered = true;
                    break;
                }
            }

            if (isDiscovered) {
                SetInventoryButtonEnabled(true);
            } else {
                SetInventoryButtonEnabled(false);
            }
        }
    }

    protected void AllResetButton(boolean blnEnabled){
        ReaderOn();
    }
    protected void OpenMainActivity(){ }
    protected void BarcodeDataReceived(String strData) { }
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) { }


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
