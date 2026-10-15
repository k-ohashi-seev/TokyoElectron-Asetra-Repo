@imports PagedList.Mvc
@Imports Asetra.Models
@Imports Asetra.Misc
@Imports Asetra.Helpers
@ModelType List(Of InventoryInquiryViewModel)
@Code
    Dim Message As MessageRfid = App(CommonConst.KEY_APP_LANGUAGE)
    ViewData(CommonConst.KEY_PAGE_TITLE) = Message.getMessage(CommonLabelInventoryInquiry.LABEL_TITLE)
    Dim _lstCheckedKey As List(Of String) = Session(CommonSessionKey.LIST_CHECKED_KEYS)
    Dim jsonCheckKeys As String = Newtonsoft.Json.JsonConvert.SerializeObject(_lstCheckedKey)
    Dim assetNoCondition As String() = String.Join(",", _lstCheckedKey.Select(Function(x) x.Split(",")).Select(Function(X) X(0))).Split(",")
    Dim _editedContain As List(Of EditedContainModel) = Session(CommonSessionKey.LIST_EDITED_CONTAINS)
    Dim jsonEditedContain As String = Newtonsoft.Json.JsonConvert.SerializeObject(_editedContain)
End Code
<script>

    const changedCellColor = "yellow";
    const errorCellColor = "red";

    function MyCustomEditor(args) {
        var $input;
        var defaultValue;
        var maxLen = args.column.maxLength || 20;
        this.init = function () {
            $input = $("<input type='text' class='editor-text' />")
                .attr('maxlength', maxLen)
                .appendTo(args.container)
                .focus()
                .select();
        };

        this.destroy = function () {
            $input.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            defaultValue = item[args.column.field];
            $input.val(defaultValue);
            $input.select();
        };

        this.serializeValue = function () {
            return $input.val();
        };

        this.applyValue = function (item, state) {
            const oldValue = item[args.column.field];
            var o = oldValue == null ? "" : oldValue;
            var s = state == null ? "" : state;

            if (o != s) {
                item["EDITED_" + args.column.field] = true;
            }
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return $input.val() != defaultValue;
        };

        this.validate = function () {
            return { valid: true, msg: null };
        };

        this.init();
    }

    $(document).on("mousedown", function (e) {

        // Select2 の候補リストは body 直下に描画されるため、祖先要素まで判定する
        if ($(e.target).closest("select, option, .ui-selectmenu-menu, .dropdown-menu, .select2-container").length > 0) {
            return; // ← 重要
        }

        // SlickGrid の DOM か？
        // ※グリッド内（エディタの入力欄や年月ピッカーのボタン）のクリックで編集が確定しないようにする
        const isInsideGrid = $(e.target).closest(".slickgrid-container, #ListGrids").length > 0;

        // Grid外クリックなら編集終了
        if (!isInsideGrid) {
            if (grid.getEditorLock().isActive()) {
                grid.getEditorLock().commitCurrentEdit();
            }
        }
    });

    function MyHalfWidthEditor(args) {
        var $input;
        var defaultValue;
        var maxLen = args.column.maxLength || 20;

        this.init = function () {
            $input = $("<input type='text' class='editor-text' style='backgounrd: blue;' pattern='[ -~]*' />") // 半角のみ
                .on('input', function () {
                    // 入力時に全角を削除
                    this.value = this.value.replace(/[^\x20-\x7E]/g, '');
                })
                .attr('maxlength', maxLen)
                .appendTo(args.container)
                .focus()
                .select();
        };

        this.destroy = function () {
            $input.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            defaultValue = item[args.column.field];
            $input.val(defaultValue);
            $input.select();
        };

        this.serializeValue = function () {
            return $input.val();
        };

        this.applyValue = function (item, state) {
            const oldValue = item[args.column.field];
            var o = oldValue == null ? "" : oldValue;
            var s = state == null ? "" : state;

            if (o != s) {
                item["EDITED_" + args.column.field] = true;
            }
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return $input.val() != defaultValue;
        };

        this.validate = function () {
            return { valid: true, msg: null };
        };

        this.init();
    }

    // 年月入力エディタ（除却時期）
    // ※入力UIの値は yyyy-MM、行データはサーバーの形式チェックに合わせて yyyy/MM で保持する
    function MonthEditor(args) {
        var $input;
        var defaultValue;

        // yyyy/MM（yyyy/M、yyyy-MM も可）→ yyyy-MM に変換。形式が不正な場合は空
        function toMonthInputValue(value) {
            var m = String(value == null ? "" : value).trim().match(/^(\d{4})[\/-](\d{1,2})$/);
            return m ? m[1] + "-" + ("0" + m[2]).slice(-2) : "";
        }

        this.init = function () {
            $input = $("<input type='month' class='editor-text' />")
                .on("keydown.nav", function (e) {
                    // 矢印キーは年月の変更に使うため、グリッドのセル移動に渡さない
                    if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT ||
                        e.keyCode === $.ui.keyCode.UP || e.keyCode === $.ui.keyCode.DOWN) {
                        e.stopImmediatePropagation();
                    }
                })
                .appendTo(args.container)
                .focus();
        };

        this.destroy = function () {
            $input.remove();
        };

        this.focus = function () {
            $input.focus();
        };

        this.loadValue = function (item) {
            $input.val(toMonthInputValue(item[args.column.field]));
            defaultValue = this.serializeValue();
        };

        this.serializeValue = function () {
            return $input.val().replace("-", "/");
        };

        this.applyValue = function (item, state) {
            const oldValue = item[args.column.field];
            var o = oldValue == null ? "" : oldValue;
            var s = state == null ? "" : state;

            if (o != s) {
                item["EDITED_" + args.column.field] = true;
            }
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return this.serializeValue() != defaultValue;
        };

        this.validate = function () {
            return { valid: true, msg: null };
        };

        this.init();
    }

    // ロケーション選択エディタ（変更先中区分・小区分）
    // ※中区分は locationMList、小区分は行の変更先中区分で絞り込んだ locationSList を候補にする
    function LocationSelect2Editor(args) {
        var $select;
        var defaultValue;
        var isLocS = (args.column.field === "CHANGE_LOC_S_CD");

        this.init = function () {
            var list;
            if (isLocS) {
                //[CC431] UPD Start 20261002
                // 変更後課コードが空の場合は、棚卸資産T.ロケ中区分コード配下の場所を候補にする
                //[CC431] UPD Start 20261007 変更後課コードが空白だけの場合も空として扱う（サーバー側の IsNullOrWhiteSpace と合わせる）
                var mCode = (args.item.CHANGE_LOC_M_CD || "").trim() || args.item.LOC_M_CD || "";
                //[CC431] UPD End 20261007
                //[CC431] UPD End 20261002
                list = locationSList
                    .filter(l => l.LOC_M_CD == mCode)
                    .map(l => ({ id: l.LOC_S_CD, text: l.LOC_NAME }));      // LOC_NAME は「コード 名称」
            } else {
                list = locationMList
                    .map(l => ({ id: l.LOC_M_CD, text: l.LOC_NAME }));
            }

            // 候補にない値が入っている場合も、現在値として表示できるように追加する
            //[CC431] UPD Start 20261007 空白だけの値は空として扱う（空白の候補を追加しない）
            var current = (args.item[args.column.field] || "").trim();
            //[CC431] UPD End 20261007
            if (current !== "" && !list.some(l => l.id == current)) {
                list.unshift({ id: current, text: current });
            }

            // 検索条件の単一選択（Scripts\select2-common.js の select2-clearable）と同じ見た目・動きにする
            // ・入力して絞り込み、×で選択解除（空の選択肢を先頭に追加）
            // ・選択肢・選択後とも「コード 名称」を表示（LOC_NAME）
            // ・×のツールチップは文言マスタ（_Layout.vbhtml の strSelect2ClearTooltip）
            $select = $("<select class='editor-select'></select>").appendTo(args.container);
            $select.select2({
                data: [{ id: "", text: "" }].concat(list),
                width: "100%",
                language: (typeof strSelect2ClearTooltip !== "undefined" && strSelect2ClearTooltip)
                    ? [{ removeAllItems: function () { return strSelect2ClearTooltip; } }, 'ja']
                    : 'ja',
                allowClear: true,
                placeholder: { id: "", text: "" },    // allowClear には placeholder の指定が必要（セル内のため文言は表示しない）
                dropdownParent: $("body")       // セルの外に候補を表示するため
            });
            $select.data("select2").$dropdown.addClass("inventory-grid-select2");

            // 選択を確定する
            // ※args.commitChanges() は autoEdit により下の行へ移動し、次のエディタ（候補リスト）まで開くため使わない
            // ※Select2 が選択後の後処理（候補リストを閉じる）を終えてから確定する
            var isClearing = false;
            var commitLater = function () {
                setTimeout(function () {
                    isClearing = false;
                    if (args.grid.getEditorLock().isActive()) {
                        args.grid.getEditorLock().commitCurrentEdit();
                    }
                }, 0);
            };

            // 候補を選んだら確定
            $select.on("select2:select", commitLater);

            // ×で選択解除したら確定
            // ※Select2 は×の後に候補リストを開くため、確定するまで開かないようにする
            $select.on("select2:clear", function () {
                isClearing = true;
                commitLater();
            });
            $select.on("select2:opening", function (e) {
                if (isClearing) {
                    e.preventDefault();
                }
            });

            // loadValue で現在値を設定した後に候補を開く
            setTimeout(function () {
                if ($select && $select.data("select2")) {
                    $select.select2("open");
                }
            }, 0);
        };

        this.destroy = function () {
            if ($select.data("select2")) {
                $select.select2("destroy");
            }
            $select.remove();
            $select = null;
        };

        this.focus = function () {
            $select.next(".select2-container").find(".select2-selection").focus();
        };

        this.loadValue = function (item) {
            //[CC431] UPD Start 20261007 空白だけの値は空として扱う（開いて閉じただけで変更ありにならないようにする）
            defaultValue = (item[args.column.field] || "").trim();
            //[CC431] UPD End 20261007
            $select.val(defaultValue).trigger("change.select2");
        };

        this.serializeValue = function () {
            return $select.val() || "";
        };

        this.applyValue = function (item, state) {
            const oldValue = item[args.column.field];
            var o = oldValue == null ? "" : oldValue;
            var s = state == null ? "" : state;

            if (o != s) {
                item["EDITED_" + args.column.field] = true;

                // 中区分が変わったら、小区分は候補が変わるためクリアする
                if (!isLocS && item["CHANGE_LOC_S_CD"] != null && item["CHANGE_LOC_S_CD"] != "") {
                    item["CHANGE_LOC_S_CD"] = "";
                    item["EDITED_CHANGE_LOC_S_CD"] = true;
                }
            }
            item[args.column.field] = state;
        };

        this.isValueChanged = function () {
            return this.serializeValue() != defaultValue;
        };

        this.validate = function () {
            return { valid: true, msg: null };
        };

        this.init();
    }

    // 一覧内のプルダウン（変更後稼働区分・転用or除却・除却理由）
    // ※絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を先頭に追加）
    //   ×のクリック処理・表示切替は Scripts\multi-input-dialog.js（.clearable / .clearable-clear）
    Slick.Editors.Select = function (args) {
        var $select;
        var $wrap;
        var defaultValue;

        this.init = function () {
            var option_str = "<OPTION value=''></OPTION>";

            for (key in args.column.options) {
                option_str += "<OPTION value='" + key + "'>" + args.column.options[key] + "</OPTION>";
            }
            $wrap = $("<span class='clearable'></span>").appendTo(args.container);
            $select = $("<SELECT tabindex='0' class='editor-select'>" + option_str + "</SELECT>");
            $select.appendTo($wrap);
            $("<span class='clearable-clear'></span>")
                .attr("title", "@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)")
                .text("×")
                .appendTo($wrap);
            $select.focus();

            $select.on("keydown.nav", function (e) {
                    if (e.keyCode === Slick.keyCode.LEFT || e.keyCode === Slick.keyCode.RIGHT) {
                        e.stopImmediatePropagation();
                    }
                })
                // ▼ 選択変更した瞬間に値を適用する
                .on("change", function () {
                    var value = $select.val();
                    this.applyValue(args.item, value);
                    args.commitChanges(); // これで即時確定
                }.bind(this));
        };

        this.destroy = function () {
            $wrap.remove();
        };

        this.focus = function () {
            $select.focus();
        };

        this.loadValue = function (item) {
            defaultValue = item[args.column.field] || "";
            $select.val(defaultValue);
            // 値がある場合のみ×を表示する
            refreshClearable($select);
        };

        this.serializeValue = function () {
            return $select.val();
        };

        this.applyValue = function (item, state) {
            const oldValue = item[args.column.field];
            var o = oldValue == null ? "" : oldValue;
            var s = state == null ? "" : state;

            if (o !== s) {
                item["EDITED_" + args.column.field] = true;

                if (args.column.field == "DIVERSION_REMOVE") {
                    var DiversionRemoveDataList = @Html.Raw(ViewBag.DiversionRemoveDataList);
                    var data = DiversionRemoveDataList.find(f => f.Key1 == state);
                    if (!(data != null && data.DATA2 == "Y")) {
                        //「Y」以外の場合は除去時期入力不可。
                        if (item["REMOVE_DATE"] != null && item["REMOVE_DATE"] != "") {
                            item["EDITED_REMOVE_DATE"] = true;
                        }
                        item["REMOVE_DATE"] = "";
                        item["DISABLE_REMOVE_DATE"] = true;

                        //「Y」以外の場合は除却理由入力不可
                        if (item["REMOVE_REASON"] != null && item["REMOVE_REASON"] != "") {
                            item["EDITED_REMOVE_REASON"] = true;
                        }
                        item["REMOVE_REASON"] = "";
                        item["DISABLE_REMOVE_REASON"] = true;

                        //「Y」以外の場合は除却内容入力不可
                        if (item["REMOVE_CONTENT"] != null && item["REMOVE_CONTENT"] != "") {
                            item["EDITED_REMOVE_CONTENT"] = true;
                        }
                        item["REMOVE_CONTENT"] = "";
                        item["DISABLE_REMOVE_CONTENT"] = true;


                    } else {
                        item["DISABLE_REMOVE_DATE"] = false;
                        item["DISABLE_REMOVE_REASON"] = false;
                    }
                }

                if (args.column.field == "REMOVE_REASON") {
                    //[CC431] UPD Start 20261002
                    // 除却内容は、除却理由が空白以外の場合に入力可。空白に戻したら除却内容をクリアする
                    if (s == "") {
                    //[CC431] UPD End 20261002
                        //「Y」の場合は除去時期入力不可。
                        if (item["REMOVE_CONTENT"] != null && item["REMOVE_CONTENT"] != "") {
                            item["EDITED_REMOVE_CONTENT"] = true;
                        }
                        item["REMOVE_CONTENT"] = "";
                        item["DISABLE_REMOVE_CONTENT"] = true;
                    } else {
                        item["DISABLE_REMOVE_CONTENT"] = false;
                    }
                }

            }
            item[args.column.field] = state;

        };

        this.isValueChanged = function () {
            return ($select.val() != defaultValue);
        };

        this.validate = function () {
            return { valid: true, msg: null };
        };

        this.init();
    };

    //
    //  実棚照会
    //
    $(function () {

        //ログインセッションが切れている場合はダイアログ表示
        if ("@Model.FirstOrDefault.ErrCode" != "" && "@Model.FirstOrDefault.ErrTarget" == "@CommonConst.KEY_FROM_LOGIN") {
            // 2019/12/24 miyaji upd start エラーメッセージ表示後、ライセンスコントローラーを呼び出すように修正
            showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/License")/Index");
            //エラーメッセージ表示後、ログイン画面に遷移
            @*showLoginMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "@Url.Content("~/Login")/Index");*@
            // 2019/12/24 miyaji upd end
            return;
        }

        //ロード前のボタン名を設定する
        document.getElementById("OnCreteButton").value = "@Model.FirstOrDefault.OnCreteButton"

        //[CC431] ADD Start 20261002
        // 詳細検索エリアの開閉状態の復元（DetailOpenFlg が「▲詳細検索」のときだけ開いた状態にする。既定は閉）
        if ("@Model.FirstOrDefault.DetailOpenFlg" == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)") {
            $(".detail-accordion").css("display", "flex");
            document.getElementById("DetailAccordion").innerHTML = "@Model.FirstOrDefault.DetailOpenFlg";
        }
        //[CC431] ADD End 20261002

        //[CC431] ADD Start 20261005
        // 検索条件の開閉状態の復元（OpenFlg が「閉」の文言のときだけ閉じた状態にする。既定は開）
        if ("@Model.FirstOrDefault.OpenFlg" == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            $("#cond").removeClass("in");
            $("#cond").addClass("collapse");
            document.getElementById("SearchCondition").innerHTML = "@Model.FirstOrDefault.OpenFlg";
        }
        //[CC431] ADD End 20261005

        //--2019/10/15 nv miyaji add start
        //正常終了時にメッセージを表示する
        if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessEdit") {
            //現ロケに変更
            showResultMessage("@Message.getMessage(CommonLabel.MSG_SUCCESS_EDIT)")
        } else if ("@Model.FirstOrDefault.States" == "@StatusType.SuccessLending") {
            //現物確認
            showResultMessage("@Message.getMessage(CommonLabelInventoryInquiry.MSG_GENBUTSU_KAKUNIN_SUCCESS)")
        }
        //--2019/10/15 nv miyaji add end

        //Controllerでエラーの場合、エラーメッセージを表示する
        if ("@Model.FirstOrDefault.ErrCode" != "") {
            showErrMessage("@Message.getMessage(Model.FirstOrDefault.ErrCode)", "#@Model.FirstOrDefault.ErrTarget")
        }

        // 大区分非選択時は中区分を非活性
        @*if (document.getElementById("SearchLocL").value == "") {  // 2020/03/17 nv miyaji upd*@
        if ("@ViewBag.LocUsage_M" != "@Misc.ItemUse.Yes" || document.getElementById("SearchLocL").value == "") {  // 2020/03/17 nv miyaji upd
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        // 中区分非選択時は小区分を非活性（中区分・小区分は複数選択）
        @*if (document.getElementById("SearchLocM").value == "") {*@  // 2020/03/17 nv miyaji upd
        //[CC431] UPD Start 20261007
        // ※場所から選ぶ ON の場合は、小区分非選択時に中区分を非活性
        if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes" || (!isSearchLocReverse("Loc") && getMultiSelectValues("#SearchLocM").length == 0)) {  // 2020/03/17 nv miyaji upd{
            clearMultiSelect("#SearchLocS");
            $("#SearchLocS").attr("disabled", "disabled");
        }
        if (isSearchLocReverse("Loc") && getMultiSelectValues("#SearchLocS").length == 0) {
            clearMultiSelect("#SearchLocM");
            $("#SearchLocM").attr("disabled", "disabled");
        }
        //[CC431] UPD End 20261007

        // 変更後中区分非選択時は変更後小区分を非活性（変更後中区分・変更後小区分は複数選択）
        //[CC431] UPD Start 20261007
        // ※場所から選ぶ ON の場合は、変更後小区分非選択時に変更後中区分を非活性
        if ("@ViewBag.LocUsage_S" != "@Misc.ItemUse.Yes" || (!isSearchLocReverse("ChangeLoc") && getMultiSelectValues("#SearchChangeLocMCd").length == 0)) {  // 2020/03/17 nv miyaji upd{
            clearMultiSelect("#SearchChangeLocSCd");
            $("#SearchChangeLocSCd").attr("disabled", "disabled");
        }
        if (isSearchLocReverse("ChangeLoc") && getMultiSelectValues("#SearchChangeLocSCd").length == 0) {
            clearMultiSelect("#SearchChangeLocMCd");
            $("#SearchChangeLocMCd").attr("disabled", "disabled");
        }
        //[CC431] UPD End 20261007

        //隠し項目に格納
        document.getElementById("LoadingImg").value = '@Url.Content("~/Images/")' + "Loading.gif";

        @Code
            Dim locationMList = Newtonsoft.Json.JsonConvert.SerializeObject(Model.FirstOrDefault().LocationMList)
            Dim locationSList = Newtonsoft.Json.JsonConvert.SerializeObject(Model.FirstOrDefault().LocationSList)
            Dim employeeList = Newtonsoft.Json.JsonConvert.SerializeObject(Model.FirstOrDefault().EmployeeList)
        End Code
        locationMList = @Html.Raw(locationMList);
        locationSList = @Html.Raw(locationSList);
        employeeList = @Html.Raw(employeeList);

        //列定義
        let columns = [
            { id: "@ColKey.InventoryInquiry.CHECK_SELECT", name: "@Message.getMessage(CommonLabel.LABEL_SELECT)", field: "DISP_FLAG", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.CHECK_SELECT", 40), sortOrder: -2, formatter: InventoryInquiryCheckboxFormatter, cssClass:    "first-column" },
            { id: "@ColKey.InventoryInquiry.ASSET_STATUS", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_STATUS)', field: "ASSET_STATUS", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.ASSET_STATUS", 80), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_STATUS_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_STATUS_NAME").DispFlag", formatter: AssetStatusFormatter},
            { id: "@ColKey.InventoryInquiry.LOCK_EMPLOYEE_CD", name: '@Message.getMessage(CommonLabel.LABEL_LOCK_EMPLOYEE_CD_NAME)', field: "LOCK_EMPLOYEE_CD_NAME", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.LOCK_EMPLOYEE_CD", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("LOCK_EMPLOYEE_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("LOCK_EMPLOYEE_CD").DispFlag" },
            { id: "@ColKey.InventoryInquiry.PHYINV_STATUS", name: '@Message.getMessage(CommonLabelInventoryInquiry.LABEL_JITSUTANA_STATUS)', field: "PHYINV_STATUS", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.PHYINV_STATUS", 110), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_STATUS_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_STATUS_NAME").DispFlag", formatter: PhyinvStatusFormatter},
            { id: "@ColKey.InventoryInquiry.LOC_L_CD", name: '@Message.getMessage(CommonLabel.LABEL_LOC_L_COM_NAME)', field: "LOC_L_CD_NAME", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.LOC_L_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_L_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_L_CODE_NAME").DispFlag" },
            { id: "@ColKey.InventoryInquiry.LOC_M_CD", name: '@Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_NAME)', field: "LOC_M_CD_NAME", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.LOC_M_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_M_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_M_CODE_NAME").DispFlag" },
            { id: "@ColKey.InventoryInquiry.LOC_S_CD", name: '@Message.getMessage(CommonLabel.LABEL_LOC_S_PLA_NAME)', field: "LOC_S_CD_NAME", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.LOC_S_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_S_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("BOOK_LOCATION_S_CODE_NAME").DispFlag" },
            { id: "@ColKey.InventoryInquiry.PHYINV_LOC_L_CD", name: '@Message.getMessage(CommonLabelInventoryInquiry.LABEL_PHYINV_LOC_L_CD_NAME)', field: "PHYINV_LOC_L_CD_NAME", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.PHYINV_LOC_L_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_L_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_L_CODE_NAME").DispFlag" },
            { id: "@ColKey.InventoryInquiry.PHYINV_LOC_M_CD", name: '@Message.getMessage(CommonLabelInventoryInquiry.LABEL_PHYINV_LOC_M_CD_NAME)', field: "PHYINV_LOC_M_CD_NAME", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.PHYINV_LOC_M_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_M_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_M_CODE_NAME").DispFlag" },
            { id: "@ColKey.InventoryInquiry.PHYINV_LOC_S_CD", name: '@Message.getMessage(CommonLabelInventoryInquiry.LABEL_PHYINV_LOC_S_CD_NAME)', field: "PHYINV_LOC_S_CD_NAME", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.PHYINV_LOC_S_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_S_CODE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_LOCATION_S_CODE_NAME").DispFlag" },
            { id: "@ColKey.InventoryInquiry.CAT_L_CD", name: '@Message.getMessage(CommonLabel.LABEL_CAT_L_CD_NAME)', field: "CAT_L_CD_NAME", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.CAT_L_CD", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CAT_L_CD").DispFlag" },
            { id: "@ColKey.InventoryInquiry.IS_TANGIBLE", name: '@Message.getMessage(CommonLabel.LABEL_IS_TANGIBLE)', field: "IS_TANGIBLE", formatter: TangibleFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.IS_TANGIBLE", 90), sortOrder: "@Model.FirstOrDefault.GridInfo("IS_TANGIBLE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("IS_TANGIBLE").DispFlag" },
            { id: "@ColKey.InventoryInquiry.ASSET_NO", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_NO)', field: "ASSET_NO", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.ASSET_NO", 110), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NO").DispFlag" },
            { id: "@ColKey.InventoryInquiry.SUB_ASSET_NO", name: '@Message.getMessage(CommonLabel.LABEL_SUB_ASSET_NO)', field: "SUB_ASSET_NO", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.SUB_ASSET_NO", 110), sortOrder: "@Model.FirstOrDefault.GridInfo("SUB_ASSET_NO").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("SUB_ASSET_NO").DispFlag" },
            { id: "@ColKey.InventoryInquiry.ASSET_NAME", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)', field: "ASSET_NAME", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.ASSET_NAME", 135), sortOrder: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ASSET_NAME").DispFlag" },
            { id: "@ColKey.InventoryInquiry.PHYINV_TS", name: '@Message.getMessage(CommonLabelInventoryInquiry.LABEL_PHYINV_DATE)', field: "PHYINV_TS", formatter: PhyinvTsFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.PHYINV_TS", 135), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_DATE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_DATE").DispFlag" },
            { id: "@ColKey.InventoryInquiry.PHYINV_EMPLOYEE_CD", name: '@Message.getMessage(CommonLabelInventoryInquiry.LABEL_PHYINV_PERSON_NAME)', field: "PHYINV_EMPLOYEE_CD", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.PHYINV_EMPLOYEE_CD", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_PERSON_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_PERSON_NAME").DispFlag" },
            //[CC431] ADD Start 20261002
            { id: "@ColKey.InventoryInquiry.PHYINV_TOOL_DIV", name: '@Message.getMessage(CommonLabel.LABEL_INV_TOOL_DIV)', field: "PHYINV_TOOL_DIV", formatter: PhyinvToolDivFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.PHYINV_TOOL_DIV", 100), sortOrder: "@Model.FirstOrDefault.GridInfo("PHYINV_TOOL_DIV").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("PHYINV_TOOL_DIV").DispFlag" },
            //[CC431] ADD End 20261002
            { id: "@ColKey.InventoryInquiry.FREE_ITEM6", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6)', field: "FREE_ITEM6", formatter: FreeItem6Formatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM6", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM6").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM1", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM1)', field: "FREE_ITEM1", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM1", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM1").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM2", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)', field: "FREE_ITEM2", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM2", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM2").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM3", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM3)', field: "FREE_ITEM3", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM3", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM3").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM4", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM4)', field: "FREE_ITEM4", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM4", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM4").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM5", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM5)', field: "FREE_ITEM5", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM5", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM5").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM7", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM7)', field: "FREE_ITEM7", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM7", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM7").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM8", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM8)', field: "FREE_ITEM8", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM8", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM8").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM9", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM9)', field: "FREE_ITEM9", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM9", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM9").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM10", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM10)', field: "FREE_ITEM10", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM10", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM10").DispFlag" },
            //[CC431] UPD Start 20261002
            { id: "@ColKey.InventoryInquiry.FREE_ITEM11", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM11)', field: "FREE_ITEM11", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM11", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM11").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM11").DispFlag", formatter: CommaFormatter, cssClass: "cell-right" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM12", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM12)', field: "FREE_ITEM12", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM12", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM12").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM12").DispFlag", formatter: CommaFormatter, cssClass: "cell-right" },
            //[CC431] UPD End 20261002
            { id: "@ColKey.InventoryInquiry.FREE_ITEM13", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM13)', field: "FREE_ITEM13", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM13", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM13").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM13").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM14", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM14)', field: "FREE_ITEM14", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM14", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM14").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM14").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM15", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM15)', field: "FREE_ITEM15", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM15", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM15").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM15").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM16", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM16)', field: "FREE_ITEM16", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM16", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM16").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM16").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM17", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM17)', field: "FREE_ITEM17", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM17", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM17").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM17").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM18", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)', field: "FREE_ITEM18", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM18", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM18").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM18").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM19", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)', field: "FREE_ITEM19", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM19", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM19").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM19").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM20", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM20)', field: "FREE_ITEM20", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM20", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM20").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM20").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM21", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM21)', field: "FREE_ITEM21", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM21", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM21").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM21").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM22", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM22)', field: "FREE_ITEM22", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM22", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM22").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM22").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM23", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM23)', field: "FREE_ITEM23", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM23", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM23").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM23").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM24", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM24)', field: "FREE_ITEM24", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM24", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM24").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM24").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM25", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM25)', field: "FREE_ITEM25", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM25", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM25").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM25").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM26", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM26)', field: "FREE_ITEM26", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM26", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM26").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM26").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM27", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM27)', field: "FREE_ITEM27", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM27", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM27").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM27").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM28", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM28)', field: "FREE_ITEM28", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM28", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM28").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM28").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM29", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM29)', field: "FREE_ITEM29", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM29", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM29").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM29").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM30", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM30)', field: "FREE_ITEM30", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM30", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM30").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM30").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM31", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM31)', field: "FREE_ITEM31", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM31", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM31").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM31").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM32", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM32)', field: "FREE_ITEM32", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM32", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM32").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM32").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM33", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM33)', field: "FREE_ITEM33", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM33", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM33").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM33").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM34", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM34)', field: "FREE_ITEM34", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM34", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM34").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM34").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM35", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM35)', field: "FREE_ITEM35", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM35", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM35").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM35").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM36", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM36)', field: "FREE_ITEM36", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM36", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM36").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM36").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM37", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM37)', field: "FREE_ITEM37", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM37", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM37").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM37").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM38", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM38)', field: "FREE_ITEM38", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM38", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM38").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM38").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM39", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM39)', field: "FREE_ITEM39", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM39", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM39").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM39").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM40", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM40)', field: "FREE_ITEM40", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM40", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM40").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM40").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM41", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM41)', field: "FREE_ITEM41", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM41", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM41").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM41").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM42", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM42)', field: "FREE_ITEM42", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM42", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM42").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM42").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM43", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM43)', field: "FREE_ITEM43", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM43", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM43").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM43").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM44", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM44)', field: "FREE_ITEM44", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM44", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM44").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM44").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM45", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM45)', field: "FREE_ITEM45", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM45", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM45").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM45").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM46", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM46)', field: "FREE_ITEM46", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM46", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM46").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM46").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM47", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM47)', field: "FREE_ITEM47", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM47", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM47").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM47").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM48", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM48)', field: "FREE_ITEM48", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM48", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM48").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM48").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM49", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM49)', field: "FREE_ITEM49", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM49", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM49").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM49").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM50", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM50)', field: "FREE_ITEM50", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM50", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM50").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM50").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM51", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM51)', field: "FREE_ITEM51", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM51", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM51").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM51").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM52", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM52)', field: "FREE_ITEM52", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM52", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM52").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM52").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM53", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM53)', field: "FREE_ITEM53", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM53", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM53").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM53").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM54", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM54)', field: "FREE_ITEM54", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM54", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM54").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM54").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM55", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM55)', field: "FREE_ITEM55", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM55", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM55").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM55").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM56", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM56)', field: "FREE_ITEM56", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM56", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM56").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM56").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM57", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM57)', field: "FREE_ITEM57", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM57", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM57").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM57").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM58", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM58)', field: "FREE_ITEM58", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM58", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM58").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM58").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM59", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM59)', field: "FREE_ITEM59", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM59", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM59").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM59").DispFlag" },
            { id: "@ColKey.InventoryInquiry.FREE_ITEM60", name: '@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM60)', field: "FREE_ITEM60", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.FREE_ITEM60", 180), sortOrder: "@Model.FirstOrDefault.GridInfo("FREE_ITEM60").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("FREE_ITEM60").DispFlag" },
            { id: "@ColKey.InventoryInquiry.TAG_LINK_STATUS", name: "@Message.getMessage(CommonLabel.LABEL_TAG_LINK_STATUS)", field: "TAG_LINK_STATUS", width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.TAG_LINK_STATUS", 120), sortOrder: "@Model.FirstOrDefault.GridInfo("TAG_LINK_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("TAG_LINK_STATUS").DispFlag", formatter: TagLinkStatusFormatter},
            //[CC431] ADD Start 20261002
            { id: "@ColKey.InventoryInquiry.ENT_EMPLOYEE", name: '@Message.getMessage(CommonLabel.LABEL_INPUT_EMPLOYEE_CD_NAME)', field: "ENT_EMPLOYEE_CD", formatter: EntEmployeeFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.ENT_EMPLOYEE", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("ENT_EMPLOYEE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("ENT_EMPLOYEE").DispFlag" },
            //[CC431] ADD End 20261002
            { id: "@ColKey.InventoryInquiry.OPE_STATUS", name: '@Message.getMessage(CommonLabel.CHANGED_OPE_STATUS)', field: "OPE_STATUS", editor: Slick.Editors.Select, options: @Html.Raw(ViewBag.OpeOptions), formatter: OpeStatusFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.OPE_STATUS", 210), sortOrder: "@Model.FirstOrDefault.GridInfo("OPE_STATUS").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("OPE_STATUS").DispFlag" },
            { id: "@ColKey.InventoryInquiry.DIVERSION_REMOVE", name: '@Message.getMessage(CommonLabel.LABEL_DIVERSION_OR_REMOVE)', field: "DIVERSION_REMOVE", editor: Slick.Editors.Select, options: @Html.Raw(ViewBag.DiversionRemoveOptions), formatter: DiversionRemoveFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.DIVERSION_REMOVE", 90), sortOrder: "@Model.FirstOrDefault.GridInfo("DIVERSION_REMOVE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("DIVERSION_REMOVE").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMOVE_DATE", name: '@Message.getMessage(CommonLabel.LABEL_REMOVE_DATE)', field: "REMOVE_DATE", editor: MonthEditor,width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMOVE_DATE", 110), sortOrder: "@Model.FirstOrDefault.GridInfo("REMOVE_DATE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMOVE_DATE").DispFlag", formatter: RemoveDateFormatter },
            { id: "@ColKey.InventoryInquiry.REMOVE_REASON", name: '@Message.getMessage(CommonLabel.LABEL_REMOVE_REASON)', field: "REMOVE_REASON", editor: Slick.Editors.Select, options: @Html.Raw(ViewBag.RemoveReasonOptions), formatter: RemoveReasonFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMOVE_REASON", 270), sortOrder: "@Model.FirstOrDefault.GridInfo("REMOVE_REASON").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMOVE_REASON").DispFlag"},
            { id: "@ColKey.InventoryInquiry.REMOVE_CONTENT", name: '@Message.getMessage(CommonLabel.LABEL_REMOVE_CONTENT)', field: "REMOVE_CONTENT", editor: MyCustomEditor, maxLength: 255, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMOVE_CONTENT", 200), sortOrder: "@Model.FirstOrDefault.GridInfo("REMOVE_CONTENT").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMOVE_CONTENT").DispFlag", formatter: RemoveContentFormatter },
            { id: "@ColKey.InventoryInquiry.CHANGE_LOC_M_CD", name: '@Message.getMessage(CommonLabel.LABEL_CHANGE_LOC_M_CD)', field: "CHANGE_LOC_M_CD", editor: LocationSelect2Editor,formatter: ChangeLocMFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.CHANGE_LOC_M_CD", 130), sortOrder: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_M_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_M_CD").DispFlag" },
            { id: "@ColKey.InventoryInquiry.CHANGE_LOC_M_NAME", name: '@Message.getMessage(CommonLabel.LABEL_CHANGE_LOC_M_NAME)', field: "CHANGE_LOC_M_NAME", formatter: ChangeLocMNameFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.CHANGE_LOC_M_NAME", 130), sortOrder: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_M_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_M_NAME").DispFlag" },
            { id: "@ColKey.InventoryInquiry.CHANGE_LOC_S_CD", name: '@Message.getMessage(CommonLabel.LABEL_CHANGE_LOC_S_CD)', field: "CHANGE_LOC_S_CD", editor: LocationSelect2Editor,formatter: ChangeLocSFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.CHANGE_LOC_S_CD", 170), sortOrder: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_S_CD").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_S_CD").DispFlag" },
            { id: "@ColKey.InventoryInquiry.CHANGE_LOC_S_NAME", name: '@Message.getMessage(CommonLabel.LABEL_CHANGE_LOC_S_NAME)', field: "CHANGE_LOC_S_NAME", formatter: ChangeLocSNameFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.CHANGE_LOC_S_NAME", 130), sortOrder: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_S_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CHANGE_LOC_S_NAME").DispFlag" },
            { id: "@ColKey.InventoryInquiry.CHANGE_EMPLOYEE", name: '@Message.getMessage(CommonLabel.LABEL_CHANGE_EMPLOYEE_CD)', field: "CHANGE_EMPLOYEE", editor: MyHalfWidthEditor, maxLength: 15, formatter: ChangeEmployeeFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.CHANGE_EMPLOYEE", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("CHANGE_EMPLOYEE").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CHANGE_EMPLOYEE").DispFlag" },
            { id: "@ColKey.InventoryInquiry.CHANGE_EMPLOYEE_NAME", name: '@Message.getMessage(CommonLabel.LABEL_CHANGE_EMPLOYEE_NAME)', field: "CHANGE_EMPLOYEE_NAME", formatter: ChangeEmployeeNameFormatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.CHANGE_EMPLOYEE_NAME", 150), sortOrder: "@Model.FirstOrDefault.GridInfo("CHANGE_EMPLOYEE_NAME").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("CHANGE_EMPLOYEE_NAME").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMARK1", name: '@Message.getMessage(CommonLabel.LABEL_REMARK1)', field: "REMARK1", editor: MyCustomEditor, maxLength: 255, formatter: REMARK1Formatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMARK1", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK1").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK1").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMARK2", name: '@Message.getMessage(CommonLabel.LABEL_REMARK2)', field: "REMARK2", editor: MyCustomEditor, maxLength: 255, formatter: REMARK2Formatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMARK2", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK2").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK2").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMARK3", name: '@Message.getMessage(CommonLabel.LABEL_REMARK3)', field: "REMARK3", editor: MyCustomEditor, maxLength: 255, formatter: REMARK3Formatter,  width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMARK3", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK3").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK3").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMARK4", name: '@Message.getMessage(CommonLabel.LABEL_REMARK4)', field: "REMARK4", editor: MyCustomEditor, maxLength: 255, formatter: REMARK4Formatter,  width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMARK4", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK4").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK4").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMARK5", name: '@Message.getMessage(CommonLabel.LABEL_REMARK5)', field: "REMARK5", editor: MyCustomEditor, maxLength: 255, formatter: REMARK5Formatter,  width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMARK5", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK5").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK5").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMARK6", name: '@Message.getMessage(CommonLabel.LABEL_REMARK6)', field: "REMARK6", editor: MyCustomEditor, maxLength: 255, formatter: REMARK6Formatter,  width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMARK6", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK6").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK6").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMARK7", name: '@Message.getMessage(CommonLabel.LABEL_REMARK7)', field: "REMARK7", editor: MyCustomEditor, maxLength: 255, formatter: REMARK7Formatter,  width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMARK7", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK7").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK7").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMARK8", name: '@Message.getMessage(CommonLabel.LABEL_REMARK8)', field: "REMARK8", editor: MyCustomEditor, maxLength: 255, formatter: REMARK8Formatter,  width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMARK8", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK8").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK8").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMARK9", name: '@Message.getMessage(CommonLabel.LABEL_REMARK9)', field: "REMARK9", editor: MyCustomEditor, maxLength: 255, formatter: REMARK9Formatter,  width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMARK9", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK9").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK9").DispFlag" },
            { id: "@ColKey.InventoryInquiry.REMARK10", name: '@Message.getMessage(CommonLabel.LABEL_REMARK10)', field: "REMARK10", editor: MyCustomEditor, maxLength: 255, formatter: REMARK10Formatter, width: getColumnWidth("@DispId.INVENTORY_INQUIRY", "@ColKey.InventoryInquiry.REMARK10", 160), sortOrder: "@Model.FirstOrDefault.GridInfo("REMARK10").DispOrder", visible: "@Model.FirstOrDefault.GridInfo("REMARK10").DispFlag" }

        ];

        // Gridのオプション機能の設定
        let options = {
            enableCellNavigation: true,             //グリッド内のセル間移動可否
            enableColumnReorder: false,             //列のドラッグ&ドロップでの順序入れ替え可否
            multiColumnSort: false,                 //複数列のソート可否
            autoHeight: false,                      //Gridの高さの自動調整可否（ヘッダー固定のためfalse。true にすると editor も正しく動作しない）
            editable: true,                        //セルの編集可否
            autoCommitEdit: false,                  //セル編集の自動コミット可否
        };

        // 安全に使用できるようにJSON文字列をエンコード
        let jsonData = JSON.parse('@Html.Raw(HttpUtility.JavaScriptStringEncode(Newtonsoft.Json.JsonConvert.SerializeObject(Model))) ');

        // 選択状態を復元
        let lstCheckedKey = @Html.Raw(jsonCheckKeys);
        if (lstCheckedKey.length === 0) {
            //選択状態がない場合は全選択ボタンのキャプションを全選択にする
            var btn = document.getElementById("btnSelectAll");
            btn.value ="@Message.getMessage(CommonLabel.BUTTON_ALL_ON)";
        }
        for (let i = 0; i < lstCheckedKey.length; i++) {
            for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                if (lstCheckedKey[i] === jsonData[grid_i].ASSET_NO) {
                    jsonData[grid_i].DISP_FLAG = lstCheckedKey[i]
                    break;
                }
            }
        }

        //編集状況を復元
        @*var editedContain = JSON.parse('@Html.Raw(jsonEditedContain)');
        jsonData = restore_editedlist(jsonData, editedContain);*@

        // 固定列を設定
        let fixCol = Number("@Model.FirstOrDefault.FixColCnt");
        if (fixCol > 0) {
            fixCol = fixCol + 1      //選択列を加算
        }

        //転用or除却の値で入力不可項目をする
        {
            var DiversionRemoveDataList = @Html.Raw(ViewBag.DiversionRemoveDataList);

            for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                var DiversionRemoveData = DiversionRemoveDataList.find(f => f.Key1 == jsonData[grid_i].DIVERSION_REMOVE);
                if (!(DiversionRemoveData != null && DiversionRemoveData.DATA2 == "Y")) {
                    jsonData[grid_i].DISABLE_REMOVE_DATE = true;
                    jsonData[grid_i].DISABLE_REMOVE_REASON = true;
                }
            }
        }

        //除却理由の値で入力不可項目を設定する
        {
            //[CC431] UPD Start 20261002
            // 除却内容は、除却理由が空白以外の場合に入力可
            for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
                if (convertToEmptyIfNull(jsonData[grid_i].REMOVE_REASON) == "") {
                    jsonData[grid_i].DISABLE_REMOVE_CONTENT = true;
                }
            }
            //[CC431] UPD End 20261002
        }

        let data = [];

        // Grid表示判定
        if ("@ViewBag.GridDisp" === "@ValidFlg.Cd.Valid" && "@Model.FirstOrDefault.TotalCount" > 0) {
            grid = initializeGrid("ListGrids", jsonData, columns, options, "@DispId.INVENTORY_INQUIRY", fixCol);
        } else {
            grid = initializeGrid("ListGrids", data, columns, options, fixCol);
        }

        // ヘッダー固定（グリッド内でスクロールさせ、ヘッダーを常に表示する）
        enableFixedHeader(grid, "ListGrids");


        {
            var opts = @Html.Raw(If(ViewBag.DiversionRemoveOptions Is Nothing, "{}", ViewBag.DiversionRemoveOptions));
            var $sel = $("#DIVERSION_REMOVE");
            $sel.empty().append($("<option>").val("").text(""));
            $.each(opts, function (cd, name) {
                $("<option>", { value: cd, text: name }).appendTo($sel);
            });

            $("#DIVERSION_REMOVE").on("change", function () {
                var selectedValue = $(this).val();

                var DiversionRemoveDataList = @Html.Raw(ViewBag.DiversionRemoveDataList);
                var DiversionRemoveData = DiversionRemoveDataList.find(f => f.Key1 == selectedValue);
                if (!(DiversionRemoveData != null && DiversionRemoveData.DATA2 == "Y")) {
                    $("#REMOVE_DATE").prop("disabled", true);
                    $("#REMOVE_DATE").val("");
                    $("#REMOVE_DATE").addClass("inventory-disable-input");

                    $("#REMOVE_REASON").prop("disabled", true);
                    $("#REMOVE_REASON").val("");
                    $("#REMOVE_REASON").addClass("inventory-disable-input");

                    $("#REMOVE_CONTENT").prop("disabled", true);
                    $("#REMOVE_CONTENT").val("");
                    $("#REMOVE_CONTENT").addClass("inventory-disable-input");
                }
                else
                {
                    $("#REMOVE_DATE").prop("disabled", false);
                    $("#REMOVE_DATE").removeClass("inventory-disable-input");
                    $("#REMOVE_REASON").prop("disabled", false);
                    $("#REMOVE_REASON").removeClass("inventory-disable-input");
                }
                // 除却理由の×（クリアボタン）の表示を更新する（値のクリア・入力可否の変更は change を発生させないため）
                refreshClearable("#REMOVE_REASON");
            });

        }
        {
            var opts = @Html.Raw(If(ViewBag.OpeOptions Is Nothing, "{}", ViewBag.OpeOptions));
            var $sel = $("#OPE_STATUS");
            $sel.empty().append($("<option>").val("").text(""));
            $.each(opts, function (cd, name) {
                $("<option>", { value: cd, text: name }).appendTo($sel);
            });
        }
        {
            var opts = @Html.Raw(If(ViewBag.RemoveReasonOptions Is Nothing, "{}", ViewBag.RemoveReasonOptions));
            var $sel = $("#REMOVE_REASON");
            $sel.empty().append($("<option>").val("").text(""));
            $.each(opts, function (cd, name) {
                $("<option>", { value: cd, text: name }).appendTo($sel);
            });

            $("#REMOVE_REASON").on("change", function () {
                var selectedValue = $(this).val();
                //[CC431] UPD Start 20261008
                // 除却内容は、除却理由が空白以外の場合に入力可。空白に戻したら除却内容をクリアする（一覧と同じ）
                if (convertToEmptyIfNull(selectedValue) == "") {
                //[CC431] UPD End 20261008
                    $("#REMOVE_CONTENT").prop("disabled", true);
                    $("#REMOVE_CONTENT").val("");
                    $("#REMOVE_CONTENT").addClass("inventory-disable-input");
                } else {
                    $("#REMOVE_CONTENT").prop("disabled", false);
                    $("#REMOVE_CONTENT").removeClass("inventory-disable-input");
                }
            });
        }

        {
            //検索項目
            //※中区分・小区分、変更後中区分・変更後小区分は複数選択。小区分の選択肢はサーバー側で選択中の中区分から作成済みのため、
            //  選択値をそのまま設定する（onchange は発生させない）
            //[CC431] UPD Start 20261007
            //※場所から選ぶ OFF：中区分が選択されていれば小区分を活性。ON：小区分が選択されていれば中区分を活性
            //  （選択肢はサーバー側で ON／OFF に合わせて作成済み）
            var searchLocM = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(If(Model.FirstOrDefault.SearchLocM, New String() {})));
            var searchLocS = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(If(Model.FirstOrDefault.SearchLocS, New String() {})));
            if (!isSearchLocReverse("Loc") && searchLocM.length > 0) {
                setMultiSelectValues("#SearchLocM", searchLocM);
                $("#SearchLocS").prop("disabled", false);
                setMultiSelectValues("#SearchLocS", searchLocS);
            } else if (isSearchLocReverse("Loc") && searchLocS.length > 0) {
                setMultiSelectValues("#SearchLocS", searchLocS);
                $("#SearchLocM").prop("disabled", false);
                setMultiSelectValues("#SearchLocM", searchLocM);
            }

            var searchChangeLocM = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(If(Model.FirstOrDefault.SearchChangeLocMCd, New String() {})));
            var searchChangeLocS = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(If(Model.FirstOrDefault.SearchChangeLocSCd, New String() {})));
            if (!isSearchLocReverse("ChangeLoc") && searchChangeLocM.length > 0) {
                setMultiSelectValues("#SearchChangeLocMCd", searchChangeLocM);
                $("#SearchChangeLocSCd").prop("disabled", false);
                setMultiSelectValues("#SearchChangeLocSCd", searchChangeLocS);
            } else if (isSearchLocReverse("ChangeLoc") && searchChangeLocS.length > 0) {
                setMultiSelectValues("#SearchChangeLocSCd", searchChangeLocS);
                $("#SearchChangeLocMCd").prop("disabled", false);
                setMultiSelectValues("#SearchChangeLocMCd", searchChangeLocM);
            }
            //[CC431] UPD End 20261007


            var searchRemoveReason = '@Model.FirstOrDefault.SearchRemoveReason';
            if (searchRemoveReason && searchRemoveReason.trim() !== "") {
                $("#SearchRemoveReason").val(searchRemoveReason);
            }
            var searchOpeStatus = '@Model.FirstOrDefault.SearchOpeStatus';
            if (searchOpeStatus && searchOpeStatus.trim() !== "") {
                $("#SearchOpeStatus").val(searchOpeStatus);
            }
            var searchDiversionRemove = '@Model.FirstOrDefault.SearchDiversionRemove';
            if (searchDiversionRemove && searchDiversionRemove.trim() !== "") {
                $("#SearchDiversionRemove").val(searchDiversionRemove);
            }

            //値を設定したプルダウンの×（クリアボタン）の表示を更新する
            refreshClearable("#SearchRemoveReason, #SearchOpeStatus, #SearchDiversionRemove");

        }

        setTimeout(() => {
            isEdit = true;
        }, 1);


        //非活性にしているのに、テキストエディタ領域外のセルをクリックしたら開くことができるので、
        //エディタを開けないように制御する。
        grid.onBeforeEditCell.subscribe(function (e, args) {
            var item = args.item;
            var column = args.column;

            if (item["DISABLE_" + column.field] === true) {
                return false; // ← SlickGrid がエディタを起動しなくなる
            }

            return true;
        });

    });

    let locationMList = [];
    let locationSList = [];
    let employeeList = [];
    let isEdit = false;

    function TangibleFormatter(row, cell, value, columnDef, dataContext) {
        if (value == "Y") {
            return "<div>@Message.getMessage(CommonLabel.LABEL_TANGIBLE_ASSET_NAME)</div>"
        } else if (value == "N") {
            return "<div>@Message.getMessage(CommonLabel.LABEL_INTANGIBLE_ASSET_NAME)</div>"
        }
    }

    function PhyinvTsFormatter(row, cell, value, columnDef, dataContext) {

        if (value != null && String(value).length == 14) {
            var str = String(value);
            const year = str.substring(0, 4);
            const month = str.substring(4, 6);
            const day = str.substring(6, 8);
            const hour = str.substring(8, 10);
            const min = str.substring(10, 12);
            const sec = str.substring(12, 14);

            const date = year + "/" + month + "/" + day + " " + hour + ":" + min + ":" + sec;
            return "<div>" + date + "</div>";
        } else {
            if (value != null || value != undefined) {
                return "<div>" + value + "</div>";
            }
        }
    }

    function FreeItem6Formatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {

            var employee = employeeList.find(e => e.EMPLOYEE_CD == value);
            if (employee) {
                return "<div>" + employee.EMPLOYEE_CD + " " + employee.EMPLOYEE_NAME + "</div>";
            }

        }
    }

    //[CC431] ADD Start 20261002
    // 棚卸実施端末（H:HT、W:Web）。想定外の値はそのまま表示する
    function PhyinvToolDivFormatter(row, cell, value, columnDef, dataContext) {
        if (value == "@ProcessToolDiv.HT") {
            return "<div>@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)</div>";
        } else if (value == "@ProcessToolDiv.Web") {
            return "<div>@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)</div>";
        }
        return "<div>" + convertToEmptyIfNull(value) + "</div>";
    }

    // 入力作業者コード/名（名称は社員M）。社員Mにないコードもコードは表示する
    function EntEmployeeFormatter(row, cell, value, columnDef, dataContext) {
        if (convertToEmptyIfNull(value) == "") {
            return "";
        }
        return "<div>" + value + " " + convertToEmptyIfNull(dataContext.ENT_EMPLOYEE_NAME) + "</div>";
    }

    // カンマ区切り（取得金額、当月末簿価）。数値でない値はそのまま表示する（AssetList と同じ）
    function CommaFormatter(row, cell, value, columnDef, dataContext) {
        var sValue = convertToEmptyIfNull(value);
        if (sValue == '') {
            return '';
        }
        var nValue = Number(sValue);
        if (isNaN(nValue)) {
            return sValue;
        }
        var aParts = sValue.split('.');
        var nDecimals = (aParts.length > 1) ? aParts[1].length : 0;
        return nValue.toLocaleString('ja-JP', { minimumFractionDigits: nDecimals, maximumFractionDigits: nDecimals });
    }
    //[CC431] ADD End 20261002

    //選択用formatter
    function InventoryInquiryCheckboxFormatter(row, cell, value, columnDef, dataContext) {
        let key = convertToEmptyIfNull(dataContext.ASSET_NO); // + ',' + convertToEmptyIfNull(dataContext.PHYINV_STATUS)
        let isKeyInclude = (dataContext[columnDef.field] === key);

        return '<div style="text-align:center; height:100%;"> ' +
            '<input id="listcheck" type="checkbox" class="slick-checkbox" name="checkEdit" ' +
            'value="' + key + '" '+
            (isKeyInclude ? 'checked="checked" ' : '') +
            'onchange="check_onclick(this)" ' +
            '/>' +
            '</div>';
    }

    function RemoveContentFormatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMOVE_CONTENT");
    }

    function RemoveDateFormatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMOVE_DATE");
    }

    function REMARK1Formatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMARK1");
    }

    function REMARK2Formatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMARK2");
    }

    function REMARK3Formatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMARK3");
    }

    function REMARK4Formatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMARK4");
    }

    function REMARK5Formatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMARK5");
    }

    function REMARK6Formatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMARK6");
    }

    function REMARK7Formatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMARK7");
    }

    function REMARK8Formatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMARK8");
    }

    function REMARK9Formatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMARK9");
    }

    function REMARK10Formatter(row, cell, value, columnDef, dataContext) {
        return TextEditFormatter(row, cell, value, columnDef, dataContext, "REMARK10");
    }

    function ChangeLocMFormatter(row, cell, value, columnDef, dataContext) {
        var updateMethod = (inputId, nameId) => {
            // 入力値変更時に辞書を参照して名称を更新
            const input = document.getElementById(inputId);
            const nameSpan = document.getElementById(nameId);
            if (input && nameSpan) {
                const code = input.value.trim();
                var loc = locationMList.find(l => l.LOC_M_CD == code);
                if (loc != null && loc != undefined) {
                    var splits = loc.LOC_NAME.split("@CommonConst.CodeNameDelimiter")
                    nameSpan.textContent = splits[1];
                } else {
                    nameSpan.textContent = "";
                }
            }
        };
        return CdToNameTextEditFormatter(row, cell, value, columnDef, dataContext, "CHANGE_LOC_M_CD", "CHANGE_LOC_M_NAME", updateMethod);
    }

    function ChangeLocSFormatter(row, cell, value, columnDef, dataContext) {
        var updateMethod = (inputId, nameId) => {
            // 入力値変更時に辞書を参照して名称を更新
            const input = document.getElementById(inputId);
            const nameSpan = document.getElementById(nameId);
            //[CC431] UPD Start 20261002
            // 変更後課コードが空の場合は、棚卸資産T.ロケ中区分コード配下の場所で名称を探す
            //[CC431] UPD Start 20261007 変更後課コードが空白だけの場合も空として扱う
            const mCode = (dataContext.CHANGE_LOC_M_CD || "").trim() || dataContext.LOC_M_CD;
            //[CC431] UPD End 20261007
            //[CC431] UPD End 20261002
            if (input && nameSpan) {
                const code = input.value.trim();
                var loc = locationSList.find(l => l.LOC_M_CD == mCode && l.LOC_S_CD == value);
                if (loc != null && loc != undefined) {
                    var splits = loc.LOC_NAME.split("@CommonConst.CodeNameDelimiter")
                    nameSpan.textContent = splits[1];
                } else {
                    nameSpan.textContent = "";
                }
            }
        };
        return CdToNameTextEditFormatter(row, cell, value, columnDef, dataContext, "CHANGE_LOC_S_CD", "CHANGE_LOC_S_NAME", updateMethod);
    }

    function ChangeEmployeeFormatter(row, cell, value, columnDef, dataContext) {
        var updateMethod = (inputId, nameId) => {
            // 入力値変更時に辞書を参照して名称を更新
            const input = document.getElementById(inputId);
            const nameSpan = document.getElementById(nameId);
            if (input && nameSpan) {
                const code = input.value.trim();
                var employee = employeeList.find(e => e.EMPLOYEE_CD == code);
                if (employee != null && employee != undefined) {
                    nameSpan.textContent = employee.EMPLOYEE_NAME;
                } else {
                    nameSpan.textContent = "";
                }
            }
        };
        return CdToNameTextEditFormatter(row, cell, value, columnDef, dataContext, "CHANGE_EMPLOYEE", "CHANGE_EMPLOYEE_NAME", updateMethod);
    }

    function CdToNameTextEditFormatter(row, cell, value, columnDef, dataContext, id, nameTagId, updateMethod) {
        let assetNo = dataContext.ASSET_NO;
        let disp = value || "";

        // 一意なIDを付与（行＋カラム名など）
        const inputId = `${id}_${assetNo}`;
        const nameId = `${nameTagId}_${assetNo}`; // 名称表示用ID

        setTimeout(() => {
            if (typeof updateMethod === "function") {
                updateMethod(inputId, nameId);
            }
        }, 0);

        var isEdited = false;
        if (dataContext["EDITED_" + id] != null) {
            isEdited = dataContext["EDITED_" + id];
            disableEditAssetButton();
        }

        var isErr = false;
        if (dataContext["ERROR_" + id] != null) {
            isErr = dataContext["ERROR_" + id];
            disableEditAssetButton();
        }

        if (isErr) {
            return `<input type="text" id="${inputId}" class="editable-cell" value="${disp}" style="width:100%;padding-right:2px; background-color: ${errorCellColor};" />`;
        } else if (isEdited) {
            return `<input type="text" id="${inputId}" class="editable-cell" value="${disp}" style="width:100%;padding-right:2px; background-color: ${changedCellColor};" />`;
        } else {
            return `<input type="text" id="${inputId}" class="editable-cell" value="${disp}" style="width:100%;padding-right:2px;" />`;
        }
    }

    function ChangeLocMNameFormatter(row, cell, value, columnDef, dataContext) {
        return NameFormatter(row, cell, value, columnDef, dataContext, "CHANGE_LOC_M_NAME",);
    }

    function ChangeLocSNameFormatter(row, cell, value, columnDef, dataContext) {
        return NameFormatter(row, cell, value, columnDef, dataContext, "CHANGE_LOC_S_NAME",);
    }

    function ChangeEmployeeNameFormatter(row, cell, value, columnDef, dataContext) {
        return NameFormatter(row, cell, value, columnDef, dataContext, "CHANGE_EMPLOYEE_NAME",);
    }

    function NameFormatter(row, cell, value, columnDef, dataContext, id) {
        let assetNo = dataContext.ASSET_NO;
        if (value == null || value == undefined) {
            return '<span id="' + id + '_' + assetNo + '"></span>';
        }
        return '<span id="' + id + '_' + assetNo + '">' + value + '</span>';
    }

    function TextEditFormatter(row, cell, value, columnDef, dataContext, id) {
        let assetNo = dataContext.ASSET_NO;
        var disp = "";
        if (value != null) {
            disp = value;
        }

        var isEdited = false;
        if (dataContext["EDITED_" + id] != null) {
            isEdited = dataContext["EDITED_" + id];
            disableEditAssetButton();
        }
        var isError = false;
        if (dataContext["ERROR_" + id] != null) {
            isError = dataContext["ERROR_" + id];
            disableEditAssetButton();
        }

        var isDisable = false;
        if (dataContext["DISABLE_" + id] != null) {
            isDisable = dataContext["DISABLE_" + id];
        }

        if (isError) {
            return '<input type="text" id="' + id + '_' + assetNo + '" name="' + id + '" class="editable-cell" value="' + disp + '"'
                + ' style="width:100%;padding-right:2px;background-color: ' + errorCellColor + '; " ' + (isDisable ? 'disabled' : '') + ' />';
        } else if (isEdited) {
            return '<input type="text" id="' + id + '_' + assetNo + '" name="' + id + '" class="editable-cell" value="' + disp + '"'
                + ' style="width:100%;padding-right:2px;background-color: ' + changedCellColor + '; " ' + (isDisable ? 'disabled' : '') + ' />';
        } else {
            return '<input type="text" id="' + id + '_' + assetNo + '" name="' + id + '" class="editable-cell" value="' + disp + '"'
                + '" style="width:100%;padding-right:2px;" ' + (isDisable ? 'disabled' : '') + ' />';
        }
    }

    function OpeStatusFormatter(row, cell, value, columnDef, dataContext) {
        return ComboTextEditFormatter(row, cell, value, columnDef, dataContext, "OPE_STATUS", );
    }

    function DiversionRemoveFormatter(row, cell, value, columnDef, dataContext) {
        return ComboTextEditFormatter(row, cell, value, columnDef, dataContext, "DIVERSION_REMOVE",);
    }

    function RemoveReasonFormatter(row, cell, value, columnDef, dataContext) {
        return ComboTextEditFormatter(row, cell, value, columnDef, dataContext, "REMOVE_REASON",);
    }

    function ComboTextEditFormatter(row, cell, value, columnDef, dataContext, id) {
        let assetNo = dataContext.ASSET_NO;
        let disp = "";
        if (columnDef.options[value] != undefined)
            disp = columnDef.options[value];

        var isEdited = false;
        if (dataContext["EDITED_" + id] != null) {
            isEdited = dataContext["EDITED_" + id];
            disableEditAssetButton();
        }
        var isErr = false;
        if (dataContext["ERROR_" + id] != null) {
            isErr = dataContext["ERROR_" + id];
            disableEditAssetButton();
        }
        var isDisable = false;
        if (dataContext["DISABLE_" + id] != null) {
            isDisable = dataContext["DISABLE_" + id];
        }

        if (isErr) {
            return '<input type="text" id="' + id + '_' + assetNo + '" name="' + id + '" class="editable-cell" value="' + disp + '"'
                + ' style="width:100%;padding-right:2px;background-color: ' + errorCellColor + '; " ' + (isDisable ? 'disabled' : '') + ' />';
        } else if (isEdited) {
            return '<input type="text" id="' + id + '_' + assetNo + '" name="' + id + '" class="editable-cell" value="' + disp + '"'
                + ' style="width:100%;padding-right:2px;background-color: ' + changedCellColor + '; " ' + (isDisable ? 'disabled' : '') + ' />';
        } else {
            return '<input type="text" id="' + id + '_' + assetNo + '" name="' + id + '" class="editable-cell" value="' + disp + '"'
                + ' style="width:100%;padding-right:2px; " ' + (isDisable ? 'disabled' : '') + ' />';
        }
    }

    // 資産状態用formatter
    function AssetStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        switch(String(dataContext.ASSET_STATUS)) {
            case "@ASSET_STATUS.Preparation":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_JUNBI)"
                break;
            case "@ASSET_STATUS.Storage":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_HOKAN)"
                break;
            case "@ASSET_STATUS.Lending":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_LEASE)"
                break;
            case "@ASSET_STATUS.Removed":
                status = "@Message.getMessage(CommonLabel.VALUE_ASSET_STATUS_RETIRE)"
                break;
            default :
                status = dataContext.ASSET_STATUS;
                break;
        }

        return status;
    }

    //実棚ステータス用formatter
    function PhyinvStatusFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            switch (dataContext.PHYINV_STATUS) {
                case 'E': return "<span style='color: red;'>現物なし</span>"
                case 'W': return "ロケ違い"
                case 'N': return "正常"
                defaut: return dataContext.PHYINV_STATUS
            }
        }
    }

    //帳簿ロケーション大区分コード/名用formatter
    function BookLocationLCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.LOC_L_CD + ' ' + value
        }
    }

    //帳簿ロケーション中区分コード/名用formatter
    function BookLocationMCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.LOC_M_CD + ' ' + value
        }
    }

    //帳簿ロケーション小区分コード/名用formatter
    function BookLocationSCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.LOC_S_CD + ' ' + value
        }
    }

    //実棚ロケーション大区分コード/名用formatter
    function PhyinvLocationLCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.PHYINV_LOC_L_CD + ' ' + value
        }
    }

    //実棚ロケーション中区分コード/名用formatter
    function PhyinvLocationMCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.PHYINV_LOC_M_CD + ' ' + value
        }
    }

    //実棚ロケーション小区分コード/名用formatter
    function PhyinvLocationSCodeNameFormatter(row, cell, value, columnDef, dataContext) {
        if (value != null) {
            return dataContext.PHYINV_LOC_S_CD + ' ' + value
        }
    }

    //実棚日時用formatter
    function PhyinvDateFormatter(row, cell, value, columnDef, dataContext) {
        let status = convertToEmptyIfNull(value).toString()
        if (status != '') {
            let yyyy = status.substr(0, 4)
            let mm = status.substr(4, 2)
            let dd = status.substr(6, 2)
            let hhmm = status.substr(8, 2) + ':' + status.substr(10, 2)
            return yyyy + '/' + mm + '/' + dd + ' ' + hhmm
        }
    }

    // タグ紐付け状態用formatter
    function TagLinkStatusFormatter(row, cell, value, columnDef, dataContext) {
        let status = "";

        if (String(dataContext.TAG_LINK_STATUS) === "0") {
            status = "@Message.getMessage(CommonLabel.VALUE_MI)"
        } else {
            status = "@Message.getMessage(CommonLabel.VALUE_SUMI)"
        }

        return status;
    }

    //現ロケに変更
    function btnChangeLocation_onclick() {
        // 選択行の資産番号を取得
        //var assetNo = [];
        //var phyinvStatus = [];
        //var grid = document.getElementById("inventory-detail");
        //var checkBoxes = grid.getElementsByTagName("INPUT");
        //for (var i = 0; i < checkBoxes.length; i++) {
        //    if (checkBoxes[i].checked) {
        //        var row = checkBoxes[i].parentNode.parentNode;
        //        assetNo.push(checkBoxes[i].value);
        //        phyinvStatus.push(row.cells[13].innerHTML.trim());
        //    }
        //}

        //チェックボックスの状態からキーを設定する
        setting_checklist();


        //if (assetNo.length == 0) { //テーブルの選択行がない場合
        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        @*// 選択データ
        // phyinvStatusにW以外を選択している場合はエラー
        document.getElementById("SelectAssetNo").value = "";
        for (var i = 0; i < assetNo.length; i++) {
            if (phyinvStatus[i] != "W") {
                // W以外を選択
                showErrMessage("@Message.getMessage(CommonErrorMsg.PHYINV_STATUS_CHECKBOX_NOTSELWARNING)")
                return false;
            }
        }
        document.getElementById("SelectAssetNo").value = assetNo;*@

        //確認メッセージ
        var req = showConfirm("@Message.getMessage(CommonLabelInventoryInquiry.INVENTORY_CHANGE_LOCATION_CONFIRM)", "@Url.Content("~/InventoryInquiry")/UpdateInventory");
    }

    //QR画面表示
    function btnShowQR_onclick() {
        // 選択行の資産番号を取得
        var assetNo = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked) {
                //--2019/12/17 miyaji update start
                var result = checkBoxes[i].value.split(',');
                assetNo.push(result[0]);
                //var row = checkBoxes[i].parentNode.parentNode;
                //assetNo.push(checkBoxes[i].value);
                //--2019/12/17 miyaji update end
            }
        }
        if (assetNo.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        } else if (assetNo.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)")
            return false;
        }
        @*//タイトルの設定
        document.getElementById("popup-title-qr").innerHTML = "@ViewBag.Title" + "@Message.getMessage(CommonLabel.BUTTON_SHOW_QR)";*@

        // 以下では、１度しか、実行しないので、ダメ
        //document.getElementById("qrview").src = '@Url.Content("~/AssetList")/GetQRData'
        //document.getElementById("qrview").src = '@Url.Action("GetQRData", New With {.sAssetNo = Model.FirstOrDefault.ASSET_NO})'

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {
            //表示するＱＲの資産番号を表示
            document.getElementById("QRAssetNo").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_NO)" + ":" + assetNo[0]
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/InventoryInquiry")/GetQRData',
                data: { sAssetNo: assetNo[0] },
                cache: false,
                success: function (result, dataType) {
                    this;
                    //取得したデータを表示する　（以下では、正常に表示できない）
                    //document.getElementById("qrview").src = result;
                    //alert(result);
                    document.getElementById("qrview").src = '@Url.Content("~/QRimages/")' + result;
                    //データが存在しない場合エラー
                    if (result == null) {
                        return;
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }

        // --2020/01/07 miyaji update start
        //チェックボックスの状態からキーを設定する
        setting_checklist();
        // --2020/01/07 miyaji update end

        //モーダルウインドウを開く
        ShowQRModalWindow()
    }

    //現物確認ボタンクリック処理
    function btnGenbutsuKakunin_onclick() {
        //チェックボックスの状態からキーを設定する
        setting_checklist();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        //[CC431] UPD Start 20261007
        //ロック・実棚ステータスを先に確認する（エラーなら変更処理も現物確認も行わない）
        var form = $('#form')[0];
        var formData = new FormData(form);

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/ValidateGenbutsuKakunin',
            data: formData,
            processData: false,
            contentType: false,
            cache: false,
            beforeSend: function () {
                //画面ブロックを表示
                showLoading();
            },
            success: function (result, dataType) {
                if (result == null || result == false) {
                    //NG
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)")
                    return;
                }

                if (!result.ok) {
                    showErrMessage(result.errMsg);
                    return;
                }

                var editedList = getEditedList();
                if (editedList.length == 0) {
                    //画面上に変更がない場合：確認メッセージ
                    var req = showConfirmShowLoading("@Message.getMessage(CommonLabelInventoryInquiry.MSG_GENBUTSU_KAKUNIN_CONFIRM)", "@Url.Content("~/InventoryInquiry")/UpdateGenbutsuKakunin");
                } else {
                    //画面上に変更がある場合：警告を出し、変更処理を行ってから現物確認を行う
                    showConfirmOkEvt("@Message.getMessage(CommonLabelInventoryInquiry.MSG_GENBUTSU_KAKUNIN_EDITED_CONFIRM)").then(() => {
                        btnChange_onclick(submitGenbutsuKakunin);
                    });
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            },
            complete: function () {
                //画面ブロックを解除
                hideLoading();
            }
        });
        //[CC431] UPD End 20261007
    }

    //[CC431] ADD Start 20261007
    //現物確認処理を実行する（変更処理の後に呼ぶ。ローディングは表示したまま）
    function submitGenbutsuKakunin() {
        showLoading();

        var befAction = $('#form').attr("action");
        $('#form').attr("action", "@Url.Content("~/InventoryInquiry")/UpdateGenbutsuKakunin");
        $('#form').submit();
        $('#form').attr("action", befAction);
    }
    //[CC431] ADD End 20261007

    //現物未確認ボタンクリック処理
    function btnGenbutsuMikakunin_onclick() {
        //チェックボックスの状態からキーを設定する
        setting_checklist();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        //確認メッセージ
        var req = showConfirm("@Message.getMessage(CommonLabelInventoryInquiry.MSG_GENBUTSU_MIKAKUNIN_CONFIRM)", "@Url.Content("~/InventoryInquiry")/UpdateGenbutsuMikakunin");
    }

    //ロックボタンクリック処理
    function btnLock_onclick() {

        setting_checklist();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        var form = $('#form')[0];
        var formData = new FormData(form);

        showConfirmOkEvt("@Message.getMessage(CommonLabelInventoryInquiry.MSG_LOCK_CONFIRM)").then(function () {
            //選択行のデータをロックする
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/InventoryInquiry")/Lock',
                data: formData,
                processData: false,
                contentType: false,
                cache: false,
                beforeSend: function () {
                    //画面ブロックを表示
                    showLoading();
                },
                success: function (result, dataType) {
                    if (result == null) {
                        //NG
                        showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)")
                        return;
                    }
                    if (!(result.err == null || result.err == "")) {
                        showErrMessage(result.err);
                        return;
                    }

                    showMessageAndFunc("@Message.getMessage(CommonLabelInventoryInquiry.MSG_LOCK_COMPLETE)", () => {
                        window.location.href = '@Url.Content("~/InventoryInquiry/Index")?actionname=@CommonConst.KEY_ON_CREATE_CLICK';
                    });

                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                },
                complete: function () {
                    //画面ブロックを解除
                    hideLoading();
                }
            });
        });

    }

    //ロック解除ボタンクリック処理
    function btnUnlock_onclick() {
        setting_checklist();

        if (document.getElementById("selectcount").innerHTML == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)")
            return false;
        }

        var form = $('#form')[0];
        var formData = new FormData(form);

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/UnlockCheck',
            data: formData,
            processData: false,
            contentType: false,
            cache: false,
            beforeSend: function () {
                //画面ブロックを表示
                showLoading();
            },
            success: function (result, dataType) {
                if (result == null || result == false) {
                    //NG
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)");
                    return;
                }
                if (!(result.err == null || result.err == "")) {
                    showErrMessage(result.err);
                    return;
                }

                if (result.ok == true && result.message != null) {
                    var ret = showConfirmOkEvt(result.message).then(() => {
                        Unlock(formData);
                    });
                    return;
                }

                Unlock(formData);

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            },
            complete: function () {
                //画面ブロックを解除
                hideLoading();
            }
        });

    }

    function Unlock(formData) {
        //選択行のデータをロック解除する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/Unlock',
            data: formData,
            processData: false,
            contentType: false,
            cache: false,
            success: function (result, dataType) {
                if (result == null || result == false) {
                    //NG
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)");
                    return;
                }
                if (!(result.err == null || result.err == "")) {
                    showErrMessage(result.err);
                    return;
                }

                showMessageAndFunc("@Message.getMessage(CommonLabelInventoryInquiry.MSG_UNLOCK_COMPLETE)", () => {
                    window.location.href = '@Url.Content("~/InventoryInquiry/Index")?actionname=@CommonConst.KEY_ON_CREATE_CLICK';
                });

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //変更ボタンクリック処理
    //[CC431] UPD Start 20261007 onChanged：変更処理が成功した後に呼ぶ関数（現物確認から呼ぶ場合。省略時は完了メッセージを表示）
    function btnChange_onclick(onChanged) {
        //続けて現物確認を行う場合はローディングを解除しない
        var continueLoading = false;
    //[CC431] UPD End 20261007

        var editedList = getEditedList();
        if (editedList.length == 0) {
            showErrMessage("@Message.getMessage(CommonLabelInventoryInquiry.LABEL_NO_EDITED)");
            return;
        }

        $("#EditJson").val(JSON.stringify(editedList));

        const gridData = grid.getData();
        for (const data of gridData) {
            data.ERROR_CHANGE_LOC_M_CD = false;
            data.ERROR_CHANGE_LOC_S_CD = false;
            //[CC431] UPD Start 20261009 セルの色を決める処理（CdToNameTextEditFormatter）が見る名前に合わせる
            data.ERROR_CHANGE_EMPLOYEE = false;
            //[CC431] UPD End 20261009
            data.ERROR_OPE_STATUS = false;
            data.ERROR_REMOVE_CONTENT = false;
            data.ERROR_REMOVE_DATE = false;
        }

        var form = $('#form')[0];
        var formData = new FormData(form);
        //[CC431] ADD Start 20261007
        //現物確認から呼ぶ場合は、続けて現物確認を行うためチェック状態を消さない
        if (typeof onChanged === "function") {
            formData.append("KeepChecked", "true");
        }
        //[CC431] ADD End 20261007

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/ChangeAsset',
            data: formData,
            processData: false,
            contentType: false,
            cache: false,
            beforeSend: function () {
                //画面ブロックを表示
                showLoading();
            },
            success: function (result, dataType) {
                if (result == null || result == false) {
                    //NG
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)")
                    return;
                }

                if (!result.ok) {
                    if (result.errAsset == "" || result.errAsset == null) {
                        showErrMessage(result.errMsg);
                        return;
                    }

                    const gridData = grid.getData();
                    var i = 0;
                    for (const data of gridData) {
                        if (data.ASSET_NO == result.errAsset) {
                            switch (result.errColumn) {
                                case "LOCK": break;
                                case "CHANGE_LOC_M_CD": data.ERROR_CHANGE_LOC_M_CD = true; break;
                                case "CHANGE_LOC_S_CD": data.ERROR_CHANGE_LOC_S_CD = true; break;
                                //[CC431] UPD Start 20261009 セルの色を決める処理（CdToNameTextEditFormatter）が見る名前に合わせる
                                case "CHANGE_EMPLOYEE": data.ERROR_CHANGE_EMPLOYEE = true; break;
                                //[CC431] UPD End 20261009
                                case "OPE_STATUS": data.ERROR_OPE_STATUS = true; break;
                                case "REMOVE_CONTENT": data.ERROR_REMOVE_CONTENT = true; break;
                                case "REMOVE_DATE": data.ERROR_REMOVE_DATE = true; break;
                            }

                            //gridセルの再描画
                            grid.invalidateRow(i);
                            grid.render();

                            const errDom = document.getElementById(result.errColumn + "_" + result.errAsset);
                            showErrMessage(result.errMsg, errDom);

                            break;
                        }
                        i++;
                    }

                    return;
                }

                //[CC431] ADD Start 20261007
                //現物確認から呼ばれた場合は完了メッセージを出さず、続けて現物確認を行う
                if (typeof onChanged === "function") {
                    continueLoading = true;
                    onChanged();
                    return;
                }
                //[CC431] ADD End 20261007

                showMessageAndFunc("@Message.getMessage(CommonLabelInventoryInquiry.MSG_CHANGE_COMPLETE)", () => {
                    window.location.href = '@Url.Content("~/InventoryInquiry/Index")?actionname=@CommonConst.KEY_ON_CREATE_CLICK';
                });

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            },
            complete: function () {
                //画面ブロックを解除
                //[CC431] UPD Start 20261007 続けて現物確認を行う場合は解除しない
                if (!continueLoading) {
                    hideLoading();
                }
                //[CC431] UPD End 20261007
            }
        });
    }

    //[CC431] ADD Start 20261009
    //(変更モーダル) エラー項目の要素を返す
    //※select2 の項目も元の select を返す（Site.css で、input-error が付いた select のすぐ後ろの select2 の表示部分を赤くしている）
    function getModalErrTarget(errColumn) {
        if (errColumn == null || errColumn == "") {
            return null;
        }
        return $("#modal-content #" + errColumn);
    }

    //(変更モーダル) エラー項目の赤表示を消す
    function clearModalErr() {
        $("#modal-content .input-error").removeClass("input-error");
    }
    //[CC431] ADD End 20261009

    //(変更モーダル) 変更ボタンクリック処理
    function btnUpdate_onclick() {
        //[CC431] ADD Start 20261009
        //前回のエラー項目の赤表示を消す
        clearModalErr();
        //[CC431] ADD End 20261009

        var editedContain = {
            ASSET_NO: document.getElementById('ASSET_NO').value,
            DIVERSION_REMOVE: document.getElementById('DIVERSION_REMOVE').value,
            OPE_STATUS: document.getElementById('OPE_STATUS').value,
            REMOVE_DATE: document.getElementById('REMOVE_DATE').value,
            REMOVE_REASON: document.getElementById('REMOVE_REASON').value,
            REMOVE_CONTENT: document.getElementById('REMOVE_CONTENT').value,
            LOC_L_CD: document.getElementById('SearchLocL').value,
            CHANGE_LOC_M_CD: document.getElementById('CHANGE_LOC_M_CD').value,
            CHANGE_LOC_S_CD: document.getElementById('CHANGE_LOC_S_CD').value,
            CHANGE_EMPLOYEE: document.getElementById('CHANGE_EMPLOYEE').value,
            REMARK1: document.getElementById('REMARK1').value,
            REMARK2: document.getElementById('REMARK2').value,
            REMARK3: document.getElementById('REMARK3').value,
            REMARK4: document.getElementById('REMARK4').value,
            REMARK5: document.getElementById('REMARK5').value,
            REMARK6: document.getElementById('REMARK6').value,
            REMARK7: document.getElementById('REMARK7').value,
            REMARK8: document.getElementById('REMARK8').value,
            REMARK9: document.getElementById('REMARK9').value,
            REMARK10: document.getElementById('REMARK10').value,
        };

        $("#EditJson").val(JSON.stringify(editedContain));
        var form = $('#form')[0];
        var formData = new FormData(form);

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/ValidateEditContain',
            data: formData,
            processData: false,
            contentType: false,
            cache: false,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            },
            success: function (result, dataType) {

                if (result == null || result == false) {
                    //NG
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)")
                    return;
                }

                if (!(result.err == null || result.err == "")) {
                    //[CC431] UPD Start 20261009 エラー項目を赤く表示する
                    showErrMessage(result.err, getModalErrTarget(result.errColumn));
                    //[CC431] UPD End 20261009
                    return;
                }

                if (result.ok != true) {
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)");
                    return;
                }

                //確認メッセージを出し、ＯＫなら変更処理
                // ダイアログのメッセージを設定
                $("#show-dialog").html("@Message.getMessage(CommonInfoMsg.EDIT_CONFIRM)");

                // ダイアログを作成
                $("#show-dialog").dialog({
                    modal: true,
                    title: strConfirmTitle,
                    dialogClass: "show-confirm-wrap",
                    buttons: {
                        "OK": function () {
                            $(this).dialog("close");
                            detailEdit(formData);
                        },
                        "Cancel": function () {
                            $(this).dialog("close");
                            return false;
                        }
                    },
                    open: function () {      // キャンセルボタンにフォーカスをあてる
                        $(this).siblings('.ui-dialog-buttonpane').find('button:eq(1)').focus();
                    }
                });

            }
        });



    }

    function detailEdit(formData) {
        //[CC431] ADD Start 20261009
        //エラーでモーダルに戻るときにボタンを元に戻すため、無効化する前のスタイルを保持する
        var createBtnStyle = $('#modal-create-btn').attr('style');
        var closeBtnStyle = $('#modal-close').attr('style');
        //[CC431] ADD End 20261009
        $('#modal-create-btn').prop('disabled', true);
        DisableButton("modal-create-btn");
        DisableButton("modal-close");

        //レコードを更新する。
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/EditAsset',
            data: formData,
            processData: false,
            contentType: false,
            cache: false,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            },
            success: function (result, dataType) {
                if (result == null || result == false) {
                    //NG
                    showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_SYSTEM)")
                    return;
                }

                //[CC431] ADD Start 20261009
                //EditAsset のエラー（ロック・値のチェック）は、モーダルを閉じずにメッセージを出し、エラー項目を赤く表示する
                if (!(result.err == null || result.err == "")) {
                    $('#modal-create-btn').prop('disabled', false).attr('style', createBtnStyle);
                    $('#modal-close').prop('disabled', false).attr('style', closeBtnStyle);
                    showErrMessage(result.err, getModalErrTarget(result.errColumn));
                    return;
                }
                //[CC431] ADD End 20261009

                $("#modal-content, #modal-overlay").fadeOut("slow", function () {
                    //[#modal-overlay]を削除する
                    $('#modal-overlay').remove();
                });

                showMessageAndFunc("@Message.getMessage(CommonLabelInventoryInquiry.MSG_CHANGE_COMPLETE)", () => {
                    window.location.href = '@Url.Content("~/InventoryInquiry/Index")?actionname=@CommonConst.KEY_ON_CREATE_CLICK';
                });
            }
        });
    }

    function ValidateCheck() {

        var locL = '@Model.FirstOrDefault.SearchLocLSelected';
        var changeLocM = $("#CHANGE_LOC_M_CD").val();

        if (!existsPair(locL, changeLocM)) {
            return "error";
        }

        return "";
    }

    // サーババリデーション（true/false と エラー配列を返す想定）
    function serverValidate(editedContain) {
      return $.ajax({
        type: 'POST',
        url: '@Url.Content("~/InventoryInquiry")/ValidateAsset',
        data: { editedContain: editedContain },
        cache: false
      });
    }

    function existsPair(locL, locM) {
        const L = (locL || '').trim().toLowerCase();
        const M = (locM || '').trim().toLowerCase();
        return locationMList.some(x =>
            String(x.LOC_L_CD || '').trim().toLowerCase() === L &&
            String(x.LOC_M_CD || '').trim().toLowerCase() === M
        );
    }

    //全選択ボタンクリック時処理
    function btnSelectAll_onclick() {
        var chkFlag = false;
        var btn = document.getElementById("btnSelectAll");
        if( btn.value == "@Message.getMessage(CommonLabel.BUTTON_ALL_ON)"){
            btn.value ="@Message.getMessage(CommonLabel.BUTTON_ALL_OFF)";
            $('input[name="checkEdit"]').prop('checked',true)
            chkFlag = true;
        }else{
            btn.value ="@Message.getMessage(CommonLabel.BUTTON_ALL_ON)";
            $('input[name="checkEdit"]').prop('checked',false)
            chkFlag = false;
        }

        //[CC431] ADD Start 20261007
        //グリッドのデータにもチェック状態を保持する（編集などで行が描き直されてもチェック状態が変わらないようにする）
        for (const data of grid.getData()) {
            data.DISP_FLAG = chkFlag ? convertToEmptyIfNull(data.ASSET_NO) : null;
        }
        //[CC431] ADD End 20261007

        //非同期で現在の検索条件での全件を取得する
        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/SelectAll',
            data: {blnSelectAll:chkFlag},
            dataType: 'json',
            cache: false,
            beforeSend: function () {
                //画面ブロックを表示
                showLoading();
            },
            success: function (data, dataType) {
                //件数を設定する
                document.getElementById("selectcount").innerHTML = data;

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            },
            complete: function () {
                //画面ブロックを解除
                hideLoading();
            }
        });
    }

    //CSVデータ取込ボタンクリック処理
    function btnCsvImport_onclick() {
        //[CC431] UPD Start 20261008
        document.getElementById("popup-title-csv").innerHTML = "@ViewBag.Title" + " " + "@Message.getMessage(CommonLabel.BUTTON_DATA_IMPORT)";
        document.getElementById("modal-import-btn").value = "@Message.getMessage(CommonLabel.BUTTON_DATA_IMPORT)";
        //[CC431] UPD End 20261008
        document.getElementById("CsvFile").value = "";          //ファイル選択をクリア
        document.getElementById("modal-msg").innerHTML = "";    //メッセージをクリア
        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.Nomal";   //メッセージの背景色を白にする

        // td内のtrをループ。rowsコレクションで,行位置取得。
        var tblTbody = document.getElementById('csv-err-ichiran');
        var rowCount = tblTbody.rows.length
        for (var i=0; i<rowCount; i++) {
            if( i < "@DataInport.DISP_ROW_COUNT"){
                // tr内のtdをループ。cellsコレクションで行内セル位置取得。
                for (var j=0, colLen=tblTbody.rows[i].cells.length ; j<colLen; j++) {
                    //i行目のj列目のセルをクリア
                    tblTbody.rows[i].cells[j].innerHTML ="　";
                    tblTbody.rows[i].cells[j].title ="　";
                }
            }else{
                //i行目のj列目のセルを削除
                tblTbody.rows["@DataInport.DISP_ROW_COUNT"].remove();
            }

        }

        //CSV取込画面を表示
        ShowCsvModalWindow();
    }


    //CSVデータ取込画面　取込開始ボタンクリック処理
    function btnCsvImportStart_onclick() {
        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.Ok";
        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORT)";

        // td内のtrをループ。rowsコレクションで,行位置取得。
        var tblTbody = document.getElementById('csv-err-ichiran');
        var rowCount = tblTbody.rows.length
        for (var i=0; i<rowCount; i++) {
            if( i < "@DataInport.DISP_ROW_COUNT"){
                // tr内のtdをループ。cellsコレクションで行内セル位置取得。
                for (var j=0, colLen=tblTbody.rows[i].cells.length ; j<colLen; j++) {
                    //i行目のj列目のセルをクリア
                    tblTbody.rows[i].cells[j].innerHTML ="　";
                    tblTbody.rows[i].cells[j].title ="　";
                }
            }else{
                //i行目のj列目のセルを削除
                tblTbody.rows["@DataInport.DISP_ROW_COUNT"].remove();
            }

        }

        //CSVデータを取得し表示する(Ajax非同期通信)
        var $form = $(this).closest('form')
        var formData = new FormData($form.get(0));
        //[CC431] UPD Start 20261008
        formData.append("ExcelFile", $('#CsvFile')[0].files[0]);
        //[CC431] UPD End 20261008

        $.ajax({
            type: 'POST',
            //[CC431] UPD Start 20261008
            url: '@Url.Content("~/InventoryInquiry")/ImportExcelFile',
            //[CC431] UPD End 20261008
            data: formData,
            dataType: 'json',
            processData: false,
            contentType: false,
            cache: false,
            success: function (data, dataType) {
                this;

                switch (data[0].ResultCd) {
                    case @DataInport.DataInportResultCd.OK:
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORTOK)";
                        break;
                    case @DataInport.DataInportResultCd.NoSelect:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SELECT)";
                        break;
                    case @DataInport.DataInportResultCd.Empty:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_EMPTY)";
                        break;
                    case @DataInport.DataInportResultCd.NotCsv:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_NOTCSV)";
                        break;
                    //[CC431] ADD Start 20261008
                    case @DataInport.DataInportResultCd.NotExcel:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_NOTEXCEL)";
                        break;
                    //[CC431] ADD End 20261008
                    case @DataInport.DataInportResultCd.KoumokuSuuErr:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_KOUMOKUSUU)";
                        break;
                    case @DataInport.DataInportResultCd.DataNG:
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_INPORTNG)";

                        for (var icnt = 0; icnt < data.length; icnt++) {
                            if( icnt < "@DataInport.DISP_ROW_COUNT"){
                                //icnt行目の各項目に値設定
                                var dispErrRowIdx = "";
                                if (data[icnt].ErrRowIdx != null) {
                                    dispErrRowIdx = data[icnt].ErrRowIdx;
                                }
                                //tblTbody.rows[icnt].cells[0].innerHTML =data[icnt].ErrRowIdx;
                                tblTbody.rows[icnt].cells[0].innerHTML =dispErrRowIdx;
                                tblTbody.rows[icnt].cells[0].title = dispErrRowIdx;
                                tblTbody.rows[icnt].cells[1].innerHTML =data[icnt].ErrKoumoku;
                                tblTbody.rows[icnt].cells[1].title = data[icnt].ErrKoumoku;
                                tblTbody.rows[icnt].cells[2].innerHTML =data[icnt].ErrGaiyou;
                                tblTbody.rows[icnt].cells[2].title =data[icnt].ErrGaiyou;
                            }else{
                                var dispErrRowIdx = "";
                                if (data[icnt].ErrRowIdx != null) {
                                    dispErrRowIdx = data[icnt].ErrRowIdx;
                                }
                                //$("#csv-err-ichiran").append('<tr><td class="col1">'+ data[icnt].ErrRowIdx +'</td><td class="col2" >'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" >'+ data[icnt].ErrGaiyou +'</td></tr>');
                                //$("#csv-err-ichiran").append('<tr><td class="col1">'+ dispErrRowIdx +'</td><td class="col2" >'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" >'+ data[icnt].ErrGaiyou +'</td></tr>');
                                $("#csv-err-ichiran").append('<tr><td class="col1" title=' + dispErrRowIdx + '>'+ dispErrRowIdx +'</td><td class="col2" title=' + data[icnt].ErrKoumoku + '>'+ data[icnt].ErrKoumoku +'</td>' + '<td class="col3" title=' + data[icnt].ErrGaiyou + '>'+ data[icnt].ErrGaiyou +'</td></tr>');
                            }

                        }

                        break;
                    case @DataInport.DataInportResultCd.LoginSession:
                        // ログインセッション切れの場合
                        document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                        document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_LOGININFO_EMPTY)";
                        break;
                }
                // 2022/7/25 sv sato add start
                if (data[0].ResultCd != "@DataInport.DataInportResultCd.OK" &&
                    data[0].ResultCd != "@DataInport.DataInportResultCd.NoSelect") {
                    //正常終了以外の場合、CSVデータ取込ボタンを非活性にする
                    btnCsvImportEnabled(false)
                }
                // 2022/7/25 sv sato add end
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
                document.getElementById("modal-msg").style.backgroundColor = "@DataInport.color.NG";
                document.getElementById("modal-msg").innerHTML = "@Message.getMessage(CommonLabel.MSG_ERR_SOUTEIGAI)";
                btnCsvImportEnabled(false)           // ADD sv sato 2022/7/25
            }
        });
    }

    //CSVモーダル CSVデータ取込ボタン押下可否設定処理
    function btnCsvImportEnabled(enable) {
        if (enable) {
            //活性
            $("#modal-import-btn").attr("disabled", false)
        } else {
            //非活性
            $("#modal-import-btn").attr("disabled", "disabled")
            $("#modal-import-btn").css("background-color","#cecece");
        }
    }

    //CSV出力ボタンクリック処理
    function btnCsvExport_onclick() {
        //確認メッセージ
        //[CC431] UPD Start 20261005
        return showConfirmFileDownload("@Message.getMessage(CommonInfoMsg.DATA_EXPORT_CONFIRM)", "@Url.Content("~/InventoryInquiry")/DataExport");
        //[CC431] UPD End 20261005
    }

    //編集ボタンを非活性化
    function disableEditAssetButton() {

        $("#editAsset").prop("disabled", true);
        $("#editAsset").css("background-color", "#cecece");

    }


    //検索ボタンクリック処理
    function btnSearch_onclick() {
        // 実棚ステータスの選択状態をチェック
        @*if (!document.getElementById("SearchPhyinvStatusN").checked
            && !document.getElementById("SearchPhyinvStatusW").checked
            && !document.getElementById("SearchPhyinvStatusE").checked) {
            showErrMessage("@Message.getMessage(CommonErrorMsg.PHYINV_STATUS_CHECKBOX_NO_SELECTED)")
            return false;
        }*@

        //[CC431] ADD Start 20261002
        // 日付範囲の前後チェック（From・To の両方が入力されていて From＞To の場合はエラー。片方のみの指定は可）
        // ※type="date" の値は yyyy-MM-dd 固定のため、文字列比較で前後を判定できる
        var dateRangeChecks = [
            { from: "SearchPhyinvDateFrom", to: "SearchPhyinvDateTo", label: "@Message.getMessage(CommonLabel.LABEL_PHYINV_DATE_SEARCH)" },
            { from: "SearchFreeItem2From", to: "SearchFreeItem2To", label: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)" },
            { from: "SearchFreeItem18From", to: "SearchFreeItem18To", label: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)" },
            { from: "SearchFreeItem19From", to: "SearchFreeItem19To", label: "@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)" }
        ];
        for (var i = 0; i < dateRangeChecks.length; i++) {
            var chk = dateRangeChecks[i];
            var dateFrom = document.getElementById(chk.from).value;
            var dateTo = document.getElementById(chk.to).value;
            if (dateFrom != "" && dateTo != "" && dateFrom > dateTo) {
                showErrMessage("@Message.getMessage(CommonLabel.MSG_ERR_DATE_ORDER_ITEM)".format(chk.label), "#" + chk.from);
                return false;
            }
        }
        //[CC431] ADD End 20261002

        //画面ブロックを表示
        showLoading();

        //ページ番号のリセット
        ResetPageNum();
        return true;
    }

    //ページ番号クリック処理
    function btnPage_onclick(element) {
        document.getElementById("PageNum").value = element.name;    //2020/02/20 nv miyaji
        //document.getElementById("PageNum").value = element.value;    //2020/02/20 nv miyaji

        //チェックボックスの状態からキーを設定する
        setting_checklist();

        var editedList = getEditedList();
        if (editedList.length > 0) {

            showConfirmOkEvt("@Message.getMessage(CommonLabelInventoryInquiry.MSG_PAGE_MOVE_CONFIRM)").then(() => {
                $('#form').submit();
            });
        }
        else {
            $('#form').submit();
        }

    }

    //検索条件の権限ラジオボタンクリック処理
    function rdoSelect_onclick(element) {
        //クリックされたラジオボタンの値を隠し項目に格納する
        document.getElementById("SearchKengen").value = element.value;
    }

    //※プルダウン・テキストボックス内の×（クリアボタン）の表示更新 refreshClearable は Scripts\multi-input-dialog.js

    //(変更モーダル) コンボボックスの選択肢を作成する
    //※list は { id: コード, text: 「コード 名称」 } の配列。現在値が選択肢にない場合も表示できるよう追加する
    function setModalSelectOptions(selectId, list, value) {
        var select = document.getElementById(selectId);
        var current = value || "";

        select.options.length = 0;
        //選択解除用の空の選択肢
        select.appendChild(new Option("", ""));
        if (current !== "" && !list.some(l => l.id == current)) {
            select.appendChild(new Option(current, current));
        }
        list.forEach(l => select.appendChild(new Option(l.text, l.id)));

        select.value = current;
        //Select2の表示に反映（onchange は発生させない）
        refreshSelect2(select);
    }

    //(変更モーダル) 変更後設置場所の選択肢を作成する（変更後部課で絞り込み。検索条件の変更後設置場所コードと同じ選択肢）
    //[CC431] ADD Start 20261008
    //(変更モーダル) 該当データのロケ中区分コード（変更後部課が空のときの変更後設置場所の絞り込みに使う）
    var modalLocMCd = "";
    //[CC431] ADD End 20261008
    function setModalChangeLocSOptions(mCode, sCode) {
        var list = [];
        //[CC431] UPD Start 20261008
        //変更後部課が空なら、該当データのロケ中区分コードで絞り込む（一覧の LocationSelect2Editor と同じ）
        var filterMCd = (mCode || "").trim() || modalLocMCd;
        if (filterMCd) {
            list = locationSList
                .filter(l => l.LOC_M_CD == filterMCd)
        //[CC431] UPD End 20261008
                .map(l => ({ id: l.LOC_S_CD, text: l.LOC_NAME }));       // LOC_NAME は「コード 名称」
        }
        setModalSelectOptions("CHANGE_LOC_S_CD", list, sCode);
    }

    //(変更モーダル) 変更後部課・変更後設置場所の選択肢を作成し、値を設定する
    //[CC431] UPD Start 20261008
    function setModalChangeLoc(mCode, sCode, locMCd) {
        //該当データのロケ中区分コードを保持する
        modalLocMCd = (locMCd || "").trim();
    //[CC431] UPD End 20261008
        //[CC431] ADD Start 20261007
        //変更後部課が空白だけの場合は空として扱う
        mCode = (mCode || "").trim();
        //[CC431] ADD End 20261007
        //変更後部課（検索条件の変更後課コードと同じ選択肢）
        var listM = locationMList.map(l => ({ id: l.LOC_M_CD, text: l.LOC_NAME }));     // LOC_NAME は「コード 名称」
        setModalSelectOptions("CHANGE_LOC_M_CD", listM, mCode);
        //変更後設置場所
        setModalChangeLocSOptions(mCode, sCode);
    }

    //(変更モーダル) 変更後部課を変更したら、変更後設置場所の選択肢を入れ替える（変更後設置場所はクリア）
    $(document).on("change", "#CHANGE_LOC_M_CD", function () {
        setModalChangeLocSOptions($(this).val(), "");
    });

    //大区分選択
    //※大区分は棚卸対象のロケーション会社で固定（非活性）のため、通常は呼び出されない
    function ChangeLocationL(locL) {
        // 中区分をクリア（中区分・小区分は複数選択）
        clearMultiSelect("#SearchLocM");
        $("#SearchLocM").prop("disabled", true);
        // 小区分をクリア
        clearMultiSelect("#SearchLocS");
        $("#SearchLocS").prop("disabled", true);

        // 選択されていない場合はここで終了
        if (!locL) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationM", selectLCode: locL, selectMCode: "" },
            dataType: 'json',
            cache: false,
            success: function (data, dataType) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocM = document.getElementById("SearchLocM");
                    SearchLocM.options.length = 0;

                    // 複数選択のため選択解除用の空の選択肢は不要
                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocM.appendChild(op);
                    }
                }
                // 中区分データ取得
                $("#SearchLocM").removeAttr("disabled");
                // 選択肢の入れ替えをSelect2に反映（onchange は発生させない）
                clearMultiSelect("#SearchLocM");
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //中区分選択（複数選択）
    function ChangeLocationM(locM) {
        var locSId = "SearchLocS";
        ChangeLocMCommonProc(locM, locSId);
    }

    //変更後中区分選択（複数選択）
    function ChangeChangedLocationM(locM) {
        var locSId = "SearchChangeLocSCd";
        ChangeLocMCommonProc(locM, locSId);
    }

    //中区分（複数選択）の変更に合わせて小区分（複数選択）の選択肢を入れ替える
    function ChangeLocMCommonProc(locM, locSId) {
        var lCode = document.getElementById("SearchLocL").value;
        // 選択中の中区分コード（複数選択のため「,」区切りで渡す）
        var mCode = getMultiSelectValues(locM).join(",");

        // 小区分をクリア
        clearMultiSelect("#" + locSId);
        $("#" + locSId).attr("disabled", "disabled");

        // 選択されていない場合はここで終了
        if (!mCode) {
            return;
        }

        $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/ChangeLocation',
            data: { locationLevel: "@LOC_DIV.LocationS", selectLCode: lCode, selectMCode: mCode },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data == null) {
                    return;
                }
                else {
                    var SearchLocS = document.getElementById(locSId);
                    SearchLocS.options.length = 0;

                    // 複数選択のため選択解除用の空の選択肢は不要
                    for (var i = 0; i < data.Data.length; i++) {
                        let op = document.createElement("option");
                        op.value = data.Data[i].LOC_CD;
                        op.text = data.Data[i].LOC_NAME;
                        SearchLocS.appendChild(op);
                    }
                }
                // 小区分データ取得
                $("#" + locSId).removeAttr("disabled");
                // 選択肢の入れ替えをSelect2に反映（onchange は発生させない）
                clearMultiSelect("#" + locSId);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                this;
            }
        });
    }

    //[CC431] ADD Start 20261007
    //検索条件 ロケーション（原価センタ・場所）／変更後（変更後課・新変更後設置場所）の連動
    //  場所から選ぶ OFF：中区分（m）で小区分（s）の選択肢を絞り込む
    //  場所から選ぶ ON ：小区分（s）で中区分（m）の選択肢を絞り込む
    //  絞り込む側の選択値を変えたら、絞り込まれる側の選択値は使えるものを残し、使えなくなったものを消す
    //  xhr：選択肢を取得中の通信（続けて選び直したときに、古い応答で選択肢が上書きされないよう中止する）
    var searchLocPairs = {
        Loc: { m: "#SearchLocM", s: "#SearchLocS", reverse: "#SearchLocReverse", xhr: null },
        ChangeLoc: { m: "#SearchChangeLocMCd", s: "#SearchChangeLocSCd", reverse: "#SearchChangeLocReverse", xhr: null }
    };

    //場所から選ぶが ON か（チェックボックスがない場合は OFF）
    function isSearchLocReverse(key) {
        return $(searchLocPairs[key].reverse).prop("checked") === true;
    }

    //中区分・小区分の選択値を変えたとき
    //  変えた側が絞り込む側（OFF なら中区分、ON なら小区分）のときだけ、反対側の選択肢を作り直す
    function onSearchLocChange(key, level) {
        var fromLevel = isSearchLocReverse(key) ? "@LOC_DIV.LocationS" : "@LOC_DIV.LocationM";
        if (level !== fromLevel) {
            return;
        }
        refreshSearchLocOptions(key);
    }

    //絞り込まれる側の選択肢を、絞り込む側の選択値で作り直す（絞り込まれる側の選択値は使えるものだけ残す）
    function refreshSearchLocOptions(key) {
        var pair = searchLocPairs[key];
        var reverse = isSearchLocReverse(key);
        var from = reverse ? pair.s : pair.m;
        var to = reverse ? pair.m : pair.s;
        var toLevel = reverse ? "@LOC_DIV.LocationM" : "@LOC_DIV.LocationS";

        abortSearchLocRequest(key);

        // 絞り込む側が未選択の場合は、絞り込まれる側をクリアして非活性
        var codes = getMultiSelectValues(from);
        if (codes.length == 0) {
            clearMultiSelect(to);
            $(to).prop("disabled", true);
            return;
        }

        loadSearchLocOptions(key, toLevel, codes, to, getMultiSelectValues(to));
    }

    //場所から選ぶを変えたとき
    //  ON にしたら：場所の選択値は残し、原価センタはクリア。OFF にしたら：原価センタの選択値は残し、場所はクリア
    //  残した側の選択肢を全件にしてから、その選択値で反対側の選択肢を作る
    function onSearchLocReverseChange(key) {
        var pair = searchLocPairs[key];
        var reverse = isSearchLocReverse(key);
        var from = reverse ? pair.s : pair.m;
        var fromLevel = reverse ? "@LOC_DIV.LocationS" : "@LOC_DIV.LocationM";
        var to = reverse ? pair.m : pair.s;

        abortSearchLocRequest(key);

        clearMultiSelect(to);
        $(to).prop("disabled", true);

        loadSearchLocOptions(key, fromLevel, [], from, getMultiSelectValues(from), function () {
            refreshSearchLocOptions(key);
        });
    }

    //選択肢を取得中の通信を中止する
    function abortSearchLocRequest(key) {
        var pair = searchLocPairs[key];
        if (pair.xhr) {
            var xhr = pair.xhr;
            pair.xhr = null;
            xhr.abort();
        }
    }

    //選択肢を取得して入れ替え、keepValues のうち選択肢にあるものだけを選択し直す（onchange は発生させない）
    //  codes：絞り込みに使う反対側の選択値（絞り込む側の選択肢を取得する場合はサーバー側で使わない）
    function loadSearchLocOptions(key, level, codes, target, keepValues, callback) {
        var pair = searchLocPairs[key];
        var xhr = $.ajax({
            type: 'POST',
            url: '@Url.Content("~/InventoryInquiry")/GetSearchLocationOptions',
            data: { target: key, level: level, reverse: isSearchLocReverse(key), codes: codes.join(",") },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (pair.xhr !== xhr) {
                    return;    // 中止した通信、または古い通信の応答
                }
                pair.xhr = null;
                if (data == null) {
                    return;
                }

                var select = $(target)[0];
                select.options.length = 0;
                // 複数選択のため選択解除用の空の選択肢は不要
                for (var i = 0; i < data.length; i++) {
                    let op = document.createElement("option");
                    op.value = data[i].LOC_CD;
                    op.text = data[i].LOC_NAME;
                    select.appendChild(op);
                }
                $(target).prop("disabled", false);
                // 選択肢にない値は jQuery が外すため、使える値だけが残る
                setMultiSelectValues(target, keepValues);

                if (callback) {
                    callback();
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                if (pair.xhr === xhr) {
                    pair.xhr = null;
                }
            }
        });
        pair.xhr = xhr;
    }
    //[CC431] ADD End 20261007

    //チェックボックスクリック処理
    function check_onclick(element) {
        var checkconfirm = element.checked;
        var count = document.getElementById("selectcount").innerHTML;
        if (checkconfirm) {
            count++;
        } else {
            count--;
        }
        document.getElementById("selectcount").innerHTML = count;

        //[CC431] ADD Start 20261007
        //グリッドのデータにもチェック状態を保持する（編集などで行が描き直されてもチェック状態が変わらないようにする）
        for (const data of grid.getData()) {
            if (convertToEmptyIfNull(data.ASSET_NO) === element.value) {
                data.DISP_FLAG = checkconfirm ? element.value : null;
                break;
            }
        }
        //[CC431] ADD End 20261007
    }

    //チェックボックスの選択状態を設定する
    function setting_checklist() {
        var keys = [];
        var nonekeys = [];
        // SlickGridのチェックボックスを取得
        var checkBoxes = $(grid.getContainerNode()).find(".slick-checkbox");
        for (var i = 0; i < checkBoxes.length; i++) {
            //チェックボックスの場合
            if (checkBoxes[i].type == "checkbox") {
                //キーを取得する
                var result = checkBoxes[i].value.split(",")

                if (checkBoxes[i].checked) {
                    //選択されている場合
                    keys.push(result[0]);
                } else {
                    //選択されていない場合
                    nonekeys.push(result[0]);
                }
            }
        }

        // スラッシュ区切りの文字列を設定
        document.getElementById("SelectKeys").value = keys.join("/");
        document.getElementById("NoneSelectKeys").value = nonekeys.join("/");

        return keys;
    }

    //編集内容を設定する
    function getEditedList() {
        var list = [];

        grid.getData().forEach((r) => {

            var isEdited = [r.EDITED_OPE_STATUS,
                r.EDITED_DIVERSION_REMOVE,
                r.EDITED_REMOVE_DATE,
                r.EDITED_REMOVE_REASON,
                r.EDITED_REMOVE_CONTENT,
                r.EDITED_CHANGE_LOC_M_CD,
                r.EDITED_CHANGE_LOC_S_CD,
                r.EDITED_CHANGE_EMPLOYEE,
                r.EDITED_REMARK1,
                r.EDITED_REMARK2,
                r.EDITED_REMARK3,
                r.EDITED_REMARK4,
                r.EDITED_REMARK5,
                r.EDITED_REMARK6,
                r.EDITED_REMARK7,
                r.EDITED_REMARK8,
                r.EDITED_REMARK9,
                r.EDITED_REMARK10].some(x => x == 1);

            if (!isEdited) {
                return;
            }

            var obj = {
                "ASSET_NO": r.ASSET_NO,

                "OPE_STATUS": r.OPE_STATUS,
                "DIVERSION_REMOVE": r.DIVERSION_REMOVE,
                "REMOVE_DATE": r.REMOVE_DATE,
                "REMOVE_REASON": r.REMOVE_REASON,
                "REMOVE_CONTENT": r.REMOVE_CONTENT,
                "LOC_L_CD": document.getElementById('SearchLocL').value,
                "CHANGE_LOC_M_CD": r.CHANGE_LOC_M_CD,
                "CHANGE_LOC_S_CD": r.CHANGE_LOC_S_CD,
                "CHANGE_EMPLOYEE": r.CHANGE_EMPLOYEE,
                "REMARK1": r.REMARK1,
                "REMARK2": r.REMARK2,
                "REMARK3": r.REMARK3,
                "REMARK4": r.REMARK4,
                "REMARK5": r.REMARK5,
                "REMARK6": r.REMARK6,
                "REMARK7": r.REMARK7,
                "REMARK8": r.REMARK8,
                "REMARK9": r.REMARK9,
                "REMARK10": r.REMARK10,

                "EDITED_OPE_STATUS": r.EDITED_OPE_STATUS,
                "EDITED_DIVERSION_REMOVE": r.EDITED_DIVERSION_REMOVE,
                "EDITED_REMOVE_DATE": r.EDITED_REMOVE_DATE,
                "EDITED_REMOVE_REASON": r.EDITED_REMOVE_REASON,
                "EDITED_REMOVE_CONTENT": r.EDITED_REMOVE_CONTENT,
                "EDITED_CHANGE_LOC_M_CD": r.EDITED_CHANGE_LOC_M_CD,
                "EDITED_CHANGE_LOC_S_CD": r.EDITED_CHANGE_LOC_S_CD,
                "EDITED_CHANGE_EMPLOYEE": r.EDITED_CHANGE_EMPLOYEE,
                "EDITED_REMARK1": r.EDITED_REMARK1,
                "EDITED_REMARK2": r.EDITED_REMARK2,
                "EDITED_REMARK3": r.EDITED_REMARK3,
                "EDITED_REMARK4": r.EDITED_REMARK4,
                "EDITED_REMARK5": r.EDITED_REMARK5,
                "EDITED_REMARK6": r.EDITED_REMARK6,
                "EDITED_REMARK7": r.EDITED_REMARK7,
                "EDITED_REMARK8": r.EDITED_REMARK8,
                "EDITED_REMARK9": r.EDITED_REMARK9,
                "EDITED_REMARK10": r.EDITED_REMARK10
            };

            list.push(obj);
        });

        return list;
    }

    //編集内容を復元する
    function restore_editedlist(jsonData, editedData) {
        for (let grid_i = 0; grid_i < jsonData.length; grid_i++) {
            var e = editedData.find(e => e.ASSET_NO == jsonData[grid_i].ASSET_NO);
            if (e == undefined) {
                continue;
            }

            //orginal_dataを退避
            jsonData[grid_i].ORG_OPE_STATUS = jsonData[grid_i].OPE_STATUS;
            jsonData[grid_i].ORG_DIVERSION_REMOVE = jsonData[grid_i].DIVERSION_REMOVE;
            jsonData[grid_i].ORG_REMOVE_DATE = jsonData[grid_i].REMOVE_DATE;
            jsonData[grid_i].ORG_REMOVE_REASON = jsonData[grid_i].REMOVE_REASON;
            jsonData[grid_i].ORG_REMOVE_CONTENT = jsonData[grid_i].REMOVE_CONTENT;
            jsonData[grid_i].ORG_CHANGE_LOC_M_CD = jsonData[grid_i].CHANGE_LOC_M_CD;
            jsonData[grid_i].ORG_CHANGE_LOC_S_CD = jsonData[grid_i].CHANGE_LOC_S_CD;
            jsonData[grid_i].ORG_CHANGE_EMPLOYEE = jsonData[grid_i].CHANGE_EMPLOYEE;
            jsonData[grid_i].ORG_REMARK1 = jsonData[grid_i].REMARK1;
            jsonData[grid_i].ORG_REMARK2 = jsonData[grid_i].REMARK2;
            jsonData[grid_i].ORG_REMARK3 = jsonData[grid_i].REMARK3;
            jsonData[grid_i].ORG_REMARK4 = jsonData[grid_i].REMARK4;
            jsonData[grid_i].ORG_REMARK5 = jsonData[grid_i].REMARK5;
            jsonData[grid_i].ORG_REMARK6 = jsonData[grid_i].REMARK6;
            jsonData[grid_i].ORG_REMARK7 = jsonData[grid_i].REMARK7;
            jsonData[grid_i].ORG_REMARK8 = jsonData[grid_i].REMARK8;
            jsonData[grid_i].ORG_REMARK9 = jsonData[grid_i].REMARK9;
            jsonData[grid_i].ORG_REMARK10 = jsonData[grid_i].REMARK10;

            //編集された内容で上書き
            jsonData[grid_i].OPE_STATUS = e.OPE_STATUS;
            jsonData[grid_i].DIVERSION_REMOVE = e.DIVERSION_REMOVE;
            jsonData[grid_i].REMOVE_DATE = e.REMOVE_DATE;
            jsonData[grid_i].REMOVE_REASON = e.REMOVE_REASON;
            jsonData[grid_i].REMOVE_CONTENT = e.REMOVE_CONTENT;
            jsonData[grid_i].CHANGE_LOC_M_CD = e.CHANGE_LOC_M_CD;
            jsonData[grid_i].CHANGE_LOC_S_CD = e.CHANGE_LOC_S_CD;
            jsonData[grid_i].CHANGE_EMPLOYEE = e.CHANGE_EMPLOYEE;
            jsonData[grid_i].REMARK1 = e.REMARK1;
            jsonData[grid_i].REMARK2 = e.REMARK2;
            jsonData[grid_i].REMARK3 = e.REMARK3;
            jsonData[grid_i].REMARK4 = e.REMARK4;
            jsonData[grid_i].REMARK5 = e.REMARK5;
            jsonData[grid_i].REMARK6 = e.REMARK6;
            jsonData[grid_i].REMARK7 = e.REMARK7;
            jsonData[grid_i].REMARK8 = e.REMARK8;
            jsonData[grid_i].REMARK9 = e.REMARK9;
            jsonData[grid_i].REMARK10 = e.REMARK10;

            jsonData[grid_i].EDITED_OPE_STATUS = jsonData[grid_i].OPE_STATUS != jsonData[grid_i].ORG_OPE_STATUS;
            jsonData[grid_i].EDITED_DIVERSION_REMOVE = jsonData[grid_i].DIVERSION_REMOVE != jsonData[grid_i].ORG_DIVERSION_REMOVE;
            jsonData[grid_i].EDITED_REMOVE_DATE = jsonData[grid_i].REMOVE_DATE != jsonData[grid_i].ORG_REMOVE_DATE;
            jsonData[grid_i].EDITED_REMOVE_REASON = jsonData[grid_i].REMOVE_REASON != jsonData[grid_i].ORG_REMOVE_REASON;
            jsonData[grid_i].EDITED_REMOVE_CONTENT = jsonData[grid_i].REMOVE_CONTENT != jsonData[grid_i].ORG_REMOVE_CONTENT;
            jsonData[grid_i].EDITED_CHANGE_LOC_M_CD = jsonData[grid_i].CHANGE_LOC_M_CD != jsonData[grid_i].ORG_CHANGE_LOC_M_CD;
            jsonData[grid_i].EDITED_CHANGE_LOC_S_CD = jsonData[grid_i].CHANGE_LOC_S_CD != jsonData[grid_i].ORG_CHANGE_LOC_S_CD;
            jsonData[grid_i].EDITED_CHANGE_EMPLOYEE = jsonData[grid_i].CHANGE_EMPLOYEE != jsonData[grid_i].ORG_CHANGE_EMPLOYEE;
            jsonData[grid_i].EDITED_REMARK1 = jsonData[grid_i].REMARK1 != jsonData[grid_i].ORG_REMARK1;
            jsonData[grid_i].EDITED_REMARK2 = jsonData[grid_i].REMARK2 != jsonData[grid_i].ORG_REMARK2;
            jsonData[grid_i].EDITED_REMARK3 = jsonData[grid_i].REMARK3 != jsonData[grid_i].ORG_REMARK3;
            jsonData[grid_i].EDITED_REMARK4 = jsonData[grid_i].REMARK4 != jsonData[grid_i].ORG_REMARK4;
            jsonData[grid_i].EDITED_REMARK5 = jsonData[grid_i].REMARK5 != jsonData[grid_i].ORG_REMARK5;
            jsonData[grid_i].EDITED_REMARK6 = jsonData[grid_i].REMARK6 != jsonData[grid_i].ORG_REMARK6;
            jsonData[grid_i].EDITED_REMARK7 = jsonData[grid_i].REMARK7 != jsonData[grid_i].ORG_REMARK7;
            jsonData[grid_i].EDITED_REMARK8 = jsonData[grid_i].REMARK8 != jsonData[grid_i].ORG_REMARK8;
            jsonData[grid_i].EDITED_REMARK9 = jsonData[grid_i].REMARK9 != jsonData[grid_i].ORG_REMARK9;
            jsonData[grid_i].EDITED_REMARK10 = jsonData[grid_i].REMARK10 != jsonData[grid_i].ORG_REMARK10;

        }
        return jsonData;
    }

    //検索条件リンククリック時処理
    function SearchCondition_onclick() {
        if (document.getElementById("SearchCondition").innerHTML == "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)") {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)"
        } else {
            document.getElementById("SearchCondition").innerHTML = "@Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_CLOSE)"
        }
        //[CC431] ADD Start 20261005
        // 検索条件の開閉状態を hidden に退避し、検索・ページ移動・現物確認などの後も同じ状態で復元する
        document.getElementById("OpenFlg").value = $.trim(document.getElementById("SearchCondition").innerText);
        //[CC431] ADD End 20261005
    }

    //[CC431] ADD Start 20261002
    //詳細検索エリアの開閉切り替え
    //  リンクの文言（▼詳細検索／▲詳細検索）を現在の状態として扱い、クリックのたびに反転させる。
    //  開閉状態は hidden 項目 DetailOpenFlg に退避し、検索・ページング後も同じ状態で復元する。
    //  閉じていても入力欄は form 内にあるため、検索条件として送信される。
    function DetailAccordion_onclick() {
        if (document.getElementById("DetailAccordion").innerHTML == "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)") {
            $(".detail-accordion").css("display", "none");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)"
        } else {
            $(".detail-accordion").css("display", "flex");
            document.getElementById("DetailAccordion").innerHTML = "@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_CLOSE)"
        }
        document.getElementById("DetailOpenFlg").value = document.getElementById("DetailAccordion").innerText;
    }
    //[CC431] ADD End 20261002


    //変更ボタンクリック処理
    function btnEdit_onclick() {

        var selectedCheck = setting_checklist();
        if (selectedCheck.length == 0) { //テーブルの選択行がない場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_NO_SELECTED)");
            return;
        } else if (selectedCheck.length > 1) {//テーブルの選択行が複数の場合
            showErrMessage("@Message.getMessage(CommonErrorMsg.CHECKBOX_OVER_SELECTED)");
            return;
        }
        var assetNoStr = selectedCheck[0];

        //タイトルの設定
        document.getElementById("popup-title").innerHTML = "@Message.getMessage(CommonLabelInventoryInquiry.TITLE_INV_RESULT_DETAIL)";

        //エラー情報のクリア
        $("#@Model.FirstOrDefault.ErrTarget").removeClass("input-error");
        //[CC431] ADD Start 20261009
        //前回モーダルを開いたときのエラー項目の赤表示を消す
        clearModalErr();
        //[CC431] ADD End 20261009

        // 資産番号を黒色のフォント表示（資産番号自動生成で別色に変更したことの復帰処理)
        document.getElementById("ASSET_NO").style.color = "black";

        //エラーの場合検索しない
        if ("@Model.FirstOrDefault.ErrCode" == "") {
            //選択行のデータを取得し表示する(Ajax非同期通信)
            $.ajax({
                type: 'POST',
                url: '@Url.Content("~/InventoryInquiry")/GetEditData',
                cache: false,
                data: { assetNo: assetNoStr },
                success: function (data) {
                    //データが存在しない場合エラー
                    if (data == null) {
                        return;
                    }

                    //※変更後部課・変更後設置場所はコンボボックス（「コード 名称」を表示）のため、名称の表示処理は不要
                    {
                        //変更後社員番号
                        var func = (() => {
                            const input = document.getElementById('CHANGE_EMPLOYEE');
                            const nameInput = document.getElementById('CHANGE_EMPLOYEE_NAME');
                            if (input && nameInput) {
                                const code = input.value.trim();
                                var employee = employeeList.find(e => e.EMPLOYEE_CD == code);
                                if (employee != null && employee != undefined) {
                                    nameInput.value = employee.EMPLOYEE_NAME;
                                } else {
                                    nameInput.value = "";
                                }
                            }
                        });
                        document.getElementById('CHANGE_EMPLOYEE').addEventListener('input', func);
                    }

                    $("#ASSET_STATUS_NAME").val(data.ASSET_STATUS_NAME);
                    $("#LOCK_EMPLOYEE_CD").val(data.LOCK_EMPLOYEE_CD_NAME);
                    $("#PHYINV_STATUS_NAME").val(data.PHYINV_STATUS_NAME);
                    $("#ASSET_NO").val(data.ASSET_NO);;
                    $("#SUB_ASSET_NO").val(data.SUB_ASSET_NO);
                    $("#ASSET_NAME").val(data.ASSET_NAME);
                    if (String(data.PHYINV_TS).length == 14) {
                        var str = String(data.PHYINV_TS);
                        var year = str.substring(0, 4);
                        var month = str.substring(4, 6);
                        var day = str.substring(6, 8);
                        var hour = str.substring(8, 10);
                        var minute = str.substring(10, 12);
                        var sec = str.substring(12, 14);
                        var date = year + "/" + month + "/" + day + " " + hour + ":" + minute + ":" + sec;
                        $("#PHYINV_TS").val(date);
                    }

                    if (data.PHYINV_EMPLOYEE_CD != null) {
                        var str = data.PHYINV_EMPLOYEE_CD;
                        if (data.PHYINV_EMPLOYEE_NAME != null) {
                            str += " " + data.PHYINV_EMPLOYEE_NAME
                        }
                        $("#PHYINV_EMPLOYEE_CD").val(str);
                    }

                    $("#CAT_L_CD_NAME").val(data.CAT_L_CD_NAME);

                    $("#IS_TANGIBLE").val(data.IS_TANGIBLE == "Y" ? "@Message.getMessage(CommonLabel.VALUE_TANGIBLE_ASSET)" : "@Message.getMessage(CommonLabel.VALUE_INTANGIBLE_ASSET)");

                    //[CC431] ADD Start 20261008
                    //入力作業者コード/名（名称は社員M）。社員Mにないコードもコードは表示する（一覧の EntEmployeeFormatter と同じ）
                    if (convertToEmptyIfNull(data.ENT_EMPLOYEE_CD) != "") {
                        $("#ENT_EMPLOYEE_CD").val(data.ENT_EMPLOYEE_CD + " " + convertToEmptyIfNull(data.ENT_EMPLOYEE_NAME));
                    } else {
                        $("#ENT_EMPLOYEE_CD").val("");
                    }

                    //棚卸実施端末（H:HT、W:Web）。想定外の値はそのまま表示する（一覧の PhyinvToolDivFormatter と同じ）
                    if (data.PHYINV_TOOL_DIV == "@ProcessToolDiv.HT") {
                        $("#PHYINV_TOOL_DIV").val("@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_HT)");
                    } else if (data.PHYINV_TOOL_DIV == "@ProcessToolDiv.Web") {
                        $("#PHYINV_TOOL_DIV").val("@Message.getMessage(CommonLabel.LABEL_PROCESS_TOOL_WEB)");
                    } else {
                        $("#PHYINV_TOOL_DIV").val(convertToEmptyIfNull(data.PHYINV_TOOL_DIV));
                    }
                    //[CC431] ADD End 20261008

                    if (data.FREE_ITEM6 != null) {
                        var str = data.FREE_ITEM6;
                        if (data.FREE_ITEM6_NAME != null) {
                            str += " " + data.FREE_ITEM6_NAME
                        }
                        $("#FREE_ITEM6").val(str);
                    }

                    $("#FREE_ITEM1").val(data.FREE_ITEM1);
                    $("#FREE_ITEM2").val(data.FREE_ITEM2);
                    $("#FREE_ITEM3").val(data.FREE_ITEM3);
                    $("#FREE_ITEM4").val(data.FREE_ITEM4);
                    $("#FREE_ITEM5").val(data.FREE_ITEM5);
                    $("#FREE_ITEM7").val(data.FREE_ITEM7);
                    $("#FREE_ITEM8").val(data.FREE_ITEM8);
                    $("#FREE_ITEM9").val(data.FREE_ITEM9);
                    $("#FREE_ITEM10").val(data.FREE_ITEM10);
                    $("#FREE_ITEM11").val(data.FREE_ITEM11);
                    $("#FREE_ITEM12").val(data.FREE_ITEM12);
                    $("#FREE_ITEM13").val(data.FREE_ITEM13);
                    $("#FREE_ITEM14").val(data.FREE_ITEM14);
                    $("#FREE_ITEM15").val(data.FREE_ITEM15);
                    $("#FREE_ITEM16").val(data.FREE_ITEM16);
                    $("#FREE_ITEM17").val(data.FREE_ITEM17);
                    $("#FREE_ITEM18").val(data.FREE_ITEM18);
                    $("#FREE_ITEM19").val(data.FREE_ITEM19);
                    $("#FREE_ITEM20").val(data.FREE_ITEM20);
                    $("#FREE_ITEM21").val(data.FREE_ITEM21);
                    $("#FREE_ITEM22").val(data.FREE_ITEM22);
                    $("#FREE_ITEM23").val(data.FREE_ITEM23);
                    $("#FREE_ITEM24").val(data.FREE_ITEM24);
                    $("#FREE_ITEM25").val(data.FREE_ITEM25);
                    $("#FREE_ITEM26").val(data.FREE_ITEM26);
                    $("#FREE_ITEM27").val(data.FREE_ITEM27);
                    $("#FREE_ITEM28").val(data.FREE_ITEM28);
                    $("#FREE_ITEM29").val(data.FREE_ITEM29);
                    $("#FREE_ITEM30").val(data.FREE_ITEM30);
                    $("#FREE_ITEM31").val(data.FREE_ITEM31);
                    $("#FREE_ITEM32").val(data.FREE_ITEM32);
                    $("#FREE_ITEM33").val(data.FREE_ITEM33);
                    $("#FREE_ITEM34").val(data.FREE_ITEM34);
                    $("#FREE_ITEM35").val(data.FREE_ITEM35);
                    $("#FREE_ITEM36").val(data.FREE_ITEM36);
                    $("#FREE_ITEM37").val(data.FREE_ITEM37);
                    $("#FREE_ITEM38").val(data.FREE_ITEM38);
                    $("#FREE_ITEM39").val(data.FREE_ITEM39);
                    $("#FREE_ITEM40").val(data.FREE_ITEM40);
                    $("#FREE_ITEM41").val(data.FREE_ITEM41);
                    $("#FREE_ITEM42").val(data.FREE_ITEM42);
                    $("#FREE_ITEM43").val(data.FREE_ITEM43);
                    $("#FREE_ITEM44").val(data.FREE_ITEM44);
                    $("#FREE_ITEM45").val(data.FREE_ITEM45);
                    $("#FREE_ITEM46").val(data.FREE_ITEM46);
                    $("#FREE_ITEM47").val(data.FREE_ITEM47);
                    $("#FREE_ITEM48").val(data.FREE_ITEM48);
                    $("#FREE_ITEM49").val(data.FREE_ITEM49);
                    $("#FREE_ITEM50").val(data.FREE_ITEM50);
                    $("#FREE_ITEM51").val(data.FREE_ITEM51);
                    $("#FREE_ITEM52").val(data.FREE_ITEM52);
                    $("#FREE_ITEM53").val(data.FREE_ITEM53);
                    $("#FREE_ITEM54").val(data.FREE_ITEM54);
                    $("#FREE_ITEM55").val(data.FREE_ITEM55);
                    $("#FREE_ITEM56").val(data.FREE_ITEM56);
                    $("#FREE_ITEM57").val(data.FREE_ITEM57);
                    $("#FREE_ITEM58").val(data.FREE_ITEM58);
                    $("#FREE_ITEM59").val(data.FREE_ITEM59);
                    $("#FREE_ITEM60").val(data.FREE_ITEM60);

                    if (data.OPE_STATUS) $("#OPE_STATUS").val(data.OPE_STATUS);
                    if (data.DIVERSION_REMOVE) $("#DIVERSION_REMOVE").val(data.DIVERSION_REMOVE);
                    if (data.REMOVE_REASON) $("#REMOVE_REASON").val(data.REMOVE_REASON);
                    // ×（クリアボタン）の表示を更新する（入力不可の設定後にも更新する）
                    refreshClearable("#OPE_STATUS, #DIVERSION_REMOVE, #REMOVE_REASON");

                    //転用・除却の値で入力不可項目を設定する
                    var DiversionRemoveDataList = @Html.Raw(ViewBag.DiversionRemoveDataList);
                    var DiversionRemoveData = DiversionRemoveDataList.find(f => f.Key1 == data.DIVERSION_REMOVE);
                    if (!(DiversionRemoveData != null && DiversionRemoveData.DATA2 == "Y")) {
                        $("#REMOVE_DATE").prop("disabled", true);
                        $("#REMOVE_DATE").addClass("inventory-disable-input");

                        $("#REMOVE_REASON").prop("disabled", true);
                        $("#REMOVE_REASON").addClass("inventory-disable-input");

                        $("#REMOVE_CONTENT").prop("disabled", true);
                        $("#REMOVE_CONTENT").addClass("inventory-disable-input");
                    }

                    //除却理由の値で入力不可項目を設定する
                    //[CC431] UPD Start 20261008
                    // 除却内容は、除却理由が空白以外の場合に入力可（一覧と同じ）
                    if (convertToEmptyIfNull(data.REMOVE_REASON) == "") {
                        $("#REMOVE_CONTENT").prop("disabled", true);
                        $("#REMOVE_CONTENT").addClass("inventory-disable-input");
                    }
                    //[CC431] UPD End 20261008

                    refreshClearable("#REMOVE_REASON");

                    //[CC431] UPD Start 20261007
                    // 除却日が NULL（未変更の資産）でもエラーで止まらないようにする
                    var replaceDateStr = (data.REMOVE_DATE || "").replace("/", "-");
                    //[CC431] UPD End 20261007
                    $("#REMOVE_DATE").val(replaceDateStr);
                    $("#REMOVE_CONTENT").val(data.REMOVE_CONTENT);
                    $("#LOC_L_CD_NAME").val(data.LOC_L_CD_NAME);
                    $("#LOC_M_CD_NAME").val(data.LOC_M_CD_NAME);
                    $("#LOC_S_CD_NAME").val(data.LOC_S_CD_NAME);
                    $("#PHYINV_LOC_L_CD_NAME").val(data.PHYINV_LOC_L_CD_NAME);
                    $("#PHYINV_LOC_M_CD_NAME").val(data.PHYINV_LOC_M_CD_NAME);
                    $("#PHYINV_LOC_S_CD_NAME").val(data.PHYINV_LOC_S_CD_NAME);
                    $("#REMARK1").val(data.REMARK1);
                    $("#REMARK2").val(data.REMARK2);
                    $("#REMARK3").val(data.REMARK3);
                    $("#REMARK4").val(data.REMARK4);
                    $("#REMARK5").val(data.REMARK5);
                    $("#REMARK6").val(data.REMARK6);
                    $("#REMARK7").val(data.REMARK7);
                    $("#REMARK8").val(data.REMARK8);
                    $("#REMARK9").val(data.REMARK9);
                    $("#REMARK10").val(data.REMARK10);

                    //変更後部課・変更後設置場所（選択肢を作成して値を設定）
                    //[CC431] UPD Start 20261008
                    // 変更後部課が空のときは、該当データのロケ中区分コード配下の場所を選択肢にする
                    setModalChangeLoc(data.CHANGE_LOC_M_CD, data.CHANGE_LOC_S_CD, data.LOC_M_CD);
                    //[CC431] UPD End 20261008

                    var changeEmployee = document.getElementById('CHANGE_EMPLOYEE');
                    changeEmployee.value = data.CHANGE_EMPLOYEE;
                    changeEmployee.dispatchEvent(new Event('input', { bubbles: true }));

                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    this;
                }
            });
        }

        //モーダルウインドウを開く
        ShowModalWindow();

    }

