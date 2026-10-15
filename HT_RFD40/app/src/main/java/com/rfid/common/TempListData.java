package com.rfid.common;

import java.util.ArrayList;
import java.util.List;

/**
 * 仮置きリスト　データ保持クラス
 */
public class TempListData {
    private String tempId = "";                                             //仮置きID
    private String tempDt = "";                                             //持出日
    private String tempEmpCd = "";                                          //作業責任者コード
    private String tempEmpName = "";                                        //作業責任者名
    private String tempInfo1 = "";                                          //仮置き情報1
    private String tempInfo2 = "";                                          //仮置き情報2
    private String tempInfo3 = "";                                          //仮置き情報3
    private String tempInfo4 = "";                                          //仮置き情報4
    private String tempInfo5 = "";                                          //仮置き情報5
    private String tempInfo6 = "";                                          //仮置き情報6
    private String tempInfo7 = "";                                          //仮置き情報7
    private String tempRemark = "";                                         //備考
    private List<TempAssetListData> tempAssetList = new ArrayList<>();      //仮置き資産リスト

    /**
     * 仮置きID
     * @param tempId
     */
    public void setTempId(String tempId) { this.tempId = tempId; }
    public String getTempId() { return this.tempId; }

    /**
     * 仮置き日
     * @param tempDt
     */
    public void setTempDt(String tempDt) { this.tempDt = tempDt; }
    public String getTempDt() { return this.tempDt; }

    /**
     * 作業責任者コード
     * @param tempEmpCd
     */
    public void setTempEmpCd(String tempEmpCd) { this.tempEmpCd = tempEmpCd; }
    public String getTempEmpCd() { return this.tempEmpCd; }

    /**
     * 作業責任者名
     * @param tempEmpName
     */
    public void setTempEmpName(String tempEmpName) { this.tempEmpName = tempEmpName; }
    public String getTempEmpName() { return this.tempEmpName; }

    /**
     * 仮置き情報1
     * @param tempInfo1
     */
    public void setTempInfo1(String tempInfo1) { this.tempInfo1 = tempInfo1; }
    public String getTempInfo1() { return this.tempInfo1; }

    /**
     * 仮置き情報2
     * @param tempInfo2
     */
    public void setTempInfo2(String tempInfo2) { this.tempInfo2 = tempInfo2; }
    public String getTempInfo2() { return this.tempInfo2; }

    /**
     * 仮置き情報3
     * @param tempInfo3
     */
    public void setTempInfo3(String tempInfo3) { this.tempInfo3 = tempInfo3; }
    public String getTempInfo3() { return this.tempInfo3; }

    /**
     * 仮置き情報4
     * @param tempInfo4
     */
    public void setTempInfo4(String tempInfo4) { this.tempInfo4 = tempInfo4; }
    public String getTempInfo4() { return this.tempInfo4; }


    /**
     * 仮置き情報5
     * @param tempInfo5
     */
    public void setTempInfo5(String tempInfo5) { this.tempInfo5 = tempInfo5; }
    public String getTempInfo5() { return this.tempInfo5; }

    /**
     * 仮置き情報6
     * @param tempInfo6
     */
    public void setTempInfo6(String tempInfo6) { this.tempInfo6 = tempInfo6; }
    public String getTempInfo6() { return this.tempInfo6; }

    /**
     * 仮置き情報7
     * @param tempInfo7
     */
    public void setTempInfo7(String tempInfo7) { this.tempInfo7 = tempInfo7; }
    public String getTempInfo7() { return this.tempInfo7; }

    /**
     * 備考
     * @param tempRemark
     */
    public void setTempRemark(String tempRemark) { this.tempRemark = tempRemark; }
    public String getTempRemark() { return this.tempRemark; }

    /**
     * 仮置き資産リスト
     * @param tempAssetList
     * */
    public void setTempAssetList(List<TempAssetListData> tempAssetList) {
        this.tempAssetList = tempAssetList;
    }
    public List<TempAssetListData> getTempAssetList() {
        return tempAssetList;
    }

}
