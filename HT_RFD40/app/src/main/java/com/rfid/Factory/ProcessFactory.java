package com.rfid.Factory;

import android.app.Activity;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.text.TextUtils;
import android.util.Log;

import com.rfid.common.AssetModel;
import com.rfid.common.CommonAuthority;
import com.rfid.common.CommonConst;
import com.rfid.common.DatabaseHelper;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.EntryMatchProcessData;
import com.rfid.common.bean.EntryMatchProcessDataDetail;
import com.rfid.common.bean.EntryProcessData;
import com.rfid.common.bean.EntryProcessDataDetail;
import com.rfid.setting.SettingValue;

import java.io.Serializable;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;

public class ProcessFactory implements Serializable {

    private static HashMap<String, String> _mapAssetMasterAll = new HashMap<String, String>(); //全資産データ 20191126 Add Sasaki
    private static HashMap<String, String> _mapCollation = new HashMap<String, String>(); //照合データ 20241018 Add Shirakawa
    private static HashMap<String, String> _mapCollationView = new HashMap<String, String>(); //読み取り照合データ(画面表示用) 20241018 Add Shirakawa
    private static Map<String, String> _SortMap1 = new HashMap<String, String>();    //照合機能用一覧並び替え用Map 昇順1
    private static Map<String, String> _SortMap2 = new HashMap<String, String>();    //照合機能用一覧並び替え用Map 昇順2
    private static Map<String, String> _SortMap4 = new HashMap<String, String>();    //照合機能用一覧並び替え用Map　昇順4
    private static HashMap<String, EntryProcessDataDetail> _mapOK = new HashMap<String, EntryProcessDataDetail>();          //貸出OKデータ
    private static LinkedHashMap<String, EntryProcessDataDetail> _mapAllData = new LinkedHashMap<String, EntryProcessDataDetail>();    //貸出データ
    private static List<String> _listMatAssetIDRead = new ArrayList<String>();                       //未読み取り照合資産ID読込データ (照合データ)
    private static List<String> _listMatAssetNORead = new ArrayList<String>();                       //未読み取り照合資産NO読込データ (照合データ)
    private static List<String> _listTempList = new ArrayList<String>();                       //すべての仮置き済みデータ
    private static List<String> _SortList3 = new ArrayList<String>();    //一覧並び替え用List　昇順3
    private static List<String> _listAssetIDRead = new ArrayList<String>();                       //資産ID読込データ
    private static List<String> _listAssetNoRead = new ArrayList<String>();                       //資産No読込データ
    private static List<String> _listAssetMasterRead  = new ArrayList<String>();                  //資産マスタあり読込データ
    private static List<String> _listLeaseLimitErr  = new ArrayList<String>();                    //資産マスタ貸出上限エラーデータ      20200226 Add Sasaki V1.2
    private static List<String> _listLeaseLimitWarning  = new ArrayList<String>();                //資産マスタ貸出上限警告データ        20200226 Add Sasaki V1.2
    private static List<String> _listNoAuth = new ArrayList<String>();                            //権限無しエラーデータ                20210918 ADD Takase V2.0
    private static List<String> _listLeaseUntilErr  = new ArrayList<String>();                  //リース期限エラーデータ 20230830 ADD Kawamoto v2.4.1
    private static List<String> _listGenericUntilErr  = new ArrayList<String>();                //○○期限エラーデータ 20230830 ADD Kawamoto v2.4.1
    private static List<String> _listReturnUntilErr  = new ArrayList<String>();                 //返却期限エラーデータ 20230908 ADD okubo v2.4.1
    private static String _LeaseDestination = "";                                             //貸出先コード
    private static String _LeaseDestinationName = "";                                        //貸出先名称
    private static String _LeaseEmployee = "";                                                 //貸出担当コード
    private static String _LeaseEmployeeName = "";                                            //貸出担当名称
    private static String _LeaseReturn = "";                                                   //貸出返却予定日
    private static String _Remark = "";                                                        //備考
    private static String _LocLCode = "";                                                      //ロケ大区分
    private static String _LocMCode = "";                                                      //ロケ中区分
    private static String _LocSCode = "";                                                      //ロケ小区分
    private static String _LocLName = "";                                                      //ロケ大区分名称
    private static String _LocMName = "";                                                      //ロケ中区分名称
    private static String _LocSName = "";                                                      //ロケ小区分名称
    private static String _ProsessId = "";                                                      //現在のプロセスID

    private static String _TempDt = "";                                                         //仮置き日
//    private static String _TempWorker = "";                                                     //責任者
    private static String _TempInfo1 = "";                                                      //仮置き情報1
    private static String _TempInfo2 = "";                                                      //仮置き情報2
    private static String _TempInfo3 = "";                                                      //仮置き情報3
    private static String _TempInfo4 = "";                                                      //仮置き情報4
    private static String _TempInfo5 = "";                                                      //仮置き情報5
    private static String _TempInfo6 = "";                                                      //仮置き情報6
    private static String _TempInfo7 = "";                                                      //仮置き情報7

    private static int _ListCnt = 0;                                                        //照合一致数
    private static int _NoListCnt = 0;                                                     //リスト不一致数
    private static int _AlreadyTempCnt = 0;                                                     //他仮置き数
    private static boolean _isLease = false;                                                 //「貸出」かどうか                     20210918 ADD Takase V2.0
    private static boolean _isReturn = false;                                                //「返却」かどうか     20230906 ADD okubo V2.4.1
    private static boolean _isMatLease = false;                                                 //「貸出(照合)」かどうか

    private static boolean _isTemporary = false;                                                 //「仮置き」かどうか


    /**
     * 処理履歴データを取得する
     * @param db
     */
    public EntryProcessData SelectData(SQLiteDatabase db){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  PROCESS_LOG_ID");                 //処理履歴ID
        sql.append("  ,PROCESS_DT");                    //処理日
        sql.append("  ,PROCESS_TM");                    //処理時刻
        sql.append("  ,PROCESS_TOOL_DIV");             //処理ツール区分
        sql.append("  ,ASSET_NO");                      //資産番号
        sql.append("  ,ASSET_ID");                      //資産ID
        sql.append("  ,PROCESS_DIV");                   //処理区分
        sql.append("  ,LEASE_PLACE_CD");               //貸出先場所コード
        sql.append("  ,LEASE_EMPLOYEE_CD");            //貸出者コード
        sql.append("  ,LEASE_DUE_DT");                  //返却予定日
        sql.append("  ,REMARK");                        //備考
        sql.append("  ,LOC_L_CD");                      //ロケ大区分コード
        sql.append("  ,LOC_M_CD");                      //ロケ中区分コード
        sql.append("  ,LOC_S_CD");                      //ロケ小区分コード
        sql.append("  ,EMPLOYEE_CD");                   //ログイン担当者コード
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_PROCESS_LOG);

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        //取得したデータを処理履歴クラスに格納する
        EntryProcessData map = new EntryProcessData();
        while (cursor.moveToNext()) {
            EntryProcessDataDetail data = new EntryProcessDataDetail();

            //各項目を設定する
            data.setProcessLogId(cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID")));        //処理履歴ID
            data.setProcessDt(cursor.getString(cursor.getColumnIndex("PROCESS_DT")));                //処理日
            data.setProcessTm(cursor.getString(cursor.getColumnIndex("PROCESS_TM")));                //処理時刻
            data.setProcessToolDiv(cursor.getString(cursor.getColumnIndex("PROCESS_TOOL_DIV")));   //処理ツール区分
            data.setAssetNo(cursor.getString(cursor.getColumnIndex("ASSET_NO")));                    //資産番号
            data.setAssetId(cursor.getString(cursor.getColumnIndex("ASSET_ID")));                    //資産ID
            data.setProcessDiv(cursor.getString(cursor.getColumnIndex("PROCESS_DIV")));              //処理区分
            data.setLeasePlaceCd(cursor.getString(cursor.getColumnIndex("LEASE_PLACE_CD")));        //貸出先場所コード
            data.setLeaseEmployeeCd(cursor.getString(cursor.getColumnIndex("LEASE_EMPLOYEE_CD")));  //貸出社コード
            data.setLeaseDueDt(cursor.getString(cursor.getColumnIndex("LEASE_DUE_DT")));            //返却予定日
            data.setRemark(cursor.getString(cursor.getColumnIndex("REMARK")));                       //備考
            data.setLocLCd(cursor.getString(cursor.getColumnIndex("LOC_L_CD")));                     //ロケ大区分コード
            data.setLocMCd(cursor.getString(cursor.getColumnIndex("LOC_M_CD")));                     //ロケ中区分コード
            data.setLocSCd(cursor.getString(cursor.getColumnIndex("LOC_S_CD")));                     //ロケ小区分コード
            data.setEmployeeCd(cursor.getString(cursor.getColumnIndex("EMPLOYEE_CD")));             //ログイン担当者コード

            //溜め込む
            map.setprocessDataList(data);
        }

        return map;
    }

    /**
     * 処理履歴テーブル登録
     * @param db
     */
    public static void Insert(SQLiteDatabase db, EntryProcessDataDetail value){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" INSERT");
        sql.append(" INTO ");
        sql.append(CommonConst.TABLE_PROCESS_LOG);
        sql.append(" (");
        sql.append("  PROCESS_DT");                    //処理日
        sql.append("  ,PROCESS_TM");                    //処理時刻
        sql.append("  ,PROCESS_TOOL_DIV");             //処理ツール区分
        sql.append("  ,ASSET_NO");                      //資産番号
        sql.append("  ,ASSET_ID");                      //資産ID
        sql.append("  ,PROCESS_DIV");                   //処理区分
        sql.append("  ,LEASE_PLACE_CD");               //貸出先場所コード
        sql.append("  ,LEASE_EMPLOYEE_CD");           //貸出先場所コード
        sql.append("  ,LEASE_DUE_DT");                 //貸出返却予定日
        sql.append("  ,REMARK");                        //備考
        sql.append("  ,LOC_L_CD");                      //ロケ大区分コード
        sql.append("  ,LOC_M_CD");                      //ロケ中区分コード
        sql.append("  ,LOC_S_CD");                      //ロケ小区分コード
        sql.append("  ,EMPLOYEE_CD");                   //ログイン担当者コード
        sql.append("  ) ");
        sql.append(" VALUES (");
        //2021/11/08 UPD Start Suzuki
//        sql.append(" strftime('%Y%m%d', 'now'),");
        sql.append(" strftime('%Y%m%d', datetime('now', 'localtime')),");
        //2021/11/08 UPD End Suzuki
        sql.append(" replace(strftime('%H%M%f', datetime('now', 'localtime')), '.', ''),");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ? ");
        sql.append(" )");

