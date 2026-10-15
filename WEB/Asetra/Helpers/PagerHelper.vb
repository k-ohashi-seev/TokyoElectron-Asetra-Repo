Imports System.Linq.Expressions
Imports Asetra.Misc
Imports Asetra.Models

Namespace Helpers
    ''' <summary>
    ''' ページング機能のヘルパークラス
    ''' </summary>
    Public Class PagerHelper

        ''' <summary>
        ''' ページャーを表示します
        ''' </summary>
        ''' <param name="helper">ヘルパークラス</param>
        ''' <param name="pageData">ページャークラス</param>
        ''' <returns>View表示内容</returns>
        Public Shared Function Pager(ByVal Helper As HtmlHelper, ByVal PageData As Pager, ByVal Message As MessageRfid) As MvcHtmlString
            Dim sbPagerHtml As StringBuilder = New StringBuilder

            sbPagerHtml.Append(Helper.Raw("<div id='pager-wrap'>"))

            '件数を表示
            sbPagerHtml.Append(Helper.Raw("<span class='page-num'>"))
            sbPagerHtml.Append(Helper.Raw(Message.getMessage(CommonLabel.KEY_PAGER_LABEL_ALL) + "&nbsp;" + PageData.TotalCount.ToString + "&nbsp;" + Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER)))
            sbPagerHtml.Append(Helper.Raw("</span>"))

            '1件以上で1ページの表示数内に収まらない場合表示する
            If PageData.TotalCount > 0 Then

                '表示される範囲を表示
                sbPagerHtml.Append(Helper.Raw("<span class='page-title'>"))
                sbPagerHtml.Append(Helper.Raw($"：{PageData.Min}&nbsp;{Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER_FROM)}&nbsp;{PageData.Max}{Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER_TO)}"))
                sbPagerHtml.Append(Helper.Raw("</span>"))

                sbPagerHtml.Append(Helper.Raw("<ul>"))
                sbPagerHtml.Append(Helper.Raw("<li>"))

                '先頭ページかチェックする
                If PageData.IsFirstTop Then
                    '先頭ページの場合
                    sbPagerHtml.Append(Helper.Raw(Message.getMessage(CommonLabel.LABEL_FIRST)))   '★追加(文言は文言マスタから取得してください)★
                    sbPagerHtml.Append(Helper.Raw("</li>")) '★追加★
                    sbPagerHtml.Append(Helper.Raw("<li>")) '★追加★
                    sbPagerHtml.Append(Helper.Raw(Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER_BEFORE)))
                Else
                    '先頭ページではない場合
                    'sbPagerHtml.Append($"<input class='page-input-btn' name='{PageData.TopPage}' type='button' value='最初' onclick='btnPage_onclick(this)'>") ) '2020/02/20 nv miyaji 削除
                    sbPagerHtml.Append($"<input class='page-input-btn' name='{PageData.TopPage}' type='button' value='{Message.getMessage(CommonLabel.LABEL_FIRST)}' onClick='btnPage_onclick(this)'>")    '2020/02/20 nv miyaji add
                    sbPagerHtml.Append(Helper.Raw("</li>")) '★追加★
                    sbPagerHtml.Append(Helper.Raw("<li>")) '★追加★
                    sbPagerHtml.Append($"<input class='page-input-btn' name='{PageData.PrevPage}' type='button' value='{Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER_BEFORE)}' onClick='btnPage_onclick(this)'>")
                End If
                sbPagerHtml.Append(Helper.Raw("</li>"))

                'スキップ表示
                If PageData.SkipFrom Then
                    sbPagerHtml.Append(Helper.Raw("<li class='skip'>"))
                    sbPagerHtml.Append("...")
                    sbPagerHtml.Append(Helper.Raw("</li>"))
                End If

                '数字を表示する
                For i As Integer = PageData.TopPageRange To PageData.LastPageRange
                    sbPagerHtml.Append(Helper.Raw("<li>"))
                    If PageData.Current(i) Then
                        sbPagerHtml.Append("<b>")
                        sbPagerHtml.Append(Helper.Raw(i.ToString()))
                        sbPagerHtml.Append("</b> ")
                    Else
                        sbPagerHtml.Append($"<input class='page-input-btn' name='{i}' type='button' value='{i}' onClick='btnPage_onclick(this)'>")
                    End If
                    sbPagerHtml.Append(Helper.Raw("</li>"))
                Next

                '最終ページかチェックする
                If (PageData.IsLastEnd) Then
                    '最終ページの場合
                    sbPagerHtml.Append(Helper.Raw("<li>"))
                    sbPagerHtml.Append(Helper.Raw(Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER_AFTER)))
                    sbPagerHtml.Append(Helper.Raw("</li>")) '★追加★
                    sbPagerHtml.Append(Helper.Raw("<li>")) '★追加★
                    sbPagerHtml.Append(Helper.Raw(Helper.Raw(Message.getMessage(CommonLabel.LABEL_LAST))))   '★追加(文言は文言マスタから取得してください)★
                    sbPagerHtml.Append(Helper.Raw("</li>"))
                Else
                    '最終ページではない場合
                    'スキップ表示
                    If (PageData.SkipTo) Then
                        sbPagerHtml.Append(Helper.Raw("<li class='skip'>"))
                        sbPagerHtml.Append("...")
                        sbPagerHtml.Append(Helper.Raw("</li>"))
                    End If
                    sbPagerHtml.Append(Helper.Raw("<li>"))
                    sbPagerHtml.Append($"<input class='page-input-btn' name='{PageData.NextPage}' type='button' value='{Message.getMessage(CommonLabel.KEY_PAGER_LABEL_NUMBER_AFTER)}' onClick='btnPage_onclick(this)'>")
                    sbPagerHtml.Append(Helper.Raw("</li>")) '★追加★
                    sbPagerHtml.Append(Helper.Raw("<li>")) '★追加★
                    'sbPagerHtml.Append($"<input class='page-input-btn' name='{PageData.LastPage}' type='button' value='最後({PageData.LastPage})' onClick='btnPage_onclick(this)'>") '2020/02/20 nv miyaji 削除
                    sbPagerHtml.Append($"<input class='page-input-btn' name='{PageData.LastPage}' type='button' value='{Message.getMessage(CommonLabel.LABEL_LAST)}({PageData.LastPage})' onClick='btnPage_onclick(this)'>")    '2020/02/20 nv miyaji add
                    sbPagerHtml.Append(Helper.Raw("</li>"))
                End If

                'ページ番号のhidden項目(ボタンがクリックされるとhiddenに番号が設定される)
                sbPagerHtml.Append(Helper.Raw("</ul>"))
            End If

            sbPagerHtml.Append(Helper.Raw("</div>"))

            Return MvcHtmlString.Create(sbPagerHtml.ToString())

        End Function
    End Class

    ''' <summary>
    ''' 検索条件、入力作成用のヘルパークラス
    ''' </summary>
    Public Class SearchInputItemHelper

        ''' <summary>
        ''' 検索条件を表示します
        ''' </summary>
        ''' <returns>View表示内容</returns>
        Public Shared Function SearchItemFor(Of TModel, TProperty)(htmlHelper As HtmlHelper(Of TModel),
                                                                   expression As Expression(Of Func(Of TModel, TProperty)),
                                                                   ByVal searchItem As SearchInputItemModel,
                                                                   Optional htmlAttributes As Object = Nothing) As MvcHtmlString

            Dim ret = New MvcHtmlString("")

            ' TextBoxForメソッドの実行
            Dim sbPagerHtml As StringBuilder = New StringBuilder

            ' 設定マスタに未登録の場合テキストボックスを返却
            If searchItem Is Nothing Then
                Return htmlHelper.TextBoxFor(expression, htmlAttributes)
            End If

            Select Case searchItem.DATA1
                Case InputType.TEXT
                    ret = htmlHelper.TextBoxFor(expression, htmlAttributes)
                Case InputType.LIST
                    ret = htmlHelper.DropDownListFor(expression, DirectCast(searchItem.DropdownList, IEnumerable(Of SelectListItem)), "", htmlAttributes)
                Case Else
                    Return InputExtensions.TextBoxFor(htmlHelper, expression, htmlAttributes)
            End Select

            Return ret

        End Function

        ''' <summary>
        ''' 入力欄を表示します
        ''' </summary>
        ''' <returns>View表示内容</returns>
        Public Shared Function InputItemFor(Of TModel, TProperty)(htmlHelper As HtmlHelper(Of TModel),
                                                                   expression As Expression(Of Func(Of TModel, TProperty)),
                                                                   ByVal searchItem As SearchInputItemModel,
                                                                   Optional htmlAttributes As Object = Nothing) As MvcHtmlString

            Dim ret = New MvcHtmlString("")

            ' TextBoxForメソッドの実行
            Dim sbPagerHtml As StringBuilder = New StringBuilder

            ' 設定マスタに未登録の場合テキストボックスを返却
            If searchItem Is Nothing Then
                Return htmlHelper.EditorFor(expression, htmlAttributes)
            End If

            Select Case searchItem.DATA1
                Case InputType.TEXT, InputType.AREA
                    ret = htmlHelper.EditorFor(expression, htmlAttributes)
                Case InputType.LIST
                    ret = htmlHelper.DropDownListFor(expression, DirectCast(searchItem.DropdownList, IEnumerable(Of SelectListItem)), "", htmlAttributes.htmlAttributes)
                Case InputType.LABEL
                    Return htmlHelper.EditorFor(expression, New With {.htmlAttributes = New With {.class = "label-control", .maxlength = htmlAttributes.htmlAttributes.maxlength, .style = htmlAttributes.htmlAttributes.style, .readonly = True, .tabIndex = "-1"}})
                Case Else
                    ret = htmlHelper.EditorFor(expression, htmlAttributes)
            End Select

            Return ret

        End Function
    End Class
End Namespace