
''' <summary>
''' 送信用マスタデータパラメータ
''' </summary>
Public Class HtGetMasterDataCmdResParameter
    Inherits CommonResParameter

    ''' <summary>
    ''' 資産マスタデータ
    ''' </summary>
    Property adl As AssetData()             '2012/02/13 戸祭 v1.1 変数名を変更

    '''' <summary>
    '''' 貸出先マスタデータ
    '''' </summary>
    'Property leasePlaceDataList As LeasePlaceData()

    ''' <summary>
    ''' ロケーションマスタデータ
    ''' </summary>
    Property locationDataList As LocationData()

    ''' <summary>
    ''' 社員マスタデータ
    ''' </summary>
    Property employeeDataList As EmployeeData()

    ''' <summary>
    ''' 文言マスタデータ
    ''' </summary>
    Property wordDataList As WordData()

    ''' <summary>
    ''' 設定マスタデータ
    ''' </summary>
    Property settingDataList As SettingData()

    '2021/09/15 高瀬 v2.0 予約機能 START
    ''' <summary>
    ''' 権限マスタデータ
    ''' </summary>
    ''' <returns></returns>
    Property authDataList As AuthData()

    '''' <summary>
    '''' 社員所属拠点マスタデータ
    '''' </summary>
    '''' <returns></returns>
    'Property employeeHubDataList As EmployeeHubData()
    '2021/09/15 高瀬 v2.0 予約機能 END
    ''' <summary>
    ''' 棚卸開始情報データ
    ''' </summary>
    ''' <returns></returns>
    Property invList As InvStartData()

    '''' <summary>
    '''' 工事情報データ
    '''' </summary>
    '''' <returns></returns>
    'Property constList As ConstRuctionData()

    '''' <summary>
    '''' 仮置き情報データ
    '''' </summary>
    '''' <returns></returns>
    'Property temporaryList As TemporaryData()

    '''' <summary>
    '''' 仮置き資産情報データ
    '''' </summary>
    '''' <returns></returns>
    'Property tempAssetList As TempAssetData()

    '''' <summary>
    '''' 照合情報データ
    '''' </summary>
    '''' <returns></returns>
    'Property matchList As MatchData()

    '''' <summary>
    '''' 予約情報データ
    '''' </summary>
    '''' <returns></returns>
    'Property resList As ReserveData()
End Class

''' <summary>
''' 資産マスタデータ
''' </summary>
Public Class AssetData
    '2012/02/13 戸祭 v1.1 変数名を変更　START
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Property no As String

    ''' <summary>
    ''' 資産ID
    ''' </summary>
    Property id As String

    ''' <summary>
    ''' 資産名称
    ''' </summary>
    Property nm As String

    ''' <summary>
    ''' 型番
    ''' </summary>
    Property mn As String

    ''' <summary>
    ''' ロケ大区分コード
    ''' </summary>
    Property llcd As String

    ''' <summary>
    ''' ロケ中区分コード
    ''' </summary>
    Property lmcd As String

    ''' <summary>
    ''' ロケ小区分コード
    ''' </summary>
    Property lscd As String

    ''' <summary>
    ''' 資産ID登録方法
    ''' </summary>
    Property reg As Integer

    ''' <summary>
    ''' EPCサイズ
    ''' </summary>
    Property sz As Integer

    ''' <summary>
    ''' 資産区分
    ''' </summary>
    Property st As String
    '2012/02/13 戸祭 v1.1 変数名を変更　END

    '-- v.1.1 戸祭 連続棚卸対応 start --
    ''' <summary>
    ''' 実棚済み棚卸フラグ(Y/M)
    ''' </summary>
    Property pif As String

    ''' <summary>
    ''' 実棚ステータス
    ''' </summary>
    Property pis As String
    '-- v.1.1 戸祭 連続棚卸対応 end --

    ''' <summary>
    ''' 貸出警告数
    ''' </summary>
    Property ual As String

    '-- 2020/02/12 v1.2 戸祭 貸出上限機能 START --
    ''' <summary>
    ''' 貸出数
    ''' </summary>
    Property ln As String
    ''' <summary>
    ''' 貸出上限エラー区分(Y：エラー、N：警告、NULL：チェックなし)
    ''' </summary>
    Property ule As String
    ''' <summary>
    ''' 貸出上限数
    ''' </summary>
    Property ul As String
    '-- 2020/02/12 v1.2 戸祭 貸出上限機能 END --

    '2021/09/15 高瀬 v2.0 予約機能 START
    ''' <summary>
    ''' 管理ロケ大区分コード
    ''' </summary>
    ''' <returns></returns>
    Property mll As String

    ''' <summary>
    ''' 貸出状態
    ''' </summary>
    ''' <returns></returns>
    Property ls As String
    '2021/09/15 高瀬 v2.0 予約機能 END

    '2023/08/25 戸祭 v2.4.1 ADD START
    ''' <summary>
    ''' 貸出返却予定日
    ''' </summary>
    ''' <returns></returns>
    Property rdt As String
    ''' <summary>
    ''' リース期限
    ''' </summary>
    ''' <returns></returns>
    Property llt As String
    ''' <summary>
    ''' ○○期限
    ''' </summary>
    ''' <returns></returns>
    Property qlt As String
    '2023/08/25 戸祭 v2.4.1 ADD END
    ''' <summary>
    ''' 資産サブ番号
    ''' </summary>
    ''' <returns></returns>
    Property subno As String

    ''' <summary>
    ''' 有形/無形区分
    ''' </summary>
    ''' <returns></returns>
    Property istangible As String

    ''' <summary>
    ''' 棚卸対象区分
    ''' </summary>
    ''' <returns></returns>
    Property invflg As String
End Class

''' <summary>
''' 貸出先マスタデータ
''' </summary>
Public Class LeasePlaceData
    ''' <summary>
    ''' 貸出先コード
    ''' </summary>
    Property leasePlaceCd As String

    ''' <summary>
    ''' 貸出先名
    ''' </summary>
    Property leasePlaceName As String