        //SQL発行
        db.execSQL(sql.toString(), new String[]{
                CommonConst.WORD_TOOL_DIV.H.getString(),    //処理ツール区分
                value.getAssetNo(),                 //資産番号
                value.getAssetId(),                 //資産ID
                value.getProcessDiv(),              //処理区分
                value.getLeasePlaceCd(),            //貸出先場所コード
                value.getLeaseEmployeeCd(),         //貸出先担当者社員コード
                value.getLeaseDueDt(),              //貸出返却予定日
                value.getRemark(),                  //備考
                value.getLocLCd(),                  //ロケ大区分コード
                value.getLocMCd(),                  //ロケ中区分コード
                value.getLocSCd(),                  //ロケ小区分コード
                value.getEmployeeCd()               //ログイン担当者コード
        });

//        Cursor cursor = db.rawQuery("SELECT last_insert_rowid()", null);
//
//        if (cursor.moveToFirst()) {
//            //登録して作成されたProcessLogIdを保存する
//            value.setProcessLogId(Long.toString(cursor.getLong(0)));
//        }

    }

    /**
     * 照合処理履歴データを取得する
     * @param db
     */
    public EntryMatchProcessData MatSelectData(SQLiteDatabase db){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  PROCESS_LOG_ID");                 //処理履歴ID
        sql.append("  ,PROCESS_DT");                    //処理日
        sql.append("  ,PROCESS_TM");                    //処理時刻
        sql.append("  ,PROCESS_DIV");                   //処理ツール区分
        sql.append("  ,CONST_NO");                      //工事番号
        sql.append("  ,LEASE_EMPLOYEE_CD");             //貸出先担当者社員コード
        sql.append("  ,LEASE_DUE_DT");                  //貸出先返却予定日
        sql.append("  ,CONT_MATCH_DIV");                //照合結果
        sql.append("  ,CONT_MATCH_NUM");                //照合数
        sql.append("  ,EMPLOYEE_CD");                   //ログイン担当者コード
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_MATCH_LIST);

        //SQL発行
        Cursor cursor = db.rawQuery(sql.toString(), null);

        //取得したデータを処理履歴クラスに格納する
        EntryMatchProcessData map = new EntryMatchProcessData();
        while (cursor.moveToNext()) {
            EntryMatchProcessDataDetail data = new EntryMatchProcessDataDetail();

            //各項目を設定する
            data.setProcessLogId(cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID")));        //処理履歴ID
            data.setProcessDt(cursor.getString(cursor.getColumnIndex("PROCESS_DT")));               //処理日
            data.setProcessTm(cursor.getString(cursor.getColumnIndex("PROCESS_TM")));               //処理時刻
            data.setEmployeeCd(cursor.getString(cursor.getColumnIndex("EMPLOYEE_CD")));             //ログイン担当者コード
            data.setProcessDiv(cursor.getString(cursor.getColumnIndex("PROCESS_DIV")));             //処理区分
            data.setConstNo(cursor.getString(cursor.getColumnIndex("CONST_NO")));                   //工事番号
            data.setLeaseEmployeeCd(cursor.getString(cursor.getColumnIndex("LEASE_EMPLOYEE_CD")));  //貸出先担当者社員コード
            data.setLeaseDueDt(cursor.getString(cursor.getColumnIndex("LEASE_DUE_DT")));            //貸出先返却予定日
            data.SetContMatchDiv(cursor.getString(cursor.getColumnIndex("CONT_MATCH_DIV")));        //照合結果
            data.SetContMatchNum(cursor.getString(cursor.getColumnIndex("CONT_MATCH_NUM")));        //照合数

            //貸出リスト追加
            data.setLentAssetList(LentAssetListFactory.SelectData(db,cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID"))));
            //返却リスト追加
            data.setRetAssetList(RetAseetListFactory.SelectData(db,cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID"))));
            //照合資産リスト追加
            data.setMatchAssetList(MatchAseetListFactory.SelectData(db,cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID"))));
            //仮置きリスト追加
            data.setTempList(TempListFactory.SelectData(db,cursor.getString(cursor.getColumnIndex("PROCESS_LOG_ID"))));
            //溜め込む
            map.setprocessDataList(data);
        }

        return map;
    }

    /**
     * 処理履歴テーブル登録
     * @param db
     */
    public static void MatInsert(SQLiteDatabase db, EntryProcessDataDetail value,String ConstRuctionNo,String MatchDiv,String CntOkData,String LentEndDt){

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" INSERT");
        sql.append(" INTO ");
        sql.append(CommonConst.TABLE_MATCH_LIST);
        sql.append(" (");
        sql.append("  PROCESS_DT");                    //処理日
        sql.append("  ,PROCESS_TM");                   //処理時刻
        sql.append("  ,PROCESS_DIV");                  //処理区分
        sql.append("  ,CONST_NO");                     //工事番号
        sql.append("  ,LEASE_EMPLOYEE_CD");            //貸出先担当者社員コード
        sql.append("  ,LEASE_DUE_DT");                 //貸出先返却予定日
        sql.append("  ,CONT_MATCH_DIV");               //照合結果
        sql.append("  ,CONT_MATCH_NUM");               //照合数
        sql.append("  ,EMPLOYEE_CD");                  //ログイン担当者コード
        sql.append("  ) ");
        sql.append(" VALUES (");
        //2021/11/08 UPD Start Suzuki
//        sql.append(" strftime('%Y%m%d', 'now'),");
        sql.append(" strftime('%Y%m%d', datetime('now', 'localtime')),");
        //2021/11/08 UPD End Suzuki
        sql.append(" replace(strftime('%H%M%f', datetime('now', 'localtime')), '.', ''),");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ?,");
        sql.append(" ? ");
        sql.append(" )");

        //SQL発行
        db.execSQL(sql.toString(), new String[]{
                value.getProcessDiv(),              //処理区分
                ConstRuctionNo,                     //工事番号
                value.getEmployeeCd(),              //貸出先担当者社員コード
                LentEndDt,                          //貸出先返却予定日
                MatchDiv,                           //照合結果
                CntOkData,                          //照合数
                value.getEmployeeCd()               //ログイン担当者コード
        });

        Cursor cursor = db.rawQuery("SELECT last_insert_rowid()", null);

        if (cursor.moveToFirst()) {
            //登録して作成されたProcessLogIdを保存する
            _ProsessId = Long.toString(cursor.getLong(0));
        }

    }

    /**
     * 資産データを読み込みます。
     * @param db データベース接続
     * @param dueDt 返却予定日
     * @param pLeaseAlertDisp 貸出時のアラート表示有無  20230907 ADD okubo v2.4.1
     * @param pReturnAlertDisp 返却時のアラート表示有無 20230907 ADD okubo v2.4.1
     */
    public LinkedHashMap<String, EntryProcessDataDetail> SelectAssetData(Activity _act, SQLiteDatabase db, String dueDt, boolean pLeaseAlertDisp, boolean pReturnAlertDisp) {

        try {
            StringBuilder sql = new StringBuilder();
            sql.append(" SELECT ");
            sql.append("  ASSET_NO");
            sql.append("  ,ASSET_ID");
            sql.append("  ,ASSET_NAME");
            sql.append("  ,ASSET_STATUS");
            sql.append("  ,LEASE_CNT");                 //20200225 Add Sasaki V1.2
            sql.append("  ,LEASE_UPPER_ERR_DIV");     //20200225 Add Sasaki V1.2
            sql.append("  ,LEASE_UPPER_LIMIT");       //20200225 Add Sasaki V1.2
            if(pLeaseAlertDisp || pReturnAlertDisp) {
                sql.append("  ,LEASE_LIMIT");           //20230830 ADD Kawamoto v2.4.1
                sql.append("  ,GENERIC_LIMIT");         //20230830 ADD Kawamoto v2.4.1
                sql.append("  ,LEASE_DUE_DT");          //20230912 ADD okubo v2.4.1
            }
            if(pReturnAlertDisp) {
                sql.append("  ,LEASE_UPPER_ALERT_LIMIT");  //20230911 ADD okubo  V2.4.1
            }
            sql.append(" FROM (");
            sql.append("  SELECT");
            sql.append("   TEMP_PROCESS_LOG.ASSET_NO,");
            sql.append("   M_ASSET.ASSET_ID,");
            sql.append("   M_ASSET.ASSET_NAME,");
            sql.append("   M_ASSET.ASSET_STATUS");
            sql.append("  ,LEASE_CNT");                 //20200225 Add Sasaki V1.2
            sql.append("  ,LEASE_UPPER_ERR_DIV");     //20200225 Add Sasaki V1.2
            sql.append("  ,LEASE_UPPER_LIMIT");       //20200225 Add Sasaki V1.2
            if(pLeaseAlertDisp || pReturnAlertDisp) {
                sql.append("  ,M_ASSET.LEASE_LIMIT");           //20230830 ADD Kawamoto v2.4.1
                sql.append("  ,M_ASSET.GENERIC_LIMIT");         //20230830 ADD Kawamoto v2.4.1
                sql.append("  ,M_ASSET.LEASE_DUE_DT");          //20230912 ADD okubo v2.4.1
            }
            if(pReturnAlertDisp) {
                sql.append("  ,LEASE_UPPER_ALERT_LIMIT");  //20230911 ADD okubo  V2.4.1
            }
            sql.append("  FROM ");
            sql.append(CommonConst.TEMP_PROCESS_LOG);
            sql.append("  LEFT JOIN ");
            sql.append(CommonConst.MASTER_ASSET);
            sql.append("  ON ");
            sql.append(CommonConst.MASTER_ASSET + ".ASSET_NO ");
            sql.append("  = ");
            sql.append(CommonConst.TEMP_PROCESS_LOG + ".ASSET_NO ");
            sql.append("  UNION ALL ");
            sql.append("  SELECT");
            sql.append("   M_ASSET.ASSET_NO,");
            sql.append("   TEMP_PROCESS_LOG.ASSET_ID,");
            sql.append("   M_ASSET.ASSET_NAME,");
            sql.append("   M_ASSET.ASSET_STATUS");
            sql.append("  ,LEASE_CNT");                 //20200225 Add Sasaki V1.2
            sql.append("  ,LEASE_UPPER_ERR_DIV");     //20200225 Add Sasaki V1.2
            sql.append("  ,LEASE_UPPER_LIMIT");       //20200225 Add Sasaki V1.2
            if(pLeaseAlertDisp || pReturnAlertDisp) {
                sql.append("  ,M_ASSET.LEASE_LIMIT");           //20230830 ADD Kawamoto v2.4.1
                sql.append("  ,M_ASSET.GENERIC_LIMIT");         //20230830 ADD Kawamoto v2.4.1
                sql.append("  ,M_ASSET.LEASE_DUE_DT");          //20230912 ADD okubo v2.4.1
            }
            if(pReturnAlertDisp) {
                sql.append("  ,LEASE_UPPER_ALERT_LIMIT");  //20230911 ADD okubo  V2.4.1
            }
            sql.append("  FROM ");
            sql.append(CommonConst.TEMP_PROCESS_LOG);
            sql.append("  LEFT JOIN ");
            sql.append(CommonConst.MASTER_ASSET);
            sql.append("  ON ");
            sql.append(CommonConst.MASTER_ASSET + ".ASSET_ID ");
            sql.append("  = ");
            sql.append(CommonConst.TEMP_PROCESS_LOG + ".ASSET_ID ");
            sql.append(" ) ");
            sql.append(" ORDER BY");
            //20191122 Upd Start Sasaki
            sql.append("  CASE WHEN ASSET_NO IS NULL OR ASSET_STATUS IS NULL THEN 1 ELSE 4 END DESC");  //マスタが存在しない場合は、一番下へ
            sql.append("  ,CASE WHEN ASSET_STATUS IN (0, 9) THEN 2 ELSE 3 END DESC");                       //準備中、除却
            sql.append("  ,ASSET_NO ASC");
            sql.append("  ,ASSET_STATUS DESC");
            //20191122 Upd End Sasaki

            //SQL発行
            Cursor cursorAll = db.rawQuery(sql.toString(), null);

            _mapAllData.clear();            //20191120 Add Sasaki
            _listLeaseLimitErr.clear();         //20200226 Add Sasaki V1.2
            _listLeaseLimitWarning.clear();    //20200226 Add Sasaki V1.2
            _listNoAuth.clear();       //20210918 ADD Takase V2.0
            _listLeaseUntilErr.clear();       //20230830 ADD Kawamoto v2.4.1
            _listGenericUntilErr.clear();       //20230830 ADD Kawamoto v2.4.1
            _listReturnUntilErr.clear();        //20230908 ADD okubo v2.4.1

            while (cursorAll.moveToNext()) {
                EntryProcessDataDetail data  = new EntryProcessDataDetail();

                //各項目を設定する
                data.setAssetNo(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO")));
                data.setAssetId(cursorAll.getString(cursorAll.getColumnIndex("ASSET_ID")));
                data.setAssetName(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NAME")));
                data.setAssetStatus(cursorAll.getString(cursorAll.getColumnIndex("ASSET_STATUS")));
                data.setLeaseCount(cursorAll.getInt(cursorAll.getColumnIndex("LEASE_CNT")));                        //20200225 Add Sasaki V1.2
                data.setLeaseUpperErrDiv(cursorAll.getString(cursorAll.getColumnIndex("LEASE_UPPER_ERR_DIV")));   //20200225 Add Sasaki V1.2
                data.setLeaseUpperLimit(cursorAll.getString(cursorAll.getColumnIndex("LEASE_UPPER_LIMIT")));      //20200225 Add Sasaki V1.2
                if(pLeaseAlertDisp || pReturnAlertDisp) {
                    data.setLeaseLimit(cursorAll.getString(cursorAll.getColumnIndex("LEASE_LIMIT")));                   //20230830 ADD Kawamoto v2.4.1
                    data.setGenericLimit(cursorAll.getString(cursorAll.getColumnIndex("GENERIC_LIMIT")));               //20230830 ADD Kawamoto v2.4.1
                    data.setLeaseDueDt(cursorAll.getString(cursorAll.getColumnIndex("LEASE_DUE_DT")));                  //20230912 ADD okubo v2.4.1
                }
                if(pReturnAlertDisp) {
                    data.SetLeaseUpperAlert(cursorAll.getString(cursorAll.getColumnIndex("LEASE_UPPER_ALERT_LIMIT")));  //20230911 ADD okubo  V2.4.1
                }

                //データに溜め込む
                if (data.getAssetNo() == null && data.getAssetId() != null) {
                    _mapAllData.put(data.getAssetId(), data);
                }
                else if (data.getAssetNo() != null) {
                    _mapAllData.put(data.getAssetNo(), data);
                    if (data.getAssetStatus() != null) {
                        //状態0か9以外の場合登録する
                        if (!(CommonConst.ASSET_STATUS.prepare.getString().equals(data.getAssetStatus()) ||
                                CommonConst.ASSET_STATUS.Dispose.getString().equals(data.getAssetStatus()))) {
                            //20210918 ADD Takase V2.0 START
                            //更新権限を持っている資産のみ登録する
                            AssetModel asset = AssetFactory.Select(db, data.getAssetNo());
                            if (!CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {
                                _listNoAuth.add(data.getAssetNo());
                            }
                            //20210918 ADD Takase V2.0 END
                            else {
                                if (isLease()) {
                                    //貸出の場合
                                    if (!CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus())) {
                                        _listNoAuth.add(data.getAssetNo());
                                        continue;
                                    }

                                    Date _date = new Date();
                                    SimpleDateFormat DtFormat = new SimpleDateFormat("yyyyMMdd");

                                    //20230830 ADD Kawamoto v2.4.1 Start
                                    //貸出時のアラート表示有
                                    if (pLeaseAlertDisp) {      //エラーとは別にチェックする  20230914 ADD okubo v2.4.1

                                        //期限日判定日付の判定
                                        //期限日=システム日付
                                        String CheckDate = DtFormat.format(_date);

                                        //期限日判定項目が「返却予定日」の場合
                                        //期限日=返却予定日
                                        String[] temp = SettingFactory.GetLeaseAlertDisplay(db);
                                        if (temp[1].equals(CommonConst.LEASE_DEADLONE.RETURN_DATE.getString())) {
                                            CheckDate = _LeaseReturn;
                                        }

                                        //リース期限の判定
                                        String leaseLimit = data.getLeaseLimit();
                                        if (leaseLimit.length() > 0) {
                                            if (leaseLimit.compareTo(CheckDate) <= 0) {
                                                _listLeaseUntilErr.add(data.getAssetNo());
                                                //リース期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LEASE_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.LEASE_LIMIT.getSortRank());
                                            }
                                        }

                                        //○○期限の判定
                                        String genericLimit = data.getGenericLimit();
                                        if (genericLimit.length() > 0) {
                                            if (genericLimit.compareTo(CheckDate) <= 0) {
                                                _listGenericUntilErr.add(data.getAssetNo());
                                                //○○期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_GENERIC_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.GENERIC_LIMIT.getSortRank());
                                            }
                                        }
                                    }
                                    //20230830 ADD Kawamoto v2.4.1 End

                                    if (CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString().equals(GetLeaseUpperErrDiv(data.getAssetNo())) &&
                                            CheckLeaseNum(data.getAssetNo())) {
                                        //エラーで貸出上限を超えている場合
                                        _listLeaseLimitErr.add(data.getAssetNo());

                                        if (pLeaseAlertDisp) {
                                            //貸出時のアラート表示有
                                            setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LIMIT_COUNT, "", _act), CommonConst.WARNING_DISPLAY.LEASE_UPPER_LIMIT_ERR.getSortRank());
                                        }
                                    }
                                    else if (CommonConst.LEASE_UPPER_ERR_DIV.ALERT.getString().equals(GetLeaseUpperErrDiv(data.getAssetNo())) &&
                                            CheckLeaseNum(data.getAssetNo())) {
                                        //警告で貸出上限を超えている場合
                                        _listLeaseLimitWarning.add(data.getAssetNo());
                                        _mapOK.put(data.getAssetNo(), data);

                                        if (pLeaseAlertDisp) {
                                            //貸出時のアラート表示有
                                            setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LIMIT_WARNING, "", _act), CommonConst.WARNING_DISPLAY.LEASE_UPPER_LIMIT_WARN.getSortRank());
                                        }
                                    }
                                    else if(isMatLease() && CommonConst.ASSET_STATUS.Lease.getString().equals(data.getAssetStatus())){
                                        //貸出照合かつ貸出ステータスが貸出中の場合
                                    }
                                    else {
                                        //上記以外の場合、登録する
                                        _mapOK.put(data.getAssetNo(), data);
                                    }

                                    //20231024 ADD Sato v2.4.1 Start
                                    //貸出時のアラート表示有
                                    if (pLeaseAlertDisp) {
                                        //システム日付
                                        String sysDate = DtFormat.format(_date);
                                        String DueDate = data.getLeaseDueDt();

                                        if(DueDate.length() > 0) {
                                            if (DueDate.compareTo(sysDate) < 0) {
                                                _listReturnUntilErr.add(data.getAssetNo());
                                                //返却期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_DUE_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.RETURN_LIMIT.getSortRank());
                                            }
                                        }
                                    }
                                    //20231024 ADD Sato v2.4.1 End
                                }
                                else if(isReturn()) {
                                    //返却の場合　20230907 ADD okubo v2.4.1 Start

                                    //正常データ判別フラグ
                                    boolean isNormal = true;

                                    //貸出可能な資産か判定
                                    if (!CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus())) {
                                        //貸出不可
                                        _listNoAuth.add(data.getAssetNo());
                                        continue;
                                    }

                                    if(pReturnAlertDisp) {
                                        //返却時のアラート表示有

                                        //システム日付
                                        Date _date = new Date();
                                        SimpleDateFormat DtFormat = new SimpleDateFormat("yyyyMMdd");
                                        String CheckDate = DtFormat.format(_date);

                                        // 資産M.リース期限≦システム日付　？
                                        String leaseLimit = data.getLeaseLimit();
                                        if (leaseLimit.length() > 0) {
                                            if (leaseLimit.compareTo(CheckDate) <= 0) {
                                                _listLeaseUntilErr.add(data.getAssetNo());
                                                //リース期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LEASE_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.LEASE_LIMIT.getSortRank());
                                                isNormal = false;
                                            }
                                        }

                                        // 資産M.○○期限≦システム日付　？
                                        String genericLimit = data.getGenericLimit();
                                        if (genericLimit.length() > 0) {
                                            if (genericLimit.compareTo(CheckDate) <= 0) {
                                                _listGenericUntilErr.add(data.getAssetNo());
                                                //○○期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_GENERIC_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.GENERIC_LIMIT.getSortRank());
                                                isNormal = false;
                                            }
                                        }

                                        int leaseCnt = data.getLeaseCount();
                                        String strUpperLimit = data.getLeaseUpperLimit();
                                        if(strUpperLimit.length() > 0) {
                                            int intUpperLimit = Integer.valueOf(strUpperLimit);

                                            if(data.getLeaseUpperErrDiv().equals(CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString())) {
                                                // カテゴリM.貸出上限回数≦資産M.貸出回数　？
                                                if(intUpperLimit <= leaseCnt) {
                                                    _listLeaseLimitErr.add(data.getAssetNo());
                                                    //貸出上限エラー
                                                    setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LIMIT_COUNT, "", _act), CommonConst.WARNING_DISPLAY.LEASE_UPPER_LIMIT_ERR.getSortRank());
                                                    isNormal = false;
                                                }
                                            } else if(data.getLeaseUpperErrDiv().equals(CommonConst.LEASE_UPPER_ERR_DIV.ALERT.getString())) {
                                                // カテゴリM.貸出アラート回数≦資産M.貸出回数　？
                                                if(intUpperLimit <= leaseCnt) {
                                                    _listLeaseLimitWarning.add(data.getAssetNo());
                                                    //貸出上限警告
                                                    setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LIMIT_WARNING, "", _act), CommonConst.WARNING_DISPLAY.LEASE_UPPER_LIMIT_WARN.getSortRank());
                                                }
                                            }
                                        }

                                        // 資産M.返却予定日＜システム日付　？
                                        String DueDate = data.getLeaseDueDt();
                                        if(DueDate.length() > 0) {
                                            if (DueDate.compareTo(CheckDate) < 0) {
                                                _listReturnUntilErr.add(data.getAssetNo());
                                                //返却期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_DUE_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.RETURN_LIMIT.getSortRank());
                                            }
                                        }
                                    }

                                    if (isNormal) {
                                        //チェックエラーの資産はOKリストに含めない
                                        _mapOK.put(data.getAssetNo(), data);
                                    }
                                    //20230907 ADD okubo v2.4.1 End
                                }
                                else {
                                    //貸出・返却以外の場合
                                    _mapOK.put(data.getAssetNo(), data);
                                }
                            }
                        }
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
        }
        finally {
            if (db != null) {
                db.close();
            }
        }

        return _mapAllData;
    }

    /**
     * 資産データを読み込みます。ソートをしない。ほかの所でする(照合用に追加)
     * @param db データベース接続
     * @param dueDt 返却予定日
     * @param pLeaseAlertDisp 貸出時のアラート表示有無  20230907 ADD okubo v2.4.1
     * @param pReturnAlertDisp 返却時のアラート表示有無 20230907 ADD okubo v2.4.1
     */
    public LinkedHashMap<String, EntryProcessDataDetail> SelectMatAssetData(Activity _act, SQLiteDatabase db, String dueDt, boolean pLeaseAlertDisp, boolean pReturnAlertDisp) {

        try {
            StringBuilder sql = new StringBuilder();
            sql.append(" SELECT ");
            sql.append("  ASSET_NO");
            sql.append("  ,ASSET_ID");
            sql.append("  ,ASSET_NAME");
            sql.append("  ,ASSET_STATUS");
            sql.append("  ,LEASE_CNT");
            sql.append("  ,LEASE_UPPER_ERR_DIV");
            sql.append("  ,LEASE_UPPER_LIMIT");
            if(pLeaseAlertDisp || pReturnAlertDisp) {
                sql.append("  ,LEASE_LIMIT");
                sql.append("  ,GENERIC_LIMIT");
                sql.append("  ,LEASE_DUE_DT");
            }
            if(pReturnAlertDisp) {
                sql.append("  ,LEASE_UPPER_ALERT_LIMIT");
            }
            sql.append(" FROM (");
            sql.append("  SELECT");
            sql.append("   TEMP_PROCESS_LOG.ASSET_NO,");
            sql.append("   M_ASSET.ASSET_ID,");
            sql.append("   M_ASSET.ASSET_NAME,");
            sql.append("   M_ASSET.ASSET_STATUS");
            sql.append("  ,LEASE_CNT");
            sql.append("  ,LEASE_UPPER_ERR_DIV");
            sql.append("  ,LEASE_UPPER_LIMIT");
            if(pLeaseAlertDisp || pReturnAlertDisp) {
                sql.append("  ,M_ASSET.LEASE_LIMIT");
                sql.append("  ,M_ASSET.GENERIC_LIMIT");
                sql.append("  ,M_ASSET.LEASE_DUE_DT");
            }
            if(pReturnAlertDisp) {
                sql.append("  ,LEASE_UPPER_ALERT_LIMIT");
            }
            sql.append("  FROM ");
            sql.append(CommonConst.TEMP_PROCESS_LOG);
            sql.append("  LEFT JOIN ");
            sql.append(CommonConst.MASTER_ASSET);
            sql.append("  ON ");
            sql.append(CommonConst.MASTER_ASSET + ".ASSET_NO ");
            sql.append("  = ");
            sql.append(CommonConst.TEMP_PROCESS_LOG + ".ASSET_NO ");
            sql.append("  UNION ALL ");
            sql.append("  SELECT");
            sql.append("   M_ASSET.ASSET_NO,");
            sql.append("   TEMP_PROCESS_LOG.ASSET_ID,");
            sql.append("   M_ASSET.ASSET_NAME,");
            sql.append("   M_ASSET.ASSET_STATUS");
            sql.append("  ,LEASE_CNT");
            sql.append("  ,LEASE_UPPER_ERR_DIV");
            sql.append("  ,LEASE_UPPER_LIMIT");
            if(pLeaseAlertDisp || pReturnAlertDisp) {
                sql.append("  ,M_ASSET.LEASE_LIMIT");
                sql.append("  ,M_ASSET.GENERIC_LIMIT");
                sql.append("  ,M_ASSET.LEASE_DUE_DT");
            }
            if(pReturnAlertDisp) {
                sql.append("  ,LEASE_UPPER_ALERT_LIMIT");  
            }
            sql.append("  FROM ");
            sql.append(CommonConst.TEMP_PROCESS_LOG);
            sql.append("  LEFT JOIN ");
            sql.append(CommonConst.MASTER_ASSET);
            sql.append("  ON ");
            sql.append(CommonConst.MASTER_ASSET + ".ASSET_ID ");
            sql.append("  = ");
            sql.append(CommonConst.TEMP_PROCESS_LOG + ".ASSET_ID ");
            sql.append(" ) ");

            //SQL発行
            Cursor cursorAll = db.rawQuery(sql.toString(), null);

            _mapAllData.clear();            //20191120 Add Sasaki
            _listLeaseLimitErr.clear();         //20200226 Add Sasaki V1.2
            _listLeaseLimitWarning.clear();    //20200226 Add Sasaki V1.2
            _listNoAuth.clear();       //20210918 ADD Takase V2.0
            _listLeaseUntilErr.clear();       //20230830 ADD Kawamoto v2.4.1
            _listGenericUntilErr.clear();       //20230830 ADD Kawamoto v2.4.1
            _listReturnUntilErr.clear();        //20230908 ADD okubo v2.4.1

            while (cursorAll.moveToNext()) {
                EntryProcessDataDetail data  = new EntryProcessDataDetail();

                //各項目を設定する
                data.setAssetNo(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO")));
                data.setAssetId(cursorAll.getString(cursorAll.getColumnIndex("ASSET_ID")));
                data.setAssetName(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NAME")));
                data.setAssetStatus(cursorAll.getString(cursorAll.getColumnIndex("ASSET_STATUS")));
                data.setLeaseCount(cursorAll.getInt(cursorAll.getColumnIndex("LEASE_CNT")));                        //20200225 Add Sasaki V1.2
                data.setLeaseUpperErrDiv(cursorAll.getString(cursorAll.getColumnIndex("LEASE_UPPER_ERR_DIV")));   //20200225 Add Sasaki V1.2
                data.setLeaseUpperLimit(cursorAll.getString(cursorAll.getColumnIndex("LEASE_UPPER_LIMIT")));      //20200225 Add Sasaki V1.2
                if(pLeaseAlertDisp || pReturnAlertDisp) {
                    data.setLeaseLimit(cursorAll.getString(cursorAll.getColumnIndex("LEASE_LIMIT")));                   //20230830 ADD Kawamoto v2.4.1
                    data.setGenericLimit(cursorAll.getString(cursorAll.getColumnIndex("GENERIC_LIMIT")));               //20230830 ADD Kawamoto v2.4.1
                    data.setLeaseDueDt(cursorAll.getString(cursorAll.getColumnIndex("LEASE_DUE_DT")));                  //20230912 ADD okubo v2.4.1
                }
                if(pReturnAlertDisp) {
                    data.SetLeaseUpperAlert(cursorAll.getString(cursorAll.getColumnIndex("LEASE_UPPER_ALERT_LIMIT")));  //20230911 ADD okubo  V2.4.1
                }

                //データに溜め込む
                if (data.getAssetNo() == null && data.getAssetId() != null) {
                    _mapAllData.put(data.getAssetId(), data);
                }
                else if (data.getAssetNo() != null) {
                    _mapAllData.put(data.getAssetNo(), data);
                    if (data.getAssetStatus() != null) {
                        //状態0か9以外の場合登録する
                        if (!(CommonConst.ASSET_STATUS.prepare.getString().equals(data.getAssetStatus()) ||
                                CommonConst.ASSET_STATUS.Dispose.getString().equals(data.getAssetStatus()))) {
                            //20210918 ADD Takase V2.0 START
                            //更新権限を持っている資産のみ登録する
                            AssetModel asset = AssetFactory.Select(db, data.getAssetNo());
                            if (!CommonAuthority.CheckAssetAuth(db, asset, CommonAuthority.Auth.updHubList)) {
                                _listNoAuth.add(data.getAssetNo());
                            }
                            //20210918 ADD Takase V2.0 END
                            else {
                                if (isLease()) {
                                    //貸出の場合
                                    if (!CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus())) {
                                        _listNoAuth.add(data.getAssetNo());
                                        continue;
                                    }

                                    Date _date = new Date();
                                    SimpleDateFormat DtFormat = new SimpleDateFormat("yyyyMMdd");

                                    //20230830 ADD Kawamoto v2.4.1 Start
                                    //貸出時のアラート表示有
                                    if (pLeaseAlertDisp) {      //エラーとは別にチェックする  20230914 ADD okubo v2.4.1

                                        //期限日判定日付の判定
                                        //期限日=システム日付
                                        String CheckDate = DtFormat.format(_date);

                                        //期限日判定項目が「返却予定日」の場合
                                        //期限日=返却予定日
                                        String[] temp = SettingFactory.GetLeaseAlertDisplay(db);
                                        if (temp[1].equals(CommonConst.LEASE_DEADLONE.RETURN_DATE.getString())) {
                                            CheckDate = _LeaseReturn;
                                        }

                                        //リース期限の判定
                                        String leaseLimit = data.getLeaseLimit();
                                        if (leaseLimit.length() > 0) {
                                            if (leaseLimit.compareTo(CheckDate) <= 0) {
                                                _listLeaseUntilErr.add(data.getAssetNo());
                                                //リース期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LEASE_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.LEASE_LIMIT.getSortRank());
                                            }
                                        }

                                        //○○期限の判定
                                        String genericLimit = data.getGenericLimit();
                                        if (genericLimit.length() > 0) {
                                            if (genericLimit.compareTo(CheckDate) <= 0) {
                                                _listGenericUntilErr.add(data.getAssetNo());
                                                //○○期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_GENERIC_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.GENERIC_LIMIT.getSortRank());
                                            }
                                        }
                                    }
                                    //20230830 ADD Kawamoto v2.4.1 End

                                    if (CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString().equals(GetLeaseUpperErrDiv(data.getAssetNo())) &&
                                            CheckLeaseNum(data.getAssetNo())) {
                                        //エラーで貸出上限を超えている場合
                                        _listLeaseLimitErr.add(data.getAssetNo());

                                        if (pLeaseAlertDisp) {
                                            //貸出時のアラート表示有
                                            setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LIMIT_COUNT, "", _act), CommonConst.WARNING_DISPLAY.LEASE_UPPER_LIMIT_ERR.getSortRank());
                                        }
                                    }
                                    else if (CommonConst.LEASE_UPPER_ERR_DIV.ALERT.getString().equals(GetLeaseUpperErrDiv(data.getAssetNo())) &&
                                            CheckLeaseNum(data.getAssetNo())) {
                                        //警告で貸出上限を超えている場合
                                        _listLeaseLimitWarning.add(data.getAssetNo());
                                        _mapOK.put(data.getAssetNo(), data);

                                        if (pLeaseAlertDisp) {
                                            //貸出時のアラート表示有
                                            setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LIMIT_WARNING, "", _act), CommonConst.WARNING_DISPLAY.LEASE_UPPER_LIMIT_WARN.getSortRank());
                                        }
                                    }
                                    else {
                                        //上記以外の場合、登録する
                                        _mapOK.put(data.getAssetNo(), data);
                                    }

                                    //20231024 ADD Sato v2.4.1 Start
                                    //貸出時のアラート表示有
                                    if (pLeaseAlertDisp) {
                                        //システム日付
                                        String sysDate = DtFormat.format(_date);
                                        String DueDate = data.getLeaseDueDt();

                                        if(DueDate.length() > 0) {
                                            if (DueDate.compareTo(sysDate) < 0) {
                                                _listReturnUntilErr.add(data.getAssetNo());
                                                //返却期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_DUE_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.RETURN_LIMIT.getSortRank());
                                            }
                                        }
                                    }
                                    //20231024 ADD Sato v2.4.1 End
                                }
                                else if(isReturn()) {
                                    //返却の場合　20230907 ADD okubo v2.4.1 Start

                                    //正常データ判別フラグ
                                    boolean isNormal = true;

                                    //貸出可能な資産か判定
                                    if (!CommonConst.ASSET_LEASE_STATUS.Possible.getString().equals(asset.getLeaseStatus())) {
                                        //貸出不可
                                        _listNoAuth.add(data.getAssetNo());
                                        continue;
                                    }

                                    if(pReturnAlertDisp) {
                                        //返却時のアラート表示有

                                        //システム日付
                                        Date _date = new Date();
                                        SimpleDateFormat DtFormat = new SimpleDateFormat("yyyyMMdd");
                                        String CheckDate = DtFormat.format(_date);

                                        // 資産M.リース期限≦システム日付　？
                                        String leaseLimit = data.getLeaseLimit();
                                        if (leaseLimit.length() > 0) {
                                            if (leaseLimit.compareTo(CheckDate) <= 0) {
                                                _listLeaseUntilErr.add(data.getAssetNo());
                                                //リース期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LEASE_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.LEASE_LIMIT.getSortRank());
                                                isNormal = false;
                                            }
                                        }

                                        // 資産M.○○期限≦システム日付　？
                                        String genericLimit = data.getGenericLimit();
                                        if (genericLimit.length() > 0) {
                                            if (genericLimit.compareTo(CheckDate) <= 0) {
                                                _listGenericUntilErr.add(data.getAssetNo());
                                                //○○期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_GENERIC_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.GENERIC_LIMIT.getSortRank());
                                                isNormal = false;
                                            }
                                        }

                                        int leaseCnt = data.getLeaseCount();
                                        String strUpperLimit = data.getLeaseUpperLimit();
                                        if(strUpperLimit.length() > 0) {
                                            int intUpperLimit = Integer.valueOf(strUpperLimit);

                                            if(data.getLeaseUpperErrDiv().equals(CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString())) {
                                                // カテゴリM.貸出上限回数≦資産M.貸出回数　？
                                                if(intUpperLimit <= leaseCnt) {
                                                    _listLeaseLimitErr.add(data.getAssetNo());
                                                    //貸出上限エラー
                                                    setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LIMIT_COUNT, "", _act), CommonConst.WARNING_DISPLAY.LEASE_UPPER_LIMIT_ERR.getSortRank());
                                                    isNormal = false;
                                                }
                                            } else if(data.getLeaseUpperErrDiv().equals(CommonConst.LEASE_UPPER_ERR_DIV.ALERT.getString())) {
                                                // カテゴリM.貸出アラート回数≦資産M.貸出回数　？
                                                if(intUpperLimit <= leaseCnt) {
                                                    _listLeaseLimitWarning.add(data.getAssetNo());
                                                    //貸出上限警告
                                                    setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_LIMIT_WARNING, "", _act), CommonConst.WARNING_DISPLAY.LEASE_UPPER_LIMIT_WARN.getSortRank());
                                                }
                                            }
                                        }

                                        // 資産M.返却予定日＜システム日付　？
                                        String DueDate = data.getLeaseDueDt();
                                        if(DueDate.length() > 0) {
                                            if (DueDate.compareTo(CheckDate) < 0) {
                                                _listReturnUntilErr.add(data.getAssetNo());
                                                //返却期限
                                                setWarningDisp(data.getAssetNo(), SystemSetting.getString(CommonConst.VIEW_WORD_RETURN_MESS_DUE_LIMIT, "", _act), CommonConst.WARNING_DISPLAY.RETURN_LIMIT.getSortRank());
                                            }
                                        }
                                    }

                                    if (isNormal) {
                                        //チェックエラーの資産はOKリストに含めない
                                        _mapOK.put(data.getAssetNo(), data);
                                    }
                                    //20230907 ADD okubo v2.4.1 End
                                }
                                //仮置きの場合
                                else if(isTemporary()){
//                                    //照合対象の場合
//                                    if(ExistCollationAssetNo(data.getAssetNo())){
//                                        //ほか仮置きでない場合
//                                        if(!ExistTempListAssetNo(data.getAssetNo())){
//                                            //登録データに追加する
//                                            _mapOK.put(data.getAssetNo(), data);
//                                        }
//                                    }else{
//                                        //リスト不一致として登録
//                                        //登録データに追加する
//                                        _mapOK.put(data.getAssetNo(), data);
//                                    }
                                    //ほか仮置きでない場合
                                    if(!ExistTempListAssetNo(data.getAssetNo())){
                                        //登録データに追加する
                                        _mapOK.put(data.getAssetNo(), data);
                                    }
                                }
                                else {
                                    //貸出・返却以外の場合
                                    _mapOK.put(data.getAssetNo(), data);
                                }
                            }
                        }
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
        }
        finally {
            if (db != null) {
                db.close();
            }
        }

        return _mapAllData;
    }

    /**
     * 処理履歴データテーブル登録(貸出)
     * @param db
     * @param activity
     */
    public void InsertProcesLease(SQLiteDatabase db, Activity activity){

        //読込んだ貸出データを登録する
        for (Iterator<EntryProcessDataDetail> iterator = _mapOK.values().iterator(); iterator.hasNext(); ) {
            //1件取得する
            EntryProcessDataDetail data = iterator.next();

            //20210918 ADD Takase V2.0 START
            //貸出権限が無い資産の場合、登録しない
            if (!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, data.getAssetNo()), CommonAuthority.Auth.updHubList)) {
                continue;
            }
            //20210918 ADD Takase V2.0 END

            //20200225 Add Start Sasaki V1.2
            //貸出チェック対象で貸出回数が上限を超えた場合、登録しない
            if (CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString().equals(GetLeaseUpperErrDiv(data.getAssetNo())) &&
                CheckLeaseNum(data.getAssetNo())) {
                continue;
            }
            //20200225 Add End Sasaki V1.2

            if(data.getAssetNo() != null && !"".equals(data.getAssetNo())) {
                data.setProcessDiv(CommonConst.PROCESS_DIV.Lease.getString());
                data.setLeasePlaceCd(_LeaseDestination);           //貸出先コード
                data.setLeaseEmployeeCd(_LeaseEmployee);           //貸出担当コード
                data.setLeaseDueDt(_LeaseReturn);                   //返却予定日
                data.setRemark(_Remark);                             //備考
                data.setEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));            //ログイン担当者コード

                ProcessFactory.Insert(db, data);
            }
        }
    }

    /**
     * 処理履歴データテーブル登録(返却)
     * @param db
     * @param activity
     */
    public void InsertProcesReturn(SQLiteDatabase db, Activity activity){

        // 20220725 ADD SATO START v2_2
        SettingValue C_Setting = new SettingValue();
        String locCode = C_Setting.getTargetHub(activity);     //設定済み対象拠点を取得
        // 20220725 ADD SATO END v2_2

        //読込んだ返却データを登録する
        for (Iterator<EntryProcessDataDetail> iterator = _mapOK.values().iterator(); iterator.hasNext(); ) {

            //1件取得する
            EntryProcessDataDetail data = iterator.next();
            if(data.getAssetNo() != null && !"".equals(data.getAssetNo())) {
                data.setProcessDiv(CommonConst.PROCESS_DIV.Return.getString());      //返却
                data.setEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));            //ログイン担当者コード
                data.setLocLCd(locCode);        // 20220725 ADD SATO v2_2

                ProcessFactory.Insert(db, data);
            }
        }
    }

    /**
     * 処理履歴データテーブル登録(除却)
     * @param db
     * @param activity
     */
    public void InsertProcesDispose(SQLiteDatabase db, Activity activity){

        //読込んだ除却データを登録する
        for (Iterator<EntryProcessDataDetail> iterator = _mapOK.values().iterator(); iterator.hasNext(); ) {

            //1件取得する
            EntryProcessDataDetail data = iterator.next();
            if(data.getAssetNo() != null && !"".equals(data.getAssetNo())) {
                data.setProcessDiv(CommonConst.PROCESS_DIV.Dispose.getString());      //除却
                data.setEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));            //ログイン担当者コード

                ProcessFactory.Insert(db, data);
            }
        }
    }

    /**
     * 処理履歴データテーブル登録(保管)
     * @param db
     * @param activity
     */
    public void InsertProcesStorage(SQLiteDatabase db, Activity activity){

        //読込んだ保管データを登録する
        for (Iterator<EntryProcessDataDetail> iterator = _mapOK.values().iterator(); iterator.hasNext(); ) {

            //1件取得する
            EntryProcessDataDetail data = iterator.next();
            if(data.getAssetNo() != null && !"".equals(data.getAssetNo())) {
                data.setProcessDiv(CommonConst.PROCESS_DIV.Storage.getString());      //保管
                data.setLocLCd(_LocLCode);                                           //ロケ大区分コード
                data.setLocMCd(_LocMCode);                                           //ロケ中区分コード
                data.setLocSCd(_LocSCode);                                           //ロケ小区分コード
                data.setEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));            //ログイン担当者コード

                ProcessFactory.Insert(db, data);
            }
        }
    }

    /**
     * 処理履歴データテーブル登録(タグ紐付)
     * @param db
     * @param model
     * @param activity
     */
    public static void InsertProcesTaglink(SQLiteDatabase db, AssetModel model, String strProcessDiv, Activity activity){

        //読込んだタグ紐付データを登録する
        EntryProcessDataDetail data = new EntryProcessDataDetail();
        data.setAssetNo(model.getAssetNo());
        data.setAssetName(model.getAssetName());
        data.setAssetId(model.getAssetID());
        data.setProcessDiv(strProcessDiv);      //タグ紐付
        data.setEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));            //ログイン担当者コード

        ProcessFactory.Insert(db, data);
    }

    /**
     * 一時表を作成し読取データを登録する
     * @param db
     */
    public void CreateTempTable(SQLiteDatabase db) {

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" CREATE TEMP TABLE ");
        sql.append(CommonConst.TEMP_PROCESS_LOG);
        sql.append(" (");
        sql.append("  ASSET_NO");                      //資産番号
        sql.append("  ,ASSET_ID");                     //資産ID
        sql.append("  ,ASSET_STATUS");                //資産状態
        sql.append("  ) ");

        //SQL発行
        db.execSQL(sql.toString());

        for(String assetNo : GetAssetNo()){
            //SQL作成
            StringBuilder sqlInsert = new StringBuilder();
            sqlInsert.append(" INSERT");
            sqlInsert.append(" INTO ");
            sqlInsert.append(CommonConst.TEMP_PROCESS_LOG);
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
        }

        for(String assetID : GetAssetID()){
            //SQL作成
            StringBuilder sqlInsert = new StringBuilder();
            sqlInsert.append(" INSERT");
            sqlInsert.append(" INTO ");
            sqlInsert.append(CommonConst.TEMP_PROCESS_LOG);
            sqlInsert.append(" (");
            sqlInsert.append("  ASSET_ID");                //資産番号
            sqlInsert.append("  ) ");
            sqlInsert.append(" VALUES (");
            sqlInsert.append(" ?");
            sqlInsert.append(" )");

            //SQL発行
            db.execSQL(sqlInsert.toString(), new String[]{
                    assetID             //資産ID
            });
        }

    }

    /**
     * 一時表を作成し読取データを登録する
     * @param db
     */
    public void CreateMatTempTable(SQLiteDatabase db) {

        //SQL作成
        StringBuilder sql = new StringBuilder();
        sql.append(" CREATE TEMP TABLE ");
        sql.append(CommonConst.TEMP_PROCESS_LOG);
        sql.append(" (");
        sql.append("  ASSET_NO");                      //資産番号
        sql.append("  ,ASSET_ID");                     //資産ID
        sql.append("  ,ASSET_STATUS");                //資産状態
        sql.append("  ) ");

        //SQL発行
        db.execSQL(sql.toString());

        for(String assetNo : GetMatAssetNo()){
            //SQL作成
            StringBuilder sqlInsert = new StringBuilder();
            sqlInsert.append(" INSERT");
            sqlInsert.append(" INTO ");
            sqlInsert.append(CommonConst.TEMP_PROCESS_LOG);
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
        }

        for(String assetID : GetMatAssetID()){
            //SQL作成
            StringBuilder sqlInsert = new StringBuilder();
            sqlInsert.append(" INSERT");
            sqlInsert.append(" INTO ");
            sqlInsert.append(CommonConst.TEMP_PROCESS_LOG);
            sqlInsert.append(" (");
            sqlInsert.append("  ASSET_ID");                //資産番号
            sqlInsert.append("  ) ");
            sqlInsert.append(" VALUES (");
            sqlInsert.append(" ?");
            sqlInsert.append(" )");

            //SQL発行
            db.execSQL(sqlInsert.toString(), new String[]{
                    assetID             //資産ID
            });
        }

        for(String assetNo : GetAssetNo()){
            //SQL作成
            StringBuilder sqlInsert = new StringBuilder();
            sqlInsert.append(" INSERT");
            sqlInsert.append(" INTO ");
            sqlInsert.append(CommonConst.TEMP_PROCESS_LOG);
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
        }

        for(String assetID : GetAssetID()){
            //SQL作成
            StringBuilder sqlInsert = new StringBuilder();
            sqlInsert.append(" INSERT");
            sqlInsert.append(" INTO ");
            sqlInsert.append(CommonConst.TEMP_PROCESS_LOG);
            sqlInsert.append(" (");
            sqlInsert.append("  ASSET_ID");                //資産番号
            sqlInsert.append("  ) ");
            sqlInsert.append(" VALUES (");
            sqlInsert.append(" ?");
            sqlInsert.append(" )");

            //SQL発行
            db.execSQL(sqlInsert.toString(), new String[]{
                    assetID             //資産ID
            });
        }

    }

