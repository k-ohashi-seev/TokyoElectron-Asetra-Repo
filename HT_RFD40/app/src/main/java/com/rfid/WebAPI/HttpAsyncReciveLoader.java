package com.rfid.WebAPI;

import android.app.Activity;
import android.os.AsyncTask;

import com.rfid.Factory.LogFactory;
import com.rfid.common.Common;
import com.rfid.common.CommonConst;
import com.rfid.common.SystemSetting;
import com.rfid.setting.SettingValue;

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
import java.util.Map;
import java.util.concurrent.TimeUnit;


/**
 * マスタ受信クラス
 */
public class HttpAsyncReciveLoader extends AsyncTask<String, Integer, Integer> {
    private String result = "";
    private String time;
    private String version;
    private Map _data = null;
    private JSONArray _item = null;
    private Activity _activity = null;
    private String _SendData = null;
    private int _ResponseCode = 0;
    private int _timeout = 30 * 1000;

    /**
     * コンストラクタ
     */
    public HttpAsyncReciveLoader(Activity activity, String SendData, int timeout) {
        super();
        _activity = activity;
        _SendData = SendData;
        _timeout = timeout;
    }

    /** Callback. */
    private Listener listener;

    private enum RETUEN_CODE {
        OK(0),
        Error(1),
        Worning(2),
        Confirm(3),
        WargningTag(4),
        AssetNoSend(5);

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
            //サーバー接続をopenする
            url = new URL(value[0]);
            conn = (HttpURLConnection) url.openConnection();
            conn.setConnectTimeout(_timeout);
            conn.setReadTimeout(_timeout);

            //送信ヘッダーの設定
            conn.setRequestMethod("POST");
            conn.setInstanceFollowRedirects(false);
            conn.setRequestProperty("Accept-Language", "jp");
            conn.setRequestProperty("Content-Type", "application/json; charset=utf-8");

            if (_SendData != null) {
                conn.getOutputStream().write(_SendData.getBytes("UTF-8"));
            }
            conn.getOutputStream().flush();
            conn.getOutputStream().close();

            conn.connect();

            //レスポンスコードの取得
            _ResponseCode= conn.getResponseCode();

            //ステータス200の場合、正常
            if(_ResponseCode == HttpURLConnection.HTTP_OK){

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

                //closeする
                bufferedReader.close();
                inReader.close();
                in.close();
            }

        } catch (MalformedURLException e) {
            e.printStackTrace();
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", _activity) + ":" + Common.toString(e), _activity);
            return -1;
        } catch (IOException e){
            e.printStackTrace();
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", _activity) + ":" + Common.toString(e), _activity);
            return -1;
        }finally {
            if (conn != null) {
                // コネクションを切断
                conn.disconnect();
            }
        }
        return 0;
    }

    /**
     * バックグランド処理が完了し、DBに登録する
     */
    @Override
    protected void onPostExecute(Integer res) {
        //TQProgress.hideProgress();
        String error = null;
        String strErrCd = "";

        try {
            //Web接続に失敗した場合
            if (res < 0) {
                listener.onConnectError(SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_CONNECT,"",_activity));
                return;
            }

            if (!"".equals(result)){
                //WebApiからデータを受信した場合
                JSONObject json = new JSONObject(result);
                strErrCd = json.getString("wordCd");
                int intReturnCd = json.getInt("returnCode");
                //正常以外の場合
                if (intReturnCd > 0) {
                    //20230206 UPD Kawamoto V2.3 START
                    //処理結果が5(資産情報未送信)の場合
                    if (intReturnCd == RETUEN_CODE.AssetNoSend.getInt()) {
                        listener.onSuccess(json);
                    } else {
                        listener.onError(SystemSetting.getString(strErrCd, "", _activity));
                    }
                    //20230206 UPD Kawamoto V2.3 END
                } else {
                    //正常の場合
                    if (listener != null) {
                        listener.onSuccess(json);
                    }
                }
            } else {
                //WebApiからデータを受信に失敗した場合
                listener.onError(SystemSetting.getString(CommonConst.VIEW_WORD_WEBAPI_ERROR_CONNECT,"",_activity) + "(" + _ResponseCode + ")");
                return;
            }
        } catch (JSONException e) {
            e.printStackTrace();
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error,
                    SystemSetting.getString(CommonConst.VIEW_WORD_ALL_RECIVE_DATA, "", _activity) + ":" + Common.toString(e), _activity);
            if (strErrCd != null) {
                listener.onError(SystemSetting.getString(strErrCd, "", _activity));
            }
        }
    }

    /**
     * リスナーを設定
     * @param listener
     */
    public void setListener(Listener listener) {
        this.listener = listener;
    }

    /**
     * インターフェース
     */
    public interface Listener {
        void onSuccess(JSONObject _item);
        void onError(String strResult);
        void onConnectError(String strResult);
    }
}