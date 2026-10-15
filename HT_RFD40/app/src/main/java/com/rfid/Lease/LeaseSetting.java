package com.rfid.Lease;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextUtils;
import android.text.TextWatcher;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MenuItem;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import com.rfid.Factory.EmployeeFactory;
import com.rfid.Factory.LeaseFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonCalender;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.EmployeeSelect;
import com.rfid.common.SystemSetting;
import com.rfid.main.MainActivity;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;

import java.nio.charset.Charset;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;

/**
 * 貸出先選択画面のクラス
 * 貸出先を選択します。
 *
 */
public class LeaseSetting extends BaseRFIDActivity implements CommonCalender.Callback{

    //DB接続
    private SQLiteDatabase _db;

    //選択画面からの戻り用フラグ
    private boolean _blnSubPageFlag = false;

    //貸出先
    private String _LeaseCode = "";
    private String _LeaseName = "";

    //貸出担当
    private String _EmployeeCode = "";
    private String _EmployeeName = "";

    //返却予定日
    private String _ReturnDate = "";

    //BC読取場所制御
    private TextPosition _BCReadPosition = TextPosition.LeaseDestination;

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }

    private enum TextPosition {
        LeaseDestination,
        LeaseResponsible,
        none;
    }

    //選択ボタン用
    private static final int FORM_REQUEST_DESTINATION = 1;  //貸出先
    private static final int FORM_REQUEST_RESPONSIBLE = 2;  //貸出担当
    private static final int FORM_REQUEST_RETURN = 3;        //返却予定日
    private static final int FORM_REQUEST_READ = 4;          //読取画面
    private final static int BUTTON_SETTEI = 9;

    private final Activity activity = this;

    /**
     * クリエイトイベント
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_lease_setting);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //文言表示
            InitializeWord();

            //ボタンの非活性設定
            InitializeButtonEnable();

            //返却予定日を自動セット
            setAutoReturnDueDate();

            //貸出先選択ボタン Clickイベント
            final Activity nowActivity = this;
            findViewById(R.id.btnLeaseSettingDestination).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", activity), activity);

                    //ボタン非活性
                    AllResetButton(false);

                    try {
                        //フラグ制御
                        _blnSubPageFlag = true;

                        //ReaderOff
                        ReaderOff();

                        //貸出先指定画面表示
                        Intent intent = new Intent(getApplication(), LeaseDestinationSelect.class);
                        intent.putExtra(CommonConst.VIEW_KEY_LEASE_CD, ((EditText) findViewById(R.id.txtLeaseSettingDestination)).getText().toString());
                        startActivityForResult(intent, FORM_REQUEST_DESTINATION);
                    } catch (Exception e) {
                        //ボタンを制御する
                        ResetButton();
                    }

                }
            });

            //貸出担当選択ボタン Clickイベント
            findViewById(R.id.btnLeaseSettingResponsible).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", activity), activity);

                    //ボタン非活性
                    AllResetButton(false);

                    try {
                        //フラグ制御
                        _blnSubPageFlag = true;

                        //ReaderOff
                        ReaderOff();

                        //社員指定画面表示
                        Intent intent = new Intent(getApplication(), EmployeeSelect.class);
                        intent.putExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD, ((EditText) findViewById(R.id.txtLeaseSettingResponsible)).getText().toString());

                        //20210918 ADD Takase V2.0 START
                        SettingValue settingValue = new SettingValue();
                        intent.putExtra(CommonConst.VIEW_KEY_HT_HUB, settingValue.getTargetHub(nowActivity));
                        //20210918 ADD Takase V2.0 END 
                        startActivityForResult(intent, FORM_REQUEST_RESPONSIBLE);
                    } catch (Exception e) {
                        //ボタンを制御する
                        ResetButton();
                    }

                }
            });

            //カレンダー選択ボタン Clickイベント
            final CommonCalender.Builder calender = new CommonCalender.Builder(this);
            findViewById(R.id.btnLeaseSettingReturn).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", activity), activity);

                    //カレンダー表示
                    calender.show();
                }
            });

            /**
             * 貸出先テキスト変更イベント
             */
            EditText _editText = findViewById(R.id.txtLeaseSettingDestination);
            _editText.addTextChangedListener(new TextWatcher() {
                @Override
                public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
                @Override
                public void onTextChanged(CharSequence s, int start, int before, int count) {
                }
                @Override
                public void afterTextChanged(Editable s) {
                    //貸出先選択画面からの戻り以外の場合
                    if (!_blnSubPageFlag) {
                        //貸出先を再設定する
                        _LeaseCode = ((EditText) findViewById(R.id.txtLeaseSettingDestination)).getText().toString();

                        //貸出先名をクリアする
                        ((TextView) findViewById(R.id.lblLeaseSettingDestinationName)).setText("");
                        _LeaseName = "";

                        //ボタンを非活性/活性にする
                        ResetButton();
                    }
                }
            });

            //貸出先Enterキーイベント
            _editText.setOnEditorActionListener(new TextView.OnEditorActionListener() {
                @Override
                public boolean onEditorAction(TextView view, int actionId, KeyEvent event) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, activity);

                    //Enterキーイベント
                    if (actionId == EditorInfo.IME_ACTION_DONE){
                        //マスタチェックをする
                        String strDestination = ((EditText) findViewById(R.id.txtLeaseSettingDestination)).getText().toString();

                        //名称を取得する
                        String strDestinationName = LeaseFactory.GetLeaseDestinationName(nowActivity, strDestination);
                        if (!"".equals(strDestinationName)) {
                            _LeaseName = strDestinationName;
                            ((TextView)findViewById(R.id.lblLeaseSettingDestinationName)).setText(Common.substring(strDestinationName, CommonConst.VIEW_LEASE_SETTING_NAME_SIZE));
                        }
                    }
                    return false;
                }
            });
