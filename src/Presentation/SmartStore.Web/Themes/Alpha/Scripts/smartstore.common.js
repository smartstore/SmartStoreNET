(function ($, window, document, undefined) {

	window.setLocation = function (url) {
		window.location.href = url;
	}

	window.openPopup = function (url, fluid) {
		var modal = $('#modal-popup-shared');

		if (modal.length === 0) {
			// TODO: (mc) Update to BS4 modal html later
			var html =
				'<div id="modal-popup-shared" class="modal modal-flex {0} fade" tabindex="-1" style="border-radius: 0">'.format(!!(fluid) ? 'modal-fluid' : 'modal-xlarge')
					+ '<div class="modal-body" style="padding: 0">'
						+ '<iframe class="modal-flex-fill-area" frameborder="0" src="' + url + '" />'
					+ '</div>'
					+ '<div class="modal-footer">'
						+ '<button type="button" class="btn btn-secondary btn-default" data-dismiss="modal">' + window.Res['Common.Close'] + '</button>'
					+ '</div>'
				+ '</div>';

			modal = $(html).appendTo('body').on('hidden.bs.modal', function (e) {
				//modal.remove();
			});
		}
		else {
			var iframe = modal.find('> .modal-body > iframe');
			iframe.attr('src', url);
		}

		modal.modal('show');
	}

	window.closePopup = function () {
		var modal = $('#modal-popup-shared');
		if (modal.length > 0) {
			modal.modal('hide');
		}
	}

	window.openWindow = function (url, w, h, scroll) {
		var l = (screen.width - w) / 2;
		var t = (screen.height - h) / 2;

		// TODO: (MC) temp only. Global viewport is larger now.
		// But add this value to the callers later.
		h += 100;

		winprops = 'resizable=0, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
		if (scroll) winprops += ',scrollbars=1';
		var f = window.open(url, "_blank", winprops);
	}

	window.modifyUrl = function (url, qsName, qsValue) {
		var search = null;

		if (!url) {
			url = window.location.protocol + "//" +
				window.location.host +
				window.location.pathname;
		}
		else {
			// strip query from url
			var idx = url.indexOf('?', 0);
			if (idx > -1) {
				search = url.substring(idx);
				url = url.substring(0, idx);
			}
		}

		var qs = getQueryStrings(search);

		// Add new params to the querystring dictionary
		qs[qsName] = qsValue;

		return url + createQueryString(qs);

		// http://stackoverflow.com/questions/2907482
		// Gets Querystring from window.location and converts all keys to lowercase
		function getQueryStrings(search) {
			var assoc = { };
			var decode = function (s) { return decodeURIComponent(s.replace(/\+/g, " ")); };
			var queryString = (search || location.search).substring(1);
			var keyValues = queryString.split('&');

			for (var i in keyValues) {
				var key = keyValues[i].split('=');
				if (key.length > 1)
					assoc[decode(key[0]).toLowerCase()] = decode(key[1]);
			}

			return assoc;
		}

		function createQueryString(dict) {
			var bits = [];
			for (var key in dict) {
				if (dict.hasOwnProperty(key) && dict[key]) {
					bits.push(key + "=" + dict[key]);
				}
			}
			return bits.length > 0 ? "?" + bits.join("&") : "";
		}
	}

	window.htmlEncode = function (value) {
		return $('<div/>').text(value).html();
	}

	window.htmlDecode = function (value) {
		return $('<div/>').html(value).text();
	}

	window.displayNotification = function (message, type, sticky, delay) {
		if (window.EventBroker === undefined || window._ === undefined)
			return;

		var notify = function (msg) {

		    if (!msg)
		        return;

			EventBroker.publish("message", {
				text: msg,
				type: type,
				delay: delay || (type == "success" ? 2000 : 5000),
				hide: !sticky
			})
		};

		if (_.isArray(message)) {
			$.each(message, function (i, val) {
				notify(val)
			});
		}
		else {
			notify(message);
		}
	}

	window.Prefixer = (function () {
		var TransitionEndEvent = {
			WebkitTransition: 'webkitTransitionEnd',
			MozTransition: 'transitionend',
			OTransition: 'oTransitionEnd otransitionend',
			transition: 'transitionend'
		};

		var AnimationEndEvent = {
			WebkitAnimation: 'webkitAnimationEnd',
			MozAnimation: 'animationend',
			OAnimation: 'webkitAnimationEnd oAnimationEnd',
			animation: 'animationend'
		};

		var cssProps = {},
			cssValues = {},
			domProps = {};

		function prefixCss(prop) {
			return cssProps[prop] || (cssProps[prop] = Modernizr.prefixedCSS(prop));
		}
		
		function prefixCssValue(prop, value) {
			var key = prop + '.' + value;
			return cssValues[key] || (cssValues[key] = Modernizr.prefixedCSSValue(prop, value));
		}

		function prefixDom(prop) {
			return domProps[prop] || (domProps[prop] = Modernizr.prefixed(prop));
		}

		return {
			css: prefixCss,
			cssValue: prefixCssValue,
			dom: prefixDom,
			event: {
				transitionEnd: TransitionEndEvent[prefixDom('transition')],
				animationEnd: AnimationEndEvent[prefixDom('animation')]
			}
		}
	})();

	window.createCircularSpinner = function (size, active, strokeWidth, boxed, white) {
	    var spinner = $('<div class="spinner"></div>');
	    if (active) spinner.addClass('active');
	    if (boxed) spinner.addClass('spinner-boxed').css('font-size', size + 'px');
	    if (white) spinner.addClass('white');
	    
	    if (!_.isNumber(strokeWidth)) {
	        strokeWidth = 6;
	    }

	    var svg = '<svg style="width:{0}px; height:{0}px" viewBox="0 0 64 64"><circle cx="32" cy="32" r="{1}" fill="none" stroke-width="{2}" stroke-miterlimit="10"></circle></svg>'.format(size, 32 - strokeWidth, strokeWidth);
	    spinner.append($(svg));

	    return spinner;
	}

    // on document ready
	$(function () {

		function getFunction(code, argNames) {
			var fn = window, parts = (code || "").split(".");
			while (fn && parts.length) {
				fn = fn[parts.shift()];
			}
			if (typeof (fn) === "function") {
				return fn;
			}
			argNames.push(code);
			return Function.constructor.apply(null, argNames);
		}

		function decode(str) {
			try {
				if (str)
					return decodeURIComponent(escape(str));
			}
			catch (e) {
				return str;
			}
			return str;
		}

		// Adjust initPNotify global defaults
		if (typeof PNotify !== 'undefined') {
			PNotify.prototype.options = $.extend(PNotify.prototype.options, {
				styling: "fontawesome",
				stack: { "dir1": "down", "dir2": "left", "push": "bottom", "firstpos1": 80, "spacing1": 25, "spacing2": 25, "context": $("body") },
				addclass: 'stack-topright',
				//stack: { "dir1": "up", "dir2": "right", "firstpos1": 100, "spacing1": 20 }, // SMNET style
				//addclass: 'stack-bottomcenter x-ui-pnotify-dark', // SMNET style
				width: "450px",
				mobile: { swipe_dismiss: true, styling: true },
				//animation: 'none',
				animate: { animate: true, in_class: "fadeInDown", out_class: "fadeOutRight" }
			});
		}

		// Global notification subscriber
		if (window.EventBroker && window._ && typeof PNotify !== 'undefined') {
			//var stack_bottomright = { "dir1": "up", "dir2": "left", "firstpos1": 25, "firstpos2": 25 };
			var stack_bottomcenter = { "dir1": "up", "dir2": "right", "firstpos1": 100, "firstpos2": 10 };
			EventBroker.subscribe("message", function (message, data) {
				var opts = _.isString(data) ? { text: data } : data;
				new PNotify(opts);
			});
		}

		// tab strip smart auto selection
		$('.tabs-autoselect > ul.nav a[data-toggle=tab]').on('shown', function(e) {
			var tab = $(e.target),
				strip = tab.closest('.tabbable'),
				href = strip.data("tabselector-href"),
				hash = tab.attr("href");

			if (hash)
				hash = hash.replace(/#/, "");

			if (href) {
				$.ajax({
					type: "POST",
					url: href,
					async: true,
					data: { navId: strip.attr('id'), tabId: hash, path: location.pathname + location.search },
					global: false
				});
			}		
		});

		// Telerik grid smart AJAX state preserving
		$('.t-grid.grid-preservestate').on('dataBound', function (e) {
			var grid = $(this).data("tGrid"),
				href = $(this).data("statepreserver-href"),
				gridId = $(this).data("statepreserver-key");

			if (href) {
				$.ajax({
					type: "POST",
					url: href,
					async: true,
					data: {
						gridId: gridId,
						path: location.pathname + location.search,
						page: grid.currentPage,
						size: grid.pageSize,
						orderBy: grid.orderBy,
						groupBy: grid.groupBy,
						filter: grid.filterBy
					},
					global: false
				});
			}
		});

		// AJAX tabs
		$('.nav a[data-ajax-url]').on('show', function (e) {
			var newTab = $(e.target),
				tabbable = newTab.closest('.tabbable'),
				pane = tabbable.find(newTab.attr("href")),
				url = newTab.data('ajax-url');

			if (newTab.data("loaded") || !url)
				return;

			$.ajax({
				cache: false,
				type: "GET",
				async: false,
				global: false,
				url: url,
				beforeSend: function (xhr) {
					getFunction(tabbable.data("ajax-onbegin"), ["tab", "pane", "xhr"]).apply(this, [newTab, pane, xhr]);
				},
				success: function (data, status, xhr) {
					pane.html(data);
					getFunction(tabbable.data("ajax-onsuccess"), ["tab", "pane", "data", "status", "xhr"]).apply(this, [newTab, pane, data, status, xhr]);
				},
				error: function (xhr, ajaxOptions, thrownError) {
					pane.html('<div class="text-error">Error while loading resource: ' + thrownError + '</div>');
					getFunction(tabbable.data("ajax-onfailure"), ["tab", "pane", "xhr", "ajaxOptions", "thrownError"]).apply(this, [newTab, pane, xhr, ajaxOptions, thrownError]);
				},
				complete: function (xhr, status) {
					newTab.data("loaded", true);
					var tabName = newTab.data('tab-name') || newTab.attr("href").replace(/#/, "");
					tabbable.append('<input type="hidden" class="loaded-tab-name" name="LoadedTabs" value="' + tabName + '" />');

					getFunction(tabbable.data("ajax-oncomplete"), ["tab", "pane", "xhr", "status"]).apply(this, [newTab, pane, xhr, status]);
				}
			});
		});

		// Handle ajax notifications
		$(document)
			.ajaxSuccess(function (ev, xhr) {
				var msg = xhr.getResponseHeader('X-Message');
				if (msg) {
					displayNotification(decode(msg), xhr.getResponseHeader('X-Message-Type'));
				}
			})
			.ajaxError(function (ev, xhr) {
				var msg = xhr.getResponseHeader('X-Message');
				if (msg) {
					displayNotification(decode(msg), xhr.getResponseHeader('X-Message-Type'));
				}
				else {
					try {
						var data = JSON.parse(xhr.responseText);
						if (data.error && data.message) {
							displayNotification(decode(data.message), "error");
						}
					}
					catch (ex) {
						displayNotification(xhr.responseText, "error");
					}
				}
			}
		);

		// .mf-dropdown (mobile friendly dropdown)
		$('body').on('mouseenter mouseleave mousedown change', '.mf-dropdown > select', function (e) {
			var btn = $(this).parent().find('> .btn');
			if (e.type == "mouseenter") {
				btn.addClass('focus');
			}
			else if (e.type == "mousedown") {
				btn.addClass('active').removeClass('focus');
				_.delay(function () {
					$('body').one('mousedown touch', function (e) { btn.removeClass('active'); });
				}, 50);
			}
			else if (e.type == "mouseleave") {
				btn.removeClass('focus');
			}
			else if (e.type == "change") {
				btn.removeClass('focus active');
				btn.find('.mf-dropdown-value').text($(this).val());
			}
		});

		// html text collapser
		if ($.fn.moreLess) {
			$('.more-less').moreLess();
		}
		
		// fixes bootstrap 2 bug: non functional links on mobile devices
		// TODO: (mc) delete this later
	    // https://github.com/twbs/bootstrap/issues/4550
		$('body').on('touchstart.dropdown', '.dropdown-menu a', function (e) { e.stopPropagation(); });
    });

})( jQuery, this, document );