</script>
<style type="text/css">
    .value-col {
        width: 225px;
        padding: 0px 10px;
    }

    /* 通常の value-col より5文字分広げる（モーダルの稼働区分） */
    .value-col.value-col-wide {
        width: calc(225px + 5em);
    }

    .inventory-disable-input {
        background-color: #cecece;
        width: 100%;
    }

    .half-width {
        width: 580px;
    }

    /* モーダルの×付きプルダウン
       ・プルダウンの幅に合わせて×の位置を決める（×がプルダウンの枠外に出ないようにする）
       ・clearable-fill：枠（value-col）いっぱいの幅にする（下の行の実棚会社などの入力欄と幅・列位置をそろえる） */
    #modal-content .clearable {
        display: inline-block;
        max-width: 100%;
    }

    #modal-content .clearable.clearable-fill {
        display: block;
    }

        #modal-content .clearable.clearable-fill select {
            width: 100%;
            box-sizing: border-box;
        }

    /* 一覧内の×付きプルダウン（変更後稼働区分・転用or除却・除却理由）
       プルダウンをセルいっぱいの幅にし、×をプルダウンの枠内の右端（▼の左）に表示する */
    #ListGrids .slick-cell .clearable select {
        width: 100%;
        box-sizing: border-box;
    }

    /*[CC431] ADD Start 20261002*/
    /* 一覧内のテキストボックス（除却内容・変更後管理者社員番号・備考1～10）
       セルの幅に合わせる（ブラウザ既定の幅だと、列幅により枠より狭く／広く表示されるため） */
    #ListGrids .slick-cell input[type='text'].editor-text {
        width: 100%;
        box-sizing: border-box;
    }
    /*[CC431] ADD End 20261002*/

    /* 一覧内エディタ（Select2）の高さをセルに合わせる */
    #ListGrids .select2-container--default .select2-selection--single {
        height: 22px;
        border-radius: 2px;
    }

    #ListGrids .select2-container--default .select2-selection--single .select2-selection__arrow {
        height: 20px;
    }

    /* 一覧内エディタ（Select2）の候補リストは、列幅が狭くても名称が読める幅にする */
    .inventory-grid-select2 .select2-dropdown {
        min-width: 250px;
    }
