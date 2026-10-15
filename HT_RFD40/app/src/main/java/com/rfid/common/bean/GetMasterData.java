package com.rfid.common.bean;

import java.io.Serializable;
import java.util.ArrayList;

public class GetMasterData implements Serializable {
    private ArrayList<GetMasterDataDetail> masterDataList = new ArrayList<GetMasterDataDetail>();
    private String employeeCd = "";    //20230206 ADD Kawamoto V2.3

    public void setmasterDataList(GetMasterDataDetail invDataDetailList) {
        this.masterDataList.add(invDataDetailList);
    }

    public ArrayList<GetMasterDataDetail> getmasterDataList() {
        return this.masterDataList;
    }

    public int count() {
        return masterDataList.size();
    }

    //20230206 ADD Kawamoto V2.3 START
    /* 資産取得ロケコード */
    public void setEmployeeCd(String EmployeeCode) { this.employeeCd = EmployeeCode; }
    public String getEmployeeCd() {
        return this.employeeCd;
    }
    //20230206 ADD Kawamoto V2.3 END
}
