package com.rfid.Inventory;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import androidx.core.content.ContextCompat;
import android.util.Log;
import android.util.TypedValue;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.ListView;
import android.widget.SimpleAdapter;
import android.widget.TabHost;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.main.LoginMn;
import com.rfid.setting.SystemSett;
import com.rfidpk.R;
import com.rfid.common.BaseActivity;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.CommonProgres;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.EntryInventDataDetail;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/**
 * 棚卸詳細表示のクラス
 */
public class InventorySummary extends BaseActivity implements CommonOKDialog.Callback, CommonConfirmDialog.Callback{

    //戻り値用オブジェクト
    private Intent ResIntent = new Intent();

    //プログレスダイアログ クラス
    private CommonProgres C_Progres = new CommonProgres();

    //通信中フラグ
    private Boolean _TransMesuarFlag = false;

    //完了フラグ
    private boolean _CompleteFlag = false;

    // 棚卸し保持データ
    //private InventoryFactory _InventoryData = null;   //20200211 Del Sasaki
    CommonConfirmDialog.Builder  _confirmDialog = null;

    //確認画面
    private static final int ACTION_LOGOUT = 1;
    private static final int BUTTON_REGISTER = 2;
    private static final int BUTTON_REGISTER_CONFIRM = 3;
    private static final int BUTTON_MENU = 4;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_inventory_summary);

            // プログレスダイアログの表示
            C_Progres.StartProgressDialog(this, "Loading...");

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            //前画面からデータを取得する
            //_InventoryData = (InventoryFactory)getIntent().getSerializableExtra(CommonConst.VIEW_KEY_INVENTORY_DATA);

            //文言表示
            InitializeWord();

            //画面初期化
            InitializeActivity();

            //一覧表示する(ロケエラー)
            ViewInventoryDataLocErr();

            //一覧表示する(在庫なし)
            ViewInventoryDataNone();

            //操作時間のタイマーをセット
            StartTimeoutTimer(CommonConst.VIEW_WORD_INV_LOC_TITLE);

            _confirmDialog = new CommonConfirmDialog.Builder(this);

            //20191016 Del Start Sasaki
            //実棚件数が0件の場合、登録ボタン押下不可
            /*if (_InventoryData.InventoryCount() <= 0 ) {
                ((Button) findViewById(R.id.btnInvCConfirm)).setEnabled(false);
                ((Button)findViewById(R.id.btnInvCConfirm)).setBackground(getDrawable(R.drawable.execute_button_diable));
                ((Button)findViewById(R.id.btnInvCConfirm)).setTextColor(Color.parseColor("#a9a9a9"));
            }*/
            //20191016 Del End Sasaki

            //登録ボタン Clickイベント
            final Activity activity = this;
            final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
            findViewById(R.id.btnInvCConfirm).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view) {
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", activity), activity);

                    if (checkTimeout()) {return;}

                    //20191016 Add Start Sasaki
                    if (_InventoryData.InventoryCount() == 0 ) {
                        //実棚件数0件の確認メッセージを表示する
                        ConfirmMessage(BUTTON_REGISTER_CONFIRM, CommonConst.VIEW_WORD_INV_CONFIRM_ZERO_REGISTER);
                    } else {
                        //確認メッセージを表示する
                        ConfirmMessage(BUTTON_REGISTER_CONFIRM, CommonConst.VIEW_WORD_INV_CONFIRM_REGISTER);
                    }
                    //20191016 Add End Sasaki

                }
            });

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    //メニュー選択イベント
    @SuppressLint("NonConstantResourceId")
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            switch(item.getItemId()) {
                case android.R.id.home:     //戻るボタン
//20191210 Del Start Sasaki
/*                    if(_TransMesuarFlag) {
                        //通信開始済み  除外
                        return true;
                    }
                    else if(_CompleteFlag) {
                        //完了済
                        setResult(RESULT_OK, ResIntent);    //登録完了を返す
                    }
                    else {
                        setResult(RESULT_CANCELED, ResIntent);      //キャンセルを返す
                    }*/
//20191210 Del End Sasaki
                    setResult(RESULT_OK, ResIntent);    //20191210 Add Sasaki
                    this.finish();
                    return true;

                case R.id.action_logout:  //ログアウトボタン
                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, ACTION_LOGOUT);
                    _confirmDialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_LOGIN_LOGOUT, "", this));  //ログアウト
                    _confirmDialog.message(SystemSetting.getString(
                            CommonConst.VIEW_WORD_LOGOUT, "", this));                //ログアウトします。よろしいですか？
                    _confirmDialog.params(bundle);
                    _confirmDialog.show();
                    return true;

                    //20220322 ADD Suzuki V2.1 START
                case R.id.action_setting:   //設定ボタン
                    Intent intent = new Intent(getApplication(), SystemSett.class);
                    startActivity(intent);
                    return true;
                    //20220322 ADD Suzuki V2.1 END
            }
            return super.onOptionsItemSelected(item);
        }
        catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
            return false;
        }
    }

    // レジュームイベント
    @Override
    public void onResume() {
        super.onResume();
    }

    /**
     *  ポーズイベント
     */
    @Override
    public void onPause() {

        super.onPause();
    }

    //画面初期化
    private void InitializeActivity() {
        TabHost tabHost = (TabHost) findViewById(R.id.tabHost);
        tabHost.setup();
        TabHost.TabSpec spec;

        //ロケ違いタブの設定
        spec = tabHost.newTabSpec("Tab1")
                .setIndicator(SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_ERR_BUTTON, "", this)
                        , ContextCompat.getDrawable(this, R.color.ButtonChange))
                .setContent(R.id.tabSummaryLocErr);
        tabHost.addTab(spec);

        //在庫なしタブの設定
        spec = tabHost.newTabSpec("Tab2")
                .setIndicator(SystemSetting.getString(CommonConst.VIEW_WORD_INV_STOCK_NONE_BUTTON, "", this)
                        , ContextCompat.getDrawable(this, R.color.ButtonDefault))
                .setContent(R.id.tabSummaryNone);
        tabHost.addTab(spec);

        //連続棚卸モードで実棚収集件数が0件の場合、登録ボタンを非活性にする
        if (CommonConst.INVENTORY_CONTINUE.YES.getString().equals(_InventoryData.getInventoryContinue())
                && _InventoryData.InventoryCount() == 0) {
            AllResetButton(false);
        }

        //件数表示
        ClearCountView();

    }

    /**
     * 件数を表示する
     */
    private void ClearCountView() {
        //件数表示
        ((TextView)findViewById(R.id.txtInvReadCount)).setText(
                Long.toString(_InventoryData.InventoryCountOK() + _InventoryData.InventoryCountLocErr()) +
                "/" + Long.toString(_InventoryData.TotalCount()));      //読込数   20200217 Upd Sasaki
        //((TextView)findViewById(R.id.txtInvOKCount)).setText(Long.toString(_InventoryData.InventoryCountOK()));           //正常数             20200305 Del Sasaki V1.2

        //20200305 Add Start Sasaki V1.2
        //実施数⇒今回の正常数-元の実施済正常数
        Long lngJisshiCount = _InventoryData.InventoryCountOK()  - _InventoryData.InventoryCountOKOriginal();
        //残⇒総数 - 前回までの実施済正常数 - 貸出数
        Long lngZan = _InventoryData.TotalCount() - _InventoryData.InventoryCountOKOriginal() - _InventoryData.InventoryCountLease();
        ((TextView)findViewById(R.id.txtInvOKCount)).setText(lngJisshiCount.toString() + "/"
                + SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_ZAN,"",this) + lngZan.toString());           //実施数/残
        //20200305 Add End Sasaki V1.2

        ((TextView)findViewById(R.id.txtInvLeaseCount)).setText(Long.toString(_InventoryData.InventoryCountLease()));     //棚卸時の貸出数     20200305 Add Sasaki V1.2
        ((TextView)findViewById(R.id.txtInvNoneCount)).setText(Long.toString(_InventoryData.InventoryCountNone()));       //在庫エラー
        ((TextView)findViewById(R.id.txtInvLocErrCount)).setText(Long.toString(_InventoryData.InventoryCountLocErr()));   //ロケエラー
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_TITLE,"",this));                 //実棚　ロケ指定　タイトル
        ((TextView)findViewById(R.id.labInvReadCount))
        //        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_COUNT, "", this));      //実棚収集件数                      20200305 Del Sasaki V1.2
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_SUMI_COUNT, "", this));      //実施済/総数  20200305 Add Sasaki V1.2
        ((TextView)findViewById(R.id.labInvOKCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_OK_COUNT, "", this));   //正常数⇒実施数/残
        ((TextView)findViewById(R.id.labInvLeaseCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_LEASE_COUNT, "", this));   //棚卸時の貸出数    20200305 Add Sasaki V1.2
        ((TextView)findViewById(R.id.labInvLocErrCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_ERR_COUNT, "", this));  //ロケ違い
        ((TextView)findViewById(R.id.labInvNoneCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_NONE_COUNT, "", this)); //在庫なし数
        ((TextView)findViewById(R.id.btnInvCConfirm))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));        //登録
    }

    /** ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * */
    private void SetButtonEnabled(boolean prmEnabled) {
        Button _ButonConfirm = findViewById(R.id.btnInvCConfirm);
        _ButonConfirm.setEnabled(prmEnabled);
    }

    /**
     * 一覧表示する(在庫なし)
     */
    private void ViewInventoryDataNone() {
        new Thread() {
            @Override
            public void run() {
                try {
                    //在庫なしデータを取得する
                    LinkedHashMap<String, EntryInventDataDetail> data = _InventoryData.GetInventoryNoneData();

                    //取得したデータを一覧表示に設定
                    final List<Map<String, String>> list = new ArrayList<Map<String, String>>();
                    for (Map.Entry<String, EntryInventDataDetail> entry : data.entrySet()) {
                        //表示データを設定する
                        Map<String, String> map = new HashMap<String, String>();
                        map.put(CommonConst.List_VIEW_KEY.Main.getString(), entry.getKey());                     //資産番号
                        map.put(CommonConst.List_VIEW_KEY.Sub.getString(), entry.getValue().getAssetName());     //資産名称
                        map.put(CommonConst.List_VIEW_KEY.Sub2.getString(), entry.getValue().getSubAssetNo());   //SUB資産名称
                        list.add(map);

                    }

                    //初期化
                    SimpleAdapter arrayAdapter = InitializeAdapter(list);
                    ListView listView = (ListView) findViewById(R.id.listview_inventory_summary_none);

                    //リストに追加
                    listView.setAdapter(arrayAdapter);

                    //長押しイベントを追加
                    listView.setOnItemLongClickListener(new AdapterView.OnItemLongClickListener() {
                        @Override
                        public boolean onItemLongClick(AdapterView<?> parent, View view, int position, long id) {

                            if (checkTimeout()) {return true;}

                            // 長押しされた行のデータを取得
                            Map<String, String> item = list.get(position);

                            String assetNo = item.get(CommonConst.List_VIEW_KEY.Main.getString());
                            String subAssetNo = item.get(CommonConst.List_VIEW_KEY.Sub2.getString());
                            String assetName = item.get(CommonConst.List_VIEW_KEY.Sub.getString());

                            ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NUMBER, "", InventorySummary.this) + ": " + assetNo +
                                    "\n" + SystemSetting.getString(CommonConst.VIEW_WORD_INV_SUB_ASSETNO, "", InventorySummary.this) + ": " + subAssetNo +
                                    "\n" + SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NAME, "", InventorySummary.this) + ": " + assetName, SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_TITLE,"",InventorySummary.this));

                            // trueを返すとクリックイベントをこれ以上伝搬させない
                            return true;
                        }
                    });
                } finally {
                    C_Progres.StopProgressDialog();
                }
            }
        }.start();
    }

    /**
     * 一覧表示する(ロケエラー)
     */
    private void ViewInventoryDataLocErr() {

        //ロケエラーデータを取得する
        LinkedHashMap<String, EntryInventDataDetail> data = _InventoryData.GetInventoryLocErrData();

        //取得したデータを一覧表示に設定
        final List<Map<String, String>> list = new ArrayList<Map<String, String>>();
        for  (Map.Entry<String, EntryInventDataDetail>entry : data.entrySet()){
            //表示データを設定する
            Map<String, String> map = new HashMap<String, String>();
            map.put(CommonConst.List_VIEW_KEY.Main.getString(), entry.getKey());                     //資産番号
            if ( entry.getValue() != null) {
                map.put(CommonConst.List_VIEW_KEY.Sub.getString(), entry.getValue().getAssetName());     //資産名称
                map.put(CommonConst.List_VIEW_KEY.Sub2.getString(), entry.getValue().getSubAssetNo());   //SUB資産名称
            }
            list.add(map);

        }

        //初期化
        SimpleAdapter arrayAdapter = InitializeAdapter(list);
        ListView listView = (ListView) findViewById(R.id.listview_inventory_summary_LocErr);

        //リストに追加
        listView.setAdapter(arrayAdapter);

        //長押しイベントを追加
        listView.setOnItemLongClickListener(new AdapterView.OnItemLongClickListener() {
            @Override
            public boolean onItemLongClick(AdapterView<?> parent, View view, int position, long id) {

                if (checkTimeout()) {return true;}

                // 長押しされた行のデータを取得
                Map<String, String> item = list.get(position);

                String assetNo = item.get(CommonConst.List_VIEW_KEY.Main.getString());
                String subAssetNo = item.get(CommonConst.List_VIEW_KEY.Sub2.getString());
                String assetName = item.get(CommonConst.List_VIEW_KEY.Sub.getString());

                ShowOKMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NUMBER, "", InventorySummary.this) + ": " + assetNo +
                        "\n" + SystemSetting.getString(CommonConst.VIEW_WORD_INV_SUB_ASSETNO, "", InventorySummary.this) + ": " + subAssetNo +
                        "\n" + SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_ASSET_NAME, "", InventorySummary.this) + ": " + assetName, SystemSetting.getString(CommonConst.VIEW_WORD_INV_LOC_TITLE,"",InventorySummary.this));

                // trueを返すとクリックイベントをこれ以上伝搬させない
                return true;
            }
        });

    }

    /**
     * 一覧表示の初期化処理
     * @return 一覧表示用Adapter
     */
    private SimpleAdapter InitializeAdapter(List list){

        //Adapterの設定
        SimpleAdapter adapter = new SimpleAdapter(
                this,
                list,
                android.R.layout.simple_list_item_2,
                new String[] {CommonConst.List_VIEW_KEY.Main.getString(), CommonConst.List_VIEW_KEY.Sub.getString()},
                new int[] {android.R.id.text1, android.R.id.text2}
        ){
            /**
             * 一覧表示設定
             * @param position
             * @param convertView
             * @param parent
             * @return
             */
            @Override
            public View getView(int position, View convertView, ViewGroup parent){
                View view = super.getView(position, convertView, parent);

                if (position % 2 == 0){
                    view.setBackgroundResource(R.color.ListViewBack);
                } else {
                    view.setBackgroundResource(R.color.ListViewBack2);
                }

                //20230221 ADD Sato V2.3 Start
                if (Common.ConnectRFIDName.equals(CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString())) {
                    TextView first = view.findViewById(android.R.id.text1);
                    TextView second = view.findViewById(android.R.id.text2);
                    first.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_first_line_text));
                    second.setTextSize(TypedValue.COMPLEX_UNIT_PX, getResources().getDimension(R.dimen.simple_second_line_text));
                }
                //20230221 ADD Sato V2.3 End

                return view;
            }
        };

        return adapter;
    }

    //20191016 Add Start Sasaki
    /**
     * 実棚登録処理
     */
    private void RegisterProcess() {
        SQLiteDatabase db = null;
        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            db.beginTransaction();

            //登録前に件数を取得する
            Long lngCount =  _InventoryData.InventoryCount();       //20191210 Add Sasaki

            //登録処理
            //_InventoryData.Insert(db);                  20191016 Del Sasaki
            _InventoryData.InsertLoc(db, this);    //20191016 Add Sasaki

            //イベントログ
            LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                    GetWord(CommonConst.VIEW_WORD_INV_MSG_REGISTER)
//                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + _InventoryData.InventoryCountOK() 20191210 Del Sasaki
                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + lngCount   //20191210 Add Sasaki
                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_LOC_L_KBN) + "】" + _InventoryData.getLocLCode()
                            + " " + _InventoryData.getLocLName()
                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_LOC_M_KBN) + "】" + _InventoryData.getLocMCode()
                            + " " + _InventoryData.getLocMName()
                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_LOC_S_KBN) + "】" + _InventoryData.getLocSCode()
                            + " " + _InventoryData.getLocSName()
                    , CommonConst.LOG_KIND.Inventory
                    , this);

            //HT読込データをクリアする
            //_InventoryData.Clear();       20200217 Del Sasaki
            _InventoryData.AllClear();      //20200217 Add Sasaki

            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    //件数表示
                    ClearCountView();
                }
            });

            //commit
            db.setTransactionSuccessful();

            //メッセージBOX
            CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
            Bundle bundle = new Bundle();
            bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_REGISTER);
            dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
            dialog.message(SystemSetting.getString(
                    CommonConst.VIEW_WORD_INV_MSG_REGISTER, "", this));                //実棚登録は完了しました
            dialog.params(bundle);
            dialog.show();

        } catch (Exception ex) {
            //エラーログ
            if (db != null) {
                db.endTransaction();
                db.close();
                db = null;
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_MENU_INVENTORY, "", this) + ":" + Common.toString(ex), this);
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.endTransaction();
            }
            // プログレスダイアログを閉じる
            C_Progres.StopProgressDialog();
        }
    }
    //20191016 Add End Sasaki

    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        if (params == null) {
            return;
        }

        try {
            //呼出元で分岐する
            switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
                //20191016 Add Start Sasaki
                case BUTTON_REGISTER_CONFIRM:    //登録確認ボタン
                    //ボタン非活性
                    AllResetButton(false);
                    C_Progres.StartProgressDialog(this, "Processing...");
                    new Thread() {
                        @Override
                        public void run() {
                            RegisterProcess();
                        }
                    }.start();
                    break;
                //20191016 Add End Sasaki

                case BUTTON_REGISTER:      //登録ボタン
                    //ボタン非活性
                    AllResetButton(false);
                    //実棚収集画面(ロケ指定)へ
                    Intent intent = new Intent(getApplication(), InventoryLocationSetting.class);
                    startActivity(intent);
                    break;

                case ACTION_LOGOUT:      //ログアウトボタン
                    //ボタン非活性
                    AllResetButton(false);
                    //ログイン画面を開く
                    intent = new Intent(getApplication(), LoginMn.class);
                    intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, CommonConst.VIEW_WORD_MENU_TITLE);
                    startActivity(intent);
                    break;
            }
        } catch (Exception e) {
            //ボタン活性
            AllResetButton(true);
        }
    }

    /**
     * 確認画面を表示する
     * @param intButton
     */
    protected void onReturnConfirm(int intButton) {
        //確認画面を表示し、OKの場合true、キャンセルの場合false
        final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, intButton);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
        dialog.message(SystemSetting.getString(
                CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_RETURN, "", this));               //登録されていない読取データは破棄されます。よろしいですか？
        dialog.params(bundle);
        dialog.show();
    }

    /**
     * 確認メッセージを表示する
     * @param intButton
     * @param strMessage
     */
    private void ConfirmMessage(int intButton, String strMessage) {
        //メッセージBOX
        final CommonConfirmDialog.Builder dialog = new CommonConfirmDialog.Builder(this);
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, intButton);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
        dialog.message(SystemSetting.getString(strMessage, "", this));
        dialog.params(bundle);
        dialog.show();
    }

    /**
     * メニューへ戻る処理
     * @return
     */
    private boolean returnMenu() {
        if (_InventoryData.InventoryCount() > 0) {
            onReturnConfirm(BUTTON_MENU);
            return true;
        } else {
            _InventoryData.AllClear();
            onMenuReturn();
            return true;
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
        getMenuInflater().inflate(R.menu.menu_noconnect, menu);
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
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {
        //20191021 Add Start Sasaki
        if(blnEnabled) {
            ((Button) findViewById(R.id.btnInvCConfirm)).setEnabled(false);
            ((Button)findViewById(R.id.btnInvCConfirm)).setBackground(getDrawable(R.drawable.execute_button));
            ((Button)findViewById(R.id.btnInvCConfirm)).setTextColor(Color.parseColor("#ffffff"));
        } else {
            ((Button) findViewById(R.id.btnInvCConfirm)).setEnabled(false);
            ((Button)findViewById(R.id.btnInvCConfirm)).setBackground(getDrawable(R.drawable.execute_button_diable));
            ((Button)findViewById(R.id.btnInvCConfirm)).setTextColor(Color.parseColor("#a9a9a9"));
        }

    }

/************************************************************************/
/*  定義のみ
/************************************************************************/
    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {}
}