/* 20191213 Del Sasaki クリックイベントの場合、拾えない場合があるため
            //貸出先クリックイベント
            _editText.setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    //選択ポジションを保持する(BC/QR用)
                    _BCReadPosition = TextPosition.LeaseDestination;
                }
            });
*/
//20191213 Add Start Sasaki
            _editText.setOnFocusChangeListener(new View.OnFocusChangeListener() {
                @Override
                public void onFocusChange(View v, boolean hasFocus) {
                    //フォーカスを受け取った時
                    if(hasFocus){
                        //選択ポジションを保持する(BC/QR用)
                        _BCReadPosition = TextPosition.LeaseDestination;
                    }
                }
            });
//20191213 Add End Sasaki

            /**
             * 備考テキスト変更イベント
             */
            EditText _editTextBiko = findViewById(R.id.editBiko);
            _editTextBiko.addTextChangedListener(new TextWatcher() {
                @Override
                public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
                @Override
                public void onTextChanged(CharSequence s, int start, int before, int count) {
                }
                @Override
                public void afterTextChanged(Editable s) {
                    //ボタンを非活性/活性にする
                    ResetButton();
                }
            });

            //貸出先担当変更イベント
            EditText _editTextEmployee = findViewById(R.id.txtLeaseSettingResponsible);
            _editTextEmployee.addTextChangedListener(new TextWatcher() {
                @Override
                public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
                @Override
                public void onTextChanged(CharSequence s, int start, int before, int count) {}
                @Override
                public void afterTextChanged(Editable s) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, activity);

                    //貸出先担当選択画面からの戻り以外の場合
                    if (!_blnSubPageFlag) {
                        //社員コードを再設定する
                        _EmployeeCode = ((EditText) findViewById(R.id.txtLeaseSettingResponsible)).getText().toString();

                        //社員名をクリアする
                        ((TextView) findViewById(R.id.labLeaseSettingResponsibleName)).setText("");
                        _EmployeeName = "";

                        //ボタンを非活性/活性にする
                        ResetButton();
                    }
                }
            });

            //貸出先担当Enterキーイベント
            final Context context = this;
            _editTextEmployee.setOnEditorActionListener(new TextView.OnEditorActionListener() {
                @Override
                public boolean onEditorAction(TextView view, int actionId, KeyEvent event) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, activity);

                    //Enterキーイベント
                    if (actionId == EditorInfo.IME_ACTION_DONE){
                        //マスタチェックをする
                        EditText Employeecd = findViewById(R.id.txtLeaseSettingResponsible);

                        //社員名称を取得する
                        String strEmployeeName = EmployeeFactory.GetEmployeeName(Employeecd.getText().toString(), context);
                        if (!"".equals(strEmployeeName)) {
                            _EmployeeName = strEmployeeName;
                            ((TextView)findViewById(R.id.labLeaseSettingResponsibleName)).setText(Common.substring(strEmployeeName, CommonConst.VIEW_LEASE_SETTING_NAME_SIZE));
                        }
                    }
                    return false;
                }
            });
