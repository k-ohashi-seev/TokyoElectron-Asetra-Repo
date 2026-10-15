package com.rfid.common.bean;

import java.io.Serializable;
import java.util.ArrayList;

public class EntryLogData implements Serializable {
    private ArrayList<EntryLogDataDetail> logDataDetailList = new ArrayList<EntryLogDataDetail>();

    public void setlogDataDetailList(EntryLogDataDetail logDataDetailList) {
        this.logDataDetailList.add(logDataDetailList);
    }

    public ArrayList<EntryLogDataDetail> getlogDataDetailList() {
        return this.logDataDetailList;
    }

    public int count() {
        return logDataDetailList.size();
    }
}
