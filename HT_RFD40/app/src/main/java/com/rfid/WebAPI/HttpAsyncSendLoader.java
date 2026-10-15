package com.rfid.WebAPI;

import android.app.Activity;
import android.os.AsyncTask;
import android.util.Log;
import android.widget.TextView;

import com.rfid.Factory.LogFactory;
import com.rfid.common.Common;
import com.rfidpk.R;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.rfid.common.CommonConst;
import com.rfid.common.SystemSetting;
import com.rfid.common.bean.EntryInventData;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.concurrent.CountDownLatch;


public class HttpAsyncSendLoader extends AsyncTask<String, Integer, Integer> {

    private String result = "";
    private String time;
    private String version;
    private String _senddata = null;
    private Activity _activity = null;
    private int _timeout = 30 * 1000;

    CountDownLatch _latch;

    /**
     * コンストラクタ
     */
    public HttpAsyncSendLoader(Activity activity, String senddata, int timeout) {
        super();
        _senddata = senddata;
        _activity = activity;
        _timeout = timeout;
    }

    /** Callback. */
    private Listener listener;

    private enum RETUEN_CODE {
        OK(0),                  //20210929 ADD Takase V2.0
        Error(1),
        Worning(2),
        Confirm(3),
        TagError(4),
        LeaseUpperErr(5),       //20200225 Add Sasaki V1.2
        LeaseUpperWarn(6),      //20200225 Add Sasaki V1.2
        LeaseReservationErr(7), //20210918 ADD Takase V2.0
        MatchErr(8),
        LockErr(9);
        private final int text;
        private RETUEN_CODE(final int text) {
            this.text = text;
        }
        public int getInt() {
            return this.text;
        }
    }

    /**
     * バックグランドで行う処理
     */
    @Override
    protected Integer doInBackground(String... value) {
        URL url = null;
        HttpURLConnection conn = null;

        try {
            byte[] sendJson = null;
            if (_senddata != null) {
                Log.d("senddata", _senddata);
                //送信データ
                sendJson = _senddata.getBytes("UTF-8");
            }

            //送信設定
            url = new URL(value[0]);
            conn = (HttpURLConnection) url.openConnection();
            conn.setConnectTimeout(_timeout);
            conn.setReadTimeout(_timeout);
            conn.setRequestMethod("POST");
            conn.setInstanceFollowRedirects(false);
            conn.setRequestProperty("Accept-Language", "jp");
            conn.setRequestProperty("Content-Type", "application/json; charset=utf-8");
            if (sendJson != null) {
                conn.getOutputStream().write(sendJson);
                conn.getOutputStream().flush();
                conn.getOutputStream().close();
            }

            conn.connect();

            //リターンコードを取得
            int statusCode = conn.getResponseCode();

            //ステータス200の場合、正常
            if(statusCode == HttpURLConnection.HTTP_OK){

                //responseの読み込み
                final InputStream in = conn.getInputStream();
                String encoding = conn.getContentEncoding();
                if(null == encoding){
                    encoding = "UTF-8";
                }
                final InputStreamReader inReader = new InputStreamReader(in, encoding);
                final BufferedReader bufferedReader = new BufferedReader(inReader);
                String line = null;
                while((line = bufferedReader.readLine()) != null) {
                    result += line;
                }
                bufferedReader.close();
                inReader.close();
                in.close();
            }

            return statusCode;

        } catch (MalformedURLException e) {
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", _activity) + ":" + Common.toString(e), _activity);
            e.printStackTrace();
            return -1;
        } catch (IOException e){
            e.printStackTrace();
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", _activity) + ":" + Common.toString(e), _activity);
            return -1;
        } catch (Exception e){
            e.printStackTrace();
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", _activity) + ":" + Common.toString(e), _activity);
            return -1;
        }finally {
            if (conn != null) {
                // コネクションを切断
                conn.disconnect();
            }
        }
    }

