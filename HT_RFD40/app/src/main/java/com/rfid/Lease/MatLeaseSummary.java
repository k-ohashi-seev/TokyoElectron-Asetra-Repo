package com.rfid.Lease;

import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.widget.TextView;

import com.rfid.Factory.AssetFactory;
import com.rfid.Factory.LentAssetListFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.BaseSummary;
import com.rfid.common.Common;
import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonMatRead;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.ConstRuction;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfidpk.R;

/**
 * 貸出詳細表示のクラス
 */
public class MatLeaseSummary extends BaseSummary implements CommonOKDialog.Callback{

    //工事選択No
    private String _ConstRuctionNo = "";
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

        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_LENDING,"",this));                 //貸出　タイトル
        ((TextView)findViewById(R.id.lblSummaryListCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_COUNT, "", this));           //件数

        //登録ボタン
        ((TextView)findViewById(R.id.btnSummaryConfirm))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));        //登録

        ((TextView)findViewById(R.id.btnSummarClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));           //クリア
        ((TextView)findViewById(R.id.lblSummaryListTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ASSETNAME_TITLE, "", this));    //資産番号・名称
        ((TextView)findViewById(R.id.lblSummaryListCountErr))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_MASTER_NONE, "", this));    //マスタ未登録件数
        ((TextView)findViewById(R.id.lblSummaryListNoAuthority))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_ERR_NO_AUTHORITY, "", this));    //貸出不可(権限無し)
        ((TextView)findViewById(R.id.lblSummaryLeaseLimitWarning))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_LIMIT_WARNING, "", this));    //貸出上限警告件数
        ((TextView)findViewById(R.id.lblSummaryLeaseLimitErr))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_LIMIT_COUNT, "", this));    //貸出上限エラー件数

        //HT貸出時のアラート表示ありの場合
        if (_LeaseAlertDisplay) {
            ((TextView)findViewById(R.id.lblSummaryLeaseLimit))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_LEASE_LIMIT_ERROR, "", this));    //リース期限エラー
            ((TextView)findViewById(R.id.lblSummaryGenericLimit))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_GENERIC_LIMIT_ERROR, "", this));  //○○期限エラー
        }
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

        //登録する前に工事情報が同じものを一回すべて破棄

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


        //登録処理
        _ProcessData.InsertMatProcesLease(db, this,CommonConst.PROCESS_DIV.Lease.getString(),_ConstRuctionNo,_LentEndDt);

        //資産状態を更新
        AssetFactory.Update(db, _ProcessData.GetRegisterAsset(), CommonConst.ASSET_STATUS.Lease);

        //イベントログ
        LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                GetWord(CommonConst.VIEW_WORD_LEASE_LIST_MSG_REGISTER)
                        + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + _ProcessData.CountOKAll()
                        + " 【" + GetWord(CommonConst.VIEW_WORD_LEASE_DESTINATION) + "】" + _ProcessData.getLeaseDestination()
                        + " " + _ProcessData.getLeaseDestinationName()
                        + " 【" + GetWord(CommonConst.VIEW_WORD_LEASE_RESPONSIBLE) + "】" + _ProcessData.getLeaseEmployee()
                        + " " + _ProcessData.getLeaseEmployeeName()
                        + " 【" + GetWord(CommonConst.VIEW_WORD_LEASE_RETURN) + "】" + _ProcessData.getReturnDate()
                , this);

        //データクリア
        _ProcessData.Clear();

        if(SettingFactory.GetMatchingReserveFunction(db).equals(CommonConst.MATCHING_RESERVE_FUNCTION.NO.getString())){
            //メッセージBOX
            Bundle bundle = new Bundle();
            bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_COMPLETE);
            final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
            dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
            dialog.message(SystemSetting.getString(
                    CommonConst.VIEW_WORD_LEASE_LIST_MSG_REGISTER, "", this));                //貸出登録は完了しました
            dialog.params(bundle);
            dialog.show();
        }else {
            Bundle bundle = new Bundle();
            bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_COMPLETE);
        }

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
    protected int SettingListColor(String strAssetStatus, String strAssetNo, String strWarning) {   //ワーニング表示(非表示)追加   20230907 ADD okubo v2.4.1
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
            else if (!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, strAssetNo), CommonAuthority.Auth.updHubList)){
                //権限無しの場合(#A6C78E)
                intColor =Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_AUTHROTY));
            }
            else if(CommonConst.ASSET_STATUS.prepare.getString().equals(strAssetStatus) ||
                    CommonConst.ASSET_STATUS.Dispose.getString().equals(strAssetStatus)) {
                //準備中、除却の場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_ASSET_DATA_ILLEGAL));
            }
            else if (!CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(AssetFactory.Select(db, strAssetNo).getLeaseStatus())) {
                //貸出不可の場合
                intColor =Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_LEASE_STATUS_ILLEGAL));
            }
