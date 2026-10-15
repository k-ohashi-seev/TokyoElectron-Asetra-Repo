package com.rfid.common;

public class WarningDisplay {
    private String message = "";
    private int sortRank;

    public WarningDisplay(String message, int sortRank) {
        this.message = message;
        this.sortRank = sortRank;
    }

    public String getMessage() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }

    public int getSortRank() {
        return sortRank;
    }

    public void setSortRank(int sortRank) {
        this.sortRank = sortRank;
    }
}
