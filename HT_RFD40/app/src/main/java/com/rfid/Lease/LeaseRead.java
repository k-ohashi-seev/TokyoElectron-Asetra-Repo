package com.rfid.Lease;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.CompoundButton;
import android.widget.Switch;
import android.widget.TextView;

import com.rfid.Factory.EmployeeFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.ProcessFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseRead;
import com.rfid.common.Common;
import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.main.LoginMn;
import com.rfid.main.MainActivity;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;

/**
 * 貸出資産読取画面
 *
 */
public class LeaseRead extends BaseRead implements CommonConfirmDialog.Callback, CommonOKDialog.Callback {

    //システム設定値
    private SettingValue C_Setting = new SettingValue();

    //貸出先
    private String _LeaseCode = "";
    private String _LeaseName = "";

    //備考
    private String _Remark = "";

    //貸出担当
    private String _EmployeeCode = "";
    private String _EmployeeName = "";

    //返却予定日
    private String _ReturnDate = "";

    private TextView text = null;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {

        SQLiteDatabase db = null;    //20210918 ADD Takase V2.0 
        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //20210918 ADD Takase V2.0 START
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            if (SettingFactory.GetReservationFunction(db).equals((CommonConst.RESERVATION_FUNCTION.YES.getString()))) {
                //予約機能あり
                setContentView(R.layout.activity_read);
            } else {
                //予約機能なし
                setContentView(R.layout.activity_lease_read);
            }
            //20210918 ADD Takase V2.0 END

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            // プログレスダイアログの表示
            C_Progres.StartProgressDialog(this, "Loading...");

            //RFIDリーダーをOFFにする
            RFID_Read_Stop();

            //文言表示
            InitializeWord();

            //初期表示
            InitializeViewData();

            //ボタン非活性
            //SetButtonEnabled(false, false);   20200213 Del Sasaki
            AllResetButton(false);  //20200213 Add Sasaki

            //処理履歴登録用データ設定
            //20210918 CHG Takase V2.0 START
            //SetProcessData();
            if (SettingFactory.GetReservationFunction(db).equals((CommonConst.RESERVATION_FUNCTION.NO.getString()))) {
                SetProcessData();
            }
            //20210918 CHG Takase V2.0 END

            //20210918 ADD Takase V2.0 START
            //「貸出」設定
            _ProcessData.setIsLease(true);
            //20210918 ADD Takase V2.0 END

            //データ読み込み
            InsertData();

            _InitializedFlag = false;

            //タグ読取モード Clickイベント
            final Activity activity = this;
            //final BarcodeManager.BarcodeManagerListener listner = this;       //Android用
            ((Switch)findViewById(R.id.switchTagMode)).setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener(){
                @Override
                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_INV_TAG_MODE, "", activity), activity);

