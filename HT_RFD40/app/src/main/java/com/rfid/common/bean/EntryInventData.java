package com.rfid.common.bean;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Iterator;

public class EntryInventData implements Serializable {
    private ArrayList<EntryInventDataDetail> invDataDetailList = new ArrayList<EntryInventDataDetail>();
    private String ProcessToolDiv = "";

    public void setinvDataDetailList(EntryInventDataDetail ie) {
        this.invDataDetailList.add(ie);
    }

    public ArrayList<EntryInventDataDetail> getie() {
        return this.invDataDetailList;
    }

    public int count() {
        return invDataDetailList.size();
    }

    public Iterator<EntryInventDataDetail> iterator() {
        return invDataDetailList.iterator();
    }

    /* 処理ツール区分 */
    public void setProcessToolDiv(String ProcessToolDiv) { this.ProcessToolDiv = ProcessToolDiv; }
    public String gettd() {
        return this.ProcessToolDiv;
    }
}