End Class

''' <summary>
''' ロケーションマスタデータ
''' </summary>
Public Class LocationData
    ''' <summary>
    ''' ロケ区分
    ''' </summary>
    Property locDiv As String

    ''' <summary>
    ''' ロケ大区分コード
    ''' </summary>
    Property locLCd As String

    ''' <summary>
    ''' ロケ中区分コード
    ''' </summary>
    Property locMCd As String

    ''' <summary>
    ''' ロケ小区分コード
    ''' </summary>
    Property locSCd As String

    ''' <summary>
    ''' ロケ名称
    ''' </summary>
    Property locName As String

    '2021/09/15 高瀬 v2.0 予約機能 START
    '''' <summary>
    '''' 共通拠点フラグ
    '''' </summary>
    '''' <returns></returns>
    'Property locShareFlg As String
    '2021/09/15 高瀬 v2.0 予約機能 END

    ''' <summary>
    ''' RFIDタグ拠点
    ''' </summary>
    ''' <returns></returns>
    Property locRFIDFlg As String

End Class

''' <summary>
''' 社員マスタデータ
''' </summary>
Public Class EmployeeData
    ''' <summary>
    ''' 社員コード
    ''' </summary>
    Property employeeCd As String

    ''' <summary>
    ''' 社員名
    ''' </summary>
    Property employeeName As String

    '2021/09/15 高瀬 v2.0 予約機能 START
    ''' <summary>
    ''' 権限区分
    ''' </summary>
    ''' <returns></returns>
    Property authDiv As String

    ''' <summary>
    ''' 本部フラグ
    ''' </summary>
    ''' <returns></returns>
    Property headOfficeFlg As String
    '2021/09/15 高瀬 v2.0 予約機能 END

End Class

'2021/09/15 高瀬 v2.0 予約機能 START
''' <summary>
''' 社員所属拠点マスタ
''' </summary>
Public Class EmployeeHubData
    ''' <summary>
    ''' 社員コード
    ''' </summary>
    ''' <returns></returns>
    Property employeeCd As String

    ''' <summary>
    ''' 拠点ロケ大コード
    ''' </summary>
    ''' <returns></returns>
    Property hubLocLCd As String

End Class
'2021/09/15 高瀬 v2.0 予約機能 END

''' <summary>
''' 文言マスタデータ
''' </summary>
Public Class WordData
    ''' <summary>
    ''' 文言区分
    ''' </summary>
    Property wordDiv As String

    ''' <summary>
    ''' 文言コード
    ''' </summary>
    Property wordCd As String

    ''' <summary>
    ''' 文言
    ''' </summary>
    Property wording As String
End Class

