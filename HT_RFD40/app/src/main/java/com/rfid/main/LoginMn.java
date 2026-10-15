package com.rfid.main;

import static com.rfid.WebAPI.WebReciveActivity.GetReceiveMasterList;
import static com.rfid.WebAPI.WebReciveActivity.dbInsert;
import static com.rfid.WebAPI.WebSendActivity.InventoryComplete;
import static com.rfid.common.SystemSetting.setString;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.graphics.Rect;
import android.os.Bundle;
import android.os.Handler;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.TouchDelegate;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.appcompat.widget.Toolbar;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.rfid.Dispose.DisposeRead;
import com.rfid.Factory.EmployeeFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.ProcessFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Inventory.InventoryLocationSetting;
import com.rfid.Inventory.InventoryTagIn;
import com.rfid.Lease.LeaseRead;
import com.rfid.Lease.LeaseSetting;
import com.rfid.Return.ReturnRead;
import com.rfid.Search.SearchRead;
import com.rfid.Storage.StorageLocationSetting;
import com.rfid.Taglink.TaglinkRead;
import com.rfid.WebAPI.HttpAsyncReciveLoader;
import com.rfid.WebAPI.WebReciveActivity;
import com.rfid.common.AESUtil;
import com.rfid.common.BaseActivity;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.ConstRuction;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.EmployeeSelect;
import com.rfid.common.SystemSetting;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.Charset;
import java.util.Objects;


/**
 * ログイン画面のクラス
 * ログイン後、メイン画面で選択した画面を表示します。
 *
 */
public class LoginMn extends BaseRFIDActivity implements CommonConfirmDialog.Callback {

    //メニュー選択画面
    private String _MenuClick = CommonConst.MENU_CLICK.Inventory.toString();
    private ProgressBar progressBar;

    public static String LOGIN_USER_CD;
    private String _strServer = "";

    //社員選択画面からの戻り用フラグ
    private boolean _blnSubPageFlag = false;
    private String _MasterList = "";

    private static final int FORM_REQUEST_EMPLOYEE = 0;
    private final static int BUTTON_SETTEI = 9;
    private int _timeout = 30000;
    final Context _context = this;
    private int tapCount = 0;          // タップ回数
    private long firstTapTime = 0;     // 最初のタップ時刻
    private static final int TAP_LIMIT = 5;       // 5回タップで設定画面
    private static final long TIME_WINDOW = 1000; // 1秒以内で判定
    private Handler handler = new Handler();
    private Activity nowActivity = this;

    private final static int BUTTON_EXIT = 1;

