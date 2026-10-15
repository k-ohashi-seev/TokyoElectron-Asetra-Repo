package com.rfid.setting;

import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.SimpleAdapter;
import android.widget.Spinner;
import android.widget.TextView;

import com.rfid.Factory.LocationFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.WebAPI.UsbConfirmActivity;
import com.rfid.WebAPI.WebReciveActivity;
import com.rfid.common.BaseActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonInputPasswordDialog;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfidpk.R;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class SystemSettHub extends BaseActivity {

    List<CmbItem> spinnerItems;
    Adapter adapter;

    //送受信画面キー
    private final static int FORM_REQUEST_RECEIVE = 1;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_system_sett_hub);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //文言設定
            InitializeWord();

            //対象拠点コンボボックス作成
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
                    //20230206 UPD Kawamoto V2.3 START
                    //setValueData();
                    //this.finish();
                    if (setValueData()) {
                        this.finish();
                    }
                    //20230206 UPD Kawamoto V2.3 END
                    return true;
                //20230206 DEL Kawamoto V2.3 START
                //case R.id.action_menu:      //メニューへボタン
                //設定値保存
                //setValueData();
                //onMenuReturn();
                //return true;
                //20230206 DEL Kawamoto V2.3 END
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
                    //20230206 UPD Kawamoto V2.3 START
                    //setValueData();
                    //this.finish();
                    if (setValueData()) {
                        this.finish();
                    }
                    //20230206 UPD Kawamoto V2.3 END
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
            getValueData();
        }
        catch (Exception ex) {
            ex.printStackTrace();
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    private void InitializeActivity() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //対象拠点のコンボボックスを設定する
        spinnerItems = SetHubComboData();
        adapter = new Adapter(this, spinnerItems);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        ((Spinner)findViewById(R.id.cmbSetHub)).setAdapter(adapter);
    }

    private void getValueData() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            SettingValue C_Setting = new SettingValue();

            //現在設定されている対象拠点を取得する
            String code = C_Setting.getTargetHub(this);

            //現在設定されている対象拠点を初期値とする
            int spinnerPosition = -1;
            if (code != null) {
                for(CmbItem item : spinnerItems) {
                    if (item._code.equals(code)) {
                        spinnerPosition = adapter.getPosition(item);
                        break;
                    }
                }
            }
            ((Spinner)findViewById(R.id.cmbSetHub)).setSelection(spinnerPosition);

        } catch (Exception ex) {
            ex.printStackTrace();
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    // 設定値保存
    private  Boolean setValueData()  {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            SettingValue C_Setting = new SettingValue();
            String _Session = "";
            Spinner _CmbSession = null;

            _CmbSession = (Spinner)findViewById(R.id.cmbSetHub);

            CmbItem item = (CmbItem) _CmbSession.getSelectedItem();
            _Session = item._code;

            //20230206 ADD Kawamoto V2.3 START
            if (!_Session.equals(C_Setting.getTargetHub(this))) {
                //メッセージ
                ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CONFIRM_SET_HUB,"", this), SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_SET_HUB,"", this));     //対象拠点を変更した場合はマスタ受信を行ってください。
                C_Setting.setTargetHub(this, _Session);
                return false;
            }
            //20230206 ADD Kawamoto V2.3 END
            C_Setting.setTargetHub(this, _Session);

            return true;
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * 拠点コンボボックス設定
     * @return
     */
    private List<CmbItem> SetHubComboData() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        SQLiteDatabase db = null;
        List<CmbItem> listview = null;
        try {
            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            //DB接続用変数
            db = dbHelper.getWritableDatabase();

            //SQL作成
            String strSql = LocationFactory.createSQLData();
            List<String> list = new ArrayList<>();
            list.add(CommonConst.LOCATION_DIV.L.getString());
            list.add("%");
            String[] strWhere = list.toArray(new String[list.size()]);
            if (strWhere.length == 0){
                strWhere = null;
            }
            //取得
            Cursor cursor = db.rawQuery(strSql, strWhere);

            //取得したデータを一覧表示に設定
            listview = new ArrayList<>();

            //空白設定
            CmbItem item = new CmbItem("", "");
            listview.add(item);

            while (cursor.moveToNext()){
                //表示データを設定する
                item = new CmbItem(cursor.getString(0), cursor.getString(1));
                listview.add(item);
            }
        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        } finally {
            if (db != null) {
                db.close();
            }
        }
        return listview;
    }

    public class CmbItem {
        public String _code;     //ロケコード
        public String _name;     //ロケ名称

        public CmbItem(String code, String name) {
            _code = code;
            _name = name;
        }
    }

    public class Adapter extends ArrayAdapter<CmbItem> {
        public Adapter(Context context, List<CmbItem> list) {
            super(context, android.R.layout.simple_spinner_item, (List<CmbItem>) list);
            setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        }

        @Override
        public View getView(int position, View convertView, ViewGroup parent) {
            TextView textView = (TextView) super.getView(position, convertView, parent);
            textView.setText(getItem(position)._name);
            return textView;
        }

        @Override
        public View getDropDownView(int position, View convertView, ViewGroup parent) {
            TextView textView = (TextView) super.getDropDownView(position, convertView, parent);
            textView.setText(getItem(position)._name);
            return textView;
        }
    }

    /**
     * メニューの設定処理
     * @param menu
     * @return
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu_setting, menu);
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
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_SETTEI,"",this));                      //タイトル
        ((TextView)findViewById(R.id.labSetHub))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_SYSTEM_HUB, "", this));             //対象拠点
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}


    //20230206 ADD Kawamoto V2.3 START
    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     * @param requestCode CustomDialg 実行時 requestCode
     * @param params CustomDialg に受渡した引数
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        super.onMyDialogSucceeded(requestCode, params);

        SettingValue setting = new SettingValue();
        if (setting.getTransfer(this).equals(CommonConst.TRANSFER_METHOD.USB.getString())) {
            //ＵＳＢ接続確認画面へ
            Intent intent = new Intent(getApplication(), UsbConfirmActivity.class);
            intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_ALL_RECIVE_DATA);
            startActivityForResult(intent, FORM_REQUEST_RECEIVE);
        } else {
            //マスタ受信画面へ
            Intent intent = new Intent(getApplication(), WebReciveActivity.class);
            startActivityForResult(intent, FORM_REQUEST_RECEIVE);
        }
        finish();
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }
    //20230206 ADD Kawamoto V2.3 END
}
