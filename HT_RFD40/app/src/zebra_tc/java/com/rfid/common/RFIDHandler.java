package com.rfid.common;

import android.os.AsyncTask;
import android.util.Log;
import android.widget.TextView;

import com.zebra.rfid.api3.ACCESS_OPERATION_CODE;
import com.zebra.rfid.api3.ACCESS_OPERATION_STATUS;
import com.zebra.rfid.api3.Antennas;
import com.zebra.rfid.api3.BEEPER_VOLUME;
import com.zebra.rfid.api3.DYNAMIC_POWER_OPTIMIZATION;
import com.zebra.rfid.api3.ENUM_TRANSPORT;
import com.zebra.rfid.api3.ENUM_TRIGGER_MODE;
import com.zebra.rfid.api3.HANDHELD_TRIGGER_EVENT_TYPE;
import com.zebra.rfid.api3.INVENTORY_STATE;
import com.zebra.rfid.api3.InvalidUsageException;
import com.zebra.rfid.api3.MEMORY_BANK;
import com.zebra.rfid.api3.OperationFailureException;
import com.zebra.rfid.api3.RFIDReader;
import com.zebra.rfid.api3.ReaderDevice;
import com.zebra.rfid.api3.Readers;
import com.zebra.rfid.api3.RfidEventsListener;
import com.zebra.rfid.api3.RfidReadEvents;
import com.zebra.rfid.api3.RfidStatusEvents;
import com.zebra.rfid.api3.SESSION;
import com.zebra.rfid.api3.SL_FLAG;
import com.zebra.rfid.api3.START_TRIGGER_TYPE;
import com.zebra.rfid.api3.STATUS_EVENT_TYPE;
import com.zebra.rfid.api3.STOP_TRIGGER_TYPE;
import com.zebra.rfid.api3.TAG_FIELD;
import com.zebra.rfid.api3.TagAccess;
import com.zebra.rfid.api3.TagData;
import com.zebra.rfid.api3.TagStorageSettings;
import com.zebra.rfid.api3.TriggerInfo;

import java.util.ArrayList;

class RFIDHandler implements Readers.RFIDReaderEventHandler{

    final static String TAG = "RFID_AM";
    // RFID Reader
    private static Readers readers;
    public static ArrayList<ReaderDevice> availableRFIDReaderList;
    private static ReaderDevice readerDevice;
    public static RFIDReader reader;
    private EventHandler eventHandler;

    private AsyncTask<Void, Void, Boolean> DisconnectTask;

    private DispMode dispMode = DispMode.Init;
    private String tagId;
    private boolean tagIsNew;
    private enum DispMode {
        Init,
        TagReaded,
        TagWrited
    }

    // UI and context
    TextView textView;
    // general
    private int MAX_POWER = 270;
    // In case of RFD8500 change reader name with intended device below from list of paired RFD8500
    String readername = "RFD8500123";
    private BaseRFIDActivity context;

    private String SearchTagID = "";
    private TagData _data = null;


    void onCreate(BaseRFIDActivity activity) {
        // application context
        context = activity;
        // SDK
        InitSDK();
    }

    /**
     * 接続状態を取得する
     * @return
     */
    public boolean isReaderConnected() {
        boolean blnReturn = false;

        if (reader != null) {
            if (reader.isConnected()) {
                blnReturn = true;
            }
        } else {
            Log.d(TAG, "reader is not connected");
        }
        return blnReturn;
    }


    String onResume() {
        return connect();
    }

    void onPause() {
        disconnect();
    }

    void onDestroy() {
        dispose();
    }

    /**
     * RFIDりだー接続処理
     */
    public void InitSDK() {
        Log.d(TAG, "InitSDK");
        if (readers == null) {
            new CreateInstanceTask().execute();
        } else
            new ConnectionTask().execute();
    }

