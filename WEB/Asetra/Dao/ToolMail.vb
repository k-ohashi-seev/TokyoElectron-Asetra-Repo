Imports Asetra.Misc
Imports System.IO
Imports System.Data.SqlClient
Imports Asetra.Models

Namespace Dao
    ''' <summary>
    ''' システム名    ：ＲＦＩＤ資産管理パッケージ
    ''' サブシステム名：
    ''' クラス名      ：メール関連ツール系 Data Access クラス
    ''' 作成日        ：2021/10/16      nv Inoue
    ''' 修正日        ：0000/00/00
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ToolMail
        Private Shared logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        '管理番号＝10桁
        'シリアル=20桁

        Private Const LEN_ASSET_NO As Integer = 10          '文字列長    資産番号
        Private Const LEN_Serial_NO As Integer = 20         '文字列長    シリアル番号
        Private Const LEN_ASSET_NM As Integer = 120         '文字列長    製品名

        Private Const DLT_LOOP As String = "{LOOP_START}"   'LOOPデリミタ
        Private Const CON_KEISHO As String = ""             '社員名の後の敬称文字列

#Region "変数宣言"
        ''' <summary>AxQRMAKERADLib</summary>
        Private _Message As MessageRfid
        Private _LoginInfo As RfidEntity.LoginUser    'ログイン情報

#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="loginInfo">LoginUser クラス</param>
        ''' <param name="messageRfid">MessageRfid クラス</param>
        ''' <remarks></remarks>
        Public Sub New(loginInfo As RfidEntity.LoginUser, messageRfid As MessageRfid)
            Me._LoginInfo = loginInfo
            Me._Message = messageRfid
        End Sub
#End Region

        '--- Public 関数

#Region "メール本文マスタデータ取得"

        ''' <summary>
        ''' メール本文マスタデータ取得データ取得
        ''' </summary>
        ''' <param name="iMailDiv">メール区分</param>
        ''' <returns>
        ''' DataTable
        ''' </returns>
        Public Function GetMailTextData(ByVal iMailDiv As Integer) As DataTable
            Dim tbl As New DataTable

            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT MMAIL.*")
                sbSQL.AppendLine("FROM M_MAIL_TEXT MMAIL")
                sbSQL.AppendLine("WHERE MMAIL.MAIL_DIV = @MailDiv ")
                params.Add(New SqlParameter($"@MailDiv", iMailDiv))

                tbl = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try

            Return tbl

        End Function

#End Region

#Region "出荷通知メールデータ作成"

        ''' <summary>
        ''' 出荷通知メールデータ作成
        ''' </summary>
        ''' <param name="db">SystemDBContext</param>
        ''' <param name="modelLendingEntry">予約 model</param>
        ''' <param name="lstModelReserveDetail">予約詳細 model リスト</param>
        Public Sub MakeMailShukkaTsuchi(ByVal db As SystemDBContext,
                                              ByVal modelLendingEntry As LendingEntryModel,
                                              ByVal lstModelReserveDetail As List(Of ReserveDetailModel)
                                              )
            '======= メール内容 start
            '※このメールはシステムから送信しておりますので返信しないで下さい。

            '以下のデモ機の出荷が完了しました。

            '-----------------------
            '予約者：{1}
            '貸出期間：{2}
            '送り状No：{3}

            '管理番号        シリアル番号			製品名
            '{4}
            '-----------------------

            'URL
            '{5}
            '======= メール内容 end

            Dim sToMailAddr As String = String.Empty
            Dim sCcMailAddr As String = String.Empty

            'メール本文データ取得
            Dim tblMailText As DataTable = GetMailTextData(Misc.MAIL_DIV.Shukka)

            '管理ロケデータ取得
            Dim tblMngLoc As DataTable = ToolLocation.GetLargeLocationData(modelLendingEntry.MNG_LOC_L_CD)

            '社員データ取得
            Dim tblEmployee As DataTable = ToolEmployee.GetEmployeeData(modelLendingEntry.CHARGE_EMP_CD)

            If String.IsNullOrEmpty(tblEmployee.Rows(0).Item("MAIL_ADDRESS").ToString) Then
                If String.IsNullOrEmpty(tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString) Then
                    Return
                Else
                    sToMailAddr = tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString
                End If
            Else
                sToMailAddr = tblEmployee.Rows(0).Item("MAIL_ADDRESS").ToString
                If Not String.IsNullOrEmpty(tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString) Then
                    sCcMailAddr = tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString
                End If
            End If

            Dim sMailBody As String = String.Empty

            Dim sMailWk() As String = Split(tblMailText.Rows(0).Item("MAIL_BODY").ToString, "{4}")

            sMailBody = sMailWk(0)

            sMailBody = sMailBody.Replace("{1}", tblEmployee.Rows(0).Item("EMPLOYEE_NAME").ToString & CON_KEISHO)
            Dim sKikan As String = MailDateStr(modelLendingEntry.REAL_START_DT) & "～" & MailDateStr(modelLendingEntry.REAL_END_DT)
            sMailBody = sMailBody.Replace("{2}", sKikan)
            sMailBody = sMailBody.Replace("{3}", modelLendingEntry.DEST_NO)

            '資産LOOP部
            Dim sLoopStr As String = String.Empty
            For Each item As ReserveDetailModel In lstModelReserveDetail
                If Not String.IsNullOrEmpty(sLoopStr) Then
                    sLoopStr &= vbCrLf
                End If

                '資産データ取得
                Dim tblAsset As DataTable = ToolAsset.GetMstAsset(item.ASSET_NO)

                Dim sAsetStr As String = MailAssetStr(tblAsset.Rows(0).Item("ASSET_NO").ToString, LEN_ASSET_NO) & " " &
                                         MailAssetStr(tblAsset.Rows(0).Item("FREE_ITEM10").ToString, LEN_Serial_NO) & " " &
                                         MailAssetStr(tblAsset.Rows(0).Item("ASSET_NAME").ToString, LEN_ASSET_NM)
                sLoopStr &= sAsetStr.Trim
            Next
            sMailBody &= sLoopStr

            Dim mailUrlSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Mail_ASETRA_URL.KeyDiv, CommonSettingKey.Lending_Status_Setting.Key1, CommonSettingKey.Lending_Status_Setting.Key2, CommonSettingKey.Lending_Status_Setting.Key3, True)
            sMailBody &= sMailWk(1).Replace("{5}", mailUrlSetting.DATA1)

            'モデル作成（メールT）
            Dim modelMail As New MailModel
            modelMail.MAIL_ID = 0           '自動採番
            modelMail.MAIL_DIV = Misc.MAIL_DIV.Shukka
            modelMail.MAIL_SUBJECT = tblMailText.Rows(0).Item("MAIL_SUBJECT").ToString
            modelMail.MAIL_BODY = sMailBody
            modelMail.MAIL_STATUS = Misc.MAIL_STATUS.Mi
            modelMail.MAIL_RETRY_CNT = 0
            modelMail.INS_DATETIME = CommonRfid.getNowDate
            modelMail.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            modelMail.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            modelMail.UPD_DATETIME = CommonRfid.getNowDate
            modelMail.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            modelMail.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            modelMail.UPD_TASK = Misc.MAIL_DIV_NM.Shukka

            db.Mail.Add(modelMail)
            db.SaveChanges()

            'モデル作成（メール宛先T）
            Dim modelMailTo As MailToModel
            Dim aryMailAdddrs As String()

            aryMailAdddrs = sToMailAddr.Split(","c)
            For Each sMailad As String In aryMailAdddrs
                modelMailTo = New MailToModel

                modelMailTo.MAIL_ID = modelMail.MAIL_ID
                modelMailTo.MAIL_SEND_DIV = Misc.MAIL_SEND_DIV.MailTO
                modelMailTo.MAIL_ADDRESS = sMailad.Trim
                modelMailTo.INS_DATETIME = CommonRfid.getNowDate
                modelMailTo.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                modelMailTo.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                modelMailTo.UPD_DATETIME = CommonRfid.getNowDate
                modelMailTo.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                modelMailTo.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                modelMailTo.UPD_TASK = Misc.MAIL_DIV_NM.Shukka

                db.MailTo.Add(modelMailTo)
            Next

            If Not String.IsNullOrEmpty(sCcMailAddr) Then

                aryMailAdddrs = sCcMailAddr.Split(","c)
                For Each sMailad As String In aryMailAdddrs
                    modelMailTo = New MailToModel

                    modelMailTo.MAIL_ID = modelMail.MAIL_ID
                    modelMailTo.MAIL_SEND_DIV = Misc.MAIL_SEND_DIV.CC
                    modelMailTo.MAIL_ADDRESS = sMailad.Trim
                    modelMailTo.INS_DATETIME = CommonRfid.getNowDate
                    modelMailTo.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                    modelMailTo.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                    modelMailTo.UPD_DATETIME = CommonRfid.getNowDate
                    modelMailTo.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                    modelMailTo.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                    modelMailTo.UPD_TASK = Misc.MAIL_DIV_NM.Shukka

                    db.MailTo.Add(modelMailTo)
                Next

            End If
            db.SaveChanges()

        End Sub