''' <summary>
''' 設定マスタデータ
''' </summary>
Public Class SettingData
    ''' <summary>
    ''' KEY種別
    ''' </summary>
    Property keyDiv As String

    ''' <summary>
    ''' KEY1
    ''' </summary>
    Property key1 As String

    ''' <summary>
    ''' KEY2
    ''' </summary>
    Property key2 As String

    ''' <summary>
    ''' KEY3
    ''' </summary>
    Property key3 As String

    ''' <summary>
    ''' 備考
    ''' </summary>
    Property remark As String

    ''' <summary>
    ''' DATA1
    ''' </summary>
    Property data1 As String

    ''' <summary>
    ''' DATA2
    ''' </summary>
    Property data2 As String

    ''' <summary>
    ''' DATA3
    ''' </summary>
    Property data3 As String

    ''' <summary>
    ''' DATA4
    ''' </summary>
    Property data4 As String

    ''' <summary>
    ''' DATA5
    ''' </summary>
    Property data5 As String

    ''' <summary>
    ''' DATA6
    ''' </summary>
    Property data6 As String

    ''' <summary>
    ''' DATA7
    ''' </summary>
    Property data7 As String

    ''' <summary>
    ''' DATA8
    ''' </summary>
    Property data8 As String

    ''' <summary>
    ''' DATA9
    ''' </summary>
    Property data9 As String

    ''' <summary>
    ''' DATA10
    ''' </summary>
    Property data10 As String

    ''' <summary>
    ''' 特定データ区分
    ''' </summary>
    Property specialDataDiv As String
End Class

'2021/09/15 高瀬 v2.0 予約機能 START
''' <summary>
''' 権限マスタデータ
''' </summary>
Public Class AuthData

    ''' <summary>
    ''' 処理ツール区分
    ''' </summary>
    ''' <returns></returns>
    Property processToolDiv As String

    ''' <summary>
    ''' 機能ID
    ''' </summary>
    ''' <returns></returns>
    Property functionId As String

    ''' <summary>
    ''' 権限区分
    ''' </summary>
    ''' <returns></returns>
    Property authDiv As String

    ''' <summary>
    ''' 権限タイプ
    ''' </summary>
    ''' <returns></returns>
    Property authType As String

    ''' <summary>
    ''' 本部フラグ
    ''' </summary>
    ''' <returns></returns>
    Property headOfficeFlg As String

    ''' <summary>
    ''' 参照権限
    ''' </summary>
    ''' <returns></returns>
    Property readAuthFlg As String

    ''' <summary>
    ''' 更新権限
    ''' </summary>
    ''' <returns></returns>
    Property updAuthFlg As String

    ''' <summary>
    ''' 参照権限(他拠点)
    ''' </summary>
    ''' <returns></returns>
    Property readHubAuthFlg As String

    ''' <summary>
    ''' 更新権限(他拠点)
    ''' </summary>
    ''' <returns></returns>
    Property updHubAuthFlg As String

    ''' <summary>
    ''' 特殊権限
    ''' </summary>
    ''' <returns></returns>
    Property authOption As String

End Class
'2021/09/15 高瀬 v2.0 予約機能 END