//20191126 Add Start Sasaki
    /**
     * 全資産データを読み込みます。
     * @param db
     */
    public void SetAssetAllData(SQLiteDatabase db) {

        //クリアする
        _mapAssetMasterAll.clear();

        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO,");
        sql.append("  ASSET_ID");
        sql.append(" FROM ");
        sql.append(CommonConst.MASTER_ASSET);
        sql.append(" WHERE");
        sql.append("  ASSET_STATUS NOT IN (0, 9)");       //未紐付、除却を除く

        //SQL発行
        Cursor cursorAll = db.rawQuery(sql.toString(), null);
        while (cursorAll.moveToNext()) {
            //資産マスタデータに溜め込む
            _mapAssetMasterAll.put(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO"))            //資産番号
                                    , cursorAll.getString(cursorAll.getColumnIndex("ASSET_ID")));          //資産ID
        }
    }
//20191126 Add End Sasaki

    //20241101 Add Start Shirakawa v4.0

    /**
     * 処理履歴データテーブル登録(持出照合/持帰照合/照合返却)
     * @param db
     * @param activity
     */
    public void InsertMatProces(SQLiteDatabase db, Activity activity ,String prmProcessDiv,String ConstRuctionNo,String MatchDiv,int OkData,String LentEndDt){

        int cnt = 0;
        //処理履歴IDクリア
        _ProsessId = "";

        //読込んだ貸出データを登録する
        for (Iterator<EntryProcessDataDetail> iterator = _mapOK.values().iterator(); iterator.hasNext(); ) {
            //1件取得する
            EntryProcessDataDetail data = iterator.next();

            //照合返却の場合チェックを行う
            if(prmProcessDiv.equals(CommonConst.PROCESS_DIV.MatchReturn.getString())){
                //貸出権限が無い資産の場合、登録しない
                if (!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, data.getAssetNo()), CommonAuthority.Auth.updHubList)) {
                    continue;
                }

                //貸出チェック対象で貸出回数が上限を超えた場合、登録しない
                if (CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString().equals(GetLeaseUpperErrDiv(data.getAssetNo())) &&
                        CheckLeaseNum(data.getAssetNo())) {
                    continue;
                }
            }

            if(data.getAssetNo() != null && !"".equals(data.getAssetNo())) {
                data.setProcessDiv(prmProcessDiv);
                data.setEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));            //ログイン担当者コード

                //初めの一回のみ
                if(cnt == 0){
                    ProcessFactory.MatInsert(db, data,ConstRuctionNo,MatchDiv,String.valueOf(OkData),LentEndDt);
                    //カウントアップ
                    cnt = cnt +1;
                }

                //登録して作成されたProcessLogIdを保存する
                data.setProcessLogId(_ProsessId);

                if (prmProcessDiv.equals(CommonConst.PROCESS_DIV.MatchIn.getString()) || prmProcessDiv.equals(CommonConst.PROCESS_DIV.MatchOut.getString())){
                    //持出照合か持帰照合の場合
                    //照合資産リストテーブルに登録
                    MatchAseetListFactory.Insert(db,data,ConstRuctionNo);
                }else if(prmProcessDiv.equals(CommonConst.PROCESS_DIV.MatchReturn.getString())){
                    //照合返却の場合
                    //返却リストテーブルに登録
                    RetAseetListFactory.Insert(db,data,ConstRuctionNo);
                }

            }
        }
    }

    /**
     * 処理履歴データテーブル登録(返却(照合))
     * @param db
     * @param activity
     */
    public void InsertMatReturnProces(SQLiteDatabase db, Activity activity ,String prmProcessDiv){

        int cnt = 0;
        //処理履歴IDクリア
        _ProsessId = "";

        //読込んだ貸出データを登録する
        for (Iterator<EntryProcessDataDetail> iterator = _mapOK.values().iterator(); iterator.hasNext(); ) {
            //1件取得する
            EntryProcessDataDetail data = iterator.next();

                //貸出権限が無い資産の場合、登録しない
                if (!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, data.getAssetNo()), CommonAuthority.Auth.updHubList)) {
                    continue;
                }

                //貸出チェック対象で貸出回数が上限を超えた場合、登録しない
                if (CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString().equals(GetLeaseUpperErrDiv(data.getAssetNo())) &&
                        CheckLeaseNum(data.getAssetNo())) {
                    continue;
                }

            if(data.getAssetNo() != null && !"".equals(data.getAssetNo())) {
                data.setProcessDiv(prmProcessDiv);
                data.setEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));            //ログイン担当者コード

                //初めの一回のみ
                if(cnt == 0){
                    ProcessFactory.MatInsert(db, data,"","","","");
                    //カウントアップ
                    cnt = cnt +1;
                }

                //登録して作成されたProcessLogIdを保存する
                data.setProcessLogId(_ProsessId);


                    //照合返却の場合
                    //返却リストテーブルに登録
                    RetAseetListFactory.Insert(db,data,"");


            }
        }
    }

    /**
     * 処理履歴データテーブル登録(貸出(照合))
     * @param db
     * @param activity
     */
    public void InsertMatProcesLease(SQLiteDatabase db, Activity activity ,String prmProcessDiv,String ConstRuctionNo,String LentEndDt){

        int cnt = 0;
        //処理履歴IDクリア
        _ProsessId = "";

        //読込んだ貸出データを登録する
        for (Iterator<EntryProcessDataDetail> iterator = _mapOK.values().iterator(); iterator.hasNext(); ) {
            //1件取得する
            EntryProcessDataDetail data = iterator.next();

            //貸出権限が無い資産の場合、登録しない
            if (!CommonAuthority.CheckAssetAuth(db, AssetFactory.Select(db, data.getAssetNo()), CommonAuthority.Auth.updHubList)) {
                continue;
            }

            //貸出状況が貸出中の場合、登録しない
            if (CommonConst.ASSET_STATUS.Lease.getString().equals(data.getAssetStatus())){
                continue;
            }

            //貸出チェック対象で貸出回数が上限を超えた場合、登録しない
            if (CommonConst.LEASE_UPPER_ERR_DIV.ERROR.getString().equals(GetLeaseUpperErrDiv(data.getAssetNo())) &&
                    CheckLeaseNum(data.getAssetNo())) {
                continue;
            }


            if(data.getAssetNo() != null && !"".equals(data.getAssetNo())) {
                data.setProcessDiv(prmProcessDiv);
                data.setEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));            //ログイン担当者コード

                //初めの一回のみ
                if(cnt == 0){
                    ProcessFactory.MatInsert(db, data,ConstRuctionNo,"0","",LentEndDt);
                    //カウントアップ
                    cnt = cnt +1;
                }

                //登録して作成されたProcessLogIdを保存する
                data.setProcessLogId(_ProsessId);

                //貸出リストテーブルに追加
                LentAssetListFactory.Insert(db,data,ConstRuctionNo);

                //既に含まれているか確認
                if (!ExistCollationAssetID(data.getAssetId())) {
                    //工事資産テーブルに貸出したものを追加する
                    ConstAssetFactory.AseetInsert(db,data,ConstRuctionNo);
                }
            }
        }
    }

    /**
     * 処理履歴データテーブル登録(仮置き　新規登録)
     * @param db
     * @param activity
     */
    public void InsertMatProcesTemp(SQLiteDatabase db, Activity activity ,String prmProcessDiv,String ConstRuctionNo,String TempId,String LentEndDt){

        int cnt = 0;
        //処理履歴IDクリア
        _ProsessId = "";

        //読込んだ貸出データを登録する
        for (Iterator<EntryProcessDataDetail> iterator = _mapOK.values().iterator(); iterator.hasNext(); ) {
            //1件取得する
            EntryProcessDataDetail data = iterator.next();

            if(data.getAssetNo() != null && !"".equals(data.getAssetNo())) {
                data.setProcessDiv(prmProcessDiv);
                data.setEmployeeCd(SystemSetting.getString(CommonConst.VIEW_KEY_LOGIN_EMPLOYEE_CD, "", activity));            //ログイン担当者コード

                //初めの一回のみ
                if(cnt == 0){
                    ProcessFactory.MatInsert(db, data,ConstRuctionNo,"0","",LentEndDt);
                    //カウントアップ
                    cnt = cnt +1;

                    //登録して作成されたProcessLogIdを保存する
                    data.setProcessLogId(_ProsessId);

                    //仮置きリストテーブルに追加
                    TempListFactory.Insert(db,data,ConstRuctionNo, TempId);

                }else{
                    //登録して作成されたProcessLogIdを保存する
                    data.setProcessLogId(_ProsessId);
                }

                //仮置き資産リストテーブルに追加
                TempAssetListFactory.Insert(db,data,ConstRuctionNo, TempId);

                //仮置き資産テーブルに追加
                TempAssetFactory.Insert(db,data,ConstRuctionNo, TempId);
            }
        }
    }

    //20241101 Add End Shirakawa v4.0

