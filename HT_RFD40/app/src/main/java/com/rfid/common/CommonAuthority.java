package com.rfid.common;

import android.app.Activity;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.Factory.AuthFactory;
import com.rfid.Factory.EmployeeHubFactory;
import com.rfid.Factory.LocationFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.setting.SettingValue;

public class CommonAuthority {

    /**
     * その機能を利用する権限があるかチェックする
     * @param db
     * @param strEmployeeCode　社員コード
     * @param strFunctionId　機能ID
     * @return
     */
    public static boolean CheckAuth(SQLiteDatabase db, String strEmployeeCode, String strFunctionId) {
        try {
            if (SettingFactory.GetAuthorityType(db).equals(CommonConst.AUTHORITY_TYPE.NORMAL.getString())) {
                return true;

            } else if (SettingFactory.GetAuthorityType(db).equals(CommonConst.AUTHORITY_TYPE.HUB.getString())) {
                //権限取得
                AuthFactory.GetAuth(db, strEmployeeCode, strFunctionId);
                //参照権限と更新権限の少なくともどちらかがある場合
                if (Auth.readAuthFlg.equals(CommonConst.AUTH_FLG.YES.getString())
                        || Auth.updAuthFlg.equals(CommonConst.AUTH_FLG.YES.getString())) {
                    return true;
                }
            }
            return false;

        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
            return false;
        }
    }

    /**
     * HT.対象拠点に所属しているかチェックする
     * @param db
     * @param strEmployeeCode
     * @param activity
     * @return
     */
    public static boolean CheckHtHub(SQLiteDatabase db, String strEmployeeCode, Activity activity) {
        try {
            //更新権限(他拠点)を持っていない場合
            if (!Auth.updHubAuthFlg.equals(CommonConst.AUTH_FLG.YES.getString())) {

                //HT.対象拠点が設定されている場合
                SettingValue settingValue = new SettingValue();
                if (!settingValue.getTargetHub(activity).equals("")) {

                    //HT.対象拠点を拠点とする社員が見つからない場合
                    if (!EmployeeHubFactory.CheckHub(db, strEmployeeCode, settingValue.getTargetHub(activity))) {
                        return false;
                    }
                }
            }

            return true;

        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
            return false;
        }
    }

