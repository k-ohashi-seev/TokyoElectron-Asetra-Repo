package com.rfid.Factory;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.AssetModel;
import com.rfid.common.Common;
import com.rfid.common.CommonConst;
import com.rfid.common.RFIDPkgException;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.EntryInventData;
import com.rfid.common.bean.EntryInventDataDetail;

import java.io.Serializable;
import java.text.ParseException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;

public class InventoryFactory implements Serializable {

    private static HashMap<String, AssetModel> _mapInvAll = new HashMap<String, AssetModel>();                          //全資産データ
    private static HashMap<String, AssetModel> _mapInvAllBySubNo = new HashMap<String, AssetModel>();                   //全資産データ(主キーがサブ資産番号)
    private static HashMap<String, AssetModel> _mapInvOriginal = new HashMap<String, AssetModel>();                     //棚卸対象データ
    private static HashMap<String, AssetModel> _mapInvOriginalBySubNo = new HashMap<String, AssetModel>();              //棚卸対象データ(主キーがサブ資産番号)
    private static HashMap<String, EntryInventDataDetail> _mapInv = new HashMap<String, EntryInventDataDetail>();                  //棚卸済データ
    private static HashMap<String, EntryInventDataDetail> _mapInvOK = new HashMap<String, EntryInventDataDetail>();                //棚卸OKデータ
    private static HashMap<String, EntryInventDataDetail> _mapInvOKOriginal = new HashMap<String, EntryInventDataDetail>();        //棚卸OKデータ(連続棚卸用)
    private static HashMap<String, EntryInventDataDetail> _mapInvLocErr = new HashMap<String, EntryInventDataDetail>();            //棚卸ロケエラーデータ
    private static HashMap<String, EntryInventDataDetail> _mapLease = new HashMap<String, EntryInventDataDetail>();                //貸出中(未棚卸)データ        //20200305 Add Sasaki V1.2
    private static HashMap<String, EntryInventDataDetail> _mapLeaseOriginal = new HashMap<String, EntryInventDataDetail>();        //貸出中(未棚卸)元データ      //20200305 Add Sasaki V1.2
    private static HashMap<String, EntryInventDataDetail> _mapInvNone = new HashMap<String, EntryInventDataDetail>();              //在庫なしデータ
    private static HashMap<String, EntryInventDataDetail> _mapInvNoneOriginal = new HashMap<String, EntryInventDataDetail>();      //在庫なし元データ
    private static HashMap<String, String> _mapLinkID = new HashMap<String, String>();                     //資産番号と資産IDの紐付
    private static HashMap<String, EntryInventDataDetail> _mapStoreData = new HashMap<String, EntryInventDataDetail>();                         //HT蓄積データ
    private static String _LocLCode = "";                                                      //ロケ大区分
    private static String _LocMCode = "";                                                      //ロケ中区分
    private static String _LocSCode = "";                                                      //ロケ小区分
    private static String _LocLName = "";                                                      //ロケ大区分名称
    private static String _LocMName = "";                                                      //ロケ中区分名称
    private static String _LocSName = "";                                                      //ロケ小区分名称

    private String _InventoryContinue = CommonConst.INVENTORY_CONTINUE.NO.getString();      //20191209 Add Sasaki

    /**
     * コンストラクタ
     */
    public InventoryFactory() {

    }

    /**
     * 資産マスタから全データを取得する
     * @param db
     * @param context
     */
    @SuppressLint("Range")
    public void CreateAssetData(SQLiteDatabase db, Context context){
        //クリアする
        _mapInvAll.clear();     //20200214 Add Sasaki

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO");
        sql.append("  ,ASSET_ID");
        sql.append("  ,ASSET_NAME");
        sql.append(" FROM M_ASSET");
        sql.append("  ASSET_STATUS NOT IN (0, 9)");       //未紐付、除却を除く

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);
        //Cursor cursor = db.rawQuery(sql.toString(), null);

        //取得した資産データを資産クラスに格納する
        while (cursor.moveToNext()) {
            AssetModel assetModel = new AssetModel();

            //各項目を設定する
            assetModel.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));
            assetModel.setAssetID(cursor.getString(cursor.getColumnIndex("ASSET_ID")));
            assetModel.setAssetName(cursor.getString(cursor.getColumnIndex("ASSET_NAME")));

            //溜め込む
            _mapInvAll.put((assetModel.getAssetNo()), assetModel);

        }
    }

    /**
     * 資産マスタから棚卸データを取得します。
     * @param db
     * @param strLLoc
     * @param strMLoc
     * @param strSLoc
     */
    public void CreateInventoryData(SQLiteDatabase db, String strLLoc, String strMLoc, String strSLoc){

        //棚卸対象データを取得しセットする
        SetInventoryData(db, strLLoc, strMLoc, strSLoc);

        //実棚ロケーションが指定された大区分、中区分の全資産データを読み込む（ロケ違いを判定するため小区分は省く）
        SetAssetAllDataByLoc(db, strLLoc, strMLoc);
    }

    /**
     * HT蓄積データを取得し設定します。
     * @param db
     */
    public void CreateHTStoreData(SQLiteDatabase db) {
        //クリアする
        _mapStoreData.clear();

        EntryInventData StoreData = CollectInventoryData(db);
        for (Iterator<EntryInventDataDetail> iterator = StoreData.iterator(); iterator.hasNext(); ) {
            //1件取得する
            EntryInventDataDetail data = iterator.next();
            _mapStoreData.put(data.getan(), data);
        }
    }

    /**
     * HT蓄積から棚卸データを取得する
     * @param db
     */
    @SuppressLint("Range")
    public EntryInventData CollectInventoryData(SQLiteDatabase db){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO");
        sql.append("  ,ASSET_NAME");
        sql.append("  ,PHYINV_LOC_L_CD");
        sql.append("  ,PHYINV_LOC_M_CD");
        sql.append("  ,PHYINV_LOC_S_CD");
        sql.append("  ,PHYINV_TS");
        sql.append("  ,PHYINV_STATUS");
        sql.append("  ,PHYINV_EMPLOYEE_CD");
        sql.append(" FROM INVENTORY");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        //取得したデータを棚卸クラスに格納する
        EntryInventData map = new EntryInventData();
        while (cursor.moveToNext()) {
            EntryInventDataDetail invdata = new EntryInventDataDetail();

            //各項目を設定する
            invdata.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));
            invdata.setAssetName(cursor.getString(cursor.getColumnIndex("ASSET_NAME")));
            invdata.setphyinvLocLCd(cursor.getString(cursor.getColumnIndex("PHYINV_LOC_L_CD")));
            invdata.setPhyinvLocMCd(cursor.getString(cursor.getColumnIndex("PHYINV_LOC_M_CD")));
            invdata.setphyinvLocSCd(cursor.getString(cursor.getColumnIndex("PHYINV_LOC_S_CD")));
            invdata.setphyinvTs(cursor.getString(cursor.getColumnIndex("PHYINV_TS")));
            invdata.setphyinvStatus(cursor.getString(cursor.getColumnIndex("PHYINV_STATUS")));
            invdata.setphyinvEmployeeCd(cursor.getString(cursor.getColumnIndex("PHYINV_EMPLOYEE_CD")));

            //溜め込む
            map.setinvDataDetailList(invdata);
        }

        map.setProcessToolDiv(CommonConst.WORD_TOOL_DIV.H.getString());
        return map;
    }

