package com.rfid.common.idata.event;

import android.view.KeyEvent;

/**
 * Author CYD
 * Date 2019/2/20
 * Email chengyd@idatachina.com
 */
public interface OnKeyListener extends OnKeyDownListener {

    void onKeyUp(int keyCode, KeyEvent event);
}
