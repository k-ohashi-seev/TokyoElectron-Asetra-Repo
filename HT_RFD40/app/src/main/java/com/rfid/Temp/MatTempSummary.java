package com.rfid.Temp;

import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.widget.SimpleAdapter;
import android.widget.TextView;

import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Factory.TempAssetFactory;
import com.rfid.Factory.TempAssetListFactory;
import com.rfid.Factory.TemporaryFactory;
import com.rfid.common.BaseSummary;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfidpk.R;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * 貸出詳細表示のクラス
 */
public class MatTempSummary extends BaseSummary implements CommonOKDialog.Callback{

    //工事選択No
    private String _ConstRuctionNo = "";
    //仮置きID
    private String _TempId = "";

    //仮置き登録 or 仮置き変更　チェック
    private String _TempSelectState = "";

    //工事貸出終了日
    private String _LentEndDt = "";

    public static final int RESIST_OK = 5;

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

    /**
     * 文言表示
     */
    protected void InitializeWord() {

        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST,"",this));                 //仮置き　タイトル


        ((TextView)findViewById(R.id.lblSummaryListCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_READ_COUNT, "", this));       //読取済件数
        ((TextView)findViewById(R.id.btnSummaryConfirm))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));        //登録
        ((TextView)findViewById(R.id.btnSummarClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));           //クリア
        ((TextView)findViewById(R.id.lblSummaryListTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ASSETNAME_TITLE, "", this));    //資産番号・名称
        ((TextView)findViewById(R.id.lblSummaryListCountErr))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_OTHER_TEMP_COUNT, "", this));    //他仮置き
        ((TextView)findViewById(R.id.lblSummaryListNoAuthority))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_MISMATCH_COUNT, "", this));    //リスト不一致数



    }

    /**
     * 件数追加
     */
    protected  void ClearCountView(){
        //件数表示
        ((TextView) findViewById(R.id.txtSummaryListCount)).setText(
                Long.toString(_ProcessData.CountAll()));                                        //件数

        ((TextView) findViewById(R.id.txtSummaryListCountErr)).setText(Long.toString(_ProcessData.getAlreadyTempCnt()));      //ほか仮置き数
        ((TextView) findViewById(R.id.txtSummaryListNoAuthority)).setText(Long.toString(_ProcessData.getNoListCnt()));        //リスト不一致数

    }

    /**
     * 登録処理
     * サブクラスで実装する
     */
    protected void RegisterProcess(SQLiteDatabase db){

        Intent intent = getIntent();
        //工事選択No取得
        _ConstRuctionNo = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO);

        //仮置き登録したID登録or更新
        _TempId = intent.getStringExtra(CommonConst.VIEW_KEY_HT_TEMP_ID);

        //工事貸出終了日
        _LentEndDt = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT);

        //新規登録か変更か
        _TempSelectState = intent.getStringExtra(CommonConst.VIEW_KEY_STATE_SELECT);

        //登録する前に工事情報が同じものを一回すべて破棄
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

        //登録処理
        _ProcessData.InsertMatProcesTemp(db, this,CommonConst.PROCESS_DIV.MatchTemp.getString(),_ConstRuctionNo,_TempId,_LentEndDt);                //仮置き登録　

        //イベントログ
        LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                GetWord(CommonConst.VIEW_WORD_TEMPDATA_REGISTER)
                        + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + _ProcessData.CountOKAll()
                        + " 【" + GetWord(CommonConst.VIEW_WORD_CONSTRUCTION_SET_WORK_DAY) + "】" + _ProcessData.getTempDt()
