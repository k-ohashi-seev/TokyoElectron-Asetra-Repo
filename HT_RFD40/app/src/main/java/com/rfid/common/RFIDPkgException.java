package com.rfid.common;

public class RFIDPkgException extends Exception{
    //warningを回避するための宣言
    private static final long serialVersionUID = 1L;

    // コンストラクタ
    public RFIDPkgException() { }
    public RFIDPkgException(String message) { super(message); }
    public RFIDPkgException(Throwable cause) { super(cause); }
    public RFIDPkgException(String message, Throwable cause) {
        super(message, cause);
    }
}
