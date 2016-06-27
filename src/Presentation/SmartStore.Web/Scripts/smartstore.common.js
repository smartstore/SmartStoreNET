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

	window.copyTextToClipboard = function (text) {
		var result = false;

		if (window.clipboardData && window.clipboardData.setData) {
			result = clipboardData.setData('Text', text);
		}
		else if (document.queryCommandSupported && document.queryCommandSupported('copy')) {
			var textarea = document.createElement('textarea'),
				focusElement = document.activeElement;

			textarea.textContent = text;
			textarea.style.position = 'fixed';
			document.body.appendChild(textarea);
			textarea.focus();
			textarea.setSelectionRange(0, textarea.value.length);

			try {
				result = document.execCommand('copy');
			}
			catch (e) {
			}
			finally {
				document.body.removeChild(textarea);
				if (focusElement) {
					focusElement.focus();
				}
			}
		}
		return result;
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

		// html text collapser
		if ($({}).moreLess) {
			$('.more-less').moreLess();
		}
		
		// fixes bootstrap 2 bug: non functional links on mobile devices
	    // https://github.com/twbs/bootstrap/issues/4550
		$('body').on('touchstart.dropdown', '.dropdown-menu a', function (e) { e.stopPropagation(); });
    });

})( jQuery, this, document );

