package com.rfid.common;

import android.os.AsyncTask;
import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;
import android.text.TextUtils;
import android.util.Log;
import android.util.Xml;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.symbol.emdk.EMDKManager;
import com.symbol.emdk.EMDKManager.EMDKListener;
import com.symbol.emdk.EMDKResults;
import com.symbol.emdk.ProfileManager;
import com.symbol.emdk.barcode.BarcodeManager;
import com.symbol.emdk.barcode.ScanDataCollection;
import com.symbol.emdk.barcode.Scanner;
import com.symbol.emdk.barcode.Scanner.DataListener;
import com.symbol.emdk.barcode.Scanner.StatusListener;
import com.symbol.emdk.barcode.ScannerConfig;
import com.symbol.emdk.barcode.ScannerException;
import com.symbol.emdk.barcode.ScannerResults;
import com.symbol.emdk.barcode.StatusData;

import org.xmlpull.v1.XmlPullParser;
import org.xmlpull.v1.XmlPullParserException;

import java.io.StringReader;
import java.util.ArrayList;

public class BarcodeZebraHandler implements EMDKListener, StatusListener, DataListener{
    // Declare a variable to store EMDKManager object
    private EMDKManager emdkManager = null;
    private ProfileManager profileManager = null;
    private String profileName = "ClockProfile-1";
    private String parmName = "";
    private String errorType = "";

    // Declare a variable to store Barcode Manager object
    private BarcodeManager barcodeManager = null;

    // Declare a variable to hold scanner device to scan
    private Scanner scanner = null;

    // Text view to display status of EMDK and Barcode Scanning Operations
    private TextView statusTextView = null;

    // Edit Text used to display scanned barcode data
    private EditText dataView = null;

    // boolean flag to start scanning after scanner initialization
// Used in OnStatus callback to ensure scanner is idle before read() method is called
    private boolean startRead = false;

    private BaseRFIDActivity context;

    // Provides error description
    private String errorDescription = "";

    // Provides error string with type/name + description
    private String errorString = "";


    public void onCreate(BaseRFIDActivity activity) {

        this.context = activity;
// Requests the EMDKManager object. This is an asynchronous call and
// should be called from the main thread.
// The callback also will receive in the main thread without blocking
// it until the EMDK resources are ready.

        EMDKResults results = EMDKManager.getEMDKManager(activity, this);

// Check the return status of getEMDKManager() and update the
// status TextView accordingly.

        if (results.statusCode != EMDKResults.STATUS_CODE.SUCCESS) {
            updateStatus("EMDKManager object request failed!");
            return;
        } else {
            updateStatus("EMDKManager object initialization is in progress.......");
        }
    }

    @Override
    public void onClosed() {
        // The EMDK closed unexpectedly. Release all the resources.
        if (emdkManager != null) {
            emdkManager.release();
            emdkManager = null;
        }
        updateStatus("EMDK closed unexpectedly! Please close and restart the application.");
    }
    @Override
    public void onOpened(EMDKManager emdkManager) {
// Get a reference to the EMDKManager

        this.emdkManager = emdkManager;

// Get a reference to the feature object - BarcodeManager

        initBarcodeManager();

// Initialize the scanner

        initScanner();
    }

    public void onDestroy() {
        // Release all the EMDK resources
        if (emdkManager != null) {
            emdkManager.release();
            emdkManager = null;
        }
    }

