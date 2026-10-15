package com.rfid.common;

import static com.rfid.common.CommonConst.PC_LENGTH_BIT_SIZE;

import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;

import com.rfid.setting.SettingValue;

import java.io.PrintWriter;
import java.io.StringWriter;
import java.io.UnsupportedEncodingException;
import java.net.InetAddress;
import java.net.NetworkInterface;
import java.nio.ByteBuffer;
import java.nio.CharBuffer;
import java.nio.charset.Charset;
import java.nio.charset.CharsetEncoder;
import java.nio.charset.CoderResult;
import java.nio.charset.CodingErrorAction;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Enumeration;

public class Common {

    /**
     * 接続機器名
     */
    public static String ConnectRFIDName = CommonConst.CONNECT_RFIDNAME.ZEBRA.getString();

    /**
     *  現在時刻取得
      */
    public static String getNowTime(String strFormat)  {
        try {
            // 現在の時刻を取得
            Date date = new Date();
            // 表示形式を設定
            SimpleDateFormat DtFormat = new SimpleDateFormat(strFormat);
            return DtFormat.format(date);
        } catch (Exception ex) {
            return "";
        }
    }

    /**
     * 引数に指定したバイト数でカットする(文字コードMS932固定）
     * @param strData
     * @param maxBytes
     * @return
     */
    public static String substring(String strData, int maxBytes) {

        //文字コード
        Charset charset = Charset.forName("MS932");
        return substring(strData, maxBytes, charset);

    }

    /**
     * 引数に指定したバイト数でカットする
     * @param strData
     * @param maxBytes
     * @return
     */
    public static String substring(String strData, int maxBytes, Charset charset) {
        String strReturn = strData;

        try {
            if (strData == null) {
                return strData;
            }

            //バイト変換する
            ByteBuffer bb = ByteBuffer.allocate(maxBytes);
            CharBuffer cb = CharBuffer.wrap(strData);
            CharsetEncoder encoder = charset.newEncoder()
                    .onMalformedInput(CodingErrorAction.REPLACE)
                    .onUnmappableCharacter(CodingErrorAction.REPLACE)
                    .reset();
            CoderResult cr = encoder.encode(cb, bb, true);
            if (!cr.isOverflow()) {
                return strReturn;
            } else {
                encoder.flush(bb);
                strReturn = cb.flip().toString();
            }
        } catch (Exception ex) {
            ex.printStackTrace();
        } finally {
            return strReturn;
        }
    }

    /**
     * スタックトレースを文字列に変換する
     * @param e
     * @return
     */
    public static String toString(Exception e){
        try {
            StringWriter sw = new StringWriter();
            PrintWriter pw = new PrintWriter(sw);
            e.printStackTrace(pw);
            pw.flush();
            String strReturn = sw.toString();
            strReturn = strReturn.replaceAll("\t", "");
            strReturn = strReturn.replaceAll("\n", ",");
            return strReturn;
        } catch (Exception ex) {
            ex.printStackTrace();
            return "";
        }
    }

    /**
     * 文字列を数値に変換します。
     * 数値に変換できない場合、0を返却します。
     *
     * @param value 文字列
     * @return 数値
     */
    public static int toInt(String value) {
        int retValue = 0;
        if (value != null) {
            try {
                retValue = Integer.parseInt(value);
            } catch (NumberFormatException e) { /* 何もしない. */ }
        }
        return retValue;
    }

    /**
     * バージョンコードを取得する
     *
     * @param context
     * @return
     */
    public static String getVersionCode(Context context){
        PackageManager pm = context.getPackageManager();
        String versionName = "";
        try{
            PackageInfo packageInfo = pm.getPackageInfo(context.getPackageName(), 0);
            versionName = packageInfo.versionName;
        }catch(PackageManager.NameNotFoundException e){
            e.printStackTrace();
        }
        return versionName;
    }

