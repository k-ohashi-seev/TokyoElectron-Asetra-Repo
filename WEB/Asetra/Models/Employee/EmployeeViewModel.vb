Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' 社員マスタメンテ用表示用モデル
    ''' </summary>
    Partial Public Class EmployeeViewModel
        Inherits BaseModel

        ''' <summary>
        ''' 社員コード
        ''' </summary>
        ''' <returns>社員コード</returns>
        <Key>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(15, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' 社員名
        ''' </summary>
        ''' <returns>社員名</returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(60, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' パスワード
        ''' </summary>
        ''' <returns>パスワード</returns>
        <StringLength(60)>
        Public Property PASSWORD As String = String.Empty

        ''' <summary>
        ''' 部署コード
        ''' </summary>
        ''' <returns>部署コード</returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(15)>
        Public Property DEPARTMENT_CD As String = String.Empty

        ''' <summary>
        ''' 権限
        ''' </summary>
        ''' <returns>権限</returns>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(1)>
        Public Property AUTH_DIV As String = String.Empty

        ''' <summary>
        ''' 退職フラグ			
        ''' </summary>
        ''' <returns>退職フラグ</returns>
        Public Property RETIRE_FLG As String = String.Empty

        ''' <summary>
        ''' メールアドレス
        ''' </summary>
        ''' <returns>メールアドレス</returns>
        <StringLength(itemlength.MailAddress, ErrorMessage:=CommonLabel.MSG_ERR_MAXLENGTH)>
        Public Property MAIL_ADDRESS As String = String.Empty

        ''' <summary>
        ''' 登録日時
        ''' </summary>
        ''' <returns>登録日時</returns>
        <DisplayFormat(DataFormatString:="{0:yyyy/MM/dd HH:mm}")>
        Public Property INS_DATETIME As Date? = Nothing

        ''' <summary>
        ''' 登録者社員コード
        ''' </summary>
        ''' <returns>登録者社員コード</returns>
        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' 登録者氏名
        ''' </summary>
        ''' <returns>登録者氏名</returns>
        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' 最終登録更新日時
        ''' </summary>
        ''' <returns>最終登録更新日時</returns>
        <DisplayFormat(DataFormatString:="{0:yyyy/MM/dd HH:mm}")>
        Public Property UPD_DATETIME As Date? = Nothing

        ''' <summary>
        ''' 最終登録更新者社員コード
        ''' </summary>
        ''' <returns>最終登録更新者社員コード</returns>
        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' 最終登録更新者氏名
        ''' </summary>
        ''' <returns>最終登録更新者氏名</returns>
        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' 最終登録更新処理
        ''' </summary>
        ''' <returns>最終登録更新処理</returns>
        <StringLength(255)>
        Public Property UPD_TASK As String = String.Empty

        ''' <summary>
        ''' 編集フラグ			
        ''' </summary>
        ''' <returns>編集フラグ</returns>
        ''' <remarks>Y=編集可　N=編集不可</remarks>
        Public Property UPD_FLG As String = AUTH_FLG.No

        ''' <summary>
        ''' 所属拠点大区分コード(CSV出力用)
        ''' </summary>
        ''' <returns>表示項目：所属会社1</returns>
        Public Property HUB_LOC_L_CD As String = String.Empty

        ''' <summary>
        ''' 所属拠点大区分名(CSV出力・グリッド表示用)
        ''' </summary>
        ''' <returns>表示項目：所属会社1</returns>
        Public Property HUB_LOC_L_NAME As String = String.Empty

        ''' <summary>
        ''' 所属拠点中区分コード(CSV出力用)
        ''' </summary>
        ''' <returns>表示項目：所属部署1</returns>
        'Public Property HUB_LOC_M_CD As String = String.Empty

        ''' <summary>
        ''' 所属拠点中区分名(CSV出力・グリッド表示用)
        ''' </summary>
        ''' <returns>表示項目：所属部署1</returns>
        'Public Property HUB_LOC_M_NAME As String = String.Empty

        ''' <summary>
        ''' 検索項目：社員コード
        ''' </summary>
        ''' <returns>検索項目：社員コード</returns>
        <NotMapped>
        Public Property SearchCode As String = String.Empty

        ''' <summary>
        ''' 検索項目：社員名
        ''' </summary>
        ''' <returns>検索項目：社員名</returns>
        <NotMapped>
        Public Property SearchName As String = String.Empty

        ''' <summary>
        ''' 検索項目：部署
        ''' </summary>
        ''' <returns>検索項目：部署</returns>
        <NotMapped>
        Public Property SearchBusho As String = String.Empty

        ''' <summary>
        ''' 検索項目：所属拠点
        ''' </summary>
        ''' <returns>検索項目：所属拠点大区分</returns>
        <NotMapped>
        Public Property SearchHubLocLCd As String = String.Empty

        ''' <summary>
        ''' 検索項目：所属拠点
        ''' </summary>
        ''' <returns>検索項目：所属拠点中区分</returns>
        <NotMapped>
        Public Property SearchHubLocMCd As String = String.Empty

        ''' <summary>
        ''' 検索項目：退職者　含めるか含めないか
        ''' </summary>
        ''' <returns>検索項目：退職者　含めるか含めないか</returns>
        <NotMapped>
        Public Property SearchRetire As Boolean = False

        ''' <summary>
        ''' 検索項目：権限(拠点権限：使用しない　の場合)
        ''' </summary>
        ''' <returns>検索項目：権限</returns>
        <NotMapped>
        Public Property SearchKengen_KyotenN As String = String.Empty

        ''' <summary>
        ''' 検索項目：権限(拠点権限：使用する　の場合)
        ''' </summary>
        ''' <returns>検索項目：権限</returns>
        <NotMapped>
        Public Property SearchKengen_KyotenY As String = String.Empty

        ''' <summary>
        ''' 表示項目：部署名
        ''' </summary>
        ''' <returns>表示項目：部署名</returns>
        <StringLength(60)>
        Public Property DEPARTMENT_NAME As String = String.Empty

        ''' <summary>
        ''' 表示項目：権限
        ''' </summary>
        ''' <returns>表示項目：権限</returns>
        <NotMapped>
        Public Property AUTH_DIV_NAME As String = String.Empty

        ''' <summary>
        ''' 表示項目：退職フラグ　チェックボックス表示用
        ''' </summary>
        ''' <returns>退職フラグ　チェックボックス表示用</returns>
        <NotMapped>
        Public Property DispRetireFlg As Boolean = False

        '' <summary>
        '' 表示項目：所属拠点大区分
        '' </summary>
        '' <returns>表示項目：所属拠点大区分1</returns>
        <NotMapped>
        Public Property HUB_LOC_L As List(Of String)

        '' <summary>
        '' 表示項目：所属拠点中区分
        '' </summary>
        '' <returns>表示項目：所属拠点中区分</returns>
        <NotMapped>
        Public Property HUB_LOC_M As List(Of String)

        ''' <summary>
        '''  大区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>大区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property locLDropdownData As List(Of LocationDropdownModel)
        ''' <summary>
        '''  大区分ドロップダウンリストデータ(配列)
        ''' </summary>
        ''' <returns>大区分ドロップダウンリストデータ(配列)</returns>
        <NotMapped>
        Public Property hubLocLDropdownDatas As List(Of List(Of LocationDropdownModel))
        Public locLOptions As List(Of SelectList)

        ''' <summary>
        '''  中区分ドロップダウンリストデータ
        ''' </summary>
        ''' <returns>中区分ドロップダウンリストデータ(配列)</returns>
        <NotMapped>
        Public Property locMDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  中区分ドロップダウンリストデータ(配列)
        ''' </summary>
        ''' <returns>中区分ドロップダウンリストデータ</returns>
        <NotMapped>
        Public Property hubLocMDropdownDatas As List(Of List(Of LocationDropdownModel))
        Public locMOptions As List(Of SelectList)
        '----------ログイン画面で使用
        ''' <summary>
        ''' 入力項目：新パスワード
        ''' </summary>
        ''' <returns>入力項目：新パスワード</returns>
        <NotMapped>
        <StringLength(60)>
        <DataType(DataType.Password)>
        Public Property NewPassword As String = String.Empty

        ''' <summary>
        ''' 入力項目：確認パスワード
        ''' </summary>
        ''' <returns>入力項目：確認パスワード</returns>
        <NotMapped>
        <StringLength(60)>
        <DataType(DataType.Password)>
        Public Property KakuninPassword As String = String.Empty

        ''' <summary>
        ''' 登録日時 文字列
        ''' </summary>
        ''' <returns>登録日時 文字列</returns>
        <NotMapped>
        Public Property InsDatetimeString As String = String.Empty

        ''' <summary>
        ''' 最終登録更新日時 文字列
        ''' </summary>
        ''' <returns>最終登録更新日時 文字列</returns>
        <NotMapped>
        Public Property UpdDatetimeString As String = String.Empty

        ''' <summary>
        ''' 固定列
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property FixColCnt As Integer

        ''' <summary>
        ''' ロケマスタ情報
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property GridInfo As Dictionary(Of String, SlickGridInfo)

        ''' <summary>
        ''' 選択キーリスト(カンマ区切り)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property SelectKeys As String

        ''' <summary>
        ''' 未選択キーリスト(カンマ区切り)
        ''' </summary>
        ''' <returns></returns>
        <NotMapped>
        Public Property NoneSelectKeys As String

        ''' <summary>
        ''' プリンタ情報KEY1
        ''' </summary>
        ''' <returns>プリンタ情報KEY1</returns>
        <NotMapped>
        Public Property PrinterKey As String = String.Empty

        ''' <summary>
        ''' 自画面内の検索条件復帰有無
        ''' </summary>
        ''' <returns>自画面内の検索条件復帰有無</returns>
        <NotMapped>
        Public Property IsRestoreOwnSearchConditions As Boolean = False

        ''' <summary>
        ''' ローカルストレージがある場合の初回検索
        ''' </summary>
        ''' <returns>ローカルストレージがある場合の初回検索</returns>
        <NotMapped>
        Public Property FirstSearch As Boolean = False

        Sub New()
            '所属拠点関連リスト初期化
            hubLocLDropdownDatas = New List(Of List(Of LocationDropdownModel))
            hubLocMDropdownDatas = New List(Of List(Of LocationDropdownModel))
            locLOptions = New List(Of SelectList)
            locMOptions = New List(Of SelectList)
            HUB_LOC_L = New List(Of String)
            HUB_LOC_M = New List(Of String)

            '所属拠点関連リストに所属拠点数分データを追加する
            For i As Integer = 1 To CommonConst.MAX_HUBLOC_CNT
                hubLocLDropdownDatas.Add(Nothing)
                hubLocMDropdownDatas.Add(Nothing)

                locLOptions.Add(New SelectList(String.Empty))
                locMOptions.Add(New SelectList(String.Empty))

                HUB_LOC_L.Add(String.Empty)
                HUB_LOC_M.Add(String.Empty)
            Next
        End Sub

    End Class

End Namespace
