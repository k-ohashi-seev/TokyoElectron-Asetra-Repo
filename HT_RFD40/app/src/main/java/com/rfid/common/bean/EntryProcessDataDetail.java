package com.rfid.common.bean;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.rfid.common.WarningDisplay;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.stream.Collectors;

/**
 * 処理履歴テーブルのデータ保持クラス
 */
public class EntryProcessDataDetail implements Serializable {

    private String processLogId = "";           //処理履歴ID
    private String processDt  = "";             //処理日
    private String processTm  = "";             //処理時刻
    private String processToolDiv = "";         //処理ツール区分
    private String assetNo  = "";               //資産番号
    private String assetId  = "";               //資産ID
    private String assetName  = "";             //資産名称
    private String assetStatus = "";            //資産状態
    private int leaseCount = 0;                 //貸出回数              20200225 Add Sasaki V1.2
    private String leaseUpperErrDiv = "";      //貸出上限エラー区分    20200225 Add Sasaki V1.2
    private String leaseUpperLimit = "";       //貸出上限数            20200225 Add Sasaki V1.2
    private String processDiv  = "";           //処理区分
    private String leasePlaceCd  = "";         //貸出先場所コード
    private String leasePlaceName = "";       //貸出先場所名
    private String leaseEmployeeCd  = "";     //貸出先担当者社員コード
    private String leaseDueDt  = "";           //貸出返却予定日
    private String remark = "";                 //備考
    private String locLCd  = "";                //ロケーション大区分コード
    private String locMCd  = "";                //ロケーション中区分コード
    private String locSCd  = "";                //ロケーション小区分コード
    private String employeeCd  = "";           //ログイン担当者コード
    private String leaseLimit  = "";            //リース期限 20230830 ADD Kawamoto v2.4.1
    private String genericLimit  = "";          //○○期限 20230830 ADD Kawamoto v2.4.1
    private String leaseUpperAlert = "";        //貸出上限警告数 20230911 ADD okubo  V2.4.1
    private String ContMatchDiv = "";           //照合結果 20241030 ADD Shirakwa v4.0
    private List<WarningDisplay> warningDisplay = new ArrayList<>();         //警告文言 型修正 20231025 UPD sato v2.4.1

    /**
     * 処理履歴ID
     * @param processLogId
     */
    public void setProcessLogId(String processLogId) { this.processLogId = processLogId; }
    public String getProcessLogId() { return this.processLogId; }

    /**
     * 処理日
     * @param processDt
     */
    public void setProcessDt(String processDt) { this.processDt = processDt; }
    public String getProcessDt() { return this.processDt; }

    /**
     * 処理時刻
     * @param processTm
     */
    public void setProcessTm(String processTm) { this.processTm = processTm; }
    public String getProcessTm() { return this.processTm; }

    /**
     * 処理ツール区分
     * @param processToolDiv
     */
    public void setProcessToolDiv (String processToolDiv ) { this.processToolDiv  = processToolDiv ; }
    public String getProcessToolDiv() { return this.processToolDiv ; }

    /**
     * 資産番号
     * @param assetNo
     */
    public void setAssetNo(String assetNo) { this.assetNo = assetNo; }
    public String getAssetNo() { return this.assetNo; }

    /**
     * 資産ID
     * @param assetId
     */
    public void setAssetId(String assetId) { this.assetId = assetId; }
    public String getAssetId() { return this.assetId; }

    /**
     * 資産名称
     * @param assetName
     */
    @JsonIgnore
    public void setAssetName(String assetName) { this.assetName = assetName; }
    @JsonIgnore
    public String getAssetName() { return this.assetName; }

    /**
     * 資産状態
     * @param assetStatus
     */
    @JsonIgnore
    public void setAssetStatus(String assetStatus) { this.assetStatus = assetStatus; }
    @JsonIgnore
    public String getAssetStatus() { return this.assetStatus; }

    /**
     * 貸出回数     20200225 Add Sasaki V1.2
     * @param leaseCount
     */
    @JsonIgnore
    public void setLeaseCount(int leaseCount) { this.leaseCount = leaseCount; }
    @JsonIgnore
    public int getLeaseCount() { return this.leaseCount; }

    /**
     * 貸出上限エラー区分     20200225 Add Sasaki V1.2
     * @param leaseUpperErrDiv
     */
    @JsonIgnore
    public void setLeaseUpperErrDiv(String leaseUpperErrDiv) { this.leaseUpperErrDiv = leaseUpperErrDiv; }
    @JsonIgnore
    public String getLeaseUpperErrDiv() { return this.leaseUpperErrDiv; }

