package com.rfid.common;

/**
 * 貸出リスト　データ保持クラス
 */
public class LentAssetListData {
    private String assetNo = "";            //資産番号
    private String leaseDt = "";            //持出日
    private String remark = "";             //備考

    /**
     * 資産番号
     * @param assetNo
     */
    public void setAssetNo(String assetNo) { this.assetNo = assetNo; }
    public String getAssetNo() { return this.assetNo; }

    /**
     * 持出日
     * @param leaseDt
     */
    public void setLeaseDt(String leaseDt) { this.leaseDt = leaseDt; }
    public String getLeaseDt() { return this.leaseDt; }

    /**
     * 備考
     * @param remark
     */
    public void setRemark(String remark) { this.remark = remark; }
    public String getRemark() { return this.remark; }
}
