package com.rfid.Factory;

import android.annotation.SuppressLint;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.AssetModel;
import com.rfid.common.CommonConst;
import com.rfid.common.bean.EntryProcessDataDetail;

import org.json.JSONArray;
import org.json.JSONException;

import java.util.HashMap;
import java.util.Iterator;

public class AssetFactory {

    /**
     * 資産マスタのデータを更新する
     * @param db
     * @param map
     * @param status
     */
    public static void Update(SQLiteDatabase db, HashMap<String, EntryProcessDataDetail>map, CommonConst.ASSET_STATUS status) {
        Log.d("ASSET_UPDATE", "START");

        //件数分ループして更新する
        for (Iterator<EntryProcessDataDetail> iterator = map.values().iterator(); iterator.hasNext(); ) {

            //1件取得する
            EntryProcessDataDetail data = iterator.next();

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" UPDATE ");
            sql.append(CommonConst.MASTER_ASSET);
            sql.append(" SET ASSET_STATUS = ?");
            //貸出の場合、貸出回数を更新する
            if (CommonConst.ASSET_STATUS.Lease.getString().equals(status.getString())) {
                sql.append(" , LEASE_CNT = ?");
            }
            sql.append(" WHERE ASSET_NO = ?");

            //パラメーター
            String[] param = null;
            if (CommonConst.ASSET_STATUS.Lease.getString().equals(status.getString())) {
                //貸出の場合
                param = new String[]{
                        status.getString(),       //資産状態
                        String.valueOf(data.getLeaseCount() + 1), //貸出回数
                        data.getAssetNo()         //資産番号
                };
            } else {
                //貸出以外の場合
                param = new String[]{
                        status.getString(),    //資産状態
                        data.getAssetNo()         //資産番号
                };
            }

            //SQL発行
            db.execSQL(sql.toString(), param);

            Log.d("ASSET_UPDATE:ASSET_STATUS:",  data.getAssetNo() + "⇒" + data.getAssetStatus());
        }
    }

    /**
     * 貸出(照合)読取画面クリア処理用、資産マスタのデータを更新する
     * @param db
     * @param AseetNo
     */
    public static void ClearUpdate(SQLiteDatabase db, String AseetNo) {
        Log.d("ASSET_UPDATE", "START");

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" UPDATE ");
            sql.append(CommonConst.MASTER_ASSET);
            sql.append(" SET LEASE_CNT = LEASE_CNT - 1 ");
            sql.append(" WHERE ASSET_NO = ?");

            //パラメーター
            String[] param = null;

                //貸出の場合
                param = new String[]{
                        AseetNo         //資産番号
                };

            //SQL発行
            db.execSQL(sql.toString(), param);

            Log.d("ASSET_UPDATE:LEASE_CNT:",  "貸出件数デクリメント");

    }

    /**
     * 資産マスタのデータを更新する
     * @param db
     * @param map
     * @param status
     */
    public static void UpdateStrage(SQLiteDatabase db, HashMap<String, EntryProcessDataDetail>map, CommonConst.ASSET_STATUS status) {
        Log.d("ASSET_UPDATE_STRAGE", "START");

        //件数分ループして更新する
        for (Iterator<EntryProcessDataDetail> iterator = map.values().iterator(); iterator.hasNext(); ) {

            //1件取得する
            EntryProcessDataDetail data = iterator.next();

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" UPDATE ");
            sql.append(CommonConst.MASTER_ASSET);
            sql.append(" SET ASSET_STATUS = ?");
            sql.append(" ,LOC_L_CD = ?");
            sql.append(" ,LOC_M_CD = ?");
            sql.append(" ,LOC_S_CD = ?");
            sql.append(" WHERE ASSET_NO = ?");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    status.getString(),    //資産状態
                    data.getLocLCd(),      //ロケ大区分
                    data.getLocMCd(),      //ロケ中区分
                    data.getLocSCd(),       //ロケ小区分
                    data.getAssetNo()     //資産番号
            });

            Log.d("ASSET_UPDATE_STRAGE:ASSET_STATUS:",  data.getAssetNo() + "⇒" + data.getAssetStatus());
        }
    }

    /**
     * 資産マスタのデータを更新する
     * @param db
     * @param assetModel
     * @param status
     */
    public static void Update(SQLiteDatabase db, AssetModel assetModel, CommonConst.ASSET_STATUS status) {
        Log.d("ASSET_UPDATE", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" UPDATE ");
        sql.append(CommonConst.MASTER_ASSET);
        sql.append(" SET ASSET_STATUS = ?");
        sql.append(" ,ASSET_ID = ?");
        sql.append(" WHERE ASSET_NO = ?");

        //SQL発行
        db.execSQL(sql.toString(), new String[]{
                status.getString(),         //資産状態
                assetModel.getAssetID(),    //資産ID
                assetModel.getAssetNo()     //資産番号
        });

        Log.d("ASSET_UPDATE:ASSET_STATUS:",  assetModel.getAssetNo() + "⇒" + assetModel.getAssetID() + ":" + status.getString());
    }

