package com.rfid.common;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MenuItem;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import com.rfid.Factory.InventoryStartFactory;
import com.rfid.Factory.LocationFactory;
import com.rfid.main.LoginMn;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;
import com.rfid.main.MainActivity;
import com.rfid.setting.SystemSett;

import java.nio.charset.Charset;

/**
 * ロケーション選択画面のクラス
 * ロケーションを選択します。
 * 設定マスタの値で、どの区分まで表示するか制御します。
 *
 */
public abstract class BaseLocationSetting extends BaseRFIDActivity {

    //DB接続
    private SQLiteDatabase _db;

    //ロケーション選択画面からの戻り用フラグ
    private boolean _blnSubPageFlag = false;

    //ロケーションコード
    private String _LocLCode = "";
    private String _LocMCode = "";
    private String _LocSCode = "";

    //ロケーション名称
    private String _LocLName = "";
    private String _LocMName = "";
    private String _LocSName = "";

    //選択ボタン用
    private static final int FORM_REQUEST_ALL = 0;   //ALL
    private static final int FORM_REQUEST_LOCATION_L = 1;   //大区分
    private static final int FORM_REQUEST_LOCATION_M = 2;   //中区分
    private static final int FORM_REQUEST_LOCATION_S = 3;   //小区分
    private static final int FORM_REQUEST_READ = 4;
    private static final int BUTTON_SETTEI = 9;         //設定画面

    private String[] _LocUseKbn = null;

