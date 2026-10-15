Imports Asetra.Misc
Imports System.IO
Imports Asetra.Models
Imports System.Data.SqlClient
Imports GrapeCity.Documents.Excel

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：資産台帳Excel取込関連クラス
    ''' 作成日        ：2025/10/03      Shirai
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolAssetExcelImport

        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        Public Const EXCEL_DATA_END_COL As Integer = 211       '最終列

        Public Const EXCEL_DATA_START_ROW As Integer = 4       'データ開始行

        Public Const EXCEL_FOOTER_ROW_COUNT As Integer = 2     '対象外とするフッター行数



        ''' <summary>
        ''' Excel取込定義
        ''' </summary>
        ''' 
        Public Class ExcelImportDef
            ''' <summary>
            ''' Excel取込定義（Excel列情報）
            ''' </summary>
            Public Class ExcelCol
                ''' <summary>列名（DB列と紐づけ用）</summary>
                Public Property Name As String
                ''' <summary>Excel列インデックス</summary>
                Public Property ExcelColNo As Integer
                ''' <summary>入力エラー時の項目文言コード</summary>
                Public Property WordCd As String
                ''' <summary>入力チェック有無（必須チェック）</summary>
                Public Property IsRequired As Boolean = False
                ''' <summary>入力チェック有無（DB桁数内かチェック）</summary>
                Public Property IsLengthCheck As Boolean = True
            End Class

            ''' <summary>
            ''' Excel取込定義（取込先テーブル列情報）
            ''' </summary>
            Public Class DBCol
                ''' <summary>DB列名</summary>
                Public Property Name As String
                ''' <summary>データ型</summary>
                Public Property DataType As Type
                ''' <summary>最大桁数</summary>
                Public Property MaxLength As Integer? = Nothing
            End Class
        End Class

        ''' <summary>
        ''' 資産番号重複チェック用定義
        ''' </summary>
        Public Class DupAssetResult
            ''' <summary>資産番号</summary>
            Public Property ASSET_NO As String
            ''' <summary>行番号（カンマ区切り）</summary>
            Public Property ROWS As String
        End Class

