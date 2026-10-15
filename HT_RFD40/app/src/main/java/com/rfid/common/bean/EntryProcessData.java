package com.rfid.common.bean;

import java.io.Serializable;
import java.util.ArrayList;

/**
 * 処理履歴テーブルのデータ保持クラス
 */
public class EntryProcessData implements Serializable {
    private ArrayList<EntryProcessDataDetail> processDataList = new ArrayList<EntryProcessDataDetail>();

    public void setprocessDataList(EntryProcessDataDetail processDataList) {
        this.processDataList.add(processDataList);
    }

    public ArrayList<EntryProcessDataDetail> getprocessDataList() {
        return this.processDataList;
    }

    public int count() {
        return processDataList.size();
    }
}
