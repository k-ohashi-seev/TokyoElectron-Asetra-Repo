package com.rfid.common;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.Switch;
import android.widget.TextView;

import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.LentAssetListFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.MatchAseetListFactory;
import com.rfid.Factory.RetAseetListFactory;
import com.rfid.Factory.TempAssetFactory;
import com.rfid.Factory.TempAssetListFactory;
import com.rfid.Factory.TemporaryFactory;
import com.rfid.Lease.MatLeaseSummary;
import com.rfid.Match.MatchSummary;
import com.rfid.Temp.MatTempSummary;
import com.rfid.setting.SettingValue;
import com.rfidpk.R;

import java.util.Map;
import java.util.Objects;
import java.util.TreeMap;

/**
 * 照合資産読取画面
 *
 */
public class CommonMatRead extends BaseRead implements CommonConfirmDialog.Callback, CommonOKDialog.Callback{

    //システム設定値
    private SettingValue C_Setting = new SettingValue();

    private TextView text = null;

    private final static int BUTTON_CLEAR = 1;
    private final static int BUTTON_RETURN = 2;

    public static final int RESIST_OK = 5;

    //メニュー選択画面
    private String _MenuClick = "";

    //工事貸出終了日
    private String _LentEndDt = "";

    //工事選択No
    private String _ConstRuctionNo = "";

    //仮置きId (仮置き時に使用)
    private String _TempId = "";

    //仮置き登録 or 仮置き変更　チェック
    private String _TempSelectState = "";


    private boolean _isRFIDReaderUse = true;

    //クリエイトイベント
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        try {
            super.onCreate(savedInstanceState);
            //ほか仮置き数
            _ProcessData.ClearAlreadyTempCnt();
            //リスト不一致数クリア
            _ProcessData.ClearNoListCnt();
            //照合一致数クリア
            _ProcessData.ClearListCnt();
            //未読み取り照合リストクリア
            _ProcessData.MatAssetIDReadClear();
            _ProcessData.MatAssetNOReadClear();

            //ソートデータクリア
            _ProcessData.ClearSortData();

            Intent intent = getIntent();
            //メニューで選択したメニュー取得
            _MenuClick = intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT);