//20241018 Add Start Shirakawa v4.0

    /**
     * 照合情報をクリアします。
     */
    public void CleaCollationData() {
        //クリアする
        _mapCollation.clear();
        _mapCollationView.clear();
    }

    /**
     * 工事と紐づいて貸出した資産照合を情報に追加。
     * @param db
     */
    public void SetCollationData(SQLiteDatabase db,String prmConstNo) {

        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  CONST.ASSET_NO,");
        sql.append("  ASSET.ASSET_ID");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_CONST_ASSET + " CONST");
        sql.append(" INNER JOIN " + CommonConst.MASTER_ASSET + " ASSET");
        sql.append(" ON CONST.ASSET_NO = ASSET.ASSET_NO ");

        sql.append(" WHERE");
        sql.append("  CONST.CONST_NO = '" + prmConstNo + "'");       //工事番号

        //SQL発行
        Cursor cursorAll = db.rawQuery(sql.toString(), null);
        while (cursorAll.moveToNext()) {
            //照合データに溜め込む
            _mapCollation.put(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO"))            //資産番号
                    , cursorAll.getString(cursorAll.getColumnIndex("ASSET_ID")));          //資産ID

            //画面表示用
            _mapCollationView.put(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO"))            //資産番号
                    , cursorAll.getString(cursorAll.getColumnIndex("ASSET_ID")));          //資産ID
        }

    }

    /**
     * 工事と紐づいて今回貸出した資産照合を情報に追加。
     * @param prmConstNo
     * @return
     */
    public void SetCollationLentData(SQLiteDatabase db,String prmConstNo){
        StringBuffer sql = new StringBuffer();

        sql.append(" SELECT");
        sql.append("  LENT_ASEET.ASSET_NO,");
        sql.append("  ASSET.ASSET_ID");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_LENT_ASSET_LIST  + " LENT_ASEET ");
        sql.append(" INNER JOIN " + CommonConst.MASTER_ASSET + " ASSET ");
        sql.append(" ON LENT_ASEET.ASSET_NO = ASSET.ASSET_NO ");

        sql.append(" WHERE ");
        sql.append(" CONST_NO = '" + prmConstNo + "'");

        //SQL発行
        Cursor cursorAll = db.rawQuery(sql.toString(), null);
        while (cursorAll.moveToNext()) {

            //既に含まれていない場合追加する
            if (!_mapCollation.containsKey(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO")))) {
                //照合データに溜め込む
                _mapCollation.put(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO"))            //資産番号
                        , cursorAll.getString(cursorAll.getColumnIndex("ASSET_ID")));          //資産ID

                //画面表示用
                _mapCollationView.put(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO"))            //資産番号
                        , cursorAll.getString(cursorAll.getColumnIndex("ASSET_ID")));          //資産ID
            }

        }
    }

    /**
     * 紐づき仮置き資産情報を照合情報に追加。
     * @param db
     */
    public void SetAllCollationTempDataAdd(SQLiteDatabase db) {

        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  TEMPA.ASSET_NO,");
        sql.append("  ASSET.ASSET_ID");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_ASSET + " TEMPA");
        sql.append(" INNER JOIN " + CommonConst.MASTER_ASSET + " ASSET");
        sql.append(" ON TEMPA.ASSET_NO = ASSET.ASSET_NO ");

        //SQL発行
        Cursor cursorAll = db.rawQuery(sql.toString(), null);
        while (cursorAll.moveToNext()) {

        //照合データに溜め込む
        _listTempList.add(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO")));          //資産番号

        }
    }

    /**
     * 紐づき仮置き資産情報を照合情報に追加(現在の仮置きID以外)。
     * @param db
     */
    public void SetNotTempCollationTempDataAdd(SQLiteDatabase db,String tempId) {

        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  TEMPA.ASSET_NO,");
        sql.append("  ASSET.ASSET_ID");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_ASSET + " TEMPA");
        sql.append(" INNER JOIN " + CommonConst.MASTER_ASSET + " ASSET");
        sql.append(" ON TEMPA.ASSET_NO = ASSET.ASSET_NO ");
        sql.append(" WHERE");
        sql.append("  TEMP_ID NOT IN( '" + tempId + "')");       //特定の仮置き資産以外

        //SQL発行
        Cursor cursorAll = db.rawQuery(sql.toString(), null);
        while (cursorAll.moveToNext()) {

            //照合データに溜め込む
            _listTempList.add(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO")));          //資産番号

        }
    }

    /**
     * 工事別仮置きした資産を照合情報から削除。
     * @param db
     */
    public void SetCollationTempDataAdd(SQLiteDatabase db, String prmConstNo) {

        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  TEMPA.ASSET_NO,");
        sql.append("  ASSET.ASSET_ID");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_ASSET + " TEMPA");
        sql.append(" INNER JOIN " + CommonConst.MASTER_ASSET + " ASSET");
        sql.append(" ON TEMPA.ASSET_NO = ASSET.ASSET_NO ");
        sql.append(" INNER JOIN " + CommonConst.TABLE_TEMPORARY + " TEMP");
        sql.append(" ON TEMPA.TEMP_ID = TEMP.TEMP_ID ");

        Date _date = new Date();
        SimpleDateFormat DtFormat = new SimpleDateFormat("yyyyMMdd");
        String CheckDate = DtFormat.format(_date);

        sql.append(" WHERE");
        sql.append("  TEMPA.CONST_NO = '" + prmConstNo + "'");       //工事番号