// 20191016 Add Start Sasaki
    /**
     * HT内に蓄積したデータをDBに登録する
     * @param db
     * @throws RFIDPkgException
     */
    public void InsertLoc(SQLiteDatabase db, Activity activity) throws RFIDPkgException{

        String strAssetNo = "";
        String PhyInvTs = Common.getNowTime("yyyyMMddHHmmss");

        //読取データを設定する
        HashMap<String, EntryInventDataDetail> InsertData = _mapInv;

        try {
            //連続棚卸モード連続なしの場合、ロケを追加する
            if (CommonConst.INVENTORY_CONTINUE.NO.getString().equals(_InventoryContinue)) {             //20191209 Add Sasaki

                //読取していないロケデータを現物なしとして追加する
                for(Iterator<AssetModel> iterator = _mapInvOriginal.values().iterator(); iterator.hasNext(); ) {

                    //対象ロケの資産Mのデータを1件取得する
                    AssetModel model = iterator.next();
                    strAssetNo = model.getAssetNo();

                    //if (!InsertData.containsKey(strAssetNo)) {        20200306 Del Sasaki V1.2
                    if (!InsertData.containsKey(strAssetNo) &&
                        !CommonConst.ASSET_STATUS.Lease.getString().equals(model.getAssetStatus())) {          //20200306 Add Sasaki V1.2
                        //貸出は現物なしに含めない
                        EntryInventDataDetail invdata = null;
                        invdata = new EntryInventDataDetail();
                        invdata.setAssetNo(strAssetNo);
                        invdata.setAssetName(model.getAssetName());
                        invdata.setphyinvTs(PhyInvTs);
                        invdata.setphyinvStatus(CommonConst.PHYINV_STATUS.Error.getString());
                        invdata.setphyinvEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));
                        invdata.setphyinvLocLCd(_LocLCode);
                        invdata.setPhyinvLocMCd(_LocMCode);
                        invdata.setphyinvLocSCd(_LocSCode);

                        InsertData.put(strAssetNo, invdata);
                    }
                }

            }

            //棚卸データを登録する(連続棚卸モードなし⇒読取したデータ＋対象ロケの未読取データ、連続棚卸モード連続あり⇒読取したデータ)
            for(Iterator<EntryInventDataDetail> iterator = InsertData.values().iterator(); iterator.hasNext(); ) {

                //1件取得する
                EntryInventDataDetail invdata = iterator.next();

                try {
                    //削除
                    DeleteInventoryTable(db, invdata.getan());

                    //登録
                    InsertInventoryTable(db, invdata);

//20191210 Add Start Sasaki
                    //資産マスタを更新する
                    AssetModel assetModel = new AssetModel();
                    assetModel.setAssetNo(invdata.getan());
                    assetModel.setPhyinvFlag(CommonConst.PHYINV_FLAG.Yes.getString());
                    assetModel.setPhyinvStatus(invdata.getps());
                    AssetFactory.UpdateInventory(db, assetModel);
//20191210 Upd End Sasaki
                } catch (Exception ex) {
                    throw new RFIDPkgException(ex.getMessage() + ":" + invdata.getan(), ex);
                }
            }

        } catch (Exception ex) {
            throw new RFIDPkgException(ex.getMessage() + ":" + strAssetNo, ex);
        }
    }
// 20191016 Add End Sasaki

    /**
     * HT内に蓄積したデータをDBに登録する(ロケなし)
     * @param db
     * @throws RFIDPkgException
     */
    public void Insert(SQLiteDatabase db) throws RFIDPkgException{

        //読込んだ棚卸データを登録する
        for(Iterator<EntryInventDataDetail> iterator = _mapInv.values().iterator(); iterator.hasNext(); ) {

            //1件取得する
            EntryInventDataDetail invdata = iterator.next();

            try {
                //削除
                DeleteInventoryTable(db, invdata.getan());

                //登録
                InsertInventoryTable(db, invdata);

            } catch (Exception ex) {
                throw new RFIDPkgException(ex.getMessage() + ":" + invdata.getan(), ex);
            }
        }
    }

    /**
     * 棚卸対象データを取得しセットします。
     * @param db
     * @param strLLoc
     * @param strMLoc
     * @param strSLoc
     */
    private void SetInventoryData(SQLiteDatabase db, String strLLoc, String strMLoc, String strSLoc) {
        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO");
        sql.append("  ,ASSET_ID");
        sql.append("  ,ASSET_NAME");
        sql.append("  ,ASSET_STATUS");     //20200305 Add Sasaki
        sql.append("  ,PHYINV_FLAG");      //20191206 Add Sasaki
        sql.append("  ,PHYINV_STATUS");    //20191206 Add Sasaki
        sql.append("  ,SUB_ASSET_NO");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_ASSET);
        sql.append(" WHERE");
        sql.append("  ASSET_STATUS NOT IN (0, 9)");       //未紐付、除却を除く
        sql.append("  AND INV_FLG = 'Y'");

        List<String> list = new ArrayList<>();
        if (!"".equals(strLLoc)) {
            sql.append("  AND LOC_L_CD = ?");
            list.add(strLLoc);
        }
        if (!"".equals(strMLoc)) {
            sql.append(" AND LOC_M_CD = ?");
            list.add(strMLoc);
        }
        if (!"".equals(strSLoc)) {
            sql.append(" AND LOC_S_CD = ?");
            list.add(strSLoc);
        }

        //検索条件がない場合、nullを設定
        String[] strWhere = list.toArray(new String[list.size()]);
        if (strWhere.length == 0){
            strWhere = null;
        }

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), strWhere);

