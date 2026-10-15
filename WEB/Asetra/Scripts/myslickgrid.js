/**
 Slickgridの共通関数群です。
 */

// SlickGrid初期化処理
function initializeGrid(gridId, data, columnDefinitions, gridOptions, dispDiv, fixColCnt) {
    let gridElement = document.getElementById(gridId);

    // 実際に表示する列のみをフィルタリング
    let columns = columnDefinitions.filter(function (column) {
        // visible が false または 0 の場合に非表示
        return column.visible !== false && column.visible !== '0';
    });

    // ソート設定
    columns = sortColumnsByOrder(columns);

    // 最初の列にCSSを適用
    if (columns[0]) {
        // cssClassプロパティが存在し、かつ'first-column'クラスが含まれていない場合に追加
        if (columns[0].cssClass) {
            if (!columns[0].cssClass.includes('first-column')) {
                columns[0].cssClass += ' first-column';
            }
        } else {
            // cssClassプロパティが存在しない場合は新しく作成
            columns[0].cssClass = 'first-column';
        }
    }

    // Grid共通設定
    Object.assign(gridOptions, {
        enableTextSelectionOnCells: true,               // セル内のテキスト選択を有効
    });

    // 列固定有無の判定
    if (isNumber(fixColCnt)) {
        let fixColumn = Number(fixColCnt)
        if (fixColumn > 0) {
            //列固定あり
            Object.assign(gridOptions, { frozenColumn: fixColumn - 1});
        }
    }

    // 表示する列幅の合計値を取得
    let totalWidth = getColumnTotalWidth(columns);

    // 列幅の合計を計算し、グリッドのコンテナに適用
    if (gridId == 'LendingStatusListGrids' || gridId == 'TemporaryListGrids' || gridId == 'ReserveListGrids' || gridId == 'AssetReserveListGrids') {
    } else if (gridOptions.hasOwnProperty('frozenColumn')) {
        //固定列あり
        let gridWidth = gridElement.clientWidth;

        // 列幅の合計値 < グリッドの横幅
        if (totalWidth < gridWidth) {
            //列の幅をグリッドに合わせて自動調整
            Object.assign(gridOptions, { forceFitColumns: true });
        }
    } else {
        // 固定列がない場合のみ、列幅の合計値をグリッドに適用
        gridElement.style.width = totalWidth + 'px';
    }

    // SlickGridの初期化
    let grid = new Slick.Grid("#" + gridId, data, columns, gridOptions);

    // オリジナルのgrid.getColumnsを保存
    // ※プラグインの追加によるメソッドのオーバーライド対策
    let originalGetColumns = grid.getColumns;

    // 列幅変更イベントの処理
    grid.onColumnsResized.subscribe(function (e, args) {
        // 全ての列幅を取得して、Gridの横幅に適用
        let currentColumns = originalGetColumns.call(grid);
        let totalWidth = getColumnTotalWidth(currentColumns);
        gridElement.style.width = totalWidth + 'px';

        //ローカルストレージに列幅を保存
        for (let i = 0; i < currentColumns.length ; i++) {
            localStorage.setItem(dispDiv + currentColumns[i].id, currentColumns[i].width)

            if (currentColumns[i].children) {
                // 子となる列が存在する場合、各列幅を保存
                currentColumns[i].children.forEach(subColumn => {
                    localStorage.setItem(dispDiv + subColumn.id, subColumn.width)
                });
            }
        }
    });

    // マウスポインタ侵入イベント
    grid.onMouseEnter.subscribe(function (e, args) {
        let cell = grid.getCellFromEvent(e);
        if (!cell) {
            return;
        }

        let row = cell.row;
        let col = cell.cell;
        let dataItem = grid.getDataItem(row);
        if (!dataItem) {
            return;
        }

        let value = dataItem[grid.getColumns()[col].field];

        // セルのDOM要素を取得
        let cellNode = grid.getCellNode(row, col);
        if (cellNode) {
            // セルのDOM要素から直接テキストを取得
            let displayedValue = cellNode.textContent || cellNode.innerText;

            // 取得したテキストをtitle属性に設定
            cellNode.setAttribute('title', displayedValue);
        }
    });

    // ダブルクリックイベントのハンドラを追加
    grid.onDblClick.subscribe(function (e, args) {
        // ダブルクリックされたセルの要素を取得
        let cell = grid.getCellNode(args.row, args.cell);
        if (cell) {
            let selection = window.getSelection();                  // 現在の選択範囲を示すオブジェクト
            let range = document.createRange();                     // 選択状態にする範囲を示すオブジェクト
            // ダブルクリックされたセルの値を全選択状態にする
            range.selectNodeContents(cell);
            selection.removeAllRanges();
            selection.addRange(range);
        }
    });

    return grid;
}