</style>

<div id="inventory-inquiry">
    @Using (Html.BeginForm("Index", "InventoryInquiry", FormMethod.Post, New With {.id = "form", .enctype = "multipart/form-data"}))
        @Html.AntiForgeryToken()
        @<div class="view-area mb20" style="width: auto;">
            <div id="view-area-button-parent">
                <div class="view-area-button-left">
                    @* 現ロケに変更 *@
                    @*@If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                            @<input class="defButton" type="button" value="@Message.getMessage(CommonLabelInventorySummary.BUTTON_CHANGE_CURRENT_LOC)" onclick="return btnChangeLocation_onclick()">
                        End If*@
                    @* QR画面表示 *@
                    <input class="defButton" type="button" value="@Message.getMessage(CommonLabel.BUTTON_SHOW_QR)" onclick="return btnShowQR_onclick()">
                    @If ViewBag.AuthProcFlg = AuthProcFlg.All Then
                        @* 現物確認 *@
                        @<input class="defButton" type="button" value="@Message.getMessage(CommonLabelInventoryInquiry.BUTTON_GENBUTSU_KAKUNIN)" onclick="return btnGenbutsuKakunin_onclick()">
                        @* 現物未確認 *@
                        @*@<input class="defButton" type="button" value="@Message.getMessage(CommonLabelInventoryInquiry.BUTTON_GENBUTSU_MIKAKUNIN)" onclick="return btnGenbutsuMikakunin_onclick()">*@
                        @* 編集 *@
                        @<input class="defButton" type="button" id="editAsset" value="@Message.getMessage(CommonLabelInventoryInquiry.LABEL_INVENTORY_EDIT)" onclick="return btnEdit_onclick()">
                        @* 全選択 *@
                        @Code
                            Dim AllSelectFlg As Boolean = Session(CommonSessionKey.ALL_SELECT_FLG)
                            Dim sDispStr As String = Message.getMessage(CommonLabel.BUTTON_ALL_ON)
                            If AllSelectFlg Then
                                sDispStr = Message.getMessage(CommonLabel.BUTTON_ALL_OFF)
                            End If
                        End Code
                        @<input Class="defButton" type="button" id="btnSelectAll" value="@sDispStr" onclick="btnSelectAll_onclick()">
                    End If
                </div>
                <div class="view-area-button-right">
                    @* 変更 *@
                    <input class="defButton" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="return btnChange_onclick()">
                    @* ロック *@
                    <input class="defButton" type="button" name="CsvExport" value="@Message.getMessage(CommonLabelInventoryInquiry.LABEL_LOCK)" onclick="return btnLock_onclick()">
                    @* ロック解除 *@
                    <input class="defButton" type="button" name="CsvExport" value="@Message.getMessage(CommonLabelInventoryInquiry.LABEL_UNLOCK)" onclick="return btnUnlock_onclick()">
                    @* CSV取込 *@
                    @*[CC431] UPD Start 20261002*@
                    <input class="defButton" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_DATA_IMPORT)" onclick="return btnCsvImport_onclick()">
                    @*[CC431] UPD End 20261002*@
                    @* CSV出力 *@
                    @*[CC431] UPD Start 20261002*@
                    <input class="defButton" type="button" name="CsvExport" value="@Message.getMessage(CommonLabel.BUTTON_DATA_EXPORT)" onclick="return btnCsvExport_onclick()">
                    @*[CC431] UPD End 20261002*@
                    @* 戻る *@
                    <input class="defButton" type="button" name="Return" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="location.href='@Url.Action("Index", "InventorySummary")'">
                </div>
            </div>
        </div>
        @* 検索条件 *@
        @<div Class="search-wrap">
            <a id="SearchCondition" data-toggle="collapse" href="#cond" aria-expanded="true" onclick="SearchCondition_onclick()">
                @Message.getMessage(CommonLabel.LABEL_SEARCH_CONDITION_OPEN)
            </a>
            <div class="search-border">
                <div id="cond" class="container collapse in">
                    <div class="row" id="item-conditions">
                        @* condition-flex：複数選択で項目の高さが伸びても段が崩れないよう flex で並べる（Content\Site.css） *@
                        <div class="col-md-12 condition-flex">
                            @* ロケーション 大区分 *@
                            <div class="col-sm-2 filter-combo-area" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_L)</div>
                                <div Class="grid-condition-col">
                                    @* select2-clearable クラスで入力して絞り込めるSelect2になる（Scripts\select2-common.js）。×で選択解除するため、空の選択肢を追加している *@
                                    @* ※棚卸対象のロケーション会社で固定のため非活性（検索条件の値は隠し項目 SearchLocL） *@
                                    @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLocLSelected, DirectCast(ViewBag.modelLocL, IEnumerable(Of SelectListItem)), "", New With {.onchange = "ChangeLocationL(this.value)", .class = "cond-control select2-clearable", .disabled = "disabled"})
                                </div>
                                @Html.HiddenFor(Function(model) model.FirstOrDefault.SearchLocL)
                            </div>
                            @* ロケーション 中区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                            <div class="col-sm-2 combo-area" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOC_M)</div>
                                <div Class="grid-condition-col">
                                    @* ListBoxFor で複数選択（select multiple）。select2-clearable クラスで×ボタン付きSelect2になる。空の選択肢は不要 *@
                                    @*[CC431] UPD Start 20261007*@
                                    @* 選択値を変えたら、場所から選ぶ OFF のとき場所の選択肢を作り直す *@
                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchLocM, DirectCast(ViewBag.modelLocM, IEnumerable(Of SelectListItem)), New With {.onchange = "onSearchLocChange('Loc', '" & LOC_DIV.LocationM & "')", .class = "cond-control select2-clearable"})
                                    @*[CC431] UPD End 20261007*@
                                </div>
                            </div>
                            @* ロケーション 小区分（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                            <div class="col-sm-2 combo-area" style="display: block">
                                @*[CC431] UPD Start 20261007*@
                                <div class="grid-head-col">
                                    @Message.getMessage(CommonLabel.LABEL_LOC_S)
                                    @* 「場所から選ぶ」チェックボックス（ON：場所で原価センタの選択肢を絞り込む）。中区分・小区分の両方を使用する場合だけ表示 *@
                                    @* head-check：見出し右端の緑のチェックボックス（Content\Site.css） *@
                                    @If ViewBag.LocUsage_M = Misc.ItemUse.Yes AndAlso ViewBag.LocUsage_S = Misc.ItemUse.Yes Then
                                        @<label class="head-check">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchLocReverse, New With {.onchange = "onSearchLocReverseChange('Loc')"})@Message.getMessage(CommonLabel.LABEL_SELECT_FROM_PLACE)</label>
                                    End If
                                </div>
                                <div class="grid-condition-col">
                                    @* ListBoxFor で複数選択（select multiple）。選択値を変えたら、場所から選ぶ ON のとき原価センタの選択肢を作り直す *@
                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchLocS, DirectCast(ViewBag.modelLocS, IEnumerable(Of SelectListItem)), New With {.onchange = "onSearchLocChange('Loc', '" & LOC_DIV.LocationS & "')", .class = "cond-control select2-clearable"})
                                </div>
                                @*[CC431] UPD End 20261007*@
                            </div>
                            @* カテゴリー 大分類（複数選択のため高さ固定の filter-combo-area ではなく combo-area） *@
                            <div class="col-sm-2 combo-area" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</div>
                                <div class="grid-condition-col">
                                    @* ListBoxFor で複数選択（select multiple）。select2-clearable クラスで×ボタン付きSelect2になる。空の選択肢は不要 *@
                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchCatL, DirectCast(ViewBag.modelCatL, IEnumerable(Of SelectListItem)), New With {.class = "cond-control select2-clearable"})
                                </div>
                            </div>
                            @* 実棚ステータス *@
                            <div style="display: block">
                                <div class="col-sm-3" style="min-width:355px; height: 56px;">
                                    <div class="grid-head-col">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_JITSUTANA_STATUS)</div>
                                    <div class="grid-condition-col">
                                        @*<div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchPhyinvStatusAll) @Message.getMessage(CommonLabel.LABEL_SELECT_ALL)</div>*@
                                        <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchPhyinvStatusN) @Message.getMessage(CommonLabel.VALUE_JITSUTANA_STATUS_SEIJO)</div>
                                        <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchPhyinvStatusW) @Message.getMessage(CommonLabel.VALUE_JITSUTANA_STATUS_ARI)</div>
                                        <div class="input-select-item-last" style="color:#ff0000;">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchPhyinvStatusE) @Message.getMessage(CommonLabel.VALUE_JITSUTANA_STATUS_NASHI)</div>
                                    </div>
                                </div>
                            </div>
                            @*[CC431] ADD Start 20261002*@
                            @* 実棚日（日付範囲。From のみ・To のみの指定も可） *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PHYINV_DATE_SEARCH)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchPhyinvDateFrom, New With {.class = "cond-control", .type = "date"})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchPhyinvDateTo, New With {.class = "cond-control", .type = "date"})
                                </div>
                            </div>
                            @* 実棚者コード *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_PHYINV_EMPLOYEE_CD)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchPhyinvEmployee, New With {.class = "cond-control", .maxlength = 15, .oninput = "editHanEi(this)"})
                                </div>
                            </div>
                            @* 棚卸実施端末 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_INV_TOOL_DIV)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchPhyinvToolDiv, DirectCast(ViewBag.PhyinvToolDivList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                            @*[CC431] ADD End 20261002*@
                            @* 資産番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NO)</div>
                                <div class="grid-condition-col-multi">
                                    @* 「|」区切りで複数入力可。×でクリア、複数ボタンで複数入力ダイアログを表示（Scripts\multi-input-dialog.js） *@
                                    <span class="clearable">
                                        @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetNo, New With {.class = "cond-control", .oninput = "editAssetNoMultiInput(this)"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                    @*[CC431] UPD Start 20261013 複数入力ダイアログの文字数（24）・文字種（editAssetNo）の制限を追加*@
                                    <button type="button" class="btn btn-default multi-input-button" data-multi-input="#SearchAssetNo" data-multi-title="@Message.getMessage(CommonLabel.LABEL_ASSET_NO)" data-multi-maxlength="24" data-multi-edit="editAssetNo">@Message.getMessage(CommonLabel.BUTTON_MULTI_INPUT)</button>
                                    @*[CC431] UPD End 20261013*@
                                </div>
                            </div>
                            @* 資産名称 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchAssetName, New With {.class = "cond-control", .maxlength = 60})
                                </div>
                            </div>
                            @* 有形・無形 *@
                            <div class="col-sm-2" style="display: block; height: 56px;">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_IS_TANGIBLE)</div>
                                <div class="grid-condition-col">
                                    <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchTangibleCheck)@Message.getMessage(CommonLabel.VALUE_TANGIBLE_ASSET)</div>
                                    <div class="input-select-item">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchNotTangibleCheck)@Message.getMessage(CommonLabel.VALUE_INTANGIBLE_ASSET)</div>
                                </div>
                            </div>
                            @* ロック担当者コード *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOCK_EMPLOYEE_CD)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchLockEmployee, New With {.class = "cond-control", .maxlength = 15, .oninput = "editHanEi(this)"})
                                </div>
                            </div>
                            @*[CC431] ADD Start 20261002*@
                            @* ロック資産ステータス（未：ロック担当者なし、済：ロック担当者あり） *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_LOCK_STATUS)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchLockStatus, DirectCast(ViewBag.LockStatusList, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                            @*[CC431] ADD End 20261002*@
                            @* 転用or除却 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("DIVERSION_REMOVE").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_DIVERSION_OR_REMOVE)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchDiversionRemove, DirectCast(ViewBag.DiversionRemoveOptionsModel, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                            @* 変更後稼働区分 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.CHANGED_OPE_STATUS)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                    @* ※選択肢の「-」は稼働区分が空のデータを検索する値のため、選択解除（空）とは別 *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchOpeStatus, DirectCast(ViewBag.OpeOptionsModel, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                            @* 除却時期 *@
                            <div class="col-sm-2" style="display:@(If(Model.FirstOrDefault.GridInfo("REMOVE_DATE").DispFlag = "1", "display", "None"))">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_REMOVE_DATE)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchRemoveDate, New With {.class = "cond-control", .type = "month"})
                                </div>
                            </div>
                            @* 除却理由 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_REMOVE_REASON)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchRemoveReason, DirectCast(ViewBag.RemoveReasonOptionsModel, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                            @* 除却内容 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_REMOVE_CONTENT)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchRemoveContent, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 変更後部課コード（複数選択のため combo-area） *@
                            <div class="col-sm-2 combo-area" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CHANGE_LOC_M_CD)</div>
                                <div Class="grid-condition-col">
                                    @* ListBoxFor で複数選択（select multiple）。select2-clearable クラスで×ボタン付きSelect2になる。空の選択肢は不要 *@
                                    @*[CC431] UPD Start 20261007*@
                                    @* ※選択肢は当該棚卸の棚卸資産Tの変更後課コード（権限は見ない）。選択値を変えたら、場所から選ぶ OFF のとき変更後設置場所の選択肢を作り直す *@
                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchChangeLocMCd, DirectCast(ViewBag.modelChangeLocM, IEnumerable(Of SelectListItem)), New With {.onchange = "onSearchLocChange('ChangeLoc', '" & LOC_DIV.LocationM & "')", .class = "cond-control select2-clearable"})
                                    @*[CC431] UPD End 20261007*@
                                </div>
                            </div>
                            @* 変更後設置場所コード（複数選択のため combo-area） *@
                            <div class="col-sm-2 combo-area" style="display: block">
                                @*[CC431] UPD Start 20261007*@
                                <div class="grid-head-col">
                                    @Message.getMessage(CommonLabel.LABEL_CHANGE_LOC_S_CD)
                                    @* 「場所から選ぶ」チェックボックス（ON：変更後設置場所で変更後課の選択肢を絞り込む）。中区分・小区分の両方を使用する場合だけ表示 *@
                                    @If ViewBag.LocUsage_M = Misc.ItemUse.Yes AndAlso ViewBag.LocUsage_S = Misc.ItemUse.Yes Then
                                        @<label class="head-check">@Html.CheckBoxFor(Function(model) model.FirstOrDefault.SearchChangeLocReverse, New With {.onchange = "onSearchLocReverseChange('ChangeLoc')"})@Message.getMessage(CommonLabel.LABEL_SELECT_FROM_PLACE)</label>
                                    End If
                                </div>
                                <div class="grid-condition-col">
                                    @* ListBoxFor で複数選択（select multiple）。選択値を変えたら、場所から選ぶ ON のとき変更後課の選択肢を作り直す *@
                                    @Html.ListBoxFor(Function(model) model.FirstOrDefault.SearchChangeLocSCd, DirectCast(ViewBag.modelChangeLocS, IEnumerable(Of SelectListItem)), New With {.onchange = "onSearchLocChange('ChangeLoc', '" & LOC_DIV.LocationS & "')", .class = "cond-control select2-clearable"})
                                </div>
                                @*[CC431] UPD End 20261007*@
                            </div>
                            @* 変更後設置社員番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_CHANGE_EMPLOYEE_CD)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchChangeEmployee, New With {.class = "cond-control", .maxlength = 15, .oninput = "{editHanEi(this)}"})
                                </div>
                            </div>
                            @* 貸与資産状態区分 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM10)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除（選択解除用に空の選択肢を追加） *@
                                    @* ※選択肢の「-」は貸与資産状態区分が空のデータを検索する値のため、選択解除（空）とは別 *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchFreeItem10, DirectCast(ViewBag.FreeItem10List, IEnumerable(Of SelectListItem)), "", New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                            @* 資本化日付 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)</div>
                                @*[CC431] UPD Start 20261002*@
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem2From, New With {.class = "cond-control", .type = "date"})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem2To, New With {.class = "cond-control", .type = "date"})
                                </div>
                                @*[CC431] UPD End 20261002*@
                            </div>
                            @* 稼働区分名 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM4)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem4, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 型式 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM5)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem5, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 管理社員番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6_CD)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem6, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* シリアルNo *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM7)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem7, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* WBS番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM8)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem8, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 備考 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM9)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem9, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 取得金額 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM11)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem11From, New With {.htmlAttributes = New With {.max = 1.0E+21, .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                    @Html.Raw("～")
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem11To, New With {.htmlAttributes = New With {.max = 1.0E+21, .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                </div>
                            </div>
                            @* 当月末簿価 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM12)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem12From, New With {.htmlAttributes = New With {.max = 1.0E+21, .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                    @Html.Raw("～")
                                    @Html.EditorFor(Function(model) model.FirstOrDefault.SearchFreeItem12To, New With {.htmlAttributes = New With {.max = 1.0E+21, .min = 0, .class = "numText cond-control", .type = "number", .step = "0.01"}})
                                </div>
                            </div>
                            @* 装置管理番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM14)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem14, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 資産グループコード *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM15)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem15, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 貸与先 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM16)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem16, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 貸与先名 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM17)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem17, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 開始日(貸与) *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)</div>
                                @*[CC431] UPD Start 20261002*@
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem18From, New With {.class = "cond-control", .type = "date"})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem18To, New With {.class = "cond-control", .type = "date"})
                                </div>
                                @*[CC431] UPD End 20261002*@
                            </div>
                            @* 終了日(貸与) *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)</div>
                                @*[CC431] UPD Start 20261002*@
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem19From, New With {.class = "cond-control", .type = "date"})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem19To, New With {.class = "cond-control", .type = "date"})
                                </div>
                                @*[CC431] UPD End 20261002*@
                            </div>
                            @* 貸与回数 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM20)</div>
                                <div class="grid-condition-col-asset">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem20From, New With {.class = "cond-control", .oninput = "editNumber(this)", .style = "text-align: right;", .maxlength = 2})
                                    ～
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem20To, New With {.class = "cond-control", .oninput = "editNumber(this)", .style = "text-align: right;", .maxlength = 2})
                                </div>
                            </div>
                            @* 備考欄 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM21)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem21, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 市区町村コード名称 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM39)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem39, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* CAPS番号 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM42)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem42, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 資産リース区分 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM37)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem37, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* タグ紐付状態 *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_TAG_LINK_STATUS)</div>
                                <div class="grid-condition-col-multi">
                                    @* 絞り込みなしの通常のプルダウン。×で選択解除 *@
                                    @* 選択肢の「全て」の値が空のため、選択解除用の空の選択肢は追加しない（×で「全て」に戻る。「全て」選択中は×を表示しない） *@
                                    <span class="clearable">
                                        @Html.DropDownListFor(Function(model) model.FirstOrDefault.SearchTagLinkStatus, DirectCast(ViewBag.TagLinkStatusList, IEnumerable(Of SelectListItem)), New With {.class = "cond-control"})
                                        <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                    </span>
                                </div>
                            </div>
                            @*[CC431] ADD Start 20261002*@
                            @* 棚卸ノート（部分一致） *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM27)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchFreeItem27, New With {.class = "cond-control", .maxlength = 255})
                                </div>
                            </div>
                            @* 入力作業者（前方一致） *@
                            <div class="col-sm-2" style="display: block">
                                <div class="grid-head-col">@Message.getMessage(CommonLabel.LABEL_INPUT_EMPLOYEE_CD)</div>
                                <div class="grid-condition-col">
                                    @Html.TextBoxFor(Function(model) model.FirstOrDefault.SearchEntEmployee, New With {.class = "cond-control", .maxlength = 15, .oninput = "editHanEi(this)"})
                                </div>
                            </div>
                            @*[CC431] ADD End 20261002*@
                        </div>
                    </div>
                    @*[CC431] ADD Start 20261002*@
                    @* 並び順（ラジオボタン17種。「項目の文言＋昇順の文言」で表示する。選択値は SearchSortOrder、Controller の GetSortColumn で ORDER BY に変換） *@
                    <div class="row" id="item-sort">
                        <div class="col-md-12">
                            <div class="grid-head-col sort-head-col">@Message.getMessage(CommonLabel.LABEL_SORT_ORDER)</div>
                            <div id="search-sort" class="grid-condition-col sort-condition-col" style="margin-left: 15px; margin-right: 15px; width: calc(100% - 30px);">
                                @Code
                                    Dim sSortAsc As String = Message.getMessage(CommonLabel.LABEL_SORT_ASC)
                                    Dim lstSort As New List(Of KeyValuePair(Of Integer, String)) From {
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.CatLAsc, Message.getMessage(CommonLabel.LABEL_CAT_L)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.AssetNoAsc, Message.getMessage(CommonLabel.LABEL_ASSET_NO)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.LocMAsc, Message.getMessage(CommonLabel.LABEL_LOC_M_DEP_CODE)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.LocSAsc, Message.getMessage(CommonLabel.LABEL_LOC_S_PLA_CODE)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.FreeItem2Asc, Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.FreeItem6Asc, Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6_CD)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.PhyinvEmployeeAsc, Message.getMessage(CommonLabel.LABEL_PHYINV_EMPLOYEE_CD)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark1Asc, Message.getMessage(CommonLabel.LABEL_REMARK1)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark2Asc, Message.getMessage(CommonLabel.LABEL_REMARK2)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark3Asc, Message.getMessage(CommonLabel.LABEL_REMARK3)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark4Asc, Message.getMessage(CommonLabel.LABEL_REMARK4)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark5Asc, Message.getMessage(CommonLabel.LABEL_REMARK5)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark6Asc, Message.getMessage(CommonLabel.LABEL_REMARK6)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark7Asc, Message.getMessage(CommonLabel.LABEL_REMARK7)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark8Asc, Message.getMessage(CommonLabel.LABEL_REMARK8)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark9Asc, Message.getMessage(CommonLabel.LABEL_REMARK9)),
                                        New KeyValuePair(Of Integer, String)(InventoryInquirySort.Remark10Asc, Message.getMessage(CommonLabel.LABEL_REMARK10))
                                    }
                                End Code
                                @For Each kvSort As KeyValuePair(Of Integer, String) In lstSort
                                    @<div class="input-select-item">
                                        @Html.RadioButtonFor(Function(model) model.FirstOrDefault.SearchSortOrder, kvSort.Key)@(kvSort.Value & sSortAsc)
                                    </div>
                                Next
                            </div>
                        </div>
                    </div>
                    @* 詳細検索エリア（備考1～備考10。複数テキスト入力「|」区切り、部分一致）。クリックで表示／非表示を切り替え、初期は非表示 *@
                    <div class="row">
                        <div class="col-md-12">
                            <a href="#" id="DetailAccordion" onclick="DetailAccordion_onclick();return false;">@Message.getMessage(CommonLabel.LABEL_ASSET_DETAIL_OPEN)</a>
                        </div>
                    </div>
                    <div class="row detail-accordion" id="item-conditions-detail" style="display:none;">
                        <div class="col-md-12 condition-flex">
                            @Code
                                Dim lstRemark As New List(Of KeyValuePair(Of String, String)) From {
                                    New KeyValuePair(Of String, String)("SearchRemark1", Message.getMessage(CommonLabel.LABEL_REMARK1)),
                                    New KeyValuePair(Of String, String)("SearchRemark2", Message.getMessage(CommonLabel.LABEL_REMARK2)),
                                    New KeyValuePair(Of String, String)("SearchRemark3", Message.getMessage(CommonLabel.LABEL_REMARK3)),
                                    New KeyValuePair(Of String, String)("SearchRemark4", Message.getMessage(CommonLabel.LABEL_REMARK4)),
                                    New KeyValuePair(Of String, String)("SearchRemark5", Message.getMessage(CommonLabel.LABEL_REMARK5)),
                                    New KeyValuePair(Of String, String)("SearchRemark6", Message.getMessage(CommonLabel.LABEL_REMARK6)),
                                    New KeyValuePair(Of String, String)("SearchRemark7", Message.getMessage(CommonLabel.LABEL_REMARK7)),
                                    New KeyValuePair(Of String, String)("SearchRemark8", Message.getMessage(CommonLabel.LABEL_REMARK8)),
                                    New KeyValuePair(Of String, String)("SearchRemark9", Message.getMessage(CommonLabel.LABEL_REMARK9)),
                                    New KeyValuePair(Of String, String)("SearchRemark10", Message.getMessage(CommonLabel.LABEL_REMARK10))
                                }
                                Dim dicRemarkValue As New Dictionary(Of String, String) From {
                                    {"SearchRemark1", Model.FirstOrDefault.SearchRemark1},
                                    {"SearchRemark2", Model.FirstOrDefault.SearchRemark2},
                                    {"SearchRemark3", Model.FirstOrDefault.SearchRemark3},
                                    {"SearchRemark4", Model.FirstOrDefault.SearchRemark4},
                                    {"SearchRemark5", Model.FirstOrDefault.SearchRemark5},
                                    {"SearchRemark6", Model.FirstOrDefault.SearchRemark6},
                                    {"SearchRemark7", Model.FirstOrDefault.SearchRemark7},
                                    {"SearchRemark8", Model.FirstOrDefault.SearchRemark8},
                                    {"SearchRemark9", Model.FirstOrDefault.SearchRemark9},
                                    {"SearchRemark10", Model.FirstOrDefault.SearchRemark10}
                                }
                            End Code
                            @For Each kvRemark As KeyValuePair(Of String, String) In lstRemark
                                @<div class="col-sm-2" style="display: block">
                                    <div class="grid-head-col">@kvRemark.Value</div>
                                    <div class="grid-condition-col-multi">
                                        @* 「|」区切りで複数入力可。×でクリア、複数ボタンで複数入力ダイアログを表示（Scripts\multi-input-dialog.js） *@
                                        <span class="clearable">
                                            @Html.TextBox(kvRemark.Key, dicRemarkValue(kvRemark.Key), New With {.class = "cond-control"})
                                            <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                                        </span>
                                        @*[CC431] UPD Start 20261013 複数入力ダイアログの文字数（255）の制限を追加*@
                                        <button type="button" class="btn btn-default multi-input-button" data-multi-input="#@kvRemark.Key" data-multi-title="@kvRemark.Value" data-multi-maxlength="255">@Message.getMessage(CommonLabel.BUTTON_MULTI_INPUT)</button>
                                        @*[CC431] UPD End 20261013*@
                                    </div>
                                </div>
                            Next
                        </div>
                    </div>
                    @*[CC431] ADD End 20261002*@
                    @* 検索ボタン *@
                    <div Class="view-area-label" style="height: 25px"></div>
                    <input Class="float-right defButton submit-button" name="actionname" type="submit" style="margin-top: 7px;" value="@Message.getMessage(CommonLabel.BUTTON_SEARCH)" onclick="return btnSearch_onclick()" />
                </div>
            </div>
        </div>
        @* 各数量表示 *@
        @<div Class="search-wrap">
            <table Class="detail-table" border="0">
                <tbody>
                    <tr Class="detail-row">
                        @*<td style="background-color:#ffffff; padding:3px; font-weight:bold; width:90px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_BOOK_QUANTITY) &nbsp;=</td>
                            <td class="quantity-col">@ViewBag.LogicInvNum</td> yamada*@
                        <td style="background-color:#ffffff; padding:3px; font-weight:bold; width:200px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_BOOK_QUANTITY) &nbsp;=&nbsp; @ViewBag.LogicInvNum</td>
                        @*<td style="background-color:#ffffff; padding:3px; font-weight:bold; width:70px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_NORMAL_QUANTITY) &nbsp;=</td>
                            <td class="quantity-col">@ViewBag.PhyinvNum</td> yamada*@
                        <td style="background-color:#ffffff; padding:3px; font-weight:bold; width:230px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_NORMAL_QUANTITY) &nbsp;=&nbsp; @ViewBag.PhyinvNum</td>
                        @*<td style="background-color:#ffffff; padding:3px; font-weight:bold; width:90px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_LOCATION_DIFF_QUANTITY) &nbsp;=</td>
                            <td class="quantity-col">@ViewBag.LocDiffNum</td> yamada*@
                        <td style="background-color:#ffffff; padding:3px; font-weight:bold; width:280px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_LOCATION_DIFF_QUANTITY) &nbsp;=&nbsp; @ViewBag.LocDiffNum</td>
                        @*<td style="background-color:#ffffff; padding:3px; font-weight:bold; width:90px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_NONE_QUANTITY) &nbsp;=</td>
                            <td class="quantity-col">@ViewBag.NoItemNum</td> yamada*@
                        <td style="background-color:#ffffff; padding:3px; font-weight:bold; width:270px;">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_NONE_QUANTITY) &nbsp;=&nbsp; @ViewBag.NoItemNum</td>
                    </tr>
                </tbody>
            </table>
        </div>

        @*ページャー表示*@
        @PagerHelper.Pager(Me.Html, Model.FirstOrDefault.Pager, Message)
        @*@Html.Label("選択件数：", New With {.style = "font-size:12px;"}) yamada*@
        @Html.Label(Message.getMessage(CommonLabel.LABEL_SELECT_COUNT), New With {.style = "font-size:12px;"})
        @Code
            Dim CheckedCount As String = "0"
            If Not IsNothing(_lstCheckedKey) Then
                CheckedCount = _lstCheckedKey.Count.ToString
            End If
            @Html.Label(CheckedCount, New With {.id = "selectcount", .style = "margin-right:15px;font-size:12px;"})
        End Code

        @<div class="list-view">
            @* 高さはヘッダー固定の処理（enableFixedHeader）で画面の残り領域に合わせる *@
            <div id="ListGrids" style="height:auto;margin-bottom: 20px;" />
        </div>


        '実棚紹介詳細のモーダル画面
        @<div id="modal-content" class="modal-assetlist" style="width: 1550px; z-index: 100">
            <div class="popup-title-wrap">
                <p class="Line3"></p>
                <p class="Line4"></p>
                <h2 id="popup-title"></h2>
                <p class="Line3"></p>
                <p class="Line4"></p>
            </div>
            @CODE
                'モデルの値を設定
                Dim SessionModel As InventoryInquiryViewModel = Session(CommonSessionKey.CREAGE_ASSET_LIST)
                If Not IsNothing(SessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(SessionModel)
                Else
                    Model.Add(New InventoryInquiryViewModel())
                End If
            END CODE
            @Html.AntiForgeryToken()

            <Table>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_STATUS)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.ASSET_STATUS_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col" style="padding: 3px 0px;">@Message.getMessage(CommonLabel.LABEL_LOCK_EMPLOYEE_CD_NAME)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.LOCK_EMPLOYEE_CD, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_PHYINV_STATUS)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.PHYINV_STATUS_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                </tr>
            </Table>
            <Table>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NO)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.ASSET_NO, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_SUB_ASSET_NO)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.SUB_ASSET_NO, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_NAME)</td>
                    @*[CC431] UPD Start 20261008*@
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.ASSET_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    @*[CC431] UPD End 20261008*@
                </tr>
            </Table>

            <Table>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_PHYINV_DATE)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.PHYINV_TS, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True, .type = "text"}})</td>

                    <td class="head-col">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_PHYINV_PERSON_NAME)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.PHYINV_EMPLOYEE_CD, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM6)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM6, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                </tr>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_CAT_L)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.CAT_L_CD_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_IS_TANGIBLE)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.IS_TANGIBLE, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    @*[CC431] ADD Start 20261008*@
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_INPUT_EMPLOYEE_CD_NAME)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.ENT_EMPLOYEE_CD, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_INV_TOOL_DIV)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.PHYINV_TOOL_DIV, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    @*[CC431] ADD End 20261008*@
                </tr>
            </Table>

            <div style="height: 165px; overflow-y:auto; overflow-x: hidden;">
                <Table>
                    <tr Class="popup-row">
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM1)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM1, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM2)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM2, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM3)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM3, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM4)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM4, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM5)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM5, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM7)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM7, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM8)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM8, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM9)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM9, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM10)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM10, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM11)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM11, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM12)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM12, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM13)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM13, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM14)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM14, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM15)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM15, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM16)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM16, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM17)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM17, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM18)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM18, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM19)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM19, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM20)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM20, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM21)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM21, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM22)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM22, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM23)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM23, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM24)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM24, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM25)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM25, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM26)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM26, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM27)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM27, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM28)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM28, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM29)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM29, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM30)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM30, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM31)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM31, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM32)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM32, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM33)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM33, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM34)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM34, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM35)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM35, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM36)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM36, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM37)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM37, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM38)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM38, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM39)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM39, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM40)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM40, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM41)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM41, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM42)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM42, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM43)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM43, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM44)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM44, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM45)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM45, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM46)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM46, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM47)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM47, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM48)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM48, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM49)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM49, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM50)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM50, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM51)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM51, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM52)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM52, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM53)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM53, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM54)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM54, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM55)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM55, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM56)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM56, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM57)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM57, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM58)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM58, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                        <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM59)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM59, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>
                    <tr Class="popup-row">
                        <td class="head-col">@Message.getMessage(CommonLabel.LABEL_ASSET_FREE_ITEM60)</td>
                        <td Class="value-col">@Html.EditorFor(Function(model) model.Last.FREE_ITEM60, New With {.htmlAttributes = New With {.class = "inventory-disable-input half-width", .readOnly = True}})</td>
                    </tr>

                </Table>
            </div>
            <Table>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_DIVERSION_OR_REMOVE)</td>
                    <td Class="value-col">
                        @* 絞り込みなしの通常のプルダウン。×で選択解除（選択肢と選択解除用の空の選択肢は JS で追加） *@
                        @* clearable-fill：下の行（実棚会社など）の入力欄と同じく枠いっぱいの幅にする *@
                        <span class="clearable clearable-fill">
                            <SELECT id="DIVERSION_REMOVE" class='editor-select'></SELECT>
                            <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                        </span>
                    </td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_OPE_STATUS)</td>
                    @* value-col-wide：×ボタンの分、選択肢が切れないように5文字分広げる *@
                    <td Class="value-col value-col-wide">
                        <span class="clearable clearable-fill">
                            <SELECT id="OPE_STATUS" class='editor-select'></SELECT>
                            <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                        </span>
                    </td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_REMOVE_DATE)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMOVE_DATE, New With {.htmlAttributes = New With {.class = "", .type = "month"}})</td>
                </tr>
            </Table>
            <Table>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_REMOVE_REASON)</td>
                    <td Class="value-col">
                        <span class="clearable">
                            <SELECT id="REMOVE_REASON" class='editor-select'></SELECT>
                            <span class="clearable-clear" title="@Message.getMessage(CommonLabel.LABEL_TOOLTIP_CLEAR)">×</span>
                        </span>
                    </td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_REMOVE_CONTENT)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMOVE_CONTENT, New With {.htmlAttributes = New With {.style = "width: 880px", .maxlength = "255"}})</td>
                </tr>
            </Table>

            <Table>
                <tr Class="popup-row">
                    <td Class="head-col" style="display: flex; justify-content: space-between"><span>@Message.getMessage(CommonLabel.LABEL_GEN_LOCATION)</span><span>@Message.getMessage(CommonLabel.VALUE_LOC_L_COM)</span></td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.LOC_L_CD_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col" style="display: flex; justify-content: space-between"><span>@Message.getMessage(CommonLabelInventorySummary.LABEL_REAL_SHELF)</span><span>@Message.getMessage(CommonLabel.VALUE_LOC_L_COM)</span></td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.PHYINV_LOC_L_CD_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_CHANGE_LOC_M)</td>
                    <td Class="value-col" style="width: 500px;">
                        @* 絞り込み機能付きの単一選択（select2-clearable）。選択肢・選択後とも「コード 名称」を表示 *@
                        @* 選択肢は検索条件の変更後課コードと同じ（locationMList）。モーダルを開く時に JS（setModalChangeLoc）で設定する *@
                        <select id="CHANGE_LOC_M_CD" name="CHANGE_LOC_M_CD" class="select2-clearable" style="width: 480px;"><option value=""></option></select>
                    </td>
                </tr>
                <tr Class="popup-row">
                    <td Class="head-col" style="text-align: right;">@Message.getMessage(CommonLabel.VALUE_LOC_M_DEP)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.LOC_M_CD_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col" style="text-align: right;">@Message.getMessage(CommonLabel.VALUE_LOC_M_DEP)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.PHYINV_LOC_M_CD_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_CHANGE_LOC_S)</td>
                    <td Class="value-col" style="width: 500px;">
                        @* 絞り込み機能付きの単一選択（select2-clearable）。選択肢・選択後とも「コード 名称」を表示 *@
                        @* 選択肢は検索条件の変更後設置場所コードと同じ（locationSList を変更後課コードで絞り込み）。変更後課コードの変更時に入れ替える *@
                        <select id="CHANGE_LOC_S_CD" name="CHANGE_LOC_S_CD" class="select2-clearable" style="width: 480px;"><option value=""></option></select>
                    </td>
                </tr>
                <tr Class="popup-row">
                    <td Class="head-col" style="text-align: right;">@Message.getMessage(CommonLabel.VALUE_LOC_S_PLA)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.LOC_S_CD_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col" style="text-align: right;">@Message.getMessage(CommonLabel.VALUE_LOC_S_PLA)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.PHYINV_LOC_S_CD_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .readOnly = True}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabelInventoryInquiry.LABEL_CHANGE_EMPLOYEE)</td>
                    <td Class="value-col" style="width: 500px;">
                        @Html.EditorFor(Function(model) model.Last.CHANGE_EMPLOYEE, New With {.htmlAttributes = New With {.class = "", .maxlength = "15", .oninput = "editHanEi(this)"}})
                        @Html.EditorFor(Function(model) model.Last.CHANGE_EMPLOYEE_NAME, New With {.htmlAttributes = New With {.class = "inventory-disable-input", .style = "width: 225px", .readOnly = True}})
                    </td>
                </tr>
            </Table>

            <Table>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_REMARK1)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMARK1, New With {.htmlAttributes = New With {.class = "half-width", .maxlength = "255"}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_REMARK2)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMARK2, New With {.htmlAttributes = New With {.class = "half-width", .maxlength = "255"}})</td>
                </tr>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_REMARK3)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMARK3, New With {.htmlAttributes = New With {.class = "half-width", .maxlength = "255"}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_REMARK4)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMARK4, New With {.htmlAttributes = New With {.class = "half-width", .maxlength = "255"}})</td>
                </tr>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_REMARK5)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMARK5, New With {.htmlAttributes = New With {.class = "half-width", .maxlength = "255"}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_REMARK6)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMARK6, New With {.htmlAttributes = New With {.class = "half-width", .maxlength = "255"}})</td>
                </tr>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_REMARK7)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMARK7, New With {.htmlAttributes = New With {.class = "half-width", .maxlength = "255"}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_REMARK8)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMARK8, New With {.htmlAttributes = New With {.class = "half-width", .maxlength = "255"}})</td>
                </tr>
                <tr Class="popup-row">
                    <td Class="head-col">@Message.getMessage(CommonLabel.LABEL_REMARK9)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMARK9, New With {.htmlAttributes = New With {.class = "half-width", .maxlength = "255"}})</td>
                    <td class="head-col">@Message.getMessage(CommonLabel.LABEL_REMARK10)</td>
                    <td Class="value-col">@Html.EditorFor(Function(model) model.Last.REMARK10, New With {.htmlAttributes = New With {.class = "half-width", .maxlength = "255"}})</td>
                </tr>

            </Table>
            <div Class="PopupButton" style="display:flex;justify-content:center;gap:220px;margin-top:15px;">
                <input id="modal-create-btn" class="defButton modal-create-btn submit-button"
                       style="padding:8px 20px;font-size:14px;cursor:pointer;"
                       type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_CHANGE)" onclick="return btnUpdate_onclick()">

                <input id="modal-close" class="defButton modal-close"
                       style="padding:8px 20px;font-size:14px;cursor:pointer;"
                       type="submit" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
            </div>
        </div>


        'CSV取込のモーダル画面
        @<div id="modal-csv-content">
            <div class="popup-title-wrap">
                <p class="Line3"></p>
                <p class="Line4"></p>
                <h2 id="popup-title-csv"></h2>
                <p class="Line3"></p>
                <p class="Line4"></p>
            </div>
            @CODE
                'モデルの値を設定
                Dim csvSessionModel As InventoryInquiryViewModel = Session(CommonSessionKey.CREAGE_CATEGORY)
                If Not IsNothing(csvSessionModel) AndAlso Model(0).ErrCode <> String.Empty Then
                    Model.Add(csvSessionModel)
                Else
                    Model.Add(New InventoryInquiryViewModel())
                End If
            END CODE
            @*@Html.AntiForgeryToken()*@
            <div Class="PopupButton clearfix mb10">
                <div class="float-left">
                    <input type="file" name="CsvFile" id="CsvFile" />
                    @*[CC431] UPD Start 20261008*@
                    <div style="color:red;">@Message.getMessage(CommonLabelInventoryInquiry.MSG_INFO_CSV_IMPORT_DATA_UPDATE)</div>
                    @*[CC431] UPD End 20261008*@
                </div>
                <div class="float-right">
                    @*[CC431] UPD Start 20261008*@
                    <input id="modal-import-btn" class="defButton CsvImport" style="width:120px" type="button" name="ButtonName" value="@Message.getMessage(CommonLabel.BUTTON_DATA_IMPORT)" onclick="btnCsvImportStart_onclick()">
                    @*[CC431] UPD End 20261008*@
                </div>
            </div>

            <div class="ichiran clearfix mb10">
                <div id="div_koumoku_h">
                    <table class="ich_table" id="csv_tbl_koumoku_h">
                        <tbody>
                            <tr>
                                <th class="col1">@Message.getMessage(CommonLabel.LABEL_ROW)</th>
                                <th class="col2">@Message.getMessage(CommonLabel.LABEL_DATA)</th>
                                <th class="col3">@Message.getMessage(CommonLabel.LABEL_ERRORGAIYOU)</th>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div style="height:133px; overflow-y:scroll;">
                    <div id="div_koumoku_d">
                        <table class="ich_table" id="csv_tbl_koumoku_d">
                            <tbody id="csv-err-ichiran">
                                @*エラーグリッド一覧表示*@
                                @for icnt As Integer = 1 To DataInport.DISP_ROW_COUNT
                                    @<tr><td Class="col1">　</td><td class="col2">　</td><td class="col3">　</td></tr>
                                Next
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>

            <div id="modal-msg" class="mb10">

            </div>

            <div Class="PopupButton float-right">
                <input id="csvimport-modal-close" class="defButton modal-close" type="submit" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)" onclick="fileNmClear()">
            </div>
        </div>


        'QR画面表示のモーダル画面
        @<div id="modal-qr-content" style="text-align:center">
            <div id="QRAssetNo" class="mb10">
            </div>
            <img id="qrview" src='' width="150" height="150">
            <div class="text-center">
                <input id="modal-close" class="defButton modal-close" style="margin:0 auto;" type="button" value="@Message.getMessage(CommonLabel.BUTTON_RETURN)">
            </div>
        </div>

        'ローディング画像引き渡し用隠し項目
        @<input id="LoadingImg" name="LoadingImg" type="hidden">

        @Html.HiddenFor(Function(model) model.FirstOrDefault.PageNum)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OnCreteButton)
        @Html.HiddenFor(Function(model) model.FirstOrDefault.TotalCount)
        @*[CC431] ADD Start 20261002*@
        @* 詳細検索エリアの開閉状態。ポストバック時も開閉状態を維持するため hidden で往復させる *@
        @Html.HiddenFor(Function(model) model.FirstOrDefault.DetailOpenFlg)
        @*[CC431] ADD Start 20261005*@
        @Html.HiddenFor(Function(model) model.FirstOrDefault.OpenFlg)
        @*[CC431] ADD End 20261005*@
        @*[CC431] ADD End 20261002*@

        @*@Html.HiddenFor(Function(model) model.FirstOrDefault.SelectAssetNo)*@
        @<input id="SelectKeys" name="SelectKeys" type="hidden">
        @<input id="NoneSelectKeys" name="NoneSelectKeys" type="hidden">
        @<input id="EditedContainListJson" name="EditedContainListJson" type="hidden">
        @<input id="EditJson" name="EditJson" type="hidden">
    End Using
</div>