//20191206 UPD START SASAKI
        //設定マスタの連続棚卸モードが連続の場合_
        if (CommonConst.INVENTORY_CONTINUE.YES.getString().equals(SettingFactory.GetInventoryContinue(db))) {
            SetInventoryDataContinue(cursor);
        } else {
            SetInventoryDataNoContinue(cursor);
        }
//20191206 UPD END SASAKI

    }

//20191206 ADD START SASAKI

    /**
     * 非連続棚卸のデータを設定する
     * @param cursor
     */
    @SuppressLint("Range")
    private void SetInventoryDataNoContinue(Cursor cursor) {
        //クリアする //20200214 Add Sasaki
        _mapInvOriginal.clear();
        _mapInvOriginalBySubNo.clear();
        _mapInvNone.clear();
        _mapInvNoneOriginal.clear();

        //取得した棚卸データを棚卸クラスに格納する
        while (cursor.moveToNext()) {
            AssetModel assetModel = new AssetModel();
            assetModel.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));
            assetModel.setAssetID(cursor.getString(cursor.getColumnIndex("ASSET_ID")));
            assetModel.setAssetName(cursor.getString(cursor.getColumnIndex("ASSET_NAME")));
            assetModel.setAssetStatus(cursor.getString(cursor.getColumnIndex("ASSET_STATUS")));         //20200305 Add Sasaki V1.2
            assetModel.setPhyinvFlag(cursor.getString(cursor.getColumnIndex("PHYINV_FLAG")));           //20191206 Add Sasaki
            assetModel.setPhyinvStatus(cursor.getString(cursor.getColumnIndex("PHYINV_STATUS")));   //20191206 Add Sasaki
            assetModel.setSubAssetNo(cursor.getString(cursor.getColumnIndex("SUB_ASSET_NO")));

            //実棚済みのデータを設定する
            EntryInventDataDetail invdata = new EntryInventDataDetail();
            invdata.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));
            invdata.setAssetName(cursor.getString(cursor.getColumnIndex("ASSET_NAME")));
            invdata.setSubAssetNo(cursor.getString(cursor.getColumnIndex("SUB_ASSET_NO")));

            //棚卸データに溜め込む
            _mapInvOriginal.put(assetModel.getAssetNo(), assetModel);
            _mapInvOriginalBySubNo.put(assetModel.getSubAssetNo(), assetModel);

            //20200305 Add Start Sasaki V1.2
            if (CommonConst.ASSET_STATUS.Lease.getString().equals(assetModel.getAssetStatus())){
                //貸出の場合
                _mapLease.put(invdata.getan(), invdata);
                _mapLeaseOriginal.put(invdata.getan(), invdata);
            //20200305 Add End Sasaki V1.2
            } else {
                //在庫なしデータに溜め込む
                _mapInvNone.put(invdata.getan(), invdata);
                _mapInvNoneOriginal.put(invdata.getan(), invdata);
            }
        }
    }

//20191206 Add Start Sasaki
    /**
     * 連続棚卸のデータを設定する
     * @param cursor
     */
    @SuppressLint("Range")
    private void SetInventoryDataContinue(Cursor cursor) {
        //クリアする //20200214 Add Sasaki
        _mapInvOriginal.clear();
        _mapInvOriginalBySubNo.clear();
        _mapInvOK.clear();
        _mapInvOKOriginal.clear();
        _mapInvNone.clear();
        _mapInvNoneOriginal.clear();

        //取得した棚卸データを棚卸クラスに格納する
        while (cursor.moveToNext()) {
            AssetModel assetModel = new AssetModel();
            assetModel.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));
            assetModel.setAssetID(cursor.getString(cursor.getColumnIndex("ASSET_ID")));
            assetModel.setAssetName(cursor.getString(cursor.getColumnIndex("ASSET_NAME")));
            assetModel.setAssetStatus(cursor.getString(cursor.getColumnIndex("ASSET_STATUS")));         //20200305 Add Sasaki V1.2
            assetModel.setPhyinvFlag(cursor.getString(cursor.getColumnIndex("PHYINV_FLAG")));
            assetModel.setPhyinvStatus(cursor.getString(cursor.getColumnIndex("PHYINV_STATUS")));
            assetModel.setSubAssetNo(cursor.getString(cursor.getColumnIndex("SUB_ASSET_NO")));

            //実棚済みのデータを設定する
            EntryInventDataDetail invdata = new EntryInventDataDetail();
            invdata.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));
            invdata.setAssetName(cursor.getString(cursor.getColumnIndex("ASSET_NAME")));
            invdata.setSubAssetNo(cursor.getString(cursor.getColumnIndex("SUB_ASSET_NO")));

            //棚卸データに溜め込む
            _mapInvOriginal.put(assetModel.getAssetNo(), assetModel);
            _mapInvOriginalBySubNo.put(assetModel.getSubAssetNo(), assetModel);

            if (CommonConst.PHYINV_FLAG.Yes.getString().equals(assetModel.getPhyinvFlag()) &&
                    CommonConst.PHYINV_STATUS.Normal.getString().equals(assetModel.getPhyinvStatus())) {
                //正常で実棚済みの場合
                _mapInvOK.put(invdata.getan(), invdata);
                _mapInvOKOriginal.put(invdata.getan(), invdata);
            //20200305 Add Start Sasaki V1.2
            } else if (CommonConst.PHYINV_FLAG.No.getString().equals(assetModel.getPhyinvFlag()) &&
                    CommonConst.ASSET_STATUS.Lease.getString().equals(assetModel.getAssetStatus())){
                //貸出で未棚卸の場合
                _mapLease.put(invdata.getan(), invdata);
                _mapLeaseOriginal.put(invdata.getan(), invdata);
            //20200305 Add End Sasaki V1.2
            } else {
                //在庫なしデータに溜め込む
                _mapInvNone.put(invdata.getan(), invdata);
                _mapInvNoneOriginal.put(invdata.getan(), invdata);
            }
        }
    }
