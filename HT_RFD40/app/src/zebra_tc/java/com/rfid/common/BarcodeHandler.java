package com.rfid.common;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.media.AudioManager;
import android.os.AsyncTask;
import android.os.Handler;
import android.os.Message;
import android.util.Log;
import android.view.View;
import android.widget.ListView;
import android.widget.Toast;

import com.zebra.scannercontrol.DCSSDKDefs;
import com.zebra.scannercontrol.DCSScannerInfo;
import com.zebra.scannercontrol.FirmwareUpdateEvent;
import com.zebra.scannercontrol.IDcsSdkApiDelegate;
import com.rfid.scannercontrol.app.application.Application;
import com.rfid.scannercontrol.app.helpers.AvailableScanner;
import com.rfid.scannercontrol.app.helpers.Barcode;
import com.rfid.scannercontrol.app.helpers.Constants;
import com.rfid.scannercontrol.app.helpers.CustomProgressDialog;
import com.rfid.scannercontrol.app.helpers.Foreground;
import com.rfid.scannercontrol.app.helpers.ScannerAppEngine;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import static android.app.Activity.RESULT_CANCELED;
import static com.zebra.scannercontrol.DCSSDKDefs.DCSSDK_CONN_TYPES.DCSSDK_CONNTYPE_BT_NORMAL;
import static com.rfid.scannercontrol.app.helpers.Constants.DEBUG_TYPE.TYPE_DEBUG;
import static com.rfid.scannercontrol.app.helpers.Constants.logAsMessage;

public class BarcodeHandler implements ScannerAppEngine, IDcsSdkApiDelegate , ScannerAppEngine.IScannerAppEngineDevConnectionsDelegate, ScannerAppEngine.IScannerAppEngineDevListDelegate {
    protected static String TAG;
    protected static DCSSDKDefs.DCSSDK_BT_PROTOCOL selectedProtocol;
    protected static DCSSDKDefs.DCSSDK_BT_SCANNER_CONFIG selectedConfig;
    private static ArrayList<IScannerAppEngineDevConnectionsDelegate> mDevConnDelegates=new ArrayList<IScannerAppEngineDevConnectionsDelegate>();
    private static ArrayList<IScannerAppEngineDevEventsDelegate> mDevEventsDelegates=new ArrayList<IScannerAppEngineDevEventsDelegate>();;
    private static ArrayList<DCSScannerInfo> mScannerInfoList;
    private static ArrayList<DCSScannerInfo> mOfflineScannerInfoList;
    public static int lastConnectedScannerID = 0;
    static MyAsyncTask cmdExecTask = null;
    static AvailableScanner curAvailableScanner = null;
    private CustomProgressDialog progressDialog;
    ArrayList<AvailableScanner> scannersList = new ArrayList<>();
    ListView lastConnectedScanner;
    ArrayList<AvailableScanner> lastConnectedScannerList = new ArrayList<>();
    private int REFRESH = 1;
    private int EVENT = 2;
    private int CREATE = 3;
    // AvailableScannerListAdapter availableScannerListAdapter;
    ListView otherScanners;

    // The Handler that gets information back from the BluetoothChatService
    protected final Handler mHandler = initializeHandler();
    static  boolean waitingForFWReboot = false;

    public Application application = null;
    private BaseRFIDActivity context;

    public void onCreate(BaseRFIDActivity activity) {
        // application context
        context = activity;

        //音量を一旦消す
        AudioManager am = (AudioManager)activity.getSystemService(Context.AUDIO_SERVICE);
        int intValue = am.getStreamVolume(AudioManager.STREAM_RING);
        try {
            am.setStreamVolume(AudioManager.STREAM_RING, 0, AudioManager.FLAG_ALLOW_RINGER_MODES);
        } catch(Exception e) {
        }

        application = new Application(context);

        mScannerInfoList=application.mScannerInfoList;
        mOfflineScannerInfoList=new ArrayList<DCSScannerInfo>();
        addDevListDelegate(this);
        configureNotificationAvailable(true);
        TAG = getClass().getSimpleName();
        application.sdkHandler.dcssdkSetDelegate(this);
        initializeDcsSdkWithAppSettings();
        initialize();
        //音量を戻す
        try {
            am.setStreamVolume(AudioManager.STREAM_RING, intValue, AudioManager.FLAG_ALLOW_RINGER_MODES);
        } catch (Exception e) {
        }
    }

    private void initialize() {
        initializeDcsSdk();
        addDevConnectionsDelegate(this);
    }

    private void initializeDcsSdk(){
        application.sdkHandler.dcssdkEnableAvailableScannersDetection(true);
        application.sdkHandler.dcssdkSetOperationalMode(DCSSDKDefs.DCSSDK_MODE.DCSSDK_OPMODE_BT_NORMAL);
        application.sdkHandler.dcssdkSetOperationalMode(DCSSDKDefs.DCSSDK_MODE.DCSSDK_OPMODE_SNAPI);
        application.sdkHandler.dcssdkSetOperationalMode(DCSSDKDefs.DCSSDK_MODE.DCSSDK_OPMODE_BT_LE);
        application.sdkHandler.dcssdkSetOperationalMode(DCSSDKDefs.DCSSDK_MODE.DCSSDK_OPMODE_USB_CDC);      //econnexで接続したRFIDリーダのスキャナからバーコード読取結果を受信するために必要
    }

    private Handler initializeHandler() {
        if (application.globalMsgHandler != null)
            return application.globalMsgHandler;
        return null;
    }

