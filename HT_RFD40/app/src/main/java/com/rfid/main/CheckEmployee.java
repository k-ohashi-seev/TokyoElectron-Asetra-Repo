package com.rfid.main;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import android.os.Handler;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;

import com.rfid.Dispose.DisposeRead;
import com.rfid.Factory.EmployeeFactory;
import com.rfid.Factory.LogFactory;
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
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.ConstRuction;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;

import java.nio.charset.Charset;


/**
 * 作業者継続確認画面のクラス
 * 作業者継続確認後、メイン画面で選択した画面を表示します。
 *
 */
public class CheckEmployee extends BaseRFIDActivity implements CommonConfirmDialog.Callback {

    //時間切れ元画面
    private String _TimeoutScreen = CommonConst.MENU_CLICK.Inventory.toString();
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

    private final static int BUTTON_LOGOUT = 1;

    /** Callback. */
    private HttpAsyncReciveLoader.Listener listener;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.activity_check_employee);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //タイトルバー 戻るボタン非表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(false);

            progressBar = findViewById(R.id.checkEmpProgressBar);

            //引数を取得
            Intent intent = getIntent();
            _TimeoutScreen = intent.getStringExtra(CommonConst.VIEW_KEY_TIMEOUT);
            //選択したメニューを内部に保持
            SystemSetting.setString(CommonConst.VIEW_KEY_TIMEOUT, _TimeoutScreen,this);
            //文言表示
            InitializeWord();

            String empCodeCheck = C_Setting.getEmployeePrevSet(this);
            String empCode=SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD ,"", this);
            LOGIN_USER_CD="";

            if(empCodeCheck.equals("true") && !empCode.equals("")){
                ((TextView)findViewById(R.id.txtCheckEmpCd)).setText(empCode);
            }

            //選択ボタン Clickイベント
            final Activity nowActivity = this;

            //社員入力の変更イベント
            EditText _editTextEmployee = (EditText) findViewById(R.id.txtCheckEmpCd);
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

                        //社員コードがブランクの場合、ログインボタンを非活性
                        if ("".equals(((TextView) findViewById(R.id.txtCheckEmpCd)).getText().toString().trim())) {
                            //ボタンを非活性とする
                            setButtonEnabled(false);
                        } else {
                            //ボタンを活性とする
                            setButtonEnabled(true);
                        }
                    }
                }
            });

            //接続ボタン Clickイベント
            findViewById(R.id.btnCheckEmpNext).setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,
                            getLocalClassName() + "," + methodName + ":" +
                                    SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGIN, "", nowActivity),
                            nowActivity);

                    try {
                        AllResetButton(false);
                        //ログイン処理
                        NextButtonClick();
                    } catch (Exception ex) {
                        showErrorMessage("SYSTEM ERROR : LOGIN");
                        LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                                GetWord(CommonConst.VIEW_WORD_LOGIN_LOGIN) + ":" + Common.toString(ex), nowActivity);
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));

                    } finally {
                        AllResetButton(true);
                    }
                }
            });

            final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
            final Activity activity = this;
            final Context context = this;
            //ログアウトボタン Clickイベント
            findViewById(R.id.btnCheckEmpExit).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_LOGOUT);
                    dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGOUT, "", activity));  //ログアウト
                    dialog.message(SystemSetting.getString(
                            CommonConst.VIEW_WORD_LOGOUT, "", activity));                //ログアウトします。よろしいですか？
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
                        ((TextView)findViewById(R.id.txtCheckEmpCd)).setText(strEmployeeCd);
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
            EditText editText = findViewById(R.id.txtCheckEmpCd);

            // フォーカスは戻す
            editText.requestFocus();

            // テキストを直接セット
            editText.setText(strReadData);
            editText.setSelection(editText.getText().length());
        });
    }

    @Override
    public boolean dispatchKeyEvent(KeyEvent event) {
        // IMEを閉じる
        InputMethodManager imm = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
        imm.hideSoftInputFromWindow(findViewById(R.id.txtCheckEmpCd).getWindowToken(), 0);
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
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_CHECK_EMPLOYEE ,"",this));                   //タイトル
        ((TextView)findViewById(R.id.labCheckEmpCd))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_EMPLOYEE , "", this));     //社員コード
        ((TextView)findViewById(R.id.btnCheckEmpExit))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGOUT , "", this));     //ログアウト
        ((TextView)findViewById(R.id.labCheckEmpSubTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_ERR_EMPLOYEE_CODE_NONE , "", this));     //ガイドメッセージ
        ((TextView)findViewById(R.id.btnCheckEmpNext))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_CONTINE , "", this));         //継続
    }

    /**
     * 入力チェックを行います
     * @return
     */
    private boolean InputCheck(){
        boolean blnReturn = true;

        String strEmployeeCD = ((EditText)findViewById(R.id.txtCheckEmpCd)).getText().toString();

        //未入力の場合、エラー
        if ("".equals(strEmployeeCD)) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_ERR_EMPLOYEE_CODE_NONE , "", this));
            blnReturn = false;
            //桁数オーバーの場合、エラー
        } else if (strEmployeeCD.getBytes(Charset.forName(CommonConst.DEFUALT_CHAR)).length > CommonConst.VIEW_MAXLENGTH_EMPLOYEE_CD) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_INPUT_MAX_LENGTH , "", this));
            //入力項目を選択状態とする
            ((EditText)findViewById(R.id.txtCheckEmpCd)).setSelection(0, strEmployeeCD.length());
            blnReturn = false;
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
                        SystemSetting.getString(intent.getStringExtra(CommonConst.VIEW_KEY_TIMEOUT), "", this));
                ResetButton();
                return false;
            }

            //権限チェック(社員所属拠点マスタ)
            if (!CommonAuthority.CheckHtHub(db, strEmployeeCd, this)) {
                //エラーメッセージを表示
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_TARGET_HUB , "", this),
                        SystemSetting.getString(intent.getStringExtra(CommonConst.VIEW_KEY_TIMEOUT), "", this));
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
            EditText logincd = findViewById(R.id.txtCheckEmpCd);
            boolean blnMasterCheck = MasterCheck(strEmployeeCd);

            //画面に表示する
            logincd.setText(strEmployeeCd);

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

            SettingValue setting = new SettingValue();
            if (((TextView)findViewById(R.id.txtCheckEmpCd)).getText().toString().trim().equals(setting.getLoginEmployeeCode(this))) {
                //元画面に戻る
//                PageTransition(_TimeoutScreen);
                finish();
            } else {
                ((TextView)findViewById(R.id.txtCheckEmpCd)).setText("");
                ((TextView)findViewById(R.id.txtCheckEmpCd)).requestFocus();
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ERROR_EMPLOYEE_CODE, "", this));
            }

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

                //社員に値がある場合、活性化する
                if (!"".equals(((TextView) findViewById(R.id.txtCheckEmpCd)).getText().toString())) {
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
        ((TextView) findViewById(R.id.txtCheckEmpCd)).setText("");
    }

    /**
     * ボタンの非活性、活性をします
     * @param blnEnable
     */
    private void setButtonEnabled(boolean blnEnable) {
        if (blnEnable) {
            findViewById(R.id.btnCheckEmpNext).setEnabled(true);
            findViewById(R.id.btnCheckEmpNext).setBackground(getDrawable(R.drawable.execute_button));
            ((Button)findViewById(R.id.btnCheckEmpNext)).setTextColor(Color.parseColor("#ffffff"));
        } else {
            findViewById(R.id.btnCheckEmpNext).setEnabled(false);
            findViewById(R.id.btnCheckEmpNext).setBackground(getDrawable(R.drawable.execute_button_diable));
            ((Button)findViewById(R.id.btnCheckEmpNext)).setTextColor(Color.parseColor("#a9a9a9"));
        }
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(final boolean blnEnabled) {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                findViewById(R.id.btnCheckEmpExit).setEnabled(blnEnabled);
                if (blnEnabled) {
                    findViewById(R.id.btnCheckEmpExit).setEnabled(true);
                    findViewById(R.id.btnCheckEmpExit).setBackground(getDrawable(R.drawable.execute_button_exit));
                    ((Button)findViewById(R.id.btnCheckEmpExit)).setTextColor(Color.parseColor("#ffffff"));
                } else {
                    findViewById(R.id.btnCheckEmpExit).setEnabled(false);
                    findViewById(R.id.btnCheckEmpExit).setBackground(getDrawable(R.drawable.execute_button_diable));
                    ((Button)findViewById(R.id.btnCheckEmpExit)).setTextColor(Color.parseColor("#a9a9a9"));
                }
                if (blnEnabled) {
                    ResetButton();
                } else {
                    setButtonEnabled(blnEnabled);
                }
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
            case BUTTON_LOGOUT:      //終了ボタン
                //ログイン画面を開く
                Intent intent = new Intent(getApplication(), LoginMn.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                startActivity(intent);

        }

    }

    public interface EmployeeAuthCallback {
        void onResult(boolean success, String response) throws Exception;
    }

    /**
     * エラーメッセージを表示する
     * @param strErrMessage
     */
    private void ShowErrorMessage(String strErrMessage) {
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
