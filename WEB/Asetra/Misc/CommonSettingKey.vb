Namespace Misc

    Public Class CommonSettingKey

        ''' <summary>
        ''' ロケ使用区分
        ''' </summary>
        Public Class LocationUsage
            Public Const KeyDiv As String = "Location_usage_classification"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 実棚ロケ使用区分
        ''' </summary>
        Public Class ActualShelfLocation
            Public Const KeyDiv As String = "Actual_shelf_location"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 書込みタグEPC領域サイズ
        ''' </summary>
        Public Class TagSize
            Public Const KeyDiv As String = "Tag_size"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' カテゴリマスタの生成資産IDの選択初期値
        ''' </summary>
        Public Class AssetIDGenerationMethod
            Public Const KeyDiv As String = "Asset_ID_generation_method"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 資産ID生成方法
        ''' </summary>
        Public Class AssetIDGenerationUsage
            Public Const KeyDiv As String = "Asset_ID_generation_usage"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' EPC事業所別資産ID生成方法
        ''' </summary>
        Public Class AssetIDGenerationCoPrefix
            Public Const KeyDiv As String = "Asset_ID_generation_co_prefix"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 資産ID発番
        ''' </summary>
        Public Class AssetIdAutomatic
            Public Const KeyDiv As String = "Asset_ID_automatic_generation"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' ラベルプリンタ
        ''' </summary>
        Public Class LabelPrinter
            Public Const KeyDiv As String = "Label_Printer"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 文言マスタCSV出力ファイル名
        ''' </summary>
        Public Class Word
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "WORD"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 部署マスタCSV出力ファイル名
        ''' </summary>
        Public Class Department
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "DEPARTMENT"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 社員マスタCSV出力ファイル名
        ''' </summary>
        Public Class Employee
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "EMPLOYEE"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' ロケーションマスタCSV出力ファイル名
        ''' </summary>
        Public Class Location
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "LOCATION"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' ロケーションラベル発行CSV出力ファイル名
        ''' </summary>
        Public Class LocationLabel
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "LOCATION_LABEL"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 貸出先マスタCSV出力ファイル名
        ''' </summary>
        Public Class LeasePlace
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "LEASE"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' カテゴリーマスタCSV出力ファイル名
        ''' </summary>
        Public Class Category
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "CATEGORY"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' カレンダーマスタCSV出力ファイル名
        ''' </summary>
        Public Class Calendar
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "CALENDAR"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 在庫照会CSV出力ファイル名
        ''' </summary>
        Public Class StockList
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "STOCK"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 資産一覧CSV出力ファイル名
        ''' </summary>
        Public Class AssetList
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "ASSET"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 資産ラベル発行CSV出力ファイル名
        ''' </summary>
        Public Class AssetLabelPrint
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "ASSET_LABEL_PRINT"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class
        
        ''' <summary>
        ''' 履歴照会(資産）CSV出力ファイル名
        ''' </summary>
        Public Class AssetDetail
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "ASSET_DETAIL"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 棚卸概況CSV出力ファイル名
        ''' </summary>
        Public Class InventorySummary
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "INVENTORY_OVERVIEW"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 実棚照会CSV出力ファイル名
        ''' </summary>
        Public Class InventoryInquiry
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "INVENTORY_INQUIRY"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 棚卸実績CSV出力ファイル名
        ''' </summary>
        Public Class InventoryHistory
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "INVENTORY_RESULTS"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 棚卸実績照会CSV出力ファイル名
        ''' </summary>
        Public Class InventoryAssetHistory
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "INVENTORY_RESULTS_DETAIL"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        '20231024 ADD Start Shirakawa
        ''' <summary>
        ''' 棚卸実績ロケ別一覧CSV出力ファイル名
        ''' </summary>
        Public Class InventoryLocHistory
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "INVENTORY_LOC_RESULTS"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class
        '20231024 ADD End Shirakawa

        ''' <summary>
        ''' 資産履歴照会CSV出力ファイル名
        ''' </summary>
        Public Class AssetHistory
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "ASSET_HIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 資産一覧(QR台帳印刷)CSV出力ファイル名
        ''' </summary>
        Public Class AssetQRList
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "ASSET_QR_LIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 資産一覧(ラベル)CSV出力ファイル名
        ''' </summary>
        Public Class AssetLabel
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "ASSET_LABEL"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        'TODO:ログ照会CSV出力ファイル名は不要？
        ''' <summary>
        ''' ログ照会CSV出力ファイル名
        ''' </summary>
        Public Class OperationLog
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "ASSET_HIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 貸出先一覧CSV出力ファイル名
        ''' </summary>
        Public Class AssetLease
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "ASSET_LEASE"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' ゲート持ち出し履歴CSV出力ファイル名
        ''' </summary>
        Public Class GatePassLog
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "GATE_PASS_LOG"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 貸出先資産照会CSV出力ファイル名
        ''' </summary>
        Public Class AssetLeaseDetail
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "ASSET_LEASE_DETAIL"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約照会一覧CSVファイル名
        ''' </summary>
        Public Class LendingRefList
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "LENDING_REF_LIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約照会メンテナンスCSVファイル名
        ''' </summary>
        Public Class LendingRefMainte
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "LENDING_REF_MAINTE"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約履歴CSVファイル名
        ''' </summary>
        Public Class LendingHist
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "LENDING_HIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約一覧CSVファイル名
        ''' </summary>
        Public Class LendingList
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "LENDING_LIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        '20230430 ADD Sato v2.4 START
        ''' <summary>
        ''' 管理簿PDFファイル名
        ''' </summary>
        Public Class ManagePrint
            Public Const KeyDiv As String = "WEB_Output_Pdf_FileName"
            Public Const Key1 As String = "MANAGE_PRINT"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 指定数量管理表PDFファイル名
        ''' </summary>
        Public Class DirectPrint
            Public Const KeyDiv As String = "WEB_Output_Pdf_FileName"
            Public Const Key1 As String = "DIRECT_PRINT"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class
        '20230430 ADD Sato v2.4 END

        ''' <summary>
        ''' 工事一覧CSV出力ファイル名
        ''' </summary>
        Public Class ConstructionList
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "CONSTRUCTION_LIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 工事一覧PDF出力ファイル名
        ''' </summary>
        Public Class ConstPrint
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "CONST_PRINT"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 工事資産一覧CSV出力ファイル名
        ''' </summary>
        Public Class ConstAssetList
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "CONST_ASSET_LIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 工事履歴一覧CSV出力ファイル名
        ''' </summary>
        Public Class ConstructionHist
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "CONSTRUCTION_HIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 工事資産履歴一覧CSV出力ファイル名
        ''' </summary>
        Public Class ConstAssetHist
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "CONST_ASSET_HIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 仮置一覧CSV出力ファイル名
        ''' </summary>
        Public Class TemporaryList
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "TEMPORARY_LIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 仮置資産一覧CSV出力ファイル名
        ''' </summary>
        Public Class TempAssetList
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "TEMP_ASSET_LIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class
        ''' <summary>
        ''' 商品マスタCSV出力ファイル名
        ''' </summary>
        Public Class Goods
            Public Const KeyDiv As String = "WEB_Output_Csv_FileName"
            Public Const Key1 As String = "GOODS"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 写真保存先フォルダ
        ''' </summary>
        Public Class Image
            Public Const KeyDiv As String = "Save_to"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' データ保存期間
        ''' </summary>
        Public Class DeleteKikan
            Public Const KeyDiv As String = "Data_retention_period"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"

            Public Class Key1
                ''' <summary>資産処理履歴T</summary>
                Public Const T_PROCESS_LOG As String = "DK"
                ''' <summary>過去棚卸実績T</summary>
                Public Const T_INV_RESULT As String = "DT"
                ''' <summary>操作ｲﾍﾞﾝﾄﾛｸﾞ</summary>
                Public Const T_OPERATION_LOG As String = "FI"
                ''' <summary>資産M除却</summary>
                Public Const ASSET_JOKYKU As String = "DP"
                ''' <summary>ゲート通過履歴</summary>
                Public Const T_GATE_PASS_LOG As String = "GL"
                ''' <summary>予約一覧テーブル</summary>
                Public Const T_RESERVATION_STAMP As String = "RS"
            End Class

            Public Class DATA1
                ''' <summary>年</summary>
                Public Const YY As String = "Y"
                ''' <summary>月</summary>
                Public Const MM As String = "M"
                ''' <summary>日</summary>
                Public Const DD As String = "D"
            End Class

        End Class

        ''' <summary>
        ''' CSV取込用 文字コード UTF
        ''' </summary>
        Public Class CsvMapUtf
            Public Const KeyDiv As String = "CSV_charset"
            Public Const Key1 As String = "utf-8"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' CSV取込用 文字コード shift_jis
        ''' </summary>
        Public Class CsvMapShiftJis
            Public Const KeyDiv As String = "CSV_charset"
            Public Const Key1 As String = "shift_jis"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' CSV取込データ削除設定
        ''' </summary>
        Public Class DeleteAsset
            Public Const KeyDiv As String = "Delete_Asset"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 資産台帳取込の資産削除時に警告を表示する件数
        ''' </summary>
        ''' <remarks>DATA1に件数を設定する。0以下の場合は警告を表示しない。</remarks>
        Public Class AssetDeleteAlertCount
            Public Const KeyDiv As String = "Asset_Delete_AlertCount"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' キー取得URL
        ''' </summary>
        Public Class GetkeyURL
            Public Const KeyDiv As String = "Authentication_Url"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' EPC事業所キー(GIAI)
        ''' </summary>
        Public Class AssetIDGenerationCoPrefixGIAI
            Public Const KeyDiv As String = "Asset_ID_generation_co_prefix"
            Public Const Key1 As String = "5"
            Public Const Key2 As String = "1"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' EPC事業所キー(GRAI)
        ''' </summary>
        Public Class AssetIDGenerationCoPrefixGRAI
            Public Const KeyDiv As String = "Asset_ID_generation_co_prefix"
            Public Const Key1 As String = "6"
            Public Const Key2 As String = "1"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' オプション ゲート機能(内容1：Y[利用する]/N[利用しない])
        ''' </summary>
        Public Class Gate_Setting
            Public Const KeyDiv As String = "Gate_Setting"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約機能
        ''' </summary>
        Public Class ReservationFunction
            Public Const KeyDiv As String = "Reservation_Function"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 権限タイプ
        ''' </summary>
        Public Class Authority_Type
            Public Const KeyDiv As String = "Authority_Type"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約申請の明細数
        ''' </summary>
        Public Class Lending_Number_Items
            Public Const KeyDiv As String = "Lending_Number_Items"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 送り状作成可否
        ''' </summary>
        Public Class Lending_Invoice_Function
            Public Const KeyDiv As String = "Lending_Invoice_Function"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 送り状作成
        ''' </summary>
        Public Class Lending_Invoice_Output
            Public Const KeyDiv As String = "Lending_Invoice_Output"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約 確認書出力
        ''' </summary>
        Public Class Lending_Confirm_Output
            Public Const KeyDiv As String = "Lending_Confirm_Output"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 確認ラベル作成可否
        ''' </summary>
        Public Class Lending_Label_Function
            Public Const KeyDiv As String = "Lending_Label_Function"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約 確認ラベル出力
        ''' </summary>
        Public Class Lending_Label_Output
            Public Const KeyDiv As String = "Lending_Label_Output"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約状況照会画面の設定
        ''' </summary>
        Public Class Lending_Status_Setting
            Public Const KeyDiv As String = "Lending_Status_Setting"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 貸出時に該当する予約の貸出開始日までのリードタイム
        ''' </summary>
        Public Class Lending_LT_ReserveData
            Public Const KeyDiv As String = "Lending_LT_ReserveData"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 貸出用途
        ''' </summary>
        Public Class Reservation_Method
            Public Const KeyDiv As String = "Reservation_Method"
            Public Const Key1 As String = "1"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 貸出用途
        ''' </summary>
        Public Class Mail_ASETRA_URL
            Public Const KeyDiv As String = "Mail_ASETRA_URL"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        '''商品マスタの資産への反映機能
        ''' </summary>
        Public Class GoodsReflect
            Public Const KeyDiv As String = "Goods_Reflect"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        '''商品マスタの資産への反映機能の項目
        ''' </summary>
        Public Class GoodsReflectItem
            Public Const KeyDiv As String = "Goods_Reflect_Item"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        '''期間延長回数
        ''' </summary>
        Public Class Lending_ChangedTimes
            Public Const KeyDiv As String = "Lending_ChangedTimes"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        '''返却リードタイム
        ''' </summary>
        Public Class Return_LeadTime
            Public Const KeyDiv As String = "Return_LeadTime"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        '''予約変更可能日数
        ''' </summary>
        Public Class Lending_Update_Enabled
            Public Const KeyDiv As String = "Lending_Update_Enabled"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 期間延長メールの送信
        ''' </summary>
        Public Class Mail_Period_Extension
            Public Const KeyDiv As String = "Mail_Period_Extension"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 期間延長メール送信を行う対象の資産を管理するロケ
        ''' </summary>
        Public Class Mail_Period_Extension_Loc
            Public Const KeyDiv As String = "Mail_Period_Extension_Loc"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 担当者変更通知メールの送信
        ''' </summary>
        Public Class Mail_Change_Person
            Public Const KeyDiv As String = "Mail_Change_Person"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class


        ''' <summary>
        ''' 担当者変更通知メール送信を行う対象の資産を管理するロケ
        ''' </summary>
        Public Class Mail_Change_Person_Loc
            Public Const KeyDiv As String = "Mail_Change_Person_Loc"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 出荷通知メールの送信
        ''' </summary>
        Public Class Mail_Shipping
            Public Const KeyDiv As String = "Mail_Shipping"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        '''WebApiのUrl
        ''' </summary>
        Public Class WebAPI_Url
            Public Const KeyDiv As String = "WebAPI_Url"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        '''貸出開始日LT
        ''' </summary>
        Public Class Lending_StartDt_LT
            Public Const KeyDiv As String = "Lending_StartDt_LT"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約機能表示切替
        ''' </summary>
        Public Class Reserve_Regist_Option
            Public Const KeyDiv As String = "Reservation_Regist_Option"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 貸出(PC機能)の貸出最大日数
        ''' </summary>
        Public Class Lease_Max_Days_PC
            Public Const KeyDiv As String = "Lease_Max_Days_PC"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        '2023/02/06 ADD Start Shirakawa
        ''' <summary>
        ''' ロケごとの資産登録上限数
        ''' </summary>
        Public Class Asset_Registration_Limit
            Public Const KeyDiv As String = "Asset_Registration_Limit"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class
        '2023/02/06 ADD End Shirakawa

        '2023/02/08 Kawamoto ADD START
        ''' <summary>
        ''' 資産CSVファイル取込全件対象
        ''' </summary>
        Public Class Asset_CSV_Regist_ALL
            Public Const KeyDiv As String = "Asset_CSV_Regist_ALL"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 薬品管理機能
        ''' </summary>
        Public Class Drugs_Function
            Public Const KeyDiv As String = "Drugs_Function"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 危険物の指定数量
        ''' </summary>
        Public Class Drugs_FSAct_Number
            Public Const KeyDiv As String = "Drugs_FSAct_Number"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 計量器
        ''' </summary>
        Public Class Drugs_Weighing_Machine
            Public Const KeyDiv As String = "Drugs_Weighing_Machine"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class
        '2023/02/08 Kawamoto ADD END

        '2023/10/17 Sato ADD  v2.5 START
        Public Class QR_Ledger_Type
            Public Const KeyDiv As String = "QR_Ledger_Type"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class
        '2023/10/17 Sato ADD  v2.5 END

        ''' <summary>
        ''' 資産番号自動生成
        ''' </summary>
        Public Class AssetNo_Self_Regist
            Public Const KeyDiv As String = "AssetNo_Self_Regist"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 工事号自動生成
        ''' </summary>
        Public Class ConstNo_Self_Regist
            Public Const KeyDiv As String = "Const_No_automatic_generation"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 照合機能
        ''' </summary>
        Public Class MatchingFunction
            Public Const KeyDiv As String = "Matching_Function"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 照合機能の予約実施
        ''' </summary>
        Public Class MatchingReserveFunction
            Public Const KeyDiv As String = "Matching_Reserve_Function"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 仮置き機能
        ''' </summary>
        Public Class TemporaryLeaveFunction
            Public Const KeyDiv As String = "TemporaryLeave_Function"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 実質貸出期間補正日数
        ''' </summary>
        Public Class Const_Lent_Term
            Public Const KeyDiv As String = "Const_Lent_Term"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        '20230901 ADD Start Shirakawa
        ''' <summary>
        ''' Web資産一覧画面 検索設定
        ''' </summary>
        Public Class Web_Read_Data_Asset_List
            Public Const KeyDiv As String = "WEB_Read_Data"
            Public Const Key1 As String = "ASSET_LIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' <summary>
        ''' Web資産ラベル発行画面 検索設定
        ''' </summary>
        Public Class Web_Read_Data_Asset_Label_Print
            Public Const KeyDiv As String = "WEB_Read_Data"
            Public Const Key1 As String = "ASSET_LABEL_PRINT"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' Web資産在庫照会画面 検索設定
        ''' </summary>
        Public Class Web_Read_Data_Stock_List
            Public Const KeyDiv As String = "WEB_Read_Data"
            Public Const Key1 As String = "STOCK_LIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' Web資産履歴照会画面 検索設定
        ''' </summary>
        Public Class Web_Read_Data_Asset_Hist
            Public Const KeyDiv As String = "WEB_Read_Data"
            Public Const Key1 As String = "ASSET_HIST"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' Web貸出履歴照会画面 検索設定
        ''' </summary>
        Public Class Web_Read_Data_LEASE_HISTORY
            Public Const KeyDiv As String = "WEB_Read_Data"
            Public Const Key1 As String = "LEASE_HISTORY"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' Web棚卸進捗状況画面 検索設定
        ''' </summary>
        Public Class Web_Read_Data_Inventory_Overview
            Public Const KeyDiv As String = "WEB_Read_Data"
            Public Const Key1 As String = "INVENTORY_OVERVIEW"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' Web棚卸実績照会画面 検索設定
        ''' </summary>
        Public Class Web_Read_Data_Inventory_Results_Detail
            Public Const KeyDiv As String = "WEB_Read_Data"
            Public Const Key1 As String = "INVENTORY_RESULTS_DETAIL"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' Webログ照会画面 検索設定
        ''' </summary>
        Public Class Web_Read_Data_Operation_Log
            Public Const KeyDiv As String = "WEB_Read_Data"
            Public Const Key1 As String = "OPERATION_LOG"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' Web予約状況照会画面 検索設定
        ''' </summary>
        Public Class Web_Read_Data_Lending_Ref
            Public Const KeyDiv As String = "WEB_Read_Data"
            Public Const Key1 As String = "LENDING_REF"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        '20230901 ADD End Shirakawa

        '20240417 ADD Start Tomatsuri
        ''' <summary>
        ''' Web予約申請 送り先未設定の貸出先
        ''' </summary>
        Public Class Destination_Manual_Rrgist
            Public Const KeyDiv As String = "DESTINATION_MANUAL_REGIST"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        '20240417 ADD End Tomatsuri

        ''' <summary>
        ''' 工事一覧入力設定
        ''' </summary>
        Public Class ConstructionInputItem
            Public Const KeyDiv As String = "Construction_Input_Item"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 工事一覧検索条件
        ''' </summary>
        Public Class ConstructionSearchItem
            Public Const KeyDiv As String = "Construction_Search_Item"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 仮置一覧入力設定
        ''' </summary>
        Public Class TemporaryInputItem
            Public Const KeyDiv As String = "Temporary_Input_Item"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 工事帳票印刷設定
        ''' </summary>
        Public Class ConstPrintSetting
            Public Const KeyDiv As String = "Const_Print"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' ラベル出力方法設定
        ''' </summary>
        Public Class LabelPrntMethodSelect
            Public Const KeyDiv As String = "LabelPrint_MethodSelect"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' メンテナンス区分設定　メンテナンス
        ''' </summary>
        Public Class Reserve_Mainte_Status_Setting_M
            Public Const KeyDiv As String = "Reserve_Mainte_Status_Setting"
            Public Const Key1 As String = "1"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' メンテナンス区分設定　〇〇登録
        ''' </summary>
        Public Class Reserve_Mainte_Status_Setting_O
            Public Const KeyDiv As String = "Reserve_Mainte_Status_Setting"
            Public Const Key1 As String = "2"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' メンテナンス区分設定　予備登録１
        ''' </summary>
        Public Class Reserve_Mainte_Status_Setting_P
            Public Const KeyDiv As String = "Reserve_Mainte_Status_Setting"
            Public Const Key1 As String = "3"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' メンテナンス区分設定　予備登録２
        ''' </summary>
        Public Class Reserve_Mainte_Status_Setting_Q
            Public Const KeyDiv As String = "Reserve_Mainte_Status_Setting"
            Public Const Key1 As String = "4"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 予約状況照会 凡例表示可否設定
        ''' </summary>
        Public Class Reserve_Legend_Disp
            Public Const KeyDiv As String = "Reserve_Legend_Disp"
            Public Const Key1 As String = "**"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 棚卸実績.稼働区分
        ''' </summary>
        Public Class OpeStatusItem
            Public Const KeyDiv As String = "Ope_Status_Item"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 棚卸実績.除却理由
        ''' </summary>
        Public Class RemoveReasonItem
            Public Const KeyDiv As String = "Remove_Reason_Item"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 棚卸実績.転用or除却
        ''' </summary>
        Public Class DiversionOrRemove
            Public Const KeyDiv As String = "Diversion_or_Remove"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class

        ''' <summary>
        ''' 有形資産カテゴリー
        ''' </summary>
        Public Class TangibleCategory
            Public Const KeyDiv As String = "Tangible_Category"
            Public Const Key2 As String = "**"
            Public Const Key3 As String = "**"
        End Class
    End Class
End Namespace
