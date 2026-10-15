package com.rfid.common.enumparam;

public enum EnSession {
    SESSION_0(0),
    SESSION_1(1),
    SESSION_2(2),
    SESSION_3(3);

    private final int value;
    EnSession(int value) {
        this.value = value;
    }

    /**
     * セッション数値を返却
     * @return セッション値
     */
    public int getValue() {
        return value;
    }

    /**
     * セッションを返却
     * @param session セッション数値
     * @return セッション列挙値
     */
    public static EnSession getEnSession(int session) {
        for (EnSession enSession : EnSession.values()) {
            if (enSession.value == session) {
                return enSession;
            }
        }
        return null;
    }
}
