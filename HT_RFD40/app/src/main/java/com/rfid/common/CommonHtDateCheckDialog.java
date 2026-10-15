package com.rfid.common;

import android.app.AlertDialog;
import android.app.Dialog;
import android.content.Context;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import androidx.annotation.NonNull;
import androidx.annotation.StringRes;
import androidx.fragment.app.DialogFragment;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.TextView;

import com.rfidpk.R;


/**
 * Activityから呼び出すダイアログに関するクラスです。
 * OK、キャンセルボタンのあるメッセージボックスのダイアログFragmentです。
 *　DialogFragmentを継承します。
 * 本クラスを使用する場合は、interfaceにDialogConfirm.Callbackを指定してください。
 *
 * @see Callback#onMyDialogSucceeded(int, Bundle)  OKボタンがクリックされた際のイベントです。
 *
 * @since 1.00
 */
public final class CommonHtDateCheckDialog extends DialogFragment {
    /** Callback */
    private Callback mCallback;

    /** ダイアログ表示フラグ */
    public static boolean p_ShowDialog = false;

    /**
     * Dialogで何か処理が起こった場合にコールバックされます。
     */
    public interface Callback {

        /**
         * OKボタンがクリックされた際のイベントです。
         *
         * @param requestCode リクエストコード
         * @param params Bundle
         */
        void onMyDialogSucceeded(int requestCode, Bundle params);
    }

    /**
     * DialogをBuilderパターンで生成する為のクラスです。
     */
    public static class Builder {

        /** ダイアログ受け渡し用バンドル */
        private final Bundle bundle = new Bundle();

        /** Dialogをキャンセル可かどうかを指定 */
        boolean mCancelable = true;

        /**
         * タイトルを設定する.
         *
         * @param title タイトル
         * @return Builder
         */
        public void title(final String title) {
            bundle.putString("title", title);
        }

        /**
         * メッセージを設定する.
         *
         * @param message メッセージ
         * @return Builder
         */
        public void message(@NonNull final String message) {
            bundle.putString("message", message);
        }

        /**
         * サーバー日時タイトルを設定します。
         *
         * @param titleServerDate サーバー日時タイトル
         */
        public void titleServerDate(@NonNull final String titleServerDate) {
            bundle.putString("titleServerDate", titleServerDate);
        }

        /**
         * HT日時タイトルを設定します。
         *
         * @param titleHtDate HT日時タイトル
         */
        public void titleHtDate(@NonNull final String titleHtDate) {
            bundle.putString("titleHtDate", titleHtDate);
        }

        /**
         * サーバー日時を設定します。
         *
         * @param serverDate サーバー日時
         */
        public void serverDate(@NonNull final String serverDate) {
            bundle.putString("serverDate", serverDate);
        }

        /**
         * HT日時を設定します。
         *
         * @param htDate HT日時
         */
        public void htDate(@NonNull final String htDate) {
            bundle.putString("htDate", htDate);
        }

        /**
         * ボタン押下時のリスナに受け渡すパラメータを設定します。
         *
         * @param params リスナに受け渡すパラメータ
         */
        public void params(final Bundle params) {
            Bundle prmBundle = new Bundle(params);
            bundle.putBundle("params", prmBundle);
        }

        /**
         * Dialogをキャンセルできるか否かをセットします。
         *
         * @param cancelable キャンセル可か否か
         * @return Builder
         */
        public void cancelable(final boolean cancelable) {
            mCancelable = cancelable;
        }

        /**
         * ダイアログを生成します。
         */
        public void show(FragmentManager manager, String tag) {
            CommonHtDateCheckDialog dialog = new CommonHtDateCheckDialog();
            dialog.setArguments(bundle);
            dialog.show(manager, tag);
        }
    }

    /**
     * onAttachイベント
     *
     * @param context　Context(Activityへの参照が格納されている)
     */
    @Override
    public void onAttach(@NonNull Context context) {
        super.onAttach(context);

        //呼び出し元Fragment取得(Activityから呼び出しの場合はNull)
        Fragment parentFragment = getParentFragment();

        //インターフェース実装チェック
        if (parentFragment instanceof Callback) {
            //Fragmentから呼び出しの場合
            mCallback = (Callback)parentFragment;
        } else if (context instanceof Callback) {
            //Activityから呼び出しの場合
            mCallback = (Callback)context;
        } else {
            //呼び出し元でインターフェースが実装されていない場合
            throw new IllegalStateException();
        }

    }

    /**
     * onDetachイベント
     */
    @Override
    public void onDetach() {
        super.onDetach();
        p_ShowDialog = false;
        mCallback = null;
    }

    /**
     * ダイアログを作成します。
     *
     * @param savedInstanceState　Bundle
     * @return Dialog
     */
    @NonNull
    @Override
    public Dialog onCreateDialog(Bundle savedInstanceState) {

        final String title = getArguments().getString("title");
        final String message = getArguments().getString("message");
        final String titleServerDate = getArguments().getString("titleServerDate") + "：";
        final String titleHtDate = getArguments().getString("titleHtDate") + "：";
        final String serverDate = getArguments().getString("serverDate");
        final String htDate = getArguments().getString("htDate");
        setCancelable(getArguments().getBoolean("cancelable"));
        final AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());

        //レイアウト
        LayoutInflater i
                = (LayoutInflater) getActivity()
                .getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        View content = i.inflate(R.layout.dialog_ht_date, null);

        //ダイアログのタイトルとメッセージをセット
        ((TextView)content.findViewById(R.id.dialog_title)).setText(title);
        ((TextView)content.findViewById(R.id.dialog_message)).setText(message);
        ((TextView)content.findViewById(R.id.title_server_date)).setText(titleServerDate);
        ((TextView)content.findViewById(R.id.title_ht_date)).setText(titleHtDate);

        //サーバー日時とHT日時をセット
        ((TextView)content.findViewById(R.id.dialog_server_date)).setText(serverDate);
        ((TextView)content.findViewById(R.id.dialog_ht_date)).setText(htDate);

        builder.setView(content);

        // OKボタンのリスナ
        ((TextView)content.findViewById(R.id.positive_button)).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                dismiss();  //ダイアログを閉じる
                mCallback.onMyDialogSucceeded(getRequestCode(), getArguments().getBundle("params"));
            }
        });

        Dialog dialog = builder.create();

        // 背景を透明にする
        dialog.getWindow().setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));

        return dialog;
    }

    /**
     * リクエストコードを取得します。
     *
     * @return リクエストコード
     */
    private int getRequestCode() {
        return getArguments().containsKey("request_code") ? getArguments().getInt("request_code") : getTargetRequestCode();
    }
}
