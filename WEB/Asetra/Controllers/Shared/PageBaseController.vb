Imports Asetra.Models

Namespace Controllers
    ''' <summary>
    ''' ページングを利用するための基底クラス
    ''' BaseControllerを継承します
    ''' </summary>
    Public MustInherit Class PageBaseController
        Inherits BaseController

#Region "クラス変数"
        Protected _PageSize As Integer = ConfigurationManager.AppSettings("PagerSize")       '1ページの表示数
#End Region

#Region "一覧表示処理"
        ''' <summary>
        ''' ページ遷移のクエリを作成します
        ''' </summary>
        ''' <param name="PageNum">クリックされたページ番号</param>
        ''' <param name="model">モデル</param>
        Protected Overrides Sub ViewQueryPager(ByVal PageNum As Integer, ByVal model As BaseModel)
            Dim sbSql As StringBuilder = New StringBuilder(_SQL)

            'ページ番号の設定
            If IsNothing(PageNum) OrElse PageNum <= 0 Then
                PageNum = 1
            End If

            'ページ遷移用のSQLを設定
            sbSql.Append($" OFFSET {_PageSize * (PageNum - 1)} ROWS")
            sbSql.Append($" FETCH NEXT {_PageSize} ROWS ONLY")

            'SQLを追加する
            _SQL = sbSql.ToString

        End Sub
#End Region
        ''' <summary>
        ''' Deleteアクションの後処理をします
        ''' </summary>
        Protected Overrides Sub DisposeDelete(ByRef model As BaseModel)
            '最終ページを削除した場合ページ番号を再設定
            model.PageNum = CurrentPageNum(model)
            'MyBase.Dispose()
        End Sub

        ''' <summary>
        ''' 最終ページを削除した場合の新ページ番号取得処理
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        Protected Function CurrentPageNum(ByVal model As BaseModel) As Integer
            Dim intPageNum As Integer = 1

            With model
                '最終ページを削除した場合、ひとつ前のページ番号を設定する
                If (.PageNum - 1) * .Pager.PageSize + 1 >= .TotalCount Then
                    If .PageNum - 1 > 0 Then
                        intPageNum = .PageNum - 1
                    End If
                End If
            End With

            Return intPageNum

        End Function

    End Class
End Namespace