//                        + " 【" + GetWord(CommonConst.VIEW_WORD_CONSTRUCTION_SET_LIABILITY) + "】" + _ProcessData.getTempWorker()
                        + " 【" + GetWord(CommonConst.VIEW_WORD_TEMP_WORK_LOC) + "】" + _ProcessData.getTempInfo1()
                , this);

        //データクリア
        _ProcessData.Clear();

        //メッセージBOX
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_COMPLETE);
        final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
        //完了メッセージ表示
        dialog.message(SystemSetting.getString(
                CommonConst.VIEW_WORD_TEMPDATA_REGISTER, "", this));                //仮置き登録は完了しました。
        dialog.params(bundle);
        dialog.show();
    };

    /**
     * 状態ステータスによってリストビューの文字色を返す
     * サブクラスで実装する
     * @param strAssetStatus
     * @param strAssetNo
     * @return
     */
    //protected int SettingListColor(String strAssetStatus) {
    //protected int SettingListColor(String strAssetStatus, String strAssetNo) {
    protected int SettingListColor(String strAssetStatus, String strAssetNo, String strWarning) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        int intColor = Color.BLACK;
        SQLiteDatabase db = null;

        try {
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            if (strAssetStatus == null || "".equals(strAssetStatus)) {
                //マスタなしの場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_ASSET_NO_REGIST));
            }
            else if (!_ProcessData.ExistCollationAssetNo(strAssetNo)){
                //読み取りありかつ照合対象外
                intColor =Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_ASSET_NO_REGIST));

            }
            else {
                intColor = Color.BLACK;
            }


        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            if (db != null) {
                db.close();
            }
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        } finally {
            if (db != null) {
                db.close();
            }
        }
        return intColor;
    }

    /**
     * 登録ボタンOK後の画面遷移処理
     */
    protected void onReturnComplete() {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        setResult(RESIST_OK);

        this.finish();

    }

    /**
     * 警告メッセージコードを返却します
     * @return
     */
    @Override
    protected String GetAlertMessage(){
        return CommonConst.VIEW_WORD_ALL_CONFIRM_LEASE_DATA;
    }

    public void onMyDialogCancelled(int requestCode, Bundle params) {}


    /**
     * 一覧表示の初期化処理
     * @return 一覧表示用Adapter
     */
    protected SimpleAdapter InitializeAdapter(List list){

        //Adapterの設定   //ワーニングキーとワーニング表示Text(非表示)追加     //20230907 ADD okubo v2.4.1
        SimpleAdapter adapter = new SimpleAdapter(
                this,
                list,
                //android.R.layout.simple_list_item_2,
                R.layout.custom_listview,
                new String[] {CommonConst.List_VIEW_KEY.Main.getString(), CommonConst.List_VIEW_KEY.Sub.getString(), CommonConst.List_VIEW_KEY.Hidden.getString(), CommonConst.List_VIEW_KEY.Aux.getString()},
                new int[] {android.R.id.text1, android.R.id.text2, R.id.text3, R.id.textAux}
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

                //背景色の色を設定する(1行ごとに色を変える)
                if (position % 2 == 0){
                    view.setBackgroundResource(R.color.ListViewBack);
                }
                else {
                    view.setBackgroundResource(R.color.ListViewBack2);
                }

                //資産状態を取得する
                Map<String, String> item = (HashMap)this.getItem(position);
                String strAssetStatus = item.get(CommonConst.List_VIEW_KEY.Hidden.getString());
                String strAssetNo = item.get(CommonConst.List_VIEW_KEY.Main.getString());

                String strWarning = item.get(CommonConst.List_VIEW_KEY.Aux.getString());        //ワーニング表示(非表示)追加
                strAssetNo = strAssetNo.replace(strWarning, "");
                strAssetNo = strAssetNo.replace("()", "");  //ワーニング除去

                //資産状態によって文字色を変更する(サブクラスで実装する)
                int intColor = SettingListColor(strAssetStatus, strAssetNo, strWarning);          //ワーニング表示(非表示)追加

                //マスタに存在する場合かつ照合情報にあるかつ登録済みの場合にない場合
                if (_ProcessData.ExistMasterAssetNo(strAssetNo) && _ProcessData.ExistCollationAssetNo(strAssetNo) && _ProcessData.ExistTempListAssetNo(strAssetNo)){
                        view.setBackgroundResource(R.color.TagModeOFF);

                }else {
                    //背景色クリア
                    //背景色の色を設定する(1行ごとに色を変える)
                    if (position % 2 == 0){
                        view.setBackgroundResource(R.color.ListViewBack);
                    }
                    else {
                        view.setBackgroundResource(R.color.ListViewBack2);
                    }
                }

                //色を設定
                ((TextView) view.findViewById(android.R.id.text1)).setTextColor(intColor);      //資産番号
                ((TextView) view.findViewById(android.R.id.text2)).setTextColor(intColor);      //資産名称

                return view;
            }
        };

        return adapter;
    }

    /**
     * 確認メッセージを表示する
     * @param intButton
     * @param strMessage
     */
    private void ConfirmMessage(int intButton, String strMessage) {
        //メッセージBOX
        final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, intButton);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
        dialog.message(SystemSetting.getString(strMessage, "", this));
        dialog.params(bundle);
        dialog.show();
    }
}
