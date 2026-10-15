package com.rfid.Storage;

import android.content.Intent;
import android.database.sqlite.SQLiteDatabase;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.common.Common;
import com.rfid.common.CommonAuthority;
import com.rfidpk.R;
import com.rfid.Factory.AssetFactory;
import com.rfid.common.BaseSummary;
import com.rfid.common.CommonConst;
import com.rfid.common.CommonOKDialog;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;

/**
 * 保管詳細表示のクラス
 */
public class StorageSummary extends BaseSummary implements CommonOKDialog.Callback{

    /**
     * 文言表示
     */
    protected void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_MENU_STORE,"",this));                    //保管　タイトル
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
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_STORAGE_ERR_NO_AUTHORITY, "", this));    //保管不可(権限なし)
            //20210918 ADD Takase V2.0 END
    }

    /**
     * 登録処理
     * サブクラスで実装する
     */
    protected void RegisterProcess(SQLiteDatabase db){

        //登録処理
        _ProcessData.InsertProcesStorage(db, this);

        //資産状態を更新
        //AssetFactory.Update(db, _ProcessData.GetRegisterAsset(), CommonConst.ASSET_STATUS.Storage);            20191119 Del Sasaki
        AssetFactory.UpdateStrage(db, _ProcessData.GetRegisterAsset(), CommonConst.ASSET_STATUS.Storage);   //20191119 Add Sasaki

        //イベントログ
        LogFactory.Insert(db, CommonConst.PROCESS_CAT.Event,
                 GetWord(CommonConst.VIEW_WORD_STORAGE_MSG_REGISTER)
                        + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_COUNT) + "】" + _ProcessData.CountOKAll()
                        + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_LOC_L_KBN) + "】" + _ProcessData.getLocLCode()
                        + " " + _ProcessData.getLocLName()
                        + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_LOC_M_KBN) + "】" + _ProcessData.getLocMCode()
                        + " " + _ProcessData.getLocMName()
                        + " 【" + GetWord(CommonConst.VIEW_WORD_ALL_LOC_S_KBN) + "】" + _ProcessData.getLocSCode()
                        + " " + _ProcessData.getLocSName()
                , this);

        //データクリア
        _ProcessData.Clear();

        //メッセージBOX
        Bundle bundle = new Bundle();
        bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_COMPLETE);
        final CommonOKDialog.Builder  dialog = new CommonOKDialog.Builder(this, false);
        dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", this));  //登録
        dialog.message(SystemSetting.getString(
                CommonConst.VIEW_WORD_STORAGE_MSG_REGISTER, "", this));                      //保管登録は完了しました
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
    //protected int SettingListColor(String strAssetStatus) {       20200225 Del Sasaki V1.2
    //protected int SettingListColor(String strAssetStatus, String strAssetNo) {      //20200225 Add Sasaki V1.2
    protected int SettingListColor(String strAssetStatus, String strAssetNo, String strWarning) {   //ワーニング表示(非表示)追加   20230907 ADD okubo v2.4.1
        int intColor = Color.BLACK;
        SQLiteDatabase db = null;      //20210918 ADD Takase V2.0

        try {
            //20210918 ADD Takase V2.0 START
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            //20210918 ADD Takase V2.0 END

            if (strAssetStatus == null || "".equals(strAssetStatus)) {
                //マスタなしの場合
                intColor = Color.RED;
                //20210918 ADD Takase V2.0 START
            } else if (!CommonAuthority.CheckAssetAuth
                    (db, AssetFactory.Select(db, strAssetNo), CommonAuthority.Auth.updHubList)){
                //権限無しの場合(#A6C78E)
                intColor = 0XFFA6C78E;
                //20210918 ADD Takase V2.0 END

            } else if(CommonConst.ASSET_STATUS.Lease.getString().equals(strAssetStatus)) {
                //貸出の場合
                intColor = Color.BLUE;

                //警告フラグをTrueに
                _blnAlertFlag = true;
            } else if(CommonConst.ASSET_STATUS.prepare.getString().equals(strAssetStatus) ||
                    CommonConst.ASSET_STATUS.Dispose.getString().equals(strAssetStatus)) {
                //準備中、除却の場合
                intColor = Color.RED;
            } else {
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
        //保管入力画面へ
        Intent intent = new Intent(getApplication(), StorageLocationSetting.class);
        startActivity(intent);
    }

    /**
     * 警告メッセージコードを返却します
     * @return
     */
    @Override
    protected String GetAlertMessage(){
        return CommonConst.VIEW_WORD_ALL_CONFIRM_LEASE_DATA;
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) { }
}
