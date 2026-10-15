package com.rfid.common.bean;

import com.fasterxml.jackson.annotation.JsonIgnore;

import java.io.Serializable;

public class EntryInventDataDetail implements Serializable {

    private String AssetNo = "";
    private String AssetName = "";
    private String phyinvTs = "";
    private String phyinvStatus = "";
    private String phyinvLocLCd  = "";
    private String phyinvLocMCd  = "";
    private String phyinvLocSCd  = "";
    private String phyinvEmployeeCd = "";
    private String SubAssetNo = "";

    /* 資産番号 */
    public void setAssetNo(String AssetNo) { this.AssetNo = AssetNo; }
    public String getan() {
        return this.AssetNo;
    }

    /* 資産名称 */
    @JsonIgnore
    public void setAssetName(String AssetName) { this.AssetName = AssetName; }
    @JsonIgnore
    public String getAssetName() {
        return this.AssetName;
    }

    /* 実棚日時 */
    public void setphyinvTs(String phyinvTs) { this.phyinvTs = phyinvTs; }
    public String getpt() {
        return this.phyinvTs;
    }

    /* 実棚ステータス */
    public void setphyinvStatus(String phyinvStatus) { this.phyinvStatus = phyinvStatus; }
    public String getps() {
        return this.phyinvStatus;
    }

    /* 実棚ロケーション大区分コード */
    public void setphyinvLocLCd(String phyinvLocLCd) {
        this.phyinvLocLCd = phyinvLocLCd;
    }
    public String getlc() {
        return this.phyinvLocLCd;
    }

    /* 実棚ロケーション中区分コード */
    public void setPhyinvLocMCd(String phyinvLocMCd) {
        this.phyinvLocMCd = phyinvLocMCd;
    }
    public String getmc() {
        return this.phyinvLocMCd;
    }

    /* 実棚ロケーション小区分コード */
    public void setphyinvLocSCd(String phyinvLocSCd) {
        this.phyinvLocSCd = phyinvLocSCd;
    }
    public String getsc() {
        return this.phyinvLocSCd;
    }

    /* 実棚実施者社員コード */
    public void setphyinvEmployeeCd(String phyinvEmployeeCd) { this.phyinvEmployeeCd = phyinvEmployeeCd; }
    public String getec() {
        return this.phyinvEmployeeCd;
    }

    /* サブ資産番号 */
    public void setSubAssetNo(String SubAssetNo) { this.SubAssetNo = SubAssetNo; }
    public String getSubAssetNo() {
        return this.SubAssetNo;
    }

}