''' <summary>
''' 棚卸開始情報データ
''' </summary>
Public Class InvStartData

    ''' <summary>
    ''' 棚卸開始大ロケコード
    ''' </summary>
    ''' <returns></returns>
    Property invLocLCd As String

End Class

''' <summary>
''' 工事マスタデータ
''' </summary>
Public Class ConstRuctionData
    ''' <summary>
    ''' 工事番号
    ''' </summary>
    Property no As String

    ''' <summary>
    ''' 工事開始日
    ''' </summary>
    Property stDt As Integer

    ''' <summary>
    ''' 工事終了日
    ''' </summary>
    Property endDt As Integer

    ''' <summary>
    ''' 工事貸出開始日
    ''' </summary>
    Property lentStDt As Integer

    ''' <summary>
    ''' 工事貸出終了日
    ''' </summary>
    Property lentEndDt As Integer

    ''' <summary>
    ''' 工事内容
    ''' </summary>
    Property contents As String

    ''' <summary>
    ''' 工事責任者コード
    ''' </summary>
    Property empCd As String

    ''' <summary>
    ''' 工事責任者氏名
    ''' </summary>
    Property empName As String

    ''' <summary>
    ''' 管理ロケ大区分コード
    ''' </summary>
    Property mngLocLCd As String

    ''' <summary>
    ''' 工事情報1
    ''' </summary>
    Property info1 As String

    ''' <summary>
    ''' 工事情報2
    ''' </summary>
    Property info2 As String

    ''' <summary>
    ''' 工事情報3
    ''' </summary>
    Property info3 As String

    ''' <summary>
    ''' 工事情報4
    ''' </summary>
    Property info4 As String

    ''' <summary>
    ''' 工事情報5
    ''' </summary>
    Property info5 As String

    ''' <summary>
    ''' 工事情報6
    ''' </summary>
    Property info6 As String

    ''' <summary>
    ''' 工事情報7
    ''' </summary>
    Property info7 As String

    ''' <summary>
    ''' 工事情報8
    ''' </summary>
    Property info8 As String

    ''' <summary>
    ''' 工事情報9
    ''' </summary>
    Property info9 As String

    ''' <summary>
    ''' 工事情報10
    ''' </summary>
    Property info10 As String

    ''' <summary>
    ''' 工事情報11
    ''' </summary>
    Property info11 As String

    ''' <summary>
    ''' 工事情報12
    ''' </summary>
    Property info12 As String

    ''' <summary>
    ''' 工事情報13
    ''' </summary>
    Property info13 As String

    ''' <summary>
    ''' 工事情報14
    ''' </summary>
    Property info14 As String

    ''' <summary>
    ''' 工事情報15
    ''' </summary>
    Property info15 As String

    ''' <summary>
    ''' 工事情報16
    ''' </summary>
    Property info16 As String

    ''' <summary>
    ''' 工事情報17
    ''' </summary>
    Property info17 As String

    ''' <summary>
    ''' 工事情報18
    ''' </summary>
    Property info18 As String

    ''' <summary>
    ''' 工事情報19
    ''' </summary>
    Property info19 As String

    ''' <summary>
    ''' 工事情報20
    ''' </summary>
    Property info20 As String

    ''' <summary>
    ''' 仮置き情報データ
    ''' </summary>
    ''' <returns></returns>
    Property temporaryList As TemporaryData()

    ''' <summary>
    ''' 照合情報データ
    ''' </summary>
    ''' <returns></returns>
    Property matchList As MatchData()

End Class

''' <summary>
''' 仮置き情報明細
''' </summary>
Public Class TemporaryData
    ''' <summary>
    ''' 仮置きID
    ''' </summary>
    Property tempId As Long

    ''' <summary>
    ''' 仮置き日
    ''' </summary>
    Property tempDt As Integer

    '''' <summary>
    '''' 作業責任者コード
    '''' </summary>
    'Property tempEmpCd As String

    '''' <summary>
    '''' 作業責任者名
    '''' </summary>
    'Property tempEmpName As String

    ''' <summary>
    ''' 仮置き情報1
    ''' </summary>
    Property tempInfo1 As String

    ''' <summary>
    ''' 仮置き情報2
    ''' </summary>
    Property tempInfo2 As String

    ''' <summary>
    ''' 仮置き情報3
    ''' </summary>
    Property tempInfo3 As String

    ''' <summary>
    ''' 仮置き情報4
    ''' </summary>
    Property tempInfo4 As String

    ''' <summary>
    ''' 仮置き情報5
    ''' </summary>
    Property tempInfo5 As String

    ''' <summary>
    ''' 仮置き情報6
    ''' </summary>
    Property tempInfo6 As String

    ''' <summary>
    ''' 仮置き情報7
    ''' </summary>
    Property tempInfo7 As String

    ''' <summary>
    ''' 備考
    ''' </summary>
    Property tempRemark As String

    ''' <summary>
    ''' 照合情報明細
    ''' </summary>
    ''' <returns></returns>
    Property tempAssetList As TempAssetData()
End Class

''' <summary>
''' 仮置き資産情報明細
''' </summary>
Public Class TempAssetData
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Property no As String
End Class

''' <summary>
''' 照合情報明細
''' </summary>
Public Class MatchData
    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Property no As String
End Class

''' <summary>
''' 予約情報明細
''' </summary>
Public Class ReserveData

    ''' <summary>
    ''' 予約番号
    ''' </summary>
    Property reserveId As Long

    ''' <summary>
    ''' 工事番号
    ''' </summary>
    Property constNo As String

    ''' <summary>
    ''' 工事内容
    ''' </summary>
    Property contents As String

    ''' <summary>
    ''' 担当者社員コード
    ''' </summary>
    Property emp As String

    ''' <summary>
    ''' 実質貸出開始日
    ''' </summary>
    Property resStDt As Integer

    ''' <summary>
    ''' 実質貸出終了日
    ''' </summary>
    Property resEndDt As Integer

    ''' <summary>
    ''' 資産番号
    ''' </summary>
    Property no As String
End Class