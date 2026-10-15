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
import java.util.Objects;

public class MatchProcessFactory implements Serializable {

    private static HashMap<String, String> _mapAssetMasterAll = new HashMap<String, String>(); //全資産データ 20191126 Add Sasaki
    private static HashMap<String, EntryProcessDataDetail> _mapOK = new HashMap<String, EntryProcessDataDetail>();          //貸出OKデータ
    private static LinkedHashMap<String, EntryProcessDataDetail> _mapAllData = new LinkedHashMap<String, EntryProcessDataDetail>();    //貸出データ
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
    private static boolean _isLease = false;                                                 //「貸出」かどうか                     20210918 ADD Takase V2.0
    private static boolean _isReturn = false;                                                //「返却」かどうか     20230906 ADD okubo V2.4.1

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

                MatchProcessFactory.Insert(db, data);
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

                MatchProcessFactory.Insert(db, data);
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

                MatchProcessFactory.Insert(db, data);
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

                MatchProcessFactory.Insert(db, data);
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

        MatchProcessFactory.Insert(db, data);
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

    /**
     * 資産IDを溜め込む
     * @param strData
     */
    public void StockAssetID(String strData) {
        //読込資産IDデータを追加
        _listAssetIDRead.add(strData);
    }

    /**
     * 溜め込んだ資産IDを取得する
     * @return
     */
    public List<String> GetAssetID() {
        return _listAssetIDRead;
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
    public void AllClear(){
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
     * 処理履歴データをクリアする
     */
    public void ClearTable(SQLiteDatabase db){

        //処理履歴データテーブルの削除
        db.delete(CommonConst.TABLE_PROCESS_LOG, null, null);

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