//        sql.append("  AND TEMP.TEMP_DT = '" + CheckDate + "'");       //工事番号


        //SQL発行
        Cursor cursorAll = db.rawQuery(sql.toString(), null);
        while (cursorAll.moveToNext()) {

            //既に含まれているか確認
            if (_mapCollation.containsKey(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO")))) {
                // 照合データに含まれている場合、そのデータを削除
                String assetNo = cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO"));

                _mapCollation.remove(assetNo); // 削除処理
                _mapCollationView.remove(assetNo); // 削除処理
            }

        }
    }

    /**
     * 工事別仮置きした資産を照合情報から削除。
     * @param db
     */
    public void SetCollationTempListDataAdd(SQLiteDatabase db,String prmConstNo) {

        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  ASSET_NO ");
        sql.append(" FROM ");
        sql.append(CommonConst.TABLE_TEMP_ASSET_LIST);

        sql.append(" WHERE");
        sql.append("  CONST_NO = '" + prmConstNo + "'");       //工事番号

        //SQL発行
        Cursor cursorAll = db.rawQuery(sql.toString(), null);
        while (cursorAll.moveToNext()) {

            //既に含まれているか確認
            if (_mapCollation.containsKey(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO")))) {
                // 照合データに含まれている場合、そのデータを削除
                String assetNo = cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO"));

                _mapCollation.remove(assetNo); // 削除処理
                _mapCollationView.remove(assetNo); // 削除処理
            }

        }
    }

//20241018 Add End Shirakawa v4.0

    /**
     * 照合機能用一覧並び替え用Map　資産を溜め込む
     * @param strData
     */
    public void SetSortMap1(String strAssetNo, String strData) {
        //読込資産IDデータを追加
        _SortMap1.put(strAssetNo,strData);
    }

    /**
     * 照合機能用一覧並び替え用Map　取得
     */
    public Map<String, String> GetSortMap1() {
        return _SortMap1;
    }
    /**
     * 照合機能用一覧並び替え用Map　資産を溜め込む
     * @param strData
     */
    public void SetSortMap2(String strAssetNo, String strData) {
        //読込資産IDデータを追加
        _SortMap2.put(strAssetNo,strData);
    }

    /**
     * 照合機能用一覧並び替え用Map　取得
     */
    public Map<String, String> GetSortMap2() {
        return _SortMap2;
    }

    /**
     * 一覧並び替え用List　資産を溜め込む
     * @param strData
     */
    public void SetSortList3(String strData) {
        //読込資産IDデータを追加
        _SortList3.add(strData);
    }

    /**
     * 一覧並び替え用List　取得
     */
    public List<String> GetSortList3() {
        return _SortList3;
    }

    /**
     * 照合機能用一覧並び替え用Map(リスト不一致)　資産を溜め込む
     * @param strData
     */
    public void SetSortMap4(String strAssetNo, String strData) {
        //読込資産IDデータを追加
        _SortMap4.put(strAssetNo,strData);
    }

    /**
     * 照合機能用一覧並び替え用Map　取得
     */
    public Map<String, String> GetSortMap4() {
        return _SortMap4;
    }

    /**
     * 照合機能用一覧並び替え用Map　クリア
     */
    public void ClearSortData() {
        _SortMap1.clear();
        _SortMap2.clear();
        _SortList3.clear();
        _SortMap4.clear();
    }

    //202411/01 Add Start Shirakawa v4.0

    /**
     * 資産IDを溜め込む
     * @param strData
     */
    public void StockAssetID(String strData) {
        //読込資産IDデータを追加
        _listAssetIDRead.add(strData);
    }

    /**
     * 未読み取り照合資産IDを溜め込む
     */
    public static void addKeysTolistMatAssetIDRead() {
        for (Map.Entry<String, String> entry : _mapCollationView.entrySet()) {
            _listMatAssetIDRead.add(entry.getValue());
        }
    }

    /**
     * 未読み取り照合資産NOを溜め込む
     */
    public static void addKeysTolistMatAssetNORead() {
        for (Map.Entry<String, String> entry : _mapCollationView.entrySet()) {
            _listMatAssetNORead.add(entry.getKey());
        }
    }

    /**
     * 溜め込んだ資産IDを取得する
     * @return
     */
    public List<String> GetAssetID() {
        return _listAssetIDRead;
    }

    /**
     * 溜め込んだ照合資産IDを取得する
     * @return
     */
    public List<String> GetMatAssetID() {
        return _listMatAssetIDRead;
    }

    /**
     * 溜め込んだ照合資産NOを取得する
     * @return
     */
    public List<String> GetMatAssetNo() {
        return _listMatAssetNORead;
    }

    /**
     * 登録対象の資産データを取得する
     * @return
     */
    public HashMap<String, EntryProcessDataDetail> GetRegisterAsset() {
        return _mapOK;
    }

    /**
     * 読込データに資産IDをチェックする
     * @param strData
     * @return
     */
    public boolean ExistAssetID(String strData) {
        boolean blnReturn = false;

        if (_listAssetIDRead.contains(strData)) {
            blnReturn = true;
        }

        return blnReturn;
    }

    /**
     * 資産IDがマスタに存在するかチェックする
     * @param strData
     * @return
     */
    public boolean ExistMasterAssetID(String strData) {
        boolean blnReturn = false;
        //資産IDがマスタに存在する場合
        if (_mapAssetMasterAll.containsValue(strData)) {
            blnReturn = true;
        }
        return blnReturn;
    }

    /**
     * 資産Noがマスタに存在するかチェックする
     * @param strData
     * @return
     */
    public boolean ExistMasterAssetNo(String strData) {
        boolean blnReturn = false;
        //資産Noがマスタに存在する場合
        if (_mapAssetMasterAll.containsKey(strData)) {
            blnReturn = true;
        }
        return blnReturn;
    }

    /**
     * 資産IDが照合情報に存在するかチェックする
     * @param strData
     * @return
     */
    public boolean ExistCollationAssetID(String strData) {
        boolean blnReturn = false;
        //資産IDがマスタに存在する場合
        if (_mapCollation.containsValue(strData)) {
            blnReturn = true;
        }
        return blnReturn;
    }

    /**
     * 未読み取り照合情報(表示用)からRemove
     * @param strData
     * @return
     */
    public void RemoveCollationViewAssetNo(String strData) {
        if (_mapCollationView.containsKey(strData)) {
            //資産IDがマスタに存在する場合
            _mapCollationView.remove(strData); // 削除処理
        }
    }

    /**
     * 資産番号が照合情報に存在するかチェックする
     * @param strData
     * @return
     */
    public boolean ExistCollationAssetNo(String strData) {
        boolean blnReturn = false;
        //資産IDがマスタに存在する場合
        if (_mapCollation.containsKey(strData)) {
            blnReturn = true;
        }
        return blnReturn;
    }

    /**
     * 未読み取り資産IDが照合情報に存在するかチェックする
     * @param strData
     * @return
     */
    public boolean ExistCollationViewAssetNo(String strData) {
        boolean blnReturn = false;
        //資産IDがマスタに存在する場合
        if (_mapCollationView.containsKey(strData)) {
            blnReturn = true;
        }
        return blnReturn;
    }

    /**
     * 資産番号を溜め込む
     * @param strData
     */
    public void StockAssetNo(String strData) {
        //読込データを追加
        _listAssetNoRead.add(strData);
    }

    /**
     * カウント表示用の資産番号or資産IDを溜め込む
     * @param strData
     */
    public void StockViewAssetData(String strData) {
        _listAssetMasterRead.add(strData);
    }

    /**
     * 溜め込んだ資産Noを取得する
     * @return
     */
    public List<String> GetAssetNo() {
        return _listAssetNoRead;
    }

    /**
     * 資産Noをチェックする
     * @param strData
     * @return
     */
    public boolean ExistAssetNo(String strData) {
        boolean blnReturn = false;

        if (_listAssetNoRead.contains(strData)) {
            blnReturn = true;
        }

        return blnReturn;
    }

    /**
     * 資産ID件数を取得
     * @return
     */
    public int CountAssetID() {
        return _listAssetIDRead.size();
    }

    /**
     * 資産番号件数を取得
     * @return
     */
    public int CountAssetNo() {
        return _listAssetNoRead.size();
    }

    /**
     * 資産件数を取得
     * @return
     */
    public int CountAll() {
        return _listAssetNoRead.size() + _listAssetIDRead.size();
    }

    /**
     * 資産件数を取得 20200226 Add Sasaki V1.2
     * @return
     */
    public int CountLeaseLimitErr() {
        return _listLeaseLimitErr.size();
    }

    /**
     * 未読み取り照合を取得
     * @return
     */
    public int CountMatAsset() {
        return _listMatAssetIDRead.size() +  _listMatAssetNORead.size();
    }


    /**
     * 資産件数を取得 20200226 Add Sasaki V1.2
     * @return
     */
    public int CountLeaseLimitWarning() {
        return _listLeaseLimitWarning.size();
    }

    //20210918 ADD Takase V2.0 START
    /**
     * 権限無し資産件数を取得
     * @return
     */
    public int CountNoAuth() {
        return _listNoAuth.size();
    }
    //20210918 ADD Takase V2.0 END

    /**
     * 表示用(マスタなし除く)資産件数を取得
     * @return
     */
    public int CountViewAll() {
        return _listAssetMasterRead.size();
    }

    /**
     * 表示用照合データ件数を取得
     * @return
     */
    public int CountmapCollation() {
        return _mapCollation.size();
    }

    /**
     * 資産正常件数を取得
     * @return
     */
    public int CountOKAll() {
        return _mapOK.size();
    }

    /**
     * リース期限エラー件数を取得 //20230830 ADD Kawamoto v2.4.1
     * @return 件数
     */
    public int CountLeaseUntilErr() {
        return _listLeaseUntilErr.size();
    }

    /**
     * ○○期限エラー件数を取得 //20230830 ADD Kawamoto v2.4.1
     * @return 件数
     */
    public int CountGenericUntilErr() {
        return _listGenericUntilErr.size();
    }

    /**
     * 返却期限エラー件数を取得 20230908 ADD okubo v2.4.1
     * @return 件数
     * */
    public int CountReturnUntilErr() {
        return _listReturnUntilErr.size();
    }

    /**
     * マスタ未登録件数を取得
     * @return 件数
     */
    public long CountMstNoData() {
        return _mapAllData.values().stream()
                .filter((data)-> TextUtils.isEmpty(data.getAssetStatus()))
                .count();
    }

    /**
     * データをクリアする
     */
    public void Clear(){
        _listAssetIDRead.clear();
        _listAssetNoRead.clear();
        _listAssetMasterRead.clear();  //20191126 Add Sasaki
        _listLeaseLimitErr.clear();         //20200226 Add Sasaki V1.2
        _listLeaseLimitWarning.clear();    //20200226 Add Sasaki V1.2
        _mapOK.clear();
        _mapAllData.clear();
        _listLeaseUntilErr.clear();       //20230830 ADD Kawamoto v2.4.1
        _listGenericUntilErr.clear();     //20230830 ADD Kawamoto v2.4.1
        _listReturnUntilErr.clear();        //20230908 ADD okubo v2.4.1
    }

    /**
     * データをクリアする
     */
    public static void AllClear(){
        _mapOK.clear();
        _mapAllData.clear();
        _listAssetIDRead.clear();
        _listAssetNoRead.clear();
        _listAssetMasterRead.clear();
        _listLeaseLimitErr.clear();         //20200226 Add Sasaki V1.2
        _listLeaseLimitWarning.clear();    //20200226 Add Sasaki V1.2
        _listLeaseUntilErr.clear();       //20230830 ADD Kawamoto v2.4.1
        _listGenericUntilErr.clear();     //20230830 ADD Kawamoto v2.4.1
        _listReturnUntilErr.clear();      //20230908 ADD okubo v2.4.1
        _LeaseDestination = "";
        _LeaseDestinationName = "";
        _LeaseEmployee = "";
        _LeaseEmployeeName = "";
        _LeaseReturn = "";
        _Remark = "";
        _LocLCode = "";
        _LocMCode = "";
        _LocSCode = "";
        _LocLName = "";
        _LocMName = "";
        _LocSName = "";
    }

    /**
     * ログ送信用仮置きデータ情報をクリアする
     */
    public void AllTempClear() {
        _TempDt = "";
//        _TempWorker = "";
        _TempInfo1 = "";
        _TempInfo2 = "";
        _TempInfo3 = "";
        _TempInfo4 = "";
        _TempInfo5 = "";
        _TempInfo6 = "";
        _TempInfo7 = "";
    }
    /*
    * 資産マスタあり読込データクリア
     */
    public void AssetIDReadClear(){
        _listAssetIDRead.clear();
    }

    /*
     * 資産マスタあり読込データクリア
     */
    public void AssetNoReadClear(){
        _listAssetNoRead.clear();
    }

    /**
     * 未読み取り照合データをクリアする
     */
    public void MatAssetIDReadClear(){
        _listMatAssetIDRead.clear();
    }

    /**
     * 未読み取り照合データをクリアする
     */
    public void MatAssetNOReadClear(){
        _listMatAssetNORead.clear();
    }

    /**
     * 仮置き済みデータ削除
     */
    public void TempListClear(){
        _listTempList.clear();
    }

    /**
     * 仮置き済みデータをチェック
     * @param strData
     * @return
     */
    public boolean ExistTempListAssetNo(String strData) {
        boolean blnReturn = false;

        if (_listTempList.contains(strData)) {
            blnReturn = true;
        }

        return blnReturn;
    }


    /**
     * 処理履歴データをクリアする
     */
    public void ClearTable(SQLiteDatabase db){

        //処理履歴データテーブルの削除
        db.delete(CommonConst.TABLE_PROCESS_LOG, null, null);

    }

    /**
     * 照合処理履歴データをクリアする
     */
    public void ClearMatTable(SQLiteDatabase db){

        //照合処理履歴データテーブルの削除
        db.delete(CommonConst.TABLE_MATCH_LIST, null, null);
        //貸出リスト削除
        LentAssetListFactory.Delete(db);
        //返却リスト削除
        RetAseetListFactory.Delete(db);
        //照合資産リスト削除
        MatchAseetListFactory.Delete(db);
        //仮置きリスト削除
        TempListFactory.Delete(db);
        //仮置き資産リスト
        TempAssetListFactory.Delete(db);

    }

    /**
     * 件数を取得する
     */
    public static long Count(Activity activity) {
        SQLiteDatabase db = null;
        try {
            long longCount = 0;

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(activity);
            db = dbHelper.getWritableDatabase();

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" SELECT ");
            sql.append("  COUNT(*) AS COUNT ");
            sql.append(" FROM ");
            sql.append(CommonConst.TABLE_PROCESS_LOG);

            //SQL発行
            Cursor cursor = db.rawQuery(sql.toString(), null);

            //件数を取得する
            while (cursor.moveToNext()) {
                longCount = cursor.getLong(cursor.getColumnIndex("COUNT"));
            }

            return longCount;

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 件数を取得する(照合データ)
     */
    public static long MatCount(Activity activity) {
        SQLiteDatabase db = null;
        try {
            long longCount = 0;

            //DB接続
            DatabaseHelper dbHelper = new DatabaseHelper(activity);
            db = dbHelper.getWritableDatabase();

            //SQL作成
            StringBuilder sql = new StringBuilder();
            sql.append(" SELECT ");
            sql.append("  COUNT(*) AS COUNT ");
            sql.append(" FROM ");
            sql.append(CommonConst.TABLE_MATCH_LIST);

            //SQL発行
            Cursor cursor = db.rawQuery(sql.toString(), null);

            //件数を取得する
            while (cursor.moveToNext()) {
                longCount = cursor.getLong(cursor.getColumnIndex("COUNT"));
            }

            return longCount;

        } finally {
            if (db != null) {
                db.close();
            }
        }
    }

    /**
     * 貸出回数が上限に達しているかチェックする 20200225 Add Sasaki V1.2
     * @param strAssetNo
     * @return
     */
    public boolean CheckLeaseNum(String strAssetNo) {
        boolean blnReturn = false;
       //資産マスタのデータを取得する
        EntryProcessDataDetail data = this._mapAllData.get(strAssetNo);

        //上限回数がNULLの場合、上限とする
        if (data.getLeaseUpperLimit().isEmpty()) {
            blnReturn = true;
        } else {
            int leaseCount = Integer.valueOf(Objects.toString(data.getLeaseCount(), "0"));              //貸出回数
            int leaseUpperLimit = Integer.valueOf(Objects.toString(data.getLeaseUpperLimit(), "0"));    //貸出上限
            //貸出回数 >= 貸出上限
            if (leaseCount >= leaseUpperLimit) {
                blnReturn = true;
            }
        }
        return blnReturn;
    }

    /**
     * リース期限が期限日判定日付を超えているかチェックする 20230830 ADD Kawamoto v2.4.1
     * @param strAssetNo, dtDeadline
     * @return
     */
    public boolean CheckLeaseLimit(String strAssetNo, Date dtDeadline) {
        boolean blnReturn = true;
        try {
            SimpleDateFormat DtFormat = new SimpleDateFormat("yyyy/MM/dd");

            //資産マスタのデータを取得する
            EntryProcessDataDetail data = this._mapAllData.get(strAssetNo);

            if (!data.getLeaseLimit().equals("")) {
                if (DtFormat.parse(data.getLeaseLimit()).before(dtDeadline)) {
                    blnReturn = false;
                }
            }
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
        return blnReturn;
    }

    /**
     * ○○期限が期限日判定日付を超えているかチェックする 20230830 ADD Kawamoto v2.4.1
     * @param strAssetNo, dtDeadline
     * @return
     */
    public boolean CheckGenericLimit(String strAssetNo, Date dtDeadline) {
        boolean blnReturn = true;
        try {
            SimpleDateFormat DtFormat = new SimpleDateFormat("yyyy/MM/dd");

            //資産マスタのデータを取得する
            EntryProcessDataDetail data = this._mapAllData.get(strAssetNo);

            if (!data.getGenericLimit().equals("")) {
                if (DtFormat.parse(data.getGenericLimit()).before(dtDeadline)) {
                    blnReturn = false;
                }
            }
        } catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
        return blnReturn;
    }

    /**
     * 貸出上限エラー区分を取得する 20200225 Add Sasaki V1.2
     * @param strAssetNo
     * @return
     */
    public String GetLeaseUpperErrDiv(String strAssetNo) {
        EntryProcessDataDetail data = this._mapAllData.get(strAssetNo);
        String strErrDiv = data.getLeaseUpperErrDiv();
        return strErrDiv;
    }

    //20210918 ADD Takase V2.0 START
    /**
     * 貸出か否かを設定する
     * @param isLease
     */
    public void setIsLease(boolean isLease) {
        this._isLease = isLease;
        this._isReturn = false;     //20230906 ADD okubo V2.4.1
    }
    //20210918 ADD Takase V2.0 END

    /**
     * 貸出かチェックする
     * @return true：貸出、false：貸出以外
     */
    public boolean isLease() {
    	//20210918 CHG Takase V2.0 START
        return this._isLease;
        //boolean blnReturn = false;
        //if (!_LeaseDestination.isEmpty() || !_LeaseEmployee.isEmpty()) {
        //    blnReturn = true;
        //}
        //return blnReturn;
    	//20210918 CHG Takase V2.0 END
    }

    /**
     * 貸出(照合)か否かを設定する
     * @param isMatLease
     */
    public void setIsMatLease(boolean isMatLease) {
        this._isMatLease = isMatLease;
    }

    /**
     * 貸出(照合)かチェックする
     * @return true：貸出、false：貸出以外
     */
    public boolean isMatLease() {
        return this._isMatLease;
    }

    /**
     * 仮置きか否かを設定する
     * @param isTemporary
     */
    public void setIsTemporary(boolean isTemporary) {
        this._isTemporary = isTemporary;
    }

    /**
     * 仮置きかチェックする
     * @return true：仮置き、false：仮置き以外
     */
    public boolean isTemporary() {
        return this._isTemporary;
    }


    /**
     * 返却であることをセットする 20230906 ADD okubo V2.4.1
     * */
    public void setReturn() {
        this._isReturn = true;
    }

    /**
     * 返却かチェックする 20230906 ADD okubo V2.4.1
     * @return 返却(=true)
     * */
    public boolean isReturn() {
        return this._isReturn;
    }

    /**
     * 貸出先コードを設定する
     * @param LeaseDestination
     */
    public void setLeaseDestination(String LeaseDestination) {
        this._LeaseDestination = LeaseDestination;
    }

    /**
     * 貸出先コードを取得する
     * @return
     */
    public String getLeaseDestination() {
        return this._LeaseDestination;
    }

    /**
     * 貸出先名を設定する
     * @param LeaseDestinationName
     */
    public void setLeaseDestinationName(String LeaseDestinationName) {
        this._LeaseDestinationName = LeaseDestinationName;
    }

    /**
     * 貸出先名を取得する
     * @return
     */
    public String getLeaseDestinationName() {
        return this._LeaseDestinationName;
    }

    /**
     * 貸出担当コードを設定する
     * @param LeaseEmployee
     */
    public void setLeaseEmployee(String LeaseEmployee) {
        this._LeaseEmployee = LeaseEmployee;
    }

    /**
     * 貸出担当コードを取得する
     * @return
     */
    public String getLeaseEmployee() {
        return this._LeaseEmployee;
    }

    /**
     * 貸出担当名を設定する
     * @param LeaseEmployeeName
     */
    public void setLeaseEmployeeName(String LeaseEmployeeName) {
        this._LeaseEmployeeName = LeaseEmployeeName;
    }

    /**
     * 貸出担当名を取得する
     * @return
     */
    public String getLeaseEmployeeName() {
        return this._LeaseEmployeeName;
    }

    /**
     * 備考を設定する
     * @param Remark
     */
    public void setRemark(String Remark) {
        this._Remark = Remark;
    }

    /**
     * 備考を取得する
     * @return
     */
    public String getRemark() {
        return this._Remark;
    }

    /**
     * 返却予定日を設定する
     * @param LeaseReturn
     */
    public void setReturnDate(String LeaseReturn) {
        this._LeaseReturn = LeaseReturn;
    }

    /**
     * 返却予定日を取得する
     * @return
     */
    public String getReturnDate() {
        return this._LeaseReturn;
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
    /**
     * 照合一致数インクリメント
     */
    public void cntListCnt() {
        this._ListCnt = _ListCnt + 1;
    }

    /**
     * 照合一致数取得
     * @return
     */
    public int getListCnt() {
        return this._ListCnt;
    }

    /**
     * 照合一致数クリア
     * @return
     */
    public void ClearListCnt() {_ListCnt = 0;}

    /**
     * リスト不一致数インクリメント
     */
    public void cntNoListCnt() {
        this._NoListCnt = _NoListCnt + 1;
    }

    /**
     * リスト不一致数取得
     * @return
     */
    public int getNoListCnt() {
        return this._NoListCnt;
    }

    /**
     * リスト不一致数クリア
     * @return
     */
    public void ClearNoListCnt() {_NoListCnt = 0;}

    /**
     * 他仮置き数インクリメント
     */
    public void cntAlreadyTempCnt() {
        this._AlreadyTempCnt = _AlreadyTempCnt + 1;
    }

    /**
     * 他仮置き数取得
     * @return
     */
    public int getAlreadyTempCnt() {
        return this._AlreadyTempCnt;
    }

    /**
     * 他仮置き数クリア
     * @return
     */
    public void ClearAlreadyTempCnt() {_AlreadyTempCnt = 0;}

    /**
     * 処理履歴ID(照合)を取得
     * @return
     */
    public String getProsessId() {
        return this._ProsessId;
    }

    /**
     * 仮置き日を設定する
     * @param TempDt
     */
    public void setTempDt(String TempDt) {
        this._TempDt = TempDt;
    }

    /**
     * 仮置き日を取得する
     * @return
     */
    public String getTempDt() {
        return this._TempDt;
    }

//    /**
//     * 責任者を設定する
//     * @param TempWorker
//     */
//    public void setTempWorker(String TempWorker) {
//        this._TempWorker = TempWorker;
//    }

//    /**
//     * 責任者を取得する
//     * @return
//     */
//    public String getTempWorker() {
//        return this._TempWorker;
//    }

    /**
     * 仮置き情報1を設定する
     * @param TempInfo1
     */
    public void setTempInfo1(String TempInfo1) {
        this._TempInfo1 = TempInfo1;
    }

    /**
     * 仮置き情報1を取得する
     * @return
     */
    public String getTempInfo1() {
        return this._TempInfo1.replace("$$$", "");
    }

    /**
     * 仮置き情報2を設定する
     * @param TempInfo2
     */
    public void setTempInfo2(String TempInfo2) {
        this._TempInfo2 = TempInfo2;
    }

    /**
     * 仮置き情報3を取得する
     * @return
     */
    public String getTempInfo2() {
        return this._TempInfo2.replace("$$$", "");
    }

    /**
     * 仮置き情報2を設定する
     * @param TempInfo3
     */
    public void setTempInfo3(String TempInfo3) {
        this._TempInfo3 = TempInfo3;
    }

    /**
     * 仮置き情報3を取得する
     * @return
     */
    public String getTempInfo3() {
        return this._TempInfo3.replace("$$$", "");
    }

    /**
     * 仮置き情報4を設定する
     * @param TempInfo4
     */
    public void setTempInfo4(String TempInfo4) {
        this._TempInfo4 = TempInfo4;
    }

    /**
     * 仮置き情報4を取得する
     * @return
     */
    public String getTempInfo4() {
        return this._TempInfo4.replace("$$$", "");
    }

    /**
     * 仮置き情報5を設定する
     * @param TempInfo5
     */
    public void setTempInfo5(String TempInfo5) {
        this._TempInfo5 = TempInfo5;
    }

    /**
     * 仮置き情報5を取得する
     * @return
     */
    public String getTempInfo5() {
        return this._TempInfo5.replace("$$$", "");
    }

    /**
     * 仮置き情報6を設定する
     * @param TempInfo5
     */
    public void setTempInfo6(String TempInfo5) {
        this._TempInfo6 = TempInfo5;
    }

    /**
     * 仮置き情報6を取得する
     * @return
     */
    public String getTempInfo6() {
        return this._TempInfo6.replace("$$$", "");
    }

    /**
     * 仮置き情報7を設定する
     * @param TempInfo5
     */
    public void setTempInfo7(String TempInfo5) {
        this._TempInfo7 = TempInfo5;
    }

    /**
     * 仮置き情報7を取得する
     * @return
     */
    public String getTempInfo7() {
        return this._TempInfo7.replace("$$$", "");
    }

    /**
     * 警告文言 追加表示 20230908 ADD okubo v2.4.1
     * @param strAssetNo 資産番号
     * @param value 文言
     * */
    public void setWarningDisp(String strAssetNo, String value, int sortRank) {
        EntryProcessDataDetail data = this._mapAllData.get(strAssetNo);
        if(data != null) {
            data.setWarningDisplay(value, sortRank);
        }
    }
}