//20191206 ADD END SASAKI

    /**
     * 全資産データを読み込みます。
     * @param db
     */
    @SuppressLint("Range")
    private void SetAssetAllData(SQLiteDatabase db) {

        //クリアする
        _mapInvAll.clear();     //20200214 Add Sasaki
        _mapInvAllBySubNo.clear();
        _mapLinkID.clear();     //20200214 Add Sasaki

        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO,");
        sql.append("  ASSET_ID,");
        sql.append("  ASSET_NAME,");
        sql.append("  LOC_L_CD,");
        sql.append("  LOC_M_CD,");
        sql.append("  LOC_S_CD,");
        sql.append("  PHYINV_FLAG,");      //20191209 Add Sasaki
        sql.append("  PHYINV_STATUS,");    //20191209 Add Sasaki
        sql.append("  ASSET_STATUS,");
        sql.append("  SUB_ASSET_NO");
        sql.append(" FROM M_ASSET");

        //SQL発行
        Cursor cursorAll = db.rawQuery(sql.toString(), null);

        while (cursorAll.moveToNext()) {
            AssetModel assetModel  = new AssetModel();

            //各項目を設定する
            assetModel.setAssetNo(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO")));
            assetModel.setAssetID(cursorAll.getString(cursorAll.getColumnIndex("ASSET_ID")));
            assetModel.setAssetName(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NAME")));
            assetModel.setPhyinvFlag(cursorAll.getString(cursorAll.getColumnIndex("PHYINV_FLAG")));         //20191209 Add Sasaki
            assetModel.setPhyinvStatus(cursorAll.getString(cursorAll.getColumnIndex("PHYINV_STATUS")));     //20191209 Add Sasaki
            assetModel.setSubAssetNo(cursorAll.getString(cursorAll.getColumnIndex("SUB_ASSET_NO")));

            //資産マスタデータに溜め込む
            _mapInvAll.put(assetModel.getAssetNo(), assetModel);
            _mapInvAllBySubNo.put(assetModel.getSubAssetNo(), assetModel);

            //資産ID紐付データに溜め込む
            _mapLinkID.put(assetModel.getAssetID(), assetModel.getAssetNo());
        }
    }

    /**
     * 実棚ロケーションが指定された区分の全資産データを読み込みます。
     * @param db
     */
    @SuppressLint("Range")
    private void SetAssetAllDataByLoc(SQLiteDatabase db, String locL, String locM) {

        //クリアする
        _mapInvAll.clear();     //20200214 Add Sasaki
        _mapInvAllBySubNo.clear();
        _mapLinkID.clear();     //20200214 Add Sasaki

        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO,");
        sql.append("  ASSET_ID,");
        sql.append("  ASSET_NAME,");
        sql.append("  LOC_L_CD,");
        sql.append("  LOC_M_CD,");
        sql.append("  LOC_S_CD,");
        sql.append("  PHYINV_FLAG,");      //20191209 Add Sasaki
        sql.append("  PHYINV_STATUS,");    //20191209 Add Sasaki
        sql.append("  ASSET_STATUS,");
        sql.append("  SUB_ASSET_NO");
        sql.append(" FROM M_ASSET");
        sql.append("  WHERE INV_FLG = 'Y'");
        List<String> list = new ArrayList<>();
        if (!locL.isEmpty()) {
            sql.append(" AND LOC_L_CD = ? ");
            list.add(locL);
        }
        if (!locM.isEmpty() && !locL.isEmpty()) {
            sql.append(" AND LOC_M_CD = ? ");
            list.add(locM);
        }



        //検索条件がない場合、nullを設定
        String[] strWhere = list.toArray(new String[list.size()]);
        if (strWhere.length == 0){
            strWhere = null;
        }

        //SQL発行
        Cursor cursorAll = db.rawQuery(sql.toString(), strWhere);

        while (cursorAll.moveToNext()) {
            AssetModel assetModel  = new AssetModel();

            //各項目を設定する
            assetModel.setAssetNo(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO")));
            assetModel.setAssetID(cursorAll.getString(cursorAll.getColumnIndex("ASSET_ID")));
            assetModel.setAssetName(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NAME")));
            assetModel.setPhyinvFlag(cursorAll.getString(cursorAll.getColumnIndex("PHYINV_FLAG")));         //20191209 Add Sasaki
            assetModel.setPhyinvStatus(cursorAll.getString(cursorAll.getColumnIndex("PHYINV_STATUS")));     //20191209 Add Sasaki
            assetModel.setSubAssetNo(cursorAll.getString(cursorAll.getColumnIndex("SUB_ASSET_NO")));

            //資産マスタデータに溜め込む
            _mapInvAll.put(assetModel.getAssetNo(), assetModel);
            _mapInvAllBySubNo.put(assetModel.getSubAssetNo(), assetModel);

            //資産ID紐付データに溜め込む
            _mapLinkID.put(assetModel.getAssetID(), assetModel.getAssetNo());

        }
    }

    /**
     * 棚卸データテーブル登録
     * @param db
     */
    private void InsertInventoryTable(SQLiteDatabase db, EntryInventDataDetail value){

        //実棚していないデータは登録しない
        //if (value.getphyinvTs() == "") {
        //    return;
        //}

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" INSERT");
        sql.append(" INTO INVENTORY(");
        sql.append("  ASSET_NO,");
        sql.append("  ASSET_NAME,");
        sql.append("  LEASE_CNT,");
        sql.append("  PHYINV_TS,");
        sql.append("  PHYINV_STATUS,");
        sql.append("  PHYINV_LOC_L_CD,");
        sql.append("  PHYINV_LOC_M_CD,");
        sql.append("  PHYINV_LOC_S_CD,");
        sql.append("  PHYINV_EMPLOYEE_CD,");
        sql.append("  INS_DATETIME,");
        sql.append("  INS_EMPLOYEE_CD,");
        sql.append("  INS_EMPLOYEE_NAME,");
        sql.append("  UPD_DATETIME,");
        sql.append("  UPD_EMPLOYEE_CD,");
        sql.append("  UPD_EMPLOYEE_NAME,");
        sql.append("  UPD_TASK");
        sql.append("  ) ");
        sql.append(" VALUES (");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" 0,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        //2021/11/08 UPD Start Suzuki
//        sql.append(" datetime('now'),");
        sql.append(" datetime('now', 'localtime'),");
        //2021/11/08 UPD End Suzuki
        sql.append(" ?,");
        sql.append(" ?,");
        //2021/11/08 UPD Start Suzuki
//        sql.append(" datetime('now'),");
        sql.append(" datetime('now', 'localtime'),");
        //2021/11/08 UPD End Suzuki
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?");
        sql.append(" )");

        //SQL発行
        db.execSQL(sql.toString(), new String[]{
                value.getan(),
                value.getAssetName(),
                value.getpt(),
                value.getps(),
                value.getlc(),
                value.getmc(),
                value.getsc(),
                value.getec(),
                "0001",
                "test",
                "0001",
                "test",
                "登録"});

    }

    /**
     * HT蓄積データテーブルの削除
     * @param db
     * @param strAssetNo 削除対象の資産番号
     */
    private void DeleteInventoryTable(SQLiteDatabase db, String strAssetNo){

        //HT蓄積データテーブルの削除
        db.delete("INVENTORY", "Asset_NO = ?", new String[] {strAssetNo});
    }

    /**
     * 棚卸したデータを追加する
     * @param activity
     * @param AssetNo
     * @param LocLCD
     * @param LocMCD
     * @param LocSCD
     * @param strStatus
     */
    public void StockData(Activity activity, String AssetNo, String LocLCD, String LocMCD, String LocSCD, String strStatus, SQLiteDatabase db) {
        EntryInventDataDetail inventory = new EntryInventDataDetail();

         //棚卸情報を設定する
        inventory.setAssetNo(AssetNo);
        inventory.setphyinvTs(Common.getNowTime("yyyyMMddHHmmss"));
        inventory.setphyinvStatus(strStatus);
        inventory.setphyinvEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));
        inventory.setphyinvLocLCd(LocLCD);
        inventory.setPhyinvLocMCd(LocMCD);
        inventory.setphyinvLocSCd(LocSCD);

        AssetModel assetModel=  _mapInvAll.get(AssetNo);

        if (assetModel != null) {
            inventory.setAssetName(assetModel.getAssetName());
        } else {
            AssetModel subAssetModel = _mapInvAllBySubNo.get(AssetNo);
            if (subAssetModel != null) {
                String assetNo = GetAssetNoBySubAssetNo(db, AssetNo);
                inventory.setAssetNo(assetNo);
                inventory.setAssetName(subAssetModel.getAssetName());
            }
        }

         //棚卸を追加する
        _mapInv.put(AssetNo, inventory);
    }

    /**
     * 棚卸したデータを追加する（ロケ指定なし）
     * @param activity
     * @param AssetNo
     */
    public void StockData(Activity activity, String AssetNo) {
        EntryInventDataDetail inventory = new EntryInventDataDetail();

        //棚卸情報を設定する
        inventory.setAssetNo(AssetNo);
        inventory.setphyinvTs(Common.getNowTime("yyyyMMddHHmmss"));
        inventory.setphyinvStatus(CommonConst.PHYINV_STATUS.Normal.getString());
        inventory.setphyinvEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));

        AssetModel assetModel=  _mapInvAll.get(AssetNo);
        if (assetModel != null) {
            inventory.setAssetName(assetModel.getAssetName());
            inventory.setphyinvLocLCd(assetModel.gettLocLCd());
            inventory.setPhyinvLocMCd(assetModel.gettLocMCd());
            inventory.setphyinvLocSCd(assetModel.gettLocSCd());
        }

        //棚卸を追加する
        _mapInv.put(AssetNo, inventory);
    }

    /**
     * 棚卸OKデータを追加する
     * @param activity
     * @param AssetNo
     * @param LocLCD
     * @param LocMCD
     * @param LocSCD
     */
    public void StockDataOK(Activity activity, String AssetNo, String LocLCD, String LocMCD, String LocSCD) {
        EntryInventDataDetail inventory = new EntryInventDataDetail();

        //ロケステータスを更新する
        inventory.setAssetNo(AssetNo);
        inventory.setphyinvTs(Common.getNowTime("yyyyMMddHHmmss"));
        inventory.setphyinvStatus(CommonConst.PHYINV_STATUS.Normal.getString());
        inventory.setphyinvEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));
        inventory.setphyinvLocLCd(LocLCD);
        inventory.setPhyinvLocMCd(LocMCD);
        inventory.setphyinvLocSCd(LocSCD);

        AssetModel assetModel=  _mapInvAll.get(AssetNo);
        if (assetModel != null) {
            inventory.setAssetName(assetModel.getAssetName());
        }

        //棚卸OKデータを追加する
        _mapInvOK.put(AssetNo, inventory);
    }

    /**
     * 棚卸ロケエラーデータを追加する
     * @param activity
     * @param AssetNo
     * @param LocLCD
     * @param LocMCD
     * @param LocSCD
     */
    public void StockDataLocErr(Activity activity, String AssetNo, String LocLCD, String LocMCD, String LocSCD, SQLiteDatabase db) {
        EntryInventDataDetail inventory = new EntryInventDataDetail();

        //ロケステータスを更新する
        inventory.setAssetNo(AssetNo);
        inventory.setSubAssetNo(AssetNo);
        inventory.setphyinvTs(Common.getNowTime("yyyyMMddHHmmss"));
        inventory.setphyinvStatus(CommonConst.PHYINV_STATUS.Warning.getString());
        inventory.setphyinvEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));
        inventory.setphyinvLocLCd(LocLCD);
        inventory.setPhyinvLocMCd(LocMCD);
        inventory.setphyinvLocSCd(LocSCD);

        AssetModel assetModel=  _mapInvAll.get(AssetNo);
        String key = AssetNo;
        if (assetModel != null) {
            inventory.setAssetName(assetModel.getAssetName());
            String subAssetNo = GetSubAssetNoByAssetNo(db, AssetNo);
            inventory.setSubAssetNo(subAssetNo);
        } else {
            AssetModel subAssetModel=  _mapInvAllBySubNo.get(AssetNo);
            if (subAssetModel != null) {
                String assetNo = GetAssetNoBySubAssetNo(db, AssetNo);
                key = assetNo;
                inventory.setAssetNo(assetNo);
                inventory.setAssetName(subAssetModel.getAssetName());
            }
        }

        //棚卸ロケエラーデータを追加する
        _mapInvLocErr.put(key, inventory);
    }

    /**
     * 実棚OKデータから削除する
     * @param AssetNo
     */
    public void DeleteDataOK(String AssetNo, SQLiteDatabase db) throws ParseException{
        //正常データを削除する
        _mapInvOK.remove(AssetNo);
        String assetNo = GetAssetNoBySubAssetNo(db, AssetNo);
        _mapInvOK.remove(assetNo);
    }

    /**
     * ロケエラーデータから削除する
     * @param AssetNo
     */
    public void DeleteDataLocErr(String AssetNo, SQLiteDatabase db) throws ParseException{
        //ロケエラーデータを削除する
        _mapInvLocErr.remove(AssetNo);
        String assetNo = GetAssetNoBySubAssetNo(db, AssetNo);
        _mapInvLocErr.remove(assetNo);
    }

    /**
     * 在庫なしエラーデータから削除する
     * @param AssetNo
     */
    public void DeleteDataNone(String AssetNo, SQLiteDatabase db) throws ParseException{
        //在庫なしエラーデータを削除する
        _mapInvNone.remove(AssetNo);
        String assetNo = GetAssetNoBySubAssetNo(db, AssetNo);
        _mapInvNone.remove(assetNo);
    }

    /**
     * 貸出中(未棚卸)データから削除する     20200305 Add Sasaki V1.2
     * @param AssetNo
     */
    public void DeleteDataLease(String AssetNo, SQLiteDatabase db) throws ParseException{
        //貸出中(未棚卸)データを削除する
        _mapLease.remove(AssetNo);
        String assetNo = GetAssetNoBySubAssetNo(db, AssetNo);
        _mapLease.remove(assetNo);
    }

    /**
     * 棚卸対象件数を取得する
     * @return
     */
    public long TotalCount(){
        return _mapInvOriginal.size();
    }

    /**
     * 棚卸収集件数を取得する
     * @return
     */
    public long InventoryCount() {
        return _mapInv.size();
    }

    /**
     * HT蓄積件数を取得する(DBに登録している件数)
     * @return
     */
    @SuppressLint("Range")
    public long HTStoreCount(SQLiteDatabase db){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  count(*) AS COUNT");
        sql.append(" FROM INVENTORY");

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        cursor.moveToFirst();
        return cursor.getLong(cursor.getColumnIndex("COUNT"));
    }

