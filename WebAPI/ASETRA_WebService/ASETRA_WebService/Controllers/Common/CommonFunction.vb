Public Class CommonFunction

    ''' <summary>
    ''' 操作ログを登録する（正常・警告）
    ''' </summary>
    Public Shared Function InsertOperationLog(ByVal taskName As String, ByVal WordCd As String, Optional ProcessToolDiv As String = CommonConst.PROCESS_TOOL_H) As String
        Dim sb As StringBuilder = New StringBuilder()

        sb.AppendLine("INSERT INTO T_OPERATION_LOG")
        sb.AppendLine("(")
        sb.AppendLine("	 PROCESS_DT")
        sb.AppendLine("	,PROCESS_TM")
        sb.AppendLine("	,PROCESS_TOOL_DIV")
        sb.AppendLine("	,PROCESS_CAT")
        sb.AppendLine("	,PROCESS_CONTENTS")
        sb.AppendLine("	,UPD_DATETIME")
        sb.AppendLine("	,UPD_EMPLOYEE_CD")
        sb.AppendLine("	,UPD_EMPLOYEE_NAME")
        sb.AppendLine(" ,UPD_TASK")
        sb.AppendLine(")")
        sb.AppendLine("SELECT")
        sb.AppendLine("	 CONVERT(int,CONVERT(nvarchar,GETDATE(),112)) ")
        sb.AppendLine("	,FORMAT(GETDATE(),'HHmmssfff') ")
        sb.AppendLine($",'{ProcessToolDiv}'")
        sb.AppendLine($",'{CommonConst.PROCESS_CAT_EV}' ")
        sb.AppendLine("	,M_WORD.WORDING ")
        sb.AppendLine("	,GETDATE()")
        sb.AppendLine("	,' '")
        sb.AppendLine("	,' '")
        sb.AppendLine($",UPD_TASK = '{taskName}'")
        sb.AppendLine("FROM")
        sb.AppendLine(" M_WORD")
        sb.AppendLine("WHERE")
        sb.AppendLine($"     LANG_DIV = '{CommonConst.LANG_DIV_LU}'")
        sb.AppendLine($" AND TOOL_DIV = '{ProcessToolDiv}'")
        sb.AppendLine($" AND WORD_DIV = '{CommonConst.WORD_DIV_M}'")
        sb.AppendLine($" AND WORD_CD = '{WordCd}'")

        Return sb.ToString
    End Function

    ''' <summary>
    ''' 操作ログを登録する（エラー）
    ''' </summary>
    Public Shared Function InsertErrorOperationLog(ByVal taskName As String, ByVal errorMessage As String, Optional ProcessToolDiv As String = CommonConst.PROCESS_TOOL_H) As String
        Dim sb As StringBuilder = New StringBuilder()

        sb.AppendLine("INSERT INTO T_OPERATION_LOG")
        sb.AppendLine("(")
        sb.AppendLine("	 PROCESS_DT")
        sb.AppendLine("	,PROCESS_TM")
        sb.AppendLine("	,PROCESS_TOOL_DIV")
        sb.AppendLine("	,PROCESS_CAT")
        sb.AppendLine("	,PROCESS_CONTENTS")
        sb.AppendLine("	,UPD_DATETIME")
        sb.AppendLine("	,UPD_EMPLOYEE_CD")
        sb.AppendLine("	,UPD_EMPLOYEE_NAME")
        sb.AppendLine(" ,UPD_TASK")
        sb.AppendLine(")")
        sb.AppendLine("SELECT")
        sb.AppendLine("	 CONVERT(int,CONVERT(nvarchar,GETDATE(),112)) ")
        sb.AppendLine("	,FORMAT(GETDATE(),'HHmmssfff') ")
        sb.AppendLine($",'{ProcessToolDiv}'")
        sb.AppendLine($",'{CommonConst.PROCESS_CAT_EV}' ")
        sb.AppendLine($",'{errorMessage}' ")
        sb.AppendLine("	,GETDATE()")
        sb.AppendLine("	,' '")
        sb.AppendLine("	,' '")
        sb.AppendLine($",UPD_TASK = '{taskName}'")

        Return sb.ToString
    End Function

    ''' <summary>
    ''' エラー操作ログ登録を実行する
    ''' </summary>
    Public Shared Sub ErrorLogExecuteSql(ByVal db As SystemDBContext, ByVal taskName As String, ByVal errorMessage As String, Optional ProcessToolDiv As String = CommonConst.PROCESS_TOOL_H)
        Try
            Using tx = db.Database.BeginTransaction((System.Data.IsolationLevel.Serializable))
                db.Database.ExecuteSqlCommand(InsertErrorOperationLog(taskName, errorMessage, ProcessToolDiv))
                db.SaveChanges()
                tx.Commit()
            End Using
        Catch ex As Exception
            '無視する
        End Try
    End Sub

    ''' <summary>
    ''' メッセージ取得
    ''' </summary>
    Public Shared Function GetMessage(ByVal db As SystemDBContext, ByVal strKey As String) As String
        Dim sb As StringBuilder = New StringBuilder()
        Dim strMessage As String = String.Empty

        'キーが存在しない場合ブランクを返却
        If IsNothing(strKey) Then
            Return strMessage
        End If

        'M_WORDテーブルから取得するSQLを作成
        sb.AppendLine("SELECT ")
        sb.AppendLine(" LANG_DIV ")
        sb.AppendLine(" ,TOOL_DIV ")
        sb.AppendLine(" ,WORD_DIV ")
        sb.AppendLine(" ,WORD_CD ")
        sb.AppendLine(" ,WORDING ")
        sb.AppendLine(" ,INS_DATETIME ")
        sb.AppendLine(" ,INS_EMPLOYEE_CD ")
        sb.AppendLine(" ,INS_EMPLOYEE_NAME ")
        sb.AppendLine(" ,UPD_DATETIME ")
        sb.AppendLine(" ,UPD_EMPLOYEE_CD ")
        sb.AppendLine(" ,UPD_EMPLOYEE_NAME ")
        sb.AppendLine(" ,UPD_TASK ")
        sb.AppendLine("FROM ")
        sb.AppendLine("  M_WORD ")
        sb.AppendLine("WHERE ")
        sb.AppendLine($"  TOOL_DIV + WORD_DIV + WORD_CD = '{strKey}' ")
        sb.AppendLine($"  AND LANG_DIV = '{CommonConst.LANG_DIV_LU}' ")

        strMessage = db.M_WORD.SqlQuery(sb.ToString).FirstOrDefault.WORDING

        'メッセージを返却
        Return strMessage
    End Function
End Class
