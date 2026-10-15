Namespace Misc
    Public Class Pager

        ''' <summary>
        ''' 1ページに表示する最大件数
        ''' </summary>
        Public Property PageSize As Integer = ConfigurationManager.AppSettings("PagerSize")

        ''' <summary>
        ''' 画面に表示するページリンクの最大数
        ''' </summary>
        Public Property MaxPageNumber As Integer = 10       '★変更★

        ''' <summary>
        ''' 現在のページ番号
        ''' </summary>
        Public Property PageNumber As Integer = 0

        ''' <summary>
        ''' 全件数
        ''' </summary>
        Public Property TotalCount As Integer = 0

        ''' <summary>
        ''' ページャに表示する前後の最大ページ数
        ''' </summary>
        Public Property Pages As Integer = 0

        ''' <summary>
        ''' 検索結果が 1件以上あるかを取得します
        ''' </summary>
        ''' <returns>True:1件以上、False:0件</returns>
        Public Function IsResult() As Boolean
            Dim blnRet As Boolean = False
            If TotalCount >= 1 Then
                blnRet = True
            End If
            Return blnRet
        End Function

        ''' <summary>
        ''' ページャの表示が必要かを取得します
        ''' 結果が1ページ内に収まっている場合はページャは不要
        ''' </summary>
        ''' <returns>True:ページャー必要、False:ページャー不要</returns>
        Public Function IsPageNeed() As Boolean
            Dim blnRet As Boolean = False
            If TotalCount > PageSize Then
                blnRet = True
            End If
            Return blnRet
        End Function

        ''' <summary>
        ''' 表示開始位置が先頭であるかを取得します
        ''' </summary>
        ''' <returns>True:先頭、False:先頭ではない</returns>
        Public Function IsFirstTop() As Boolean
            Dim blnRet As Boolean = False
            If PageNumber = 1 Then
                blnRet = True
            End If
            Return blnRet
        End Function

        ''' <summary>
        ''' 表示開始位置が末尾であるかを取得します
        ''' </summary>
        ''' <returns>True:末尾、False:末尾ではない</returns>
        Public Function IsLastEnd() As Boolean
            Dim blnRet As Boolean = False
            If PageNumber * PageSize >= TotalCount Then
                blnRet = True
            End If
            Return blnRet
        End Function

        ''' <summary>
        '''  先頭ページの先頭 ID を取得します
        ''' </summary>
        ''' <returns>先頭ID</returns>
        Public Function TopPage() As Integer
            Return 1
        End Function

        ''' <summary>
        '''  画面に表示する先頭ページの先頭 ID を取得します
        ''' </summary>
        ''' <returns>先頭ID</returns>
        Public Function TopPageRange() As Integer
            Dim intPlusNum As Integer = 0
            Dim intHalfNum As Integer = getHalf()
            Dim intLastPage As Integer = LastPage()

            '選択番号が半分を超えている場合、その分前の数値を表示する
            If PageNumber + intHalfNum > intLastPage Then
                intPlusNum = PageNumber + intHalfNum - intLastPage
            End If

            '★追加 START★
            '表示ページ番号数が2で割り切れる場合、中心をずらす
            If MaxPageNumber Mod 2 = 0 Then
                intHalfNum = intHalfNum - 1
            End If
            '★追加 END★

            Return Math.Max(PageNumber - intHalfNum - intPlusNum, 1)
        End Function

        ''' <summary>
        '''  前のページの先頭 id を取得します
        ''' </summary>
        ''' <returns>前のページの先頭 id</returns>
        Public Function PrevPage() As Integer
            Return PageNumber - 1
        End Function

        ''' <summary>
        '''  次のページの先頭 id を取得します
        ''' </summary>
        ''' <returns>次のページの先頭 id</returns>
        Public Function NextPage() As Integer
            Return PageNumber + 1
        End Function

        ''' <summary>
        '''  最後のページの先頭 id を取得します
        ''' </summary>
        ''' <returns>最後のページの先頭 id</returns>
        Public Function LastPage() As Integer
            Return Math.Ceiling(TotalCount / PageSize)
        End Function

        ''' <summary>
        '''  画面に表示する次のページの先頭 id を取得します
        ''' </summary>
        ''' <returns>次のページの先頭 id </returns>
        Public Function LastPageRange() As Integer
            Return Math.Min(TopPageRange() + MaxPageNumber - 1, LastPage)
        End Function

        ''' <summary>
        '''  現在のページから指定したページ数前の先頭 id を取得します
        ''' </summary>
        ''' <returns></returns>
        Public Function Min() As Integer
            Return Math.Max(0, (PageNumber - 1) * PageSize + 1)
        End Function

        ''' <summary>
        '''  現在のページから指定したページ数後の先頭 id を取得します
        ''' </summary>
        ''' <returns></returns>
        Public Function Max() As Integer
            Return Math.Min(TotalCount, PageNumber * PageSize)
        End Function

        ''' <summary>
        ''' 現在表示しているページの表示件数を取得します
        ''' </summary>
        ''' <returns></returns>
        Public Function GetCurrentPageRecord() As String
            Dim px = 57 + (Max() - Min() + 1) * 25 + 25
            'ヘッダーと下部のスクロールバーの分を足す
            Return px.ToString() + "px"
        End Function

        ''' <summary>
        ''' ページングを開始する最初のページが先頭より後であるかを取得します
        ''' </summary>
        ''' <returns></returns>
        Public Function SkipFrom() As Boolean
            Dim blnRet As Boolean = False
            If TopPageRange() > TopPage() Then
                blnRet = True
            End If
            Return blnRet
        End Function

        ''' <summary>
        ''' ページングを開始する最初のページが先頭より後であるかを取得します
        ''' </summary>
        ''' <returns></returns>
        Public Function SkipTo() As Boolean
            Dim blnRet As Boolean = False
            If LastPageRange() < LastPage() Then
                blnRet = True
            End If
            Return blnRet
        End Function

        ''' <summary>
        ''' 指定した id が現在のページであるかを取得します。
        ''' </summary>
        ''' <param name="id">検査する id。</param>
        ''' <returns>id が現在のページ内であれば true。</returns>
        Public Function Current(ByVal id As Integer) As Boolean
            Dim blnRet As Boolean = False
            If id = PageNumber Then
                blnRet = True
            End If
            Return blnRet
        End Function

        ''' <summary>
        ''' 中心の位置を取得します。
        ''' </summary>
        ''' <returns>中心の番号</returns>
        Private Function getHalf() As Integer
            Return Math.Truncate(MaxPageNumber / 2)
        End Function

    End Class
End Namespace