    protected void onPause() {
        removeDevListDelegate(this);
        Application.sdkHandler.dcssdkStopScanningDevices();
    }

    protected void onResume() {
        String ns = Context.NOTIFICATION_SERVICE;
        addDevListDelegate(this);
    }

    @Override
    public boolean scannerHasAppeared(int scannerID) {
        return false;
    }

    @Override
    public boolean scannerHasDisappeared(int scannerID) {
        return false;
    }

    @Override
    public boolean scannerHasConnected(int scannerID) {

        ArrayList<DCSScannerInfo> activeScanners = new ArrayList<DCSScannerInfo>();
        application.sdkHandler.dcssdkGetActiveScannersList(activeScanners);

        for (DCSScannerInfo scannerInfo : application.mScannerInfoList) {
            if (scannerInfo.getScannerID() == scannerID) {
                application.isAnyScannerConnected = true;
                application.currentConnectedScannerID = scannerID;
                application.currentConnectedScanner = scannerInfo;
                application.lastConnectedScanner = application.currentConnectedScanner;
                break;
            }
        }
        context.Reader_ReConnect();
        context.setScannerID(scannerID);    //20200108 Add Sasaki
        return true;
    }

    @Override
    public boolean scannerHasDisconnected(int scannerID) {
        context.onDisconnectEvent();
        application.barcodeData.clear();
        application.isAnyScannerConnected = false;
        application.currentConnectedScannerID = -1;
        application.lastConnectedScanner = application.currentConnectedScanner;
        application.currentConnectedScanner = null;
        return false;
    }

    public void onResumeBarcode() {
        application.sdkHandler.dcssdkSetDelegate(this);
        IntentFilter filter = new IntentFilter(Constants.ACTION_SCANNER_CONNECTED);
        filter.addAction(Constants.ACTION_SCANNER_DISCONNECTED);
        filter.addAction(Constants.ACTION_SCANNER_AVAILABLE);
        filter.addAction(Constants.ACTION_SCANNER_CONN_FAILED);

        TAG = getClass().getSimpleName();
    }

    @Override
    public void initializeDcsSdkWithAppSettings() {
        // Restore preferences
        SharedPreferences settings = context.getSharedPreferences(Constants.PREFS_NAME, 0);

        application.MOT_SETTING_OPMODE = settings.getInt(Constants.PREF_OPMODE, DCSSDK_CONNTYPE_BT_NORMAL.value);

        application.MOT_SETTING_SCANNER_DETECTION = settings.getBoolean(Constants.PREF_SCANNER_DETECTION, true);
        application.MOT_SETTING_EVENT_IMAGE = settings.getBoolean(Constants.PREF_EVENT_IMAGE, true);
        application.MOT_SETTING_EVENT_VIDEO = settings.getBoolean(Constants.PREF_EVENT_VIDEO, true);
        application.MOT_SETTING_EVENT_BINARY_DATA = settings.getBoolean(Constants.PREF_EVENT_BINARY_DATA, true);

        application.MOT_SETTING_EVENT_ACTIVE = settings.getBoolean(Constants.PREF_EVENT_ACTIVE, true);
        application.MOT_SETTING_EVENT_AVAILABLE = settings.getBoolean(Constants.PREF_EVENT_AVAILABLE, true);
        application.MOT_SETTING_EVENT_BARCODE = settings.getBoolean(Constants.PREF_EVENT_BARCODE, true);

        application.MOT_SETTING_NOTIFICATION_AVAILABLE = settings.getBoolean(Constants.PREF_NOTIFY_AVAILABLE, false);
        application.MOT_SETTING_NOTIFICATION_ACTIVE = settings.getBoolean(Constants.PREF_NOTIFY_ACTIVE, false);
        application.MOT_SETTING_NOTIFICATION_BARCODE = settings.getBoolean(Constants.PREF_NOTIFY_BARCODE, false);

        application.MOT_SETTING_NOTIFICATION_IMAGE = settings.getBoolean(Constants.PREF_NOTIFY_IMAGE, false);
        application.MOT_SETTING_NOTIFICATION_VIDEO = settings.getBoolean(Constants.PREF_NOTIFY_VIDEO, false);
        application.MOT_SETTING_NOTIFICATION_BINARY_DATA = settings.getBoolean(Constants.PREF_NOTIFY_BINARY_DATA, false);

        int notifications_mask = 0;
        if (application.MOT_SETTING_EVENT_AVAILABLE) {
            notifications_mask |= (DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SCANNER_APPEARANCE.value | DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SCANNER_DISAPPEARANCE.value);
        }
        if (application.MOT_SETTING_EVENT_ACTIVE) {
            notifications_mask |= (DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SESSION_ESTABLISHMENT.value | DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SESSION_TERMINATION.value);
        }
        if (application.MOT_SETTING_EVENT_BARCODE) {
            notifications_mask |= (DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_BARCODE.value);
        }
        if (application.MOT_SETTING_EVENT_IMAGE) {
            notifications_mask |= (DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_IMAGE.value);
        }
        if (application.MOT_SETTING_EVENT_VIDEO) {
            notifications_mask |= (DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_VIDEO.value);
        }
        if (application.MOT_SETTING_EVENT_BINARY_DATA) {
            notifications_mask |= (DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_BINARY_DATA.value);
        }
        application.sdkHandler.dcssdkSubsribeForEvents(notifications_mask);
    }