            _LentEndDt = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT);

            //工事選択No取得
            _ConstRuctionNo = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO);

            String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
            LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            setContentView(R.layout.activity_mat_read);

            //タイトルバー 戻るボタン表示
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);

            // プログレスダイアログの表示
            C_Progres.StartProgressDialog(this, "Loading...");

            //設定の読取モード取得
            boolean _isRFIDReaderUse = Objects.equals(C_Setting.getRfidReaderUse(this), "1");

            //メニューで選択したボタンが貸出(照合)
            if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_LENDING)){
                // 「貸出」設定
                _ProcessData.setIsLease(true);

                findViewById(R.id.labItemCarriedCntTtl).setVisibility(View.GONE);
                findViewById(R.id.labItemCarriedCnt).setVisibility(View.GONE);
            }else if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST)){
                //仮置き登録メニューの場合
                findViewById(R.id.labItemCarriedCntTtl).setVisibility(View.GONE);
                findViewById(R.id.labItemCarriedCnt).setVisibility(View.GONE);

                //仮置きID
                _TempId = intent.getStringExtra(CommonConst.VIEW_KEY_HT_TEMP_ID);
                //仮置き登録 or 変更　チェック
                _TempSelectState = intent.getStringExtra(CommonConst.VIEW_KEY_STATE_SELECT);
            }

            //RFIDリーダーをOFFにする
            RFID_Read_Stop();

            //文言表示
            InitializeWord();

            //ボタン非活性
            AllResetButton(false);

            //データ読み込み
            InsertData();

            //照合データ取得
            MatchData();

            //タグ読取モード Clickイベント
            final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
            final CommonOKDialog.Builder  errdialog = new CommonOKDialog.Builder(this, true);
            final Activity activity = this;
            final Context context = this;
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
                    String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                    LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", activity), activity);

                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_CLEAR);
                    dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", activity));  //クリア
                    dialog.message(SystemSetting.getString(
                        CommonConst.VIEW_WORD_ALL_CONFIRM_DATA_CLEAR, "", activity));                //読取データをクリアします。よろしいですか？
                    dialog.params(bundle);
                    dialog.show();

                    MatchData();
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
                    Intent intent = null;

                    //未読み取り照合リストクリア
                    _ProcessData.MatAssetIDReadClear();
                    _ProcessData.MatAssetNOReadClear();

                    TreeMap<String, String> sortedMap = new TreeMap<String,String>();

                    //メニュー画面で選択した機能へ遷移する
                    switch (_MenuClick) {
                        case CommonConst.VIEW_WORD_MENU_LENDING:        //貸出
                            // 「貸出(照合)」設定
                            _ProcessData.setIsMatLease(true);
                            //詳細表示画面へ
                            intent = new Intent(getApplication(), MatLeaseSummary.class);

                            break;
                        case CommonConst.VIEW_WORD_MENU_MATCHIN:           //持込照合
                        case CommonConst.VIEW_WORD_MENU_MATCHOUT:          //持帰照合
                        case CommonConst.VIEW_WORD_MENU_MATCHRETURN:       //照合返却
                            //詳細表示画面へ
                            intent = new Intent(getApplication(), MatchSummary.class);
                            //未読み取りの照合データを入れ込む
                                                        //読み取り機能がRFIDの時
                            if(_isRFIDReaderUse == true){
                                _ProcessData.addKeysTolistMatAssetIDRead();
                            }else{
                                _ProcessData.addKeysTolistMatAssetNORead();
                            }

                            break;
                        case CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST:   //仮置登録
                            // 「仮置き」設定
                            _ProcessData.setIsTemporary(true);
                            //詳細表示画面へ
                            intent = new Intent(getApplication(), MatTempSummary.class);

                            intent.putExtra(CommonConst.VIEW_KEY_STATE_SELECT,_TempSelectState);
                            intent.putExtra(CommonConst.VIEW_KEY_HT_TEMP_ID, _TempId);

                            break;

                        default:
                            break;
                    }

                    intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                    intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, _ConstRuctionNo);
                    intent.putExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT, _LentEndDt);
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
        //タイトル
        //メニューボタンでの遷移機能でタイトルを設定

        //メニュー画面で選択した機能へ遷移する
        switch (_MenuClick) {

            case CommonConst.VIEW_WORD_MENU_LENDING:        //貸出
                setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING,"",this));
                //読み取り強度
                ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_LEASE));
                break;
            case CommonConst.VIEW_WORD_MENU_MATCHIN:           //持込照合
                setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHIN,"",this));
                //読み取り強度
                ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_MATIN));
                ((TextView)findViewById(R.id.labItemCarriedCntTtl))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CARRY_COUNT, "", this));       //持出数
                break;
            case CommonConst.VIEW_WORD_MENU_MATCHOUT:          //持帰照合
                setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHOUT,"",this));
                //読み取り強度
                ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_MATOUT));
                ((TextView)findViewById(R.id.labItemCarriedCntTtl))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CARRY_COUNT, "", this));       //持出数
                break;
            case CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST:   //仮置登録
                setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST,"",this));
                //読み取り強度
                ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_MATTEMP));
                break;
            case CommonConst.VIEW_WORD_MENU_MATCHRETURN:       //照合返却
                setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHRETURN,"",this));
                //読み取り強度
                ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, C_Setting.KEY_EACH_MATRET));
                ((TextView)findViewById(R.id.labItemCarriedCntTtl))
                        .setText(SystemSetting.getString(CommonConst.VIEW_WORD_MATRETURN_COUNT, "", this));       //持出数
                break;
            default:
                break;
        }

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
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_LIST, "", this));        //一覧
        ((TextView)findViewById(R.id.btnTagReadStart))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_INV_READ_START, "", this));    //タグ読取開始
    }


    /**
     * 戻り先の画面
     */
    @Override
    public void onReadReturn(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //クリア
        _ProcessData.AllClear();

        finish();
    }

    /**
     * 画面ごとの設定キーを返す
     * @return
     */
    @Override
    protected String GetSettingKey(){

        //画面によって強度を選択
        switch (_MenuClick) {

            case CommonConst.VIEW_WORD_MENU_LENDING:        //貸出
                return C_Setting.KEY_EACH_LEASE;

            case CommonConst.VIEW_WORD_MENU_MATCHIN:           //持込照合
                return C_Setting.KEY_EACH_MATIN;

            case CommonConst.VIEW_WORD_MENU_MATCHOUT:          //持帰照合
                return C_Setting.KEY_EACH_MATOUT;

            case CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST:   //仮置登録
                return C_Setting.KEY_EACH_MATTEMP;

            case CommonConst.VIEW_WORD_MENU_MATCHRETURN:       //照合返却
                return C_Setting.KEY_EACH_MATRET;

            default:
                return "";
        }
    }

    @Override
    protected void RFIDDataReceived(String strData, short tempRSSI, final String strPcHex) {
        SQLiteDatabase db = null;
        try {

            //既に読込済みの場合終了
            if (_ProcessData.ExistAssetID(strData)) {
                return;
            }
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //資産番号を取得
            String strAssetNo = AssetFactory.Convert(db, strData);

            //資産マスタに存在する資産の場合、QR読取済みかどうかをチェック
            if (_ProcessData.ExistMasterAssetID(strData)) {
                //既に読込済みの場合終了
                if (_ProcessData.ExistAssetNo(strAssetNo)) {
                    return;
                }
            }

            //資産ID情報を溜め込む
            _ProcessData.StockAssetID(strData);
            Log.d("資産ID", strData);

            //参照権限チェック
            //参照権限が無い資産の場合
            if(!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, strAssetNo), CommonAuthority.Auth.readHubList)) {
                //読み捨てる（一覧表示は行う）
                return;
            }

            //メニュー画面で選択した機能へ遷移する
            switch (_MenuClick) {
                case CommonConst.VIEW_WORD_MENU_LENDING:        //貸出(照合)
                    //マスタに存在する場合、資産IDを溜め込む
                    if (_ProcessData.ExistMasterAssetID(strData)) {
                        //更新権限チェック
                        AssetModel asset = AssetFactory.Select(db, strAssetNo);
                        // (貸出の時貸出状態チェック）　&& 更新権限チェック
                        if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())
                                || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {

                            //貸出ステータス貸出中でない場合
                            if(!CommonConst.ASSET_STATUS.Lease.getString().equals(asset.getAssetStatus())){
                                //カウント音を出す
                                if (beepSound == null) {
                                    beepSound = new BeepSound(this);
                                }
                                beepSound.BeepCountUp();
                                //資産IDを溜め込む
                                _ProcessData.StockViewAssetData(strData);
                            }
                        }

                    }
                    break;

                case CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST:   //仮置登録

                    //マスタに存在する場合、資産IDを溜め込む
                    if (_ProcessData.ExistMasterAssetID(strData)) {
                        //更新権限チェック
                        AssetModel asset = AssetFactory.Select(db, strAssetNo);
                        // (貸出の時貸出状態チェック）　&& 更新権限チェック
                        if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())
                                || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {
                            //カウント音を出す
                            if (beepSound == null) {
                                beepSound = new BeepSound(this);
                            }
                            beepSound.BeepCountUp();

                            //資産IDを溜め込む
                            _ProcessData.StockViewAssetData(strData);

//                            //照合データに含まれていない場合チェック
//                            if(!_ProcessData.ExistCollationAssetID(strData)){
//                                //リスト不一致数カウントアップ
//                                _ProcessData.cntNoListCnt();
//
//                            }else{
//                                //ほかで仮置き登録済みである場合
//                                if(_ProcessData.ExistTempListAssetNo(strAssetNo)){
//                                    //他仮置き数カウントアップ
//                                    _ProcessData.cntAlreadyTempCnt();
//                                }
//                            }
                            //ほかで仮置き登録済みである場合
                            if(_ProcessData.ExistTempListAssetNo(strAssetNo)){
                                //他仮置き数カウントアップ
                                _ProcessData.cntAlreadyTempCnt();
                            }else{
                                if(!_ProcessData.ExistCollationAssetID(strData)){
                                    //リスト不一致数カウントアップ
                                    _ProcessData.cntNoListCnt();

                                }
                            }

                        }
                    }


                    break;
                case CommonConst.VIEW_WORD_MENU_MATCHIN:           //持込照合
                case CommonConst.VIEW_WORD_MENU_MATCHOUT:          //持帰照合
                case CommonConst.VIEW_WORD_MENU_MATCHRETURN:       //照合返却

                    //マスタに存在する場合、資産IDを溜め込む
                    if (_ProcessData.ExistMasterAssetID(strData)) {

                        //貸出、仮置きの場合は、資産にない情報も取得
//                        //資産ID情報を溜め込む
//                        _ProcessData.StockAssetID(strData);

                        //更新権限チェック
                        AssetModel asset = AssetFactory.Select(db, strAssetNo);
                        // (貸出の時貸出状態チェック）　&& 更新権限チェック
                        if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())
                                || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {


                            //カウント音を出す
                            if (beepSound == null) {
                                beepSound = new BeepSound(this);
                            }

                            _ProcessData.RemoveCollationViewAssetNo(strAssetNo); // 削除処理

                            beepSound.BeepCountUp();
                            //資産IDを溜め込む
                            _ProcessData.StockViewAssetData(strData);

                            //照合データに含まれているかチェック
                            if(_ProcessData.ExistCollationAssetID(strData)){
                                //照合一致数カウントアップ
                                _ProcessData.cntListCnt();
                            }else{
                                //リスト不一致数カウントアップ
                                _ProcessData.cntNoListCnt();
                            }
                        }

                    }

                    break;

                default:
                    break;
            }


            // 読取りデータ件数表示
            UpdateGranteCountDisp();

        } catch (Exception ex) {
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this) + ":" + Common.toString(ex), this);
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.close();
            }
        }

    }

    /**
     * Barcode/QR読取りイベント
     * 親クラスから呼ばれます。
     * @param strData
     */
    @Override
    protected void BarcodeDataReceived(String strData) {
        SQLiteDatabase db = null;
        try {
            //既に読込済みの場合終了
            if (_ProcessData.ExistAssetNo(strData)) {
                return;
            }

            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //資産マスタに存在する資産の場合、タグ読取済みかどうかをチェック
            if (_ProcessData.ExistMasterAssetNo(strData)) {
                AssetModel asset = AssetFactory.Select(db, strData);

                //既に読込済みの場合終了
                if (_ProcessData.ExistAssetID(asset.getAssetID())) {
                    return;
                }
            }

            //資産番号情報を溜め込む
            _ProcessData.StockAssetNo(strData);
            Log.d("資産番号", strData);

            //参照権限チェック
            //参照権限の無い資産の場合
            if(!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, strData), CommonAuthority.Auth.readHubList)) {
                //読み捨てる（一覧表示は行う）
                return;
            }

            //メニュー画面で選択した機能へ遷移する
            switch (_MenuClick) {
                case CommonConst.VIEW_WORD_MENU_LENDING:        //貸出(照合)

                    //マスタに存在する場合、資産IDを溜め込む
                    if (_ProcessData.ExistMasterAssetNo(strData)) {
                        //20210918 ADD Takase V2.0 START
                        AssetModel asset = AssetFactory.Select(db, strData);
                        // (貸出の時貸出状態チェック）　&& 更新権限チェック
                        if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_LEASE.getString())
                                || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {

                            //貸出ステータス2の場合
                            if(!CommonConst.ASSET_STATUS.Lease.getString().equals(asset.getAssetStatus())){
                                //資産IDを溜め込む
                                _ProcessData.StockViewAssetData(strData);
                            }
                        }
                    }
                    break;

                case CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST:   //仮置登録

                    //マスタに存在する場合、資産IDを溜め込む
                    if (_ProcessData.ExistMasterAssetNo(strData)) {
                        //更新権限チェック
                        AssetModel asset = AssetFactory.Select(db, strData);
                        // (貸出の時貸出状態チェック）　&& 更新権限チェック
                        if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())
                                || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {
                            //カウント音を出す
                            if (beepSound == null) {
                                beepSound = new BeepSound(this);
                            }
                            beepSound.BeepCountUp();

                            //資産IDを溜め込む
                            _ProcessData.StockViewAssetData(strData);

//                            //照合データに含まれていない場合チェック
//                            if(!_ProcessData.ExistCollationAssetNo(strData)){
//                                //リスト不一致数カウントアップ
//                                _ProcessData.cntNoListCnt();
//
//                            }else{
//                                //ほかで仮置き登録済みである場合
//                                if(_ProcessData.ExistTempListAssetNo(strData)){
//                                    //他仮置き数カウントアップ
//                                    _ProcessData.cntAlreadyTempCnt();
//                                }
//                            }

                            //ほかで仮置き登録済みである場合
                            if(_ProcessData.ExistTempListAssetNo(strData)){
                                //他仮置き数カウントアップ
                                _ProcessData.cntAlreadyTempCnt();
                            }else{
                                //照合データに含まれていない場合チェック
                                if(!_ProcessData.ExistCollationAssetNo(strData)){
                                    //リスト不一致数カウントアップ
                                    _ProcessData.cntNoListCnt();

                                }
                            }

                        }
                    }

                    break;
                case CommonConst.VIEW_WORD_MENU_MATCHIN:           //持込照合
                case CommonConst.VIEW_WORD_MENU_MATCHOUT:          //持帰照合
                case CommonConst.VIEW_WORD_MENU_MATCHRETURN:       //照合返却

                    //マスタに存在する場合、資産IDを溜め込む
                    if (_ProcessData.ExistMasterAssetNo(strData)) {

                        //更新権限チェック
                        AssetModel asset = AssetFactory.Select(db, strData);
                        // (貸出の時貸出状態チェック）　&& 更新権限チェック
                        if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())
                                || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {

                            //カウント音を出す
                            if (beepSound == null) {
                                beepSound = new BeepSound(this);
                            }

                            _ProcessData.RemoveCollationViewAssetNo(strData); // 削除処理

                            beepSound.BeepCountUp();
                            //資産IDを溜め込む
                            _ProcessData.StockViewAssetData(strData);

                            //照合データに含まれているかチェック
                            if(_ProcessData.ExistCollationAssetNo(strData)){
                                //照合一致数カウントアップ
                                _ProcessData.cntListCnt();

                            }else{
                                //リスト不一致数カウントアップ
                                _ProcessData.cntNoListCnt();
                            }
                        }

                    }

                    break;

                default:
                    break;
            }

            // 読取りデータ件数表示
            UpdateGranteCountDisp();

            //ボタン活性
            SetButtonEnabled(true, true);

        } catch (Exception ex) {
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this) + ":" + Common.toString(ex), this);
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 各画面の照合対象を取得
     */
    private void MatchData(){
        SQLiteDatabase db = null;
        try {

            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            //仮置き済みデータ削除
            _ProcessData.TempListClear();

            //照合情報クリア
            _ProcessData.CleaCollationData();

            //工事と紐づいて貸出した資産
            _ProcessData.SetCollationData(db,_ConstRuctionNo);

            //今回貸出した資産追加
            _ProcessData.SetCollationLentData(db,_ConstRuctionNo);

            //すべての仮置き情報取得
            _ProcessData.SetAllCollationTempDataAdd(db);

            //メニュー画面で選択した機能へ遷移する
            switch (_MenuClick) {
                case CommonConst.VIEW_WORD_MENU_MATCHOUT:          //持帰照合
                case CommonConst.VIEW_WORD_MENU_MATCHRETURN:       //照合返却

                    //今回仮置きした資産(持帰/返却用)
                    _ProcessData.SetCollationTempDataAdd(db,_ConstRuctionNo);

                    _ProcessData.SetCollationTempListDataAdd(db,_ConstRuctionNo);

                    break;

                case CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST:          //仮置登録
                    //仮置き済みデータ削除
                    _ProcessData.TempListClear();

                    //選択中の仮置きID以外のすべての仮置き情報取得
                    _ProcessData.SetNotTempCollationTempDataAdd(db,_TempId);

                default:
                    break;
            }

            ((TextView)findViewById(R.id.labItemCarriedCnt))
                    .setText(Integer.toString(_ProcessData.CountmapCollation()));       //件数

        } catch (Exception ex) {
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this) + ":" + Common.toString(ex), this);
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.close();
            }
        }

    }

    /**
     * 件数表示のリセット
     */
    protected void ResetCountDisp(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);
        SQLiteDatabase db = null;

        //読取件数のクリア
        ((TextView)findViewById(R.id.labReadCountVal)).setText("0");

        try {
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            String sql;
            Cursor cursor;
            //メニューで押したボタン仮置きの場合
            switch (_MenuClick) {
                case CommonConst.VIEW_WORD_MENU_MATCHRETURN:   //照合返却
                    //HT蓄積件数の取得
                    sql = RetAseetListFactory.GetAssetNoData(_ConstRuctionNo);
                    //SQL発行
                    cursor = db.rawQuery(sql, null);
                    while (cursor.moveToNext()) {
                        //取得する
                        String AssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));
                        String ContMatchDiv = cursor.getString(cursor.getColumnIndex("CONT_MATCH_DIV"));

                        //照合結果区分が未読取の場合
                        if(ContMatchDiv.equals(CommonConst.CONST_MATCH_DIV.NG.getString())){
                            //処理を飛ばす
                            continue;
                        }

                        //資産番号から資産ID
                        String AssetId = AssetFactory.ConvertId(db,AssetNo);

                        //読み取り機能がRFIDの時
                        if(_isRFIDReaderUse == true){
                            //既に読込済みの場合終了
                            if (_ProcessData.ExistAssetID(AssetId)) {
                                continue;
                            }

                            //資産マスタに存在する資産の場合、QR読取済みかどうかをチェック
                            if (_ProcessData.ExistMasterAssetID(AssetId)) {
                                //既に読込済みの場合終了
                                if (_ProcessData.ExistAssetNo(AssetNo)) {
                                    continue;
                                }
                            }
                            //資産番号情報を溜め込む
                            _ProcessData.StockAssetNo(AssetNo);
                            Log.d("資産番号", AssetNo);

                            //マスタに存在する場合、資産IDを溜め込む
                            if (_ProcessData.ExistMasterAssetID(AssetId)) {

                                //貸出、仮置きの場合は、資産にない情報も取得
    //                        //資産ID情報を溜め込む
    //                        _ProcessData.StockAssetID(strData);

                                //更新権限チェック
                                AssetModel asset = AssetFactory.Select(db, AssetNo);
                                // (貸出の時貸出状態チェック）　&& 更新権限チェック
                                if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())
                                        || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                        && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {

                                    //カウント音を出す
                                    if (beepSound == null) {
                                        beepSound = new BeepSound(this);
                                    }

                                    _ProcessData.RemoveCollationViewAssetNo(AssetNo); // 削除処理

                                    beepSound.BeepCountUp();
                                    //資産IDを溜め込む
                                    _ProcessData.StockViewAssetData(AssetId);

                                    //照合データに含まれているかチェック
                                    if(_ProcessData.ExistCollationAssetID(AssetId)){
                                        //照合一致数カウントアップ
                                        _ProcessData.cntListCnt();
                                    }else{
                                        //リスト不一致数カウントアップ
                                        _ProcessData.cntNoListCnt();

                                    }
                                }

                            }
                        }else {
                            //既に読込済みの場合終了
                            if (_ProcessData.ExistAssetNo(AssetNo)) {
                                continue;
                            }

                            //資産マスタに存在する資産の場合、タグ読取済みかどうかをチェック
                            if (_ProcessData.ExistMasterAssetNo(AssetNo)) {

                                //既に読込済みの場合終了
                                if (_ProcessData.ExistAssetID(AssetId)) {
                                    continue;
                                }
                            }

                            //資産番号情報を溜め込む
                            _ProcessData.StockAssetNo(AssetNo);
                            Log.d("資産番号", AssetNo);

                            //マスタに存在する場合、資産IDを溜め込む
                            if (_ProcessData.ExistMasterAssetNo(AssetId)) {

                                //貸出、仮置きの場合は、資産にない情報も取得
//                        //資産ID情報を溜め込む
//                        _ProcessData.StockAssetID(strData);

                                //更新権限チェック
                                AssetModel asset = AssetFactory.Select(db, AssetId);
                                // (貸出の時貸出状態チェック）　&& 更新権限チェック
                                if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())
                                        || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                        && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {

                                    //カウント音を出す
                                    if (beepSound == null) {
                                        beepSound = new BeepSound(this);
                                    }

                                    _ProcessData.RemoveCollationViewAssetNo(AssetId); // 削除処理

                                    beepSound.BeepCountUp();
                                    //資産IDを溜め込む
                                    _ProcessData.StockViewAssetData(AssetId);

                                    //照合データに含まれているかチェック
                                    if(_ProcessData.ExistCollationAssetNo(AssetId)){
                                        //照合一致数カウントアップ
                                        _ProcessData.cntListCnt();

                                    }else{
                                        //リスト不一致数カウントアップ
                                        _ProcessData.cntNoListCnt();

                                    }
                                }

                            }
                        }

                    }
                    //件数を追加
                    ((TextView)findViewById(R.id.labReadCountVal)).setText(Integer.toString(_ProcessData.CountViewAll()));

                    cursor.close();
                    break;

                case CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST:   //仮置登録
                    //HT蓄積件数の取得
                    sql = TemporaryFactory.GetTempIdData(db,_ConstRuctionNo,_TempId);

                    //SQL発行
                    cursor = db.rawQuery(sql, null);

                    while (cursor.moveToNext()) {
                        //取得する
                        String AssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));       //表示、非表示

                        //資産番号から資産ID
                        String AssetId = AssetFactory.ConvertId(db,AssetNo);       //表示、非表示

                        //読み取り機能がRFIDの時
                        if(_isRFIDReaderUse == true){
                            //RFIDモードの時

                            //既に読込済みの場合終了
                            if (_ProcessData.ExistAssetID(AssetId)) {
                                continue;
                            }

                            //資産ID情報を溜め込む
                            _ProcessData.StockAssetID(AssetId);
                            Log.d("資産ID", AssetId);

                            //資産マスタに存在する資産の場合、QR読取済みかどうかをチェック
                            if (_ProcessData.ExistMasterAssetID(AssetId)) {
                                //既に読込済みの場合終了
                                if (_ProcessData.ExistAssetNo(AssetNo)) {
                                    continue;
                                }
                            }

                            //マスタに存在する場合、資産IDを溜め込む
                            if (_ProcessData.ExistMasterAssetID(AssetId)) {
                                //更新権限チェック
                                AssetModel asset = AssetFactory.Select(db, AssetNo);
                                // (貸出の時貸出状態チェック）　&& 更新権限チェック
                                if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())
                                        || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                        && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {
                                    //カウント音を出す
                                    if (beepSound == null) {
                                        beepSound = new BeepSound(this);
                                    }
                                    beepSound.BeepCountUp();

                                    //資産IDを溜め込む
                                    _ProcessData.StockViewAssetData(AssetId);



//                                    //照合データに含まれていない場合チェック
//                                    if(!_ProcessData.ExistCollationAssetID(AssetId)){
//                                        //リスト不一致数カウントアップ
//                                        _ProcessData.cntNoListCnt();
//
//                                    }else{
//                                        //ほかで仮置き登録済みである場合
//                                        if(_ProcessData.ExistTempListAssetNo(AssetNo)){
//                                            //他仮置き数カウントアップ
//                                            _ProcessData.cntAlreadyTempCnt();
//
//                                        }
//                                    }

                                    if(_ProcessData.ExistTempListAssetNo(AssetNo)){
                                            //他仮置き数カウントアップ
                                            _ProcessData.cntAlreadyTempCnt();
                                    }else {
                                        if(!_ProcessData.ExistCollationAssetNo(AssetNo)){
                                        //リスト不一致数カウントアップ
                                        _ProcessData.cntNoListCnt();
                                        }
                                    }

                                }
                            }

                        }else{
                            //バーコードモードの時
                            //既に読込済みの場合終了
                            if (_ProcessData.ExistAssetNo(AssetNo)) {
                                continue;
                            }

                            //資産マスタに存在する資産の場合、タグ読取済みかどうかをチェック
                            if (_ProcessData.ExistMasterAssetNo(AssetNo)) {

                                //既に読込済みの場合終了
                                if (_ProcessData.ExistAssetID(AssetId)) {
                                    continue;
                                }
                            }

                            //資産番号情報を溜め込む
                            _ProcessData.StockAssetNo(AssetNo);

                            Log.d("資産番号", AssetNo);

                            //マスタに存在する場合、資産IDを溜め込む
                            if (_ProcessData.ExistMasterAssetNo(AssetNo)) {
                                //更新権限チェック
                                AssetModel asset = AssetFactory.Select(db, AssetNo);
                                // (貸出の時貸出状態チェック）　&& 更新権限チェック
                                if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_MATCHLEASE.getString())
                                        || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                        && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {
                                    //カウント音を出す
                                    if (beepSound == null) {
                                        beepSound = new BeepSound(this);
                                    }
                                    beepSound.BeepCountUp();
                                    //照合データに含まれていない場合チェック
                                    if(!_ProcessData.ExistCollationAssetNo(AssetNo)){
                                        //リスト不一致数カウントアップ
                                        _ProcessData.cntNoListCnt();
                                    }else{
                                        //ほかで仮置き登録済みである場合
                                        if(_ProcessData.ExistTempListAssetNo(AssetNo)){
                                            //他仮置き数カウントアップ
                                            _ProcessData.cntAlreadyTempCnt();
                                            //ほか仮置きは登録データに入れない
                                            continue;
                                        }
                                    }

                                    //資産IDを溜め込む
                                    _ProcessData.StockViewAssetData(AssetNo);

                                }
                            }
                        }
                    }

                    //件数を追加
                    ((TextView)findViewById(R.id.labReadCountVal)).setText(Integer.toString(_ProcessData.CountViewAll()));

                    cursor.close();
                    break;

                case CommonConst.VIEW_WORD_MENU_LENDING:   //貸出(照合)
                    //HT蓄積件数の取得
                    sql = LentAssetListFactory.GetAssetNoData(_ConstRuctionNo);

                    cursor = db.rawQuery(sql, null);

                    while (cursor.moveToNext()){
                        //取得する
                        String AssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));

                        //資産番号から資産ID
                        String AssetId = AssetFactory.ConvertId(db,AssetNo);

                        //既に読込済みの場合終了
                        if (_ProcessData.ExistAssetID(AssetId)) {
                            continue;
                        }

                        //資産マスタに存在する資産の場合、QR読取済みかどうかをチェック
                        if (_ProcessData.ExistMasterAssetID(AssetId)) {
                            //既に読込済みの場合終了
                            if (_ProcessData.ExistAssetNo(AssetNo)) {
                                continue;
                            }
                        }
                        //資産番号情報を溜め込む
                        _ProcessData.StockAssetNo(AssetNo);
                        Log.d("資産番号", AssetNo);

                        //マスタに存在する場合、資産IDを溜め込む
                        if (_ProcessData.ExistMasterAssetNo(AssetNo)) {
                            //20210918 ADD Takase V2.0 START
                            AssetModel asset = AssetFactory.Select(db, AssetNo);
                            // (貸出の時貸出状態チェック）　&& 更新権限チェック
                            if ((!CommonAuthority.FunctionId.getFunctionId().equals(CommonAuthority.FunctionId.FUNCTION_ID.FUNCTION_ID_LEASE.getString())
                                    || CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus()))
                                    && CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {

                                //貸出ステータス2の場合
                                if(!CommonConst.ASSET_STATUS.Lease.getString().equals(asset.getAssetStatus())){
                                    //資産IDを溜め込む
                                    _ProcessData.StockViewAssetData(AssetNo);
                                }
                            }
                        }
                    }

                    //件数を追加
                    ((TextView)findViewById(R.id.labReadCountVal)).setText(Integer.toString(_ProcessData.CountViewAll()));

                    cursor.close();

                    break;
            }

        }catch (Exception ex) {
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this) + ":" + Common.toString(ex), this);
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        if (params == null) {
            return;
        }

        //呼出元で分岐する
        switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
            case BUTTON_CLEAR:      //クリアボタン
                //ほか仮置き数
                _ProcessData.ClearAlreadyTempCnt();
                //リスト不一致数クリア
                _ProcessData.ClearNoListCnt();
                //照合一致数クリア
                _ProcessData.ClearListCnt();

                //ソートデータクリア
                _ProcessData.ClearSortData();

                //仮置き、照合返却、貸出データ削除
                DataClear();

                //クリア処理
                ClearRfidTagList();
                break;
            case BUTTON_RETURN:     //戻るボタン
                setResult(RESULT_CANCELED, ResIntent);      //キャンセルを返す

                //仮置き、照合返却、貸出データ削除
                DataClear();

                onReadReturn();
                break;
            case BUTTON_MENU:       //メニューボタン
                ReaderOff();

                //仮置き、照合返却、貸出データ削除
                DataClear();

                onMenuReturn();
                break;
        }

    }

    /**
     * 一覧画面から戻るイベント処理をします。
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data){
        try {
            switch(requestCode) {
                case BUTTON_SUMMARY:        //一覧画面から戻る
                    if(resultCode == RESULT_OK) {
                        //前画面からデータを取得する
                        //_ProcessData = (ProcessFactory)data.getSerializableExtra(CommonConst.VIEW_KEY_PROCESS_DATA);

                        if (_ProcessData.CountAll() <= 0) {
                            //ボタンを制御する
                            SetButtonEnabled(false, false);

                            //照合
                            MatchData();
                        }

                        //バーコードの場合、読取開始
                        if (!_TagMode) {
                            ReaderOn();
                        }
                    }

                    if(resultCode == RESIST_OK){

                        setResult(RESIST_OK);
                        //終了する
                        this.finish();

                    }

                    //ボタンを活性
                    SetButtonEnabled(true, true);
                    AllResetButton(true);
                    break;

                case BUTTON_SETTEI:
                    //電波強度再表示
                    ((TextView) findViewById(R.id.txtIntencityVal)).setText(C_Setting.getBTReadIntensity(this, GetSettingKey()));
                    break;

                default:
                    break;
            }

        } catch (Exception ex) {
            //ShowErrorMessage(ex.getMessage(), "");
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /** ボタン有効/無効設定
     * @param prmEnabled  有効/無効
     * @param blnTagMode  タグモードの有効/無効
     * */
    protected void SetButtonEnabled(boolean prmEnabled, boolean blnTagMode) {
        //0件の場合非活性
        if (_ProcessData.CountAll() <= 0) {
            prmEnabled = false;
        }

        if (findViewById(R.id.btnDetail) != null) {


            if(_MenuClick != null) {
                if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN) ||
                        _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT) ||
                        _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)) {

                    //照合対象が0件より大きい場合
                    if(_ProcessData.CountmapCollation() > 0){
                        //詳細表示ボタン
                        ((Button) findViewById(R.id.btnDetail)).setEnabled(true);
                    }else{
                        //詳細表示ボタン
                        ((Button) findViewById(R.id.btnDetail)).setEnabled(prmEnabled);
                    }
                }else{
                    //詳細表示ボタン
                    ((Button) findViewById(R.id.btnDetail)).setEnabled(prmEnabled);
                }
            }else{
                //詳細表示ボタン
                ((Button) findViewById(R.id.btnDetail)).setEnabled(prmEnabled);
            }
        }

        if (findViewById(R.id.btnClear) != null) {
            //クリアボタン
            ((Button) findViewById(R.id.btnClear)).setEnabled(prmEnabled);
        }

        if (findViewById(R.id.switchTagMode) != null) {
            //スイッチ
            ((Switch) findViewById(R.id.switchTagMode)).setEnabled(blnTagMode);
        }

        final boolean blnEnabled = prmEnabled;
        runOnUiThread(new Runnable() {	//20200121 Add Thread Sasaki
            @Override
            public void run() {
                if (blnEnabled == false) {
                    //詳細表示ボタン
                    ((Button) findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button_diable));
                    ((Button) findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#a9a9a9"));

                    //クリアボタン
                    Drawable leftDrawable = getResources().getDrawable(R.drawable.trash_48_disable);
                    leftDrawable.setBounds(0, 0, leftDrawable.getMinimumWidth(), leftDrawable.getMinimumHeight());
                    Button clear = findViewById(R.id.btnClear);
                    clear.setBackground(getDrawable(R.drawable.execute_button_clear_disable));
                    clear.setCompoundDrawables(null, leftDrawable, null, null);
                    clear.setTextColor(Color.parseColor("#a9a9a9"));

                } else {
                    //詳細表示ボタン
                    ((Button) findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button));
                    ((Button) findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#ffffff"));

                    //クリアボタン
                    Drawable leftDrawable = getResources().getDrawable(R.drawable.trash_48);
                    leftDrawable.setBounds(0, 0, leftDrawable.getMinimumWidth(), leftDrawable.getMinimumHeight());
                    Button clear = findViewById(R.id.btnClear);
                    clear.setBackground(getDrawable(R.drawable.execute_button_clear));
                    clear.setCompoundDrawables(null, leftDrawable, null, null);
                    clear.setTextColor(Color.parseColor("#ffffff"));
                }

                if(_MenuClick != null) {
                    if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN) ||
                            _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT) ||
                            _MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)) {

                        //照合対象が0件より大きい場合
                        if(_ProcessData.CountmapCollation() > 0){
                            //詳細表示ボタン
                            ((Button) findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button));
                            ((Button) findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#ffffff"));
                        }else{
                            //詳細表示ボタン
                            ((Button) findViewById(R.id.btnDetail)).setBackground(getDrawable(R.drawable.execute_button_diable));
                            ((Button) findViewById(R.id.btnDetail)).setTextColor(Color.parseColor("#a9a9a9"));
                        }
                    }
                }
            }
        });
        //タグモードボタン(TAGモードの場合)
        if (CommonConst.READING_MEDIUM.TAG.getString().equals(_SettingTagMode)) {

            if (findViewById(R.id.switchTagMode) != null) {
                if (_TagReadStartFlag) {
                    ((Switch) findViewById(R.id.switchTagMode)).setEnabled(false);
                } else {
                    ((Switch) findViewById(R.id.switchTagMode)).setEnabled(true);
                }
            }
        } else {
            if (findViewById(R.id.switchTagMode) != null) {
                ((Switch) findViewById(R.id.switchTagMode)).setEnabled(false);
            }
        }


    }

    /** 照合返却/貸出照合/仮置きの場合データ削除
     */
    private void DataClear(){
        //貸出(照合)の場合
        if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_LENDING)){
            SQLiteDatabase db = null;

            try {
                DatabaseHelper dbHelper = new DatabaseHelper(this);
                db = dbHelper.getWritableDatabase();
                String sql;
                Cursor cursor;

                //工事番号で貸出リストテーブルのデータ取得
                //HT蓄積データの取得
                sql = LentAssetListFactory.GetAseetNoProcessIdData(_ConstRuctionNo);

                cursor = db.rawQuery(sql, null);

                while (cursor.moveToNext()){
                    //取得する
                    String AssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));
                    String ProcessLogId = cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID"));

                    //処理履歴IDで照合リストテーブルを削除
                    db.delete(CommonConst.TABLE_MATCH_LIST, "PROCESS_LOG_ID = ?", new String[] {ProcessLogId});

                    //資産番号で資産マスタテーブルを更新
                    AssetFactory.ClearUpdate(db, AssetNo);

                }
                if (cursor != null) {
                    cursor.close(); // Cursor を閉じる
                }

                //工事番号で貸出リストテーブルのデータ削除
                LentAssetListFactory.ConstNoDelete(db,_ConstRuctionNo);

            }catch (Exception ex) {
                //エラーログ
                if (db != null) {
                    db.close();
                }
                LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                        SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this) + ":" + Common.toString(ex), this);
                showErrorMessage(ex.getMessage());
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            } finally {
                if (db != null) {
                    db.close();
                }
            }
        }

        //仮置きの場合
        if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST)){
            SQLiteDatabase db = null;

            try {
                DatabaseHelper dbHelper = new DatabaseHelper(this);
                db = dbHelper.getWritableDatabase();
                String sql;
                Cursor cursor;

                //工事番号で仮置き資産リストテーブルのデータ取得
                //HT蓄積データの取得
                sql = TempAssetListFactory.GetAseetNoProcessIdTempIdData(_ConstRuctionNo,_TempId);

                cursor = db.rawQuery(sql, null);

                while (cursor.moveToNext()){
                    //取得する
                    String AssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));
                    String ProcessLogId = cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID"));

                    //処理履歴IDで照合リストテーブルを削除
                    db.delete(CommonConst.TABLE_MATCH_LIST, "PROCESS_LOG_ID = ?", new String[] {ProcessLogId});
                    //処理履歴IDで仮置きリストテーブルを削除
                    db.delete(CommonConst.TABLE_TEMP_LIST, "PROCESS_LOG_ID = ?", new String[] {ProcessLogId});
                }
                if (cursor != null) {
                    cursor.close(); // Cursor を閉じる
                }

                //工事番号、仮置きIDで仮置き資産テーブルのデータ削除
                TempAssetFactory.ConstNoTempIdDelete(db,_TempId,_ConstRuctionNo);
                //工事番号、仮置きIDで仮置き資産リストテーブルのデータ削除
                TempAssetListFactory.ConstNoTempIdDelete(db,_ConstRuctionNo,_TempId);

            }catch (Exception ex) {
                //エラーログ
                if (db != null) {
                    db.close();
                }
                LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                        SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this) + ":" + Common.toString(ex), this);
                showErrorMessage(ex.getMessage());
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            } finally {
                if (db != null) {
                    db.close();
                }
            }
        }

        //返却照合の場合
        if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)){
            SQLiteDatabase db = null;

            try {
                DatabaseHelper dbHelper = new DatabaseHelper(this);
                db = dbHelper.getWritableDatabase();
                String sql;
                Cursor cursor;

                //工事番号で返却リストテーブルのデータ取得
                //HT蓄積データの取得
                sql = RetAseetListFactory.GetAseetNoProcessIdData(_ConstRuctionNo);

                cursor = db.rawQuery(sql, null);

                while (cursor.moveToNext()){
                    //取得する
                    String AssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));
                    String ProcessLogId = cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID"));

                    //処理履歴IDで照合リストテーブルを削除
                    db.delete(CommonConst.TABLE_MATCH_LIST, "PROCESS_LOG_ID = ?", new String[] {ProcessLogId});

                }
                if (cursor != null) {
                    cursor.close(); // Cursor を閉じる
                }

                //工事番号で返却リストテーブルのデータ削除
                RetAseetListFactory.ConstNoDelete(db,_ConstRuctionNo);

            }catch (Exception ex) {
                //エラーログ
                if (db != null) {
                    db.close();
                }
                LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                        SystemSetting.getString(CommonConst.VIEW_WORD_ALL_TAG_READ, "", this) + ":" + Common.toString(ex), this);
                showErrorMessage(ex.getMessage());
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            } finally {
                if (db != null) {
                    db.close();
                }
            }
        }
    }
}
