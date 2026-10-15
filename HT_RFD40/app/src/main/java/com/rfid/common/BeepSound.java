package com.rfid.common;

import android.app.Activity;
import android.content.Context;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.media.ToneGenerator;
import android.util.Log;

// 読取り音発生クラス
public class BeepSound {

    private ToneGenerator _toneGenerator = null;
    private int _ringVolume = 0;

    /**
     * コンストラクタ
     * @param activity
     */
    public BeepSound(Activity activity) {
        AudioManager am = (AudioManager)activity.getSystemService(Context.AUDIO_SERVICE);
        _ringVolume = am.getStreamVolume(AudioManager.STREAM_RING); //着信音量の取得
        _toneGenerator = new ToneGenerator(AudioManager.STREAM_RING, GetVolume());  //音量
    }

    /**
     * 終了
     */
    public void Destory () {
        _toneGenerator.stopTone();
        _toneGenerator = null;
    }

    /**
     * カウント音発生
     */
    public void BeepCountUp() {
            try {
                _toneGenerator.startTone(ToneGenerator.TONE_CDMA_SOFT_ERROR_LITE,250);        //合成音
            }
            catch (Exception ex) {
                Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            }
    }

    /**
     * 接続音
     */
    public void BeepConnect() {
        try {
            _toneGenerator.startTone(ToneGenerator.TONE_CDMA_EMERGENCY_RINGBACK,250);        //合成音
        }
        catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    /**
     * 書き込み音
     */
    public void BeepWrite() {
        try {
            _toneGenerator.startTone(ToneGenerator.TONE_CDMA_ALERT_CALL_GUARD,350);        //合成音
        }
        catch (Exception ex) {
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    private int GetVolume() {
        int volume = 0;

        //20232021 UPD Sato v2.3
        //20230410 UPD Sato v2.3.1 RS35追加
        if (CommonConst.CONNECT_RFIDNAME.KEYENCE_DX.getString().equals(Common.ConnectRFIDName) ||
            CommonConst.CONNECT_RFIDNAME.IDATA.getString().equals(Common.ConnectRFIDName) ||
            CommonConst.CONNECT_RFIDNAME.CIPHER_LAB_RS35.getString().equals(Common.ConnectRFIDName)) {

            //キーエンス製DXシリーズ、iDATA、RS35の場合
            switch (_ringVolume) {
                case 0:
                    volume = 0;
                    break;
                case 1:
                    volume = 8;
                    break;
                case 2:
                    volume = 15;
                    break;
                case 3:
                    volume = 22;
                    break;
                case 4:
                    volume = 29;
                    break;
                case 5:
                    volume = 36;
                    break;
                case 6:
                    volume = 43;
                    break;
                case 7:
                    volume = 50;
                    break;
                case 8:
                    volume = 57;
                    break;
                case 9:
                    volume = 64;
                    break;
                case 10:
                    volume = 70;
                    break;
                case 11:
                    volume = 76;
                    break;
                case 12:
                    volume = 82;
                    break;
                case 13:
                    volume = 88;
                    break;
                case 14:
                    volume = 94;
                    break;
                case 15:
                default:
                    volume = 100;
                    break;
            }
        } else {
            //その他の端末の場合
            switch (_ringVolume) {
                case 0:
                    volume = 0;
                    break;
                case 1:
                    volume = 15;
                    break;
                case 2:
                    volume = 30;
                    break;
                case 3:
                    volume = 55;
                    break;
                case 4:
                    volume = 70;
                    break;
                case 5:
                    volume = 80;
                    break;
                case 6:
                    volume = 90;
                    break;
                case 7:
                    volume = 100;
                    break;
            }
        }

        return volume;
    }
}
