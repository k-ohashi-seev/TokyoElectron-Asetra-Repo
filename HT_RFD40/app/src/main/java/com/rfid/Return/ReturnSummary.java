package com.rfid.Return;

import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.common.Common;
import com.rfid.common.CommonAuthority;
import com.rfid.common.DatabaseHelper;
import com.rfidpk.R;
import com.rfid.Factory.AssetFactory;
import com.rfid.common.BaseSummary;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.SystemSetting;

/**
 * 返却詳細表示のクラス
 */
public class ReturnSummary extends BaseSummary implements CommonOKDialog.Callback{

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
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_RETURN,"",this));                 //返却　タイトル
        ((TextView)findViewById(R.id.lblSummaryListCount))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_COUNT, "", this));           //件数
        ((TextView)findViewById(R.id.btnSummaryConfirm))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));        //登録
        ((TextView)findViewById(R.id.btnSummarClear))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_CLEAR, "", this));           //クリア
        ((TextView)findViewById(R.id.lblSummaryListTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ASSETNAME_TITLE, "", this));    //資産番号・名称
        ((TextView)findViewById(R.id.lblSummaryListCountErr))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_MASTER_NONE, "", this));    //マスタ未登録件数
        //20210918 ADD Takase V2.0 START
        ((TextView)findViewById(R.id.lblSummaryListNoAuthority))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_ERR_NO_AUTHORITY, "", this));    //返却不可(権限無し)
        //20210918 ADD Takase V2.0 END

        //HT返却時のアラート表示有  20230906 ADD okubo V2.4.1 Start
        if(_ReturnAlertDisplay) {
            ((TextView)findViewById(R.id.lblSummaryReturnLimit))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_LIST_DUE_LIMIT_ERROR, "", this)); //返却期限エラー
            ((TextView)findViewById(R.id.lblSummaryLeaseLimitWarning))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_LIMIT_WARNING, "", this));    //貸出上限警告件数
            ((TextView)findViewById(R.id.lblSummaryLeaseLimitErr))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_LIMIT_COUNT, "", this));    //貸出上限エラー件数
            ((TextView)findViewById(R.id.lblSummaryLeaseLimit))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_LEASE_LIMIT_ERROR, "", this));    //リース期限エラー
            ((TextView)findViewById(R.id.lblSummaryGenericLimit))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_GENERIC_LIMIT_ERROR, "", this));  //○○期限エラー
        }
        //20230906 ADD okubo V2.4.1 End
    }

    /**
     * 登録処理
     */
    protected void RegisterProcess(SQLiteDatabase db){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);


        //照合機能の設定がYESの時
        if(SettingFactory.GetMatchingFunction(db).equals(CommonConst.MATCHING_FUNCTION.YES.getString())){
            //返却(照合)
            _ProcessData.InsertMatReturnProces(db, this,CommonConst.PROCESS_DIV.Return.getString());

            //資産状態を更新
            AssetFactory.Update(db, _ProcessData.GetRegisterAsset(), CommonConst.ASSET_STATUS.Storage);

            //イベントログ
            LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                    GetWord(CommonConst.VIEW_WORD_RETURN_LIST_MSG_REGISTER)
                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + _ProcessData.CountOKAll()
                    , this);

            //データクリア
            _ProcessData.Clear();

            //メッセージBOX
            Bundle bundle = new Bundle();
            bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_COMPLETE);
            final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
            dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
            dialog.message(SystemSetting.getString(
                    CommonConst.VIEW_WORD_RETURN_LIST_MSG_REGISTER, "", this));                  //返却登録は完了しました
            dialog.params(bundle);
            dialog.show();

        }else{
            //返却
            //登録処理
            _ProcessData.InsertProcesReturn(db, this);

            //資産状態を更新
            AssetFactory.Update(db, _ProcessData.GetRegisterAsset(), CommonConst.ASSET_STATUS.Storage);

            //イベントログ
            LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                    GetWord(CommonConst.VIEW_WORD_RETURN_LIST_MSG_REGISTER)
                            + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + _ProcessData.CountOKAll()
                    , this);

            //データクリア
            _ProcessData.Clear();

            //メッセージBOX
            Bundle bundle = new Bundle();
            bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_COMPLETE);
            final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
            dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
            dialog.message(SystemSetting.getString(
                    CommonConst.VIEW_WORD_RETURN_LIST_MSG_REGISTER, "", this));                  //返却登録は完了しました
            dialog.params(bundle);
            dialog.show();
        }


    };

    /**
     * 戻り先の画面
     */
    protected void onSummaryReturn(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        //返却資産読取画面へ
        Intent intent = new Intent();
        intent.putExtra(CommonConst.VIEW_KEY_PROCESS_DATA, _ProcessData);
        setResult(RESULT_OK, intent);

        this.finish();
    };

    /**
     * 状態ステータスによってリストビューの文字色を返す
     * サブクラスで実装する
     * @param strAssetStatus
     * @param strAssetNo
     * @return
     */
    //protected int SettingListColor(String strAssetStatus) {       20200225 Del Sasaki V1.2
    //protected int SettingListColor(String strAssetStatus, String strAssetNo) {      //20200225 Add Sasaki V1.2
    protected int SettingListColor(String strAssetStatus, String strAssetNo, String strWarning) {   //ワーニング表示(非表示)追加   20230907 ADD okubo v2.4.1
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        int intColor = Color.BLACK;
        SQLiteDatabase db = null;     //20210918 ADD Takase V2.0

        try {
            //20210918 ADD Takase V2.0 START
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            //20210918 ADD Takase V2.0 END

            if (strAssetStatus == null || "".equals(strAssetStatus)) {
                //マスタなしの場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_ASSET_NO_REGIST));

                if (_ReturnAlertDisplay) {
                    //上限期限詳細表示が有効の場合
                    _blnCheckErrAlertFlag = true;
                }
            }
            //20210918 ADD Takase V2.0 START
            else if (!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, strAssetNo), CommonAuthority.Auth.updHubList)) {
                //権限無しの場合(#A6C78E)
                intColor =Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_AUTHROTY));

                if (_ReturnAlertDisplay) {
                    //上限期限詳細表示が有効の場合
                    _blnCheckErrAlertFlag = true;
                }
            }
            //20210918 ADD Takase V2.0 END
            else if (CommonConst.ASSET_STATUS.prepare.getString().equals(strAssetStatus) ||
                    CommonConst.ASSET_STATUS.Dispose.getString().equals(strAssetStatus)) {
                //準備中、除却の場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_ASSET_DATA_ILLEGAL));

                if (_ReturnAlertDisplay) {
                    //上限期限詳細表示が有効の場合
                    _blnCheckErrAlertFlag = true;
                }
            }
            else if (!CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(AssetFactory.Select(db, strAssetNo).getLeaseStatus())) {
                //貸出不可の場合
                intColor =Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_LEASE_STATUS_ILLEGAL));

                if (_ReturnAlertDisplay) {
                    //上限期限詳細表示が有効の場合
                    _blnCheckErrAlertFlag = true;
                }
            }
            //HT返却時のアラート表示有  20230907 ADD okubo V2.4.1 Start
            else if(_ReturnAlertDisplay &&
                    strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LEASE_LIMIT, "", this))) {
                //リース期限エラーの場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_LEASE_DATE_LIMIT));
                _blnCheckErrAlertFlag = true;
            }
            else if(_ReturnAlertDisplay &&
                    strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_GENERIC_LIMIT, "", this))) {
                //○○期限エラーの場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_GENERAL_DATE_LIMIT));
                _blnCheckErrAlertFlag = true;
            }
            else if (_ReturnAlertDisplay &&
                    CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString().equals(_ProcessData.GetLeaseUpperErrDiv(strAssetNo)) &&
                    _ProcessData.CheckLeaseNum(strAssetNo)) {
                //エラーで貸出回数が上限を超えている場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_LEASE_ERR));
                _blnCheckErrAlertFlag = true;

                if(CommonConst.ASSET_STATUS.Lease.getString().equals(strAssetStatus)) {
                    //警告フラグをTrueに
                    _blnAlertFlag = true;
                }
            }
            else if (_ReturnAlertDisplay &&
                    CommonConst.LEASE_UPPER_ERR_DIV.ALERT.getString().equals(_ProcessData.GetLeaseUpperErrDiv(strAssetNo)) &&
                    _ProcessData.CheckLeaseNum(strAssetNo)) {
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
            else if (CommonConst.ASSET_STATUS.Storage.getString().equals(strAssetStatus)) {
                //保管の場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_ASSET_STATUS_ILLEGAL));;

                //警告フラグをTrueに
                _blnAlertFlag = true;
            }
            else {
                intColor = Color.BLACK;
            }
            //20230907 ADD okubo V2.4.1 End
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
        return intColor;
    }

    /**
     * 一覧のヘッダー背景色区分を設定します
     * @param strAssetStatus　資産状態
     * @param strAssetNo　資産番号
     * @param strWarning　警告・エラーメッセージ
     */
    @Override
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
                else if(_ReturnAlertDisplay &&
                        strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LEASE_LIMIT, "", this))) {
                    //リース期限エラーの場合
                    _headerColorDiv = ListHeaderBkgColorDiv.ERROR;
                }
                else if(_ReturnAlertDisplay &&
                        strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_GENERIC_LIMIT, "", this))) {
                    //○○期限エラーの場合
                    _headerColorDiv = ListHeaderBkgColorDiv.ERROR;
                }
                else if (_ReturnAlertDisplay &&
                        CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString().equals(_ProcessData.GetLeaseUpperErrDiv(strAssetNo)) &&
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
                    if (_ReturnAlertDisplay &&
                            CommonConst.LEASE_UPPER_ERR_DIV.ALERT.getString().equals(_ProcessData.GetLeaseUpperErrDiv(strAssetNo)) &&
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
        return CommonConst.VIEW_WORD_ALL_CONFIRM_RETURN_DATA;
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) { }

}
