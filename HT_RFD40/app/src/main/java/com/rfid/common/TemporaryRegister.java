package com.rfid.common;
import static com.rfid.common.BaseRead.BUTTON_SUMMARY;

import com.rfid.common.CommonConfirmDialog;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.os.Bundle;
import android.provider.ContactsContract;
import android.renderscript.ScriptGroup;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.Spinner;
import android.widget.TextView;

import com.rfid.Dispose.DisposeRead;
import com.rfid.Factory.ConstRuctionFactory;
import com.rfid.Factory.EmployeeFactory;
import com.rfid.Factory.LocationFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Factory.TempListFactory;
import com.rfid.Factory.TemporaryFactory;
import com.rfid.Inventory.InventoryLocationSetting;
import com.rfid.Inventory.InventoryTagIn;
import com.rfid.Lease.LeaseRead;
import com.rfid.Lease.LeaseSetting;
import com.rfid.Return.ReturnRead;
import com.rfid.Search.SearchRead;
import com.rfid.Storage.StorageLocationSetting;
import com.rfid.Taglink.TaglinkRead;
import com.rfid.setting.SettingValue;
import com.rfid.setting.SystemSettHub;
import com.rfidpk.R;

import org.w3c.dom.Text;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * 工事データの一覧表示画面のクラスです。
 * 工事マスタからデータを取得し、一覧表示します。
 */
public class TemporaryRegister extends BaseActivity implements CommonCalender.Callback,CommonOKDialog.Callback, CommonConfirmDialog.Callback {
    //確認画面
    protected static final int BUTTON_REGISTER = 1;

    // 前回選択されたアイテムのビュー
    private View previousSelectedView = null;
    // 前回選択されたアイテムのposition
    private int previousSelectedViewPosition = -1;
    // 選択したメニュー項目
    private String menu = "";
    //アクティビティ名
    private Activity activity = this;
    //貸出担当
    private String _EmployeeCode = "";
    private String _EmployeeName = "";
    //作業実施日
    private String _WorkDay = "";
    //メニュー選択画面
    private String _MenuClick = "";

    //選択画面からの戻り用フラグ
    private boolean _blnSubPageFlag = false;

    //工事貸出終了日
    private String _LentEndDt = "";

    //仮置き日
    private String _TempDate = "";
    //工事番号
    private String no ="";
    //仮置きID
    private String tempId = "z1";
    //選択状態
    private String selectState ="";
    //表示ビュー取得(入力チェック用)
    ArrayList<View> viewList;
    //設定マスタの値取得用
    String temporaryInputItem[][];

    //選択ボタン用
    private static final int FORM_REQUEST_Employee = 1;  //貸出先
    private static final int FORM_REQUEST_Temporary = 3;    //仮置き日

    private static final int RESIST_OK = 5;

