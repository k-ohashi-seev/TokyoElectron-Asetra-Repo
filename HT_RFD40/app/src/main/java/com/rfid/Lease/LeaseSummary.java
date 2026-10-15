package com.rfid.Lease;

import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
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

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;

/**
 * 貸出詳細表示のクラス
 */
public class LeaseSummary extends BaseSummary implements CommonOKDialog.Callback{

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
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_ERR_NO_AUTHORITY, "", this));    //貸出不可(権限無し)
        //20210918 ADD Takase V2.0 END
        ((TextView)findViewById(R.id.lblSummaryLeaseLimitWarning))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_LIMIT_WARNING, "", this));    //貸出上限警告件数
        ((TextView)findViewById(R.id.lblSummaryLeaseLimitErr))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_LIMIT_COUNT, "", this));    //貸出上限エラー件数

        //20230830 ADD Kawamoto v2.4.1 START
        //HT貸出時のアラート表示ありの場合
        if (_LeaseAlertDisplay) {
            ((TextView)findViewById(R.id.lblSummaryLeaseLimit))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_LEASE_LIMIT_ERROR, "", this));    //リース期限エラー
            ((TextView)findViewById(R.id.lblSummaryGenericLimit))
                    .setText(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_LIST_GENERIC_LIMIT_ERROR, "", this));  //○○期限エラー
        }
        //20230830 ADD Kawamoto v2.4.1 END
    }

    /**
     * 登録処理
     * サブクラスで実装する
     */
    protected void RegisterProcess(SQLiteDatabase db){
        //登録処理
        _ProcessData.InsertProcesLease(db, this);

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

        //メッセージBOX
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_COMPLETE);
        final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
        dialog.message(SystemSetting.getString(
                CommonConst.VIEW_WORD_LEASE_LIST_MSG_REGISTER, "", this));                //貸出登録は完了しました
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
    //protected int SettingListColor(String strAssetStatus) {                           //20200225 Del Sasaki V1.2
    //protected int SettingListColor(String strAssetStatus, String strAssetNo) {        //20200225 Add Sasaki V1.2
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
            //20210918 ADD Takase V2.0 START
            else if (!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, strAssetNo), CommonAuthority.Auth.updHubList)){
                //権限無しの場合(#A6C78E)
                intColor =Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_AUTHROTY));
            }
            //20210918 ADD Takase V2.0 END
            else if(CommonConst.ASSET_STATUS.prepare.getString().equals(strAssetStatus) ||
                    CommonConst.ASSET_STATUS.Dispose.getString().equals(strAssetStatus)) {
                //準備中、除却の場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_ASSET_DATA_ILLEGAL));
            }
            else if (!CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(AssetFactory.Select(db, strAssetNo).getLeaseStatus())) {
                //貸出不可の場合
                intColor =Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_LEASE_STATUS_ILLEGAL));
            }
            //20230830 ADD Kawamoto v2.4.1 START    HT貸出時のアラート表示ありの場合
            else if (_LeaseAlertDisplay &&
                    strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LEASE_LIMIT, "", this))) {
                //リース期限エラーの場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_LEASE_DATE_LIMIT));
            }
            else if (_LeaseAlertDisplay &&
                    strWarning.contains(SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_GENERIC_LIMIT, "", this))) {
                //○○期限エラーの場合
                intColor = Color.parseColor(SettingFactory.GetAssetColor(db, CommonConst.SETTEI_KEY1_GENERAL_DATE_LIMIT));
            }
            //20230830 ADD Kawamoto v2.4.1 END
            //20200317 Add Sasaki Start
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

        Intent intent;
        //20210918 ADD Takase V2.0 START
        SQLiteDatabase db = null;
        try {
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            if (SettingFactory.GetReservationFunction(db).equals((CommonConst.RESERVATION_FUNCTION.YES.getString()))) {
                //予約機能あり 貸出読取画面へ
                intent = new Intent(getApplication(), LeaseRead.class);
                this.finish();
            }
            //20230911 ADD okubo v2.4.1 START
            else if(SettingFactory.GetLendingContinuous(db).equals(CommonConst.LENDING_CONTINUOUS.YES.getString())) {
                //予約機能なし＆連続貸出 読取画面へ
                intent = new Intent(getApplication(), LeaseRead.class);
             /*   intent.putExtra(CommonConst.VIEW_KEY_LEASE_CD, _ProcessData.getLeaseDestination());         //貸出先コードの値を渡す
                intent.putExtra(CommonConst.VIEW_KEY_LEASE_NAME, _ProcessData.getLeaseDestinationName());   //貸出先名の値を渡す
                intent.putExtra(CommonConst.VIEW_KEY_EMPLOYEE_CD, _ProcessData.getLeaseEmployee());         //社員コードの値を渡す
                intent.putExtra(CommonConst.VIEW_KEY_EMPLOYEE_NAME, _ProcessData.getLeaseEmployeeName());   //社員名の値を渡す
                intent.putExtra(CommonConst.VIEW_KEY_DATE, _ProcessData.getReturnDate());                   //返却予定日の値を渡す
                intent.putExtra(CommonConst.VIEW_KEY_LEASE_REMARK, _ProcessData.getRemark());               //備考を渡す
            */
                _ProcessData.Clear();    //件数のみクリア
                setResult(RESULT_OK, intent);   //前画面(読取画面)に戻して、startActivityしない
                this.finish();
                return;
            }
            //20230911 ADD okubo v2.4.1 END
            else {
                //予約機能なし 貸出入力画面へ
                intent = new Intent(getApplication(), LeaseSetting.class);
                this.finish();
            }
            //20210918 ADD Takase V2.0 END
            startActivity(intent);

        }
        catch (Exception ex) {
            showErrorMessage(ex.getMessage());
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
