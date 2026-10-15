package com.rfid.setting;

import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.LinearLayout;
import android.widget.Spinner;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseActivity;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonInputPasswordDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfidpk.R;

import java.util.Objects;

//20210918 CHG Takase V2.0
//public class SystemSett extends BaseActivity {
public class SystemSett extends BaseRFIDActivity implements CommonInputPasswordDialog.Callback, CommonConfirmDialog.Callback {

    //20210918 ADD Takase V2.0
    private final static int FORM_REQUEST_PASSWORD = 1;

    //20220316 ADD Suzuki
    private boolean flgPw = false;

    //ダイアログキー
    private final static int RFIDReader_USE = 0;

    //RFIDリーダの使用有無の初期値
    private boolean isInitReaderUse = false;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_system_sett);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //コンボボックスのデータ設定
            SetComboDataIntencity();

            //AutoLinkProfileの設定
            SetAutoLinkProfile();

            //照合作業の設定
            SetCollation();

            //文言設定
            InitializeWord();

            //RFIDリーダの使用有無の初期値を保存
            isInitReaderUse = Objects.equals(C_Setting.getRfidReaderuseSetting(this), "1");

            //RFIDReader使用有無の設定チェックイベント
            CheckBox chkRFIDReaderUse = (CheckBox) findViewById(R.id.chkRFIDReaderUse);
            chkRFIDReaderUse.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
                @Override
                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                    Spinner _CmbValue = null;

                    //他のボタンの活性非活性の設定
                    setReaderOrAndroid(isChecked);
                    _CmbValue = findViewById(R.id.cmbSearch);   //電波強度(探索)
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbLease);    //電波強度(貸出)
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbReturn);   //電波強度(返却)
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbStorage);  //電波強度(保管)
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbDispose);  //電波強度(除却)
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbTaglink);  //電波強度(タグ紐付)
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbMatIn);    //電波強度(持込照合)
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbMatOut);   //電波強度(持帰照合)
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbMatTemp);  //電波強度(仮置登録)
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbMatRet);   //電波強度(照合返却)
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbInventoryL);   //電波強度(棚卸) 強
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbInventoryM);   //電波強度(棚卸) 中
                    _CmbValue.setEnabled(isChecked);
                    _CmbValue = findViewById(R.id.cmbInventoryS);   //電波強度(棚卸) 弱
                    _CmbValue.setEnabled(isChecked);
                    CheckBox chkAutoLink = findViewById(R.id.chkAutoLinkProfile);   //AutoLinkProfile
                    chkAutoLink.setEnabled(isChecked);
                }
            });

            //電波強度(探索) 選択イベント
            ((Spinner)findViewById(R.id.cmbSearch)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(貸出) 選択イベント
            ((Spinner)findViewById(R.id.cmbLease)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(返却) 選択イベント
            ((Spinner)findViewById(R.id.cmbReturn)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(保管) 選択イベント
            ((Spinner)findViewById(R.id.cmbStorage)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(除却) 選択イベント
            ((Spinner)findViewById(R.id.cmbDispose)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(タグ紐付) 選択イベント
            ((Spinner)findViewById(R.id.cmbTaglink)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(持込照合) 選択イベント
            ((Spinner)findViewById(R.id.cmbMatIn)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(持帰照合) 選択イベント
            ((Spinner)findViewById(R.id.cmbMatOut)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(仮置登録) 選択イベント
            ((Spinner)findViewById(R.id.cmbMatTemp)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(照合返却) 選択イベント
            ((Spinner)findViewById(R.id.cmbMatRet)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

//20191120 Add Start Sasaki
            //電波強度(棚卸 強) 選択イベント
            ((Spinner)findViewById(R.id.cmbInventoryL)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(棚卸 中) 選択イベント
            ((Spinner)findViewById(R.id.cmbInventoryM)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //電波強度(棚卸 弱) 選択イベント
            ((Spinner)findViewById(R.id.cmbInventoryS)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });
//20191120 Add End Sasaki

            //RFIDReader 選択イベント
            ((Spinner)findViewById(R.id.cmbRFIDReader)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {

                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });

            //cmbCollation 選択イベント
            ((Spinner)findViewById(R.id.cmbCollation)).setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {

                @Override
                public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
                @Override
                public void onNothingSelected(AdapterView<?> adapterView) { }
            });


            final SystemSett activity = this;
            //詳細ボタン Clickイベント
            findViewById(R.id.btnSettingSub).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DESCRIPTION, "", activity), activity);

                    //詳細設定画面
                    Intent intent = new Intent(getApplication(), SystemSettSub.class);
                    startActivity(intent);
                }
            });

            //20210918 ADD Takase V2.0 START
            //拠点設定ボタン Clickイベント
            findViewById(R.id.btnSettingHub).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    flgPw = true;               //20220316 ADD Suzuki
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_SET_HUB, "", activity), activity);

                    CommonInputPasswordDialog.Builder dialog
                            = new CommonInputPasswordDialog.Builder(activity);
                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, FORM_REQUEST_PASSWORD);
                    dialog.title(CommonConst.DIALOG_PASSWORD_TITLE);  //パスワード
                    dialog.message(SystemSetting.getString(
                            CommonConst.VIEW_WORD_ALL_INPUT_PASSWORD, "", activity));
                    dialog.params(bundle);
                    dialog.show();
                }
            });
            //20210918 ADD Takase V2.0 END
            //20220317 ADD Suzuki V2.1 START
            CheckBox chkAutoLink = (CheckBox) findViewById(R.id.chkAutoLinkProfile);
            chkAutoLink.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
                @Override
                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                    //設定値保存
                    //※保存タイミング変更
                    //setValueDatas();
                }
            });
            //20220317 ADD Suzuki V2.1 END
        }catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    //メニュー選択イベント
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            switch(item.getItemId()) {
                case android.R.id.home://戻るボタン
                    CheckBox chkRFIDReaderUse = findViewById(R.id.chkRFIDReaderUse);
                    onShutDownDialogueOrFinish(chkRFIDReaderUse.isChecked());
                    //this.finish();
                    return true;
            }
            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            return false;
        }
    }

    @Override
    protected String GetSettingKey() {
        return null;
    }

    // レジュームイベント
    @Override
    public void onResume() {
        try {
            super.onResume();

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //設定値取得
            getValueDatas();
        }
        catch (Exception ex) {
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    // オンポーズイベント
    @Override
    protected void onPause() {
        try {
        super.onPause();
            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //設定値保存
            setValueDatas();

        }
        catch (Exception ex) {
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    // 設定値取得
    private void getValueDatas() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            SettingValue C_Setting = new SettingValue();
            String _Intencity = "30";
            Spinner SpinnerData = null;
            int intPosition = 0;

            //RFIDReader使用有無設定
            CheckBox chkRFIDReaderUse = findViewById(R.id.chkRFIDReaderUse);
            boolean isRFIDReaderUse = Objects.equals(C_Setting.getRfidReaderuseSetting(this), "1");
            boolean isKeyence = CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString().equals(Common.ConnectRFIDName); //キーエンス DX-A600用のアプリの場合は非活性
            chkRFIDReaderUse.setChecked(isRFIDReaderUse);
            chkRFIDReaderUse.setEnabled(true);
            if (isKeyence)
            {
                chkRFIDReaderUse.setEnabled(false);
                chkRFIDReaderUse.setChecked(true);
            } else {
                if (Objects.equals(C_Setting.getLoginLocRfidFlg(this), "N"))
                {
                    chkRFIDReaderUse.setEnabled(false);
                    chkRFIDReaderUse.setChecked(false);
                }
            }

            //ログイン社員継続有無設定
            CheckBox chkEmployeePrevSet = findViewById(R.id.chkEmployeePrevSet);
            boolean isEmployeePrevSet = Boolean.valueOf(C_Setting.getEmployeePrevSet(this));
            chkEmployeePrevSet.setChecked(isEmployeePrevSet);

            //リーダー⇔Android設定
            SpinnerData = findViewById(R.id.cmbRFIDReader);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(C_Setting.getReaderChange(this));
            SpinnerData.setSelection(intPosition);

            //20230221 ADD Sato V2.3 Start
            //キーエンス製DXシリーズ、iData、RS35の場合は非活性  20230410 UPD Sato v2.3.1 RS35を追加
            if (Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString()) ||
                    Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.IDATA.getString()) ||
                    Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString())) {
                SpinnerData.setEnabled(false);
            }
            //20230221 ADD Sato V2.3 End

            //管理⇔一般設定
            SpinnerData = findViewById(R.id.cmbCollation);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(C_Setting.getCollationOperation(this));
            SpinnerData.setSelection(intPosition);

            //電波強度(探索)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_SEARCH);
            SpinnerData = findViewById(R.id.cmbSearch);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(貸出)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_LEASE);
            SpinnerData = findViewById(R.id.cmbLease);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(返却)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_RETURN);
            SpinnerData = findViewById(R.id.cmbReturn);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(保管)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_STORAGE);
            SpinnerData = findViewById(R.id.cmbStorage);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(除却)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_DISPOSE);
            SpinnerData = findViewById(R.id.cmbDispose);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(タグ紐付)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_TAGLINK);
            SpinnerData = findViewById(R.id.cmbTaglink);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(持込照合)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_MATIN);
            SpinnerData = findViewById(R.id.cmbMatIn);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(持帰照合)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_MATOUT);
            SpinnerData = findViewById(R.id.cmbMatOut);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(仮置登録)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_MATTEMP);
            SpinnerData = findViewById(R.id.cmbMatTemp);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(照合返却)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_MATRET);
            SpinnerData = findViewById(R.id.cmbMatRet);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