    void setWaitingForFWReboot(boolean _waitingForFWReboot){
        waitingForFWReboot = _waitingForFWReboot;
        Log.i("*************", "setWaitingForFWReboot "+String.valueOf(waitingForFWReboot));
    }

    /* ###################################################################### */
    /* ########## Utility functions ######################################### */
    /* ###################################################################### */
    @Override
    public void showMessageBox(String message) {
        //TODO - Handle the callback from SDK Handler
    }

    @Override
    public int showBackgroundNotification(String text) {
        //TODO - Handle the callback from SDK Handler
        return 0;
    }

    @Override
    public int dismissBackgroundNotifications() {
        //TODO - Handle the callback from SDK Handler
        return 0;
    }

    /**
     * Checks if the application is being sent in the background (i.e behind
     * another application's Activity).
     * @param context the context
     * @return <code>true</code> if another application will be above this one.
     */

    @Override
    public boolean isInBackgroundMode(final Context context) {
        return Foreground.get().isBackground();
    }

    /* ###################################################################### */
    /* ########## API calls for UI View Controllers ######################### */
    /* ###################################################################### */
    @Override
    public void addDevListDelegate(IScannerAppEngineDevListDelegate delegate) {
        if (application.mDevListDelegates == null)
            application.mDevListDelegates = new ArrayList<IScannerAppEngineDevListDelegate>();
        application.mDevListDelegates.add(delegate);
    }

    @Override
    public void addDevConnectionsDelegate(IScannerAppEngineDevConnectionsDelegate delegate) {
        if (mDevConnDelegates == null)
            mDevConnDelegates = new ArrayList<IScannerAppEngineDevConnectionsDelegate>();
        mDevConnDelegates.add(delegate);
    }

    @Override
    public void addDevEventsDelegate(IScannerAppEngineDevEventsDelegate delegate) {
        if (mDevEventsDelegates == null)
            mDevEventsDelegates = new ArrayList<IScannerAppEngineDevEventsDelegate>();
        mDevEventsDelegates.add(delegate);
    }

    @Override
    public void removeDevListDelegate(IScannerAppEngineDevListDelegate delegate) {
        if (application.mDevListDelegates != null)
            application.mDevListDelegates.remove(delegate);
    }

    @Override
    public void removeDevConnectiosDelegate(IScannerAppEngineDevConnectionsDelegate delegate) {
        if (mDevConnDelegates != null)
            mDevConnDelegates.remove(delegate);
    }

    @Override
    public void removeDevEventsDelegate(ScannerAppEngine.IScannerAppEngineDevEventsDelegate delegate) {
        if (mDevEventsDelegates != null)
            mDevEventsDelegates.remove(delegate);
    }

    @Override
    public List<DCSScannerInfo> getActualScannersList() {
        return mScannerInfoList;
    }

    @Override
    public DCSScannerInfo getScannerInfoByIdx(int dev_index) {
        if (mScannerInfoList != null)
            return mScannerInfoList.get(dev_index);
        else
            return null;
    }

    @Override
    public DCSScannerInfo getScannerByID(int scannerId) {
        if (mScannerInfoList != null) {
            for (DCSScannerInfo scannerInfo : mScannerInfoList) {
                if (scannerInfo != null && scannerInfo.getScannerID() == scannerId)
                    return scannerInfo;
            }
        }
        return null;
    }

    @Override
    public void raiseDeviceNotificationsIfNeeded() {

    }

    /* ###################################################################### */
    /* ########## Interface for DCS SDK ##################################### */
    /* ###################################################################### */
    @Override
    public void updateScannersList() {
        if (application.sdkHandler != null) {
            mScannerInfoList.clear();
            ArrayList<DCSScannerInfo> scannerTreeList=new ArrayList<DCSScannerInfo>();
            application.sdkHandler.dcssdkGetAvailableScannersList(scannerTreeList);
            application.sdkHandler.dcssdkGetActiveScannersList(scannerTreeList);
            createFlatScannerList(scannerTreeList);
        }
    }

    private void createFlatScannerList(ArrayList<DCSScannerInfo> scannerTreeList) {
        for (DCSScannerInfo s :
                scannerTreeList) {
            addToScannerList(s);
        }
    }

    private void addToScannerList(DCSScannerInfo s) {
        mScannerInfoList.add(s);
        if(s.getAuxiliaryScanners() !=null) {
            for (DCSScannerInfo aux :
                    s.getAuxiliaryScanners().values()) {
                addToScannerList(aux);
            }
        }    }


    @Override
    public DCSSDKDefs.DCSSDK_RESULT connect(int scannerId) {
        if (application.sdkHandler != null) {

            return application.sdkHandler.dcssdkEstablishCommunicationSession(scannerId);
        } else {
            return DCSSDKDefs.DCSSDK_RESULT.DCSSDK_RESULT_FAILURE;
        }
    }

    @Override
    public void disconnect(int scannerId) {
        if (application.sdkHandler != null) {
            DCSSDKDefs.DCSSDK_RESULT ret = application.sdkHandler.dcssdkTerminateCommunicationSession(scannerId);
            updateScannersList();
        }
        removeDevListDelegate(this);
    }

    @Override
    public DCSSDKDefs.DCSSDK_RESULT setAutoReconnectOption(int scannerId, boolean enable) {
        DCSSDKDefs.DCSSDK_RESULT ret = DCSSDKDefs.DCSSDK_RESULT.DCSSDK_RESULT_FAILURE;
        if (application.sdkHandler != null) {
            ret =  application.sdkHandler.dcssdkEnableAutomaticSessionReestablishment(enable, scannerId);
        }
        context.Reader_ReConnect();         //20200115 Add Sasaki
        context.setScannerID(scannerId);    //20200115 Add Sasaki
        return ret;
    }

