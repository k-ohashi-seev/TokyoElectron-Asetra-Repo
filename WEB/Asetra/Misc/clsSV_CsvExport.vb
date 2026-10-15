Imports System.IO
Imports System.Reflection.MethodBase
Imports System.Text

''' <summary>
''' 株式会社 シービジョン
''' クラス名：CSVファイル出力操作クラス
''' 作成日  ：2018/04/01  Ver.1.0.0.0
''' 修正日  ：0000/00/00  Ver.0.0.0.0
''' </summary>
''' <remarks></remarks>
Public Class clsSV_CsvExport

#Region "定数定義"
    ''' <summary>クラス名</summary>
    Private Const CLASS_NAME As String = "clsSV_CsvExport"

#End Region

#Region "プロパティ"
    ''' <summary>CSV出力データリスト</summary>
    Private _csvDataList As List(Of String)
    ''' <summary>CSV出力データリスト</summary>
    Public Property p_csvDataList As List(Of String)
        Get
            Return _csvDataList
        End Get
        Set(ByVal Value As List(Of String))
            _csvDataList = Value
        End Set
    End Property

    ''' <summary>CSV出力ファイル名</summary>
    Private _saveFileName As String = String.Empty
    ''' <summary>CSV出力ファイル名</summary>
    Public Property p_saveFileName As String
        Get
            Return _saveFileName
        End Get
        Set(ByVal Value As String)
            _saveFileName = Value
        End Set
    End Property

    ''' <summary>文字エンコード</summary>
    Private _encoding As Encoding = Encoding.Default
    ''' <summary>文字エンコード</summary>
    Public WriteOnly Property p_encoding As Encoding
        Set(ByVal value As Encoding)
            _encoding = value
        End Set
    End Property

    ''' <summary>ログ用アプリケーション名</summary>
    Private _logAppName As String = String.Empty
    ''' <summary>ログ用 アプリケーション名</summary>
    Public WriteOnly Property p_logAppName As String
        Set(ByVal value As String)
            _logAppName = value
        End Set
    End Property

#End Region

#Region "コンストラクタ"
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        _csvDataList = New List(Of String)
    End Sub

#End Region

#Region "ファイル保存先セット処理"
    ''' <summary>
    ''' ファイル保存先セット処理
    ''' </summary>
    ''' <param name="prmInitPath">初期表示フォルダ</param>
    ''' <param name="prmInitFile">初期表示ファイル</param>
    ''' <returns>True=ファイル選択, False=非選択</returns>
    ''' <remarks></remarks>
    Public Function SVFncGetSaveFileName(ByVal prmInitPath As String, Optional ByVal prmInitFile As String = "*.csv") As Boolean
        Dim funcName As String = GetCurrentMethod().Name
        Try
            Me._saveFileName = prmInitPath + "\" + prmInitFile         '選択ファイル名

            Return True

        Catch ex As Exception
            'Call SVSubWriteLog(EN_LogType.Err, CLASS_NAME, funcName, LOG_ERR_CATHCH_TITLE, ex.Message, _logAppName)
            Return False
        End Try
    End Function

#End Region

#Region "CSVファイル出力"
    ''' <summary>
    ''' CSVファイル出力
    ''' </summary>
    ''' <param name="prmAppend">ファイル上書き種別 True=追記, False=上書き</param>
    ''' <remarks>True=ファイル出力正常終了, False=ファイル出力異常終了</remarks>
    Public Function SVFncCSVFileOutPut(Optional ByVal prmAppend As Boolean = False) As Boolean
        Dim funcName As String = GetCurrentMethod().Name
        Try
            Using sw = New StreamWriter(_saveFileName, prmAppend, _encoding)
                For Each rec As String In _csvDataList
                    sw.WriteLine(rec)
                Next
            End Using

            Return True

        Catch ex As DirectoryNotFoundException
            Return False
        End Try

    End Function

#End Region

#Region "列単位　ダブルクオーテーション、エスケープ処理"
    ''' <summary>
    ''' 列単位　ダブルクオーテーション、エスケープ処理
    ''' </summary>
    ''' <param name="item">列項目値</param>
    ''' <returns>編集後列文字列</returns>
    ''' <remarks></remarks>
    Public Function SVFncCSVAddDquot(ByVal item As Object) As String

        item = Misc.CommonRfid.ToStringNvl(item)

        'ダブルクオーテーションが含まれているか確認
        ' => 2重化してエスケープ
        If item.Contains("""") = True Then
            item = item.Replace("""", """""")
        End If
        '両端にダブルクオーテーションを入れてエスケープ
        item = """" & item & """"

        Return item
    End Function

#End Region

#Region "ダブルクオーテーションで囲まれていないヘッダ文字列を囲む文字列にする"
    ''' <summary>
    ''' ダブルクオーテーションで囲まれていないヘッダ文字列を囲む文字列にする
    ''' </summary>
    ''' <param name="sHed">ヘッダ文字列</param>
    ''' <returns>編集後列文字列</returns>
    ''' <remarks></remarks>
    Public Function SVFncCSVAddDquotHed(ByVal sHed As String) As String
        Return """" & sHed.Replace(",", """,""") & """"
    End Function

#End Region

End Class
