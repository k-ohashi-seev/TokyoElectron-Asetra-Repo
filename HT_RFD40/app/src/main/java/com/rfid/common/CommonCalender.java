package com.rfid.common;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.Context;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import androidx.annotation.NonNull;
import androidx.fragment.app.DialogFragment;
import androidx.appcompat.app.AppCompatActivity;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.CalendarView;

import com.rfidpk.R;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;

/**
 * カレンダーのダイアログ作成クラス
 */
public final class CommonCalender extends DialogFragment {

    /**
     * Dialog で何か処理が起こった場合にコールバックされるリスナ.
     */
    public interface Callback {

        /**
         * 日付がクリックされたときに呼ばれるイベント
         *
         * @param strDate 日付
         */
        void onCalenderClick(String strDate);
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
            final CommonCalender f = new CommonCalender();
            f.show(mActivity.getSupportFragmentManager(), "calender");
        }
    }

    /** Callback. */
    private CommonCalender.Callback mCallback;

    @Override
    public void onAttach(Activity activity) {
        super.onAttach(activity);
        Object callback = getParentFragment();
        if (callback == null) {
            callback = getActivity();
            if (callback == null || !(callback instanceof CommonCalender.Callback)) {
                throw new IllegalStateException();
            }
        }
        mCallback = (CommonCalender.Callback) callback;
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
        LayoutInflater i
                = (LayoutInflater) getActivity()
                .getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        View content = i.inflate(R.layout.dialog_calender, null);

        //ボタンのリスナ
        content.findViewById(R.id.close_button).setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                dismiss();
            }
        });

        CalendarView calendarView = content.findViewById(R.id.calendarView);
        calendarView.setOnDateChangeListener(new CalendarView.OnDateChangeListener() {
            @Override
            public void onSelectedDayChange(CalendarView view, int year, int month,
                                            int dayOfMonth) {

                try {
                    //20210427 UPD Start Suzuki
                    //String strDate = Integer.toString(year) + "/" + Integer.toString(month) + "/" + Integer.toString(dayOfMonth);
                    String strDate = Integer.toString(year) + "/" + Integer.toString(month + 1) + "/" + Integer.toString(dayOfMonth);
                    //20210427 UPD End Suzuki

                    // DateFormat
                    DateFormat df = new SimpleDateFormat("yyyy/MM/dd");
                    Date dt = df.parse(strDate);

                    // カレンダークラスのインスタンスを取得
                    Calendar cal = Calendar.getInstance();
                    cal.setTime(dt);
                    //cal.add(Calendar.MONTH, 1);       20210427 DEL Suzuki

                    dismiss();
                    mCallback.onCalenderClick(df.format(cal.getTime()));
                } catch (Exception ex) {
                    Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
                }
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