package com.rfid.common;

import java.io.Serializable;

/**
 * 資産モデルクラス
 */
public class AssetModel implements Serializable {

    private String _AssetNo = "";
    private String _AssetID = "";
    private String _AssetName = "";
    private String _AssetStatus = "";           //20200305 Add Sasaki V1.2
    private String _PhyinvFlag = "";
    private String _PhyinvStatus = "";
    private String _LocLCd = "";
    private String _LocMCd = "";
    private String _LocSCd = "";
    private String _AssetIdRegDiv = "";
    private String _EpcSize = "";
    private String _MngLocLCd = "";              //20210918 ADD Takase V2.0
    private String _LocShareFlg = "";            //20210918 ADD Takase V2.0
    private String _LeaseStatus = "";            //20210918 ADD Takase V2.0
    private String _SubAssetNo = "";
    private String _IsTangible = "";
    private String _InvFlg = "";

    public AssetModel() {
        this._AssetNo = "";
    }

    public AssetModel(String AssetNo) {
        this._AssetNo = AssetNo;
    }

    public void setAssetNo(String AssetNo) { this._AssetNo = AssetNo; }

    public String getAssetNo() {
        return this._AssetNo;
    }

    public void setAssetID(String AssetID) { this._AssetID = AssetID; }

    public String getAssetID() {
        return this._AssetID;
    }

    public void setAssetName(String AssetName) {
        this._AssetName = AssetName;
    }

    public String getAssetName() {
        return this._AssetName;
    }

    //20200305 Add Start Sasaki V1.2
    public void setAssetStatus(String AssetStatus) {
        this._AssetStatus = AssetStatus;
    }
    public String getAssetStatus() {
        return this._AssetStatus;
    }
    //20200305 Add End Sasaki V1.2

    public void setLocLCd(String LocLCd) {
        this._LocLCd = LocLCd;
    }

    public String gettLocLCd() {
        return this._LocLCd;
    }

    public void setLocMCd(String LocMCd) {
        this._LocMCd = LocMCd;
    }

    public String gettLocMCd() {
        return this._LocMCd;
    }

    public void setLocSCd(String LocSCd) {
        this._LocSCd = LocSCd;
    }

    public String gettLocSCd() {
        return this._LocSCd;
    }

    public void setAssetIdRegDiv(String AssetIdRegDiv) {
        this._AssetIdRegDiv = AssetIdRegDiv;
    }

    public String gettAssetIdRegDiv() {
        return this._AssetIdRegDiv;
    }

//20191210 Upd Start Sasaki
    public void setPhyinvFlag(String PhyinvFlag) {
        this._PhyinvFlag = PhyinvFlag;
    }

    public String getPhyinvFlag() {
        return this._PhyinvFlag;
    }
//20191210 Upd End Sasaki

    public void setPhyinvStatus(String PhyinvStatus) {
        this._PhyinvStatus = PhyinvStatus;
    }

    public String getPhyinvStatus() {
        return this._PhyinvStatus;
    }

    public void setEpcSize(String strEpcSize) {this._EpcSize = strEpcSize; }

    public String getEpcSize() {return this._EpcSize;}

    //20210918 ADD Takase V2.0 START
    public void setMngLocLCd(String MngLocLCd) {this._MngLocLCd = MngLocLCd; }

    public String getMngLocLCd() {return this._MngLocLCd; }

    public void setLocShareFlg(String LocShareFlg) {this._LocShareFlg = LocShareFlg; }

    public String getLocShareFlg() {return this._LocShareFlg; }

    public void setLeaseStatus(String LeaseStatus) {this._LeaseStatus = LeaseStatus; }

    public String getLeaseStatus() {return this._LeaseStatus; }
    //20210918 ADD Takase V2.0 END

    public void setSubAssetNo(String SubAssetNo) {this._SubAssetNo = SubAssetNo; }

    public String getSubAssetNo() {return this._SubAssetNo; }

    public void setIsTangible(String IsTangible) {this._IsTangible = IsTangible; }

    public String getIsTangible() {return this._IsTangible; }

    public void setInvFlg(String InvFlg) {this._InvFlg = InvFlg; }

    public String getInvFlg() {return this._InvFlg; }
}