#Region "クラス変数"
        ''' <summary>
        ''' Excel列情報
        ''' </summary>
        Private Shared ReadOnly _excelDefs As List(Of ExcelImportDef.ExcelCol) = New List(Of ExcelImportDef.ExcelCol) From {
            New ExcelImportDef.ExcelCol With {.Name = "ASSET_NO", .ExcelColNo = 8, .WordCd = CommonLabel.LABEL_ASSET_NO, .IsRequired = True},                '資産番号
            New ExcelImportDef.ExcelCol With {.Name = "SUB_ASSET_NO", .ExcelColNo = 59, .WordCd = CommonLabel.LABEL_SUB_ASSET_NO},                           '移行元資産番号
            New ExcelImportDef.ExcelCol With {.Name = "ASSET_NAME", .ExcelColNo = 10, .WordCd = CommonLabel.LABEL_ASSET_NAME, .IsRequired = True},           'テキスト1
            New ExcelImportDef.ExcelCol With {.Name = "MNG_LOC_L_CD", .ExcelColNo = 1, .WordCd = CommonLabel.LABEL_LOC_L_COM_CODE, .IsLengthCheck = False},  '会社コード
            New ExcelImportDef.ExcelCol With {.Name = "LOC_L_CD", .ExcelColNo = 1, .WordCd = CommonLabel.LABEL_LOC_L_COM_CODE_C, .IsRequired = True},        '会社コード
            New ExcelImportDef.ExcelCol With {.Name = "LOC_L_NAME", .ExcelColNo = 2, .WordCd = CommonLabel.LABEL_LOC_L_COM_NAME_C},                          '会社名
            New ExcelImportDef.ExcelCol With {.Name = "LOC_M_CD", .ExcelColNo = 27, .WordCd = CommonLabel.LABEL_LOC_M_DEP_CODE_C},                           '原価センタ
            New ExcelImportDef.ExcelCol With {.Name = "LOC_M_NAME", .ExcelColNo = 28, .WordCd = CommonLabel.LABEL_LOC_M_DEP_NAME_C},                         '原価センタ名
            New ExcelImportDef.ExcelCol With {.Name = "LOC_S_CD", .ExcelColNo = 29, .WordCd = CommonLabel.LABEL_LOC_M_PLA_NAME_C},                           '場所コード
            New ExcelImportDef.ExcelCol With {.Name = "LOC_S_NAME", .ExcelColNo = 30, .WordCd = CommonLabel.LABEL_LOC_M_PLA_CODE_C},                         '場所コード名
            New ExcelImportDef.ExcelCol With {.Name = "CAT_L_CD", .ExcelColNo = 6, .WordCd = CommonLabel.LABEL_CAT_L_CODE_C, .IsRequired = True},            '資産クラス
            New ExcelImportDef.ExcelCol With {.Name = "CAT_L_NAME", .ExcelColNo = 7, .WordCd = CommonLabel.LABEL_CAT_L_NAME_C},                              '資産クラス名
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM1", .ExcelColNo = 14, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM1},                         '最終棚卸日
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM2", .ExcelColNo = 16, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM2},                         '資本化日付
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM3", .ExcelColNo = 47, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM3},                         '稼働区分
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM4", .ExcelColNo = 48, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM4},                         '稼働区分名
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM5", .ExcelColNo = 60, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM5},                         '予備1
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM6", .ExcelColNo = 61, .WordCd = CommonLabel.LABEL_MANAGER_EMPLOYEE_CD},                      '予備2(社員コード)
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM7", .ExcelColNo = 62, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM7},                         '予備3(シリアルナンバー)
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM8", .ExcelColNo = 63, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM8},                         '予備4
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM9", .ExcelColNo = 64, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM9},                         '予備5
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM10", .ExcelColNo = 65, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM10},                       '貸与資産状態区分
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM11", .ExcelColNo = 97, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM11},                       '01グローバル-取得価額(圧縮後)
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM12", .ExcelColNo = 108, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM12},                      '01グローバル-当月末帳簿価額
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM13", .ExcelColNo = 9, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM13},                        '補助番号
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM14", .ExcelColNo = 24, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM14},                       '装置管理番号
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM15", .ExcelColNo = 40, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM15},                       '資産グループコード
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM16", .ExcelColNo = 66, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM16},                       '貸与先
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM17", .ExcelColNo = 67, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM17},                       '貸与先名
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM18", .ExcelColNo = 71, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM18},                       '開始日(貸与)
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM19", .ExcelColNo = 72, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM19},                       '終了日(貸与)
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM20", .ExcelColNo = 73, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM20},                       '貸与回数
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM21", .ExcelColNo = 74, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM21},                       '備考欄
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM22", .ExcelColNo = 3, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM22},                        '会計年度
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM23", .ExcelColNo = 4, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM23},                        '会計期間
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM24", .ExcelColNo = 11, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM24},                       'テキスト2
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM25", .ExcelColNo = 12, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM25},                       '勘定科目
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM26", .ExcelColNo = 13, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM26},                       '勘定科目名称
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM27", .ExcelColNo = 15, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM27},                       '棚卸ノート
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM28", .ExcelColNo = 17, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM28},                       '初回取得日
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM29", .ExcelColNo = 21, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM29},                       '数量
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM30", .ExcelColNo = 23, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM30},                       '資産番号テキスト
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM31", .ExcelColNo = 31, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM31},                       '車両のナンバープレート
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM32", .ExcelColNo = 32, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM32},                       '資産休止フラグ
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM33", .ExcelColNo = 43, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM33},                       '取得区分
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM34", .ExcelColNo = 44, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM34},                       '取得区分名
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM35", .ExcelColNo = 45, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM35},                       '資産種類
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM36", .ExcelColNo = 46, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM36},                       '資産種類名
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM37", .ExcelColNo = 49, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM37},                       '資産・リース区分
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM38", .ExcelColNo = 50, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM38},                       '市区町村コード
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM39", .ExcelColNo = 51, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM39},                       '市区町村コード名
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM40", .ExcelColNo = 52, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM40},                       '仕入先
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM41", .ExcelColNo = 53, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM41},                       '資産供給者名
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM42", .ExcelColNo = 57, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM42},                       'CAPS番号
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM43", .ExcelColNo = 58, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM43},                       '稟議＃
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM44", .ExcelColNo = 75, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM44},                       'リース会社
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM45", .ExcelColNo = 76, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM45},                       'リース会社名
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM46", .ExcelColNo = 77, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM46},                       '契約番号
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM47", .ExcelColNo = 78, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM47},                       '契約日
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM48", .ExcelColNo = 79, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM48},                       'リース終了日付
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM49", .ExcelColNo = 80, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM49},                       'リース開始日
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM50", .ExcelColNo = 81, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM50},                       'リース期間(年)
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM51", .ExcelColNo = 82, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM51},                       'リース期間(期)
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM52", .ExcelColNo = 87, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM52},                       'リース支払回数
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM53", .ExcelColNo = 88, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM53},                       'リース支払
            New ExcelImportDef.ExcelCol With {.Name = "FREE_ITEM54", .ExcelColNo = 89, .WordCd = CommonLabel.LABEL_ASSET_FREE_ITEM54}                       '年利
        }

        ''' <summary>
        ''' Excel取込先テーブル列情報
        ''' </summary>
        Private Shared ReadOnly _dbColDefs As List(Of ExcelImportDef.DBCol) = New List(Of ExcelImportDef.DBCol) From {
            New ExcelImportDef.DBCol With {.Name = "ASSET_NO", .DataType = GetType(String), .MaxLength = 24},                '資産番号
            New ExcelImportDef.DBCol With {.Name = "SUB_ASSET_NO", .DataType = GetType(String), .MaxLength = 24},            'サブ資産番号
            New ExcelImportDef.DBCol With {.Name = "ASSET_NAME", .DataType = GetType(String), .MaxLength = 60},              '資産名称
            New ExcelImportDef.DBCol With {.Name = "MNG_LOC_L_CD", .DataType = GetType(String), .MaxLength = 10},            '管理ロケ大区分コード
            New ExcelImportDef.DBCol With {.Name = "LOC_L_CD", .DataType = GetType(String), .MaxLength = 10},                'ロケ大区分コード
            New ExcelImportDef.DBCol With {.Name = "LOC_L_NAME", .DataType = GetType(String), .MaxLength = 60},              'ロケ大区分名称
            New ExcelImportDef.DBCol With {.Name = "LOC_M_CD", .DataType = GetType(String), .MaxLength = 10},                'ロケ中区分コード
            New ExcelImportDef.DBCol With {.Name = "LOC_M_NAME", .DataType = GetType(String), .MaxLength = 60},              'ロケ中区分名称
            New ExcelImportDef.DBCol With {.Name = "LOC_S_CD", .DataType = GetType(String), .MaxLength = 10},                'ロケ小区分コード
            New ExcelImportDef.DBCol With {.Name = "LOC_S_NAME", .DataType = GetType(String), .MaxLength = 60},              'ロケ小区分名称
            New ExcelImportDef.DBCol With {.Name = "CAT_L_CD", .DataType = GetType(String), .MaxLength = 8},                 'カテゴリ大分類コード
            New ExcelImportDef.DBCol With {.Name = "CAT_L_NAME", .DataType = GetType(String), .MaxLength = 60},              'カテゴリ大分類名称
            New ExcelImportDef.DBCol With {.Name = "ASSET_STATUS", .DataType = GetType(Byte)},                               '資産状態
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM1", .DataType = GetType(String), .MaxLength = 255},             'フリー項目1
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM2", .DataType = GetType(String), .MaxLength = 255},             'フリー項目2
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM3", .DataType = GetType(String), .MaxLength = 255},             'フリー項目3
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM4", .DataType = GetType(String), .MaxLength = 255},             'フリー項目4
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM5", .DataType = GetType(String), .MaxLength = 255},             'フリー項目5
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM6", .DataType = GetType(String), .MaxLength = 15},              'フリー項目6
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM7", .DataType = GetType(String), .MaxLength = 255},             'フリー項目7
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM8", .DataType = GetType(String), .MaxLength = 255},             'フリー項目8
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM9", .DataType = GetType(String), .MaxLength = 255},             'フリー項目9
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM10", .DataType = GetType(String), .MaxLength = 255},            'フリー項目10
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM11", .DataType = GetType(String), .MaxLength = 255},            'フリー項目11
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM12", .DataType = GetType(String), .MaxLength = 255},            'フリー項目12
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM13", .DataType = GetType(String), .MaxLength = 255},            'フリー項目13
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM14", .DataType = GetType(String), .MaxLength = 255},            'フリー項目14
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM15", .DataType = GetType(String), .MaxLength = 255},            'フリー項目15
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM16", .DataType = GetType(String), .MaxLength = 255},            'フリー項目16
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM17", .DataType = GetType(String), .MaxLength = 255},            'フリー項目17
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM18", .DataType = GetType(String), .MaxLength = 255},            'フリー項目18
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM19", .DataType = GetType(String), .MaxLength = 255},            'フリー項目19
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM20", .DataType = GetType(String), .MaxLength = 255},            'フリー項目20
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM21", .DataType = GetType(String), .MaxLength = 255},            'フリー項目21
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM22", .DataType = GetType(String), .MaxLength = 255},            'フリー項目22
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM23", .DataType = GetType(String), .MaxLength = 255},            'フリー項目23
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM24", .DataType = GetType(String), .MaxLength = 255},            'フリー項目24
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM25", .DataType = GetType(String), .MaxLength = 255},            'フリー項目25
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM26", .DataType = GetType(String), .MaxLength = 255},            'フリー項目26
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM27", .DataType = GetType(String), .MaxLength = 255},            'フリー項目27
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM28", .DataType = GetType(String), .MaxLength = 255},            'フリー項目28
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM29", .DataType = GetType(String), .MaxLength = 255},            'フリー項目29
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM30", .DataType = GetType(String), .MaxLength = 255},            'フリー項目30
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM31", .DataType = GetType(String), .MaxLength = 255},            'フリー項目31
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM32", .DataType = GetType(String), .MaxLength = 255},            'フリー項目32
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM33", .DataType = GetType(String), .MaxLength = 255},            'フリー項目33
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM34", .DataType = GetType(String), .MaxLength = 255},            'フリー項目34
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM35", .DataType = GetType(String), .MaxLength = 255},            'フリー項目35
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM36", .DataType = GetType(String), .MaxLength = 255},            'フリー項目36
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM37", .DataType = GetType(String), .MaxLength = 255},            'フリー項目37
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM38", .DataType = GetType(String), .MaxLength = 255},            'フリー項目38
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM39", .DataType = GetType(String), .MaxLength = 255},            'フリー項目39
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM40", .DataType = GetType(String), .MaxLength = 255},            'フリー項目40
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM41", .DataType = GetType(String), .MaxLength = 255},            'フリー項目41
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM42", .DataType = GetType(String), .MaxLength = 255},            'フリー項目42
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM43", .DataType = GetType(String), .MaxLength = 255},            'フリー項目43
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM44", .DataType = GetType(String), .MaxLength = 255},            'フリー項目44
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM45", .DataType = GetType(String), .MaxLength = 255},            'フリー項目45
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM46", .DataType = GetType(String), .MaxLength = 255},            'フリー項目46
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM47", .DataType = GetType(String), .MaxLength = 255},            'フリー項目47
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM48", .DataType = GetType(String), .MaxLength = 255},            'フリー項目48
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM49", .DataType = GetType(String), .MaxLength = 255},            'フリー項目49
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM50", .DataType = GetType(String), .MaxLength = 255},            'フリー項目50
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM51", .DataType = GetType(String), .MaxLength = 255},            'フリー項目51
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM52", .DataType = GetType(String), .MaxLength = 255},            'フリー項目52
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM53", .DataType = GetType(String), .MaxLength = 255},            'フリー項目53
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM54", .DataType = GetType(String), .MaxLength = 255},            'フリー項目54
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM55", .DataType = GetType(String), .MaxLength = 255},      　　　'フリー項目55
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM56", .DataType = GetType(String), .MaxLength = 255},      　　　'フリー項目56
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM57", .DataType = GetType(String), .MaxLength = 255},      　　　'フリー項目57
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM58", .DataType = GetType(String), .MaxLength = 255},      　　　'フリー項目58
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM59", .DataType = GetType(String), .MaxLength = 255},      　　　'フリー項目59
            New ExcelImportDef.DBCol With {.Name = "FREE_ITEM60", .DataType = GetType(String), .MaxLength = 255},      　　　'フリー項目60
            New ExcelImportDef.DBCol With {.Name = "IS_TANGIBLE", .DataType = GetType(String), .MaxLength = 1},      　　　  '有形・無形区分
            New ExcelImportDef.DBCol With {.Name = "IP_DATETIME", .DataType = GetType(Long)},                                '取込処理日時
            New ExcelImportDef.DBCol With {.Name = "IP_EMPLOYEE_CD", .DataType = GetType(String), .MaxLength = 15},          '取込担当者コード
            New ExcelImportDef.DBCol With {.Name = "IP_EMPLOYEE_NAME", .DataType = GetType(String), .MaxLength = 60},        '取込担当者氏名
            New ExcelImportDef.DBCol With {.Name = "IP_FILE_NAME", .DataType = GetType(String), .MaxLength = 255},           '取込ファイル名
            New ExcelImportDef.DBCol With {.Name = "UPD_FLG", .DataType = GetType(String), .MaxLength = 1},                  '更新フラグ
            New ExcelImportDef.DBCol With {.Name = "UPD_TASK", .DataType = GetType(String), .MaxLength = 255},               '最終登録更新処理
            New ExcelImportDef.DBCol With {.Name = "ROW_NO", .DataType = GetType(Long)}                                      '行番号
        }


        ''' <summary>入庫予定ワークテーブル名</summary>
        Private Const WORK_TABLE_NAME As String = "W_ASSET_IMP"

        ''' <summary>入庫予定ワークのデータを削除するまでの日数（取込処理日時がこの日数以上前のデータを削除する）</summary>
        Private Const WORK_KEEP_DAYS As Integer = 2

