package com.rfid.common;

import com.densowave.bhtsdk.barcode.BarcodeManager;
import com.densowave.bhtsdk.barcode.BarcodeScanner;
import com.densowave.bhtsdk.barcode.BarcodeScannerInfo;
import com.densowave.bhtsdk.barcode.BarcodeScannerSettings;
import com.densowave.bhtsdk.barcode.DecodeSettings;
import com.densowave.bhtsdk.barcode.NotificationSettings;
import com.densowave.bhtsdk.barcode.ScanSettings;
import com.densowave.bhtsdk.barcode.Symbologies;

import java.nio.charset.Charset;

// バーコード制御共通クラス
public class CommonBarCode {

    //--- スキャナ設定プロパティ ---
    //トリガモード
    public ScanSettings.TriggerMode p_TriggerMode = ScanSettings.TriggerMode.AUTO_OFF;

    //照明モード
    public ScanSettings.LightMode p_LightMode = ScanSettings.LightMode.AUTO;

    //マーカモード
    public ScanSettings.MarkerMode p_MarkerMode = ScanSettings.MarkerMode.NORMAL;

    //読取通知音
    public boolean p_sound_enabled = true;

    //読取通知バイブレータ
    public boolean p_vibrate_enabled = false;

    //JAN-13(EAN-13), UPC-A 読取許可
    public boolean p_ean13UpcA_enabled = true;

    // JAN-8(EAN-8) 読取許可
    public boolean p_ean8_enabled = true;

    // UPC-E 読取許可
    public boolean p_upcE_enabled = true;

    // ITF 読取許可
    public boolean p_itf_enabled = true;

    // STF 読取許可
    public boolean p_stf_enabled = true;

    // Codabar 読取許可
    public boolean p_codabar_enabled = true;

    // Code39 読取許可
    public boolean p_code39_enabled = true;

    // Code93 読取許可
    public boolean p_code93_enabled = true;

    // Code128 読取許可
    public boolean p_code128_enabled = true;

    // GS1 Databar 読取許可
    public boolean p_gs1DataBar_enabled = true;

    // QR Code 読取許可
    public boolean p_qrCode_enabled = true;

    // Data Matrix 読取許可
    public boolean p_dataMatrix_enabled = true;

    // PDF417 読取許可
    public boolean p_pdf417_enabled = true;

    // Maxi 読取許可
    public boolean p_maxiCode_enabled = true;

