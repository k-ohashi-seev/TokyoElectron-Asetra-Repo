package com.rfid.common.bean;

import java.io.Serializable;
import java.util.ArrayList;

/**
 * 照合処理履歴テーブルのデータ保持クラス
 */
public class EntryMatchProcessData implements Serializable {
    private ArrayList<EntryMatchProcessDataDetail> matchProcessDataList = new ArrayList<EntryMatchProcessDataDetail>();

    public void setprocessDataList(EntryMatchProcessDataDetail matchProcessDataList) {
        this.matchProcessDataList.add(matchProcessDataList);
    }

    public ArrayList<EntryMatchProcessDataDetail> getMatchProcessDataList() {
        return this.matchProcessDataList;
    }

    public int count() {
        return matchProcessDataList.size();
    }
}
