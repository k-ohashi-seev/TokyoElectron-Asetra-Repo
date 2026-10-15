package com.rfid.common.parameter;

import com.keyence.autoid.sdk.rfid.params.MemoryBank;
import com.keyence.autoid.sdk.rfid.params.WriteParams;

/**
 * 書込み処理に関するパラメータを作成するクラスです。
 * <ui>
 *     <li>メモリバンクを設定します</li>
 *     <li>読み出し開始位置を設定します</li>
 *     <li>書込みデータを設定します</li>
 *     <li>アクセスパスワードを設定します</li>
 *     <li>タイムアウトを設定します</li>
 *     <li>各パラメータを設定し、build()で作成します</li>
 * </ui>
 */
public class WriteParamsBuilder {
    private MemoryBank memoryBank = MemoryBank.EPC;
    private byte[] data = new byte[0];
    private String accessPassword = "00000000";
    private int timeout = 10000;

    //EPC領域は、「CRC + PC + EPC」から構成される(EPCglobal C1G2、ISO/IEC 18000-63に準拠する場合)
    //「CRC + PC」は32ビットであるため、offsetは2以上を指定することでEPCの先頭からの値を対象にできる。
    //※Filterのoffset(bit単位)とは異なり、16進数時の桁数で指定する必要があり、
    //　2を指定することで、先頭から8桁分(ビット換算で32ビット)ずらした位置を開始位置とすることができる。
    private int hexOffset = 2;

    /** メモリバンク設定 */
    public WriteParamsBuilder memoryBank(MemoryBank memoryBank) {
        this.memoryBank = memoryBank;
        return this;
    }

    /** 読取開始位置設定 */
    public WriteParamsBuilder offset(int offset) {
        if (offset >= 0 && offset <= 65535) {
            //範囲内
            this.hexOffset = offset;
        }
        return this;
    }

    /**
     * 書込みデータ設定
     * @param data
     */
    public WriteParamsBuilder data(String data) {
        this.data = data.getBytes();
        return this;
    }

    /** 書込みデータ設定 */
    public WriteParamsBuilder data(byte[] data) {
        this.data = data;
        return this;
    }

    /** アクセスパスワード設定(8桁の16進数文字列) */
    public WriteParamsBuilder accessPassword(String accessPassword) {
        if (accessPassword.length() == 8) {
            //8桁の場合のみ設定
            this.accessPassword = accessPassword;
        }
        return this;
    }

    /** タイムアウト設定(秒) */
    public WriteParamsBuilder timeout(int timeout) {
        if (timeout >= 0 && timeout <=1800) {
            //範囲内
            this.timeout = timeout;
        }
        return this;
    }

    /** 書込みパラメータを作成します。 */
    public WriteParams build() {
        WriteParams params = new WriteParams();
        params.memoryBank = memoryBank;
        params.offset = hexOffset;
        params.data = data;
        params.accessPassword = accessPassword;
        params.timeout = timeout;
        return params;
    }
}
