package com.rfid.common.idata.event;

import android.os.SystemClock;

import com.rfid.common.idata.IdataApplication;


/**
 * author CYD
 * date 2018/11/19
 * email chengyd@idatachina.com
 */
public class GetRFIDThread extends Thread {
    static GetRFIDThread instance = null;
    private final Object lock = new Object();
    private boolean pause = false;
    private boolean Run = false;

    /**
     * 调用这个方法实现暂停线程
     */
    public void pauseThread() {
        pause = true;
    }

    /**
     * 调用这个方法实现恢复线程的运行
     */
    public void resumeThread() {
        pause = false;
        synchronized (lock) {
            lock.notifyAll();
        }
    }
    public boolean getRunStatus() {
        return Run;
    }
    /**
     * 注意：这个方法只能在run方法里调用，不然会阻塞主线程，导致页面无响应
     */
    void onPause() {
        synchronized (lock) {
            try {
                lock.wait();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
    }

    private GetRFIDThread() {
    }

    public static GetRFIDThread getInstance() {
        if (null == instance) {
            synchronized (GetRFIDThread.class) {
                if (null == instance) {
                    instance = new GetRFIDThread();
                }
            }
        }
        return instance;
    }


    public void setInstance(GetRFIDThread instance) {
        this.instance=instance;
    }

    private BackResult ba;

    private boolean ifPostMsg = true;

    public void setBackResult(BackResult ba) {
        this.ba = ba;
    }

    public boolean isIfPostMsg() {
        return ifPostMsg;
    }


    private boolean flag = true;

    public void destoryThread() {
        flag = false;
    }

    private long sTime;

    public void setIfPostMsg(boolean ifPostMsg) {
        synchronized (lock) {
            if (ifPostMsg) {
                sTime = SystemClock.elapsedRealtime();
            }
            this.ifPostMsg = ifPostMsg;
        }
    }

    private boolean searchTag; //是否处于查询标签模式

    public void setSearchTag(boolean searchTag) {
        this.searchTag = searchTag;
    }

    @Override
    public void run() {
        long curTime, oldTime = 0;
        Run=true;
        while (flag) {
            while (pause) {
                onPause();
            }

            //if (ifPostMsg) {
                String[] tagData = IdataApplication.getIdataApp().getIdataLib().readTagFromBuffer();
                if (tagData != null) {
                    ba.postResult(tagData);
                } else if (searchTag) { //当超过一秒查询不到标签，清空状态
                    curTime = System.currentTimeMillis();
                    if ((curTime - oldTime) > 1000 && (oldTime != 0)) {
                        ba.postResult(null);
                    }
                    if (oldTime == 0) {
                        oldTime = curTime;
                    }
                }
            //}
        }

        instance = null;
    }
}
