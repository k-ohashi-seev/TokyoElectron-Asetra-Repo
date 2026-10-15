Imports System.Web.Optimization

Public Module BundleConfig
    ' バンドルの詳細については、http://go.microsoft.com/fwlink/?LinkId=301862 を参照してください
    Public Sub RegisterBundles(ByVal bundles As BundleCollection)

        bundles.Add(New ScriptBundle("~/bundles/jquery").Include(
                            "~/Scripts/jquery-{version}.js",
                            "~/Scripts/jquery-ui.js",
                            "~/Scripts/jquery.event.drag-2.3.0.js"))

        bundles.Add(New ScriptBundle("~/bundles/jqueryval").Include(
                    "~/Scripts/jquery.validate*"))

        ' 開発と学習には、Modernizr の開発バージョンを使用します。次に、実稼働の準備が
        ' できたら、http://modernizr.com にあるビルド ツールを使用して、必要なテストのみを選択します。
        bundles.Add(New ScriptBundle("~/bundles/modernizr").Include(
                    "~/Scripts/modernizr-*"))

        bundles.Add(New ScriptBundle("~/bundles/bootstrap").Include(
                  "~/Scripts/bootstrap.js",
                  "~/Scripts/respond.js"))

        bundles.Add(New ScriptBundle("~/bundles/common").Include(
                  "~/Scripts/common.js"))

        bundles.Add(New ScriptBundle("~/bundles/myslickgrid").Include(
                  "~/Scripts/myslickgrid.js",
                  "~/Scripts/slick.core.js",
                  "~/Scripts/slick.grid.js",
                  "~/Scripts/slick.dataview.js",
                  "~/Scripts/slick.checkboxselectcolumn.js",
                  "~/Scripts/slick.colgroup.js",
                  "~/Scripts/slick.rowmovemanager.js",
                  "~/Scripts/slick.rowselectionmodel.js",
                  "~/Scripts/slick.editors.js"))

        bundles.Add(New ScriptBundle("~/bundles/select2").Include(
                  "~/Scripts/select2/select2.min.js",
                  "~/Scripts/select2/i18n/ja.js",
                  "~/Scripts/select2-common.js"))

        ' 複数入力ダイアログ（共通）
        bundles.Add(New ScriptBundle("~/bundles/multiinput").Include(
                  "~/Scripts/multi-input-dialog.js"))

        ' select 表示（選択肢＝「コード 名称」／選択後＝コードのみ）（共通）
        bundles.Add(New ScriptBundle("~/bundles/selectcodename").Include(
                  "~/Scripts/select-code-name.js"))

        bundles.Add(New StyleBundle("~/Content/css").Include(
                  "~/Content/bootstrap.css",
                  "~/Content/customize.css",
                  "~/Content/Menu.css",
                  "~/Content/Modal.css",
                  "~/Content/Operation.css",
                  "~/Content/Site.css",
                  "~/Content/Table.css",
                  "~/Content/jquery-ui.css",
                  "~/Content/jquery-ui.min.css",
                  "~/Content/slick.grid.css",
                  "~/Content/slick-default-theme.css",
                  "~/Content/select2/select2.min.css"
                  ))

    End Sub
End Module

