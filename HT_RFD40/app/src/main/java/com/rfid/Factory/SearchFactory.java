package com.rfid.Factory;

import android.annotation.SuppressLint;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import com.rfid.common.AssetModel;
import com.rfid.common.CommonConst;
import com.rfid.common.bean.EntryProcessDataDetail;

import org.json.JSONArray;
import org.json.JSONException;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

public class SearchFactory implements Serializable {

    private HashMap<String, AssetModel> _mapSearch = new HashMap<String, AssetModel>();          //データ
    private HashMap<String, AssetModel> _mapSearchError = new HashMap<String, AssetModel>();    //エラーデータ
    private List<String> _listAssetNoRead = new ArrayList<String>();                                //資産No読込データ

    /**
     * 資産データを読み込みます。
     * @param db
     */
    @SuppressLint("Range")
    public HashMap<String, AssetModel> SelectAssetData(SQLiteDatabase db) {

        StringBuilder sql = new StringBuilder();
        sql.append(" SELECT");
        sql.append("  TEMP_SEARCH.ASSET_NO,");
        sql.append("  M_ASSET.ASSET_ID,");
        sql.append("  M_ASSET.ASSET_NAME,");
        sql.append("  M_ASSET.SUB_ASSET_NO");
        sql.append(" FROM ");
        sql.append(CommonConst.TEMP_SEARCH);
        sql.append(" LEFT JOIN ");
        sql.append(CommonConst.MASTER_ASSET);
        sql.append(" ON ");
        sql.append(CommonConst.MASTER_ASSET + ".ASSET_NO ");
        sql.append(" = ");
        sql.append(CommonConst.TEMP_SEARCH + ".ASSET_NO ");

        //SQL発行
        Cursor cursorAll = db.rawQuery(sql.toString(), null);

        while (cursorAll.moveToNext()) {
            AssetModel data  = new AssetModel();

            //各項目を設定する
            data.setAssetNo(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NO")));
            data.setAssetID(cursorAll.getString(cursorAll.getColumnIndex("ASSET_ID")));
            data.setAssetName(cursorAll.getString(cursorAll.getColumnIndex("ASSET_NAME")));
            data.setSubAssetNo(cursorAll.getString(cursorAll.getColumnIndex("SUB_ASSET_NO")));

            //データに溜め込む
            _mapSearch.put(data.getAssetNo(), data);

            if (data.getAssetName() == null) {
                //マスタがない場合、エラーデータにも溜め込む
                _mapSearchError.put(data.getAssetNo(), data);
            }

        }

        return _mapSearch;
    }

    /**
     * 資産番号を溜め込む
     * @param strData
     */
    public void StockAssetNo(String strData) {
        _listAssetNoRead.add(strData);
    }

    /**
     * 溜め込んだ資産Noを取得する
     * @return
     */
    public List<String> GetAssetNo() {
        return _listAssetNoRead;
    }

    /**
     * 資産Noからサブ資産Noを取得する
     * @return
     */
    public String GetSubAssetNo(String strAssetNo) {
        String strSubAssetNo= "";

        AssetModel model = (AssetModel)_mapSearch.get(strAssetNo);
        if (model != null) {
            strSubAssetNo = model.getSubAssetNo();
        }
        return strSubAssetNo;
    }

    /**
     * 資産番号を削除する
     * @param strData
     */
    public void DeleteAssetNo(String strData) {
        _mapSearch.remove(strData);
        _listAssetNoRead.remove(strData);
    }

    /**
     * 資産Noをチェックする
     * @param strData
     * @return
     */
    public boolean ExistAssetNo(String strData) {
        boolean blnReturn = false;

        if (_listAssetNoRead.contains(strData)) {
            blnReturn = true;
        }

        return blnReturn;
    }

    /**
     * 資産番号件数を取得
     * @return
     */
    public int CountAssetNo() {
        return _listAssetNoRead.size();
    }

    /**
     * 資産件数を取得
     * @return
     */
    public int CountAll() {
        return _listAssetNoRead.size();
    }

    /**
     * エラー件数を取得
     * @return
     */
    public int CountError() {
        return _mapSearchError.size();
    }

    /**
     * 資産番号から資産IDを検索する
     * @param strAssetNo
     * @return
     */
    public String GetAssetID(String strAssetNo) {
        String strAssetID = "";

        AssetModel model = (AssetModel)_mapSearch.get(strAssetNo);
        if (model != null) {
            strAssetID = model.getAssetID();
        }
        return strAssetID;
    }

    /** 資産データを取得する
     * @return
     */
    public HashMap<String, AssetModel>  GetSearchData() {
        return _mapSearch;
    }

    /**
     * データをクリアする
     */
    public void Clear(){
        _listAssetNoRead.clear();
        _mapSearch.clear();
    }
}