    /**
     * 参照権限を持っている拠点リストの取得
     * @param strEmployeeCode
     * @return
     */
    public static String[] GetReadHubList(SQLiteDatabase db, String strEmployeeCode, Activity activity) {
        //拠点リスト
        String[] hubList = null;

        try{
            //拠点権限利用の場合
            if (SettingFactory.GetAuthorityType(db).equals(CommonConst.AUTHORITY_TYPE.HUB.getString())) {
                // 参照権限(他拠点)を持っていない場合
                if (!Auth.readHubAuthFlg.equals(CommonConst.AUTH_FLG.YES.getString())) {

                    SettingValue settingValue = new SettingValue();
                    if (!settingValue.getTargetHub(activity).equals("")) {
                        //HT.対象拠点設定時
                        hubList = new String[1];
                        hubList[0] = settingValue.getTargetHub(activity);

                    } else {
                        //HT.対象拠点未設定時
                        //ユーザーが所属している拠点リストを取得
                        hubList = EmployeeHubFactory.GetHubList(db, strEmployeeCode);
                    }
                }
            }
        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
        return hubList;     //hubList = nullの場合は全拠点利用可能
    }

    /**
     * 更新権限を持っている拠点リストの取得
     * @param strEmployeeCode
     * @return
     */
    public static String[] GetUpdHubList(SQLiteDatabase db, String strEmployeeCode, Activity activity) {
        //拠点リスト
        String[] hubList = null;

        try{
            //拠点権限利用の場合
            if (SettingFactory.GetAuthorityType(db).equals(CommonConst.AUTHORITY_TYPE.HUB.getString())) {

                //更新権限を持っていない場合
                if (!Auth.updAuthFlg.equals(CommonConst.AUTH_FLG.YES.getString())) {
                    hubList = new String[0];

                //更新権限(他拠点)を持っていない場合
                } else if (!Auth.updHubAuthFlg.equals(CommonConst.AUTH_FLG.YES.getString())) {

                    SettingValue settingValue = new SettingValue();
                    if (!settingValue.getTargetHub(activity).equals("")) {
                        //HT.対象拠点設定時
                        //自拠点が複数あっても、更新できる拠点は対象拠点のみ
                        hubList = new String[1];
                        hubList[0] = settingValue.getTargetHub(activity);

                    } else {
                        //HT.対象拠点未設定時
                        //ユーザーが所属している拠点リストを取得
                        hubList = EmployeeHubFactory.GetHubList(db, strEmployeeCode);
                    }
                }
            }

        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));
        }
        return hubList;     //hubList = nullの場合は全拠点利用可能
    }

    /**
     * [貸出][返却][保管][除却][タグ紐付]
     * 権限がある資産かチェックする
     * @return
     */
    public static boolean CheckAssetAuth(SQLiteDatabase db, AssetModel assetModel, String[] hubList) {
        try{
            //権限を持っている場合
            if (hubList == null) {
                // 権限チェック実施しない
                return true;
            }

            //資産が共通資産の場合
//            if (assetModel.getLocShareFlg().equals(CommonConst.LOC_SHARE_FLG.SHARED.getString())){
//                return true;
//            }

            //マスターなしの場合
            String assetNo = assetModel.getAssetNo();
            if (assetNo == null || assetNo == "") {
                return true;
            }

            if (hubList != null) {
                //資産の管理ロケ大区分コードがユーザーの拠点と一致している場合
                for (String loc : hubList) {
                    if (loc.equals(assetModel.getMngLocLCd())) return true;
                }
            }

            return false;

        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));

            return false;
        }
    }

    /**
     *
     * 権限がある工事情報かチェックする
     * @return
     */
    public static boolean CheckConstRactionAuth( String constMLoc, String[] hubList, String[] shareList) {
        try{
            //権限を持っている場合
            if (hubList == null) {
                // 権限チェック実施しない
                return true;
            }

            //更新権限Yの場合
            if (CommonAuthority.Auth.updAuthFlg.equals(CommonConst.AUTH_FLG.YES.getString())){
                if (shareList != null) {
                    //工事情報の管理ロケ大区分コードが共通拠点と一致している場合
                    for (String loc : shareList) {
                        if (loc.equals(constMLoc)) return true;
                    }
                }
            }

            if (hubList != null) {
                //工事情報の管理ロケ大区分コードがユーザーの拠点と一致している場合
                for (String loc : hubList) {
                    if (loc.equals(constMLoc)) return true;
                }
            }

            return false;

        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));

            return false;
        }
    }

    /**
     * [棚卸]
     * 棚卸権限がある資産かチェック
     * @param db
     * @param assetModel
     * @param hubList)
     * @return
     */
    public static boolean CheckAssetInventoryAuth(SQLiteDatabase db, AssetModel assetModel, String[] hubList) {
        try{

            //権限を持っている場合
            if (hubList == null) {
                // 権限チェック実施しない
                return true;
            }

            //棚卸対象のロケ大と資産のロケ大または管理ロケ大が一致している場合
            for (String loc : hubList) {
                if (loc.equals(assetModel.gettLocLCd()))  return true;
                if (loc.equals(assetModel.getMngLocLCd())) return true;
            }

            return false;

        } catch (Exception e) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(e));

            return false;
        }
    }

    /**
     * 機能ID
     */
    public static class FunctionId {
        private static FUNCTION_ID _functionId;

        public void setFunctionId(FUNCTION_ID functionId) { _functionId = functionId; }

        // 機能IDを返す(権限マスタ)
        public static String getFunctionId() {
            switch (_functionId) {
                case FUNCTION_ID_LEASE:
                    return "Lease";

                case FUNCTION_ID_RETURN:
                    return "Return";

                case FUNCTION_ID_TAG_LINK:
                    return "TagLink";

                case FUNCTION_ID_DISPOSE:
                    return "Dispose";

                case FUNCTION_ID_STORAGE:
                    return "Storage";

                case FUNCTION_ID_INVENTORY:
                    return "Inventory";

                case FUNCTION_ID_SEARCH:
                    return "Search";

                case FUNCTION_ID_SETTING:
                    return "Setting";

                case FUNCTION_ID_MATCHLEASE:
                    return "MatchLease";

                case FUNCTION_ID_MATCHIN:
                    return "MatchIn";

                case FUNCTION_ID_MATCHOUT:
                    return "MatchOut";

                case FUNCTION_ID_MATCHTEMP:
                    return "MatchTemp";

                case FUNCTION_ID_MATCHRETURN:
                    return "MatchReturn";

                default:
                    return "";
            }
        }

        // 機能定義
        public enum FUNCTION_ID{
            FUNCTION_ID_LEASE("Lease"),         //貸出
            FUNCTION_ID_RETURN("Return"),       //返却
            FUNCTION_ID_TAG_LINK("TagLink"),    //紐付
            FUNCTION_ID_DISPOSE("Dispose"),     //除却
            FUNCTION_ID_STORAGE("Storage"),     //保管
            FUNCTION_ID_INVENTORY("Inventory"), //棚卸
            FUNCTION_ID_SEARCH("Search"),       //探索
            FUNCTION_ID_SETTING("Setting"),     //設定
            FUNCTION_ID_MATCHLEASE("MatchLease"),     //貸出(照合)
            FUNCTION_ID_MATCHIN("MatchIn"),           //持込照合
            FUNCTION_ID_MATCHOUT("MatchOut"),         //持帰照合
            FUNCTION_ID_MATCHTEMP("MatchTemp"),       //仮置登録
            FUNCTION_ID_MATCHRETURN("MatchReturn");   //照合返却
            private final String text;
            FUNCTION_ID(final String text) {
                this.text = text;
            }
            public String getString() {
                return this.text;
            }
        }
    }

    /**
     * 権限
     */
    public static class Auth {
        //権限有無
        public static String readAuthFlg = "";      //参照権限
        public static String updAuthFlg = "";       //更新権限
        public static String readHubAuthFlg = "";   //参照権限(他拠点)
        public static String updHubAuthFlg = "";    //更新権限(他拠点)

        //参照権限有拠点
        public static String[] readHubList = null;
        //更新権限有拠点
        public static String[] updHubList = null;
    }
}