    /**
     * 画面表示処理
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_temporary_register);
        //タイトルバー 戻るボタン表示
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        //DB接続用変数
        SQLiteDatabase db = null;
        SQLiteDatabase dbLoc = null;



        try {
            Intent intent = getIntent();
            no = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO);
            selectState = intent.getStringExtra(CommonConst.VIEW_KEY_STATE_SELECT);

            //メニューで選択したメニュー取得
            _MenuClick = intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT);

            _LentEndDt = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT);

            if(selectState.equals(CommonConst.VIEW_KEY_STATE_UPDATE)){
                tempId = intent.getStringExtra(CommonConst.VIEW_KEY_HT_TEMP_ID);
            }

            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            dbLoc = dbHelper.getWritableDatabase();

            //文言表示
            InitializeWord();

            viewList = ViewConstRuctionData();
            final CommonConfirmDialog.Builder  dialog = new CommonConfirmDialog.Builder(this);
            //登録ボタン　Clickイベント
            findViewById(R.id.btnSettingNext).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view){
                    menu = SystemSetting.getString(CommonConst.VIEW_KEY_MENU_SELECT,CommonConst.MENU_CLICK.Inventory.toString(),activity);
//                    //仮置き日入力チェック
//                    if(!InputCheck(findViewById(R.id.txtTempDay))){
//                        //エラーメッセージ表示
//                        showErrorMessage(("仮置き日")+SystemSetting.getString(CommonConst.VIEW_KEY_TEMPDATA_INPUT_CHECK, "", TemporaryRegister.this));
//                        return;
//                    }
//
//                    int i=1;
//                    for(View viewItem : viewList){
//                        if(!InputCheck(viewItem)){
//                            //エラーメッセージ表示
//                            showErrorMessage(("仮置き情報"+i)+SystemSetting.getString(CommonConst.VIEW_KEY_TEMPDATA_INPUT_CHECK, "", TemporaryRegister.this));
//                            return;
//                        }
//                        i++;
//                    }
                    //DBに新規登録または変更

                    Bundle bundle = new Bundle();
                    bundle.putInt(CommonConst.VIEW_CONFIRM_KEY, BUTTON_REGISTER);
                    dialog.title(SystemSetting.getString(CommonConst.VIEW_WORD_ALL_REGISTER, "", activity));  //クリア
                    dialog.message(SystemSetting.getString(
                            CommonConst.VIEW_KEY_TEMPDATA_REGISTER_CHECK, "", activity));                //仮置きデータを登録します。よろしいですか？
                    dialog.params(bundle);
                    dialog.show();

                }
            });


        } catch (Exception ex){
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            db.close();
        }



        //カレンダー選択ボタン Clickイベント
        final CommonCalender.Builder calender = new CommonCalender.Builder(this);
        //開始日ボタン Clickイベント
        findViewById(R.id.txtTempDay).setOnClickListener(new View.OnClickListener(){
            @Override
            public void onClick(View view) {
                String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
                //LogFactory.Insert(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName + ":" + SystemSetting.getString(CommonConst.VIEW_WORD_ALL_SELECT, "", activity), activity);

                //カレンダー表示
                calender.show();

            }
        });

    }

    /**
     * サブ画面から戻るイベント処理をします。
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        try {
            switch(requestCode) {

                case FORM_REQUEST_Temporary:            //カレンダー選択画面から戻る
                    if(resultCode == RESULT_OK) {
                        //返却予定日を保持する
                        _TempDate = data.getStringExtra(CommonConst.VIEW_KEY_DATE);

                        // サブ画面から戻り値を取得して表示する
                        TextView textS = findViewById(R.id.txtLeaseSettingReturn);
                        textS.setText(_TempDate);
                    }
                    break;
                case BUTTON_SUMMARY:        //一覧画面から戻る

                    if(resultCode == RESIST_OK){
                        setResult(RESIST_OK);

                        //画面を終了する
                        this.finish();
                    }
                    break;
                default:
                    break;
            }

            //ボタンを制御する
            AllResetButton(true);

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
            //エラーログ
            LogFactory.Insert(CommonConst.PROCESS_CAT.Error, getLocalClassName() + ":" +  Common.toString(ex), this);
        }
    }

    /**
     * カレンダークリック時のイベント
     * @param strDate 日付
     */
    @Override
    public void onCalenderClick(String strDate) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        if (strDate != null && !"".equals(strDate)){
            _WorkDay = strDate;
//            if (InputReturnDateCheck()) {
//                ((TextView) findViewById(R.id.txtWorkDay)).setText(strDate);
//            } else {
//                //元の日付に戻す
//                _WorkDay = ((TextView) findViewById(R.id.txtWorkDay)).getText().toString();
//                //過去日の場合、エラーメッセージを表示する
//                ShowErrorMessage(SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_ERROR_RETURN, "", this)
//                        , SystemSetting.getString(CommonConst.VIEW_WORD_LEASE_RETURN, "", this));
//            }

            ((TextView) findViewById(R.id.txtTempDay)).setText(strDate);
        }


    }

    /**
     * 仮置き日過去入力日チェックを行います。
     * @return
     */
    private boolean InputReturnDateCheck(){
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

        boolean blnReturn = true;

        try {
            //返却予定日
            SimpleDateFormat date = new SimpleDateFormat("yyyy/MM/dd");
            Date returnDate = date.parse(_TempDate);

            //本日日付
            Date today = date.parse(Common.getNowTime("yyyy/MM/dd"));

            //返却予定日が未入力の場合、エラー
            if (returnDate.before(today)) {
                blnReturn = false;
            }
            return blnReturn;
        } catch (ParseException pe) {
            return false;
        }
    }