//20191120 Add Start Sasaki
            //電波強度(棚卸 強)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_L);
            SpinnerData = findViewById(R.id.cmbInventoryL);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(棚卸 中)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_M);
            SpinnerData = findViewById(R.id.cmbInventoryM);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);

            //電波強度(棚卸 弱)
            _Intencity = C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_S);
            SpinnerData = findViewById(R.id.cmbInventoryS);
            intPosition = ((ArrayAdapter) SpinnerData.getAdapter()).getPosition(_Intencity);
            SpinnerData.setSelection(intPosition);
//20191120 Add End Sasaki

            //20220316 ADD Suzuki V2.1 START
            //AutoLinkProfile設定
            if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
                //DENSOの場合
                CheckBox chkAutoLink = findViewById(R.id.chkAutoLinkProfile);
                Boolean blnAutoLink = Boolean.valueOf(C_Setting.getBTAutoLinkProfile( this, C_Setting.KEY_BT_READ_AUTO_LINK));
                chkAutoLink.setChecked(blnAutoLink);
            }
            //20220316 ADD Suzuki V2.1 END

        } catch (Exception ex) {
            ex.printStackTrace();
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * 電波強度のコンボボックス設定
     */
    private void SetComboDataIntencity() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //コンボボックスの数値を設定する
        String[] spinnerItems = SetIntenciryComboData();

        // ArrayAdapter
        ArrayAdapter<String> adapter
                = new ArrayAdapter<>(this,
                android.R.layout.simple_spinner_item, spinnerItems);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);

        Spinner spinner = null;

        //電波強度コンボ(探索)
        spinner = findViewById(R.id.cmbSearch);
        spinner.setAdapter(adapter);

        //電波強度コンボ(貸出)
        spinner = findViewById(R.id.cmbLease);
        spinner.setAdapter(adapter);

        //電波強度コンボ(返却)
        spinner = findViewById(R.id.cmbReturn);
        spinner.setAdapter(adapter);

        //電波強度コンボ(保管)
        spinner = findViewById(R.id.cmbStorage);
        spinner.setAdapter(adapter);

        //電波強度コンボ(除却)
        spinner = findViewById(R.id.cmbDispose);
        spinner.setAdapter(adapter);

        //電波強度コンボ(タグ紐付)
        spinner = findViewById(R.id.cmbTaglink);
        spinner.setAdapter(adapter);

        //電波強度コンボ(持込照合)
        spinner = findViewById(R.id.cmbMatIn);
        spinner.setAdapter(adapter);

        //電波強度コンボ(持帰照合)
        spinner = findViewById(R.id.cmbMatOut);
        spinner.setAdapter(adapter);

        //電波強度コンボ(仮置登録)
        spinner = findViewById(R.id.cmbMatTemp);
        spinner.setAdapter(adapter);

        //電波強度コンボ(照合返却)
        spinner = findViewById(R.id.cmbMatRet);
        spinner.setAdapter(adapter);


