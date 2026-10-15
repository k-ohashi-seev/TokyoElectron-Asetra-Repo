package com.rfid.common;

/**
 * 照合資産リスト　データ保持クラス
 */
public class MatchAssetListData {
    private String assetNo = "";            //資産番号
    private String contMatchDiv = "";       //照合結果
    /**
     * 資産番号
     * @param assetNo
     */
    public void setAssetNo(String assetNo) { this.assetNo = assetNo; }
    public String getAssetNo() { return this.assetNo; }

    /**
     * 照合結果
     * @param contMatchDiv
     */
    public void setContMatchDiv(String contMatchDiv) { this.contMatchDiv = contMatchDiv; }
    public String getContMatchDiv() { return this.contMatchDiv; }

}
