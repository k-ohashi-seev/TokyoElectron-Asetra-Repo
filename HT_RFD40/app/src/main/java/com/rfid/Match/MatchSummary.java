package com.rfid.Match;

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
import com.rfid.Factory.ConstRuctionFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.RetAseetListFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseSummary;
import com.rfid.common.Common;
import com.rfid.common.CommonConfirmDialog;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.EntryProcessDataDetail;
import com.rfidpk.R;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * 貸出詳細表示のクラス
 */
public class MatchSummary extends BaseSummary implements CommonOKDialog.Callback{

    //記号一時保存
    private String _sinbol ="";

    private String _contMatchDiv = "";

    //工事選択No
    private String _ConstRuctionNo = "";
    //工事貸出終了日
    private String _LentEndDt = "";
    private String _MenuClick = "";

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
        Intent intent = getIntent();
        _MenuClick = intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT);

        if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN)){
            setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHIN,"",this));                 //持込照合　タイトル
        }else if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT)){
            setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHOUT,"",this));                //持込照合　タイトル
        }else if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)){
            setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_MATCHRETURN,"",this));             //照合返却　タイトル
        }

        ((TextView)findViewById(R.id.lblSummaryListCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_READ_COUNT, "", this));       //読取済件数
        ((TextView)findViewById(R.id.btnSummaryConfirm))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));        //登録
        ((TextView)findViewById(R.id.btnSummarClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));           //クリア
        ((TextView)findViewById(R.id.lblSummaryListTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ASSETNAME_TITLE, "", this));    //資産番号・名称
        ((TextView)findViewById(R.id.lblSummaryListCountErr))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_UNREAD_COUNT, "", this));    //未読取数
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

        ((TextView) findViewById(R.id.txtSummaryListCountErr)).setText(Long.toString(_ProcessData.CountMatAsset()));        //未読取数
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

        //工事貸出終了日
        _LentEndDt = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT);

        //照合返却の場合
        if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)){
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
        }

        //送信用のデータ取得
        HashMap<String, EntryProcessDataDetail>map = _ProcessData.GetRegisterAsset();

        for (Map.Entry<String, EntryProcessDataDetail> entry : map.entrySet()) {
            String strAssetNo = entry.getKey();
            EntryProcessDataDetail processDataDetail = entry.getValue();;

            //照合結果情報が""の場合
            if(processDataDetail.getContMatchDiv().equals("")){
                //初期化　照合NG
                _contMatchDiv = "";
                //照合結果チェック処理
                //マスタに存在する場合かつ照合情報にある場合(背景灰色)
                if (_ProcessData.ExistMasterAssetNo(strAssetNo)){
                    if(_ProcessData.ExistCollationAssetNo(strAssetNo)){
                        //未読み取り照合リストにない場合
                        if(!_ProcessData.ExistCollationViewAssetNo(strAssetNo)){
                            _contMatchDiv = CommonConst.CONST_MATCH_DIV.OK.getString();
                        }else {
                            _contMatchDiv = CommonConst.CONST_MATCH_DIV.NG.getString();
                        }
                    }else {
                        _contMatchDiv = CommonConst.CONST_MATCH_DIV.OTHER.getString();
                    }
                }else {
                    _contMatchDiv = "";
                }

                //送信用のデータの照合結果更新
                processDataDetail.SetContMatchDiv(_contMatchDiv);
            }
        }