    @Override
    public void onData(ScanDataCollection scanDataCollection) {
// The ScanDataCollection object gives scanning result and
// the collection of ScanData. So check the data and its status.
        String dataStr = "";
        if ((scanDataCollection != null) && (scanDataCollection.getResult() == ScannerResults.SUCCESS)) {
            ArrayList<ScanDataCollection.ScanData> scanData = scanDataCollection.getScanData();
            // Iterate through scanned data and prepare the dataStr
            for (ScanDataCollection.ScanData data : scanData) {
                // Get the scanned data
                String barcodeData = data.getData();
                // Get the type of label being scanned
                ScanDataCollection.LabelType labelType = data.getLabelType();
                // Concatenate barcode data and label type
                dataStr = barcodeData;
            }

// Updates EditText with scanned data and type of label on UI thread.
            updateData(dataStr);


        }
    }
    @Override
    public void onStatus(StatusData statusData) {
        // The status is returned on multiple cases. Check the state and take the action.
// Get the current state of scanner in background.

        StatusData.ScannerStates state = statusData.getState();
        String statusStr = "";

// Different states of Scanner

        switch (state) {
            case IDLE:
                // Scanner is idle and ready to change configuration and submit read.
                statusStr = statusData.getFriendlyName()+" is enabled and idle...";

                // Change scanner configuration. This should be done while the scanner is in IDLE state.
                setConfig();

                try {
                    // Starts an asynchronous Scan. The method will NOT turn ON the
                    // scanner beam, but puts it in a state in which the scanner
                    // can be turned on automatically or by pressing a hardware trigger.

                    scanner.read();
                } catch (ScannerException e) {
                    updateStatus(e.getMessage());
                }

                break;
            case WAITING:
                // Scanner is waiting for trigger press to scan...
                statusStr = "Scanner is waiting for trigger press...";
                break;
            case SCANNING:
                // Scanning is in progress...
                statusStr = "Scanning...";
                break;
            case DISABLED:
                // Scanner is disabled
                statusStr = statusData.getFriendlyName()+" is disabled.";
                break;
            case ERROR:
                // Error has occurred during scanning
                statusStr = "An error has occurred.";
                break;
            default:
                break;


        }

        // Update TextView with scanner state on UI thread.
        updateStatus(statusStr);

    }
    // Method to initialize and enable Scanner and its listeners
    private void initializeScanner() throws ScannerException {
        if (scanner == null) {
            // Get the Barcode Manager object
            barcodeManager = (BarcodeManager) this.emdkManager
                    .getInstance(EMDKManager.FEATURE_TYPE.BARCODE);
            // Get default scanner defined on the device
            scanner = barcodeManager.getDevice(BarcodeManager.DeviceIdentifier.DEFAULT);
            // Add data and status listeners
            scanner.addDataListener(this);
            scanner.addStatusListener(this);
            // Hard trigger. When this mode is set, the user has to manually
            // press the trigger on the device after issuing the read call.
            scanner.triggerType = com.symbol.emdk.barcode.Scanner.TriggerType.HARD;
            // Enable the scanner
            scanner.enable();
            scanner.read();
            //set startRead flag to true. this flag will be used in the OnStatus callback to insure
            //the scanner is at an IDLE state and a read is not pending before calling scanner.read()
            startRead = true;
        }
    }

    private void initBarcodeManager() {

// Get the feature object such as BarcodeManager object for
// accessing the feature.

        barcodeManager = (BarcodeManager) emdkManager.getInstance(EMDKManager.FEATURE_TYPE.BARCODE);

// Add external scanner connection listener

        if (barcodeManager == null) {
            //Toast.makeText(this, "Barcode scanning is not supported.", Toast.LENGTH_LONG).show();
            //finish();
        }
    }

    private void initScanner() {
        if (scanner == null) {

            // Get default scanner defined on the device

            scanner = barcodeManager.getDevice(BarcodeManager.DeviceIdentifier.DEFAULT);
            if (scanner != null) {

                scanner.addDataListener(this);
                scanner.addStatusListener(this);

                scanner.triggerType = Scanner.TriggerType.HARD;
                try {
                    scanner.enable();
                    scanner.read();
                } catch (ScannerException e) {
                    updateStatus(e.getMessage());
                    deInitScanner();
                }
            } else {
                updateStatus("Failed to initialize the scanner device.");
            }
        }


    }

    public void disable() {
        try {
            scanner.disable();
        } catch (ScannerException e) {
            e.printStackTrace();        }
    }

    public void enable() {
        try {
            scanner.enable();
        } catch (ScannerException e) {
            e.printStackTrace();
        }
    }

    private void deInitScanner() {
        if (scanner != null) {
            try {
                // Release the scanner
                scanner.release();
            } catch (Exception e) {
                updateStatus(e.getMessage());
            }
            scanner = null;
        }
    }