    //20210918 ADD Takase V2.0 START
    /**
     * IPアドレスを取得する
     * @return
     */
    public static String getIpAddress() {
        String ip = null;
        try {
            // インターフェースを列挙
            Enumeration<NetworkInterface> en = NetworkInterface.getNetworkInterfaces();
            while ( (ip == null) && en.hasMoreElements() ) {
                // インターフェースオブジェクトを取得
                NetworkInterface ni = en.nextElement();

                // このインターフェースのアドレスを列挙
                Enumeration<InetAddress> ei = ni.getInetAddresses();
                while (ei.hasMoreElements()) {
                    // アドレスを取得
                    InetAddress ia = ei.nextElement();

                    // IPv4でなければ無視
                    if ( !(ia instanceof java.net.Inet4Address) ) {
                        continue;
                    }
                    // ループバックアドレスは無視
                    if ( ia.isLoopbackAddress() ) {
                        continue;
                    }

                    // IPアドレスを取得
                    ip = ia.getHostAddress();
                    break;
                }
            }
        } catch (Exception ex) {
            ip = "000.000.000.000";
        }
        return ip;
    }

    /**
     * 端末IDを取得する
     * @param ipAddress
     * @return
     */
    public static String getPdaId(String ipAddress) {
        if (ipAddress == null || ipAddress.equals("")) return "000.000";

        String[] sp = ipAddress.split("\\.");
        switch (sp.length) {
            case 0:
                return "000.000";
            case 1:
                return "000." + String.format("000", sp[0]);
            default:
                return String.format("%03d", Integer.parseInt(sp[sp.length - 2])) + String.format("%03d", Integer.parseInt(sp[sp.length - 1]));
        }
    }
    //20210918 ADD Takase V2.0 END

    /**
     * EPCサイズの変更要否チェック
     * @param epcBitSize EPCのビットサイズ
     * @param pcHex 16進数のPC文字列
     * @return
     */
    public static boolean needOverWritePc(int epcBitSize, String pcHex) {
        String beforePcLengthBit = convertHexToBinary(pcHex).substring(0, PC_LENGTH_BIT_SIZE);
        String overWritePcLengthBit = createPcLengthBit(epcBitSize);

        return !beforePcLengthBit.equals(overWritePcLengthBit);
    }

    /**
     * EPCサイズ変更用のPC算出処理
     * @param epcBitSize EPCのビットサイズ
     * @param pcHex 16進数のPC文字列
     * @return
     */
    public static String calcTagPcHex(int epcBitSize, String pcHex) {
        String beforePcBit = convertHexToBinary(pcHex);
        String overWritePcLengthBit = createPcLengthBit(epcBitSize);

        String afterPcBit = overWritePcLengthBit + beforePcBit.substring(PC_LENGTH_BIT_SIZE);
        return binaryToHexString(afterPcBit);
    }

    /**
     * RFIDタグのPCのLength(bit 単位)作成処理
     * @param epcBitSize EPCのビットサイズ
     * @return
     */
    private static String createPcLengthBit(int epcBitSize) {
        return String.format("%" + PC_LENGTH_BIT_SIZE + "s",
                        Integer.toBinaryString(epcBitSize / 16))
                .replace(' ', '0');
    }

    /**
     * 16進数文字列→2進数文字列に変換
     * @param hexSrt 16進数文字列
     * @return
     */
    public static String convertHexToBinary(String hexSrt) {
        StringBuilder binaryString = new StringBuilder();
        for (char hexChar : hexSrt.toCharArray()) {
            if (!Character.isDigit(hexChar) && (hexChar < 'A' || hexChar > 'F')) {
                return hexSrt;  // 16進数文字列以外が含まれている場合
            }
            int value = Integer.parseInt(Character.toString(hexChar), 16);
            binaryString.append(String.format("%4s", Integer.toBinaryString(value)).replace(' ', '0'));
        }
        return binaryString.toString();
    }

    /**
     * 2進数文字列→16進数文字列に変換
     * @param binaryStr 2進数文字列
     * @return
     */
    public static String binaryToHexString(String binaryStr) {
        if (binaryStr.length() % 4 != 0) {
            return binaryStr;  // 4桁で割り切れない場合
        }
        StringBuilder hexString = new StringBuilder();
        for (int i = 0; i < binaryStr.length(); i += 4) {
            String bits = binaryStr.substring(i, i + 4);
            int value = Integer.parseInt(bits, 2);
            hexString.append(Integer.toHexString(value).toUpperCase());
        }
        return hexString.toString();
    }
}