//20191210 Add Start Sasaki
    /**
     * 資産マスタのデータを更新する(棚卸用)
     * @param db
     * @param assetModel
     */
    public static void UpdateInventory(SQLiteDatabase db, AssetModel assetModel) {
        Log.d("UpdateInventory", "START");

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" UPDATE ");
        sql.append(CommonConst.MASTER_ASSET);
        sql.append(" SET PHYINV_FLAG = ?");
        sql.append(" ,PHYINV_STATUS = ?");
        sql.append(" WHERE ASSET_NO = ?");

        //SQL発行
        db.execSQL(sql.toString(), new String[]{
            assetModel.getPhyinvFlag(),           //実棚済みフラグ
            assetModel.getPhyinvStatus(),   //実棚ステータス
            assetModel.getAssetNo()         //資産番号
        });

        Log.d("UpdateInventory",  assetModel.getAssetNo() + "⇒" + assetModel.getPhyinvFlag() + ":" + assetModel.getPhyinvStatus());
    }
//20191210 Add End Sasaki

    /**
     * 資産番号をキーに資産マスタのデータを取得する
     * @param db
     * @param strAssetNo
     */
    @SuppressLint("Range")
    public static AssetModel Select(SQLiteDatabase db, String strAssetNo) {
        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("   MS_A.ASSET_NO");
        sql.append("  ,MS_A.ASSET_NAME");
        sql.append("  ,MS_A.ASSET_ID");
        sql.append("  ,MS_A.LOC_L_CD");
        sql.append("  ,MS_A.LOC_M_CD");
        sql.append("  ,MS_A.LOC_S_CD");
        sql.append("  ,MS_A.EPC_SIZE");
        sql.append("  ,MS_A.ASSETID_REG_DIB");
        sql.append("  ,MS_A.LEASE_STATUS");
        sql.append("  ,MS_A.ASSET_STATUS");
        sql.append("  ,MS_A.SUB_ASSET_NO");
        sql.append("  ,MS_A.IS_TANGIBLE");
        sql.append("  ,MS_A.INV_FLG");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_ASSET).append(" MS_A ");
        //20210918 ADD Takase V2.0 START
        sql.append(" INNER JOIN ");
        sql.append(CommonConst.MASTER_LOCATION).append(" MS_L ");
        sql.append(" ON ");
        sql.append("  MS_A.LOC_L_CD = MS_L.LOC_L_CD");
        //20210918 ADD Takase V2.0 END
        sql.append(" WHERE MS_A.ASSET_NO = ?");

        //貸出先検索
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strAssetNo != null ? strAssetNo : ""});

        AssetModel assetModel  = new AssetModel();
        while (cursor.moveToNext()) {
            //各項目を設定する
            assetModel.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));
            assetModel.setAssetID(cursor.getString(cursor.getColumnIndex("ASSET_ID")));
            assetModel.setAssetName(cursor.getString(cursor.getColumnIndex("ASSET_NAME")));
            assetModel.setLocLCd(cursor.getString(cursor.getColumnIndex("LOC_L_CD")));
            assetModel.setLocMCd(cursor.getString(cursor.getColumnIndex("LOC_M_CD")));
            assetModel.setLocSCd(cursor.getString(cursor.getColumnIndex("LOC_S_CD")));
            assetModel.setEpcSize(cursor.getString(cursor.getColumnIndex("EPC_SIZE")));
            assetModel.setAssetIdRegDiv(cursor.getString(cursor.getColumnIndex("ASSETID_REG_DIB")));
            assetModel.setLeaseStatus(cursor.getString(cursor.getColumnIndex("LEASE_STATUS")));     //20210918 ADD Takase V2.0
            assetModel.setAssetStatus(cursor.getString(cursor.getColumnIndex("ASSET_STATUS")));
            assetModel.setSubAssetNo(cursor.getString(cursor.getColumnIndex("SUB_ASSET_NO")));
            assetModel.setIsTangible(cursor.getString(cursor.getColumnIndex("IS_TANGIBLE")));
            assetModel.setInvFlg(cursor.getString(cursor.getColumnIndex("INV_FLG")));
        }

        return assetModel;
    }

    //20210918 ADD Takase V2.0 START
    /**
     * 資産IDを資産番号に変換する
     * @param db
     * @param strAssetId
     */
    @SuppressLint("Range")
    public static String Convert(SQLiteDatabase db, String strAssetId) {
        String strAssetNo = "";

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_ASSET);
        sql.append(" WHERE ASSET_ID = ?");

        //貸出先検索
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strAssetId});

        while (cursor.moveToNext()) {
            strAssetNo = cursor.getString(cursor.getColumnIndex("ASSET_NO"));
        }

        return strAssetNo;
    }
    //20210918 ADD Takase V2.0 END

    /**
     * 資産番号を資産IDに変換する
     * @param db
     * @param strAssetNo
     */
    @SuppressLint("Range")
    public static String ConvertId(SQLiteDatabase db, String strAssetNo) {
        String strAssetId = "";

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_ID");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_ASSET);
        sql.append(" WHERE ASSET_NO = ?");

        //貸出先検索
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strAssetNo});

        while (cursor.moveToNext()) {
            strAssetId = cursor.getString(cursor.getColumnIndex("ASSET_ID"));
        }

        return strAssetId;
    }

    /**
     * 資産マスタにデータを登録する
     * @param db
     * @param json
     * @throws JSONException
     */
    public static void insertAssetMaster(SQLiteDatabase db, JSONArray json) throws JSONException {

        //件数分ループして登録する
        for (int i = 0; i < json.length(); i++) {

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" INSERT");
            sql.append(" INTO M_ASSET(");
            sql.append("  ASSET_NO,");
            sql.append("  ASSET_NAME,");
            sql.append("  ASSET_ID,");
            sql.append("  ASSET_STATUS,");
            sql.append("  LOC_L_CD,");
            sql.append("  LOC_M_CD,");
            sql.append("  LOC_S_CD,");
            sql.append("  EPC_SIZE,");
            sql.append("  ASSETID_REG_DIB,");
            sql.append("  PHYINV_FLAG,");           //20191206 Add Sasaki
            sql.append("  PHYINV_STATUS,");         //20191206 Add Sasaki
            sql.append("  LEASE_CNT,");             //20200225 Add Sasaki
            sql.append("  LEASE_UPPER_ERR_DIV,");   //20200225 Add Sasaki
            sql.append("  LEASE_UPPER_LIMIT,");     //20200225 Add Sasaki
            sql.append("  MNG_LOC_L_CD,");          //20210918 ADD Takase V2.0
            sql.append("  LEASE_STATUS,");           //20210918 ADD Takase V2.0
            sql.append("  LEASE_DUE_DT,");          //20230831 ADD Kawamoto V2.4.1
            sql.append("  MODEL_NO,");          //20241128 ADD shimomura V4.0
            sql.append("  LEASE_LIMIT,");           //20230831 ADD Kawamoto V2.4.1
            sql.append("  GENERIC_LIMIT,");          //20230831 ADD Kawamoto V2.4.1
            sql.append("  LEASE_UPPER_ALERT_LIMIT,");  //20230911 ADD okubo  V2.4.1
            sql.append("  SUB_ASSET_NO,");
            sql.append("  IS_TANGIBLE,");
            sql.append("  INV_FLG ");
            sql.append("  ) ");
            sql.append(" VALUES (");
            sql.append(" ?,");                  //資産番号
            sql.append(" ?,");                  //資産名称
            sql.append(" ?,");                  //資産ID
            sql.append(" ?,");                  //資産ステータス
            sql.append(" ?,");                  //ロケ大区分コード
            sql.append(" ?,");                  //ロケ中区分コード
            sql.append(" ?,");                  //ロケ小区分コード
            sql.append(" ?,");                  //EPC SIZE
            sql.append(" ?,");                  //資産ID書込み区分
            sql.append(" ?,");                  //実棚フラグ
            sql.append(" ?,");                  //実棚ステータス
            sql.append(" ?,");                  //貸出回数
            sql.append(" ?,");                  //貸出上限エラー区分
            sql.append(" ?,");                  //貸出上限数
            sql.append(" ?,");                  //管理ロケ大区分コード  20210918 ADD Takase V2.0
            sql.append(" ?,");                  //貸出状態              20210918 ADD Takase V2.0
            sql.append(" ?,");                  //貸出返却予定日       20230831 ADD Kawamoto V2.4.1
            sql.append(" ?,");                  //型番                20241128 ADD shimomura V4.0
            sql.append(" ?,");                  //リース期限          20230831 ADD Kawamoto V2.4.1
            sql.append(" ?,");                  //○○期限            20230831 ADD Kawamoto V2.4.1
            sql.append(" ?,");                  //貸出上限警告数       20230911 ADD okubo  V2.4.1
            sql.append(" ?,");                  //資産サブ番号
            sql.append(" ?,");                  //有形/無形区分
            sql.append(" ? ");                  //棚卸対象区分
            sql.append(" )");

            //SQL発行
            db.execSQL(sql.toString(), new String[]{
                    json.getJSONObject(i).getString("no"),             //資産番号
                    json.getJSONObject(i).getString("nm"),             //資産名称
                    json.getJSONObject(i).getString("id"),             //資産ID
                    json.getJSONObject(i).getString("st"),             //資産ステータス
                    json.getJSONObject(i).getString("llcd"),           //ロケ大区分
                    json.getJSONObject(i).getString("lmcd"),           //ロケ中区分
                    json.getJSONObject(i).getString("lscd"),           //ロケ小区分
                    json.getJSONObject(i).getString("sz"),             //EPCSize
                    json.getJSONObject(i).getString("reg"),            //資産ID書込み区分
                    json.getJSONObject(i).getString("pif"),            //実棚フラグ
                    json.getJSONObject(i).getString("pis"),            //実棚ステータス
                    json.getJSONObject(i).getString("ln"),             //貸出回数
                    json.getJSONObject(i).getString("ule"),            //貸出上限エラー区分
                    json.getJSONObject(i).getString("ul"),             //貸出上限数
                    json.getJSONObject(i).getString("mll"),            //管理ロケ大区分コード   20210918 ADD Takase V2.0
                    json.getJSONObject(i).getString("ls"),             //貸出状態            20210918 ADD Takase V2.0
                    json.getJSONObject(i).getString("rdt"),            //貸出返却予定日       20230831 ADD Kawamoto V2.4.1
                    json.getJSONObject(i).getString("mn"),             //型番       20230831 ADD Shimomura V4.0
                    json.getJSONObject(i).getString("llt"),            //リース期限          20230831 ADD Kawamoto V2.4.1
                    json.getJSONObject(i).getString("qlt"),            //○○期限            20230831 ADD Kawamoto V2.4.1
                    json.getJSONObject(i).getString("ual") ,           //貸出上限警告数       20230911 ADD okubo  V2.4.1
                    json.getJSONObject(i).getString("subno"),          //資産サブ番号
                    json.getJSONObject(i).getString("istangible"),     //有形/無形区分
                    json.getJSONObject(i).getString("invflg")          //棚卸対象区分
            });

            Log.d("insertAssetMaster:" + i, json.getJSONObject(i).toString());
        }
    }

    public static boolean existAssetID(SQLiteDatabase db, String strAssetID){
        boolean blnReturn = false;

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_ID");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_ASSET);
        sql.append(" WHERE ASSET_ID = ?");

        //貸出先検索
        Cursor cursor = db.rawQuery(sql.toString(), new String[]{strAssetID});

        //レコードが存在する場合
        if (cursor.getCount() > 0) {
            blnReturn = true;
        }

        return blnReturn;
    }

    /**
     * 資産マスタのデータを削除する
     * @param db
     */
    public static void deleteAssetMaster(SQLiteDatabase db) {
        Log.d("deleteAssetMaster", "START");

        //資産マスタの削除
        db.delete("M_ASSET", null, null);

        Log.d("deleteAssetMaster", "END");
    }
}