/* 20191213 Del Sasaki クリックイベントの場合、拾えない場合があるため
            //貸出先担当クリックイベント
            _editTextEmployee.setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    //選択ポジションを保持する(BC/QR用)
                    _BCReadPosition = TextPosition.LeaseResponsible;
                }
            });
*/
//20191213 Add Start Sasaki
            _editTextEmployee.setOnFocusChangeListener(new View.OnFocusChangeListener() {
                @Override
                public void onFocusChange(View v, boolean hasFocus) {
                    //フォーカスを受け取った時
                    if(hasFocus){
                        //選択ポジションを保持する(BC/QR用)
                        _BCReadPosition = TextPosition.LeaseResponsible;
                    }
                }
            });
//20191213 Add End Sasaki

            //次へボタン Clickイベント
            findViewById(R.id.btnLeaseSettingNext).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_NEXT, "", activity), activity);

                    //ボタン非活性
                    AllResetButton(false);

                    SQLiteDatabase db = null;

                    try {
                        //データベースヘルパーのインスタンスを作成する
                        DatabaseHelper dbHelper = new DatabaseHelper(view.getContext());
                        db = dbHelper.getWritableDatabase();

                        //次へボタン処理
                        NextButtonClick(db);

                    } catch (Exception ex) {
                        showErrorMessage(ex.getMessage());
                        ex.printStackTrace();

                        //ボタンを制御する
                        ResetButton();
                    } finally {
                        if (db != null) {
                            db.close();
                        }
                        //ボタンを制御する
                        AllResetButton(true);         //20191122 Add Sasaki
                    }
                    }

            });

            //クリアボタン Clickイベント
            findViewById(R.id.btnClear).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", activity), activity);

                    //ボタン非活性
                    AllResetButton(false);

                    try {
                        //各項目クリア
                        DataClear();
                    //} catch (Exception e) {   //20191016 Del Sasaki
                    //    ResetButton();        //20191016 Del Sasaki
                    } finally {                //20191016 Add Sasaki
                        //ボタン活性
                        AllResetButton(true);
                    }
                }
            });

            //サービス開始
            super.startService();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
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

            //サブ画面からの戻りの場合終了
            if (_blnSubPageFlag) {
                _blnSubPageFlag = false;
                return;
            }

            //画面初期化
            InitializeActivity();

            //タイマー開始
            StartTimer();

            //リーダーON
            ReaderOn();

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

            //サブ画面から表示の場合終了
            if (_blnSubPageFlag) {
                return;
            }

            //ReaderOff
            ReaderOff();

            //タイマー終了
            StopTimer();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
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
                case FORM_REQUEST_DESTINATION:        //貸出先選択画面から戻る
                    if(resultCode == RESULT_OK) {
                        //貸出先を保持する
                        _LeaseCode = data.getStringExtra(CommonConst.VIEW_KEY_LEASE_CD);
                        _LeaseName = data.getStringExtra(CommonConst.VIEW_KEY_LEASE_NAME);

                        //BC読取に貸出担当を設定
                        _BCReadPosition = TextPosition.LeaseResponsible;

                        // サブ画面から戻り値を取得して表示する
                        TextView text = findViewById(R.id.txtLeaseSettingDestination);
                        text.setText(_LeaseCode);
                        TextView textName = findViewById(R.id.lblLeaseSettingDestinationName);
                        textName.setText(Common.substring(_LeaseName,CommonConst.VIEW_LEASE_SETTING_NAME_SIZE));
                    }
                    //ReaderOn
                    ReaderOn();
                    break;

                case FORM_REQUEST_RESPONSIBLE:        //貸出担当選択画面から戻る
                    if(resultCode == RESULT_OK) {
                        //貸出担当を保持する
                        _EmployeeCode = data.getStringExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD);
                        _EmployeeName = data.getStringExtra(CommonConst.VIEW_KEY_EMPLOYEE_NAME);

                        //BC読取に貸出先を設定
                        _BCReadPosition = TextPosition.LeaseDestination;

                        // サブ画面から戻り値を取得して表示する
                        TextView textM = findViewById(R.id.txtLeaseSettingResponsible);
                        textM.setText(_EmployeeCode);
                        TextView textMName = findViewById(R.id.labLeaseSettingResponsibleName);
                        textMName.setText(Common.substring(_EmployeeName,CommonConst.VIEW_LEASE_SETTING_NAME_SIZE));
                    }
                    //ReaderOn
                    ReaderOn();
                    break;

                case FORM_REQUEST_RETURN:            //カレンダー選択画面から戻る
                    if(resultCode == RESULT_OK) {
                        //返却予定日を保持する
                        _ReturnDate = data.getStringExtra(CommonConst.VIEW_KEY_DATE);

                        // サブ画面から戻り値を取得して表示する
                        TextView textS = findViewById(R.id.txtLeaseSettingReturn);
                        textS.setText(_ReturnDate);
                    }
                    break;

                case BUTTON_SETTEI:                 //設定画面から戻る
                    //リーダー再起動
                    ReaderOn();
                    break;

                case FORM_REQUEST_READ:             //読取画面から戻る
                    //貸出先コードを設定
                    _LeaseCode =  data.getStringExtra(CommonConst.VIEW_KEY_LEASE_CD);
                    ((TextView)findViewById(R.id.txtLeaseSettingDestination)).setText(_LeaseCode);

                    //貸出先名を設定
                    _LeaseName  =  data.getStringExtra(CommonConst.VIEW_KEY_LEASE_NAME);
                    ((TextView)findViewById(R.id.lblLeaseSettingDestinationName)).setText(_LeaseName);

                    //社員コードを設定
                    _EmployeeCode  =  data.getStringExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD);
                    ((TextView)findViewById(R.id.txtLeaseSettingResponsible)).setText(_EmployeeCode);

                    //社員名を設定
                    _EmployeeName  =  data.getStringExtra(CommonConst.VIEW_KEY_EMPLOYEE_NAME);
                    ((TextView)findViewById(R.id.labLeaseSettingResponsibleName)).setText(_EmployeeName);

                    //返却予定日を設定
                    _ReturnDate  =  data.getStringExtra(CommonConst.VIEW_KEY_DATE);
                    ((TextView)findViewById(R.id.txtLeaseSettingReturn)).setText(_ReturnDate);

                    //備考を設定
                    ((EditText)findViewById(R.id.editBiko)).setText(data.getStringExtra(CommonConst.VIEW_KEY_LEASE_REMARK));
                    break;

                default:
                    break;
            }

            //ボタンを制御する
            AllResetButton(true);
            ResetButton();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
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
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            switch (keyCode)  {
                case KeyEvent.KEYCODE_BACK:  //Back ボタン
                    //ボタン非活性
                    AllResetButton(false);

                    // メニュー画面を表示
                    Intent intent = new Intent(getApplication(), MainActivity.class);
                    startActivity(intent);

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
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        _blnSubPageFlag = true;

        //選択場所によって読取BCを判定する
        if(_BCReadPosition == TextPosition.LeaseDestination) {

            ((EditText) findViewById(R.id.txtLeaseSettingDestination)).setText(strReadData);
            _LeaseCode = strReadData;

            if (InputCheck()) {         //20191122 Add Sasaki
                //貸出先を表示
                ViewLeaseDestinationData(strReadData);
            } else {
                ((EditText) findViewById(R.id.txtLeaseSettingDestination)).setText("");
                ((TextView) findViewById(R.id.lblLeaseSettingDestinationName)).setText("");
                _LeaseCode = "";
                _LeaseName = "";
                //ボタンを非活性にする
                setButtonEnabled(false);
                return;
            }

        } else if(_BCReadPosition == TextPosition.LeaseResponsible) {

            ((EditText) findViewById(R.id.txtLeaseSettingResponsible)).setText(strReadData);
            _EmployeeCode = strReadData;

            if (InputCheck()) {         //20191122 Add Sasaki
                //貸出担当を表示
                ViewLeaseResponsibleData(strReadData);
            } else {
                ((EditText) findViewById(R.id.txtLeaseSettingResponsible)).setText("");
                ((TextView) findViewById(R.id.labLeaseSettingResponsibleName)).setText("");
                _EmployeeCode = "";
                _EmployeeName = "";
                //ボタンを非活性にする
                setButtonEnabled(false);
                return;
            }

        }

        //ボタンを制御する
        ResetButton();

        _blnSubPageFlag = false;

    }

    /**
     * 読取開始
     */
    protected void ReaderOn(){
        // バーコードOn
        Barcode_Read_Start();
    }

    /**
     * 読取終了
     */
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

    /**
     * メニュー選択イベント
     * @param item
     * @return
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            int id = item.getItemId();
            switch(id){
                case android.R.id.home:     //戻るボタン
                    //ボタン非活性
                    AllResetButton(false);

                    //メニュー画面を表示
                    Intent intentreturn = new Intent(getApplication(), MainActivity.class);
                    startActivity(intentreturn);

                    return true;

                case R.id.action_setting:
                    //ボタン非活性
                    AllResetButton(false);

                    try {
                        //読取OFF
                        ReaderOff();

                        //設定 →　システム設定画面開く
                        Intent intent = new Intent(getApplication(), SystemSett.class);
                        startActivityForResult(intent, BUTTON_SETTEI);
                    } catch (Exception e) {
                        //ボタン活性
                        ResetButton();
                    }

                    return true;

            }

            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * カレンダークリック時のイベント
     * @param strDate 日付
     */
    @Override
    public void onCalenderClick(String strDate) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        if (strDate != null && !"".equals(strDate)){
            _ReturnDate = strDate;
            if (InputReturnDateCheck()) {
                ((TextView) findViewById(R.id.txtLeaseSettingReturn)).setText(strDate);
            } else {
                //元の日付に戻す
                _ReturnDate = ((TextView) findViewById(R.id.txtLeaseSettingReturn)).getText().toString();
                //過去日の場合、エラーメッセージを表示する
                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_ERROR_RETURN, "", this)
                        , SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_RETURN, "", this));
            }
        }

        //ボタンを制御する
        ResetButton();
    }

    /**
     * 文言を表示します
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING,"",this));                   //貸出　タイトル
        ((TextView)findViewById(R.id.labLeaseSettingDestination))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_DESTINATION, "", this));   //貸出先
        ((TextView)findViewById(R.id.labBiko))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_BIKO, "", this));            //備考
        ((TextView)findViewById(R.id.labLeaseSettingResponsible))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_RESPONSIBLE, "", this));   //貸出担当
        ((TextView)findViewById(R.id.labLeaseSettingReturn))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_RETURN, "", this));         //返却予定日
        ((TextView)findViewById(R.id.btnLeaseSettingDestination))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", this));           //貸出先選択ボタン
        ((TextView)findViewById(R.id.btnLeaseSettingResponsible))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", this));           //貸出担当選択ボタン
        ((TextView)findViewById(R.id.btnLeaseSettingReturn))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", this));           //返却予定日選択ボタン
        ((TextView)findViewById(R.id.labLeaseSubTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_DESTINATION_SUB_TITLE, "", this));     //貸出先、返却日入力
        ((TextView)findViewById(R.id.btnLeaseSettingNext))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_NEXT, "", this));                           //次へ
        ((TextView)findViewById(R.id.btnClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));           //クリア
    }

    /**
     * 貸出先、貸出担当の入力チェックを行います
     * @return
     */
    private boolean InputCheck(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        boolean blnReturn = true   ;

        String strBiko = ((EditText) findViewById(R.id.editBiko)).getText().toString();

        //貸出先、貸出担当共に未入力の場合、エラー
        if ("".equals(_LeaseCode) && "".equals(_EmployeeCode)) {
            blnReturn = false;
        //桁数オーバー(貸出先)の場合、エラー
        } else if (_LeaseCode.getBytes(Charset.forName(CommonConst.DEFUALT_CHAR)).length > CommonConst.VIEW_MAXLENGTH_LEASE_CD) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_INPUT_MAX_LENGTH, "", this));
            //入力項目を選択状態とする
            EditText txtCode =  findViewById(R.id.txtLeaseSettingDestination);
            //((EditText) findViewById(R.id.txtLeaseSettingDestination)).setSelection(0, _LeaseCode.length());                      20191122 Del Sasaki
            blnReturn = false;
        //桁数オーバー(貸出担当)の場合、エラー
        } else if (_EmployeeCode.getBytes(Charset.forName(CommonConst.DEFUALT_CHAR)).length > CommonConst.VIEW_MAXLENGTH_EMPLOYEE_CD) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_INPUT_MAX_LENGTH, "", this));
            //入力項目を選択状態とする
            EditText txtCode =  findViewById(R.id.txtLeaseSettingResponsible);
            //((EditText) findViewById(R.id.txtLeaseSettingResponsible)).setSelection(0, _EmployeeCode.length());                   20191122 Del Sasaki
            blnReturn = false;
        //桁数オーバー(備考)の場合、エラー
        } else if (strBiko.getBytes(Charset.forName(CommonConst.DEFUALT_CHAR)).length > CommonConst.VIEW_MAXLENGTH_BIOS) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_INPUT_MAX_LENGTH, "", this));
            //入力項目を選択状態とする
            EditText txtCode =  findViewById(R.id.editBiko);
            //((EditText) findViewById(R.id.editBiko)).setSelection(0, strBiko.length());                                     20191122 Del Sasaki
            blnReturn = false;
        }

        return blnReturn;
    }

    /**
     * 返却予定日入力チェックを行います。
     * @return
     */
    private boolean InputReturnCheck(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        boolean blnReturn = true;

        //返却予定日が未入力の場合、エラー
        if ("".equals(_ReturnDate)) {
            blnReturn = false;
        }

        return blnReturn;
    }

    /**
     * 返却予定日過去入力日チェックを行います。
     * @return
     */
    private boolean InputReturnDateCheck(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        boolean blnReturn = true;

        try {
            //返却予定日
            SimpleDateFormat date = new SimpleDateFormat("yyyy/MM/dd");
            Date returnDate = date.parse(_ReturnDate);

            //本日日付
            Date today = date.parse(Common.getNowTime("yyyy/MM/dd"));

            //返却予定日が未入力の場合、エラー
            if (returnDate.before(today)) {
                blnReturn = false;
            }
            return blnReturn;
        } catch (ParseException pe) {
            return false;
        }
    }

    /**
     * BC/QRで読取した貸出先を設定する
     * @param LeaseCode 読取データ
     */
    private void ViewLeaseDestinationData(final String LeaseCode) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        SQLiteDatabase db = null;

        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //貸出先を取得する
            _LeaseCode = LeaseCode.trim();
            _LeaseName = LeaseFactory.GetLeaseDestinationName(db, LeaseCode).trim();

            //名称を画面に表示する
            ((TextView) findViewById(R.id.txtLeaseSettingDestination)).setText(_LeaseCode);
            ((TextView) findViewById(R.id.lblLeaseSettingDestinationName)).setText(Common.substring(_LeaseName,CommonConst.VIEW_LEASE_SETTING_NAME_SIZE));

            if ("".equals(_LeaseName)) {
                //貸出先マスタに存在しない場合、エラー
                showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_ERR_MASTER_NONE, "", this));
                ((EditText)findViewById(R.id.txtLeaseSettingDestination)).setSelection(0, _LeaseCode.length());
            } else {
                //貸出担当に移動
                _BCReadPosition = TextPosition.LeaseResponsible;
                EditText text = findViewById(R.id.txtLeaseSettingResponsible);
                text.setFocusable(true);
                text.requestFocus();
            }

        } catch (Exception ex){
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
     * BC/QRで読取した貸出担当を設定する
     * @param EmployeeCode 読取データ
     */
    private void ViewLeaseResponsibleData(final String EmployeeCode) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        SQLiteDatabase db = null;

        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //貸出担当を取得する
            _EmployeeCode = EmployeeCode.trim();
            _EmployeeName = EmployeeFactory.GetEmployeeName(db, EmployeeCode).trim();

            //名称を画面に表示する
            ((TextView) findViewById(R.id.txtLeaseSettingResponsible)).setText(_EmployeeCode);
            ((TextView) findViewById(R.id.labLeaseSettingResponsibleName)).setText(Common.substring(_EmployeeName,CommonConst.VIEW_LEASE_SETTING_NAME_SIZE));

            if ("".equals(_EmployeeName)) {
                //社員マスタに存在しない場合、エラー
                showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_EMPLOYEE_NONE,"",this));
                ((EditText)findViewById(R.id.txtLeaseSettingResponsible)).setSelection(0, _EmployeeCode.length());
            } else {
                //貸出先に移動
                _BCReadPosition = TextPosition.LeaseDestination;
                EditText text = findViewById(R.id.txtLeaseSettingDestination);
                text.setFocusable(true);
                text.requestFocus();
            }

        } catch (Exception ex){
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
     * 次へボタンクリックイベント処理
     * @param db
     */
    private void NextButtonClick(SQLiteDatabase db) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //貸出先、貸出担当の入力チェックをする
        if (!InputCheck()) {
            return;
        }

        //貸出先マスタチェック
        if (!"".equals(_LeaseCode)) {
            _LeaseName = LeaseFactory.GetLeaseDestinationName(db, _LeaseCode);
            if ("".equals(_LeaseName)) {
                ((EditText)findViewById(R.id.txtLeaseSettingDestination)).setSelection(0, _LeaseCode.length());
                showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_ERR_MASTER_NONE, "", this));

                //貸出先にフォーカス移動 20191213 Add Sasaki
                _BCReadPosition = TextPosition.LeaseDestination;
                EditText text = findViewById(R.id.txtLeaseSettingDestination);
                text.setFocusable(true);
                text.requestFocus();
                return;
            }
        }

        //社員マスタチェック
        if (!"".equals(_EmployeeCode)) {
            _EmployeeName = EmployeeFactory.GetEmployeeName(db, _EmployeeCode);
            if ("".equals(_EmployeeName)) {
                ((EditText)findViewById(R.id.txtLeaseSettingResponsible)).setSelection(0, _EmployeeCode.length());
                showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_EMPLOYEE_NONE, "", this));

                //貸出担当にフォーカス移動 20191213 Add Sasaki
                _BCReadPosition = TextPosition.LeaseResponsible;
                EditText text = findViewById(R.id.txtLeaseSettingResponsible);
                text.setFocusable(true);
                text.requestFocus();
                return;
            }
        }

        //返却予定日の入力チェックをする
        if (!InputReturnCheck()) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_ERR_RETURN_INPUT, "", this));
            findViewById(R.id.txtLeaseSettingReturn).setFocusable(true);    //20191213 Add Sasaki

            return;
        }

        //貸出先名称がない場合、コードはブランク
        if ("".equals(_LeaseName)) {
            _LeaseCode = "";
        }

        //社員名称がない場合、コードはブランク
        if ("".equals(_EmployeeName)) {
            _EmployeeCode = "";
        }

        //貸出資産読取画面表示
        Intent intent = new Intent(getApplication(), LeaseRead.class);
        intent.putExtra(CommonConst.VIEW_KEY_LEASE_CD, _LeaseCode);              //貸出先コードの値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LEASE_NAME, _LeaseName);            //貸出先名の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD, _EmployeeCode);       //社員コードの値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_EMPLOYEE_NAME, _EmployeeName);     //社員名の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_DATE, _ReturnDate);                  //返却予定日の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LEASE_REMARK,
                ((EditText)findViewById(R.id.editBiko)).getText().toString());       //備考を渡す

        startActivityForResult(intent, FORM_REQUEST_READ);

    }

    /**
     * 初期表示時のボタン設定をします
     */
    private void InitializeButtonEnable() {
        //ボタンを非活性にする
        setButtonEnabled(false);
    }

    /**
     * テキスト、ボタンを制御します
     */
    private void ResetButton() {

        //入力項目が入力された場合、次へボタンを活性
        if (InputCheck() && InputReturnCheck() && InputReturnDateCheck()) {
            //ボタンを活性にする
            setButtonEnabled(true);
        } else {
            //ボタンを非活性にする
            setButtonEnabled(false);
        }
    }

    /**
     * クリア処理
     */
    private void DataClear() {
        //画面のクリア
        ((EditText)findViewById(R.id.txtLeaseSettingDestination)).setText("");      //貸出先
        ((TextView)findViewById(R.id.lblLeaseSettingDestinationName)).setText(""); //貸出先名
        ((EditText)findViewById(R.id.txtLeaseSettingResponsible)).setText("");      //貸出担当
        ((TextView)findViewById(R.id.labLeaseSettingResponsibleName)).setText(""); //貸出担当名
        ((TextView)findViewById(R.id.txtLeaseSettingReturn)).setText("");            //返却予定日
        ((EditText)findViewById(R.id.editBiko)).setText("");                           //備考

        //内部データのクリア
        _LeaseCode = "";
        _LeaseName = "";
        _EmployeeCode = "";
        _EmployeeName = "";
        _ReturnDate = "";

        //ボタンを非活性
        setButtonEnabled(false);
    }

    /**
     * ボタンの非活性、活性をします
     * @param blnEnable
     */
    private void setButtonEnabled(boolean blnEnable) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        final boolean blnfinalEnabled = blnEnable;
        runOnUiThread(new Runnable() { //20200121 Add Thread Sasaki
            @Override
            public void run() {
                //クリアボタン,全て未入力の場合
                if ("".equals(_LeaseCode) && "".equals(_EmployeeCode)
                        && "".equals(_ReturnDate) && "".equals(((EditText) findViewById(R.id.editBiko)).getText().toString())) {
                    Drawable leftDrawable = getResources().getDrawable(R.drawable.trash_32_disable);
                    leftDrawable.setBounds(0, 0, leftDrawable.getMinimumWidth(), leftDrawable.getMinimumHeight());
                    Button clear = findViewById(R.id.btnClear);
                    clear.setBackground(getDrawable(R.drawable.execute_button_clear_disable));
                    clear.setCompoundDrawables(leftDrawable, null, null, null);
                    clear.setTextColor(Color.parseColor("#a9a9a9"));
                    clear.setEnabled(false);
                } else {
                    //クリアボタン
                    Drawable leftDrawable = getResources().getDrawable(R.drawable.trash_32);
                    leftDrawable.setBounds(0, 0, leftDrawable.getMinimumWidth(), leftDrawable.getMinimumHeight());
                    Button clear = findViewById(R.id.btnClear);
                    clear.setBackground(getDrawable(R.drawable.execute_button_clear));
                    clear.setCompoundDrawables(leftDrawable, null, null, null);
                    clear.setTextColor(Color.parseColor("#ffffff"));
                    clear.setEnabled(true);
                }

                if (blnfinalEnabled) {
                    //次へボタン
                    Drawable nextDraw = getResources().getDrawable(R.drawable.next_32);
                    nextDraw.setBounds(0, 0, nextDraw.getMinimumWidth(), nextDraw.getMinimumHeight());
                    Button next = findViewById(R.id.btnLeaseSettingNext);
                    next.setBackground(getDrawable(R.drawable.execute_button));
                    next.setCompoundDrawables(nextDraw, null, null, null);
                    next.setTextColor(Color.parseColor("#ffffff"));
                    next.setEnabled(true);
                } else {
                    //次へボタン
                    Drawable nextDraw = getResources().getDrawable(R.drawable.next_32_disable);
                    nextDraw.setBounds(0, 0, nextDraw.getMinimumWidth(), nextDraw.getMinimumHeight());
                    Button next = findViewById(R.id.btnLeaseSettingNext);
                    next.setBackground(getDrawable(R.drawable.execute_button_diable));
                    next.setCompoundDrawables(nextDraw, null, null, null);
                    next.setTextColor(Color.parseColor("#a9a9a9"));
                    next.setEnabled(false);
                }
            }
        });
    }

    /**
     * 返却予定日の自動セット
     */
    private void setAutoReturnDueDate() {
        SQLiteDatabase db = null;
        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //返却予定日の自動セット設定取得
            String[] setSysdateHt = SettingFactory.GetLeaseSetSysdateHT(db);
            if (setSysdateHt[0].equals(CommonConst.LEASE_SET_SYSDATE_HT.NO.getString())) {
                //自動セットをしない場合は終了
                return;
            }

            //加算する日数を取得
            int addDate;
            addDate = Common.toInt(setSysdateHt[1]);

            //システム日時に設定日数を加算
            Calendar calSysDate = Calendar.getInstance();
            calSysDate.add(Calendar.DAY_OF_MONTH, addDate);
            DateFormat df = new SimpleDateFormat("yyyy/MM/dd");

            //画面に表示
            String retDate = df.format(calSysDate.getTime());
            TextView txtReturnDueDate = findViewById(R.id.txtLeaseSettingReturn);
            txtReturnDueDate.setText(retDate);
            _ReturnDate = retDate;

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {

        //選択ボタン
        findViewById(R.id.btnLeaseSettingDestination).setEnabled(blnEnabled);
        findViewById(R.id.btnLeaseSettingResponsible).setEnabled(blnEnabled);
        findViewById(R.id.btnLeaseSettingReturn).setEnabled(blnEnabled);

        //20191016 Upd Start Sasaki
        //setButtonEnabled(blnEnabled);
        if (blnEnabled) {
            ResetButton();
        } else {
            setButtonEnabled(blnEnabled);
        }
        //20191016 Upd End Sasaki
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return C_Setting.KEY_EACH_LEASE;
    }

 /**************************************************/
    protected void OpenMainActivity(){ }
    protected void RFIDDataReceived(String strReadData, short tempRSSI, final String strPcHex) { }
}