#End Region

#Region "直接引継通知メールデータ作成"

        ' 管理部署単位のメール送信ロジック　　一応、変更の可能性を考え、残しておく　　2021/12/15
        '''' <summary>
        '''' 直接引継通知メールデータ作成
        '''' </summary>
        '''' <param name="db">SystemDBContext</param>
        '''' <param name="modelLendingEntry">予約 model</param>
        '''' <param name="lstModelReserveDetail">予約詳細 model リスト</param>
        '''' <param name="lstReserveDetailOld">変更前予約詳細 model リスト</param>
        'Public Sub MakeMailHikitsugi(ByVal db As SystemDBContext,
        '                                      ByVal modelLendingEntry As LendingEntryModel,
        '                                      ByVal lstModelReserveDetail As List(Of ReserveDetailModel),
        '                                      ByVal lstReserveDetailOld As List(Of ReserveDetailViewModel)
        '                                      )
        '    '======= メール内容 start
        '    '※このメールはシステムから送信しておりますので返信しないで下さい。
        '    '
        '    '以下のデモ機の所有者が変更されました。
        '    '{LOOP_START}
        '    '－－－－－－－－－－－－－－－－－－－
        '    '{1}
        '    '{2}
        '    '↓
        '    '{3}
        '    '{4}
        '    '
        '    '【申請内容】
        '    '貸出拠点：{5}
        '    '予約番号：{6}
        '    '貸出先　：{7}
        '    '貸出期間：{8}
        '    '
        '    '管理番号		シリアル番号			製品名
        '    '{9}
        '    '－－－－－－－－－－－－－－－－－－－
        '    '
        '    'URL
        '    '{10}
        '    '======= メール内容 end
        '    Dim lstModelSelDetail As ReserveDetailModel() = (From model In lstModelReserveDetail
        '                                                     Where (model.HANDOVER_DIV = Misc.HandOverDiv.HikitsugiMoto)
        '                                                     Order By model.ASSET_NO).ToArray

        '    'ありえないが、一応、チェック
        '    If lstModelSelDetail.Count = 0 Then
        '        Return
        '    End If

        '    '管理ロケデータ取得
        '    Dim tblMngLoc As DataTable = ToolLocation.GetLargeLocationData(modelLendingEntry.MNG_LOC_L_CD)
        '    Dim sMngLocNm As String = tblMngLoc.Rows(0).Item("LOC_NAME").ToString

        '    'メールアドレス設定
        '    Dim sToMailAddr As String = String.Empty
        '    Dim sCcMailAddr As String = String.Empty

        '    If String.IsNullOrEmpty(tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString) Then
        '        Return
        '    End If
        '    sToMailAddr = tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString


        '    '最初にソートのため、以下のdatatable を作成する
        '    Dim tblHiki As New DataTable
        '    'Dim tblSaki As DataTable = GetReserveDtl(0, "")        '引継ぎ先情報を行なしのDatatable を作成する

        '    tblHiki.Columns.Add("ASSET_NO", GetType(String))
        '    tblHiki.Columns.Add("OLD_CHARGE_EMP_CD", GetType(String))
        '    tblHiki.Columns.Add("RESERVE_ID", GetType(Long))
        '    tblHiki.Columns.Add("NEW_CHARGE_EMP_CD", GetType(String))
        '    tblHiki.Columns.Add("LEASE_PLACE_NAME", GetType(String))
        '    tblHiki.Columns.Add("REAL_START_DT", GetType(Integer))
        '    tblHiki.Columns.Add("REAL_END_DT", GetType(Integer))
        '    For Each item As ReserveDetailModel In lstModelSelDetail

        '        Dim lstModelOldDetail As ReserveDetailViewModel() = (From model In lstReserveDetailOld
        '                                                             Where (model.RESERVE_ID = item.RESERVE_ID And
        '                                                                    model.ASSET_NO = item.ASSET_NO And
        '                                                                    model.REAL_START_DT = item.REAL_START_DT
        '                                                                   )
        '                                                             ).ToArray

        '        If (Not lstModelOldDetail(0).HANDOVER_RESERVE_ID.HasValue) OrElse lstModelOldDetail(0).HANDOVER_RESERVE_ID <> item.HANDOVER_RESERVE_ID Then
        '            Dim newrow As DataRow = tblHiki.NewRow

        '            newrow("ASSET_NO") = item.ASSET_NO
        '            newrow("OLD_CHARGE_EMP_CD") = item.CHARGE_EMP_CD
        '            'Dim tblTmp As DataTable = GetReserveDtl(item.HANDOVER_RESERVE_ID, item.ASSET_NO)

        '            Dim modelLendingNew As LendingEntryModel
        '            modelLendingNew = db.LendingEntry.Find(item.HANDOVER_RESERVE_ID)
        '            Dim modelLeasePlace As LeasePlaceModel
        '            modelLeasePlace = db.LeasePlace.Find(modelLendingNew.LEASE_PLACE_CD)
        '            Dim modeldtlNew As ReserveDetailModel
        '            modeldtlNew = db.ReserveDetail.Find(item.HANDOVER_RESERVE_ID, item.ASSET_NO, modelLendingNew.REAL_START_DT)

        '            newrow("RESERVE_ID") = modelLendingNew.RESERVE_ID
        '            newrow("NEW_CHARGE_EMP_CD") = modelLendingNew.CHARGE_EMP_CD
        '            newrow("LEASE_PLACE_NAME") = modelLeasePlace.LEASE_PLACE_NAME
        '            newrow("REAL_START_DT") = modeldtlNew.REAL_START_DT
        '            newrow("REAL_END_DT") = modeldtlNew.REAL_END_DT
        '            tblHiki.Rows.Add(newrow)

        '            'tblSaki.ImportRow(tblTmp.Rows(0))           '先頭行をコピー
        '        End If
        '    Next

        '    'メール本文データ取得
        '    Dim tblMailText As DataTable = GetMailTextData(Misc.MAIL_DIV.Yokomochi)
        '    Dim sMailBody As String = String.Empty

        '    Dim sMailWk() As String = Split(tblMailText.Rows(0).Item("MAIL_BODY").ToString, "{9}")
        '    Dim sMailWk2() As String = Split(sMailWk(0).ToString, DLT_LOOP)

        '    sMailBody = sMailWk2(0)

        '    '変更後担当者LOOP部
        '    Dim sLoopStr As String = String.Empty
        '    Dim sNewEmpCd As String = String.Empty
        '    Dim isCR As Boolean = False
        '    For Each row As DataRow In tblHiki.Select(String.Empty, ("NEW_CHARGE_EMP_CD, ASSET_NO"))
        '        If sNewEmpCd <> row("NEW_CHARGE_EMP_CD").ToString Then

        '            Dim sMailMidHed As String = sMailWk2(1)

        '            '前回社員データ取得
        '            Dim tblExEmployee As DataTable = ToolEmployee.GetEmployeeDepartmentNm(row("OLD_CHARGE_EMP_CD").ToString)
        '            If tblExEmployee.Rows.Count = 0 Then
        '                Return
        '            End If
        '            '引継ぎ先社員データ取得
        '            Dim tblNewEmployee As DataTable = ToolEmployee.GetEmployeeDepartmentNm(row("NEW_CHARGE_EMP_CD").ToString)
        '            If tblNewEmployee.Rows.Count = 0 Then
        '                Return
        '            End If

        '            '引継ぎ先の予約明細情報
        '            'Dim rowNewDtl As DataRow = tblSaki.Select("ASSET_NO = '" & row("ASSET_NO").ToString & "'")(0)

        '            sMailMidHed = sMailMidHed.Replace("{1}", tblExEmployee.Rows(0).Item("DEPARTMENT_NAME").ToString)
        '            sMailMidHed = sMailMidHed.Replace("{2}", tblExEmployee.Rows(0).Item("EMPLOYEE_NAME").ToString & CON_KEISHO)
        '            sMailMidHed = sMailMidHed.Replace("{3}", tblNewEmployee.Rows(0).Item("DEPARTMENT_NAME").ToString)
        '            sMailMidHed = sMailMidHed.Replace("{4}", tblNewEmployee.Rows(0).Item("EMPLOYEE_NAME").ToString & CON_KEISHO)

        '            sMailMidHed = sMailMidHed.Replace("{5}", sMngLocNm)
        '            sMailMidHed = sMailMidHed.Replace("{6}", row("RESERVE_ID").ToString)
        '            sMailMidHed = sMailMidHed.Replace("{7}", row("LEASE_PLACE_NAME").ToString)
        '            Dim sDltKikan As String = MailDateStr(CInt(row("REAL_START_DT"))) & "～" & MailDateStr(CInt(row("REAL_END_DT")))
        '            sMailMidHed = sMailMidHed.Replace("{8}", sDltKikan)

        '            sLoopStr &= sMailMidHed

        '            sNewEmpCd = row("NEW_CHARGE_EMP_CD").ToString
        '            isCR = False
        '        End If

        '        If isCR Then
        '            sLoopStr &= vbCrLf
        '        End If

        '        '資産データ取得
        '        Dim tblAsset As DataTable = ToolAsset.GetMstAsset(row("ASSET_NO").ToString)
        '        Dim sAsetStr As String = MailAssetStr(tblAsset.Rows(0).Item("ASSET_NO").ToString, LEN_ASSET_NO) & " " &
        '                                     MailAssetStr(tblAsset.Rows(0).Item("FREE_ITEM10").ToString, LEN_Serial_NO) & " " &
        '                                     MailAssetStr(tblAsset.Rows(0).Item("ASSET_NAME").ToString, LEN_ASSET_NM)
        '        sLoopStr &= sAsetStr.Trim

        '        isCR = True

        '    Next
        '    sMailBody &= sLoopStr

        '    Dim mailUrlSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Mail_ASETRA_URL.KeyDiv, CommonSettingKey.Lending_Status_Setting.Key1, CommonSettingKey.Lending_Status_Setting.Key2, CommonSettingKey.Lending_Status_Setting.Key3, True)
        '    sMailBody &= sMailWk(1).Replace("{10}", mailUrlSetting.DATA1)

        '    'モデル作成（メールT）
        '    Dim modelMail As New MailModel
        '    modelMail.MAIL_DIV = Misc.MAIL_DIV.Yokomochi
        '    modelMail.MAIL_SUBJECT = tblMailText.Rows(0).Item("MAIL_SUBJECT").ToString
        '    modelMail.MAIL_BODY = sMailBody
        '    modelMail.MAIL_STATUS = Misc.MAIL_STATUS.Mi
        '    modelMail.MAIL_RETRY_CNT = 0
        '    modelMail.INS_DATETIME = CommonRfid.getNowDate
        '    modelMail.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
        '    modelMail.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
        '    modelMail.UPD_DATETIME = CommonRfid.getNowDate
        '    modelMail.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
        '    modelMail.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
        '    modelMail.UPD_TASK = Misc.MAIL_DIV_NM.Yokomochi

        '    'メールテーブルの更新
        '    db.Mail.Add(modelMail)
        '    db.SaveChanges()

        '    'モデル作成（メール宛先T）
        '    Dim modelMailTo As MailToModel
        '    Dim aryMailAdddrs As String()

        '    aryMailAdddrs = sToMailAddr.Split(","c)
        '    For Each sMailad As String In aryMailAdddrs
        '        modelMailTo = New MailToModel

        '        modelMailTo.MAIL_ID = modelMail.MAIL_ID
        '        modelMailTo.MAIL_SEND_DIV = Misc.MAIL_SEND_DIV.MailTO
        '        modelMailTo.MAIL_ADDRESS = sMailad.Trim
        '        modelMailTo.INS_DATETIME = CommonRfid.getNowDate
        '        modelMailTo.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
        '        modelMailTo.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
        '        modelMailTo.UPD_DATETIME = CommonRfid.getNowDate
        '        modelMailTo.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
        '        modelMailTo.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
        '        modelMailTo.UPD_TASK = Misc.MAIL_DIV_NM.Yokomochi

        '        db.MailTo.Add(modelMailTo)
        '    Next

        '    If Not String.IsNullOrEmpty(sCcMailAddr) Then

        '        aryMailAdddrs = sToMailAddr.Split(","c)
        '        For Each sMailad As String In aryMailAdddrs
        '            modelMailTo = New MailToModel

        '            modelMailTo.MAIL_ID = modelMail.MAIL_ID
        '            modelMailTo.MAIL_SEND_DIV = Misc.MAIL_SEND_DIV.CC
        '            modelMailTo.MAIL_ADDRESS = sMailad.Trim
        '            modelMailTo.INS_DATETIME = CommonRfid.getNowDate
        '            modelMailTo.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
        '            modelMailTo.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
        '            modelMailTo.UPD_DATETIME = CommonRfid.getNowDate
        '            modelMailTo.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
        '            modelMailTo.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
        '            modelMailTo.UPD_TASK = Misc.MAIL_DIV_NM.Yokomochi

        '            db.MailTo.Add(modelMailTo)
        '        Next

        '    End If

        '    db.SaveChanges()

        'End Sub

        ''' <summary>
        ''' 直接引継通知メールデータ作成
        ''' </summary>
        ''' <param name="db">SystemDBContext</param>
        ''' <param name="modelLendingEntry">予約 model</param>
        ''' <param name="lstModelReserveDetail">予約詳細 model リスト</param>
        ''' <param name="lstReserveDetailOld">変更前予約詳細 model リスト</param>
        Public Sub MakeMailHikitsugi(ByVal db As SystemDBContext,
                                              ByVal modelLendingEntry As LendingEntryModel,
                                              ByVal lstModelReserveDetail As List(Of ReserveDetailModel),
                                              ByVal lstReserveDetailOld As List(Of ReserveDetailViewModel)
                                              )
            '======= メール内容 start
            '※このメールはシステムから送信しておりますので返信しないで下さい。
            '
            '以下のデモ機の所有者が変更されました。
            '
            '－－－－－－－－－－－－－－－－－－－
            '{1}
            '{2}
            '↓
            '{3}
            '{4}
            '
            '【申請内容】
            '貸出拠点：{5}
            '予約番号：{6}
            '貸出先　：{7}
            '貸出期間：{8}
            '
            '管理番号		シリアル番号			製品名
            '{9}
            '－－－－－－－－－－－－－－－－－－－
            '
            'URL
            '{10}
            '======= メール内容 end
            Dim lstModelSelDetail As ReserveDetailModel() = (From model In lstModelReserveDetail
                                                             Where (model.HANDOVER_DIV = Misc.HandOverDiv.HikitsugiMoto)
                                                             Order By model.ASSET_NO).ToArray

            'ありえないが、一応、チェック
            If lstModelSelDetail.Count = 0 Then
                Return
            End If

            '管理ロケデータ取得
            Dim tblMngLoc As DataTable = ToolLocation.GetLargeLocationData(modelLendingEntry.MNG_LOC_L_CD)
            Dim sMngLocNm As String = tblMngLoc.Rows(0).Item("LOC_NAME").ToString

            '最初にソートのため、以下のdatatable を作成する
            Dim tblHiki As New DataTable
            'Dim tblSaki As DataTable = GetReserveDtl(0, "")        '引継ぎ先情報を行なしのDatatable を作成する

            tblHiki.Columns.Add("ASSET_NO", GetType(String))
            tblHiki.Columns.Add("OLD_CHARGE_EMP_CD", GetType(String))
            tblHiki.Columns.Add("RESERVE_ID", GetType(Long))
            tblHiki.Columns.Add("NEW_CHARGE_EMP_CD", GetType(String))
            tblHiki.Columns.Add("LEASE_PLACE_NAME", GetType(String))
            tblHiki.Columns.Add("REAL_START_DT", GetType(Integer))
            tblHiki.Columns.Add("REAL_END_DT", GetType(Integer))
            For Each item As ReserveDetailModel In lstModelSelDetail

                Dim lstModelOldDetail As ReserveDetailViewModel() = (From model In lstReserveDetailOld
                                                                     Where (model.RESERVE_ID = item.RESERVE_ID And
                                                                            model.ASSET_NO = item.ASSET_NO And
                                                                            model.REAL_START_DT = item.REAL_START_DT
                                                                           )
                                                                     ).ToArray

                If (Not lstModelOldDetail(0).HANDOVER_RESERVE_ID.HasValue) OrElse lstModelOldDetail(0).HANDOVER_RESERVE_ID <> item.HANDOVER_RESERVE_ID Then
                    Dim newrow As DataRow = tblHiki.NewRow

                    newrow("ASSET_NO") = item.ASSET_NO
                    newrow("OLD_CHARGE_EMP_CD") = item.CHARGE_EMP_CD
                    'Dim tblTmp As DataTable = GetReserveDtl(item.HANDOVER_RESERVE_ID, item.ASSET_NO)

                    Dim modelLendingNew As LendingEntryModel
                    modelLendingNew = db.LendingEntry.Find(item.HANDOVER_RESERVE_ID)
                    Dim modelLeasePlace As LeasePlaceModel
                    modelLeasePlace = db.LeasePlace.Find(modelLendingNew.LEASE_PLACE_CD)
                    Dim modeldtlNew As ReserveDetailModel
                    modeldtlNew = db.ReserveDetail.Find(item.HANDOVER_RESERVE_ID, item.ASSET_NO, modelLendingNew.REAL_START_DT)

                    newrow("RESERVE_ID") = modelLendingNew.RESERVE_ID
                    newrow("NEW_CHARGE_EMP_CD") = modelLendingNew.CHARGE_EMP_CD
                    newrow("LEASE_PLACE_NAME") = modelLeasePlace.LEASE_PLACE_NAME
                    newrow("REAL_START_DT") = modeldtlNew.REAL_START_DT
                    newrow("REAL_END_DT") = modeldtlNew.REAL_END_DT
                    tblHiki.Rows.Add(newrow)

                    'tblSaki.ImportRow(tblTmp.Rows(0))           '先頭行をコピー
                End If
            Next

            'メール本文データ取得
            Dim tblMailText As DataTable = GetMailTextData(Misc.MAIL_DIV.Yokomochi)
            Dim sMailBody As String = String.Empty

            Dim sMailWk() As String = Split(tblMailText.Rows(0).Item("MAIL_BODY").ToString, "{9}")

            '変更後担当者LOOP部
            Dim sLoopStr As String = String.Empty
            Dim sNewEmpCd As String = String.Empty
            Dim isCR As Boolean = False
            Dim sMailMidHed As String
            Dim datarows As DataRow() = tblHiki.Select(String.Empty, ("NEW_CHARGE_EMP_CD, ASSET_NO"))
            Dim row As DataRow = Nothing
            For ii As Integer = 0 To datarows.Count

                If ii < datarows.Count Then
                    row = datarows(ii)
                End If

                If sNewEmpCd <> row("NEW_CHARGE_EMP_CD").ToString OrElse ii = datarows.Count Then

                    If Not String.IsNullOrEmpty(sLoopStr) Then

                        sMailBody = sLoopStr

                        Dim mailUrlSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Mail_ASETRA_URL.KeyDiv, CommonSettingKey.Lending_Status_Setting.Key1, CommonSettingKey.Lending_Status_Setting.Key2, CommonSettingKey.Lending_Status_Setting.Key3, True)
                        sMailBody &= sMailWk(1).Replace("{10}", mailUrlSetting.DATA1)

                        '社員データ取得
                        Dim tblEmployee As DataTable = ToolEmployee.GetEmployeeData(sNewEmpCd)

                        'メールアドレス設定
                        Dim sToMailAddr As String = String.Empty
                        Dim sCcMailAddr As String = String.Empty
                        Dim isSendMail As Boolean = True

                        If String.IsNullOrEmpty(tblEmployee.Rows(0).Item("MAIL_ADDRESS").ToString) Then
                            If String.IsNullOrEmpty(tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString) Then
                                isSendMail = False
                            Else
                                sToMailAddr = tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString
                            End If
                        Else
                            sToMailAddr = tblEmployee.Rows(0).Item("MAIL_ADDRESS").ToString
                            If Not String.IsNullOrEmpty(tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString) Then
                                sCcMailAddr = tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString
                            End If
                        End If

                        If isSendMail Then
                            'モデル作成（メールT）
                            Dim modelMail As New MailModel
                            modelMail.MAIL_DIV = Misc.MAIL_DIV.Yokomochi
                            modelMail.MAIL_SUBJECT = tblMailText.Rows(0).Item("MAIL_SUBJECT").ToString
                            modelMail.MAIL_BODY = sMailBody
                            modelMail.MAIL_STATUS = Misc.MAIL_STATUS.Mi
                            modelMail.MAIL_RETRY_CNT = 0
                            modelMail.INS_DATETIME = CommonRfid.getNowDate
                            modelMail.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                            modelMail.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                            modelMail.UPD_DATETIME = CommonRfid.getNowDate
                            modelMail.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                            modelMail.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                            modelMail.UPD_TASK = Misc.MAIL_DIV_NM.Yokomochi

                            'メールテーブルの更新
                            db.Mail.Add(modelMail)
                            db.SaveChanges()

                            'モデル作成（メール宛先T）
                            Dim modelMailTo As MailToModel
                            Dim aryMailAdddrs As String()

                            aryMailAdddrs = sToMailAddr.Split(","c)
                            For Each sMailad As String In aryMailAdddrs
                                modelMailTo = New MailToModel

                                modelMailTo.MAIL_ID = modelMail.MAIL_ID
                                modelMailTo.MAIL_SEND_DIV = Misc.MAIL_SEND_DIV.MailTO
                                modelMailTo.MAIL_ADDRESS = sMailad.Trim
                                modelMailTo.INS_DATETIME = CommonRfid.getNowDate
                                modelMailTo.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                                modelMailTo.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                                modelMailTo.UPD_DATETIME = CommonRfid.getNowDate
                                modelMailTo.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                                modelMailTo.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                                modelMailTo.UPD_TASK = Misc.MAIL_DIV_NM.Yokomochi

                                db.MailTo.Add(modelMailTo)
                            Next

                            If Not String.IsNullOrEmpty(sCcMailAddr) Then

                                aryMailAdddrs = sCcMailAddr.Split(","c)
                                For Each sMailad As String In aryMailAdddrs
                                    modelMailTo = New MailToModel

                                    modelMailTo.MAIL_ID = modelMail.MAIL_ID
                                    modelMailTo.MAIL_SEND_DIV = Misc.MAIL_SEND_DIV.CC
                                    modelMailTo.MAIL_ADDRESS = sMailad.Trim
                                    modelMailTo.INS_DATETIME = CommonRfid.getNowDate
                                    modelMailTo.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                                    modelMailTo.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                                    modelMailTo.UPD_DATETIME = CommonRfid.getNowDate
                                    modelMailTo.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                                    modelMailTo.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                                    modelMailTo.UPD_TASK = Misc.MAIL_DIV_NM.Yokomochi

                                    db.MailTo.Add(modelMailTo)
                                Next

                            End If
                        End If
                        If ii = datarows.Count Then
                            'データ数と同じ場合、抜ける
                            Exit For
                        End If
                    End If

                    sLoopStr = String.Empty
                    sMailMidHed = sMailWk(0)

                    '前回社員データ取得
                    Dim tblExEmployee As DataTable = ToolEmployee.GetEmployeeDepartmentNm(row("OLD_CHARGE_EMP_CD").ToString)
                    If tblExEmployee.Rows.Count = 0 Then
                        Return
                    End If
                    '引継ぎ先社員データ取得
                    Dim tblNewEmployee As DataTable = ToolEmployee.GetEmployeeDepartmentNm(row("NEW_CHARGE_EMP_CD").ToString)
                    If tblNewEmployee.Rows.Count = 0 Then
                        Return
                    End If

                    '引継ぎ先の予約明細情報
                    'Dim rowNewDtl As DataRow = tblSaki.Select("ASSET_NO = '" & row("ASSET_NO").ToString & "'")(0)

                    sMailMidHed = sMailMidHed.Replace("{1}", tblExEmployee.Rows(0).Item("DEPARTMENT_NAME").ToString)
                    sMailMidHed = sMailMidHed.Replace("{2}", tblExEmployee.Rows(0).Item("EMPLOYEE_NAME").ToString & CON_KEISHO)
                    sMailMidHed = sMailMidHed.Replace("{3}", tblNewEmployee.Rows(0).Item("DEPARTMENT_NAME").ToString)
                    sMailMidHed = sMailMidHed.Replace("{4}", tblNewEmployee.Rows(0).Item("EMPLOYEE_NAME").ToString & CON_KEISHO)

                    sMailMidHed = sMailMidHed.Replace("{5}", sMngLocNm)
                    sMailMidHed = sMailMidHed.Replace("{6}", row("RESERVE_ID").ToString)
                    sMailMidHed = sMailMidHed.Replace("{7}", row("LEASE_PLACE_NAME").ToString)
                    Dim sDltKikan As String = MailDateStr(CInt(row("REAL_START_DT"))) & "～" & MailDateStr(CInt(row("REAL_END_DT")))
                    sMailMidHed = sMailMidHed.Replace("{8}", sDltKikan)

                    sLoopStr &= sMailMidHed

                    sNewEmpCd = row("NEW_CHARGE_EMP_CD").ToString
                    isCR = False
                End If

                If isCR Then
                    sLoopStr &= vbCrLf
                Else
                    isCR = True
                End If

                '資産データ取得
                Dim tblAsset As DataTable = ToolAsset.GetMstAsset(row("ASSET_NO").ToString)
                Dim sAsetStr As String = MailAssetStr(tblAsset.Rows(0).Item("ASSET_NO").ToString, LEN_ASSET_NO) & " " &
                                                     MailAssetStr(tblAsset.Rows(0).Item("FREE_ITEM10").ToString, LEN_Serial_NO) & " " &
                                                     MailAssetStr(tblAsset.Rows(0).Item("ASSET_NAME").ToString, LEN_ASSET_NM)
                sLoopStr &= sAsetStr.Trim

            Next

            db.SaveChanges()

        End Sub