    /**
     * スキャナ 設定
     * @param mBarcodeScanner スキャナオブジェクト
     * @param mScannerType  スキャナタイプ
     * */
    public void setScanner(BarcodeScanner mBarcodeScanner, BarcodeScannerInfo.BarcodeScannerType mScannerType) {
        // Scanner default settings

        BarcodeScannerSettings settings = mBarcodeScanner.getSettings();

        //トリガモード
        settings.scan.triggerMode = p_TriggerMode;

        //For 2D Module Settings
        if (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D) {
            // 照明モード
            settings.scan.lightMode =  p_LightMode;

            //マーカモード
            settings.scan.markerMode = p_MarkerMode;
        }

        //For 2D_LONG Module Settings
        if (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D_LONG) {
            settings.scan.sideLightMode = ScanSettings.SideLightMode.OFF;
            //settings.scan.sideLightMode = ScanSettings.SideLightMode.ON;
        }

        // 読取通知音
        settings.notification.sound.enabled = p_sound_enabled;
        //settings.notification.sound.enabled = false;

        if (settings.notification.sound.enabled) {
            settings.notification.sound.usageType = NotificationSettings.UsageType.RINGTONE;    //着信音
            //settings.notification.sound.usageType = NotificationSettings.UsageType.MEDIA;     //メディア
            //settings.notification.sound.usageType = NotificationSettings.UsageType.ALARM;     //アラーム

            if (settings.notification.sound.usageType == NotificationSettings.UsageType.MEDIA) {
                //TO BE Implement
                settings.notification.sound.goodDecodeFilePath = "";
            }
        }

        //読取通知バイブレータ
        settings.notification.vibrate.enabled = p_vibrate_enabled;
        //settings.notification.vibrate.enabled = false;

        // Decode Settings

        // Decode interval 二度読み防止解除時間(x100ms)
        settings.decode.sameBarcodeIntervalTime = 10; // 1,000msec

        // Decode Level デコードレベル
        settings.decode.decodeLevel = 4;              // Decode Level

        // Invert Mode 白黒反転読取
        settings.decode.invertMode = DecodeSettings.InvertMode.DISABLED;    //禁止
        //settings.decode.invertMode = DecodeSettings.InvertMode.INVERSION_ONLY;
        //settings.decode.invertMode = DecodeSettings.InvertMode.AUTO;

        // Point Scan Mode  ポイントスキャンモード
        settings.decode.pointScanMode = DecodeSettings.PointScanMode.DISABLED;  //ノーマル
        //settings.decode.pointScanMode = DecodeSettings.PointScanMode.ENABLED;

        // Reverse Mode     表裏反転読取
        settings.decode.reverseMode = DecodeSettings.ReverseMode.DISABLED;  //禁止
        //settings.decode.reverseMode = DecodeSettings.ReverseMode.ENABLED;

        // Encode Charset   バーコードのencoding 方式
        settings.decode.charset = Charset.forName("Shift-JIS");
        //settings.decode.charset = Charset.forName("UTF-8");

        // Symbology Settings

        // Multi Line   多段読取
        settings.decode.multiLineMode.enabled = false;  //禁止

        //JAN-13(EAN-13), UPC-A
        settings.decode.symbologies.ean13UpcA.enabled = p_ean13UpcA_enabled;
        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) ||
                (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D)) {
            settings.decode.symbologies.ean13UpcA.firstCharacter = "";
            settings.decode.symbologies.ean13UpcA.secondCharacter = "";
        }
        settings.editing.ean13.reportCheckDigit = true;
        settings.editing.upcA.reportCheckDigit = true;
        settings.editing.upcA.addLeadingZero = true;

        // EAN-13 add on
        settings.decode.symbologies.ean13UpcA.addOn.enabled = false;
        settings.decode.symbologies.ean13UpcA.addOn.onlyWithAddOn = false;
        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) ||
                (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D)) {
            settings.decode.symbologies.ean13UpcA.addOn.addOn2Digit = false;
            settings.decode.symbologies.ean13UpcA.addOn.addOn5Digit = false;
        }

        // JAN-8(EAN-8)
        settings.decode.symbologies.ean8.enabled = p_ean8_enabled;
        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) ||
                (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D)) {
            settings.decode.symbologies.ean8.firstCharacter = "";
            settings.decode.symbologies.ean8.secondCharacter = "";
        }
        settings.editing.ean8.reportCheckDigit = true;

        // EAN-8 add on
        settings.decode.symbologies.ean8.addOn.enabled = false;
        settings.decode.symbologies.ean8.addOn.onlyWithAddOn = false;
        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) ||
                (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D)) {
            settings.decode.symbologies.ean8.addOn.addOn2Digit = false;
            settings.decode.symbologies.ean8.addOn.addOn5Digit = false;
        }

        // UPC-E
        settings.decode.symbologies.upcE.enabled = p_upcE_enabled;
        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) ||
                (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D)) {
            settings.decode.symbologies.upcE.firstCharacter = "";
            settings.decode.symbologies.upcE.secondCharacter = "";
        }
        settings.editing.upcE.reportCheckDigit = true;
        settings.editing.upcE.addLeadingZero = false;
        settings.editing.upcE.convertToUpcA = false;
        settings.editing.upcE.reportNumberSystemCharacterOfConvertedUpcA = true;

        // UPC-E add on
        settings.decode.symbologies.upcE.addOn.enabled = false;
        settings.decode.symbologies.upcE.addOn.onlyWithAddOn = false;
        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) ||
                (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D)) {
            settings.decode.symbologies.upcE.addOn.addOn2Digit = false;
            settings.decode.symbologies.upcE.addOn.addOn5Digit = false;
        }

        // ITF
        settings.decode.symbologies.itf.enabled = p_itf_enabled;
        settings.decode.symbologies.itf.lengthMin = 4;
        settings.decode.symbologies.itf.lengthMax = 99;
        settings.decode.symbologies.itf.verifyCheckDigit = false;
        //settings.decode.symbologies.itf.verifyCheckDigit = true;
        settings.editing.itf.reportCheckDigit = true;

        // STF
        settings.decode.symbologies.stf.enabled = p_stf_enabled;
        settings.decode.symbologies.stf.lengthMin = 4;
        settings.decode.symbologies.stf.lengthMax = 99;
        settings.decode.symbologies.stf.verifyCheckDigit = false;
        //settings.decode.symbologies.stf.verifyCheckDigit = true;
        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) ||
                (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D_LONG)) {
            settings.decode.symbologies.stf.startStopCharacter = "";
            //settings.decode.symbologies.stf.startStopCharacter = "S";
            //settings.decode.symbologies.stf.startStopCharacter = "N";
        }
        settings.editing.stf.reportCheckDigit = true;

        // Codabar
        settings.decode.symbologies.codabar.enabled = p_codabar_enabled;
        settings.decode.symbologies.codabar.lengthMin = 4;
        settings.decode.symbologies.codabar.lengthMax = 99;
        settings.decode.symbologies.codabar.verifyCheckDigit = false;
        //settings.decode.symbologies.codabar.verifyCheckDigit = true;
        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) ||
                (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D)) {
            settings.decode.symbologies.codabar.startStopCharacter = "";
        }
        settings.editing.codabar.reportCheckDigit = true;
        settings.editing.codabar.reportStartStopCharacter = true;
        settings.editing.codabar.convertToUpperCase = false;

        // Code39
        settings.decode.symbologies.code39.enabled = p_code39_enabled;
        settings.decode.symbologies.code39.lengthMin = 1;
        settings.decode.symbologies.code39.lengthMax = 99;
        settings.decode.symbologies.code39.verifyCheckDigit = false;
        //settings.decode.symbologies.code39.verifyCheckDigit = true;
        settings.editing.code39.reportCheckDigit = true;
        settings.editing.code39.reportStartStopCharacter = false;

        // Code93
        settings.decode.symbologies.code93.enabled = p_code93_enabled;
        settings.decode.symbologies.code93.lengthMin = 1;
        settings.decode.symbologies.code93.lengthMax = 99;

        // Code128
        settings.decode.symbologies.code128.enabled = p_code128_enabled;
        settings.decode.symbologies.code128.lengthMin = 1;
        settings.decode.symbologies.code128.lengthMax = 99;

        // MSI
        if (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) {
            settings.decode.symbologies.msi.enabled = false; //true;
            settings.decode.symbologies.msi.lengthMin = 1;
            settings.decode.symbologies.msi.lengthMax = 99;
            settings.decode.symbologies.msi.numberOfCheckDigitVerification = 1;
            //settings.decode.symbologies.msi.numberOfCheckDigitVerification = 2;
        }

        // GS1 Databar
        settings.decode.symbologies.gs1DataBar.enabled = p_gs1DataBar_enabled;
        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) ||
                (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D)) {
            settings.decode.symbologies.gs1DataBar.stacked = false;
        }

        // Gs1 Databar Limited
        settings.decode.symbologies.gs1DataBarLimited.enabled = false;

        // Gs1 Databar Expanded
        settings.decode.symbologies.gs1DataBarExpanded.enabled = false;
        settings.decode.symbologies.gs1DataBarExpanded.lengthMin = 1;
        settings.decode.symbologies.gs1DataBarExpanded.lengthMax = 99;
        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_1D) ||
                (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D)) {
            settings.decode.symbologies.gs1DataBarExpanded.stacked = false;
        }

        // Gs1 Composite
        settings.decode.symbologies.gs1Composite.enabled = false;

        if ((mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D) ||
            (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D_LONG)) {

            // QR Code
            settings.decode.symbologies.qrCode.enabled = p_qrCode_enabled;

            if (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D) {
                settings.decode.symbologies.qrCode.splitMode = Symbologies.SplitModeQr.DISABLED;    //連結読取モード 禁止
                //settings.decode.symbologies.qrCode.splitMode = Symbologies.SplitModeQr.EDIT;
                //settings.decode.symbologies.qrCode.splitMode = Symbologies.SplitModeQr.BATCH_EDIT;
                //settings.decode.symbologies.qrCode.splitMode = Symbologies.SplitModeQr.NON_EDIT;

                // QR Code Model1
                settings.decode.symbologies.qrCode.model1.enabled = p_qrCode_enabled;
                settings.decode.symbologies.qrCode.model1.versionMin = 1;
                settings.decode.symbologies.qrCode.model1.versionMax = 22;

                // QR Code Model2
                settings.decode.symbologies.qrCode.model2.enabled = p_qrCode_enabled;
                settings.decode.symbologies.qrCode.model2.versionMin = 1;
                settings.decode.symbologies.qrCode.model2.versionMax = 40;

                // Micro QR Code
                settings.decode.symbologies.microQr.enabled = p_qrCode_enabled;
                settings.decode.symbologies.microQr.versionMin = 1;
                settings.decode.symbologies.microQr.versionMax = 4;

                // iQR Code
                settings.decode.symbologies.iqrCode.enabled = p_qrCode_enabled;
                settings.decode.symbologies.iqrCode.splitMode = Symbologies.SplitModeIqr.DISABLED;
                //settings.decode.symbologies.iqrCode.splitMode = Symbologies.SplitModeIqr.EDIT;
                //settings.decode.symbologies.iqrCode.splitMode = Symbologies.SplitModeIqr.NON_EDIT;

                // Square iQR Code
                settings.decode.symbologies.iqrCode.square.enabled = p_qrCode_enabled;
                settings.decode.symbologies.iqrCode.square.versionMin = 1;
                settings.decode.symbologies.iqrCode.square.versionMax = 61;

                // Rectangle iQR Code
                settings.decode.symbologies.iqrCode.rectangle.enabled = p_qrCode_enabled;
                settings.decode.symbologies.iqrCode.rectangle.versionMin = 1;
                settings.decode.symbologies.iqrCode.rectangle.versionMax = 15;

            } else {
                //For 2D Long model
                settings.decode.symbologies.microQr.enabled = false;
                settings.decode.symbologies.iqrCode.enabled = false;
            }

            // Data Matrix
            settings.decode.symbologies.dataMatrix.enabled = p_dataMatrix_enabled;

            if (mScannerType == BarcodeScannerInfo.BarcodeScannerType.TYPE_2D) {
                // DataMatrix Square
                settings.decode.symbologies.dataMatrix.square.enabled = p_dataMatrix_enabled;
                settings.decode.symbologies.dataMatrix.square.codeNumberMin = 1;
                settings.decode.symbologies.dataMatrix.square.codeNumberMax = 24;

                // DataMatrix ReactAngle
                settings.decode.symbologies.dataMatrix.rectangle.enabled = p_dataMatrix_enabled;
                settings.decode.symbologies.dataMatrix.rectangle.codeNumberMin = 1;
                settings.decode.symbologies.dataMatrix.rectangle.codeNumberMax = 6;
            }

            // PDF417
            settings.decode.symbologies.pdf417.enabled = p_pdf417_enabled;

            // Micro PDF 417
            settings.decode.symbologies.microPdf417.enabled = p_pdf417_enabled;

            // Maxi
            settings.decode.symbologies.maxiCode.enabled = p_maxiCode_enabled;
        } else {
            //For 1D model
            settings.decode.symbologies.qrCode.enabled = false;
            settings.decode.symbologies.microQr.enabled = false;
            settings.decode.symbologies.iqrCode.enabled = false;
            settings.decode.symbologies.pdf417.enabled = false;
            settings.decode.symbologies.microPdf417.enabled = false;
            settings.decode.symbologies.maxiCode.enabled = false;
            settings.decode.symbologies.dataMatrix.enabled = false;
        }

        mBarcodeScanner.setSettings(settings);
    }

}