#End Region


#Region "資産台帳Excel取込関連クラス"
        '''' <summary>
        '''' Excel取込処理のエントリポイント
        '''' </summary>
        '''' <returns>0:正常終了  0以外:異常終了</returns>


















        ''' <summary>
        ''' Excel取込処理のエントリポイント（資産台帳取込ボタン）
        ''' </summary>
        ''' <param name="functionId">呼び出し元画面の権限機能ID</param>
        ''' <param name="filePath">取込ファイルパス</param>
        ''' <param name="locLCd">ロケ大コード</param>
        ''' <param name="loginInfo">ログイン情報</param>
        ''' <param name="lstErr">画面表示用エラーリスト</param>
        ''' <param name="excelRowCount">Excel行数</param>
        ''' <returns>True:正常終了  False:異常終了または削除確認</returns>
        ''' <remarks>
        ''' 削除件数が警告件数を超える場合は、ワークのデータを残したまま、
        ''' ResultCd=ImportDeleteConfirm に削除件数と取込ごとのキーを付けて返す。
        ''' それ以外は、終了時（正常・異常とも）に取込ごとのキーを条件にワークのデータを削除する。
        ''' </remarks>
        Public Shared Function Execute(ByVal functionId As String,
                                       ByVal filePath As String,
                                       ByVal locLCd As String,
                                       ByVal loginInfo As RfidEntity.LoginUser,
                                       ByRef lstErr As List(Of DataInportResult),
                                       ByRef excelRowCount As Integer) As Boolean

            Dim importKey As String = Guid.NewGuid().ToString()
            Dim isWorkSaved As Boolean = False
            Dim isKeepWork As Boolean = False

            Try
                Using db As New SystemDBContext

                    DeleteOldWork(db)

                    Dim dtTmpAsset As DataTable = ReadAndValidation(functionId, filePath, locLCd, loginInfo, lstErr, excelRowCount)

                    If lstErr.Any(Function(r) {DataInport.DataInportResultCd.DataNG,
                                               DataInport.DataInportResultCd.KoumokuSuuErr}.Contains(r.ResultCd)) Then
                        Return False
                    End If

                    If dtTmpAsset.Rows.Count <= 0 Then
                        Return True
                    End If

                    isWorkSaved = True
                    SaveToWork(db, dtTmpAsset, importKey, excelRowCount)

                    If ValidationForTmpTable(db, importKey, lstErr, loginInfo, locLCd, functionId) Then
                        Return False
                    End If

                    Dim nAlertCount As Integer = GetDeleteAlertCount()
                    If nAlertCount > 0 Then
                        Dim nDeleteCount As Integer = GetDeleteTargetCount(db, importKey, locLCd)
                        If nDeleteCount > nAlertCount Then
                            isKeepWork = True
                            lstErr.Add(New DataInportResult With {
                                .ResultCd = DataInport.DataInportResultCd.ImportDeleteConfirm,
                                .DeleteCount = nDeleteCount,
                                .ImportKey = importKey
                            })
                            Return False
                        End If
                    End If

                    Return ImportFromWork(db, importKey, locLCd, loginInfo, lstErr, functionId)
                End Using

            Catch ex As Exception
                logger.Error(ex.Message)
                lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.ImportExcelErr})
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_SYSTEM)))

            Finally
                If isWorkSaved AndAlso Not isKeepWork Then
                    DeleteWork(importKey, loginInfo)
                End If
            End Try
        End Function

        ''' <summary>
        ''' 削除確認で「はい」が選ばれた場合の取込処理
        ''' </summary>
        ''' <param name="functionId">呼び出し元画面の権限機能ID</param>
        ''' <param name="importKey">取込ごとのキー（初回の取込で画面へ返したもの）</param>
        ''' <param name="locLCd">ロケ大コード</param>
        ''' <param name="loginInfo">ログイン情報</param>
        ''' <param name="lstErr">画面表示用エラーリスト</param>
        ''' <param name="rowCount">取込件数（操作ログ用。初回の取込で保存したExcel行数）</param>
        ''' <param name="fileName">取込ファイル名（操作ログ用。ワークの取込ファイル名）</param>
        ''' <returns>True:正常終了  False:異常終了</returns>
        ''' <remarks>
        ''' Excelファイルは再読込せず、入庫予定ワークに保存したデータで取込を行う。
        ''' 次の場合は取込失敗（ImportExcelErr。取込失敗と同じ表示）とし、最初からやり直してもらう。
        ''' 　・ワークにデータが無い（2日経過で削除された等）、またはログイン中の社員の取込でない
        ''' 　・会社（ロケ大コード）が取込時と異なる
        ''' 削除件数の再チェックは行わない（確認済みのため）。入力チェックは、確認中に他の利用者が
        ''' 資産の拠点を変えた場合に備えてもう一度行う。
        ''' 終了時（正常・異常とも）に取込ごとのキーを条件にワークのデータを削除する。
        ''' </remarks>
        Public Shared Function ExecuteConfirmed(ByVal functionId As String,
                                                ByVal importKey As String,
                                                ByVal locLCd As String,
                                                ByVal loginInfo As RfidEntity.LoginUser,
                                                ByRef lstErr As List(Of DataInportResult),
                                                ByRef rowCount As Integer,
                                                ByRef fileName As String) As Boolean

            Dim keyGuid As Guid
            If String.IsNullOrEmpty(importKey) OrElse Not Guid.TryParse(importKey, keyGuid) Then
                lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.ImportExcelErr})
                Return False
            End If
            importKey = keyGuid.ToString()

            Try
                Using db As New SystemDBContext

                    Dim workInfo As WorkSummary = GetWorkSummary(db, importKey, locLCd, loginInfo)
                    fileName = workInfo.IP_FILE_NAME
                    If workInfo.ROW_COUNT <= 0 OrElse workInfo.OTHER_LOC_COUNT > 0 Then
                        lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.ImportExcelErr})
                        Return False
                    End If
                    rowCount = workInfo.IP_ROW_COUNT

                    If ValidationForTmpTable(db, importKey, lstErr, loginInfo, locLCd, functionId) Then
                        Return False
                    End If

                    Return ImportFromWork(db, importKey, locLCd, loginInfo, lstErr, functionId)
                End Using

            Catch ex As Exception
                logger.Error(ex.Message)
                lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.ImportExcelErr})
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_SYSTEM)))

            Finally
                DeleteWork(importKey, loginInfo)
            End Try
        End Function

        ''' <summary>
        ''' 削除確認で「いいえ」が選ばれた場合の処理
        ''' </summary>
        ''' <param name="importKey">取込ごとのキー（初回の取込で画面へ返したもの）</param>
        ''' <param name="loginInfo">ログイン情報</param>
        ''' <remarks>取込ごとのキーを条件に入庫予定ワークのデータを削除して中断する。</remarks>
        Public Shared Sub CancelImport(ByVal importKey As String,
                                       ByVal loginInfo As RfidEntity.LoginUser)
            Dim keyGuid As Guid
            If String.IsNullOrEmpty(importKey) OrElse Not Guid.TryParse(importKey, keyGuid) Then
                Return
            End If
            DeleteWork(keyGuid.ToString(), loginInfo)
        End Sub

        ''' <summary>
        ''' 入庫予定ワークのデータで取込（ストアド）を実行する
        ''' </summary>
        ''' <param name="db">DBコンテキスト</param>
        ''' <param name="importKey">取込ごとのキー</param>
        ''' <param name="locLCd">ロケ大コード</param>
        ''' <param name="loginInfo">ログイン情報</param>
        ''' <param name="lstErr">画面表示用エラーリスト</param>
        ''' <param name="functionId">呼び出し元画面の権限機能ID</param>
        ''' <returns>True:正常終了  False:異常終了</returns>
        ''' <remarks>
        ''' ストアド dbo.IMPORT_ASSET_FILE は渡された表の全件を処理するため、ワークを直接は渡せない。
        ''' 取込ごとのキーで絞ったデータを一時表に写して渡す（ストアドは変更しない）。
        ''' </remarks>
        Private Shared Function ImportFromWork(ByVal db As SystemDBContext,
                                               ByVal importKey As String,
                                               ByVal locLCd As String,
                                               ByVal loginInfo As RfidEntity.LoginUser,
                                               ByRef lstErr As List(Of DataInportResult),
                                               ByVal functionId As String) As Boolean

            Dim sTmpTblNm = "#TASSET" & Now.ToString("HHmmss") & (New Random).Next(0, 1000).ToString("000")

            Using tran = db.Database.BeginTransaction()
                Try
                    CreateTempTableFromWork(db, sTmpTblNm, importKey)

                    Dim ret = ExecuteAssetImportProcess(db, sTmpTblNm, locLCd, loginInfo, functionId)

                    If ret <> ImportAssetFileResult.Success Then
                        tran.Rollback()

                        If ret = ImportAssetFileResult.SpSystemErr Then
                            lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.ImportSpSystemErr})
                        ElseIf ret = ImportAssetFileResult.ParamErr Then
                            lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.ImportSpParamErr})
                        ElseIf ret = ImportAssetFileResult.SpLocMaterErr Then
                            lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.ImportSpLocMaterErr})
                        ElseIf ret = ImportAssetFileResult.SpEmployeeErr Then
                            lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.ImportSpEmployeeErr})
                        End If
                        Return False
                    End If

                    tran.Commit()

                Catch ex As Exception
                    tran.Rollback()
                    Throw
                End Try
            End Using

            Return True
        End Function

        ''' <summary>
        ''' 資産削除時の警告を表示する件数を取得する
        ''' </summary>
        ''' <returns>設定M(KEY種別=Asset_Delete_AlertCount)のDATA1。未設定・数値以外の場合は0</returns>
        Private Shared Function GetDeleteAlertCount() As Integer
            Dim settingAlert As SettingInfo = CommonRfid.MessageRfid.getSettingInfo(
                CommonSettingKey.AssetDeleteAlertCount.KeyDiv,
                CommonSettingKey.AssetDeleteAlertCount.Key1,
                CommonSettingKey.AssetDeleteAlertCount.Key2,
                CommonSettingKey.AssetDeleteAlertCount.Key3)

            If settingAlert IsNot Nothing AndAlso IsNumeric(settingAlert.DATA1) Then
                Return CInt(settingAlert.DATA1)
            End If
            Return 0
        End Function
#End Region

#Region "入力チェック処理"

        ''' <summary>
        ''' Excel値の読込＆入力チェック
        ''' </summary>
        ''' <param name="functionId">呼び出し元画面の権限機能ID</param>
        ''' <param name="filePath">取込ファイルパス</param>
        ''' <param name="locLCd">ロケ大コード</param>
        ''' <param name="loginInfo">ログイン情報</param>
        ''' <param name="lstErr">画面表示用エラーリスト</param>
        ''' <returns></returns>
        Private Shared Function ReadAndValidation(ByVal functionId As String,
                                                  ByVal filePath As String,
                                                  ByVal locLCd As String,
                                                  ByVal loginInfo As RfidEntity.LoginUser,
                                                  ByRef lstErr As List(Of DataInportResult),
                                                  ByRef excelRowCount As Integer) As DataTable

            Dim dtTmpAsset As New DataTable()

            'ファイル読込
            Dim wb As IWorkbook = New Workbook
            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                ' ライセンスキー
                GrapeCity.Documents.Excel.Workbook.SetLicenseKey("株式会社シービジョン,496378668853486#A03NJgzNzYTO4IiOiQWSisnOiQkIsISP3cWe6QTZ95WWvEmMYdTStZ5MltGdv26dWlWazcmYBREamhXNyUEVRdGbOl6SuREarg4L6dVRwZnUz4EZFZzKzYmZv2Ee7gje9tWb5IzZ6x6arQGUVl5UiojITJCL7YzM8cTM8gDO0IicfJye#4Xfd5nI9g4NyIiOiMkIsIiMWBCdl9kLgwWZjhXRgI7bmBCduVWb5N6bEByQHJiOi8kI1tlOiQmcQJCLiEDNzIDNwAiNwITM9EDMyIiOiQncDJCLiM1gjf0gjjrgjP9gjz1gjfrgj1LpnrJvk/IvlrKomLiOiEmTDJCLiYDO4M5Ntg")
                wb.Open(fs)
            End Using
            Dim ws As IWorksheet = wb.Worksheets(0)

            '行情報
            Dim lastDataRow = ws.UsedRange.Rows.RowCount
            Dim lastDataCol = ws.UsedRange.Columns.ColumnCount
            Dim rowIndex As Integer = 0

            Try
                '最終行判定
                Dim assetNoCol = _excelDefs.Where(Function(x) x.Name = "ASSET_NO").Select(Function(x) x.ExcelColNo).FirstOrDefault()
                Do While lastDataRow >= EXCEL_DATA_START_ROW
                    '資産番号列の値を取得
                    Dim cv As Object = ws.Cells(lastDataRow - 1, assetNoCol - 1).Value
                    If Not cv Is Nothing AndAlso Not String.IsNullOrEmpty(cv.ToString().Trim()) Then
                        '資産番号が入っている行を最終行とする
                        Exit Do
                    End If
                    lastDataRow -= 1
                Loop

                '更新可能拠点の取得
                Dim AuthInfo As AuthInfo = Misc.CommonRfid.MessageRfid.GetFunctionAuth(functionId, loginInfo)
                Dim updatableLocL = Dao.ToolLocation.GetUpdatableLocLList(loginInfo, AuthInfo)

                '取込Excelの項目数チェック
                If lastDataCol <> EXCEL_DATA_END_COL Then
                    lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.KoumokuSuuErr})
                    Return dtTmpAsset
                End If

                '資産台帳Excel定義を取得
                For Each dbDef In _dbColDefs
                    dtTmpAsset.Columns.Add(dbDef.Name, dbDef.DataType)
                Next

                'Excelの行ごとに入力チェック
                For rowNo = EXCEL_DATA_START_ROW To lastDataRow
                    rowIndex = rowNo    '行番号
                    Dim dr = dtTmpAsset.NewRow()
                    Dim hasError As Boolean = False
                    Dim shouldSkipRow As Boolean = False

                    '定義の列でループ
                    For Each excelDef In _excelDefs
                        Dim cellVal As String = String.Empty
                        Dim cv As Object = ws.Cells(rowNo - 1, excelDef.ExcelColNo - 1).Value
                        If Not cv Is Nothing Then
                            cellVal = cv.ToString().Trim()
                        End If

                        '必須チェック
                        If excelDef.IsRequired AndAlso String.IsNullOrEmpty(cellVal) Then
                            AddErrorList(lstErr, rowNo, excelDef.WordCd, CommonLabel.MSG_ERR_DATAEMPTY)
                            hasError = True
                        End If

                        '最大桁数チェック
                        Dim dbDef = _dbColDefs.FirstOrDefault(Function(x) x.Name = excelDef.Name)
                        If excelDef.IsLengthCheck AndAlso dbDef.MaxLength < cellVal.Length Then
                            AddErrorList(lstErr, rowNo, excelDef.WordCd, CommonLabel.MSG_ERR_DATAOVER)
                            hasError = True
                        End If

                        '項目別の入力チェック
                        Select Case excelDef.Name
                        '会社コード
                            Case "LOC_L_CD"
                                '検索条件と一致しない場合、エラー
                                If cellVal <> locLCd Then
                                    AddErrorList(lstErr, rowNo, excelDef.WordCd, CommonLabel.MSG_ERR_COMPANY_MISMATCH_SEARCH_CONDITION)
                                    hasError = True

                                    'ログ登録(意図せずエラーになる場合があり、原因特定のためにログを追加)
                                    Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event,
                                                                   "ロケ違い 行番号" & rowNo - 1 & " 条件:" & locLCd & " 値:" & cellVal,
                                                                   loginInfo, CommonLabel.BUTTON_ASSET_LEDGER_IMPORT)
                                ElseIf updatableLocL.Rows.Count = 0 OrElse updatableLocL.Select($"LOC_CD = '{cellVal}'").Length <= 0 Then
                                    '更新可能拠点でない場合、エラー
                                    AddErrorList(lstErr, rowNo, excelDef.WordCd, CommonLabel.MSG_ERR_COMPANY_NOT_UPDATABLE)
                                    hasError = True
                                    End If
                        End Select

                        'スキップチェック
                        Select Case excelDef.Name
                        '01グローバル取得価額（圧縮後）
                            Case "FREE_ITEM11"
                                '0の場合は取り込まない
                                If cellVal <> "" AndAlso IsNumeric(cellVal) AndAlso cellVal = 0 Then
                                    shouldSkipRow = True
                                End If
                        End Select

                        '一時表登録用に値保持
                        dr(excelDef.Name) = cellVal
                    Next

                    '取込対象行の場合
                    If Not hasError And Not shouldSkipRow Then
                        'Excelにない項目に値をセット
                        dr("ASSET_STATUS") = ASSET_STATUS.Storage
                        dr("IP_DATETIME") = DateTime.Now.ToString("yyyyMMddHHmmss")
                        dr("IP_EMPLOYEE_CD") = loginInfo.EmployeeCd
                        dr("IP_EMPLOYEE_NAME") = loginInfo.EmployeeNm
                        dr("IP_FILE_NAME") = Path.GetFileName(filePath)
                        dr("UPD_TASK") = Misc.CommonRfid.MessageRfid.getMessage(GetDispTitle(functionId))
                        dr("ROW_NO") = rowNo

                        '資産番号の5桁目から有形/無形区分をセット
                        Dim assetNo = dr("ASSET_NO").ToString.Trim
                        If assetNo.Length >= CommonConst.IS_TANGIBLE_DIGIT AndAlso
                            assetNo.Substring(CommonConst.IS_TANGIBLE_DIGIT - 1, 1) = CommonConst.NOT_TANGIBLE_CHAR Then
                            dr("IS_TANGIBLE") = TANGIBLE.NOT_TANGIBLE
                        Else
                            dr("IS_TANGIBLE") = TANGIBLE.TANGIBLE
                        End If

                        '一時表登録用のデータテーブルに格納
                        dtTmpAsset.Rows.Add(dr)
                    End If
                Next

            Catch ex As Exception
                logger.Error("資産台帳" & rowIndex & "行目:" & ex.Message)
                AddErrorList(lstErr, rowIndex, CommonLabel.LABEL_ASSET_NO, CommonLabel.MSG_ERR_SYSTEM)
            End Try

            '入力エラーありの場合、エラーコード設定する
            If lstErr.Any() Then
                lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
            End If

            'Excelデータ件数（ログ用）
            excelRowCount = lastDataRow - EXCEL_DATA_START_ROW + 1

            Return dtTmpAsset
        End Function

        ''' <summary>
        ''' エラー情報をリストに追加する
        ''' </summary>
        ''' <param name="lstErr">エラーリスト</param>
        ''' <param name="rowNo">データ行</param>
        ''' <param name="koumokuCd">データ項目（文言コード）</param>
        ''' <param name="gaiyouCd">エラー概要（文言コード）</param>
        Private Shared Sub AddErrorList(ByRef lstErr As List(Of DataInportResult),
                                        ByVal rowNo As Integer,
                                        ByVal koumokuCd As String,
                                        ByVal gaiyouCd As String)

            lstErr.Add(New DataInportResult With {
                .ErrRowIdx = rowNo,
                .ErrKoumoku = Misc.CommonRfid.MessageRfid.getMessage(koumokuCd),
                .ErrGaiyou = Misc.CommonRfid.MessageRfid.getMessage(gaiyouCd)
            })
        End Sub
#End Region

#Region "一時表テーブル作成後の入力チェック処理"
        ''' <summary>
        ''' 一時表テーブル作成後の入力チェック処理
        ''' </summary>
        ''' <param name="db">Dbコンテキスト</param>
        ''' <param name="importKey">取込ごとのキー（入庫予定ワークの SESSION_ID）</param>
        ''' <param name="lstErr">画面表示用エラーリスト</param>
        ''' <param name="loginInfo">ログイン情報</param>
        ''' <param name="locLCd">ロケ大コード</param>
        ''' <param name="functionId">呼び出し元画面の権限機能ID</param>
        ''' <returns>True:エラーあり  False:エラーなし</returns>
        Public Shared Function ValidationForTmpTable(ByVal db As SystemDBContext,
                                                     ByVal importKey As String,
                                                     ByRef lstErr As List(Of DataInportResult),
                                                     ByVal loginInfo As RfidEntity.LoginUser,
                                                     ByVal locLCd As String,
                                                     ByVal functionId As String) As Boolean

            '更新可能拠点の取得
            Dim AuthInfo As AuthInfo = Misc.CommonRfid.MessageRfid.GetFunctionAuth(functionId, loginInfo)
            Dim updatableLocL = Dao.ToolLocation.GetUpdatableLocLList(loginInfo, AuthInfo)
            Dim updatableLocLList As List(Of String) = (From r In updatableLocL.AsEnumerable() Select r.Field(Of String)("LOC_CD")).ToList()

            '一時表テーブルにある資産番号に紐づく既存資産のロケコードを取得して、更新可能拠点以外を含むかチェック
            Dim existAssetLocList = GetExistAssetLocForTmpTable(db, importKey)
            Dim hasNotUpdatable As Boolean = existAssetLocList.Any(Function(loc) Not updatableLocLList.Contains(loc))

            If hasNotUpdatable Then
                lstErr.Add(New DataInportResult With {.ResultCd = DataInport.DataInportResultCd.ImportNotUpdatableComErr})
                Return True
            End If

            '一時表テーブル内の資産番号・サブ資産番号の重複チェック
            Dim dupList As List(Of DupAssetResult) = IsExistsAssetNoForTmpTable(db, importKey, locLCd)
            If dupList.Any() Then
                '行番号ごとにエラー格納
                For Each dup In dupList
                    Dim rowNos = dup.ROWS.Split(","c).Select(Function(x) x.Trim()).Where(Function(x) x <> "").ToList()
                    For Each rowNo In rowNos
                        lstErr.Add(New DataInportResult With {
                            .ErrRowIdx = Convert.ToInt32(rowNo),
                            .ErrKoumoku = "-",
                            .ErrGaiyou = Misc.CommonRfid.MessageRfid.getMessage(CommonErrorMsg.MSG_ERRNO_ASSETNO_JUUFUKU)
                        })
                    Next
                Next
                lstErr = lstErr.GroupBy(Function(x) New With {
                                                Key .ErrRowIdx = x.ErrRowIdx,
                                                Key .ErrKoumoku = x.ErrKoumoku,
                                                Key .ErrGaiyou = x.ErrGaiyou}) _
                                           .Select(Function(g) g.First()) _
                                           .OrderBy(Function(x) x.ErrRowIdx).ToList()
                lstErr(0).ResultCd = DataInport.DataInportResultCd.DataNG
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' 資産番号、サブ資産番号が他の資産で使用されているかどうかを返す
        ''' </summary>
        ''' <param name="db">Dbコンテキスト</param>
        ''' <param name="importKey">取込ごとのキー（入庫予定ワークの SESSION_ID）</param>
        ''' <param name="sLocLCd">ロケ大コード</param>
        ''' <returns>True:使用されている  False:使用されていない</returns>
        Private Shared Function IsExistsAssetNoForTmpTable(ByVal db As SystemDBContext,
                                                           ByVal importKey As String,
                                                           ByVal sLocLCd As String) As List(Of DupAssetResult)

            Dim tbl As New DataTable
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim lstParam As New List(Of SqlParameter)

            sbSQL.Append("WITH ALL_NOS (ASSET_NO, ROW_NO) AS (  ")
            sbSQL.Append("    SELECT ASSET_NO, ROW_NO FROM " & WORK_TABLE_NAME & " WHERE SESSION_ID = @ImportKey ")
            sbSQL.Append("    UNION ALL SELECT SUB_ASSET_NO, ROW_NO FROM " & WORK_TABLE_NAME & " WHERE SESSION_ID = @ImportKey ")
            lstParam.Add(New SqlParameter("@ImportKey", importKey))
            sbSQL.Append("    UNION ALL SELECT ASSET_NO, NULL AS ROW_NO FROM M_ASSET WHERE MNG_LOC_L_CD <> @LocLCd ")
            sbSQL.Append("    UNION ALL SELECT SUB_ASSET_NO, NULL AS ROW_NO FROM M_ASSET WHERE MNG_LOC_L_CD <> @LocLCd ")
            sbSQL.Append("    UNION ALL SELECT ASSET_NO, NULL AS ROW_NO FROM T_IN_ASSET WHERE MNG_LOC_L_CD <> @LocLCd ")
            sbSQL.Append("    UNION ALL SELECT SUB_ASSET_NO, NULL AS ROW_NO FROM T_IN_ASSET WHERE MNG_LOC_L_CD <> @LocLCd ")
            sbSQL.Append(") ")
            sbSQL.Append("SELECT ")
            sbSQL.Append("    AL.ASSET_NO ")
            sbSQL.Append("   ,STRING_AGG(CONVERT(VARCHAR(max), AL.ROW_NO), ',') AS ROWS ")
            sbSQL.Append("FROM ALL_NOS AL ")
            sbSQL.Append("WHERE ")
            sbSQL.Append("    AL.ASSET_NO IS NOT NULL ")
            sbSQL.Append("AND AL.ASSET_NO <> '' ")
            sbSQL.Append("GROUP BY AL.ASSET_NO ")
            sbSQL.Append("HAVING COUNT(ASSET_NO) > 1 ")

            lstParam.Add(New SqlParameter("@LocLCd", sLocLCd))

            Dim dupList = db.Database.SqlQuery(Of DupAssetResult)(sbSQL.ToString, lstParam.ToArray).ToList()
            Return dupList
        End Function

        ''' <summary>
        ''' 一時表にある資産番号に紐づくロケコードを取得する
        ''' </summary>
        ''' <param name="db">Dbコンテキスト</param>
        ''' <param name="importKey">取込ごとのキー（入庫予定ワークの SESSION_ID）</param>
        ''' <returns>True:使用されている  False:使用されていない</returns>
        Private Shared Function GetExistAssetLocForTmpTable(ByVal db As SystemDBContext, ByVal importKey As String) As List(Of String)

            Dim sbSQL As StringBuilder = New StringBuilder

            sbSQL.Append("SELECT ")
            sbSQL.Append("  DISTINCT AL.MNG_LOC_L_CD ")
            sbSQL.Append("FROM " & WORK_TABLE_NAME & " TMP ")
            sbSQL.Append("INNER JOIN (  ")
            sbSQL.Append("  SELECT ASSET_NO, MNG_LOC_L_CD FROM M_ASSET ")
            sbSQL.Append("  UNION ALL")
            sbSQL.Append("  SELECT ASSET_NO, MNG_LOC_L_CD FROM T_IN_ASSET ")
            sbSQL.Append(") AS AL ")
            sbSQL.Append("ON TMP.ASSET_NO = AL.ASSET_NO ")
            sbSQL.Append("WHERE TMP.SESSION_ID = @ImportKey ")

            Dim existAssetLocList = db.Database.SqlQuery(Of String)(sbSQL.ToString, New SqlParameter("@ImportKey", importKey)).ToList()
            Return existAssetLocList
        End Function