    @Override
    public void enableScannersDetection(boolean enable) {
        if (application.sdkHandler != null) {
            application.sdkHandler.dcssdkEnableAvailableScannersDetection(enable);
        }
    }

    @Override
    public void configureNotificationAvailable(boolean enable) {
        if (application.sdkHandler != null) {
            if (enable) {
                application.sdkHandler.dcssdkSubsribeForEvents(DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SCANNER_APPEARANCE.value |
                        DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SCANNER_DISAPPEARANCE.value);
            } else {
                application.sdkHandler.dcssdkUnsubsribeForEvents(DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SCANNER_APPEARANCE.value |
                        DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SCANNER_DISAPPEARANCE.value);
            }
        }
    }

    @Override
    public void configureNotificationActive(boolean enable) {
        if (application.sdkHandler != null) {
            if (enable) {
                application.sdkHandler.dcssdkSubsribeForEvents(DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SESSION_ESTABLISHMENT.value |
                        DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SESSION_TERMINATION.value);
            } else {
                application.sdkHandler.dcssdkUnsubsribeForEvents(DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SESSION_ESTABLISHMENT.value |
                        DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_SESSION_TERMINATION.value);
            }
        }
    }

    @Override
    public void configureNotificationBarcode(boolean enable) {
        if (application.sdkHandler != null) {
            if (enable) {
                application.sdkHandler.dcssdkSubsribeForEvents(DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_BARCODE.value);
            } else {
                application.sdkHandler.dcssdkUnsubsribeForEvents(DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_BARCODE.value);
            }
        }
    }

    @Override
    public void configureNotificationImage(boolean enable) {
        if (application.sdkHandler != null) {
            if (enable) {
                application.sdkHandler.dcssdkSubsribeForEvents(DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_IMAGE.value);
            } else {
                application.sdkHandler.dcssdkUnsubsribeForEvents(DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_IMAGE.value);
            }
        }
    }

    @Override
    public void configureNotificationVideo(boolean enable) {
        if (application.sdkHandler != null) {
            if (enable) {
                application.sdkHandler.dcssdkSubsribeForEvents(DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_VIDEO.value);
            } else {
                application.sdkHandler.dcssdkUnsubsribeForEvents(DCSSDKDefs.DCSSDK_EVENT.DCSSDK_EVENT_VIDEO.value);
            }
        }
    }

    @Override
    public void configureOperationalMode(DCSSDKDefs.DCSSDK_MODE mode) {
        if (application.sdkHandler != null) {
            application.sdkHandler.dcssdkSetOperationalMode(mode);
        }
    }

    @Override
    public boolean executeCommand(DCSSDKDefs.DCSSDK_COMMAND_OPCODE opCode, String inXML, StringBuilder outXML, int scannerID) {
        if (application.sdkHandler != null)
        {
            if(outXML == null){
                outXML = new StringBuilder();
            }
            DCSSDKDefs.DCSSDK_RESULT result=application.sdkHandler.dcssdkExecuteCommandOpCodeInXMLForScanner(opCode,inXML,outXML,scannerID);
            if(result== DCSSDKDefs.DCSSDK_RESULT.DCSSDK_RESULT_SUCCESS)
                return true;
            else if(result==DCSSDKDefs.DCSSDK_RESULT.DCSSDK_RESULT_FAILURE)
                return false;
        }
        return false;
    }


    /* ###################################################################### */
    /* ########## IDcsSdkApiDelegate Protocol implementation ################ */
    /* ###################################################################### */
    @Override
    public void dcssdkEventScannerAppeared(DCSScannerInfo availableScanner) {
        dataHandler.obtainMessage(Constants.SCANNER_APPEARED,availableScanner).sendToTarget();
    }

    @Override
    public void dcssdkEventScannerDisappeared(int scannerID) {
        dataHandler.obtainMessage(Constants.SCANNER_DISAPPEARED,scannerID).sendToTarget();
    }

    @Override
    public void dcssdkEventCommunicationSessionEstablished(DCSScannerInfo activeScanner) {
        dataHandler.obtainMessage(Constants.SESSION_ESTABLISHED,activeScanner).sendToTarget();
        context.ReConnect();		//20200121 Add Sasaki
    }

    @Override
    public void dcssdkEventCommunicationSessionTerminated(int scannerID) {
        dataHandler.obtainMessage(Constants.SESSION_TERMINATED,scannerID).sendToTarget();
    }

    @Override
    public void dcssdkEventBarcode(byte[] barcodeData, int barcodeType, int fromScannerID) {
        String result=null;
        try {
            result = new String(barcodeData, "UTF-8");
        } catch (Exception e) {
        }
        Log.d("★★barcode_read",result);

        //サブクラスで実装する
        context.handleBarcodeReceived(result);

        Barcode barcode=new Barcode(barcodeData,barcodeType,fromScannerID);
        dataHandler.obtainMessage(Constants.BARCODE_RECEIVED,barcode).sendToTarget();
    }

    @Override
    public void dcssdkEventFirmwareUpdate(FirmwareUpdateEvent firmwareUpdateEvent){
        dataHandler.obtainMessage(Constants.FW_UPDATE_EVENT,firmwareUpdateEvent).sendToTarget();
    }

