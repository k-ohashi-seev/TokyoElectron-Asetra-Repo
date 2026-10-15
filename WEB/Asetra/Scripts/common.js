/********************************
 * 確認ダイアログの表示
 *********************************/
function showConfirm(strMessage, strActionName) {

    // ダイアログのメッセージを設定
    strMessage = strMessage.replace("@@CR@@", "<BR>");   //　2020/02/27 nv miyaji add
    $("#show-dialog").html(strMessage);

    // ダイアログを作成
    $("#show-dialog").dialog({
        modal: true,
        title: strConfirmTitle,
        dialogClass: "show-confirm-wrap",
        buttons: {
            "OK": function () {
                var befAction = $('#form').attr("action");
                $(this).dialog("close");
                $('#form').attr("action", strActionName);
                $('#form').submit();
                $('#form').attr("action", befAction);
                return true;
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

/********************************
 * 確認ダイアログの表示(ローディング表示)
 *********************************/
function showConfirmShowLoading(strMessage, strActionName) {

    // ダイアログのメッセージを設定
    strMessage = strMessage.replace("@@CR@@", "<BR>");   //　2020/02/27 nv miyaji add
    $("#show-dialog").html(strMessage);

    // ダイアログを作成
    $("#show-dialog").dialog({
        modal: true,
        title: strConfirmTitle,
        dialogClass: "show-confirm-wrap",
        buttons: {
            "OK": function () {
                // ローディング表示
                showLoading();

                var befAction = $('#form').attr("action");
                $(this).dialog("close");
                $('#form').attr("action", strActionName);
                $('#form').submit();
                $('#form').attr("action", befAction);
                return true;
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

/********************************
 * 確認ダイアログの表示（呼び出し側でOKを押した際のイベントを定義可能）
 *********************************/
function showConfirmOkEvt(strMessage) {

    // ダイアログのメッセージを設定
    strMessage = strMessage.replace("@@CR@@", "<BR>");

    return new Promise(function (resolve, reject) {

        $("#show-dialog").html(strMessage);

        // ダイアログを作成
        $("#show-dialog").dialog({
            modal: true,
            title: strConfirmTitle,
            dialogClass: "show-confirm-wrap",
            buttons: {
                "OK": function () {
                    $(this).dialog("close");
                    resolve();
                },
                "Cancel": function () {
                    $(this).dialog("close");
                    reject();
                }
            },
            open: function () {      // キャンセルボタンにフォーカスをあてる
                $(this).siblings('.ui-dialog-buttonpane').find('button:eq(1)').focus();
            }
        });

    });
}

/********************************
 * 確認ダイアログの表示(ファイルダウンロード用)
 *********************************/
function showConfirmFileDownload(strMessage, strActionName) {

    // ダイアログのメッセージを設定
    strMessage = strMessage.replace("@@CR@@", "<BR>");   //　2020/02/27 nv miyaji add
    $("#show-dialog").html(strMessage);

    // フォームを取得
    var $form = $('#form');

    // ダイアログを作成
    $("#show-dialog").dialog({
        modal: true,
        title: strConfirmTitle,
        dialogClass: "show-confirm-wrap",
        buttons: {
            "OK": function () {
                // ダイアログを閉じる
                $(this).dialog("close");

                // ローディング表示
                showLoading();

                // 変更前に退避
                var befAction = $form.attr("action");
                var befTarget = $form.attr("target");

                // ダウンロード用に target を iframe に変更
                $form.attr("target", "downloadFrame");

                // ダウンロード完了待ち処理
                waitDownloadComplete($form, befTarget);
                               
                // アクションを変更
                $form.attr("action", strActionName);

                // フォーム送信
                $form.submit();

                // 元のアクションに戻す
                $form.attr("action", befAction);
            },
            "Cancel": function () {
                // キャンセル時
                $(this).dialog("close");
            }
        },
        open: function () {
            // キャンセルボタンにフォーカスをあてる
            $(this).siblings('.ui-dialog-buttonpane').find('button:eq(1)').focus();
        }
    });
}

//ダウンロード完了待ち処理
function waitDownloadComplete($form, befTarget) {
    // ポーリングして完了を検知
    var interval = setInterval(function () {
        if (document.cookie.indexOf("fileDownloadToken=done") !== -1) {
            // Cookieが見つかった場合
            clearInterval(interval);

            // Cookieを削除
            document.cookie = "fileDownloadToken=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/";

            // target を元に戻す
            if (befTarget) {
                $form.attr("target", befTarget);
            } else {
                $form.removeAttr("target"); 
            }

            // ローディングを消す
            hideLoading();
        }
    }, 500);
}

/********************************
 * ログインセッションが切れている時のエラーメッセージの表示
 *********************************/
function showLoginMessage(strMessage, strActionName) {

    // ダイアログのメッセージを設定
    $("#show-message").html(strMessage);

    // ダイアログを作成
    $("#show-message").dialog({
        modal: true,
        title: strConfirmTitle,
        dialogClass: "show-message-wrap",
        buttons: {
            "OK": function () {
                $(this).dialog("close");
                $('#form').attr("action", strActionName);
                $('#form').submit();
                return true;
            }
        }
    });
}


/********************************
 * エラーメッセージの表示
 *********************************/
function showErrMessage(strMessage, strTarget) {


    //エラー項目にセット
    $(strTarget).addClass("input-error");
    $(strTarget).focus();

    // ダイアログのメッセージを設定
    $("#show-message").html(strMessage);

    // ダイアログを作成
    $("#show-message").dialog({
        modal: true,
        title: strErrMessageTitle,
        dialogClass: "show-message-wrap",
        buttons: {
            "OK": function () {
                $(this).dialog("close");
                return true;
            }
        }
    });
}

/********************************
 * 結果メッセージの表示
 *********************************/
function showResultMessage(strMessage) {

    // ダイアログのメッセージを設定
    $("#show-message").html(strMessage);

    // ダイアログを作成
    $("#show-message").dialog({
        modal: true,
        title: strConfirmTitle,
        dialogClass: "show-confirm-wrap",
        buttons: {
            "OK": function () {
                $(this).dialog("close");
                return true;
            }
        }
    });
}

/********************************
 * メッセージを表示し、画面遷移する。    '2021/10/18 nv miyaji add
 *********************************/
function showMessageAndSeni(strMessage, strActionName) {

    // ダイアログのメッセージを設定
    $("#show-message").html(strMessage);

    // ダイアログを作成
    $("#show-message").dialog({
        modal: true,
        title: strConfirmTitle,
        dialogClass: "show-confirm-wrap",
        buttons: {
            "OK": function () {
                $(this).dialog("close");
                $('#form').attr("action", strActionName);
                $('#form').submit();
                return true;
            }
        }
    });
}

/********************************
 * メッセージを表示し、処理を実行する。
 *********************************/
function showMessageAndFunc(strMessage, callback) {

    // ダイアログのメッセージを設定
    $("#show-message").html(strMessage);

    // ダイアログを作成
    $("#show-message").dialog({
        modal: true,
        title: strConfirmTitle,
        dialogClass: "show-confirm-wrap",
        buttons: {
            "OK": function () {
                $(this).dialog("close");
                callback();
                return true;
            }
        }
    });
}

/********************************
 * 日付フォーマット変換
 *********************************/
function formatDate(date, format) {
    format = format.replace(/yyyy/g, date.getFullYear());
    format = format.replace(/MM/g, ('0' + (date.getMonth() + 1)).slice(-2));
    format = format.replace(/dd/g, ('0' + date.getDate()).slice(-2));
    format = format.replace(/HH/g, ('0' + date.getHours()).slice(-2));
    format = format.replace(/mm/g, ('0' + date.getMinutes()).slice(-2));
    format = format.replace(/ss/g, ('0' + date.getSeconds()).slice(-2));
    format = format.replace(/SSS/g, ('00' + date.getMilliseconds()).slice(-3));
    return format;
};

/********************************
 * ページ番号リセット
 *********************************/
function ResetPageNum() {
    if (document.getElementById("PageNum") != null) {
        document.getElementById("PageNum").value = 1;
    }
}

/********************************
 * ブラウザ戻りボタン不可制御設定
 *********************************/
history.pushState(null, null, null);
$(window).on("popstate", function (event) {
    if (!event.originalEvent.state) {
        history.pushState(null, null, null);
        return;
    }
});

/********************************
 * Enterキーでサブミットされるのを防ぐ
 *********************************/

$(function () {
    //$(document).on("keypress", "input:not(.allow_submit)", function (event) {
    //    return event.which !== 13;
    //});
    //document.onkeypress = enter; ★削除★

    //★追加START★
    $('#form').keypress(function (e) {
        if (!e) {
            return true;
        }
        if (this.parentElement.id == 'login') {
            // ログイン画面の場合、Enterキーでサブミットを有効とする
            return true;
        }
        var target = e.srcElement || e.target;
        if (e.keyCode != 13) {
            // Enterキー(13)以外の場合、押下したキーを有効とする
            return true;
        }
        if (!(target.type == 'button' || target.type == 'submit')) {
            // Enterキーで、対象がボタンかサブミット以外ならば、Enterキーを無効とする
            return false;
        }
        // Enterキー押下で対象がボタンならば、サブミット有効とする
        return true;
    });
    //★追加END★
});
/*function enter() {　★削除★
    if (window.event.keyCode == 13) {
        return false;
    }
}*/




/********************************
 * モーダルウィンドウ 
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowModalWindow() {

    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //if( $( "#modal-overlay" )[0] ) return false ;

    //オーバーレイを出現させる
    $("body").append('<div id="modal-overlay"></div>');
    $("#modal-overlay").show();

    //コンテンツをセンタリングする
    centeringModalSyncer();

    //コンテンツをフェードインする
    $("#modal-content").show();

    //[.modal-close]をクリック
    $(".modal-close").unbind().click(function () {

        //[#modal-content-create]と[#modal-overlay]をフェードアウトした後
        $("#modal-content, #modal-overlay").fadeOut("slow", function () {

            //[#modal-overlay]を削除する
            $('#modal-overlay').remove();

        });

    });

}

//センタリングを実行する関数
function centeringModalSyncer() {

    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();

    // コンテンツ(#modal-content)の幅、高さを取得
    var cw = $("#modal-content").outerWidth();
    var ch = $("#modal-content").outerHeight();

    //センタリングを実行する
    $("#modal-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });

}

/********************************
 * モーダルウィンドウ (CSV取込)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowCsvModalWindow() {

    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //if( $( "#modal-overlay" )[0] ) return false ;

    //オーバーレイを出現させる
    $("body").append('<div id="modal-overlay"></div>');
    $("#modal-overlay").show();

    //コンテンツをセンタリングする
    centeringCsvModalSyncer();

    //コンテンツをフェードインする
    $("#modal-csv-content").show();

    //[.modal-close]をクリック
    $(".modal-close").unbind().click(function () {

        //[#modal-csv-content-create]と[#modal-overlay]をフェードアウトした後
        $("#modal-csv-content, #modal-overlay").fadeOut("slow", function () {

            //[#modal-csv-overlay]を削除する
            $('#modal-overlay').remove();

        });

    });

}

//センタリングを実行する関数(CSV取込)
function centeringCsvModalSyncer() {

    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();

    // コンテンツ(#modal-csv-content)の幅、高さを取得
    var cw = $("#modal-csv-content").outerWidth();
    var ch = $("#modal-csv-content").outerHeight();

    //センタリングを実行する
    $("#modal-csv-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });

}

/********************************
 * モーダルウィンドウ (QR取込)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowQRModalWindow() {

    document.getElementById("qrview").src = document.getElementById("LoadingImg").value;

    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //if( $( "#modal-overlay" )[0] ) return false ;

    //オーバーレイを出現させる
    $("body").append('<div id="modal-overlay"></div>');
    $("#modal-overlay").show();

    //コンテンツをセンタリングする
    centeringQRModalSyncer();

    //コンテンツをフェードインする
    $("#modal-qr-content").show();

    //[.modal-close]をクリック
    $(".modal-close").unbind().click(function () {

        //[#modal-qr-content-create]と[#modal-overlay]をフェードアウトした後
        $("#modal-qr-content, #modal-overlay").fadeOut("slow", function () {

            //画像のパスをクリア
            document.getElementById("qrview").src = document.getElementById("LoadingImg").value;

            //[#modal-overlay]を削除する
            $('#modal-overlay').remove();

        });

    });

}

//センタリングを実行する関数(CSV取込)
function centeringQRModalSyncer() {

    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();

    // コンテンツ(#modal-csv-content)の幅、高さを取得
    var cw = $("#modal-qr-content").outerWidth();
    var ch = $("#modal-qr-content").outerHeight();

    //センタリングを実行する
    $("#modal-qr-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });

}

/********************************
 * モーダルウィンドウ(プリンタ選択)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowPRTModalWindow() {
    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //オーバーレイを出現させる
    $("body").append('<div id="modal-overlay"></div>');
    $("#modal-overlay").show();
    //コンテンツをセンタリングする
    centeringPRTModalSyncer();
    //コンテンツをフェードインする
    $("#modal-prt-content").show();
    //[.modal-close]をクリック
    $(".modal-close").unbind().click(function () {
        //[#modal-content-create]と[#modal-overlay]をフェードアウトした後
        $("#modal-prt-content, #modal-overlay").fadeOut("slow", function () {
            //[#modal-overlay]を削除する
            $('#modal-overlay').remove();
        });
    });
}

//センタリングを実行する関数(プリンタ選択)
function centeringPRTModalSyncer() {
    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();
    // コンテンツ(#modal-csv-content)の幅、高さを取得
    var cw = $("#modal-prt-content").outerWidth();
    var ch = $("#modal-prt-content").outerHeight();
    //センタリングを実行する
    $("#modal-prt-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
}

/********************************
 * モーダルウィンドウ(送り状I/F情報選択)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowInvoiceModalWindow() {
    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //オーバーレイを出現させる
    $("body").append('<div id="modal-overlay"></div>');
    $("#modal-overlay").show();
    //コンテンツをセンタリングする
    centeringInvoiceModalSyncer();
    //コンテンツをフェードインする
    $("#modal-invoice-content").show();
    //[.modal-close]をクリック
    $(".modal-close").unbind().click(function () {
        //[#modal-content-create]と[#modal-overlay]をフェードアウトした後
        $("#modal-invoice-content, #modal-overlay").fadeOut("slow", function () {
            //[#modal-overlay]を削除する
            $('#modal-overlay').remove();
        });
    });
}
//センタリングを実行する関数(送り状I/F情報選択)
function centeringInvoiceModalSyncer() {
    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();
    // コンテンツ(#modal-csv-content)の幅、高さを取得
    var cw = $("#modal-invoice-content").outerWidth();
    var ch = $("#modal-invoice-content").outerHeight();
    //センタリングを実行する
    $("#modal-invoice-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
}

/********************************
 * モーダルウィンドウ (カゴ)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowCartModalWindow() {

    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //if( $( "#modal-overlay" )[0] ) return false ;

    //オーバーレイを出現させる
    $("body").append('<div id="modal-overlay"></div>');
    $("#modal-overlay").show();

    //コンテンツをセンタリングする
    centeringCartModalSyncer();

    //コンテンツをフェードインする
    $("#modal-cart-content").show();

    //[.modal-close]をクリック
    $(".modal-close").unbind().click(function () {

        //[#modal-cart-content-create]と[#modal-overlay]をフェードアウトした後
        $("#modal-cart-content, #modal-overlay").fadeOut("slow", function () {

            //[#modal-cart-overlay]を削除する
            $('#modal-overlay').remove();

        });

    });

}
//センタリングを実行する関数(カゴ)
function centeringCartModalSyncer() {

    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();

    // コンテンツ(#modal-cart-content)の幅、高さを取得
    var cw = $("#modal-cart-content").outerWidth();
    var ch = $("#modal-cart-content").outerHeight();

    //センタリングを実行する
    $("#modal-cart-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });

}

/********************************
 * モーダルウィンドウ (予約)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowReserveModalWindow() {

    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //if( $( "#modal-overlay" )[0] ) return false ;

    //オーバーレイを出現させる
    $("body").append('<div id="modal-overlay"></div>');
    $("#modal-overlay").show();

    //コンテンツをセンタリングする
    centeringReserveModalSyncer();

    //コンテンツをフェードインする
    $("#modal-reserve-content").show();

    //[.modal-close]をクリック
    $(".modal-close").unbind().click(function () {

        //[#modal-reserve-content-create]と[#modal-overlay]をフェードアウトした後
        $("#modal-reserve-content, #modal-overlay").fadeOut("slow", function () {

            //[#modal-reserve-overlay]を削除する
            $('#modal-overlay').remove();

        });

    });

}
//センタリングを実行する関数(予約)
function centeringReserveModalSyncer() {

    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();

    // コンテンツ(#modal-reserve-content)の幅、高さを取得
    var cw = $("#modal-reserve-content").outerWidth();
    var ch = $("#modal-reserve-content").outerHeight();

    //センタリングを実行する
    $("#modal-reserve-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });

}

/********************************
 * モーダルウィンドウ (引継ぎ)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowHikitsugiModalWindow() {

    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //if( $( "#modal-overlay" )[0] ) return false ;

    //オーバーレイを出現させる
    $("body").append('<div id="modal-overlay"></div>');
    $("#modal-overlay").show();

    //コンテンツをセンタリングする
    centeringHikitsugiModalSyncer();

    //コンテンツをフェードインする
    $("#modal-hikitsugi-content").show();

    //[.modal-close]をクリック
    $(".modal-close").unbind().click(function () {

        //[#modal-hikitsugi-content-create]と[#modal-overlay]をフェードアウトした後
        $("#modal-hikitsugi-content, #modal-overlay").fadeOut("slow", function () {

            //[#modal-hikitsugi-overlay]を削除する
            $('#modal-overlay').remove();

        });

    });

}
//センタリングを実行する関数(引継ぎ)
function centeringHikitsugiModalSyncer() {

    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();

    // コンテンツ(#modal-hikitsugi-content)の幅、高さを取得
    var cw = $("#modal-hikitsugi-content").outerWidth();
    var ch = $("#modal-hikitsugi-content").outerHeight();

    //センタリングを実行する
    $("#modal-hikitsugi-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });

}

/********************************
 * モーダルウィンドウ (個別期間延長)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowKikanEnchoModalWindow() {

    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //if( $( "#modal-overlay" )[0] ) return false ;

    //オーバーレイを出現させる
    $("body").append('<div id="modal-overlay"></div>');
    $("#modal-overlay").show();

    //コンテンツをセンタリングする
    centeringKikanEnchoModalSyncer();

    //コンテンツをフェードインする
    $("#modal-kikanencho-content").show();

    //[.modal-close]をクリック
    $(".modal-close").unbind().click(function () {

        //[#modal-kikanencho-content-create]と[#modal-overlay]をフェードアウトした後
        $("#modal-kikanencho-content, #modal-overlay").fadeOut("slow", function () {

            //[#modal-kikanencho-overlay]を削除する
            $('#modal-overlay').remove();

        });

    });

}
//センタリングを実行する関数(個別期間延長)
function centeringKikanEnchoModalSyncer() {

    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();

    // コンテンツ(#modal-kikanencho-content)の幅、高さを取得
    var cw = $("#modal-kikanencho-content").outerWidth();
    var ch = $("#modal-kikanencho-content").outerHeight();

    //センタリングを実行する
    $("#modal-kikanencho-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });

}

// 2023/03/22 Kawamoto ADD START
/********************************
 * モーダルウィンドウ (添付ファイル管理)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowAttachmentModalWindow() {

    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す

    //オーバーレイを位置指定を変更（前面に移動）
    $("#modal-overlay").css({ "z-index": 2});

    //コンテンツをセンタリングする
    centeringAttachmentModalSyncer();

    //コンテンツをフェードインする
    $("#modal-attachment-content").show();

    //[.attachment-modal-close]をクリック
    $(".attachment-modal-close").unbind().click(function () {

        //[#modal-attachment-content]をフェードアウトした後
        $("#modal-attachment-content").fadeOut("slow", function () {

            //[#modal-overlay]の位置指定を変更（背面に移動）
            $("#modal-overlay").css({ "z-index": 1 });

        });

    });

}

//センタリングを実行する関数(添付ファイル管理)
function centeringAttachmentModalSyncer() {

    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();

    // コンテンツ(#modal-attachment-content)の幅、高さを取得
    var cw = $("#modal-attachment-content").outerWidth();
    var ch = $("#modal-attachment-content").outerHeight();

    //センタリングを実行する
    $("#modal-attachment-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });

}

/********************************
 * モーダルウィンドウ (ファイル選択)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowFiledropModalWindow() {

    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す

    //オーバーレイを位置指定を変更（前面に移動）
    $("#modal-overlay").css({ "z-index": 3 });

    //コンテンツをセンタリングする
    centeringFiledropModalSyncer();

    //コンテンツをフェードインする
    $("#modal-filedrop-content").show();

    //[#modal-overlay]をクリック
    $("#modal-overlay").unbind().click(function () {

        // モーダルウィンドウ(ファイル選択)が表示されている時
        if (!$('#modal-filedrop-content').is(':hidden')) {

            //[#modal-filedrop-content]をフェードアウトした後
            $("#modal-filedrop-content").fadeOut("slow", function () {

                //[#modal-overlay]の位置指定を変更（背面に移動）
                $("#modal-overlay").css({ "z-index": 2 });
            });
        }
    });
}

//センタリングを実行する関数(ファイル選択)
function centeringFiledropModalSyncer() {

    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();

    // コンテンツ(#modal-filedrop-content)の幅、高さを取得
    var cw = $("#modal-filedrop-content").outerWidth();
    var ch = $("#modal-filedrop-content").outerHeight();

    //センタリングを実行する
    $("#modal-filedrop-content").css({ "left": ((w - cw) / 2) + "px", "top": (((h - ch) / 2) + 50) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": (((h - ch) / 2) + 50) + "px" });

}
// 2023/03/22 Kawamoto ADD END

/********************************
 * モーダルウィンドウ (ロケQR読取)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowReadLocationModalWindow() {

    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す

    //オーバーレイを位置指定を変更（前面に移動）
    $("#modal-overlay").css({ "z-index": 2 });

    //コンテンツをセンタリングする
    centeringReadLocationModalSyncer();

    //コンテンツをフェードインする
    $("#modal-location-content").show();

    //ロケQRのデータをテキストボックスに入れ込むためにフォーカスをあてる
    document.getElementById('TxtLocationQr').focus();
    
    //[.location-modal-close]をクリック
    $(".location-modal-close").unbind().click(function () {

        //[#modal-location-content]をフェードアウトした後
        $("#modal-location-content").fadeOut("slow", function () {

            //[#modal-overlay]の位置指定を変更（背面に移動）
            $("#modal-overlay").css({ "z-index": 1 });

        });

    });

}

//センタリングを実行する関数(添付ファイル管理)
function centeringReadLocationModalSyncer() {

    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();

    // コンテンツ(#modal-location-content)の幅、高さを取得
    var cw = $("#modal-location-content").outerWidth();
    var ch = $("#modal-location-content").outerHeight();

    //センタリングを実行する
    $("#modal-location-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });

}

/********************************
 * モーダルウィンドウ(工事完了)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowConstCompleteModalWindow() {
    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //オーバーレイを出現させる
    $("body").append('<div id="modal-overlay"></div>');
    $("#modal-overlay").show();
    //コンテンツをセンタリングする
    centeringConstCompleteModalSyncer();
    //コンテンツをフェードインする
    $("#modal-const-complete-content").show();
    //[.modal-close]をクリック
    $(".modal-close").unbind().click(function () {
        //[#modal-content-create]と[#modal-overlay]をフェードアウトした後
        $("#modal-const-complete-content, #modal-overlay").fadeOut("slow", function () {
            //[#modal-overlay]を削除する
            $('#modal-overlay').remove();
        });
    });
}

//センタリングを実行する関数(プリンタ選択)
function centeringConstCompleteModalSyncer() {
    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();
    // コンテンツ(#modal-const-complete-content)の幅、高さを取得
    var cw = $("#modal-const-complete-content").outerWidth();
    var ch = $("#modal-const-complete-content").outerHeight();
    //センタリングを実行する
    $("#modal-const-complete-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
}

/********************************
 * モーダルウィンドウ(資産予約)
 *********************************/
//モーダルウィンドウを出現させるクリックイベント
function ShowAssetReserveModalWindow() {
    //キーボード操作などにより、オーバーレイが多重起動するのを防止する
    $(this).blur();	//ボタンからフォーカスを外す
    //オーバーレイを位置指定を変更（前面に移動）
    $("#modal-overlay").css({ "z-index": 2 });
    //コンテンツをセンタリングする
    centeringAssetReserveModalSyncer();
    //コンテンツをフェードインする
    $("#modal-asset-reserve-content").show();
    //[.asset-reserve-modal-close]をクリック
    $(".asset-reserve-modal-close").unbind().click(function () {
        //[#modal-asset-reserve-content]をフェードアウトした後
        $("#modal-asset-reserve-content").fadeOut("slow", function () {
            //[#modal-overlay]の位置指定を変更（背面に移動）
            $("#modal-overlay").css({ "z-index": 1 });
        });
    });
}

//センタリングを実行する関数(資産予約)
function centeringAssetReserveModalSyncer() {
    //画面(ウィンドウ)の幅、高さを取得
    var w = $(window).width();
    var h = $(window).height();
    // コンテンツ(#modal-asset-reserve-content)の幅、高さを取得
    var cw = $("#modal-asset-reserve-content").outerWidth();
    var ch = $("#modal-asset-reserve-content").outerHeight();
    //センタリングを実行する
    $("#modal-asset-reserve-content").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
    $(".modal-dialog").css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px" });
}

$(function () {

    /********************************
    * カレンダー表示
    *********************************/
    $(".datepicker").datepicker();
    $.datepicker.setDefaults({
        showOn: "both",
        buttonImageOnly: true,
        buttonText: "Calendar",
        dateFormat: 'yy/mm/dd'
    });



    //リサイズされたら、センタリングをする関数[centeringModalSyncer()]を実行する
    $(window).resize(centeringModalSyncer);

    /********************************
     * ページング設定
     *********************************/
    //ページング番号のhidden項目
    $(".page-input-btn").click(function () {
        $('input:hidden[name="PageNum"]').val($(this).attr('name'))
    });

    ///********************************
    // * 連続送信防止設定
    // *********************************/
    //var clickFlg = true;
    ////ボタンクリックイベント
    //$("#form").submit(function () {
    //    if (clickFlg) {
    //        clickFlg = false;
    //    } else {
    //        return false;
    //    }
    //});

    /********************************
     * DatePicker設定
     *********************************/
    $(function () {
        $('.datepicker').datepicker({
            autoclose: 'true',
            language: 'en',
            todayHighlight: true
        });
    })

});

/*******************************************
 * エディットボックス設定　半角英数のみ入力可
 ********************************************/
function editHanEi($this) {
    var str = $this.value;
    while (str.match(/[^A-Za-z\d]/)) {
        str = str.replace(/[^A-Za-z\d]/, "");
    }
    $this.value = str;
}

//2022/08/31 Add Strat Kawamoto v2.2
/*******************************************
 * エディットボックス設定　資産番号 半角英数字記号(#$*+-.:;=<>_)のみ入力可
 ********************************************/
function editAssetNo($this) {
    var str = $this.value;
    while (str.match(/[^A-Za-z\d#\$\*\+\-\.:;=<>_]/)) {
        str = str.replace(/[^A-Za-z\d#\$\*\+\-\.:;=<>_]/, "");
    }
    $this.value = str;
}
//2022/08/31 Add End Kawamoto v2.2

/*******************************************
 * エディットボックス設定　資産番号（複数入力）
 *   半角英数字記号(#$*+-.:;=<>_)と、複数入力の区切り文字「|」のみ入力可
 *   ※editAssetNo は「|」を除去するため、複数入力テキストボックス（Scripts\multi-input-dialog.js）では本処理を使用する
 *   例）@Html.TextBoxFor(Function(m) m.SearchAssetNo, New With {.class = "cond-control", .oninput = "editAssetNoMultiInput(this)"})
 ********************************************/
function editAssetNoMultiInput($this) {
    $this.value = $this.value.replace(/[^A-Za-z\d#\$\*\+\-\.:;=<>_|]/g, "");
}

/*******************************************
 * エディットボックス設定　半角英数記号のみ入力可
 ********************************************/
function editPassword($this) {
    var str = $this.value;
    //--2020/03/14 nv miyaji update start
    //--2020/07/13 tomatsuri update start
    while (str.match(/[^a-zA-Z\d!-/:-@\[-`{-~]+$/)) {
        str = str.replace(/[^a-zA-Z\d!-/:-@\[-`{-~]+$/, "");
    }
    //while (str.match(/[^a-zA-Z\d!#$%&()*+,.:;=?@\[\]^_{}-]/)) {
    //    str = str.replace(/[^a-zA-Z\d!#$%&()*+,.:;=?@\[\]^_{}-]/, "");
    //}
    //--2020/07/13 tomatsuri update end
    //while (str.match(/[^a-zA-Z\d!#$%&()*+,.:;=?@\[\]^_{}-]+$/)) {
    //    str = str.replace(/[^a-zA-Z\d!#$%&()*+,.:;=?@\[\]^_{}-]+$/, "");
    //}
    //--2020/03/14 nv miyaji update end
    $this.value = str;
}

/*******************************************
 * エディットボックス設定　半角数字のみ入力可
 ********************************************/
function editNumber($this) {
    var str = $this.value;
    //--2020/03/14 nv miyaji update start
    while (str.match(/[^0-9]/)) {
        str = str.replace(/[^0-9]/, "");
    }
    //while (str.match(/[^0-9]+$/)) {
    //    str = str.replace(/[^0-9]+$/, "");
    //}
    //--2020/03/14 nv miyaji update end
    $this.value = str;
}

/*******************************************
 * エディットボックス設定　半角数字+"-"のみ入力可  '2021/08/17 nv miyaji add
 ********************************************/
function editNumberKigou($this) {
    var str = $this.value;
    while (str.match(/[^0-9-]/)) {
        str = str.replace(/[^0-9-]/, "");
    }
    $this.value = str;
}

/*******************************************
 * エディットボックス設定　半角英数字+"-"のみ入力可
 ********************************************/
function editNumberAlphabetHyphen($this) {
    var str = $this.value;
    while (str.match(/[^0-9- ^a-zA-Z]/)) {
        str = str.replace(/[^0-9- ^a-zA-Z]/, "");
    }
    $this.value = str;
}


/*******************************************
 * エディットボックス設定　プロダクトキー入力画面　半角英数+=/のみ入力可
 ********************************************/
function editProductKey($this) {
    var str = $this.value;
    while (str.match(/[^a-z0-9A-Z/\+=]/)) {
        str = str.replace(/[^a-z0-9A-Z/\+=]/, "");
    }
    $this.value = str;
}

//20220328 ADD Strat Kawamoto v2.1
/*******************************************
 * エディットボックス設定　ゲートマスタメンテ入力画面　数字-のみ入力可
 ********************************************/
function editGateSensitivity($this) {
    var str = $this.value;
    while (str.match(/[^0-9/-]/)) {
        str = str.replace(/[^0-9/-]/, "");
    }
    $this.value = str;
}
//20220328 ADD End Kawamoto v2.1

/*******************************************
 * CSV取込画面のファイル選択をクリアする
 ********************************************/
function fileNmClear() {
    document.getElementById("CsvFile").value = "";
}

//2023/02/09 ADD Start Shirakawa
/*******************************************
 * エディットボックス設定　半角数字のみ入力可
 ********************************************/
function editNumberDicimal($this) {
    var str = $this.value;
    while (str.match(/[^0-9/.]/)) {
        str = str.replace(/[^0-9/.]/, "");
    }

    $this.value = str;
}
//2023/02/09 ADD End Shirakawa

//2023/04/30 ADD START SATO 
/*******************************************
 * .NETのString.formatのように{0}等を任意の文字に置換する
 ********************************************/
String.prototype.format = function () {
    let formatted = this
    for (let arg in arguments) {
        formatted = formatted.replace("{" + arg + "}", arguments[arg]);   
    }
    return formatted;
};
//2023/04/30 ADD END SATO 

/*******************************************
 * yyyyMMdd→yyyy/MM/ddに変換
 ********************************************/
function toStringDateYMD(dateYYYYMMDD) {
    if (!dateYYYYMMDD) {
        //null、undefined、空白の文字列の場合は空白で返す
        return '';
    }

    let date;
    if (!(dateYYYYMMDD instanceof Date)) {
        // 日付型でなければ日付に変換
        date = parseDateyyyyMMdd(String(dateYYYYMMDD));
    }

    let yyyy = date.getFullYear().toString();
    let mm = (date.getMonth() + 1).toString().padStart(2, '0'); // 月は0から始まる
    let dd = date.getDate().toString().padStart(2, '0');
    return `${yyyy}/${mm}/${dd}`;
}

/*******************************************
 * 空の値を空白文字列に変換
 ********************************************/
function convertToEmptyIfNull(value) {
    // 値が null または undefined の場合、空白文字列を返す
    if (value === null || value === undefined) {
        return "";
    }

    // それ以外の場合は、値をそのまま返す
    return value;
}

/*******************************************
 * yyyyMMdd文字列を日付型に変換
 ********************************************/
function parseDateyyyyMMdd(dateStr) {
    if (dateStr.length !== 8 || isNaN(dateStr)) {
        return null; // 不正なフォーマットまたは数字でない場合はnullを返す
    }

    var year = parseInt(dateStr.substring(0, 4), 10);
    var month = parseInt(dateStr.substring(4, 6), 10) - 1; // JavaScriptの月は0から始まる
    var day = parseInt(dateStr.substring(6, 8), 10);

    var date = new Date(year, month, day);
    return date;
}

/*******************************************
 * 文字列日付を任意のフォーマットで文字列に変換
 ********************************************/
function toFormatDate(dateStr, format) {
    var date = new Date(dateStr);

    // 日付が無効かどうかをチェック
    if (isNaN(date.getTime())) {
        // 無効な日付の場合は元の文字列を返す
        return dateStr;
    }

    var year = date.getFullYear();
    var month = ('0' + (date.getMonth() + 1)).slice(-2);
    var day = ('0' + date.getDate()).slice(-2);
    var hour = ('0' + date.getHours()).slice(-2);
    var minute = ('0' + date.getMinutes()).slice(-2);
    var second = ('0' + date.getSeconds()).slice(-2);

    let d = format.replace(/yyyy/g, year)
                     .replace(/MM/g, month)
                     .replace(/dd/g, day)
                     .replace(/HH/g, hour)
                     .replace(/mm/g, minute)
                     .replace(/ss/g, second);

    // フォーマット文字列に基づいて日付をフォーマット
    return format.replace(/yyyy/g, year)
                 .replace(/MM/g, month)
                 .replace(/dd/g, day)
                 .replace(/HH/g, hour)
                 .replace(/mm/g, minute)
                 .replace(/ss/g, second);
}

/*******************************************
 * 日付をフォーマットした文字列を返却(時分込み)。
 ********************************************/
function toStringYMDHM(target) {
    let sTarget = target.toString();
    let formattedDate = `${sTarget.substring(0, 4)}/${sTarget.substring(4, 6)}/${sTarget.substring(6, 8)} ${sTarget.substring(8, 10)}:${sTarget.substring(10, 12)}`;
    return formattedDate;
}

/*******************************************
 * 指定した小数点以下有効桁数になるよう四捨五入します。
 * @param {number} dValue - 切り上げ対象の数値
 * @param {number} iDigits - 小数点以下有効桁数
 * @returns {number}
 ********************************************/
function toRound(dValue, iDigits) {
    // 対象の数値が負数かどうかを取得します。
    const isMinus = dValue < 0;

    // 対象の数値の絶対値を取得します。
    dValue = Math.abs(dValue);

    // 10 の iDigits乗を取得します。
    const dCoef = Math.pow(10, iDigits);

    // 四捨五入処理を行います。
    dValue = Math.floor(dValue * dCoef + 0.5) / dCoef;

    // 負数の場合は絶対値をマイナスに戻します。
    if (isMinus) {
        dValue = dValue * -1;
    }

    return dValue;
}

function DisableButton(buttonId) {
    $("#" + buttonId).prop("disabled", true);
    $("#" + buttonId).attr('style', 'background-color:#cecece;');
    $("#" + buttonId).css("cursor", "default");
}


/*******************************************
 * 絞込プルダウン
 *
 * ※class="select2-clearable" が付いている場合は、×ボタン付きSelect2の共通設定
 *   （Scripts\select2-common.js）で適用するため、ここでは対象外とする
 ********************************************/
$(document).ready(function () {
    $(".filter-combo-area select").not(".select2-clearable").select2({
        language: 'ja',
        allowClear: false,
        width: "100%"
    });
    $(".filter-combo").not(".select2-clearable").select2({
        language: 'ja',
        allowClear: false
    });
});

/*******************************************
 * 全画面ローディング
 ********************************************/
function showLoading() {
    $("#overlay").show();
    $("body").css("cursor", "wait");
}

function hideLoading() {
    setTimeout(function () {
        $("#overlay").hide();
        $("body").css("cursor", "default");
    }, 10);
}