var fn = {};
fn.cookie = {
	get : function(key) {
		var arr = document.cookie.match(new RegExp("(^| )" + key
				+ "=([^;]*)(;|$)"));
		if (arr != null)
			return unescape(arr[2]);
		return null;
	},
	set : function(key, value) {
		var Days = 30; // 此 cookie 将被保存 30 天
		var exp = new Date(); // new Date("December 31, 9998");
		exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
		document.cookie = key + "=" + escape(value) + ";expires="
				+ exp.toGMTString();
	}
};
fn.bind = function(eventName, fun, obj) {
	if (obj == null || obj == undefined) {
		obj = window;
	}
	if (obj.attachEvent) {
		obj.attachEvent("on" + eventName, fun);
	} else if (obj.addEventListener) {
		obj.addEventListener(eventName, fun, false);
	}
};
