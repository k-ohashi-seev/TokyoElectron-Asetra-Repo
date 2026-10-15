
''' <summary>
''' 定義クラス
''' </summary>
Public Class CommonConst

    ''' <summary>
    ''' 資産マスタ
    ''' </summary>
    Public Const MASTER_ASSET As String = "M_ASSET"
    ''' <summary>
    ''' 部署マスタ
    ''' </summary>
    Public Const MASTER_DEPARTMENT As String = "M_DEPARTMENT"
    ''' <summary>
    ''' 貸出先マスタ
    ''' </summary>
    Public Const MASTER_LEASE_PLACE As String = "M_LEASE_PLACE"
    ''' <summary>
    ''' ロケーションマスタ
    ''' </summary>
    Public Const MASTER_LOCATION As String = "M_LOCATION"
    ''' <summary>
    ''' カテゴリーマスタ
    ''' </summary>
    Public Const MASTER_CATEGORY As String = "M_CATEGORY"
    ''' <summary>
    ''' 社員マスタ
    ''' </summary>
    Public Const MASTER_EMPLOYEE As String = "M_EMPLOYEE"
    ''' <summary>
    ''' 文言マスタ
    ''' </summary>
    Public Const MASTER_WORD As String = "M_WORD"
    ''' <summary>
    ''' 設定マスタ
    ''' </summary>
    Public Const MASTER_SETTING As String = "M_SETTING"
    '2021/09/15 高瀬 v2.0 予約機能 START
    ''' <summary>
    ''' 権限マスタ
    ''' </summary>
    Public Const MASTER_AUTH As String = "M_AUTH"
    ''' <summary>
    ''' 社員所属拠点マスタ
    ''' </summary>
    Public Const MASTER_EMPLOYEE_HUB As String = "M_EMPLOYEE_HUB"
    '2021/09/15 高瀬 v2.0 予約機能 END
    ''' <summary>
    ''' 社員所属拠点マスタ＋共通ロケーションも拠点扱い
    ''' </summary>
    Public Const MASTER_EMPLOYEE_COMMON_HUB As String = "M_EMPLOYEE_COMMON_HUB"
    ''' <summary>
    ''' 工事マスタ
    ''' </summary>
    Public Const MASTER_CONSTRUCTION As String = "M_CONSTRUCTION"
    '''<summary>
    '''仮置きテーブル
    '''</summary>
    Public Const TABLE_TEMPORARY As String = "T_TEMPORARY"
    '''<summary>
    '''仮置き資産テーブル
    '''</summary>
    Public Const TABLE_TEMP_ASSET As String = "T_TEMP_ASSET"
    '''<summary>
    '''工事資産テーブル
    '''</summary>
    Public Const TABLE_CONST_ASSET As String = "T_CONST_ASSET"
    ''' <summary>
    ''' 棚卸管理テーブル
    ''' </summary>
    Public Const TABLE_INVENTORY As String = "T_INVENTORY"
    ''' <summary>
    ''' 棚卸明細テーブル
    ''' </summary>
    Public Const TABLE_INVENTORY_DETAIL As String = "T_INV_DETAIL"
    ''' <summary>
    ''' 棚卸資産テーブル
    ''' </summary>
    Public Const T_INV_ASSET As String = "T_INV_ASSET"
    ''' <summary>
    ''' 資産処理履歴テーブル
    ''' </summary>
    Public Const TABLE_PROCESS_LOG As String = "T_PROCESS_LOG"
    ''' <summary>
    ''' 操作ログテーブル
    ''' </summary>
    Public Const TABLE_OPERATION_LOG As String = "T_OPERATION_LOG"
    '2021/09/15 高瀬 v2.0 予約機能 START
    ''' <summary>
    ''' 予約テーブル
    ''' </summary>
    Public Const TABLE_RESERVE As String = "T_RESERVE"
    ''' <summary>
    ''' 予約明細テーブル
    ''' </summary>
    Public Const TABLE_RESERVE_DETAIL As String = "T_RESERVE_DETAIL"
    ''' <summary>
    ''' 予約明細テーブル
    ''' </summary>
    Public Const TABLE_RESERVE_MAINTE As String = "T_RESERVE_MAINTE"
    '2021/09/15 高瀬 v2.0 予約機能 END

    ''' <summary>
    ''' ロケ大区分
    ''' </summary>
    Public Const LOC_L_DIV As String = "L"
    ''' <summary>
    ''' ロケ中区分
    ''' </summary>
    Public Const LOC_M_DIV As String = "M"
    ''' <summary>
    ''' ロケ小区分
    ''' </summary>
    Public Const LOC_S_DIV As String = "S"

    ''' <summary>
    ''' カテゴリー大分類
    ''' </summary>
    Public Const CAT_L_DIV As String = "L"
    ''' <summary>
    ''' カテゴリー中分類
    ''' </summary>
    Public Const CAT_M_DIV As String = "M"
    ''' <summary>
    ''' カテゴリー小分類
    ''' </summary>
    Public Const CAT_S_DIV As String = "S"

    ''' <summary>
    ''' 言語区分 日本語
    ''' </summary>
    Public Const LANG_DIV_JA As String = "JA"
    ''' <summary>
    ''' 言語区分 英語
    ''' </summary>
    Public Const LANG_DIV_EN As String = "EN"
    ''' <summary>
    ''' 言語区分 使用言語
    ''' </summary>
    Public Const LANG_DIV_LU As String = "LU"

    ''' <summary>
    ''' 利用ツール区分 Web画面
    ''' </summary>
    Public Const WORD_TOOL_W As String = "W"
    ''' <summary>
    ''' 利用ツール区分 C/S画面
    ''' </summary>
    Public Const WORD_TOOL_C As String = "C"
    ''' <summary>
    ''' 利用ツール区分 HT画面
    ''' </summary>
    Public Const WORD_TOOL_H As String = "H"

    ''' <summary>
    ''' 文言区分 タイトル
    ''' </summary>
    Public Const WORD_DIV_T As String = "T"
    ''' <summary>
    ''' 文言区分 項目
    ''' </summary>
    Public Const WORD_DIV_I As String = "I"
    ''' <summary>
    ''' 文言区分 ボタン
    ''' </summary>
    Public Const WORD_DIV_B As String = "B"
    ''' <summary>
    ''' 文言区分 値
    ''' </summary>
    Public Const WORD_DIV_V As String = "V"
    ''' <summary>
    ''' 文言区分 メッセージ
    ''' </summary>
    Public Const WORD_DIV_M As String = "M"

    ''' <summary>
    ''' 処理ツール区分 Web画面
    ''' </summary>
    Public Const PROCESS_TOOL_W As String = "W"
    ''' <summary>
    ''' 処理ツール区分 C/S画面
    ''' </summary>
    Public Const PROCESS_TOOL_C As String = "C"
    ''' <summary>
    ''' 処理ツール区分 HT画面
    ''' </summary>
    Public Const PROCESS_TOOL_H As String = "H"

    ''' <summary>
    ''' 処理区分 タグ紐付2
    ''' </summary>
    Public Const PROCESS_DIV_T1 As String = "T1"
    ''' <summary>
    ''' 処理区分 タグ紐付2
    ''' </summary>
    Public Const PROCESS_DIV_T2 As String = "T2"
    ''' <summary>
    ''' 処理区分 保管
    ''' </summary>
    Public Const PROCESS_DIV_BS As String = "BS"
    ''' <summary>
    ''' 処理区分 貸出
    ''' </summary>
    Public Const PROCESS_DIV_BO As String = "BO"
    ''' <summary>
    ''' 処理区分 返却
    ''' </summary>
    Public Const PROCESS_DIV_BI As String = "BI"
    ''' <summary>
    ''' 処理区分 除却
    ''' </summary>
    Public Const PROCESS_DIV_BR As String = "BR"
    ''' <summary>
    ''' 処理区分 持帰照合
    ''' </summary>
    Public Const PROCESS_DIV_MI As String = "MI"
    ''' <summary>
    ''' 処理区分 持出照合
    ''' </summary>
    Public Const PROCESS_DIV_MO As String = "MO"
    ''' <summary>
    ''' 処理区分 照合返却
    ''' </summary>
    Public Const PROCESS_DIV_BM As String = "BM"
    ''' <summary>
    ''' 処理区分 仮置き
    ''' </summary>
    Public Const PROCESS_DIV_TM As String = "TM"

    ''' <summary>
    ''' 資産状態 未紐付け
    ''' </summary>
    Public Const ASSET_STATUS_NOT_LINK As String = "0"
    ''' <summary>
    ''' 資産状態 在庫
    ''' </summary>
    Public Const ASSET_STATUS_INV As String = "1"
    ''' <summary>
    ''' 資産状態 貸出
    ''' </summary>
    Public Const ASSET_STATUS_LEASE As String = "2"
    ''' <summary>
    ''' 資産状態 除却
    ''' </summary>
    Public Const ASSET_STATUS_RETIRE As String = "9"

    ''' <summary>
    ''' 貸出状態 不可
    ''' </summary>
    Public Const LEASE_STATUS_NOT As String = "0"

    ''' <summary>
    ''' 実棚ステータス：値
    ''' </summary>
    Public Const PHYINV_STATUS_VALUE As String = "Shelf_status"

    ''' <summary>
    ''' 実棚ステータス：対象外
    ''' </summary>
    Public Const PHYINV_STATUS_EXCLUDED As String = "0"

    ''' <summary>
    ''' 実棚ステータス：正常
    ''' </summary>
    Public Const PHYINV_STATUS_NORMAL As String = "N"

    ''' <summary>
    ''' 実棚ステータス：現物無し
    ''' </summary>
    Public Const PHYINV_STATUS_ERROR As String = "E"

    ''' <summary>
    ''' 実棚ステータス：ロケ違い
    ''' </summary>
    Public Const PHYINV_STATUS_WARN As String = "W"

    ''' <summary>
    '''処理種別 イベント
    ''' </summary>
    Public Const PROCESS_CAT_EV As String = "EV"

    ''' <summary>
    '''処理種別 操作
    ''' </summary>
    Public Const PROCESS_CAT_OP As String = "OP"

    ''' <summary>
    '''処理種別 エラー
    ''' </summary>
    Public Const PROCESS_CAT_ER As String = "ER"

    ''' <summary>
    '''処理結果 正常
    ''' </summary>
    Public Const TASK_RESULT_N As String = "N"

    ''' <summary>
    '''処理種別 エラー
    ''' </summary>
    Public Const TASK_RESULT_E As String = "E"

    '-- 2020/02/12 v1.2 戸祭 貸出上限機能 START --
    ''' <summary>
    '''貸出アラート区分 上位に準ずる
    ''' </summary>
    Public Const LEASE_ALERT_DIV_F As String = "F"

    ''' <summary>
    '''貸出アラート区分 アラート表示しない
    ''' </summary>
    Public Const LEASE_ALERT_DIV_N As String = "N"

    ''' <summary>
    '''貸出アラート区分 アラート表示する
    ''' </summary>
    Public Const LEASE_ALERT_DIV_Y As String = "Y"

    ''' <summary>
    '''貸出上限エラー区分 エラー
    ''' </summary>
    Public Const LEASE_UPPER_ERR_DIV_ERR As String = "Y"

    ''' <summary>
    '''貸出上限エラー区分 警告
    ''' </summary>
    Public Const LEASE_UPPER_ERR_DIV_WARN As String = "N"
    '-- 2020/02/12 v1.2 戸祭 貸出上限機能 END --

    '2021/09/15 高瀬 v2.0 予約機能 START

    ''' <summary>
    ''' キー区分　予約機能
    ''' </summary>
    Public Const RESERVATION_FUNCTION As String = "Reservation_Function"

    ''' <summary>
    ''' キー区分　リードタイム
    ''' </summary>
    Public Const LENDING_LT_RESERVE_DATA As String = "Lending_LT_ReserveData"

    ''' <summary>
    ''' 予約機能　あり
    ''' </summary>
    Public Const RESERVATION_FUNCTION_YES As String = "Y"

    ''' <summary>
    ''' 予約機能　あり
    ''' </summary>
    Public Const RESERVATION_FUNCTION_NO As String = "N"

    ''' <summary>
    ''' 稼働日区分：J（実績）
    ''' </summary>
    Public Const DATA_DIV_J As String = "J"
    ''' <summary>
    ''' 稼働日区分：K（稼働日）
    ''' </summary>
    Public Const DATA_DIV_K As String = "K"

    ''' <summary>
    ''' 予約ステータス　未貸出
    ''' </summary>
    Public Const RESERVE_STATUS_UNRENTED As String = "0"

    ''' <summary>
    ''' 予約ステータス　貸出中
    ''' </summary>
    Public Const RESERVE_STATUS_RENTED As String = "1"

    ''' <summary>
    ''' 予約ステータス　返却済み
    ''' </summary>
    Public Const RESERVE_STATUS_RERUENED As String = "2"

    ''' <summary>
    ''' デフォルトリードタイム
    ''' </summary>
    Public Const DEFAULT_LEAD_TIME As String = "0"

    ''' <summary>
    ''' 稼働日数/実日数区分　実日数
    ''' </summary>
    Public Const RETURN_ACTUAL_DAYS_DIV As String = "J"
    '2021/09/15 高瀬 v2.0 予約機能 END

    '2022/07/25 佐藤 v2.2 拠点ロケ更新 START
    ''' <summary>
    ''' キー区分　拠点ロケ更新
    ''' </summary>
    Public Const RETURN_UPDATE_LOCATION As String = "Return_Update_Location"

    ''' <summary>
    ''' 拠点ロケ更新　あり
    ''' </summary>
    Public Const RETURN_LOC_UPDATE_YES As String = "Y"

    ''' <summary>
    ''' 拠点ロケ更新　なし
    ''' </summary>
    Public Const RETURN_LOC_UPDATE_NO As String = "N"
    '2022/07/25 佐藤 v2.2 拠点ロケ更新 END

    '2023/01/31 v2.3 Asetra 資産１００万件対応 川元 START
    ''' <summary>
    ''' キー区分　ロケごとの資産登録上限数
    ''' </summary>
    Public Const ASSET_REGISTRATION_LIMIT As String = "Asset_Registration_Limit"
    '2023/01/31 v2.3 Asetra 資産１００万件対応 川元 END

    '2023/04/11 川元 v2.4 ADD START
    ''' <summary>
    ''' キー区分　薬品管理機能
    ''' </summary>
    Public Const KEY_DRUGS_FUNCTION As String = "Drugs_Function"

    ''' <summary>
    ''' キー区分　照合機能
    ''' </summary>
    Public Const KEY_MATCHING_FUNCTION As String = "Matching_Function"

    ''' <summary>
    ''' キー区分　照合予約機能
    ''' </summary>
    Public Const KEY_MATCHING_RESERVE_FUNCTION As String = "Matching_Reserve_Function"

    Public Class DRUGS_FUNCTION
        ''' <summary>あり</summary>
        Public Const YES As String = "Y"
        ''' <summary>なし</summary>
        Public Const NO As String = "N"
    End Class

    ''' <summary>
    ''' 照合機能
    ''' </summary>
    Public Class MATCHING_FUNCTION
        ''' <summary>あり</summary>
        Public Const YES As String = "Y"
        ''' <summary>なし</summary>
        Public Const NO As String = "N"
    End Class

    ''' <summary>
    ''' 実稼働日
    ''' </summary>
    Public Class WORK_FLAG
        ''' <summary>あり</summary>
        Public Const YES As String = "Y"
        ''' <summary>なし</summary>
        Public Const NO As String = "N"
    End Class

    ''' <summary>
    ''' 前回の仮置きフラグ
    ''' </summary>
    Public Class PREV_TEMP_FLAG
        ''' <summary>前回仮置きした</summary>
        Public Const YES As String = "Y"
        ''' <summary>前回仮置きしていない</summary>
        Public Const NO As String = "N"
    End Class

    ''' <summary>
    '''    ''' 仮置きフラグ
    ''' </summary>
    Public Class TEMP_FLAG
        ''' <summary>仮置きした</summary>
        Public Const YES As String = "Y"
        ''' <summary>仮置きしていない</summary>
        Public Const NO As String = "N"
    End Class
    ''' <summary>
    ''' 薬品作業区分
    ''' </summary>
    Public Class DRUG_WORK_DIV
        ''' <summary>受入</summary>
        Public Const ACCEPT As String = "1"
        ''' <summary>返却</summary>
        Public Const RET As String = "2"
        ''' <summary>確認</summary>
        Public Const CONFIRM As String = "3"
    End Class

    ''' <summary>
    ''' 代替品番予約更新ストアドエラー区分
    ''' </summary>
    Public Class RESERV_UPDATE_ERR_DIV
        ''' <summary>予約エラー</summary>
        Public Const RESERV_ERR As String = "8"
        ''' <summary>期限エラー</summary>
        Public Const LIMIT_ERRT As String = "9"

    End Class

    ''' <summary>
    ''' 工事作業区分
    ''' </summary>
    Public Class CONST_WORK_DIV
        ''' <summary>工事登録</summary>
        Public Const CONST_REGIST As String = "1"
        ''' <summary>仮置き紐付</summary>
        Public Const TEMP_LINK As String = "2"
        ''' <summary>貸出</summary>
        Public Const LENT As String = "3"
        ''' <summary>持込照合</summary>
        Public Const MATCH_IN As String = "4"
        ''' <summary>仮置き登録</summary>
        Public Const TEMP_REGIST As String = "5"
        ''' <summary>持ち帰照合</summary>
        Public Const MATCH_OUT As String = "6"
        ''' <summary>照合返却</summary>
        Public Const MATCH_RETURN As String = "7"
        ''' <summary>返却</summary>
        Public Const [RETURN] As String = "8"
        ''' <summary>工事完了</summary>
        Public Const CONST_COMPLETE As String = "9"
        ''' <summary>別工事紐</summary>
        Public Const OTHER_CONST_LINK As String = "10"

    End Class

    ''' <summary>
    ''' 工事資産状態
    ''' </summary>
    Public Class CONST_ASSET_STATUS
        ''' <summary>貸出中</summary>
        Public Const CONST_LENT As String = "1"
        ''' <summary>予約中</summary>
        Public Const CONST_RESERVE As String = "2"
        ''' <summary>返却済み</summary>
        Public Const CONST_RETURN As String = "3"

    End Class

    ''' <summary>
    ''' 照合結果区分
    ''' </summary>
    Public Class CONST_MATCH_DIV
        ''' <summary>照合なし</summary>
        Public Const NO As String = "0"
        ''' <summary>照合OK</summary>
        Public Const OK As String = "1"
        ''' <summary>照合リスト外</summary>
        Public Const OTHER_LIST As String = "8"
        ''' <summary>照合NG</summary>
        Public Const NG As String = "9"
        ''' <summary>未照合</summary>
        Public Const MI As String = "E"

    End Class

    ''' <summary>
    ''' 退職フラグ
    ''' </summary>
    Public Class RETIRE_FLG
        ''' <summary>退職済</summary>
        Public Const YES As String = "Y"
        ''' <summary>就業中</summary>
        Public Const NO As String = "N"
    End Class

    ''' <summary>
    ''' 予約ステータス
    ''' </summary>
    Public Class RESERVE_STATUS
        ''' <summary>未貸出</summary>
        Public Const MI As String = "0"
        ''' <summary>貸出中</summary>
        Public Const LENDING As String = "1"
        ''' <summary>返却済み</summary>
        Public Const HENKYAKUSUMI As String = "2"
    End Class

    ''' <summary>
    ''' 貸出返却区分
    ''' </summary>
    Public Class LENT_RET_FLG
        ''' <summary>貸出</summary>
        Public Const LENT As String = "L"
        ''' <summary>返却</summary>
        Public Const RET As String = "R"
    End Class

    ''' <summary>
    ''' 処理名：貸出登録
    ''' </summary>
    Public Const TASK_LENT_REGIST As String = "貸出登録"

    ''' <summary>
    ''' 処理名：返却登録
    ''' </summary>
    Public Const TASK_RETURN_REGIST As String = "返却登録"

    ''' <summary>
    ''' 処理名：照合返却登録
    ''' </summary>
    Public Const TASK_MATCH_RETURN_REGIST As String = "照合返却登録"

    ''' <summary>
    ''' 処理名：照合登録
    ''' </summary>
    Public Const TASK_MATCH_REGIST As String = "照合登録"

    ''' <summary>
    ''' 処理名：仮置き登録
    ''' </summary>
    Public Const TASK_TEMP_REGIST As String = "仮置き登録"

    ''' <summary>
    ''' キー区分　危険物の指定数量
    ''' </summary>
    Public Const KEY_DRUGS_FSACT_NUMBER As String = "Drugs_FSAct_Number"
    ''' <summary>
    ''' 
    ''' 危険物の指定数量
    ''' </summary>
    Public Class DRUGS_FSACT_NUMBER
        'Public Class Data3
        '    ''' <summary>単位：L</summary>
        '    Public Shared L As String = "L"
        '    ''' <summary>単位：KG</summary>
        '    Public Shared KG As String = "KG"
        'End Class
    End Class
    '2023/04/11 川元 v2.4 ADD END

    ''' <summary>
    ''' メッセージ：予期せぬエラー
    ''' </summary>
    Public Const MSG_EXCEPTION As String = "An unexpected error has occurred."

    ''' <summary>
    ''' メッセージ：DB接続エラー
    ''' </summary>
    Public Const MSG_ERR_DB As String = "Cannot connect to DB."

    ''' <summary>
    ''' メッセージ：引数不正
    ''' </summary>
    Public Const MSG_INV_ARG As String = "Invalid argument."

End Class


