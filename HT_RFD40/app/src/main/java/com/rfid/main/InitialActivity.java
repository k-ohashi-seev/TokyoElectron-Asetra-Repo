package com.rfid.main;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.database.sqlite.SQLiteDatabase;
import android.os.Build;
import android.os.Bundle;
import androidx.annotation.NonNull;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import androidx.appcompat.widget.Toolbar;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Factory.WordFactory;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;

import java.io.File;

/**
 * InitialActivityは、起動時に呼ばれます。
 * リーダーと接続します。
 * リーダーと接続すると、メニュー画面に遷移します。
 *
 */
public class InitialActivity extends BaseRFIDActivity {

    // 権限要求コード
    private static final int REQUEST_PERMISSIONS = 101;

    // The key value to exchange data with Service
    public static final String serviceKey = "serviceParam";

    //Bluetoothローカルネーム
    public static String p_btLocalName = "";

    //プログラムバージョン
    public final static String p_VersionName = "Version 1.0.0.0";

    //初期化フラグ
    private Boolean _InitializeFlag = false;

    //権限要求中フラグ
    private boolean _isPermissionRequestInProgress = false;

    /**
     *  クリエイトイベント
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {

        try {
            super.onCreate(savedInstanceState);

            checkAndRequestBluetoothPermissions();
        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * レジュームイベント
     */
    @Override
    public void onResume() {
        SQLiteDatabase _db = null;
        try {
            super.onResume();

            //権限要求中は処理なし
            if (_isPermissionRequestInProgress) {
                return;
            }

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);


            //初期化前の場合、接続する
            if(!_InitializeFlag) {
                //リーダー接続
                Reader_Connect();
                _InitializeFlag = true;
            } else {
                DatabaseHelper dbHelper = new DatabaseHelper(this);
                _db = dbHelper.getWritableDatabase();
            }

            if (CommonConst.READING_MEDIUM.QR.getString().equals(SettingFactory.GetReadingMedium(_db, this))) {
                //ログイン画面を開く
                Intent intent = new Intent(getApplication(), LoginMn.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                startActivity(intent);
            }

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (_db != null) {
                _db.close();
            }
        }
    }