//            //HT貸出時のアラート表示ありの場合
//            else if (_LeaseAlertDisplay &&
//                    strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LEASE_LIMIT, "", this))) {
//                //リース期限エラーの場合
//                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_LEASE_DATE_LIMIT));
//            }
//            else if (_LeaseAlertDisplay &&
//                    strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_GENERIC_LIMIT, "", this))) {
//                //○○期限エラーの場合
//                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_GENERAL_DATE_LIMIT));
//            }
            else if (CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString().equals(_ProcessData.GetLeaseUpperErrDiv(strAssetNo)) && _ProcessData.CheckLeaseNum(strAssetNo)) {
                //エラーで貸出回数が上限を超えている場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_LEASE_ERR));
                if(CommonConst.ASSET_STATUS.Lease.getString().equals(strAssetStatus)) {
                    //警告フラグをTrueに
                    _blnAlertFlag = true;
                }
            }
            else if (CommonConst.LEASE_UPPER_ERR_DIV.ALERT.getString().equals(_ProcessData.GetLeaseUpperErrDiv(strAssetNo)) && _ProcessData.CheckLeaseNum(strAssetNo)) {
                //警告で貸出回数が上限を超えている場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_LEASE_ALERT));
                if(CommonConst.ASSET_STATUS.Lease.getString().equals(strAssetStatus)) {
                    //警告フラグをTrueに
                    _blnAlertFlag = true;
                }
                //20200317 Add Sasaki End
            }
            else if (strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_DUE_LIMIT, "", this))) {
                //返却期限エラーの場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_RETURN_DATE_LIMIT));
            }
            else if(CommonConst.ASSET_STATUS.Lease.getString().equals(strAssetStatus)) {
                //貸出の場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_ASSET_STATUS_ILLEGAL));;

                //警告フラグをTrueに
                _blnAlertFlag = true;
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
     * 一覧のヘッダー背景色区分を設定します
     * @param strAssetStatus　資産状態
     * @param strAssetNo　資産番号
     * @param strWarning　警告・エラーメッセージ
     */
    protected void setHeaderBkgColorDiv(String strAssetStatus, String strAssetNo, String strWarning) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        SQLiteDatabase db = null;

        try {
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();

            if (_headerColorDiv != ListHeaderBkgColorDiv.ERROR) {
                //設定済みの区分が警告or通常の場合
                if (strAssetStatus == null || "".equals(strAssetStatus)) {
                    //マスタなしの場合
                    _headerColorDiv = ListHeaderBkgColorDiv.ERROR;
                }
                else if (!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, strAssetNo), CommonAuthority.Auth.updHubList)) {
                    //権限無しの場合(#A6C78E)
                    _headerColorDiv = ListHeaderBkgColorDiv.ERROR;
                }
                else if(CommonConst.ASSET_STATUS.prepare.getString().equals(strAssetStatus) ||
                        CommonConst.ASSET_STATUS.Dispose.getString().equals(strAssetStatus)) {
                    //準備中、除却の場合
                    _headerColorDiv = ListHeaderBkgColorDiv.ERROR;
                }
                else if (!CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(AssetFactory.Select(db, strAssetNo).getLeaseStatus())) {
                    //貸出不可の場合
                    _headerColorDiv = ListHeaderBkgColorDiv.ERROR;
                }
                else if(_LeaseAlertDisplay &&
                        strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LEASE_LIMIT, "", this))) {
                    //リース期限エラーの場合
                    _headerColorDiv = ListHeaderBkgColorDiv.ERROR;
                }
                else if(_LeaseAlertDisplay &&
                        strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_GENERIC_LIMIT, "", this))) {
                    //○○期限エラーの場合
                    _headerColorDiv = ListHeaderBkgColorDiv.ERROR;
                }
                else if (CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString().equals(_ProcessData.GetLeaseUpperErrDiv(strAssetNo)) &&
                        _ProcessData.CheckLeaseNum(strAssetNo)) {
                    //エラーで貸出回数が上限を超えている場合
                    _headerColorDiv = ListHeaderBkgColorDiv.ERROR;
                }

                if (_headerColorDiv == ListHeaderBkgColorDiv.ERROR) {
                    //本メソッドで区分にエラーがセットされた場合、警告のチェックはなし
                    return;
                }

                if (_headerColorDiv != ListHeaderBkgColorDiv.WARN) {
                    //設定済みの区分が通常の場合
                    if (CommonConst.LEASE_UPPER_ERR_DIV.ALERT.getString().equals(_ProcessData.GetLeaseUpperErrDiv(strAssetNo)) &&
                            _ProcessData.CheckLeaseNum(strAssetNo)) {
                        //警告で貸出回数が上限を超えている場合
                        _headerColorDiv = ListHeaderBkgColorDiv.WARN;
                    }
                    else if (strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_DUE_LIMIT, "", this))) {
                        //返却期限エラーの場合
                        _headerColorDiv = ListHeaderBkgColorDiv.WARN;
                    }
                }
            }
        }
        catch (Exception ex) {
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
     * 警告メッセージコードを返却します
     * @return
     */
    @Override
    protected String GetAlertMessage(){
        return CommonConst.VIEW_WORD_ALL_CONFIRM_LEASE_DATA;
    }

    public void onMyDialogCancelled(int requestCode, Bundle params) {}
}
