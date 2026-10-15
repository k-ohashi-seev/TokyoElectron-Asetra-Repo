package com.rfid.Return;

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
import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseRead;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.main.MainActivity;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;

/**
 * 返却読取画面
 *
 */
public class ReturnRead extends BaseRead implements CommonConfirmDialog.Callback, CommonOKDialog.Callback{

    //システム設定値
    private SettingValue C_Setting = new SettingValue();

    private TextView text = null;

    private final static int BUTTON_CLEAR = 1;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {

        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            //適用する画面レイアウトを設定   20231024 UPD Sato v2.4.1
//            setContentView(R.layout.activity_read);
            setViewLayout();

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            // プログレスダイアログの表示
            C_Progres.StartProgressDialog(this, "Loading...");

            //RFIDリーダーをOFFにする
            RFID_Read_Stop();

            //文言表示
            InitializeWord();

            //ボタン非活性
            //SetButtonEnabled(false, false);   20200213 Del Sasaki
            AllResetButton(false);  //20200213 Add Sasaki

            ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_RETURN));

            //返却であることをセットする 20230906 ADD okubo V2.4.1
            _ProcessData.setReturn();

            //データ読み込み
            InsertData();

            //タグ読取モード Clickイベント
            final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
            final CommonOKDialog.Builder  errdialog = new CommonOKDialog.Builder(this, true);
            final Activity activity = this;
            final Context context = this;

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
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", activity), activity);

                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_CLEAR);
                    dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", activity));  //クリア
                    dialog.message(SystemSetting.getString(
                            CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_CLEAR, "", activity));                //読取データをクリアします。よろしいですか？
                    dialog.params(bundle);
                    dialog.show();
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
                    Intent intent = new Intent(getApplication(), ReturnSummary.class);
                    //intent.putExtra(CommonConst.VIEW_KEY_PROCESS_DATA, _ProcessData);
                    startActivityForResult(intent, BUTTON_SUMMARY);

                }
            });

            _InitializedFlag = false;

            //サービス開始
            startService();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_RETURN,"",this));               //タイトル
        ((TextView)findViewById(R.id.labReadCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_COUNT, "", this));       //件数
        ((TextView)findViewById(R.id.labIntencity))
            .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_DENPA, "", this));           //電波強度
        ((TextView)findViewById(R.id.lblTagModeTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_TAG_MODE, "", this));    //タグモード
        ((TextView)findViewById(R.id.labReadTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this));    //タグ読取
        ((TextView)findViewById(R.id.btnClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));       //クリア
        ((TextView)findViewById(R.id.btnDetail))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LIST, "", this));         //一覧
        ((TextView)findViewById(R.id.btnTagReadStart))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", this));    //タグ読取開始
    }

    /**
     * 適用する画面レイアウトを判別
     */
    private void setViewLayout() {
        SQLiteDatabase db = null;
        try {
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //返却情報表示設定を取得
            String dspInfo = SettingFactory.GetLReturnDspInfo(db);

            if (dspInfo.equals(CommonConst.LEASE_DSP_INFO.LOGIN_INFO.getString())) {
                //ログイン情報
                setContentView(R.layout.activity_read_info_display);
                findViewById(R.id.linearDspInfo).setVisibility(View.VISIBLE);

                //ログイン社員情報を表示
                String loginEmpCode = SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD , "", this);
                String loginEmpName = EmployeeFactory.GetEmployeeName(db, loginEmpCode).trim();
                ((TextView)findViewById(R.id.txtLoginEmp)).setText(Common.substring(loginEmpCode + " " + loginEmpName, CommonConst.VIEW_LEASE_NAME_SIZE));
            } else {
                setContentView(R.layout.activity_read);
            }
        }
        catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
        finally {
            if (db != null) {
                db.close();
            }
        }

    }

    /**
     * 戻り先の画面
     */
    @Override
    public void onReadReturn(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //クリア
        _ProcessData.AllClear();    //20200217 Add Sasaki

        //メニュー画面へ
        Intent intent = new Intent(getApplication(), MainActivity.class);
        startActivity(intent);
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return C_Setting.KEY_EACH_RETURN;
    }

}
