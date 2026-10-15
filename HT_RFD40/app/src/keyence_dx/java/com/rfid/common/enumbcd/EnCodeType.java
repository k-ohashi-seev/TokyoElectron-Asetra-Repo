package com.rfid.common.enumbcd;

public enum EnCodeType {
    UPC_EAN_JAN("UPC/EAN/JAN"),
    CODE_128("Code128"),
    CODE_39("Code39"),
    ITF("ITF"),
    GS1_DATA_BAR("GS1 DataBar"),
    DATA_MATRIX("Datamatrix"),
    QR_CODE("QRCode"),
    PDF_417("PDF417"),
    INDUSTRIAL_2OF5("Industrial 2of5"),
    CODABAR_NW7("Codabar(NW7)"),
    COOP_2OF5("COOP2of5"),
    CODE_93("Code93"),
    COMPOSITEAB_GS1_DATA_BAR("Composite AB(GS1-Databar)"),
    COMPOSITEAB_EAN_UPC("Composite AB(EAN/UPC)"),
    COMPOSITEAB_GS1_128("Composite (GS1-128)"),
    POSTAL("Postal"),
    OCR("Ocr");

    public final String codeType;
    EnCodeType(String codeType) {
        this.codeType = codeType;
    }
}
