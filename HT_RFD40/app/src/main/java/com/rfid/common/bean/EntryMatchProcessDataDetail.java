package com.rfid.common.bean;

import com.rfid.common.LentAssetListData;
import com.rfid.common.MatchAssetListData;
import com.rfid.common.RetAssetListData;
import com.rfid.common.TempAssetListData;
import com.rfid.common.TempListData;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;

/**
 * 照合処理履歴テーブルのデータ保持クラス
 */
public class EntryMatchProcessDataDetail implements Serializable {

    private String processLogId = "";                                       //処理履歴ID
    private String processDt  = "";                                         //処理日
    private String processTm  = "";                                         //処理時刻
    private String employeeCd  = "";                                        //ログイン担当者コード
    private String processDiv  = "";                                        //処理区分
    private String constNo  = "";                                           //工事番号
    private String leaseEmployeeCd = "";    //貸出先担当者社員コード
    private String leaseDueDt = "";         //貸出返却予定日
    private String contMatchDiv = "";                                       //照合結果
    private String contMatchNum = "";                                       //照合数
    private List<LentAssetListData> lentAssetList = new ArrayList<>();      //貸出リスト
    private List<RetAssetListData> retAssetList = new ArrayList<>();        //返却リスト
    private List<MatchAssetListData> matchAssetList = new ArrayList<>();    //照合資産リスト
    private List<TempListData> tempList = new ArrayList<>();                //仮置きリスト

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
     * ログイン担当者コード
     * @param employeeCd
     */
    public void setEmployeeCd(String employeeCd) { this.employeeCd = employeeCd; }
    public String getEmployeeCd() { return this.employeeCd; }

    /**
     * 処理区分
     * @param processDiv
     */
    public void setProcessDiv(String processDiv) { this.processDiv = processDiv; }
    public String getProcessDiv() { return this.processDiv; }

    /**
     * 工事番号
     * @param constNo
     */
    public void setConstNo(String constNo) { this.constNo = constNo; }
    public String getConstNo() { return this.constNo; }

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
     * 照合結果
     * @param contMatchDiv
     */
    public void SetContMatchDiv(String contMatchDiv) { this.contMatchDiv = contMatchDiv; }
    public String getContMatchDiv() { return this.contMatchDiv; }

    /**
     * 照合数
     * @param contMatchNum
     */
    public void SetContMatchNum(String contMatchNum) { this.contMatchNum = contMatchNum; }
    public String getContMatchNum() { return this.contMatchNum; }

    /**
     * 貸出リスト
     * @param lentAssetList
     * */
    public void setLentAssetList(List<LentAssetListData> lentAssetList) {
        this.lentAssetList = lentAssetList;
    }
    public List<LentAssetListData> getLentAssetList() {
        return lentAssetList;
    }

    /**
     * 返却リスト
     * @param retAssetList
     * */
    public void setRetAssetList(List<RetAssetListData> retAssetList) {
        this.retAssetList = retAssetList;
    }
    public List<RetAssetListData> getRetAssetList() {
        return retAssetList;
    }

    /**
     * 照合資産リスト
     * @param matchAssetList
     * */
    public void setMatchAssetList(List<MatchAssetListData> matchAssetList) {
        this.matchAssetList = matchAssetList;
    }
    public List<MatchAssetListData> getMatchAssetList() {
        return matchAssetList;
    }

    /**
     * 仮置きリスト
     * @param tempList
     * */
    public void setTempList(List<TempListData> tempList) {
        this.tempList = tempList;
    }
    public List<TempListData> getTempList() {
        return tempList;
    }

}