#End Region

#Region "DB操作処理"

        '''' <summary>
        '''' 一時表を作成する
        '''' </summary>
        '''' <param name="db">Dbコンテキスト</param>
        '''' <param name="sTmpTblNm">一時テーブル名</param>
        '''' <returns>DBコマンドの戻り値</returns>






        ''' <summary>
        ''' 入庫予定ワークのデータの集計結果（「はい」の時の確認用）
        ''' </summary>
        Private Class WorkSummary
            ''' <summary>取込ごとのキー・取込担当者に該当する件数</summary>
            Public Property ROW_COUNT As Integer
            ''' <summary>うち、会社（ロケ大コード）が指定と異なる件数</summary>
            Public Property OTHER_LOC_COUNT As Integer
            ''' <summary>取込対象行数（初回の取込で保存したExcel行数）</summary>
            Public Property IP_ROW_COUNT As Integer
            ''' <summary>取込ファイル名</summary>
            Public Property IP_FILE_NAME As String
        End Class

        ''' <summary>
        ''' 取込処理日時が2日以上前の入庫予定ワークのデータを削除する（設計書 手順①）
        ''' </summary>
        ''' <param name="db">DBコンテキスト</param>
        ''' <remarks>
        ''' 確認中のセッション切れ・画面を閉じた等で残ったデータを削除する。
        ''' 削除に失敗しても取込には影響しないため、ログを出力して処理を続行する。
        ''' </remarks>
        Private Shared Sub DeleteOldWork(ByVal db As SystemDBContext)
            Try
                Dim lBorder As Long = CLng(DateTime.Now.AddDays(-WORK_KEEP_DAYS).ToString("yyyyMMddHHmmss"))
                db.Database.ExecuteSqlCommand("DELETE FROM " & WORK_TABLE_NAME & " WHERE IP_DATETIME <= @IpDatetime",
                                              New SqlParameter("@IpDatetime", lBorder))
            Catch ex As Exception
                logger.Error("入庫予定ワークの古いデータの削除に失敗しました:" & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' 取込データを、取込ごとのキーを付けて入庫予定ワークに登録する（設計書 手順③）
        ''' </summary>
        ''' <param name="db">DBコンテキスト</param>
        ''' <param name="dtTmpAsset">取込データ（ReadAndValidation の戻り値）</param>
        ''' <param name="importKey">取込ごとのキー</param>
        ''' <param name="excelRowCount">Excel行数</param>
        ''' <remarks>
        ''' ワークは他の利用者も使うため、ロックを長く持たないよう、登録はこの処理だけでコミットする
        ''' （SqlBulkCopy の内部トランザクション）。
        ''' ワークは先頭がキー列のため、列は並び順ではなく列名で対応付ける。
        ''' </remarks>
        Private Shared Sub SaveToWork(ByVal db As SystemDBContext,
                                      ByVal dtTmpAsset As DataTable,
                                      ByVal importKey As String,
                                      ByVal excelRowCount As Integer)

            dtTmpAsset.Columns.Add("SESSION_ID", GetType(String))
            dtTmpAsset.Columns.Add("IP_ROW_COUNT", GetType(Integer))
            For Each dr As DataRow In dtTmpAsset.Rows
                dr("SESSION_ID") = importKey
                dr("IP_ROW_COUNT") = excelRowCount
            Next

            Dim conn = CType(db.Database.Connection, SqlClient.SqlConnection)
            If conn.State = ConnectionState.Closed Then conn.Open()

            Using bulk As New SqlClient.SqlBulkCopy(conn, SqlClient.SqlBulkCopyOptions.UseInternalTransaction, Nothing)
                bulk.DestinationTableName = WORK_TABLE_NAME
                For Each col As DataColumn In dtTmpAsset.Columns
                    bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName)
                Next
                bulk.WriteToServer(dtTmpAsset)
            End Using
        End Sub

        ''' <summary>
        ''' 取込ごとのキーを条件に入庫予定ワークのデータを削除する（設計書 手順⑭・「いいえ」）
        ''' </summary>
        ''' <param name="importKey">取込ごとのキー</param>
        ''' <param name="loginInfo">ログイン情報</param>
        ''' <remarks>
        ''' 他の社員のデータを削除しないよう、取込担当者コードがログイン中の社員であることも条件にする。
        ''' 呼び出し元のトランザクションの状態に左右されないよう、別のDBコンテキストで実行する。
        ''' 削除に失敗しても、残ったデータは2日経過後の取込時に削除されるため、ログを出力して処理を続行する。
        ''' </remarks>
        Private Shared Sub DeleteWork(ByVal importKey As String,
                                      ByVal loginInfo As RfidEntity.LoginUser)
            Try
                Using db As New SystemDBContext
                    db.Database.ExecuteSqlCommand("DELETE FROM " & WORK_TABLE_NAME & " WHERE SESSION_ID = @ImportKey AND IP_EMPLOYEE_CD = @EmployeeCd",
                                                  New SqlParameter("@ImportKey", importKey),
                                                  New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd))
                End Using
            Catch ex As Exception
                logger.Error("入庫予定ワークのデータの削除に失敗しました（キー:" & importKey & "）:" & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' 入庫予定ワークのデータの件数・会社・取込ファイル名を取得する（「はい」の時の確認用）
        ''' </summary>
        ''' <param name="db">DBコンテキスト</param>
        ''' <param name="importKey">取込ごとのキー</param>
        ''' <param name="locLCd">ロケ大コード（画面の検索条件-ロケーション会社）</param>
        ''' <param name="loginInfo">ログイン情報</param>
        ''' <returns>集計結果（該当データが無い場合は件数0）</returns>
        Private Shared Function GetWorkSummary(ByVal db As SystemDBContext,
                                               ByVal importKey As String,
                                               ByVal locLCd As String,
                                               ByVal loginInfo As RfidEntity.LoginUser) As WorkSummary
            Dim sbSQL As New StringBuilder()
            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("    COUNT(*) AS ROW_COUNT")
            sbSQL.AppendLine("  , ISNULL(SUM(CASE WHEN LOC_L_CD = @LocLCd THEN 0 ELSE 1 END), 0) AS OTHER_LOC_COUNT")
            sbSQL.AppendLine("  , ISNULL(MAX(IP_ROW_COUNT), COUNT(*)) AS IP_ROW_COUNT")
            sbSQL.AppendLine("  , MAX(IP_FILE_NAME) AS IP_FILE_NAME")
            sbSQL.AppendLine("FROM " & WORK_TABLE_NAME)
            sbSQL.AppendLine("WHERE SESSION_ID = @ImportKey")
            sbSQL.AppendLine("AND IP_EMPLOYEE_CD = @EmployeeCd")

            Return db.Database.SqlQuery(Of WorkSummary)(sbSQL.ToString(),
                                                        New SqlParameter("@LocLCd", If(locLCd, String.Empty)),
                                                        New SqlParameter("@ImportKey", importKey),
                                                        New SqlParameter("@EmployeeCd", loginInfo.EmployeeCd)).First()
        End Function

        ''' <summary>
        ''' 入庫予定ワークから、取込ごとのキーのデータで一時表を作成する
        ''' </summary>
        ''' <param name="db">DBコンテキスト</param>
        ''' <param name="sTmpTblNm">一時テーブル名</param>
        ''' <param name="importKey">取込ごとのキー</param>
        ''' <remarks>
        ''' 一時表の列はワークと同じになる（ストアドは一時表の列を名前で参照しており、キー列が増えても影響しない）。
        ''' パラメータ付きのSQLは sp_executesql の中で実行され、作成した一時表が実行後に消えるため、
        ''' 一時表の作成はパラメータなしで行い、データの写しは別に行う。
        ''' </remarks>
        Private Shared Sub CreateTempTableFromWork(ByVal db As SystemDBContext,
                                                   ByVal sTmpTblNm As String,
                                                   ByVal importKey As String)
            db.Database.ExecuteSqlCommand("SELECT * INTO " & sTmpTblNm & " FROM " & WORK_TABLE_NAME & " WHERE 1 = 0")

            db.Database.ExecuteSqlCommand("INSERT INTO " & sTmpTblNm & " SELECT * FROM " & WORK_TABLE_NAME & " WHERE SESSION_ID = @ImportKey",
                                          New SqlParameter("@ImportKey", importKey))
        End Sub

        ''' <summary>
        ''' 資産台帳取込で削除されるデータの件数を取得する。
        ''' </summary>
        ''' <param name="db">DBコンテキスト</param>
        ''' <param name="importKey">取込ごとのキー</param>
        ''' <param name="locLCd">ロケ大区分コード</param>
        ''' <returns>削除対象の件数（入庫予定 ＋ 資産マスタ の合算）</returns>
        ''' <remarks>
        ''' 【重要】削除条件はストアド dbo.IMPORT_ASSET_FILE の削除処理と同一にすること。
        ''' 　取込を実行する前に件数だけを知る必要があるため、やむを得ず条件を二重に持っている。
        ''' 　ストアド側の削除条件を変更した場合は、本メソッドも必ず合わせて修正すること。
        '''
        ''' 　ストアド側の該当処理（抜粋）
        ''' 　　DELETE FROM T_IN_ASSET
        ''' 　　WHERE MNG_LOC_L_CD = @IN_LOC_L_CD
        ''' 　　AND NOT EXISTS (SELECT 'X' FROM [一時表] AS W WHERE W.ASSET_NO = T_IN_ASSET.ASSET_NO)
        '''
        ''' 　　DELETE FROM M_ASSET
        ''' 　　WHERE MNG_LOC_L_CD = @IN_LOC_L_CD
        ''' 　　AND NOT EXISTS (SELECT 'X' FROM [一時表] AS W WHERE W.ASSET_NO = M_ASSET.ASSET_NO)
        '''
        ''' 　ストアドに渡す一時表は、入庫予定ワークを取込ごとのキーで絞ったものと同じため、
        ''' 　本メソッドでは入庫予定ワークを取込ごとのキーで絞って数える。
        ''' 　カウント対象は各テーブルの資産番号（主キー・NOT NULL）とする。
        ''' </remarks>
        Public Shared Function GetDeleteTargetCount(ByVal db As SystemDBContext,
                                                    ByVal importKey As String,
                                                    ByVal locLCd As String) As Integer
            Dim sbSQL As New StringBuilder()
            Dim parameters As New List(Of SqlParameter)()

            sbSQL.AppendLine("SELECT")
            sbSQL.AppendLine("    (")
            sbSQL.AppendLine("        SELECT COUNT(T_IN_ASSET.ASSET_NO)")
            sbSQL.AppendLine("        FROM T_IN_ASSET")
            sbSQL.AppendLine("        WHERE MNG_LOC_L_CD = @IN_LOC_L_CD")
            sbSQL.AppendLine("        AND NOT EXISTS (")
            sbSQL.AppendLine("            SELECT 'X'")
            sbSQL.AppendLine("            FROM " & WORK_TABLE_NAME & " AS W")
            sbSQL.AppendLine("            WHERE W.SESSION_ID = @ImportKey")
            sbSQL.AppendLine("            AND W.ASSET_NO = T_IN_ASSET.ASSET_NO")
            sbSQL.AppendLine("        )")
            sbSQL.AppendLine("    )")
            sbSQL.AppendLine("  + (")
            sbSQL.AppendLine("        SELECT COUNT(M_ASSET.ASSET_NO)")
            sbSQL.AppendLine("        FROM M_ASSET")
            sbSQL.AppendLine("        WHERE MNG_LOC_L_CD = @IN_LOC_L_CD")
            sbSQL.AppendLine("        AND NOT EXISTS (")
            sbSQL.AppendLine("            SELECT 'X'")
            sbSQL.AppendLine("            FROM " & WORK_TABLE_NAME & " AS W")
            sbSQL.AppendLine("            WHERE W.SESSION_ID = @ImportKey")
            sbSQL.AppendLine("            AND W.ASSET_NO = M_ASSET.ASSET_NO")
            sbSQL.AppendLine("        )")
            sbSQL.AppendLine("    )")

            parameters.Add(New SqlParameter("@IN_LOC_L_CD", locLCd))
            parameters.Add(New SqlParameter("@ImportKey", importKey))

            Return db.Database.SqlQuery(Of Integer)(sbSQL.ToString(), parameters.ToArray()).FirstOrDefault()
        End Function

        ''' <summary>
        ''' 一時表をもとに各テーブル登録/更新/削除を行うストアドを実行する
        ''' </summary>
        ''' <param name="db">Dbコンテキスト</param>
        ''' <param name="sTmpTblNm">一時テーブル名</param>
        ''' <param name="locLCd">ロケーションコード</param>
        ''' <param name="loginInfo">ログイン情報</param>
        ''' <param name="functionId">呼び出し元画面の権限機能ID</param>
        ''' <returns>ストアドの戻り値（正常:1、異常:9など）</returns>
        Private Shared Function ExecuteAssetImportProcess(ByVal db As SystemDBContext,
                                                          ByVal sTmpTblNm As String,
                                                          ByVal locLCd As String,
                                                          ByVal loginInfo As RfidEntity.LoginUser,
                                                          ByVal functionId As String) As Integer
            Dim sbSQL As New StringBuilder()
            Dim parameters As New List(Of SqlParameter)()

            ' SQL 組み立て
            sbSQL.AppendLine("EXEC dbo.IMPORT_ASSET_FILE")
            sbSQL.AppendLine("    @IN_TMPTBL_NAME = @IN_TMPTBL_NAME,")
            sbSQL.AppendLine("    @IN_LOC_L_CD    = @IN_LOC_L_CD,")
            sbSQL.AppendLine("    @OUT_RETURN     = @OUT_RETURN OUTPUT,")
            sbSQL.AppendLine("    @OUT_MESSAGE    = @OUT_MESSAGE OUTPUT;")

            ' パラメータ定義
            parameters.Add(New SqlParameter("@IN_TMPTBL_NAME", sTmpTblNm))
            parameters.Add(New SqlParameter("@IN_LOC_L_CD", locLCd))

            ' 戻り値用の出力パラメータ定義
            Dim returnParam As New SqlParameter("@OUT_RETURN", SqlDbType.Int)
            returnParam.Direction = ParameterDirection.Output
            parameters.Add(returnParam)

            Dim messageParam As New SqlParameter("@OUT_MESSAGE", SqlDbType.NVarChar, 4000)
            messageParam.Direction = ParameterDirection.Output
            parameters.Add(messageParam)

            ' 実行
            db.Database.ExecuteSqlCommand(sbSQL.ToString(), parameters.ToArray())

            ' 戻り値取得
            Dim retCode As Integer = parameters.Where(Function(x) x.ParameterName = "@OUT_RETURN").FirstOrDefault.Value

            Dim retMessage As String = ""
            If messageParam IsNot Nothing AndAlso messageParam.Value IsNot DBNull.Value Then
                retMessage = messageParam.Value.ToString()

                'エラーメッセージがあれば操作ログに出力する
                Dim sTitle As String = Misc.CommonRfid.MessageRfid.getMessage((GetDispTitle(functionId)))
                Dim sAction As String = Misc.CommonRfid.MessageRfid.getMessage((CommonLabel.BUTTON_ASSET_LEDGER_IMPORT))
                Dim sProcessContents As String = sTitle & " " & sAction & " " & retMessage
                Dao.ToolOperationLog.InsOpeLog(Misc.ProcessCat.Event, sProcessContents, loginInfo, sTitle)
            End If

            Return retCode
        End Function

#End Region

#Region "画面別のタイトル文言定義を取得する"

        ''' <summary>
        ''' 画面別のタイトル文言定義を取得する
        ''' </summary>
        ''' <param name="functionId">呼び出し元画面の権限機能ID</param>
        ''' <returns>画面別のタイトル文言定義</returns>
        Private Shared Function GetDispTitle(ByVal functionId As String) As String
            Dim titleCd As String = String.Empty
            Select Case functionId
                Case Misc.Auth.FunctionId.AssetList
                    titleCd = CommonLabelAssetList.TITLE
                Case Misc.Auth.FunctionId.AssetLabelPrint
                    titleCd = CommonLabelAssetLabelPrint.TITLE
            End Select
            Return titleCd
        End Function

#End Region
    End Class

End Namespace