    /** Callback. */
    private HttpAsyncReciveLoader.Listener listener;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.activity_login);
            Toolbar toolbar = findViewById(R.id.toolbarLogin);
            setSupportActionBar(toolbar);
            Objects.requireNonNull(getSupportActionBar()).setDisplayShowTitleEnabled(false);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            progressBar = findViewById(R.id.loginProgressBar);

            //引数を取得
            Intent intent = getIntent();
            _MenuClick = intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT);
            //選択したメニューを内部に保持
            SystemSetting.setString(CommonConst.VIEW_KEY_MENU_SELECT,_MenuClick,this);
            //文言表示
            InitializeWord();

            String empCodeCheck = C_Setting.getEmployeePrevSet(this);
            String empCode=SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD ,"", this);
            LOGIN_USER_CD="";

            //ログイン情報をリセット
            C_Setting.setLoginEmployeeCode(this, "");
            C_Setting.setLoginEmployeeName(this, "");
            C_Setting.setLoginHeadOffice(this, "");
            C_Setting.setLoginAuth(this, "");
            C_Setting.setLoginLocRfidFlg(this, "");

            if(empCodeCheck.equals("true") && !empCode.equals("")){
                ((TextView)findViewById(R.id.txtLoginCd)).setText(empCode);
            }

            //選択ボタン Clickイベント
            final Activity nowActivity = this;
            findViewById(R.id.btnLoginSelect).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", nowActivity), nowActivity);
                    //ボタン非活性
                    AllResetButton(false);

                    try {
                        //フラグ制御
                        _blnSubPageFlag = true;

                        //リーダーOff
                        ReaderOff();

                        //社員選択画面表示
                        Intent intent = new Intent(getApplication(), EmployeeSelect.class);
                        intent.putExtra(CommonConst.VIEW_KEY_LOGIN_TITLE,
                                SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LOC_L_KBN, "", nowActivity));      //一覧表示タイトル
                        intent.putExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD, ((EditText) findViewById(R.id.txtLoginCd)).getText().toString());
                        SettingValue settingValue = new SettingValue();                                          //20210918 ADD Takase V2.0
                        intent.putExtra(CommonConst.VIEW_KEY_HT_HUB, settingValue.getTargetHub(nowActivity));    //20210918 ADD Takase V2.0
                        startActivityForResult(intent, FORM_REQUEST_EMPLOYEE);
                    } catch (Exception e) {
                        //ボタン活性
                        AllResetButton(true);
                    }

                }
            });

            //社員入力の変更イベント
            EditText _editTextEmployee = (EditText) findViewById(R.id.txtLoginCd);
            _editTextEmployee.setOnKeyListener(new View.OnKeyListener() {
                // 多重入力防止(IMEの影響で同じキーイベントが2回発生する場合があるため)
                private boolean wasCommand = false;

                @Override
                public boolean onKey(View v, int keyCode, KeyEvent event) {
                    if (wasCommand) {
                        // 2回目のイベントを無効化
                        return true;
                    }
                    wasCommand = true;

                    // 一定時間後にリセット
                    new Handler().postDelayed(() -> wasCommand = false, 50);
                    return false;
                }
            });
            _editTextEmployee.addTextChangedListener(new TextWatcher() {
                @Override
                public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
                @Override
                public void onTextChanged(CharSequence s, int start, int before, int count) {}

                /**
                 * テキスト変更後
                 * @param s
                 */
                @Override
                public void afterTextChanged(Editable s) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, nowActivity);

                    //社員マスタ選択画面からの戻り以外の場合
                    if (!_blnSubPageFlag) {
                        //社員名をクリアする
                        ((TextView) findViewById(R.id.lblLoginName)).setText("");

                        //社員コードがブランクの場合、ログインボタンを非活性
                        if ("".equals(((TextView) findViewById(R.id.txtLoginCd)).getText().toString().trim()) || "".equals(((TextView) findViewById(R.id.txtLoginPass)).getText().toString().trim())) {
                            //ボタンを非活性とする
                            setButtonEnabled(false);
                        } else {
                            //ボタンを活性とする
                            setButtonEnabled(true);
                        }
                    }
                }
            });

            //パスワード入力の変更イベント
            EditText _editTextEmployeePass = (EditText) findViewById(R.id.txtLoginPass);
            _editTextEmployeePass.setOnKeyListener(new View.OnKeyListener() {
                // 多重入力防止(IMEの影響で同じキーイベントが2回発生する場合があるため)
                private boolean wasCommand = false;

                @Override
                public boolean onKey(View v, int keyCode, KeyEvent event) {
                    if (wasCommand) {
                        // 2回目のイベントを無効化
                        return true;
                    }
                    wasCommand = true;

                    // 一定時間後にリセット
                    new Handler().postDelayed(() -> wasCommand = false, 50);
                    return false;
                }
            });
            _editTextEmployeePass.addTextChangedListener(new TextWatcher() {
                @Override
                public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
                @Override
                public void onTextChanged(CharSequence s, int start, int before, int count) {}

                /**
                 * テキスト変更後
                 * @param s
                 */
                @Override
                public void afterTextChanged(Editable s) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, nowActivity);

                    //社員名をクリアする
                    ((TextView) findViewById(R.id.lblLoginName)).setText("");

                    //社員コードがブランクの場合、ログインボタンを非活性
                    if ("".equals(((TextView) findViewById(R.id.txtLoginCd)).getText().toString().trim()) || "".equals(((TextView) findViewById(R.id.txtLoginPass)).getText().toString().trim())) {
                        //ボタンを非活性とする
                        setButtonEnabled(false);
                    } else {
                        //ボタンを活性とする
                        setButtonEnabled(true);
                    }
                }
            });

            //社員Enterキーイベント
