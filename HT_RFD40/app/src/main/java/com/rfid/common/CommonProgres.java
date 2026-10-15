package com.rfid.common;

import android.app.Activity;
import android.app.ProgressDialog;

/**
 *  プログレスダイアログ共通クラス
 */
public class CommonProgres {

    //プログレスダイアログ
    private ProgressDialog _ProgressDialog = null;

    /**
     * プログレスダイアログ表示
     * @param act アクテイビティ
     * @param prmMessage  メッセージ
     * */
    public void StartProgressDialog(Activity act, String prmMessage) {
        // プログレスダイアログの生成
        _ProgressDialog = new ProgressDialog(act);

        // プログレスダイアログの設定
        _ProgressDialog.setMessage(prmMessage);  // メッセージをセット

        _ProgressDialog.setProgressStyle(ProgressDialog.STYLE_SPINNER);

        // プログレスダイアログの表示
        _ProgressDialog.show();

        _ProgressDialog.setCanceledOnTouchOutside(false);
    }

    /**
     * プログレスダイアログ消去
     */
    public void StopProgressDialog() {
        // プログレスダイアログを閉じる
        if (_ProgressDialog != null && _ProgressDialog.isShowing()) {
            _ProgressDialog.dismiss();
        }
    }
}
