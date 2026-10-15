package com.rfid.common.bean;

import com.fasterxml.jackson.annotation.JsonIgnore;

import java.io.Serializable;

public class GetMasterDataDetail implements Serializable {

    private String masterName = "";

    /**
     * マスター名
     * @param masterName
     */
    public void setMasterName(String masterName) { this.masterName = masterName; }
    public String getMasterName() { return this.masterName; }

}