    /**
     * バックグランド処理が完了し、UIスレッドに反映する
     * @param res
     */
    @Override
    protected void onPostExecute(Integer res) {

        try {
            //Web接続に失敗した場合
            if (res < 0) {
                listener.onConnectError(SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_CONNECT, "", _activity));
            } else {
                if (!"".equals(result)) {
                    //WebApiからデータを受信した場合
                    JSONObject json = new JSONObject(result);

                    String strErrCd = json.getString("wordCd");
                    int returnCd = json.getInt("returnCode");    //20210929 CHG Takase V2.0

                    //20210929 ADD Takase V2.0 Start
                    int intReturnCd = -1;
                    switch (returnCd) {
                        case 0:
                            intReturnCd = RETUEN_CODE.OK.getInt();
                            break;
                        case 1:
                            intReturnCd = RETUEN_CODE.Error.getInt();
                            if (!json.isNull("reserveList")) {
                                intReturnCd = RETUEN_CODE.LeaseReservationErr.getInt();
                            }
                            if (!json.isNull("assetTagList")) {
                                intReturnCd = RETUEN_CODE.TagError.getInt();
                            }
                            if (!json.isNull("assetLeaseList")) {
                                intReturnCd = RETUEN_CODE.LeaseUpperErr.getInt();
                            }
                            if (!json.isNull("outErrorList")) {
                                intReturnCd = RETUEN_CODE.MatchErr.getInt();
                            }
                            break;
                        case 2:
                            intReturnCd = RETUEN_CODE.Worning.getInt();
                            if (!json.isNull("assetLeaseList")) {
                                intReturnCd = RETUEN_CODE.LeaseUpperWarn.getInt();
                            }
                            break;
                        case 3:
                            intReturnCd = RETUEN_CODE.Confirm.getInt();
                            break;
                        case 5:
                            intReturnCd = RETUEN_CODE.LockErr.getInt();
                            break;
                        default:
                            intReturnCd = RETUEN_CODE.Error.getInt();
                            break;
                    }
                    //20210929 ADD Takase V2.0 End

                    if (intReturnCd == RETUEN_CODE.TagError.getInt()) {
                        //タグ書込みエラー
                        listener.onTagError(json);
                    //20200225 Add Start Sasaki V1.2
                    } else if (intReturnCd == RETUEN_CODE.LeaseUpperErr.getInt() ||
                                intReturnCd == RETUEN_CODE.LeaseUpperWarn.getInt()) {
                        //貸出上限越えの場合、警告メッセージを表示する(正常処理扱い)
                        listener.onLeaseError(json);
                    //20200225 Add End Sasaki V1.2
                    //20210918 ADD Takase V2.0 START
                    } else if (intReturnCd == RETUEN_CODE.LeaseReservationErr.getInt()) {
                        //対象の予約なしの場合、警告メッセージを表示する(正常処理扱い)
                        listener.onReservationError(json);
                    //20210918 ADD Takase V2.0 END
                    }else if (intReturnCd == RETUEN_CODE.MatchErr.getInt()) {
                        //照合機能でエラーになった場合
                        listener.onMatchError(json);
                    } else if (intReturnCd == RETUEN_CODE.LockErr.getInt()) {
                        //ロックエラー警告の場合
                        listener.onLockError(json);
                    } else if (intReturnCd > 0) {
                        //正常以外の場合
                        listener.onError(SystemSetting.getString(strErrCd, "", _activity), intReturnCd);
                    } else {
                        //正常の場合
                        listener.onSuccess(json);
                    }
                } else {
                    //WebApiからデータを受信に失敗した場合
                    listener.onError(SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_CONNECT,"",_activity) + "(" + res.toString() + ")", 1);
                }
            }

        } catch (JSONException e) {
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SEND_DATA, "", _activity) + ":" + Common.toString(e), _activity);
            listener.onError(e.getMessage(), 1);
            e.printStackTrace();
        }
    }

    /**
     * リスナーを設定
     * @param listener
     */
    void setListener(HttpAsyncSendLoader.Listener listener) {
        this.listener = listener;
    }

    /**
     * インターフェース
     */
    interface Listener {
        void onSuccess(final JSONObject json);
        void onError(String strResult, int intReturnCd);
        void onConnectError(String strResult);
        void onTagError(JSONObject _item);
        void onLeaseError(JSONObject _item);        //20200225 Add Sasaki V1.2
        void onReservationError(JSONObject _item);  //20210918 ADD Takase V2.0
        void onMatchError(JSONObject _item);
        void onLockError(JSONObject _item) throws JSONException;
    }

}