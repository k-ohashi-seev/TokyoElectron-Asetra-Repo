package com.rfid.common.parameter;

import com.keyence.autoid.sdk.rfid.params.CommonParams;
import com.keyence.autoid.sdk.rfid.params.IDType;
import com.keyence.autoid.sdk.rfid.params.PowerSaveMode;
import com.keyence.autoid.sdk.rfid.params.SessionFlag;

/**
 * RFID共通設定パラメータを作成するクラスです。
 * <ui>
 *     <li>RFIDリーダの設定パラメータを作成します</li>
 *     <li>タグID(種別)を設定します</li>
 *     <li>アクセスパスワードを設定します</li>
 *     <li>出力電力を設定します</li>
 *     <li>周波数チャネルを設定します(DX-RH1では5,11,17,23,24,25が設定可能)</li>
 *     <li>省電力モードを設定します</li>
 *     <li>セッションフラグを設定します</li>
 * </ui>
 */
public class CommonParamsBuilder {
    private IDType idType = IDType.TAG_ID;
    private String accessPassword = "00000000";
    private int transmitPower = 30;
    private int[] frequencyChannel = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50};
    private PowerSaveMode powerSaveMode = PowerSaveMode.DISABLE_POWER_SAVE;
    private SessionFlag sessionFlag = SessionFlag.SESSION1;

    /** タグIDを設定します */
    public CommonParamsBuilder idType(IDType idType) {
        this.idType = idType;
        return this;
    }

    /** アクセスパスワードを設定します */
    public CommonParamsBuilder accessPassword(String accessPassword) {
        if (accessPassword.length() == 8) {
            //8桁の場合のみ設定
            this.accessPassword = accessPassword;
        }
        return this;
    }

    /** 出力電力を設定します */
    public CommonParamsBuilder transmitPower(int transmitPower) {
        if (transmitPower >= 0 && transmitPower <= 30) {
            this.transmitPower = transmitPower;
        }
        return this;
    }

    /** 周波数チャネルを設定します */
    public CommonParamsBuilder frequencyChannel(int[] frequencyChannel) {
        this.frequencyChannel = frequencyChannel;
        return this;
    }

    /** 省電力モードを設定します */
    public CommonParamsBuilder powerSaveMode(PowerSaveMode powerSaveMode) {
        this.powerSaveMode = powerSaveMode;
        return this;
    }

    /** セッションフラグを設定します */
    public CommonParamsBuilder sessionFlag(SessionFlag sessionFlag) {
        this.sessionFlag = sessionFlag;
        return this;
    }

    /** RFID共通パラメータを作成します */
    public CommonParams build() {
        CommonParams params = new CommonParams();
        params.idType = this.idType;
        params.accessPassword = this.accessPassword;
        params.transmitPower = this.transmitPower;
        params.frequencyChannel = this.frequencyChannel;
        params.powerSaveMode = this.powerSaveMode;
        params.sessionFlag = this.sessionFlag;
        return params;
    }

    /**
     * RFID共通パラメータを作成します
     * @param deviceParams 端末に現在適用されているの設定 ※getConfigで取得したパラメータを渡すこと
     */
    public CommonParams build(CommonParams deviceParams) {
        //周波数は端末の設定を適用する
        deviceParams.idType = this.idType;
        deviceParams.accessPassword = this.accessPassword;
        deviceParams.transmitPower = this.transmitPower;
//        params.frequencyChannel = this.frequencyChannel;
        deviceParams.powerSaveMode = this.powerSaveMode;
        deviceParams.sessionFlag = this.sessionFlag;
        return deviceParams;
    }
}
