Imports System.Data.SqlClient
Imports Asetra.Models

Namespace Misc
    ''' <summary>
    ''' メッセージをDBから取得し保持するクラス
    ''' </summary>
    Public Class MessageRfid

        ''' <summary>DBデータの改行を表す文字列です。</summary>
        Private Const BreakChar As String = "CRLF"

        Private _MessageList As List(Of M_WORDModel)    'メッセージリスト
        Private _SettingList As List(Of SettingInfo)    '設定リスト
        Private _AuthList As List(Of AuthInfo)          '権限リスト

        ''' <summary>
        ''' メッセージをDBから取得します。
        ''' (注意)Loadを呼び出す前後で、Application.Lock,Application.Unlockをすること
        ''' </summary>
        Public Sub Load()
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim db As SystemDBContext = New SystemDBContext

            'M_WORDテーブルから取得するSQLを作成
            sbSQL.Append("SELECT ")
            'TODO:
            'sbSQL.Append("  SYSTEM_DIV + WORD_DIV + WORD_CD AS MESSAGE_KEY")
            sbSQL.Append(" TOOL_DIV + WORD_DIV + WORD_CD AS MESSAGE_KEY")
            sbSQL.Append(", WORDING ")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_WORD ")
            sbSQL.Append("WHERE ")
            sbSQL.Append("  LANG_DIV = @lang_div ")

            'SQLを実行する(メッセージのインスタンス変数へのセット)
            _MessageList = db.M_WORD.SqlQuery(sbSQL.ToString, New SqlParameter("@lang_div", Misc.LangDiv.LU)).ToList

            sbSQL.Clear()
            sbSQL.Append("SELECT ")
            sbSQL.Append("  KEY_DIV ")
            sbSQL.Append("  , KEY1 ")
            sbSQL.Append("  , KEY2 ")
            sbSQL.Append("  , KEY3 ")
            sbSQL.Append("  , REMARK ")
            sbSQL.Append("  , DATA1 ")
            sbSQL.Append("  , DATA2 ")
            sbSQL.Append("  , DATA3 ")
            sbSQL.Append("  , DATA4 ")
            sbSQL.Append("  , DATA5 ")
            sbSQL.Append("  , DATA6 ")
            sbSQL.Append("  , DATA7 ")
            sbSQL.Append("  , DATA8 ")
            sbSQL.Append("  , DATA9 ")
            sbSQL.Append("  , DATA10 ")
            sbSQL.Append("  , SPECIAL_DATA_DIV ")
            sbSQL.Append("  , FUNC_CAT_NAME1 ")
            sbSQL.Append("  , FUNC_CAT_NAME2 ")
            sbSQL.Append("  , FUNC_CAT_NAME3 ")
            sbSQL.Append("  , SETTING_OBJECT_DIV ")
            sbSQL.Append("  , FUNC_CAT_SORT ")
            sbSQL.Append("  , UPD_AUTHORITY_FLG ")
            sbSQL.Append("  , DEL_AUTHORITY_FLG ")
            sbSQL.Append("  , INS_DATETIME ")
            sbSQL.Append("  , INS_EMPLOYEE_CD ")
            sbSQL.Append("  , INS_EMPLOYEE_NAME ")
            sbSQL.Append("  , UPD_DATETIME ")
            sbSQL.Append("  , UPD_EMPLOYEE_CD ")
            sbSQL.Append("  , UPD_EMPLOYEE_NAME ")
            sbSQL.Append("  , UPD_TASK ")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_SETTING ")

            sbSQL.Append(" ORDER BY KEY_DIV, KEY1, KEY2, KEY3")

            'SQLを実行する
            Dim lstSetting As List(Of SettingModel) = db.Setting.SqlQuery(sbSQL.ToString).ToList

            '(設定情報のインスタンス変数へのセット)
            CopySettingModelData(lstSetting)

            '----- 権限情報
            sbSQL.Clear()
            sbSQL.Append("SELECT ")
            sbSQL.Append("    PROCESS_TOOL_DIV ")
            sbSQL.Append("  , MENU_ID ")
            sbSQL.Append("  , FUNCTION_ID ")
            sbSQL.Append("  , AUTH_DIV ")
            sbSQL.Append("  , AUTH_TYPE ")
            sbSQL.Append("  , HEAD_OFFICE_FLG ")
            sbSQL.Append("  , READ_AUTH_FLG ")
            sbSQL.Append("  , UPD_AUTH_FLG ")
            sbSQL.Append("  , READ_HUB_AUTH_FLG ")
            sbSQL.Append("  , UPD_HUB_AUTH_FLG ")
            sbSQL.Append("  , AUTH_OPTION ")
            sbSQL.Append("  , INS_DATETIME ")
            sbSQL.Append("  , INS_EMPLOYEE_CD ")
            sbSQL.Append("  , INS_EMPLOYEE_NAME ")
            sbSQL.Append("  , UPD_DATETIME ")
            sbSQL.Append("  , UPD_EMPLOYEE_CD ")
            sbSQL.Append("  , UPD_EMPLOYEE_NAME ")
            sbSQL.Append("  , UPD_TASK ")
            sbSQL.Append("FROM ")
            sbSQL.Append("  M_AUTH ")
            sbSQL.Append("WHERE PROCESS_TOOL_DIV = @ProcessToolDiv_Web")

            sbSQL.Append(" ORDER BY MENU_ID, FUNCTION_ID, AUTH_DIV, AUTH_TYPE, HEAD_OFFICE_FLG")

            'SQLを実行する
            Dim lstAuth As List(Of AuthModel) = db.Auth.SqlQuery(sbSQL.ToString, New SqlParameter("@ProcessToolDiv_Web", Misc.ProcessToolDiv.Web)).ToList

            '(設定情報のインスタンス変数へのセット)
            CopyAuthModelData(lstAuth)

        End Sub

        ''' <summary>
        ''' メッセージを返します。
        ''' キーがnullの場合、ブランクを返します。
        ''' </summary>
        ''' <param name="strKey"></param>
        ''' <returns></returns>
        Public Function getMessage(ByVal strKey As String) As String
            Dim strMessage As String = String.Empty

            'キーが存在しない場合ブランクを返却
            If IsNothing(strKey) Then
                Return strMessage
            End If

            'キーに該当する要素を取得
            Dim itemData As IEnumerable(Of M_WORDModel) = Me._MessageList.Where(Function(item) item.MESSAGE_KEY = strKey)
            If Not IsNothing(itemData) AndAlso itemData.Count > 0 Then
                strMessage = itemData.ElementAt(0).WORDING
            End If

            '--2021/10/04 sv suzuki del start
            ''ロケーション文字列変換
            'Dim setting As SettingInfo = getSettingInfo(CommonSettingKey.LocationUsage.KeyDiv, CommonSettingKey.LocationUsage.Key1, CommonSettingKey.LocationUsage.Key2, CommonSettingKey.LocationUsage.Key3)
            'If Not IsNothing(setting) Then
            '    strMessage = strMessage.Replace(Misc.CommonConst.WORD_LOC_L_CD, setting.DATA4)      '大区分文字列置き換え
            '    strMessage = strMessage.Replace(Misc.CommonConst.WORD_LOC_M_CD, setting.DATA5)      '中区分文字列置き換え
            '    strMessage = strMessage.Replace(Misc.CommonConst.WORD_LOC_S_CD, setting.DATA6)      '小区分文字列置き換え
            'End If
            '--2021/10/04 sv suzuki del end

            'メッセージを返却
            Return strMessage

        End Function

        ''' <summary>
        ''' 設定情報を返します。
        ''' キーがnullの場合、ブランクを返します。
        ''' </summary>
        ''' <param name="strKeyDiv">KEY種別</param>
        ''' <param name="strKey1">Key1</param>
        ''' <param name="strKey2">Key2</param>
        ''' <param name="strKey3">Key3</param>
        ''' <returns></returns>
        Public Function getSettingInfo(ByVal strKeyDiv As String, ByVal strKey1 As String, ByVal strKey2 As String, ByVal strKey3 As String, Optional isSeniErrPage As Boolean = False) As SettingInfo '2020/03/05 nv miyaji 設定マスタから取得できなかった場合、エラーページに飛ぶようにする
            'Public Function getSettingInfo(ByVal strKeyDiv As String, ByVal strKey1 As String, ByVal strKey2 As String, ByVal strKey3 As String) As SettingInfo '2020/03/05 nv miyaji 設定マスタから取得できなかった場合、エラーページに飛ぶようにする
            Dim strSettingInfo As SettingInfo = Nothing

            'キーが存在しない場合ブランクを返却
            If IsNothing(strKeyDiv) OrElse IsNothing(strKey1) OrElse IsNothing(strKey2) OrElse IsNothing(strKey3) Then
                Return strSettingInfo
            End If

            'キーに該当する要素を取得
            Dim itemData As IEnumerable(Of SettingInfo) = _SettingList.Where(Function(item) item.KeyDiv = strKeyDiv And item.Key1 = strKey1 And item.Key2 = strKey2 And item.Key3 = strKey3)
            If Not IsNothing(itemData) AndAlso itemData.Count > 0 Then
                strSettingInfo = itemData.ElementAt(0)
            End If

            '--2020/03/05 nv miyaji add start 設定マスタから取得できなかった場合、エラーページに飛ぶようにする
            If isSeniErrPage Then
                If strSettingInfo Is Nothing Then
                    Dim sErrStr As String = strKeyDiv
                    If strKey1 <> "**" Then
                        sErrStr = sErrStr & " " & strKey1
                    End If
                    If strKey2 <> "**" Then
                        sErrStr = sErrStr & " " & strKey2
                    End If
                    If strKey3 <> "**" Then
                        sErrStr = sErrStr & " " & strKey3
                    End If
                    Throw New Exception(String.Format(getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), sErrStr))
                End If
            End If
            '--2020/03/05 nv miyaji add end

            'メッセージを返却
            Return strSettingInfo

        End Function

        ''' <summary>
        ''' 設定情報を返します。(配列)
        ''' キーがnullの場合、ブランクを返します。
        ''' </summary>
        ''' <param name="strKeyDiv">KEY種別</param>
        ''' <returns></returns>
        Public Function getSettingList(ByVal strKeyDiv As String, Optional isSeniErrPage As Boolean = False) As SettingInfo()

            'キーが存在しない場合ブランクを返却
            If IsNothing(strKeyDiv) Then
                Return Nothing
            End If

            'キーに該当する要素を取得
            Dim itemData As IEnumerable(Of SettingInfo) = _SettingList.Where(Function(item) item.KeyDiv = strKeyDiv)
            If Not IsNothing(itemData) AndAlso itemData.Count > 0 Then
                '取得時
                Return itemData.ToArray
            End If

            If isSeniErrPage Then
                If itemData Is Nothing Then
                    Dim sErrStr As String = strKeyDiv
                    Throw New Exception(String.Format(getMessage(CommonLabel.MSG_ERR_NOT_EXIST_SETTING), sErrStr))
                End If
            End If

            Return Nothing

        End Function

        ''' <summary>
        ''' メニュー表示チェック
        ''' </summary>
        ''' <param name="sMenuId">メニューID</param>
        ''' <param name="clsLoginUser">ログインユーザークラス</param>
        ''' <returns>True:表示する  False:表示しない</returns>
        Public Function IsDspMenu(ByVal sMenuId As String, ByVal clsLoginUser As RfidEntity.LoginUser) As Boolean

            If sMenuId = Misc.Auth.MenuId.Gate Then
                'ゲート時 設定マスタ値をチェック
                Dim settingInfo As SettingInfo = getSettingInfo(CommonSettingKey.Gate_Setting.KeyDiv, CommonSettingKey.Gate_Setting.Key1, CommonSettingKey.Gate_Setting.Key2, CommonSettingKey.Gate_Setting.Key3)
                If settingInfo Is Nothing Then
                    Return False
                Else
                    Dim sValidFlg As String = settingInfo.DATA1.Trim
                    If sValidFlg <> ValidFlg.Cd.Valid Then
                        Return False
                    End If
                End If
            End If

            If sMenuId = Misc.Auth.MenuId.Construction Then
                '工事時 設定マスタ値をチェック
                Dim settingInfo As SettingInfo = getSettingInfo(CommonSettingKey.MatchingFunction.KeyDiv, CommonSettingKey.MatchingFunction.Key1, CommonSettingKey.MatchingFunction.Key2, CommonSettingKey.MatchingFunction.Key3)
                If settingInfo Is Nothing Then
                    Return False
                Else
                    Dim sValidFlg As String = settingInfo.DATA1.Trim
                    Dim sViewFlg As String = settingInfo.DATA2
                    If String.IsNullOrWhiteSpace(sViewFlg) Then
                        sViewFlg = ""
                    Else
                        sViewFlg = sViewFlg.Trim
                    End If
                    If sValidFlg <> ValidFlg.Cd.Valid OrElse sViewFlg <> ValidFlg.Cd.Valid Then
                        Return False
                    End If
                End If
            End If

            '--2021/09/27 nv miyaji add start
            'clsLoginUserがNothingの場合があるので、その場合Falseを返す。
            If clsLoginUser Is Nothing Then
                Return False
            End If
            '--2021/09/27 nv miyaji add end

            Dim sAuthY As String = Misc.AUTH_FLG.Yes

            'キーに該当する要素を取得
            Dim itemData As IEnumerable(Of AuthInfo) =
                _AuthList.Where(Function(item) item.MenuId = sMenuId And
                                                item.AuthDiv = clsLoginUser.KengenKbn And
                                                item.AuthType = clsLoginUser.AuthType And
                                                item.HeadOfficeFlg = clsLoginUser.HeadOfficeFlg And
                                                (item.ReadAuthFlg = sAuthY Or item.UpdAuthFlg = sAuthY Or item.ReadHubAuthFlg = sAuthY Or item.UpdHubAuthFlg = sAuthY Or Len(item.AuthOption) > 0))
            If IsNothing(itemData) OrElse itemData.Count = 0 Then
                Return False
            End If

            Return True

        End Function

        ''' <summary>
        ''' 機能メニュー表示チェック
        ''' </summary>
        ''' <param name="sFunctionId">機能ID</param>
        ''' <param name="clsLoginUser">ログインユーザークラス</param>
        ''' <returns>True:表示する  False:表示しない</returns>
        Public Function IsDspFunction(ByVal sFunctionId As String, ByVal clsLoginUser As RfidEntity.LoginUser) As Boolean

            If sFunctionId = Misc.Auth.FunctionId.LendingList Or sFunctionId = Misc.Auth.FunctionId.AssetLeaseHitory Then
                '予約系時 設定マスタ値をチェック
                Dim settingInfo As SettingInfo = getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3)
                If settingInfo Is Nothing Then
                    Return False
                Else
                    Dim sValidFlg As String = settingInfo.DATA1.Trim
                    If sValidFlg <> ValidFlg.Cd.Valid Then
                        Return False
                    End If
                End If
                '--2021/11/09 sv suzuki add start
            ElseIf sFunctionId = Misc.Auth.FunctionId.LendingStatus Then
                '予約状況照会時 設定マスタ値をチェック
                Dim settingInfoReserve As SettingInfo = getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3)
                Dim settingInfoMatch As SettingInfo = getSettingInfo(CommonSettingKey.MatchingFunction.KeyDiv, CommonSettingKey.MatchingFunction.Key1, CommonSettingKey.MatchingFunction.Key2, CommonSettingKey.MatchingFunction.Key3)
                If settingInfoReserve Is Nothing OrElse settingInfoMatch Is Nothing Then
                    Return False
                Else
                    Dim sValidFlgReserve As String = settingInfoReserve.DATA1.Trim
                    Dim sValidFlgMatch As String = settingInfoMatch.DATA1.Trim
                    If sValidFlgReserve <> ValidFlg.Cd.Valid AndAlso sValidFlgMatch <> ValidFlg.Cd.Valid Then
                        Return False
                    End If
                End If
            ElseIf sFunctionId = Misc.Auth.FunctionId.AssetLeasePlaceList Or sFunctionId = Misc.Auth.FunctionId.AssetLeasePlaceDetail Then
                '貸出系時 設定マスタ値をチェック
                Dim settingInfo As SettingInfo = getSettingInfo(CommonSettingKey.MatchingFunction.KeyDiv, CommonSettingKey.MatchingFunction.Key1, CommonSettingKey.MatchingFunction.Key2, CommonSettingKey.MatchingFunction.Key3)
                If settingInfo Is Nothing Then
                    Return False
                Else
                    Dim sValidFlg As String = settingInfo.DATA1.Trim
                    If sValidFlg <> ValidFlg.Cd.Invalid Then
                        Return False
                    End If
                End If
            ElseIf sFunctionId = Misc.Auth.FunctionId.Goods Then
                '商品マスタ時 設定マスタ値をチェック
                Dim settingInfo As SettingInfo = getSettingInfo(CommonSettingKey.GoodsReflect.KeyDiv, CommonSettingKey.GoodsReflect.Key1, CommonSettingKey.GoodsReflect.Key2, CommonSettingKey.GoodsReflect.Key3)
                If settingInfo Is Nothing Then
                    Return False
                Else
                    Dim sValidFlg As String = settingInfo.DATA1.Trim
                    If sValidFlg <> ValidFlg.Cd.Valid Then
                        Return False
                    End If
                End If
                '--2021/11/09 sv suzuki add end

                '2022/08/31 Upd Start Kawamoto 
                '--2021/11/23 sv Inoue add start
                'ElseIf sFunctionId = Misc.Auth.FunctionId.Calender Then
                '    'カレンダーマスタ時 設定マスタ値をチェック
                '    Dim settingInfo As SettingInfo = getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3)
                '    If settingInfo Is Nothing
                '        Return False
                '    Else
                '        Dim sValidFlg As String = settingInfo.DATA1.Trim
                '        If sValidFlg <> ValidFlg.Cd.Valid
                '            Return False
                '        End If
                '    End If
                '--2021/11/23 sv Inoue add end
            ElseIf sFunctionId = Misc.Auth.FunctionId.Calender Then
                'カレンダーマスタ時 設定マスタ値をチェック
                Dim settingInfo As SettingInfo = getSettingInfo(CommonSettingKey.ReservationFunction.KeyDiv, CommonSettingKey.ReservationFunction.Key1, CommonSettingKey.ReservationFunction.Key2, CommonSettingKey.ReservationFunction.Key3)
                Dim settingInfoPc As SettingInfo = getSettingInfo(CommonSettingKey.Lease_Max_Days_PC.KeyDiv, CommonSettingKey.Lease_Max_Days_PC.Key1, CommonSettingKey.Lease_Max_Days_PC.Key2, CommonSettingKey.Lease_Max_Days_PC.Key3)
                If settingInfo Is Nothing OrElse settingInfoPc Is Nothing Then
                    Return False
                Else
                    Dim sValidFlg As String = settingInfo.DATA1.Trim
                    Dim sValidFlgPc1 As String = settingInfoPc.DATA1.Trim
                    Dim sValidFlgPc2 As String = settingInfoPc.DATA2.Trim
                    '予約機能がOFF または (貸出(PC機能)の最大貸出判定がOFF または 稼働日考慮が不要)の場合はメニュー非表示
                    If sValidFlg <> ValidFlg.Cd.Valid AndAlso (sValidFlgPc1 = 0 OrElse sValidFlgPc2 <> WorkDayFlg.Yes) Then
                        Return False
                    End If
                End If
                '2022/08/31 Upd End Kawamoto 
                '2023/03/22 Kawamoto ADD START
                '指定数量管理表出力、または管理簿出力の場合 
            ElseIf sFunctionId = Misc.Auth.FunctionId.DrugSpecList Or sFunctionId = Misc.Auth.FunctionId.DrugManageList Then
                '設定マスタ（薬品管理機能）の取得
                Dim DrugsFunc As SettingInfo = getSettingInfo(CommonSettingKey.Drugs_Function.KeyDiv,
                                                              CommonSettingKey.Drugs_Function.Key1,
                                                              CommonSettingKey.Drugs_Function.Key2,
                                                              CommonSettingKey.Drugs_Function.Key3, True)

                '薬品機能がOFFの場合、メニュー非表示
                If DrugsFunc.DATA1 = DrugsFunction.No Then
                    Return False
                End If
                '2023/03/22 Kawamoto ADD START
            ElseIf sFunctionId = Misc.Auth.FunctionId.TemporaryList Then
                '仮置き一覧時 設定マスタ値をチェック
                Dim settingInfo As SettingInfo = getSettingInfo(CommonSettingKey.TemporaryLeaveFunction.KeyDiv, CommonSettingKey.TemporaryLeaveFunction.Key1, CommonSettingKey.TemporaryLeaveFunction.Key2, CommonSettingKey.TemporaryLeaveFunction.Key3)
                If settingInfo Is Nothing Then
                    Return False
                Else
                    Dim sValidFlg As String = settingInfo.DATA1.Trim
                    If sValidFlg <> ValidFlg.Cd.Valid Then
                        Return False
                    End If
                End If
            ElseIf sFunctionId = Misc.Auth.FunctionId.AssetLabelPrint Then
                '資産ラベル発行の場合
                If clsLoginUser Is Nothing Then
                    'clsLoginUserがNothingの場合はFalse
                    Return False
                End If

                '更新可能拠点にRFID拠点が含まれていない場合はFalse
                Dim AuthInfo As AuthInfo = GetFunctionAuth(Misc.Auth.FunctionId.AssetLabelPrint, clsLoginUser)
                If Not Dao.ToolLocation.IsExistRFIDLoc(clsLoginUser, AuthInfo) Then
                    Return False
                End If
            End If

            '--2021/09/27 nv miyaji add start
            'clsLoginUserがNothingの場合があるので、その場合Falseを返す。
            If clsLoginUser Is Nothing Then
                Return False
            End If
            '--2021/09/27 nv miyaji add end

            Dim sAuthY As String = Misc.AUTH_FLG.Yes

            'キーに該当する要素を取得
            Dim itemData As IEnumerable(Of AuthInfo) =
                _AuthList.Where(Function(item) item.FunctionId = sFunctionId And
                                                item.AuthDiv = clsLoginUser.KengenKbn And
                                                item.AuthType = clsLoginUser.AuthType And
                                                item.HeadOfficeFlg = clsLoginUser.HeadOfficeFlg And
                                                (item.ReadAuthFlg = sAuthY Or item.UpdAuthFlg = sAuthY Or item.ReadHubAuthFlg = sAuthY Or item.UpdHubAuthFlg = sAuthY Or Len(item.AuthOption) > 0))
            If IsNothing(itemData) OrElse itemData.Count = 0 Then
                Return False
            End If

            Return True

        End Function

        ''' <summary>
        ''' 機能別の権限マスタ情報を取得する
        ''' </summary>
        ''' <param name="sFunctionId">機能ID</param>
        ''' <param name="clsLoginUser">ログインユーザークラス</param>
        ''' <returns>True:表示する  False:表示しない</returns>
        Public Function GetFunctionAuth(ByVal sFunctionId As String, ByVal clsLoginUser As RfidEntity.LoginUser) As AuthInfo

            '--2021/09/27 nv miyaji add start
            'clsLoginUserがNothingの場合があるので、その場合Nothingを返す。
            If clsLoginUser Is Nothing Then
                Return Nothing
            End If
            '--2021/09/27 nv miyaji add end

            'キーに該当する要素を取得
            Dim itemData As IEnumerable(Of AuthInfo) =
                _AuthList.Where(Function(item) item.FunctionId = sFunctionId And
                                                item.AuthDiv = clsLoginUser.KengenKbn And
                                                item.AuthType = clsLoginUser.AuthType And
                                                item.HeadOfficeFlg = clsLoginUser.HeadOfficeFlg)
            If IsNothing(itemData) OrElse itemData.Count = 0 Then
                Return Nothing
            End If

            Return itemData.ElementAt(0)

        End Function

        ''' <summary>
        ''' ログインユーザー表示名を取得します。
        ''' </summary>
        ''' <param name="clsLoginUser">ログインユーザークラス</param>
        ''' <remarks>
        ''' </remarks>
        Public Function GetUserDspNm(ByVal clsLoginUser As RfidEntity.LoginUser) As String
            Dim sDspNm As String = String.Empty

            Select Case clsLoginUser.ShosaiAuthDiv
                Case Misc.SHOSAI_AUTH_DIV.SuperAdmin
                    sDspNm = getMessage(CommonLabel.LABEL_KENGEN_SUPERADMIN)
                Case Misc.SHOSAI_AUTH_DIV.Tsujo_Admin
                    sDspNm = getMessage(CommonLabel.LABEL_KENGEN_ADMIN)
                Case Misc.SHOSAI_AUTH_DIV.Tsujo_Nomal
                    sDspNm = getMessage(CommonLabel.LABEL_KENGEN_NOMAL)
                Case Misc.SHOSAI_AUTH_DIV.Honbu_Admin
                    sDspNm = getMessage(CommonLabel.LABEL_HONBU_SEKI)
                Case Misc.SHOSAI_AUTH_DIV.Honbu_Nomal
                    sDspNm = getMessage(CommonLabel.LABEL_HONBU_TAN)
                Case Misc.SHOSAI_AUTH_DIV.Kyoten_Admin
                    sDspNm = getMessage(CommonLabel.LABEL_KYOTEN_SEKI)
                Case Misc.SHOSAI_AUTH_DIV.Kyoten_Nomal
                    sDspNm = getMessage(CommonLabel.LABEL_KYOTEN_TAN)
            End Select

            sDspNm &= " : " & clsLoginUser.EmployeeNm

            Return sDspNm
        End Function


        ''' <summary>
        ''' 設定情報を返します。
        ''' キーがnullの場合、ブランクを返します。
        ''' </summary>
        ''' <param name="List">設定情報</param>
        Private Sub CopySettingModelData(List As List(Of SettingModel))
            _SettingList = New List(Of SettingInfo)
            For Each row As SettingModel In List
                Dim newrow As SettingInfo = New SettingInfo
                newrow.KeyDiv = row.KEY_DIV
                newrow.Key1 = row.KEY1
                newrow.Key2 = row.KEY2
                newrow.Key3 = row.KEY3
                newrow.DATA1 = row.DATA1
                newrow.DATA2 = row.DATA2
                newrow.DATA3 = row.DATA3
                newrow.DATA4 = row.DATA4
                newrow.DATA5 = row.DATA5
                newrow.DATA6 = row.DATA6
                newrow.DATA7 = row.DATA7
                newrow.DATA8 = row.DATA8
                newrow.DATA9 = row.DATA9
                newrow.DATA10 = row.DATA10
                _SettingList.Add(newrow)
            Next

        End Sub

        ''' <summary>
        ''' 権限情報を返します。
        ''' キーがnullの場合、ブランクを返します。
        ''' </summary>
        ''' <param name="List">設定情報</param>
        Private Sub CopyAuthModelData(List As List(Of AuthModel))
            _AuthList = New List(Of AuthInfo)
            For Each row As AuthModel In List
                Dim newrow As AuthInfo = New AuthInfo
                newrow.MenuId = row.MENU_ID
                newrow.FunctionId = row.FUNCTION_ID
                newrow.AuthDiv = row.AUTH_DIV
                newrow.AuthType = row.AUTH_TYPE
                newrow.HeadOfficeFlg = row.HEAD_OFFICE_FLG
                newrow.ReadAuthFlg = row.READ_AUTH_FLG
                newrow.UpdAuthFlg = row.UPD_AUTH_FLG
                newrow.ReadHubAuthFlg = row.READ_HUB_AUTH_FLG
                newrow.UpdHubAuthFlg = row.UPD_HUB_AUTH_FLG
                newrow.AuthOption = row.AUTH_OPTION
                _AuthList.Add(newrow)
            Next

        End Sub

    End Class
End Namespace
