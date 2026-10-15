package com.rfid.common;

import android.app.Dialog;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import androidx.fragment.app.DialogFragment;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.Window;
import android.view.WindowManager;
import android.widget.TextView;

import com.rfidpk.R;

/**
 * OKボタンのみのダイアログ作成クラス
 */
public class CommonDialog extends DialogFragment {

    private String _Message = "";

    //ダイアログ表示フラグ ADD 2019/12/12
    public static boolean p_ShowDialog = false;

    /**
     * ダイアログを作成する
     * @param savedInstanceState
     * @return
     */
    @Override
    public Dialog onCreateDialog(Bundle savedInstanceState) {

        Dialog dialog = new Dialog(getActivity());
        // タイトル非表示
        dialog.getWindow().requestFeature(Window.FEATURE_NO_TITLE);
        // フルスクリーン
        dialog.getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN, WindowManager.LayoutParams.FLAG_LAYOUT_IN_SCREEN);
        // デザイン
        dialog.setContentView(R.layout.dialog_custom);
        // 背景を透明にする
        dialog.getWindow().setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
        //タイトルを設定する
        ((TextView)dialog.findViewById(R.id.dialog_title)).setText(this.getTag());
        //メッセージを設定する
        ((TextView)dialog.findViewById(R.id.dialog_message)).setText(_Message);
        // OK ボタンのリスナ
        dialog.findViewById(R.id.close_button).setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                p_ShowDialog = false;     //ダイアログ表示ＯＦＦ ADD 2019/12/12
                dismiss();
            }
        });
        return dialog;
    }

    /**
     * 表示するメッセージ
     * @param strMessage
     */
    public void Message(String strMessage) {
        _Message = strMessage;
        p_ShowDialog = true;     //ダイアログ表示ＯＮ ADD 2019/12/12
    }

//20191213 Add Start Sasaki
    /**
     * 画面を閉じられた際のイベント
     */
    @Override
    public void onDetach() {
        super.onDetach();
        p_ShowDialog = false;
    }
//20191213 Add End Sasaki
}