    /**
     * ポーズイベント
     */
    @Override
    public void onPause() {
        try {
            super.onPause();

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            StopTimer();
        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     *  メニュー画面を開く
     */
    @Override
    protected void OpenMainActivity() {
        if( ConnectStatus()) {
            //メニュー画面を開く
            Intent intent = new Intent(getApplication(), MainActivity.class);
            startActivity(intent);
        }
    }

    /**
     * メニューの設定処理
     * @param menu
     * @return
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_initialize, menu);
        return true;
    }

    /**
     * メニュー選択イベント
     * @param item
     * @return
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        try {
            int id = item.getItemId();
            switch(id){
                case R.id.action_setting:
                    //設定 →　システム設定画面開く
                    Intent intent2 = new Intent(getApplication(), SystemSett.class);
                    startActivity(intent2);
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
            }

            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            return false;
        }
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
            //DENSOの場合
            ((TextView) findViewById(R.id.txtInitializeTitleNo1))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_INDICATION, "", this));
            ((TextView) findViewById(R.id.txtInitializeTitleNo2))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_CHARGE, "", this));
            ((TextView) findViewById(R.id.txtInitializeTitleNo3))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_PAIR_LED, "", this));
            ((TextView) findViewById(R.id.txtInitializeTitleNo4))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_POWER_LED, "", this));
            ((TextView) findViewById(R.id.txtInitializeTitleNo5))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_POWER_BUTTON, "", this));
            ((TextView) findViewById(R.id.txtInitializeTitleNo6))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_MODE_BUTTON, "", this));
            ((TextView) findViewById(R.id.txtInitializeTitleNo7))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_PAIR_BUTTON, "", this));
            ((TextView) findViewById(R.id.txtInitializeTextNo1))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_DENSO_MESSAGE1, "", this));
            ((TextView) findViewById(R.id.txtInitializeTextNo2))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_DENSO_MESSAGE2, "", this));
            ((TextView) findViewById(R.id.txtInitializeTextNo3))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_DENSO_MESSAGE3, "", this));
            ((TextView) findViewById(R.id.txtInitializeTextNo4))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_DENSO_MESSAGE4, "", this));
            ((TextView) findViewById(R.id.txtInitializeTextNo7))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_DENSO_MESSAGE7, "", this));
            ((TextView) findViewById(R.id.txtInitializeTextNo5))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTextNo6))
                    .setVisibility(View.GONE);
        }else if (CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString().equals(Common.ConnectRFIDName)) {
            //20230221 ADD Sato V2.3 キーエンス製DXシリーズの場合
            ((TextView) findViewById(R.id.txtInitializeTitleNo1))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_SCAN, "", this));
            ((TextView) findViewById(R.id.txtInitializeTitleNo2))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTitleNo3))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTitleNo4))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTitleNo5))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTitleNo6))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTitleNo7))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTextNo1))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_ZEBRA_MESSAGE4, "", this));
            ((TextView) findViewById(R.id.txtInitializeTextNo2))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTextNo3))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTextNo4))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTextNo7))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTextNo5))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeTextNo6))
                    .setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeNo2)).setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeNo3)).setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeNo4)).setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeNo5)).setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeNo6)).setVisibility(View.GONE);
            ((TextView) findViewById(R.id.txtInitializeNo7)).setVisibility(View.GONE);
        }else{
            //ZEBRAの場合
            ((TextView)findViewById(R.id.txtInitializeTitleNo1))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_MODE_BUTTON, "", this));
            ((TextView)findViewById(R.id.txtInitializeTitleNo2))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_BLUETOOTH_BUTTON, "", this));
            ((TextView)findViewById(R.id.txtInitializeTitleNo3))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_POWER_BUTTON, "", this));
            ((TextView)findViewById(R.id.txtInitializeTitleNo4))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_SCAN, "", this));
            ((TextView)findViewById(R.id.txtInitializeTitleNo5))
                    .setVisibility(View.GONE);
            ((TextView)findViewById(R.id.txtInitializeTitleNo6))
                    .setVisibility(View.GONE);
            ((TextView)findViewById(R.id.txtInitializeTitleNo7))
                    .setVisibility(View.GONE);
            ((TextView)findViewById(R.id.txtInitializeTextNo1))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_ZEBRA_MESSAGE1, "", this));
            ((TextView)findViewById(R.id.txtInitializeTextNo2))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_ZEBRA_MESSAGE2, "", this));
            ((TextView)findViewById(R.id.txtInitializeTextNo3))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_ZEBRA_MESSAGE3, "", this));
            ((TextView)findViewById(R.id.txtInitializeTextNo4))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_KIDO_ZEBRA_MESSAGE4, "", this));
            ((TextView)findViewById(R.id.txtInitializeTextNo5))
                    .setVisibility(View.GONE);
            ((TextView)findViewById(R.id.txtInitializeTextNo6))
                    .setVisibility(View.GONE);
            ((TextView)findViewById(R.id.txtInitializeTextNo7))
                    .setVisibility(View.GONE);
            ((TextView)findViewById(R.id.txtInitializeNo5))
                    .setVisibility(View.GONE);
            ((TextView)findViewById(R.id.txtInitializeNo6))
                    .setVisibility(View.GONE);
            ((TextView)findViewById(R.id.txtInitializeNo7))
                    .setVisibility(View.GONE);
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        _isPermissionRequestInProgress = false;

        if (requestCode == REQUEST_PERMISSIONS) {
            // パーミッション要求の結果を処理
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                // パーミッションが許可された場合の処理
                loadOnCreate();
            } else {
                // ユーザーがパーミッションを拒否した場合の処理
                finish();
            }
        }
    }

    /**
     * 必要な権限の要求
     */
    private void checkAndRequestBluetoothPermissions() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            // Android 12以降の場合
            if (ContextCompat.checkSelfPermission(this, Manifest.permission.BLUETOOTH_SCAN) != PackageManager.PERMISSION_GRANTED ||
                    ContextCompat.checkSelfPermission(this, Manifest.permission.BLUETOOTH_CONNECT) != PackageManager.PERMISSION_GRANTED) {
                //権限が許可されていない場合は権限を要求
                _isPermissionRequestInProgress = true;

                ActivityCompat.requestPermissions(this,
                        new String[]{Manifest.permission.BLUETOOTH_SCAN, Manifest.permission.BLUETOOTH_CONNECT},
                        REQUEST_PERMISSIONS);

            } else {
                // パーミッションがすでに許可されている場合の処理
                loadOnCreate();
            }
        } else {
            // Android 11以下の場合、パーミッション要求なしで処理続行
            loadOnCreate();
        }
    }

