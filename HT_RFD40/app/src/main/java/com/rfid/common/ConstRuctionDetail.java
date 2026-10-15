package com.rfid.common;

import static com.rfid.common.BaseRead.BUTTON_SUMMARY;

import android.app.Activity;
import android.content.Intent;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.location.Location;
import android.os.Bundle;
import android.util.Log;
import android.util.TypedValue;
import android.view.Menu;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.SimpleAdapter;
import android.widget.TextView;

import com.rfid.Dispose.DisposeRead;
import com.rfid.Factory.ConstRuctionFactory;
import com.rfid.Factory.LocationFactory;
import com.rfid.Factory.LogFactory;
import com.rfid.Factory.SettingFactory;
import com.rfid.Inventory.InventoryLocationSetting;
import com.rfid.Inventory.InventoryTagIn;
import com.rfid.Lease.LeaseRead;
import com.rfid.Lease.LeaseSetting;
import com.rfid.Return.ReturnRead;
import com.rfid.Search.SearchRead;
import com.rfid.Storage.StorageLocationSetting;
import com.rfid.Taglink.TaglinkRead;
import com.rfid.Temp.MatTempList;
import com.rfid.main.LoginMn;
import com.rfid.main.MainActivity;
import com.rfidpk.R;
import com.rfid.common.CommonCalender;

import org.w3c.dom.Text;

import java.security.spec.ECField;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * 工事データの一覧表示画面のクラスです。
 * 工事マスタからデータを取得し、一覧表示します。
 */
public class ConstRuctionDetail extends BaseActivity implements CommonCalender.Callback {
    // 前回選択されたアイテムのビュー
    private View previousSelectedView = null;
    // 前回選択されたアイテムのposition
    private int previousSelectedViewPosition = -1;
    // 選択したメニュー項目
    private String menu = "";
    //アクティビティ名
    private Activity activity = this;
    //工事番号
    private String no = "";
    //メニュー選択画面
    private String _MenuClick = CommonConst.MENU_CLICK.Inventory.toString();

    private String _LentEndDt = "";

    public static final int RESIST_OK = 5;

    /**
     * 画面表示処理
     * @param savedInstanceState
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_construction_detail);
        //タイトルバー 戻るボタン表示
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        //DB接続用変数
        SQLiteDatabase db = null;
        SQLiteDatabase dbLoc = null;

        try {
            Intent intent = getIntent();
            no = intent.getStringExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO);

            //メニューで選択したメニュー取得
            _MenuClick = intent.getStringExtra(CommonConst.VIEW_KEY_MENU_SELECT);
            //呼出元画面からタイトルを取得し表示する
            //((TextView)findViewById(R.id.lblListViewTitle)).setText(intent.getStringExtra(CommonConst.VIEW_KEY_LOCATION_TITLE));


            //データベースヘルパーのインスタンスを作成する
            DatabaseHelper dbHelper = new DatabaseHelper(this);
            db = dbHelper.getWritableDatabase();
            dbLoc = dbHelper.getWritableDatabase();

            //工具マスタからデータを取得し一覧表示する
            ViewConstRuctionData(db,dbLoc,no);

            //文言表示
            InitializeWord();

            //次へボタン　Clickイベント
            findViewById(R.id.btnSettingNext).setOnClickListener(new View.OnClickListener(){
                @Override
                public void onClick(View view){
                    menu = SystemSetting.getString(CommonConst.VIEW_KEY_MENU_SELECT,CommonConst.MENU_CLICK.Inventory.toString(),activity);
                    PageTransitionPlusNo(no);
                }
            });


        } catch (Exception ex){
            showErrorMessage(ex.getMessage());
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        } finally {
            db.close();
        }
    }

    /**
     * 文言表示
     */
    private void InitializeWord() {
        setTitle(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_TITLE,"",this));

