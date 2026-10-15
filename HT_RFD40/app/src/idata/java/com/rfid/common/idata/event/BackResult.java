package com.rfid.common.idata.event;


/**
 * author CYD
 * date 2018/11/19
 * email chengyd@idatachina.com
 */
public interface BackResult extends OnKeyDownListener {
    void postResult(String[] tagData);

    //void postInventoryRate(long rate);
}
