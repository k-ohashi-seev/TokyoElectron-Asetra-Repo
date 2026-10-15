Namespace Misc

    Public Class CommonLabel

        '-----タイトル(T)-----

        ''' <summary>
        ''' システム名
        ''' </summary>
        Public Const TITLE_SYSTEM As String = "WT990001"

        ''' <summary>
        ''' 検索条件
        ''' </summary>
        Public Const TITLE_SEARCH As String = "WT990002"

        ''' <summary>
        ''' ログイン画面下部コピーライト表現
        ''' </summary>
        Public Const TITLE_COPYRIGHT As String = "WT990099"

        ''' <summary>
        ''' メニュー:資産
        ''' </summary>
        Public Const TITLE_ASSET As String = "WT990101"

        ''' <summary>
        ''' メニュー:棚卸
        ''' </summary>
        Public Const TITLE_INVENTORY As String = "WT990102"

        ''' <summary>
        ''' メニュー:マスタメンテ
        ''' </summary>
        Public Const TITLE_MST As String = "WT990103"

        ''' <summary>
        ''' メニュー:ログ照会
        ''' </summary>
        Public Const TITLE_LOG As String = "WT990104"

        ''' <summary>
        ''' メニュー:貸出
        ''' </summary>
        Public Const TITLE_LEASE As String = "WT990105"

        ''' <summary>
        ''' メニュー:ゲート
        ''' </summary>
        Public Const TITLE_GATE As String = "WT990106"

        ''' <summary>
        ''' 出力条件  20230428 ADD Sato v2.4
        ''' </summary>
        Public Const TITLE_OUTPUT_CONDITIONS As String = "WT990107"

        ''' <summary>
        ''' メニュー:工事
        ''' </summary>
        Public Const TITLE_CONSTRUCTION As String = "WT990108"

        '-----ボタン(B)-----

        ''' <summary>
        ''' ログイン
        ''' </summary>
        Public Const BUTTON_LOGIN As String = "WB990001"

        ''' <summary>
        ''' 終了
        ''' </summary>
        Public Const BUTTON_EXIT As String = "WB990002"

        ''' <summary>
        ''' ログアウト
        ''' </summary>
        Public Const BUTTON_LOGOUT As String = "WB990003"

        ''' <summary>
        ''' 検索
        ''' </summary>
        Public Const BUTTON_SEARCH As String = "WB990004"

        ''' <summary>
        ''' 新規登録
        ''' </summary>
        Public Const BUTTON_CREATE As String = "WB990005"

        ''' <summary>
        ''' 変更
        ''' </summary>
        Public Const BUTTON_CHANGE As String = "WB990006"

        ''' <summary>
        ''' 削除
        ''' </summary>
        Public Const BUTTON_DELETE As String = "WB990007"

        ''' <summary>
        ''' 戻る
        ''' </summary>
        Public Const BUTTON_RETURN As String = "WB990008"

        ''' <summary>
        ''' クリア
        ''' </summary>
        Public Const BUTTON_CLEAR As String = "WB990009"

        ''' <summary>
        ''' OK
        ''' </summary>
        Public Const BUTTON_OK As String = "WB990010"

        ''' <summary>
        ''' キャンセル
        ''' </summary>
        Public Const BUTTON_CANCEL As String = "WB990011"

        ''' <summary>
        ''' CSVデータ取込
        ''' </summary>
        Public Const BUTTON_CSVIMPORT As String = "WB990012"

        ''' <summary>
        ''' 全データCSV出力
        ''' </summary>
        Public Const BUTTON_CSVEXPORT_ALL As String = "WB990013"

        ''' <summary>
        ''' CSV出力
        ''' </summary>
        Public Const BUTTON_CSVEXPORT As String = "WB990014"

        ''' <summary>
        ''' コピー新規登録
        ''' </summary>
        Public Const BUTTON_COPY_CREATE As String = "WB990015"

        ''' <summary>
        ''' 参照
        ''' </summary>
        Public Const BUTTON_SANSHO As String = "WB990016"

        ''' <summary>
        ''' 取込開始
        ''' </summary>
        Public Const BUTTON_INPORT As String = "WB990017"

        ''' <summary>
        ''' 全選択
        ''' </summary>
        Public Const BUTTON_ALL_ON As String = "WB990018"

        ''' <summary>
        ''' 全選択解除
        ''' </summary>
        Public Const BUTTON_ALL_OFF As String = "WB990019"

        ''' <summary>
        ''' 資産詳細
        ''' </summary>
        Public Const BUTTON_ASSET_DETAIL As String = "WB990020"

        ''' <summary>
        ''' 履歴照会
        ''' </summary>
        Public Const BUTTON_HISTORY As String = "WB990021"

        ''' <summary>
        ''' 除却
        ''' </summary>
        Public Const BUTTON_RETIRE As String = "WB990022"

        ''' <summary>
        ''' 画像選択
        ''' </summary>
        Public Const BUTTON_PIC_SELECT As String = "WB990023"

        ''' <summary>
        ''' ロケーションラベル発行
        ''' </summary>
        Public Const BUTTON_LOC_LABEL As String = "WB990025"

        ''' <summary>
        ''' タグ発行
        ''' </summary>
        Public Const BUTTON_PRINT_TAG As String = "WB990026"

        ''' <summary>
        ''' QR台帳印刷
        ''' </summary>
        Public Const BUTTON_PRINT_QR_DAICHO As String = "WB990027"

        ''' <summary>
        ''' QR画面表示
        ''' </summary>
        Public Const BUTTON_SHOW_QR As String = "WB990028"

        ''' <summary>
        ''' 除却解除
        ''' </summary>
        Public Const BUTTON_RETIRE_OFF As String = "WB990029"

        ''' <summary>
        ''' 画像削除
        ''' </summary>
        Public Const BUTTON_PIC_CLEAR As String = "WB990030"

        ''' <summary>
        ''' 詳細
        ''' </summary>
        Public Const BUTTON_DETAIL As String = "WB990031"

        ''' <summary>
        ''' 更新
        ''' </summary>
        Public Const BUTTON_UPDATE As String = "WB990032"

        ''' <summary>
        ''' 予約申請
        ''' </summary>
        Public Const BUTTON_RESERVE As String = "WB990033"

        ''' <summary>
        ''' メンテナンス登録
        ''' </summary>
        Public Const BUTTON_MAINTE_ENTRY As String = "WB990034"

        ''' <summary>
        ''' カゴ追加
        ''' </summary>
        Public Const BUTTON_CART_ADD As String = "WB990035"

        ''' <summary>
        ''' カゴクリア
        ''' </summary>
        Public Const BUTTON_CART_CLEAR As String = "WB990036"

        ''' <summary>
        ''' 閉じる
        ''' </summary>
        Public Const BUTTON_CLOSE As String = "WB990037"

        ''' <summary>
        ''' 先月
        ''' </summary>
        Public Const BUTTON_LAST_MONTH As String = "WB990038"

        ''' <summary>
        ''' 当月
        ''' </summary>
        Public Const BUTTON_THIS_MONTH As String = "WB990039"

        ''' <summary>
        ''' 次月
        ''' </summary>
        Public Const BUTTON_NEXT_MONTH As String = "WB990040"

        ''' <summary>
        ''' 全解除
        ''' </summary>
        Public Const BUTTON_CANCEL_ALL As String = "WB990041"

        ''' <summary>
        ''' 一覧削除
        ''' </summary>
        Public Const BUTTON_DELETE_ICHIRAN As String = "WB990042"

        ''' <summary>
        ''' 資産追加
        ''' </summary>
        Public Const BUTTON_ADD_ASSET As String = "WB990043"

        ''' <summary>
        ''' 登録
        ''' </summary>
        Public Const BUTTON_TOUROKU As String = "WB990044"

        ''' <summary>
        ''' 照会
        ''' </summary>
        Public Const BUTTON_DISP As String = "WB990045"

        ''' <summary>
        ''' 送り状作成
        ''' </summary>
        Public Const BUTTON_OUTPUT_INVOICE As String = "WB990046"

        ''' <summary>
        ''' 貸出確認書作成
        ''' </summary>
        Public Const BUTTON_OUTPUT_LENDING_CONFIRM As String = "WB990047"

        ''' <summary>
        ''' 貸出ラベル発行
        ''' </summary>
        Public Const BUTTON_OUTPUT_LENDING_LABEL As String = "WB990048"

        ''' <summary>
        ''' 履歴
        ''' </summary>
        Public Const BUTTON_RIREKI As String = "WB990049"

        ''' <summary>
        ''' 個別期間延長
        ''' </summary>
        Public Const BUTTON_KOBETSU_KIKAN_ENCHOU As String = "WB990050"

        ''' <summary>
        ''' 直接引継
        ''' </summary>
        Public Const BUTTON_CHOKUSETSU_HIKITSUGI As String = "WB990051"

        ''' <summary>
        ''' 貸出
        ''' </summary>
        Public Const BUTTON_KASHIDASHI As String = "WB990052"

        ''' <summary>
        ''' 返却
        ''' </summary>
        Public Const BUTTON_HENKYAKU As String = "WB990053"

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' 添付ファイル[0]
        ''' </summary>
        Public Const BUTTON_ATTACHMENT As String = "WB990054"

        ''' <summary>
        ''' 追加
        ''' </summary>
        Public Const BUTTON_ADD As String = "WB990055"
        '2023/03/22 Kawamoto ADD END

        ''' <summary>
        ''' 帳票出力  20230430 ADD Sato v2.4
        ''' </summary>
        Public Const BUTTON_REPORT_OUTPUT As String = "WB990056"

        ''' <summary>
        ''' 管理簿確認  20230430 ADD Sato v2.4
        ''' </summary>
        Public Const BUTTON_MANAGEBOOK_CHECK As String = "WB990057"

        ''' <summary>
        ''' ロケQR読取
        ''' </summary>
        Public Const BUTTON_LOCATION_READ As String = "WB990058"

        ''' <summary>
        ''' 紐づけ資産
        ''' </summary>
        Public Const BUTTON_ASSET_LINK As String = "WB990059"

        ''' <summary>
        ''' 反映
        ''' </summary>
        Public Const BUTTON_REFLECT As String = "WB770005"

        ''' <summary>
        ''' 〇〇登録
        ''' </summary>
        Public Const BUTTON_BLA_REGIST As String = "WB990060"

        ''' <summary>
        ''' 予備登録１
        ''' </summary>
        Public Const BUTTON_PRE_REGIST1 As String = "WB990061"

        ''' <summary>
        ''' 予備登録２
        ''' </summary>
        Public Const BUTTON_PRE_REGIST2 As String = "WB990062"

        ''' <summary>
        ''' ラベル不要
        ''' </summary>
        Public Const BUTTON_LABEL_NOT_REQUIRED As String = "WB990063"

        ''' <summary>
        ''' ラベル発行
        ''' </summary>
        Public Const BUTTON_LABEL_PRINT As String = "WB990064"

        ''' <summary>
        ''' 資産台帳取込
        ''' </summary>
        Public Const BUTTON_ASSET_LEDGER_IMPORT As String = "WB990065"

        ''' <summary>
        ''' 棚卸実績出力
        ''' </summary>
        Public Const BUTTON_INVENTRY_RESULT_OUTPUT As String = "WB990066"

        ''' <summary>
        ''' 選択ボタン(複数選択テキストボックス)
        ''' </summary>
        Public Const BUTTON_MULTI_INPUT_SELECT As String = "WB990068"

        ''' <summary>
        ''' 複数ボタン(複数選択テキストボックス)
        ''' </summary>
        Public Const BUTTON_MULTI_INPUT As String = "WB990069"

        '-----項目(I)-----

        ''' <summary>
        ''' 操作ログ照会:ログ種別
        ''' </summary>
        Public Const LABEL_LOG_CAT As String = "WI930001"

        ''' <summary>
        ''' 全
        ''' </summary>
        Public Const KEY_PAGER_LABEL_ALL As String = "WI990001"

        ''' <summary>
        ''' 件
        ''' </summary>
        Public Const KEY_PAGER_LABEL_NUMBER As String = "WI990002"

        ''' <summary>
        ''' 確認
        ''' </summary>
        Public Const LABEL_CONFIRM As String = "WI990003"

        ''' <summary>
        ''' エラー
        ''' </summary>
        Public Const LABEL_ERROR As String = "WI990004"

        ''' <summary>
        ''' 件目から
        ''' </summary>
        Public Const KEY_PAGER_LABEL_NUMBER_FROM As String = "WI990005"

        ''' <summary>
        ''' 件目までを表示
        ''' </summary>
        Public Const KEY_PAGER_LABEL_NUMBER_TO As String = "WI990006"

        ''' <summary>
        ''' 前へ
        ''' </summary>
        Public Const KEY_PAGER_LABEL_NUMBER_BEFORE As String = "WI990007"

        ''' <summary>
        ''' 後ろへ
        ''' </summary>
        Public Const KEY_PAGER_LABEL_NUMBER_AFTER As String = "WI990008"

        ''' <summary>
        ''' 全て
        ''' </summary>
        Public Const LABEL_SELECT_ALL As String = "WI990009"

        ''' <summary>
        ''' 選択
        ''' </summary>
        Public Const LABEL_SELECT As String = "WI990010"

        ''' <summary>
        ''' 期間
        ''' </summary>
        Public Const LABEL_PERIOD As String = "WI990011"

        ''' <summary>
        ''' 表示指定
        ''' </summary>
        Public Const LABEL_DISP_SELECT As String = "WI990012"

        ''' <summary>
        ''' 集約表示
        ''' </summary>
        Public Const LABEL_SUM_DISP As String = "WI990013"

        ''' <summary>
        ''' 表示順
        ''' </summary>
        Public Const LABEL_DISP_ORDER As String = "WI990014"

        ''' <summary>
        ''' 数量
        ''' </summary>
        Public Const LABEL_QUANTITY As String = "WI990015"

        ''' <summary>
        ''' 登録日時
        ''' </summary>
        Public Const LABEL_INS_DATETIME As String = "WI990016"

        ''' <summary>
        ''' 登録者
        ''' </summary>
        Public Const LABEL_INS_EMPLOYEE_NAME As String = "WI990017"

        ''' <summary>
        ''' 最終登録更新日時
        ''' </summary>
        Public Const LABEL_UPD_DATETIME As String = "WI990018"

        ''' <summary>
        ''' 最終登録更新者
        ''' </summary>
        Public Const LABEL_UPD_EMPLOYEE_NAME As String = "WI990019"

        ''' <summary>
        ''' ファイル
        ''' </summary>
        Public Const LABEL_FILE As String = "WI990020"

        ''' <summary>
        ''' データ行
        ''' </summary>
        Public Const LABEL_ROW As String = "WI990021"

        ''' <summary>
        ''' データ項目
        ''' </summary>
        Public Const LABEL_DATA As String = "WI990022"

        ''' <summary>
        ''' エラー概要
        ''' </summary>
        Public Const LABEL_ERRORGAIYOU As String = "WI990023"

        ''' <summary>
        ''' 社員コード
        ''' </summary>
        Public Const LABEL_EMPLOYEEID As String = "WI990101"

        ''' <summary>
        ''' 氏名
        ''' </summary>
        Public Const LABEL_EMPLOYEENM As String = "WI990102"

        ''' <summary>
        ''' パスワード
        ''' </summary>
        Public Const LABEL_PASSWORD As String = "WI990103"

        ''' <summary>
        ''' パスワード変更
        ''' </summary>
        Public Const LABEL_EDITPASSWORD As String = "WI990104"

        ''' <summary>
        ''' 新パスワード
        ''' </summary>
        Public Const LABEL_NEWPASSWORD As String = "WI990105"

        ''' <summary>
        ''' 確認パスワード
        ''' </summary>
        Public Const LABEL_KAKUNINPASSWORD As String = "WI990106"

        ''' <summary>
        ''' 利用者：
        ''' </summary>
        Public Const LABEL_USER As String = "WI990107"

        ''' <summary>
        ''' 権限
        ''' </summary>
        Public Const LABEL_KENGEN As String = "WI990109"

        ''' <summary>
        ''' 部署
        ''' </summary>
        Public Const LABEL_BUSHO As String = "WI990110"

        ''' <summary>
        ''' 部署コード
        ''' </summary>
        Public Const LABEL_BUSHOCODE As String = "WI990111"

        ''' <summary>
        ''' 部署名
        ''' </summary>
        Public Const LABEL_BUSHONAME As String = "WI990112"

        ''' <summary>
        ''' ロケーション
        ''' </summary>
        Public Const LABEL_GEN_LOCATION As String = "WI990113"

        ''' <summary>
        ''' ロケーション区分
        ''' </summary>
        Public Const LABEL_LOC_DIV As String = "WI990114"

        ''' <summary>
        ''' ロケーションコード
        ''' </summary>
        Public Const LABEL_LOC_CD As String = "WI990115"

        ''' <summary>
        ''' ロケーション名
        ''' </summary>
        Public Const LABEL_LOC_NAME As String = "WI990116"

        ''' <summary>
        ''' 大区分コード
        ''' </summary>
        Public Const LABEL_LOC_L_CD As String = "WI990117"

        ''' <summary>
        ''' 中区分コード
        ''' </summary>
        Public Const LABEL_LOC_M_CD As String = "WI990118"

        ''' <summary>
        ''' 小区分コード
        ''' </summary>
        Public Const LABEL_LOC_S_CD As String = "WI990119"

        ''' <summary>
        ''' 大中区分
        ''' </summary>
        Public Const LABEL_LOC_LM As String = "WI990120"

        ''' <summary>
        ''' 大中小区分
        ''' </summary>
        Public Const LABEL_LOC_LMS As String = "WI990121"

        ''' <summary>
        ''' ロケーション区分/区分名
        ''' </summary>
        Public Const LABEL_LOC_DIV_NAME As String = "WI990122"

        ''' <summary>
        ''' ロケーション大区分コード/名
        ''' </summary>
        Public Const LABEL_LOC_L_CD_NAME As String = "WI990123"

        ''' <summary>
        ''' ロケーション中区分コード/名
        ''' </summary>
        Public Const LABEL_LOC_M_CD_NAME As String = "WI990124"

        ''' <summary>
        ''' ロケーション小区分コード/名
        ''' </summary>
        Public Const LABEL_LOC_S_CD_NAME As String = "WI990125"

        ''' <summary>
        ''' 帳簿ﾛｹｰｼｮﾝ大区分ｺｰﾄﾞ/名
        ''' </summary>
        Public Const LABEL_BOOK_LOCATION_L_CODE_NAME As String = "WI990126"

        ''' <summary>
        ''' 帳簿ﾛｹｰｼｮﾝ中区分ｺｰﾄﾞ/名
        ''' </summary>
        Public Const LABEL_BOOK_LOCATION_M_CODE_NAME As String = "WI990127"

        ''' <summary>
        ''' 帳簿ﾛｹｰｼｮﾝ小区分ｺｰﾄﾞ/名
        ''' </summary>
        Public Const LABEL_BOOK_LOCATION_S_CODE_NAME As String = "WI990128"

        ''' <summary>
        ''' 実棚照会:実棚ﾛｹｰｼｮﾝ大区分ｺｰﾄﾞ/名
        ''' </summary>
        Public Const LABEL_PHYINV_LOCATION_L_CODE_NAME As String = "WI990129"

        ''' <summary>
        ''' 実棚照会:実棚ﾛｹｰｼｮﾝ中区分ｺｰﾄﾞ/名
        ''' </summary>
        Public Const LABEL_PHYINV_LOCATION_M_CODE_NAME As String = "WI990130"

        ''' <summary>
        ''' 実棚照会:実棚ﾛｹｰｼｮﾝ小区分ｺｰﾄﾞ/名
        ''' </summary>
        Public Const LABEL_PHYINV_LOCATION_S_CODE_NAME As String = "WI990131"

        ''' <summary>
        ''' カテゴリー
        ''' </summary>
        Public Const LABEL_CATEGORY As String = "WI990132"

        ''' <summary>
        ''' 分類区分
        ''' </summary>
        Public Const LABEL_CAT_DIV As String = "WI990133"

        ''' <summary>
        ''' 分類コード
        ''' </summary>
        Public Const LABEL_CAT_CD As String = "WI990134"

        ''' <summary>
        ''' 分類名
        ''' </summary>
        Public Const LABEL_CAT_NAME As String = "WI990135"

        ''' <summary>
        ''' 大分類コード
        ''' </summary>
        Public Const LABEL_CAT_L_CD As String = "WI990136"

        ''' <summary>
        ''' 中分類コード
        ''' </summary>
        Public Const LABEL_CAT_M_CD As String = "WI990137"

        ''' <summary>
        ''' 小分類コード
        ''' </summary>
        Public Const LABEL_CAT_S_CD As String = "WI990138"

        ''' <summary>
        ''' 大中分類
        ''' </summary>
        Public Const LABEL_CATEGORY_LM As String = "WI990139"

        ''' <summary>
        ''' 大中小分類
        ''' </summary>
        Public Const LABEL_CATEGORY_LMS As String = "WI990140"

        ''' <summary>
        ''' カテゴリー大分類コード/名
        ''' </summary>
        Public Const LABEL_CAT_L_CD_NAME As String = "WI990142"

        ''' <summary>
        ''' カテゴリー中分類コード/名
        ''' </summary>
        Public Const LABEL_CAT_M_CD_NAME As String = "WI990143"

        ''' <summary>
        ''' カテゴリー小分類コード/名
        ''' </summary>
        Public Const LABEL_CAT_S_CD_NAME As String = "WI990144"

        ''' <summary>
        ''' リース期限
        ''' </summary>
        Public Const LABEL_LEASE_LIMIT As String = "WI990145"

        ''' <summary>
        ''' ○○期限
        ''' </summary>
        Public Const LABEL_GENERIC_LIMIT As String = "WI990146"

        ''' <summary>
        ''' 期限切れ
        ''' </summary>
        Public Const LABEL_LEASE_LIMIT_EXPIRED As String = "WI990147"

        ''' <summary>
        ''' アラート
        ''' </summary>
        Public Const LABEL_ALERT As String = "WI990148"

        ''' <summary>
        ''' 日前
        ''' </summary>
        Public Const LABEL_BEF_DAYS As String = "WI990149"

        ''' <summary>
        ''' リース期限アラート表示
        ''' </summary>
        Public Const LABEL_LEASE_LIMIT_ALERT As String = "WI990150"

        ''' <summary>
        ''' ○○期限アラート表示
        ''' </summary>
        Public Const LABEL_GENERIC_LIMIT_ALERT As String = "WI990151"

        ''' <summary>
        ''' 貸出先コード
        ''' </summary>
        Public Const LABEL_LEASE_PLACE_CD As String = "WI990152"

        ''' <summary>
        ''' 貸出先名
        ''' </summary>
        Public Const LABEL_LEASE_PLACE_NAME As String = "WI990153"

        ''' <summary>
        ''' 資産ID
        ''' </summary>
        Public Const LABEL_ASSET_ID As String = "WI990154"

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        Public Const LABEL_ASSET_NO As String = "WI990155"

        ''' <summary>
        ''' 資産名称
        ''' </summary>
        Public Const LABEL_ASSET_NAME As String = "WI990156"

        ''' <summary>
        ''' メーカ→資産製造元に変更
        ''' </summary>
        Public Const LABEL_MAKER As String = "WI990157"

        ''' <summary>
        ''' 品番
        ''' </summary>
        Public Const LABEL_PRODUCT_NO As String = "WI990158"

        ''' <summary>
        ''' 型番
        ''' </summary>
        Public Const LABEL_MODEL_NO As String = "WI990159"

        ''' <summary>
        ''' リース管理番号
        ''' </summary>
        Public Const LABEL_LEASE_NO As String = "WI990160"

        ''' <summary>
        ''' フリー項目1
        ''' </summary>
        Public Const LABEL_FREE_ITEM1 As String = "WI990161"

        ''' <summary>
        ''' フリー項目2
        ''' </summary>
        Public Const LABEL_FREE_ITEM2 As String = "WI990162"

        ''' <summary>
        ''' フリー項目3
        ''' </summary>
        Public Const LABEL_FREE_ITEM3 As String = "WI990163"

        ''' <summary>
        ''' フリー項目4
        ''' </summary>
        Public Const LABEL_FREE_ITEM4 As String = "WI990164"

        ''' <summary>
        ''' フリー項目5
        ''' </summary>
        Public Const LABEL_FREE_ITEM5 As String = "WI990165"

        ''' <summary>
        ''' フリー項目6
        ''' </summary>
        Public Const LABEL_FREE_ITEM6 As String = "WI990166"

        ''' <summary>
        ''' フリー項目7
        ''' </summary>
        Public Const LABEL_FREE_ITEM7 As String = "WI990167"

        ''' <summary>
        ''' フリー項目8
        ''' </summary>
        Public Const LABEL_FREE_ITEM8 As String = "WI990168"

        ''' <summary>
        ''' フリー項目9
        ''' </summary>
        Public Const LABEL_FREE_ITEM9 As String = "WI990169"

        ''' <summary>
        ''' フリー項目10
        ''' </summary>
        Public Const LABEL_FREE_ITEM10 As String = "WI990170"

        '20230203 ADD Start Shirakawa
        ''' <summary>
        ''' フリー項目11
        ''' </summary>
        Public Const LABEL_FREE_ITEM11 As String = "WI990434"

        ''' <summary>
        ''' フリー項目12
        ''' </summary>
        Public Const LABEL_FREE_ITEM12 As String = "WI990435"

        ''' <summary>
        ''' フリー項目13
        ''' </summary>
        Public Const LABEL_FREE_ITEM13 As String = "WI990436"

        ''' <summary>
        ''' フリー項目14
        ''' </summary>
        Public Const LABEL_FREE_ITEM14 As String = "WI990437"

        ''' <summary>
        ''' フリー項目15
        ''' </summary>
        Public Const LABEL_FREE_ITEM15 As String = "WI990438"
        ''' <summary>
        ''' フリー項目16
        ''' </summary>
        Public Const LABEL_FREE_ITEM16 As String = "WI990439"
        ''' <summary>
        ''' フリー項目17
        ''' </summary>
        Public Const LABEL_FREE_ITEM17 As String = "WI990440"
        ''' <summary>
        ''' フリー項目18
        ''' </summary>
        Public Const LABEL_FREE_ITEM18 As String = "WI990441"
        ''' <summary>
        ''' フリー項目19
        ''' </summary>
        Public Const LABEL_FREE_ITEM19 As String = "WI990442"

        ''' <summary>
        ''' kg/L注意書き
        ''' </summary>
        Public Const LABEL_KG_L_NOTICE As String = "WM990198"


        '20230203 ADD End Shirakawa

        ''' <summary>
        ''' 資産画像
        ''' </summary>
        Public Const LABEL_ASSET_PIC_FILENAME As String = "WI990171"

        ''' <summary>
        ''' 資産状態
        ''' </summary>
        Public Const LABEL_ASSET_STATUS As String = "WI990172"

        ''' <summary>
        ''' 資産ID状態
        ''' </summary>
        Public Const LABEL_ASSET_ID_STATUS As String = "WI990173"

        ''' <summary>
        ''' タグ紐付け状態
        ''' </summary>
        Public Const LABEL_TAG_LINK_STATUS As String = "WI990174"

        ''' <summary>
        ''' ロケ・資産番号順
        ''' </summary>
        Public Const LABEL_SORT_LOC_ASSET_NO As String = "WI990175"

        ''' <summary>
        ''' カテゴリ・資産番号順
        ''' </summary>
        Public Const LABEL_SORT_CAT_ASSET_NO As String = "WI990176"

        ''' <summary>
        ''' 資産番号順
        ''' </summary>
        Public Const LABEL_SORT_ASSET_NO As String = "WI990177"

        ''' <summary>
        ''' 貸出回数
        ''' </summary>
        Public Const LABEL_LEASE_COUNT As String = "WI990178"

        ''' <summary>
        ''' 除却日
        ''' </summary>
        Public Const LABEL_RETIRE_DT As String = "WI990179"

        ''' <summary>
        ''' 利用ツール
        ''' </summary>
        Public Const LABEL_TOOL_DIV As String = "WI990180"

        ''' <summary>
        ''' 処理ツール
        ''' </summary>
        Public Const LABEL_PROCESS_TOOL As String = "WI990181"

        ''' <summary>
        ''' 返却予定日
        ''' </summary>
        Public Const LABEL_LEASE_DUE_DT As String = "WI990182"

        ''' <summary>
        ''' 貸出期間
        ''' </summary>
        Public Const LABEL_LEASE_KIKAN As String = "WI990183"

        ''' <summary>
        ''' 貸出先コード/名
        ''' </summary>
        Public Const LABEL_LEASE_PLACE_CD_NAME As String = "WI990184"

        ''' <summary>
        ''' 貸出先担当社員コード/名
        ''' </summary>
        Public Const LABEL_LEASE_EMPLOYEE_CD_NAME As String = "WI990185"

        ''' <summary>
        ''' ログイン担当社員コード/名
        ''' </summary>
        Public Const LABEL_EMPLOYEE_CD_NAME As String = "WI990186"

        ''' <summary>
        ''' 処理区分
        ''' </summary>
        Public Const LABEL_PROCESS_DIV As String = "WI990187"

        ''' <summary>
        ''' 資産区分
        ''' </summary>
        Public Const LABEL_ASSET_DIV As String = "WI990188"

        ''' <summary>
        ''' 処理日時
        ''' </summary>
        Public Const LABEL_TASK_TS As String = "WI990189"

        ''' <summary>
        ''' キー項目
        ''' </summary>
        Public Const LABEL_KEY_ITEM As String = "WI990190"

        ''' <summary>
        ''' 大区分名称
        ''' </summary>
        Public Const LABEL_LOC_L_NAME As String = "WI990191"

        ''' <summary>
        ''' 中区分名称
        ''' </summary>
        Public Const LABEL_LOC_M_NAME As String = "WI990192"

        ''' <summary>
        ''' 小区分名称
        ''' </summary>
        Public Const LABEL_LOC_S_NAME As String = "WI990193"

        ''' <summary>
        ''' 大分類名称
        ''' </summary>
        Public Const LABEL_CAT_L_NAME As String = "WI990194"

        ''' <summary>
        ''' 中分類名称
        ''' </summary>
        Public Const LABEL_CAT_M_NAME As String = "WI990195"

        ''' <summary>
        ''' 小分類名称
        ''' </summary>
        Public Const LABEL_CAT_S_NAME As String = "WI990196"

        ''' <summary>
        ''' プリンター選択
        ''' </summary>
        Public Const LABEL_SELECT_PRINTER As String = "WI990197"

        ''' <summary>
        ''' 資産購入日
        ''' </summary>
        Public Const LABEL_ASSET_KOUNYU_DATE As String = "WI990198"

        ''' <summary>
        ''' ▼詳細検索
        ''' </summary>
        Public Const LABEL_ASSET_DETAIL_OPEN As String = "WI990199"

        ''' <summary>
        ''' ▲詳細検索
        ''' </summary>
        Public Const LABEL_ASSET_DETAIL_CLOSE As String = "WI990200"

        ''' <summary>
        ''' ログインオプション▲
        ''' </summary>
        Public Const LABEL_LOGINOPTION_CLOSE As String = "WI990201"

        ''' <summary>
        ''' ログインオプションs▼
        ''' </summary>
        Public Const LABEL_LOGINOPTION_OPEN As String = "WI990202"

        ''' <summary>
        ''' 資産内容
        ''' </summary>
        Public Const LABEL_ASSET_CONTENT As String = "WI990204"

        ''' <summary>
        ''' 登録
        ''' </summary>
        Public Const LABEL_ASSET_INSERT As String = "WI990205"

        ''' <summary>
        ''' 変更
        ''' </summary>
        Public Const LABEL_ASSET_UPDATE As String = "WI990206"

        ''' <summary>
        ''' 直近棚卸日
        ''' </summary>
        Public Const LABEL_NEAR_INVENTORY_DATE As String = "WI990207"

        ''' <summary>
        ''' 棚卸未実施あり
        ''' </summary>
        Public Const LABEL_INVENTORY_MI As String = "WI990208"

        ''' <summary>
        ''' 状態
        ''' </summary>
        Public Const LABEL_STATUS As String = "WI990209"

        ''' <summary>
        ''' 件数
        ''' </summary>
        Public Const LABEL_KENSUU As String = "WI990210"

        ''' <summary>
        ''' エラー件数
        ''' </summary>
        Public Const LABEL_ERR_KENSUU As String = "WI990211"

        ''' <summary>
        ''' 貸出先備考
        ''' </summary>
        Public Const LABEL_LEASE_REMARK As String = "WI990212"

        ''' <summary>
        ''' 中分類のみ
        ''' </summary>
        Public Const LABEL_CATEGORY_M_ONLY As String = "WI990213"

        ''' <summary>
        ''' 小分類のみ
        ''' </summary>
        Public Const LABEL_CATEGORY_S_ONLY As String = "WI990214"

        ''' <summary>
        ''' 中区分のみ
        ''' </summary>
        Public Const LABEL_LOC_M_ONLY As String = "WI990215"

        ''' <summary>
        ''' 小区分のみ
        ''' </summary>
        Public Const LABEL_LOC_S_ONLY As String = "WI990216"

        ''' <summary>
        ''' 返却期限
        ''' </summary>
        Public Const LABEL_HENKYAKU_LIMIT As String = "WI990217"

        ''' <summary>
        ''' 最初
        ''' </summary>
        Public Const LABEL_FIRST As String = "WI990218"

        ''' <summary>
        ''' 最初
        ''' </summary>
        Public Const LABEL_LAST As String = "WI990219"

        ''' <summary>
        ''' NO
        ''' </summary>
        Public Const LABEL_NO As String = "WI990220"

        ''' <summary>
        ''' 資産CSV項目
        ''' </summary>
        Public Const LABEL_CSV_KOUMOKU_ASSET As String = "WI990221"

        ''' <summary>
        ''' 必須
        ''' </summary>
        Public Const LABEL_REQUIRED As String = "WI990222"

        ''' <summary>
        ''' 指定方法
        ''' </summary>
        Public Const LABEL_SPECIFIED As String = "WI990223"

        ''' <summary>
        ''' 列番号
        ''' </summary>
        Public Const LABEL_COL_NO As String = "WI990224"

        ''' <summary>
        ''' 固定値
        ''' </summary>
        Public Const LABEL_FIX_VAL As String = "WI990225"

        ''' <summary>
        ''' 未指定
        ''' </summary>
        Public Const LABEL_NO_SELECTED As String = "WI990226"

        ''' <summary>
        ''' 列番指定
        ''' </summary>
        Public Const LABEL_COL_SELECTED As String = "WI990227"

        ''' <summary>
        ''' 文字コード
        ''' </summary>
        Public Const LABEL_CHAR_CD As String = "WI990228"

        ''' <summary>
        ''' ロケ大区分コード
        ''' </summary>
        Public Const LABEL_CSV_LOC_L_CD As String = "WI990229"

        ''' <summary>
        ''' ロケ大区分名
        ''' </summary>
        Public Const LABEL_CSV_LOC_L_NAME As String = "WI990230"

        ''' <summary>
        ''' ロケ中区分コード
        ''' </summary>
        Public Const LABEL_CSV_LOC_M_CD As String = "WI990231"

        ''' <summary>
        ''' ロケ中区分名
        ''' </summary>
        Public Const LABEL_CSV_LOC_M_NAME As String = "WI990232"

        ''' <summary>
        ''' ロケ小区分コード
        ''' </summary>
        Public Const LABEL_CSV_LOC_S_CD As String = "WI990233"

        ''' <summary>
        ''' ロケ小区分名
        ''' </summary>
        Public Const LABEL_CSV_LOC_S_NAME As String = "WI990234"

        ''' <summary>
        ''' カテゴリ大分類コード
        ''' </summary>
        Public Const LABEL_CSV_CAT_L_CD As String = "WI990235"

        ''' <summary>
        ''' カテゴリ大分類名
        ''' </summary>
        Public Const LABEL_CSV_CAT_L_NAME As String = "WI990236"

        ''' <summary>
        ''' カテゴリ中分類コード
        ''' </summary>
        Public Const LABEL_CSV_CAT_M_CD As String = "WI990237"

        ''' <summary>
        ''' カテゴリ中分類名
        ''' </summary>
        Public Const LABEL_CSV_CAT_M_NAME As String = "WI990238"

        ''' <summary>
        ''' カテゴリ小分類コード
        ''' </summary>
        Public Const LABEL_CSV_CAT_S_CD As String = "WI990239"

        ''' <summary>
        ''' カテゴリ小分類名称
        ''' </summary>
        Public Const LABEL_CSV_CAT_S_NAME As String = "WI990240"

        ''' <summary>
        ''' 現在貸出
        ''' </summary>
        Public Const LABEL_NOW_LENDING As String = "WI990241"

        ''' <summary>
        ''' 貸出期限
        ''' </summary>
        Public Const LABEL_ASSET_LEASE_LIMIT As String = "WI990242"

        ''' <summary>
        ''' 切れ
        ''' </summary>
        Public Const LABEL_EXPIRED As String = "WI990243"

        ''' <summary>
        ''' あり
        ''' </summary>
        Public Const LABEL_ARI As String = "WI990244"

        ''' <summary>
        ''' 貸出先
        ''' </summary>
        Public Const LABEL_LEASE_PLACE As String = "WI990245"

        ''' <summary>
        ''' 貸出状態
        ''' </summary>
        Public Const LABEL_LEASE_STATUS As String = "WI990246"

        ''' <summary>
        ''' 過去貸出
        ''' </summary>
        Public Const LABEL_LEASE_PAST As String = "WI990247"

        ''' <summary>
        ''' 含む
        ''' </summary>
        Public Const LABEL_FUKUMU As String = "WI990248"

        ''' <summary>
        ''' 貸出担当者
        ''' </summary>
        Public Const LABEL_LEASE_EMPLOYEE As String = "WI990249"

        ''' <summary>
        ''' 貸出備考
        ''' </summary>
        Public Const LABEL_LEASE_BIKO As String = "WI990250"

        ''' <summary>
        ''' 貸出アラート表示
        ''' </summary>
        Public Const LABEL_LEASE_ALERT As String = "WI990251"

        ''' <summary>
        ''' 貸出アラート回数
        ''' </summary>
        Public Const LABEL_LEASE_ALERT_CNT As String = "WI990252"

        ''' <summary>
        ''' 貸出上限
        ''' </summary>
        Public Const LABEL_LEASE_UPPER_LIMIT As String = "WI990253"

        ''' <summary>
        ''' 上限アラート
        ''' </summary>
        Public Const LABEL_LEASE_UPPER_LIMIT_ALERT As String = "WI990254"

        ''' <summary>
        ''' タグEPC資産タイプ
        ''' </summary>
        Public Const LABEL_EPC_ASSET_TYPE As String = "WI990255"

        ''' <summary>
        ''' タグEPC事業所
        ''' </summary>
        Public Const LABEL_EPC_COMPANY As String = "WI990256"

        ''' <summary>
        ''' ゲート名
        ''' </summary>
        Public Const LABEL_GATE_NAME As String = "WI990257"

        ''' <summary>
        ''' IPアドレス
        ''' </summary>
        Public Const LABEL_GATE_IP As String = "WI990258"

        ''' <summary>
        ''' 有効/無効
        ''' </summary>
        Public Const LABEL_GATE_VALID As String = "WI990259"

        ''' <summary>
        ''' ゲートコード
        ''' </summary>
        Public Const LABEL_GATE_CD As String = "WI990260"

        ''' <summary>
        ''' アンテナ１
        ''' </summary>
        Public Const LABEL_GATE_ANTENNA1 As String = "WI990261"

        ''' <summary>
        ''' アンテナ２
        ''' </summary>
        Public Const LABEL_GATE_ANTENNA2 As String = "WI990262"

        ''' <summary>
        ''' アンテナ３
        ''' </summary>
        Public Const LABEL_GATE_ANTENNA3 As String = "WI990263"

        ''' <summary>
        ''' アンテナ４
        ''' </summary>
        Public Const LABEL_GATE_ANTENNA4 As String = "WI990264"

        ''' <summary>
        ''' 電波強度
        ''' </summary>
        Public Const LABEL_ANTENNA_POW As String = "WI990265"

        ''' <summary>
        ''' ゲート
        ''' </summary>
        Public Const LABEL_GATE As String = "WI990266"

        ''' <summary>
        ''' パトライト
        ''' </summary>
        Public Const LABEL_PATLITE As String = "WI990267"

        ''' <summary>
        ''' サーモセンサ１
        ''' </summary>
        Public Const LABEL_THERMO1 As String = "WI990268"

        ''' <summary>
        ''' サーモセンサ２
        ''' </summary>
        Public Const LABEL_THERMO2 As String = "WI990269"

        ''' <summary>
        ''' データ
        ''' </summary>
        Public Const LABEL_GATE_DATA As String = "WI990270"

        ''' <summary>
        ''' 通過日
        ''' </summary>
        Public Const LABEL_GATE_PASS_DT As String = "WI990271"

        ''' <summary>
        ''' 通過日時
        ''' </summary>
        Public Const LABEL_GATE_PASS_DTTM As String = "WI990272"

        ''' <summary>
        ''' 貸出
        ''' </summary>
        Public Const LABEL_LENDING As String = "WI990273"

        ''' <summary>
        ''' チャンネル
        ''' </summary>
        Public Const LABEL_GATE_CHANNEL As String = "WI990274"

        ''' <summary>
        ''' 通過ゲート
        ''' </summary>
        Public Const LABEL_PASSED_GATE As String = "WI990275"

        ''' <summary>
        ''' 選択件数：
        ''' </summary>
        Public Const LABEL_SELECT_COUNT As String = "WI990276"

        '20220328 ADD Start Kawamoto v2.1
        ''' <summary>
        ''' 感度
        ''' </summary>
        Public Const SENSITIVITY As String = "WI610002"

        ''' <summary>
        ''' 周波数
        ''' </summary>
        Public Const FREQUENCY As String = "WI610003"

        ''' <summary>
        ''' IO設定
        ''' </summary>
        Public Const IO_SETTING As String = "WI610004"

        ''' <summary>
        ''' カメラ
        ''' </summary>
        Public Const CAMERA As String = "WI610005"

        ''' <summary>
        ''' セッション
        ''' </summary>
        Public Const SESSION As String = "WI610006"

        ''' <summary>
        ''' Searchモード
        ''' </summary>
        Public Const SEARCH_MODE As String = "WI610007"

        ''' <summary>
        ''' Readerモード
        ''' </summary>
        Public Const READER_MODE As String = "WI610008"

        ''' <summary>
        ''' Tag Population
        ''' </summary>
        Public Const TAG_POPULATION As String = "WI610009"

        ''' <summary>
        ''' 機器名
        ''' </summary>
        Public Const DEVICE_NAME As String = "WI610010"

        ''' <summary>
        ''' bit位置
        ''' </summary>
        Public Const BIT_POS As String = "WI610011"

        ''' <summary>
        ''' システムエラー
        ''' </summary>
        Public Const VALUE_GATE_PASS_DIV_SE As String = "WV610001"

        ''' <summary>
        ''' 備考
        ''' </summary>
        Public Const GATE_REMARK As String = "WI610012"

        ''' <summary>
        ''' 無条件で発報(カメラ)
        ''' </summary>
        Public Const UNCONDITIONAL_C As String = "WI610013"

        ''' <summary>
        ''' 無条件で発報(パトライト)
        ''' </summary>
        Public Const UNCONDITIONAL_P As String = "WI610014"

        ''' <summary>
        ''' 発報条件説明(カメラ)
        ''' </summary>
        Public Const CONDITIONS_C As String = "WI610015"

        ''' <summary>
        ''' 発報条件説明(パトライト)
        ''' </summary>
        Public Const CONDITIONS_P As String = "WI610016"

        ''' <summary>
        ''' 発報条件：無効説明(カメラ)
        ''' </summary>
        Public Const CONDITIONS_INVALID_C As String = "WI610017"

        ''' <summary>
        ''' 発報条件：有効説明(カメラ)
        ''' </summary>
        Public Const CONDITIONS_VALID_C As String = "WI610018"

        ''' <summary>
        ''' 発報条件：無効説明(パトライト)
        ''' </summary>
        Public Const CONDITIONS_INVALID_P As String = "WI610019"

        ''' <summary>
        ''' 発報条件：有効説明(パトライト)
        ''' </summary>
        Public Const CONDITIONS_VALID_P As String = "WI610020"
        '20220328 ADD End Kawamoto v2.1

        '20220511 ADD Start Kawamoto
        ''' <summary>
        ''' DIOボード/GPO
        ''' </summary>
        Public Const DIO_BOARD_GPO As String = "WI610021"

        ''' <summary>
        ''' 機種
        ''' </summary>
        Public Const DEVICE_TYPE As String = "WI610022"

        ''' <summary>
        ''' GPIO
        ''' </summary>
        Public Const GPIO As String = "WI610023"

        ''' <summary>
        ''' GPIO機器
        ''' </summary>
        Public Const GPIO_DEVICE As String = "WI610024"

        ''' <summary>
        ''' GPO
        ''' </summary>
        Public Const GPO As String = "WI610025"

        ''' <summary>
        ''' GPOポート
        ''' </summary>
        Public Const GPO_PORT As String = "WI610026"

        ''' <summary>
        ''' R420
        ''' </summary>
        Public Const R420 As String = "WI610027"

        ''' <summary>
        ''' R700
        ''' </summary>
        Public Const R700 As String = "WI610028"

        ''' <summary>
        ''' 
        ''' SpeedWay
        ''' </summary>
        Public Const SPEED_WAY As String = "WI610029"

        ''' <summary>
        ''' CONTEC DIOボード
        ''' </summary>
        Public Const CONTEC_DIO_BOARD As String = "WI610030"

        ''' <summary>
        ''' IO No.1
        ''' </summary>
        Public Const IO_No1 As String = "WI610031"

        ''' <summary>
        ''' IO No.2
        ''' </summary>
        Public Const IO_No2 As String = "WI610032"
        '20220511 ADD End Kawamoto

        ''' <summary>
        ''' 郵便番号
        ''' </summary>
        Public Const LABEL_ZIP_CODE As String = "WI990277"

        ''' <summary>
        ''' 住所
        ''' </summary>
        Public Const LABEL_ADDRESS As String = "WI990278"

        ''' <summary>
        ''' 送付部署名
        ''' </summary>
        Public Const LABEL_REQ_DEP_NAME As String = "WI990279"

        ''' <summary>
        ''' 電話番号
        ''' </summary>
        Public Const LABEL_PHONE_NUMBER As String = "WI990280"

        ''' <summary>
        ''' FAX
        ''' </summary>
        Public Const LABEL_FAX As String = "WI990281"

        ''' <summary>
        ''' 本部/拠点
        ''' </summary>
        Public Const LABEL_HEAD_OFFICE_FLG As String = "WI990282"

        ''' <summary>
        ''' 本部にする
        ''' </summary>
        Public Const LABEL_TO_HEAD_OFFICE As String = "WI990283"

        ''' <summary>
        ''' 共通/拠点
        ''' </summary>
        Public Const LABEL_LOC_SHARE_FLG As String = "WI990284"

        ''' <summary>
        ''' メンテ発生貸出回数
        ''' </summary>
        Public Const LABEL_MAINTE_LEASE_NUM As String = "WI990285"

        ''' <summary>
        ''' メンテ発生貸出日数
        ''' </summary>
        Public Const LABEL_MAINTE_LEASE_DAY As String = "WI990286"

        ''' <summary>
        ''' メンテ日数
        ''' </summary>
        Public Const LABEL_MAINTE_DAY As String = "WI990287"

        ''' <summary>
        ''' メンテナンス日数
        ''' </summary>
        Public Const LABEL_MAINTENANCE_DAY As String = "WI990288"

        ''' <summary>
        ''' メンテナンス発生
        ''' </summary>
        Public Const LABEL_MAINTENANCE As String = "WI990289"

        ''' <summary>
        ''' 貸出日数
        ''' </summary>
        Public Const LABEL_LEASE_DAY As String = "WI990290"

        ''' <summary>
        ''' 共通ロケーション
        ''' </summary>
        Public Const LABEL_LOC_SHARE As String = "WI990291"

        ''' <summary>
        ''' 稼働日考慮
        ''' </summary>
        Public Const LABEL_WORK_FLAG As String = "WI990292"

        ''' <summary>
        ''' 稼働日
        ''' </summary>
        Public Const LABEL_WORK_DAYS As String = "WI990293"

        ''' <summary>
        ''' 日
        ''' </summary>
        Public Const LABEL_DAY As String = "WI990294"

        ''' <summary>
        ''' メール送信先
        ''' </summary>
        Public Const LABEL_MAIL_SEND_ADDRESS As String = "WI990295"

        ''' <summary>
        ''' 年月日
        ''' </summary>
        Public Const LABEL_YEAR_MONTH_DAY As String = "WI990296"

        ''' <summary>
        ''' 曜日
        ''' </summary>
        Public Const LABEL_YOBI As String = "WI990297"

        ''' <summary>
        ''' 稼働日/非稼働日
        ''' </summary>
        Public Const LABEL_WORK_OR_NOT_WORK As String = "WI990298"

        ''' <summary>
        ''' 稼働日
        ''' </summary>
        Public Const LABEL_NOT_WORK_FLG_N As String = "WI990299"

        ''' <summary>
        ''' 非稼働日
        ''' </summary>
        Public Const LABEL_NOT_WORK_FLG_Y As String = "WI990300"

        ''' <summary>
        ''' 退職
        ''' </summary>
        Public Const LABEL_RETIRE As String = "WI990301"

        ''' <summary>
        ''' 所属拠点
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN As String = "WI990302"

        ''' <summary>
        ''' 退職済み
        ''' </summary>
        Public Const LABEL_RETIRED As String = "WI990303"

        ''' <summary>
        ''' 種類
        ''' </summary>
        Public Const LABEL_SHURUI As String = "WI990304"

        ''' <summary>
        ''' メールアドレス
        ''' </summary>
        Public Const LABEL_MAIL_ADDRESS As String = "WI990305"

        ''' <summary>
        ''' 拠点責任者
        ''' </summary>
        Public Const LABEL_KYOTEN_SEKI As String = "WI990306"

        ''' <summary>
        ''' 拠点担当者
        ''' </summary>
        Public Const LABEL_KYOTEN_TAN As String = "WI990307"

        ''' <summary>
        ''' 本部責任者
        ''' </summary>
        Public Const LABEL_HONBU_SEKI As String = "WI990308"

        ''' <summary>
        ''' 本部担当者
        ''' </summary>
        Public Const LABEL_HONBU_TAN As String = "WI990309"

        ''' <summary>
        ''' 退職者
        ''' </summary>
        Public Const LABEL_RETIREE As String = "WI990310"

        ''' <summary>
        ''' 含める
        ''' </summary>
        Public Const LABEL_FUKUMERU As String = "WI990311"

        ''' <summary>
        ''' 管理ロケーション
        ''' </summary>
        Public Const LABEL_MNG_LOC As String = "WI990312"

        ''' <summary>
        ''' 担当者
        ''' </summary>
        Public Const LABEL_EMP As String = "WI990313"

        ''' <summary>
        ''' 申請者
        ''' </summary>
        Public Const LABEL_REQ_EMP As String = "WI990314"

        ''' <summary>
        ''' 表示開始日
        ''' </summary>
        Public Const LABEL_DSP_START_DT As String = "WI990315"

        ''' <summary>
        ''' 出荷通知
        ''' </summary>
        Public Const LABEL_SHIPPING_NOTIFICATION As String = "WI990316"

        ''' <summary>
        ''' 並び替え
        ''' </summary>
        Public Const LABEL_SORT As String = "WI990317"

        ''' <summary>
        ''' 社員CSV項目
        ''' </summary>
        Public Const LABEL_CSV_KOUMOKU_EMPLOYEE As String = "WI990318"

        ''' <summary>
        ''' 所属拠点コード1
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN1 As String = "WI990319"
        ''' <summary>
        ''' 所属拠点コード2
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN2 As String = "WI990320"
        ''' <summary>
        ''' 所属拠点コード3
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN3 As String = "WI990321"
        ''' <summary>
        ''' 所属拠点コード4
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN4 As String = "WI990322"
        ''' <summary>
        ''' 所属拠点コード5
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN5 As String = "WI990323"
        ''' <summary>
        ''' 所属拠点コード6
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN6 As String = "WI990324"
        ''' <summary>
        ''' 所属拠点コード7
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN7 As String = "WI990325"
        ''' <summary>
        ''' 所属拠点コード8
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN8 As String = "WI990326"
        ''' <summary>
        ''' 所属拠点コード9
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN9 As String = "WI990327"
        ''' <summary>
        ''' 所属拠点コード10
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN10 As String = "WI990328"
        ''' <summary>
        ''' 所属拠点コード11
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN11 As String = "WI990329"
        ''' <summary>
        ''' 所属拠点コード12
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN12 As String = "WI990330"
        ''' <summary>
        ''' 所属拠点コード13
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN13 As String = "WI990331"
        ''' <summary>
        ''' 所属拠点コード14
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN14 As String = "WI990332"
        ''' <summary>
        ''' 所属拠点コード15
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN15 As String = "WI990333"
        ''' <summary>
        ''' 所属拠点コード16
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN16 As String = "WI990334"
        ''' <summary>
        ''' 所属拠点コード17
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN17 As String = "WI990335"
        ''' <summary>
        ''' 所属拠点コード18
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN18 As String = "WI990336"
        ''' <summary>
        ''' 所属拠点コード19
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN19 As String = "WI990337"
        ''' <summary>
        ''' 所属拠点コード20
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN20 As String = "WI990338"
        ''' <summary>
        ''' 所属拠点コード21
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN21 As String = "WI990339"
        ''' <summary>
        ''' 所属拠点コード22
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN22 As String = "WI990340"
        ''' <summary>
        ''' 所属拠点コード23
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN23 As String = "WI990341"
        ''' <summary>
        ''' 所属拠点コード24
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN24 As String = "WI990342"
        ''' <summary>
        ''' 所属拠点コード25
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN25 As String = "WI990343"
        ''' <summary>
        ''' 所属拠点コード26
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN26 As String = "WI990344"
        ''' <summary>
        ''' 所属拠点コード27
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN27 As String = "WI990345"
        ''' <summary>
        ''' 所属拠点コード28
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN28 As String = "WI990346"
        ''' <summary>
        ''' 所属拠点コード29
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN29 As String = "WI990347"
        ''' <summary>
        ''' 所属拠点コード30
        ''' </summary>
        Public Const LABEL_SHOZOKU_KYOTEN30 As String = "WI990348"

        ''' <summary>
        ''' 退職フラグ
        ''' </summary>
        Public Const LABEL_RETIRE_FLG As String = "WI990349"

        ''' <summary>
        ''' 権限区分
        ''' </summary>
        Public Const LABEL_KENGEN_KBN As String = "WI990350"

        ''' <summary>
        ''' 可能
        ''' </summary>
        Public Const LABEL_KANOU As String = "WI990351"

        ''' <summary>
        ''' 不可
        ''' </summary>
        Public Const LABEL_FUKA As String = "WI990352"

        ''' <summary>
        ''' 稼働日考慮
        ''' </summary>
        Public Const LABEL_KADOUBI_KOURYO As String = "WI990353"

        ''' <summary>
        ''' 管理ロケーションコード/名
        ''' </summary>
        Public Const LABEL_MNG_LOC_L_CD_NAME As String = "WI990354"

        ''' <summary>
        ''' 選択数
        ''' </summary>
        Public Const LABEL_SELECT_NUMBER As String = "WI990355"

        ''' <summary>
        ''' 予約番号
        ''' </summary>
        Public Const LABEL_LEASE_NUMBER As String = "WI990371"          '2022/01/19 貸出番号→予約番号へ統一

        ''' <summary>
        ''' 実質貸出期間
        ''' </summary>
        Public Const LABEL_LEASE_KIKAN_JISSHITSU As String = "WI990357"

        ''' <summary>
        ''' ユーザー貸出期間
        ''' </summary>
        Public Const LABEL_LEASE_KIKAN_USER As String = "WI990358"

        ''' <summary>
        ''' 貸出用途
        ''' </summary>
        Public Const LABEL_LEASE_YOUTO As String = "WI990359"

        ''' <summary>
        ''' 送り先名
        ''' </summary>
        Public Const LABEL_SEND_NAME As String = "WI990360"

        ''' <summary>
        ''' 送り先郵便番号
        ''' </summary>
        Public Const LABEL_SEND_ZIP_CODE As String = "WI990361"

        ''' <summary>
        ''' 送り先電話番号
        ''' </summary>
        Public Const LABEL_SEND_PHONE_NUMBER As String = "WI990362"

        ''' <summary>
        ''' 送り先住所
        ''' </summary>
        Public Const LABEL_SEND_ADDRESS As String = "WI990363"

        ''' <summary>
        ''' 送り先備考
        ''' </summary>
        Public Const LABEL_SEND_BIKO As String = "WI990364"

        ''' <summary>
        ''' 社内用備考
        ''' </summary>
        Public Const LABEL_SHANAI_BIKO As String = "WI990365"

        ''' <summary>
        ''' シリアルNo
        ''' </summary>
        Public Const LABEL_SERIAL_NO As String = "WI990366"

        ''' <summary>
        ''' 社員マスタメンテナンス:権限区分名（スーパー管理者）
        ''' </summary>
        Public Const LABEL_KENGEN_SUPERADMIN As String = "WI990367"

        ''' <summary>
        ''' 管理ロケ区分コード
        ''' </summary>
        Public Const LABEL_MNG_LOC_KBN_CD As String = "WI990368"

        ''' <summary>
        ''' メンテ発生貸出日数稼働日フラグ
        ''' </summary>
        Public Const LABEL_MAINTE_LEASE_DAY_WORK_FLG As String = "WI990369"

        ''' <summary>
        ''' メンテ日数稼働日フラグ
        ''' </summary>
        Public Const LABEL_MAINTE_DAY_WORK_FLG As String = "WI990370"

        ''' <summary>
        ''' 予約番号
        ''' </summary>
        Public Const LABEL_RESERVE_NO As String = "WI990371"

        ''' <summary>
        ''' 日付抽出
        ''' </summary>
        Public Const LABEL_DATE_EXTRACT As String = "WI990372"

        ''' <summary>
        ''' 当日出荷
        ''' </summary>
        Public Const LABEL_SAME_DAY_SHIPPING As String = "WI990373"

        ''' <summary>
        ''' 翌日出荷
        ''' </summary>
        Public Const LABEL_NEXT_DAY_SHIPPING As String = "WI990374"

        ''' <summary>
        ''' 実質貸出開始日
        ''' </summary>
        Public Const LABEL_REAL_LENDING_START_DATE As String = "WI990375"

        ''' <summary>
        ''' 実質貸出終了日
        ''' </summary>
        Public Const LABEL_REAL_LENDING_END_DATE As String = "WI990376"

        ''' <summary>
        ''' 抽出期間
        ''' </summary>
        Public Const LABEL_EXTRACTION_PERIOD As String = "WI990377"

        ''' <summary>
        ''' 営業日数
        ''' </summary>
        Public Const LABEL_EIGYO_DAY As String = "WI990378"

        ''' <summary>
        ''' 貸出率
        ''' </summary>
        Public Const LABEL_LENDING_RATE As String = "WI990379"

        ''' <summary>
        ''' 処理分類
        ''' </summary>
        Public Const LABEL_PROCESS_CLASS As String = "WI990380"

        ''' <summary>
        ''' ユーザー貸出開始日
        ''' </summary>
        Public Const USER_START_DT As String = "WI990381"

        ''' <summary>
        ''' ユーザー貸出終了日
        ''' </summary>
        Public Const USER_END_DT As String = "WI990382"

        ''' <summary>
        ''' 実貸出日
        ''' </summary>
        Public Const LABEL_REAL_LENDING_DT As String = "WI990383"

        ''' <summary>
        ''' 実返却日
        ''' </summary>
        Public Const LABEL_REAL_RETURN_DT As String = "WI990384"

        ''' <summary>
        ''' 資産担当者
        ''' </summary>
        Public Const LABEL_ASSET_EMPLOYEE As String = "WI990385"

        ''' <summary>
        ''' 資産実質貸出開始日
        ''' </summary>
        Public Const LABEL_ASSET_REAL_START_DT As String = "WI990386"

        ''' <summary>
        ''' 資産実質貸出終了日
        ''' </summary>
        Public Const LABEL_ASSET_REAL_END_DT As String = "WI990387"

        ''' <summary>
        ''' 非稼働日フラグ
        ''' </summary>
        Public Const LABEL_NOT_WORK_FLG As String = "WI990388"

        '2021/10/24 nv Inoue いらなくなったのでコメントアウト　後に削除する
        '''' <summary>
        '''' 住所録コード
        '''' </summary>
        'Public Const LABEL_JUSHOROKU_CODE As String = "WI990377"

        '''' <summary>
        '''' お届け先電話番号
        '''' </summary>
        'Public Const LABEL_OTODOKE_PHONE_NUMBER As String = "WI990378"

        '''' <summary>
        '''' お届け先郵便番号
        '''' </summary>
        'Public Const LABEL_OTODOKE_ZIP_CODE As String = "WI990379"

        '''' <summary>
        '''' お届け先住所1
        '''' </summary>
        'Public Const LABEL_OTODOKE_ADDRESS1 As String = "WI990380"

        '''' <summary>
        '''' お届け先住所2
        '''' </summary>
        'Public Const LABEL_OTODOKE_ADDRESS2 As String = "WI990381"

        '''' <summary>
        '''' お届け先住所3
        '''' </summary>
        'Public Const LABEL_OTODOKE_ADDRESS3 As String = "WI990382"

        '''' <summary>
        '''' お届け先名称1
        '''' </summary>
        'Public Const LABEL_OTODOKE_MEISHO1 As String = "WI990383"

        '''' <summary>
        '''' お届け先名称2
        '''' </summary>
        'Public Const LABEL_OTODOKE_MEISHO2 As String = "WI990384"

        '''' <summary>
        '''' お客様管理ナンバー
        '''' </summary>
        'Public Const LABEL_CUSTOMER_KANRI_NO As String = "WI990385"

        '''' <summary>
        '''' お客様コード
        '''' </summary>
        'Public Const LABEL_CUSTOMER_CODE As String = "WI990386"

        '''' <summary>
        '''' 部署・担当者名
        '''' </summary>
        'Public Const LABEL_BUSHO_TANTOSHA_MEI As String = "WI990387"

        '''' <summary>
        '''' 荷送人電話番号
        '''' </summary>
        'Public Const LABEL_NIOKURI_PHONE_NUMBER As String = "WI990388"

        '''' <summary>
        '''' ご依頼主電話番号
        '''' </summary>
        'Public Const LABEL_IRAINUSHI_PHONE_NUMBER As String = "WI990389"

        '''' <summary>
        '''' ご依頼主郵便番号
        '''' </summary>
        'Public Const LABEL_IRAINUSHI_ZIP_CODE As String = "WI990390"

        '''' <summary>
        ''''ご依頼主住所1
        '''' </summary>
        'Public Const LABEL_IRAINUSHI_ADDRESS1 As String = "WI990391"

        '''' <summary>
        '''' ご依頼主住所2
        '''' </summary>
        'Public Const LABEL_IRAINUSHI_ADDRESS2 As String = "WI990392"

        '''' <summary>
        '''' 荷姿コード
        '''' </summary>
        'Public Const LABEL_NISUGATA_CODE As String = "WI990393"

        '''' <summary>
        '''' 品名1
        '''' </summary>
        'Public Const LABEL_HINMEI1 As String = "WI990394"

        '''' <summary>
        '''' 品名2
        '''' </summary>
        'Public Const LABEL_HINMEI2 As String = "WI9903895"

        '''' <summary>
        '''' 品名3
        '''' </summary>
        'Public Const LABEL_HINMEI3 As String = "WI990396"

        '''' <summary>
        '''' 品名4
        '''' </summary>
        'Public Const LABEL_HINMEI4 As String = "WI9903897"

        '''' <summary>
        '''' 品名5
        '''' </summary>
        'Public Const LABEL_HINMEI5 As String = "WI9903898"

        '''' <summary>
        '''' 出荷荷数
        '''' </summary>
        'Public Const LABEL_SHUKKA_KASU As String = "WI9903899"

        '''' <summary>
        '''' 便種（スピードで選択）
        '''' </summary>
        'Public Const LABEL_BINSHU_SPEED As String = "WI990400"

        '''' <summary>
        '''' 便種（商品）
        '''' </summary>
        'Public Const LABEL_BINSHU_SHOHIN As String = "WI990401"

        '''' <summary>
        '''' 配達日
        '''' </summary>
        'Public Const LABEL_HAITATSUBI As String = "WI990402"

        '''' <summary>
        '''' 配達指定時間帯
        '''' </summary>
        'Public Const LABEL_HAITATSU_SHITEI_JIKANTAI As String = "WI990403"

        '''' <summary>
        '''' 配達指定時間（時分）
        '''' </summary>
        'Public Const LABEL_HAITATSU_SHITEI_JIKAN As String = "WI990404"

        '''' <summary>
        '''' 代引金額
        '''' </summary>
        'Public Const LABEL_DAIBIKI_KINGAKU As String = "WI990405"

        '''' <summary>
        '''' 消費税
        '''' </summary>
        'Public Const LABEL_SHOHI_ZEI As String = "WI990406"

        '''' <summary>
        '''' 決済種別
        '''' </summary>
        'Public Const LABEL_KESSAI_SHUBETSU As String = "WI990407"

        '''' <summary>
        '''' 保険金額
        '''' </summary>
        'Public Const LABEL_HOKEN_KINGAKU As String = "WI990408"

        '''' <summary>
        '''' 保険金額印字
        '''' </summary>
        'Public Const LABEL_HOKEN_KINGAKU_INJI As String = "WI990409"

        '''' <summary>
        '''' 指定シール①
        '''' </summary>
        'Public Const LABEL_SHITEI_SHEEL1 As String = "WI990410"

        '''' <summary>
        '''' 指定シール②
        '''' </summary>
        'Public Const LABEL_SHITEI_SHEEL2 As String = "WI990411"

        '''' <summary>
        '''' 指定シール③
        '''' </summary>
        'Public Const LABEL_SHITEI_SHEEL3 As String = "WI990412"

        '''' <summary>
        '''' 営業店止め
        '''' </summary>
        'Public Const LABEL_EIGYOTEN_DOME As String = "WI990413"

        '''' <summary>
        '''' SRC区分
        '''' </summary>
        'Public Const LABEL_SRC_KUBUN As String = "WI990414"

        '''' <summary>
        '''' 営業店コード
        '''' </summary>
        'Public Const LABEL_EIGYOTEN_CODE As String = "WI990415"

        '''' <summary>
        '''' 元着区分
        '''' </summary>
        'Public Const LABEL_MOTOCHAKU_KUBUN As String = "WI990416"

        ''' <summary>
        ''' メンテナンスID
        ''' </summary>
        Public Const LABEL_MAINTE_ID As String = "WI990417"

        ''' <summary>
        ''' メンテナンス期間
        ''' </summary>
        Public Const LABEL_MAINTE_KIKAN As String = "WI990418"

        ''' <summary>
        ''' 備考
        ''' </summary>
        Public Const LABEL_REMARK As String = "WI990419"

        ''' <summary>
        ''' 送り状I/Fの種類選択
        ''' </summary>
        Public Const LABEL_SELECT_INVOICE As String = "WI990420"

        ''' <summary>
        ''' 受渡日
        ''' </summary>
        Public Const LABEL_UKEWATASHI_DAY As String = "WI990421"

        ''' <summary>
        ''' 送り状No
        ''' </summary>
        Public Const LABEL_DEST_NO As String = "WI990422"

        ''' <summary>
        ''' 貸出終了日
        ''' </summary>
        Public Const LABEL_LENDING_END_DATE As String = "WI990423"

        ''' <summary>
        ''' 引継
        ''' </summary>
        Public Const LABEL_HIKITSUGI As String = "WI990424"

        '--2021/11/25 nv miyaji update start 必要なかったのでコメントアウト
        '''' <summary>
        '''' 所属拠点名１
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME1 As String = "WI990425"
        '''' <summary>
        '''' 所属拠点名２
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME2 As String = "WI990426"
        '''' <summary>
        '''' 所属拠点名３
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME3 As String = "WI990427"
        '''' <summary>
        '''' 所属拠点名４
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME4 As String = "WI990428"
        '''' <summary>
        '''' 所属拠点名５
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME5 As String = "WI990429"
        '''' <summary>
        '''' 所属拠点名６
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME6 As String = "WI990430"
        '''' <summary>
        '''' 所属拠点名７
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME7 As String = "WI990431"
        '''' <summary>
        '''' 所属拠点名８
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME8 As String = "WI990432"
        '''' <summary>
        '''' 所属拠点名９
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME9 As String = "WI990433"
        '''' <summary>
        '''' 所属拠点名１０
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME10 As String = "WI990434"
        '''' <summary>
        '''' 所属拠点名１１
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME11 As String = "WI990435"
        '''' <summary>
        '''' 所属拠点名１２
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME12 As String = "WI990436"
        '''' <summary>
        '''' 所属拠点名１３
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME13 As String = "WI990437"
        '''' <summary>
        '''' 所属拠点名１４
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME14 As String = "WI990438"
        '''' <summary>
        '''' 所属拠点名１５
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME15 As String = "WI990439"
        '''' <summary>
        '''' 所属拠点名１６
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME16 As String = "WI990440"
        '''' <summary>
        '''' 所属拠点名１７
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME17 As String = "WI990441"
        '''' <summary>
        '''' 所属拠点名１８
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME18 As String = "WI990442"
        '''' <summary>
        '''' 所属拠点名１９
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME19 As String = "WI990443"
        '''' <summary>
        '''' 所属拠点名２０
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME20 As String = "WI990444"
        '''' <summary>
        '''' 所属拠点名２１
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME21 As String = "WI990445"
        '''' <summary>
        '''' 所属拠点名２２
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME22 As String = "WI990446"
        '''' <summary>
        '''' 所属拠点名２３
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME23 As String = "WI990447"
        '''' <summary>
        '''' 所属拠点名２４
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME24 As String = "WI990448"
        '''' <summary>
        '''' 所属拠点名２５
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME25 As String = "WI990449"
        '''' <summary>
        '''' 所属拠点名２６
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME26 As String = "WI990450"
        '''' <summary>
        '''' 所属拠点名２７
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME27 As String = "WI990451"
        '''' <summary>
        '''' 所属拠点名２８
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME28 As String = "WI990452"
        '''' <summary>
        '''' 所属拠点名２９
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME29 As String = "WI990453"
        '''' <summary>
        '''' 所属拠点名３０
        '''' </summary>
        'Public Const LABEL_HUB_LOC_L_NAME30 As String = "WI990454"
        '--2021/11/25 nv miyaji update end 必要なかったのでコメントアウト

        ''' <summary>
        ''' 申請日　2022/07/25 sv sato add
        ''' </summary>
        Public Const LABEL_REQUEST_DT As String = "WI990425"

        ''' <summary>
        ''' 申請社員コード　2022/07/25 sv sato add
        ''' </summary>
        Public Const LABEL_REQ_EMP_CD As String = "WI990426"

        ''' <summary>
        ''' 担当社員コード　2022/07/25 sv sato add
        ''' </summary>
        Public Const LABEL_CHARGE_EMP_CD As String = "WI990427"

        ''' <summary>
        ''' 送り先名1　2022/07/25 sv sato add
        ''' </summary>
        Public Const LABEL_DEST_NAME1 As String = "WI990428"

        ''' <summary>
        ''' 送り先名2　2022/07/25 sv sato add
        ''' </summary>
        Public Const LABEL_DEST_NAME2 As String = "WI990429"

        ''' <summary>
        ''' 送り先住所1　2022/07/25 sv sato add
        ''' </summary>
        Public Const LABEL_ADDRESS1 As String = "WI990430"

        ''' <summary>
        ''' 送り先住所2　2022/07/25 sv sato add
        ''' </summary>
        Public Const LABEL_ADDRESS2 As String = "WI990431"

        ''' <summary>
        ''' 送り先住所3　2022/07/25 sv sato add
        ''' </summary>
        Public Const LABEL_ADDRESS3 As String = "WI990432"

        ''' <summary>
        ''' 送り状番号　2022/07/25 sv sato add
        ''' </summary>
        Public Const LABEL_SEND_DEST_NUMBER As String = "WI990433"

        '2023/02/08 ADD Start Shirakawa
        ''' <summary>
        ''' 指定数量
        ''' </summary>
        Public Const LABEL_SPECIFIED_QTY As String = "WI990443"
        '2023/02/08 ADD End Shirakawa

        '2023/02/06 Kawamoto ADD START
        ''' <summary>
        ''' 残量割合順
        ''' </summary>
        Public Const LABEL_SORT_CONSUMPTION_RATE As String = "WI990444"
        '2023/02/06 Kawamoto ADD END

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' 残量(NET重量)
        ''' </summary>
        Public Const LABEL_WEIGHT As String = "WI990445"

        ''' <summary>
        ''' 作業者コード/名
        ''' </summary>
        Public Const LABEL_WORKER_CD_NAME As String = "WI990446"

        ''' <summary>
        ''' 作業時間
        ''' </summary>
        Public Const LABEL_WORKING_HOURS As String = "WI990447"

        ''' <summary>
        ''' 作業概要
        ''' </summary>
        Public Const LABEL_WORK_SUMMARY As String = "WI990448"

        ''' <summary>
        ''' 返却備考1
        ''' </summary>
        Public Const LABEL_RETURN_REMARK1 As String = "WI990449"

        ''' <summary>
        ''' 返却備考2
        ''' </summary>
        Public Const LABEL_RETURN_REMARK2 As String = "WI990450"

        ''' <summary>
        ''' ファイル名
        ''' </summary>
        Public Const LABEL_FILE_NAME As String = "WI990451"

        ''' <summary>
        ''' バイト数
        ''' </summary>
        Public Const LABEL_BYTE_NUM As String = "WI990452"

        ''' <summary>
        ''' アップロード日時
        ''' </summary>
        Public Const LABEL_UPLOAD_DATE As String = "WI990453"

        ''' <summary>
        ''' 添付ファイル管理
        ''' </summary>
        Public Const LABEL_ATTACHMENT_MANAGEMENT As String = "WI990454"

        ''' <summary>
        ''' 添付ファイル
        ''' </summary>
        Public Const LABEL_ATTACHMENT As String = "WI990455"

        ''' <summary>
        ''' 二次貸出先
        ''' </summary>
        Public Const LABEL_SECOND_LEASE_PLACE As String = "WI990456"

        ''' <summary>
        ''' 対象
        ''' </summary>
        Public Const LABEL_SUBJECT As String = "WI990457"

        ''' <summary>
        ''' 送り先
        ''' </summary>
        Public Const LABEL_SEND As String = "WI990458"

        ''' <summary>
        ''' 貸出確認書宛名
        ''' </summary>
        Public Const LABEL_TO_DEST_NAME As String = "WI990459"

        ''' <summary>
        ''' 返送先
        ''' </summary>
        Public Const LABEL_RETURN_PLACE As String = "WI990460"

        ''' <summary>
        ''' 返送先郵便番号
        ''' </summary>
        Public Const LABEL_RETURN_PLACE_ZIP_CODE As String = "WI990461"

        ''' <summary>
        ''' 返送先住所
        ''' </summary>
        Public Const LABEL_RETURN_PLACE_ADDRESS As String = "WI990462"

        ''' <summary>
        ''' 返送先住所1
        ''' </summary>
        Public Const LABEL_RETURN_PLACE_ADDRESS1 As String = "WI990463"

        ''' <summary>
        ''' 返送先住所2
        ''' </summary>
        Public Const LABEL_RETURN_PLACE_ADDRESS2 As String = "WI990464"

        ''' <summary>
        ''' 返送先住所3
        ''' </summary>
        Public Const LABEL_RETURN_PLACE_ADDRESS3 As String = "WI990465"

        ''' <summary>
        ''' 返送先(貸出先コード)
        ''' </summary>
        Public Const LABEL_RETURN_PLACE_CODE As String = "WI990466"
        '2023/03/22 Kawamoto ADD END

        '2023/04/30 Sato v2.4 ADD START
        ''' <summary>
        ''' 保管場所　
        ''' </summary>
        Public Const LABEL_STORAGE_LOCATION As String = "WI990467"

        ''' <summary>
        ''' 階　
        ''' </summary>
        Public Const LABEL_FLOOR As String = "WI990468"

        ''' <summary>
        ''' 棚　
        ''' </summary>
        Public Const LABEL_SHELF As String = "WI990469"

        ''' <summary>
        ''' 薬品番号　
        ''' </summary>
        Public Const LABEL_DRUG_NO As String = "WI990470"

        ''' <summary>
        ''' 薬品名称　
        ''' </summary>
        Public Const LABEL_DRUG_NAME As String = "WI990471"

        ''' <summary>
        ''' ロット番号
        ''' </summary>
        Public Const LABEL_LOT_NO As String = "WI990472"

        ''' <summary>
        ''' 購入者
        ''' </summary>
        Public Const LABEL_PURCHASER As String = "WI990473"

        ''' <summary>
        ''' 納入日　
        ''' </summary>
        Public Const LABEL_DUE_DATE As String = "WI990474"

        ''' <summary>
        ''' 直近確認日　
        ''' </summary>
        Public Const LABEL_LATEST_CHECK_DATE As String = "WI990475"

        ''' <summary>
        ''' 薬品名　
        ''' </summary>
        Public Const LABEL_DRUG_NAME_SHORT As String = "WI990476"

        ''' <summary>
        ''' ▼薬品指定
        ''' </summary>
        Public Const LABEL_DRUG_SPECIFY_OEPN As String = "WI990477"

        ''' <summary>
        ''' ▲薬品指定　
        ''' </summary>
        Public Const LABEL_DRUG_SPECIFY_CLOSE As String = "WI990478"

        ''' <summary>
        ''' 重量　
        ''' </summary>
        Public Const LABEL_WEIGHT_VAL As String = "WI990479"

        ''' <summary>
        ''' (全在庫薬品の合算)
        ''' </summary>
        Public Const LABEL_ALL_DRUG_TOTAL As String = "WI990480"
        '2023/04/30 Sato v2.4 ADD END

        ''' <summary>
        ''' 品番　※資産登録/変更画面用
        ''' </summary>
        Public Const LABEL_PRODUCT_NO_REGIST As String = "WI990481"

        ''' <summary>
        ''' 特化則又は労安法(がん原性物質)の該当有無　※資産登録/変更画面用
        ''' </summary>
        Public Const LABEL_SPECIAL_RULE_REGIST As String = "WI990482"

        ''' <summary>
        ''' 入力必須
        ''' </summary>
        Public Const LABEL_REQUIRED_INPUT As String = "WI990483"

        ''' <summary>
        ''' ロケ大区分
        ''' </summary>
        Public Const LABEL_INV_LOC_L_KUBUN As String = "WI990486"

        '-----値(V)-----

        ''' <summary>
        ''' 準備中
        ''' </summary>
        Public Const VALUE_ASSET_STATUS_JUNBI As String = "WV990011"

        ''' <summary>
        ''' 保管中
        ''' </summary>
        Public Const VALUE_ASSET_STATUS_HOKAN As String = "WV990012"

        ''' <summary>
        ''' 貸出中
        ''' </summary>
        Public Const VALUE_ASSET_STATUS_LEASE As String = "WV990013"

        ''' <summary>
        ''' 除却済
        ''' </summary>
        Public Const VALUE_ASSET_STATUS_RETIRE As String = "WV990014"

        ''' <summary>
        ''' 設定未
        ''' </summary>
        Public Const VALUE_SETTING_MI As String = "WV990021"

        ''' <summary>
        ''' 設定済
        ''' </summary>
        Public Const VALUE_SETTING_SUMI As String = "WV990022"

        ''' <summary>
        ''' 未
        ''' </summary>
        Public Const VALUE_MI As String = "WV990031"

        ''' <summary>
        ''' 済
        ''' </summary>
        Public Const VALUE_SUMI As String = "WV990032"

        ''' <summary>
        ''' 現物なし
        ''' </summary>
        Public Const VALUE_JITSUTANA_STATUS_NASHI As String = "WV990042"

        ''' <summary>
        ''' 正常
        ''' </summary>
        Public Const VALUE_JITSUTANA_STATUS_SEIJO As String = "WV990043"

        ''' <summary>
        ''' 現物あり
        ''' </summary>
        Public Const VALUE_JITSUTANA_STATUS_ARI As String = "WV990044"

        ''' <summary>
        ''' 大区分
        ''' </summary>
        Public Const VALUE_LOC_L As String = "WV990051"

        ''' <summary>
        ''' 中区分
        ''' </summary>
        Public Const VALUE_LOC_M As String = "WV990052"

        ''' <summary>
        ''' 小区分
        ''' </summary>
        Public Const VALUE_LOC_S As String = "WV990053"

        ''' <summary>
        ''' 大分類
        ''' </summary>
        Public Const VALUE_CAT_L As String = "WV990061"

        ''' <summary>
        ''' 中分類
        ''' </summary>
        Public Const VALUE_CAT_M As String = "WV990062"

        ''' <summary>
        ''' 小分類
        ''' </summary>
        Public Const VALUE_CAT_S As String = "WV990063"

        ''' <summary>
        ''' 上位分類に準ずる
        ''' </summary>
        Public Const VALUE_ALERT_DIV_F As String = "WV990071"

        ''' <summary>
        ''' アラート表示しない
        ''' </summary>
        Public Const VALUE_ALERT_DIV_N As String = "WV990072"

        ''' <summary>
        ''' アラート表示する
        ''' </summary>
        Public Const VALUE_ALERT_DIV_Y As String = "WV990073"

        ''' <summary>
        ''' 社員マスタメンテナンス:権限区分名（管理者）
        ''' </summary>
        Public Const LABEL_KENGEN_ADMIN As String = "WV990081"

        ''' <summary>
        ''' 社員マスタメンテナンス:権限区分名（一般者）
        ''' </summary>
        Public Const LABEL_KENGEN_NOMAL As String = "WV990082"

        ''' <summary>
        ''' 操作ログ照会:Web
        ''' </summary>
        Public Const LABEL_PROCESS_TOOL_WEB As String = "WV990091"

        ''' <summary>
        ''' 操作ログ照会:C/S
        ''' </summary>
        Public Const LABEL_PROCESS_TOOL_CS As String = "WV990092"

        ''' <summary>
        ''' 操作ログ照会:H/T
        ''' </summary>
        Public Const LABEL_PROCESS_TOOL_HT As String = "WV990093"

        ''' <summary>
        ''' 文言区分:項目名
        ''' </summary>
        Public Const LABEL_ITEM_NAME As String = "WV990102"

        ''' <summary>
        ''' 文言区分:タイトル
        ''' </summary>
        Public Const LABEL_TITLE As String = "WV990101"

        ''' <summary>
        ''' 文言区分:ボタン
        ''' </summary>
        Public Const LABEL_BUTTON As String = "WV990103"

        ''' <summary>
        ''' 文言区分:値
        ''' </summary>
        Public Const LABEL_VALUE As String = "WV990104"

        ''' <summary>
        ''' 文言区分:メッセージ
        ''' </summary>
        Public Const LABEL_MESSAGE As String = "WV990105"

        ''' <summary>
        ''' 登録(メンテ)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_MI As String = "WV990121"

        ''' <summary>
        ''' 登録(取込)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_CI As String = "WV990122"

        ''' <summary>
        ''' 更新(メンテ)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_MU As String = "WV990123"

        ''' <summary>
        ''' 更新(取込)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_CU As String = "WV990124"

        ''' <summary>
        ''' 除却(メンテ)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_MR As String = "WV990125"

        ''' <summary>
        ''' 除却(取込)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_CR As String = "WV990126"

        ''' <summary>
        ''' 削除(メンテ)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_MD As String = "WV990127"

        ''' <summary>
        ''' タグラベル発行＆紐付
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_T1 As String = "WV990128"

        ''' <summary>
        ''' タグ紐付
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_T2 As String = "WV990129"

        ''' <summary>
        ''' 保管
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_BS As String = "WV990130"

        ''' <summary>
        ''' 貸出
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_BO As String = "WV990131"

        ''' <summary>
        ''' 返却
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_BI As String = "WV990132"

        ''' <summary>
        ''' 除却
        ''' </summary>
        Public Const VALUE_RETIRE As String = "WV990133"

        ''' <summary>
        ''' 現ロケ変更
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_LC As String = "WV990134"

        ''' <summary>
        ''' 操作ログ照会:操作
        ''' </summary>
        Public Const LABEL_PROCESS_CAT_OP As String = "WV990152"

        ''' <summary>
        ''' 操作ログ照会:イベント
        ''' </summary>
        Public Const LABEL_PROCESS_CAT_EV As String = "WV990151"

        ''' <summary>
        ''' 操作ログ照会:エラー
        ''' </summary>
        Public Const LABEL_PROCESS_CAT_ER As String = "WV990153"


        ''' <summary>
        ''' 生成しない(タグ紐付で行う)
        ''' </summary>
        Public Const VALUE_ASSET_ID_REG_DIV_NONE As String = "WV990161"

        ''' <summary>
        ''' 資産番号より生成
        ''' </summary>
        Public Const VALUE_ASSET_ID_REG_DIV_ASSET_NO As String = "WV990162"

        ''' <summary>
        ''' 自動採番で生成
        ''' </summary>
        Public Const VALUE_ASSET_ID_REG_DIV_AUTO As String = "WV990163"

        ''' <summary>
        ''' 入力・CSV取込時の資産IDをそのまま使用する
        ''' </summary>
        Public Const VALUE_ASSET_ID_REG_DIV_INP As String = "WV990164"

        ''' <summary>
        ''' GIAI
        ''' </summary>
        Public Const VALUE_ASSET_ID_REG_DIV_GIAI As String = "WV990165"

        ''' <summary>
        ''' GRAI
        ''' </summary>
        Public Const VALUE_ASSET_ID_REG_DIV_GRAI As String = "WV990166"

        ''' <summary>
        ''' EPC領域サイズ:064
        ''' </summary>
        Public Const VALUE_EPC_SIZE_064 As String = "WV990171"

        ''' <summary>
        ''' EPC領域サイズ:096
        ''' </summary>
        Public Const VALUE_EPC_SIZE_096 As String = "WV990172"

        ''' <summary>
        ''' EPC領域サイズ:128
        ''' </summary>
        Public Const VALUE_EPC_SIZE_128 As String = "WV990173"

        ''' <summary>
        ''' EPC領域サイズ:256
        ''' </summary>
        Public Const VALUE_EPC_SIZE_256 As String = "WV990174"

        ''' <summary>
        ''' EPC領域サイズ:512
        ''' </summary>
        Public Const VALUE_EPC_SIZE_512 As String = "WV990175"

        ''' <summary>
        ''' 削除(取込)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_CD As String = "WV990176"

        ''' <summary>
        ''' 返却済
        ''' </summary>
        Public Const VALUE_LEASESTATUS_RETURNED As String = "WV990181"

        ''' <summary>
        ''' 貸出中
        ''' </summary>
        Public Const VALUE_LEASESTATUS_LENDING As String = "WV990182"

        ''' <summary>
        ''' 期限切
        ''' </summary>
        Public Const VALUE_LEASESTATUS_LIMIT_EXPIRED As String = "WV990183"

        ''' <summary>
        ''' 貸出上限エラー区分:警告(上限を超える貸出の場合アラートにする)
        ''' </summary>
        Public Const VALUE_LEASE_UPPER_ERR_DIV_N As String = "WV990191"

        ''' <summary>
        ''' 貸出上限エラー区分:エラー(上限を超える貸出の場合エラーにする)
        ''' </summary>
        Public Const VALUE_LEASE_UPPER_ERR_DIV_Y As String = "WV990192"

        ''' <summary>
        ''' 貸出上限エラー区分:しない
        ''' </summary>
        Public Const VALUE_LEASE_UPPER_ERR_DIV_NONE As String = "WV990193"

        ''' <summary>
        ''' 有効
        ''' </summary>
        Public Const VALUE_VALID As String = "WV990201"

        ''' <summary>
        ''' 無効
        ''' </summary>
        Public Const VALUE_INVALID As String = "WV990202"

        ''' <summary>
        ''' エラー
        ''' </summary>
        Public Const VALUE_GATE_PASS_DIV_ER As String = "WV990211"

        ''' <summary>
        ''' 入退室
        ''' </summary>
        Public Const VALUE_GATE_PASS_DIV_IO As String = "WV990212"

        ''' <summary>
        ''' 持出/返却
        ''' </summary>
        Public Const VALUE_GATE_PASS_DIV_LR As String = "WV990213"

        ''' <summary>
        ''' 共通拠点フラグ:共通
        ''' </summary>
        Public Const VALUE_LOC_SHARE_FLG_Y As String = "WV990214"

        ''' <summary>
        ''' 共通拠点フラグ:一般拠点
        ''' </summary>
        Public Const VALUE_LOC_SHARE_FLG_N As String = "WV990215"

        ''' <summary>
        ''' 〇
        ''' </summary>
        Public Const VALUE_MARK_CIRCLE As String = "WV990216"

        ''' <summary>
        ''' 本部
        ''' </summary>
        Public Const VALUE_HEAD_OFFICE_FLG_Y As String = "WV990217"

        ''' <summary>
        ''' 拠点
        ''' </summary>
        Public Const VALUE_HEAD_OFFICE_FLG_N As String = "WV990218"

        ''' <summary>
        ''' 未貸出
        ''' </summary>
        Public Const VALUE_NOT_RENTED As String = "WV990219"

        ''' <summary>
        ''' メンテ
        ''' </summary>
        Public Const VALUE_MAINTE As String = "WV990220"

        ''' <summary>
        ''' 予約観点
        ''' </summary>
        Public Const VALUE_SORT_RESERVE As String = "WV990221"

        ''' <summary>
        ''' メンテナンス観点
        ''' </summary>
        Public Const VALUE_SORT_MAINTE As String = "WV990222"

        ''' <summary>
        ''' 資産
        ''' </summary>
        Public Const VALUE_CSV_DIV_ASSET As String = "WV990223"

        ''' <summary>
        ''' 社員
        ''' </summary>
        Public Const VALUE_CSV_DIV_EMPLOYEE As String = "WV990224"

        ''' <summary>
        ''' 貸出中(遅延)
        ''' </summary>
        Public Const VALUE_LENDING_DELAY As String = "WV990225"

        ''' <summary>
        ''' 未通知
        ''' </summary>
        Public Const VALUE_NOT_NOTIFIED As String = "WV990226"

        ''' <summary>
        ''' 通知済
        ''' </summary>
        Public Const VALUE_NOTIFIED As String = "WV990227"

        ''' <summary>
        ''' 可能
        ''' </summary>
        Public Const VALUE_KANOU As String = "WV990228"

        ''' <summary>
        ''' 不可
        ''' </summary>
        Public Const VALUE_FUKA As String = "WV990229"

        ''' <summary>
        ''' 返却遅延
        ''' </summary>
        Public Const VALUE_LEASESTATUS_RETUEN_DELAY As String = "WV990230"

        ''' <summary>
        ''' 予約中
        ''' </summary>
        Public Const VALUE_LEASESTATUS_RESERVE As String = "WV990231"

        ''' <summary>
        ''' 予約登録
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_RI As String = "WV990232"

        ''' <summary>
        ''' 予約変更
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_RU As String = "WV990233"

        ''' <summary>
        ''' 予約削除
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_RD As String = "WV990234"

        ''' <summary>
        ''' 予約なし
        ''' </summary>
        Public Const VALUE_NO_RESERVE As String = "WV990235"

        '2022/7/25 sv sato add start
        ''' <summary>
        ''' 予約登録(取込)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_XI As String = "WV990236"

        ''' <summary>
        ''' 予約変更(取込)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_XU As String = "WV990237"

        ''' <summary>
        ''' 予約削除(取込)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_XD As String = "WV990238"
        '2022/7/25 sv sato add end

        '2023/02/08 Kawamoto ADD START
        ''' <summary>
        ''' Y
        ''' </summary>
        Public Const VALUE_YES As String = "WV990239"

        ''' <summary>
        ''' N
        ''' </summary>
        Public Const VALUE_NO As String = "WV990240"

        ''' <summary>
        ''' -
        ''' </summary>
        Public Const VALUE_NONE As String = "WV990241"

        ''' <summary>
        ''' 毒物
        ''' </summary>
        Public Const VALUE_POISIN As String = "WV990242"

        ''' <summary>
        ''' 劇物
        ''' </summary>
        Public Const VALUE_DELETERIOUS As String = "WV990243"

        ''' <summary>
        ''' 指定なし
        ''' </summary>
        Public Const VALUE_UNSPECIFIED As String = "WV990244"
        '2023/02/08 Kawamoto ADD END

        '2023/04/30 Sato ADD START
        ''' <summary>
        ''' 有
        ''' </summary>
        Public Const VALUE_VALID_1_LETTER As String = "WV990245"

        ''' <summary>
        ''' 無
        ''' </summary>
        Public Const VALUE_INVALID_1_LETTER As String = "WV990246"

        ''' <summary>
        ''' 特化則
        ''' </summary>
        Public Const VALUE_SPECIFIC As String = "WV990247"

        ''' <summary>
        ''' 毒劇物
        ''' </summary>
        Public Const VALUE_POISOUNUOS As String = "WV990248"

        ''' <summary>
        ''' 有規則
        ''' </summary>
        Public Const VALUE_ORGANIC As String = "WV990249"

        ''' <summary>
        ''' 残量確認
        ''' </summary>
        Public Const VALUE_REMAIN_CHECK As String = "WV990250"
        '2023/04/30 Sato ADD END

        ''' <summary>
        ''' 未完了
        ''' </summary>
        Public Const VALUE_CONSTRUCTION_STATUS_INCOMPLETE As String = "WV990251"

        ''' <summary>
        ''' 完了
        ''' </summary>
        Public Const VALUE_CONSTRUCTION_STATUS_COMPLETE As String = "WV990252"

        ''' <summary>
        ''' 他工事に紐づき済み
        ''' </summary>
        Public Const VALUE_REL_CONSTRUCTION_STATUS_LINKED As String = "WV990253"

        ''' <summary>
        ''' 未紐付け
        ''' </summary>
        Public Const VALUE_REL_CONSTRUCTION_STATUS_UNLINKED As String = "WV990254"

        ''' <summary>
        ''' ○
        ''' </summary>
        Public Const VALUE_CIRCLE As String = "WV990255"

        ''' <summary>
        ''' ×
        ''' </summary>
        Public Const VALUE_CROSS_MARK As String = "WV990256"

        ''' <summary>
        ''' 仮置きあり
        ''' </summary>
        Public Const VALUE_TEMP_EXISTS As String = "WV990257"

        ''' <summary>
        ''' 仮置きなし
        ''' </summary>
        Public Const VALUE_TEMP_NOT_EXISTS As String = "WV990258"

        ''' <summary>
        ''' 工事登録
        ''' </summary>
        Public Const VALUE_CONST_REGIST As String = "WV990259"

        ''' <summary>
        ''' 仮置き紐付
        ''' </summary>
        Public Const VALUE_TEMP_LINK As String = "WV990260"

        ''' <summary>
        ''' 貸出
        ''' </summary>
        Public Const VALUE_LEASE As String = "WV990261"

        ''' <summary>
        ''' 持出照合
        ''' </summary>
        Public Const VALUE_IN_MATCH As String = "WV990262"

        ''' <summary>
        ''' 仮置き登録
        ''' </summary>
        Public Const VALUE_TEMP_REGIST As String = "WV990263"

        ''' <summary>
        ''' 持帰照合
        ''' </summary>
        Public Const VALUE_OUT_MATCH As String = "WV990264"

        ''' <summary>
        ''' 照合返却
        ''' </summary>
        Public Const VALUE_MATCH_RETURN As String = "WV990265"

        ''' <summary>
        ''' 返却
        ''' </summary>
        Public Const VALUE_RETURNED As String = "WV990266"

        ''' <summary>
        ''' 工事完了
        ''' </summary>
        Public Const VALUE_CONST_COMPLETE As String = "WV990267"

        ''' <summary>
        ''' 別工事紐付
        ''' </summary>
        Public Const VALUE_ANOTHER_CONST_LINK As String = "WV990268"

        ''' <summary>
        ''' 仮置き解除
        ''' </summary>
        Public Const VALUE_TEMP_RELEASE As String = "WV990269"

        ''' <summary>
        ''' 照合なし
        ''' </summary>
        Public Const VALUE_NONE_MATCH As String = "WV990270"

        ''' <summary>
        ''' 照合OK
        ''' </summary>
        Public Const VALUE_MATCH_OK As String = "WV990271"

        ''' <summary>
        ''' 照合リスト外
        ''' </summary>
        Public Const VALUE_OUT_OF_MATCH_LIST As String = "WV990272"

        ''' <summary>
        ''' 照合NG
        ''' </summary>
        Public Const VALUE_MATCH_NG As String = "WV990273"

        ''' <summary>
        ''' 保管中(返却済み)
        ''' </summary>
        Public Const VALUE_STORAGE_RETURNED As String = "WV990274"

        ''' <summary>
        ''' 仮置き中
        ''' </summary>
        Public Const VALUE_DURING_TEMP As String = "WV990275"

        ''' <summary>
        ''' 仮置きしていない
        ''' </summary>
        Public Const VALUE_NOT_TEMP As String = "WV990276"

        ''' <summary>
        ''' 工事
        ''' </summary>
        Public Const VALUE_CSV_DIV_CONSTRUCTION As String = "WV990277"

        ''' <summary>
        ''' 文言区分:設定マスタ備考
        ''' </summary>
        Public Const LABEL_SETTING_REMARK As String = "WV990278"

        ''' <summary>
        ''' 文言区分:CSVヘッダ項目名
        ''' </summary>
        Public Const LABEL_CSV_ITEM As String = "WV990279"

        ''' <summary>
        ''' 文言区分:機能分類名
        ''' </summary>
        Public Const LABEL_SETTING_CATEGORY As String = "WV990280"

        ''' <summary>
        ''' 商品
        ''' </summary>
        Public Const VALUE_CSV_DIV_GOODS As String = "WV770002"

        ''' <summary>
        ''' 文言区分:〇〇登録
        ''' </summary>
        Public Const VALUE_BLAREGIST As String = "WV990281"

        ''' <summary>
        ''' 文言区分:予備登録１
        ''' </summary>
        Public Const VALUE_PREREGIST1 As String = "WV990282"

        ''' <summary>
        ''' 文言区分:予備登録２
        ''' </summary>
        Public Const VALUE_PREREGIST2 As String = "WV990283"

        ''' <summary>
        ''' 入庫予定
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_LP As String = "WV990284"

        ''' <summary>
        ''' 登録(ラベル発行)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_LI As String = "WV990285"

        ''' <summary>
        ''' 登録(ラベル不要)
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_UI As String = "WV990286"

        ''' <summary>
        ''' 会社
        ''' </summary>
        Public Const VALUE_LOC_L_COM As String = "WV990287"

        ''' <summary>
        ''' 部署
        ''' </summary>
        Public Const VALUE_LOC_M_DEP As String = "WV990288"

        ''' <summary>
        ''' 場所
        ''' </summary>
        Public Const VALUE_LOC_S_PLA As String = "WV990289"

        ''' <summary>
        ''' 有形資産
        ''' </summary>
        Public Const VALUE_TANGIBLE_ASSET As String = "WV990290"

        ''' <summary>
        ''' 無形資産
        ''' </summary>
        Public Const VALUE_INTANGIBLE_ASSET As String = "WV990291"

        ''' <summary>
        ''' 棚卸確定
        ''' </summary>
        Public Const VALUE_PROCESS_DIV_IC As String = "WV990292"

        ''' <summary>
        ''' ロケーション会社コード/名
        ''' </summary>
        Public Const LABEL_LOC_L_COM_NAME As String = "WI990683"

        ''' <summary>
        ''' ロケーション部署コード/名
        ''' </summary>
        Public Const LABEL_LOC_M_DEP_NAME As String = "WI990684"

        ''' <summary>
        ''' ロケーション場所コード/名
        ''' </summary>
        Public Const LABEL_LOC_S_PLA_NAME As String = "WI990685"

        ''' <summary>
        ''' 会社コード
        ''' </summary>
        Public Const LABEL_LOC_L_COM_CODE As String = "WI990686"

        ''' <summary>
        ''' 原価センタ
        ''' </summary>
        Public Const LABEL_LOC_M_DEP_CODE As String = "WI990687"

        ''' <summary>
        ''' 場所
        ''' </summary>
        Public Const LABEL_LOC_S_PLA_CODE As String = "WI990715"

        ''' <summary>
        ''' 所属拠点
        ''' </summary>
        Public Const LABEL_LOC_HUB As String = "WI990688"

        ''' <summary>
        ''' 並び順
        ''' </summary>
        ''' <remarks>既存の項目配置マスタメンテ用文言(WI780003)を流用する。</remarks>
        Public Const LABEL_SORT_ORDER As String = "WI780003"

        ''' <summary>
        ''' 昇順
        ''' </summary>
        Public Const LABEL_SORT_ASC As String = "WI990706"

        ''' <summary>
        ''' 降順
        ''' </summary>
        Public Const LABEL_SORT_DESC As String = "WI990707"

        ''' <summary>
        ''' 全てのアイテムを削除(×ボタンツールチップ)
        ''' </summary>
        Public Const LABEL_TOOLTIP_ALL_CLEAR As String = "WI990708"

        ''' <summary>
        ''' クリア(×ボタンツールチップ)
        ''' </summary>
        Public Const LABEL_TOOLTIP_CLEAR As String = "WI990709"

        ''' <summary>
        ''' 場所から選ぶ
        ''' </summary>
        Public Const LABEL_SELECT_FROM_PLACE As String = "WI990710"


        '-----メッセージ(M)-----

        ''' <summary>
        ''' 正常終了しました。
        ''' </summary>
        Public Const MSG_INPORTOK As String = "WM990001"

        ''' <summary>
        ''' データ取込中。しばらくお待ち下さい。
        ''' </summary>
        Public Const MSG_INPORT As String = "WM990005"

        ''' <summary>
        ''' 取込失敗。一覧内容を参考にデータ修正して下さい。
        ''' </summary>
        Public Const MSG_INPORTNG As String = "WM990006"

        ''' <summary>
        ''' 項目値がありません。
        ''' </summary>
        Public Const MSG_ERR_DATAEMPTY As String = "WM990007"

        ''' <summary>
        ''' 登録できる文字数を超えています。
        ''' </summary>
        Public Const MSG_ERR_DATAOVER As String = "WM990008"

        ''' <summary>
        ''' ファイルを選択して下さい。
        ''' </summary>
        Public Const MSG_ERR_SELECT As String = "WM990009"

        ''' <summary>
        ''' 空のファイルです。
        ''' </summary>
        Public Const MSG_ERR_EMPTY As String = "WM990010"

        ''' <summary>
        ''' CSVファイルを選択して下さい。
        ''' </summary>
        Public Const MSG_ERR_NOTCSV As String = "WM990011"

        ''' <summary>
        ''' 項目数が違います。
        ''' </summary>
        Public Const MSG_ERR_KOUMOKUSUU As String = "WM990012"

        ''' <summary>
        ''' 取込失敗しました。
        ''' </summary>
        Public Const MSG_ERR_ELSE As String = "WM990013"

        ''' <summary>
        ''' キーが重複しています。
        ''' </summary>
        Public Const MSG_ERR_DATAJUUFUKU As String = "WM990014"

        ''' <summary>
        ''' 文字の制約に反します。
        ''' </summary>
        Public Const MSG_ERR_DATASEIYAKU As String = "WM990015"

        ''' <summary>
        ''' ログイン情報がありません。
        ''' </summary>
        Public Const MSG_ERR_LOGININFO_EMPTY As String = "WM990016"

        ''' <summary>
        ''' 新パスワードを入力して下さい。
        ''' </summary>
        Public Const MSG_ERR_INPUT_NEWPASSWORD As String = "WM990017"

        ''' <summary>
        ''' 確認パスワードを入力して下さい。
        ''' </summary>
        Public Const MSG_ERR_INPUT_KAKUNINPASSWORD As String = "WM990018"

        ''' <summary>
        ''' 新パスワードと確認パスワードが一致しません。
        ''' </summary>
        Public Const MSG_ERR_NEWPASSWORD As String = "WM990019"

        ''' <summary>
        ''' パスワードが一致しません。
        ''' </summary>
        Public Const MSG_ERR_PASSWORD As String = "WM990020"

        ''' <summary>
        ''' 該当の社員データがありません。
        ''' </summary>
        Public Const MSG_ERR_EMPLOYEECD As String = "WM990021"

        ''' <summary>
        ''' 上位の大区分コードが存在しません。
        ''' </summary>
        Public Const MSG_ERR_NOT_EXIST_LOC_L_CD As String = "WM990025"

        ''' <summary>
        ''' 上位の中区分コードが存在しません。
        ''' </summary>
        Public Const MSG_ERR_NOT_EXIST_LOC_M_CD As String = "WM990026"

        ''' <summary>
        ''' 設定マスタにデータが存在しません。({0})
        ''' </summary>
        Public Const MSG_ERR_NOT_EXIST_SETTING As String = "WM990027"

        ''' <summary>
        ''' 未入力エラー
        ''' </summary>
        Public Const MSG_ERR_NOT_ENTERD As String = "WM990028"

        ''' <summary>
        ''' 最大桁数オーバー
        ''' </summary>
        Public Const MSG_ERR_MAXLENGTH As String = "WM990029"

        ''' <summary>
        ''' 予約可能数を超えています。
        ''' </summary>
        Public Const MSG_ERR_RESERVE_POSSIBLE_OVER As String = "WM410016"

        ''' <summary>
        ''' カテゴリーの変更はできません。
        ''' </summary>
        Public Const MSG_ERR_ASSET_ID_REG As String = "WM770003"

        ''' <summary>
        ''' 商品マスタにデータがありません。
        ''' </summary>
        Public Const MSG_ERR_GOODS_NOT_EXISTS As String = "WM770004"

        ''' <summary>
        ''' 資産画像ファイルが見つかりません。選択し直してください。
        ''' </summary>
        Public Const MSG_ERR_PIC_FILE_NOT_FOUND As String = "WM770006"

        ''' <summary>
        ''' 同じ並び順は指定できません。
        ''' </summary>
        Public Const MSG_ERR_DUPLICATE_DISP_ORDER As String = "WM780001"

        ''' <summary>
        ''' 表示する項目を選択してください。
        ''' </summary>
        Public Const MSG_ERR_NOT_EXISTS_DISP_ITEM As String = "WM780002"

        ''' <summary>
        ''' 固定列数が不正です。
        ''' </summary>
        Public Const MSG_ERR_FIX_COL_CNT As String = "WM780003"

        ''' <summary>
        ''' 表示する項目以上の固定列の指定はできません。
        ''' </summary>
        Public Const MSG_ERR_DISP_OVER_FIX_COL_CNT As String = "WM780004"

        ''' <summary>
        ''' 0を指定すると固定列表示は行いません。
        ''' </summary>
        Public Const MSG_ERR_NOT_FIX_COL As String = "WM780005"

        ''' <summary>
        ''' 表示順を変更する場合は、一覧の行をドラッグして表示したい位置でドロップしてください。
        ''' </summary>
        Public Const MSG_ERR_ORDER_CHANGE_DRAG_AND_DROP As String = "WM780006"

        ''' <summary>
        ''' 上位の大分類コードが存在しません。
        ''' </summary>
        Public Const MSG_ERR_NOT_EXIST_CAT_L_CD As String = "WM990030"

        ''' <summary>
        ''' 上位の中分類コードが存在しません。
        ''' </summary>
        Public Const MSG_ERR_NOT_EXIST_CAT_M_CD As String = "WM990031"

        ''' <summary>
        ''' 処理区分を選択してください。
        ''' </summary>
        Public Const MSG_ERR_PROCESS_DIV_UNSELECTED As String = "WM990032"

        ''' <summary>
        ''' 処理ツールを選択してください。
        ''' </summary>
        Public Const MSG_ERR_PROCESS_TOOL_DIV_UNSELECTED As String = "WM990033"

        ''' <summary>
        ''' 16進数で入力して下さい。
        ''' </summary>
        Public Const MSG_ERR_NOT_HEX As String = "WM990034"

        ''' <summary>
        ''' 入力範囲エラー
        ''' </summary>
        Public Const MSG_ERR_INPUT_RANGE As String = "WM990035"

        ''' <summary>
        ''' データベース接続エラー。
        ''' </summary>
        Public Const MSG_ERR_DB_CONNECT_ERR As String = "WM990036"

        ''' <summary>
        ''' データベース構文エラー。
        ''' </summary>
        Public Const MSG_ERR_DB_SQL_ERR As String = "WM990037"

        ''' <summary>
        ''' マスタに存在しないコードです。
        ''' </summary>
        Public Const MSG_ERR_NOTEXIST_DB As String = "WM990038"

        ''' <summary>
        ''' 資産IDが変更されています。
        ''' </summary>
        Public Const MSG_ERR_CHANGED_ID As String = "WM990039"

        ''' <summary>
        ''' カテゴリ大分類が変更されています。
        ''' </summary>
        Public Const MSG_ERR_CHANGED_CAT As String = "WM990040"

        '''' <summary>
        '''' 画像ファイルが存在しません。
        '''' </summary>
        'Public Const MSG_ERR_NOTEXIST_PICTURE As String = "WM990041"

        ''' <summary>
        ''' 資産IDの桁数がEPC領域サイズを超えています。
        ''' </summary>
        Public Const MSG_ERR_ASSETID_LENGTH As String = "WM990043"

        ''' <summary>
        ''' 登録が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_CREATE As String = "WM990044"

        ''' <summary>
        ''' 変更が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_EDIT As String = "WM990045"

        ''' <summary>
        ''' ゲートマスタ更新完了メッセージ（設定を更新しました。AsetraGateサービスを再起動してください。）
        ''' </summary>
        Public Const MSG_SUCCESS_EDIT_GATE As String = "WM990073"

        ''' <summary>
        ''' 削除が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_DELETE As String = "WM990046"

        ''' <summary>
        ''' 除却が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_RETIRE As String = "WM990047"

        ''' <summary>
        ''' 除却解除が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_RETIRE_OFF As String = "WM990048"

        ''' <summary>
        ''' 棚卸開始処理が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_INV_START As String = "WM990049"

        ''' <summary>
        ''' 棚卸確定処理が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_INV_KAKUTEI As String = "WM990050"

        ''' <summary>
        ''' 資産ID(EPC)が未登録のため、発行できません。
        ''' </summary>
        Public Const MSG_ERR_ID_MITOUROKU As String = "WM990051"

        ''' <summary>
        ''' 紐付け処理を行う資産はタグ発行できません。
        ''' </summary>
        Public Const MSG_ERR_ID_HIMODUKE As String = "WM990052"

        ''' <summary>
        ''' 既に使用されているデータがCSVファイルに存在しません。
        ''' </summary>
        Public Const MSG_NOT_EXIST_USED_DATA As String = "WM990053"

        ''' <summary>
        ''' 発行データの送信が完了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_HAKKO As String = "WM990054"

        ''' <summary>
        ''' 設定マスタで有効になっていない区分のデータが入力されています。
        ''' </summary>
        Public Const MSG_CANNOT_USE_BY_SETTING As String = "WM990055"

        ''' <summary>
        ''' 貸出回数範囲エラー。
        ''' </summary>
        Public Const MSG_ERR_LEASECOUNT As String = "WM990056"

        ''' <summary>
        ''' 想定外のエラーで取込失敗しました。
        ''' </summary>
        Public Const MSG_ERR_SOUTEIGAI As String = "WM990057"

        ''' <summary>
        ''' 登録方法[2]のカテゴリーを選択しているにも関わらず、資産IDが入力されています。
        ''' </summary>
        Public Const MSG_ERR_HOUHOU2_INPUT As String = "WM990058"

        ''' <summary>
        ''' 登録方法[3]のカテゴリーを選択しているにも関わらず、資産IDが入力されています。
        ''' </summary>
        Public Const MSG_ERR_HOUHOU3_INPUT As String = "WM990059"

        ''' <summary>
        ''' 登録方法[1]のカテゴリーを選択しているにも関わらず、資産IDが入力されています。
        ''' </summary>
        Public Const MSG_ERR_HOUHOU1_INPUT As String = "WM990060"

        ''' <summary>
        ''' 未タグ紐付けの資産が有ります
        ''' </summary>
        Public Const MSG_ASSET_MIHIMODUKEARI As String = "WM990061"

        ''' <summary>
        ''' CSVマッピング明細マスタの項目が不正です。
        ''' </summary>
        Public Const MSG_ERR_CSVMAPMEI As String = "WM990062"

        ''' <summary>
        ''' CSVマッピングマスタの文字コードが不正です。
        ''' </summary>
        Public Const MSG_ERR_CHARCD As String = "WM990063"

        ''' <summary>
        ''' 取り込み対象ではない項目を出力します。(ヘッダに*がついている項目が対象外)
        ''' </summary>
        Public Const MSG_ASSET_CSVOUTPUT As String = "WM990064"

        ''' <summary>
        ''' タイムアウトになりました。再度ライセンス認証チェックを行います。
        ''' </summary>
        Public Const MSG_ERR_LICENSE_SESSION As String = "WM990065"

        ''' <summary>
        ''' 取込み対象ではない項目を取込みします。(ヘッダに*がついている項目が対象外)
        ''' </summary>
        Public Const MSG_ASSET_CSVINPUT As String = "WM990068"

        ''' <summary>
        ''' 登録方法[5]のカテゴリーを選択しているにも関わらず、資産IDが入力されています。
        ''' </summary>
        Public Const MSG_ERR_HOUHOU5_INPUT As String = "WM990066"

        ''' <summary>
        ''' 登録方法[6]のカテゴリーを選択しているにも関わらず、資産IDが入力されています。
        ''' </summary>
        Public Const MSG_ERR_HOUHOU6_INPUT As String = "WM990067"

        ''' <summary>
        ''' 事業所コードが不正です。
        ''' </summary>
        Public Const MSG_ERR_COM_CD As String = "WM990069"

        ''' <summary>
        ''' 資産タイプが不正です。
        ''' </summary>
        Public Const MSG_ERR_ASSET_TYPE As String = "WM990070"

        ''' <summary>
        ''' シリアルNoが不正です。
        ''' </summary>
        Public Const MSG_ERR_SERIAL_NO As String = "WM990071"

        ''' <summary>
        ''' データを選択してください。
        ''' </summary>
        Public Const MSG_ERR_DATA_UNSELECTED As String = "WM990072"

        ''' <summary>
        ''' 資産タイプが不正です。タグEPC事業所とタグ資産タイプの桁数の合計が 12桁になるように設定してください。
        ''' </summary>
        Public Const MSG_ERR_ASSET_TYPE_DIGITS As String = "WM990074"

        ''' <summary>
        ''' 所属拠点が重複しています。
        ''' </summary>
        Public Const MSG_ERR_HUBLOCLCD_JUUFUKU As String = "WM990075"

        ''' <summary>
        ''' 貸出状態をチェックしてください。
        ''' </summary>
        Public Const MSG_ERR_LEASE_STATUS As String = "WM990076"

        ''' <summary>
        ''' 管理ロケーションをチェックしてください。
        ''' </summary>
        Public Const MSG_ERR_MNG_LOC As String = "WM990077"

        ''' <summary>
        ''' 出荷通知をチェックしてください。
        ''' </summary>
        Public Const MSG_ERR_SHIPPING_NOTIFICATION As String = "WM990078"

        ''' <summary>
        ''' 更新権限がない資産が選択されています。
        ''' </summary>
        Public Const MSG_ERR_SELECTED_NOT_UPD_ASSET As String = "WM990079"

        ''' <summary>
        ''' 資産状態が「保管中」でない資産は貸出状態を変更できません。
        ''' </summary>
        Public Const MSG_ERR_NOT_UPD_LEASE_STATUS As String = "WM990080"

        ''' <summary>
        ''' 異なる管理ロケーションの資産が選択されています。
        ''' </summary>
        Public Const MSG_ERR_DIFFERENT_MNG_LOC As String = "WM990081"

        ''' <summary>
        ''' カゴに追加できる資産は、最大{0}件です。
        ''' </summary>
        Public Const MSG_ERR_CART_OVER_NUMBER As String = "WM990082"

        ''' <summary>
        ''' 資産が選択されていません。
        ''' </summary>
        Public Const MSG_ERR_ASSET_NOT_SELECTED As String = "WM990083"

        ''' <summary>
        ''' メンテナンス開始日が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_MAINTE_START_DT_EMPTY As String = "WM990084"

        ''' <summary>
        ''' メンテナンス開始日が過去日になっています。
        ''' </summary>
        Public Const MSG_ERR_MAINTE_START_DT_KAKOBI As String = "WM990085"

        ''' <summary>
        ''' メンテナンス終了日が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_MAINTE_END_DT_EMPTY As String = "WM990086"

        ''' <summary>
        ''' メンテナンス終了日がメンテナンス開始日より過去の日付です。
        ''' </summary>
        Public Const MSG_ERR_MAINTE_END_DT_KAKOBI As String = "WM990087"

        ''' <summary>
        ''' 他の予約またはメンテナンス期間と重複しています。
        ''' </summary>
        Public Const MSG_ERR_MAINTE_DT_JUUFUKU As String = "WM990088"

        ''' <summary>
        ''' 送り状を作成します。よろしいですか？
        ''' </summary>
        Public Const MSG_INFO_INVOICE_CONFIRM As String = "WM990089"

        ''' <summary>
        ''' 貸出確認書を作成します。よろしいですか？
        ''' </summary>
        Public Const MSG_INFO_LENDING_CONFIRM As String = "WM990090"

        ''' <summary>
        ''' 100件以上選択されています。貸出確認書の作成に時間がかかりますが、よろしいですか？
        ''' </summary>
        Public Const MSG_INFO_LENDING_CONFIRM_TAKE_TIME As String = "WM990091"

        ''' <summary>
        ''' 実質貸出開始日が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_REAL_START_DT_EMPTY As String = "WM990092"

        ''' <summary>
        ''' 実質貸出開始日が過去日になっています。
        ''' </summary>
        Public Const MSG_ERR_REAL_START_DT_KAKOBI As String = "WM990093"

        ''' <summary>
        ''' 実質貸出終了日が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_REAL_END_DT_EMPTY As String = "WM990094"

        ''' <summary>
        ''' 実質貸出終了日が実質貸出開始日より過去の日付です。
        ''' </summary>
        Public Const MSG_ERR_REAL_END_DT_KAKOBI As String = "WM990095"

        ''' <summary>
        ''' 実質貸出期間が長すぎます。(貸出可能期間:{0})
        ''' </summary>
        Public Const MSG_ERR_REAL_DT_TOO_LONG As String = "WM990096"

        ''' <summary>
        ''' 資産番号:{0}が、他の予約またはメンテナンス期間と重複しています。
        ''' </summary>
        Public Const MSG_ERR_YOYAKU_DT_JUUFUKU As String = "WM990097"

        ''' <summary>
        ''' 期間のFromもしくはToを入力してください。
        ''' </summary>
        Public Const MSG_ERR_ENTERED_PERIOD_FROM_OR_TO As String = "WM990098"

        ''' <summary>
        ''' 受渡日を入力してください。
        ''' </summary>
        Public Const MSG_ERR_REQ_WATASHI_DAY As String = "WM990099"

        ''' <summary>
        ''' 担当者を入力してください。
        ''' </summary>
        Public Const MSG_ERR_REQ_TANTO As String = "WM990100"

        ''' <summary>
        ''' 貸出中以外の資産が選択されています。
        ''' </summary>
        Public Const MSG_ERR_SELECTED_NOT_STATUS_LENDING As String = "WM990101"

        ''' <summary>
        ''' 未貸出以外の資産が選択されています。
        ''' </summary>
        Public Const MSG_ERR_SELECTED_NOT_STATUS_MI As String = "WM990102"

        ''' <summary>
        ''' 既に同じ資産番号が追加されています。
        ''' </summary>
        Public Const MSG_ERR_SELECTED_SAME_ASSET_NO As String = "WM990103"

        ''' <summary>
        ''' 入力された資産は貸出不可状態です。
        ''' </summary>
        Public Const MSG_ERR_STATUS_FUKA As String = "WM990104"

        ''' <summary>
        ''' 資産の予約上限数に達しています。
        ''' </summary>
        Public Const MSG_ERR_OVER_MAX_RESERVE_SUU As String = "WM990105"

        ''' <summary>
        ''' 資産番号:{0}の、次の予約がありません。
        ''' </summary>
        Public Const MSG_ERR_NO_NEXT_RESERVE As String = "WM990106"

        ''' <summary>
        ''' 資産番号:{0}の、次の予約の担当者が違います。
        ''' </summary>
        Public Const MSG_ERR_NOT_SAME_TANTO_NEXT_RESERVE As String = "WM990107"

        ''' <summary>
        ''' 受渡日が不正です。
        ''' </summary>
        Public Const MSG_ERR_UKEWATASHI_DAY As String = "WM990108"

        ''' <summary>
        ''' 個別に期間変更した資産が存在するため、まとめて予約全体の期間を変更できません。
        ''' </summary>
        Public Const MSG_ERR_KIKAN_ENCHO_KOBETSU As String = "WM990109"

        ''' <summary>
        ''' 直接引継した資産が存在するため、まとめて予約全体の期間を変更できません。
        ''' </summary>
        Public Const MSG_ERR_KIKAN_ENCHO_HIKI As String = "WM990110"

        ''' <summary>
        ''' 返却した資産が存在するため、まとめて予約全体の期間を変更できません。
        ''' </summary>
        Public Const MSG_ERR_KIKAN_ENCHO_HENKYAKU As String = "WM990111"

        ''' <summary>
        ''' 期間延長できる最大回数を超えました。
        ''' </summary>
        Public Const MSG_ERR_KIKAN_ENCHO_OVER As String = "WM990112"

        ''' <summary>
        ''' 返却済の資産が選択されています。
        ''' </summary>
        Public Const MSG_ERR_SELECTED_STATUS_RETURNED As String = "WM990113"

        ''' <summary>
        ''' 引継されている資産が選択されています。
        ''' </summary>
        Public Const MSG_ERR_SELECTED_STATUS_HIKITSUGI As String = "WM990114"

        ''' <summary>
        ''' 貸出します。よろしいですか？
        ''' </summary>
        Public Const LENDING_CONFIRM As String = "WM990115"

        ''' <summary>
        ''' 返却します。よろしいですか？
        ''' </summary>
        Public Const RETURNED_CONFIRM As String = "WM990116"

        ''' <summary>
        ''' 貸出処理が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_LENDING As String = "WM990117"

        ''' <summary>
        ''' 返却処理が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_RETURNED As String = "WM990118"

        ''' <summary>
        ''' WebAPIの応答がありません。
        ''' </summary>
        Public Const MSG_ERR_API_NO_RESPONSE As String = "WM990119"

        ''' <summary>
        ''' 貸出上限数を超えたため貸出できなかった資産があります。
        ''' </summary>
        Public Const MSG_ERR_API_JOGEN As String = "WM990120"

        ''' <summary>
        ''' 貸出上限数を超えた資産がありました。
        ''' </summary>
        Public Const MSG_ERR_API_JOGEN_KEIKOKU As String = "WM990121"

        ''' <summary>
        ''' マスタが最新ではありません。マスタを再取り込みしてください。
        ''' </summary>
        Public Const MSG_ERR_API_MASTER_OLD As String = "WM990122"

        ''' <summary>
        ''' 予約されていないため貸出できなかった資産があります。
        ''' </summary>
        Public Const MSG_ERR_API_NOT_RESERVED As String = "WM990123"

        ''' <summary>
        ''' WebAPIから異常応答がありました。
        ''' </summary>
        Public Const MSG_ERR_API_IJOU As String = "WM990124"

        ''' <summary>
        ''' 追加する資産番号を入力して下さい。
        ''' </summary>
        Public Const MSG_ERR_REQ_ADD_ASSET_NO As String = "WM990125"

        ''' <summary>
        ''' 明細がありません。
        ''' </summary>
        Public Const MSG_ERR_MEISAI_EMPTY As String = "WM990126"

        ''' <summary>
        ''' メンテナンスが必要なため、予約できません。
        ''' </summary>
        Public Const MSG_ERR_MUST_MAINTE As String = "WM990127"

        ''' <summary>
        ''' 日数=０で保存はできません。
        ''' </summary>
        Public Const MSG_ERR_NISSU_ZERO As String = "WM990128"

        ''' <summary>
        ''' 所属拠点が違う資産です。
        ''' </summary>
        Public Const MSG_ERR_ASSET_MNG_LOC_L_CD As String = "WM990129"

        ''' <summary>
        ''' マイナス値が入力されています。
        ''' </summary>
        Public Const MSG_ERR_NEGATIVE_VALUE As String = "WM990130"

        ''' <summary>
        ''' 非稼働日が設定されています。
        ''' </summary>
        Public Const MSG_ERR_NOT_WORK_FRG_YES As String = "WM990131"

        ''' <summary>
        ''' 予約ステータスが「未貸出」以外の明細があります。
        ''' </summary>
        Public Const MSG_ERR_EXIST_NOT_RESERVE_STATUS_MI As String = "WM990132"

        ''' <summary>
        ''' 取込を行うとすべてのデータが削除されてから登録されます。
        ''' </summary>
        Public Const MSG_INFO_CSV_IMPORT_ALL_DATA_DELETE As String = "WM990133"

        ''' <summary>
        ''' 表示開始日が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_DSP_START_DT_EMPTY As String = "WM990134"

        ''' <summary>
        ''' 受渡日が実貸出開始日と同じです。
        ''' </summary>
        Public Const MSG_ERR_SAME_WATASHI_DAY As String = "WM990135"

        ''' <summary>
        ''' この日付は実質貸出開始日に指定できません。未来の日付を指定してください。
        ''' </summary>
        Public Const MSG_ERR_REAL_START_DATE_LT As String = "WM990136"

        ''' <summary>
        ''' 設定外の日付です。
        ''' </summary>
        Public Const MSG_ERR_DATE_SETTEIGAI As String = "WM990137"

        ''' <summary>
        ''' 自拠点の資産ではないためメンテナンスを編集できません。
        ''' </summary>
        Public Const MSG_ERR_MAINTE_NOT_JIKYOTEN As String = "WM990138"

        ''' <summary>
        ''' 資産状態が「保管中」「貸出中」以外の資産です。
        ''' </summary>
        Public Const MSG_ERR_INPUT_ASSET_STATUS As String = "WM990139"

        ''' <summary>
        ''' 引継担当者が異なる資産が選択されています。
        ''' </summary>
        Public Const MSG_ERR_SELECTED_DEF_HIKITSUGI_EMPLOYEE As String = "WM990140"

        ''' <summary>
        ''' 同じ資産番号でかつ過去の未貸出の予約が存在するため貸出できません。
        ''' </summary>
        Public Const MSG_ERR_EXITS_BEFORE_RESERVE_STATUS_MI As String = "WM990141"

        '2022/7/25 sv sato add start
        ''' <summary>
        ''' 予約番号が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_RESERVE_ID As String = "WM990142"

        ''' <summary>
        ''' 予約番号が重複しています。
        ''' </summary>
        Public Const MSG_ERR_EXITS_RESERVE_ID As String = "WM990143"

        ''' <summary>
        ''' 送り状番号が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_DEST_NO As String = "WM990144"

        ''' <summary>
        ''' 申請日が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_REQUEST_DT As String = "WM990145"

        ''' <summary>
        ''' 申請社員コードが入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_REQ_EMP_CD As String = "WM990146"

        ''' <summary>
        ''' 担当社員コードが入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_CHARGE_EMP_CD As String = "WM990147"

        ''' <summary>
        ''' 貸出先コードが入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_LEASE_PLACE_CD As String = "WM990148"

        ''' <summary>
        ''' 貸出用途が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_LEASE_USE As String = "WM990149"

        ''' <summary>
        ''' 管理ロケ：{0}と資産の管理ロケ：{1}が異なります。
        ''' </summary>
        Public Const MSG_ERR_NOT_ASSET_MNG_LOC_CD As String = "WM990150"

        ''' <summary>
        ''' 送り先名が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_DEST_NAME As String = "WM990151"

        ''' <summary>
        ''' 送り先住所が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_DEST_ADDRESS As String = "WM990152"

        ''' <summary>
        ''' 文字数が不足しています。
        ''' </summary>
        Public Const MSG_ERR_SHORTAGE_STR_LEN As String = "WM990153"

        ''' <summary>
        ''' 区分値が不正です。
        ''' </summary>
        Public Const MSG_ERR_DIV_VAL As String = "WM990154"

        ''' <summary>
        ''' データの登録に失敗しました。
        ''' </summary>
        Public Const MSG_ERR_INSERT_FAILURE As String = "WM990155"

        ''' <summary>
        ''' データの更新に失敗しました。
        ''' </summary>
        Public Const MSG_ERR_UPDATE_FAILURE As String = "WM990156"

        ''' <summary>
        ''' データの削除に失敗しました。
        ''' </summary>
        Public Const MSG_ERR_DELETE_FAILURE As String = "WM990157"

        ''' <summary>
        ''' 予約データが存在しません。
        ''' </summary>
        Public Const MSG_ERR_NOT_EXITS_RESERVE_ID As String = "WM990158"

        ''' <summary>
        ''' 資産番号が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_ASSET_NO As String = "WM990159"

        ''' <summary>
        ''' マスタに存在しないコードです。
        ''' </summary>
        Public Const MSG_ERR_NOT_EXITS_ASSET_NO As String = "WM990160"

        ''' <summary>
        ''' 入力された資産は貸出不可状態です。　資産番号：{0}
        ''' </summary>
        Public Const MSG_ERR_FORMAT_STATUS_FUKA As String = "WM990161"

        ''' <summary>
        ''' 資産状態が「保管中」「貸出中」以外の資産です。　資産番号：{0}
        ''' </summary>
        Public Const MSG_ERR_FORMAT_INPUT_ASSET_STATUS As String = "WM990162"
        '2022/7/25 sv sato add end

        '2022/9/13 Add Start Kawamoto
        ''' <summary>
        ''' 資産番号は半角英数と一部記号(#$*+-.:;=<>_)しか使用できません。
        ''' </summary>
        Public Const MSG_ERR_DATASEIYAKU_ASSET_NO As String = "WM990163"
        '2022/9/13 Add End Kawamoto

        ''' <summary>
        ''' 返却済みの予約の削除はスキップしました。 2022/7/25 sv sato add
        ''' </summary>
        Public Const MSG_ERR_RETRUN_RESERVE_SKIP As String = "WM990164"

        ''' <summary>
        ''' 貸出中の予約の削除はスキップしました。 2022/7/25 sv sato add
        ''' </summary>
        Public Const MSG_ERR_LENDING_RESERVE_SKIP As String = "WM990165"

        ''' <summary>
        ''' 返却済みの予約の更新はスキップしました。 2022/7/25 sv sato add
        ''' </summary>
        Public Const MSG_ERR_HENKYAKUZUMI_RESERVE_SKIP As String = "WM990166"

        ''' <summary>
        ''' 実質貸出期間外のため、貸出できなかった資産があります。
        ''' </summary>
        Public Const MSG_ERR_API_DATE_OUT_RESERVED As String = "WM990167"

        '2023/02/08 Kawamoto ADD START
        ''' <summary>
        ''' ロケ{0}の登録件数が{1}件になります。登録上限数{2}件を超えたため登録できません。
        ''' </summary>
        Public Const MSG_ERR_EXCEED_MAX_NUM As String = "WM990168"

        '2023/02/08 Kawamoto ADD END
        '2023/02/13 ADD Shirakawa
        ''' <summary>
        ''' ロケの登録上限数を超えたためロケを変更できません。
        ''' </summary>
        Public Const MSG_ERR_LOC_LIMIT_OVER As String = "WM990169"

        '2023/02/08 Kawamoto ADD START
        ''' <summary>
        ''' 納品日の前後入力エラーです。
        ''' </summary>
        Public Const MSG_ERR_FROM_TO_FREEITEM13 As String = "WM990170"

        ''' <summary>
        ''' 登録/更新権限のない拠点です。
        ''' </summary>
        Public Const MSG_ERR_NOT_AUTH_LOCATION As String = "WM990171"

        ''' <summary>
        ''' 内容量(kg)の前後入力エラーです。
        ''' </summary>
        Public Const MSG_ERR_FROM_TO_FREEITEM10 As String = "WM990172"

        ''' <summary>
        ''' 内容量(L)の前後入力エラーです。
        ''' </summary>
        Public Const MSG_ERR_FROM_TO_FREEITEM11 As String = "WM990173"

        ''' <summary>
        ''' 比重の前後入力エラーです。
        ''' </summary>
        Public Const MSG_ERR_FROM_TO_FREEITEM12 As String = "WM990174"

        ''' <summary>
        ''' 受入時の容器を含む重量(kg)の前後入力エラーです。。
        ''' </summary>
        Public Const MSG_ERR_FROM_TO_FREEITEM14 As String = "WM990175"

        ''' <summary>
        ''' 残量(NET重量)の前後入力エラーです。
        ''' </summary>
        Public Const MSG_ERR_FROM_TO_FREEITEM16 As String = "WM990176"

        ''' <summary>
        ''' 残量割合(残量/内容量)の前後入力エラーです。
        ''' </summary>
        Public Const MSG_ERR_FROM_TO_FREEITEM17 As String = "WM990177"

        ''' <summary>
        ''' 危険物の指定数量の単位が"kg"の為、内容量(L)には入力できません。
        ''' </summary>
        Public Const MSG_ERR_UNIT_MISMATCH_FREEITEM11 As String = "WM990178"

        ''' <summary>
        ''' 危険物の指定数量の単位が"L"の為、内容量(kg)には入力できません。
        ''' </summary>
        Public Const MSG_ERR_UNIT_MISMATCH_FREEITEM10 As String = "WM990179"

        ''' <summary>
        ''' 0～99999.999の範囲で入力してください。
        ''' </summary>
        Public Const MSG_ERR_NUMERIC_RANGE As String = "WM990180"

        ''' <summary>
        ''' 小数点以下3桁までで入力してください。
        ''' </summary>
        Public Const MSG_ERR_DIGITS_RANGE As String = "WM990196"

        ''' <summary>
        ''' 0以上の値を入力してください。
        ''' </summary>
        Public Const MSG_ERR_NUMERIC_PLUS As String = "WM990197"

        ''' <summary>
        ''' 数値を入力してください。
        ''' </summary>
        Public Const MSG_ERR_NUMERIC As String = "WM990199"

        ''' <summary>
        ''' 資産番号が存在しません。
        ''' </summary>
        Public Const MSG_ERR_NOT_EXISTS_ASSET_NO As String = "WM990181"

        ''' <summary>
        ''' 新規登録の場合、貸出状態は「可能」で登録できません。
        ''' </summary>
        Public Const MSG_ERR_NOT_REGIST_LEASE_STATUS As String = "WM990182"
        '2023/02/08 Kawamoto ADD END

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' ファイルをドロップ＆ドラッグしてください　またはファイル追加（Shiftキーを押しながらファイルを複数選択可能）
        ''' </summary>
        Public Const MSG_FILE_SELECTION As String = "WM990183"

        ''' <summary>
        ''' 貸出確認書宛名が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_TO_DEST_NAME As String = "WM990184"

        ''' <summary>
        ''' 返送先(貸出先コード)が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_RET_LEASE_PLACE_CD As String = "WM990185"

        ''' <summary>
        ''' 返送先住所1が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_RET_ADDRESS1 As String = "WM990186"

        ''' <summary>
        ''' 返送先郵便番号が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_RET_ZIP_CODE As String = "WM990187"

        '2023/03/22 Kawamoto ADD END

        '2023/04/30 Sato ADD START
        ''' <summary>
        ''' 出力可能なデータが存在しません。
        ''' </summary>
        Public Const MSG_ERR_NOT_EXISTS_OUT_PUT_DATA As String = "WM990188"

        ''' <summary>
        ''' 保管場所が入力されていません。
        ''' </summary>
        Public Const MSG_ERR_NOT_INPUT_STORAGE_LOC As String = "WM990189"

        ''' <summary>
        ''' ({0}が必須)
        ''' </summary>
        Public Const MSG_ERR_REQUIRED_LESS_THAN_VALUE As String = "WM990191"
        '2023/04/30 Sato ADD END

        '2023/05/10 Kawamoto ADD START
        ''' <summary>
        ''' 計量器連携されているため、残量(NET重量)は変更できません。
        ''' </summary>
        Public Const MSG_ERR_NOT_CHANGE_REMAINING_AMOUNT As String = "WM990190"
        '2023/05/10 Kawamoto ADD END

        '2023/04/30 Sato ADD START
        ''' <summary>
        ''' 受け入れ
        ''' </summary>
        Public Const MSG_ERR_ACCEPT As String = "WM990192"

        ''' <summary>
        ''' 在庫量確認（{0}g）
        ''' </summary>
        Public Const MSG_ERR_STOCK_QTY_CHECK_G As String = "WM990193"

        ''' <summary>
        ''' ファイルサイズが上限を超えています。(上限 {0}kbyte)
        ''' </summary>
        Public Const MSG_ERR_FILE_MAX_SIZE_OVER As String = "WM990194"

        ''' <summary>
        ''' 内容量(kg)と内容量(L)はどちらか片方を入力してください。
        ''' </summary>
        Public Const MSG_ERR_KG_OR_L_ONE_SIDE_INPUT As String = "WM990195"
        '2023/04/30 Sato ADD END

        '20230904 ADD Start Shirakawa
        ''' <summary>
        ''' 検索対象のデータが{0}件あります。検索条件を見直して、再度検索してください。(最大{1}件)
        ''' </summary>
        Public Const MSG_ERR_LIMIT_DATA As String = "WM990201"
        '20230904 ADD End Shirakawa

        ''' <summary>
        ''' 半角で入力してください。
        ''' </summary>
        Public Const MSG_ERR_INPUT_HANKAKU As String = "WM990202"

        ''' <summary>
        ''' 整数を入力してください。
        ''' </summary>
        Public Const MSG_ERR_INPUT_INTEGER As String = "WM990203"

        ''' <summary>
        ''' 1以上を入力してください。
        ''' </summary>
        Public Const MSG_ERR_LOWER_LIMIT_1 As String = "WM990204"

        ''' <summary>
        ''' 資産番号は自動生成されるため入力できません。
        ''' </summary>
        Public Const MSG_ERR_AUTO_GENERATE_NOT_INPUT As String = "WM990205"

        ''' <summary>
        ''' 資産番号の自動生成上限に達しています。
        ''' </summary>
        Public Const MSG_ERR_AUTO_GENERATE_MAX_SIZE As String = "WM990206"

        ''' <summary>
        ''' 登録済みの資産IDです。
        ''' </summary>
        Public Const MSG_ERR_REGISTED_ASSET_ID As String = "WM990208"

        ''' <summary>
        ''' 解除します。よろしいですか？
        ''' </summary>
        Public Const MSG_RELEASE_CONFIRM As String = "WM990209"

        ''' <summary>
        ''' 工事番号は自動生成されるため入力できません。
        ''' </summary>
        Public Const MSG_ERR_CONST_NO_AUTO_GENERATE_NOT_INPUT As String = "WM990210"

        ''' <summary>
        ''' 工事終了日が工事開始日より過去の日付です。
        ''' </summary>
        Public Const MSG_ERR_CONST_END_DT_KAKOBI As String = "WM990211"

        ''' <summary>
        ''' 工事貸出開始日が工事開始日より未来の日付です。
        ''' </summary>
        Public Const MSG_ERR_CONST_LENT_START_DT_MIRAIBI As String = "WM990212"

        ''' <summary>
        ''' 工事貸出終了日が工事貸出開始日より過去の日付です。
        ''' </summary>
        Public Const MSG_ERR_CONST_LENT_END_DT_KAKOBI As String = "WM990213"

        ''' <summary>
        ''' 工事貸出終了日が工事終了日より過去の日付です。
        ''' </summary>
        Public Const MSG_ERR_CONST_LENT_END_DT_KAKOBI_CONST_END_DT As String = "WM990214"

        ''' <summary>
        ''' 工事番号が存在しません。
        ''' </summary>
        Public Const MSG_ERR_NOT_EXISTS_CONST_NO As String = "WM990215"

        ''' <summary>
        ''' 不正な日付です。
        ''' </summary>
        Public Const MSG_ERR_INCORRECT_DATE As String = "WM990219"

        ''' <summary>
        ''' {0}と重複しています。登録しますか？
        ''' </summary>
        Public Const MSG_INS_CONFIRM_WARN_MAINTE_KBN As String = "WM990220"

        ''' <summary>
        ''' {0}と重複しています。変更しますか？
        ''' </summary>
        Public Const MSG_UPD_CONFIRM_WARN_MAINTE_KBN As String = "WM990221"

        ''' <summary>
        ''' 警告のメンテナンスデータが｛0}件あります。登録してよろしいですか？
        ''' </summary>
        Public Const MSG_INS_CONFIRM_WARN_MAINTE_NUM As String = "WM990222"

        ''' <summary>
        ''' 警告のメンテナンスデータが｛0}件あります。変更してよろしいですか？
        ''' </summary>
        Public Const MSG_UPD_CONFIRM_WARN_MAINTE_NUM As String = "WM990223"

        ''' <summary>
        ''' 選択した資産のラベル発行を不要とします。よろしいですか？
        ''' </summary>
        Public Const MSG_LABEL_NOT_REQUIRED_CONFIRM As String = "WM990233"

        ''' <summary>
        ''' ラベル発行不要登録が完了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_LABEL_NOT_REQUIRED As String = "WM990234"

        ''' <summary>
        ''' ラベル発行指示が完了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_LABEL_PRINT As String = "WM990235"

        ''' <summary>
        ''' 会社コードが検索条件と一致しません。
        ''' </summary>
        Public Const MSG_ERR_COMPANY_MISMATCH_SEARCH_CONDITION As String = "WM990236"

        ''' <summary>
        ''' Excel取込でエラーが発生しました。
        ''' </summary>
        Public Const MSG_ERR_IMPORT_ERR As String = "WM990237"

        ''' <summary>
        ''' Excelファイルを選択して下さい。
        ''' </summary>
        Public Const MSG_ERR_NOTEXCEL As String = "WM990238"

        ''' <summary>
        ''' 棚卸実績ファイルを出力します。よろしいですか？
        ''' </summary>
        Public Const MSG_INVENTRY_RESULT_OUTPUT As String = "WM990239"

        ''' <summary>
        ''' Excel取込のDB操作時に予期しないエラーが発生しました。
        ''' </summary>
        Public Const MSG_ERR_IMPORT_SP_UNEXPECTED_ERR As String = "WM990241"

        ''' <summary>
        ''' Excel取込のDB操作時にパラメーターエラーが発生しました。
        ''' </summary>
        Public Const MSG_ERR_IMPORT_SP_PARAM_ERR As String = "WM990242"

        ''' <summary>
        ''' Excel取込でDB操作時にロケマスタエラーが発生しました。
        ''' </summary>
        Public Const MSG_ERR_IMPORT_SP_LOC_ERR As String = "WM990243"

        ''' <summary>
        ''' Excel取込でDB操作時に担当者不明エラーが発生しました。
        ''' </summary>
        Public Const MSG_ERR_IMPORT_SP_EMPLOYEE_ERR As String = "WM990244"

        ''' <summary>
        ''' 会社コードが更新可能拠点ではありません。
        ''' </summary>
        Public Const MSG_ERR_COMPANY_NOT_UPDATABLE As String = "WM990246"

        ''' <summary>
        ''' 資産台帳取込時に資産が{0}件削除されます。よろしいですか？
        ''' </summary>
        ''' <remarks>{0}には削除対象の件数が入る。設定M(KEY種別=Asset_Delete_AlertCount)のDATA1を超えた場合に表示する。</remarks>
        Public Const MSG_CONF_ASSET_IMPORT_DELETE As String = "WM990249"

        ''' <summary>
        ''' {0}の開始日は終了日以前を指定してください。
        ''' </summary>
        ''' <remarks>{0}には項目名（文言マスタの項目名文言）が入る。日付範囲が3組ある資産一覧で、どの項目が前後逆転しているかを示すために使用する（不足情報 No.18）。</remarks>
        Public Const MSG_ERR_DATE_ORDER_ITEM As String = "WM990250"

        ''' <summary>
        ''' 更新先の資産データに更新可能拠点以外が存在します。
        ''' </summary>
        Public Const MSG_ERR_EXIST_ASSET_COMPANY_NOT_UPDATABLE As String = "WM990247"

        ''' <summary>
        ''' DB接続エラーです。
        ''' </summary>
        Public Const MSG_ERR_DB_CONNECT As String = "WM999998"

        ''' <summary>
        ''' 予期せぬエラーが発生しました。
        ''' </summary>
        Public Const MSG_ERR_SYSTEM As String = "WM999999"

        '20251020 ADD Start Kudo
        ''' <summary>
        ''' ロケーション(会社)を追加する権限がありません。 
        ''' </summary>
        Public Const MSG_ERR_ADDKENGEN As String = "WM990240"
        '20251020 ADD  END Start Kudo

        '20240417 ADD Start Tomatsuri
        ''' <summary>
        ''' 除却
        ''' </summary>
        Public Const LABEL_DISPOSE As String = "WI990500"
        '20240417 ADD End Tomatsuri

        ''' <summary>
        ''' ▼検索条件
        ''' </summary>
        Public Const LABEL_SEARCH_CONDITION_OPEN As String = "WI990503"

        ''' <summary>
        ''' ▲検索条件
        ''' </summary>
        Public Const LABEL_SEARCH_CONDITION_CLOSE As String = "WI990504"

        ''' <summary>
        ''' 仮置き日
        ''' </summary>
        Public Const LABEL_TEMP_DT As String = "WI990505"

        ''' <summary>
        ''' 作業内容
        ''' </summary>
        Public Const LABEL_WORK_CONTENTS As String = "WI990506"

        ''' <summary>
        ''' 仮置き場所
        ''' </summary>
        Public Const LABEL_TEMP_STORAGE_PLACE As String = "WI990507"

        ''' <summary>
        ''' 品名
        ''' </summary>
        Public Const LABEL_PRODUCT_NAME As String = "WI990508"

        ''' <summary>
        ''' 予約数
        ''' </summary>
        Public Const LABEL_RESERVATION_NUM As String = "WI990509"

        ''' <summary>
        ''' 予約可能数
        ''' </summary>
        Public Const LABEL_AVAILABLE_RESERVATION_NUM As String = "WI990510"

        ''' <summary>
        ''' 仮置き紐付状態
        ''' </summary>
        Public Const LABEL_REL_CONST_STATUS As String = "WI990511"

        ''' <summary>
        ''' 紐付工事番号
        ''' </summary>
        Public Const LABEL_REL_CONST_NO As String = "WI990512"

        ''' <summary>
        ''' 登録工事番号
        ''' </summary>
        Public Const LABEL_REG_CONST_NO As String = "WI990513"

        ''' <summary>
        ''' 仮置き情報1
        ''' </summary>
        Public Const LABEL_TEMP_INFO1 As String = "WI990514"

        ''' <summary>
        ''' 仮置き情報2
        ''' </summary>
        Public Const LABEL_TEMP_INFO2 As String = "WI990515"

        ''' <summary>
        ''' 仮置き情報3
        ''' </summary>
        Public Const LABEL_TEMP_INFO3 As String = "WI990516"

        ''' <summary>
        ''' 仮置き情報4
        ''' </summary>
        Public Const LABEL_TEMP_INFO4 As String = "WI990517"

        ''' <summary>
        ''' 仮置き情報5
        ''' </summary>
        Public Const LABEL_TEMP_INFO5 As String = "WI990518"

        ''' <summary>
        ''' 仮置き備考
        ''' </summary>
        Public Const LABEL_TEMP_REMARK As String = "WI990519"

        ''' <summary>
        ''' 仮置き数
        ''' </summary>
        Public Const LABEL_TEMP_NUM As String = "WI990520"

        ''' <summary>
        ''' 仮置きID
        ''' </summary>
        Public Const LABEL_TEMP_ID As String = "WI990521"

        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Public Const LABEL_CONST_NO As String = "WI990524"

        ''' <summary>
        ''' 工事開始日
        ''' </summary>
        Public Const LABEL_CONST_START_DT As String = "WI990525"

        ''' <summary>
        ''' 工事終了日
        ''' </summary>
        Public Const LABEL_CONST_END_DT As String = "WI990526"

        ''' <summary>
        ''' 工事貸出開始日
        ''' </summary>
        Public Const LABEL_CONST_LENT_START_DT As String = "WI990527"

        ''' <summary>
        ''' 工事貸出終了日
        ''' </summary>
        Public Const LABEL_CONST_LENT_END_DT As String = "WI990528"

        ''' <summary>
        ''' 工事状態
        ''' </summary>
        Public Const LABEL_CONST_STATUS As String = "WI990529"

        ''' <summary>
        ''' 工事内容
        ''' </summary>
        Public Const LABEL_CONST_CONTENTS As String = "WI990530"

        ''' <summary>
        ''' 工事責任者氏名
        ''' </summary>
        Public Const LABEL_CONST_EMPLOYEE_NAME As String = "WI990531"

        ''' <summary>
        ''' 拠点ロケ
        ''' </summary>
        Public Const LABEL_KYOTEN_CD_NAME As String = "WI990532"

        ''' <summary>
        ''' 工事情報1
        ''' </summary>
        Public Const LABEL_CONST_INFO1 As String = "WI990533"

        ''' <summary>
        ''' 工事情報2
        ''' </summary>
        Public Const LABEL_CONST_INFO2 As String = "WI990534"

        ''' <summary>
        ''' 工事情報3
        ''' </summary>
        Public Const LABEL_CONST_INFO3 As String = "WI990535"

        ''' <summary>
        ''' 工事情報4
        ''' </summary>
        Public Const LABEL_CONST_INFO4 As String = "WI990536"

        ''' <summary>
        ''' 工事情報5
        ''' </summary>
        Public Const LABEL_CONST_INFO5 As String = "WI990537"

        ''' <summary>
        ''' 工事情報6
        ''' </summary>
        Public Const LABEL_CONST_INFO6 As String = "WI990538"

        ''' <summary>
        ''' 工事情報7
        ''' </summary>
        Public Const LABEL_CONST_INFO7 As String = "WI990539"

        ''' <summary>
        ''' 工事情報8
        ''' </summary>
        Public Const LABEL_CONST_INFO8 As String = "WI990540"

        ''' <summary>
        ''' 工事情報9
        ''' </summary>
        Public Const LABEL_CONST_INFO9 As String = "WI990541"

        ''' <summary>
        ''' 工事情報10
        ''' </summary>
        Public Const LABEL_CONST_INFO10 As String = "WI990542"

        ''' <summary>
        ''' 工事情報11
        ''' </summary>
        Public Const LABEL_CONST_INFO11 As String = "WI990543"

        ''' <summary>
        ''' 工事情報12
        ''' </summary>
        Public Const LABEL_CONST_INFO12 As String = "WI990544"

        ''' <summary>
        ''' 工事情報13
        ''' </summary>
        Public Const LABEL_CONST_INFO13 As String = "WI990545"

        ''' <summary>
        ''' 工事情報14
        ''' </summary>
        Public Const LABEL_CONST_INFO14 As String = "WI990546"

        ''' <summary>
        ''' 工事情報15
        ''' </summary>
        Public Const LABEL_CONST_INFO15 As String = "WI990547"

        ''' <summary>
        ''' 工事情報16
        ''' </summary>
        Public Const LABEL_CONST_INFO16 As String = "WI990548"

        ''' <summary>
        ''' 工事情報17
        ''' </summary>
        Public Const LABEL_CONST_INFO17 As String = "WI990549"

        ''' <summary>
        ''' 工事情報18
        ''' </summary>
        Public Const LABEL_CONST_INFO18 As String = "WI990550"

        ''' <summary>
        ''' 工事情報19
        ''' </summary>
        Public Const LABEL_CONST_INFO19 As String = "WI990551"

        ''' <summary>
        ''' 工事情報20
        ''' </summary>
        Public Const LABEL_CONST_INFO20 As String = "WI990552"

        ''' <summary>
        ''' 前回仮置数
        ''' </summary>
        Public Const LABEL_PREV_TEMP_NUM As String = "WI990553"

        ''' <summary>
        ''' 持込数
        ''' </summary>
        Public Const LABEL_CONST_IN_NUM As String = "WI990554"

        ''' <summary>
        ''' 今回仮置き数
        ''' </summary>
        Public Const LABEL_NOW_TEMP_NUM As String = "WI990555"

        ''' <summary>
        ''' 持帰数
        ''' </summary>
        Public Const LABEL_CONST_OUT_NUM As String = "WI990556"

        ''' <summary>
        ''' 現在貸出数
        ''' </summary>
        Public Const LABEL_CUR_LENT_NUM As String = "WI990557"

        ''' <summary>
        ''' 現在仮置き数
        ''' </summary>
        Public Const LABEL_CUR_TEMP_NUM As String = "WI990558"

        ''' <summary>
        ''' 現場確認者氏名
        ''' </summary>
        Public Const LABEL_SITE_EMPLOYEE_NAME As String = "WI990559"

        ''' <summary>
        ''' 工事完了日時
        ''' </summary>
        Public Const LABEL_CONST_CONFIRM_TS As String = "WI990560"

        ''' <summary>
        ''' 工事完了者氏名
        ''' </summary>
        Public Const LABEL_CONST_CONFIRM_EMPLOYEE_NAME As String = "WI990561"

        ''' <summary>
        ''' 工事完了者備考
        ''' </summary>
        Public Const LABEL_CONST_CONFIRM_REMARK As String = "WI990562"

        ''' <summary>
        ''' 仮置有無
        ''' </summary>
        Public Const LABEL_TEMP_YES_OR_NO As String = "WI990563"

        ''' <summary>
        ''' ロケーション　会社
        ''' </summary>
        Public Const LABEL_LOC_L As String = "WI990694"

        ''' <summary>
        ''' ロケーション　部署
        ''' </summary>
        Public Const LABEL_LOC_M As String = "WI990695"

        ''' <summary>
        ''' ロケーション　場所
        ''' </summary>
        Public Const LABEL_LOC_S As String = "WI990696"

        ''' <summary>
        ''' カテゴリー　大分類
        ''' </summary>
        Public Const LABEL_CAT_L As String = "WI990569"

        ''' <summary>
        ''' カテゴリー　中分類
        ''' </summary>
        Public Const LABEL_CAT_M As String = "WI990570"

        ''' <summary>
        ''' カテゴリー　小分類
        ''' </summary>
        Public Const LABEL_CAT_S As String = "WI990571"

        ''' <summary>
        ''' マスタメンテナンス:RFIDタグ　　2025/10/14 ADD Kudo
        ''' </summary>
        Public Const LABEL_LOC_RFID_FLG As String = "WI990698"

        ''' <summary>
        ''' マスタメンテナンス:運用　　2025/10/14 ADD Kudo
        ''' </summary>
        Public Const VALUE_LOC_RFID_FLG As String = "WI990699"

        ''' <summary>
        ''' 作業日時
        ''' </summary>
        Public Const LABEL_CONST_WORK_TS As String = "WI990574"

        ''' <summary>
        ''' 照合結果
        ''' </summary>
        Public Const LABEL_CONST_MATCH_DIV As String = "WI990575"

        ''' <summary>
        ''' 返却数
        ''' </summary>
        Public Const LABEL_LEASE_NUM As String = "WI990576"

        ''' <summary>
        ''' 仮置き登録数
        ''' </summary>
        Public Const LABEL_TEMP_REG_NUM As String = "WI990577"

        ''' <summary>
        ''' 持込照合数
        ''' </summary>
        Public Const LABEL_CONST_IN_MATCH_NUM As String = "WI990578"

        ''' <summary>
        ''' 持帰照合数
        ''' </summary>
        Public Const LABEL_CONST_OUT_MATCH_NUM As String = "WI990579"

        ''' <summary>
        ''' 仮置き
        ''' </summary>
        Public Const LABEL_TEMP As String = "WI990583"

        ''' <summary>
        ''' 予約者
        ''' </summary>
        Public Const LABEL_RESERVE_EMPLOYEE As String = "WI990584"

        ''' <summary>
        ''' 工事日
        ''' </summary>
        Public Const LABEL_CONST_DT As String = "WI990585"

        ''' <summary>
        ''' 工事責任者
        ''' </summary>
        Public Const LABEL_CONST_EMPLOYEE As String = "WI990586"

        ''' <summary>
        ''' 仮置き情報6
        ''' </summary>
        Public Const LABEL_TEMP_INFO6 As String = "WI990590"

        ''' <summary>
        ''' 仮置き情報7
        ''' </summary>
        Public Const LABEL_TEMP_INFO7 As String = "WI990591"

        ''' <summary>
        ''' サブ資産番号
        ''' </summary>
        Public Const LABEL_SUB_ASSET_NO As String = "WI990593"

        ''' <summary>
        ''' 有形/無形区分
        ''' </summary>
        Public Const LABEL_IS_TANGIBLE As String = "WI990594"

        ''' <summary>
        ''' フリー項目1（資産取得日）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM1 As String = "WI990596"

        ''' <summary>
        ''' フリー項目2（資本化日付）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM2 As String = "WI990597"

        ''' <summary>
        ''' フリー項目3（稼働区分）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM3 As String = "WI990598"

        ''' <summary>
        ''' フリー項目4（稼働区分名）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM4 As String = "WI990599"

        ''' <summary>
        ''' フリー項目5（型式）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM5 As String = "WI990600"

        ''' <summary>
        ''' フリー項目6（管理者社員コード/名）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM6 As String = "WI990601"
        ''' <summary>
        ''' フリー項目6（管理者社員番号）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM6_CD As String = "WI990697"

        ''' <summary>
        ''' フリー項目7（シリアルNO）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM7 As String = "WI990602"

        ''' <summary>
        ''' フリー項目8（WBS番号）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM8 As String = "WI990603"

        ''' <summary>
        ''' フリー項目9（備考）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM9 As String = "WI990604"

        ''' <summary>
        ''' フリー項目10（貸与資産状態）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM10 As String = "WI990605"

        ''' <summary>
        ''' フリー項目11（取得価格）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM11 As String = "WI990606"

        ''' <summary>
        ''' フリー項目12（当月末帳簿価格額）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM12 As String = "WI990607"

        ''' <summary>
        ''' フリー項目13（補助番号）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM13 As String = "WI990608"

        ''' <summary>
        ''' フリー項目14（装置管理番号）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM14 As String = "WI990626"

        ''' <summary>
        ''' フリー項目15（資産グループ）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM15 As String = "WI990627"

        ''' <summary>
        ''' フリー項目16（貸与先）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM16 As String = "WI990628"

        ''' <summary>
        ''' フリー項目17（貸与先名）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM17 As String = "WI990629"

        ''' <summary>
        ''' フリー項目18（開始日）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM18 As String = "WI990630"

        ''' <summary>
        ''' フリー項目19（終了日）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM19 As String = "WI990631"

        ''' <summary>
        ''' フリー項目20（貸与回数）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM20 As String = "WI990632"

        ''' <summary>
        ''' フリー項目21（備考）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM21 As String = "WI990633"

        ''' <summary>
        ''' フリー項目22（会計年度）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM22 As String = "WI990634"

        ''' <summary>
        ''' フリー項目23（会計期間）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM23 As String = "WI990635"

        ''' <summary>
        ''' フリー項目24（テキスト2）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM24 As String = "WI990636"

        ''' <summary>
        ''' フリー項目25（勘定項目）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM25 As String = "WI990637"

        ''' <summary>
        ''' フリー項目26（勘定項目名称）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM26 As String = "WI990638"

        ''' <summary>
        ''' フリー項目27（棚卸ノート）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM27 As String = "WI990639"

        ''' <summary>
        ''' フリー項目28（初回所得日）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM28 As String = "WI990640"

        ''' <summary>
        ''' フリー項目29（数量）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM29 As String = "WI990641"

        ''' <summary>
        ''' フリー項目30（資産番号テキスト）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM30 As String = "WI990642"

        ''' <summary>
        ''' フリー項目31（車両ナンバープレート）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM31 As String = "WI990643"

        ''' <summary>
        ''' フリー項目32（資産休止フラグ）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM32 As String = "WI990644"

        ''' <summary>
        ''' フリー項目33（取得区分）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM33 As String = "WI990645"

        ''' <summary>
        ''' フリー項目34（取得区分名）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM34 As String = "WI990646"

        ''' <summary>
        ''' フリー項目35（資産種類）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM35 As String = "WI990647"

        ''' <summary>
        ''' フリー項目36（資産種類名）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM36 As String = "WI990648"

        ''' <summary>
        ''' フリー項目37（資産リース区分）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM37 As String = "WI990649"

        ''' <summary>
        ''' フリー項目38（市区町村コード）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM38 As String = "WI990650"

        ''' <summary>
        ''' フリー項目39（市区町村コード名）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM39 As String = "WI990651"

        ''' <summary>
        ''' フリー項目40（仕入先）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM40 As String = "WI990652"

        ''' <summary>
        ''' フリー項目41（資産供給者名）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM41 As String = "WI990653"

        ''' <summary>
        ''' フリー項目42（CAPS番号）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM42 As String = "WI990654"

        ''' <summary>
        ''' フリー項目43（稟議#）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM43 As String = "WI990655"

        ''' <summary>
        ''' フリー項目44（リース会社）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM44 As String = "WI990656"

        ''' <summary>
        ''' フリー項目45（リース会社名）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM45 As String = "WI990657"

        ''' <summary>
        ''' フリー項目46（契約番号）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM46 As String = "WI990658"

        ''' <summary>
        ''' フリー項目47（契約日）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM47 As String = "WI990659"

        ''' <summary>
        ''' フリー項目48（リース終了日付）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM48 As String = "WI990660"

        ''' <summary>
        ''' フリー項目49（リース開始日）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM49 As String = "WI990661"

        ''' <summary>
        ''' フリー項目50（リース期間（年））
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM50 As String = "WI990662"

        ''' <summary>
        ''' フリー項目51（リース期間（期））
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM51 As String = "WI990663"

        ''' <summary>
        ''' フリー項目52（リース支払回数）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM52 As String = "WI990664"

        ''' <summary>
        ''' フリー項目53（リース支払）
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM53 As String = "WI990665"

        ''' <summary>
        ''' フリー項目54（年利））
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM54 As String = "WI990666"

        ''' <summary>
        ''' フリー項目55
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM55 As String = "WI990667"

        ''' <summary>
        ''' フリー項目56
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM56 As String = "WI990668"

        ''' <summary>
        ''' フリー項目57
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM57 As String = "WI990669"

        ''' <summary>
        ''' フリー項目58
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM58 As String = "WI990670"

        ''' <summary>
        ''' フリー項目59
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM59 As String = "WI990671"

        ''' <summary>
        ''' フリー項目60
        ''' </summary>
        Public Const LABEL_ASSET_FREE_ITEM60 As String = "WI990672"

        ''' <summary>
        ''' 取込日時
        ''' </summary>
        Public Const LABEL_IP_DATETIME As String = "WI990609"

        ''' <summary>
        ''' 取込ファイル名
        ''' </summary>
        Public Const LABEL_IP_FILE_NAME As String = "WI990610"

        ''' <summary>
        ''' 取込担当者コード/名
        ''' </summary>
        Public Const LABEL_IP_EMPLOYEE_CD As String = "WI990611"

        ''' <summary>
        ''' 管理社員番号
        ''' </summary>
        Public Const LABEL_MANAGER_EMPLOYEE_CD As String = "WI990689"

        ''' <summary>
        ''' 取込担当者コード
        ''' </summary>
        Public Const LABEL_IMPORT_EMPLOYEE_CD As String = "WI990690"

        ''' <summary>
        ''' 取込日
        ''' </summary>
        Public Const LABEL_IP_DATE As String = "WI990691"

        ''' <summary>
        ''' 有形
        ''' </summary>
        Public Const LABEL_TANGIBLE_ASSET_NAME As String = "WI990692"

        ''' <summary>
        ''' 無形
        ''' </summary>
        Public Const LABEL_INTANGIBLE_ASSET_NAME As String = "WI990693"

        ''' <summary>
        ''' 資産台帳データ取込
        ''' </summary>
        Public Const LABEL_ASSET_LEDGER_DATA_IMPORT As String = "WI990700"

        ''' <summary>
        ''' 稼働区分
        ''' </summary>
        Public Const LABEL_OPE_STATUS As String = "WC130110"

        ''' <summary>
        ''' 稼働区分名
        ''' </summary>
        Public Const LABEL_OPE_STATUS_NAME As String = "WC130111"

        ''' <summary>
        ''' 管理ロケ区分コード
        ''' </summary>
        Public Const LABEL_MNG_LOC_L_COM_CODE_C As String = "WC320004"

        ''' <summary>
        ''' 管理ロケ名
        ''' </summary>
        Public Const LABEL_MNG_LOC_L_COM_NAME_C As String = "WC320005"

        ''' <summary>
        ''' ロケーション(会社)コード
        ''' </summary>
        Public Const LABEL_LOC_L_COM_CODE_C As String = "WC320006"

        ''' <summary>
        ''' ロケーション(会社)名
        ''' </summary>
        Public Const LABEL_LOC_L_COM_NAME_C As String = "WC320007"

        ''' <summary>
        ''' ロケーション(部署)コード
        ''' </summary>
        Public Const LABEL_LOC_M_DEP_CODE_C As String = "WC320008"

        ''' <summary>
        ''' ロケーション(部署)名
        ''' </summary>
        Public Const LABEL_LOC_M_DEP_NAME_C As String = "WC320009"

        ''' <summary>
        ''' ロケーション(場所)コード
        ''' </summary>
        Public Const LABEL_LOC_M_PLA_NAME_C As String = "WC320010"

        ''' <summary>
        ''' ロケーション(場所)名
        ''' </summary>
        Public Const LABEL_LOC_M_PLA_CODE_C As String = "WC320011"

        ''' <summary>
        ''' カテゴリ大分類コード
        ''' </summary>
        Public Const LABEL_CAT_L_CODE_C As String = "WC320012"

        ''' <summary>
        ''' カテゴリ大分類名
        ''' </summary>
        Public Const LABEL_CAT_L_NAME_C As String = "WC320013"

        ''' <summary>
        ''' 転用or除却
        ''' </summary>
        Public Const LABEL_DIVERSION_OR_REMOVE As String = "WI990615"

        ''' <summary>
        ''' 除却内容
        ''' </summary>
        Public Const LABEL_REMOVE_CONTENT As String = "WI990702"

        ''' <summary>
        ''' 除却理由
        ''' </summary>
        Public Const LABEL_REMOVE_REASON As String = "WI990703"

        ''' <summary>
        ''' 除却時期
        ''' </summary>
        Public Const LABEL_REMOVE_DATE As String = "WI990704"

        ''' <summary>
        ''' 変更後課コード/名
        ''' </summary>
        Public Const LABEL_CHANGE_LOC_M_CD_NAME As String = "WI990616"

        ''' <summary>
        ''' 変更後課コード
        ''' </summary>
        Public Const LABEL_CHANGE_LOC_M_CD As String = "WI990617"

        ''' <summary>
        ''' 変更後課名
        ''' </summary>
        Public Const LABEL_CHANGE_LOC_M_NAME As String = "WI990618"


        ''' <summary>
        ''' 新変更後設置場所コード/名
        ''' </summary>
        Public Const LABEL_CHANGE_LOC_S_CD_NAME As String = "WI990619"

        ''' <summary>
        ''' 新変更後設置場所コード
        ''' </summary>
        Public Const LABEL_CHANGE_LOC_S_CD As String = "WI990620"

        ''' <summary>
        ''' 新変更後設置場所名
        ''' </summary>
        Public Const LABEL_CHANGE_LOC_S_NAME As String = "WI990621"

        ''' <summary>
        ''' 変更後使用社員番号/名
        ''' </summary>
        Public Const LABEL_CHANGE_EMPLOYEE_CD_NAME As String = "WI990622"

        ''' <summary>
        ''' 変更後使用社員番号
        ''' </summary>
        Public Const LABEL_CHANGE_EMPLOYEE_CD As String = "WI990623"

        ''' <summary>
        ''' 変更後使用社員名
        ''' </summary>
        Public Const LABEL_CHANGE_EMPLOYEE_NAME As String = "WI990624"

        ''' <summary>
        ''' 備考1
        ''' </summary>
        Public Const LABEL_REMARK1 As String = "WI990673"

        ''' <summary>
        ''' 備考2
        ''' </summary>
        Public Const LABEL_REMARK2 As String = "WI990674"

        ''' <summary>
        ''' 備考3
        ''' </summary>
        Public Const LABEL_REMARK3 As String = "WI990675"

        ''' <summary>
        ''' 備考4
        ''' </summary>
        Public Const LABEL_REMARK4 As String = "WI990676"

        ''' <summary>
        ''' 備考5
        ''' </summary>
        Public Const LABEL_REMARK5 As String = "WI990677"

        ''' <summary>
        ''' 備考6
        ''' </summary>
        Public Const LABEL_REMARK6 As String = "WI990678"

        ''' <summary>
        ''' 備考7
        ''' </summary>
        Public Const LABEL_REMARK7 As String = "WI990679"

        ''' <summary>
        ''' 備考8
        ''' </summary>
        Public Const LABEL_REMARK8 As String = "WI990680"

        ''' <summary>
        ''' 備考9
        ''' </summary>
        Public Const LABEL_REMARK9 As String = "WI990681"

        ''' <summary>
        ''' 備考10
        ''' </summary>
        Public Const LABEL_REMARK10 As String = "WI990682"

        ''' <summary>
        ''' ロック担当者コード
        ''' </summary>
        Public Const LABEL_LOCK_EMPLOYEE_CD As String = "WI210026"

        ''' <summary>
        ''' ロック担当者コード/名
        ''' </summary>
        Public Const LABEL_LOCK_EMPLOYEE_CD_NAME As String = "WI990625"

        ''' <summary>
        ''' 変更後稼働区分
        ''' </summary>
        Public Const CHANGED_OPE_STATUS As String = "WI990705"

        ''' <summary>
        ''' CSV取込
        ''' </summary>
        Public Const BUTTON_CSV_IMPORT As String = "WB990067"

        ''' <summary>
        ''' データ取込
        ''' </summary>
        Public Const BUTTON_DATA_IMPORT As String = "WB990071"

        ''' <summary>
        ''' データ出力
        ''' </summary>
        Public Const BUTTON_DATA_EXPORT As String = "WB990070"

        ''' <summary>
        ''' 棚卸実施端末
        ''' </summary>
        Public Const LABEL_INV_TOOL_DIV As String = "WI990711"

        ''' <summary>
        ''' 入力作業者
        ''' </summary>
        Public Const LABEL_INPUT_EMPLOYEE_CD As String = "WI990712"

        ''' <summary>
        ''' 入力作業者コード/名
        ''' </summary>
        Public Const LABEL_INPUT_EMPLOYEE_CD_NAME As String = "WI990713"

        ''' <summary>
        ''' ロック資産ステータス
        ''' </summary>
        Public Const LABEL_LOCK_STATUS As String = "WI990714"
        '[CC431] ADD Start 20261002
        ''' <summary>
        ''' 実棚日
        ''' </summary>
        Public Const LABEL_PHYINV_DATE_SEARCH As String = "WI990716"
        ''' <summary>
        ''' 実棚者コード
        ''' </summary>
        Public Const LABEL_PHYINV_EMPLOYEE_CD As String = "WI990717"
        '[CC431] ADD End 20261002

    End Class

    ''' <summary>
    ''' プロトタイプ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelMember
        ''' <summary>
        ''' マスタメンテナンス:ID
        ''' </summary>
        Public Const LABEL_ID As String = "WI800001"

        ''' <summary>
        ''' マスタメンテナンス:名前
        ''' </summary>
        Public Const LABEL_NAME As String = "WI800002"

        ''' <summary>
        ''' マスタメンテナンス:メールアドレス
        ''' </summary>
        Public Const LABEL_MAIL As String = "WI800003"

        ''' <summary>
        ''' マスタメンテナンス:タイトル
        ''' </summary>
        Public Const LABEL_TITLE As String = "WT800001"

        ''' <summary>
        ''' マスタメンテナンス:有効期限
        ''' </summary>
        Public Const LABEL_YUKO_DATE As String = "WI800004"

    End Class

    ''' <summary>
    ''' ログイン画面用定義クラス
    ''' </summary>
    Public Class CommonLabelLogin
        ''' <summary>
        ''' ログイン:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT010002"

        ''' <summary>
        ''' 退職者のためログインできません。
        ''' </summary>
        Public Const MSG_ERR_RETIRE_EMPLOYEE As String = "WM010001"
    End Class

    ''' <summary>
    ''' 資産ラベル発行画面用定義クラス
    ''' </summary>
    Public Class CommonLabelAssetLabelPrint
        ''' <summary>
        ''' 資産ラベル発行:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT080001"
    End Class

    ''' <summary>
    ''' 資産一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelAssetList
        ''' <summary>
        ''' 資産一覧:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT010001"

        ''' <summary>
        ''' 資産一覧:現在貸出先
        ''' </summary>
        Public Const LABEL_LEASE_PLACE_NAME As String = "WI010001"
    End Class

    ''' <summary>
    ''' 在庫照会画面用定義クラス
    ''' </summary>
    Public Class CommonLabelStockList
        ''' <summary>
        ''' 在庫照会:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT020001"

    End Class

    ''' <summary>
    ''' 資産履歴照会画面用定義クラス
    ''' </summary>
    Public Class CommonLabelAssetHistory
        ''' <summary>
        ''' 資産履歴照会:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT030001"
    End Class

    ''' <summary>
    ''' CSVマッピング画面用定義クラス
    ''' </summary>
    Public Class CommonLabelCsvMap
        ''' <summary>
        ''' CSVマッピング:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT050001"

    End Class

    '2023/03/22 Kawamoto ADD START
    ''' <summary>
    ''' 指定数量管理表出力画面用定義クラス
    ''' </summary>
    Public Class CommonLabelDrugSpecList
        ''' <summary>
        ''' 指定数量管理表出力:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT060001"
    End Class

    ''' <summary>
    ''' 管理簿出力画面用定義クラス
    ''' </summary>
    Public Class CommonLabelDrugManageList
        ''' <summary>
        ''' 管理簿出力:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT070001"
    End Class
    '2023/03/22 Kawamoto ADD END

    ''' <summary>
    ''' 工事一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelConstructionList
        ''' <summary>
        ''' 工事一覧:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT410001"

        ''' <summary>
        ''' 工事登録:タイトル
        ''' </summary>
        Public Const TITLE_CONSTUCTION_REGISTRATION As String = "WT410002"

        ''' <summary>
        ''' 工事更新:タイトル
        ''' </summary>
        Public Const TITLE_CONSTUCTION_UPDATE As String = "WT410003"

        ''' <summary>
        ''' 工事完了:タイトル
        ''' </summary>
        Public Const TITLE_CONST_COMPLETE As String = "WT410004"

        ''' <summary>
        ''' 資産予約:タイトル
        ''' </summary>
        Public Const TITLE_ASSET_RESERVE As String = "WT410005"

        ''' <summary>
        ''' 工事完了:ボタン
        ''' </summary>
        Public Const BUTTON_CONST_COMPLETE As String = "WB410001"

        ''' <summary>
        ''' 資産照会:ボタン
        ''' </summary>
        Public Const BUTTON_ASSET_INQUILY As String = "WB410002"

        ''' <summary>
        ''' 解除:ボタン
        ''' </summary>
        Public Const BUTTON_RELEASE As String = "WB410003"

        ''' <summary>
        ''' 個数変更:ボタン
        ''' </summary>
        Public Const BUTTON_QUANTITY_CHANGE As String = "WB410004"

        ''' <summary>
        ''' 完了:ボタン
        ''' </summary>
        Public Const BUTTON_COMPLETE As String = "WB410005"

        ''' <summary>
        ''' 工事完了:完了済みの工事が含まれています。
        ''' </summary>
        Public Const MSG_CONST_COMPLETED As String = "WM410001"

        ''' <summary>
        ''' 工事完了:未完了の工事が含まれています。
        ''' </summary>
        Public Const MSG_CONST_UNCOMPLETED As String = "WM410002"

        ''' <summary>
        ''' 工事完了:完了備考を入力して下さい。
        ''' </summary>
        Public Const MSG_ERR_NOT_ENTEREDCONFIRM_REMARK As String = "WM410003"

        ''' <summary>
        ''' 工事完了:工事完了処理が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_CONST_COMPLETE As String = "WM410004"

        ''' <summary>
        ''' 工事完了:更新権限がない工事が選択されています。
        ''' </summary>
        Public Const MSG_ERR_SELECTED_NOT_UPD_CONST As String = "WM410005"

        ''' <summary>
        ''' 工事完了:工事完了処理を行います。よろしいですか？
        ''' </summary>
        Public Const MSG_CONST_COMPLET_CONFIRM As String = "WM410006"

        ''' <summary>
        ''' 工事完了:指定した工事の資産は全て返却されていませんが、完了してもよろしいですか？
        ''' </summary>
        Public Const MSG_UNRETURNED_CONFIRM As String = "WM410007"

        ''' <summary>
        ''' 工事更新:仮置き登録されている工事のため、削除できません。
        ''' </summary>
        Public Const MSG_EXIST_REG_CONST_NO As String = "WM410008"

        ''' <summary>
        ''' 資産予約:貸出・返却済み数を下回った設定はできません。（貸出・返却済み数：{0}件）
        ''' </summary>
        Public Const MSG_LOWER_LEND_RETURNED_NUM As String = "WM410009"

        ''' <summary>
        ''' 資産予約:貸出中の資産の予約期間が変更できませんでした。{0}
        ''' </summary>
        Public Const MSG_CANNOT_CHANGE_LENDING_ASSET As String = "WM410010"

        ''' <summary>
        ''' 資産予約:予約変更できませんでした。{0}
        ''' </summary>
        Public Const MSG_CANNOT_CHANGE_RESERVE As String = "WM410011"

        ''' <summary>
        ''' 資産予約:予約期間が変更できませんでした。{0}
        ''' </summary>
        Public Const MSG_CANNOT_CHANGE_TERM As String = "WM410012"

        ''' <summary>
        ''' 資産予約:予約できませんでした。
        ''' </summary>
        Public Const MSG_CANNOT_RESERVE As String = "WM410013"

        ''' <summary>
        ''' 工事更新:※実質貸出期間の変更時は予約の追加/更新はできません。更新してから実施してください。
        ''' </summary>
        Public Const MSG_CANNOT_CHANGE_ADD_RESERVE As String = "WM410014"

        ''' <summary>
        ''' 工事登録:※工事登録後、仮置き登録が可能になります。
        ''' </summary>
        Public Const MSG_CANNOT_ADD_RESERVE As String = "WM410015"

        ''' <summary>
        ''' 作業日
        ''' </summary>
        Public Const LABEL_CONST_WORK_DAY As String = "WI410001"

        ''' <summary>
        ''' 作業責任者
        ''' </summary>
        Public Const LABEL_CONST_PIC As String = "WI410002"

        ''' <summary>
        ''' 仮置き情報
        ''' </summary>
        Public Const LABEL_TEMP_STORAGE_INFO As String = "WI410003"

        ''' <summary>
        ''' 予約情報
        ''' </summary>
        Public Const LABEL_RESERVATION_INFO As String = "WI410004"

        ''' <summary>
        ''' 完了備考
        ''' </summary>
        Public Const LABEL_COMPLETION_REMARKS As String = "WI410005"

        ''' <summary>
        ''' 数量不足
        ''' </summary>
        Public Const LABEL_QUANTITY_SHORTAGE As String = "WI410006"

        ''' <summary>
        ''' 工事責任者コード
        ''' </summary>
        Public Const LABEL_CONST_EMPLOYEE_CD As String = "WI410007"

        ''' <summary>
        ''' 管理ロケ大区分コード
        ''' </summary>
        Public Const LABEL_MNG_LOC_L_CD As String = "WI410008"
    End Class

    ''' <summary>
    ''' 工事資産一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelConstAssetList
        ''' <summary>
        ''' 工事資産一覧:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT420001"
    End Class

    ''' <summary>
    ''' 工事履歴一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelConstructionHist
        ''' <summary>
        ''' 工事履歴一覧:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT430001"
    End Class

    ''' <summary>
    ''' 工事資産履歴一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelConstAssetHist
        ''' <summary>
        ''' 工事資産履歴一覧:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT440001"
    End Class

    ''' <summary>
    ''' 仮置一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelTemporaryList
        ''' <summary>
        ''' 仮置一覧:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT450001"

        ''' <summary>
        ''' 仮置き更新:タイトル
        ''' </summary>
        Public Const TITLE_TEMPORARY_UPDATE As String = "WT450002"

        ''' <summary>
        ''' 割当済みの仮置きを選択しています。
        ''' </summary>
        Public Const MSG_ADD_REL_TEMP As String = "WM450001"

        ''' <summary>
        ''' 選択した仮置きを追加してよろしいですか？
        ''' </summary>
        Public Const MSG_ADD_CONFIRM As String = "WM450002"

        ''' <summary>
        ''' 仮置き追加処理が正常終了しました。
        ''' </summary>
        Public Const MSG_SUCCESS_ADD_REL_COMPLETE As String = "WM450003"

    End Class

    ''' <summary>
    ''' 仮置資産一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelTempAssetList
        ''' <summary>
        ''' 仮置資産一覧:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT460001"

    End Class

    ''' <summary>
    ''' 棚卸進捗状況画面用定義クラス
    ''' </summary>
    Public Class CommonLabelInventorySummary

        ''' <summary>
        ''' 棚卸進捗状況:タイトル
        ''' </summary>
        Public Const LABEL_TITLE As String = "WT210001"

        ''' <summary>
        ''' 棚卸進捗状況：棚卸開始宣言ボタン
        ''' </summary>
        Public Const LABEL_TANAOROSHI_KAISHI As String = "WB210001"

        ''' <summary>
        ''' 棚卸進捗状況：棚卸確定ボタン
        ''' </summary>
        Public Const LABEL_TANAOROSHI_KAKUTEI As String = "WB210002"

        ''' <summary>
        ''' 棚卸進捗状況：実棚照会ボタン
        ''' </summary>
        Public Const LABEL_JITSUTANA_SHOKAI As String = "WB210003"

        ''' <summary>
        ''' 棚卸進捗状況：現ロケに変更ボタン
        ''' </summary>
        Public Const BUTTON_CHANGE_CURRENT_LOC As String = "WB210004"

        ''' <summary>
        ''' 棚卸進捗状況：棚卸開始宣言済
        ''' </summary>
        Public Const MSG_TANAOROSHI_KAISHI_ALREADY As String = "WM210001"

        ''' <summary>
        ''' 棚卸進捗状況：棚卸開始/確定する権限がありません。
        ''' </summary>
        Public Const MSG_TANAOROSHI_AUTH As String = "WM210011"

        '20231027 ADD Start Shirakawa
        ''' <summary>
        ''' 棚卸進捗状況：選択したロケは棚卸開始宣言をしていません。
        ''' </summary>
        Public Const MSG_NOT_INVENTORY As String = "WM210009"

        ''' <summary>
        ''' 棚卸進捗状況：選択したロケに異なる棚卸IDが含まれています。
        ''' </summary>
        Public Const MSG_DIFFERENT_INVID As String = "WM210010"
        '20231027 ADD End Shirakawa

        ''' <summary>
        ''' 棚卸進捗状況：稼働区分が設定されていないデータがあります。棚卸確定処理を中止しました。
        ''' </summary>
        Public Const MSG_OPE_STATUS_ERR As String = "WM210031"

        ''' <summary>
        ''' 棚卸進捗状況：除却時期はyyyy/mmの形式で入力してください。
        ''' </summary>
        Public Const MSG_REMOVE_DATE_FORMAT As String = "WM210032"

        ''' <summary>
        ''' 帳簿
        ''' </summary>
        Public Const LABEL_BOOK As String = "WI210001"

        ''' <summary>
        ''' 実棚
        ''' </summary>
        Public Const LABEL_REAL_SHELF As String = "WI210002"

        ''' <summary>
        ''' 資産数
        ''' </summary>
        Public Const LABEL_NUMBER_OF_ASSETS As String = "WI210003"

        ''' <summary>
        ''' 貸出数
        ''' </summary>
        Public Const LABEL_NUMBER_OF_LOANS As String = "WI210004"

        ''' <summary>
        ''' 在庫数
        ''' </summary>
        Public Const LABEL_NUMBER_OF_STOCKS As String = "WI210005"

        ''' <summary>
        ''' 未確認
        ''' </summary>
        Public Const LABEL_UNCONFIRMED As String = "WI210006"

        ''' <summary>
        ''' 帳簿在庫数
        ''' </summary>
        Public Const LABEL_BOOK_QUANTITY As String = "WI210007"

        ''' <summary>
        ''' 内貸出数
        ''' </summary>
        Public Const LABEL_NUMBER_OF_LOANS_WITHIN As String = "WI210008"

        ''' <summary>
        ''' 実棚数
        ''' </summary>
        Public Const LABEL_ACTUAL_NUMBER_OF_SHELVES As String = "WI210009"

        ''' <summary>
        ''' 正常数
        ''' </summary>
        Public Const LABEL_NORMAL_NUMBER As String = "WI210010"

        ''' <summary>
        ''' ロケ違い数
        ''' </summary>
        Public Const LABEL_LOCATION_DIFFERENCE_NUMBER As String = "WI210011"

        ''' <summary>
        ''' 在庫なし数
        ''' </summary>
        Public Const LABEL_OUT_OF_STOCK As String = "WI210012"

        '--201908/06 del start nv iwamoto CommonLabelに移動
        '''' <summary>
        '''' 実棚照会:実棚ﾛｹｰｼｮﾝ大区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_PHYINV_LOCATION_L_CODE_NAME As String = "WI040001"

        '''' <summary>
        '''' 実棚照会:実棚ﾛｹｰｼｮﾝ中区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_PHYINV_LOCATION_M_CODE_NAME As String = "WI040002"

        '''' <summary>
        '''' 実棚照会:実棚ﾛｹｰｼｮﾝ小区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_PHYINV_LOCATION_S_CODE_NAME As String = "WI040003"
        '--201908/06 del end nv iwamoto

        ''' <summary>
        ''' 実棚照会:実棚日時
        ''' </summary>
        Public Const LABEL_PHYINV_DATE As String = "WI210014"

        ''' <summary>
        ''' 実棚照会:実棚者
        ''' </summary>
        Public Const LABEL_PHYINV_PERSON_NAME As String = "WI210015"

        ''' <summary>
        ''' 棚卸名入力:タイトル
        ''' </summary>
        Public Const LABEL_TANAOROSHI_NAME_TITLE As String = "WT210002"

        ''' <summary>
        ''' 棚卸名:ラベル
        ''' </summary>
        Public Const LABEL_TANAOROSHI_NAME As String = "WI210016"

        ''' <summary>
        ''' 棚卸名:棚卸を確定します。よろしいですか？
        ''' </summary>
        Public Const INVENTORY_CONFIRM As String = "WM210003"

        '20230929 ADD Start Shirakawa
        ''' <summary>
        ''' 棚卸状態
        ''' </summary>
        Public Const LABEL_INVENTORY_STATUIS As String = "WI210021"

        ''' <summary>
        ''' 棚卸開始日
        ''' </summary>
        Public Const LABEL_INVENTORY_STARTDAY As String = "WI210022"
        '20230929 ADD End Shirakawa

        ''' <summary>
        ''' 棚卸中
        ''' </summary>
        Public Const LABEL_INVENTORY_STARTED As String = "WI990493"

    End Class

    ''' <summary>
    ''' 実棚照会画面用定義クラス
    ''' </summary>
    Public Class CommonLabelInventoryInquiry

        ''' <summary>
        ''' 実棚照会:タイトル
        ''' </summary>
        Public Const LABEL_TITLE As String = "WT210004"

        ''' <summary>
        ''' 実棚照会:タイトル
        ''' </summary>
        Public Const TITLE_INV_RESULT As String = "WT210005"

        ''' <summary>
        ''' 実棚照会:実棚照会詳細
        ''' </summary>
        Public Const TITLE_INV_RESULT_DETAIL As String = "WT210009"

        ''' <summary>
        ''' 実棚照会:未紐付け
        ''' </summary>
        Public Const TITLE_INV_UNLINKED As String = "WV990011"

        ''' <summary>
        ''' 実棚照会:在庫
        ''' </summary>
        Public Const TITLE_INV_STOCK As String = "WV990012"

        ''' <summary>
        ''' 実棚照会:貸出
        ''' </summary>
        Public Const TITLE_INV_LENDING As String = "WV990013"

        ''' <summary>
        ''' 実棚照会:除却
        ''' </summary>
        Public Const TITLE_INV_REMOVALE As String = "WV990014"

        ''' <summary>
        ''' 実棚照会:対象外
        ''' </summary>
        Public Const TITLE_INV_EXCLUDED As String = "WV990041"

        ''' <summary>
        ''' 実棚照会:現物なし(未処理を含む)
        ''' </summary>
        Public Const TITLE_INV_NOSPOT As String = "WV990042"

        ''' <summary>
        ''' 実棚照会:正常
        ''' </summary>
        Public Const TITLE_INV_NORMAL As String = "WV990043"

        ''' <summary>
        ''' 実棚照会:現物あり(ロケ違い)
        ''' </summary>
        Public Const TITLE_INV_INKIND As String = "WV990044"



        '--2019/07/16 nv miyaji コメントアウト start　CommonLabelに移動
        '''' <summary>
        '''' 実棚照会：ロケーション
        '''' </summary>
        'Public Const LABEL_GEN_LOCATION As String = "WI040001"

        '''' <summary>
        '''' 実棚照会：大区分
        '''' </summary>
        'Public Const LABEL_KUBUN_L As String = "WI040002"

        '''' <summary>
        '''' 実棚照会：中区分
        '''' </summary>
        'Public Const LABEL_KUBUN_M As String = "WI040003"

        '''' <summary>
        '''' 実棚照会：小区分
        '''' </summary>
        'Public Const LABEL_KUBUN_S As String = "WI040004"

        ''' <summary>
        ''' 実棚照会：実棚ステータス
        ''' </summary>
        Public Const LABEL_JITSUTANA_STATUS As String = "WI210013"

        '''' <summary>
        '''' 実棚照会：実棚ステータス 全て
        '''' </summary>
        'Public Const LABEL_JITSUTANA_STATUS_ALL As String = "WI040006"

        '''' <summary>
        '''' 実棚照会：実棚ステータス 正常
        '''' </summary>
        'Public Const LABEL_JITSUTANA_STATUS_SEIJO As String = "WI040007"

        '''' <summary>
        '''' 実棚照会：実棚ステータス 現物あり
        '''' </summary>
        'Public Const LABEL_JITSUTANA_STATUS_ARI As String = "WI040008"

        '''' <summary>
        '''' 実棚照会：実棚ステータス 現物なし
        '''' </summary>
        'Public Const LABEL_JITSUTANA_STATUS_NASHI As String = "WI040009"

        '''' <summary>
        '''' 実棚照会：資産番号
        '''' </summary>
        'Public Const LABEL_ASSET_NO As String = "WI040010"

        '''' <summary>
        '''' 実棚照会：資産名称
        '''' </summary>
        'Public Const LABEL_ASSET_NAME As String = "WI040011"

        '''' <summary>
        '''' 実棚照会：メーカ
        '''' </summary>
        'Public Const LABEL_MAKER As String = "WI040012"

        '''' <summary>
        '''' 実棚照会：品番
        '''' </summary>
        'Public Const LABEL_PRODUCT_NO As String = "WI040013"

        '''' <summary>
        '''' 実棚照会：型番
        '''' </summary>
        'Public Const LABEL_MODEL_NO As String = "WI040014"

        ''' <summary>
        ''' 実棚照会:帳簿在庫数
        ''' </summary>
        Public Const LABEL_BOOK_QUANTITY As String = "WI210007"

        ''' <summary>
        ''' 実棚照会:正常数
        ''' </summary>
        Public Const LABEL_NORMAL_QUANTITY As String = "WI210010"

        ''' <summary>
        ''' 実棚照会:ロケ違い数
        ''' </summary>
        Public Const LABEL_LOCATION_DIFF_QUANTITY As String = "WI210011"

        ''' <summary>
        ''' 実棚照会:在庫なし数
        ''' </summary>
        Public Const LABEL_NONE_QUANTITY As String = "WI210012"

        '''' <summary>
        '''' 実棚照会:資産状態
        '''' </summary>
        'Public Const LABEL_ASSET_STATUS As String = "WI040019"

        ''' <summary>
        ''' 実棚照会:実棚ｽﾃｰﾀｽ
        ''' </summary>
        Public Const LABEL_PHYINV_STATUS As String = "WI210013"

        '''' <summary>
        '''' 実棚照会:帳簿ﾛｹｰｼｮﾝ大区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_BOOK_LOCATION_L_CODE_NAME As String = "WI040021"

        '''' <summary>
        '''' 実棚照会:帳簿ﾛｹｰｼｮﾝ中区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_BOOK_LOCATION_M_CODE_NAME As String = "WI040022"

        '''' <summary>
        '''' 実棚照会:帳簿ﾛｹｰｼｮﾝ小区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_BOOK_LOCATION_S_CODE_NAME As String = "WI040023"

        '--2019/07/16 nv miyaji コメントアウト end　CommonLabelに移動

        '--201908/06 del start nv iwamoto CommonLabelに移動
        '''' <summary>
        '''' 実棚照会:実棚ﾛｹｰｼｮﾝ大区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_PHYINV_LOCATION_L_CODE_NAME As String = "WI040001"

        '''' <summary>
        '''' 実棚照会:実棚ﾛｹｰｼｮﾝ中区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_PHYINV_LOCATION_M_CODE_NAME As String = "WI040002"

        '''' <summary>
        '''' 実棚照会:実棚ﾛｹｰｼｮﾝ小区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_PHYINV_LOCATION_S_CODE_NAME As String = "WI040003"
        '--201908/06 del end nv iwamoto

        ''' <summary>
        ''' 実棚照会:実棚日時
        ''' </summary>
        Public Const LABEL_PHYINV_DATE As String = "WI210014"

        ''' <summary>
        ''' 実棚照会:実棚者
        ''' </summary>
        Public Const LABEL_PHYINV_PERSON_NAME As String = "WI210015"

        ''' <summary>
        ''' 実棚照会:実棚会社コード/名
        ''' </summary>
        Public Const LABEL_PHYINV_LOC_L_CD_NAME As String = "WI210023"

        ''' <summary>
        ''' 実棚照会:実棚部署コード/名
        ''' </summary>
        Public Const LABEL_PHYINV_LOC_M_CD_NAME As String = "WI210024"

        ''' <summary>
        ''' 実棚照会:実棚場所コード/名
        ''' </summary>
        Public Const LABEL_PHYINV_LOC_S_CD_NAME As String = "WI210025"

        ''' <summary>
        ''' 実棚照会:変更後部課
        ''' </summary>
        Public Const LABEL_CHANGE_LOC_M As String = "WI210027"

        ''' <summary>
        ''' 実棚照会:変更後設置番所
        ''' </summary>
        Public Const LABEL_CHANGE_LOC_S As String = "WI210028"

        ''' <summary>
        ''' 実棚照会:変更後社員番号
        ''' </summary>
        Public Const LABEL_CHANGE_EMPLOYEE As String = "WI210029"

        ''' <summary>
        ''' 実棚照会:現ロケに変更します。よろしいですか？
        ''' </summary>
        Public Const INVENTORY_CHANGE_LOCATION_CONFIRM As String = "WM210002"

        ''' <summary>
        ''' 実棚照会：現物確認処理を行います。よろしいですか？
        ''' </summary>
        Public Const MSG_GENBUTSU_KAKUNIN_CONFIRM As String = "WM210006"

        ''' <summary>
        ''' 実棚照会：現物未確認処理を行います。よろしいですか？
        ''' </summary>
        Public Const MSG_GENBUTSU_MIKAKUNIN_CONFIRM As String = "WM210007"

        ''' <summary>
        ''' 実棚照会：現物確認済に更新しました。
        ''' </summary>
        Public Const MSG_GENBUTSU_KAKUNIN_SUCCESS As String = "WM210008"

        ''' <summary>
        ''' 実棚照会：他の担当者がロックしています。
        ''' </summary>
        Public Const MSG_LOCK_OTHER_USER_DISABLE As String = "WM210012"

        ''' <summary>
        ''' 実棚照会：他の担当者がロックした資産が選択されています。よろしいですか？
        ''' </summary>
        Public Const MSG_LOCK_OTHER_USER_ENABLE As String = "WM210023"

        ''' <summary>
        ''' 実棚照会：変更後部課コードがロケーションマスタに登録されていないコードです。
        ''' </summary>
        Public Const MSG_LOC_M_ERROR As String = "WM210013"

        ''' <summary>
        ''' 実棚照会：変更後部署コードがロケーションマスタに登録されていないコードです。
        ''' </summary>
        Public Const MSG_LOC_S_ERROR As String = "WM210014"

        ''' <summary>
        ''' 実棚照会：変更後社員番号が社員マスタに登録されていません。
        ''' </summary>
        Public Const MSG_EMPLOYEE_ERROR As String = "WM210015"

        ''' <summary>
        ''' 実棚照会：稼働区分が入力されていません。
        ''' </summary>
        Public Const MSG_OPE_STATUS_ERROR As String = "WM210016"

        ''' <summary>
        ''' 実棚照会：除却時期(予定)が入力されていません。
        ''' </summary>
        Public Const MSG_REMOVE_DATE_ERROR As String = "WM210017"

        ''' <summary>
        ''' 実棚照会：除却内容が入力されていません。
        ''' </summary>
        Public Const MSG_REMOVE_CONTENT_ERROR As String = "WM210018"

        ''' <summary>
        ''' 実棚照会：除却時期(予定)を設定することはできません。
        ''' </summary>
        Public Const MSG_DONT_DIVERSION_OR_REMOVE_ERROR As String = "WM210019"

        ''' <summary>
        ''' 実棚照会：除却理由を設定することはできません。
        ''' </summary>
        Public Const MSG_DONT_REMOVE_REASON_ERROR As String = "WM210020"

        ''' <summary>
        ''' 実棚照会：除却内容を設定することはできません。
        ''' </summary>
        Public Const MSG_DONT_REMOVE_CONTENT_ERROR As String = "WM210021"

        ''' <summary>
        ''' 実棚照会：棚卸中ではありません。
        ''' </summary>
        Public Const MSG_NOT_INVENTORY_NOW As String = "WM210022"

        ''' <summary>
        ''' 実棚照会：ロックします。よろしいですか？
        ''' </summary>
        Public Const MSG_LOCK_CONFIRM As String = "WM210024"

        ''' <summary>
        ''' 実棚照会：ロックしました。
        ''' </summary>
        Public Const MSG_LOCK_COMPLETE As String = "WM210025"

        ''' <summary>
        ''' 実棚照会：ロック解除しました。
        ''' </summary>
        Public Const MSG_UNLOCK_COMPLETE As String = "WM210026"

        ''' <summary>
        ''' 実棚照会：変更しました。
        ''' </summary>
        Public Const MSG_CHANGE_COMPLETE As String = "WM210027"

        ''' <summary>
        ''' 実棚照会：変更処理を実行していません。ページを移動しますか？
        ''' </summary>
        Public Const MSG_PAGE_MOVE_CONFIRM As String = "WM210028"

        ''' <summary>
        ''' 実棚照会：現物確認ボタン
        ''' </summary>
        Public Const BUTTON_GENBUTSU_KAKUNIN As String = "WB210005"

        ''' <summary>
        ''' 実棚照会：現物未確認ボタン
        ''' </summary>
        Public Const BUTTON_GENBUTSU_MIKAKUNIN As String = "WB210006"

        ''' <summary>
        ''' 実棚照会:編集
        ''' </summary>
        Public Const LABEL_INVENTORY_EDIT As String = "WB210007"
        ''' <summary>
        ''' 実棚照会:ロック
        ''' </summary>
        Public Const LABEL_LOCK As String = "WB210008"

        ''' <summary>
        ''' 実棚照会:ロック解除
        ''' </summary>
        Public Const LABEL_UNLOCK As String = "WB210009"

        ''' <summary>
        ''' 実棚照会:変更内容がありません。
        ''' </summary>
        Public Const LABEL_NO_EDITED As String = "WM210029"

        ''' <summary>
        ''' 実棚照会：実棚ステータスが正常の資産が選択されています。
        ''' </summary>
        Public Const MSG_CONTAIN_NORMAL As String = "WM210030"

        ''' <summary>
        ''' 実棚照会：画面上に変更項目があります。変更を保存してから現物確認を行います。よろしいですか？
        ''' </summary>
        Public Const MSG_GENBUTSU_KAKUNIN_EDITED_CONFIRM As String = "WM210033"

        ''' <summary>
        ''' 実棚照会：取込を行うと、資産番号が一致する棚卸資産が更新されます。
        ''' </summary>
        Public Const MSG_INFO_CSV_IMPORT_DATA_UPDATE As String = "WM210034"

        ''' <summary>
        ''' 実棚照会：更新件数が取込件数と一致しません。（取込:{0} 更新:{1}）
        ''' </summary>
        Public Const MSG_ERR_UPDATEDATA_COUNT As String = "WM210035"

    End Class

    ''' <summary>
    ''' 棚卸実績一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelInventoryHistory

        ''' <summary>
        ''' 棚卸実績一覧:タイトル
        ''' </summary>
        Public Const LABEL_TITLE As String = "WT210006"

        ''' <summary>
        ''' 棚卸名変更:ボタン
        ''' </summary>
        Public Const LABEL_CHANGE_INVENTORY_NAME As String = "WT210003"

        ''' <summary>
        ''' 実棚照会:ボタン→「詳細」に文言変更　2019/09/16 UPDATE Inoue
        ''' </summary>
        Public Const LABEL_SHOW_INVENTORY_INQUIRY As String = "WB990031"

        ''' <summary>
        ''' 棚卸開始宣言日時:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_START_DEC_TS As String = "WI210017"

        ''' <summary>
        ''' 開始宣言者:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_START_DEC_NAME As String = "WI210018"

        ''' <summary>
        ''' 棚卸名:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_INV_NAME As String = "WI210016"

        ''' <summary>
        ''' 棚卸確定日時:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_INV_CONFIRM_TS As String = "WI210019"

        ''' <summary>
        ''' 棚卸確定者:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_INV_CONFIRM_NAME As String = "WI210020"

        ''' <summary>
        ''' 帳簿在庫数:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_LOGIC_INV_NUM As String = "WI210007"

        ''' <summary>
        ''' 実棚正常数:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_PHYINV_NUM As String = "WI210010"

        ''' <summary>
        ''' ロケ違い数:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_LOC_DIFF_NUM As String = "WI210011"

        ''' <summary>
        ''' 在庫なし数:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_NO_ITEM_NUM As String = "WI210012"

    End Class

    ''' <summary>
    ''' 棚卸実績照会画面用定義クラス
    ''' </summary>
    Public Class CommonLabelInventoryAssetHistory

        ''' <summary>
        ''' 棚卸実績照会:タイトル
        ''' </summary>
        Public Const LABEL_TITLE As String = "WT210007"

        ''' <summary>
        ''' 棚卸実績照会:タイトル
        ''' </summary>
        Public Const TITLE_INV_RESULT As String = "WT210005"

        '--2019/07/16 nv miyaji コメントアウト start　CommonLabelに移動
        '''' <summary>
        '''' 実棚照会：ロケーション
        '''' </summary>
        'Public Const LABEL_GEN_LOCATION As String = "WI040001"

        '''' <summary>
        '''' 実棚照会：大区分
        '''' </summary>
        'Public Const LABEL_KUBUN_L As String = "WI040002"

        '''' <summary>
        '''' 実棚照会：中区分
        '''' </summary>
        'Public Const LABEL_KUBUN_M As String = "WI040003"

        '''' <summary>
        '''' 実棚照会：小区分
        '''' </summary>
        'Public Const LABEL_KUBUN_S As String = "WI040004"

        ''' <summary>
        ''' 棚卸実績照会：実棚ステータス
        ''' </summary>
        Public Const LABEL_JITSUTANA_STATUS As String = "WI210013"

        '''' <summary>
        '''' 実棚照会：実棚ステータス 全て
        '''' </summary>
        'Public Const LABEL_JITSUTANA_STATUS_ALL As String = "WI040006"

        '''' <summary>
        '''' 実棚照会：実棚ステータス 正常
        '''' </summary>
        'Public Const LABEL_JITSUTANA_STATUS_SEIJO As String = "WI040007"

        '''' <summary>
        '''' 実棚照会：実棚ステータス 現物あり
        '''' </summary>
        'Public Const LABEL_JITSUTANA_STATUS_ARI As String = "WI040008"

        '''' <summary>
        '''' 実棚照会：実棚ステータス 現物なし
        '''' </summary>
        'Public Const LABEL_JITSUTANA_STATUS_NASHI As String = "WI040009"

        '''' <summary>
        '''' 実棚照会：資産番号
        '''' </summary>
        'Public Const LABEL_ASSET_NO As String = "WI040010"

        '''' <summary>
        '''' 実棚照会：資産名称
        '''' </summary>
        'Public Const LABEL_ASSET_NAME As String = "WI040011"

        '''' <summary>
        '''' 実棚照会：メーカ
        '''' </summary>
        'Public Const LABEL_MAKER As String = "WI040012"

        '''' <summary>
        '''' 実棚照会：品番
        '''' </summary>
        'Public Const LABEL_PRODUCT_NO As String = "WI040013"

        '''' <summary>
        '''' 実棚照会：型番
        '''' </summary>
        'Public Const LABEL_MODEL_NO As String = "WI040014"

        ''' <summary>
        ''' 棚卸実績照会:帳簿在庫数
        ''' </summary>
        Public Const LABEL_BOOK_QUANTITY As String = "WI210007"

        ''' <summary>
        ''' 棚卸実績照会:正常数
        ''' </summary>
        Public Const LABEL_NORMAL_QUANTITY As String = "WI210010"

        ''' <summary>
        ''' 棚卸実績照会:ロケ違い数
        ''' </summary>
        Public Const LABEL_LOCATION_DIFF_QUANTITY As String = "WI210011"

        ''' <summary>
        ''' 棚卸実績照会:在庫なし数
        ''' </summary>
        Public Const LABEL_NONE_QUANTITY As String = "WI210012"

        '''' <summary>
        '''' 実棚照会:資産状態
        '''' </summary>
        'Public Const LABEL_ASSET_STATUS As String = "WI040019"

        ''' <summary>
        ''' 棚卸実績照会:実棚ｽﾃｰﾀｽ
        ''' </summary>
        Public Const LABEL_PHYINV_STATUS As String = "WI210013"

        '''' <summary>
        '''' 実棚照会:帳簿ﾛｹｰｼｮﾝ大区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_BOOK_LOCATION_L_CODE_NAME As String = "WI040021"

        '''' <summary>
        '''' 実棚照会:帳簿ﾛｹｰｼｮﾝ中区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_BOOK_LOCATION_M_CODE_NAME As String = "WI040022"

        '''' <summary>
        '''' 実棚照会:帳簿ﾛｹｰｼｮﾝ小区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_BOOK_LOCATION_S_CODE_NAME As String = "WI040023"

        '--2019/07/16 nv miyaji コメントアウト end　CommonLabelに移動

        '--201908/06 del start nv iwamoto CommonLabelに移動
        '''' <summary>
        '''' 実棚照会:実棚ﾛｹｰｼｮﾝ大区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_PHYINV_LOCATION_L_CODE_NAME As String = "WI040001"

        '''' <summary>
        '''' 実棚照会:実棚ﾛｹｰｼｮﾝ中区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_PHYINV_LOCATION_M_CODE_NAME As String = "WI040002"

        '''' <summary>
        '''' 実棚照会:実棚ﾛｹｰｼｮﾝ小区分ｺｰﾄﾞ/名
        '''' </summary>
        'Public Const LABEL_PHYINV_LOCATION_S_CODE_NAME As String = "WI040003"
        '--201908/06 del end nv iwamoto

        ''' <summary>
        ''' 棚卸実績照会:実棚日時
        ''' </summary>
        Public Const LABEL_PHYINV_DATE As String = "WI210014"

        ''' <summary>
        ''' 棚卸実績照会:実棚者
        ''' </summary>
        Public Const LABEL_PHYINV_PERSON_NAME As String = "WI210015"

    End Class

    '20231023 ADD Start Shirakawa
    ''' <summary>
    ''' 棚卸実績ロケ別一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelInventoryLocHistoryController

        ''' <summary>
        ''' 棚卸実績ロケ別一覧:タイトル
        ''' </summary>
        Public Const LABEL_TITLE As String = "WT210008"

        ''' <summary>
        ''' 詳細:ボタン
        ''' </summary>
        Public Const LABEL_SHOW_INVENTORY_INQUIRY As String = "WB990031"


        'ヘッダーエリア-------------------
        ''' <summary>
        ''' 棚卸開始宣言日時:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_START_DEC_TS As String = "WI210017"

        ''' <summary>
        ''' 開始宣言者:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_START_DEC_NAME As String = "WI210018"

        ''' <summary>
        ''' 棚卸名:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_INV_NAME As String = "WI210016"

        ''' <summary>
        ''' 棚卸確定日時:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_INV_CONFIRM_TS As String = "WI210019"

        ''' <summary>
        ''' 棚卸確定者:グリッドヘッダタイトル
        ''' </summary>
        Public Const LABEL_INV_CONFIRM_NAME As String = "WI210020"
        '---------------------------------

        ''' <summary>
        ''' 帳簿在庫数
        ''' </summary>
        Public Const LABEL_BOOK_QUANTITY As String = "WI210007"

        ''' <summary>
        ''' 内貸出数
        ''' </summary>
        Public Const LABEL_NUMBER_OF_LOANS_WITHIN As String = "WI210008"

        ''' <summary>
        ''' 実棚数
        ''' </summary>
        Public Const LABEL_ACTUAL_NUMBER_OF_SHELVES As String = "WI210009"

        ''' <summary>
        ''' 正常数
        ''' </summary>
        Public Const LABEL_NORMAL_NUMBER As String = "WI210010"

        ''' <summary>
        ''' ロケ違い数
        ''' </summary>
        Public Const LABEL_LOCATION_DIFFERENCE_NUMBER As String = "WI210011"

        ''' <summary>
        ''' 在庫なし数
        ''' </summary>
        Public Const LABEL_OUT_OF_STOCK As String = "WI210012"

    End Class

    '20231023 ADD End Shirakawa

    ''' <summary>
    ''' 操作ログ照会画面用定義クラス
    ''' </summary>
    Public Class CommonLabelOperationLog

        ''' <summary>
        ''' 操作ログ照会:タイトル
        ''' </summary>
        Public Const LABEL_TITLE As String = "WI990011"

        ''' <summary>
        ''' 操作ログ照会:ログイン者名
        ''' </summary>
        Public Const LABEL_LOGIN_EMPLOYEE_NAME As String = "WI930002"

        ''' <summary>
        ''' 操作ログ照会:グリッド：処理日時
        ''' </summary>
        Public Const LABEL_PROCESS_DATE As String = "WI930003"

        ''' <summary>
        ''' 操作ログ照会:グリッド：処理種別
        ''' </summary>
        Public Const LABEL_PROCESS_CAT As String = "WI930004"

        ''' <summary>
        ''' 操作ログ照会:グリッド：操作社員ｺｰﾄﾞ/名
        ''' </summary>
        Public Const LABEL_EMPLOYEE_CODE_NAME As String = "WI930005"

        ''' <summary>
        ''' 操作ログ照会:グリッド：操作内容
        ''' </summary>
        Public Const LABEL_PROCESS_CONTENTS As String = "WI930006"

        ''' <summary>
        ''' 操作ログ照会:ログ種別を選択して下さい
        ''' </summary>
        Public Const MSG_ERR_PROCESS_CAT_UNSELECTED As String = "WM930001"

    End Class

    ''' <summary>
    ''' カテゴリーマスタ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelCategory
        ''' <summary>
        ''' カテゴリーマスタメンテナンス:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT710001"

        ''' <summary>
        ''' カテゴリーマスタメンテナンス:資産ID(EPC)登録方法
        ''' </summary>
        Public Const LABEL_ASSET_ID_REG_DIV As String = "WI710001"

        ''' <summary>
        ''' カテゴリーマスタメンテナンス:タグEPC領域サイズ
        ''' </summary>
        Public Const LABEL_EPC_SIZE As String = "WI710002"

        ''' <summary>
        ''' 中分類または小分類のデータにもかかわらず、資産ID(EPC)登録方法が入力されています。
        ''' </summary>
        Public Const MSG_ERR_ASSET_ID_REG_DIV_INPUT As String = "WM710004"

        ''' <summary>
        ''' 中分類または小分類のデータにもかかわらず、タグEPC領域サイズが入力されています。
        ''' </summary>
        Public Const MSG_ERR_EPC_SIZE_INPUT As String = "WM710005"

        ''' <summary>
        ''' 中分類または小分類のデータにもかかわらず、タグEPC事業所が入力されています。
        ''' </summary>
        Public Const MSG_ERR_EPC_COMPANY_KEY_INPUT As String = "WM710006"

        ''' <summary>
        ''' 中分類または小分類のデータにもかかわらず、タグEPC資産タイプが入力されています。
        ''' </summary>
        Public Const MSG_ERR_EPC_ASSET_TYPE_INPUT As String = "WM710007"

        ''' <summary>
        ''' 貸出上限数は、貸出アラート回数以上の値を入力してください。
        ''' </summary>
        Public Const MSG_ERR_LEASE_UPPER_LIMIT_INPUT As String = "WM710008"

        ''' <summary>
        ''' 貸出日数を入力せずに、稼働日考慮はチェックできません。
        ''' </summary>
        Public Const MSG_ERR_CANNOT_CHECK_MAINTE_LEASE_DAY_WORK_FLAG As String = "WM710009"

        ''' <summary>
        ''' メンテナンス日数を入力せずに、稼働日考慮はチェックできません。
        ''' </summary>
        Public Const MSG_ERR_CANNOT_CHECK_MAINTE_DAY_WORK_FLAG As String = "WM710010"
    End Class

    ''' <summary>
    ''' ロケーションマスタ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelLocation
        ''' <summary>
        ''' ロケーションマスタメンテナンス:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT720001"

        ''' <summary>
        ''' 管理資産棚卸区分
        ''' </summary>
        Public Const LABEL_OWN_INV_FLG As String = "WI720001"

        ''' <summary>
        ''' 管理資産のみ
        ''' </summary>
        Public Const LABEL_OWN_INV_ONLY As String = "WI720002"

        ''' <summary>
        ''' RFIDタグ拠点に変更されました。資産マスタを削除しますか？
        ''' </summary>
        Public Const MSG_ALERT_DEL_ASSET As String = "WM720003"

        ''' <summary>
        ''' 項目配置マスタの登録が正常終了しました。
        ''' </summary>
        Public Const MSG_INFO_MSTCOL_INS As String = "WM720004"

        ''' <summary>
        ''' 資産の削除が正常終了しました。
        ''' </summary>
        Public Const MSG_INFO_ASSET_DEL As String = "WM720005"

        ''' <summary>
        ''' CSV取込ではバーコード運用拠点からRFIDタグ拠点に変更できません。。
        ''' </summary>
        Public Const MSG_ERR_RFID_CHANGE As String = "WM720006"

    End Class

    ''' <summary>
    ''' 部署マスタ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelDepartment
        ''' <summary>
        ''' 部署マスタメンテナンス:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT730001"
    End Class

    ''' <summary>
    ''' 社員マスタ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelEmployee
        ''' <summary>
        ''' 社員マスタメンテナンス:タイトル
        ''' </summary>
        Public Const LABEL_TITLE As String = "WT740001"
        ''' <summary>
        ''' 本部区分は変更できません。
        ''' </summary>
        Public Const MSG_ERR_NOT_EDIT_DEPARTMENT_CD As String = "WM740001"
    End Class

    ''' <summary>
    ''' 貸出先マスタ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelLeasePlace
        ''' <summary>
        ''' 貸出先マスタメンテナンス:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT750001"
    End Class

    ''' <summary>
    ''' 文言設定マスタ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelWord
        ''' <summary>
        ''' 文言設定マスタメンテナンス:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT920001"

        ''' <summary>
        ''' 文言設定マスタメンテナンス:文言区分
        ''' </summary>
        Public Const LABEL_WORD_DIV As String = "WI910001"

        ''' <summary>
        ''' 文言設定マスタメンテナンス:文面コード
        ''' </summary>
        Public Const LABEL_WORD_CODE As String = "WI910002"

        ''' <summary>
        ''' 文言設定マスタメンテナンス:ディフォルト英語
        ''' </summary>
        Public Const LABEL_DEFAULT_EN As String = "WI910003"

        ''' <summary>
        ''' 文言設定マスタメンテナンス:ディフォルト日本語
        ''' </summary>
        Public Const LABEL_DEFAULT_JA As String = "WI910004"

        ''' <summary>
        ''' 文言設定マスタメンテナンス:表示名
        ''' </summary>
        Public Const LABEL_DISPLAY_NAME As String = "WI910005"
    End Class

    ''' <summary>
    ''' 設定マスタ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelSetting

        ''' <summary>
        ''' 設定マスタメンテナンス:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT910001"

        '2024/4/18 Add Start Iseda
        ''' <summary>
        ''' 設定マスタメンテナンス:機能分類名
        ''' </summary>
        Public Const LABEL_FUNC_CAT_NAME As String = "WI920016"
        '2024/4/18 Add END Iseda

        ''' <summary>
        ''' 設定マスタメンテナンス:種別
        ''' </summary>
        Public Const LABEL_KEY_DIV As String = "WI920001"

        ''' <summary>
        ''' 設定マスタメンテナンス:KEY1
        ''' </summary>
        Public Const LABEL_KEY1 As String = "WI920002"

        ''' <summary>
        ''' 設定マスタメンテナンス:KEY2
        ''' </summary>
        Public Const LABEL_KEY2 As String = "WI920003"

        ''' <summary>
        ''' 設定マスタメンテナンス:KEY3
        ''' </summary>
        Public Const LABEL_KEY3 As String = "WI920004"

        ''' <summary>
        ''' 設定マスタメンテナンス:備考
        ''' </summary>
        Public Const LABEL_REMARK As String = "WI920005"

        ''' <summary>
        ''' 設定マスタメンテナンス:内容1
        ''' </summary>
        Public Const LABEL_DATA1 As String = "WI920006"

        ''' <summary>
        ''' 設定マスタメンテナンス:内容2
        ''' </summary>
        Public Const LABEL_DATA2 As String = "WI920007"

        ''' <summary>
        ''' 設定マスタメンテナンス:内容3
        ''' </summary>
        Public Const LABEL_DATA3 As String = "WI920008"

        ''' <summary>
        ''' 設定マスタメンテナンス:内容4
        ''' </summary>
        Public Const LABEL_DATA4 As String = "WI920009"

        ''' <summary>
        ''' 設定マスタメンテナンス:内容5
        ''' </summary>
        Public Const LABEL_DATA5 As String = "WI920010"

        ''' <summary>
        ''' 設定マスタメンテナンス:内容6
        ''' </summary>
        Public Const LABEL_DATA6 As String = "WI920011"

        ''' <summary>
        ''' 設定マスタメンテナンス:内容7
        ''' </summary>
        Public Const LABEL_DATA7 As String = "WI920012"

        ''' <summary>
        ''' 設定マスタメンテナンス:内容8
        ''' </summary>
        Public Const LABEL_DATA8 As String = "WI920013"

        ''' <summary>
        ''' 設定マスタメンテナンス:内容9
        ''' </summary>
        Public Const LABEL_DATA9 As String = "WI920014"

        ''' <summary>
        ''' 設定マスタメンテナンス:内容10
        ''' </summary>
        Public Const LABEL_DATA10 As String = "WI920015"

        '2024/4/18 Add Start Iseda
        ''' <summary>
        ''' 設定マスタメンテナンス:設定対象区分
        ''' </summary>
        Public Const LABEL_SETTING_OBJECT_DIV As String = "WI920021"

        ''' <summary>
        ''' 設定マスタメンテナンス:更新可否
        ''' </summary>
        Public Const LABEL_UPD_AUTHORITY_FLG As String = "WI920022"

        ''' <summary>
        ''' 設定マスタメンテナンス:削除可否
        ''' </summary>
        Public Const LABEL_DEL_AUTHORITY_FLG As String = "WI920023"

        ''' <summary>
        ''' 設定マスタメンテナンス:設定対象区分 HT
        ''' </summary>
        Public Const LABEL_SETTING_OBJECT_DIV_HT As String = "WI920029"

        ''' <summary>
        ''' 設定マスタメンテナンス:設定対象区分 PC
        ''' </summary>
        Public Const LABEL_SETTING_OBJECT_DIV_PC As String = "WI920030"

        ''' <summary>
        ''' 設定マスタメンテナンス:設定対象区分 WEB
        ''' </summary>
        Public Const LABEL_SETTING_OBJECT_DIV_WEB As String = "WI920031"

        ''' <summary>
        ''' 設定マスタメンテナンス:設定対象区分 ALL
        ''' </summary>
        Public Const LABEL_SETTING_OBJECT_DIV_ALL As String = "WI920032"

        ''' <summary>
        ''' 設定マスタメンテナンス:機能分類名1
        ''' </summary>
        Public Const LABEL_FUNC_CAT_NAME1 As String = "WI920017"

        ''' <summary>
        ''' 設定マスタメンテナンス:機能分類名2
        ''' </summary>
        Public Const LABEL_FUNC_CAT_NAME2 As String = "WI920018"

        ''' <summary>
        ''' 設定マスタメンテナンス:機能分類名3
        ''' </summary>
        Public Const LABEL_FUNC_CAT_NAME3 As String = "WI920019"

        ''' <summary>
        ''' 設定マスタメンテナンス:機能分類_並び順
        ''' </summary>
        Public Const LABEL_FUNC_CAT_SORT As String = "WI920020"

        ''' <summary>
        ''' 設定マスタメンテナンス:更新可否 更新可能
        ''' </summary>
        Public Const LABEL_UPD_AUTHORITY_FLG_Y As String = "WI920025"

        ''' <summary>
        ''' 設定マスタメンテナンス:更新可否 更新不可
        ''' </summary>
        Public Const LABEL_UPD_AUTHORITY_FLG_N As String = "WI920026"

        ''' <summary>
        ''' 設定マスタメンテナンス:削除可否 削除可能
        ''' </summary>
        Public Const LABEL_DEL_AUTHORITY_FLG_Y As String = "WI920027"

        ''' <summary>
        ''' 設定マスタメンテナンス:削除可否 削除不可
        ''' </summary>
        Public Const LABEL_DEL_AUTHORITY_FLG_N As String = "WI920028"
        '2024/4/18 Add END Iseda

        ''' <summary>
        ''' このIDは既に登録済みです。
        ''' </summary>
        Public Const MSG_ERR_ID_REGISTERED As String = "WM800003"

        '2024/4/18 Add Start Iseda
        ''' <summary>
        ''' 削除権限がない設定が選択されています。
        ''' </summary>
        Public Const MSG_ERR_ID_NO_DELETE_AUTHORITY As String = "WM920001"

        ''' <summary>
        ''' データを削除すると種別の設定が0件になるデータが選択されています。削除しますか？
        ''' </summary>
        Public Const MSG_ERR_ID_DATA_ONLY_ONE As String = "WM920002"

        ''' <summary>
        ''' 機能分類_並び順が重複しています。
        ''' </summary>
        Public Const MSG_ERR_ID_FNC_CAT_ORDER_DUPLICATION As String = "WM920003"

        ''' <summary>
        ''' 設定対象区分を選択してください。
        ''' </summary>
        Public Const MSG_ERR_ID_SELECT_SETTING_OBJECT_DIV As String = "WM920004"
        '2024/4/18 Add END Iseda

        ''' <summary>
        ''' ロケQRを読取りしてください。
        ''' </summary>
        Public Const MSG_READ_LOCATION_QR As String = "WM920005"

        ''' <summary>
        ''' このロケーションは登録されていません。
        ''' </summary>
        Public Const MSG_LOCATION_NOT_REGISTERE As String = "WM920006"

        ''' <summary>
        ''' ロケーションQRではありません。
        ''' </summary>
        Public Const MSG_NOT_LOCATION As String = "WM920007"

    End Class

    ''' <summary>
    ''' カレンダーマスタ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelCalendar
        ''' <summary>
        ''' カレンダーマスタメンテナンス:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT760001"
    End Class

    ''' <summary>
    ''' 貸出先一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelAssetLeasePlaceList
        ''' <summary>
        ''' 貸出先一覧:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT310001"

        ''' <summary>
        ''' 貸出先一覧:貸出数
        ''' </summary>
        Public Const LABEL_LEASE_COUNT As String = "WI310001"

        ''' <summary>
        ''' 貸出先一覧:期限切れ
        ''' </summary>
        Public Const LABEL_LEASE_LIMIT_EXPIRED As String = "WI310002"

    End Class

    ''' <summary>
    ''' 貸出先資産照会画面用定義クラス
    ''' </summary>
    Public Class CommonLabelAssetLeasePlaceDetail
        ''' <summary>
        ''' 貸出先資産照会:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT320001"

    End Class

    Public Class CommonLabelColSet
        ''' <summary>
        ''' 項目配置マスタメンテナンス:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT780001"

        ''' <summary>
        ''' 画面名
        ''' </summary>
        Public Const LABEL_DISP_NAME As String = "WI780001"

        ''' <summary>
        ''' 所属拠点(会社) 
        ''' </summary>
        Public Const LABEL_HOME_BASE As String = "WI780006"

        ''' <summary>
        ''' 所属拠点(会社) 
        ''' </summary>
        Public Const BUTTON_ALL_BASE_UPDATA As String = "WB780001"

        ''' <summary>
        ''' 全拠点
        ''' </summary>
        Public Const LABEL_HOME_BASE_ALL As String = "WI780007"

        ''' <summary>
        ''' テンプレート
        ''' </summary>
        Public Const LABEL_TEMPLATE As String = "WI780008"

        ''' <summary>
        ''' 固定列数
        ''' </summary>
        Public Const LABEL_FIX_COL_CNT As String = "WI780002"

        ''' <summary>
        ''' 並び順
        ''' </summary>
        Public Const LABEL_DISP_ORDER As String = "WI780003"

        ''' <summary>
        ''' 表示
        ''' </summary>
        Public Const LABEL_DISP As String = "WI780004"

        ''' <summary>
        ''' 項目名
        ''' </summary>
        Public Const LABEL_COL_NAME As String = "WI780005"

    End Class

    ''' <summary>
    ''' 貸出履歴照会画面用定義クラス
    ''' </summary>
    Public Class CommonLabelAssetLeaseHitory
        ''' <summary>
        ''' 貸出履歴照会:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT330001"
    End Class

    ''' <summary>
    ''' 予約状況照会画面用定義クラス
    ''' </summary>
    Public Class CommonLabelLendingStatus
        ''' <summary>
        ''' 予約状況照会:タイトル
        ''' </summary>
        Public Const TITLE_LENDING_STATUS As String = "WT340001"

        ''' <summary>
        ''' 予約履歴:タイトル
        ''' </summary>
        Public Const TITLE_LENDING_HISTORY As String = "WT340002"

        ''' <summary>
        ''' 予約申請登録/更新:タイトル（登録）
        ''' </summary>
        Public Const TITLE_LENDING_TOUROKU As String = "WT340003"

        ''' <summary>
        ''' 予約申請登録/更新:タイトル（変更）
        ''' </summary>
        Public Const TITLE_LENDING_HENKOU As String = "WT340004"

        ''' <summary>
        ''' 予約メンテナンス:タイトル（登録）
        ''' </summary>
        Public Const TITLE_LENDING_MAINTE_TOUROKU As String = "WT340005"

        ''' <summary>
        ''' 予約メンテナンス:タイトル（変更）
        ''' </summary>
        Public Const TITLE_LENDING_MAINTE_HENKOU As String = "WT340006"

        ''' <summary>
        ''' 予約申請登録/更新:タイトル（詳細）
        ''' </summary>
        Public Const TITLE_LENDING_SHOSAI As String = "WT340007"

        ''' <summary>
        ''' 〇〇登録:タイトル（登録）
        ''' </summary>
        Public Const TITLE_BLE_TOUROKU As String = "WT340008"

        ''' <summary>
        ''' 〇〇変更:タイトル（変更）
        ''' </summary>
        Public Const TITLE_BLE_HENKOU As String = "WT340009"

        ''' <summary>
        ''' 予備登録１:タイトル（登録）
        ''' </summary>
        Public Const TITLE_PRE_TOUROKU1 As String = "WT340010"

        ''' <summary>
        ''' 予備変更１:タイトル（変更）
        ''' </summary>
        Public Const TITLE_PRE_HENKOU1 As String = "WT340011"

        ''' <summary>
        ''' 予備登録２:タイトル（登録）
        ''' </summary>
        Public Const TITLE_PRE_TOUROKU2 As String = "WT340012"

        ''' <summary>
        ''' 予備登録２:タイトル（変更）
        ''' </summary>
        Public Const TITLE_PRE_HENKOU2 As String = "WT340013"

        ''' <summary>
        ''' カゴに申請データがありません。
        ''' </summary>
        Public Const MSG_ERR_CART_NO_DATA As String = "WM340001"

        ''' <summary>
        ''' カゴに追加済の資産が選択されています。
        ''' </summary>
        Public Const MSG_ERR_ASSET_NO_ALREADY_ADDED As String = "WM340002"

        ''' <summary>
        ''' 稼働日が登録されていないため予約申請できません。
        ''' </summary>
        Public Const MSG_ERR_CANNOT_RESERVE_WORK_DAY_NOTHING As String = "WM340003"
    End Class

    ''' <summary>
    ''' 予約一覧画面用定義クラス
    ''' </summary>
    Public Class CommonLabelLendingList
        ''' <summary>
        ''' 予約一覧:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT350001"
    End Class

    ''' <summary>
    ''' ゲートマスタ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelGate
        ''' <summary>
        ''' ゲートマスタ:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT610001"

        '20220511 ADD Start Kawamoto
        ''' <summary>
        ''' 周波数は最低１つ入力してください。
        ''' </summary>
        Public Const MSG_ERR_FREQUENCY_NO_INPUT As String = "WM610001"

        ''' <summary>
        ''' IO No.1とIO No.2のGPOポートは違うポートを選択してください。
        ''' </summary>
        Public Const MSG_ERR_SELECT_DIFFERENT_GPO_PORT As String = "WM610002"

        ''' <summary>
        ''' IO No.1とIO No.2のbit位置は違うbitを指定してください。
        ''' </summary>
        Public Const MSG_ERR_SELECT_DIFFERENT_BIT_POS As String = "WM610003"
        '20220511 ADD End Kawamoto

    End Class

    ''' <summary>
    ''' ゲート持ち出し履歴画面用定義クラス
    ''' </summary>
    Public Class CommonLabelGatePassLog
        ''' <summary>
        ''' ゲートマスタ:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT620001"

    End Class

    ''' <summary>
    ''' 商品マスタ画面用定義クラス
    ''' </summary>
    Public Class CommonLabelGoods
        ''' <summary>
        ''' 商品マスタメンテナンス:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT770001"
    End Class

    ''' <summary>
    ''' CSVヘッダ文字列定義クラス
    ''' </summary>
    Public Class CommonLabelCsvHedder
        ''' <summary>
        ''' 社員マスタ画面
        ''' </summary>
        Public Const LABEL_EMPLOYEE As String = "WI980001"

        ''' <summary>
        ''' 部署マスタ画面
        ''' </summary>
        Public Const LABEL_DEPARTMENT As String = "WI980002"

        ''' <summary>
        ''' カテゴリーマスタ画面
        ''' </summary>
        Public Const LABEL_CATEGORY As String = "WI980003"

        ''' <summary>
        ''' ロケーションマスタ画面
        ''' </summary>
        Public Const LABEL_LOCATION As String = "WI980004"

        ''' <summary>
        ''' 貸出先マスタ画面
        ''' </summary>
        Public Const LABEL_LEASE_PLACE As String = "WI980005"

        ''' <summary>
        ''' 文言マスタ画面
        ''' </summary>
        Public Const LABEL_WORD As String = "WI980006"

        ''' <summary>
        ''' 資産一覧(CSV出力：全件対象)
        ''' </summary>
        Public Const LABEL_ASSET_ALL As String = "WI980007"

        ''' <summary>
        ''' 資産履歴照会画面
        ''' </summary>
        Public Const LABEL_ASSET_HISTORY As String = "WI980010"

        ''' <summary>
        ''' 在庫照会画面
        ''' </summary>
        Public Const LABEL_STOCK_LIST As String = "WI980009"

        ''' <summary>
        ''' 棚卸概況
        ''' </summary>
        Public Const LABEL_INVENTORY_SUMMARY As String = "WI980011"

        ''' <summary>
        ''' 実棚照会
        ''' </summary>
        Public Const LABEL_INVENTORY_INQUIRY As String = "WI980012"

        ''' <summary>
        ''' 棚卸実績一覧
        ''' </summary>
        Public Const LABEL_INVENTORY_HISTORY As String = "WI980013"

        ''' <summary>
        ''' 棚卸実績照会
        ''' </summary>
        Public Const LABEL_INVENTORY_ASSET_HISTORY As String = "WI980014"

        ''' <summary>
        ''' 履歴照会(資産)
        ''' </summary>
        Public Const LABEL_ASSET_DETAIL As String = "WI980015"

        ''' <summary>
        ''' 資産一覧(CSV出力)
        ''' </summary>
        Public Const LABEL_ASSET_ICHIRAN As String = "WI980016"

        ''' <summary>
        ''' ゲート持ち出し履歴画面
        ''' </summary>
        Public Const LABEL_GATE_PASS_LOG As String = "WI980018"

        ''' <summary>
        ''' 貸出先資産照会画面
        ''' </summary>
        Public Const LABEL_ASSET_LEASE_DETAIL As String = "WI980019"

        ''' <summary>
        ''' 貸出先一覧画面
        ''' </summary>
        Public Const LABEL_ASSET_LEASE As String = "WI980020"

        ''' <summary>
        ''' カレンダーマスタ画面
        ''' </summary>
        Public Const LABEL_CALENDAR As String = "WI980021"

        ''' <summary>
        ''' 貸出履歴照会画面
        ''' </summary>
        Public Const LABEL_ASSET_LEASE_HISTORY As String = "WI980022"

        ''' <summary>
        ''' 予約一覧画面
        ''' </summary>
        Public Const LABEL_LENDING_LIST As String = "WI980023"

        ''' <summary>
        ''' 予約状況照会画面(予約照会一覧)
        ''' </summary>
        Public Const LABEL_LENDING_STATUS_REF_LIST As String = "WI980024"

        ''' <summary>
        ''' 予約状況照会画面(予約照会メンテナンス)
        ''' </summary>
        Public Const LABEL_LENDING_STATUS_REF_MAINTE As String = "WI980025"

        ''' <summary>
        ''' 予約履歴画面
        ''' </summary>
        Public Const LABEL_LENDING_HISTORY As String = "WI980026"

        ''' <summary>
        ''' 資産一覧(CSV出力：差分取込)
        ''' </summary>
        Public Const LABEL_ASSET_DIFFERENCE As String = "WI980027"

        '2023/05/10 Kawamoto ADD START
        ''' <summary>
        ''' 資産履歴照会画面(薬品機能ON)
        ''' </summary>
        Public Const LABEL_ASSET_HISTORY_DRUG_FUNC As String = "WI980028"

        ''' <summary>
        ''' 履歴照会(資産)(薬品機能ON)
        ''' </summary>
        Public Const LABEL_ASSET_DETAIL_DRUG_FUNC As String = "WI980029"
        '2023/05/10 Kawamoto ADD END

        '20231024 ADD Start Shirakawa
        ''' <summary>
        ''' 棚卸実績ロケ別一覧
        ''' </summary>
        Public Const LABEL_INVENTORY_LOC_HISTORY As String = "WI980030"
        '20231025 ADD End Shirakawa
        ''' <summary>
        ''' 工事一覧
        ''' </summary>
        Public Const LABEL_CONSTRUCTION_LIST As String = "WI980031"
        ''' <summary>
        ''' 仮置一覧
        ''' </summary>
        Public Const LABEL_TEMPORARY_LIST As String = "WI980032"
        ''' <summary>
        ''' 仮置資産一覧
        ''' </summary>
        Public Const LABEL_TEMP_ASSET_LIST As String = "WI980033"
        ''' <summary>
        ''' 工事資産一覧
        ''' </summary>
        Public Const LABEL_CONST_ASSET_LIST As String = "WI980034"
        ''' <summary>
        ''' 工事履歴一覧
        ''' </summary>
        Public Const LABEL_CONSTRUCTION_HIST As String = "WI980035"
        ''' <summary>
        ''' 工事資産履歴一覧
        ''' </summary>
        Public Const LABEL_CONST_ASSET_HIST As String = "WI980036"
    End Class

    Public Class CommonInfoMsg

        ''' <summary>
        ''' 削除してもよろしいですか？
        ''' </summary>
        Public Const DELETE_CONFIRM As String = "WM991001"

        ''' <summary>
        ''' 全データCSV出力します。よろしいですか？
        ''' </summary>
        Public Const CSVEXPORT_ALL_CONFIRM As String = "WM991002"

        ''' <summary>
        ''' CSV出力します。よろしいですか？
        ''' </summary>
        Public Const CSVEXPORT_CONFIRM As String = "WM991003"

        ''' <summary>
        ''' 登録します。よろしいですか？
        ''' </summary>
        Public Const CREATE_CONFIRM As String = "WM991004"

        ''' <summary>
        ''' 変更します。よろしいですか？
        ''' </summary>
        Public Const EDIT_CONFIRM As String = "WM991005"

        ''' <summary>
        ''' 除却します。よろしいですか？
        ''' </summary>
        Public Const RETIRE_CONFIRM As String = "WM991006"

        ''' <summary>
        ''' 除却解除します。よろしいですか？
        ''' </summary>
        Public Const RETIRE_OFF_CONFIRM As String = "WM991007"

        ''' <summary>
        ''' QR台帳を出力します。よろしいですか？
        ''' </summary>
        Public Const QRDAICHO_CONFIRM As String = "WM991008"

        ''' <summary>
        ''' 棚卸処理を開始します。よろしいですか？
        ''' </summary>
        Public Const INV_START_CONFIRM As String = "WM991009"

        ''' <summary>
        ''' 棚卸処理を確定します。よろしいですか？
        ''' </summary>
        Public Const INV_KAKUTEI_CONFIRM As String = "WM991010"

        ''' <summary>
        ''' 発行します。よろしいですか？
        ''' </summary>
        Public Const HAKKO_CONFIRM As String = "WM991011"

        ''' <summary>
        ''' 100件以上選択されています。QR台帳の作成に時間がかかりますが、よろしいですか？
        ''' </summary>
        Public Const EXC_QRDAICHO_CONFIRM As String = "WM991012"

        ''' <summary>
        ''' 選択されたファイルを全て削除します。よろしいですか？
        ''' </summary>
        Public Const SELECT_FILE_DELETE_CONFIRM As String = "WM991013"

        ''' <summary>
        ''' 帳票を出力します。よろしいですか？  20230430 ADD Sato v2.4 
        ''' </summary>
        Public Const REPORT_OUTPUT_COFIRM As String = "WM991014"

        ''' <summary>
        ''' 管理簿確認を実施します。よろしいですか？  20230430 ADD Sato v2.4 
        ''' </summary>
        Public Const DRUG_MANEGE_CHECK_CONFIRM As String = "WM991015"

        ''' <summary>
        ''' 全拠点の設定を変更します。よろしいですか？
        ''' </summary>
        Public Const EDIT_CONFIRM_ALL As String = "WM990245"

        ''' <summary>
        ''' データを出力します。よろしいですか？
        ''' </summary>
        Public Const DATA_EXPORT_CONFIRM As String = "WM990251"

    End Class

    ''' <summary>
    ''' ライセンス認証画面用定義クラス
    ''' </summary>
    Public Class CommonLabelLicense
        ''' <summary>
        ''' ライセンス認証:タイトル
        ''' </summary>
        Public Const TITLE As String = "WT940001"

        ''' <summary>
        ''' キー取得
        ''' </summary>
        Public Const BUTTON_GET_KEY As String = "WB940001"

        ''' <summary>
        ''' 試用
        ''' </summary>
        Public Const BUTTON_TRIAL As String = "WB940002"

        ''' <summary>
        ''' 登録
        ''' </summary>
        Public Const BUTTON_REGIST As String = "WB940003"

        ''' <summary>
        ''' プロダクト番号
        ''' </summary>
        Public Const LABEL_NO As String = "WI940001"

        ''' <summary>
        ''' 試用期間 残り:
        ''' </summary>
        Public Const LABEL_TRIAL1 As String = "WI940002"

        ''' <summary>
        ''' 日
        ''' </summary>
        Public Const LABEL_TRIAL2 As String = "WI940003"

        ''' <summary>
        ''' 入力されたプロダクトキーは、正しくありません。
        ''' </summary>
        Public Const MSG_ERR_PRODUCT_KEY As String = "WM940001"

        ''' <summary>
        ''' プロダクトキーが認証されました。
        ''' </summary>
        Public Const MSG_OK_PRODUCT_KEY As String = "WM940002"

        ''' <summary>
        ''' タイムアウトになりました。画面を再表示します。
        ''' </summary>
        Public Const MSG_REDISPLAY As String = "WM940003"

    End Class



    Public Class CommonErrorMsg

        ''' <summary>
        ''' 使用されているカテゴリーのため、削除できません。
        ''' </summary>
        Public Const MSG_ERR_USED_CATEGORY As String = "WM710001"

        ''' <summary>
        ''' 分類区分が異常です。
        ''' </summary>
        Public Const MSG_ERR_CAT_DIV As String = "WM710002"

        ''' <summary>
        ''' 大分類のデータに「上位分類に準じる」は設定できません。
        ''' </summary>
        Public Const MSG_ERR_ALERT_DIV As String = "WM710003"

        ''' <summary>
        ''' 使用されているロケーションのため、削除できません。
        ''' </summary>
        Public Const MSG_ERR_USED_LOCATION As String = "WM720001"

        ''' <summary>
        ''' ロケ区分が異常です。
        ''' </summary>
        Public Const MSG_ERR_LOC_DIV As String = "WM720002"

        ''' <summary>
        ''' 使用されている部署のため、削除できません。
        ''' </summary>
        Public Const MSG_ERR_USED_DEPARTMENT As String = "WM730001"

        Public Const OTHER_MACHINE_UPDATE As String = "WM990002"
        ''' <summary>
        ''' チェックボックス未選択時のエラーメッセージ
        ''' </summary>
        Public Const CHECKBOX_NO_SELECTED As String = "WM990003"
        ''' <summary>
        ''' チェックボックス複数選択時のエラーメッセージ
        ''' </summary>
        Public Const CHECKBOX_OVER_SELECTED As String = "WM990004"
        ''' <summary>
        ''' 実棚ステータス条件非選択時のエラーメッセージ
        ''' </summary>
        Public Const PHYINV_STATUS_CHECKBOX_NO_SELECTED As String = "WM210004"
        ''' <summary>
        ''' 期間(From)を入力して下さい。
        ''' </summary>
        Public Const MSG_ERR_NOT_ENTERED_DATE_FROM As String = "WM990022"
        ''' <summary>
        ''' 期間(To)を入力して下さい。
        ''' </summary>
        Public Const MSG_ERR_NOT_ENTERED_DATE_TO As String = "WM990023"
        ''' <summary>
        ''' 期間前後エラー。
        ''' </summary>
        Public Const MSG_ERR_DATE_ORDER As String = "WM990024"

        ''' <summary>
        ''' ロケ違い以外が選択されている時のエラーメッセージ
        ''' </summary>
        Public Const PHYINV_STATUS_CHECKBOX_NOTSELWARNING As String = "WM210005"

        ''' <summary>
        ''' 棚卸中ではないロケに保管した資産が選択されています。
        ''' </summary>
        Public Const PHYINV_STATUS_CHECKBOX_NOTINVENTORY As String = "WM990207"

        ''' <summary>
        ''' ロケーション(会社)を選択して下さい。
        ''' </summary>
        Public Const MSG_ERRNO_SELECTED_LOC_L As String = "WM990224"

        ''' <summary>
        ''' 少なくとも1件は会社、部署を選択して下さい。
        ''' </summary>
        Public Const MSG_ERRNO_SELECTED_HUB_LOC As String = "WM990225"

        ''' <summary>
        ''' 部署を選択して下さい。
        ''' </summary>
        Public Const MSG_ERRNO_EMPLOYEE_HUB_LOC_M As String = "WM990226"

        ''' <summary>
        ''' 少なくとも1件は会社、部署を入力して下さい。
        ''' </summary>
        Public Const MSG_ERRNO_SELECTED_HUB_LOC_IMP As String = "WM990227"

        ''' <summary>
        ''' ロケーションマスタ、もしくは所属拠点に原価センタが登録されていません。
        ''' </summary>
        Public Const MSG_ERRNO_EMPLOYEE_HUB_NOT_EXISTS As String = "WM990228"

        ''' <summary>
        ''' 会社コードと部署コードはセットで入力してください。
        ''' </summary>
        Public Const MSG_ERRNO_EMPLOYEE_HUB_LOC_SET As String = "WM990229"

        ''' <summary>
        ''' 会社を選択してください。
        ''' </summary>
        Public Const MSG_ERRNO_SELECT_HUB_LOC_L As String = "WM990230"

        ''' <summary>
        ''' 他拠点で登録済みの社員コードです。
        ''' </summary>
        Public Const MSG_ERRNO_EMPLOYEE_OTHER_HUB As String = "WM990231"

        ''' <summary>
        ''' 資産番号、サブ資産番号が重複しています。
        ''' </summary>
        Public Const MSG_ERRNO_ASSETNO_JUUFUKU As String = "WM990232"

        ''' <summary>
        ''' ロケーションマスタに場所コードが登録されていません。
        ''' </summary>
        Public Const MSG_ERRNO_NO_LOC_S As String = "WM990248"

    End Class

    Public Class CommonErrorMsgMember

        Public Const NOT_ENTERD_ID As String = "WM800001"

        Public Const NOT_ENTERD_NAME As String = "WM800002"

        Public Const DUPLICATION_ID As String = "WM800003"

        Public Const MAXLENGTH_ID As String = "WM800004"

        Public Const MAXLENGTH_NAME As String = "WM800005"

        Public Const MAXLENGTH_EMAIL As String = "WM800006"

        Public Const NOT_FORMAT_DATE As String = "WM800007"

    End Class


    '20241121 ADD Start Iseda
    ''' <summary>
    ''' CSVヘッダ機能IDクラス
    ''' </summary>
    Public Class CommonCsvHedderFundtionId
        ''' <summary>
        ''' 社員マスタ画面
        ''' </summary>
        Public Const FUNCID_EMPLOYEE As String = "01"

        ''' <summary>
        ''' 部署マスタ画面
        ''' </summary>
        Public Const FUNCID_DEPARTMENT As String = "02"

        ''' <summary>
        ''' カテゴリーマスタ画面
        ''' </summary>
        Public Const FUNCID_CATEGORY As String = "03"

        ''' <summary>
        ''' ロケーションマスタ画面
        ''' </summary>
        Public Const FUNCID_LOCATION As String = "04"

        ''' <summary>
        ''' 貸出先マスタ画面
        ''' </summary>
        Public Const FUNCID_LEASE_PLACE As String = "05"

        ''' <summary>
        ''' 文言マスタ画面
        ''' </summary>
        Public Const FUNCID_WORD As String = "06"

        ''' <summary>
        ''' 資産一覧(CSV出力：全件対象)
        ''' </summary>
        Public Const FUNCID_ASSET_ALL As String = "07"

        ''' <summary>
        ''' 在庫照会画面
        ''' </summary>
        Public Const FUNCID_STOCK_LIST As String = "08"

        ''' <summary>
        ''' 資産履歴照会画面
        ''' </summary>
        Public Const FUNCID_ASSET_HISTORY As String = "09"

        ''' <summary>
        ''' 棚卸概況
        ''' </summary>
        Public Const FUNCID_INVENTORY_SUMMARY As String = "10"

        ''' <summary>
        ''' 実棚照会
        ''' </summary>
        Public Const FUNCID_INVENTORY_INQUIRY As String = "11"

        ''' <summary>
        ''' 棚卸実績一覧
        ''' </summary>
        Public Const FUNCID_INVENTORY_HISTORY As String = "12"

        ''' <summary>
        ''' 棚卸実績照会
        ''' </summary>
        Public Const FUNCID_INVENTORY_ASSET_HISTORY As String = "13"

        ''' <summary>
        ''' 履歴照会(資産)
        ''' </summary>
        Public Const FUNCID_ASSET_DETAIL As String = "14"

        ''' <summary>
        ''' 資産一覧(CSV出力)
        ''' </summary>
        Public Const FUNCID_ASSET_ICHIRAN As String = "15"

        ''' <summary>
        ''' ゲート持ち出し履歴画面
        ''' </summary>
        Public Const FUNCID_GATE_PASS_LOG As String = "16"

        ''' <summary>
        ''' 貸出先資産照会画面
        ''' </summary>
        Public Const FUNCID_ASSET_LEASE_DETAIL As String = "17"

        ''' <summary>
        ''' 貸出先一覧画面
        ''' </summary>
        Public Const FUNCID_ASSET_LEASE As String = "18"

        ''' <summary>
        ''' カレンダーマスタ画面
        ''' </summary>
        Public Const FUNCID_CALENDAR As String = "19"

        ''' <summary>
        ''' 貸出履歴照会画面
        ''' </summary>
        Public Const FUNCID_ASSET_LEASE_HISTORY As String = "20"

        ''' <summary>
        ''' 予約一覧画面
        ''' </summary>
        Public Const FUNCID_LENDING_LIST As String = "21"

        ''' <summary>
        ''' 予約状況照会画面(予約照会一覧)
        ''' </summary>
        Public Const FUNCID_LENDING_STATUS_REF_LIST As String = "22"

        ''' <summary>
        ''' 予約状況照会画面(予約照会メンテナンス)
        ''' </summary>
        Public Const FUNCID_LENDING_STATUS_REF_MAINTE As String = "23"

        ''' <summary>
        ''' 予約履歴画面
        ''' </summary>
        Public Const FUNCID_LENDING_HISTORY As String = "24"

        ''' <summary>
        ''' 資産一覧(CSV出力：差分取込)
        ''' </summary>
        Public Const FUNCID_ASSET_DIFFERENCE As String = "25"

        ''' <summary>
        ''' 資産履歴照会画面(薬品機能ON)
        ''' </summary>
        Public Const FUNCID_ASSET_HISTORY_DRUG_FUNC As String = "26"

        ''' <summary>
        ''' 履歴照会(資産)(薬品機能ON)
        ''' </summary>
        Public Const FUNCID_ASSET_DETAIL_DRUG_FUNC As String = "27"

        ''' <summary>
        ''' 棚卸実績ロケ別一覧
        ''' </summary>
        Public Const FUNCID_INVENTORY_LOC_HISTORY As String = "28"

        ''' <summary>
        ''' 工事一覧
        ''' </summary>
        Public Const FUNCID_CONSTRUCTION_LIST As String = "29"

        ''' <summary>
        ''' 棚卸実績(ABUMN_MALSY)
        ''' </summary>
        Public Const FUNCID_CONSTRUCTION_ASSET_LIST As String = "30"

        ''' <summary>
        ''' 棚卸実績(AS02_MALSY)
        ''' </summary>
        Public Const FUNCID_CONSTRUCTION_HISTORY_LIST As String = "31"

        ''' <summary>
        ''' 資産ラベル発行
        ''' </summary>
        Public Const FUNCID_ASSET_LABEL_PRINT As String = "32"

        ''' <summary>
        ''' 工事履歴資産一覧
        ''' </summary>
        Public Const FUNCID_CONSTRUCTION_HISTORY_ASSET_LIST As String = "32"

        ''' <summary>
        ''' 仮置一覧
        ''' </summary>
        Public Const FUNCID_TEMPORARY_LIST As String = "33"

        ''' <summary>
        ''' 仮置資産一覧
        ''' </summary>
        Public Const FUNCID_TEMPORARY_ASSET_LIST As String = "34"

        ''' <summary>
        ''' 商品マスタ
        ''' </summary>
        Public Const FUNCID_GOODS As String = "35"

        ''' <summary>
        ''' 予約状況照会画面(予約照会一覧(照合用))
        ''' </summary>
        Public Const FUNCID_LENDING_STATUS_REF_LIST_MATCHING As String = "36"

    End Class
    '20241121 ADD End Iseda
End Namespace