//20191210 Add Start Sasaki
    /**
     * 棚卸済数を取得する(DBに登録している件数)
     * @return
     */
    @SuppressLint("Range")
    public long InventorySumiCount(SQLiteDatabase db){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  count(*) AS COUNT");
        sql.append(" FROM M_ASSET");
        sql.append(" WHERE PHYINV_FLAG = ?");

        //検索条件
        String[] strWhere = {CommonConst.PHYINV_FLAG.Yes.getString()};

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), strWhere);
        cursor.moveToFirst();
        return cursor.getLong(cursor.getColumnIndex("COUNT"));
    }
//20191210 Add End Sasaki

    /**
     * ①読み取った値の先頭が0の場合、先頭4桁と末尾1桁をカットして検索
     * ②読み取った値の先頭が1の場合、先頭4桁をカットして検索、見つからなければそのままの値で再検索
     * ③読み取った値の先頭0,1以外の場合、そのままの値で検索
     * 何も見つからなければ空文字を返す
     * @return
     */
    @SuppressLint("Range")
    public String GetAssetNoByProcessedData(SQLiteDatabase db, String readData){

        Log.d("READ_DATA", readData);

        String assetNo = "";
        String processedData = "";
        String firstChar = readData.substring(0,1);
        if (firstChar.equals("0")) {
            // 読み取った値の先頭が0の場合、先頭4桁と末尾1桁をカットして検索
            if (readData.length() <= 5) return "";
            processedData = readData.substring(4, readData.length() - 1);
            assetNo = GetAssetNo(db, processedData);
            if (!assetNo.equals("")) {
                // 資産番号で取得できた場合
                return assetNo;
            }
            assetNo = GetAssetNoBySubAssetNo(db, processedData);
            return assetNo;
        } else if (firstChar.equals("1")) {
            // 読み取った値の先頭が1の場合、先頭4桁をカットして検索、見つからなければそのままの値で再検索
            if (readData.length() <= 4) return "";
            processedData = readData.substring(4);
            assetNo = GetAssetNo(db, processedData);
            if (!assetNo.equals("")) {
                // 資産番号で取得できた場合
                return assetNo;
            }
            assetNo = GetAssetNoBySubAssetNo(db, processedData);
            if (!assetNo.equals("")) {
                // 旧資産番号で取得できた場合
                return assetNo;
            }
            // 見つからなければそのままの値で再検索
            processedData = readData;
        } else {
            // 読み取った値の先頭0,1以外の場合、そのままの値で検索
            processedData = readData;
        }

        // そのままの値で検索
        assetNo = GetAssetNo(db, processedData);
        return assetNo;
    }

    /**
     * 引数が旧資産番号の場合はそのデータの資産番号を返す(取得できない場合は空文字を返す)
     * @return
     */
    @SuppressLint("Range")
    public String GetAssetNoBySubAssetNo(SQLiteDatabase db, String readData){



        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO");
        sql.append(" FROM M_ASSET");
        sql.append(" WHERE SUB_ASSET_NO = ?");

        String[] parameters = new String[] {readData};

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), parameters);

        cursor.moveToFirst();
        if (cursor.getCount() <= 0) {
            return "";
        }

        return cursor.getString(cursor.getColumnIndex("ASSET_NO"));
    }

    /**
     * 資産番号からサブ資産番号を取得
     * @return
     */
    @SuppressLint("Range")
    public String GetSubAssetNoByAssetNo(SQLiteDatabase db, String assetNo){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  SUB_ASSET_NO");
        sql.append(" FROM M_ASSET");
        sql.append(" WHERE ASSET_NO = ?");

        String[] parameters = new String[] {assetNo};

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), parameters);

        cursor.moveToFirst();
        if (cursor.getCount() <= 0) {
            return assetNo;
        }

        return cursor.getString(cursor.getColumnIndex("SUB_ASSET_NO"));
    }

    /**
     * 資産番号があればその資産番号を、なければ空文字を返す
     * @return
     */
    @SuppressLint("Range")
    public String GetAssetNo(SQLiteDatabase db, String assetNo){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO");
        sql.append(" FROM M_ASSET");
        sql.append(" WHERE ASSET_NO = ?");

        String[] parameters = new String[] {assetNo};

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), parameters);

        cursor.moveToFirst();
        if (cursor.getCount() <= 0) {
            return "";
        }

        return cursor.getString(cursor.getColumnIndex("ASSET_NO"));
    }

    /**
     * 棚卸ロケエラー収集件数を取得する
     * @return
     */
    public long InventoryCountLocErr() {
        return _mapInvLocErr.size();
    }

    /**
     * 棚卸在庫なし件数を取得する
     * @return
     */
    public long InventoryCountNone() {
        return _mapInvNone.size();
    }

    /**
     * 棚卸正常件数を取得する
     * @return
     */
    public long InventoryCountOK() { return _mapInvOK.size(); }

    /**
     * 棚卸正常件数(元データ)を取得する
     * @return
     */
    public long InventoryCountOKOriginal() { return _mapInvOKOriginal.size(); }

    /**
     * 貸出数(未棚卸)を取得する
     * @return
     */
    public long InventoryCountLease() { return _mapLease.size(); }

    /**
     * 棚卸ロケエラーデータを取得する
     * @return
     */
    public LinkedHashMap<String, EntryInventDataDetail> GetInventoryLocErrData() {
        LinkedHashMap<String, EntryInventDataDetail> hashData = new LinkedHashMap<>();

        // キーでソートする
        Object[] mapkey = _mapInvLocErr.keySet().toArray();
        Arrays.sort(mapkey);

        for( int i = 0; i < mapkey.length; i++) {
            hashData.put(mapkey[i].toString(), _mapInvLocErr.get(mapkey[i].toString()));
        }

        return hashData;
    }

    /**
     * 在庫なしエラーデータを取得する
     * @return
     */
    public LinkedHashMap<String, EntryInventDataDetail> GetInventoryNoneData() {
        LinkedHashMap<String, EntryInventDataDetail> hashData = new LinkedHashMap<>();

        // キーでソートする
        Object[] mapkey = _mapInvNone.keySet().toArray();
        Arrays.sort(mapkey);

        for( int i = 0; i < mapkey.length; i++) {
            hashData.put(mapkey[i].toString(), _mapInvNone.get(mapkey[i].toString()));
        }

        return hashData;
    }

    /**
     * 棚卸収集データをクリアする
     */
    public void Clear(){
        _mapInv.clear();
        _mapInvLocErr.clear();
        _mapInvNone.clear();
        _mapInvOK.clear();
        _mapLease.clear();          //20200305 Add Sasaki V1.2

        //初期データをコピーする
        copyOriginalZumiData();
        copyOriginalNashiData();
        copyOriginalLeaseData();    //20200305 Add Sasaki V1.2

        //読込んだ棚卸データを登録する
/*        AssetModel model = new AssetModel();
        for (String key : _mapInvOriginal.keySet()) {
            EntryInventDataDetail modelOrigin = new EntryInventDataDetail();
            model.setAssetNo(modelOrigin.getan());
            model.setAssetName(modelOrigin.getAssetName());
            model.setPhyinvStatus(modelOrigin.getps());
            _mapInvNone.put(key, modelOrigin);
        }*/
    }

    /**
     * 全てクリアする
     */
    public void AllClear(){
        _mapInvAll.clear();
        _mapInvAllBySubNo.clear();
        _mapInvOriginal.clear();
        _mapInvOriginalBySubNo.clear();
        _mapInv.clear();
        _mapInvOK.clear();
        _mapInvOKOriginal.clear();
        _mapInvLocErr.clear();
        _mapInvNone.clear();
        _mapInvNoneOriginal.clear();
        _mapLinkID.clear();
        _mapLease.clear();              //20200305 Add Sasaki V1.2
        _mapLeaseOriginal.clear();      //20200305 Add Sasaki V1.2
        _LocLCode = "";
        _LocMCode = "";
        _LocSCode = "";
        _LocLName="";
        _LocMName="";
        _LocSName="";
    }

    /**
     * ロケを照合する
     * @param strAssetNo
     * @param strLocLCd
     * @param strLocMCd
     * @param strLocSCd
     * @return
     */
    public boolean ComparisonLocation(String strAssetNo, String strLocLCd, String strLocMCd, String strLocSCd) {
        boolean blnRetun = false;

        //該当の資産データを取得する
        AssetModel AssetData = _mapInvOriginal.get(strAssetNo);

        //ロケが一致しているか照合する
        if (AssetData != null) {
            if (AssetData.gettLocLCd() == strLocLCd
                    && AssetData.gettLocLCd() == strLocMCd
                    && AssetData.gettLocLCd() == strLocSCd) {
                blnRetun = true;
            }
        }

        return blnRetun;
    }

    /**
     * 資産番号が資産に存在するかチェックする
     * @param strAssetNo
     * @return
     */
    public boolean ExistAsset(String strAssetNo) {
        boolean blnRetun = false;

        //該当の資産データを取得する
        AssetModel AssetData = _mapInvAll.get(strAssetNo);
        AssetModel AssetDataSub = _mapInvAllBySubNo.get(strAssetNo);

        if (AssetData == null && AssetDataSub == null) {
            blnRetun = false;
        } else {
            blnRetun = true;
        }

        return blnRetun;
    }

    /**
     * 資産番号が棚卸対象データに存在するかチェックする
     * @param strAssetNo
     * @return
     */
    public boolean ExistInventory(String strAssetNo) {
        boolean blnRetun = false;

        //該当の資産データを取得する
        AssetModel inventData = _mapInvOriginal.get(strAssetNo);
        AssetModel inventDataSub = _mapInvOriginalBySubNo.get(strAssetNo);

        if (inventData == null && inventDataSub == null) {
            blnRetun = false;
        } else {
            blnRetun = true;
        }

        return blnRetun;
    }

    /**
     * 資産番号が読込データに存在するかチェックする
     * @param strAssetNo
     * @return
     */
    public boolean ExistAssetRead(String strAssetNo) {
        boolean blnRetun = false;

        //該当の資産データを取得する
        EntryInventDataDetail AssetData = _mapInv.get(strAssetNo);

        if (AssetData == null) {
            blnRetun = false;
        } else {
            blnRetun = true;
        }

        return blnRetun;
    }

    /**
     * HTの棚卸蓄積データをクリアする
     */
    public void ClearHTSoreData(SQLiteDatabase db){

        //HT蓄積データテーブルの削除
        db.delete("INVENTORY", null, null);

        //内部データもクリアする
        this.Clear();
    }