//            final Context context = this;
//            _editTextEmployee.setOnEditorActionListener(new TextView.OnEditorActionListener() {
//                @Override
//                public boolean onEditorAction(TextView view, int actionId, KeyEvent event) {
//                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
//                    LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, nowActivity);
//
//                    //Enterキーイベント
//                    if (actionId == EditorInfo.IME_ACTION_DONE){
//                        //マスタチェックをする
//                        EditText logincd = findViewById(R.id.txtLoginCd);
//
//                        //社員名称を取得する
//                        String strEmployeeName = EmployeeFactory.GetEmployeeName(logincd.getText().toString(), context);
//                        if (!"".equals(strEmployeeName)) {
//                            ((TextView)findViewById(R.id.lblLoginName)).setText(strEmployeeName);
//                        }
//                    }
//                    return false;
//                }
//            });

            TextView toolbarTitle = findViewById(R.id.toolbarTitle);
            toolbarTitle.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,
                            getLocalClassName() + "," + methodName + ":" +
                                    SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGIN, "", nowActivity),
                            nowActivity);

                    long now = System.currentTimeMillis();

                    if (tapCount == 0) {
                        firstTapTime = now;
                    }

                    if (now - firstTapTime <= TIME_WINDOW) {
                        tapCount++;
                    } else {
                        tapCount = 1;
                        firstTapTime = now;
                    }

                    if (tapCount >= TAP_LIMIT) {
                        handler.removeCallbacksAndMessages(null);
                        tapCount = 0;
                        firstTapTime = 0;
                        AllResetButton(false);
                        ReaderOff();
                        Intent intent = new Intent(getApplication(), SystemSett.class);
                        startActivityForResult(intent, BUTTON_SETTEI);
                        return;
                    }

                    handler.removeCallbacksAndMessages(null);
                    handler.postDelayed(new Runnable() {
                        @Override
                        public void run() {
                            tapCount = 0;
                            firstTapTime = 0;
                        }
                    }, TIME_WINDOW);
                }
            });

            //ログインボタン Clickイベント
            findViewById(R.id.btnLoginNext).setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,
                            getLocalClassName() + "," + methodName + ":" +
                                    SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGIN, "", nowActivity),
                            nowActivity);

                    try {
                        AllResetButton(false);
                        setButtonEnabled(false);
                        progressBar.setVisibility(View.VISIBLE);
                        //作業者継続タイマーリセット
                        C_Setting.setNoControlTime(LoginMn.this, "0");
                        //ログイン処理
                        NextButtonClick();
                    } catch (Exception ex) {
                        showErrorMessage("SYSTEM ERROR : LOGIN");
                        LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" + Common.toString(ex), nowActivity);
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                        AllResetButton(true);
                    }
                }
            });

            final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
            final Activity activity = this;
            final Context context = this;
            //終了ボタン Clickイベント
            findViewById(R.id.btnLoginExit).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_EXIT);
                    dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_EXIT, "", activity));  //終了
                    dialog.message(SystemSetting.getString(
                            CommonConst.VIEW_WORD_APPLICATION_EXIT, "", activity));                //アプリを終了します。よろしいですか？
                    dialog.params(bundle);
                    dialog.show();
                }
            });

            //ボタンを非活性とする
            setButtonEnabled(false);

            //サービス開始
            super.startService();

        } catch (Exception ex) {
            showErrorMessage("SYSTEM ERROR : LOGIN");
            AllResetButton(true);
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" +  Common.toString(ex), this);
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu_main, menu);

        // メニュー項目を取得
        MenuItem actionMenu = menu.findItem(R.id.action_menu);
        MenuItem actionSetting = menu.findItem(R.id.action_setting);
        MenuItem actionLogout = menu.findItem(R.id.action_logout);

        // 非表示にする
        if (actionMenu != null) {
            actionMenu.setVisible(false);
        }

        if (actionSetting != null) {
            actionSetting.setVisible(false);
        }

        if (actionLogout != null) {
            actionLogout.setVisible(false);
        }

        return true;
    }

    /**
     * レジュームイベント
     */
    @Override
    public void onResume() {
        try {
            super.onResume();

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //サーバー接続設定確認
            SettingValue C_Setting = new SettingValue();
            String ServerIP = C_Setting.getConnectServer(this);     //ＩＰアドレス
            if(ServerIP.length() <= 0
                    && !C_Setting.getTransfer(nowActivity).equals(CommonConst.TRANSFER_METHOD.USB.getString())) { //20210918 CHG Takase V2.0
                //未登録 →　システム設定画面開く
                Intent intent = new Intent(getApplication(), SystemSett.class);
                startActivity(intent);
            }

            //タイマー開始
            StartTimer();

            //画面初期化
            InitializeActivity();

            //リーダーON
            ReaderOn();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            AllResetButton(true);
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" +  Common.toString(ex), this);
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
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
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + CommonConst.VIEW_WORD_LOGIN_LOGIN, this);

            //サブ画面から表示の場合終了
            if (_blnSubPageFlag) {
                return;
            }

            //リーダーOff
            ReaderOff();

            //タイマー終了
            StopTimer();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            AllResetButton(true);
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" + Common.toString(ex), this);
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * サブ画面から戻るイベント処理をします。
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            switch(requestCode) {
                case FORM_REQUEST_EMPLOYEE:        //選択画面から戻る
                    if(resultCode == RESULT_OK) {
                        //社員コードを保持する
                        String strEmployeeCd = data.getStringExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD);
                        String strEmployeeName = data.getStringExtra(CommonConst.VIEW_KEY_EMPLOYEE_NAME);

                        // サブ画面から戻り値を取得して表示する
                        ((TextView)findViewById(R.id.txtLoginCd)).setText(strEmployeeCd);
                        ((TextView)findViewById(R.id.lblLoginName)).setText(strEmployeeName);
                    }
                    //リーダー再起動
                    ReaderOn();
                    break;
                case BUTTON_SETTEI:
                    //リーダー再起動
                    ReaderOn();
                default:
                    break;
            }

            //ボタンを制御する
            AllResetButton(true);

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            AllResetButton(true);
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" +  Common.toString(ex), this);
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
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
//                    //ボタン非活性
//                    AllResetButton(false);
//
//                    //Back ボタン メニュー画面を表示
//                    Intent intent = new Intent(getApplication(), MainActivity.class);
//                    startActivity(intent);

                    return false;
            }

            return super.onKeyDown(keyCode, event);
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * バーコード取得のイベント処理をします。
     * 親クラスから呼ばれます。
     * @param strReadData
     */
    @Override
    protected void BarcodeDataReceived(String strReadData){
        //社員コードに出力
        runOnUiThread(() -> {
            EditText editText = findViewById(R.id.txtLoginCd);

            // フォーカスは戻す
            editText.requestFocus();

            // テキストを直接セット
            editText.setText(strReadData);
            editText.setSelection(editText.getText().length());
        });
    }

    @Override
    public boolean dispatchKeyEvent(KeyEvent event) {
        if (event.getKeyCode() == KEY_CODE_TRIGGER || event.getKeyCode() == KEY_CODE_TRIGGER_SIDE) {
            // IMEを閉じる(QR読み込み時にIMEが表示されているとエラーで落ちるため)
            InputMethodManager imm = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
            imm.hideSoftInputFromWindow(findViewById(R.id.txtLoginCd).getWindowToken(), 0);

        }
        super.dispatchKeyEvent(event);
        return super.dispatchKeyEvent(event);
    }


    protected void ReaderOn(){
        // バーコードOn
        Barcode_Read_Start();
    }

    protected void ReaderOff(){
        // バーコードOFF
        Barcode_Read_Stop();
    }

    /**
     * 画面を初期化します。
     */
    private void InitializeActivity() {
        //ボタンを制御する
        ResetButton();
    }

    //メニュー選択イベント
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            int id = item.getItemId();
            switch(id){
                case android.R.id.home:     //戻るボタン
//                    //ボタン非活性
//                    AllResetButton(false);
//
//                    //Back ボタン メニュー画面を表示
//                    onMenuReturn();

                    return true;

                case R.id.action_setting:
                    //ボタン非活性
                    AllResetButton(false);

                    //読取OFF
                    ReaderOff();

                    //設定 →　システム設定画面開く
                    Intent intent = new Intent(getApplication(), SystemSett.class);
                    startActivityForResult(intent, BUTTON_SETTEI);

                    return true;
                 }

            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            return false;
        }
    }


    /**
     * 文言を表示します。
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGIN ,"",this));                   //タイトル
        ((TextView)findViewById(R.id.labLoginCd))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_EMPLOYEE , "", this));     //社員
        ((TextView)findViewById(R.id.labLoginPass))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_PASS , "", this));     //パスワード
        ((TextView)findViewById(R.id.btnLoginExit))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_EXIT , "", this));     //終了
        ((TextView)findViewById(R.id.btnLoginSelect))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_GAID, "", this));           //ガイドメッセージ
        ((TextView)findViewById(R.id.labLoginSubTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_GAID , "", this));     //コード読取、選択
        ((TextView)findViewById(R.id.btnLoginNext))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGIN , "", this));         //ログイン
    }

    /**
     * 入力チェックを行います
     * @return
     */
    private boolean InputCheck(){
        boolean blnReturn = true   ;

        String strEmployeeCD = ((EditText)findViewById(R.id.txtLoginCd)).getText().toString();
        String strEmployeePASS = ((EditText)findViewById(R.id.txtLoginPass)).getText().toString();

        //未入力の場合、エラー
        if ("".equals(strEmployeeCD)) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_ERR_EMPLOYEE_CODE_NONE , "", this));
            blnReturn = false;
            AllResetButton(true);
        //桁数オーバーの場合、エラー
        } else if (strEmployeeCD.getBytes(Charset.forName(CommonConst.DEFUALT_CHAR)).length > CommonConst.VIEW_MAXLENGTH_EMPLOYEE_CD) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_INPUT_MAX_LENGTH , "", this));
            //入力項目を選択状態とする
            ((EditText)findViewById(R.id.txtLoginCd)).setSelection(0, strEmployeeCD.length());
            blnReturn = false;
            AllResetButton(true);
        } else if ("".equals(strEmployeePASS)) {
            //パスワード未入力の場合、エラー
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_INPUT_PASSWORD , "", this));
            blnReturn = false;
            AllResetButton(true);
        }
        return blnReturn;
    }

    /**
     * 社員マスタチェックを行います
     * @param strEmployeeCd
     * @return
     */
    private boolean MasterCheck(String strEmployeeCd){
        boolean blnReturn = true   ;

        String strEmployeeName = EmployeeFactory.GetEmployeeName(strEmployeeCd, this);

        //社員存在チェック
        if ("".equals(strEmployeeName)) {
            //エラーメッセージを表示
            ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_EMPLOYEE_NONE , "", this), SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_TITLE, "", this));

            //ログインボタンを非活性とする
            ResetButton();

            blnReturn = false;

        } else {
            //社員名称を表示する
            ((TextView) findViewById(R.id.lblLoginName)).setText(strEmployeeName);

            blnReturn = true;
        }

        return blnReturn;
    }

    //20210918 ADD Takase V2.0 START
    /**
     * 権限チェック
     * @param strEmployeeCd
     * @return
     */
    private boolean AuthCheck(String strEmployeeCd){
        Intent intent = getIntent();
        SQLiteDatabase db = null;

        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //権限チェック(権限マスタ)
            CommonAuthority.FunctionId functionId = new CommonAuthority.FunctionId();

            //探索の場合は権限チェックしない
            if (functionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_SEARCH.getString())) {
                return true;
            }

            if (!CommonAuthority.CheckAuth(db, strEmployeeCd, functionId.getFunctionId())) {
                //エラーメッセージを表示
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_NO_AUTHORITY , "", this),
                        SystemSetting.getString(intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT), "", this));
                ResetButton();
                return false;
            }

            //権限チェック(社員所属拠点マスタ)
            if (!CommonAuthority.CheckHtHub(db, strEmployeeCd, this)) {
                //エラーメッセージを表示
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_TARGET_HUB , "", this),
                        SystemSetting.getString(intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT), "", this));
                ResetButton();
                return false;
            }

            return true;

        } catch (Exception ex){
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" +  Common.toString(ex), this);
            return false;
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }
    //20210918 ADD Takase V2.0 END

    /**
     * BC/QRで読取したコードを設定する
     * @param strEmployeeCd 読取データ
     */
    private void ViewLocationData(final String strEmployeeCd) {

        SQLiteDatabase db = null;

        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //社員存在チェック
            EditText logincd = findViewById(R.id.txtLoginCd);
            boolean blnMasterCheck = MasterCheck(strEmployeeCd);

            //画面に表示する
            logincd.setText(strEmployeeCd);
            ((TextView) findViewById(R.id.lblLoginName)).setText(EmployeeFactory.GetEmployeeName(strEmployeeCd, this));

            //社員存在チェックエラーの場合
            if (!blnMasterCheck) {
                //入力項目を選択状態とする
                logincd.setSelection(0, logincd.length());
                return;
            }

            //20210918 ADD Takase V2.0 START
            if (!AuthCheck(strEmployeeCd)) {
                logincd.requestFocus();
                logincd.setSelection(0, logincd.length());
                return;
            }
            //20210918 ADD Takase V2.0 END

            //ボタンを活性とする
            ResetButton();

        } catch (Exception ex){
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" +  Common.toString(ex), this);
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * ログインボタンクリックイベント処理
     */
    private void NextButtonClick() throws ClassNotFoundException{
        SQLiteDatabase db = null;

        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //入力チェックをする
            if (!InputCheck()) {
                return;
            }

            String[] args = {((TextView)findViewById(R.id.txtLoginCd)).getText().toString().trim(),
                    ((TextView)findViewById(R.id.txtLoginPass)).getText().toString().trim()};
            EmployeeAuth(args, (success, response) -> {
                if (success) {

                    //設定値を取得する
                    SettingValue setting = new SettingValue();
                    _strServer = setting.getConnectServer(this);
                    final CommonOKDialog.Builder  errdialog = new CommonOKDialog.Builder(this, true);
                    if (_strServer == null || _strServer == "") {
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
                                    HttpAsyncReciveLoader ReciveAsset = new HttpAsyncReciveLoader(nowActivity, null, _timeout);
                                    ReciveAsset.setListener(timeListener());
                                    ReciveAsset.execute("http://" + _strServer + "/" + CommonConst.FIXED_VALUE_WEBAPI_DIRECTORY + "/api/HtGetDateTimeCmd");
                                }
                            });
                        }
                    }.start();

                } else {
                    ShowErrorMessage(response);
                    AllResetButton(true);
                }
            });

            //20210918 CHG Takase V2.0 START
            ////社員コードを内部に保持する
            //SystemSetting.setString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD , ((TextView)findViewById(R.id.txtLoginCd)).getText().toString().trim(), this);
            String employeeCd = ((TextView)findViewById(R.id.txtLoginCd)).getText().toString().trim();
            LOGIN_USER_CD = employeeCd;

            //社員コードを内部に保持する
            setString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD , employeeCd, this);


            //拠点リストを作成する