    // Variable to hold scan data length

    private int dataLength = 0;
    private void updateData(final String result) {
        context.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                // Update the dataView EditText on UI thread with barcode data and its label type
                /* 20200408 Del Sasaki
                if (dataLength++ >= 50) {
                    // Clear the cache after 50 scans
                    dataView.getText().clear();
                     dataLength = 0;
                }*/

                //サブクラスで実装する
                context.handleBarcodeReceived(result);
                Log.d("barcode_read",result);
            }
        });
    }


    private void updateStatus(final String status) {
        context.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                // Update the status text view on UI thread with current scanner state
                //statusTextView.setText("" + status);
            }
        });
    }

    private void setConfig() {
        if (scanner != null) {
            try {
                // Get scanner config
                ScannerConfig config = scanner.getConfig();

                // Enable haptic feedback
                if (config.isParamSupported("config.scanParams.decodeHapticFeedback")) {
                    config.scanParams.decodeHapticFeedback = true;
                }

                // Set scanner config
                scanner.setConfig(config);
            } catch (ScannerException e) {
                updateStatus(e.getMessage());
            }
        }
    }

    public void UpdateProfile(String modifyData) {
        new BarcodeZebraHandler.ProcessProfileTask().execute(modifyData);
    }

    public class ProcessProfileTask extends AsyncTask<String, Void, EMDKResults> {

        @Override
        protected EMDKResults doInBackground(String... params) {
            profileManager = (ProfileManager) emdkManager.getInstance(EMDKManager.FEATURE_TYPE.PROFILE);
            //Call process profile to modify the profile of specified profile name
            EMDKResults results = profileManager.processProfile(profileName, ProfileManager.PROFILE_FLAG.SET, params);

            return results;
        }

        @Override
        protected void onPostExecute(EMDKResults results) {

            super.onPostExecute(results);

            String resultString = "";

            //Check the return status of processProfile
            if(results.statusCode == EMDKResults.STATUS_CODE.CHECK_XML) {

                // Get XML response as a String
                String statusXMLResponse = results.getStatusString();

                try {
                    // Create instance of XML Pull Parser to parse the response
                    XmlPullParser parser = Xml.newPullParser();
                    // Provide the string response to the String Reader that reads
                    // for the parser
                    parser.setInput(new StringReader(statusXMLResponse));
                    // Call method to parse the response
                    parseXML(parser);

                    if ( TextUtils.isEmpty(parmName) && TextUtils.isEmpty(errorType) && TextUtils.isEmpty(errorDescription) ) {
                        resultString = "Profile update success.";
                    }
                    else {

                        resultString = "Profile update failed." + errorString;
                    }

                } catch (XmlPullParserException e) {
                    resultString =  e.getMessage();
                }
            }

            //statusTextView.setText(resultString);
        }
    }

    // Method to parse the XML response using XML Pull Parser
    public void parseXML(XmlPullParser myParser) {
        int event;
        try {
            // Retrieve error details if parm-error/characteristic-error in the response XML
            event = myParser.getEventType();
            while (event != XmlPullParser.END_DOCUMENT) {
                String name = myParser.getName();
                switch (event) {
                    case XmlPullParser.START_TAG:

                        if (name.equals("parm-error")) {
                            parmName = myParser.getAttributeValue(null, "name");
                            errorDescription = myParser.getAttributeValue(null, "desc");
                            errorString = " (Name: " + parmName + ", Error Description: " + errorDescription + ")";
                            return;
                        }
                        if (name.equals("characteristic-error")) {
                            errorType = myParser.getAttributeValue(null, "type");
                            errorDescription = myParser.getAttributeValue(null, "desc");
                            errorString = " (Type: " + errorType + ", Error Description: " + errorDescription + ")";
                            return;
                        }
                        break;
                    case XmlPullParser.END_TAG:

                        break;
                }
                event = myParser.next();
            }

        } catch (Exception e) {
            e.printStackTrace();
        }
    }


    /**
     * アクティビティを上書きする
     * @param activity
     */
    public void setActivity(BaseRFIDActivity activity) {
        context = activity;
    }

}
