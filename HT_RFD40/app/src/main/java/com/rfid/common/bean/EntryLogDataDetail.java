package com.rfid.common.bean;

import com.fasterxml.jackson.annotation.JsonIgnore;

import java.io.Serializable;

/**
 * 操作ログ用データクラス
 */
public class EntryLogDataDetail implements Serializable {

    private String opeLogId = "";               //操作ログID
    private String processDt  = "";             //処理日
    private String processTm  = "";             //処理時刻
    private String processToolDiv = "";         //処理ツール区分
    private String processCat  = "";            //処理種別
    private String processContents  = "";      //処理内容
    private String opeEmployeeCd  = "";        //操作者社員コード
    private String logKind = "";                //ログ種別

    /**
     * 処理履歴ID
     * @param opeLogId
     */
    public void setOpeLogId(String opeLogId) { this.opeLogId = opeLogId; }
    public String getOpeLogId() { return this.opeLogId; }

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
     * 処理種別
     * @param processCat
     */
    public void setProcessCat(String processCat) { this.processCat = processCat; }
    public String getProcessCat() { return this.processCat; }

    /**
     * 処理内容
     * @param processContents
     */
    public void setProcessContent(String processContents) { this.processContents = processContents; }
    public String getProcessContents() { return this.processContents; }

    /**
     * 操作者社員コード
     * @param opeEmployeeCd
     */
    public void setOpeEmployeeCd(String opeEmployeeCd) { this.opeEmployeeCd = opeEmployeeCd; }
    public String getOpeEmployeeCd() { return this.opeEmployeeCd; }

    /**
     * ログ種別
     * @param logKind
     */
    @JsonIgnore
    public void setLogKind(String logKind) { this.logKind = logKind; }
    @JsonIgnore
    public String getLogKind() { return this.logKind; }

}