//    /**
//     * 文言表示
//     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_REGISTER_TITLE,"",this));

        ((TextView) findViewById(R.id.labSubTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_INFO_REGISTER, "", this));     //仮置き情報を登録してください
        ((TextView) findViewById(R.id.lbTempDay))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_DAY, "", this));     //仮置き日
//        ((TextView) findViewById(R.id.lbEmployeeCD))
//                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SET_LIABILITY, "", this));     //責任者
        ((TextView) findViewById(R.id.lbWorkLoc))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_WORK_LOC, "", this));     //作業場所
        ((TextView) findViewById(R.id.lbTempRemark))
                .setText(SystemSetting.getString(CommonConst.VIEW_KEY_TEMP_REMARK, "", this));     //備考
    }

    private String changeFormatDate(String date){
        String year = String.valueOf(Integer.parseInt(date.substring(0, 4)));
        String month = String.format("%02d",Integer.parseInt(date.substring(4, 6)));
        String day = String.format("%02d",Integer.parseInt(date.substring(6, 8)));
        String formattedDate = year + "/" + month + "/" +day;
        return formattedDate;
    }

    private Integer changeFormatDateToInt(String date){
        String year = date.substring(0, 4);
        String month = date.substring(5, 7);
        String day = date.substring(8, 10);
        String formattedDate = year + month + day;
        return Integer.parseInt(formattedDate);
    }
