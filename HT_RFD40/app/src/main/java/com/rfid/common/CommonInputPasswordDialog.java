package com.rfid.common;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.Context;
import android.content.DialogInterface;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import androidx.annotation.NonNull;
import androidx.annotation.StringRes;
import androidx.fragment.app.DialogFragment;
import androidx.fragment.app.Fragment;
import androidx.appcompat.app.AppCompatActivity;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.rfidpk.R;

public final class CommonInputPasswordDialog extends DialogFragment {

    /**
     * MyDialog で何か処理が起こった場合にコールバックされるリスナ.
     */
    public interface Callback {

        /**
         * MyDialog で positiveButton, NegativeButton, リスト選択など行われた際に呼ばれる.
         *
         * @param requestCode CustomDialg 実行時 requestCode
         * @param params CustomDialg に受渡した引数
         */
        void onMyDialogSucceeded(int requestCode, Bundle params);

        /**
         * MyDialog がキャンセルされた時に呼ばれる.
         *
         * @param requestCode CustomDialg 実行時 requestCode
         * @param params CustomDialg に受渡した引数
         */
        void onMyDialogCancelled(int requestCode, Bundle params);
    }

    /**
     * CustomDialg を Builder パターンで生成する為のクラス.
     */
    public static class Builder {

        /** Activity. */
        final AppCompatActivity mActivity;

        /** 親 Fragment. */
        final Fragment mParentFragment;

        /** タイトル. */
        String mTitle;

        /** メッセージ. */
        String mMessage;

        /** リスナに受け渡す任意のパラメータ. */
        Bundle mParams;

        /** DialogFragment のタグ. */
        String mTag = "default";

        /** Dialog をキャンセル可かどうか. */
        boolean mCancelable = true;

        /**
         * コンストラクタ. Activity 上から生成する場合.
         *
         * @param activity
         */
        public <A extends AppCompatActivity & Callback> Builder(@NonNull final A activity) {
            mActivity = activity;
            mParentFragment = null;
        }

        /**
         * タイトルを設定する.
         *
         * @param title タイトル
         * @return Builder
         */
        public Builder title(@NonNull final String title) {
            mTitle = title;
            return this;
        }

        /**
         * タイトルを設定する.
         *
         * @param title タイトル
         * @return Builder
         */
        public Builder title(@StringRes final int title) {
            return title(getContext().getString(title));
        }

        /**
         * メッセージを設定する.
         *
         * @param message メッセージ
         * @return Builder
         */
        public Builder message(@NonNull final String message) {
            mMessage = message;
            return this;
        }

        /**
         * メッセージを設定する.
         *
         * @param message メッセージ
         * @return Builder
         */
        public Builder message(@StringRes final int message) {
            return message(getContext().getString(message));
        }

        /**
         * DialogFragment のタグを設定する.
         *
         * @param tag タグ
         * @return Builder
         */
        public Builder tag(final String tag) {
            mTag = tag;
            return this;
        }

        /**
         * Positive / Negative 押下時のリスナに受け渡すパラメータを設定する.
         *
         * @param params リスナに受け渡すパラメータ
         * @return Builder
         */
        public Builder params(final Bundle params) {
            mParams = new Bundle(params);
            return this;
        }

        /**
         * Dialog をキャンセルできるか否かをセットする.
         *
         * @param cancelable キャンセル可か否か
         * @return Builder
         */
        public Builder cancelable(final boolean cancelable) {
            mCancelable = cancelable;
            return this;
        }

        /**
         * DialogFragment を Builder に設定した情報を元に show する.
         */
        public void show() {
            final Bundle args = new Bundle();
            args.putString("title", mTitle);
            args.putString("message", mMessage);
            args.putBoolean("cancelable", mCancelable);
            if (mParams != null) {
                args.putBundle("params", mParams);
            }

            final CommonInputPasswordDialog f = new CommonInputPasswordDialog();

            f.setArguments(args);
            f.show(mActivity.getSupportFragmentManager(), mTag);
        }

        /**
         * コンテキストを取得する. getString() 呼び出しの為.
         *
         * @return Context
         */
        private Context getContext() {
            return (mActivity == null ? mParentFragment.getActivity() : mActivity).getApplicationContext();
        }
    }

    /** Callback. */
    private Callback mCallback;

    @Override
    public void onAttach(Activity activity) {
        super.onAttach(activity);
        Object callback = getParentFragment();
        if (callback == null) {
            callback = getActivity();
            if (callback == null || !(callback instanceof Callback)) {
                throw new IllegalStateException();
            }
        }
        mCallback = (Callback) callback;
    }

    @Override
    public void onDetach() {
        super.onDetach();
        mCallback = null;
    }

    @NonNull
    @Override
    public Dialog onCreateDialog(Bundle savedInstanceState) {

        final String title = getArguments().getString("title");
        final String message = getArguments().getString("message");
        setCancelable(getArguments().getBoolean("cancelable"));
        final AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());

        //レイアウト
        LayoutInflater i
                = (LayoutInflater) getActivity()
                .getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        final View content = i.inflate(R.layout.dialog_input_password, null);

        ((TextView)content.findViewById(R.id.dialog_title)).setText(title);
        ((TextView)content.findViewById(R.id.dialog_message)).setText(message);
        builder.setView(content);

        // OKボタンのリスナ
        ((TextView)content.findViewById(R.id.positive_button)).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String val = ((EditText)content.findViewById(R.id.dialog_password)).getText().toString();
                if (val.equals(CommonConst.SYSTEM_PASSWORD)) {
                    Bundle b = getArguments().getBundle("params");
                    b.putString("PASSWORD", val);
                    mCallback.onMyDialogSucceeded(getRequestCode(), b);
                    dismiss();
                } else {
                    String msg = SystemSetting.getString(CommonConst.VIEW_WORD_ALL_ERR_PASSWORD, "", getActivity());
                    Toast ts = Toast.makeText(getActivity(), msg, Toast.LENGTH_SHORT);
                    ts.setGravity(Gravity.CENTER, 0, 0);
                    ts.show();
                }
            }
        });

        // キャンセルボタンのリスナ
        ((TextView)content.findViewById(R.id.cancel_button)).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                dismiss();
                mCallback.onMyDialogCancelled(getRequestCode(), getArguments().getBundle("params"));
            }
        });

        Dialog dialog = builder.create();

        // 背景を透明にする
        dialog.getWindow().setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));

        return dialog;
    }

    @Override
    public void onCancel(DialogInterface dialog) {
        mCallback.onMyDialogCancelled(getRequestCode(), getArguments().getBundle("params"));
    }

    /**
     * リクエストコードを取得する
     *
     * @return requestCode
     */
    private int getRequestCode() {
        return getArguments().containsKey("request_code") ? getArguments().getInt("request_code") : getTargetRequestCode();
    }
}