// 列幅の合計を計算する関数
function getColumnTotalWidth(columns) {
    // reduceを使用して列の合計幅を計算
    return columns.reduce(function (sum, column) {
        // 子列がある場合は、子列の幅の合計値を追加
        if (column.children && column.children.length > 0) {
            return sum + getColumnTotalWidth(column.children);
        }
        // 子列がない場合は、その列の幅を合計に追加
        return sum + column.width;
    }, 0);
}

// ローカルストレージから列幅を取得
function getColumnWidth(dispDiv, colKey, defaultVal) {
    let key = dispDiv + colKey;

    if (!localStorage.hasOwnProperty(key)) {
        // キーが存在しなければデフォルト値を返す
        return defaultVal;
    }

    let colValue = localStorage.getItem(key)
    if (isNaN(colValue)) {
        // 数値でなければデフォルト値
        return defaultVal;
    }

    return Number(colValue);
}

// ソート番号の昇順で配列を並び替え
function sortColumnsByOrder(columnsArray) {
    // sortOrderが定義されている列があるかどうかをチェック
    const hasSortOrder = columnsArray.some(column => column.sortOrder !== undefined);

    // もしsortOrderプロパティがある列があれば、それに基づいてソート
    if (hasSortOrder) {
        return columnsArray.sort((a, b) => {
            let orderA = a.sortOrder !== undefined ? a.sortOrder : Number.MAX_VALUE;
            let orderB = b.sortOrder !== undefined ? b.sortOrder : Number.MAX_VALUE;
            return orderA - orderB;
        });
    }

    // sortOrderがなければ、元の配列をそのまま返す
    return columnsArray;
}

/***********************************
* カスタムUI部品
************************************/

// チェックボックス(中央寄せ)
// ※各引数はSlickGridから自動で引き渡しされる
function CheckboxFormatter(row, cell, value, columnDef, dataContext) {
    return '<div style="text-align:center; height:100%;">' +
           '<input type="checkbox" class="slick-checkbox" ' + (value == 1 ? 'checked="checked"' : '') + ' />' +
           '</div>';
}

// テキスト表示(右寄せ)
function RightAlignFormatter(row, cell, value, columnDef, dataContext) {
    return '<div style="text-align:right; width:100%;">' + value + '</div>';
}

// テキスト表示(中央寄せ)
function CenterAlignFormatter(row, cell, value, columnDef, dataContext) {
    return '<div style="text-align:center; width:100%;">' + value + '</div>';
}

// テキストボックス表示(左寄せ)
function EditLeftAlignFormatter(row, cell, value, columnDef, dataContext) {
    value = value || ""; // nullまたはundefinedの値を空文字列に置き換える
    return '<div class="editable-cell" style="text-align:left; width:100%;">' + value + '</div>';
}

// テキストボックス表示(右寄せ)
function EditRightAlignFormatter(row, cell, value, columnDef, dataContext) {
    value = value || ""; // nullまたはundefinedの値を空文字列に置き換える
    return '<div class="editable-cell" style="text-align:right; width:100%;">' + value + '</div>';
}

/***********************************
* 汎用
************************************/
// 数値変換チェック(文字列数字も対応)
function isNumber(value) {
    if (value == null) return false;
    const number = parseFloat(value);
    return !isNaN(number) && number.toString() === value.toString();
}