#End Region

#Region "延長通知メールデータ作成"

        ''' <summary>
        ''' 延長通知メールデータ作成
        ''' </summary>
        ''' <param name="db">SystemDBContext</param>
        ''' <param name="modelLendingEntry">予約 model</param>
        ''' <param name="lstModelReserveDetail">予約詳細 model リスト</param>
        ''' <param name="lstReserveDetailOld">変更前予約詳細 model リスト</param>
        Public Sub MakeMailEncho(ByVal db As SystemDBContext,
                                                  ByVal modelLendingEntry As LendingEntryModel,
                                                  ByVal lstModelReserveDetail As List(Of ReserveDetailModel),
                                                  ByVal lstReserveDetailOld As List(Of ReserveDetailViewModel)
                                                  )
            '======= メール内容 start
            '※このメールはシステムから送信しておりますので返信しないで下さい。
            '
            '以下のデモ機の貸出予約期間が延長されました。
            '
            '－－－－－－－－－－－－－－－－－－－
            '{1}
            '{2}
            '
            '【申請内容】
            '貸出拠点：{3}
            '予約番号：{4}
            '貸出先　：{5}
            '貸出期間(旧)：{6}
            '貸出期間(新)：{7}
            '
            '
            '管理番号		シリアル番号			製品名
            '{8}
            '－－－－－－－－－－－－－－－－－－－
            '
            'URL
            '{9}
            '======= メール内容 end

            Dim sToMailAddr As String = String.Empty
            Dim sCcMailAddr As String = String.Empty

            'メール本文データ取得
            Dim tblMailText As DataTable = GetMailTextData(Misc.MAIL_DIV.Encho)

            '管理ロケデータ取得
            Dim tblMngLoc As DataTable = ToolLocation.GetLargeLocationData(modelLendingEntry.MNG_LOC_L_CD)
            '社員データ取得
            Dim tblEmployee As DataTable = ToolEmployee.GetEmployeeDepartmentNm(modelLendingEntry.CHARGE_EMP_CD)

            'If String.IsNullOrEmpty(tblEmployee.Rows(0).Item("MAIL_ADDRESS").ToString) Then
            '    If String.IsNullOrEmpty(tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString) Then
            '        Return
            '    Else
            '        sToMailAddr = tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString
            '    End If
            'Else
            '    sToMailAddr = tblEmployee.Rows(0).Item("MAIL_ADDRESS").ToString
            '    If Not String.IsNullOrEmpty(tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString) Then
            '        sCcMailAddr = tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString
            '    End If
            'End If

            If String.IsNullOrEmpty(tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString) Then
                Return
            End If
            sToMailAddr = tblMngLoc.Rows(0).Item("MAIL_SEND_ADDRESS").ToString


            '管理ロケデータ取得
            Dim sMngLocNm As String = tblMngLoc.Rows(0).Item("LOC_NAME").ToString

            Dim lstModelSelDetail As ReserveDetailModel() = (From model In lstModelReserveDetail
                                                             Where (model.LEASE_CHG_TIMES > 0)
                                                             Order By model.REAL_END_DT, model.ASSET_NO).ToArray

            Dim sMailBody As String = String.Empty

            Dim sMailWk() As String = Split(tblMailText.Rows(0).Item("MAIL_BODY").ToString, "{8}")
            Dim sMailWk2() As String = Split(sMailWk(0).ToString, DLT_LOOP)

            sMailBody = sMailWk2(0)

            sMailBody = sMailBody.Replace("{1}", tblEmployee.Rows(0).Item("DEPARTMENT_NAME").ToString)
            sMailBody = sMailBody.Replace("{2}", tblEmployee.Rows(0).Item("EMPLOYEE_NAME").ToString & CON_KEISHO)

            '資産LOOP部
            Dim sLoopStr As String = String.Empty
            Dim iRealEndDt As Integer = -1
            Dim isCR As Boolean = False
            For Each item As ReserveDetailModel In lstModelSelDetail

                Dim lstDetailOld As ReserveDetailViewModel() = (From model In lstReserveDetailOld
                                                                Where (model.ASSET_NO = item.ASSET_NO And model.REAL_START_DT = item.REAL_START_DT)
                                                                Order By model.ASSET_NO).ToArray

                If lstDetailOld.Length > 0 AndAlso item.LEASE_CHG_TIMES > lstDetailOld(0).LEASE_CHG_TIMES Then
                    '期間延長変更回数が今回、upしている場合
                    If iRealEndDt <> item.REAL_END_DT Then
                        Dim sMailMidHed As String = sMailWk2(1)
                        sMailMidHed = sMailMidHed.Replace("{3}", sMngLocNm)
                        sMailMidHed = sMailMidHed.Replace("{4}", modelLendingEntry.RESERVE_ID)
                        sMailMidHed = sMailMidHed.Replace("{5}", modelLendingEntry.DEST_NAME1 & modelLendingEntry.DEST_NAME2)
                        Dim sOldKikan As String = MailDateStr(lstDetailOld(0).REAL_START_DT) & "～" & MailDateStr(lstDetailOld(0).REAL_END_DT)
                        sMailMidHed = sMailMidHed.Replace("{6}", sOldKikan)
                        Dim sNewKikan As String = MailDateStr(item.REAL_START_DT) & "～" & MailDateStr(item.REAL_END_DT)
                        sMailMidHed = sMailMidHed.Replace("{7}", sNewKikan)
                        sLoopStr &= sMailMidHed

                        iRealEndDt = item.REAL_END_DT
                        isCR = False
                    End If

                    If isCR Then
                        sLoopStr &= vbCrLf
                    End If

                    '資産データ取得
                    Dim tblAsset As DataTable = ToolAsset.GetMstAsset(item.ASSET_NO)

                    Dim sAsetStr As String = MailAssetStr(tblAsset.Rows(0).Item("ASSET_NO").ToString, LEN_ASSET_NO) & " " &
                                                 MailAssetStr(tblAsset.Rows(0).Item("FREE_ITEM10").ToString, LEN_Serial_NO) & " " &
                                                 MailAssetStr(tblAsset.Rows(0).Item("ASSET_NAME").ToString, LEN_ASSET_NM)
                    sLoopStr &= sAsetStr.Trim

                    isCR = True
                End If

            Next
            sMailBody &= sLoopStr

            Dim mailUrlSetting As SettingInfo = Me._Message.getSettingInfo(CommonSettingKey.Mail_ASETRA_URL.KeyDiv, CommonSettingKey.Lending_Status_Setting.Key1, CommonSettingKey.Lending_Status_Setting.Key2, CommonSettingKey.Lending_Status_Setting.Key3, True)
            sMailBody &= sMailWk(1).Replace("{9}", mailUrlSetting.DATA1)

            'モデル作成（メールT）
            Dim modelMail As New MailModel
            modelMail.MAIL_DIV = Misc.MAIL_DIV.Encho
            modelMail.MAIL_SUBJECT = tblMailText.Rows(0).Item("MAIL_SUBJECT").ToString
            modelMail.MAIL_BODY = sMailBody
            modelMail.MAIL_STATUS = Misc.MAIL_STATUS.Mi
            modelMail.MAIL_RETRY_CNT = 0
            modelMail.INS_DATETIME = CommonRfid.getNowDate
            modelMail.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            modelMail.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            modelMail.UPD_DATETIME = CommonRfid.getNowDate
            modelMail.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
            modelMail.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
            modelMail.UPD_TASK = Misc.MAIL_DIV_NM.Encho

            db.Mail.Add(modelMail)
            db.SaveChanges()

            'モデル作成（メール宛先T）
            Dim modelMailTo As MailToModel
            Dim aryMailAdddrs As String()

            aryMailAdddrs = sToMailAddr.Split(","c)
            For Each sMailad As String In aryMailAdddrs
                modelMailTo = New MailToModel

                modelMailTo.MAIL_ID = modelMail.MAIL_ID
                modelMailTo.MAIL_SEND_DIV = Misc.MAIL_SEND_DIV.MailTO
                modelMailTo.MAIL_ADDRESS = sMailad.Trim
                modelMailTo.INS_DATETIME = CommonRfid.getNowDate
                modelMailTo.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                modelMailTo.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                modelMailTo.UPD_DATETIME = CommonRfid.getNowDate
                modelMailTo.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                modelMailTo.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                modelMailTo.UPD_TASK = Misc.MAIL_DIV_NM.Encho

                db.MailTo.Add(modelMailTo)
            Next

            If Not String.IsNullOrEmpty(sCcMailAddr) Then

                aryMailAdddrs = sCcMailAddr.Split(","c)
                For Each sMailad As String In aryMailAdddrs
                    modelMailTo = New MailToModel

                    modelMailTo.MAIL_ID = modelMail.MAIL_ID
                    modelMailTo.MAIL_SEND_DIV = Misc.MAIL_SEND_DIV.CC
                    modelMailTo.MAIL_ADDRESS = sMailad.Trim
                    modelMailTo.INS_DATETIME = CommonRfid.getNowDate
                    modelMailTo.INS_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                    modelMailTo.INS_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                    modelMailTo.UPD_DATETIME = CommonRfid.getNowDate
                    modelMailTo.UPD_EMPLOYEE_CD = _LoginInfo.EmployeeCd
                    modelMailTo.UPD_EMPLOYEE_NAME = _LoginInfo.EmployeeNm
                    modelMailTo.UPD_TASK = Misc.MAIL_DIV_NM.Encho

                    db.MailTo.Add(modelMailTo)
                Next

            End If

            db.SaveChanges()

        End Sub

#End Region

#Region "管理ロケーション存在チェック"

        ''' <summary>
        ''' 管理ロケーション存在チェック
        ''' </summary>
        ''' <param name="sKeyDiv">KEY種別</param>
        ''' <param name="sMngLoc">管理ロケーション</param>
        ''' <returns>
        ''' True: データ作成  False:作成できない（送信しない）
        ''' </returns>
        Public Function ChkMngLoc(ByVal sKeyDiv As String, ByVal sMngLoc As String) As Boolean

            Dim settingArray As SettingInfo() = Me._Message.getSettingList(sKeyDiv, False)
            If IsNothing(settingArray) Then
                Return False
            End If

            For Each setting As SettingInfo In settingArray
                If setting.DATA1 = sMngLoc Then
                    Return True
                End If
            Next

            Return False

        End Function

#End Region

        '--- Private 関数

#Region "メール用日付文字列作成"

        ''' <summary>
        ''' メール用日付文字列作成
        ''' </summary>
        ''' <param name="iDate">変換元日付(数字8桁)</param>
        Private Function MailDateStr(ByVal iDate As Integer) As String
            Dim dtwk As Date = New Date(iDate.ToString.Substring(0, 4), iDate.ToString.Substring(4, 2), iDate.ToString.Substring(6, 2))

            Return dtwk.ToString("yyyy/MM/dd")
        End Function

#End Region

#Region "メール用資産関連文字列作成"

        ''' <summary>
        ''' メール用資産関連文字列作成
        ''' </summary>
        ''' <param name="sStr">変換元文字列</param>
        Private Function MailAssetStr(ByVal sStr As String, ByVal iLen As Integer) As String
            Dim sWk As String = ToolCommon.FixByteString(sStr, iLen)

            Return sWk.PadRight(iLen)

        End Function

#End Region

#Region "予約明細取得"
        ''' <summary>
        ''' 予約明細取得
        ''' </summary>
        ''' <param name="lReserveId">予約ID</param>
        ''' <param name="sAssetNo">資産番号</param>
        ''' <returns>
        ''' 予約明細情報
        ''' </returns>
        Public Shared Function GetReserveDtl(lReserveId As Long, sAssetNo As String) As DataTable
            Dim sbSQL As StringBuilder = New StringBuilder
            Dim params As List(Of SqlParameter) = New List(Of SqlParameter)
            Dim clsDB As clsSV_DB = New clsSV_DB
            If clsDB.SVFncOpenConnection(Misc.CommonConst.KEY_DB_CONNECTION) = False Then
                '接続失敗
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_CONNECT_ERR)))
            End If
            Try
                sbSQL.AppendLine("SELECT DTL.* ")
                sbSQL.AppendLine("   ,ASET.ASSET_NAME")
                sbSQL.AppendLine("   ,ASET.FREE_ITEM10")
                sbSQL.AppendLine("   ,PLACE.LEASE_PLACE_NAME")
                sbSQL.AppendLine("FROM T_RESERVE_DETAIL DTL")
                sbSQL.AppendLine("INNER JOIN M_ASSET ASET")
                sbSQL.AppendLine("ON ASET.ASSET_NO = DTL.ASSET_NO")
                sbSQL.AppendLine("INNER JOIN T_RESERVE RSV")
                sbSQL.AppendLine("ON RSV.RESERVE_ID = DTL.RESERVE_ID")
                sbSQL.AppendLine("INNER JOIN M_LEASE_PLACE PLACE")
                sbSQL.AppendLine("ON   PLACE.LEASE_PLACE_CD = RSV.LEASE_PLACE_CD")
                sbSQL.AppendLine("WHERE DTL.RESERVE_ID = @RESERVE_ID AND DTL.ASSET_NO = @ASSET_NO")
                sbSQL.AppendLine("ORDER BY DTL.REAL_START_DT")
                params.Add(New SqlParameter($"@RESERVE_ID", lReserveId))
                params.Add(New SqlParameter($"@ASSET_NO", sAssetNo))

                Dim tbl As DataTable = clsDB.SVFncSelectDataTable(sbSQL.ToString, params)

                Return tbl
            Catch ex As Exception
                logger.Error(ex.Message)
                logger.Error(sbSQL)
                Throw New Exception(String.Format(Misc.CommonRfid.MessageRfid.getMessage(CommonLabel.MSG_ERR_DB_SQL_ERR)))
            Finally
                'DB CLOSE
                clsDB.SVSubCloseConnection()
            End Try
        End Function
#End Region

    End Class
End Namespace