//        // キーを使って値を更新
//        if (map.containsKey(strAssetNo)) {
//            EntryProcessDataDetail ProcessData = map.get(strAssetNo);
//            if (ProcessData != null) {
//                //照合結果追加
//                ProcessData.SetContMatchDiv(_contMatchDiv);
//            }
//        }

        //未読み取り数 + リスト不一致数が0の場合
        String MatchDiv = "";

        //未読み取り件数取得
        TextView txtSummaryListCountErr = findViewById(R.id.txtSummaryListCountErr);
        int MatAssetCnt = Integer.parseInt(txtSummaryListCountErr.getText().toString());

        if(MatAssetCnt == 0 && _ProcessData.getNoListCnt() == 0){
            //照合OK
            MatchDiv = CommonConst.CONST_MATCH_DIV.OK.getString();
        }else{
            //照合NG
            MatchDiv = CommonConst.CONST_MATCH_DIV.NG.getString();
        }
        //照合一致数取得
        int cntOKData = _ProcessData.getListCnt();

        //登録処理
        if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN)){
            _ProcessData.InsertMatProces(db, this,CommonConst.PROCESS_DIV.MatchIn.getString(),_ConstRuctionNo,MatchDiv,cntOKData,_LentEndDt);                //持込照合　
        }else if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT)){
            _ProcessData.InsertMatProces(db, this,CommonConst.PROCESS_DIV.MatchOut.getString(),_ConstRuctionNo,MatchDiv,cntOKData,_LentEndDt);               //持帰照合　
        }else if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)){
            _ProcessData.InsertMatProces(db, this,CommonConst.PROCESS_DIV.MatchReturn.getString(),_ConstRuctionNo,MatchDiv,cntOKData,_LentEndDt);            //照合返却　
        }

        //資産状態を更新
        AssetFactory.Update(db, _ProcessData.GetRegisterAsset(), CommonConst.ASSET_STATUS.Lease);

        //工事内容取得
        String ConstContents = ConstRuctionFactory.GetConstContents(db,_ConstRuctionNo);

        if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN)){
            //イベントログ
            LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                    GetWord(CommonConst.VIEW_WORD_MATCHINDATA_REGISTER)
                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + _ProcessData.CountOKAll()
                            + " 【" + GetWord(CommonConst.VIEW_WORD_CONST_ITEM) + "】" + _ConstRuctionNo
                            + "、" + ConstContents
                    , this);
        }else if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT)){
            //イベントログ
            LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                    GetWord(CommonConst.VIEW_WORD_MATCHOUTDATA_REGISTER)
                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + _ProcessData.CountOKAll()
                            + " 【" + GetWord(CommonConst.VIEW_WORD_CONST_ITEM) + "】" + _ConstRuctionNo
                            + "、" + ConstContents
                    , this);
        }else if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)){
            //イベントログ
            LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                    GetWord(CommonConst.VIEW_WORD_MATCHRETURNDATA_REGISTER)
                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + _ProcessData.CountOKAll()
                            + " 【" + GetWord(CommonConst.VIEW_WORD_CONST_ITEM) + "】" + _ConstRuctionNo
                            + "、" + ConstContents
                    , this);
        }

        //データクリア
        _ProcessData.Clear();

        //メッセージBOX
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_COMPLETE);
        final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録

        if (_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHIN)){
            //完了メッセージ表示
            dialog.message(SystemSetting.getString(
                    CommonConst.VIEW_WORD_MATCHINDATA_REGISTER, "", this));                //持出照合を登録は完了しました。
        }else if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHOUT)){
            //完了メッセージ表示
            dialog.message(SystemSetting.getString(
                    CommonConst.VIEW_WORD_MATCHOUTDATA_REGISTER, "", this));                //持帰照合を登録は完了しました。
        }else if(_MenuClick.equals(CommonConst.VIEW_WORD_MENU_MATCHRETURN)){
            //完了メッセージ表示
            dialog.message(SystemSetting.getString(
                    CommonConst.VIEW_WORD_MATCHRETURNDATA_REGISTER, "", this));                //照合返却を登録は完了しました。
        }

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
                R.layout.custom_matlistview,
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

                //初期化　照合NG
                _contMatchDiv = "";

                //資産状態を取得する
                Map<String, String> item = (HashMap)this.getItem(position);
                String strAssetStatus = item.get(CommonConst.List_VIEW_KEY.Hidden.getString());
                String strAssetNo = item.get(CommonConst.List_VIEW_KEY.Main.getString());

                String strWarning = item.get(CommonConst.List_VIEW_KEY.Aux.getString());        //ワーニング表示(非表示)追加
                strAssetNo = strAssetNo.replace(strWarning, "");
                strAssetNo = strAssetNo.replace("()", "");  //ワーニング除去

                //資産状態によって文字色を変更する(サブクラスで実装する)
                int intColor = SettingListColor(strAssetStatus, strAssetNo, strWarning);          //ワーニング表示(非表示)追加

                //マスタに存在する場合かつ照合情報にある場合(背景灰色)
                if (_ProcessData.ExistMasterAssetNo(strAssetNo)){
                    if(_ProcessData.ExistCollationAssetNo(strAssetNo)){
                        //未読み取り照合リストにない場合
                        if(!_ProcessData.ExistCollationViewAssetNo(strAssetNo)){
                            view.setBackgroundResource(R.color.TagModeOFF);
                            _sinbol = "〇";
                            _contMatchDiv = CommonConst.CONST_MATCH_DIV.OK.getString();
                            item.put(CommonConst.List_VIEW_KEY.Mat.getString(), CommonConst.CONST_MATCH_DIV.OK.getString());
                        }else {
                            //背景色クリア
                            view.setBackground(null);
                            _sinbol = "✕";
                            _contMatchDiv = CommonConst.CONST_MATCH_DIV.NG.getString();
                            item.put(CommonConst.List_VIEW_KEY.Mat.getString(), CommonConst.CONST_MATCH_DIV.NG.getString());
                        }
                    }else {
                        //背景色クリア
                        view.setBackground(null);
                        _sinbol = "✕";
                        _contMatchDiv = CommonConst.CONST_MATCH_DIV.OTHER.getString();
                        item.put(CommonConst.List_VIEW_KEY.Mat.getString(), CommonConst.CONST_MATCH_DIV.OTHER.getString());
                    }
                }else {
                    //背景色クリア
                    view.setBackground(null);
                    _sinbol = "✕";
                    _contMatchDiv = "";
                    item.put(CommonConst.List_VIEW_KEY.Mat.getString(), "");
                }

                //送信用のデータ取得
                HashMap<String, EntryProcessDataDetail>map = _ProcessData.GetRegisterAsset();

                // キーを使って値を更新
                if (map.containsKey(strAssetNo)) {
                    EntryProcessDataDetail ProcessData = map.get(strAssetNo);
                    if (ProcessData != null) {
                        //照合結果追加
                        ProcessData.SetContMatchDiv(_contMatchDiv);
                    }
                }

                //色を設定
                ((TextView) view.findViewById(android.R.id.text1)).setTextColor(intColor);      //資産番号
                ((TextView) view.findViewById(android.R.id.text2)).setTextColor(intColor);      //資産名称

                ((TextView)view.findViewById(R.id.textsymbol))
                        .setText(_sinbol);    //読み取り済みか否か

                //前回の選択があれば色を変える
                if (position == previousSelectedViewPosition){
                    // 現在選択されたアイテムの背景色を変更
                    view.setBackgroundResource(R.color.ButtonChange); // 直接背景色を変更
                }

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


    protected void CreateTempTable() {
        SQLiteDatabase db = null;

        try {
            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" CREATE TEMP TABLE ");
            sql.append(CommonConst.TEMP_SEARCH);
            sql.append(" (");
            sql.append("  ASSET_NO");                      //資産番号
            sql.append("  ) ");

            //SQL発行
            db.execSQL(sql.toString());

            for(String assetNo : _SearchData.GetAssetNo()){
                //SQL作成
                StringBuilder sqlInsert = new StringBuilder();
                sqlInsert.append(" INSERT");
                sqlInsert.append(" INTO ");
                sqlInsert.append(CommonConst.TEMP_SEARCH);
                sqlInsert.append(" (");
                sqlInsert.append("  ASSET_NO");                //資産番号
                sqlInsert.append("  ) ");
                sqlInsert.append(" VALUES (");
                sqlInsert.append(" ?");
                sqlInsert.append(" )");

                //SQL発行
                db.execSQL(sqlInsert.toString(), new String[]{
                        assetNo             //資産番号
                });

                _SearchData.SelectAssetData(db);
            }
        }catch (Exception ex) {
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


    }
}