    /**
     * RFIDリーダー接続処理
     */
    private class CreateInstanceTask extends AsyncTask<Void, Void, Void> {
        @Override
        protected Void doInBackground(Void... voids) {
            Log.d(TAG, "CreateInstanceTask");
            // Based on support available on host device choose the reader type
            InvalidUsageException invalidUsageException = null;

            //readers = new Readers(context, ENUM_TRANSPORT.SERVICE_SERIAL);
            readers = new Readers(context, ENUM_TRANSPORT.ALL);
            try {
                availableRFIDReaderList = readers.GetAvailableRFIDReaderList();
            } catch (Exception ex) {
                Log.e(TAG, ex.getMessage());
            }

            if (invalidUsageException != null) {
                readers.Dispose();
                readers = null;
                if (readers == null) {
                    readers = new Readers(context, ENUM_TRANSPORT.BLUETOOTH);
                }
            }
            return null;
        }

        @Override
        protected void onPostExecute(Void aVoid) {
            super.onPostExecute(aVoid);
            new ConnectionTask().execute();
        }
    }

    /**
     * RFIDリーダー接続処理
     */
    private class ConnectionTask extends AsyncTask<Void, Void, String> {
        @Override
        protected String doInBackground(Void... voids) {
            Log.d(TAG, "ConnectionTask");
            GetAvailableReader();
            if (reader != null)
                return connect();
            return "Failed to find or connect reader";
        }

        @Override
        protected void onPostExecute(String result) {
            super.onPostExecute(result);
            context.OnScannerAppeared();
        }
    }

    /**
     * 利用可能なRFIDリーダーを取得する
     */
    public synchronized void GetAvailableReader() {
        Log.d(TAG, "GetAvailableReader");
        if (readers != null) {
            readers.attach(this);
            try {
                availableRFIDReaderList = readers.GetAvailableRFIDReaderList();
            } catch (Exception ex) {
                Log.e(TAG, ex.getMessage());
            }
            if (availableRFIDReaderList != null) {
                if (availableRFIDReaderList.size() != 0) {
                    // if single reader is available then connect it
                    if (availableRFIDReaderList.size() == 1) {
                        readerDevice = availableRFIDReaderList.get(0);
                        reader = readerDevice.getRFIDReader();
                    } else {
                        // search reader specified by name
                        for (ReaderDevice device : availableRFIDReaderList) {
                            if (device.getName().equals(readername)) {
                                readerDevice = device;
                                reader = readerDevice.getRFIDReader();
                            }
                        }
                    }
                }
            }
        }
    }