                    //タグ読取モード変更処理
                    onSwitchChanged(isChecked);
                }
            });

            //クリアボタン Clickイベント
            findViewById(R.id.btnClear).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    onClearButton();
                }
            });

            //タグ読取開始ボタン Clickイベント
            findViewById(R.id.btnTagReadStart).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", activity), activity);

                    //ボタン切替
                    if (_TagReadStartFlag){
                        //タグ読取開始している場合、収集終了処理
                        InventoryEndProcess();
                    } else {
                        //タグ読取を開始していない場合、収集開始処理
                        InventoryStartProcess();
                    }

                }
            });

            //詳細表示ボタン Clickイベント
            findViewById(R.id.btnDetail).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LIST, "", activity), activity);

                    //ボタン非活性
                    AllResetButton(false);
                    SetButtonEnabled(false, false);

                    ReaderOff();

                    //詳細表示画面へ
                    Intent intent = new Intent(getApplication(), LeaseSummary.class);
                    //intent.putExtra(CommonConst.VIEW_KEY_PROCESS_DATA, _ProcessData);
                    startActivityForResult(intent, BUTTON_SUMMARY);

                }
            });

            //サービス開始
            startService();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        } finally {
        	//20210918 ADD Takase V2.0 START
            if (db != null) {
            	db.close();
            }
            //20210918 ADD Takase V2.0 END
        }
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING,"",this));             //貸出タイトル
        ((TextView)findViewById(R.id.labReadCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_COUNT, "", this));       //件数
        ((TextView)findViewById(R.id.labIntencity))
            .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this));           //電波強度
        ((TextView)findViewById(R.id.lblTagModeTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_TAG_MODE, "", this));    //タグモード
        ((TextView)findViewById(R.id.labReadTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this));    //タグ読取
        ((TextView)findViewById(R.id.btnTagReadStart))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", this));       //タグ読取開始
        ((TextView)findViewById(R.id.btnClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));       //クリア
        ((TextView)findViewById(R.id.btnDetail))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LIST, "", this));         //一覧
        ((TextView)findViewById(R.id.btnTagReadStart))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", this));    //タグ読取開始
    }

    /**
     * 初期表示
     */
    private void InitializeViewData(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //20210918 CHG Takase V2.0 START
        SQLiteDatabase db = null;
        try {
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            if (SettingFactory.GetReservationFunction(db).equals((CommonConst.RESERVATION_FUNCTION.NO.getString()))) {
                //予約機能なしの場合

                Intent intent = getIntent();
                _LeaseCode = intent.getStringExtra(CommonConst.VIEW_KEY_LEASE_CD);
                _LeaseName = intent.getStringExtra(CommonConst.VIEW_KEY_LEASE_NAME);
                _EmployeeCode = intent.getStringExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD);
                _EmployeeName = intent.getStringExtra(CommonConst.VIEW_KEY_EMPLOYEE_NAME);
                _ReturnDate = intent.getStringExtra(CommonConst.VIEW_KEY_DATE);
                _Remark = intent.getStringExtra(CommonConst.VIEW_KEY_LEASE_REMARK);
                String loginEmpCode = SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD , "", this);                                                      //20231024 ADD Sato v2.4.1
                String loginEmpName = EmployeeFactory.GetEmployeeName(db, loginEmpCode).trim();                                                                                  //20231024 ADD Sato v2.4.1

                ((TextView)findViewById(R.id.txtLoginEmp)).setText(Common.substring(loginEmpCode + " " + loginEmpName, CommonConst.VIEW_LEASE_NAME_SIZE));                //20231024 ADD Sato v2.4.1
                ((TextView)findViewById(R.id.txtLeaseDestination)).setText(Common.substring(_LeaseCode + " " + _LeaseName, CommonConst.VIEW_LEASE_NAME_SIZE));
                ((TextView)findViewById(R.id.txtLeaseBiko)).setText(Common.substring(_Remark, CommonConst.VIEW_LEASE_NAME_SIZE));
                ((TextView)findViewById(R.id.txtLeaseResponsible)).setText(Common.substring(_EmployeeCode + " " + _EmployeeName, CommonConst.VIEW_LEASE_NAME_SIZE));
                ((TextView)findViewById(R.id.txtLeaseReturn)).setText(_ReturnDate);

                //20231024 ADD Sato v2.4.1 START
                //貸出情報表示設定を取得
                String dspInfo = SettingFactory.GetLendingDspInfo(db);

                if (dspInfo.equals(CommonConst.LEASE_DSP_INFO.LOGIN_INFO.getString())) {
                    //ログイン情報
                    findViewById(R.id.linearBiko).setVisibility(View.GONE);
                    findViewById(R.id.linearLeaseResponsible).setVisibility(View.GONE);
                    findViewById(R.id.linearLeaseReturn).setVisibility(View.GONE);
                }
                else if (dspInfo.equals(CommonConst.LEASE_DSP_INFO.LENDING_INFO.getString())) {
                    //貸出情報
                    findViewById(R.id.linearLoginInfo).setVisibility(View.GONE);
                    findViewById(R.id.linearBiko).setVisibility(View.GONE);
                } else {
                    //何も表示しない
                    findViewById(R.id.linearDspInfo).setVisibility(View.GONE);
                }
                //20231024 ADD Sato v2.4.1 END
            }
        }
        catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
        finally {
            if (db != null) {
                db.close();
            }
        }
        //20210918 CHG Takase V2.0 END

        //電波強度
        ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_LEASE));
    }

    /**
     * 処理履歴データを設定する
     */
    private void SetProcessData() {

        //返却日をYYYYMMDD形式にする
        String strReturnDate = _ReturnDate.replace("/", "");

        _ProcessData.setLeaseDestination(_LeaseCode);       //貸出先
        _ProcessData.setLeaseDestinationName(_LeaseName);   //貸出先名称
        _ProcessData.setLeaseEmployee(_EmployeeCode);       //貸出担当
        _ProcessData.setLeaseEmployeeName(_EmployeeName);   //貸出担当名称
        _ProcessData.setReturnDate(strReturnDate);            //返却予定日
        _ProcessData.setRemark(_Remark);
    }

    /**
     * 戻り先の画面
     */
    @Override
    protected void onReadReturn(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //クリア
        _ProcessData.AllClear();    //20200217 Add Sasaki

        SQLiteDatabase db = null;
        try {
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //予約機能なしの場合
            if (SettingFactory.GetReservationFunction(db).equals((CommonConst.RESERVATION_FUNCTION.NO.getString()))) {
                //貸出先入力画面へ
                Intent intent = new Intent(getApplication(), LeaseSetting.class);
                intent.putExtra(CommonConst.VIEW_KEY_LEASE_CD, _LeaseCode);              //貸出先コードの値を渡す
                intent.putExtra(CommonConst.VIEW_KEY_LEASE_NAME, _LeaseName);            //貸出先名の値を渡す
                intent.putExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD, _EmployeeCode);       //社員コードの値を渡す
                intent.putExtra(CommonConst.VIEW_KEY_EMPLOYEE_NAME, _EmployeeName);     //社員名の値を渡す
                intent.putExtra(CommonConst.VIEW_KEY_DATE, _ReturnDate);                  //返却予定日の値を渡す
                intent.putExtra(CommonConst.VIEW_KEY_LEASE_REMARK,_Remark);              //備考を渡す
                setResult(RESULT_OK, intent);
                finish();
            }
            else {
                //20210918 ADD Takase V2.0 START
                //予約機能あり メニュー画面へ
                Intent intent = new Intent(getApplication(), MainActivity.class);
                setResult(RESULT_OK, intent);
                startActivity(intent);
                //20210918 ADD Takase V2.0 END
            }
        }
        catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
        finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return C_Setting.KEY_EACH_LEASE;
    }

}