//20191210 Add Start Sasaki
    /**
     * 正常で実棚済みか確認する
     * @param strAssetNo
     * @return
     */
    public boolean isJittanaZumi(String strAssetNo) {
        boolean blnReturn = false;

        //実棚済みか確認する
        AssetModel modelOriginal = _mapInvOriginal.get(strAssetNo);
        if (modelOriginal != null
                && CommonConst.PHYINV_STATUS.Normal.getString().equals(modelOriginal.getPhyinvStatus())
                && CommonConst.PHYINV_FLAG.Yes.getString().equals(modelOriginal.getPhyinvFlag())) {
            //正常で実棚済みの場合TRUEを返却
            blnReturn = true;
        } else {
            AssetModel subModel = _mapInvOriginalBySubNo.get(strAssetNo);
            if (subModel != null
                    && CommonConst.PHYINV_STATUS.Normal.getString().equals(subModel.getPhyinvStatus())
                    && CommonConst.PHYINV_FLAG.Yes.getString().equals(subModel.getPhyinvFlag())) {
                //正常で実棚済みの場合TRUEを返却
                blnReturn = true;
            }
        }

        //実棚済みか確認する
        AssetModel model = _mapInvAll.get(strAssetNo);
        if (model != null
            && CommonConst.PHYINV_STATUS.Normal.getString().equals(model.getPhyinvStatus())
            && CommonConst.PHYINV_FLAG.Yes.getString().equals(model.getPhyinvFlag())) {
            //正常で実棚済みの場合TRUEを返却
            blnReturn = true;
        } else {
            AssetModel subModel = _mapInvAllBySubNo.get(strAssetNo);
            if (subModel != null
                    && CommonConst.PHYINV_STATUS.Normal.getString().equals(subModel.getPhyinvStatus())
                    && CommonConst.PHYINV_FLAG.Yes.getString().equals(subModel.getPhyinvFlag())) {
                //正常で実棚済みの場合TRUEを返却
                blnReturn = true;
            }
        }

        return blnReturn;
    }