//20191120 Add Start Sasaki
        //電波強度コンボ(棚卸 強)
        spinner = findViewById(R.id.cmbInventoryL);
        spinner.setAdapter(adapter);

        //電波強度コンボ(棚卸 中)
        spinner = findViewById(R.id.cmbInventoryM);
        spinner.setAdapter(adapter);

        //電波強度コンボ(棚卸 弱)
        spinner = findViewById(R.id.cmbInventoryS);
        spinner.setAdapter(adapter);
//20191120 Add End Sasaki

        // ArrayAdapter
        ArrayAdapter<String> adapterReader
                = new ArrayAdapter<>(this,
                android.R.layout.simple_spinner_item, new String[]{CommonConst.READER_SETTING.Reader.getString(), CommonConst.READER_SETTING.Android.getString()});
        adapterReader.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);

        //RFIDReader⇔QR切替
        spinner = findViewById(R.id.cmbRFIDReader);
        spinner.setAdapter(adapterReader);

        //設定マスタがQRモードの場合、非活性にする
        SQLiteDatabase db = null;

        //コンボボックスの数値を設定する
        spinnerItems = SetLoginTimeoutComboData();

        // ArrayAdapter
        ArrayAdapter<String> adapterLoginTimeout
                = new ArrayAdapter<>(this,
                android.R.layout.simple_spinner_item, spinnerItems);
        adapterLoginTimeout.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);

        //操作タイムアウト(分)
        spinner = findViewById(R.id.cmbLoginTimeout);
        spinner.setAdapter(adapterLoginTimeout);
        int position = adapterLoginTimeout.getPosition(C_Setting.getLoginTimeoutSetting(this)); // インデックスを取得
        spinner.setSelection(position); // 選択状態に設定

        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            //読取媒体の設定により、初期値を変更する
            if (CommonConst.READING_MEDIUM.QR.getString().equals(SettingFactory.GetReadingMedium(db, this))) {
                findViewById(R.id.cmbRFIDReader).setEnabled(false);
                findViewById(R.id.cmbSearch).setEnabled(false);
                findViewById(R.id.cmbLease).setEnabled(false);
                findViewById(R.id.cmbReturn).setEnabled(false);
                findViewById(R.id.cmbStorage).setEnabled(false);
                findViewById(R.id.cmbDispose).setEnabled(false);
                findViewById(R.id.cmbTaglink).setEnabled(false);
                findViewById(R.id.cmbMatIn).setEnabled(false);
                findViewById(R.id.cmbMatOut).setEnabled(false);
                findViewById(R.id.cmbMatTemp).setEnabled(false);
                findViewById(R.id.cmbMatRet).setEnabled(false);
                findViewById(R.id.cmbInventoryL).setEnabled(false);
                findViewById(R.id.cmbInventoryM).setEnabled(false);
                findViewById(R.id.cmbInventoryS).setEnabled(false);
                findViewById(R.id.chkAutoLinkProfile).setEnabled(false);                 //20220316 ADD Suzuki V2.1
            }

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    //20220316 ADD Suzuki V2.1 START
    /**
     * AutoLinkProfile設定
     * @return
     */
    private void SetAutoLinkProfile() {
        LinearLayout layoutAutoLink = findViewById(R.id.layoutAutoLink);
        View view9 = findViewById(R.id.view9);
        if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
            //DENSOの場合
            layoutAutoLink.setVisibility(View.VISIBLE);
            view9.setVisibility(View.VISIBLE);
        } else {
            //ZEBRAの場合
            layoutAutoLink.setVisibility(View.GONE);
            view9.setVisibility(View.GONE);
        }
    }
    //20220316 ADD Suzuki V2.1 END

    /**
     * 照合機能の設定
     * @return
     */
    private void SetCollation() {
        SQLiteDatabase db = null;

        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            //照合機能がYの場合
            if(SettingFactory.GetMatchingFunction(db).equals(CommonConst.MATCHING_FUNCTION.YES.getString())){
                findViewById(R.id.cmbCollation).setEnabled(true);
            }else {
                findViewById(R.id.cmbCollation).setEnabled(false);
            }
        }finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 電波強度コンボボックス設定
     * @return
     */
    private String[] SetIntenciryComboData() {
        String[] comboItems = null;
        //20230221 UPD Sato v2.3 キーエンス製DXシリーズの場合を追加
        if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName) ||
                CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString().equals(Common.ConnectRFIDName)) {
            //DENSOもしくはKEYENCEのDXシリーズの場合
            comboItems = new String[27];
            for (int i = 0; i < 27; i++) {
                comboItems[i] = String.valueOf(i + 4);
            }
        } else if (CommonConst.CONNECT_RFIDNAME.IDATA.getString().equals(Common.ConnectRFIDName) ||
                  CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString().equals(Common.ConnectRFIDName)) {
            //IDATAの場合
            comboItems = new String[26];
            for (int i = 0; i <= 25; i++) {
                comboItems[i] = String.valueOf(i+5);
            }
        } else {
            //ZEBRAの場合
            comboItems = SetIntensityRange();
        }
        return comboItems;
    }

    /***
     * 操作タイムアウト(分)コンボボックス設定
     * @return
     */
    private String[] SetLoginTimeoutComboData() {
        String[] comboItems = new String[7];
        for (int i = 0; i <= 6; i++) {
            comboItems[i] = String.valueOf(i*10);
        }
        return comboItems;
    }

    /**
     * 設定値保存
     * @return
     */
    private  Boolean setValueDatas()  {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            SettingValue C_Setting = new SettingValue();
            Spinner _CmbValue = null;

            //RFIDReader使用有無の設定
            CheckBox chkRFIDReaderUse = findViewById(R.id.chkRFIDReaderUse);

            //管理⇔一般設定
            _CmbValue = findViewById(R.id.cmbCollation);
            C_Setting.setCollationOperation(this, _CmbValue.getSelectedItem().toString());

            //EmployeePrevSet使用有無の設定
            CheckBox chkEmployeePrevSet = findViewById(R.id.chkEmployeePrevSet);

            //チェックボックスのデータをセットする
            C_Setting.setRfidReaderuseSetting(this, chkRFIDReaderUse.isChecked() ? "1" : "2");

            //ログイン社員継続チェックボックスのデータをセットする
            C_Setting.setEmployeePrevSet(this, String.valueOf(chkEmployeePrevSet.isChecked()));

            //リーダー⇔Android設定
            _CmbValue = findViewById(R.id.cmbRFIDReader);
            C_Setting.setReaderChange(this, _CmbValue.getSelectedItem().toString());

            //電波強度(探索)
            _CmbValue = findViewById(R.id.cmbSearch);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_SEARCH, _CmbValue.getSelectedItem().toString());

            //電波強度(貸出)
            _CmbValue = findViewById(R.id.cmbLease);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_LEASE, _CmbValue.getSelectedItem().toString());

            //電波強度(返却)
            _CmbValue = findViewById(R.id.cmbReturn);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_RETURN, _CmbValue.getSelectedItem().toString());

            //電波強度(保管)
            _CmbValue = findViewById(R.id.cmbStorage);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_STORAGE, _CmbValue.getSelectedItem().toString());

            //電波強度(除却)
            _CmbValue = findViewById(R.id.cmbDispose);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_DISPOSE, _CmbValue.getSelectedItem().toString());

            //電波強度(タグ紐付)
            _CmbValue = findViewById(R.id.cmbTaglink);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_TAGLINK, _CmbValue.getSelectedItem().toString());

            //電波強度(持込照合)
            _CmbValue = findViewById(R.id.cmbMatIn);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_MATIN, _CmbValue.getSelectedItem().toString());

            //電波強度(持帰照合)
            _CmbValue = findViewById(R.id.cmbMatOut);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_MATOUT, _CmbValue.getSelectedItem().toString());

            //電波強度(仮置登録)
            _CmbValue = findViewById(R.id.cmbMatTemp);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_MATTEMP, _CmbValue.getSelectedItem().toString());

            //電波強度(照合返却)
            _CmbValue = findViewById(R.id.cmbMatRet);
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_MATRET, _CmbValue.getSelectedItem().toString());

            //電波強度(棚卸)設定
            SettingIntensity();         //20191120 Add Sasaki

            //20220316 ADD Suzuki V2.1 START
            //AutoLinkProfile設定
            if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
                //DENSOの場合
                CheckBox chkAutoLink = findViewById(R.id.chkAutoLinkProfile);
                C_Setting.setBTAutoLinkProfile( this, C_Setting.KEY_BT_READ_AUTO_LINK, String.valueOf(chkAutoLink.isChecked()));
            }
            //20220316 ADD Suzuki V2.1 END

            //操作タイムアウト時間
            _CmbValue = findViewById(R.id.cmbLoginTimeout);
            C_Setting.setLoginTimeoutSetting( this, _CmbValue.getSelectedItem().toString());

            return true;
        } catch (Exception ex) {
            return false;
        }
    }

    /**
     * 戻るボタンを押したときRFIDリーダの使用の設定が変更されている場合ダイアログを表示するための関数
     */
    protected void onShutDownDialogueOrFinish(boolean isChecked) {

        if (isChecked != isInitReaderUse) {
            //設定値保存
            setValueDatas();

            final CommonConfirmDialog.Builder dialog = new CommonConfirmDialog.Builder(this);
            Bundle bundle = new Bundle();
            bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, RFIDReader_USE);
            dialog.title(CommonConst.DIALOG_RESTART_TITLE);  //シャットダウン
            dialog.message(SystemSetting.getString(
                    CommonConst.VIEW_WORD_ALL_RESTART_APPLICATION, "", this));   //RFIDReader使用の設定が変更されました。アプリを終了します。よろしいですか？
            dialog.params(bundle);
            dialog.show();
        } else {
            this.finish();
        }
    }

    /**
     * RFIDリーダの使用有無の設定の値によってQR読取機器の選択を制御する
     * @param RFIDReaderUse RFIDリーダの使用有無の設定の値
     */
    private void setReaderOrAndroid(boolean RFIDReaderUse)  {

        SettingValue C_Setting = new SettingValue();
        Spinner _CmbValue = findViewById(R.id.cmbRFIDReader);

        if (RFIDReaderUse) {
            _CmbValue.setEnabled(true);
            //リーダーを使う場合RFIDReaderを選択
            _CmbValue.setSelection(CommonConst.READER_SETTING.Reader.ordinal());
        } else {
            //リーダーを使わない場合Androidを選択
            _CmbValue.setSelection(CommonConst.READER_SETTING.Android.ordinal());
            _CmbValue.setEnabled(false);
        }
    }

    /**
     * メニューの設定処理
     * @param menu
     * @return
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //getMenuInflater().inflate(R.menu.menu_nosetting, menu);       //20191126 Del Sasaki
        getMenuInflater().inflate(R.menu.menu_setting, menu);         //20191126 Add Sasaki
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
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SETTEI,"",this));                        //タイトル
        ((TextView)findViewById(R.id.labSearch))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", this) + ")");                    //電波強度(探索)
        ((TextView)findViewById(R.id.labLease))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING, "", this) + ")");                    //電波強度(貸出)
        ((TextView)findViewById(R.id.labReturn))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_RETURN, "", this) + ")");                    //電波強度(返却)
        ((TextView)findViewById(R.id.labStorage))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_STORE, "", this) + ")");                    //電波強度(保管)
        ((TextView)findViewById(R.id.labDispose))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_DELETE, "", this) + ")");                    //電波強度(除却)
        ((TextView)findViewById(R.id.labTaglink))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG, "", this) + ")");                       //電波強度(タグ紐付)
        ((TextView)findViewById(R.id.labMatIn))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHIN, "", this) + ")");                  //電波強度(持込照合)
        ((TextView)findViewById(R.id.labMatOut))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHOUT, "", this) + ")");                 //電波強度(持帰照合)
        ((TextView)findViewById(R.id.labMatTemp))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST, "", this) + ")");          //電波強度(仮置登録)
        ((TextView)findViewById(R.id.labMatRet))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHRETURN, "", this) + ")");              //電波強度(照合返却)

        ((TextView)findViewById(R.id.labInventoryL))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", this) + ")");                //電波強度(棚卸)
        ((TextView)findViewById(R.id.labInventoryKbnL))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA_L, "", this));                         //電波強度(棚卸) 強
        ((TextView)findViewById(R.id.labInventoryKbnM))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA_M, "", this));                         //電波強度(棚卸) 中
        ((TextView)findViewById(R.id.labInventoryKbnS))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA_S, "", this));                          //電波強度(棚卸) 弱
        ((TextView)findViewById(R.id.btnSettingSub))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DESCRIPTION, "", this) );                  //詳細設定
        ((TextView)findViewById(R.id.btnSettingHub))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_SET_HUB, "", this) );                      //拠点設定　            //20210918 ADD Takase V2.0
        //20220316 ADD Suzuki V2.1 START
        if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
            //DENSOの場合
            ((CheckBox)findViewById(R.id.chkAutoLinkProfile))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_VALID, "", this));                    //AutoLinkProfileのチェックボックス
        }
        //20220316 ADD Suzuki V2.1 END
        ((CheckBox)findViewById(R.id.chkRFIDReaderUse))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_RFIDREADER_USE_VALUE, "", this));
        ((CheckBox)findViewById(R.id.chkEmployeePrevSet))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_RFIDREADER_USE_VALUE, "", this));
        ((TextView)findViewById(R.id.labsetReader))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_QRREADER, "", this));
        ((TextView)findViewById(R.id.labRFIDReader))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_RFIDREADER, "", this));

        ((TextView)findViewById(R.id.loginTimeout))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_LOGIN_TIMEOUT, "", this));
        ((TextView)findViewById(R.id.loginTimeoutMessage))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_LOGIN_TIMEOUT_MESSAGE, "", this));

        //20241016 ADD Shirakawa v4.0
        ((TextView)findViewById(R.id.labsetCollation))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_COLLATIONWORK, "", this));
        ((TextView)findViewById(R.id.labEmployeePrevSet))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LOGIN_EMPLOYEE_CONTINUE, "", this));
    }

//20191120 Add Start Sasaki
    /**
     * 棚卸の電波強度に関する設定
     */
    private void SettingIntensity() {

        //電波強度(棚卸 強)
        String strIntensityValueL = ((Spinner)findViewById(R.id.cmbInventoryL)).getSelectedItem().toString();
        //電波強度(棚卸 中)
        String strIntensityValueM = ((Spinner)findViewById(R.id.cmbInventoryM)).getSelectedItem().toString();
        //電波強度(棚卸 弱)
        String strIntensityValueS = ((Spinner)findViewById(R.id.cmbInventoryS)).getSelectedItem().toString();

        //強中弱のチェック処理を行う
        if (!CheckIntensity( Integer.parseInt(strIntensityValueL)
                            ,  Integer.parseInt(strIntensityValueM)
                            ,  Integer.parseInt(strIntensityValueS))) {
            //エラーの場合メッセージを表示する
            ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_INTENCITY_VALUE, "", this),
                            SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SETTEI, "", this));

            int intPositionL = ((ArrayAdapter) ((Spinner)findViewById(R.id.cmbInventoryL)).getAdapter()).getPosition(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_L));
            ((Spinner)findViewById(R.id.cmbInventoryL)).setSelection(intPositionL);
            int intPositionM = ((ArrayAdapter) ((Spinner)findViewById(R.id.cmbInventoryM)).getAdapter()).getPosition(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_M));
            ((Spinner)findViewById(R.id.cmbInventoryM)).setSelection(intPositionM);
            int intPositionS = ((ArrayAdapter) ((Spinner)findViewById(R.id.cmbInventoryS)).getAdapter()).getPosition(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_INVENTORY_S));
            ((Spinner)findViewById(R.id.cmbInventoryS)).setSelection(intPositionS);
            return;
        }

        //電波強度を設定する
        C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY_L, strIntensityValueL);        //(棚卸 強)
        C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY_M, strIntensityValueM);        //(棚卸 中)
        C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY_S, strIntensityValueS);        //(棚卸 弱)

        //棚卸画面で設定されている区分を取得する
        String strIntensityKbun = C_Setting.getBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY_KBN);
        if (C_Setting.INVENTORY_VALUE_L.equals(strIntensityKbun)) {
            //強の場合
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY, strIntensityValueL);
        } else if (C_Setting.INVENTORY_VALUE_M.equals(strIntensityKbun)) {
            //中の場合
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY, strIntensityValueM);
        } else {
            //弱の場合
            C_Setting.setBTReadIntensity( this, C_Setting.KEY_EACH_INVENTORY, strIntensityValueS);
        }

    }

    /**
     * 電波強度の設定をチェックする
     * @param IntensityValueL
     * @param IntensityValueM
     * @param IntensityValueS
     * @return
     */
    private boolean CheckIntensity(int IntensityValueL, int IntensityValueM, int IntensityValueS) {
        boolean blnRetrun = false;

        //弱 <= 中 <= 強の場合、OK
        if (IntensityValueS <= IntensityValueM && IntensityValueM <= IntensityValueL) {
            blnRetrun = true;
        }
        return blnRetrun;
    }
//20191120 Add End Sasaki

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    //20210918 ADD Takase V2.0 START
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        super.onMyDialogSucceeded(requestCode, params);

        if (params == null) {
            return;
        }

        //呼出元で分岐する
        switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
            case RFIDReader_USE://戻るボタンを押したときにRFIDリーダの使用のチェックを変更されている場合
                //アプリ終了
                allActivityFinish();
                break;
            case FORM_REQUEST_PASSWORD:     //拠点設定ボタン
                //20220316 UPD Suzuki
                if(flgPw){
                    flgPw = false;
                    //対象拠点設定画面
                    Intent intent = new Intent(getApplication(), SystemSettHub.class);
                    startActivity(intent);
                    break;
                }

        }


    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {flgPw = false;}
    //20210918 ADD Takase V2.0 END

    // BaseRFIDActivityのabstractイベントやメソッド
    // ※本画面では使用しない
    @Override
    protected void OpenMainActivity() {}
    @Override
    protected void BarcodeDataReceived(String strReadData) {}
    @Override
    protected void RFIDDataReceived(String strReadData, short tempRSSI, String strPcHex) {}
    @Override
    protected void ReaderOn() {}
    @Override
    protected void ReaderOff() {}
}