    @Override
    public void dcssdkEventAuxScannerAppeared(DCSScannerInfo newTopology, DCSScannerInfo auxScanner) {
        dataHandler.obtainMessage(Constants.AUX_SCANNER_CONNECTED,auxScanner).sendToTarget();
    }


    @Override
    public void dcssdkEventImage(byte[] imageData, int fromScannerID) {
        dataHandler.obtainMessage(Constants.IMAGE_RECEIVED,imageData).sendToTarget();
    }

    @Override
    public void dcssdkEventVideo(byte[] videoFrame, int fromScannerID) {
        dataHandler.obtainMessage(Constants.VIDEO_RECEIVED,videoFrame).sendToTarget();
    }

    @Override
    public void dcssdkEventBinaryData(byte[] binaryData, int fromScannerID) {
        // todo: implement this
        logAsMessage(TYPE_DEBUG, TAG, "BinaryData Event received no.of bytes : "+binaryData.length + " for Scanner ID : "+fromScannerID);
    }
    /**
     * Method to retrieve barcodes
     * @param scannerId data tobe used to retrieve barcodes
     */
    public ArrayList<Barcode> getBarcodeData(int scannerId){
        ArrayList<Barcode> barcodes=new ArrayList<Barcode>();
        for(Barcode barcode:application.barcodeData){
            if(barcode.getFromScannerID()==scannerId){
                barcodes.add(barcode);
            }
        }
        return barcodes;
    }

    public void clearBarcodeData(){
        application.barcodeData.clear();
    }

    /**
     * Receiver to handle the events about RFID Reader
     */
    private BroadcastReceiver onNotification = new BroadcastReceiver() {
        public void onReceive(Context ctxt, Intent i) {

            //Since the application is in foreground, show a dialog.
            Toast.makeText(ctxt,i.getStringExtra(Constants.NOTIFICATIONS_TEXT),Toast.LENGTH_SHORT).show();

            //Abort the broadcast since it has been handled.
            abortBroadcast();
        }
    };

//Handler to show the data on UI