//            CommonAuthority.Auth.updHubList
//                    = CommonAuthority.GetUpdHubList
//                    (db, SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", this), this);
//            CommonAuthority.Auth.readHubList
//                    = CommonAuthority.GetReadHubList
//                    (db, SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", this), this);
            //20210918 CHG Takase V2.0 END

        } catch (Exception ex){
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" +  Common.toString(ex), this);
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * テキスト、ボタンを制御します。
     */
    private void ResetButton() {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {

                findViewById(R.id.btnLoginSelect).setEnabled(true);

                //社員に値がある場合、活性化する
                if (!"".equals(((TextView) findViewById(R.id.txtLoginCd)).getText().toString())) {
                    //ボタンを活性とする
                    setButtonEnabled(true);
                } else {
                    //ボタンを非活性とする
                    setButtonEnabled(false);
                }
            }
        });
    }

    /**
     * テキストボックスをクリアします。
     * @param intKbn 選択ボタン種類
     */
    private void ClearTextData(int intKbn) {
        //社員コードをクリア
        ((TextView) findViewById(R.id.txtLoginCd)).setText("");
    }

    /**
     * ボタンの非活性、活性をします
     * @param blnEnable
     */
    private void setButtonEnabled(boolean blnEnable) {
        if (blnEnable) {
            findViewById(R.id.btnLoginNext).setEnabled(true);
            findViewById(R.id.btnLoginNext).setBackground(getDrawable(R.drawable.execute_button));
            ((Button)findViewById(R.id.btnLoginNext)).setTextColor(Color.parseColor("#ffffff"));
        } else {
            findViewById(R.id.btnLoginNext).setEnabled(false);
            findViewById(R.id.btnLoginNext).setBackground(getDrawable(R.drawable.execute_button_diable));
            ((Button)findViewById(R.id.btnLoginNext)).setTextColor(Color.parseColor("#a9a9a9"));
        }
    }

    /**
     * ログインボタン以外の全てのボタンと入力欄を活性/非活性にする
     */
    @Override
    protected void AllResetButton(final boolean blnEnabled) {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                findViewById(R.id.btnLoginSelect).setEnabled(blnEnabled);
                findViewById(R.id.btnLoginExit).setEnabled(blnEnabled);
                findViewById(R.id.txtLoginCd).setEnabled(blnEnabled);
                findViewById(R.id.txtLoginPass).setEnabled(blnEnabled);
                if (blnEnabled) {
                    findViewById(R.id.btnLoginExit).setEnabled(true);
                    findViewById(R.id.btnLoginExit).setBackground(getDrawable(R.drawable.execute_button_exit));
                    ((Button)findViewById(R.id.btnLoginExit)).setTextColor(Color.parseColor("#ffffff"));
                } else {
                    findViewById(R.id.btnLoginExit).setEnabled(false);
                    findViewById(R.id.btnLoginExit).setBackground(getDrawable(R.drawable.execute_button_diable));
                    ((Button)findViewById(R.id.btnLoginExit)).setTextColor(Color.parseColor("#a9a9a9"));
                }
//                if (blnEnabled) {
//                    ResetButton();
//                } else {
//                    setButtonEnabled(blnEnabled);
//                }
            }
        });
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
            case BUTTON_EXIT:      //終了ボタン
                //アプリ終了
                allActivityFinish();

        }

    }

    public interface EmployeeAuthCallback {
        void onResult(boolean success, String response) throws Exception;
    }

    /**
     * 社員認証を行います(認証後に社員コード、社員名、権限、拠点RFIDフラグの値を保存します)
     * @param args
     * @throws Exception
     */
    public void EmployeeAuth(String[] args, EmployeeAuthCallback callback) {

        new Thread(() -> {
            boolean success = false;
            String responseStr = null;

            try {
                // URL
                SettingValue setting = new SettingValue();
                _strServer = setting.getConnectServer(this);
                if (_strServer == null || _strServer.equals("")) {
                    runOnUiThread(() -> {
                        ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_SETTING, "", nowActivity));
                        try {
                            callback.onResult(false, SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_CONNECT, "", nowActivity));
                        } catch (Exception e) {
                            //エラーログ
                            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", nowActivity) + ":" + Common.toString(e), nowActivity);
                            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                        }
                    });
                    return;
                }

                // タイムアウト
                int timeout = Integer.parseInt(setting.getConnectTimeOut(this)) * 1000;

                String urlStr = "http://" + _strServer  + "/" + CommonConst.FIXED_VALUE_WEBAPI_DIRECTORY + "/api/HtTHEmployeeAuthCmd";
                String employeeCd = args[0];
                String password = args[1];
                String encryptedPass = AESUtil.encrypt(password, CommonConst.FIXED_VALUE_AES_KEY);

                String json = "{\"employeeCd\":\"" + employeeCd + "\",\"password\":\"" + encryptedPass + "\"}";

                URL url = new URL(urlStr);
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                conn.setRequestMethod("POST");
                conn.setRequestProperty("Content-Type", "application/json");
                conn.setConnectTimeout(timeout);
                conn.setReadTimeout(timeout);
                conn.setDoOutput(true);

                try (OutputStream os = conn.getOutputStream()) {
                    os.write(json.getBytes("UTF-8"));
                }

                int responseCode = conn.getResponseCode();
                System.out.println("ステータスコード: " + responseCode);

                StringBuilder response = new StringBuilder();
                try (BufferedReader br = new BufferedReader(new InputStreamReader(conn.getInputStream(), "UTF-8"))) {
                    String line;
                    while ((line = br.readLine()) != null) {
                        response.append(line);
                    }
                }

                // ここで通信成功判定（必要に応じてJSON解析）
                if (responseCode == 200) {
                    success = true;

                    // JSON をパース
                    ObjectMapper mapper = new ObjectMapper();
                    JsonNode root = mapper.readTree(String.valueOf(response));

                    if ("0".equals(root.get("returnCode").asText()))
                    {
                        // 値をセット
                        C_Setting.setLoginEmployeeCode(this, employeeCd);
                        C_Setting.setLoginEmployeeName(this, root.get("employeeName").asText());
                        C_Setting.setLoginHeadOffice(this, root.get("headOfficeFlg").asText());
                        C_Setting.setLoginAuth(this, root.get("employeeAuth").asText());
                        C_Setting.setLoginLocRfidFlg(this, root.get("locRFIDFlg").asText());

                        String rfidReaderUseSetting = C_Setting.getRfidReaderuseSetting(this);
                        if ("1".equals(rfidReaderUseSetting) && "Y".equals(root.get("locRFIDFlg").asText()))
                        {
                            C_Setting.setRfidReaderUse(this, "1");
                        }
                        else if ("1".equals(rfidReaderUseSetting) && "N".equals(root.get("locRFIDFlg").asText()))
                        {
                            C_Setting.setRfidReaderUse(this, "2");
                        } else if ("2".equals(rfidReaderUseSetting))
                        {
                            C_Setting.setRfidReaderUse(this, "2");
                        }
                    }
                    else
                    {
                        success = false;
                        responseStr = root.get("message").asText();
                    }
                }
                else{
                    success = false;
                    responseStr = SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_CONNECT, "", nowActivity);
                }

            } catch (Exception e) {
                e.printStackTrace();
                responseStr = SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_CONNECT, "", nowActivity);
                //エラーログ
                LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                        SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", nowActivity) + ":" + Common.toString(e), nowActivity);
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
            }

            final boolean result = success;
            final String responseText = responseStr;

            // UIスレッドでコールバック呼び出し
            runOnUiThread(() -> {
                try {
                    callback.onResult(result, responseText);
                } catch (Exception e) {
                    //エラーログ
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", nowActivity) + ":" + Common.toString(e), nowActivity);
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
                }
            });

        }).start();
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
                            HttpAsyncReciveLoader ReciveAsset = new HttpAsyncReciveLoader(nowActivity, _MasterList, _timeout);
                            ReciveAsset.setListener(createListener());
                            ReciveAsset.execute("http://" + _strServer + "/" + CommonConst.FIXED_VALUE_WEBAPI_DIRECTORY + "/api/HtGetMasterDataCmd");
                        }
                    });
                    //UPD 20230221 Sato v2.3 END
                }catch (Exception ex){
                    AllResetButton(true);
                    //エラーログ
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", nowActivity) + ":" + Common.toString(ex), nowActivity);
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                }
            }

            @Override
            public void onError(String strErrMessage) {
                AllResetButton(true);
                ShowErrorMessage(strErrMessage);
            }

            /**
             * エラー処理
             * @param strErrMessage
             */
            @Override
            public void onConnectError(String strErrMessage) {
                AllResetButton(true);
                runOnUiThread(() -> progressBar.setVisibility(View.INVISIBLE));
                ShowErrorMessage(strErrMessage);
            }
        };
    }

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

                    int result = dbInsert(_context, nowActivity, json);

                    //処理結果が5(資産情報未送信)の場合
                    if (result == WebReciveActivity.RETUEN_CODE.AssetNoSend.getInt()) {
                        ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_ASSET_NO_SEND, "", nowActivity));
                    }
                    setResult(RESULT_OK, new Intent());
                    //20230206 UPD Kawamoto V2.3 END

                    SystemSetting.setString(CommonConst.SYSTEM_KEY_MASTER_RECEIVE_DATE, Common.getNowTime("yyyy/MM/dd HH:mm"),nowActivity);

                    //蓄積データをクリア
                    InventoryComplete(_context);
                    ProcessFactory.AllClear();
                    //DB接続
                    DatabaseHelper dbHelper = new DatabaseHelper(_context);
                    db = dbHelper.getWritableDatabase();
                    db.beginTransaction();

                    //エラーログを削除
                    LogFactory log = new LogFactory();
                    log.ClearErrorLog(db);

                    //コミット
                    db.setTransactionSuccessful();

                    //メニュー画面を表示
                    PageTransition(_MenuClick);
                    finish();

                } catch (Exception ex){
                    //エラーログ
                    if (db != null) {
                        db.endTransaction();
                        db.close();
                        db = null;
                    }
                    AllResetButton(true);
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", nowActivity) + ":" + Common.toString(ex), nowActivity);
                    //エラーメッセージを表示
                    ShowErrorMessage(ex.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                } finally {
                    if (db != null) {
                        db.endTransaction();
                    }
                    runOnUiThread(() -> progressBar.setVisibility(View.INVISIBLE));
                }
            }

            /**
             * エラー処理
             * @param strErrMessage
             */
            @Override
            public void onError(String strErrMessage) {
                AllResetButton(true);
                runOnUiThread(() -> progressBar.setVisibility(View.INVISIBLE));
                ShowErrorMessage(strErrMessage);
            }

            /**
             * エラー処理
             * @param strErrMessage
             */
            @Override
            public void onConnectError(String strErrMessage) {
                AllResetButton(true);
                ShowErrorMessage(strErrMessage);
            }
        };
    }

    /**
     * リスナーを設定
     * @param listener
     */
    void setListener(HttpAsyncReciveLoader.Listener listener) {
        this.listener = listener;
    }

    /**
     * エラーメッセージを表示する
     * @param strErrMessage
     */
    private void ShowErrorMessage(String strErrMessage) {
        runOnUiThread(() -> progressBar.setVisibility(View.INVISIBLE));
        ((TextView)findViewById(R.id.btnLoginExit)).setEnabled(true);
        CommonOKDialog.Builder  errdialog = new CommonOKDialog.Builder(this, true);
        errdialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGIN, "", nowActivity));
        errdialog.message(strErrMessage);
        errdialog.params(new Bundle());
        errdialog.show();
    }

    protected String GetSettingKey(){return "";}
    protected void OpenMainActivity(){ }
    protected void RFIDDataReceived(String strReadData, short tempRSSI, final String strPcHex) { }
}
