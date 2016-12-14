(function ($, window, document, undefined) {

	window.setLocation = function (url) {
		window.location.href = url;
	}

	window.OpenWindow = function (query, w, h, scroll) {
		var l = (screen.width - w) / 2;
		var t = (screen.height - h) / 2;

		// TODO: (MC) temp only. Global viewport is larger now.
		// But add this value to the callers later.
		h += 100;

		winprops = 'resizable=0, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
		if (scroll) winprops += ',scrollbars=1';
		var f = window.open(query, "_blank", winprops);
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

		if (!Modernizr.csstransitions) {
			$.fn.transition = $.fn.animate;
		}

		// adjust pnotify global defaults
		if ($.pnotify) {
			$.extend($.pnotify.defaults, {
				history: false,
				animate_speed: "normal",
				shadow: true,
				width: "400px",
				icon: true
			});
		}

		// global notification subscriber
		if (window.EventBroker && window._ && $.pnotify) {
			//var stack_bottomright = { "dir1": "up", "dir2": "left", "firstpos1": 25, "firstpos2": 25 };
			var stack_bottomcenter = { "dir1": "up", "dir2": "right", "firstpos1": 100, "firstpos2": 10 };
			EventBroker.subscribe("message", function (message, data) {
				var opts = _.isString(data) ? { text: data } : data;
				opts.stack = stack_bottomcenter;
				opts.addclass = "stack-bottomcenter";
				$.pnotify(opts);
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

		// .mf-dropdown (mobile friendly dropdown
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
			}
		});

		// html text collapser
		if ($.fn.moreLess) {
			$('.more-less').moreLess();
		}
		
		// fixes bootstrap 2 bug: non functional links on mobile devices
	    // https://github.com/twbs/bootstrap/issues/4550
		$('body').on('touchstart.dropdown', '.dropdown-menu a', function (e) { e.stopPropagation(); });
    });

})( jQuery, this, document );

