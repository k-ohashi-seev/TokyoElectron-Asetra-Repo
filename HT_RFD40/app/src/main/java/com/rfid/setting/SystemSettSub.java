package com.rfid.setting;

import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Spinner;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfidpk.R;

import java.util.Objects;

public class SystemSettSub extends BaseActivity {

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_system_sett_sub);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //文言設定
            InitializeWord();

            //機種により変更する
            InitializeActivity();

            //設定ボタン非表示
            getSupportActionBar().setDisplayShowCustomEnabled(false);

        } catch (Exception ex) {
            ShowErrorMessage(ex.getMessage(), "");
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
                case android.R.id.home:     //戻るボタン
                    //設定値保存
                    setValueDatas();
                    this.finish();
                    return true;
                case R.id.action_menu:      //メニューへボタン
                    //設定値保存
                    setValueDatas();
                    onMenuReturn();
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
                    //設定値保存
                    setValueDatas();
                    this.finish();
                    return true;
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

            //設定値取得
            getValueDatas();
        }
        catch (Exception ex) {
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    private void InitializeActivity() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //20191115 Add Start Sasaki
        //タイムアウトのコンボボックスを設定する
        String[] spinnerItems = SetTimeoutComboData();
        ArrayAdapter<String> adapter
                = new ArrayAdapter<>(this,
                android.R.layout.simple_spinner_item, spinnerItems);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        ((Spinner)findViewById(R.id.cmbSetTimeOut)).setAdapter(adapter);
        //20191115 Add End Sasaki

        //DENSOとRS35以外の場合、非表示にする項目 20230410 UPD Sato v2.3.1
//        if (CommonConst.CONNECT_RFIDNAME.ZEBRA.getString().equals(Common.ConnectRFIDName)) {
//        if (!CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
        if (!CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName) &&
            !CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString().equals(Common.ConnectRFIDName)) {
            //棚卸
            findViewById(R.id.labQValueInv).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueInv).setVisibility(View.INVISIBLE);
            //探索
            findViewById(R.id.labQValueSearch).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueSearch).setVisibility(View.INVISIBLE);
            //貸出
            findViewById(R.id.labQValueLease).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueLease).setVisibility(View.INVISIBLE);
            //返却
            findViewById(R.id.labQValueOtr).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueReturn).setVisibility(View.INVISIBLE);
            //保管
            findViewById(R.id.labQValueStorage).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueCmp).setVisibility(View.INVISIBLE);
            //除却
            findViewById(R.id.labQValueDispose).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueDispose).setVisibility(View.INVISIBLE);
            //紐付
            findViewById(R.id.labQValueTaglink).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueTaglink).setVisibility(View.INVISIBLE);

            //持込照合
            findViewById(R.id.labQValueMatIn).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueMatIn).setVisibility(View.INVISIBLE);
            //持帰照合
            findViewById(R.id.labQValueMatOut).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueMatOut).setVisibility(View.INVISIBLE);
            //仮置登録
            findViewById(R.id.labQValueMatTemp).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueMatTemp).setVisibility(View.INVISIBLE);
            //照合返却
            findViewById(R.id.labQValueMatRet).setVisibility(View.INVISIBLE);
            findViewById(R.id.cmbQValueMatRet).setVisibility(View.INVISIBLE);

        }

        //設定マスタがQRモードの場合、非活性にする
        SQLiteDatabase db = null;
        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //読取媒体の設定により、非活性にする
            if (CommonConst.READING_MEDIUM.QR.getString().equals(SettingFactory.GetReadingMedium(db, this))) {
                findViewById(R.id.cmbSesstionInv).setEnabled(false);
                findViewById(R.id.cmbSesstionSearch).setEnabled(false);
                findViewById(R.id.cmbSesstionLease).setEnabled(false);
                findViewById(R.id.cmbSesstionReturn).setEnabled(false);
                findViewById(R.id.cmbSesstionStorage).setEnabled(false);
                findViewById(R.id.cmbSesstionDispose).setEnabled(false);
                findViewById(R.id.cmbSesstionTaglink).setEnabled(false);
                findViewById(R.id.cmbSesstionMatIn).setEnabled(false);
                findViewById(R.id.cmbSesstionMatOut).setEnabled(false);
                findViewById(R.id.cmbSesstionMatTemp).setEnabled(false);
                findViewById(R.id.cmbSesstionMatRet).setEnabled(false);
                //DENSOもしくはRS35の場合　20230410 UPD Sato v2.3.1
                if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName) ||
                        CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString().equals(Common.ConnectRFIDName)) {
                    findViewById(R.id.cmbQValueInv).setEnabled(false);
                    findViewById(R.id.cmbQValueSearch).setEnabled(false);
                    findViewById(R.id.cmbQValueLease).setEnabled(false);
                    findViewById(R.id.cmbQValueReturn).setEnabled(false);
                    findViewById(R.id.cmbQValueCmp).setEnabled(false);
                    findViewById(R.id.cmbQValueDispose).setEnabled(false);
                    findViewById(R.id.cmbQValueTaglink).setEnabled(false);
                    findViewById(R.id.cmbQValueMatIn).setEnabled(false);
                    findViewById(R.id.cmbQValueMatOut).setEnabled(false);
                    findViewById(R.id.cmbQValueMatTemp).setEnabled(false);
                    findViewById(R.id.cmbQValueMatRet).setEnabled(false);
                }
            }

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    // 設定値取得
    private void getValueDatas() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {

            SettingValue C_Setting = new SettingValue();

            //サーバー接続ＩＰアドレス
            TextView _TxtServerIP = findViewById(R.id.TxtSetServerIP);
            _TxtServerIP.setText(C_Setting.getConnectServer(this));
            if (Objects.equals(C_Setting.getLoginHeadOffice(this), "N")) {
                _TxtServerIP.setEnabled(false);
                _TxtServerIP.setFocusable(false);
            } else {
                _TxtServerIP.setEnabled(true);
                _TxtServerIP.setFocusable(true);
            }

            //サーバー接続タイムアウト
            int intPosition = ((ArrayAdapter)((Spinner)findViewById(R.id.cmbSetTimeOut)).getAdapter()).getPosition(C_Setting.getConnectTimeOut(this));
            ((Spinner)findViewById(R.id.cmbSetTimeOut)).setSelection(intPosition);
            
            //20210918 ADD Takase V2.0 START
            // 送信方法
            intPosition = ((ArrayAdapter)((Spinner)findViewById(R.id.cmbSetSend)).getAdapter()).getPosition(C_Setting.getTransfer(this));
            ((Spinner)findViewById(R.id.cmbSetSend)).setSelection(intPosition);
            //20210918 ADD Takase V2.0 END

            int _Session = 0;
            int _Q_Value = 0;
            Spinner _CmbSesstion = null;
            Spinner _CmbQValue = null;
            //他の項目を活性非活性の切り替え設定
            boolean isRFIDReaderUse = Objects.equals(C_Setting.getRfidReaderUse(this), "1");

            //--- 電波設定(棚卸) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_INVENTORY ));
            _CmbSesstion = findViewById(R.id.cmbSesstionInv);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_INVENTORY));
            _CmbQValue = findViewById(R.id.cmbQValueInv);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

            //--- 電波設定(探索) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_SEARCH ));
            _CmbSesstion = findViewById(R.id.cmbSesstionSearch);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_SEARCH));
            _CmbQValue = findViewById(R.id.cmbQValueSearch);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

            //--- 電波設定(貸出) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_LEASE ));
            _CmbSesstion = findViewById(R.id.cmbSesstionLease);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_LEASE));
            _CmbQValue = findViewById(R.id.cmbQValueLease);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

            //--- 電波設定(返却) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_RETURN));
            _CmbSesstion = findViewById(R.id.cmbSesstionReturn);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_RETURN));
            _CmbQValue = findViewById(R.id.cmbQValueReturn);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

            //--- 電波設定(保管) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_STORAGE));
            _CmbSesstion = findViewById(R.id.cmbSesstionStorage);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_STORAGE));
            _CmbQValue = findViewById(R.id.cmbQValueCmp);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

            //--- 電波設定(除却) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_DISPOSE ));
            _CmbSesstion = findViewById(R.id.cmbSesstionDispose);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_DISPOSE));
            _CmbQValue = findViewById(R.id.cmbQValueDispose);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

            //--- 電波設定(タグ紐付) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_TAGLINK ));
            _CmbSesstion = findViewById(R.id.cmbSesstionTaglink);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_TAGLINK ));
            _CmbQValue = findViewById(R.id.cmbQValueTaglink);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

            //--- 電波設定(持込照合) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_MATIN));
            _CmbSesstion = findViewById(R.id.cmbSesstionMatIn);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_MATIN));
            _CmbQValue = findViewById(R.id.cmbQValueMatIn);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

            //--- 電波設定(持帰照合) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_MATOUT));
            _CmbSesstion = findViewById(R.id.cmbSesstionMatOut);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_MATOUT));
            _CmbQValue = findViewById(R.id.cmbQValueMatOut);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

            //--- 電波設定(仮置登録) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_MATTEMP));
            _CmbSesstion = findViewById(R.id.cmbSesstionMatTemp);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_MATTEMP));
            _CmbQValue = findViewById(R.id.cmbQValueMatTemp);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

            //--- 電波設定(照合返却) ---
            //セッション値
            _Session = Integer.parseInt(C_Setting.getBTReadSession(this, C_Setting.KEY_EACH_MATRET));
            _CmbSesstion = findViewById(R.id.cmbSesstionMatRet);
            _CmbSesstion.setEnabled(isRFIDReaderUse);
            _CmbSesstion.setSelection(_Session);

            //Ｑバリュー値
            _Q_Value = Integer.parseInt(C_Setting.getBTReadQValue(this, C_Setting .KEY_EACH_MATRET));
            _CmbQValue = findViewById(R.id.cmbQValueMatRet);
            _CmbQValue.setEnabled(isRFIDReaderUse);
            _CmbQValue.setSelection(_Q_Value);

        } catch (Exception ex) {
            ex.printStackTrace();
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    // 設定値保存
    private  Boolean setValueDatas()  {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            SettingValue C_Setting = new SettingValue();

            //サーバー接続ＩＰアドレス
            TextView _TxtServerIP = findViewById(R.id.TxtSetServerIP);
            String _tempValue = _TxtServerIP.getText().toString();
            _tempValue = _tempValue.trim();     //空白除去
            C_Setting.setConnectServer(this, _tempValue);
            
            //20210918 ADD Takase V2.0 START
            //送信方法
            _tempValue = ((Spinner)findViewById(R.id.cmbSetSend)).getSelectedItem().toString();
            C_Setting.setTransfer(this, _tempValue);
            //20210918 ADD Takase V2.0 END

            //サーバー接続タイムアウト
            _tempValue = ((Spinner)findViewById(R.id.cmbSetTimeOut)).getSelectedItem().toString();
            C_Setting.setConnectTimeOut(this, _tempValue);

            String _Session = "";
            String _Q_Value = "";
            Spinner _CmbSesstion = null;
            Spinner _CmbQValue = null;

            //--- 電波設定(棚卸) ---
            //セッション値
            _CmbSesstion = findViewById(R.id.cmbSesstionInv);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_INVENTORY, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueInv);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_INVENTORY, _Q_Value);

            //--- 電波設定(探索) ---
            //セッション値
            _CmbSesstion = findViewById(R.id.cmbSesstionSearch);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_SEARCH, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueSearch);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_SEARCH, _Q_Value);

            //--- 電波設定(貸出) ---
            //セッション値
            _CmbSesstion = findViewById(R.id.cmbSesstionLease);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_LEASE, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueLease);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_LEASE, _Q_Value);

            //--- 電波設定(返却) ---
            //セッション値
            _CmbSesstion= findViewById(R.id.cmbSesstionReturn);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_RETURN, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueReturn);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_RETURN, _Q_Value);

            //--- 電波設定(保管) ---
            //セッション値
            _CmbSesstion = findViewById(R.id.cmbSesstionStorage);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_STORAGE, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueCmp);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_STORAGE, _Q_Value);

            //--- 電波設定(除却) ---
            //セッション値
            _CmbSesstion = findViewById(R.id.cmbSesstionDispose);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_DISPOSE, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueDispose);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_DISPOSE, _Q_Value);

            //--- 電波設定(タグ紐付) ---
            //セッション値
            _CmbSesstion = findViewById(R.id.cmbSesstionTaglink);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_TAGLINK, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueTaglink);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_TAGLINK, _Q_Value);

            //--- 電波設定(持込照合) ---
            //セッション値
            _CmbSesstion = findViewById(R.id.cmbSesstionMatIn);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_MATIN, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueMatIn);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_MATIN, _Q_Value);

            //--- 電波設定(持帰照合) ---
            //セッション値
            _CmbSesstion = findViewById(R.id.cmbSesstionMatOut);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_MATOUT, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueMatOut);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_MATOUT, _Q_Value);

            //--- 電波設定(仮置登録) ---
            //セッション値
            _CmbSesstion = findViewById(R.id.cmbSesstionMatTemp);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_MATTEMP, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueMatTemp);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_MATTEMP, _Q_Value);

            //--- 電波設定(照合返却) ---
            //セッション値
            _CmbSesstion = findViewById(R.id.cmbSesstionMatRet);
            _Session  = _CmbSesstion.getSelectedItem().toString();
            _Session = _Session.substring(1);      //頭のＳ カット
            C_Setting.setBTReadSession(this, C_Setting.KEY_EACH_MATRET, _Session );

            //Ｑバリュー値
            _CmbQValue = findViewById(R.id.cmbQValueMatRet);
            _Q_Value  = _CmbQValue.getSelectedItem().toString();
            C_Setting.setBTReadQValue( this, C_Setting.KEY_EACH_MATRET, _Q_Value);

            return true;
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * タイムアウトコンボボックス設定
     * @return
     */
    // 20191115 Add Start Sasaki
    private String[] SetTimeoutComboData() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        String[] comboItems = null;

        comboItems = new String[7];
        //30秒～120秒まで設定する
        for (int i = 0; i < 7; i++) {
            comboItems[i] = String.valueOf(i * 15 + 30);
        }

        return comboItems;
    }
    // 20191115 Add End Sasaki

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
        //20210918 ADD Takase V2.0 START
        ((TextView)findViewById(R.id.labSetSend))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_TRANSFER, "", this));                   //HTデータ送受信
        //20210918 ADD Takase V2.0 END
        ((TextView)findViewById(R.id.labSetConnect))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_SERVER_CONNECT, "", this));             //サーバー接続
        ((TextView)findViewById(R.id.labSetTimeOut))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_TIME_OUT, "", this));                    //タイムアウト
        ((TextView)findViewById(R.id.labRfsetTitleSearch))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SEARCH, "", this) + ")");                    //電波設定(探索)
        ((TextView)findViewById(R.id.labSetTitleLease))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING, "", this) + ")");                    //電波設定(貸出)
        ((TextView)findViewById(R.id.labSetTitleReturn))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_RETURN, "", this) + ")");                    //電波設定(返却)
        ((TextView)findViewById(R.id.labRfsetTitleStorage))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_STORE, "", this) + ")");                    //電波設定(保管)
        ((TextView)findViewById(R.id.labRfsetTitleDispose))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_DELETE, "", this) + ")");                    //電波設定(除却)
        ((TextView)findViewById(R.id.labRfsetTitleTaglink))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_TAG, "", this) + ")");                       //電波設定(タグ紐付)
        ((TextView)findViewById(R.id.labRfsetTitleInv))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", this) + ")");                 //電波設定(棚卸)
        ((TextView)findViewById(R.id.labRfsetTitleMatIn))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHIN, "", this) + ")");                 //電波設定(持込照合)
        ((TextView)findViewById(R.id.labRfsetTitleMatOut))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHOUT, "", this) + ")");                 //電波設定(持帰照合)
        ((TextView)findViewById(R.id.labRfsetTitleMatTemp))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST, "", this) + ")");          //電波設定(仮置登録)
        ((TextView)findViewById(R.id.labRfsetTitleMatRet))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_DENPA, "", this) + "(" +
                        SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHRETURN, "", this) + ")");              //電波設定(照合返却)

    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }
}