    private void loadOnCreate() {
        SQLiteDatabase _db = null;
        try {
            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_initial);

            Toolbar toolbar = findViewById(R.id.toolbarInitialize);
            // アイコンを指定
            toolbar.setLogo(R.drawable.logo);
            toolbar.setTitle("");
            setSupportActionBar(toolbar);

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            _db = dbHelper.getWritableDatabase();

            //テスト用EPCコードを格納するフォルダのパス
            String folderPath = String.valueOf(getExternalFilesDir(CommonConst.FOLDER_TEST));
            File rootFile = new File(folderPath);
            if (!rootFile.exists()) {
                //ディレクトリ作成
                rootFile.mkdirs();
            }

            //初期起動でない場合の値をセットする
            SystemSetting.setString(CommonConst.SYSTEM_KEY_STARTUP, CommonConst.RETURN_STARTUP_VALUE.NO.getString(), this);

            //初期起動の場合、DBから取得する
            String strWord = SystemSetting.getString(CommonConst.SYSTEM_KEY_INITIALIZE, "", this);
            if (strWord == "") {
                //文言マスタからデータを取得し設定する
                WordFactory.CreateWordData(_db, this);
                //初期起動の場合の値をセットする
                SystemSetting.setString(CommonConst.SYSTEM_KEY_STARTUP, CommonConst.RETURN_STARTUP_VALUE.YES.getString(), this);
            }

            //設定マスタの件数が0件またはQRモードの場合、メニュー画面を表示する
            if (CommonConst.READING_MEDIUM.QR.getString().equals(SettingFactory.GetReadingMedium(_db, this)) ||
                    (strWord == "")) {

                //ログイン画面を開く
                Intent intent = new Intent(getApplication(), LoginMn.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                startActivity(intent);
            } else {
                //IDATAとキーエンス製DXシリーズの場合はリーダ接続待ち画面を表示しない
                //RS35の場合も表示しない 20230410 UPD Sato v2.3.1
                if(CommonConst.CONNECT_RFIDNAME.IDATA.getString().equals(Common.ConnectRFIDName) ||
                        CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString().equals(Common.ConnectRFIDName) ||
                        CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString().equals(Common.ConnectRFIDName)){

                    //ログイン画面を開く
                    Intent intent = new Intent(getApplication(), LoginMn.class);
                    intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                    startActivity(intent);
                }else {
                    ImageView _imageView = (ImageView) findViewById(R.id.imageView);
                    if (_imageView != null) {
                        if (CommonConst.CONNECT_RFIDNAME.DENSO.getString().equals(Common.ConnectRFIDName)) {
                            //デンソーの場合の画像を設定する
                            _imageView.setImageResource(R.drawable.sp1_image);
                        } else {
                            //ZEBRAの場合の画像を設定する
                            _imageView.setImageResource(R.drawable.zebra_image);
                        }
                    }
                }
            }

            InitializeActivity = true;	//20200121 Add Sasaki

            //文言表示
            InitializeWord();

            // サービスを開始する
            super.startService();
        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (_db != null) {
                _db.close();
            }
        }
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){
        return "";
    }

    protected void BarcodeDataReceived(String strData) {}
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) {}
    protected void ReaderOn(){}
    protected void ReaderOff(){}

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }
}