//
//    private String delimiterReplace(String valueName){
//        valueName = valueName.replace("$$$","~");
//        return valueName;
//    }

    /**
     * 画面表示内容(新規登録、変更)
     * @return
     */
    private ArrayList<View> ViewConstRuctionData() {
        //SQL作成
        String strSql = new String();

        //DB接続用変数
        SQLiteDatabase db = null;
        //データベースヘルパーのインスタンスを作成する
        DatabaseHelper dbHelper = new DatabaseHelper(this);
        db = dbHelper.getWritableDatabase();

        //Temporary_Input_Item用変数
        temporaryInputItem = SettingFactory.GetTemporaryInputItem(db);
        //Const_SelectItem用変数
        HashMap<String, String> selectItem;
        ArrayList<Map<String, String>> temporarySelectItems = new ArrayList<>();

        //ビュー取得用変数
        String spinnerName = "spnTemporaryInfo";
        String spnUnitName = "spnUnit";
        String spnLayoutName = "spnLayout";
        String txtViewName = "txtTemporaryInfo";
        String txtUnitName = "txtUnit";
        String txtLayoutName = "txtLayout";
        String lblViewName = "lblTemporaryInfo";
        String lblUnitName = "lblUnit";
        String lblLayoutName = "lblLayout";
        String allLayoutName = "allLayout";

        //項目のタイトル
        String txtTitleView = "txtTitle";

        ArrayList<View> viewList = new ArrayList<>();

        SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy/MM/dd");

        try {

            EditText txtTempDay = findViewById(R.id.txtTempDay);

            //仮置きリストからも取得
            String sql = TempListFactory.GetTempListDataTempId(no,tempId);
            Cursor cursor = db.rawQuery(sql, null);

            //取得できた場合
            if (cursor == null || cursor.getCount() == 0) {
                // 必ず cursor を閉じる
                if (cursor != null) {
                    cursor.close();
                }
                sql = "";
                cursor = null;

                //仮置きテーブルからも取得
                sql = TemporaryFactory.GetTemporaryData(no,tempId);
                cursor = db.rawQuery(sql, null);
            }

            //新規登録の場合
            if(selectState.equals(CommonConst.VIEW_KEY_STATE_REGISTER)) {   //新規登録の場合
                Date nowDate = new Date();
                txtTempDay.setText(dateFormat.format(nowDate));
            }
        else{   //変更の場合
            if (cursor.moveToNext()) {
                String date = changeFormatDate(cursor.getString(cursor.getColumnIndex("TEMP_DT")));
                txtTempDay.setText(date);
//                EditText txtLiability = findViewById(R.id.txtTempEmployeeCD);
//                txtLiability.setText(cursor.getString(cursor.getColumnIndex("TEMP_EMP_CD")));
//                TextView txtTempEmployeeName = findViewById(R.id.txtTempEmployeeName);
//                txtTempEmployeeName.setText(cursor.getString(cursor.getColumnIndex("TEMP_EMP_NAME")));
                EditText txtRemark = findViewById(R.id.txtRemark);
                txtRemark.setText(cursor.getString(cursor.getColumnIndex("REMARK")));
            }

        }
            //仮置き情報
            for (int i = 0; i < temporaryInputItem.length; i++) {
                //スピナのID取得
                Spinner editSpinner = (Spinner) GetView(spinnerName, i + 1);
                //単位テキスト取得
                TextView spnUnit = (TextView) GetView(spnUnitName, i + 1);
                //LinearLayout(スピナー)の取得
                LinearLayout spnlayout = (LinearLayout) GetView(spnLayoutName, i + 1);
                //テキストビューのID取得
                TextView editText = (TextView) GetView(txtViewName,i+1);
                //単位テキスト取得
                TextView txtUnit = (TextView) GetView(txtUnitName, i + 1);
                //LinearLayout(テキスト)の取得
                LinearLayout txtLayout = (LinearLayout) GetView(txtLayoutName, i + 1);

                //テキストビューのID取得
                TextView lblText = (TextView) GetView(lblViewName,i+1);
                //単位テキスト取得
                TextView lblUnit = (TextView) GetView(lblUnitName, i + 1);
                //LinearLayout(ラベル)の取得
                LinearLayout lblLayout = (LinearLayout) GetView(lblLayoutName, i + 1);

                //AllLinearLayout
                LinearLayout allLayout = (LinearLayout) GetView(allLayoutName, i + 1);

                //項目タイトル
                TextView txtTitle = (TextView) GetView(txtTitleView, i + 1);

                //設定マスタDATA1が非表示の場合
                if(temporaryInputItem[i][0].equals("N")){
                    spnlayout.setVisibility(View.GONE);
                    lblLayout.setVisibility(View.GONE);
                    txtLayout.setVisibility(View.GONE);
                    allLayout.setVisibility(View.GONE);
                    viewList.add(null);
                    continue;
                }

                //タイトル設定
                txtTitle.setText(temporaryInputItem[i][5]);

                //設定マスタDATA2がTEXTの場合
                if (temporaryInputItem[i][1].equals("TEXT")) {
                    //editSpinner.setVisibility(View.GONE);
                    spnlayout.setVisibility(View.GONE);
                    lblLayout.setVisibility(View.GONE);
                    txtLayout.setVisibility(View.VISIBLE);
                    viewList.add(editText);

                    //DATA4に単位データがある場合
                    if (!temporaryInputItem[i][3].isEmpty()) {
                        txtUnit.setText(temporaryInputItem[i][3]);
                    } else {
                        txtUnit.setVisibility(View.INVISIBLE);
                    }
                    //変更モードの場合仮置き情報をセット
                    if(selectState.equals(CommonConst.VIEW_KEY_STATE_UPDATE)){
                        String txtName = "INFO"+(i+1);
                        editText.setText(DeleteUnit(cursor.getString(cursor.getColumnIndex(txtName))));
                    }
                    else{
                        editText.setText(temporaryInputItem[i][2]);
                    }
                } else if(temporaryInputItem[i][1].equals("LIST")) {    //設定マスタDATA2がLISTの場合
                    txtLayout.setVisibility(View.GONE);
                    lblLayout.setVisibility(View.GONE);
                    spnlayout.setVisibility(View.VISIBLE);
                    viewList.add(editSpinner);
                    //ドロップダウンリスト項目内容取得
                    if(selectState.equals(CommonConst.VIEW_KEY_STATE_UPDATE)){
                        String txtName = "INFO"+(i+1);
                        selectItem = SettingFactory.GetTemporarySelectItem(db, temporaryInputItem[i][4],DeleteUnit(cursor.getString(cursor.getColumnIndex(txtName))));
                    }else{
                        selectItem = SettingFactory.GetTemporarySelectItem(db, temporaryInputItem[i][4],temporaryInputItem[i][2]);
                    }

                    temporarySelectItems.add(selectItem);
                    //先頭に空白項目を追加
                    List<String> value = new ArrayList<>(selectItem.values());
                    value.add(0,"");
                    //ドロップダウンリストへ表示する処理
                    ArrayAdapter<String> arrayAdapter = new ArrayAdapter<>(this, R.layout.custom_spinner, value);
                    arrayAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
                    editSpinner.setAdapter(arrayAdapter);
                    //初期表示設定
                    if(selectState.equals(CommonConst.VIEW_KEY_STATE_UPDATE)){
                        //初期値設定のリスト番号取得
                        String txtName = "INFO"+(i+1);
                        // 値のリストを取得
                        List<String> values = new ArrayList<>(selectItem.values());
                        int valueIndex = values.indexOf(DeleteUnit(cursor.getString(cursor.getColumnIndex(txtName))));
                        editSpinner.setSelection(valueIndex + 1);
                    }else{
                        //初期値表示
                        // 値のリストを取得
                        List<String> values = new ArrayList<>(selectItem.values());
                        int valueIndex = -1;
                        //初期値がある場合
                        if (!temporaryInputItem[i][2].trim().equals("")){
                            valueIndex = values.indexOf(temporaryInputItem[i][2]);
                        }
                        editSpinner.setSelection(valueIndex + 1);
                    }
                    //DATA4に単位データがあるか
                    if (!temporaryInputItem[i][3].isEmpty()) {
                        spnUnit.setText(temporaryInputItem[i][3]);
                    } else {
                        spnUnit.setVisibility(View.INVISIBLE);
                    }
                }else if(temporaryInputItem[i][1].equals("LABEL")){
                    txtLayout.setVisibility(View.GONE);
                    spnlayout.setVisibility(View.GONE);
                    lblLayout.setVisibility(View.VISIBLE);
                    viewList.add(lblText);

                    //DATA4に単位データがある場合
                    if (!temporaryInputItem[i][3].isEmpty()) {
                        lblUnit.setText(temporaryInputItem[i][3]);
                    } else {
                        lblUnit.setVisibility(View.INVISIBLE);
                    }
                    //変更モードの場合仮置き情報をセット
                    if(selectState.equals(CommonConst.VIEW_KEY_STATE_UPDATE)){
                        String txtName = "INFO"+(i+1);
                        lblText.setText(DeleteUnit(cursor.getString(cursor.getColumnIndex(txtName))));
                    }
                    else{
                        lblText.setText(temporaryInputItem[i][2]);
                    }
                }

            }
            cursor.close();

        } catch (Exception ex) {
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
        return viewList;
    }

    private int getSelectionIndex(HashMap<String,String> selectItem,String columnName){
        if(selectState.equals(CommonConst.VIEW_KEY_STATE_UPDATE)){
            int i=1;
            for(String key : selectItem.keySet()){
                if(key.equals(columnName)){
                    return i;
                }
                i++;
            }
        }
        return 0;
    }

    /**
     * 確認ダイアログでOKがクリックされたイベント処理
     */
    @Override
    public void onMyDialogSucceeded(int requestCode, Bundle params) {
        if (params == null) {
            return;
        }

        //呼出元で分岐する
        switch (params.getInt(CommonConst.VIEW_CONFIRM_KEY)) {
            case BUTTON_REGISTER:      //登録ボタン
                //登録処理
                if(selectState.equals(CommonConst.VIEW_KEY_STATE_REGISTER)) {
                    addTemporaryInsert(viewList);
                }
                else{
                    addTemporaryUpdate(viewList);
                }
                //仮置き読み取り画面へ遷移
                Intent intent =new Intent(getApplication(), CommonMatRead.class);

                intent.putExtra(CommonConst.VIEW_KEY_HT_TEMP_ID, tempId);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, no);
                intent.putExtra(CommonConst.VIEW_KEY_STATE_SELECT,selectState);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT, _LentEndDt);
                startActivityForResult(intent, BUTTON_SUMMARY);

                break;

        }

    }

    /**
     * 仮置きテーブルに追加(新規登録)
     */
    private void addTemporaryInsert(ArrayList<View> viewList){
        DatabaseHelper dbHelper = new DatabaseHelper(this);
        SQLiteDatabase db = dbHelper.getWritableDatabase();

        try {
            //仮置きにすでに新規登録データがあるか取得
            String MaxTempId = TemporaryFactory.GetMaxTempId(db);
            //取得したデータが""でない場合
            if (MaxTempId != "") {
                int number = Integer.parseInt(MaxTempId.substring(1));
                // 数値部分を1つ繰り上げる
                number += 1;
                tempId = "z" + number;
            }

            //データ格納用配列
            String[] dataList = new String[9];
            //データ格納
            Integer tempDate = changeFormatDateToInt(getTempData(findViewById(R.id.txtTempDay)));
//            dataList[0] = getTempData(findViewById(R.id.txtTempEmployeeCD));
//            dataList[1] = getTempData(findViewById(R.id.txtTempEmployeeName));
            int i = 0;    //dataList[0]～[6]までが仮置き情報
            for (View view : viewList) {
                dataList[i] = getTempData(view) + WithUnitRegister(i);
                i++;
            }
            dataList[7] = getTempData(findViewById(R.id.txtRemark));
            dataList[8] = no;

            //仮置きテーブルに追加
            TemporaryFactory.Insert(db, null, dataList,tempId);
        }catch(Exception ex){
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }

    }

    /**
     * 仮置きテーブルに追加(変更)
     */
    private void addTemporaryUpdate(ArrayList<View> viewList){
        DatabaseHelper dbHelper = new DatabaseHelper(this);
        SQLiteDatabase db = dbHelper.getWritableDatabase();

        try {
            //データ格納用配列
            String[] dataList = new String[9];
            //データ格納
            Integer tempDate = changeFormatDateToInt(getTempData(findViewById(R.id.txtTempDay)));
//            dataList[0] = getTempData(findViewById(R.id.txtTempEmployeeCD));
//            dataList[1] = getTempData(findViewById(R.id.txtTempEmployeeName));
            int i = 0;    //dataList[0]～[6]までが仮置き情報
            for (View view : viewList) {
                dataList[i] = getTempData(view) + WithUnitRegister(i);
                i++;
            }
            dataList[7] = getTempData(findViewById(R.id.txtRemark));
            dataList[8] = no;

            //仮置きテーブルに追加
                TemporaryFactory.Update(db, null, dataList, tempId, tempDate, no);
        }catch(Exception ex){
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }

    }

    /**
     * 画面の仮置き情報を取得
     * @param view
     * @return
     */
    private String getTempData(View view){
        if(view instanceof EditText){
            EditText editText = (EditText)view;
            return editText.getText().toString();
        }
        else if(view instanceof TextView){
            TextView textView = (TextView)view;
            return textView.getText().toString();
        }
        else if(view instanceof Spinner){
            Spinner spinner = (Spinner)view;
            return spinner.getSelectedItem().toString();
        }
        else{
            return "";
        }
    }

    private String WithUnitRegister(int index){
        if(!temporaryInputItem[index][3].equals("")){
            return "$$$"+temporaryInputItem[index][3];
        }
        return "";
    }

    private View GetView(String viewName,int index){
        int viewId = getResources().getIdentifier((viewName+index),"id",getPackageName());
        return findViewById(viewId);
    }

    private String nullCheck(String valueName){
        if (valueName.equals(null)){
                   valueName="";
        }
        return valueName;
    }

    /**
     * 入力チェック
     * @param view
     * @return
     */
    private boolean InputCheck(View view){
        if(view instanceof TextView){
            TextView txtView = (TextView) view;
            if(txtView.getText().toString().equals("")){
                return false;
            }
        }
        else if(view instanceof Spinner){
            Spinner spinner = (Spinner) view;
            if(spinner.getSelectedItem().equals("")){
                return false;
            }
        }
        return true;
    }

    private String DeleteUnit(String tempInfo){
        String delimiter ="$$$";
        int index = tempInfo.indexOf(delimiter);
        if(index!= -1){
            tempInfo = tempInfo.substring(0,index);
        }
        return tempInfo;
    }


    /**
     * メニューの設定処理
     * @param menu
     * @return
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_nosetting, menu);
        return true;
    }

    /**
     * メニュー変更イベント処理
     * @param menu
     * @return
     */
    @Override
    public boolean onPrepareOptionsMenu(Menu menu) {
        return true;
    }

    /**
     * 全てのボタンを活性/非活性にする
     */
    @Override
    protected void AllResetButton(boolean blnEnabled) {}

    /**
     * 警告メッセージコードを返却します
     * @return
     */
    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }

    private String getMonthDay(String dateStr) {

        // 月と日の部分を抽出
        String month = String.valueOf(Integer.parseInt(dateStr.substring(4, 6))); // 08 -> 8
        String day = dateStr.substring(6, 8); // 24

        // フォーマットして出力
        String formattedDate = month + "/" + day;

        return formattedDate;
    }

}