/***********************************
* カスタムクラス定義
************************************/
// カスタムテキストエディタのクラス定義
function CustomTextEditor(args) {
    var $input;
    var defaultValue;
    var scope = this;

    // エディタを初期化する
    this.init = function () {
        // DOM要素（テキスト入力）を作成し、セルのコンテナに追加する
        $input = $("<INPUT type=text class='editor-text' />")
          .appendTo(args.container)
          .on("keydown.nav", function (e) {
              // フォーカスされている時のみ矢印キーによるカーソル移動を許可する
              if (e.keyCode !== $.ui.keyCode.LEFT && e.keyCode !== $.ui.keyCode.RIGHT) {
                  e.stopImmediatePropagation();
              }
          })
          .focus()
          .select();
    };

    // エディタを破棄する　※DOMから削除
    this.destroy = function () {
        // DOMから入力要素を削除する
        $input.remove();
    };

    // エディタにフォーカスを設定する
    // ※キーボード入力を開始するために必要
    this.focus = function () {
        $input.focus();
    };

    // 現在の入力値を取得する
    // ※グリッドがエディタの現在地を必要とする際に使用される
    this.getValue = function () {
        return $input.val();
    };

    // 入力値を設定する
    // グリッドがエディタにデータを設定する際に使用される
    this.setValue = function (val) {
        $input.val(val);
    };

    // グリッドに表示されている値をロードしてエディタに設定する
    this.loadValue = function (item) {
        defaultValue = item[args.column.field] || "";
        $input.val(defaultValue);
        $input[0].defaultValue = defaultValue;
        $input.select();
    };

    // エディタの値をシリアライズして、グリッドのデータモデルに保存する
    this.serializeValue = function () {
        return $input.val();
    };

    // 入力値をデータ項目に適用する
    // ユーザーが編集を完了した際、変更をグリッドのデータモデルに反映する際に使用される
    this.applyValue = function (item, state) {
        item[args.column.field] = state;
    };

    // 値が変更されたかどうかを確認する
    this.isValueChanged = function () {
        return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
    };

    // 入力値の検証を行う
    this.validate = function () {
        // カラム定義に入力チェック関数が設定されていればその関数を実行
        if (args.column.validator) {
            var validationResults = args.column.validator($input.val());
            if (!validationResults.valid) {
                return validationResults;
            }
        }

        return {
            valid: true,
            msg: null
        };
    };

    // エディタを初期化する
    this.init();
}

// ヘッダー固定の設定
// ※Grid初期化時は autoHeight: false を指定すること
function enableFixedHeader(grid, gridId) {
    let gridElement = document.getElementById(gridId);

    let refresh = function () {
        // 横幅は画面に合わせ、横スクロールもグリッド内で行う
        gridElement.style.width = '100%';
        fitGridHeight(grid, gridElement);
        grid.resizeCanvas();
    };

    // 列幅変更時に共通処理が横幅を列幅合計に戻すため、その後に再設定する
    grid.onColumnsResized.subscribe(refresh);

    // ウィンドウサイズ変更、検索条件の開閉で上部の高さが変わった場合に再計算
    $(window).on('resize load', refresh);
    $("#cond").on('shown.bs.collapse hidden.bs.collapse', refresh);

    refresh();
}

// グリッドの高さを画面の残り領域に合わせる
// ※データ件数分の高さを上限とする（件数が少ない場合に余白を出さない）
function fitGridHeight(grid, gridElement) {
    const minHeight = 200;          // 最小の高さ
    const bottomMargin = 20;        // 画面下端との余白
    const scrollbarHeight = 20;     // 横スクロールバー分

    let top = gridElement.getBoundingClientRect().top + window.pageYOffset;
    let height = Math.max(minHeight, window.innerHeight - top - bottomMargin);

    let header = gridElement.querySelector('.slick-header');
    let headerHeight = header ? header.offsetHeight : 0;
    let contentHeight = headerHeight + grid.getOptions().rowHeight * grid.getDataLength() + scrollbarHeight;

    gridElement.style.height = Math.min(height, contentHeight) + 'px';
}
