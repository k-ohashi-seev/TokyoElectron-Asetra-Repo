/* =============================================================================
	jQuery Dropdown ver1.1.7
	Copyright(c) 2015, ShanaBrian
	Dual licensed under the MIT and GPL licenses.
============================================================================= */
(function($) {
	$.fn.dropdown = function(settings) {
		/*
			action           : マウスの動作 [ hover | click ]
			animate          : アニメーションの種類 [ direct | slide ]
			animateDirection : アニメーションの方向 [ y | x ]
			animateDuration  : アニメーションの動作時間 [ ミリ秒 ]
			animateEasing    : アニメーションのイージング
			actionElement    : actionを適用する要素
			targetElement    : 開閉する要素
			textElement      : targetElementが存在する文字要素
			showClass        : 展開時targetElementに付加するClass名
			textClass        : targetElementが存在するtextElementに付加するClass名
			textOpenClass    : targetElementが展開状態のtextElementに付加するClass名
		*/
		settings = $.extend({
			action           : 'hover',
			animate          : 'slide',
			animateDirection : 'y',
			animateDuration  : 100,
			animateEasing    : 'linear',
			actionElement    : 'li',
			targetElement    : 'ul',
			textElement      : 'a',
			showClass        : '',
			textClass        : '',
			textOpenClass    : ''
		}, settings);

		var $ddElmt = $(this).find(settings.actionElement + ':has(' + settings.targetElement + ')'),
			startOp = { opacity : 0 };

		var changeAction = function(mode, $target, aspect) {
			var $base = null,
				$te   = null,
				op    = {},
				num   = 0;

			if (settings.showClass !== '') {
				$base = $target.parent();
				if (mode === 'show') {
					$base.addClass(settings.showClass);
				} else {
					$base.removeClass(settings.showClass);
				}
			}

			if (settings.textOpenClass !== '') {
				$te = $target.parent().find('> ' + settings.textElement);
				if (mode === 'show') {
					$te.addClass(settings.textOpenClass);
				} else {
					$te.removeClass(settings.textOpenClass);
				}
			}
			if (settings.animate === 'direct') {
				if (mode === 'show') {
					$target.show();
				} else {
					$target.hide();
				}
			} else if (settings.animate === 'slide') {
				op  = { opacity : 0 };
				num = 0;

				if (mode === 'show') {
					op.opacity = 1;
					num = aspect;
				}
				if (settings.animateDirection === 'y') {
					op.height = num;
				} else if (settings.animateDirection === 'x') {
					op.width = num;
				}

				$target.show().stop().animate(
					op,
					{
						duration : settings.animateDuration,
						easing   : settings.animateEasing,
						complete : function() {
							if (!$.support.opacity) {
								this.style.removeAttribute('filter');
							}
							if (mode === 'hide') {
								$(this).find('> ' + settings.targetElement).not(':hidden').animate(startOp, 0).hide();
								$(this).hide();
							} else {
								$(this).css('overflow', 'visible');
							}
						}
					}
				);
			}
		};

		$.each($ddElmt, function() {
			var aspect = 0,
				target = $('> ' + settings.targetElement, this);

			if (settings.animateDirection === 'y') {
				aspect         = target.height();
				startOp.height = 0;
			} else if (settings.animateDirection === 'x') {
				aspect        = target.width();
				startOp.width = 0;
			}

			if (settings.action === 'hover') {
				$(this).on({
					'mouseenter' : function() {
						changeAction('show', target, aspect);
					},
					'mouseleave' : function() {
						changeAction('hide', target, aspect);
					}
				});
			} else if (settings.action === 'click') {
				$(this).on('click', function() {
					if ($target.is(':hidden')) {
						changeAction('hide', $(this).siblings().find(settings.targetElement + ':not(:hidden)'), aspect);
						changeAction('show', target, aspect);
					} else {
						changeAction('hide', $(this).find(settings.targetElement), aspect);
					}
					return false;
				});
			}

			if (settings.textClass !== '') {
				$(this).find('> ' + settings.textElement).addClass(settings.textClass);
			}
		});

		if(settings.animate === 'direct') {
			$ddElmt.find(settings.targetElement).hide();
		} else if(settings.animate === 'slide') {
			$ddElmt.find(settings.targetElement).animate(startOp, 0, 'linear', function(){$(this).hide()});
		}

		return this;
	};
})(jQuery);
