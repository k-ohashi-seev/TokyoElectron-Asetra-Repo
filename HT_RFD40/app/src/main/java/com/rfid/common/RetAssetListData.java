package com.rfid.common;

/**
 * 返却リスト　データ保持クラス
 */
public class RetAssetListData {
    private String assetNo = "";            //資産番号
    private String contMatchDiv = "";            //照合結果

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