    /**
     * 貸出上限数     20200225 Add Sasaki V1.2
     * @param leaseUpperLimit
     */
    @JsonIgnore
    public void setLeaseUpperLimit(String leaseUpperLimit) { this.leaseUpperLimit = leaseUpperLimit; }
    @JsonIgnore
    public String getLeaseUpperLimit() { return this.leaseUpperLimit; }

    /**
     * 処理区分
     * @param processDiv
     */
    public void setProcessDiv(String processDiv) { this.processDiv = processDiv; }
    public String getProcessDiv() { return this.processDiv; }

    /**
     * 貸出先場所コード
     * @param leasePlaceCd
     */
    public void setLeasePlaceCd(String leasePlaceCd) { this.leasePlaceCd = leasePlaceCd; }
    public String getLeasePlaceCd() { return this.leasePlaceCd; }

    /**
     * 貸出先担当者社員コード
     * @param leaseEmployeeCd
     */
    public void setLeaseEmployeeCd(String leaseEmployeeCd) { this.leaseEmployeeCd = leaseEmployeeCd; }
    public String getLeaseEmployeeCd() { return this.leaseEmployeeCd; }

    /**
     * 貸出返却予定日
     * @param leaseDueDt
     */
    public void setLeaseDueDt(String leaseDueDt) { this.leaseDueDt = leaseDueDt; }
    public String getLeaseDueDt() { return this.leaseDueDt; }

    /**
     * 備考
     * @param remark
     */
    public void setRemark(String remark) {
        this.remark = remark;
    }
    public String getRemark() {
        return this.remark;
    }

    /**
     * ロケ大区分コード
     * @param locLCd
     */
    public void setLocLCd(String locLCd) { this.locLCd = locLCd; }
    public String getLocLCd() { return this.locLCd; }

    /**
     * ロケ中区分コード
     * @param locMCd
     */
    public void setLocMCd(String locMCd) { this.locMCd = locMCd; }
    public String getLocMCd() { return this.locMCd; }

    /**
     * ロケ小区分コード
     * @param locSCd
     */
    public void setLocSCd(String locSCd) { this.locSCd = locSCd; }
    public String getLocSCd() { return this.locSCd; }

    /**
     * ログイン担当者コード
     * @param employeeCd
     */
    public void setEmployeeCd(String employeeCd) { this.employeeCd = employeeCd; }
    public String getEmployeeCd() { return this.employeeCd; }

    /**
     * リース期限 20230830 ADD Kawamoto v2.4.1
     * @param leaseLimit
     */
    public void setLeaseLimit(String leaseLimit) { this.leaseLimit = leaseLimit; }
    public String getLeaseLimit() { return this.leaseLimit; }

    /**
     * ○○期限 20230830 ADD Kawamoto v2.4.1
     * @param genericLimit
     */
    public void setGenericLimit(String genericLimit) { this.genericLimit = genericLimit; }
    public String getGenericLimit() { return this.genericLimit; }

    /**
     * 貸出上限警告数 20230911 ADD okubo  V2.4.1
     * */
    public void SetLeaseUpperAlert(String Value) { this.leaseUpperAlert = Value; }
    public String getLeaseUpperAlert() { return this.leaseUpperAlert; }

    public void SetContMatchDiv(String Value) { this.ContMatchDiv = Value; }
    public String getContMatchDiv() { return this.ContMatchDiv; }

    /**
     * 警告文言 追加表示 20230907 ADD okubo v2.4.1
     * @param value
     * @remark 複数代入可能 カンマ区切りで連結
     * */
    public void setWarningDisplay(String value, int sortRank) {
        this.warningDisplay.add(new WarningDisplay(value, sortRank));
    }

    public String getWarningDisplay() {
        //RANKの昇順でソートして、メッセージのみを抽出
        List<String> warnMsgList = this.warningDisplay.stream()
                                    .sorted(Comparator.comparingInt(WarningDisplay::getSortRank))
                                    .map(WarningDisplay::getMessage)
                                    .collect(Collectors.toList());

        //警告文言を配列に変換
        String[] array = new String[warnMsgList.size()];
        warnMsgList.toArray(array);

        //カンマ区切りで返却
        return String.join(",", array);
    }
}