        ((TextView) findViewById(R.id.labSubTitle))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_NEXT_PUSH, "", this));     //次へタップ

        ((TextView)findViewById(R.id.lbWorkDay))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SET_WORK_DAY, "", this));  //作業日
        ((TextView)findViewById(R.id.lbEmployee))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SET_LIABILITY, "", this));  //責任者
        ((TextView)findViewById(R.id.lbContent))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_SET_WORK_CONTENT, "", this));  //作業内容
        ((TextView)findViewById(R.id.lbLocation))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_LOCATION, "", this));  //管理拠点
        ((TextView)findViewById(R.id.lbWorkLoc))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_TEMP_WORK_LOC, "", this));  //作業場所
        ((TextView)findViewById(R.id.lbUpAndDown))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_UP_AND_DOWN, "", this));  //上り/下り
        ((TextView)findViewById(R.id.lbLineType))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_LINE_TYPE, "", this));  //線路種
        ((TextView)findViewById(R.id.lbStation))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_STATION, "", this));  //駅区間
        ((TextView)findViewById(R.id.lbDistance))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_DISTANCE, "", this));  //距離区間
        ((TextView)findViewById(R.id.lbTempInfo6))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO6, "", this));  //工事情報6
        ((TextView)findViewById(R.id.lbTempInfo7))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO7, "", this));  //工事情報7
        ((TextView)findViewById(R.id.lbTempInfo8))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO8, "", this));  //工事情報8
        ((TextView)findViewById(R.id.lbTempInfo9))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO9, "", this));  //工事情報9
        ((TextView)findViewById(R.id.lbTempInfo10))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO10, "", this));  //工事情報10
        ((TextView)findViewById(R.id.lbTempInfo11))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO11, "", this));  //工事情報11
        ((TextView)findViewById(R.id.lbTempInfo12))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO12, "", this));  //工事情報12
        ((TextView)findViewById(R.id.lbTempInfo13))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO13, "", this));  //工事情報13
        ((TextView)findViewById(R.id.lbTempInfo14))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO14, "", this));  //工事情報14
        ((TextView)findViewById(R.id.lbTempInfo15))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO15, "", this));  //工事情報15
        ((TextView)findViewById(R.id.lbTempInfo16))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO16, "", this));  //工事情報16
        ((TextView)findViewById(R.id.lbTempInfo17))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO17, "", this));  //工事情報17
        ((TextView)findViewById(R.id.lbTempInfo18))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO18, "", this));  //工事情報18
        ((TextView)findViewById(R.id.lbTempInfo19))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO19, "", this));  //工事情報19
        ((TextView)findViewById(R.id.lbTempInfo20))
                .setText(SystemSetting.getString(CommonConst.VIEW_WORD_CONSTRUCTION_DETAIL_SET_TEMPINFO20, "", this));  //工事情報20
    }

    public void EventButtonInventory(String strInventoryMethod){
        Intent intent = null;

        //設定マスタの実棚方法により画面遷移する
        switch (strInventoryMethod) {
            case CommonConst.SETTING_INVENTORY_METHOD_NONELOC:
                //実棚収集画面(ロケ指定なし)へ
                intent = new Intent(getApplication(), InventoryTagIn.class);
                break;
            case CommonConst.SETTING_INVENTORY_METHOD_LOC:
                //実棚収集画面(ロケ指定あり)へ
                intent = new Intent(getApplication(), InventoryLocationSetting.class);
                break;
            default:
                //実棚収集画面(ロケ指定なし)へ
                intent = new Intent(getApplication(), InventoryTagIn.class);
                break;
        }
        startActivity(intent);
    }


    /**
     * ロケーションマスタからデータを取得し、一覧表示する
     * @param db DB接続
     */
    private void ViewConstRuctionData(SQLiteDatabase db,SQLiteDatabase dbLoc,String no) {

        //SQL作成
        String strSql = new String();

        //SQL作成
        strSql = ConstRuctionFactory.createSQLData(no);

    try {
        //工事マスタからデータを取得
        Cursor cursor = db.rawQuery(strSql, null);
        cursor.moveToFirst();
        //ロケマスタからデータを取得
        //取得したデータを一覧表示に設定
        //作業日
        TextView workDate = findViewById(R.id.txtWorkDate);
        String strStartDay = changeFormatDate(cursor.getString(0));
        String strEndDay = changeFormatDate(cursor.getString(1));
        workDate.setText(strStartDay + "~" + strEndDay);
        //責任者
        TextView employeeName = findViewById(R.id.txtEmployeeName);
        employeeName.setText(nullCheck(cursor.getString(2)));
        //作業内容
        TextView contents = findViewById(R.id.txtContents);
        contents.setText(nullCheck(cursor.getString(3)));
        //管理拠点
        TextView loc = findViewById(R.id.txtLocation);
        loc.setText(nullCheck(LocationFactory.GetLocationLName(dbLoc, cursor.getString(4))));

        String txtName="txtInfo";
        int culumnIndex = 5;
        String constRuctionData[][] =SettingFactory.GetConstructionInputItem(db);

        for(int i=1;i<=20;i++){
            int viewId = getResources().getIdentifier((txtName+i),"id",getPackageName());
            TextView txtInfo = findViewById(viewId);
            String setTextValue = cursor.getString(culumnIndex);    //工事情報取得用

            if(constRuctionData[i-1][0].equals("LIST")) { //工事情報DATA1
                HashMap<String,String> item = SettingFactory.GetConstRuctionSelectItem(db,constRuctionData[i-1][2]);
                setTextValue = DataCheck(setTextValue,item);
            }
            else if (constRuctionData[i-1][0].equals("AREA")){
                setTextValue = DelimiterReplace(setTextValue);
            }
            else{
                //TEXTもしくはLABELの場合はそのまま出力
            }

            txtInfo.setText(nullCheck(setTextValue));
            culumnIndex++;
        }
        //工事貸出終了日保管
        _LentEndDt = nullCheck(cursor.getString(25));

    }catch(Exception ex){
        
    }
    }

    private String DataCheck(String value, HashMap<String,String> item){
            for(String key : item.keySet()) {
                if (key.equals(value)) {
                    return item.get(key);
                }
            }
            return "";
    }

    private String changeFormatDate(String date){
        String year = String.valueOf(Integer.parseInt(date.substring(0, 4)));
        String month = String.valueOf(Integer.parseInt(date.substring(4, 6)));
        String day = String.valueOf(Integer.parseInt(date.substring(6, 8)));
        String formattedDate = year + "/" + month + "/" +day;
        return formattedDate;
    }

    private String DelimiterReplace(String valueName){
        valueName = valueName.replace("$$$","~");
        return valueName;
    }

    private String nullCheck(String valueName){
        if (valueName==null){
            valueName="";
        }
        if(valueName.equals("null")){
            valueName="";
        }
        return valueName;
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

    private String getMonthDay(String dateStr) {

        // 月と日の部分を抽出
        String month = String.valueOf(Integer.parseInt(dateStr.substring(4, 6))); // 08 -> 8
        String day = dateStr.substring(6, 8); // 24

        // フォーマットして出力
        String formattedDate = month + "/" + day;

        return formattedDate;
    }

    /**
     * カレンダークリック時のイベント
     * @param strDate 日付
     */
    @Override
    public void onCalenderClick(String strDate) {
        String methodName = new Object(){}.getClass().getEnclosingMethod().getName();
        LogFactory.WriteFileLog(CommonConst.PROCESS_CAT.Event,getLocalClassName() + "," + methodName, this);

            ((TextView) findViewById(R.id.txtWorkDay)).setText(strDate);
        }


    /**
     * メニュー画面で選択した機能へ遷移する
     */
    private void PageTransitionPlusNo(String no) {

        Intent intent = null;
        SQLiteDatabase db = null;
        DatabaseHelper dbHelper = new DatabaseHelper(this);
        db = dbHelper.getWritableDatabase();

        //メニュー画面で選択した機能へ遷移する
        switch (_MenuClick) {

            case CommonConst.VIEW_WORD_MENU_LENDING:        //貸出
                intent = new Intent(getApplication(), CommonMatRead.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, no);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT, _LentEndDt);
                startActivityForResult(intent, BUTTON_SUMMARY);
                break;
            case CommonConst.VIEW_WORD_MENU_MATCHIN:           //持込照合
                intent = new Intent(getApplication(), CommonMatRead.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, no);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT, _LentEndDt);
                startActivityForResult(intent, BUTTON_SUMMARY);
                break;
            case CommonConst.VIEW_WORD_MENU_MATCHOUT:          //持帰照合
                intent = new Intent(getApplication(), CommonMatRead.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, no);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT, _LentEndDt);
                startActivityForResult(intent, BUTTON_SUMMARY);
                break;
            case CommonConst.VIEW_WORD_MENU_MATCHTEMPREGIST:   //仮置登録