    private Activity _nowActivity = this;
    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {

        SQLiteDatabase db = null;

        try {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.activity_location_setting);

            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //文言表示
            InitializeWord();

            //設定マスタより区分を取得する
            _LocUseKbn = GetLocationUsage(db);

            //ボタンリセット
            ResetButton();

            //操作時間のタイマーをセット
            StartTimeoutTimer(CommonConst.VIEW_WORD_INV_LOC_TITLE);

            //ロケ大区分選択ボタン Clickイベント
            final Activity nowActivity = this;
            findViewById(R.id.btnSettingLocL).setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {

                    if (checkTimeout()) {return;}

                    //ボタン非活性
                    AllResetButton(false);

                    //フラグ制御
                    _blnSubPageFlag = true;

                    //ReaderOff
                    ReaderOff();

                    //ロケ大区分指定画面表示
                    Intent intent = new Intent(getApplication(), LocationSelect.class);
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_TITLE,
                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LOC_L_KBN, "", nowActivity));      //一覧表示タイトル
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_L_DATA, ((EditText)findViewById(R.id.txtInvSettingLocL)).getText().toString());
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_SELECT, CommonConst.LOCATION_DIV.L.getString());
                    startActivityForResult(intent, FORM_REQUEST_LOCATION_L);

                }
            });

            //ロケ大区分テキストボタン Entキーイベント
            EditText _editTextL = findViewById(R.id.txtInvSettingLocL);
            _editTextL.setOnEditorActionListener(new TextView.OnEditorActionListener() {
                @Override
                public boolean onEditorAction(TextView view, int actionId, KeyEvent event) {
                    //Enter押下処理
                    if (actionId == EditorInfo.IME_ACTION_DONE){
                        //ロケ大区分名の取得
                        _LocLCode = ((EditText)findViewById(R.id.txtInvSettingLocL)).getText().toString();
                        _LocLName = LocationFactory.GetLocationLName(nowActivity, _LocLCode);
                        ((TextView)findViewById(R.id.lblInvSettingLocLName)).setText(Common.substring(_LocLName, CommonConst.VIEW_LOCATION_SETTING_NAME_SIZE));
                    }
                    return false;
                }
            });

            /**
             * ロケ大区分テキスト変更イベント
             */
            _editTextL.addTextChangedListener(new TextWatcher() {
                @Override
                public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
                @Override
                public void onTextChanged(CharSequence s, int start, int before, int count) {}
                @Override
                public void afterTextChanged(Editable s) {
                    //ロケ選択画面からの戻り以外の場合
                    if (!_blnSubPageFlag) {
                        //ロケ大区分の変更処理
                        EditLocL();
                    }
                }
            });

            _editTextL.setOnFocusChangeListener(new View.OnFocusChangeListener() {
                @Override
                public void onFocusChange(View v, boolean hasFocus) {
                    if (hasFocus) {
                        // 👇 フォーカスが当たった時の処理
                        if (checkTimeout()) {return;}
                    }
                }
            });

            //ロケ中区分選択ボタン Clickイベント
            findViewById(R.id.btnSettingLocM).setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {

                    if (checkTimeout()) {return;}

                    //ボタン非活性
                    AllResetButton(false);

                    //フラグ制御
                    _blnSubPageFlag = true;

                    //ReaderOff
                    ReaderOff();

                    //ロケ中区分指定画面表示
                    Intent intent = new Intent(getApplication(), LocationSelect.class);
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_TITLE,
                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LOC_M_KBN, "", nowActivity));      //一覧表示タイトル
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_L_DATA, _LocLCode);              //大区分の値を渡す
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_M_DATA, ((EditText)findViewById(R.id.txtInvSettingLocM)).getText().toString());
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_SELECT, CommonConst.LOCATION_DIV.M.getString());
                    startActivityForResult(intent, FORM_REQUEST_LOCATION_M);

                }
            });

            //ロケ中区分テキストボタン Entキーイベント
            EditText _editTextM = findViewById(R.id.txtInvSettingLocM);
            _editTextM.setOnEditorActionListener(new TextView.OnEditorActionListener() {
                @Override
                public boolean onEditorAction(TextView view, int actionId, KeyEvent event) {
                    //Enter押下処理
                    if (actionId == EditorInfo.IME_ACTION_DONE){
                        //ロケ中区分名の取得
                        _LocMCode = ((EditText)findViewById(R.id.txtInvSettingLocM)).getText().toString();
                        _LocMName = LocationFactory.GetLocationMName(nowActivity, _LocLCode, _LocMCode);
                        ((TextView)findViewById(R.id.lblInvSettingLocMName)).setText(Common.substring(_LocMName, CommonConst.VIEW_LOCATION_SETTING_NAME_SIZE));
                    }
                    return false;
                }
            });

            /**
             * ロケ中区分テキスト変更イベント
             */
            _editTextM.addTextChangedListener(new TextWatcher() {
                @Override
                public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
                @Override
                public void onTextChanged(CharSequence s, int start, int before, int count) {}
                @Override
                public void afterTextChanged(Editable s) {
                    //ロケ選択画面からの戻り以外の場合
                    if (!_blnSubPageFlag) {
                        //ロケ中区分の変更処理
                        EditLocM();
                    }
                }
            });

            _editTextM.setOnFocusChangeListener(new View.OnFocusChangeListener() {
                @Override
                public void onFocusChange(View v, boolean hasFocus) {
                    if (hasFocus) {
                        // 👇 フォーカスが当たった時の処理
                        if (checkTimeout()) {return;}
                    }
                }
            });

            //ロケ小区分選択ボタン Clickイベント
            findViewById(R.id.btnSettingLocS).setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {

                    if (checkTimeout()) {return;}

                    //ボタン非活性
                    AllResetButton(false);

                    //フラグ制御
                    _blnSubPageFlag = true;

                    //ReaderOff
                    ReaderOff();

                    //ロケ小区分指定画面表示
                    Intent intent = new Intent(getApplication(), LocationSelect.class);
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_TITLE,
                            SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LOC_S_KBN, "", nowActivity));      //一覧表示タイトル
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_L_DATA, _LocLCode);              //大区分の値を渡す
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_M_DATA, _LocMCode);              //中区分の値を渡す
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_S_DATA, ((EditText)findViewById(R.id.txtInvSettingLocS)).getText().toString());
                    intent.putExtra(CommonConst.VIEW_KEY_LOCATION_SELECT, CommonConst.LOCATION_DIV.S.getString());
                    startActivityForResult(intent, FORM_REQUEST_LOCATION_S);

                }
            });

            //ロケ小区分テキストボタン Entキーイベント
            EditText _editTextS = findViewById(R.id.txtInvSettingLocS);
            _editTextS.setOnEditorActionListener(new TextView.OnEditorActionListener() {
                @Override
                public boolean onEditorAction(TextView view, int actionId, KeyEvent event) {
                    //Enter押下処理
                    if (actionId == EditorInfo.IME_ACTION_DONE){
                        //ロケ小区分名の取得
                        _LocSCode = ((EditText)findViewById(R.id.txtInvSettingLocS)).getText().toString();
                        _LocSName = LocationFactory.GetLocationSName(nowActivity, _LocLCode, _LocMCode, _LocSCode);
                        ((TextView)findViewById(R.id.lblInvSettingLocSName)).setText(Common.substring(_LocSName, CommonConst.VIEW_LOCATION_SETTING_NAME_SIZE));
                    }
                    return false;
                }
            });

            /**
             * ロケ小区分テキスト変更イベント
             */
            _editTextS.addTextChangedListener(new TextWatcher() {
                @Override
                public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
                @Override
                public void onTextChanged(CharSequence s, int start, int before, int count) {}
                @Override
                public void afterTextChanged(Editable s) {
                    //ロケ選択画面からの戻り以外の場合
                    if (!_blnSubPageFlag) {
                        //ロケ小区分の変更処理
                        EditLocS();
                    }
                }
            });

            _editTextS.setOnFocusChangeListener(new View.OnFocusChangeListener() {
                @Override
                public void onFocusChange(View v, boolean hasFocus) {
                    if (hasFocus) {
                        // 👇 フォーカスが当たった時の処理
                        if (checkTimeout()) {return;}
                    }
                }
            });

            //次へボタン Clickイベント
            findViewById(R.id.btnSettingNext).setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {

                    if (checkTimeout()) {return;}

                    if (_editTextL.getText().toString().equals("")) {
                        _editTextL.requestFocus();
                        showErrorMessage(GetWord(CommonConst.VIEW_WORD_INV_MSG_LOC_L));
                        return;
                    }

                    boolean isHead = C_Setting.getLoginHeadOffice(_nowActivity).equals("Y");
                    boolean isAuth = C_Setting.getLoginAuth(_nowActivity).equals("A");
                    if (!isHead && !isAuth && _editTextM.getText().toString().equals("")) {
                        //本部フラグが拠点　かつ　権限区分が通常者　かつ　ロケ中に入力がない場合
                        _editTextM.requestFocus();
                        showErrorMessage(GetWord(CommonConst.VIEW_WORD_INV_MSG_LOC_M));
                        return;
                    }

                    //ボタン非活性
                    AllResetButton(false);

                    final Context context = view.getContext();

                    SQLiteDatabase db = null;

                    try {
                        //データベースヘルパーのインスタンスを作成する
                        DatabaseHelper dbHelper = new DatabaseHelper(context);
                        db = dbHelper.getWritableDatabase();

                        //次へボタン処理
                        NextButtonClick(db);

                    } catch (Exception ex) {
                        showErrorMessage(ex.getMessage());
                        Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                    } finally {
                        if (db != null) {
                            db.close();
                        }
                    }

                }
            });

            //サービス開始
            super.startService();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.close();
            }

        }
    }

    /**
     * レジュームイベント
     */
    @Override
    public void onResume() {
        try {
            super.onResume();

            //サブ画面からの戻りの場合終了
            if (_blnSubPageFlag) {
                _blnSubPageFlag = false;
                return;
            }

            //画面初期化
            //InitializeActivity();

            //タイマー開始
            StartTimer();

            //リーダーON
            ReaderOn();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * ポーズイベント
     */
    @Override
    public void onPause() {
        try {
            super.onPause();

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
        }
    }

    /**
     * サブ画面から戻るイベント処理をします。
     *
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        try {
            switch (requestCode) {
                case FORM_REQUEST_LOCATION_L:        //ロケ大区分選択画面から戻る
                    if (resultCode == RESULT_OK) {
                        //ロケコードを保持する
                        _LocLCode = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION_CD);
                        _LocLName = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION);

                        // サブ画面から戻り値を取得して表示する
                        TextView text = findViewById(R.id.txtInvSettingLocL);
                        text.setText(_LocLCode);
                        TextView textName = findViewById(R.id.lblInvSettingLocLName);
                        textName.setText(Common.substring(_LocLName, CommonConst.VIEW_LOCATION_SETTING_NAME_SIZE));

                        //テキストボックスをクリアする
                        ClearTextData(FORM_REQUEST_LOCATION_L);
                    }
                    _blnSubPageFlag = false;
                    //ReaderOn
                    ReaderOn();
                    break;

                case FORM_REQUEST_LOCATION_M:        //ロケ中区分選択画面から戻る
                    if (resultCode == RESULT_OK) {
                        //ロケコードを保持する
                        _LocMCode = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION_CD);
                        _LocMName = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION);

                        // サブ画面から戻り値を取得して表示する
                        TextView textM = findViewById(R.id.txtInvSettingLocM);
                        textM.setText(_LocMCode);
                        TextView textMName = findViewById(R.id.lblInvSettingLocMName);
                        textMName.setText(Common.substring(_LocMName, CommonConst.VIEW_LOCATION_SETTING_NAME_SIZE));

                        //テキストボックスをクリアする
                        ClearTextData(FORM_REQUEST_LOCATION_M);
                    }
                    _blnSubPageFlag = false;
                    //ReaderOn
                    ReaderOn();
                    break;

                case FORM_REQUEST_LOCATION_S:        //ロケ小区分選択画面から戻る
                    if (resultCode == RESULT_OK) {
                        //ロケコードを保持する
                        _LocSCode = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION_CD);
                        _LocSName = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION);

                        // サブ画面から戻り値を取得して表示する
                        TextView textS = findViewById(R.id.txtInvSettingLocS);
                        textS.setText(_LocSCode);
                        TextView textSName = findViewById(R.id.lblInvSettingLocSName);
                        textSName.setText(Common.substring(_LocSName, CommonConst.VIEW_LOCATION_SETTING_NAME_SIZE));
                    }
                    _blnSubPageFlag = false;
                    //ReaderOn
                    ReaderOn();
                    break;

                case FORM_REQUEST_READ:
                    //大区分
                    _LocLCode = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION_L_DATA);
                    _LocLName = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION_L_TEXT);
                    ((TextView)findViewById(R.id.txtInvSettingLocL)).setText(_LocLCode);
                    ((TextView)findViewById(R.id.lblInvSettingLocLName)).setText(_LocLName);

                    //中区分
                    _LocMCode = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION_M_DATA);
                    _LocMName = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION_M_TEXT);
                    ((TextView)findViewById(R.id.txtInvSettingLocM)).setText(_LocMCode);
                    ((TextView)findViewById(R.id.lblInvSettingLocMName)).setText(_LocMName);

                    //小区分
                    _LocSCode = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION_S_DATA);
                    _LocSName = data.getStringExtra(CommonConst.VIEW_KEY_LOCATION_S_TEXT);
                    ((TextView)findViewById(R.id.txtInvSettingLocS)).setText(_LocSCode);
                    ((TextView)findViewById(R.id.lblInvSettingLocSName)).setText(_LocSName);
                    break;

                case BUTTON_SETTEI:     //設定画面
                    ResetButton();

                default:
                    break;
            }

            //ボタンを制御する
            ResetButton();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    //キー入力イベント
    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        try {
            switch (keyCode) {
                case KeyEvent.KEYCODE_BACK:
                    //ボタン非活性
                    AllResetButton(false);

                    //Back ボタン メニュー画面を表示
                    Intent intent = onReturnPage();
                    startActivity(intent);

                    return false;
            }

            return super.onKeyDown(keyCode, event);
        } catch (Exception ex) {
            return false;
        }
    }

    /**
     * バーコード取得のイベント処理をします。
     * 親クラスから呼ばれます。
     *
     * @param strReadData
     */
    @Override
    protected void BarcodeDataReceived(String strReadData) {

        //読取データをチェックする
        if (!ChekLocationData(strReadData)) {
            showToastMessage(SystemSetting.getString(CommonConst.VIEW_WORD_STORAGE_LOCATION_INPUT_ERR_LABEL_QR, "", this));
            return;
        }

        //20210918 ADD Takase V2.0 START
        //保管以外の場合
        if (!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_STORAGE.getString())) {
            if (!CheckLocationAuth(strReadData)) {
                //エラーメッセージを表示
                showToastMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_TARGET_HUB_LOC , "", this));
                return;
            }
        }
        //20210918 ADD Takase V2.0 END

        //ロケコード表示
        ViewLocationData(strReadData);

        //ボタンを制御する
        ResetButton();

        SQLiteDatabase db = null;

        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //ロケ存在チェック
            if (!LocationFactory.ExistLocation(db, _LocLCode, _LocMCode, _LocSCode)) {
                setLocForcus(_LocLCode, _LocMCode, _LocSCode);
                showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_LOC_NONE, "", this));
                DisabledNextButton();
                return;
            }
        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    protected void ReaderOn() {
        // バーコードOn
        Barcode_Read_Start();
    }

    protected void ReaderOff() {
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
     * ロケ大区分を変更した際の処理
     */
    private void EditLocL() {
        //ロケ大区分コードが変更された場合、ロケ大区分名をクリアする
        if (_LocLCode.equals(((EditText)findViewById(R.id.txtInvSettingLocL)).getText().toString())) {
            return;
        }

        ((TextView) findViewById(R.id.lblInvSettingLocLName)).setText("");
        _LocLName = "";

        //ロケ大区分を再設定する
        _LocLCode = ((EditText)findViewById(R.id.txtInvSettingLocL)).getText().toString();

        //リセットする
        ResetButton();

    }

    /**
     * ロケ中区分を変更した際の処理
     */
    private void EditLocM() {
        //ロケ中区分コードが変更された場合、ロケ中区分名をクリアする
        if (_LocMCode.equals(((EditText)findViewById(R.id.txtInvSettingLocM)).getText().toString())) {
            return;
        }

        ((TextView) findViewById(R.id.lblInvSettingLocMName)).setText("");
        _LocMName = "";

        //ロケ中区分を再設定する
        _LocMCode = ((EditText)findViewById(R.id.txtInvSettingLocM)).getText().toString();

        //リセットする
        ResetButton();
    }

    /**
     * ロケ小区分を変更した際の処理
     */
    private void EditLocS() {
        //ロケ小区分コードが変更された場合、ロケ小区分名をクリアする
        if (_LocSCode.equals(((EditText)findViewById(R.id.txtInvSettingLocS)).getText().toString())) {
            return;
        }

        ((TextView) findViewById(R.id.lblInvSettingLocSName)).setText("");
        _LocSName = "";

        //ロケ小区分を再設定する
        _LocSCode = ((EditText)findViewById(R.id.txtInvSettingLocS)).getText().toString();

        //リセットする
        ResetButton();
    }

    /**
     * エラー時にフォーカスを当てる
     * @param strLocLCode ロケ大区分
     * @param strLocMCode ロケ中区分
     * @param strLocSCode ロケ小区分
     */
    private void setLocForcus(String strLocLCode, String strLocMCode, String strLocSCode) {

        if (!"".equals(strLocLCode) && !"".equals(strLocMCode) && !"".equals(strLocSCode)) {
            //ロケ小区分までの場合、ロケ小区分にフォーカス
            EditText txtLoc = findViewById(R.id.txtInvSettingLocS);
            txtLoc.setFocusable(true);
            txtLoc.requestFocus();
        } else if (!"".equals(strLocLCode) && !"".equals(strLocMCode)) {
            //ロケ中区分までの場合、ロケ中区分にフォーカス
            EditText txtLoc = findViewById(R.id.txtInvSettingLocM);
            txtLoc.setFocusable(true);
            txtLoc.requestFocus();
        } else if (!"".equals(strLocLCode)) {
            //ロケ大区分までの場合、ロケ大区分にフォーカス
            EditText txtLoc = findViewById(R.id.txtInvSettingLocL);
            txtLoc.setFocusable(true);
            txtLoc.requestFocus();
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
            int id = item.getItemId();
            switch (id) {
                case android.R.id.home:     //戻るボタン

                    if (checkTimeout()) {return true;}

                    //ボタン非活性
                    AllResetButton(false);

                    //Back ボタン 実棚サブメニュー画面を表示
                    Intent intentreturn = new Intent(getApplication(), MainActivity.class);
                    startActivity(intentreturn);

                    return true;

                case R.id.action_setting:
                    //ボタン非活性
                    AllResetButton(false);

                    //設定 →　システム設定画面開く
                    Intent intent = new Intent(getApplication(), SystemSett.class);
                    startActivityForResult(intent, BUTTON_SETTEI);

                    return true;
            }

            return super.onOptionsItemSelected(item);
        } catch (Exception ex) {
            return false;
        }
    }

    /**
     * 文言を表示します。
     */
    protected void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_STORE, "", this));                     //保管　タイトル
        ((TextView) findViewById(R.id.labSettingLocL))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LOC_L_KBN, "", this));         //ロケ大区分
        ((TextView) findViewById(R.id.labSettingLocM))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LOC_M_KBN, "", this));         //ロケ中区分
        ((TextView) findViewById(R.id.labSettingLocS))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LOC_S_KBN, "", this));         //ロケ小区分
        ((TextView) findViewById(R.id.btnSettingLocL))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", this));             //選択ボタン
        ((TextView) findViewById(R.id.btnSettingLocM))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", this));             //選択ボタン
        ((TextView) findViewById(R.id.btnSettingLocS))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", this));             //選択ボタン
        ((TextView) findViewById(R.id.labSubTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_STORAGE_LOCATION_INPUT_MSG_ITEM, "", this));     //保管先の読取又は選択
        ((TextView) findViewById(R.id.btnSettingNext))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_NEXT, "", this));                 //次へ
    }

    /**
     * 入力チェックを行います。
     *
     * @param strLLoc ロケ大区分
     * @param strMLoc ロケ中区分
     * @param strSLoc ロケ小区分
     * @return
     */
    private boolean InputCheck(String strLLoc, String strMLoc, String strSLoc) {
        boolean blnReturn = true;

        //大区分が未入力の場合エラー
        if (strLLoc == "") {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_LOC_INPUT, "", this));
            blnReturn = false;
        //桁数オーバー(ロケ大区分)の場合、エラー
        } else if (strLLoc.getBytes(Charset.forName(CommonConst.DEFUALT_CHAR)).length > CommonConst.VIEW_MAXLENGTH_LOC_L_CD) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_INPUT_MAX_LENGTH , "", this));
            //入力項目を選択状態とする
            ((EditText)findViewById(R.id.txtInvSettingLocL)).setSelection(0, strLLoc.length());
            blnReturn = false;
        //桁数オーバー(ロケ中区分)の場合、エラー
        } else if (strMLoc.getBytes(Charset.forName(CommonConst.DEFUALT_CHAR)).length > CommonConst.VIEW_MAXLENGTH_LOC_M_CD) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_INPUT_MAX_LENGTH , "", this));
            //入力項目を選択状態とする
            ((EditText)findViewById(R.id.txtInvSettingLocM)).setSelection(0, strMLoc.length());
            blnReturn = false;
        //桁数オーバー(ロケ小区分)の場合、エラー
        } else if (strSLoc.getBytes(Charset.forName(CommonConst.DEFUALT_CHAR)).length > CommonConst.VIEW_MAXLENGTH_LOC_S_CD) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_INPUT_MAX_LENGTH , "", this));
            //入力項目を選択状態とする
            ((EditText)findViewById(R.id.txtInvSettingLocS)).setSelection(0, strSLoc.length());
            blnReturn = false;
        }

        return blnReturn;
    }

    /**
     * BC/QRで読取したコードをチェックする
     *
     * @param LocationData 読取データ
     * @return true：OK、false：異常
     */
    private boolean ChekLocationData(String LocationData) {
        boolean blnReturn = true;

        //ロケ大+ロケ中+ロケ小の桁数
        int intLocLMSNum = CommonConst.VIEW_MAXLENGTH_LOC_L_CD
                + CommonConst.VIEW_MAXLENGTH_LOC_M_CD
                +  CommonConst.VIEW_MAXLENGTH_LOC_S_CD;   //20191115 Add Sasaki

        //指定桁数以外の場合エラー
        //if (LocationData.length() > 9 || LocationData.length() < 3) {             20191115 Del Sasaki
        //if (LocationData.length() > intLocLMSNum || LocationData.length() < 1) {   //20191115 Add Sasaki //20200131 Del Sasaki
        if (LocationData.length() > intLocLMSNum || LocationData.length() < CommonConst.VIEW_MAXLENGTH_LOC_L_CD) {   //20200131 Add Sasaki
            blnReturn = false;
        }

        return blnReturn;
    }

    //20210918 ADD Takase V2.0 START
    /**
     * 更新権限があるロケーションかチェックする
     * @param LocationData
     * @return
     */
    private boolean CheckLocationAuth(String LocationData) {
        //20230821 ADD Sato v2.3.1 QR読取データからロケ大区分コードを抽出
        String LocLCd = LocationData.substring(0, CommonConst.VIEW_MAXLENGTH_LOC_L_CD).trim();

        //更新権限(他拠点)がない時
        if (CommonAuthority.Auth.updHubList != null) {
            //スキャンしたロケ大の権限が無ければエラー
            for (int i = 0; i < CommonAuthority.Auth.updHubList.length; i++) {
//                if (CommonAuthority.Auth.updHubList[i] == LocationData.substring(0, CommonConst.VIEW_MAXLENGTH_LOC_L_CD)) {
                if (CommonAuthority.Auth.updHubList[i].equals(LocLCd)) {        //20230821 UPD Sato v2.3.1
                    return true;
                }
            }
            return false;
        }

        //更新権限(他拠点)がある時
        SettingValue settingValue = new SettingValue();
        if (!settingValue.getTargetHub(this).equals("")) {
            //HT.対象拠点設定時
//            if (LocationData == settingValue.getTargetHub(this)) {
            if (LocLCd.equals(settingValue.getTargetHub(this))) {               //20230821 UPD Sato v2.3.1
                return true;
            } else {
                return false;
            }
        }

        return true;
    }
    //20210918 ADD Takase V2.0 END

    /**
     * BC/QRで読取したコードを設定する
     *
     * @param LocationData 読取データ
     */
    private void ViewLocationData(String LocationData) {

        SQLiteDatabase db = null;

        try {
            //クリアする
            ClearTextData(FORM_REQUEST_ALL);

            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //LocationData = LocationData.trim(); 20200121 Dell Sasaki

            if (CommonConst.LOCATION_USAGE.USE.getString().equals(_LocUseKbn[0])) {
                //if (LocationData.length() >= 3) {                                           20191115 Del Sasaki
                if (LocationData.length() >= CommonConst.VIEW_MAXLENGTH_LOC_L_CD) {     //20191115 Add Sasaki
                    //ロケ大区分
                    //_LocLCode = LocationData.substring(0, 3);                                        20191115 Del Sasaki
                    _LocLCode = LocationData.substring(0, CommonConst.VIEW_MAXLENGTH_LOC_L_CD).trim(); //20191115 Add Sasaki 20200121 Upd Sasaki
                    //ロケ大区分名称を取得する
                    _LocLName = LocationFactory.GetLocationLName(db, _LocLCode);
                    //画面に表示する
                    ((TextView) findViewById(R.id.txtInvSettingLocL)).setText(_LocLCode);
                    ((TextView) findViewById(R.id.lblInvSettingLocLName)).setText(Common.substring(_LocLName, CommonConst.VIEW_LOCATION_SETTING_NAME_SIZE));
                }
            }

            //ロケ大+ロケ中の桁数
            int intLocLMNum = CommonConst.VIEW_MAXLENGTH_LOC_L_CD + CommonConst.VIEW_MAXLENGTH_LOC_M_CD;   //20191115 Add Sasaki

            if (CommonConst.LOCATION_USAGE.USE.getString().equals(_LocUseKbn[1])) {
                //if (LocationData.length() >= 6) {                  20191115 Del Sasaki
                if (LocationData.length() >= intLocLMNum) {        //20191115 Add Sasaki
                    //ロケ中区分
                    //_LocMCode = LocationData.substring(3, 6);      20191115 Del Sasaki
                    _LocMCode = LocationData.substring(CommonConst.VIEW_MAXLENGTH_LOC_L_CD, intLocLMNum).trim();  //20191115 Add Sasaki 20200121 Upd Sasaki
                    //ロケ中区分名称を取得する
                    _LocMName = LocationFactory.GetLocationMName(db, _LocLCode, _LocMCode);
                    //画面に表示する
                    ((TextView) findViewById(R.id.txtInvSettingLocM)).setText(_LocMCode);
                    ((TextView) findViewById(R.id.lblInvSettingLocMName)).setText(Common.substring(_LocMName, CommonConst.VIEW_LOCATION_SETTING_NAME_SIZE));
                }
            }

            //ロケ大+ロケ中+ロケ小の桁数
            int intLocLMSNum = CommonConst.VIEW_MAXLENGTH_LOC_L_CD
                                + CommonConst.VIEW_MAXLENGTH_LOC_M_CD
                                +  CommonConst.VIEW_MAXLENGTH_LOC_S_CD;   //20191115 Add Sasaki

            if (CommonConst.LOCATION_USAGE.USE.getString().equals(_LocUseKbn[2])) {
                //if (LocationData.length() == 9) {                   20191115 Del Sasaki
                if (LocationData.length() == intLocLMSNum) {         //20191115 Add Sasaki
                    //ロケ小区分
                    //_LocSCode = LocationData.substring(6);                     20191115 Del Sasaki
                    _LocSCode = LocationData.substring(intLocLMNum).trim(); ;         //20191115 Add Sasaki 20200121 Upd Sasaki
                    //ロケ小区分名称を取得する
                    _LocSName = LocationFactory.GetLocationSName(db, _LocLCode, _LocMCode, _LocSCode);
                    //画面に表示する
                    ((TextView) findViewById(R.id.txtInvSettingLocS)).setText(_LocSCode);
                    ((TextView) findViewById(R.id.lblInvSettingLocSName)).setText(Common.substring(_LocSName, CommonConst.VIEW_LOCATION_SETTING_NAME_SIZE));
                }
            }

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 次へボタンクリックイベント処理
     *
     * @param db
     */
    private void NextButtonClick(SQLiteDatabase db) {

        //入力チェックをする
        if (!InputCheck(_LocLCode, _LocMCode, _LocSCode)) {
            ResetButton();
            return;
        }

        //ロケ存在チェック
        if (!LocationFactory.ExistLocation(db, _LocLCode, _LocMCode, _LocSCode)) {
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_LOC_NONE, "", this));
            ResetButton();
            return;
        }

        //
        //参照権限(他拠点)を持っていない場合
        if (CommonAuthority.Auth.readHubList != null) {
            boolean isErr = true;
            for (String hub : CommonAuthority.Auth.readHubList) {
                if (_LocLCode.equals(hub)) {
                    isErr = false;
                    break;
                }
            }

            if (isErr) {
                showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_NO_AUTHORITY, "", this));
                ResetButton();
                return;
            }
        }
        //20231011 Add Start Shibata
        if (!InventoryStartCheck(_LocLCode, db)) {
            //棚卸開始情報に画面のロケ大区分コードがない場合エラーを表示
            showErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_INV_NO_START_LOCL, "", this));
            ResetButton();
            return;
        }
        //20231011 Add End Shibata

        //保管資産読取画面表示
        Intent intent = onNextPage();
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_L_DATA, _LocLCode); //大区分の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_M_DATA, _LocMCode); //中区分の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_S_DATA, _LocSCode); //小区分の値を渡す

        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_L_TEXT, _LocLName); //大区分名称の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_M_TEXT, _LocMName); //中区分名称の値を渡す
        intent.putExtra(CommonConst.VIEW_KEY_LOCATION_S_TEXT, _LocSName); //小区分名称の値を渡す
        startActivityForResult(intent, FORM_REQUEST_READ);

    }

    /**
     * 棚卸開始済みのロケかどうかのチェックします
     * @param locLCode 画面のロケ大区分コード
     * @param db
     * @return
     */
    protected boolean InventoryStartCheck(String locLCode, SQLiteDatabase db) {
        //棚卸の場合はオーバーライドして、中身を記述して使用する
        //保管の場合はスルー
        return true;
    }

    /**
     * テキスト、ボタンを制御します。
     */
    private void ResetButton() {
        final Activity nowActivity = this;  //20210918 ADD Takase V2.0
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                SQLiteDatabase db = null;
                try {
                    //画面をリセット
                    ((TextView) findViewById(R.id.btnSettingLocL)).setEnabled(false);
                    ((TextView) findViewById(R.id.btnSettingLocM)).setEnabled(false);
                    ((TextView) findViewById(R.id.btnSettingLocS)).setEnabled(false);
                    ((TextView) findViewById(R.id.btnSettingNext)).setEnabled(false);       //次へボタン
                    ((Button) findViewById(R.id.btnSettingNext)).setBackground(getDrawable(R.drawable.execute_button_diable));
                    ((Button) findViewById(R.id.btnSettingNext)).setTextColor(Color.parseColor("#a9a9a9"));

                    //大区分が対象の場合、大区分を活性化する
                    if (CommonConst.LOCATION_USAGE.USE.getString().equals(_LocUseKbn[0])) {
                        ((TextView) findViewById(R.id.btnSettingLocL)).setEnabled(true);
                    }

                    //20210918 ADD Takase V2.0 START
                    //保管以外の場合
                    if (!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_STORAGE.getString())) {
                        SettingValue settingValue = new SettingValue();
                        String htHub =  settingValue.getTargetHub(nowActivity);
                        //HT.対象拠点が設定されている場合
                        if (!htHub.equals("") && CommonAuthority.Auth.updHubList != null) {
                            DatabaseHelper dbHelper = new DatabaseHelper(nowActivity);
                            db = dbHelper.getWritableDatabase();
                            //HT.対象拠点をロケ大区分に設定する
                            _LocLCode = htHub;
                            //ロケ大区分名称を取得する
                            _LocLName = LocationFactory.GetLocationLName(db, _LocLCode);
                            //画面に表示する
                            ((TextView) findViewById(R.id.txtInvSettingLocL)).setText(_LocLCode);
                            ((TextView) findViewById(R.id.lblInvSettingLocLName)).setText(Common.substring(_LocLName, CommonConst.VIEW_LOCATION_SETTING_NAME_SIZE));
                            //ロケ大区分固定
                            ((TextView) findViewById(R.id.txtInvSettingLocL)).setEnabled(false);
                            ((Button) findViewById(R.id.btnSettingLocL)).setEnabled(false);
                            ((TextView) findViewById(R.id.txtInvSettingLocL)).setFocusable(false);
                            ((Button) findViewById(R.id.btnSettingLocL)).setFocusable(false);
                        }
                    }
                    //20210918 ADD Takase V2.0

                    //中区分が対象かつ大区分に値がある場合、中区分を活性化する
                    if (CommonConst.LOCATION_USAGE.USE.getString().equals(_LocUseKbn[1])) {
                        if (!"".equals(_LocLCode)) {
                            ((TextView) findViewById(R.id.btnSettingLocM)).setEnabled(true);
                        }
                    }

                    //小区分が対象かつ中区分に値がある場合、小区分を活性化する
                    if (CommonConst.LOCATION_USAGE.USE.getString().equals(_LocUseKbn[2])) {
                        if (!"".equals(_LocMCode)) {
                            ((TextView) findViewById(R.id.btnSettingLocS)).setEnabled(true);
                        }
                    }

                    //大区分が入力されていた場合、次へボタンを活性化する
                    if (!"".equals(_LocLCode)) {
                        ((TextView) findViewById(R.id.btnSettingNext)).setEnabled(true);     //次へボタン
                        ((Button) findViewById(R.id.btnSettingNext)).setBackground(getDrawable(R.drawable.execute_button));
                        ((Button) findViewById(R.id.btnSettingNext)).setTextColor(Color.parseColor("#ffffff"));
                    }
                } catch (Exception ex) {
                    showErrorMessage(ex.getMessage());
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                } finally {
                    if (db != null) {
                        db.close();
                    }
                }
            }
        });
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {
        //画面をリセット
        ((TextView) findViewById(R.id.btnSettingLocL)).setEnabled(blnEnabled);
        ((TextView) findViewById(R.id.btnSettingLocM)).setEnabled(blnEnabled);
        ((TextView) findViewById(R.id.btnSettingLocS)).setEnabled(blnEnabled);
        ((TextView) findViewById(R.id.btnSettingNext)).setEnabled(blnEnabled);       //次へボタン

        if (blnEnabled) {
            ((Button) findViewById(R.id.btnSettingNext)).setBackground(getDrawable(R.drawable.execute_button));
            ((Button) findViewById(R.id.btnSettingNext)).setTextColor(Color.parseColor("#ffffff"));
        } else {
            ((Button) findViewById(R.id.btnSettingNext)).setBackground(getDrawable(R.drawable.execute_button_diable));
            ((Button) findViewById(R.id.btnSettingNext)).setTextColor(Color.parseColor("#a9a9a9"));
        }
    }

    /**
     * テキストボックスをクリアします。
     *
     * @param intKbn 選択ボタン種類
     */
    private void ClearTextData(int intKbn) {

        //選択されたボタンで処理を分ける
        switch (intKbn) {
            case FORM_REQUEST_LOCATION_L:   //大区分
                //中・小区分をクリア
                _LocMCode = "";
                _LocMName = "";
                _LocSCode = "";
                _LocSName = "";
                ((TextView) findViewById(R.id.txtInvSettingLocM)).setText("");
                ((TextView) findViewById(R.id.lblInvSettingLocMName)).setText("");
                ((TextView) findViewById(R.id.txtInvSettingLocS)).setText("");
                ((TextView) findViewById(R.id.lblInvSettingLocSName)).setText("");
                break;

            case FORM_REQUEST_LOCATION_M:   //中区分
                //小区分をクリア
                _LocSCode = "";
                _LocSName = "";
                ((TextView) findViewById(R.id.txtInvSettingLocS)).setText("");
                ((TextView) findViewById(R.id.lblInvSettingLocSName)).setText("");
                break;

            case FORM_REQUEST_ALL:          //全てクリア
                //大・中・小区分をクリア
                _LocLCode = "";
                _LocLName = "";
                _LocMCode = "";
                _LocMName = "";
                _LocSCode = "";
                _LocSName = "";
                ((TextView) findViewById(R.id.txtInvSettingLocL)).setText("");
                ((TextView) findViewById(R.id.lblInvSettingLocLName)).setText("");
                ((TextView) findViewById(R.id.txtInvSettingLocM)).setText("");
                ((TextView) findViewById(R.id.lblInvSettingLocMName)).setText("");
                ((TextView) findViewById(R.id.txtInvSettingLocS)).setText("");
                ((TextView) findViewById(R.id.lblInvSettingLocSName)).setText("");
                break;
        }
    }

    private void DisabledNextButton() {
        ((TextView) findViewById(R.id.btnSettingNext)).setEnabled(false);     //次へボタン
        ((Button) findViewById(R.id.btnSettingNext)).setBackground(getDrawable(R.drawable.execute_button_diable));
        ((Button) findViewById(R.id.btnSettingNext)).setTextColor(Color.parseColor("#a9a9a9"));
    }

    protected abstract Intent onNextPage();
    protected abstract Intent onReturnPage();
    protected abstract String[] GetLocationUsage(SQLiteDatabase db);

    protected void OpenMainActivity() {
    }

    protected void RFIDDataReceived(String strReadData, short tempRSSI, final String strPcHex) {
    }

}
