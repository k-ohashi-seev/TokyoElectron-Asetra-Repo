package com.rfid.common.parameter;

import com.keyence.autoid.sdk.rfid.params.InventoryMode;
import com.keyence.autoid.sdk.rfid.params.InventoryParams;
import com.keyence.autoid.sdk.rfid.params.MemoryBank;
import com.keyence.autoid.sdk.rfid.params.TriggerMode;

/**
 * インベントリ処理に関するパラメータを作成するクラスです。
 * <ui>
 *     <li>RFIDリーダの設定パラメータを作成します</li>
 *     <li>インベントリモードを設定します</li>
 *     <li>トリガーモードを設定します</li>
 *     <li>読み取り最小間隔を設定します</li>
 *     <li>二度読み許可を設定します</li>
 *     <li>メモリバンクを設定します</li>
 *     <li>読み出し開始位置を設定します</li>
 *     <li>読み出しサイズを設定します</li>
 *     <li>アクセスパスワードを設定します</li>
 *     <li>タイムアウトを設定します</li>
 *     <li>各パラメータを設定し、build()で作成します</li>
 * </ui>
 */
public class InventoryParamsBuilder {
    private InventoryMode inventoryMode = InventoryMode.NORMAL;
    private TriggerMode triggerMode = TriggerMode.ALWAYS_ON;
    private int minInterval = 0;
    private boolean allowDoubleReading = false;
    private MemoryBank memoryBank = MemoryBank.EPC;
    private int size = 1;
    private String accessPassword = "00000000";
    private int timeout = 1800;

    //EPC領域は、「CRC + PC + EPC」から構成される(EPCglobal C1G2、ISO/IEC 18000-63に準拠する場合)
    //「CRC + PC」は32ビットであるため、offsetは2以上を指定することでEPCの先頭からの値を対象にできる。
    //※Filterのoffset(bit単位)とは異なり、16進数時の桁数で指定する必要があり、
    //　2を指定することで、先頭から8桁分(ビット換算で32ビット)ずらした位置を開始位置とすることができる。
    private int hexOffset = 2;

    /** インベントリモード設定 */
    public InventoryParamsBuilder inventoryMode(InventoryMode inventoryMode) {
        this.inventoryMode = inventoryMode;
        return this;
    }

    /** トリガーモード設定 */
    public InventoryParamsBuilder triggerMode(TriggerMode triggerMode) {
        this.triggerMode = triggerMode;
        return this;
    }

    /** 読み取り最小間隔設定(ミリ秒) */
    public InventoryParamsBuilder minInterval(int minInterval) {
        if (minInterval >= 0 && minInterval <= 10000) {
            //範囲内なら設定
            this.minInterval = minInterval;
        }
        return this;
    }

    /** 二度読み許可設定 */
    public InventoryParamsBuilder allowDoubleReading(boolean allow) {
        this.allowDoubleReading = allow;
        return this;
    }

    /** メモリバンク設定 */
    public InventoryParamsBuilder memoryBank(MemoryBank memoryBank) {
        //インベントリモードがDETAILではない場合、設定しない
        if (inventoryMode != InventoryMode.DETAIL) {return this;}
        this.memoryBank = memoryBank;
        return this;
    }

    /** 読取開始位置設定 */
    public InventoryParamsBuilder offset(int offset) {
        //インベントリモードがDETAILではない場合、設定しない
        if (inventoryMode != InventoryMode.DETAIL) {return this;}
        if (offset >= 0 && offset <= 65535) {
            //範囲内
            this.hexOffset = offset;
        }
        return this;
    }

    /** 読み出しサイズ設定 */
    public InventoryParamsBuilder size(int size) {
        //インベントリモードがDETAILではない場合、設定しない
        if (inventoryMode != InventoryMode.DETAIL) {return this;}
        if (size >= 1 && size <= 255) {
            //範囲内
            this.size = size;
        }
        return this;
    }

    /** アクセスパスワード設定(8桁の16進数文字列) */
    public InventoryParamsBuilder accessPassword(String accessPassword) {
        //インベントリモードがDETAILではない場合、設定しない
        if (inventoryMode != InventoryMode.DETAIL) {return this;}
        if (accessPassword.length() == 8) {
            //8桁の場合のみ設定
            this.accessPassword = accessPassword;
        }
        return this;
    }

    /** タイムアウト設定(秒) */
    public InventoryParamsBuilder timeout(int timeout) {
        if (timeout >= 0 && timeout <=1800) {
            //範囲内
            this.timeout = timeout;
        }
        return this;
    }

    /** インベントリパラメータを作成します。 */
    public InventoryParams build() {
        InventoryParams params = new InventoryParams();
        params.inventoryMode = inventoryMode;
        params.triggerMode = triggerMode;
        params.minInterval = minInterval;
        params.allowDoubleReading = allowDoubleReading;
        params.memoryBank = memoryBank;
        params.offset = hexOffset;
        params.size = size;
        params.accessPassword = accessPassword;
        params.timeout = timeout;
        return params;
    }
}
