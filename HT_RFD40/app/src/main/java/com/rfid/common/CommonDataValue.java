package com.rfid.common;

// データ定義共通クラス
public class CommonDataValue {

    // ステータス値定義
    public static final String STATUS_VAL_NOT_IC = "0";          //IC未発行
    public static final String STATUS_VAL_ZAIKO  = "1";          //在庫
    public static final String STATUS_VAL_MOTIDASHI  = "2";     //持出中
    public static final String STATUS_VAL_KANSAI  = "3";         //完済
    public static final String STATUS_VAL_TANA_NOT_SET  = "4";  //棚未設定

    // ステータス名称を返す
    public String getStatusName(String Value) {
        switch (Value) {
            case STATUS_VAL_NOT_IC:
                return "IC未発行";

            case STATUS_VAL_ZAIKO:
                return "在庫";

            case STATUS_VAL_MOTIDASHI:
                return "持出中";

            case STATUS_VAL_KANSAI:
                return "完済";

            case STATUS_VAL_TANA_NOT_SET:
                return "棚未設定";

            default:
                return "未定義";
        }
    }

    //状態変更処理フラグ定義
    public static final String SHORI_FLG_OUTGO = "1";       //持出
    public static final String SHORI_FLG_RETURN = "2";      //返却
    public static final String SHORI_FLG_PAYMENT = "3";     //完済

    //20210918 ADD Takase V2.0 START
    // 機能定義  20200825
    public enum FUNCTION_ID {
        FUNCTION_ID_LEASE,     //貸出
        FUNCTION_ID_RETURN,    //返却
        FUNCTION_ID_TAG_LINK,  //紐付
        FUNCTION_ID_DISPOSE,   //除却
        FUNCTION_ID_STORAGE,   //保管
        FUNCTION_ID_INVENTORY, //棚卸
        FUNCTION_ID_SEARCH,    //探索
        FUNCTION_ID_SETTING,   //設定
    }

    // 機能IDを返す
    public String getFunctionId(FUNCTION_ID Value) {
        switch (Value) {
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

            default:
                return "";
        }
    }
    //20210918 ADD Takase V2.0 END
}
