package com.rfid.common;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import androidx.annotation.NonNull;
import androidx.fragment.app.DialogFragment;
import androidx.appcompat.app.AppCompatActivity;

import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.NumberPicker;

import com.rfidpk.R;

/**
 * カレンダーのダイアログ作成クラス
 */
public final class CommonNumberpicker extends DialogFragment {

    /**
     * Dialog で何か処理が起こった場合にコールバックされるリスナ.
     */
    public interface Callback {

        /**
         * OKボタンがクリックされた時のイベント
         *
         * @param strNumber 数値
         */
        void onOKButtonClick(String strNumber);
    }

    /**
     * Dialg を Builder パターンで生成する為のクラス.
     */
    public static class Builder {

        /** Activity. */
        final AppCompatActivity mActivity;

        /**
         * コンストラクタ. Activity 上から生成する場合.
         *
         * @param activity
         */
        public <A extends AppCompatActivity & Callback> Builder(@NonNull final A activity) {
            mActivity = activity;
        }

        /**
         * カレンダーを表示する
         */
        public void show() {
            final CommonNumberpicker f = new CommonNumberpicker();
            f.show(mActivity.getSupportFragmentManager(), "calender");
        }
    }

    /** Callback. */
    private CommonNumberpicker.Callback mCallback;

    @Override
    public void onAttach(Activity activity) {
        super.onAttach(activity);
        Object callback = getParentFragment();
        if (callback == null) {
            callback = getActivity();
            if (callback == null || !(callback instanceof CommonNumberpicker.Callback)) {
                throw new IllegalStateException();
            }
        }
        mCallback = (CommonNumberpicker.Callback) callback;
    }

    @Override
    public void onDetach() {
        super.onDetach();
        mCallback = null;
    }

    /**
     * ダイアログを作成する
     * @param savedInstanceState
     * @return
     */
    @Override
    public Dialog onCreateDialog(Bundle savedInstanceState) {

        //レイアウト
        LayoutInflater inflater = getActivity().getLayoutInflater();
        View content = inflater.inflate(R.layout.dialog_number, null, false);

        /*NumberPicker oPicker = content.findViewById(R.id.numberpicker);
        oPicker.setMinValue(4);
        oPicker.setMaxValue(30);*/

        //キャンセルボタンのリスナ
        content.findViewById(R.id.close_button).setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                dismiss();
            }
        });

        //OKボタンのリスナ
        content.findViewById(R.id.ok_button).setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                int intNumber = ((NumberPicker)v.findViewById(R.id.numberpicker)).getValue();
                mCallback.onOKButtonClick(String.valueOf(intNumber));
            }
        });

        final AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());
        builder.setView(content);
        Dialog dialog = builder.create();

        // 背景を透明にする
        dialog.getWindow().setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));

        return dialog;
    }
}