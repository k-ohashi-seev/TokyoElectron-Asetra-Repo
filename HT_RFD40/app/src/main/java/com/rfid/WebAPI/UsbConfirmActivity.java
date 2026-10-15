package com.rfid.WebAPI;

import android.app.Activity;
import android.app.Instrumentation;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.media.MediaScannerConnection;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.common.BaseRFIDActivity;
import com.rfid.common.Common;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;

public class UsbConfirmActivity extends BaseRFIDActivity {
    //送受信画面キー
    private final static int FORM_REQUEST_RECEIVE = 1;
    private final static int FORM_REQUEST_SEND = 2;

    private boolean _sendFlg = true;

    //20230206 ADD Kawamoto V2.3 START
    // フォルダ名
    private final String INPUT_FOLDER = "Input";
    // 対象拠点ファイル名
    private final String TARGER_LOC_FILE_NAME = "HT_TARGET_LOC.TXT";
    //20230206 ADD Kawamoto V2.3 END

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        SQLiteDatabase db = null;
        try {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.activity_usb_confirm);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            Intent intent = getIntent();
            String val = intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT);
            if (CommonConst.VIEW_WORD_ALL_RECIVE_DATA.equals(val)) {
                _sendFlg = false;

                //対象拠点ファイル作成
                WriteFile();
            }

            //文言表示
            InitializeWord();

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(getApplicationContext());
            db = dbHelper.getWritableDatabase();


            //次ボタン Clickイベント
            findViewById(R.id.btnUsbNext).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    if (_sendFlg) {
                        //データ送信(ファイル書込)画面へ
                        Intent intent = new Intent(getApplication(), UsbSendActivity.class);
                        startActivityForResult(intent, FORM_REQUEST_SEND);
                    } else {
                        //マスタ受信(ファイル読込)画面へ
                        Intent intent = new Intent(getApplication(), UsbRecieveActivity.class);
                        startActivityForResult(intent, FORM_REQUEST_RECEIVE);
                    }
                }
            });

        } catch (Exception ex) {
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", this) + ":" + Common.toString(ex), this);
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    @Override
    protected void OpenMainActivity() { }

    @Override
    protected void ReaderOn() { }

    @Override
    protected void ReaderOff() { }

    @Override
    protected void RFIDDataReceived(String strReadData, short tempRSSI, final String strPcHex) { }

    @Override
    protected void BarcodeDataReceived(String strReadData) { }

    @Override
    protected String GetSettingKey() {
        return null;
    }

    @Override
    protected void AllResetButton(boolean blnEnabled) { }



    /**
     * 文言表示
     */
    private void InitializeWord() {
        if (_sendFlg) {
            setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_USB_SEND_TITLE,"",this));                   //蓄積データ送信
            ((TextView)findViewById(R.id.labUsbText)).setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_USB_SEND_GUIDE,"",this));
            ((Button)findViewById(R.id.btnUsbNext)).setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_USB_SEND_DATA,"",this));
        } else {
            setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_USB_RECEIVE_TITLE,"",this));                 //マスタ取込受信
            ((TextView)findViewById(R.id.labUsbText)).setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_USB_RECEIVE_GUIDE,"",this));
            ((Button)findViewById(R.id.btnUsbNext)).setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_USD_RECIVE_DATA,"",this));
        }
    }
    /**
     * サブ画面から戻るイベント
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data){
        setResult(resultCode);
        if (resultCode == RESULT_OK) {
            setResult(RESULT_OK);
            finish();
        }
    }

    /**
     * 対象拠点ファイル作成
     * @throws IOException
     */
    public void WriteFile() throws IOException {

        // フォルダ名
        String folderPath = String.valueOf(this.getExternalFilesDir(INPUT_FOLDER));
        File rootFile = new File(folderPath);
        if (!rootFile.exists()) {
            //ディレクトリ作成
            rootFile.mkdirs();
        }

        // ファイル名（パス）
        String fileName = folderPath + "/" + TARGER_LOC_FILE_NAME;
        File file = new File(fileName);
        if (file.exists()) {
            //ファイル再作成
            file.delete();
            file = new File(fileName);
        }

        //現在設定されている対象拠点を取得
        SettingValue C_Setting = new SettingValue();
        String LocCode = C_Setting.getTargetHub(this);

        // ファイル書込
        FileOutputStream fos = new FileOutputStream(fileName);
        byte[] sendJson = null;
        sendJson = LocCode.getBytes("UTF-8");
        fos.write(sendJson);

        // 確定
        fos.flush();
        fos.close();

        //作成したファイルをAndroidに認識させる
        String[] path = new String[]{file.getPath()};
        String[] type = new String[]{"application/json"};
        MediaScannerConnection.scanFile(this.getApplicationContext(), path, type, null);
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }
}