    protected Handler dataHandler = new Handler(){
        boolean notificaton_processed = false;
        boolean result = false;
        boolean found = false;
        @Override
        public void handleMessage(Message msg) {
            switch(msg.what){
                case Constants.IMAGE_RECEIVED:
                    logAsMessage(TYPE_DEBUG, TAG, "Image Received");
                    byte[] imageData=(byte[])msg.obj;
                    //Barcode barcode=(Barcode)msg.obj;
                    //application.barcodeData.add(barcode);
                    for (IScannerAppEngineDevEventsDelegate delegate : mDevEventsDelegates) {
                        if (delegate != null) {
                            logAsMessage(TYPE_DEBUG, TAG, "Show Image Received");
                            delegate.scannerImageEvent(imageData);
                        }
                    }
                    break;
                case Constants.VIDEO_RECEIVED:
                    logAsMessage(TYPE_DEBUG, TAG, "Video Received");
                    byte[] videoEvent=(byte[])msg.obj;
                    for (IScannerAppEngineDevEventsDelegate delegate : mDevEventsDelegates) {
                        if (delegate != null) {
                            logAsMessage(TYPE_DEBUG, TAG, "Show Video Received");
                            delegate.scannerVideoEvent(videoEvent);
                        }
                    }
                    //Toast.makeText(getApplicationContext(),"Image event received 000000000",Toast.LENGTH_SHORT).show();
                    break;
                case Constants.FW_UPDATE_EVENT:
                    logAsMessage(TYPE_DEBUG, TAG, "FW_UPDATE_EVENT Received. Client count = "+mDevEventsDelegates.size());
                    FirmwareUpdateEvent firmwareUpdateEvent=(FirmwareUpdateEvent)msg.obj;
                    for (IScannerAppEngineDevEventsDelegate delegate : mDevEventsDelegates) {
                        if (delegate != null) {
                            logAsMessage(TYPE_DEBUG, TAG, "Show FW_UPDATE_EVENT Received");
                            delegate.scannerFirmwareUpdateEvent(firmwareUpdateEvent);
                        }
                    }
                    break;
                case Constants.BARCODE_RECEIVED:
                    logAsMessage(TYPE_DEBUG, TAG, "Barcode Received");
                    Barcode barcode=(Barcode)msg.obj;
                    application.barcodeData.add(barcode);
                    for (IScannerAppEngineDevEventsDelegate delegate : mDevEventsDelegates) {
                        if (delegate != null) {
                            logAsMessage(TYPE_DEBUG, TAG, "Show Barcode Received");
                            delegate.scannerBarcodeEvent(barcode.getBarcodeData(), barcode.getBarcodeType(), barcode.getFromScannerID());
                        }
                    }
                    if(application.MOT_SETTING_NOTIFICATION_BARCODE && !notificaton_processed) {
                        String scannerName ="";
                        if (mScannerInfoList != null) {
                            for (DCSScannerInfo ex_info : mScannerInfoList) {
                                if (ex_info.getScannerID() == barcode.getFromScannerID()) {
                                    scannerName = ex_info.getScannerName();
                                    break;
                                }
                            }
                        }

                    }
                    if(!application.isFirmwareUpdateInProgress) {
                        if (application.CurScannerId != application.SCANNER_ID_NONE) {
                            Intent intent = new Intent("activities.ActiveScannerActivity");
                            //intent.setComponent(new ComponentName(getPackageName(),ActiveScannerActivity.class.getName()));
                            intent.putExtra(Constants.SCANNER_NAME, application.CurScannerName);
                            intent.putExtra(Constants.SCANNER_ADDRESS, application.CurScannerAddress);
                            intent.putExtra(Constants.SCANNER_ID, application.CurScannerId);
                            intent.putExtra(Constants.AUTO_RECONNECTION, application.CurAutoReconnectionState);
                            intent.putExtra(Constants.SHOW_BARCODE_VIEW, true);
                            intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                            //getApplicationContext().startActivity(intent);
                        }
                    }

                    break;
                case Constants.SESSION_ESTABLISHED:
                    DCSScannerInfo activeScanner=(DCSScannerInfo)msg.obj;
                    notificaton_processed = false;
                    result = false;
                    //ScannersActivity.curAvailableScanner = new AvailableScanner(activeScanner);
                    //ScannersActivity.curAvailableScanner.setConnected(true);
                    setAutoReconnectOption(activeScanner.getScannerID(),true);
                    /* notify connections delegates */
                    if (mDevConnDelegates != null) {
                        for (IScannerAppEngineDevConnectionsDelegate delegate : mDevConnDelegates) {
                            if (delegate != null) {
                                result = delegate.scannerHasConnected(activeScanner.getScannerID());
                                if (result) {
                                /*
                                 DevConnections delegates should NOT display any UI alerts,
                                 so from UI notification side the event is not processed
                                 */
                                    notificaton_processed = false;
                                }
                            }
                        }
                    }

                    /* update dev list */
                    found = false;
                    if (mScannerInfoList != null) {
                        for (DCSScannerInfo ex_info : mScannerInfoList) {
                            if (ex_info.getScannerID() == activeScanner.getScannerID()) {
                                mScannerInfoList.remove(ex_info);
                                application.barcodeData.clear();
                                found = true;
                                break;
                            }
                        }
                    }

                    if (mOfflineScannerInfoList != null) {
                        for (DCSScannerInfo off_info : mOfflineScannerInfoList) {
                            if (off_info.getScannerID() == activeScanner.getScannerID()) {
                                mOfflineScannerInfoList.remove(off_info);
                                break;
                            }
                        }
                    }

                    if (mScannerInfoList != null)
                        mScannerInfoList.add(activeScanner);

                    /* notify dev list delegates */
                    if (application.mDevListDelegates != null) {
                        for (IScannerAppEngineDevListDelegate delegate : application.mDevListDelegates) {
                            if (delegate != null) {
                                result = delegate.scannersListHasBeenUpdated();
                                if (result) {
                                    /*
                                     DeList delegates should NOT display any UI alerts,
                                     so from UI notification side the event is not processed
                                     */
                                    notificaton_processed = false;
                                }
                            }
                        }
                    }

                    //TODO - Showing notifications in foreground and background mode

                    if(application.MOT_SETTING_NOTIFICATION_ACTIVE && !notificaton_processed) {
                        StringBuilder notification_Msg = new StringBuilder();
                        if (!found) {
                            notification_Msg.append(activeScanner.getScannerName()).append(" has appeared and connected");
                        } else {
                            notification_Msg.append(activeScanner.getScannerName()).append(" has connected");
                        }
                    }
                    break;
                case Constants.SESSION_TERMINATED:
                    int scannerID=(Integer)msg.obj;
                    String scannerName = "";
                    notificaton_processed = false;
                    result = false;

                    /* notify connections delegates */
                    for (IScannerAppEngineDevConnectionsDelegate delegate : mDevConnDelegates) {
                        if (delegate != null) {
                            result = delegate.scannerHasDisconnected(scannerID);
                            if (result) {
                            /*
                             DevConnections delegates should NOT display any UI alerts,
                             so from UI notification side the event is not processed
                             */
                                notificaton_processed = false;
                            }
                        }
                    }

                    DCSScannerInfo scannerInfo = getScannerByID(scannerID);
                    mOfflineScannerInfoList.add(scannerInfo);
                    if(scannerInfo!=null) {
                        scannerName =scannerInfo.getScannerName();
                        //ScannersActivity.curAvailableScanner = null;
                    }
                    updateScannersList();

                    /* notify dev list delegates */
                    for (IScannerAppEngineDevListDelegate delegate : application.mDevListDelegates) {
                        if (delegate != null) {
                            result = delegate.scannersListHasBeenUpdated();
                            if (result) {
                                /*
                                 DeList delegates should NOT display any UI alerts,
                                 so from UI notification side the event is not processed
                                 */
                                notificaton_processed = false;
                            }
                        }
                    }
                    if(application.MOT_SETTING_NOTIFICATION_ACTIVE && !notificaton_processed) {

                    }
                    break;
                case Constants.SCANNER_APPEARED:
                case Constants.AUX_SCANNER_CONNECTED:
                    notificaton_processed = false;
                    result = false;
                    DCSScannerInfo availableScanner=(DCSScannerInfo)msg.obj;

                    /* notify connections delegates */
                    for (IScannerAppEngineDevConnectionsDelegate delegate : mDevConnDelegates) {
                        if (delegate != null) {
                            result = delegate.scannerHasAppeared(availableScanner.getScannerID());
                            if (result) {
                            /*
                             DevConnections delegates should NOT display any UI alerts,
                             so from UI notification side the event is not processed
                             */
                                notificaton_processed = false;
                            }
                        }
                    }

                    /* update dev list */
                    for (DCSScannerInfo ex_info : mScannerInfoList) {
                        if (ex_info.getScannerID() == availableScanner.getScannerID()) {
                            mScannerInfoList.remove(ex_info);
                            break;
                        }
                    }

                    mScannerInfoList.add(availableScanner);

                    /* notify dev list delegates */
                    for (IScannerAppEngineDevListDelegate delegate : application.mDevListDelegates) {
                        if (delegate != null) {
                            result = delegate.scannersListHasBeenUpdated();
                            if (result) {
                            /*
                             DeList delegates should NOT display any UI alerts,
                             so from UI notification side the event is not processed
                             */

                                notificaton_processed = false;
                            }
                        }
                    }

                    //TODO - Showing notifications in foreground and background mode

                    break;
                case Constants.SCANNER_DISAPPEARED:
                    notificaton_processed = false;
                    result = false;
                    scannerID=(Integer)msg.obj;
                    scannerName = "";
                    /* notify connections delegates */
                    for (IScannerAppEngineDevConnectionsDelegate delegate : mDevConnDelegates) {
                        if (delegate != null) {
                            result = delegate.scannerHasDisappeared(scannerID);
                            if (result) {
                            /*
                             DevConnections delegates should NOT display any UI alerts,
                             so from UI notification side the event is not processed
                             */
                                notificaton_processed = false;
                            }
                        }
                    }

                    /* update dev list */
                    found = false;
                    for (DCSScannerInfo ex_info : mScannerInfoList) {
                        if (ex_info.getScannerID() == scannerID) {
                            /* find scanner with ID in dev list */
                            mScannerInfoList.remove(ex_info);
                            scannerName = ex_info.getScannerName();
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        for (DCSScannerInfo off_info : mOfflineScannerInfoList) {
                            if (off_info.getScannerID() == scannerID) {
                                scannerName = off_info.getScannerName();
                                break;
                            }
                        }
                        logAsMessage(TYPE_DEBUG, TAG, "ScannerAppEngine:dcssdkEventScannerDisappeared: scanner is not in list");
                    }

                    /* notify dev list delegates */
                    for (IScannerAppEngineDevListDelegate delegate : application.mDevListDelegates) {
                        if (delegate != null) {
                            result = delegate.scannersListHasBeenUpdated();
                            if (result) {
                            /*
                             DeList delegates should NOT display any UI alerts,
                             so from UI notification side the event is not processed
                             */
                                notificaton_processed = false;
                            }
                        }
                    }

                    break;
            }
        }
    };


    private final Handler UpdateScannersHandler = new Handler() {

        public void handleMessage(Message msg) {
            updateScannersList();
            scannersList.clear();
            lastConnectedScannerList.clear();
            boolean enableLastScannerConnection = false;
            if (com.rfid.scannercontrol.app.application.Application.lastConnectedScanner != null) {
                DCSScannerInfo device = com.rfid.scannercontrol.app.application.Application.lastConnectedScanner;
                addToLastConnectedScannerList(new AvailableScanner(device.getScannerID(), device.getScannerName(), device.getScannerHWSerialNumber(), false, device.isAutoCommunicationSessionReestablishment(), device.getConnectionType()));
            }
            for (DCSScannerInfo device : getActualScannersList()) {
                if (device.isActive()) {
                    AvailableScanner availableScanner = new AvailableScanner(device.getScannerID(), device.getScannerName(), device.getScannerHWSerialNumber(), true, device.isAutoCommunicationSessionReestablishment(), device.getConnectionType());
                    com.rfid.scannercontrol.app.application.Application.currentConnectedScanner = device;
                    com.rfid.scannercontrol.app.application.Application.lastConnectedScanner = com.rfid.scannercontrol.app.application.Application.currentConnectedScanner;
                    availableScanner.setIsConnectable(true);
                    addToLastConnectedScannerList(availableScanner);
                    enableLastScannerConnection = true;
                } else {
                    AvailableScanner availableScanner = new AvailableScanner(device.getScannerID(), device.getScannerName(), device.getScannerHWSerialNumber(), false, device.isAutoCommunicationSessionReestablishment(), device.getConnectionType());
                    availableScanner.setIsConnectable(true);
                    addToAvailableScannerList(availableScanner);
                }
            }

            Collections.sort(scannersList);

            AvailableScanner currentConnectedScannerX = null;

            if (com.rfid.scannercontrol.app.application.Application.lastConnectedScanner != null) {
                AvailableScanner lastConnectedScanner = null;
                for (AvailableScanner scanner : scannersList) {
                    if (scanner.getScannerId() == com.rfid.scannercontrol.app.application.Application.lastConnectedScanner.getScannerID()) {
                        lastConnectedScanner = scanner;
                        scanner.setIsConnectable(true);
                        addToLastConnectedScannerList(scanner);
                        enableLastScannerConnection = true;
                        break;
                    }

                    if (com.rfid.scannercontrol.app.application.Application.currentConnectedScanner != null) {
                        if (com.rfid.scannercontrol.app.application.Application.currentConnectedScanner.getScannerHWSerialNumber().equals(scanner.getScannerAddress())) {
                            currentConnectedScannerX = scanner;
                        }
                    }
                }
                if (lastConnectedScanner != null) {
                    scannersList.remove(lastConnectedScanner);
                }
            }

            if (currentConnectedScannerX != null) {
                scannersList.remove(currentConnectedScannerX);
            }

/*
            if(lastConnectedScannerListAdapter.getCount() == 0){
                tblROwLastScannerConnected.setVisibility(View.GONE);
            }else{
                tblROwLastScannerConnected.setVisibility(View.VISIBLE);
            }*/
            /*TextView lastConnectedScannerTxt = (TextView) findViewById(R.id.txt_last_connected_scanner);
            if (lastConnectedScannerTxt != null) {
                lastConnectedScannerTxt.setText("Last Connected Scanner");
            }*/

            if (lastConnectedScannerList.size() > 0) {
                if (lastConnectedScannerList.get(0).isConnected()) {
                    if (msg.what == EVENT) {
                        if (cmdExecTask == null || AsyncTask.Status.RUNNING != cmdExecTask.getStatus()) {
                            com.rfid.scannercontrol.app.application.Application.CurScannerName = lastConnectedScannerList.get(0).getScannerName();
                            com.rfid.scannercontrol.app.application.Application.CurScannerAddress = lastConnectedScannerList.get(0).getScannerAddress();
                            com.rfid.scannercontrol.app.application.Application.CurScannerId = lastConnectedScannerList.get(0).getScannerId();
                            com.rfid.scannercontrol.app.application.Application.CurAutoReconnectionState = lastConnectedScannerList.get(0).isAutoReconnection();
                            /*Intent intent = new Intent(ScannersActivity.this, ActiveScannerActivity.class);
                            intent.putExtra(com.rfid.scannercontrol.app.helpers.Constants.SCANNER_NAME, com.rfid.scannercontrol.app.application.Application.CurScannerName);
                            intent.putExtra(com.rfid.scannercontrol.app.helpers.Constants.SCANNER_ADDRESS, com.rfid.scannercontrol.app.application.Application.CurScannerAddress);
                            intent.putExtra(com.rfid.scannercontrol.app.helpers.Constants.SCANNER_ID, com.rfid.scannercontrol.app.application.Application.CurScannerId);
                            intent.putExtra(com.rfid.scannercontrol.app.helpers.Constants.AUTO_RECONNECTION, Application.CurAutoReconnectionState);
                            intent.putExtra(com.rfid.scannercontrol.app.helpers.Constants.CONNECTED, true);
                            intent.putExtra(Constants.SHOW_BARCODE_VIEW, false);
                            startActivity(intent);*/
                        }
                    }
                }
            }

            ///sasaki
            if (getActualScannersList().size() > 0 && scannersList.size() > 0) {
                ConnectToScanner(scannersList.get(0));
            }
        }
    };

    private void addToAvailableScannerList(AvailableScanner availableScanner) {
        if (!scannersList.contains(availableScanner)) {
            scannersList.add(availableScanner);
        }
    }

    private void addToLastConnectedScannerList(AvailableScanner availableScanner) {
        lastConnectedScannerList.clear();
        lastConnectedScannerList.add(availableScanner);
    }

    private void ConnectToScanner(AvailableScanner availableScanner) {
        for (DCSScannerInfo device : getActualScannersList()) {
            if (device.getScannerID() == availableScanner.getScannerId()) {
                availableScanner.setIsAutoReconnection(device.isAutoCommunicationSessionReestablishment());
            }
        }
        if (availableScanner != null) {
            if (!availableScanner.isConnected()) {

                if ((curAvailableScanner != null) && (!availableScanner.getScannerAddress().equals(curAvailableScanner.getScannerAddress()))) {
                    if (curAvailableScanner.isConnected())
                        disconnect(curAvailableScanner.getScannerId());
                }
                cmdExecTask = new MyAsyncTask(availableScanner);
                cmdExecTask.execute();

            } else {
                curAvailableScanner = availableScanner;
                if (curAvailableScanner.isConnected()) {

                    application.CurScannerName = availableScanner.getScannerName();
                    application.CurScannerAddress = availableScanner.getScannerAddress();
                    application.CurAutoReconnectionState = availableScanner.isAutoReconnection();
                    application.CurScannerId = availableScanner.getScannerId();
                }
            }
        }
    }

    private class MyAsyncTask extends AsyncTask<Void, AvailableScanner, Boolean> {
        private AvailableScanner scanner;

        public MyAsyncTask(AvailableScanner scn) {
            this.scanner = scn;
        }

        @Override
        protected void onPreExecute() {
            super.onPreExecute();
        }

        @Override
        protected Boolean doInBackground(Void... voids) {
            DCSSDKDefs.DCSSDK_RESULT result = connect(scanner.getScannerId());
            if (result == DCSSDKDefs.DCSSDK_RESULT.DCSSDK_RESULT_SUCCESS) {
                curAvailableScanner = scanner;
                curAvailableScanner.setConnected(true);
                return true;
            } else {
                curAvailableScanner = null;
                return false;
            }
        }

        @Override
        protected void onPostExecute(Boolean b) {
            super.onPostExecute(b);
            if (progressDialog != null && progressDialog.isShowing())
                progressDialog.dismiss();
            if (!b) {
                context.doConnectResult(RESULT_CANCELED, "Unable to communicate with scanner");
                scannersListHasBeenUpdated();
            }

        }
    }

    public boolean scannersListHasBeenUpdated() {

        UpdateScannerListView(EVENT);

        return true;
    }

    public void UpdateScannerListView(int what) {

        Message msg = new Message();
        msg.what = what;
        UpdateScannersHandler.sendMessage(msg);

    }

    /**
     * アクティビティを上書きする
     * @param activity
     */
    public void setActivity(BaseRFIDActivity activity) {
        context = activity;
    }

    interface ResponseHandlerInterface {
        void handleBarcodeReceived(String strReadData);
        void doConnectResult(int ResultCode, String strMessage);
    }

}