//20191210 Add End Sasaki

//20191210 Add Start Sasaki
    /**
     * 正常で実棚済みか確認する
     * @param strAssetNo
     * @return
     */
    public boolean isJittanaZumiNoLoc(String strAssetNo) {
        boolean blnReturn = false;

        //HT蓄積データをチェックする
        if (_mapStoreData.containsKey(strAssetNo)) {
            blnReturn = true;
        //資産マスタをチェックする
        } else if (isJittanaZumi(strAssetNo)) {
             blnReturn = true;
        }

        return blnReturn;
    }
//20191210 Add End Sasaki

    /**
     * 貸出中で未棚卸かチェックする
     * @return True:貸出+未棚卸、False 貸出+棚卸済み or 貸出以外
     */
    public boolean isLeaseNoInv(String strAssetNo, SQLiteDatabase db) {
        boolean blnReturn = false;

        //貸出中で未棚卸かをチェックする
        if (_mapLease.containsKey(strAssetNo)) {
            blnReturn = true;
        }
        String assetNo = GetAssetNoBySubAssetNo(db, strAssetNo);
        if (_mapLease.containsKey(assetNo)) {
            blnReturn = true;
        }

        return blnReturn;
    }

    /**
     * 資産IDから資産番号を取得する
     * @param strAssetID
     * @return 資産番号
     */
    public String GetAssetNoFromID(String strAssetID){
        return _mapLinkID.get(strAssetID);
    }

    /**
     * ロケ大区分コードを設定する
     * @param LocLCode
     */
    public void setLocLKbnCd(String LocLCode) {
        this._LocLCode = LocLCode;
    }

    /**
     * ロケ大区分を取得する
     * @return
     */
    public String getLocLCode() {
        return this._LocLCode;
    }

    /**
     * ロケ大区分名称を設定する
     * @param LocLName
     */
    public void setLocLKbnName(String LocLName) {
        this._LocLName = LocLName;
    }

    /**
     * ロケ大区分名称を取得する
     * @return
     */
    public String getLocLName() {
        return this._LocLName;
    }

    /**
     * ロケ中区分コードを設定する
     * @param LocMCode
     */
    public void setLocMKbnCd(String LocMCode) {
        this._LocMCode = LocMCode;
    }

    /**
     * ロケ中区分を取得する
     * @return
     */
    public String getLocMCode() {
        return this._LocMCode;
    }

    /**
     * ロケ中区分名称を設定する
     * @param LocMName
     */
    public void setLocMKbnName(String LocMName) {
        this._LocMName = LocMName;
    }

    /**
     * ロケ中区分名称を取得する
     * @return
     */
    public String getLocMName() {
        return this._LocMName;
    }

    /**
     * ロケ小区分コードを設定する
     * @param LocSCode
     */
    public void setLocSKbnCd(String LocSCode) {
        this._LocSCode = LocSCode;
    }

    /**
     * ロケ小区分を取得する
     * @return
     */
    public String getLocSCode() {
        return this._LocSCode;
    }

    /**
     * ロケ小区分名称を設定する
     * @param LocSName
     */
    public void setLocSKbnName(String LocSName) {
        this._LocSName = LocSName;
    }

    /**
     * ロケ小区分名称を取得する
     * @return
     */
    public String getLocSName() {
        return this._LocSName;
    }

//20191210 Add Start Sasaki
    /**
     * 棚卸継続有無を設定する
     * @param strData
     */
    public void setInventoryContinue(String strData) {
        _InventoryContinue = strData;
    }

    /**
     * 棚卸継続有無を取得する
     * @return
     */
    public String getInventoryContinue() {
        return _InventoryContinue;
    }
//20191210 Add End Sasaki

//20200107 Add Start Sasaki
    /**
     * 正常データの初期値をコピーする
     */
    public void copyOriginalZumiData() {
        _mapInvOK = (HashMap<String, EntryInventDataDetail>)_mapInvOKOriginal.clone();
    }

    /**
     * 在庫なしデータの初期値をコピーする
     */
    public void copyOriginalNashiData() {
        _mapInvNone = (HashMap<String, EntryInventDataDetail>)_mapInvNoneOriginal.clone();
    }
//20200107 Add End Sasaki
    /**
     * 貸出データの初期値をコピーする    20200305 Add Sasaki V1.2
     */
    public void copyOriginalLeaseData() {
        _mapLease = (HashMap<String, EntryInventDataDetail>)_mapLeaseOriginal.clone();
    }
}