    /**
     * RFIDリーダーの接続状態を確認する
     * @return
     */
    public boolean BluetoothConnected() {
        boolean blnReturn = false;
        if (reader != null) {
            Log.d(TAG, "connect " + reader.getHostName());
            try {
                if (!reader.isConnected()) {
                    //reader.connect();
                } else {
                    reader.Config.getBeeperVolume();
                }
                blnReturn = true;
            } catch (InvalidUsageException e) {
                e.printStackTrace();
            } catch (OperationFailureException e) {
                e.getResults();
                //Connection以外のエラーは接続として返却
                if ("RFID_COMM_NO_CONNECTION".equals(e.getResults().toString()) || "RFID_API_COMMAND_TIMEOUT".equals(e.getResults().toString())) {
                //if ("RFID_COMM_NO_CONNECTION".equals(e.getResults().toString())) {
                } else {
                    blnReturn = true;
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
        return blnReturn;
    }

    /**
     * RFIDリーダー接続イベント
     * @param readerDevice
     */
    @Override
    public void RFIDReaderAppeared(ReaderDevice readerDevice) {
        Log.d(TAG, "RFIDReaderAppeared " + readerDevice.getName());
        new ConnectionTask().execute();
    }

    /**
     * RFIDリーダー切断イベント
     * @param readerDevice
     */
    @Override
    public void RFIDReaderDisappeared(ReaderDevice readerDevice) {
        Log.d(TAG, "RFIDReaderDisappeared " + readerDevice.getName());
        if (readerDevice.getName().equals(reader.getHostName()))
            disconnect();
    }


    /**
     * RFIDリーダーに接続する
     * @return
     */
    public synchronized String connect() {
        if (reader != null) {
            Log.d(TAG, "connect " + reader.getHostName());
            try {
                if (!reader.isConnected()) {
                    reader.connect();
                    return "";
                }
            } catch (InvalidUsageException e) {
                e.printStackTrace();
            } catch (OperationFailureException e) {
                e.printStackTrace();
                String des = e.getResults().toString();
                return des;
            }
        }
        return "";
    }

    /**
     * 読取設定
     * @param Intencity 電波強度
     * @param Session セッション
     * @param TagPopulation 読取幅
     */
    protected void ConfigureReader(Integer Intencity, String Session, Short TagPopulation) {

        Log.d(TAG, "ConfigureReader " + reader.getHostName());
        if (reader != null) {
            if (reader.isConnected()) {
                try {
                    // イベントハンドラー登録
                    if (eventHandler == null)
                        eventHandler = new EventHandler();
                    reader.Events.addEventsListener(eventHandler);

                    // イベント登録
                    reader.Events.setHandheldEvent(true);
                    reader.Events.setBatteryEvent(true);
                    reader.Events.setTagReadEvent(true);
                    reader.Events.setAttachTagDataWithReadEvent(false);

                    //トリガー設定
                    TriggerInfo triggerInfo = new TriggerInfo();
                    //triggerInfo.StartTrigger.setTriggerType(START_TRIGGER_TYPE.START_TRIGGER_TYPE_IMMEDIATE);     //ボタンでStart
                    //triggerInfo.StopTrigger.setTriggerType(STOP_TRIGGER_TYPE.STOP_TRIGGER_TYPE_TAG_OBSERVATION_WITH_TIMEOUT);//ボタンでStop
                    triggerInfo.StartTrigger.setTriggerType(START_TRIGGER_TYPE.START_TRIGGER_TYPE_IMMEDIATE);           //トリガーでStart
                    triggerInfo.StopTrigger.setTriggerType(STOP_TRIGGER_TYPE.STOP_TRIGGER_TYPE_IMMEDIATE);             //トリガーでStop
                    //20200203 Add Start Sasaki
                    try {
                        reader.Config.setStartTrigger(triggerInfo.StartTrigger);
                        reader.Config.setStopTrigger(triggerInfo.StopTrigger);
                    } catch (OperationFailureException op) {
                        //機種により、setStartTriggerでエラーとなる場合があるため、Sleepを入れて再設定する
                        op.printStackTrace();
                        try {
                            Thread.sleep(1000);
                        } catch (InterruptedException ie) {
                            ie.printStackTrace();
                        }
                        reader.Config.setStartTrigger(triggerInfo.StartTrigger);
                        reader.Config.setStopTrigger(triggerInfo.StopTrigger);
                    }
                    //20200203 Add End Sasaki

                    //二度読み防止設定
                    reader.Config.setUniqueTagReport(false);
                    reader.Config.setUniqueTagReport(true);

                    //電波強度設定
                    Antennas.AntennaRfConfig config = reader.Config.Antennas.getAntennaRfConfig(1);
                    config.setTransmitPowerIndex(Intencity);
                    config.setrfModeTableIndex(0);
                    config.setTari(0);
                    reader.Config.Antennas.setAntennaRfConfig(1, config);
                    Antennas.AntennaRfConfig rfConfig = reader.Config.Antennas.getAntennaRfConfig(1);
                    rfConfig.setTransmitPowerIndex(Intencity);

                    Antennas.SingulationControl s1_singulationControl = reader.Config.Antennas.getSingulationControl(1);
                    s1_singulationControl.setSession(getSession(Session));      //セッション
                    s1_singulationControl.setTagPopulation(TagPopulation);      //読取幅
                    s1_singulationControl.Action.setInventoryState(INVENTORY_STATE.INVENTORY_STATE_A);
                    s1_singulationControl.Action.setSLFlag(SL_FLAG.SL_ALL);
                    reader.Config.Antennas.setSingulationControl(1, s1_singulationControl);

                    // RFIDorバーコードのモードのモード設定
                    reader.Config.setTriggerMode(ENUM_TRIGGER_MODE.RFID_MODE, true);

                    //音量-なし
                    reader.Config.setBeeperVolume(BEEPER_VOLUME.QUIET_BEEP);

                    reader.Actions.PreFilters.deleteAll();
                    //
                } catch (InvalidUsageException | OperationFailureException e) {
                    e.printStackTrace();
                }
            }
        }
    }

    /**
     * バッテリー取得設定
     */
    protected void ConfigureBattery() throws InvalidUsageException , OperationFailureException{
        Log.d(TAG, "ConfigureBattery ");
        if (reader != null) {
            if (reader.isConnected()) {
                // イベントハンドラー登録
                if (eventHandler == null)
                    eventHandler = new EventHandler();
                reader.Events.addEventsListener(eventHandler);

                // バッテリー取得イベント登録
                reader.Events.setBatteryEvent(true);
                reader.Events.setHandheldEvent(true);
                //reader.Events.setReaderDisconnectEvent(true);

                //電池残量取得
                reader.Config.getDeviceStatus(true, false, false);

                //音量-なし
                reader.Config.setBeeperVolume(BEEPER_VOLUME.QUIET_BEEP);

                //トリガーモード
                reader.Config.setTriggerMode(ENUM_TRIGGER_MODE.RFID_MODE, true);
            }
        }
    }

    /**
     * タグ紐付用書込み設定
     */
    protected void ConfigureReaderWrite() throws InvalidUsageException, OperationFailureException{
        Log.d(TAG, "ConfigureReaderWrite " + reader.getHostName());
        if (reader != null) {
            if (reader.isConnected()) {
                // HH event
                reader.Events.setHandheldEvent(true);
                // tag event with tag data
                reader.Events.setTagReadEvent(true);
                reader.Events.setAttachTagDataWithReadEvent(false);
                reader.Config.setDPOState(DYNAMIC_POWER_OPTIMIZATION.DISABLE);
                // RFIDorバーコードのモードのモード設定
                reader.Config.setTriggerMode(ENUM_TRIGGER_MODE.RFID_MODE, true);

                //電波強度
                Antennas.AntennaRfConfig config = reader.Config.Antennas.getAntennaRfConfig(1);
                config.setTransmitPowerIndex(MAX_POWER);
                config.setrfModeTableIndex(0);
                config.setTari(0);
                reader.Config.Antennas.setAntennaRfConfig(1, config);
                Antennas.AntennaRfConfig rfConfig = reader.Config.Antennas.getAntennaRfConfig(1);
                rfConfig.setTransmitPowerIndex(MAX_POWER);

                //セッションの設定
                Antennas.SingulationControl s1_singulationControl = reader.Config.Antennas.getSingulationControl(1);
                s1_singulationControl.setSession(SESSION.SESSION_S0);
                s1_singulationControl.Action.setSLFlag(SL_FLAG.SL_ALL);
                reader.Config.Antennas.setSingulationControl(1, s1_singulationControl);
                reader.Config.setAccessOperationWaitTimeout(1000);
            }
        }
    }

    protected void ConfigureReaderSingleTrigger(Integer Intencity, String Session, Short TagPopulation) {
        Log.d(TAG, "ConfigureReaderSingleTrigger " + reader.getHostName());
        if (reader != null) {
            if (reader.isConnected()) {
                try {
                    if (eventHandler == null)
                        eventHandler = new EventHandler();
                    reader.Events.addEventsListener(eventHandler);
                    // HH event
                    reader.Events.setHandheldEvent(true);
                    // tag event with tag data
                    reader.Events.setTagReadEvent(true);
                    reader.Events.setAttachTagDataWithReadEvent(false);
                    // RFIDorバーコードのモードのモード設定
                    reader.Config.setTriggerMode(ENUM_TRIGGER_MODE.RFID_MODE, true);

                    //二度読み防止の設定
                    reader.Config.setUniqueTagReport(false);
                    reader.Config.setUniqueTagReport(true);

                    reader.Config.setDPOState(DYNAMIC_POWER_OPTIMIZATION.DISABLE);

                    //タグ内の処理するフィールドの範囲を設定
                    TagStorageSettings tagStorageSettings = reader.Config.getTagStorageSettings();
                    TAG_FIELD[] tagField = new TAG_FIELD[2];
                    tagField[0] = TAG_FIELD.PC;
                    tagField[1] = TAG_FIELD.PEAK_RSSI;
//                    tagField[2] = TAG_FIELD.TAG_SEEN_COUNT;
//                    tagField[3] = TAG_FIELD.CRC;
                    tagStorageSettings.setTagFields(tagField);
                    reader.Config.setTagStorageSettings(tagStorageSettings);

                    //電波強度
                    //MAX_POWER = reader.ReaderCapabilities.getTransmitPowerLevelValues().length - 1;
                    // set antenna configurations
                    Antennas.AntennaRfConfig config = reader.Config.Antennas.getAntennaRfConfig(1);
                    config.setTransmitPowerIndex(Intencity);
                    config.setrfModeTableIndex(0);
                    config.setTari(0);
                    reader.Config.Antennas.setAntennaRfConfig(1, config);
                    Antennas.AntennaRfConfig rfConfig = reader.Config.Antennas.getAntennaRfConfig(1);
                    rfConfig.setTransmitPowerIndex(Intencity);

                    //セッションの設定
                    Antennas.SingulationControl s1_singulationControl = reader.Config.Antennas.getSingulationControl(1);
                    s1_singulationControl.setSession(getSession(Session));
                    s1_singulationControl.setTagPopulation(TagPopulation);      //読取幅
                    s1_singulationControl.Action.setInventoryState(INVENTORY_STATE.INVENTORY_STATE_A);
                    s1_singulationControl.Action.setSLFlag(SL_FLAG.SL_ALL);
                    reader.Config.Antennas.setSingulationControl(1, s1_singulationControl);

                    //1回のトリガーで終了する
                    TriggerInfo triggerInfo = new TriggerInfo();
                    triggerInfo.StartTrigger.setTriggerType(START_TRIGGER_TYPE.START_TRIGGER_TYPE_HANDHELD);
                    triggerInfo.StopTrigger.setTriggerType(STOP_TRIGGER_TYPE.STOP_TRIGGER_TYPE_ACCESS_N_ATTEMPTS_WITH_TIMEOUT);
                    triggerInfo.StopTrigger.NumAttempts.setN((short) 1); // perform 3 rounds of inventory
                    triggerInfo.StopTrigger.NumAttempts.setTimeout(3000); //

                    reader.Config.Antennas.setSingulationControl(1, s1_singulationControl);

                    reader.Actions.PreFilters.deleteAll();
                } catch (InvalidUsageException | OperationFailureException e) {
                    e.printStackTrace();
                }
            }
        }
    }

    /**
     * 探索用読取設定
     * @param Intencity
     * @param Session
     * @param TagPopulation
     */
    protected void ConfigureReaderSearch(Integer Intencity, String Session, Short TagPopulation) {
        Log.d(TAG, "ConfigureReader " + reader.getHostName());
        if (reader != null) {
            if (reader.isConnected()) {
                try {
                    if (eventHandler == null)
                        eventHandler = new EventHandler();
                    reader.Events.addEventsListener(eventHandler);
                    // HH event
                    reader.Events.setHandheldEvent(true);
                    // tag event with tag data
                    reader.Events.setTagReadEvent(true);
                    reader.Events.setAttachTagDataWithReadEvent(false);

                    try {
                        // RFIDorバーコードのモードのモード設定
                        reader.Config.setTriggerMode(ENUM_TRIGGER_MODE.RFID_MODE, true);
                    } catch (OperationFailureException op) {
                        op.printStackTrace();
                    }

                    //二度読み防止の設定
                    reader.Config.setUniqueTagReport(false);

                    //電波強度
                    Antennas.AntennaRfConfig config = reader.Config.Antennas.getAntennaRfConfig(1);
                    config.setTransmitPowerIndex(Intencity);
                    config.setrfModeTableIndex(0);
                    config.setTari(0);
                    reader.Config.Antennas.setAntennaRfConfig(1, config);
                    Antennas.AntennaRfConfig rfConfig = reader.Config.Antennas.getAntennaRfConfig(1);
                    rfConfig.setTransmitPowerIndex(Intencity);

                    //セッションの設定
                    Antennas.SingulationControl s1_singulationControl = reader.Config.Antennas.getSingulationControl(1);
                    s1_singulationControl.setSession(getSession(Session));        //セッション
                    s1_singulationControl.setTagPopulation(TagPopulation);        //読取幅
                    s1_singulationControl.Action.setInventoryState(INVENTORY_STATE.INVENTORY_STATE_A);
                    s1_singulationControl.Action.setSLFlag(SL_FLAG.SL_ALL);
                    reader.Config.Antennas.setSingulationControl(1, s1_singulationControl);

                    //トリガーモード
                    TriggerInfo triggerInfo = new TriggerInfo();
                    triggerInfo.StartTrigger.setTriggerType(START_TRIGGER_TYPE.START_TRIGGER_TYPE_IMMEDIATE);           //トリガーでStart
                    triggerInfo.StopTrigger.setTriggerType(STOP_TRIGGER_TYPE.STOP_TRIGGER_TYPE_IMMEDIATE);             //トリガーでStop
                    reader.Config.setStartTrigger(triggerInfo.StartTrigger);
                    reader.Config.setStopTrigger(triggerInfo.StopTrigger);

                    reader.Config.Antennas.setSingulationControl(1, s1_singulationControl);

                    reader.Actions.PreFilters.deleteAll();
                } catch (InvalidUsageException | OperationFailureException e) {
                    e.printStackTrace();
                }
            }
        }
    }

    /**
     * 探索一覧用設定
     * @param Intencity
     * @param Session
     * @param TagPopulation
     * @throws InvalidUsageException
     * @throws OperationFailureException
     */
    protected void ConfigureReaderSearchSummary(Integer Intencity, String Session, Short TagPopulation) throws InvalidUsageException, OperationFailureException{
        Log.d(TAG, "ConfigureReaderSearchSummary " + reader.getHostName());
        if (reader != null) {
            if (reader.isConnected()) {
                reader.Events.setHandheldEvent(true);
                reader.Events.setTagReadEvent(true);
                reader.Events.setAttachTagDataWithReadEvent(false);
                reader.Config.setDPOState(DYNAMIC_POWER_OPTIMIZATION.DISABLE);
                reader.Config.setTriggerMode(ENUM_TRIGGER_MODE.RFID_MODE, true);
                //電波強度
                Antennas.AntennaRfConfig config = reader.Config.Antennas.getAntennaRfConfig(1);
                config.setTransmitPowerIndex(Intencity);
                config.setrfModeTableIndex(0);
                config.setTari(0);
                reader.Config.Antennas.setAntennaRfConfig(1, config);
                Antennas.AntennaRfConfig rfConfig = reader.Config.Antennas.getAntennaRfConfig(1);
                rfConfig.setTransmitPowerIndex(Intencity);

                //セッションの設定
                Antennas.SingulationControl s1_singulationControl = reader.Config.Antennas.getSingulationControl(1);
                s1_singulationControl.setSession(getSession(Session));        //セッション
                s1_singulationControl.setTagPopulation(TagPopulation);        //読取幅
                s1_singulationControl.Action.setSLFlag(SL_FLAG.SL_ALL);
                reader.Config.Antennas.setSingulationControl(1, s1_singulationControl);
                reader.Config.setAccessOperationWaitTimeout(1000);
            }
        }
    }

    /**
     * 切断処理
     */
    private synchronized void disconnect() {
        Log.d(TAG, "disconnect " + reader);
        try {
            if (reader != null) {
                reader.Events.removeEventsListener(eventHandler);
                reader.disconnect();
            }

            //サブクラスで実装する(切断イベント)
            context.onDisconnectEvent();
        } catch (InvalidUsageException e) {
            e.printStackTrace();
        } catch (OperationFailureException e) {
            e.printStackTrace();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    private synchronized void dispose() {
        try {
            if (readers != null) {
                reader = null;
                readers.Dispose();
                readers = null;
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * トリガーを引いた場合のイベント
     */
    synchronized void performInventory() {
        // check reader connection
        if (!isReaderConnected())
            return;
        try {
            reader.Actions.Inventory.perform();
        } catch (InvalidUsageException e) {
            e.printStackTrace();
        } catch (OperationFailureException e) {
            e.printStackTrace();
        }
    }

    /**
     * トリガーを引いた場合のイベント(探索用)
     * @param strTagID
     */
    synchronized  void performSearchInventory(String strTagID) {
        if (!isReaderConnected())
            return;
        try {
            SearchTagID = strTagID;
            reader.Actions.TagLocationing.Perform(strTagID, null, null);
        } catch (InvalidUsageException e) {
            e.printStackTrace();
        } catch (OperationFailureException e) {
            e.printStackTrace();
        }
    }

    /**
     * トリガーを離した場合のイベント
     */
    synchronized void stopInventory() {
        // check reader connection
        if (!isReaderConnected())
            return;
        try {
            reader.Actions.Inventory.stop();
        } catch (InvalidUsageException e) {
            e.printStackTrace();
        } catch (OperationFailureException e) {
            e.printStackTrace();
        }
    }

    /**
     * トリガーを離した場合のイベント(探索用)
     */
    synchronized  void stopSearchInventory() {
        if (!isReaderConnected())
            return;
        try {
            reader.Actions.TagLocationing.Stop();
        } catch (InvalidUsageException e) {
            e.printStackTrace();
        } catch (OperationFailureException e) {
            e.printStackTrace();
        }
    }

    /**
     * リスナー削除
     */
    public void removeEventsListener() {
        Log.d(TAG, "removeEventsListener " + reader);
        try {
            if (reader != null) {
                reader.Events.removeEventsListener(eventHandler);
            }
        } catch (InvalidUsageException e) {
            e.printStackTrace();
        } catch (OperationFailureException e) {
            e.printStackTrace();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * タグ取得用イベントハンドラー
     */
    public class EventHandler implements RfidEventsListener {
        // Read Event Notification
        public void eventReadNotify(RfidReadEvents e) {
            // Recommended to use new method getReadTagsEx for better performance in case of large tag population
            TagData[] myTags = reader.Actions.getReadTags(100);
            if (myTags != null) {
                for (int index = 0; index < myTags.length; index++) {
                    Log.d(TAG, "Tag ID " + myTags[index].getTagID());
                    if (myTags[index].getOpCode() == ACCESS_OPERATION_CODE.ACCESS_OPERATION_READ &&
                            myTags[index].getOpStatus() == ACCESS_OPERATION_STATUS.ACCESS_SUCCESS) {
                        if (myTags[index].getMemoryBankData().length() > 0) {
                            Log.d(TAG, " Mem Bank Data " + myTags[index].getMemoryBankData());
                        }
                    }

                    _data = myTags[index];
                    String strPcHex = Integer.toHexString(myTags[index].getPC());

                    if (myTags[index].isContainsLocationInfo()) {
                        //探索データ取得
                        short dist = myTags[index].LocationInfo.getRelativeDistance();
                        Log.d(TAG, "Tag relative distance " + dist);
                        //サブクラスで実装する
                        context.handleRfidReceived(SearchTagID, dist, strPcHex);
                    } else {
                        //探索以外のデータ取得
                        //サブクラスで実装する
                        context.handleRfidReceived(_data.getTagID(), myTags[index].getPeakRSSI(), strPcHex);
                    }

                }
                // possibly if operation was invoked from async task and still busy
                // handle tag data responses on parallel thread thus THREAD_POOL_EXECUTOR
                new AsyncDataUpdate().executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR, myTags);
            }
        }

        /**
         * イベント受信
         * @param rfidStatusEvents
         */
        public void eventStatusNotify(RfidStatusEvents rfidStatusEvents) {
            Log.d(TAG, "Status Notification: " + rfidStatusEvents.StatusEventData.getStatusEventType());
            //トリガーイベントの場合
            if (rfidStatusEvents.StatusEventData.getStatusEventType() == STATUS_EVENT_TYPE.HANDHELD_TRIGGER_EVENT) {
                if (rfidStatusEvents.StatusEventData.HandheldTriggerEventData.getHandheldEvent() == HANDHELD_TRIGGER_EVENT_TYPE.HANDHELD_TRIGGER_PRESSED) {
                    new AsyncTask<Void, Void, Void>() {
                        @Override
                        protected Void doInBackground(Void... voids) {
                            context.handleTriggerPress(true);
                            return null;
                        }
                    }.execute();
                }
                if (rfidStatusEvents.StatusEventData.HandheldTriggerEventData.getHandheldEvent() == HANDHELD_TRIGGER_EVENT_TYPE.HANDHELD_TRIGGER_RELEASED) {
                    new AsyncTask<Void, Void, Void>() {
                        @Override
                        protected Void doInBackground(Void... voids) {
                            context.handleTriggerPress(false);
                            return null;
                        }
                    }.execute();
                }

            //バッテリーイベントの場合
            } else if (rfidStatusEvents.StatusEventData.getStatusEventType() == STATUS_EVENT_TYPE.BATTERY_EVENT) {
                context.setBatteryData(rfidStatusEvents.StatusEventData.BatteryData);

            //コネクション切断イベントの場合
            } else if (rfidStatusEvents.StatusEventData.getStatusEventType() == STATUS_EVENT_TYPE.DISCONNECTION_EVENT){
                //DisconnectTask = new UpdateDisconnectedStatusTask(readerDevice.getName()).execute();
                context.onDisconnectEvent();
            }
        }
    }

    public boolean reConnect() {
        boolean blnReturn = false;

        try {
            if (reader != null && readerDevice != null) {
                reader.reconnect();
                //reader.connect();
                blnReturn = true;
            }
        } catch (InvalidUsageException e) {
            e.getMessage();
        } catch (OperationFailureException e) {
            e.getMessage();
        }
        return blnReturn;
    }

    private class AsyncDataUpdate extends AsyncTask<TagData[], Void, Void> {
        @Override
        protected Void doInBackground(TagData[]... params) {
            context.handleTagdata(params[0]);
            return null;
        }
    }

    /**
     * タグ書込み
     * @param strOldTagID
     * @param strNewTagID
     */
    protected void WriteTag(final String strOldTagID, final String strNewTagID, final boolean isPcOverWrite) throws InvalidUsageException, OperationFailureException{
        int offset = CommonConst.EPC_WORD_TYPE_START_POS;
        if (isPcOverWrite) {
            //PCの書換が必要な場合は、開始位置を変更する
            offset = CommonConst.PC_WORD_TYPE_START_POS;
        }

        TagData tagData = null;
        String tagId = strOldTagID;
        TagAccess tagAccess = new TagAccess();
        TagAccess.WriteAccessParams writeAccessParams = tagAccess.new WriteAccessParams();
        String writeData = strNewTagID; //write data in string
        writeAccessParams.setAccessPassword(0);
        writeAccessParams.setMemoryBank(MEMORY_BANK.MEMORY_BANK_EPC);
        writeAccessParams.setOffset(offset); // start writing from word offset 0
        writeAccessParams.setWriteData(writeData);
        writeAccessParams.setWriteRetries(3);
        writeAccessParams.setWriteDataLength(writeData.length() / 4);
        reader.Actions.TagAccess.writeWait(tagId, writeAccessParams, null, tagData, false, false);
    }

    /**
     * タグ読込
     * @param strNewTagID
     */
    public TagData ReadTag(String strNewTagID) throws InvalidUsageException, OperationFailureException{
        TagAccess tagAccess = new TagAccess();
        TagAccess.ReadAccessParams readAccessParams = tagAccess.new ReadAccessParams();
        TagData readAccessTag;
        readAccessParams.setAccessPassword(0);
        readAccessParams.setCount(4); // read 4 words
        readAccessParams.setMemoryBank(MEMORY_BANK.MEMORY_BANK_EPC);
        readAccessParams.setOffset(2); // start reading from word offset 0
        readAccessTag = reader.Actions.TagAccess.readWait(strNewTagID, readAccessParams, null);
        return readAccessTag;
    }

    /**
     * Activityを上書きする
     * @param activity
     */
    protected void setActivity(BaseRFIDActivity activity){
        context = activity;
    }


    private SESSION getSession(String Session) {
        //セッション設定
        SESSION sess;
        switch (Session) {
            case "0":
                sess = SESSION.SESSION_S0;
                break;
            case "1":
                sess = SESSION.SESSION_S1;
                break;
            case "2":
                sess = SESSION.SESSION_S2;
                break;
            default:
                sess = SESSION.SESSION_S0;
                break;
        }

        return sess;
    }

    interface ResponseHandlerInterface {
        void handleTagdata(TagData[] tagData);
        void handleTriggerPress(boolean pressed);
        //void handleStatusEvents(Events.StatusEventData eventData);
        void handleRfidReceived(String strReadData, final short tempRSSI, final String strPcHex);
        void OnScannerAppeared();
        void onDisconnectEvent();
    }

}
