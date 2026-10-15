
''' <summary>
''' 送信用マスタデータパラメータ
''' </summary>
Public Class HtTHGetMasterDataCmdResParameter
    Inherits CommonResParameterTH

    ''' <summary>
    ''' 資産マスタデータ
    ''' </summary>
    Property adl As AssetData()

    ''' <summary>
    ''' 社員マスタデータ
    ''' </summary>
    Property employeeDataList As EmployeeData()

    ''' <summary>
    ''' 社員所属拠点マスタデータ
    ''' </summary>
    ''' <returns></returns>
    Property employeeHubDataList As EmployeeHubData()

    ''' <summary>
    ''' 設定マスタデータ
    ''' </summary>
    Property settingDataList As SettingData()

    ''' <summary>
    ''' 権限マスタデータ
    ''' </summary>
    ''' <returns></returns>
    Property authDataList As AuthData()

    ''' <summary>
    ''' 資産マスタデータ
    ''' </summary>
    Public Class AssetData
        ''' <summary>
        ''' 資産番号
        ''' </summary>
        Property no As String

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
        ''' 資産状態
        ''' </summary>
        Property st As String

        ''' <summary>
        ''' 貸出状態
        ''' </summary>
        ''' <returns></returns>
        Property ls As String

        ''' <summary>
        ''' 管理ロケ大区分コード
        ''' </summary>
        ''' <returns></returns>
        Property mll As String
    End Class
End Class