//                intent = new Intent(getApplication(), CommonMatRead.class);
                //仮置きのデータがあるかチェック
                //ある場合仮置き一覧
                //intent = new Intent(getApplication(), MatTempList.class);
                //ない場合仮置き登録
				if(ConstRuctionFactory.GetTemporaryInfo(db,no).equals(CommonConst.TEMPINFO_FLAG.No.getString())){
                intent = new Intent(getApplication(),TemporaryRegister.class);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, no);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT, _LentEndDt);
                intent.putExtra(CommonConst.VIEW_KEY_STATE_SELECT,CommonConst.VIEW_KEY_STATE_REGISTER);
            }
                else{
                    intent = new Intent(getApplication(),MatTempList.class);
                    intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                    intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, no);
                    intent.putExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT, _LentEndDt);
                }

                //intent = new Intent(getApplication(), CommonMatRead.class);
                //intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                //intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, no);
                startActivityForResult(intent, BUTTON_SUMMARY);
                break;
            case CommonConst.VIEW_WORD_MENU_MATCHRETURN:       //照合返却
                intent = new Intent(getApplication(), CommonMatRead.class);
                intent.putExtra(CommonConst.VIEW_KEY_MENU_SELECT, _MenuClick);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONSTRUCTION_NO, no);
                intent.putExtra(CommonConst.VIEW_KEY_HT_CONST_LENT_END_DT, _LentEndDt);
                startActivityForResult(intent, BUTTON_SUMMARY);
                break;
            default:
                break;
        }
    }

    /**
     * 一覧画面から戻るイベント処理をします。
     * @param requestCode
     * @param resultCode
     * @param data
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data){
        try {
            switch(requestCode) {
                case BUTTON_SUMMARY:        //一覧画面から戻る

                    if(resultCode == RESIST_OK){
                        //画面を終了する
                        this.finish();
                    }

                default:
                    break;
            }
        } catch (Exception ex) {
            //ShowErrorMessage(ex.getMessage(), "");
            Log.e(CommonConst.DIALOG_ERROR_TITLE, Log.getStackTraceString(ex));
        }
    }

    @Override
    public void onMyDialogCancelled(int requestCode, Bundle params) {

    }
}
